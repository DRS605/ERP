var Vs = { exports: {} }, Ql = {}, Bs = { exports: {} }, W = {};
/**
 * @license React
 * react.production.min.js
 *
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */
var Mr = Symbol.for("react.element"), ld = Symbol.for("react.portal"), ad = Symbol.for("react.fragment"), id = Symbol.for("react.strict_mode"), od = Symbol.for("react.profiler"), sd = Symbol.for("react.provider"), ud = Symbol.for("react.context"), cd = Symbol.for("react.forward_ref"), dd = Symbol.for("react.suspense"), fd = Symbol.for("react.memo"), pd = Symbol.for("react.lazy"), zo = Symbol.iterator;
function md(e) {
  return e === null || typeof e != "object" ? null : (e = zo && e[zo] || e["@@iterator"], typeof e == "function" ? e : null);
}
var Hs = { isMounted: function() {
  return !1;
}, enqueueForceUpdate: function() {
}, enqueueReplaceState: function() {
}, enqueueSetState: function() {
} }, Ws = Object.assign, Qs = {};
function Vn(e, t, n) {
  this.props = e, this.context = t, this.refs = Qs, this.updater = n || Hs;
}
Vn.prototype.isReactComponent = {};
Vn.prototype.setState = function(e, t) {
  if (typeof e != "object" && typeof e != "function" && e != null) throw Error("setState(...): takes an object of state variables to update or a function which returns an object of state variables.");
  this.updater.enqueueSetState(this, e, t, "setState");
};
Vn.prototype.forceUpdate = function(e) {
  this.updater.enqueueForceUpdate(this, e, "forceUpdate");
};
function Gs() {
}
Gs.prototype = Vn.prototype;
function Ti(e, t, n) {
  this.props = e, this.context = t, this.refs = Qs, this.updater = n || Hs;
}
var Di = Ti.prototype = new Gs();
Di.constructor = Ti;
Ws(Di, Vn.prototype);
Di.isPureReactComponent = !0;
var To = Array.isArray, Ks = Object.prototype.hasOwnProperty, Li = { current: null }, qs = { key: !0, ref: !0, __self: !0, __source: !0 };
function bs(e, t, n) {
  var r, l = {}, i = null, o = null;
  if (t != null) for (r in t.ref !== void 0 && (o = t.ref), t.key !== void 0 && (i = "" + t.key), t) Ks.call(t, r) && !qs.hasOwnProperty(r) && (l[r] = t[r]);
  var s = arguments.length - 2;
  if (s === 1) l.children = n;
  else if (1 < s) {
    for (var u = Array(s), d = 0; d < s; d++) u[d] = arguments[d + 2];
    l.children = u;
  }
  if (e && e.defaultProps) for (r in s = e.defaultProps, s) l[r] === void 0 && (l[r] = s[r]);
  return { $$typeof: Mr, type: e, key: i, ref: o, props: l, _owner: Li.current };
}
function hd(e, t) {
  return { $$typeof: Mr, type: e.type, key: t, ref: e.ref, props: e.props, _owner: e._owner };
}
function Mi(e) {
  return typeof e == "object" && e !== null && e.$$typeof === Mr;
}
function vd(e) {
  var t = { "=": "=0", ":": "=2" };
  return "$" + e.replace(/[=:]/g, function(n) {
    return t[n];
  });
}
var Do = /\/+/g;
function pa(e, t) {
  return typeof e == "object" && e !== null && e.key != null ? vd("" + e.key) : t.toString(36);
}
function sl(e, t, n, r, l) {
  var i = typeof e;
  (i === "undefined" || i === "boolean") && (e = null);
  var o = !1;
  if (e === null) o = !0;
  else switch (i) {
    case "string":
    case "number":
      o = !0;
      break;
    case "object":
      switch (e.$$typeof) {
        case Mr:
        case ld:
          o = !0;
      }
  }
  if (o) return o = e, l = l(o), e = r === "" ? "." + pa(o, 0) : r, To(l) ? (n = "", e != null && (n = e.replace(Do, "$&/") + "/"), sl(l, t, n, "", function(d) {
    return d;
  })) : l != null && (Mi(l) && (l = hd(l, n + (!l.key || o && o.key === l.key ? "" : ("" + l.key).replace(Do, "$&/") + "/") + e)), t.push(l)), 1;
  if (o = 0, r = r === "" ? "." : r + ":", To(e)) for (var s = 0; s < e.length; s++) {
    i = e[s];
    var u = r + pa(i, s);
    o += sl(i, t, n, u, l);
  }
  else if (u = md(e), typeof u == "function") for (e = u.call(e), s = 0; !(i = e.next()).done; ) i = i.value, u = r + pa(i, s++), o += sl(i, t, n, u, l);
  else if (i === "object") throw t = String(e), Error("Objects are not valid as a React child (found: " + (t === "[object Object]" ? "object with keys {" + Object.keys(e).join(", ") + "}" : t) + "). If you meant to render a collection of children, use an array instead.");
  return o;
}
function Qr(e, t, n) {
  if (e == null) return e;
  var r = [], l = 0;
  return sl(e, r, "", "", function(i) {
    return t.call(n, i, l++);
  }), r;
}
function gd(e) {
  if (e._status === -1) {
    var t = e._result;
    t = t(), t.then(function(n) {
      (e._status === 0 || e._status === -1) && (e._status = 1, e._result = n);
    }, function(n) {
      (e._status === 0 || e._status === -1) && (e._status = 2, e._result = n);
    }), e._status === -1 && (e._status = 0, e._result = t);
  }
  if (e._status === 1) return e._result.default;
  throw e._result;
}
var De = { current: null }, ul = { transition: null }, xd = { ReactCurrentDispatcher: De, ReactCurrentBatchConfig: ul, ReactCurrentOwner: Li };
function Ys() {
  throw Error("act(...) is not supported in production builds of React.");
}
W.Children = { map: Qr, forEach: function(e, t, n) {
  Qr(e, function() {
    t.apply(this, arguments);
  }, n);
}, count: function(e) {
  var t = 0;
  return Qr(e, function() {
    t++;
  }), t;
}, toArray: function(e) {
  return Qr(e, function(t) {
    return t;
  }) || [];
}, only: function(e) {
  if (!Mi(e)) throw Error("React.Children.only expected to receive a single React element child.");
  return e;
} };
W.Component = Vn;
W.Fragment = ad;
W.Profiler = od;
W.PureComponent = Ti;
W.StrictMode = id;
W.Suspense = dd;
W.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED = xd;
W.act = Ys;
W.cloneElement = function(e, t, n) {
  if (e == null) throw Error("React.cloneElement(...): The argument must be a React element, but you passed " + e + ".");
  var r = Ws({}, e.props), l = e.key, i = e.ref, o = e._owner;
  if (t != null) {
    if (t.ref !== void 0 && (i = t.ref, o = Li.current), t.key !== void 0 && (l = "" + t.key), e.type && e.type.defaultProps) var s = e.type.defaultProps;
    for (u in t) Ks.call(t, u) && !qs.hasOwnProperty(u) && (r[u] = t[u] === void 0 && s !== void 0 ? s[u] : t[u]);
  }
  var u = arguments.length - 2;
  if (u === 1) r.children = n;
  else if (1 < u) {
    s = Array(u);
    for (var d = 0; d < u; d++) s[d] = arguments[d + 2];
    r.children = s;
  }
  return { $$typeof: Mr, type: e.type, key: l, ref: i, props: r, _owner: o };
};
W.createContext = function(e) {
  return e = { $$typeof: ud, _currentValue: e, _currentValue2: e, _threadCount: 0, Provider: null, Consumer: null, _defaultValue: null, _globalName: null }, e.Provider = { $$typeof: sd, _context: e }, e.Consumer = e;
};
W.createElement = bs;
W.createFactory = function(e) {
  var t = bs.bind(null, e);
  return t.type = e, t;
};
W.createRef = function() {
  return { current: null };
};
W.forwardRef = function(e) {
  return { $$typeof: cd, render: e };
};
W.isValidElement = Mi;
W.lazy = function(e) {
  return { $$typeof: pd, _payload: { _status: -1, _result: e }, _init: gd };
};
W.memo = function(e, t) {
  return { $$typeof: fd, type: e, compare: t === void 0 ? null : t };
};
W.startTransition = function(e) {
  var t = ul.transition;
  ul.transition = {};
  try {
    e();
  } finally {
    ul.transition = t;
  }
};
W.unstable_act = Ys;
W.useCallback = function(e, t) {
  return De.current.useCallback(e, t);
};
W.useContext = function(e) {
  return De.current.useContext(e);
};
W.useDebugValue = function() {
};
W.useDeferredValue = function(e) {
  return De.current.useDeferredValue(e);
};
W.useEffect = function(e, t) {
  return De.current.useEffect(e, t);
};
W.useId = function() {
  return De.current.useId();
};
W.useImperativeHandle = function(e, t, n) {
  return De.current.useImperativeHandle(e, t, n);
};
W.useInsertionEffect = function(e, t) {
  return De.current.useInsertionEffect(e, t);
};
W.useLayoutEffect = function(e, t) {
  return De.current.useLayoutEffect(e, t);
};
W.useMemo = function(e, t) {
  return De.current.useMemo(e, t);
};
W.useReducer = function(e, t, n) {
  return De.current.useReducer(e, t, n);
};
W.useRef = function(e) {
  return De.current.useRef(e);
};
W.useState = function(e) {
  return De.current.useState(e);
};
W.useSyncExternalStore = function(e, t, n) {
  return De.current.useSyncExternalStore(e, t, n);
};
W.useTransition = function() {
  return De.current.useTransition();
};
W.version = "18.3.1";
Bs.exports = W;
var j = Bs.exports;
/**
 * @license React
 * react-jsx-runtime.production.min.js
 *
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */
var yd = j, jd = Symbol.for("react.element"), Nd = Symbol.for("react.fragment"), wd = Object.prototype.hasOwnProperty, Sd = yd.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED.ReactCurrentOwner, kd = { key: !0, ref: !0, __self: !0, __source: !0 };
function Xs(e, t, n) {
  var r, l = {}, i = null, o = null;
  n !== void 0 && (i = "" + n), t.key !== void 0 && (i = "" + t.key), t.ref !== void 0 && (o = t.ref);
  for (r in t) wd.call(t, r) && !kd.hasOwnProperty(r) && (l[r] = t[r]);
  if (e && e.defaultProps) for (r in t = e.defaultProps, t) l[r] === void 0 && (l[r] = t[r]);
  return { $$typeof: jd, type: e, key: i, ref: o, props: l, _owner: Sd.current };
}
Ql.Fragment = Nd;
Ql.jsx = Xs;
Ql.jsxs = Xs;
Vs.exports = Ql;
var a = Vs.exports, Zs = { exports: {} }, Ke = {}, Js = { exports: {} }, eu = {};
/**
 * @license React
 * scheduler.production.min.js
 *
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */
(function(e) {
  function t(C, x) {
    var S = C.length;
    C.push(x);
    e: for (; 0 < S; ) {
      var A = S - 1 >>> 1, H = C[A];
      if (0 < l(H, x)) C[A] = x, C[S] = H, S = A;
      else break e;
    }
  }
  function n(C) {
    return C.length === 0 ? null : C[0];
  }
  function r(C) {
    if (C.length === 0) return null;
    var x = C[0], S = C.pop();
    if (S !== x) {
      C[0] = S;
      e: for (var A = 0, H = C.length, G = H >>> 1; A < G; ) {
        var me = 2 * (A + 1) - 1, ge = C[me], te = me + 1, U = C[te];
        if (0 > l(ge, S)) te < H && 0 > l(U, ge) ? (C[A] = U, C[te] = S, A = te) : (C[A] = ge, C[me] = S, A = me);
        else if (te < H && 0 > l(U, S)) C[A] = U, C[te] = S, A = te;
        else break e;
      }
    }
    return x;
  }
  function l(C, x) {
    var S = C.sortIndex - x.sortIndex;
    return S !== 0 ? S : C.id - x.id;
  }
  if (typeof performance == "object" && typeof performance.now == "function") {
    var i = performance;
    e.unstable_now = function() {
      return i.now();
    };
  } else {
    var o = Date, s = o.now();
    e.unstable_now = function() {
      return o.now() - s;
    };
  }
  var u = [], d = [], y = 1, c = null, m = 3, v = !1, g = !1, N = !1, F = typeof setTimeout == "function" ? setTimeout : null, p = typeof clearTimeout == "function" ? clearTimeout : null, f = typeof setImmediate < "u" ? setImmediate : null;
  typeof navigator < "u" && navigator.scheduling !== void 0 && navigator.scheduling.isInputPending !== void 0 && navigator.scheduling.isInputPending.bind(navigator.scheduling);
  function h(C) {
    for (var x = n(d); x !== null; ) {
      if (x.callback === null) r(d);
      else if (x.startTime <= C) r(d), x.sortIndex = x.expirationTime, t(u, x);
      else break;
      x = n(d);
    }
  }
  function k(C) {
    if (N = !1, h(C), !g) if (n(u) !== null) g = !0, de(z);
    else {
      var x = n(d);
      x !== null && Ne(k, x.startTime - C);
    }
  }
  function z(C, x) {
    g = !1, N && (N = !1, p(D), D = -1), v = !0;
    var S = m;
    try {
      for (h(x), c = n(u); c !== null && (!(c.expirationTime > x) || C && !_()); ) {
        var A = c.callback;
        if (typeof A == "function") {
          c.callback = null, m = c.priorityLevel;
          var H = A(c.expirationTime <= x);
          x = e.unstable_now(), typeof H == "function" ? c.callback = H : c === n(u) && r(u), h(x);
        } else r(u);
        c = n(u);
      }
      if (c !== null) var G = !0;
      else {
        var me = n(d);
        me !== null && Ne(k, me.startTime - x), G = !1;
      }
      return G;
    } finally {
      c = null, m = S, v = !1;
    }
  }
  var R = !1, T = null, D = -1, L = 5, E = -1;
  function _() {
    return !(e.unstable_now() - E < L);
  }
  function $() {
    if (T !== null) {
      var C = e.unstable_now();
      E = C;
      var x = !0;
      try {
        x = T(!0, C);
      } finally {
        x ? Ce() : (R = !1, T = null);
      }
    } else R = !1;
  }
  var Ce;
  if (typeof f == "function") Ce = function() {
    f($);
  };
  else if (typeof MessageChannel < "u") {
    var Be = new MessageChannel(), be = Be.port2;
    Be.port1.onmessage = $, Ce = function() {
      be.postMessage(null);
    };
  } else Ce = function() {
    F($, 0);
  };
  function de(C) {
    T = C, R || (R = !0, Ce());
  }
  function Ne(C, x) {
    D = F(function() {
      C(e.unstable_now());
    }, x);
  }
  e.unstable_IdlePriority = 5, e.unstable_ImmediatePriority = 1, e.unstable_LowPriority = 4, e.unstable_NormalPriority = 3, e.unstable_Profiling = null, e.unstable_UserBlockingPriority = 2, e.unstable_cancelCallback = function(C) {
    C.callback = null;
  }, e.unstable_continueExecution = function() {
    g || v || (g = !0, de(z));
  }, e.unstable_forceFrameRate = function(C) {
    0 > C || 125 < C ? console.error("forceFrameRate takes a positive int between 0 and 125, forcing frame rates higher than 125 fps is not supported") : L = 0 < C ? Math.floor(1e3 / C) : 5;
  }, e.unstable_getCurrentPriorityLevel = function() {
    return m;
  }, e.unstable_getFirstCallbackNode = function() {
    return n(u);
  }, e.unstable_next = function(C) {
    switch (m) {
      case 1:
      case 2:
      case 3:
        var x = 3;
        break;
      default:
        x = m;
    }
    var S = m;
    m = x;
    try {
      return C();
    } finally {
      m = S;
    }
  }, e.unstable_pauseExecution = function() {
  }, e.unstable_requestPaint = function() {
  }, e.unstable_runWithPriority = function(C, x) {
    switch (C) {
      case 1:
      case 2:
      case 3:
      case 4:
      case 5:
        break;
      default:
        C = 3;
    }
    var S = m;
    m = C;
    try {
      return x();
    } finally {
      m = S;
    }
  }, e.unstable_scheduleCallback = function(C, x, S) {
    var A = e.unstable_now();
    switch (typeof S == "object" && S !== null ? (S = S.delay, S = typeof S == "number" && 0 < S ? A + S : A) : S = A, C) {
      case 1:
        var H = -1;
        break;
      case 2:
        H = 250;
        break;
      case 5:
        H = 1073741823;
        break;
      case 4:
        H = 1e4;
        break;
      default:
        H = 5e3;
    }
    return H = S + H, C = { id: y++, callback: x, priorityLevel: C, startTime: S, expirationTime: H, sortIndex: -1 }, S > A ? (C.sortIndex = S, t(d, C), n(u) === null && C === n(d) && (N ? (p(D), D = -1) : N = !0, Ne(k, S - A))) : (C.sortIndex = H, t(u, C), g || v || (g = !0, de(z))), C;
  }, e.unstable_shouldYield = _, e.unstable_wrapCallback = function(C) {
    var x = m;
    return function() {
      var S = m;
      m = x;
      try {
        return C.apply(this, arguments);
      } finally {
        m = S;
      }
    };
  };
})(eu);
Js.exports = eu;
var Cd = Js.exports;
/**
 * @license React
 * react-dom.production.min.js
 *
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */
var Ed = j, Ge = Cd;
function P(e) {
  for (var t = "https://reactjs.org/docs/error-decoder.html?invariant=" + e, n = 1; n < arguments.length; n++) t += "&args[]=" + encodeURIComponent(arguments[n]);
  return "Minified React error #" + e + "; visit " + t + " for the full message or use the non-minified dev environment for full errors and additional helpful warnings.";
}
var tu = /* @__PURE__ */ new Set(), xr = {};
function fn(e, t) {
  Dn(e, t), Dn(e + "Capture", t);
}
function Dn(e, t) {
  for (xr[e] = t, e = 0; e < t.length; e++) tu.add(t[e]);
}
var wt = !(typeof window > "u" || typeof window.document > "u" || typeof window.document.createElement > "u"), Ua = Object.prototype.hasOwnProperty, Id = /^[:A-Z_a-z\u00C0-\u00D6\u00D8-\u00F6\u00F8-\u02FF\u0370-\u037D\u037F-\u1FFF\u200C-\u200D\u2070-\u218F\u2C00-\u2FEF\u3001-\uD7FF\uF900-\uFDCF\uFDF0-\uFFFD][:A-Z_a-z\u00C0-\u00D6\u00D8-\u00F6\u00F8-\u02FF\u0370-\u037D\u037F-\u1FFF\u200C-\u200D\u2070-\u218F\u2C00-\u2FEF\u3001-\uD7FF\uF900-\uFDCF\uFDF0-\uFFFD\-.0-9\u00B7\u0300-\u036F\u203F-\u2040]*$/, Lo = {}, Mo = {};
function Pd(e) {
  return Ua.call(Mo, e) ? !0 : Ua.call(Lo, e) ? !1 : Id.test(e) ? Mo[e] = !0 : (Lo[e] = !0, !1);
}
function _d(e, t, n, r) {
  if (n !== null && n.type === 0) return !1;
  switch (typeof t) {
    case "function":
    case "symbol":
      return !0;
    case "boolean":
      return r ? !1 : n !== null ? !n.acceptsBooleans : (e = e.toLowerCase().slice(0, 5), e !== "data-" && e !== "aria-");
    default:
      return !1;
  }
}
function Fd(e, t, n, r) {
  if (t === null || typeof t > "u" || _d(e, t, n, r)) return !0;
  if (r) return !1;
  if (n !== null) switch (n.type) {
    case 3:
      return !t;
    case 4:
      return t === !1;
    case 5:
      return isNaN(t);
    case 6:
      return isNaN(t) || 1 > t;
  }
  return !1;
}
function Le(e, t, n, r, l, i, o) {
  this.acceptsBooleans = t === 2 || t === 3 || t === 4, this.attributeName = r, this.attributeNamespace = l, this.mustUseProperty = n, this.propertyName = e, this.type = t, this.sanitizeURL = i, this.removeEmptyString = o;
}
var ke = {};
"children dangerouslySetInnerHTML defaultValue defaultChecked innerHTML suppressContentEditableWarning suppressHydrationWarning style".split(" ").forEach(function(e) {
  ke[e] = new Le(e, 0, !1, e, null, !1, !1);
});
[["acceptCharset", "accept-charset"], ["className", "class"], ["htmlFor", "for"], ["httpEquiv", "http-equiv"]].forEach(function(e) {
  var t = e[0];
  ke[t] = new Le(t, 1, !1, e[1], null, !1, !1);
});
["contentEditable", "draggable", "spellCheck", "value"].forEach(function(e) {
  ke[e] = new Le(e, 2, !1, e.toLowerCase(), null, !1, !1);
});
["autoReverse", "externalResourcesRequired", "focusable", "preserveAlpha"].forEach(function(e) {
  ke[e] = new Le(e, 2, !1, e, null, !1, !1);
});
"allowFullScreen async autoFocus autoPlay controls default defer disabled disablePictureInPicture disableRemotePlayback formNoValidate hidden loop noModule noValidate open playsInline readOnly required reversed scoped seamless itemScope".split(" ").forEach(function(e) {
  ke[e] = new Le(e, 3, !1, e.toLowerCase(), null, !1, !1);
});
["checked", "multiple", "muted", "selected"].forEach(function(e) {
  ke[e] = new Le(e, 3, !0, e, null, !1, !1);
});
["capture", "download"].forEach(function(e) {
  ke[e] = new Le(e, 4, !1, e, null, !1, !1);
});
["cols", "rows", "size", "span"].forEach(function(e) {
  ke[e] = new Le(e, 6, !1, e, null, !1, !1);
});
["rowSpan", "start"].forEach(function(e) {
  ke[e] = new Le(e, 5, !1, e.toLowerCase(), null, !1, !1);
});
var Oi = /[\-:]([a-z])/g;
function $i(e) {
  return e[1].toUpperCase();
}
"accent-height alignment-baseline arabic-form baseline-shift cap-height clip-path clip-rule color-interpolation color-interpolation-filters color-profile color-rendering dominant-baseline enable-background fill-opacity fill-rule flood-color flood-opacity font-family font-size font-size-adjust font-stretch font-style font-variant font-weight glyph-name glyph-orientation-horizontal glyph-orientation-vertical horiz-adv-x horiz-origin-x image-rendering letter-spacing lighting-color marker-end marker-mid marker-start overline-position overline-thickness paint-order panose-1 pointer-events rendering-intent shape-rendering stop-color stop-opacity strikethrough-position strikethrough-thickness stroke-dasharray stroke-dashoffset stroke-linecap stroke-linejoin stroke-miterlimit stroke-opacity stroke-width text-anchor text-decoration text-rendering underline-position underline-thickness unicode-bidi unicode-range units-per-em v-alphabetic v-hanging v-ideographic v-mathematical vector-effect vert-adv-y vert-origin-x vert-origin-y word-spacing writing-mode xmlns:xlink x-height".split(" ").forEach(function(e) {
  var t = e.replace(
    Oi,
    $i
  );
  ke[t] = new Le(t, 1, !1, e, null, !1, !1);
});
"xlink:actuate xlink:arcrole xlink:role xlink:show xlink:title xlink:type".split(" ").forEach(function(e) {
  var t = e.replace(Oi, $i);
  ke[t] = new Le(t, 1, !1, e, "http://www.w3.org/1999/xlink", !1, !1);
});
["xml:base", "xml:lang", "xml:space"].forEach(function(e) {
  var t = e.replace(Oi, $i);
  ke[t] = new Le(t, 1, !1, e, "http://www.w3.org/XML/1998/namespace", !1, !1);
});
["tabIndex", "crossOrigin"].forEach(function(e) {
  ke[e] = new Le(e, 1, !1, e.toLowerCase(), null, !1, !1);
});
ke.xlinkHref = new Le("xlinkHref", 1, !1, "xlink:href", "http://www.w3.org/1999/xlink", !0, !1);
["src", "href", "action", "formAction"].forEach(function(e) {
  ke[e] = new Le(e, 1, !1, e.toLowerCase(), null, !0, !0);
});
function Ai(e, t, n, r) {
  var l = ke.hasOwnProperty(t) ? ke[t] : null;
  (l !== null ? l.type !== 0 : r || !(2 < t.length) || t[0] !== "o" && t[0] !== "O" || t[1] !== "n" && t[1] !== "N") && (Fd(t, n, l, r) && (n = null), r || l === null ? Pd(t) && (n === null ? e.removeAttribute(t) : e.setAttribute(t, "" + n)) : l.mustUseProperty ? e[l.propertyName] = n === null ? l.type === 3 ? !1 : "" : n : (t = l.attributeName, r = l.attributeNamespace, n === null ? e.removeAttribute(t) : (l = l.type, n = l === 3 || l === 4 && n === !0 ? "" : "" + n, r ? e.setAttributeNS(r, t, n) : e.setAttribute(t, n))));
}
var Et = Ed.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED, Gr = Symbol.for("react.element"), vn = Symbol.for("react.portal"), gn = Symbol.for("react.fragment"), Ui = Symbol.for("react.strict_mode"), Va = Symbol.for("react.profiler"), nu = Symbol.for("react.provider"), ru = Symbol.for("react.context"), Vi = Symbol.for("react.forward_ref"), Ba = Symbol.for("react.suspense"), Ha = Symbol.for("react.suspense_list"), Bi = Symbol.for("react.memo"), Rt = Symbol.for("react.lazy"), lu = Symbol.for("react.offscreen"), Oo = Symbol.iterator;
function Yn(e) {
  return e === null || typeof e != "object" ? null : (e = Oo && e[Oo] || e["@@iterator"], typeof e == "function" ? e : null);
}
var le = Object.assign, ma;
function lr(e) {
  if (ma === void 0) try {
    throw Error();
  } catch (n) {
    var t = n.stack.trim().match(/\n( *(at )?)/);
    ma = t && t[1] || "";
  }
  return `
` + ma + e;
}
var ha = !1;
function va(e, t) {
  if (!e || ha) return "";
  ha = !0;
  var n = Error.prepareStackTrace;
  Error.prepareStackTrace = void 0;
  try {
    if (t) if (t = function() {
      throw Error();
    }, Object.defineProperty(t.prototype, "props", { set: function() {
      throw Error();
    } }), typeof Reflect == "object" && Reflect.construct) {
      try {
        Reflect.construct(t, []);
      } catch (d) {
        var r = d;
      }
      Reflect.construct(e, [], t);
    } else {
      try {
        t.call();
      } catch (d) {
        r = d;
      }
      e.call(t.prototype);
    }
    else {
      try {
        throw Error();
      } catch (d) {
        r = d;
      }
      e();
    }
  } catch (d) {
    if (d && r && typeof d.stack == "string") {
      for (var l = d.stack.split(`
`), i = r.stack.split(`
`), o = l.length - 1, s = i.length - 1; 1 <= o && 0 <= s && l[o] !== i[s]; ) s--;
      for (; 1 <= o && 0 <= s; o--, s--) if (l[o] !== i[s]) {
        if (o !== 1 || s !== 1)
          do
            if (o--, s--, 0 > s || l[o] !== i[s]) {
              var u = `
` + l[o].replace(" at new ", " at ");
              return e.displayName && u.includes("<anonymous>") && (u = u.replace("<anonymous>", e.displayName)), u;
            }
          while (1 <= o && 0 <= s);
        break;
      }
    }
  } finally {
    ha = !1, Error.prepareStackTrace = n;
  }
  return (e = e ? e.displayName || e.name : "") ? lr(e) : "";
}
function Rd(e) {
  switch (e.tag) {
    case 5:
      return lr(e.type);
    case 16:
      return lr("Lazy");
    case 13:
      return lr("Suspense");
    case 19:
      return lr("SuspenseList");
    case 0:
    case 2:
    case 15:
      return e = va(e.type, !1), e;
    case 11:
      return e = va(e.type.render, !1), e;
    case 1:
      return e = va(e.type, !0), e;
    default:
      return "";
  }
}
function Wa(e) {
  if (e == null) return null;
  if (typeof e == "function") return e.displayName || e.name || null;
  if (typeof e == "string") return e;
  switch (e) {
    case gn:
      return "Fragment";
    case vn:
      return "Portal";
    case Va:
      return "Profiler";
    case Ui:
      return "StrictMode";
    case Ba:
      return "Suspense";
    case Ha:
      return "SuspenseList";
  }
  if (typeof e == "object") switch (e.$$typeof) {
    case ru:
      return (e.displayName || "Context") + ".Consumer";
    case nu:
      return (e._context.displayName || "Context") + ".Provider";
    case Vi:
      var t = e.render;
      return e = e.displayName, e || (e = t.displayName || t.name || "", e = e !== "" ? "ForwardRef(" + e + ")" : "ForwardRef"), e;
    case Bi:
      return t = e.displayName || null, t !== null ? t : Wa(e.type) || "Memo";
    case Rt:
      t = e._payload, e = e._init;
      try {
        return Wa(e(t));
      } catch {
      }
  }
  return null;
}
function zd(e) {
  var t = e.type;
  switch (e.tag) {
    case 24:
      return "Cache";
    case 9:
      return (t.displayName || "Context") + ".Consumer";
    case 10:
      return (t._context.displayName || "Context") + ".Provider";
    case 18:
      return "DehydratedFragment";
    case 11:
      return e = t.render, e = e.displayName || e.name || "", t.displayName || (e !== "" ? "ForwardRef(" + e + ")" : "ForwardRef");
    case 7:
      return "Fragment";
    case 5:
      return t;
    case 4:
      return "Portal";
    case 3:
      return "Root";
    case 6:
      return "Text";
    case 16:
      return Wa(t);
    case 8:
      return t === Ui ? "StrictMode" : "Mode";
    case 22:
      return "Offscreen";
    case 12:
      return "Profiler";
    case 21:
      return "Scope";
    case 13:
      return "Suspense";
    case 19:
      return "SuspenseList";
    case 25:
      return "TracingMarker";
    case 1:
    case 0:
    case 17:
    case 2:
    case 14:
    case 15:
      if (typeof t == "function") return t.displayName || t.name || null;
      if (typeof t == "string") return t;
  }
  return null;
}
function Qt(e) {
  switch (typeof e) {
    case "boolean":
    case "number":
    case "string":
    case "undefined":
      return e;
    case "object":
      return e;
    default:
      return "";
  }
}
function au(e) {
  var t = e.type;
  return (e = e.nodeName) && e.toLowerCase() === "input" && (t === "checkbox" || t === "radio");
}
function Td(e) {
  var t = au(e) ? "checked" : "value", n = Object.getOwnPropertyDescriptor(e.constructor.prototype, t), r = "" + e[t];
  if (!e.hasOwnProperty(t) && typeof n < "u" && typeof n.get == "function" && typeof n.set == "function") {
    var l = n.get, i = n.set;
    return Object.defineProperty(e, t, { configurable: !0, get: function() {
      return l.call(this);
    }, set: function(o) {
      r = "" + o, i.call(this, o);
    } }), Object.defineProperty(e, t, { enumerable: n.enumerable }), { getValue: function() {
      return r;
    }, setValue: function(o) {
      r = "" + o;
    }, stopTracking: function() {
      e._valueTracker = null, delete e[t];
    } };
  }
}
function Kr(e) {
  e._valueTracker || (e._valueTracker = Td(e));
}
function iu(e) {
  if (!e) return !1;
  var t = e._valueTracker;
  if (!t) return !0;
  var n = t.getValue(), r = "";
  return e && (r = au(e) ? e.checked ? "true" : "false" : e.value), e = r, e !== n ? (t.setValue(e), !0) : !1;
}
function Nl(e) {
  if (e = e || (typeof document < "u" ? document : void 0), typeof e > "u") return null;
  try {
    return e.activeElement || e.body;
  } catch {
    return e.body;
  }
}
function Qa(e, t) {
  var n = t.checked;
  return le({}, t, { defaultChecked: void 0, defaultValue: void 0, value: void 0, checked: n ?? e._wrapperState.initialChecked });
}
function $o(e, t) {
  var n = t.defaultValue == null ? "" : t.defaultValue, r = t.checked != null ? t.checked : t.defaultChecked;
  n = Qt(t.value != null ? t.value : n), e._wrapperState = { initialChecked: r, initialValue: n, controlled: t.type === "checkbox" || t.type === "radio" ? t.checked != null : t.value != null };
}
function ou(e, t) {
  t = t.checked, t != null && Ai(e, "checked", t, !1);
}
function Ga(e, t) {
  ou(e, t);
  var n = Qt(t.value), r = t.type;
  if (n != null) r === "number" ? (n === 0 && e.value === "" || e.value != n) && (e.value = "" + n) : e.value !== "" + n && (e.value = "" + n);
  else if (r === "submit" || r === "reset") {
    e.removeAttribute("value");
    return;
  }
  t.hasOwnProperty("value") ? Ka(e, t.type, n) : t.hasOwnProperty("defaultValue") && Ka(e, t.type, Qt(t.defaultValue)), t.checked == null && t.defaultChecked != null && (e.defaultChecked = !!t.defaultChecked);
}
function Ao(e, t, n) {
  if (t.hasOwnProperty("value") || t.hasOwnProperty("defaultValue")) {
    var r = t.type;
    if (!(r !== "submit" && r !== "reset" || t.value !== void 0 && t.value !== null)) return;
    t = "" + e._wrapperState.initialValue, n || t === e.value || (e.value = t), e.defaultValue = t;
  }
  n = e.name, n !== "" && (e.name = ""), e.defaultChecked = !!e._wrapperState.initialChecked, n !== "" && (e.name = n);
}
function Ka(e, t, n) {
  (t !== "number" || Nl(e.ownerDocument) !== e) && (n == null ? e.defaultValue = "" + e._wrapperState.initialValue : e.defaultValue !== "" + n && (e.defaultValue = "" + n));
}
var ar = Array.isArray;
function Pn(e, t, n, r) {
  if (e = e.options, t) {
    t = {};
    for (var l = 0; l < n.length; l++) t["$" + n[l]] = !0;
    for (n = 0; n < e.length; n++) l = t.hasOwnProperty("$" + e[n].value), e[n].selected !== l && (e[n].selected = l), l && r && (e[n].defaultSelected = !0);
  } else {
    for (n = "" + Qt(n), t = null, l = 0; l < e.length; l++) {
      if (e[l].value === n) {
        e[l].selected = !0, r && (e[l].defaultSelected = !0);
        return;
      }
      t !== null || e[l].disabled || (t = e[l]);
    }
    t !== null && (t.selected = !0);
  }
}
function qa(e, t) {
  if (t.dangerouslySetInnerHTML != null) throw Error(P(91));
  return le({}, t, { value: void 0, defaultValue: void 0, children: "" + e._wrapperState.initialValue });
}
function Uo(e, t) {
  var n = t.value;
  if (n == null) {
    if (n = t.children, t = t.defaultValue, n != null) {
      if (t != null) throw Error(P(92));
      if (ar(n)) {
        if (1 < n.length) throw Error(P(93));
        n = n[0];
      }
      t = n;
    }
    t == null && (t = ""), n = t;
  }
  e._wrapperState = { initialValue: Qt(n) };
}
function su(e, t) {
  var n = Qt(t.value), r = Qt(t.defaultValue);
  n != null && (n = "" + n, n !== e.value && (e.value = n), t.defaultValue == null && e.defaultValue !== n && (e.defaultValue = n)), r != null && (e.defaultValue = "" + r);
}
function Vo(e) {
  var t = e.textContent;
  t === e._wrapperState.initialValue && t !== "" && t !== null && (e.value = t);
}
function uu(e) {
  switch (e) {
    case "svg":
      return "http://www.w3.org/2000/svg";
    case "math":
      return "http://www.w3.org/1998/Math/MathML";
    default:
      return "http://www.w3.org/1999/xhtml";
  }
}
function ba(e, t) {
  return e == null || e === "http://www.w3.org/1999/xhtml" ? uu(t) : e === "http://www.w3.org/2000/svg" && t === "foreignObject" ? "http://www.w3.org/1999/xhtml" : e;
}
var qr, cu = function(e) {
  return typeof MSApp < "u" && MSApp.execUnsafeLocalFunction ? function(t, n, r, l) {
    MSApp.execUnsafeLocalFunction(function() {
      return e(t, n, r, l);
    });
  } : e;
}(function(e, t) {
  if (e.namespaceURI !== "http://www.w3.org/2000/svg" || "innerHTML" in e) e.innerHTML = t;
  else {
    for (qr = qr || document.createElement("div"), qr.innerHTML = "<svg>" + t.valueOf().toString() + "</svg>", t = qr.firstChild; e.firstChild; ) e.removeChild(e.firstChild);
    for (; t.firstChild; ) e.appendChild(t.firstChild);
  }
});
function yr(e, t) {
  if (t) {
    var n = e.firstChild;
    if (n && n === e.lastChild && n.nodeType === 3) {
      n.nodeValue = t;
      return;
    }
  }
  e.textContent = t;
}
var sr = {
  animationIterationCount: !0,
  aspectRatio: !0,
  borderImageOutset: !0,
  borderImageSlice: !0,
  borderImageWidth: !0,
  boxFlex: !0,
  boxFlexGroup: !0,
  boxOrdinalGroup: !0,
  columnCount: !0,
  columns: !0,
  flex: !0,
  flexGrow: !0,
  flexPositive: !0,
  flexShrink: !0,
  flexNegative: !0,
  flexOrder: !0,
  gridArea: !0,
  gridRow: !0,
  gridRowEnd: !0,
  gridRowSpan: !0,
  gridRowStart: !0,
  gridColumn: !0,
  gridColumnEnd: !0,
  gridColumnSpan: !0,
  gridColumnStart: !0,
  fontWeight: !0,
  lineClamp: !0,
  lineHeight: !0,
  opacity: !0,
  order: !0,
  orphans: !0,
  tabSize: !0,
  widows: !0,
  zIndex: !0,
  zoom: !0,
  fillOpacity: !0,
  floodOpacity: !0,
  stopOpacity: !0,
  strokeDasharray: !0,
  strokeDashoffset: !0,
  strokeMiterlimit: !0,
  strokeOpacity: !0,
  strokeWidth: !0
}, Dd = ["Webkit", "ms", "Moz", "O"];
Object.keys(sr).forEach(function(e) {
  Dd.forEach(function(t) {
    t = t + e.charAt(0).toUpperCase() + e.substring(1), sr[t] = sr[e];
  });
});
function du(e, t, n) {
  return t == null || typeof t == "boolean" || t === "" ? "" : n || typeof t != "number" || t === 0 || sr.hasOwnProperty(e) && sr[e] ? ("" + t).trim() : t + "px";
}
function fu(e, t) {
  e = e.style;
  for (var n in t) if (t.hasOwnProperty(n)) {
    var r = n.indexOf("--") === 0, l = du(n, t[n], r);
    n === "float" && (n = "cssFloat"), r ? e.setProperty(n, l) : e[n] = l;
  }
}
var Ld = le({ menuitem: !0 }, { area: !0, base: !0, br: !0, col: !0, embed: !0, hr: !0, img: !0, input: !0, keygen: !0, link: !0, meta: !0, param: !0, source: !0, track: !0, wbr: !0 });
function Ya(e, t) {
  if (t) {
    if (Ld[e] && (t.children != null || t.dangerouslySetInnerHTML != null)) throw Error(P(137, e));
    if (t.dangerouslySetInnerHTML != null) {
      if (t.children != null) throw Error(P(60));
      if (typeof t.dangerouslySetInnerHTML != "object" || !("__html" in t.dangerouslySetInnerHTML)) throw Error(P(61));
    }
    if (t.style != null && typeof t.style != "object") throw Error(P(62));
  }
}
function Xa(e, t) {
  if (e.indexOf("-") === -1) return typeof t.is == "string";
  switch (e) {
    case "annotation-xml":
    case "color-profile":
    case "font-face":
    case "font-face-src":
    case "font-face-uri":
    case "font-face-format":
    case "font-face-name":
    case "missing-glyph":
      return !1;
    default:
      return !0;
  }
}
var Za = null;
function Hi(e) {
  return e = e.target || e.srcElement || window, e.correspondingUseElement && (e = e.correspondingUseElement), e.nodeType === 3 ? e.parentNode : e;
}
var Ja = null, _n = null, Fn = null;
function Bo(e) {
  if (e = Ar(e)) {
    if (typeof Ja != "function") throw Error(P(280));
    var t = e.stateNode;
    t && (t = Yl(t), Ja(e.stateNode, e.type, t));
  }
}
function pu(e) {
  _n ? Fn ? Fn.push(e) : Fn = [e] : _n = e;
}
function mu() {
  if (_n) {
    var e = _n, t = Fn;
    if (Fn = _n = null, Bo(e), t) for (e = 0; e < t.length; e++) Bo(t[e]);
  }
}
function hu(e, t) {
  return e(t);
}
function vu() {
}
var ga = !1;
function gu(e, t, n) {
  if (ga) return e(t, n);
  ga = !0;
  try {
    return hu(e, t, n);
  } finally {
    ga = !1, (_n !== null || Fn !== null) && (vu(), mu());
  }
}
function jr(e, t) {
  var n = e.stateNode;
  if (n === null) return null;
  var r = Yl(n);
  if (r === null) return null;
  n = r[t];
  e: switch (t) {
    case "onClick":
    case "onClickCapture":
    case "onDoubleClick":
    case "onDoubleClickCapture":
    case "onMouseDown":
    case "onMouseDownCapture":
    case "onMouseMove":
    case "onMouseMoveCapture":
    case "onMouseUp":
    case "onMouseUpCapture":
    case "onMouseEnter":
      (r = !r.disabled) || (e = e.type, r = !(e === "button" || e === "input" || e === "select" || e === "textarea")), e = !r;
      break e;
    default:
      e = !1;
  }
  if (e) return null;
  if (n && typeof n != "function") throw Error(P(231, t, typeof n));
  return n;
}
var ei = !1;
if (wt) try {
  var Xn = {};
  Object.defineProperty(Xn, "passive", { get: function() {
    ei = !0;
  } }), window.addEventListener("test", Xn, Xn), window.removeEventListener("test", Xn, Xn);
} catch {
  ei = !1;
}
function Md(e, t, n, r, l, i, o, s, u) {
  var d = Array.prototype.slice.call(arguments, 3);
  try {
    t.apply(n, d);
  } catch (y) {
    this.onError(y);
  }
}
var ur = !1, wl = null, Sl = !1, ti = null, Od = { onError: function(e) {
  ur = !0, wl = e;
} };
function $d(e, t, n, r, l, i, o, s, u) {
  ur = !1, wl = null, Md.apply(Od, arguments);
}
function Ad(e, t, n, r, l, i, o, s, u) {
  if ($d.apply(this, arguments), ur) {
    if (ur) {
      var d = wl;
      ur = !1, wl = null;
    } else throw Error(P(198));
    Sl || (Sl = !0, ti = d);
  }
}
function pn(e) {
  var t = e, n = e;
  if (e.alternate) for (; t.return; ) t = t.return;
  else {
    e = t;
    do
      t = e, t.flags & 4098 && (n = t.return), e = t.return;
    while (e);
  }
  return t.tag === 3 ? n : null;
}
function xu(e) {
  if (e.tag === 13) {
    var t = e.memoizedState;
    if (t === null && (e = e.alternate, e !== null && (t = e.memoizedState)), t !== null) return t.dehydrated;
  }
  return null;
}
function Ho(e) {
  if (pn(e) !== e) throw Error(P(188));
}
function Ud(e) {
  var t = e.alternate;
  if (!t) {
    if (t = pn(e), t === null) throw Error(P(188));
    return t !== e ? null : e;
  }
  for (var n = e, r = t; ; ) {
    var l = n.return;
    if (l === null) break;
    var i = l.alternate;
    if (i === null) {
      if (r = l.return, r !== null) {
        n = r;
        continue;
      }
      break;
    }
    if (l.child === i.child) {
      for (i = l.child; i; ) {
        if (i === n) return Ho(l), e;
        if (i === r) return Ho(l), t;
        i = i.sibling;
      }
      throw Error(P(188));
    }
    if (n.return !== r.return) n = l, r = i;
    else {
      for (var o = !1, s = l.child; s; ) {
        if (s === n) {
          o = !0, n = l, r = i;
          break;
        }
        if (s === r) {
          o = !0, r = l, n = i;
          break;
        }
        s = s.sibling;
      }
      if (!o) {
        for (s = i.child; s; ) {
          if (s === n) {
            o = !0, n = i, r = l;
            break;
          }
          if (s === r) {
            o = !0, r = i, n = l;
            break;
          }
          s = s.sibling;
        }
        if (!o) throw Error(P(189));
      }
    }
    if (n.alternate !== r) throw Error(P(190));
  }
  if (n.tag !== 3) throw Error(P(188));
  return n.stateNode.current === n ? e : t;
}
function yu(e) {
  return e = Ud(e), e !== null ? ju(e) : null;
}
function ju(e) {
  if (e.tag === 5 || e.tag === 6) return e;
  for (e = e.child; e !== null; ) {
    var t = ju(e);
    if (t !== null) return t;
    e = e.sibling;
  }
  return null;
}
var Nu = Ge.unstable_scheduleCallback, Wo = Ge.unstable_cancelCallback, Vd = Ge.unstable_shouldYield, Bd = Ge.unstable_requestPaint, ce = Ge.unstable_now, Hd = Ge.unstable_getCurrentPriorityLevel, Wi = Ge.unstable_ImmediatePriority, wu = Ge.unstable_UserBlockingPriority, kl = Ge.unstable_NormalPriority, Wd = Ge.unstable_LowPriority, Su = Ge.unstable_IdlePriority, Gl = null, pt = null;
function Qd(e) {
  if (pt && typeof pt.onCommitFiberRoot == "function") try {
    pt.onCommitFiberRoot(Gl, e, void 0, (e.current.flags & 128) === 128);
  } catch {
  }
}
var ot = Math.clz32 ? Math.clz32 : qd, Gd = Math.log, Kd = Math.LN2;
function qd(e) {
  return e >>>= 0, e === 0 ? 32 : 31 - (Gd(e) / Kd | 0) | 0;
}
var br = 64, Yr = 4194304;
function ir(e) {
  switch (e & -e) {
    case 1:
      return 1;
    case 2:
      return 2;
    case 4:
      return 4;
    case 8:
      return 8;
    case 16:
      return 16;
    case 32:
      return 32;
    case 64:
    case 128:
    case 256:
    case 512:
    case 1024:
    case 2048:
    case 4096:
    case 8192:
    case 16384:
    case 32768:
    case 65536:
    case 131072:
    case 262144:
    case 524288:
    case 1048576:
    case 2097152:
      return e & 4194240;
    case 4194304:
    case 8388608:
    case 16777216:
    case 33554432:
    case 67108864:
      return e & 130023424;
    case 134217728:
      return 134217728;
    case 268435456:
      return 268435456;
    case 536870912:
      return 536870912;
    case 1073741824:
      return 1073741824;
    default:
      return e;
  }
}
function Cl(e, t) {
  var n = e.pendingLanes;
  if (n === 0) return 0;
  var r = 0, l = e.suspendedLanes, i = e.pingedLanes, o = n & 268435455;
  if (o !== 0) {
    var s = o & ~l;
    s !== 0 ? r = ir(s) : (i &= o, i !== 0 && (r = ir(i)));
  } else o = n & ~l, o !== 0 ? r = ir(o) : i !== 0 && (r = ir(i));
  if (r === 0) return 0;
  if (t !== 0 && t !== r && !(t & l) && (l = r & -r, i = t & -t, l >= i || l === 16 && (i & 4194240) !== 0)) return t;
  if (r & 4 && (r |= n & 16), t = e.entangledLanes, t !== 0) for (e = e.entanglements, t &= r; 0 < t; ) n = 31 - ot(t), l = 1 << n, r |= e[n], t &= ~l;
  return r;
}
function bd(e, t) {
  switch (e) {
    case 1:
    case 2:
    case 4:
      return t + 250;
    case 8:
    case 16:
    case 32:
    case 64:
    case 128:
    case 256:
    case 512:
    case 1024:
    case 2048:
    case 4096:
    case 8192:
    case 16384:
    case 32768:
    case 65536:
    case 131072:
    case 262144:
    case 524288:
    case 1048576:
    case 2097152:
      return t + 5e3;
    case 4194304:
    case 8388608:
    case 16777216:
    case 33554432:
    case 67108864:
      return -1;
    case 134217728:
    case 268435456:
    case 536870912:
    case 1073741824:
      return -1;
    default:
      return -1;
  }
}
function Yd(e, t) {
  for (var n = e.suspendedLanes, r = e.pingedLanes, l = e.expirationTimes, i = e.pendingLanes; 0 < i; ) {
    var o = 31 - ot(i), s = 1 << o, u = l[o];
    u === -1 ? (!(s & n) || s & r) && (l[o] = bd(s, t)) : u <= t && (e.expiredLanes |= s), i &= ~s;
  }
}
function ni(e) {
  return e = e.pendingLanes & -1073741825, e !== 0 ? e : e & 1073741824 ? 1073741824 : 0;
}
function ku() {
  var e = br;
  return br <<= 1, !(br & 4194240) && (br = 64), e;
}
function xa(e) {
  for (var t = [], n = 0; 31 > n; n++) t.push(e);
  return t;
}
function Or(e, t, n) {
  e.pendingLanes |= t, t !== 536870912 && (e.suspendedLanes = 0, e.pingedLanes = 0), e = e.eventTimes, t = 31 - ot(t), e[t] = n;
}
function Xd(e, t) {
  var n = e.pendingLanes & ~t;
  e.pendingLanes = t, e.suspendedLanes = 0, e.pingedLanes = 0, e.expiredLanes &= t, e.mutableReadLanes &= t, e.entangledLanes &= t, t = e.entanglements;
  var r = e.eventTimes;
  for (e = e.expirationTimes; 0 < n; ) {
    var l = 31 - ot(n), i = 1 << l;
    t[l] = 0, r[l] = -1, e[l] = -1, n &= ~i;
  }
}
function Qi(e, t) {
  var n = e.entangledLanes |= t;
  for (e = e.entanglements; n; ) {
    var r = 31 - ot(n), l = 1 << r;
    l & t | e[r] & t && (e[r] |= t), n &= ~l;
  }
}
var q = 0;
function Cu(e) {
  return e &= -e, 1 < e ? 4 < e ? e & 268435455 ? 16 : 536870912 : 4 : 1;
}
var Eu, Gi, Iu, Pu, _u, ri = !1, Xr = [], Ot = null, $t = null, At = null, Nr = /* @__PURE__ */ new Map(), wr = /* @__PURE__ */ new Map(), Tt = [], Zd = "mousedown mouseup touchcancel touchend touchstart auxclick dblclick pointercancel pointerdown pointerup dragend dragstart drop compositionend compositionstart keydown keypress keyup input textInput copy cut paste click change contextmenu reset submit".split(" ");
function Qo(e, t) {
  switch (e) {
    case "focusin":
    case "focusout":
      Ot = null;
      break;
    case "dragenter":
    case "dragleave":
      $t = null;
      break;
    case "mouseover":
    case "mouseout":
      At = null;
      break;
    case "pointerover":
    case "pointerout":
      Nr.delete(t.pointerId);
      break;
    case "gotpointercapture":
    case "lostpointercapture":
      wr.delete(t.pointerId);
  }
}
function Zn(e, t, n, r, l, i) {
  return e === null || e.nativeEvent !== i ? (e = { blockedOn: t, domEventName: n, eventSystemFlags: r, nativeEvent: i, targetContainers: [l] }, t !== null && (t = Ar(t), t !== null && Gi(t)), e) : (e.eventSystemFlags |= r, t = e.targetContainers, l !== null && t.indexOf(l) === -1 && t.push(l), e);
}
function Jd(e, t, n, r, l) {
  switch (t) {
    case "focusin":
      return Ot = Zn(Ot, e, t, n, r, l), !0;
    case "dragenter":
      return $t = Zn($t, e, t, n, r, l), !0;
    case "mouseover":
      return At = Zn(At, e, t, n, r, l), !0;
    case "pointerover":
      var i = l.pointerId;
      return Nr.set(i, Zn(Nr.get(i) || null, e, t, n, r, l)), !0;
    case "gotpointercapture":
      return i = l.pointerId, wr.set(i, Zn(wr.get(i) || null, e, t, n, r, l)), !0;
  }
  return !1;
}
function Fu(e) {
  var t = en(e.target);
  if (t !== null) {
    var n = pn(t);
    if (n !== null) {
      if (t = n.tag, t === 13) {
        if (t = xu(n), t !== null) {
          e.blockedOn = t, _u(e.priority, function() {
            Iu(n);
          });
          return;
        }
      } else if (t === 3 && n.stateNode.current.memoizedState.isDehydrated) {
        e.blockedOn = n.tag === 3 ? n.stateNode.containerInfo : null;
        return;
      }
    }
  }
  e.blockedOn = null;
}
function cl(e) {
  if (e.blockedOn !== null) return !1;
  for (var t = e.targetContainers; 0 < t.length; ) {
    var n = li(e.domEventName, e.eventSystemFlags, t[0], e.nativeEvent);
    if (n === null) {
      n = e.nativeEvent;
      var r = new n.constructor(n.type, n);
      Za = r, n.target.dispatchEvent(r), Za = null;
    } else return t = Ar(n), t !== null && Gi(t), e.blockedOn = n, !1;
    t.shift();
  }
  return !0;
}
function Go(e, t, n) {
  cl(e) && n.delete(t);
}
function ef() {
  ri = !1, Ot !== null && cl(Ot) && (Ot = null), $t !== null && cl($t) && ($t = null), At !== null && cl(At) && (At = null), Nr.forEach(Go), wr.forEach(Go);
}
function Jn(e, t) {
  e.blockedOn === t && (e.blockedOn = null, ri || (ri = !0, Ge.unstable_scheduleCallback(Ge.unstable_NormalPriority, ef)));
}
function Sr(e) {
  function t(l) {
    return Jn(l, e);
  }
  if (0 < Xr.length) {
    Jn(Xr[0], e);
    for (var n = 1; n < Xr.length; n++) {
      var r = Xr[n];
      r.blockedOn === e && (r.blockedOn = null);
    }
  }
  for (Ot !== null && Jn(Ot, e), $t !== null && Jn($t, e), At !== null && Jn(At, e), Nr.forEach(t), wr.forEach(t), n = 0; n < Tt.length; n++) r = Tt[n], r.blockedOn === e && (r.blockedOn = null);
  for (; 0 < Tt.length && (n = Tt[0], n.blockedOn === null); ) Fu(n), n.blockedOn === null && Tt.shift();
}
var Rn = Et.ReactCurrentBatchConfig, El = !0;
function tf(e, t, n, r) {
  var l = q, i = Rn.transition;
  Rn.transition = null;
  try {
    q = 1, Ki(e, t, n, r);
  } finally {
    q = l, Rn.transition = i;
  }
}
function nf(e, t, n, r) {
  var l = q, i = Rn.transition;
  Rn.transition = null;
  try {
    q = 4, Ki(e, t, n, r);
  } finally {
    q = l, Rn.transition = i;
  }
}
function Ki(e, t, n, r) {
  if (El) {
    var l = li(e, t, n, r);
    if (l === null) Pa(e, t, r, Il, n), Qo(e, r);
    else if (Jd(l, e, t, n, r)) r.stopPropagation();
    else if (Qo(e, r), t & 4 && -1 < Zd.indexOf(e)) {
      for (; l !== null; ) {
        var i = Ar(l);
        if (i !== null && Eu(i), i = li(e, t, n, r), i === null && Pa(e, t, r, Il, n), i === l) break;
        l = i;
      }
      l !== null && r.stopPropagation();
    } else Pa(e, t, r, null, n);
  }
}
var Il = null;
function li(e, t, n, r) {
  if (Il = null, e = Hi(r), e = en(e), e !== null) if (t = pn(e), t === null) e = null;
  else if (n = t.tag, n === 13) {
    if (e = xu(t), e !== null) return e;
    e = null;
  } else if (n === 3) {
    if (t.stateNode.current.memoizedState.isDehydrated) return t.tag === 3 ? t.stateNode.containerInfo : null;
    e = null;
  } else t !== e && (e = null);
  return Il = e, null;
}
function Ru(e) {
  switch (e) {
    case "cancel":
    case "click":
    case "close":
    case "contextmenu":
    case "copy":
    case "cut":
    case "auxclick":
    case "dblclick":
    case "dragend":
    case "dragstart":
    case "drop":
    case "focusin":
    case "focusout":
    case "input":
    case "invalid":
    case "keydown":
    case "keypress":
    case "keyup":
    case "mousedown":
    case "mouseup":
    case "paste":
    case "pause":
    case "play":
    case "pointercancel":
    case "pointerdown":
    case "pointerup":
    case "ratechange":
    case "reset":
    case "resize":
    case "seeked":
    case "submit":
    case "touchcancel":
    case "touchend":
    case "touchstart":
    case "volumechange":
    case "change":
    case "selectionchange":
    case "textInput":
    case "compositionstart":
    case "compositionend":
    case "compositionupdate":
    case "beforeblur":
    case "afterblur":
    case "beforeinput":
    case "blur":
    case "fullscreenchange":
    case "focus":
    case "hashchange":
    case "popstate":
    case "select":
    case "selectstart":
      return 1;
    case "drag":
    case "dragenter":
    case "dragexit":
    case "dragleave":
    case "dragover":
    case "mousemove":
    case "mouseout":
    case "mouseover":
    case "pointermove":
    case "pointerout":
    case "pointerover":
    case "scroll":
    case "toggle":
    case "touchmove":
    case "wheel":
    case "mouseenter":
    case "mouseleave":
    case "pointerenter":
    case "pointerleave":
      return 4;
    case "message":
      switch (Hd()) {
        case Wi:
          return 1;
        case wu:
          return 4;
        case kl:
        case Wd:
          return 16;
        case Su:
          return 536870912;
        default:
          return 16;
      }
    default:
      return 16;
  }
}
var Lt = null, qi = null, dl = null;
function zu() {
  if (dl) return dl;
  var e, t = qi, n = t.length, r, l = "value" in Lt ? Lt.value : Lt.textContent, i = l.length;
  for (e = 0; e < n && t[e] === l[e]; e++) ;
  var o = n - e;
  for (r = 1; r <= o && t[n - r] === l[i - r]; r++) ;
  return dl = l.slice(e, 1 < r ? 1 - r : void 0);
}
function fl(e) {
  var t = e.keyCode;
  return "charCode" in e ? (e = e.charCode, e === 0 && t === 13 && (e = 13)) : e = t, e === 10 && (e = 13), 32 <= e || e === 13 ? e : 0;
}
function Zr() {
  return !0;
}
function Ko() {
  return !1;
}
function qe(e) {
  function t(n, r, l, i, o) {
    this._reactName = n, this._targetInst = l, this.type = r, this.nativeEvent = i, this.target = o, this.currentTarget = null;
    for (var s in e) e.hasOwnProperty(s) && (n = e[s], this[s] = n ? n(i) : i[s]);
    return this.isDefaultPrevented = (i.defaultPrevented != null ? i.defaultPrevented : i.returnValue === !1) ? Zr : Ko, this.isPropagationStopped = Ko, this;
  }
  return le(t.prototype, { preventDefault: function() {
    this.defaultPrevented = !0;
    var n = this.nativeEvent;
    n && (n.preventDefault ? n.preventDefault() : typeof n.returnValue != "unknown" && (n.returnValue = !1), this.isDefaultPrevented = Zr);
  }, stopPropagation: function() {
    var n = this.nativeEvent;
    n && (n.stopPropagation ? n.stopPropagation() : typeof n.cancelBubble != "unknown" && (n.cancelBubble = !0), this.isPropagationStopped = Zr);
  }, persist: function() {
  }, isPersistent: Zr }), t;
}
var Bn = { eventPhase: 0, bubbles: 0, cancelable: 0, timeStamp: function(e) {
  return e.timeStamp || Date.now();
}, defaultPrevented: 0, isTrusted: 0 }, bi = qe(Bn), $r = le({}, Bn, { view: 0, detail: 0 }), rf = qe($r), ya, ja, er, Kl = le({}, $r, { screenX: 0, screenY: 0, clientX: 0, clientY: 0, pageX: 0, pageY: 0, ctrlKey: 0, shiftKey: 0, altKey: 0, metaKey: 0, getModifierState: Yi, button: 0, buttons: 0, relatedTarget: function(e) {
  return e.relatedTarget === void 0 ? e.fromElement === e.srcElement ? e.toElement : e.fromElement : e.relatedTarget;
}, movementX: function(e) {
  return "movementX" in e ? e.movementX : (e !== er && (er && e.type === "mousemove" ? (ya = e.screenX - er.screenX, ja = e.screenY - er.screenY) : ja = ya = 0, er = e), ya);
}, movementY: function(e) {
  return "movementY" in e ? e.movementY : ja;
} }), qo = qe(Kl), lf = le({}, Kl, { dataTransfer: 0 }), af = qe(lf), of = le({}, $r, { relatedTarget: 0 }), Na = qe(of), sf = le({}, Bn, { animationName: 0, elapsedTime: 0, pseudoElement: 0 }), uf = qe(sf), cf = le({}, Bn, { clipboardData: function(e) {
  return "clipboardData" in e ? e.clipboardData : window.clipboardData;
} }), df = qe(cf), ff = le({}, Bn, { data: 0 }), bo = qe(ff), pf = {
  Esc: "Escape",
  Spacebar: " ",
  Left: "ArrowLeft",
  Up: "ArrowUp",
  Right: "ArrowRight",
  Down: "ArrowDown",
  Del: "Delete",
  Win: "OS",
  Menu: "ContextMenu",
  Apps: "ContextMenu",
  Scroll: "ScrollLock",
  MozPrintableKey: "Unidentified"
}, mf = {
  8: "Backspace",
  9: "Tab",
  12: "Clear",
  13: "Enter",
  16: "Shift",
  17: "Control",
  18: "Alt",
  19: "Pause",
  20: "CapsLock",
  27: "Escape",
  32: " ",
  33: "PageUp",
  34: "PageDown",
  35: "End",
  36: "Home",
  37: "ArrowLeft",
  38: "ArrowUp",
  39: "ArrowRight",
  40: "ArrowDown",
  45: "Insert",
  46: "Delete",
  112: "F1",
  113: "F2",
  114: "F3",
  115: "F4",
  116: "F5",
  117: "F6",
  118: "F7",
  119: "F8",
  120: "F9",
  121: "F10",
  122: "F11",
  123: "F12",
  144: "NumLock",
  145: "ScrollLock",
  224: "Meta"
}, hf = { Alt: "altKey", Control: "ctrlKey", Meta: "metaKey", Shift: "shiftKey" };
function vf(e) {
  var t = this.nativeEvent;
  return t.getModifierState ? t.getModifierState(e) : (e = hf[e]) ? !!t[e] : !1;
}
function Yi() {
  return vf;
}
var gf = le({}, $r, { key: function(e) {
  if (e.key) {
    var t = pf[e.key] || e.key;
    if (t !== "Unidentified") return t;
  }
  return e.type === "keypress" ? (e = fl(e), e === 13 ? "Enter" : String.fromCharCode(e)) : e.type === "keydown" || e.type === "keyup" ? mf[e.keyCode] || "Unidentified" : "";
}, code: 0, location: 0, ctrlKey: 0, shiftKey: 0, altKey: 0, metaKey: 0, repeat: 0, locale: 0, getModifierState: Yi, charCode: function(e) {
  return e.type === "keypress" ? fl(e) : 0;
}, keyCode: function(e) {
  return e.type === "keydown" || e.type === "keyup" ? e.keyCode : 0;
}, which: function(e) {
  return e.type === "keypress" ? fl(e) : e.type === "keydown" || e.type === "keyup" ? e.keyCode : 0;
} }), xf = qe(gf), yf = le({}, Kl, { pointerId: 0, width: 0, height: 0, pressure: 0, tangentialPressure: 0, tiltX: 0, tiltY: 0, twist: 0, pointerType: 0, isPrimary: 0 }), Yo = qe(yf), jf = le({}, $r, { touches: 0, targetTouches: 0, changedTouches: 0, altKey: 0, metaKey: 0, ctrlKey: 0, shiftKey: 0, getModifierState: Yi }), Nf = qe(jf), wf = le({}, Bn, { propertyName: 0, elapsedTime: 0, pseudoElement: 0 }), Sf = qe(wf), kf = le({}, Kl, {
  deltaX: function(e) {
    return "deltaX" in e ? e.deltaX : "wheelDeltaX" in e ? -e.wheelDeltaX : 0;
  },
  deltaY: function(e) {
    return "deltaY" in e ? e.deltaY : "wheelDeltaY" in e ? -e.wheelDeltaY : "wheelDelta" in e ? -e.wheelDelta : 0;
  },
  deltaZ: 0,
  deltaMode: 0
}), Cf = qe(kf), Ef = [9, 13, 27, 32], Xi = wt && "CompositionEvent" in window, cr = null;
wt && "documentMode" in document && (cr = document.documentMode);
var If = wt && "TextEvent" in window && !cr, Tu = wt && (!Xi || cr && 8 < cr && 11 >= cr), Xo = " ", Zo = !1;
function Du(e, t) {
  switch (e) {
    case "keyup":
      return Ef.indexOf(t.keyCode) !== -1;
    case "keydown":
      return t.keyCode !== 229;
    case "keypress":
    case "mousedown":
    case "focusout":
      return !0;
    default:
      return !1;
  }
}
function Lu(e) {
  return e = e.detail, typeof e == "object" && "data" in e ? e.data : null;
}
var xn = !1;
function Pf(e, t) {
  switch (e) {
    case "compositionend":
      return Lu(t);
    case "keypress":
      return t.which !== 32 ? null : (Zo = !0, Xo);
    case "textInput":
      return e = t.data, e === Xo && Zo ? null : e;
    default:
      return null;
  }
}
function _f(e, t) {
  if (xn) return e === "compositionend" || !Xi && Du(e, t) ? (e = zu(), dl = qi = Lt = null, xn = !1, e) : null;
  switch (e) {
    case "paste":
      return null;
    case "keypress":
      if (!(t.ctrlKey || t.altKey || t.metaKey) || t.ctrlKey && t.altKey) {
        if (t.char && 1 < t.char.length) return t.char;
        if (t.which) return String.fromCharCode(t.which);
      }
      return null;
    case "compositionend":
      return Tu && t.locale !== "ko" ? null : t.data;
    default:
      return null;
  }
}
var Ff = { color: !0, date: !0, datetime: !0, "datetime-local": !0, email: !0, month: !0, number: !0, password: !0, range: !0, search: !0, tel: !0, text: !0, time: !0, url: !0, week: !0 };
function Jo(e) {
  var t = e && e.nodeName && e.nodeName.toLowerCase();
  return t === "input" ? !!Ff[e.type] : t === "textarea";
}
function Mu(e, t, n, r) {
  pu(r), t = Pl(t, "onChange"), 0 < t.length && (n = new bi("onChange", "change", null, n, r), e.push({ event: n, listeners: t }));
}
var dr = null, kr = null;
function Rf(e) {
  Ku(e, 0);
}
function ql(e) {
  var t = Nn(e);
  if (iu(t)) return e;
}
function zf(e, t) {
  if (e === "change") return t;
}
var Ou = !1;
if (wt) {
  var wa;
  if (wt) {
    var Sa = "oninput" in document;
    if (!Sa) {
      var es = document.createElement("div");
      es.setAttribute("oninput", "return;"), Sa = typeof es.oninput == "function";
    }
    wa = Sa;
  } else wa = !1;
  Ou = wa && (!document.documentMode || 9 < document.documentMode);
}
function ts() {
  dr && (dr.detachEvent("onpropertychange", $u), kr = dr = null);
}
function $u(e) {
  if (e.propertyName === "value" && ql(kr)) {
    var t = [];
    Mu(t, kr, e, Hi(e)), gu(Rf, t);
  }
}
function Tf(e, t, n) {
  e === "focusin" ? (ts(), dr = t, kr = n, dr.attachEvent("onpropertychange", $u)) : e === "focusout" && ts();
}
function Df(e) {
  if (e === "selectionchange" || e === "keyup" || e === "keydown") return ql(kr);
}
function Lf(e, t) {
  if (e === "click") return ql(t);
}
function Mf(e, t) {
  if (e === "input" || e === "change") return ql(t);
}
function Of(e, t) {
  return e === t && (e !== 0 || 1 / e === 1 / t) || e !== e && t !== t;
}
var ut = typeof Object.is == "function" ? Object.is : Of;
function Cr(e, t) {
  if (ut(e, t)) return !0;
  if (typeof e != "object" || e === null || typeof t != "object" || t === null) return !1;
  var n = Object.keys(e), r = Object.keys(t);
  if (n.length !== r.length) return !1;
  for (r = 0; r < n.length; r++) {
    var l = n[r];
    if (!Ua.call(t, l) || !ut(e[l], t[l])) return !1;
  }
  return !0;
}
function ns(e) {
  for (; e && e.firstChild; ) e = e.firstChild;
  return e;
}
function rs(e, t) {
  var n = ns(e);
  e = 0;
  for (var r; n; ) {
    if (n.nodeType === 3) {
      if (r = e + n.textContent.length, e <= t && r >= t) return { node: n, offset: t - e };
      e = r;
    }
    e: {
      for (; n; ) {
        if (n.nextSibling) {
          n = n.nextSibling;
          break e;
        }
        n = n.parentNode;
      }
      n = void 0;
    }
    n = ns(n);
  }
}
function Au(e, t) {
  return e && t ? e === t ? !0 : e && e.nodeType === 3 ? !1 : t && t.nodeType === 3 ? Au(e, t.parentNode) : "contains" in e ? e.contains(t) : e.compareDocumentPosition ? !!(e.compareDocumentPosition(t) & 16) : !1 : !1;
}
function Uu() {
  for (var e = window, t = Nl(); t instanceof e.HTMLIFrameElement; ) {
    try {
      var n = typeof t.contentWindow.location.href == "string";
    } catch {
      n = !1;
    }
    if (n) e = t.contentWindow;
    else break;
    t = Nl(e.document);
  }
  return t;
}
function Zi(e) {
  var t = e && e.nodeName && e.nodeName.toLowerCase();
  return t && (t === "input" && (e.type === "text" || e.type === "search" || e.type === "tel" || e.type === "url" || e.type === "password") || t === "textarea" || e.contentEditable === "true");
}
function $f(e) {
  var t = Uu(), n = e.focusedElem, r = e.selectionRange;
  if (t !== n && n && n.ownerDocument && Au(n.ownerDocument.documentElement, n)) {
    if (r !== null && Zi(n)) {
      if (t = r.start, e = r.end, e === void 0 && (e = t), "selectionStart" in n) n.selectionStart = t, n.selectionEnd = Math.min(e, n.value.length);
      else if (e = (t = n.ownerDocument || document) && t.defaultView || window, e.getSelection) {
        e = e.getSelection();
        var l = n.textContent.length, i = Math.min(r.start, l);
        r = r.end === void 0 ? i : Math.min(r.end, l), !e.extend && i > r && (l = r, r = i, i = l), l = rs(n, i);
        var o = rs(
          n,
          r
        );
        l && o && (e.rangeCount !== 1 || e.anchorNode !== l.node || e.anchorOffset !== l.offset || e.focusNode !== o.node || e.focusOffset !== o.offset) && (t = t.createRange(), t.setStart(l.node, l.offset), e.removeAllRanges(), i > r ? (e.addRange(t), e.extend(o.node, o.offset)) : (t.setEnd(o.node, o.offset), e.addRange(t)));
      }
    }
    for (t = [], e = n; e = e.parentNode; ) e.nodeType === 1 && t.push({ element: e, left: e.scrollLeft, top: e.scrollTop });
    for (typeof n.focus == "function" && n.focus(), n = 0; n < t.length; n++) e = t[n], e.element.scrollLeft = e.left, e.element.scrollTop = e.top;
  }
}
var Af = wt && "documentMode" in document && 11 >= document.documentMode, yn = null, ai = null, fr = null, ii = !1;
function ls(e, t, n) {
  var r = n.window === n ? n.document : n.nodeType === 9 ? n : n.ownerDocument;
  ii || yn == null || yn !== Nl(r) || (r = yn, "selectionStart" in r && Zi(r) ? r = { start: r.selectionStart, end: r.selectionEnd } : (r = (r.ownerDocument && r.ownerDocument.defaultView || window).getSelection(), r = { anchorNode: r.anchorNode, anchorOffset: r.anchorOffset, focusNode: r.focusNode, focusOffset: r.focusOffset }), fr && Cr(fr, r) || (fr = r, r = Pl(ai, "onSelect"), 0 < r.length && (t = new bi("onSelect", "select", null, t, n), e.push({ event: t, listeners: r }), t.target = yn)));
}
function Jr(e, t) {
  var n = {};
  return n[e.toLowerCase()] = t.toLowerCase(), n["Webkit" + e] = "webkit" + t, n["Moz" + e] = "moz" + t, n;
}
var jn = { animationend: Jr("Animation", "AnimationEnd"), animationiteration: Jr("Animation", "AnimationIteration"), animationstart: Jr("Animation", "AnimationStart"), transitionend: Jr("Transition", "TransitionEnd") }, ka = {}, Vu = {};
wt && (Vu = document.createElement("div").style, "AnimationEvent" in window || (delete jn.animationend.animation, delete jn.animationiteration.animation, delete jn.animationstart.animation), "TransitionEvent" in window || delete jn.transitionend.transition);
function bl(e) {
  if (ka[e]) return ka[e];
  if (!jn[e]) return e;
  var t = jn[e], n;
  for (n in t) if (t.hasOwnProperty(n) && n in Vu) return ka[e] = t[n];
  return e;
}
var Bu = bl("animationend"), Hu = bl("animationiteration"), Wu = bl("animationstart"), Qu = bl("transitionend"), Gu = /* @__PURE__ */ new Map(), as = "abort auxClick cancel canPlay canPlayThrough click close contextMenu copy cut drag dragEnd dragEnter dragExit dragLeave dragOver dragStart drop durationChange emptied encrypted ended error gotPointerCapture input invalid keyDown keyPress keyUp load loadedData loadedMetadata loadStart lostPointerCapture mouseDown mouseMove mouseOut mouseOver mouseUp paste pause play playing pointerCancel pointerDown pointerMove pointerOut pointerOver pointerUp progress rateChange reset resize seeked seeking stalled submit suspend timeUpdate touchCancel touchEnd touchStart volumeChange scroll toggle touchMove waiting wheel".split(" ");
function Kt(e, t) {
  Gu.set(e, t), fn(t, [e]);
}
for (var Ca = 0; Ca < as.length; Ca++) {
  var Ea = as[Ca], Uf = Ea.toLowerCase(), Vf = Ea[0].toUpperCase() + Ea.slice(1);
  Kt(Uf, "on" + Vf);
}
Kt(Bu, "onAnimationEnd");
Kt(Hu, "onAnimationIteration");
Kt(Wu, "onAnimationStart");
Kt("dblclick", "onDoubleClick");
Kt("focusin", "onFocus");
Kt("focusout", "onBlur");
Kt(Qu, "onTransitionEnd");
Dn("onMouseEnter", ["mouseout", "mouseover"]);
Dn("onMouseLeave", ["mouseout", "mouseover"]);
Dn("onPointerEnter", ["pointerout", "pointerover"]);
Dn("onPointerLeave", ["pointerout", "pointerover"]);
fn("onChange", "change click focusin focusout input keydown keyup selectionchange".split(" "));
fn("onSelect", "focusout contextmenu dragend focusin keydown keyup mousedown mouseup selectionchange".split(" "));
fn("onBeforeInput", ["compositionend", "keypress", "textInput", "paste"]);
fn("onCompositionEnd", "compositionend focusout keydown keypress keyup mousedown".split(" "));
fn("onCompositionStart", "compositionstart focusout keydown keypress keyup mousedown".split(" "));
fn("onCompositionUpdate", "compositionupdate focusout keydown keypress keyup mousedown".split(" "));
var or = "abort canplay canplaythrough durationchange emptied encrypted ended error loadeddata loadedmetadata loadstart pause play playing progress ratechange resize seeked seeking stalled suspend timeupdate volumechange waiting".split(" "), Bf = new Set("cancel close invalid load scroll toggle".split(" ").concat(or));
function is(e, t, n) {
  var r = e.type || "unknown-event";
  e.currentTarget = n, Ad(r, t, void 0, e), e.currentTarget = null;
}
function Ku(e, t) {
  t = (t & 4) !== 0;
  for (var n = 0; n < e.length; n++) {
    var r = e[n], l = r.event;
    r = r.listeners;
    e: {
      var i = void 0;
      if (t) for (var o = r.length - 1; 0 <= o; o--) {
        var s = r[o], u = s.instance, d = s.currentTarget;
        if (s = s.listener, u !== i && l.isPropagationStopped()) break e;
        is(l, s, d), i = u;
      }
      else for (o = 0; o < r.length; o++) {
        if (s = r[o], u = s.instance, d = s.currentTarget, s = s.listener, u !== i && l.isPropagationStopped()) break e;
        is(l, s, d), i = u;
      }
    }
  }
  if (Sl) throw e = ti, Sl = !1, ti = null, e;
}
function X(e, t) {
  var n = t[di];
  n === void 0 && (n = t[di] = /* @__PURE__ */ new Set());
  var r = e + "__bubble";
  n.has(r) || (qu(t, e, 2, !1), n.add(r));
}
function Ia(e, t, n) {
  var r = 0;
  t && (r |= 4), qu(n, e, r, t);
}
var el = "_reactListening" + Math.random().toString(36).slice(2);
function Er(e) {
  if (!e[el]) {
    e[el] = !0, tu.forEach(function(n) {
      n !== "selectionchange" && (Bf.has(n) || Ia(n, !1, e), Ia(n, !0, e));
    });
    var t = e.nodeType === 9 ? e : e.ownerDocument;
    t === null || t[el] || (t[el] = !0, Ia("selectionchange", !1, t));
  }
}
function qu(e, t, n, r) {
  switch (Ru(t)) {
    case 1:
      var l = tf;
      break;
    case 4:
      l = nf;
      break;
    default:
      l = Ki;
  }
  n = l.bind(null, t, n, e), l = void 0, !ei || t !== "touchstart" && t !== "touchmove" && t !== "wheel" || (l = !0), r ? l !== void 0 ? e.addEventListener(t, n, { capture: !0, passive: l }) : e.addEventListener(t, n, !0) : l !== void 0 ? e.addEventListener(t, n, { passive: l }) : e.addEventListener(t, n, !1);
}
function Pa(e, t, n, r, l) {
  var i = r;
  if (!(t & 1) && !(t & 2) && r !== null) e: for (; ; ) {
    if (r === null) return;
    var o = r.tag;
    if (o === 3 || o === 4) {
      var s = r.stateNode.containerInfo;
      if (s === l || s.nodeType === 8 && s.parentNode === l) break;
      if (o === 4) for (o = r.return; o !== null; ) {
        var u = o.tag;
        if ((u === 3 || u === 4) && (u = o.stateNode.containerInfo, u === l || u.nodeType === 8 && u.parentNode === l)) return;
        o = o.return;
      }
      for (; s !== null; ) {
        if (o = en(s), o === null) return;
        if (u = o.tag, u === 5 || u === 6) {
          r = i = o;
          continue e;
        }
        s = s.parentNode;
      }
    }
    r = r.return;
  }
  gu(function() {
    var d = i, y = Hi(n), c = [];
    e: {
      var m = Gu.get(e);
      if (m !== void 0) {
        var v = bi, g = e;
        switch (e) {
          case "keypress":
            if (fl(n) === 0) break e;
          case "keydown":
          case "keyup":
            v = xf;
            break;
          case "focusin":
            g = "focus", v = Na;
            break;
          case "focusout":
            g = "blur", v = Na;
            break;
          case "beforeblur":
          case "afterblur":
            v = Na;
            break;
          case "click":
            if (n.button === 2) break e;
          case "auxclick":
          case "dblclick":
          case "mousedown":
          case "mousemove":
          case "mouseup":
          case "mouseout":
          case "mouseover":
          case "contextmenu":
            v = qo;
            break;
          case "drag":
          case "dragend":
          case "dragenter":
          case "dragexit":
          case "dragleave":
          case "dragover":
          case "dragstart":
          case "drop":
            v = af;
            break;
          case "touchcancel":
          case "touchend":
          case "touchmove":
          case "touchstart":
            v = Nf;
            break;
          case Bu:
          case Hu:
          case Wu:
            v = uf;
            break;
          case Qu:
            v = Sf;
            break;
          case "scroll":
            v = rf;
            break;
          case "wheel":
            v = Cf;
            break;
          case "copy":
          case "cut":
          case "paste":
            v = df;
            break;
          case "gotpointercapture":
          case "lostpointercapture":
          case "pointercancel":
          case "pointerdown":
          case "pointermove":
          case "pointerout":
          case "pointerover":
          case "pointerup":
            v = Yo;
        }
        var N = (t & 4) !== 0, F = !N && e === "scroll", p = N ? m !== null ? m + "Capture" : null : m;
        N = [];
        for (var f = d, h; f !== null; ) {
          h = f;
          var k = h.stateNode;
          if (h.tag === 5 && k !== null && (h = k, p !== null && (k = jr(f, p), k != null && N.push(Ir(f, k, h)))), F) break;
          f = f.return;
        }
        0 < N.length && (m = new v(m, g, null, n, y), c.push({ event: m, listeners: N }));
      }
    }
    if (!(t & 7)) {
      e: {
        if (m = e === "mouseover" || e === "pointerover", v = e === "mouseout" || e === "pointerout", m && n !== Za && (g = n.relatedTarget || n.fromElement) && (en(g) || g[St])) break e;
        if ((v || m) && (m = y.window === y ? y : (m = y.ownerDocument) ? m.defaultView || m.parentWindow : window, v ? (g = n.relatedTarget || n.toElement, v = d, g = g ? en(g) : null, g !== null && (F = pn(g), g !== F || g.tag !== 5 && g.tag !== 6) && (g = null)) : (v = null, g = d), v !== g)) {
          if (N = qo, k = "onMouseLeave", p = "onMouseEnter", f = "mouse", (e === "pointerout" || e === "pointerover") && (N = Yo, k = "onPointerLeave", p = "onPointerEnter", f = "pointer"), F = v == null ? m : Nn(v), h = g == null ? m : Nn(g), m = new N(k, f + "leave", v, n, y), m.target = F, m.relatedTarget = h, k = null, en(y) === d && (N = new N(p, f + "enter", g, n, y), N.target = h, N.relatedTarget = F, k = N), F = k, v && g) t: {
            for (N = v, p = g, f = 0, h = N; h; h = hn(h)) f++;
            for (h = 0, k = p; k; k = hn(k)) h++;
            for (; 0 < f - h; ) N = hn(N), f--;
            for (; 0 < h - f; ) p = hn(p), h--;
            for (; f--; ) {
              if (N === p || p !== null && N === p.alternate) break t;
              N = hn(N), p = hn(p);
            }
            N = null;
          }
          else N = null;
          v !== null && os(c, m, v, N, !1), g !== null && F !== null && os(c, F, g, N, !0);
        }
      }
      e: {
        if (m = d ? Nn(d) : window, v = m.nodeName && m.nodeName.toLowerCase(), v === "select" || v === "input" && m.type === "file") var z = zf;
        else if (Jo(m)) if (Ou) z = Mf;
        else {
          z = Df;
          var R = Tf;
        }
        else (v = m.nodeName) && v.toLowerCase() === "input" && (m.type === "checkbox" || m.type === "radio") && (z = Lf);
        if (z && (z = z(e, d))) {
          Mu(c, z, n, y);
          break e;
        }
        R && R(e, m, d), e === "focusout" && (R = m._wrapperState) && R.controlled && m.type === "number" && Ka(m, "number", m.value);
      }
      switch (R = d ? Nn(d) : window, e) {
        case "focusin":
          (Jo(R) || R.contentEditable === "true") && (yn = R, ai = d, fr = null);
          break;
        case "focusout":
          fr = ai = yn = null;
          break;
        case "mousedown":
          ii = !0;
          break;
        case "contextmenu":
        case "mouseup":
        case "dragend":
          ii = !1, ls(c, n, y);
          break;
        case "selectionchange":
          if (Af) break;
        case "keydown":
        case "keyup":
          ls(c, n, y);
      }
      var T;
      if (Xi) e: {
        switch (e) {
          case "compositionstart":
            var D = "onCompositionStart";
            break e;
          case "compositionend":
            D = "onCompositionEnd";
            break e;
          case "compositionupdate":
            D = "onCompositionUpdate";
            break e;
        }
        D = void 0;
      }
      else xn ? Du(e, n) && (D = "onCompositionEnd") : e === "keydown" && n.keyCode === 229 && (D = "onCompositionStart");
      D && (Tu && n.locale !== "ko" && (xn || D !== "onCompositionStart" ? D === "onCompositionEnd" && xn && (T = zu()) : (Lt = y, qi = "value" in Lt ? Lt.value : Lt.textContent, xn = !0)), R = Pl(d, D), 0 < R.length && (D = new bo(D, e, null, n, y), c.push({ event: D, listeners: R }), T ? D.data = T : (T = Lu(n), T !== null && (D.data = T)))), (T = If ? Pf(e, n) : _f(e, n)) && (d = Pl(d, "onBeforeInput"), 0 < d.length && (y = new bo("onBeforeInput", "beforeinput", null, n, y), c.push({ event: y, listeners: d }), y.data = T));
    }
    Ku(c, t);
  });
}
function Ir(e, t, n) {
  return { instance: e, listener: t, currentTarget: n };
}
function Pl(e, t) {
  for (var n = t + "Capture", r = []; e !== null; ) {
    var l = e, i = l.stateNode;
    l.tag === 5 && i !== null && (l = i, i = jr(e, n), i != null && r.unshift(Ir(e, i, l)), i = jr(e, t), i != null && r.push(Ir(e, i, l))), e = e.return;
  }
  return r;
}
function hn(e) {
  if (e === null) return null;
  do
    e = e.return;
  while (e && e.tag !== 5);
  return e || null;
}
function os(e, t, n, r, l) {
  for (var i = t._reactName, o = []; n !== null && n !== r; ) {
    var s = n, u = s.alternate, d = s.stateNode;
    if (u !== null && u === r) break;
    s.tag === 5 && d !== null && (s = d, l ? (u = jr(n, i), u != null && o.unshift(Ir(n, u, s))) : l || (u = jr(n, i), u != null && o.push(Ir(n, u, s)))), n = n.return;
  }
  o.length !== 0 && e.push({ event: t, listeners: o });
}
var Hf = /\r\n?/g, Wf = /\u0000|\uFFFD/g;
function ss(e) {
  return (typeof e == "string" ? e : "" + e).replace(Hf, `
`).replace(Wf, "");
}
function tl(e, t, n) {
  if (t = ss(t), ss(e) !== t && n) throw Error(P(425));
}
function _l() {
}
var oi = null, si = null;
function ui(e, t) {
  return e === "textarea" || e === "noscript" || typeof t.children == "string" || typeof t.children == "number" || typeof t.dangerouslySetInnerHTML == "object" && t.dangerouslySetInnerHTML !== null && t.dangerouslySetInnerHTML.__html != null;
}
var ci = typeof setTimeout == "function" ? setTimeout : void 0, Qf = typeof clearTimeout == "function" ? clearTimeout : void 0, us = typeof Promise == "function" ? Promise : void 0, Gf = typeof queueMicrotask == "function" ? queueMicrotask : typeof us < "u" ? function(e) {
  return us.resolve(null).then(e).catch(Kf);
} : ci;
function Kf(e) {
  setTimeout(function() {
    throw e;
  });
}
function _a(e, t) {
  var n = t, r = 0;
  do {
    var l = n.nextSibling;
    if (e.removeChild(n), l && l.nodeType === 8) if (n = l.data, n === "/$") {
      if (r === 0) {
        e.removeChild(l), Sr(t);
        return;
      }
      r--;
    } else n !== "$" && n !== "$?" && n !== "$!" || r++;
    n = l;
  } while (n);
  Sr(t);
}
function Ut(e) {
  for (; e != null; e = e.nextSibling) {
    var t = e.nodeType;
    if (t === 1 || t === 3) break;
    if (t === 8) {
      if (t = e.data, t === "$" || t === "$!" || t === "$?") break;
      if (t === "/$") return null;
    }
  }
  return e;
}
function cs(e) {
  e = e.previousSibling;
  for (var t = 0; e; ) {
    if (e.nodeType === 8) {
      var n = e.data;
      if (n === "$" || n === "$!" || n === "$?") {
        if (t === 0) return e;
        t--;
      } else n === "/$" && t++;
    }
    e = e.previousSibling;
  }
  return null;
}
var Hn = Math.random().toString(36).slice(2), ft = "__reactFiber$" + Hn, Pr = "__reactProps$" + Hn, St = "__reactContainer$" + Hn, di = "__reactEvents$" + Hn, qf = "__reactListeners$" + Hn, bf = "__reactHandles$" + Hn;
function en(e) {
  var t = e[ft];
  if (t) return t;
  for (var n = e.parentNode; n; ) {
    if (t = n[St] || n[ft]) {
      if (n = t.alternate, t.child !== null || n !== null && n.child !== null) for (e = cs(e); e !== null; ) {
        if (n = e[ft]) return n;
        e = cs(e);
      }
      return t;
    }
    e = n, n = e.parentNode;
  }
  return null;
}
function Ar(e) {
  return e = e[ft] || e[St], !e || e.tag !== 5 && e.tag !== 6 && e.tag !== 13 && e.tag !== 3 ? null : e;
}
function Nn(e) {
  if (e.tag === 5 || e.tag === 6) return e.stateNode;
  throw Error(P(33));
}
function Yl(e) {
  return e[Pr] || null;
}
var fi = [], wn = -1;
function qt(e) {
  return { current: e };
}
function Z(e) {
  0 > wn || (e.current = fi[wn], fi[wn] = null, wn--);
}
function Y(e, t) {
  wn++, fi[wn] = e.current, e.current = t;
}
var Gt = {}, Fe = qt(Gt), $e = qt(!1), an = Gt;
function Ln(e, t) {
  var n = e.type.contextTypes;
  if (!n) return Gt;
  var r = e.stateNode;
  if (r && r.__reactInternalMemoizedUnmaskedChildContext === t) return r.__reactInternalMemoizedMaskedChildContext;
  var l = {}, i;
  for (i in n) l[i] = t[i];
  return r && (e = e.stateNode, e.__reactInternalMemoizedUnmaskedChildContext = t, e.__reactInternalMemoizedMaskedChildContext = l), l;
}
function Ae(e) {
  return e = e.childContextTypes, e != null;
}
function Fl() {
  Z($e), Z(Fe);
}
function ds(e, t, n) {
  if (Fe.current !== Gt) throw Error(P(168));
  Y(Fe, t), Y($e, n);
}
function bu(e, t, n) {
  var r = e.stateNode;
  if (t = t.childContextTypes, typeof r.getChildContext != "function") return n;
  r = r.getChildContext();
  for (var l in r) if (!(l in t)) throw Error(P(108, zd(e) || "Unknown", l));
  return le({}, n, r);
}
function Rl(e) {
  return e = (e = e.stateNode) && e.__reactInternalMemoizedMergedChildContext || Gt, an = Fe.current, Y(Fe, e), Y($e, $e.current), !0;
}
function fs(e, t, n) {
  var r = e.stateNode;
  if (!r) throw Error(P(169));
  n ? (e = bu(e, t, an), r.__reactInternalMemoizedMergedChildContext = e, Z($e), Z(Fe), Y(Fe, e)) : Z($e), Y($e, n);
}
var xt = null, Xl = !1, Fa = !1;
function Yu(e) {
  xt === null ? xt = [e] : xt.push(e);
}
function Yf(e) {
  Xl = !0, Yu(e);
}
function bt() {
  if (!Fa && xt !== null) {
    Fa = !0;
    var e = 0, t = q;
    try {
      var n = xt;
      for (q = 1; e < n.length; e++) {
        var r = n[e];
        do
          r = r(!0);
        while (r !== null);
      }
      xt = null, Xl = !1;
    } catch (l) {
      throw xt !== null && (xt = xt.slice(e + 1)), Nu(Wi, bt), l;
    } finally {
      q = t, Fa = !1;
    }
  }
  return null;
}
var Sn = [], kn = 0, zl = null, Tl = 0, Ye = [], Xe = 0, on = null, yt = 1, jt = "";
function Zt(e, t) {
  Sn[kn++] = Tl, Sn[kn++] = zl, zl = e, Tl = t;
}
function Xu(e, t, n) {
  Ye[Xe++] = yt, Ye[Xe++] = jt, Ye[Xe++] = on, on = e;
  var r = yt;
  e = jt;
  var l = 32 - ot(r) - 1;
  r &= ~(1 << l), n += 1;
  var i = 32 - ot(t) + l;
  if (30 < i) {
    var o = l - l % 5;
    i = (r & (1 << o) - 1).toString(32), r >>= o, l -= o, yt = 1 << 32 - ot(t) + l | n << l | r, jt = i + e;
  } else yt = 1 << i | n << l | r, jt = e;
}
function Ji(e) {
  e.return !== null && (Zt(e, 1), Xu(e, 1, 0));
}
function eo(e) {
  for (; e === zl; ) zl = Sn[--kn], Sn[kn] = null, Tl = Sn[--kn], Sn[kn] = null;
  for (; e === on; ) on = Ye[--Xe], Ye[Xe] = null, jt = Ye[--Xe], Ye[Xe] = null, yt = Ye[--Xe], Ye[Xe] = null;
}
var Qe = null, We = null, ee = !1, at = null;
function Zu(e, t) {
  var n = Ze(5, null, null, 0);
  n.elementType = "DELETED", n.stateNode = t, n.return = e, t = e.deletions, t === null ? (e.deletions = [n], e.flags |= 16) : t.push(n);
}
function ps(e, t) {
  switch (e.tag) {
    case 5:
      var n = e.type;
      return t = t.nodeType !== 1 || n.toLowerCase() !== t.nodeName.toLowerCase() ? null : t, t !== null ? (e.stateNode = t, Qe = e, We = Ut(t.firstChild), !0) : !1;
    case 6:
      return t = e.pendingProps === "" || t.nodeType !== 3 ? null : t, t !== null ? (e.stateNode = t, Qe = e, We = null, !0) : !1;
    case 13:
      return t = t.nodeType !== 8 ? null : t, t !== null ? (n = on !== null ? { id: yt, overflow: jt } : null, e.memoizedState = { dehydrated: t, treeContext: n, retryLane: 1073741824 }, n = Ze(18, null, null, 0), n.stateNode = t, n.return = e, e.child = n, Qe = e, We = null, !0) : !1;
    default:
      return !1;
  }
}
function pi(e) {
  return (e.mode & 1) !== 0 && (e.flags & 128) === 0;
}
function mi(e) {
  if (ee) {
    var t = We;
    if (t) {
      var n = t;
      if (!ps(e, t)) {
        if (pi(e)) throw Error(P(418));
        t = Ut(n.nextSibling);
        var r = Qe;
        t && ps(e, t) ? Zu(r, n) : (e.flags = e.flags & -4097 | 2, ee = !1, Qe = e);
      }
    } else {
      if (pi(e)) throw Error(P(418));
      e.flags = e.flags & -4097 | 2, ee = !1, Qe = e;
    }
  }
}
function ms(e) {
  for (e = e.return; e !== null && e.tag !== 5 && e.tag !== 3 && e.tag !== 13; ) e = e.return;
  Qe = e;
}
function nl(e) {
  if (e !== Qe) return !1;
  if (!ee) return ms(e), ee = !0, !1;
  var t;
  if ((t = e.tag !== 3) && !(t = e.tag !== 5) && (t = e.type, t = t !== "head" && t !== "body" && !ui(e.type, e.memoizedProps)), t && (t = We)) {
    if (pi(e)) throw Ju(), Error(P(418));
    for (; t; ) Zu(e, t), t = Ut(t.nextSibling);
  }
  if (ms(e), e.tag === 13) {
    if (e = e.memoizedState, e = e !== null ? e.dehydrated : null, !e) throw Error(P(317));
    e: {
      for (e = e.nextSibling, t = 0; e; ) {
        if (e.nodeType === 8) {
          var n = e.data;
          if (n === "/$") {
            if (t === 0) {
              We = Ut(e.nextSibling);
              break e;
            }
            t--;
          } else n !== "$" && n !== "$!" && n !== "$?" || t++;
        }
        e = e.nextSibling;
      }
      We = null;
    }
  } else We = Qe ? Ut(e.stateNode.nextSibling) : null;
  return !0;
}
function Ju() {
  for (var e = We; e; ) e = Ut(e.nextSibling);
}
function Mn() {
  We = Qe = null, ee = !1;
}
function to(e) {
  at === null ? at = [e] : at.push(e);
}
var Xf = Et.ReactCurrentBatchConfig;
function tr(e, t, n) {
  if (e = n.ref, e !== null && typeof e != "function" && typeof e != "object") {
    if (n._owner) {
      if (n = n._owner, n) {
        if (n.tag !== 1) throw Error(P(309));
        var r = n.stateNode;
      }
      if (!r) throw Error(P(147, e));
      var l = r, i = "" + e;
      return t !== null && t.ref !== null && typeof t.ref == "function" && t.ref._stringRef === i ? t.ref : (t = function(o) {
        var s = l.refs;
        o === null ? delete s[i] : s[i] = o;
      }, t._stringRef = i, t);
    }
    if (typeof e != "string") throw Error(P(284));
    if (!n._owner) throw Error(P(290, e));
  }
  return e;
}
function rl(e, t) {
  throw e = Object.prototype.toString.call(t), Error(P(31, e === "[object Object]" ? "object with keys {" + Object.keys(t).join(", ") + "}" : e));
}
function hs(e) {
  var t = e._init;
  return t(e._payload);
}
function ec(e) {
  function t(p, f) {
    if (e) {
      var h = p.deletions;
      h === null ? (p.deletions = [f], p.flags |= 16) : h.push(f);
    }
  }
  function n(p, f) {
    if (!e) return null;
    for (; f !== null; ) t(p, f), f = f.sibling;
    return null;
  }
  function r(p, f) {
    for (p = /* @__PURE__ */ new Map(); f !== null; ) f.key !== null ? p.set(f.key, f) : p.set(f.index, f), f = f.sibling;
    return p;
  }
  function l(p, f) {
    return p = Wt(p, f), p.index = 0, p.sibling = null, p;
  }
  function i(p, f, h) {
    return p.index = h, e ? (h = p.alternate, h !== null ? (h = h.index, h < f ? (p.flags |= 2, f) : h) : (p.flags |= 2, f)) : (p.flags |= 1048576, f);
  }
  function o(p) {
    return e && p.alternate === null && (p.flags |= 2), p;
  }
  function s(p, f, h, k) {
    return f === null || f.tag !== 6 ? (f = Oa(h, p.mode, k), f.return = p, f) : (f = l(f, h), f.return = p, f);
  }
  function u(p, f, h, k) {
    var z = h.type;
    return z === gn ? y(p, f, h.props.children, k, h.key) : f !== null && (f.elementType === z || typeof z == "object" && z !== null && z.$$typeof === Rt && hs(z) === f.type) ? (k = l(f, h.props), k.ref = tr(p, f, h), k.return = p, k) : (k = yl(h.type, h.key, h.props, null, p.mode, k), k.ref = tr(p, f, h), k.return = p, k);
  }
  function d(p, f, h, k) {
    return f === null || f.tag !== 4 || f.stateNode.containerInfo !== h.containerInfo || f.stateNode.implementation !== h.implementation ? (f = $a(h, p.mode, k), f.return = p, f) : (f = l(f, h.children || []), f.return = p, f);
  }
  function y(p, f, h, k, z) {
    return f === null || f.tag !== 7 ? (f = ln(h, p.mode, k, z), f.return = p, f) : (f = l(f, h), f.return = p, f);
  }
  function c(p, f, h) {
    if (typeof f == "string" && f !== "" || typeof f == "number") return f = Oa("" + f, p.mode, h), f.return = p, f;
    if (typeof f == "object" && f !== null) {
      switch (f.$$typeof) {
        case Gr:
          return h = yl(f.type, f.key, f.props, null, p.mode, h), h.ref = tr(p, null, f), h.return = p, h;
        case vn:
          return f = $a(f, p.mode, h), f.return = p, f;
        case Rt:
          var k = f._init;
          return c(p, k(f._payload), h);
      }
      if (ar(f) || Yn(f)) return f = ln(f, p.mode, h, null), f.return = p, f;
      rl(p, f);
    }
    return null;
  }
  function m(p, f, h, k) {
    var z = f !== null ? f.key : null;
    if (typeof h == "string" && h !== "" || typeof h == "number") return z !== null ? null : s(p, f, "" + h, k);
    if (typeof h == "object" && h !== null) {
      switch (h.$$typeof) {
        case Gr:
          return h.key === z ? u(p, f, h, k) : null;
        case vn:
          return h.key === z ? d(p, f, h, k) : null;
        case Rt:
          return z = h._init, m(
            p,
            f,
            z(h._payload),
            k
          );
      }
      if (ar(h) || Yn(h)) return z !== null ? null : y(p, f, h, k, null);
      rl(p, h);
    }
    return null;
  }
  function v(p, f, h, k, z) {
    if (typeof k == "string" && k !== "" || typeof k == "number") return p = p.get(h) || null, s(f, p, "" + k, z);
    if (typeof k == "object" && k !== null) {
      switch (k.$$typeof) {
        case Gr:
          return p = p.get(k.key === null ? h : k.key) || null, u(f, p, k, z);
        case vn:
          return p = p.get(k.key === null ? h : k.key) || null, d(f, p, k, z);
        case Rt:
          var R = k._init;
          return v(p, f, h, R(k._payload), z);
      }
      if (ar(k) || Yn(k)) return p = p.get(h) || null, y(f, p, k, z, null);
      rl(f, k);
    }
    return null;
  }
  function g(p, f, h, k) {
    for (var z = null, R = null, T = f, D = f = 0, L = null; T !== null && D < h.length; D++) {
      T.index > D ? (L = T, T = null) : L = T.sibling;
      var E = m(p, T, h[D], k);
      if (E === null) {
        T === null && (T = L);
        break;
      }
      e && T && E.alternate === null && t(p, T), f = i(E, f, D), R === null ? z = E : R.sibling = E, R = E, T = L;
    }
    if (D === h.length) return n(p, T), ee && Zt(p, D), z;
    if (T === null) {
      for (; D < h.length; D++) T = c(p, h[D], k), T !== null && (f = i(T, f, D), R === null ? z = T : R.sibling = T, R = T);
      return ee && Zt(p, D), z;
    }
    for (T = r(p, T); D < h.length; D++) L = v(T, p, D, h[D], k), L !== null && (e && L.alternate !== null && T.delete(L.key === null ? D : L.key), f = i(L, f, D), R === null ? z = L : R.sibling = L, R = L);
    return e && T.forEach(function(_) {
      return t(p, _);
    }), ee && Zt(p, D), z;
  }
  function N(p, f, h, k) {
    var z = Yn(h);
    if (typeof z != "function") throw Error(P(150));
    if (h = z.call(h), h == null) throw Error(P(151));
    for (var R = z = null, T = f, D = f = 0, L = null, E = h.next(); T !== null && !E.done; D++, E = h.next()) {
      T.index > D ? (L = T, T = null) : L = T.sibling;
      var _ = m(p, T, E.value, k);
      if (_ === null) {
        T === null && (T = L);
        break;
      }
      e && T && _.alternate === null && t(p, T), f = i(_, f, D), R === null ? z = _ : R.sibling = _, R = _, T = L;
    }
    if (E.done) return n(
      p,
      T
    ), ee && Zt(p, D), z;
    if (T === null) {
      for (; !E.done; D++, E = h.next()) E = c(p, E.value, k), E !== null && (f = i(E, f, D), R === null ? z = E : R.sibling = E, R = E);
      return ee && Zt(p, D), z;
    }
    for (T = r(p, T); !E.done; D++, E = h.next()) E = v(T, p, D, E.value, k), E !== null && (e && E.alternate !== null && T.delete(E.key === null ? D : E.key), f = i(E, f, D), R === null ? z = E : R.sibling = E, R = E);
    return e && T.forEach(function($) {
      return t(p, $);
    }), ee && Zt(p, D), z;
  }
  function F(p, f, h, k) {
    if (typeof h == "object" && h !== null && h.type === gn && h.key === null && (h = h.props.children), typeof h == "object" && h !== null) {
      switch (h.$$typeof) {
        case Gr:
          e: {
            for (var z = h.key, R = f; R !== null; ) {
              if (R.key === z) {
                if (z = h.type, z === gn) {
                  if (R.tag === 7) {
                    n(p, R.sibling), f = l(R, h.props.children), f.return = p, p = f;
                    break e;
                  }
                } else if (R.elementType === z || typeof z == "object" && z !== null && z.$$typeof === Rt && hs(z) === R.type) {
                  n(p, R.sibling), f = l(R, h.props), f.ref = tr(p, R, h), f.return = p, p = f;
                  break e;
                }
                n(p, R);
                break;
              } else t(p, R);
              R = R.sibling;
            }
            h.type === gn ? (f = ln(h.props.children, p.mode, k, h.key), f.return = p, p = f) : (k = yl(h.type, h.key, h.props, null, p.mode, k), k.ref = tr(p, f, h), k.return = p, p = k);
          }
          return o(p);
        case vn:
          e: {
            for (R = h.key; f !== null; ) {
              if (f.key === R) if (f.tag === 4 && f.stateNode.containerInfo === h.containerInfo && f.stateNode.implementation === h.implementation) {
                n(p, f.sibling), f = l(f, h.children || []), f.return = p, p = f;
                break e;
              } else {
                n(p, f);
                break;
              }
              else t(p, f);
              f = f.sibling;
            }
            f = $a(h, p.mode, k), f.return = p, p = f;
          }
          return o(p);
        case Rt:
          return R = h._init, F(p, f, R(h._payload), k);
      }
      if (ar(h)) return g(p, f, h, k);
      if (Yn(h)) return N(p, f, h, k);
      rl(p, h);
    }
    return typeof h == "string" && h !== "" || typeof h == "number" ? (h = "" + h, f !== null && f.tag === 6 ? (n(p, f.sibling), f = l(f, h), f.return = p, p = f) : (n(p, f), f = Oa(h, p.mode, k), f.return = p, p = f), o(p)) : n(p, f);
  }
  return F;
}
var On = ec(!0), tc = ec(!1), Dl = qt(null), Ll = null, Cn = null, no = null;
function ro() {
  no = Cn = Ll = null;
}
function lo(e) {
  var t = Dl.current;
  Z(Dl), e._currentValue = t;
}
function hi(e, t, n) {
  for (; e !== null; ) {
    var r = e.alternate;
    if ((e.childLanes & t) !== t ? (e.childLanes |= t, r !== null && (r.childLanes |= t)) : r !== null && (r.childLanes & t) !== t && (r.childLanes |= t), e === n) break;
    e = e.return;
  }
}
function zn(e, t) {
  Ll = e, no = Cn = null, e = e.dependencies, e !== null && e.firstContext !== null && (e.lanes & t && (Oe = !0), e.firstContext = null);
}
function et(e) {
  var t = e._currentValue;
  if (no !== e) if (e = { context: e, memoizedValue: t, next: null }, Cn === null) {
    if (Ll === null) throw Error(P(308));
    Cn = e, Ll.dependencies = { lanes: 0, firstContext: e };
  } else Cn = Cn.next = e;
  return t;
}
var tn = null;
function ao(e) {
  tn === null ? tn = [e] : tn.push(e);
}
function nc(e, t, n, r) {
  var l = t.interleaved;
  return l === null ? (n.next = n, ao(t)) : (n.next = l.next, l.next = n), t.interleaved = n, kt(e, r);
}
function kt(e, t) {
  e.lanes |= t;
  var n = e.alternate;
  for (n !== null && (n.lanes |= t), n = e, e = e.return; e !== null; ) e.childLanes |= t, n = e.alternate, n !== null && (n.childLanes |= t), n = e, e = e.return;
  return n.tag === 3 ? n.stateNode : null;
}
var zt = !1;
function io(e) {
  e.updateQueue = { baseState: e.memoizedState, firstBaseUpdate: null, lastBaseUpdate: null, shared: { pending: null, interleaved: null, lanes: 0 }, effects: null };
}
function rc(e, t) {
  e = e.updateQueue, t.updateQueue === e && (t.updateQueue = { baseState: e.baseState, firstBaseUpdate: e.firstBaseUpdate, lastBaseUpdate: e.lastBaseUpdate, shared: e.shared, effects: e.effects });
}
function Nt(e, t) {
  return { eventTime: e, lane: t, tag: 0, payload: null, callback: null, next: null };
}
function Vt(e, t, n) {
  var r = e.updateQueue;
  if (r === null) return null;
  if (r = r.shared, Q & 2) {
    var l = r.pending;
    return l === null ? t.next = t : (t.next = l.next, l.next = t), r.pending = t, kt(e, n);
  }
  return l = r.interleaved, l === null ? (t.next = t, ao(r)) : (t.next = l.next, l.next = t), r.interleaved = t, kt(e, n);
}
function pl(e, t, n) {
  if (t = t.updateQueue, t !== null && (t = t.shared, (n & 4194240) !== 0)) {
    var r = t.lanes;
    r &= e.pendingLanes, n |= r, t.lanes = n, Qi(e, n);
  }
}
function vs(e, t) {
  var n = e.updateQueue, r = e.alternate;
  if (r !== null && (r = r.updateQueue, n === r)) {
    var l = null, i = null;
    if (n = n.firstBaseUpdate, n !== null) {
      do {
        var o = { eventTime: n.eventTime, lane: n.lane, tag: n.tag, payload: n.payload, callback: n.callback, next: null };
        i === null ? l = i = o : i = i.next = o, n = n.next;
      } while (n !== null);
      i === null ? l = i = t : i = i.next = t;
    } else l = i = t;
    n = { baseState: r.baseState, firstBaseUpdate: l, lastBaseUpdate: i, shared: r.shared, effects: r.effects }, e.updateQueue = n;
    return;
  }
  e = n.lastBaseUpdate, e === null ? n.firstBaseUpdate = t : e.next = t, n.lastBaseUpdate = t;
}
function Ml(e, t, n, r) {
  var l = e.updateQueue;
  zt = !1;
  var i = l.firstBaseUpdate, o = l.lastBaseUpdate, s = l.shared.pending;
  if (s !== null) {
    l.shared.pending = null;
    var u = s, d = u.next;
    u.next = null, o === null ? i = d : o.next = d, o = u;
    var y = e.alternate;
    y !== null && (y = y.updateQueue, s = y.lastBaseUpdate, s !== o && (s === null ? y.firstBaseUpdate = d : s.next = d, y.lastBaseUpdate = u));
  }
  if (i !== null) {
    var c = l.baseState;
    o = 0, y = d = u = null, s = i;
    do {
      var m = s.lane, v = s.eventTime;
      if ((r & m) === m) {
        y !== null && (y = y.next = {
          eventTime: v,
          lane: 0,
          tag: s.tag,
          payload: s.payload,
          callback: s.callback,
          next: null
        });
        e: {
          var g = e, N = s;
          switch (m = t, v = n, N.tag) {
            case 1:
              if (g = N.payload, typeof g == "function") {
                c = g.call(v, c, m);
                break e;
              }
              c = g;
              break e;
            case 3:
              g.flags = g.flags & -65537 | 128;
            case 0:
              if (g = N.payload, m = typeof g == "function" ? g.call(v, c, m) : g, m == null) break e;
              c = le({}, c, m);
              break e;
            case 2:
              zt = !0;
          }
        }
        s.callback !== null && s.lane !== 0 && (e.flags |= 64, m = l.effects, m === null ? l.effects = [s] : m.push(s));
      } else v = { eventTime: v, lane: m, tag: s.tag, payload: s.payload, callback: s.callback, next: null }, y === null ? (d = y = v, u = c) : y = y.next = v, o |= m;
      if (s = s.next, s === null) {
        if (s = l.shared.pending, s === null) break;
        m = s, s = m.next, m.next = null, l.lastBaseUpdate = m, l.shared.pending = null;
      }
    } while (!0);
    if (y === null && (u = c), l.baseState = u, l.firstBaseUpdate = d, l.lastBaseUpdate = y, t = l.shared.interleaved, t !== null) {
      l = t;
      do
        o |= l.lane, l = l.next;
      while (l !== t);
    } else i === null && (l.shared.lanes = 0);
    un |= o, e.lanes = o, e.memoizedState = c;
  }
}
function gs(e, t, n) {
  if (e = t.effects, t.effects = null, e !== null) for (t = 0; t < e.length; t++) {
    var r = e[t], l = r.callback;
    if (l !== null) {
      if (r.callback = null, r = n, typeof l != "function") throw Error(P(191, l));
      l.call(r);
    }
  }
}
var Ur = {}, mt = qt(Ur), _r = qt(Ur), Fr = qt(Ur);
function nn(e) {
  if (e === Ur) throw Error(P(174));
  return e;
}
function oo(e, t) {
  switch (Y(Fr, t), Y(_r, e), Y(mt, Ur), e = t.nodeType, e) {
    case 9:
    case 11:
      t = (t = t.documentElement) ? t.namespaceURI : ba(null, "");
      break;
    default:
      e = e === 8 ? t.parentNode : t, t = e.namespaceURI || null, e = e.tagName, t = ba(t, e);
  }
  Z(mt), Y(mt, t);
}
function $n() {
  Z(mt), Z(_r), Z(Fr);
}
function lc(e) {
  nn(Fr.current);
  var t = nn(mt.current), n = ba(t, e.type);
  t !== n && (Y(_r, e), Y(mt, n));
}
function so(e) {
  _r.current === e && (Z(mt), Z(_r));
}
var ne = qt(0);
function Ol(e) {
  for (var t = e; t !== null; ) {
    if (t.tag === 13) {
      var n = t.memoizedState;
      if (n !== null && (n = n.dehydrated, n === null || n.data === "$?" || n.data === "$!")) return t;
    } else if (t.tag === 19 && t.memoizedProps.revealOrder !== void 0) {
      if (t.flags & 128) return t;
    } else if (t.child !== null) {
      t.child.return = t, t = t.child;
      continue;
    }
    if (t === e) break;
    for (; t.sibling === null; ) {
      if (t.return === null || t.return === e) return null;
      t = t.return;
    }
    t.sibling.return = t.return, t = t.sibling;
  }
  return null;
}
var Ra = [];
function uo() {
  for (var e = 0; e < Ra.length; e++) Ra[e]._workInProgressVersionPrimary = null;
  Ra.length = 0;
}
var ml = Et.ReactCurrentDispatcher, za = Et.ReactCurrentBatchConfig, sn = 0, re = null, he = null, xe = null, $l = !1, pr = !1, Rr = 0, Zf = 0;
function Ee() {
  throw Error(P(321));
}
function co(e, t) {
  if (t === null) return !1;
  for (var n = 0; n < t.length && n < e.length; n++) if (!ut(e[n], t[n])) return !1;
  return !0;
}
function fo(e, t, n, r, l, i) {
  if (sn = i, re = t, t.memoizedState = null, t.updateQueue = null, t.lanes = 0, ml.current = e === null || e.memoizedState === null ? np : rp, e = n(r, l), pr) {
    i = 0;
    do {
      if (pr = !1, Rr = 0, 25 <= i) throw Error(P(301));
      i += 1, xe = he = null, t.updateQueue = null, ml.current = lp, e = n(r, l);
    } while (pr);
  }
  if (ml.current = Al, t = he !== null && he.next !== null, sn = 0, xe = he = re = null, $l = !1, t) throw Error(P(300));
  return e;
}
function po() {
  var e = Rr !== 0;
  return Rr = 0, e;
}
function dt() {
  var e = { memoizedState: null, baseState: null, baseQueue: null, queue: null, next: null };
  return xe === null ? re.memoizedState = xe = e : xe = xe.next = e, xe;
}
function tt() {
  if (he === null) {
    var e = re.alternate;
    e = e !== null ? e.memoizedState : null;
  } else e = he.next;
  var t = xe === null ? re.memoizedState : xe.next;
  if (t !== null) xe = t, he = e;
  else {
    if (e === null) throw Error(P(310));
    he = e, e = { memoizedState: he.memoizedState, baseState: he.baseState, baseQueue: he.baseQueue, queue: he.queue, next: null }, xe === null ? re.memoizedState = xe = e : xe = xe.next = e;
  }
  return xe;
}
function zr(e, t) {
  return typeof t == "function" ? t(e) : t;
}
function Ta(e) {
  var t = tt(), n = t.queue;
  if (n === null) throw Error(P(311));
  n.lastRenderedReducer = e;
  var r = he, l = r.baseQueue, i = n.pending;
  if (i !== null) {
    if (l !== null) {
      var o = l.next;
      l.next = i.next, i.next = o;
    }
    r.baseQueue = l = i, n.pending = null;
  }
  if (l !== null) {
    i = l.next, r = r.baseState;
    var s = o = null, u = null, d = i;
    do {
      var y = d.lane;
      if ((sn & y) === y) u !== null && (u = u.next = { lane: 0, action: d.action, hasEagerState: d.hasEagerState, eagerState: d.eagerState, next: null }), r = d.hasEagerState ? d.eagerState : e(r, d.action);
      else {
        var c = {
          lane: y,
          action: d.action,
          hasEagerState: d.hasEagerState,
          eagerState: d.eagerState,
          next: null
        };
        u === null ? (s = u = c, o = r) : u = u.next = c, re.lanes |= y, un |= y;
      }
      d = d.next;
    } while (d !== null && d !== i);
    u === null ? o = r : u.next = s, ut(r, t.memoizedState) || (Oe = !0), t.memoizedState = r, t.baseState = o, t.baseQueue = u, n.lastRenderedState = r;
  }
  if (e = n.interleaved, e !== null) {
    l = e;
    do
      i = l.lane, re.lanes |= i, un |= i, l = l.next;
    while (l !== e);
  } else l === null && (n.lanes = 0);
  return [t.memoizedState, n.dispatch];
}
function Da(e) {
  var t = tt(), n = t.queue;
  if (n === null) throw Error(P(311));
  n.lastRenderedReducer = e;
  var r = n.dispatch, l = n.pending, i = t.memoizedState;
  if (l !== null) {
    n.pending = null;
    var o = l = l.next;
    do
      i = e(i, o.action), o = o.next;
    while (o !== l);
    ut(i, t.memoizedState) || (Oe = !0), t.memoizedState = i, t.baseQueue === null && (t.baseState = i), n.lastRenderedState = i;
  }
  return [i, r];
}
function ac() {
}
function ic(e, t) {
  var n = re, r = tt(), l = t(), i = !ut(r.memoizedState, l);
  if (i && (r.memoizedState = l, Oe = !0), r = r.queue, mo(uc.bind(null, n, r, e), [e]), r.getSnapshot !== t || i || xe !== null && xe.memoizedState.tag & 1) {
    if (n.flags |= 2048, Tr(9, sc.bind(null, n, r, l, t), void 0, null), je === null) throw Error(P(349));
    sn & 30 || oc(n, t, l);
  }
  return l;
}
function oc(e, t, n) {
  e.flags |= 16384, e = { getSnapshot: t, value: n }, t = re.updateQueue, t === null ? (t = { lastEffect: null, stores: null }, re.updateQueue = t, t.stores = [e]) : (n = t.stores, n === null ? t.stores = [e] : n.push(e));
}
function sc(e, t, n, r) {
  t.value = n, t.getSnapshot = r, cc(t) && dc(e);
}
function uc(e, t, n) {
  return n(function() {
    cc(t) && dc(e);
  });
}
function cc(e) {
  var t = e.getSnapshot;
  e = e.value;
  try {
    var n = t();
    return !ut(e, n);
  } catch {
    return !0;
  }
}
function dc(e) {
  var t = kt(e, 1);
  t !== null && st(t, e, 1, -1);
}
function xs(e) {
  var t = dt();
  return typeof e == "function" && (e = e()), t.memoizedState = t.baseState = e, e = { pending: null, interleaved: null, lanes: 0, dispatch: null, lastRenderedReducer: zr, lastRenderedState: e }, t.queue = e, e = e.dispatch = tp.bind(null, re, e), [t.memoizedState, e];
}
function Tr(e, t, n, r) {
  return e = { tag: e, create: t, destroy: n, deps: r, next: null }, t = re.updateQueue, t === null ? (t = { lastEffect: null, stores: null }, re.updateQueue = t, t.lastEffect = e.next = e) : (n = t.lastEffect, n === null ? t.lastEffect = e.next = e : (r = n.next, n.next = e, e.next = r, t.lastEffect = e)), e;
}
function fc() {
  return tt().memoizedState;
}
function hl(e, t, n, r) {
  var l = dt();
  re.flags |= e, l.memoizedState = Tr(1 | t, n, void 0, r === void 0 ? null : r);
}
function Zl(e, t, n, r) {
  var l = tt();
  r = r === void 0 ? null : r;
  var i = void 0;
  if (he !== null) {
    var o = he.memoizedState;
    if (i = o.destroy, r !== null && co(r, o.deps)) {
      l.memoizedState = Tr(t, n, i, r);
      return;
    }
  }
  re.flags |= e, l.memoizedState = Tr(1 | t, n, i, r);
}
function ys(e, t) {
  return hl(8390656, 8, e, t);
}
function mo(e, t) {
  return Zl(2048, 8, e, t);
}
function pc(e, t) {
  return Zl(4, 2, e, t);
}
function mc(e, t) {
  return Zl(4, 4, e, t);
}
function hc(e, t) {
  if (typeof t == "function") return e = e(), t(e), function() {
    t(null);
  };
  if (t != null) return e = e(), t.current = e, function() {
    t.current = null;
  };
}
function vc(e, t, n) {
  return n = n != null ? n.concat([e]) : null, Zl(4, 4, hc.bind(null, t, e), n);
}
function ho() {
}
function gc(e, t) {
  var n = tt();
  t = t === void 0 ? null : t;
  var r = n.memoizedState;
  return r !== null && t !== null && co(t, r[1]) ? r[0] : (n.memoizedState = [e, t], e);
}
function xc(e, t) {
  var n = tt();
  t = t === void 0 ? null : t;
  var r = n.memoizedState;
  return r !== null && t !== null && co(t, r[1]) ? r[0] : (e = e(), n.memoizedState = [e, t], e);
}
function yc(e, t, n) {
  return sn & 21 ? (ut(n, t) || (n = ku(), re.lanes |= n, un |= n, e.baseState = !0), t) : (e.baseState && (e.baseState = !1, Oe = !0), e.memoizedState = n);
}
function Jf(e, t) {
  var n = q;
  q = n !== 0 && 4 > n ? n : 4, e(!0);
  var r = za.transition;
  za.transition = {};
  try {
    e(!1), t();
  } finally {
    q = n, za.transition = r;
  }
}
function jc() {
  return tt().memoizedState;
}
function ep(e, t, n) {
  var r = Ht(e);
  if (n = { lane: r, action: n, hasEagerState: !1, eagerState: null, next: null }, Nc(e)) wc(t, n);
  else if (n = nc(e, t, n, r), n !== null) {
    var l = Te();
    st(n, e, r, l), Sc(n, t, r);
  }
}
function tp(e, t, n) {
  var r = Ht(e), l = { lane: r, action: n, hasEagerState: !1, eagerState: null, next: null };
  if (Nc(e)) wc(t, l);
  else {
    var i = e.alternate;
    if (e.lanes === 0 && (i === null || i.lanes === 0) && (i = t.lastRenderedReducer, i !== null)) try {
      var o = t.lastRenderedState, s = i(o, n);
      if (l.hasEagerState = !0, l.eagerState = s, ut(s, o)) {
        var u = t.interleaved;
        u === null ? (l.next = l, ao(t)) : (l.next = u.next, u.next = l), t.interleaved = l;
        return;
      }
    } catch {
    } finally {
    }
    n = nc(e, t, l, r), n !== null && (l = Te(), st(n, e, r, l), Sc(n, t, r));
  }
}
function Nc(e) {
  var t = e.alternate;
  return e === re || t !== null && t === re;
}
function wc(e, t) {
  pr = $l = !0;
  var n = e.pending;
  n === null ? t.next = t : (t.next = n.next, n.next = t), e.pending = t;
}
function Sc(e, t, n) {
  if (n & 4194240) {
    var r = t.lanes;
    r &= e.pendingLanes, n |= r, t.lanes = n, Qi(e, n);
  }
}
var Al = { readContext: et, useCallback: Ee, useContext: Ee, useEffect: Ee, useImperativeHandle: Ee, useInsertionEffect: Ee, useLayoutEffect: Ee, useMemo: Ee, useReducer: Ee, useRef: Ee, useState: Ee, useDebugValue: Ee, useDeferredValue: Ee, useTransition: Ee, useMutableSource: Ee, useSyncExternalStore: Ee, useId: Ee, unstable_isNewReconciler: !1 }, np = { readContext: et, useCallback: function(e, t) {
  return dt().memoizedState = [e, t === void 0 ? null : t], e;
}, useContext: et, useEffect: ys, useImperativeHandle: function(e, t, n) {
  return n = n != null ? n.concat([e]) : null, hl(
    4194308,
    4,
    hc.bind(null, t, e),
    n
  );
}, useLayoutEffect: function(e, t) {
  return hl(4194308, 4, e, t);
}, useInsertionEffect: function(e, t) {
  return hl(4, 2, e, t);
}, useMemo: function(e, t) {
  var n = dt();
  return t = t === void 0 ? null : t, e = e(), n.memoizedState = [e, t], e;
}, useReducer: function(e, t, n) {
  var r = dt();
  return t = n !== void 0 ? n(t) : t, r.memoizedState = r.baseState = t, e = { pending: null, interleaved: null, lanes: 0, dispatch: null, lastRenderedReducer: e, lastRenderedState: t }, r.queue = e, e = e.dispatch = ep.bind(null, re, e), [r.memoizedState, e];
}, useRef: function(e) {
  var t = dt();
  return e = { current: e }, t.memoizedState = e;
}, useState: xs, useDebugValue: ho, useDeferredValue: function(e) {
  return dt().memoizedState = e;
}, useTransition: function() {
  var e = xs(!1), t = e[0];
  return e = Jf.bind(null, e[1]), dt().memoizedState = e, [t, e];
}, useMutableSource: function() {
}, useSyncExternalStore: function(e, t, n) {
  var r = re, l = dt();
  if (ee) {
    if (n === void 0) throw Error(P(407));
    n = n();
  } else {
    if (n = t(), je === null) throw Error(P(349));
    sn & 30 || oc(r, t, n);
  }
  l.memoizedState = n;
  var i = { value: n, getSnapshot: t };
  return l.queue = i, ys(uc.bind(
    null,
    r,
    i,
    e
  ), [e]), r.flags |= 2048, Tr(9, sc.bind(null, r, i, n, t), void 0, null), n;
}, useId: function() {
  var e = dt(), t = je.identifierPrefix;
  if (ee) {
    var n = jt, r = yt;
    n = (r & ~(1 << 32 - ot(r) - 1)).toString(32) + n, t = ":" + t + "R" + n, n = Rr++, 0 < n && (t += "H" + n.toString(32)), t += ":";
  } else n = Zf++, t = ":" + t + "r" + n.toString(32) + ":";
  return e.memoizedState = t;
}, unstable_isNewReconciler: !1 }, rp = {
  readContext: et,
  useCallback: gc,
  useContext: et,
  useEffect: mo,
  useImperativeHandle: vc,
  useInsertionEffect: pc,
  useLayoutEffect: mc,
  useMemo: xc,
  useReducer: Ta,
  useRef: fc,
  useState: function() {
    return Ta(zr);
  },
  useDebugValue: ho,
  useDeferredValue: function(e) {
    var t = tt();
    return yc(t, he.memoizedState, e);
  },
  useTransition: function() {
    var e = Ta(zr)[0], t = tt().memoizedState;
    return [e, t];
  },
  useMutableSource: ac,
  useSyncExternalStore: ic,
  useId: jc,
  unstable_isNewReconciler: !1
}, lp = { readContext: et, useCallback: gc, useContext: et, useEffect: mo, useImperativeHandle: vc, useInsertionEffect: pc, useLayoutEffect: mc, useMemo: xc, useReducer: Da, useRef: fc, useState: function() {
  return Da(zr);
}, useDebugValue: ho, useDeferredValue: function(e) {
  var t = tt();
  return he === null ? t.memoizedState = e : yc(t, he.memoizedState, e);
}, useTransition: function() {
  var e = Da(zr)[0], t = tt().memoizedState;
  return [e, t];
}, useMutableSource: ac, useSyncExternalStore: ic, useId: jc, unstable_isNewReconciler: !1 };
function rt(e, t) {
  if (e && e.defaultProps) {
    t = le({}, t), e = e.defaultProps;
    for (var n in e) t[n] === void 0 && (t[n] = e[n]);
    return t;
  }
  return t;
}
function vi(e, t, n, r) {
  t = e.memoizedState, n = n(r, t), n = n == null ? t : le({}, t, n), e.memoizedState = n, e.lanes === 0 && (e.updateQueue.baseState = n);
}
var Jl = { isMounted: function(e) {
  return (e = e._reactInternals) ? pn(e) === e : !1;
}, enqueueSetState: function(e, t, n) {
  e = e._reactInternals;
  var r = Te(), l = Ht(e), i = Nt(r, l);
  i.payload = t, n != null && (i.callback = n), t = Vt(e, i, l), t !== null && (st(t, e, l, r), pl(t, e, l));
}, enqueueReplaceState: function(e, t, n) {
  e = e._reactInternals;
  var r = Te(), l = Ht(e), i = Nt(r, l);
  i.tag = 1, i.payload = t, n != null && (i.callback = n), t = Vt(e, i, l), t !== null && (st(t, e, l, r), pl(t, e, l));
}, enqueueForceUpdate: function(e, t) {
  e = e._reactInternals;
  var n = Te(), r = Ht(e), l = Nt(n, r);
  l.tag = 2, t != null && (l.callback = t), t = Vt(e, l, r), t !== null && (st(t, e, r, n), pl(t, e, r));
} };
function js(e, t, n, r, l, i, o) {
  return e = e.stateNode, typeof e.shouldComponentUpdate == "function" ? e.shouldComponentUpdate(r, i, o) : t.prototype && t.prototype.isPureReactComponent ? !Cr(n, r) || !Cr(l, i) : !0;
}
function kc(e, t, n) {
  var r = !1, l = Gt, i = t.contextType;
  return typeof i == "object" && i !== null ? i = et(i) : (l = Ae(t) ? an : Fe.current, r = t.contextTypes, i = (r = r != null) ? Ln(e, l) : Gt), t = new t(n, i), e.memoizedState = t.state !== null && t.state !== void 0 ? t.state : null, t.updater = Jl, e.stateNode = t, t._reactInternals = e, r && (e = e.stateNode, e.__reactInternalMemoizedUnmaskedChildContext = l, e.__reactInternalMemoizedMaskedChildContext = i), t;
}
function Ns(e, t, n, r) {
  e = t.state, typeof t.componentWillReceiveProps == "function" && t.componentWillReceiveProps(n, r), typeof t.UNSAFE_componentWillReceiveProps == "function" && t.UNSAFE_componentWillReceiveProps(n, r), t.state !== e && Jl.enqueueReplaceState(t, t.state, null);
}
function gi(e, t, n, r) {
  var l = e.stateNode;
  l.props = n, l.state = e.memoizedState, l.refs = {}, io(e);
  var i = t.contextType;
  typeof i == "object" && i !== null ? l.context = et(i) : (i = Ae(t) ? an : Fe.current, l.context = Ln(e, i)), l.state = e.memoizedState, i = t.getDerivedStateFromProps, typeof i == "function" && (vi(e, t, i, n), l.state = e.memoizedState), typeof t.getDerivedStateFromProps == "function" || typeof l.getSnapshotBeforeUpdate == "function" || typeof l.UNSAFE_componentWillMount != "function" && typeof l.componentWillMount != "function" || (t = l.state, typeof l.componentWillMount == "function" && l.componentWillMount(), typeof l.UNSAFE_componentWillMount == "function" && l.UNSAFE_componentWillMount(), t !== l.state && Jl.enqueueReplaceState(l, l.state, null), Ml(e, n, l, r), l.state = e.memoizedState), typeof l.componentDidMount == "function" && (e.flags |= 4194308);
}
function An(e, t) {
  try {
    var n = "", r = t;
    do
      n += Rd(r), r = r.return;
    while (r);
    var l = n;
  } catch (i) {
    l = `
Error generating stack: ` + i.message + `
` + i.stack;
  }
  return { value: e, source: t, stack: l, digest: null };
}
function La(e, t, n) {
  return { value: e, source: null, stack: n ?? null, digest: t ?? null };
}
function xi(e, t) {
  try {
    console.error(t.value);
  } catch (n) {
    setTimeout(function() {
      throw n;
    });
  }
}
var ap = typeof WeakMap == "function" ? WeakMap : Map;
function Cc(e, t, n) {
  n = Nt(-1, n), n.tag = 3, n.payload = { element: null };
  var r = t.value;
  return n.callback = function() {
    Vl || (Vl = !0, Pi = r), xi(e, t);
  }, n;
}
function Ec(e, t, n) {
  n = Nt(-1, n), n.tag = 3;
  var r = e.type.getDerivedStateFromError;
  if (typeof r == "function") {
    var l = t.value;
    n.payload = function() {
      return r(l);
    }, n.callback = function() {
      xi(e, t);
    };
  }
  var i = e.stateNode;
  return i !== null && typeof i.componentDidCatch == "function" && (n.callback = function() {
    xi(e, t), typeof r != "function" && (Bt === null ? Bt = /* @__PURE__ */ new Set([this]) : Bt.add(this));
    var o = t.stack;
    this.componentDidCatch(t.value, { componentStack: o !== null ? o : "" });
  }), n;
}
function ws(e, t, n) {
  var r = e.pingCache;
  if (r === null) {
    r = e.pingCache = new ap();
    var l = /* @__PURE__ */ new Set();
    r.set(t, l);
  } else l = r.get(t), l === void 0 && (l = /* @__PURE__ */ new Set(), r.set(t, l));
  l.has(n) || (l.add(n), e = yp.bind(null, e, t, n), t.then(e, e));
}
function Ss(e) {
  do {
    var t;
    if ((t = e.tag === 13) && (t = e.memoizedState, t = t !== null ? t.dehydrated !== null : !0), t) return e;
    e = e.return;
  } while (e !== null);
  return null;
}
function ks(e, t, n, r, l) {
  return e.mode & 1 ? (e.flags |= 65536, e.lanes = l, e) : (e === t ? e.flags |= 65536 : (e.flags |= 128, n.flags |= 131072, n.flags &= -52805, n.tag === 1 && (n.alternate === null ? n.tag = 17 : (t = Nt(-1, 1), t.tag = 2, Vt(n, t, 1))), n.lanes |= 1), e);
}
var ip = Et.ReactCurrentOwner, Oe = !1;
function Re(e, t, n, r) {
  t.child = e === null ? tc(t, null, n, r) : On(t, e.child, n, r);
}
function Cs(e, t, n, r, l) {
  n = n.render;
  var i = t.ref;
  return zn(t, l), r = fo(e, t, n, r, i, l), n = po(), e !== null && !Oe ? (t.updateQueue = e.updateQueue, t.flags &= -2053, e.lanes &= ~l, Ct(e, t, l)) : (ee && n && Ji(t), t.flags |= 1, Re(e, t, r, l), t.child);
}
function Es(e, t, n, r, l) {
  if (e === null) {
    var i = n.type;
    return typeof i == "function" && !So(i) && i.defaultProps === void 0 && n.compare === null && n.defaultProps === void 0 ? (t.tag = 15, t.type = i, Ic(e, t, i, r, l)) : (e = yl(n.type, null, r, t, t.mode, l), e.ref = t.ref, e.return = t, t.child = e);
  }
  if (i = e.child, !(e.lanes & l)) {
    var o = i.memoizedProps;
    if (n = n.compare, n = n !== null ? n : Cr, n(o, r) && e.ref === t.ref) return Ct(e, t, l);
  }
  return t.flags |= 1, e = Wt(i, r), e.ref = t.ref, e.return = t, t.child = e;
}
function Ic(e, t, n, r, l) {
  if (e !== null) {
    var i = e.memoizedProps;
    if (Cr(i, r) && e.ref === t.ref) if (Oe = !1, t.pendingProps = r = i, (e.lanes & l) !== 0) e.flags & 131072 && (Oe = !0);
    else return t.lanes = e.lanes, Ct(e, t, l);
  }
  return yi(e, t, n, r, l);
}
function Pc(e, t, n) {
  var r = t.pendingProps, l = r.children, i = e !== null ? e.memoizedState : null;
  if (r.mode === "hidden") if (!(t.mode & 1)) t.memoizedState = { baseLanes: 0, cachePool: null, transitions: null }, Y(In, He), He |= n;
  else {
    if (!(n & 1073741824)) return e = i !== null ? i.baseLanes | n : n, t.lanes = t.childLanes = 1073741824, t.memoizedState = { baseLanes: e, cachePool: null, transitions: null }, t.updateQueue = null, Y(In, He), He |= e, null;
    t.memoizedState = { baseLanes: 0, cachePool: null, transitions: null }, r = i !== null ? i.baseLanes : n, Y(In, He), He |= r;
  }
  else i !== null ? (r = i.baseLanes | n, t.memoizedState = null) : r = n, Y(In, He), He |= r;
  return Re(e, t, l, n), t.child;
}
function _c(e, t) {
  var n = t.ref;
  (e === null && n !== null || e !== null && e.ref !== n) && (t.flags |= 512, t.flags |= 2097152);
}
function yi(e, t, n, r, l) {
  var i = Ae(n) ? an : Fe.current;
  return i = Ln(t, i), zn(t, l), n = fo(e, t, n, r, i, l), r = po(), e !== null && !Oe ? (t.updateQueue = e.updateQueue, t.flags &= -2053, e.lanes &= ~l, Ct(e, t, l)) : (ee && r && Ji(t), t.flags |= 1, Re(e, t, n, l), t.child);
}
function Is(e, t, n, r, l) {
  if (Ae(n)) {
    var i = !0;
    Rl(t);
  } else i = !1;
  if (zn(t, l), t.stateNode === null) vl(e, t), kc(t, n, r), gi(t, n, r, l), r = !0;
  else if (e === null) {
    var o = t.stateNode, s = t.memoizedProps;
    o.props = s;
    var u = o.context, d = n.contextType;
    typeof d == "object" && d !== null ? d = et(d) : (d = Ae(n) ? an : Fe.current, d = Ln(t, d));
    var y = n.getDerivedStateFromProps, c = typeof y == "function" || typeof o.getSnapshotBeforeUpdate == "function";
    c || typeof o.UNSAFE_componentWillReceiveProps != "function" && typeof o.componentWillReceiveProps != "function" || (s !== r || u !== d) && Ns(t, o, r, d), zt = !1;
    var m = t.memoizedState;
    o.state = m, Ml(t, r, o, l), u = t.memoizedState, s !== r || m !== u || $e.current || zt ? (typeof y == "function" && (vi(t, n, y, r), u = t.memoizedState), (s = zt || js(t, n, s, r, m, u, d)) ? (c || typeof o.UNSAFE_componentWillMount != "function" && typeof o.componentWillMount != "function" || (typeof o.componentWillMount == "function" && o.componentWillMount(), typeof o.UNSAFE_componentWillMount == "function" && o.UNSAFE_componentWillMount()), typeof o.componentDidMount == "function" && (t.flags |= 4194308)) : (typeof o.componentDidMount == "function" && (t.flags |= 4194308), t.memoizedProps = r, t.memoizedState = u), o.props = r, o.state = u, o.context = d, r = s) : (typeof o.componentDidMount == "function" && (t.flags |= 4194308), r = !1);
  } else {
    o = t.stateNode, rc(e, t), s = t.memoizedProps, d = t.type === t.elementType ? s : rt(t.type, s), o.props = d, c = t.pendingProps, m = o.context, u = n.contextType, typeof u == "object" && u !== null ? u = et(u) : (u = Ae(n) ? an : Fe.current, u = Ln(t, u));
    var v = n.getDerivedStateFromProps;
    (y = typeof v == "function" || typeof o.getSnapshotBeforeUpdate == "function") || typeof o.UNSAFE_componentWillReceiveProps != "function" && typeof o.componentWillReceiveProps != "function" || (s !== c || m !== u) && Ns(t, o, r, u), zt = !1, m = t.memoizedState, o.state = m, Ml(t, r, o, l);
    var g = t.memoizedState;
    s !== c || m !== g || $e.current || zt ? (typeof v == "function" && (vi(t, n, v, r), g = t.memoizedState), (d = zt || js(t, n, d, r, m, g, u) || !1) ? (y || typeof o.UNSAFE_componentWillUpdate != "function" && typeof o.componentWillUpdate != "function" || (typeof o.componentWillUpdate == "function" && o.componentWillUpdate(r, g, u), typeof o.UNSAFE_componentWillUpdate == "function" && o.UNSAFE_componentWillUpdate(r, g, u)), typeof o.componentDidUpdate == "function" && (t.flags |= 4), typeof o.getSnapshotBeforeUpdate == "function" && (t.flags |= 1024)) : (typeof o.componentDidUpdate != "function" || s === e.memoizedProps && m === e.memoizedState || (t.flags |= 4), typeof o.getSnapshotBeforeUpdate != "function" || s === e.memoizedProps && m === e.memoizedState || (t.flags |= 1024), t.memoizedProps = r, t.memoizedState = g), o.props = r, o.state = g, o.context = u, r = d) : (typeof o.componentDidUpdate != "function" || s === e.memoizedProps && m === e.memoizedState || (t.flags |= 4), typeof o.getSnapshotBeforeUpdate != "function" || s === e.memoizedProps && m === e.memoizedState || (t.flags |= 1024), r = !1);
  }
  return ji(e, t, n, r, i, l);
}
function ji(e, t, n, r, l, i) {
  _c(e, t);
  var o = (t.flags & 128) !== 0;
  if (!r && !o) return l && fs(t, n, !1), Ct(e, t, i);
  r = t.stateNode, ip.current = t;
  var s = o && typeof n.getDerivedStateFromError != "function" ? null : r.render();
  return t.flags |= 1, e !== null && o ? (t.child = On(t, e.child, null, i), t.child = On(t, null, s, i)) : Re(e, t, s, i), t.memoizedState = r.state, l && fs(t, n, !0), t.child;
}
function Fc(e) {
  var t = e.stateNode;
  t.pendingContext ? ds(e, t.pendingContext, t.pendingContext !== t.context) : t.context && ds(e, t.context, !1), oo(e, t.containerInfo);
}
function Ps(e, t, n, r, l) {
  return Mn(), to(l), t.flags |= 256, Re(e, t, n, r), t.child;
}
var Ni = { dehydrated: null, treeContext: null, retryLane: 0 };
function wi(e) {
  return { baseLanes: e, cachePool: null, transitions: null };
}
function Rc(e, t, n) {
  var r = t.pendingProps, l = ne.current, i = !1, o = (t.flags & 128) !== 0, s;
  if ((s = o) || (s = e !== null && e.memoizedState === null ? !1 : (l & 2) !== 0), s ? (i = !0, t.flags &= -129) : (e === null || e.memoizedState !== null) && (l |= 1), Y(ne, l & 1), e === null)
    return mi(t), e = t.memoizedState, e !== null && (e = e.dehydrated, e !== null) ? (t.mode & 1 ? e.data === "$!" ? t.lanes = 8 : t.lanes = 1073741824 : t.lanes = 1, null) : (o = r.children, e = r.fallback, i ? (r = t.mode, i = t.child, o = { mode: "hidden", children: o }, !(r & 1) && i !== null ? (i.childLanes = 0, i.pendingProps = o) : i = na(o, r, 0, null), e = ln(e, r, n, null), i.return = t, e.return = t, i.sibling = e, t.child = i, t.child.memoizedState = wi(n), t.memoizedState = Ni, e) : vo(t, o));
  if (l = e.memoizedState, l !== null && (s = l.dehydrated, s !== null)) return op(e, t, o, r, s, l, n);
  if (i) {
    i = r.fallback, o = t.mode, l = e.child, s = l.sibling;
    var u = { mode: "hidden", children: r.children };
    return !(o & 1) && t.child !== l ? (r = t.child, r.childLanes = 0, r.pendingProps = u, t.deletions = null) : (r = Wt(l, u), r.subtreeFlags = l.subtreeFlags & 14680064), s !== null ? i = Wt(s, i) : (i = ln(i, o, n, null), i.flags |= 2), i.return = t, r.return = t, r.sibling = i, t.child = r, r = i, i = t.child, o = e.child.memoizedState, o = o === null ? wi(n) : { baseLanes: o.baseLanes | n, cachePool: null, transitions: o.transitions }, i.memoizedState = o, i.childLanes = e.childLanes & ~n, t.memoizedState = Ni, r;
  }
  return i = e.child, e = i.sibling, r = Wt(i, { mode: "visible", children: r.children }), !(t.mode & 1) && (r.lanes = n), r.return = t, r.sibling = null, e !== null && (n = t.deletions, n === null ? (t.deletions = [e], t.flags |= 16) : n.push(e)), t.child = r, t.memoizedState = null, r;
}
function vo(e, t) {
  return t = na({ mode: "visible", children: t }, e.mode, 0, null), t.return = e, e.child = t;
}
function ll(e, t, n, r) {
  return r !== null && to(r), On(t, e.child, null, n), e = vo(t, t.pendingProps.children), e.flags |= 2, t.memoizedState = null, e;
}
function op(e, t, n, r, l, i, o) {
  if (n)
    return t.flags & 256 ? (t.flags &= -257, r = La(Error(P(422))), ll(e, t, o, r)) : t.memoizedState !== null ? (t.child = e.child, t.flags |= 128, null) : (i = r.fallback, l = t.mode, r = na({ mode: "visible", children: r.children }, l, 0, null), i = ln(i, l, o, null), i.flags |= 2, r.return = t, i.return = t, r.sibling = i, t.child = r, t.mode & 1 && On(t, e.child, null, o), t.child.memoizedState = wi(o), t.memoizedState = Ni, i);
  if (!(t.mode & 1)) return ll(e, t, o, null);
  if (l.data === "$!") {
    if (r = l.nextSibling && l.nextSibling.dataset, r) var s = r.dgst;
    return r = s, i = Error(P(419)), r = La(i, r, void 0), ll(e, t, o, r);
  }
  if (s = (o & e.childLanes) !== 0, Oe || s) {
    if (r = je, r !== null) {
      switch (o & -o) {
        case 4:
          l = 2;
          break;
        case 16:
          l = 8;
          break;
        case 64:
        case 128:
        case 256:
        case 512:
        case 1024:
        case 2048:
        case 4096:
        case 8192:
        case 16384:
        case 32768:
        case 65536:
        case 131072:
        case 262144:
        case 524288:
        case 1048576:
        case 2097152:
        case 4194304:
        case 8388608:
        case 16777216:
        case 33554432:
        case 67108864:
          l = 32;
          break;
        case 536870912:
          l = 268435456;
          break;
        default:
          l = 0;
      }
      l = l & (r.suspendedLanes | o) ? 0 : l, l !== 0 && l !== i.retryLane && (i.retryLane = l, kt(e, l), st(r, e, l, -1));
    }
    return wo(), r = La(Error(P(421))), ll(e, t, o, r);
  }
  return l.data === "$?" ? (t.flags |= 128, t.child = e.child, t = jp.bind(null, e), l._reactRetry = t, null) : (e = i.treeContext, We = Ut(l.nextSibling), Qe = t, ee = !0, at = null, e !== null && (Ye[Xe++] = yt, Ye[Xe++] = jt, Ye[Xe++] = on, yt = e.id, jt = e.overflow, on = t), t = vo(t, r.children), t.flags |= 4096, t);
}
function _s(e, t, n) {
  e.lanes |= t;
  var r = e.alternate;
  r !== null && (r.lanes |= t), hi(e.return, t, n);
}
function Ma(e, t, n, r, l) {
  var i = e.memoizedState;
  i === null ? e.memoizedState = { isBackwards: t, rendering: null, renderingStartTime: 0, last: r, tail: n, tailMode: l } : (i.isBackwards = t, i.rendering = null, i.renderingStartTime = 0, i.last = r, i.tail = n, i.tailMode = l);
}
function zc(e, t, n) {
  var r = t.pendingProps, l = r.revealOrder, i = r.tail;
  if (Re(e, t, r.children, n), r = ne.current, r & 2) r = r & 1 | 2, t.flags |= 128;
  else {
    if (e !== null && e.flags & 128) e: for (e = t.child; e !== null; ) {
      if (e.tag === 13) e.memoizedState !== null && _s(e, n, t);
      else if (e.tag === 19) _s(e, n, t);
      else if (e.child !== null) {
        e.child.return = e, e = e.child;
        continue;
      }
      if (e === t) break e;
      for (; e.sibling === null; ) {
        if (e.return === null || e.return === t) break e;
        e = e.return;
      }
      e.sibling.return = e.return, e = e.sibling;
    }
    r &= 1;
  }
  if (Y(ne, r), !(t.mode & 1)) t.memoizedState = null;
  else switch (l) {
    case "forwards":
      for (n = t.child, l = null; n !== null; ) e = n.alternate, e !== null && Ol(e) === null && (l = n), n = n.sibling;
      n = l, n === null ? (l = t.child, t.child = null) : (l = n.sibling, n.sibling = null), Ma(t, !1, l, n, i);
      break;
    case "backwards":
      for (n = null, l = t.child, t.child = null; l !== null; ) {
        if (e = l.alternate, e !== null && Ol(e) === null) {
          t.child = l;
          break;
        }
        e = l.sibling, l.sibling = n, n = l, l = e;
      }
      Ma(t, !0, n, null, i);
      break;
    case "together":
      Ma(t, !1, null, null, void 0);
      break;
    default:
      t.memoizedState = null;
  }
  return t.child;
}
function vl(e, t) {
  !(t.mode & 1) && e !== null && (e.alternate = null, t.alternate = null, t.flags |= 2);
}
function Ct(e, t, n) {
  if (e !== null && (t.dependencies = e.dependencies), un |= t.lanes, !(n & t.childLanes)) return null;
  if (e !== null && t.child !== e.child) throw Error(P(153));
  if (t.child !== null) {
    for (e = t.child, n = Wt(e, e.pendingProps), t.child = n, n.return = t; e.sibling !== null; ) e = e.sibling, n = n.sibling = Wt(e, e.pendingProps), n.return = t;
    n.sibling = null;
  }
  return t.child;
}
function sp(e, t, n) {
  switch (t.tag) {
    case 3:
      Fc(t), Mn();
      break;
    case 5:
      lc(t);
      break;
    case 1:
      Ae(t.type) && Rl(t);
      break;
    case 4:
      oo(t, t.stateNode.containerInfo);
      break;
    case 10:
      var r = t.type._context, l = t.memoizedProps.value;
      Y(Dl, r._currentValue), r._currentValue = l;
      break;
    case 13:
      if (r = t.memoizedState, r !== null)
        return r.dehydrated !== null ? (Y(ne, ne.current & 1), t.flags |= 128, null) : n & t.child.childLanes ? Rc(e, t, n) : (Y(ne, ne.current & 1), e = Ct(e, t, n), e !== null ? e.sibling : null);
      Y(ne, ne.current & 1);
      break;
    case 19:
      if (r = (n & t.childLanes) !== 0, e.flags & 128) {
        if (r) return zc(e, t, n);
        t.flags |= 128;
      }
      if (l = t.memoizedState, l !== null && (l.rendering = null, l.tail = null, l.lastEffect = null), Y(ne, ne.current), r) break;
      return null;
    case 22:
    case 23:
      return t.lanes = 0, Pc(e, t, n);
  }
  return Ct(e, t, n);
}
var Tc, Si, Dc, Lc;
Tc = function(e, t) {
  for (var n = t.child; n !== null; ) {
    if (n.tag === 5 || n.tag === 6) e.appendChild(n.stateNode);
    else if (n.tag !== 4 && n.child !== null) {
      n.child.return = n, n = n.child;
      continue;
    }
    if (n === t) break;
    for (; n.sibling === null; ) {
      if (n.return === null || n.return === t) return;
      n = n.return;
    }
    n.sibling.return = n.return, n = n.sibling;
  }
};
Si = function() {
};
Dc = function(e, t, n, r) {
  var l = e.memoizedProps;
  if (l !== r) {
    e = t.stateNode, nn(mt.current);
    var i = null;
    switch (n) {
      case "input":
        l = Qa(e, l), r = Qa(e, r), i = [];
        break;
      case "select":
        l = le({}, l, { value: void 0 }), r = le({}, r, { value: void 0 }), i = [];
        break;
      case "textarea":
        l = qa(e, l), r = qa(e, r), i = [];
        break;
      default:
        typeof l.onClick != "function" && typeof r.onClick == "function" && (e.onclick = _l);
    }
    Ya(n, r);
    var o;
    n = null;
    for (d in l) if (!r.hasOwnProperty(d) && l.hasOwnProperty(d) && l[d] != null) if (d === "style") {
      var s = l[d];
      for (o in s) s.hasOwnProperty(o) && (n || (n = {}), n[o] = "");
    } else d !== "dangerouslySetInnerHTML" && d !== "children" && d !== "suppressContentEditableWarning" && d !== "suppressHydrationWarning" && d !== "autoFocus" && (xr.hasOwnProperty(d) ? i || (i = []) : (i = i || []).push(d, null));
    for (d in r) {
      var u = r[d];
      if (s = l != null ? l[d] : void 0, r.hasOwnProperty(d) && u !== s && (u != null || s != null)) if (d === "style") if (s) {
        for (o in s) !s.hasOwnProperty(o) || u && u.hasOwnProperty(o) || (n || (n = {}), n[o] = "");
        for (o in u) u.hasOwnProperty(o) && s[o] !== u[o] && (n || (n = {}), n[o] = u[o]);
      } else n || (i || (i = []), i.push(
        d,
        n
      )), n = u;
      else d === "dangerouslySetInnerHTML" ? (u = u ? u.__html : void 0, s = s ? s.__html : void 0, u != null && s !== u && (i = i || []).push(d, u)) : d === "children" ? typeof u != "string" && typeof u != "number" || (i = i || []).push(d, "" + u) : d !== "suppressContentEditableWarning" && d !== "suppressHydrationWarning" && (xr.hasOwnProperty(d) ? (u != null && d === "onScroll" && X("scroll", e), i || s === u || (i = [])) : (i = i || []).push(d, u));
    }
    n && (i = i || []).push("style", n);
    var d = i;
    (t.updateQueue = d) && (t.flags |= 4);
  }
};
Lc = function(e, t, n, r) {
  n !== r && (t.flags |= 4);
};
function nr(e, t) {
  if (!ee) switch (e.tailMode) {
    case "hidden":
      t = e.tail;
      for (var n = null; t !== null; ) t.alternate !== null && (n = t), t = t.sibling;
      n === null ? e.tail = null : n.sibling = null;
      break;
    case "collapsed":
      n = e.tail;
      for (var r = null; n !== null; ) n.alternate !== null && (r = n), n = n.sibling;
      r === null ? t || e.tail === null ? e.tail = null : e.tail.sibling = null : r.sibling = null;
  }
}
function Ie(e) {
  var t = e.alternate !== null && e.alternate.child === e.child, n = 0, r = 0;
  if (t) for (var l = e.child; l !== null; ) n |= l.lanes | l.childLanes, r |= l.subtreeFlags & 14680064, r |= l.flags & 14680064, l.return = e, l = l.sibling;
  else for (l = e.child; l !== null; ) n |= l.lanes | l.childLanes, r |= l.subtreeFlags, r |= l.flags, l.return = e, l = l.sibling;
  return e.subtreeFlags |= r, e.childLanes = n, t;
}
function up(e, t, n) {
  var r = t.pendingProps;
  switch (eo(t), t.tag) {
    case 2:
    case 16:
    case 15:
    case 0:
    case 11:
    case 7:
    case 8:
    case 12:
    case 9:
    case 14:
      return Ie(t), null;
    case 1:
      return Ae(t.type) && Fl(), Ie(t), null;
    case 3:
      return r = t.stateNode, $n(), Z($e), Z(Fe), uo(), r.pendingContext && (r.context = r.pendingContext, r.pendingContext = null), (e === null || e.child === null) && (nl(t) ? t.flags |= 4 : e === null || e.memoizedState.isDehydrated && !(t.flags & 256) || (t.flags |= 1024, at !== null && (Ri(at), at = null))), Si(e, t), Ie(t), null;
    case 5:
      so(t);
      var l = nn(Fr.current);
      if (n = t.type, e !== null && t.stateNode != null) Dc(e, t, n, r, l), e.ref !== t.ref && (t.flags |= 512, t.flags |= 2097152);
      else {
        if (!r) {
          if (t.stateNode === null) throw Error(P(166));
          return Ie(t), null;
        }
        if (e = nn(mt.current), nl(t)) {
          r = t.stateNode, n = t.type;
          var i = t.memoizedProps;
          switch (r[ft] = t, r[Pr] = i, e = (t.mode & 1) !== 0, n) {
            case "dialog":
              X("cancel", r), X("close", r);
              break;
            case "iframe":
            case "object":
            case "embed":
              X("load", r);
              break;
            case "video":
            case "audio":
              for (l = 0; l < or.length; l++) X(or[l], r);
              break;
            case "source":
              X("error", r);
              break;
            case "img":
            case "image":
            case "link":
              X(
                "error",
                r
              ), X("load", r);
              break;
            case "details":
              X("toggle", r);
              break;
            case "input":
              $o(r, i), X("invalid", r);
              break;
            case "select":
              r._wrapperState = { wasMultiple: !!i.multiple }, X("invalid", r);
              break;
            case "textarea":
              Uo(r, i), X("invalid", r);
          }
          Ya(n, i), l = null;
          for (var o in i) if (i.hasOwnProperty(o)) {
            var s = i[o];
            o === "children" ? typeof s == "string" ? r.textContent !== s && (i.suppressHydrationWarning !== !0 && tl(r.textContent, s, e), l = ["children", s]) : typeof s == "number" && r.textContent !== "" + s && (i.suppressHydrationWarning !== !0 && tl(
              r.textContent,
              s,
              e
            ), l = ["children", "" + s]) : xr.hasOwnProperty(o) && s != null && o === "onScroll" && X("scroll", r);
          }
          switch (n) {
            case "input":
              Kr(r), Ao(r, i, !0);
              break;
            case "textarea":
              Kr(r), Vo(r);
              break;
            case "select":
            case "option":
              break;
            default:
              typeof i.onClick == "function" && (r.onclick = _l);
          }
          r = l, t.updateQueue = r, r !== null && (t.flags |= 4);
        } else {
          o = l.nodeType === 9 ? l : l.ownerDocument, e === "http://www.w3.org/1999/xhtml" && (e = uu(n)), e === "http://www.w3.org/1999/xhtml" ? n === "script" ? (e = o.createElement("div"), e.innerHTML = "<script><\/script>", e = e.removeChild(e.firstChild)) : typeof r.is == "string" ? e = o.createElement(n, { is: r.is }) : (e = o.createElement(n), n === "select" && (o = e, r.multiple ? o.multiple = !0 : r.size && (o.size = r.size))) : e = o.createElementNS(e, n), e[ft] = t, e[Pr] = r, Tc(e, t, !1, !1), t.stateNode = e;
          e: {
            switch (o = Xa(n, r), n) {
              case "dialog":
                X("cancel", e), X("close", e), l = r;
                break;
              case "iframe":
              case "object":
              case "embed":
                X("load", e), l = r;
                break;
              case "video":
              case "audio":
                for (l = 0; l < or.length; l++) X(or[l], e);
                l = r;
                break;
              case "source":
                X("error", e), l = r;
                break;
              case "img":
              case "image":
              case "link":
                X(
                  "error",
                  e
                ), X("load", e), l = r;
                break;
              case "details":
                X("toggle", e), l = r;
                break;
              case "input":
                $o(e, r), l = Qa(e, r), X("invalid", e);
                break;
              case "option":
                l = r;
                break;
              case "select":
                e._wrapperState = { wasMultiple: !!r.multiple }, l = le({}, r, { value: void 0 }), X("invalid", e);
                break;
              case "textarea":
                Uo(e, r), l = qa(e, r), X("invalid", e);
                break;
              default:
                l = r;
            }
            Ya(n, l), s = l;
            for (i in s) if (s.hasOwnProperty(i)) {
              var u = s[i];
              i === "style" ? fu(e, u) : i === "dangerouslySetInnerHTML" ? (u = u ? u.__html : void 0, u != null && cu(e, u)) : i === "children" ? typeof u == "string" ? (n !== "textarea" || u !== "") && yr(e, u) : typeof u == "number" && yr(e, "" + u) : i !== "suppressContentEditableWarning" && i !== "suppressHydrationWarning" && i !== "autoFocus" && (xr.hasOwnProperty(i) ? u != null && i === "onScroll" && X("scroll", e) : u != null && Ai(e, i, u, o));
            }
            switch (n) {
              case "input":
                Kr(e), Ao(e, r, !1);
                break;
              case "textarea":
                Kr(e), Vo(e);
                break;
              case "option":
                r.value != null && e.setAttribute("value", "" + Qt(r.value));
                break;
              case "select":
                e.multiple = !!r.multiple, i = r.value, i != null ? Pn(e, !!r.multiple, i, !1) : r.defaultValue != null && Pn(
                  e,
                  !!r.multiple,
                  r.defaultValue,
                  !0
                );
                break;
              default:
                typeof l.onClick == "function" && (e.onclick = _l);
            }
            switch (n) {
              case "button":
              case "input":
              case "select":
              case "textarea":
                r = !!r.autoFocus;
                break e;
              case "img":
                r = !0;
                break e;
              default:
                r = !1;
            }
          }
          r && (t.flags |= 4);
        }
        t.ref !== null && (t.flags |= 512, t.flags |= 2097152);
      }
      return Ie(t), null;
    case 6:
      if (e && t.stateNode != null) Lc(e, t, e.memoizedProps, r);
      else {
        if (typeof r != "string" && t.stateNode === null) throw Error(P(166));
        if (n = nn(Fr.current), nn(mt.current), nl(t)) {
          if (r = t.stateNode, n = t.memoizedProps, r[ft] = t, (i = r.nodeValue !== n) && (e = Qe, e !== null)) switch (e.tag) {
            case 3:
              tl(r.nodeValue, n, (e.mode & 1) !== 0);
              break;
            case 5:
              e.memoizedProps.suppressHydrationWarning !== !0 && tl(r.nodeValue, n, (e.mode & 1) !== 0);
          }
          i && (t.flags |= 4);
        } else r = (n.nodeType === 9 ? n : n.ownerDocument).createTextNode(r), r[ft] = t, t.stateNode = r;
      }
      return Ie(t), null;
    case 13:
      if (Z(ne), r = t.memoizedState, e === null || e.memoizedState !== null && e.memoizedState.dehydrated !== null) {
        if (ee && We !== null && t.mode & 1 && !(t.flags & 128)) Ju(), Mn(), t.flags |= 98560, i = !1;
        else if (i = nl(t), r !== null && r.dehydrated !== null) {
          if (e === null) {
            if (!i) throw Error(P(318));
            if (i = t.memoizedState, i = i !== null ? i.dehydrated : null, !i) throw Error(P(317));
            i[ft] = t;
          } else Mn(), !(t.flags & 128) && (t.memoizedState = null), t.flags |= 4;
          Ie(t), i = !1;
        } else at !== null && (Ri(at), at = null), i = !0;
        if (!i) return t.flags & 65536 ? t : null;
      }
      return t.flags & 128 ? (t.lanes = n, t) : (r = r !== null, r !== (e !== null && e.memoizedState !== null) && r && (t.child.flags |= 8192, t.mode & 1 && (e === null || ne.current & 1 ? ve === 0 && (ve = 3) : wo())), t.updateQueue !== null && (t.flags |= 4), Ie(t), null);
    case 4:
      return $n(), Si(e, t), e === null && Er(t.stateNode.containerInfo), Ie(t), null;
    case 10:
      return lo(t.type._context), Ie(t), null;
    case 17:
      return Ae(t.type) && Fl(), Ie(t), null;
    case 19:
      if (Z(ne), i = t.memoizedState, i === null) return Ie(t), null;
      if (r = (t.flags & 128) !== 0, o = i.rendering, o === null) if (r) nr(i, !1);
      else {
        if (ve !== 0 || e !== null && e.flags & 128) for (e = t.child; e !== null; ) {
          if (o = Ol(e), o !== null) {
            for (t.flags |= 128, nr(i, !1), r = o.updateQueue, r !== null && (t.updateQueue = r, t.flags |= 4), t.subtreeFlags = 0, r = n, n = t.child; n !== null; ) i = n, e = r, i.flags &= 14680066, o = i.alternate, o === null ? (i.childLanes = 0, i.lanes = e, i.child = null, i.subtreeFlags = 0, i.memoizedProps = null, i.memoizedState = null, i.updateQueue = null, i.dependencies = null, i.stateNode = null) : (i.childLanes = o.childLanes, i.lanes = o.lanes, i.child = o.child, i.subtreeFlags = 0, i.deletions = null, i.memoizedProps = o.memoizedProps, i.memoizedState = o.memoizedState, i.updateQueue = o.updateQueue, i.type = o.type, e = o.dependencies, i.dependencies = e === null ? null : { lanes: e.lanes, firstContext: e.firstContext }), n = n.sibling;
            return Y(ne, ne.current & 1 | 2), t.child;
          }
          e = e.sibling;
        }
        i.tail !== null && ce() > Un && (t.flags |= 128, r = !0, nr(i, !1), t.lanes = 4194304);
      }
      else {
        if (!r) if (e = Ol(o), e !== null) {
          if (t.flags |= 128, r = !0, n = e.updateQueue, n !== null && (t.updateQueue = n, t.flags |= 4), nr(i, !0), i.tail === null && i.tailMode === "hidden" && !o.alternate && !ee) return Ie(t), null;
        } else 2 * ce() - i.renderingStartTime > Un && n !== 1073741824 && (t.flags |= 128, r = !0, nr(i, !1), t.lanes = 4194304);
        i.isBackwards ? (o.sibling = t.child, t.child = o) : (n = i.last, n !== null ? n.sibling = o : t.child = o, i.last = o);
      }
      return i.tail !== null ? (t = i.tail, i.rendering = t, i.tail = t.sibling, i.renderingStartTime = ce(), t.sibling = null, n = ne.current, Y(ne, r ? n & 1 | 2 : n & 1), t) : (Ie(t), null);
    case 22:
    case 23:
      return No(), r = t.memoizedState !== null, e !== null && e.memoizedState !== null !== r && (t.flags |= 8192), r && t.mode & 1 ? He & 1073741824 && (Ie(t), t.subtreeFlags & 6 && (t.flags |= 8192)) : Ie(t), null;
    case 24:
      return null;
    case 25:
      return null;
  }
  throw Error(P(156, t.tag));
}
function cp(e, t) {
  switch (eo(t), t.tag) {
    case 1:
      return Ae(t.type) && Fl(), e = t.flags, e & 65536 ? (t.flags = e & -65537 | 128, t) : null;
    case 3:
      return $n(), Z($e), Z(Fe), uo(), e = t.flags, e & 65536 && !(e & 128) ? (t.flags = e & -65537 | 128, t) : null;
    case 5:
      return so(t), null;
    case 13:
      if (Z(ne), e = t.memoizedState, e !== null && e.dehydrated !== null) {
        if (t.alternate === null) throw Error(P(340));
        Mn();
      }
      return e = t.flags, e & 65536 ? (t.flags = e & -65537 | 128, t) : null;
    case 19:
      return Z(ne), null;
    case 4:
      return $n(), null;
    case 10:
      return lo(t.type._context), null;
    case 22:
    case 23:
      return No(), null;
    case 24:
      return null;
    default:
      return null;
  }
}
var al = !1, Pe = !1, dp = typeof WeakSet == "function" ? WeakSet : Set, O = null;
function En(e, t) {
  var n = e.ref;
  if (n !== null) if (typeof n == "function") try {
    n(null);
  } catch (r) {
    oe(e, t, r);
  }
  else n.current = null;
}
function ki(e, t, n) {
  try {
    n();
  } catch (r) {
    oe(e, t, r);
  }
}
var Fs = !1;
function fp(e, t) {
  if (oi = El, e = Uu(), Zi(e)) {
    if ("selectionStart" in e) var n = { start: e.selectionStart, end: e.selectionEnd };
    else e: {
      n = (n = e.ownerDocument) && n.defaultView || window;
      var r = n.getSelection && n.getSelection();
      if (r && r.rangeCount !== 0) {
        n = r.anchorNode;
        var l = r.anchorOffset, i = r.focusNode;
        r = r.focusOffset;
        try {
          n.nodeType, i.nodeType;
        } catch {
          n = null;
          break e;
        }
        var o = 0, s = -1, u = -1, d = 0, y = 0, c = e, m = null;
        t: for (; ; ) {
          for (var v; c !== n || l !== 0 && c.nodeType !== 3 || (s = o + l), c !== i || r !== 0 && c.nodeType !== 3 || (u = o + r), c.nodeType === 3 && (o += c.nodeValue.length), (v = c.firstChild) !== null; )
            m = c, c = v;
          for (; ; ) {
            if (c === e) break t;
            if (m === n && ++d === l && (s = o), m === i && ++y === r && (u = o), (v = c.nextSibling) !== null) break;
            c = m, m = c.parentNode;
          }
          c = v;
        }
        n = s === -1 || u === -1 ? null : { start: s, end: u };
      } else n = null;
    }
    n = n || { start: 0, end: 0 };
  } else n = null;
  for (si = { focusedElem: e, selectionRange: n }, El = !1, O = t; O !== null; ) if (t = O, e = t.child, (t.subtreeFlags & 1028) !== 0 && e !== null) e.return = t, O = e;
  else for (; O !== null; ) {
    t = O;
    try {
      var g = t.alternate;
      if (t.flags & 1024) switch (t.tag) {
        case 0:
        case 11:
        case 15:
          break;
        case 1:
          if (g !== null) {
            var N = g.memoizedProps, F = g.memoizedState, p = t.stateNode, f = p.getSnapshotBeforeUpdate(t.elementType === t.type ? N : rt(t.type, N), F);
            p.__reactInternalSnapshotBeforeUpdate = f;
          }
          break;
        case 3:
          var h = t.stateNode.containerInfo;
          h.nodeType === 1 ? h.textContent = "" : h.nodeType === 9 && h.documentElement && h.removeChild(h.documentElement);
          break;
        case 5:
        case 6:
        case 4:
        case 17:
          break;
        default:
          throw Error(P(163));
      }
    } catch (k) {
      oe(t, t.return, k);
    }
    if (e = t.sibling, e !== null) {
      e.return = t.return, O = e;
      break;
    }
    O = t.return;
  }
  return g = Fs, Fs = !1, g;
}
function mr(e, t, n) {
  var r = t.updateQueue;
  if (r = r !== null ? r.lastEffect : null, r !== null) {
    var l = r = r.next;
    do {
      if ((l.tag & e) === e) {
        var i = l.destroy;
        l.destroy = void 0, i !== void 0 && ki(t, n, i);
      }
      l = l.next;
    } while (l !== r);
  }
}
function ea(e, t) {
  if (t = t.updateQueue, t = t !== null ? t.lastEffect : null, t !== null) {
    var n = t = t.next;
    do {
      if ((n.tag & e) === e) {
        var r = n.create;
        n.destroy = r();
      }
      n = n.next;
    } while (n !== t);
  }
}
function Ci(e) {
  var t = e.ref;
  if (t !== null) {
    var n = e.stateNode;
    switch (e.tag) {
      case 5:
        e = n;
        break;
      default:
        e = n;
    }
    typeof t == "function" ? t(e) : t.current = e;
  }
}
function Mc(e) {
  var t = e.alternate;
  t !== null && (e.alternate = null, Mc(t)), e.child = null, e.deletions = null, e.sibling = null, e.tag === 5 && (t = e.stateNode, t !== null && (delete t[ft], delete t[Pr], delete t[di], delete t[qf], delete t[bf])), e.stateNode = null, e.return = null, e.dependencies = null, e.memoizedProps = null, e.memoizedState = null, e.pendingProps = null, e.stateNode = null, e.updateQueue = null;
}
function Oc(e) {
  return e.tag === 5 || e.tag === 3 || e.tag === 4;
}
function Rs(e) {
  e: for (; ; ) {
    for (; e.sibling === null; ) {
      if (e.return === null || Oc(e.return)) return null;
      e = e.return;
    }
    for (e.sibling.return = e.return, e = e.sibling; e.tag !== 5 && e.tag !== 6 && e.tag !== 18; ) {
      if (e.flags & 2 || e.child === null || e.tag === 4) continue e;
      e.child.return = e, e = e.child;
    }
    if (!(e.flags & 2)) return e.stateNode;
  }
}
function Ei(e, t, n) {
  var r = e.tag;
  if (r === 5 || r === 6) e = e.stateNode, t ? n.nodeType === 8 ? n.parentNode.insertBefore(e, t) : n.insertBefore(e, t) : (n.nodeType === 8 ? (t = n.parentNode, t.insertBefore(e, n)) : (t = n, t.appendChild(e)), n = n._reactRootContainer, n != null || t.onclick !== null || (t.onclick = _l));
  else if (r !== 4 && (e = e.child, e !== null)) for (Ei(e, t, n), e = e.sibling; e !== null; ) Ei(e, t, n), e = e.sibling;
}
function Ii(e, t, n) {
  var r = e.tag;
  if (r === 5 || r === 6) e = e.stateNode, t ? n.insertBefore(e, t) : n.appendChild(e);
  else if (r !== 4 && (e = e.child, e !== null)) for (Ii(e, t, n), e = e.sibling; e !== null; ) Ii(e, t, n), e = e.sibling;
}
var we = null, lt = !1;
function Ft(e, t, n) {
  for (n = n.child; n !== null; ) $c(e, t, n), n = n.sibling;
}
function $c(e, t, n) {
  if (pt && typeof pt.onCommitFiberUnmount == "function") try {
    pt.onCommitFiberUnmount(Gl, n);
  } catch {
  }
  switch (n.tag) {
    case 5:
      Pe || En(n, t);
    case 6:
      var r = we, l = lt;
      we = null, Ft(e, t, n), we = r, lt = l, we !== null && (lt ? (e = we, n = n.stateNode, e.nodeType === 8 ? e.parentNode.removeChild(n) : e.removeChild(n)) : we.removeChild(n.stateNode));
      break;
    case 18:
      we !== null && (lt ? (e = we, n = n.stateNode, e.nodeType === 8 ? _a(e.parentNode, n) : e.nodeType === 1 && _a(e, n), Sr(e)) : _a(we, n.stateNode));
      break;
    case 4:
      r = we, l = lt, we = n.stateNode.containerInfo, lt = !0, Ft(e, t, n), we = r, lt = l;
      break;
    case 0:
    case 11:
    case 14:
    case 15:
      if (!Pe && (r = n.updateQueue, r !== null && (r = r.lastEffect, r !== null))) {
        l = r = r.next;
        do {
          var i = l, o = i.destroy;
          i = i.tag, o !== void 0 && (i & 2 || i & 4) && ki(n, t, o), l = l.next;
        } while (l !== r);
      }
      Ft(e, t, n);
      break;
    case 1:
      if (!Pe && (En(n, t), r = n.stateNode, typeof r.componentWillUnmount == "function")) try {
        r.props = n.memoizedProps, r.state = n.memoizedState, r.componentWillUnmount();
      } catch (s) {
        oe(n, t, s);
      }
      Ft(e, t, n);
      break;
    case 21:
      Ft(e, t, n);
      break;
    case 22:
      n.mode & 1 ? (Pe = (r = Pe) || n.memoizedState !== null, Ft(e, t, n), Pe = r) : Ft(e, t, n);
      break;
    default:
      Ft(e, t, n);
  }
}
function zs(e) {
  var t = e.updateQueue;
  if (t !== null) {
    e.updateQueue = null;
    var n = e.stateNode;
    n === null && (n = e.stateNode = new dp()), t.forEach(function(r) {
      var l = Np.bind(null, e, r);
      n.has(r) || (n.add(r), r.then(l, l));
    });
  }
}
function nt(e, t) {
  var n = t.deletions;
  if (n !== null) for (var r = 0; r < n.length; r++) {
    var l = n[r];
    try {
      var i = e, o = t, s = o;
      e: for (; s !== null; ) {
        switch (s.tag) {
          case 5:
            we = s.stateNode, lt = !1;
            break e;
          case 3:
            we = s.stateNode.containerInfo, lt = !0;
            break e;
          case 4:
            we = s.stateNode.containerInfo, lt = !0;
            break e;
        }
        s = s.return;
      }
      if (we === null) throw Error(P(160));
      $c(i, o, l), we = null, lt = !1;
      var u = l.alternate;
      u !== null && (u.return = null), l.return = null;
    } catch (d) {
      oe(l, t, d);
    }
  }
  if (t.subtreeFlags & 12854) for (t = t.child; t !== null; ) Ac(t, e), t = t.sibling;
}
function Ac(e, t) {
  var n = e.alternate, r = e.flags;
  switch (e.tag) {
    case 0:
    case 11:
    case 14:
    case 15:
      if (nt(t, e), ct(e), r & 4) {
        try {
          mr(3, e, e.return), ea(3, e);
        } catch (N) {
          oe(e, e.return, N);
        }
        try {
          mr(5, e, e.return);
        } catch (N) {
          oe(e, e.return, N);
        }
      }
      break;
    case 1:
      nt(t, e), ct(e), r & 512 && n !== null && En(n, n.return);
      break;
    case 5:
      if (nt(t, e), ct(e), r & 512 && n !== null && En(n, n.return), e.flags & 32) {
        var l = e.stateNode;
        try {
          yr(l, "");
        } catch (N) {
          oe(e, e.return, N);
        }
      }
      if (r & 4 && (l = e.stateNode, l != null)) {
        var i = e.memoizedProps, o = n !== null ? n.memoizedProps : i, s = e.type, u = e.updateQueue;
        if (e.updateQueue = null, u !== null) try {
          s === "input" && i.type === "radio" && i.name != null && ou(l, i), Xa(s, o);
          var d = Xa(s, i);
          for (o = 0; o < u.length; o += 2) {
            var y = u[o], c = u[o + 1];
            y === "style" ? fu(l, c) : y === "dangerouslySetInnerHTML" ? cu(l, c) : y === "children" ? yr(l, c) : Ai(l, y, c, d);
          }
          switch (s) {
            case "input":
              Ga(l, i);
              break;
            case "textarea":
              su(l, i);
              break;
            case "select":
              var m = l._wrapperState.wasMultiple;
              l._wrapperState.wasMultiple = !!i.multiple;
              var v = i.value;
              v != null ? Pn(l, !!i.multiple, v, !1) : m !== !!i.multiple && (i.defaultValue != null ? Pn(
                l,
                !!i.multiple,
                i.defaultValue,
                !0
              ) : Pn(l, !!i.multiple, i.multiple ? [] : "", !1));
          }
          l[Pr] = i;
        } catch (N) {
          oe(e, e.return, N);
        }
      }
      break;
    case 6:
      if (nt(t, e), ct(e), r & 4) {
        if (e.stateNode === null) throw Error(P(162));
        l = e.stateNode, i = e.memoizedProps;
        try {
          l.nodeValue = i;
        } catch (N) {
          oe(e, e.return, N);
        }
      }
      break;
    case 3:
      if (nt(t, e), ct(e), r & 4 && n !== null && n.memoizedState.isDehydrated) try {
        Sr(t.containerInfo);
      } catch (N) {
        oe(e, e.return, N);
      }
      break;
    case 4:
      nt(t, e), ct(e);
      break;
    case 13:
      nt(t, e), ct(e), l = e.child, l.flags & 8192 && (i = l.memoizedState !== null, l.stateNode.isHidden = i, !i || l.alternate !== null && l.alternate.memoizedState !== null || (yo = ce())), r & 4 && zs(e);
      break;
    case 22:
      if (y = n !== null && n.memoizedState !== null, e.mode & 1 ? (Pe = (d = Pe) || y, nt(t, e), Pe = d) : nt(t, e), ct(e), r & 8192) {
        if (d = e.memoizedState !== null, (e.stateNode.isHidden = d) && !y && e.mode & 1) for (O = e, y = e.child; y !== null; ) {
          for (c = O = y; O !== null; ) {
            switch (m = O, v = m.child, m.tag) {
              case 0:
              case 11:
              case 14:
              case 15:
                mr(4, m, m.return);
                break;
              case 1:
                En(m, m.return);
                var g = m.stateNode;
                if (typeof g.componentWillUnmount == "function") {
                  r = m, n = m.return;
                  try {
                    t = r, g.props = t.memoizedProps, g.state = t.memoizedState, g.componentWillUnmount();
                  } catch (N) {
                    oe(r, n, N);
                  }
                }
                break;
              case 5:
                En(m, m.return);
                break;
              case 22:
                if (m.memoizedState !== null) {
                  Ds(c);
                  continue;
                }
            }
            v !== null ? (v.return = m, O = v) : Ds(c);
          }
          y = y.sibling;
        }
        e: for (y = null, c = e; ; ) {
          if (c.tag === 5) {
            if (y === null) {
              y = c;
              try {
                l = c.stateNode, d ? (i = l.style, typeof i.setProperty == "function" ? i.setProperty("display", "none", "important") : i.display = "none") : (s = c.stateNode, u = c.memoizedProps.style, o = u != null && u.hasOwnProperty("display") ? u.display : null, s.style.display = du("display", o));
              } catch (N) {
                oe(e, e.return, N);
              }
            }
          } else if (c.tag === 6) {
            if (y === null) try {
              c.stateNode.nodeValue = d ? "" : c.memoizedProps;
            } catch (N) {
              oe(e, e.return, N);
            }
          } else if ((c.tag !== 22 && c.tag !== 23 || c.memoizedState === null || c === e) && c.child !== null) {
            c.child.return = c, c = c.child;
            continue;
          }
          if (c === e) break e;
          for (; c.sibling === null; ) {
            if (c.return === null || c.return === e) break e;
            y === c && (y = null), c = c.return;
          }
          y === c && (y = null), c.sibling.return = c.return, c = c.sibling;
        }
      }
      break;
    case 19:
      nt(t, e), ct(e), r & 4 && zs(e);
      break;
    case 21:
      break;
    default:
      nt(
        t,
        e
      ), ct(e);
  }
}
function ct(e) {
  var t = e.flags;
  if (t & 2) {
    try {
      e: {
        for (var n = e.return; n !== null; ) {
          if (Oc(n)) {
            var r = n;
            break e;
          }
          n = n.return;
        }
        throw Error(P(160));
      }
      switch (r.tag) {
        case 5:
          var l = r.stateNode;
          r.flags & 32 && (yr(l, ""), r.flags &= -33);
          var i = Rs(e);
          Ii(e, i, l);
          break;
        case 3:
        case 4:
          var o = r.stateNode.containerInfo, s = Rs(e);
          Ei(e, s, o);
          break;
        default:
          throw Error(P(161));
      }
    } catch (u) {
      oe(e, e.return, u);
    }
    e.flags &= -3;
  }
  t & 4096 && (e.flags &= -4097);
}
function pp(e, t, n) {
  O = e, Uc(e);
}
function Uc(e, t, n) {
  for (var r = (e.mode & 1) !== 0; O !== null; ) {
    var l = O, i = l.child;
    if (l.tag === 22 && r) {
      var o = l.memoizedState !== null || al;
      if (!o) {
        var s = l.alternate, u = s !== null && s.memoizedState !== null || Pe;
        s = al;
        var d = Pe;
        if (al = o, (Pe = u) && !d) for (O = l; O !== null; ) o = O, u = o.child, o.tag === 22 && o.memoizedState !== null ? Ls(l) : u !== null ? (u.return = o, O = u) : Ls(l);
        for (; i !== null; ) O = i, Uc(i), i = i.sibling;
        O = l, al = s, Pe = d;
      }
      Ts(e);
    } else l.subtreeFlags & 8772 && i !== null ? (i.return = l, O = i) : Ts(e);
  }
}
function Ts(e) {
  for (; O !== null; ) {
    var t = O;
    if (t.flags & 8772) {
      var n = t.alternate;
      try {
        if (t.flags & 8772) switch (t.tag) {
          case 0:
          case 11:
          case 15:
            Pe || ea(5, t);
            break;
          case 1:
            var r = t.stateNode;
            if (t.flags & 4 && !Pe) if (n === null) r.componentDidMount();
            else {
              var l = t.elementType === t.type ? n.memoizedProps : rt(t.type, n.memoizedProps);
              r.componentDidUpdate(l, n.memoizedState, r.__reactInternalSnapshotBeforeUpdate);
            }
            var i = t.updateQueue;
            i !== null && gs(t, i, r);
            break;
          case 3:
            var o = t.updateQueue;
            if (o !== null) {
              if (n = null, t.child !== null) switch (t.child.tag) {
                case 5:
                  n = t.child.stateNode;
                  break;
                case 1:
                  n = t.child.stateNode;
              }
              gs(t, o, n);
            }
            break;
          case 5:
            var s = t.stateNode;
            if (n === null && t.flags & 4) {
              n = s;
              var u = t.memoizedProps;
              switch (t.type) {
                case "button":
                case "input":
                case "select":
                case "textarea":
                  u.autoFocus && n.focus();
                  break;
                case "img":
                  u.src && (n.src = u.src);
              }
            }
            break;
          case 6:
            break;
          case 4:
            break;
          case 12:
            break;
          case 13:
            if (t.memoizedState === null) {
              var d = t.alternate;
              if (d !== null) {
                var y = d.memoizedState;
                if (y !== null) {
                  var c = y.dehydrated;
                  c !== null && Sr(c);
                }
              }
            }
            break;
          case 19:
          case 17:
          case 21:
          case 22:
          case 23:
          case 25:
            break;
          default:
            throw Error(P(163));
        }
        Pe || t.flags & 512 && Ci(t);
      } catch (m) {
        oe(t, t.return, m);
      }
    }
    if (t === e) {
      O = null;
      break;
    }
    if (n = t.sibling, n !== null) {
      n.return = t.return, O = n;
      break;
    }
    O = t.return;
  }
}
function Ds(e) {
  for (; O !== null; ) {
    var t = O;
    if (t === e) {
      O = null;
      break;
    }
    var n = t.sibling;
    if (n !== null) {
      n.return = t.return, O = n;
      break;
    }
    O = t.return;
  }
}
function Ls(e) {
  for (; O !== null; ) {
    var t = O;
    try {
      switch (t.tag) {
        case 0:
        case 11:
        case 15:
          var n = t.return;
          try {
            ea(4, t);
          } catch (u) {
            oe(t, n, u);
          }
          break;
        case 1:
          var r = t.stateNode;
          if (typeof r.componentDidMount == "function") {
            var l = t.return;
            try {
              r.componentDidMount();
            } catch (u) {
              oe(t, l, u);
            }
          }
          var i = t.return;
          try {
            Ci(t);
          } catch (u) {
            oe(t, i, u);
          }
          break;
        case 5:
          var o = t.return;
          try {
            Ci(t);
          } catch (u) {
            oe(t, o, u);
          }
      }
    } catch (u) {
      oe(t, t.return, u);
    }
    if (t === e) {
      O = null;
      break;
    }
    var s = t.sibling;
    if (s !== null) {
      s.return = t.return, O = s;
      break;
    }
    O = t.return;
  }
}
var mp = Math.ceil, Ul = Et.ReactCurrentDispatcher, go = Et.ReactCurrentOwner, Je = Et.ReactCurrentBatchConfig, Q = 0, je = null, pe = null, Se = 0, He = 0, In = qt(0), ve = 0, Dr = null, un = 0, ta = 0, xo = 0, hr = null, Me = null, yo = 0, Un = 1 / 0, gt = null, Vl = !1, Pi = null, Bt = null, il = !1, Mt = null, Bl = 0, vr = 0, _i = null, gl = -1, xl = 0;
function Te() {
  return Q & 6 ? ce() : gl !== -1 ? gl : gl = ce();
}
function Ht(e) {
  return e.mode & 1 ? Q & 2 && Se !== 0 ? Se & -Se : Xf.transition !== null ? (xl === 0 && (xl = ku()), xl) : (e = q, e !== 0 || (e = window.event, e = e === void 0 ? 16 : Ru(e.type)), e) : 1;
}
function st(e, t, n, r) {
  if (50 < vr) throw vr = 0, _i = null, Error(P(185));
  Or(e, n, r), (!(Q & 2) || e !== je) && (e === je && (!(Q & 2) && (ta |= n), ve === 4 && Dt(e, Se)), Ue(e, r), n === 1 && Q === 0 && !(t.mode & 1) && (Un = ce() + 500, Xl && bt()));
}
function Ue(e, t) {
  var n = e.callbackNode;
  Yd(e, t);
  var r = Cl(e, e === je ? Se : 0);
  if (r === 0) n !== null && Wo(n), e.callbackNode = null, e.callbackPriority = 0;
  else if (t = r & -r, e.callbackPriority !== t) {
    if (n != null && Wo(n), t === 1) e.tag === 0 ? Yf(Ms.bind(null, e)) : Yu(Ms.bind(null, e)), Gf(function() {
      !(Q & 6) && bt();
    }), n = null;
    else {
      switch (Cu(r)) {
        case 1:
          n = Wi;
          break;
        case 4:
          n = wu;
          break;
        case 16:
          n = kl;
          break;
        case 536870912:
          n = Su;
          break;
        default:
          n = kl;
      }
      n = qc(n, Vc.bind(null, e));
    }
    e.callbackPriority = t, e.callbackNode = n;
  }
}
function Vc(e, t) {
  if (gl = -1, xl = 0, Q & 6) throw Error(P(327));
  var n = e.callbackNode;
  if (Tn() && e.callbackNode !== n) return null;
  var r = Cl(e, e === je ? Se : 0);
  if (r === 0) return null;
  if (r & 30 || r & e.expiredLanes || t) t = Hl(e, r);
  else {
    t = r;
    var l = Q;
    Q |= 2;
    var i = Hc();
    (je !== e || Se !== t) && (gt = null, Un = ce() + 500, rn(e, t));
    do
      try {
        gp();
        break;
      } catch (s) {
        Bc(e, s);
      }
    while (!0);
    ro(), Ul.current = i, Q = l, pe !== null ? t = 0 : (je = null, Se = 0, t = ve);
  }
  if (t !== 0) {
    if (t === 2 && (l = ni(e), l !== 0 && (r = l, t = Fi(e, l))), t === 1) throw n = Dr, rn(e, 0), Dt(e, r), Ue(e, ce()), n;
    if (t === 6) Dt(e, r);
    else {
      if (l = e.current.alternate, !(r & 30) && !hp(l) && (t = Hl(e, r), t === 2 && (i = ni(e), i !== 0 && (r = i, t = Fi(e, i))), t === 1)) throw n = Dr, rn(e, 0), Dt(e, r), Ue(e, ce()), n;
      switch (e.finishedWork = l, e.finishedLanes = r, t) {
        case 0:
        case 1:
          throw Error(P(345));
        case 2:
          Jt(e, Me, gt);
          break;
        case 3:
          if (Dt(e, r), (r & 130023424) === r && (t = yo + 500 - ce(), 10 < t)) {
            if (Cl(e, 0) !== 0) break;
            if (l = e.suspendedLanes, (l & r) !== r) {
              Te(), e.pingedLanes |= e.suspendedLanes & l;
              break;
            }
            e.timeoutHandle = ci(Jt.bind(null, e, Me, gt), t);
            break;
          }
          Jt(e, Me, gt);
          break;
        case 4:
          if (Dt(e, r), (r & 4194240) === r) break;
          for (t = e.eventTimes, l = -1; 0 < r; ) {
            var o = 31 - ot(r);
            i = 1 << o, o = t[o], o > l && (l = o), r &= ~i;
          }
          if (r = l, r = ce() - r, r = (120 > r ? 120 : 480 > r ? 480 : 1080 > r ? 1080 : 1920 > r ? 1920 : 3e3 > r ? 3e3 : 4320 > r ? 4320 : 1960 * mp(r / 1960)) - r, 10 < r) {
            e.timeoutHandle = ci(Jt.bind(null, e, Me, gt), r);
            break;
          }
          Jt(e, Me, gt);
          break;
        case 5:
          Jt(e, Me, gt);
          break;
        default:
          throw Error(P(329));
      }
    }
  }
  return Ue(e, ce()), e.callbackNode === n ? Vc.bind(null, e) : null;
}
function Fi(e, t) {
  var n = hr;
  return e.current.memoizedState.isDehydrated && (rn(e, t).flags |= 256), e = Hl(e, t), e !== 2 && (t = Me, Me = n, t !== null && Ri(t)), e;
}
function Ri(e) {
  Me === null ? Me = e : Me.push.apply(Me, e);
}
function hp(e) {
  for (var t = e; ; ) {
    if (t.flags & 16384) {
      var n = t.updateQueue;
      if (n !== null && (n = n.stores, n !== null)) for (var r = 0; r < n.length; r++) {
        var l = n[r], i = l.getSnapshot;
        l = l.value;
        try {
          if (!ut(i(), l)) return !1;
        } catch {
          return !1;
        }
      }
    }
    if (n = t.child, t.subtreeFlags & 16384 && n !== null) n.return = t, t = n;
    else {
      if (t === e) break;
      for (; t.sibling === null; ) {
        if (t.return === null || t.return === e) return !0;
        t = t.return;
      }
      t.sibling.return = t.return, t = t.sibling;
    }
  }
  return !0;
}
function Dt(e, t) {
  for (t &= ~xo, t &= ~ta, e.suspendedLanes |= t, e.pingedLanes &= ~t, e = e.expirationTimes; 0 < t; ) {
    var n = 31 - ot(t), r = 1 << n;
    e[n] = -1, t &= ~r;
  }
}
function Ms(e) {
  if (Q & 6) throw Error(P(327));
  Tn();
  var t = Cl(e, 0);
  if (!(t & 1)) return Ue(e, ce()), null;
  var n = Hl(e, t);
  if (e.tag !== 0 && n === 2) {
    var r = ni(e);
    r !== 0 && (t = r, n = Fi(e, r));
  }
  if (n === 1) throw n = Dr, rn(e, 0), Dt(e, t), Ue(e, ce()), n;
  if (n === 6) throw Error(P(345));
  return e.finishedWork = e.current.alternate, e.finishedLanes = t, Jt(e, Me, gt), Ue(e, ce()), null;
}
function jo(e, t) {
  var n = Q;
  Q |= 1;
  try {
    return e(t);
  } finally {
    Q = n, Q === 0 && (Un = ce() + 500, Xl && bt());
  }
}
function cn(e) {
  Mt !== null && Mt.tag === 0 && !(Q & 6) && Tn();
  var t = Q;
  Q |= 1;
  var n = Je.transition, r = q;
  try {
    if (Je.transition = null, q = 1, e) return e();
  } finally {
    q = r, Je.transition = n, Q = t, !(Q & 6) && bt();
  }
}
function No() {
  He = In.current, Z(In);
}
function rn(e, t) {
  e.finishedWork = null, e.finishedLanes = 0;
  var n = e.timeoutHandle;
  if (n !== -1 && (e.timeoutHandle = -1, Qf(n)), pe !== null) for (n = pe.return; n !== null; ) {
    var r = n;
    switch (eo(r), r.tag) {
      case 1:
        r = r.type.childContextTypes, r != null && Fl();
        break;
      case 3:
        $n(), Z($e), Z(Fe), uo();
        break;
      case 5:
        so(r);
        break;
      case 4:
        $n();
        break;
      case 13:
        Z(ne);
        break;
      case 19:
        Z(ne);
        break;
      case 10:
        lo(r.type._context);
        break;
      case 22:
      case 23:
        No();
    }
    n = n.return;
  }
  if (je = e, pe = e = Wt(e.current, null), Se = He = t, ve = 0, Dr = null, xo = ta = un = 0, Me = hr = null, tn !== null) {
    for (t = 0; t < tn.length; t++) if (n = tn[t], r = n.interleaved, r !== null) {
      n.interleaved = null;
      var l = r.next, i = n.pending;
      if (i !== null) {
        var o = i.next;
        i.next = l, r.next = o;
      }
      n.pending = r;
    }
    tn = null;
  }
  return e;
}
function Bc(e, t) {
  do {
    var n = pe;
    try {
      if (ro(), ml.current = Al, $l) {
        for (var r = re.memoizedState; r !== null; ) {
          var l = r.queue;
          l !== null && (l.pending = null), r = r.next;
        }
        $l = !1;
      }
      if (sn = 0, xe = he = re = null, pr = !1, Rr = 0, go.current = null, n === null || n.return === null) {
        ve = 1, Dr = t, pe = null;
        break;
      }
      e: {
        var i = e, o = n.return, s = n, u = t;
        if (t = Se, s.flags |= 32768, u !== null && typeof u == "object" && typeof u.then == "function") {
          var d = u, y = s, c = y.tag;
          if (!(y.mode & 1) && (c === 0 || c === 11 || c === 15)) {
            var m = y.alternate;
            m ? (y.updateQueue = m.updateQueue, y.memoizedState = m.memoizedState, y.lanes = m.lanes) : (y.updateQueue = null, y.memoizedState = null);
          }
          var v = Ss(o);
          if (v !== null) {
            v.flags &= -257, ks(v, o, s, i, t), v.mode & 1 && ws(i, d, t), t = v, u = d;
            var g = t.updateQueue;
            if (g === null) {
              var N = /* @__PURE__ */ new Set();
              N.add(u), t.updateQueue = N;
            } else g.add(u);
            break e;
          } else {
            if (!(t & 1)) {
              ws(i, d, t), wo();
              break e;
            }
            u = Error(P(426));
          }
        } else if (ee && s.mode & 1) {
          var F = Ss(o);
          if (F !== null) {
            !(F.flags & 65536) && (F.flags |= 256), ks(F, o, s, i, t), to(An(u, s));
            break e;
          }
        }
        i = u = An(u, s), ve !== 4 && (ve = 2), hr === null ? hr = [i] : hr.push(i), i = o;
        do {
          switch (i.tag) {
            case 3:
              i.flags |= 65536, t &= -t, i.lanes |= t;
              var p = Cc(i, u, t);
              vs(i, p);
              break e;
            case 1:
              s = u;
              var f = i.type, h = i.stateNode;
              if (!(i.flags & 128) && (typeof f.getDerivedStateFromError == "function" || h !== null && typeof h.componentDidCatch == "function" && (Bt === null || !Bt.has(h)))) {
                i.flags |= 65536, t &= -t, i.lanes |= t;
                var k = Ec(i, s, t);
                vs(i, k);
                break e;
              }
          }
          i = i.return;
        } while (i !== null);
      }
      Qc(n);
    } catch (z) {
      t = z, pe === n && n !== null && (pe = n = n.return);
      continue;
    }
    break;
  } while (!0);
}
function Hc() {
  var e = Ul.current;
  return Ul.current = Al, e === null ? Al : e;
}
function wo() {
  (ve === 0 || ve === 3 || ve === 2) && (ve = 4), je === null || !(un & 268435455) && !(ta & 268435455) || Dt(je, Se);
}
function Hl(e, t) {
  var n = Q;
  Q |= 2;
  var r = Hc();
  (je !== e || Se !== t) && (gt = null, rn(e, t));
  do
    try {
      vp();
      break;
    } catch (l) {
      Bc(e, l);
    }
  while (!0);
  if (ro(), Q = n, Ul.current = r, pe !== null) throw Error(P(261));
  return je = null, Se = 0, ve;
}
function vp() {
  for (; pe !== null; ) Wc(pe);
}
function gp() {
  for (; pe !== null && !Vd(); ) Wc(pe);
}
function Wc(e) {
  var t = Kc(e.alternate, e, He);
  e.memoizedProps = e.pendingProps, t === null ? Qc(e) : pe = t, go.current = null;
}
function Qc(e) {
  var t = e;
  do {
    var n = t.alternate;
    if (e = t.return, t.flags & 32768) {
      if (n = cp(n, t), n !== null) {
        n.flags &= 32767, pe = n;
        return;
      }
      if (e !== null) e.flags |= 32768, e.subtreeFlags = 0, e.deletions = null;
      else {
        ve = 6, pe = null;
        return;
      }
    } else if (n = up(n, t, He), n !== null) {
      pe = n;
      return;
    }
    if (t = t.sibling, t !== null) {
      pe = t;
      return;
    }
    pe = t = e;
  } while (t !== null);
  ve === 0 && (ve = 5);
}
function Jt(e, t, n) {
  var r = q, l = Je.transition;
  try {
    Je.transition = null, q = 1, xp(e, t, n, r);
  } finally {
    Je.transition = l, q = r;
  }
  return null;
}
function xp(e, t, n, r) {
  do
    Tn();
  while (Mt !== null);
  if (Q & 6) throw Error(P(327));
  n = e.finishedWork;
  var l = e.finishedLanes;
  if (n === null) return null;
  if (e.finishedWork = null, e.finishedLanes = 0, n === e.current) throw Error(P(177));
  e.callbackNode = null, e.callbackPriority = 0;
  var i = n.lanes | n.childLanes;
  if (Xd(e, i), e === je && (pe = je = null, Se = 0), !(n.subtreeFlags & 2064) && !(n.flags & 2064) || il || (il = !0, qc(kl, function() {
    return Tn(), null;
  })), i = (n.flags & 15990) !== 0, n.subtreeFlags & 15990 || i) {
    i = Je.transition, Je.transition = null;
    var o = q;
    q = 1;
    var s = Q;
    Q |= 4, go.current = null, fp(e, n), Ac(n, e), $f(si), El = !!oi, si = oi = null, e.current = n, pp(n), Bd(), Q = s, q = o, Je.transition = i;
  } else e.current = n;
  if (il && (il = !1, Mt = e, Bl = l), i = e.pendingLanes, i === 0 && (Bt = null), Qd(n.stateNode), Ue(e, ce()), t !== null) for (r = e.onRecoverableError, n = 0; n < t.length; n++) l = t[n], r(l.value, { componentStack: l.stack, digest: l.digest });
  if (Vl) throw Vl = !1, e = Pi, Pi = null, e;
  return Bl & 1 && e.tag !== 0 && Tn(), i = e.pendingLanes, i & 1 ? e === _i ? vr++ : (vr = 0, _i = e) : vr = 0, bt(), null;
}
function Tn() {
  if (Mt !== null) {
    var e = Cu(Bl), t = Je.transition, n = q;
    try {
      if (Je.transition = null, q = 16 > e ? 16 : e, Mt === null) var r = !1;
      else {
        if (e = Mt, Mt = null, Bl = 0, Q & 6) throw Error(P(331));
        var l = Q;
        for (Q |= 4, O = e.current; O !== null; ) {
          var i = O, o = i.child;
          if (O.flags & 16) {
            var s = i.deletions;
            if (s !== null) {
              for (var u = 0; u < s.length; u++) {
                var d = s[u];
                for (O = d; O !== null; ) {
                  var y = O;
                  switch (y.tag) {
                    case 0:
                    case 11:
                    case 15:
                      mr(8, y, i);
                  }
                  var c = y.child;
                  if (c !== null) c.return = y, O = c;
                  else for (; O !== null; ) {
                    y = O;
                    var m = y.sibling, v = y.return;
                    if (Mc(y), y === d) {
                      O = null;
                      break;
                    }
                    if (m !== null) {
                      m.return = v, O = m;
                      break;
                    }
                    O = v;
                  }
                }
              }
              var g = i.alternate;
              if (g !== null) {
                var N = g.child;
                if (N !== null) {
                  g.child = null;
                  do {
                    var F = N.sibling;
                    N.sibling = null, N = F;
                  } while (N !== null);
                }
              }
              O = i;
            }
          }
          if (i.subtreeFlags & 2064 && o !== null) o.return = i, O = o;
          else e: for (; O !== null; ) {
            if (i = O, i.flags & 2048) switch (i.tag) {
              case 0:
              case 11:
              case 15:
                mr(9, i, i.return);
            }
            var p = i.sibling;
            if (p !== null) {
              p.return = i.return, O = p;
              break e;
            }
            O = i.return;
          }
        }
        var f = e.current;
        for (O = f; O !== null; ) {
          o = O;
          var h = o.child;
          if (o.subtreeFlags & 2064 && h !== null) h.return = o, O = h;
          else e: for (o = f; O !== null; ) {
            if (s = O, s.flags & 2048) try {
              switch (s.tag) {
                case 0:
                case 11:
                case 15:
                  ea(9, s);
              }
            } catch (z) {
              oe(s, s.return, z);
            }
            if (s === o) {
              O = null;
              break e;
            }
            var k = s.sibling;
            if (k !== null) {
              k.return = s.return, O = k;
              break e;
            }
            O = s.return;
          }
        }
        if (Q = l, bt(), pt && typeof pt.onPostCommitFiberRoot == "function") try {
          pt.onPostCommitFiberRoot(Gl, e);
        } catch {
        }
        r = !0;
      }
      return r;
    } finally {
      q = n, Je.transition = t;
    }
  }
  return !1;
}
function Os(e, t, n) {
  t = An(n, t), t = Cc(e, t, 1), e = Vt(e, t, 1), t = Te(), e !== null && (Or(e, 1, t), Ue(e, t));
}
function oe(e, t, n) {
  if (e.tag === 3) Os(e, e, n);
  else for (; t !== null; ) {
    if (t.tag === 3) {
      Os(t, e, n);
      break;
    } else if (t.tag === 1) {
      var r = t.stateNode;
      if (typeof t.type.getDerivedStateFromError == "function" || typeof r.componentDidCatch == "function" && (Bt === null || !Bt.has(r))) {
        e = An(n, e), e = Ec(t, e, 1), t = Vt(t, e, 1), e = Te(), t !== null && (Or(t, 1, e), Ue(t, e));
        break;
      }
    }
    t = t.return;
  }
}
function yp(e, t, n) {
  var r = e.pingCache;
  r !== null && r.delete(t), t = Te(), e.pingedLanes |= e.suspendedLanes & n, je === e && (Se & n) === n && (ve === 4 || ve === 3 && (Se & 130023424) === Se && 500 > ce() - yo ? rn(e, 0) : xo |= n), Ue(e, t);
}
function Gc(e, t) {
  t === 0 && (e.mode & 1 ? (t = Yr, Yr <<= 1, !(Yr & 130023424) && (Yr = 4194304)) : t = 1);
  var n = Te();
  e = kt(e, t), e !== null && (Or(e, t, n), Ue(e, n));
}
function jp(e) {
  var t = e.memoizedState, n = 0;
  t !== null && (n = t.retryLane), Gc(e, n);
}
function Np(e, t) {
  var n = 0;
  switch (e.tag) {
    case 13:
      var r = e.stateNode, l = e.memoizedState;
      l !== null && (n = l.retryLane);
      break;
    case 19:
      r = e.stateNode;
      break;
    default:
      throw Error(P(314));
  }
  r !== null && r.delete(t), Gc(e, n);
}
var Kc;
Kc = function(e, t, n) {
  if (e !== null) if (e.memoizedProps !== t.pendingProps || $e.current) Oe = !0;
  else {
    if (!(e.lanes & n) && !(t.flags & 128)) return Oe = !1, sp(e, t, n);
    Oe = !!(e.flags & 131072);
  }
  else Oe = !1, ee && t.flags & 1048576 && Xu(t, Tl, t.index);
  switch (t.lanes = 0, t.tag) {
    case 2:
      var r = t.type;
      vl(e, t), e = t.pendingProps;
      var l = Ln(t, Fe.current);
      zn(t, n), l = fo(null, t, r, e, l, n);
      var i = po();
      return t.flags |= 1, typeof l == "object" && l !== null && typeof l.render == "function" && l.$$typeof === void 0 ? (t.tag = 1, t.memoizedState = null, t.updateQueue = null, Ae(r) ? (i = !0, Rl(t)) : i = !1, t.memoizedState = l.state !== null && l.state !== void 0 ? l.state : null, io(t), l.updater = Jl, t.stateNode = l, l._reactInternals = t, gi(t, r, e, n), t = ji(null, t, r, !0, i, n)) : (t.tag = 0, ee && i && Ji(t), Re(null, t, l, n), t = t.child), t;
    case 16:
      r = t.elementType;
      e: {
        switch (vl(e, t), e = t.pendingProps, l = r._init, r = l(r._payload), t.type = r, l = t.tag = Sp(r), e = rt(r, e), l) {
          case 0:
            t = yi(null, t, r, e, n);
            break e;
          case 1:
            t = Is(null, t, r, e, n);
            break e;
          case 11:
            t = Cs(null, t, r, e, n);
            break e;
          case 14:
            t = Es(null, t, r, rt(r.type, e), n);
            break e;
        }
        throw Error(P(
          306,
          r,
          ""
        ));
      }
      return t;
    case 0:
      return r = t.type, l = t.pendingProps, l = t.elementType === r ? l : rt(r, l), yi(e, t, r, l, n);
    case 1:
      return r = t.type, l = t.pendingProps, l = t.elementType === r ? l : rt(r, l), Is(e, t, r, l, n);
    case 3:
      e: {
        if (Fc(t), e === null) throw Error(P(387));
        r = t.pendingProps, i = t.memoizedState, l = i.element, rc(e, t), Ml(t, r, null, n);
        var o = t.memoizedState;
        if (r = o.element, i.isDehydrated) if (i = { element: r, isDehydrated: !1, cache: o.cache, pendingSuspenseBoundaries: o.pendingSuspenseBoundaries, transitions: o.transitions }, t.updateQueue.baseState = i, t.memoizedState = i, t.flags & 256) {
          l = An(Error(P(423)), t), t = Ps(e, t, r, n, l);
          break e;
        } else if (r !== l) {
          l = An(Error(P(424)), t), t = Ps(e, t, r, n, l);
          break e;
        } else for (We = Ut(t.stateNode.containerInfo.firstChild), Qe = t, ee = !0, at = null, n = tc(t, null, r, n), t.child = n; n; ) n.flags = n.flags & -3 | 4096, n = n.sibling;
        else {
          if (Mn(), r === l) {
            t = Ct(e, t, n);
            break e;
          }
          Re(e, t, r, n);
        }
        t = t.child;
      }
      return t;
    case 5:
      return lc(t), e === null && mi(t), r = t.type, l = t.pendingProps, i = e !== null ? e.memoizedProps : null, o = l.children, ui(r, l) ? o = null : i !== null && ui(r, i) && (t.flags |= 32), _c(e, t), Re(e, t, o, n), t.child;
    case 6:
      return e === null && mi(t), null;
    case 13:
      return Rc(e, t, n);
    case 4:
      return oo(t, t.stateNode.containerInfo), r = t.pendingProps, e === null ? t.child = On(t, null, r, n) : Re(e, t, r, n), t.child;
    case 11:
      return r = t.type, l = t.pendingProps, l = t.elementType === r ? l : rt(r, l), Cs(e, t, r, l, n);
    case 7:
      return Re(e, t, t.pendingProps, n), t.child;
    case 8:
      return Re(e, t, t.pendingProps.children, n), t.child;
    case 12:
      return Re(e, t, t.pendingProps.children, n), t.child;
    case 10:
      e: {
        if (r = t.type._context, l = t.pendingProps, i = t.memoizedProps, o = l.value, Y(Dl, r._currentValue), r._currentValue = o, i !== null) if (ut(i.value, o)) {
          if (i.children === l.children && !$e.current) {
            t = Ct(e, t, n);
            break e;
          }
        } else for (i = t.child, i !== null && (i.return = t); i !== null; ) {
          var s = i.dependencies;
          if (s !== null) {
            o = i.child;
            for (var u = s.firstContext; u !== null; ) {
              if (u.context === r) {
                if (i.tag === 1) {
                  u = Nt(-1, n & -n), u.tag = 2;
                  var d = i.updateQueue;
                  if (d !== null) {
                    d = d.shared;
                    var y = d.pending;
                    y === null ? u.next = u : (u.next = y.next, y.next = u), d.pending = u;
                  }
                }
                i.lanes |= n, u = i.alternate, u !== null && (u.lanes |= n), hi(
                  i.return,
                  n,
                  t
                ), s.lanes |= n;
                break;
              }
              u = u.next;
            }
          } else if (i.tag === 10) o = i.type === t.type ? null : i.child;
          else if (i.tag === 18) {
            if (o = i.return, o === null) throw Error(P(341));
            o.lanes |= n, s = o.alternate, s !== null && (s.lanes |= n), hi(o, n, t), o = i.sibling;
          } else o = i.child;
          if (o !== null) o.return = i;
          else for (o = i; o !== null; ) {
            if (o === t) {
              o = null;
              break;
            }
            if (i = o.sibling, i !== null) {
              i.return = o.return, o = i;
              break;
            }
            o = o.return;
          }
          i = o;
        }
        Re(e, t, l.children, n), t = t.child;
      }
      return t;
    case 9:
      return l = t.type, r = t.pendingProps.children, zn(t, n), l = et(l), r = r(l), t.flags |= 1, Re(e, t, r, n), t.child;
    case 14:
      return r = t.type, l = rt(r, t.pendingProps), l = rt(r.type, l), Es(e, t, r, l, n);
    case 15:
      return Ic(e, t, t.type, t.pendingProps, n);
    case 17:
      return r = t.type, l = t.pendingProps, l = t.elementType === r ? l : rt(r, l), vl(e, t), t.tag = 1, Ae(r) ? (e = !0, Rl(t)) : e = !1, zn(t, n), kc(t, r, l), gi(t, r, l, n), ji(null, t, r, !0, e, n);
    case 19:
      return zc(e, t, n);
    case 22:
      return Pc(e, t, n);
  }
  throw Error(P(156, t.tag));
};
function qc(e, t) {
  return Nu(e, t);
}
function wp(e, t, n, r) {
  this.tag = e, this.key = n, this.sibling = this.child = this.return = this.stateNode = this.type = this.elementType = null, this.index = 0, this.ref = null, this.pendingProps = t, this.dependencies = this.memoizedState = this.updateQueue = this.memoizedProps = null, this.mode = r, this.subtreeFlags = this.flags = 0, this.deletions = null, this.childLanes = this.lanes = 0, this.alternate = null;
}
function Ze(e, t, n, r) {
  return new wp(e, t, n, r);
}
function So(e) {
  return e = e.prototype, !(!e || !e.isReactComponent);
}
function Sp(e) {
  if (typeof e == "function") return So(e) ? 1 : 0;
  if (e != null) {
    if (e = e.$$typeof, e === Vi) return 11;
    if (e === Bi) return 14;
  }
  return 2;
}
function Wt(e, t) {
  var n = e.alternate;
  return n === null ? (n = Ze(e.tag, t, e.key, e.mode), n.elementType = e.elementType, n.type = e.type, n.stateNode = e.stateNode, n.alternate = e, e.alternate = n) : (n.pendingProps = t, n.type = e.type, n.flags = 0, n.subtreeFlags = 0, n.deletions = null), n.flags = e.flags & 14680064, n.childLanes = e.childLanes, n.lanes = e.lanes, n.child = e.child, n.memoizedProps = e.memoizedProps, n.memoizedState = e.memoizedState, n.updateQueue = e.updateQueue, t = e.dependencies, n.dependencies = t === null ? null : { lanes: t.lanes, firstContext: t.firstContext }, n.sibling = e.sibling, n.index = e.index, n.ref = e.ref, n;
}
function yl(e, t, n, r, l, i) {
  var o = 2;
  if (r = e, typeof e == "function") So(e) && (o = 1);
  else if (typeof e == "string") o = 5;
  else e: switch (e) {
    case gn:
      return ln(n.children, l, i, t);
    case Ui:
      o = 8, l |= 8;
      break;
    case Va:
      return e = Ze(12, n, t, l | 2), e.elementType = Va, e.lanes = i, e;
    case Ba:
      return e = Ze(13, n, t, l), e.elementType = Ba, e.lanes = i, e;
    case Ha:
      return e = Ze(19, n, t, l), e.elementType = Ha, e.lanes = i, e;
    case lu:
      return na(n, l, i, t);
    default:
      if (typeof e == "object" && e !== null) switch (e.$$typeof) {
        case nu:
          o = 10;
          break e;
        case ru:
          o = 9;
          break e;
        case Vi:
          o = 11;
          break e;
        case Bi:
          o = 14;
          break e;
        case Rt:
          o = 16, r = null;
          break e;
      }
      throw Error(P(130, e == null ? e : typeof e, ""));
  }
  return t = Ze(o, n, t, l), t.elementType = e, t.type = r, t.lanes = i, t;
}
function ln(e, t, n, r) {
  return e = Ze(7, e, r, t), e.lanes = n, e;
}
function na(e, t, n, r) {
  return e = Ze(22, e, r, t), e.elementType = lu, e.lanes = n, e.stateNode = { isHidden: !1 }, e;
}
function Oa(e, t, n) {
  return e = Ze(6, e, null, t), e.lanes = n, e;
}
function $a(e, t, n) {
  return t = Ze(4, e.children !== null ? e.children : [], e.key, t), t.lanes = n, t.stateNode = { containerInfo: e.containerInfo, pendingChildren: null, implementation: e.implementation }, t;
}
function kp(e, t, n, r, l) {
  this.tag = t, this.containerInfo = e, this.finishedWork = this.pingCache = this.current = this.pendingChildren = null, this.timeoutHandle = -1, this.callbackNode = this.pendingContext = this.context = null, this.callbackPriority = 0, this.eventTimes = xa(0), this.expirationTimes = xa(-1), this.entangledLanes = this.finishedLanes = this.mutableReadLanes = this.expiredLanes = this.pingedLanes = this.suspendedLanes = this.pendingLanes = 0, this.entanglements = xa(0), this.identifierPrefix = r, this.onRecoverableError = l, this.mutableSourceEagerHydrationData = null;
}
function ko(e, t, n, r, l, i, o, s, u) {
  return e = new kp(e, t, n, s, u), t === 1 ? (t = 1, i === !0 && (t |= 8)) : t = 0, i = Ze(3, null, null, t), e.current = i, i.stateNode = e, i.memoizedState = { element: r, isDehydrated: n, cache: null, transitions: null, pendingSuspenseBoundaries: null }, io(i), e;
}
function Cp(e, t, n) {
  var r = 3 < arguments.length && arguments[3] !== void 0 ? arguments[3] : null;
  return { $$typeof: vn, key: r == null ? null : "" + r, children: e, containerInfo: t, implementation: n };
}
function bc(e) {
  if (!e) return Gt;
  e = e._reactInternals;
  e: {
    if (pn(e) !== e || e.tag !== 1) throw Error(P(170));
    var t = e;
    do {
      switch (t.tag) {
        case 3:
          t = t.stateNode.context;
          break e;
        case 1:
          if (Ae(t.type)) {
            t = t.stateNode.__reactInternalMemoizedMergedChildContext;
            break e;
          }
      }
      t = t.return;
    } while (t !== null);
    throw Error(P(171));
  }
  if (e.tag === 1) {
    var n = e.type;
    if (Ae(n)) return bu(e, n, t);
  }
  return t;
}
function Yc(e, t, n, r, l, i, o, s, u) {
  return e = ko(n, r, !0, e, l, i, o, s, u), e.context = bc(null), n = e.current, r = Te(), l = Ht(n), i = Nt(r, l), i.callback = t ?? null, Vt(n, i, l), e.current.lanes = l, Or(e, l, r), Ue(e, r), e;
}
function ra(e, t, n, r) {
  var l = t.current, i = Te(), o = Ht(l);
  return n = bc(n), t.context === null ? t.context = n : t.pendingContext = n, t = Nt(i, o), t.payload = { element: e }, r = r === void 0 ? null : r, r !== null && (t.callback = r), e = Vt(l, t, o), e !== null && (st(e, l, o, i), pl(e, l, o)), o;
}
function Wl(e) {
  if (e = e.current, !e.child) return null;
  switch (e.child.tag) {
    case 5:
      return e.child.stateNode;
    default:
      return e.child.stateNode;
  }
}
function $s(e, t) {
  if (e = e.memoizedState, e !== null && e.dehydrated !== null) {
    var n = e.retryLane;
    e.retryLane = n !== 0 && n < t ? n : t;
  }
}
function Co(e, t) {
  $s(e, t), (e = e.alternate) && $s(e, t);
}
function Ep() {
  return null;
}
var Xc = typeof reportError == "function" ? reportError : function(e) {
  console.error(e);
};
function Eo(e) {
  this._internalRoot = e;
}
la.prototype.render = Eo.prototype.render = function(e) {
  var t = this._internalRoot;
  if (t === null) throw Error(P(409));
  ra(e, t, null, null);
};
la.prototype.unmount = Eo.prototype.unmount = function() {
  var e = this._internalRoot;
  if (e !== null) {
    this._internalRoot = null;
    var t = e.containerInfo;
    cn(function() {
      ra(null, e, null, null);
    }), t[St] = null;
  }
};
function la(e) {
  this._internalRoot = e;
}
la.prototype.unstable_scheduleHydration = function(e) {
  if (e) {
    var t = Pu();
    e = { blockedOn: null, target: e, priority: t };
    for (var n = 0; n < Tt.length && t !== 0 && t < Tt[n].priority; n++) ;
    Tt.splice(n, 0, e), n === 0 && Fu(e);
  }
};
function Io(e) {
  return !(!e || e.nodeType !== 1 && e.nodeType !== 9 && e.nodeType !== 11);
}
function aa(e) {
  return !(!e || e.nodeType !== 1 && e.nodeType !== 9 && e.nodeType !== 11 && (e.nodeType !== 8 || e.nodeValue !== " react-mount-point-unstable "));
}
function As() {
}
function Ip(e, t, n, r, l) {
  if (l) {
    if (typeof r == "function") {
      var i = r;
      r = function() {
        var d = Wl(o);
        i.call(d);
      };
    }
    var o = Yc(t, r, e, 0, null, !1, !1, "", As);
    return e._reactRootContainer = o, e[St] = o.current, Er(e.nodeType === 8 ? e.parentNode : e), cn(), o;
  }
  for (; l = e.lastChild; ) e.removeChild(l);
  if (typeof r == "function") {
    var s = r;
    r = function() {
      var d = Wl(u);
      s.call(d);
    };
  }
  var u = ko(e, 0, !1, null, null, !1, !1, "", As);
  return e._reactRootContainer = u, e[St] = u.current, Er(e.nodeType === 8 ? e.parentNode : e), cn(function() {
    ra(t, u, n, r);
  }), u;
}
function ia(e, t, n, r, l) {
  var i = n._reactRootContainer;
  if (i) {
    var o = i;
    if (typeof l == "function") {
      var s = l;
      l = function() {
        var u = Wl(o);
        s.call(u);
      };
    }
    ra(t, o, e, l);
  } else o = Ip(n, t, e, l, r);
  return Wl(o);
}
Eu = function(e) {
  switch (e.tag) {
    case 3:
      var t = e.stateNode;
      if (t.current.memoizedState.isDehydrated) {
        var n = ir(t.pendingLanes);
        n !== 0 && (Qi(t, n | 1), Ue(t, ce()), !(Q & 6) && (Un = ce() + 500, bt()));
      }
      break;
    case 13:
      cn(function() {
        var r = kt(e, 1);
        if (r !== null) {
          var l = Te();
          st(r, e, 1, l);
        }
      }), Co(e, 1);
  }
};
Gi = function(e) {
  if (e.tag === 13) {
    var t = kt(e, 134217728);
    if (t !== null) {
      var n = Te();
      st(t, e, 134217728, n);
    }
    Co(e, 134217728);
  }
};
Iu = function(e) {
  if (e.tag === 13) {
    var t = Ht(e), n = kt(e, t);
    if (n !== null) {
      var r = Te();
      st(n, e, t, r);
    }
    Co(e, t);
  }
};
Pu = function() {
  return q;
};
_u = function(e, t) {
  var n = q;
  try {
    return q = e, t();
  } finally {
    q = n;
  }
};
Ja = function(e, t, n) {
  switch (t) {
    case "input":
      if (Ga(e, n), t = n.name, n.type === "radio" && t != null) {
        for (n = e; n.parentNode; ) n = n.parentNode;
        for (n = n.querySelectorAll("input[name=" + JSON.stringify("" + t) + '][type="radio"]'), t = 0; t < n.length; t++) {
          var r = n[t];
          if (r !== e && r.form === e.form) {
            var l = Yl(r);
            if (!l) throw Error(P(90));
            iu(r), Ga(r, l);
          }
        }
      }
      break;
    case "textarea":
      su(e, n);
      break;
    case "select":
      t = n.value, t != null && Pn(e, !!n.multiple, t, !1);
  }
};
hu = jo;
vu = cn;
var Pp = { usingClientEntryPoint: !1, Events: [Ar, Nn, Yl, pu, mu, jo] }, rr = { findFiberByHostInstance: en, bundleType: 0, version: "18.3.1", rendererPackageName: "react-dom" }, _p = { bundleType: rr.bundleType, version: rr.version, rendererPackageName: rr.rendererPackageName, rendererConfig: rr.rendererConfig, overrideHookState: null, overrideHookStateDeletePath: null, overrideHookStateRenamePath: null, overrideProps: null, overridePropsDeletePath: null, overridePropsRenamePath: null, setErrorHandler: null, setSuspenseHandler: null, scheduleUpdate: null, currentDispatcherRef: Et.ReactCurrentDispatcher, findHostInstanceByFiber: function(e) {
  return e = yu(e), e === null ? null : e.stateNode;
}, findFiberByHostInstance: rr.findFiberByHostInstance || Ep, findHostInstancesForRefresh: null, scheduleRefresh: null, scheduleRoot: null, setRefreshHandler: null, getCurrentFiber: null, reconcilerVersion: "18.3.1-next-f1338f8080-20240426" };
if (typeof __REACT_DEVTOOLS_GLOBAL_HOOK__ < "u") {
  var ol = __REACT_DEVTOOLS_GLOBAL_HOOK__;
  if (!ol.isDisabled && ol.supportsFiber) try {
    Gl = ol.inject(_p), pt = ol;
  } catch {
  }
}
Ke.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED = Pp;
Ke.createPortal = function(e, t) {
  var n = 2 < arguments.length && arguments[2] !== void 0 ? arguments[2] : null;
  if (!Io(t)) throw Error(P(200));
  return Cp(e, t, null, n);
};
Ke.createRoot = function(e, t) {
  if (!Io(e)) throw Error(P(299));
  var n = !1, r = "", l = Xc;
  return t != null && (t.unstable_strictMode === !0 && (n = !0), t.identifierPrefix !== void 0 && (r = t.identifierPrefix), t.onRecoverableError !== void 0 && (l = t.onRecoverableError)), t = ko(e, 1, !1, null, null, n, !1, r, l), e[St] = t.current, Er(e.nodeType === 8 ? e.parentNode : e), new Eo(t);
};
Ke.findDOMNode = function(e) {
  if (e == null) return null;
  if (e.nodeType === 1) return e;
  var t = e._reactInternals;
  if (t === void 0)
    throw typeof e.render == "function" ? Error(P(188)) : (e = Object.keys(e).join(","), Error(P(268, e)));
  return e = yu(t), e = e === null ? null : e.stateNode, e;
};
Ke.flushSync = function(e) {
  return cn(e);
};
Ke.hydrate = function(e, t, n) {
  if (!aa(t)) throw Error(P(200));
  return ia(null, e, t, !0, n);
};
Ke.hydrateRoot = function(e, t, n) {
  if (!Io(e)) throw Error(P(405));
  var r = n != null && n.hydratedSources || null, l = !1, i = "", o = Xc;
  if (n != null && (n.unstable_strictMode === !0 && (l = !0), n.identifierPrefix !== void 0 && (i = n.identifierPrefix), n.onRecoverableError !== void 0 && (o = n.onRecoverableError)), t = Yc(t, null, e, 1, n ?? null, l, !1, i, o), e[St] = t.current, Er(e), r) for (e = 0; e < r.length; e++) n = r[e], l = n._getVersion, l = l(n._source), t.mutableSourceEagerHydrationData == null ? t.mutableSourceEagerHydrationData = [n, l] : t.mutableSourceEagerHydrationData.push(
    n,
    l
  );
  return new la(t);
};
Ke.render = function(e, t, n) {
  if (!aa(t)) throw Error(P(200));
  return ia(null, e, t, !1, n);
};
Ke.unmountComponentAtNode = function(e) {
  if (!aa(e)) throw Error(P(40));
  return e._reactRootContainer ? (cn(function() {
    ia(null, null, e, !1, function() {
      e._reactRootContainer = null, e[St] = null;
    });
  }), !0) : !1;
};
Ke.unstable_batchedUpdates = jo;
Ke.unstable_renderSubtreeIntoContainer = function(e, t, n, r) {
  if (!aa(n)) throw Error(P(200));
  if (e == null || e._reactInternals === void 0) throw Error(P(38));
  return ia(e, t, n, !1, r);
};
Ke.version = "18.3.1-next-f1338f8080-20240426";
function Zc() {
  if (!(typeof __REACT_DEVTOOLS_GLOBAL_HOOK__ > "u" || typeof __REACT_DEVTOOLS_GLOBAL_HOOK__.checkDCE != "function"))
    try {
      __REACT_DEVTOOLS_GLOBAL_HOOK__.checkDCE(Zc);
    } catch (e) {
      console.error(e);
    }
}
Zc(), Zs.exports = Ke;
var Fp = Zs.exports, Jc, Us = Fp;
Jc = Us.createRoot, Us.hydrateRoot;
class Rp extends Error {
  constructor(t, n) {
    super(t), this.status = n, this.name = "ApiError";
  }
}
function zp(e, t) {
  async function n(r, l = {}) {
    const i = { ...l.headers ?? {} }, o = t();
    o && (i.Authorization = "Bearer " + o);
    let s;
    l.body !== void 0 && (i["Content-Type"] = "application/json", s = JSON.stringify(l.body));
    const u = await e(r, { method: l.method ?? "GET", headers: i, body: s });
    if (!u.ok) {
      let y = `HTTP ${u.status}`;
      try {
        const c = await u.json();
        y = c.detail || c.title || y;
      } catch {
      }
      throw new Rp(y, u.status);
    }
    return u.status === 204 ? void 0 : (u.headers.get("content-type") ?? "").includes("json") ? await u.json() : await u.text();
  }
  return {
    get: (r) => n(r),
    post: (r, l) => n(r, { method: "POST", body: l }),
    put: (r, l) => n(r, { method: "PUT", body: l }),
    del: (r) => n(r, { method: "DELETE" })
  };
}
const ed = j.createContext(null);
function vt() {
  const e = j.useContext(ed);
  if (!e) throw new Error("Fuera del módulo de documentos");
  return e;
}
function Tp(e) {
  return zp((t, n) => fetch(t, n), e.token);
}
async function zi(e, t) {
  const n = e.token(), r = await fetch(t, { headers: n ? { Authorization: "Bearer " + n } : {} });
  if (!r.ok) throw new Error("No se pudo generar el documento");
  const l = URL.createObjectURL(await r.blob());
  window.open(l, "_blank"), setTimeout(() => URL.revokeObjectURL(l), 6e4);
}
const td = new Intl.NumberFormat("es-ES", { minimumFractionDigits: 2, maximumFractionDigits: 2 }), Dp = new Intl.NumberFormat("es-ES", { maximumFractionDigits: 3 }), M = (e) => `${td.format(Number(e) || 0)} €`, ye = (e) => td.format(Number(e) || 0), ze = (e) => Dp.format(Number(e) || 0), Ve = (e) => {
  if (!e) return "";
  const t = String(e).slice(0, 10).split("-");
  return t.length === 3 ? `${t[2]}/${t[1]}/${t[0]}` : String(e);
}, ht = () => (/* @__PURE__ */ new Date()).toISOString().slice(0, 10), jl = (e) => Math.round((e + Number.EPSILON) * 100) / 100;
let Lp = 0;
const Vr = () => `l${Date.now().toString(36)}${(++Lp).toString(36)}`;
function Br(e, t) {
  const [n, r] = j.useState(e);
  return j.useEffect(() => {
    const l = setTimeout(() => r(e), t);
    return () => clearTimeout(l);
  }, [e, t]), n;
}
function oa() {
  const e = j.useRef(0);
  return () => {
    const t = ++e.current;
    return () => t === e.current;
  };
}
function Po(e) {
  switch (e) {
    case "Emitida":
    case "Aceptado":
    case "Facturado":
    case "Servido":
    case "Recibido":
      return "pill ok";
    case "Anulada":
    case "Rechazado":
    case "Cancelado":
      return "pill neg";
    case "Borrador":
    case "Rectificada":
      return "pill wait";
    default:
      return "pill part";
  }
}
const Mp = { presupuesto: "Presupuestos", pedido: "Pedidos de venta", factura: "Facturas emitidas", compra: "Pedidos de compra", gasto: "Facturas de proveedor y gastos" }, Op = { presupuesto: "+ Nuevo presupuesto", pedido: "+ Nuevo pedido", factura: "+ Nueva factura", compra: "+ Nuevo pedido", gasto: "+ Registrar factura" }, $p = {
  presupuesto: ["Borrador", "Aceptado", "Rechazado"],
  pedido: ["Borrador", "Confirmado", "ServidoParcial", "Servido", "Facturado", "Cancelado"],
  factura: ["Emitida", "Rectificada", "Anulada"],
  compra: ["Borrador", "Confirmado", "RecibidoParcial", "Recibido", "Facturado", "Cancelado"],
  gasto: ["Registrado", "Anulado"]
};
function Ap(e) {
  const { api: t, navegar: n } = vt(), [r, l] = j.useState(""), [i, o] = j.useState(""), [s, u] = j.useState(""), [d, y] = j.useState(""), [c, m] = j.useState(1), [v, g] = j.useState(null), [N, F] = j.useState(0), [p, f] = j.useState(""), h = Br(r, 250), k = 50;
  j.useEffect(() => m(1), [h, i, s, d, e.tipo]), j.useEffect(() => {
    f(""), (async () => {
      switch (e.tipo) {
        case "factura": {
          const E = new URLSearchParams({ pagina: String(c), tamanoPagina: String(k) });
          h.trim() && E.set("texto", h.trim()), i && E.set("estado", i), s && E.set("desde", s), d && E.set("hasta", d);
          const _ = await t.get(`/facturas/buscar?${E}`);
          return F(_.total), _.elementos.map(($) => ({ id: $.id, numero: $.numeroCompleto, fecha: $.fechaEmision, tercero: $.clienteNombre + ($.clienteNif ? ` · ${$.clienteNif}` : ""), total: $.total, estado: $.estado, extra: $.tipo !== "Ordinaria" ? $.tipo : void 0 }));
        }
        case "gasto": {
          const E = new URLSearchParams({ pagina: String(c), tamanoPagina: String(k) });
          h.trim() && E.set("texto", h.trim()), i && E.set("estado", i), s && E.set("desde", s), d && E.set("hasta", d);
          const _ = await t.get(`/gastos/buscar?${E}`);
          return F(_.total), _.elementos.map(($) => ({ id: $.id, numero: $.numeroFactura ?? "—", fecha: $.fecha, tercero: `${$.proveedorTexto ?? ""}${$.numeroFactura ? "" : ` · ${$.concepto}`}`, total: $.total, estado: $.estado === "Anulado" ? "Anulada" : $.estado, extra: $.esRectificativa ? "Rectificativa" : void 0 }));
        }
        case "presupuesto":
          return (await t.get("/presupuestos")).map((E) => ({ id: E.id, numero: E.numeroCompleto, fecha: E.fecha, tercero: E.clienteNombre, total: E.total, estado: E.estado }));
        case "pedido":
          return (await t.get("/pedidos-venta")).map((E) => ({ id: E.id, numero: E.numeroCompleto, fecha: E.fecha, tercero: E.clienteNombre, total: E.total, estado: E.estado }));
        case "compra":
          return (await t.get("/compras/pedidos")).map((E) => ({ id: E.id, numero: E.numeroCompleto, fecha: E.fecha, tercero: E.proveedorTexto, total: E.total, estado: E.estado, extra: E.empresaOrigenId ? "Intragrupo" : void 0 }));
      }
    })().then(g).catch((E) => (f(E.message), g([])));
  }, [t, e.tipo, c, h, i, s, d]);
  const z = j.useMemo(() => {
    if (!v || e.tipo === "factura" || e.tipo === "gasto") return v ?? [];
    const L = h.trim().toLowerCase();
    return v.filter((E) => (!L || E.numero.toLowerCase().includes(L) || E.tercero.toLowerCase().includes(L)) && (!i || E.estado === i) && (!s || E.fecha >= s) && (!d || E.fecha <= d));
  }, [v, h, i, s, d, e.tipo]), R = z.reduce((L, E) => L + (E.estado === "Anulada" || E.estado === "Cancelado" ? 0 : E.total), 0), T = e.tipo === "factura" || e.tipo === "gasto", D = T ? Math.max(1, Math.ceil(N / k)) : 1;
  return /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
    /* @__PURE__ */ a.jsxs("div", { className: "panel-head", children: [
      /* @__PURE__ */ a.jsx("h2", { children: Mp[e.tipo] }),
      /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: () => n({ tipo: e.tipo, pantalla: "editor" }), children: Op[e.tipo] })
    ] }),
    /* @__PURE__ */ a.jsxs("div", { className: "dx-filtros", children: [
      /* @__PURE__ */ a.jsx("input", { placeholder: e.tipo === "compra" || e.tipo === "gasto" ? "Número, proveedor o concepto…" : "Número, cliente o NIF…", value: r, onChange: (L) => l(L.target.value), autoFocus: !0 }),
      /* @__PURE__ */ a.jsxs("select", { value: i, onChange: (L) => o(L.target.value), children: [
        /* @__PURE__ */ a.jsx("option", { value: "", children: "Todos los estados" }),
        $p[e.tipo].map((L) => /* @__PURE__ */ a.jsx("option", { value: L, children: L }, L))
      ] }),
      /* @__PURE__ */ a.jsx("input", { type: "date", value: s, onChange: (L) => u(L.target.value), title: "Desde" }),
      /* @__PURE__ */ a.jsx("input", { type: "date", value: d, onChange: (L) => y(L.target.value), title: "Hasta" })
    ] }),
    p && /* @__PURE__ */ a.jsx("p", { className: "dx-rojo", children: p }),
    v === null ? /* @__PURE__ */ a.jsx("p", { className: "muted", children: "Cargando…" }) : z.length === 0 ? /* @__PURE__ */ a.jsx("p", { className: "muted", children: "No hay documentos con estos filtros." }) : /* @__PURE__ */ a.jsxs("table", { className: "dx-lista-docs", children: [
      /* @__PURE__ */ a.jsx("thead", { children: /* @__PURE__ */ a.jsxs("tr", { children: [
        /* @__PURE__ */ a.jsx("th", { children: "Número" }),
        /* @__PURE__ */ a.jsx("th", { children: "Fecha" }),
        /* @__PURE__ */ a.jsx("th", { children: e.tipo === "compra" || e.tipo === "gasto" ? "Proveedor" : "Cliente" }),
        /* @__PURE__ */ a.jsx("th", { children: "Estado" }),
        /* @__PURE__ */ a.jsx("th", { className: "num", children: "Total" })
      ] }) }),
      /* @__PURE__ */ a.jsx("tbody", { children: z.map((L) => /* @__PURE__ */ a.jsxs("tr", { onClick: () => n({ tipo: e.tipo, pantalla: "vista", id: L.id }), tabIndex: 0, onKeyDown: (E) => E.key === "Enter" && n({ tipo: e.tipo, pantalla: "vista", id: L.id }), children: [
        /* @__PURE__ */ a.jsxs("td", { className: "mono", children: [
          /* @__PURE__ */ a.jsx("strong", { children: L.numero }),
          L.extra && /* @__PURE__ */ a.jsx("span", { className: "pill part", style: { marginLeft: 6 }, children: L.extra })
        ] }),
        /* @__PURE__ */ a.jsx("td", { children: Ve(L.fecha) }),
        /* @__PURE__ */ a.jsx("td", { children: L.tercero }),
        /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx("span", { className: Po(L.estado), children: L.estado }) }),
        /* @__PURE__ */ a.jsx("td", { className: "num", children: /* @__PURE__ */ a.jsx("strong", { children: M(L.total) }) })
      ] }, L.id)) }),
      /* @__PURE__ */ a.jsx("tfoot", { children: /* @__PURE__ */ a.jsxs("tr", { children: [
        /* @__PURE__ */ a.jsxs("td", { colSpan: 4, className: "muted", children: [
          T ? `${N} documentos` : `${z.length} documentos`,
          " · suma de la página sin anulados"
        ] }),
        /* @__PURE__ */ a.jsx("td", { className: "num", children: /* @__PURE__ */ a.jsx("strong", { children: M(R) }) })
      ] }) })
    ] }),
    D > 1 && /* @__PURE__ */ a.jsxs("div", { className: "dx-paginas", children: [
      /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", disabled: c <= 1, onClick: () => m(c - 1), children: "←" }),
      /* @__PURE__ */ a.jsxs("span", { className: "muted", children: [
        "Página ",
        c,
        " de ",
        D
      ] }),
      /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", disabled: c >= D, onClick: () => m(c + 1), children: "→" })
    ] })
  ] });
}
function dn(e) {
  return j.useEffect(() => {
    const t = (n) => n.key === "Escape" && e.alCerrar();
    return window.addEventListener("keydown", t), () => window.removeEventListener("keydown", t);
  }, [e]), /* @__PURE__ */ a.jsx("div", { className: "dx-fondo", onMouseDown: (t) => t.target === t.currentTarget && e.alCerrar(), children: /* @__PURE__ */ a.jsxs("div", { className: "dx-dialogo", style: { maxWidth: e.ancho ?? 560 }, role: "dialog", "aria-label": e.titulo, children: [
    /* @__PURE__ */ a.jsxs("div", { className: "dx-dialogo-cab", children: [
      /* @__PURE__ */ a.jsx("strong", { children: e.titulo }),
      /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: e.alCerrar, "aria-label": "Cerrar", children: "✕" })
    ] }),
    /* @__PURE__ */ a.jsx("div", { className: "dx-dialogo-cuerpo", children: e.children }),
    e.acciones && /* @__PURE__ */ a.jsx("div", { className: "dx-dialogo-pie", children: e.acciones })
  ] }) });
}
function Up(e) {
  const { api: t } = vt(), [n, r] = j.useState(!1), [l, i] = j.useState([]), [o, s] = j.useState(null), [u, d] = j.useState(0), y = Br(e.texto, 180), c = o === e.texto.trim() ? l : [], m = oa();
  j.useEffect(() => {
    if (!n) return;
    const g = m(), N = encodeURIComponent(y.trim());
    t.get(`/productos/buscar?texto=${N}&tamanoPagina=12`).then((F) => g() && (i(F.elementos ?? []), s(y.trim()), d(0))).catch(() => g() && (i([]), s(y.trim())));
  }, [y, n]);
  function v(g) {
    var N;
    if (n && g.key === "Enter" && e.texto.trim() && !c.length) {
      g.preventDefault(), g.stopPropagation();
      return;
    }
    if (n && c.length) {
      if (g.key === "ArrowDown") return g.preventDefault(), d((F) => Math.min(F + 1, c.length - 1));
      if (g.key === "ArrowUp") return g.preventDefault(), d((F) => Math.max(F - 1, 0));
      if (g.key === "Enter") {
        g.preventDefault(), g.stopPropagation(), e.alElegir(c[u]), r(!1);
        return;
      }
    }
    if (g.key === "Escape") return r(!1);
    if (g.key === "F2") return g.preventDefault(), r(!0);
    (N = e.alTeclaFuera) == null || N.call(e, g);
  }
  return /* @__PURE__ */ a.jsxs("div", { className: "dx-buscador", children: [
    /* @__PURE__ */ a.jsx(
      "input",
      {
        value: e.texto,
        placeholder: "Buscar artículo…",
        autoFocus: e.autoFocus,
        onChange: (g) => (e.alCambiarTexto(g.target.value), r(!0)),
        onFocus: (g) => g.target.select(),
        onBlur: () => setTimeout(() => r(!1), 150),
        onKeyDown: v,
        ...Object.fromEntries(Object.entries(e.datos ?? {}).map(([g, N]) => [`data-${g}`, N]))
      }
    ),
    n && c.length > 0 && /* @__PURE__ */ a.jsx("div", { className: "dx-lista", children: c.map((g, N) => /* @__PURE__ */ a.jsxs(
      "div",
      {
        className: "dx-opcion" + (N === u ? " activa" : ""),
        onMouseDown: (F) => (F.preventDefault(), e.alElegir(g), r(!1)),
        onMouseEnter: () => d(N),
        children: [
          /* @__PURE__ */ a.jsxs("span", { children: [
            g.referencia && /* @__PURE__ */ a.jsxs("span", { className: "mono muted", children: [
              g.referencia,
              " · "
            ] }),
            /* @__PURE__ */ a.jsx("strong", { children: g.nombre }),
            g.familia && /* @__PURE__ */ a.jsxs("span", { className: "muted", children: [
              " · ",
              g.familia
            ] })
          ] }),
          /* @__PURE__ */ a.jsxs("span", { className: "muted", style: { whiteSpace: "nowrap" }, children: [
            M(e.precioDe ? e.precioDe(g) : g.precioUnitario),
            "/",
            g.unidad,
            g.controlarStock && /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
              " · stock ",
              ze(g.stock)
            ] })
          ] })
        ]
      },
      g.id
    )) })
  ] });
}
function _o(e) {
  const [t, n] = j.useState(""), [r, l] = j.useState(!1), [i, o] = j.useState(0), s = e.terceros.find((c) => c.id === e.valor), u = j.useMemo(() => {
    const c = t.trim().toLowerCase();
    return e.terceros.filter((m) => m.activo !== !1 && (!c || m.nombre.toLowerCase().includes(c) || (m.nifFiscal ?? "").toLowerCase().includes(c))).slice(0, 30);
  }, [t, e.terceros]), d = j.useRef(null);
  function y(c) {
    e.alCambiar(c.id), n(""), l(!1);
  }
  return /* @__PURE__ */ a.jsxs("div", { children: [
    /* @__PURE__ */ a.jsx("label", { children: e.etiqueta }),
    /* @__PURE__ */ a.jsxs("div", { className: "dx-buscador", children: [
      /* @__PURE__ */ a.jsx(
        "input",
        {
          ref: d,
          disabled: e.deshabilitado,
          value: r ? t : s ? `${s.nombre}${s.nifFiscal ? " · " + s.nifFiscal : ""}` : t,
          placeholder: `Buscar ${e.etiqueta.toLowerCase()} por nombre o NIF…`,
          onFocus: () => (l(!0), n("")),
          onBlur: () => setTimeout(() => l(!1), 150),
          onChange: (c) => (n(c.target.value), o(0)),
          onKeyDown: (c) => {
            if (c.key === "ArrowDown") return c.preventDefault(), o((m) => Math.min(m + 1, u.length - 1));
            if (c.key === "ArrowUp") return c.preventDefault(), o((m) => Math.max(m - 1, 0));
            if (c.key === "Enter" && u[i]) return c.preventDefault(), y(u[i]);
            if (c.key === "Escape") return l(!1);
          }
        }
      ),
      r && u.length > 0 && /* @__PURE__ */ a.jsx("div", { className: "dx-lista", children: u.map((c, m) => /* @__PURE__ */ a.jsxs("div", { className: "dx-opcion" + (m === i ? " activa" : ""), onMouseDown: (v) => (v.preventDefault(), y(c)), onMouseEnter: () => o(m), children: [
        /* @__PURE__ */ a.jsx("strong", { children: c.nombre }),
        /* @__PURE__ */ a.jsx("span", { className: "muted", children: [c.nifFiscal, c.poblacion].filter(Boolean).join(" · ") })
      ] }, c.id)) })
    ] })
  ] });
}
const Vp = { Porcentaje: "%", PorUnidad: "€/ud", PorKilo: "€/kg", Importe: "€" };
function Fo(e) {
  if (!e.catalogo.length) return null;
  const t = e.lista === void 0, n = e.lista ?? e.sugeridos ?? [], r = (l, i) => e.alCambiar(n.map((o, s) => s === l ? { ...o, ...i } : o));
  return /* @__PURE__ */ a.jsxs("div", { className: "dx-conceptos", children: [
    /* @__PURE__ */ a.jsxs("span", { className: "muted", children: [
      e.documento ? "Conceptos del documento" : t ? "Conceptos automáticos" : "Conceptos",
      ":"
    ] }),
    n.map((l, i) => {
      const o = e.catalogo.find((s) => s.id === l.conceptoId);
      return /* @__PURE__ */ a.jsxs("span", { className: "dx-chip" + ((o == null ? void 0 : o.efecto) === "Coste" ? " coste" : "") + (t ? " auto" : ""), children: [
        /* @__PURE__ */ a.jsx("select", { value: l.conceptoId, onChange: (s) => r(i, { conceptoId: s.target.value, valor: null }), children: e.catalogo.map((s) => /* @__PURE__ */ a.jsxs("option", { value: s.id, children: [
          s.codigo,
          " ",
          s.sentido === "Resta" ? "−" : "+",
          Vp[s.calculo],
          s.efecto === "Coste" ? " · coste" : ""
        ] }, s.id)) }),
        /* @__PURE__ */ a.jsx("input", { type: "number", step: "0.0001", value: l.valor ?? "", placeholder: String((o == null ? void 0 : o.valor) ?? ""), onChange: (s) => r(i, { valor: s.target.value === "" ? null : Number(s.target.value) }) }),
        /* @__PURE__ */ a.jsx("button", { type: "button", title: "Quitar", onClick: () => e.alCambiar(n.filter((s, u) => u !== i)), children: "✕" })
      ] }, i);
    }),
    /* @__PURE__ */ a.jsx("button", { type: "button", className: "dx-enlace", onClick: () => e.alCambiar([...n, { conceptoId: e.catalogo[0].id, valor: null }]), children: "+ concepto" }),
    !e.documento && !t && /* @__PURE__ */ a.jsx("button", { type: "button", className: "dx-enlace", onClick: () => e.alCambiar(void 0), children: "volver a los automáticos" }),
    !e.documento && t && n.length === 0 && /* @__PURE__ */ a.jsx("span", { className: "muted", children: "ninguno" })
  ] });
}
function sa(e) {
  var t;
  return (t = e.conceptos) != null && t.length ? /* @__PURE__ */ a.jsx("div", { className: "dx-aplicados", children: e.conceptos.map((n, r) => /* @__PURE__ */ a.jsxs("div", { children: [
    "· ",
    n.nombre,
    n.calculo === "Porcentaje" ? ` (${ze(n.valor)} %)` : "",
    n.repartido ? " · del documento" : "",
    n.efecto === "Coste" ? " · coste" : "",
    ": ",
    /* @__PURE__ */ a.jsx("strong", { children: M(n.importe) })
  ] }, r)) }) : null;
}
const gr = () => ({ clave: Vr(), productoId: null, descripcion: "", cantidad: 1, precio: null, dto: 0, iva: null });
function nd(e) {
  const t = j.useRef(null), [n, r] = j.useState(/* @__PURE__ */ new Set()), l = e.modo === "venta", i = l ? 7 : 5, o = (c, m) => e.alCambiar(e.lineas.map((v) => v.clave === c ? { ...v, ...m } : v)), s = (c) => {
    const m = e.lineas.filter((v) => v.clave !== c);
    e.alCambiar(m.length ? m : [gr()]);
  };
  function u(c, m) {
    var g;
    const v = (g = t.current) == null ? void 0 : g.querySelector(`[data-f="${c}"][data-c="${m}"]`);
    v == null || v.focus(), v instanceof HTMLInputElement && v.select();
  }
  function d(c) {
    const m = c.target, v = Number(m.dataset.f), g = Number(m.dataset.c);
    if (!(Number.isNaN(v) || Number.isNaN(g)))
      if (c.key === "Enter") {
        if (c.preventDefault(), g < i - 1) return u(v, g + 1);
        v === e.lineas.length - 1 ? (e.alCambiar([...e.lineas, gr()]), setTimeout(() => u(v + 1, 0), 30)) : u(v + 1, 0);
      } else c.key === "ArrowDown" && m.tagName !== "SELECT" ? (c.preventDefault(), u(Math.min(v + 1, e.lineas.length - 1), g)) : c.key === "ArrowUp" && m.tagName !== "SELECT" && (c.preventDefault(), u(Math.max(v - 1, 0), g));
  }
  const y = (c) => r((m) => {
    const v = new Set(m);
    return v.has(c) ? v.delete(c) : v.add(c), v;
  });
  return /* @__PURE__ */ a.jsxs("div", { ref: t, className: "dx-rejilla", onKeyDown: d, children: [
    /* @__PURE__ */ a.jsxs("table", { children: [
      /* @__PURE__ */ a.jsx("thead", { children: /* @__PURE__ */ a.jsxs("tr", { children: [
        /* @__PURE__ */ a.jsx("th", { style: { width: "22%" }, children: "Artículo" }),
        /* @__PURE__ */ a.jsx("th", { children: "Descripción" }),
        /* @__PURE__ */ a.jsx("th", { className: "num", style: { width: 90 }, children: "Cantidad" }),
        /* @__PURE__ */ a.jsx("th", { className: "num", style: { width: 110 }, children: "Precio" }),
        l && /* @__PURE__ */ a.jsx("th", { className: "num", style: { width: 74 }, children: "Dto %" }),
        l && /* @__PURE__ */ a.jsx("th", { style: { width: 150 }, children: "Impuesto" }),
        /* @__PURE__ */ a.jsx("th", { className: "num", style: { width: 110 }, children: "Importe" }),
        /* @__PURE__ */ a.jsx("th", { className: "num", style: { width: 100 }, children: l ? "Margen" : "Coste entrada" }),
        /* @__PURE__ */ a.jsx("th", { style: { width: 70 } })
      ] }) }),
      /* @__PURE__ */ a.jsx("tbody", { children: e.lineas.map((c, m) => {
        const v = e.calculos[m], g = (v == null ? void 0 : v.conceptos) ?? [], N = l && c.controlarStock && c.stock != null && c.cantidad > c.stock, F = v && v.margen != null && v.importe ? v.margen / v.importe * 100 : null;
        return [
          /* @__PURE__ */ a.jsxs("tr", { className: m % 2 ? "dx-par" : "", children: [
            /* @__PURE__ */ a.jsxs("td", { children: [
              e.soloLectura ? /* @__PURE__ */ a.jsx("span", { className: "mono", children: c.referencia ?? "" }) : /* @__PURE__ */ a.jsx(
                Up,
                {
                  texto: c.referencia ?? (c.productoId ? c.descripcion : ""),
                  alCambiarTexto: (p) => o(c.clave, { referencia: p, ...p === "" ? { productoId: null } : {} }),
                  alElegir: (p) => (e.alElegirArticulo(c.clave, p), u(m, 2)),
                  precioDe: l ? void 0 : (p) => p.precioCompraPorUnidadCompra ?? p.precioCompra,
                  datos: { f: m, c: 0 }
                }
              ),
              c.productoId && /* @__PURE__ */ a.jsxs("div", { className: "dx-sub", children: [
                c.unidad && /* @__PURE__ */ a.jsx("span", { children: c.unidad }),
                c.controlarStock && /* @__PURE__ */ a.jsxs("span", { className: N ? "dx-rojo" : "", children: [
                  " · stock ",
                  ze(c.stock)
                ] })
              ] })
            ] }),
            /* @__PURE__ */ a.jsxs("td", { children: [
              /* @__PURE__ */ a.jsx(
                "input",
                {
                  "data-f": m,
                  "data-c": 1,
                  value: c.descripcion,
                  placeholder: c.productoId ? "" : "Descripción (línea libre)",
                  disabled: e.soloLectura,
                  onChange: (p) => o(c.clave, { descripcion: p.target.value })
                }
              ),
              g.length > 0 && !n.has(c.clave) && /* @__PURE__ */ a.jsx("button", { type: "button", className: "dx-resumen-conc", onClick: () => y(c.clave), title: "Ver y cambiar los conceptos", children: g.map((p) => `${p.importe < 0 ? "−" : "+"} ${p.codigo.toLowerCase()} ${ye(Math.abs(p.importe))}${p.efecto === "Coste" ? " (coste)" : ""}`).join(" · ") })
            ] }),
            /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx(
              "input",
              {
                "data-f": m,
                "data-c": 2,
                className: "num",
                type: "number",
                step: "0.001",
                value: c.cantidad,
                disabled: e.soloLectura,
                onChange: (p) => o(c.clave, { cantidad: Number(p.target.value) })
              }
            ) }),
            /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx(
              "input",
              {
                "data-f": m,
                "data-c": 3,
                className: "num" + (c.precio == null ? " dx-auto" : ""),
                type: "number",
                step: "0.0001",
                disabled: e.soloLectura,
                value: c.precio ?? "",
                placeholder: v ? ye(v.precio) : "",
                title: c.precio == null ? "Precio de la tarifa del cliente o del artículo (escribe uno para fijarlo)" : "",
                onChange: (p) => o(c.clave, { precio: p.target.value === "" ? null : Number(p.target.value) })
              }
            ) }),
            l && /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx(
              "input",
              {
                "data-f": m,
                "data-c": 4,
                className: "num",
                type: "number",
                step: "0.01",
                value: c.dto || (c.precio == null && (v != null && v.dto) ? v.dto : 0),
                disabled: e.soloLectura,
                onChange: (p) => o(c.clave, { dto: Number(p.target.value), precio: c.precio ?? (v == null ? void 0 : v.precio) ?? null })
              }
            ) }),
            l && /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsxs("select", { "data-f": m, "data-c": 5, value: c.iva ?? (v == null ? void 0 : v.iva) ?? "", disabled: e.soloLectura, onChange: (p) => o(c.clave, { iva: p.target.value || null }), children: [
              !c.iva && !(v != null && v.iva) && /* @__PURE__ */ a.jsx("option", { value: "", children: "Del artículo" }),
              e.ivas.map((p) => /* @__PURE__ */ a.jsx("option", { value: p.codigo, children: p.nombre }, p.codigo))
            ] }) }),
            /* @__PURE__ */ a.jsx("td", { className: "num", children: /* @__PURE__ */ a.jsx("strong", { children: v ? M(v.importe) : "—" }) }),
            /* @__PURE__ */ a.jsx("td", { className: "num", children: l ? (v == null ? void 0 : v.margen) != null && /* @__PURE__ */ a.jsxs("span", { className: v.margen < 0 ? "dx-rojo" : "muted", children: [
              M(v.margen),
              F != null && /* @__PURE__ */ a.jsxs("div", { className: "dx-sub", children: [
                ye(F),
                " %"
              ] })
            ] }) : (v == null ? void 0 : v.costeUnitarioEntrada) != null && /* @__PURE__ */ a.jsxs("span", { className: "muted", children: [
              M(v.costeUnitarioEntrada),
              "/ud"
            ] }) }),
            /* @__PURE__ */ a.jsxs("td", { className: "right", style: { whiteSpace: "nowrap" }, children: [
              !e.soloLectura && e.catalogo.length > 0 && /* @__PURE__ */ a.jsx("button", { type: "button", className: "dx-icono" + (n.has(c.clave) ? " activo" : ""), title: "Conceptos de la línea", onClick: () => y(c.clave), "data-f": m, "data-c": l ? 6 : 4, children: "±" }),
              !e.soloLectura && /* @__PURE__ */ a.jsx("button", { type: "button", className: "dx-icono", title: "Quitar la línea", onClick: () => s(c.clave), children: "✕" })
            ] })
          ] }, c.clave),
          n.has(c.clave) && /* @__PURE__ */ a.jsx("tr", { className: "dx-fila-conc", children: /* @__PURE__ */ a.jsx("td", { colSpan: l ? 9 : 7, children: /* @__PURE__ */ a.jsx(Fo, { catalogo: e.catalogo, lista: c.conceptos, sugeridos: e.sugeridos[c.clave], alCambiar: (p) => o(c.clave, { conceptos: p }) }) }) }, c.clave + "c")
        ];
      }) })
    ] }),
    !e.soloLectura && /* @__PURE__ */ a.jsx("button", { type: "button", className: "btn small ghost", style: { marginTop: 8 }, onClick: () => (e.alCambiar([...e.lineas, gr()]), setTimeout(() => u(e.lineas.length, 0), 30)), children: "+ Añadir línea" }),
    !e.soloLectura && /* @__PURE__ */ a.jsx("span", { className: "muted dx-ayuda", children: "Intro: siguiente casilla · ↑↓: cambiar de línea · F2: buscar artículo · el precio en gris es el de tarifa" })
  ] });
}
const Bp = { presupuesto: "presupuesto", pedido: "pedido de venta", factura: "factura" };
function rd(e) {
  const t = /* @__PURE__ */ new Map();
  for (const n of e)
    for (const r of n.conceptos ?? [])
      if (r.repartido) {
        const l = t.get(r.conceptoId);
        t.set(r.conceptoId, { conceptoId: r.conceptoId, valor: r.calculo === "Importe" ? jl(((l == null ? void 0 : l.valor) ?? 0) + r.valor) : r.valor });
      }
  return {
    porLinea: e.map((n) => (n.conceptos ?? []).filter((r) => !r.repartido).map((r) => ({ conceptoId: r.conceptoId, valor: r.valor }))),
    documento: [...t.values()]
  };
}
function Hp(e) {
  var fe, ue, K, _t;
  const { api: t, anfitrion: n } = vt(), r = !!((fe = e.semilla) != null && fe.rectificaId), [l, i] = j.useState([]), [o, s] = j.useState([]), [u, d] = j.useState([]), [y, c] = j.useState([]), [m, v] = j.useState([]), [g, N] = j.useState(((ue = e.semilla) == null ? void 0 : ue.clienteId) ?? ""), [F, p] = j.useState(e.tipo === "pedido" && ((K = e.semilla) != null && K.fecha) ? e.semilla.fecha : ht()), [f, h] = j.useState(""), [k, z] = j.useState(""), [R, T] = j.useState(0), [D, L] = j.useState(!1), [E, _] = j.useState(null), [$, Ce] = j.useState(30), [Be, be] = j.useState(""), [de, Ne] = j.useState([gr()]), [C, x] = j.useState([]), [S, A] = j.useState(null), [H, G] = j.useState(""), [me, ge] = j.useState(!1), [te, U] = j.useState(!1), [Wn, Yt] = j.useState(!1), Qn = oa();
  j.useEffect(() => {
    t.get("/clientes").then(i).catch(() => i([])), t.get("/tipos-iva").then((I) => s(I.filter((V) => V.activo))).catch(() => s([])), t.get("/formas-pago").then((I) => d(I.filter((V) => V.activo))).catch(() => d([])), t.get("/series").then((I) => c([...new Set(I.filter((V) => V.tipoDocumento === "Factura").map((V) => V.prefijo))])).catch(() => c([])), t.get("/conceptos-linea?ambito=Ventas&activos=true").then(v).catch(() => v([]));
  }, [t]), j.useEffect(() => {
    const I = e.semilla;
    if (!I || !I.lineas.length) return;
    const { porLinea: V, documento: J } = rd(I.lineas), ie = I.lineas.map((b, fa) => ({
      clave: Vr(),
      productoId: b.productoId ?? null,
      descripcion: b.descripcion,
      cantidad: b.cantidad,
      precio: b.precioUnitario,
      dto: b.porcentajeDescuento,
      iva: b.codigoIva,
      conceptos: r ? [] : V[fa]
    }));
    Ne(ie), x(r ? [] : J), Promise.all(ie.map((b) => b.productoId ? t.get(`/productos/${b.productoId}`).catch(() => null) : Promise.resolve(null))).then(
      (b) => Ne((fa) => fa.map((Ro, mn) => b[mn] ? { ...Ro, referencia: b[mn].referencia ?? b[mn].nombre, unidad: b[mn].unidad, stock: b[mn].stock, controlarStock: b[mn].controlarStock } : Ro))
    );
  }, [e.semilla, t, r]);
  const ae = l.find((I) => I.id === g);
  j.useEffect(() => {
    ae && (L(!!ae.recargoEquivalencia), ae.formaPagoDefectoId && z(ae.formaPagoDefectoId));
  }, [ae]);
  const It = j.useMemo(() => de.map((I, V) => ({ l: I, i: V })).filter(({ l: I }) => (I.productoId || I.descripcion.trim()) && I.cantidad > 0), [de]), Hr = j.useMemo(
    () => ({
      clienteId: g,
      fechaEmision: e.tipo === "factura" ? F : null,
      serie: f || null,
      diasVencimiento: R,
      formaPagoId: k || null,
      recargoEquivalencia: D,
      porcentajeIrpf: E,
      conceptosDocumento: C,
      lineas: It.map(({ l: I }) => ({
        cantidad: I.cantidad,
        descripcion: I.descripcion.trim() || null,
        precioUnitario: I.precio,
        codigoIva: I.iva,
        porcentajeDescuento: I.dto,
        productoId: I.productoId,
        ...r ? { conceptos: [] } : I.conceptos === void 0 ? {} : { conceptos: I.conceptos }
      }))
    }),
    [g, F, f, R, k, D, E, C, It, e.tipo, r]
  ), se = Br(Hr, 350);
  j.useEffect(() => {
    if (!se.clienteId || se.lineas.length === 0) {
      A(null), G(se.clienteId ? "" : "Elige el cliente para calcular precios e importes.");
      return;
    }
    const I = Qn();
    ge(!0), t.post("/facturas/simular", se).then((V) => I() && (A(V), G(""))).catch((V) => I() && (A(null), G(V.message))).finally(() => I() && ge(!1));
  }, [se, t]);
  const Xt = j.useMemo(() => {
    const I = de.map(() => {
    });
    return S && It.forEach(({ i: V }, J) => {
      const ie = S.lineas[J];
      ie && (I[V] = { precio: ie.precioUnitario, dto: ie.porcentajeDescuento, iva: ie.codigoIva, importe: ie.base, margen: ie.productoId || ie.costeUnitario || ie.costeConceptos ? ie.margen : void 0, conceptos: ie.conceptos });
    }), I;
  }, [S, de, It]), Gn = j.useMemo(() => {
    const I = {};
    return de.forEach((V, J) => {
      var ie;
      return I[V.clave] = (((ie = Xt[J]) == null ? void 0 : ie.conceptos) ?? []).filter((b) => !b.repartido).map((b) => ({ conceptoId: b.conceptoId, valor: b.valor }));
    }), I;
  }, [de, Xt]);
  function Pt(I, V) {
    Ne(
      (J) => J.map(
        (ie) => ie.clave === I ? { ...ie, productoId: V.id, referencia: V.referencia ?? V.nombre, descripcion: V.nombre, precio: null, dto: 0, iva: null, conceptos: void 0, unidad: V.unidad, stock: V.stock, controlarStock: V.controlarStock } : ie
      )
    );
  }
  const Wr = j.useMemo(() => {
    const I = /* @__PURE__ */ new Map();
    for (const V of (S == null ? void 0 : S.lineas) ?? []) {
      const J = I.get(V.codigoIva) ?? { base: 0, cuota: 0, pct: V.porcentajeIva };
      J.base += V.base, J.cuota += V.cuotaIva, I.set(V.codigoIva, J);
    }
    return [...I.entries()];
  }, [S]), Kn = ((S == null ? void 0 : S.lineas) ?? []).reduce((I, V) => I + (V.base - V.margen), 0), qn = S ? S.baseImponible - Kn : 0, da = (I) => {
    var V;
    return ((V = o.find((J) => J.codigo === I)) == null ? void 0 : V.nombre) ?? I;
  };
  async function bn() {
    if (S) {
      U(!0);
      try {
        const I = It.map(({ l: J }, ie) => {
          const b = S.lineas[ie];
          return {
            cantidad: J.cantidad,
            descripcion: b.descripcion,
            precioUnitario: b.precioUnitario,
            codigoIva: b.codigoIva,
            porcentajeDescuento: b.porcentajeDescuento,
            productoId: J.productoId,
            ...r ? {} : J.conceptos === void 0 ? {} : { conceptos: J.conceptos }
          };
        });
        let V;
        if (r)
          V = (await t.post(`/facturas/${e.semilla.rectificaId}/rectificar`, { motivo: Be, lineas: I, fechaEmision: F, porcentajeIrpf: E, serie: f || null })).id;
        else if (e.tipo === "factura")
          V = (await t.post("/facturas", { ...Hr, lineas: I })).id;
        else if (e.tipo === "presupuesto") {
          const J = { clienteId: g, diasValidez: $, lineas: I, conceptosDocumento: C };
          V = e.id ? (await t.put(`/presupuestos/${e.id}`, J)).id : (await t.post("/presupuestos", J)).id;
        } else {
          const J = { clienteId: g, fecha: F, lineas: I, conceptosDocumento: C };
          V = e.id ? (await t.put(`/pedidos-venta/${e.id}`, J)).id : (await t.post("/pedidos-venta", J)).id;
        }
        n.aviso(e.tipo === "factura" ? "Factura emitida." : "Documento guardado.", "ok"), e.alGuardar(V);
      } catch (I) {
        n.aviso(I.message, "err");
      } finally {
        U(!1), Yt(!1);
      }
    }
  }
  const w = r ? `Rectificativa de la factura ${((_t = e.semilla) == null ? void 0 : _t.rectificaNumero) ?? ""}` : `${e.id ? "Editar" : "Nuevo"} ${Bp[e.tipo]}`.replace("Nuevo factura", "Nueva factura"), B = !!S && !me && (!r || Be.trim().length > 0);
  return /* @__PURE__ */ a.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsxs("div", { className: "panel-head", children: [
        /* @__PURE__ */ a.jsx("h2", { children: w }),
        /* @__PURE__ */ a.jsxs("div", { style: { display: "flex", gap: 8 }, children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: e.alCancelar, children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", disabled: !B || te, onClick: () => e.tipo === "factura" ? Yt(!0) : bn(), children: e.tipo === "factura" ? "Emitir factura" : "Guardar" })
        ] })
      ] }),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-cabecera", children: [
        /* @__PURE__ */ a.jsxs("div", { className: "dx-cab-campos", children: [
          /* @__PURE__ */ a.jsx(_o, { terceros: l, valor: g, alCambiar: N, etiqueta: "Cliente", deshabilitado: r || e.tipo === "pedido" && !!e.id && !1 }),
          /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", children: [
            e.tipo !== "presupuesto" && /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: e.tipo === "factura" ? "Fecha de emisión" : "Fecha del pedido" }),
              /* @__PURE__ */ a.jsx("input", { type: "date", value: F, onChange: (I) => p(I.target.value) })
            ] }),
            e.tipo === "presupuesto" && /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Validez" }),
              /* @__PURE__ */ a.jsx("select", { value: $, onChange: (I) => Ce(Number(I.target.value)), children: [15, 30, 60, 90].map((I) => /* @__PURE__ */ a.jsxs("option", { value: I, children: [
                I,
                " días"
              ] }, I)) })
            ] }),
            e.tipo === "factura" && y.length > 0 && /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Serie" }),
              /* @__PURE__ */ a.jsxs("select", { value: f, onChange: (I) => h(I.target.value), children: [
                /* @__PURE__ */ a.jsx("option", { value: "", children: "La del cliente" }),
                y.map((I) => /* @__PURE__ */ a.jsx("option", { value: I, children: I }, I))
              ] })
            ] }),
            e.tipo === "factura" && !r && /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
              /* @__PURE__ */ a.jsxs("div", { children: [
                /* @__PURE__ */ a.jsx("label", { children: "Forma de pago" }),
                /* @__PURE__ */ a.jsxs("select", { value: k, onChange: (I) => z(I.target.value), children: [
                  /* @__PURE__ */ a.jsx("option", { value: "", children: "— Vencimiento a mano —" }),
                  u.map((I) => /* @__PURE__ */ a.jsx("option", { value: I.id, children: I.nombre }, I.id))
                ] })
              ] }),
              !k && /* @__PURE__ */ a.jsxs("div", { children: [
                /* @__PURE__ */ a.jsx("label", { children: "Vencimiento" }),
                /* @__PURE__ */ a.jsx("select", { value: R, onChange: (I) => T(Number(I.target.value)), children: [0, 15, 30, 45, 60, 90].map((I) => /* @__PURE__ */ a.jsx("option", { value: I, children: I ? `${I} días` : "Contado" }, I)) })
              ] })
            ] }),
            e.tipo === "factura" && /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Retención IRPF %" }),
              /* @__PURE__ */ a.jsx("input", { type: "number", step: "0.01", value: E ?? "", placeholder: String((ae == null ? void 0 : ae.porcentajeIrpfDefecto) ?? 0), onChange: (I) => _(I.target.value === "" ? null : Number(I.target.value)) })
            ] })
          ] }),
          e.tipo === "factura" && !r && /* @__PURE__ */ a.jsxs("label", { className: "dx-check", children: [
            /* @__PURE__ */ a.jsx("input", { type: "checkbox", checked: D, onChange: (I) => L(I.target.checked) }),
            " Recargo de equivalencia"
          ] }),
          r && /* @__PURE__ */ a.jsxs("div", { children: [
            /* @__PURE__ */ a.jsx("label", { children: "Motivo de la rectificación (obligatorio)" }),
            /* @__PURE__ */ a.jsx("input", { value: Be, onChange: (I) => be(I.target.value), placeholder: "Devolución, error en precio…" })
          ] })
        ] }),
        /* @__PURE__ */ a.jsx("div", { className: "dx-ficha", children: ae ? /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("strong", { children: ae.nombre }),
          /* @__PURE__ */ a.jsx("div", { className: "muted", children: [ae.nifFiscal, ae.poblacion, ae.provincia].filter(Boolean).join(" · ") }),
          ae.limiteRiesgo != null && /* @__PURE__ */ a.jsxs("div", { className: "muted", children: [
            "Límite de riesgo ",
            M(ae.limiteRiesgo)
          ] }),
          ae.tarifaId && /* @__PURE__ */ a.jsx("div", { className: "muted", children: "Con tarifa de precios propia" }),
          ae.recargoEquivalencia && /* @__PURE__ */ a.jsx("div", { className: "muted", children: "En recargo de equivalencia" }),
          (S == null ? void 0 : S.avisoRiesgo) && /* @__PURE__ */ a.jsxs("div", { className: "dx-aviso", children: [
            "⚠ ",
            S.avisoRiesgo
          ] })
        ] }) : /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Elige un cliente: se aplican su tarifa, su forma de pago, su recargo y sus conceptos." }) })
      ] })
    ] }),
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsx(nd, { modo: "venta", lineas: de, alCambiar: Ne, calculos: Xt, ivas: o, catalogo: r ? [] : m, sugeridos: Gn, alElegirArticulo: Pt }),
      !r && m.length > 0 && /* @__PURE__ */ a.jsx("div", { style: { marginTop: 10 }, children: /* @__PURE__ */ a.jsx(Fo, { catalogo: m, lista: C, alCambiar: (I) => x(I ?? []), documento: !0 }) })
    ] }),
    /* @__PURE__ */ a.jsxs("div", { className: "dx-pie", children: [
      /* @__PURE__ */ a.jsxs("div", { className: "dx-estado", children: [
        me && /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Calculando…" }),
        !me && H && /* @__PURE__ */ a.jsx("span", { className: "dx-rojo", children: H }),
        (S == null ? void 0 : S.mencionFiscal) && /* @__PURE__ */ a.jsx("div", { className: "muted", style: { fontSize: 12 }, children: S.mencionFiscal })
      ] }),
      /* @__PURE__ */ a.jsxs("div", { className: "panel dx-totales", children: [
        Wr.map(([I, V]) => /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsxs("span", { className: "muted", children: [
            da(I),
            " · base ",
            ye(V.base)
          ] }),
          /* @__PURE__ */ a.jsx("span", { children: M(V.cuota) })
        ] }, I)),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Base imponible" }),
          /* @__PURE__ */ a.jsx("span", { children: M(S == null ? void 0 : S.baseImponible) })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Impuestos" }),
          /* @__PURE__ */ a.jsx("span", { children: M(S == null ? void 0 : S.cuotaIva) })
        ] }),
        !!(S != null && S.recargoTotal) && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Recargo de equivalencia" }),
          /* @__PURE__ */ a.jsx("span", { children: M(S.recargoTotal) })
        ] }),
        !!(S != null && S.retencionIrpf) && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsxs("span", { className: "muted", children: [
            "Retención IRPF (",
            ye(S.porcentajeIrpf),
            " %)"
          ] }),
          /* @__PURE__ */ a.jsxs("span", { children: [
            "−",
            M(S.retencionIrpf)
          ] })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ a.jsx("span", { children: "Total" }),
          /* @__PURE__ */ a.jsx("span", { children: M(S == null ? void 0 : S.total) })
        ] }),
        S && Kn > 0 && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Coste · margen" }),
          /* @__PURE__ */ a.jsxs("span", { className: qn < 0 ? "dx-rojo" : "muted", children: [
            M(Kn),
            " · ",
            M(qn),
            " (",
            ye(S.baseImponible ? qn / S.baseImponible * 100 : 0),
            " %)"
          ] })
        ] })
      ] })
    ] }),
    Wn && S && /* @__PURE__ */ a.jsx(
      dn,
      {
        titulo: r ? "Emitir la rectificativa" : "Emitir la factura",
        alCerrar: () => Yt(!1),
        acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => Yt(!1), children: "Revisar" }),
          /* @__PURE__ */ a.jsxs("button", { className: "btn small", disabled: te, onClick: bn, children: [
            "Emitir ",
            M(S.total)
          ] })
        ] }),
        children: /* @__PURE__ */ a.jsxs("p", { style: { margin: 0 }, children: [
          "Se emitirá una factura ",
          r ? "rectificativa " : "",
          "de ",
          /* @__PURE__ */ a.jsx("strong", { children: M(S.total) }),
          " a ",
          /* @__PURE__ */ a.jsx("strong", { children: ae == null ? void 0 : ae.nombre }),
          " con fecha ",
          F.split("-").reverse().join("/"),
          ". Una factura emitida no se modifica: queda numerada y encadenada en VeriFactu; para corregirla hay que rectificarla o anularla."
        ] })
      }
    )
  ] });
}
function Wp(e) {
  var be, de, Ne;
  const { api: t, anfitrion: n } = vt(), [r, l] = j.useState([]), [i, o] = j.useState([]), [s, u] = j.useState(((be = e.semilla) == null ? void 0 : be.proveedorId) ?? ""), [d, y] = j.useState(((de = e.semilla) == null ? void 0 : de.fecha) ?? ht()), [c, m] = j.useState([gr()]), [v, g] = j.useState([]), [N, F] = j.useState(null), [p, f] = j.useState(""), [h, k] = j.useState(!1), z = oa();
  j.useEffect(() => {
    t.get("/proveedores").then(l).catch(() => l([])), t.get("/conceptos-linea?ambito=Compras&activos=true").then(o).catch(() => o([]));
  }, [t]), j.useEffect(() => {
    const C = e.semilla;
    if (!C) return;
    const x = C.lineas.map((G) => ({ ...G, porcentajeDescuento: 0, codigoIva: "" })), { porLinea: S, documento: A } = rd(x), H = C.lineas.map((G, me) => ({ clave: Vr(), productoId: G.productoId ?? null, descripcion: G.descripcion, cantidad: G.cantidad, precio: G.precioUnitario, dto: 0, iva: null, conceptos: S[me] }));
    m(H), g(A), Promise.all(H.map((G) => G.productoId ? t.get(`/productos/${G.productoId}`).catch(() => null) : Promise.resolve(null))).then(
      (G) => m((me) => me.map((ge, te) => G[te] ? { ...ge, referencia: G[te].referencia ?? G[te].nombre, unidad: G[te].unidadCompra || G[te].unidad, stock: G[te].stock, controlarStock: G[te].controlarStock } : ge))
    );
  }, [e.semilla, t]);
  const R = r.find((C) => C.id === s), T = j.useMemo(() => c.map((C, x) => ({ l: C, i: x })).filter(({ l: C }) => C.descripcion.trim() && C.cantidad > 0), [c]), D = j.useMemo(
    () => {
      var C, x;
      return {
        proveedorId: s || null,
        proveedorTexto: (R == null ? void 0 : R.nombre) ?? (((C = e.semilla) == null ? void 0 : C.proveedorTexto) || null),
        solicitudOrigenId: e.id ? null : ((x = e.semilla) == null ? void 0 : x.solicitudOrigenId) ?? null,
        fecha: d,
        conceptosDocumento: v,
        lineas: T.map(({ l: S }) => ({ descripcion: S.descripcion.trim(), cantidad: S.cantidad, precioUnitario: S.precio ?? 0, productoId: S.productoId, ...S.conceptos === void 0 ? {} : { conceptos: S.conceptos } }))
      };
    },
    [s, R, d, v, T, e.id, e.semilla]
  ), L = Br(D, 350);
  j.useEffect(() => {
    if (!L.proveedorId || L.lineas.length === 0) {
      F(null), f(L.proveedorId ? "" : "Elige el proveedor.");
      return;
    }
    const C = z();
    t.post("/compras/pedidos/simular", L).then((x) => C() && (F(x), f(""))).catch((x) => C() && (F(null), f(x.message)));
  }, [L, t]);
  const E = j.useMemo(() => {
    const C = c.map(() => {
    });
    return T.forEach(({ i: x }, S) => {
      const A = N == null ? void 0 : N.lineas[S];
      A && (C[x] = { precio: A.precioUnitario, importe: A.importe, costeUnitarioEntrada: A.costeUnitarioEntrada, conceptos: A.conceptos });
    }), C;
  }, [N, c, T]), _ = j.useMemo(() => {
    const C = {};
    return c.forEach((x, S) => {
      var A;
      return C[x.clave] = (((A = E[S]) == null ? void 0 : A.conceptos) ?? []).filter((H) => !H.repartido).map((H) => ({ conceptoId: H.conceptoId, valor: H.valor }));
    }), C;
  }, [c, E]);
  function $(C, x) {
    const S = x.precioCompraPorUnidadCompra ?? x.precioCompra;
    m((A) => A.map((H) => H.clave === C ? { ...H, productoId: x.id, referencia: x.referencia ?? x.nombre, descripcion: x.nombre, precio: S, conceptos: void 0, unidad: x.unidadCompra || x.unidad, stock: x.stock, controlarStock: x.controlarStock } : H));
  }
  async function Ce() {
    k(!0);
    try {
      const C = e.id ? await t.put(`/compras/pedidos/${e.id}`, D) : await t.post("/compras/pedidos", D);
      n.aviso("Pedido de compra guardado.", "ok"), e.alGuardar(C.id);
    } catch (C) {
      n.aviso(C.message, "err");
    } finally {
      k(!1);
    }
  }
  const Be = ((N == null ? void 0 : N.lineas) ?? []).reduce((C, x) => C + x.costeConceptos, 0);
  return /* @__PURE__ */ a.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsxs("div", { className: "panel-head", children: [
        /* @__PURE__ */ a.jsx("h2", { children: e.id ? `Editar pedido ${((Ne = e.semilla) == null ? void 0 : Ne.numeroCompleto) ?? ""}` : "Nuevo pedido de compra" }),
        /* @__PURE__ */ a.jsxs("div", { style: { display: "flex", gap: 8 }, children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: e.alCancelar, children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", disabled: !N || h, onClick: Ce, children: "Guardar" })
        ] })
      ] }),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-cabecera", children: [
        /* @__PURE__ */ a.jsxs("div", { className: "dx-cab-campos", children: [
          /* @__PURE__ */ a.jsx(_o, { terceros: r, valor: s, alCambiar: u, etiqueta: "Proveedor", deshabilitado: !!e.id }),
          /* @__PURE__ */ a.jsx("div", { className: "dx-fila", children: /* @__PURE__ */ a.jsxs("div", { children: [
            /* @__PURE__ */ a.jsx("label", { children: "Fecha del pedido" }),
            /* @__PURE__ */ a.jsx("input", { type: "date", value: d, onChange: (C) => y(C.target.value) })
          ] }) })
        ] }),
        /* @__PURE__ */ a.jsx("div", { className: "dx-ficha", children: R ? /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("strong", { children: R.nombre }),
          /* @__PURE__ */ a.jsx("div", { className: "muted", children: [R.nifFiscal, R.poblacion, R.pais].filter(Boolean).join(" · ") })
        ] }) : /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Elige un proveedor: se aplican sus conceptos (pronto pago, portes…)." }) })
      ] })
    ] }),
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsx(nd, { modo: "compra", lineas: c, alCambiar: m, calculos: E, ivas: [], catalogo: i, sugeridos: _, alElegirArticulo: $ }),
      i.length > 0 && /* @__PURE__ */ a.jsx("div", { style: { marginTop: 10 }, children: /* @__PURE__ */ a.jsx(Fo, { catalogo: i, lista: v, alCambiar: (C) => g(C ?? []), documento: !0 }) })
    ] }),
    /* @__PURE__ */ a.jsxs("div", { className: "dx-pie", children: [
      /* @__PURE__ */ a.jsx("div", { className: "dx-estado", children: p && /* @__PURE__ */ a.jsx("span", { className: "dx-rojo", children: p }) }),
      /* @__PURE__ */ a.jsxs("div", { className: "panel dx-totales", children: [
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ a.jsx("span", { children: "Total del pedido (sin impuestos)" }),
          /* @__PURE__ */ a.jsx("span", { children: M(N == null ? void 0 : N.total) })
        ] }),
        Be !== 0 && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Costes añadidos al almacén" }),
          /* @__PURE__ */ a.jsx("span", { children: M(Be) })
        ] }),
        /* @__PURE__ */ a.jsx("div", { className: "muted", style: { fontSize: 12, marginTop: 6 }, children: "El impuesto se aplica al facturar el pedido." })
      ] })
    ] })
  ] });
}
function ua(e) {
  return /* @__PURE__ */ a.jsxs("div", { className: "panel-head", children: [
    /* @__PURE__ */ a.jsxs("h2", { style: { display: "flex", alignItems: "center", gap: 10 }, children: [
      /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: e.volver, title: "Volver a la lista", children: "←" }),
      e.titulo,
      e.estado && /* @__PURE__ */ a.jsx("span", { className: Po(e.estado), children: e.estado })
    ] }),
    /* @__PURE__ */ a.jsx("div", { className: "dx-acciones", children: e.acciones })
  ] });
}
function _e(e) {
  return /* @__PURE__ */ a.jsxs("div", { className: "dx-dato", children: [
    /* @__PURE__ */ a.jsx("small", { children: e.etiqueta }),
    /* @__PURE__ */ a.jsx("div", { children: e.children })
  ] });
}
function Lr(e) {
  return /* @__PURE__ */ a.jsx("button", { type: "button", className: "dx-enlace", onClick: e.alPulsar, children: e.children });
}
function ca(e) {
  const [t, n] = j.useState(null), [r, l] = j.useState(""), i = j.useCallback(() => {
    e().then(n).catch((o) => l(o.message));
  }, []);
  return j.useEffect(i, [i]), { dato: t, error: r, recargar: i };
}
async function it(e, t, n) {
  try {
    return await e(), n && t(n, "ok"), !0;
  } catch (r) {
    return t(r.message, "err"), !1;
  }
}
function Qp(e) {
  const { api: t, anfitrion: n, navegar: r } = vt(), { dato: l, error: i, recargar: o } = ca(() => t.get(`/facturas/${e.id}`)), [s, u] = j.useState(null), [d, y] = j.useState(!1), [c, m] = j.useState("");
  if (j.useEffect(() => void t.get(`/facturas/${e.id}/saldo`).then(u).catch(() => u(null)), [t, e.id, l]), i) return /* @__PURE__ */ a.jsx("div", { className: "panel", children: /* @__PURE__ */ a.jsx("p", { className: "dx-rojo", children: i }) });
  if (!l) return /* @__PURE__ */ a.jsx("div", { className: "muted", children: "Cargando…" });
  const v = { clienteId: l.clienteId ?? void 0, lineas: l.lineas }, g = l.lineas.reduce((F, p) => F + (p.base - p.margen), 0), N = l.estado === "Emitida";
  return /* @__PURE__ */ a.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsx(
        ua,
        {
          titulo: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            l.tipo === "Rectificativa" ? "Rectificativa" : l.tipo === "Simplificada" ? "Ticket" : "Factura",
            " ",
            /* @__PURE__ */ a.jsx("span", { className: "mono", children: l.numeroCompleto })
          ] }),
          estado: l.estado,
          volver: () => r({ tipo: "factura", pantalla: "lista" }),
          acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => zi(n, `/facturas/${l.id}/pdf`).catch((F) => n.aviso(F.message, "err")), children: "PDF" }),
            l.tipo !== "Simplificada" && l.clienteNif && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => zi(n, `/facturas/${l.id}/facturae.xml`).catch((F) => n.aviso(F.message, "err")), children: "Facturae" }),
            /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "factura", pantalla: "editor", semilla: v }), children: "Duplicar" }),
            N && l.tipo === "Ordinaria" && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "factura", pantalla: "editor", semilla: { ...v, rectificaId: l.id, rectificaNumero: l.numeroCompleto } }), children: "Rectificar" }),
            N && /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", onClick: () => y(!0), children: "Anular" })
          ] })
        }
      ),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ a.jsxs(_e, { etiqueta: "Cliente", children: [
          /* @__PURE__ */ a.jsx("strong", { children: l.clienteNombre }),
          l.clienteNif && /* @__PURE__ */ a.jsx("div", { className: "muted mono", children: l.clienteNif }),
          /* @__PURE__ */ a.jsx("div", { className: "muted", children: [l.clienteCalle, l.clienteCodigoPostal, l.clientePoblacion].filter(Boolean).join(" ") })
        ] }),
        /* @__PURE__ */ a.jsxs(_e, { etiqueta: "Emisión", children: [
          Ve(l.fechaEmision),
          l.fechaOperacion !== l.fechaEmision && /* @__PURE__ */ a.jsxs("div", { className: "muted", children: [
            "Operación ",
            Ve(l.fechaOperacion)
          ] })
        ] }),
        /* @__PURE__ */ a.jsx(_e, { etiqueta: "Vencimiento", children: Ve(l.fechaVencimiento) }),
        /* @__PURE__ */ a.jsxs(_e, { etiqueta: "Cobro", children: [
          s ? s.pendiente <= 0 ? /* @__PURE__ */ a.jsx("span", { className: "pill ok", children: "Cobrada" }) : /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            "Pendiente ",
            /* @__PURE__ */ a.jsx("strong", { children: M(s.pendiente) }),
            s.liquidado > 0 && /* @__PURE__ */ a.jsxs("div", { className: "muted", children: [
              "Cobrado ",
              M(s.liquidado)
            ] })
          ] }) : "—",
          s && s.pendiente > 0 && N && n.irA && /* @__PURE__ */ a.jsx("div", { children: /* @__PURE__ */ a.jsx(Lr, { alPulsar: () => n.irA("cobros"), children: "Registrar cobro" }) })
        ] })
      ] }),
      l.motivoRectificacion && /* @__PURE__ */ a.jsxs("p", { className: "muted", children: [
        "Rectifica: ",
        l.motivoRectificacion,
        l.rectificaFacturaId && /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          " · ",
          /* @__PURE__ */ a.jsx(Lr, { alPulsar: () => r({ tipo: "factura", pantalla: "vista", id: l.rectificaFacturaId }), children: "ver la factura original" })
        ] })
      ] }),
      l.motivoAnulacion && /* @__PURE__ */ a.jsxs("p", { className: "dx-rojo", children: [
        "Anulada: ",
        l.motivoAnulacion
      ] })
    ] }),
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsxs("table", { children: [
        /* @__PURE__ */ a.jsx("thead", { children: /* @__PURE__ */ a.jsxs("tr", { children: [
          /* @__PURE__ */ a.jsx("th", { children: "Descripción" }),
          /* @__PURE__ */ a.jsx("th", { className: "num", children: "Cantidad" }),
          /* @__PURE__ */ a.jsx("th", { className: "num", children: "Precio" }),
          /* @__PURE__ */ a.jsx("th", { className: "num", children: "Dto" }),
          /* @__PURE__ */ a.jsx("th", { children: "Impuesto" }),
          /* @__PURE__ */ a.jsx("th", { className: "num", children: "Importe" }),
          /* @__PURE__ */ a.jsx("th", { className: "num", children: "Margen" })
        ] }) }),
        /* @__PURE__ */ a.jsx("tbody", { children: l.lineas.map((F, p) => /* @__PURE__ */ a.jsxs("tr", { children: [
          /* @__PURE__ */ a.jsxs("td", { children: [
            F.descripcion,
            /* @__PURE__ */ a.jsx(sa, { conceptos: F.conceptos })
          ] }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: ze(F.cantidad) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: M(F.precioUnitario) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: F.porcentajeDescuento ? `${ye(F.porcentajeDescuento)} %` : "" }),
          /* @__PURE__ */ a.jsxs("td", { children: [
            F.codigoIva,
            " · ",
            ye(F.porcentajeIva),
            " %"
          ] }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: /* @__PURE__ */ a.jsx("strong", { children: M(F.base) }) }),
          /* @__PURE__ */ a.jsx("td", { className: "num muted", children: F.costeUnitario || F.costeConceptos ? M(F.margen) : "" })
        ] }, p)) })
      ] }),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-totales-vista", children: [
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Base imponible" }),
          /* @__PURE__ */ a.jsx("span", { children: M(l.baseImponible) })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: l.impuesto === "Igic" ? "IGIC" : "IVA" }),
          /* @__PURE__ */ a.jsx("span", { children: M(l.cuotaIva) })
        ] }),
        !!l.recargoTotal && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Recargo de equivalencia" }),
          /* @__PURE__ */ a.jsx("span", { children: M(l.recargoTotal) })
        ] }),
        !!l.retencionIrpf && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsxs("span", { className: "muted", children: [
            "Retención IRPF (",
            ye(l.porcentajeIrpf),
            " %)"
          ] }),
          /* @__PURE__ */ a.jsxs("span", { children: [
            "−",
            M(l.retencionIrpf)
          ] })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ a.jsx("span", { children: "Total" }),
          /* @__PURE__ */ a.jsx("span", { children: M(l.total) })
        ] }),
        g > 0 && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Margen" }),
          /* @__PURE__ */ a.jsxs("span", { className: "muted", children: [
            M(l.baseImponible - g),
            " (",
            ye(l.baseImponible ? (l.baseImponible - g) / l.baseImponible * 100 : 0),
            " %)"
          ] })
        ] })
      ] }),
      l.mencionFiscal && /* @__PURE__ */ a.jsx("p", { className: "muted", style: { fontSize: 12 }, children: l.mencionFiscal }),
      l.huella && /* @__PURE__ */ a.jsxs("p", { className: "muted mono", style: { fontSize: 11, wordBreak: "break-all" }, children: [
        "VeriFactu · ",
        l.huella
      ] })
    ] }),
    d && /* @__PURE__ */ a.jsxs(
      dn,
      {
        titulo: `Anular ${l.numeroCompleto}`,
        alCerrar: () => y(!1),
        acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => y(!1), children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", disabled: !c.trim(), onClick: async () => await it(() => t.post(`/facturas/${l.id}/anular`, { motivo: c }), n.aviso, "Factura anulada.") && (y(!1), o()), children: "Anular" })
        ] }),
        children: [
          /* @__PURE__ */ a.jsx("p", { className: "muted", style: { marginTop: 0 }, children: "La factura no se borra: queda anulada con un registro de anulación VeriFactu. Si hay que corregir importes, rectifícala en lugar de anularla." }),
          /* @__PURE__ */ a.jsx("label", { children: "Motivo" }),
          /* @__PURE__ */ a.jsx("input", { value: c, onChange: (F) => m(F.target.value), autoFocus: !0 })
        ]
      }
    )
  ] });
}
function Gp(e) {
  const { api: t, anfitrion: n, navegar: r } = vt(), { dato: l, error: i, recargar: o } = ca(() => t.get(`/presupuestos/${e.id}`));
  if (i) return /* @__PURE__ */ a.jsx("div", { className: "panel", children: /* @__PURE__ */ a.jsx("p", { className: "dx-rojo", children: i }) });
  if (!l) return /* @__PURE__ */ a.jsx("div", { className: "muted", children: "Cargando…" });
  const s = l.estado === "Borrador", u = { clienteId: l.clienteId, lineas: l.lineas };
  return /* @__PURE__ */ a.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsx(
        ua,
        {
          titulo: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            "Presupuesto ",
            /* @__PURE__ */ a.jsx("span", { className: "mono", children: l.numeroCompleto })
          ] }),
          estado: l.estado,
          volver: () => r({ tipo: "presupuesto", pantalla: "lista" }),
          acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => zi(n, `/presupuestos/${l.id}/pdf`).catch((d) => n.aviso(d.message, "err")), children: "PDF" }),
            /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "presupuesto", pantalla: "editor", semilla: u }), children: "Duplicar" }),
            s && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "presupuesto", pantalla: "editor", id: l.id, semilla: u }), children: "Editar" }),
            s && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: async () => {
              try {
                const d = await t.post("/pedidos-venta/desde-presupuesto", { presupuestoId: l.id });
                n.aviso("Pedido creado.", "ok"), r({ tipo: "pedido", pantalla: "vista", id: d.id });
              } catch (d) {
                n.aviso(d.message, "err");
              }
            }, children: "Pasar a pedido" }),
            s && /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: async () => {
              try {
                const d = await t.post(`/presupuestos/${l.id}/aceptar`, {});
                n.aviso("Factura emitida.", "ok"), r({ tipo: "factura", pantalla: "vista", id: d.id });
              } catch (d) {
                n.aviso(d.message, "err");
              }
            }, children: "Aceptar y facturar" }),
            s && /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", onClick: async () => await it(() => t.post(`/presupuestos/${l.id}/rechazar`, {}), n.aviso, "Presupuesto rechazado.") && o(), children: "Rechazar" })
          ] })
        }
      ),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ a.jsx(_e, { etiqueta: "Cliente", children: /* @__PURE__ */ a.jsx("strong", { children: l.clienteNombre }) }),
        /* @__PURE__ */ a.jsx(_e, { etiqueta: "Fecha", children: Ve(l.fecha) }),
        /* @__PURE__ */ a.jsx(_e, { etiqueta: "Válido hasta", children: Ve(l.validez) }),
        /* @__PURE__ */ a.jsx(_e, { etiqueta: "Factura", children: l.facturaId ? /* @__PURE__ */ a.jsx(Lr, { alPulsar: () => r({ tipo: "factura", pantalla: "vista", id: l.facturaId }), children: "ver la factura" }) : "—" })
      ] })
    ] }),
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsxs("table", { children: [
        /* @__PURE__ */ a.jsx("thead", { children: /* @__PURE__ */ a.jsxs("tr", { children: [
          /* @__PURE__ */ a.jsx("th", { children: "Descripción" }),
          /* @__PURE__ */ a.jsx("th", { className: "num", children: "Cantidad" }),
          /* @__PURE__ */ a.jsx("th", { className: "num", children: "Precio" }),
          /* @__PURE__ */ a.jsx("th", { className: "num", children: "Dto" }),
          /* @__PURE__ */ a.jsx("th", { children: "Impuesto" }),
          /* @__PURE__ */ a.jsx("th", { className: "num", children: "Importe" })
        ] }) }),
        /* @__PURE__ */ a.jsx("tbody", { children: l.lineas.map((d, y) => /* @__PURE__ */ a.jsxs("tr", { children: [
          /* @__PURE__ */ a.jsxs("td", { children: [
            d.descripcion,
            /* @__PURE__ */ a.jsx(sa, { conceptos: d.conceptos })
          ] }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: ze(d.cantidad) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: M(d.precioUnitario) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: d.porcentajeDescuento ? `${ye(d.porcentajeDescuento)} %` : "" }),
          /* @__PURE__ */ a.jsx("td", { children: d.codigoIva }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: /* @__PURE__ */ a.jsx("strong", { children: M(d.base) }) })
        ] }, y)) })
      ] }),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-totales-vista", children: [
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Base imponible" }),
          /* @__PURE__ */ a.jsx("span", { children: M(l.baseImponible) })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Impuestos" }),
          /* @__PURE__ */ a.jsx("span", { children: M(l.cuotaIva) })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ a.jsx("span", { children: "Total" }),
          /* @__PURE__ */ a.jsx("span", { children: M(l.total) })
        ] })
      ] })
    ] })
  ] });
}
function Kp(e) {
  const { api: t, anfitrion: n, navegar: r } = vt(), { dato: l, error: i, recargar: o } = ca(() => t.get(`/pedidos-venta/${e.id}`)), [s, u] = j.useState([]), [d, y] = j.useState([]), [c, m] = j.useState(null), [v, g] = j.useState(""), [N, F] = j.useState(ht()), [p, f] = j.useState(!1), [h, k] = j.useState(ht()), [z, R] = j.useState("");
  if (j.useEffect(() => void t.get(`/pedidos-venta/${e.id}/albaranes`).then(u).catch(() => u([])), [t, e.id, l]), j.useEffect(() => void t.get("/formas-pago").then((_) => y(_.filter(($) => $.activo))).catch(() => y([])), [t]), i) return /* @__PURE__ */ a.jsx("div", { className: "panel", children: /* @__PURE__ */ a.jsx("p", { className: "dx-rojo", children: i }) });
  if (!l) return /* @__PURE__ */ a.jsx("div", { className: "muted", children: "Cargando…" });
  const T = (l.estado === "Borrador" || l.estado === "Confirmado") && l.lineas.every((_) => _.cantidadServida === 0), D = l.lineas.some((_) => _.pendienteServir > 0), L = l.estado !== "Cancelado" && l.estado !== "Facturado", E = { clienteId: l.clienteId, fecha: l.fecha, lineas: l.lineas };
  return /* @__PURE__ */ a.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsx(
        ua,
        {
          titulo: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            "Pedido de venta ",
            /* @__PURE__ */ a.jsx("span", { className: "mono", children: l.numeroCompleto })
          ] }),
          estado: l.estado,
          volver: () => r({ tipo: "pedido", pantalla: "lista" }),
          acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "pedido", pantalla: "editor", semilla: { ...E, fecha: void 0 } }), children: "Duplicar" }),
            T && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "pedido", pantalla: "editor", id: l.id, semilla: E }), children: "Editar" }),
            l.estado === "Borrador" && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: async () => await it(() => t.post(`/pedidos-venta/${l.id}/confirmar`), n.aviso, "Pedido confirmado.") && o(), children: "Confirmar" }),
            L && l.estado !== "Borrador" && D && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => m(Object.fromEntries(l.lineas.map((_) => [_.id, _.pendienteServir]))), children: "Entregar (albarán)" }),
            L && l.estado !== "Borrador" && /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: () => f(!0), children: "Facturar" }),
            L && /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", onClick: async () => window.confirm("¿Cancelar el pedido?") && await it(() => t.post(`/pedidos-venta/${l.id}/cancelar`), n.aviso, "Pedido cancelado.") && o(), children: "Cancelar" })
          ] })
        }
      ),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ a.jsx(_e, { etiqueta: "Cliente", children: /* @__PURE__ */ a.jsx("strong", { children: l.clienteNombre }) }),
        /* @__PURE__ */ a.jsx(_e, { etiqueta: "Fecha", children: Ve(l.fecha) }),
        /* @__PURE__ */ a.jsx(_e, { etiqueta: "Viene de", children: l.presupuestoOrigenId ? /* @__PURE__ */ a.jsx(Lr, { alPulsar: () => r({ tipo: "presupuesto", pantalla: "vista", id: l.presupuestoOrigenId }), children: "presupuesto" }) : "—" }),
        /* @__PURE__ */ a.jsx(_e, { etiqueta: "Factura", children: l.facturaId ? /* @__PURE__ */ a.jsx(Lr, { alPulsar: () => r({ tipo: "factura", pantalla: "vista", id: l.facturaId }), children: "ver la factura" }) : "—" })
      ] })
    ] }),
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsxs("table", { children: [
        /* @__PURE__ */ a.jsx("thead", { children: /* @__PURE__ */ a.jsxs("tr", { children: [
          /* @__PURE__ */ a.jsx("th", { children: "Descripción" }),
          /* @__PURE__ */ a.jsx("th", { className: "num", children: "Pedido" }),
          /* @__PURE__ */ a.jsx("th", { className: "num", children: "Servido" }),
          /* @__PURE__ */ a.jsx("th", { className: "num", children: "Pendiente" }),
          /* @__PURE__ */ a.jsx("th", { className: "num", children: "Precio" }),
          /* @__PURE__ */ a.jsx("th", { className: "num", children: "Dto" }),
          /* @__PURE__ */ a.jsx("th", { className: "num", children: "Importe" })
        ] }) }),
        /* @__PURE__ */ a.jsx("tbody", { children: l.lineas.map((_) => /* @__PURE__ */ a.jsxs("tr", { children: [
          /* @__PURE__ */ a.jsxs("td", { children: [
            _.descripcion,
            /* @__PURE__ */ a.jsx(sa, { conceptos: _.conceptos })
          ] }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: ze(_.cantidad) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: ze(_.cantidadServida) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: _.pendienteServir > 0 ? /* @__PURE__ */ a.jsx("strong", { children: ze(_.pendienteServir) }) : "—" }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: M(_.precioUnitario) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: _.porcentajeDescuento ? `${ye(_.porcentajeDescuento)} %` : "" }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: /* @__PURE__ */ a.jsx("strong", { children: M(_.base) }) })
        ] }, _.id)) })
      ] }),
      /* @__PURE__ */ a.jsx("div", { className: "dx-totales-vista", children: /* @__PURE__ */ a.jsxs("div", { className: "dx-tot dx-grande", children: [
        /* @__PURE__ */ a.jsx("span", { children: "Total (sin impuestos)" }),
        /* @__PURE__ */ a.jsx("span", { children: M(l.total) })
      ] }) })
    ] }),
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsx("div", { className: "panel-head", children: /* @__PURE__ */ a.jsx("h2", { children: "Albaranes de entrega" }) }),
      s.length ? /* @__PURE__ */ a.jsxs("table", { children: [
        /* @__PURE__ */ a.jsx("thead", { children: /* @__PURE__ */ a.jsxs("tr", { children: [
          /* @__PURE__ */ a.jsx("th", { children: "Número" }),
          /* @__PURE__ */ a.jsx("th", { children: "Fecha" }),
          /* @__PURE__ */ a.jsx("th", { children: "Referencia" }),
          /* @__PURE__ */ a.jsx("th", { children: "Líneas" }),
          /* @__PURE__ */ a.jsx("th", {})
        ] }) }),
        /* @__PURE__ */ a.jsx("tbody", { children: s.map((_) => /* @__PURE__ */ a.jsxs("tr", { children: [
          /* @__PURE__ */ a.jsxs("td", { className: "mono", children: [
            /* @__PURE__ */ a.jsx("strong", { children: _.numeroCompleto }),
            " ",
            _.anulado && /* @__PURE__ */ a.jsx("span", { className: "pill neg", title: _.motivoAnulacion ?? "", children: "Anulado" })
          ] }),
          /* @__PURE__ */ a.jsx("td", { children: Ve(_.fecha) }),
          /* @__PURE__ */ a.jsx("td", { className: "muted", children: _.referencia }),
          /* @__PURE__ */ a.jsx("td", { className: "muted", children: _.lineas.map(($) => `${ze($.cantidad)} × ${$.descripcion}`).join(" · ") }),
          /* @__PURE__ */ a.jsx("td", { className: "right", children: !_.anulado && l.estado !== "Facturado" && /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", onClick: async () => {
            const $ = window.prompt("Motivo de la anulación del albarán:");
            $ !== null && await it(() => t.post(`/pedidos-venta/${l.id}/albaranes/${_.id}/anular`, { motivo: $ || null }), n.aviso, "Albarán anulado.") && o();
          }, children: "Anular" }) })
        ] }, _.id)) })
      ] }) : /* @__PURE__ */ a.jsx("p", { className: "muted", style: { margin: 0 }, children: "Sin entregas todavía." })
    ] }),
    c && /* @__PURE__ */ a.jsxs(
      dn,
      {
        titulo: "Entrega (albarán de venta)",
        alCerrar: () => m(null),
        ancho: 640,
        acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => m(null), children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: async () => await it(() => t.post(`/pedidos-venta/${l.id}/entregar`, { fecha: N, referencia: v || null, lineas: Object.entries(c).filter(([, _]) => _ > 0).map(([_, $]) => ({ lineaPedidoId: _, cantidad: $ })) }), n.aviso, "Albarán creado.") && (m(null), o()), children: "Crear albarán" })
        ] }),
        children: [
          /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Fecha" }),
              /* @__PURE__ */ a.jsx("input", { type: "date", value: N, onChange: (_) => F(_.target.value) })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Referencia" }),
              /* @__PURE__ */ a.jsx("input", { value: v, onChange: (_) => g(_.target.value) })
            ] })
          ] }),
          /* @__PURE__ */ a.jsxs("table", { style: { marginTop: 10 }, children: [
            /* @__PURE__ */ a.jsx("thead", { children: /* @__PURE__ */ a.jsxs("tr", { children: [
              /* @__PURE__ */ a.jsx("th", { children: "Línea" }),
              /* @__PURE__ */ a.jsx("th", { className: "num", children: "Pendiente" }),
              /* @__PURE__ */ a.jsx("th", { className: "num", style: { width: 120 }, children: "Entregar" })
            ] }) }),
            /* @__PURE__ */ a.jsx("tbody", { children: l.lineas.filter((_) => _.pendienteServir > 0).map((_) => /* @__PURE__ */ a.jsxs("tr", { children: [
              /* @__PURE__ */ a.jsx("td", { children: _.descripcion }),
              /* @__PURE__ */ a.jsx("td", { className: "num", children: ze(_.pendienteServir) }),
              /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx("input", { className: "num", type: "number", step: "0.001", value: c[_.id] ?? 0, onChange: ($) => m({ ...c, [_.id]: Number($.target.value) }) }) })
            ] }, _.id)) })
          ] })
        ]
      }
    ),
    p && /* @__PURE__ */ a.jsxs(
      dn,
      {
        titulo: `Facturar el pedido ${l.numeroCompleto}`,
        alCerrar: () => f(!1),
        acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => f(!1), children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: async () => {
            try {
              const _ = await t.post(`/pedidos-venta/${l.id}/facturar`, { fechaEmision: h, formaPagoId: z || null });
              n.aviso("Factura emitida.", "ok"), r({ tipo: "factura", pantalla: "vista", id: _.id });
            } catch (_) {
              n.aviso(_.message, "err");
            }
          }, children: "Emitir factura" })
        ] }),
        children: [
          /* @__PURE__ */ a.jsx("p", { className: "muted", style: { marginTop: 0 }, children: "Se factura el pedido completo con sus precios y conceptos. La factura queda numerada y encadenada en VeriFactu." }),
          /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Fecha de emisión" }),
              /* @__PURE__ */ a.jsx("input", { type: "date", value: h, onChange: (_) => k(_.target.value) })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Forma de pago" }),
              /* @__PURE__ */ a.jsxs("select", { value: z, onChange: (_) => R(_.target.value), children: [
                /* @__PURE__ */ a.jsx("option", { value: "", children: "La del cliente" }),
                d.map((_) => /* @__PURE__ */ a.jsx("option", { value: _.id, children: _.nombre }, _.id))
              ] })
            ] })
          ] })
        ]
      }
    )
  ] });
}
function qp(e) {
  const { api: t, anfitrion: n, navegar: r } = vt(), { dato: l, error: i, recargar: o } = ca(() => t.get(`/compras/pedidos/${e.id}`)), [s, u] = j.useState([]), [d, y] = j.useState([]), [c, m] = j.useState([]), [v, g] = j.useState(null), [N, F] = j.useState(""), [p, f] = j.useState(""), [h, k] = j.useState(ht()), [z, R] = j.useState(!1), [T, D] = j.useState("IVA21"), [L, E] = j.useState(0), [_, $] = j.useState(""), [Ce, Be] = j.useState(ht());
  if (j.useEffect(() => void t.get(`/compras/pedidos/${e.id}/albaranes`).then(u).catch(() => u([])), [t, e.id, l]), j.useEffect(() => {
    t.get("/inventario/almacenes").then((x) => (y(x), x[0] && F(x[0].id))).catch(() => y([])), t.get("/tipos-iva").then((x) => m(x.filter((S) => S.activo))).catch(() => m([]));
  }, [t]), i) return /* @__PURE__ */ a.jsx("div", { className: "panel", children: /* @__PURE__ */ a.jsx("p", { className: "dx-rojo", children: i }) });
  if (!l) return /* @__PURE__ */ a.jsx("div", { className: "muted", children: "Cargando…" });
  const be = (l.estado === "Borrador" || l.estado === "Confirmado") && l.lineas.every((x) => x.cantidadRecibida === 0 && x.cantidadFacturada === 0) && !l.empresaOrigenId, de = l.estado !== "Cancelado" && l.estado !== "Facturado", Ne = l.lineas.some((x) => x.pendienteRecibir > 0), C = l.lineas.reduce((x, S) => x + S.costeConceptos, 0);
  return /* @__PURE__ */ a.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsx(
        ua,
        {
          titulo: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            "Pedido de compra ",
            /* @__PURE__ */ a.jsx("span", { className: "mono", children: l.numeroCompleto })
          ] }),
          estado: l.estado,
          volver: () => r({ tipo: "compra", pantalla: "lista" }),
          acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            !l.empresaOrigenId && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "compra", pantalla: "editor", semilla: { ...l, fecha: ht() } }), children: "Duplicar" }),
            be && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "compra", pantalla: "editor", id: l.id, semilla: l }), children: "Editar" }),
            l.estado === "Borrador" && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: async () => await it(() => t.post(`/compras/pedidos/${l.id}/confirmar`), n.aviso, "Pedido confirmado.") && o(), children: "Confirmar" }),
            de && l.estado !== "Borrador" && Ne && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => g(Object.fromEntries(l.lineas.map((x) => [x.id, { cantidad: x.pendienteRecibir, lote: "" }]))), children: "Recibir mercancía" }),
            de && l.estado !== "Borrador" && /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: () => R(!0), children: "Facturar" }),
            de && !l.empresaOrigenId && /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", onClick: async () => window.confirm("¿Cancelar el pedido?") && await it(() => t.post(`/compras/pedidos/${l.id}/cancelar`), n.aviso, "Pedido cancelado.") && o(), children: "Cancelar" })
          ] })
        }
      ),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ a.jsx(_e, { etiqueta: "Proveedor", children: /* @__PURE__ */ a.jsx("strong", { children: l.proveedorTexto }) }),
        /* @__PURE__ */ a.jsx(_e, { etiqueta: "Fecha", children: Ve(l.fecha) }),
        /* @__PURE__ */ a.jsx(_e, { etiqueta: "Total", children: M(l.total) }),
        /* @__PURE__ */ a.jsx(_e, { etiqueta: "Costes añadidos", children: C ? M(C) : "—" })
      ] }),
      l.empresaOrigenId && /* @__PURE__ */ a.jsx("p", { className: "muted", children: "Traspaso de otra empresa del grupo: se gestiona desde el documento de venta de origen." })
    ] }),
    /* @__PURE__ */ a.jsx("div", { className: "panel", children: /* @__PURE__ */ a.jsxs("table", { children: [
      /* @__PURE__ */ a.jsx("thead", { children: /* @__PURE__ */ a.jsxs("tr", { children: [
        /* @__PURE__ */ a.jsx("th", { children: "Descripción" }),
        /* @__PURE__ */ a.jsx("th", { className: "num", children: "Pedido" }),
        /* @__PURE__ */ a.jsx("th", { className: "num", children: "Recibido" }),
        /* @__PURE__ */ a.jsx("th", { className: "num", children: "Pendiente" }),
        /* @__PURE__ */ a.jsx("th", { className: "num", children: "Precio" }),
        /* @__PURE__ */ a.jsx("th", { className: "num", children: "Importe" }),
        /* @__PURE__ */ a.jsx("th", { className: "num", children: "Coste entrada" })
      ] }) }),
      /* @__PURE__ */ a.jsx("tbody", { children: l.lineas.map((x) => /* @__PURE__ */ a.jsxs("tr", { children: [
        /* @__PURE__ */ a.jsxs("td", { children: [
          x.descripcion,
          /* @__PURE__ */ a.jsx(sa, { conceptos: x.conceptos })
        ] }),
        /* @__PURE__ */ a.jsx("td", { className: "num", children: ze(x.cantidad) }),
        /* @__PURE__ */ a.jsx("td", { className: "num", children: ze(x.cantidadRecibida) }),
        /* @__PURE__ */ a.jsx("td", { className: "num", children: x.pendienteRecibir > 0 ? /* @__PURE__ */ a.jsx("strong", { children: ze(x.pendienteRecibir) }) : "—" }),
        /* @__PURE__ */ a.jsx("td", { className: "num", children: M(x.precioUnitario) }),
        /* @__PURE__ */ a.jsx("td", { className: "num", children: /* @__PURE__ */ a.jsx("strong", { children: M(x.importe) }) }),
        /* @__PURE__ */ a.jsxs("td", { className: "num muted", children: [
          M(x.costeUnitarioEntrada),
          "/ud"
        ] })
      ] }, x.id)) })
    ] }) }),
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsx("div", { className: "panel-head", children: /* @__PURE__ */ a.jsx("h2", { children: "Albaranes de recepción" }) }),
      s.length ? /* @__PURE__ */ a.jsxs("table", { children: [
        /* @__PURE__ */ a.jsx("thead", { children: /* @__PURE__ */ a.jsxs("tr", { children: [
          /* @__PURE__ */ a.jsx("th", { children: "Número" }),
          /* @__PURE__ */ a.jsx("th", { children: "Fecha" }),
          /* @__PURE__ */ a.jsx("th", { children: "Referencia" }),
          /* @__PURE__ */ a.jsx("th", { children: "Almacén" }),
          /* @__PURE__ */ a.jsx("th", { children: "Líneas" }),
          /* @__PURE__ */ a.jsx("th", {})
        ] }) }),
        /* @__PURE__ */ a.jsx("tbody", { children: s.map((x) => {
          var S;
          return /* @__PURE__ */ a.jsxs("tr", { children: [
            /* @__PURE__ */ a.jsxs("td", { className: "mono", children: [
              /* @__PURE__ */ a.jsx("strong", { children: x.numeroCompleto }),
              " ",
              x.anulado && /* @__PURE__ */ a.jsx("span", { className: "pill neg", title: x.motivoAnulacion ?? "", children: "Anulado" })
            ] }),
            /* @__PURE__ */ a.jsx("td", { children: Ve(x.fecha) }),
            /* @__PURE__ */ a.jsx("td", { className: "muted", children: x.referencia }),
            /* @__PURE__ */ a.jsx("td", { className: "muted", children: ((S = d.find((A) => A.id === x.almacenId)) == null ? void 0 : S.nombre) ?? "—" }),
            /* @__PURE__ */ a.jsx("td", { className: "muted", children: x.lineas.map((A) => `${ze(A.cantidad)} × ${A.descripcion}`).join(" · ") }),
            /* @__PURE__ */ a.jsx("td", { className: "right", children: !x.anulado && l.estado !== "Facturado" && /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", onClick: async () => {
              const A = window.prompt("Motivo de la anulación del albarán:");
              A !== null && await it(() => t.post(`/compras/pedidos/${l.id}/albaranes/${x.id}/anular`, { motivo: A || null }), n.aviso, "Albarán anulado.") && o();
            }, children: "Anular" }) })
          ] }, x.id);
        }) })
      ] }) : /* @__PURE__ */ a.jsx("p", { className: "muted", style: { margin: 0 }, children: "Sin recepciones todavía." })
    ] }),
    v && /* @__PURE__ */ a.jsxs(
      dn,
      {
        titulo: "Recepción de mercancía",
        alCerrar: () => g(null),
        ancho: 680,
        acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => g(null), children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: async () => await it(() => t.post(`/compras/pedidos/${l.id}/recibir`, { fecha: h, referencia: p || null, almacenId: N || null, lineas: Object.entries(v).filter(([, x]) => x.cantidad > 0).map(([x, S]) => ({ lineaPedidoId: x, cantidad: S.cantidad, lote: S.lote || null })) }), n.aviso, "Recepción registrada.") && (g(null), o()), children: "Registrar recepción" })
        ] }),
        children: [
          /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Fecha" }),
              /* @__PURE__ */ a.jsx("input", { type: "date", value: h, onChange: (x) => k(x.target.value) })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Albarán del proveedor" }),
              /* @__PURE__ */ a.jsx("input", { value: p, onChange: (x) => f(x.target.value) })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Almacén" }),
              /* @__PURE__ */ a.jsxs("select", { value: N, onChange: (x) => F(x.target.value), children: [
                /* @__PURE__ */ a.jsx("option", { value: "", children: "Sin entrada en almacén" }),
                d.map((x) => /* @__PURE__ */ a.jsx("option", { value: x.id, children: x.nombre }, x.id))
              ] })
            ] })
          ] }),
          /* @__PURE__ */ a.jsxs("table", { style: { marginTop: 10 }, children: [
            /* @__PURE__ */ a.jsx("thead", { children: /* @__PURE__ */ a.jsxs("tr", { children: [
              /* @__PURE__ */ a.jsx("th", { children: "Línea" }),
              /* @__PURE__ */ a.jsx("th", { className: "num", children: "Pendiente" }),
              /* @__PURE__ */ a.jsx("th", { className: "num", style: { width: 110 }, children: "Recibir" }),
              /* @__PURE__ */ a.jsx("th", { style: { width: 130 }, children: "Lote" })
            ] }) }),
            /* @__PURE__ */ a.jsx("tbody", { children: l.lineas.filter((x) => x.pendienteRecibir > 0).map((x) => {
              var S, A;
              return /* @__PURE__ */ a.jsxs("tr", { children: [
                /* @__PURE__ */ a.jsx("td", { children: x.descripcion }),
                /* @__PURE__ */ a.jsx("td", { className: "num", children: ze(x.pendienteRecibir) }),
                /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx("input", { className: "num", type: "number", step: "0.001", value: ((S = v[x.id]) == null ? void 0 : S.cantidad) ?? 0, onChange: (H) => g({ ...v, [x.id]: { ...v[x.id], cantidad: Number(H.target.value) } }) }) }),
                /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx("input", { value: ((A = v[x.id]) == null ? void 0 : A.lote) ?? "", onChange: (H) => g({ ...v, [x.id]: { ...v[x.id], lote: H.target.value } }) }) })
              ] }, x.id);
            }) })
          ] })
        ]
      }
    ),
    z && /* @__PURE__ */ a.jsxs(
      dn,
      {
        titulo: `Facturar el pedido ${l.numeroCompleto}`,
        alCerrar: () => R(!1),
        acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => R(!1), children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: async () => await it(() => t.post(`/compras/pedidos/${l.id}/facturar`, { codigoIva: T, porcentajeIrpf: L, numeroFactura: _ || null, fechaFactura: Ce }), n.aviso, "Factura del proveedor registrada como gasto.") && (R(!1), o()), children: "Registrar factura" })
        ] }),
        children: [
          /* @__PURE__ */ a.jsxs("p", { className: "muted", style: { marginTop: 0 }, children: [
            "Registra la factura del proveedor por el total del pedido (",
            M(l.total),
            ") como gasto, con su asiento si la contabilidad es automática."
          ] }),
          /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Nº de factura del proveedor" }),
              /* @__PURE__ */ a.jsx("input", { value: _, onChange: (x) => $(x.target.value), autoFocus: !0 })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Fecha de la factura" }),
              /* @__PURE__ */ a.jsx("input", { type: "date", value: Ce, onChange: (x) => Be(x.target.value) })
            ] })
          ] }),
          /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Impuesto" }),
              /* @__PURE__ */ a.jsx("select", { value: T, onChange: (x) => D(x.target.value), children: c.map((x) => /* @__PURE__ */ a.jsx("option", { value: x.codigo, children: x.nombre }, x.codigo)) })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Retención IRPF %" }),
              /* @__PURE__ */ a.jsx("input", { type: "number", step: "0.01", value: L, onChange: (x) => E(Number(x.target.value)) })
            ] })
          ] })
        ]
      }
    )
  ] });
}
const Aa = (e = "") => ({ clave: Vr(), descripcion: "", cuentaGasto: "", base: 0, codigoIva: e, porcentajeIva: null, porcentajeDeducible: 100 }), bp = (e) => e === "InversionSujetoPasivo" || e === "Intracomunitario";
function Yp(e) {
  const { api: t, anfitrion: n } = vt(), r = e.semilla, [l, i] = j.useState([]), [o, s] = j.useState([]), [u, d] = j.useState([]), [y, c] = j.useState([]), [m, v] = j.useState((r == null ? void 0 : r.proveedorId) ?? ""), [g, N] = j.useState(e.id ? (r == null ? void 0 : r.numeroFactura) ?? "" : ""), [F, p] = j.useState((r == null ? void 0 : r.fechaFactura) ?? ht()), [f, h] = j.useState(e.id ? (r == null ? void 0 : r.fecha) ?? ht() : ht()), [k, z] = j.useState(r != null && r.concepto && !r.concepto.startsWith("Factura ") ? r.concepto : ""), [R, T] = j.useState((r == null ? void 0 : r.porcentajeIrpf) ?? 0), [D, L] = j.useState(""), [E, _] = j.useState(((r == null ? void 0 : r.recargoTotal) ?? 0) > 0), $ = !!(r != null && r.esRectificativa), [Ce, Be] = j.useState((r == null ? void 0 : r.numeroRectificado) ?? ""), [be, de] = j.useState((r == null ? void 0 : r.fechaRectificada) ?? ""), [Ne, C] = j.useState((r == null ? void 0 : r.motivoRectificacion) ?? ""), [x, S] = j.useState(!1), [A, H] = j.useState((r == null ? void 0 : r.afectacion) ?? "Comun"), [G, me] = j.useState(
    () => {
      var w;
      return (w = r == null ? void 0 : r.lineas) != null && w.length ? r.lineas.map((B) => ({ clave: Vr(), descripcion: B.descripcion ?? "", cuentaGasto: B.cuentaGasto ?? "", base: B.base, codigoIva: B.codigoIva, porcentajeIva: B.autoliquidada ? B.porcentajeIva : null, porcentajeDeducible: B.porcentajeDeducible })) : [Aa()];
    }
  ), [ge, te] = j.useState(e.id && (r != null && r.vencimientos) && r.vencimientos.length > 1 ? r.vencimientos : null), [U, Wn] = j.useState(null), [Yt, Qn] = j.useState(""), [ae, It] = j.useState(!1), Hr = oa();
  j.useEffect(() => {
    t.get("/proveedores").then(i).catch(() => i([])), t.get("/tipos-iva").then((w) => s(w.filter((B) => B.activo))).catch(() => s([])), t.get("/formas-pago").then((w) => d(w.filter((B) => B.activo))).catch(() => d([])), t.get("/empresas/actual").then((w) => {
      w.regimenIva === "RecargoEquivalencia" && (S(!0), r || _(!0));
    }).catch(() => {
    }), t.get("/contabilidad/cuentas").then((w) => c(w.filter((B) => B.codigo.startsWith("6") || B.codigo.startsWith("2")))).catch(() => c([]));
  }, [t]);
  const se = l.find((w) => w.id === m);
  j.useEffect(() => {
    se != null && se.formaPagoDefectoId && !D && L(se.formaPagoDefectoId);
  }, [se]);
  const Xt = j.useMemo(
    () => ({
      proveedorId: m || null,
      proveedorTexto: (se == null ? void 0 : se.nombre) ?? null,
      numeroFactura: g.trim() || null,
      fechaFactura: F || null,
      fecha: f,
      concepto: k.trim() || null,
      porcentajeIrpf: R,
      formaPagoId: D || null,
      recargoEquivalencia: E,
      afectacion: A,
      baseImponible: 0,
      lineas: G.filter((w) => w.base !== 0).map((w) => ({
        base: w.base,
        codigoIva: w.codigoIva || null,
        descripcion: w.descripcion.trim() || null,
        porcentajeIva: w.porcentajeIva,
        porcentajeDeducible: w.porcentajeDeducible,
        cuentaGasto: w.cuentaGasto.trim() || null
      })),
      vencimientos: ge,
      rectificaGastoId: $ ? (r == null ? void 0 : r.rectificaGastoId) ?? null : null,
      numeroRectificado: $ && Ce.trim() || null,
      fechaRectificada: $ && be || null,
      motivoRectificacion: $ && Ne.trim() || null
    }),
    [m, se, g, F, f, k, R, D, E, A, G, ge, $, r, Ce, be, Ne]
  ), Gn = Br(Xt, 350);
  j.useEffect(() => {
    if (!Gn.lineas.length) {
      Wn(null), Qn("Añade al menos una línea con base.");
      return;
    }
    const w = Hr();
    t.post("/gastos/simular", Gn).then((B) => w() && (Wn(B), Qn(""))).catch((B) => w() && (Wn(null), Qn(B.message)));
  }, [Gn, t]);
  const Pt = (w, B) => me((fe) => fe.map((ue) => ue.clave === w ? { ...ue, ...B } : ue)), Wr = (w) => o.find((B) => B.codigo === w), Kn = (w) => {
    var B;
    return (B = U == null ? void 0 : U.lineas) == null ? void 0 : B[G.filter((fe) => fe.base !== 0).indexOf(w)];
  };
  function qn(w) {
    if (!U) return;
    const B = /* @__PURE__ */ new Date((F || f) + "T00:00:00"), fe = jl(U.total / w);
    te(Array.from({ length: w }, (ue, K) => {
      const _t = new Date(B);
      return _t.setMonth(_t.getMonth() + K + 1), { fecha: _t.toISOString().slice(0, 10), importe: K === w - 1 ? jl(U.total - fe * (w - 1)) : fe };
    }));
  }
  async function da() {
    It(!0);
    try {
      const w = e.id ? await t.put(`/gastos/${e.id}`, Xt) : await t.post("/gastos", Xt);
      n.aviso(e.id ? "Factura corregida." : "Factura registrada.", "ok"), w.avisoRiesgo && n.aviso(w.avisoRiesgo, "err"), e.alGuardar(w.id);
    } catch (w) {
      n.aviso(w.message, "err");
    } finally {
      It(!1);
    }
  }
  const bn = jl((ge ?? []).reduce((w, B) => w + (Number(B.importe) || 0), 0));
  return /* @__PURE__ */ a.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsxs("div", { className: "panel-head", children: [
        /* @__PURE__ */ a.jsx("h2", { children: e.id ? `Corregir factura ${(r == null ? void 0 : r.numeroFactura) ?? ""}` : $ ? "Rectificativa / abono del proveedor" : "Nueva factura de proveedor" }),
        /* @__PURE__ */ a.jsxs("div", { style: { display: "flex", gap: 8 }, children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: e.alCancelar, children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", disabled: !U || ae, onClick: da, children: e.id ? "Guardar corrección" : $ ? "Registrar abono" : "Registrar factura" })
        ] })
      ] }),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-cabecera", children: [
        /* @__PURE__ */ a.jsxs("div", { className: "dx-cab-campos", children: [
          /* @__PURE__ */ a.jsx(_o, { terceros: l, valor: m, alCambiar: v, etiqueta: "Proveedor" }),
          /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Nº de factura del proveedor" }),
              /* @__PURE__ */ a.jsx("input", { value: g, onChange: (w) => N(w.target.value), placeholder: "F-2026/0117" })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Fecha de la factura" }),
              /* @__PURE__ */ a.jsx("input", { type: "date", value: F, onChange: (w) => p(w.target.value) })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Fecha de registro" }),
              /* @__PURE__ */ a.jsx("input", { type: "date", value: f, onChange: (w) => h(w.target.value), title: "La del asiento y la del periodo de IVA en que se deduce" })
            ] })
          ] }),
          /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Forma de pago" }),
              /* @__PURE__ */ a.jsxs("select", { value: D, onChange: (w) => L(w.target.value), children: [
                /* @__PURE__ */ a.jsx("option", { value: "", children: "Contado (vence en la fecha de factura)" }),
                u.map((w) => /* @__PURE__ */ a.jsx("option", { value: w.id, children: w.nombre }, w.id))
              ] })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Retención IRPF %" }),
              /* @__PURE__ */ a.jsx("input", { type: "number", step: "0.01", value: R, onChange: (w) => T(Number(w.target.value)) })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Prorrata especial" }),
              /* @__PURE__ */ a.jsxs("select", { value: A, onChange: (w) => H(w.target.value), children: [
                /* @__PURE__ */ a.jsx("option", { value: "Comun", children: "Uso común" }),
                /* @__PURE__ */ a.jsx("option", { value: "ConDerecho", children: "Solo operaciones con derecho" }),
                /* @__PURE__ */ a.jsx("option", { value: "SinDerecho", children: "Solo operaciones exentas" })
              ] })
            ] })
          ] }),
          /* @__PURE__ */ a.jsx("div", { className: "dx-fila", children: /* @__PURE__ */ a.jsxs("div", { style: { gridColumn: "1 / -1" }, children: [
            /* @__PURE__ */ a.jsx("label", { children: "Concepto (vacío: «Factura nº»)" }),
            /* @__PURE__ */ a.jsx("input", { value: k, onChange: (w) => z(w.target.value) })
          ] }) }),
          $ && /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Factura que rectifica" }),
              /* @__PURE__ */ a.jsx("input", { value: Ce, disabled: !!(r != null && r.rectificaGastoId), onChange: (w) => Be(w.target.value) })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Fecha de la rectificada" }),
              /* @__PURE__ */ a.jsx("input", { type: "date", value: be, disabled: !!(r != null && r.rectificaGastoId), onChange: (w) => de(w.target.value) })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Motivo" }),
              /* @__PURE__ */ a.jsx("input", { value: Ne, onChange: (w) => C(w.target.value), placeholder: "Devolución, descuento posterior, error de precio…" })
            ] })
          ] }),
          $ && /* @__PURE__ */ a.jsx("div", { className: "muted", children: "Las bases del abono van en negativo; el asiento y el SII (R1, por diferencias) salen con el signo." }),
          /* @__PURE__ */ a.jsxs("label", { className: "dx-check", children: [
            /* @__PURE__ */ a.jsx("input", { type: "checkbox", checked: E, onChange: (w) => _(w.target.checked) }),
            " El proveedor me cobra recargo de equivalencia"
          ] }),
          x && /* @__PURE__ */ a.jsx("div", { className: "muted", children: "La empresa está en recargo de equivalencia: el IVA y el recargo soportados son coste (no se deducen)." })
        ] }),
        /* @__PURE__ */ a.jsx("div", { className: "dx-ficha", children: se ? /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("strong", { children: se.nombre }),
          /* @__PURE__ */ a.jsx("div", { className: "muted", children: [se.nifFiscal, se.poblacion, se.pais].filter(Boolean).join(" · ") }),
          !se.nifFiscal && /* @__PURE__ */ a.jsx("div", { className: "dx-aviso", children: "⚠ Sin NIF en la ficha: hace falta para el libro de IVA, el 347 y el SII." }),
          (U == null ? void 0 : U.avisoRiesgo) && /* @__PURE__ */ a.jsxs("div", { className: "dx-aviso", children: [
            "⚠ ",
            U.avisoRiesgo
          ] })
        ] }) : /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Elige el proveedor: su NIF va al libro de IVA y al SII, y el número de factura no se puede repetir." }) })
      ] })
    ] }),
    /* @__PURE__ */ a.jsxs("div", { className: "panel dx-rejilla", children: [
      /* @__PURE__ */ a.jsxs("table", { children: [
        /* @__PURE__ */ a.jsx("thead", { children: /* @__PURE__ */ a.jsxs("tr", { children: [
          /* @__PURE__ */ a.jsx("th", { children: "Descripción" }),
          /* @__PURE__ */ a.jsx("th", { style: { width: 130 }, children: "Cuenta de gasto" }),
          /* @__PURE__ */ a.jsx("th", { className: "num", style: { width: 120 }, children: "Base" }),
          /* @__PURE__ */ a.jsx("th", { style: { width: 200 }, children: "Impuesto" }),
          /* @__PURE__ */ a.jsx("th", { className: "num", style: { width: 80 }, children: "% IVA" }),
          /* @__PURE__ */ a.jsx("th", { className: "num", style: { width: 90 }, children: "% deduc." }),
          /* @__PURE__ */ a.jsx("th", { className: "num", style: { width: 110 }, children: "Cuota" }),
          /* @__PURE__ */ a.jsx("th", { style: { width: 40 } })
        ] }) }),
        /* @__PURE__ */ a.jsx("tbody", { children: G.map((w, B) => {
          const fe = Wr(w.codigoIva), ue = Kn(w);
          return /* @__PURE__ */ a.jsxs("tr", { className: B % 2 ? "dx-par" : "", children: [
            /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx("input", { value: w.descripcion, onChange: (K) => Pt(w.clave, { descripcion: K.target.value }), placeholder: "Qué se compra" }) }),
            /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx("input", { list: "dx-cuentas-gasto", value: w.cuentaGasto, onChange: (K) => Pt(w.clave, { cuentaGasto: K.target.value }), placeholder: "la de la regla" }) }),
            /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx("input", { className: "num", type: "number", step: "0.01", value: w.base || "", onChange: (K) => Pt(w.clave, { base: Number(K.target.value) }) }) }),
            /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsxs("select", { value: w.codigoIva, onChange: (K) => Pt(w.clave, { codigoIva: K.target.value, porcentajeIva: null }), children: [
              /* @__PURE__ */ a.jsx("option", { value: "", children: "General" }),
              o.map((K) => /* @__PURE__ */ a.jsx("option", { value: K.codigo, children: K.nombre }, K.codigo))
            ] }) }),
            /* @__PURE__ */ a.jsx("td", { children: bp(fe == null ? void 0 : fe.clase) ? /* @__PURE__ */ a.jsx("input", { className: "num", type: "number", step: "0.01", value: w.porcentajeIva ?? "", placeholder: "21", title: "Tipo que autoliquidas", onChange: (K) => Pt(w.clave, { porcentajeIva: K.target.value === "" ? null : Number(K.target.value) }) }) : /* @__PURE__ */ a.jsx("span", { className: "muted", children: ue ? `${ye(ue.porcentajeIva)} %` : "" }) }),
            /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx("input", { className: "num", type: "number", step: "1", min: 0, max: 100, value: w.porcentajeDeducible, onChange: (K) => Pt(w.clave, { porcentajeDeducible: Number(K.target.value) }) }) }),
            /* @__PURE__ */ a.jsx("td", { className: "num", children: ue ? /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
              /* @__PURE__ */ a.jsx("strong", { children: M(ue.cuota) }),
              ue.autoliquidada && /* @__PURE__ */ a.jsx("div", { className: "dx-sub", children: "autoliquidada" }),
              ue.cuotaRecargo !== 0 && /* @__PURE__ */ a.jsxs("div", { className: "dx-sub", children: [
                "+ recargo ",
                M(ue.cuotaRecargo)
              ] })
            ] }) : "—" }),
            /* @__PURE__ */ a.jsx("td", { className: "right", children: /* @__PURE__ */ a.jsx("button", { className: "dx-icono", title: "Quitar", onClick: () => me((K) => K.length > 1 ? K.filter((_t) => _t.clave !== w.clave) : [Aa()]), children: "✕" }) })
          ] }, w.clave);
        }) })
      ] }),
      /* @__PURE__ */ a.jsx("datalist", { id: "dx-cuentas-gasto", children: y.map((w) => /* @__PURE__ */ a.jsx("option", { value: w.codigo, children: w.nombre }, w.codigo)) }),
      /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", style: { marginTop: 8 }, onClick: () => me((w) => {
        var B;
        return [...w, Aa(((B = w[w.length - 1]) == null ? void 0 : B.codigoIva) ?? "")];
      }), children: "+ Añadir línea" }),
      /* @__PURE__ */ a.jsx("span", { className: "muted dx-ayuda", children: "Una línea por cada base e impuesto de la factura. En inversión del sujeto pasivo e intracomunitarias el IVA lo autoliquidas tú (no lo cobra el proveedor)." })
    ] }),
    /* @__PURE__ */ a.jsxs("div", { className: "dx-pie", children: [
      /* @__PURE__ */ a.jsxs("div", { className: "panel", style: { margin: 0 }, children: [
        /* @__PURE__ */ a.jsxs("div", { className: "panel-head", children: [
          /* @__PURE__ */ a.jsx("h2", { children: "Vencimientos" }),
          /* @__PURE__ */ a.jsxs("div", { className: "dx-acciones", children: [
            [2, 3, 4].map((w) => /* @__PURE__ */ a.jsxs("button", { className: "btn small secondary", disabled: !U, onClick: () => qn(w), children: [
              w,
              " plazos"
            ] }, w)),
            ge && /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", onClick: () => te(null), children: "Según forma de pago" })
          ] })
        ] }),
        ge ? /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          ge.map((w, B) => /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", style: { marginBottom: 6 }, children: [
            /* @__PURE__ */ a.jsx("input", { type: "date", value: w.fecha, onChange: (fe) => te(ge.map((ue, K) => K === B ? { ...ue, fecha: fe.target.value } : ue)) }),
            /* @__PURE__ */ a.jsx("input", { className: "num", type: "number", step: "0.01", value: w.importe, onChange: (fe) => te(ge.map((ue, K) => K === B ? { ...ue, importe: Number(fe.target.value) } : ue)) })
          ] }, B)),
          U && bn !== U.total && /* @__PURE__ */ a.jsxs("p", { className: "dx-rojo", style: { margin: 0 }, children: [
            "Los plazos suman ",
            M(bn),
            "; la factura, ",
            M(U.total),
            "."
          ] })
        ] }) : /* @__PURE__ */ a.jsx("p", { className: "muted", style: { margin: 0 }, children: ((U == null ? void 0 : U.vencimientos) ?? []).map((w) => `${Ve(w.fecha)}: ${M(w.importe)}`).join(" · ") || "—" }),
        Yt && /* @__PURE__ */ a.jsx("p", { className: "dx-rojo", style: { marginBottom: 0 }, children: Yt })
      ] }),
      /* @__PURE__ */ a.jsxs("div", { className: "panel dx-totales", children: [
        ((U == null ? void 0 : U.desglose) ?? []).map((w, B) => {
          var fe;
          return /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
            /* @__PURE__ */ a.jsxs("span", { className: "muted", children: [
              ((fe = Wr(w.codigoIva)) == null ? void 0 : fe.nombre) ?? w.codigoIva,
              " ",
              w.autoliquidada ? `(${ye(w.porcentajeIva)} %, autoliquidado)` : "",
              " · base ",
              ye(w.base)
            ] }),
            /* @__PURE__ */ a.jsx("span", { children: M(w.cuota) })
          ] }, B);
        }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Base imponible" }),
          /* @__PURE__ */ a.jsx("span", { children: M(U == null ? void 0 : U.baseImponible) })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Impuestos" }),
          /* @__PURE__ */ a.jsx("span", { children: M(U == null ? void 0 : U.cuotaIva) })
        ] }),
        !!(U != null && U.recargoTotal) && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Recargo de equivalencia" }),
          /* @__PURE__ */ a.jsx("span", { children: M(U.recargoTotal) })
        ] }),
        !!(U != null && U.retencionIrpf) && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Retención IRPF" }),
          /* @__PURE__ */ a.jsxs("span", { children: [
            "−",
            M(U.retencionIrpf)
          ] })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ a.jsx("span", { children: ((U == null ? void 0 : U.total) ?? 0) < 0 ? "A favor (abono del proveedor)" : "Total a pagar" }),
          /* @__PURE__ */ a.jsx("span", { children: M(U == null ? void 0 : U.total) })
        ] }),
        U && (U.desglose ?? []).some((w) => w.cuotaDeducible !== w.cuota) && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "IVA deducible (antes de prorrata)" }),
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: M((U.desglose ?? []).reduce((w, B) => w + B.cuotaDeducible, 0)) })
        ] })
      ] })
    ] })
  ] });
}
function Xp(e) {
  const { api: t, anfitrion: n, navegar: r } = vt(), [l, i] = j.useState(null), [o, s] = j.useState(null), [u, d] = j.useState(""), [y, c] = j.useState(!1), m = () => {
    t.get(`/gastos/${e.id}`).then(i).catch((N) => d(N.message)), t.get(`/gastos/${e.id}/saldo`).then(s).catch(() => s(null));
  };
  if (j.useEffect(m, [e.id]), u) return /* @__PURE__ */ a.jsx("div", { className: "panel", children: /* @__PURE__ */ a.jsx("p", { className: "dx-rojo", children: u }) });
  if (!l) return /* @__PURE__ */ a.jsx("div", { className: "muted", children: "Cargando…" });
  const v = l.estado === "Registrado", g = !o || o.liquidado === 0;
  return /* @__PURE__ */ a.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsxs("div", { className: "panel-head", children: [
        /* @__PURE__ */ a.jsxs("h2", { style: { display: "flex", alignItems: "center", gap: 10 }, children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "gasto", pantalla: "lista" }), children: "←" }),
          "Factura ",
          /* @__PURE__ */ a.jsx("span", { className: "mono", children: l.numeroFactura ?? "(sin número)" }),
          " ",
          /* @__PURE__ */ a.jsx("span", { className: Po(l.estado === "Anulado" ? "Anulada" : "Emitida"), children: l.estado }),
          l.esRectificativa && /* @__PURE__ */ a.jsxs("span", { className: "pill", children: [
            "Rectifica ",
            l.numeroRectificado
          ] })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-acciones", children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "gasto", pantalla: "editor", semilla: { ...l, numeroFactura: null } }), children: "Duplicar" }),
          v && g && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "gasto", pantalla: "editor", id: l.id, semilla: l }), children: "Corregir" }),
          v && o && o.pendiente > 0 && n.irA && /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: () => n.irA("pagos"), children: "Pagar" }),
          v && !l.esRectificativa && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => {
            var N;
            return r({ tipo: "gasto", pantalla: "editor", semilla: {
              ...l,
              numeroFactura: null,
              esRectificativa: !0,
              rectificaGastoId: l.id,
              numeroRectificado: l.numeroFactura ?? l.concepto,
              fechaRectificada: l.fechaFactura ?? l.fecha,
              motivoRectificacion: "",
              vencimientos: null,
              lineas: (N = l.lineas) == null ? void 0 : N.map((F) => ({ ...F, base: -F.base }))
            } });
          }, children: "Rectificativa / abono" }),
          v && g && /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", onClick: () => c(!0), children: "Anular" })
        ] })
      ] }),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ a.jsxs("div", { className: "dx-dato", children: [
          /* @__PURE__ */ a.jsx("small", { children: "Proveedor" }),
          /* @__PURE__ */ a.jsx("div", { children: /* @__PURE__ */ a.jsx("strong", { children: l.proveedorTexto }) })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-dato", children: [
          /* @__PURE__ */ a.jsx("small", { children: "Fecha de factura" }),
          /* @__PURE__ */ a.jsx("div", { children: Ve(l.fechaFactura ?? l.fecha) })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-dato", children: [
          /* @__PURE__ */ a.jsx("small", { children: "Registro" }),
          /* @__PURE__ */ a.jsx("div", { children: Ve(l.fecha) })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-dato", children: [
          /* @__PURE__ */ a.jsx("small", { children: "Pago" }),
          /* @__PURE__ */ a.jsx("div", { children: o ? o.pendiente <= 0 ? /* @__PURE__ */ a.jsx("span", { className: "pill ok", children: "Pagada" }) : /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            "Pendiente ",
            /* @__PURE__ */ a.jsx("strong", { children: M(o.pendiente) })
          ] }) : "—" })
        ] })
      ] }),
      /* @__PURE__ */ a.jsx("p", { className: "muted", style: { marginBottom: 0 }, children: l.concepto })
    ] }),
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsxs("table", { children: [
        /* @__PURE__ */ a.jsx("thead", { children: /* @__PURE__ */ a.jsxs("tr", { children: [
          /* @__PURE__ */ a.jsx("th", { children: "Descripción" }),
          /* @__PURE__ */ a.jsx("th", { children: "Cuenta" }),
          /* @__PURE__ */ a.jsx("th", { className: "num", children: "Base" }),
          /* @__PURE__ */ a.jsx("th", { children: "Impuesto" }),
          /* @__PURE__ */ a.jsx("th", { className: "num", children: "Cuota" }),
          /* @__PURE__ */ a.jsx("th", { className: "num", children: "Deducible" })
        ] }) }),
        /* @__PURE__ */ a.jsx("tbody", { children: (l.lineas ?? []).map((N, F) => /* @__PURE__ */ a.jsxs("tr", { children: [
          /* @__PURE__ */ a.jsx("td", { children: N.descripcion ?? "" }),
          /* @__PURE__ */ a.jsx("td", { className: "mono muted", children: N.cuentaGasto ?? "regla" }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: M(N.base) }),
          /* @__PURE__ */ a.jsxs("td", { children: [
            N.codigoIva,
            " · ",
            ye(N.porcentajeIva),
            " %",
            N.autoliquidada ? " · autoliquidada" : "",
            N.cuotaRecargo ? ` · recargo ${M(N.cuotaRecargo)}` : ""
          ] }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: M(N.cuota) }),
          /* @__PURE__ */ a.jsxs("td", { className: "num muted", children: [
            N.porcentajeDeducible !== 100 ? `${ye(N.porcentajeDeducible)} % · ` : "",
            M(N.cuotaDeducible)
          ] })
        ] }, F)) })
      ] }),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-totales-vista", children: [
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Base imponible" }),
          /* @__PURE__ */ a.jsx("span", { children: M(l.baseImponible) })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Impuestos" }),
          /* @__PURE__ */ a.jsx("span", { children: M(l.cuotaIva) })
        ] }),
        !!l.recargoTotal && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Recargo de equivalencia" }),
          /* @__PURE__ */ a.jsx("span", { children: M(l.recargoTotal) })
        ] }),
        !!l.retencionIrpf && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsxs("span", { className: "muted", children: [
            "Retención IRPF (",
            ye(l.porcentajeIrpf),
            " %)"
          ] }),
          /* @__PURE__ */ a.jsxs("span", { children: [
            "−",
            M(l.retencionIrpf)
          ] })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ a.jsx("span", { children: l.total < 0 ? "A favor (abono del proveedor)" : "Total a pagar" }),
          /* @__PURE__ */ a.jsx("span", { children: M(l.total) })
        ] })
      ] }),
      /* @__PURE__ */ a.jsxs("p", { className: "muted", style: { fontSize: 12.5 }, children: [
        "Vencimientos: ",
        (l.vencimientos ?? []).map((N) => `${Ve(N.fecha)} ${M(N.importe)}`).join(" · ")
      ] })
    ] }),
    y && /* @__PURE__ */ a.jsx(
      dn,
      {
        titulo: "Anular la factura",
        alCerrar: () => c(!1),
        acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => c(!1), children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: async () => {
            try {
              await t.post(`/gastos/${l.id}/anular`), n.aviso("Factura anulada.", "ok"), c(!1), m();
            } catch (N) {
              n.aviso(N.message, "err");
            }
          }, children: "Anular" })
        ] }),
        children: /* @__PURE__ */ a.jsx("p", { style: { margin: 0 }, children: "Sale de los libros de IVA y de las declaraciones, y se contabiliza su contraasiento. Si solo hay que cambiar algo, mejor «Corregir»." })
      }
    )
  ] });
}
function Zp(e) {
  const [t, n] = j.useState(e.inicial), r = j.useRef(0), [l, i] = j.useState(0), o = j.useMemo(() => Tp(e.anfitrion), [e.anfitrion]), s = (c) => {
    n(c), i(++r.current), window.scrollTo({ top: 0 });
  }, u = { api: o, anfitrion: e.anfitrion, ruta: t, navegar: s }, d = `${t.tipo}-${t.pantalla}-${t.id ?? ""}-${l}`;
  let y;
  if (t.pantalla === "lista") y = /* @__PURE__ */ a.jsx(Ap, { tipo: t.tipo });
  else if (t.pantalla === "vista" && t.id)
    y = t.tipo === "factura" ? /* @__PURE__ */ a.jsx(Qp, { id: t.id }) : t.tipo === "presupuesto" ? /* @__PURE__ */ a.jsx(Gp, { id: t.id }) : t.tipo === "pedido" ? /* @__PURE__ */ a.jsx(Kp, { id: t.id }) : t.tipo === "gasto" ? /* @__PURE__ */ a.jsx(Xp, { id: t.id }) : /* @__PURE__ */ a.jsx(qp, { id: t.id });
  else if (t.tipo === "gasto")
    y = /* @__PURE__ */ a.jsx(
      Yp,
      {
        id: t.id,
        semilla: t.semilla,
        alGuardar: (c) => s({ tipo: "gasto", pantalla: "vista", id: c }),
        alCancelar: () => s(t.id ? { tipo: "gasto", pantalla: "vista", id: t.id } : { tipo: "gasto", pantalla: "lista" })
      }
    );
  else if (t.tipo === "compra")
    y = /* @__PURE__ */ a.jsx(
      Wp,
      {
        id: t.id,
        semilla: t.semilla,
        alGuardar: (c) => s({ tipo: "compra", pantalla: "vista", id: c }),
        alCancelar: () => s(t.id ? { tipo: "compra", pantalla: "vista", id: t.id } : { tipo: "compra", pantalla: "lista" })
      }
    );
  else {
    const c = t.tipo;
    y = /* @__PURE__ */ a.jsx(
      Hp,
      {
        tipo: c,
        id: t.id,
        semilla: t.semilla,
        alGuardar: (m) => s({ tipo: c, pantalla: "vista", id: m }),
        alCancelar: () => s(t.id ? { tipo: c, pantalla: "vista", id: t.id } : { tipo: c, pantalla: "lista" })
      }
    );
  }
  return /* @__PURE__ */ a.jsx(ed.Provider, { value: u, children: /* @__PURE__ */ a.jsx("div", { className: "dx-raiz", children: y }, d) });
}
const Jp = `
.dx-raiz { display:flex; flex-direction:column; gap:16px; }
.dx-editor { display:flex; flex-direction:column; gap:16px; }
.dx-editor .panel { margin:0; }
.dx-cabecera { display:grid; grid-template-columns: minmax(0,2fr) minmax(0,1fr); gap:18px; align-items:start; }
.dx-cab-campos label, .dx-dialogo label { display:block; margin:10px 0 5px; font-size:12.5px; color:var(--muted); font-weight:500; }
.dx-fila { display:grid; grid-template-columns: repeat(auto-fit, minmax(150px,1fr)); gap:10px; }
.dx-check { display:flex !important; align-items:center; gap:8px; margin-top:12px !important; color:var(--ink) !important; cursor:pointer; }
.dx-check input { width:auto; }
.dx-ficha { background:var(--bg,#f4f6f9); border:1px solid var(--line); border-radius:12px; padding:12px 14px; font-size:13px; line-height:1.55; min-height:80px; }
.dx-aviso { margin-top:8px; color:#b45309; font-weight:600; }
.dx-rojo { color:#dc2626; }
.dx-buscador { position:relative; }
.dx-lista { position:absolute; z-index:40; left:0; right:0; top:calc(100% + 2px); min-width:420px; max-height:320px; overflow:auto; background:var(--surface,#fff); border:1px solid var(--line); border-radius:10px; box-shadow:0 12px 30px rgba(15,23,42,.14); }
.dx-opcion { display:flex; justify-content:space-between; gap:10px; padding:8px 11px; cursor:pointer; font-size:13px; border-bottom:1px solid var(--line); }
.dx-opcion:last-child { border-bottom:none; }
.dx-opcion.activa { background:var(--accent-soft); }
.dx-rejilla table { width:100%; border-collapse:collapse; }
.dx-rejilla th { font-size:11.5px; text-transform:uppercase; letter-spacing:.03em; color:var(--muted); padding:6px 6px; text-align:left; }
.dx-rejilla td { padding:4px 4px; vertical-align:top; border-top:1px solid var(--line); }
.dx-rejilla input, .dx-rejilla select { padding:7px 8px; font-size:13px; border-radius:8px; margin:0; width:100%; }
.dx-rejilla input.num { text-align:right; }
.dx-rejilla input.dx-auto::placeholder { color:#0e7490; opacity:.75; }
.dx-rejilla .num { text-align:right; }
.dx-par { background:rgba(148,163,184,.05); }
.dx-sub { font-size:11.5px; color:var(--muted); margin-top:2px; }
.dx-icono { border:1px solid var(--line); background:var(--surface,#fff); border-radius:8px; width:28px; height:28px; cursor:pointer; margin-left:3px; color:var(--muted); font-weight:700; }
.dx-icono.activo, .dx-icono:hover { color:var(--accent); border-color:var(--accent); }
.dx-fila-conc td { background:var(--accent-soft); }
.dx-resumen-conc { display:block; border:none; background:none; padding:2px 2px 0; font-size:11.5px; color:#0e7490; cursor:pointer; text-align:left; }
.dx-conceptos { display:flex; flex-wrap:wrap; gap:6px; align-items:center; font-size:12.5px; padding:4px 2px; }
.dx-chip { display:inline-flex; align-items:center; gap:3px; background:var(--surface,#fff); border:1px solid var(--line); border-radius:9px; padding:2px 4px; }
.dx-chip.auto { border-style:dashed; }
.dx-chip.coste { border-color:#f59e0b; }
.dx-chip select, .dx-chip input { padding:3px 5px !important; font-size:12px !important; border:none !important; background:transparent; width:auto !important; margin:0 !important; }
.dx-chip input { width:72px !important; }
.dx-chip button { border:none; background:none; cursor:pointer; color:var(--muted); }
.dx-enlace { border:none; background:none; color:var(--accent); cursor:pointer; font-weight:600; padding:0 4px; font-size:12.5px; }
.dx-ayuda { margin-left:12px; font-size:11.5px; }
.dx-pie { display:grid; grid-template-columns: 1fr minmax(320px, 380px); gap:16px; align-items:start; }
.dx-totales { margin:0; }
.dx-tot { display:flex; justify-content:space-between; gap:12px; padding:4px 0; font-size:13.5px; }
.dx-grande { font-size:17px; font-weight:750; border-top:1px solid var(--line); margin-top:4px; padding-top:8px; }
.dx-totales-vista { max-width:360px; margin:14px 0 0 auto; }
.dx-datos { display:grid; grid-template-columns:repeat(auto-fit,minmax(170px,1fr)); gap:14px; }
.dx-dato small { display:block; font-size:11.5px; text-transform:uppercase; letter-spacing:.04em; color:var(--muted); margin-bottom:3px; }
.dx-acciones { display:flex; gap:6px; flex-wrap:wrap; justify-content:flex-end; }
.dx-aplicados { font-size:11.5px; color:var(--muted); margin-top:2px; }
.dx-filtros { display:grid; grid-template-columns: 2fr 1fr 1fr 1fr; gap:8px; margin-bottom:12px; }
.dx-lista-docs tbody tr { cursor:pointer; }
.dx-lista-docs tbody tr:hover, .dx-lista-docs tbody tr:focus { background:var(--accent-soft); outline:none; }
.dx-paginas { display:flex; gap:10px; align-items:center; justify-content:flex-end; margin-top:10px; }
.dx-fondo { position:fixed; inset:0; background:rgba(15,23,42,.45); display:grid; place-items:center; z-index:1000; padding:16px; }
.dx-dialogo { background:var(--surface,#fff); border-radius:16px; width:100%; box-shadow:0 20px 60px rgba(0,0,0,.25); max-height:90vh; display:flex; flex-direction:column; }
.dx-dialogo-cab { display:flex; justify-content:space-between; align-items:center; padding:14px 18px; border-bottom:1px solid var(--line); }
.dx-dialogo-cuerpo { padding:14px 18px; overflow:auto; }
.dx-dialogo-pie { display:flex; justify-content:flex-end; gap:8px; padding:12px 18px; border-top:1px solid var(--line); }
@media (max-width: 900px) { .dx-cabecera, .dx-pie { grid-template-columns: 1fr; } .dx-filtros { grid-template-columns: 1fr 1fr; } .dx-rejilla { overflow-x:auto; } .dx-rejilla table { min-width: 860px; } }
`;
function em() {
  if (document.getElementById("dx-estilos")) return;
  const e = document.createElement("style");
  e.id = "dx-estilos", e.textContent = Jp, document.head.appendChild(e);
}
function tm(e, t, n) {
  em();
  const r = Jc(e);
  return r.render(/* @__PURE__ */ a.jsx(Zp, { anfitrion: t, inicial: n })), () => r.unmount();
}
export {
  tm as montar
};
