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

public class FbxPropertyEvalState : FbxEvalState {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;

  internal FbxPropertyEvalState(global::System.IntPtr cPtr, bool cMemoryOwn) : base(FbxWrapperNativePINVOKE.FbxPropertyEvalState_SWIGUpcast(cPtr), cMemoryOwn) {
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(FbxPropertyEvalState obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~FbxPropertyEvalState() {
    Dispose();
  }

  public override void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          FbxWrapperNativePINVOKE.delete_FbxPropertyEvalState(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
      base.Dispose();
    }
  }

  public FbxPropertyEvalState(FbxProperty pProperty) : this(FbxWrapperNativePINVOKE.new_FbxPropertyEvalState(FbxProperty.getCPtr(pProperty)), true) {
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
  }

  public FbxPropertyValue mValue {
    set {
      FbxWrapperNativePINVOKE.FbxPropertyEvalState_mValue_set(swigCPtr, FbxPropertyValue.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxPropertyEvalState_mValue_get(swigCPtr);
      FbxPropertyValue ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxPropertyValue(cPtr, false);
      return ret;
    } 
  }

}

}
