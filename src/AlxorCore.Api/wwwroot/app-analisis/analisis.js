var js = { exports: {} }, Sl = {}, Ns = { exports: {} }, O = {};
/**
 * @license React
 * react.production.min.js
 *
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */
var pr = Symbol.for("react.element"), Uc = Symbol.for("react.portal"), Ac = Symbol.for("react.fragment"), Vc = Symbol.for("react.strict_mode"), Hc = Symbol.for("react.profiler"), Bc = Symbol.for("react.provider"), Wc = Symbol.for("react.context"), Qc = Symbol.for("react.forward_ref"), Gc = Symbol.for("react.suspense"), Kc = Symbol.for("react.memo"), Yc = Symbol.for("react.lazy"), sa = Symbol.iterator;
function Xc(e) {
  return e === null || typeof e != "object" ? null : (e = sa && e[sa] || e["@@iterator"], typeof e == "function" ? e : null);
}
var Cs = { isMounted: function() {
  return !1;
}, enqueueForceUpdate: function() {
}, enqueueReplaceState: function() {
}, enqueueSetState: function() {
} }, Es = Object.assign, _s = {};
function Cn(e, t, n) {
  this.props = e, this.context = t, this.refs = _s, this.updater = n || Cs;
}
Cn.prototype.isReactComponent = {};
Cn.prototype.setState = function(e, t) {
  if (typeof e != "object" && typeof e != "function" && e != null) throw Error("setState(...): takes an object of state variables to update or a function which returns an object of state variables.");
  this.updater.enqueueSetState(this, e, t, "setState");
};
Cn.prototype.forceUpdate = function(e) {
  this.updater.enqueueForceUpdate(this, e, "forceUpdate");
};
function zs() {
}
zs.prototype = Cn.prototype;
function ci(e, t, n) {
  this.props = e, this.context = t, this.refs = _s, this.updater = n || Cs;
}
var di = ci.prototype = new zs();
di.constructor = ci;
Es(di, Cn.prototype);
di.isPureReactComponent = !0;
var ua = Array.isArray, Ps = Object.prototype.hasOwnProperty, fi = { current: null }, Ts = { key: !0, ref: !0, __self: !0, __source: !0 };
function Ls(e, t, n) {
  var r, l = {}, o = null, i = null;
  if (t != null) for (r in t.ref !== void 0 && (i = t.ref), t.key !== void 0 && (o = "" + t.key), t) Ps.call(t, r) && !Ts.hasOwnProperty(r) && (l[r] = t[r]);
  var a = arguments.length - 2;
  if (a === 1) l.children = n;
  else if (1 < a) {
    for (var s = Array(a), f = 0; f < a; f++) s[f] = arguments[f + 2];
    l.children = s;
  }
  if (e && e.defaultProps) for (r in a = e.defaultProps, a) l[r] === void 0 && (l[r] = a[r]);
  return { $$typeof: pr, type: e, key: o, ref: i, props: l, _owner: fi.current };
}
function Zc(e, t) {
  return { $$typeof: pr, type: e.type, key: t, ref: e.ref, props: e.props, _owner: e._owner };
}
function pi(e) {
  return typeof e == "object" && e !== null && e.$$typeof === pr;
}
function Jc(e) {
  var t = { "=": "=0", ":": "=2" };
  return "$" + e.replace(/[=:]/g, function(n) {
    return t[n];
  });
}
var ca = /\/+/g;
function Al(e, t) {
  return typeof e == "object" && e !== null && e.key != null ? Jc("" + e.key) : t.toString(36);
}
function Ur(e, t, n, r, l) {
  var o = typeof e;
  (o === "undefined" || o === "boolean") && (e = null);
  var i = !1;
  if (e === null) i = !0;
  else switch (o) {
    case "string":
    case "number":
      i = !0;
      break;
    case "object":
      switch (e.$$typeof) {
        case pr:
        case Uc:
          i = !0;
      }
  }
  if (i) return i = e, l = l(i), e = r === "" ? "." + Al(i, 0) : r, ua(l) ? (n = "", e != null && (n = e.replace(ca, "$&/") + "/"), Ur(l, t, n, "", function(f) {
    return f;
  })) : l != null && (pi(l) && (l = Zc(l, n + (!l.key || i && i.key === l.key ? "" : ("" + l.key).replace(ca, "$&/") + "/") + e)), t.push(l)), 1;
  if (i = 0, r = r === "" ? "." : r + ":", ua(e)) for (var a = 0; a < e.length; a++) {
    o = e[a];
    var s = r + Al(o, a);
    i += Ur(o, t, n, s, l);
  }
  else if (s = Xc(e), typeof s == "function") for (e = s.call(e), a = 0; !(o = e.next()).done; ) o = o.value, s = r + Al(o, a++), i += Ur(o, t, n, s, l);
  else if (o === "object") throw t = String(e), Error("Objects are not valid as a React child (found: " + (t === "[object Object]" ? "object with keys {" + Object.keys(e).join(", ") + "}" : t) + "). If you meant to render a collection of children, use an array instead.");
  return i;
}
function kr(e, t, n) {
  if (e == null) return e;
  var r = [], l = 0;
  return Ur(e, r, "", "", function(o) {
    return t.call(n, o, l++);
  }), r;
}
function qc(e) {
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
var ke = { current: null }, Ar = { transition: null }, bc = { ReactCurrentDispatcher: ke, ReactCurrentBatchConfig: Ar, ReactCurrentOwner: fi };
function Ms() {
  throw Error("act(...) is not supported in production builds of React.");
}
O.Children = { map: kr, forEach: function(e, t, n) {
  kr(e, function() {
    t.apply(this, arguments);
  }, n);
}, count: function(e) {
  var t = 0;
  return kr(e, function() {
    t++;
  }), t;
}, toArray: function(e) {
  return kr(e, function(t) {
    return t;
  }) || [];
}, only: function(e) {
  if (!pi(e)) throw Error("React.Children.only expected to receive a single React element child.");
  return e;
} };
O.Component = Cn;
O.Fragment = Ac;
O.Profiler = Hc;
O.PureComponent = ci;
O.StrictMode = Vc;
O.Suspense = Gc;
O.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED = bc;
O.act = Ms;
O.cloneElement = function(e, t, n) {
  if (e == null) throw Error("React.cloneElement(...): The argument must be a React element, but you passed " + e + ".");
  var r = Es({}, e.props), l = e.key, o = e.ref, i = e._owner;
  if (t != null) {
    if (t.ref !== void 0 && (o = t.ref, i = fi.current), t.key !== void 0 && (l = "" + t.key), e.type && e.type.defaultProps) var a = e.type.defaultProps;
    for (s in t) Ps.call(t, s) && !Ts.hasOwnProperty(s) && (r[s] = t[s] === void 0 && a !== void 0 ? a[s] : t[s]);
  }
  var s = arguments.length - 2;
  if (s === 1) r.children = n;
  else if (1 < s) {
    a = Array(s);
    for (var f = 0; f < s; f++) a[f] = arguments[f + 2];
    r.children = a;
  }
  return { $$typeof: pr, type: e.type, key: l, ref: o, props: r, _owner: i };
};
O.createContext = function(e) {
  return e = { $$typeof: Wc, _currentValue: e, _currentValue2: e, _threadCount: 0, Provider: null, Consumer: null, _defaultValue: null, _globalName: null }, e.Provider = { $$typeof: Bc, _context: e }, e.Consumer = e;
};
O.createElement = Ls;
O.createFactory = function(e) {
  var t = Ls.bind(null, e);
  return t.type = e, t;
};
O.createRef = function() {
  return { current: null };
};
O.forwardRef = function(e) {
  return { $$typeof: Qc, render: e };
};
O.isValidElement = pi;
O.lazy = function(e) {
  return { $$typeof: Yc, _payload: { _status: -1, _result: e }, _init: qc };
};
O.memo = function(e, t) {
  return { $$typeof: Kc, type: e, compare: t === void 0 ? null : t };
};
O.startTransition = function(e) {
  var t = Ar.transition;
  Ar.transition = {};
  try {
    e();
  } finally {
    Ar.transition = t;
  }
};
O.unstable_act = Ms;
O.useCallback = function(e, t) {
  return ke.current.useCallback(e, t);
};
O.useContext = function(e) {
  return ke.current.useContext(e);
};
O.useDebugValue = function() {
};
O.useDeferredValue = function(e) {
  return ke.current.useDeferredValue(e);
};
O.useEffect = function(e, t) {
  return ke.current.useEffect(e, t);
};
O.useId = function() {
  return ke.current.useId();
};
O.useImperativeHandle = function(e, t, n) {
  return ke.current.useImperativeHandle(e, t, n);
};
O.useInsertionEffect = function(e, t) {
  return ke.current.useInsertionEffect(e, t);
};
O.useLayoutEffect = function(e, t) {
  return ke.current.useLayoutEffect(e, t);
};
O.useMemo = function(e, t) {
  return ke.current.useMemo(e, t);
};
O.useReducer = function(e, t, n) {
  return ke.current.useReducer(e, t, n);
};
O.useRef = function(e) {
  return ke.current.useRef(e);
};
O.useState = function(e) {
  return ke.current.useState(e);
};
O.useSyncExternalStore = function(e, t, n) {
  return ke.current.useSyncExternalStore(e, t, n);
};
O.useTransition = function() {
  return ke.current.useTransition();
};
O.version = "18.3.1";
Ns.exports = O;
var A = Ns.exports;
/**
 * @license React
 * react-jsx-runtime.production.min.js
 *
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */
var ed = A, td = Symbol.for("react.element"), nd = Symbol.for("react.fragment"), rd = Object.prototype.hasOwnProperty, ld = ed.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED.ReactCurrentOwner, od = { key: !0, ref: !0, __self: !0, __source: !0 };
function Ds(e, t, n) {
  var r, l = {}, o = null, i = null;
  n !== void 0 && (o = "" + n), t.key !== void 0 && (o = "" + t.key), t.ref !== void 0 && (i = t.ref);
  for (r in t) rd.call(t, r) && !od.hasOwnProperty(r) && (l[r] = t[r]);
  if (e && e.defaultProps) for (r in t = e.defaultProps, t) l[r] === void 0 && (l[r] = t[r]);
  return { $$typeof: td, type: e, key: o, ref: i, props: l, _owner: ld.current };
}
Sl.Fragment = nd;
Sl.jsx = Ds;
Sl.jsxs = Ds;
js.exports = Sl;
var u = js.exports, Rs = { exports: {} }, De = {}, Fs = { exports: {} }, $s = {};
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
  function t(z, R) {
    var F = z.length;
    z.push(R);
    e: for (; 0 < F; ) {
      var G = F - 1 >>> 1, b = z[G];
      if (0 < l(b, R)) z[G] = R, z[F] = b, F = G;
      else break e;
    }
  }
  function n(z) {
    return z.length === 0 ? null : z[0];
  }
  function r(z) {
    if (z.length === 0) return null;
    var R = z[0], F = z.pop();
    if (F !== R) {
      z[0] = F;
      e: for (var G = 0, b = z.length, Rt = b >>> 1; G < Rt; ) {
        var Xe = 2 * (G + 1) - 1, Zt = z[Xe], Ze = Xe + 1, Ft = z[Ze];
        if (0 > l(Zt, F)) Ze < b && 0 > l(Ft, Zt) ? (z[G] = Ft, z[Ze] = F, G = Ze) : (z[G] = Zt, z[Xe] = F, G = Xe);
        else if (Ze < b && 0 > l(Ft, F)) z[G] = Ft, z[Ze] = F, G = Ze;
        else break e;
      }
    }
    return R;
  }
  function l(z, R) {
    var F = z.sortIndex - R.sortIndex;
    return F !== 0 ? F : z.id - R.id;
  }
  if (typeof performance == "object" && typeof performance.now == "function") {
    var o = performance;
    e.unstable_now = function() {
      return o.now();
    };
  } else {
    var i = Date, a = i.now();
    e.unstable_now = function() {
      return i.now() - a;
    };
  }
  var s = [], f = [], h = 1, v = null, p = 3, g = !1, w = !1, k = !1, N = typeof setTimeout == "function" ? setTimeout : null, d = typeof clearTimeout == "function" ? clearTimeout : null, c = typeof setImmediate < "u" ? setImmediate : null;
  typeof navigator < "u" && navigator.scheduling !== void 0 && navigator.scheduling.isInputPending !== void 0 && navigator.scheduling.isInputPending.bind(navigator.scheduling);
  function m(z) {
    for (var R = n(f); R !== null; ) {
      if (R.callback === null) r(f);
      else if (R.startTime <= z) r(f), R.sortIndex = R.expirationTime, t(s, R);
      else break;
      R = n(f);
    }
  }
  function x(z) {
    if (k = !1, m(z), !w) if (n(s) !== null) w = !0, pe(C);
    else {
      var R = n(f);
      R !== null && ft(x, R.startTime - z);
    }
  }
  function C(z, R) {
    w = !1, k && (k = !1, d(_), _ = -1), g = !0;
    var F = p;
    try {
      for (m(R), v = n(s); v !== null && (!(v.expirationTime > R) || z && !M()); ) {
        var G = v.callback;
        if (typeof G == "function") {
          v.callback = null, p = v.priorityLevel;
          var b = G(v.expirationTime <= R);
          R = e.unstable_now(), typeof b == "function" ? v.callback = b : v === n(s) && r(s), m(R);
        } else r(s);
        v = n(s);
      }
      if (v !== null) var Rt = !0;
      else {
        var Xe = n(f);
        Xe !== null && ft(x, Xe.startTime - R), Rt = !1;
      }
      return Rt;
    } finally {
      v = null, p = F, g = !1;
    }
  }
  var E = !1, S = null, _ = -1, L = 5, D = -1;
  function M() {
    return !(e.unstable_now() - D < L);
  }
  function Q() {
    if (S !== null) {
      var z = e.unstable_now();
      D = z;
      var R = !0;
      try {
        R = S(!0, z);
      } finally {
        R ? Fe() : (E = !1, S = null);
      }
    } else E = !1;
  }
  var Fe;
  if (typeof c == "function") Fe = function() {
    c(Q);
  };
  else if (typeof MessageChannel < "u") {
    var Dt = new MessageChannel(), fe = Dt.port2;
    Dt.port1.onmessage = Q, Fe = function() {
      fe.postMessage(null);
    };
  } else Fe = function() {
    N(Q, 0);
  };
  function pe(z) {
    S = z, E || (E = !0, Fe());
  }
  function ft(z, R) {
    _ = N(function() {
      z(e.unstable_now());
    }, R);
  }
  e.unstable_IdlePriority = 5, e.unstable_ImmediatePriority = 1, e.unstable_LowPriority = 4, e.unstable_NormalPriority = 3, e.unstable_Profiling = null, e.unstable_UserBlockingPriority = 2, e.unstable_cancelCallback = function(z) {
    z.callback = null;
  }, e.unstable_continueExecution = function() {
    w || g || (w = !0, pe(C));
  }, e.unstable_forceFrameRate = function(z) {
    0 > z || 125 < z ? console.error("forceFrameRate takes a positive int between 0 and 125, forcing frame rates higher than 125 fps is not supported") : L = 0 < z ? Math.floor(1e3 / z) : 5;
  }, e.unstable_getCurrentPriorityLevel = function() {
    return p;
  }, e.unstable_getFirstCallbackNode = function() {
    return n(s);
  }, e.unstable_next = function(z) {
    switch (p) {
      case 1:
      case 2:
      case 3:
        var R = 3;
        break;
      default:
        R = p;
    }
    var F = p;
    p = R;
    try {
      return z();
    } finally {
      p = F;
    }
  }, e.unstable_pauseExecution = function() {
  }, e.unstable_requestPaint = function() {
  }, e.unstable_runWithPriority = function(z, R) {
    switch (z) {
      case 1:
      case 2:
      case 3:
      case 4:
      case 5:
        break;
      default:
        z = 3;
    }
    var F = p;
    p = z;
    try {
      return R();
    } finally {
      p = F;
    }
  }, e.unstable_scheduleCallback = function(z, R, F) {
    var G = e.unstable_now();
    switch (typeof F == "object" && F !== null ? (F = F.delay, F = typeof F == "number" && 0 < F ? G + F : G) : F = G, z) {
      case 1:
        var b = -1;
        break;
      case 2:
        b = 250;
        break;
      case 5:
        b = 1073741823;
        break;
      case 4:
        b = 1e4;
        break;
      default:
        b = 5e3;
    }
    return b = F + b, z = { id: h++, callback: R, priorityLevel: z, startTime: F, expirationTime: b, sortIndex: -1 }, F > G ? (z.sortIndex = F, t(f, z), n(s) === null && z === n(f) && (k ? (d(_), _ = -1) : k = !0, ft(x, F - G))) : (z.sortIndex = b, t(s, z), w || g || (w = !0, pe(C))), z;
  }, e.unstable_shouldYield = M, e.unstable_wrapCallback = function(z) {
    var R = p;
    return function() {
      var F = p;
      p = R;
      try {
        return z.apply(this, arguments);
      } finally {
        p = F;
      }
    };
  };
})($s);
Fs.exports = $s;
var id = Fs.exports;
/**
 * @license React
 * react-dom.production.min.js
 *
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */
var ad = A, Me = id;
function j(e) {
  for (var t = "https://reactjs.org/docs/error-decoder.html?invariant=" + e, n = 1; n < arguments.length; n++) t += "&args[]=" + encodeURIComponent(arguments[n]);
  return "Minified React error #" + e + "; visit " + t + " for the full message or use the non-minified dev environment for full errors and additional helpful warnings.";
}
var Os = /* @__PURE__ */ new Set(), Zn = {};
function Yt(e, t) {
  xn(e, t), xn(e + "Capture", t);
}
function xn(e, t) {
  for (Zn[e] = t, e = 0; e < t.length; e++) Os.add(t[e]);
}
var at = !(typeof window > "u" || typeof window.document > "u" || typeof window.document.createElement > "u"), go = Object.prototype.hasOwnProperty, sd = /^[:A-Z_a-z\u00C0-\u00D6\u00D8-\u00F6\u00F8-\u02FF\u0370-\u037D\u037F-\u1FFF\u200C-\u200D\u2070-\u218F\u2C00-\u2FEF\u3001-\uD7FF\uF900-\uFDCF\uFDF0-\uFFFD][:A-Z_a-z\u00C0-\u00D6\u00D8-\u00F6\u00F8-\u02FF\u0370-\u037D\u037F-\u1FFF\u200C-\u200D\u2070-\u218F\u2C00-\u2FEF\u3001-\uD7FF\uF900-\uFDCF\uFDF0-\uFFFD\-.0-9\u00B7\u0300-\u036F\u203F-\u2040]*$/, da = {}, fa = {};
function ud(e) {
  return go.call(fa, e) ? !0 : go.call(da, e) ? !1 : sd.test(e) ? fa[e] = !0 : (da[e] = !0, !1);
}
function cd(e, t, n, r) {
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
function dd(e, t, n, r) {
  if (t === null || typeof t > "u" || cd(e, t, n, r)) return !0;
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
function Se(e, t, n, r, l, o, i) {
  this.acceptsBooleans = t === 2 || t === 3 || t === 4, this.attributeName = r, this.attributeNamespace = l, this.mustUseProperty = n, this.propertyName = e, this.type = t, this.sanitizeURL = o, this.removeEmptyString = i;
}
var de = {};
"children dangerouslySetInnerHTML defaultValue defaultChecked innerHTML suppressContentEditableWarning suppressHydrationWarning style".split(" ").forEach(function(e) {
  de[e] = new Se(e, 0, !1, e, null, !1, !1);
});
[["acceptCharset", "accept-charset"], ["className", "class"], ["htmlFor", "for"], ["httpEquiv", "http-equiv"]].forEach(function(e) {
  var t = e[0];
  de[t] = new Se(t, 1, !1, e[1], null, !1, !1);
});
["contentEditable", "draggable", "spellCheck", "value"].forEach(function(e) {
  de[e] = new Se(e, 2, !1, e.toLowerCase(), null, !1, !1);
});
["autoReverse", "externalResourcesRequired", "focusable", "preserveAlpha"].forEach(function(e) {
  de[e] = new Se(e, 2, !1, e, null, !1, !1);
});
"allowFullScreen async autoFocus autoPlay controls default defer disabled disablePictureInPicture disableRemotePlayback formNoValidate hidden loop noModule noValidate open playsInline readOnly required reversed scoped seamless itemScope".split(" ").forEach(function(e) {
  de[e] = new Se(e, 3, !1, e.toLowerCase(), null, !1, !1);
});
["checked", "multiple", "muted", "selected"].forEach(function(e) {
  de[e] = new Se(e, 3, !0, e, null, !1, !1);
});
["capture", "download"].forEach(function(e) {
  de[e] = new Se(e, 4, !1, e, null, !1, !1);
});
["cols", "rows", "size", "span"].forEach(function(e) {
  de[e] = new Se(e, 6, !1, e, null, !1, !1);
});
["rowSpan", "start"].forEach(function(e) {
  de[e] = new Se(e, 5, !1, e.toLowerCase(), null, !1, !1);
});
var mi = /[\-:]([a-z])/g;
function hi(e) {
  return e[1].toUpperCase();
}
"accent-height alignment-baseline arabic-form baseline-shift cap-height clip-path clip-rule color-interpolation color-interpolation-filters color-profile color-rendering dominant-baseline enable-background fill-opacity fill-rule flood-color flood-opacity font-family font-size font-size-adjust font-stretch font-style font-variant font-weight glyph-name glyph-orientation-horizontal glyph-orientation-vertical horiz-adv-x horiz-origin-x image-rendering letter-spacing lighting-color marker-end marker-mid marker-start overline-position overline-thickness paint-order panose-1 pointer-events rendering-intent shape-rendering stop-color stop-opacity strikethrough-position strikethrough-thickness stroke-dasharray stroke-dashoffset stroke-linecap stroke-linejoin stroke-miterlimit stroke-opacity stroke-width text-anchor text-decoration text-rendering underline-position underline-thickness unicode-bidi unicode-range units-per-em v-alphabetic v-hanging v-ideographic v-mathematical vector-effect vert-adv-y vert-origin-x vert-origin-y word-spacing writing-mode xmlns:xlink x-height".split(" ").forEach(function(e) {
  var t = e.replace(
    mi,
    hi
  );
  de[t] = new Se(t, 1, !1, e, null, !1, !1);
});
"xlink:actuate xlink:arcrole xlink:role xlink:show xlink:title xlink:type".split(" ").forEach(function(e) {
  var t = e.replace(mi, hi);
  de[t] = new Se(t, 1, !1, e, "http://www.w3.org/1999/xlink", !1, !1);
});
["xml:base", "xml:lang", "xml:space"].forEach(function(e) {
  var t = e.replace(mi, hi);
  de[t] = new Se(t, 1, !1, e, "http://www.w3.org/XML/1998/namespace", !1, !1);
});
["tabIndex", "crossOrigin"].forEach(function(e) {
  de[e] = new Se(e, 1, !1, e.toLowerCase(), null, !1, !1);
});
de.xlinkHref = new Se("xlinkHref", 1, !1, "xlink:href", "http://www.w3.org/1999/xlink", !0, !1);
["src", "href", "action", "formAction"].forEach(function(e) {
  de[e] = new Se(e, 1, !1, e.toLowerCase(), null, !0, !0);
});
function vi(e, t, n, r) {
  var l = de.hasOwnProperty(t) ? de[t] : null;
  (l !== null ? l.type !== 0 : r || !(2 < t.length) || t[0] !== "o" && t[0] !== "O" || t[1] !== "n" && t[1] !== "N") && (dd(t, n, l, r) && (n = null), r || l === null ? ud(t) && (n === null ? e.removeAttribute(t) : e.setAttribute(t, "" + n)) : l.mustUseProperty ? e[l.propertyName] = n === null ? l.type === 3 ? !1 : "" : n : (t = l.attributeName, r = l.attributeNamespace, n === null ? e.removeAttribute(t) : (l = l.type, n = l === 3 || l === 4 && n === !0 ? "" : "" + n, r ? e.setAttributeNS(r, t, n) : e.setAttribute(t, n))));
}
var dt = ad.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED, Sr = Symbol.for("react.element"), bt = Symbol.for("react.portal"), en = Symbol.for("react.fragment"), gi = Symbol.for("react.strict_mode"), xo = Symbol.for("react.profiler"), Is = Symbol.for("react.provider"), Us = Symbol.for("react.context"), xi = Symbol.for("react.forward_ref"), yo = Symbol.for("react.suspense"), wo = Symbol.for("react.suspense_list"), yi = Symbol.for("react.memo"), mt = Symbol.for("react.lazy"), As = Symbol.for("react.offscreen"), pa = Symbol.iterator;
function zn(e) {
  return e === null || typeof e != "object" ? null : (e = pa && e[pa] || e["@@iterator"], typeof e == "function" ? e : null);
}
var Z = Object.assign, Vl;
function On(e) {
  if (Vl === void 0) try {
    throw Error();
  } catch (n) {
    var t = n.stack.trim().match(/\n( *(at )?)/);
    Vl = t && t[1] || "";
  }
  return `
` + Vl + e;
}
var Hl = !1;
function Bl(e, t) {
  if (!e || Hl) return "";
  Hl = !0;
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
      for (var l = f.stack.split(`
`), o = r.stack.split(`
`), i = l.length - 1, a = o.length - 1; 1 <= i && 0 <= a && l[i] !== o[a]; ) a--;
      for (; 1 <= i && 0 <= a; i--, a--) if (l[i] !== o[a]) {
        if (i !== 1 || a !== 1)
          do
            if (i--, a--, 0 > a || l[i] !== o[a]) {
              var s = `
` + l[i].replace(" at new ", " at ");
              return e.displayName && s.includes("<anonymous>") && (s = s.replace("<anonymous>", e.displayName)), s;
            }
          while (1 <= i && 0 <= a);
        break;
      }
    }
  } finally {
    Hl = !1, Error.prepareStackTrace = n;
  }
  return (e = e ? e.displayName || e.name : "") ? On(e) : "";
}
function fd(e) {
  switch (e.tag) {
    case 5:
      return On(e.type);
    case 16:
      return On("Lazy");
    case 13:
      return On("Suspense");
    case 19:
      return On("SuspenseList");
    case 0:
    case 2:
    case 15:
      return e = Bl(e.type, !1), e;
    case 11:
      return e = Bl(e.type.render, !1), e;
    case 1:
      return e = Bl(e.type, !0), e;
    default:
      return "";
  }
}
function ko(e) {
  if (e == null) return null;
  if (typeof e == "function") return e.displayName || e.name || null;
  if (typeof e == "string") return e;
  switch (e) {
    case en:
      return "Fragment";
    case bt:
      return "Portal";
    case xo:
      return "Profiler";
    case gi:
      return "StrictMode";
    case yo:
      return "Suspense";
    case wo:
      return "SuspenseList";
  }
  if (typeof e == "object") switch (e.$$typeof) {
    case Us:
      return (e.displayName || "Context") + ".Consumer";
    case Is:
      return (e._context.displayName || "Context") + ".Provider";
    case xi:
      var t = e.render;
      return e = e.displayName, e || (e = t.displayName || t.name || "", e = e !== "" ? "ForwardRef(" + e + ")" : "ForwardRef"), e;
    case yi:
      return t = e.displayName || null, t !== null ? t : ko(e.type) || "Memo";
    case mt:
      t = e._payload, e = e._init;
      try {
        return ko(e(t));
      } catch {
      }
  }
  return null;
}
function pd(e) {
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
      return ko(t);
    case 8:
      return t === gi ? "StrictMode" : "Mode";
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
function zt(e) {
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
function Vs(e) {
  var t = e.type;
  return (e = e.nodeName) && e.toLowerCase() === "input" && (t === "checkbox" || t === "radio");
}
function md(e) {
  var t = Vs(e) ? "checked" : "value", n = Object.getOwnPropertyDescriptor(e.constructor.prototype, t), r = "" + e[t];
  if (!e.hasOwnProperty(t) && typeof n < "u" && typeof n.get == "function" && typeof n.set == "function") {
    var l = n.get, o = n.set;
    return Object.defineProperty(e, t, { configurable: !0, get: function() {
      return l.call(this);
    }, set: function(i) {
      r = "" + i, o.call(this, i);
    } }), Object.defineProperty(e, t, { enumerable: n.enumerable }), { getValue: function() {
      return r;
    }, setValue: function(i) {
      r = "" + i;
    }, stopTracking: function() {
      e._valueTracker = null, delete e[t];
    } };
  }
}
function jr(e) {
  e._valueTracker || (e._valueTracker = md(e));
}
function Hs(e) {
  if (!e) return !1;
  var t = e._valueTracker;
  if (!t) return !0;
  var n = t.getValue(), r = "";
  return e && (r = Vs(e) ? e.checked ? "true" : "false" : e.value), e = r, e !== n ? (t.setValue(e), !0) : !1;
}
function qr(e) {
  if (e = e || (typeof document < "u" ? document : void 0), typeof e > "u") return null;
  try {
    return e.activeElement || e.body;
  } catch {
    return e.body;
  }
}
function So(e, t) {
  var n = t.checked;
  return Z({}, t, { defaultChecked: void 0, defaultValue: void 0, value: void 0, checked: n ?? e._wrapperState.initialChecked });
}
function ma(e, t) {
  var n = t.defaultValue == null ? "" : t.defaultValue, r = t.checked != null ? t.checked : t.defaultChecked;
  n = zt(t.value != null ? t.value : n), e._wrapperState = { initialChecked: r, initialValue: n, controlled: t.type === "checkbox" || t.type === "radio" ? t.checked != null : t.value != null };
}
function Bs(e, t) {
  t = t.checked, t != null && vi(e, "checked", t, !1);
}
function jo(e, t) {
  Bs(e, t);
  var n = zt(t.value), r = t.type;
  if (n != null) r === "number" ? (n === 0 && e.value === "" || e.value != n) && (e.value = "" + n) : e.value !== "" + n && (e.value = "" + n);
  else if (r === "submit" || r === "reset") {
    e.removeAttribute("value");
    return;
  }
  t.hasOwnProperty("value") ? No(e, t.type, n) : t.hasOwnProperty("defaultValue") && No(e, t.type, zt(t.defaultValue)), t.checked == null && t.defaultChecked != null && (e.defaultChecked = !!t.defaultChecked);
}
function ha(e, t, n) {
  if (t.hasOwnProperty("value") || t.hasOwnProperty("defaultValue")) {
    var r = t.type;
    if (!(r !== "submit" && r !== "reset" || t.value !== void 0 && t.value !== null)) return;
    t = "" + e._wrapperState.initialValue, n || t === e.value || (e.value = t), e.defaultValue = t;
  }
  n = e.name, n !== "" && (e.name = ""), e.defaultChecked = !!e._wrapperState.initialChecked, n !== "" && (e.name = n);
}
function No(e, t, n) {
  (t !== "number" || qr(e.ownerDocument) !== e) && (n == null ? e.defaultValue = "" + e._wrapperState.initialValue : e.defaultValue !== "" + n && (e.defaultValue = "" + n));
}
var In = Array.isArray;
function fn(e, t, n, r) {
  if (e = e.options, t) {
    t = {};
    for (var l = 0; l < n.length; l++) t["$" + n[l]] = !0;
    for (n = 0; n < e.length; n++) l = t.hasOwnProperty("$" + e[n].value), e[n].selected !== l && (e[n].selected = l), l && r && (e[n].defaultSelected = !0);
  } else {
    for (n = "" + zt(n), t = null, l = 0; l < e.length; l++) {
      if (e[l].value === n) {
        e[l].selected = !0, r && (e[l].defaultSelected = !0);
        return;
      }
      t !== null || e[l].disabled || (t = e[l]);
    }
    t !== null && (t.selected = !0);
  }
}
function Co(e, t) {
  if (t.dangerouslySetInnerHTML != null) throw Error(j(91));
  return Z({}, t, { value: void 0, defaultValue: void 0, children: "" + e._wrapperState.initialValue });
}
function va(e, t) {
  var n = t.value;
  if (n == null) {
    if (n = t.children, t = t.defaultValue, n != null) {
      if (t != null) throw Error(j(92));
      if (In(n)) {
        if (1 < n.length) throw Error(j(93));
        n = n[0];
      }
      t = n;
    }
    t == null && (t = ""), n = t;
  }
  e._wrapperState = { initialValue: zt(n) };
}
function Ws(e, t) {
  var n = zt(t.value), r = zt(t.defaultValue);
  n != null && (n = "" + n, n !== e.value && (e.value = n), t.defaultValue == null && e.defaultValue !== n && (e.defaultValue = n)), r != null && (e.defaultValue = "" + r);
}
function ga(e) {
  var t = e.textContent;
  t === e._wrapperState.initialValue && t !== "" && t !== null && (e.value = t);
}
function Qs(e) {
  switch (e) {
    case "svg":
      return "http://www.w3.org/2000/svg";
    case "math":
      return "http://www.w3.org/1998/Math/MathML";
    default:
      return "http://www.w3.org/1999/xhtml";
  }
}
function Eo(e, t) {
  return e == null || e === "http://www.w3.org/1999/xhtml" ? Qs(t) : e === "http://www.w3.org/2000/svg" && t === "foreignObject" ? "http://www.w3.org/1999/xhtml" : e;
}
var Nr, Gs = function(e) {
  return typeof MSApp < "u" && MSApp.execUnsafeLocalFunction ? function(t, n, r, l) {
    MSApp.execUnsafeLocalFunction(function() {
      return e(t, n, r, l);
    });
  } : e;
}(function(e, t) {
  if (e.namespaceURI !== "http://www.w3.org/2000/svg" || "innerHTML" in e) e.innerHTML = t;
  else {
    for (Nr = Nr || document.createElement("div"), Nr.innerHTML = "<svg>" + t.valueOf().toString() + "</svg>", t = Nr.firstChild; e.firstChild; ) e.removeChild(e.firstChild);
    for (; t.firstChild; ) e.appendChild(t.firstChild);
  }
});
function Jn(e, t) {
  if (t) {
    var n = e.firstChild;
    if (n && n === e.lastChild && n.nodeType === 3) {
      n.nodeValue = t;
      return;
    }
  }
  e.textContent = t;
}
var Vn = {
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
}, hd = ["Webkit", "ms", "Moz", "O"];
Object.keys(Vn).forEach(function(e) {
  hd.forEach(function(t) {
    t = t + e.charAt(0).toUpperCase() + e.substring(1), Vn[t] = Vn[e];
  });
});
function Ks(e, t, n) {
  return t == null || typeof t == "boolean" || t === "" ? "" : n || typeof t != "number" || t === 0 || Vn.hasOwnProperty(e) && Vn[e] ? ("" + t).trim() : t + "px";
}
function Ys(e, t) {
  e = e.style;
  for (var n in t) if (t.hasOwnProperty(n)) {
    var r = n.indexOf("--") === 0, l = Ks(n, t[n], r);
    n === "float" && (n = "cssFloat"), r ? e.setProperty(n, l) : e[n] = l;
  }
}
var vd = Z({ menuitem: !0 }, { area: !0, base: !0, br: !0, col: !0, embed: !0, hr: !0, img: !0, input: !0, keygen: !0, link: !0, meta: !0, param: !0, source: !0, track: !0, wbr: !0 });
function _o(e, t) {
  if (t) {
    if (vd[e] && (t.children != null || t.dangerouslySetInnerHTML != null)) throw Error(j(137, e));
    if (t.dangerouslySetInnerHTML != null) {
      if (t.children != null) throw Error(j(60));
      if (typeof t.dangerouslySetInnerHTML != "object" || !("__html" in t.dangerouslySetInnerHTML)) throw Error(j(61));
    }
    if (t.style != null && typeof t.style != "object") throw Error(j(62));
  }
}
function zo(e, t) {
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
var Po = null;
function wi(e) {
  return e = e.target || e.srcElement || window, e.correspondingUseElement && (e = e.correspondingUseElement), e.nodeType === 3 ? e.parentNode : e;
}
var To = null, pn = null, mn = null;
function xa(e) {
  if (e = vr(e)) {
    if (typeof To != "function") throw Error(j(280));
    var t = e.stateNode;
    t && (t = _l(t), To(e.stateNode, e.type, t));
  }
}
function Xs(e) {
  pn ? mn ? mn.push(e) : mn = [e] : pn = e;
}
function Zs() {
  if (pn) {
    var e = pn, t = mn;
    if (mn = pn = null, xa(e), t) for (e = 0; e < t.length; e++) xa(t[e]);
  }
}
function Js(e, t) {
  return e(t);
}
function qs() {
}
var Wl = !1;
function bs(e, t, n) {
  if (Wl) return e(t, n);
  Wl = !0;
  try {
    return Js(e, t, n);
  } finally {
    Wl = !1, (pn !== null || mn !== null) && (qs(), Zs());
  }
}
function qn(e, t) {
  var n = e.stateNode;
  if (n === null) return null;
  var r = _l(n);
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
  if (n && typeof n != "function") throw Error(j(231, t, typeof n));
  return n;
}
var Lo = !1;
if (at) try {
  var Pn = {};
  Object.defineProperty(Pn, "passive", { get: function() {
    Lo = !0;
  } }), window.addEventListener("test", Pn, Pn), window.removeEventListener("test", Pn, Pn);
} catch {
  Lo = !1;
}
function gd(e, t, n, r, l, o, i, a, s) {
  var f = Array.prototype.slice.call(arguments, 3);
  try {
    t.apply(n, f);
  } catch (h) {
    this.onError(h);
  }
}
var Hn = !1, br = null, el = !1, Mo = null, xd = { onError: function(e) {
  Hn = !0, br = e;
} };
function yd(e, t, n, r, l, o, i, a, s) {
  Hn = !1, br = null, gd.apply(xd, arguments);
}
function wd(e, t, n, r, l, o, i, a, s) {
  if (yd.apply(this, arguments), Hn) {
    if (Hn) {
      var f = br;
      Hn = !1, br = null;
    } else throw Error(j(198));
    el || (el = !0, Mo = f);
  }
}
function Xt(e) {
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
function eu(e) {
  if (e.tag === 13) {
    var t = e.memoizedState;
    if (t === null && (e = e.alternate, e !== null && (t = e.memoizedState)), t !== null) return t.dehydrated;
  }
  return null;
}
function ya(e) {
  if (Xt(e) !== e) throw Error(j(188));
}
function kd(e) {
  var t = e.alternate;
  if (!t) {
    if (t = Xt(e), t === null) throw Error(j(188));
    return t !== e ? null : e;
  }
  for (var n = e, r = t; ; ) {
    var l = n.return;
    if (l === null) break;
    var o = l.alternate;
    if (o === null) {
      if (r = l.return, r !== null) {
        n = r;
        continue;
      }
      break;
    }
    if (l.child === o.child) {
      for (o = l.child; o; ) {
        if (o === n) return ya(l), e;
        if (o === r) return ya(l), t;
        o = o.sibling;
      }
      throw Error(j(188));
    }
    if (n.return !== r.return) n = l, r = o;
    else {
      for (var i = !1, a = l.child; a; ) {
        if (a === n) {
          i = !0, n = l, r = o;
          break;
        }
        if (a === r) {
          i = !0, r = l, n = o;
          break;
        }
        a = a.sibling;
      }
      if (!i) {
        for (a = o.child; a; ) {
          if (a === n) {
            i = !0, n = o, r = l;
            break;
          }
          if (a === r) {
            i = !0, r = o, n = l;
            break;
          }
          a = a.sibling;
        }
        if (!i) throw Error(j(189));
      }
    }
    if (n.alternate !== r) throw Error(j(190));
  }
  if (n.tag !== 3) throw Error(j(188));
  return n.stateNode.current === n ? e : t;
}
function tu(e) {
  return e = kd(e), e !== null ? nu(e) : null;
}
function nu(e) {
  if (e.tag === 5 || e.tag === 6) return e;
  for (e = e.child; e !== null; ) {
    var t = nu(e);
    if (t !== null) return t;
    e = e.sibling;
  }
  return null;
}
var ru = Me.unstable_scheduleCallback, wa = Me.unstable_cancelCallback, Sd = Me.unstable_shouldYield, jd = Me.unstable_requestPaint, ee = Me.unstable_now, Nd = Me.unstable_getCurrentPriorityLevel, ki = Me.unstable_ImmediatePriority, lu = Me.unstable_UserBlockingPriority, tl = Me.unstable_NormalPriority, Cd = Me.unstable_LowPriority, ou = Me.unstable_IdlePriority, jl = null, et = null;
function Ed(e) {
  if (et && typeof et.onCommitFiberRoot == "function") try {
    et.onCommitFiberRoot(jl, e, void 0, (e.current.flags & 128) === 128);
  } catch {
  }
}
var Ge = Math.clz32 ? Math.clz32 : Pd, _d = Math.log, zd = Math.LN2;
function Pd(e) {
  return e >>>= 0, e === 0 ? 32 : 31 - (_d(e) / zd | 0) | 0;
}
var Cr = 64, Er = 4194304;
function Un(e) {
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
function nl(e, t) {
  var n = e.pendingLanes;
  if (n === 0) return 0;
  var r = 0, l = e.suspendedLanes, o = e.pingedLanes, i = n & 268435455;
  if (i !== 0) {
    var a = i & ~l;
    a !== 0 ? r = Un(a) : (o &= i, o !== 0 && (r = Un(o)));
  } else i = n & ~l, i !== 0 ? r = Un(i) : o !== 0 && (r = Un(o));
  if (r === 0) return 0;
  if (t !== 0 && t !== r && !(t & l) && (l = r & -r, o = t & -t, l >= o || l === 16 && (o & 4194240) !== 0)) return t;
  if (r & 4 && (r |= n & 16), t = e.entangledLanes, t !== 0) for (e = e.entanglements, t &= r; 0 < t; ) n = 31 - Ge(t), l = 1 << n, r |= e[n], t &= ~l;
  return r;
}
function Td(e, t) {
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
function Ld(e, t) {
  for (var n = e.suspendedLanes, r = e.pingedLanes, l = e.expirationTimes, o = e.pendingLanes; 0 < o; ) {
    var i = 31 - Ge(o), a = 1 << i, s = l[i];
    s === -1 ? (!(a & n) || a & r) && (l[i] = Td(a, t)) : s <= t && (e.expiredLanes |= a), o &= ~a;
  }
}
function Do(e) {
  return e = e.pendingLanes & -1073741825, e !== 0 ? e : e & 1073741824 ? 1073741824 : 0;
}
function iu() {
  var e = Cr;
  return Cr <<= 1, !(Cr & 4194240) && (Cr = 64), e;
}
function Ql(e) {
  for (var t = [], n = 0; 31 > n; n++) t.push(e);
  return t;
}
function mr(e, t, n) {
  e.pendingLanes |= t, t !== 536870912 && (e.suspendedLanes = 0, e.pingedLanes = 0), e = e.eventTimes, t = 31 - Ge(t), e[t] = n;
}
function Md(e, t) {
  var n = e.pendingLanes & ~t;
  e.pendingLanes = t, e.suspendedLanes = 0, e.pingedLanes = 0, e.expiredLanes &= t, e.mutableReadLanes &= t, e.entangledLanes &= t, t = e.entanglements;
  var r = e.eventTimes;
  for (e = e.expirationTimes; 0 < n; ) {
    var l = 31 - Ge(n), o = 1 << l;
    t[l] = 0, r[l] = -1, e[l] = -1, n &= ~o;
  }
}
function Si(e, t) {
  var n = e.entangledLanes |= t;
  for (e = e.entanglements; n; ) {
    var r = 31 - Ge(n), l = 1 << r;
    l & t | e[r] & t && (e[r] |= t), n &= ~l;
  }
}
var V = 0;
function au(e) {
  return e &= -e, 1 < e ? 4 < e ? e & 268435455 ? 16 : 536870912 : 4 : 1;
}
var su, ji, uu, cu, du, Ro = !1, _r = [], wt = null, kt = null, St = null, bn = /* @__PURE__ */ new Map(), er = /* @__PURE__ */ new Map(), vt = [], Dd = "mousedown mouseup touchcancel touchend touchstart auxclick dblclick pointercancel pointerdown pointerup dragend dragstart drop compositionend compositionstart keydown keypress keyup input textInput copy cut paste click change contextmenu reset submit".split(" ");
function ka(e, t) {
  switch (e) {
    case "focusin":
    case "focusout":
      wt = null;
      break;
    case "dragenter":
    case "dragleave":
      kt = null;
      break;
    case "mouseover":
    case "mouseout":
      St = null;
      break;
    case "pointerover":
    case "pointerout":
      bn.delete(t.pointerId);
      break;
    case "gotpointercapture":
    case "lostpointercapture":
      er.delete(t.pointerId);
  }
}
function Tn(e, t, n, r, l, o) {
  return e === null || e.nativeEvent !== o ? (e = { blockedOn: t, domEventName: n, eventSystemFlags: r, nativeEvent: o, targetContainers: [l] }, t !== null && (t = vr(t), t !== null && ji(t)), e) : (e.eventSystemFlags |= r, t = e.targetContainers, l !== null && t.indexOf(l) === -1 && t.push(l), e);
}
function Rd(e, t, n, r, l) {
  switch (t) {
    case "focusin":
      return wt = Tn(wt, e, t, n, r, l), !0;
    case "dragenter":
      return kt = Tn(kt, e, t, n, r, l), !0;
    case "mouseover":
      return St = Tn(St, e, t, n, r, l), !0;
    case "pointerover":
      var o = l.pointerId;
      return bn.set(o, Tn(bn.get(o) || null, e, t, n, r, l)), !0;
    case "gotpointercapture":
      return o = l.pointerId, er.set(o, Tn(er.get(o) || null, e, t, n, r, l)), !0;
  }
  return !1;
}
function fu(e) {
  var t = It(e.target);
  if (t !== null) {
    var n = Xt(t);
    if (n !== null) {
      if (t = n.tag, t === 13) {
        if (t = eu(n), t !== null) {
          e.blockedOn = t, du(e.priority, function() {
            uu(n);
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
function Vr(e) {
  if (e.blockedOn !== null) return !1;
  for (var t = e.targetContainers; 0 < t.length; ) {
    var n = Fo(e.domEventName, e.eventSystemFlags, t[0], e.nativeEvent);
    if (n === null) {
      n = e.nativeEvent;
      var r = new n.constructor(n.type, n);
      Po = r, n.target.dispatchEvent(r), Po = null;
    } else return t = vr(n), t !== null && ji(t), e.blockedOn = n, !1;
    t.shift();
  }
  return !0;
}
function Sa(e, t, n) {
  Vr(e) && n.delete(t);
}
function Fd() {
  Ro = !1, wt !== null && Vr(wt) && (wt = null), kt !== null && Vr(kt) && (kt = null), St !== null && Vr(St) && (St = null), bn.forEach(Sa), er.forEach(Sa);
}
function Ln(e, t) {
  e.blockedOn === t && (e.blockedOn = null, Ro || (Ro = !0, Me.unstable_scheduleCallback(Me.unstable_NormalPriority, Fd)));
}
function tr(e) {
  function t(l) {
    return Ln(l, e);
  }
  if (0 < _r.length) {
    Ln(_r[0], e);
    for (var n = 1; n < _r.length; n++) {
      var r = _r[n];
      r.blockedOn === e && (r.blockedOn = null);
    }
  }
  for (wt !== null && Ln(wt, e), kt !== null && Ln(kt, e), St !== null && Ln(St, e), bn.forEach(t), er.forEach(t), n = 0; n < vt.length; n++) r = vt[n], r.blockedOn === e && (r.blockedOn = null);
  for (; 0 < vt.length && (n = vt[0], n.blockedOn === null); ) fu(n), n.blockedOn === null && vt.shift();
}
var hn = dt.ReactCurrentBatchConfig, rl = !0;
function $d(e, t, n, r) {
  var l = V, o = hn.transition;
  hn.transition = null;
  try {
    V = 1, Ni(e, t, n, r);
  } finally {
    V = l, hn.transition = o;
  }
}
function Od(e, t, n, r) {
  var l = V, o = hn.transition;
  hn.transition = null;
  try {
    V = 4, Ni(e, t, n, r);
  } finally {
    V = l, hn.transition = o;
  }
}
function Ni(e, t, n, r) {
  if (rl) {
    var l = Fo(e, t, n, r);
    if (l === null) to(e, t, r, ll, n), ka(e, r);
    else if (Rd(l, e, t, n, r)) r.stopPropagation();
    else if (ka(e, r), t & 4 && -1 < Dd.indexOf(e)) {
      for (; l !== null; ) {
        var o = vr(l);
        if (o !== null && su(o), o = Fo(e, t, n, r), o === null && to(e, t, r, ll, n), o === l) break;
        l = o;
      }
      l !== null && r.stopPropagation();
    } else to(e, t, r, null, n);
  }
}
var ll = null;
function Fo(e, t, n, r) {
  if (ll = null, e = wi(r), e = It(e), e !== null) if (t = Xt(e), t === null) e = null;
  else if (n = t.tag, n === 13) {
    if (e = eu(t), e !== null) return e;
    e = null;
  } else if (n === 3) {
    if (t.stateNode.current.memoizedState.isDehydrated) return t.tag === 3 ? t.stateNode.containerInfo : null;
    e = null;
  } else t !== e && (e = null);
  return ll = e, null;
}
function pu(e) {
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
      switch (Nd()) {
        case ki:
          return 1;
        case lu:
          return 4;
        case tl:
        case Cd:
          return 16;
        case ou:
          return 536870912;
        default:
          return 16;
      }
    default:
      return 16;
  }
}
var xt = null, Ci = null, Hr = null;
function mu() {
  if (Hr) return Hr;
  var e, t = Ci, n = t.length, r, l = "value" in xt ? xt.value : xt.textContent, o = l.length;
  for (e = 0; e < n && t[e] === l[e]; e++) ;
  var i = n - e;
  for (r = 1; r <= i && t[n - r] === l[o - r]; r++) ;
  return Hr = l.slice(e, 1 < r ? 1 - r : void 0);
}
function Br(e) {
  var t = e.keyCode;
  return "charCode" in e ? (e = e.charCode, e === 0 && t === 13 && (e = 13)) : e = t, e === 10 && (e = 13), 32 <= e || e === 13 ? e : 0;
}
function zr() {
  return !0;
}
function ja() {
  return !1;
}
function Re(e) {
  function t(n, r, l, o, i) {
    this._reactName = n, this._targetInst = l, this.type = r, this.nativeEvent = o, this.target = i, this.currentTarget = null;
    for (var a in e) e.hasOwnProperty(a) && (n = e[a], this[a] = n ? n(o) : o[a]);
    return this.isDefaultPrevented = (o.defaultPrevented != null ? o.defaultPrevented : o.returnValue === !1) ? zr : ja, this.isPropagationStopped = ja, this;
  }
  return Z(t.prototype, { preventDefault: function() {
    this.defaultPrevented = !0;
    var n = this.nativeEvent;
    n && (n.preventDefault ? n.preventDefault() : typeof n.returnValue != "unknown" && (n.returnValue = !1), this.isDefaultPrevented = zr);
  }, stopPropagation: function() {
    var n = this.nativeEvent;
    n && (n.stopPropagation ? n.stopPropagation() : typeof n.cancelBubble != "unknown" && (n.cancelBubble = !0), this.isPropagationStopped = zr);
  }, persist: function() {
  }, isPersistent: zr }), t;
}
var En = { eventPhase: 0, bubbles: 0, cancelable: 0, timeStamp: function(e) {
  return e.timeStamp || Date.now();
}, defaultPrevented: 0, isTrusted: 0 }, Ei = Re(En), hr = Z({}, En, { view: 0, detail: 0 }), Id = Re(hr), Gl, Kl, Mn, Nl = Z({}, hr, { screenX: 0, screenY: 0, clientX: 0, clientY: 0, pageX: 0, pageY: 0, ctrlKey: 0, shiftKey: 0, altKey: 0, metaKey: 0, getModifierState: _i, button: 0, buttons: 0, relatedTarget: function(e) {
  return e.relatedTarget === void 0 ? e.fromElement === e.srcElement ? e.toElement : e.fromElement : e.relatedTarget;
}, movementX: function(e) {
  return "movementX" in e ? e.movementX : (e !== Mn && (Mn && e.type === "mousemove" ? (Gl = e.screenX - Mn.screenX, Kl = e.screenY - Mn.screenY) : Kl = Gl = 0, Mn = e), Gl);
}, movementY: function(e) {
  return "movementY" in e ? e.movementY : Kl;
} }), Na = Re(Nl), Ud = Z({}, Nl, { dataTransfer: 0 }), Ad = Re(Ud), Vd = Z({}, hr, { relatedTarget: 0 }), Yl = Re(Vd), Hd = Z({}, En, { animationName: 0, elapsedTime: 0, pseudoElement: 0 }), Bd = Re(Hd), Wd = Z({}, En, { clipboardData: function(e) {
  return "clipboardData" in e ? e.clipboardData : window.clipboardData;
} }), Qd = Re(Wd), Gd = Z({}, En, { data: 0 }), Ca = Re(Gd), Kd = {
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
}, Yd = {
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
}, Xd = { Alt: "altKey", Control: "ctrlKey", Meta: "metaKey", Shift: "shiftKey" };
function Zd(e) {
  var t = this.nativeEvent;
  return t.getModifierState ? t.getModifierState(e) : (e = Xd[e]) ? !!t[e] : !1;
}
function _i() {
  return Zd;
}
var Jd = Z({}, hr, { key: function(e) {
  if (e.key) {
    var t = Kd[e.key] || e.key;
    if (t !== "Unidentified") return t;
  }
  return e.type === "keypress" ? (e = Br(e), e === 13 ? "Enter" : String.fromCharCode(e)) : e.type === "keydown" || e.type === "keyup" ? Yd[e.keyCode] || "Unidentified" : "";
}, code: 0, location: 0, ctrlKey: 0, shiftKey: 0, altKey: 0, metaKey: 0, repeat: 0, locale: 0, getModifierState: _i, charCode: function(e) {
  return e.type === "keypress" ? Br(e) : 0;
}, keyCode: function(e) {
  return e.type === "keydown" || e.type === "keyup" ? e.keyCode : 0;
}, which: function(e) {
  return e.type === "keypress" ? Br(e) : e.type === "keydown" || e.type === "keyup" ? e.keyCode : 0;
} }), qd = Re(Jd), bd = Z({}, Nl, { pointerId: 0, width: 0, height: 0, pressure: 0, tangentialPressure: 0, tiltX: 0, tiltY: 0, twist: 0, pointerType: 0, isPrimary: 0 }), Ea = Re(bd), ef = Z({}, hr, { touches: 0, targetTouches: 0, changedTouches: 0, altKey: 0, metaKey: 0, ctrlKey: 0, shiftKey: 0, getModifierState: _i }), tf = Re(ef), nf = Z({}, En, { propertyName: 0, elapsedTime: 0, pseudoElement: 0 }), rf = Re(nf), lf = Z({}, Nl, {
  deltaX: function(e) {
    return "deltaX" in e ? e.deltaX : "wheelDeltaX" in e ? -e.wheelDeltaX : 0;
  },
  deltaY: function(e) {
    return "deltaY" in e ? e.deltaY : "wheelDeltaY" in e ? -e.wheelDeltaY : "wheelDelta" in e ? -e.wheelDelta : 0;
  },
  deltaZ: 0,
  deltaMode: 0
}), of = Re(lf), af = [9, 13, 27, 32], zi = at && "CompositionEvent" in window, Bn = null;
at && "documentMode" in document && (Bn = document.documentMode);
var sf = at && "TextEvent" in window && !Bn, hu = at && (!zi || Bn && 8 < Bn && 11 >= Bn), _a = " ", za = !1;
function vu(e, t) {
  switch (e) {
    case "keyup":
      return af.indexOf(t.keyCode) !== -1;
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
function gu(e) {
  return e = e.detail, typeof e == "object" && "data" in e ? e.data : null;
}
var tn = !1;
function uf(e, t) {
  switch (e) {
    case "compositionend":
      return gu(t);
    case "keypress":
      return t.which !== 32 ? null : (za = !0, _a);
    case "textInput":
      return e = t.data, e === _a && za ? null : e;
    default:
      return null;
  }
}
function cf(e, t) {
  if (tn) return e === "compositionend" || !zi && vu(e, t) ? (e = mu(), Hr = Ci = xt = null, tn = !1, e) : null;
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
      return hu && t.locale !== "ko" ? null : t.data;
    default:
      return null;
  }
}
var df = { color: !0, date: !0, datetime: !0, "datetime-local": !0, email: !0, month: !0, number: !0, password: !0, range: !0, search: !0, tel: !0, text: !0, time: !0, url: !0, week: !0 };
function Pa(e) {
  var t = e && e.nodeName && e.nodeName.toLowerCase();
  return t === "input" ? !!df[e.type] : t === "textarea";
}
function xu(e, t, n, r) {
  Xs(r), t = ol(t, "onChange"), 0 < t.length && (n = new Ei("onChange", "change", null, n, r), e.push({ event: n, listeners: t }));
}
var Wn = null, nr = null;
function ff(e) {
  Pu(e, 0);
}
function Cl(e) {
  var t = ln(e);
  if (Hs(t)) return e;
}
function pf(e, t) {
  if (e === "change") return t;
}
var yu = !1;
if (at) {
  var Xl;
  if (at) {
    var Zl = "oninput" in document;
    if (!Zl) {
      var Ta = document.createElement("div");
      Ta.setAttribute("oninput", "return;"), Zl = typeof Ta.oninput == "function";
    }
    Xl = Zl;
  } else Xl = !1;
  yu = Xl && (!document.documentMode || 9 < document.documentMode);
}
function La() {
  Wn && (Wn.detachEvent("onpropertychange", wu), nr = Wn = null);
}
function wu(e) {
  if (e.propertyName === "value" && Cl(nr)) {
    var t = [];
    xu(t, nr, e, wi(e)), bs(ff, t);
  }
}
function mf(e, t, n) {
  e === "focusin" ? (La(), Wn = t, nr = n, Wn.attachEvent("onpropertychange", wu)) : e === "focusout" && La();
}
function hf(e) {
  if (e === "selectionchange" || e === "keyup" || e === "keydown") return Cl(nr);
}
function vf(e, t) {
  if (e === "click") return Cl(t);
}
function gf(e, t) {
  if (e === "input" || e === "change") return Cl(t);
}
function xf(e, t) {
  return e === t && (e !== 0 || 1 / e === 1 / t) || e !== e && t !== t;
}
var Ye = typeof Object.is == "function" ? Object.is : xf;
function rr(e, t) {
  if (Ye(e, t)) return !0;
  if (typeof e != "object" || e === null || typeof t != "object" || t === null) return !1;
  var n = Object.keys(e), r = Object.keys(t);
  if (n.length !== r.length) return !1;
  for (r = 0; r < n.length; r++) {
    var l = n[r];
    if (!go.call(t, l) || !Ye(e[l], t[l])) return !1;
  }
  return !0;
}
function Ma(e) {
  for (; e && e.firstChild; ) e = e.firstChild;
  return e;
}
function Da(e, t) {
  var n = Ma(e);
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
    n = Ma(n);
  }
}
function ku(e, t) {
  return e && t ? e === t ? !0 : e && e.nodeType === 3 ? !1 : t && t.nodeType === 3 ? ku(e, t.parentNode) : "contains" in e ? e.contains(t) : e.compareDocumentPosition ? !!(e.compareDocumentPosition(t) & 16) : !1 : !1;
}
function Su() {
  for (var e = window, t = qr(); t instanceof e.HTMLIFrameElement; ) {
    try {
      var n = typeof t.contentWindow.location.href == "string";
    } catch {
      n = !1;
    }
    if (n) e = t.contentWindow;
    else break;
    t = qr(e.document);
  }
  return t;
}
function Pi(e) {
  var t = e && e.nodeName && e.nodeName.toLowerCase();
  return t && (t === "input" && (e.type === "text" || e.type === "search" || e.type === "tel" || e.type === "url" || e.type === "password") || t === "textarea" || e.contentEditable === "true");
}
function yf(e) {
  var t = Su(), n = e.focusedElem, r = e.selectionRange;
  if (t !== n && n && n.ownerDocument && ku(n.ownerDocument.documentElement, n)) {
    if (r !== null && Pi(n)) {
      if (t = r.start, e = r.end, e === void 0 && (e = t), "selectionStart" in n) n.selectionStart = t, n.selectionEnd = Math.min(e, n.value.length);
      else if (e = (t = n.ownerDocument || document) && t.defaultView || window, e.getSelection) {
        e = e.getSelection();
        var l = n.textContent.length, o = Math.min(r.start, l);
        r = r.end === void 0 ? o : Math.min(r.end, l), !e.extend && o > r && (l = r, r = o, o = l), l = Da(n, o);
        var i = Da(
          n,
          r
        );
        l && i && (e.rangeCount !== 1 || e.anchorNode !== l.node || e.anchorOffset !== l.offset || e.focusNode !== i.node || e.focusOffset !== i.offset) && (t = t.createRange(), t.setStart(l.node, l.offset), e.removeAllRanges(), o > r ? (e.addRange(t), e.extend(i.node, i.offset)) : (t.setEnd(i.node, i.offset), e.addRange(t)));
      }
    }
    for (t = [], e = n; e = e.parentNode; ) e.nodeType === 1 && t.push({ element: e, left: e.scrollLeft, top: e.scrollTop });
    for (typeof n.focus == "function" && n.focus(), n = 0; n < t.length; n++) e = t[n], e.element.scrollLeft = e.left, e.element.scrollTop = e.top;
  }
}
var wf = at && "documentMode" in document && 11 >= document.documentMode, nn = null, $o = null, Qn = null, Oo = !1;
function Ra(e, t, n) {
  var r = n.window === n ? n.document : n.nodeType === 9 ? n : n.ownerDocument;
  Oo || nn == null || nn !== qr(r) || (r = nn, "selectionStart" in r && Pi(r) ? r = { start: r.selectionStart, end: r.selectionEnd } : (r = (r.ownerDocument && r.ownerDocument.defaultView || window).getSelection(), r = { anchorNode: r.anchorNode, anchorOffset: r.anchorOffset, focusNode: r.focusNode, focusOffset: r.focusOffset }), Qn && rr(Qn, r) || (Qn = r, r = ol($o, "onSelect"), 0 < r.length && (t = new Ei("onSelect", "select", null, t, n), e.push({ event: t, listeners: r }), t.target = nn)));
}
function Pr(e, t) {
  var n = {};
  return n[e.toLowerCase()] = t.toLowerCase(), n["Webkit" + e] = "webkit" + t, n["Moz" + e] = "moz" + t, n;
}
var rn = { animationend: Pr("Animation", "AnimationEnd"), animationiteration: Pr("Animation", "AnimationIteration"), animationstart: Pr("Animation", "AnimationStart"), transitionend: Pr("Transition", "TransitionEnd") }, Jl = {}, ju = {};
at && (ju = document.createElement("div").style, "AnimationEvent" in window || (delete rn.animationend.animation, delete rn.animationiteration.animation, delete rn.animationstart.animation), "TransitionEvent" in window || delete rn.transitionend.transition);
function El(e) {
  if (Jl[e]) return Jl[e];
  if (!rn[e]) return e;
  var t = rn[e], n;
  for (n in t) if (t.hasOwnProperty(n) && n in ju) return Jl[e] = t[n];
  return e;
}
var Nu = El("animationend"), Cu = El("animationiteration"), Eu = El("animationstart"), _u = El("transitionend"), zu = /* @__PURE__ */ new Map(), Fa = "abort auxClick cancel canPlay canPlayThrough click close contextMenu copy cut drag dragEnd dragEnter dragExit dragLeave dragOver dragStart drop durationChange emptied encrypted ended error gotPointerCapture input invalid keyDown keyPress keyUp load loadedData loadedMetadata loadStart lostPointerCapture mouseDown mouseMove mouseOut mouseOver mouseUp paste pause play playing pointerCancel pointerDown pointerMove pointerOut pointerOver pointerUp progress rateChange reset resize seeked seeking stalled submit suspend timeUpdate touchCancel touchEnd touchStart volumeChange scroll toggle touchMove waiting wheel".split(" ");
function Tt(e, t) {
  zu.set(e, t), Yt(t, [e]);
}
for (var ql = 0; ql < Fa.length; ql++) {
  var bl = Fa[ql], kf = bl.toLowerCase(), Sf = bl[0].toUpperCase() + bl.slice(1);
  Tt(kf, "on" + Sf);
}
Tt(Nu, "onAnimationEnd");
Tt(Cu, "onAnimationIteration");
Tt(Eu, "onAnimationStart");
Tt("dblclick", "onDoubleClick");
Tt("focusin", "onFocus");
Tt("focusout", "onBlur");
Tt(_u, "onTransitionEnd");
xn("onMouseEnter", ["mouseout", "mouseover"]);
xn("onMouseLeave", ["mouseout", "mouseover"]);
xn("onPointerEnter", ["pointerout", "pointerover"]);
xn("onPointerLeave", ["pointerout", "pointerover"]);
Yt("onChange", "change click focusin focusout input keydown keyup selectionchange".split(" "));
Yt("onSelect", "focusout contextmenu dragend focusin keydown keyup mousedown mouseup selectionchange".split(" "));
Yt("onBeforeInput", ["compositionend", "keypress", "textInput", "paste"]);
Yt("onCompositionEnd", "compositionend focusout keydown keypress keyup mousedown".split(" "));
Yt("onCompositionStart", "compositionstart focusout keydown keypress keyup mousedown".split(" "));
Yt("onCompositionUpdate", "compositionupdate focusout keydown keypress keyup mousedown".split(" "));
var An = "abort canplay canplaythrough durationchange emptied encrypted ended error loadeddata loadedmetadata loadstart pause play playing progress ratechange resize seeked seeking stalled suspend timeupdate volumechange waiting".split(" "), jf = new Set("cancel close invalid load scroll toggle".split(" ").concat(An));
function $a(e, t, n) {
  var r = e.type || "unknown-event";
  e.currentTarget = n, wd(r, t, void 0, e), e.currentTarget = null;
}
function Pu(e, t) {
  t = (t & 4) !== 0;
  for (var n = 0; n < e.length; n++) {
    var r = e[n], l = r.event;
    r = r.listeners;
    e: {
      var o = void 0;
      if (t) for (var i = r.length - 1; 0 <= i; i--) {
        var a = r[i], s = a.instance, f = a.currentTarget;
        if (a = a.listener, s !== o && l.isPropagationStopped()) break e;
        $a(l, a, f), o = s;
      }
      else for (i = 0; i < r.length; i++) {
        if (a = r[i], s = a.instance, f = a.currentTarget, a = a.listener, s !== o && l.isPropagationStopped()) break e;
        $a(l, a, f), o = s;
      }
    }
  }
  if (el) throw e = Mo, el = !1, Mo = null, e;
}
function B(e, t) {
  var n = t[Ho];
  n === void 0 && (n = t[Ho] = /* @__PURE__ */ new Set());
  var r = e + "__bubble";
  n.has(r) || (Tu(t, e, 2, !1), n.add(r));
}
function eo(e, t, n) {
  var r = 0;
  t && (r |= 4), Tu(n, e, r, t);
}
var Tr = "_reactListening" + Math.random().toString(36).slice(2);
function lr(e) {
  if (!e[Tr]) {
    e[Tr] = !0, Os.forEach(function(n) {
      n !== "selectionchange" && (jf.has(n) || eo(n, !1, e), eo(n, !0, e));
    });
    var t = e.nodeType === 9 ? e : e.ownerDocument;
    t === null || t[Tr] || (t[Tr] = !0, eo("selectionchange", !1, t));
  }
}
function Tu(e, t, n, r) {
  switch (pu(t)) {
    case 1:
      var l = $d;
      break;
    case 4:
      l = Od;
      break;
    default:
      l = Ni;
  }
  n = l.bind(null, t, n, e), l = void 0, !Lo || t !== "touchstart" && t !== "touchmove" && t !== "wheel" || (l = !0), r ? l !== void 0 ? e.addEventListener(t, n, { capture: !0, passive: l }) : e.addEventListener(t, n, !0) : l !== void 0 ? e.addEventListener(t, n, { passive: l }) : e.addEventListener(t, n, !1);
}
function to(e, t, n, r, l) {
  var o = r;
  if (!(t & 1) && !(t & 2) && r !== null) e: for (; ; ) {
    if (r === null) return;
    var i = r.tag;
    if (i === 3 || i === 4) {
      var a = r.stateNode.containerInfo;
      if (a === l || a.nodeType === 8 && a.parentNode === l) break;
      if (i === 4) for (i = r.return; i !== null; ) {
        var s = i.tag;
        if ((s === 3 || s === 4) && (s = i.stateNode.containerInfo, s === l || s.nodeType === 8 && s.parentNode === l)) return;
        i = i.return;
      }
      for (; a !== null; ) {
        if (i = It(a), i === null) return;
        if (s = i.tag, s === 5 || s === 6) {
          r = o = i;
          continue e;
        }
        a = a.parentNode;
      }
    }
    r = r.return;
  }
  bs(function() {
    var f = o, h = wi(n), v = [];
    e: {
      var p = zu.get(e);
      if (p !== void 0) {
        var g = Ei, w = e;
        switch (e) {
          case "keypress":
            if (Br(n) === 0) break e;
          case "keydown":
          case "keyup":
            g = qd;
            break;
          case "focusin":
            w = "focus", g = Yl;
            break;
          case "focusout":
            w = "blur", g = Yl;
            break;
          case "beforeblur":
          case "afterblur":
            g = Yl;
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
            g = Na;
            break;
          case "drag":
          case "dragend":
          case "dragenter":
          case "dragexit":
          case "dragleave":
          case "dragover":
          case "dragstart":
          case "drop":
            g = Ad;
            break;
          case "touchcancel":
          case "touchend":
          case "touchmove":
          case "touchstart":
            g = tf;
            break;
          case Nu:
          case Cu:
          case Eu:
            g = Bd;
            break;
          case _u:
            g = rf;
            break;
          case "scroll":
            g = Id;
            break;
          case "wheel":
            g = of;
            break;
          case "copy":
          case "cut":
          case "paste":
            g = Qd;
            break;
          case "gotpointercapture":
          case "lostpointercapture":
          case "pointercancel":
          case "pointerdown":
          case "pointermove":
          case "pointerout":
          case "pointerover":
          case "pointerup":
            g = Ea;
        }
        var k = (t & 4) !== 0, N = !k && e === "scroll", d = k ? p !== null ? p + "Capture" : null : p;
        k = [];
        for (var c = f, m; c !== null; ) {
          m = c;
          var x = m.stateNode;
          if (m.tag === 5 && x !== null && (m = x, d !== null && (x = qn(c, d), x != null && k.push(or(c, x, m)))), N) break;
          c = c.return;
        }
        0 < k.length && (p = new g(p, w, null, n, h), v.push({ event: p, listeners: k }));
      }
    }
    if (!(t & 7)) {
      e: {
        if (p = e === "mouseover" || e === "pointerover", g = e === "mouseout" || e === "pointerout", p && n !== Po && (w = n.relatedTarget || n.fromElement) && (It(w) || w[st])) break e;
        if ((g || p) && (p = h.window === h ? h : (p = h.ownerDocument) ? p.defaultView || p.parentWindow : window, g ? (w = n.relatedTarget || n.toElement, g = f, w = w ? It(w) : null, w !== null && (N = Xt(w), w !== N || w.tag !== 5 && w.tag !== 6) && (w = null)) : (g = null, w = f), g !== w)) {
          if (k = Na, x = "onMouseLeave", d = "onMouseEnter", c = "mouse", (e === "pointerout" || e === "pointerover") && (k = Ea, x = "onPointerLeave", d = "onPointerEnter", c = "pointer"), N = g == null ? p : ln(g), m = w == null ? p : ln(w), p = new k(x, c + "leave", g, n, h), p.target = N, p.relatedTarget = m, x = null, It(h) === f && (k = new k(d, c + "enter", w, n, h), k.target = m, k.relatedTarget = N, x = k), N = x, g && w) t: {
            for (k = g, d = w, c = 0, m = k; m; m = Jt(m)) c++;
            for (m = 0, x = d; x; x = Jt(x)) m++;
            for (; 0 < c - m; ) k = Jt(k), c--;
            for (; 0 < m - c; ) d = Jt(d), m--;
            for (; c--; ) {
              if (k === d || d !== null && k === d.alternate) break t;
              k = Jt(k), d = Jt(d);
            }
            k = null;
          }
          else k = null;
          g !== null && Oa(v, p, g, k, !1), w !== null && N !== null && Oa(v, N, w, k, !0);
        }
      }
      e: {
        if (p = f ? ln(f) : window, g = p.nodeName && p.nodeName.toLowerCase(), g === "select" || g === "input" && p.type === "file") var C = pf;
        else if (Pa(p)) if (yu) C = gf;
        else {
          C = hf;
          var E = mf;
        }
        else (g = p.nodeName) && g.toLowerCase() === "input" && (p.type === "checkbox" || p.type === "radio") && (C = vf);
        if (C && (C = C(e, f))) {
          xu(v, C, n, h);
          break e;
        }
        E && E(e, p, f), e === "focusout" && (E = p._wrapperState) && E.controlled && p.type === "number" && No(p, "number", p.value);
      }
      switch (E = f ? ln(f) : window, e) {
        case "focusin":
          (Pa(E) || E.contentEditable === "true") && (nn = E, $o = f, Qn = null);
          break;
        case "focusout":
          Qn = $o = nn = null;
          break;
        case "mousedown":
          Oo = !0;
          break;
        case "contextmenu":
        case "mouseup":
        case "dragend":
          Oo = !1, Ra(v, n, h);
          break;
        case "selectionchange":
          if (wf) break;
        case "keydown":
        case "keyup":
          Ra(v, n, h);
      }
      var S;
      if (zi) e: {
        switch (e) {
          case "compositionstart":
            var _ = "onCompositionStart";
            break e;
          case "compositionend":
            _ = "onCompositionEnd";
            break e;
          case "compositionupdate":
            _ = "onCompositionUpdate";
            break e;
        }
        _ = void 0;
      }
      else tn ? vu(e, n) && (_ = "onCompositionEnd") : e === "keydown" && n.keyCode === 229 && (_ = "onCompositionStart");
      _ && (hu && n.locale !== "ko" && (tn || _ !== "onCompositionStart" ? _ === "onCompositionEnd" && tn && (S = mu()) : (xt = h, Ci = "value" in xt ? xt.value : xt.textContent, tn = !0)), E = ol(f, _), 0 < E.length && (_ = new Ca(_, e, null, n, h), v.push({ event: _, listeners: E }), S ? _.data = S : (S = gu(n), S !== null && (_.data = S)))), (S = sf ? uf(e, n) : cf(e, n)) && (f = ol(f, "onBeforeInput"), 0 < f.length && (h = new Ca("onBeforeInput", "beforeinput", null, n, h), v.push({ event: h, listeners: f }), h.data = S));
    }
    Pu(v, t);
  });
}
function or(e, t, n) {
  return { instance: e, listener: t, currentTarget: n };
}
function ol(e, t) {
  for (var n = t + "Capture", r = []; e !== null; ) {
    var l = e, o = l.stateNode;
    l.tag === 5 && o !== null && (l = o, o = qn(e, n), o != null && r.unshift(or(e, o, l)), o = qn(e, t), o != null && r.push(or(e, o, l))), e = e.return;
  }
  return r;
}
function Jt(e) {
  if (e === null) return null;
  do
    e = e.return;
  while (e && e.tag !== 5);
  return e || null;
}
function Oa(e, t, n, r, l) {
  for (var o = t._reactName, i = []; n !== null && n !== r; ) {
    var a = n, s = a.alternate, f = a.stateNode;
    if (s !== null && s === r) break;
    a.tag === 5 && f !== null && (a = f, l ? (s = qn(n, o), s != null && i.unshift(or(n, s, a))) : l || (s = qn(n, o), s != null && i.push(or(n, s, a)))), n = n.return;
  }
  i.length !== 0 && e.push({ event: t, listeners: i });
}
var Nf = /\r\n?/g, Cf = /\u0000|\uFFFD/g;
function Ia(e) {
  return (typeof e == "string" ? e : "" + e).replace(Nf, `
`).replace(Cf, "");
}
function Lr(e, t, n) {
  if (t = Ia(t), Ia(e) !== t && n) throw Error(j(425));
}
function il() {
}
var Io = null, Uo = null;
function Ao(e, t) {
  return e === "textarea" || e === "noscript" || typeof t.children == "string" || typeof t.children == "number" || typeof t.dangerouslySetInnerHTML == "object" && t.dangerouslySetInnerHTML !== null && t.dangerouslySetInnerHTML.__html != null;
}
var Vo = typeof setTimeout == "function" ? setTimeout : void 0, Ef = typeof clearTimeout == "function" ? clearTimeout : void 0, Ua = typeof Promise == "function" ? Promise : void 0, _f = typeof queueMicrotask == "function" ? queueMicrotask : typeof Ua < "u" ? function(e) {
  return Ua.resolve(null).then(e).catch(zf);
} : Vo;
function zf(e) {
  setTimeout(function() {
    throw e;
  });
}
function no(e, t) {
  var n = t, r = 0;
  do {
    var l = n.nextSibling;
    if (e.removeChild(n), l && l.nodeType === 8) if (n = l.data, n === "/$") {
      if (r === 0) {
        e.removeChild(l), tr(t);
        return;
      }
      r--;
    } else n !== "$" && n !== "$?" && n !== "$!" || r++;
    n = l;
  } while (n);
  tr(t);
}
function jt(e) {
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
function Aa(e) {
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
var _n = Math.random().toString(36).slice(2), be = "__reactFiber$" + _n, ir = "__reactProps$" + _n, st = "__reactContainer$" + _n, Ho = "__reactEvents$" + _n, Pf = "__reactListeners$" + _n, Tf = "__reactHandles$" + _n;
function It(e) {
  var t = e[be];
  if (t) return t;
  for (var n = e.parentNode; n; ) {
    if (t = n[st] || n[be]) {
      if (n = t.alternate, t.child !== null || n !== null && n.child !== null) for (e = Aa(e); e !== null; ) {
        if (n = e[be]) return n;
        e = Aa(e);
      }
      return t;
    }
    e = n, n = e.parentNode;
  }
  return null;
}
function vr(e) {
  return e = e[be] || e[st], !e || e.tag !== 5 && e.tag !== 6 && e.tag !== 13 && e.tag !== 3 ? null : e;
}
function ln(e) {
  if (e.tag === 5 || e.tag === 6) return e.stateNode;
  throw Error(j(33));
}
function _l(e) {
  return e[ir] || null;
}
var Bo = [], on = -1;
function Lt(e) {
  return { current: e };
}
function W(e) {
  0 > on || (e.current = Bo[on], Bo[on] = null, on--);
}
function H(e, t) {
  on++, Bo[on] = e.current, e.current = t;
}
var Pt = {}, ge = Lt(Pt), Ee = Lt(!1), Bt = Pt;
function yn(e, t) {
  var n = e.type.contextTypes;
  if (!n) return Pt;
  var r = e.stateNode;
  if (r && r.__reactInternalMemoizedUnmaskedChildContext === t) return r.__reactInternalMemoizedMaskedChildContext;
  var l = {}, o;
  for (o in n) l[o] = t[o];
  return r && (e = e.stateNode, e.__reactInternalMemoizedUnmaskedChildContext = t, e.__reactInternalMemoizedMaskedChildContext = l), l;
}
function _e(e) {
  return e = e.childContextTypes, e != null;
}
function al() {
  W(Ee), W(ge);
}
function Va(e, t, n) {
  if (ge.current !== Pt) throw Error(j(168));
  H(ge, t), H(Ee, n);
}
function Lu(e, t, n) {
  var r = e.stateNode;
  if (t = t.childContextTypes, typeof r.getChildContext != "function") return n;
  r = r.getChildContext();
  for (var l in r) if (!(l in t)) throw Error(j(108, pd(e) || "Unknown", l));
  return Z({}, n, r);
}
function sl(e) {
  return e = (e = e.stateNode) && e.__reactInternalMemoizedMergedChildContext || Pt, Bt = ge.current, H(ge, e), H(Ee, Ee.current), !0;
}
function Ha(e, t, n) {
  var r = e.stateNode;
  if (!r) throw Error(j(169));
  n ? (e = Lu(e, t, Bt), r.__reactInternalMemoizedMergedChildContext = e, W(Ee), W(ge), H(ge, e)) : W(Ee), H(Ee, n);
}
var rt = null, zl = !1, ro = !1;
function Mu(e) {
  rt === null ? rt = [e] : rt.push(e);
}
function Lf(e) {
  zl = !0, Mu(e);
}
function Mt() {
  if (!ro && rt !== null) {
    ro = !0;
    var e = 0, t = V;
    try {
      var n = rt;
      for (V = 1; e < n.length; e++) {
        var r = n[e];
        do
          r = r(!0);
        while (r !== null);
      }
      rt = null, zl = !1;
    } catch (l) {
      throw rt !== null && (rt = rt.slice(e + 1)), ru(ki, Mt), l;
    } finally {
      V = t, ro = !1;
    }
  }
  return null;
}
var an = [], sn = 0, ul = null, cl = 0, $e = [], Oe = 0, Wt = null, lt = 1, ot = "";
function $t(e, t) {
  an[sn++] = cl, an[sn++] = ul, ul = e, cl = t;
}
function Du(e, t, n) {
  $e[Oe++] = lt, $e[Oe++] = ot, $e[Oe++] = Wt, Wt = e;
  var r = lt;
  e = ot;
  var l = 32 - Ge(r) - 1;
  r &= ~(1 << l), n += 1;
  var o = 32 - Ge(t) + l;
  if (30 < o) {
    var i = l - l % 5;
    o = (r & (1 << i) - 1).toString(32), r >>= i, l -= i, lt = 1 << 32 - Ge(t) + l | n << l | r, ot = o + e;
  } else lt = 1 << o | n << l | r, ot = e;
}
function Ti(e) {
  e.return !== null && ($t(e, 1), Du(e, 1, 0));
}
function Li(e) {
  for (; e === ul; ) ul = an[--sn], an[sn] = null, cl = an[--sn], an[sn] = null;
  for (; e === Wt; ) Wt = $e[--Oe], $e[Oe] = null, ot = $e[--Oe], $e[Oe] = null, lt = $e[--Oe], $e[Oe] = null;
}
var Le = null, Te = null, K = !1, Qe = null;
function Ru(e, t) {
  var n = Ie(5, null, null, 0);
  n.elementType = "DELETED", n.stateNode = t, n.return = e, t = e.deletions, t === null ? (e.deletions = [n], e.flags |= 16) : t.push(n);
}
function Ba(e, t) {
  switch (e.tag) {
    case 5:
      var n = e.type;
      return t = t.nodeType !== 1 || n.toLowerCase() !== t.nodeName.toLowerCase() ? null : t, t !== null ? (e.stateNode = t, Le = e, Te = jt(t.firstChild), !0) : !1;
    case 6:
      return t = e.pendingProps === "" || t.nodeType !== 3 ? null : t, t !== null ? (e.stateNode = t, Le = e, Te = null, !0) : !1;
    case 13:
      return t = t.nodeType !== 8 ? null : t, t !== null ? (n = Wt !== null ? { id: lt, overflow: ot } : null, e.memoizedState = { dehydrated: t, treeContext: n, retryLane: 1073741824 }, n = Ie(18, null, null, 0), n.stateNode = t, n.return = e, e.child = n, Le = e, Te = null, !0) : !1;
    default:
      return !1;
  }
}
function Wo(e) {
  return (e.mode & 1) !== 0 && (e.flags & 128) === 0;
}
function Qo(e) {
  if (K) {
    var t = Te;
    if (t) {
      var n = t;
      if (!Ba(e, t)) {
        if (Wo(e)) throw Error(j(418));
        t = jt(n.nextSibling);
        var r = Le;
        t && Ba(e, t) ? Ru(r, n) : (e.flags = e.flags & -4097 | 2, K = !1, Le = e);
      }
    } else {
      if (Wo(e)) throw Error(j(418));
      e.flags = e.flags & -4097 | 2, K = !1, Le = e;
    }
  }
}
function Wa(e) {
  for (e = e.return; e !== null && e.tag !== 5 && e.tag !== 3 && e.tag !== 13; ) e = e.return;
  Le = e;
}
function Mr(e) {
  if (e !== Le) return !1;
  if (!K) return Wa(e), K = !0, !1;
  var t;
  if ((t = e.tag !== 3) && !(t = e.tag !== 5) && (t = e.type, t = t !== "head" && t !== "body" && !Ao(e.type, e.memoizedProps)), t && (t = Te)) {
    if (Wo(e)) throw Fu(), Error(j(418));
    for (; t; ) Ru(e, t), t = jt(t.nextSibling);
  }
  if (Wa(e), e.tag === 13) {
    if (e = e.memoizedState, e = e !== null ? e.dehydrated : null, !e) throw Error(j(317));
    e: {
      for (e = e.nextSibling, t = 0; e; ) {
        if (e.nodeType === 8) {
          var n = e.data;
          if (n === "/$") {
            if (t === 0) {
              Te = jt(e.nextSibling);
              break e;
            }
            t--;
          } else n !== "$" && n !== "$!" && n !== "$?" || t++;
        }
        e = e.nextSibling;
      }
      Te = null;
    }
  } else Te = Le ? jt(e.stateNode.nextSibling) : null;
  return !0;
}
function Fu() {
  for (var e = Te; e; ) e = jt(e.nextSibling);
}
function wn() {
  Te = Le = null, K = !1;
}
function Mi(e) {
  Qe === null ? Qe = [e] : Qe.push(e);
}
var Mf = dt.ReactCurrentBatchConfig;
function Dn(e, t, n) {
  if (e = n.ref, e !== null && typeof e != "function" && typeof e != "object") {
    if (n._owner) {
      if (n = n._owner, n) {
        if (n.tag !== 1) throw Error(j(309));
        var r = n.stateNode;
      }
      if (!r) throw Error(j(147, e));
      var l = r, o = "" + e;
      return t !== null && t.ref !== null && typeof t.ref == "function" && t.ref._stringRef === o ? t.ref : (t = function(i) {
        var a = l.refs;
        i === null ? delete a[o] : a[o] = i;
      }, t._stringRef = o, t);
    }
    if (typeof e != "string") throw Error(j(284));
    if (!n._owner) throw Error(j(290, e));
  }
  return e;
}
function Dr(e, t) {
  throw e = Object.prototype.toString.call(t), Error(j(31, e === "[object Object]" ? "object with keys {" + Object.keys(t).join(", ") + "}" : e));
}
function Qa(e) {
  var t = e._init;
  return t(e._payload);
}
function $u(e) {
  function t(d, c) {
    if (e) {
      var m = d.deletions;
      m === null ? (d.deletions = [c], d.flags |= 16) : m.push(c);
    }
  }
  function n(d, c) {
    if (!e) return null;
    for (; c !== null; ) t(d, c), c = c.sibling;
    return null;
  }
  function r(d, c) {
    for (d = /* @__PURE__ */ new Map(); c !== null; ) c.key !== null ? d.set(c.key, c) : d.set(c.index, c), c = c.sibling;
    return d;
  }
  function l(d, c) {
    return d = _t(d, c), d.index = 0, d.sibling = null, d;
  }
  function o(d, c, m) {
    return d.index = m, e ? (m = d.alternate, m !== null ? (m = m.index, m < c ? (d.flags |= 2, c) : m) : (d.flags |= 2, c)) : (d.flags |= 1048576, c);
  }
  function i(d) {
    return e && d.alternate === null && (d.flags |= 2), d;
  }
  function a(d, c, m, x) {
    return c === null || c.tag !== 6 ? (c = co(m, d.mode, x), c.return = d, c) : (c = l(c, m), c.return = d, c);
  }
  function s(d, c, m, x) {
    var C = m.type;
    return C === en ? h(d, c, m.props.children, x, m.key) : c !== null && (c.elementType === C || typeof C == "object" && C !== null && C.$$typeof === mt && Qa(C) === c.type) ? (x = l(c, m.props), x.ref = Dn(d, c, m), x.return = d, x) : (x = Zr(m.type, m.key, m.props, null, d.mode, x), x.ref = Dn(d, c, m), x.return = d, x);
  }
  function f(d, c, m, x) {
    return c === null || c.tag !== 4 || c.stateNode.containerInfo !== m.containerInfo || c.stateNode.implementation !== m.implementation ? (c = fo(m, d.mode, x), c.return = d, c) : (c = l(c, m.children || []), c.return = d, c);
  }
  function h(d, c, m, x, C) {
    return c === null || c.tag !== 7 ? (c = Ht(m, d.mode, x, C), c.return = d, c) : (c = l(c, m), c.return = d, c);
  }
  function v(d, c, m) {
    if (typeof c == "string" && c !== "" || typeof c == "number") return c = co("" + c, d.mode, m), c.return = d, c;
    if (typeof c == "object" && c !== null) {
      switch (c.$$typeof) {
        case Sr:
          return m = Zr(c.type, c.key, c.props, null, d.mode, m), m.ref = Dn(d, null, c), m.return = d, m;
        case bt:
          return c = fo(c, d.mode, m), c.return = d, c;
        case mt:
          var x = c._init;
          return v(d, x(c._payload), m);
      }
      if (In(c) || zn(c)) return c = Ht(c, d.mode, m, null), c.return = d, c;
      Dr(d, c);
    }
    return null;
  }
  function p(d, c, m, x) {
    var C = c !== null ? c.key : null;
    if (typeof m == "string" && m !== "" || typeof m == "number") return C !== null ? null : a(d, c, "" + m, x);
    if (typeof m == "object" && m !== null) {
      switch (m.$$typeof) {
        case Sr:
          return m.key === C ? s(d, c, m, x) : null;
        case bt:
          return m.key === C ? f(d, c, m, x) : null;
        case mt:
          return C = m._init, p(
            d,
            c,
            C(m._payload),
            x
          );
      }
      if (In(m) || zn(m)) return C !== null ? null : h(d, c, m, x, null);
      Dr(d, m);
    }
    return null;
  }
  function g(d, c, m, x, C) {
    if (typeof x == "string" && x !== "" || typeof x == "number") return d = d.get(m) || null, a(c, d, "" + x, C);
    if (typeof x == "object" && x !== null) {
      switch (x.$$typeof) {
        case Sr:
          return d = d.get(x.key === null ? m : x.key) || null, s(c, d, x, C);
        case bt:
          return d = d.get(x.key === null ? m : x.key) || null, f(c, d, x, C);
        case mt:
          var E = x._init;
          return g(d, c, m, E(x._payload), C);
      }
      if (In(x) || zn(x)) return d = d.get(m) || null, h(c, d, x, C, null);
      Dr(c, x);
    }
    return null;
  }
  function w(d, c, m, x) {
    for (var C = null, E = null, S = c, _ = c = 0, L = null; S !== null && _ < m.length; _++) {
      S.index > _ ? (L = S, S = null) : L = S.sibling;
      var D = p(d, S, m[_], x);
      if (D === null) {
        S === null && (S = L);
        break;
      }
      e && S && D.alternate === null && t(d, S), c = o(D, c, _), E === null ? C = D : E.sibling = D, E = D, S = L;
    }
    if (_ === m.length) return n(d, S), K && $t(d, _), C;
    if (S === null) {
      for (; _ < m.length; _++) S = v(d, m[_], x), S !== null && (c = o(S, c, _), E === null ? C = S : E.sibling = S, E = S);
      return K && $t(d, _), C;
    }
    for (S = r(d, S); _ < m.length; _++) L = g(S, d, _, m[_], x), L !== null && (e && L.alternate !== null && S.delete(L.key === null ? _ : L.key), c = o(L, c, _), E === null ? C = L : E.sibling = L, E = L);
    return e && S.forEach(function(M) {
      return t(d, M);
    }), K && $t(d, _), C;
  }
  function k(d, c, m, x) {
    var C = zn(m);
    if (typeof C != "function") throw Error(j(150));
    if (m = C.call(m), m == null) throw Error(j(151));
    for (var E = C = null, S = c, _ = c = 0, L = null, D = m.next(); S !== null && !D.done; _++, D = m.next()) {
      S.index > _ ? (L = S, S = null) : L = S.sibling;
      var M = p(d, S, D.value, x);
      if (M === null) {
        S === null && (S = L);
        break;
      }
      e && S && M.alternate === null && t(d, S), c = o(M, c, _), E === null ? C = M : E.sibling = M, E = M, S = L;
    }
    if (D.done) return n(
      d,
      S
    ), K && $t(d, _), C;
    if (S === null) {
      for (; !D.done; _++, D = m.next()) D = v(d, D.value, x), D !== null && (c = o(D, c, _), E === null ? C = D : E.sibling = D, E = D);
      return K && $t(d, _), C;
    }
    for (S = r(d, S); !D.done; _++, D = m.next()) D = g(S, d, _, D.value, x), D !== null && (e && D.alternate !== null && S.delete(D.key === null ? _ : D.key), c = o(D, c, _), E === null ? C = D : E.sibling = D, E = D);
    return e && S.forEach(function(Q) {
      return t(d, Q);
    }), K && $t(d, _), C;
  }
  function N(d, c, m, x) {
    if (typeof m == "object" && m !== null && m.type === en && m.key === null && (m = m.props.children), typeof m == "object" && m !== null) {
      switch (m.$$typeof) {
        case Sr:
          e: {
            for (var C = m.key, E = c; E !== null; ) {
              if (E.key === C) {
                if (C = m.type, C === en) {
                  if (E.tag === 7) {
                    n(d, E.sibling), c = l(E, m.props.children), c.return = d, d = c;
                    break e;
                  }
                } else if (E.elementType === C || typeof C == "object" && C !== null && C.$$typeof === mt && Qa(C) === E.type) {
                  n(d, E.sibling), c = l(E, m.props), c.ref = Dn(d, E, m), c.return = d, d = c;
                  break e;
                }
                n(d, E);
                break;
              } else t(d, E);
              E = E.sibling;
            }
            m.type === en ? (c = Ht(m.props.children, d.mode, x, m.key), c.return = d, d = c) : (x = Zr(m.type, m.key, m.props, null, d.mode, x), x.ref = Dn(d, c, m), x.return = d, d = x);
          }
          return i(d);
        case bt:
          e: {
            for (E = m.key; c !== null; ) {
              if (c.key === E) if (c.tag === 4 && c.stateNode.containerInfo === m.containerInfo && c.stateNode.implementation === m.implementation) {
                n(d, c.sibling), c = l(c, m.children || []), c.return = d, d = c;
                break e;
              } else {
                n(d, c);
                break;
              }
              else t(d, c);
              c = c.sibling;
            }
            c = fo(m, d.mode, x), c.return = d, d = c;
          }
          return i(d);
        case mt:
          return E = m._init, N(d, c, E(m._payload), x);
      }
      if (In(m)) return w(d, c, m, x);
      if (zn(m)) return k(d, c, m, x);
      Dr(d, m);
    }
    return typeof m == "string" && m !== "" || typeof m == "number" ? (m = "" + m, c !== null && c.tag === 6 ? (n(d, c.sibling), c = l(c, m), c.return = d, d = c) : (n(d, c), c = co(m, d.mode, x), c.return = d, d = c), i(d)) : n(d, c);
  }
  return N;
}
var kn = $u(!0), Ou = $u(!1), dl = Lt(null), fl = null, un = null, Di = null;
function Ri() {
  Di = un = fl = null;
}
function Fi(e) {
  var t = dl.current;
  W(dl), e._currentValue = t;
}
function Go(e, t, n) {
  for (; e !== null; ) {
    var r = e.alternate;
    if ((e.childLanes & t) !== t ? (e.childLanes |= t, r !== null && (r.childLanes |= t)) : r !== null && (r.childLanes & t) !== t && (r.childLanes |= t), e === n) break;
    e = e.return;
  }
}
function vn(e, t) {
  fl = e, Di = un = null, e = e.dependencies, e !== null && e.firstContext !== null && (e.lanes & t && (Ce = !0), e.firstContext = null);
}
function Ae(e) {
  var t = e._currentValue;
  if (Di !== e) if (e = { context: e, memoizedValue: t, next: null }, un === null) {
    if (fl === null) throw Error(j(308));
    un = e, fl.dependencies = { lanes: 0, firstContext: e };
  } else un = un.next = e;
  return t;
}
var Ut = null;
function $i(e) {
  Ut === null ? Ut = [e] : Ut.push(e);
}
function Iu(e, t, n, r) {
  var l = t.interleaved;
  return l === null ? (n.next = n, $i(t)) : (n.next = l.next, l.next = n), t.interleaved = n, ut(e, r);
}
function ut(e, t) {
  e.lanes |= t;
  var n = e.alternate;
  for (n !== null && (n.lanes |= t), n = e, e = e.return; e !== null; ) e.childLanes |= t, n = e.alternate, n !== null && (n.childLanes |= t), n = e, e = e.return;
  return n.tag === 3 ? n.stateNode : null;
}
var ht = !1;
function Oi(e) {
  e.updateQueue = { baseState: e.memoizedState, firstBaseUpdate: null, lastBaseUpdate: null, shared: { pending: null, interleaved: null, lanes: 0 }, effects: null };
}
function Uu(e, t) {
  e = e.updateQueue, t.updateQueue === e && (t.updateQueue = { baseState: e.baseState, firstBaseUpdate: e.firstBaseUpdate, lastBaseUpdate: e.lastBaseUpdate, shared: e.shared, effects: e.effects });
}
function it(e, t) {
  return { eventTime: e, lane: t, tag: 0, payload: null, callback: null, next: null };
}
function Nt(e, t, n) {
  var r = e.updateQueue;
  if (r === null) return null;
  if (r = r.shared, U & 2) {
    var l = r.pending;
    return l === null ? t.next = t : (t.next = l.next, l.next = t), r.pending = t, ut(e, n);
  }
  return l = r.interleaved, l === null ? (t.next = t, $i(r)) : (t.next = l.next, l.next = t), r.interleaved = t, ut(e, n);
}
function Wr(e, t, n) {
  if (t = t.updateQueue, t !== null && (t = t.shared, (n & 4194240) !== 0)) {
    var r = t.lanes;
    r &= e.pendingLanes, n |= r, t.lanes = n, Si(e, n);
  }
}
function Ga(e, t) {
  var n = e.updateQueue, r = e.alternate;
  if (r !== null && (r = r.updateQueue, n === r)) {
    var l = null, o = null;
    if (n = n.firstBaseUpdate, n !== null) {
      do {
        var i = { eventTime: n.eventTime, lane: n.lane, tag: n.tag, payload: n.payload, callback: n.callback, next: null };
        o === null ? l = o = i : o = o.next = i, n = n.next;
      } while (n !== null);
      o === null ? l = o = t : o = o.next = t;
    } else l = o = t;
    n = { baseState: r.baseState, firstBaseUpdate: l, lastBaseUpdate: o, shared: r.shared, effects: r.effects }, e.updateQueue = n;
    return;
  }
  e = n.lastBaseUpdate, e === null ? n.firstBaseUpdate = t : e.next = t, n.lastBaseUpdate = t;
}
function pl(e, t, n, r) {
  var l = e.updateQueue;
  ht = !1;
  var o = l.firstBaseUpdate, i = l.lastBaseUpdate, a = l.shared.pending;
  if (a !== null) {
    l.shared.pending = null;
    var s = a, f = s.next;
    s.next = null, i === null ? o = f : i.next = f, i = s;
    var h = e.alternate;
    h !== null && (h = h.updateQueue, a = h.lastBaseUpdate, a !== i && (a === null ? h.firstBaseUpdate = f : a.next = f, h.lastBaseUpdate = s));
  }
  if (o !== null) {
    var v = l.baseState;
    i = 0, h = f = s = null, a = o;
    do {
      var p = a.lane, g = a.eventTime;
      if ((r & p) === p) {
        h !== null && (h = h.next = {
          eventTime: g,
          lane: 0,
          tag: a.tag,
          payload: a.payload,
          callback: a.callback,
          next: null
        });
        e: {
          var w = e, k = a;
          switch (p = t, g = n, k.tag) {
            case 1:
              if (w = k.payload, typeof w == "function") {
                v = w.call(g, v, p);
                break e;
              }
              v = w;
              break e;
            case 3:
              w.flags = w.flags & -65537 | 128;
            case 0:
              if (w = k.payload, p = typeof w == "function" ? w.call(g, v, p) : w, p == null) break e;
              v = Z({}, v, p);
              break e;
            case 2:
              ht = !0;
          }
        }
        a.callback !== null && a.lane !== 0 && (e.flags |= 64, p = l.effects, p === null ? l.effects = [a] : p.push(a));
      } else g = { eventTime: g, lane: p, tag: a.tag, payload: a.payload, callback: a.callback, next: null }, h === null ? (f = h = g, s = v) : h = h.next = g, i |= p;
      if (a = a.next, a === null) {
        if (a = l.shared.pending, a === null) break;
        p = a, a = p.next, p.next = null, l.lastBaseUpdate = p, l.shared.pending = null;
      }
    } while (!0);
    if (h === null && (s = v), l.baseState = s, l.firstBaseUpdate = f, l.lastBaseUpdate = h, t = l.shared.interleaved, t !== null) {
      l = t;
      do
        i |= l.lane, l = l.next;
      while (l !== t);
    } else o === null && (l.shared.lanes = 0);
    Gt |= i, e.lanes = i, e.memoizedState = v;
  }
}
function Ka(e, t, n) {
  if (e = t.effects, t.effects = null, e !== null) for (t = 0; t < e.length; t++) {
    var r = e[t], l = r.callback;
    if (l !== null) {
      if (r.callback = null, r = n, typeof l != "function") throw Error(j(191, l));
      l.call(r);
    }
  }
}
var gr = {}, tt = Lt(gr), ar = Lt(gr), sr = Lt(gr);
function At(e) {
  if (e === gr) throw Error(j(174));
  return e;
}
function Ii(e, t) {
  switch (H(sr, t), H(ar, e), H(tt, gr), e = t.nodeType, e) {
    case 9:
    case 11:
      t = (t = t.documentElement) ? t.namespaceURI : Eo(null, "");
      break;
    default:
      e = e === 8 ? t.parentNode : t, t = e.namespaceURI || null, e = e.tagName, t = Eo(t, e);
  }
  W(tt), H(tt, t);
}
function Sn() {
  W(tt), W(ar), W(sr);
}
function Au(e) {
  At(sr.current);
  var t = At(tt.current), n = Eo(t, e.type);
  t !== n && (H(ar, e), H(tt, n));
}
function Ui(e) {
  ar.current === e && (W(tt), W(ar));
}
var Y = Lt(0);
function ml(e) {
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
var lo = [];
function Ai() {
  for (var e = 0; e < lo.length; e++) lo[e]._workInProgressVersionPrimary = null;
  lo.length = 0;
}
var Qr = dt.ReactCurrentDispatcher, oo = dt.ReactCurrentBatchConfig, Qt = 0, X = null, re = null, oe = null, hl = !1, Gn = !1, ur = 0, Df = 0;
function me() {
  throw Error(j(321));
}
function Vi(e, t) {
  if (t === null) return !1;
  for (var n = 0; n < t.length && n < e.length; n++) if (!Ye(e[n], t[n])) return !1;
  return !0;
}
function Hi(e, t, n, r, l, o) {
  if (Qt = o, X = t, t.memoizedState = null, t.updateQueue = null, t.lanes = 0, Qr.current = e === null || e.memoizedState === null ? Of : If, e = n(r, l), Gn) {
    o = 0;
    do {
      if (Gn = !1, ur = 0, 25 <= o) throw Error(j(301));
      o += 1, oe = re = null, t.updateQueue = null, Qr.current = Uf, e = n(r, l);
    } while (Gn);
  }
  if (Qr.current = vl, t = re !== null && re.next !== null, Qt = 0, oe = re = X = null, hl = !1, t) throw Error(j(300));
  return e;
}
function Bi() {
  var e = ur !== 0;
  return ur = 0, e;
}
function qe() {
  var e = { memoizedState: null, baseState: null, baseQueue: null, queue: null, next: null };
  return oe === null ? X.memoizedState = oe = e : oe = oe.next = e, oe;
}
function Ve() {
  if (re === null) {
    var e = X.alternate;
    e = e !== null ? e.memoizedState : null;
  } else e = re.next;
  var t = oe === null ? X.memoizedState : oe.next;
  if (t !== null) oe = t, re = e;
  else {
    if (e === null) throw Error(j(310));
    re = e, e = { memoizedState: re.memoizedState, baseState: re.baseState, baseQueue: re.baseQueue, queue: re.queue, next: null }, oe === null ? X.memoizedState = oe = e : oe = oe.next = e;
  }
  return oe;
}
function cr(e, t) {
  return typeof t == "function" ? t(e) : t;
}
function io(e) {
  var t = Ve(), n = t.queue;
  if (n === null) throw Error(j(311));
  n.lastRenderedReducer = e;
  var r = re, l = r.baseQueue, o = n.pending;
  if (o !== null) {
    if (l !== null) {
      var i = l.next;
      l.next = o.next, o.next = i;
    }
    r.baseQueue = l = o, n.pending = null;
  }
  if (l !== null) {
    o = l.next, r = r.baseState;
    var a = i = null, s = null, f = o;
    do {
      var h = f.lane;
      if ((Qt & h) === h) s !== null && (s = s.next = { lane: 0, action: f.action, hasEagerState: f.hasEagerState, eagerState: f.eagerState, next: null }), r = f.hasEagerState ? f.eagerState : e(r, f.action);
      else {
        var v = {
          lane: h,
          action: f.action,
          hasEagerState: f.hasEagerState,
          eagerState: f.eagerState,
          next: null
        };
        s === null ? (a = s = v, i = r) : s = s.next = v, X.lanes |= h, Gt |= h;
      }
      f = f.next;
    } while (f !== null && f !== o);
    s === null ? i = r : s.next = a, Ye(r, t.memoizedState) || (Ce = !0), t.memoizedState = r, t.baseState = i, t.baseQueue = s, n.lastRenderedState = r;
  }
  if (e = n.interleaved, e !== null) {
    l = e;
    do
      o = l.lane, X.lanes |= o, Gt |= o, l = l.next;
    while (l !== e);
  } else l === null && (n.lanes = 0);
  return [t.memoizedState, n.dispatch];
}
function ao(e) {
  var t = Ve(), n = t.queue;
  if (n === null) throw Error(j(311));
  n.lastRenderedReducer = e;
  var r = n.dispatch, l = n.pending, o = t.memoizedState;
  if (l !== null) {
    n.pending = null;
    var i = l = l.next;
    do
      o = e(o, i.action), i = i.next;
    while (i !== l);
    Ye(o, t.memoizedState) || (Ce = !0), t.memoizedState = o, t.baseQueue === null && (t.baseState = o), n.lastRenderedState = o;
  }
  return [o, r];
}
function Vu() {
}
function Hu(e, t) {
  var n = X, r = Ve(), l = t(), o = !Ye(r.memoizedState, l);
  if (o && (r.memoizedState = l, Ce = !0), r = r.queue, Wi(Qu.bind(null, n, r, e), [e]), r.getSnapshot !== t || o || oe !== null && oe.memoizedState.tag & 1) {
    if (n.flags |= 2048, dr(9, Wu.bind(null, n, r, l, t), void 0, null), ie === null) throw Error(j(349));
    Qt & 30 || Bu(n, t, l);
  }
  return l;
}
function Bu(e, t, n) {
  e.flags |= 16384, e = { getSnapshot: t, value: n }, t = X.updateQueue, t === null ? (t = { lastEffect: null, stores: null }, X.updateQueue = t, t.stores = [e]) : (n = t.stores, n === null ? t.stores = [e] : n.push(e));
}
function Wu(e, t, n, r) {
  t.value = n, t.getSnapshot = r, Gu(t) && Ku(e);
}
function Qu(e, t, n) {
  return n(function() {
    Gu(t) && Ku(e);
  });
}
function Gu(e) {
  var t = e.getSnapshot;
  e = e.value;
  try {
    var n = t();
    return !Ye(e, n);
  } catch {
    return !0;
  }
}
function Ku(e) {
  var t = ut(e, 1);
  t !== null && Ke(t, e, 1, -1);
}
function Ya(e) {
  var t = qe();
  return typeof e == "function" && (e = e()), t.memoizedState = t.baseState = e, e = { pending: null, interleaved: null, lanes: 0, dispatch: null, lastRenderedReducer: cr, lastRenderedState: e }, t.queue = e, e = e.dispatch = $f.bind(null, X, e), [t.memoizedState, e];
}
function dr(e, t, n, r) {
  return e = { tag: e, create: t, destroy: n, deps: r, next: null }, t = X.updateQueue, t === null ? (t = { lastEffect: null, stores: null }, X.updateQueue = t, t.lastEffect = e.next = e) : (n = t.lastEffect, n === null ? t.lastEffect = e.next = e : (r = n.next, n.next = e, e.next = r, t.lastEffect = e)), e;
}
function Yu() {
  return Ve().memoizedState;
}
function Gr(e, t, n, r) {
  var l = qe();
  X.flags |= e, l.memoizedState = dr(1 | t, n, void 0, r === void 0 ? null : r);
}
function Pl(e, t, n, r) {
  var l = Ve();
  r = r === void 0 ? null : r;
  var o = void 0;
  if (re !== null) {
    var i = re.memoizedState;
    if (o = i.destroy, r !== null && Vi(r, i.deps)) {
      l.memoizedState = dr(t, n, o, r);
      return;
    }
  }
  X.flags |= e, l.memoizedState = dr(1 | t, n, o, r);
}
function Xa(e, t) {
  return Gr(8390656, 8, e, t);
}
function Wi(e, t) {
  return Pl(2048, 8, e, t);
}
function Xu(e, t) {
  return Pl(4, 2, e, t);
}
function Zu(e, t) {
  return Pl(4, 4, e, t);
}
function Ju(e, t) {
  if (typeof t == "function") return e = e(), t(e), function() {
    t(null);
  };
  if (t != null) return e = e(), t.current = e, function() {
    t.current = null;
  };
}
function qu(e, t, n) {
  return n = n != null ? n.concat([e]) : null, Pl(4, 4, Ju.bind(null, t, e), n);
}
function Qi() {
}
function bu(e, t) {
  var n = Ve();
  t = t === void 0 ? null : t;
  var r = n.memoizedState;
  return r !== null && t !== null && Vi(t, r[1]) ? r[0] : (n.memoizedState = [e, t], e);
}
function ec(e, t) {
  var n = Ve();
  t = t === void 0 ? null : t;
  var r = n.memoizedState;
  return r !== null && t !== null && Vi(t, r[1]) ? r[0] : (e = e(), n.memoizedState = [e, t], e);
}
function tc(e, t, n) {
  return Qt & 21 ? (Ye(n, t) || (n = iu(), X.lanes |= n, Gt |= n, e.baseState = !0), t) : (e.baseState && (e.baseState = !1, Ce = !0), e.memoizedState = n);
}
function Rf(e, t) {
  var n = V;
  V = n !== 0 && 4 > n ? n : 4, e(!0);
  var r = oo.transition;
  oo.transition = {};
  try {
    e(!1), t();
  } finally {
    V = n, oo.transition = r;
  }
}
function nc() {
  return Ve().memoizedState;
}
function Ff(e, t, n) {
  var r = Et(e);
  if (n = { lane: r, action: n, hasEagerState: !1, eagerState: null, next: null }, rc(e)) lc(t, n);
  else if (n = Iu(e, t, n, r), n !== null) {
    var l = we();
    Ke(n, e, r, l), oc(n, t, r);
  }
}
function $f(e, t, n) {
  var r = Et(e), l = { lane: r, action: n, hasEagerState: !1, eagerState: null, next: null };
  if (rc(e)) lc(t, l);
  else {
    var o = e.alternate;
    if (e.lanes === 0 && (o === null || o.lanes === 0) && (o = t.lastRenderedReducer, o !== null)) try {
      var i = t.lastRenderedState, a = o(i, n);
      if (l.hasEagerState = !0, l.eagerState = a, Ye(a, i)) {
        var s = t.interleaved;
        s === null ? (l.next = l, $i(t)) : (l.next = s.next, s.next = l), t.interleaved = l;
        return;
      }
    } catch {
    } finally {
    }
    n = Iu(e, t, l, r), n !== null && (l = we(), Ke(n, e, r, l), oc(n, t, r));
  }
}
function rc(e) {
  var t = e.alternate;
  return e === X || t !== null && t === X;
}
function lc(e, t) {
  Gn = hl = !0;
  var n = e.pending;
  n === null ? t.next = t : (t.next = n.next, n.next = t), e.pending = t;
}
function oc(e, t, n) {
  if (n & 4194240) {
    var r = t.lanes;
    r &= e.pendingLanes, n |= r, t.lanes = n, Si(e, n);
  }
}
var vl = { readContext: Ae, useCallback: me, useContext: me, useEffect: me, useImperativeHandle: me, useInsertionEffect: me, useLayoutEffect: me, useMemo: me, useReducer: me, useRef: me, useState: me, useDebugValue: me, useDeferredValue: me, useTransition: me, useMutableSource: me, useSyncExternalStore: me, useId: me, unstable_isNewReconciler: !1 }, Of = { readContext: Ae, useCallback: function(e, t) {
  return qe().memoizedState = [e, t === void 0 ? null : t], e;
}, useContext: Ae, useEffect: Xa, useImperativeHandle: function(e, t, n) {
  return n = n != null ? n.concat([e]) : null, Gr(
    4194308,
    4,
    Ju.bind(null, t, e),
    n
  );
}, useLayoutEffect: function(e, t) {
  return Gr(4194308, 4, e, t);
}, useInsertionEffect: function(e, t) {
  return Gr(4, 2, e, t);
}, useMemo: function(e, t) {
  var n = qe();
  return t = t === void 0 ? null : t, e = e(), n.memoizedState = [e, t], e;
}, useReducer: function(e, t, n) {
  var r = qe();
  return t = n !== void 0 ? n(t) : t, r.memoizedState = r.baseState = t, e = { pending: null, interleaved: null, lanes: 0, dispatch: null, lastRenderedReducer: e, lastRenderedState: t }, r.queue = e, e = e.dispatch = Ff.bind(null, X, e), [r.memoizedState, e];
}, useRef: function(e) {
  var t = qe();
  return e = { current: e }, t.memoizedState = e;
}, useState: Ya, useDebugValue: Qi, useDeferredValue: function(e) {
  return qe().memoizedState = e;
}, useTransition: function() {
  var e = Ya(!1), t = e[0];
  return e = Rf.bind(null, e[1]), qe().memoizedState = e, [t, e];
}, useMutableSource: function() {
}, useSyncExternalStore: function(e, t, n) {
  var r = X, l = qe();
  if (K) {
    if (n === void 0) throw Error(j(407));
    n = n();
  } else {
    if (n = t(), ie === null) throw Error(j(349));
    Qt & 30 || Bu(r, t, n);
  }
  l.memoizedState = n;
  var o = { value: n, getSnapshot: t };
  return l.queue = o, Xa(Qu.bind(
    null,
    r,
    o,
    e
  ), [e]), r.flags |= 2048, dr(9, Wu.bind(null, r, o, n, t), void 0, null), n;
}, useId: function() {
  var e = qe(), t = ie.identifierPrefix;
  if (K) {
    var n = ot, r = lt;
    n = (r & ~(1 << 32 - Ge(r) - 1)).toString(32) + n, t = ":" + t + "R" + n, n = ur++, 0 < n && (t += "H" + n.toString(32)), t += ":";
  } else n = Df++, t = ":" + t + "r" + n.toString(32) + ":";
  return e.memoizedState = t;
}, unstable_isNewReconciler: !1 }, If = {
  readContext: Ae,
  useCallback: bu,
  useContext: Ae,
  useEffect: Wi,
  useImperativeHandle: qu,
  useInsertionEffect: Xu,
  useLayoutEffect: Zu,
  useMemo: ec,
  useReducer: io,
  useRef: Yu,
  useState: function() {
    return io(cr);
  },
  useDebugValue: Qi,
  useDeferredValue: function(e) {
    var t = Ve();
    return tc(t, re.memoizedState, e);
  },
  useTransition: function() {
    var e = io(cr)[0], t = Ve().memoizedState;
    return [e, t];
  },
  useMutableSource: Vu,
  useSyncExternalStore: Hu,
  useId: nc,
  unstable_isNewReconciler: !1
}, Uf = { readContext: Ae, useCallback: bu, useContext: Ae, useEffect: Wi, useImperativeHandle: qu, useInsertionEffect: Xu, useLayoutEffect: Zu, useMemo: ec, useReducer: ao, useRef: Yu, useState: function() {
  return ao(cr);
}, useDebugValue: Qi, useDeferredValue: function(e) {
  var t = Ve();
  return re === null ? t.memoizedState = e : tc(t, re.memoizedState, e);
}, useTransition: function() {
  var e = ao(cr)[0], t = Ve().memoizedState;
  return [e, t];
}, useMutableSource: Vu, useSyncExternalStore: Hu, useId: nc, unstable_isNewReconciler: !1 };
function Be(e, t) {
  if (e && e.defaultProps) {
    t = Z({}, t), e = e.defaultProps;
    for (var n in e) t[n] === void 0 && (t[n] = e[n]);
    return t;
  }
  return t;
}
function Ko(e, t, n, r) {
  t = e.memoizedState, n = n(r, t), n = n == null ? t : Z({}, t, n), e.memoizedState = n, e.lanes === 0 && (e.updateQueue.baseState = n);
}
var Tl = { isMounted: function(e) {
  return (e = e._reactInternals) ? Xt(e) === e : !1;
}, enqueueSetState: function(e, t, n) {
  e = e._reactInternals;
  var r = we(), l = Et(e), o = it(r, l);
  o.payload = t, n != null && (o.callback = n), t = Nt(e, o, l), t !== null && (Ke(t, e, l, r), Wr(t, e, l));
}, enqueueReplaceState: function(e, t, n) {
  e = e._reactInternals;
  var r = we(), l = Et(e), o = it(r, l);
  o.tag = 1, o.payload = t, n != null && (o.callback = n), t = Nt(e, o, l), t !== null && (Ke(t, e, l, r), Wr(t, e, l));
}, enqueueForceUpdate: function(e, t) {
  e = e._reactInternals;
  var n = we(), r = Et(e), l = it(n, r);
  l.tag = 2, t != null && (l.callback = t), t = Nt(e, l, r), t !== null && (Ke(t, e, r, n), Wr(t, e, r));
} };
function Za(e, t, n, r, l, o, i) {
  return e = e.stateNode, typeof e.shouldComponentUpdate == "function" ? e.shouldComponentUpdate(r, o, i) : t.prototype && t.prototype.isPureReactComponent ? !rr(n, r) || !rr(l, o) : !0;
}
function ic(e, t, n) {
  var r = !1, l = Pt, o = t.contextType;
  return typeof o == "object" && o !== null ? o = Ae(o) : (l = _e(t) ? Bt : ge.current, r = t.contextTypes, o = (r = r != null) ? yn(e, l) : Pt), t = new t(n, o), e.memoizedState = t.state !== null && t.state !== void 0 ? t.state : null, t.updater = Tl, e.stateNode = t, t._reactInternals = e, r && (e = e.stateNode, e.__reactInternalMemoizedUnmaskedChildContext = l, e.__reactInternalMemoizedMaskedChildContext = o), t;
}
function Ja(e, t, n, r) {
  e = t.state, typeof t.componentWillReceiveProps == "function" && t.componentWillReceiveProps(n, r), typeof t.UNSAFE_componentWillReceiveProps == "function" && t.UNSAFE_componentWillReceiveProps(n, r), t.state !== e && Tl.enqueueReplaceState(t, t.state, null);
}
function Yo(e, t, n, r) {
  var l = e.stateNode;
  l.props = n, l.state = e.memoizedState, l.refs = {}, Oi(e);
  var o = t.contextType;
  typeof o == "object" && o !== null ? l.context = Ae(o) : (o = _e(t) ? Bt : ge.current, l.context = yn(e, o)), l.state = e.memoizedState, o = t.getDerivedStateFromProps, typeof o == "function" && (Ko(e, t, o, n), l.state = e.memoizedState), typeof t.getDerivedStateFromProps == "function" || typeof l.getSnapshotBeforeUpdate == "function" || typeof l.UNSAFE_componentWillMount != "function" && typeof l.componentWillMount != "function" || (t = l.state, typeof l.componentWillMount == "function" && l.componentWillMount(), typeof l.UNSAFE_componentWillMount == "function" && l.UNSAFE_componentWillMount(), t !== l.state && Tl.enqueueReplaceState(l, l.state, null), pl(e, n, l, r), l.state = e.memoizedState), typeof l.componentDidMount == "function" && (e.flags |= 4194308);
}
function jn(e, t) {
  try {
    var n = "", r = t;
    do
      n += fd(r), r = r.return;
    while (r);
    var l = n;
  } catch (o) {
    l = `
Error generating stack: ` + o.message + `
` + o.stack;
  }
  return { value: e, source: t, stack: l, digest: null };
}
function so(e, t, n) {
  return { value: e, source: null, stack: n ?? null, digest: t ?? null };
}
function Xo(e, t) {
  try {
    console.error(t.value);
  } catch (n) {
    setTimeout(function() {
      throw n;
    });
  }
}
var Af = typeof WeakMap == "function" ? WeakMap : Map;
function ac(e, t, n) {
  n = it(-1, n), n.tag = 3, n.payload = { element: null };
  var r = t.value;
  return n.callback = function() {
    xl || (xl = !0, oi = r), Xo(e, t);
  }, n;
}
function sc(e, t, n) {
  n = it(-1, n), n.tag = 3;
  var r = e.type.getDerivedStateFromError;
  if (typeof r == "function") {
    var l = t.value;
    n.payload = function() {
      return r(l);
    }, n.callback = function() {
      Xo(e, t);
    };
  }
  var o = e.stateNode;
  return o !== null && typeof o.componentDidCatch == "function" && (n.callback = function() {
    Xo(e, t), typeof r != "function" && (Ct === null ? Ct = /* @__PURE__ */ new Set([this]) : Ct.add(this));
    var i = t.stack;
    this.componentDidCatch(t.value, { componentStack: i !== null ? i : "" });
  }), n;
}
function qa(e, t, n) {
  var r = e.pingCache;
  if (r === null) {
    r = e.pingCache = new Af();
    var l = /* @__PURE__ */ new Set();
    r.set(t, l);
  } else l = r.get(t), l === void 0 && (l = /* @__PURE__ */ new Set(), r.set(t, l));
  l.has(n) || (l.add(n), e = ep.bind(null, e, t, n), t.then(e, e));
}
function ba(e) {
  do {
    var t;
    if ((t = e.tag === 13) && (t = e.memoizedState, t = t !== null ? t.dehydrated !== null : !0), t) return e;
    e = e.return;
  } while (e !== null);
  return null;
}
function es(e, t, n, r, l) {
  return e.mode & 1 ? (e.flags |= 65536, e.lanes = l, e) : (e === t ? e.flags |= 65536 : (e.flags |= 128, n.flags |= 131072, n.flags &= -52805, n.tag === 1 && (n.alternate === null ? n.tag = 17 : (t = it(-1, 1), t.tag = 2, Nt(n, t, 1))), n.lanes |= 1), e);
}
var Vf = dt.ReactCurrentOwner, Ce = !1;
function ye(e, t, n, r) {
  t.child = e === null ? Ou(t, null, n, r) : kn(t, e.child, n, r);
}
function ts(e, t, n, r, l) {
  n = n.render;
  var o = t.ref;
  return vn(t, l), r = Hi(e, t, n, r, o, l), n = Bi(), e !== null && !Ce ? (t.updateQueue = e.updateQueue, t.flags &= -2053, e.lanes &= ~l, ct(e, t, l)) : (K && n && Ti(t), t.flags |= 1, ye(e, t, r, l), t.child);
}
function ns(e, t, n, r, l) {
  if (e === null) {
    var o = n.type;
    return typeof o == "function" && !bi(o) && o.defaultProps === void 0 && n.compare === null && n.defaultProps === void 0 ? (t.tag = 15, t.type = o, uc(e, t, o, r, l)) : (e = Zr(n.type, null, r, t, t.mode, l), e.ref = t.ref, e.return = t, t.child = e);
  }
  if (o = e.child, !(e.lanes & l)) {
    var i = o.memoizedProps;
    if (n = n.compare, n = n !== null ? n : rr, n(i, r) && e.ref === t.ref) return ct(e, t, l);
  }
  return t.flags |= 1, e = _t(o, r), e.ref = t.ref, e.return = t, t.child = e;
}
function uc(e, t, n, r, l) {
  if (e !== null) {
    var o = e.memoizedProps;
    if (rr(o, r) && e.ref === t.ref) if (Ce = !1, t.pendingProps = r = o, (e.lanes & l) !== 0) e.flags & 131072 && (Ce = !0);
    else return t.lanes = e.lanes, ct(e, t, l);
  }
  return Zo(e, t, n, r, l);
}
function cc(e, t, n) {
  var r = t.pendingProps, l = r.children, o = e !== null ? e.memoizedState : null;
  if (r.mode === "hidden") if (!(t.mode & 1)) t.memoizedState = { baseLanes: 0, cachePool: null, transitions: null }, H(dn, Pe), Pe |= n;
  else {
    if (!(n & 1073741824)) return e = o !== null ? o.baseLanes | n : n, t.lanes = t.childLanes = 1073741824, t.memoizedState = { baseLanes: e, cachePool: null, transitions: null }, t.updateQueue = null, H(dn, Pe), Pe |= e, null;
    t.memoizedState = { baseLanes: 0, cachePool: null, transitions: null }, r = o !== null ? o.baseLanes : n, H(dn, Pe), Pe |= r;
  }
  else o !== null ? (r = o.baseLanes | n, t.memoizedState = null) : r = n, H(dn, Pe), Pe |= r;
  return ye(e, t, l, n), t.child;
}
function dc(e, t) {
  var n = t.ref;
  (e === null && n !== null || e !== null && e.ref !== n) && (t.flags |= 512, t.flags |= 2097152);
}
function Zo(e, t, n, r, l) {
  var o = _e(n) ? Bt : ge.current;
  return o = yn(t, o), vn(t, l), n = Hi(e, t, n, r, o, l), r = Bi(), e !== null && !Ce ? (t.updateQueue = e.updateQueue, t.flags &= -2053, e.lanes &= ~l, ct(e, t, l)) : (K && r && Ti(t), t.flags |= 1, ye(e, t, n, l), t.child);
}
function rs(e, t, n, r, l) {
  if (_e(n)) {
    var o = !0;
    sl(t);
  } else o = !1;
  if (vn(t, l), t.stateNode === null) Kr(e, t), ic(t, n, r), Yo(t, n, r, l), r = !0;
  else if (e === null) {
    var i = t.stateNode, a = t.memoizedProps;
    i.props = a;
    var s = i.context, f = n.contextType;
    typeof f == "object" && f !== null ? f = Ae(f) : (f = _e(n) ? Bt : ge.current, f = yn(t, f));
    var h = n.getDerivedStateFromProps, v = typeof h == "function" || typeof i.getSnapshotBeforeUpdate == "function";
    v || typeof i.UNSAFE_componentWillReceiveProps != "function" && typeof i.componentWillReceiveProps != "function" || (a !== r || s !== f) && Ja(t, i, r, f), ht = !1;
    var p = t.memoizedState;
    i.state = p, pl(t, r, i, l), s = t.memoizedState, a !== r || p !== s || Ee.current || ht ? (typeof h == "function" && (Ko(t, n, h, r), s = t.memoizedState), (a = ht || Za(t, n, a, r, p, s, f)) ? (v || typeof i.UNSAFE_componentWillMount != "function" && typeof i.componentWillMount != "function" || (typeof i.componentWillMount == "function" && i.componentWillMount(), typeof i.UNSAFE_componentWillMount == "function" && i.UNSAFE_componentWillMount()), typeof i.componentDidMount == "function" && (t.flags |= 4194308)) : (typeof i.componentDidMount == "function" && (t.flags |= 4194308), t.memoizedProps = r, t.memoizedState = s), i.props = r, i.state = s, i.context = f, r = a) : (typeof i.componentDidMount == "function" && (t.flags |= 4194308), r = !1);
  } else {
    i = t.stateNode, Uu(e, t), a = t.memoizedProps, f = t.type === t.elementType ? a : Be(t.type, a), i.props = f, v = t.pendingProps, p = i.context, s = n.contextType, typeof s == "object" && s !== null ? s = Ae(s) : (s = _e(n) ? Bt : ge.current, s = yn(t, s));
    var g = n.getDerivedStateFromProps;
    (h = typeof g == "function" || typeof i.getSnapshotBeforeUpdate == "function") || typeof i.UNSAFE_componentWillReceiveProps != "function" && typeof i.componentWillReceiveProps != "function" || (a !== v || p !== s) && Ja(t, i, r, s), ht = !1, p = t.memoizedState, i.state = p, pl(t, r, i, l);
    var w = t.memoizedState;
    a !== v || p !== w || Ee.current || ht ? (typeof g == "function" && (Ko(t, n, g, r), w = t.memoizedState), (f = ht || Za(t, n, f, r, p, w, s) || !1) ? (h || typeof i.UNSAFE_componentWillUpdate != "function" && typeof i.componentWillUpdate != "function" || (typeof i.componentWillUpdate == "function" && i.componentWillUpdate(r, w, s), typeof i.UNSAFE_componentWillUpdate == "function" && i.UNSAFE_componentWillUpdate(r, w, s)), typeof i.componentDidUpdate == "function" && (t.flags |= 4), typeof i.getSnapshotBeforeUpdate == "function" && (t.flags |= 1024)) : (typeof i.componentDidUpdate != "function" || a === e.memoizedProps && p === e.memoizedState || (t.flags |= 4), typeof i.getSnapshotBeforeUpdate != "function" || a === e.memoizedProps && p === e.memoizedState || (t.flags |= 1024), t.memoizedProps = r, t.memoizedState = w), i.props = r, i.state = w, i.context = s, r = f) : (typeof i.componentDidUpdate != "function" || a === e.memoizedProps && p === e.memoizedState || (t.flags |= 4), typeof i.getSnapshotBeforeUpdate != "function" || a === e.memoizedProps && p === e.memoizedState || (t.flags |= 1024), r = !1);
  }
  return Jo(e, t, n, r, o, l);
}
function Jo(e, t, n, r, l, o) {
  dc(e, t);
  var i = (t.flags & 128) !== 0;
  if (!r && !i) return l && Ha(t, n, !1), ct(e, t, o);
  r = t.stateNode, Vf.current = t;
  var a = i && typeof n.getDerivedStateFromError != "function" ? null : r.render();
  return t.flags |= 1, e !== null && i ? (t.child = kn(t, e.child, null, o), t.child = kn(t, null, a, o)) : ye(e, t, a, o), t.memoizedState = r.state, l && Ha(t, n, !0), t.child;
}
function fc(e) {
  var t = e.stateNode;
  t.pendingContext ? Va(e, t.pendingContext, t.pendingContext !== t.context) : t.context && Va(e, t.context, !1), Ii(e, t.containerInfo);
}
function ls(e, t, n, r, l) {
  return wn(), Mi(l), t.flags |= 256, ye(e, t, n, r), t.child;
}
var qo = { dehydrated: null, treeContext: null, retryLane: 0 };
function bo(e) {
  return { baseLanes: e, cachePool: null, transitions: null };
}
function pc(e, t, n) {
  var r = t.pendingProps, l = Y.current, o = !1, i = (t.flags & 128) !== 0, a;
  if ((a = i) || (a = e !== null && e.memoizedState === null ? !1 : (l & 2) !== 0), a ? (o = !0, t.flags &= -129) : (e === null || e.memoizedState !== null) && (l |= 1), H(Y, l & 1), e === null)
    return Qo(t), e = t.memoizedState, e !== null && (e = e.dehydrated, e !== null) ? (t.mode & 1 ? e.data === "$!" ? t.lanes = 8 : t.lanes = 1073741824 : t.lanes = 1, null) : (i = r.children, e = r.fallback, o ? (r = t.mode, o = t.child, i = { mode: "hidden", children: i }, !(r & 1) && o !== null ? (o.childLanes = 0, o.pendingProps = i) : o = Dl(i, r, 0, null), e = Ht(e, r, n, null), o.return = t, e.return = t, o.sibling = e, t.child = o, t.child.memoizedState = bo(n), t.memoizedState = qo, e) : Gi(t, i));
  if (l = e.memoizedState, l !== null && (a = l.dehydrated, a !== null)) return Hf(e, t, i, r, a, l, n);
  if (o) {
    o = r.fallback, i = t.mode, l = e.child, a = l.sibling;
    var s = { mode: "hidden", children: r.children };
    return !(i & 1) && t.child !== l ? (r = t.child, r.childLanes = 0, r.pendingProps = s, t.deletions = null) : (r = _t(l, s), r.subtreeFlags = l.subtreeFlags & 14680064), a !== null ? o = _t(a, o) : (o = Ht(o, i, n, null), o.flags |= 2), o.return = t, r.return = t, r.sibling = o, t.child = r, r = o, o = t.child, i = e.child.memoizedState, i = i === null ? bo(n) : { baseLanes: i.baseLanes | n, cachePool: null, transitions: i.transitions }, o.memoizedState = i, o.childLanes = e.childLanes & ~n, t.memoizedState = qo, r;
  }
  return o = e.child, e = o.sibling, r = _t(o, { mode: "visible", children: r.children }), !(t.mode & 1) && (r.lanes = n), r.return = t, r.sibling = null, e !== null && (n = t.deletions, n === null ? (t.deletions = [e], t.flags |= 16) : n.push(e)), t.child = r, t.memoizedState = null, r;
}
function Gi(e, t) {
  return t = Dl({ mode: "visible", children: t }, e.mode, 0, null), t.return = e, e.child = t;
}
function Rr(e, t, n, r) {
  return r !== null && Mi(r), kn(t, e.child, null, n), e = Gi(t, t.pendingProps.children), e.flags |= 2, t.memoizedState = null, e;
}
function Hf(e, t, n, r, l, o, i) {
  if (n)
    return t.flags & 256 ? (t.flags &= -257, r = so(Error(j(422))), Rr(e, t, i, r)) : t.memoizedState !== null ? (t.child = e.child, t.flags |= 128, null) : (o = r.fallback, l = t.mode, r = Dl({ mode: "visible", children: r.children }, l, 0, null), o = Ht(o, l, i, null), o.flags |= 2, r.return = t, o.return = t, r.sibling = o, t.child = r, t.mode & 1 && kn(t, e.child, null, i), t.child.memoizedState = bo(i), t.memoizedState = qo, o);
  if (!(t.mode & 1)) return Rr(e, t, i, null);
  if (l.data === "$!") {
    if (r = l.nextSibling && l.nextSibling.dataset, r) var a = r.dgst;
    return r = a, o = Error(j(419)), r = so(o, r, void 0), Rr(e, t, i, r);
  }
  if (a = (i & e.childLanes) !== 0, Ce || a) {
    if (r = ie, r !== null) {
      switch (i & -i) {
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
      l = l & (r.suspendedLanes | i) ? 0 : l, l !== 0 && l !== o.retryLane && (o.retryLane = l, ut(e, l), Ke(r, e, l, -1));
    }
    return qi(), r = so(Error(j(421))), Rr(e, t, i, r);
  }
  return l.data === "$?" ? (t.flags |= 128, t.child = e.child, t = tp.bind(null, e), l._reactRetry = t, null) : (e = o.treeContext, Te = jt(l.nextSibling), Le = t, K = !0, Qe = null, e !== null && ($e[Oe++] = lt, $e[Oe++] = ot, $e[Oe++] = Wt, lt = e.id, ot = e.overflow, Wt = t), t = Gi(t, r.children), t.flags |= 4096, t);
}
function os(e, t, n) {
  e.lanes |= t;
  var r = e.alternate;
  r !== null && (r.lanes |= t), Go(e.return, t, n);
}
function uo(e, t, n, r, l) {
  var o = e.memoizedState;
  o === null ? e.memoizedState = { isBackwards: t, rendering: null, renderingStartTime: 0, last: r, tail: n, tailMode: l } : (o.isBackwards = t, o.rendering = null, o.renderingStartTime = 0, o.last = r, o.tail = n, o.tailMode = l);
}
function mc(e, t, n) {
  var r = t.pendingProps, l = r.revealOrder, o = r.tail;
  if (ye(e, t, r.children, n), r = Y.current, r & 2) r = r & 1 | 2, t.flags |= 128;
  else {
    if (e !== null && e.flags & 128) e: for (e = t.child; e !== null; ) {
      if (e.tag === 13) e.memoizedState !== null && os(e, n, t);
      else if (e.tag === 19) os(e, n, t);
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
  if (H(Y, r), !(t.mode & 1)) t.memoizedState = null;
  else switch (l) {
    case "forwards":
      for (n = t.child, l = null; n !== null; ) e = n.alternate, e !== null && ml(e) === null && (l = n), n = n.sibling;
      n = l, n === null ? (l = t.child, t.child = null) : (l = n.sibling, n.sibling = null), uo(t, !1, l, n, o);
      break;
    case "backwards":
      for (n = null, l = t.child, t.child = null; l !== null; ) {
        if (e = l.alternate, e !== null && ml(e) === null) {
          t.child = l;
          break;
        }
        e = l.sibling, l.sibling = n, n = l, l = e;
      }
      uo(t, !0, n, null, o);
      break;
    case "together":
      uo(t, !1, null, null, void 0);
      break;
    default:
      t.memoizedState = null;
  }
  return t.child;
}
function Kr(e, t) {
  !(t.mode & 1) && e !== null && (e.alternate = null, t.alternate = null, t.flags |= 2);
}
function ct(e, t, n) {
  if (e !== null && (t.dependencies = e.dependencies), Gt |= t.lanes, !(n & t.childLanes)) return null;
  if (e !== null && t.child !== e.child) throw Error(j(153));
  if (t.child !== null) {
    for (e = t.child, n = _t(e, e.pendingProps), t.child = n, n.return = t; e.sibling !== null; ) e = e.sibling, n = n.sibling = _t(e, e.pendingProps), n.return = t;
    n.sibling = null;
  }
  return t.child;
}
function Bf(e, t, n) {
  switch (t.tag) {
    case 3:
      fc(t), wn();
      break;
    case 5:
      Au(t);
      break;
    case 1:
      _e(t.type) && sl(t);
      break;
    case 4:
      Ii(t, t.stateNode.containerInfo);
      break;
    case 10:
      var r = t.type._context, l = t.memoizedProps.value;
      H(dl, r._currentValue), r._currentValue = l;
      break;
    case 13:
      if (r = t.memoizedState, r !== null)
        return r.dehydrated !== null ? (H(Y, Y.current & 1), t.flags |= 128, null) : n & t.child.childLanes ? pc(e, t, n) : (H(Y, Y.current & 1), e = ct(e, t, n), e !== null ? e.sibling : null);
      H(Y, Y.current & 1);
      break;
    case 19:
      if (r = (n & t.childLanes) !== 0, e.flags & 128) {
        if (r) return mc(e, t, n);
        t.flags |= 128;
      }
      if (l = t.memoizedState, l !== null && (l.rendering = null, l.tail = null, l.lastEffect = null), H(Y, Y.current), r) break;
      return null;
    case 22:
    case 23:
      return t.lanes = 0, cc(e, t, n);
  }
  return ct(e, t, n);
}
var hc, ei, vc, gc;
hc = function(e, t) {
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
ei = function() {
};
vc = function(e, t, n, r) {
  var l = e.memoizedProps;
  if (l !== r) {
    e = t.stateNode, At(tt.current);
    var o = null;
    switch (n) {
      case "input":
        l = So(e, l), r = So(e, r), o = [];
        break;
      case "select":
        l = Z({}, l, { value: void 0 }), r = Z({}, r, { value: void 0 }), o = [];
        break;
      case "textarea":
        l = Co(e, l), r = Co(e, r), o = [];
        break;
      default:
        typeof l.onClick != "function" && typeof r.onClick == "function" && (e.onclick = il);
    }
    _o(n, r);
    var i;
    n = null;
    for (f in l) if (!r.hasOwnProperty(f) && l.hasOwnProperty(f) && l[f] != null) if (f === "style") {
      var a = l[f];
      for (i in a) a.hasOwnProperty(i) && (n || (n = {}), n[i] = "");
    } else f !== "dangerouslySetInnerHTML" && f !== "children" && f !== "suppressContentEditableWarning" && f !== "suppressHydrationWarning" && f !== "autoFocus" && (Zn.hasOwnProperty(f) ? o || (o = []) : (o = o || []).push(f, null));
    for (f in r) {
      var s = r[f];
      if (a = l != null ? l[f] : void 0, r.hasOwnProperty(f) && s !== a && (s != null || a != null)) if (f === "style") if (a) {
        for (i in a) !a.hasOwnProperty(i) || s && s.hasOwnProperty(i) || (n || (n = {}), n[i] = "");
        for (i in s) s.hasOwnProperty(i) && a[i] !== s[i] && (n || (n = {}), n[i] = s[i]);
      } else n || (o || (o = []), o.push(
        f,
        n
      )), n = s;
      else f === "dangerouslySetInnerHTML" ? (s = s ? s.__html : void 0, a = a ? a.__html : void 0, s != null && a !== s && (o = o || []).push(f, s)) : f === "children" ? typeof s != "string" && typeof s != "number" || (o = o || []).push(f, "" + s) : f !== "suppressContentEditableWarning" && f !== "suppressHydrationWarning" && (Zn.hasOwnProperty(f) ? (s != null && f === "onScroll" && B("scroll", e), o || a === s || (o = [])) : (o = o || []).push(f, s));
    }
    n && (o = o || []).push("style", n);
    var f = o;
    (t.updateQueue = f) && (t.flags |= 4);
  }
};
gc = function(e, t, n, r) {
  n !== r && (t.flags |= 4);
};
function Rn(e, t) {
  if (!K) switch (e.tailMode) {
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
function he(e) {
  var t = e.alternate !== null && e.alternate.child === e.child, n = 0, r = 0;
  if (t) for (var l = e.child; l !== null; ) n |= l.lanes | l.childLanes, r |= l.subtreeFlags & 14680064, r |= l.flags & 14680064, l.return = e, l = l.sibling;
  else for (l = e.child; l !== null; ) n |= l.lanes | l.childLanes, r |= l.subtreeFlags, r |= l.flags, l.return = e, l = l.sibling;
  return e.subtreeFlags |= r, e.childLanes = n, t;
}
function Wf(e, t, n) {
  var r = t.pendingProps;
  switch (Li(t), t.tag) {
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
      return he(t), null;
    case 1:
      return _e(t.type) && al(), he(t), null;
    case 3:
      return r = t.stateNode, Sn(), W(Ee), W(ge), Ai(), r.pendingContext && (r.context = r.pendingContext, r.pendingContext = null), (e === null || e.child === null) && (Mr(t) ? t.flags |= 4 : e === null || e.memoizedState.isDehydrated && !(t.flags & 256) || (t.flags |= 1024, Qe !== null && (si(Qe), Qe = null))), ei(e, t), he(t), null;
    case 5:
      Ui(t);
      var l = At(sr.current);
      if (n = t.type, e !== null && t.stateNode != null) vc(e, t, n, r, l), e.ref !== t.ref && (t.flags |= 512, t.flags |= 2097152);
      else {
        if (!r) {
          if (t.stateNode === null) throw Error(j(166));
          return he(t), null;
        }
        if (e = At(tt.current), Mr(t)) {
          r = t.stateNode, n = t.type;
          var o = t.memoizedProps;
          switch (r[be] = t, r[ir] = o, e = (t.mode & 1) !== 0, n) {
            case "dialog":
              B("cancel", r), B("close", r);
              break;
            case "iframe":
            case "object":
            case "embed":
              B("load", r);
              break;
            case "video":
            case "audio":
              for (l = 0; l < An.length; l++) B(An[l], r);
              break;
            case "source":
              B("error", r);
              break;
            case "img":
            case "image":
            case "link":
              B(
                "error",
                r
              ), B("load", r);
              break;
            case "details":
              B("toggle", r);
              break;
            case "input":
              ma(r, o), B("invalid", r);
              break;
            case "select":
              r._wrapperState = { wasMultiple: !!o.multiple }, B("invalid", r);
              break;
            case "textarea":
              va(r, o), B("invalid", r);
          }
          _o(n, o), l = null;
          for (var i in o) if (o.hasOwnProperty(i)) {
            var a = o[i];
            i === "children" ? typeof a == "string" ? r.textContent !== a && (o.suppressHydrationWarning !== !0 && Lr(r.textContent, a, e), l = ["children", a]) : typeof a == "number" && r.textContent !== "" + a && (o.suppressHydrationWarning !== !0 && Lr(
              r.textContent,
              a,
              e
            ), l = ["children", "" + a]) : Zn.hasOwnProperty(i) && a != null && i === "onScroll" && B("scroll", r);
          }
          switch (n) {
            case "input":
              jr(r), ha(r, o, !0);
              break;
            case "textarea":
              jr(r), ga(r);
              break;
            case "select":
            case "option":
              break;
            default:
              typeof o.onClick == "function" && (r.onclick = il);
          }
          r = l, t.updateQueue = r, r !== null && (t.flags |= 4);
        } else {
          i = l.nodeType === 9 ? l : l.ownerDocument, e === "http://www.w3.org/1999/xhtml" && (e = Qs(n)), e === "http://www.w3.org/1999/xhtml" ? n === "script" ? (e = i.createElement("div"), e.innerHTML = "<script><\/script>", e = e.removeChild(e.firstChild)) : typeof r.is == "string" ? e = i.createElement(n, { is: r.is }) : (e = i.createElement(n), n === "select" && (i = e, r.multiple ? i.multiple = !0 : r.size && (i.size = r.size))) : e = i.createElementNS(e, n), e[be] = t, e[ir] = r, hc(e, t, !1, !1), t.stateNode = e;
          e: {
            switch (i = zo(n, r), n) {
              case "dialog":
                B("cancel", e), B("close", e), l = r;
                break;
              case "iframe":
              case "object":
              case "embed":
                B("load", e), l = r;
                break;
              case "video":
              case "audio":
                for (l = 0; l < An.length; l++) B(An[l], e);
                l = r;
                break;
              case "source":
                B("error", e), l = r;
                break;
              case "img":
              case "image":
              case "link":
                B(
                  "error",
                  e
                ), B("load", e), l = r;
                break;
              case "details":
                B("toggle", e), l = r;
                break;
              case "input":
                ma(e, r), l = So(e, r), B("invalid", e);
                break;
              case "option":
                l = r;
                break;
              case "select":
                e._wrapperState = { wasMultiple: !!r.multiple }, l = Z({}, r, { value: void 0 }), B("invalid", e);
                break;
              case "textarea":
                va(e, r), l = Co(e, r), B("invalid", e);
                break;
              default:
                l = r;
            }
            _o(n, l), a = l;
            for (o in a) if (a.hasOwnProperty(o)) {
              var s = a[o];
              o === "style" ? Ys(e, s) : o === "dangerouslySetInnerHTML" ? (s = s ? s.__html : void 0, s != null && Gs(e, s)) : o === "children" ? typeof s == "string" ? (n !== "textarea" || s !== "") && Jn(e, s) : typeof s == "number" && Jn(e, "" + s) : o !== "suppressContentEditableWarning" && o !== "suppressHydrationWarning" && o !== "autoFocus" && (Zn.hasOwnProperty(o) ? s != null && o === "onScroll" && B("scroll", e) : s != null && vi(e, o, s, i));
            }
            switch (n) {
              case "input":
                jr(e), ha(e, r, !1);
                break;
              case "textarea":
                jr(e), ga(e);
                break;
              case "option":
                r.value != null && e.setAttribute("value", "" + zt(r.value));
                break;
              case "select":
                e.multiple = !!r.multiple, o = r.value, o != null ? fn(e, !!r.multiple, o, !1) : r.defaultValue != null && fn(
                  e,
                  !!r.multiple,
                  r.defaultValue,
                  !0
                );
                break;
              default:
                typeof l.onClick == "function" && (e.onclick = il);
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
      return he(t), null;
    case 6:
      if (e && t.stateNode != null) gc(e, t, e.memoizedProps, r);
      else {
        if (typeof r != "string" && t.stateNode === null) throw Error(j(166));
        if (n = At(sr.current), At(tt.current), Mr(t)) {
          if (r = t.stateNode, n = t.memoizedProps, r[be] = t, (o = r.nodeValue !== n) && (e = Le, e !== null)) switch (e.tag) {
            case 3:
              Lr(r.nodeValue, n, (e.mode & 1) !== 0);
              break;
            case 5:
              e.memoizedProps.suppressHydrationWarning !== !0 && Lr(r.nodeValue, n, (e.mode & 1) !== 0);
          }
          o && (t.flags |= 4);
        } else r = (n.nodeType === 9 ? n : n.ownerDocument).createTextNode(r), r[be] = t, t.stateNode = r;
      }
      return he(t), null;
    case 13:
      if (W(Y), r = t.memoizedState, e === null || e.memoizedState !== null && e.memoizedState.dehydrated !== null) {
        if (K && Te !== null && t.mode & 1 && !(t.flags & 128)) Fu(), wn(), t.flags |= 98560, o = !1;
        else if (o = Mr(t), r !== null && r.dehydrated !== null) {
          if (e === null) {
            if (!o) throw Error(j(318));
            if (o = t.memoizedState, o = o !== null ? o.dehydrated : null, !o) throw Error(j(317));
            o[be] = t;
          } else wn(), !(t.flags & 128) && (t.memoizedState = null), t.flags |= 4;
          he(t), o = !1;
        } else Qe !== null && (si(Qe), Qe = null), o = !0;
        if (!o) return t.flags & 65536 ? t : null;
      }
      return t.flags & 128 ? (t.lanes = n, t) : (r = r !== null, r !== (e !== null && e.memoizedState !== null) && r && (t.child.flags |= 8192, t.mode & 1 && (e === null || Y.current & 1 ? le === 0 && (le = 3) : qi())), t.updateQueue !== null && (t.flags |= 4), he(t), null);
    case 4:
      return Sn(), ei(e, t), e === null && lr(t.stateNode.containerInfo), he(t), null;
    case 10:
      return Fi(t.type._context), he(t), null;
    case 17:
      return _e(t.type) && al(), he(t), null;
    case 19:
      if (W(Y), o = t.memoizedState, o === null) return he(t), null;
      if (r = (t.flags & 128) !== 0, i = o.rendering, i === null) if (r) Rn(o, !1);
      else {
        if (le !== 0 || e !== null && e.flags & 128) for (e = t.child; e !== null; ) {
          if (i = ml(e), i !== null) {
            for (t.flags |= 128, Rn(o, !1), r = i.updateQueue, r !== null && (t.updateQueue = r, t.flags |= 4), t.subtreeFlags = 0, r = n, n = t.child; n !== null; ) o = n, e = r, o.flags &= 14680066, i = o.alternate, i === null ? (o.childLanes = 0, o.lanes = e, o.child = null, o.subtreeFlags = 0, o.memoizedProps = null, o.memoizedState = null, o.updateQueue = null, o.dependencies = null, o.stateNode = null) : (o.childLanes = i.childLanes, o.lanes = i.lanes, o.child = i.child, o.subtreeFlags = 0, o.deletions = null, o.memoizedProps = i.memoizedProps, o.memoizedState = i.memoizedState, o.updateQueue = i.updateQueue, o.type = i.type, e = i.dependencies, o.dependencies = e === null ? null : { lanes: e.lanes, firstContext: e.firstContext }), n = n.sibling;
            return H(Y, Y.current & 1 | 2), t.child;
          }
          e = e.sibling;
        }
        o.tail !== null && ee() > Nn && (t.flags |= 128, r = !0, Rn(o, !1), t.lanes = 4194304);
      }
      else {
        if (!r) if (e = ml(i), e !== null) {
          if (t.flags |= 128, r = !0, n = e.updateQueue, n !== null && (t.updateQueue = n, t.flags |= 4), Rn(o, !0), o.tail === null && o.tailMode === "hidden" && !i.alternate && !K) return he(t), null;
        } else 2 * ee() - o.renderingStartTime > Nn && n !== 1073741824 && (t.flags |= 128, r = !0, Rn(o, !1), t.lanes = 4194304);
        o.isBackwards ? (i.sibling = t.child, t.child = i) : (n = o.last, n !== null ? n.sibling = i : t.child = i, o.last = i);
      }
      return o.tail !== null ? (t = o.tail, o.rendering = t, o.tail = t.sibling, o.renderingStartTime = ee(), t.sibling = null, n = Y.current, H(Y, r ? n & 1 | 2 : n & 1), t) : (he(t), null);
    case 22:
    case 23:
      return Ji(), r = t.memoizedState !== null, e !== null && e.memoizedState !== null !== r && (t.flags |= 8192), r && t.mode & 1 ? Pe & 1073741824 && (he(t), t.subtreeFlags & 6 && (t.flags |= 8192)) : he(t), null;
    case 24:
      return null;
    case 25:
      return null;
  }
  throw Error(j(156, t.tag));
}
function Qf(e, t) {
  switch (Li(t), t.tag) {
    case 1:
      return _e(t.type) && al(), e = t.flags, e & 65536 ? (t.flags = e & -65537 | 128, t) : null;
    case 3:
      return Sn(), W(Ee), W(ge), Ai(), e = t.flags, e & 65536 && !(e & 128) ? (t.flags = e & -65537 | 128, t) : null;
    case 5:
      return Ui(t), null;
    case 13:
      if (W(Y), e = t.memoizedState, e !== null && e.dehydrated !== null) {
        if (t.alternate === null) throw Error(j(340));
        wn();
      }
      return e = t.flags, e & 65536 ? (t.flags = e & -65537 | 128, t) : null;
    case 19:
      return W(Y), null;
    case 4:
      return Sn(), null;
    case 10:
      return Fi(t.type._context), null;
    case 22:
    case 23:
      return Ji(), null;
    case 24:
      return null;
    default:
      return null;
  }
}
var Fr = !1, ve = !1, Gf = typeof WeakSet == "function" ? WeakSet : Set, P = null;
function cn(e, t) {
  var n = e.ref;
  if (n !== null) if (typeof n == "function") try {
    n(null);
  } catch (r) {
    q(e, t, r);
  }
  else n.current = null;
}
function ti(e, t, n) {
  try {
    n();
  } catch (r) {
    q(e, t, r);
  }
}
var is = !1;
function Kf(e, t) {
  if (Io = rl, e = Su(), Pi(e)) {
    if ("selectionStart" in e) var n = { start: e.selectionStart, end: e.selectionEnd };
    else e: {
      n = (n = e.ownerDocument) && n.defaultView || window;
      var r = n.getSelection && n.getSelection();
      if (r && r.rangeCount !== 0) {
        n = r.anchorNode;
        var l = r.anchorOffset, o = r.focusNode;
        r = r.focusOffset;
        try {
          n.nodeType, o.nodeType;
        } catch {
          n = null;
          break e;
        }
        var i = 0, a = -1, s = -1, f = 0, h = 0, v = e, p = null;
        t: for (; ; ) {
          for (var g; v !== n || l !== 0 && v.nodeType !== 3 || (a = i + l), v !== o || r !== 0 && v.nodeType !== 3 || (s = i + r), v.nodeType === 3 && (i += v.nodeValue.length), (g = v.firstChild) !== null; )
            p = v, v = g;
          for (; ; ) {
            if (v === e) break t;
            if (p === n && ++f === l && (a = i), p === o && ++h === r && (s = i), (g = v.nextSibling) !== null) break;
            v = p, p = v.parentNode;
          }
          v = g;
        }
        n = a === -1 || s === -1 ? null : { start: a, end: s };
      } else n = null;
    }
    n = n || { start: 0, end: 0 };
  } else n = null;
  for (Uo = { focusedElem: e, selectionRange: n }, rl = !1, P = t; P !== null; ) if (t = P, e = t.child, (t.subtreeFlags & 1028) !== 0 && e !== null) e.return = t, P = e;
  else for (; P !== null; ) {
    t = P;
    try {
      var w = t.alternate;
      if (t.flags & 1024) switch (t.tag) {
        case 0:
        case 11:
        case 15:
          break;
        case 1:
          if (w !== null) {
            var k = w.memoizedProps, N = w.memoizedState, d = t.stateNode, c = d.getSnapshotBeforeUpdate(t.elementType === t.type ? k : Be(t.type, k), N);
            d.__reactInternalSnapshotBeforeUpdate = c;
          }
          break;
        case 3:
          var m = t.stateNode.containerInfo;
          m.nodeType === 1 ? m.textContent = "" : m.nodeType === 9 && m.documentElement && m.removeChild(m.documentElement);
          break;
        case 5:
        case 6:
        case 4:
        case 17:
          break;
        default:
          throw Error(j(163));
      }
    } catch (x) {
      q(t, t.return, x);
    }
    if (e = t.sibling, e !== null) {
      e.return = t.return, P = e;
      break;
    }
    P = t.return;
  }
  return w = is, is = !1, w;
}
function Kn(e, t, n) {
  var r = t.updateQueue;
  if (r = r !== null ? r.lastEffect : null, r !== null) {
    var l = r = r.next;
    do {
      if ((l.tag & e) === e) {
        var o = l.destroy;
        l.destroy = void 0, o !== void 0 && ti(t, n, o);
      }
      l = l.next;
    } while (l !== r);
  }
}
function Ll(e, t) {
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
function ni(e) {
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
function xc(e) {
  var t = e.alternate;
  t !== null && (e.alternate = null, xc(t)), e.child = null, e.deletions = null, e.sibling = null, e.tag === 5 && (t = e.stateNode, t !== null && (delete t[be], delete t[ir], delete t[Ho], delete t[Pf], delete t[Tf])), e.stateNode = null, e.return = null, e.dependencies = null, e.memoizedProps = null, e.memoizedState = null, e.pendingProps = null, e.stateNode = null, e.updateQueue = null;
}
function yc(e) {
  return e.tag === 5 || e.tag === 3 || e.tag === 4;
}
function as(e) {
  e: for (; ; ) {
    for (; e.sibling === null; ) {
      if (e.return === null || yc(e.return)) return null;
      e = e.return;
    }
    for (e.sibling.return = e.return, e = e.sibling; e.tag !== 5 && e.tag !== 6 && e.tag !== 18; ) {
      if (e.flags & 2 || e.child === null || e.tag === 4) continue e;
      e.child.return = e, e = e.child;
    }
    if (!(e.flags & 2)) return e.stateNode;
  }
}
function ri(e, t, n) {
  var r = e.tag;
  if (r === 5 || r === 6) e = e.stateNode, t ? n.nodeType === 8 ? n.parentNode.insertBefore(e, t) : n.insertBefore(e, t) : (n.nodeType === 8 ? (t = n.parentNode, t.insertBefore(e, n)) : (t = n, t.appendChild(e)), n = n._reactRootContainer, n != null || t.onclick !== null || (t.onclick = il));
  else if (r !== 4 && (e = e.child, e !== null)) for (ri(e, t, n), e = e.sibling; e !== null; ) ri(e, t, n), e = e.sibling;
}
function li(e, t, n) {
  var r = e.tag;
  if (r === 5 || r === 6) e = e.stateNode, t ? n.insertBefore(e, t) : n.appendChild(e);
  else if (r !== 4 && (e = e.child, e !== null)) for (li(e, t, n), e = e.sibling; e !== null; ) li(e, t, n), e = e.sibling;
}
var ue = null, We = !1;
function pt(e, t, n) {
  for (n = n.child; n !== null; ) wc(e, t, n), n = n.sibling;
}
function wc(e, t, n) {
  if (et && typeof et.onCommitFiberUnmount == "function") try {
    et.onCommitFiberUnmount(jl, n);
  } catch {
  }
  switch (n.tag) {
    case 5:
      ve || cn(n, t);
    case 6:
      var r = ue, l = We;
      ue = null, pt(e, t, n), ue = r, We = l, ue !== null && (We ? (e = ue, n = n.stateNode, e.nodeType === 8 ? e.parentNode.removeChild(n) : e.removeChild(n)) : ue.removeChild(n.stateNode));
      break;
    case 18:
      ue !== null && (We ? (e = ue, n = n.stateNode, e.nodeType === 8 ? no(e.parentNode, n) : e.nodeType === 1 && no(e, n), tr(e)) : no(ue, n.stateNode));
      break;
    case 4:
      r = ue, l = We, ue = n.stateNode.containerInfo, We = !0, pt(e, t, n), ue = r, We = l;
      break;
    case 0:
    case 11:
    case 14:
    case 15:
      if (!ve && (r = n.updateQueue, r !== null && (r = r.lastEffect, r !== null))) {
        l = r = r.next;
        do {
          var o = l, i = o.destroy;
          o = o.tag, i !== void 0 && (o & 2 || o & 4) && ti(n, t, i), l = l.next;
        } while (l !== r);
      }
      pt(e, t, n);
      break;
    case 1:
      if (!ve && (cn(n, t), r = n.stateNode, typeof r.componentWillUnmount == "function")) try {
        r.props = n.memoizedProps, r.state = n.memoizedState, r.componentWillUnmount();
      } catch (a) {
        q(n, t, a);
      }
      pt(e, t, n);
      break;
    case 21:
      pt(e, t, n);
      break;
    case 22:
      n.mode & 1 ? (ve = (r = ve) || n.memoizedState !== null, pt(e, t, n), ve = r) : pt(e, t, n);
      break;
    default:
      pt(e, t, n);
  }
}
function ss(e) {
  var t = e.updateQueue;
  if (t !== null) {
    e.updateQueue = null;
    var n = e.stateNode;
    n === null && (n = e.stateNode = new Gf()), t.forEach(function(r) {
      var l = np.bind(null, e, r);
      n.has(r) || (n.add(r), r.then(l, l));
    });
  }
}
function He(e, t) {
  var n = t.deletions;
  if (n !== null) for (var r = 0; r < n.length; r++) {
    var l = n[r];
    try {
      var o = e, i = t, a = i;
      e: for (; a !== null; ) {
        switch (a.tag) {
          case 5:
            ue = a.stateNode, We = !1;
            break e;
          case 3:
            ue = a.stateNode.containerInfo, We = !0;
            break e;
          case 4:
            ue = a.stateNode.containerInfo, We = !0;
            break e;
        }
        a = a.return;
      }
      if (ue === null) throw Error(j(160));
      wc(o, i, l), ue = null, We = !1;
      var s = l.alternate;
      s !== null && (s.return = null), l.return = null;
    } catch (f) {
      q(l, t, f);
    }
  }
  if (t.subtreeFlags & 12854) for (t = t.child; t !== null; ) kc(t, e), t = t.sibling;
}
function kc(e, t) {
  var n = e.alternate, r = e.flags;
  switch (e.tag) {
    case 0:
    case 11:
    case 14:
    case 15:
      if (He(t, e), Je(e), r & 4) {
        try {
          Kn(3, e, e.return), Ll(3, e);
        } catch (k) {
          q(e, e.return, k);
        }
        try {
          Kn(5, e, e.return);
        } catch (k) {
          q(e, e.return, k);
        }
      }
      break;
    case 1:
      He(t, e), Je(e), r & 512 && n !== null && cn(n, n.return);
      break;
    case 5:
      if (He(t, e), Je(e), r & 512 && n !== null && cn(n, n.return), e.flags & 32) {
        var l = e.stateNode;
        try {
          Jn(l, "");
        } catch (k) {
          q(e, e.return, k);
        }
      }
      if (r & 4 && (l = e.stateNode, l != null)) {
        var o = e.memoizedProps, i = n !== null ? n.memoizedProps : o, a = e.type, s = e.updateQueue;
        if (e.updateQueue = null, s !== null) try {
          a === "input" && o.type === "radio" && o.name != null && Bs(l, o), zo(a, i);
          var f = zo(a, o);
          for (i = 0; i < s.length; i += 2) {
            var h = s[i], v = s[i + 1];
            h === "style" ? Ys(l, v) : h === "dangerouslySetInnerHTML" ? Gs(l, v) : h === "children" ? Jn(l, v) : vi(l, h, v, f);
          }
          switch (a) {
            case "input":
              jo(l, o);
              break;
            case "textarea":
              Ws(l, o);
              break;
            case "select":
              var p = l._wrapperState.wasMultiple;
              l._wrapperState.wasMultiple = !!o.multiple;
              var g = o.value;
              g != null ? fn(l, !!o.multiple, g, !1) : p !== !!o.multiple && (o.defaultValue != null ? fn(
                l,
                !!o.multiple,
                o.defaultValue,
                !0
              ) : fn(l, !!o.multiple, o.multiple ? [] : "", !1));
          }
          l[ir] = o;
        } catch (k) {
          q(e, e.return, k);
        }
      }
      break;
    case 6:
      if (He(t, e), Je(e), r & 4) {
        if (e.stateNode === null) throw Error(j(162));
        l = e.stateNode, o = e.memoizedProps;
        try {
          l.nodeValue = o;
        } catch (k) {
          q(e, e.return, k);
        }
      }
      break;
    case 3:
      if (He(t, e), Je(e), r & 4 && n !== null && n.memoizedState.isDehydrated) try {
        tr(t.containerInfo);
      } catch (k) {
        q(e, e.return, k);
      }
      break;
    case 4:
      He(t, e), Je(e);
      break;
    case 13:
      He(t, e), Je(e), l = e.child, l.flags & 8192 && (o = l.memoizedState !== null, l.stateNode.isHidden = o, !o || l.alternate !== null && l.alternate.memoizedState !== null || (Xi = ee())), r & 4 && ss(e);
      break;
    case 22:
      if (h = n !== null && n.memoizedState !== null, e.mode & 1 ? (ve = (f = ve) || h, He(t, e), ve = f) : He(t, e), Je(e), r & 8192) {
        if (f = e.memoizedState !== null, (e.stateNode.isHidden = f) && !h && e.mode & 1) for (P = e, h = e.child; h !== null; ) {
          for (v = P = h; P !== null; ) {
            switch (p = P, g = p.child, p.tag) {
              case 0:
              case 11:
              case 14:
              case 15:
                Kn(4, p, p.return);
                break;
              case 1:
                cn(p, p.return);
                var w = p.stateNode;
                if (typeof w.componentWillUnmount == "function") {
                  r = p, n = p.return;
                  try {
                    t = r, w.props = t.memoizedProps, w.state = t.memoizedState, w.componentWillUnmount();
                  } catch (k) {
                    q(r, n, k);
                  }
                }
                break;
              case 5:
                cn(p, p.return);
                break;
              case 22:
                if (p.memoizedState !== null) {
                  cs(v);
                  continue;
                }
            }
            g !== null ? (g.return = p, P = g) : cs(v);
          }
          h = h.sibling;
        }
        e: for (h = null, v = e; ; ) {
          if (v.tag === 5) {
            if (h === null) {
              h = v;
              try {
                l = v.stateNode, f ? (o = l.style, typeof o.setProperty == "function" ? o.setProperty("display", "none", "important") : o.display = "none") : (a = v.stateNode, s = v.memoizedProps.style, i = s != null && s.hasOwnProperty("display") ? s.display : null, a.style.display = Ks("display", i));
              } catch (k) {
                q(e, e.return, k);
              }
            }
          } else if (v.tag === 6) {
            if (h === null) try {
              v.stateNode.nodeValue = f ? "" : v.memoizedProps;
            } catch (k) {
              q(e, e.return, k);
            }
          } else if ((v.tag !== 22 && v.tag !== 23 || v.memoizedState === null || v === e) && v.child !== null) {
            v.child.return = v, v = v.child;
            continue;
          }
          if (v === e) break e;
          for (; v.sibling === null; ) {
            if (v.return === null || v.return === e) break e;
            h === v && (h = null), v = v.return;
          }
          h === v && (h = null), v.sibling.return = v.return, v = v.sibling;
        }
      }
      break;
    case 19:
      He(t, e), Je(e), r & 4 && ss(e);
      break;
    case 21:
      break;
    default:
      He(
        t,
        e
      ), Je(e);
  }
}
function Je(e) {
  var t = e.flags;
  if (t & 2) {
    try {
      e: {
        for (var n = e.return; n !== null; ) {
          if (yc(n)) {
            var r = n;
            break e;
          }
          n = n.return;
        }
        throw Error(j(160));
      }
      switch (r.tag) {
        case 5:
          var l = r.stateNode;
          r.flags & 32 && (Jn(l, ""), r.flags &= -33);
          var o = as(e);
          li(e, o, l);
          break;
        case 3:
        case 4:
          var i = r.stateNode.containerInfo, a = as(e);
          ri(e, a, i);
          break;
        default:
          throw Error(j(161));
      }
    } catch (s) {
      q(e, e.return, s);
    }
    e.flags &= -3;
  }
  t & 4096 && (e.flags &= -4097);
}
function Yf(e, t, n) {
  P = e, Sc(e);
}
function Sc(e, t, n) {
  for (var r = (e.mode & 1) !== 0; P !== null; ) {
    var l = P, o = l.child;
    if (l.tag === 22 && r) {
      var i = l.memoizedState !== null || Fr;
      if (!i) {
        var a = l.alternate, s = a !== null && a.memoizedState !== null || ve;
        a = Fr;
        var f = ve;
        if (Fr = i, (ve = s) && !f) for (P = l; P !== null; ) i = P, s = i.child, i.tag === 22 && i.memoizedState !== null ? ds(l) : s !== null ? (s.return = i, P = s) : ds(l);
        for (; o !== null; ) P = o, Sc(o), o = o.sibling;
        P = l, Fr = a, ve = f;
      }
      us(e);
    } else l.subtreeFlags & 8772 && o !== null ? (o.return = l, P = o) : us(e);
  }
}
function us(e) {
  for (; P !== null; ) {
    var t = P;
    if (t.flags & 8772) {
      var n = t.alternate;
      try {
        if (t.flags & 8772) switch (t.tag) {
          case 0:
          case 11:
          case 15:
            ve || Ll(5, t);
            break;
          case 1:
            var r = t.stateNode;
            if (t.flags & 4 && !ve) if (n === null) r.componentDidMount();
            else {
              var l = t.elementType === t.type ? n.memoizedProps : Be(t.type, n.memoizedProps);
              r.componentDidUpdate(l, n.memoizedState, r.__reactInternalSnapshotBeforeUpdate);
            }
            var o = t.updateQueue;
            o !== null && Ka(t, o, r);
            break;
          case 3:
            var i = t.updateQueue;
            if (i !== null) {
              if (n = null, t.child !== null) switch (t.child.tag) {
                case 5:
                  n = t.child.stateNode;
                  break;
                case 1:
                  n = t.child.stateNode;
              }
              Ka(t, i, n);
            }
            break;
          case 5:
            var a = t.stateNode;
            if (n === null && t.flags & 4) {
              n = a;
              var s = t.memoizedProps;
              switch (t.type) {
                case "button":
                case "input":
                case "select":
                case "textarea":
                  s.autoFocus && n.focus();
                  break;
                case "img":
                  s.src && (n.src = s.src);
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
                var h = f.memoizedState;
                if (h !== null) {
                  var v = h.dehydrated;
                  v !== null && tr(v);
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
            throw Error(j(163));
        }
        ve || t.flags & 512 && ni(t);
      } catch (p) {
        q(t, t.return, p);
      }
    }
    if (t === e) {
      P = null;
      break;
    }
    if (n = t.sibling, n !== null) {
      n.return = t.return, P = n;
      break;
    }
    P = t.return;
  }
}
function cs(e) {
  for (; P !== null; ) {
    var t = P;
    if (t === e) {
      P = null;
      break;
    }
    var n = t.sibling;
    if (n !== null) {
      n.return = t.return, P = n;
      break;
    }
    P = t.return;
  }
}
function ds(e) {
  for (; P !== null; ) {
    var t = P;
    try {
      switch (t.tag) {
        case 0:
        case 11:
        case 15:
          var n = t.return;
          try {
            Ll(4, t);
          } catch (s) {
            q(t, n, s);
          }
          break;
        case 1:
          var r = t.stateNode;
          if (typeof r.componentDidMount == "function") {
            var l = t.return;
            try {
              r.componentDidMount();
            } catch (s) {
              q(t, l, s);
            }
          }
          var o = t.return;
          try {
            ni(t);
          } catch (s) {
            q(t, o, s);
          }
          break;
        case 5:
          var i = t.return;
          try {
            ni(t);
          } catch (s) {
            q(t, i, s);
          }
      }
    } catch (s) {
      q(t, t.return, s);
    }
    if (t === e) {
      P = null;
      break;
    }
    var a = t.sibling;
    if (a !== null) {
      a.return = t.return, P = a;
      break;
    }
    P = t.return;
  }
}
var Xf = Math.ceil, gl = dt.ReactCurrentDispatcher, Ki = dt.ReactCurrentOwner, Ue = dt.ReactCurrentBatchConfig, U = 0, ie = null, te = null, ce = 0, Pe = 0, dn = Lt(0), le = 0, fr = null, Gt = 0, Ml = 0, Yi = 0, Yn = null, Ne = null, Xi = 0, Nn = 1 / 0, nt = null, xl = !1, oi = null, Ct = null, $r = !1, yt = null, yl = 0, Xn = 0, ii = null, Yr = -1, Xr = 0;
function we() {
  return U & 6 ? ee() : Yr !== -1 ? Yr : Yr = ee();
}
function Et(e) {
  return e.mode & 1 ? U & 2 && ce !== 0 ? ce & -ce : Mf.transition !== null ? (Xr === 0 && (Xr = iu()), Xr) : (e = V, e !== 0 || (e = window.event, e = e === void 0 ? 16 : pu(e.type)), e) : 1;
}
function Ke(e, t, n, r) {
  if (50 < Xn) throw Xn = 0, ii = null, Error(j(185));
  mr(e, n, r), (!(U & 2) || e !== ie) && (e === ie && (!(U & 2) && (Ml |= n), le === 4 && gt(e, ce)), ze(e, r), n === 1 && U === 0 && !(t.mode & 1) && (Nn = ee() + 500, zl && Mt()));
}
function ze(e, t) {
  var n = e.callbackNode;
  Ld(e, t);
  var r = nl(e, e === ie ? ce : 0);
  if (r === 0) n !== null && wa(n), e.callbackNode = null, e.callbackPriority = 0;
  else if (t = r & -r, e.callbackPriority !== t) {
    if (n != null && wa(n), t === 1) e.tag === 0 ? Lf(fs.bind(null, e)) : Mu(fs.bind(null, e)), _f(function() {
      !(U & 6) && Mt();
    }), n = null;
    else {
      switch (au(r)) {
        case 1:
          n = ki;
          break;
        case 4:
          n = lu;
          break;
        case 16:
          n = tl;
          break;
        case 536870912:
          n = ou;
          break;
        default:
          n = tl;
      }
      n = Tc(n, jc.bind(null, e));
    }
    e.callbackPriority = t, e.callbackNode = n;
  }
}
function jc(e, t) {
  if (Yr = -1, Xr = 0, U & 6) throw Error(j(327));
  var n = e.callbackNode;
  if (gn() && e.callbackNode !== n) return null;
  var r = nl(e, e === ie ? ce : 0);
  if (r === 0) return null;
  if (r & 30 || r & e.expiredLanes || t) t = wl(e, r);
  else {
    t = r;
    var l = U;
    U |= 2;
    var o = Cc();
    (ie !== e || ce !== t) && (nt = null, Nn = ee() + 500, Vt(e, t));
    do
      try {
        qf();
        break;
      } catch (a) {
        Nc(e, a);
      }
    while (!0);
    Ri(), gl.current = o, U = l, te !== null ? t = 0 : (ie = null, ce = 0, t = le);
  }
  if (t !== 0) {
    if (t === 2 && (l = Do(e), l !== 0 && (r = l, t = ai(e, l))), t === 1) throw n = fr, Vt(e, 0), gt(e, r), ze(e, ee()), n;
    if (t === 6) gt(e, r);
    else {
      if (l = e.current.alternate, !(r & 30) && !Zf(l) && (t = wl(e, r), t === 2 && (o = Do(e), o !== 0 && (r = o, t = ai(e, o))), t === 1)) throw n = fr, Vt(e, 0), gt(e, r), ze(e, ee()), n;
      switch (e.finishedWork = l, e.finishedLanes = r, t) {
        case 0:
        case 1:
          throw Error(j(345));
        case 2:
          Ot(e, Ne, nt);
          break;
        case 3:
          if (gt(e, r), (r & 130023424) === r && (t = Xi + 500 - ee(), 10 < t)) {
            if (nl(e, 0) !== 0) break;
            if (l = e.suspendedLanes, (l & r) !== r) {
              we(), e.pingedLanes |= e.suspendedLanes & l;
              break;
            }
            e.timeoutHandle = Vo(Ot.bind(null, e, Ne, nt), t);
            break;
          }
          Ot(e, Ne, nt);
          break;
        case 4:
          if (gt(e, r), (r & 4194240) === r) break;
          for (t = e.eventTimes, l = -1; 0 < r; ) {
            var i = 31 - Ge(r);
            o = 1 << i, i = t[i], i > l && (l = i), r &= ~o;
          }
          if (r = l, r = ee() - r, r = (120 > r ? 120 : 480 > r ? 480 : 1080 > r ? 1080 : 1920 > r ? 1920 : 3e3 > r ? 3e3 : 4320 > r ? 4320 : 1960 * Xf(r / 1960)) - r, 10 < r) {
            e.timeoutHandle = Vo(Ot.bind(null, e, Ne, nt), r);
            break;
          }
          Ot(e, Ne, nt);
          break;
        case 5:
          Ot(e, Ne, nt);
          break;
        default:
          throw Error(j(329));
      }
    }
  }
  return ze(e, ee()), e.callbackNode === n ? jc.bind(null, e) : null;
}
function ai(e, t) {
  var n = Yn;
  return e.current.memoizedState.isDehydrated && (Vt(e, t).flags |= 256), e = wl(e, t), e !== 2 && (t = Ne, Ne = n, t !== null && si(t)), e;
}
function si(e) {
  Ne === null ? Ne = e : Ne.push.apply(Ne, e);
}
function Zf(e) {
  for (var t = e; ; ) {
    if (t.flags & 16384) {
      var n = t.updateQueue;
      if (n !== null && (n = n.stores, n !== null)) for (var r = 0; r < n.length; r++) {
        var l = n[r], o = l.getSnapshot;
        l = l.value;
        try {
          if (!Ye(o(), l)) return !1;
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
function gt(e, t) {
  for (t &= ~Yi, t &= ~Ml, e.suspendedLanes |= t, e.pingedLanes &= ~t, e = e.expirationTimes; 0 < t; ) {
    var n = 31 - Ge(t), r = 1 << n;
    e[n] = -1, t &= ~r;
  }
}
function fs(e) {
  if (U & 6) throw Error(j(327));
  gn();
  var t = nl(e, 0);
  if (!(t & 1)) return ze(e, ee()), null;
  var n = wl(e, t);
  if (e.tag !== 0 && n === 2) {
    var r = Do(e);
    r !== 0 && (t = r, n = ai(e, r));
  }
  if (n === 1) throw n = fr, Vt(e, 0), gt(e, t), ze(e, ee()), n;
  if (n === 6) throw Error(j(345));
  return e.finishedWork = e.current.alternate, e.finishedLanes = t, Ot(e, Ne, nt), ze(e, ee()), null;
}
function Zi(e, t) {
  var n = U;
  U |= 1;
  try {
    return e(t);
  } finally {
    U = n, U === 0 && (Nn = ee() + 500, zl && Mt());
  }
}
function Kt(e) {
  yt !== null && yt.tag === 0 && !(U & 6) && gn();
  var t = U;
  U |= 1;
  var n = Ue.transition, r = V;
  try {
    if (Ue.transition = null, V = 1, e) return e();
  } finally {
    V = r, Ue.transition = n, U = t, !(U & 6) && Mt();
  }
}
function Ji() {
  Pe = dn.current, W(dn);
}
function Vt(e, t) {
  e.finishedWork = null, e.finishedLanes = 0;
  var n = e.timeoutHandle;
  if (n !== -1 && (e.timeoutHandle = -1, Ef(n)), te !== null) for (n = te.return; n !== null; ) {
    var r = n;
    switch (Li(r), r.tag) {
      case 1:
        r = r.type.childContextTypes, r != null && al();
        break;
      case 3:
        Sn(), W(Ee), W(ge), Ai();
        break;
      case 5:
        Ui(r);
        break;
      case 4:
        Sn();
        break;
      case 13:
        W(Y);
        break;
      case 19:
        W(Y);
        break;
      case 10:
        Fi(r.type._context);
        break;
      case 22:
      case 23:
        Ji();
    }
    n = n.return;
  }
  if (ie = e, te = e = _t(e.current, null), ce = Pe = t, le = 0, fr = null, Yi = Ml = Gt = 0, Ne = Yn = null, Ut !== null) {
    for (t = 0; t < Ut.length; t++) if (n = Ut[t], r = n.interleaved, r !== null) {
      n.interleaved = null;
      var l = r.next, o = n.pending;
      if (o !== null) {
        var i = o.next;
        o.next = l, r.next = i;
      }
      n.pending = r;
    }
    Ut = null;
  }
  return e;
}
function Nc(e, t) {
  do {
    var n = te;
    try {
      if (Ri(), Qr.current = vl, hl) {
        for (var r = X.memoizedState; r !== null; ) {
          var l = r.queue;
          l !== null && (l.pending = null), r = r.next;
        }
        hl = !1;
      }
      if (Qt = 0, oe = re = X = null, Gn = !1, ur = 0, Ki.current = null, n === null || n.return === null) {
        le = 1, fr = t, te = null;
        break;
      }
      e: {
        var o = e, i = n.return, a = n, s = t;
        if (t = ce, a.flags |= 32768, s !== null && typeof s == "object" && typeof s.then == "function") {
          var f = s, h = a, v = h.tag;
          if (!(h.mode & 1) && (v === 0 || v === 11 || v === 15)) {
            var p = h.alternate;
            p ? (h.updateQueue = p.updateQueue, h.memoizedState = p.memoizedState, h.lanes = p.lanes) : (h.updateQueue = null, h.memoizedState = null);
          }
          var g = ba(i);
          if (g !== null) {
            g.flags &= -257, es(g, i, a, o, t), g.mode & 1 && qa(o, f, t), t = g, s = f;
            var w = t.updateQueue;
            if (w === null) {
              var k = /* @__PURE__ */ new Set();
              k.add(s), t.updateQueue = k;
            } else w.add(s);
            break e;
          } else {
            if (!(t & 1)) {
              qa(o, f, t), qi();
              break e;
            }
            s = Error(j(426));
          }
        } else if (K && a.mode & 1) {
          var N = ba(i);
          if (N !== null) {
            !(N.flags & 65536) && (N.flags |= 256), es(N, i, a, o, t), Mi(jn(s, a));
            break e;
          }
        }
        o = s = jn(s, a), le !== 4 && (le = 2), Yn === null ? Yn = [o] : Yn.push(o), o = i;
        do {
          switch (o.tag) {
            case 3:
              o.flags |= 65536, t &= -t, o.lanes |= t;
              var d = ac(o, s, t);
              Ga(o, d);
              break e;
            case 1:
              a = s;
              var c = o.type, m = o.stateNode;
              if (!(o.flags & 128) && (typeof c.getDerivedStateFromError == "function" || m !== null && typeof m.componentDidCatch == "function" && (Ct === null || !Ct.has(m)))) {
                o.flags |= 65536, t &= -t, o.lanes |= t;
                var x = sc(o, a, t);
                Ga(o, x);
                break e;
              }
          }
          o = o.return;
        } while (o !== null);
      }
      _c(n);
    } catch (C) {
      t = C, te === n && n !== null && (te = n = n.return);
      continue;
    }
    break;
  } while (!0);
}
function Cc() {
  var e = gl.current;
  return gl.current = vl, e === null ? vl : e;
}
function qi() {
  (le === 0 || le === 3 || le === 2) && (le = 4), ie === null || !(Gt & 268435455) && !(Ml & 268435455) || gt(ie, ce);
}
function wl(e, t) {
  var n = U;
  U |= 2;
  var r = Cc();
  (ie !== e || ce !== t) && (nt = null, Vt(e, t));
  do
    try {
      Jf();
      break;
    } catch (l) {
      Nc(e, l);
    }
  while (!0);
  if (Ri(), U = n, gl.current = r, te !== null) throw Error(j(261));
  return ie = null, ce = 0, le;
}
function Jf() {
  for (; te !== null; ) Ec(te);
}
function qf() {
  for (; te !== null && !Sd(); ) Ec(te);
}
function Ec(e) {
  var t = Pc(e.alternate, e, Pe);
  e.memoizedProps = e.pendingProps, t === null ? _c(e) : te = t, Ki.current = null;
}
function _c(e) {
  var t = e;
  do {
    var n = t.alternate;
    if (e = t.return, t.flags & 32768) {
      if (n = Qf(n, t), n !== null) {
        n.flags &= 32767, te = n;
        return;
      }
      if (e !== null) e.flags |= 32768, e.subtreeFlags = 0, e.deletions = null;
      else {
        le = 6, te = null;
        return;
      }
    } else if (n = Wf(n, t, Pe), n !== null) {
      te = n;
      return;
    }
    if (t = t.sibling, t !== null) {
      te = t;
      return;
    }
    te = t = e;
  } while (t !== null);
  le === 0 && (le = 5);
}
function Ot(e, t, n) {
  var r = V, l = Ue.transition;
  try {
    Ue.transition = null, V = 1, bf(e, t, n, r);
  } finally {
    Ue.transition = l, V = r;
  }
  return null;
}
function bf(e, t, n, r) {
  do
    gn();
  while (yt !== null);
  if (U & 6) throw Error(j(327));
  n = e.finishedWork;
  var l = e.finishedLanes;
  if (n === null) return null;
  if (e.finishedWork = null, e.finishedLanes = 0, n === e.current) throw Error(j(177));
  e.callbackNode = null, e.callbackPriority = 0;
  var o = n.lanes | n.childLanes;
  if (Md(e, o), e === ie && (te = ie = null, ce = 0), !(n.subtreeFlags & 2064) && !(n.flags & 2064) || $r || ($r = !0, Tc(tl, function() {
    return gn(), null;
  })), o = (n.flags & 15990) !== 0, n.subtreeFlags & 15990 || o) {
    o = Ue.transition, Ue.transition = null;
    var i = V;
    V = 1;
    var a = U;
    U |= 4, Ki.current = null, Kf(e, n), kc(n, e), yf(Uo), rl = !!Io, Uo = Io = null, e.current = n, Yf(n), jd(), U = a, V = i, Ue.transition = o;
  } else e.current = n;
  if ($r && ($r = !1, yt = e, yl = l), o = e.pendingLanes, o === 0 && (Ct = null), Ed(n.stateNode), ze(e, ee()), t !== null) for (r = e.onRecoverableError, n = 0; n < t.length; n++) l = t[n], r(l.value, { componentStack: l.stack, digest: l.digest });
  if (xl) throw xl = !1, e = oi, oi = null, e;
  return yl & 1 && e.tag !== 0 && gn(), o = e.pendingLanes, o & 1 ? e === ii ? Xn++ : (Xn = 0, ii = e) : Xn = 0, Mt(), null;
}
function gn() {
  if (yt !== null) {
    var e = au(yl), t = Ue.transition, n = V;
    try {
      if (Ue.transition = null, V = 16 > e ? 16 : e, yt === null) var r = !1;
      else {
        if (e = yt, yt = null, yl = 0, U & 6) throw Error(j(331));
        var l = U;
        for (U |= 4, P = e.current; P !== null; ) {
          var o = P, i = o.child;
          if (P.flags & 16) {
            var a = o.deletions;
            if (a !== null) {
              for (var s = 0; s < a.length; s++) {
                var f = a[s];
                for (P = f; P !== null; ) {
                  var h = P;
                  switch (h.tag) {
                    case 0:
                    case 11:
                    case 15:
                      Kn(8, h, o);
                  }
                  var v = h.child;
                  if (v !== null) v.return = h, P = v;
                  else for (; P !== null; ) {
                    h = P;
                    var p = h.sibling, g = h.return;
                    if (xc(h), h === f) {
                      P = null;
                      break;
                    }
                    if (p !== null) {
                      p.return = g, P = p;
                      break;
                    }
                    P = g;
                  }
                }
              }
              var w = o.alternate;
              if (w !== null) {
                var k = w.child;
                if (k !== null) {
                  w.child = null;
                  do {
                    var N = k.sibling;
                    k.sibling = null, k = N;
                  } while (k !== null);
                }
              }
              P = o;
            }
          }
          if (o.subtreeFlags & 2064 && i !== null) i.return = o, P = i;
          else e: for (; P !== null; ) {
            if (o = P, o.flags & 2048) switch (o.tag) {
              case 0:
              case 11:
              case 15:
                Kn(9, o, o.return);
            }
            var d = o.sibling;
            if (d !== null) {
              d.return = o.return, P = d;
              break e;
            }
            P = o.return;
          }
        }
        var c = e.current;
        for (P = c; P !== null; ) {
          i = P;
          var m = i.child;
          if (i.subtreeFlags & 2064 && m !== null) m.return = i, P = m;
          else e: for (i = c; P !== null; ) {
            if (a = P, a.flags & 2048) try {
              switch (a.tag) {
                case 0:
                case 11:
                case 15:
                  Ll(9, a);
              }
            } catch (C) {
              q(a, a.return, C);
            }
            if (a === i) {
              P = null;
              break e;
            }
            var x = a.sibling;
            if (x !== null) {
              x.return = a.return, P = x;
              break e;
            }
            P = a.return;
          }
        }
        if (U = l, Mt(), et && typeof et.onPostCommitFiberRoot == "function") try {
          et.onPostCommitFiberRoot(jl, e);
        } catch {
        }
        r = !0;
      }
      return r;
    } finally {
      V = n, Ue.transition = t;
    }
  }
  return !1;
}
function ps(e, t, n) {
  t = jn(n, t), t = ac(e, t, 1), e = Nt(e, t, 1), t = we(), e !== null && (mr(e, 1, t), ze(e, t));
}
function q(e, t, n) {
  if (e.tag === 3) ps(e, e, n);
  else for (; t !== null; ) {
    if (t.tag === 3) {
      ps(t, e, n);
      break;
    } else if (t.tag === 1) {
      var r = t.stateNode;
      if (typeof t.type.getDerivedStateFromError == "function" || typeof r.componentDidCatch == "function" && (Ct === null || !Ct.has(r))) {
        e = jn(n, e), e = sc(t, e, 1), t = Nt(t, e, 1), e = we(), t !== null && (mr(t, 1, e), ze(t, e));
        break;
      }
    }
    t = t.return;
  }
}
function ep(e, t, n) {
  var r = e.pingCache;
  r !== null && r.delete(t), t = we(), e.pingedLanes |= e.suspendedLanes & n, ie === e && (ce & n) === n && (le === 4 || le === 3 && (ce & 130023424) === ce && 500 > ee() - Xi ? Vt(e, 0) : Yi |= n), ze(e, t);
}
function zc(e, t) {
  t === 0 && (e.mode & 1 ? (t = Er, Er <<= 1, !(Er & 130023424) && (Er = 4194304)) : t = 1);
  var n = we();
  e = ut(e, t), e !== null && (mr(e, t, n), ze(e, n));
}
function tp(e) {
  var t = e.memoizedState, n = 0;
  t !== null && (n = t.retryLane), zc(e, n);
}
function np(e, t) {
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
      throw Error(j(314));
  }
  r !== null && r.delete(t), zc(e, n);
}
var Pc;
Pc = function(e, t, n) {
  if (e !== null) if (e.memoizedProps !== t.pendingProps || Ee.current) Ce = !0;
  else {
    if (!(e.lanes & n) && !(t.flags & 128)) return Ce = !1, Bf(e, t, n);
    Ce = !!(e.flags & 131072);
  }
  else Ce = !1, K && t.flags & 1048576 && Du(t, cl, t.index);
  switch (t.lanes = 0, t.tag) {
    case 2:
      var r = t.type;
      Kr(e, t), e = t.pendingProps;
      var l = yn(t, ge.current);
      vn(t, n), l = Hi(null, t, r, e, l, n);
      var o = Bi();
      return t.flags |= 1, typeof l == "object" && l !== null && typeof l.render == "function" && l.$$typeof === void 0 ? (t.tag = 1, t.memoizedState = null, t.updateQueue = null, _e(r) ? (o = !0, sl(t)) : o = !1, t.memoizedState = l.state !== null && l.state !== void 0 ? l.state : null, Oi(t), l.updater = Tl, t.stateNode = l, l._reactInternals = t, Yo(t, r, e, n), t = Jo(null, t, r, !0, o, n)) : (t.tag = 0, K && o && Ti(t), ye(null, t, l, n), t = t.child), t;
    case 16:
      r = t.elementType;
      e: {
        switch (Kr(e, t), e = t.pendingProps, l = r._init, r = l(r._payload), t.type = r, l = t.tag = lp(r), e = Be(r, e), l) {
          case 0:
            t = Zo(null, t, r, e, n);
            break e;
          case 1:
            t = rs(null, t, r, e, n);
            break e;
          case 11:
            t = ts(null, t, r, e, n);
            break e;
          case 14:
            t = ns(null, t, r, Be(r.type, e), n);
            break e;
        }
        throw Error(j(
          306,
          r,
          ""
        ));
      }
      return t;
    case 0:
      return r = t.type, l = t.pendingProps, l = t.elementType === r ? l : Be(r, l), Zo(e, t, r, l, n);
    case 1:
      return r = t.type, l = t.pendingProps, l = t.elementType === r ? l : Be(r, l), rs(e, t, r, l, n);
    case 3:
      e: {
        if (fc(t), e === null) throw Error(j(387));
        r = t.pendingProps, o = t.memoizedState, l = o.element, Uu(e, t), pl(t, r, null, n);
        var i = t.memoizedState;
        if (r = i.element, o.isDehydrated) if (o = { element: r, isDehydrated: !1, cache: i.cache, pendingSuspenseBoundaries: i.pendingSuspenseBoundaries, transitions: i.transitions }, t.updateQueue.baseState = o, t.memoizedState = o, t.flags & 256) {
          l = jn(Error(j(423)), t), t = ls(e, t, r, n, l);
          break e;
        } else if (r !== l) {
          l = jn(Error(j(424)), t), t = ls(e, t, r, n, l);
          break e;
        } else for (Te = jt(t.stateNode.containerInfo.firstChild), Le = t, K = !0, Qe = null, n = Ou(t, null, r, n), t.child = n; n; ) n.flags = n.flags & -3 | 4096, n = n.sibling;
        else {
          if (wn(), r === l) {
            t = ct(e, t, n);
            break e;
          }
          ye(e, t, r, n);
        }
        t = t.child;
      }
      return t;
    case 5:
      return Au(t), e === null && Qo(t), r = t.type, l = t.pendingProps, o = e !== null ? e.memoizedProps : null, i = l.children, Ao(r, l) ? i = null : o !== null && Ao(r, o) && (t.flags |= 32), dc(e, t), ye(e, t, i, n), t.child;
    case 6:
      return e === null && Qo(t), null;
    case 13:
      return pc(e, t, n);
    case 4:
      return Ii(t, t.stateNode.containerInfo), r = t.pendingProps, e === null ? t.child = kn(t, null, r, n) : ye(e, t, r, n), t.child;
    case 11:
      return r = t.type, l = t.pendingProps, l = t.elementType === r ? l : Be(r, l), ts(e, t, r, l, n);
    case 7:
      return ye(e, t, t.pendingProps, n), t.child;
    case 8:
      return ye(e, t, t.pendingProps.children, n), t.child;
    case 12:
      return ye(e, t, t.pendingProps.children, n), t.child;
    case 10:
      e: {
        if (r = t.type._context, l = t.pendingProps, o = t.memoizedProps, i = l.value, H(dl, r._currentValue), r._currentValue = i, o !== null) if (Ye(o.value, i)) {
          if (o.children === l.children && !Ee.current) {
            t = ct(e, t, n);
            break e;
          }
        } else for (o = t.child, o !== null && (o.return = t); o !== null; ) {
          var a = o.dependencies;
          if (a !== null) {
            i = o.child;
            for (var s = a.firstContext; s !== null; ) {
              if (s.context === r) {
                if (o.tag === 1) {
                  s = it(-1, n & -n), s.tag = 2;
                  var f = o.updateQueue;
                  if (f !== null) {
                    f = f.shared;
                    var h = f.pending;
                    h === null ? s.next = s : (s.next = h.next, h.next = s), f.pending = s;
                  }
                }
                o.lanes |= n, s = o.alternate, s !== null && (s.lanes |= n), Go(
                  o.return,
                  n,
                  t
                ), a.lanes |= n;
                break;
              }
              s = s.next;
            }
          } else if (o.tag === 10) i = o.type === t.type ? null : o.child;
          else if (o.tag === 18) {
            if (i = o.return, i === null) throw Error(j(341));
            i.lanes |= n, a = i.alternate, a !== null && (a.lanes |= n), Go(i, n, t), i = o.sibling;
          } else i = o.child;
          if (i !== null) i.return = o;
          else for (i = o; i !== null; ) {
            if (i === t) {
              i = null;
              break;
            }
            if (o = i.sibling, o !== null) {
              o.return = i.return, i = o;
              break;
            }
            i = i.return;
          }
          o = i;
        }
        ye(e, t, l.children, n), t = t.child;
      }
      return t;
    case 9:
      return l = t.type, r = t.pendingProps.children, vn(t, n), l = Ae(l), r = r(l), t.flags |= 1, ye(e, t, r, n), t.child;
    case 14:
      return r = t.type, l = Be(r, t.pendingProps), l = Be(r.type, l), ns(e, t, r, l, n);
    case 15:
      return uc(e, t, t.type, t.pendingProps, n);
    case 17:
      return r = t.type, l = t.pendingProps, l = t.elementType === r ? l : Be(r, l), Kr(e, t), t.tag = 1, _e(r) ? (e = !0, sl(t)) : e = !1, vn(t, n), ic(t, r, l), Yo(t, r, l, n), Jo(null, t, r, !0, e, n);
    case 19:
      return mc(e, t, n);
    case 22:
      return cc(e, t, n);
  }
  throw Error(j(156, t.tag));
};
function Tc(e, t) {
  return ru(e, t);
}
function rp(e, t, n, r) {
  this.tag = e, this.key = n, this.sibling = this.child = this.return = this.stateNode = this.type = this.elementType = null, this.index = 0, this.ref = null, this.pendingProps = t, this.dependencies = this.memoizedState = this.updateQueue = this.memoizedProps = null, this.mode = r, this.subtreeFlags = this.flags = 0, this.deletions = null, this.childLanes = this.lanes = 0, this.alternate = null;
}
function Ie(e, t, n, r) {
  return new rp(e, t, n, r);
}
function bi(e) {
  return e = e.prototype, !(!e || !e.isReactComponent);
}
function lp(e) {
  if (typeof e == "function") return bi(e) ? 1 : 0;
  if (e != null) {
    if (e = e.$$typeof, e === xi) return 11;
    if (e === yi) return 14;
  }
  return 2;
}
function _t(e, t) {
  var n = e.alternate;
  return n === null ? (n = Ie(e.tag, t, e.key, e.mode), n.elementType = e.elementType, n.type = e.type, n.stateNode = e.stateNode, n.alternate = e, e.alternate = n) : (n.pendingProps = t, n.type = e.type, n.flags = 0, n.subtreeFlags = 0, n.deletions = null), n.flags = e.flags & 14680064, n.childLanes = e.childLanes, n.lanes = e.lanes, n.child = e.child, n.memoizedProps = e.memoizedProps, n.memoizedState = e.memoizedState, n.updateQueue = e.updateQueue, t = e.dependencies, n.dependencies = t === null ? null : { lanes: t.lanes, firstContext: t.firstContext }, n.sibling = e.sibling, n.index = e.index, n.ref = e.ref, n;
}
function Zr(e, t, n, r, l, o) {
  var i = 2;
  if (r = e, typeof e == "function") bi(e) && (i = 1);
  else if (typeof e == "string") i = 5;
  else e: switch (e) {
    case en:
      return Ht(n.children, l, o, t);
    case gi:
      i = 8, l |= 8;
      break;
    case xo:
      return e = Ie(12, n, t, l | 2), e.elementType = xo, e.lanes = o, e;
    case yo:
      return e = Ie(13, n, t, l), e.elementType = yo, e.lanes = o, e;
    case wo:
      return e = Ie(19, n, t, l), e.elementType = wo, e.lanes = o, e;
    case As:
      return Dl(n, l, o, t);
    default:
      if (typeof e == "object" && e !== null) switch (e.$$typeof) {
        case Is:
          i = 10;
          break e;
        case Us:
          i = 9;
          break e;
        case xi:
          i = 11;
          break e;
        case yi:
          i = 14;
          break e;
        case mt:
          i = 16, r = null;
          break e;
      }
      throw Error(j(130, e == null ? e : typeof e, ""));
  }
  return t = Ie(i, n, t, l), t.elementType = e, t.type = r, t.lanes = o, t;
}
function Ht(e, t, n, r) {
  return e = Ie(7, e, r, t), e.lanes = n, e;
}
function Dl(e, t, n, r) {
  return e = Ie(22, e, r, t), e.elementType = As, e.lanes = n, e.stateNode = { isHidden: !1 }, e;
}
function co(e, t, n) {
  return e = Ie(6, e, null, t), e.lanes = n, e;
}
function fo(e, t, n) {
  return t = Ie(4, e.children !== null ? e.children : [], e.key, t), t.lanes = n, t.stateNode = { containerInfo: e.containerInfo, pendingChildren: null, implementation: e.implementation }, t;
}
function op(e, t, n, r, l) {
  this.tag = t, this.containerInfo = e, this.finishedWork = this.pingCache = this.current = this.pendingChildren = null, this.timeoutHandle = -1, this.callbackNode = this.pendingContext = this.context = null, this.callbackPriority = 0, this.eventTimes = Ql(0), this.expirationTimes = Ql(-1), this.entangledLanes = this.finishedLanes = this.mutableReadLanes = this.expiredLanes = this.pingedLanes = this.suspendedLanes = this.pendingLanes = 0, this.entanglements = Ql(0), this.identifierPrefix = r, this.onRecoverableError = l, this.mutableSourceEagerHydrationData = null;
}
function ea(e, t, n, r, l, o, i, a, s) {
  return e = new op(e, t, n, a, s), t === 1 ? (t = 1, o === !0 && (t |= 8)) : t = 0, o = Ie(3, null, null, t), e.current = o, o.stateNode = e, o.memoizedState = { element: r, isDehydrated: n, cache: null, transitions: null, pendingSuspenseBoundaries: null }, Oi(o), e;
}
function ip(e, t, n) {
  var r = 3 < arguments.length && arguments[3] !== void 0 ? arguments[3] : null;
  return { $$typeof: bt, key: r == null ? null : "" + r, children: e, containerInfo: t, implementation: n };
}
function Lc(e) {
  if (!e) return Pt;
  e = e._reactInternals;
  e: {
    if (Xt(e) !== e || e.tag !== 1) throw Error(j(170));
    var t = e;
    do {
      switch (t.tag) {
        case 3:
          t = t.stateNode.context;
          break e;
        case 1:
          if (_e(t.type)) {
            t = t.stateNode.__reactInternalMemoizedMergedChildContext;
            break e;
          }
      }
      t = t.return;
    } while (t !== null);
    throw Error(j(171));
  }
  if (e.tag === 1) {
    var n = e.type;
    if (_e(n)) return Lu(e, n, t);
  }
  return t;
}
function Mc(e, t, n, r, l, o, i, a, s) {
  return e = ea(n, r, !0, e, l, o, i, a, s), e.context = Lc(null), n = e.current, r = we(), l = Et(n), o = it(r, l), o.callback = t ?? null, Nt(n, o, l), e.current.lanes = l, mr(e, l, r), ze(e, r), e;
}
function Rl(e, t, n, r) {
  var l = t.current, o = we(), i = Et(l);
  return n = Lc(n), t.context === null ? t.context = n : t.pendingContext = n, t = it(o, i), t.payload = { element: e }, r = r === void 0 ? null : r, r !== null && (t.callback = r), e = Nt(l, t, i), e !== null && (Ke(e, l, i, o), Wr(e, l, i)), i;
}
function kl(e) {
  if (e = e.current, !e.child) return null;
  switch (e.child.tag) {
    case 5:
      return e.child.stateNode;
    default:
      return e.child.stateNode;
  }
}
function ms(e, t) {
  if (e = e.memoizedState, e !== null && e.dehydrated !== null) {
    var n = e.retryLane;
    e.retryLane = n !== 0 && n < t ? n : t;
  }
}
function ta(e, t) {
  ms(e, t), (e = e.alternate) && ms(e, t);
}
function ap() {
  return null;
}
var Dc = typeof reportError == "function" ? reportError : function(e) {
  console.error(e);
};
function na(e) {
  this._internalRoot = e;
}
Fl.prototype.render = na.prototype.render = function(e) {
  var t = this._internalRoot;
  if (t === null) throw Error(j(409));
  Rl(e, t, null, null);
};
Fl.prototype.unmount = na.prototype.unmount = function() {
  var e = this._internalRoot;
  if (e !== null) {
    this._internalRoot = null;
    var t = e.containerInfo;
    Kt(function() {
      Rl(null, e, null, null);
    }), t[st] = null;
  }
};
function Fl(e) {
  this._internalRoot = e;
}
Fl.prototype.unstable_scheduleHydration = function(e) {
  if (e) {
    var t = cu();
    e = { blockedOn: null, target: e, priority: t };
    for (var n = 0; n < vt.length && t !== 0 && t < vt[n].priority; n++) ;
    vt.splice(n, 0, e), n === 0 && fu(e);
  }
};
function ra(e) {
  return !(!e || e.nodeType !== 1 && e.nodeType !== 9 && e.nodeType !== 11);
}
function $l(e) {
  return !(!e || e.nodeType !== 1 && e.nodeType !== 9 && e.nodeType !== 11 && (e.nodeType !== 8 || e.nodeValue !== " react-mount-point-unstable "));
}
function hs() {
}
function sp(e, t, n, r, l) {
  if (l) {
    if (typeof r == "function") {
      var o = r;
      r = function() {
        var f = kl(i);
        o.call(f);
      };
    }
    var i = Mc(t, r, e, 0, null, !1, !1, "", hs);
    return e._reactRootContainer = i, e[st] = i.current, lr(e.nodeType === 8 ? e.parentNode : e), Kt(), i;
  }
  for (; l = e.lastChild; ) e.removeChild(l);
  if (typeof r == "function") {
    var a = r;
    r = function() {
      var f = kl(s);
      a.call(f);
    };
  }
  var s = ea(e, 0, !1, null, null, !1, !1, "", hs);
  return e._reactRootContainer = s, e[st] = s.current, lr(e.nodeType === 8 ? e.parentNode : e), Kt(function() {
    Rl(t, s, n, r);
  }), s;
}
function Ol(e, t, n, r, l) {
  var o = n._reactRootContainer;
  if (o) {
    var i = o;
    if (typeof l == "function") {
      var a = l;
      l = function() {
        var s = kl(i);
        a.call(s);
      };
    }
    Rl(t, i, e, l);
  } else i = sp(n, t, e, l, r);
  return kl(i);
}
su = function(e) {
  switch (e.tag) {
    case 3:
      var t = e.stateNode;
      if (t.current.memoizedState.isDehydrated) {
        var n = Un(t.pendingLanes);
        n !== 0 && (Si(t, n | 1), ze(t, ee()), !(U & 6) && (Nn = ee() + 500, Mt()));
      }
      break;
    case 13:
      Kt(function() {
        var r = ut(e, 1);
        if (r !== null) {
          var l = we();
          Ke(r, e, 1, l);
        }
      }), ta(e, 1);
  }
};
ji = function(e) {
  if (e.tag === 13) {
    var t = ut(e, 134217728);
    if (t !== null) {
      var n = we();
      Ke(t, e, 134217728, n);
    }
    ta(e, 134217728);
  }
};
uu = function(e) {
  if (e.tag === 13) {
    var t = Et(e), n = ut(e, t);
    if (n !== null) {
      var r = we();
      Ke(n, e, t, r);
    }
    ta(e, t);
  }
};
cu = function() {
  return V;
};
du = function(e, t) {
  var n = V;
  try {
    return V = e, t();
  } finally {
    V = n;
  }
};
To = function(e, t, n) {
  switch (t) {
    case "input":
      if (jo(e, n), t = n.name, n.type === "radio" && t != null) {
        for (n = e; n.parentNode; ) n = n.parentNode;
        for (n = n.querySelectorAll("input[name=" + JSON.stringify("" + t) + '][type="radio"]'), t = 0; t < n.length; t++) {
          var r = n[t];
          if (r !== e && r.form === e.form) {
            var l = _l(r);
            if (!l) throw Error(j(90));
            Hs(r), jo(r, l);
          }
        }
      }
      break;
    case "textarea":
      Ws(e, n);
      break;
    case "select":
      t = n.value, t != null && fn(e, !!n.multiple, t, !1);
  }
};
Js = Zi;
qs = Kt;
var up = { usingClientEntryPoint: !1, Events: [vr, ln, _l, Xs, Zs, Zi] }, Fn = { findFiberByHostInstance: It, bundleType: 0, version: "18.3.1", rendererPackageName: "react-dom" }, cp = { bundleType: Fn.bundleType, version: Fn.version, rendererPackageName: Fn.rendererPackageName, rendererConfig: Fn.rendererConfig, overrideHookState: null, overrideHookStateDeletePath: null, overrideHookStateRenamePath: null, overrideProps: null, overridePropsDeletePath: null, overridePropsRenamePath: null, setErrorHandler: null, setSuspenseHandler: null, scheduleUpdate: null, currentDispatcherRef: dt.ReactCurrentDispatcher, findHostInstanceByFiber: function(e) {
  return e = tu(e), e === null ? null : e.stateNode;
}, findFiberByHostInstance: Fn.findFiberByHostInstance || ap, findHostInstancesForRefresh: null, scheduleRefresh: null, scheduleRoot: null, setRefreshHandler: null, getCurrentFiber: null, reconcilerVersion: "18.3.1-next-f1338f8080-20240426" };
if (typeof __REACT_DEVTOOLS_GLOBAL_HOOK__ < "u") {
  var Or = __REACT_DEVTOOLS_GLOBAL_HOOK__;
  if (!Or.isDisabled && Or.supportsFiber) try {
    jl = Or.inject(cp), et = Or;
  } catch {
  }
}
De.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED = up;
De.createPortal = function(e, t) {
  var n = 2 < arguments.length && arguments[2] !== void 0 ? arguments[2] : null;
  if (!ra(t)) throw Error(j(200));
  return ip(e, t, null, n);
};
De.createRoot = function(e, t) {
  if (!ra(e)) throw Error(j(299));
  var n = !1, r = "", l = Dc;
  return t != null && (t.unstable_strictMode === !0 && (n = !0), t.identifierPrefix !== void 0 && (r = t.identifierPrefix), t.onRecoverableError !== void 0 && (l = t.onRecoverableError)), t = ea(e, 1, !1, null, null, n, !1, r, l), e[st] = t.current, lr(e.nodeType === 8 ? e.parentNode : e), new na(t);
};
De.findDOMNode = function(e) {
  if (e == null) return null;
  if (e.nodeType === 1) return e;
  var t = e._reactInternals;
  if (t === void 0)
    throw typeof e.render == "function" ? Error(j(188)) : (e = Object.keys(e).join(","), Error(j(268, e)));
  return e = tu(t), e = e === null ? null : e.stateNode, e;
};
De.flushSync = function(e) {
  return Kt(e);
};
De.hydrate = function(e, t, n) {
  if (!$l(t)) throw Error(j(200));
  return Ol(null, e, t, !0, n);
};
De.hydrateRoot = function(e, t, n) {
  if (!ra(e)) throw Error(j(405));
  var r = n != null && n.hydratedSources || null, l = !1, o = "", i = Dc;
  if (n != null && (n.unstable_strictMode === !0 && (l = !0), n.identifierPrefix !== void 0 && (o = n.identifierPrefix), n.onRecoverableError !== void 0 && (i = n.onRecoverableError)), t = Mc(t, null, e, 1, n ?? null, l, !1, o, i), e[st] = t.current, lr(e), r) for (e = 0; e < r.length; e++) n = r[e], l = n._getVersion, l = l(n._source), t.mutableSourceEagerHydrationData == null ? t.mutableSourceEagerHydrationData = [n, l] : t.mutableSourceEagerHydrationData.push(
    n,
    l
  );
  return new Fl(t);
};
De.render = function(e, t, n) {
  if (!$l(t)) throw Error(j(200));
  return Ol(null, e, t, !1, n);
};
De.unmountComponentAtNode = function(e) {
  if (!$l(e)) throw Error(j(40));
  return e._reactRootContainer ? (Kt(function() {
    Ol(null, null, e, !1, function() {
      e._reactRootContainer = null, e[st] = null;
    });
  }), !0) : !1;
};
De.unstable_batchedUpdates = Zi;
De.unstable_renderSubtreeIntoContainer = function(e, t, n, r) {
  if (!$l(n)) throw Error(j(200));
  if (e == null || e._reactInternals === void 0) throw Error(j(38));
  return Ol(e, t, n, !1, r);
};
De.version = "18.3.1-next-f1338f8080-20240426";
function Rc() {
  if (!(typeof __REACT_DEVTOOLS_GLOBAL_HOOK__ > "u" || typeof __REACT_DEVTOOLS_GLOBAL_HOOK__.checkDCE != "function"))
    try {
      __REACT_DEVTOOLS_GLOBAL_HOOK__.checkDCE(Rc);
    } catch (e) {
      console.error(e);
    }
}
Rc(), Rs.exports = De;
var dp = Rs.exports, Fc, vs = dp;
Fc = vs.createRoot, vs.hydrateRoot;
class fp extends Error {
  constructor(t, n) {
    super(t), this.status = n, this.name = "ApiError";
  }
}
function pp(e, t) {
  async function n(r, l = {}) {
    const o = { ...l.headers ?? {} }, i = t();
    i && (o.Authorization = "Bearer " + i);
    let a;
    l.body !== void 0 && (o["Content-Type"] = "application/json", a = JSON.stringify(l.body));
    const s = await e(r, { method: l.method ?? "GET", headers: o, body: a });
    if (!s.ok) {
      let h = `HTTP ${s.status}`;
      try {
        const v = await s.json();
        h = v.detail || v.title || h;
      } catch {
      }
      throw new fp(h, s.status);
    }
    return s.status === 204 ? void 0 : (s.headers.get("content-type") ?? "").includes("json") ? await s.json() : await s.text();
  }
  return {
    get: (r) => n(r),
    post: (r, l) => n(r, { method: "POST", body: l }),
    put: (r, l) => n(r, { method: "PUT", body: l }),
    del: (r) => n(r, { method: "DELETE" })
  };
}
const gs = new Intl.NumberFormat("es-ES", { minimumFractionDigits: 2, maximumFractionDigits: 2 }), mp = new Intl.NumberFormat("es-ES", { maximumFractionDigits: 3 });
function je(e, t) {
  if (e == null) return "";
  switch (t) {
    case "Moneda":
      return `${gs.format(e)} €`;
    case "Porcentaje":
      return `${gs.format(e)} %`;
    default:
      return mp.format(e);
  }
}
function ui(e, t) {
  return e == null || t == null || t === 0 ? null : Math.round((e - t) / Math.abs(t) * 1e4) / 100;
}
const po = ["enero", "febrero", "marzo", "abril", "mayo", "junio", "julio", "agosto", "septiembre", "octubre", "noviembre", "diciembre"], hp = ["", "lunes", "martes", "miércoles", "jueves", "viernes", "sábado", "domingo"];
function ne(e, t) {
  return e == null || e === "" ? "(sin valor)" : t === "mes" && /^\d{4}-\d{2}$/.test(e) ? `${po[Number(e.slice(5)) - 1]} ${e.slice(0, 4)}` : t === "mes_anio" && /^\d{2}$/.test(e) ? po[Number(e) - 1] : t === "dia_semana" && /^\d$/.test(e) ? hp[Number(e)] : t === "trimestre" ? e.replace("-T", " · T") : t === "semana" ? e.replace("-S", " · semana ") : /^\d{4}-\d{2}-\d{2}$/.test(e) ? `${e.slice(8, 10)}/${e.slice(5, 7)}/${e.slice(0, 4)}` : t === "mes_vencimiento" && /^\d{4}-\d{2}$/.test(e) ? `${po[Number(e.slice(5)) - 1]} ${e.slice(0, 4)}` : e;
}
const xs = (e) => `${e.getFullYear()}-${String(e.getMonth() + 1).padStart(2, "0")}-${String(e.getDate()).padStart(2, "0")}`, vp = [
  ["este_mes", "Este mes"],
  ["mes_anterior", "Mes anterior"],
  ["este_trimestre", "Este trimestre"],
  ["trimestre_anterior", "Trimestre anterior"],
  ["este_anio", "Este año"],
  ["anio_anterior", "Año anterior"],
  ["ultimos_12_meses", "Últimos 12 meses"],
  ["hasta_hoy", "Este año hasta hoy"],
  ["todo", "Todo"],
  ["personalizado", "Personalizado…"]
];
function gp(e, t = /* @__PURE__ */ new Date()) {
  const n = t.getFullYear(), r = t.getMonth(), l = (i, a, s) => xs(new Date(i, a, s)), o = Math.floor(r / 3) * 3;
  switch (e) {
    case "este_mes":
      return { desde: l(n, r, 1), hasta: l(n, r + 1, 0) };
    case "mes_anterior":
      return { desde: l(n, r - 1, 1), hasta: l(n, r, 0) };
    case "este_trimestre":
      return { desde: l(n, o, 1), hasta: l(n, o + 3, 0) };
    case "trimestre_anterior":
      return { desde: l(n, o - 3, 1), hasta: l(n, o, 0) };
    case "este_anio":
      return { desde: l(n, 0, 1), hasta: l(n, 11, 31) };
    case "anio_anterior":
      return { desde: l(n - 1, 0, 1), hasta: l(n - 1, 11, 31) };
    case "ultimos_12_meses":
      return { desde: l(n, r - 11, 1), hasta: l(n, r + 1, 0) };
    case "hasta_hoy":
      return { desde: l(n, 0, 1), hasta: xs(t) };
    default:
      return { desde: null, hasta: null };
  }
}
function ys(e) {
  return e.map((t) => {
    const n = t == null ? "" : typeof t == "number" ? String(t).replace(".", ",") : t;
    return /[";\n]/.test(n) ? `"${n.replace(/"/g, '""')}"` : n;
  }).join(";");
}
const Ir = 7, mo = (e) => e === "Fecha";
function xp(e, t) {
  const n = e.medidas[t];
  if (!n) return null;
  const r = (f) => f ?? (n.aditiva ? 0 : null), l = e.filas.filter((f) => f.nivel === 1 && !f.resto), o = e.dimensiones[0];
  if (e.columna && e.valoresColumna.length) {
    const f = mo(e.columna.tipo), h = mo(o == null ? void 0 : o.tipo);
    if (f || !h) {
      const k = [...l].sort((m, x) => Math.abs(x.valores[t] ?? 0) - Math.abs(m.valores[t] ?? 0)), N = k.slice(0, Ir), d = k.slice(Ir), c = N.map((m) => ({ nombre: ne(m.claves[0], o == null ? void 0 : o.clave), valores: e.valoresColumna.map((x, C) => {
        var E, S;
        return r((S = (E = m.celdas) == null ? void 0 : E[C]) == null ? void 0 : S[t]);
      }) }));
      return d.length && n.aditiva && c.push({ nombre: `Otros (${d.length})`, valores: e.valoresColumna.map((m, x) => d.reduce((C, E) => {
        var S, _;
        return C + (((_ = (S = E.celdas) == null ? void 0 : S[x]) == null ? void 0 : _[t]) ?? 0);
      }, 0)) }), { categorias: e.valoresColumna.map((m) => m ?? ""), etiquetas: e.valoresColumna.map((m) => ne(m, e.columna.clave)), series: c, tiempo: f, tipo: n.tipo, dimension: e.columna.clave };
    }
    const v = e.valoresColumna.map((k, N) => {
      var d, c, m;
      return { ci: N, t: Math.abs(((m = (c = (d = e.filas[0]) == null ? void 0 : d.celdas) == null ? void 0 : c[N]) == null ? void 0 : m[t]) ?? 0) };
    }).sort((k, N) => N.t - k.t), g = v.slice(0, Ir).map((k) => k.ci).map((k) => ({ nombre: ne(e.valoresColumna[k], e.columna.clave), valores: l.map((N) => {
      var d, c;
      return r((c = (d = N.celdas) == null ? void 0 : d[k]) == null ? void 0 : c[t]);
    }) })), w = v.slice(Ir).map((k) => k.ci);
    return w.length && n.aditiva && g.push({ nombre: `Otros (${w.length})`, valores: l.map((k) => w.reduce((N, d) => {
      var c, m;
      return N + (((m = (c = k.celdas) == null ? void 0 : c[d]) == null ? void 0 : m[t]) ?? 0);
    }, 0)) }), { categorias: l.map((k) => k.claves[0] ?? ""), etiquetas: l.map((k) => ne(k.claves[0], o == null ? void 0 : o.clave)), series: g, tiempo: !0, tipo: n.tipo, dimension: o == null ? void 0 : o.clave };
  }
  if (!o) return null;
  const i = mo(o.tipo), a = i ? l : l.slice(0, 15), s = [{ nombre: e.desdeAnterior ? "Periodo actual" : n.nombre, valores: a.map((f) => f.valores[t]) }];
  return e.desdeAnterior && s.push({ nombre: "Periodo anterior", valores: a.map((f) => {
    var h;
    return ((h = f.anteriores) == null ? void 0 : h[t]) ?? null;
  }), anterior: !0 }), { categorias: a.map((f) => f.claves[0] ?? ""), etiquetas: a.map((f) => ne(f.claves[0], o.clave)), series: s, tiempo: i, tipo: n.tipo, dimension: o.clave };
}
const $n = (e, t) => t.anterior ? "var(--ax-anterior)" : `var(--ax-s${e % 8 + 1})`;
function ws(e) {
  const t = Math.min(0, ...e), n = Math.max(0, ...e);
  if (t === n) return { min: 0, max: 1, marcas: [0, 1] };
  const r = (n - t) / 4, l = Math.pow(10, Math.floor(Math.log10(r))), o = [1, 2, 2.5, 5, 10].map((f) => f * l).find((f) => f >= r) ?? r, i = Math.floor(t / o) * o, a = Math.ceil(n / o) * o, s = [];
  for (let f = i; f <= a + o / 2; f += o) s.push(Math.round(f * 1e6) / 1e6);
  return { min: i, max: a, marcas: s };
}
const ho = (e) => {
  const t = Math.abs(e);
  return t >= 1e6 ? `${(e / 1e6).toLocaleString("es-ES", { maximumFractionDigits: 1 })} M` : t >= 1e3 ? `${(e / 1e3).toLocaleString("es-ES", { maximumFractionDigits: 1 })} mil` : e.toLocaleString("es-ES", { maximumFractionDigits: 2 });
};
function yp(e) {
  const { datos: t } = e, [n, r] = A.useState(null), l = A.useMemo(() => t.series.flatMap((d) => d.valores.filter((c) => c !== null)), [t]);
  if (!t.categorias.length || !l.length) return /* @__PURE__ */ u.jsx("div", { className: "muted ax-vacio", children: "No hay datos que dibujar." });
  const o = ws(l), i = t.series.length > 1 && /* @__PURE__ */ u.jsx("div", { className: "ax-leyenda", children: t.series.map((d, c) => /* @__PURE__ */ u.jsxs("span", { children: [
    /* @__PURE__ */ u.jsx("i", { className: d.anterior ? "anterior" : "", style: { background: $n(c, d) } }),
    d.nombre
  ] }, d.nombre)) }), a = n !== null && /* @__PURE__ */ u.jsxs("div", { className: "ax-tooltip", children: [
    /* @__PURE__ */ u.jsx("strong", { children: t.etiquetas[n] }),
    t.series.map((d, c) => /* @__PURE__ */ u.jsxs("div", { children: [
      /* @__PURE__ */ u.jsx("i", { style: { background: $n(c, d) } }),
      d.nombre,
      /* @__PURE__ */ u.jsx("b", { children: je(d.valores[n], t.tipo) })
    ] }, d.nombre))
  ] });
  if (t.tiempo) {
    const S = t.categorias.length, _ = (M) => 64 + (S === 1 ? 820 / 2 : M * 820 / (S - 1)), L = (M) => 12 + (o.max - M) * 234 / (o.max - o.min), D = Math.max(1, Math.ceil(S / 12));
    return /* @__PURE__ */ u.jsxs("figure", { className: "ax-grafico", "aria-label": e.titulo, children: [
      i,
      /* @__PURE__ */ u.jsxs("div", { className: "ax-lienzo", children: [
        /* @__PURE__ */ u.jsxs("svg", { viewBox: "0 0 900 280", role: "img", onMouseLeave: () => r(null), children: [
          o.marcas.map((M) => /* @__PURE__ */ u.jsxs("g", { children: [
            /* @__PURE__ */ u.jsx("line", { x1: 64, x2: 884, y1: L(M), y2: L(M), className: M === 0 ? "ax-cero" : "ax-rejilla" }),
            /* @__PURE__ */ u.jsx("text", { x: 56, y: L(M) + 4, className: "ax-eje", textAnchor: "end", children: ho(M) })
          ] }, M)),
          t.etiquetas.map((M, Q) => Q % D === 0 ? /* @__PURE__ */ u.jsx("text", { x: _(Q), y: 268, className: "ax-eje", textAnchor: "middle", children: M.length > 14 ? M.slice(0, 13) + "…" : M }, Q) : null),
          n !== null && /* @__PURE__ */ u.jsx("line", { x1: _(n), x2: _(n), y1: 12, y2: 246, className: "ax-cruz" }),
          t.series.map((M, Q) => {
            const Fe = M.valores.map((fe, pe) => fe === null ? null : [_(pe), L(fe)]), Dt = Fe.reduce((fe, pe, ft) => pe ? fe + `${fe && Fe[ft - 1] ? "L" : "M"}${pe[0].toFixed(1)},${pe[1].toFixed(1)}` : fe, "");
            return /* @__PURE__ */ u.jsxs("g", { children: [
              /* @__PURE__ */ u.jsx("path", { d: Dt, fill: "none", stroke: $n(Q, M), strokeWidth: 2, strokeDasharray: M.anterior ? "5 4" : void 0, strokeLinejoin: "round", strokeLinecap: "round" }),
              S <= 40 && Fe.map((fe, pe) => fe ? /* @__PURE__ */ u.jsx("circle", { cx: fe[0], cy: fe[1], r: n === pe ? 5 : 3.5, fill: $n(Q, M), stroke: "var(--surface,#fff)", strokeWidth: 2 }, pe) : null)
            ] }, M.nombre);
          }),
          t.categorias.map((M, Q) => /* @__PURE__ */ u.jsx("rect", { x: _(Q) - 820 / Math.max(1, S - 1) / 2, y: 12, width: 820 / Math.max(1, S - 1), height: 234, fill: "transparent", onMouseEnter: () => r(Q) }, Q))
        ] }),
        a
      ] })
    ] });
  }
  const s = t.series.filter((d) => !d.anterior).length > 1, f = t.categorias.map((d, c) => t.series.filter((m) => !m.anterior).reduce((m, x) => m + (x.valores[c] ?? 0), 0)), h = s ? ws([...f, ...l.filter((d) => d < 0)]) : o, v = 26, p = 900, g = 200, w = 90, k = t.categorias.length * v + 26, N = (d) => g + (d - h.min) * (p - g - w) / (h.max - h.min);
  return /* @__PURE__ */ u.jsxs("figure", { className: "ax-grafico", "aria-label": e.titulo, children: [
    i,
    /* @__PURE__ */ u.jsxs("div", { className: "ax-lienzo", children: [
      /* @__PURE__ */ u.jsxs("svg", { viewBox: `0 0 ${p} ${k}`, role: "img", onMouseLeave: () => r(null), children: [
        h.marcas.map((d) => /* @__PURE__ */ u.jsxs("g", { children: [
          /* @__PURE__ */ u.jsx("line", { x1: N(d), x2: N(d), y1: 0, y2: k - 22, className: d === 0 ? "ax-cero" : "ax-rejilla" }),
          /* @__PURE__ */ u.jsx("text", { x: N(d), y: k - 6, className: "ax-eje", textAnchor: "middle", children: ho(d) })
        ] }, d)),
        t.etiquetas.map((d, c) => {
          const m = c * v + 4;
          let x = 0;
          const C = t.series.map((S, _) => ({ s: S, si: _ })).filter((S) => !S.s.anterior), E = t.series.find((S) => S.anterior);
          return /* @__PURE__ */ u.jsxs("g", { onMouseEnter: () => r(c), className: n === c ? "ax-activa" : void 0, children: [
            /* @__PURE__ */ u.jsx("rect", { x: 0, y: m - 2, width: p, height: v, fill: "transparent" }),
            /* @__PURE__ */ u.jsx("text", { x: g - 8, y: m + 13, className: "ax-etiqueta", textAnchor: "end", children: d.length > 28 ? d.slice(0, 27) + "…" : d }),
            C.map(({ s: S, si: _ }) => {
              const L = S.valores[c] ?? 0, D = s ? x : 0;
              x += L;
              const M = N(Math.min(D, D + L)), Q = N(Math.max(D, D + L));
              return /* @__PURE__ */ u.jsx("rect", { x: M, y: m, width: Math.max(0, Q - M - (s ? 2 : 0)), height: E ? 12 : 16, rx: 3, fill: $n(_, S) }, _);
            }),
            E && E.valores[c] !== null && (() => {
              const S = E.valores[c] ?? 0, _ = N(Math.min(0, S)), L = N(Math.max(0, S));
              return /* @__PURE__ */ u.jsx("rect", { x: _, y: m + 14, width: Math.max(0, L - _), height: 4, rx: 2, fill: "var(--ax-anterior)" });
            })(),
            /* @__PURE__ */ u.jsx("text", { x: N(s ? Math.max(0, f[c]) : Math.max(0, t.series[0].valores[c] ?? 0)) + 6, y: m + 12, className: "ax-valor", children: ho(s ? f[c] : t.series[0].valores[c] ?? 0) })
          ] }, c);
        })
      ] }),
      a
    ] })
  ] });
}
const $c = [["en", "es"], ["no_en", "no es"], ["contiene", "contiene"], ["empieza", "empieza por"], ["desde", "desde"], ["hasta", "hasta"], ["vacio", "está vacío"], ["no_vacio", "no está vacío"]], wp = (e) => {
  var t;
  return ((t = $c.find((n) => n[0] === e)) == null ? void 0 : t[1]) ?? e;
};
function Jr(e, t, n) {
  return { filtros: [], filtrosMedida: [], comparar: null, limite: null, columna: null, ordenarPor: null, ascendente: !1, ...e, periodo: t, grafico: n ?? null };
}
function kp(e) {
  const { anfitrion: t } = e, n = A.useMemo(() => pp((N, d) => fetch(N, d), t.token), [t]), [r, l] = A.useState(null), [o, i] = A.useState([]), [a, s] = A.useState(null), [f, h] = A.useState(null), [v, p] = A.useState(""), g = A.useCallback(() => n.get("/analisis/informes").then(i).catch(() => i([])), [n]);
  A.useEffect(() => {
    n.get("/analisis/catalogo").then((N) => {
      var c;
      l(N);
      const d = ((c = e.inicial) == null ? void 0 : c.plantilla) && N.plantillas.find((m) => m.clave === e.inicial.plantilla);
      d && s(Jr(d.consulta, d.periodo, d.grafico));
    }).catch((N) => p(N.message)), g();
  }, [n]);
  const w = (N) => {
    try {
      s(JSON.parse(N.definicion)), h(N);
    } catch {
      t.aviso("La definición del informe no es válida.", "err");
    }
  };
  if (v) return /* @__PURE__ */ u.jsx("div", { className: "panel", children: /* @__PURE__ */ u.jsx("p", { className: "muted", children: v }) });
  if (!r) return /* @__PURE__ */ u.jsx("div", { className: "muted", children: "Cargando…" });
  if (!a)
    return /* @__PURE__ */ u.jsx(
      Sp,
      {
        catalogo: r,
        informes: o,
        alAbrir: w,
        alPlantilla: (N) => {
          h(null), s(Jr(N.consulta, N.periodo, N.grafico));
        },
        alNuevo: (N) => {
          h(null), s(Jr({ dataset: N.clave, filas: N.filasDefecto, medidas: N.medidasDefecto }, "este_anio"));
        },
        alBorrar: async (N) => {
          try {
            await n.del(`/analisis/informes/${N.id}`), t.aviso("Informe borrado.", "ok"), g();
          } catch (d) {
            t.aviso(d.message, "err");
          }
        }
      }
    );
  const k = r.datasets.find((N) => N.clave === a.dataset);
  return k ? /* @__PURE__ */ u.jsx(
    jp,
    {
      api: n,
      anfitrion: t,
      dataset: k,
      datasets: r.datasets,
      def: a,
      setDef: s,
      actual: f,
      alGuardado: (N) => {
        h(N), g();
      },
      alVolver: () => {
        s(null), h(null);
      }
    }
  ) : /* @__PURE__ */ u.jsxs("div", { className: "panel", children: [
    /* @__PURE__ */ u.jsx("p", { className: "muted", children: "Ese conjunto de datos no está disponible." }),
    /* @__PURE__ */ u.jsx("button", { className: "btn small", onClick: () => s(null), children: "Volver" })
  ] });
}
function Sp(e) {
  const t = (n) => {
    var r;
    return ((r = e.catalogo.datasets.find((l) => l.clave === n)) == null ? void 0 : r.nombre) ?? n;
  };
  return /* @__PURE__ */ u.jsxs("div", { className: "ax-raiz", children: [
    e.informes.length > 0 && /* @__PURE__ */ u.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ u.jsx("div", { className: "panel-head", children: /* @__PURE__ */ u.jsx("h2", { children: "Mis informes" }) }),
      /* @__PURE__ */ u.jsx("div", { className: "ax-tarjetas", children: e.informes.map((n) => /* @__PURE__ */ u.jsxs("div", { className: "ax-tarjeta", onClick: () => e.alAbrir(n), children: [
        /* @__PURE__ */ u.jsxs("div", { className: "ax-tarjeta-tit", children: [
          n.favorito ? "★ " : "",
          n.nombre
        ] }),
        /* @__PURE__ */ u.jsxs("div", { className: "muted", children: [
          t(n.dataset),
          n.compartido ? " · compartido" : "",
          n.propio ? "" : " · de otro usuario"
        ] }),
        n.propio && /* @__PURE__ */ u.jsx("button", { className: "dx-enlace ax-borrar", title: "Borrar", onClick: (r) => {
          r.stopPropagation(), confirm(`¿Borrar el informe «${n.nombre}»?`) && e.alBorrar(n);
        }, children: "Borrar" })
      ] }, n.id)) })
    ] }),
    /* @__PURE__ */ u.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ u.jsx("div", { className: "panel-head", children: /* @__PURE__ */ u.jsx("h2", { children: "Informes listos para usar" }) }),
      /* @__PURE__ */ u.jsx("div", { className: "ax-tarjetas", children: e.catalogo.plantillas.map((n) => /* @__PURE__ */ u.jsxs("div", { className: "ax-tarjeta", onClick: () => e.alPlantilla(n), children: [
        /* @__PURE__ */ u.jsx("div", { className: "ax-tarjeta-tit", children: n.nombre }),
        /* @__PURE__ */ u.jsx("div", { className: "muted", children: n.descripcion }),
        /* @__PURE__ */ u.jsx("div", { className: "ax-etiqueta-ds", children: t(n.consulta.dataset) })
      ] }, n.clave)) })
    ] }),
    /* @__PURE__ */ u.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ u.jsx("div", { className: "panel-head", children: /* @__PURE__ */ u.jsx("h2", { children: "Nuevo análisis" }) }),
      /* @__PURE__ */ u.jsx("div", { className: "ax-tarjetas", children: e.catalogo.datasets.map((n) => /* @__PURE__ */ u.jsxs("div", { className: "ax-tarjeta ax-nuevo", onClick: () => e.alNuevo(n), children: [
        /* @__PURE__ */ u.jsxs("div", { className: "ax-tarjeta-tit", children: [
          "+ ",
          n.nombre
        ] }),
        /* @__PURE__ */ u.jsx("div", { className: "muted", children: n.descripcion }),
        /* @__PURE__ */ u.jsxs("div", { className: "ax-etiqueta-ds", children: [
          n.dimensiones.length,
          " dimensiones · ",
          n.medidas.length,
          " medidas"
        ] })
      ] }, n.clave)) })
    ] })
  ] });
}
function jp(e) {
  var G, b, Rt, Xe, Zt, Ze, Ft, la;
  const { api: t, anfitrion: n, dataset: r, def: l, setDef: o } = e, [i, a] = A.useState(null), [s, f] = A.useState(!1), [h, v] = A.useState(""), [p, g] = A.useState(/* @__PURE__ */ new Set()), [w, k] = A.useState(null), [N, d] = A.useState(null), [c, m] = A.useState(null), [x, C] = A.useState(l.grafico !== "no"), E = A.useRef(0), S = l.periodo === "personalizado" ? { desde: l.desde ?? null, hasta: l.hasta ?? null } : gp(l.periodo), _ = A.useMemo(() => ({
    dataset: l.dataset,
    filas: l.filas,
    columna: l.columna || null,
    medidas: l.medidas,
    filtros: l.filtros,
    filtrosMedida: l.filtrosMedida,
    desde: S.desde,
    hasta: S.hasta,
    comparar: S.desde && S.hasta && l.comparar || null,
    ordenarPor: l.ordenarPor || null,
    ascendente: !!l.ascendente,
    limite: l.limite || null
  }), [l, S.desde, S.hasta]);
  A.useEffect(() => {
    const y = ++E.current;
    f(!0);
    const T = setTimeout(() => {
      t.post("/analisis/consulta", _).then((I) => {
        y === E.current && (a(I), v(""));
      }).catch((I) => {
        y === E.current && v(I.message);
      }).finally(() => {
        y === E.current && f(!1);
      });
    }, 250);
    return () => clearTimeout(T);
  }, [t, _]);
  const L = (y) => o({ ...l, ...y }), D = (y) => r.dimensiones.find((T) => T.clave === y), M = l.filas ?? [], Q = l.medidas ?? [], Fe = r.dimensiones.filter((y) => !M.includes(y.clave) && y.clave !== l.columna), Dt = [...new Set(r.dimensiones.map((y) => y.grupo))], fe = (y) => y.claves.map((T, I) => ({ dimension: M[I], operador: "en", valores: [T ?? ""] }));
  async function pe(y, T) {
    const I = y ? fe(y) : [];
    T !== void 0 && l.columna && I.push({ dimension: l.columna, operador: "en", valores: [T ?? ""] });
    const J = y && y.nivel > 0 ? y.claves.map((ae, xr) => ne(ae, M[xr])).join(" › ") : "Todos los registros";
    d({ titulo: J, datos: null });
    try {
      const ae = await t.post("/analisis/detalle", { dataset: l.dataset, filtros: [...l.filtros ?? [], ...I], desde: S.desde, hasta: S.hasta, limite: 1e3 });
      d({ titulo: J, datos: ae });
    } catch (ae) {
      d({ titulo: J, datos: null, error: ae.message });
    }
  }
  async function ft(y) {
    var oa;
    if (!i) return;
    const T = i.dimensiones.map(($) => $.nombre), I = T.map(($) => ({ titulo: $, tipo: "texto" })), J = ($) => $.aditiva ? "suma" : "no", ae = ($) => $ === "Moneda" ? "moneda" : $ === "Porcentaje" ? "porcentaje" : $ === "Numero" ? "numero" : "texto";
    i.columna ? (i.valoresColumna.forEach(($) => i.medidas.forEach((xe) => I.push({ titulo: `${ne($, i.columna.clave)} · ${xe.nombre}`, tipo: ae(xe.tipo), total: J(xe) }))), i.medidas.forEach(($) => I.push({ titulo: `Total · ${$.nombre}`, tipo: ae($.tipo), total: J($) }))) : i.medidas.forEach(($) => {
      I.push({ titulo: $.nombre, tipo: ae($.tipo), total: J($) }), i.desdeAnterior && (I.push({ titulo: `${$.nombre} (anterior)`, tipo: ae($.tipo), total: J($) }), I.push({ titulo: `${$.nombre} Δ %`, tipo: "porcentaje", total: "no" }));
    });
    const xr = i.filas.filter(($) => $.nivel > 0).map(($) => {
      const xe = T.map((yr, se) => se < $.claves.length ? $.resto && se === 0 ? $.claves[0] : ne($.claves[se], i.dimensiones[se].clave) : se === $.claves.length ? "Total" : "");
      return i.columna ? (i.valoresColumna.forEach((yr, se) => i.medidas.forEach((Ul, wr) => {
        var ia, aa;
        return xe.push(((aa = (ia = $.celdas) == null ? void 0 : ia[se]) == null ? void 0 : aa[wr]) ?? null);
      })), i.medidas.forEach((yr, se) => xe.push($.valores[se]))) : i.medidas.forEach((yr, se) => {
        var Ul, wr;
        xe.push($.valores[se]), i.desdeAnterior && (xe.push(((Ul = $.anteriores) == null ? void 0 : Ul[se]) ?? null), xe.push(ui($.valores[se], (wr = $.anteriores) == null ? void 0 : wr[se])));
      }), xe;
    }), Il = ((oa = e.actual) == null ? void 0 : oa.nombre) ?? i.titulo;
    if (y === "xlsx")
      try {
        const $ = n.token(), xe = await fetch("/exportar/xlsx", {
          method: "POST",
          headers: { "Content-Type": "application/json", ...$ ? { Authorization: "Bearer " + $ } : {} },
          body: JSON.stringify({ titulo: Il, columnas: I, filas: xr.filter((yr, se) => i.filas[se + 1].nivel === i.dimensiones.length || i.filas[se + 1].resto) })
        });
        if (xe.ok) {
          Ss(await xe.blob(), `${ks(Il)}.xlsx`);
          return;
        }
        if (xe.status !== 404) throw new Error(`No se pudo generar el Excel (HTTP ${xe.status}).`);
      } catch ($) {
        n.aviso($.message, "err");
        return;
      }
    const Ic = [ys(I.map(($) => $.titulo)), ...xr.map(ys)];
    Ss(new Blob(["\uFEFF" + Ic.join(`\r
`)], { type: "text/csv;charset=utf-8" }), `${ks(Il)}.csv`);
  }
  const z = Math.max(0, Q.indexOf(l.medidaGrafico ?? "")), R = i ? xp(i, z) : null, F = i == null ? void 0 : i.filas[0];
  return /* @__PURE__ */ u.jsxs("div", { className: "ax-disenador", children: [
    /* @__PURE__ */ u.jsxs("aside", { className: "ax-lateral panel", children: [
      /* @__PURE__ */ u.jsxs("div", { className: "ax-lat-cab", children: [
        /* @__PURE__ */ u.jsx("button", { className: "btn small secondary", onClick: e.alVolver, children: "← Informes" }),
        /* @__PURE__ */ u.jsx("select", { value: l.dataset, onChange: (y) => {
          const T = e.datasets.find((I) => I.clave === y.target.value);
          o(Jr({ dataset: T.clave, filas: T.filasDefecto, medidas: T.medidasDefecto }, l.periodo));
        }, children: e.datasets.map((y) => /* @__PURE__ */ u.jsx("option", { value: y.clave, children: y.nombre }, y.clave)) })
      ] }),
      /* @__PURE__ */ u.jsxs(qt, { titulo: "Periodo", children: [
        /* @__PURE__ */ u.jsx("select", { value: l.periodo, onChange: (y) => L({ periodo: y.target.value, desde: S.desde, hasta: S.hasta }), children: vp.map(([y, T]) => /* @__PURE__ */ u.jsx("option", { value: y, children: T }, y)) }),
        l.periodo === "personalizado" && /* @__PURE__ */ u.jsxs("div", { className: "ax-dos", children: [
          /* @__PURE__ */ u.jsx("input", { type: "date", value: l.desde ?? "", onChange: (y) => L({ desde: y.target.value || null }) }),
          /* @__PURE__ */ u.jsx("input", { type: "date", value: l.hasta ?? "", onChange: (y) => L({ hasta: y.target.value || null }) })
        ] }),
        /* @__PURE__ */ u.jsxs("select", { value: l.comparar ?? "", disabled: !S.desde, onChange: (y) => L({ comparar: y.target.value || null }), title: S.desde ? "" : "Elige un periodo con fechas para comparar", children: [
          /* @__PURE__ */ u.jsx("option", { value: "", children: "Sin comparar" }),
          /* @__PURE__ */ u.jsx("option", { value: "anio_anterior", children: "Comparar con el año anterior" }),
          /* @__PURE__ */ u.jsx("option", { value: "periodo_anterior", children: "Comparar con el periodo anterior" })
        ] })
      ] }),
      /* @__PURE__ */ u.jsxs(qt, { titulo: "Filas (agrupar por)", children: [
        M.map((y, T) => {
          var I;
          return /* @__PURE__ */ u.jsxs("div", { className: "ax-chip-fila", children: [
            /* @__PURE__ */ u.jsxs("span", { children: [
              T + 1,
              ". ",
              ((I = D(y)) == null ? void 0 : I.nombre) ?? y
            ] }),
            /* @__PURE__ */ u.jsx("button", { className: "dx-icono", disabled: T === 0, title: "Subir", onClick: () => {
              const J = [...M];
              [J[T - 1], J[T]] = [J[T], J[T - 1]], L({ filas: J });
            }, children: "↑" }),
            /* @__PURE__ */ u.jsx("button", { className: "dx-icono", title: "Quitar", onClick: () => L({ filas: M.filter((J) => J !== y), ordenarPor: l.ordenarPor === y ? null : l.ordenarPor }), children: "✕" })
          ] }, y);
        }),
        M.length < 4 && /* @__PURE__ */ u.jsx(Oc, { grupos: Dt, dimensiones: Fe, texto: "+ Añadir nivel", alElegir: (y) => L({ filas: [...M, y] }) })
      ] }),
      /* @__PURE__ */ u.jsx(qt, { titulo: "Columnas (tabla dinámica)", children: /* @__PURE__ */ u.jsxs("select", { value: l.columna ?? "", onChange: (y) => L({ columna: y.target.value || null }), children: [
        /* @__PURE__ */ u.jsx("option", { value: "", children: "Sin columnas" }),
        Dt.map((y) => /* @__PURE__ */ u.jsx("optgroup", { label: y, children: r.dimensiones.filter((T) => T.grupo === y && !M.includes(T.clave)).map((T) => /* @__PURE__ */ u.jsx("option", { value: T.clave, children: T.nombre }, T.clave)) }, y))
      ] }) }),
      /* @__PURE__ */ u.jsx(qt, { titulo: "Medidas", children: /* @__PURE__ */ u.jsx("div", { className: "ax-medidas", children: r.medidas.map((y) => /* @__PURE__ */ u.jsxs("label", { className: "dx-check", title: y.descripcion ?? "", children: [
        /* @__PURE__ */ u.jsx("input", { type: "checkbox", checked: Q.includes(y.clave), onChange: (T) => L({ medidas: T.target.checked ? [...Q, y.clave] : Q.filter((I) => I !== y.clave) }) }),
        " ",
        y.nombre
      ] }, y.clave)) }) }),
      /* @__PURE__ */ u.jsxs(qt, { titulo: "Filtros", children: [
        /* @__PURE__ */ u.jsx(Np, { api: t, def: l, dataset: r, fechas: S, alCambiar: (y) => L({ filtros: y }) }),
        /* @__PURE__ */ u.jsx(Cp, { dataset: r, filtros: l.filtrosMedida ?? [], alCambiar: (y) => L({ filtrosMedida: y }) })
      ] }),
      /* @__PURE__ */ u.jsxs(qt, { titulo: "Orden y límite", children: [
        /* @__PURE__ */ u.jsxs("select", { value: l.ordenarPor ?? "", onChange: (y) => L({ ordenarPor: y.target.value || null }), children: [
          /* @__PURE__ */ u.jsx("option", { value: "", children: "Por la primera medida (o por fecha)" }),
          Q.map((y) => {
            var T;
            return /* @__PURE__ */ u.jsxs("option", { value: y, children: [
              "Por ",
              (T = r.medidas.find((I) => I.clave === y)) == null ? void 0 : T.nombre
            ] }, y);
          }),
          M.map((y) => {
            var T;
            return /* @__PURE__ */ u.jsxs("option", { value: y, children: [
              "Por ",
              (T = D(y)) == null ? void 0 : T.nombre,
              " (alfabético)"
            ] }, y);
          })
        ] }),
        /* @__PURE__ */ u.jsxs("div", { className: "ax-dos", children: [
          /* @__PURE__ */ u.jsxs("select", { value: l.ascendente ? "asc" : "desc", onChange: (y) => L({ ascendente: y.target.value === "asc" }), children: [
            /* @__PURE__ */ u.jsx("option", { value: "desc", children: "De mayor a menor" }),
            /* @__PURE__ */ u.jsx("option", { value: "asc", children: "De menor a mayor" })
          ] }),
          /* @__PURE__ */ u.jsxs("select", { value: l.limite ?? "", onChange: (y) => L({ limite: y.target.value ? Number(y.target.value) : null }), children: [
            /* @__PURE__ */ u.jsx("option", { value: "", children: "Todos" }),
            [5, 10, 20, 50, 100].map((y) => /* @__PURE__ */ u.jsxs("option", { value: y, children: [
              "Los ",
              y,
              " primeros"
            ] }, y))
          ] })
        ] })
      ] })
    ] }),
    /* @__PURE__ */ u.jsxs("section", { className: "ax-principal", children: [
      /* @__PURE__ */ u.jsxs("div", { className: "panel", children: [
        /* @__PURE__ */ u.jsxs("div", { className: "panel-head", children: [
          /* @__PURE__ */ u.jsxs("div", { children: [
            /* @__PURE__ */ u.jsx("h2", { children: ((G = e.actual) == null ? void 0 : G.nombre) ?? (i == null ? void 0 : i.titulo) ?? r.nombre }),
            /* @__PURE__ */ u.jsxs("div", { className: "muted ax-subtitulo", children: [
              S.desde ? `${ne(S.desde)} – ${ne(S.hasta)}` : "Todo el histórico",
              i != null && i.desdeAnterior ? ` · frente a ${ne(i.desdeAnterior)} – ${ne(i.hastaAnterior)}` : "",
              (((b = l.filtros) == null ? void 0 : b.length) ?? 0) + (((Rt = l.filtrosMedida) == null ? void 0 : Rt.length) ?? 0) > 0 ? ` · ${(((Xe = l.filtros) == null ? void 0 : Xe.length) ?? 0) + (((Zt = l.filtrosMedida) == null ? void 0 : Zt.length) ?? 0)} filtro(s)` : "",
              s ? " · calculando…" : ""
            ] })
          ] }),
          /* @__PURE__ */ u.jsxs("div", { className: "dx-acciones", children: [
            /* @__PURE__ */ u.jsx("button", { className: "btn small secondary", onClick: () => C(!x), children: x ? "Ocultar gráfico" : "Ver gráfico" }),
            /* @__PURE__ */ u.jsx("button", { className: "btn small secondary", disabled: !i, onClick: () => pe(null), children: "Registros" }),
            /* @__PURE__ */ u.jsx("button", { className: "btn small secondary", disabled: !i, onClick: () => ft("xlsx"), children: "Excel" }),
            /* @__PURE__ */ u.jsx("button", { className: "btn small secondary", disabled: !i, onClick: () => ft("csv"), children: "CSV" }),
            /* @__PURE__ */ u.jsx("button", { className: "btn small secondary", disabled: !i, onClick: () => {
              var y;
              return Pp(((y = e.actual) == null ? void 0 : y.nombre) ?? (i == null ? void 0 : i.titulo) ?? "");
            }, children: "Imprimir" }),
            ((Ze = e.actual) == null ? void 0 : Ze.propio) && /* @__PURE__ */ u.jsx("button", { className: "btn small secondary", onClick: () => m("como"), children: "Guardar como…" }),
            /* @__PURE__ */ u.jsx("button", { className: "btn small", onClick: () => {
              var y;
              return m((y = e.actual) != null && y.propio ? "guardar" : "como");
            }, children: (Ft = e.actual) != null && Ft.propio ? "Guardar" : "Guardar informe" })
          ] })
        ] }),
        h && /* @__PURE__ */ u.jsxs("div", { className: "dx-aviso", children: [
          "⚠ ",
          h
        ] }),
        (i == null ? void 0 : i.truncado) && /* @__PURE__ */ u.jsx("div", { className: "dx-aviso", children: "⚠ Hay demasiados grupos: se muestran los primeros. Filtra o quita un nivel." }),
        i && F && /* @__PURE__ */ u.jsx("div", { className: "ax-kpis", children: i.medidas.map((y, T) => {
          var J, ae;
          const I = ui(F.valores[T], (J = F.anteriores) == null ? void 0 : J[T]);
          return /* @__PURE__ */ u.jsxs("div", { className: `ax-kpi ${T === z ? "activo" : ""}`, onClick: () => L({ medidaGrafico: y.clave }), title: "Dibujar esta medida", children: [
            /* @__PURE__ */ u.jsx("small", { children: y.nombre }),
            /* @__PURE__ */ u.jsx("strong", { children: je(F.valores[T], y.tipo) || "—" }),
            i.desdeAnterior && /* @__PURE__ */ u.jsx("span", { className: I === null ? "muted" : I >= 0 ? "ax-sube" : "ax-baja", children: I === null ? "sin datos anteriores" : `${I >= 0 ? "▲" : "▼"} ${Math.abs(I).toLocaleString("es-ES")} % · antes ${je((ae = F.anteriores) == null ? void 0 : ae[T], y.tipo)}` })
          ] }, y.clave);
        }) }),
        x && R && i && i.filas.length > 1 && /* @__PURE__ */ u.jsx(yp, { datos: R, titulo: i.titulo })
      ] }),
      i && /* @__PURE__ */ u.jsx(Ep, { res: i, plegadas: p, setPlegadas: g, alMenu: (y, T, I) => k({ fila: y, x: T, y: I }), alCelda: (y, T) => pe(y, T) })
    ] }),
    w && /* @__PURE__ */ u.jsx("div", { className: "ax-capa", onClick: () => k(null), children: /* @__PURE__ */ u.jsxs("div", { className: "ax-menu", style: { left: Math.min(w.x, window.innerWidth - 260), top: Math.min(w.y, window.innerHeight - 320) }, onClick: (y) => y.stopPropagation(), children: [
      /* @__PURE__ */ u.jsx("div", { className: "ax-menu-tit", children: w.fila.claves.map((y, T) => ne(y, M[T])).join(" › ") }),
      /* @__PURE__ */ u.jsx("button", { onClick: () => {
        k(null), pe(w.fila);
      }, children: "Ver los registros" }),
      /* @__PURE__ */ u.jsx("button", { onClick: () => {
        k(null), L({ filtros: [...l.filtros ?? [], ...fe(w.fila)] });
      }, children: "Filtrar: solo esto" }),
      /* @__PURE__ */ u.jsxs("button", { onClick: () => {
        const y = w.fila.nivel - 1;
        k(null), L({ filtros: [...l.filtros ?? [], { dimension: M[y], operador: "no_en", valores: [w.fila.claves[y] ?? ""] }] });
      }, children: [
        "Excluir «",
        ne(w.fila.claves[w.fila.nivel - 1], M[w.fila.nivel - 1]),
        "»"
      ] }),
      /* @__PURE__ */ u.jsx("div", { className: "ax-menu-sub", children: "Desglosar por…" }),
      /* @__PURE__ */ u.jsx("div", { className: "ax-menu-lista", children: Fe.map((y) => /* @__PURE__ */ u.jsx("button", { onClick: () => {
        k(null), L({ filtros: [...l.filtros ?? [], ...fe(w.fila)], filas: [y.clave] });
      }, children: y.nombre }, y.clave)) })
    ] }) }),
    N && /* @__PURE__ */ u.jsx(_p, { detalle: N, alCerrar: () => d(null), anfitrion: n }),
    c && /* @__PURE__ */ u.jsx(
      zp,
      {
        inicial: c === "guardar" ? e.actual : null,
        sugerido: ((la = e.actual) == null ? void 0 : la.nombre) ?? (i == null ? void 0 : i.titulo) ?? "",
        alCerrar: () => m(null),
        alGuardar: async (y, T, I) => {
          const J = { nombre: y, dataset: l.dataset, definicion: JSON.stringify({ ...l, grafico: x ? l.grafico ?? "si" : "no" }), compartido: T, favorito: I };
          try {
            const ae = c === "guardar" && e.actual ? await t.put(`/analisis/informes/${e.actual.id}`, J) : await t.post("/analisis/informes", J);
            n.aviso("Informe guardado.", "ok"), m(null), e.alGuardado(ae);
          } catch (ae) {
            n.aviso(ae.message, "err");
          }
        }
      }
    )
  ] });
}
function qt(e) {
  return /* @__PURE__ */ u.jsxs("div", { className: "ax-seccion", children: [
    /* @__PURE__ */ u.jsx("div", { className: "ax-seccion-tit", children: e.titulo }),
    e.children
  ] });
}
function Oc(e) {
  return /* @__PURE__ */ u.jsxs("select", { value: "", onChange: (t) => t.target.value && e.alElegir(t.target.value), children: [
    /* @__PURE__ */ u.jsx("option", { value: "", children: e.texto }),
    e.grupos.map((t) => {
      const n = e.dimensiones.filter((r) => r.grupo === t);
      return n.length ? /* @__PURE__ */ u.jsx("optgroup", { label: t, children: n.map((r) => /* @__PURE__ */ u.jsx("option", { value: r.clave, children: r.nombre }, r.clave)) }, t) : null;
    })
  ] });
}
function Np(e) {
  const t = e.def.filtros ?? [], [n, r] = A.useState(null), [l, o] = A.useState(""), [i, a] = A.useState([]), s = (h) => {
    var v;
    return ((v = e.dataset.dimensiones.find((p) => p.clave === h)) == null ? void 0 : v.nombre) ?? h;
  }, f = n && ["en", "no_en"].includes(n.operador);
  return A.useEffect(() => {
    if (!n || !f) return;
    const h = new URLSearchParams({ dimension: n.dimension, texto: l });
    e.fechas.desde && h.set("desde", e.fechas.desde), e.fechas.hasta && h.set("hasta", e.fechas.hasta);
    const v = setTimeout(() => e.api.get(`/analisis/${e.def.dataset}/valores?${h}`).then(a).catch(() => a([])), 200);
    return () => clearTimeout(v);
  }, [n == null ? void 0 : n.dimension, n == null ? void 0 : n.operador, l]), /* @__PURE__ */ u.jsxs("div", { className: "ax-filtros", children: [
    t.map((h, v) => /* @__PURE__ */ u.jsxs("div", { className: "ax-filtro", children: [
      /* @__PURE__ */ u.jsxs("span", { children: [
        /* @__PURE__ */ u.jsx("b", { children: s(h.dimension) }),
        " ",
        wp(h.operador),
        " ",
        (h.valores ?? []).map((p) => ne(p, h.dimension)).join(", ")
      ] }),
      /* @__PURE__ */ u.jsx("button", { className: "dx-icono", title: "Quitar", onClick: () => e.alCambiar(t.filter((p, g) => g !== v)), children: "✕" })
    ] }, v)),
    !n && /* @__PURE__ */ u.jsx(Oc, { grupos: [...new Set(e.dataset.dimensiones.map((h) => h.grupo))], dimensiones: e.dataset.dimensiones, texto: "+ Filtrar por…", alElegir: (h) => {
      r({ dimension: h, operador: "en", valores: [] }), o("");
    } }),
    n && /* @__PURE__ */ u.jsxs("div", { className: "ax-nuevo-filtro", children: [
      /* @__PURE__ */ u.jsxs("div", { className: "ax-dos", children: [
        /* @__PURE__ */ u.jsx("b", { children: s(n.dimension) }),
        /* @__PURE__ */ u.jsx("select", { value: n.operador, onChange: (h) => r({ ...n, operador: h.target.value, valores: [] }), children: $c.map(([h, v]) => /* @__PURE__ */ u.jsx("option", { value: h, children: v }, h)) })
      ] }),
      f && /* @__PURE__ */ u.jsxs(u.Fragment, { children: [
        /* @__PURE__ */ u.jsx("input", { placeholder: "Buscar valores…", value: l, onChange: (h) => o(h.target.value), autoFocus: !0 }),
        /* @__PURE__ */ u.jsxs("div", { className: "ax-valores", children: [
          i.map((h) => {
            const v = h.valor ?? "", p = (n.valores ?? []).includes(v);
            return /* @__PURE__ */ u.jsxs("label", { className: "dx-check", children: [
              /* @__PURE__ */ u.jsx("input", { type: "checkbox", checked: p, onChange: () => r({ ...n, valores: p ? (n.valores ?? []).filter((g) => g !== v) : [...n.valores ?? [], v] }) }),
              /* @__PURE__ */ u.jsx("span", { children: ne(h.valor, n.dimension) }),
              /* @__PURE__ */ u.jsx("small", { className: "muted", children: h.registros.toLocaleString("es-ES") })
            ] }, v);
          }),
          !i.length && /* @__PURE__ */ u.jsx("div", { className: "muted", children: "Sin valores." })
        ] })
      ] }),
      n && ["contiene", "empieza", "desde", "hasta"].includes(n.operador) && /* @__PURE__ */ u.jsx("input", { placeholder: n.operador === "desde" || n.operador === "hasta" ? "Valor (p. ej. 2026-03 o 6)" : "Texto", value: (n.valores ?? [])[0] ?? "", onChange: (h) => r({ ...n, valores: [h.target.value] }), autoFocus: !0 }),
      /* @__PURE__ */ u.jsxs("div", { className: "ax-dos", children: [
        /* @__PURE__ */ u.jsx("button", { className: "btn small secondary", onClick: () => r(null), children: "Cancelar" }),
        /* @__PURE__ */ u.jsx(
          "button",
          {
            className: "btn small",
            disabled: !["vacio", "no_vacio"].includes(n.operador) && !(n.valores ?? []).some((h) => f || h.trim()),
            onClick: () => {
              e.alCambiar([...t, n]), r(null);
            },
            children: "Aplicar"
          }
        )
      ] })
    ] })
  ] });
}
function Cp(e) {
  const [t, n] = A.useState(null), r = (l) => {
    var o;
    return ((o = e.dataset.medidas.find((i) => i.clave === l)) == null ? void 0 : o.nombre) ?? l;
  };
  return /* @__PURE__ */ u.jsxs("div", { className: "ax-filtros", children: [
    e.filtros.map((l, o) => /* @__PURE__ */ u.jsxs("div", { className: "ax-filtro", children: [
      /* @__PURE__ */ u.jsxs("span", { children: [
        /* @__PURE__ */ u.jsx("b", { children: r(l.medida) }),
        " ",
        l.operador === "mayor" ? ">" : l.operador === "menor" ? "<" : "entre",
        " ",
        l.valor.toLocaleString("es-ES"),
        l.operador === "entre" ? ` y ${(l.hasta ?? 0).toLocaleString("es-ES")}` : ""
      ] }),
      /* @__PURE__ */ u.jsx("button", { className: "dx-icono", title: "Quitar", onClick: () => e.alCambiar(e.filtros.filter((i, a) => a !== o)), children: "✕" })
    ] }, o)),
    !t && /* @__PURE__ */ u.jsxs("select", { value: "", onChange: (l) => l.target.value && n({ medida: l.target.value, operador: "mayor", valor: 0 }), children: [
      /* @__PURE__ */ u.jsx("option", { value: "", children: "+ Filtrar por una cifra…" }),
      e.dataset.medidas.map((l) => /* @__PURE__ */ u.jsx("option", { value: l.clave, children: l.nombre }, l.clave))
    ] }),
    t && /* @__PURE__ */ u.jsxs("div", { className: "ax-nuevo-filtro", children: [
      /* @__PURE__ */ u.jsxs("div", { className: "ax-dos", children: [
        /* @__PURE__ */ u.jsx("b", { children: r(t.medida) }),
        /* @__PURE__ */ u.jsxs("select", { value: t.operador, onChange: (l) => n({ ...t, operador: l.target.value }), children: [
          /* @__PURE__ */ u.jsx("option", { value: "mayor", children: "mayor que" }),
          /* @__PURE__ */ u.jsx("option", { value: "menor", children: "menor que" }),
          /* @__PURE__ */ u.jsx("option", { value: "entre", children: "entre" })
        ] })
      ] }),
      /* @__PURE__ */ u.jsxs("div", { className: "ax-dos", children: [
        /* @__PURE__ */ u.jsx("input", { type: "number", value: t.valor, onChange: (l) => n({ ...t, valor: Number(l.target.value) }) }),
        t.operador === "entre" && /* @__PURE__ */ u.jsx("input", { type: "number", value: t.hasta ?? 0, onChange: (l) => n({ ...t, hasta: Number(l.target.value) }) })
      ] }),
      /* @__PURE__ */ u.jsx("div", { className: "muted ax-ayuda", children: "Se aplica al último nivel de las filas (p. ej. clientes con ventas > 10.000 €)." }),
      /* @__PURE__ */ u.jsxs("div", { className: "ax-dos", children: [
        /* @__PURE__ */ u.jsx("button", { className: "btn small secondary", onClick: () => n(null), children: "Cancelar" }),
        /* @__PURE__ */ u.jsx("button", { className: "btn small", onClick: () => {
          e.alCambiar([...e.filtros, t]), n(null);
        }, children: "Aplicar" })
      ] })
    ] })
  ] });
}
const vo = (e) => `${e.nivel}|${e.claves.join("")}`;
function Ep(e) {
  const { res: t, plegadas: n } = e, r = t.dimensiones.length, l = t.filas[0], o = !!t.desdeAnterior && !t.columna, i = [];
  let a = null;
  for (const p of t.filas.slice(1))
    a !== null && p.nivel > a || (a = null, i.push(p), n.has(vo(p)) && p.nivel < r && (a = p.nivel));
  const s = /* @__PURE__ */ new Map();
  for (const p of t.filas) s.set(p.nivel, Math.max(s.get(p.nivel) ?? 0, Math.abs(p.valores[0] ?? 0)));
  const f = (p) => e.setPlegadas(new Set(p ? t.filas.filter((g) => g.nivel > 0 && g.nivel < r).map(vo) : [])), h = (p, g, w) => /* @__PURE__ */ u.jsxs("td", { className: `num ${p != null && p < 0 ? "ax-neg" : ""}`, children: [
    w !== void 0 && w > 0 && /* @__PURE__ */ u.jsx("span", { className: "ax-barra", style: { width: `${Math.min(100, w * 100)}%` } }),
    /* @__PURE__ */ u.jsx("span", { className: "ax-num", children: je(p, g) })
  ] }), v = (p, g) => {
    const w = ui(p, g);
    return /* @__PURE__ */ u.jsx("td", { className: `num ${w === null ? "muted" : w >= 0 ? "ax-sube" : "ax-baja"}`, children: w === null ? "—" : `${w >= 0 ? "+" : ""}${w.toLocaleString("es-ES")} %` });
  };
  return /* @__PURE__ */ u.jsxs("div", { className: "panel ax-tabla-panel", children: [
    /* @__PURE__ */ u.jsxs("div", { className: "ax-tabla-herr", children: [
      /* @__PURE__ */ u.jsxs("span", { className: "muted", children: [
        t.filas.filter((p) => p.nivel === r && r > 0).length.toLocaleString("es-ES"),
        " grupos",
        r > 1 ? " · pulsa ▸ para plegar" : "",
        " · pulsa una fila para verla, filtrarla o desglosarla"
      ] }),
      r > 1 && /* @__PURE__ */ u.jsxs("span", { children: [
        /* @__PURE__ */ u.jsx("button", { className: "dx-enlace", onClick: () => f(!0), children: "Plegar todo" }),
        /* @__PURE__ */ u.jsx("button", { className: "dx-enlace", onClick: () => f(!1), children: "Desplegar todo" })
      ] })
    ] }),
    /* @__PURE__ */ u.jsx("div", { className: "ax-scroll", children: /* @__PURE__ */ u.jsxs("table", { className: "ax-tabla", "data-totales": "no", "data-pro": "1", children: [
      /* @__PURE__ */ u.jsx("thead", { children: t.columna ? /* @__PURE__ */ u.jsxs(u.Fragment, { children: [
        /* @__PURE__ */ u.jsxs("tr", { children: [
          /* @__PURE__ */ u.jsx("th", { rowSpan: 2, children: t.dimensiones.map((p) => p.nombre).join(" › ") || "Total" }),
          t.valoresColumna.map((p) => /* @__PURE__ */ u.jsx("th", { colSpan: t.medidas.length, className: "ax-th-col", children: ne(p, t.columna.clave) }, p ?? "")),
          /* @__PURE__ */ u.jsx("th", { colSpan: t.medidas.length, className: "ax-th-col ax-th-total", children: "Total" })
        ] }),
        /* @__PURE__ */ u.jsx("tr", { children: [...t.valoresColumna, "__total"].flatMap((p) => t.medidas.map((g) => /* @__PURE__ */ u.jsx("th", { className: "num", children: g.nombre }, `${p}-${g.clave}`))) })
      ] }) : /* @__PURE__ */ u.jsxs("tr", { children: [
        /* @__PURE__ */ u.jsx("th", { children: t.dimensiones.map((p) => p.nombre).join(" › ") || "Total" }),
        t.medidas.flatMap((p) => o ? [/* @__PURE__ */ u.jsx("th", { className: "num", children: p.nombre }, p.clave), /* @__PURE__ */ u.jsx("th", { className: "num ax-th-ant", children: "Anterior" }, p.clave + "a"), /* @__PURE__ */ u.jsx("th", { className: "num ax-th-ant", children: "Δ %" }, p.clave + "d")] : [/* @__PURE__ */ u.jsx("th", { className: "num", children: p.nombre }, p.clave)])
      ] }) }),
      /* @__PURE__ */ u.jsx("tbody", { children: i.map((p) => {
        var N;
        const g = vo(p), w = p.nivel < r && !p.resto, k = s.get(p.nivel) || 1;
        return /* @__PURE__ */ u.jsxs("tr", { className: `ax-n${p.nivel} ${p.nivel < r ? "ax-subtotal" : ""} ${p.resto ? "ax-resto" : ""}`, children: [
          /* @__PURE__ */ u.jsxs("td", { className: "ax-dim", style: { paddingLeft: 10 + (p.nivel - 1) * 18 }, children: [
            w ? /* @__PURE__ */ u.jsx("button", { className: "ax-plegar", onClick: () => {
              const d = new Set(n);
              d.has(g) ? d.delete(g) : d.add(g), e.setPlegadas(d);
            }, children: n.has(g) ? "▸" : "▾" }) : /* @__PURE__ */ u.jsx("span", { className: "ax-plegar-hueco" }),
            /* @__PURE__ */ u.jsx("span", { className: p.resto ? "muted" : "ax-dim-texto", onClick: (d) => !p.resto && e.alMenu(p, d.clientX, d.clientY), children: p.resto ? p.claves[0] : ne(p.claves[p.nivel - 1], (N = t.dimensiones[p.nivel - 1]) == null ? void 0 : N.clave) })
          ] }),
          t.columna ? [
            ...t.valoresColumna.map((d, c) => t.medidas.map((m, x) => {
              var C, E, S, _;
              return /* @__PURE__ */ u.jsx("td", { className: `num ax-clic ${(((E = (C = p.celdas) == null ? void 0 : C[c]) == null ? void 0 : E[x]) ?? 0) < 0 ? "ax-neg" : ""}`, onClick: () => !p.resto && e.alCelda(p, d), children: je((_ = (S = p.celdas) == null ? void 0 : S[c]) == null ? void 0 : _[x], m.tipo) }, `${c}-${x}`);
            })),
            ...t.medidas.map((d, c) => /* @__PURE__ */ u.jsx("td", { className: "num ax-col-total", children: je(p.valores[c], d.tipo) }, `t-${c}`))
          ] : t.medidas.map((d, c) => {
            var m, x;
            return o ? [h(p.valores[c], d.tipo, c === 0 ? Math.abs(p.valores[0] ?? 0) / k : void 0), /* @__PURE__ */ u.jsx("td", { className: "num muted", children: je((m = p.anteriores) == null ? void 0 : m[c], d.tipo) }, c + "a"), v(p.valores[c], (x = p.anteriores) == null ? void 0 : x[c])] : h(p.valores[c], d.tipo, c === 0 ? Math.abs(p.valores[0] ?? 0) / k : void 0);
          })
        ] }, g);
      }) }),
      l && /* @__PURE__ */ u.jsx("tfoot", { children: /* @__PURE__ */ u.jsxs("tr", { children: [
        /* @__PURE__ */ u.jsx("td", { children: "Total" }),
        t.columna ? [...t.valoresColumna.map((p, g) => t.medidas.map((w, k) => {
          var N, d;
          return /* @__PURE__ */ u.jsx("td", { className: "num", children: je((d = (N = l.celdas) == null ? void 0 : N[g]) == null ? void 0 : d[k], w.tipo) }, `${g}-${k}`);
        })), ...t.medidas.map((p, g) => /* @__PURE__ */ u.jsx("td", { className: "num ax-col-total", children: je(l.valores[g], p.tipo) }, `t${g}`))] : t.medidas.map((p, g) => {
          var w, k;
          return o ? [/* @__PURE__ */ u.jsx("td", { className: "num", children: je(l.valores[g], p.tipo) }, g), /* @__PURE__ */ u.jsx("td", { className: "num", children: je((w = l.anteriores) == null ? void 0 : w[g], p.tipo) }, g + "a"), v(l.valores[g], (k = l.anteriores) == null ? void 0 : k[g])] : /* @__PURE__ */ u.jsx("td", { className: "num", children: je(l.valores[g], p.tipo) }, g);
        })
      ] }) })
    ] }) })
  ] });
}
function _p(e) {
  const { datos: t } = e.detalle, n = (t == null ? void 0 : t.columnas.map((r) => r.tipo === "Moneda" || r.tipo === "Numero")) ?? [];
  return /* @__PURE__ */ u.jsx("div", { className: "ax-capa", onClick: e.alCerrar, children: /* @__PURE__ */ u.jsxs("div", { className: "ax-ventana panel", onClick: (r) => r.stopPropagation(), children: [
    /* @__PURE__ */ u.jsxs("div", { className: "panel-head", children: [
      /* @__PURE__ */ u.jsxs("h2", { children: [
        "Registros · ",
        e.detalle.titulo
      ] }),
      /* @__PURE__ */ u.jsx("button", { className: "btn small secondary", onClick: e.alCerrar, children: "Cerrar" })
    ] }),
    e.detalle.error && /* @__PURE__ */ u.jsxs("div", { className: "dx-aviso", children: [
      "⚠ ",
      e.detalle.error
    ] }),
    !t && !e.detalle.error && /* @__PURE__ */ u.jsx("div", { className: "muted", children: "Cargando…" }),
    t && /* @__PURE__ */ u.jsxs(u.Fragment, { children: [
      /* @__PURE__ */ u.jsxs("div", { className: "muted ax-ayuda", children: [
        t.filas.length.toLocaleString("es-ES"),
        " registro(s)",
        t.truncado ? " (se muestran los 1.000 más recientes)" : "",
        t.vistaDocumento && e.anfitrion.abrirDocumento ? " · pulsa una fila para abrir el documento" : ""
      ] }),
      /* @__PURE__ */ u.jsx("div", { className: "ax-scroll ax-scroll-detalle", children: /* @__PURE__ */ u.jsxs("table", { className: "ax-tabla", "data-totales": "no", "data-pro": "1", children: [
        /* @__PURE__ */ u.jsx("thead", { children: /* @__PURE__ */ u.jsx("tr", { children: t.columnas.map((r) => /* @__PURE__ */ u.jsx("th", { className: r.tipo === "Texto" || r.tipo === "Fecha" ? "" : "num", children: r.nombre }, r.clave)) }) }),
        /* @__PURE__ */ u.jsx("tbody", { children: t.filas.map((r, l) => /* @__PURE__ */ u.jsx(
          "tr",
          {
            className: t.ids[l] && t.vistaDocumento && e.anfitrion.abrirDocumento ? "ax-clic" : "",
            onClick: () => {
              const o = t.ids[l];
              o && t.vistaDocumento && e.anfitrion.abrirDocumento && e.anfitrion.abrirDocumento(t.vistaDocumento, o);
            },
            children: r.map((o, i) => {
              const a = t.columnas[i];
              return /* @__PURE__ */ u.jsx("td", { className: a.tipo === "Texto" || a.tipo === "Fecha" ? "" : "num", children: a.tipo === "Fecha" ? ne(o) : typeof o == "number" ? je(o, a.tipo) : String(o ?? "") }, i);
            })
          },
          l
        )) }),
        /* @__PURE__ */ u.jsx("tfoot", { children: /* @__PURE__ */ u.jsx("tr", { children: t.columnas.map((r, l) => /* @__PURE__ */ u.jsx("td", { className: "num", children: l === 0 ? "Total" : n[l] && r.nombre !== "Precio" && r.nombre !== "Coste unitario" ? je(t.filas.reduce((o, i) => o + (typeof i[l] == "number" ? i[l] : 0), 0), r.tipo) : "" }, l)) }) })
      ] }) })
    ] })
  ] }) });
}
function zp(e) {
  var a, s, f;
  const [t, n] = A.useState(((a = e.inicial) == null ? void 0 : a.nombre) ?? e.sugerido), [r, l] = A.useState(((s = e.inicial) == null ? void 0 : s.compartido) ?? !1), [o, i] = A.useState(((f = e.inicial) == null ? void 0 : f.favorito) ?? !1);
  return /* @__PURE__ */ u.jsx("div", { className: "ax-capa", onClick: e.alCerrar, children: /* @__PURE__ */ u.jsxs("div", { className: "ax-dialogo panel", onClick: (h) => h.stopPropagation(), children: [
    /* @__PURE__ */ u.jsx("div", { className: "panel-head", children: /* @__PURE__ */ u.jsx("h2", { children: e.inicial ? "Guardar informe" : "Guardar como informe nuevo" }) }),
    /* @__PURE__ */ u.jsx("label", { children: "Nombre" }),
    /* @__PURE__ */ u.jsx("input", { value: t, onChange: (h) => n(h.target.value), autoFocus: !0, maxLength: 120 }),
    /* @__PURE__ */ u.jsxs("label", { className: "dx-check", children: [
      /* @__PURE__ */ u.jsx("input", { type: "checkbox", checked: r, onChange: (h) => l(h.target.checked) }),
      " Compartido con toda la empresa"
    ] }),
    /* @__PURE__ */ u.jsxs("label", { className: "dx-check", children: [
      /* @__PURE__ */ u.jsx("input", { type: "checkbox", checked: o, onChange: (h) => i(h.target.checked) }),
      " Favorito (sale el primero)"
    ] }),
    /* @__PURE__ */ u.jsx("p", { className: "muted ax-ayuda", children: "Se guarda el periodo relativo (p. ej. «este año»), así el informe siempre está al día." }),
    /* @__PURE__ */ u.jsxs("div", { className: "ax-dos", children: [
      /* @__PURE__ */ u.jsx("button", { className: "btn small secondary", onClick: e.alCerrar, children: "Cancelar" }),
      /* @__PURE__ */ u.jsx("button", { className: "btn small", disabled: !t.trim(), onClick: () => e.alGuardar(t.trim(), r, o), children: "Guardar" })
    ] })
  ] }) });
}
function ks(e) {
  return (e || "analisis").toLowerCase().normalize("NFD").replace(/[̀-ͯ]/g, "").replace(/[^a-z0-9]+/g, "-").replace(/^-|-$/g, "").slice(0, 60) || "analisis";
}
function Ss(e, t) {
  const n = URL.createObjectURL(e), r = document.createElement("a");
  r.href = n, r.download = t, r.click(), setTimeout(() => URL.revokeObjectURL(n), 6e4);
}
function Pp(e) {
  const t = document.querySelector(".ax-principal");
  if (!t) return;
  const n = window.open("", "_blank");
  if (!n) return;
  const r = [...document.querySelectorAll("style")].map((l) => l.outerHTML).join("");
  n.document.write(`<!doctype html><html><head><meta charset="utf-8"><title>${e.replace(/</g, "&lt;")}</title>${r}<style>body{background:#fff;padding:16px}.dx-acciones,.ax-tabla-herr button,.ax-plegar{display:none!important}.ax-scroll{overflow:visible!important;max-height:none!important}</style></head><body>${t.outerHTML}</body></html>`), n.document.close(), n.focus(), setTimeout(() => n.print(), 300);
}
const Tp = `
.ax-raiz { display:flex; flex-direction:column; gap:16px; }
.ax-raiz .panel, .ax-disenador .panel { margin:0; }
:root { --ax-s1:#2a78d6; --ax-s2:#eb6834; --ax-s3:#1baf7a; --ax-s4:#eda100; --ax-s5:#e87ba4; --ax-s6:#008300; --ax-s7:#4a3aa7; --ax-s8:#e34948; --ax-anterior:#9a9892; --ax-rejilla:#e6e9ee; --ax-barra:rgba(42,120,214,.12); }
:root[data-theme="dark"] { --ax-s1:#3987e5; --ax-s2:#d95926; --ax-s3:#199e70; --ax-s4:#c98500; --ax-s5:#d55181; --ax-s6:#008300; --ax-s7:#9085e9; --ax-s8:#e66767; --ax-anterior:#7a7973; --ax-rejilla:#232a36; --ax-barra:rgba(57,135,229,.20); }
.ax-tarjetas { display:grid; grid-template-columns:repeat(auto-fill, minmax(250px, 1fr)); gap:12px; }
.ax-tarjeta { position:relative; border:1px solid var(--line); border-radius:12px; padding:14px 14px 12px; cursor:pointer; background:var(--surface,#fff); transition:border-color .15s, box-shadow .15s; font-size:13px; line-height:1.45; }
.ax-tarjeta:hover { border-color:var(--accent); box-shadow:0 4px 14px rgba(8,145,178,.12); }
.ax-tarjeta-tit { font-weight:700; margin-bottom:4px; color:var(--ink); }
.ax-nuevo .ax-tarjeta-tit { color:var(--accent); }
.ax-etiqueta-ds { margin-top:8px; font-size:11.5px; color:var(--accent); font-weight:600; }
.ax-borrar { position:absolute; top:10px; right:8px; }
.ax-disenador { display:grid; grid-template-columns:290px minmax(0,1fr); gap:16px; align-items:start; }
.ax-lateral { position:sticky; top:12px; max-height:calc(100vh - 110px); overflow:auto; padding:14px !important; }
.ax-disenador .dx-check, .ax-dialogo .dx-check { display:flex; align-items:center; gap:8px; cursor:pointer; color:var(--ink); margin:6px 0 0; font-size:13px; }
.ax-disenador input[type=checkbox], .ax-dialogo input[type=checkbox] { width:auto !important; margin:0 !important; flex:none; }
.dx-icono { border:1px solid var(--line); background:var(--surface,#fff); border-radius:8px; width:28px; height:28px; cursor:pointer; margin-left:3px; color:var(--muted); font-weight:700; }
.dx-icono:hover { color:var(--accent); border-color:var(--accent); }
.dx-enlace { border:none; background:none; color:var(--accent); cursor:pointer; font-weight:600; padding:0 4px; font-size:12.5px; }
.dx-aviso { margin:8px 0; color:#b45309; font-weight:600; }
.dx-acciones { display:flex; flex-wrap:wrap; gap:6px; justify-content:flex-end; }
.ax-principal .panel-head { align-items:flex-start; gap:12px; }
.ax-lateral select, .ax-lateral input:not([type=checkbox]) { width:100%; padding:7px 9px; font-size:13px; border-radius:8px; margin:0 0 6px; }
.ax-lat-cab { display:flex; flex-direction:column; gap:8px; margin-bottom:6px; }
.ax-seccion { border-top:1px solid var(--line); padding:10px 0 4px; }
.ax-seccion-tit { font-size:11.5px; text-transform:uppercase; letter-spacing:.04em; color:var(--muted); font-weight:700; margin-bottom:8px; }
.ax-dos { display:flex; gap:6px; align-items:center; }
.ax-dos > * { flex:1; }
.ax-chip-fila { display:flex; align-items:center; gap:2px; background:var(--accent-soft); border-radius:8px; padding:3px 4px 3px 9px; margin-bottom:5px; font-size:13px; }
.ax-chip-fila span { flex:1; }
.ax-chip-fila .dx-icono { width:24px; height:24px; }
.ax-medidas { display:flex; flex-direction:column; gap:0; }
.ax-medidas .dx-check { margin-top:4px !important; font-size:13px; }
.ax-filtros { display:flex; flex-direction:column; gap:5px; margin-bottom:6px; }
.ax-filtro { display:flex; align-items:flex-start; gap:4px; border:1px solid var(--line); border-radius:8px; padding:5px 5px 5px 9px; font-size:12.5px; }
.ax-filtro span { flex:1; }
.ax-nuevo-filtro { border:1px dashed var(--accent); border-radius:10px; padding:8px; display:flex; flex-direction:column; gap:6px; }
.ax-valores { max-height:200px; overflow:auto; }
.ax-valores .dx-check { margin-top:3px !important; font-size:12.5px; }
.ax-valores .dx-check span { flex:1; }
.ax-ayuda { font-size:11.5px; margin:4px 0; }
.ax-principal { display:flex; flex-direction:column; gap:16px; min-width:0; }
.ax-subtitulo { font-size:12.5px; margin-top:3px; }
.ax-kpis { display:grid; grid-template-columns:repeat(auto-fit, minmax(170px, 1fr)); gap:10px; margin:6px 0 12px; }
.ax-kpi { border:1px solid var(--line); border-radius:12px; padding:10px 12px; cursor:pointer; display:flex; flex-direction:column; gap:2px; }
.ax-kpi.activo { border-color:var(--accent); box-shadow:inset 0 0 0 1px var(--accent); }
.ax-kpi small { color:var(--muted); font-size:12px; }
.ax-kpi strong { font-size:20px; font-variant-numeric:tabular-nums; color:var(--ink); }
.ax-kpi span { font-size:12px; }
.ax-sube { color:var(--pos,#0f9d58); }
.ax-baja { color:var(--neg,#e0533d); }
.ax-neg { color:var(--neg,#e0533d); }
.ax-grafico { margin:0; }
.ax-lienzo { position:relative; }
.ax-grafico svg { width:100%; height:auto; display:block; }
.ax-rejilla { stroke:var(--ax-rejilla); stroke-width:1; }
.ax-cero { stroke:var(--muted); stroke-width:1; opacity:.6; }
.ax-cruz { stroke:var(--muted); stroke-width:1; stroke-dasharray:3 3; }
.ax-eje { fill:var(--muted); font-size:11px; }
.ax-etiqueta { fill:var(--ink); font-size:12px; }
.ax-valor { fill:var(--muted); font-size:11px; font-variant-numeric:tabular-nums; }
.ax-activa .ax-etiqueta { font-weight:700; }
.ax-leyenda { display:flex; flex-wrap:wrap; gap:6px 16px; font-size:12px; color:var(--ink); margin-bottom:6px; }
.ax-leyenda i, .ax-tooltip i { display:inline-block; width:10px; height:10px; border-radius:3px; margin-right:6px; vertical-align:-1px; }
.ax-leyenda i.anterior { height:3px; vertical-align:3px; }
.ax-tooltip { position:absolute; top:8px; right:8px; pointer-events:none; background:var(--surface,#fff); border:1px solid var(--line); border-radius:10px; box-shadow:0 8px 24px rgba(15,23,42,.14); padding:8px 10px; font-size:12px; min-width:180px; }
.ax-tooltip div { display:flex; align-items:center; gap:4px; margin-top:3px; color:var(--muted); }
.ax-tooltip b { margin-left:auto; color:var(--ink); font-variant-numeric:tabular-nums; }
.ax-vacio { padding:24px; text-align:center; }
.ax-tabla-panel { padding:0 !important; overflow:hidden; }
.ax-tabla-herr { display:flex; justify-content:space-between; gap:10px; padding:10px 14px; font-size:12.5px; border-bottom:1px solid var(--line); flex-wrap:wrap; }
.ax-scroll { overflow:auto; max-height:70vh; }
.ax-scroll-detalle { max-height:60vh; }
.ax-tabla { width:100%; border-collapse:separate; border-spacing:0; font-size:13px; }
.ax-tabla th { position:sticky; top:0; z-index:1; background:var(--surface,#fff); font-size:11.5px; text-transform:uppercase; letter-spacing:.03em; color:var(--muted); padding:8px 10px; text-align:left; border-bottom:1px solid var(--line); white-space:nowrap; }
.ax-tabla thead tr:nth-child(2) th { top:31px; }
.ax-tabla th.num, .ax-tabla td.num { text-align:right; }
.ax-th-col { text-align:center !important; border-left:1px solid var(--line); }
.ax-th-total, .ax-col-total { background:rgba(148,163,184,.08); font-weight:600; }
.ax-th-ant { color:var(--muted); font-weight:500; }
.ax-tabla td { padding:6px 10px; border-bottom:1px solid var(--line); white-space:nowrap; font-variant-numeric:tabular-nums; position:relative; }
.ax-tabla tfoot td { position:sticky; bottom:0; background:var(--surface,#fff); font-weight:700; border-top:2px solid var(--ink); border-bottom:none; }
.ax-subtotal td { font-weight:600; background:rgba(148,163,184,.06); }
.ax-n1.ax-subtotal td { background:rgba(8,145,178,.07); }
.ax-resto td { font-style:italic; }
.ax-dim { max-width:420px; overflow:hidden; text-overflow:ellipsis; }
.ax-dim-texto { cursor:pointer; }
.ax-dim-texto:hover { color:var(--accent); text-decoration:underline; }
.ax-plegar { border:none; background:none; cursor:pointer; color:var(--muted); width:18px; padding:0; margin-right:4px; font-size:12px; }
.ax-plegar-hueco { display:inline-block; width:22px; }
.ax-barra { position:absolute; right:10px; top:5px; bottom:5px; background:var(--ax-barra); border-radius:0 4px 4px 0; }
.ax-num { position:relative; }
.ax-clic { cursor:pointer; }
.ax-clic:hover td, td.ax-clic:hover { background:var(--accent-soft); }
.ax-capa { position:fixed; inset:0; z-index:200; background:rgba(15,23,42,.25); }
.ax-menu { position:fixed; width:250px; background:var(--surface,#fff); border:1px solid var(--line); border-radius:12px; box-shadow:0 16px 40px rgba(15,23,42,.2); padding:6px; display:flex; flex-direction:column; }
.ax-menu button { text-align:left; border:none; background:none; padding:8px 10px; border-radius:8px; cursor:pointer; font-size:13px; color:var(--ink); }
.ax-menu button:hover { background:var(--accent-soft); color:var(--accent); }
.ax-menu-tit { font-weight:700; font-size:12.5px; padding:6px 10px 8px; border-bottom:1px solid var(--line); margin-bottom:4px; }
.ax-menu-sub { font-size:11px; text-transform:uppercase; color:var(--muted); padding:8px 10px 2px; letter-spacing:.04em; }
.ax-menu-lista { max-height:170px; overflow:auto; display:flex; flex-direction:column; }
.ax-ventana { position:fixed; left:50%; top:6vh; transform:translateX(-50%); width:min(1100px, 94vw); max-height:88vh; overflow:auto; }
.ax-dialogo { position:fixed; left:50%; top:18vh; transform:translateX(-50%); width:min(440px, 94vw); }
.ax-dialogo label { display:block; margin:10px 0 5px; font-size:12.5px; color:var(--muted); }
.ax-dialogo input[type=text], .ax-dialogo input:not([type]) { width:100%; }
@media (max-width: 980px) { .ax-disenador { grid-template-columns:1fr; } .ax-lateral { position:static; max-height:none; } }
`;
function Lp(e, t, n) {
  if (!document.getElementById("ax-estilos")) {
    const l = document.createElement("style");
    l.id = "ax-estilos", l.textContent = Tp, document.head.appendChild(l);
  }
  const r = Fc(e);
  return r.render(/* @__PURE__ */ u.jsx(kp, { anfitrion: t, inicial: n })), () => r.unmount();
}
export {
  Lp as montar
};
