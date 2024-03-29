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

public class FbxDouble2 : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal FbxDouble2(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(FbxDouble2 obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~FbxDouble2() {
    Dispose();
  }

  public virtual void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          FbxWrapperNativePINVOKE.delete_FbxDouble2(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
    }
  }

  public FbxDouble2() : this(FbxWrapperNativePINVOKE.new_FbxDouble2__SWIG_0(), true) {
  }

  public FbxDouble2(double pValue) : this(FbxWrapperNativePINVOKE.new_FbxDouble2__SWIG_1(pValue), true) {
  }

  public FbxDouble2(double pData0, double pData1) : this(FbxWrapperNativePINVOKE.new_FbxDouble2__SWIG_2(pData0, pData1), true) {
  }

  public SWIGTYPE_p_double at(int pIndex) {
    SWIGTYPE_p_double ret = new SWIGTYPE_p_double(FbxWrapperNativePINVOKE.FbxDouble2_at__SWIG_0(swigCPtr, pIndex), false);
    return ret;
  }

  public FbxDouble2 assign(double pValue) {
    FbxDouble2 ret = new FbxDouble2(FbxWrapperNativePINVOKE.FbxDouble2_assign__SWIG_0(swigCPtr, pValue), false);
    return ret;
  }

  public FbxDouble2 assign(FbxDouble2 pVector) {
    FbxDouble2 ret = new FbxDouble2(FbxWrapperNativePINVOKE.FbxDouble2_assign__SWIG_1(swigCPtr, FbxDouble2.getCPtr(pVector)), false);
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool eq(FbxDouble2 pVector) {
    bool ret = FbxWrapperNativePINVOKE.FbxDouble2_eq(swigCPtr, FbxDouble2.getCPtr(pVector));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool ne(FbxDouble2 pVector) {
    bool ret = FbxWrapperNativePINVOKE.FbxDouble2_ne(swigCPtr, FbxDouble2.getCPtr(pVector));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public SWIGTYPE_p_double Buffer() {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxDouble2_Buffer__SWIG_0(swigCPtr);
    SWIGTYPE_p_double ret = (cPtr == global::System.IntPtr.Zero) ? null : new SWIGTYPE_p_double(cPtr, false);
    return ret;
  }

  public SWIGTYPE_p_double mData {
    set {
      FbxWrapperNativePINVOKE.FbxDouble2_mData_set(swigCPtr, SWIGTYPE_p_double.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxDouble2_mData_get(swigCPtr);
      SWIGTYPE_p_double ret = (cPtr == global::System.IntPtr.Zero) ? null : new SWIGTYPE_p_double(cPtr, false);
      return ret;
    } 
  }

}

}
