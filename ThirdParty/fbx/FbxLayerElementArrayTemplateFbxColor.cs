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

public class FbxLayerElementArrayTemplateFbxColor : FbxLayerElementArray {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;

  internal FbxLayerElementArrayTemplateFbxColor(global::System.IntPtr cPtr, bool cMemoryOwn) : base(FbxWrapperNativePINVOKE.FbxLayerElementArrayTemplateFbxColor_SWIGUpcast(cPtr), cMemoryOwn) {
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(FbxLayerElementArrayTemplateFbxColor obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~FbxLayerElementArrayTemplateFbxColor() {
    Dispose();
  }

  public override void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          FbxWrapperNativePINVOKE.delete_FbxLayerElementArrayTemplateFbxColor(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
      base.Dispose();
    }
  }

  public FbxLayerElementArrayTemplateFbxColor(EFbxType pDataType) : this(FbxWrapperNativePINVOKE.new_FbxLayerElementArrayTemplateFbxColor((int)pDataType), true) {
  }

  public int Add(FbxColor pItem) {
    int ret = FbxWrapperNativePINVOKE.FbxLayerElementArrayTemplateFbxColor_Add(swigCPtr, FbxColor.getCPtr(pItem));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public int InsertAt(int pIndex, FbxColor pItem) {
    int ret = FbxWrapperNativePINVOKE.FbxLayerElementArrayTemplateFbxColor_InsertAt(swigCPtr, pIndex, FbxColor.getCPtr(pItem));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public void SetAt(int pIndex, FbxColor pItem) {
    FbxWrapperNativePINVOKE.FbxLayerElementArrayTemplateFbxColor_SetAt(swigCPtr, pIndex, FbxColor.getCPtr(pItem));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
  }

  public void SetLast(FbxColor pItem) {
    FbxWrapperNativePINVOKE.FbxLayerElementArrayTemplateFbxColor_SetLast(swigCPtr, FbxColor.getCPtr(pItem));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
  }

  public FbxColor RemoveAt(int pIndex) {
    FbxColor ret = new FbxColor(FbxWrapperNativePINVOKE.FbxLayerElementArrayTemplateFbxColor_RemoveAt(swigCPtr, pIndex), true);
    return ret;
  }

  public FbxColor RemoveLast() {
    FbxColor ret = new FbxColor(FbxWrapperNativePINVOKE.FbxLayerElementArrayTemplateFbxColor_RemoveLast(swigCPtr), true);
    return ret;
  }

  public bool RemoveIt(FbxColor pItem) {
    bool ret = FbxWrapperNativePINVOKE.FbxLayerElementArrayTemplateFbxColor_RemoveIt(swigCPtr, FbxColor.getCPtr(pItem));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public FbxColor GetAt(int pIndex) {
    FbxColor ret = new FbxColor(FbxWrapperNativePINVOKE.FbxLayerElementArrayTemplateFbxColor_GetAt(swigCPtr, pIndex), true);
    return ret;
  }

  public FbxColor GetFirst() {
    FbxColor ret = new FbxColor(FbxWrapperNativePINVOKE.FbxLayerElementArrayTemplateFbxColor_GetFirst(swigCPtr), true);
    return ret;
  }

  public FbxColor GetLast() {
    FbxColor ret = new FbxColor(FbxWrapperNativePINVOKE.FbxLayerElementArrayTemplateFbxColor_GetLast(swigCPtr), true);
    return ret;
  }

  public int Find(FbxColor pItem) {
    int ret = FbxWrapperNativePINVOKE.FbxLayerElementArrayTemplateFbxColor_Find(swigCPtr, FbxColor.getCPtr(pItem));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public int FindAfter(int pAfterIndex, FbxColor pItem) {
    int ret = FbxWrapperNativePINVOKE.FbxLayerElementArrayTemplateFbxColor_FindAfter(swigCPtr, pAfterIndex, FbxColor.getCPtr(pItem));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public int FindBefore(int pBeforeIndex, FbxColor pItem) {
    int ret = FbxWrapperNativePINVOKE.FbxLayerElementArrayTemplateFbxColor_FindBefore(swigCPtr, pBeforeIndex, FbxColor.getCPtr(pItem));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public FbxColor at(int pIndex) {
    FbxColor ret = new FbxColor(FbxWrapperNativePINVOKE.FbxLayerElementArrayTemplateFbxColor_at(swigCPtr, pIndex), true);
    return ret;
  }

  public FbxLayerElementArray assign(SWIGTYPE_p_FbxArrayT_FbxColor_t pArrayTemplate) {
    FbxLayerElementArray ret = new FbxLayerElementArray(FbxWrapperNativePINVOKE.FbxLayerElementArrayTemplateFbxColor_assign__SWIG_0(swigCPtr, SWIGTYPE_p_FbxArrayT_FbxColor_t.getCPtr(pArrayTemplate)), false);
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public FbxLayerElementArrayTemplateFbxColor assign(FbxLayerElementArrayTemplateFbxColor pArrayTemplate) {
    FbxLayerElementArrayTemplateFbxColor ret = new FbxLayerElementArrayTemplateFbxColor(FbxWrapperNativePINVOKE.FbxLayerElementArrayTemplateFbxColor_assign__SWIG_1(swigCPtr, FbxLayerElementArrayTemplateFbxColor.getCPtr(pArrayTemplate)), false);
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

}

}
