var vc = { exports: {} }, rl = {}, xc = { exports: {} }, K = {};
/**
 * @license React
 * react.production.min.js
 *
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */
var Xr = Symbol.for("react.element"), Hd = Symbol.for("react.portal"), Wd = Symbol.for("react.fragment"), Qd = Symbol.for("react.strict_mode"), Gd = Symbol.for("react.profiler"), Kd = Symbol.for("react.provider"), qd = Symbol.for("react.context"), Yd = Symbol.for("react.forward_ref"), Xd = Symbol.for("react.suspense"), Zd = Symbol.for("react.memo"), Jd = Symbol.for("react.lazy"), as = Symbol.iterator;
function ef(e) {
  return e === null || typeof e != "object" ? null : (e = as && e[as] || e["@@iterator"], typeof e == "function" ? e : null);
}
var gc = { isMounted: function() {
  return !1;
}, enqueueForceUpdate: function() {
}, enqueueReplaceState: function() {
}, enqueueSetState: function() {
} }, yc = Object.assign, jc = {};
function ir(e, t, n) {
  this.props = e, this.context = t, this.refs = jc, this.updater = n || gc;
}
ir.prototype.isReactComponent = {};
ir.prototype.setState = function(e, t) {
  if (typeof e != "object" && typeof e != "function" && e != null) throw Error("setState(...): takes an object of state variables to update or a function which returns an object of state variables.");
  this.updater.enqueueSetState(this, e, t, "setState");
};
ir.prototype.forceUpdate = function(e) {
  this.updater.enqueueForceUpdate(this, e, "forceUpdate");
};
function Nc() {
}
Nc.prototype = ir.prototype;
function qi(e, t, n) {
  this.props = e, this.context = t, this.refs = jc, this.updater = n || gc;
}
var Yi = qi.prototype = new Nc();
Yi.constructor = qi;
yc(Yi, ir.prototype);
Yi.isPureReactComponent = !0;
var ls = Array.isArray, Sc = Object.prototype.hasOwnProperty, Xi = { current: null }, wc = { key: !0, ref: !0, __self: !0, __source: !0 };
function Cc(e, t, n) {
  var r, a = {}, i = null, o = null;
  if (t != null) for (r in t.ref !== void 0 && (o = t.ref), t.key !== void 0 && (i = "" + t.key), t) Sc.call(t, r) && !wc.hasOwnProperty(r) && (a[r] = t[r]);
  var s = arguments.length - 2;
  if (s === 1) a.children = n;
  else if (1 < s) {
    for (var c = Array(s), f = 0; f < s; f++) c[f] = arguments[f + 2];
    a.children = c;
  }
  if (e && e.defaultProps) for (r in s = e.defaultProps, s) a[r] === void 0 && (a[r] = s[r]);
  return { $$typeof: Xr, type: e, key: i, ref: o, props: a, _owner: Xi.current };
}
function tf(e, t) {
  return { $$typeof: Xr, type: e.type, key: t, ref: e.ref, props: e.props, _owner: e._owner };
}
function Zi(e) {
  return typeof e == "object" && e !== null && e.$$typeof === Xr;
}
function nf(e) {
  var t = { "=": "=0", ":": "=2" };
  return "$" + e.replace(/[=:]/g, function(n) {
    return t[n];
  });
}
var is = /\/+/g;
function Il(e, t) {
  return typeof e == "object" && e !== null && e.key != null ? nf("" + e.key) : t.toString(36);
}
function Na(e, t, n, r, a) {
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
        case Xr:
        case Hd:
          o = !0;
      }
  }
  if (o) return o = e, a = a(o), e = r === "" ? "." + Il(o, 0) : r, ls(a) ? (n = "", e != null && (n = e.replace(is, "$&/") + "/"), Na(a, t, n, "", function(f) {
    return f;
  })) : a != null && (Zi(a) && (a = tf(a, n + (!a.key || o && o.key === a.key ? "" : ("" + a.key).replace(is, "$&/") + "/") + e)), t.push(a)), 1;
  if (o = 0, r = r === "" ? "." : r + ":", ls(e)) for (var s = 0; s < e.length; s++) {
    i = e[s];
    var c = r + Il(i, s);
    o += Na(i, t, n, c, a);
  }
  else if (c = ef(e), typeof c == "function") for (e = c.call(e), s = 0; !(i = e.next()).done; ) i = i.value, c = r + Il(i, s++), o += Na(i, t, n, c, a);
  else if (i === "object") throw t = String(e), Error("Objects are not valid as a React child (found: " + (t === "[object Object]" ? "object with keys {" + Object.keys(e).join(", ") + "}" : t) + "). If you meant to render a collection of children, use an array instead.");
  return o;
}
function aa(e, t, n) {
  if (e == null) return e;
  var r = [], a = 0;
  return Na(e, r, "", "", function(i) {
    return t.call(n, i, a++);
  }), r;
}
function rf(e) {
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
var Ge = { current: null }, Sa = { transition: null }, af = { ReactCurrentDispatcher: Ge, ReactCurrentBatchConfig: Sa, ReactCurrentOwner: Xi };
function kc() {
  throw Error("act(...) is not supported in production builds of React.");
}
K.Children = { map: aa, forEach: function(e, t, n) {
  aa(e, function() {
    t.apply(this, arguments);
  }, n);
}, count: function(e) {
  var t = 0;
  return aa(e, function() {
    t++;
  }), t;
}, toArray: function(e) {
  return aa(e, function(t) {
    return t;
  }) || [];
}, only: function(e) {
  if (!Zi(e)) throw Error("React.Children.only expected to receive a single React element child.");
  return e;
} };
K.Component = ir;
K.Fragment = Wd;
K.Profiler = Gd;
K.PureComponent = qi;
K.StrictMode = Qd;
K.Suspense = Xd;
K.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED = af;
K.act = kc;
K.cloneElement = function(e, t, n) {
  if (e == null) throw Error("React.cloneElement(...): The argument must be a React element, but you passed " + e + ".");
  var r = yc({}, e.props), a = e.key, i = e.ref, o = e._owner;
  if (t != null) {
    if (t.ref !== void 0 && (i = t.ref, o = Xi.current), t.key !== void 0 && (a = "" + t.key), e.type && e.type.defaultProps) var s = e.type.defaultProps;
    for (c in t) Sc.call(t, c) && !wc.hasOwnProperty(c) && (r[c] = t[c] === void 0 && s !== void 0 ? s[c] : t[c]);
  }
  var c = arguments.length - 2;
  if (c === 1) r.children = n;
  else if (1 < c) {
    s = Array(c);
    for (var f = 0; f < c; f++) s[f] = arguments[f + 2];
    r.children = s;
  }
  return { $$typeof: Xr, type: e.type, key: a, ref: i, props: r, _owner: o };
};
K.createContext = function(e) {
  return e = { $$typeof: qd, _currentValue: e, _currentValue2: e, _threadCount: 0, Provider: null, Consumer: null, _defaultValue: null, _globalName: null }, e.Provider = { $$typeof: Kd, _context: e }, e.Consumer = e;
};
K.createElement = Cc;
K.createFactory = function(e) {
  var t = Cc.bind(null, e);
  return t.type = e, t;
};
K.createRef = function() {
  return { current: null };
};
K.forwardRef = function(e) {
  return { $$typeof: Yd, render: e };
};
K.isValidElement = Zi;
K.lazy = function(e) {
  return { $$typeof: Jd, _payload: { _status: -1, _result: e }, _init: rf };
};
K.memo = function(e, t) {
  return { $$typeof: Zd, type: e, compare: t === void 0 ? null : t };
};
K.startTransition = function(e) {
  var t = Sa.transition;
  Sa.transition = {};
  try {
    e();
  } finally {
    Sa.transition = t;
  }
};
K.unstable_act = kc;
K.useCallback = function(e, t) {
  return Ge.current.useCallback(e, t);
};
K.useContext = function(e) {
  return Ge.current.useContext(e);
};
K.useDebugValue = function() {
};
K.useDeferredValue = function(e) {
  return Ge.current.useDeferredValue(e);
};
K.useEffect = function(e, t) {
  return Ge.current.useEffect(e, t);
};
K.useId = function() {
  return Ge.current.useId();
};
K.useImperativeHandle = function(e, t, n) {
  return Ge.current.useImperativeHandle(e, t, n);
};
K.useInsertionEffect = function(e, t) {
  return Ge.current.useInsertionEffect(e, t);
};
K.useLayoutEffect = function(e, t) {
  return Ge.current.useLayoutEffect(e, t);
};
K.useMemo = function(e, t) {
  return Ge.current.useMemo(e, t);
};
K.useReducer = function(e, t, n) {
  return Ge.current.useReducer(e, t, n);
};
K.useRef = function(e) {
  return Ge.current.useRef(e);
};
K.useState = function(e) {
  return Ge.current.useState(e);
};
K.useSyncExternalStore = function(e, t, n) {
  return Ge.current.useSyncExternalStore(e, t, n);
};
K.useTransition = function() {
  return Ge.current.useTransition();
};
K.version = "18.3.1";
xc.exports = K;
var x = xc.exports;
/**
 * @license React
 * react-jsx-runtime.production.min.js
 *
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */
var lf = x, of = Symbol.for("react.element"), sf = Symbol.for("react.fragment"), cf = Object.prototype.hasOwnProperty, uf = lf.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED.ReactCurrentOwner, df = { key: !0, ref: !0, __self: !0, __source: !0 };
function Ec(e, t, n) {
  var r, a = {}, i = null, o = null;
  n !== void 0 && (i = "" + n), t.key !== void 0 && (i = "" + t.key), t.ref !== void 0 && (o = t.ref);
  for (r in t) cf.call(t, r) && !df.hasOwnProperty(r) && (a[r] = t[r]);
  if (e && e.defaultProps) for (r in t = e.defaultProps, t) a[r] === void 0 && (a[r] = t[r]);
  return { $$typeof: of, type: e, key: i, ref: o, props: a, _owner: uf.current };
}
rl.Fragment = sf;
rl.jsx = Ec;
rl.jsxs = Ec;
vc.exports = rl;
var l = vc.exports, Ic = { exports: {} }, it = {}, Pc = { exports: {} }, Fc = {};
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
  function t(I, g) {
    var L = I.length;
    I.push(g);
    e: for (; 0 < L; ) {
      var V = L - 1 >>> 1, B = I[V];
      if (0 < a(B, g)) I[V] = g, I[L] = B, L = V;
      else break e;
    }
  }
  function n(I) {
    return I.length === 0 ? null : I[0];
  }
  function r(I) {
    if (I.length === 0) return null;
    var g = I[0], L = I.pop();
    if (L !== g) {
      I[0] = L;
      e: for (var V = 0, B = I.length, Y = B >>> 1; V < Y; ) {
        var je = 2 * (V + 1) - 1, ve = I[je], ce = je + 1, Ne = I[ce];
        if (0 > a(ve, L)) ce < B && 0 > a(Ne, ve) ? (I[V] = Ne, I[ce] = L, V = ce) : (I[V] = ve, I[je] = L, V = je);
        else if (ce < B && 0 > a(Ne, L)) I[V] = Ne, I[ce] = L, V = ce;
        else break e;
      }
    }
    return g;
  }
  function a(I, g) {
    var L = I.sortIndex - g.sortIndex;
    return L !== 0 ? L : I.id - g.id;
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
  var c = [], f = [], j = 1, d = null, m = 3, h = !1, N = !1, w = !1, M = typeof setTimeout == "function" ? setTimeout : null, p = typeof clearTimeout == "function" ? clearTimeout : null, u = typeof setImmediate < "u" ? setImmediate : null;
  typeof navigator < "u" && navigator.scheduling !== void 0 && navigator.scheduling.isInputPending !== void 0 && navigator.scheduling.isInputPending.bind(navigator.scheduling);
  function v(I) {
    for (var g = n(f); g !== null; ) {
      if (g.callback === null) r(f);
      else if (g.startTime <= I) r(f), g.sortIndex = g.expirationTime, t(c, g);
      else break;
      g = n(f);
    }
  }
  function E(I) {
    if (w = !1, v(I), !N) if (n(c) !== null) N = !0, Ee(T);
    else {
      var g = n(f);
      g !== null && Re(E, g.startTime - I);
    }
  }
  function T(I, g) {
    N = !1, w && (w = !1, p(C), C = -1), h = !0;
    var L = m;
    try {
      for (v(g), d = n(c); d !== null && (!(d.expirationTime > g) || I && !P()); ) {
        var V = d.callback;
        if (typeof V == "function") {
          d.callback = null, m = d.priorityLevel;
          var B = V(d.expirationTime <= g);
          g = e.unstable_now(), typeof B == "function" ? d.callback = B : d === n(c) && r(c), v(g);
        } else r(c);
        d = n(c);
      }
      if (d !== null) var Y = !0;
      else {
        var je = n(f);
        je !== null && Re(E, je.startTime - g), Y = !1;
      }
      return Y;
    } finally {
      d = null, m = L, h = !1;
    }
  }
  var R = !1, D = null, C = -1, $ = 5, b = -1;
  function P() {
    return !(e.unstable_now() - b < $);
  }
  function W() {
    if (D !== null) {
      var I = e.unstable_now();
      b = I;
      var g = !0;
      try {
        g = D(!0, I);
      } finally {
        g ? se() : (R = !1, D = null);
      }
    } else R = !1;
  }
  var se;
  if (typeof u == "function") se = function() {
    u(W);
  };
  else if (typeof MessageChannel < "u") {
    var Ue = new MessageChannel(), Ve = Ue.port2;
    Ue.port1.onmessage = W, se = function() {
      Ve.postMessage(null);
    };
  } else se = function() {
    M(W, 0);
  };
  function Ee(I) {
    D = I, R || (R = !0, se());
  }
  function Re(I, g) {
    C = M(function() {
      I(e.unstable_now());
    }, g);
  }
  e.unstable_IdlePriority = 5, e.unstable_ImmediatePriority = 1, e.unstable_LowPriority = 4, e.unstable_NormalPriority = 3, e.unstable_Profiling = null, e.unstable_UserBlockingPriority = 2, e.unstable_cancelCallback = function(I) {
    I.callback = null;
  }, e.unstable_continueExecution = function() {
    N || h || (N = !0, Ee(T));
  }, e.unstable_forceFrameRate = function(I) {
    0 > I || 125 < I ? console.error("forceFrameRate takes a positive int between 0 and 125, forcing frame rates higher than 125 fps is not supported") : $ = 0 < I ? Math.floor(1e3 / I) : 5;
  }, e.unstable_getCurrentPriorityLevel = function() {
    return m;
  }, e.unstable_getFirstCallbackNode = function() {
    return n(c);
  }, e.unstable_next = function(I) {
    switch (m) {
      case 1:
      case 2:
      case 3:
        var g = 3;
        break;
      default:
        g = m;
    }
    var L = m;
    m = g;
    try {
      return I();
    } finally {
      m = L;
    }
  }, e.unstable_pauseExecution = function() {
  }, e.unstable_requestPaint = function() {
  }, e.unstable_runWithPriority = function(I, g) {
    switch (I) {
      case 1:
      case 2:
      case 3:
      case 4:
      case 5:
        break;
      default:
        I = 3;
    }
    var L = m;
    m = I;
    try {
      return g();
    } finally {
      m = L;
    }
  }, e.unstable_scheduleCallback = function(I, g, L) {
    var V = e.unstable_now();
    switch (typeof L == "object" && L !== null ? (L = L.delay, L = typeof L == "number" && 0 < L ? V + L : V) : L = V, I) {
      case 1:
        var B = -1;
        break;
      case 2:
        B = 250;
        break;
      case 5:
        B = 1073741823;
        break;
      case 4:
        B = 1e4;
        break;
      default:
        B = 5e3;
    }
    return B = L + B, I = { id: j++, callback: g, priorityLevel: I, startTime: L, expirationTime: B, sortIndex: -1 }, L > V ? (I.sortIndex = L, t(f, I), n(c) === null && I === n(f) && (w ? (p(C), C = -1) : w = !0, Re(E, L - V))) : (I.sortIndex = B, t(c, I), N || h || (N = !0, Ee(T))), I;
  }, e.unstable_shouldYield = P, e.unstable_wrapCallback = function(I) {
    var g = m;
    return function() {
      var L = m;
      m = g;
      try {
        return I.apply(this, arguments);
      } finally {
        m = L;
      }
    };
  };
})(Fc);
Pc.exports = Fc;
var ff = Pc.exports;
/**
 * @license React
 * react-dom.production.min.js
 *
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */
var pf = x, lt = ff;
function F(e) {
  for (var t = "https://reactjs.org/docs/error-decoder.html?invariant=" + e, n = 1; n < arguments.length; n++) t += "&args[]=" + encodeURIComponent(arguments[n]);
  return "Minified React error #" + e + "; visit " + t + " for the full message or use the non-minified dev environment for full errors and additional helpful warnings.";
}
var Rc = /* @__PURE__ */ new Set(), Dr = {};
function Rn(e, t) {
  Jn(e, t), Jn(e + "Capture", t);
}
function Jn(e, t) {
  for (Dr[e] = t, e = 0; e < t.length; e++) Rc.add(t[e]);
}
var Vt = !(typeof window > "u" || typeof window.document > "u" || typeof window.document.createElement > "u"), ni = Object.prototype.hasOwnProperty, mf = /^[:A-Z_a-z\u00C0-\u00D6\u00D8-\u00F6\u00F8-\u02FF\u0370-\u037D\u037F-\u1FFF\u200C-\u200D\u2070-\u218F\u2C00-\u2FEF\u3001-\uD7FF\uF900-\uFDCF\uFDF0-\uFFFD][:A-Z_a-z\u00C0-\u00D6\u00D8-\u00F6\u00F8-\u02FF\u0370-\u037D\u037F-\u1FFF\u200C-\u200D\u2070-\u218F\u2C00-\u2FEF\u3001-\uD7FF\uF900-\uFDCF\uFDF0-\uFFFD\-.0-9\u00B7\u0300-\u036F\u203F-\u2040]*$/, os = {}, ss = {};
function hf(e) {
  return ni.call(ss, e) ? !0 : ni.call(os, e) ? !1 : mf.test(e) ? ss[e] = !0 : (os[e] = !0, !1);
}
function vf(e, t, n, r) {
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
function xf(e, t, n, r) {
  if (t === null || typeof t > "u" || vf(e, t, n, r)) return !0;
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
function Ke(e, t, n, r, a, i, o) {
  this.acceptsBooleans = t === 2 || t === 3 || t === 4, this.attributeName = r, this.attributeNamespace = a, this.mustUseProperty = n, this.propertyName = e, this.type = t, this.sanitizeURL = i, this.removeEmptyString = o;
}
var De = {};
"children dangerouslySetInnerHTML defaultValue defaultChecked innerHTML suppressContentEditableWarning suppressHydrationWarning style".split(" ").forEach(function(e) {
  De[e] = new Ke(e, 0, !1, e, null, !1, !1);
});
[["acceptCharset", "accept-charset"], ["className", "class"], ["htmlFor", "for"], ["httpEquiv", "http-equiv"]].forEach(function(e) {
  var t = e[0];
  De[t] = new Ke(t, 1, !1, e[1], null, !1, !1);
});
["contentEditable", "draggable", "spellCheck", "value"].forEach(function(e) {
  De[e] = new Ke(e, 2, !1, e.toLowerCase(), null, !1, !1);
});
["autoReverse", "externalResourcesRequired", "focusable", "preserveAlpha"].forEach(function(e) {
  De[e] = new Ke(e, 2, !1, e, null, !1, !1);
});
"allowFullScreen async autoFocus autoPlay controls default defer disabled disablePictureInPicture disableRemotePlayback formNoValidate hidden loop noModule noValidate open playsInline readOnly required reversed scoped seamless itemScope".split(" ").forEach(function(e) {
  De[e] = new Ke(e, 3, !1, e.toLowerCase(), null, !1, !1);
});
["checked", "multiple", "muted", "selected"].forEach(function(e) {
  De[e] = new Ke(e, 3, !0, e, null, !1, !1);
});
["capture", "download"].forEach(function(e) {
  De[e] = new Ke(e, 4, !1, e, null, !1, !1);
});
["cols", "rows", "size", "span"].forEach(function(e) {
  De[e] = new Ke(e, 6, !1, e, null, !1, !1);
});
["rowSpan", "start"].forEach(function(e) {
  De[e] = new Ke(e, 5, !1, e.toLowerCase(), null, !1, !1);
});
var Ji = /[\-:]([a-z])/g;
function eo(e) {
  return e[1].toUpperCase();
}
"accent-height alignment-baseline arabic-form baseline-shift cap-height clip-path clip-rule color-interpolation color-interpolation-filters color-profile color-rendering dominant-baseline enable-background fill-opacity fill-rule flood-color flood-opacity font-family font-size font-size-adjust font-stretch font-style font-variant font-weight glyph-name glyph-orientation-horizontal glyph-orientation-vertical horiz-adv-x horiz-origin-x image-rendering letter-spacing lighting-color marker-end marker-mid marker-start overline-position overline-thickness paint-order panose-1 pointer-events rendering-intent shape-rendering stop-color stop-opacity strikethrough-position strikethrough-thickness stroke-dasharray stroke-dashoffset stroke-linecap stroke-linejoin stroke-miterlimit stroke-opacity stroke-width text-anchor text-decoration text-rendering underline-position underline-thickness unicode-bidi unicode-range units-per-em v-alphabetic v-hanging v-ideographic v-mathematical vector-effect vert-adv-y vert-origin-x vert-origin-y word-spacing writing-mode xmlns:xlink x-height".split(" ").forEach(function(e) {
  var t = e.replace(
    Ji,
    eo
  );
  De[t] = new Ke(t, 1, !1, e, null, !1, !1);
});
"xlink:actuate xlink:arcrole xlink:role xlink:show xlink:title xlink:type".split(" ").forEach(function(e) {
  var t = e.replace(Ji, eo);
  De[t] = new Ke(t, 1, !1, e, "http://www.w3.org/1999/xlink", !1, !1);
});
["xml:base", "xml:lang", "xml:space"].forEach(function(e) {
  var t = e.replace(Ji, eo);
  De[t] = new Ke(t, 1, !1, e, "http://www.w3.org/XML/1998/namespace", !1, !1);
});
["tabIndex", "crossOrigin"].forEach(function(e) {
  De[e] = new Ke(e, 1, !1, e.toLowerCase(), null, !1, !1);
});
De.xlinkHref = new Ke("xlinkHref", 1, !1, "xlink:href", "http://www.w3.org/1999/xlink", !0, !1);
["src", "href", "action", "formAction"].forEach(function(e) {
  De[e] = new Ke(e, 1, !1, e.toLowerCase(), null, !0, !0);
});
function to(e, t, n, r) {
  var a = De.hasOwnProperty(t) ? De[t] : null;
  (a !== null ? a.type !== 0 : r || !(2 < t.length) || t[0] !== "o" && t[0] !== "O" || t[1] !== "n" && t[1] !== "N") && (xf(t, n, a, r) && (n = null), r || a === null ? hf(t) && (n === null ? e.removeAttribute(t) : e.setAttribute(t, "" + n)) : a.mustUseProperty ? e[a.propertyName] = n === null ? a.type === 3 ? !1 : "" : n : (t = a.attributeName, r = a.attributeNamespace, n === null ? e.removeAttribute(t) : (a = a.type, n = a === 3 || a === 4 && n === !0 ? "" : "" + n, r ? e.setAttributeNS(r, t, n) : e.setAttribute(t, n))));
}
var Qt = pf.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED, la = Symbol.for("react.element"), zn = Symbol.for("react.portal"), Ln = Symbol.for("react.fragment"), no = Symbol.for("react.strict_mode"), ri = Symbol.for("react.profiler"), _c = Symbol.for("react.provider"), Tc = Symbol.for("react.context"), ro = Symbol.for("react.forward_ref"), ai = Symbol.for("react.suspense"), li = Symbol.for("react.suspense_list"), ao = Symbol.for("react.memo"), Yt = Symbol.for("react.lazy"), Dc = Symbol.for("react.offscreen"), cs = Symbol.iterator;
function dr(e) {
  return e === null || typeof e != "object" ? null : (e = cs && e[cs] || e["@@iterator"], typeof e == "function" ? e : null);
}
var pe = Object.assign, Pl;
function yr(e) {
  if (Pl === void 0) try {
    throw Error();
  } catch (n) {
    var t = n.stack.trim().match(/\n( *(at )?)/);
    Pl = t && t[1] || "";
  }
  return `
` + Pl + e;
}
var Fl = !1;
function Rl(e, t) {
  if (!e || Fl) return "";
  Fl = !0;
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
      } catch (f) {
        var r = f;
      }
      Reflect.construct(e, [], t);
    } else {
      try {
        t.call();
      } catch (f) {
        r = f;
      }
      e.call(t.prototype);
    }
    else {
      try {
        throw Error();
      } catch (f) {
        r = f;
      }
      e();
    }
  } catch (f) {
    if (f && r && typeof f.stack == "string") {
      for (var a = f.stack.split(`
`), i = r.stack.split(`
`), o = a.length - 1, s = i.length - 1; 1 <= o && 0 <= s && a[o] !== i[s]; ) s--;
      for (; 1 <= o && 0 <= s; o--, s--) if (a[o] !== i[s]) {
        if (o !== 1 || s !== 1)
          do
            if (o--, s--, 0 > s || a[o] !== i[s]) {
              var c = `
` + a[o].replace(" at new ", " at ");
              return e.displayName && c.includes("<anonymous>") && (c = c.replace("<anonymous>", e.displayName)), c;
            }
          while (1 <= o && 0 <= s);
        break;
      }
    }
  } finally {
    Fl = !1, Error.prepareStackTrace = n;
  }
  return (e = e ? e.displayName || e.name : "") ? yr(e) : "";
}
function gf(e) {
  switch (e.tag) {
    case 5:
      return yr(e.type);
    case 16:
      return yr("Lazy");
    case 13:
      return yr("Suspense");
    case 19:
      return yr("SuspenseList");
    case 0:
    case 2:
    case 15:
      return e = Rl(e.type, !1), e;
    case 11:
      return e = Rl(e.type.render, !1), e;
    case 1:
      return e = Rl(e.type, !0), e;
    default:
      return "";
  }
}
function ii(e) {
  if (e == null) return null;
  if (typeof e == "function") return e.displayName || e.name || null;
  if (typeof e == "string") return e;
  switch (e) {
    case Ln:
      return "Fragment";
    case zn:
      return "Portal";
    case ri:
      return "Profiler";
    case no:
      return "StrictMode";
    case ai:
      return "Suspense";
    case li:
      return "SuspenseList";
  }
  if (typeof e == "object") switch (e.$$typeof) {
    case Tc:
      return (e.displayName || "Context") + ".Consumer";
    case _c:
      return (e._context.displayName || "Context") + ".Provider";
    case ro:
      var t = e.render;
      return e = e.displayName, e || (e = t.displayName || t.name || "", e = e !== "" ? "ForwardRef(" + e + ")" : "ForwardRef"), e;
    case ao:
      return t = e.displayName || null, t !== null ? t : ii(e.type) || "Memo";
    case Yt:
      t = e._payload, e = e._init;
      try {
        return ii(e(t));
      } catch {
      }
  }
  return null;
}
function yf(e) {
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
      return ii(t);
    case 8:
      return t === no ? "StrictMode" : "Mode";
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
function dn(e) {
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
function zc(e) {
  var t = e.type;
  return (e = e.nodeName) && e.toLowerCase() === "input" && (t === "checkbox" || t === "radio");
}
function jf(e) {
  var t = zc(e) ? "checked" : "value", n = Object.getOwnPropertyDescriptor(e.constructor.prototype, t), r = "" + e[t];
  if (!e.hasOwnProperty(t) && typeof n < "u" && typeof n.get == "function" && typeof n.set == "function") {
    var a = n.get, i = n.set;
    return Object.defineProperty(e, t, { configurable: !0, get: function() {
      return a.call(this);
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
function ia(e) {
  e._valueTracker || (e._valueTracker = jf(e));
}
function Lc(e) {
  if (!e) return !1;
  var t = e._valueTracker;
  if (!t) return !0;
  var n = t.getValue(), r = "";
  return e && (r = zc(e) ? e.checked ? "true" : "false" : e.value), e = r, e !== n ? (t.setValue(e), !0) : !1;
}
function Da(e) {
  if (e = e || (typeof document < "u" ? document : void 0), typeof e > "u") return null;
  try {
    return e.activeElement || e.body;
  } catch {
    return e.body;
  }
}
function oi(e, t) {
  var n = t.checked;
  return pe({}, t, { defaultChecked: void 0, defaultValue: void 0, value: void 0, checked: n ?? e._wrapperState.initialChecked });
}
function us(e, t) {
  var n = t.defaultValue == null ? "" : t.defaultValue, r = t.checked != null ? t.checked : t.defaultChecked;
  n = dn(t.value != null ? t.value : n), e._wrapperState = { initialChecked: r, initialValue: n, controlled: t.type === "checkbox" || t.type === "radio" ? t.checked != null : t.value != null };
}
function Ac(e, t) {
  t = t.checked, t != null && to(e, "checked", t, !1);
}
function si(e, t) {
  Ac(e, t);
  var n = dn(t.value), r = t.type;
  if (n != null) r === "number" ? (n === 0 && e.value === "" || e.value != n) && (e.value = "" + n) : e.value !== "" + n && (e.value = "" + n);
  else if (r === "submit" || r === "reset") {
    e.removeAttribute("value");
    return;
  }
  t.hasOwnProperty("value") ? ci(e, t.type, n) : t.hasOwnProperty("defaultValue") && ci(e, t.type, dn(t.defaultValue)), t.checked == null && t.defaultChecked != null && (e.defaultChecked = !!t.defaultChecked);
}
function ds(e, t, n) {
  if (t.hasOwnProperty("value") || t.hasOwnProperty("defaultValue")) {
    var r = t.type;
    if (!(r !== "submit" && r !== "reset" || t.value !== void 0 && t.value !== null)) return;
    t = "" + e._wrapperState.initialValue, n || t === e.value || (e.value = t), e.defaultValue = t;
  }
  n = e.name, n !== "" && (e.name = ""), e.defaultChecked = !!e._wrapperState.initialChecked, n !== "" && (e.name = n);
}
function ci(e, t, n) {
  (t !== "number" || Da(e.ownerDocument) !== e) && (n == null ? e.defaultValue = "" + e._wrapperState.initialValue : e.defaultValue !== "" + n && (e.defaultValue = "" + n));
}
var jr = Array.isArray;
function Qn(e, t, n, r) {
  if (e = e.options, t) {
    t = {};
    for (var a = 0; a < n.length; a++) t["$" + n[a]] = !0;
    for (n = 0; n < e.length; n++) a = t.hasOwnProperty("$" + e[n].value), e[n].selected !== a && (e[n].selected = a), a && r && (e[n].defaultSelected = !0);
  } else {
    for (n = "" + dn(n), t = null, a = 0; a < e.length; a++) {
      if (e[a].value === n) {
        e[a].selected = !0, r && (e[a].defaultSelected = !0);
        return;
      }
      t !== null || e[a].disabled || (t = e[a]);
    }
    t !== null && (t.selected = !0);
  }
}
function ui(e, t) {
  if (t.dangerouslySetInnerHTML != null) throw Error(F(91));
  return pe({}, t, { value: void 0, defaultValue: void 0, children: "" + e._wrapperState.initialValue });
}
function fs(e, t) {
  var n = t.value;
  if (n == null) {
    if (n = t.children, t = t.defaultValue, n != null) {
      if (t != null) throw Error(F(92));
      if (jr(n)) {
        if (1 < n.length) throw Error(F(93));
        n = n[0];
      }
      t = n;
    }
    t == null && (t = ""), n = t;
  }
  e._wrapperState = { initialValue: dn(n) };
}
function $c(e, t) {
  var n = dn(t.value), r = dn(t.defaultValue);
  n != null && (n = "" + n, n !== e.value && (e.value = n), t.defaultValue == null && e.defaultValue !== n && (e.defaultValue = n)), r != null && (e.defaultValue = "" + r);
}
function ps(e) {
  var t = e.textContent;
  t === e._wrapperState.initialValue && t !== "" && t !== null && (e.value = t);
}
function Mc(e) {
  switch (e) {
    case "svg":
      return "http://www.w3.org/2000/svg";
    case "math":
      return "http://www.w3.org/1998/Math/MathML";
    default:
      return "http://www.w3.org/1999/xhtml";
  }
}
function di(e, t) {
  return e == null || e === "http://www.w3.org/1999/xhtml" ? Mc(t) : e === "http://www.w3.org/2000/svg" && t === "foreignObject" ? "http://www.w3.org/1999/xhtml" : e;
}
var oa, Oc = function(e) {
  return typeof MSApp < "u" && MSApp.execUnsafeLocalFunction ? function(t, n, r, a) {
    MSApp.execUnsafeLocalFunction(function() {
      return e(t, n, r, a);
    });
  } : e;
}(function(e, t) {
  if (e.namespaceURI !== "http://www.w3.org/2000/svg" || "innerHTML" in e) e.innerHTML = t;
  else {
    for (oa = oa || document.createElement("div"), oa.innerHTML = "<svg>" + t.valueOf().toString() + "</svg>", t = oa.firstChild; e.firstChild; ) e.removeChild(e.firstChild);
    for (; t.firstChild; ) e.appendChild(t.firstChild);
  }
});
function zr(e, t) {
  if (t) {
    var n = e.firstChild;
    if (n && n === e.lastChild && n.nodeType === 3) {
      n.nodeValue = t;
      return;
    }
  }
  e.textContent = t;
}
var wr = {
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
}, Nf = ["Webkit", "ms", "Moz", "O"];
Object.keys(wr).forEach(function(e) {
  Nf.forEach(function(t) {
    t = t + e.charAt(0).toUpperCase() + e.substring(1), wr[t] = wr[e];
  });
});
function bc(e, t, n) {
  return t == null || typeof t == "boolean" || t === "" ? "" : n || typeof t != "number" || t === 0 || wr.hasOwnProperty(e) && wr[e] ? ("" + t).trim() : t + "px";
}
function Uc(e, t) {
  e = e.style;
  for (var n in t) if (t.hasOwnProperty(n)) {
    var r = n.indexOf("--") === 0, a = bc(n, t[n], r);
    n === "float" && (n = "cssFloat"), r ? e.setProperty(n, a) : e[n] = a;
  }
}
var Sf = pe({ menuitem: !0 }, { area: !0, base: !0, br: !0, col: !0, embed: !0, hr: !0, img: !0, input: !0, keygen: !0, link: !0, meta: !0, param: !0, source: !0, track: !0, wbr: !0 });
function fi(e, t) {
  if (t) {
    if (Sf[e] && (t.children != null || t.dangerouslySetInnerHTML != null)) throw Error(F(137, e));
    if (t.dangerouslySetInnerHTML != null) {
      if (t.children != null) throw Error(F(60));
      if (typeof t.dangerouslySetInnerHTML != "object" || !("__html" in t.dangerouslySetInnerHTML)) throw Error(F(61));
    }
    if (t.style != null && typeof t.style != "object") throw Error(F(62));
  }
}
function pi(e, t) {
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
var mi = null;
function lo(e) {
  return e = e.target || e.srcElement || window, e.correspondingUseElement && (e = e.correspondingUseElement), e.nodeType === 3 ? e.parentNode : e;
}
var hi = null, Gn = null, Kn = null;
function ms(e) {
  if (e = ea(e)) {
    if (typeof hi != "function") throw Error(F(280));
    var t = e.stateNode;
    t && (t = sl(t), hi(e.stateNode, e.type, t));
  }
}
function Vc(e) {
  Gn ? Kn ? Kn.push(e) : Kn = [e] : Gn = e;
}
function Bc() {
  if (Gn) {
    var e = Gn, t = Kn;
    if (Kn = Gn = null, ms(e), t) for (e = 0; e < t.length; e++) ms(t[e]);
  }
}
function Hc(e, t) {
  return e(t);
}
function Wc() {
}
var _l = !1;
function Qc(e, t, n) {
  if (_l) return e(t, n);
  _l = !0;
  try {
    return Hc(e, t, n);
  } finally {
    _l = !1, (Gn !== null || Kn !== null) && (Wc(), Bc());
  }
}
function Lr(e, t) {
  var n = e.stateNode;
  if (n === null) return null;
  var r = sl(n);
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
  if (n && typeof n != "function") throw Error(F(231, t, typeof n));
  return n;
}
var vi = !1;
if (Vt) try {
  var fr = {};
  Object.defineProperty(fr, "passive", { get: function() {
    vi = !0;
  } }), window.addEventListener("test", fr, fr), window.removeEventListener("test", fr, fr);
} catch {
  vi = !1;
}
function wf(e, t, n, r, a, i, o, s, c) {
  var f = Array.prototype.slice.call(arguments, 3);
  try {
    t.apply(n, f);
  } catch (j) {
    this.onError(j);
  }
}
var Cr = !1, za = null, La = !1, xi = null, Cf = { onError: function(e) {
  Cr = !0, za = e;
} };
function kf(e, t, n, r, a, i, o, s, c) {
  Cr = !1, za = null, wf.apply(Cf, arguments);
}
function Ef(e, t, n, r, a, i, o, s, c) {
  if (kf.apply(this, arguments), Cr) {
    if (Cr) {
      var f = za;
      Cr = !1, za = null;
    } else throw Error(F(198));
    La || (La = !0, xi = f);
  }
}
function _n(e) {
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
function Gc(e) {
  if (e.tag === 13) {
    var t = e.memoizedState;
    if (t === null && (e = e.alternate, e !== null && (t = e.memoizedState)), t !== null) return t.dehydrated;
  }
  return null;
}
function hs(e) {
  if (_n(e) !== e) throw Error(F(188));
}
function If(e) {
  var t = e.alternate;
  if (!t) {
    if (t = _n(e), t === null) throw Error(F(188));
    return t !== e ? null : e;
  }
  for (var n = e, r = t; ; ) {
    var a = n.return;
    if (a === null) break;
    var i = a.alternate;
    if (i === null) {
      if (r = a.return, r !== null) {
        n = r;
        continue;
      }
      break;
    }
    if (a.child === i.child) {
      for (i = a.child; i; ) {
        if (i === n) return hs(a), e;
        if (i === r) return hs(a), t;
        i = i.sibling;
      }
      throw Error(F(188));
    }
    if (n.return !== r.return) n = a, r = i;
    else {
      for (var o = !1, s = a.child; s; ) {
        if (s === n) {
          o = !0, n = a, r = i;
          break;
        }
        if (s === r) {
          o = !0, r = a, n = i;
          break;
        }
        s = s.sibling;
      }
      if (!o) {
        for (s = i.child; s; ) {
          if (s === n) {
            o = !0, n = i, r = a;
            break;
          }
          if (s === r) {
            o = !0, r = i, n = a;
            break;
          }
          s = s.sibling;
        }
        if (!o) throw Error(F(189));
      }
    }
    if (n.alternate !== r) throw Error(F(190));
  }
  if (n.tag !== 3) throw Error(F(188));
  return n.stateNode.current === n ? e : t;
}
function Kc(e) {
  return e = If(e), e !== null ? qc(e) : null;
}
function qc(e) {
  if (e.tag === 5 || e.tag === 6) return e;
  for (e = e.child; e !== null; ) {
    var t = qc(e);
    if (t !== null) return t;
    e = e.sibling;
  }
  return null;
}
var Yc = lt.unstable_scheduleCallback, vs = lt.unstable_cancelCallback, Pf = lt.unstable_shouldYield, Ff = lt.unstable_requestPaint, he = lt.unstable_now, Rf = lt.unstable_getCurrentPriorityLevel, io = lt.unstable_ImmediatePriority, Xc = lt.unstable_UserBlockingPriority, Aa = lt.unstable_NormalPriority, _f = lt.unstable_LowPriority, Zc = lt.unstable_IdlePriority, al = null, Dt = null;
function Tf(e) {
  if (Dt && typeof Dt.onCommitFiberRoot == "function") try {
    Dt.onCommitFiberRoot(al, e, void 0, (e.current.flags & 128) === 128);
  } catch {
  }
}
var Ct = Math.clz32 ? Math.clz32 : Lf, Df = Math.log, zf = Math.LN2;
function Lf(e) {
  return e >>>= 0, e === 0 ? 32 : 31 - (Df(e) / zf | 0) | 0;
}
var sa = 64, ca = 4194304;
function Nr(e) {
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
function $a(e, t) {
  var n = e.pendingLanes;
  if (n === 0) return 0;
  var r = 0, a = e.suspendedLanes, i = e.pingedLanes, o = n & 268435455;
  if (o !== 0) {
    var s = o & ~a;
    s !== 0 ? r = Nr(s) : (i &= o, i !== 0 && (r = Nr(i)));
  } else o = n & ~a, o !== 0 ? r = Nr(o) : i !== 0 && (r = Nr(i));
  if (r === 0) return 0;
  if (t !== 0 && t !== r && !(t & a) && (a = r & -r, i = t & -t, a >= i || a === 16 && (i & 4194240) !== 0)) return t;
  if (r & 4 && (r |= n & 16), t = e.entangledLanes, t !== 0) for (e = e.entanglements, t &= r; 0 < t; ) n = 31 - Ct(t), a = 1 << n, r |= e[n], t &= ~a;
  return r;
}
function Af(e, t) {
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
function $f(e, t) {
  for (var n = e.suspendedLanes, r = e.pingedLanes, a = e.expirationTimes, i = e.pendingLanes; 0 < i; ) {
    var o = 31 - Ct(i), s = 1 << o, c = a[o];
    c === -1 ? (!(s & n) || s & r) && (a[o] = Af(s, t)) : c <= t && (e.expiredLanes |= s), i &= ~s;
  }
}
function gi(e) {
  return e = e.pendingLanes & -1073741825, e !== 0 ? e : e & 1073741824 ? 1073741824 : 0;
}
function Jc() {
  var e = sa;
  return sa <<= 1, !(sa & 4194240) && (sa = 64), e;
}
function Tl(e) {
  for (var t = [], n = 0; 31 > n; n++) t.push(e);
  return t;
}
function Zr(e, t, n) {
  e.pendingLanes |= t, t !== 536870912 && (e.suspendedLanes = 0, e.pingedLanes = 0), e = e.eventTimes, t = 31 - Ct(t), e[t] = n;
}
function Mf(e, t) {
  var n = e.pendingLanes & ~t;
  e.pendingLanes = t, e.suspendedLanes = 0, e.pingedLanes = 0, e.expiredLanes &= t, e.mutableReadLanes &= t, e.entangledLanes &= t, t = e.entanglements;
  var r = e.eventTimes;
  for (e = e.expirationTimes; 0 < n; ) {
    var a = 31 - Ct(n), i = 1 << a;
    t[a] = 0, r[a] = -1, e[a] = -1, n &= ~i;
  }
}
function oo(e, t) {
  var n = e.entangledLanes |= t;
  for (e = e.entanglements; n; ) {
    var r = 31 - Ct(n), a = 1 << r;
    a & t | e[r] & t && (e[r] |= t), n &= ~a;
  }
}
var te = 0;
function eu(e) {
  return e &= -e, 1 < e ? 4 < e ? e & 268435455 ? 16 : 536870912 : 4 : 1;
}
var tu, so, nu, ru, au, yi = !1, ua = [], nn = null, rn = null, an = null, Ar = /* @__PURE__ */ new Map(), $r = /* @__PURE__ */ new Map(), Zt = [], Of = "mousedown mouseup touchcancel touchend touchstart auxclick dblclick pointercancel pointerdown pointerup dragend dragstart drop compositionend compositionstart keydown keypress keyup input textInput copy cut paste click change contextmenu reset submit".split(" ");
function xs(e, t) {
  switch (e) {
    case "focusin":
    case "focusout":
      nn = null;
      break;
    case "dragenter":
    case "dragleave":
      rn = null;
      break;
    case "mouseover":
    case "mouseout":
      an = null;
      break;
    case "pointerover":
    case "pointerout":
      Ar.delete(t.pointerId);
      break;
    case "gotpointercapture":
    case "lostpointercapture":
      $r.delete(t.pointerId);
  }
}
function pr(e, t, n, r, a, i) {
  return e === null || e.nativeEvent !== i ? (e = { blockedOn: t, domEventName: n, eventSystemFlags: r, nativeEvent: i, targetContainers: [a] }, t !== null && (t = ea(t), t !== null && so(t)), e) : (e.eventSystemFlags |= r, t = e.targetContainers, a !== null && t.indexOf(a) === -1 && t.push(a), e);
}
function bf(e, t, n, r, a) {
  switch (t) {
    case "focusin":
      return nn = pr(nn, e, t, n, r, a), !0;
    case "dragenter":
      return rn = pr(rn, e, t, n, r, a), !0;
    case "mouseover":
      return an = pr(an, e, t, n, r, a), !0;
    case "pointerover":
      var i = a.pointerId;
      return Ar.set(i, pr(Ar.get(i) || null, e, t, n, r, a)), !0;
    case "gotpointercapture":
      return i = a.pointerId, $r.set(i, pr($r.get(i) || null, e, t, n, r, a)), !0;
  }
  return !1;
}
function lu(e) {
  var t = yn(e.target);
  if (t !== null) {
    var n = _n(t);
    if (n !== null) {
      if (t = n.tag, t === 13) {
        if (t = Gc(n), t !== null) {
          e.blockedOn = t, au(e.priority, function() {
            nu(n);
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
function wa(e) {
  if (e.blockedOn !== null) return !1;
  for (var t = e.targetContainers; 0 < t.length; ) {
    var n = ji(e.domEventName, e.eventSystemFlags, t[0], e.nativeEvent);
    if (n === null) {
      n = e.nativeEvent;
      var r = new n.constructor(n.type, n);
      mi = r, n.target.dispatchEvent(r), mi = null;
    } else return t = ea(n), t !== null && so(t), e.blockedOn = n, !1;
    t.shift();
  }
  return !0;
}
function gs(e, t, n) {
  wa(e) && n.delete(t);
}
function Uf() {
  yi = !1, nn !== null && wa(nn) && (nn = null), rn !== null && wa(rn) && (rn = null), an !== null && wa(an) && (an = null), Ar.forEach(gs), $r.forEach(gs);
}
function mr(e, t) {
  e.blockedOn === t && (e.blockedOn = null, yi || (yi = !0, lt.unstable_scheduleCallback(lt.unstable_NormalPriority, Uf)));
}
function Mr(e) {
  function t(a) {
    return mr(a, e);
  }
  if (0 < ua.length) {
    mr(ua[0], e);
    for (var n = 1; n < ua.length; n++) {
      var r = ua[n];
      r.blockedOn === e && (r.blockedOn = null);
    }
  }
  for (nn !== null && mr(nn, e), rn !== null && mr(rn, e), an !== null && mr(an, e), Ar.forEach(t), $r.forEach(t), n = 0; n < Zt.length; n++) r = Zt[n], r.blockedOn === e && (r.blockedOn = null);
  for (; 0 < Zt.length && (n = Zt[0], n.blockedOn === null); ) lu(n), n.blockedOn === null && Zt.shift();
}
var qn = Qt.ReactCurrentBatchConfig, Ma = !0;
function Vf(e, t, n, r) {
  var a = te, i = qn.transition;
  qn.transition = null;
  try {
    te = 1, co(e, t, n, r);
  } finally {
    te = a, qn.transition = i;
  }
}
function Bf(e, t, n, r) {
  var a = te, i = qn.transition;
  qn.transition = null;
  try {
    te = 4, co(e, t, n, r);
  } finally {
    te = a, qn.transition = i;
  }
}
function co(e, t, n, r) {
  if (Ma) {
    var a = ji(e, t, n, r);
    if (a === null) Vl(e, t, r, Oa, n), xs(e, r);
    else if (bf(a, e, t, n, r)) r.stopPropagation();
    else if (xs(e, r), t & 4 && -1 < Of.indexOf(e)) {
      for (; a !== null; ) {
        var i = ea(a);
        if (i !== null && tu(i), i = ji(e, t, n, r), i === null && Vl(e, t, r, Oa, n), i === a) break;
        a = i;
      }
      a !== null && r.stopPropagation();
    } else Vl(e, t, r, null, n);
  }
}
var Oa = null;
function ji(e, t, n, r) {
  if (Oa = null, e = lo(r), e = yn(e), e !== null) if (t = _n(e), t === null) e = null;
  else if (n = t.tag, n === 13) {
    if (e = Gc(t), e !== null) return e;
    e = null;
  } else if (n === 3) {
    if (t.stateNode.current.memoizedState.isDehydrated) return t.tag === 3 ? t.stateNode.containerInfo : null;
    e = null;
  } else t !== e && (e = null);
  return Oa = e, null;
}
function iu(e) {
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
      switch (Rf()) {
        case io:
          return 1;
        case Xc:
          return 4;
        case Aa:
        case _f:
          return 16;
        case Zc:
          return 536870912;
        default:
          return 16;
      }
    default:
      return 16;
  }
}
var en = null, uo = null, Ca = null;
function ou() {
  if (Ca) return Ca;
  var e, t = uo, n = t.length, r, a = "value" in en ? en.value : en.textContent, i = a.length;
  for (e = 0; e < n && t[e] === a[e]; e++) ;
  var o = n - e;
  for (r = 1; r <= o && t[n - r] === a[i - r]; r++) ;
  return Ca = a.slice(e, 1 < r ? 1 - r : void 0);
}
function ka(e) {
  var t = e.keyCode;
  return "charCode" in e ? (e = e.charCode, e === 0 && t === 13 && (e = 13)) : e = t, e === 10 && (e = 13), 32 <= e || e === 13 ? e : 0;
}
function da() {
  return !0;
}
function ys() {
  return !1;
}
function ot(e) {
  function t(n, r, a, i, o) {
    this._reactName = n, this._targetInst = a, this.type = r, this.nativeEvent = i, this.target = o, this.currentTarget = null;
    for (var s in e) e.hasOwnProperty(s) && (n = e[s], this[s] = n ? n(i) : i[s]);
    return this.isDefaultPrevented = (i.defaultPrevented != null ? i.defaultPrevented : i.returnValue === !1) ? da : ys, this.isPropagationStopped = ys, this;
  }
  return pe(t.prototype, { preventDefault: function() {
    this.defaultPrevented = !0;
    var n = this.nativeEvent;
    n && (n.preventDefault ? n.preventDefault() : typeof n.returnValue != "unknown" && (n.returnValue = !1), this.isDefaultPrevented = da);
  }, stopPropagation: function() {
    var n = this.nativeEvent;
    n && (n.stopPropagation ? n.stopPropagation() : typeof n.cancelBubble != "unknown" && (n.cancelBubble = !0), this.isPropagationStopped = da);
  }, persist: function() {
  }, isPersistent: da }), t;
}
var or = { eventPhase: 0, bubbles: 0, cancelable: 0, timeStamp: function(e) {
  return e.timeStamp || Date.now();
}, defaultPrevented: 0, isTrusted: 0 }, fo = ot(or), Jr = pe({}, or, { view: 0, detail: 0 }), Hf = ot(Jr), Dl, zl, hr, ll = pe({}, Jr, { screenX: 0, screenY: 0, clientX: 0, clientY: 0, pageX: 0, pageY: 0, ctrlKey: 0, shiftKey: 0, altKey: 0, metaKey: 0, getModifierState: po, button: 0, buttons: 0, relatedTarget: function(e) {
  return e.relatedTarget === void 0 ? e.fromElement === e.srcElement ? e.toElement : e.fromElement : e.relatedTarget;
}, movementX: function(e) {
  return "movementX" in e ? e.movementX : (e !== hr && (hr && e.type === "mousemove" ? (Dl = e.screenX - hr.screenX, zl = e.screenY - hr.screenY) : zl = Dl = 0, hr = e), Dl);
}, movementY: function(e) {
  return "movementY" in e ? e.movementY : zl;
} }), js = ot(ll), Wf = pe({}, ll, { dataTransfer: 0 }), Qf = ot(Wf), Gf = pe({}, Jr, { relatedTarget: 0 }), Ll = ot(Gf), Kf = pe({}, or, { animationName: 0, elapsedTime: 0, pseudoElement: 0 }), qf = ot(Kf), Yf = pe({}, or, { clipboardData: function(e) {
  return "clipboardData" in e ? e.clipboardData : window.clipboardData;
} }), Xf = ot(Yf), Zf = pe({}, or, { data: 0 }), Ns = ot(Zf), Jf = {
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
}, ep = {
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
}, tp = { Alt: "altKey", Control: "ctrlKey", Meta: "metaKey", Shift: "shiftKey" };
function np(e) {
  var t = this.nativeEvent;
  return t.getModifierState ? t.getModifierState(e) : (e = tp[e]) ? !!t[e] : !1;
}
function po() {
  return np;
}
var rp = pe({}, Jr, { key: function(e) {
  if (e.key) {
    var t = Jf[e.key] || e.key;
    if (t !== "Unidentified") return t;
  }
  return e.type === "keypress" ? (e = ka(e), e === 13 ? "Enter" : String.fromCharCode(e)) : e.type === "keydown" || e.type === "keyup" ? ep[e.keyCode] || "Unidentified" : "";
}, code: 0, location: 0, ctrlKey: 0, shiftKey: 0, altKey: 0, metaKey: 0, repeat: 0, locale: 0, getModifierState: po, charCode: function(e) {
  return e.type === "keypress" ? ka(e) : 0;
}, keyCode: function(e) {
  return e.type === "keydown" || e.type === "keyup" ? e.keyCode : 0;
}, which: function(e) {
  return e.type === "keypress" ? ka(e) : e.type === "keydown" || e.type === "keyup" ? e.keyCode : 0;
} }), ap = ot(rp), lp = pe({}, ll, { pointerId: 0, width: 0, height: 0, pressure: 0, tangentialPressure: 0, tiltX: 0, tiltY: 0, twist: 0, pointerType: 0, isPrimary: 0 }), Ss = ot(lp), ip = pe({}, Jr, { touches: 0, targetTouches: 0, changedTouches: 0, altKey: 0, metaKey: 0, ctrlKey: 0, shiftKey: 0, getModifierState: po }), op = ot(ip), sp = pe({}, or, { propertyName: 0, elapsedTime: 0, pseudoElement: 0 }), cp = ot(sp), up = pe({}, ll, {
  deltaX: function(e) {
    return "deltaX" in e ? e.deltaX : "wheelDeltaX" in e ? -e.wheelDeltaX : 0;
  },
  deltaY: function(e) {
    return "deltaY" in e ? e.deltaY : "wheelDeltaY" in e ? -e.wheelDeltaY : "wheelDelta" in e ? -e.wheelDelta : 0;
  },
  deltaZ: 0,
  deltaMode: 0
}), dp = ot(up), fp = [9, 13, 27, 32], mo = Vt && "CompositionEvent" in window, kr = null;
Vt && "documentMode" in document && (kr = document.documentMode);
var pp = Vt && "TextEvent" in window && !kr, su = Vt && (!mo || kr && 8 < kr && 11 >= kr), ws = " ", Cs = !1;
function cu(e, t) {
  switch (e) {
    case "keyup":
      return fp.indexOf(t.keyCode) !== -1;
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
function uu(e) {
  return e = e.detail, typeof e == "object" && "data" in e ? e.data : null;
}
var An = !1;
function mp(e, t) {
  switch (e) {
    case "compositionend":
      return uu(t);
    case "keypress":
      return t.which !== 32 ? null : (Cs = !0, ws);
    case "textInput":
      return e = t.data, e === ws && Cs ? null : e;
    default:
      return null;
  }
}
function hp(e, t) {
  if (An) return e === "compositionend" || !mo && cu(e, t) ? (e = ou(), Ca = uo = en = null, An = !1, e) : null;
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
      return su && t.locale !== "ko" ? null : t.data;
    default:
      return null;
  }
}
var vp = { color: !0, date: !0, datetime: !0, "datetime-local": !0, email: !0, month: !0, number: !0, password: !0, range: !0, search: !0, tel: !0, text: !0, time: !0, url: !0, week: !0 };
function ks(e) {
  var t = e && e.nodeName && e.nodeName.toLowerCase();
  return t === "input" ? !!vp[e.type] : t === "textarea";
}
function du(e, t, n, r) {
  Vc(r), t = ba(t, "onChange"), 0 < t.length && (n = new fo("onChange", "change", null, n, r), e.push({ event: n, listeners: t }));
}
var Er = null, Or = null;
function xp(e) {
  Su(e, 0);
}
function il(e) {
  var t = On(e);
  if (Lc(t)) return e;
}
function gp(e, t) {
  if (e === "change") return t;
}
var fu = !1;
if (Vt) {
  var Al;
  if (Vt) {
    var $l = "oninput" in document;
    if (!$l) {
      var Es = document.createElement("div");
      Es.setAttribute("oninput", "return;"), $l = typeof Es.oninput == "function";
    }
    Al = $l;
  } else Al = !1;
  fu = Al && (!document.documentMode || 9 < document.documentMode);
}
function Is() {
  Er && (Er.detachEvent("onpropertychange", pu), Or = Er = null);
}
function pu(e) {
  if (e.propertyName === "value" && il(Or)) {
    var t = [];
    du(t, Or, e, lo(e)), Qc(xp, t);
  }
}
function yp(e, t, n) {
  e === "focusin" ? (Is(), Er = t, Or = n, Er.attachEvent("onpropertychange", pu)) : e === "focusout" && Is();
}
function jp(e) {
  if (e === "selectionchange" || e === "keyup" || e === "keydown") return il(Or);
}
function Np(e, t) {
  if (e === "click") return il(t);
}
function Sp(e, t) {
  if (e === "input" || e === "change") return il(t);
}
function wp(e, t) {
  return e === t && (e !== 0 || 1 / e === 1 / t) || e !== e && t !== t;
}
var It = typeof Object.is == "function" ? Object.is : wp;
function br(e, t) {
  if (It(e, t)) return !0;
  if (typeof e != "object" || e === null || typeof t != "object" || t === null) return !1;
  var n = Object.keys(e), r = Object.keys(t);
  if (n.length !== r.length) return !1;
  for (r = 0; r < n.length; r++) {
    var a = n[r];
    if (!ni.call(t, a) || !It(e[a], t[a])) return !1;
  }
  return !0;
}
function Ps(e) {
  for (; e && e.firstChild; ) e = e.firstChild;
  return e;
}
function Fs(e, t) {
  var n = Ps(e);
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
    n = Ps(n);
  }
}
function mu(e, t) {
  return e && t ? e === t ? !0 : e && e.nodeType === 3 ? !1 : t && t.nodeType === 3 ? mu(e, t.parentNode) : "contains" in e ? e.contains(t) : e.compareDocumentPosition ? !!(e.compareDocumentPosition(t) & 16) : !1 : !1;
}
function hu() {
  for (var e = window, t = Da(); t instanceof e.HTMLIFrameElement; ) {
    try {
      var n = typeof t.contentWindow.location.href == "string";
    } catch {
      n = !1;
    }
    if (n) e = t.contentWindow;
    else break;
    t = Da(e.document);
  }
  return t;
}
function ho(e) {
  var t = e && e.nodeName && e.nodeName.toLowerCase();
  return t && (t === "input" && (e.type === "text" || e.type === "search" || e.type === "tel" || e.type === "url" || e.type === "password") || t === "textarea" || e.contentEditable === "true");
}
function Cp(e) {
  var t = hu(), n = e.focusedElem, r = e.selectionRange;
  if (t !== n && n && n.ownerDocument && mu(n.ownerDocument.documentElement, n)) {
    if (r !== null && ho(n)) {
      if (t = r.start, e = r.end, e === void 0 && (e = t), "selectionStart" in n) n.selectionStart = t, n.selectionEnd = Math.min(e, n.value.length);
      else if (e = (t = n.ownerDocument || document) && t.defaultView || window, e.getSelection) {
        e = e.getSelection();
        var a = n.textContent.length, i = Math.min(r.start, a);
        r = r.end === void 0 ? i : Math.min(r.end, a), !e.extend && i > r && (a = r, r = i, i = a), a = Fs(n, i);
        var o = Fs(
          n,
          r
        );
        a && o && (e.rangeCount !== 1 || e.anchorNode !== a.node || e.anchorOffset !== a.offset || e.focusNode !== o.node || e.focusOffset !== o.offset) && (t = t.createRange(), t.setStart(a.node, a.offset), e.removeAllRanges(), i > r ? (e.addRange(t), e.extend(o.node, o.offset)) : (t.setEnd(o.node, o.offset), e.addRange(t)));
      }
    }
    for (t = [], e = n; e = e.parentNode; ) e.nodeType === 1 && t.push({ element: e, left: e.scrollLeft, top: e.scrollTop });
    for (typeof n.focus == "function" && n.focus(), n = 0; n < t.length; n++) e = t[n], e.element.scrollLeft = e.left, e.element.scrollTop = e.top;
  }
}
var kp = Vt && "documentMode" in document && 11 >= document.documentMode, $n = null, Ni = null, Ir = null, Si = !1;
function Rs(e, t, n) {
  var r = n.window === n ? n.document : n.nodeType === 9 ? n : n.ownerDocument;
  Si || $n == null || $n !== Da(r) || (r = $n, "selectionStart" in r && ho(r) ? r = { start: r.selectionStart, end: r.selectionEnd } : (r = (r.ownerDocument && r.ownerDocument.defaultView || window).getSelection(), r = { anchorNode: r.anchorNode, anchorOffset: r.anchorOffset, focusNode: r.focusNode, focusOffset: r.focusOffset }), Ir && br(Ir, r) || (Ir = r, r = ba(Ni, "onSelect"), 0 < r.length && (t = new fo("onSelect", "select", null, t, n), e.push({ event: t, listeners: r }), t.target = $n)));
}
function fa(e, t) {
  var n = {};
  return n[e.toLowerCase()] = t.toLowerCase(), n["Webkit" + e] = "webkit" + t, n["Moz" + e] = "moz" + t, n;
}
var Mn = { animationend: fa("Animation", "AnimationEnd"), animationiteration: fa("Animation", "AnimationIteration"), animationstart: fa("Animation", "AnimationStart"), transitionend: fa("Transition", "TransitionEnd") }, Ml = {}, vu = {};
Vt && (vu = document.createElement("div").style, "AnimationEvent" in window || (delete Mn.animationend.animation, delete Mn.animationiteration.animation, delete Mn.animationstart.animation), "TransitionEvent" in window || delete Mn.transitionend.transition);
function ol(e) {
  if (Ml[e]) return Ml[e];
  if (!Mn[e]) return e;
  var t = Mn[e], n;
  for (n in t) if (t.hasOwnProperty(n) && n in vu) return Ml[e] = t[n];
  return e;
}
var xu = ol("animationend"), gu = ol("animationiteration"), yu = ol("animationstart"), ju = ol("transitionend"), Nu = /* @__PURE__ */ new Map(), _s = "abort auxClick cancel canPlay canPlayThrough click close contextMenu copy cut drag dragEnd dragEnter dragExit dragLeave dragOver dragStart drop durationChange emptied encrypted ended error gotPointerCapture input invalid keyDown keyPress keyUp load loadedData loadedMetadata loadStart lostPointerCapture mouseDown mouseMove mouseOut mouseOver mouseUp paste pause play playing pointerCancel pointerDown pointerMove pointerOut pointerOver pointerUp progress rateChange reset resize seeked seeking stalled submit suspend timeUpdate touchCancel touchEnd touchStart volumeChange scroll toggle touchMove waiting wheel".split(" ");
function mn(e, t) {
  Nu.set(e, t), Rn(t, [e]);
}
for (var Ol = 0; Ol < _s.length; Ol++) {
  var bl = _s[Ol], Ep = bl.toLowerCase(), Ip = bl[0].toUpperCase() + bl.slice(1);
  mn(Ep, "on" + Ip);
}
mn(xu, "onAnimationEnd");
mn(gu, "onAnimationIteration");
mn(yu, "onAnimationStart");
mn("dblclick", "onDoubleClick");
mn("focusin", "onFocus");
mn("focusout", "onBlur");
mn(ju, "onTransitionEnd");
Jn("onMouseEnter", ["mouseout", "mouseover"]);
Jn("onMouseLeave", ["mouseout", "mouseover"]);
Jn("onPointerEnter", ["pointerout", "pointerover"]);
Jn("onPointerLeave", ["pointerout", "pointerover"]);
Rn("onChange", "change click focusin focusout input keydown keyup selectionchange".split(" "));
Rn("onSelect", "focusout contextmenu dragend focusin keydown keyup mousedown mouseup selectionchange".split(" "));
Rn("onBeforeInput", ["compositionend", "keypress", "textInput", "paste"]);
Rn("onCompositionEnd", "compositionend focusout keydown keypress keyup mousedown".split(" "));
Rn("onCompositionStart", "compositionstart focusout keydown keypress keyup mousedown".split(" "));
Rn("onCompositionUpdate", "compositionupdate focusout keydown keypress keyup mousedown".split(" "));
var Sr = "abort canplay canplaythrough durationchange emptied encrypted ended error loadeddata loadedmetadata loadstart pause play playing progress ratechange resize seeked seeking stalled suspend timeupdate volumechange waiting".split(" "), Pp = new Set("cancel close invalid load scroll toggle".split(" ").concat(Sr));
function Ts(e, t, n) {
  var r = e.type || "unknown-event";
  e.currentTarget = n, Ef(r, t, void 0, e), e.currentTarget = null;
}
function Su(e, t) {
  t = (t & 4) !== 0;
  for (var n = 0; n < e.length; n++) {
    var r = e[n], a = r.event;
    r = r.listeners;
    e: {
      var i = void 0;
      if (t) for (var o = r.length - 1; 0 <= o; o--) {
        var s = r[o], c = s.instance, f = s.currentTarget;
        if (s = s.listener, c !== i && a.isPropagationStopped()) break e;
        Ts(a, s, f), i = c;
      }
      else for (o = 0; o < r.length; o++) {
        if (s = r[o], c = s.instance, f = s.currentTarget, s = s.listener, c !== i && a.isPropagationStopped()) break e;
        Ts(a, s, f), i = c;
      }
    }
  }
  if (La) throw e = xi, La = !1, xi = null, e;
}
function le(e, t) {
  var n = t[Ii];
  n === void 0 && (n = t[Ii] = /* @__PURE__ */ new Set());
  var r = e + "__bubble";
  n.has(r) || (wu(t, e, 2, !1), n.add(r));
}
function Ul(e, t, n) {
  var r = 0;
  t && (r |= 4), wu(n, e, r, t);
}
var pa = "_reactListening" + Math.random().toString(36).slice(2);
function Ur(e) {
  if (!e[pa]) {
    e[pa] = !0, Rc.forEach(function(n) {
      n !== "selectionchange" && (Pp.has(n) || Ul(n, !1, e), Ul(n, !0, e));
    });
    var t = e.nodeType === 9 ? e : e.ownerDocument;
    t === null || t[pa] || (t[pa] = !0, Ul("selectionchange", !1, t));
  }
}
function wu(e, t, n, r) {
  switch (iu(t)) {
    case 1:
      var a = Vf;
      break;
    case 4:
      a = Bf;
      break;
    default:
      a = co;
  }
  n = a.bind(null, t, n, e), a = void 0, !vi || t !== "touchstart" && t !== "touchmove" && t !== "wheel" || (a = !0), r ? a !== void 0 ? e.addEventListener(t, n, { capture: !0, passive: a }) : e.addEventListener(t, n, !0) : a !== void 0 ? e.addEventListener(t, n, { passive: a }) : e.addEventListener(t, n, !1);
}
function Vl(e, t, n, r, a) {
  var i = r;
  if (!(t & 1) && !(t & 2) && r !== null) e: for (; ; ) {
    if (r === null) return;
    var o = r.tag;
    if (o === 3 || o === 4) {
      var s = r.stateNode.containerInfo;
      if (s === a || s.nodeType === 8 && s.parentNode === a) break;
      if (o === 4) for (o = r.return; o !== null; ) {
        var c = o.tag;
        if ((c === 3 || c === 4) && (c = o.stateNode.containerInfo, c === a || c.nodeType === 8 && c.parentNode === a)) return;
        o = o.return;
      }
      for (; s !== null; ) {
        if (o = yn(s), o === null) return;
        if (c = o.tag, c === 5 || c === 6) {
          r = i = o;
          continue e;
        }
        s = s.parentNode;
      }
    }
    r = r.return;
  }
  Qc(function() {
    var f = i, j = lo(n), d = [];
    e: {
      var m = Nu.get(e);
      if (m !== void 0) {
        var h = fo, N = e;
        switch (e) {
          case "keypress":
            if (ka(n) === 0) break e;
          case "keydown":
          case "keyup":
            h = ap;
            break;
          case "focusin":
            N = "focus", h = Ll;
            break;
          case "focusout":
            N = "blur", h = Ll;
            break;
          case "beforeblur":
          case "afterblur":
            h = Ll;
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
            h = js;
            break;
          case "drag":
          case "dragend":
          case "dragenter":
          case "dragexit":
          case "dragleave":
          case "dragover":
          case "dragstart":
          case "drop":
            h = Qf;
            break;
          case "touchcancel":
          case "touchend":
          case "touchmove":
          case "touchstart":
            h = op;
            break;
          case xu:
          case gu:
          case yu:
            h = qf;
            break;
          case ju:
            h = cp;
            break;
          case "scroll":
            h = Hf;
            break;
          case "wheel":
            h = dp;
            break;
          case "copy":
          case "cut":
          case "paste":
            h = Xf;
            break;
          case "gotpointercapture":
          case "lostpointercapture":
          case "pointercancel":
          case "pointerdown":
          case "pointermove":
          case "pointerout":
          case "pointerover":
          case "pointerup":
            h = Ss;
        }
        var w = (t & 4) !== 0, M = !w && e === "scroll", p = w ? m !== null ? m + "Capture" : null : m;
        w = [];
        for (var u = f, v; u !== null; ) {
          v = u;
          var E = v.stateNode;
          if (v.tag === 5 && E !== null && (v = E, p !== null && (E = Lr(u, p), E != null && w.push(Vr(u, E, v)))), M) break;
          u = u.return;
        }
        0 < w.length && (m = new h(m, N, null, n, j), d.push({ event: m, listeners: w }));
      }
    }
    if (!(t & 7)) {
      e: {
        if (m = e === "mouseover" || e === "pointerover", h = e === "mouseout" || e === "pointerout", m && n !== mi && (N = n.relatedTarget || n.fromElement) && (yn(N) || N[Bt])) break e;
        if ((h || m) && (m = j.window === j ? j : (m = j.ownerDocument) ? m.defaultView || m.parentWindow : window, h ? (N = n.relatedTarget || n.toElement, h = f, N = N ? yn(N) : null, N !== null && (M = _n(N), N !== M || N.tag !== 5 && N.tag !== 6) && (N = null)) : (h = null, N = f), h !== N)) {
          if (w = js, E = "onMouseLeave", p = "onMouseEnter", u = "mouse", (e === "pointerout" || e === "pointerover") && (w = Ss, E = "onPointerLeave", p = "onPointerEnter", u = "pointer"), M = h == null ? m : On(h), v = N == null ? m : On(N), m = new w(E, u + "leave", h, n, j), m.target = M, m.relatedTarget = v, E = null, yn(j) === f && (w = new w(p, u + "enter", N, n, j), w.target = v, w.relatedTarget = M, E = w), M = E, h && N) t: {
            for (w = h, p = N, u = 0, v = w; v; v = Dn(v)) u++;
            for (v = 0, E = p; E; E = Dn(E)) v++;
            for (; 0 < u - v; ) w = Dn(w), u--;
            for (; 0 < v - u; ) p = Dn(p), v--;
            for (; u--; ) {
              if (w === p || p !== null && w === p.alternate) break t;
              w = Dn(w), p = Dn(p);
            }
            w = null;
          }
          else w = null;
          h !== null && Ds(d, m, h, w, !1), N !== null && M !== null && Ds(d, M, N, w, !0);
        }
      }
      e: {
        if (m = f ? On(f) : window, h = m.nodeName && m.nodeName.toLowerCase(), h === "select" || h === "input" && m.type === "file") var T = gp;
        else if (ks(m)) if (fu) T = Sp;
        else {
          T = jp;
          var R = yp;
        }
        else (h = m.nodeName) && h.toLowerCase() === "input" && (m.type === "checkbox" || m.type === "radio") && (T = Np);
        if (T && (T = T(e, f))) {
          du(d, T, n, j);
          break e;
        }
        R && R(e, m, f), e === "focusout" && (R = m._wrapperState) && R.controlled && m.type === "number" && ci(m, "number", m.value);
      }
      switch (R = f ? On(f) : window, e) {
        case "focusin":
          (ks(R) || R.contentEditable === "true") && ($n = R, Ni = f, Ir = null);
          break;
        case "focusout":
          Ir = Ni = $n = null;
          break;
        case "mousedown":
          Si = !0;
          break;
        case "contextmenu":
        case "mouseup":
        case "dragend":
          Si = !1, Rs(d, n, j);
          break;
        case "selectionchange":
          if (kp) break;
        case "keydown":
        case "keyup":
          Rs(d, n, j);
      }
      var D;
      if (mo) e: {
        switch (e) {
          case "compositionstart":
            var C = "onCompositionStart";
            break e;
          case "compositionend":
            C = "onCompositionEnd";
            break e;
          case "compositionupdate":
            C = "onCompositionUpdate";
            break e;
        }
        C = void 0;
      }
      else An ? cu(e, n) && (C = "onCompositionEnd") : e === "keydown" && n.keyCode === 229 && (C = "onCompositionStart");
      C && (su && n.locale !== "ko" && (An || C !== "onCompositionStart" ? C === "onCompositionEnd" && An && (D = ou()) : (en = j, uo = "value" in en ? en.value : en.textContent, An = !0)), R = ba(f, C), 0 < R.length && (C = new Ns(C, e, null, n, j), d.push({ event: C, listeners: R }), D ? C.data = D : (D = uu(n), D !== null && (C.data = D)))), (D = pp ? mp(e, n) : hp(e, n)) && (f = ba(f, "onBeforeInput"), 0 < f.length && (j = new Ns("onBeforeInput", "beforeinput", null, n, j), d.push({ event: j, listeners: f }), j.data = D));
    }
    Su(d, t);
  });
}
function Vr(e, t, n) {
  return { instance: e, listener: t, currentTarget: n };
}
function ba(e, t) {
  for (var n = t + "Capture", r = []; e !== null; ) {
    var a = e, i = a.stateNode;
    a.tag === 5 && i !== null && (a = i, i = Lr(e, n), i != null && r.unshift(Vr(e, i, a)), i = Lr(e, t), i != null && r.push(Vr(e, i, a))), e = e.return;
  }
  return r;
}
function Dn(e) {
  if (e === null) return null;
  do
    e = e.return;
  while (e && e.tag !== 5);
  return e || null;
}
function Ds(e, t, n, r, a) {
  for (var i = t._reactName, o = []; n !== null && n !== r; ) {
    var s = n, c = s.alternate, f = s.stateNode;
    if (c !== null && c === r) break;
    s.tag === 5 && f !== null && (s = f, a ? (c = Lr(n, i), c != null && o.unshift(Vr(n, c, s))) : a || (c = Lr(n, i), c != null && o.push(Vr(n, c, s)))), n = n.return;
  }
  o.length !== 0 && e.push({ event: t, listeners: o });
}
var Fp = /\r\n?/g, Rp = /\u0000|\uFFFD/g;
function zs(e) {
  return (typeof e == "string" ? e : "" + e).replace(Fp, `
`).replace(Rp, "");
}
function ma(e, t, n) {
  if (t = zs(t), zs(e) !== t && n) throw Error(F(425));
}
function Ua() {
}
var wi = null, Ci = null;
function ki(e, t) {
  return e === "textarea" || e === "noscript" || typeof t.children == "string" || typeof t.children == "number" || typeof t.dangerouslySetInnerHTML == "object" && t.dangerouslySetInnerHTML !== null && t.dangerouslySetInnerHTML.__html != null;
}
var Ei = typeof setTimeout == "function" ? setTimeout : void 0, _p = typeof clearTimeout == "function" ? clearTimeout : void 0, Ls = typeof Promise == "function" ? Promise : void 0, Tp = typeof queueMicrotask == "function" ? queueMicrotask : typeof Ls < "u" ? function(e) {
  return Ls.resolve(null).then(e).catch(Dp);
} : Ei;
function Dp(e) {
  setTimeout(function() {
    throw e;
  });
}
function Bl(e, t) {
  var n = t, r = 0;
  do {
    var a = n.nextSibling;
    if (e.removeChild(n), a && a.nodeType === 8) if (n = a.data, n === "/$") {
      if (r === 0) {
        e.removeChild(a), Mr(t);
        return;
      }
      r--;
    } else n !== "$" && n !== "$?" && n !== "$!" || r++;
    n = a;
  } while (n);
  Mr(t);
}
function ln(e) {
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
function As(e) {
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
var sr = Math.random().toString(36).slice(2), Tt = "__reactFiber$" + sr, Br = "__reactProps$" + sr, Bt = "__reactContainer$" + sr, Ii = "__reactEvents$" + sr, zp = "__reactListeners$" + sr, Lp = "__reactHandles$" + sr;
function yn(e) {
  var t = e[Tt];
  if (t) return t;
  for (var n = e.parentNode; n; ) {
    if (t = n[Bt] || n[Tt]) {
      if (n = t.alternate, t.child !== null || n !== null && n.child !== null) for (e = As(e); e !== null; ) {
        if (n = e[Tt]) return n;
        e = As(e);
      }
      return t;
    }
    e = n, n = e.parentNode;
  }
  return null;
}
function ea(e) {
  return e = e[Tt] || e[Bt], !e || e.tag !== 5 && e.tag !== 6 && e.tag !== 13 && e.tag !== 3 ? null : e;
}
function On(e) {
  if (e.tag === 5 || e.tag === 6) return e.stateNode;
  throw Error(F(33));
}
function sl(e) {
  return e[Br] || null;
}
var Pi = [], bn = -1;
function hn(e) {
  return { current: e };
}
function ie(e) {
  0 > bn || (e.current = Pi[bn], Pi[bn] = null, bn--);
}
function ae(e, t) {
  bn++, Pi[bn] = e.current, e.current = t;
}
var fn = {}, be = hn(fn), Ze = hn(!1), kn = fn;
function er(e, t) {
  var n = e.type.contextTypes;
  if (!n) return fn;
  var r = e.stateNode;
  if (r && r.__reactInternalMemoizedUnmaskedChildContext === t) return r.__reactInternalMemoizedMaskedChildContext;
  var a = {}, i;
  for (i in n) a[i] = t[i];
  return r && (e = e.stateNode, e.__reactInternalMemoizedUnmaskedChildContext = t, e.__reactInternalMemoizedMaskedChildContext = a), a;
}
function Je(e) {
  return e = e.childContextTypes, e != null;
}
function Va() {
  ie(Ze), ie(be);
}
function $s(e, t, n) {
  if (be.current !== fn) throw Error(F(168));
  ae(be, t), ae(Ze, n);
}
function Cu(e, t, n) {
  var r = e.stateNode;
  if (t = t.childContextTypes, typeof r.getChildContext != "function") return n;
  r = r.getChildContext();
  for (var a in r) if (!(a in t)) throw Error(F(108, yf(e) || "Unknown", a));
  return pe({}, n, r);
}
function Ba(e) {
  return e = (e = e.stateNode) && e.__reactInternalMemoizedMergedChildContext || fn, kn = be.current, ae(be, e), ae(Ze, Ze.current), !0;
}
function Ms(e, t, n) {
  var r = e.stateNode;
  if (!r) throw Error(F(169));
  n ? (e = Cu(e, t, kn), r.__reactInternalMemoizedMergedChildContext = e, ie(Ze), ie(be), ae(be, e)) : ie(Ze), ae(Ze, n);
}
var Mt = null, cl = !1, Hl = !1;
function ku(e) {
  Mt === null ? Mt = [e] : Mt.push(e);
}
function Ap(e) {
  cl = !0, ku(e);
}
function vn() {
  if (!Hl && Mt !== null) {
    Hl = !0;
    var e = 0, t = te;
    try {
      var n = Mt;
      for (te = 1; e < n.length; e++) {
        var r = n[e];
        do
          r = r(!0);
        while (r !== null);
      }
      Mt = null, cl = !1;
    } catch (a) {
      throw Mt !== null && (Mt = Mt.slice(e + 1)), Yc(io, vn), a;
    } finally {
      te = t, Hl = !1;
    }
  }
  return null;
}
var Un = [], Vn = 0, Ha = null, Wa = 0, dt = [], ft = 0, En = null, Ot = 1, bt = "";
function xn(e, t) {
  Un[Vn++] = Wa, Un[Vn++] = Ha, Ha = e, Wa = t;
}
function Eu(e, t, n) {
  dt[ft++] = Ot, dt[ft++] = bt, dt[ft++] = En, En = e;
  var r = Ot;
  e = bt;
  var a = 32 - Ct(r) - 1;
  r &= ~(1 << a), n += 1;
  var i = 32 - Ct(t) + a;
  if (30 < i) {
    var o = a - a % 5;
    i = (r & (1 << o) - 1).toString(32), r >>= o, a -= o, Ot = 1 << 32 - Ct(t) + a | n << a | r, bt = i + e;
  } else Ot = 1 << i | n << a | r, bt = e;
}
function vo(e) {
  e.return !== null && (xn(e, 1), Eu(e, 1, 0));
}
function xo(e) {
  for (; e === Ha; ) Ha = Un[--Vn], Un[Vn] = null, Wa = Un[--Vn], Un[Vn] = null;
  for (; e === En; ) En = dt[--ft], dt[ft] = null, bt = dt[--ft], dt[ft] = null, Ot = dt[--ft], dt[ft] = null;
}
var at = null, rt = null, oe = !1, wt = null;
function Iu(e, t) {
  var n = ht(5, null, null, 0);
  n.elementType = "DELETED", n.stateNode = t, n.return = e, t = e.deletions, t === null ? (e.deletions = [n], e.flags |= 16) : t.push(n);
}
function Os(e, t) {
  switch (e.tag) {
    case 5:
      var n = e.type;
      return t = t.nodeType !== 1 || n.toLowerCase() !== t.nodeName.toLowerCase() ? null : t, t !== null ? (e.stateNode = t, at = e, rt = ln(t.firstChild), !0) : !1;
    case 6:
      return t = e.pendingProps === "" || t.nodeType !== 3 ? null : t, t !== null ? (e.stateNode = t, at = e, rt = null, !0) : !1;
    case 13:
      return t = t.nodeType !== 8 ? null : t, t !== null ? (n = En !== null ? { id: Ot, overflow: bt } : null, e.memoizedState = { dehydrated: t, treeContext: n, retryLane: 1073741824 }, n = ht(18, null, null, 0), n.stateNode = t, n.return = e, e.child = n, at = e, rt = null, !0) : !1;
    default:
      return !1;
  }
}
function Fi(e) {
  return (e.mode & 1) !== 0 && (e.flags & 128) === 0;
}
function Ri(e) {
  if (oe) {
    var t = rt;
    if (t) {
      var n = t;
      if (!Os(e, t)) {
        if (Fi(e)) throw Error(F(418));
        t = ln(n.nextSibling);
        var r = at;
        t && Os(e, t) ? Iu(r, n) : (e.flags = e.flags & -4097 | 2, oe = !1, at = e);
      }
    } else {
      if (Fi(e)) throw Error(F(418));
      e.flags = e.flags & -4097 | 2, oe = !1, at = e;
    }
  }
}
function bs(e) {
  for (e = e.return; e !== null && e.tag !== 5 && e.tag !== 3 && e.tag !== 13; ) e = e.return;
  at = e;
}
function ha(e) {
  if (e !== at) return !1;
  if (!oe) return bs(e), oe = !0, !1;
  var t;
  if ((t = e.tag !== 3) && !(t = e.tag !== 5) && (t = e.type, t = t !== "head" && t !== "body" && !ki(e.type, e.memoizedProps)), t && (t = rt)) {
    if (Fi(e)) throw Pu(), Error(F(418));
    for (; t; ) Iu(e, t), t = ln(t.nextSibling);
  }
  if (bs(e), e.tag === 13) {
    if (e = e.memoizedState, e = e !== null ? e.dehydrated : null, !e) throw Error(F(317));
    e: {
      for (e = e.nextSibling, t = 0; e; ) {
        if (e.nodeType === 8) {
          var n = e.data;
          if (n === "/$") {
            if (t === 0) {
              rt = ln(e.nextSibling);
              break e;
            }
            t--;
          } else n !== "$" && n !== "$!" && n !== "$?" || t++;
        }
        e = e.nextSibling;
      }
      rt = null;
    }
  } else rt = at ? ln(e.stateNode.nextSibling) : null;
  return !0;
}
function Pu() {
  for (var e = rt; e; ) e = ln(e.nextSibling);
}
function tr() {
  rt = at = null, oe = !1;
}
function go(e) {
  wt === null ? wt = [e] : wt.push(e);
}
var $p = Qt.ReactCurrentBatchConfig;
function vr(e, t, n) {
  if (e = n.ref, e !== null && typeof e != "function" && typeof e != "object") {
    if (n._owner) {
      if (n = n._owner, n) {
        if (n.tag !== 1) throw Error(F(309));
        var r = n.stateNode;
      }
      if (!r) throw Error(F(147, e));
      var a = r, i = "" + e;
      return t !== null && t.ref !== null && typeof t.ref == "function" && t.ref._stringRef === i ? t.ref : (t = function(o) {
        var s = a.refs;
        o === null ? delete s[i] : s[i] = o;
      }, t._stringRef = i, t);
    }
    if (typeof e != "string") throw Error(F(284));
    if (!n._owner) throw Error(F(290, e));
  }
  return e;
}
function va(e, t) {
  throw e = Object.prototype.toString.call(t), Error(F(31, e === "[object Object]" ? "object with keys {" + Object.keys(t).join(", ") + "}" : e));
}
function Us(e) {
  var t = e._init;
  return t(e._payload);
}
function Fu(e) {
  function t(p, u) {
    if (e) {
      var v = p.deletions;
      v === null ? (p.deletions = [u], p.flags |= 16) : v.push(u);
    }
  }
  function n(p, u) {
    if (!e) return null;
    for (; u !== null; ) t(p, u), u = u.sibling;
    return null;
  }
  function r(p, u) {
    for (p = /* @__PURE__ */ new Map(); u !== null; ) u.key !== null ? p.set(u.key, u) : p.set(u.index, u), u = u.sibling;
    return p;
  }
  function a(p, u) {
    return p = un(p, u), p.index = 0, p.sibling = null, p;
  }
  function i(p, u, v) {
    return p.index = v, e ? (v = p.alternate, v !== null ? (v = v.index, v < u ? (p.flags |= 2, u) : v) : (p.flags |= 2, u)) : (p.flags |= 1048576, u);
  }
  function o(p) {
    return e && p.alternate === null && (p.flags |= 2), p;
  }
  function s(p, u, v, E) {
    return u === null || u.tag !== 6 ? (u = Xl(v, p.mode, E), u.return = p, u) : (u = a(u, v), u.return = p, u);
  }
  function c(p, u, v, E) {
    var T = v.type;
    return T === Ln ? j(p, u, v.props.children, E, v.key) : u !== null && (u.elementType === T || typeof T == "object" && T !== null && T.$$typeof === Yt && Us(T) === u.type) ? (E = a(u, v.props), E.ref = vr(p, u, v), E.return = p, E) : (E = Ta(v.type, v.key, v.props, null, p.mode, E), E.ref = vr(p, u, v), E.return = p, E);
  }
  function f(p, u, v, E) {
    return u === null || u.tag !== 4 || u.stateNode.containerInfo !== v.containerInfo || u.stateNode.implementation !== v.implementation ? (u = Zl(v, p.mode, E), u.return = p, u) : (u = a(u, v.children || []), u.return = p, u);
  }
  function j(p, u, v, E, T) {
    return u === null || u.tag !== 7 ? (u = wn(v, p.mode, E, T), u.return = p, u) : (u = a(u, v), u.return = p, u);
  }
  function d(p, u, v) {
    if (typeof u == "string" && u !== "" || typeof u == "number") return u = Xl("" + u, p.mode, v), u.return = p, u;
    if (typeof u == "object" && u !== null) {
      switch (u.$$typeof) {
        case la:
          return v = Ta(u.type, u.key, u.props, null, p.mode, v), v.ref = vr(p, null, u), v.return = p, v;
        case zn:
          return u = Zl(u, p.mode, v), u.return = p, u;
        case Yt:
          var E = u._init;
          return d(p, E(u._payload), v);
      }
      if (jr(u) || dr(u)) return u = wn(u, p.mode, v, null), u.return = p, u;
      va(p, u);
    }
    return null;
  }
  function m(p, u, v, E) {
    var T = u !== null ? u.key : null;
    if (typeof v == "string" && v !== "" || typeof v == "number") return T !== null ? null : s(p, u, "" + v, E);
    if (typeof v == "object" && v !== null) {
      switch (v.$$typeof) {
        case la:
          return v.key === T ? c(p, u, v, E) : null;
        case zn:
          return v.key === T ? f(p, u, v, E) : null;
        case Yt:
          return T = v._init, m(
            p,
            u,
            T(v._payload),
            E
          );
      }
      if (jr(v) || dr(v)) return T !== null ? null : j(p, u, v, E, null);
      va(p, v);
    }
    return null;
  }
  function h(p, u, v, E, T) {
    if (typeof E == "string" && E !== "" || typeof E == "number") return p = p.get(v) || null, s(u, p, "" + E, T);
    if (typeof E == "object" && E !== null) {
      switch (E.$$typeof) {
        case la:
          return p = p.get(E.key === null ? v : E.key) || null, c(u, p, E, T);
        case zn:
          return p = p.get(E.key === null ? v : E.key) || null, f(u, p, E, T);
        case Yt:
          var R = E._init;
          return h(p, u, v, R(E._payload), T);
      }
      if (jr(E) || dr(E)) return p = p.get(v) || null, j(u, p, E, T, null);
      va(u, E);
    }
    return null;
  }
  function N(p, u, v, E) {
    for (var T = null, R = null, D = u, C = u = 0, $ = null; D !== null && C < v.length; C++) {
      D.index > C ? ($ = D, D = null) : $ = D.sibling;
      var b = m(p, D, v[C], E);
      if (b === null) {
        D === null && (D = $);
        break;
      }
      e && D && b.alternate === null && t(p, D), u = i(b, u, C), R === null ? T = b : R.sibling = b, R = b, D = $;
    }
    if (C === v.length) return n(p, D), oe && xn(p, C), T;
    if (D === null) {
      for (; C < v.length; C++) D = d(p, v[C], E), D !== null && (u = i(D, u, C), R === null ? T = D : R.sibling = D, R = D);
      return oe && xn(p, C), T;
    }
    for (D = r(p, D); C < v.length; C++) $ = h(D, p, C, v[C], E), $ !== null && (e && $.alternate !== null && D.delete($.key === null ? C : $.key), u = i($, u, C), R === null ? T = $ : R.sibling = $, R = $);
    return e && D.forEach(function(P) {
      return t(p, P);
    }), oe && xn(p, C), T;
  }
  function w(p, u, v, E) {
    var T = dr(v);
    if (typeof T != "function") throw Error(F(150));
    if (v = T.call(v), v == null) throw Error(F(151));
    for (var R = T = null, D = u, C = u = 0, $ = null, b = v.next(); D !== null && !b.done; C++, b = v.next()) {
      D.index > C ? ($ = D, D = null) : $ = D.sibling;
      var P = m(p, D, b.value, E);
      if (P === null) {
        D === null && (D = $);
        break;
      }
      e && D && P.alternate === null && t(p, D), u = i(P, u, C), R === null ? T = P : R.sibling = P, R = P, D = $;
    }
    if (b.done) return n(
      p,
      D
    ), oe && xn(p, C), T;
    if (D === null) {
      for (; !b.done; C++, b = v.next()) b = d(p, b.value, E), b !== null && (u = i(b, u, C), R === null ? T = b : R.sibling = b, R = b);
      return oe && xn(p, C), T;
    }
    for (D = r(p, D); !b.done; C++, b = v.next()) b = h(D, p, C, b.value, E), b !== null && (e && b.alternate !== null && D.delete(b.key === null ? C : b.key), u = i(b, u, C), R === null ? T = b : R.sibling = b, R = b);
    return e && D.forEach(function(W) {
      return t(p, W);
    }), oe && xn(p, C), T;
  }
  function M(p, u, v, E) {
    if (typeof v == "object" && v !== null && v.type === Ln && v.key === null && (v = v.props.children), typeof v == "object" && v !== null) {
      switch (v.$$typeof) {
        case la:
          e: {
            for (var T = v.key, R = u; R !== null; ) {
              if (R.key === T) {
                if (T = v.type, T === Ln) {
                  if (R.tag === 7) {
                    n(p, R.sibling), u = a(R, v.props.children), u.return = p, p = u;
                    break e;
                  }
                } else if (R.elementType === T || typeof T == "object" && T !== null && T.$$typeof === Yt && Us(T) === R.type) {
                  n(p, R.sibling), u = a(R, v.props), u.ref = vr(p, R, v), u.return = p, p = u;
                  break e;
                }
                n(p, R);
                break;
              } else t(p, R);
              R = R.sibling;
            }
            v.type === Ln ? (u = wn(v.props.children, p.mode, E, v.key), u.return = p, p = u) : (E = Ta(v.type, v.key, v.props, null, p.mode, E), E.ref = vr(p, u, v), E.return = p, p = E);
          }
          return o(p);
        case zn:
          e: {
            for (R = v.key; u !== null; ) {
              if (u.key === R) if (u.tag === 4 && u.stateNode.containerInfo === v.containerInfo && u.stateNode.implementation === v.implementation) {
                n(p, u.sibling), u = a(u, v.children || []), u.return = p, p = u;
                break e;
              } else {
                n(p, u);
                break;
              }
              else t(p, u);
              u = u.sibling;
            }
            u = Zl(v, p.mode, E), u.return = p, p = u;
          }
          return o(p);
        case Yt:
          return R = v._init, M(p, u, R(v._payload), E);
      }
      if (jr(v)) return N(p, u, v, E);
      if (dr(v)) return w(p, u, v, E);
      va(p, v);
    }
    return typeof v == "string" && v !== "" || typeof v == "number" ? (v = "" + v, u !== null && u.tag === 6 ? (n(p, u.sibling), u = a(u, v), u.return = p, p = u) : (n(p, u), u = Xl(v, p.mode, E), u.return = p, p = u), o(p)) : n(p, u);
  }
  return M;
}
var nr = Fu(!0), Ru = Fu(!1), Qa = hn(null), Ga = null, Bn = null, yo = null;
function jo() {
  yo = Bn = Ga = null;
}
function No(e) {
  var t = Qa.current;
  ie(Qa), e._currentValue = t;
}
function _i(e, t, n) {
  for (; e !== null; ) {
    var r = e.alternate;
    if ((e.childLanes & t) !== t ? (e.childLanes |= t, r !== null && (r.childLanes |= t)) : r !== null && (r.childLanes & t) !== t && (r.childLanes |= t), e === n) break;
    e = e.return;
  }
}
function Yn(e, t) {
  Ga = e, yo = Bn = null, e = e.dependencies, e !== null && e.firstContext !== null && (e.lanes & t && (Xe = !0), e.firstContext = null);
}
function xt(e) {
  var t = e._currentValue;
  if (yo !== e) if (e = { context: e, memoizedValue: t, next: null }, Bn === null) {
    if (Ga === null) throw Error(F(308));
    Bn = e, Ga.dependencies = { lanes: 0, firstContext: e };
  } else Bn = Bn.next = e;
  return t;
}
var jn = null;
function So(e) {
  jn === null ? jn = [e] : jn.push(e);
}
function _u(e, t, n, r) {
  var a = t.interleaved;
  return a === null ? (n.next = n, So(t)) : (n.next = a.next, a.next = n), t.interleaved = n, Ht(e, r);
}
function Ht(e, t) {
  e.lanes |= t;
  var n = e.alternate;
  for (n !== null && (n.lanes |= t), n = e, e = e.return; e !== null; ) e.childLanes |= t, n = e.alternate, n !== null && (n.childLanes |= t), n = e, e = e.return;
  return n.tag === 3 ? n.stateNode : null;
}
var Xt = !1;
function wo(e) {
  e.updateQueue = { baseState: e.memoizedState, firstBaseUpdate: null, lastBaseUpdate: null, shared: { pending: null, interleaved: null, lanes: 0 }, effects: null };
}
function Tu(e, t) {
  e = e.updateQueue, t.updateQueue === e && (t.updateQueue = { baseState: e.baseState, firstBaseUpdate: e.firstBaseUpdate, lastBaseUpdate: e.lastBaseUpdate, shared: e.shared, effects: e.effects });
}
function Ut(e, t) {
  return { eventTime: e, lane: t, tag: 0, payload: null, callback: null, next: null };
}
function on(e, t, n) {
  var r = e.updateQueue;
  if (r === null) return null;
  if (r = r.shared, Z & 2) {
    var a = r.pending;
    return a === null ? t.next = t : (t.next = a.next, a.next = t), r.pending = t, Ht(e, n);
  }
  return a = r.interleaved, a === null ? (t.next = t, So(r)) : (t.next = a.next, a.next = t), r.interleaved = t, Ht(e, n);
}
function Ea(e, t, n) {
  if (t = t.updateQueue, t !== null && (t = t.shared, (n & 4194240) !== 0)) {
    var r = t.lanes;
    r &= e.pendingLanes, n |= r, t.lanes = n, oo(e, n);
  }
}
function Vs(e, t) {
  var n = e.updateQueue, r = e.alternate;
  if (r !== null && (r = r.updateQueue, n === r)) {
    var a = null, i = null;
    if (n = n.firstBaseUpdate, n !== null) {
      do {
        var o = { eventTime: n.eventTime, lane: n.lane, tag: n.tag, payload: n.payload, callback: n.callback, next: null };
        i === null ? a = i = o : i = i.next = o, n = n.next;
      } while (n !== null);
      i === null ? a = i = t : i = i.next = t;
    } else a = i = t;
    n = { baseState: r.baseState, firstBaseUpdate: a, lastBaseUpdate: i, shared: r.shared, effects: r.effects }, e.updateQueue = n;
    return;
  }
  e = n.lastBaseUpdate, e === null ? n.firstBaseUpdate = t : e.next = t, n.lastBaseUpdate = t;
}
function Ka(e, t, n, r) {
  var a = e.updateQueue;
  Xt = !1;
  var i = a.firstBaseUpdate, o = a.lastBaseUpdate, s = a.shared.pending;
  if (s !== null) {
    a.shared.pending = null;
    var c = s, f = c.next;
    c.next = null, o === null ? i = f : o.next = f, o = c;
    var j = e.alternate;
    j !== null && (j = j.updateQueue, s = j.lastBaseUpdate, s !== o && (s === null ? j.firstBaseUpdate = f : s.next = f, j.lastBaseUpdate = c));
  }
  if (i !== null) {
    var d = a.baseState;
    o = 0, j = f = c = null, s = i;
    do {
      var m = s.lane, h = s.eventTime;
      if ((r & m) === m) {
        j !== null && (j = j.next = {
          eventTime: h,
          lane: 0,
          tag: s.tag,
          payload: s.payload,
          callback: s.callback,
          next: null
        });
        e: {
          var N = e, w = s;
          switch (m = t, h = n, w.tag) {
            case 1:
              if (N = w.payload, typeof N == "function") {
                d = N.call(h, d, m);
                break e;
              }
              d = N;
              break e;
            case 3:
              N.flags = N.flags & -65537 | 128;
            case 0:
              if (N = w.payload, m = typeof N == "function" ? N.call(h, d, m) : N, m == null) break e;
              d = pe({}, d, m);
              break e;
            case 2:
              Xt = !0;
          }
        }
        s.callback !== null && s.lane !== 0 && (e.flags |= 64, m = a.effects, m === null ? a.effects = [s] : m.push(s));
      } else h = { eventTime: h, lane: m, tag: s.tag, payload: s.payload, callback: s.callback, next: null }, j === null ? (f = j = h, c = d) : j = j.next = h, o |= m;
      if (s = s.next, s === null) {
        if (s = a.shared.pending, s === null) break;
        m = s, s = m.next, m.next = null, a.lastBaseUpdate = m, a.shared.pending = null;
      }
    } while (!0);
    if (j === null && (c = d), a.baseState = c, a.firstBaseUpdate = f, a.lastBaseUpdate = j, t = a.shared.interleaved, t !== null) {
      a = t;
      do
        o |= a.lane, a = a.next;
      while (a !== t);
    } else i === null && (a.shared.lanes = 0);
    Pn |= o, e.lanes = o, e.memoizedState = d;
  }
}
function Bs(e, t, n) {
  if (e = t.effects, t.effects = null, e !== null) for (t = 0; t < e.length; t++) {
    var r = e[t], a = r.callback;
    if (a !== null) {
      if (r.callback = null, r = n, typeof a != "function") throw Error(F(191, a));
      a.call(r);
    }
  }
}
var ta = {}, zt = hn(ta), Hr = hn(ta), Wr = hn(ta);
function Nn(e) {
  if (e === ta) throw Error(F(174));
  return e;
}
function Co(e, t) {
  switch (ae(Wr, t), ae(Hr, e), ae(zt, ta), e = t.nodeType, e) {
    case 9:
    case 11:
      t = (t = t.documentElement) ? t.namespaceURI : di(null, "");
      break;
    default:
      e = e === 8 ? t.parentNode : t, t = e.namespaceURI || null, e = e.tagName, t = di(t, e);
  }
  ie(zt), ae(zt, t);
}
function rr() {
  ie(zt), ie(Hr), ie(Wr);
}
function Du(e) {
  Nn(Wr.current);
  var t = Nn(zt.current), n = di(t, e.type);
  t !== n && (ae(Hr, e), ae(zt, n));
}
function ko(e) {
  Hr.current === e && (ie(zt), ie(Hr));
}
var ue = hn(0);
function qa(e) {
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
var Wl = [];
function Eo() {
  for (var e = 0; e < Wl.length; e++) Wl[e]._workInProgressVersionPrimary = null;
  Wl.length = 0;
}
var Ia = Qt.ReactCurrentDispatcher, Ql = Qt.ReactCurrentBatchConfig, In = 0, fe = null, we = null, Pe = null, Ya = !1, Pr = !1, Qr = 0, Mp = 0;
function Le() {
  throw Error(F(321));
}
function Io(e, t) {
  if (t === null) return !1;
  for (var n = 0; n < t.length && n < e.length; n++) if (!It(e[n], t[n])) return !1;
  return !0;
}
function Po(e, t, n, r, a, i) {
  if (In = i, fe = t, t.memoizedState = null, t.updateQueue = null, t.lanes = 0, Ia.current = e === null || e.memoizedState === null ? Vp : Bp, e = n(r, a), Pr) {
    i = 0;
    do {
      if (Pr = !1, Qr = 0, 25 <= i) throw Error(F(301));
      i += 1, Pe = we = null, t.updateQueue = null, Ia.current = Hp, e = n(r, a);
    } while (Pr);
  }
  if (Ia.current = Xa, t = we !== null && we.next !== null, In = 0, Pe = we = fe = null, Ya = !1, t) throw Error(F(300));
  return e;
}
function Fo() {
  var e = Qr !== 0;
  return Qr = 0, e;
}
function _t() {
  var e = { memoizedState: null, baseState: null, baseQueue: null, queue: null, next: null };
  return Pe === null ? fe.memoizedState = Pe = e : Pe = Pe.next = e, Pe;
}
function gt() {
  if (we === null) {
    var e = fe.alternate;
    e = e !== null ? e.memoizedState : null;
  } else e = we.next;
  var t = Pe === null ? fe.memoizedState : Pe.next;
  if (t !== null) Pe = t, we = e;
  else {
    if (e === null) throw Error(F(310));
    we = e, e = { memoizedState: we.memoizedState, baseState: we.baseState, baseQueue: we.baseQueue, queue: we.queue, next: null }, Pe === null ? fe.memoizedState = Pe = e : Pe = Pe.next = e;
  }
  return Pe;
}
function Gr(e, t) {
  return typeof t == "function" ? t(e) : t;
}
function Gl(e) {
  var t = gt(), n = t.queue;
  if (n === null) throw Error(F(311));
  n.lastRenderedReducer = e;
  var r = we, a = r.baseQueue, i = n.pending;
  if (i !== null) {
    if (a !== null) {
      var o = a.next;
      a.next = i.next, i.next = o;
    }
    r.baseQueue = a = i, n.pending = null;
  }
  if (a !== null) {
    i = a.next, r = r.baseState;
    var s = o = null, c = null, f = i;
    do {
      var j = f.lane;
      if ((In & j) === j) c !== null && (c = c.next = { lane: 0, action: f.action, hasEagerState: f.hasEagerState, eagerState: f.eagerState, next: null }), r = f.hasEagerState ? f.eagerState : e(r, f.action);
      else {
        var d = {
          lane: j,
          action: f.action,
          hasEagerState: f.hasEagerState,
          eagerState: f.eagerState,
          next: null
        };
        c === null ? (s = c = d, o = r) : c = c.next = d, fe.lanes |= j, Pn |= j;
      }
      f = f.next;
    } while (f !== null && f !== i);
    c === null ? o = r : c.next = s, It(r, t.memoizedState) || (Xe = !0), t.memoizedState = r, t.baseState = o, t.baseQueue = c, n.lastRenderedState = r;
  }
  if (e = n.interleaved, e !== null) {
    a = e;
    do
      i = a.lane, fe.lanes |= i, Pn |= i, a = a.next;
    while (a !== e);
  } else a === null && (n.lanes = 0);
  return [t.memoizedState, n.dispatch];
}
function Kl(e) {
  var t = gt(), n = t.queue;
  if (n === null) throw Error(F(311));
  n.lastRenderedReducer = e;
  var r = n.dispatch, a = n.pending, i = t.memoizedState;
  if (a !== null) {
    n.pending = null;
    var o = a = a.next;
    do
      i = e(i, o.action), o = o.next;
    while (o !== a);
    It(i, t.memoizedState) || (Xe = !0), t.memoizedState = i, t.baseQueue === null && (t.baseState = i), n.lastRenderedState = i;
  }
  return [i, r];
}
function zu() {
}
function Lu(e, t) {
  var n = fe, r = gt(), a = t(), i = !It(r.memoizedState, a);
  if (i && (r.memoizedState = a, Xe = !0), r = r.queue, Ro(Mu.bind(null, n, r, e), [e]), r.getSnapshot !== t || i || Pe !== null && Pe.memoizedState.tag & 1) {
    if (n.flags |= 2048, Kr(9, $u.bind(null, n, r, a, t), void 0, null), Fe === null) throw Error(F(349));
    In & 30 || Au(n, t, a);
  }
  return a;
}
function Au(e, t, n) {
  e.flags |= 16384, e = { getSnapshot: t, value: n }, t = fe.updateQueue, t === null ? (t = { lastEffect: null, stores: null }, fe.updateQueue = t, t.stores = [e]) : (n = t.stores, n === null ? t.stores = [e] : n.push(e));
}
function $u(e, t, n, r) {
  t.value = n, t.getSnapshot = r, Ou(t) && bu(e);
}
function Mu(e, t, n) {
  return n(function() {
    Ou(t) && bu(e);
  });
}
function Ou(e) {
  var t = e.getSnapshot;
  e = e.value;
  try {
    var n = t();
    return !It(e, n);
  } catch {
    return !0;
  }
}
function bu(e) {
  var t = Ht(e, 1);
  t !== null && kt(t, e, 1, -1);
}
function Hs(e) {
  var t = _t();
  return typeof e == "function" && (e = e()), t.memoizedState = t.baseState = e, e = { pending: null, interleaved: null, lanes: 0, dispatch: null, lastRenderedReducer: Gr, lastRenderedState: e }, t.queue = e, e = e.dispatch = Up.bind(null, fe, e), [t.memoizedState, e];
}
function Kr(e, t, n, r) {
  return e = { tag: e, create: t, destroy: n, deps: r, next: null }, t = fe.updateQueue, t === null ? (t = { lastEffect: null, stores: null }, fe.updateQueue = t, t.lastEffect = e.next = e) : (n = t.lastEffect, n === null ? t.lastEffect = e.next = e : (r = n.next, n.next = e, e.next = r, t.lastEffect = e)), e;
}
function Uu() {
  return gt().memoizedState;
}
function Pa(e, t, n, r) {
  var a = _t();
  fe.flags |= e, a.memoizedState = Kr(1 | t, n, void 0, r === void 0 ? null : r);
}
function ul(e, t, n, r) {
  var a = gt();
  r = r === void 0 ? null : r;
  var i = void 0;
  if (we !== null) {
    var o = we.memoizedState;
    if (i = o.destroy, r !== null && Io(r, o.deps)) {
      a.memoizedState = Kr(t, n, i, r);
      return;
    }
  }
  fe.flags |= e, a.memoizedState = Kr(1 | t, n, i, r);
}
function Ws(e, t) {
  return Pa(8390656, 8, e, t);
}
function Ro(e, t) {
  return ul(2048, 8, e, t);
}
function Vu(e, t) {
  return ul(4, 2, e, t);
}
function Bu(e, t) {
  return ul(4, 4, e, t);
}
function Hu(e, t) {
  if (typeof t == "function") return e = e(), t(e), function() {
    t(null);
  };
  if (t != null) return e = e(), t.current = e, function() {
    t.current = null;
  };
}
function Wu(e, t, n) {
  return n = n != null ? n.concat([e]) : null, ul(4, 4, Hu.bind(null, t, e), n);
}
function _o() {
}
function Qu(e, t) {
  var n = gt();
  t = t === void 0 ? null : t;
  var r = n.memoizedState;
  return r !== null && t !== null && Io(t, r[1]) ? r[0] : (n.memoizedState = [e, t], e);
}
function Gu(e, t) {
  var n = gt();
  t = t === void 0 ? null : t;
  var r = n.memoizedState;
  return r !== null && t !== null && Io(t, r[1]) ? r[0] : (e = e(), n.memoizedState = [e, t], e);
}
function Ku(e, t, n) {
  return In & 21 ? (It(n, t) || (n = Jc(), fe.lanes |= n, Pn |= n, e.baseState = !0), t) : (e.baseState && (e.baseState = !1, Xe = !0), e.memoizedState = n);
}
function Op(e, t) {
  var n = te;
  te = n !== 0 && 4 > n ? n : 4, e(!0);
  var r = Ql.transition;
  Ql.transition = {};
  try {
    e(!1), t();
  } finally {
    te = n, Ql.transition = r;
  }
}
function qu() {
  return gt().memoizedState;
}
function bp(e, t, n) {
  var r = cn(e);
  if (n = { lane: r, action: n, hasEagerState: !1, eagerState: null, next: null }, Yu(e)) Xu(t, n);
  else if (n = _u(e, t, n, r), n !== null) {
    var a = Qe();
    kt(n, e, r, a), Zu(n, t, r);
  }
}
function Up(e, t, n) {
  var r = cn(e), a = { lane: r, action: n, hasEagerState: !1, eagerState: null, next: null };
  if (Yu(e)) Xu(t, a);
  else {
    var i = e.alternate;
    if (e.lanes === 0 && (i === null || i.lanes === 0) && (i = t.lastRenderedReducer, i !== null)) try {
      var o = t.lastRenderedState, s = i(o, n);
      if (a.hasEagerState = !0, a.eagerState = s, It(s, o)) {
        var c = t.interleaved;
        c === null ? (a.next = a, So(t)) : (a.next = c.next, c.next = a), t.interleaved = a;
        return;
      }
    } catch {
    } finally {
    }
    n = _u(e, t, a, r), n !== null && (a = Qe(), kt(n, e, r, a), Zu(n, t, r));
  }
}
function Yu(e) {
  var t = e.alternate;
  return e === fe || t !== null && t === fe;
}
function Xu(e, t) {
  Pr = Ya = !0;
  var n = e.pending;
  n === null ? t.next = t : (t.next = n.next, n.next = t), e.pending = t;
}
function Zu(e, t, n) {
  if (n & 4194240) {
    var r = t.lanes;
    r &= e.pendingLanes, n |= r, t.lanes = n, oo(e, n);
  }
}
var Xa = { readContext: xt, useCallback: Le, useContext: Le, useEffect: Le, useImperativeHandle: Le, useInsertionEffect: Le, useLayoutEffect: Le, useMemo: Le, useReducer: Le, useRef: Le, useState: Le, useDebugValue: Le, useDeferredValue: Le, useTransition: Le, useMutableSource: Le, useSyncExternalStore: Le, useId: Le, unstable_isNewReconciler: !1 }, Vp = { readContext: xt, useCallback: function(e, t) {
  return _t().memoizedState = [e, t === void 0 ? null : t], e;
}, useContext: xt, useEffect: Ws, useImperativeHandle: function(e, t, n) {
  return n = n != null ? n.concat([e]) : null, Pa(
    4194308,
    4,
    Hu.bind(null, t, e),
    n
  );
}, useLayoutEffect: function(e, t) {
  return Pa(4194308, 4, e, t);
}, useInsertionEffect: function(e, t) {
  return Pa(4, 2, e, t);
}, useMemo: function(e, t) {
  var n = _t();
  return t = t === void 0 ? null : t, e = e(), n.memoizedState = [e, t], e;
}, useReducer: function(e, t, n) {
  var r = _t();
  return t = n !== void 0 ? n(t) : t, r.memoizedState = r.baseState = t, e = { pending: null, interleaved: null, lanes: 0, dispatch: null, lastRenderedReducer: e, lastRenderedState: t }, r.queue = e, e = e.dispatch = bp.bind(null, fe, e), [r.memoizedState, e];
}, useRef: function(e) {
  var t = _t();
  return e = { current: e }, t.memoizedState = e;
}, useState: Hs, useDebugValue: _o, useDeferredValue: function(e) {
  return _t().memoizedState = e;
}, useTransition: function() {
  var e = Hs(!1), t = e[0];
  return e = Op.bind(null, e[1]), _t().memoizedState = e, [t, e];
}, useMutableSource: function() {
}, useSyncExternalStore: function(e, t, n) {
  var r = fe, a = _t();
  if (oe) {
    if (n === void 0) throw Error(F(407));
    n = n();
  } else {
    if (n = t(), Fe === null) throw Error(F(349));
    In & 30 || Au(r, t, n);
  }
  a.memoizedState = n;
  var i = { value: n, getSnapshot: t };
  return a.queue = i, Ws(Mu.bind(
    null,
    r,
    i,
    e
  ), [e]), r.flags |= 2048, Kr(9, $u.bind(null, r, i, n, t), void 0, null), n;
}, useId: function() {
  var e = _t(), t = Fe.identifierPrefix;
  if (oe) {
    var n = bt, r = Ot;
    n = (r & ~(1 << 32 - Ct(r) - 1)).toString(32) + n, t = ":" + t + "R" + n, n = Qr++, 0 < n && (t += "H" + n.toString(32)), t += ":";
  } else n = Mp++, t = ":" + t + "r" + n.toString(32) + ":";
  return e.memoizedState = t;
}, unstable_isNewReconciler: !1 }, Bp = {
  readContext: xt,
  useCallback: Qu,
  useContext: xt,
  useEffect: Ro,
  useImperativeHandle: Wu,
  useInsertionEffect: Vu,
  useLayoutEffect: Bu,
  useMemo: Gu,
  useReducer: Gl,
  useRef: Uu,
  useState: function() {
    return Gl(Gr);
  },
  useDebugValue: _o,
  useDeferredValue: function(e) {
    var t = gt();
    return Ku(t, we.memoizedState, e);
  },
  useTransition: function() {
    var e = Gl(Gr)[0], t = gt().memoizedState;
    return [e, t];
  },
  useMutableSource: zu,
  useSyncExternalStore: Lu,
  useId: qu,
  unstable_isNewReconciler: !1
}, Hp = { readContext: xt, useCallback: Qu, useContext: xt, useEffect: Ro, useImperativeHandle: Wu, useInsertionEffect: Vu, useLayoutEffect: Bu, useMemo: Gu, useReducer: Kl, useRef: Uu, useState: function() {
  return Kl(Gr);
}, useDebugValue: _o, useDeferredValue: function(e) {
  var t = gt();
  return we === null ? t.memoizedState = e : Ku(t, we.memoizedState, e);
}, useTransition: function() {
  var e = Kl(Gr)[0], t = gt().memoizedState;
  return [e, t];
}, useMutableSource: zu, useSyncExternalStore: Lu, useId: qu, unstable_isNewReconciler: !1 };
function Nt(e, t) {
  if (e && e.defaultProps) {
    t = pe({}, t), e = e.defaultProps;
    for (var n in e) t[n] === void 0 && (t[n] = e[n]);
    return t;
  }
  return t;
}
function Ti(e, t, n, r) {
  t = e.memoizedState, n = n(r, t), n = n == null ? t : pe({}, t, n), e.memoizedState = n, e.lanes === 0 && (e.updateQueue.baseState = n);
}
var dl = { isMounted: function(e) {
  return (e = e._reactInternals) ? _n(e) === e : !1;
}, enqueueSetState: function(e, t, n) {
  e = e._reactInternals;
  var r = Qe(), a = cn(e), i = Ut(r, a);
  i.payload = t, n != null && (i.callback = n), t = on(e, i, a), t !== null && (kt(t, e, a, r), Ea(t, e, a));
}, enqueueReplaceState: function(e, t, n) {
  e = e._reactInternals;
  var r = Qe(), a = cn(e), i = Ut(r, a);
  i.tag = 1, i.payload = t, n != null && (i.callback = n), t = on(e, i, a), t !== null && (kt(t, e, a, r), Ea(t, e, a));
}, enqueueForceUpdate: function(e, t) {
  e = e._reactInternals;
  var n = Qe(), r = cn(e), a = Ut(n, r);
  a.tag = 2, t != null && (a.callback = t), t = on(e, a, r), t !== null && (kt(t, e, r, n), Ea(t, e, r));
} };
function Qs(e, t, n, r, a, i, o) {
  return e = e.stateNode, typeof e.shouldComponentUpdate == "function" ? e.shouldComponentUpdate(r, i, o) : t.prototype && t.prototype.isPureReactComponent ? !br(n, r) || !br(a, i) : !0;
}
function Ju(e, t, n) {
  var r = !1, a = fn, i = t.contextType;
  return typeof i == "object" && i !== null ? i = xt(i) : (a = Je(t) ? kn : be.current, r = t.contextTypes, i = (r = r != null) ? er(e, a) : fn), t = new t(n, i), e.memoizedState = t.state !== null && t.state !== void 0 ? t.state : null, t.updater = dl, e.stateNode = t, t._reactInternals = e, r && (e = e.stateNode, e.__reactInternalMemoizedUnmaskedChildContext = a, e.__reactInternalMemoizedMaskedChildContext = i), t;
}
function Gs(e, t, n, r) {
  e = t.state, typeof t.componentWillReceiveProps == "function" && t.componentWillReceiveProps(n, r), typeof t.UNSAFE_componentWillReceiveProps == "function" && t.UNSAFE_componentWillReceiveProps(n, r), t.state !== e && dl.enqueueReplaceState(t, t.state, null);
}
function Di(e, t, n, r) {
  var a = e.stateNode;
  a.props = n, a.state = e.memoizedState, a.refs = {}, wo(e);
  var i = t.contextType;
  typeof i == "object" && i !== null ? a.context = xt(i) : (i = Je(t) ? kn : be.current, a.context = er(e, i)), a.state = e.memoizedState, i = t.getDerivedStateFromProps, typeof i == "function" && (Ti(e, t, i, n), a.state = e.memoizedState), typeof t.getDerivedStateFromProps == "function" || typeof a.getSnapshotBeforeUpdate == "function" || typeof a.UNSAFE_componentWillMount != "function" && typeof a.componentWillMount != "function" || (t = a.state, typeof a.componentWillMount == "function" && a.componentWillMount(), typeof a.UNSAFE_componentWillMount == "function" && a.UNSAFE_componentWillMount(), t !== a.state && dl.enqueueReplaceState(a, a.state, null), Ka(e, n, a, r), a.state = e.memoizedState), typeof a.componentDidMount == "function" && (e.flags |= 4194308);
}
function ar(e, t) {
  try {
    var n = "", r = t;
    do
      n += gf(r), r = r.return;
    while (r);
    var a = n;
  } catch (i) {
    a = `
Error generating stack: ` + i.message + `
` + i.stack;
  }
  return { value: e, source: t, stack: a, digest: null };
}
function ql(e, t, n) {
  return { value: e, source: null, stack: n ?? null, digest: t ?? null };
}
function zi(e, t) {
  try {
    console.error(t.value);
  } catch (n) {
    setTimeout(function() {
      throw n;
    });
  }
}
var Wp = typeof WeakMap == "function" ? WeakMap : Map;
function ed(e, t, n) {
  n = Ut(-1, n), n.tag = 3, n.payload = { element: null };
  var r = t.value;
  return n.callback = function() {
    Ja || (Ja = !0, Hi = r), zi(e, t);
  }, n;
}
function td(e, t, n) {
  n = Ut(-1, n), n.tag = 3;
  var r = e.type.getDerivedStateFromError;
  if (typeof r == "function") {
    var a = t.value;
    n.payload = function() {
      return r(a);
    }, n.callback = function() {
      zi(e, t);
    };
  }
  var i = e.stateNode;
  return i !== null && typeof i.componentDidCatch == "function" && (n.callback = function() {
    zi(e, t), typeof r != "function" && (sn === null ? sn = /* @__PURE__ */ new Set([this]) : sn.add(this));
    var o = t.stack;
    this.componentDidCatch(t.value, { componentStack: o !== null ? o : "" });
  }), n;
}
function Ks(e, t, n) {
  var r = e.pingCache;
  if (r === null) {
    r = e.pingCache = new Wp();
    var a = /* @__PURE__ */ new Set();
    r.set(t, a);
  } else a = r.get(t), a === void 0 && (a = /* @__PURE__ */ new Set(), r.set(t, a));
  a.has(n) || (a.add(n), e = lm.bind(null, e, t, n), t.then(e, e));
}
function qs(e) {
  do {
    var t;
    if ((t = e.tag === 13) && (t = e.memoizedState, t = t !== null ? t.dehydrated !== null : !0), t) return e;
    e = e.return;
  } while (e !== null);
  return null;
}
function Ys(e, t, n, r, a) {
  return e.mode & 1 ? (e.flags |= 65536, e.lanes = a, e) : (e === t ? e.flags |= 65536 : (e.flags |= 128, n.flags |= 131072, n.flags &= -52805, n.tag === 1 && (n.alternate === null ? n.tag = 17 : (t = Ut(-1, 1), t.tag = 2, on(n, t, 1))), n.lanes |= 1), e);
}
var Qp = Qt.ReactCurrentOwner, Xe = !1;
function He(e, t, n, r) {
  t.child = e === null ? Ru(t, null, n, r) : nr(t, e.child, n, r);
}
function Xs(e, t, n, r, a) {
  n = n.render;
  var i = t.ref;
  return Yn(t, a), r = Po(e, t, n, r, i, a), n = Fo(), e !== null && !Xe ? (t.updateQueue = e.updateQueue, t.flags &= -2053, e.lanes &= ~a, Wt(e, t, a)) : (oe && n && vo(t), t.flags |= 1, He(e, t, r, a), t.child);
}
function Zs(e, t, n, r, a) {
  if (e === null) {
    var i = n.type;
    return typeof i == "function" && !Oo(i) && i.defaultProps === void 0 && n.compare === null && n.defaultProps === void 0 ? (t.tag = 15, t.type = i, nd(e, t, i, r, a)) : (e = Ta(n.type, null, r, t, t.mode, a), e.ref = t.ref, e.return = t, t.child = e);
  }
  if (i = e.child, !(e.lanes & a)) {
    var o = i.memoizedProps;
    if (n = n.compare, n = n !== null ? n : br, n(o, r) && e.ref === t.ref) return Wt(e, t, a);
  }
  return t.flags |= 1, e = un(i, r), e.ref = t.ref, e.return = t, t.child = e;
}
function nd(e, t, n, r, a) {
  if (e !== null) {
    var i = e.memoizedProps;
    if (br(i, r) && e.ref === t.ref) if (Xe = !1, t.pendingProps = r = i, (e.lanes & a) !== 0) e.flags & 131072 && (Xe = !0);
    else return t.lanes = e.lanes, Wt(e, t, a);
  }
  return Li(e, t, n, r, a);
}
function rd(e, t, n) {
  var r = t.pendingProps, a = r.children, i = e !== null ? e.memoizedState : null;
  if (r.mode === "hidden") if (!(t.mode & 1)) t.memoizedState = { baseLanes: 0, cachePool: null, transitions: null }, ae(Wn, nt), nt |= n;
  else {
    if (!(n & 1073741824)) return e = i !== null ? i.baseLanes | n : n, t.lanes = t.childLanes = 1073741824, t.memoizedState = { baseLanes: e, cachePool: null, transitions: null }, t.updateQueue = null, ae(Wn, nt), nt |= e, null;
    t.memoizedState = { baseLanes: 0, cachePool: null, transitions: null }, r = i !== null ? i.baseLanes : n, ae(Wn, nt), nt |= r;
  }
  else i !== null ? (r = i.baseLanes | n, t.memoizedState = null) : r = n, ae(Wn, nt), nt |= r;
  return He(e, t, a, n), t.child;
}
function ad(e, t) {
  var n = t.ref;
  (e === null && n !== null || e !== null && e.ref !== n) && (t.flags |= 512, t.flags |= 2097152);
}
function Li(e, t, n, r, a) {
  var i = Je(n) ? kn : be.current;
  return i = er(t, i), Yn(t, a), n = Po(e, t, n, r, i, a), r = Fo(), e !== null && !Xe ? (t.updateQueue = e.updateQueue, t.flags &= -2053, e.lanes &= ~a, Wt(e, t, a)) : (oe && r && vo(t), t.flags |= 1, He(e, t, n, a), t.child);
}
function Js(e, t, n, r, a) {
  if (Je(n)) {
    var i = !0;
    Ba(t);
  } else i = !1;
  if (Yn(t, a), t.stateNode === null) Fa(e, t), Ju(t, n, r), Di(t, n, r, a), r = !0;
  else if (e === null) {
    var o = t.stateNode, s = t.memoizedProps;
    o.props = s;
    var c = o.context, f = n.contextType;
    typeof f == "object" && f !== null ? f = xt(f) : (f = Je(n) ? kn : be.current, f = er(t, f));
    var j = n.getDerivedStateFromProps, d = typeof j == "function" || typeof o.getSnapshotBeforeUpdate == "function";
    d || typeof o.UNSAFE_componentWillReceiveProps != "function" && typeof o.componentWillReceiveProps != "function" || (s !== r || c !== f) && Gs(t, o, r, f), Xt = !1;
    var m = t.memoizedState;
    o.state = m, Ka(t, r, o, a), c = t.memoizedState, s !== r || m !== c || Ze.current || Xt ? (typeof j == "function" && (Ti(t, n, j, r), c = t.memoizedState), (s = Xt || Qs(t, n, s, r, m, c, f)) ? (d || typeof o.UNSAFE_componentWillMount != "function" && typeof o.componentWillMount != "function" || (typeof o.componentWillMount == "function" && o.componentWillMount(), typeof o.UNSAFE_componentWillMount == "function" && o.UNSAFE_componentWillMount()), typeof o.componentDidMount == "function" && (t.flags |= 4194308)) : (typeof o.componentDidMount == "function" && (t.flags |= 4194308), t.memoizedProps = r, t.memoizedState = c), o.props = r, o.state = c, o.context = f, r = s) : (typeof o.componentDidMount == "function" && (t.flags |= 4194308), r = !1);
  } else {
    o = t.stateNode, Tu(e, t), s = t.memoizedProps, f = t.type === t.elementType ? s : Nt(t.type, s), o.props = f, d = t.pendingProps, m = o.context, c = n.contextType, typeof c == "object" && c !== null ? c = xt(c) : (c = Je(n) ? kn : be.current, c = er(t, c));
    var h = n.getDerivedStateFromProps;
    (j = typeof h == "function" || typeof o.getSnapshotBeforeUpdate == "function") || typeof o.UNSAFE_componentWillReceiveProps != "function" && typeof o.componentWillReceiveProps != "function" || (s !== d || m !== c) && Gs(t, o, r, c), Xt = !1, m = t.memoizedState, o.state = m, Ka(t, r, o, a);
    var N = t.memoizedState;
    s !== d || m !== N || Ze.current || Xt ? (typeof h == "function" && (Ti(t, n, h, r), N = t.memoizedState), (f = Xt || Qs(t, n, f, r, m, N, c) || !1) ? (j || typeof o.UNSAFE_componentWillUpdate != "function" && typeof o.componentWillUpdate != "function" || (typeof o.componentWillUpdate == "function" && o.componentWillUpdate(r, N, c), typeof o.UNSAFE_componentWillUpdate == "function" && o.UNSAFE_componentWillUpdate(r, N, c)), typeof o.componentDidUpdate == "function" && (t.flags |= 4), typeof o.getSnapshotBeforeUpdate == "function" && (t.flags |= 1024)) : (typeof o.componentDidUpdate != "function" || s === e.memoizedProps && m === e.memoizedState || (t.flags |= 4), typeof o.getSnapshotBeforeUpdate != "function" || s === e.memoizedProps && m === e.memoizedState || (t.flags |= 1024), t.memoizedProps = r, t.memoizedState = N), o.props = r, o.state = N, o.context = c, r = f) : (typeof o.componentDidUpdate != "function" || s === e.memoizedProps && m === e.memoizedState || (t.flags |= 4), typeof o.getSnapshotBeforeUpdate != "function" || s === e.memoizedProps && m === e.memoizedState || (t.flags |= 1024), r = !1);
  }
  return Ai(e, t, n, r, i, a);
}
function Ai(e, t, n, r, a, i) {
  ad(e, t);
  var o = (t.flags & 128) !== 0;
  if (!r && !o) return a && Ms(t, n, !1), Wt(e, t, i);
  r = t.stateNode, Qp.current = t;
  var s = o && typeof n.getDerivedStateFromError != "function" ? null : r.render();
  return t.flags |= 1, e !== null && o ? (t.child = nr(t, e.child, null, i), t.child = nr(t, null, s, i)) : He(e, t, s, i), t.memoizedState = r.state, a && Ms(t, n, !0), t.child;
}
function ld(e) {
  var t = e.stateNode;
  t.pendingContext ? $s(e, t.pendingContext, t.pendingContext !== t.context) : t.context && $s(e, t.context, !1), Co(e, t.containerInfo);
}
function ec(e, t, n, r, a) {
  return tr(), go(a), t.flags |= 256, He(e, t, n, r), t.child;
}
var $i = { dehydrated: null, treeContext: null, retryLane: 0 };
function Mi(e) {
  return { baseLanes: e, cachePool: null, transitions: null };
}
function id(e, t, n) {
  var r = t.pendingProps, a = ue.current, i = !1, o = (t.flags & 128) !== 0, s;
  if ((s = o) || (s = e !== null && e.memoizedState === null ? !1 : (a & 2) !== 0), s ? (i = !0, t.flags &= -129) : (e === null || e.memoizedState !== null) && (a |= 1), ae(ue, a & 1), e === null)
    return Ri(t), e = t.memoizedState, e !== null && (e = e.dehydrated, e !== null) ? (t.mode & 1 ? e.data === "$!" ? t.lanes = 8 : t.lanes = 1073741824 : t.lanes = 1, null) : (o = r.children, e = r.fallback, i ? (r = t.mode, i = t.child, o = { mode: "hidden", children: o }, !(r & 1) && i !== null ? (i.childLanes = 0, i.pendingProps = o) : i = ml(o, r, 0, null), e = wn(e, r, n, null), i.return = t, e.return = t, i.sibling = e, t.child = i, t.child.memoizedState = Mi(n), t.memoizedState = $i, e) : To(t, o));
  if (a = e.memoizedState, a !== null && (s = a.dehydrated, s !== null)) return Gp(e, t, o, r, s, a, n);
  if (i) {
    i = r.fallback, o = t.mode, a = e.child, s = a.sibling;
    var c = { mode: "hidden", children: r.children };
    return !(o & 1) && t.child !== a ? (r = t.child, r.childLanes = 0, r.pendingProps = c, t.deletions = null) : (r = un(a, c), r.subtreeFlags = a.subtreeFlags & 14680064), s !== null ? i = un(s, i) : (i = wn(i, o, n, null), i.flags |= 2), i.return = t, r.return = t, r.sibling = i, t.child = r, r = i, i = t.child, o = e.child.memoizedState, o = o === null ? Mi(n) : { baseLanes: o.baseLanes | n, cachePool: null, transitions: o.transitions }, i.memoizedState = o, i.childLanes = e.childLanes & ~n, t.memoizedState = $i, r;
  }
  return i = e.child, e = i.sibling, r = un(i, { mode: "visible", children: r.children }), !(t.mode & 1) && (r.lanes = n), r.return = t, r.sibling = null, e !== null && (n = t.deletions, n === null ? (t.deletions = [e], t.flags |= 16) : n.push(e)), t.child = r, t.memoizedState = null, r;
}
function To(e, t) {
  return t = ml({ mode: "visible", children: t }, e.mode, 0, null), t.return = e, e.child = t;
}
function xa(e, t, n, r) {
  return r !== null && go(r), nr(t, e.child, null, n), e = To(t, t.pendingProps.children), e.flags |= 2, t.memoizedState = null, e;
}
function Gp(e, t, n, r, a, i, o) {
  if (n)
    return t.flags & 256 ? (t.flags &= -257, r = ql(Error(F(422))), xa(e, t, o, r)) : t.memoizedState !== null ? (t.child = e.child, t.flags |= 128, null) : (i = r.fallback, a = t.mode, r = ml({ mode: "visible", children: r.children }, a, 0, null), i = wn(i, a, o, null), i.flags |= 2, r.return = t, i.return = t, r.sibling = i, t.child = r, t.mode & 1 && nr(t, e.child, null, o), t.child.memoizedState = Mi(o), t.memoizedState = $i, i);
  if (!(t.mode & 1)) return xa(e, t, o, null);
  if (a.data === "$!") {
    if (r = a.nextSibling && a.nextSibling.dataset, r) var s = r.dgst;
    return r = s, i = Error(F(419)), r = ql(i, r, void 0), xa(e, t, o, r);
  }
  if (s = (o & e.childLanes) !== 0, Xe || s) {
    if (r = Fe, r !== null) {
      switch (o & -o) {
        case 4:
          a = 2;
          break;
        case 16:
          a = 8;
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
          a = 32;
          break;
        case 536870912:
          a = 268435456;
          break;
        default:
          a = 0;
      }
      a = a & (r.suspendedLanes | o) ? 0 : a, a !== 0 && a !== i.retryLane && (i.retryLane = a, Ht(e, a), kt(r, e, a, -1));
    }
    return Mo(), r = ql(Error(F(421))), xa(e, t, o, r);
  }
  return a.data === "$?" ? (t.flags |= 128, t.child = e.child, t = im.bind(null, e), a._reactRetry = t, null) : (e = i.treeContext, rt = ln(a.nextSibling), at = t, oe = !0, wt = null, e !== null && (dt[ft++] = Ot, dt[ft++] = bt, dt[ft++] = En, Ot = e.id, bt = e.overflow, En = t), t = To(t, r.children), t.flags |= 4096, t);
}
function tc(e, t, n) {
  e.lanes |= t;
  var r = e.alternate;
  r !== null && (r.lanes |= t), _i(e.return, t, n);
}
function Yl(e, t, n, r, a) {
  var i = e.memoizedState;
  i === null ? e.memoizedState = { isBackwards: t, rendering: null, renderingStartTime: 0, last: r, tail: n, tailMode: a } : (i.isBackwards = t, i.rendering = null, i.renderingStartTime = 0, i.last = r, i.tail = n, i.tailMode = a);
}
function od(e, t, n) {
  var r = t.pendingProps, a = r.revealOrder, i = r.tail;
  if (He(e, t, r.children, n), r = ue.current, r & 2) r = r & 1 | 2, t.flags |= 128;
  else {
    if (e !== null && e.flags & 128) e: for (e = t.child; e !== null; ) {
      if (e.tag === 13) e.memoizedState !== null && tc(e, n, t);
      else if (e.tag === 19) tc(e, n, t);
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
  if (ae(ue, r), !(t.mode & 1)) t.memoizedState = null;
  else switch (a) {
    case "forwards":
      for (n = t.child, a = null; n !== null; ) e = n.alternate, e !== null && qa(e) === null && (a = n), n = n.sibling;
      n = a, n === null ? (a = t.child, t.child = null) : (a = n.sibling, n.sibling = null), Yl(t, !1, a, n, i);
      break;
    case "backwards":
      for (n = null, a = t.child, t.child = null; a !== null; ) {
        if (e = a.alternate, e !== null && qa(e) === null) {
          t.child = a;
          break;
        }
        e = a.sibling, a.sibling = n, n = a, a = e;
      }
      Yl(t, !0, n, null, i);
      break;
    case "together":
      Yl(t, !1, null, null, void 0);
      break;
    default:
      t.memoizedState = null;
  }
  return t.child;
}
function Fa(e, t) {
  !(t.mode & 1) && e !== null && (e.alternate = null, t.alternate = null, t.flags |= 2);
}
function Wt(e, t, n) {
  if (e !== null && (t.dependencies = e.dependencies), Pn |= t.lanes, !(n & t.childLanes)) return null;
  if (e !== null && t.child !== e.child) throw Error(F(153));
  if (t.child !== null) {
    for (e = t.child, n = un(e, e.pendingProps), t.child = n, n.return = t; e.sibling !== null; ) e = e.sibling, n = n.sibling = un(e, e.pendingProps), n.return = t;
    n.sibling = null;
  }
  return t.child;
}
function Kp(e, t, n) {
  switch (t.tag) {
    case 3:
      ld(t), tr();
      break;
    case 5:
      Du(t);
      break;
    case 1:
      Je(t.type) && Ba(t);
      break;
    case 4:
      Co(t, t.stateNode.containerInfo);
      break;
    case 10:
      var r = t.type._context, a = t.memoizedProps.value;
      ae(Qa, r._currentValue), r._currentValue = a;
      break;
    case 13:
      if (r = t.memoizedState, r !== null)
        return r.dehydrated !== null ? (ae(ue, ue.current & 1), t.flags |= 128, null) : n & t.child.childLanes ? id(e, t, n) : (ae(ue, ue.current & 1), e = Wt(e, t, n), e !== null ? e.sibling : null);
      ae(ue, ue.current & 1);
      break;
    case 19:
      if (r = (n & t.childLanes) !== 0, e.flags & 128) {
        if (r) return od(e, t, n);
        t.flags |= 128;
      }
      if (a = t.memoizedState, a !== null && (a.rendering = null, a.tail = null, a.lastEffect = null), ae(ue, ue.current), r) break;
      return null;
    case 22:
    case 23:
      return t.lanes = 0, rd(e, t, n);
  }
  return Wt(e, t, n);
}
var sd, Oi, cd, ud;
sd = function(e, t) {
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
Oi = function() {
};
cd = function(e, t, n, r) {
  var a = e.memoizedProps;
  if (a !== r) {
    e = t.stateNode, Nn(zt.current);
    var i = null;
    switch (n) {
      case "input":
        a = oi(e, a), r = oi(e, r), i = [];
        break;
      case "select":
        a = pe({}, a, { value: void 0 }), r = pe({}, r, { value: void 0 }), i = [];
        break;
      case "textarea":
        a = ui(e, a), r = ui(e, r), i = [];
        break;
      default:
        typeof a.onClick != "function" && typeof r.onClick == "function" && (e.onclick = Ua);
    }
    fi(n, r);
    var o;
    n = null;
    for (f in a) if (!r.hasOwnProperty(f) && a.hasOwnProperty(f) && a[f] != null) if (f === "style") {
      var s = a[f];
      for (o in s) s.hasOwnProperty(o) && (n || (n = {}), n[o] = "");
    } else f !== "dangerouslySetInnerHTML" && f !== "children" && f !== "suppressContentEditableWarning" && f !== "suppressHydrationWarning" && f !== "autoFocus" && (Dr.hasOwnProperty(f) ? i || (i = []) : (i = i || []).push(f, null));
    for (f in r) {
      var c = r[f];
      if (s = a != null ? a[f] : void 0, r.hasOwnProperty(f) && c !== s && (c != null || s != null)) if (f === "style") if (s) {
        for (o in s) !s.hasOwnProperty(o) || c && c.hasOwnProperty(o) || (n || (n = {}), n[o] = "");
        for (o in c) c.hasOwnProperty(o) && s[o] !== c[o] && (n || (n = {}), n[o] = c[o]);
      } else n || (i || (i = []), i.push(
        f,
        n
      )), n = c;
      else f === "dangerouslySetInnerHTML" ? (c = c ? c.__html : void 0, s = s ? s.__html : void 0, c != null && s !== c && (i = i || []).push(f, c)) : f === "children" ? typeof c != "string" && typeof c != "number" || (i = i || []).push(f, "" + c) : f !== "suppressContentEditableWarning" && f !== "suppressHydrationWarning" && (Dr.hasOwnProperty(f) ? (c != null && f === "onScroll" && le("scroll", e), i || s === c || (i = [])) : (i = i || []).push(f, c));
    }
    n && (i = i || []).push("style", n);
    var f = i;
    (t.updateQueue = f) && (t.flags |= 4);
  }
};
ud = function(e, t, n, r) {
  n !== r && (t.flags |= 4);
};
function xr(e, t) {
  if (!oe) switch (e.tailMode) {
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
function Ae(e) {
  var t = e.alternate !== null && e.alternate.child === e.child, n = 0, r = 0;
  if (t) for (var a = e.child; a !== null; ) n |= a.lanes | a.childLanes, r |= a.subtreeFlags & 14680064, r |= a.flags & 14680064, a.return = e, a = a.sibling;
  else for (a = e.child; a !== null; ) n |= a.lanes | a.childLanes, r |= a.subtreeFlags, r |= a.flags, a.return = e, a = a.sibling;
  return e.subtreeFlags |= r, e.childLanes = n, t;
}
function qp(e, t, n) {
  var r = t.pendingProps;
  switch (xo(t), t.tag) {
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
      return Ae(t), null;
    case 1:
      return Je(t.type) && Va(), Ae(t), null;
    case 3:
      return r = t.stateNode, rr(), ie(Ze), ie(be), Eo(), r.pendingContext && (r.context = r.pendingContext, r.pendingContext = null), (e === null || e.child === null) && (ha(t) ? t.flags |= 4 : e === null || e.memoizedState.isDehydrated && !(t.flags & 256) || (t.flags |= 1024, wt !== null && (Gi(wt), wt = null))), Oi(e, t), Ae(t), null;
    case 5:
      ko(t);
      var a = Nn(Wr.current);
      if (n = t.type, e !== null && t.stateNode != null) cd(e, t, n, r, a), e.ref !== t.ref && (t.flags |= 512, t.flags |= 2097152);
      else {
        if (!r) {
          if (t.stateNode === null) throw Error(F(166));
          return Ae(t), null;
        }
        if (e = Nn(zt.current), ha(t)) {
          r = t.stateNode, n = t.type;
          var i = t.memoizedProps;
          switch (r[Tt] = t, r[Br] = i, e = (t.mode & 1) !== 0, n) {
            case "dialog":
              le("cancel", r), le("close", r);
              break;
            case "iframe":
            case "object":
            case "embed":
              le("load", r);
              break;
            case "video":
            case "audio":
              for (a = 0; a < Sr.length; a++) le(Sr[a], r);
              break;
            case "source":
              le("error", r);
              break;
            case "img":
            case "image":
            case "link":
              le(
                "error",
                r
              ), le("load", r);
              break;
            case "details":
              le("toggle", r);
              break;
            case "input":
              us(r, i), le("invalid", r);
              break;
            case "select":
              r._wrapperState = { wasMultiple: !!i.multiple }, le("invalid", r);
              break;
            case "textarea":
              fs(r, i), le("invalid", r);
          }
          fi(n, i), a = null;
          for (var o in i) if (i.hasOwnProperty(o)) {
            var s = i[o];
            o === "children" ? typeof s == "string" ? r.textContent !== s && (i.suppressHydrationWarning !== !0 && ma(r.textContent, s, e), a = ["children", s]) : typeof s == "number" && r.textContent !== "" + s && (i.suppressHydrationWarning !== !0 && ma(
              r.textContent,
              s,
              e
            ), a = ["children", "" + s]) : Dr.hasOwnProperty(o) && s != null && o === "onScroll" && le("scroll", r);
          }
          switch (n) {
            case "input":
              ia(r), ds(r, i, !0);
              break;
            case "textarea":
              ia(r), ps(r);
              break;
            case "select":
            case "option":
              break;
            default:
              typeof i.onClick == "function" && (r.onclick = Ua);
          }
          r = a, t.updateQueue = r, r !== null && (t.flags |= 4);
        } else {
          o = a.nodeType === 9 ? a : a.ownerDocument, e === "http://www.w3.org/1999/xhtml" && (e = Mc(n)), e === "http://www.w3.org/1999/xhtml" ? n === "script" ? (e = o.createElement("div"), e.innerHTML = "<script><\/script>", e = e.removeChild(e.firstChild)) : typeof r.is == "string" ? e = o.createElement(n, { is: r.is }) : (e = o.createElement(n), n === "select" && (o = e, r.multiple ? o.multiple = !0 : r.size && (o.size = r.size))) : e = o.createElementNS(e, n), e[Tt] = t, e[Br] = r, sd(e, t, !1, !1), t.stateNode = e;
          e: {
            switch (o = pi(n, r), n) {
              case "dialog":
                le("cancel", e), le("close", e), a = r;
                break;
              case "iframe":
              case "object":
              case "embed":
                le("load", e), a = r;
                break;
              case "video":
              case "audio":
                for (a = 0; a < Sr.length; a++) le(Sr[a], e);
                a = r;
                break;
              case "source":
                le("error", e), a = r;
                break;
              case "img":
              case "image":
              case "link":
                le(
                  "error",
                  e
                ), le("load", e), a = r;
                break;
              case "details":
                le("toggle", e), a = r;
                break;
              case "input":
                us(e, r), a = oi(e, r), le("invalid", e);
                break;
              case "option":
                a = r;
                break;
              case "select":
                e._wrapperState = { wasMultiple: !!r.multiple }, a = pe({}, r, { value: void 0 }), le("invalid", e);
                break;
              case "textarea":
                fs(e, r), a = ui(e, r), le("invalid", e);
                break;
              default:
                a = r;
            }
            fi(n, a), s = a;
            for (i in s) if (s.hasOwnProperty(i)) {
              var c = s[i];
              i === "style" ? Uc(e, c) : i === "dangerouslySetInnerHTML" ? (c = c ? c.__html : void 0, c != null && Oc(e, c)) : i === "children" ? typeof c == "string" ? (n !== "textarea" || c !== "") && zr(e, c) : typeof c == "number" && zr(e, "" + c) : i !== "suppressContentEditableWarning" && i !== "suppressHydrationWarning" && i !== "autoFocus" && (Dr.hasOwnProperty(i) ? c != null && i === "onScroll" && le("scroll", e) : c != null && to(e, i, c, o));
            }
            switch (n) {
              case "input":
                ia(e), ds(e, r, !1);
                break;
              case "textarea":
                ia(e), ps(e);
                break;
              case "option":
                r.value != null && e.setAttribute("value", "" + dn(r.value));
                break;
              case "select":
                e.multiple = !!r.multiple, i = r.value, i != null ? Qn(e, !!r.multiple, i, !1) : r.defaultValue != null && Qn(
                  e,
                  !!r.multiple,
                  r.defaultValue,
                  !0
                );
                break;
              default:
                typeof a.onClick == "function" && (e.onclick = Ua);
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
      return Ae(t), null;
    case 6:
      if (e && t.stateNode != null) ud(e, t, e.memoizedProps, r);
      else {
        if (typeof r != "string" && t.stateNode === null) throw Error(F(166));
        if (n = Nn(Wr.current), Nn(zt.current), ha(t)) {
          if (r = t.stateNode, n = t.memoizedProps, r[Tt] = t, (i = r.nodeValue !== n) && (e = at, e !== null)) switch (e.tag) {
            case 3:
              ma(r.nodeValue, n, (e.mode & 1) !== 0);
              break;
            case 5:
              e.memoizedProps.suppressHydrationWarning !== !0 && ma(r.nodeValue, n, (e.mode & 1) !== 0);
          }
          i && (t.flags |= 4);
        } else r = (n.nodeType === 9 ? n : n.ownerDocument).createTextNode(r), r[Tt] = t, t.stateNode = r;
      }
      return Ae(t), null;
    case 13:
      if (ie(ue), r = t.memoizedState, e === null || e.memoizedState !== null && e.memoizedState.dehydrated !== null) {
        if (oe && rt !== null && t.mode & 1 && !(t.flags & 128)) Pu(), tr(), t.flags |= 98560, i = !1;
        else if (i = ha(t), r !== null && r.dehydrated !== null) {
          if (e === null) {
            if (!i) throw Error(F(318));
            if (i = t.memoizedState, i = i !== null ? i.dehydrated : null, !i) throw Error(F(317));
            i[Tt] = t;
          } else tr(), !(t.flags & 128) && (t.memoizedState = null), t.flags |= 4;
          Ae(t), i = !1;
        } else wt !== null && (Gi(wt), wt = null), i = !0;
        if (!i) return t.flags & 65536 ? t : null;
      }
      return t.flags & 128 ? (t.lanes = n, t) : (r = r !== null, r !== (e !== null && e.memoizedState !== null) && r && (t.child.flags |= 8192, t.mode & 1 && (e === null || ue.current & 1 ? ke === 0 && (ke = 3) : Mo())), t.updateQueue !== null && (t.flags |= 4), Ae(t), null);
    case 4:
      return rr(), Oi(e, t), e === null && Ur(t.stateNode.containerInfo), Ae(t), null;
    case 10:
      return No(t.type._context), Ae(t), null;
    case 17:
      return Je(t.type) && Va(), Ae(t), null;
    case 19:
      if (ie(ue), i = t.memoizedState, i === null) return Ae(t), null;
      if (r = (t.flags & 128) !== 0, o = i.rendering, o === null) if (r) xr(i, !1);
      else {
        if (ke !== 0 || e !== null && e.flags & 128) for (e = t.child; e !== null; ) {
          if (o = qa(e), o !== null) {
            for (t.flags |= 128, xr(i, !1), r = o.updateQueue, r !== null && (t.updateQueue = r, t.flags |= 4), t.subtreeFlags = 0, r = n, n = t.child; n !== null; ) i = n, e = r, i.flags &= 14680066, o = i.alternate, o === null ? (i.childLanes = 0, i.lanes = e, i.child = null, i.subtreeFlags = 0, i.memoizedProps = null, i.memoizedState = null, i.updateQueue = null, i.dependencies = null, i.stateNode = null) : (i.childLanes = o.childLanes, i.lanes = o.lanes, i.child = o.child, i.subtreeFlags = 0, i.deletions = null, i.memoizedProps = o.memoizedProps, i.memoizedState = o.memoizedState, i.updateQueue = o.updateQueue, i.type = o.type, e = o.dependencies, i.dependencies = e === null ? null : { lanes: e.lanes, firstContext: e.firstContext }), n = n.sibling;
            return ae(ue, ue.current & 1 | 2), t.child;
          }
          e = e.sibling;
        }
        i.tail !== null && he() > lr && (t.flags |= 128, r = !0, xr(i, !1), t.lanes = 4194304);
      }
      else {
        if (!r) if (e = qa(o), e !== null) {
          if (t.flags |= 128, r = !0, n = e.updateQueue, n !== null && (t.updateQueue = n, t.flags |= 4), xr(i, !0), i.tail === null && i.tailMode === "hidden" && !o.alternate && !oe) return Ae(t), null;
        } else 2 * he() - i.renderingStartTime > lr && n !== 1073741824 && (t.flags |= 128, r = !0, xr(i, !1), t.lanes = 4194304);
        i.isBackwards ? (o.sibling = t.child, t.child = o) : (n = i.last, n !== null ? n.sibling = o : t.child = o, i.last = o);
      }
      return i.tail !== null ? (t = i.tail, i.rendering = t, i.tail = t.sibling, i.renderingStartTime = he(), t.sibling = null, n = ue.current, ae(ue, r ? n & 1 | 2 : n & 1), t) : (Ae(t), null);
    case 22:
    case 23:
      return $o(), r = t.memoizedState !== null, e !== null && e.memoizedState !== null !== r && (t.flags |= 8192), r && t.mode & 1 ? nt & 1073741824 && (Ae(t), t.subtreeFlags & 6 && (t.flags |= 8192)) : Ae(t), null;
    case 24:
      return null;
    case 25:
      return null;
  }
  throw Error(F(156, t.tag));
}
function Yp(e, t) {
  switch (xo(t), t.tag) {
    case 1:
      return Je(t.type) && Va(), e = t.flags, e & 65536 ? (t.flags = e & -65537 | 128, t) : null;
    case 3:
      return rr(), ie(Ze), ie(be), Eo(), e = t.flags, e & 65536 && !(e & 128) ? (t.flags = e & -65537 | 128, t) : null;
    case 5:
      return ko(t), null;
    case 13:
      if (ie(ue), e = t.memoizedState, e !== null && e.dehydrated !== null) {
        if (t.alternate === null) throw Error(F(340));
        tr();
      }
      return e = t.flags, e & 65536 ? (t.flags = e & -65537 | 128, t) : null;
    case 19:
      return ie(ue), null;
    case 4:
      return rr(), null;
    case 10:
      return No(t.type._context), null;
    case 22:
    case 23:
      return $o(), null;
    case 24:
      return null;
    default:
      return null;
  }
}
var ga = !1, $e = !1, Xp = typeof WeakSet == "function" ? WeakSet : Set, O = null;
function Hn(e, t) {
  var n = e.ref;
  if (n !== null) if (typeof n == "function") try {
    n(null);
  } catch (r) {
    me(e, t, r);
  }
  else n.current = null;
}
function bi(e, t, n) {
  try {
    n();
  } catch (r) {
    me(e, t, r);
  }
}
var nc = !1;
function Zp(e, t) {
  if (wi = Ma, e = hu(), ho(e)) {
    if ("selectionStart" in e) var n = { start: e.selectionStart, end: e.selectionEnd };
    else e: {
      n = (n = e.ownerDocument) && n.defaultView || window;
      var r = n.getSelection && n.getSelection();
      if (r && r.rangeCount !== 0) {
        n = r.anchorNode;
        var a = r.anchorOffset, i = r.focusNode;
        r = r.focusOffset;
        try {
          n.nodeType, i.nodeType;
        } catch {
          n = null;
          break e;
        }
        var o = 0, s = -1, c = -1, f = 0, j = 0, d = e, m = null;
        t: for (; ; ) {
          for (var h; d !== n || a !== 0 && d.nodeType !== 3 || (s = o + a), d !== i || r !== 0 && d.nodeType !== 3 || (c = o + r), d.nodeType === 3 && (o += d.nodeValue.length), (h = d.firstChild) !== null; )
            m = d, d = h;
          for (; ; ) {
            if (d === e) break t;
            if (m === n && ++f === a && (s = o), m === i && ++j === r && (c = o), (h = d.nextSibling) !== null) break;
            d = m, m = d.parentNode;
          }
          d = h;
        }
        n = s === -1 || c === -1 ? null : { start: s, end: c };
      } else n = null;
    }
    n = n || { start: 0, end: 0 };
  } else n = null;
  for (Ci = { focusedElem: e, selectionRange: n }, Ma = !1, O = t; O !== null; ) if (t = O, e = t.child, (t.subtreeFlags & 1028) !== 0 && e !== null) e.return = t, O = e;
  else for (; O !== null; ) {
    t = O;
    try {
      var N = t.alternate;
      if (t.flags & 1024) switch (t.tag) {
        case 0:
        case 11:
        case 15:
          break;
        case 1:
          if (N !== null) {
            var w = N.memoizedProps, M = N.memoizedState, p = t.stateNode, u = p.getSnapshotBeforeUpdate(t.elementType === t.type ? w : Nt(t.type, w), M);
            p.__reactInternalSnapshotBeforeUpdate = u;
          }
          break;
        case 3:
          var v = t.stateNode.containerInfo;
          v.nodeType === 1 ? v.textContent = "" : v.nodeType === 9 && v.documentElement && v.removeChild(v.documentElement);
          break;
        case 5:
        case 6:
        case 4:
        case 17:
          break;
        default:
          throw Error(F(163));
      }
    } catch (E) {
      me(t, t.return, E);
    }
    if (e = t.sibling, e !== null) {
      e.return = t.return, O = e;
      break;
    }
    O = t.return;
  }
  return N = nc, nc = !1, N;
}
function Fr(e, t, n) {
  var r = t.updateQueue;
  if (r = r !== null ? r.lastEffect : null, r !== null) {
    var a = r = r.next;
    do {
      if ((a.tag & e) === e) {
        var i = a.destroy;
        a.destroy = void 0, i !== void 0 && bi(t, n, i);
      }
      a = a.next;
    } while (a !== r);
  }
}
function fl(e, t) {
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
function Ui(e) {
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
function dd(e) {
  var t = e.alternate;
  t !== null && (e.alternate = null, dd(t)), e.child = null, e.deletions = null, e.sibling = null, e.tag === 5 && (t = e.stateNode, t !== null && (delete t[Tt], delete t[Br], delete t[Ii], delete t[zp], delete t[Lp])), e.stateNode = null, e.return = null, e.dependencies = null, e.memoizedProps = null, e.memoizedState = null, e.pendingProps = null, e.stateNode = null, e.updateQueue = null;
}
function fd(e) {
  return e.tag === 5 || e.tag === 3 || e.tag === 4;
}
function rc(e) {
  e: for (; ; ) {
    for (; e.sibling === null; ) {
      if (e.return === null || fd(e.return)) return null;
      e = e.return;
    }
    for (e.sibling.return = e.return, e = e.sibling; e.tag !== 5 && e.tag !== 6 && e.tag !== 18; ) {
      if (e.flags & 2 || e.child === null || e.tag === 4) continue e;
      e.child.return = e, e = e.child;
    }
    if (!(e.flags & 2)) return e.stateNode;
  }
}
function Vi(e, t, n) {
  var r = e.tag;
  if (r === 5 || r === 6) e = e.stateNode, t ? n.nodeType === 8 ? n.parentNode.insertBefore(e, t) : n.insertBefore(e, t) : (n.nodeType === 8 ? (t = n.parentNode, t.insertBefore(e, n)) : (t = n, t.appendChild(e)), n = n._reactRootContainer, n != null || t.onclick !== null || (t.onclick = Ua));
  else if (r !== 4 && (e = e.child, e !== null)) for (Vi(e, t, n), e = e.sibling; e !== null; ) Vi(e, t, n), e = e.sibling;
}
function Bi(e, t, n) {
  var r = e.tag;
  if (r === 5 || r === 6) e = e.stateNode, t ? n.insertBefore(e, t) : n.appendChild(e);
  else if (r !== 4 && (e = e.child, e !== null)) for (Bi(e, t, n), e = e.sibling; e !== null; ) Bi(e, t, n), e = e.sibling;
}
var _e = null, St = !1;
function qt(e, t, n) {
  for (n = n.child; n !== null; ) pd(e, t, n), n = n.sibling;
}
function pd(e, t, n) {
  if (Dt && typeof Dt.onCommitFiberUnmount == "function") try {
    Dt.onCommitFiberUnmount(al, n);
  } catch {
  }
  switch (n.tag) {
    case 5:
      $e || Hn(n, t);
    case 6:
      var r = _e, a = St;
      _e = null, qt(e, t, n), _e = r, St = a, _e !== null && (St ? (e = _e, n = n.stateNode, e.nodeType === 8 ? e.parentNode.removeChild(n) : e.removeChild(n)) : _e.removeChild(n.stateNode));
      break;
    case 18:
      _e !== null && (St ? (e = _e, n = n.stateNode, e.nodeType === 8 ? Bl(e.parentNode, n) : e.nodeType === 1 && Bl(e, n), Mr(e)) : Bl(_e, n.stateNode));
      break;
    case 4:
      r = _e, a = St, _e = n.stateNode.containerInfo, St = !0, qt(e, t, n), _e = r, St = a;
      break;
    case 0:
    case 11:
    case 14:
    case 15:
      if (!$e && (r = n.updateQueue, r !== null && (r = r.lastEffect, r !== null))) {
        a = r = r.next;
        do {
          var i = a, o = i.destroy;
          i = i.tag, o !== void 0 && (i & 2 || i & 4) && bi(n, t, o), a = a.next;
        } while (a !== r);
      }
      qt(e, t, n);
      break;
    case 1:
      if (!$e && (Hn(n, t), r = n.stateNode, typeof r.componentWillUnmount == "function")) try {
        r.props = n.memoizedProps, r.state = n.memoizedState, r.componentWillUnmount();
      } catch (s) {
        me(n, t, s);
      }
      qt(e, t, n);
      break;
    case 21:
      qt(e, t, n);
      break;
    case 22:
      n.mode & 1 ? ($e = (r = $e) || n.memoizedState !== null, qt(e, t, n), $e = r) : qt(e, t, n);
      break;
    default:
      qt(e, t, n);
  }
}
function ac(e) {
  var t = e.updateQueue;
  if (t !== null) {
    e.updateQueue = null;
    var n = e.stateNode;
    n === null && (n = e.stateNode = new Xp()), t.forEach(function(r) {
      var a = om.bind(null, e, r);
      n.has(r) || (n.add(r), r.then(a, a));
    });
  }
}
function jt(e, t) {
  var n = t.deletions;
  if (n !== null) for (var r = 0; r < n.length; r++) {
    var a = n[r];
    try {
      var i = e, o = t, s = o;
      e: for (; s !== null; ) {
        switch (s.tag) {
          case 5:
            _e = s.stateNode, St = !1;
            break e;
          case 3:
            _e = s.stateNode.containerInfo, St = !0;
            break e;
          case 4:
            _e = s.stateNode.containerInfo, St = !0;
            break e;
        }
        s = s.return;
      }
      if (_e === null) throw Error(F(160));
      pd(i, o, a), _e = null, St = !1;
      var c = a.alternate;
      c !== null && (c.return = null), a.return = null;
    } catch (f) {
      me(a, t, f);
    }
  }
  if (t.subtreeFlags & 12854) for (t = t.child; t !== null; ) md(t, e), t = t.sibling;
}
function md(e, t) {
  var n = e.alternate, r = e.flags;
  switch (e.tag) {
    case 0:
    case 11:
    case 14:
    case 15:
      if (jt(t, e), Rt(e), r & 4) {
        try {
          Fr(3, e, e.return), fl(3, e);
        } catch (w) {
          me(e, e.return, w);
        }
        try {
          Fr(5, e, e.return);
        } catch (w) {
          me(e, e.return, w);
        }
      }
      break;
    case 1:
      jt(t, e), Rt(e), r & 512 && n !== null && Hn(n, n.return);
      break;
    case 5:
      if (jt(t, e), Rt(e), r & 512 && n !== null && Hn(n, n.return), e.flags & 32) {
        var a = e.stateNode;
        try {
          zr(a, "");
        } catch (w) {
          me(e, e.return, w);
        }
      }
      if (r & 4 && (a = e.stateNode, a != null)) {
        var i = e.memoizedProps, o = n !== null ? n.memoizedProps : i, s = e.type, c = e.updateQueue;
        if (e.updateQueue = null, c !== null) try {
          s === "input" && i.type === "radio" && i.name != null && Ac(a, i), pi(s, o);
          var f = pi(s, i);
          for (o = 0; o < c.length; o += 2) {
            var j = c[o], d = c[o + 1];
            j === "style" ? Uc(a, d) : j === "dangerouslySetInnerHTML" ? Oc(a, d) : j === "children" ? zr(a, d) : to(a, j, d, f);
          }
          switch (s) {
            case "input":
              si(a, i);
              break;
            case "textarea":
              $c(a, i);
              break;
            case "select":
              var m = a._wrapperState.wasMultiple;
              a._wrapperState.wasMultiple = !!i.multiple;
              var h = i.value;
              h != null ? Qn(a, !!i.multiple, h, !1) : m !== !!i.multiple && (i.defaultValue != null ? Qn(
                a,
                !!i.multiple,
                i.defaultValue,
                !0
              ) : Qn(a, !!i.multiple, i.multiple ? [] : "", !1));
          }
          a[Br] = i;
        } catch (w) {
          me(e, e.return, w);
        }
      }
      break;
    case 6:
      if (jt(t, e), Rt(e), r & 4) {
        if (e.stateNode === null) throw Error(F(162));
        a = e.stateNode, i = e.memoizedProps;
        try {
          a.nodeValue = i;
        } catch (w) {
          me(e, e.return, w);
        }
      }
      break;
    case 3:
      if (jt(t, e), Rt(e), r & 4 && n !== null && n.memoizedState.isDehydrated) try {
        Mr(t.containerInfo);
      } catch (w) {
        me(e, e.return, w);
      }
      break;
    case 4:
      jt(t, e), Rt(e);
      break;
    case 13:
      jt(t, e), Rt(e), a = e.child, a.flags & 8192 && (i = a.memoizedState !== null, a.stateNode.isHidden = i, !i || a.alternate !== null && a.alternate.memoizedState !== null || (Lo = he())), r & 4 && ac(e);
      break;
    case 22:
      if (j = n !== null && n.memoizedState !== null, e.mode & 1 ? ($e = (f = $e) || j, jt(t, e), $e = f) : jt(t, e), Rt(e), r & 8192) {
        if (f = e.memoizedState !== null, (e.stateNode.isHidden = f) && !j && e.mode & 1) for (O = e, j = e.child; j !== null; ) {
          for (d = O = j; O !== null; ) {
            switch (m = O, h = m.child, m.tag) {
              case 0:
              case 11:
              case 14:
              case 15:
                Fr(4, m, m.return);
                break;
              case 1:
                Hn(m, m.return);
                var N = m.stateNode;
                if (typeof N.componentWillUnmount == "function") {
                  r = m, n = m.return;
                  try {
                    t = r, N.props = t.memoizedProps, N.state = t.memoizedState, N.componentWillUnmount();
                  } catch (w) {
                    me(r, n, w);
                  }
                }
                break;
              case 5:
                Hn(m, m.return);
                break;
              case 22:
                if (m.memoizedState !== null) {
                  ic(d);
                  continue;
                }
            }
            h !== null ? (h.return = m, O = h) : ic(d);
          }
          j = j.sibling;
        }
        e: for (j = null, d = e; ; ) {
          if (d.tag === 5) {
            if (j === null) {
              j = d;
              try {
                a = d.stateNode, f ? (i = a.style, typeof i.setProperty == "function" ? i.setProperty("display", "none", "important") : i.display = "none") : (s = d.stateNode, c = d.memoizedProps.style, o = c != null && c.hasOwnProperty("display") ? c.display : null, s.style.display = bc("display", o));
              } catch (w) {
                me(e, e.return, w);
              }
            }
          } else if (d.tag === 6) {
            if (j === null) try {
              d.stateNode.nodeValue = f ? "" : d.memoizedProps;
            } catch (w) {
              me(e, e.return, w);
            }
          } else if ((d.tag !== 22 && d.tag !== 23 || d.memoizedState === null || d === e) && d.child !== null) {
            d.child.return = d, d = d.child;
            continue;
          }
          if (d === e) break e;
          for (; d.sibling === null; ) {
            if (d.return === null || d.return === e) break e;
            j === d && (j = null), d = d.return;
          }
          j === d && (j = null), d.sibling.return = d.return, d = d.sibling;
        }
      }
      break;
    case 19:
      jt(t, e), Rt(e), r & 4 && ac(e);
      break;
    case 21:
      break;
    default:
      jt(
        t,
        e
      ), Rt(e);
  }
}
function Rt(e) {
  var t = e.flags;
  if (t & 2) {
    try {
      e: {
        for (var n = e.return; n !== null; ) {
          if (fd(n)) {
            var r = n;
            break e;
          }
          n = n.return;
        }
        throw Error(F(160));
      }
      switch (r.tag) {
        case 5:
          var a = r.stateNode;
          r.flags & 32 && (zr(a, ""), r.flags &= -33);
          var i = rc(e);
          Bi(e, i, a);
          break;
        case 3:
        case 4:
          var o = r.stateNode.containerInfo, s = rc(e);
          Vi(e, s, o);
          break;
        default:
          throw Error(F(161));
      }
    } catch (c) {
      me(e, e.return, c);
    }
    e.flags &= -3;
  }
  t & 4096 && (e.flags &= -4097);
}
function Jp(e, t, n) {
  O = e, hd(e);
}
function hd(e, t, n) {
  for (var r = (e.mode & 1) !== 0; O !== null; ) {
    var a = O, i = a.child;
    if (a.tag === 22 && r) {
      var o = a.memoizedState !== null || ga;
      if (!o) {
        var s = a.alternate, c = s !== null && s.memoizedState !== null || $e;
        s = ga;
        var f = $e;
        if (ga = o, ($e = c) && !f) for (O = a; O !== null; ) o = O, c = o.child, o.tag === 22 && o.memoizedState !== null ? oc(a) : c !== null ? (c.return = o, O = c) : oc(a);
        for (; i !== null; ) O = i, hd(i), i = i.sibling;
        O = a, ga = s, $e = f;
      }
      lc(e);
    } else a.subtreeFlags & 8772 && i !== null ? (i.return = a, O = i) : lc(e);
  }
}
function lc(e) {
  for (; O !== null; ) {
    var t = O;
    if (t.flags & 8772) {
      var n = t.alternate;
      try {
        if (t.flags & 8772) switch (t.tag) {
          case 0:
          case 11:
          case 15:
            $e || fl(5, t);
            break;
          case 1:
            var r = t.stateNode;
            if (t.flags & 4 && !$e) if (n === null) r.componentDidMount();
            else {
              var a = t.elementType === t.type ? n.memoizedProps : Nt(t.type, n.memoizedProps);
              r.componentDidUpdate(a, n.memoizedState, r.__reactInternalSnapshotBeforeUpdate);
            }
            var i = t.updateQueue;
            i !== null && Bs(t, i, r);
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
              Bs(t, o, n);
            }
            break;
          case 5:
            var s = t.stateNode;
            if (n === null && t.flags & 4) {
              n = s;
              var c = t.memoizedProps;
              switch (t.type) {
                case "button":
                case "input":
                case "select":
                case "textarea":
                  c.autoFocus && n.focus();
                  break;
                case "img":
                  c.src && (n.src = c.src);
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
              var f = t.alternate;
              if (f !== null) {
                var j = f.memoizedState;
                if (j !== null) {
                  var d = j.dehydrated;
                  d !== null && Mr(d);
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
            throw Error(F(163));
        }
        $e || t.flags & 512 && Ui(t);
      } catch (m) {
        me(t, t.return, m);
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
function ic(e) {
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
function oc(e) {
  for (; O !== null; ) {
    var t = O;
    try {
      switch (t.tag) {
        case 0:
        case 11:
        case 15:
          var n = t.return;
          try {
            fl(4, t);
          } catch (c) {
            me(t, n, c);
          }
          break;
        case 1:
          var r = t.stateNode;
          if (typeof r.componentDidMount == "function") {
            var a = t.return;
            try {
              r.componentDidMount();
            } catch (c) {
              me(t, a, c);
            }
          }
          var i = t.return;
          try {
            Ui(t);
          } catch (c) {
            me(t, i, c);
          }
          break;
        case 5:
          var o = t.return;
          try {
            Ui(t);
          } catch (c) {
            me(t, o, c);
          }
      }
    } catch (c) {
      me(t, t.return, c);
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
var em = Math.ceil, Za = Qt.ReactCurrentDispatcher, Do = Qt.ReactCurrentOwner, vt = Qt.ReactCurrentBatchConfig, Z = 0, Fe = null, ye = null, Te = 0, nt = 0, Wn = hn(0), ke = 0, qr = null, Pn = 0, pl = 0, zo = 0, Rr = null, Ye = null, Lo = 0, lr = 1 / 0, $t = null, Ja = !1, Hi = null, sn = null, ya = !1, tn = null, el = 0, _r = 0, Wi = null, Ra = -1, _a = 0;
function Qe() {
  return Z & 6 ? he() : Ra !== -1 ? Ra : Ra = he();
}
function cn(e) {
  return e.mode & 1 ? Z & 2 && Te !== 0 ? Te & -Te : $p.transition !== null ? (_a === 0 && (_a = Jc()), _a) : (e = te, e !== 0 || (e = window.event, e = e === void 0 ? 16 : iu(e.type)), e) : 1;
}
function kt(e, t, n, r) {
  if (50 < _r) throw _r = 0, Wi = null, Error(F(185));
  Zr(e, n, r), (!(Z & 2) || e !== Fe) && (e === Fe && (!(Z & 2) && (pl |= n), ke === 4 && Jt(e, Te)), et(e, r), n === 1 && Z === 0 && !(t.mode & 1) && (lr = he() + 500, cl && vn()));
}
function et(e, t) {
  var n = e.callbackNode;
  $f(e, t);
  var r = $a(e, e === Fe ? Te : 0);
  if (r === 0) n !== null && vs(n), e.callbackNode = null, e.callbackPriority = 0;
  else if (t = r & -r, e.callbackPriority !== t) {
    if (n != null && vs(n), t === 1) e.tag === 0 ? Ap(sc.bind(null, e)) : ku(sc.bind(null, e)), Tp(function() {
      !(Z & 6) && vn();
    }), n = null;
    else {
      switch (eu(r)) {
        case 1:
          n = io;
          break;
        case 4:
          n = Xc;
          break;
        case 16:
          n = Aa;
          break;
        case 536870912:
          n = Zc;
          break;
        default:
          n = Aa;
      }
      n = wd(n, vd.bind(null, e));
    }
    e.callbackPriority = t, e.callbackNode = n;
  }
}
function vd(e, t) {
  if (Ra = -1, _a = 0, Z & 6) throw Error(F(327));
  var n = e.callbackNode;
  if (Xn() && e.callbackNode !== n) return null;
  var r = $a(e, e === Fe ? Te : 0);
  if (r === 0) return null;
  if (r & 30 || r & e.expiredLanes || t) t = tl(e, r);
  else {
    t = r;
    var a = Z;
    Z |= 2;
    var i = gd();
    (Fe !== e || Te !== t) && ($t = null, lr = he() + 500, Sn(e, t));
    do
      try {
        rm();
        break;
      } catch (s) {
        xd(e, s);
      }
    while (!0);
    jo(), Za.current = i, Z = a, ye !== null ? t = 0 : (Fe = null, Te = 0, t = ke);
  }
  if (t !== 0) {
    if (t === 2 && (a = gi(e), a !== 0 && (r = a, t = Qi(e, a))), t === 1) throw n = qr, Sn(e, 0), Jt(e, r), et(e, he()), n;
    if (t === 6) Jt(e, r);
    else {
      if (a = e.current.alternate, !(r & 30) && !tm(a) && (t = tl(e, r), t === 2 && (i = gi(e), i !== 0 && (r = i, t = Qi(e, i))), t === 1)) throw n = qr, Sn(e, 0), Jt(e, r), et(e, he()), n;
      switch (e.finishedWork = a, e.finishedLanes = r, t) {
        case 0:
        case 1:
          throw Error(F(345));
        case 2:
          gn(e, Ye, $t);
          break;
        case 3:
          if (Jt(e, r), (r & 130023424) === r && (t = Lo + 500 - he(), 10 < t)) {
            if ($a(e, 0) !== 0) break;
            if (a = e.suspendedLanes, (a & r) !== r) {
              Qe(), e.pingedLanes |= e.suspendedLanes & a;
              break;
            }
            e.timeoutHandle = Ei(gn.bind(null, e, Ye, $t), t);
            break;
          }
          gn(e, Ye, $t);
          break;
        case 4:
          if (Jt(e, r), (r & 4194240) === r) break;
          for (t = e.eventTimes, a = -1; 0 < r; ) {
            var o = 31 - Ct(r);
            i = 1 << o, o = t[o], o > a && (a = o), r &= ~i;
          }
          if (r = a, r = he() - r, r = (120 > r ? 120 : 480 > r ? 480 : 1080 > r ? 1080 : 1920 > r ? 1920 : 3e3 > r ? 3e3 : 4320 > r ? 4320 : 1960 * em(r / 1960)) - r, 10 < r) {
            e.timeoutHandle = Ei(gn.bind(null, e, Ye, $t), r);
            break;
          }
          gn(e, Ye, $t);
          break;
        case 5:
          gn(e, Ye, $t);
          break;
        default:
          throw Error(F(329));
      }
    }
  }
  return et(e, he()), e.callbackNode === n ? vd.bind(null, e) : null;
}
function Qi(e, t) {
  var n = Rr;
  return e.current.memoizedState.isDehydrated && (Sn(e, t).flags |= 256), e = tl(e, t), e !== 2 && (t = Ye, Ye = n, t !== null && Gi(t)), e;
}
function Gi(e) {
  Ye === null ? Ye = e : Ye.push.apply(Ye, e);
}
function tm(e) {
  for (var t = e; ; ) {
    if (t.flags & 16384) {
      var n = t.updateQueue;
      if (n !== null && (n = n.stores, n !== null)) for (var r = 0; r < n.length; r++) {
        var a = n[r], i = a.getSnapshot;
        a = a.value;
        try {
          if (!It(i(), a)) return !1;
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
function Jt(e, t) {
  for (t &= ~zo, t &= ~pl, e.suspendedLanes |= t, e.pingedLanes &= ~t, e = e.expirationTimes; 0 < t; ) {
    var n = 31 - Ct(t), r = 1 << n;
    e[n] = -1, t &= ~r;
  }
}
function sc(e) {
  if (Z & 6) throw Error(F(327));
  Xn();
  var t = $a(e, 0);
  if (!(t & 1)) return et(e, he()), null;
  var n = tl(e, t);
  if (e.tag !== 0 && n === 2) {
    var r = gi(e);
    r !== 0 && (t = r, n = Qi(e, r));
  }
  if (n === 1) throw n = qr, Sn(e, 0), Jt(e, t), et(e, he()), n;
  if (n === 6) throw Error(F(345));
  return e.finishedWork = e.current.alternate, e.finishedLanes = t, gn(e, Ye, $t), et(e, he()), null;
}
function Ao(e, t) {
  var n = Z;
  Z |= 1;
  try {
    return e(t);
  } finally {
    Z = n, Z === 0 && (lr = he() + 500, cl && vn());
  }
}
function Fn(e) {
  tn !== null && tn.tag === 0 && !(Z & 6) && Xn();
  var t = Z;
  Z |= 1;
  var n = vt.transition, r = te;
  try {
    if (vt.transition = null, te = 1, e) return e();
  } finally {
    te = r, vt.transition = n, Z = t, !(Z & 6) && vn();
  }
}
function $o() {
  nt = Wn.current, ie(Wn);
}
function Sn(e, t) {
  e.finishedWork = null, e.finishedLanes = 0;
  var n = e.timeoutHandle;
  if (n !== -1 && (e.timeoutHandle = -1, _p(n)), ye !== null) for (n = ye.return; n !== null; ) {
    var r = n;
    switch (xo(r), r.tag) {
      case 1:
        r = r.type.childContextTypes, r != null && Va();
        break;
      case 3:
        rr(), ie(Ze), ie(be), Eo();
        break;
      case 5:
        ko(r);
        break;
      case 4:
        rr();
        break;
      case 13:
        ie(ue);
        break;
      case 19:
        ie(ue);
        break;
      case 10:
        No(r.type._context);
        break;
      case 22:
      case 23:
        $o();
    }
    n = n.return;
  }
  if (Fe = e, ye = e = un(e.current, null), Te = nt = t, ke = 0, qr = null, zo = pl = Pn = 0, Ye = Rr = null, jn !== null) {
    for (t = 0; t < jn.length; t++) if (n = jn[t], r = n.interleaved, r !== null) {
      n.interleaved = null;
      var a = r.next, i = n.pending;
      if (i !== null) {
        var o = i.next;
        i.next = a, r.next = o;
      }
      n.pending = r;
    }
    jn = null;
  }
  return e;
}
function xd(e, t) {
  do {
    var n = ye;
    try {
      if (jo(), Ia.current = Xa, Ya) {
        for (var r = fe.memoizedState; r !== null; ) {
          var a = r.queue;
          a !== null && (a.pending = null), r = r.next;
        }
        Ya = !1;
      }
      if (In = 0, Pe = we = fe = null, Pr = !1, Qr = 0, Do.current = null, n === null || n.return === null) {
        ke = 1, qr = t, ye = null;
        break;
      }
      e: {
        var i = e, o = n.return, s = n, c = t;
        if (t = Te, s.flags |= 32768, c !== null && typeof c == "object" && typeof c.then == "function") {
          var f = c, j = s, d = j.tag;
          if (!(j.mode & 1) && (d === 0 || d === 11 || d === 15)) {
            var m = j.alternate;
            m ? (j.updateQueue = m.updateQueue, j.memoizedState = m.memoizedState, j.lanes = m.lanes) : (j.updateQueue = null, j.memoizedState = null);
          }
          var h = qs(o);
          if (h !== null) {
            h.flags &= -257, Ys(h, o, s, i, t), h.mode & 1 && Ks(i, f, t), t = h, c = f;
            var N = t.updateQueue;
            if (N === null) {
              var w = /* @__PURE__ */ new Set();
              w.add(c), t.updateQueue = w;
            } else N.add(c);
            break e;
          } else {
            if (!(t & 1)) {
              Ks(i, f, t), Mo();
              break e;
            }
            c = Error(F(426));
          }
        } else if (oe && s.mode & 1) {
          var M = qs(o);
          if (M !== null) {
            !(M.flags & 65536) && (M.flags |= 256), Ys(M, o, s, i, t), go(ar(c, s));
            break e;
          }
        }
        i = c = ar(c, s), ke !== 4 && (ke = 2), Rr === null ? Rr = [i] : Rr.push(i), i = o;
        do {
          switch (i.tag) {
            case 3:
              i.flags |= 65536, t &= -t, i.lanes |= t;
              var p = ed(i, c, t);
              Vs(i, p);
              break e;
            case 1:
              s = c;
              var u = i.type, v = i.stateNode;
              if (!(i.flags & 128) && (typeof u.getDerivedStateFromError == "function" || v !== null && typeof v.componentDidCatch == "function" && (sn === null || !sn.has(v)))) {
                i.flags |= 65536, t &= -t, i.lanes |= t;
                var E = td(i, s, t);
                Vs(i, E);
                break e;
              }
          }
          i = i.return;
        } while (i !== null);
      }
      jd(n);
    } catch (T) {
      t = T, ye === n && n !== null && (ye = n = n.return);
      continue;
    }
    break;
  } while (!0);
}
function gd() {
  var e = Za.current;
  return Za.current = Xa, e === null ? Xa : e;
}
function Mo() {
  (ke === 0 || ke === 3 || ke === 2) && (ke = 4), Fe === null || !(Pn & 268435455) && !(pl & 268435455) || Jt(Fe, Te);
}
function tl(e, t) {
  var n = Z;
  Z |= 2;
  var r = gd();
  (Fe !== e || Te !== t) && ($t = null, Sn(e, t));
  do
    try {
      nm();
      break;
    } catch (a) {
      xd(e, a);
    }
  while (!0);
  if (jo(), Z = n, Za.current = r, ye !== null) throw Error(F(261));
  return Fe = null, Te = 0, ke;
}
function nm() {
  for (; ye !== null; ) yd(ye);
}
function rm() {
  for (; ye !== null && !Pf(); ) yd(ye);
}
function yd(e) {
  var t = Sd(e.alternate, e, nt);
  e.memoizedProps = e.pendingProps, t === null ? jd(e) : ye = t, Do.current = null;
}
function jd(e) {
  var t = e;
  do {
    var n = t.alternate;
    if (e = t.return, t.flags & 32768) {
      if (n = Yp(n, t), n !== null) {
        n.flags &= 32767, ye = n;
        return;
      }
      if (e !== null) e.flags |= 32768, e.subtreeFlags = 0, e.deletions = null;
      else {
        ke = 6, ye = null;
        return;
      }
    } else if (n = qp(n, t, nt), n !== null) {
      ye = n;
      return;
    }
    if (t = t.sibling, t !== null) {
      ye = t;
      return;
    }
    ye = t = e;
  } while (t !== null);
  ke === 0 && (ke = 5);
}
function gn(e, t, n) {
  var r = te, a = vt.transition;
  try {
    vt.transition = null, te = 1, am(e, t, n, r);
  } finally {
    vt.transition = a, te = r;
  }
  return null;
}
function am(e, t, n, r) {
  do
    Xn();
  while (tn !== null);
  if (Z & 6) throw Error(F(327));
  n = e.finishedWork;
  var a = e.finishedLanes;
  if (n === null) return null;
  if (e.finishedWork = null, e.finishedLanes = 0, n === e.current) throw Error(F(177));
  e.callbackNode = null, e.callbackPriority = 0;
  var i = n.lanes | n.childLanes;
  if (Mf(e, i), e === Fe && (ye = Fe = null, Te = 0), !(n.subtreeFlags & 2064) && !(n.flags & 2064) || ya || (ya = !0, wd(Aa, function() {
    return Xn(), null;
  })), i = (n.flags & 15990) !== 0, n.subtreeFlags & 15990 || i) {
    i = vt.transition, vt.transition = null;
    var o = te;
    te = 1;
    var s = Z;
    Z |= 4, Do.current = null, Zp(e, n), md(n, e), Cp(Ci), Ma = !!wi, Ci = wi = null, e.current = n, Jp(n), Ff(), Z = s, te = o, vt.transition = i;
  } else e.current = n;
  if (ya && (ya = !1, tn = e, el = a), i = e.pendingLanes, i === 0 && (sn = null), Tf(n.stateNode), et(e, he()), t !== null) for (r = e.onRecoverableError, n = 0; n < t.length; n++) a = t[n], r(a.value, { componentStack: a.stack, digest: a.digest });
  if (Ja) throw Ja = !1, e = Hi, Hi = null, e;
  return el & 1 && e.tag !== 0 && Xn(), i = e.pendingLanes, i & 1 ? e === Wi ? _r++ : (_r = 0, Wi = e) : _r = 0, vn(), null;
}
function Xn() {
  if (tn !== null) {
    var e = eu(el), t = vt.transition, n = te;
    try {
      if (vt.transition = null, te = 16 > e ? 16 : e, tn === null) var r = !1;
      else {
        if (e = tn, tn = null, el = 0, Z & 6) throw Error(F(331));
        var a = Z;
        for (Z |= 4, O = e.current; O !== null; ) {
          var i = O, o = i.child;
          if (O.flags & 16) {
            var s = i.deletions;
            if (s !== null) {
              for (var c = 0; c < s.length; c++) {
                var f = s[c];
                for (O = f; O !== null; ) {
                  var j = O;
                  switch (j.tag) {
                    case 0:
                    case 11:
                    case 15:
                      Fr(8, j, i);
                  }
                  var d = j.child;
                  if (d !== null) d.return = j, O = d;
                  else for (; O !== null; ) {
                    j = O;
                    var m = j.sibling, h = j.return;
                    if (dd(j), j === f) {
                      O = null;
                      break;
                    }
                    if (m !== null) {
                      m.return = h, O = m;
                      break;
                    }
                    O = h;
                  }
                }
              }
              var N = i.alternate;
              if (N !== null) {
                var w = N.child;
                if (w !== null) {
                  N.child = null;
                  do {
                    var M = w.sibling;
                    w.sibling = null, w = M;
                  } while (w !== null);
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
                Fr(9, i, i.return);
            }
            var p = i.sibling;
            if (p !== null) {
              p.return = i.return, O = p;
              break e;
            }
            O = i.return;
          }
        }
        var u = e.current;
        for (O = u; O !== null; ) {
          o = O;
          var v = o.child;
          if (o.subtreeFlags & 2064 && v !== null) v.return = o, O = v;
          else e: for (o = u; O !== null; ) {
            if (s = O, s.flags & 2048) try {
              switch (s.tag) {
                case 0:
                case 11:
                case 15:
                  fl(9, s);
              }
            } catch (T) {
              me(s, s.return, T);
            }
            if (s === o) {
              O = null;
              break e;
            }
            var E = s.sibling;
            if (E !== null) {
              E.return = s.return, O = E;
              break e;
            }
            O = s.return;
          }
        }
        if (Z = a, vn(), Dt && typeof Dt.onPostCommitFiberRoot == "function") try {
          Dt.onPostCommitFiberRoot(al, e);
        } catch {
        }
        r = !0;
      }
      return r;
    } finally {
      te = n, vt.transition = t;
    }
  }
  return !1;
}
function cc(e, t, n) {
  t = ar(n, t), t = ed(e, t, 1), e = on(e, t, 1), t = Qe(), e !== null && (Zr(e, 1, t), et(e, t));
}
function me(e, t, n) {
  if (e.tag === 3) cc(e, e, n);
  else for (; t !== null; ) {
    if (t.tag === 3) {
      cc(t, e, n);
      break;
    } else if (t.tag === 1) {
      var r = t.stateNode;
      if (typeof t.type.getDerivedStateFromError == "function" || typeof r.componentDidCatch == "function" && (sn === null || !sn.has(r))) {
        e = ar(n, e), e = td(t, e, 1), t = on(t, e, 1), e = Qe(), t !== null && (Zr(t, 1, e), et(t, e));
        break;
      }
    }
    t = t.return;
  }
}
function lm(e, t, n) {
  var r = e.pingCache;
  r !== null && r.delete(t), t = Qe(), e.pingedLanes |= e.suspendedLanes & n, Fe === e && (Te & n) === n && (ke === 4 || ke === 3 && (Te & 130023424) === Te && 500 > he() - Lo ? Sn(e, 0) : zo |= n), et(e, t);
}
function Nd(e, t) {
  t === 0 && (e.mode & 1 ? (t = ca, ca <<= 1, !(ca & 130023424) && (ca = 4194304)) : t = 1);
  var n = Qe();
  e = Ht(e, t), e !== null && (Zr(e, t, n), et(e, n));
}
function im(e) {
  var t = e.memoizedState, n = 0;
  t !== null && (n = t.retryLane), Nd(e, n);
}
function om(e, t) {
  var n = 0;
  switch (e.tag) {
    case 13:
      var r = e.stateNode, a = e.memoizedState;
      a !== null && (n = a.retryLane);
      break;
    case 19:
      r = e.stateNode;
      break;
    default:
      throw Error(F(314));
  }
  r !== null && r.delete(t), Nd(e, n);
}
var Sd;
Sd = function(e, t, n) {
  if (e !== null) if (e.memoizedProps !== t.pendingProps || Ze.current) Xe = !0;
  else {
    if (!(e.lanes & n) && !(t.flags & 128)) return Xe = !1, Kp(e, t, n);
    Xe = !!(e.flags & 131072);
  }
  else Xe = !1, oe && t.flags & 1048576 && Eu(t, Wa, t.index);
  switch (t.lanes = 0, t.tag) {
    case 2:
      var r = t.type;
      Fa(e, t), e = t.pendingProps;
      var a = er(t, be.current);
      Yn(t, n), a = Po(null, t, r, e, a, n);
      var i = Fo();
      return t.flags |= 1, typeof a == "object" && a !== null && typeof a.render == "function" && a.$$typeof === void 0 ? (t.tag = 1, t.memoizedState = null, t.updateQueue = null, Je(r) ? (i = !0, Ba(t)) : i = !1, t.memoizedState = a.state !== null && a.state !== void 0 ? a.state : null, wo(t), a.updater = dl, t.stateNode = a, a._reactInternals = t, Di(t, r, e, n), t = Ai(null, t, r, !0, i, n)) : (t.tag = 0, oe && i && vo(t), He(null, t, a, n), t = t.child), t;
    case 16:
      r = t.elementType;
      e: {
        switch (Fa(e, t), e = t.pendingProps, a = r._init, r = a(r._payload), t.type = r, a = t.tag = cm(r), e = Nt(r, e), a) {
          case 0:
            t = Li(null, t, r, e, n);
            break e;
          case 1:
            t = Js(null, t, r, e, n);
            break e;
          case 11:
            t = Xs(null, t, r, e, n);
            break e;
          case 14:
            t = Zs(null, t, r, Nt(r.type, e), n);
            break e;
        }
        throw Error(F(
          306,
          r,
          ""
        ));
      }
      return t;
    case 0:
      return r = t.type, a = t.pendingProps, a = t.elementType === r ? a : Nt(r, a), Li(e, t, r, a, n);
    case 1:
      return r = t.type, a = t.pendingProps, a = t.elementType === r ? a : Nt(r, a), Js(e, t, r, a, n);
    case 3:
      e: {
        if (ld(t), e === null) throw Error(F(387));
        r = t.pendingProps, i = t.memoizedState, a = i.element, Tu(e, t), Ka(t, r, null, n);
        var o = t.memoizedState;
        if (r = o.element, i.isDehydrated) if (i = { element: r, isDehydrated: !1, cache: o.cache, pendingSuspenseBoundaries: o.pendingSuspenseBoundaries, transitions: o.transitions }, t.updateQueue.baseState = i, t.memoizedState = i, t.flags & 256) {
          a = ar(Error(F(423)), t), t = ec(e, t, r, n, a);
          break e;
        } else if (r !== a) {
          a = ar(Error(F(424)), t), t = ec(e, t, r, n, a);
          break e;
        } else for (rt = ln(t.stateNode.containerInfo.firstChild), at = t, oe = !0, wt = null, n = Ru(t, null, r, n), t.child = n; n; ) n.flags = n.flags & -3 | 4096, n = n.sibling;
        else {
          if (tr(), r === a) {
            t = Wt(e, t, n);
            break e;
          }
          He(e, t, r, n);
        }
        t = t.child;
      }
      return t;
    case 5:
      return Du(t), e === null && Ri(t), r = t.type, a = t.pendingProps, i = e !== null ? e.memoizedProps : null, o = a.children, ki(r, a) ? o = null : i !== null && ki(r, i) && (t.flags |= 32), ad(e, t), He(e, t, o, n), t.child;
    case 6:
      return e === null && Ri(t), null;
    case 13:
      return id(e, t, n);
    case 4:
      return Co(t, t.stateNode.containerInfo), r = t.pendingProps, e === null ? t.child = nr(t, null, r, n) : He(e, t, r, n), t.child;
    case 11:
      return r = t.type, a = t.pendingProps, a = t.elementType === r ? a : Nt(r, a), Xs(e, t, r, a, n);
    case 7:
      return He(e, t, t.pendingProps, n), t.child;
    case 8:
      return He(e, t, t.pendingProps.children, n), t.child;
    case 12:
      return He(e, t, t.pendingProps.children, n), t.child;
    case 10:
      e: {
        if (r = t.type._context, a = t.pendingProps, i = t.memoizedProps, o = a.value, ae(Qa, r._currentValue), r._currentValue = o, i !== null) if (It(i.value, o)) {
          if (i.children === a.children && !Ze.current) {
            t = Wt(e, t, n);
            break e;
          }
        } else for (i = t.child, i !== null && (i.return = t); i !== null; ) {
          var s = i.dependencies;
          if (s !== null) {
            o = i.child;
            for (var c = s.firstContext; c !== null; ) {
              if (c.context === r) {
                if (i.tag === 1) {
                  c = Ut(-1, n & -n), c.tag = 2;
                  var f = i.updateQueue;
                  if (f !== null) {
                    f = f.shared;
                    var j = f.pending;
                    j === null ? c.next = c : (c.next = j.next, j.next = c), f.pending = c;
                  }
                }
                i.lanes |= n, c = i.alternate, c !== null && (c.lanes |= n), _i(
                  i.return,
                  n,
                  t
                ), s.lanes |= n;
                break;
              }
              c = c.next;
            }
          } else if (i.tag === 10) o = i.type === t.type ? null : i.child;
          else if (i.tag === 18) {
            if (o = i.return, o === null) throw Error(F(341));
            o.lanes |= n, s = o.alternate, s !== null && (s.lanes |= n), _i(o, n, t), o = i.sibling;
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
        He(e, t, a.children, n), t = t.child;
      }
      return t;
    case 9:
      return a = t.type, r = t.pendingProps.children, Yn(t, n), a = xt(a), r = r(a), t.flags |= 1, He(e, t, r, n), t.child;
    case 14:
      return r = t.type, a = Nt(r, t.pendingProps), a = Nt(r.type, a), Zs(e, t, r, a, n);
    case 15:
      return nd(e, t, t.type, t.pendingProps, n);
    case 17:
      return r = t.type, a = t.pendingProps, a = t.elementType === r ? a : Nt(r, a), Fa(e, t), t.tag = 1, Je(r) ? (e = !0, Ba(t)) : e = !1, Yn(t, n), Ju(t, r, a), Di(t, r, a, n), Ai(null, t, r, !0, e, n);
    case 19:
      return od(e, t, n);
    case 22:
      return rd(e, t, n);
  }
  throw Error(F(156, t.tag));
};
function wd(e, t) {
  return Yc(e, t);
}
function sm(e, t, n, r) {
  this.tag = e, this.key = n, this.sibling = this.child = this.return = this.stateNode = this.type = this.elementType = null, this.index = 0, this.ref = null, this.pendingProps = t, this.dependencies = this.memoizedState = this.updateQueue = this.memoizedProps = null, this.mode = r, this.subtreeFlags = this.flags = 0, this.deletions = null, this.childLanes = this.lanes = 0, this.alternate = null;
}
function ht(e, t, n, r) {
  return new sm(e, t, n, r);
}
function Oo(e) {
  return e = e.prototype, !(!e || !e.isReactComponent);
}
function cm(e) {
  if (typeof e == "function") return Oo(e) ? 1 : 0;
  if (e != null) {
    if (e = e.$$typeof, e === ro) return 11;
    if (e === ao) return 14;
  }
  return 2;
}
function un(e, t) {
  var n = e.alternate;
  return n === null ? (n = ht(e.tag, t, e.key, e.mode), n.elementType = e.elementType, n.type = e.type, n.stateNode = e.stateNode, n.alternate = e, e.alternate = n) : (n.pendingProps = t, n.type = e.type, n.flags = 0, n.subtreeFlags = 0, n.deletions = null), n.flags = e.flags & 14680064, n.childLanes = e.childLanes, n.lanes = e.lanes, n.child = e.child, n.memoizedProps = e.memoizedProps, n.memoizedState = e.memoizedState, n.updateQueue = e.updateQueue, t = e.dependencies, n.dependencies = t === null ? null : { lanes: t.lanes, firstContext: t.firstContext }, n.sibling = e.sibling, n.index = e.index, n.ref = e.ref, n;
}
function Ta(e, t, n, r, a, i) {
  var o = 2;
  if (r = e, typeof e == "function") Oo(e) && (o = 1);
  else if (typeof e == "string") o = 5;
  else e: switch (e) {
    case Ln:
      return wn(n.children, a, i, t);
    case no:
      o = 8, a |= 8;
      break;
    case ri:
      return e = ht(12, n, t, a | 2), e.elementType = ri, e.lanes = i, e;
    case ai:
      return e = ht(13, n, t, a), e.elementType = ai, e.lanes = i, e;
    case li:
      return e = ht(19, n, t, a), e.elementType = li, e.lanes = i, e;
    case Dc:
      return ml(n, a, i, t);
    default:
      if (typeof e == "object" && e !== null) switch (e.$$typeof) {
        case _c:
          o = 10;
          break e;
        case Tc:
          o = 9;
          break e;
        case ro:
          o = 11;
          break e;
        case ao:
          o = 14;
          break e;
        case Yt:
          o = 16, r = null;
          break e;
      }
      throw Error(F(130, e == null ? e : typeof e, ""));
  }
  return t = ht(o, n, t, a), t.elementType = e, t.type = r, t.lanes = i, t;
}
function wn(e, t, n, r) {
  return e = ht(7, e, r, t), e.lanes = n, e;
}
function ml(e, t, n, r) {
  return e = ht(22, e, r, t), e.elementType = Dc, e.lanes = n, e.stateNode = { isHidden: !1 }, e;
}
function Xl(e, t, n) {
  return e = ht(6, e, null, t), e.lanes = n, e;
}
function Zl(e, t, n) {
  return t = ht(4, e.children !== null ? e.children : [], e.key, t), t.lanes = n, t.stateNode = { containerInfo: e.containerInfo, pendingChildren: null, implementation: e.implementation }, t;
}
function um(e, t, n, r, a) {
  this.tag = t, this.containerInfo = e, this.finishedWork = this.pingCache = this.current = this.pendingChildren = null, this.timeoutHandle = -1, this.callbackNode = this.pendingContext = this.context = null, this.callbackPriority = 0, this.eventTimes = Tl(0), this.expirationTimes = Tl(-1), this.entangledLanes = this.finishedLanes = this.mutableReadLanes = this.expiredLanes = this.pingedLanes = this.suspendedLanes = this.pendingLanes = 0, this.entanglements = Tl(0), this.identifierPrefix = r, this.onRecoverableError = a, this.mutableSourceEagerHydrationData = null;
}
function bo(e, t, n, r, a, i, o, s, c) {
  return e = new um(e, t, n, s, c), t === 1 ? (t = 1, i === !0 && (t |= 8)) : t = 0, i = ht(3, null, null, t), e.current = i, i.stateNode = e, i.memoizedState = { element: r, isDehydrated: n, cache: null, transitions: null, pendingSuspenseBoundaries: null }, wo(i), e;
}
function dm(e, t, n) {
  var r = 3 < arguments.length && arguments[3] !== void 0 ? arguments[3] : null;
  return { $$typeof: zn, key: r == null ? null : "" + r, children: e, containerInfo: t, implementation: n };
}
function Cd(e) {
  if (!e) return fn;
  e = e._reactInternals;
  e: {
    if (_n(e) !== e || e.tag !== 1) throw Error(F(170));
    var t = e;
    do {
      switch (t.tag) {
        case 3:
          t = t.stateNode.context;
          break e;
        case 1:
          if (Je(t.type)) {
            t = t.stateNode.__reactInternalMemoizedMergedChildContext;
            break e;
          }
      }
      t = t.return;
    } while (t !== null);
    throw Error(F(171));
  }
  if (e.tag === 1) {
    var n = e.type;
    if (Je(n)) return Cu(e, n, t);
  }
  return t;
}
function kd(e, t, n, r, a, i, o, s, c) {
  return e = bo(n, r, !0, e, a, i, o, s, c), e.context = Cd(null), n = e.current, r = Qe(), a = cn(n), i = Ut(r, a), i.callback = t ?? null, on(n, i, a), e.current.lanes = a, Zr(e, a, r), et(e, r), e;
}
function hl(e, t, n, r) {
  var a = t.current, i = Qe(), o = cn(a);
  return n = Cd(n), t.context === null ? t.context = n : t.pendingContext = n, t = Ut(i, o), t.payload = { element: e }, r = r === void 0 ? null : r, r !== null && (t.callback = r), e = on(a, t, o), e !== null && (kt(e, a, o, i), Ea(e, a, o)), o;
}
function nl(e) {
  if (e = e.current, !e.child) return null;
  switch (e.child.tag) {
    case 5:
      return e.child.stateNode;
    default:
      return e.child.stateNode;
  }
}
function uc(e, t) {
  if (e = e.memoizedState, e !== null && e.dehydrated !== null) {
    var n = e.retryLane;
    e.retryLane = n !== 0 && n < t ? n : t;
  }
}
function Uo(e, t) {
  uc(e, t), (e = e.alternate) && uc(e, t);
}
function fm() {
  return null;
}
var Ed = typeof reportError == "function" ? reportError : function(e) {
  console.error(e);
};
function Vo(e) {
  this._internalRoot = e;
}
vl.prototype.render = Vo.prototype.render = function(e) {
  var t = this._internalRoot;
  if (t === null) throw Error(F(409));
  hl(e, t, null, null);
};
vl.prototype.unmount = Vo.prototype.unmount = function() {
  var e = this._internalRoot;
  if (e !== null) {
    this._internalRoot = null;
    var t = e.containerInfo;
    Fn(function() {
      hl(null, e, null, null);
    }), t[Bt] = null;
  }
};
function vl(e) {
  this._internalRoot = e;
}
vl.prototype.unstable_scheduleHydration = function(e) {
  if (e) {
    var t = ru();
    e = { blockedOn: null, target: e, priority: t };
    for (var n = 0; n < Zt.length && t !== 0 && t < Zt[n].priority; n++) ;
    Zt.splice(n, 0, e), n === 0 && lu(e);
  }
};
function Bo(e) {
  return !(!e || e.nodeType !== 1 && e.nodeType !== 9 && e.nodeType !== 11);
}
function xl(e) {
  return !(!e || e.nodeType !== 1 && e.nodeType !== 9 && e.nodeType !== 11 && (e.nodeType !== 8 || e.nodeValue !== " react-mount-point-unstable "));
}
function dc() {
}
function pm(e, t, n, r, a) {
  if (a) {
    if (typeof r == "function") {
      var i = r;
      r = function() {
        var f = nl(o);
        i.call(f);
      };
    }
    var o = kd(t, r, e, 0, null, !1, !1, "", dc);
    return e._reactRootContainer = o, e[Bt] = o.current, Ur(e.nodeType === 8 ? e.parentNode : e), Fn(), o;
  }
  for (; a = e.lastChild; ) e.removeChild(a);
  if (typeof r == "function") {
    var s = r;
    r = function() {
      var f = nl(c);
      s.call(f);
    };
  }
  var c = bo(e, 0, !1, null, null, !1, !1, "", dc);
  return e._reactRootContainer = c, e[Bt] = c.current, Ur(e.nodeType === 8 ? e.parentNode : e), Fn(function() {
    hl(t, c, n, r);
  }), c;
}
function gl(e, t, n, r, a) {
  var i = n._reactRootContainer;
  if (i) {
    var o = i;
    if (typeof a == "function") {
      var s = a;
      a = function() {
        var c = nl(o);
        s.call(c);
      };
    }
    hl(t, o, e, a);
  } else o = pm(n, t, e, a, r);
  return nl(o);
}
tu = function(e) {
  switch (e.tag) {
    case 3:
      var t = e.stateNode;
      if (t.current.memoizedState.isDehydrated) {
        var n = Nr(t.pendingLanes);
        n !== 0 && (oo(t, n | 1), et(t, he()), !(Z & 6) && (lr = he() + 500, vn()));
      }
      break;
    case 13:
      Fn(function() {
        var r = Ht(e, 1);
        if (r !== null) {
          var a = Qe();
          kt(r, e, 1, a);
        }
      }), Uo(e, 1);
  }
};
so = function(e) {
  if (e.tag === 13) {
    var t = Ht(e, 134217728);
    if (t !== null) {
      var n = Qe();
      kt(t, e, 134217728, n);
    }
    Uo(e, 134217728);
  }
};
nu = function(e) {
  if (e.tag === 13) {
    var t = cn(e), n = Ht(e, t);
    if (n !== null) {
      var r = Qe();
      kt(n, e, t, r);
    }
    Uo(e, t);
  }
};
ru = function() {
  return te;
};
au = function(e, t) {
  var n = te;
  try {
    return te = e, t();
  } finally {
    te = n;
  }
};
hi = function(e, t, n) {
  switch (t) {
    case "input":
      if (si(e, n), t = n.name, n.type === "radio" && t != null) {
        for (n = e; n.parentNode; ) n = n.parentNode;
        for (n = n.querySelectorAll("input[name=" + JSON.stringify("" + t) + '][type="radio"]'), t = 0; t < n.length; t++) {
          var r = n[t];
          if (r !== e && r.form === e.form) {
            var a = sl(r);
            if (!a) throw Error(F(90));
            Lc(r), si(r, a);
          }
        }
      }
      break;
    case "textarea":
      $c(e, n);
      break;
    case "select":
      t = n.value, t != null && Qn(e, !!n.multiple, t, !1);
  }
};
Hc = Ao;
Wc = Fn;
var mm = { usingClientEntryPoint: !1, Events: [ea, On, sl, Vc, Bc, Ao] }, gr = { findFiberByHostInstance: yn, bundleType: 0, version: "18.3.1", rendererPackageName: "react-dom" }, hm = { bundleType: gr.bundleType, version: gr.version, rendererPackageName: gr.rendererPackageName, rendererConfig: gr.rendererConfig, overrideHookState: null, overrideHookStateDeletePath: null, overrideHookStateRenamePath: null, overrideProps: null, overridePropsDeletePath: null, overridePropsRenamePath: null, setErrorHandler: null, setSuspenseHandler: null, scheduleUpdate: null, currentDispatcherRef: Qt.ReactCurrentDispatcher, findHostInstanceByFiber: function(e) {
  return e = Kc(e), e === null ? null : e.stateNode;
}, findFiberByHostInstance: gr.findFiberByHostInstance || fm, findHostInstancesForRefresh: null, scheduleRefresh: null, scheduleRoot: null, setRefreshHandler: null, getCurrentFiber: null, reconcilerVersion: "18.3.1-next-f1338f8080-20240426" };
if (typeof __REACT_DEVTOOLS_GLOBAL_HOOK__ < "u") {
  var ja = __REACT_DEVTOOLS_GLOBAL_HOOK__;
  if (!ja.isDisabled && ja.supportsFiber) try {
    al = ja.inject(hm), Dt = ja;
  } catch {
  }
}
it.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED = mm;
it.createPortal = function(e, t) {
  var n = 2 < arguments.length && arguments[2] !== void 0 ? arguments[2] : null;
  if (!Bo(t)) throw Error(F(200));
  return dm(e, t, null, n);
};
it.createRoot = function(e, t) {
  if (!Bo(e)) throw Error(F(299));
  var n = !1, r = "", a = Ed;
  return t != null && (t.unstable_strictMode === !0 && (n = !0), t.identifierPrefix !== void 0 && (r = t.identifierPrefix), t.onRecoverableError !== void 0 && (a = t.onRecoverableError)), t = bo(e, 1, !1, null, null, n, !1, r, a), e[Bt] = t.current, Ur(e.nodeType === 8 ? e.parentNode : e), new Vo(t);
};
it.findDOMNode = function(e) {
  if (e == null) return null;
  if (e.nodeType === 1) return e;
  var t = e._reactInternals;
  if (t === void 0)
    throw typeof e.render == "function" ? Error(F(188)) : (e = Object.keys(e).join(","), Error(F(268, e)));
  return e = Kc(t), e = e === null ? null : e.stateNode, e;
};
it.flushSync = function(e) {
  return Fn(e);
};
it.hydrate = function(e, t, n) {
  if (!xl(t)) throw Error(F(200));
  return gl(null, e, t, !0, n);
};
it.hydrateRoot = function(e, t, n) {
  if (!Bo(e)) throw Error(F(405));
  var r = n != null && n.hydratedSources || null, a = !1, i = "", o = Ed;
  if (n != null && (n.unstable_strictMode === !0 && (a = !0), n.identifierPrefix !== void 0 && (i = n.identifierPrefix), n.onRecoverableError !== void 0 && (o = n.onRecoverableError)), t = kd(t, null, e, 1, n ?? null, a, !1, i, o), e[Bt] = t.current, Ur(e), r) for (e = 0; e < r.length; e++) n = r[e], a = n._getVersion, a = a(n._source), t.mutableSourceEagerHydrationData == null ? t.mutableSourceEagerHydrationData = [n, a] : t.mutableSourceEagerHydrationData.push(
    n,
    a
  );
  return new vl(t);
};
it.render = function(e, t, n) {
  if (!xl(t)) throw Error(F(200));
  return gl(null, e, t, !1, n);
};
it.unmountComponentAtNode = function(e) {
  if (!xl(e)) throw Error(F(40));
  return e._reactRootContainer ? (Fn(function() {
    gl(null, null, e, !1, function() {
      e._reactRootContainer = null, e[Bt] = null;
    });
  }), !0) : !1;
};
it.unstable_batchedUpdates = Ao;
it.unstable_renderSubtreeIntoContainer = function(e, t, n, r) {
  if (!xl(n)) throw Error(F(200));
  if (e == null || e._reactInternals === void 0) throw Error(F(38));
  return gl(e, t, n, !1, r);
};
it.version = "18.3.1-next-f1338f8080-20240426";
function Id() {
  if (!(typeof __REACT_DEVTOOLS_GLOBAL_HOOK__ > "u" || typeof __REACT_DEVTOOLS_GLOBAL_HOOK__.checkDCE != "function"))
    try {
      __REACT_DEVTOOLS_GLOBAL_HOOK__.checkDCE(Id);
    } catch (e) {
      console.error(e);
    }
}
Id(), Ic.exports = it;
var vm = Ic.exports, Pd, fc = vm;
Pd = fc.createRoot, fc.hydrateRoot;
class xm extends Error {
  constructor(t, n) {
    super(t), this.status = n, this.name = "ApiError";
  }
}
function gm(e, t) {
  async function n(r, a = {}) {
    const i = { ...a.headers ?? {} }, o = t();
    o && (i.Authorization = "Bearer " + o);
    let s;
    a.body !== void 0 && (i["Content-Type"] = "application/json", s = JSON.stringify(a.body));
    const c = await e(r, { method: a.method ?? "GET", headers: i, body: s });
    if (!c.ok) {
      let j = `HTTP ${c.status}`;
      try {
        const d = await c.json();
        j = d.detail || d.title || j;
      } catch {
      }
      throw new xm(j, c.status);
    }
    return c.status === 204 ? void 0 : (c.headers.get("content-type") ?? "").includes("json") ? await c.json() : await c.text();
  }
  return {
    get: (r) => n(r),
    post: (r, a) => n(r, { method: "POST", body: a }),
    put: (r, a) => n(r, { method: "PUT", body: a }),
    del: (r) => n(r, { method: "DELETE" })
  };
}
const Fd = x.createContext(null);
function Lt() {
  const e = x.useContext(Fd);
  if (!e) throw new Error("Fuera del módulo de documentos");
  return e;
}
function ym(e) {
  return gm((t, n) => fetch(t, n), e.token);
}
async function Yr(e, t) {
  const n = e.token(), r = await fetch(t, { headers: n ? { Authorization: "Bearer " + n } : {} });
  if (!r.ok) throw new Error("No se pudo generar el documento");
  const a = URL.createObjectURL(await r.blob());
  window.open(a, "_blank"), setTimeout(() => URL.revokeObjectURL(a), 6e4);
}
async function jm(e, t) {
  var s;
  const n = e.token(), r = await fetch(t, { headers: n ? { Authorization: "Bearer " + n } : {} });
  if (!r.ok) throw new Error((await r.json().catch(() => ({}))).title || "No se pudo generar el fichero");
  const a = ((s = /filename="?([^";]+)"?/.exec(r.headers.get("Content-Disposition") ?? "")) == null ? void 0 : s[1]) ?? "fichero", i = URL.createObjectURL(await r.blob()), o = document.createElement("a");
  o.href = i, o.download = a, o.click(), setTimeout(() => URL.revokeObjectURL(i), 6e4);
}
function Rd(e, t) {
  return `${e.toLowerCase().normalize("NFD").replace(/[̀-ͯ]/g, "").replace(/[^a-z0-9]+/g, "-").replace(/^-|-$/g, "").slice(0, 60) || "listado"}-${(/* @__PURE__ */ new Date()).toISOString().slice(0, 10)}.${t}`;
}
function _d(e, t) {
  const n = URL.createObjectURL(e), r = document.createElement("a");
  r.href = n, r.download = t, document.body.appendChild(r), r.click(), r.remove(), setTimeout(() => URL.revokeObjectURL(n), 6e4);
}
async function Nm(e, t) {
  const n = await fetch("/exportar/xlsx", {
    method: "POST",
    headers: { "Content-Type": "application/json", ...e ? { Authorization: "Bearer " + e } : {} },
    body: JSON.stringify(t)
  });
  if (!n.ok) {
    let r = `HTTP ${n.status}`;
    try {
      r = (await n.json()).title || r;
    } catch {
    }
    throw new Error(r);
  }
  _d(await n.blob(), Rd(t.titulo, "xlsx"));
}
function Sm(e) {
  const t = (r, a) => {
    if (r == null) return "";
    const i = typeof r == "number" ? a === "texto" ? String(r) : r.toFixed(2).replace(".", ",") : r;
    return /[";\n]/.test(i) ? `"${i.replace(/"/g, '""')}"` : i;
  }, n = [e.columnas.map((r) => t(r.titulo, "texto")).join(";")];
  for (const r of e.filas) n.push(r.map((a, i) => {
    var o;
    return t(a, ((o = e.columnas[i]) == null ? void 0 : o.tipo) ?? "texto");
  }).join(";"));
  return n.join(`\r
`);
}
function wm(e) {
  _d(new Blob(["\uFEFF" + Sm(e)], { type: "text/csv;charset=utf-8" }), Rd(e.titulo, "csv"));
}
const Td = new Intl.NumberFormat("es-ES", { minimumFractionDigits: 2, maximumFractionDigits: 2 }), Cm = new Intl.NumberFormat("es-ES", { maximumFractionDigits: 3 }), z = (e) => `${Td.format(Number(e) || 0)} €`, de = (e) => Td.format(Number(e) || 0), pt = (e, t) => t ? `${de(e)} ${t}` : z(e), We = (e) => Cm.format(Number(e) || 0), Ce = (e) => {
  if (!e) return "";
  const t = String(e).slice(0, 10).split("-");
  return t.length === 3 ? `${t[2]}/${t[1]}/${t[0]}` : String(e);
}, Et = () => (/* @__PURE__ */ new Date()).toISOString().slice(0, 10), Me = (e) => Math.round((e + Number.EPSILON) * 100) / 100;
let km = 0;
const na = () => `l${Date.now().toString(36)}${(++km).toString(36)}`;
function Cn(e, t) {
  const [n, r] = x.useState(e);
  return x.useEffect(() => {
    const a = setTimeout(() => r(e), t);
    return () => clearTimeout(a);
  }, [e, t]), n;
}
function ra() {
  const e = x.useRef(0);
  return () => {
    const t = ++e.current;
    return () => t === e.current;
  };
}
function Ho(e) {
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
const pc = { presupuesto: "Presupuestos", pedido: "Pedidos de venta", factura: "Facturas emitidas", compra: "Pedidos de compra", gasto: "Facturas de proveedor y gastos" }, Em = { presupuesto: "+ Nuevo presupuesto", pedido: "+ Nuevo pedido", factura: "+ Nueva factura", compra: "+ Nuevo pedido", gasto: "+ Registrar factura" }, Im = {
  presupuesto: ["Borrador", "Aceptado", "Rechazado"],
  pedido: ["Borrador", "Confirmado", "ServidoParcial", "Servido", "Facturado", "Cancelado"],
  factura: ["Emitida", "Rectificada", "Anulada"],
  compra: ["Borrador", "Confirmado", "RecibidoParcial", "Recibido", "Facturado", "Cancelado"],
  gasto: ["Registrado", "Anulado"]
}, Jl = { numero: "numero", fecha: "fecha", tercero: "tercero", base: "base", impuestos: "impuestos", total: "total" }, mc = 50, Ki = (e) => e === "Anulada" || e === "Anulado" || e === "Cancelado";
function hc(e, t) {
  const n = { documentos: 0, baseImponible: 0, impuestos: 0, retenciones: 0, total: 0, pendiente: 0, vencido: 0, documentosVencidos: 0 };
  for (const r of e) {
    if (Ki(r.estado)) continue;
    n.documentos++, n.baseImponible += r.base, n.impuestos += r.impuestos, n.total += r.total;
    const a = r.pendiente ?? 0;
    a > 0 && (n.pendiente += a, r.vencimiento && r.vencimiento < t && (n.vencido += a, n.documentosVencidos++));
  }
  return n.baseImponible = Me(n.baseImponible), n.impuestos = Me(n.impuestos), n.total = Me(n.total), n.pendiente = Me(n.pendiente), n.vencido = Me(n.vencido), n;
}
function ei(e, t, n) {
  const r = (a) => t === "numero" || t === "tercero" || t === "estado" ? a[t].toLowerCase() : t === "fecha" ? a.fecha : a[t] ?? 0;
  return [...e].sort((a, i) => {
    const o = r(a), s = r(i), c = typeof o == "number" && typeof s == "number" ? o - s : String(o).localeCompare(String(s), "es", { numeric: !0 });
    return n ? -c : c;
  });
}
const Pm = (e, t) => ({
  id: e.id,
  numero: e.numeroCompleto,
  fecha: e.fechaEmision,
  tercero: e.clienteNombre + (e.clienteNif ? ` · ${e.clienteNif}` : ""),
  terceroId: e.clienteId,
  base: e.baseImponible,
  impuestos: e.cuotaIva,
  total: e.total,
  estado: e.estado,
  extra: e.tipo !== "Ordinaria" ? e.tipo : void 0,
  pendiente: t ? t[e.id] ?? 0 : null,
  vencimiento: e.fechaVencimiento
}), Fm = (e, t) => {
  var n, r;
  return {
    id: e.id,
    numero: e.numeroFactura ?? "—",
    fecha: e.fecha,
    tercero: `${e.proveedorTexto ?? ""}${e.numeroFactura ? "" : ` · ${e.concepto}`}`,
    terceroId: e.proveedorId,
    base: e.baseImponible,
    impuestos: Me(e.cuotaIva + (e.recargoTotal || 0)),
    total: e.total,
    estado: e.estado === "Anulado" ? "Anulada" : e.estado,
    extra: e.esRectificativa ? "Rectificativa" : void 0,
    pendiente: t ? t[e.id] ?? 0 : null,
    vencimiento: ((r = (n = e.vencimientos) == null ? void 0 : n[0]) == null ? void 0 : r.fecha) ?? e.fecha
  };
};
function Rm(e) {
  const { api: t, navegar: n, anfitrion: r } = Lt(), a = e.tipo, i = a === "factura" || a === "gasto", o = a === "compra" || a === "gasto", [s, c] = x.useState(""), [f, j] = x.useState(""), [d, m] = x.useState(""), [h, N] = x.useState(""), [w, M] = x.useState(""), [p, u] = x.useState(""), [v, E] = x.useState(""), [T, R] = x.useState(""), [D, C] = x.useState(""), [$, b] = x.useState({ campo: "fecha", desc: !0 }), [P, W] = x.useState(1), [se, Ue] = x.useState(null), [Ve, Ee] = x.useState(0), [Re, I] = x.useState(null), [g, L] = x.useState([]), [V, B] = x.useState([]), [Y, je] = x.useState(""), [ve, ce] = x.useState(!1), Ne = Cn(s, 250), st = Cn(v, 350), tt = Cn(T, 350), ct = ra(), Be = Et();
  x.useEffect(() => {
    t.get(o ? "/proveedores" : "/clientes").then((_) => L([..._].sort((q, A) => q.nombre.localeCompare(A.nombre, "es")))).catch(() => L([])), a === "factura" && t.get("/series").then((_) => B([...new Set(_.filter((q) => q.tipoDocumento === "Factura" || q.tipoDocumento === 0).map((q) => q.prefijo))].sort())).catch(() => B([]));
  }, [t, a, o]), x.useEffect(() => W(1), [Ne, f, d, h, w, p, st, tt, D, $, a]);
  const k = (_, q) => {
    const A = new URLSearchParams({ pagina: String(_), tamanoPagina: String(q) });
    Ne.trim() && A.set("texto", Ne.trim()), f && A.set("estado", f === "Anulada" && a === "gasto" ? "Anulado" : f), d && A.set("desde", d), h && A.set("hasta", h), w && A.set(a === "gasto" ? "proveedorId" : "clienteId", w), p && a === "factura" && A.set("serie", p);
    const Q = parseFloat(st.replace(/\./g, "").replace(",", ".")), ne = parseFloat(tt.replace(/\./g, "").replace(",", "."));
    isNaN(Q) || A.set("importeMin", String(Q)), isNaN(ne) || A.set("importeMax", String(ne)), D && A.set("cobro", D);
    const y = Jl[$.campo];
    return y && (A.set("orden", y === "tercero" ? a === "gasto" ? "proveedor" : "cliente" : y), A.set("desc", String($.desc))), A;
  }, Pt = async (_, q) => {
    if (a === "factura") {
      const Q = await t.get(`/facturas/buscar?${k(_, q)}`);
      return { r: Q, filas: Q.elementos.map((ne) => Pm(ne, Q.pendientes)) };
    }
    const A = await t.get(`/gastos/buscar?${k(_, q)}`);
    return { r: A, filas: A.elementos.map((Q) => Fm(Q, A.pendientes)) };
  };
  x.useEffect(() => {
    je("");
    const _ = ct();
    (async () => {
      if (i) {
        const { r: A, filas: Q } = await Pt(P, mc);
        return _() && (Ee(A.total), I(A.totales ?? null)), Q;
      }
      switch (a) {
        case "presupuesto":
          return (await t.get("/presupuestos")).map((A) => ({ id: A.id, numero: A.numeroCompleto, fecha: A.fecha, tercero: A.clienteNombre, terceroId: A.clienteId, base: A.baseImponible ?? A.total, impuestos: A.cuotaIva ?? 0, total: A.total, estado: A.estado }));
        case "pedido":
          return (await t.get("/pedidos-venta")).map((A) => {
            const Q = Me(A.lineas.reduce((ne, y) => ne + y.base, 0));
            return { id: A.id, numero: A.numeroCompleto, fecha: A.fecha, tercero: A.clienteNombre, terceroId: A.clienteId, base: Q, impuestos: Me(A.total - Q), total: A.total, estado: A.estado };
          });
        default:
          return (await t.get("/compras/pedidos")).map((A) => {
            const Q = Me(A.lineas.reduce((ne, y) => ne + y.importe, 0));
            return { id: A.id, numero: A.numeroCompleto, fecha: A.fecha, tercero: A.proveedorTexto, terceroId: A.proveedorId, base: Q, impuestos: Me(A.total - Q), total: A.total, estado: A.estado, extra: A.empresaOrigenId ? "Intragrupo" : void 0 };
          });
      }
    })().then((A) => _() && Ue(A)).catch((A) => _() && (je(A.message), Ue([])));
  }, [t, a, P, Ne, f, d, h, w, p, st, tt, D, $.campo, $.desc]);
  const ut = x.useMemo(() => {
    if (!se) return [];
    if (i) return Jl[$.campo] ? se : ei(se, $.campo, $.desc);
    const _ = Ne.trim().toLowerCase(), q = parseFloat(st.replace(/\./g, "").replace(",", ".")), A = parseFloat(tt.replace(/\./g, "").replace(",", ".")), Q = se.filter((ne) => (!_ || ne.numero.toLowerCase().includes(_) || ne.tercero.toLowerCase().includes(_)) && (!f || ne.estado === f) && (!d || ne.fecha >= d) && (!h || ne.fecha <= h) && (!w || ne.terceroId === w) && (isNaN(q) || ne.total >= q) && (isNaN(A) || ne.total <= A));
    return ei(Q, $.campo, $.desc);
  }, [se, Ne, f, d, h, w, st, tt, $, i]), Ie = i ? Re : hc(ut, Be), yt = i ? Math.max(1, Math.ceil(Ve / mc)) : 1, At = [f, d, h, w, p, v, T, D].filter(Boolean).length, Gt = o ? "Proveedor" : "Cliente";
  function xe() {
    c(""), j(""), m(""), N(""), M(""), u(""), E(""), R(""), C("");
  }
  function qe(_, q, A = !1) {
    const Q = $.campo === _;
    return /* @__PURE__ */ l.jsxs("th", { className: (A ? "num " : "") + "dx-ordenable" + (Q ? " activo" : ""), onClick: () => b({ campo: _, desc: Q ? !$.desc : _ === "fecha" || A }), title: `Ordenar por ${q.toLowerCase()}`, children: [
      q,
      /* @__PURE__ */ l.jsx("span", { className: "dx-flecha", children: Q ? $.desc ? "▼" : "▲" : "" })
    ] });
  }
  async function Ft() {
    if (!i) return ut;
    const _ = [];
    for (let q = 1; q <= 500; q++) {
      const { r: A, filas: Q } = await Pt(q, 200);
      if (_.push(...Q), _.length >= A.total || Q.length === 0) break;
    }
    return Jl[$.campo] ? _ : ei(_, $.campo, $.desc);
  }
  async function ze(_) {
    ce(!0);
    try {
      const q = await Ft(), A = i, Q = i ? Re : hc(q, Be), ne = {
        titulo: pc[a],
        columnas: [
          { titulo: "Número", tipo: "texto" },
          { titulo: "Fecha", tipo: "fecha" },
          { titulo: Gt, tipo: "texto" },
          { titulo: "Estado", tipo: "texto" },
          { titulo: "Base", tipo: "moneda", total: "no" },
          { titulo: "Impuestos", tipo: "moneda", total: "no" },
          { titulo: "Total", tipo: "moneda", total: "no" },
          ...A ? [{ titulo: "Pendiente", tipo: "moneda", total: "no" }, { titulo: "Vencimiento", tipo: "fecha" }] : []
        ],
        filas: q.map((y) => [y.numero + (y.extra ? ` (${y.extra})` : ""), Ce(y.fecha), y.tercero, y.estado, y.base, y.impuestos, y.total, ...A ? [y.pendiente ?? 0, Ce(y.vencimiento)] : []]),
        totales: Q ? [`Total · ${Q.documentos} (sin anulados)`, null, null, null, Q.baseImponible, Q.impuestos, Q.total, ...A ? [Q.pendiente, null] : []] : void 0
      };
      _ === "xlsx" ? await Nm(r.token(), ne) : wm(ne), r.aviso(`Exportados ${q.length} documento(s).`, "ok");
    } catch (q) {
      r.aviso("No se pudo exportar: " + q.message, "err");
    } finally {
      ce(!1);
    }
  }
  return /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel-head", children: [
      /* @__PURE__ */ l.jsx("h2", { children: pc[a] }),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-acciones", children: [
        /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", disabled: ve || !ut.length, onClick: () => ze("xlsx"), title: "Exportar a Excel todo lo filtrado", children: ve ? "Exportando…" : "↓ Excel" }),
        /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", disabled: ve || !ut.length, onClick: () => ze("csv"), title: "Exportar a CSV todo lo filtrado", children: "↓ CSV" }),
        /* @__PURE__ */ l.jsx("button", { className: "btn small", onClick: () => n({ tipo: a, pantalla: "editor" }), children: Em[a] })
      ] })
    ] }),
    /* @__PURE__ */ l.jsxs("div", { className: "dx-filtros", children: [
      /* @__PURE__ */ l.jsx("input", { placeholder: o ? "Número, proveedor o concepto…" : "Número, cliente o NIF…", value: s, onChange: (_) => c(_.target.value), autoFocus: !0 }),
      /* @__PURE__ */ l.jsxs("select", { value: f, onChange: (_) => j(_.target.value), "aria-label": "Estado", children: [
        /* @__PURE__ */ l.jsx("option", { value: "", children: "Todos los estados" }),
        Im[a].map((_) => /* @__PURE__ */ l.jsx("option", { value: _, children: _ }, _))
      ] }),
      /* @__PURE__ */ l.jsx("input", { type: "date", value: d, onChange: (_) => m(_.target.value), title: "Desde", "aria-label": "Desde" }),
      /* @__PURE__ */ l.jsx("input", { type: "date", value: h, onChange: (_) => N(_.target.value), title: "Hasta", "aria-label": "Hasta" })
    ] }),
    /* @__PURE__ */ l.jsxs("div", { className: "dx-filtros dx-filtros2", children: [
      /* @__PURE__ */ l.jsxs("select", { value: w, onChange: (_) => M(_.target.value), "aria-label": Gt, children: [
        /* @__PURE__ */ l.jsx("option", { value: "", children: o ? "Todos los proveedores" : "Todos los clientes" }),
        g.map((_) => /* @__PURE__ */ l.jsxs("option", { value: _.id, children: [
          _.nombre,
          _.nifFiscal ? ` · ${_.nifFiscal}` : ""
        ] }, _.id))
      ] }),
      a === "factura" && /* @__PURE__ */ l.jsxs("select", { value: p, onChange: (_) => u(_.target.value), "aria-label": "Serie", children: [
        /* @__PURE__ */ l.jsx("option", { value: "", children: "Todas las series" }),
        V.map((_) => /* @__PURE__ */ l.jsxs("option", { value: _, children: [
          "Serie ",
          _
        ] }, _))
      ] }),
      /* @__PURE__ */ l.jsx("input", { inputMode: "decimal", placeholder: "Importe mín.", value: v, onChange: (_) => E(_.target.value), "aria-label": "Importe mínimo" }),
      /* @__PURE__ */ l.jsx("input", { inputMode: "decimal", placeholder: "Importe máx.", value: T, onChange: (_) => R(_.target.value), "aria-label": "Importe máximo" }),
      i && /* @__PURE__ */ l.jsxs("select", { value: D, onChange: (_) => C(_.target.value), "aria-label": a === "gasto" ? "Estado de pago" : "Estado de cobro", children: [
        /* @__PURE__ */ l.jsx("option", { value: "", children: a === "gasto" ? "Pagadas y pendientes" : "Cobradas y pendientes" }),
        /* @__PURE__ */ l.jsx("option", { value: "pendiente", children: "Pendientes" }),
        /* @__PURE__ */ l.jsx("option", { value: "vencida", children: "Vencidas" }),
        /* @__PURE__ */ l.jsx("option", { value: a === "gasto" ? "pagada" : "cobrada", children: a === "gasto" ? "Pagadas" : "Cobradas" })
      ] }),
      (At > 0 || s) && /* @__PURE__ */ l.jsxs("button", { className: "btn small secondary", onClick: xe, children: [
        "Limpiar",
        At ? ` (${At})` : ""
      ] })
    ] }),
    Y && /* @__PURE__ */ l.jsx("p", { className: "dx-rojo", children: Y }),
    se === null ? /* @__PURE__ */ l.jsx("p", { className: "muted", children: "Cargando…" }) : ut.length === 0 ? /* @__PURE__ */ l.jsx("p", { className: "muted", children: "No hay documentos con estos filtros." }) : /* @__PURE__ */ l.jsxs("table", { className: "dx-lista-docs", children: [
      /* @__PURE__ */ l.jsx("thead", { children: /* @__PURE__ */ l.jsxs("tr", { children: [
        qe("numero", "Número"),
        qe("fecha", "Fecha"),
        qe("tercero", Gt),
        qe("estado", "Estado"),
        qe("base", "Base", !0),
        qe("impuestos", "Impuestos", !0),
        qe("total", "Total", !0),
        i && qe("pendiente", "Pendiente", !0)
      ] }) }),
      /* @__PURE__ */ l.jsx("tbody", { children: ut.map((_) => {
        const q = (_.pendiente ?? 0) > 0 && !!_.vencimiento && _.vencimiento < Be;
        return /* @__PURE__ */ l.jsxs("tr", { onClick: () => n({ tipo: a, pantalla: "vista", id: _.id }), tabIndex: 0, onKeyDown: (A) => A.key === "Enter" && n({ tipo: a, pantalla: "vista", id: _.id }), className: Ki(_.estado) ? "dx-anulado" : void 0, children: [
          /* @__PURE__ */ l.jsxs("td", { className: "mono", children: [
            /* @__PURE__ */ l.jsx("strong", { children: _.numero }),
            _.extra && /* @__PURE__ */ l.jsx("span", { className: "pill part", style: { marginLeft: 6 }, children: _.extra })
          ] }),
          /* @__PURE__ */ l.jsx("td", { children: Ce(_.fecha) }),
          /* @__PURE__ */ l.jsx("td", { children: _.tercero }),
          /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx("span", { className: Ho(_.estado), children: _.estado }) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: z(_.base) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: z(_.impuestos) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: z(_.total) }) }),
          i && /* @__PURE__ */ l.jsx("td", { className: "num", children: (_.pendiente ?? 0) > 0 ? /* @__PURE__ */ l.jsx("strong", { className: q ? "dx-rojo" : void 0, title: q ? `Vencida el ${Ce(_.vencimiento)}` : `Vence el ${Ce(_.vencimiento)}`, children: z(_.pendiente) }) : /* @__PURE__ */ l.jsx("span", { className: "muted", children: Ki(_.estado) || _.estado === "Rectificada" ? "—" : a === "gasto" ? "Pagada" : "Cobrada" }) })
        ] }, _.id);
      }) }),
      Ie && /* @__PURE__ */ l.jsx("tfoot", { children: /* @__PURE__ */ l.jsxs("tr", { className: "dx-total", children: [
        /* @__PURE__ */ l.jsxs("td", { colSpan: 4, children: [
          /* @__PURE__ */ l.jsxs("strong", { children: [
            "Total · ",
            Ie.documentos
          ] }),
          " ",
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: i ? `documento${Ie.documentos === 1 ? "" : "s"} de todo el filtro (${yt} página${yt === 1 ? "" : "s"}) · sin anulados` : "sin anulados ni cancelados" }),
          i && Ie.vencido > 0 && /* @__PURE__ */ l.jsxs("span", { className: "dx-rojo", style: { marginLeft: 8 }, children: [
            "· vencido ",
            z(Ie.vencido),
            " (",
            Ie.documentosVencidos,
            ")"
          ] })
        ] }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: z(Ie.baseImponible) }) }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: z(Ie.impuestos) }) }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: z(Ie.total) }) }),
        i && /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: z(Ie.pendiente) }) })
      ] }) })
    ] }),
    yt > 1 && /* @__PURE__ */ l.jsxs("div", { className: "dx-paginas", children: [
      /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", disabled: P <= 1, onClick: () => W(P - 1), children: "←" }),
      /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
        "Página ",
        P,
        " de ",
        yt,
        " · ",
        Ve,
        " documentos"
      ] }),
      /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", disabled: P >= yt, onClick: () => W(P + 1), children: "→" })
    ] })
  ] });
}
function pn(e) {
  return x.useEffect(() => {
    const t = (n) => n.key === "Escape" && e.alCerrar();
    return window.addEventListener("keydown", t), () => window.removeEventListener("keydown", t);
  }, [e]), /* @__PURE__ */ l.jsx("div", { className: "dx-fondo", onMouseDown: (t) => t.target === t.currentTarget && e.alCerrar(), children: /* @__PURE__ */ l.jsxs("div", { className: "dx-dialogo", style: { maxWidth: e.ancho ?? 560 }, role: "dialog", "aria-label": e.titulo, children: [
    /* @__PURE__ */ l.jsxs("div", { className: "dx-dialogo-cab", children: [
      /* @__PURE__ */ l.jsx("strong", { children: e.titulo }),
      /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: e.alCerrar, "aria-label": "Cerrar", children: "✕" })
    ] }),
    /* @__PURE__ */ l.jsx("div", { className: "dx-dialogo-cuerpo", children: e.children }),
    e.acciones && /* @__PURE__ */ l.jsx("div", { className: "dx-dialogo-pie", children: e.acciones })
  ] }) });
}
function _m(e) {
  const { api: t } = Lt(), [n, r] = x.useState(!1), [a, i] = x.useState([]), [o, s] = x.useState(null), [c, f] = x.useState(0), j = Cn(e.texto, 180), d = o === e.texto.trim() ? a : [], m = ra();
  x.useEffect(() => {
    if (!n) return;
    const N = m(), w = encodeURIComponent(j.trim());
    t.get(`/productos/buscar?texto=${w}&tamanoPagina=12`).then((M) => N() && (i(M.elementos ?? []), s(j.trim()), f(0))).catch(() => N() && (i([]), s(j.trim())));
  }, [j, n]);
  function h(N) {
    var w;
    if (n && N.key === "Enter" && e.texto.trim() && !d.length) {
      N.preventDefault(), N.stopPropagation();
      return;
    }
    if (n && d.length) {
      if (N.key === "ArrowDown") return N.preventDefault(), f((M) => Math.min(M + 1, d.length - 1));
      if (N.key === "ArrowUp") return N.preventDefault(), f((M) => Math.max(M - 1, 0));
      if (N.key === "Enter") {
        N.preventDefault(), N.stopPropagation(), e.alElegir(d[c]), r(!1);
        return;
      }
    }
    if (N.key === "Escape") return r(!1);
    if (N.key === "F2") return N.preventDefault(), r(!0);
    (w = e.alTeclaFuera) == null || w.call(e, N);
  }
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-buscador", children: [
    /* @__PURE__ */ l.jsx(
      "input",
      {
        value: e.texto,
        placeholder: "Buscar artículo…",
        autoFocus: e.autoFocus,
        onChange: (N) => (e.alCambiarTexto(N.target.value), r(!0)),
        onFocus: (N) => N.target.select(),
        onBlur: () => setTimeout(() => r(!1), 150),
        onKeyDown: h,
        ...Object.fromEntries(Object.entries(e.datos ?? {}).map(([N, w]) => [`data-${N}`, w]))
      }
    ),
    n && d.length > 0 && /* @__PURE__ */ l.jsx("div", { className: "dx-lista", children: d.map((N, w) => /* @__PURE__ */ l.jsxs(
      "div",
      {
        className: "dx-opcion" + (w === c ? " activa" : ""),
        onMouseDown: (M) => (M.preventDefault(), e.alElegir(N), r(!1)),
        onMouseEnter: () => f(w),
        children: [
          /* @__PURE__ */ l.jsxs("span", { children: [
            N.referencia && /* @__PURE__ */ l.jsxs("span", { className: "mono muted", children: [
              N.referencia,
              " · "
            ] }),
            /* @__PURE__ */ l.jsx("strong", { children: N.nombre }),
            N.familia && /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
              " · ",
              N.familia
            ] })
          ] }),
          /* @__PURE__ */ l.jsxs("span", { className: "muted", style: { whiteSpace: "nowrap" }, children: [
            z(e.precioDe ? e.precioDe(N) : N.precioUnitario),
            "/",
            N.unidad,
            N.controlarStock && /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
              " · stock ",
              We(N.stock)
            ] })
          ] })
        ]
      },
      N.id
    )) })
  ] });
}
function Wo(e) {
  const [t, n] = x.useState(""), [r, a] = x.useState(!1), [i, o] = x.useState(0), s = e.terceros.find((d) => d.id === e.valor), c = x.useMemo(() => {
    const d = t.trim().toLowerCase();
    return e.terceros.filter((m) => m.activo !== !1 && (!d || m.nombre.toLowerCase().includes(d) || (m.nifFiscal ?? "").toLowerCase().includes(d))).slice(0, 30);
  }, [t, e.terceros]), f = x.useRef(null);
  function j(d) {
    e.alCambiar(d.id), n(""), a(!1);
  }
  return /* @__PURE__ */ l.jsxs("div", { children: [
    /* @__PURE__ */ l.jsx("label", { children: e.etiqueta }),
    /* @__PURE__ */ l.jsxs("div", { className: "dx-buscador", children: [
      /* @__PURE__ */ l.jsx(
        "input",
        {
          ref: f,
          disabled: e.deshabilitado,
          value: r ? t : s ? `${s.nombre}${s.nifFiscal ? " · " + s.nifFiscal : ""}` : t,
          placeholder: `Buscar ${e.etiqueta.toLowerCase()} por nombre o NIF…`,
          onFocus: () => (a(!0), n("")),
          onBlur: () => setTimeout(() => a(!1), 150),
          onChange: (d) => (n(d.target.value), o(0)),
          onKeyDown: (d) => {
            if (d.key === "ArrowDown") return d.preventDefault(), o((m) => Math.min(m + 1, c.length - 1));
            if (d.key === "ArrowUp") return d.preventDefault(), o((m) => Math.max(m - 1, 0));
            if (d.key === "Enter" && c[i]) return d.preventDefault(), j(c[i]);
            if (d.key === "Escape") return a(!1);
          }
        }
      ),
      r && c.length > 0 && /* @__PURE__ */ l.jsx("div", { className: "dx-lista", children: c.map((d, m) => /* @__PURE__ */ l.jsxs("div", { className: "dx-opcion" + (m === i ? " activa" : ""), onMouseDown: (h) => (h.preventDefault(), j(d)), onMouseEnter: () => o(m), children: [
        /* @__PURE__ */ l.jsx("strong", { children: d.nombre }),
        /* @__PURE__ */ l.jsx("span", { className: "muted", children: [d.nifFiscal, d.poblacion].filter(Boolean).join(" · ") })
      ] }, d.id)) })
    ] })
  ] });
}
const Tm = { Porcentaje: "%", PorUnidad: "€/ud", PorKilo: "€/kg", Importe: "€" };
function Qo(e) {
  if (!e.catalogo.length) return null;
  const t = e.lista === void 0, n = e.lista ?? e.sugeridos ?? [], r = (a, i) => e.alCambiar(n.map((o, s) => s === a ? { ...o, ...i } : o));
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-conceptos", children: [
    /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
      e.documento ? "Conceptos del documento" : t ? "Conceptos automáticos" : "Conceptos",
      ":"
    ] }),
    n.map((a, i) => {
      const o = e.catalogo.find((s) => s.id === a.conceptoId);
      return /* @__PURE__ */ l.jsxs("span", { className: "dx-chip" + ((o == null ? void 0 : o.efecto) === "Coste" ? " coste" : "") + (t ? " auto" : ""), children: [
        /* @__PURE__ */ l.jsx("select", { value: a.conceptoId, onChange: (s) => r(i, { conceptoId: s.target.value, valor: null }), children: e.catalogo.map((s) => /* @__PURE__ */ l.jsxs("option", { value: s.id, children: [
          s.codigo,
          " ",
          s.sentido === "Resta" ? "−" : "+",
          Tm[s.calculo],
          s.efecto === "Coste" ? " · coste" : ""
        ] }, s.id)) }),
        /* @__PURE__ */ l.jsx("input", { type: "number", step: "0.0001", value: a.valor ?? "", placeholder: String((o == null ? void 0 : o.valor) ?? ""), onChange: (s) => r(i, { valor: s.target.value === "" ? null : Number(s.target.value) }) }),
        /* @__PURE__ */ l.jsx("button", { type: "button", title: "Quitar", onClick: () => e.alCambiar(n.filter((s, c) => c !== i)), children: "✕" })
      ] }, i);
    }),
    /* @__PURE__ */ l.jsx("button", { type: "button", className: "dx-enlace", onClick: () => e.alCambiar([...n, { conceptoId: e.catalogo[0].id, valor: null }]), children: "+ concepto" }),
    !e.documento && !t && /* @__PURE__ */ l.jsx("button", { type: "button", className: "dx-enlace", onClick: () => e.alCambiar(void 0), children: "volver a los automáticos" }),
    !e.documento && t && n.length === 0 && /* @__PURE__ */ l.jsx("span", { className: "muted", children: "ninguno" })
  ] });
}
function yl(e) {
  var t;
  return (t = e.conceptos) != null && t.length ? /* @__PURE__ */ l.jsx("div", { className: "dx-aplicados", children: e.conceptos.map((n, r) => /* @__PURE__ */ l.jsxs("div", { children: [
    "· ",
    n.nombre,
    n.calculo === "Porcentaje" ? ` (${We(n.valor)} %)` : "",
    n.repartido ? " · del documento" : "",
    n.efecto === "Coste" ? " · coste" : "",
    ": ",
    /* @__PURE__ */ l.jsx("strong", { children: pt(n.importeDivisa ?? n.importe, e.moneda) })
  ] }, r)) }) : null;
}
const Tr = () => ({ clave: na(), productoId: null, descripcion: "", cantidad: 1, precio: null, dto: 0, iva: null });
function Dd(e) {
  const t = x.useRef(null), [n, r] = x.useState(/* @__PURE__ */ new Set()), a = e.modo === "venta", i = a ? 7 : 5, o = (d, m) => e.alCambiar(e.lineas.map((h) => h.clave === d ? { ...h, ...m } : h)), s = (d) => {
    const m = e.lineas.filter((h) => h.clave !== d);
    e.alCambiar(m.length ? m : [Tr()]);
  };
  function c(d, m) {
    var N;
    const h = (N = t.current) == null ? void 0 : N.querySelector(`[data-f="${d}"][data-c="${m}"]`);
    h == null || h.focus(), h instanceof HTMLInputElement && h.select();
  }
  function f(d) {
    const m = d.target, h = Number(m.dataset.f), N = Number(m.dataset.c);
    if (!(Number.isNaN(h) || Number.isNaN(N)))
      if (d.key === "Enter") {
        if (d.preventDefault(), N < i - 1) return c(h, N + 1);
        h === e.lineas.length - 1 ? (e.alCambiar([...e.lineas, Tr()]), setTimeout(() => c(h + 1, 0), 30)) : c(h + 1, 0);
      } else d.key === "ArrowDown" && m.tagName !== "SELECT" ? (d.preventDefault(), c(Math.min(h + 1, e.lineas.length - 1), N)) : d.key === "ArrowUp" && m.tagName !== "SELECT" && (d.preventDefault(), c(Math.max(h - 1, 0), N));
  }
  const j = (d) => r((m) => {
    const h = new Set(m);
    return h.has(d) ? h.delete(d) : h.add(d), h;
  });
  return /* @__PURE__ */ l.jsxs("div", { ref: t, className: "dx-rejilla", onKeyDown: f, children: [
    /* @__PURE__ */ l.jsxs("table", { children: [
      /* @__PURE__ */ l.jsx("thead", { children: /* @__PURE__ */ l.jsxs("tr", { children: [
        /* @__PURE__ */ l.jsx("th", { style: { width: "22%" }, children: "Artículo" }),
        /* @__PURE__ */ l.jsx("th", { children: "Descripción" }),
        /* @__PURE__ */ l.jsx("th", { className: "num", style: { width: 90 }, children: "Cantidad" }),
        /* @__PURE__ */ l.jsx("th", { className: "num", style: { width: 110 }, children: "Precio" }),
        a && /* @__PURE__ */ l.jsx("th", { className: "num", style: { width: 74 }, children: "Dto %" }),
        a && /* @__PURE__ */ l.jsx("th", { style: { width: 150 }, children: "Impuesto" }),
        /* @__PURE__ */ l.jsx("th", { className: "num", style: { width: 110 }, children: "Importe" }),
        /* @__PURE__ */ l.jsx("th", { className: "num", style: { width: 100 }, children: a ? "Margen" : "Coste entrada" }),
        /* @__PURE__ */ l.jsx("th", { style: { width: 70 } })
      ] }) }),
      /* @__PURE__ */ l.jsx("tbody", { children: e.lineas.map((d, m) => {
        var p;
        const h = e.calculos[m], N = (h == null ? void 0 : h.conceptos) ?? [], w = a && d.controlarStock && d.stock != null && d.cantidad > d.stock, M = h && h.margen != null && h.importe ? h.margen / h.importe * 100 : null;
        return [
          /* @__PURE__ */ l.jsxs("tr", { className: m % 2 ? "dx-par" : "", children: [
            /* @__PURE__ */ l.jsxs("td", { children: [
              e.soloLectura ? /* @__PURE__ */ l.jsx("span", { className: "mono", children: d.referencia ?? "" }) : /* @__PURE__ */ l.jsx(
                _m,
                {
                  texto: d.referencia ?? (d.productoId ? d.descripcion : ""),
                  alCambiarTexto: (u) => o(d.clave, { referencia: u, ...u === "" ? { productoId: null } : {} }),
                  alElegir: (u) => (e.alElegirArticulo(d.clave, u), c(m, 2)),
                  precioDe: a ? void 0 : (u) => u.precioCompraPorUnidadCompra ?? u.precioCompra,
                  datos: { f: m, c: 0 }
                }
              ),
              d.productoId && /* @__PURE__ */ l.jsxs("div", { className: "dx-sub", children: [
                d.unidad && /* @__PURE__ */ l.jsx("span", { children: d.unidad }),
                d.controlarStock && /* @__PURE__ */ l.jsxs("span", { className: w ? "dx-rojo" : "", children: [
                  " · stock ",
                  We(d.stock)
                ] })
              ] })
            ] }),
            /* @__PURE__ */ l.jsxs("td", { children: [
              /* @__PURE__ */ l.jsx(
                "input",
                {
                  "data-f": m,
                  "data-c": 1,
                  value: d.descripcion,
                  placeholder: d.productoId ? "" : "Descripción (línea libre)",
                  disabled: e.soloLectura,
                  onChange: (u) => o(d.clave, { descripcion: u.target.value })
                }
              ),
              N.length > 0 && !n.has(d.clave) && /* @__PURE__ */ l.jsx("button", { type: "button", className: "dx-resumen-conc", onClick: () => j(d.clave), title: "Ver y cambiar los conceptos", children: N.map((u) => `${u.importe < 0 ? "−" : "+"} ${u.codigo.toLowerCase()} ${de(Math.abs(u.importe))}${u.efecto === "Coste" ? " (coste)" : ""}`).join(" · ") })
            ] }),
            /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx(
              "input",
              {
                "data-f": m,
                "data-c": 2,
                className: "num",
                type: "number",
                step: "0.001",
                value: d.cantidad,
                disabled: e.soloLectura,
                onChange: (u) => o(d.clave, { cantidad: Number(u.target.value) })
              }
            ) }),
            /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx(
              "input",
              {
                "data-f": m,
                "data-c": 3,
                className: "num" + (d.precio == null ? " dx-auto" : ""),
                type: "number",
                step: "0.0001",
                disabled: e.soloLectura,
                value: d.precio ?? "",
                placeholder: h ? de(h.precio) : "",
                title: d.precio == null ? "Precio de la tarifa del cliente o del artículo (escribe uno para fijarlo)" : "",
                onChange: (u) => o(d.clave, { precio: u.target.value === "" ? null : Number(u.target.value) })
              }
            ) }),
            a && /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx(
              "input",
              {
                "data-f": m,
                "data-c": 4,
                className: "num",
                type: "number",
                step: "0.01",
                value: d.dto || (d.precio == null && (h != null && h.dto) ? h.dto : 0),
                disabled: e.soloLectura,
                onChange: (u) => o(d.clave, { dto: Number(u.target.value), precio: d.precio ?? (h == null ? void 0 : h.precio) ?? null })
              }
            ) }),
            a && /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsxs("select", { "data-f": m, "data-c": 5, value: d.iva ?? (h == null ? void 0 : h.iva) ?? "", disabled: e.soloLectura, onChange: (u) => o(d.clave, { iva: u.target.value || null }), children: [
              !d.iva && !(h != null && h.iva) && /* @__PURE__ */ l.jsx("option", { value: "", children: "Del artículo" }),
              e.ivas.map((u) => /* @__PURE__ */ l.jsx("option", { value: u.codigo, children: u.nombre }, u.codigo))
            ] }) }),
            /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: h ? z(h.importe) : "—" }) }),
            /* @__PURE__ */ l.jsx("td", { className: "num", children: a ? (h == null ? void 0 : h.margen) != null && /* @__PURE__ */ l.jsxs("span", { className: h.margen < 0 ? "dx-rojo" : "muted", children: [
              z(h.margen),
              M != null && /* @__PURE__ */ l.jsxs("div", { className: "dx-sub", children: [
                de(M),
                " %"
              ] })
            ] }) : (h == null ? void 0 : h.costeUnitarioEntrada) != null && /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
              z(h.costeUnitarioEntrada),
              "/ud"
            ] }) }),
            /* @__PURE__ */ l.jsxs("td", { className: "right", style: { whiteSpace: "nowrap" }, children: [
              !e.soloLectura && e.catalogo.length > 0 && /* @__PURE__ */ l.jsx("button", { type: "button", className: "dx-icono" + (n.has(d.clave) ? " activo" : ""), title: "Conceptos de la línea", onClick: () => j(d.clave), "data-f": m, "data-c": a ? 6 : 4, children: "±" }),
              !e.soloLectura && /* @__PURE__ */ l.jsx("button", { type: "button", className: "dx-icono", title: "Quitar la línea", onClick: () => s(d.clave), children: "✕" })
            ] })
          ] }, d.clave),
          n.has(d.clave) && /* @__PURE__ */ l.jsx("tr", { className: "dx-fila-conc", children: /* @__PURE__ */ l.jsxs("td", { colSpan: a ? 9 : 7, children: [
            a && !!((p = e.envases) != null && p.length) && /* @__PURE__ */ l.jsxs("label", { className: "dx-envase", children: [
              "Envase",
              " ",
              /* @__PURE__ */ l.jsxs("select", { value: d.envaseProductoId ?? "", onChange: (u) => o(d.clave, { envaseProductoId: u.target.value || null, conceptos: void 0 }), children: [
                /* @__PURE__ */ l.jsx("option", { value: "", children: "sin envase" }),
                e.envases.map((u) => /* @__PURE__ */ l.jsx("option", { value: u.id, children: u.nombre }, u.id))
              ] })
            ] }),
            /* @__PURE__ */ l.jsx(Qo, { catalogo: e.catalogo, lista: d.conceptos, sugeridos: e.sugeridos[d.clave], alCambiar: (u) => o(d.clave, { conceptos: u }) })
          ] }) }, d.clave + "c")
        ];
      }) })
    ] }),
    !e.soloLectura && /* @__PURE__ */ l.jsx("button", { type: "button", className: "btn small ghost", style: { marginTop: 8 }, onClick: () => (e.alCambiar([...e.lineas, Tr()]), setTimeout(() => c(e.lineas.length, 0), 30)), children: "+ Añadir línea" }),
    !e.soloLectura && /* @__PURE__ */ l.jsx("span", { className: "muted dx-ayuda", children: "Intro: siguiente casilla · ↑↓: cambiar de línea · F2: buscar artículo · el precio en gris es el de tarifa" })
  ] });
}
function Dm(e, t) {
  if (!e) return e;
  const n = e.toUpperCase();
  return (t === "Igic" ? { IVA21: "IGIC7", IVA10: "IGIC3", IVA4: "IGIC0", IVA0: "IGIC0", REAGP12: "REAGPIGIC", REAGP105: "REAGPIGIC" }[n] : { IGIC7: "IVA21", IGIC3: "IVA10", IGIC0: "IVA0", REAGPIGIC: "REAGP12" }[n]) ?? e;
}
const zd = ["USD", "GBP", "CHF", "JPY", "CNY", "CAD", "MXN", "BRL", "SEK", "NOK", "DKK", "PLN", "MAD"], Go = (e) => e.filter((t) => t.disponible > 0 && t.estado !== "Anulado" && !t.facturaId).sort((t, n) => t.fecha.localeCompare(n.fecha)), Ld = (e) => e.filter((t) => !!t.facturaId && (t.disponibleBase ?? 0) > 0 && t.estado !== "Anulado").sort((t, n) => t.fecha.localeCompare(n.fecha));
function Ad(e, t) {
  let n = Math.round(t * 100);
  const r = [];
  for (const a of Go(e)) {
    if (n <= 0) break;
    const i = Math.min(n, Math.round(a.disponible * 100));
    i > 0 && r.push({ id: a.id, importe: i / 100 }), n -= i;
  }
  return r;
}
const zm = { presupuesto: "presupuesto", pedido: "pedido de venta", factura: "factura" };
function $d(e) {
  const t = /* @__PURE__ */ new Map();
  for (const n of e)
    for (const r of n.conceptos ?? [])
      if (r.repartido) {
        const a = t.get(r.conceptoId);
        t.set(r.conceptoId, { conceptoId: r.conceptoId, valor: r.calculo === "Importe" ? Me(((a == null ? void 0 : a.valor) ?? 0) + r.valor) : r.valor });
      }
  return {
    porLinea: e.map((n) => (n.conceptos ?? []).filter((r) => !r.repartido).map((r) => ({ conceptoId: r.conceptoId, valor: r.valor }))),
    documento: [...t.values()]
  };
}
function Lm(e) {
  var qo, Yo, Xo, Zo, Jo, es, ts, ns;
  const { api: t, anfitrion: n } = Lt(), r = !!((qo = e.semilla) != null && qo.rectificaId), [a, i] = x.useState([]), [o, s] = x.useState([]), [c, f] = x.useState([]), [j, d] = x.useState([]), [m, h] = x.useState([]), [N, w] = x.useState([]), [M, p] = x.useState(((Yo = e.semilla) == null ? void 0 : Yo.clienteId) ?? ""), [u, v] = x.useState(e.tipo === "pedido" && ((Xo = e.semilla) != null && Xo.fecha) ? e.semilla.fecha : Et()), [E, T] = x.useState(""), [R, D] = x.useState(""), [C, $] = x.useState(0), [b, P] = x.useState(!1), [W, se] = x.useState(null), [Ue, Ve] = x.useState(30), [Ee, Re] = x.useState(""), [I, g] = x.useState([Tr()]), [L, V] = x.useState([]), [B, Y] = x.useState(!1), je = (Zo = e.semilla) != null && Zo.lineas.some((S) => /^(IGIC|REAGPIGIC)/i.test(S.codigoIva ?? "")) ? "Igic" : (Jo = e.semilla) != null && Jo.lineas.length ? "Iva" : null, [ve, ce] = x.useState(je ?? "Iva"), [Ne, st] = x.useState(((es = e.semilla) == null ? void 0 : es.moneda) ?? ""), [tt, ct] = x.useState(r ? ((ts = e.semilla) == null ? void 0 : ts.tasaCambio) ?? null : null), Be = !!Ne, [k, Pt] = x.useState(null), [ut, Ie] = x.useState(""), [yt, At] = x.useState(!1), [Gt, xe] = x.useState(!1), [qe, Ft] = x.useState(!1), [ze, _] = x.useState([]), [q, A] = x.useState(!0), [Q, ne] = x.useState([]), [y, H] = x.useState(!0), Se = ra();
  x.useEffect(() => {
    t.get("/clientes").then(i).catch(() => i([])), t.get("/tipos-iva").then((S) => s(S.filter((U) => U.activo))).catch(() => s([])), t.get("/formas-pago").then((S) => f(S.filter((U) => U.activo))).catch(() => f([])), t.get("/series").then((S) => d([...new Set(S.filter((U) => U.tipoDocumento === "Factura").map((U) => U.prefijo))])).catch(() => d([])), t.get("/conceptos-linea?ambito=Ventas&activos=true").then((S) => {
      h(S);
      const U = [...new Set(S.flatMap((G) => (G.asignaciones ?? []).map((J) => J.envaseProductoId)).filter((G) => !!G))];
      Promise.all(U.map((G) => t.get(`/productos/${G}`).then((J) => ({ id: G, nombre: J.nombre })).catch(() => ({ id: G, nombre: G })))).then(w);
    }).catch(() => h([])), t.get("/empresas/actual").then((S) => {
      Y(!!S.operaEnAmbosTerritorios), ce(je ?? (S.territorioFiscal === "Canarias" ? "Igic" : "Iva"));
    }).catch(() => Y(!1));
  }, [t]);
  function ge(S) {
    ce(S), g((U) => U.map((G) => ({ ...G, iva: G.productoId ? null : Dm(G.iva, S) })));
  }
  const ee = x.useMemo(() => B ? o.filter((S) => (S.impuesto ?? "Iva") === ve) : o, [B, o, ve]);
  x.useEffect(() => {
    const S = e.semilla;
    if (!S || !S.lineas.length) return;
    const { porLinea: U, documento: G } = $d(S.lineas), J = S.lineas.map((X, El) => ({
      clave: na(),
      productoId: X.productoId ?? null,
      descripcion: X.descripcion,
      cantidad: X.cantidad,
      precio: X.precioUnitario,
      dto: X.porcentajeDescuento,
      iva: X.codigoIva,
      envaseProductoId: X.envaseProductoId ?? null,
      conceptos: r ? [] : U[El]
    }));
    g(J), V(r ? [] : G), Promise.all(J.map((X) => X.productoId ? t.get(`/productos/${X.productoId}`).catch(() => null) : Promise.resolve(null))).then(
      (X) => g((El) => El.map((rs, Tn) => X[Tn] ? { ...rs, referencia: X[Tn].referencia ?? X[Tn].nombre, unidad: X[Tn].unidad, stock: X[Tn].stock, controlarStock: X[Tn].controlarStock } : rs))
    );
  }, [e.semilla, t, r]);
  const re = a.find((S) => S.id === M);
  x.useEffect(() => {
    if (e.tipo !== "factura" || r || !M) {
      _([]), ne([]);
      return;
    }
    t.get(`/anticipos?clienteId=${M}`).then((S) => {
      _(Go(S)), ne(Ld(S));
    }).catch(() => {
      _([]), ne([]);
    });
  }, [t, M, e.tipo, r]);
  const Sl = Me(ze.reduce((S, U) => S + U.disponible, 0));
  x.useEffect(() => {
    re && (P(!!re.recargoEquivalencia), re.formaPagoDefectoId && D(re.formaPagoDefectoId));
  }, [re]);
  const cr = x.useMemo(() => I.map((S, U) => ({ l: S, i: U })).filter(({ l: S }) => (S.productoId || S.descripcion.trim()) && S.cantidad > 0), [I]), Kt = x.useMemo(
    () => ({
      clienteId: M,
      fechaEmision: e.tipo === "factura" ? u : null,
      serie: E || null,
      diasVencimiento: C,
      formaPagoId: R || null,
      recargoEquivalencia: b,
      porcentajeIrpf: W,
      conceptosDocumento: L,
      impuesto: B && !r ? ve : null,
      descontarAnticipos: y && Q.length && !Be ? Q.map((S) => ({ anticipoId: S.id })) : null,
      moneda: Be ? Ne : null,
      tasaCambio: Be ? tt : null,
      lineas: cr.map(({ l: S }) => ({
        cantidad: S.cantidad,
        descripcion: S.descripcion.trim() || null,
        precioUnitario: S.precio,
        codigoIva: S.iva,
        porcentajeDescuento: S.dto,
        productoId: S.productoId,
        envaseProductoId: S.envaseProductoId ?? null,
        ...r ? { conceptos: [] } : S.conceptos === void 0 ? {} : { conceptos: S.conceptos }
      }))
    }),
    [M, u, E, C, R, b, W, L, cr, e.tipo, r, y, Q, B, ve, Be, Ne, tt]
  ), ur = Cn(Kt, 350);
  x.useEffect(() => {
    if (!ur.clienteId || ur.lineas.length === 0) {
      Pt(null), Ie(ur.clienteId ? "" : "Elige el cliente para calcular precios e importes.");
      return;
    }
    const S = Se();
    At(!0), t.post("/facturas/simular", ur).then((U) => S() && (Pt(U), Ie(""))).catch((U) => S() && (Pt(null), Ie(U.message))).finally(() => S() && At(!1));
  }, [ur, t]);
  const wl = x.useMemo(() => {
    const S = I.map(() => {
    });
    return k && cr.forEach(({ i: U }, G) => {
      const J = k.lineas[G];
      J && (S[U] = { precio: J.precioDivisa ?? J.precioUnitario, dto: J.porcentajeDescuento, iva: J.codigoIva, importe: J.baseDivisa ?? J.base, margen: J.productoId || J.costeUnitario || J.costeConceptos ? J.margen : void 0, conceptos: J.conceptos });
    }), S;
  }, [k, I, cr]), Md = x.useMemo(() => {
    const S = {};
    return I.forEach((U, G) => {
      var J;
      return S[U.clave] = (((J = wl[G]) == null ? void 0 : J.conceptos) ?? []).filter((X) => !X.repartido).map((X) => ({ conceptoId: X.conceptoId, valor: X.valor }));
    }), S;
  }, [I, wl]);
  function Od(S, U) {
    g(
      (G) => G.map(
        (J) => J.clave === S ? { ...J, productoId: U.id, referencia: U.referencia ?? U.nombre, descripcion: U.nombre, precio: null, dto: 0, iva: null, conceptos: void 0, unidad: U.unidad, stock: U.stock, controlarStock: U.controlarStock } : J
      )
    );
  }
  const bd = x.useMemo(() => {
    const S = /* @__PURE__ */ new Map();
    for (const U of (k == null ? void 0 : k.lineas) ?? []) {
      const G = S.get(U.codigoIva) ?? { base: 0, cuota: 0, pct: U.porcentajeIva };
      G.base += U.base, G.cuota += U.cuotaIva, S.set(U.codigoIva, G);
    }
    return [...S.entries()];
  }, [k]), Cl = ((k == null ? void 0 : k.lineas) ?? []).reduce((S, U) => S + (U.base - U.margen), 0), kl = k ? k.baseImponible - Cl : 0, Ud = (S) => {
    var U;
    return ((U = o.find((G) => G.codigo === S)) == null ? void 0 : U.nombre) ?? S;
  };
  async function Ko() {
    if (k) {
      xe(!0);
      try {
        const S = cr.map(({ l: G }, J) => {
          const X = k.lineas[J];
          return {
            cantidad: G.cantidad,
            descripcion: X.descripcion,
            // En divisa, el precio que se fija es el de la divisa (el de euros es su contravalor).
            precioUnitario: X.precioDivisa ?? X.precioUnitario,
            codigoIva: X.codigoIva,
            porcentajeDescuento: X.porcentajeDescuento,
            productoId: G.productoId,
            envaseProductoId: G.envaseProductoId ?? null,
            ...r ? {} : G.conceptos === void 0 ? {} : { conceptos: G.conceptos }
          };
        });
        let U;
        if (r)
          U = (await t.post(`/facturas/${e.semilla.rectificaId}/rectificar`, { motivo: Ee, lineas: S, fechaEmision: u, porcentajeIrpf: W, serie: E || null })).id;
        else if (e.tipo === "factura") {
          const G = await t.post("/facturas", { ...Kt, lineas: S });
          if (U = G.id, q && ze.length) {
            let J = 0;
            try {
              for (const X of Ad(ze, G.total))
                await t.post(`/anticipos/${X.id}/aplicar`, { facturaId: U, importe: X.importe }), J += X.importe;
              n.aviso(`Factura emitida. Aplicados ${z(J)} de anticipos (asiento 438 a 430).`, "ok");
            } catch (X) {
              n.aviso(`Factura emitida, pero no se pudo aplicar el anticipo: ${X.message}`, "err");
            }
            e.alGuardar(U);
            return;
          }
        } else if (e.tipo === "presupuesto") {
          const G = { clienteId: M, diasValidez: Ue, lineas: S, conceptosDocumento: Kt.conceptosDocumento, impuesto: Kt.impuesto, moneda: Kt.moneda };
          U = e.id ? (await t.put(`/presupuestos/${e.id}`, G)).id : (await t.post("/presupuestos", G)).id;
        } else {
          const G = { clienteId: M, fecha: u, lineas: S, conceptosDocumento: Kt.conceptosDocumento, impuesto: Kt.impuesto, moneda: Kt.moneda };
          U = e.id ? (await t.put(`/pedidos-venta/${e.id}`, G)).id : (await t.post("/pedidos-venta", G)).id;
        }
        n.aviso(e.tipo === "factura" ? "Factura emitida." : "Documento guardado.", "ok"), e.alGuardar(U);
      } catch (S) {
        n.aviso(S.message, "err");
      } finally {
        xe(!1), Ft(!1);
      }
    }
  }
  const Vd = r ? `Rectificativa de la factura ${((ns = e.semilla) == null ? void 0 : ns.rectificaNumero) ?? ""}` : `${e.id ? "Editar" : "Nuevo"} ${zm[e.tipo]}`.replace("Nuevo factura", "Nueva factura"), Bd = !!k && !yt && (!r || Ee.trim().length > 0);
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsxs("div", { className: "panel-head", children: [
        /* @__PURE__ */ l.jsx("h2", { children: Vd }),
        /* @__PURE__ */ l.jsxs("div", { style: { display: "flex", gap: 8 }, children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: e.alCancelar, children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", disabled: !Bd || Gt, onClick: () => e.tipo === "factura" ? Ft(!0) : Ko(), children: e.tipo === "factura" ? "Emitir factura" : "Guardar" })
        ] })
      ] }),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-cabecera", children: [
        /* @__PURE__ */ l.jsxs("div", { className: "dx-cab-campos", children: [
          /* @__PURE__ */ l.jsx(Wo, { terceros: a, valor: M, alCambiar: p, etiqueta: "Cliente", deshabilitado: r || e.tipo === "pedido" && !!e.id && !1 }),
          /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", children: [
            e.tipo !== "presupuesto" && /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: e.tipo === "factura" ? "Fecha de emisión" : "Fecha del pedido" }),
              /* @__PURE__ */ l.jsx("input", { type: "date", value: u, onChange: (S) => v(S.target.value) })
            ] }),
            e.tipo === "presupuesto" && /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Validez" }),
              /* @__PURE__ */ l.jsx("select", { value: Ue, onChange: (S) => Ve(Number(S.target.value)), children: [15, 30, 60, 90].map((S) => /* @__PURE__ */ l.jsxs("option", { value: S, children: [
                S,
                " días"
              ] }, S)) })
            ] }),
            B && !r && /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Territorio de la operación" }),
              /* @__PURE__ */ l.jsxs("select", { value: ve, onChange: (S) => ge(S.target.value), children: [
                /* @__PURE__ */ l.jsx("option", { value: "Iva", children: "Península y Baleares · IVA" }),
                /* @__PURE__ */ l.jsx("option", { value: "Igic", children: "Canarias · IGIC" })
              ] })
            ] }),
            e.tipo === "factura" && j.length > 0 && /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Serie" }),
              /* @__PURE__ */ l.jsxs("select", { value: E, onChange: (S) => T(S.target.value), children: [
                /* @__PURE__ */ l.jsx("option", { value: "", children: "La del cliente" }),
                j.map((S) => /* @__PURE__ */ l.jsx("option", { value: S, children: S }, S))
              ] })
            ] }),
            e.tipo === "factura" && !r && /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
              /* @__PURE__ */ l.jsxs("div", { children: [
                /* @__PURE__ */ l.jsx("label", { children: "Forma de pago" }),
                /* @__PURE__ */ l.jsxs("select", { value: R, onChange: (S) => D(S.target.value), children: [
                  /* @__PURE__ */ l.jsx("option", { value: "", children: "— Vencimiento a mano —" }),
                  c.map((S) => /* @__PURE__ */ l.jsx("option", { value: S.id, children: S.nombre }, S.id))
                ] })
              ] }),
              !R && /* @__PURE__ */ l.jsxs("div", { children: [
                /* @__PURE__ */ l.jsx("label", { children: "Vencimiento" }),
                /* @__PURE__ */ l.jsx("select", { value: C, onChange: (S) => $(Number(S.target.value)), children: [0, 15, 30, 45, 60, 90].map((S) => /* @__PURE__ */ l.jsx("option", { value: S, children: S ? `${S} días` : "Contado" }, S)) })
              ] })
            ] }),
            (!r || Be) && /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
              /* @__PURE__ */ l.jsxs("div", { children: [
                /* @__PURE__ */ l.jsx("label", { children: "Moneda" }),
                /* @__PURE__ */ l.jsxs("select", { value: Ne, disabled: r, title: r ? "La rectificativa va en la divisa de la original" : void 0, onChange: (S) => st(S.target.value), children: [
                  /* @__PURE__ */ l.jsx("option", { value: "", children: "EUR · euros" }),
                  zd.map((S) => /* @__PURE__ */ l.jsx("option", { value: S, children: S }, S))
                ] })
              ] }),
              Be && e.tipo === "factura" && /* @__PURE__ */ l.jsxs("div", { children: [
                /* @__PURE__ */ l.jsxs("label", { children: [
                  "Tipo de cambio (€ por 1 ",
                  Ne,
                  ")"
                ] }),
                /* @__PURE__ */ l.jsx("input", { type: "number", step: "0.000001", min: "0", value: tt ?? "", placeholder: "El del día", onChange: (S) => ct(S.target.value === "" ? null : Number(S.target.value)) })
              ] })
            ] }),
            e.tipo === "factura" && /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Retención IRPF %" }),
              /* @__PURE__ */ l.jsx("input", { type: "number", step: "0.01", value: W ?? "", placeholder: String((re == null ? void 0 : re.porcentajeIrpfDefecto) ?? 0), onChange: (S) => se(S.target.value === "" ? null : Number(S.target.value)) })
            ] })
          ] }),
          e.tipo === "factura" && !r && /* @__PURE__ */ l.jsxs("label", { className: "dx-check", children: [
            /* @__PURE__ */ l.jsx("input", { type: "checkbox", checked: b, onChange: (S) => P(S.target.checked) }),
            " Recargo de equivalencia"
          ] }),
          r && /* @__PURE__ */ l.jsxs("div", { children: [
            /* @__PURE__ */ l.jsx("label", { children: "Motivo de la rectificación (obligatorio)" }),
            /* @__PURE__ */ l.jsx("input", { value: Ee, onChange: (S) => Re(S.target.value), placeholder: "Devolución, error en precio…" })
          ] })
        ] }),
        /* @__PURE__ */ l.jsx("div", { className: "dx-ficha", children: re ? /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("strong", { children: re.nombre }),
          /* @__PURE__ */ l.jsx("div", { className: "muted", children: [re.nifFiscal, re.poblacion, re.provincia].filter(Boolean).join(" · ") }),
          re.limiteRiesgo != null && /* @__PURE__ */ l.jsxs("div", { className: "muted", children: [
            "Límite de riesgo ",
            z(re.limiteRiesgo)
          ] }),
          re.tarifaId && /* @__PURE__ */ l.jsx("div", { className: "muted", children: "Con tarifa de precios propia" }),
          re.recargoEquivalencia && /* @__PURE__ */ l.jsx("div", { className: "muted", children: "En recargo de equivalencia" }),
          (k == null ? void 0 : k.avisoRiesgo) && /* @__PURE__ */ l.jsxs("div", { className: "dx-aviso", children: [
            "⚠ ",
            k.avisoRiesgo
          ] }),
          Q.length > 0 && /* @__PURE__ */ l.jsxs("div", { className: "dx-anticipo", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              "🧾 Anticipos facturados pendientes de descontar: ",
              /* @__PURE__ */ l.jsx("strong", { children: z(Q.reduce((S, U) => S + (U.disponibleBase ?? 0), 0)) }),
              " de base (",
              Q.map((S) => S.facturaNumero).join(", "),
              ")."
            ] }),
            /* @__PURE__ */ l.jsxs("label", { className: "dx-check", children: [
              /* @__PURE__ */ l.jsx("input", { type: "checkbox", checked: y, onChange: (S) => H(S.target.checked) }),
              "Descontar en esta factura (línea negativa con su base e IVA; hasta la base de la factura)"
            ] })
          ] }),
          Sl > 0 && /* @__PURE__ */ l.jsxs("div", { className: "dx-anticipo", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              "💶 Tiene ",
              /* @__PURE__ */ l.jsx("strong", { children: z(Sl) }),
              " en ",
              ze.length === 1 ? "un anticipo pendiente" : `${ze.length} anticipos pendientes`,
              " de aplicar."
            ] }),
            /* @__PURE__ */ l.jsxs("label", { className: "dx-check", children: [
              /* @__PURE__ */ l.jsx("input", { type: "checkbox", checked: q, onChange: (S) => A(S.target.checked) }),
              "Aplicarlo al emitir",
              k ? ` (${z(Math.min(Sl, k.total))})` : ""
            ] })
          ] })
        ] }) : /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Elige un cliente: se aplican su tarifa, su forma de pago, su recargo y sus conceptos." }) })
      ] })
    ] }),
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsx(
        Dd,
        {
          modo: "venta",
          lineas: I,
          alCambiar: g,
          calculos: wl,
          ivas: ee,
          catalogo: r ? [] : m,
          sugeridos: Md,
          alElegirArticulo: Od,
          envases: N
        }
      ),
      !r && m.length > 0 && /* @__PURE__ */ l.jsx("div", { style: { marginTop: 10 }, children: /* @__PURE__ */ l.jsx(Qo, { catalogo: m, lista: L, alCambiar: (S) => V(S ?? []), documento: !0 }) })
    ] }),
    /* @__PURE__ */ l.jsxs("div", { className: "dx-pie", children: [
      /* @__PURE__ */ l.jsxs("div", { className: "dx-estado", children: [
        yt && /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Calculando…" }),
        !yt && ut && /* @__PURE__ */ l.jsx("span", { className: "dx-rojo", children: ut }),
        (k == null ? void 0 : k.mencionFiscal) && /* @__PURE__ */ l.jsx("div", { className: "muted", style: { fontSize: 12 }, children: k.mencionFiscal })
      ] }),
      /* @__PURE__ */ l.jsxs("div", { className: "panel dx-totales", children: [
        bd.map(([S, U]) => /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
            Ud(S),
            " · base ",
            de(U.base)
          ] }),
          /* @__PURE__ */ l.jsx("span", { children: z(U.cuota) })
        ] }, S)),
        k == null ? void 0 : k.lineas.filter((S) => S.anticipoId).map((S) => /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-tot-anticipo", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: S.descripcion }),
          /* @__PURE__ */ l.jsx("span", { children: z(S.base + S.cuotaIva + S.cuotaRecargo) })
        ] }, S.anticipoId)),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Base imponible" }),
          /* @__PURE__ */ l.jsx("span", { children: z(k == null ? void 0 : k.baseImponible) })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Impuestos" }),
          /* @__PURE__ */ l.jsx("span", { children: z(k == null ? void 0 : k.cuotaIva) })
        ] }),
        !!(k != null && k.recargoTotal) && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Recargo de equivalencia" }),
          /* @__PURE__ */ l.jsx("span", { children: z(k.recargoTotal) })
        ] }),
        !!(k != null && k.retencionIrpf) && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
            "Retención IRPF (",
            de(k.porcentajeIrpf),
            " %)"
          ] }),
          /* @__PURE__ */ l.jsxs("span", { children: [
            "−",
            z(k.retencionIrpf)
          ] })
        ] }),
        k != null && k.moneda ? /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
            /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
              "Contravalor en euros (1 ",
              k.moneda,
              " = ",
              String(k.tasaCambio ?? 0).replace(".", ","),
              " €)"
            ] }),
            /* @__PURE__ */ l.jsx("span", { children: z(k.total) })
          ] }),
          /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-grande", children: [
            /* @__PURE__ */ l.jsxs("span", { children: [
              "Total ",
              k.moneda
            ] }),
            /* @__PURE__ */ l.jsxs("span", { children: [
              de(k.totalDivisa ?? 0),
              " ",
              k.moneda
            ] })
          ] })
        ] }) : /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ l.jsx("span", { children: "Total" }),
          /* @__PURE__ */ l.jsx("span", { children: z(k == null ? void 0 : k.total) })
        ] }),
        k && Cl > 0 && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Coste · margen" }),
          /* @__PURE__ */ l.jsxs("span", { className: kl < 0 ? "dx-rojo" : "muted", children: [
            z(Cl),
            " · ",
            z(kl),
            " (",
            de(k.baseImponible ? kl / k.baseImponible * 100 : 0),
            " %)"
          ] })
        ] })
      ] })
    ] }),
    qe && k && /* @__PURE__ */ l.jsx(
      pn,
      {
        titulo: r ? "Emitir la rectificativa" : "Emitir la factura",
        alCerrar: () => Ft(!1),
        acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => Ft(!1), children: "Revisar" }),
          /* @__PURE__ */ l.jsxs("button", { className: "btn small", disabled: Gt, onClick: Ko, children: [
            "Emitir ",
            k.moneda ? `${de(k.totalDivisa ?? 0)} ${k.moneda}` : z(k.total)
          ] })
        ] }),
        children: /* @__PURE__ */ l.jsxs("p", { style: { margin: 0 }, children: [
          "Se emitirá una factura ",
          r ? "rectificativa " : "",
          "de ",
          /* @__PURE__ */ l.jsx("strong", { children: k.moneda ? `${de(k.totalDivisa ?? 0)} ${k.moneda} (${z(k.total)})` : z(k.total) }),
          " a ",
          /* @__PURE__ */ l.jsx("strong", { children: re == null ? void 0 : re.nombre }),
          " con fecha ",
          u.split("-").reverse().join("/"),
          ". Una factura emitida no se modifica: queda numerada y encadenada en VeriFactu; para corregirla hay que rectificarla o anularla."
        ] })
      }
    )
  ] });
}
function Am(e) {
  var Ve, Ee, Re;
  const { api: t, anfitrion: n } = Lt(), [r, a] = x.useState([]), [i, o] = x.useState([]), [s, c] = x.useState(((Ve = e.semilla) == null ? void 0 : Ve.proveedorId) ?? ""), [f, j] = x.useState(((Ee = e.semilla) == null ? void 0 : Ee.fecha) ?? Et()), [d, m] = x.useState([Tr()]), [h, N] = x.useState([]), [w, M] = x.useState(null), [p, u] = x.useState(""), [v, E] = x.useState(!1), T = ra();
  x.useEffect(() => {
    t.get("/proveedores").then(a).catch(() => a([])), t.get("/conceptos-linea?ambito=Compras&activos=true").then(o).catch(() => o([]));
  }, [t]), x.useEffect(() => {
    const I = e.semilla;
    if (!I) return;
    const g = I.lineas.map((Y) => ({ ...Y, porcentajeDescuento: 0, codigoIva: "" })), { porLinea: L, documento: V } = $d(g), B = I.lineas.map((Y, je) => ({ clave: na(), productoId: Y.productoId ?? null, descripcion: Y.descripcion, cantidad: Y.cantidad, precio: Y.precioUnitario, dto: 0, iva: null, conceptos: L[je] }));
    m(B), N(V), Promise.all(B.map((Y) => Y.productoId ? t.get(`/productos/${Y.productoId}`).catch(() => null) : Promise.resolve(null))).then(
      (Y) => m((je) => je.map((ve, ce) => Y[ce] ? { ...ve, referencia: Y[ce].referencia ?? Y[ce].nombre, unidad: Y[ce].unidadCompra || Y[ce].unidad, stock: Y[ce].stock, controlarStock: Y[ce].controlarStock } : ve))
    );
  }, [e.semilla, t]);
  const R = r.find((I) => I.id === s), D = x.useMemo(() => d.map((I, g) => ({ l: I, i: g })).filter(({ l: I }) => I.descripcion.trim() && I.cantidad > 0), [d]), C = x.useMemo(
    () => {
      var I, g;
      return {
        proveedorId: s || null,
        proveedorTexto: (R == null ? void 0 : R.nombre) ?? (((I = e.semilla) == null ? void 0 : I.proveedorTexto) || null),
        solicitudOrigenId: e.id ? null : ((g = e.semilla) == null ? void 0 : g.solicitudOrigenId) ?? null,
        fecha: f,
        conceptosDocumento: h,
        lineas: D.map(({ l: L }) => ({ descripcion: L.descripcion.trim(), cantidad: L.cantidad, precioUnitario: L.precio ?? 0, productoId: L.productoId, ...L.conceptos === void 0 ? {} : { conceptos: L.conceptos } }))
      };
    },
    [s, R, f, h, D, e.id, e.semilla]
  ), $ = Cn(C, 350);
  x.useEffect(() => {
    if (!$.proveedorId || $.lineas.length === 0) {
      M(null), u($.proveedorId ? "" : "Elige el proveedor.");
      return;
    }
    const I = T();
    t.post("/compras/pedidos/simular", $).then((g) => I() && (M(g), u(""))).catch((g) => I() && (M(null), u(g.message)));
  }, [$, t]);
  const b = x.useMemo(() => {
    const I = d.map(() => {
    });
    return D.forEach(({ i: g }, L) => {
      const V = w == null ? void 0 : w.lineas[L];
      V && (I[g] = { precio: V.precioUnitario, importe: V.importe, costeUnitarioEntrada: V.costeUnitarioEntrada, conceptos: V.conceptos });
    }), I;
  }, [w, d, D]), P = x.useMemo(() => {
    const I = {};
    return d.forEach((g, L) => {
      var V;
      return I[g.clave] = (((V = b[L]) == null ? void 0 : V.conceptos) ?? []).filter((B) => !B.repartido).map((B) => ({ conceptoId: B.conceptoId, valor: B.valor }));
    }), I;
  }, [d, b]);
  function W(I, g) {
    const L = g.precioCompraPorUnidadCompra ?? g.precioCompra;
    m((V) => V.map((B) => B.clave === I ? { ...B, productoId: g.id, referencia: g.referencia ?? g.nombre, descripcion: g.nombre, precio: L, conceptos: void 0, unidad: g.unidadCompra || g.unidad, stock: g.stock, controlarStock: g.controlarStock } : B));
  }
  async function se() {
    E(!0);
    try {
      const I = e.id ? await t.put(`/compras/pedidos/${e.id}`, C) : await t.post("/compras/pedidos", C);
      n.aviso("Pedido de compra guardado.", "ok"), e.alGuardar(I.id);
    } catch (I) {
      n.aviso(I.message, "err");
    } finally {
      E(!1);
    }
  }
  const Ue = ((w == null ? void 0 : w.lineas) ?? []).reduce((I, g) => I + g.costeConceptos, 0);
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsxs("div", { className: "panel-head", children: [
        /* @__PURE__ */ l.jsx("h2", { children: e.id ? `Editar pedido ${((Re = e.semilla) == null ? void 0 : Re.numeroCompleto) ?? ""}` : "Nuevo pedido de compra" }),
        /* @__PURE__ */ l.jsxs("div", { style: { display: "flex", gap: 8 }, children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: e.alCancelar, children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", disabled: !w || v, onClick: se, children: "Guardar" })
        ] })
      ] }),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-cabecera", children: [
        /* @__PURE__ */ l.jsxs("div", { className: "dx-cab-campos", children: [
          /* @__PURE__ */ l.jsx(Wo, { terceros: r, valor: s, alCambiar: c, etiqueta: "Proveedor", deshabilitado: !!e.id }),
          /* @__PURE__ */ l.jsx("div", { className: "dx-fila", children: /* @__PURE__ */ l.jsxs("div", { children: [
            /* @__PURE__ */ l.jsx("label", { children: "Fecha del pedido" }),
            /* @__PURE__ */ l.jsx("input", { type: "date", value: f, onChange: (I) => j(I.target.value) })
          ] }) })
        ] }),
        /* @__PURE__ */ l.jsx("div", { className: "dx-ficha", children: R ? /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("strong", { children: R.nombre }),
          /* @__PURE__ */ l.jsx("div", { className: "muted", children: [R.nifFiscal, R.poblacion, R.pais].filter(Boolean).join(" · ") })
        ] }) : /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Elige un proveedor: se aplican sus conceptos (pronto pago, portes…)." }) })
      ] })
    ] }),
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsx(Dd, { modo: "compra", lineas: d, alCambiar: m, calculos: b, ivas: [], catalogo: i, sugeridos: P, alElegirArticulo: W }),
      i.length > 0 && /* @__PURE__ */ l.jsx("div", { style: { marginTop: 10 }, children: /* @__PURE__ */ l.jsx(Qo, { catalogo: i, lista: h, alCambiar: (I) => N(I ?? []), documento: !0 }) })
    ] }),
    /* @__PURE__ */ l.jsxs("div", { className: "dx-pie", children: [
      /* @__PURE__ */ l.jsx("div", { className: "dx-estado", children: p && /* @__PURE__ */ l.jsx("span", { className: "dx-rojo", children: p }) }),
      /* @__PURE__ */ l.jsxs("div", { className: "panel dx-totales", children: [
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ l.jsx("span", { children: "Total del pedido (sin impuestos)" }),
          /* @__PURE__ */ l.jsx("span", { children: z(w == null ? void 0 : w.total) })
        ] }),
        Ue !== 0 && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Costes añadidos al almacén" }),
          /* @__PURE__ */ l.jsx("span", { children: z(Ue) })
        ] }),
        /* @__PURE__ */ l.jsx("div", { className: "muted", style: { fontSize: 12, marginTop: 6 }, children: "El impuesto se aplica al facturar el pedido." })
      ] })
    ] })
  ] });
}
function jl(e) {
  return /* @__PURE__ */ l.jsxs("div", { className: "panel-head", children: [
    /* @__PURE__ */ l.jsxs("h2", { style: { display: "flex", alignItems: "center", gap: 10 }, children: [
      /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: e.volver, title: "Volver a la lista", children: "←" }),
      e.titulo,
      e.estado && /* @__PURE__ */ l.jsx("span", { className: Ho(e.estado), children: e.estado }),
      e.extra
    ] }),
    /* @__PURE__ */ l.jsx("div", { className: "dx-acciones", children: e.acciones })
  ] });
}
function Oe(e) {
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-dato", children: [
    /* @__PURE__ */ l.jsx("small", { children: e.etiqueta }),
    /* @__PURE__ */ l.jsx("div", { children: e.children })
  ] });
}
function Zn(e) {
  return /* @__PURE__ */ l.jsx("button", { type: "button", className: "dx-enlace", onClick: e.alPulsar, children: e.children });
}
function Nl(e) {
  const [t, n] = x.useState(null), [r, a] = x.useState(""), i = x.useCallback(() => {
    e().then(n).catch((o) => a(o.message));
  }, []);
  return x.useEffect(i, [i]), { dato: t, error: r, recargar: i };
}
async function mt(e, t, n) {
  try {
    return await e(), n && t(n, "ok"), !0;
  } catch (r) {
    return t(r.message, "err"), !1;
  }
}
function $m(e) {
  const { api: t, anfitrion: n, navegar: r } = Lt(), { dato: a, error: i, recargar: o } = Nl(() => t.get(`/facturas/${e.id}`)), [s, c] = x.useState(null), [f, j] = x.useState(!1), [d, m] = x.useState("");
  x.useEffect(() => void t.get(`/facturas/${e.id}/saldo`).then(c).catch(() => c(null)), [t, e.id, a]);
  const [h, N] = x.useState([]), [w, M] = x.useState([]), [p, u] = x.useState(null);
  x.useEffect(() => {
    !(a != null && a.clienteId) || a.estado !== "Emitida" || t.get(`/anticipos?clienteId=${a.clienteId}`).then((C) => {
      N(Go(C)), M(Ld(C));
    }).catch(() => N([]));
  }, [t, a]);
  const v = h.reduce((C, $) => C + $.disponible, 0);
  if (i) return /* @__PURE__ */ l.jsx("div", { className: "panel", children: /* @__PURE__ */ l.jsx("p", { className: "dx-rojo", children: i }) });
  if (!a) return /* @__PURE__ */ l.jsx("div", { className: "muted", children: "Cargando…" });
  const E = { clienteId: a.clienteId ?? void 0, lineas: a.lineas.map((C) => ({ ...C, precioUnitario: C.precioDivisa ?? C.precioUnitario })), moneda: a.moneda, tasaCambio: a.tasaCambio }, T = a.lineas.reduce((C, $) => C + ($.base - $.margen), 0), R = a.estado === "Emitida", D = a.lineas.some((C) => C.cuentaContable === "438" && !C.anticipoId);
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsx(
        jl,
        {
          titulo: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            a.tipo === "Rectificativa" ? "Rectificativa" : a.tipo === "Simplificada" ? "Ticket" : "Factura",
            " ",
            /* @__PURE__ */ l.jsx("span", { className: "mono", children: a.numeroCompleto })
          ] }),
          estado: a.estado,
          extra: D ? /* @__PURE__ */ l.jsx("span", { className: "pill part", children: "Factura de anticipo" }) : a.lineas.some((C) => C.anticipoId) ? /* @__PURE__ */ l.jsx("span", { className: "pill", children: "Descuenta anticipos" }) : null,
          volver: () => r({ tipo: "factura", pantalla: "lista" }),
          acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => Yr(n, `/facturas/${a.id}/pdf`).catch((C) => n.aviso(C.message, "err")), children: "PDF" }),
            a.tipo !== "Simplificada" && a.clienteNif && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => Yr(n, `/facturas/${a.id}/facturae.xml`).catch((C) => n.aviso(C.message, "err")), children: "Facturae" }),
            a.estado !== "Borrador" && a.tipo !== "Simplificada" && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", title: "Factura EDIFACT INVOIC (EANCOM) para clientes con EDI", onClick: () => jm(n, `/integraciones/edi/facturas/${a.id}/invoic`).catch((C) => n.aviso(C.message, "err")), children: "EDI" }),
            /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "factura", pantalla: "editor", semilla: E }), children: "Duplicar" }),
            R && a.tipo === "Ordinaria" && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "factura", pantalla: "editor", semilla: { ...E, rectificaId: a.id, rectificaNumero: a.numeroCompleto } }), children: "Rectificar" }),
            R && /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", onClick: () => j(!0), children: "Anular" })
          ] })
        }
      ),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ l.jsxs(Oe, { etiqueta: "Cliente", children: [
          /* @__PURE__ */ l.jsx("strong", { children: a.clienteNombre }),
          a.clienteNif && /* @__PURE__ */ l.jsx("div", { className: "muted mono", children: a.clienteNif }),
          /* @__PURE__ */ l.jsx("div", { className: "muted", children: [a.clienteCalle, a.clienteCodigoPostal, a.clientePoblacion].filter(Boolean).join(" ") })
        ] }),
        /* @__PURE__ */ l.jsxs(Oe, { etiqueta: "Emisión", children: [
          Ce(a.fechaEmision),
          a.fechaOperacion !== a.fechaEmision && /* @__PURE__ */ l.jsxs("div", { className: "muted", children: [
            "Operación ",
            Ce(a.fechaOperacion)
          ] })
        ] }),
        /* @__PURE__ */ l.jsx(Oe, { etiqueta: "Vencimiento", children: Ce(a.fechaVencimiento) }),
        /* @__PURE__ */ l.jsxs(Oe, { etiqueta: "Cobro", children: [
          s ? s.pendiente <= 0 ? /* @__PURE__ */ l.jsx("span", { className: "pill ok", children: "Cobrada" }) : /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            "Pendiente ",
            /* @__PURE__ */ l.jsx("strong", { children: z(s.pendiente) }),
            s.liquidado > 0 && /* @__PURE__ */ l.jsxs("div", { className: "muted", children: [
              "Cobrado ",
              z(s.liquidado)
            ] })
          ] }) : "—",
          s && s.pendiente > 0 && R && n.irA && /* @__PURE__ */ l.jsx("div", { children: /* @__PURE__ */ l.jsx(Zn, { alPulsar: () => n.irA("cobros"), children: "Registrar cobro" }) }),
          s && s.pendiente > 0 && R && w.length > 0 && !D && /* @__PURE__ */ l.jsxs("div", { className: "dx-anticipo", children: [
            "🧾 Anticipos facturados sin descontar: ",
            /* @__PURE__ */ l.jsx("strong", { children: z(w.reduce((C, $) => C + ($.disponibleBase ?? 0), 0)) }),
            " de base. Se descuentan al hacer la siguiente factura (o rectifica esta para incluirlos)."
          ] }),
          s && s.pendiente > 0 && R && v > 0 && /* @__PURE__ */ l.jsxs("div", { className: "dx-anticipo", children: [
            "💶 Anticipos del cliente sin aplicar: ",
            /* @__PURE__ */ l.jsx("strong", { children: z(v) }),
            /* @__PURE__ */ l.jsx("div", { children: /* @__PURE__ */ l.jsx(Zn, { alPulsar: () => u(Ad(h, s.pendiente)), children: "Aplicar a esta factura" }) })
          ] })
        ] })
      ] }),
      a.motivoRectificacion && /* @__PURE__ */ l.jsxs("p", { className: "muted", children: [
        "Rectifica: ",
        a.motivoRectificacion,
        a.rectificaFacturaId && /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          " · ",
          /* @__PURE__ */ l.jsx(Zn, { alPulsar: () => r({ tipo: "factura", pantalla: "vista", id: a.rectificaFacturaId }), children: "ver la factura original" })
        ] })
      ] }),
      a.motivoAnulacion && /* @__PURE__ */ l.jsxs("p", { className: "dx-rojo", children: [
        "Anulada: ",
        a.motivoAnulacion
      ] })
    ] }),
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsxs("table", { children: [
        /* @__PURE__ */ l.jsx("thead", { children: /* @__PURE__ */ l.jsxs("tr", { children: [
          /* @__PURE__ */ l.jsx("th", { children: "Descripción" }),
          /* @__PURE__ */ l.jsx("th", { className: "num", children: "Cantidad" }),
          /* @__PURE__ */ l.jsx("th", { className: "num", children: "Precio" }),
          /* @__PURE__ */ l.jsx("th", { className: "num", children: "Dto" }),
          /* @__PURE__ */ l.jsx("th", { children: "Impuesto" }),
          /* @__PURE__ */ l.jsx("th", { className: "num", children: "Importe" }),
          /* @__PURE__ */ l.jsx("th", { className: "num", children: "Margen" })
        ] }) }),
        /* @__PURE__ */ l.jsx("tbody", { children: a.lineas.map((C, $) => /* @__PURE__ */ l.jsxs("tr", { children: [
          /* @__PURE__ */ l.jsxs("td", { children: [
            C.descripcion,
            /* @__PURE__ */ l.jsx(yl, { conceptos: C.conceptos, moneda: a.moneda })
          ] }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: We(C.cantidad) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: pt(C.precioDivisa ?? C.precioUnitario, a.moneda) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: C.porcentajeDescuento ? `${de(C.porcentajeDescuento)} %` : "" }),
          /* @__PURE__ */ l.jsxs("td", { children: [
            C.codigoIva,
            " · ",
            de(C.porcentajeIva),
            " %"
          ] }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: pt(C.baseDivisa ?? C.base, a.moneda) }) }),
          /* @__PURE__ */ l.jsx("td", { className: "num muted", children: C.costeUnitario || C.costeConceptos ? z(C.margen) : "" })
        ] }, $)) })
      ] }),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-totales-vista", children: [
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Base imponible" }),
          /* @__PURE__ */ l.jsx("span", { children: z(a.baseImponible) })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: a.impuesto === "Igic" ? "IGIC" : "IVA" }),
          /* @__PURE__ */ l.jsx("span", { children: z(a.cuotaIva) })
        ] }),
        !!a.recargoTotal && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Recargo de equivalencia" }),
          /* @__PURE__ */ l.jsx("span", { children: z(a.recargoTotal) })
        ] }),
        !!a.retencionIrpf && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
            "Retención IRPF (",
            de(a.porcentajeIrpf),
            " %)"
          ] }),
          /* @__PURE__ */ l.jsxs("span", { children: [
            "−",
            z(a.retencionIrpf)
          ] })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: a.moneda ? "dx-tot" : "dx-tot dx-grande", children: [
          /* @__PURE__ */ l.jsx("span", { children: a.moneda ? `Total en euros (1 ${a.moneda} = ${String(a.tasaCambio ?? 0).replace(".", ",")} €)` : "Total" }),
          /* @__PURE__ */ l.jsx("span", { children: z(a.total) })
        ] }),
        a.moneda && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ l.jsxs("span", { children: [
            "Total ",
            a.moneda
          ] }),
          /* @__PURE__ */ l.jsx("span", { children: pt(a.totalDivisa, a.moneda) })
        ] }),
        T > 0 && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Margen" }),
          /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
            z(a.baseImponible - T),
            " (",
            de(a.baseImponible ? (a.baseImponible - T) / a.baseImponible * 100 : 0),
            " %)"
          ] })
        ] })
      ] }),
      a.mencionFiscal && /* @__PURE__ */ l.jsx("p", { className: "muted", style: { fontSize: 12 }, children: a.mencionFiscal }),
      a.huella && /* @__PURE__ */ l.jsxs("p", { className: "muted mono", style: { fontSize: 11, wordBreak: "break-all" }, children: [
        "VeriFactu · ",
        a.huella
      ] })
    ] }),
    p && /* @__PURE__ */ l.jsxs(
      pn,
      {
        titulo: `Aplicar anticipos a ${a.numeroCompleto}`,
        alCerrar: () => u(null),
        acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => u(null), children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", disabled: !p.some((C) => C.importe > 0), onClick: async () => {
            for (const C of p.filter(($) => $.importe > 0))
              if (!await mt(() => t.post(`/anticipos/${C.id}/aplicar`, { facturaId: a.id, importe: C.importe }), n.aviso, "Anticipo aplicado.")) return;
            u(null), o();
          }, children: "Aplicar" })
        ] }),
        children: [
          /* @__PURE__ */ l.jsxs("p", { className: "muted", style: { marginTop: 0 }, children: [
            "Se registra el cobro de la factura con el anticipo y su asiento de cancelación: 438 Anticipos de clientes al debe, 430 Clientes al haber. Pendiente de la factura: ",
            /* @__PURE__ */ l.jsx("strong", { children: z(s == null ? void 0 : s.pendiente) }),
            "."
          ] }),
          /* @__PURE__ */ l.jsxs("table", { children: [
            /* @__PURE__ */ l.jsx("thead", { children: /* @__PURE__ */ l.jsxs("tr", { children: [
              /* @__PURE__ */ l.jsx("th", { children: "Anticipo" }),
              /* @__PURE__ */ l.jsx("th", { children: "Concepto" }),
              /* @__PURE__ */ l.jsx("th", { className: "num", children: "Disponible" }),
              /* @__PURE__ */ l.jsx("th", { className: "num", children: "Aplicar" })
            ] }) }),
            /* @__PURE__ */ l.jsx("tbody", { children: h.map((C) => {
              const $ = p.find((b) => b.id === C.id);
              return /* @__PURE__ */ l.jsxs("tr", { children: [
                /* @__PURE__ */ l.jsx("td", { children: Ce(C.fecha) }),
                /* @__PURE__ */ l.jsx("td", { className: "muted", children: C.concepto }),
                /* @__PURE__ */ l.jsx("td", { className: "num", children: z(C.disponible) }),
                /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx(
                  "input",
                  {
                    type: "number",
                    step: "0.01",
                    min: 0,
                    max: C.disponible,
                    style: { width: 110, textAlign: "right" },
                    value: ($ == null ? void 0 : $.importe) ?? 0,
                    onChange: (b) => {
                      const P = Math.max(0, Math.min(C.disponible, Number(b.target.value) || 0));
                      u([...p.filter((W) => W.id !== C.id), { id: C.id, importe: P }]);
                    }
                  }
                ) })
              ] }, C.id);
            }) })
          ] })
        ]
      }
    ),
    f && /* @__PURE__ */ l.jsxs(
      pn,
      {
        titulo: `Anular ${a.numeroCompleto}`,
        alCerrar: () => j(!1),
        acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => j(!1), children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", disabled: !d.trim(), onClick: async () => await mt(() => t.post(`/facturas/${a.id}/anular`, { motivo: d }), n.aviso, "Factura anulada.") && (j(!1), o()), children: "Anular" })
        ] }),
        children: [
          /* @__PURE__ */ l.jsx("p", { className: "muted", style: { marginTop: 0 }, children: "La factura no se borra: queda anulada con un registro de anulación VeriFactu. Si hay que corregir importes, rectifícala en lugar de anularla." }),
          /* @__PURE__ */ l.jsx("label", { children: "Motivo" }),
          /* @__PURE__ */ l.jsx("input", { value: d, onChange: (C) => m(C.target.value), autoFocus: !0 })
        ]
      }
    )
  ] });
}
function Mm(e) {
  const { api: t, anfitrion: n, navegar: r } = Lt(), { dato: a, error: i, recargar: o } = Nl(() => t.get(`/presupuestos/${e.id}`));
  if (i) return /* @__PURE__ */ l.jsx("div", { className: "panel", children: /* @__PURE__ */ l.jsx("p", { className: "dx-rojo", children: i }) });
  if (!a) return /* @__PURE__ */ l.jsx("div", { className: "muted", children: "Cargando…" });
  const s = a.estado === "Borrador", c = { clienteId: a.clienteId, lineas: a.lineas, moneda: a.moneda };
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsx(
        jl,
        {
          titulo: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            "Presupuesto ",
            /* @__PURE__ */ l.jsx("span", { className: "mono", children: a.numeroCompleto })
          ] }),
          estado: a.estado,
          volver: () => r({ tipo: "presupuesto", pantalla: "lista" }),
          acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => Yr(n, `/presupuestos/${a.id}/pdf`).catch((f) => n.aviso(f.message, "err")), children: "PDF" }),
            /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "presupuesto", pantalla: "editor", semilla: c }), children: "Duplicar" }),
            s && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "presupuesto", pantalla: "editor", id: a.id, semilla: c }), children: "Editar" }),
            s && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: async () => {
              try {
                const f = await t.post("/pedidos-venta/desde-presupuesto", { presupuestoId: a.id });
                n.aviso("Pedido creado.", "ok"), r({ tipo: "pedido", pantalla: "vista", id: f.id });
              } catch (f) {
                n.aviso(f.message, "err");
              }
            }, children: "Pasar a pedido" }),
            s && /* @__PURE__ */ l.jsx("button", { className: "btn small", onClick: async () => {
              try {
                const f = await t.post(`/presupuestos/${a.id}/aceptar`, {});
                n.aviso("Factura emitida.", "ok"), r({ tipo: "factura", pantalla: "vista", id: f.id });
              } catch (f) {
                n.aviso(f.message, "err");
              }
            }, children: "Aceptar y facturar" }),
            s && /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", onClick: async () => await mt(() => t.post(`/presupuestos/${a.id}/rechazar`, {}), n.aviso, "Presupuesto rechazado.") && o(), children: "Rechazar" })
          ] })
        }
      ),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ l.jsx(Oe, { etiqueta: "Cliente", children: /* @__PURE__ */ l.jsx("strong", { children: a.clienteNombre }) }),
        /* @__PURE__ */ l.jsx(Oe, { etiqueta: "Fecha", children: Ce(a.fecha) }),
        /* @__PURE__ */ l.jsx(Oe, { etiqueta: "Válido hasta", children: Ce(a.validez) }),
        /* @__PURE__ */ l.jsx(Oe, { etiqueta: "Factura", children: a.facturaId ? /* @__PURE__ */ l.jsx(Zn, { alPulsar: () => r({ tipo: "factura", pantalla: "vista", id: a.facturaId }), children: "ver la factura" }) : "—" })
      ] })
    ] }),
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsxs("table", { children: [
        /* @__PURE__ */ l.jsx("thead", { children: /* @__PURE__ */ l.jsxs("tr", { children: [
          /* @__PURE__ */ l.jsx("th", { children: "Descripción" }),
          /* @__PURE__ */ l.jsx("th", { className: "num", children: "Cantidad" }),
          /* @__PURE__ */ l.jsx("th", { className: "num", children: "Precio" }),
          /* @__PURE__ */ l.jsx("th", { className: "num", children: "Dto" }),
          /* @__PURE__ */ l.jsx("th", { children: "Impuesto" }),
          /* @__PURE__ */ l.jsx("th", { className: "num", children: "Importe" })
        ] }) }),
        /* @__PURE__ */ l.jsx("tbody", { children: a.lineas.map((f, j) => /* @__PURE__ */ l.jsxs("tr", { children: [
          /* @__PURE__ */ l.jsxs("td", { children: [
            f.descripcion,
            /* @__PURE__ */ l.jsx(yl, { conceptos: f.conceptos, moneda: a.moneda })
          ] }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: We(f.cantidad) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: pt(f.precioUnitario, a.moneda) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: f.porcentajeDescuento ? `${de(f.porcentajeDescuento)} %` : "" }),
          /* @__PURE__ */ l.jsx("td", { children: f.codigoIva }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: pt(f.base, a.moneda) }) })
        ] }, j)) })
      ] }),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-totales-vista", children: [
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Base imponible" }),
          /* @__PURE__ */ l.jsx("span", { children: pt(a.baseImponible, a.moneda) })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Impuestos" }),
          /* @__PURE__ */ l.jsx("span", { children: pt(a.cuotaIva, a.moneda) })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ l.jsx("span", { children: "Total" }),
          /* @__PURE__ */ l.jsx("span", { children: pt(a.total, a.moneda) })
        ] })
      ] })
    ] })
  ] });
}
function Om(e) {
  const { api: t, anfitrion: n, navegar: r } = Lt(), { dato: a, error: i, recargar: o } = Nl(() => t.get(`/pedidos-venta/${e.id}`)), [s, c] = x.useState([]), [f, j] = x.useState([]), [d, m] = x.useState(null), [h, N] = x.useState(""), [w, M] = x.useState(Et()), [p, u] = x.useState(!1), [v, E] = x.useState(Et()), [T, R] = x.useState("");
  if (x.useEffect(() => void t.get(`/pedidos-venta/${e.id}/albaranes`).then(c).catch(() => c([])), [t, e.id, a]), x.useEffect(() => void t.get("/formas-pago").then((P) => j(P.filter((W) => W.activo))).catch(() => j([])), [t]), i) return /* @__PURE__ */ l.jsx("div", { className: "panel", children: /* @__PURE__ */ l.jsx("p", { className: "dx-rojo", children: i }) });
  if (!a) return /* @__PURE__ */ l.jsx("div", { className: "muted", children: "Cargando…" });
  const D = (a.estado === "Borrador" || a.estado === "Confirmado") && a.lineas.every((P) => P.cantidadServida === 0), C = a.lineas.some((P) => P.pendienteServir > 0), $ = a.estado !== "Cancelado" && a.estado !== "Facturado", b = { clienteId: a.clienteId, fecha: a.fecha, lineas: a.lineas, moneda: a.moneda };
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsx(
        jl,
        {
          titulo: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            "Pedido de venta ",
            /* @__PURE__ */ l.jsx("span", { className: "mono", children: a.numeroCompleto })
          ] }),
          estado: a.estado,
          volver: () => r({ tipo: "pedido", pantalla: "lista" }),
          acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => Yr(n, `/pedidos-venta/${a.id}/pdf`).catch((P) => n.aviso(P.message, "err")), children: "PDF" }),
            /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "pedido", pantalla: "editor", semilla: { ...b, fecha: void 0 } }), children: "Duplicar" }),
            D && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "pedido", pantalla: "editor", id: a.id, semilla: b }), children: "Editar" }),
            a.estado === "Borrador" && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: async () => await mt(() => t.post(`/pedidos-venta/${a.id}/confirmar`), n.aviso, "Pedido confirmado.") && o(), children: "Confirmar" }),
            $ && a.estado !== "Borrador" && C && n.reservarPales && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", title: "Apartar palés cerrados para este pedido", onClick: () => n.reservarPales(a.id), children: "Reservar palés" }),
            $ && a.estado !== "Borrador" && C && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => m(Object.fromEntries(a.lineas.map((P) => [P.id, P.pendienteServir]))), children: "Entregar (albarán)" }),
            $ && a.estado !== "Borrador" && /* @__PURE__ */ l.jsx("button", { className: "btn small", onClick: () => u(!0), children: "Facturar" }),
            $ && /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", onClick: async () => window.confirm("¿Cancelar el pedido?") && await mt(() => t.post(`/pedidos-venta/${a.id}/cancelar`), n.aviso, "Pedido cancelado.") && o(), children: "Cancelar" })
          ] })
        }
      ),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ l.jsx(Oe, { etiqueta: "Cliente", children: /* @__PURE__ */ l.jsx("strong", { children: a.clienteNombre }) }),
        /* @__PURE__ */ l.jsx(Oe, { etiqueta: "Fecha", children: Ce(a.fecha) }),
        /* @__PURE__ */ l.jsx(Oe, { etiqueta: "Viene de", children: a.presupuestoOrigenId ? /* @__PURE__ */ l.jsx(Zn, { alPulsar: () => r({ tipo: "presupuesto", pantalla: "vista", id: a.presupuestoOrigenId }), children: "presupuesto" }) : "—" }),
        /* @__PURE__ */ l.jsx(Oe, { etiqueta: "Factura", children: a.facturaId ? /* @__PURE__ */ l.jsx(Zn, { alPulsar: () => r({ tipo: "factura", pantalla: "vista", id: a.facturaId }), children: "ver la factura" }) : "—" })
      ] })
    ] }),
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsxs("table", { children: [
        /* @__PURE__ */ l.jsx("thead", { children: /* @__PURE__ */ l.jsxs("tr", { children: [
          /* @__PURE__ */ l.jsx("th", { children: "Descripción" }),
          /* @__PURE__ */ l.jsx("th", { className: "num", children: "Pedido" }),
          /* @__PURE__ */ l.jsx("th", { className: "num", children: "Servido" }),
          /* @__PURE__ */ l.jsx("th", { className: "num", children: "Pendiente" }),
          /* @__PURE__ */ l.jsx("th", { className: "num", children: "Precio" }),
          /* @__PURE__ */ l.jsx("th", { className: "num", children: "Dto" }),
          /* @__PURE__ */ l.jsx("th", { className: "num", children: "Importe" })
        ] }) }),
        /* @__PURE__ */ l.jsx("tbody", { children: a.lineas.map((P) => /* @__PURE__ */ l.jsxs("tr", { children: [
          /* @__PURE__ */ l.jsxs("td", { children: [
            P.descripcion,
            /* @__PURE__ */ l.jsx(yl, { conceptos: P.conceptos, moneda: a.moneda })
          ] }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: We(P.cantidad) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: We(P.cantidadServida) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: P.pendienteServir > 0 ? /* @__PURE__ */ l.jsx("strong", { children: We(P.pendienteServir) }) : "—" }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: pt(P.precioUnitario, a.moneda) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: P.porcentajeDescuento ? `${de(P.porcentajeDescuento)} %` : "" }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: pt(P.base, a.moneda) }) })
        ] }, P.id)) })
      ] }),
      /* @__PURE__ */ l.jsx("div", { className: "dx-totales-vista", children: /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-grande", children: [
        /* @__PURE__ */ l.jsx("span", { children: "Total (sin impuestos)" }),
        /* @__PURE__ */ l.jsx("span", { children: pt(a.total, a.moneda) })
      ] }) })
    ] }),
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsx("div", { className: "panel-head", children: /* @__PURE__ */ l.jsx("h2", { children: "Albaranes de entrega" }) }),
      s.length ? /* @__PURE__ */ l.jsxs("table", { children: [
        /* @__PURE__ */ l.jsx("thead", { children: /* @__PURE__ */ l.jsxs("tr", { children: [
          /* @__PURE__ */ l.jsx("th", { children: "Número" }),
          /* @__PURE__ */ l.jsx("th", { children: "Fecha" }),
          /* @__PURE__ */ l.jsx("th", { children: "Referencia" }),
          /* @__PURE__ */ l.jsx("th", { children: "Líneas" }),
          /* @__PURE__ */ l.jsx("th", {})
        ] }) }),
        /* @__PURE__ */ l.jsx("tbody", { children: s.map((P) => /* @__PURE__ */ l.jsxs("tr", { children: [
          /* @__PURE__ */ l.jsxs("td", { className: "mono", children: [
            /* @__PURE__ */ l.jsx("strong", { children: P.numeroCompleto }),
            " ",
            P.anulado && /* @__PURE__ */ l.jsx("span", { className: "pill neg", title: P.motivoAnulacion ?? "", children: "Anulado" })
          ] }),
          /* @__PURE__ */ l.jsx("td", { children: Ce(P.fecha) }),
          /* @__PURE__ */ l.jsx("td", { className: "muted", children: P.referencia }),
          /* @__PURE__ */ l.jsx("td", { className: "muted", children: P.lineas.map((W) => `${We(W.cantidad)} × ${W.descripcion}`).join(" · ") }),
          /* @__PURE__ */ l.jsx("td", { className: "right", children: !P.anulado && a.estado !== "Facturado" && /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", onClick: async () => {
            const W = window.prompt("Motivo de la anulación del albarán:");
            W !== null && await mt(() => t.post(`/pedidos-venta/${a.id}/albaranes/${P.id}/anular`, { motivo: W || null }), n.aviso, "Albarán anulado.") && o();
          }, children: "Anular" }) })
        ] }, P.id)) })
      ] }) : /* @__PURE__ */ l.jsx("p", { className: "muted", style: { margin: 0 }, children: "Sin entregas todavía." })
    ] }),
    d && /* @__PURE__ */ l.jsxs(
      pn,
      {
        titulo: "Entrega (albarán de venta)",
        alCerrar: () => m(null),
        ancho: 640,
        acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => m(null), children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", onClick: async () => await mt(() => t.post(`/pedidos-venta/${a.id}/entregar`, { fecha: w, referencia: h || null, lineas: Object.entries(d).filter(([, P]) => P > 0).map(([P, W]) => ({ lineaPedidoId: P, cantidad: W })) }), n.aviso, "Albarán creado.") && (m(null), o()), children: "Crear albarán" })
        ] }),
        children: [
          /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Fecha" }),
              /* @__PURE__ */ l.jsx("input", { type: "date", value: w, onChange: (P) => M(P.target.value) })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Referencia" }),
              /* @__PURE__ */ l.jsx("input", { value: h, onChange: (P) => N(P.target.value) })
            ] })
          ] }),
          /* @__PURE__ */ l.jsxs("table", { style: { marginTop: 10 }, children: [
            /* @__PURE__ */ l.jsx("thead", { children: /* @__PURE__ */ l.jsxs("tr", { children: [
              /* @__PURE__ */ l.jsx("th", { children: "Línea" }),
              /* @__PURE__ */ l.jsx("th", { className: "num", children: "Pendiente" }),
              /* @__PURE__ */ l.jsx("th", { className: "num", style: { width: 120 }, children: "Entregar" })
            ] }) }),
            /* @__PURE__ */ l.jsx("tbody", { children: a.lineas.filter((P) => P.pendienteServir > 0).map((P) => /* @__PURE__ */ l.jsxs("tr", { children: [
              /* @__PURE__ */ l.jsx("td", { children: P.descripcion }),
              /* @__PURE__ */ l.jsx("td", { className: "num", children: We(P.pendienteServir) }),
              /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx("input", { className: "num", type: "number", step: "0.001", value: d[P.id] ?? 0, onChange: (W) => m({ ...d, [P.id]: Number(W.target.value) }) }) })
            ] }, P.id)) })
          ] })
        ]
      }
    ),
    p && /* @__PURE__ */ l.jsxs(
      pn,
      {
        titulo: `Facturar el pedido ${a.numeroCompleto}`,
        alCerrar: () => u(!1),
        acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => u(!1), children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", onClick: async () => {
            try {
              const P = await t.post(`/pedidos-venta/${a.id}/facturar`, { fechaEmision: v, formaPagoId: T || null });
              n.aviso("Factura emitida.", "ok"), r({ tipo: "factura", pantalla: "vista", id: P.id });
            } catch (P) {
              n.aviso(P.message, "err");
            }
          }, children: "Emitir factura" })
        ] }),
        children: [
          /* @__PURE__ */ l.jsx("p", { className: "muted", style: { marginTop: 0 }, children: "Se factura el pedido completo con sus precios y conceptos. La factura queda numerada y encadenada en VeriFactu." }),
          /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Fecha de emisión" }),
              /* @__PURE__ */ l.jsx("input", { type: "date", value: v, onChange: (P) => E(P.target.value) })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Forma de pago" }),
              /* @__PURE__ */ l.jsxs("select", { value: T, onChange: (P) => R(P.target.value), children: [
                /* @__PURE__ */ l.jsx("option", { value: "", children: "La del cliente" }),
                f.map((P) => /* @__PURE__ */ l.jsx("option", { value: P.id, children: P.nombre }, P.id))
              ] })
            ] })
          ] })
        ]
      }
    )
  ] });
}
function bm(e) {
  const { api: t, anfitrion: n, navegar: r } = Lt(), { dato: a, error: i, recargar: o } = Nl(() => t.get(`/compras/pedidos/${e.id}`)), [s, c] = x.useState([]), [f, j] = x.useState([]), [d, m] = x.useState([]), [h, N] = x.useState(null), [w, M] = x.useState(""), [p, u] = x.useState(""), [v, E] = x.useState(Et()), [T, R] = x.useState(!1), [D, C] = x.useState("IVA21"), [$, b] = x.useState(0), [P, W] = x.useState(""), [se, Ue] = x.useState(Et());
  if (x.useEffect(() => void t.get(`/compras/pedidos/${e.id}/albaranes`).then(c).catch(() => c([])), [t, e.id, a]), x.useEffect(() => {
    t.get("/inventario/almacenes").then((g) => (j(g), g[0] && M(g[0].id))).catch(() => j([])), t.get("/tipos-iva").then((g) => m(g.filter((L) => L.activo))).catch(() => m([]));
  }, [t]), i) return /* @__PURE__ */ l.jsx("div", { className: "panel", children: /* @__PURE__ */ l.jsx("p", { className: "dx-rojo", children: i }) });
  if (!a) return /* @__PURE__ */ l.jsx("div", { className: "muted", children: "Cargando…" });
  const Ve = (a.estado === "Borrador" || a.estado === "Confirmado") && a.lineas.every((g) => g.cantidadRecibida === 0 && g.cantidadFacturada === 0) && !a.empresaOrigenId, Ee = a.estado !== "Cancelado" && a.estado !== "Facturado", Re = a.lineas.some((g) => g.pendienteRecibir > 0), I = a.lineas.reduce((g, L) => g + L.costeConceptos, 0);
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsx(
        jl,
        {
          titulo: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            "Pedido de compra ",
            /* @__PURE__ */ l.jsx("span", { className: "mono", children: a.numeroCompleto })
          ] }),
          estado: a.estado,
          volver: () => r({ tipo: "compra", pantalla: "lista" }),
          acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => Yr(n, `/compras/pedidos/${a.id}/pdf`).catch((g) => n.aviso(g.message, "err")), children: "PDF" }),
            !a.empresaOrigenId && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "compra", pantalla: "editor", semilla: { ...a, fecha: Et() } }), children: "Duplicar" }),
            Ve && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "compra", pantalla: "editor", id: a.id, semilla: a }), children: "Editar" }),
            a.estado === "Borrador" && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: async () => await mt(() => t.post(`/compras/pedidos/${a.id}/confirmar`), n.aviso, "Pedido confirmado.") && o(), children: "Confirmar" }),
            Ee && a.estado !== "Borrador" && Re && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => N(Object.fromEntries(a.lineas.map((g) => [g.id, { cantidad: g.pendienteRecibir, lote: "" }]))), children: "Recibir mercancía" }),
            Ee && a.estado !== "Borrador" && /* @__PURE__ */ l.jsx("button", { className: "btn small", onClick: () => R(!0), children: "Facturar" }),
            Ee && !a.empresaOrigenId && /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", onClick: async () => window.confirm("¿Cancelar el pedido?") && await mt(() => t.post(`/compras/pedidos/${a.id}/cancelar`), n.aviso, "Pedido cancelado.") && o(), children: "Cancelar" })
          ] })
        }
      ),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ l.jsx(Oe, { etiqueta: "Proveedor", children: /* @__PURE__ */ l.jsx("strong", { children: a.proveedorTexto }) }),
        /* @__PURE__ */ l.jsx(Oe, { etiqueta: "Fecha", children: Ce(a.fecha) }),
        /* @__PURE__ */ l.jsx(Oe, { etiqueta: "Total", children: z(a.total) }),
        /* @__PURE__ */ l.jsx(Oe, { etiqueta: "Costes añadidos", children: I ? z(I) : "—" })
      ] }),
      a.empresaOrigenId && /* @__PURE__ */ l.jsx("p", { className: "muted", children: "Traspaso de otra empresa del grupo: se gestiona desde el documento de venta de origen." })
    ] }),
    /* @__PURE__ */ l.jsx("div", { className: "panel", children: /* @__PURE__ */ l.jsxs("table", { children: [
      /* @__PURE__ */ l.jsx("thead", { children: /* @__PURE__ */ l.jsxs("tr", { children: [
        /* @__PURE__ */ l.jsx("th", { children: "Descripción" }),
        /* @__PURE__ */ l.jsx("th", { className: "num", children: "Pedido" }),
        /* @__PURE__ */ l.jsx("th", { className: "num", children: "Recibido" }),
        /* @__PURE__ */ l.jsx("th", { className: "num", children: "Pendiente" }),
        /* @__PURE__ */ l.jsx("th", { className: "num", children: "Precio" }),
        /* @__PURE__ */ l.jsx("th", { className: "num", children: "Importe" }),
        /* @__PURE__ */ l.jsx("th", { className: "num", children: "Coste entrada" })
      ] }) }),
      /* @__PURE__ */ l.jsx("tbody", { children: a.lineas.map((g) => /* @__PURE__ */ l.jsxs("tr", { children: [
        /* @__PURE__ */ l.jsxs("td", { children: [
          g.descripcion,
          /* @__PURE__ */ l.jsx(yl, { conceptos: g.conceptos })
        ] }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: We(g.cantidad) }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: We(g.cantidadRecibida) }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: g.pendienteRecibir > 0 ? /* @__PURE__ */ l.jsx("strong", { children: We(g.pendienteRecibir) }) : "—" }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: z(g.precioUnitario) }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: z(g.importe) }) }),
        /* @__PURE__ */ l.jsxs("td", { className: "num muted", children: [
          z(g.costeUnitarioEntrada),
          "/ud"
        ] })
      ] }, g.id)) })
    ] }) }),
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsx("div", { className: "panel-head", children: /* @__PURE__ */ l.jsx("h2", { children: "Albaranes de recepción" }) }),
      s.length ? /* @__PURE__ */ l.jsxs("table", { children: [
        /* @__PURE__ */ l.jsx("thead", { children: /* @__PURE__ */ l.jsxs("tr", { children: [
          /* @__PURE__ */ l.jsx("th", { children: "Número" }),
          /* @__PURE__ */ l.jsx("th", { children: "Fecha" }),
          /* @__PURE__ */ l.jsx("th", { children: "Referencia" }),
          /* @__PURE__ */ l.jsx("th", { children: "Almacén" }),
          /* @__PURE__ */ l.jsx("th", { children: "Líneas" }),
          /* @__PURE__ */ l.jsx("th", {})
        ] }) }),
        /* @__PURE__ */ l.jsx("tbody", { children: s.map((g) => {
          var L;
          return /* @__PURE__ */ l.jsxs("tr", { children: [
            /* @__PURE__ */ l.jsxs("td", { className: "mono", children: [
              /* @__PURE__ */ l.jsx("strong", { children: g.numeroCompleto }),
              " ",
              g.anulado && /* @__PURE__ */ l.jsx("span", { className: "pill neg", title: g.motivoAnulacion ?? "", children: "Anulado" })
            ] }),
            /* @__PURE__ */ l.jsx("td", { children: Ce(g.fecha) }),
            /* @__PURE__ */ l.jsx("td", { className: "muted", children: g.referencia }),
            /* @__PURE__ */ l.jsx("td", { className: "muted", children: ((L = f.find((V) => V.id === g.almacenId)) == null ? void 0 : L.nombre) ?? "—" }),
            /* @__PURE__ */ l.jsx("td", { className: "muted", children: g.lineas.map((V) => `${We(V.cantidad)} × ${V.descripcion}`).join(" · ") }),
            /* @__PURE__ */ l.jsx("td", { className: "right", children: !g.anulado && a.estado !== "Facturado" && /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", onClick: async () => {
              const V = window.prompt("Motivo de la anulación del albarán:");
              V !== null && await mt(() => t.post(`/compras/pedidos/${a.id}/albaranes/${g.id}/anular`, { motivo: V || null }), n.aviso, "Albarán anulado.") && o();
            }, children: "Anular" }) })
          ] }, g.id);
        }) })
      ] }) : /* @__PURE__ */ l.jsx("p", { className: "muted", style: { margin: 0 }, children: "Sin recepciones todavía." })
    ] }),
    h && /* @__PURE__ */ l.jsxs(
      pn,
      {
        titulo: "Recepción de mercancía",
        alCerrar: () => N(null),
        ancho: 680,
        acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => N(null), children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", onClick: async () => await mt(() => t.post(`/compras/pedidos/${a.id}/recibir`, { fecha: v, referencia: p || null, almacenId: w || null, lineas: Object.entries(h).filter(([, g]) => g.cantidad > 0).map(([g, L]) => ({ lineaPedidoId: g, cantidad: L.cantidad, lote: L.lote || null })) }), n.aviso, "Recepción registrada.") && (N(null), o()), children: "Registrar recepción" })
        ] }),
        children: [
          /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Fecha" }),
              /* @__PURE__ */ l.jsx("input", { type: "date", value: v, onChange: (g) => E(g.target.value) })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Albarán del proveedor" }),
              /* @__PURE__ */ l.jsx("input", { value: p, onChange: (g) => u(g.target.value) })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Almacén" }),
              /* @__PURE__ */ l.jsxs("select", { value: w, onChange: (g) => M(g.target.value), children: [
                /* @__PURE__ */ l.jsx("option", { value: "", children: "Sin entrada en almacén" }),
                f.map((g) => /* @__PURE__ */ l.jsx("option", { value: g.id, children: g.nombre }, g.id))
              ] })
            ] })
          ] }),
          /* @__PURE__ */ l.jsxs("table", { style: { marginTop: 10 }, children: [
            /* @__PURE__ */ l.jsx("thead", { children: /* @__PURE__ */ l.jsxs("tr", { children: [
              /* @__PURE__ */ l.jsx("th", { children: "Línea" }),
              /* @__PURE__ */ l.jsx("th", { className: "num", children: "Pendiente" }),
              /* @__PURE__ */ l.jsx("th", { className: "num", style: { width: 110 }, children: "Recibir" }),
              /* @__PURE__ */ l.jsx("th", { style: { width: 130 }, children: "Lote" })
            ] }) }),
            /* @__PURE__ */ l.jsx("tbody", { children: a.lineas.filter((g) => g.pendienteRecibir > 0).map((g) => {
              var L, V;
              return /* @__PURE__ */ l.jsxs("tr", { children: [
                /* @__PURE__ */ l.jsx("td", { children: g.descripcion }),
                /* @__PURE__ */ l.jsx("td", { className: "num", children: We(g.pendienteRecibir) }),
                /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx("input", { className: "num", type: "number", step: "0.001", value: ((L = h[g.id]) == null ? void 0 : L.cantidad) ?? 0, onChange: (B) => N({ ...h, [g.id]: { ...h[g.id], cantidad: Number(B.target.value) } }) }) }),
                /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx("input", { value: ((V = h[g.id]) == null ? void 0 : V.lote) ?? "", onChange: (B) => N({ ...h, [g.id]: { ...h[g.id], lote: B.target.value } }) }) })
              ] }, g.id);
            }) })
          ] })
        ]
      }
    ),
    T && /* @__PURE__ */ l.jsxs(
      pn,
      {
        titulo: `Facturar el pedido ${a.numeroCompleto}`,
        alCerrar: () => R(!1),
        acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => R(!1), children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", onClick: async () => await mt(() => t.post(`/compras/pedidos/${a.id}/facturar`, { codigoIva: D, porcentajeIrpf: $, numeroFactura: P || null, fechaFactura: se }), n.aviso, "Factura del proveedor registrada como gasto.") && (R(!1), o()), children: "Registrar factura" })
        ] }),
        children: [
          /* @__PURE__ */ l.jsxs("p", { className: "muted", style: { marginTop: 0 }, children: [
            "Registra la factura del proveedor por el total del pedido (",
            z(a.total),
            ") como gasto, con su asiento si la contabilidad es automática."
          ] }),
          /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Nº de factura del proveedor" }),
              /* @__PURE__ */ l.jsx("input", { value: P, onChange: (g) => W(g.target.value), autoFocus: !0 })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Fecha de la factura" }),
              /* @__PURE__ */ l.jsx("input", { type: "date", value: se, onChange: (g) => Ue(g.target.value) })
            ] })
          ] }),
          /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Impuesto" }),
              /* @__PURE__ */ l.jsx("select", { value: D, onChange: (g) => C(g.target.value), children: d.map((g) => /* @__PURE__ */ l.jsx("option", { value: g.codigo, children: g.nombre }, g.codigo)) })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Retención IRPF %" }),
              /* @__PURE__ */ l.jsx("input", { type: "number", step: "0.01", value: $, onChange: (g) => b(Number(g.target.value)) })
            ] })
          ] })
        ]
      }
    )
  ] });
}
const ti = (e = "") => ({ clave: na(), descripcion: "", cuentaGasto: "", base: 0, codigoIva: e, porcentajeIva: null, porcentajeDeducible: 100, suplido: !1 }), Um = (e) => e === "InversionSujetoPasivo" || e === "Intracomunitario";
function Vm(e) {
  const { api: t, anfitrion: n } = Lt(), r = e.semilla, [a, i] = x.useState([]), [o, s] = x.useState([]), [c, f] = x.useState([]), [j, d] = x.useState([]), [m, h] = x.useState((r == null ? void 0 : r.proveedorId) ?? ""), [N, w] = x.useState(e.id ? (r == null ? void 0 : r.numeroFactura) ?? "" : ""), [M, p] = x.useState((r == null ? void 0 : r.fechaFactura) ?? Et()), [u, v] = x.useState(e.id ? (r == null ? void 0 : r.fecha) ?? Et() : Et()), [E, T] = x.useState(r != null && r.concepto && !r.concepto.startsWith("Factura ") ? r.concepto : ""), [R, D] = x.useState((r == null ? void 0 : r.porcentajeIrpf) ?? 0), [C, $] = x.useState(""), [b, P] = x.useState(((r == null ? void 0 : r.recargoTotal) ?? 0) > 0), W = !!(r != null && r.esRectificativa), [se, Ue] = x.useState((r == null ? void 0 : r.numeroRectificado) ?? ""), [Ve, Ee] = x.useState((r == null ? void 0 : r.fechaRectificada) ?? ""), [Re, I] = x.useState((r == null ? void 0 : r.motivoRectificacion) ?? ""), [g, L] = x.useState(!1), [V, B] = x.useState((r == null ? void 0 : r.afectacion) ?? "Comun"), [Y, je] = x.useState((r == null ? void 0 : r.moneda) ?? ""), [ve, ce] = x.useState((r == null ? void 0 : r.tasaCambio) ?? null), Ne = (y) => r != null && r.moneda && r.tasaCambio ? Me(y / r.tasaCambio) : y, [st, tt] = x.useState(
    () => {
      var y;
      return (y = r == null ? void 0 : r.lineas) != null && y.length ? r.lineas.map((H) => ({ clave: na(), descripcion: H.descripcion ?? "", cuentaGasto: H.cuentaGasto ?? "", base: Ne(H.base), codigoIva: H.codigoIva, porcentajeIva: H.autoliquidada ? H.porcentajeIva : null, porcentajeDeducible: H.porcentajeDeducible, suplido: !!H.suplido })) : [ti()];
    }
  ), [ct, Be] = x.useState(e.id && (r != null && r.vencimientos) && r.vencimientos.length > 1 ? r.vencimientos : null), [k, Pt] = x.useState(null), [ut, Ie] = x.useState(""), [yt, At] = x.useState(!1), Gt = ra();
  x.useEffect(() => {
    t.get("/proveedores").then(i).catch(() => i([])), t.get("/tipos-iva").then((y) => s(y.filter((H) => H.activo))).catch(() => s([])), t.get("/formas-pago").then((y) => f(y.filter((H) => H.activo))).catch(() => f([])), t.get("/empresas/actual").then((y) => {
      y.regimenIva === "RecargoEquivalencia" && (L(!0), r || P(!0));
    }).catch(() => {
    }), t.get("/contabilidad/cuentas").then((y) => d(y.filter((H) => H.codigo.startsWith("6") || H.codigo.startsWith("2")))).catch(() => d([]));
  }, [t]);
  const xe = a.find((y) => y.id === m);
  x.useEffect(() => {
    xe != null && xe.formaPagoDefectoId && !C && $(xe.formaPagoDefectoId);
  }, [xe]);
  const qe = x.useMemo(
    () => ({
      proveedorId: m || null,
      proveedorTexto: (xe == null ? void 0 : xe.nombre) ?? null,
      numeroFactura: N.trim() || null,
      fechaFactura: M || null,
      fecha: u,
      concepto: E.trim() || null,
      porcentajeIrpf: R,
      formaPagoId: C || null,
      recargoEquivalencia: b,
      afectacion: V,
      baseImponible: 0,
      lineas: st.filter((y) => y.base !== 0).map((y) => ({
        base: y.base,
        codigoIva: y.codigoIva || null,
        descripcion: y.descripcion.trim() || null,
        porcentajeIva: y.porcentajeIva,
        porcentajeDeducible: y.porcentajeDeducible,
        cuentaGasto: y.cuentaGasto.trim() || null,
        suplido: y.suplido
      })),
      vencimientos: ct,
      rectificaGastoId: W ? (r == null ? void 0 : r.rectificaGastoId) ?? null : null,
      numeroRectificado: W && se.trim() || null,
      fechaRectificada: W && Ve || null,
      motivoRectificacion: W && Re.trim() || null,
      moneda: Y || null,
      tasaCambio: Y ? ve : null
    }),
    [m, xe, N, M, u, E, R, C, b, V, st, ct, W, r, se, Ve, Re, Y, ve]
  ), Ft = Cn(qe, 350);
  x.useEffect(() => {
    if (!Ft.lineas.length) {
      Pt(null), Ie("Añade al menos una línea con base.");
      return;
    }
    const y = Gt();
    t.post("/gastos/simular", Ft).then((H) => y() && (Pt(H), Ie(""))).catch((H) => y() && (Pt(null), Ie(H.message)));
  }, [Ft, t]);
  const ze = (y, H) => tt((Se) => Se.map((ge) => ge.clave === y ? { ...ge, ...H } : ge)), _ = (y) => o.find((H) => H.codigo === y), q = (y) => {
    var H;
    return (H = k == null ? void 0 : k.lineas) == null ? void 0 : H[st.filter((Se) => Se.base !== 0).indexOf(y)];
  };
  function A(y) {
    if (!k) return;
    const H = /* @__PURE__ */ new Date((M || u) + "T00:00:00"), Se = Me(k.total / y);
    Be(Array.from({ length: y }, (ge, ee) => {
      const re = new Date(H);
      return re.setMonth(re.getMonth() + ee + 1), { fecha: re.toISOString().slice(0, 10), importe: ee === y - 1 ? Me(k.total - Se * (y - 1)) : Se };
    }));
  }
  async function Q() {
    At(!0);
    try {
      const y = e.id ? await t.put(`/gastos/${e.id}`, qe) : await t.post("/gastos", qe);
      n.aviso(e.id ? "Factura corregida." : "Factura registrada.", "ok"), y.avisoRiesgo && n.aviso(y.avisoRiesgo, "err"), e.alGuardar(y.id);
    } catch (y) {
      n.aviso(y.message, "err");
    } finally {
      At(!1);
    }
  }
  const ne = Me((ct ?? []).reduce((y, H) => y + (Number(H.importe) || 0), 0));
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsxs("div", { className: "panel-head", children: [
        /* @__PURE__ */ l.jsx("h2", { children: e.id ? `Corregir factura ${(r == null ? void 0 : r.numeroFactura) ?? ""}` : W ? "Rectificativa / abono del proveedor" : "Nueva factura de proveedor" }),
        /* @__PURE__ */ l.jsxs("div", { style: { display: "flex", gap: 8 }, children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: e.alCancelar, children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", disabled: !k || yt, onClick: Q, children: e.id ? "Guardar corrección" : W ? "Registrar abono" : "Registrar factura" })
        ] })
      ] }),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-cabecera", children: [
        /* @__PURE__ */ l.jsxs("div", { className: "dx-cab-campos", children: [
          /* @__PURE__ */ l.jsx(Wo, { terceros: a, valor: m, alCambiar: h, etiqueta: "Proveedor" }),
          /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Nº de factura del proveedor" }),
              /* @__PURE__ */ l.jsx("input", { value: N, onChange: (y) => w(y.target.value), placeholder: "F-2026/0117" })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Fecha de la factura" }),
              /* @__PURE__ */ l.jsx("input", { type: "date", value: M, onChange: (y) => p(y.target.value) })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Fecha de registro" }),
              /* @__PURE__ */ l.jsx("input", { type: "date", value: u, onChange: (y) => v(y.target.value), title: "La del asiento y la del periodo de IVA en que se deduce" })
            ] })
          ] }),
          /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Forma de pago" }),
              /* @__PURE__ */ l.jsxs("select", { value: C, onChange: (y) => $(y.target.value), children: [
                /* @__PURE__ */ l.jsx("option", { value: "", children: "Contado (vence en la fecha de factura)" }),
                c.map((y) => /* @__PURE__ */ l.jsx("option", { value: y.id, children: y.nombre }, y.id))
              ] })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Retención IRPF %" }),
              /* @__PURE__ */ l.jsx("input", { type: "number", step: "0.01", value: R, onChange: (y) => D(Number(y.target.value)) })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Moneda de la factura" }),
              /* @__PURE__ */ l.jsxs("select", { value: Y, onChange: (y) => je(y.target.value), children: [
                /* @__PURE__ */ l.jsx("option", { value: "", children: "EUR · euros" }),
                zd.map((y) => /* @__PURE__ */ l.jsx("option", { value: y, children: y }, y))
              ] })
            ] }),
            Y && /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsxs("label", { children: [
                "Tipo de cambio (€ por 1 ",
                Y,
                ")"
              ] }),
              /* @__PURE__ */ l.jsx("input", { type: "number", step: "0.000001", min: "0", value: ve ?? "", placeholder: "El del día de la factura", onChange: (y) => ce(y.target.value === "" ? null : Number(y.target.value)) })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Prorrata especial" }),
              /* @__PURE__ */ l.jsxs("select", { value: V, onChange: (y) => B(y.target.value), children: [
                /* @__PURE__ */ l.jsx("option", { value: "Comun", children: "Uso común" }),
                /* @__PURE__ */ l.jsx("option", { value: "ConDerecho", children: "Solo operaciones con derecho" }),
                /* @__PURE__ */ l.jsx("option", { value: "SinDerecho", children: "Solo operaciones exentas" })
              ] })
            ] })
          ] }),
          /* @__PURE__ */ l.jsx("div", { className: "dx-fila", children: /* @__PURE__ */ l.jsxs("div", { style: { gridColumn: "1 / -1" }, children: [
            /* @__PURE__ */ l.jsx("label", { children: "Concepto (vacío: «Factura nº»)" }),
            /* @__PURE__ */ l.jsx("input", { value: E, onChange: (y) => T(y.target.value) })
          ] }) }),
          W && /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Factura que rectifica" }),
              /* @__PURE__ */ l.jsx("input", { value: se, disabled: !!(r != null && r.rectificaGastoId), onChange: (y) => Ue(y.target.value) })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Fecha de la rectificada" }),
              /* @__PURE__ */ l.jsx("input", { type: "date", value: Ve, disabled: !!(r != null && r.rectificaGastoId), onChange: (y) => Ee(y.target.value) })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Motivo" }),
              /* @__PURE__ */ l.jsx("input", { value: Re, onChange: (y) => I(y.target.value), placeholder: "Devolución, descuento posterior, error de precio…" })
            ] })
          ] }),
          W && /* @__PURE__ */ l.jsx("div", { className: "muted", children: "Las bases del abono van en negativo; el asiento y el SII (R1, por diferencias) salen con el signo." }),
          /* @__PURE__ */ l.jsxs("label", { className: "dx-check", children: [
            /* @__PURE__ */ l.jsx("input", { type: "checkbox", checked: b, onChange: (y) => P(y.target.checked) }),
            " El proveedor me cobra recargo de equivalencia"
          ] }),
          g && /* @__PURE__ */ l.jsx("div", { className: "muted", children: "La empresa está en recargo de equivalencia: el IVA y el recargo soportados son coste (no se deducen)." })
        ] }),
        /* @__PURE__ */ l.jsx("div", { className: "dx-ficha", children: xe ? /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("strong", { children: xe.nombre }),
          /* @__PURE__ */ l.jsx("div", { className: "muted", children: [xe.nifFiscal, xe.poblacion, xe.pais].filter(Boolean).join(" · ") }),
          !xe.nifFiscal && /* @__PURE__ */ l.jsx("div", { className: "dx-aviso", children: "⚠ Sin NIF en la ficha: hace falta para el libro de IVA, el 347 y el SII." }),
          (k == null ? void 0 : k.avisoRiesgo) && /* @__PURE__ */ l.jsxs("div", { className: "dx-aviso", children: [
            "⚠ ",
            k.avisoRiesgo
          ] })
        ] }) : /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Elige el proveedor: su NIF va al libro de IVA y al SII, y el número de factura no se puede repetir." }) })
      ] })
    ] }),
    /* @__PURE__ */ l.jsxs("div", { className: "panel dx-rejilla", children: [
      /* @__PURE__ */ l.jsxs("table", { children: [
        /* @__PURE__ */ l.jsx("thead", { children: /* @__PURE__ */ l.jsxs("tr", { children: [
          /* @__PURE__ */ l.jsx("th", { children: "Descripción" }),
          /* @__PURE__ */ l.jsx("th", { style: { width: 130 }, children: "Cuenta de gasto" }),
          /* @__PURE__ */ l.jsx("th", { className: "num", style: { width: 120 }, children: "Base" }),
          /* @__PURE__ */ l.jsx("th", { style: { width: 200 }, children: "Impuesto" }),
          /* @__PURE__ */ l.jsx("th", { className: "num", style: { width: 80 }, children: "% IVA" }),
          /* @__PURE__ */ l.jsx("th", { className: "num", style: { width: 90 }, children: "% deduc." }),
          /* @__PURE__ */ l.jsx("th", { className: "num", style: { width: 110 }, children: "Cuota" }),
          /* @__PURE__ */ l.jsx("th", { style: { width: 40 } })
        ] }) }),
        /* @__PURE__ */ l.jsx("tbody", { children: st.map((y, H) => {
          const Se = _(y.codigoIva), ge = q(y);
          return /* @__PURE__ */ l.jsxs("tr", { className: H % 2 ? "dx-par" : "", children: [
            /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx("input", { value: y.descripcion, onChange: (ee) => ze(y.clave, { descripcion: ee.target.value }), placeholder: "Qué se compra" }) }),
            /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx("input", { list: "dx-cuentas-gasto", value: y.cuentaGasto, onChange: (ee) => ze(y.clave, { cuentaGasto: ee.target.value }), placeholder: "la de la regla" }) }),
            /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx("input", { className: "num", type: "number", step: "0.01", value: y.base || "", onChange: (ee) => ze(y.clave, { base: Number(ee.target.value) }) }) }),
            /* @__PURE__ */ l.jsxs("td", { children: [
              !y.suplido && /* @__PURE__ */ l.jsxs("select", { value: y.codigoIva, onChange: (ee) => ze(y.clave, { codigoIva: ee.target.value, porcentajeIva: null }), children: [
                /* @__PURE__ */ l.jsx("option", { value: "", children: "General" }),
                o.map((ee) => /* @__PURE__ */ l.jsx("option", { value: ee.codigo, children: ee.nombre }, ee.codigo))
              ] }),
              /* @__PURE__ */ l.jsxs("label", { className: "dx-check", title: "Pagado por el proveedor por cuenta de la empresa: sin impuesto, fuera de la base y a su cuenta (obligatoria)", children: [
                /* @__PURE__ */ l.jsx("input", { type: "checkbox", checked: y.suplido, onChange: (ee) => ze(y.clave, { suplido: ee.target.checked }) }),
                " Suplido"
              ] })
            ] }),
            /* @__PURE__ */ l.jsx("td", { children: Um(Se == null ? void 0 : Se.clase) ? /* @__PURE__ */ l.jsx("input", { className: "num", type: "number", step: "0.01", value: y.porcentajeIva ?? "", placeholder: "21", title: "Tipo que autoliquidas", onChange: (ee) => ze(y.clave, { porcentajeIva: ee.target.value === "" ? null : Number(ee.target.value) }) }) : /* @__PURE__ */ l.jsx("span", { className: "muted", children: ge ? `${de(ge.porcentajeIva)} %` : "" }) }),
            /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx("input", { className: "num", type: "number", step: "1", min: 0, max: 100, value: y.porcentajeDeducible, onChange: (ee) => ze(y.clave, { porcentajeDeducible: Number(ee.target.value) }) }) }),
            /* @__PURE__ */ l.jsx("td", { className: "num", children: ge ? /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
              /* @__PURE__ */ l.jsx("strong", { children: z(ge.cuota) }),
              ge.autoliquidada && /* @__PURE__ */ l.jsx("div", { className: "dx-sub", children: "autoliquidada" }),
              ge.cuotaRecargo !== 0 && /* @__PURE__ */ l.jsxs("div", { className: "dx-sub", children: [
                "+ recargo ",
                z(ge.cuotaRecargo)
              ] })
            ] }) : "—" }),
            /* @__PURE__ */ l.jsx("td", { className: "right", children: /* @__PURE__ */ l.jsx("button", { className: "dx-icono", title: "Quitar", onClick: () => tt((ee) => ee.length > 1 ? ee.filter((re) => re.clave !== y.clave) : [ti()]), children: "✕" }) })
          ] }, y.clave);
        }) })
      ] }),
      /* @__PURE__ */ l.jsx("datalist", { id: "dx-cuentas-gasto", children: j.map((y) => /* @__PURE__ */ l.jsx("option", { value: y.codigo, children: y.nombre }, y.codigo)) }),
      /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", style: { marginTop: 8 }, onClick: () => tt((y) => {
        var H;
        return [...y, ti(((H = y[y.length - 1]) == null ? void 0 : H.codigoIva) ?? "")];
      }), children: "+ Añadir línea" }),
      /* @__PURE__ */ l.jsx("span", { className: "muted dx-ayuda", children: "Una línea por cada base e impuesto de la factura. En inversión del sujeto pasivo e intracomunitarias el IVA lo autoliquidas tú (no lo cobra el proveedor)." })
    ] }),
    /* @__PURE__ */ l.jsxs("div", { className: "dx-pie", children: [
      /* @__PURE__ */ l.jsxs("div", { className: "panel", style: { margin: 0 }, children: [
        /* @__PURE__ */ l.jsxs("div", { className: "panel-head", children: [
          /* @__PURE__ */ l.jsx("h2", { children: "Vencimientos" }),
          /* @__PURE__ */ l.jsxs("div", { className: "dx-acciones", children: [
            [2, 3, 4].map((y) => /* @__PURE__ */ l.jsxs("button", { className: "btn small secondary", disabled: !k, onClick: () => A(y), children: [
              y,
              " plazos"
            ] }, y)),
            ct && /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", onClick: () => Be(null), children: "Según forma de pago" })
          ] })
        ] }),
        ct ? /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          ct.map((y, H) => /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", style: { marginBottom: 6 }, children: [
            /* @__PURE__ */ l.jsx("input", { type: "date", value: y.fecha, onChange: (Se) => Be(ct.map((ge, ee) => ee === H ? { ...ge, fecha: Se.target.value } : ge)) }),
            /* @__PURE__ */ l.jsx("input", { className: "num", type: "number", step: "0.01", value: y.importe, onChange: (Se) => Be(ct.map((ge, ee) => ee === H ? { ...ge, importe: Number(Se.target.value) } : ge)) })
          ] }, H)),
          k && ne !== k.total && /* @__PURE__ */ l.jsxs("p", { className: "dx-rojo", style: { margin: 0 }, children: [
            "Los plazos suman ",
            z(ne),
            "; la factura, ",
            z(k.total),
            "."
          ] })
        ] }) : /* @__PURE__ */ l.jsx("p", { className: "muted", style: { margin: 0 }, children: ((k == null ? void 0 : k.vencimientos) ?? []).map((y) => `${Ce(y.fecha)}: ${z(y.importe)}`).join(" · ") || "—" }),
        ut && /* @__PURE__ */ l.jsx("p", { className: "dx-rojo", style: { marginBottom: 0 }, children: ut })
      ] }),
      /* @__PURE__ */ l.jsxs("div", { className: "panel dx-totales", children: [
        ((k == null ? void 0 : k.desglose) ?? []).map((y, H) => {
          var Se;
          return /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
            /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
              ((Se = _(y.codigoIva)) == null ? void 0 : Se.nombre) ?? y.codigoIva,
              " ",
              y.autoliquidada ? `(${de(y.porcentajeIva)} %, autoliquidado)` : "",
              " · base ",
              de(y.base)
            ] }),
            /* @__PURE__ */ l.jsx("span", { children: z(y.cuota) })
          ] }, H);
        }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Base imponible" }),
          /* @__PURE__ */ l.jsx("span", { children: z(k == null ? void 0 : k.baseImponible) })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Impuestos" }),
          /* @__PURE__ */ l.jsx("span", { children: z(k == null ? void 0 : k.cuotaIva) })
        ] }),
        !!(k != null && k.recargoTotal) && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Recargo de equivalencia" }),
          /* @__PURE__ */ l.jsx("span", { children: z(k.recargoTotal) })
        ] }),
        !!(k != null && k.retencionIrpf) && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Retención IRPF" }),
          /* @__PURE__ */ l.jsxs("span", { children: [
            "−",
            z(k.retencionIrpf)
          ] })
        ] }),
        !!(k != null && k.suplidos) && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Suplidos (sin impuesto)" }),
          /* @__PURE__ */ l.jsx("span", { children: z(k.suplidos) })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ l.jsx("span", { children: ((k == null ? void 0 : k.total) ?? 0) < 0 ? "A favor (abono del proveedor)" : "Total a pagar" }),
          /* @__PURE__ */ l.jsx("span", { children: z(k == null ? void 0 : k.total) })
        ] }),
        (k == null ? void 0 : k.moneda) && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ l.jsxs("span", { children: [
            "Total en ",
            k.moneda,
            " (1 ",
            k.moneda,
            " = ",
            String(k.tasaCambio ?? 0).replace(".", ","),
            " €)"
          ] }),
          /* @__PURE__ */ l.jsxs("span", { children: [
            de(k.totalDivisa ?? 0),
            " ",
            k.moneda
          ] })
        ] }),
        k && (k.desglose ?? []).some((y) => y.cuotaDeducible !== y.cuota) && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "IVA deducible (antes de prorrata)" }),
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: z((k.desglose ?? []).reduce((y, H) => y + H.cuotaDeducible, 0)) })
        ] })
      ] })
    ] })
  ] });
}
function Bm(e) {
  const { api: t, anfitrion: n, navegar: r } = Lt(), [a, i] = x.useState(null), [o, s] = x.useState(null), [c, f] = x.useState(""), [j, d] = x.useState(!1), m = () => {
    t.get(`/gastos/${e.id}`).then(i).catch((w) => f(w.message)), t.get(`/gastos/${e.id}/saldo`).then(s).catch(() => s(null));
  };
  if (x.useEffect(m, [e.id]), c) return /* @__PURE__ */ l.jsx("div", { className: "panel", children: /* @__PURE__ */ l.jsx("p", { className: "dx-rojo", children: c }) });
  if (!a) return /* @__PURE__ */ l.jsx("div", { className: "muted", children: "Cargando…" });
  const h = a.estado === "Registrado", N = !o || o.liquidado === 0;
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsxs("div", { className: "panel-head", children: [
        /* @__PURE__ */ l.jsxs("h2", { style: { display: "flex", alignItems: "center", gap: 10 }, children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "gasto", pantalla: "lista" }), children: "←" }),
          "Factura ",
          /* @__PURE__ */ l.jsx("span", { className: "mono", children: a.numeroFactura ?? "(sin número)" }),
          " ",
          /* @__PURE__ */ l.jsx("span", { className: Ho(a.estado === "Anulado" ? "Anulada" : "Emitida"), children: a.estado }),
          a.esRectificativa && /* @__PURE__ */ l.jsxs("span", { className: "pill", children: [
            "Rectifica ",
            a.numeroRectificado
          ] })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-acciones", children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "gasto", pantalla: "editor", semilla: { ...a, numeroFactura: null } }), children: "Duplicar" }),
          h && N && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "gasto", pantalla: "editor", id: a.id, semilla: a }), children: "Corregir" }),
          h && o && o.pendiente > 0 && n.irA && /* @__PURE__ */ l.jsx("button", { className: "btn small", onClick: () => n.irA("pagos"), children: "Pagar" }),
          h && !a.esRectificativa && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => {
            var w;
            return r({ tipo: "gasto", pantalla: "editor", semilla: {
              ...a,
              numeroFactura: null,
              esRectificativa: !0,
              rectificaGastoId: a.id,
              numeroRectificado: a.numeroFactura ?? a.concepto,
              fechaRectificada: a.fechaFactura ?? a.fecha,
              motivoRectificacion: "",
              vencimientos: null,
              lineas: (w = a.lineas) == null ? void 0 : w.map((M) => ({ ...M, base: -M.base }))
            } });
          }, children: "Rectificativa / abono" }),
          h && N && /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", onClick: () => d(!0), children: "Anular" })
        ] })
      ] }),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ l.jsxs("div", { className: "dx-dato", children: [
          /* @__PURE__ */ l.jsx("small", { children: "Proveedor" }),
          /* @__PURE__ */ l.jsx("div", { children: /* @__PURE__ */ l.jsx("strong", { children: a.proveedorTexto }) })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-dato", children: [
          /* @__PURE__ */ l.jsx("small", { children: "Fecha de factura" }),
          /* @__PURE__ */ l.jsx("div", { children: Ce(a.fechaFactura ?? a.fecha) })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-dato", children: [
          /* @__PURE__ */ l.jsx("small", { children: "Registro" }),
          /* @__PURE__ */ l.jsx("div", { children: Ce(a.fecha) })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-dato", children: [
          /* @__PURE__ */ l.jsx("small", { children: "Pago" }),
          /* @__PURE__ */ l.jsx("div", { children: o ? o.pendiente <= 0 ? /* @__PURE__ */ l.jsx("span", { className: "pill ok", children: "Pagada" }) : /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            "Pendiente ",
            /* @__PURE__ */ l.jsx("strong", { children: z(o.pendiente) })
          ] }) : "—" })
        ] })
      ] }),
      /* @__PURE__ */ l.jsx("p", { className: "muted", style: { marginBottom: 0 }, children: a.concepto })
    ] }),
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsxs("table", { children: [
        /* @__PURE__ */ l.jsx("thead", { children: /* @__PURE__ */ l.jsxs("tr", { children: [
          /* @__PURE__ */ l.jsx("th", { children: "Descripción" }),
          /* @__PURE__ */ l.jsx("th", { children: "Cuenta" }),
          /* @__PURE__ */ l.jsx("th", { className: "num", children: "Base" }),
          /* @__PURE__ */ l.jsx("th", { children: "Impuesto" }),
          /* @__PURE__ */ l.jsx("th", { className: "num", children: "Cuota" }),
          /* @__PURE__ */ l.jsx("th", { className: "num", children: "Deducible" })
        ] }) }),
        /* @__PURE__ */ l.jsx("tbody", { children: (a.lineas ?? []).map((w, M) => /* @__PURE__ */ l.jsxs("tr", { children: [
          /* @__PURE__ */ l.jsx("td", { children: w.descripcion ?? "" }),
          /* @__PURE__ */ l.jsx("td", { className: "mono muted", children: w.cuentaGasto ?? "regla" }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: z(w.base) }),
          /* @__PURE__ */ l.jsx("td", { children: w.suplido ? "Suplido · sin impuesto" : /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            w.codigoIva,
            " · ",
            de(w.porcentajeIva),
            " %",
            w.autoliquidada ? " · autoliquidada" : "",
            w.cuotaRecargo ? ` · recargo ${z(w.cuotaRecargo)}` : ""
          ] }) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: z(w.cuota) }),
          /* @__PURE__ */ l.jsxs("td", { className: "num muted", children: [
            w.porcentajeDeducible !== 100 ? `${de(w.porcentajeDeducible)} % · ` : "",
            z(w.cuotaDeducible)
          ] })
        ] }, M)) })
      ] }),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-totales-vista", children: [
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Base imponible" }),
          /* @__PURE__ */ l.jsx("span", { children: z(a.baseImponible) })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Impuestos" }),
          /* @__PURE__ */ l.jsx("span", { children: z(a.cuotaIva) })
        ] }),
        !!a.recargoTotal && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Recargo de equivalencia" }),
          /* @__PURE__ */ l.jsx("span", { children: z(a.recargoTotal) })
        ] }),
        !!a.suplidos && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Suplidos (sin impuesto)" }),
          /* @__PURE__ */ l.jsx("span", { children: z(a.suplidos) })
        ] }),
        !!a.retencionIrpf && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
            "Retención IRPF (",
            de(a.porcentajeIrpf),
            " %)"
          ] }),
          /* @__PURE__ */ l.jsxs("span", { children: [
            "−",
            z(a.retencionIrpf)
          ] })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ l.jsx("span", { children: a.total < 0 ? "A favor (abono del proveedor)" : "Total a pagar" }),
          /* @__PURE__ */ l.jsx("span", { children: z(a.total) })
        ] })
      ] }),
      /* @__PURE__ */ l.jsxs("p", { className: "muted", style: { fontSize: 12.5 }, children: [
        "Vencimientos: ",
        (a.vencimientos ?? []).map((w) => `${Ce(w.fecha)} ${z(w.importe)}`).join(" · ")
      ] })
    ] }),
    j && /* @__PURE__ */ l.jsx(
      pn,
      {
        titulo: "Anular la factura",
        alCerrar: () => d(!1),
        acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => d(!1), children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", onClick: async () => {
            try {
              await t.post(`/gastos/${a.id}/anular`), n.aviso("Factura anulada.", "ok"), d(!1), m();
            } catch (w) {
              n.aviso(w.message, "err");
            }
          }, children: "Anular" })
        ] }),
        children: /* @__PURE__ */ l.jsx("p", { style: { margin: 0 }, children: "Sale de los libros de IVA y de las declaraciones, y se contabiliza su contraasiento. Si solo hay que cambiar algo, mejor «Corregir»." })
      }
    )
  ] });
}
function Hm(e) {
  const [t, n] = x.useState(e.inicial), r = x.useRef(0), [a, i] = x.useState(0), o = x.useMemo(() => ym(e.anfitrion), [e.anfitrion]), s = (d) => {
    n(d), i(++r.current), window.scrollTo({ top: 0 });
  }, c = { api: o, anfitrion: e.anfitrion, ruta: t, navegar: s }, f = `${t.tipo}-${t.pantalla}-${t.id ?? ""}-${a}`;
  let j;
  if (t.pantalla === "lista") j = /* @__PURE__ */ l.jsx(Rm, { tipo: t.tipo });
  else if (t.pantalla === "vista" && t.id)
    j = t.tipo === "factura" ? /* @__PURE__ */ l.jsx($m, { id: t.id }) : t.tipo === "presupuesto" ? /* @__PURE__ */ l.jsx(Mm, { id: t.id }) : t.tipo === "pedido" ? /* @__PURE__ */ l.jsx(Om, { id: t.id }) : t.tipo === "gasto" ? /* @__PURE__ */ l.jsx(Bm, { id: t.id }) : /* @__PURE__ */ l.jsx(bm, { id: t.id });
  else if (t.tipo === "gasto")
    j = /* @__PURE__ */ l.jsx(
      Vm,
      {
        id: t.id,
        semilla: t.semilla,
        alGuardar: (d) => s({ tipo: "gasto", pantalla: "vista", id: d }),
        alCancelar: () => s(t.id ? { tipo: "gasto", pantalla: "vista", id: t.id } : { tipo: "gasto", pantalla: "lista" })
      }
    );
  else if (t.tipo === "compra")
    j = /* @__PURE__ */ l.jsx(
      Am,
      {
        id: t.id,
        semilla: t.semilla,
        alGuardar: (d) => s({ tipo: "compra", pantalla: "vista", id: d }),
        alCancelar: () => s(t.id ? { tipo: "compra", pantalla: "vista", id: t.id } : { tipo: "compra", pantalla: "lista" })
      }
    );
  else {
    const d = t.tipo;
    j = /* @__PURE__ */ l.jsx(
      Lm,
      {
        tipo: d,
        id: t.id,
        semilla: t.semilla,
        alGuardar: (m) => s({ tipo: d, pantalla: "vista", id: m }),
        alCancelar: () => s(t.id ? { tipo: d, pantalla: "vista", id: t.id } : { tipo: d, pantalla: "lista" })
      }
    );
  }
  return /* @__PURE__ */ l.jsx(Fd.Provider, { value: c, children: /* @__PURE__ */ l.jsx("div", { className: "dx-raiz", children: j }, f) });
}
const Wm = `
.dx-raiz { display:flex; flex-direction:column; gap:16px; }
.dx-editor { display:flex; flex-direction:column; gap:16px; }
.dx-editor .panel { margin:0; }
.dx-cabecera { display:grid; grid-template-columns: minmax(0,2fr) minmax(0,1fr); gap:18px; align-items:start; }
.dx-cab-campos label, .dx-dialogo label { display:block; margin:10px 0 5px; font-size:12.5px; color:var(--muted); font-weight:500; }
.dx-fila { display:grid; grid-template-columns: repeat(auto-fit, minmax(150px,1fr)); gap:10px; }
.dx-check { display:flex !important; align-items:center; gap:8px; margin-top:12px !important; color:var(--ink) !important; cursor:pointer; }
.dx-check input { width:auto; }
.dx-envase { display:inline-flex; align-items:center; gap:6px; margin:0 12px 6px 0; font-size:12px; color:var(--muted); }
.dx-envase select { width:auto; min-width:160px; }
.dx-ficha { background:var(--bg,#f4f6f9); border:1px solid var(--line); border-radius:12px; padding:12px 14px; font-size:13px; line-height:1.55; min-height:80px; }
.dx-aviso { margin-top:8px; color:#b45309; font-weight:600; }
.dx-anticipo { margin-top:8px; padding:8px 10px; border-radius:10px; background:var(--accent-soft); color:var(--ink); font-size:12.5px; }
.dx-anticipo .dx-check { margin-top:6px !important; }
.dx-tot-anticipo span:last-child { color:var(--accent); }
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
.dx-filtros2 { display:flex; flex-wrap:wrap; gap:8px; }
.dx-filtros2 > select, .dx-filtros2 > input { flex:1 1 150px; width:auto; min-width:0; }
.dx-filtros2 > select:first-child { flex:2 1 220px; }
.dx-filtros2 > button { flex:none; }
.dx-ordenable { cursor:pointer; user-select:none; white-space:nowrap; }
.dx-ordenable:hover, .dx-ordenable.activo { color:var(--accent); }
.dx-flecha { display:inline-block; min-width:12px; margin-left:4px; font-size:9px; }
.dx-lista-docs tfoot td { border-top:2px solid var(--line); background:var(--bg,#f4f6f9); padding-top:11px; padding-bottom:11px; white-space:nowrap; }
.dx-lista-docs tr.dx-anulado td { opacity:.55; }
.dx-lista-docs tbody tr:hover, .dx-lista-docs tbody tr:focus { background:var(--accent-soft); outline:none; }
.dx-paginas { display:flex; gap:10px; align-items:center; justify-content:flex-end; margin-top:10px; }
.dx-fondo { position:fixed; inset:0; background:rgba(15,23,42,.45); display:grid; place-items:center; z-index:1000; padding:16px; }
.dx-dialogo { background:var(--surface,#fff); border-radius:16px; width:100%; box-shadow:0 20px 60px rgba(0,0,0,.25); max-height:90vh; display:flex; flex-direction:column; }
.dx-dialogo-cab { display:flex; justify-content:space-between; align-items:center; padding:14px 18px; border-bottom:1px solid var(--line); }
.dx-dialogo-cuerpo { padding:14px 18px; overflow:auto; }
.dx-dialogo-pie { display:flex; justify-content:flex-end; gap:8px; padding:12px 18px; border-top:1px solid var(--line); }
@media (max-width: 900px) { .dx-cabecera, .dx-pie { grid-template-columns: 1fr; } .dx-filtros { grid-template-columns: 1fr 1fr; } .dx-rejilla { overflow-x:auto; } .dx-rejilla table { min-width: 860px; } }
`;
function Qm() {
  if (document.getElementById("dx-estilos")) return;
  const e = document.createElement("style");
  e.id = "dx-estilos", e.textContent = Wm, document.head.appendChild(e);
}
function Gm(e, t, n) {
  Qm();
  const r = Pd(e);
  return r.render(/* @__PURE__ */ l.jsx(Hm, { anfitrion: t, inicial: n })), () => r.unmount();
}
export {
  Gm as montar
};
