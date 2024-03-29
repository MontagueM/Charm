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

public class FbxUserNotification : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal FbxUserNotification(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(FbxUserNotification obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~FbxUserNotification() {
    Dispose();
  }

  public virtual void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          FbxWrapperNativePINVOKE.delete_FbxUserNotification(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
    }
  }

  public static FbxUserNotification Create(FbxManager pManager, FbxString pLogFileName, FbxString pSessionDescription) {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxUserNotification_Create(FbxManager.getCPtr(pManager), FbxString.getCPtr(pLogFileName), FbxString.getCPtr(pSessionDescription));
    FbxUserNotification ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxUserNotification(cPtr, false);
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static void Destroy(FbxManager pManager) {
    FbxWrapperNativePINVOKE.FbxUserNotification_Destroy(FbxManager.getCPtr(pManager));
  }

  public FbxUserNotification(FbxManager pManager, FbxString pLogFileName, FbxString pSessionDescription) : this(FbxWrapperNativePINVOKE.new_FbxUserNotification(FbxManager.getCPtr(pManager), FbxString.getCPtr(pLogFileName), FbxString.getCPtr(pSessionDescription)), true) {
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
  }

  public void InitAccumulator() {
    FbxWrapperNativePINVOKE.FbxUserNotification_InitAccumulator(swigCPtr);
  }

  public void ClearAccumulator() {
    FbxWrapperNativePINVOKE.FbxUserNotification_ClearAccumulator(swigCPtr);
  }

  public int AddEntry(int pID, FbxString pName, FbxString pDescr, FbxAccumulatorEntry.EClass pClass) {
    int ret = FbxWrapperNativePINVOKE.FbxUserNotification_AddEntry__SWIG_0(swigCPtr, pID, FbxString.getCPtr(pName), FbxString.getCPtr(pDescr), (int)pClass);
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public int AddEntry(int pID, FbxString pName, FbxString pDescr) {
    int ret = FbxWrapperNativePINVOKE.FbxUserNotification_AddEntry__SWIG_1(swigCPtr, pID, FbxString.getCPtr(pName), FbxString.getCPtr(pDescr));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public int AddDetail(int pEntryId) {
    int ret = FbxWrapperNativePINVOKE.FbxUserNotification_AddDetail__SWIG_0(swigCPtr, pEntryId);
    return ret;
  }

  public int AddDetail(int pEntryId, FbxString pString) {
    int ret = FbxWrapperNativePINVOKE.FbxUserNotification_AddDetail__SWIG_1(swigCPtr, pEntryId, FbxString.getCPtr(pString));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public int AddDetail(int pEntryId, FbxNode pNode) {
    int ret = FbxWrapperNativePINVOKE.FbxUserNotification_AddDetail__SWIG_2(swigCPtr, pEntryId, FbxNode.getCPtr(pNode));
    return ret;
  }

  public int GetNbEntries() {
    int ret = FbxWrapperNativePINVOKE.FbxUserNotification_GetNbEntries(swigCPtr);
    return ret;
  }

  public FbxAccumulatorEntry GetEntry(int pEntryId) {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxUserNotification_GetEntry(swigCPtr, pEntryId);
    FbxAccumulatorEntry ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxAccumulatorEntry(cPtr, false);
    return ret;
  }

  public FbxAccumulatorEntry GetEntryAt(int pEntryIndex) {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxUserNotification_GetEntryAt(swigCPtr, pEntryIndex);
    FbxAccumulatorEntry ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxAccumulatorEntry(cPtr, false);
    return ret;
  }

  public int GetNbDetails() {
    int ret = FbxWrapperNativePINVOKE.FbxUserNotification_GetNbDetails(swigCPtr);
    return ret;
  }

  public int GetDetail(int pDetailId, SWIGTYPE_p_p_FbxAccumulatorEntry pAE) {
    int ret = FbxWrapperNativePINVOKE.FbxUserNotification_GetDetail(swigCPtr, pDetailId, SWIGTYPE_p_p_FbxAccumulatorEntry.getCPtr(pAE));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool Output(FbxUserNotification.EOutputSource pOutSrc, int pIndex, bool pExtraDevicesOnly) {
    bool ret = FbxWrapperNativePINVOKE.FbxUserNotification_Output__SWIG_0(swigCPtr, (int)pOutSrc, pIndex, pExtraDevicesOnly);
    return ret;
  }

  public bool Output(FbxUserNotification.EOutputSource pOutSrc, int pIndex) {
    bool ret = FbxWrapperNativePINVOKE.FbxUserNotification_Output__SWIG_1(swigCPtr, (int)pOutSrc, pIndex);
    return ret;
  }

  public bool Output(FbxUserNotification.EOutputSource pOutSrc) {
    bool ret = FbxWrapperNativePINVOKE.FbxUserNotification_Output__SWIG_2(swigCPtr, (int)pOutSrc);
    return ret;
  }

  public bool Output() {
    bool ret = FbxWrapperNativePINVOKE.FbxUserNotification_Output__SWIG_3(swigCPtr);
    return ret;
  }

  public bool OutputById(FbxUserNotification.EEntryID pId, FbxUserNotification.EOutputSource pOutSrc, bool pExtraDevicesOnly) {
    bool ret = FbxWrapperNativePINVOKE.FbxUserNotification_OutputById__SWIG_0(swigCPtr, (int)pId, (int)pOutSrc, pExtraDevicesOnly);
    return ret;
  }

  public bool OutputById(FbxUserNotification.EEntryID pId, FbxUserNotification.EOutputSource pOutSrc) {
    bool ret = FbxWrapperNativePINVOKE.FbxUserNotification_OutputById__SWIG_1(swigCPtr, (int)pId, (int)pOutSrc);
    return ret;
  }

  public bool OutputById(FbxUserNotification.EEntryID pId) {
    bool ret = FbxWrapperNativePINVOKE.FbxUserNotification_OutputById__SWIG_2(swigCPtr, (int)pId);
    return ret;
  }

  public bool Output(FbxString pName, FbxString pDescr, FbxAccumulatorEntry.EClass pClass, bool pExtraDevicesOnly) {
    bool ret = FbxWrapperNativePINVOKE.FbxUserNotification_Output__SWIG_4(swigCPtr, FbxString.getCPtr(pName), FbxString.getCPtr(pDescr), (int)pClass, pExtraDevicesOnly);
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool Output(FbxString pName, FbxString pDescr, FbxAccumulatorEntry.EClass pClass) {
    bool ret = FbxWrapperNativePINVOKE.FbxUserNotification_Output__SWIG_5(swigCPtr, FbxString.getCPtr(pName), FbxString.getCPtr(pDescr), (int)pClass);
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool Output(FbxUserNotificationFilteredIterator pAEFIter, bool pExtraDevicesOnly) {
    bool ret = FbxWrapperNativePINVOKE.FbxUserNotification_Output__SWIG_6(swigCPtr, FbxUserNotificationFilteredIterator.getCPtr(pAEFIter), pExtraDevicesOnly);
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool Output(FbxUserNotificationFilteredIterator pAEFIter) {
    bool ret = FbxWrapperNativePINVOKE.FbxUserNotification_Output__SWIG_7(swigCPtr, FbxUserNotificationFilteredIterator.getCPtr(pAEFIter));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public void SetLogMessageEmitter(SWIGTYPE_p_FbxMessageEmitter pLogMessageEmitter) {
    FbxWrapperNativePINVOKE.FbxUserNotification_SetLogMessageEmitter(swigCPtr, SWIGTYPE_p_FbxMessageEmitter.getCPtr(pLogMessageEmitter));
  }

  public virtual void GetLogFilePath(FbxString pPath) {
    FbxWrapperNativePINVOKE.FbxUserNotification_GetLogFilePath(swigCPtr, FbxString.getCPtr(pPath));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
  }

  public FbxString GetLogFileName() {
    FbxString ret = new FbxString(FbxWrapperNativePINVOKE.FbxUserNotification_GetLogFileName(swigCPtr), true);
    return ret;
  }

  public enum EEntryID {
    eBindPoseInvalidObject,
    eBindPoseInvalidRoot,
    eBindPoseNotAllAncestorsNodes,
    eBindPoseNotAllDeformingNodes,
    eBindPoseNotAllAncestorsDefinitionNodes,
    eBindPoseRelativeMatrix,
    eEmbedMediaNotify,
    eFileIONotify,
    eFileIONotifyMaterial,
    eFileIONotifyDXFNotSupportNurbs,
    eEntryStartID
  }

  public enum EOutputSource {
    eAccumulatorEntry,
    eSequencedDetails
  }

}

}
