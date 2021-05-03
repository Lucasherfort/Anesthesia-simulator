using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StanfordVREventDispatcher
{
    public delegate void OnlyEvent();

    public static event OnlyEvent onCaseStarted;


    //Parallel step
    public static event OnlyEvent onSondeUsed;


    //First step choose objects
    public static event OnlyEvent onRightObjectGrabbed;
    public static event OnlyEvent onWrongObjectGrabbed;
    public static event OnlyEvent onEscargotGrabbed;
    public static event OnlyEvent onGuideRemoved;
    public static event OnlyEvent onRightObjectDropped;
    public static event OnlyEvent onFirstStepEnded;

    //Second step echoguided anesthesia
    public static event OnlyEvent onGelUsed;
    public static event OnlyEvent onAnesthesiaEnterSkin;
    public static event OnlyEvent onSurfaceAnesthesiaDone;
    public static event OnlyEvent onAnesthesiaEnterDeep;
    public static event OnlyEvent onDeepAnesthesiaDone;
    public static event OnlyEvent onAnesthesiaExitSkin;
    public static event OnlyEvent onSecondStepEnded;

    //Third step echoguided needle
    public static event OnlyEvent onNeedleEnterSkin;
    public static event OnlyEvent onNeedleEnteredArteria;
    public static event OnlyEvent onThirdStepEnded;

    //Fourth step microguide interaction
    public static event OnlyEvent onGuideEnterNeedle;
    public static event OnlyEvent onGuideSwipped;
    public static event OnlyEvent onScalpelUsed;
    public static event OnlyEvent onClampUsed;
    public static event OnlyEvent onNeedleExitGuide;
    public static event OnlyEvent onNeedleInBox;
    public static event OnlyEvent onSheathEnterGuide;
    public static event OnlyEvent onSheathEnterSkin;


    public static event OnlyEvent onCaseFinished;

    /// <summary>
    /// Dispatch events
    /// </summary>
    
    //Case start
    public static void dispatchOnCaseStarted()
    {
        if (null != onCaseStarted) onCaseStarted();
    }

    //Sonde
    public static void dispatchOnSondeUsed()
    {
        if (null != onSondeUsed) onSondeUsed();
    }

    //First step
    public static void dispatchOnRightObjectGrabbed()
    {
        if (null != onRightObjectGrabbed) onRightObjectGrabbed();
    }
    public static void dispatchOnWrongObjectGrabbed()
    {
        if (null != onWrongObjectGrabbed) onWrongObjectGrabbed();
    }
    public static void dispatchOnEscargotGrabbed()
    {
        if (null != onEscargotGrabbed) onEscargotGrabbed();
    }
    public static void dispatchOnGuideRemoved()
    {
        if(null != onGuideRemoved) onGuideRemoved();
    }
    public static void dispatchOnRightObjectDropped()
    {
        if (null != onRightObjectDropped) onRightObjectDropped();
    }
    public static void dispatchOnFirstStepEnded()
    {
        if (null != onFirstStepEnded) onFirstStepEnded();
    }

    //Second step
    public static void dispatchOnGelUsed()
    {
        if (null != onGelUsed) onGelUsed();
    }

    public static void dispatchOnAnesthesiaEnterSkin()
    {
        if (null != onAnesthesiaEnterSkin) onAnesthesiaEnterSkin();
    }
    public static void dispatchOnSurfaceAnesthesiaDone()
    {
        if (null != onSurfaceAnesthesiaDone) onSurfaceAnesthesiaDone();
    }
    public static void dispatchOnAnesthesiaEnterDeep()
    {
        if (null != onAnesthesiaEnterSkin) onAnesthesiaEnterDeep();
    }
    public static void dispatchOnDeepAnesthesiaDone()
    {
        if (null != onDeepAnesthesiaDone) onDeepAnesthesiaDone();
    }
    public static void dispatchOnAnesthesiaExitSkin()
    {
        if (null != onAnesthesiaExitSkin) onAnesthesiaExitSkin();
    }
    public static void dispatchOnSecondStepEnded()
    {
        if (null != onSecondStepEnded) onSecondStepEnded();
    }

    //Third step
    public static void dispatchOnNeedleEnterSkin()
    {
        if (null != onNeedleEnterSkin) onNeedleEnterSkin();
    }
    public static void dispatchOnNeedleEnteredArteria()
    {
        if (null != onNeedleEnteredArteria) onNeedleEnteredArteria();
    }
    public static void dispatchOnThirdStepEnded()
    {
        if (null != onSecondStepEnded) onThirdStepEnded();
    }

    //Fourth step
    public static void dispatchOnGuideEnterNeedle()
    {
        if (null != onGuideEnterNeedle) onGuideEnterNeedle();
    }
    public static void dispatchOnGuideSwipped()
    {
        if (null != onGuideSwipped) onGuideSwipped();
    }
    public static void dispatchOnScalpelUsed()
    {
        if (null != onScalpelUsed) onScalpelUsed();
    }
    public static void dispatchOnClampUsed()
    {
        if (null != onClampUsed) onClampUsed();
    }
    public static void dispatchOnNeedleExitGuide()
    {
        if (null != onNeedleExitGuide) onNeedleExitGuide();
    }
    public static void dispatchOnNeedleInBox()
    {
        if (null != onNeedleInBox) onNeedleInBox();
    }
    public static void dispatchOnSheathEnterGuide()
    {
        if (null != onSheathEnterGuide) onSheathEnterGuide();
    }
    public static void dispatchOnSheathEnterSkin()
    {
        if (null != onSheathEnterSkin) onSheathEnterSkin();
    }

    //Case end
    public static void dispatchOnCaseFinished()
    {
        if (null != onCaseFinished) onCaseFinished();
    }
}
