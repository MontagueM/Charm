//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 3.0.12
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------

namespace Internal.Fbx {

public class FbxAnimCurveFilterResample : FbxAnimCurveFilter {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;

  internal FbxAnimCurveFilterResample(global::System.IntPtr cPtr, bool cMemoryOwn) : base(FbxWrapperNativePINVOKE.FbxAnimCurveFilterResample_SWIGUpcast(cPtr), cMemoryOwn) {
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(FbxAnimCurveFilterResample obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~FbxAnimCurveFilterResample() {
    Dispose();
  }

  public override void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          FbxWrapperNativePINVOKE.delete_FbxAnimCurveFilterResample(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
      base.Dispose();
    }
  }

  public FbxAnimCurveFilterResample() : this(FbxWrapperNativePINVOKE.new_FbxAnimCurveFilterResample(), true) {
  }

  public override string GetName() {
    string ret = FbxWrapperNativePINVOKE.FbxAnimCurveFilterResample_GetName(swigCPtr);
    return ret;
  }

  public override bool Apply(FbxAnimStack pAnimStack, FbxStatus pStatus) {
    bool ret = FbxWrapperNativePINVOKE.FbxAnimCurveFilterResample_Apply__SWIG_0(swigCPtr, FbxAnimStack.getCPtr(pAnimStack), FbxStatus.getCPtr(pStatus));
    return ret;
  }

  public override bool Apply(FbxAnimStack pAnimStack) {
    bool ret = FbxWrapperNativePINVOKE.FbxAnimCurveFilterResample_Apply__SWIG_1(swigCPtr, FbxAnimStack.getCPtr(pAnimStack));
    return ret;
  }

  public override bool Apply(FbxObject pObj, FbxAnimStack pAnimStack, FbxStatus pStatus) {
    bool ret = FbxWrapperNativePINVOKE.FbxAnimCurveFilterResample_Apply__SWIG_2(swigCPtr, FbxObject.getCPtr(pObj), FbxAnimStack.getCPtr(pAnimStack), FbxStatus.getCPtr(pStatus));
    return ret;
  }

  public override bool Apply(FbxObject pObj, FbxAnimStack pAnimStack) {
    bool ret = FbxWrapperNativePINVOKE.FbxAnimCurveFilterResample_Apply__SWIG_3(swigCPtr, FbxObject.getCPtr(pObj), FbxAnimStack.getCPtr(pAnimStack));
    return ret;
  }

  public override bool Apply(FbxAnimCurveNode pCurveNode, FbxStatus pStatus) {
    bool ret = FbxWrapperNativePINVOKE.FbxAnimCurveFilterResample_Apply__SWIG_4(swigCPtr, FbxAnimCurveNode.getCPtr(pCurveNode), FbxStatus.getCPtr(pStatus));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public override bool Apply(FbxAnimCurveNode pCurveNode) {
    bool ret = FbxWrapperNativePINVOKE.FbxAnimCurveFilterResample_Apply__SWIG_5(swigCPtr, FbxAnimCurveNode.getCPtr(pCurveNode));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public override bool Apply(SWIGTYPE_p_p_FbxAnimCurve pCurve, int pCount, FbxStatus pStatus) {
    bool ret = FbxWrapperNativePINVOKE.FbxAnimCurveFilterResample_Apply__SWIG_6(swigCPtr, SWIGTYPE_p_p_FbxAnimCurve.getCPtr(pCurve), pCount, FbxStatus.getCPtr(pStatus));
    return ret;
  }

  public override bool Apply(SWIGTYPE_p_p_FbxAnimCurve pCurve, int pCount) {
    bool ret = FbxWrapperNativePINVOKE.FbxAnimCurveFilterResample_Apply__SWIG_7(swigCPtr, SWIGTYPE_p_p_FbxAnimCurve.getCPtr(pCurve), pCount);
    return ret;
  }

  public override bool Apply(FbxAnimCurve pCurve, FbxStatus pStatus) {
    bool ret = FbxWrapperNativePINVOKE.FbxAnimCurveFilterResample_Apply__SWIG_8(swigCPtr, FbxAnimCurve.getCPtr(pCurve), FbxStatus.getCPtr(pStatus));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public override bool Apply(FbxAnimCurve pCurve) {
    bool ret = FbxWrapperNativePINVOKE.FbxAnimCurveFilterResample_Apply__SWIG_9(swigCPtr, FbxAnimCurve.getCPtr(pCurve));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public override void Reset() {
    FbxWrapperNativePINVOKE.FbxAnimCurveFilterResample_Reset(swigCPtr);
  }

  public void SetKeysOnFrame(bool pKeysOnFrame) {
    FbxWrapperNativePINVOKE.FbxAnimCurveFilterResample_SetKeysOnFrame(swigCPtr, pKeysOnFrame);
  }

  public bool GetKeysOnFrame() {
    bool ret = FbxWrapperNativePINVOKE.FbxAnimCurveFilterResample_GetKeysOnFrame(swigCPtr);
    return ret;
  }

  public FbxTime GetPeriodTime() {
    FbxTime ret = new FbxTime(FbxWrapperNativePINVOKE.FbxAnimCurveFilterResample_GetPeriodTime(swigCPtr), true);
    return ret;
  }

  public void SetPeriodTime(FbxTime pPeriod) {
    FbxWrapperNativePINVOKE.FbxAnimCurveFilterResample_SetPeriodTime(swigCPtr, FbxTime.getCPtr(pPeriod));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
  }

  public bool GetIntelligentMode() {
    bool ret = FbxWrapperNativePINVOKE.FbxAnimCurveFilterResample_GetIntelligentMode(swigCPtr);
    return ret;
  }

  public void SetIntelligentMode(bool pIntelligent) {
    FbxWrapperNativePINVOKE.FbxAnimCurveFilterResample_SetIntelligentMode(swigCPtr, pIntelligent);
  }

}

}
