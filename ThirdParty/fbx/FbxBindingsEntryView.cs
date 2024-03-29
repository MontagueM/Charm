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

public class FbxBindingsEntryView : FbxEntryView {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;

  internal FbxBindingsEntryView(global::System.IntPtr cPtr, bool cMemoryOwn) : base(FbxWrapperNativePINVOKE.FbxBindingsEntryView_SWIGUpcast(cPtr), cMemoryOwn) {
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(FbxBindingsEntryView obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~FbxBindingsEntryView() {
    Dispose();
  }

  public override void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          FbxWrapperNativePINVOKE.delete_FbxBindingsEntryView(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
      base.Dispose();
    }
  }

  public static string sEntryType {
    set {
      FbxWrapperNativePINVOKE.FbxBindingsEntryView_sEntryType_set(value);
    } 
    get {
      string ret = FbxWrapperNativePINVOKE.FbxBindingsEntryView_sEntryType_get();
      return ret;
    } 
  }

  public FbxBindingsEntryView(FbxBindingTableEntry pEntry, bool pAsSource, bool pCreate) : this(FbxWrapperNativePINVOKE.new_FbxBindingsEntryView__SWIG_0(FbxBindingTableEntry.getCPtr(pEntry), pAsSource, pCreate), true) {
  }

  public FbxBindingsEntryView(FbxBindingTableEntry pEntry, bool pAsSource) : this(FbxWrapperNativePINVOKE.new_FbxBindingsEntryView__SWIG_1(FbxBindingTableEntry.getCPtr(pEntry), pAsSource), true) {
  }

  public string GetBindingTableName() {
    string ret = FbxWrapperNativePINVOKE.FbxBindingsEntryView_GetBindingTableName(swigCPtr);
    return ret;
  }

  public void SetBindingTableName(string pName) {
    FbxWrapperNativePINVOKE.FbxBindingsEntryView_SetBindingTableName(swigCPtr, pName);
  }

  public override string EntryType() {
    string ret = FbxWrapperNativePINVOKE.FbxBindingsEntryView_EntryType(swigCPtr);
    return ret;
  }

}

}
