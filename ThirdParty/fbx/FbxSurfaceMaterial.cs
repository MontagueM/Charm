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

public class FbxSurfaceMaterial : FbxObject {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;

  internal FbxSurfaceMaterial(global::System.IntPtr cPtr, bool cMemoryOwn) : base(FbxWrapperNativePINVOKE.FbxSurfaceMaterial_SWIGUpcast(cPtr), cMemoryOwn) {
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(FbxSurfaceMaterial obj) {
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
      FbxWrapperNativePINVOKE.FbxSurfaceMaterial_ClassId_set(FbxClassId.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxSurfaceMaterial_ClassId_get();
      FbxClassId ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxClassId(cPtr, false);
      return ret;
    } 
  }

  public override FbxClassId GetClassId() {
    FbxClassId ret = new FbxClassId(FbxWrapperNativePINVOKE.FbxSurfaceMaterial_GetClassId(swigCPtr), true);
    return ret;
  }

  public new static FbxSurfaceMaterial Create(FbxManager pManager, string pName) {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxSurfaceMaterial_Create__SWIG_0(FbxManager.getCPtr(pManager), pName);
    FbxSurfaceMaterial ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxSurfaceMaterial(cPtr, false);
    return ret;
  }

  public new static FbxSurfaceMaterial Create(FbxObject pContainer, string pName) {
    global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxSurfaceMaterial_Create__SWIG_1(FbxObject.getCPtr(pContainer), pName);
    FbxSurfaceMaterial ret = (cPtr == global::System.IntPtr.Zero) ? null : new FbxSurfaceMaterial(cPtr, false);
    return ret;
  }

  public static string sShadingModel {
    set {
      FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sShadingModel_set(value);
    } 
    get {
      string ret = FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sShadingModel_get();
      return ret;
    } 
  }

  public static string sMultiLayer {
    set {
      FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sMultiLayer_set(value);
    } 
    get {
      string ret = FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sMultiLayer_get();
      return ret;
    } 
  }

  public static string sEmissive {
    set {
      FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sEmissive_set(value);
    } 
    get {
      string ret = FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sEmissive_get();
      return ret;
    } 
  }

  public static string sEmissiveFactor {
    set {
      FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sEmissiveFactor_set(value);
    } 
    get {
      string ret = FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sEmissiveFactor_get();
      return ret;
    } 
  }

  public static string sAmbient {
    set {
      FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sAmbient_set(value);
    } 
    get {
      string ret = FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sAmbient_get();
      return ret;
    } 
  }

  public static string sAmbientFactor {
    set {
      FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sAmbientFactor_set(value);
    } 
    get {
      string ret = FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sAmbientFactor_get();
      return ret;
    } 
  }

  public static string sDiffuse {
    set {
      FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sDiffuse_set(value);
    } 
    get {
      string ret = FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sDiffuse_get();
      return ret;
    } 
  }

  public static string sDiffuseFactor {
    set {
      FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sDiffuseFactor_set(value);
    } 
    get {
      string ret = FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sDiffuseFactor_get();
      return ret;
    } 
  }

  public static string sSpecular {
    set {
      FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sSpecular_set(value);
    } 
    get {
      string ret = FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sSpecular_get();
      return ret;
    } 
  }

  public static string sSpecularFactor {
    set {
      FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sSpecularFactor_set(value);
    } 
    get {
      string ret = FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sSpecularFactor_get();
      return ret;
    } 
  }

  public static string sShininess {
    set {
      FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sShininess_set(value);
    } 
    get {
      string ret = FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sShininess_get();
      return ret;
    } 
  }

  public static string sBump {
    set {
      FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sBump_set(value);
    } 
    get {
      string ret = FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sBump_get();
      return ret;
    } 
  }

  public static string sNormalMap {
    set {
      FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sNormalMap_set(value);
    } 
    get {
      string ret = FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sNormalMap_get();
      return ret;
    } 
  }

  public static string sBumpFactor {
    set {
      FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sBumpFactor_set(value);
    } 
    get {
      string ret = FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sBumpFactor_get();
      return ret;
    } 
  }

  public static string sTransparentColor {
    set {
      FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sTransparentColor_set(value);
    } 
    get {
      string ret = FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sTransparentColor_get();
      return ret;
    } 
  }

  public static string sTransparencyFactor {
    set {
      FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sTransparencyFactor_set(value);
    } 
    get {
      string ret = FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sTransparencyFactor_get();
      return ret;
    } 
  }

  public static string sReflection {
    set {
      FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sReflection_set(value);
    } 
    get {
      string ret = FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sReflection_get();
      return ret;
    } 
  }

  public static string sReflectionFactor {
    set {
      FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sReflectionFactor_set(value);
    } 
    get {
      string ret = FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sReflectionFactor_get();
      return ret;
    } 
  }

  public static string sDisplacementColor {
    set {
      FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sDisplacementColor_set(value);
    } 
    get {
      string ret = FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sDisplacementColor_get();
      return ret;
    } 
  }

  public static string sDisplacementFactor {
    set {
      FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sDisplacementFactor_set(value);
    } 
    get {
      string ret = FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sDisplacementFactor_get();
      return ret;
    } 
  }

  public static string sVectorDisplacementColor {
    set {
      FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sVectorDisplacementColor_set(value);
    } 
    get {
      string ret = FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sVectorDisplacementColor_get();
      return ret;
    } 
  }

  public static string sVectorDisplacementFactor {
    set {
      FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sVectorDisplacementFactor_set(value);
    } 
    get {
      string ret = FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sVectorDisplacementFactor_get();
      return ret;
    } 
  }

  public SWIGTYPE_p_FbxPropertyTT_FbxString_t ShadingModel {
    set {
      FbxWrapperNativePINVOKE.FbxSurfaceMaterial_ShadingModel_set(swigCPtr, SWIGTYPE_p_FbxPropertyTT_FbxString_t.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxSurfaceMaterial_ShadingModel_get(swigCPtr);
      SWIGTYPE_p_FbxPropertyTT_FbxString_t ret = (cPtr == global::System.IntPtr.Zero) ? null : new SWIGTYPE_p_FbxPropertyTT_FbxString_t(cPtr, false);
      return ret;
    } 
  }

  public SWIGTYPE_p_FbxPropertyTT_bool_t MultiLayer {
    set {
      FbxWrapperNativePINVOKE.FbxSurfaceMaterial_MultiLayer_set(swigCPtr, SWIGTYPE_p_FbxPropertyTT_bool_t.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = FbxWrapperNativePINVOKE.FbxSurfaceMaterial_MultiLayer_get(swigCPtr);
      SWIGTYPE_p_FbxPropertyTT_bool_t ret = (cPtr == global::System.IntPtr.Zero) ? null : new SWIGTYPE_p_FbxPropertyTT_bool_t(cPtr, false);
      return ret;
    } 
  }

  public static bool sMultiLayerDefault {
    get {
      bool ret = FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sMultiLayerDefault_get();
      return ret;
    } 
  }

  public static string sShadingModelDefault {
    set {
      FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sShadingModelDefault_set(value);
    } 
    get {
      string ret = FbxWrapperNativePINVOKE.FbxSurfaceMaterial_sShadingModelDefault_get();
      return ret;
    } 
  }

}

}
