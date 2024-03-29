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

public class FbxAxisSystem : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal FbxAxisSystem(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(FbxAxisSystem obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~FbxAxisSystem() {
    Dispose();
  }

  public virtual void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          FbxWrapperNativePINVOKE.delete_FbxAxisSystem(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
    }
  }

  public FbxAxisSystem() : this(FbxWrapperNativePINVOKE.new_FbxAxisSystem__SWIG_0(), true) {
  }

  public FbxAxisSystem(FbxAxisSystem.EUpVector pUpVector, FbxAxisSystem.EFrontVector pFrontVector, FbxAxisSystem.ECoordSystem pCoorSystem) : this(FbxWrapperNativePINVOKE.new_FbxAxisSystem__SWIG_1((int)pUpVector, (int)pFrontVector, (int)pCoorSystem), true) {
  }

  public FbxAxisSystem(FbxAxisSystem pAxisSystem) : this(FbxWrapperNativePINVOKE.new_FbxAxisSystem__SWIG_2(FbxAxisSystem.getCPtr(pAxisSystem)), true) {
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
  }

  public FbxAxisSystem(FbxAxisSystem.EPreDefinedAxisSystem pAxisSystem) : this(FbxWrapperNativePINVOKE.new_FbxAxisSystem__SWIG_3((int)pAxisSystem), true) {
  }

  public bool eq(FbxAxisSystem pAxisSystem) {
    bool ret = FbxWrapperNativePINVOKE.FbxAxisSystem_eq(swigCPtr, FbxAxisSystem.getCPtr(pAxisSystem));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool ne(FbxAxisSystem pAxisSystem) {
    bool ret = FbxWrapperNativePINVOKE.FbxAxisSystem_ne(swigCPtr, FbxAxisSystem.getCPtr(pAxisSystem));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public FbxAxisSystem assign(FbxAxisSystem pAxisSystem) {
    FbxAxisSystem ret = new FbxAxisSystem(FbxWrapperNativePINVOKE.FbxAxisSystem_assign(swigCPtr, FbxAxisSystem.getCPtr(pAxisSystem)), false);
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static FbxAxisSystem MayaZUp {
    get {
      global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxAxisSystem_MayaZUp_get();
      FbxAxisSystem ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxAxisSystem(cPtr, false);
      return ret;
    } 
  }

  public static FbxAxisSystem MayaYUp {
    get {
      global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxAxisSystem_MayaYUp_get();
      FbxAxisSystem ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxAxisSystem(cPtr, false);
      return ret;
    } 
  }

  public static FbxAxisSystem Max {
    get {
      global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxAxisSystem_Max_get();
      FbxAxisSystem ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxAxisSystem(cPtr, false);
      return ret;
    } 
  }

  public static FbxAxisSystem Motionbuilder {
    get {
      global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxAxisSystem_Motionbuilder_get();
      FbxAxisSystem ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxAxisSystem(cPtr, false);
      return ret;
    } 
  }

  public static FbxAxisSystem OpenGL {
    get {
      global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxAxisSystem_OpenGL_get();
      FbxAxisSystem ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxAxisSystem(cPtr, false);
      return ret;
    } 
  }

  public static FbxAxisSystem DirectX {
    get {
      global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxAxisSystem_DirectX_get();
      FbxAxisSystem ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxAxisSystem(cPtr, false);
      return ret;
    } 
  }

  public static FbxAxisSystem Lightwave {
    get {
      global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxAxisSystem_Lightwave_get();
      FbxAxisSystem ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxAxisSystem(cPtr, false);
      return ret;
    } 
  }

  public void ConvertScene(FbxScene pScene) {
    FbxWrapperNativePINVOKE.FbxAxisSystem_ConvertScene__SWIG_0(swigCPtr, FbxScene.getCPtr(pScene));
  }

  public void ConvertScene(FbxScene pScene, FbxNode pFbxRoot) {
    FbxWrapperNativePINVOKE.FbxAxisSystem_ConvertScene__SWIG_1(swigCPtr, FbxScene.getCPtr(pScene), FbxNode.getCPtr(pFbxRoot));
  }

  public FbxAxisSystem.EFrontVector GetFrontVector(SWIGTYPE_p_int pSign) {
    FbxAxisSystem.EFrontVector ret = (FbxAxisSystem.EFrontVector)FbxWrapperNativePINVOKE.FbxAxisSystem_GetFrontVector(swigCPtr, SWIGTYPE_p_int.getCPtr(pSign));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public FbxAxisSystem.EUpVector GetUpVector(SWIGTYPE_p_int pSign) {
    FbxAxisSystem.EUpVector ret = (FbxAxisSystem.EUpVector)FbxWrapperNativePINVOKE.FbxAxisSystem_GetUpVector(swigCPtr, SWIGTYPE_p_int.getCPtr(pSign));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public FbxAxisSystem.ECoordSystem GetCoorSystem() {
    FbxAxisSystem.ECoordSystem ret = (FbxAxisSystem.ECoordSystem)FbxWrapperNativePINVOKE.FbxAxisSystem_GetCoorSystem(swigCPtr);
    return ret;
  }

  public void GetMatrix(FbxAMatrix pMatrix) {
    FbxWrapperNativePINVOKE.FbxAxisSystem_GetMatrix(swigCPtr, FbxAMatrix.getCPtr(pMatrix));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
  }

  public void ConvertChildren(FbxNode pRoot, FbxAxisSystem pSrcSystem) {
    FbxWrapperNativePINVOKE.FbxAxisSystem_ConvertChildren(swigCPtr, FbxNode.getCPtr(pRoot), FbxAxisSystem.getCPtr(pSrcSystem));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
  }

  public enum EUpVector {
    eXAxis = 1,
    eYAxis = 2,
    eZAxis = 3
  }

  public enum EFrontVector {
    eParityEven = 1,
    eParityOdd = 2
  }

  public enum ECoordSystem {
    eRightHanded,
    eLeftHanded
  }

  public enum EPreDefinedAxisSystem {
    eMayaZUp,
    eMayaYUp,
    eMax,
    eMotionBuilder,
    eOpenGL,
    eDirectX,
    eLightwave
  }

}

}
