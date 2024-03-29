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

public class FbxAnimStack : FbxCollection {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;

  internal FbxAnimStack(global::System.IntPtr cPtr, bool cMemoryOwn) : base(FbxWrapperNativePINVOKE.FbxAnimStack_SWIGUpcast(cPtr), cMemoryOwn) {
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(FbxAnimStack obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  public override void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          throw new global::System.MethodAccessException("C++ destructor does not have public access");
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
      base.Dispose();
    }
  }

  public static FbxClassId ClassId {
    set {
      FbxWrapperNativePINVOKE.FbxAnimStack_ClassId_set(FbxClassId.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxAnimStack_ClassId_get();
      FbxClassId ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxClassId(cPtr, false);
      return ret;
    } 
  }

  public override FbxClassId GetClassId() {
    FbxClassId ret = new FbxClassId(FbxWrapperNativePINVOKE.FbxAnimStack_GetClassId(swigCPtr), true);
    return ret;
  }

  public new static FbxAnimStack Create(FbxManager pManager, string pName) {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxAnimStack_Create__SWIG_0(FbxManager.getCPtr(pManager), pName);
    FbxAnimStack ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxAnimStack(cPtr, false);
    return ret;
  }

  public new static FbxAnimStack Create(FbxObject pContainer, string pName) {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxAnimStack_Create__SWIG_1(FbxObject.getCPtr(pContainer), pName);
    FbxAnimStack ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxAnimStack(cPtr, false);
    return ret;
  }

  public SWIGTYPE_p_FbxPropertyTT_FbxString_t Description {
    set {
      FbxWrapperNativePINVOKE.FbxAnimStack_Description_set(swigCPtr, SWIGTYPE_p_FbxPropertyTT_FbxString_t.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxAnimStack_Description_get(swigCPtr);
      SWIGTYPE_p_FbxPropertyTT_FbxString_t ret = (cPtr == global::System.IntPtr.Zero) ? null : new SWIGTYPE_p_FbxPropertyTT_FbxString_t(cPtr, false);
      return ret;
    } 
  }

  public SWIGTYPE_p_FbxPropertyTT_FbxTime_t LocalStart {
    set {
      FbxWrapperNativePINVOKE.FbxAnimStack_LocalStart_set(swigCPtr, SWIGTYPE_p_FbxPropertyTT_FbxTime_t.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxAnimStack_LocalStart_get(swigCPtr);
      SWIGTYPE_p_FbxPropertyTT_FbxTime_t ret = (cPtr == global::System.IntPtr.Zero) ? null : new SWIGTYPE_p_FbxPropertyTT_FbxTime_t(cPtr, false);
      return ret;
    } 
  }

  public SWIGTYPE_p_FbxPropertyTT_FbxTime_t LocalStop {
    set {
      FbxWrapperNativePINVOKE.FbxAnimStack_LocalStop_set(swigCPtr, SWIGTYPE_p_FbxPropertyTT_FbxTime_t.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxAnimStack_LocalStop_get(swigCPtr);
      SWIGTYPE_p_FbxPropertyTT_FbxTime_t ret = (cPtr == global::System.IntPtr.Zero) ? null : new SWIGTYPE_p_FbxPropertyTT_FbxTime_t(cPtr, false);
      return ret;
    } 
  }

  public SWIGTYPE_p_FbxPropertyTT_FbxTime_t ReferenceStart {
    set {
      FbxWrapperNativePINVOKE.FbxAnimStack_ReferenceStart_set(swigCPtr, SWIGTYPE_p_FbxPropertyTT_FbxTime_t.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxAnimStack_ReferenceStart_get(swigCPtr);
      SWIGTYPE_p_FbxPropertyTT_FbxTime_t ret = (cPtr == global::System.IntPtr.Zero) ? null : new SWIGTYPE_p_FbxPropertyTT_FbxTime_t(cPtr, false);
      return ret;
    } 
  }

  public SWIGTYPE_p_FbxPropertyTT_FbxTime_t ReferenceStop {
    set {
      FbxWrapperNativePINVOKE.FbxAnimStack_ReferenceStop_set(swigCPtr, SWIGTYPE_p_FbxPropertyTT_FbxTime_t.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxAnimStack_ReferenceStop_get(swigCPtr);
      SWIGTYPE_p_FbxPropertyTT_FbxTime_t ret = (cPtr == global::System.IntPtr.Zero) ? null : new SWIGTYPE_p_FbxPropertyTT_FbxTime_t(cPtr, false);
      return ret;
    } 
  }

  public void Reset(FbxTakeInfo pTakeInfo) {
    FbxWrapperNativePINVOKE.FbxAnimStack_Reset__SWIG_0(swigCPtr, FbxTakeInfo.getCPtr(pTakeInfo));
  }

  public void Reset() {
    FbxWrapperNativePINVOKE.FbxAnimStack_Reset__SWIG_1(swigCPtr);
  }

  public FbxTimeSpan GetLocalTimeSpan() {
    FbxTimeSpan ret = new FbxTimeSpan(FbxWrapperNativePINVOKE.FbxAnimStack_GetLocalTimeSpan(swigCPtr), true);
    return ret;
  }

  public void SetLocalTimeSpan(FbxTimeSpan pTimeSpan) {
    FbxWrapperNativePINVOKE.FbxAnimStack_SetLocalTimeSpan(swigCPtr, FbxTimeSpan.getCPtr(pTimeSpan));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
  }

  public FbxTimeSpan GetReferenceTimeSpan() {
    FbxTimeSpan ret = new FbxTimeSpan(FbxWrapperNativePINVOKE.FbxAnimStack_GetReferenceTimeSpan(swigCPtr), true);
    return ret;
  }

  public void SetReferenceTimeSpan(FbxTimeSpan pTimeSpan) {
    FbxWrapperNativePINVOKE.FbxAnimStack_SetReferenceTimeSpan(swigCPtr, FbxTimeSpan.getCPtr(pTimeSpan));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
  }

  public bool BakeLayers(FbxAnimEvaluator pEvaluator, FbxTime pStart, FbxTime pStop, FbxTime pPeriod) {
    bool ret = FbxWrapperNativePINVOKE.FbxAnimStack_BakeLayers(swigCPtr, FbxAnimEvaluator.getCPtr(pEvaluator), FbxTime.getCPtr(pStart), FbxTime.getCPtr(pStop), FbxTime.getCPtr(pPeriod));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static FbxAnimStack Cast(FbxObject arg0) {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxAnimStack_Cast(FbxObject.getCPtr(arg0));
    FbxAnimStack ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxAnimStack(cPtr, false);
    return ret;
  }

}

}
