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

public class FbxAnimCurveFilterUnroll : FbxAnimCurveFilter {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;

  internal FbxAnimCurveFilterUnroll(global::System.IntPtr cPtr, bool cMemoryOwn) : base(FbxWrapperNativePINVOKE.FbxAnimCurveFilterUnroll_SWIGUpcast(cPtr), cMemoryOwn) {
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(FbxAnimCurveFilterUnroll obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~FbxAnimCurveFilterUnroll() {
    Dispose();
  }

  public override void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          FbxWrapperNativePINVOKE.delete_FbxAnimCurveFilterUnroll(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
      base.Dispose();
    }
  }

  public FbxAnimCurveFilterUnroll() : this(FbxWrapperNativePINVOKE.new_FbxAnimCurveFilterUnroll(), true) {
  }

  public override string GetName() {
    string ret = FbxWrapperNativePINVOKE.FbxAnimCurveFilterUnroll_GetName(swigCPtr);
    return ret;
  }

  public override bool NeedApply(FbxAnimStack arg0, FbxStatus pStatus) {
    bool ret = FbxWrapperNativePINVOKE.FbxAnimCurveFilterUnroll_NeedApply__SWIG_0(swigCPtr, FbxAnimStack.getCPtr(arg0), FbxStatus.getCPtr(pStatus));
    return ret;
  }

  public override bool NeedApply(FbxAnimStack arg0) {
    bool ret = FbxWrapperNativePINVOKE.FbxAnimCurveFilterUnroll_NeedApply__SWIG_1(swigCPtr, FbxAnimStack.getCPtr(arg0));
    return ret;
  }

  public override bool NeedApply(FbxObject arg0, FbxAnimStack arg1, FbxStatus pStatus) {
    bool ret = FbxWrapperNativePINVOKE.FbxAnimCurveFilterUnroll_NeedApply__SWIG_2(swigCPtr, FbxObject.getCPtr(arg0), FbxAnimStack.getCPtr(arg1), FbxStatus.getCPtr(pStatus));
    return ret;
  }

  public override bool NeedApply(FbxObject arg0, FbxAnimStack arg1) {
    bool ret = FbxWrapperNativePINVOKE.FbxAnimCurveFilterUnroll_NeedApply__SWIG_3(swigCPtr, FbxObject.getCPtr(arg0), FbxAnimStack.getCPtr(arg1));
    return ret;
  }

  public override bool NeedApply(FbxAnimCurveNode pCurveNode, FbxStatus pStatus) {
    bool ret = FbxWrapperNativePINVOKE.FbxAnimCurveFilterUnroll_NeedApply__SWIG_4(swigCPtr, FbxAnimCurveNode.getCPtr(pCurveNode), FbxStatus.getCPtr(pStatus));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public override bool NeedApply(FbxAnimCurveNode pCurveNode) {
    bool ret = FbxWrapperNativePINVOKE.FbxAnimCurveFilterUnroll_NeedApply__SWIG_5(swigCPtr, FbxAnimCurveNode.getCPtr(pCurveNode));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public override bool NeedApply(SWIGTYPE_p_p_FbxAnimCurve pCurve, int pCount, FbxStatus pStatus) {
    bool ret = FbxWrapperNativePINVOKE.FbxAnimCurveFilterUnroll_NeedApply__SWIG_6(swigCPtr, SWIGTYPE_p_p_FbxAnimCurve.getCPtr(pCurve), pCount, FbxStatus.getCPtr(pStatus));
    return ret;
  }

  public override bool NeedApply(SWIGTYPE_p_p_FbxAnimCurve pCurve, int pCount) {
    bool ret = FbxWrapperNativePINVOKE.FbxAnimCurveFilterUnroll_NeedApply__SWIG_7(swigCPtr, SWIGTYPE_p_p_FbxAnimCurve.getCPtr(pCurve), pCount);
    return ret;
  }

  public override bool NeedApply(FbxAnimCurve arg0, FbxStatus pStatus) {
    bool ret = FbxWrapperNativePINVOKE.FbxAnimCurveFilterUnroll_NeedApply__SWIG_8(swigCPtr, FbxAnimCurve.getCPtr(arg0), FbxStatus.getCPtr(pStatus));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public override bool NeedApply(FbxAnimCurve arg0) {
    bool ret = FbxWrapperNativePINVOKE.FbxAnimCurveFilterUnroll_NeedApply__SWIG_9(swigCPtr, FbxAnimCurve.getCPtr(arg0));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public override bool Apply(FbxAnimStack arg0, FbxStatus pStatus) {
    bool ret = FbxWrapperNativePINVOKE.FbxAnimCurveFilterUnroll_Apply__SWIG_0(swigCPtr, FbxAnimStack.getCPtr(arg0), FbxStatus.getCPtr(pStatus));
    return ret;
  }

  public override bool Apply(FbxAnimStack arg0) {
    bool ret = FbxWrapperNativePINVOKE.FbxAnimCurveFilterUnroll_Apply__SWIG_1(swigCPtr, FbxAnimStack.getCPtr(arg0));
    return ret;
  }

  public override bool Apply(FbxObject arg0, FbxAnimStack arg1, FbxStatus pStatus) {
    bool ret = FbxWrapperNativePINVOKE.FbxAnimCurveFilterUnroll_Apply__SWIG_2(swigCPtr, FbxObject.getCPtr(arg0), FbxAnimStack.getCPtr(arg1), FbxStatus.getCPtr(pStatus));
    return ret;
  }

  public override bool Apply(FbxObject arg0, FbxAnimStack arg1) {
    bool ret = FbxWrapperNativePINVOKE.FbxAnimCurveFilterUnroll_Apply__SWIG_3(swigCPtr, FbxObject.getCPtr(arg0), FbxAnimStack.getCPtr(arg1));
    return ret;
  }

  public override bool Apply(FbxAnimCurveNode pCurveNode, FbxStatus pStatus) {
    bool ret = FbxWrapperNativePINVOKE.FbxAnimCurveFilterUnroll_Apply__SWIG_4(swigCPtr, FbxAnimCurveNode.getCPtr(pCurveNode), FbxStatus.getCPtr(pStatus));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public override bool Apply(FbxAnimCurveNode pCurveNode) {
    bool ret = FbxWrapperNativePINVOKE.FbxAnimCurveFilterUnroll_Apply__SWIG_5(swigCPtr, FbxAnimCurveNode.getCPtr(pCurveNode));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public override bool Apply(SWIGTYPE_p_p_FbxAnimCurve pCurve, int pCount, FbxStatus pStatus) {
    bool ret = FbxWrapperNativePINVOKE.FbxAnimCurveFilterUnroll_Apply__SWIG_6(swigCPtr, SWIGTYPE_p_p_FbxAnimCurve.getCPtr(pCurve), pCount, FbxStatus.getCPtr(pStatus));
    return ret;
  }

  public override bool Apply(SWIGTYPE_p_p_FbxAnimCurve pCurve, int pCount) {
    bool ret = FbxWrapperNativePINVOKE.FbxAnimCurveFilterUnroll_Apply__SWIG_7(swigCPtr, SWIGTYPE_p_p_FbxAnimCurve.getCPtr(pCurve), pCount);
    return ret;
  }

  public override bool Apply(FbxAnimCurve arg0, FbxStatus pStatus) {
    bool ret = FbxWrapperNativePINVOKE.FbxAnimCurveFilterUnroll_Apply__SWIG_8(swigCPtr, FbxAnimCurve.getCPtr(arg0), FbxStatus.getCPtr(pStatus));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public override bool Apply(FbxAnimCurve arg0) {
    bool ret = FbxWrapperNativePINVOKE.FbxAnimCurveFilterUnroll_Apply__SWIG_9(swigCPtr, FbxAnimCurve.getCPtr(arg0));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public override void Reset() {
    FbxWrapperNativePINVOKE.FbxAnimCurveFilterUnroll_Reset(swigCPtr);
  }

  public double GetQualityTolerance() {
    double ret = FbxWrapperNativePINVOKE.FbxAnimCurveFilterUnroll_GetQualityTolerance(swigCPtr);
    return ret;
  }

  public void SetQualityTolerance(double pQualityTolerance) {
    FbxWrapperNativePINVOKE.FbxAnimCurveFilterUnroll_SetQualityTolerance(swigCPtr, pQualityTolerance);
  }

  public bool GetTestForPath() {
    bool ret = FbxWrapperNativePINVOKE.FbxAnimCurveFilterUnroll_GetTestForPath(swigCPtr);
    return ret;
  }

  public void SetTestForPath(bool pTestForPath) {
    FbxWrapperNativePINVOKE.FbxAnimCurveFilterUnroll_SetTestForPath(swigCPtr, pTestForPath);
  }

  public bool GetForceAutoTangents() {
    bool ret = FbxWrapperNativePINVOKE.FbxAnimCurveFilterUnroll_GetForceAutoTangents(swigCPtr);
    return ret;
  }

  public void SetForceAutoTangents(bool pForceAutoTangents) {
    FbxWrapperNativePINVOKE.FbxAnimCurveFilterUnroll_SetForceAutoTangents(swigCPtr, pForceAutoTangents);
  }

  public void SetRotationOrder(FbxEuler.EOrder pOrder) {
    FbxWrapperNativePINVOKE.FbxAnimCurveFilterUnroll_SetRotationOrder(swigCPtr, (int)pOrder);
  }

}

}
