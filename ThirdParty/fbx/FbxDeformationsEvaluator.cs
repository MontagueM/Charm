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

public class FbxDeformationsEvaluator : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal FbxDeformationsEvaluator(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(FbxDeformationsEvaluator obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~FbxDeformationsEvaluator() {
    Dispose();
  }

  public virtual void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          FbxWrapperNativePINVOKE.delete_FbxDeformationsEvaluator(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
    }
  }

  public bool Init(FbxNode pNode, FbxMesh pMesh) {
    bool ret = FbxWrapperNativePINVOKE.FbxDeformationsEvaluator_Init(swigCPtr, FbxNode.getCPtr(pNode), FbxMesh.getCPtr(pMesh));
    return ret;
  }

  public bool ComputeShapeDeformation(FbxVector4 pVertexArray, FbxTime pTime) {
    bool ret = FbxWrapperNativePINVOKE.FbxDeformationsEvaluator_ComputeShapeDeformation(swigCPtr, FbxVector4.getCPtr(pVertexArray), FbxTime.getCPtr(pTime));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool ComputeSkinDeformation(FbxVector4 pVertexArray, FbxTime pTime, FbxAMatrix pGX, FbxPose pPose) {
    bool ret = FbxWrapperNativePINVOKE.FbxDeformationsEvaluator_ComputeSkinDeformation__SWIG_0(swigCPtr, FbxVector4.getCPtr(pVertexArray), FbxTime.getCPtr(pTime), FbxAMatrix.getCPtr(pGX), FbxPose.getCPtr(pPose));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool ComputeSkinDeformation(FbxVector4 pVertexArray, FbxTime pTime, FbxAMatrix pGX) {
    bool ret = FbxWrapperNativePINVOKE.FbxDeformationsEvaluator_ComputeSkinDeformation__SWIG_1(swigCPtr, FbxVector4.getCPtr(pVertexArray), FbxTime.getCPtr(pTime), FbxAMatrix.getCPtr(pGX));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool ComputeSkinDeformation(FbxVector4 pVertexArray, FbxTime pTime) {
    bool ret = FbxWrapperNativePINVOKE.FbxDeformationsEvaluator_ComputeSkinDeformation__SWIG_2(swigCPtr, FbxVector4.getCPtr(pVertexArray), FbxTime.getCPtr(pTime));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public FbxDeformationsEvaluator() : this(FbxWrapperNativePINVOKE.new_FbxDeformationsEvaluator(), true) {
  }

}

}
