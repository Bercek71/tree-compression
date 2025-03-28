//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (https://www.swig.org).
// Version 4.1.1
//
// Do not make changes to this file unless you know what you are doing - modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------

namespace Ufal.MorphoDiTa {

public class Tagger : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal Tagger(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(Tagger obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  internal static global::System.Runtime.InteropServices.HandleRef swigRelease(Tagger obj) {
    if (obj != null) {
      if (!obj.swigCMemOwn)
        throw new global::System.ApplicationException("Cannot release ownership as memory is not owned");
      global::System.Runtime.InteropServices.HandleRef ptr = obj.swigCPtr;
      obj.swigCMemOwn = false;
      obj.Dispose();
      return ptr;
    } else {
      return new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
    }
  }

  ~Tagger() {
    Dispose(false);
  }

  public void Dispose() {
    Dispose(true);
    global::System.GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing) {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          morphodita_csharpPINVOKE.delete_Tagger(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public static Tagger load(string fname) {
    global::System.IntPtr cPtr = morphodita_csharpPINVOKE.Tagger_load(fname);
    Tagger ret = (cPtr == global::System.IntPtr.Zero) ? null : new Tagger(cPtr, true);
    return ret;
  }

  public virtual Morpho getMorpho() {
    global::System.IntPtr cPtr = morphodita_csharpPINVOKE.Tagger_getMorpho(swigCPtr);
    Morpho ret = (cPtr == global::System.IntPtr.Zero) ? null : new Morpho(cPtr, false);
    return ret;
  }

  public void tag(Forms forms, TaggedLemmas tags, int guesser) {
    morphodita_csharpPINVOKE.Tagger_tag__SWIG_0(swigCPtr, Forms.getCPtr(forms), TaggedLemmas.getCPtr(tags), guesser);
    if (morphodita_csharpPINVOKE.SWIGPendingException.Pending) throw morphodita_csharpPINVOKE.SWIGPendingException.Retrieve();
  }

  public void tag(Forms forms, TaggedLemmas tags) {
    morphodita_csharpPINVOKE.Tagger_tag__SWIG_1(swigCPtr, Forms.getCPtr(forms), TaggedLemmas.getCPtr(tags));
    if (morphodita_csharpPINVOKE.SWIGPendingException.Pending) throw morphodita_csharpPINVOKE.SWIGPendingException.Retrieve();
  }

  public void tagAnalyzed(Forms forms, Analyses analyses, Indices tags) {
    morphodita_csharpPINVOKE.Tagger_tagAnalyzed(swigCPtr, Forms.getCPtr(forms), Analyses.getCPtr(analyses), Indices.getCPtr(tags));
    if (morphodita_csharpPINVOKE.SWIGPendingException.Pending) throw morphodita_csharpPINVOKE.SWIGPendingException.Retrieve();
  }

  public Tokenizer newTokenizer() {
    global::System.IntPtr cPtr = morphodita_csharpPINVOKE.Tagger_newTokenizer(swigCPtr);
    Tokenizer ret = (cPtr == global::System.IntPtr.Zero) ? null : new Tokenizer(cPtr, true);
    return ret;
  }

}

}
