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

public class TokenRanges : global::System.IDisposable, global::System.Collections.IEnumerable, global::System.Collections.Generic.IEnumerable<TokenRange>
 {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal TokenRanges(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(TokenRanges obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  internal static global::System.Runtime.InteropServices.HandleRef swigRelease(TokenRanges obj) {
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

  ~TokenRanges() {
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
          morphodita_csharpPINVOKE.delete_TokenRanges(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public TokenRanges(global::System.Collections.IEnumerable c) : this() {
    if (c == null)
      throw new global::System.ArgumentNullException("c");
    foreach (TokenRange element in c) {
      this.Add(element);
    }
  }

  public TokenRanges(global::System.Collections.Generic.IEnumerable<TokenRange> c) : this() {
    if (c == null)
      throw new global::System.ArgumentNullException("c");
    foreach (TokenRange element in c) {
      this.Add(element);
    }
  }

  public bool IsFixedSize {
    get {
      return false;
    }
  }

  public bool IsReadOnly {
    get {
      return false;
    }
  }

  public TokenRange this[int index]  {
    get {
      return getitem(index);
    }
    set {
      setitem(index, value);
    }
  }

  public int Capacity {
    get {
      return (int)capacity();
    }
    set {
      if (value < 0 || (uint)value < size())
        throw new global::System.ArgumentOutOfRangeException("Capacity");
      reserve((uint)value);
    }
  }

  public int Count {
    get {
      return (int)size();
    }
  }

  public bool IsSynchronized {
    get {
      return false;
    }
  }

  public void CopyTo(TokenRange[] array)
  {
    CopyTo(0, array, 0, this.Count);
  }

  public void CopyTo(TokenRange[] array, int arrayIndex)
  {
    CopyTo(0, array, arrayIndex, this.Count);
  }

  public void CopyTo(int index, TokenRange[] array, int arrayIndex, int count)
  {
    if (array == null)
      throw new global::System.ArgumentNullException("array");
    if (index < 0)
      throw new global::System.ArgumentOutOfRangeException("index", "Value is less than zero");
    if (arrayIndex < 0)
      throw new global::System.ArgumentOutOfRangeException("arrayIndex", "Value is less than zero");
    if (count < 0)
      throw new global::System.ArgumentOutOfRangeException("count", "Value is less than zero");
    if (array.Rank > 1)
      throw new global::System.ArgumentException("Multi dimensional array.", "array");
    if (index+count > this.Count || arrayIndex+count > array.Length)
      throw new global::System.ArgumentException("Number of elements to copy is too large.");
    for (int i=0; i<count; i++)
      array.SetValue(getitemcopy(index+i), arrayIndex+i);
  }

  public TokenRange[] ToArray() {
    TokenRange[] array = new TokenRange[this.Count];
    this.CopyTo(array);
    return array;
  }

  global::System.Collections.Generic.IEnumerator<TokenRange> global::System.Collections.Generic.IEnumerable<TokenRange>.GetEnumerator() {
    return new TokenRangesEnumerator(this);
  }

  global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator() {
    return new TokenRangesEnumerator(this);
  }

  public TokenRangesEnumerator GetEnumerator() {
    return new TokenRangesEnumerator(this);
  }

  // Type-safe enumerator
  /// Note that the IEnumerator documentation requires an InvalidOperationException to be thrown
  /// whenever the collection is modified. This has been done for changes in the size of the
  /// collection but not when one of the elements of the collection is modified as it is a bit
  /// tricky to detect unmanaged code that modifies the collection under our feet.
  public sealed class TokenRangesEnumerator : global::System.Collections.IEnumerator
    , global::System.Collections.Generic.IEnumerator<TokenRange>
  {
    private TokenRanges collectionRef;
    private int currentIndex;
    private object currentObject;
    private int currentSize;

    public TokenRangesEnumerator(TokenRanges collection) {
      collectionRef = collection;
      currentIndex = -1;
      currentObject = null;
      currentSize = collectionRef.Count;
    }

    // Type-safe iterator Current
    public TokenRange Current {
      get {
        if (currentIndex == -1)
          throw new global::System.InvalidOperationException("Enumeration not started.");
        if (currentIndex > currentSize - 1)
          throw new global::System.InvalidOperationException("Enumeration finished.");
        if (currentObject == null)
          throw new global::System.InvalidOperationException("Collection modified.");
        return (TokenRange)currentObject;
      }
    }

    // Type-unsafe IEnumerator.Current
    object global::System.Collections.IEnumerator.Current {
      get {
        return Current;
      }
    }

    public bool MoveNext() {
      int size = collectionRef.Count;
      bool moveOkay = (currentIndex+1 < size) && (size == currentSize);
      if (moveOkay) {
        currentIndex++;
        currentObject = collectionRef[currentIndex];
      } else {
        currentObject = null;
      }
      return moveOkay;
    }

    public void Reset() {
      currentIndex = -1;
      currentObject = null;
      if (collectionRef.Count != currentSize) {
        throw new global::System.InvalidOperationException("Collection modified.");
      }
    }

    public void Dispose() {
        currentIndex = -1;
        currentObject = null;
    }
  }

  public void Clear() {
    morphodita_csharpPINVOKE.TokenRanges_Clear(swigCPtr);
  }

  public void Add(TokenRange x) {
    morphodita_csharpPINVOKE.TokenRanges_Add(swigCPtr, TokenRange.getCPtr(x));
    if (morphodita_csharpPINVOKE.SWIGPendingException.Pending) throw morphodita_csharpPINVOKE.SWIGPendingException.Retrieve();
  }

  private uint size() {
    uint ret = morphodita_csharpPINVOKE.TokenRanges_size(swigCPtr);
    return ret;
  }

  private uint capacity() {
    uint ret = morphodita_csharpPINVOKE.TokenRanges_capacity(swigCPtr);
    return ret;
  }

  private void reserve(uint n) {
    morphodita_csharpPINVOKE.TokenRanges_reserve(swigCPtr, n);
  }

  public TokenRanges() : this(morphodita_csharpPINVOKE.new_TokenRanges__SWIG_0(), true) {
  }

  public TokenRanges(TokenRanges other) : this(morphodita_csharpPINVOKE.new_TokenRanges__SWIG_1(TokenRanges.getCPtr(other)), true) {
    if (morphodita_csharpPINVOKE.SWIGPendingException.Pending) throw morphodita_csharpPINVOKE.SWIGPendingException.Retrieve();
  }

  public TokenRanges(int capacity) : this(morphodita_csharpPINVOKE.new_TokenRanges__SWIG_2(capacity), true) {
    if (morphodita_csharpPINVOKE.SWIGPendingException.Pending) throw morphodita_csharpPINVOKE.SWIGPendingException.Retrieve();
  }

  private TokenRange getitemcopy(int index) {
    TokenRange ret = new TokenRange(morphodita_csharpPINVOKE.TokenRanges_getitemcopy(swigCPtr, index), true);
    if (morphodita_csharpPINVOKE.SWIGPendingException.Pending) throw morphodita_csharpPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  private TokenRange getitem(int index) {
    TokenRange ret = new TokenRange(morphodita_csharpPINVOKE.TokenRanges_getitem(swigCPtr, index), false);
    if (morphodita_csharpPINVOKE.SWIGPendingException.Pending) throw morphodita_csharpPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  private void setitem(int index, TokenRange val) {
    morphodita_csharpPINVOKE.TokenRanges_setitem(swigCPtr, index, TokenRange.getCPtr(val));
    if (morphodita_csharpPINVOKE.SWIGPendingException.Pending) throw morphodita_csharpPINVOKE.SWIGPendingException.Retrieve();
  }

  public void AddRange(TokenRanges values) {
    morphodita_csharpPINVOKE.TokenRanges_AddRange(swigCPtr, TokenRanges.getCPtr(values));
    if (morphodita_csharpPINVOKE.SWIGPendingException.Pending) throw morphodita_csharpPINVOKE.SWIGPendingException.Retrieve();
  }

  public TokenRanges GetRange(int index, int count) {
    global::System.IntPtr cPtr = morphodita_csharpPINVOKE.TokenRanges_GetRange(swigCPtr, index, count);
    TokenRanges ret = (cPtr == global::System.IntPtr.Zero) ? null : new TokenRanges(cPtr, true);
    if (morphodita_csharpPINVOKE.SWIGPendingException.Pending) throw morphodita_csharpPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public void Insert(int index, TokenRange x) {
    morphodita_csharpPINVOKE.TokenRanges_Insert(swigCPtr, index, TokenRange.getCPtr(x));
    if (morphodita_csharpPINVOKE.SWIGPendingException.Pending) throw morphodita_csharpPINVOKE.SWIGPendingException.Retrieve();
  }

  public void InsertRange(int index, TokenRanges values) {
    morphodita_csharpPINVOKE.TokenRanges_InsertRange(swigCPtr, index, TokenRanges.getCPtr(values));
    if (morphodita_csharpPINVOKE.SWIGPendingException.Pending) throw morphodita_csharpPINVOKE.SWIGPendingException.Retrieve();
  }

  public void RemoveAt(int index) {
    morphodita_csharpPINVOKE.TokenRanges_RemoveAt(swigCPtr, index);
    if (morphodita_csharpPINVOKE.SWIGPendingException.Pending) throw morphodita_csharpPINVOKE.SWIGPendingException.Retrieve();
  }

  public void RemoveRange(int index, int count) {
    morphodita_csharpPINVOKE.TokenRanges_RemoveRange(swigCPtr, index, count);
    if (morphodita_csharpPINVOKE.SWIGPendingException.Pending) throw morphodita_csharpPINVOKE.SWIGPendingException.Retrieve();
  }

  public static TokenRanges Repeat(TokenRange value, int count) {
    global::System.IntPtr cPtr = morphodita_csharpPINVOKE.TokenRanges_Repeat(TokenRange.getCPtr(value), count);
    TokenRanges ret = (cPtr == global::System.IntPtr.Zero) ? null : new TokenRanges(cPtr, true);
    if (morphodita_csharpPINVOKE.SWIGPendingException.Pending) throw morphodita_csharpPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public void Reverse() {
    morphodita_csharpPINVOKE.TokenRanges_Reverse__SWIG_0(swigCPtr);
  }

  public void Reverse(int index, int count) {
    morphodita_csharpPINVOKE.TokenRanges_Reverse__SWIG_1(swigCPtr, index, count);
    if (morphodita_csharpPINVOKE.SWIGPendingException.Pending) throw morphodita_csharpPINVOKE.SWIGPendingException.Retrieve();
  }

  public void SetRange(int index, TokenRanges values) {
    morphodita_csharpPINVOKE.TokenRanges_SetRange(swigCPtr, index, TokenRanges.getCPtr(values));
    if (morphodita_csharpPINVOKE.SWIGPendingException.Pending) throw morphodita_csharpPINVOKE.SWIGPendingException.Retrieve();
  }

}

}
