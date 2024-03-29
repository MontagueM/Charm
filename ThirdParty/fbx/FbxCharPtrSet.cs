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

public class FbxCharPtrSet : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal FbxCharPtrSet(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(FbxCharPtrSet obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~FbxCharPtrSet() {
    Dispose();
  }

  public virtual void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          FbxWrapperNativePINVOKE.delete_FbxCharPtrSet(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
    }
  }

  public FbxCharPtrSet(int pItemPerBlock) : this(FbxWrapperNativePINVOKE.new_FbxCharPtrSet__SWIG_0(pItemPerBlock), true) {
  }

  public FbxCharPtrSet() : this(FbxWrapperNativePINVOKE.new_FbxCharPtrSet__SWIG_1(), true) {
  }

  public void Add(string pReference, ulong pItem) {
    FbxWrapperNativePINVOKE.FbxCharPtrSet_Add(swigCPtr, pReference, pItem);
  }

  public bool Remove(string pReference) {
    bool ret = FbxWrapperNativePINVOKE.FbxCharPtrSet_Remove(swigCPtr, pReference);
    return ret;
  }

  public ulong Get(string pReference, SWIGTYPE_p_int PIndex) {
    ulong ret = FbxWrapperNativePINVOKE.FbxCharPtrSet_Get__SWIG_0(swigCPtr, pReference, SWIGTYPE_p_int.getCPtr(PIndex));
    return ret;
  }

  public ulong Get(string pReference) {
    ulong ret = FbxWrapperNativePINVOKE.FbxCharPtrSet_Get__SWIG_1(swigCPtr, pReference);
    return ret;
  }

  public SWIGTYPE_p_unsigned___int64 at(int pIndex) {
    SWIGTYPE_p_unsigned___int64 ret = new SWIGTYPE_p_unsigned___int64(FbxWrapperNativePINVOKE.FbxCharPtrSet_at(swigCPtr, pIndex), false);
    return ret;
  }

  public ulong GetFromIndex(int pIndex, SWIGTYPE_p_p_char pReference) {
    ulong ret = FbxWrapperNativePINVOKE.FbxCharPtrSet_GetFromIndex__SWIG_0(swigCPtr, pIndex, SWIGTYPE_p_p_char.getCPtr(pReference));
    return ret;
  }

  public ulong GetFromIndex(int pIndex) {
    ulong ret = FbxWrapperNativePINVOKE.FbxCharPtrSet_GetFromIndex__SWIG_1(swigCPtr, pIndex);
    return ret;
  }

  public void RemoveFromIndex(int pIndex) {
    FbxWrapperNativePINVOKE.FbxCharPtrSet_RemoveFromIndex(swigCPtr, pIndex);
  }

  public int GetCount() {
    int ret = FbxWrapperNativePINVOKE.FbxCharPtrSet_GetCount(swigCPtr);
    return ret;
  }

  public void Sort() {
    FbxWrapperNativePINVOKE.FbxCharPtrSet_Sort(swigCPtr);
  }

  public void Clear() {
    FbxWrapperNativePINVOKE.FbxCharPtrSet_Clear(swigCPtr);
  }

  public void SetCaseSensitive(bool pIsCaseSensitive) {
    FbxWrapperNativePINVOKE.FbxCharPtrSet_SetCaseSensitive(swigCPtr, pIsCaseSensitive);
  }

}

}
