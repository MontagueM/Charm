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

public class FbxCollectionExclusive : FbxCollection {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;

  internal FbxCollectionExclusive(global::System.IntPtr cPtr, bool cMemoryOwn) : base(FbxWrapperNativePINVOKE.FbxCollectionExclusive_SWIGUpcast(cPtr), cMemoryOwn) {
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(FbxCollectionExclusive obj) {
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
      FbxWrapperNativePINVOKE.FbxCollectionExclusive_ClassId_set(FbxClassId.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxCollectionExclusive_ClassId_get();
      FbxClassId ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxClassId(cPtr, false);
      return ret;
    } 
  }

  public override FbxClassId GetClassId() {
    FbxClassId ret = new FbxClassId(FbxWrapperNativePINVOKE.FbxCollectionExclusive_GetClassId(swigCPtr), true);
    return ret;
  }

  public new static FbxCollectionExclusive Create(FbxManager pManager, string pName) {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxCollectionExclusive_Create__SWIG_0(FbxManager.getCPtr(pManager), pName);
    FbxCollectionExclusive ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxCollectionExclusive(cPtr, false);
    return ret;
  }

  public new static FbxCollectionExclusive Create(FbxObject pContainer, string pName) {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxCollectionExclusive_Create__SWIG_1(FbxObject.getCPtr(pContainer), pName);
    FbxCollectionExclusive ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxCollectionExclusive(cPtr, false);
    return ret;
  }

  public override bool AddMember(FbxObject pMember) {
    bool ret = FbxWrapperNativePINVOKE.FbxCollectionExclusive_AddMember(swigCPtr, FbxObject.getCPtr(pMember));
    return ret;
  }

}

}
