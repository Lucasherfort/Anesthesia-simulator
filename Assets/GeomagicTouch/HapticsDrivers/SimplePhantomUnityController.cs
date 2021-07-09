/**
 * ------------------------------------------------
 * ManagedPhantom
 * 
 * Simple PHANToM for Unity
 * 
 * Copyright (c) 2014 Kirurobo
 * http://twitter.com/kirurobo
 * 
 * This software is released under the MIT License.
 * http://opensource.org/licenses/mit-license.php
 * 
 * 
 * REQUIREMENTS
 *  - PHANTOM haptic device
 *  - PHANTOM Device Drivers
 *  - hd.dll (Sensable OpenHaptics Toolkit)
 * 
 * ------------------------------------------------
 */

using UnityEngine;
using System;
using System.Collections.Generic;
using ManagedPhantom;

/// <summary>
/// Unityで手軽にPHANTOMを利用するためのクラス
/// Class to easily use the PHANTOM in Unity
/// </summary>
/// <description>
/// 単位はPHANTOMに従い [mm] [mm/s] [N] 等です。
/// The units are [mm] [mm / s] [N] in accordance with PHANTOM.
/// ただしUnityに合わせZ軸を反転させています。
/// However, we have to invert the Z-axis according to Unity.
/// </description>
public class SimplePhantomUnityController
{
    uint hHD = (uint)Hd.DeviceHandle.HD_INVALID_HANDLE;     // デバイスハンドル - Device handle
    List<Hd.SchedulerCallback> CallbackMethods;             // 参照が無くなるとGCされるので、メソッドを保持 - holds the methods when GC lost reference
    private List<ulong> ScheduleHandles;                    // HDAPIDでスケジューリングした際のハンドルを保持 - Hold the handle at the time of scheduling in HDAPI
    private Buttons CurrentButtons = Buttons.None;          // 現在のPHANTOMボタン押下状況 - Current PHANTOM buttons state
    private Buttons LastButtons = Buttons.None;             // 前回Update時のPHANTOMボタン - PHANTOM buttons at the time of the last Update

    /// <summary>
    /// PHANToMに接続できていればtrue
    /// If able to connect to the PHANToM : true
    /// </summary>
    /// <value><c>true</c> if this instance is available; otherwise, <c>false</c>.</value>
    internal bool IsAvailable { get { return hHD != (uint)Hd.DeviceHandle.HD_INVALID_HANDLE; } }

    /// <summary>
    /// ジンバル部を基準としたペン先端座標 [mm] (PHANTOM座標系)
    /// Pen tip coordinates relative to the gimbal portion [mm] (PHANTOM coordinate system)
    /// </summary>
    public Vector3 TipOffset = new Vector3(0.0f, 0.0f, -40.0f);

    /// <summary>
    /// 可動範囲下限 [mm]
    /// Movable range limit [mm]
    /// </summary>
    public Vector3 WorkspaceMinimum { get; private set; }

    /// <summary>
    /// 可動範囲上限 [mm]
    /// Movable range limit [mm]
    /// </summary>
    public Vector3 WorkspaceMaximum { get; private set; }

    /// <summary>
    /// 推奨可動範囲下限 [mm]
    /// Recommended movable range limit [mm]
    /// </summary>
    public Vector3 UsableWorkspaceMinimum { get; private set; }

    /// <summary>
    /// 推奨可動範囲上限 [mm]
    /// Recommended movable range limit [mm]
    /// </summary>
    public Vector3 UsableWorkspaceMaximum { get; private set; }

    /// <summary>
    /// 机の面に相当するY座標 [mm]
    /// Y-coordinate corresponding to the surface of the desk [mm]
    /// </summary>
    public float TableTopOffset { get; private set; }

    /// <summary>
    /// PHANTOMの処理が実行中なら true とする
    /// is true if running process in PHANTOM
    /// </summary>
    public bool IsRunning = false;


    /// <summary>
    /// 非同期で呼ばれるメソッドです
    /// This method is called asynchronously
    /// </summary>
    /// <returns>true:要継続, false:終了 - true: main ongoing, false: completion</returns>
    public delegate bool Callback();


    /// <summary>
    /// デフォルトのデバイスに接続します
    /// Connect to the default device
    /// </summary>
    public SimplePhantomUnityController()
    {
        IsRunning = false;

        // デフォルトのデバイスを準備
        // Prepare the default device
        hHD = Hd.hdInitDevice("Right Device");
        ErrorCheck("Initialize device");

        // コールバックメソッドを保持するリスト
        // List to hold the callback method
        CallbackMethods = new List<Hd.SchedulerCallback>();

        // スケジューリングされたメソッドのハンドルをこれで保持
        // Hold the handle of the scheduled method
        ScheduleHandles = new List<ulong>();

        // 可動範囲を取得
        // Get the movable range
        LoadWorkspaceLimit();
    }

    /// <summary>
    /// デバイスの使用を終了し、切断します
    /// Finished using the device, and then disconnect
    /// </summary>
    public void Close()
    {
        Stop();
        ClearSchedule();

        if (hHD != (uint)Hd.DeviceHandle.HD_INVALID_HANDLE)
        {
            Hd.hdDisableDevice(hHD);
            ErrorCheck("Disable device");

            hHD = (uint)Hd.DeviceHandle.HD_INVALID_HANDLE;
        }
    }

    #region スケジューリング関連
    // Scheduling related
    /// <summary>
    /// 非同期処理を開始します
    /// To start the asynchronous processing
    /// </summary>
    public void Start()
    {
        if (!IsAvailable || IsRunning) return;

        // 力を発生させるのは標準でON
        // ON : the standard is to generate a force
        Hd.hdEnable(Hd.Capability.HD_FORCE_OUTPUT);
        ErrorCheck("Enable force output");

        // 非同期処理も開始
        // Also started asynchronous processing
        Hd.hdStartScheduler();
        ErrorCheck("Start scheduler");

        IsRunning = true;
    }

    /// <summary>
    /// 非同期処理を停止します
    /// Stop the asynchronously processing
    /// </summary>
    public void Stop()
    {
        if (!IsAvailable || !IsRunning) return;

        IsRunning = false;

        ////System.Threading.Thread.Sleep (10);
        //foreach (uint handle in ScheduleHandles)
        //{
        //	Hd.hdWaitForCompletion(handle, Hd.WaiteCode.HD_WAIT_INFINITE);
        //	ErrorCheck("Waiting for completion");
        //}

        Hd.hdStopScheduler();
        ErrorCheck("StopScheduler");
		/*

		// 力も停止
		// Force also stop
		Hd.hdDisable(Hd.Capability.HD_FORCE_OUTPUT);
		ErrorCheck("Disable force output");*/
    }

    /// <summary>
    /// 同期的に処理を呼び出します
    /// Call the synchronously processing
    /// </summary>
    public void Do(Callback callback)
    {
        Hd.hdScheduleSynchronous(
            (data) => { return DoCallback(callback); },
            IntPtr.Zero,
            Hd.Priority.HD_DEFAULT_SCHEDULER_PRIORITY
            );
        ErrorCheck("ScheduleSynchronous");
    }

    /// <summary>
    /// 非同期実行にメソッドを追加します
    /// Add a method to the asynchronous execution
    /// </summary>
    /// <param name="callback">要継続ならtrueを返すコールバックメソッド - Callback method that returns true if main continues</param>
    public void AddSchedule(Callback callback, ushort priority)
    {
        Hd.SchedulerCallback method = (data) =>
        {
            return DoCallback(callback);
        };
        CallbackMethods.Add(method);

        ulong handle = Hd.hdScheduleAsynchronous(
            method,
            IntPtr.Zero,
            priority
            );
        ErrorCheck("ScheduleAsynchronous");
        ScheduleHandles.Add(handle);
    }

    /// <summary>
    /// コールバックメソッド呼び出しをより簡略化するためのラップ
    /// Wrap in order to further simplify the callback method calls
    /// </summary>
    /// <param name="callback">引数無しでboolを返すだけに簡略化したメソッド - Methods only to simplify return a bool with no arguments</param>
    /// <returns>完了したか - One completed</returns>
    private Hd.CallbackResult DoCallback(Callback callback)
    {
        bool result;

        Hd.hdBeginFrame(hHD);
        ErrorCheck("BeginFrame");

        result = callback();

        Hd.hdEndFrame(hHD);
        ErrorCheck("EndFrame");

        return (result ? Hd.CallbackResult.HD_CALLBACK_CONTINUE : Hd.CallbackResult.HD_CALLBACK_DONE);
    }

    /// <summary>
    /// 登録済の非同期実行処理を全て消去します
    /// Clear all the asynchronous execution processing registered
    /// </summary>
    public void ClearSchedule()
    {
        foreach (uint handle in ScheduleHandles)
        {
            Hd.hdUnschedule(handle);
            ErrorCheck("Unschedule #" + handle.ToString());
        }
        ScheduleHandles.Clear();
        CallbackMethods.Clear();
    }

    /// <summary>
    /// 処理が1秒間に何回呼ばれるかを指定します
    /// Process to specify whether this will be called many times per second
    /// </summary>
    /// <param name="rate">周期 [Hz] 500 or 1000 - Cycle [Hz] 500 or 1000</param>
    public void SetSchedulerRate(uint rate)
    {
        Hd.hdSetSchedulerRate(rate);
        ErrorCheck("Set scheduler rate");
    }

    #endregion

    #region 情報取得メソッド
    // Information acquisition method
    /// <summary>
    /// 現在のPHANTOM手先座標を返します
    /// It returns the current PHANTOM hand coordinate
    /// </summary>
    /// <returns>ジンバル座標 [mm] - Gimbal coordinate [mm]</returns>
    public Vector3 GetPosition()
    {
        double[] position = new double[3] { 0, 0, 0 };
        Hd.hdGetDoublev(Hd.ParameterName.HD_CURRENT_POSITION, position);
        return new Vector3((float)position[0], (float)position[1], -(float)position[2]);
    }

    /// <summary>
    /// 現在のPHANTOM手先速度を返します
    /// It returns the current PHANTOM hand speed
    /// </summary>
    /// <returns>速度ベクトル [mm/s] - Velocity vector [mm / s]</returns>
    public Vector3 GetVelocity()
    {
        double[] velocity = new double[3] { 0, 0, 0 };
        Hd.hdGetDoublev(Hd.ParameterName.HD_CURRENT_VELOCITY, velocity);
        return new Vector3((float)velocity[0], (float)velocity[1], -(float)velocity[2]);
    }

    /// <summary>
    /// ペン先端の座標を返します
    /// It returns the coordinates of the pen tip
    /// </summary>
    /// <returns>ペン先端座標 [mm] - Pen tip coordinate [mm]</returns>
    public Vector3 GetTipPosition()
    {
        double[] position = new double[3] { 0, 0, 0 };
        double[] matrix = new double[16];
        Hd.hdGetDoublev(Hd.ParameterName.HD_CURRENT_POSITION, position);
        Hd.hdGetDoublev(Hd.ParameterName.HD_CURRENT_TRANSFORM, matrix);

        Vector3 tipPosition;
        tipPosition.x = (float)(position[0] + matrix[0] * TipOffset.x + matrix[4] * TipOffset.y + matrix[8] * TipOffset.z);
        tipPosition.y = (float)(position[1] + matrix[1] * TipOffset.x + matrix[5] * TipOffset.y + matrix[9] * TipOffset.z);
        tipPosition.z = -(float)(position[2] + matrix[2] * TipOffset.x + matrix[6] * TipOffset.y + matrix[10] * TipOffset.z);

        return tipPosition;
    }

    //　↓たぶん誰も使わないし、Unity座標系への変換をしていないのでコメントアウト
    //  ↓ you probably do not use, because it does not have a conversion to Unity coordinate system
    //	/// <summary>
    //	/// 現在のPHANTOMジンバル姿勢を返します
    //	/// <remarks>手先の姿勢ではありません。ジンバル部エンコーダの値です。</remarks>
    //	/// </summary>
    //	/// <returns>ジンバル部分の内、根元からペン部にかけて X～Z に対応した角度 [rad]</returns>
    //	public Vector3 GetGimbalAngles()
    //	{
    //		double[] gimbals = new double[3] { 0, 0, 0 };
    //		Hd.hdGetDoublev(Hd.ParameterName.HD_CURRENT_GIMBAL_ANGLES, gimbals);
    //		return new Vector3((float)gimbals[0], (float)gimbals[1], (float)gimbals[2]);
    //	}

    double[] matrixRotation = new double[16];

    /// <summary>
    /// TODO - get rotation matrix
    /// </summary>
    /// <returns></returns>
    public double[] GetRotationMatrix()
    {
        double[] matrix = new double[16];
        Hd.hdGetDoublev(Hd.ParameterName.HD_CURRENT_TRANSFORM, matrix);

        /*Matrix4x4 result = new Matrix4x4();

        for (int i = 0; i < 4; i++)
        {
            result.SetRow(i, new Vector4((float)matrix[10 * i + 0], (float)matrix[10 * i + 1], (float)matrix[10 * i + 2], (float)matrix[10 * i + 3]));
        }*/

        return matrixRotation;
    }
    public void GetRotationMatrix(out double[] matrix)
    {
        matrix = new double[16];
        Hd.hdGetDoublev(Hd.ParameterName.HD_CURRENT_TRANSFORM, matrix);
    }

    /// <summary>
    /// 現在のPHANTOM手先姿勢を返します
    /// It returns the current PHANTOM hand posture
    /// </summary>
    /// <returns>姿勢を表すクォータニオン - Quaternion that represents the orientation</returns>
    public Quaternion GetRotation()
    {
        //double[] matrix = new double[16];
        matrixRotation = new double[16];
        Hd.hdGetDoublev(Hd.ParameterName.HD_CURRENT_TRANSFORM, matrixRotation);
        //
        //		double qw = Math.Sqrt(1f + matrixRotation[0] + matrixRotation[5] + matrixRotation[10]) / 2;
        //		double w = 4 * qw;
        //		double qx = (matrixRotation[6] - matrixRotation[9]) / w;
        //		double qy = (matrixRotation[8] - matrixRotation[2]) / w;
        //		double qz = (matrixRotation[1] - matrixRotation[4]) / w;
        //		return new Quaternion((float)-qx, (float)-qy, (float)qz, (float)qw);

        double t = 1.0 + matrixRotation[0] + matrixRotation[5] + matrixRotation[10];
        double s;
        double qw, qx, qy, qz;
        if (t >= 1.0)
        {
            s = 0.5 / Math.Sqrt(t);
            qw = 0.25 / s;
            qx = (matrixRotation[6] - matrixRotation[9]) * s;
            qy = (matrixRotation[8] - matrixRotation[2]) * s;
            qz = (matrixRotation[1] - matrixRotation[4]) * s;
        }
        else {
            double max;
            if (matrixRotation[5] > matrixRotation[10])
            {
                max = matrixRotation[5];
            }
            else {
                max = matrixRotation[10];
            }

            if (max < matrixRotation[0])
            {
                t = Math.Sqrt(matrixRotation[0] - (matrixRotation[5] + matrixRotation[10]) + 1.0);
                s = 0.5 / t;
                qw = (matrixRotation[6] - matrixRotation[9]) * s;
                qx = t * 0.5;
                qy = (matrixRotation[1] + matrixRotation[4]) * s;
                qz = (matrixRotation[8] + matrixRotation[2]) * s;
            }
            else if (max == matrixRotation[5])
            {
                t = Math.Sqrt(matrixRotation[5] - (matrixRotation[10] + matrixRotation[0]) + 1.0);
                s = 0.5 / t;
                qw = (matrixRotation[8] - matrixRotation[2]) * s;
                qx = (matrixRotation[1] + matrixRotation[4]) * s;
                qy = t * 0.5;
                qz = (matrixRotation[6] + matrixRotation[9]) * s;
            }
            else {
                t = Math.Sqrt(matrixRotation[10] - (matrixRotation[0] + matrixRotation[5]) + 1.0);
                s = 0.5 / t;
                qw = (matrixRotation[1] - matrixRotation[4]) * s;
                qx = (matrixRotation[8] + matrixRotation[2]) * s;
                qy = (matrixRotation[6] + matrixRotation[9]) * s;
                qz = t * 0.5;
            }
        }
        return new Quaternion(-(float)qx, -(float)qy, (float)qz, (float)qw);
    }

    /// <summary>
    /// 現在押されているボタンを取得します
    /// It gets the button that is currently pressed
    /// </summary>
    /// <returns>Button1 | Button2 | Button3 | Button4</returns>
    public Buttons GetButton()
    {
        int[] button = new int[1];
        Hd.hdGetIntegerv(Hd.ParameterName.HD_CURRENT_BUTTONS, button);
        return (Buttons)button[0];
    }

    /// <summary>
    /// PHANTOのボタン押下状況を更新
    /// Update the button pressing situation of the PHANTOM
    /// </summary>
    /// <returns>The buttons.</returns>
    public Buttons UpdateButtons()
    {
        LastButtons = CurrentButtons;
        CurrentButtons = GetButton();
        return CurrentButtons;
    }

    /// <summary>
    /// 指定されたボタンがまさに押されたところならばtrueを返す
    /// It returns true if the place that has been designated by button is just pressed
    /// </summary>
    /// <returns><c>true</c>, if button was down, <c>false</c> otherwise.</returns>
    /// <param name="button">Button.</param>
    public bool GetButtonDown(Buttons button)
    {
        return (((LastButtons & button) == Buttons.None) && ((CurrentButtons & button) == button));
    }

    /// <summary>
    /// 指定されたボタンがまさに離されたところならばtrueを返す
    /// It returns true if the place that has been designated by button has been just released
    /// </summary>
    /// <returns><c>true</c>, if button was up, <c>false</c> otherwise.</returns>
    /// <param name="button">Button.</param>
    public bool GetButtonUp(Buttons button)
    {
        return (((LastButtons & button) == button) && ((CurrentButtons & button) == Buttons.None));
    }

    /// <summary>
    /// サーボループ開始からの経過時間を取得します
    /// It gets the elapsed time from the servo loop start
    /// </summary>
    /// <returns>時間 [s] - Time [s]</returns>
    public double GetSchedulerTimeStamp()
    {
        return Hd.hdGetSchedulerTimeStamp();
    }

    /// <summary>
    /// Gets the instantaneous update rate of the device [Hz]
    /// </summary>
    /// <returns>The Hz instantaneous rate</returns>
    public double GetInstantaneousUpdateRate()
    {
        double[] rate = new double[1] { 0 };
        Hd.hdGetDoublev(Hd.ParameterName.HD_INSTANTANEOUS_UPDATE_RATE, rate);
        return rate[0];
    }

    /// <summary>
    /// Gets the max force limit of the device [N]
    /// </summary>
    /// <returns>The max force</returns>
    public double GetForceLimit()
    {
        double[] maxForce = new double[1] { 0 };

        Hd.hdGetDoublev(Hd.ParameterName.HD_NOMINAL_MAX_FORCE, maxForce);
        return maxForce[0];
    }

    /// <summary>
    /// Gets the max continuous force limit of the device [N]
    /// </summary>
    /// <returns>The max force</returns>
    public double GetContinuousForceLimit()
    {
        double[] maxForce = new double[1] { 0 };

        Hd.hdGetDoublev(Hd.ParameterName.HD_NOMINAL_MAX_CONTINUOUS_FORCE, maxForce);
        return maxForce[0];
    }

    /// <summary>
    /// verifies if the capability of force clamping is enabled
    /// </summary>
    /// <returns><code>true</code> if the max force campling is enabled, <code>false</code> otherwise</returns>
    public bool IsEnabledMaxForceClamping()
    {
        return Hd.hdIsEnabled(Hd.Capability.HD_MAX_FORCE_CLAMPING);
    }

    /// <summary>
    /// verifies if the capability of software force limit
    /// </summary>
    /// <returns><code>true</code> if the software force limit is enabled, <code>false</code> otherwise</returns>
    public bool IsEnabledSwForceLimit()
    {
        return Hd.hdIsEnabled(Hd.Capability.HD_SOFTWARE_FORCE_LIMIT);
    }

    #endregion

    #region 情報設定メソッド
    // Information setting method
    /// <summary>
    /// PHANTOM発揮力を設定します
    /// Set the PHANTOM display force
    /// </summary>
    /// <param name="force">力のベクトル [N] - Vector of the force [N]</param>
    public void SetForce(Vector3 force)
    {
        double[] forceArray = new double[3];
        forceArray[0] = force.x;
        forceArray[1] = force.y;
        forceArray[2] = -force.z;
        Hd.hdSetDoublev(Hd.ParameterName.HD_CURRENT_FORCE, forceArray);
    }

    #endregion

    #region 内部メソッド
    // Internal method
    /// <summary>
    /// 可動範囲を取得
    /// Get the movable range
    /// </summary>
    private void LoadWorkspaceLimit()
    {
        double[] val = new double[6];

        // 可動限界範囲を取得
        // Get the movable limit range
        Hd.hdGetDoublev(Hd.ParameterName.HD_MAX_WORKSPACE_DIMENSIONS, val);
        ErrorCheck("Getting max workspace");
        WorkspaceMinimum = new Vector3((float)val[0], (float)val[1], -(float)val[2]);
        WorkspaceMaximum = new Vector3((float)val[3], (float)val[4], -(float)val[5]);

        // 推奨可動範囲を取得
        // Get the recommended range of movement
        Hd.hdGetDoublev(Hd.ParameterName.HD_USABLE_WORKSPACE_DIMENSIONS, val);
        ErrorCheck("Getting usable workspace");
        UsableWorkspaceMinimum = new Vector3((float)val[0], (float)val[1], -(float)val[2]);
        UsableWorkspaceMaximum = new Vector3((float)val[3], (float)val[4], -(float)val[5]);

        // 机の高さを取得
        // Gets the height of the desk
        float[] offset = new float[1];
        Hd.hdGetFloatv(Hd.ParameterName.HD_TABLETOP_OFFSET, offset);
        ErrorCheck("Getting table-top offset");
        TableTopOffset = (float)offset[0];
    }

    /// <summary>
    /// 直前のHDAPI呼び出しでエラーがあれば、例外を発生させます
    /// If there is an error in the previous API call, an exception will be raised
    /// </summary>
    static private void ErrorCheck()
    {
        ErrorCheck("");
    }

    /// <summary>
    /// 直前のHDAPI呼び出しでエラーがあれば、例外を発生させます
    /// If there is an error in the previous API call, an exception will be raised
    /// </summary>
    /// <param name="situation">何をしていたかを伝える文字列 - string to know if the situation happened or not</param>
    static private void ErrorCheck(string situation)
    {
        Hd.ErrorInfo error;

        if (Hd.IsError(error = Hd.hdGetError()))
        {
            string errorMessage = Hd.GetErrorString(error.ErrorCode);

            if (situation.Equals(""))
            {
                throw new UnityException("HDAPI : " + errorMessage);
            }
            else {
                throw new UnityException(situation + " / HDAPI : " + errorMessage);
            }
		}
		//Debug.LogError ("err : "+ situation + " = " + error.ErrorCode + "|" + error.hHD + "|" + error.InternalErrorCode);
    }
    #endregion
}
