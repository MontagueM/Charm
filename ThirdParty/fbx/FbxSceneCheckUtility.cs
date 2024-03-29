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

public class FbxSceneCheckUtility : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal FbxSceneCheckUtility(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(FbxSceneCheckUtility obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~FbxSceneCheckUtility() {
    Dispose();
  }

  public virtual void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          FbxWrapperNativePINVOKE.delete_FbxSceneCheckUtility(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
    }
  }

  public FbxSceneCheckUtility(FbxScene pScene, FbxStatus pStatus, SWIGTYPE_p_FbxArrayT_FbxString_p_t pDetails) : this(FbxWrapperNativePINVOKE.new_FbxSceneCheckUtility__SWIG_0(FbxScene.getCPtr(pScene), FbxStatus.getCPtr(pStatus), SWIGTYPE_p_FbxArrayT_FbxString_p_t.getCPtr(pDetails)), true) {
  }

  public FbxSceneCheckUtility(FbxScene pScene, FbxStatus pStatus) : this(FbxWrapperNativePINVOKE.new_FbxSceneCheckUtility__SWIG_1(FbxScene.getCPtr(pScene), FbxStatus.getCPtr(pStatus)), true) {
  }

  public FbxSceneCheckUtility(FbxScene pScene) : this(FbxWrapperNativePINVOKE.new_FbxSceneCheckUtility__SWIG_2(FbxScene.getCPtr(pScene)), true) {
  }

  public bool Validate(FbxSceneCheckUtility.ECheckMode pCheckMode) {
    bool ret = FbxWrapperNativePINVOKE.FbxSceneCheckUtility_Validate__SWIG_0(swigCPtr, (int)pCheckMode);
    return ret;
  }

  public bool Validate() {
    bool ret = FbxWrapperNativePINVOKE.FbxSceneCheckUtility_Validate__SWIG_1(swigCPtr);
    return ret;
  }

  public enum ECheckMode {
    eCheckCycles = 1,
    eCheckAnimationEmptyLayers = 2,
    eCheckAnimatioCurveData = 4,
    eCheckAnimationData = 6,
    eCheckGeometryData = 8,
    eCheckOtherData = 16,
    eCkeckData = 30
  }

}

}
