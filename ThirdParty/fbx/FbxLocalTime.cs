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

public class FbxLocalTime : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal FbxLocalTime(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(FbxLocalTime obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~FbxLocalTime() {
    Dispose();
  }

  public virtual void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          FbxWrapperNativePINVOKE.delete_FbxLocalTime(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
    }
  }

  public FbxLocalTime() : this(FbxWrapperNativePINVOKE.new_FbxLocalTime(), true) {
  }

  public int mYear {
    set {
      FbxWrapperNativePINVOKE.FbxLocalTime_mYear_set(swigCPtr, value);
    } 
    get {
      int ret = FbxWrapperNativePINVOKE.FbxLocalTime_mYear_get(swigCPtr);
      return ret;
    } 
  }

  public int mMonth {
    set {
      FbxWrapperNativePINVOKE.FbxLocalTime_mMonth_set(swigCPtr, value);
    } 
    get {
      int ret = FbxWrapperNativePINVOKE.FbxLocalTime_mMonth_get(swigCPtr);
      return ret;
    } 
  }

  public int mDay {
    set {
      FbxWrapperNativePINVOKE.FbxLocalTime_mDay_set(swigCPtr, value);
    } 
    get {
      int ret = FbxWrapperNativePINVOKE.FbxLocalTime_mDay_get(swigCPtr);
      return ret;
    } 
  }

  public int mHour {
    set {
      FbxWrapperNativePINVOKE.FbxLocalTime_mHour_set(swigCPtr, value);
    } 
    get {
      int ret = FbxWrapperNativePINVOKE.FbxLocalTime_mHour_get(swigCPtr);
      return ret;
    } 
  }

  public int mMinute {
    set {
      FbxWrapperNativePINVOKE.FbxLocalTime_mMinute_set(swigCPtr, value);
    } 
    get {
      int ret = FbxWrapperNativePINVOKE.FbxLocalTime_mMinute_get(swigCPtr);
      return ret;
    } 
  }

  public int mSecond {
    set {
      FbxWrapperNativePINVOKE.FbxLocalTime_mSecond_set(swigCPtr, value);
    } 
    get {
      int ret = FbxWrapperNativePINVOKE.FbxLocalTime_mSecond_get(swigCPtr);
      return ret;
    } 
  }

  public int mMillisecond {
    set {
      FbxWrapperNativePINVOKE.FbxLocalTime_mMillisecond_set(swigCPtr, value);
    } 
    get {
      int ret = FbxWrapperNativePINVOKE.FbxLocalTime_mMillisecond_get(swigCPtr);
      return ret;
    } 
  }

}

}
