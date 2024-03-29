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

public class FbxStringListTFbxStringListItem : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal FbxStringListTFbxStringListItem(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(FbxStringListTFbxStringListItem obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~FbxStringListTFbxStringListItem() {
    Dispose();
  }

  public virtual void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          FbxWrapperNativePINVOKE.delete_FbxStringListTFbxStringListItem(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
    }
  }

  public int AddItem(FbxStringListItem pItem) {
    int ret = FbxWrapperNativePINVOKE.FbxStringListTFbxStringListItem_AddItem(swigCPtr, FbxStringListItem.getCPtr(pItem));
    return ret;
  }

  public int InsertItemAt(int pIndex, FbxStringListItem pItem) {
    int ret = FbxWrapperNativePINVOKE.FbxStringListTFbxStringListItem_InsertItemAt(swigCPtr, pIndex, FbxStringListItem.getCPtr(pItem));
    return ret;
  }

  public FbxStringListItem GetItemAt(int pIndex) {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxStringListTFbxStringListItem_GetItemAt(swigCPtr, pIndex);
    FbxStringListItem ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxStringListItem(cPtr, false);
    return ret;
  }

  public int FindItem(FbxStringListItem pItem) {
    int ret = FbxWrapperNativePINVOKE.FbxStringListTFbxStringListItem_FindItem(swigCPtr, FbxStringListItem.getCPtr(pItem));
    return ret;
  }

  public FbxStringListTFbxStringListItem() : this(FbxWrapperNativePINVOKE.new_FbxStringListTFbxStringListItem(), true) {
  }

  public void RemoveLast() {
    FbxWrapperNativePINVOKE.FbxStringListTFbxStringListItem_RemoveLast(swigCPtr);
  }

  public int GetCount() {
    int ret = FbxWrapperNativePINVOKE.FbxStringListTFbxStringListItem_GetCount(swigCPtr);
    return ret;
  }

  public FbxString at(int pIndex) {
    FbxString ret = new FbxString(FbxWrapperNativePINVOKE.FbxStringListTFbxStringListItem_at(swigCPtr, pIndex), false);
    return ret;
  }

  public ulong GetReferenceAt(int pIndex) {
    ulong ret = FbxWrapperNativePINVOKE.FbxStringListTFbxStringListItem_GetReferenceAt(swigCPtr, pIndex);
    return ret;
  }

  public void SetReferenceAt(int pIndex, ulong pRef) {
    FbxWrapperNativePINVOKE.FbxStringListTFbxStringListItem_SetReferenceAt(swigCPtr, pIndex, pRef);
  }

  public string GetStringAt(int pIndex) {
    string ret = FbxWrapperNativePINVOKE.FbxStringListTFbxStringListItem_GetStringAt(swigCPtr, pIndex);
    return ret;
  }

  public virtual bool SetStringAt(int pIndex, string pString) {
    bool ret = FbxWrapperNativePINVOKE.FbxStringListTFbxStringListItem_SetStringAt(swigCPtr, pIndex, pString);
    return ret;
  }

  public int Find(FbxStringListItem pItem) {
    int ret = FbxWrapperNativePINVOKE.FbxStringListTFbxStringListItem_Find(swigCPtr, FbxStringListItem.getCPtr(pItem));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public int FindIndex(ulong pReference) {
    int ret = FbxWrapperNativePINVOKE.FbxStringListTFbxStringListItem_FindIndex__SWIG_0(swigCPtr, pReference);
    return ret;
  }

  public int FindIndex(string pString) {
    int ret = FbxWrapperNativePINVOKE.FbxStringListTFbxStringListItem_FindIndex__SWIG_1(swigCPtr, pString);
    return ret;
  }

  public ulong FindReference(string pString) {
    ulong ret = FbxWrapperNativePINVOKE.FbxStringListTFbxStringListItem_FindReference(swigCPtr, pString);
    return ret;
  }

  public bool Remove(FbxStringListItem pItem) {
    bool ret = FbxWrapperNativePINVOKE.FbxStringListTFbxStringListItem_Remove__SWIG_0(swigCPtr, FbxStringListItem.getCPtr(pItem));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool Remove(string pString) {
    bool ret = FbxWrapperNativePINVOKE.FbxStringListTFbxStringListItem_Remove__SWIG_1(swigCPtr, pString);
    return ret;
  }

  public bool RemoveIt(FbxStringListItem pItem) {
    bool ret = FbxWrapperNativePINVOKE.FbxStringListTFbxStringListItem_RemoveIt(swigCPtr, FbxStringListItem.getCPtr(pItem));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public void Sort() {
    FbxWrapperNativePINVOKE.FbxStringListTFbxStringListItem_Sort(swigCPtr);
  }

  public SWIGTYPE_p_void FindEqual(string pString) {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxStringListTFbxStringListItem_FindEqual(swigCPtr, pString);
    SWIGTYPE_p_void ret = (cPtr == global::System.IntPtr.Zero) ? null : new SWIGTYPE_p_void(cPtr, false);
    return ret;
  }

  public SWIGTYPE_p_void FindCaseSensitive(string pString) {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxStringListTFbxStringListItem_FindCaseSensitive(swigCPtr, pString);
    SWIGTYPE_p_void ret = (cPtr == global::System.IntPtr.Zero) ? null : new SWIGTYPE_p_void(cPtr, false);
    return ret;
  }

  public int Add(string pString, ulong pItem) {
    int ret = FbxWrapperNativePINVOKE.FbxStringListTFbxStringListItem_Add__SWIG_0(swigCPtr, pString, pItem);
    return ret;
  }

  public int Add(string pString) {
    int ret = FbxWrapperNativePINVOKE.FbxStringListTFbxStringListItem_Add__SWIG_1(swigCPtr, pString);
    return ret;
  }

  public virtual int InsertAt(int pIndex, string pString, ulong pItem) {
    int ret = FbxWrapperNativePINVOKE.FbxStringListTFbxStringListItem_InsertAt__SWIG_0(swigCPtr, pIndex, pString, pItem);
    return ret;
  }

  public virtual int InsertAt(int pIndex, string pString) {
    int ret = FbxWrapperNativePINVOKE.FbxStringListTFbxStringListItem_InsertAt__SWIG_1(swigCPtr, pIndex, pString);
    return ret;
  }

  public virtual void RemoveAt(int pIndex) {
    FbxWrapperNativePINVOKE.FbxStringListTFbxStringListItem_RemoveAt(swigCPtr, pIndex);
  }

  public virtual void Clear() {
    FbxWrapperNativePINVOKE.FbxStringListTFbxStringListItem_Clear(swigCPtr);
  }

  public virtual void GetText(FbxString pText) {
    FbxWrapperNativePINVOKE.FbxStringListTFbxStringListItem_GetText(swigCPtr, FbxString.getCPtr(pText));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
  }

  public virtual int SetText(string pList) {
    int ret = FbxWrapperNativePINVOKE.FbxStringListTFbxStringListItem_SetText(swigCPtr, pList);
    return ret;
  }

}

}
