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

public class FbxPropertyPage : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal FbxPropertyPage(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(FbxPropertyPage obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  public virtual void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          throw new global::System.MethodAccessException("C++ destructor does not have public access");
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
    }
  }

  public static FbxPropertyPage Create(FbxPropertyPage pInstanceOf) {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxPropertyPage_Create__SWIG_0(FbxPropertyPage.getCPtr(pInstanceOf));
    FbxPropertyPage ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxPropertyPage(cPtr, false);
    return ret;
  }

  public static FbxPropertyPage Create() {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxPropertyPage_Create__SWIG_1();
    FbxPropertyPage ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxPropertyPage(cPtr, false);
    return ret;
  }

  public static FbxPropertyPage Create(string pName, FbxPropertyPage pTypeInfo) {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxPropertyPage_Create__SWIG_2(pName, FbxPropertyPage.getCPtr(pTypeInfo));
    FbxPropertyPage ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxPropertyPage(cPtr, false);
    return ret;
  }

  public static FbxPropertyPage Create(string pName, EFbxType pType) {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxPropertyPage_Create__SWIG_3(pName, (int)pType);
    FbxPropertyPage ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxPropertyPage(cPtr, false);
    return ret;
  }

  public static FbxPropertyPage Create(string pName) {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxPropertyPage_Create__SWIG_4(pName);
    FbxPropertyPage ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxPropertyPage(cPtr, false);
    return ret;
  }

  public void Destroy() {
    FbxWrapperNativePINVOKE.FbxPropertyPage_Destroy(swigCPtr);
  }

  public string GetName(int pId) {
    string ret = FbxWrapperNativePINVOKE.FbxPropertyPage_GetName__SWIG_0(swigCPtr, pId);
    return ret;
  }

  public string GetName() {
    string ret = FbxWrapperNativePINVOKE.FbxPropertyPage_GetName__SWIG_1(swigCPtr);
    return ret;
  }

  public string GetLabel(int pId) {
    string ret = FbxWrapperNativePINVOKE.FbxPropertyPage_GetLabel__SWIG_0(swigCPtr, pId);
    return ret;
  }

  public string GetLabel() {
    string ret = FbxWrapperNativePINVOKE.FbxPropertyPage_GetLabel__SWIG_1(swigCPtr);
    return ret;
  }

  public bool SetLabel(int pId, string pLabel) {
    bool ret = FbxWrapperNativePINVOKE.FbxPropertyPage_SetLabel__SWIG_0(swigCPtr, pId, pLabel);
    return ret;
  }

  public bool SetLabel(int pId) {
    bool ret = FbxWrapperNativePINVOKE.FbxPropertyPage_SetLabel__SWIG_1(swigCPtr, pId);
    return ret;
  }

  public bool SetLabel() {
    bool ret = FbxWrapperNativePINVOKE.FbxPropertyPage_SetLabel__SWIG_2(swigCPtr);
    return ret;
  }

  public SWIGTYPE_p_void GetUserData(int pId) {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxPropertyPage_GetUserData__SWIG_0(swigCPtr, pId);
    SWIGTYPE_p_void ret = (cPtr == global::System.IntPtr.Zero) ? null : new SWIGTYPE_p_void(cPtr, false);
    return ret;
  }

  public SWIGTYPE_p_void GetUserData() {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxPropertyPage_GetUserData__SWIG_1(swigCPtr);
    SWIGTYPE_p_void ret = (cPtr == global::System.IntPtr.Zero) ? null : new SWIGTYPE_p_void(cPtr, false);
    return ret;
  }

  public bool SetUserData(int pId, SWIGTYPE_p_void pUserData) {
    bool ret = FbxWrapperNativePINVOKE.FbxPropertyPage_SetUserData__SWIG_0(swigCPtr, pId, SWIGTYPE_p_void.getCPtr(pUserData));
    return ret;
  }

  public bool SetUserData(int pId) {
    bool ret = FbxWrapperNativePINVOKE.FbxPropertyPage_SetUserData__SWIG_1(swigCPtr, pId);
    return ret;
  }

  public bool SetUserData() {
    bool ret = FbxWrapperNativePINVOKE.FbxPropertyPage_SetUserData__SWIG_2(swigCPtr);
    return ret;
  }

  public int GetUserTag(int pId) {
    int ret = FbxWrapperNativePINVOKE.FbxPropertyPage_GetUserTag__SWIG_0(swigCPtr, pId);
    return ret;
  }

  public int GetUserTag() {
    int ret = FbxWrapperNativePINVOKE.FbxPropertyPage_GetUserTag__SWIG_1(swigCPtr);
    return ret;
  }

  public bool SetUserTag(int pId, int pUserTag) {
    bool ret = FbxWrapperNativePINVOKE.FbxPropertyPage_SetUserTag__SWIG_0(swigCPtr, pId, pUserTag);
    return ret;
  }

  public bool SetUserTag(int pId) {
    bool ret = FbxWrapperNativePINVOKE.FbxPropertyPage_SetUserTag__SWIG_1(swigCPtr, pId);
    return ret;
  }

  public bool SetUserTag() {
    bool ret = FbxWrapperNativePINVOKE.FbxPropertyPage_SetUserTag__SWIG_2(swigCPtr);
    return ret;
  }

  public EFbxType GetType(int pId) {
    EFbxType ret = (EFbxType)FbxWrapperNativePINVOKE.FbxPropertyPage_GetType__SWIG_0(swigCPtr, pId);
    return ret;
  }

  public EFbxType GetType() {
    EFbxType ret = (EFbxType)FbxWrapperNativePINVOKE.FbxPropertyPage_GetType__SWIG_1(swigCPtr);
    return ret;
  }

  public int GetParent(int pId) {
    int ret = FbxWrapperNativePINVOKE.FbxPropertyPage_GetParent__SWIG_0(swigCPtr, pId);
    return ret;
  }

  public int GetParent() {
    int ret = FbxWrapperNativePINVOKE.FbxPropertyPage_GetParent__SWIG_1(swigCPtr);
    return ret;
  }

  public FbxPropertyPage GetTypeInfo(int pId) {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxPropertyPage_GetTypeInfo__SWIG_0(swigCPtr, pId);
    FbxPropertyPage ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxPropertyPage(cPtr, false);
    return ret;
  }

  public FbxPropertyPage GetTypeInfo() {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxPropertyPage_GetTypeInfo__SWIG_1(swigCPtr);
    FbxPropertyPage ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxPropertyPage(cPtr, false);
    return ret;
  }

  public int Add(int pParentId, string pName, EFbxType pType) {
    int ret = FbxWrapperNativePINVOKE.FbxPropertyPage_Add__SWIG_0(swigCPtr, pParentId, pName, (int)pType);
    return ret;
  }

  public int Add(int pParentId, string pName, FbxPropertyPage pTypeInfo) {
    int ret = FbxWrapperNativePINVOKE.FbxPropertyPage_Add__SWIG_1(swigCPtr, pParentId, pName, FbxPropertyPage.getCPtr(pTypeInfo));
    return ret;
  }

  public bool Reparent(int arg0, int arg1) {
    bool ret = FbxWrapperNativePINVOKE.FbxPropertyPage_Reparent(swigCPtr, arg0, arg1);
    return ret;
  }

  public bool IsChildOf(int pId, int pParentId) {
    bool ret = FbxWrapperNativePINVOKE.FbxPropertyPage_IsChildOf(swigCPtr, pId, pParentId);
    return ret;
  }

  public bool IsDescendentOf(int pId, int pAncestorId) {
    bool ret = FbxWrapperNativePINVOKE.FbxPropertyPage_IsDescendentOf(swigCPtr, pId, pAncestorId);
    return ret;
  }

  public int GetChild(int pParentId) {
    int ret = FbxWrapperNativePINVOKE.FbxPropertyPage_GetChild__SWIG_0(swigCPtr, pParentId);
    return ret;
  }

  public int GetChild() {
    int ret = FbxWrapperNativePINVOKE.FbxPropertyPage_GetChild__SWIG_1(swigCPtr);
    return ret;
  }

  public int GetSibling(int pId) {
    int ret = FbxWrapperNativePINVOKE.FbxPropertyPage_GetSibling(swigCPtr, pId);
    return ret;
  }

  public int GetFirstDescendent(int pAnscestorId) {
    int ret = FbxWrapperNativePINVOKE.FbxPropertyPage_GetFirstDescendent__SWIG_0(swigCPtr, pAnscestorId);
    return ret;
  }

  public int GetFirstDescendent() {
    int ret = FbxWrapperNativePINVOKE.FbxPropertyPage_GetFirstDescendent__SWIG_1(swigCPtr);
    return ret;
  }

  public int GetNextDescendent(int pAnscestorId, int pId) {
    int ret = FbxWrapperNativePINVOKE.FbxPropertyPage_GetNextDescendent(swigCPtr, pAnscestorId, pId);
    return ret;
  }

  public int FastFind(int pId, string pName, FbxPropertyPage pTypeInfo, bool pCaseSensitive) {
    int ret = FbxWrapperNativePINVOKE.FbxPropertyPage_FastFind(swigCPtr, pId, pName, FbxPropertyPage.getCPtr(pTypeInfo), pCaseSensitive);
    return ret;
  }

  public int Find(int pId, string pName, FbxPropertyPage pTypeInfo, bool pCaseSensitive, string pChildrenSeparators) {
    int ret = FbxWrapperNativePINVOKE.FbxPropertyPage_Find(swigCPtr, pId, pName, FbxPropertyPage.getCPtr(pTypeInfo), pCaseSensitive, pChildrenSeparators);
    return ret;
  }

  public int AddEnumValue(int pId, string pStringValue) {
    int ret = FbxWrapperNativePINVOKE.FbxPropertyPage_AddEnumValue(swigCPtr, pId, pStringValue);
    return ret;
  }

  public void InsertEnumValue(int pId, int pIndex, string pStringValue) {
    FbxWrapperNativePINVOKE.FbxPropertyPage_InsertEnumValue(swigCPtr, pId, pIndex, pStringValue);
  }

  public int GetEnumCount(int pId) {
    int ret = FbxWrapperNativePINVOKE.FbxPropertyPage_GetEnumCount(swigCPtr, pId);
    return ret;
  }

  public void SetEnumValue(int pId, int pIndex, string pStringValue) {
    FbxWrapperNativePINVOKE.FbxPropertyPage_SetEnumValue(swigCPtr, pId, pIndex, pStringValue);
  }

  public void RemoveEnumValue(int pId, int pIndex) {
    FbxWrapperNativePINVOKE.FbxPropertyPage_RemoveEnumValue(swigCPtr, pId, pIndex);
  }

  public string GetEnumValue(int pId, int pIndex) {
    string ret = FbxWrapperNativePINVOKE.FbxPropertyPage_GetEnumValue(swigCPtr, pId, pIndex);
    return ret;
  }

  public void ClearConnectCache(int pId) {
    FbxWrapperNativePINVOKE.FbxPropertyPage_ClearConnectCache(swigCPtr, pId);
  }

  public void WipeAllConnections(int pId) {
    FbxWrapperNativePINVOKE.FbxPropertyPage_WipeAllConnections(swigCPtr, pId);
  }

  public bool ConnectSrc(int pDstId, FbxPropertyPage pSrcPage, int pSrcId, FbxConnection.EType pType) {
    bool ret = FbxWrapperNativePINVOKE.FbxPropertyPage_ConnectSrc(swigCPtr, pDstId, FbxPropertyPage.getCPtr(pSrcPage), pSrcId, (int)pType);
    return ret;
  }

  public bool DisconnectSrc(int pDstId, FbxPropertyPage pSrcPage, int pSrcId) {
    bool ret = FbxWrapperNativePINVOKE.FbxPropertyPage_DisconnectSrc(swigCPtr, pDstId, FbxPropertyPage.getCPtr(pSrcPage), pSrcId);
    return ret;
  }

  public bool IsConnectedSrc(int pDstId, FbxPropertyPage pSrcPage, int pSrcId) {
    bool ret = FbxWrapperNativePINVOKE.FbxPropertyPage_IsConnectedSrc(swigCPtr, pDstId, FbxPropertyPage.getCPtr(pSrcPage), pSrcId);
    return ret;
  }

  public int GetSrcCount(int pId, FbxConnectionPointFilter pFilter) {
    int ret = FbxWrapperNativePINVOKE.FbxPropertyPage_GetSrcCount(swigCPtr, pId, FbxConnectionPointFilter.getCPtr(pFilter));
    return ret;
  }

  public bool GetSrc(int pId, int pIndex, FbxConnectionPointFilter pFilter, SWIGTYPE_p_p_FbxPropertyPage pSrcPage, SWIGTYPE_p_int pSrcId) {
    bool ret = FbxWrapperNativePINVOKE.FbxPropertyPage_GetSrc(swigCPtr, pId, pIndex, FbxConnectionPointFilter.getCPtr(pFilter), SWIGTYPE_p_p_FbxPropertyPage.getCPtr(pSrcPage), SWIGTYPE_p_int.getCPtr(pSrcId));
    return ret;
  }

  public bool ConnectDst(int pSrcId, FbxPropertyPage pDstPage, int pDstId, FbxConnection.EType pType) {
    bool ret = FbxWrapperNativePINVOKE.FbxPropertyPage_ConnectDst(swigCPtr, pSrcId, FbxPropertyPage.getCPtr(pDstPage), pDstId, (int)pType);
    return ret;
  }

  public bool DisconnectDst(int pSrcId, FbxPropertyPage pDstPage, int pDstId) {
    bool ret = FbxWrapperNativePINVOKE.FbxPropertyPage_DisconnectDst(swigCPtr, pSrcId, FbxPropertyPage.getCPtr(pDstPage), pDstId);
    return ret;
  }

  public bool IsConnectedDst(int pSrcId, FbxPropertyPage pDstPage, int pDstId) {
    bool ret = FbxWrapperNativePINVOKE.FbxPropertyPage_IsConnectedDst(swigCPtr, pSrcId, FbxPropertyPage.getCPtr(pDstPage), pDstId);
    return ret;
  }

  public int GetDstCount(int pId, FbxConnectionPointFilter pFilter) {
    int ret = FbxWrapperNativePINVOKE.FbxPropertyPage_GetDstCount(swigCPtr, pId, FbxConnectionPointFilter.getCPtr(pFilter));
    return ret;
  }

  public bool GetDst(int pId, int pIndex, FbxConnectionPointFilter pFilter, SWIGTYPE_p_p_FbxPropertyPage pDstPage, SWIGTYPE_p_int pDstId) {
    bool ret = FbxWrapperNativePINVOKE.FbxPropertyPage_GetDst(swigCPtr, pId, pIndex, FbxConnectionPointFilter.getCPtr(pFilter), SWIGTYPE_p_p_FbxPropertyPage.getCPtr(pDstPage), SWIGTYPE_p_int.getCPtr(pDstId));
    return ret;
  }

  public bool HasMinMax(int pId, FbxPropertyInfo.EValueIndex pValueId) {
    bool ret = FbxWrapperNativePINVOKE.FbxPropertyPage_HasMinMax(swigCPtr, pId, (int)pValueId);
    return ret;
  }

  public bool GetMinMax(int pId, FbxPropertyInfo.EValueIndex pValueId, SWIGTYPE_p_void pValue, EFbxType pValueType) {
    bool ret = FbxWrapperNativePINVOKE.FbxPropertyPage_GetMinMax(swigCPtr, pId, (int)pValueId, SWIGTYPE_p_void.getCPtr(pValue), (int)pValueType);
    return ret;
  }

  public bool SetMinMax(int pId, FbxPropertyInfo.EValueIndex pValueId, SWIGTYPE_p_void pValue, EFbxType pValueType) {
    bool ret = FbxWrapperNativePINVOKE.FbxPropertyPage_SetMinMax(swigCPtr, pId, (int)pValueId, SWIGTYPE_p_void.getCPtr(pValue), (int)pValueType);
    return ret;
  }

  public bool Get(int pId, SWIGTYPE_p_void pValue, EFbxType pValueType) {
    bool ret = FbxWrapperNativePINVOKE.FbxPropertyPage_Get(swigCPtr, pId, SWIGTYPE_p_void.getCPtr(pValue), (int)pValueType);
    return ret;
  }

  public bool Set(int pId, SWIGTYPE_p_void pValue, EFbxType pValueType, bool pCheckValueEquality) {
    bool ret = FbxWrapperNativePINVOKE.FbxPropertyPage_Set(swigCPtr, pId, SWIGTYPE_p_void.getCPtr(pValue), (int)pValueType, pCheckValueEquality);
    return ret;
  }

  public FbxPropertyFlags.EInheritType GetValueInherit(int pId, bool pCheckInstanceOf) {
    FbxPropertyFlags.EInheritType ret = (FbxPropertyFlags.EInheritType)FbxWrapperNativePINVOKE.FbxPropertyPage_GetValueInherit(swigCPtr, pId, pCheckInstanceOf);
    return ret;
  }

  public bool SetValueInherit(int pId, FbxPropertyFlags.EInheritType pType) {
    bool ret = FbxWrapperNativePINVOKE.FbxPropertyPage_SetValueInherit(swigCPtr, pId, (int)pType);
    return ret;
  }

  public bool GetDefaultValue(int pId, SWIGTYPE_p_void pValue, EFbxType pValueType) {
    bool ret = FbxWrapperNativePINVOKE.FbxPropertyPage_GetDefaultValue(swigCPtr, pId, SWIGTYPE_p_void.getCPtr(pValue), (int)pValueType);
    return ret;
  }

  public void SetDataPtr(SWIGTYPE_p_void pDataPtr) {
    FbxWrapperNativePINVOKE.FbxPropertyPage_SetDataPtr(swigCPtr, SWIGTYPE_p_void.getCPtr(pDataPtr));
  }

  public SWIGTYPE_p_void GetDataPtr() {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxPropertyPage_GetDataPtr(swigCPtr);
    SWIGTYPE_p_void ret = (cPtr == global::System.IntPtr.Zero) ? null : new SWIGTYPE_p_void(cPtr, false);
    return ret;
  }

  public void PushPropertiesToParentInstance() {
    FbxWrapperNativePINVOKE.FbxPropertyPage_PushPropertiesToParentInstance(swigCPtr);
  }

  public FbxPropertyPage GetInstanceOf() {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxPropertyPage_GetInstanceOf__SWIG_0(swigCPtr);
    FbxPropertyPage ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxPropertyPage(cPtr, false);
    return ret;
  }

  public SWIGTYPE_p_FbxArrayT_FbxPropertyPage_p_t GetInstances() {
    SWIGTYPE_p_FbxArrayT_FbxPropertyPage_p_t ret = new SWIGTYPE_p_FbxArrayT_FbxPropertyPage_p_t(FbxWrapperNativePINVOKE.FbxPropertyPage_GetInstances__SWIG_0(swigCPtr), false);
    return ret;
  }

  public FbxPropertyFlags.EFlags GetFlags(int pId) {
    FbxPropertyFlags.EFlags ret = (FbxPropertyFlags.EFlags)FbxWrapperNativePINVOKE.FbxPropertyPage_GetFlags__SWIG_0(swigCPtr, pId);
    return ret;
  }

  public FbxPropertyFlags.EFlags GetFlags() {
    FbxPropertyFlags.EFlags ret = (FbxPropertyFlags.EFlags)FbxWrapperNativePINVOKE.FbxPropertyPage_GetFlags__SWIG_1(swigCPtr);
    return ret;
  }

  public bool ModifyFlags(int pId, FbxPropertyFlags.EFlags pFlags, bool pValue, bool pCheckFlagEquality) {
    bool ret = FbxWrapperNativePINVOKE.FbxPropertyPage_ModifyFlags__SWIG_0(swigCPtr, pId, (int)pFlags, pValue, pCheckFlagEquality);
    return ret;
  }

  public bool ModifyFlags(int pId, FbxPropertyFlags.EFlags pFlags, bool pValue) {
    bool ret = FbxWrapperNativePINVOKE.FbxPropertyPage_ModifyFlags__SWIG_1(swigCPtr, pId, (int)pFlags, pValue);
    return ret;
  }

  public bool ModifyFlags(int pId, FbxPropertyFlags.EFlags pFlags) {
    bool ret = FbxWrapperNativePINVOKE.FbxPropertyPage_ModifyFlags__SWIG_2(swigCPtr, pId, (int)pFlags);
    return ret;
  }

  public bool ModifyFlags(int pId) {
    bool ret = FbxWrapperNativePINVOKE.FbxPropertyPage_ModifyFlags__SWIG_3(swigCPtr, pId);
    return ret;
  }

  public bool ModifyFlags() {
    bool ret = FbxWrapperNativePINVOKE.FbxPropertyPage_ModifyFlags__SWIG_4(swigCPtr);
    return ret;
  }

  public FbxPropertyFlags.EInheritType GetFlagsInheritType(FbxPropertyFlags.EFlags pFlags, bool pCheckInstanceOf, int pId) {
    FbxPropertyFlags.EInheritType ret = (FbxPropertyFlags.EInheritType)FbxWrapperNativePINVOKE.FbxPropertyPage_GetFlagsInheritType__SWIG_0(swigCPtr, (int)pFlags, pCheckInstanceOf, pId);
    return ret;
  }

  public FbxPropertyFlags.EInheritType GetFlagsInheritType(FbxPropertyFlags.EFlags pFlags, bool pCheckInstanceOf) {
    FbxPropertyFlags.EInheritType ret = (FbxPropertyFlags.EInheritType)FbxWrapperNativePINVOKE.FbxPropertyPage_GetFlagsInheritType__SWIG_1(swigCPtr, (int)pFlags, pCheckInstanceOf);
    return ret;
  }

  public bool SetFlagsInheritType(FbxPropertyFlags.EInheritType pInheritType, FbxPropertyFlags.EFlags pFlags, int pId) {
    bool ret = FbxWrapperNativePINVOKE.FbxPropertyPage_SetFlagsInheritType__SWIG_0(swigCPtr, (int)pInheritType, (int)pFlags, pId);
    return ret;
  }

  public bool SetFlagsInheritType(FbxPropertyFlags.EInheritType pInheritType, FbxPropertyFlags.EFlags pFlags) {
    bool ret = FbxWrapperNativePINVOKE.FbxPropertyPage_SetFlagsInheritType__SWIG_1(swigCPtr, (int)pInheritType, (int)pFlags);
    return ret;
  }

  public void BeginCreateOrFindProperty() {
    FbxWrapperNativePINVOKE.FbxPropertyPage_BeginCreateOrFindProperty(swigCPtr);
  }

  public void EndCreateOrFindProperty() {
    FbxWrapperNativePINVOKE.FbxPropertyPage_EndCreateOrFindProperty(swigCPtr);
  }

  public enum EValueIndex {
    eValueMin,
    eValueSoftMin,
    eValueMax,
    eValueSoftMax,
    eValueCount
  }

}

}
