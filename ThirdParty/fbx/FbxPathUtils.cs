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

public class FbxPathUtils : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal FbxPathUtils(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(FbxPathUtils obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~FbxPathUtils() {
    Dispose();
  }

  public virtual void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          FbxWrapperNativePINVOKE.delete_FbxPathUtils(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
    }
  }

  public static FbxString Bind(string pRootPath, string pFilePath, bool pCleanPath) {
    FbxString ret = new FbxString(FbxWrapperNativePINVOKE.FbxPathUtils_Bind__SWIG_0(pRootPath, pFilePath, pCleanPath), true);
    return ret;
  }

  public static FbxString Bind(string pRootPath, string pFilePath) {
    FbxString ret = new FbxString(FbxWrapperNativePINVOKE.FbxPathUtils_Bind__SWIG_1(pRootPath, pFilePath), true);
    return ret;
  }

  public static FbxString GetFolderName(string pFilePath) {
    FbxString ret = new FbxString(FbxWrapperNativePINVOKE.FbxPathUtils_GetFolderName(pFilePath), true);
    return ret;
  }

  public static FbxString GetFileName(string pFilePath, bool pWithExtension) {
    FbxString ret = new FbxString(FbxWrapperNativePINVOKE.FbxPathUtils_GetFileName__SWIG_0(pFilePath, pWithExtension), true);
    return ret;
  }

  public static FbxString GetFileName(string pFilePath) {
    FbxString ret = new FbxString(FbxWrapperNativePINVOKE.FbxPathUtils_GetFileName__SWIG_1(pFilePath), true);
    return ret;
  }

  public static FbxString GetExtensionName(string pFilePath) {
    FbxString ret = new FbxString(FbxWrapperNativePINVOKE.FbxPathUtils_GetExtensionName(pFilePath), true);
    return ret;
  }

  public static FbxString ChangeExtension(string pFilePath, string pExtension) {
    FbxString ret = new FbxString(FbxWrapperNativePINVOKE.FbxPathUtils_ChangeExtension(pFilePath, pExtension), true);
    return ret;
  }

  public static bool IsRelative(string pPath) {
    bool ret = FbxWrapperNativePINVOKE.FbxPathUtils_IsRelative(pPath);
    return ret;
  }

  public static FbxString GetRelativePath(string pRootPath, string pNewPath) {
    FbxString ret = new FbxString(FbxWrapperNativePINVOKE.FbxPathUtils_GetRelativePath(pRootPath, pNewPath), true);
    return ret;
  }

  public static FbxString GetRelativeFilePath(string pRootPath, string pNewFilePath) {
    FbxString ret = new FbxString(FbxWrapperNativePINVOKE.FbxPathUtils_GetRelativeFilePath(pRootPath, pNewFilePath), true);
    return ret;
  }

  public static FbxString Resolve(string pRelPath) {
    FbxString ret = new FbxString(FbxWrapperNativePINVOKE.FbxPathUtils_Resolve(pRelPath), true);
    return ret;
  }

  public static FbxString Clean(string pPath) {
    FbxString ret = new FbxString(FbxWrapperNativePINVOKE.FbxPathUtils_Clean(pPath), true);
    return ret;
  }

  public static FbxString GenerateFileName(string pFolder, string pPrefix) {
    FbxString ret = new FbxString(FbxWrapperNativePINVOKE.FbxPathUtils_GenerateFileName(pFolder, pPrefix), true);
    return ret;
  }

  public static bool Exist(string pFolderPathUTF8) {
    bool ret = FbxWrapperNativePINVOKE.FbxPathUtils_Exist(pFolderPathUTF8);
    return ret;
  }

  public static bool Create(string pFolderPathUTF8) {
    bool ret = FbxWrapperNativePINVOKE.FbxPathUtils_Create(pFolderPathUTF8);
    return ret;
  }

  public static bool Delete(string pFolderPathUTF8) {
    bool ret = FbxWrapperNativePINVOKE.FbxPathUtils_Delete(pFolderPathUTF8);
    return ret;
  }

  public static bool IsEmpty(string pFolderPath_UTF8) {
    bool ret = FbxWrapperNativePINVOKE.FbxPathUtils_IsEmpty(pFolderPath_UTF8);
    return ret;
  }

  public FbxPathUtils() : this(FbxWrapperNativePINVOKE.new_FbxPathUtils(), true) {
  }

}

}
