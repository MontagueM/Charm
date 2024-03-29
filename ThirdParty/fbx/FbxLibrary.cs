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

public class FbxLibrary : FbxDocument {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;

  internal FbxLibrary(global::System.IntPtr cPtr, bool cMemoryOwn) : base(FbxWrapperNativePINVOKE.FbxLibrary_SWIGUpcast(cPtr), cMemoryOwn) {
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(FbxLibrary obj) {
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
      FbxWrapperNativePINVOKE.FbxLibrary_ClassId_set(FbxClassId.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxLibrary_ClassId_get();
      FbxClassId ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxClassId(cPtr, false);
      return ret;
    } 
  }

  public override FbxClassId GetClassId() {
    FbxClassId ret = new FbxClassId(FbxWrapperNativePINVOKE.FbxLibrary_GetClassId(swigCPtr), true);
    return ret;
  }

  public new static FbxLibrary Create(FbxManager pManager, string pName) {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxLibrary_Create__SWIG_0(FbxManager.getCPtr(pManager), pName);
    FbxLibrary ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxLibrary(cPtr, false);
    return ret;
  }

  public new static FbxLibrary Create(FbxObject pContainer, string pName) {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxLibrary_Create__SWIG_1(FbxObject.getCPtr(pContainer), pName);
    FbxLibrary ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxLibrary(cPtr, false);
    return ret;
  }

  public new FbxLibrary GetParentLibrary() {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxLibrary_GetParentLibrary(swigCPtr);
    FbxLibrary ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxLibrary(cPtr, false);
    return ret;
  }

  public void SystemLibrary(bool pSystemLibrary) {
    FbxWrapperNativePINVOKE.FbxLibrary_SystemLibrary(swigCPtr, pSystemLibrary);
  }

  public bool IsSystemLibrary() {
    bool ret = FbxWrapperNativePINVOKE.FbxLibrary_IsSystemLibrary(swigCPtr);
    return ret;
  }

  public void LocalizationBaseNamePrefix(string pPrefix) {
    FbxWrapperNativePINVOKE.FbxLibrary_LocalizationBaseNamePrefix__SWIG_0(swigCPtr, pPrefix);
  }

  public FbxString LocalizationBaseNamePrefix() {
    FbxString ret = new FbxString(FbxWrapperNativePINVOKE.FbxLibrary_LocalizationBaseNamePrefix__SWIG_1(swigCPtr), true);
    return ret;
  }

  public bool AddSubLibrary(FbxLibrary pSubLibrary) {
    bool ret = FbxWrapperNativePINVOKE.FbxLibrary_AddSubLibrary(swigCPtr, FbxLibrary.getCPtr(pSubLibrary));
    return ret;
  }

  public bool RemoveSubLibrary(FbxLibrary pSubLibrary) {
    bool ret = FbxWrapperNativePINVOKE.FbxLibrary_RemoveSubLibrary(swigCPtr, FbxLibrary.getCPtr(pSubLibrary));
    return ret;
  }

  public int GetSubLibraryCount() {
    int ret = FbxWrapperNativePINVOKE.FbxLibrary_GetSubLibraryCount(swigCPtr);
    return ret;
  }

  public FbxLibrary GetSubLibrary(int pIndex) {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxLibrary_GetSubLibrary(swigCPtr, pIndex);
    FbxLibrary ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxLibrary(cPtr, false);
    return ret;
  }

  public FbxObject CloneAsset(FbxObject pToClone, FbxObject pOptionalDestinationContainer) {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxLibrary_CloneAsset__SWIG_0(swigCPtr, FbxObject.getCPtr(pToClone), FbxObject.getCPtr(pOptionalDestinationContainer));
    FbxObject ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxObject(cPtr, false);
    return ret;
  }

  public FbxObject CloneAsset(FbxObject pToClone) {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxLibrary_CloneAsset__SWIG_1(swigCPtr, FbxObject.getCPtr(pToClone));
    FbxObject ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxObject(cPtr, false);
    return ret;
  }

  public static FbxCriteria GetAssetCriteriaFilter() {
    FbxCriteria ret = new FbxCriteria(FbxWrapperNativePINVOKE.FbxLibrary_GetAssetCriteriaFilter(), true);
    return ret;
  }

  public static FbxCriteria GetAssetDependentsFilter() {
    FbxCriteria ret = new FbxCriteria(FbxWrapperNativePINVOKE.FbxLibrary_GetAssetDependentsFilter(), true);
    return ret;
  }

  public bool ImportAssets(FbxLibrary pSrcLibrary) {
    bool ret = FbxWrapperNativePINVOKE.FbxLibrary_ImportAssets__SWIG_0(swigCPtr, FbxLibrary.getCPtr(pSrcLibrary));
    return ret;
  }

  public bool ImportAssets(FbxLibrary pSrcLibrary, FbxCriteria pAssetFilter) {
    bool ret = FbxWrapperNativePINVOKE.FbxLibrary_ImportAssets__SWIG_1(swigCPtr, FbxLibrary.getCPtr(pSrcLibrary), FbxCriteria.getCPtr(pAssetFilter));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public SWIGTYPE_p_FbxLocalizationManager GetLocalizationManager() {
    SWIGTYPE_p_FbxLocalizationManager ret = new SWIGTYPE_p_FbxLocalizationManager(FbxWrapperNativePINVOKE.FbxLibrary_GetLocalizationManager(swigCPtr), false);
    return ret;
  }

  public override string Localize(string pID, string pDefault) {
    string ret = FbxWrapperNativePINVOKE.FbxLibrary_Localize__SWIG_0(swigCPtr, pID, pDefault);
    return ret;
  }

  public override string Localize(string pID) {
    string ret = FbxWrapperNativePINVOKE.FbxLibrary_Localize__SWIG_1(swigCPtr, pID);
    return ret;
  }

  public bool AddShadingObject(FbxObject pShadingObject) {
    bool ret = FbxWrapperNativePINVOKE.FbxLibrary_AddShadingObject(swigCPtr, FbxObject.getCPtr(pShadingObject));
    return ret;
  }

  public bool RemoveShadingObject(FbxObject pShadingObject) {
    bool ret = FbxWrapperNativePINVOKE.FbxLibrary_RemoveShadingObject(swigCPtr, FbxObject.getCPtr(pShadingObject));
    return ret;
  }

  public int GetShadingObjectCount() {
    int ret = FbxWrapperNativePINVOKE.FbxLibrary_GetShadingObjectCount__SWIG_0(swigCPtr);
    return ret;
  }

  public FbxObject GetShadingObject(int pIndex) {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxLibrary_GetShadingObject__SWIG_0(swigCPtr, pIndex);
    FbxObject ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxObject(cPtr, false);
    return ret;
  }

  public int GetShadingObjectCount(FbxImplementationFilter pCriteria) {
    int ret = FbxWrapperNativePINVOKE.FbxLibrary_GetShadingObjectCount__SWIG_1(swigCPtr, FbxImplementationFilter.getCPtr(pCriteria));
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public FbxObject GetShadingObject(int pIndex, FbxImplementationFilter pCriteria) {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxLibrary_GetShadingObject__SWIG_1(swigCPtr, pIndex, FbxImplementationFilter.getCPtr(pCriteria));
    FbxObject ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxObject(cPtr, false);
    if (FbxWrapperNativePINVOKE.SWIGPendingException.Pending) throw FbxWrapperNativePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

}

}
