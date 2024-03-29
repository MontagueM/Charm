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

public class FbxLayerElementArrayTemplateFbxTexturePtr : FbxLayerElementArray {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;

  internal FbxLayerElementArrayTemplateFbxTexturePtr(global::System.IntPtr cPtr, bool cMemoryOwn) : base(FbxWrapperNativePINVOKE.FbxLayerElementArrayTemplateFbxTexturePtr_SWIGUpcast(cPtr), cMemoryOwn) {
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(FbxLayerElementArrayTemplateFbxTexturePtr obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~FbxLayerElementArrayTemplateFbxTexturePtr() {
    Dispose();
  }

  public override void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          FbxWrapperNativePINVOKE.delete_FbxLayerElementArrayTemplateFbxTexturePtr(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
      base.Dispose();
    }
  }

  public FbxLayerElementArrayTemplateFbxTexturePtr(EFbxType pDataType) : this(FbxWrapperNativePINVOKE.new_FbxLayerElementArrayTemplateFbxTexturePtr((int)pDataType), true) {
  }

  public int Add(FbxTexture pItem) {
    int ret = FbxWrapperNativePINVOKE.FbxLayerElementArrayTemplateFbxTexturePtr_Add(swigCPtr, FbxTexture.getCPtr(pItem));
    return ret;
  }

  public int InsertAt(int pIndex, FbxTexture pItem) {
    int ret = FbxWrapperNativePINVOKE.FbxLayerElementArrayTemplateFbxTexturePtr_InsertAt(swigCPtr, pIndex, FbxTexture.getCPtr(pItem));
    return ret;
  }

  public void SetAt(int pIndex, FbxTexture pItem) {
    FbxWrapperNativePINVOKE.FbxLayerElementArrayTemplateFbxTexturePtr_SetAt(swigCPtr, pIndex, FbxTexture.getCPtr(pItem));
  }

  public void SetLast(FbxTexture pItem) {
    FbxWrapperNativePINVOKE.FbxLayerElementArrayTemplateFbxTexturePtr_SetLast(swigCPtr, FbxTexture.getCPtr(pItem));
  }

  public FbxTexture RemoveAt(int pIndex) {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxLayerElementArrayTemplateFbxTexturePtr_RemoveAt(swigCPtr, pIndex);
    FbxTexture ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxTexture(cPtr, false);
    return ret;
  }

  public FbxTexture RemoveLast() {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxLayerElementArrayTemplateFbxTexturePtr_RemoveLast(swigCPtr);
    FbxTexture ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxTexture(cPtr, false);
    return ret;
  }

  public bool RemoveIt(FbxTexture pItem) {
    bool ret = FbxWrapperNativePINVOKE.FbxLayerElementArrayTemplateFbxTexturePtr_RemoveIt(swigCPtr, FbxTexture.getCPtr(pItem));
    return ret;
  }

  public FbxTexture GetAt(int pIndex) {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxLayerElementArrayTemplateFbxTexturePtr_GetAt(swigCPtr, pIndex);
    FbxTexture ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxTexture(cPtr, false);
    return ret;
  }

  public FbxTexture GetFirst() {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxLayerElementArrayTemplateFbxTexturePtr_GetFirst(swigCPtr);
    FbxTexture ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxTexture(cPtr, false);
    return ret;
  }

  public FbxTexture GetLast() {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxLayerElementArrayTemplateFbxTexturePtr_GetLast(swigCPtr);
    FbxTexture ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxTexture(cPtr, false);
    return ret;
  }

  public int Find(FbxTexture pItem) {
    int ret = FbxWrapperNativePINVOKE.FbxLayerElementArrayTemplateFbxTexturePtr_Find(swigCPtr, FbxTexture.getCPtr(pItem));
    return ret;
  }

  public int FindAfter(int pAfterIndex, FbxTexture pItem) {
    int ret = FbxWrapperNativePINVOKE.FbxLayerElementArrayTemplateFbxTexturePtr_FindAfter(swigCPtr, pAfterIndex, FbxTexture.getCPtr(pItem));
    return ret;
  }

  public int FindBefore(int pBeforeIndex, FbxTexture pItem) {
    int ret = FbxWrapperNativePINVOKE.FbxLayerElementArrayTemplateFbxTexturePtr_FindBefore(swigCPtr, pBeforeIndex, FbxTexture.getCPtr(pItem));
    return ret;
  }

  public FbxTexture at(int pIndex) {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxLayerElementArrayTemplateFbxTexturePtr_at(swigCPtr, pIndex);
    FbxTexture ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxTexture(cPtr, false);
    return ret;
  }

  public FbxLayerElementArray assign(SWIGTYPE_p_FbxArrayT_FbxTexture_p_t pArrayTemplate) {
    FbxLayerElementArray ret = new FbxLayerElementArray(FbxWrapperNativePINVOKE.FbxLayerElementArrayTemplateFbxTexturePtr_assign__SWIG_0(swigCPtr, SWIGTYPE_p_FbxArrayT_FbxTexture_p_t.getCPtr(pArrayTemplate)), false);
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public FbxLayerElementArrayTemplateFbxTexturePtr assign(FbxLayerElementArrayTemplateFbxTexturePtr pArrayTemplate) {
    FbxLayerElementArrayTemplateFbxTexturePtr ret = new FbxLayerElementArrayTemplateFbxTexturePtr(FbxWrapperNativePINVOKE.FbxLayerElementArrayTemplateFbxTexturePtr_assign__SWIG_1(swigCPtr, FbxLayerElementArrayTemplateFbxTexturePtr.getCPtr(pArrayTemplate)), false);
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

}

}
