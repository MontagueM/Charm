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

public class FbxPropertyTFbxDouble : FbxProperty {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;

  internal FbxPropertyTFbxDouble(global::System.IntPtr cPtr, bool cMemoryOwn) : base(FbxWrapperNativePINVOKE.FbxPropertyTFbxDouble_SWIGUpcast(cPtr), cMemoryOwn) {
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(FbxPropertyTFbxDouble obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~FbxPropertyTFbxDouble() {
    Dispose();
  }

  public override void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          FbxWrapperNativePINVOKE.delete_FbxPropertyTFbxDouble(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
      base.Dispose();
    }
  }

  public FbxProperty StaticInit(FbxObject pObject, string pName, double pValue, bool pForceSet, FbxPropertyFlags.EFlags pFlags) {
    FbxProperty ret = new FbxProperty(FbxWrapperNativePINVOKE.FbxPropertyTFbxDouble_StaticInit__SWIG_0(swigCPtr, FbxObject.getCPtr(pObject), pName, pValue, pForceSet, (int)pFlags), false);
    return ret;
  }

  public FbxProperty StaticInit(FbxObject pObject, string pName, double pValue, bool pForceSet) {
    FbxProperty ret = new FbxProperty(FbxWrapperNativePINVOKE.FbxPropertyTFbxDouble_StaticInit__SWIG_1(swigCPtr, FbxObject.getCPtr(pObject), pName, pValue, pForceSet), false);
    return ret;
  }

  public FbxProperty StaticInit(FbxObject pObject, string pName, FbxDataType pDataType, double pValue, bool pForceSet, FbxPropertyFlags.EFlags pFlags) {
    FbxProperty ret = new FbxProperty(FbxWrapperNativePINVOKE.FbxPropertyTFbxDouble_StaticInit__SWIG_2(swigCPtr, FbxObject.getCPtr(pObject), pName, FbxDataType.getCPtr(pDataType), pValue, pForceSet, (int)pFlags), false);
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public FbxProperty StaticInit(FbxObject pObject, string pName, FbxDataType pDataType, double pValue, bool pForceSet) {
    FbxProperty ret = new FbxProperty(FbxWrapperNativePINVOKE.FbxPropertyTFbxDouble_StaticInit__SWIG_3(swigCPtr, FbxObject.getCPtr(pObject), pName, FbxDataType.getCPtr(pDataType), pValue, pForceSet), false);
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public FbxProperty StaticInit(FbxProperty pCompound, string pName, FbxDataType pDataType, double pValue, bool pForceSet, FbxPropertyFlags.EFlags pFlags) {
    FbxProperty ret = new FbxProperty(FbxWrapperNativePINVOKE.FbxPropertyTFbxDouble_StaticInit__SWIG_4(swigCPtr, FbxProperty.getCPtr(pCompound), pName, FbxDataType.getCPtr(pDataType), pValue, pForceSet, (int)pFlags), false);
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public FbxProperty StaticInit(FbxProperty pCompound, string pName, FbxDataType pDataType, double pValue, bool pForceSet) {
    FbxProperty ret = new FbxProperty(FbxWrapperNativePINVOKE.FbxPropertyTFbxDouble_StaticInit__SWIG_5(swigCPtr, FbxProperty.getCPtr(pCompound), pName, FbxDataType.getCPtr(pDataType), pValue, pForceSet), false);
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public FbxProperty StaticInit(FbxProperty pCompound, string pName, FbxDataType pDataType, double pValue) {
    FbxProperty ret = new FbxProperty(FbxWrapperNativePINVOKE.FbxPropertyTFbxDouble_StaticInit__SWIG_6(swigCPtr, FbxProperty.getCPtr(pCompound), pName, FbxDataType.getCPtr(pDataType), pValue), false);
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public FbxPropertyTFbxDouble Set(double pValue) {
    FbxPropertyTFbxDouble ret = new FbxPropertyTFbxDouble(FbxWrapperNativePINVOKE.FbxPropertyTFbxDouble_Set(swigCPtr, pValue), false);
    return ret;
  }

  public double Get() {
    double ret = FbxWrapperNativePINVOKE.FbxPropertyTFbxDouble_Get(swigCPtr);
    return ret;
  }

  public FbxPropertyTFbxDouble assign(double pValue) {
    FbxPropertyTFbxDouble ret = new FbxPropertyTFbxDouble(FbxWrapperNativePINVOKE.FbxPropertyTFbxDouble_assign(swigCPtr, pValue), false);
    return ret;
  }

  public double ToType() {
    double ret = FbxWrapperNativePINVOKE.FbxPropertyTFbxDouble_ToType(swigCPtr);
    return ret;
  }

  public double EvaluateValue(FbxTime pTime, bool pForceEval) {
    double ret = FbxWrapperNativePINVOKE.FbxPropertyTFbxDouble_EvaluateValue__SWIG_0(swigCPtr, FbxTime.getCPtr(pTime), pForceEval);
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public double EvaluateValue(FbxTime pTime) {
    double ret = FbxWrapperNativePINVOKE.FbxPropertyTFbxDouble_EvaluateValue__SWIG_1(swigCPtr, FbxTime.getCPtr(pTime));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public double EvaluateValue() {
    double ret = FbxWrapperNativePINVOKE.FbxPropertyTFbxDouble_EvaluateValue__SWIG_2(swigCPtr);
    return ret;
  }

  public FbxPropertyTFbxDouble() : this(FbxWrapperNativePINVOKE.new_FbxPropertyTFbxDouble__SWIG_0(), true) {
  }

  public FbxPropertyTFbxDouble(FbxProperty pProperty) : this(FbxWrapperNativePINVOKE.new_FbxPropertyTFbxDouble__SWIG_1(FbxProperty.getCPtr(pProperty)), true) {
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
  }

}

}