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

public class FbxColor : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal FbxColor(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(FbxColor obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~FbxColor() {
    Dispose();
  }

  public virtual void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          FbxWrapperNativePINVOKE.delete_FbxColor(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
    }
  }

  public FbxColor() : this(FbxWrapperNativePINVOKE.new_FbxColor__SWIG_0(), true) {
  }

  public FbxColor(double pRed, double pGreen, double pBlue, double pAlpha) : this(FbxWrapperNativePINVOKE.new_FbxColor__SWIG_1(pRed, pGreen, pBlue, pAlpha), true) {
  }

  public FbxColor(double pRed, double pGreen, double pBlue) : this(FbxWrapperNativePINVOKE.new_FbxColor__SWIG_2(pRed, pGreen, pBlue), true) {
  }

  public FbxColor(FbxDouble3 pRGB, double pAlpha) : this(FbxWrapperNativePINVOKE.new_FbxColor__SWIG_3(FbxDouble3.getCPtr(pRGB), pAlpha), true) {
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
  }

  public FbxColor(FbxDouble3 pRGB) : this(FbxWrapperNativePINVOKE.new_FbxColor__SWIG_4(FbxDouble3.getCPtr(pRGB)), true) {
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
  }

  public FbxColor(FbxDouble4 pRGBA) : this(FbxWrapperNativePINVOKE.new_FbxColor__SWIG_5(FbxDouble4.getCPtr(pRGBA)), true) {
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
  }

  public void Set(double pRed, double pGreen, double pBlue, double pAlpha) {
    FbxWrapperNativePINVOKE.FbxColor_Set__SWIG_0(swigCPtr, pRed, pGreen, pBlue, pAlpha);
  }

  public void Set(double pRed, double pGreen, double pBlue) {
    FbxWrapperNativePINVOKE.FbxColor_Set__SWIG_1(swigCPtr, pRed, pGreen, pBlue);
  }

  public bool IsValid() {
    bool ret = FbxWrapperNativePINVOKE.FbxColor_IsValid(swigCPtr);
    return ret;
  }

  public SWIGTYPE_p_double at(int pIndex) {
    SWIGTYPE_p_double ret = new SWIGTYPE_p_double(FbxWrapperNativePINVOKE.FbxColor_at__SWIG_0(swigCPtr, pIndex), false);
    return ret;
  }

  public FbxColor assign(FbxColor pColor) {
    FbxColor ret = new FbxColor(FbxWrapperNativePINVOKE.FbxColor_assign__SWIG_0(swigCPtr, FbxColor.getCPtr(pColor)), false);
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public FbxColor assign(FbxDouble3 pColor) {
    FbxColor ret = new FbxColor(FbxWrapperNativePINVOKE.FbxColor_assign__SWIG_1(swigCPtr, FbxDouble3.getCPtr(pColor)), false);
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public FbxColor assign(FbxDouble4 pColor) {
    FbxColor ret = new FbxColor(FbxWrapperNativePINVOKE.FbxColor_assign__SWIG_2(swigCPtr, FbxDouble4.getCPtr(pColor)), false);
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool eq(FbxColor pColor) {
    bool ret = FbxWrapperNativePINVOKE.FbxColor_eq(swigCPtr, FbxColor.getCPtr(pColor));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool ne(FbxColor pColor) {
    bool ret = FbxWrapperNativePINVOKE.FbxColor_ne(swigCPtr, FbxColor.getCPtr(pColor));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public double mRed {
    set {
      FbxWrapperNativePINVOKE.FbxColor_mRed_set(swigCPtr, value);
    } 
    get {
      double ret = FbxWrapperNativePINVOKE.FbxColor_mRed_get(swigCPtr);
      return ret;
    } 
  }

  public double mGreen {
    set {
      FbxWrapperNativePINVOKE.FbxColor_mGreen_set(swigCPtr, value);
    } 
    get {
      double ret = FbxWrapperNativePINVOKE.FbxColor_mGreen_get(swigCPtr);
      return ret;
    } 
  }

  public double mBlue {
    set {
      FbxWrapperNativePINVOKE.FbxColor_mBlue_set(swigCPtr, value);
    } 
    get {
      double ret = FbxWrapperNativePINVOKE.FbxColor_mBlue_get(swigCPtr);
      return ret;
    } 
  }

  public double mAlpha {
    set {
      FbxWrapperNativePINVOKE.FbxColor_mAlpha_set(swigCPtr, value);
    } 
    get {
      double ret = FbxWrapperNativePINVOKE.FbxColor_mAlpha_get(swigCPtr);
      return ret;
    } 
  }

}

}
