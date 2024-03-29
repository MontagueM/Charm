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

public class FbxEventHandler : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal FbxEventHandler(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(FbxEventHandler obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~FbxEventHandler() {
    Dispose();
  }

  public virtual void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          FbxWrapperNativePINVOKE.delete_FbxEventHandler(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
    }
  }

  public virtual int GetHandlerEventType() {
    int ret = FbxWrapperNativePINVOKE.FbxEventHandler_GetHandlerEventType(swigCPtr);
    return ret;
  }

  public virtual void FunctionCall(FbxEventBase pEvent) {
    FbxWrapperNativePINVOKE.FbxEventHandler_FunctionCall(swigCPtr, FbxEventBase.getCPtr(pEvent));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
  }

  public virtual FbxListener GetListener() {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxEventHandler_GetListener(swigCPtr);
    FbxListener ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxListener(cPtr, false);
    return ret;
  }

  public SWIGTYPE_p_FbxListNodeT_FbxEventHandler_t GetListNode(int index) {
    SWIGTYPE_p_FbxListNodeT_FbxEventHandler_t ret = new SWIGTYPE_p_FbxListNodeT_FbxEventHandler_t(FbxWrapperNativePINVOKE.FbxEventHandler_GetListNode__SWIG_0(swigCPtr, index), false);
    return ret;
  }

  public SWIGTYPE_p_FbxListNodeT_FbxEventHandler_t GetListNode() {
    SWIGTYPE_p_FbxListNodeT_FbxEventHandler_t ret = new SWIGTYPE_p_FbxListNodeT_FbxEventHandler_t(FbxWrapperNativePINVOKE.FbxEventHandler_GetListNode__SWIG_1(swigCPtr), false);
    return ret;
  }

  public enum EType {
    eListener,
    eEmitter,
    eCount
  }

}

}
