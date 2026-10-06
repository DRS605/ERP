var dc = { exports: {} }, tl = {}, fc = { exports: {} }, X = {};
/**
 * @license React
 * react.production.min.js
 *
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */
var Kr = Symbol.for("react.element"), Od = Symbol.for("react.portal"), bd = Symbol.for("react.fragment"), Ud = Symbol.for("react.strict_mode"), Vd = Symbol.for("react.profiler"), Bd = Symbol.for("react.provider"), Hd = Symbol.for("react.context"), Wd = Symbol.for("react.forward_ref"), Qd = Symbol.for("react.suspense"), Gd = Symbol.for("react.memo"), Kd = Symbol.for("react.lazy"), Jo = Symbol.iterator;
function qd(e) {
  return e === null || typeof e != "object" ? null : (e = Jo && e[Jo] || e["@@iterator"], typeof e == "function" ? e : null);
}
var pc = { isMounted: function() {
  return !1;
}, enqueueForceUpdate: function() {
}, enqueueReplaceState: function() {
}, enqueueSetState: function() {
} }, mc = Object.assign, hc = {};
function ar(e, t, n) {
  this.props = e, this.context = t, this.refs = hc, this.updater = n || pc;
}
ar.prototype.isReactComponent = {};
ar.prototype.setState = function(e, t) {
  if (typeof e != "object" && typeof e != "function" && e != null) throw Error("setState(...): takes an object of state variables to update or a function which returns an object of state variables.");
  this.updater.enqueueSetState(this, e, t, "setState");
};
ar.prototype.forceUpdate = function(e) {
  this.updater.enqueueForceUpdate(this, e, "forceUpdate");
};
function vc() {
}
vc.prototype = ar.prototype;
function Qi(e, t, n) {
  this.props = e, this.context = t, this.refs = hc, this.updater = n || pc;
}
var Gi = Qi.prototype = new vc();
Gi.constructor = Qi;
mc(Gi, ar.prototype);
Gi.isPureReactComponent = !0;
var es = Array.isArray, xc = Object.prototype.hasOwnProperty, Ki = { current: null }, gc = { key: !0, ref: !0, __self: !0, __source: !0 };
function yc(e, t, n) {
  var r, a = {}, i = null, o = null;
  if (t != null) for (r in t.ref !== void 0 && (o = t.ref), t.key !== void 0 && (i = "" + t.key), t) xc.call(t, r) && !gc.hasOwnProperty(r) && (a[r] = t[r]);
  var s = arguments.length - 2;
  if (s === 1) a.children = n;
  else if (1 < s) {
    for (var c = Array(s), d = 0; d < s; d++) c[d] = arguments[d + 2];
    a.children = c;
  }
  if (e && e.defaultProps) for (r in s = e.defaultProps, s) a[r] === void 0 && (a[r] = s[r]);
  return { $$typeof: Kr, type: e, key: i, ref: o, props: a, _owner: Ki.current };
}
function Yd(e, t) {
  return { $$typeof: Kr, type: e.type, key: t, ref: e.ref, props: e.props, _owner: e._owner };
}
function qi(e) {
  return typeof e == "object" && e !== null && e.$$typeof === Kr;
}
function Xd(e) {
  var t = { "=": "=0", ":": "=2" };
  return "$" + e.replace(/[=:]/g, function(n) {
    return t[n];
  });
}
var ts = /\/+/g;
function Cl(e, t) {
  return typeof e == "object" && e !== null && e.key != null ? Xd("" + e.key) : t.toString(36);
}
function ya(e, t, n, r, a) {
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
        case Kr:
        case Od:
          o = !0;
      }
  }
  if (o) return o = e, a = a(o), e = r === "" ? "." + Cl(o, 0) : r, es(a) ? (n = "", e != null && (n = e.replace(ts, "$&/") + "/"), ya(a, t, n, "", function(d) {
    return d;
  })) : a != null && (qi(a) && (a = Yd(a, n + (!a.key || o && o.key === a.key ? "" : ("" + a.key).replace(ts, "$&/") + "/") + e)), t.push(a)), 1;
  if (o = 0, r = r === "" ? "." : r + ":", es(e)) for (var s = 0; s < e.length; s++) {
    i = e[s];
    var c = r + Cl(i, s);
    o += ya(i, t, n, c, a);
  }
  else if (c = qd(e), typeof c == "function") for (e = c.call(e), s = 0; !(i = e.next()).done; ) i = i.value, c = r + Cl(i, s++), o += ya(i, t, n, c, a);
  else if (i === "object") throw t = String(e), Error("Objects are not valid as a React child (found: " + (t === "[object Object]" ? "object with keys {" + Object.keys(e).join(", ") + "}" : t) + "). If you meant to render a collection of children, use an array instead.");
  return o;
}
function na(e, t, n) {
  if (e == null) return e;
  var r = [], a = 0;
  return ya(e, r, "", "", function(i) {
    return t.call(n, i, a++);
  }), r;
}
function Zd(e) {
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
var Ge = { current: null }, ja = { transition: null }, Jd = { ReactCurrentDispatcher: Ge, ReactCurrentBatchConfig: ja, ReactCurrentOwner: Ki };
function jc() {
  throw Error("act(...) is not supported in production builds of React.");
}
X.Children = { map: na, forEach: function(e, t, n) {
  na(e, function() {
    t.apply(this, arguments);
  }, n);
}, count: function(e) {
  var t = 0;
  return na(e, function() {
    t++;
  }), t;
}, toArray: function(e) {
  return na(e, function(t) {
    return t;
  }) || [];
}, only: function(e) {
  if (!qi(e)) throw Error("React.Children.only expected to receive a single React element child.");
  return e;
} };
X.Component = ar;
X.Fragment = bd;
X.Profiler = Vd;
X.PureComponent = Qi;
X.StrictMode = Ud;
X.Suspense = Qd;
X.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED = Jd;
X.act = jc;
X.cloneElement = function(e, t, n) {
  if (e == null) throw Error("React.cloneElement(...): The argument must be a React element, but you passed " + e + ".");
  var r = mc({}, e.props), a = e.key, i = e.ref, o = e._owner;
  if (t != null) {
    if (t.ref !== void 0 && (i = t.ref, o = Ki.current), t.key !== void 0 && (a = "" + t.key), e.type && e.type.defaultProps) var s = e.type.defaultProps;
    for (c in t) xc.call(t, c) && !gc.hasOwnProperty(c) && (r[c] = t[c] === void 0 && s !== void 0 ? s[c] : t[c]);
  }
  var c = arguments.length - 2;
  if (c === 1) r.children = n;
  else if (1 < c) {
    s = Array(c);
    for (var d = 0; d < c; d++) s[d] = arguments[d + 2];
    r.children = s;
  }
  return { $$typeof: Kr, type: e.type, key: a, ref: i, props: r, _owner: o };
};
X.createContext = function(e) {
  return e = { $$typeof: Hd, _currentValue: e, _currentValue2: e, _threadCount: 0, Provider: null, Consumer: null, _defaultValue: null, _globalName: null }, e.Provider = { $$typeof: Bd, _context: e }, e.Consumer = e;
};
X.createElement = yc;
X.createFactory = function(e) {
  var t = yc.bind(null, e);
  return t.type = e, t;
};
X.createRef = function() {
  return { current: null };
};
X.forwardRef = function(e) {
  return { $$typeof: Wd, render: e };
};
X.isValidElement = qi;
X.lazy = function(e) {
  return { $$typeof: Kd, _payload: { _status: -1, _result: e }, _init: Zd };
};
X.memo = function(e, t) {
  return { $$typeof: Gd, type: e, compare: t === void 0 ? null : t };
};
X.startTransition = function(e) {
  var t = ja.transition;
  ja.transition = {};
  try {
    e();
  } finally {
    ja.transition = t;
  }
};
X.unstable_act = jc;
X.useCallback = function(e, t) {
  return Ge.current.useCallback(e, t);
};
X.useContext = function(e) {
  return Ge.current.useContext(e);
};
X.useDebugValue = function() {
};
X.useDeferredValue = function(e) {
  return Ge.current.useDeferredValue(e);
};
X.useEffect = function(e, t) {
  return Ge.current.useEffect(e, t);
};
X.useId = function() {
  return Ge.current.useId();
};
X.useImperativeHandle = function(e, t, n) {
  return Ge.current.useImperativeHandle(e, t, n);
};
X.useInsertionEffect = function(e, t) {
  return Ge.current.useInsertionEffect(e, t);
};
X.useLayoutEffect = function(e, t) {
  return Ge.current.useLayoutEffect(e, t);
};
X.useMemo = function(e, t) {
  return Ge.current.useMemo(e, t);
};
X.useReducer = function(e, t, n) {
  return Ge.current.useReducer(e, t, n);
};
X.useRef = function(e) {
  return Ge.current.useRef(e);
};
X.useState = function(e) {
  return Ge.current.useState(e);
};
X.useSyncExternalStore = function(e, t, n) {
  return Ge.current.useSyncExternalStore(e, t, n);
};
X.useTransition = function() {
  return Ge.current.useTransition();
};
X.version = "18.3.1";
fc.exports = X;
var x = fc.exports;
/**
 * @license React
 * react-jsx-runtime.production.min.js
 *
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */
var ef = x, tf = Symbol.for("react.element"), nf = Symbol.for("react.fragment"), rf = Object.prototype.hasOwnProperty, af = ef.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED.ReactCurrentOwner, lf = { key: !0, ref: !0, __self: !0, __source: !0 };
function Nc(e, t, n) {
  var r, a = {}, i = null, o = null;
  n !== void 0 && (i = "" + n), t.key !== void 0 && (i = "" + t.key), t.ref !== void 0 && (o = t.ref);
  for (r in t) rf.call(t, r) && !lf.hasOwnProperty(r) && (a[r] = t[r]);
  if (e && e.defaultProps) for (r in t = e.defaultProps, t) a[r] === void 0 && (a[r] = t[r]);
  return { $$typeof: tf, type: e, key: i, ref: o, props: a, _owner: af.current };
}
tl.Fragment = nf;
tl.jsx = Nc;
tl.jsxs = Nc;
dc.exports = tl;
var l = dc.exports, Sc = { exports: {} }, lt = {}, wc = { exports: {} }, Cc = {};
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
  function t(E, y) {
    var L = E.length;
    E.push(y);
    e: for (; 0 < L; ) {
      var B = L - 1 >>> 1, W = E[B];
      if (0 < a(W, y)) E[B] = y, E[L] = W, L = B;
      else break e;
    }
  }
  function n(E) {
    return E.length === 0 ? null : E[0];
  }
  function r(E) {
    if (E.length === 0) return null;
    var y = E[0], L = E.pop();
    if (L !== y) {
      E[0] = L;
      e: for (var B = 0, W = E.length, q = W >>> 1; B < q; ) {
        var Se = 2 * (B + 1) - 1, je = E[Se], me = Se + 1, Re = E[me];
        if (0 > a(je, L)) me < W && 0 > a(Re, je) ? (E[B] = Re, E[me] = L, B = me) : (E[B] = je, E[Se] = L, B = Se);
        else if (me < W && 0 > a(Re, L)) E[B] = Re, E[me] = L, B = me;
        else break e;
      }
    }
    return y;
  }
  function a(E, y) {
    var L = E.sortIndex - y.sortIndex;
    return L !== 0 ? L : E.id - y.id;
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
  var c = [], d = [], j = 1, u = null, m = 3, h = !1, g = !1, w = !1, $ = typeof setTimeout == "function" ? setTimeout : null, p = typeof clearTimeout == "function" ? clearTimeout : null, f = typeof setImmediate < "u" ? setImmediate : null;
  typeof navigator < "u" && navigator.scheduling !== void 0 && navigator.scheduling.isInputPending !== void 0 && navigator.scheduling.isInputPending.bind(navigator.scheduling);
  function v(E) {
    for (var y = n(d); y !== null; ) {
      if (y.callback === null) r(d);
      else if (y.startTime <= E) r(d), y.sortIndex = y.expirationTime, t(c, y);
      else break;
      y = n(d);
    }
  }
  function k(E) {
    if (w = !1, v(E), !g) if (n(c) !== null) g = !0, pe(T);
    else {
      var y = n(d);
      y !== null && ge(k, y.startTime - E);
    }
  }
  function T(E, y) {
    g = !1, w && (w = !1, p(C), C = -1), h = !0;
    var L = m;
    try {
      for (v(y), u = n(c); u !== null && (!(u.expirationTime > y) || E && !P()); ) {
        var B = u.callback;
        if (typeof B == "function") {
          u.callback = null, m = u.priorityLevel;
          var W = B(u.expirationTime <= y);
          y = e.unstable_now(), typeof W == "function" ? u.callback = W : u === n(c) && r(c), v(y);
        } else r(c);
        u = n(c);
      }
      if (u !== null) var q = !0;
      else {
        var Se = n(d);
        Se !== null && ge(k, Se.startTime - y), q = !1;
      }
      return q;
    } finally {
      u = null, m = L, h = !1;
    }
  }
  var F = !1, D = null, C = -1, M = 5, b = -1;
  function P() {
    return !(e.unstable_now() - b < M);
  }
  function Q() {
    if (D !== null) {
      var E = e.unstable_now();
      b = E;
      var y = !0;
      try {
        y = D(!0, E);
      } finally {
        y ? ce() : (F = !1, D = null);
      }
    } else F = !1;
  }
  var ce;
  if (typeof f == "function") ce = function() {
    f(Q);
  };
  else if (typeof MessageChannel < "u") {
    var Le = new MessageChannel(), Ve = Le.port2;
    Le.port1.onmessage = Q, ce = function() {
      Ve.postMessage(null);
    };
  } else ce = function() {
    $(Q, 0);
  };
  function pe(E) {
    D = E, F || (F = !0, ce());
  }
  function ge(E, y) {
    C = $(function() {
      E(e.unstable_now());
    }, y);
  }
  e.unstable_IdlePriority = 5, e.unstable_ImmediatePriority = 1, e.unstable_LowPriority = 4, e.unstable_NormalPriority = 3, e.unstable_Profiling = null, e.unstable_UserBlockingPriority = 2, e.unstable_cancelCallback = function(E) {
    E.callback = null;
  }, e.unstable_continueExecution = function() {
    g || h || (g = !0, pe(T));
  }, e.unstable_forceFrameRate = function(E) {
    0 > E || 125 < E ? console.error("forceFrameRate takes a positive int between 0 and 125, forcing frame rates higher than 125 fps is not supported") : M = 0 < E ? Math.floor(1e3 / E) : 5;
  }, e.unstable_getCurrentPriorityLevel = function() {
    return m;
  }, e.unstable_getFirstCallbackNode = function() {
    return n(c);
  }, e.unstable_next = function(E) {
    switch (m) {
      case 1:
      case 2:
      case 3:
        var y = 3;
        break;
      default:
        y = m;
    }
    var L = m;
    m = y;
    try {
      return E();
    } finally {
      m = L;
    }
  }, e.unstable_pauseExecution = function() {
  }, e.unstable_requestPaint = function() {
  }, e.unstable_runWithPriority = function(E, y) {
    switch (E) {
      case 1:
      case 2:
      case 3:
      case 4:
      case 5:
        break;
      default:
        E = 3;
    }
    var L = m;
    m = E;
    try {
      return y();
    } finally {
      m = L;
    }
  }, e.unstable_scheduleCallback = function(E, y, L) {
    var B = e.unstable_now();
    switch (typeof L == "object" && L !== null ? (L = L.delay, L = typeof L == "number" && 0 < L ? B + L : B) : L = B, E) {
      case 1:
        var W = -1;
        break;
      case 2:
        W = 250;
        break;
      case 5:
        W = 1073741823;
        break;
      case 4:
        W = 1e4;
        break;
      default:
        W = 5e3;
    }
    return W = L + W, E = { id: j++, callback: y, priorityLevel: E, startTime: L, expirationTime: W, sortIndex: -1 }, L > B ? (E.sortIndex = L, t(d, E), n(c) === null && E === n(d) && (w ? (p(C), C = -1) : w = !0, ge(k, L - B))) : (E.sortIndex = W, t(c, E), g || h || (g = !0, pe(T))), E;
  }, e.unstable_shouldYield = P, e.unstable_wrapCallback = function(E) {
    var y = m;
    return function() {
      var L = m;
      m = y;
      try {
        return E.apply(this, arguments);
      } finally {
        m = L;
      }
    };
  };
})(Cc);
wc.exports = Cc;
var of = wc.exports;
/**
 * @license React
 * react-dom.production.min.js
 *
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */
var sf = x, at = of;
function I(e) {
  for (var t = "https://reactjs.org/docs/error-decoder.html?invariant=" + e, n = 1; n < arguments.length; n++) t += "&args[]=" + encodeURIComponent(arguments[n]);
  return "Minified React error #" + e + "; visit " + t + " for the full message or use the non-minified dev environment for full errors and additional helpful warnings.";
}
var kc = /* @__PURE__ */ new Set(), Rr = {};
function Pn(e, t) {
  Xn(e, t), Xn(e + "Capture", t);
}
function Xn(e, t) {
  for (Rr[e] = t, e = 0; e < t.length; e++) kc.add(t[e]);
}
var Ut = !(typeof window > "u" || typeof window.document > "u" || typeof window.document.createElement > "u"), Jl = Object.prototype.hasOwnProperty, cf = /^[:A-Z_a-z\u00C0-\u00D6\u00D8-\u00F6\u00F8-\u02FF\u0370-\u037D\u037F-\u1FFF\u200C-\u200D\u2070-\u218F\u2C00-\u2FEF\u3001-\uD7FF\uF900-\uFDCF\uFDF0-\uFFFD][:A-Z_a-z\u00C0-\u00D6\u00D8-\u00F6\u00F8-\u02FF\u0370-\u037D\u037F-\u1FFF\u200C-\u200D\u2070-\u218F\u2C00-\u2FEF\u3001-\uD7FF\uF900-\uFDCF\uFDF0-\uFFFD\-.0-9\u00B7\u0300-\u036F\u203F-\u2040]*$/, ns = {}, rs = {};
function uf(e) {
  return Jl.call(rs, e) ? !0 : Jl.call(ns, e) ? !1 : cf.test(e) ? rs[e] = !0 : (ns[e] = !0, !1);
}
function df(e, t, n, r) {
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
function ff(e, t, n, r) {
  if (t === null || typeof t > "u" || df(e, t, n, r)) return !0;
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
var ze = {};
"children dangerouslySetInnerHTML defaultValue defaultChecked innerHTML suppressContentEditableWarning suppressHydrationWarning style".split(" ").forEach(function(e) {
  ze[e] = new Ke(e, 0, !1, e, null, !1, !1);
});
[["acceptCharset", "accept-charset"], ["className", "class"], ["htmlFor", "for"], ["httpEquiv", "http-equiv"]].forEach(function(e) {
  var t = e[0];
  ze[t] = new Ke(t, 1, !1, e[1], null, !1, !1);
});
["contentEditable", "draggable", "spellCheck", "value"].forEach(function(e) {
  ze[e] = new Ke(e, 2, !1, e.toLowerCase(), null, !1, !1);
});
["autoReverse", "externalResourcesRequired", "focusable", "preserveAlpha"].forEach(function(e) {
  ze[e] = new Ke(e, 2, !1, e, null, !1, !1);
});
"allowFullScreen async autoFocus autoPlay controls default defer disabled disablePictureInPicture disableRemotePlayback formNoValidate hidden loop noModule noValidate open playsInline readOnly required reversed scoped seamless itemScope".split(" ").forEach(function(e) {
  ze[e] = new Ke(e, 3, !1, e.toLowerCase(), null, !1, !1);
});
["checked", "multiple", "muted", "selected"].forEach(function(e) {
  ze[e] = new Ke(e, 3, !0, e, null, !1, !1);
});
["capture", "download"].forEach(function(e) {
  ze[e] = new Ke(e, 4, !1, e, null, !1, !1);
});
["cols", "rows", "size", "span"].forEach(function(e) {
  ze[e] = new Ke(e, 6, !1, e, null, !1, !1);
});
["rowSpan", "start"].forEach(function(e) {
  ze[e] = new Ke(e, 5, !1, e.toLowerCase(), null, !1, !1);
});
var Yi = /[\-:]([a-z])/g;
function Xi(e) {
  return e[1].toUpperCase();
}
"accent-height alignment-baseline arabic-form baseline-shift cap-height clip-path clip-rule color-interpolation color-interpolation-filters color-profile color-rendering dominant-baseline enable-background fill-opacity fill-rule flood-color flood-opacity font-family font-size font-size-adjust font-stretch font-style font-variant font-weight glyph-name glyph-orientation-horizontal glyph-orientation-vertical horiz-adv-x horiz-origin-x image-rendering letter-spacing lighting-color marker-end marker-mid marker-start overline-position overline-thickness paint-order panose-1 pointer-events rendering-intent shape-rendering stop-color stop-opacity strikethrough-position strikethrough-thickness stroke-dasharray stroke-dashoffset stroke-linecap stroke-linejoin stroke-miterlimit stroke-opacity stroke-width text-anchor text-decoration text-rendering underline-position underline-thickness unicode-bidi unicode-range units-per-em v-alphabetic v-hanging v-ideographic v-mathematical vector-effect vert-adv-y vert-origin-x vert-origin-y word-spacing writing-mode xmlns:xlink x-height".split(" ").forEach(function(e) {
  var t = e.replace(
    Yi,
    Xi
  );
  ze[t] = new Ke(t, 1, !1, e, null, !1, !1);
});
"xlink:actuate xlink:arcrole xlink:role xlink:show xlink:title xlink:type".split(" ").forEach(function(e) {
  var t = e.replace(Yi, Xi);
  ze[t] = new Ke(t, 1, !1, e, "http://www.w3.org/1999/xlink", !1, !1);
});
["xml:base", "xml:lang", "xml:space"].forEach(function(e) {
  var t = e.replace(Yi, Xi);
  ze[t] = new Ke(t, 1, !1, e, "http://www.w3.org/XML/1998/namespace", !1, !1);
});
["tabIndex", "crossOrigin"].forEach(function(e) {
  ze[e] = new Ke(e, 1, !1, e.toLowerCase(), null, !1, !1);
});
ze.xlinkHref = new Ke("xlinkHref", 1, !1, "xlink:href", "http://www.w3.org/1999/xlink", !0, !1);
["src", "href", "action", "formAction"].forEach(function(e) {
  ze[e] = new Ke(e, 1, !1, e.toLowerCase(), null, !0, !0);
});
function Zi(e, t, n, r) {
  var a = ze.hasOwnProperty(t) ? ze[t] : null;
  (a !== null ? a.type !== 0 : r || !(2 < t.length) || t[0] !== "o" && t[0] !== "O" || t[1] !== "n" && t[1] !== "N") && (ff(t, n, a, r) && (n = null), r || a === null ? uf(t) && (n === null ? e.removeAttribute(t) : e.setAttribute(t, "" + n)) : a.mustUseProperty ? e[a.propertyName] = n === null ? a.type === 3 ? !1 : "" : n : (t = a.attributeName, r = a.attributeNamespace, n === null ? e.removeAttribute(t) : (a = a.type, n = a === 3 || a === 4 && n === !0 ? "" : "" + n, r ? e.setAttributeNS(r, t, n) : e.setAttribute(t, n))));
}
var Wt = sf.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED, ra = Symbol.for("react.element"), Tn = Symbol.for("react.portal"), Dn = Symbol.for("react.fragment"), Ji = Symbol.for("react.strict_mode"), ei = Symbol.for("react.profiler"), Ec = Symbol.for("react.provider"), Ic = Symbol.for("react.context"), eo = Symbol.for("react.forward_ref"), ti = Symbol.for("react.suspense"), ni = Symbol.for("react.suspense_list"), to = Symbol.for("react.memo"), Gt = Symbol.for("react.lazy"), Pc = Symbol.for("react.offscreen"), as = Symbol.iterator;
function sr(e) {
  return e === null || typeof e != "object" ? null : (e = as && e[as] || e["@@iterator"], typeof e == "function" ? e : null);
}
var fe = Object.assign, kl;
function vr(e) {
  if (kl === void 0) try {
    throw Error();
  } catch (n) {
    var t = n.stack.trim().match(/\n( *(at )?)/);
    kl = t && t[1] || "";
  }
  return `
` + kl + e;
}
var El = !1;
function Il(e, t) {
  if (!e || El) return "";
  El = !0;
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
      for (var a = d.stack.split(`
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
    El = !1, Error.prepareStackTrace = n;
  }
  return (e = e ? e.displayName || e.name : "") ? vr(e) : "";
}
function pf(e) {
  switch (e.tag) {
    case 5:
      return vr(e.type);
    case 16:
      return vr("Lazy");
    case 13:
      return vr("Suspense");
    case 19:
      return vr("SuspenseList");
    case 0:
    case 2:
    case 15:
      return e = Il(e.type, !1), e;
    case 11:
      return e = Il(e.type.render, !1), e;
    case 1:
      return e = Il(e.type, !0), e;
    default:
      return "";
  }
}
function ri(e) {
  if (e == null) return null;
  if (typeof e == "function") return e.displayName || e.name || null;
  if (typeof e == "string") return e;
  switch (e) {
    case Dn:
      return "Fragment";
    case Tn:
      return "Portal";
    case ei:
      return "Profiler";
    case Ji:
      return "StrictMode";
    case ti:
      return "Suspense";
    case ni:
      return "SuspenseList";
  }
  if (typeof e == "object") switch (e.$$typeof) {
    case Ic:
      return (e.displayName || "Context") + ".Consumer";
    case Ec:
      return (e._context.displayName || "Context") + ".Provider";
    case eo:
      var t = e.render;
      return e = e.displayName, e || (e = t.displayName || t.name || "", e = e !== "" ? "ForwardRef(" + e + ")" : "ForwardRef"), e;
    case to:
      return t = e.displayName || null, t !== null ? t : ri(e.type) || "Memo";
    case Gt:
      t = e._payload, e = e._init;
      try {
        return ri(e(t));
      } catch {
      }
  }
  return null;
}
function mf(e) {
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
      return ri(t);
    case 8:
      return t === Ji ? "StrictMode" : "Mode";
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
function sn(e) {
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
function Fc(e) {
  var t = e.type;
  return (e = e.nodeName) && e.toLowerCase() === "input" && (t === "checkbox" || t === "radio");
}
function hf(e) {
  var t = Fc(e) ? "checked" : "value", n = Object.getOwnPropertyDescriptor(e.constructor.prototype, t), r = "" + e[t];
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
function aa(e) {
  e._valueTracker || (e._valueTracker = hf(e));
}
function Rc(e) {
  if (!e) return !1;
  var t = e._valueTracker;
  if (!t) return !0;
  var n = t.getValue(), r = "";
  return e && (r = Fc(e) ? e.checked ? "true" : "false" : e.value), e = r, e !== n ? (t.setValue(e), !0) : !1;
}
function _a(e) {
  if (e = e || (typeof document < "u" ? document : void 0), typeof e > "u") return null;
  try {
    return e.activeElement || e.body;
  } catch {
    return e.body;
  }
}
function ai(e, t) {
  var n = t.checked;
  return fe({}, t, { defaultChecked: void 0, defaultValue: void 0, value: void 0, checked: n ?? e._wrapperState.initialChecked });
}
function ls(e, t) {
  var n = t.defaultValue == null ? "" : t.defaultValue, r = t.checked != null ? t.checked : t.defaultChecked;
  n = sn(t.value != null ? t.value : n), e._wrapperState = { initialChecked: r, initialValue: n, controlled: t.type === "checkbox" || t.type === "radio" ? t.checked != null : t.value != null };
}
function _c(e, t) {
  t = t.checked, t != null && Zi(e, "checked", t, !1);
}
function li(e, t) {
  _c(e, t);
  var n = sn(t.value), r = t.type;
  if (n != null) r === "number" ? (n === 0 && e.value === "" || e.value != n) && (e.value = "" + n) : e.value !== "" + n && (e.value = "" + n);
  else if (r === "submit" || r === "reset") {
    e.removeAttribute("value");
    return;
  }
  t.hasOwnProperty("value") ? ii(e, t.type, n) : t.hasOwnProperty("defaultValue") && ii(e, t.type, sn(t.defaultValue)), t.checked == null && t.defaultChecked != null && (e.defaultChecked = !!t.defaultChecked);
}
function is(e, t, n) {
  if (t.hasOwnProperty("value") || t.hasOwnProperty("defaultValue")) {
    var r = t.type;
    if (!(r !== "submit" && r !== "reset" || t.value !== void 0 && t.value !== null)) return;
    t = "" + e._wrapperState.initialValue, n || t === e.value || (e.value = t), e.defaultValue = t;
  }
  n = e.name, n !== "" && (e.name = ""), e.defaultChecked = !!e._wrapperState.initialChecked, n !== "" && (e.name = n);
}
function ii(e, t, n) {
  (t !== "number" || _a(e.ownerDocument) !== e) && (n == null ? e.defaultValue = "" + e._wrapperState.initialValue : e.defaultValue !== "" + n && (e.defaultValue = "" + n));
}
var xr = Array.isArray;
function Hn(e, t, n, r) {
  if (e = e.options, t) {
    t = {};
    for (var a = 0; a < n.length; a++) t["$" + n[a]] = !0;
    for (n = 0; n < e.length; n++) a = t.hasOwnProperty("$" + e[n].value), e[n].selected !== a && (e[n].selected = a), a && r && (e[n].defaultSelected = !0);
  } else {
    for (n = "" + sn(n), t = null, a = 0; a < e.length; a++) {
      if (e[a].value === n) {
        e[a].selected = !0, r && (e[a].defaultSelected = !0);
        return;
      }
      t !== null || e[a].disabled || (t = e[a]);
    }
    t !== null && (t.selected = !0);
  }
}
function oi(e, t) {
  if (t.dangerouslySetInnerHTML != null) throw Error(I(91));
  return fe({}, t, { value: void 0, defaultValue: void 0, children: "" + e._wrapperState.initialValue });
}
function os(e, t) {
  var n = t.value;
  if (n == null) {
    if (n = t.children, t = t.defaultValue, n != null) {
      if (t != null) throw Error(I(92));
      if (xr(n)) {
        if (1 < n.length) throw Error(I(93));
        n = n[0];
      }
      t = n;
    }
    t == null && (t = ""), n = t;
  }
  e._wrapperState = { initialValue: sn(n) };
}
function Tc(e, t) {
  var n = sn(t.value), r = sn(t.defaultValue);
  n != null && (n = "" + n, n !== e.value && (e.value = n), t.defaultValue == null && e.defaultValue !== n && (e.defaultValue = n)), r != null && (e.defaultValue = "" + r);
}
function ss(e) {
  var t = e.textContent;
  t === e._wrapperState.initialValue && t !== "" && t !== null && (e.value = t);
}
function Dc(e) {
  switch (e) {
    case "svg":
      return "http://www.w3.org/2000/svg";
    case "math":
      return "http://www.w3.org/1998/Math/MathML";
    default:
      return "http://www.w3.org/1999/xhtml";
  }
}
function si(e, t) {
  return e == null || e === "http://www.w3.org/1999/xhtml" ? Dc(t) : e === "http://www.w3.org/2000/svg" && t === "foreignObject" ? "http://www.w3.org/1999/xhtml" : e;
}
var la, zc = function(e) {
  return typeof MSApp < "u" && MSApp.execUnsafeLocalFunction ? function(t, n, r, a) {
    MSApp.execUnsafeLocalFunction(function() {
      return e(t, n, r, a);
    });
  } : e;
}(function(e, t) {
  if (e.namespaceURI !== "http://www.w3.org/2000/svg" || "innerHTML" in e) e.innerHTML = t;
  else {
    for (la = la || document.createElement("div"), la.innerHTML = "<svg>" + t.valueOf().toString() + "</svg>", t = la.firstChild; e.firstChild; ) e.removeChild(e.firstChild);
    for (; t.firstChild; ) e.appendChild(t.firstChild);
  }
});
function _r(e, t) {
  if (t) {
    var n = e.firstChild;
    if (n && n === e.lastChild && n.nodeType === 3) {
      n.nodeValue = t;
      return;
    }
  }
  e.textContent = t;
}
var jr = {
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
}, vf = ["Webkit", "ms", "Moz", "O"];
Object.keys(jr).forEach(function(e) {
  vf.forEach(function(t) {
    t = t + e.charAt(0).toUpperCase() + e.substring(1), jr[t] = jr[e];
  });
});
function Lc(e, t, n) {
  return t == null || typeof t == "boolean" || t === "" ? "" : n || typeof t != "number" || t === 0 || jr.hasOwnProperty(e) && jr[e] ? ("" + t).trim() : t + "px";
}
function Ac(e, t) {
  e = e.style;
  for (var n in t) if (t.hasOwnProperty(n)) {
    var r = n.indexOf("--") === 0, a = Lc(n, t[n], r);
    n === "float" && (n = "cssFloat"), r ? e.setProperty(n, a) : e[n] = a;
  }
}
var xf = fe({ menuitem: !0 }, { area: !0, base: !0, br: !0, col: !0, embed: !0, hr: !0, img: !0, input: !0, keygen: !0, link: !0, meta: !0, param: !0, source: !0, track: !0, wbr: !0 });
function ci(e, t) {
  if (t) {
    if (xf[e] && (t.children != null || t.dangerouslySetInnerHTML != null)) throw Error(I(137, e));
    if (t.dangerouslySetInnerHTML != null) {
      if (t.children != null) throw Error(I(60));
      if (typeof t.dangerouslySetInnerHTML != "object" || !("__html" in t.dangerouslySetInnerHTML)) throw Error(I(61));
    }
    if (t.style != null && typeof t.style != "object") throw Error(I(62));
  }
}
function ui(e, t) {
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
var di = null;
function no(e) {
  return e = e.target || e.srcElement || window, e.correspondingUseElement && (e = e.correspondingUseElement), e.nodeType === 3 ? e.parentNode : e;
}
var fi = null, Wn = null, Qn = null;
function cs(e) {
  if (e = Xr(e)) {
    if (typeof fi != "function") throw Error(I(280));
    var t = e.stateNode;
    t && (t = il(t), fi(e.stateNode, e.type, t));
  }
}
function Mc(e) {
  Wn ? Qn ? Qn.push(e) : Qn = [e] : Wn = e;
}
function $c() {
  if (Wn) {
    var e = Wn, t = Qn;
    if (Qn = Wn = null, cs(e), t) for (e = 0; e < t.length; e++) cs(t[e]);
  }
}
function Oc(e, t) {
  return e(t);
}
function bc() {
}
var Pl = !1;
function Uc(e, t, n) {
  if (Pl) return e(t, n);
  Pl = !0;
  try {
    return Oc(e, t, n);
  } finally {
    Pl = !1, (Wn !== null || Qn !== null) && (bc(), $c());
  }
}
function Tr(e, t) {
  var n = e.stateNode;
  if (n === null) return null;
  var r = il(n);
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
  if (n && typeof n != "function") throw Error(I(231, t, typeof n));
  return n;
}
var pi = !1;
if (Ut) try {
  var cr = {};
  Object.defineProperty(cr, "passive", { get: function() {
    pi = !0;
  } }), window.addEventListener("test", cr, cr), window.removeEventListener("test", cr, cr);
} catch {
  pi = !1;
}
function gf(e, t, n, r, a, i, o, s, c) {
  var d = Array.prototype.slice.call(arguments, 3);
  try {
    t.apply(n, d);
  } catch (j) {
    this.onError(j);
  }
}
var Nr = !1, Ta = null, Da = !1, mi = null, yf = { onError: function(e) {
  Nr = !0, Ta = e;
} };
function jf(e, t, n, r, a, i, o, s, c) {
  Nr = !1, Ta = null, gf.apply(yf, arguments);
}
function Nf(e, t, n, r, a, i, o, s, c) {
  if (jf.apply(this, arguments), Nr) {
    if (Nr) {
      var d = Ta;
      Nr = !1, Ta = null;
    } else throw Error(I(198));
    Da || (Da = !0, mi = d);
  }
}
function Fn(e) {
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
function Vc(e) {
  if (e.tag === 13) {
    var t = e.memoizedState;
    if (t === null && (e = e.alternate, e !== null && (t = e.memoizedState)), t !== null) return t.dehydrated;
  }
  return null;
}
function us(e) {
  if (Fn(e) !== e) throw Error(I(188));
}
function Sf(e) {
  var t = e.alternate;
  if (!t) {
    if (t = Fn(e), t === null) throw Error(I(188));
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
        if (i === n) return us(a), e;
        if (i === r) return us(a), t;
        i = i.sibling;
      }
      throw Error(I(188));
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
        if (!o) throw Error(I(189));
      }
    }
    if (n.alternate !== r) throw Error(I(190));
  }
  if (n.tag !== 3) throw Error(I(188));
  return n.stateNode.current === n ? e : t;
}
function Bc(e) {
  return e = Sf(e), e !== null ? Hc(e) : null;
}
function Hc(e) {
  if (e.tag === 5 || e.tag === 6) return e;
  for (e = e.child; e !== null; ) {
    var t = Hc(e);
    if (t !== null) return t;
    e = e.sibling;
  }
  return null;
}
var Wc = at.unstable_scheduleCallback, ds = at.unstable_cancelCallback, wf = at.unstable_shouldYield, Cf = at.unstable_requestPaint, ye = at.unstable_now, kf = at.unstable_getCurrentPriorityLevel, ro = at.unstable_ImmediatePriority, Qc = at.unstable_UserBlockingPriority, za = at.unstable_NormalPriority, Ef = at.unstable_LowPriority, Gc = at.unstable_IdlePriority, nl = null, _t = null;
function If(e) {
  if (_t && typeof _t.onCommitFiberRoot == "function") try {
    _t.onCommitFiberRoot(nl, e, void 0, (e.current.flags & 128) === 128);
  } catch {
  }
}
var St = Math.clz32 ? Math.clz32 : Rf, Pf = Math.log, Ff = Math.LN2;
function Rf(e) {
  return e >>>= 0, e === 0 ? 32 : 31 - (Pf(e) / Ff | 0) | 0;
}
var ia = 64, oa = 4194304;
function gr(e) {
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
function La(e, t) {
  var n = e.pendingLanes;
  if (n === 0) return 0;
  var r = 0, a = e.suspendedLanes, i = e.pingedLanes, o = n & 268435455;
  if (o !== 0) {
    var s = o & ~a;
    s !== 0 ? r = gr(s) : (i &= o, i !== 0 && (r = gr(i)));
  } else o = n & ~a, o !== 0 ? r = gr(o) : i !== 0 && (r = gr(i));
  if (r === 0) return 0;
  if (t !== 0 && t !== r && !(t & a) && (a = r & -r, i = t & -t, a >= i || a === 16 && (i & 4194240) !== 0)) return t;
  if (r & 4 && (r |= n & 16), t = e.entangledLanes, t !== 0) for (e = e.entanglements, t &= r; 0 < t; ) n = 31 - St(t), a = 1 << n, r |= e[n], t &= ~a;
  return r;
}
function _f(e, t) {
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
function Tf(e, t) {
  for (var n = e.suspendedLanes, r = e.pingedLanes, a = e.expirationTimes, i = e.pendingLanes; 0 < i; ) {
    var o = 31 - St(i), s = 1 << o, c = a[o];
    c === -1 ? (!(s & n) || s & r) && (a[o] = _f(s, t)) : c <= t && (e.expiredLanes |= s), i &= ~s;
  }
}
function hi(e) {
  return e = e.pendingLanes & -1073741825, e !== 0 ? e : e & 1073741824 ? 1073741824 : 0;
}
function Kc() {
  var e = ia;
  return ia <<= 1, !(ia & 4194240) && (ia = 64), e;
}
function Fl(e) {
  for (var t = [], n = 0; 31 > n; n++) t.push(e);
  return t;
}
function qr(e, t, n) {
  e.pendingLanes |= t, t !== 536870912 && (e.suspendedLanes = 0, e.pingedLanes = 0), e = e.eventTimes, t = 31 - St(t), e[t] = n;
}
function Df(e, t) {
  var n = e.pendingLanes & ~t;
  e.pendingLanes = t, e.suspendedLanes = 0, e.pingedLanes = 0, e.expiredLanes &= t, e.mutableReadLanes &= t, e.entangledLanes &= t, t = e.entanglements;
  var r = e.eventTimes;
  for (e = e.expirationTimes; 0 < n; ) {
    var a = 31 - St(n), i = 1 << a;
    t[a] = 0, r[a] = -1, e[a] = -1, n &= ~i;
  }
}
function ao(e, t) {
  var n = e.entangledLanes |= t;
  for (e = e.entanglements; n; ) {
    var r = 31 - St(n), a = 1 << r;
    a & t | e[r] & t && (e[r] |= t), n &= ~a;
  }
}
var ne = 0;
function qc(e) {
  return e &= -e, 1 < e ? 4 < e ? e & 268435455 ? 16 : 536870912 : 4 : 1;
}
var Yc, lo, Xc, Zc, Jc, vi = !1, sa = [], Jt = null, en = null, tn = null, Dr = /* @__PURE__ */ new Map(), zr = /* @__PURE__ */ new Map(), qt = [], zf = "mousedown mouseup touchcancel touchend touchstart auxclick dblclick pointercancel pointerdown pointerup dragend dragstart drop compositionend compositionstart keydown keypress keyup input textInput copy cut paste click change contextmenu reset submit".split(" ");
function fs(e, t) {
  switch (e) {
    case "focusin":
    case "focusout":
      Jt = null;
      break;
    case "dragenter":
    case "dragleave":
      en = null;
      break;
    case "mouseover":
    case "mouseout":
      tn = null;
      break;
    case "pointerover":
    case "pointerout":
      Dr.delete(t.pointerId);
      break;
    case "gotpointercapture":
    case "lostpointercapture":
      zr.delete(t.pointerId);
  }
}
function ur(e, t, n, r, a, i) {
  return e === null || e.nativeEvent !== i ? (e = { blockedOn: t, domEventName: n, eventSystemFlags: r, nativeEvent: i, targetContainers: [a] }, t !== null && (t = Xr(t), t !== null && lo(t)), e) : (e.eventSystemFlags |= r, t = e.targetContainers, a !== null && t.indexOf(a) === -1 && t.push(a), e);
}
function Lf(e, t, n, r, a) {
  switch (t) {
    case "focusin":
      return Jt = ur(Jt, e, t, n, r, a), !0;
    case "dragenter":
      return en = ur(en, e, t, n, r, a), !0;
    case "mouseover":
      return tn = ur(tn, e, t, n, r, a), !0;
    case "pointerover":
      var i = a.pointerId;
      return Dr.set(i, ur(Dr.get(i) || null, e, t, n, r, a)), !0;
    case "gotpointercapture":
      return i = a.pointerId, zr.set(i, ur(zr.get(i) || null, e, t, n, r, a)), !0;
  }
  return !1;
}
function eu(e) {
  var t = xn(e.target);
  if (t !== null) {
    var n = Fn(t);
    if (n !== null) {
      if (t = n.tag, t === 13) {
        if (t = Vc(n), t !== null) {
          e.blockedOn = t, Jc(e.priority, function() {
            Xc(n);
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
function Na(e) {
  if (e.blockedOn !== null) return !1;
  for (var t = e.targetContainers; 0 < t.length; ) {
    var n = xi(e.domEventName, e.eventSystemFlags, t[0], e.nativeEvent);
    if (n === null) {
      n = e.nativeEvent;
      var r = new n.constructor(n.type, n);
      di = r, n.target.dispatchEvent(r), di = null;
    } else return t = Xr(n), t !== null && lo(t), e.blockedOn = n, !1;
    t.shift();
  }
  return !0;
}
function ps(e, t, n) {
  Na(e) && n.delete(t);
}
function Af() {
  vi = !1, Jt !== null && Na(Jt) && (Jt = null), en !== null && Na(en) && (en = null), tn !== null && Na(tn) && (tn = null), Dr.forEach(ps), zr.forEach(ps);
}
function dr(e, t) {
  e.blockedOn === t && (e.blockedOn = null, vi || (vi = !0, at.unstable_scheduleCallback(at.unstable_NormalPriority, Af)));
}
function Lr(e) {
  function t(a) {
    return dr(a, e);
  }
  if (0 < sa.length) {
    dr(sa[0], e);
    for (var n = 1; n < sa.length; n++) {
      var r = sa[n];
      r.blockedOn === e && (r.blockedOn = null);
    }
  }
  for (Jt !== null && dr(Jt, e), en !== null && dr(en, e), tn !== null && dr(tn, e), Dr.forEach(t), zr.forEach(t), n = 0; n < qt.length; n++) r = qt[n], r.blockedOn === e && (r.blockedOn = null);
  for (; 0 < qt.length && (n = qt[0], n.blockedOn === null); ) eu(n), n.blockedOn === null && qt.shift();
}
var Gn = Wt.ReactCurrentBatchConfig, Aa = !0;
function Mf(e, t, n, r) {
  var a = ne, i = Gn.transition;
  Gn.transition = null;
  try {
    ne = 1, io(e, t, n, r);
  } finally {
    ne = a, Gn.transition = i;
  }
}
function $f(e, t, n, r) {
  var a = ne, i = Gn.transition;
  Gn.transition = null;
  try {
    ne = 4, io(e, t, n, r);
  } finally {
    ne = a, Gn.transition = i;
  }
}
function io(e, t, n, r) {
  if (Aa) {
    var a = xi(e, t, n, r);
    if (a === null) Ol(e, t, r, Ma, n), fs(e, r);
    else if (Lf(a, e, t, n, r)) r.stopPropagation();
    else if (fs(e, r), t & 4 && -1 < zf.indexOf(e)) {
      for (; a !== null; ) {
        var i = Xr(a);
        if (i !== null && Yc(i), i = xi(e, t, n, r), i === null && Ol(e, t, r, Ma, n), i === a) break;
        a = i;
      }
      a !== null && r.stopPropagation();
    } else Ol(e, t, r, null, n);
  }
}
var Ma = null;
function xi(e, t, n, r) {
  if (Ma = null, e = no(r), e = xn(e), e !== null) if (t = Fn(e), t === null) e = null;
  else if (n = t.tag, n === 13) {
    if (e = Vc(t), e !== null) return e;
    e = null;
  } else if (n === 3) {
    if (t.stateNode.current.memoizedState.isDehydrated) return t.tag === 3 ? t.stateNode.containerInfo : null;
    e = null;
  } else t !== e && (e = null);
  return Ma = e, null;
}
function tu(e) {
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
      switch (kf()) {
        case ro:
          return 1;
        case Qc:
          return 4;
        case za:
        case Ef:
          return 16;
        case Gc:
          return 536870912;
        default:
          return 16;
      }
    default:
      return 16;
  }
}
var Xt = null, oo = null, Sa = null;
function nu() {
  if (Sa) return Sa;
  var e, t = oo, n = t.length, r, a = "value" in Xt ? Xt.value : Xt.textContent, i = a.length;
  for (e = 0; e < n && t[e] === a[e]; e++) ;
  var o = n - e;
  for (r = 1; r <= o && t[n - r] === a[i - r]; r++) ;
  return Sa = a.slice(e, 1 < r ? 1 - r : void 0);
}
function wa(e) {
  var t = e.keyCode;
  return "charCode" in e ? (e = e.charCode, e === 0 && t === 13 && (e = 13)) : e = t, e === 10 && (e = 13), 32 <= e || e === 13 ? e : 0;
}
function ca() {
  return !0;
}
function ms() {
  return !1;
}
function it(e) {
  function t(n, r, a, i, o) {
    this._reactName = n, this._targetInst = a, this.type = r, this.nativeEvent = i, this.target = o, this.currentTarget = null;
    for (var s in e) e.hasOwnProperty(s) && (n = e[s], this[s] = n ? n(i) : i[s]);
    return this.isDefaultPrevented = (i.defaultPrevented != null ? i.defaultPrevented : i.returnValue === !1) ? ca : ms, this.isPropagationStopped = ms, this;
  }
  return fe(t.prototype, { preventDefault: function() {
    this.defaultPrevented = !0;
    var n = this.nativeEvent;
    n && (n.preventDefault ? n.preventDefault() : typeof n.returnValue != "unknown" && (n.returnValue = !1), this.isDefaultPrevented = ca);
  }, stopPropagation: function() {
    var n = this.nativeEvent;
    n && (n.stopPropagation ? n.stopPropagation() : typeof n.cancelBubble != "unknown" && (n.cancelBubble = !0), this.isPropagationStopped = ca);
  }, persist: function() {
  }, isPersistent: ca }), t;
}
var lr = { eventPhase: 0, bubbles: 0, cancelable: 0, timeStamp: function(e) {
  return e.timeStamp || Date.now();
}, defaultPrevented: 0, isTrusted: 0 }, so = it(lr), Yr = fe({}, lr, { view: 0, detail: 0 }), Of = it(Yr), Rl, _l, fr, rl = fe({}, Yr, { screenX: 0, screenY: 0, clientX: 0, clientY: 0, pageX: 0, pageY: 0, ctrlKey: 0, shiftKey: 0, altKey: 0, metaKey: 0, getModifierState: co, button: 0, buttons: 0, relatedTarget: function(e) {
  return e.relatedTarget === void 0 ? e.fromElement === e.srcElement ? e.toElement : e.fromElement : e.relatedTarget;
}, movementX: function(e) {
  return "movementX" in e ? e.movementX : (e !== fr && (fr && e.type === "mousemove" ? (Rl = e.screenX - fr.screenX, _l = e.screenY - fr.screenY) : _l = Rl = 0, fr = e), Rl);
}, movementY: function(e) {
  return "movementY" in e ? e.movementY : _l;
} }), hs = it(rl), bf = fe({}, rl, { dataTransfer: 0 }), Uf = it(bf), Vf = fe({}, Yr, { relatedTarget: 0 }), Tl = it(Vf), Bf = fe({}, lr, { animationName: 0, elapsedTime: 0, pseudoElement: 0 }), Hf = it(Bf), Wf = fe({}, lr, { clipboardData: function(e) {
  return "clipboardData" in e ? e.clipboardData : window.clipboardData;
} }), Qf = it(Wf), Gf = fe({}, lr, { data: 0 }), vs = it(Gf), Kf = {
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
}, qf = {
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
}, Yf = { Alt: "altKey", Control: "ctrlKey", Meta: "metaKey", Shift: "shiftKey" };
function Xf(e) {
  var t = this.nativeEvent;
  return t.getModifierState ? t.getModifierState(e) : (e = Yf[e]) ? !!t[e] : !1;
}
function co() {
  return Xf;
}
var Zf = fe({}, Yr, { key: function(e) {
  if (e.key) {
    var t = Kf[e.key] || e.key;
    if (t !== "Unidentified") return t;
  }
  return e.type === "keypress" ? (e = wa(e), e === 13 ? "Enter" : String.fromCharCode(e)) : e.type === "keydown" || e.type === "keyup" ? qf[e.keyCode] || "Unidentified" : "";
}, code: 0, location: 0, ctrlKey: 0, shiftKey: 0, altKey: 0, metaKey: 0, repeat: 0, locale: 0, getModifierState: co, charCode: function(e) {
  return e.type === "keypress" ? wa(e) : 0;
}, keyCode: function(e) {
  return e.type === "keydown" || e.type === "keyup" ? e.keyCode : 0;
}, which: function(e) {
  return e.type === "keypress" ? wa(e) : e.type === "keydown" || e.type === "keyup" ? e.keyCode : 0;
} }), Jf = it(Zf), ep = fe({}, rl, { pointerId: 0, width: 0, height: 0, pressure: 0, tangentialPressure: 0, tiltX: 0, tiltY: 0, twist: 0, pointerType: 0, isPrimary: 0 }), xs = it(ep), tp = fe({}, Yr, { touches: 0, targetTouches: 0, changedTouches: 0, altKey: 0, metaKey: 0, ctrlKey: 0, shiftKey: 0, getModifierState: co }), np = it(tp), rp = fe({}, lr, { propertyName: 0, elapsedTime: 0, pseudoElement: 0 }), ap = it(rp), lp = fe({}, rl, {
  deltaX: function(e) {
    return "deltaX" in e ? e.deltaX : "wheelDeltaX" in e ? -e.wheelDeltaX : 0;
  },
  deltaY: function(e) {
    return "deltaY" in e ? e.deltaY : "wheelDeltaY" in e ? -e.wheelDeltaY : "wheelDelta" in e ? -e.wheelDelta : 0;
  },
  deltaZ: 0,
  deltaMode: 0
}), ip = it(lp), op = [9, 13, 27, 32], uo = Ut && "CompositionEvent" in window, Sr = null;
Ut && "documentMode" in document && (Sr = document.documentMode);
var sp = Ut && "TextEvent" in window && !Sr, ru = Ut && (!uo || Sr && 8 < Sr && 11 >= Sr), gs = " ", ys = !1;
function au(e, t) {
  switch (e) {
    case "keyup":
      return op.indexOf(t.keyCode) !== -1;
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
function lu(e) {
  return e = e.detail, typeof e == "object" && "data" in e ? e.data : null;
}
var zn = !1;
function cp(e, t) {
  switch (e) {
    case "compositionend":
      return lu(t);
    case "keypress":
      return t.which !== 32 ? null : (ys = !0, gs);
    case "textInput":
      return e = t.data, e === gs && ys ? null : e;
    default:
      return null;
  }
}
function up(e, t) {
  if (zn) return e === "compositionend" || !uo && au(e, t) ? (e = nu(), Sa = oo = Xt = null, zn = !1, e) : null;
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
      return ru && t.locale !== "ko" ? null : t.data;
    default:
      return null;
  }
}
var dp = { color: !0, date: !0, datetime: !0, "datetime-local": !0, email: !0, month: !0, number: !0, password: !0, range: !0, search: !0, tel: !0, text: !0, time: !0, url: !0, week: !0 };
function js(e) {
  var t = e && e.nodeName && e.nodeName.toLowerCase();
  return t === "input" ? !!dp[e.type] : t === "textarea";
}
function iu(e, t, n, r) {
  Mc(r), t = $a(t, "onChange"), 0 < t.length && (n = new so("onChange", "change", null, n, r), e.push({ event: n, listeners: t }));
}
var wr = null, Ar = null;
function fp(e) {
  xu(e, 0);
}
function al(e) {
  var t = Mn(e);
  if (Rc(t)) return e;
}
function pp(e, t) {
  if (e === "change") return t;
}
var ou = !1;
if (Ut) {
  var Dl;
  if (Ut) {
    var zl = "oninput" in document;
    if (!zl) {
      var Ns = document.createElement("div");
      Ns.setAttribute("oninput", "return;"), zl = typeof Ns.oninput == "function";
    }
    Dl = zl;
  } else Dl = !1;
  ou = Dl && (!document.documentMode || 9 < document.documentMode);
}
function Ss() {
  wr && (wr.detachEvent("onpropertychange", su), Ar = wr = null);
}
function su(e) {
  if (e.propertyName === "value" && al(Ar)) {
    var t = [];
    iu(t, Ar, e, no(e)), Uc(fp, t);
  }
}
function mp(e, t, n) {
  e === "focusin" ? (Ss(), wr = t, Ar = n, wr.attachEvent("onpropertychange", su)) : e === "focusout" && Ss();
}
function hp(e) {
  if (e === "selectionchange" || e === "keyup" || e === "keydown") return al(Ar);
}
function vp(e, t) {
  if (e === "click") return al(t);
}
function xp(e, t) {
  if (e === "input" || e === "change") return al(t);
}
function gp(e, t) {
  return e === t && (e !== 0 || 1 / e === 1 / t) || e !== e && t !== t;
}
var kt = typeof Object.is == "function" ? Object.is : gp;
function Mr(e, t) {
  if (kt(e, t)) return !0;
  if (typeof e != "object" || e === null || typeof t != "object" || t === null) return !1;
  var n = Object.keys(e), r = Object.keys(t);
  if (n.length !== r.length) return !1;
  for (r = 0; r < n.length; r++) {
    var a = n[r];
    if (!Jl.call(t, a) || !kt(e[a], t[a])) return !1;
  }
  return !0;
}
function ws(e) {
  for (; e && e.firstChild; ) e = e.firstChild;
  return e;
}
function Cs(e, t) {
  var n = ws(e);
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
    n = ws(n);
  }
}
function cu(e, t) {
  return e && t ? e === t ? !0 : e && e.nodeType === 3 ? !1 : t && t.nodeType === 3 ? cu(e, t.parentNode) : "contains" in e ? e.contains(t) : e.compareDocumentPosition ? !!(e.compareDocumentPosition(t) & 16) : !1 : !1;
}
function uu() {
  for (var e = window, t = _a(); t instanceof e.HTMLIFrameElement; ) {
    try {
      var n = typeof t.contentWindow.location.href == "string";
    } catch {
      n = !1;
    }
    if (n) e = t.contentWindow;
    else break;
    t = _a(e.document);
  }
  return t;
}
function fo(e) {
  var t = e && e.nodeName && e.nodeName.toLowerCase();
  return t && (t === "input" && (e.type === "text" || e.type === "search" || e.type === "tel" || e.type === "url" || e.type === "password") || t === "textarea" || e.contentEditable === "true");
}
function yp(e) {
  var t = uu(), n = e.focusedElem, r = e.selectionRange;
  if (t !== n && n && n.ownerDocument && cu(n.ownerDocument.documentElement, n)) {
    if (r !== null && fo(n)) {
      if (t = r.start, e = r.end, e === void 0 && (e = t), "selectionStart" in n) n.selectionStart = t, n.selectionEnd = Math.min(e, n.value.length);
      else if (e = (t = n.ownerDocument || document) && t.defaultView || window, e.getSelection) {
        e = e.getSelection();
        var a = n.textContent.length, i = Math.min(r.start, a);
        r = r.end === void 0 ? i : Math.min(r.end, a), !e.extend && i > r && (a = r, r = i, i = a), a = Cs(n, i);
        var o = Cs(
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
var jp = Ut && "documentMode" in document && 11 >= document.documentMode, Ln = null, gi = null, Cr = null, yi = !1;
function ks(e, t, n) {
  var r = n.window === n ? n.document : n.nodeType === 9 ? n : n.ownerDocument;
  yi || Ln == null || Ln !== _a(r) || (r = Ln, "selectionStart" in r && fo(r) ? r = { start: r.selectionStart, end: r.selectionEnd } : (r = (r.ownerDocument && r.ownerDocument.defaultView || window).getSelection(), r = { anchorNode: r.anchorNode, anchorOffset: r.anchorOffset, focusNode: r.focusNode, focusOffset: r.focusOffset }), Cr && Mr(Cr, r) || (Cr = r, r = $a(gi, "onSelect"), 0 < r.length && (t = new so("onSelect", "select", null, t, n), e.push({ event: t, listeners: r }), t.target = Ln)));
}
function ua(e, t) {
  var n = {};
  return n[e.toLowerCase()] = t.toLowerCase(), n["Webkit" + e] = "webkit" + t, n["Moz" + e] = "moz" + t, n;
}
var An = { animationend: ua("Animation", "AnimationEnd"), animationiteration: ua("Animation", "AnimationIteration"), animationstart: ua("Animation", "AnimationStart"), transitionend: ua("Transition", "TransitionEnd") }, Ll = {}, du = {};
Ut && (du = document.createElement("div").style, "AnimationEvent" in window || (delete An.animationend.animation, delete An.animationiteration.animation, delete An.animationstart.animation), "TransitionEvent" in window || delete An.transitionend.transition);
function ll(e) {
  if (Ll[e]) return Ll[e];
  if (!An[e]) return e;
  var t = An[e], n;
  for (n in t) if (t.hasOwnProperty(n) && n in du) return Ll[e] = t[n];
  return e;
}
var fu = ll("animationend"), pu = ll("animationiteration"), mu = ll("animationstart"), hu = ll("transitionend"), vu = /* @__PURE__ */ new Map(), Es = "abort auxClick cancel canPlay canPlayThrough click close contextMenu copy cut drag dragEnd dragEnter dragExit dragLeave dragOver dragStart drop durationChange emptied encrypted ended error gotPointerCapture input invalid keyDown keyPress keyUp load loadedData loadedMetadata loadStart lostPointerCapture mouseDown mouseMove mouseOut mouseOver mouseUp paste pause play playing pointerCancel pointerDown pointerMove pointerOut pointerOver pointerUp progress rateChange reset resize seeked seeking stalled submit suspend timeUpdate touchCancel touchEnd touchStart volumeChange scroll toggle touchMove waiting wheel".split(" ");
function dn(e, t) {
  vu.set(e, t), Pn(t, [e]);
}
for (var Al = 0; Al < Es.length; Al++) {
  var Ml = Es[Al], Np = Ml.toLowerCase(), Sp = Ml[0].toUpperCase() + Ml.slice(1);
  dn(Np, "on" + Sp);
}
dn(fu, "onAnimationEnd");
dn(pu, "onAnimationIteration");
dn(mu, "onAnimationStart");
dn("dblclick", "onDoubleClick");
dn("focusin", "onFocus");
dn("focusout", "onBlur");
dn(hu, "onTransitionEnd");
Xn("onMouseEnter", ["mouseout", "mouseover"]);
Xn("onMouseLeave", ["mouseout", "mouseover"]);
Xn("onPointerEnter", ["pointerout", "pointerover"]);
Xn("onPointerLeave", ["pointerout", "pointerover"]);
Pn("onChange", "change click focusin focusout input keydown keyup selectionchange".split(" "));
Pn("onSelect", "focusout contextmenu dragend focusin keydown keyup mousedown mouseup selectionchange".split(" "));
Pn("onBeforeInput", ["compositionend", "keypress", "textInput", "paste"]);
Pn("onCompositionEnd", "compositionend focusout keydown keypress keyup mousedown".split(" "));
Pn("onCompositionStart", "compositionstart focusout keydown keypress keyup mousedown".split(" "));
Pn("onCompositionUpdate", "compositionupdate focusout keydown keypress keyup mousedown".split(" "));
var yr = "abort canplay canplaythrough durationchange emptied encrypted ended error loadeddata loadedmetadata loadstart pause play playing progress ratechange resize seeked seeking stalled suspend timeupdate volumechange waiting".split(" "), wp = new Set("cancel close invalid load scroll toggle".split(" ").concat(yr));
function Is(e, t, n) {
  var r = e.type || "unknown-event";
  e.currentTarget = n, Nf(r, t, void 0, e), e.currentTarget = null;
}
function xu(e, t) {
  t = (t & 4) !== 0;
  for (var n = 0; n < e.length; n++) {
    var r = e[n], a = r.event;
    r = r.listeners;
    e: {
      var i = void 0;
      if (t) for (var o = r.length - 1; 0 <= o; o--) {
        var s = r[o], c = s.instance, d = s.currentTarget;
        if (s = s.listener, c !== i && a.isPropagationStopped()) break e;
        Is(a, s, d), i = c;
      }
      else for (o = 0; o < r.length; o++) {
        if (s = r[o], c = s.instance, d = s.currentTarget, s = s.listener, c !== i && a.isPropagationStopped()) break e;
        Is(a, s, d), i = c;
      }
    }
  }
  if (Da) throw e = mi, Da = !1, mi = null, e;
}
function ie(e, t) {
  var n = t[Ci];
  n === void 0 && (n = t[Ci] = /* @__PURE__ */ new Set());
  var r = e + "__bubble";
  n.has(r) || (gu(t, e, 2, !1), n.add(r));
}
function $l(e, t, n) {
  var r = 0;
  t && (r |= 4), gu(n, e, r, t);
}
var da = "_reactListening" + Math.random().toString(36).slice(2);
function $r(e) {
  if (!e[da]) {
    e[da] = !0, kc.forEach(function(n) {
      n !== "selectionchange" && (wp.has(n) || $l(n, !1, e), $l(n, !0, e));
    });
    var t = e.nodeType === 9 ? e : e.ownerDocument;
    t === null || t[da] || (t[da] = !0, $l("selectionchange", !1, t));
  }
}
function gu(e, t, n, r) {
  switch (tu(t)) {
    case 1:
      var a = Mf;
      break;
    case 4:
      a = $f;
      break;
    default:
      a = io;
  }
  n = a.bind(null, t, n, e), a = void 0, !pi || t !== "touchstart" && t !== "touchmove" && t !== "wheel" || (a = !0), r ? a !== void 0 ? e.addEventListener(t, n, { capture: !0, passive: a }) : e.addEventListener(t, n, !0) : a !== void 0 ? e.addEventListener(t, n, { passive: a }) : e.addEventListener(t, n, !1);
}
function Ol(e, t, n, r, a) {
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
        if (o = xn(s), o === null) return;
        if (c = o.tag, c === 5 || c === 6) {
          r = i = o;
          continue e;
        }
        s = s.parentNode;
      }
    }
    r = r.return;
  }
  Uc(function() {
    var d = i, j = no(n), u = [];
    e: {
      var m = vu.get(e);
      if (m !== void 0) {
        var h = so, g = e;
        switch (e) {
          case "keypress":
            if (wa(n) === 0) break e;
          case "keydown":
          case "keyup":
            h = Jf;
            break;
          case "focusin":
            g = "focus", h = Tl;
            break;
          case "focusout":
            g = "blur", h = Tl;
            break;
          case "beforeblur":
          case "afterblur":
            h = Tl;
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
            h = hs;
            break;
          case "drag":
          case "dragend":
          case "dragenter":
          case "dragexit":
          case "dragleave":
          case "dragover":
          case "dragstart":
          case "drop":
            h = Uf;
            break;
          case "touchcancel":
          case "touchend":
          case "touchmove":
          case "touchstart":
            h = np;
            break;
          case fu:
          case pu:
          case mu:
            h = Hf;
            break;
          case hu:
            h = ap;
            break;
          case "scroll":
            h = Of;
            break;
          case "wheel":
            h = ip;
            break;
          case "copy":
          case "cut":
          case "paste":
            h = Qf;
            break;
          case "gotpointercapture":
          case "lostpointercapture":
          case "pointercancel":
          case "pointerdown":
          case "pointermove":
          case "pointerout":
          case "pointerover":
          case "pointerup":
            h = xs;
        }
        var w = (t & 4) !== 0, $ = !w && e === "scroll", p = w ? m !== null ? m + "Capture" : null : m;
        w = [];
        for (var f = d, v; f !== null; ) {
          v = f;
          var k = v.stateNode;
          if (v.tag === 5 && k !== null && (v = k, p !== null && (k = Tr(f, p), k != null && w.push(Or(f, k, v)))), $) break;
          f = f.return;
        }
        0 < w.length && (m = new h(m, g, null, n, j), u.push({ event: m, listeners: w }));
      }
    }
    if (!(t & 7)) {
      e: {
        if (m = e === "mouseover" || e === "pointerover", h = e === "mouseout" || e === "pointerout", m && n !== di && (g = n.relatedTarget || n.fromElement) && (xn(g) || g[Vt])) break e;
        if ((h || m) && (m = j.window === j ? j : (m = j.ownerDocument) ? m.defaultView || m.parentWindow : window, h ? (g = n.relatedTarget || n.toElement, h = d, g = g ? xn(g) : null, g !== null && ($ = Fn(g), g !== $ || g.tag !== 5 && g.tag !== 6) && (g = null)) : (h = null, g = d), h !== g)) {
          if (w = hs, k = "onMouseLeave", p = "onMouseEnter", f = "mouse", (e === "pointerout" || e === "pointerover") && (w = xs, k = "onPointerLeave", p = "onPointerEnter", f = "pointer"), $ = h == null ? m : Mn(h), v = g == null ? m : Mn(g), m = new w(k, f + "leave", h, n, j), m.target = $, m.relatedTarget = v, k = null, xn(j) === d && (w = new w(p, f + "enter", g, n, j), w.target = v, w.relatedTarget = $, k = w), $ = k, h && g) t: {
            for (w = h, p = g, f = 0, v = w; v; v = _n(v)) f++;
            for (v = 0, k = p; k; k = _n(k)) v++;
            for (; 0 < f - v; ) w = _n(w), f--;
            for (; 0 < v - f; ) p = _n(p), v--;
            for (; f--; ) {
              if (w === p || p !== null && w === p.alternate) break t;
              w = _n(w), p = _n(p);
            }
            w = null;
          }
          else w = null;
          h !== null && Ps(u, m, h, w, !1), g !== null && $ !== null && Ps(u, $, g, w, !0);
        }
      }
      e: {
        if (m = d ? Mn(d) : window, h = m.nodeName && m.nodeName.toLowerCase(), h === "select" || h === "input" && m.type === "file") var T = pp;
        else if (js(m)) if (ou) T = xp;
        else {
          T = hp;
          var F = mp;
        }
        else (h = m.nodeName) && h.toLowerCase() === "input" && (m.type === "checkbox" || m.type === "radio") && (T = vp);
        if (T && (T = T(e, d))) {
          iu(u, T, n, j);
          break e;
        }
        F && F(e, m, d), e === "focusout" && (F = m._wrapperState) && F.controlled && m.type === "number" && ii(m, "number", m.value);
      }
      switch (F = d ? Mn(d) : window, e) {
        case "focusin":
          (js(F) || F.contentEditable === "true") && (Ln = F, gi = d, Cr = null);
          break;
        case "focusout":
          Cr = gi = Ln = null;
          break;
        case "mousedown":
          yi = !0;
          break;
        case "contextmenu":
        case "mouseup":
        case "dragend":
          yi = !1, ks(u, n, j);
          break;
        case "selectionchange":
          if (jp) break;
        case "keydown":
        case "keyup":
          ks(u, n, j);
      }
      var D;
      if (uo) e: {
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
      else zn ? au(e, n) && (C = "onCompositionEnd") : e === "keydown" && n.keyCode === 229 && (C = "onCompositionStart");
      C && (ru && n.locale !== "ko" && (zn || C !== "onCompositionStart" ? C === "onCompositionEnd" && zn && (D = nu()) : (Xt = j, oo = "value" in Xt ? Xt.value : Xt.textContent, zn = !0)), F = $a(d, C), 0 < F.length && (C = new vs(C, e, null, n, j), u.push({ event: C, listeners: F }), D ? C.data = D : (D = lu(n), D !== null && (C.data = D)))), (D = sp ? cp(e, n) : up(e, n)) && (d = $a(d, "onBeforeInput"), 0 < d.length && (j = new vs("onBeforeInput", "beforeinput", null, n, j), u.push({ event: j, listeners: d }), j.data = D));
    }
    xu(u, t);
  });
}
function Or(e, t, n) {
  return { instance: e, listener: t, currentTarget: n };
}
function $a(e, t) {
  for (var n = t + "Capture", r = []; e !== null; ) {
    var a = e, i = a.stateNode;
    a.tag === 5 && i !== null && (a = i, i = Tr(e, n), i != null && r.unshift(Or(e, i, a)), i = Tr(e, t), i != null && r.push(Or(e, i, a))), e = e.return;
  }
  return r;
}
function _n(e) {
  if (e === null) return null;
  do
    e = e.return;
  while (e && e.tag !== 5);
  return e || null;
}
function Ps(e, t, n, r, a) {
  for (var i = t._reactName, o = []; n !== null && n !== r; ) {
    var s = n, c = s.alternate, d = s.stateNode;
    if (c !== null && c === r) break;
    s.tag === 5 && d !== null && (s = d, a ? (c = Tr(n, i), c != null && o.unshift(Or(n, c, s))) : a || (c = Tr(n, i), c != null && o.push(Or(n, c, s)))), n = n.return;
  }
  o.length !== 0 && e.push({ event: t, listeners: o });
}
var Cp = /\r\n?/g, kp = /\u0000|\uFFFD/g;
function Fs(e) {
  return (typeof e == "string" ? e : "" + e).replace(Cp, `
`).replace(kp, "");
}
function fa(e, t, n) {
  if (t = Fs(t), Fs(e) !== t && n) throw Error(I(425));
}
function Oa() {
}
var ji = null, Ni = null;
function Si(e, t) {
  return e === "textarea" || e === "noscript" || typeof t.children == "string" || typeof t.children == "number" || typeof t.dangerouslySetInnerHTML == "object" && t.dangerouslySetInnerHTML !== null && t.dangerouslySetInnerHTML.__html != null;
}
var wi = typeof setTimeout == "function" ? setTimeout : void 0, Ep = typeof clearTimeout == "function" ? clearTimeout : void 0, Rs = typeof Promise == "function" ? Promise : void 0, Ip = typeof queueMicrotask == "function" ? queueMicrotask : typeof Rs < "u" ? function(e) {
  return Rs.resolve(null).then(e).catch(Pp);
} : wi;
function Pp(e) {
  setTimeout(function() {
    throw e;
  });
}
function bl(e, t) {
  var n = t, r = 0;
  do {
    var a = n.nextSibling;
    if (e.removeChild(n), a && a.nodeType === 8) if (n = a.data, n === "/$") {
      if (r === 0) {
        e.removeChild(a), Lr(t);
        return;
      }
      r--;
    } else n !== "$" && n !== "$?" && n !== "$!" || r++;
    n = a;
  } while (n);
  Lr(t);
}
function nn(e) {
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
function _s(e) {
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
var ir = Math.random().toString(36).slice(2), Rt = "__reactFiber$" + ir, br = "__reactProps$" + ir, Vt = "__reactContainer$" + ir, Ci = "__reactEvents$" + ir, Fp = "__reactListeners$" + ir, Rp = "__reactHandles$" + ir;
function xn(e) {
  var t = e[Rt];
  if (t) return t;
  for (var n = e.parentNode; n; ) {
    if (t = n[Vt] || n[Rt]) {
      if (n = t.alternate, t.child !== null || n !== null && n.child !== null) for (e = _s(e); e !== null; ) {
        if (n = e[Rt]) return n;
        e = _s(e);
      }
      return t;
    }
    e = n, n = e.parentNode;
  }
  return null;
}
function Xr(e) {
  return e = e[Rt] || e[Vt], !e || e.tag !== 5 && e.tag !== 6 && e.tag !== 13 && e.tag !== 3 ? null : e;
}
function Mn(e) {
  if (e.tag === 5 || e.tag === 6) return e.stateNode;
  throw Error(I(33));
}
function il(e) {
  return e[br] || null;
}
var ki = [], $n = -1;
function fn(e) {
  return { current: e };
}
function oe(e) {
  0 > $n || (e.current = ki[$n], ki[$n] = null, $n--);
}
function ae(e, t) {
  $n++, ki[$n] = e.current, e.current = t;
}
var cn = {}, Ue = fn(cn), Xe = fn(!1), wn = cn;
function Zn(e, t) {
  var n = e.type.contextTypes;
  if (!n) return cn;
  var r = e.stateNode;
  if (r && r.__reactInternalMemoizedUnmaskedChildContext === t) return r.__reactInternalMemoizedMaskedChildContext;
  var a = {}, i;
  for (i in n) a[i] = t[i];
  return r && (e = e.stateNode, e.__reactInternalMemoizedUnmaskedChildContext = t, e.__reactInternalMemoizedMaskedChildContext = a), a;
}
function Ze(e) {
  return e = e.childContextTypes, e != null;
}
function ba() {
  oe(Xe), oe(Ue);
}
function Ts(e, t, n) {
  if (Ue.current !== cn) throw Error(I(168));
  ae(Ue, t), ae(Xe, n);
}
function yu(e, t, n) {
  var r = e.stateNode;
  if (t = t.childContextTypes, typeof r.getChildContext != "function") return n;
  r = r.getChildContext();
  for (var a in r) if (!(a in t)) throw Error(I(108, mf(e) || "Unknown", a));
  return fe({}, n, r);
}
function Ua(e) {
  return e = (e = e.stateNode) && e.__reactInternalMemoizedMergedChildContext || cn, wn = Ue.current, ae(Ue, e), ae(Xe, Xe.current), !0;
}
function Ds(e, t, n) {
  var r = e.stateNode;
  if (!r) throw Error(I(169));
  n ? (e = yu(e, t, wn), r.__reactInternalMemoizedMergedChildContext = e, oe(Xe), oe(Ue), ae(Ue, e)) : oe(Xe), ae(Xe, n);
}
var Mt = null, ol = !1, Ul = !1;
function ju(e) {
  Mt === null ? Mt = [e] : Mt.push(e);
}
function _p(e) {
  ol = !0, ju(e);
}
function pn() {
  if (!Ul && Mt !== null) {
    Ul = !0;
    var e = 0, t = ne;
    try {
      var n = Mt;
      for (ne = 1; e < n.length; e++) {
        var r = n[e];
        do
          r = r(!0);
        while (r !== null);
      }
      Mt = null, ol = !1;
    } catch (a) {
      throw Mt !== null && (Mt = Mt.slice(e + 1)), Wc(ro, pn), a;
    } finally {
      ne = t, Ul = !1;
    }
  }
  return null;
}
var On = [], bn = 0, Va = null, Ba = 0, ut = [], dt = 0, Cn = null, $t = 1, Ot = "";
function hn(e, t) {
  On[bn++] = Ba, On[bn++] = Va, Va = e, Ba = t;
}
function Nu(e, t, n) {
  ut[dt++] = $t, ut[dt++] = Ot, ut[dt++] = Cn, Cn = e;
  var r = $t;
  e = Ot;
  var a = 32 - St(r) - 1;
  r &= ~(1 << a), n += 1;
  var i = 32 - St(t) + a;
  if (30 < i) {
    var o = a - a % 5;
    i = (r & (1 << o) - 1).toString(32), r >>= o, a -= o, $t = 1 << 32 - St(t) + a | n << a | r, Ot = i + e;
  } else $t = 1 << i | n << a | r, Ot = e;
}
function po(e) {
  e.return !== null && (hn(e, 1), Nu(e, 1, 0));
}
function mo(e) {
  for (; e === Va; ) Va = On[--bn], On[bn] = null, Ba = On[--bn], On[bn] = null;
  for (; e === Cn; ) Cn = ut[--dt], ut[dt] = null, Ot = ut[--dt], ut[dt] = null, $t = ut[--dt], ut[dt] = null;
}
var rt = null, nt = null, se = !1, Nt = null;
function Su(e, t) {
  var n = pt(5, null, null, 0);
  n.elementType = "DELETED", n.stateNode = t, n.return = e, t = e.deletions, t === null ? (e.deletions = [n], e.flags |= 16) : t.push(n);
}
function zs(e, t) {
  switch (e.tag) {
    case 5:
      var n = e.type;
      return t = t.nodeType !== 1 || n.toLowerCase() !== t.nodeName.toLowerCase() ? null : t, t !== null ? (e.stateNode = t, rt = e, nt = nn(t.firstChild), !0) : !1;
    case 6:
      return t = e.pendingProps === "" || t.nodeType !== 3 ? null : t, t !== null ? (e.stateNode = t, rt = e, nt = null, !0) : !1;
    case 13:
      return t = t.nodeType !== 8 ? null : t, t !== null ? (n = Cn !== null ? { id: $t, overflow: Ot } : null, e.memoizedState = { dehydrated: t, treeContext: n, retryLane: 1073741824 }, n = pt(18, null, null, 0), n.stateNode = t, n.return = e, e.child = n, rt = e, nt = null, !0) : !1;
    default:
      return !1;
  }
}
function Ei(e) {
  return (e.mode & 1) !== 0 && (e.flags & 128) === 0;
}
function Ii(e) {
  if (se) {
    var t = nt;
    if (t) {
      var n = t;
      if (!zs(e, t)) {
        if (Ei(e)) throw Error(I(418));
        t = nn(n.nextSibling);
        var r = rt;
        t && zs(e, t) ? Su(r, n) : (e.flags = e.flags & -4097 | 2, se = !1, rt = e);
      }
    } else {
      if (Ei(e)) throw Error(I(418));
      e.flags = e.flags & -4097 | 2, se = !1, rt = e;
    }
  }
}
function Ls(e) {
  for (e = e.return; e !== null && e.tag !== 5 && e.tag !== 3 && e.tag !== 13; ) e = e.return;
  rt = e;
}
function pa(e) {
  if (e !== rt) return !1;
  if (!se) return Ls(e), se = !0, !1;
  var t;
  if ((t = e.tag !== 3) && !(t = e.tag !== 5) && (t = e.type, t = t !== "head" && t !== "body" && !Si(e.type, e.memoizedProps)), t && (t = nt)) {
    if (Ei(e)) throw wu(), Error(I(418));
    for (; t; ) Su(e, t), t = nn(t.nextSibling);
  }
  if (Ls(e), e.tag === 13) {
    if (e = e.memoizedState, e = e !== null ? e.dehydrated : null, !e) throw Error(I(317));
    e: {
      for (e = e.nextSibling, t = 0; e; ) {
        if (e.nodeType === 8) {
          var n = e.data;
          if (n === "/$") {
            if (t === 0) {
              nt = nn(e.nextSibling);
              break e;
            }
            t--;
          } else n !== "$" && n !== "$!" && n !== "$?" || t++;
        }
        e = e.nextSibling;
      }
      nt = null;
    }
  } else nt = rt ? nn(e.stateNode.nextSibling) : null;
  return !0;
}
function wu() {
  for (var e = nt; e; ) e = nn(e.nextSibling);
}
function Jn() {
  nt = rt = null, se = !1;
}
function ho(e) {
  Nt === null ? Nt = [e] : Nt.push(e);
}
var Tp = Wt.ReactCurrentBatchConfig;
function pr(e, t, n) {
  if (e = n.ref, e !== null && typeof e != "function" && typeof e != "object") {
    if (n._owner) {
      if (n = n._owner, n) {
        if (n.tag !== 1) throw Error(I(309));
        var r = n.stateNode;
      }
      if (!r) throw Error(I(147, e));
      var a = r, i = "" + e;
      return t !== null && t.ref !== null && typeof t.ref == "function" && t.ref._stringRef === i ? t.ref : (t = function(o) {
        var s = a.refs;
        o === null ? delete s[i] : s[i] = o;
      }, t._stringRef = i, t);
    }
    if (typeof e != "string") throw Error(I(284));
    if (!n._owner) throw Error(I(290, e));
  }
  return e;
}
function ma(e, t) {
  throw e = Object.prototype.toString.call(t), Error(I(31, e === "[object Object]" ? "object with keys {" + Object.keys(t).join(", ") + "}" : e));
}
function As(e) {
  var t = e._init;
  return t(e._payload);
}
function Cu(e) {
  function t(p, f) {
    if (e) {
      var v = p.deletions;
      v === null ? (p.deletions = [f], p.flags |= 16) : v.push(f);
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
  function a(p, f) {
    return p = on(p, f), p.index = 0, p.sibling = null, p;
  }
  function i(p, f, v) {
    return p.index = v, e ? (v = p.alternate, v !== null ? (v = v.index, v < f ? (p.flags |= 2, f) : v) : (p.flags |= 2, f)) : (p.flags |= 1048576, f);
  }
  function o(p) {
    return e && p.alternate === null && (p.flags |= 2), p;
  }
  function s(p, f, v, k) {
    return f === null || f.tag !== 6 ? (f = Kl(v, p.mode, k), f.return = p, f) : (f = a(f, v), f.return = p, f);
  }
  function c(p, f, v, k) {
    var T = v.type;
    return T === Dn ? j(p, f, v.props.children, k, v.key) : f !== null && (f.elementType === T || typeof T == "object" && T !== null && T.$$typeof === Gt && As(T) === f.type) ? (k = a(f, v.props), k.ref = pr(p, f, v), k.return = p, k) : (k = Ra(v.type, v.key, v.props, null, p.mode, k), k.ref = pr(p, f, v), k.return = p, k);
  }
  function d(p, f, v, k) {
    return f === null || f.tag !== 4 || f.stateNode.containerInfo !== v.containerInfo || f.stateNode.implementation !== v.implementation ? (f = ql(v, p.mode, k), f.return = p, f) : (f = a(f, v.children || []), f.return = p, f);
  }
  function j(p, f, v, k, T) {
    return f === null || f.tag !== 7 ? (f = Nn(v, p.mode, k, T), f.return = p, f) : (f = a(f, v), f.return = p, f);
  }
  function u(p, f, v) {
    if (typeof f == "string" && f !== "" || typeof f == "number") return f = Kl("" + f, p.mode, v), f.return = p, f;
    if (typeof f == "object" && f !== null) {
      switch (f.$$typeof) {
        case ra:
          return v = Ra(f.type, f.key, f.props, null, p.mode, v), v.ref = pr(p, null, f), v.return = p, v;
        case Tn:
          return f = ql(f, p.mode, v), f.return = p, f;
        case Gt:
          var k = f._init;
          return u(p, k(f._payload), v);
      }
      if (xr(f) || sr(f)) return f = Nn(f, p.mode, v, null), f.return = p, f;
      ma(p, f);
    }
    return null;
  }
  function m(p, f, v, k) {
    var T = f !== null ? f.key : null;
    if (typeof v == "string" && v !== "" || typeof v == "number") return T !== null ? null : s(p, f, "" + v, k);
    if (typeof v == "object" && v !== null) {
      switch (v.$$typeof) {
        case ra:
          return v.key === T ? c(p, f, v, k) : null;
        case Tn:
          return v.key === T ? d(p, f, v, k) : null;
        case Gt:
          return T = v._init, m(
            p,
            f,
            T(v._payload),
            k
          );
      }
      if (xr(v) || sr(v)) return T !== null ? null : j(p, f, v, k, null);
      ma(p, v);
    }
    return null;
  }
  function h(p, f, v, k, T) {
    if (typeof k == "string" && k !== "" || typeof k == "number") return p = p.get(v) || null, s(f, p, "" + k, T);
    if (typeof k == "object" && k !== null) {
      switch (k.$$typeof) {
        case ra:
          return p = p.get(k.key === null ? v : k.key) || null, c(f, p, k, T);
        case Tn:
          return p = p.get(k.key === null ? v : k.key) || null, d(f, p, k, T);
        case Gt:
          var F = k._init;
          return h(p, f, v, F(k._payload), T);
      }
      if (xr(k) || sr(k)) return p = p.get(v) || null, j(f, p, k, T, null);
      ma(f, k);
    }
    return null;
  }
  function g(p, f, v, k) {
    for (var T = null, F = null, D = f, C = f = 0, M = null; D !== null && C < v.length; C++) {
      D.index > C ? (M = D, D = null) : M = D.sibling;
      var b = m(p, D, v[C], k);
      if (b === null) {
        D === null && (D = M);
        break;
      }
      e && D && b.alternate === null && t(p, D), f = i(b, f, C), F === null ? T = b : F.sibling = b, F = b, D = M;
    }
    if (C === v.length) return n(p, D), se && hn(p, C), T;
    if (D === null) {
      for (; C < v.length; C++) D = u(p, v[C], k), D !== null && (f = i(D, f, C), F === null ? T = D : F.sibling = D, F = D);
      return se && hn(p, C), T;
    }
    for (D = r(p, D); C < v.length; C++) M = h(D, p, C, v[C], k), M !== null && (e && M.alternate !== null && D.delete(M.key === null ? C : M.key), f = i(M, f, C), F === null ? T = M : F.sibling = M, F = M);
    return e && D.forEach(function(P) {
      return t(p, P);
    }), se && hn(p, C), T;
  }
  function w(p, f, v, k) {
    var T = sr(v);
    if (typeof T != "function") throw Error(I(150));
    if (v = T.call(v), v == null) throw Error(I(151));
    for (var F = T = null, D = f, C = f = 0, M = null, b = v.next(); D !== null && !b.done; C++, b = v.next()) {
      D.index > C ? (M = D, D = null) : M = D.sibling;
      var P = m(p, D, b.value, k);
      if (P === null) {
        D === null && (D = M);
        break;
      }
      e && D && P.alternate === null && t(p, D), f = i(P, f, C), F === null ? T = P : F.sibling = P, F = P, D = M;
    }
    if (b.done) return n(
      p,
      D
    ), se && hn(p, C), T;
    if (D === null) {
      for (; !b.done; C++, b = v.next()) b = u(p, b.value, k), b !== null && (f = i(b, f, C), F === null ? T = b : F.sibling = b, F = b);
      return se && hn(p, C), T;
    }
    for (D = r(p, D); !b.done; C++, b = v.next()) b = h(D, p, C, b.value, k), b !== null && (e && b.alternate !== null && D.delete(b.key === null ? C : b.key), f = i(b, f, C), F === null ? T = b : F.sibling = b, F = b);
    return e && D.forEach(function(Q) {
      return t(p, Q);
    }), se && hn(p, C), T;
  }
  function $(p, f, v, k) {
    if (typeof v == "object" && v !== null && v.type === Dn && v.key === null && (v = v.props.children), typeof v == "object" && v !== null) {
      switch (v.$$typeof) {
        case ra:
          e: {
            for (var T = v.key, F = f; F !== null; ) {
              if (F.key === T) {
                if (T = v.type, T === Dn) {
                  if (F.tag === 7) {
                    n(p, F.sibling), f = a(F, v.props.children), f.return = p, p = f;
                    break e;
                  }
                } else if (F.elementType === T || typeof T == "object" && T !== null && T.$$typeof === Gt && As(T) === F.type) {
                  n(p, F.sibling), f = a(F, v.props), f.ref = pr(p, F, v), f.return = p, p = f;
                  break e;
                }
                n(p, F);
                break;
              } else t(p, F);
              F = F.sibling;
            }
            v.type === Dn ? (f = Nn(v.props.children, p.mode, k, v.key), f.return = p, p = f) : (k = Ra(v.type, v.key, v.props, null, p.mode, k), k.ref = pr(p, f, v), k.return = p, p = k);
          }
          return o(p);
        case Tn:
          e: {
            for (F = v.key; f !== null; ) {
              if (f.key === F) if (f.tag === 4 && f.stateNode.containerInfo === v.containerInfo && f.stateNode.implementation === v.implementation) {
                n(p, f.sibling), f = a(f, v.children || []), f.return = p, p = f;
                break e;
              } else {
                n(p, f);
                break;
              }
              else t(p, f);
              f = f.sibling;
            }
            f = ql(v, p.mode, k), f.return = p, p = f;
          }
          return o(p);
        case Gt:
          return F = v._init, $(p, f, F(v._payload), k);
      }
      if (xr(v)) return g(p, f, v, k);
      if (sr(v)) return w(p, f, v, k);
      ma(p, v);
    }
    return typeof v == "string" && v !== "" || typeof v == "number" ? (v = "" + v, f !== null && f.tag === 6 ? (n(p, f.sibling), f = a(f, v), f.return = p, p = f) : (n(p, f), f = Kl(v, p.mode, k), f.return = p, p = f), o(p)) : n(p, f);
  }
  return $;
}
var er = Cu(!0), ku = Cu(!1), Ha = fn(null), Wa = null, Un = null, vo = null;
function xo() {
  vo = Un = Wa = null;
}
function go(e) {
  var t = Ha.current;
  oe(Ha), e._currentValue = t;
}
function Pi(e, t, n) {
  for (; e !== null; ) {
    var r = e.alternate;
    if ((e.childLanes & t) !== t ? (e.childLanes |= t, r !== null && (r.childLanes |= t)) : r !== null && (r.childLanes & t) !== t && (r.childLanes |= t), e === n) break;
    e = e.return;
  }
}
function Kn(e, t) {
  Wa = e, vo = Un = null, e = e.dependencies, e !== null && e.firstContext !== null && (e.lanes & t && (Ye = !0), e.firstContext = null);
}
function ht(e) {
  var t = e._currentValue;
  if (vo !== e) if (e = { context: e, memoizedValue: t, next: null }, Un === null) {
    if (Wa === null) throw Error(I(308));
    Un = e, Wa.dependencies = { lanes: 0, firstContext: e };
  } else Un = Un.next = e;
  return t;
}
var gn = null;
function yo(e) {
  gn === null ? gn = [e] : gn.push(e);
}
function Eu(e, t, n, r) {
  var a = t.interleaved;
  return a === null ? (n.next = n, yo(t)) : (n.next = a.next, a.next = n), t.interleaved = n, Bt(e, r);
}
function Bt(e, t) {
  e.lanes |= t;
  var n = e.alternate;
  for (n !== null && (n.lanes |= t), n = e, e = e.return; e !== null; ) e.childLanes |= t, n = e.alternate, n !== null && (n.childLanes |= t), n = e, e = e.return;
  return n.tag === 3 ? n.stateNode : null;
}
var Kt = !1;
function jo(e) {
  e.updateQueue = { baseState: e.memoizedState, firstBaseUpdate: null, lastBaseUpdate: null, shared: { pending: null, interleaved: null, lanes: 0 }, effects: null };
}
function Iu(e, t) {
  e = e.updateQueue, t.updateQueue === e && (t.updateQueue = { baseState: e.baseState, firstBaseUpdate: e.firstBaseUpdate, lastBaseUpdate: e.lastBaseUpdate, shared: e.shared, effects: e.effects });
}
function bt(e, t) {
  return { eventTime: e, lane: t, tag: 0, payload: null, callback: null, next: null };
}
function rn(e, t, n) {
  var r = e.updateQueue;
  if (r === null) return null;
  if (r = r.shared, J & 2) {
    var a = r.pending;
    return a === null ? t.next = t : (t.next = a.next, a.next = t), r.pending = t, Bt(e, n);
  }
  return a = r.interleaved, a === null ? (t.next = t, yo(r)) : (t.next = a.next, a.next = t), r.interleaved = t, Bt(e, n);
}
function Ca(e, t, n) {
  if (t = t.updateQueue, t !== null && (t = t.shared, (n & 4194240) !== 0)) {
    var r = t.lanes;
    r &= e.pendingLanes, n |= r, t.lanes = n, ao(e, n);
  }
}
function Ms(e, t) {
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
function Qa(e, t, n, r) {
  var a = e.updateQueue;
  Kt = !1;
  var i = a.firstBaseUpdate, o = a.lastBaseUpdate, s = a.shared.pending;
  if (s !== null) {
    a.shared.pending = null;
    var c = s, d = c.next;
    c.next = null, o === null ? i = d : o.next = d, o = c;
    var j = e.alternate;
    j !== null && (j = j.updateQueue, s = j.lastBaseUpdate, s !== o && (s === null ? j.firstBaseUpdate = d : s.next = d, j.lastBaseUpdate = c));
  }
  if (i !== null) {
    var u = a.baseState;
    o = 0, j = d = c = null, s = i;
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
          var g = e, w = s;
          switch (m = t, h = n, w.tag) {
            case 1:
              if (g = w.payload, typeof g == "function") {
                u = g.call(h, u, m);
                break e;
              }
              u = g;
              break e;
            case 3:
              g.flags = g.flags & -65537 | 128;
            case 0:
              if (g = w.payload, m = typeof g == "function" ? g.call(h, u, m) : g, m == null) break e;
              u = fe({}, u, m);
              break e;
            case 2:
              Kt = !0;
          }
        }
        s.callback !== null && s.lane !== 0 && (e.flags |= 64, m = a.effects, m === null ? a.effects = [s] : m.push(s));
      } else h = { eventTime: h, lane: m, tag: s.tag, payload: s.payload, callback: s.callback, next: null }, j === null ? (d = j = h, c = u) : j = j.next = h, o |= m;
      if (s = s.next, s === null) {
        if (s = a.shared.pending, s === null) break;
        m = s, s = m.next, m.next = null, a.lastBaseUpdate = m, a.shared.pending = null;
      }
    } while (!0);
    if (j === null && (c = u), a.baseState = c, a.firstBaseUpdate = d, a.lastBaseUpdate = j, t = a.shared.interleaved, t !== null) {
      a = t;
      do
        o |= a.lane, a = a.next;
      while (a !== t);
    } else i === null && (a.shared.lanes = 0);
    En |= o, e.lanes = o, e.memoizedState = u;
  }
}
function $s(e, t, n) {
  if (e = t.effects, t.effects = null, e !== null) for (t = 0; t < e.length; t++) {
    var r = e[t], a = r.callback;
    if (a !== null) {
      if (r.callback = null, r = n, typeof a != "function") throw Error(I(191, a));
      a.call(r);
    }
  }
}
var Zr = {}, Tt = fn(Zr), Ur = fn(Zr), Vr = fn(Zr);
function yn(e) {
  if (e === Zr) throw Error(I(174));
  return e;
}
function No(e, t) {
  switch (ae(Vr, t), ae(Ur, e), ae(Tt, Zr), e = t.nodeType, e) {
    case 9:
    case 11:
      t = (t = t.documentElement) ? t.namespaceURI : si(null, "");
      break;
    default:
      e = e === 8 ? t.parentNode : t, t = e.namespaceURI || null, e = e.tagName, t = si(t, e);
  }
  oe(Tt), ae(Tt, t);
}
function tr() {
  oe(Tt), oe(Ur), oe(Vr);
}
function Pu(e) {
  yn(Vr.current);
  var t = yn(Tt.current), n = si(t, e.type);
  t !== n && (ae(Ur, e), ae(Tt, n));
}
function So(e) {
  Ur.current === e && (oe(Tt), oe(Ur));
}
var ue = fn(0);
function Ga(e) {
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
var Vl = [];
function wo() {
  for (var e = 0; e < Vl.length; e++) Vl[e]._workInProgressVersionPrimary = null;
  Vl.length = 0;
}
var ka = Wt.ReactCurrentDispatcher, Bl = Wt.ReactCurrentBatchConfig, kn = 0, de = null, Ce = null, Pe = null, Ka = !1, kr = !1, Br = 0, Dp = 0;
function Ae() {
  throw Error(I(321));
}
function Co(e, t) {
  if (t === null) return !1;
  for (var n = 0; n < t.length && n < e.length; n++) if (!kt(e[n], t[n])) return !1;
  return !0;
}
function ko(e, t, n, r, a, i) {
  if (kn = i, de = t, t.memoizedState = null, t.updateQueue = null, t.lanes = 0, ka.current = e === null || e.memoizedState === null ? Mp : $p, e = n(r, a), kr) {
    i = 0;
    do {
      if (kr = !1, Br = 0, 25 <= i) throw Error(I(301));
      i += 1, Pe = Ce = null, t.updateQueue = null, ka.current = Op, e = n(r, a);
    } while (kr);
  }
  if (ka.current = qa, t = Ce !== null && Ce.next !== null, kn = 0, Pe = Ce = de = null, Ka = !1, t) throw Error(I(300));
  return e;
}
function Eo() {
  var e = Br !== 0;
  return Br = 0, e;
}
function Ft() {
  var e = { memoizedState: null, baseState: null, baseQueue: null, queue: null, next: null };
  return Pe === null ? de.memoizedState = Pe = e : Pe = Pe.next = e, Pe;
}
function vt() {
  if (Ce === null) {
    var e = de.alternate;
    e = e !== null ? e.memoizedState : null;
  } else e = Ce.next;
  var t = Pe === null ? de.memoizedState : Pe.next;
  if (t !== null) Pe = t, Ce = e;
  else {
    if (e === null) throw Error(I(310));
    Ce = e, e = { memoizedState: Ce.memoizedState, baseState: Ce.baseState, baseQueue: Ce.baseQueue, queue: Ce.queue, next: null }, Pe === null ? de.memoizedState = Pe = e : Pe = Pe.next = e;
  }
  return Pe;
}
function Hr(e, t) {
  return typeof t == "function" ? t(e) : t;
}
function Hl(e) {
  var t = vt(), n = t.queue;
  if (n === null) throw Error(I(311));
  n.lastRenderedReducer = e;
  var r = Ce, a = r.baseQueue, i = n.pending;
  if (i !== null) {
    if (a !== null) {
      var o = a.next;
      a.next = i.next, i.next = o;
    }
    r.baseQueue = a = i, n.pending = null;
  }
  if (a !== null) {
    i = a.next, r = r.baseState;
    var s = o = null, c = null, d = i;
    do {
      var j = d.lane;
      if ((kn & j) === j) c !== null && (c = c.next = { lane: 0, action: d.action, hasEagerState: d.hasEagerState, eagerState: d.eagerState, next: null }), r = d.hasEagerState ? d.eagerState : e(r, d.action);
      else {
        var u = {
          lane: j,
          action: d.action,
          hasEagerState: d.hasEagerState,
          eagerState: d.eagerState,
          next: null
        };
        c === null ? (s = c = u, o = r) : c = c.next = u, de.lanes |= j, En |= j;
      }
      d = d.next;
    } while (d !== null && d !== i);
    c === null ? o = r : c.next = s, kt(r, t.memoizedState) || (Ye = !0), t.memoizedState = r, t.baseState = o, t.baseQueue = c, n.lastRenderedState = r;
  }
  if (e = n.interleaved, e !== null) {
    a = e;
    do
      i = a.lane, de.lanes |= i, En |= i, a = a.next;
    while (a !== e);
  } else a === null && (n.lanes = 0);
  return [t.memoizedState, n.dispatch];
}
function Wl(e) {
  var t = vt(), n = t.queue;
  if (n === null) throw Error(I(311));
  n.lastRenderedReducer = e;
  var r = n.dispatch, a = n.pending, i = t.memoizedState;
  if (a !== null) {
    n.pending = null;
    var o = a = a.next;
    do
      i = e(i, o.action), o = o.next;
    while (o !== a);
    kt(i, t.memoizedState) || (Ye = !0), t.memoizedState = i, t.baseQueue === null && (t.baseState = i), n.lastRenderedState = i;
  }
  return [i, r];
}
function Fu() {
}
function Ru(e, t) {
  var n = de, r = vt(), a = t(), i = !kt(r.memoizedState, a);
  if (i && (r.memoizedState = a, Ye = !0), r = r.queue, Io(Du.bind(null, n, r, e), [e]), r.getSnapshot !== t || i || Pe !== null && Pe.memoizedState.tag & 1) {
    if (n.flags |= 2048, Wr(9, Tu.bind(null, n, r, a, t), void 0, null), Fe === null) throw Error(I(349));
    kn & 30 || _u(n, t, a);
  }
  return a;
}
function _u(e, t, n) {
  e.flags |= 16384, e = { getSnapshot: t, value: n }, t = de.updateQueue, t === null ? (t = { lastEffect: null, stores: null }, de.updateQueue = t, t.stores = [e]) : (n = t.stores, n === null ? t.stores = [e] : n.push(e));
}
function Tu(e, t, n, r) {
  t.value = n, t.getSnapshot = r, zu(t) && Lu(e);
}
function Du(e, t, n) {
  return n(function() {
    zu(t) && Lu(e);
  });
}
function zu(e) {
  var t = e.getSnapshot;
  e = e.value;
  try {
    var n = t();
    return !kt(e, n);
  } catch {
    return !0;
  }
}
function Lu(e) {
  var t = Bt(e, 1);
  t !== null && wt(t, e, 1, -1);
}
function Os(e) {
  var t = Ft();
  return typeof e == "function" && (e = e()), t.memoizedState = t.baseState = e, e = { pending: null, interleaved: null, lanes: 0, dispatch: null, lastRenderedReducer: Hr, lastRenderedState: e }, t.queue = e, e = e.dispatch = Ap.bind(null, de, e), [t.memoizedState, e];
}
function Wr(e, t, n, r) {
  return e = { tag: e, create: t, destroy: n, deps: r, next: null }, t = de.updateQueue, t === null ? (t = { lastEffect: null, stores: null }, de.updateQueue = t, t.lastEffect = e.next = e) : (n = t.lastEffect, n === null ? t.lastEffect = e.next = e : (r = n.next, n.next = e, e.next = r, t.lastEffect = e)), e;
}
function Au() {
  return vt().memoizedState;
}
function Ea(e, t, n, r) {
  var a = Ft();
  de.flags |= e, a.memoizedState = Wr(1 | t, n, void 0, r === void 0 ? null : r);
}
function sl(e, t, n, r) {
  var a = vt();
  r = r === void 0 ? null : r;
  var i = void 0;
  if (Ce !== null) {
    var o = Ce.memoizedState;
    if (i = o.destroy, r !== null && Co(r, o.deps)) {
      a.memoizedState = Wr(t, n, i, r);
      return;
    }
  }
  de.flags |= e, a.memoizedState = Wr(1 | t, n, i, r);
}
function bs(e, t) {
  return Ea(8390656, 8, e, t);
}
function Io(e, t) {
  return sl(2048, 8, e, t);
}
function Mu(e, t) {
  return sl(4, 2, e, t);
}
function $u(e, t) {
  return sl(4, 4, e, t);
}
function Ou(e, t) {
  if (typeof t == "function") return e = e(), t(e), function() {
    t(null);
  };
  if (t != null) return e = e(), t.current = e, function() {
    t.current = null;
  };
}
function bu(e, t, n) {
  return n = n != null ? n.concat([e]) : null, sl(4, 4, Ou.bind(null, t, e), n);
}
function Po() {
}
function Uu(e, t) {
  var n = vt();
  t = t === void 0 ? null : t;
  var r = n.memoizedState;
  return r !== null && t !== null && Co(t, r[1]) ? r[0] : (n.memoizedState = [e, t], e);
}
function Vu(e, t) {
  var n = vt();
  t = t === void 0 ? null : t;
  var r = n.memoizedState;
  return r !== null && t !== null && Co(t, r[1]) ? r[0] : (e = e(), n.memoizedState = [e, t], e);
}
function Bu(e, t, n) {
  return kn & 21 ? (kt(n, t) || (n = Kc(), de.lanes |= n, En |= n, e.baseState = !0), t) : (e.baseState && (e.baseState = !1, Ye = !0), e.memoizedState = n);
}
function zp(e, t) {
  var n = ne;
  ne = n !== 0 && 4 > n ? n : 4, e(!0);
  var r = Bl.transition;
  Bl.transition = {};
  try {
    e(!1), t();
  } finally {
    ne = n, Bl.transition = r;
  }
}
function Hu() {
  return vt().memoizedState;
}
function Lp(e, t, n) {
  var r = ln(e);
  if (n = { lane: r, action: n, hasEagerState: !1, eagerState: null, next: null }, Wu(e)) Qu(t, n);
  else if (n = Eu(e, t, n, r), n !== null) {
    var a = Qe();
    wt(n, e, r, a), Gu(n, t, r);
  }
}
function Ap(e, t, n) {
  var r = ln(e), a = { lane: r, action: n, hasEagerState: !1, eagerState: null, next: null };
  if (Wu(e)) Qu(t, a);
  else {
    var i = e.alternate;
    if (e.lanes === 0 && (i === null || i.lanes === 0) && (i = t.lastRenderedReducer, i !== null)) try {
      var o = t.lastRenderedState, s = i(o, n);
      if (a.hasEagerState = !0, a.eagerState = s, kt(s, o)) {
        var c = t.interleaved;
        c === null ? (a.next = a, yo(t)) : (a.next = c.next, c.next = a), t.interleaved = a;
        return;
      }
    } catch {
    } finally {
    }
    n = Eu(e, t, a, r), n !== null && (a = Qe(), wt(n, e, r, a), Gu(n, t, r));
  }
}
function Wu(e) {
  var t = e.alternate;
  return e === de || t !== null && t === de;
}
function Qu(e, t) {
  kr = Ka = !0;
  var n = e.pending;
  n === null ? t.next = t : (t.next = n.next, n.next = t), e.pending = t;
}
function Gu(e, t, n) {
  if (n & 4194240) {
    var r = t.lanes;
    r &= e.pendingLanes, n |= r, t.lanes = n, ao(e, n);
  }
}
var qa = { readContext: ht, useCallback: Ae, useContext: Ae, useEffect: Ae, useImperativeHandle: Ae, useInsertionEffect: Ae, useLayoutEffect: Ae, useMemo: Ae, useReducer: Ae, useRef: Ae, useState: Ae, useDebugValue: Ae, useDeferredValue: Ae, useTransition: Ae, useMutableSource: Ae, useSyncExternalStore: Ae, useId: Ae, unstable_isNewReconciler: !1 }, Mp = { readContext: ht, useCallback: function(e, t) {
  return Ft().memoizedState = [e, t === void 0 ? null : t], e;
}, useContext: ht, useEffect: bs, useImperativeHandle: function(e, t, n) {
  return n = n != null ? n.concat([e]) : null, Ea(
    4194308,
    4,
    Ou.bind(null, t, e),
    n
  );
}, useLayoutEffect: function(e, t) {
  return Ea(4194308, 4, e, t);
}, useInsertionEffect: function(e, t) {
  return Ea(4, 2, e, t);
}, useMemo: function(e, t) {
  var n = Ft();
  return t = t === void 0 ? null : t, e = e(), n.memoizedState = [e, t], e;
}, useReducer: function(e, t, n) {
  var r = Ft();
  return t = n !== void 0 ? n(t) : t, r.memoizedState = r.baseState = t, e = { pending: null, interleaved: null, lanes: 0, dispatch: null, lastRenderedReducer: e, lastRenderedState: t }, r.queue = e, e = e.dispatch = Lp.bind(null, de, e), [r.memoizedState, e];
}, useRef: function(e) {
  var t = Ft();
  return e = { current: e }, t.memoizedState = e;
}, useState: Os, useDebugValue: Po, useDeferredValue: function(e) {
  return Ft().memoizedState = e;
}, useTransition: function() {
  var e = Os(!1), t = e[0];
  return e = zp.bind(null, e[1]), Ft().memoizedState = e, [t, e];
}, useMutableSource: function() {
}, useSyncExternalStore: function(e, t, n) {
  var r = de, a = Ft();
  if (se) {
    if (n === void 0) throw Error(I(407));
    n = n();
  } else {
    if (n = t(), Fe === null) throw Error(I(349));
    kn & 30 || _u(r, t, n);
  }
  a.memoizedState = n;
  var i = { value: n, getSnapshot: t };
  return a.queue = i, bs(Du.bind(
    null,
    r,
    i,
    e
  ), [e]), r.flags |= 2048, Wr(9, Tu.bind(null, r, i, n, t), void 0, null), n;
}, useId: function() {
  var e = Ft(), t = Fe.identifierPrefix;
  if (se) {
    var n = Ot, r = $t;
    n = (r & ~(1 << 32 - St(r) - 1)).toString(32) + n, t = ":" + t + "R" + n, n = Br++, 0 < n && (t += "H" + n.toString(32)), t += ":";
  } else n = Dp++, t = ":" + t + "r" + n.toString(32) + ":";
  return e.memoizedState = t;
}, unstable_isNewReconciler: !1 }, $p = {
  readContext: ht,
  useCallback: Uu,
  useContext: ht,
  useEffect: Io,
  useImperativeHandle: bu,
  useInsertionEffect: Mu,
  useLayoutEffect: $u,
  useMemo: Vu,
  useReducer: Hl,
  useRef: Au,
  useState: function() {
    return Hl(Hr);
  },
  useDebugValue: Po,
  useDeferredValue: function(e) {
    var t = vt();
    return Bu(t, Ce.memoizedState, e);
  },
  useTransition: function() {
    var e = Hl(Hr)[0], t = vt().memoizedState;
    return [e, t];
  },
  useMutableSource: Fu,
  useSyncExternalStore: Ru,
  useId: Hu,
  unstable_isNewReconciler: !1
}, Op = { readContext: ht, useCallback: Uu, useContext: ht, useEffect: Io, useImperativeHandle: bu, useInsertionEffect: Mu, useLayoutEffect: $u, useMemo: Vu, useReducer: Wl, useRef: Au, useState: function() {
  return Wl(Hr);
}, useDebugValue: Po, useDeferredValue: function(e) {
  var t = vt();
  return Ce === null ? t.memoizedState = e : Bu(t, Ce.memoizedState, e);
}, useTransition: function() {
  var e = Wl(Hr)[0], t = vt().memoizedState;
  return [e, t];
}, useMutableSource: Fu, useSyncExternalStore: Ru, useId: Hu, unstable_isNewReconciler: !1 };
function yt(e, t) {
  if (e && e.defaultProps) {
    t = fe({}, t), e = e.defaultProps;
    for (var n in e) t[n] === void 0 && (t[n] = e[n]);
    return t;
  }
  return t;
}
function Fi(e, t, n, r) {
  t = e.memoizedState, n = n(r, t), n = n == null ? t : fe({}, t, n), e.memoizedState = n, e.lanes === 0 && (e.updateQueue.baseState = n);
}
var cl = { isMounted: function(e) {
  return (e = e._reactInternals) ? Fn(e) === e : !1;
}, enqueueSetState: function(e, t, n) {
  e = e._reactInternals;
  var r = Qe(), a = ln(e), i = bt(r, a);
  i.payload = t, n != null && (i.callback = n), t = rn(e, i, a), t !== null && (wt(t, e, a, r), Ca(t, e, a));
}, enqueueReplaceState: function(e, t, n) {
  e = e._reactInternals;
  var r = Qe(), a = ln(e), i = bt(r, a);
  i.tag = 1, i.payload = t, n != null && (i.callback = n), t = rn(e, i, a), t !== null && (wt(t, e, a, r), Ca(t, e, a));
}, enqueueForceUpdate: function(e, t) {
  e = e._reactInternals;
  var n = Qe(), r = ln(e), a = bt(n, r);
  a.tag = 2, t != null && (a.callback = t), t = rn(e, a, r), t !== null && (wt(t, e, r, n), Ca(t, e, r));
} };
function Us(e, t, n, r, a, i, o) {
  return e = e.stateNode, typeof e.shouldComponentUpdate == "function" ? e.shouldComponentUpdate(r, i, o) : t.prototype && t.prototype.isPureReactComponent ? !Mr(n, r) || !Mr(a, i) : !0;
}
function Ku(e, t, n) {
  var r = !1, a = cn, i = t.contextType;
  return typeof i == "object" && i !== null ? i = ht(i) : (a = Ze(t) ? wn : Ue.current, r = t.contextTypes, i = (r = r != null) ? Zn(e, a) : cn), t = new t(n, i), e.memoizedState = t.state !== null && t.state !== void 0 ? t.state : null, t.updater = cl, e.stateNode = t, t._reactInternals = e, r && (e = e.stateNode, e.__reactInternalMemoizedUnmaskedChildContext = a, e.__reactInternalMemoizedMaskedChildContext = i), t;
}
function Vs(e, t, n, r) {
  e = t.state, typeof t.componentWillReceiveProps == "function" && t.componentWillReceiveProps(n, r), typeof t.UNSAFE_componentWillReceiveProps == "function" && t.UNSAFE_componentWillReceiveProps(n, r), t.state !== e && cl.enqueueReplaceState(t, t.state, null);
}
function Ri(e, t, n, r) {
  var a = e.stateNode;
  a.props = n, a.state = e.memoizedState, a.refs = {}, jo(e);
  var i = t.contextType;
  typeof i == "object" && i !== null ? a.context = ht(i) : (i = Ze(t) ? wn : Ue.current, a.context = Zn(e, i)), a.state = e.memoizedState, i = t.getDerivedStateFromProps, typeof i == "function" && (Fi(e, t, i, n), a.state = e.memoizedState), typeof t.getDerivedStateFromProps == "function" || typeof a.getSnapshotBeforeUpdate == "function" || typeof a.UNSAFE_componentWillMount != "function" && typeof a.componentWillMount != "function" || (t = a.state, typeof a.componentWillMount == "function" && a.componentWillMount(), typeof a.UNSAFE_componentWillMount == "function" && a.UNSAFE_componentWillMount(), t !== a.state && cl.enqueueReplaceState(a, a.state, null), Qa(e, n, a, r), a.state = e.memoizedState), typeof a.componentDidMount == "function" && (e.flags |= 4194308);
}
function nr(e, t) {
  try {
    var n = "", r = t;
    do
      n += pf(r), r = r.return;
    while (r);
    var a = n;
  } catch (i) {
    a = `
Error generating stack: ` + i.message + `
` + i.stack;
  }
  return { value: e, source: t, stack: a, digest: null };
}
function Ql(e, t, n) {
  return { value: e, source: null, stack: n ?? null, digest: t ?? null };
}
function _i(e, t) {
  try {
    console.error(t.value);
  } catch (n) {
    setTimeout(function() {
      throw n;
    });
  }
}
var bp = typeof WeakMap == "function" ? WeakMap : Map;
function qu(e, t, n) {
  n = bt(-1, n), n.tag = 3, n.payload = { element: null };
  var r = t.value;
  return n.callback = function() {
    Xa || (Xa = !0, Ui = r), _i(e, t);
  }, n;
}
function Yu(e, t, n) {
  n = bt(-1, n), n.tag = 3;
  var r = e.type.getDerivedStateFromError;
  if (typeof r == "function") {
    var a = t.value;
    n.payload = function() {
      return r(a);
    }, n.callback = function() {
      _i(e, t);
    };
  }
  var i = e.stateNode;
  return i !== null && typeof i.componentDidCatch == "function" && (n.callback = function() {
    _i(e, t), typeof r != "function" && (an === null ? an = /* @__PURE__ */ new Set([this]) : an.add(this));
    var o = t.stack;
    this.componentDidCatch(t.value, { componentStack: o !== null ? o : "" });
  }), n;
}
function Bs(e, t, n) {
  var r = e.pingCache;
  if (r === null) {
    r = e.pingCache = new bp();
    var a = /* @__PURE__ */ new Set();
    r.set(t, a);
  } else a = r.get(t), a === void 0 && (a = /* @__PURE__ */ new Set(), r.set(t, a));
  a.has(n) || (a.add(n), e = em.bind(null, e, t, n), t.then(e, e));
}
function Hs(e) {
  do {
    var t;
    if ((t = e.tag === 13) && (t = e.memoizedState, t = t !== null ? t.dehydrated !== null : !0), t) return e;
    e = e.return;
  } while (e !== null);
  return null;
}
function Ws(e, t, n, r, a) {
  return e.mode & 1 ? (e.flags |= 65536, e.lanes = a, e) : (e === t ? e.flags |= 65536 : (e.flags |= 128, n.flags |= 131072, n.flags &= -52805, n.tag === 1 && (n.alternate === null ? n.tag = 17 : (t = bt(-1, 1), t.tag = 2, rn(n, t, 1))), n.lanes |= 1), e);
}
var Up = Wt.ReactCurrentOwner, Ye = !1;
function He(e, t, n, r) {
  t.child = e === null ? ku(t, null, n, r) : er(t, e.child, n, r);
}
function Qs(e, t, n, r, a) {
  n = n.render;
  var i = t.ref;
  return Kn(t, a), r = ko(e, t, n, r, i, a), n = Eo(), e !== null && !Ye ? (t.updateQueue = e.updateQueue, t.flags &= -2053, e.lanes &= ~a, Ht(e, t, a)) : (se && n && po(t), t.flags |= 1, He(e, t, r, a), t.child);
}
function Gs(e, t, n, r, a) {
  if (e === null) {
    var i = n.type;
    return typeof i == "function" && !Ao(i) && i.defaultProps === void 0 && n.compare === null && n.defaultProps === void 0 ? (t.tag = 15, t.type = i, Xu(e, t, i, r, a)) : (e = Ra(n.type, null, r, t, t.mode, a), e.ref = t.ref, e.return = t, t.child = e);
  }
  if (i = e.child, !(e.lanes & a)) {
    var o = i.memoizedProps;
    if (n = n.compare, n = n !== null ? n : Mr, n(o, r) && e.ref === t.ref) return Ht(e, t, a);
  }
  return t.flags |= 1, e = on(i, r), e.ref = t.ref, e.return = t, t.child = e;
}
function Xu(e, t, n, r, a) {
  if (e !== null) {
    var i = e.memoizedProps;
    if (Mr(i, r) && e.ref === t.ref) if (Ye = !1, t.pendingProps = r = i, (e.lanes & a) !== 0) e.flags & 131072 && (Ye = !0);
    else return t.lanes = e.lanes, Ht(e, t, a);
  }
  return Ti(e, t, n, r, a);
}
function Zu(e, t, n) {
  var r = t.pendingProps, a = r.children, i = e !== null ? e.memoizedState : null;
  if (r.mode === "hidden") if (!(t.mode & 1)) t.memoizedState = { baseLanes: 0, cachePool: null, transitions: null }, ae(Bn, tt), tt |= n;
  else {
    if (!(n & 1073741824)) return e = i !== null ? i.baseLanes | n : n, t.lanes = t.childLanes = 1073741824, t.memoizedState = { baseLanes: e, cachePool: null, transitions: null }, t.updateQueue = null, ae(Bn, tt), tt |= e, null;
    t.memoizedState = { baseLanes: 0, cachePool: null, transitions: null }, r = i !== null ? i.baseLanes : n, ae(Bn, tt), tt |= r;
  }
  else i !== null ? (r = i.baseLanes | n, t.memoizedState = null) : r = n, ae(Bn, tt), tt |= r;
  return He(e, t, a, n), t.child;
}
function Ju(e, t) {
  var n = t.ref;
  (e === null && n !== null || e !== null && e.ref !== n) && (t.flags |= 512, t.flags |= 2097152);
}
function Ti(e, t, n, r, a) {
  var i = Ze(n) ? wn : Ue.current;
  return i = Zn(t, i), Kn(t, a), n = ko(e, t, n, r, i, a), r = Eo(), e !== null && !Ye ? (t.updateQueue = e.updateQueue, t.flags &= -2053, e.lanes &= ~a, Ht(e, t, a)) : (se && r && po(t), t.flags |= 1, He(e, t, n, a), t.child);
}
function Ks(e, t, n, r, a) {
  if (Ze(n)) {
    var i = !0;
    Ua(t);
  } else i = !1;
  if (Kn(t, a), t.stateNode === null) Ia(e, t), Ku(t, n, r), Ri(t, n, r, a), r = !0;
  else if (e === null) {
    var o = t.stateNode, s = t.memoizedProps;
    o.props = s;
    var c = o.context, d = n.contextType;
    typeof d == "object" && d !== null ? d = ht(d) : (d = Ze(n) ? wn : Ue.current, d = Zn(t, d));
    var j = n.getDerivedStateFromProps, u = typeof j == "function" || typeof o.getSnapshotBeforeUpdate == "function";
    u || typeof o.UNSAFE_componentWillReceiveProps != "function" && typeof o.componentWillReceiveProps != "function" || (s !== r || c !== d) && Vs(t, o, r, d), Kt = !1;
    var m = t.memoizedState;
    o.state = m, Qa(t, r, o, a), c = t.memoizedState, s !== r || m !== c || Xe.current || Kt ? (typeof j == "function" && (Fi(t, n, j, r), c = t.memoizedState), (s = Kt || Us(t, n, s, r, m, c, d)) ? (u || typeof o.UNSAFE_componentWillMount != "function" && typeof o.componentWillMount != "function" || (typeof o.componentWillMount == "function" && o.componentWillMount(), typeof o.UNSAFE_componentWillMount == "function" && o.UNSAFE_componentWillMount()), typeof o.componentDidMount == "function" && (t.flags |= 4194308)) : (typeof o.componentDidMount == "function" && (t.flags |= 4194308), t.memoizedProps = r, t.memoizedState = c), o.props = r, o.state = c, o.context = d, r = s) : (typeof o.componentDidMount == "function" && (t.flags |= 4194308), r = !1);
  } else {
    o = t.stateNode, Iu(e, t), s = t.memoizedProps, d = t.type === t.elementType ? s : yt(t.type, s), o.props = d, u = t.pendingProps, m = o.context, c = n.contextType, typeof c == "object" && c !== null ? c = ht(c) : (c = Ze(n) ? wn : Ue.current, c = Zn(t, c));
    var h = n.getDerivedStateFromProps;
    (j = typeof h == "function" || typeof o.getSnapshotBeforeUpdate == "function") || typeof o.UNSAFE_componentWillReceiveProps != "function" && typeof o.componentWillReceiveProps != "function" || (s !== u || m !== c) && Vs(t, o, r, c), Kt = !1, m = t.memoizedState, o.state = m, Qa(t, r, o, a);
    var g = t.memoizedState;
    s !== u || m !== g || Xe.current || Kt ? (typeof h == "function" && (Fi(t, n, h, r), g = t.memoizedState), (d = Kt || Us(t, n, d, r, m, g, c) || !1) ? (j || typeof o.UNSAFE_componentWillUpdate != "function" && typeof o.componentWillUpdate != "function" || (typeof o.componentWillUpdate == "function" && o.componentWillUpdate(r, g, c), typeof o.UNSAFE_componentWillUpdate == "function" && o.UNSAFE_componentWillUpdate(r, g, c)), typeof o.componentDidUpdate == "function" && (t.flags |= 4), typeof o.getSnapshotBeforeUpdate == "function" && (t.flags |= 1024)) : (typeof o.componentDidUpdate != "function" || s === e.memoizedProps && m === e.memoizedState || (t.flags |= 4), typeof o.getSnapshotBeforeUpdate != "function" || s === e.memoizedProps && m === e.memoizedState || (t.flags |= 1024), t.memoizedProps = r, t.memoizedState = g), o.props = r, o.state = g, o.context = c, r = d) : (typeof o.componentDidUpdate != "function" || s === e.memoizedProps && m === e.memoizedState || (t.flags |= 4), typeof o.getSnapshotBeforeUpdate != "function" || s === e.memoizedProps && m === e.memoizedState || (t.flags |= 1024), r = !1);
  }
  return Di(e, t, n, r, i, a);
}
function Di(e, t, n, r, a, i) {
  Ju(e, t);
  var o = (t.flags & 128) !== 0;
  if (!r && !o) return a && Ds(t, n, !1), Ht(e, t, i);
  r = t.stateNode, Up.current = t;
  var s = o && typeof n.getDerivedStateFromError != "function" ? null : r.render();
  return t.flags |= 1, e !== null && o ? (t.child = er(t, e.child, null, i), t.child = er(t, null, s, i)) : He(e, t, s, i), t.memoizedState = r.state, a && Ds(t, n, !0), t.child;
}
function ed(e) {
  var t = e.stateNode;
  t.pendingContext ? Ts(e, t.pendingContext, t.pendingContext !== t.context) : t.context && Ts(e, t.context, !1), No(e, t.containerInfo);
}
function qs(e, t, n, r, a) {
  return Jn(), ho(a), t.flags |= 256, He(e, t, n, r), t.child;
}
var zi = { dehydrated: null, treeContext: null, retryLane: 0 };
function Li(e) {
  return { baseLanes: e, cachePool: null, transitions: null };
}
function td(e, t, n) {
  var r = t.pendingProps, a = ue.current, i = !1, o = (t.flags & 128) !== 0, s;
  if ((s = o) || (s = e !== null && e.memoizedState === null ? !1 : (a & 2) !== 0), s ? (i = !0, t.flags &= -129) : (e === null || e.memoizedState !== null) && (a |= 1), ae(ue, a & 1), e === null)
    return Ii(t), e = t.memoizedState, e !== null && (e = e.dehydrated, e !== null) ? (t.mode & 1 ? e.data === "$!" ? t.lanes = 8 : t.lanes = 1073741824 : t.lanes = 1, null) : (o = r.children, e = r.fallback, i ? (r = t.mode, i = t.child, o = { mode: "hidden", children: o }, !(r & 1) && i !== null ? (i.childLanes = 0, i.pendingProps = o) : i = fl(o, r, 0, null), e = Nn(e, r, n, null), i.return = t, e.return = t, i.sibling = e, t.child = i, t.child.memoizedState = Li(n), t.memoizedState = zi, e) : Fo(t, o));
  if (a = e.memoizedState, a !== null && (s = a.dehydrated, s !== null)) return Vp(e, t, o, r, s, a, n);
  if (i) {
    i = r.fallback, o = t.mode, a = e.child, s = a.sibling;
    var c = { mode: "hidden", children: r.children };
    return !(o & 1) && t.child !== a ? (r = t.child, r.childLanes = 0, r.pendingProps = c, t.deletions = null) : (r = on(a, c), r.subtreeFlags = a.subtreeFlags & 14680064), s !== null ? i = on(s, i) : (i = Nn(i, o, n, null), i.flags |= 2), i.return = t, r.return = t, r.sibling = i, t.child = r, r = i, i = t.child, o = e.child.memoizedState, o = o === null ? Li(n) : { baseLanes: o.baseLanes | n, cachePool: null, transitions: o.transitions }, i.memoizedState = o, i.childLanes = e.childLanes & ~n, t.memoizedState = zi, r;
  }
  return i = e.child, e = i.sibling, r = on(i, { mode: "visible", children: r.children }), !(t.mode & 1) && (r.lanes = n), r.return = t, r.sibling = null, e !== null && (n = t.deletions, n === null ? (t.deletions = [e], t.flags |= 16) : n.push(e)), t.child = r, t.memoizedState = null, r;
}
function Fo(e, t) {
  return t = fl({ mode: "visible", children: t }, e.mode, 0, null), t.return = e, e.child = t;
}
function ha(e, t, n, r) {
  return r !== null && ho(r), er(t, e.child, null, n), e = Fo(t, t.pendingProps.children), e.flags |= 2, t.memoizedState = null, e;
}
function Vp(e, t, n, r, a, i, o) {
  if (n)
    return t.flags & 256 ? (t.flags &= -257, r = Ql(Error(I(422))), ha(e, t, o, r)) : t.memoizedState !== null ? (t.child = e.child, t.flags |= 128, null) : (i = r.fallback, a = t.mode, r = fl({ mode: "visible", children: r.children }, a, 0, null), i = Nn(i, a, o, null), i.flags |= 2, r.return = t, i.return = t, r.sibling = i, t.child = r, t.mode & 1 && er(t, e.child, null, o), t.child.memoizedState = Li(o), t.memoizedState = zi, i);
  if (!(t.mode & 1)) return ha(e, t, o, null);
  if (a.data === "$!") {
    if (r = a.nextSibling && a.nextSibling.dataset, r) var s = r.dgst;
    return r = s, i = Error(I(419)), r = Ql(i, r, void 0), ha(e, t, o, r);
  }
  if (s = (o & e.childLanes) !== 0, Ye || s) {
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
      a = a & (r.suspendedLanes | o) ? 0 : a, a !== 0 && a !== i.retryLane && (i.retryLane = a, Bt(e, a), wt(r, e, a, -1));
    }
    return Lo(), r = Ql(Error(I(421))), ha(e, t, o, r);
  }
  return a.data === "$?" ? (t.flags |= 128, t.child = e.child, t = tm.bind(null, e), a._reactRetry = t, null) : (e = i.treeContext, nt = nn(a.nextSibling), rt = t, se = !0, Nt = null, e !== null && (ut[dt++] = $t, ut[dt++] = Ot, ut[dt++] = Cn, $t = e.id, Ot = e.overflow, Cn = t), t = Fo(t, r.children), t.flags |= 4096, t);
}
function Ys(e, t, n) {
  e.lanes |= t;
  var r = e.alternate;
  r !== null && (r.lanes |= t), Pi(e.return, t, n);
}
function Gl(e, t, n, r, a) {
  var i = e.memoizedState;
  i === null ? e.memoizedState = { isBackwards: t, rendering: null, renderingStartTime: 0, last: r, tail: n, tailMode: a } : (i.isBackwards = t, i.rendering = null, i.renderingStartTime = 0, i.last = r, i.tail = n, i.tailMode = a);
}
function nd(e, t, n) {
  var r = t.pendingProps, a = r.revealOrder, i = r.tail;
  if (He(e, t, r.children, n), r = ue.current, r & 2) r = r & 1 | 2, t.flags |= 128;
  else {
    if (e !== null && e.flags & 128) e: for (e = t.child; e !== null; ) {
      if (e.tag === 13) e.memoizedState !== null && Ys(e, n, t);
      else if (e.tag === 19) Ys(e, n, t);
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
      for (n = t.child, a = null; n !== null; ) e = n.alternate, e !== null && Ga(e) === null && (a = n), n = n.sibling;
      n = a, n === null ? (a = t.child, t.child = null) : (a = n.sibling, n.sibling = null), Gl(t, !1, a, n, i);
      break;
    case "backwards":
      for (n = null, a = t.child, t.child = null; a !== null; ) {
        if (e = a.alternate, e !== null && Ga(e) === null) {
          t.child = a;
          break;
        }
        e = a.sibling, a.sibling = n, n = a, a = e;
      }
      Gl(t, !0, n, null, i);
      break;
    case "together":
      Gl(t, !1, null, null, void 0);
      break;
    default:
      t.memoizedState = null;
  }
  return t.child;
}
function Ia(e, t) {
  !(t.mode & 1) && e !== null && (e.alternate = null, t.alternate = null, t.flags |= 2);
}
function Ht(e, t, n) {
  if (e !== null && (t.dependencies = e.dependencies), En |= t.lanes, !(n & t.childLanes)) return null;
  if (e !== null && t.child !== e.child) throw Error(I(153));
  if (t.child !== null) {
    for (e = t.child, n = on(e, e.pendingProps), t.child = n, n.return = t; e.sibling !== null; ) e = e.sibling, n = n.sibling = on(e, e.pendingProps), n.return = t;
    n.sibling = null;
  }
  return t.child;
}
function Bp(e, t, n) {
  switch (t.tag) {
    case 3:
      ed(t), Jn();
      break;
    case 5:
      Pu(t);
      break;
    case 1:
      Ze(t.type) && Ua(t);
      break;
    case 4:
      No(t, t.stateNode.containerInfo);
      break;
    case 10:
      var r = t.type._context, a = t.memoizedProps.value;
      ae(Ha, r._currentValue), r._currentValue = a;
      break;
    case 13:
      if (r = t.memoizedState, r !== null)
        return r.dehydrated !== null ? (ae(ue, ue.current & 1), t.flags |= 128, null) : n & t.child.childLanes ? td(e, t, n) : (ae(ue, ue.current & 1), e = Ht(e, t, n), e !== null ? e.sibling : null);
      ae(ue, ue.current & 1);
      break;
    case 19:
      if (r = (n & t.childLanes) !== 0, e.flags & 128) {
        if (r) return nd(e, t, n);
        t.flags |= 128;
      }
      if (a = t.memoizedState, a !== null && (a.rendering = null, a.tail = null, a.lastEffect = null), ae(ue, ue.current), r) break;
      return null;
    case 22:
    case 23:
      return t.lanes = 0, Zu(e, t, n);
  }
  return Ht(e, t, n);
}
var rd, Ai, ad, ld;
rd = function(e, t) {
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
Ai = function() {
};
ad = function(e, t, n, r) {
  var a = e.memoizedProps;
  if (a !== r) {
    e = t.stateNode, yn(Tt.current);
    var i = null;
    switch (n) {
      case "input":
        a = ai(e, a), r = ai(e, r), i = [];
        break;
      case "select":
        a = fe({}, a, { value: void 0 }), r = fe({}, r, { value: void 0 }), i = [];
        break;
      case "textarea":
        a = oi(e, a), r = oi(e, r), i = [];
        break;
      default:
        typeof a.onClick != "function" && typeof r.onClick == "function" && (e.onclick = Oa);
    }
    ci(n, r);
    var o;
    n = null;
    for (d in a) if (!r.hasOwnProperty(d) && a.hasOwnProperty(d) && a[d] != null) if (d === "style") {
      var s = a[d];
      for (o in s) s.hasOwnProperty(o) && (n || (n = {}), n[o] = "");
    } else d !== "dangerouslySetInnerHTML" && d !== "children" && d !== "suppressContentEditableWarning" && d !== "suppressHydrationWarning" && d !== "autoFocus" && (Rr.hasOwnProperty(d) ? i || (i = []) : (i = i || []).push(d, null));
    for (d in r) {
      var c = r[d];
      if (s = a != null ? a[d] : void 0, r.hasOwnProperty(d) && c !== s && (c != null || s != null)) if (d === "style") if (s) {
        for (o in s) !s.hasOwnProperty(o) || c && c.hasOwnProperty(o) || (n || (n = {}), n[o] = "");
        for (o in c) c.hasOwnProperty(o) && s[o] !== c[o] && (n || (n = {}), n[o] = c[o]);
      } else n || (i || (i = []), i.push(
        d,
        n
      )), n = c;
      else d === "dangerouslySetInnerHTML" ? (c = c ? c.__html : void 0, s = s ? s.__html : void 0, c != null && s !== c && (i = i || []).push(d, c)) : d === "children" ? typeof c != "string" && typeof c != "number" || (i = i || []).push(d, "" + c) : d !== "suppressContentEditableWarning" && d !== "suppressHydrationWarning" && (Rr.hasOwnProperty(d) ? (c != null && d === "onScroll" && ie("scroll", e), i || s === c || (i = [])) : (i = i || []).push(d, c));
    }
    n && (i = i || []).push("style", n);
    var d = i;
    (t.updateQueue = d) && (t.flags |= 4);
  }
};
ld = function(e, t, n, r) {
  n !== r && (t.flags |= 4);
};
function mr(e, t) {
  if (!se) switch (e.tailMode) {
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
function Me(e) {
  var t = e.alternate !== null && e.alternate.child === e.child, n = 0, r = 0;
  if (t) for (var a = e.child; a !== null; ) n |= a.lanes | a.childLanes, r |= a.subtreeFlags & 14680064, r |= a.flags & 14680064, a.return = e, a = a.sibling;
  else for (a = e.child; a !== null; ) n |= a.lanes | a.childLanes, r |= a.subtreeFlags, r |= a.flags, a.return = e, a = a.sibling;
  return e.subtreeFlags |= r, e.childLanes = n, t;
}
function Hp(e, t, n) {
  var r = t.pendingProps;
  switch (mo(t), t.tag) {
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
      return Me(t), null;
    case 1:
      return Ze(t.type) && ba(), Me(t), null;
    case 3:
      return r = t.stateNode, tr(), oe(Xe), oe(Ue), wo(), r.pendingContext && (r.context = r.pendingContext, r.pendingContext = null), (e === null || e.child === null) && (pa(t) ? t.flags |= 4 : e === null || e.memoizedState.isDehydrated && !(t.flags & 256) || (t.flags |= 1024, Nt !== null && (Hi(Nt), Nt = null))), Ai(e, t), Me(t), null;
    case 5:
      So(t);
      var a = yn(Vr.current);
      if (n = t.type, e !== null && t.stateNode != null) ad(e, t, n, r, a), e.ref !== t.ref && (t.flags |= 512, t.flags |= 2097152);
      else {
        if (!r) {
          if (t.stateNode === null) throw Error(I(166));
          return Me(t), null;
        }
        if (e = yn(Tt.current), pa(t)) {
          r = t.stateNode, n = t.type;
          var i = t.memoizedProps;
          switch (r[Rt] = t, r[br] = i, e = (t.mode & 1) !== 0, n) {
            case "dialog":
              ie("cancel", r), ie("close", r);
              break;
            case "iframe":
            case "object":
            case "embed":
              ie("load", r);
              break;
            case "video":
            case "audio":
              for (a = 0; a < yr.length; a++) ie(yr[a], r);
              break;
            case "source":
              ie("error", r);
              break;
            case "img":
            case "image":
            case "link":
              ie(
                "error",
                r
              ), ie("load", r);
              break;
            case "details":
              ie("toggle", r);
              break;
            case "input":
              ls(r, i), ie("invalid", r);
              break;
            case "select":
              r._wrapperState = { wasMultiple: !!i.multiple }, ie("invalid", r);
              break;
            case "textarea":
              os(r, i), ie("invalid", r);
          }
          ci(n, i), a = null;
          for (var o in i) if (i.hasOwnProperty(o)) {
            var s = i[o];
            o === "children" ? typeof s == "string" ? r.textContent !== s && (i.suppressHydrationWarning !== !0 && fa(r.textContent, s, e), a = ["children", s]) : typeof s == "number" && r.textContent !== "" + s && (i.suppressHydrationWarning !== !0 && fa(
              r.textContent,
              s,
              e
            ), a = ["children", "" + s]) : Rr.hasOwnProperty(o) && s != null && o === "onScroll" && ie("scroll", r);
          }
          switch (n) {
            case "input":
              aa(r), is(r, i, !0);
              break;
            case "textarea":
              aa(r), ss(r);
              break;
            case "select":
            case "option":
              break;
            default:
              typeof i.onClick == "function" && (r.onclick = Oa);
          }
          r = a, t.updateQueue = r, r !== null && (t.flags |= 4);
        } else {
          o = a.nodeType === 9 ? a : a.ownerDocument, e === "http://www.w3.org/1999/xhtml" && (e = Dc(n)), e === "http://www.w3.org/1999/xhtml" ? n === "script" ? (e = o.createElement("div"), e.innerHTML = "<script><\/script>", e = e.removeChild(e.firstChild)) : typeof r.is == "string" ? e = o.createElement(n, { is: r.is }) : (e = o.createElement(n), n === "select" && (o = e, r.multiple ? o.multiple = !0 : r.size && (o.size = r.size))) : e = o.createElementNS(e, n), e[Rt] = t, e[br] = r, rd(e, t, !1, !1), t.stateNode = e;
          e: {
            switch (o = ui(n, r), n) {
              case "dialog":
                ie("cancel", e), ie("close", e), a = r;
                break;
              case "iframe":
              case "object":
              case "embed":
                ie("load", e), a = r;
                break;
              case "video":
              case "audio":
                for (a = 0; a < yr.length; a++) ie(yr[a], e);
                a = r;
                break;
              case "source":
                ie("error", e), a = r;
                break;
              case "img":
              case "image":
              case "link":
                ie(
                  "error",
                  e
                ), ie("load", e), a = r;
                break;
              case "details":
                ie("toggle", e), a = r;
                break;
              case "input":
                ls(e, r), a = ai(e, r), ie("invalid", e);
                break;
              case "option":
                a = r;
                break;
              case "select":
                e._wrapperState = { wasMultiple: !!r.multiple }, a = fe({}, r, { value: void 0 }), ie("invalid", e);
                break;
              case "textarea":
                os(e, r), a = oi(e, r), ie("invalid", e);
                break;
              default:
                a = r;
            }
            ci(n, a), s = a;
            for (i in s) if (s.hasOwnProperty(i)) {
              var c = s[i];
              i === "style" ? Ac(e, c) : i === "dangerouslySetInnerHTML" ? (c = c ? c.__html : void 0, c != null && zc(e, c)) : i === "children" ? typeof c == "string" ? (n !== "textarea" || c !== "") && _r(e, c) : typeof c == "number" && _r(e, "" + c) : i !== "suppressContentEditableWarning" && i !== "suppressHydrationWarning" && i !== "autoFocus" && (Rr.hasOwnProperty(i) ? c != null && i === "onScroll" && ie("scroll", e) : c != null && Zi(e, i, c, o));
            }
            switch (n) {
              case "input":
                aa(e), is(e, r, !1);
                break;
              case "textarea":
                aa(e), ss(e);
                break;
              case "option":
                r.value != null && e.setAttribute("value", "" + sn(r.value));
                break;
              case "select":
                e.multiple = !!r.multiple, i = r.value, i != null ? Hn(e, !!r.multiple, i, !1) : r.defaultValue != null && Hn(
                  e,
                  !!r.multiple,
                  r.defaultValue,
                  !0
                );
                break;
              default:
                typeof a.onClick == "function" && (e.onclick = Oa);
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
      return Me(t), null;
    case 6:
      if (e && t.stateNode != null) ld(e, t, e.memoizedProps, r);
      else {
        if (typeof r != "string" && t.stateNode === null) throw Error(I(166));
        if (n = yn(Vr.current), yn(Tt.current), pa(t)) {
          if (r = t.stateNode, n = t.memoizedProps, r[Rt] = t, (i = r.nodeValue !== n) && (e = rt, e !== null)) switch (e.tag) {
            case 3:
              fa(r.nodeValue, n, (e.mode & 1) !== 0);
              break;
            case 5:
              e.memoizedProps.suppressHydrationWarning !== !0 && fa(r.nodeValue, n, (e.mode & 1) !== 0);
          }
          i && (t.flags |= 4);
        } else r = (n.nodeType === 9 ? n : n.ownerDocument).createTextNode(r), r[Rt] = t, t.stateNode = r;
      }
      return Me(t), null;
    case 13:
      if (oe(ue), r = t.memoizedState, e === null || e.memoizedState !== null && e.memoizedState.dehydrated !== null) {
        if (se && nt !== null && t.mode & 1 && !(t.flags & 128)) wu(), Jn(), t.flags |= 98560, i = !1;
        else if (i = pa(t), r !== null && r.dehydrated !== null) {
          if (e === null) {
            if (!i) throw Error(I(318));
            if (i = t.memoizedState, i = i !== null ? i.dehydrated : null, !i) throw Error(I(317));
            i[Rt] = t;
          } else Jn(), !(t.flags & 128) && (t.memoizedState = null), t.flags |= 4;
          Me(t), i = !1;
        } else Nt !== null && (Hi(Nt), Nt = null), i = !0;
        if (!i) return t.flags & 65536 ? t : null;
      }
      return t.flags & 128 ? (t.lanes = n, t) : (r = r !== null, r !== (e !== null && e.memoizedState !== null) && r && (t.child.flags |= 8192, t.mode & 1 && (e === null || ue.current & 1 ? Ee === 0 && (Ee = 3) : Lo())), t.updateQueue !== null && (t.flags |= 4), Me(t), null);
    case 4:
      return tr(), Ai(e, t), e === null && $r(t.stateNode.containerInfo), Me(t), null;
    case 10:
      return go(t.type._context), Me(t), null;
    case 17:
      return Ze(t.type) && ba(), Me(t), null;
    case 19:
      if (oe(ue), i = t.memoizedState, i === null) return Me(t), null;
      if (r = (t.flags & 128) !== 0, o = i.rendering, o === null) if (r) mr(i, !1);
      else {
        if (Ee !== 0 || e !== null && e.flags & 128) for (e = t.child; e !== null; ) {
          if (o = Ga(e), o !== null) {
            for (t.flags |= 128, mr(i, !1), r = o.updateQueue, r !== null && (t.updateQueue = r, t.flags |= 4), t.subtreeFlags = 0, r = n, n = t.child; n !== null; ) i = n, e = r, i.flags &= 14680066, o = i.alternate, o === null ? (i.childLanes = 0, i.lanes = e, i.child = null, i.subtreeFlags = 0, i.memoizedProps = null, i.memoizedState = null, i.updateQueue = null, i.dependencies = null, i.stateNode = null) : (i.childLanes = o.childLanes, i.lanes = o.lanes, i.child = o.child, i.subtreeFlags = 0, i.deletions = null, i.memoizedProps = o.memoizedProps, i.memoizedState = o.memoizedState, i.updateQueue = o.updateQueue, i.type = o.type, e = o.dependencies, i.dependencies = e === null ? null : { lanes: e.lanes, firstContext: e.firstContext }), n = n.sibling;
            return ae(ue, ue.current & 1 | 2), t.child;
          }
          e = e.sibling;
        }
        i.tail !== null && ye() > rr && (t.flags |= 128, r = !0, mr(i, !1), t.lanes = 4194304);
      }
      else {
        if (!r) if (e = Ga(o), e !== null) {
          if (t.flags |= 128, r = !0, n = e.updateQueue, n !== null && (t.updateQueue = n, t.flags |= 4), mr(i, !0), i.tail === null && i.tailMode === "hidden" && !o.alternate && !se) return Me(t), null;
        } else 2 * ye() - i.renderingStartTime > rr && n !== 1073741824 && (t.flags |= 128, r = !0, mr(i, !1), t.lanes = 4194304);
        i.isBackwards ? (o.sibling = t.child, t.child = o) : (n = i.last, n !== null ? n.sibling = o : t.child = o, i.last = o);
      }
      return i.tail !== null ? (t = i.tail, i.rendering = t, i.tail = t.sibling, i.renderingStartTime = ye(), t.sibling = null, n = ue.current, ae(ue, r ? n & 1 | 2 : n & 1), t) : (Me(t), null);
    case 22:
    case 23:
      return zo(), r = t.memoizedState !== null, e !== null && e.memoizedState !== null !== r && (t.flags |= 8192), r && t.mode & 1 ? tt & 1073741824 && (Me(t), t.subtreeFlags & 6 && (t.flags |= 8192)) : Me(t), null;
    case 24:
      return null;
    case 25:
      return null;
  }
  throw Error(I(156, t.tag));
}
function Wp(e, t) {
  switch (mo(t), t.tag) {
    case 1:
      return Ze(t.type) && ba(), e = t.flags, e & 65536 ? (t.flags = e & -65537 | 128, t) : null;
    case 3:
      return tr(), oe(Xe), oe(Ue), wo(), e = t.flags, e & 65536 && !(e & 128) ? (t.flags = e & -65537 | 128, t) : null;
    case 5:
      return So(t), null;
    case 13:
      if (oe(ue), e = t.memoizedState, e !== null && e.dehydrated !== null) {
        if (t.alternate === null) throw Error(I(340));
        Jn();
      }
      return e = t.flags, e & 65536 ? (t.flags = e & -65537 | 128, t) : null;
    case 19:
      return oe(ue), null;
    case 4:
      return tr(), null;
    case 10:
      return go(t.type._context), null;
    case 22:
    case 23:
      return zo(), null;
    case 24:
      return null;
    default:
      return null;
  }
}
var va = !1, $e = !1, Qp = typeof WeakSet == "function" ? WeakSet : Set, O = null;
function Vn(e, t) {
  var n = e.ref;
  if (n !== null) if (typeof n == "function") try {
    n(null);
  } catch (r) {
    ve(e, t, r);
  }
  else n.current = null;
}
function Mi(e, t, n) {
  try {
    n();
  } catch (r) {
    ve(e, t, r);
  }
}
var Xs = !1;
function Gp(e, t) {
  if (ji = Aa, e = uu(), fo(e)) {
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
        var o = 0, s = -1, c = -1, d = 0, j = 0, u = e, m = null;
        t: for (; ; ) {
          for (var h; u !== n || a !== 0 && u.nodeType !== 3 || (s = o + a), u !== i || r !== 0 && u.nodeType !== 3 || (c = o + r), u.nodeType === 3 && (o += u.nodeValue.length), (h = u.firstChild) !== null; )
            m = u, u = h;
          for (; ; ) {
            if (u === e) break t;
            if (m === n && ++d === a && (s = o), m === i && ++j === r && (c = o), (h = u.nextSibling) !== null) break;
            u = m, m = u.parentNode;
          }
          u = h;
        }
        n = s === -1 || c === -1 ? null : { start: s, end: c };
      } else n = null;
    }
    n = n || { start: 0, end: 0 };
  } else n = null;
  for (Ni = { focusedElem: e, selectionRange: n }, Aa = !1, O = t; O !== null; ) if (t = O, e = t.child, (t.subtreeFlags & 1028) !== 0 && e !== null) e.return = t, O = e;
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
            var w = g.memoizedProps, $ = g.memoizedState, p = t.stateNode, f = p.getSnapshotBeforeUpdate(t.elementType === t.type ? w : yt(t.type, w), $);
            p.__reactInternalSnapshotBeforeUpdate = f;
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
          throw Error(I(163));
      }
    } catch (k) {
      ve(t, t.return, k);
    }
    if (e = t.sibling, e !== null) {
      e.return = t.return, O = e;
      break;
    }
    O = t.return;
  }
  return g = Xs, Xs = !1, g;
}
function Er(e, t, n) {
  var r = t.updateQueue;
  if (r = r !== null ? r.lastEffect : null, r !== null) {
    var a = r = r.next;
    do {
      if ((a.tag & e) === e) {
        var i = a.destroy;
        a.destroy = void 0, i !== void 0 && Mi(t, n, i);
      }
      a = a.next;
    } while (a !== r);
  }
}
function ul(e, t) {
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
function $i(e) {
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
function id(e) {
  var t = e.alternate;
  t !== null && (e.alternate = null, id(t)), e.child = null, e.deletions = null, e.sibling = null, e.tag === 5 && (t = e.stateNode, t !== null && (delete t[Rt], delete t[br], delete t[Ci], delete t[Fp], delete t[Rp])), e.stateNode = null, e.return = null, e.dependencies = null, e.memoizedProps = null, e.memoizedState = null, e.pendingProps = null, e.stateNode = null, e.updateQueue = null;
}
function od(e) {
  return e.tag === 5 || e.tag === 3 || e.tag === 4;
}
function Zs(e) {
  e: for (; ; ) {
    for (; e.sibling === null; ) {
      if (e.return === null || od(e.return)) return null;
      e = e.return;
    }
    for (e.sibling.return = e.return, e = e.sibling; e.tag !== 5 && e.tag !== 6 && e.tag !== 18; ) {
      if (e.flags & 2 || e.child === null || e.tag === 4) continue e;
      e.child.return = e, e = e.child;
    }
    if (!(e.flags & 2)) return e.stateNode;
  }
}
function Oi(e, t, n) {
  var r = e.tag;
  if (r === 5 || r === 6) e = e.stateNode, t ? n.nodeType === 8 ? n.parentNode.insertBefore(e, t) : n.insertBefore(e, t) : (n.nodeType === 8 ? (t = n.parentNode, t.insertBefore(e, n)) : (t = n, t.appendChild(e)), n = n._reactRootContainer, n != null || t.onclick !== null || (t.onclick = Oa));
  else if (r !== 4 && (e = e.child, e !== null)) for (Oi(e, t, n), e = e.sibling; e !== null; ) Oi(e, t, n), e = e.sibling;
}
function bi(e, t, n) {
  var r = e.tag;
  if (r === 5 || r === 6) e = e.stateNode, t ? n.insertBefore(e, t) : n.appendChild(e);
  else if (r !== 4 && (e = e.child, e !== null)) for (bi(e, t, n), e = e.sibling; e !== null; ) bi(e, t, n), e = e.sibling;
}
var Te = null, jt = !1;
function Qt(e, t, n) {
  for (n = n.child; n !== null; ) sd(e, t, n), n = n.sibling;
}
function sd(e, t, n) {
  if (_t && typeof _t.onCommitFiberUnmount == "function") try {
    _t.onCommitFiberUnmount(nl, n);
  } catch {
  }
  switch (n.tag) {
    case 5:
      $e || Vn(n, t);
    case 6:
      var r = Te, a = jt;
      Te = null, Qt(e, t, n), Te = r, jt = a, Te !== null && (jt ? (e = Te, n = n.stateNode, e.nodeType === 8 ? e.parentNode.removeChild(n) : e.removeChild(n)) : Te.removeChild(n.stateNode));
      break;
    case 18:
      Te !== null && (jt ? (e = Te, n = n.stateNode, e.nodeType === 8 ? bl(e.parentNode, n) : e.nodeType === 1 && bl(e, n), Lr(e)) : bl(Te, n.stateNode));
      break;
    case 4:
      r = Te, a = jt, Te = n.stateNode.containerInfo, jt = !0, Qt(e, t, n), Te = r, jt = a;
      break;
    case 0:
    case 11:
    case 14:
    case 15:
      if (!$e && (r = n.updateQueue, r !== null && (r = r.lastEffect, r !== null))) {
        a = r = r.next;
        do {
          var i = a, o = i.destroy;
          i = i.tag, o !== void 0 && (i & 2 || i & 4) && Mi(n, t, o), a = a.next;
        } while (a !== r);
      }
      Qt(e, t, n);
      break;
    case 1:
      if (!$e && (Vn(n, t), r = n.stateNode, typeof r.componentWillUnmount == "function")) try {
        r.props = n.memoizedProps, r.state = n.memoizedState, r.componentWillUnmount();
      } catch (s) {
        ve(n, t, s);
      }
      Qt(e, t, n);
      break;
    case 21:
      Qt(e, t, n);
      break;
    case 22:
      n.mode & 1 ? ($e = (r = $e) || n.memoizedState !== null, Qt(e, t, n), $e = r) : Qt(e, t, n);
      break;
    default:
      Qt(e, t, n);
  }
}
function Js(e) {
  var t = e.updateQueue;
  if (t !== null) {
    e.updateQueue = null;
    var n = e.stateNode;
    n === null && (n = e.stateNode = new Qp()), t.forEach(function(r) {
      var a = nm.bind(null, e, r);
      n.has(r) || (n.add(r), r.then(a, a));
    });
  }
}
function gt(e, t) {
  var n = t.deletions;
  if (n !== null) for (var r = 0; r < n.length; r++) {
    var a = n[r];
    try {
      var i = e, o = t, s = o;
      e: for (; s !== null; ) {
        switch (s.tag) {
          case 5:
            Te = s.stateNode, jt = !1;
            break e;
          case 3:
            Te = s.stateNode.containerInfo, jt = !0;
            break e;
          case 4:
            Te = s.stateNode.containerInfo, jt = !0;
            break e;
        }
        s = s.return;
      }
      if (Te === null) throw Error(I(160));
      sd(i, o, a), Te = null, jt = !1;
      var c = a.alternate;
      c !== null && (c.return = null), a.return = null;
    } catch (d) {
      ve(a, t, d);
    }
  }
  if (t.subtreeFlags & 12854) for (t = t.child; t !== null; ) cd(t, e), t = t.sibling;
}
function cd(e, t) {
  var n = e.alternate, r = e.flags;
  switch (e.tag) {
    case 0:
    case 11:
    case 14:
    case 15:
      if (gt(t, e), Pt(e), r & 4) {
        try {
          Er(3, e, e.return), ul(3, e);
        } catch (w) {
          ve(e, e.return, w);
        }
        try {
          Er(5, e, e.return);
        } catch (w) {
          ve(e, e.return, w);
        }
      }
      break;
    case 1:
      gt(t, e), Pt(e), r & 512 && n !== null && Vn(n, n.return);
      break;
    case 5:
      if (gt(t, e), Pt(e), r & 512 && n !== null && Vn(n, n.return), e.flags & 32) {
        var a = e.stateNode;
        try {
          _r(a, "");
        } catch (w) {
          ve(e, e.return, w);
        }
      }
      if (r & 4 && (a = e.stateNode, a != null)) {
        var i = e.memoizedProps, o = n !== null ? n.memoizedProps : i, s = e.type, c = e.updateQueue;
        if (e.updateQueue = null, c !== null) try {
          s === "input" && i.type === "radio" && i.name != null && _c(a, i), ui(s, o);
          var d = ui(s, i);
          for (o = 0; o < c.length; o += 2) {
            var j = c[o], u = c[o + 1];
            j === "style" ? Ac(a, u) : j === "dangerouslySetInnerHTML" ? zc(a, u) : j === "children" ? _r(a, u) : Zi(a, j, u, d);
          }
          switch (s) {
            case "input":
              li(a, i);
              break;
            case "textarea":
              Tc(a, i);
              break;
            case "select":
              var m = a._wrapperState.wasMultiple;
              a._wrapperState.wasMultiple = !!i.multiple;
              var h = i.value;
              h != null ? Hn(a, !!i.multiple, h, !1) : m !== !!i.multiple && (i.defaultValue != null ? Hn(
                a,
                !!i.multiple,
                i.defaultValue,
                !0
              ) : Hn(a, !!i.multiple, i.multiple ? [] : "", !1));
          }
          a[br] = i;
        } catch (w) {
          ve(e, e.return, w);
        }
      }
      break;
    case 6:
      if (gt(t, e), Pt(e), r & 4) {
        if (e.stateNode === null) throw Error(I(162));
        a = e.stateNode, i = e.memoizedProps;
        try {
          a.nodeValue = i;
        } catch (w) {
          ve(e, e.return, w);
        }
      }
      break;
    case 3:
      if (gt(t, e), Pt(e), r & 4 && n !== null && n.memoizedState.isDehydrated) try {
        Lr(t.containerInfo);
      } catch (w) {
        ve(e, e.return, w);
      }
      break;
    case 4:
      gt(t, e), Pt(e);
      break;
    case 13:
      gt(t, e), Pt(e), a = e.child, a.flags & 8192 && (i = a.memoizedState !== null, a.stateNode.isHidden = i, !i || a.alternate !== null && a.alternate.memoizedState !== null || (To = ye())), r & 4 && Js(e);
      break;
    case 22:
      if (j = n !== null && n.memoizedState !== null, e.mode & 1 ? ($e = (d = $e) || j, gt(t, e), $e = d) : gt(t, e), Pt(e), r & 8192) {
        if (d = e.memoizedState !== null, (e.stateNode.isHidden = d) && !j && e.mode & 1) for (O = e, j = e.child; j !== null; ) {
          for (u = O = j; O !== null; ) {
            switch (m = O, h = m.child, m.tag) {
              case 0:
              case 11:
              case 14:
              case 15:
                Er(4, m, m.return);
                break;
              case 1:
                Vn(m, m.return);
                var g = m.stateNode;
                if (typeof g.componentWillUnmount == "function") {
                  r = m, n = m.return;
                  try {
                    t = r, g.props = t.memoizedProps, g.state = t.memoizedState, g.componentWillUnmount();
                  } catch (w) {
                    ve(r, n, w);
                  }
                }
                break;
              case 5:
                Vn(m, m.return);
                break;
              case 22:
                if (m.memoizedState !== null) {
                  tc(u);
                  continue;
                }
            }
            h !== null ? (h.return = m, O = h) : tc(u);
          }
          j = j.sibling;
        }
        e: for (j = null, u = e; ; ) {
          if (u.tag === 5) {
            if (j === null) {
              j = u;
              try {
                a = u.stateNode, d ? (i = a.style, typeof i.setProperty == "function" ? i.setProperty("display", "none", "important") : i.display = "none") : (s = u.stateNode, c = u.memoizedProps.style, o = c != null && c.hasOwnProperty("display") ? c.display : null, s.style.display = Lc("display", o));
              } catch (w) {
                ve(e, e.return, w);
              }
            }
          } else if (u.tag === 6) {
            if (j === null) try {
              u.stateNode.nodeValue = d ? "" : u.memoizedProps;
            } catch (w) {
              ve(e, e.return, w);
            }
          } else if ((u.tag !== 22 && u.tag !== 23 || u.memoizedState === null || u === e) && u.child !== null) {
            u.child.return = u, u = u.child;
            continue;
          }
          if (u === e) break e;
          for (; u.sibling === null; ) {
            if (u.return === null || u.return === e) break e;
            j === u && (j = null), u = u.return;
          }
          j === u && (j = null), u.sibling.return = u.return, u = u.sibling;
        }
      }
      break;
    case 19:
      gt(t, e), Pt(e), r & 4 && Js(e);
      break;
    case 21:
      break;
    default:
      gt(
        t,
        e
      ), Pt(e);
  }
}
function Pt(e) {
  var t = e.flags;
  if (t & 2) {
    try {
      e: {
        for (var n = e.return; n !== null; ) {
          if (od(n)) {
            var r = n;
            break e;
          }
          n = n.return;
        }
        throw Error(I(160));
      }
      switch (r.tag) {
        case 5:
          var a = r.stateNode;
          r.flags & 32 && (_r(a, ""), r.flags &= -33);
          var i = Zs(e);
          bi(e, i, a);
          break;
        case 3:
        case 4:
          var o = r.stateNode.containerInfo, s = Zs(e);
          Oi(e, s, o);
          break;
        default:
          throw Error(I(161));
      }
    } catch (c) {
      ve(e, e.return, c);
    }
    e.flags &= -3;
  }
  t & 4096 && (e.flags &= -4097);
}
function Kp(e, t, n) {
  O = e, ud(e);
}
function ud(e, t, n) {
  for (var r = (e.mode & 1) !== 0; O !== null; ) {
    var a = O, i = a.child;
    if (a.tag === 22 && r) {
      var o = a.memoizedState !== null || va;
      if (!o) {
        var s = a.alternate, c = s !== null && s.memoizedState !== null || $e;
        s = va;
        var d = $e;
        if (va = o, ($e = c) && !d) for (O = a; O !== null; ) o = O, c = o.child, o.tag === 22 && o.memoizedState !== null ? nc(a) : c !== null ? (c.return = o, O = c) : nc(a);
        for (; i !== null; ) O = i, ud(i), i = i.sibling;
        O = a, va = s, $e = d;
      }
      ec(e);
    } else a.subtreeFlags & 8772 && i !== null ? (i.return = a, O = i) : ec(e);
  }
}
function ec(e) {
  for (; O !== null; ) {
    var t = O;
    if (t.flags & 8772) {
      var n = t.alternate;
      try {
        if (t.flags & 8772) switch (t.tag) {
          case 0:
          case 11:
          case 15:
            $e || ul(5, t);
            break;
          case 1:
            var r = t.stateNode;
            if (t.flags & 4 && !$e) if (n === null) r.componentDidMount();
            else {
              var a = t.elementType === t.type ? n.memoizedProps : yt(t.type, n.memoizedProps);
              r.componentDidUpdate(a, n.memoizedState, r.__reactInternalSnapshotBeforeUpdate);
            }
            var i = t.updateQueue;
            i !== null && $s(t, i, r);
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
              $s(t, o, n);
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
              var d = t.alternate;
              if (d !== null) {
                var j = d.memoizedState;
                if (j !== null) {
                  var u = j.dehydrated;
                  u !== null && Lr(u);
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
            throw Error(I(163));
        }
        $e || t.flags & 512 && $i(t);
      } catch (m) {
        ve(t, t.return, m);
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
function tc(e) {
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
function nc(e) {
  for (; O !== null; ) {
    var t = O;
    try {
      switch (t.tag) {
        case 0:
        case 11:
        case 15:
          var n = t.return;
          try {
            ul(4, t);
          } catch (c) {
            ve(t, n, c);
          }
          break;
        case 1:
          var r = t.stateNode;
          if (typeof r.componentDidMount == "function") {
            var a = t.return;
            try {
              r.componentDidMount();
            } catch (c) {
              ve(t, a, c);
            }
          }
          var i = t.return;
          try {
            $i(t);
          } catch (c) {
            ve(t, i, c);
          }
          break;
        case 5:
          var o = t.return;
          try {
            $i(t);
          } catch (c) {
            ve(t, o, c);
          }
      }
    } catch (c) {
      ve(t, t.return, c);
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
var qp = Math.ceil, Ya = Wt.ReactCurrentDispatcher, Ro = Wt.ReactCurrentOwner, mt = Wt.ReactCurrentBatchConfig, J = 0, Fe = null, Ne = null, De = 0, tt = 0, Bn = fn(0), Ee = 0, Qr = null, En = 0, dl = 0, _o = 0, Ir = null, qe = null, To = 0, rr = 1 / 0, At = null, Xa = !1, Ui = null, an = null, xa = !1, Zt = null, Za = 0, Pr = 0, Vi = null, Pa = -1, Fa = 0;
function Qe() {
  return J & 6 ? ye() : Pa !== -1 ? Pa : Pa = ye();
}
function ln(e) {
  return e.mode & 1 ? J & 2 && De !== 0 ? De & -De : Tp.transition !== null ? (Fa === 0 && (Fa = Kc()), Fa) : (e = ne, e !== 0 || (e = window.event, e = e === void 0 ? 16 : tu(e.type)), e) : 1;
}
function wt(e, t, n, r) {
  if (50 < Pr) throw Pr = 0, Vi = null, Error(I(185));
  qr(e, n, r), (!(J & 2) || e !== Fe) && (e === Fe && (!(J & 2) && (dl |= n), Ee === 4 && Yt(e, De)), Je(e, r), n === 1 && J === 0 && !(t.mode & 1) && (rr = ye() + 500, ol && pn()));
}
function Je(e, t) {
  var n = e.callbackNode;
  Tf(e, t);
  var r = La(e, e === Fe ? De : 0);
  if (r === 0) n !== null && ds(n), e.callbackNode = null, e.callbackPriority = 0;
  else if (t = r & -r, e.callbackPriority !== t) {
    if (n != null && ds(n), t === 1) e.tag === 0 ? _p(rc.bind(null, e)) : ju(rc.bind(null, e)), Ip(function() {
      !(J & 6) && pn();
    }), n = null;
    else {
      switch (qc(r)) {
        case 1:
          n = ro;
          break;
        case 4:
          n = Qc;
          break;
        case 16:
          n = za;
          break;
        case 536870912:
          n = Gc;
          break;
        default:
          n = za;
      }
      n = gd(n, dd.bind(null, e));
    }
    e.callbackPriority = t, e.callbackNode = n;
  }
}
function dd(e, t) {
  if (Pa = -1, Fa = 0, J & 6) throw Error(I(327));
  var n = e.callbackNode;
  if (qn() && e.callbackNode !== n) return null;
  var r = La(e, e === Fe ? De : 0);
  if (r === 0) return null;
  if (r & 30 || r & e.expiredLanes || t) t = Ja(e, r);
  else {
    t = r;
    var a = J;
    J |= 2;
    var i = pd();
    (Fe !== e || De !== t) && (At = null, rr = ye() + 500, jn(e, t));
    do
      try {
        Zp();
        break;
      } catch (s) {
        fd(e, s);
      }
    while (!0);
    xo(), Ya.current = i, J = a, Ne !== null ? t = 0 : (Fe = null, De = 0, t = Ee);
  }
  if (t !== 0) {
    if (t === 2 && (a = hi(e), a !== 0 && (r = a, t = Bi(e, a))), t === 1) throw n = Qr, jn(e, 0), Yt(e, r), Je(e, ye()), n;
    if (t === 6) Yt(e, r);
    else {
      if (a = e.current.alternate, !(r & 30) && !Yp(a) && (t = Ja(e, r), t === 2 && (i = hi(e), i !== 0 && (r = i, t = Bi(e, i))), t === 1)) throw n = Qr, jn(e, 0), Yt(e, r), Je(e, ye()), n;
      switch (e.finishedWork = a, e.finishedLanes = r, t) {
        case 0:
        case 1:
          throw Error(I(345));
        case 2:
          vn(e, qe, At);
          break;
        case 3:
          if (Yt(e, r), (r & 130023424) === r && (t = To + 500 - ye(), 10 < t)) {
            if (La(e, 0) !== 0) break;
            if (a = e.suspendedLanes, (a & r) !== r) {
              Qe(), e.pingedLanes |= e.suspendedLanes & a;
              break;
            }
            e.timeoutHandle = wi(vn.bind(null, e, qe, At), t);
            break;
          }
          vn(e, qe, At);
          break;
        case 4:
          if (Yt(e, r), (r & 4194240) === r) break;
          for (t = e.eventTimes, a = -1; 0 < r; ) {
            var o = 31 - St(r);
            i = 1 << o, o = t[o], o > a && (a = o), r &= ~i;
          }
          if (r = a, r = ye() - r, r = (120 > r ? 120 : 480 > r ? 480 : 1080 > r ? 1080 : 1920 > r ? 1920 : 3e3 > r ? 3e3 : 4320 > r ? 4320 : 1960 * qp(r / 1960)) - r, 10 < r) {
            e.timeoutHandle = wi(vn.bind(null, e, qe, At), r);
            break;
          }
          vn(e, qe, At);
          break;
        case 5:
          vn(e, qe, At);
          break;
        default:
          throw Error(I(329));
      }
    }
  }
  return Je(e, ye()), e.callbackNode === n ? dd.bind(null, e) : null;
}
function Bi(e, t) {
  var n = Ir;
  return e.current.memoizedState.isDehydrated && (jn(e, t).flags |= 256), e = Ja(e, t), e !== 2 && (t = qe, qe = n, t !== null && Hi(t)), e;
}
function Hi(e) {
  qe === null ? qe = e : qe.push.apply(qe, e);
}
function Yp(e) {
  for (var t = e; ; ) {
    if (t.flags & 16384) {
      var n = t.updateQueue;
      if (n !== null && (n = n.stores, n !== null)) for (var r = 0; r < n.length; r++) {
        var a = n[r], i = a.getSnapshot;
        a = a.value;
        try {
          if (!kt(i(), a)) return !1;
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
function Yt(e, t) {
  for (t &= ~_o, t &= ~dl, e.suspendedLanes |= t, e.pingedLanes &= ~t, e = e.expirationTimes; 0 < t; ) {
    var n = 31 - St(t), r = 1 << n;
    e[n] = -1, t &= ~r;
  }
}
function rc(e) {
  if (J & 6) throw Error(I(327));
  qn();
  var t = La(e, 0);
  if (!(t & 1)) return Je(e, ye()), null;
  var n = Ja(e, t);
  if (e.tag !== 0 && n === 2) {
    var r = hi(e);
    r !== 0 && (t = r, n = Bi(e, r));
  }
  if (n === 1) throw n = Qr, jn(e, 0), Yt(e, t), Je(e, ye()), n;
  if (n === 6) throw Error(I(345));
  return e.finishedWork = e.current.alternate, e.finishedLanes = t, vn(e, qe, At), Je(e, ye()), null;
}
function Do(e, t) {
  var n = J;
  J |= 1;
  try {
    return e(t);
  } finally {
    J = n, J === 0 && (rr = ye() + 500, ol && pn());
  }
}
function In(e) {
  Zt !== null && Zt.tag === 0 && !(J & 6) && qn();
  var t = J;
  J |= 1;
  var n = mt.transition, r = ne;
  try {
    if (mt.transition = null, ne = 1, e) return e();
  } finally {
    ne = r, mt.transition = n, J = t, !(J & 6) && pn();
  }
}
function zo() {
  tt = Bn.current, oe(Bn);
}
function jn(e, t) {
  e.finishedWork = null, e.finishedLanes = 0;
  var n = e.timeoutHandle;
  if (n !== -1 && (e.timeoutHandle = -1, Ep(n)), Ne !== null) for (n = Ne.return; n !== null; ) {
    var r = n;
    switch (mo(r), r.tag) {
      case 1:
        r = r.type.childContextTypes, r != null && ba();
        break;
      case 3:
        tr(), oe(Xe), oe(Ue), wo();
        break;
      case 5:
        So(r);
        break;
      case 4:
        tr();
        break;
      case 13:
        oe(ue);
        break;
      case 19:
        oe(ue);
        break;
      case 10:
        go(r.type._context);
        break;
      case 22:
      case 23:
        zo();
    }
    n = n.return;
  }
  if (Fe = e, Ne = e = on(e.current, null), De = tt = t, Ee = 0, Qr = null, _o = dl = En = 0, qe = Ir = null, gn !== null) {
    for (t = 0; t < gn.length; t++) if (n = gn[t], r = n.interleaved, r !== null) {
      n.interleaved = null;
      var a = r.next, i = n.pending;
      if (i !== null) {
        var o = i.next;
        i.next = a, r.next = o;
      }
      n.pending = r;
    }
    gn = null;
  }
  return e;
}
function fd(e, t) {
  do {
    var n = Ne;
    try {
      if (xo(), ka.current = qa, Ka) {
        for (var r = de.memoizedState; r !== null; ) {
          var a = r.queue;
          a !== null && (a.pending = null), r = r.next;
        }
        Ka = !1;
      }
      if (kn = 0, Pe = Ce = de = null, kr = !1, Br = 0, Ro.current = null, n === null || n.return === null) {
        Ee = 1, Qr = t, Ne = null;
        break;
      }
      e: {
        var i = e, o = n.return, s = n, c = t;
        if (t = De, s.flags |= 32768, c !== null && typeof c == "object" && typeof c.then == "function") {
          var d = c, j = s, u = j.tag;
          if (!(j.mode & 1) && (u === 0 || u === 11 || u === 15)) {
            var m = j.alternate;
            m ? (j.updateQueue = m.updateQueue, j.memoizedState = m.memoizedState, j.lanes = m.lanes) : (j.updateQueue = null, j.memoizedState = null);
          }
          var h = Hs(o);
          if (h !== null) {
            h.flags &= -257, Ws(h, o, s, i, t), h.mode & 1 && Bs(i, d, t), t = h, c = d;
            var g = t.updateQueue;
            if (g === null) {
              var w = /* @__PURE__ */ new Set();
              w.add(c), t.updateQueue = w;
            } else g.add(c);
            break e;
          } else {
            if (!(t & 1)) {
              Bs(i, d, t), Lo();
              break e;
            }
            c = Error(I(426));
          }
        } else if (se && s.mode & 1) {
          var $ = Hs(o);
          if ($ !== null) {
            !($.flags & 65536) && ($.flags |= 256), Ws($, o, s, i, t), ho(nr(c, s));
            break e;
          }
        }
        i = c = nr(c, s), Ee !== 4 && (Ee = 2), Ir === null ? Ir = [i] : Ir.push(i), i = o;
        do {
          switch (i.tag) {
            case 3:
              i.flags |= 65536, t &= -t, i.lanes |= t;
              var p = qu(i, c, t);
              Ms(i, p);
              break e;
            case 1:
              s = c;
              var f = i.type, v = i.stateNode;
              if (!(i.flags & 128) && (typeof f.getDerivedStateFromError == "function" || v !== null && typeof v.componentDidCatch == "function" && (an === null || !an.has(v)))) {
                i.flags |= 65536, t &= -t, i.lanes |= t;
                var k = Yu(i, s, t);
                Ms(i, k);
                break e;
              }
          }
          i = i.return;
        } while (i !== null);
      }
      hd(n);
    } catch (T) {
      t = T, Ne === n && n !== null && (Ne = n = n.return);
      continue;
    }
    break;
  } while (!0);
}
function pd() {
  var e = Ya.current;
  return Ya.current = qa, e === null ? qa : e;
}
function Lo() {
  (Ee === 0 || Ee === 3 || Ee === 2) && (Ee = 4), Fe === null || !(En & 268435455) && !(dl & 268435455) || Yt(Fe, De);
}
function Ja(e, t) {
  var n = J;
  J |= 2;
  var r = pd();
  (Fe !== e || De !== t) && (At = null, jn(e, t));
  do
    try {
      Xp();
      break;
    } catch (a) {
      fd(e, a);
    }
  while (!0);
  if (xo(), J = n, Ya.current = r, Ne !== null) throw Error(I(261));
  return Fe = null, De = 0, Ee;
}
function Xp() {
  for (; Ne !== null; ) md(Ne);
}
function Zp() {
  for (; Ne !== null && !wf(); ) md(Ne);
}
function md(e) {
  var t = xd(e.alternate, e, tt);
  e.memoizedProps = e.pendingProps, t === null ? hd(e) : Ne = t, Ro.current = null;
}
function hd(e) {
  var t = e;
  do {
    var n = t.alternate;
    if (e = t.return, t.flags & 32768) {
      if (n = Wp(n, t), n !== null) {
        n.flags &= 32767, Ne = n;
        return;
      }
      if (e !== null) e.flags |= 32768, e.subtreeFlags = 0, e.deletions = null;
      else {
        Ee = 6, Ne = null;
        return;
      }
    } else if (n = Hp(n, t, tt), n !== null) {
      Ne = n;
      return;
    }
    if (t = t.sibling, t !== null) {
      Ne = t;
      return;
    }
    Ne = t = e;
  } while (t !== null);
  Ee === 0 && (Ee = 5);
}
function vn(e, t, n) {
  var r = ne, a = mt.transition;
  try {
    mt.transition = null, ne = 1, Jp(e, t, n, r);
  } finally {
    mt.transition = a, ne = r;
  }
  return null;
}
function Jp(e, t, n, r) {
  do
    qn();
  while (Zt !== null);
  if (J & 6) throw Error(I(327));
  n = e.finishedWork;
  var a = e.finishedLanes;
  if (n === null) return null;
  if (e.finishedWork = null, e.finishedLanes = 0, n === e.current) throw Error(I(177));
  e.callbackNode = null, e.callbackPriority = 0;
  var i = n.lanes | n.childLanes;
  if (Df(e, i), e === Fe && (Ne = Fe = null, De = 0), !(n.subtreeFlags & 2064) && !(n.flags & 2064) || xa || (xa = !0, gd(za, function() {
    return qn(), null;
  })), i = (n.flags & 15990) !== 0, n.subtreeFlags & 15990 || i) {
    i = mt.transition, mt.transition = null;
    var o = ne;
    ne = 1;
    var s = J;
    J |= 4, Ro.current = null, Gp(e, n), cd(n, e), yp(Ni), Aa = !!ji, Ni = ji = null, e.current = n, Kp(n), Cf(), J = s, ne = o, mt.transition = i;
  } else e.current = n;
  if (xa && (xa = !1, Zt = e, Za = a), i = e.pendingLanes, i === 0 && (an = null), If(n.stateNode), Je(e, ye()), t !== null) for (r = e.onRecoverableError, n = 0; n < t.length; n++) a = t[n], r(a.value, { componentStack: a.stack, digest: a.digest });
  if (Xa) throw Xa = !1, e = Ui, Ui = null, e;
  return Za & 1 && e.tag !== 0 && qn(), i = e.pendingLanes, i & 1 ? e === Vi ? Pr++ : (Pr = 0, Vi = e) : Pr = 0, pn(), null;
}
function qn() {
  if (Zt !== null) {
    var e = qc(Za), t = mt.transition, n = ne;
    try {
      if (mt.transition = null, ne = 16 > e ? 16 : e, Zt === null) var r = !1;
      else {
        if (e = Zt, Zt = null, Za = 0, J & 6) throw Error(I(331));
        var a = J;
        for (J |= 4, O = e.current; O !== null; ) {
          var i = O, o = i.child;
          if (O.flags & 16) {
            var s = i.deletions;
            if (s !== null) {
              for (var c = 0; c < s.length; c++) {
                var d = s[c];
                for (O = d; O !== null; ) {
                  var j = O;
                  switch (j.tag) {
                    case 0:
                    case 11:
                    case 15:
                      Er(8, j, i);
                  }
                  var u = j.child;
                  if (u !== null) u.return = j, O = u;
                  else for (; O !== null; ) {
                    j = O;
                    var m = j.sibling, h = j.return;
                    if (id(j), j === d) {
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
              var g = i.alternate;
              if (g !== null) {
                var w = g.child;
                if (w !== null) {
                  g.child = null;
                  do {
                    var $ = w.sibling;
                    w.sibling = null, w = $;
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
                Er(9, i, i.return);
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
          var v = o.child;
          if (o.subtreeFlags & 2064 && v !== null) v.return = o, O = v;
          else e: for (o = f; O !== null; ) {
            if (s = O, s.flags & 2048) try {
              switch (s.tag) {
                case 0:
                case 11:
                case 15:
                  ul(9, s);
              }
            } catch (T) {
              ve(s, s.return, T);
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
        if (J = a, pn(), _t && typeof _t.onPostCommitFiberRoot == "function") try {
          _t.onPostCommitFiberRoot(nl, e);
        } catch {
        }
        r = !0;
      }
      return r;
    } finally {
      ne = n, mt.transition = t;
    }
  }
  return !1;
}
function ac(e, t, n) {
  t = nr(n, t), t = qu(e, t, 1), e = rn(e, t, 1), t = Qe(), e !== null && (qr(e, 1, t), Je(e, t));
}
function ve(e, t, n) {
  if (e.tag === 3) ac(e, e, n);
  else for (; t !== null; ) {
    if (t.tag === 3) {
      ac(t, e, n);
      break;
    } else if (t.tag === 1) {
      var r = t.stateNode;
      if (typeof t.type.getDerivedStateFromError == "function" || typeof r.componentDidCatch == "function" && (an === null || !an.has(r))) {
        e = nr(n, e), e = Yu(t, e, 1), t = rn(t, e, 1), e = Qe(), t !== null && (qr(t, 1, e), Je(t, e));
        break;
      }
    }
    t = t.return;
  }
}
function em(e, t, n) {
  var r = e.pingCache;
  r !== null && r.delete(t), t = Qe(), e.pingedLanes |= e.suspendedLanes & n, Fe === e && (De & n) === n && (Ee === 4 || Ee === 3 && (De & 130023424) === De && 500 > ye() - To ? jn(e, 0) : _o |= n), Je(e, t);
}
function vd(e, t) {
  t === 0 && (e.mode & 1 ? (t = oa, oa <<= 1, !(oa & 130023424) && (oa = 4194304)) : t = 1);
  var n = Qe();
  e = Bt(e, t), e !== null && (qr(e, t, n), Je(e, n));
}
function tm(e) {
  var t = e.memoizedState, n = 0;
  t !== null && (n = t.retryLane), vd(e, n);
}
function nm(e, t) {
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
      throw Error(I(314));
  }
  r !== null && r.delete(t), vd(e, n);
}
var xd;
xd = function(e, t, n) {
  if (e !== null) if (e.memoizedProps !== t.pendingProps || Xe.current) Ye = !0;
  else {
    if (!(e.lanes & n) && !(t.flags & 128)) return Ye = !1, Bp(e, t, n);
    Ye = !!(e.flags & 131072);
  }
  else Ye = !1, se && t.flags & 1048576 && Nu(t, Ba, t.index);
  switch (t.lanes = 0, t.tag) {
    case 2:
      var r = t.type;
      Ia(e, t), e = t.pendingProps;
      var a = Zn(t, Ue.current);
      Kn(t, n), a = ko(null, t, r, e, a, n);
      var i = Eo();
      return t.flags |= 1, typeof a == "object" && a !== null && typeof a.render == "function" && a.$$typeof === void 0 ? (t.tag = 1, t.memoizedState = null, t.updateQueue = null, Ze(r) ? (i = !0, Ua(t)) : i = !1, t.memoizedState = a.state !== null && a.state !== void 0 ? a.state : null, jo(t), a.updater = cl, t.stateNode = a, a._reactInternals = t, Ri(t, r, e, n), t = Di(null, t, r, !0, i, n)) : (t.tag = 0, se && i && po(t), He(null, t, a, n), t = t.child), t;
    case 16:
      r = t.elementType;
      e: {
        switch (Ia(e, t), e = t.pendingProps, a = r._init, r = a(r._payload), t.type = r, a = t.tag = am(r), e = yt(r, e), a) {
          case 0:
            t = Ti(null, t, r, e, n);
            break e;
          case 1:
            t = Ks(null, t, r, e, n);
            break e;
          case 11:
            t = Qs(null, t, r, e, n);
            break e;
          case 14:
            t = Gs(null, t, r, yt(r.type, e), n);
            break e;
        }
        throw Error(I(
          306,
          r,
          ""
        ));
      }
      return t;
    case 0:
      return r = t.type, a = t.pendingProps, a = t.elementType === r ? a : yt(r, a), Ti(e, t, r, a, n);
    case 1:
      return r = t.type, a = t.pendingProps, a = t.elementType === r ? a : yt(r, a), Ks(e, t, r, a, n);
    case 3:
      e: {
        if (ed(t), e === null) throw Error(I(387));
        r = t.pendingProps, i = t.memoizedState, a = i.element, Iu(e, t), Qa(t, r, null, n);
        var o = t.memoizedState;
        if (r = o.element, i.isDehydrated) if (i = { element: r, isDehydrated: !1, cache: o.cache, pendingSuspenseBoundaries: o.pendingSuspenseBoundaries, transitions: o.transitions }, t.updateQueue.baseState = i, t.memoizedState = i, t.flags & 256) {
          a = nr(Error(I(423)), t), t = qs(e, t, r, n, a);
          break e;
        } else if (r !== a) {
          a = nr(Error(I(424)), t), t = qs(e, t, r, n, a);
          break e;
        } else for (nt = nn(t.stateNode.containerInfo.firstChild), rt = t, se = !0, Nt = null, n = ku(t, null, r, n), t.child = n; n; ) n.flags = n.flags & -3 | 4096, n = n.sibling;
        else {
          if (Jn(), r === a) {
            t = Ht(e, t, n);
            break e;
          }
          He(e, t, r, n);
        }
        t = t.child;
      }
      return t;
    case 5:
      return Pu(t), e === null && Ii(t), r = t.type, a = t.pendingProps, i = e !== null ? e.memoizedProps : null, o = a.children, Si(r, a) ? o = null : i !== null && Si(r, i) && (t.flags |= 32), Ju(e, t), He(e, t, o, n), t.child;
    case 6:
      return e === null && Ii(t), null;
    case 13:
      return td(e, t, n);
    case 4:
      return No(t, t.stateNode.containerInfo), r = t.pendingProps, e === null ? t.child = er(t, null, r, n) : He(e, t, r, n), t.child;
    case 11:
      return r = t.type, a = t.pendingProps, a = t.elementType === r ? a : yt(r, a), Qs(e, t, r, a, n);
    case 7:
      return He(e, t, t.pendingProps, n), t.child;
    case 8:
      return He(e, t, t.pendingProps.children, n), t.child;
    case 12:
      return He(e, t, t.pendingProps.children, n), t.child;
    case 10:
      e: {
        if (r = t.type._context, a = t.pendingProps, i = t.memoizedProps, o = a.value, ae(Ha, r._currentValue), r._currentValue = o, i !== null) if (kt(i.value, o)) {
          if (i.children === a.children && !Xe.current) {
            t = Ht(e, t, n);
            break e;
          }
        } else for (i = t.child, i !== null && (i.return = t); i !== null; ) {
          var s = i.dependencies;
          if (s !== null) {
            o = i.child;
            for (var c = s.firstContext; c !== null; ) {
              if (c.context === r) {
                if (i.tag === 1) {
                  c = bt(-1, n & -n), c.tag = 2;
                  var d = i.updateQueue;
                  if (d !== null) {
                    d = d.shared;
                    var j = d.pending;
                    j === null ? c.next = c : (c.next = j.next, j.next = c), d.pending = c;
                  }
                }
                i.lanes |= n, c = i.alternate, c !== null && (c.lanes |= n), Pi(
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
            if (o = i.return, o === null) throw Error(I(341));
            o.lanes |= n, s = o.alternate, s !== null && (s.lanes |= n), Pi(o, n, t), o = i.sibling;
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
      return a = t.type, r = t.pendingProps.children, Kn(t, n), a = ht(a), r = r(a), t.flags |= 1, He(e, t, r, n), t.child;
    case 14:
      return r = t.type, a = yt(r, t.pendingProps), a = yt(r.type, a), Gs(e, t, r, a, n);
    case 15:
      return Xu(e, t, t.type, t.pendingProps, n);
    case 17:
      return r = t.type, a = t.pendingProps, a = t.elementType === r ? a : yt(r, a), Ia(e, t), t.tag = 1, Ze(r) ? (e = !0, Ua(t)) : e = !1, Kn(t, n), Ku(t, r, a), Ri(t, r, a, n), Di(null, t, r, !0, e, n);
    case 19:
      return nd(e, t, n);
    case 22:
      return Zu(e, t, n);
  }
  throw Error(I(156, t.tag));
};
function gd(e, t) {
  return Wc(e, t);
}
function rm(e, t, n, r) {
  this.tag = e, this.key = n, this.sibling = this.child = this.return = this.stateNode = this.type = this.elementType = null, this.index = 0, this.ref = null, this.pendingProps = t, this.dependencies = this.memoizedState = this.updateQueue = this.memoizedProps = null, this.mode = r, this.subtreeFlags = this.flags = 0, this.deletions = null, this.childLanes = this.lanes = 0, this.alternate = null;
}
function pt(e, t, n, r) {
  return new rm(e, t, n, r);
}
function Ao(e) {
  return e = e.prototype, !(!e || !e.isReactComponent);
}
function am(e) {
  if (typeof e == "function") return Ao(e) ? 1 : 0;
  if (e != null) {
    if (e = e.$$typeof, e === eo) return 11;
    if (e === to) return 14;
  }
  return 2;
}
function on(e, t) {
  var n = e.alternate;
  return n === null ? (n = pt(e.tag, t, e.key, e.mode), n.elementType = e.elementType, n.type = e.type, n.stateNode = e.stateNode, n.alternate = e, e.alternate = n) : (n.pendingProps = t, n.type = e.type, n.flags = 0, n.subtreeFlags = 0, n.deletions = null), n.flags = e.flags & 14680064, n.childLanes = e.childLanes, n.lanes = e.lanes, n.child = e.child, n.memoizedProps = e.memoizedProps, n.memoizedState = e.memoizedState, n.updateQueue = e.updateQueue, t = e.dependencies, n.dependencies = t === null ? null : { lanes: t.lanes, firstContext: t.firstContext }, n.sibling = e.sibling, n.index = e.index, n.ref = e.ref, n;
}
function Ra(e, t, n, r, a, i) {
  var o = 2;
  if (r = e, typeof e == "function") Ao(e) && (o = 1);
  else if (typeof e == "string") o = 5;
  else e: switch (e) {
    case Dn:
      return Nn(n.children, a, i, t);
    case Ji:
      o = 8, a |= 8;
      break;
    case ei:
      return e = pt(12, n, t, a | 2), e.elementType = ei, e.lanes = i, e;
    case ti:
      return e = pt(13, n, t, a), e.elementType = ti, e.lanes = i, e;
    case ni:
      return e = pt(19, n, t, a), e.elementType = ni, e.lanes = i, e;
    case Pc:
      return fl(n, a, i, t);
    default:
      if (typeof e == "object" && e !== null) switch (e.$$typeof) {
        case Ec:
          o = 10;
          break e;
        case Ic:
          o = 9;
          break e;
        case eo:
          o = 11;
          break e;
        case to:
          o = 14;
          break e;
        case Gt:
          o = 16, r = null;
          break e;
      }
      throw Error(I(130, e == null ? e : typeof e, ""));
  }
  return t = pt(o, n, t, a), t.elementType = e, t.type = r, t.lanes = i, t;
}
function Nn(e, t, n, r) {
  return e = pt(7, e, r, t), e.lanes = n, e;
}
function fl(e, t, n, r) {
  return e = pt(22, e, r, t), e.elementType = Pc, e.lanes = n, e.stateNode = { isHidden: !1 }, e;
}
function Kl(e, t, n) {
  return e = pt(6, e, null, t), e.lanes = n, e;
}
function ql(e, t, n) {
  return t = pt(4, e.children !== null ? e.children : [], e.key, t), t.lanes = n, t.stateNode = { containerInfo: e.containerInfo, pendingChildren: null, implementation: e.implementation }, t;
}
function lm(e, t, n, r, a) {
  this.tag = t, this.containerInfo = e, this.finishedWork = this.pingCache = this.current = this.pendingChildren = null, this.timeoutHandle = -1, this.callbackNode = this.pendingContext = this.context = null, this.callbackPriority = 0, this.eventTimes = Fl(0), this.expirationTimes = Fl(-1), this.entangledLanes = this.finishedLanes = this.mutableReadLanes = this.expiredLanes = this.pingedLanes = this.suspendedLanes = this.pendingLanes = 0, this.entanglements = Fl(0), this.identifierPrefix = r, this.onRecoverableError = a, this.mutableSourceEagerHydrationData = null;
}
function Mo(e, t, n, r, a, i, o, s, c) {
  return e = new lm(e, t, n, s, c), t === 1 ? (t = 1, i === !0 && (t |= 8)) : t = 0, i = pt(3, null, null, t), e.current = i, i.stateNode = e, i.memoizedState = { element: r, isDehydrated: n, cache: null, transitions: null, pendingSuspenseBoundaries: null }, jo(i), e;
}
function im(e, t, n) {
  var r = 3 < arguments.length && arguments[3] !== void 0 ? arguments[3] : null;
  return { $$typeof: Tn, key: r == null ? null : "" + r, children: e, containerInfo: t, implementation: n };
}
function yd(e) {
  if (!e) return cn;
  e = e._reactInternals;
  e: {
    if (Fn(e) !== e || e.tag !== 1) throw Error(I(170));
    var t = e;
    do {
      switch (t.tag) {
        case 3:
          t = t.stateNode.context;
          break e;
        case 1:
          if (Ze(t.type)) {
            t = t.stateNode.__reactInternalMemoizedMergedChildContext;
            break e;
          }
      }
      t = t.return;
    } while (t !== null);
    throw Error(I(171));
  }
  if (e.tag === 1) {
    var n = e.type;
    if (Ze(n)) return yu(e, n, t);
  }
  return t;
}
function jd(e, t, n, r, a, i, o, s, c) {
  return e = Mo(n, r, !0, e, a, i, o, s, c), e.context = yd(null), n = e.current, r = Qe(), a = ln(n), i = bt(r, a), i.callback = t ?? null, rn(n, i, a), e.current.lanes = a, qr(e, a, r), Je(e, r), e;
}
function pl(e, t, n, r) {
  var a = t.current, i = Qe(), o = ln(a);
  return n = yd(n), t.context === null ? t.context = n : t.pendingContext = n, t = bt(i, o), t.payload = { element: e }, r = r === void 0 ? null : r, r !== null && (t.callback = r), e = rn(a, t, o), e !== null && (wt(e, a, o, i), Ca(e, a, o)), o;
}
function el(e) {
  if (e = e.current, !e.child) return null;
  switch (e.child.tag) {
    case 5:
      return e.child.stateNode;
    default:
      return e.child.stateNode;
  }
}
function lc(e, t) {
  if (e = e.memoizedState, e !== null && e.dehydrated !== null) {
    var n = e.retryLane;
    e.retryLane = n !== 0 && n < t ? n : t;
  }
}
function $o(e, t) {
  lc(e, t), (e = e.alternate) && lc(e, t);
}
function om() {
  return null;
}
var Nd = typeof reportError == "function" ? reportError : function(e) {
  console.error(e);
};
function Oo(e) {
  this._internalRoot = e;
}
ml.prototype.render = Oo.prototype.render = function(e) {
  var t = this._internalRoot;
  if (t === null) throw Error(I(409));
  pl(e, t, null, null);
};
ml.prototype.unmount = Oo.prototype.unmount = function() {
  var e = this._internalRoot;
  if (e !== null) {
    this._internalRoot = null;
    var t = e.containerInfo;
    In(function() {
      pl(null, e, null, null);
    }), t[Vt] = null;
  }
};
function ml(e) {
  this._internalRoot = e;
}
ml.prototype.unstable_scheduleHydration = function(e) {
  if (e) {
    var t = Zc();
    e = { blockedOn: null, target: e, priority: t };
    for (var n = 0; n < qt.length && t !== 0 && t < qt[n].priority; n++) ;
    qt.splice(n, 0, e), n === 0 && eu(e);
  }
};
function bo(e) {
  return !(!e || e.nodeType !== 1 && e.nodeType !== 9 && e.nodeType !== 11);
}
function hl(e) {
  return !(!e || e.nodeType !== 1 && e.nodeType !== 9 && e.nodeType !== 11 && (e.nodeType !== 8 || e.nodeValue !== " react-mount-point-unstable "));
}
function ic() {
}
function sm(e, t, n, r, a) {
  if (a) {
    if (typeof r == "function") {
      var i = r;
      r = function() {
        var d = el(o);
        i.call(d);
      };
    }
    var o = jd(t, r, e, 0, null, !1, !1, "", ic);
    return e._reactRootContainer = o, e[Vt] = o.current, $r(e.nodeType === 8 ? e.parentNode : e), In(), o;
  }
  for (; a = e.lastChild; ) e.removeChild(a);
  if (typeof r == "function") {
    var s = r;
    r = function() {
      var d = el(c);
      s.call(d);
    };
  }
  var c = Mo(e, 0, !1, null, null, !1, !1, "", ic);
  return e._reactRootContainer = c, e[Vt] = c.current, $r(e.nodeType === 8 ? e.parentNode : e), In(function() {
    pl(t, c, n, r);
  }), c;
}
function vl(e, t, n, r, a) {
  var i = n._reactRootContainer;
  if (i) {
    var o = i;
    if (typeof a == "function") {
      var s = a;
      a = function() {
        var c = el(o);
        s.call(c);
      };
    }
    pl(t, o, e, a);
  } else o = sm(n, t, e, a, r);
  return el(o);
}
Yc = function(e) {
  switch (e.tag) {
    case 3:
      var t = e.stateNode;
      if (t.current.memoizedState.isDehydrated) {
        var n = gr(t.pendingLanes);
        n !== 0 && (ao(t, n | 1), Je(t, ye()), !(J & 6) && (rr = ye() + 500, pn()));
      }
      break;
    case 13:
      In(function() {
        var r = Bt(e, 1);
        if (r !== null) {
          var a = Qe();
          wt(r, e, 1, a);
        }
      }), $o(e, 1);
  }
};
lo = function(e) {
  if (e.tag === 13) {
    var t = Bt(e, 134217728);
    if (t !== null) {
      var n = Qe();
      wt(t, e, 134217728, n);
    }
    $o(e, 134217728);
  }
};
Xc = function(e) {
  if (e.tag === 13) {
    var t = ln(e), n = Bt(e, t);
    if (n !== null) {
      var r = Qe();
      wt(n, e, t, r);
    }
    $o(e, t);
  }
};
Zc = function() {
  return ne;
};
Jc = function(e, t) {
  var n = ne;
  try {
    return ne = e, t();
  } finally {
    ne = n;
  }
};
fi = function(e, t, n) {
  switch (t) {
    case "input":
      if (li(e, n), t = n.name, n.type === "radio" && t != null) {
        for (n = e; n.parentNode; ) n = n.parentNode;
        for (n = n.querySelectorAll("input[name=" + JSON.stringify("" + t) + '][type="radio"]'), t = 0; t < n.length; t++) {
          var r = n[t];
          if (r !== e && r.form === e.form) {
            var a = il(r);
            if (!a) throw Error(I(90));
            Rc(r), li(r, a);
          }
        }
      }
      break;
    case "textarea":
      Tc(e, n);
      break;
    case "select":
      t = n.value, t != null && Hn(e, !!n.multiple, t, !1);
  }
};
Oc = Do;
bc = In;
var cm = { usingClientEntryPoint: !1, Events: [Xr, Mn, il, Mc, $c, Do] }, hr = { findFiberByHostInstance: xn, bundleType: 0, version: "18.3.1", rendererPackageName: "react-dom" }, um = { bundleType: hr.bundleType, version: hr.version, rendererPackageName: hr.rendererPackageName, rendererConfig: hr.rendererConfig, overrideHookState: null, overrideHookStateDeletePath: null, overrideHookStateRenamePath: null, overrideProps: null, overridePropsDeletePath: null, overridePropsRenamePath: null, setErrorHandler: null, setSuspenseHandler: null, scheduleUpdate: null, currentDispatcherRef: Wt.ReactCurrentDispatcher, findHostInstanceByFiber: function(e) {
  return e = Bc(e), e === null ? null : e.stateNode;
}, findFiberByHostInstance: hr.findFiberByHostInstance || om, findHostInstancesForRefresh: null, scheduleRefresh: null, scheduleRoot: null, setRefreshHandler: null, getCurrentFiber: null, reconcilerVersion: "18.3.1-next-f1338f8080-20240426" };
if (typeof __REACT_DEVTOOLS_GLOBAL_HOOK__ < "u") {
  var ga = __REACT_DEVTOOLS_GLOBAL_HOOK__;
  if (!ga.isDisabled && ga.supportsFiber) try {
    nl = ga.inject(um), _t = ga;
  } catch {
  }
}
lt.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED = cm;
lt.createPortal = function(e, t) {
  var n = 2 < arguments.length && arguments[2] !== void 0 ? arguments[2] : null;
  if (!bo(t)) throw Error(I(200));
  return im(e, t, null, n);
};
lt.createRoot = function(e, t) {
  if (!bo(e)) throw Error(I(299));
  var n = !1, r = "", a = Nd;
  return t != null && (t.unstable_strictMode === !0 && (n = !0), t.identifierPrefix !== void 0 && (r = t.identifierPrefix), t.onRecoverableError !== void 0 && (a = t.onRecoverableError)), t = Mo(e, 1, !1, null, null, n, !1, r, a), e[Vt] = t.current, $r(e.nodeType === 8 ? e.parentNode : e), new Oo(t);
};
lt.findDOMNode = function(e) {
  if (e == null) return null;
  if (e.nodeType === 1) return e;
  var t = e._reactInternals;
  if (t === void 0)
    throw typeof e.render == "function" ? Error(I(188)) : (e = Object.keys(e).join(","), Error(I(268, e)));
  return e = Bc(t), e = e === null ? null : e.stateNode, e;
};
lt.flushSync = function(e) {
  return In(e);
};
lt.hydrate = function(e, t, n) {
  if (!hl(t)) throw Error(I(200));
  return vl(null, e, t, !0, n);
};
lt.hydrateRoot = function(e, t, n) {
  if (!bo(e)) throw Error(I(405));
  var r = n != null && n.hydratedSources || null, a = !1, i = "", o = Nd;
  if (n != null && (n.unstable_strictMode === !0 && (a = !0), n.identifierPrefix !== void 0 && (i = n.identifierPrefix), n.onRecoverableError !== void 0 && (o = n.onRecoverableError)), t = jd(t, null, e, 1, n ?? null, a, !1, i, o), e[Vt] = t.current, $r(e), r) for (e = 0; e < r.length; e++) n = r[e], a = n._getVersion, a = a(n._source), t.mutableSourceEagerHydrationData == null ? t.mutableSourceEagerHydrationData = [n, a] : t.mutableSourceEagerHydrationData.push(
    n,
    a
  );
  return new ml(t);
};
lt.render = function(e, t, n) {
  if (!hl(t)) throw Error(I(200));
  return vl(null, e, t, !1, n);
};
lt.unmountComponentAtNode = function(e) {
  if (!hl(e)) throw Error(I(40));
  return e._reactRootContainer ? (In(function() {
    vl(null, null, e, !1, function() {
      e._reactRootContainer = null, e[Vt] = null;
    });
  }), !0) : !1;
};
lt.unstable_batchedUpdates = Do;
lt.unstable_renderSubtreeIntoContainer = function(e, t, n, r) {
  if (!hl(n)) throw Error(I(200));
  if (e == null || e._reactInternals === void 0) throw Error(I(38));
  return vl(e, t, n, !1, r);
};
lt.version = "18.3.1-next-f1338f8080-20240426";
function Sd() {
  if (!(typeof __REACT_DEVTOOLS_GLOBAL_HOOK__ > "u" || typeof __REACT_DEVTOOLS_GLOBAL_HOOK__.checkDCE != "function"))
    try {
      __REACT_DEVTOOLS_GLOBAL_HOOK__.checkDCE(Sd);
    } catch (e) {
      console.error(e);
    }
}
Sd(), Sc.exports = lt;
var dm = Sc.exports, wd, oc = dm;
wd = oc.createRoot, oc.hydrateRoot;
class fm extends Error {
  constructor(t, n) {
    super(t), this.status = n, this.name = "ApiError";
  }
}
function pm(e, t) {
  async function n(r, a = {}) {
    const i = { ...a.headers ?? {} }, o = t();
    o && (i.Authorization = "Bearer " + o);
    let s;
    a.body !== void 0 && (i["Content-Type"] = "application/json", s = JSON.stringify(a.body));
    const c = await e(r, { method: a.method ?? "GET", headers: i, body: s });
    if (!c.ok) {
      let j = `HTTP ${c.status}`;
      try {
        const u = await c.json();
        j = u.detail || u.title || j;
      } catch {
      }
      throw new fm(j, c.status);
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
const Cd = x.createContext(null);
function Dt() {
  const e = x.useContext(Cd);
  if (!e) throw new Error("Fuera del módulo de documentos");
  return e;
}
function mm(e) {
  return pm((t, n) => fetch(t, n), e.token);
}
async function Gr(e, t) {
  const n = e.token(), r = await fetch(t, { headers: n ? { Authorization: "Bearer " + n } : {} });
  if (!r.ok) throw new Error("No se pudo generar el documento");
  const a = URL.createObjectURL(await r.blob());
  window.open(a, "_blank"), setTimeout(() => URL.revokeObjectURL(a), 6e4);
}
async function hm(e, t) {
  var s;
  const n = e.token(), r = await fetch(t, { headers: n ? { Authorization: "Bearer " + n } : {} });
  if (!r.ok) throw new Error((await r.json().catch(() => ({}))).title || "No se pudo generar el fichero");
  const a = ((s = /filename="?([^";]+)"?/.exec(r.headers.get("Content-Disposition") ?? "")) == null ? void 0 : s[1]) ?? "fichero", i = URL.createObjectURL(await r.blob()), o = document.createElement("a");
  o.href = i, o.download = a, o.click(), setTimeout(() => URL.revokeObjectURL(i), 6e4);
}
function kd(e, t) {
  return `${e.toLowerCase().normalize("NFD").replace(/[̀-ͯ]/g, "").replace(/[^a-z0-9]+/g, "-").replace(/^-|-$/g, "").slice(0, 60) || "listado"}-${(/* @__PURE__ */ new Date()).toISOString().slice(0, 10)}.${t}`;
}
function Ed(e, t) {
  const n = URL.createObjectURL(e), r = document.createElement("a");
  r.href = n, r.download = t, document.body.appendChild(r), r.click(), r.remove(), setTimeout(() => URL.revokeObjectURL(n), 6e4);
}
async function vm(e, t) {
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
  Ed(await n.blob(), kd(t.titulo, "xlsx"));
}
function xm(e) {
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
function gm(e) {
  Ed(new Blob(["\uFEFF" + xm(e)], { type: "text/csv;charset=utf-8" }), kd(e.titulo, "csv"));
}
const Id = new Intl.NumberFormat("es-ES", { minimumFractionDigits: 2, maximumFractionDigits: 2 }), ym = new Intl.NumberFormat("es-ES", { maximumFractionDigits: 3 }), _ = (e) => `${Id.format(Number(e) || 0)} €`, xe = (e) => Id.format(Number(e) || 0), We = (e) => ym.format(Number(e) || 0), ke = (e) => {
  if (!e) return "";
  const t = String(e).slice(0, 10).split("-");
  return t.length === 3 ? `${t[2]}/${t[1]}/${t[0]}` : String(e);
}, Ct = () => (/* @__PURE__ */ new Date()).toISOString().slice(0, 10), Oe = (e) => Math.round((e + Number.EPSILON) * 100) / 100;
let jm = 0;
const Jr = () => `l${Date.now().toString(36)}${(++jm).toString(36)}`;
function Sn(e, t) {
  const [n, r] = x.useState(e);
  return x.useEffect(() => {
    const a = setTimeout(() => r(e), t);
    return () => clearTimeout(a);
  }, [e, t]), n;
}
function ea() {
  const e = x.useRef(0);
  return () => {
    const t = ++e.current;
    return () => t === e.current;
  };
}
function Uo(e) {
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
const sc = { presupuesto: "Presupuestos", pedido: "Pedidos de venta", factura: "Facturas emitidas", compra: "Pedidos de compra", gasto: "Facturas de proveedor y gastos" }, Nm = { presupuesto: "+ Nuevo presupuesto", pedido: "+ Nuevo pedido", factura: "+ Nueva factura", compra: "+ Nuevo pedido", gasto: "+ Registrar factura" }, Sm = {
  presupuesto: ["Borrador", "Aceptado", "Rechazado"],
  pedido: ["Borrador", "Confirmado", "ServidoParcial", "Servido", "Facturado", "Cancelado"],
  factura: ["Emitida", "Rectificada", "Anulada"],
  compra: ["Borrador", "Confirmado", "RecibidoParcial", "Recibido", "Facturado", "Cancelado"],
  gasto: ["Registrado", "Anulado"]
}, Yl = { numero: "numero", fecha: "fecha", tercero: "tercero", base: "base", impuestos: "impuestos", total: "total" }, cc = 50, Wi = (e) => e === "Anulada" || e === "Anulado" || e === "Cancelado";
function uc(e, t) {
  const n = { documentos: 0, baseImponible: 0, impuestos: 0, retenciones: 0, total: 0, pendiente: 0, vencido: 0, documentosVencidos: 0 };
  for (const r of e) {
    if (Wi(r.estado)) continue;
    n.documentos++, n.baseImponible += r.base, n.impuestos += r.impuestos, n.total += r.total;
    const a = r.pendiente ?? 0;
    a > 0 && (n.pendiente += a, r.vencimiento && r.vencimiento < t && (n.vencido += a, n.documentosVencidos++));
  }
  return n.baseImponible = Oe(n.baseImponible), n.impuestos = Oe(n.impuestos), n.total = Oe(n.total), n.pendiente = Oe(n.pendiente), n.vencido = Oe(n.vencido), n;
}
function Xl(e, t, n) {
  const r = (a) => t === "numero" || t === "tercero" || t === "estado" ? a[t].toLowerCase() : t === "fecha" ? a.fecha : a[t] ?? 0;
  return [...e].sort((a, i) => {
    const o = r(a), s = r(i), c = typeof o == "number" && typeof s == "number" ? o - s : String(o).localeCompare(String(s), "es", { numeric: !0 });
    return n ? -c : c;
  });
}
const wm = (e, t) => ({
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
}), Cm = (e, t) => {
  var n, r;
  return {
    id: e.id,
    numero: e.numeroFactura ?? "—",
    fecha: e.fecha,
    tercero: `${e.proveedorTexto ?? ""}${e.numeroFactura ? "" : ` · ${e.concepto}`}`,
    terceroId: e.proveedorId,
    base: e.baseImponible,
    impuestos: Oe(e.cuotaIva + (e.recargoTotal || 0)),
    total: e.total,
    estado: e.estado === "Anulado" ? "Anulada" : e.estado,
    extra: e.esRectificativa ? "Rectificativa" : void 0,
    pendiente: t ? t[e.id] ?? 0 : null,
    vencimiento: ((r = (n = e.vencimientos) == null ? void 0 : n[0]) == null ? void 0 : r.fecha) ?? e.fecha
  };
};
function km(e) {
  const { api: t, navegar: n, anfitrion: r } = Dt(), a = e.tipo, i = a === "factura" || a === "gasto", o = a === "compra" || a === "gasto", [s, c] = x.useState(""), [d, j] = x.useState(""), [u, m] = x.useState(""), [h, g] = x.useState(""), [w, $] = x.useState(""), [p, f] = x.useState(""), [v, k] = x.useState(""), [T, F] = x.useState(""), [D, C] = x.useState(""), [M, b] = x.useState({ campo: "fecha", desc: !0 }), [P, Q] = x.useState(1), [ce, Le] = x.useState(null), [Ve, pe] = x.useState(0), [ge, E] = x.useState(null), [y, L] = x.useState([]), [B, W] = x.useState([]), [q, Se] = x.useState(""), [je, me] = x.useState(!1), Re = Sn(s, 250), ot = Sn(v, 350), Be = Sn(T, 350), z = ea(), st = Ct();
  x.useEffect(() => {
    t.get(o ? "/proveedores" : "/clientes").then((R) => L([...R].sort((G, A) => G.nombre.localeCompare(A.nombre, "es")))).catch(() => L([])), a === "factura" && t.get("/series").then((R) => W([...new Set(R.filter((G) => G.tipoDocumento === "Factura" || G.tipoDocumento === 0).map((G) => G.prefijo))].sort())).catch(() => W([]));
  }, [t, a, o]), x.useEffect(() => Q(1), [Re, d, u, h, w, p, ot, Be, D, M, a]);
  const U = (R, G) => {
    const A = new URLSearchParams({ pagina: String(R), tamanoPagina: String(G) });
    Re.trim() && A.set("texto", Re.trim()), d && A.set("estado", d === "Anulada" && a === "gasto" ? "Anulado" : d), u && A.set("desde", u), h && A.set("hasta", h), w && A.set(a === "gasto" ? "proveedorId" : "clienteId", w), p && a === "factura" && A.set("serie", p);
    const Y = parseFloat(ot.replace(/\./g, "").replace(",", ".")), le = parseFloat(Be.replace(/\./g, "").replace(",", "."));
    isNaN(Y) || A.set("importeMin", String(Y)), isNaN(le) || A.set("importeMax", String(le)), D && A.set("cobro", D);
    const N = Yl[M.campo];
    return N && (A.set("orden", N === "tercero" ? a === "gasto" ? "proveedor" : "cliente" : N), A.set("desc", String(M.desc))), A;
  }, Et = async (R, G) => {
    if (a === "factura") {
      const Y = await t.get(`/facturas/buscar?${U(R, G)}`);
      return { r: Y, filas: Y.elementos.map((le) => wm(le, Y.pendientes)) };
    }
    const A = await t.get(`/gastos/buscar?${U(R, G)}`);
    return { r: A, filas: A.elementos.map((Y) => Cm(Y, A.pendientes)) };
  };
  x.useEffect(() => {
    Se("");
    const R = z();
    (async () => {
      if (i) {
        const { r: A, filas: Y } = await Et(P, cc);
        return R() && (pe(A.total), E(A.totales ?? null)), Y;
      }
      switch (a) {
        case "presupuesto":
          return (await t.get("/presupuestos")).map((A) => ({ id: A.id, numero: A.numeroCompleto, fecha: A.fecha, tercero: A.clienteNombre, terceroId: A.clienteId, base: A.baseImponible ?? A.total, impuestos: A.cuotaIva ?? 0, total: A.total, estado: A.estado }));
        case "pedido":
          return (await t.get("/pedidos-venta")).map((A) => {
            const Y = Oe(A.lineas.reduce((le, N) => le + N.base, 0));
            return { id: A.id, numero: A.numeroCompleto, fecha: A.fecha, tercero: A.clienteNombre, terceroId: A.clienteId, base: Y, impuestos: Oe(A.total - Y), total: A.total, estado: A.estado };
          });
        default:
          return (await t.get("/compras/pedidos")).map((A) => {
            const Y = Oe(A.lineas.reduce((le, N) => le + N.importe, 0));
            return { id: A.id, numero: A.numeroCompleto, fecha: A.fecha, tercero: A.proveedorTexto, terceroId: A.proveedorId, base: Y, impuestos: Oe(A.total - Y), total: A.total, estado: A.estado, extra: A.empresaOrigenId ? "Intragrupo" : void 0 };
          });
      }
    })().then((A) => R() && Le(A)).catch((A) => R() && (Se(A.message), Le([])));
  }, [t, a, P, Re, d, u, h, w, p, ot, Be, D, M.campo, M.desc]);
  const et = x.useMemo(() => {
    if (!ce) return [];
    if (i) return Yl[M.campo] ? ce : Xl(ce, M.campo, M.desc);
    const R = Re.trim().toLowerCase(), G = parseFloat(ot.replace(/\./g, "").replace(",", ".")), A = parseFloat(Be.replace(/\./g, "").replace(",", ".")), Y = ce.filter((le) => (!R || le.numero.toLowerCase().includes(R) || le.tercero.toLowerCase().includes(R)) && (!d || le.estado === d) && (!u || le.fecha >= u) && (!h || le.fecha <= h) && (!w || le.terceroId === w) && (isNaN(G) || le.total >= G) && (isNaN(A) || le.total <= A));
    return Xl(Y, M.campo, M.desc);
  }, [ce, Re, d, u, h, w, ot, Be, M, i]), _e = i ? ge : uc(et, st), It = i ? Math.max(1, Math.ceil(Ve / cc)) : 1, zt = [d, u, h, w, p, v, T, D].filter(Boolean).length, mn = o ? "Proveedor" : "Cliente";
  function he() {
    c(""), j(""), m(""), g(""), $(""), f(""), k(""), F(""), C("");
  }
  function Ie(R, G, A = !1) {
    const Y = M.campo === R;
    return /* @__PURE__ */ l.jsxs("th", { className: (A ? "num " : "") + "dx-ordenable" + (Y ? " activo" : ""), onClick: () => b({ campo: R, desc: Y ? !M.desc : R === "fecha" || A }), title: `Ordenar por ${G.toLowerCase()}`, children: [
      G,
      /* @__PURE__ */ l.jsx("span", { className: "dx-flecha", children: Y ? M.desc ? "▼" : "▲" : "" })
    ] });
  }
  async function Lt() {
    if (!i) return et;
    const R = [];
    for (let G = 1; G <= 500; G++) {
      const { r: A, filas: Y } = await Et(G, 200);
      if (R.push(...Y), R.length >= A.total || Y.length === 0) break;
    }
    return Yl[M.campo] ? R : Xl(R, M.campo, M.desc);
  }
  async function ct(R) {
    me(!0);
    try {
      const G = await Lt(), A = i, Y = i ? ge : uc(G, st), le = {
        titulo: sc[a],
        columnas: [
          { titulo: "Número", tipo: "texto" },
          { titulo: "Fecha", tipo: "fecha" },
          { titulo: mn, tipo: "texto" },
          { titulo: "Estado", tipo: "texto" },
          { titulo: "Base", tipo: "moneda", total: "no" },
          { titulo: "Impuestos", tipo: "moneda", total: "no" },
          { titulo: "Total", tipo: "moneda", total: "no" },
          ...A ? [{ titulo: "Pendiente", tipo: "moneda", total: "no" }, { titulo: "Vencimiento", tipo: "fecha" }] : []
        ],
        filas: G.map((N) => [N.numero + (N.extra ? ` (${N.extra})` : ""), ke(N.fecha), N.tercero, N.estado, N.base, N.impuestos, N.total, ...A ? [N.pendiente ?? 0, ke(N.vencimiento)] : []]),
        totales: Y ? [`Total · ${Y.documentos} (sin anulados)`, null, null, null, Y.baseImponible, Y.impuestos, Y.total, ...A ? [Y.pendiente, null] : []] : void 0
      };
      R === "xlsx" ? await vm(r.token(), le) : gm(le), r.aviso(`Exportados ${G.length} documento(s).`, "ok");
    } catch (G) {
      r.aviso("No se pudo exportar: " + G.message, "err");
    } finally {
      me(!1);
    }
  }
  return /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel-head", children: [
      /* @__PURE__ */ l.jsx("h2", { children: sc[a] }),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-acciones", children: [
        /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", disabled: je || !et.length, onClick: () => ct("xlsx"), title: "Exportar a Excel todo lo filtrado", children: je ? "Exportando…" : "↓ Excel" }),
        /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", disabled: je || !et.length, onClick: () => ct("csv"), title: "Exportar a CSV todo lo filtrado", children: "↓ CSV" }),
        /* @__PURE__ */ l.jsx("button", { className: "btn small", onClick: () => n({ tipo: a, pantalla: "editor" }), children: Nm[a] })
      ] })
    ] }),
    /* @__PURE__ */ l.jsxs("div", { className: "dx-filtros", children: [
      /* @__PURE__ */ l.jsx("input", { placeholder: o ? "Número, proveedor o concepto…" : "Número, cliente o NIF…", value: s, onChange: (R) => c(R.target.value), autoFocus: !0 }),
      /* @__PURE__ */ l.jsxs("select", { value: d, onChange: (R) => j(R.target.value), "aria-label": "Estado", children: [
        /* @__PURE__ */ l.jsx("option", { value: "", children: "Todos los estados" }),
        Sm[a].map((R) => /* @__PURE__ */ l.jsx("option", { value: R, children: R }, R))
      ] }),
      /* @__PURE__ */ l.jsx("input", { type: "date", value: u, onChange: (R) => m(R.target.value), title: "Desde", "aria-label": "Desde" }),
      /* @__PURE__ */ l.jsx("input", { type: "date", value: h, onChange: (R) => g(R.target.value), title: "Hasta", "aria-label": "Hasta" })
    ] }),
    /* @__PURE__ */ l.jsxs("div", { className: "dx-filtros dx-filtros2", children: [
      /* @__PURE__ */ l.jsxs("select", { value: w, onChange: (R) => $(R.target.value), "aria-label": mn, children: [
        /* @__PURE__ */ l.jsx("option", { value: "", children: o ? "Todos los proveedores" : "Todos los clientes" }),
        y.map((R) => /* @__PURE__ */ l.jsxs("option", { value: R.id, children: [
          R.nombre,
          R.nifFiscal ? ` · ${R.nifFiscal}` : ""
        ] }, R.id))
      ] }),
      a === "factura" && /* @__PURE__ */ l.jsxs("select", { value: p, onChange: (R) => f(R.target.value), "aria-label": "Serie", children: [
        /* @__PURE__ */ l.jsx("option", { value: "", children: "Todas las series" }),
        B.map((R) => /* @__PURE__ */ l.jsxs("option", { value: R, children: [
          "Serie ",
          R
        ] }, R))
      ] }),
      /* @__PURE__ */ l.jsx("input", { inputMode: "decimal", placeholder: "Importe mín.", value: v, onChange: (R) => k(R.target.value), "aria-label": "Importe mínimo" }),
      /* @__PURE__ */ l.jsx("input", { inputMode: "decimal", placeholder: "Importe máx.", value: T, onChange: (R) => F(R.target.value), "aria-label": "Importe máximo" }),
      i && /* @__PURE__ */ l.jsxs("select", { value: D, onChange: (R) => C(R.target.value), "aria-label": a === "gasto" ? "Estado de pago" : "Estado de cobro", children: [
        /* @__PURE__ */ l.jsx("option", { value: "", children: a === "gasto" ? "Pagadas y pendientes" : "Cobradas y pendientes" }),
        /* @__PURE__ */ l.jsx("option", { value: "pendiente", children: "Pendientes" }),
        /* @__PURE__ */ l.jsx("option", { value: "vencida", children: "Vencidas" }),
        /* @__PURE__ */ l.jsx("option", { value: a === "gasto" ? "pagada" : "cobrada", children: a === "gasto" ? "Pagadas" : "Cobradas" })
      ] }),
      (zt > 0 || s) && /* @__PURE__ */ l.jsxs("button", { className: "btn small secondary", onClick: he, children: [
        "Limpiar",
        zt ? ` (${zt})` : ""
      ] })
    ] }),
    q && /* @__PURE__ */ l.jsx("p", { className: "dx-rojo", children: q }),
    ce === null ? /* @__PURE__ */ l.jsx("p", { className: "muted", children: "Cargando…" }) : et.length === 0 ? /* @__PURE__ */ l.jsx("p", { className: "muted", children: "No hay documentos con estos filtros." }) : /* @__PURE__ */ l.jsxs("table", { className: "dx-lista-docs", children: [
      /* @__PURE__ */ l.jsx("thead", { children: /* @__PURE__ */ l.jsxs("tr", { children: [
        Ie("numero", "Número"),
        Ie("fecha", "Fecha"),
        Ie("tercero", mn),
        Ie("estado", "Estado"),
        Ie("base", "Base", !0),
        Ie("impuestos", "Impuestos", !0),
        Ie("total", "Total", !0),
        i && Ie("pendiente", "Pendiente", !0)
      ] }) }),
      /* @__PURE__ */ l.jsx("tbody", { children: et.map((R) => {
        const G = (R.pendiente ?? 0) > 0 && !!R.vencimiento && R.vencimiento < st;
        return /* @__PURE__ */ l.jsxs("tr", { onClick: () => n({ tipo: a, pantalla: "vista", id: R.id }), tabIndex: 0, onKeyDown: (A) => A.key === "Enter" && n({ tipo: a, pantalla: "vista", id: R.id }), className: Wi(R.estado) ? "dx-anulado" : void 0, children: [
          /* @__PURE__ */ l.jsxs("td", { className: "mono", children: [
            /* @__PURE__ */ l.jsx("strong", { children: R.numero }),
            R.extra && /* @__PURE__ */ l.jsx("span", { className: "pill part", style: { marginLeft: 6 }, children: R.extra })
          ] }),
          /* @__PURE__ */ l.jsx("td", { children: ke(R.fecha) }),
          /* @__PURE__ */ l.jsx("td", { children: R.tercero }),
          /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx("span", { className: Uo(R.estado), children: R.estado }) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: _(R.base) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: _(R.impuestos) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: _(R.total) }) }),
          i && /* @__PURE__ */ l.jsx("td", { className: "num", children: (R.pendiente ?? 0) > 0 ? /* @__PURE__ */ l.jsx("strong", { className: G ? "dx-rojo" : void 0, title: G ? `Vencida el ${ke(R.vencimiento)}` : `Vence el ${ke(R.vencimiento)}`, children: _(R.pendiente) }) : /* @__PURE__ */ l.jsx("span", { className: "muted", children: Wi(R.estado) || R.estado === "Rectificada" ? "—" : a === "gasto" ? "Pagada" : "Cobrada" }) })
        ] }, R.id);
      }) }),
      _e && /* @__PURE__ */ l.jsx("tfoot", { children: /* @__PURE__ */ l.jsxs("tr", { className: "dx-total", children: [
        /* @__PURE__ */ l.jsxs("td", { colSpan: 4, children: [
          /* @__PURE__ */ l.jsxs("strong", { children: [
            "Total · ",
            _e.documentos
          ] }),
          " ",
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: i ? `documento${_e.documentos === 1 ? "" : "s"} de todo el filtro (${It} página${It === 1 ? "" : "s"}) · sin anulados` : "sin anulados ni cancelados" }),
          i && _e.vencido > 0 && /* @__PURE__ */ l.jsxs("span", { className: "dx-rojo", style: { marginLeft: 8 }, children: [
            "· vencido ",
            _(_e.vencido),
            " (",
            _e.documentosVencidos,
            ")"
          ] })
        ] }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: _(_e.baseImponible) }) }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: _(_e.impuestos) }) }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: _(_e.total) }) }),
        i && /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: _(_e.pendiente) }) })
      ] }) })
    ] }),
    It > 1 && /* @__PURE__ */ l.jsxs("div", { className: "dx-paginas", children: [
      /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", disabled: P <= 1, onClick: () => Q(P - 1), children: "←" }),
      /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
        "Página ",
        P,
        " de ",
        It,
        " · ",
        Ve,
        " documentos"
      ] }),
      /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", disabled: P >= It, onClick: () => Q(P + 1), children: "→" })
    ] })
  ] });
}
function un(e) {
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
function Em(e) {
  const { api: t } = Dt(), [n, r] = x.useState(!1), [a, i] = x.useState([]), [o, s] = x.useState(null), [c, d] = x.useState(0), j = Sn(e.texto, 180), u = o === e.texto.trim() ? a : [], m = ea();
  x.useEffect(() => {
    if (!n) return;
    const g = m(), w = encodeURIComponent(j.trim());
    t.get(`/productos/buscar?texto=${w}&tamanoPagina=12`).then(($) => g() && (i($.elementos ?? []), s(j.trim()), d(0))).catch(() => g() && (i([]), s(j.trim())));
  }, [j, n]);
  function h(g) {
    var w;
    if (n && g.key === "Enter" && e.texto.trim() && !u.length) {
      g.preventDefault(), g.stopPropagation();
      return;
    }
    if (n && u.length) {
      if (g.key === "ArrowDown") return g.preventDefault(), d(($) => Math.min($ + 1, u.length - 1));
      if (g.key === "ArrowUp") return g.preventDefault(), d(($) => Math.max($ - 1, 0));
      if (g.key === "Enter") {
        g.preventDefault(), g.stopPropagation(), e.alElegir(u[c]), r(!1);
        return;
      }
    }
    if (g.key === "Escape") return r(!1);
    if (g.key === "F2") return g.preventDefault(), r(!0);
    (w = e.alTeclaFuera) == null || w.call(e, g);
  }
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-buscador", children: [
    /* @__PURE__ */ l.jsx(
      "input",
      {
        value: e.texto,
        placeholder: "Buscar artículo…",
        autoFocus: e.autoFocus,
        onChange: (g) => (e.alCambiarTexto(g.target.value), r(!0)),
        onFocus: (g) => g.target.select(),
        onBlur: () => setTimeout(() => r(!1), 150),
        onKeyDown: h,
        ...Object.fromEntries(Object.entries(e.datos ?? {}).map(([g, w]) => [`data-${g}`, w]))
      }
    ),
    n && u.length > 0 && /* @__PURE__ */ l.jsx("div", { className: "dx-lista", children: u.map((g, w) => /* @__PURE__ */ l.jsxs(
      "div",
      {
        className: "dx-opcion" + (w === c ? " activa" : ""),
        onMouseDown: ($) => ($.preventDefault(), e.alElegir(g), r(!1)),
        onMouseEnter: () => d(w),
        children: [
          /* @__PURE__ */ l.jsxs("span", { children: [
            g.referencia && /* @__PURE__ */ l.jsxs("span", { className: "mono muted", children: [
              g.referencia,
              " · "
            ] }),
            /* @__PURE__ */ l.jsx("strong", { children: g.nombre }),
            g.familia && /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
              " · ",
              g.familia
            ] })
          ] }),
          /* @__PURE__ */ l.jsxs("span", { className: "muted", style: { whiteSpace: "nowrap" }, children: [
            _(e.precioDe ? e.precioDe(g) : g.precioUnitario),
            "/",
            g.unidad,
            g.controlarStock && /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
              " · stock ",
              We(g.stock)
            ] })
          ] })
        ]
      },
      g.id
    )) })
  ] });
}
function Vo(e) {
  const [t, n] = x.useState(""), [r, a] = x.useState(!1), [i, o] = x.useState(0), s = e.terceros.find((u) => u.id === e.valor), c = x.useMemo(() => {
    const u = t.trim().toLowerCase();
    return e.terceros.filter((m) => m.activo !== !1 && (!u || m.nombre.toLowerCase().includes(u) || (m.nifFiscal ?? "").toLowerCase().includes(u))).slice(0, 30);
  }, [t, e.terceros]), d = x.useRef(null);
  function j(u) {
    e.alCambiar(u.id), n(""), a(!1);
  }
  return /* @__PURE__ */ l.jsxs("div", { children: [
    /* @__PURE__ */ l.jsx("label", { children: e.etiqueta }),
    /* @__PURE__ */ l.jsxs("div", { className: "dx-buscador", children: [
      /* @__PURE__ */ l.jsx(
        "input",
        {
          ref: d,
          disabled: e.deshabilitado,
          value: r ? t : s ? `${s.nombre}${s.nifFiscal ? " · " + s.nifFiscal : ""}` : t,
          placeholder: `Buscar ${e.etiqueta.toLowerCase()} por nombre o NIF…`,
          onFocus: () => (a(!0), n("")),
          onBlur: () => setTimeout(() => a(!1), 150),
          onChange: (u) => (n(u.target.value), o(0)),
          onKeyDown: (u) => {
            if (u.key === "ArrowDown") return u.preventDefault(), o((m) => Math.min(m + 1, c.length - 1));
            if (u.key === "ArrowUp") return u.preventDefault(), o((m) => Math.max(m - 1, 0));
            if (u.key === "Enter" && c[i]) return u.preventDefault(), j(c[i]);
            if (u.key === "Escape") return a(!1);
          }
        }
      ),
      r && c.length > 0 && /* @__PURE__ */ l.jsx("div", { className: "dx-lista", children: c.map((u, m) => /* @__PURE__ */ l.jsxs("div", { className: "dx-opcion" + (m === i ? " activa" : ""), onMouseDown: (h) => (h.preventDefault(), j(u)), onMouseEnter: () => o(m), children: [
        /* @__PURE__ */ l.jsx("strong", { children: u.nombre }),
        /* @__PURE__ */ l.jsx("span", { className: "muted", children: [u.nifFiscal, u.poblacion].filter(Boolean).join(" · ") })
      ] }, u.id)) })
    ] })
  ] });
}
const Im = { Porcentaje: "%", PorUnidad: "€/ud", PorKilo: "€/kg", Importe: "€" };
function Bo(e) {
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
          Im[s.calculo],
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
function xl(e) {
  var t;
  return (t = e.conceptos) != null && t.length ? /* @__PURE__ */ l.jsx("div", { className: "dx-aplicados", children: e.conceptos.map((n, r) => /* @__PURE__ */ l.jsxs("div", { children: [
    "· ",
    n.nombre,
    n.calculo === "Porcentaje" ? ` (${We(n.valor)} %)` : "",
    n.repartido ? " · del documento" : "",
    n.efecto === "Coste" ? " · coste" : "",
    ": ",
    /* @__PURE__ */ l.jsx("strong", { children: _(n.importe) })
  ] }, r)) }) : null;
}
const Fr = () => ({ clave: Jr(), productoId: null, descripcion: "", cantidad: 1, precio: null, dto: 0, iva: null });
function Pd(e) {
  const t = x.useRef(null), [n, r] = x.useState(/* @__PURE__ */ new Set()), a = e.modo === "venta", i = a ? 7 : 5, o = (u, m) => e.alCambiar(e.lineas.map((h) => h.clave === u ? { ...h, ...m } : h)), s = (u) => {
    const m = e.lineas.filter((h) => h.clave !== u);
    e.alCambiar(m.length ? m : [Fr()]);
  };
  function c(u, m) {
    var g;
    const h = (g = t.current) == null ? void 0 : g.querySelector(`[data-f="${u}"][data-c="${m}"]`);
    h == null || h.focus(), h instanceof HTMLInputElement && h.select();
  }
  function d(u) {
    const m = u.target, h = Number(m.dataset.f), g = Number(m.dataset.c);
    if (!(Number.isNaN(h) || Number.isNaN(g)))
      if (u.key === "Enter") {
        if (u.preventDefault(), g < i - 1) return c(h, g + 1);
        h === e.lineas.length - 1 ? (e.alCambiar([...e.lineas, Fr()]), setTimeout(() => c(h + 1, 0), 30)) : c(h + 1, 0);
      } else u.key === "ArrowDown" && m.tagName !== "SELECT" ? (u.preventDefault(), c(Math.min(h + 1, e.lineas.length - 1), g)) : u.key === "ArrowUp" && m.tagName !== "SELECT" && (u.preventDefault(), c(Math.max(h - 1, 0), g));
  }
  const j = (u) => r((m) => {
    const h = new Set(m);
    return h.has(u) ? h.delete(u) : h.add(u), h;
  });
  return /* @__PURE__ */ l.jsxs("div", { ref: t, className: "dx-rejilla", onKeyDown: d, children: [
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
      /* @__PURE__ */ l.jsx("tbody", { children: e.lineas.map((u, m) => {
        const h = e.calculos[m], g = (h == null ? void 0 : h.conceptos) ?? [], w = a && u.controlarStock && u.stock != null && u.cantidad > u.stock, $ = h && h.margen != null && h.importe ? h.margen / h.importe * 100 : null;
        return [
          /* @__PURE__ */ l.jsxs("tr", { className: m % 2 ? "dx-par" : "", children: [
            /* @__PURE__ */ l.jsxs("td", { children: [
              e.soloLectura ? /* @__PURE__ */ l.jsx("span", { className: "mono", children: u.referencia ?? "" }) : /* @__PURE__ */ l.jsx(
                Em,
                {
                  texto: u.referencia ?? (u.productoId ? u.descripcion : ""),
                  alCambiarTexto: (p) => o(u.clave, { referencia: p, ...p === "" ? { productoId: null } : {} }),
                  alElegir: (p) => (e.alElegirArticulo(u.clave, p), c(m, 2)),
                  precioDe: a ? void 0 : (p) => p.precioCompraPorUnidadCompra ?? p.precioCompra,
                  datos: { f: m, c: 0 }
                }
              ),
              u.productoId && /* @__PURE__ */ l.jsxs("div", { className: "dx-sub", children: [
                u.unidad && /* @__PURE__ */ l.jsx("span", { children: u.unidad }),
                u.controlarStock && /* @__PURE__ */ l.jsxs("span", { className: w ? "dx-rojo" : "", children: [
                  " · stock ",
                  We(u.stock)
                ] })
              ] })
            ] }),
            /* @__PURE__ */ l.jsxs("td", { children: [
              /* @__PURE__ */ l.jsx(
                "input",
                {
                  "data-f": m,
                  "data-c": 1,
                  value: u.descripcion,
                  placeholder: u.productoId ? "" : "Descripción (línea libre)",
                  disabled: e.soloLectura,
                  onChange: (p) => o(u.clave, { descripcion: p.target.value })
                }
              ),
              g.length > 0 && !n.has(u.clave) && /* @__PURE__ */ l.jsx("button", { type: "button", className: "dx-resumen-conc", onClick: () => j(u.clave), title: "Ver y cambiar los conceptos", children: g.map((p) => `${p.importe < 0 ? "−" : "+"} ${p.codigo.toLowerCase()} ${xe(Math.abs(p.importe))}${p.efecto === "Coste" ? " (coste)" : ""}`).join(" · ") })
            ] }),
            /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx(
              "input",
              {
                "data-f": m,
                "data-c": 2,
                className: "num",
                type: "number",
                step: "0.001",
                value: u.cantidad,
                disabled: e.soloLectura,
                onChange: (p) => o(u.clave, { cantidad: Number(p.target.value) })
              }
            ) }),
            /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx(
              "input",
              {
                "data-f": m,
                "data-c": 3,
                className: "num" + (u.precio == null ? " dx-auto" : ""),
                type: "number",
                step: "0.0001",
                disabled: e.soloLectura,
                value: u.precio ?? "",
                placeholder: h ? xe(h.precio) : "",
                title: u.precio == null ? "Precio de la tarifa del cliente o del artículo (escribe uno para fijarlo)" : "",
                onChange: (p) => o(u.clave, { precio: p.target.value === "" ? null : Number(p.target.value) })
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
                value: u.dto || (u.precio == null && (h != null && h.dto) ? h.dto : 0),
                disabled: e.soloLectura,
                onChange: (p) => o(u.clave, { dto: Number(p.target.value), precio: u.precio ?? (h == null ? void 0 : h.precio) ?? null })
              }
            ) }),
            a && /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsxs("select", { "data-f": m, "data-c": 5, value: u.iva ?? (h == null ? void 0 : h.iva) ?? "", disabled: e.soloLectura, onChange: (p) => o(u.clave, { iva: p.target.value || null }), children: [
              !u.iva && !(h != null && h.iva) && /* @__PURE__ */ l.jsx("option", { value: "", children: "Del artículo" }),
              e.ivas.map((p) => /* @__PURE__ */ l.jsx("option", { value: p.codigo, children: p.nombre }, p.codigo))
            ] }) }),
            /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: h ? _(h.importe) : "—" }) }),
            /* @__PURE__ */ l.jsx("td", { className: "num", children: a ? (h == null ? void 0 : h.margen) != null && /* @__PURE__ */ l.jsxs("span", { className: h.margen < 0 ? "dx-rojo" : "muted", children: [
              _(h.margen),
              $ != null && /* @__PURE__ */ l.jsxs("div", { className: "dx-sub", children: [
                xe($),
                " %"
              ] })
            ] }) : (h == null ? void 0 : h.costeUnitarioEntrada) != null && /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
              _(h.costeUnitarioEntrada),
              "/ud"
            ] }) }),
            /* @__PURE__ */ l.jsxs("td", { className: "right", style: { whiteSpace: "nowrap" }, children: [
              !e.soloLectura && e.catalogo.length > 0 && /* @__PURE__ */ l.jsx("button", { type: "button", className: "dx-icono" + (n.has(u.clave) ? " activo" : ""), title: "Conceptos de la línea", onClick: () => j(u.clave), "data-f": m, "data-c": a ? 6 : 4, children: "±" }),
              !e.soloLectura && /* @__PURE__ */ l.jsx("button", { type: "button", className: "dx-icono", title: "Quitar la línea", onClick: () => s(u.clave), children: "✕" })
            ] })
          ] }, u.clave),
          n.has(u.clave) && /* @__PURE__ */ l.jsx("tr", { className: "dx-fila-conc", children: /* @__PURE__ */ l.jsx("td", { colSpan: a ? 9 : 7, children: /* @__PURE__ */ l.jsx(Bo, { catalogo: e.catalogo, lista: u.conceptos, sugeridos: e.sugeridos[u.clave], alCambiar: (p) => o(u.clave, { conceptos: p }) }) }) }, u.clave + "c")
        ];
      }) })
    ] }),
    !e.soloLectura && /* @__PURE__ */ l.jsx("button", { type: "button", className: "btn small ghost", style: { marginTop: 8 }, onClick: () => (e.alCambiar([...e.lineas, Fr()]), setTimeout(() => c(e.lineas.length, 0), 30)), children: "+ Añadir línea" }),
    !e.soloLectura && /* @__PURE__ */ l.jsx("span", { className: "muted dx-ayuda", children: "Intro: siguiente casilla · ↑↓: cambiar de línea · F2: buscar artículo · el precio en gris es el de tarifa" })
  ] });
}
function Pm(e, t) {
  if (!e) return e;
  const n = e.toUpperCase();
  return (t === "Igic" ? { IVA21: "IGIC7", IVA10: "IGIC3", IVA4: "IGIC0", IVA0: "IGIC0", REAGP12: "REAGPIGIC", REAGP105: "REAGPIGIC" }[n] : { IGIC7: "IVA21", IGIC3: "IVA10", IGIC0: "IVA0", REAGPIGIC: "REAGP12" }[n]) ?? e;
}
const Fd = ["USD", "GBP", "CHF", "JPY", "CNY", "CAD", "MXN", "BRL", "SEK", "NOK", "DKK", "PLN", "MAD"], Ho = (e) => e.filter((t) => t.disponible > 0 && t.estado !== "Anulado" && !t.facturaId).sort((t, n) => t.fecha.localeCompare(n.fecha)), Rd = (e) => e.filter((t) => !!t.facturaId && (t.disponibleBase ?? 0) > 0 && t.estado !== "Anulado").sort((t, n) => t.fecha.localeCompare(n.fecha));
function _d(e, t) {
  let n = Math.round(t * 100);
  const r = [];
  for (const a of Ho(e)) {
    if (n <= 0) break;
    const i = Math.min(n, Math.round(a.disponible * 100));
    i > 0 && r.push({ id: a.id, importe: i / 100 }), n -= i;
  }
  return r;
}
const Fm = { presupuesto: "presupuesto", pedido: "pedido de venta", factura: "factura" };
function Td(e) {
  const t = /* @__PURE__ */ new Map();
  for (const n of e)
    for (const r of n.conceptos ?? [])
      if (r.repartido) {
        const a = t.get(r.conceptoId);
        t.set(r.conceptoId, { conceptoId: r.conceptoId, valor: r.calculo === "Importe" ? Oe(((a == null ? void 0 : a.valor) ?? 0) + r.valor) : r.valor });
      }
  return {
    porLinea: e.map((n) => (n.conceptos ?? []).filter((r) => !r.repartido).map((r) => ({ conceptoId: r.conceptoId, valor: r.valor }))),
    documento: [...t.values()]
  };
}
function Rm(e) {
  var Qo, Go, Ko, qo, Yo, Xo;
  const { api: t, anfitrion: n } = Dt(), r = !!((Qo = e.semilla) != null && Qo.rectificaId), [a, i] = x.useState([]), [o, s] = x.useState([]), [c, d] = x.useState([]), [j, u] = x.useState([]), [m, h] = x.useState([]), [g, w] = x.useState(((Go = e.semilla) == null ? void 0 : Go.clienteId) ?? ""), [$, p] = x.useState(e.tipo === "pedido" && ((Ko = e.semilla) != null && Ko.fecha) ? e.semilla.fecha : Ct()), [f, v] = x.useState(""), [k, T] = x.useState(""), [F, D] = x.useState(0), [C, M] = x.useState(!1), [b, P] = x.useState(null), [Q, ce] = x.useState(30), [Le, Ve] = x.useState(""), [pe, ge] = x.useState([Fr()]), [E, y] = x.useState([]), [L, B] = x.useState(!1), W = (qo = e.semilla) != null && qo.lineas.some((S) => /^(IGIC|REAGPIGIC)/i.test(S.codigoIva ?? "")) ? "Igic" : (Yo = e.semilla) != null && Yo.lineas.length ? "Iva" : null, [q, Se] = x.useState(W ?? "Iva"), [je, me] = x.useState(""), [Re, ot] = x.useState(null), Be = e.tipo === "factura" && !r && !!je, [z, st] = x.useState(null), [U, Et] = x.useState(""), [et, _e] = x.useState(!1), [It, zt] = x.useState(!1), [mn, he] = x.useState(!1), [Ie, Lt] = x.useState([]), [ct, R] = x.useState(!0), [G, A] = x.useState([]), [Y, le] = x.useState(!0), N = ea();
  x.useEffect(() => {
    t.get("/clientes").then(i).catch(() => i([])), t.get("/tipos-iva").then((S) => s(S.filter((V) => V.activo))).catch(() => s([])), t.get("/formas-pago").then((S) => d(S.filter((V) => V.activo))).catch(() => d([])), t.get("/series").then((S) => u([...new Set(S.filter((V) => V.tipoDocumento === "Factura").map((V) => V.prefijo))])).catch(() => u([])), t.get("/conceptos-linea?ambito=Ventas&activos=true").then(h).catch(() => h([])), t.get("/empresas/actual").then((S) => {
      B(!!S.operaEnAmbosTerritorios), Se(W ?? (S.territorioFiscal === "Canarias" ? "Igic" : "Iva"));
    }).catch(() => B(!1));
  }, [t]);
  function H(S) {
    Se(S), ge((V) => V.map((ee) => ({ ...ee, iva: ee.productoId ? null : Pm(ee.iva, S) })));
  }
  const we = x.useMemo(() => L ? o.filter((S) => (S.impuesto ?? "Iva") === q) : o, [L, o, q]);
  x.useEffect(() => {
    const S = e.semilla;
    if (!S || !S.lineas.length) return;
    const { porLinea: V, documento: ee } = Td(S.lineas), re = S.lineas.map((Z, wl) => ({
      clave: Jr(),
      productoId: Z.productoId ?? null,
      descripcion: Z.descripcion,
      cantidad: Z.cantidad,
      precio: Z.precioUnitario,
      dto: Z.porcentajeDescuento,
      iva: Z.codigoIva,
      conceptos: r ? [] : V[wl]
    }));
    ge(re), y(r ? [] : ee), Promise.all(re.map((Z) => Z.productoId ? t.get(`/productos/${Z.productoId}`).catch(() => null) : Promise.resolve(null))).then(
      (Z) => ge((wl) => wl.map((Zo, Rn) => Z[Rn] ? { ...Zo, referencia: Z[Rn].referencia ?? Z[Rn].nombre, unidad: Z[Rn].unidad, stock: Z[Rn].stock, controlarStock: Z[Rn].controlarStock } : Zo))
    );
  }, [e.semilla, t, r]);
  const K = a.find((S) => S.id === g);
  x.useEffect(() => {
    if (e.tipo !== "factura" || r || !g) {
      Lt([]), A([]);
      return;
    }
    t.get(`/anticipos?clienteId=${g}`).then((S) => {
      Lt(Ho(S)), A(Rd(S));
    }).catch(() => {
      Lt([]), A([]);
    });
  }, [t, g, e.tipo, r]);
  const te = Oe(Ie.reduce((S, V) => S + V.disponible, 0));
  x.useEffect(() => {
    K && (M(!!K.recargoEquivalencia), K.formaPagoDefectoId && T(K.formaPagoDefectoId));
  }, [K]);
  const xt = x.useMemo(() => pe.map((S, V) => ({ l: S, i: V })).filter(({ l: S }) => (S.productoId || S.descripcion.trim()) && S.cantidad > 0), [pe]), ta = x.useMemo(
    () => ({
      clienteId: g,
      fechaEmision: e.tipo === "factura" ? $ : null,
      serie: f || null,
      diasVencimiento: F,
      formaPagoId: k || null,
      recargoEquivalencia: C,
      porcentajeIrpf: b,
      conceptosDocumento: E,
      impuesto: L && !r ? q : null,
      descontarAnticipos: Y && G.length && !Be ? G.map((S) => ({ anticipoId: S.id })) : null,
      moneda: Be ? je : null,
      tasaCambio: Be ? Re : null,
      lineas: xt.map(({ l: S }) => ({
        cantidad: S.cantidad,
        descripcion: S.descripcion.trim() || null,
        precioUnitario: S.precio,
        codigoIva: S.iva,
        porcentajeDescuento: S.dto,
        productoId: S.productoId,
        ...r ? { conceptos: [] } : S.conceptos === void 0 ? {} : { conceptos: S.conceptos }
      }))
    }),
    [g, $, f, F, k, C, b, E, xt, e.tipo, r, Y, G, L, q, Be, je, Re]
  ), or = Sn(ta, 350);
  x.useEffect(() => {
    if (!or.clienteId || or.lineas.length === 0) {
      st(null), Et(or.clienteId ? "" : "Elige el cliente para calcular precios e importes.");
      return;
    }
    const S = N();
    _e(!0), t.post("/facturas/simular", or).then((V) => S() && (st(V), Et(""))).catch((V) => S() && (st(null), Et(V.message))).finally(() => S() && _e(!1));
  }, [or, t]);
  const jl = x.useMemo(() => {
    const S = pe.map(() => {
    });
    return z && xt.forEach(({ i: V }, ee) => {
      const re = z.lineas[ee];
      re && (S[V] = { precio: re.precioDivisa ?? re.precioUnitario, dto: re.porcentajeDescuento, iva: re.codigoIva, importe: re.baseDivisa ?? re.base, margen: re.productoId || re.costeUnitario || re.costeConceptos ? re.margen : void 0, conceptos: re.conceptos });
    }), S;
  }, [z, pe, xt]), Dd = x.useMemo(() => {
    const S = {};
    return pe.forEach((V, ee) => {
      var re;
      return S[V.clave] = (((re = jl[ee]) == null ? void 0 : re.conceptos) ?? []).filter((Z) => !Z.repartido).map((Z) => ({ conceptoId: Z.conceptoId, valor: Z.valor }));
    }), S;
  }, [pe, jl]);
  function zd(S, V) {
    ge(
      (ee) => ee.map(
        (re) => re.clave === S ? { ...re, productoId: V.id, referencia: V.referencia ?? V.nombre, descripcion: V.nombre, precio: null, dto: 0, iva: null, conceptos: void 0, unidad: V.unidad, stock: V.stock, controlarStock: V.controlarStock } : re
      )
    );
  }
  const Ld = x.useMemo(() => {
    const S = /* @__PURE__ */ new Map();
    for (const V of (z == null ? void 0 : z.lineas) ?? []) {
      const ee = S.get(V.codigoIva) ?? { base: 0, cuota: 0, pct: V.porcentajeIva };
      ee.base += V.base, ee.cuota += V.cuotaIva, S.set(V.codigoIva, ee);
    }
    return [...S.entries()];
  }, [z]), Nl = ((z == null ? void 0 : z.lineas) ?? []).reduce((S, V) => S + (V.base - V.margen), 0), Sl = z ? z.baseImponible - Nl : 0, Ad = (S) => {
    var V;
    return ((V = o.find((ee) => ee.codigo === S)) == null ? void 0 : V.nombre) ?? S;
  };
  async function Wo() {
    if (z) {
      zt(!0);
      try {
        const S = xt.map(({ l: ee }, re) => {
          const Z = z.lineas[re];
          return {
            cantidad: ee.cantidad,
            descripcion: Z.descripcion,
            // En divisa, el precio que se fija es el de la divisa (el de euros es su contravalor).
            precioUnitario: Z.precioDivisa ?? Z.precioUnitario,
            codigoIva: Z.codigoIva,
            porcentajeDescuento: Z.porcentajeDescuento,
            productoId: ee.productoId,
            ...r ? {} : ee.conceptos === void 0 ? {} : { conceptos: ee.conceptos }
          };
        });
        let V;
        if (r)
          V = (await t.post(`/facturas/${e.semilla.rectificaId}/rectificar`, { motivo: Le, lineas: S, fechaEmision: $, porcentajeIrpf: b, serie: f || null })).id;
        else if (e.tipo === "factura") {
          const ee = await t.post("/facturas", { ...ta, lineas: S });
          if (V = ee.id, ct && Ie.length) {
            let re = 0;
            try {
              for (const Z of _d(Ie, ee.total))
                await t.post(`/anticipos/${Z.id}/aplicar`, { facturaId: V, importe: Z.importe }), re += Z.importe;
              n.aviso(`Factura emitida. Aplicados ${_(re)} de anticipos (asiento 438 a 430).`, "ok");
            } catch (Z) {
              n.aviso(`Factura emitida, pero no se pudo aplicar el anticipo: ${Z.message}`, "err");
            }
            e.alGuardar(V);
            return;
          }
        } else if (e.tipo === "presupuesto") {
          const ee = { clienteId: g, diasValidez: Q, lineas: S, conceptosDocumento: E, impuesto: ta.impuesto };
          V = e.id ? (await t.put(`/presupuestos/${e.id}`, ee)).id : (await t.post("/presupuestos", ee)).id;
        } else {
          const ee = { clienteId: g, fecha: $, lineas: S, conceptosDocumento: E, impuesto: ta.impuesto };
          V = e.id ? (await t.put(`/pedidos-venta/${e.id}`, ee)).id : (await t.post("/pedidos-venta", ee)).id;
        }
        n.aviso(e.tipo === "factura" ? "Factura emitida." : "Documento guardado.", "ok"), e.alGuardar(V);
      } catch (S) {
        n.aviso(S.message, "err");
      } finally {
        zt(!1), he(!1);
      }
    }
  }
  const Md = r ? `Rectificativa de la factura ${((Xo = e.semilla) == null ? void 0 : Xo.rectificaNumero) ?? ""}` : `${e.id ? "Editar" : "Nuevo"} ${Fm[e.tipo]}`.replace("Nuevo factura", "Nueva factura"), $d = !!z && !et && (!r || Le.trim().length > 0);
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsxs("div", { className: "panel-head", children: [
        /* @__PURE__ */ l.jsx("h2", { children: Md }),
        /* @__PURE__ */ l.jsxs("div", { style: { display: "flex", gap: 8 }, children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: e.alCancelar, children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", disabled: !$d || It, onClick: () => e.tipo === "factura" ? he(!0) : Wo(), children: e.tipo === "factura" ? "Emitir factura" : "Guardar" })
        ] })
      ] }),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-cabecera", children: [
        /* @__PURE__ */ l.jsxs("div", { className: "dx-cab-campos", children: [
          /* @__PURE__ */ l.jsx(Vo, { terceros: a, valor: g, alCambiar: w, etiqueta: "Cliente", deshabilitado: r || e.tipo === "pedido" && !!e.id && !1 }),
          /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", children: [
            e.tipo !== "presupuesto" && /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: e.tipo === "factura" ? "Fecha de emisión" : "Fecha del pedido" }),
              /* @__PURE__ */ l.jsx("input", { type: "date", value: $, onChange: (S) => p(S.target.value) })
            ] }),
            e.tipo === "presupuesto" && /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Validez" }),
              /* @__PURE__ */ l.jsx("select", { value: Q, onChange: (S) => ce(Number(S.target.value)), children: [15, 30, 60, 90].map((S) => /* @__PURE__ */ l.jsxs("option", { value: S, children: [
                S,
                " días"
              ] }, S)) })
            ] }),
            L && !r && /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Territorio de la operación" }),
              /* @__PURE__ */ l.jsxs("select", { value: q, onChange: (S) => H(S.target.value), children: [
                /* @__PURE__ */ l.jsx("option", { value: "Iva", children: "Península y Baleares · IVA" }),
                /* @__PURE__ */ l.jsx("option", { value: "Igic", children: "Canarias · IGIC" })
              ] })
            ] }),
            e.tipo === "factura" && j.length > 0 && /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Serie" }),
              /* @__PURE__ */ l.jsxs("select", { value: f, onChange: (S) => v(S.target.value), children: [
                /* @__PURE__ */ l.jsx("option", { value: "", children: "La del cliente" }),
                j.map((S) => /* @__PURE__ */ l.jsx("option", { value: S, children: S }, S))
              ] })
            ] }),
            e.tipo === "factura" && !r && /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
              /* @__PURE__ */ l.jsxs("div", { children: [
                /* @__PURE__ */ l.jsx("label", { children: "Forma de pago" }),
                /* @__PURE__ */ l.jsxs("select", { value: k, onChange: (S) => T(S.target.value), children: [
                  /* @__PURE__ */ l.jsx("option", { value: "", children: "— Vencimiento a mano —" }),
                  c.map((S) => /* @__PURE__ */ l.jsx("option", { value: S.id, children: S.nombre }, S.id))
                ] })
              ] }),
              !k && /* @__PURE__ */ l.jsxs("div", { children: [
                /* @__PURE__ */ l.jsx("label", { children: "Vencimiento" }),
                /* @__PURE__ */ l.jsx("select", { value: F, onChange: (S) => D(Number(S.target.value)), children: [0, 15, 30, 45, 60, 90].map((S) => /* @__PURE__ */ l.jsx("option", { value: S, children: S ? `${S} días` : "Contado" }, S)) })
              ] })
            ] }),
            e.tipo === "factura" && !r && /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
              /* @__PURE__ */ l.jsxs("div", { children: [
                /* @__PURE__ */ l.jsx("label", { children: "Moneda" }),
                /* @__PURE__ */ l.jsxs("select", { value: je, onChange: (S) => me(S.target.value), children: [
                  /* @__PURE__ */ l.jsx("option", { value: "", children: "EUR · euros" }),
                  Fd.map((S) => /* @__PURE__ */ l.jsx("option", { value: S, children: S }, S))
                ] })
              ] }),
              Be && /* @__PURE__ */ l.jsxs("div", { children: [
                /* @__PURE__ */ l.jsxs("label", { children: [
                  "Tipo de cambio (€ por 1 ",
                  je,
                  ")"
                ] }),
                /* @__PURE__ */ l.jsx("input", { type: "number", step: "0.000001", min: "0", value: Re ?? "", placeholder: "El del día", onChange: (S) => ot(S.target.value === "" ? null : Number(S.target.value)) })
              ] })
            ] }),
            e.tipo === "factura" && /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Retención IRPF %" }),
              /* @__PURE__ */ l.jsx("input", { type: "number", step: "0.01", value: b ?? "", placeholder: String((K == null ? void 0 : K.porcentajeIrpfDefecto) ?? 0), onChange: (S) => P(S.target.value === "" ? null : Number(S.target.value)) })
            ] })
          ] }),
          e.tipo === "factura" && !r && /* @__PURE__ */ l.jsxs("label", { className: "dx-check", children: [
            /* @__PURE__ */ l.jsx("input", { type: "checkbox", checked: C, onChange: (S) => M(S.target.checked) }),
            " Recargo de equivalencia"
          ] }),
          r && /* @__PURE__ */ l.jsxs("div", { children: [
            /* @__PURE__ */ l.jsx("label", { children: "Motivo de la rectificación (obligatorio)" }),
            /* @__PURE__ */ l.jsx("input", { value: Le, onChange: (S) => Ve(S.target.value), placeholder: "Devolución, error en precio…" })
          ] })
        ] }),
        /* @__PURE__ */ l.jsx("div", { className: "dx-ficha", children: K ? /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("strong", { children: K.nombre }),
          /* @__PURE__ */ l.jsx("div", { className: "muted", children: [K.nifFiscal, K.poblacion, K.provincia].filter(Boolean).join(" · ") }),
          K.limiteRiesgo != null && /* @__PURE__ */ l.jsxs("div", { className: "muted", children: [
            "Límite de riesgo ",
            _(K.limiteRiesgo)
          ] }),
          K.tarifaId && /* @__PURE__ */ l.jsx("div", { className: "muted", children: "Con tarifa de precios propia" }),
          K.recargoEquivalencia && /* @__PURE__ */ l.jsx("div", { className: "muted", children: "En recargo de equivalencia" }),
          (z == null ? void 0 : z.avisoRiesgo) && /* @__PURE__ */ l.jsxs("div", { className: "dx-aviso", children: [
            "⚠ ",
            z.avisoRiesgo
          ] }),
          G.length > 0 && /* @__PURE__ */ l.jsxs("div", { className: "dx-anticipo", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              "🧾 Anticipos facturados pendientes de descontar: ",
              /* @__PURE__ */ l.jsx("strong", { children: _(G.reduce((S, V) => S + (V.disponibleBase ?? 0), 0)) }),
              " de base (",
              G.map((S) => S.facturaNumero).join(", "),
              ")."
            ] }),
            /* @__PURE__ */ l.jsxs("label", { className: "dx-check", children: [
              /* @__PURE__ */ l.jsx("input", { type: "checkbox", checked: Y, onChange: (S) => le(S.target.checked) }),
              "Descontar en esta factura (línea negativa con su base e IVA; hasta la base de la factura)"
            ] })
          ] }),
          te > 0 && /* @__PURE__ */ l.jsxs("div", { className: "dx-anticipo", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              "💶 Tiene ",
              /* @__PURE__ */ l.jsx("strong", { children: _(te) }),
              " en ",
              Ie.length === 1 ? "un anticipo pendiente" : `${Ie.length} anticipos pendientes`,
              " de aplicar."
            ] }),
            /* @__PURE__ */ l.jsxs("label", { className: "dx-check", children: [
              /* @__PURE__ */ l.jsx("input", { type: "checkbox", checked: ct, onChange: (S) => R(S.target.checked) }),
              "Aplicarlo al emitir",
              z ? ` (${_(Math.min(te, z.total))})` : ""
            ] })
          ] })
        ] }) : /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Elige un cliente: se aplican su tarifa, su forma de pago, su recargo y sus conceptos." }) })
      ] })
    ] }),
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsx(Pd, { modo: "venta", lineas: pe, alCambiar: ge, calculos: jl, ivas: we, catalogo: r ? [] : m, sugeridos: Dd, alElegirArticulo: zd }),
      !r && m.length > 0 && /* @__PURE__ */ l.jsx("div", { style: { marginTop: 10 }, children: /* @__PURE__ */ l.jsx(Bo, { catalogo: m, lista: E, alCambiar: (S) => y(S ?? []), documento: !0 }) })
    ] }),
    /* @__PURE__ */ l.jsxs("div", { className: "dx-pie", children: [
      /* @__PURE__ */ l.jsxs("div", { className: "dx-estado", children: [
        et && /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Calculando…" }),
        !et && U && /* @__PURE__ */ l.jsx("span", { className: "dx-rojo", children: U }),
        (z == null ? void 0 : z.mencionFiscal) && /* @__PURE__ */ l.jsx("div", { className: "muted", style: { fontSize: 12 }, children: z.mencionFiscal })
      ] }),
      /* @__PURE__ */ l.jsxs("div", { className: "panel dx-totales", children: [
        Ld.map(([S, V]) => /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
            Ad(S),
            " · base ",
            xe(V.base)
          ] }),
          /* @__PURE__ */ l.jsx("span", { children: _(V.cuota) })
        ] }, S)),
        z == null ? void 0 : z.lineas.filter((S) => S.anticipoId).map((S) => /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-tot-anticipo", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: S.descripcion }),
          /* @__PURE__ */ l.jsx("span", { children: _(S.base + S.cuotaIva + S.cuotaRecargo) })
        ] }, S.anticipoId)),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Base imponible" }),
          /* @__PURE__ */ l.jsx("span", { children: _(z == null ? void 0 : z.baseImponible) })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Impuestos" }),
          /* @__PURE__ */ l.jsx("span", { children: _(z == null ? void 0 : z.cuotaIva) })
        ] }),
        !!(z != null && z.recargoTotal) && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Recargo de equivalencia" }),
          /* @__PURE__ */ l.jsx("span", { children: _(z.recargoTotal) })
        ] }),
        !!(z != null && z.retencionIrpf) && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
            "Retención IRPF (",
            xe(z.porcentajeIrpf),
            " %)"
          ] }),
          /* @__PURE__ */ l.jsxs("span", { children: [
            "−",
            _(z.retencionIrpf)
          ] })
        ] }),
        z != null && z.moneda ? /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
            /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
              "Contravalor en euros (1 ",
              z.moneda,
              " = ",
              String(z.tasaCambio ?? 0).replace(".", ","),
              " €)"
            ] }),
            /* @__PURE__ */ l.jsx("span", { children: _(z.total) })
          ] }),
          /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-grande", children: [
            /* @__PURE__ */ l.jsxs("span", { children: [
              "Total ",
              z.moneda
            ] }),
            /* @__PURE__ */ l.jsxs("span", { children: [
              xe(z.totalDivisa ?? 0),
              " ",
              z.moneda
            ] })
          ] })
        ] }) : /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ l.jsx("span", { children: "Total" }),
          /* @__PURE__ */ l.jsx("span", { children: _(z == null ? void 0 : z.total) })
        ] }),
        z && Nl > 0 && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Coste · margen" }),
          /* @__PURE__ */ l.jsxs("span", { className: Sl < 0 ? "dx-rojo" : "muted", children: [
            _(Nl),
            " · ",
            _(Sl),
            " (",
            xe(z.baseImponible ? Sl / z.baseImponible * 100 : 0),
            " %)"
          ] })
        ] })
      ] })
    ] }),
    mn && z && /* @__PURE__ */ l.jsx(
      un,
      {
        titulo: r ? "Emitir la rectificativa" : "Emitir la factura",
        alCerrar: () => he(!1),
        acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => he(!1), children: "Revisar" }),
          /* @__PURE__ */ l.jsxs("button", { className: "btn small", disabled: It, onClick: Wo, children: [
            "Emitir ",
            z.moneda ? `${xe(z.totalDivisa ?? 0)} ${z.moneda}` : _(z.total)
          ] })
        ] }),
        children: /* @__PURE__ */ l.jsxs("p", { style: { margin: 0 }, children: [
          "Se emitirá una factura ",
          r ? "rectificativa " : "",
          "de ",
          /* @__PURE__ */ l.jsx("strong", { children: z.moneda ? `${xe(z.totalDivisa ?? 0)} ${z.moneda} (${_(z.total)})` : _(z.total) }),
          " a ",
          /* @__PURE__ */ l.jsx("strong", { children: K == null ? void 0 : K.nombre }),
          " con fecha ",
          $.split("-").reverse().join("/"),
          ". Una factura emitida no se modifica: queda numerada y encadenada en VeriFactu; para corregirla hay que rectificarla o anularla."
        ] })
      }
    )
  ] });
}
function _m(e) {
  var Ve, pe, ge;
  const { api: t, anfitrion: n } = Dt(), [r, a] = x.useState([]), [i, o] = x.useState([]), [s, c] = x.useState(((Ve = e.semilla) == null ? void 0 : Ve.proveedorId) ?? ""), [d, j] = x.useState(((pe = e.semilla) == null ? void 0 : pe.fecha) ?? Ct()), [u, m] = x.useState([Fr()]), [h, g] = x.useState([]), [w, $] = x.useState(null), [p, f] = x.useState(""), [v, k] = x.useState(!1), T = ea();
  x.useEffect(() => {
    t.get("/proveedores").then(a).catch(() => a([])), t.get("/conceptos-linea?ambito=Compras&activos=true").then(o).catch(() => o([]));
  }, [t]), x.useEffect(() => {
    const E = e.semilla;
    if (!E) return;
    const y = E.lineas.map((q) => ({ ...q, porcentajeDescuento: 0, codigoIva: "" })), { porLinea: L, documento: B } = Td(y), W = E.lineas.map((q, Se) => ({ clave: Jr(), productoId: q.productoId ?? null, descripcion: q.descripcion, cantidad: q.cantidad, precio: q.precioUnitario, dto: 0, iva: null, conceptos: L[Se] }));
    m(W), g(B), Promise.all(W.map((q) => q.productoId ? t.get(`/productos/${q.productoId}`).catch(() => null) : Promise.resolve(null))).then(
      (q) => m((Se) => Se.map((je, me) => q[me] ? { ...je, referencia: q[me].referencia ?? q[me].nombre, unidad: q[me].unidadCompra || q[me].unidad, stock: q[me].stock, controlarStock: q[me].controlarStock } : je))
    );
  }, [e.semilla, t]);
  const F = r.find((E) => E.id === s), D = x.useMemo(() => u.map((E, y) => ({ l: E, i: y })).filter(({ l: E }) => E.descripcion.trim() && E.cantidad > 0), [u]), C = x.useMemo(
    () => {
      var E, y;
      return {
        proveedorId: s || null,
        proveedorTexto: (F == null ? void 0 : F.nombre) ?? (((E = e.semilla) == null ? void 0 : E.proveedorTexto) || null),
        solicitudOrigenId: e.id ? null : ((y = e.semilla) == null ? void 0 : y.solicitudOrigenId) ?? null,
        fecha: d,
        conceptosDocumento: h,
        lineas: D.map(({ l: L }) => ({ descripcion: L.descripcion.trim(), cantidad: L.cantidad, precioUnitario: L.precio ?? 0, productoId: L.productoId, ...L.conceptos === void 0 ? {} : { conceptos: L.conceptos } }))
      };
    },
    [s, F, d, h, D, e.id, e.semilla]
  ), M = Sn(C, 350);
  x.useEffect(() => {
    if (!M.proveedorId || M.lineas.length === 0) {
      $(null), f(M.proveedorId ? "" : "Elige el proveedor.");
      return;
    }
    const E = T();
    t.post("/compras/pedidos/simular", M).then((y) => E() && ($(y), f(""))).catch((y) => E() && ($(null), f(y.message)));
  }, [M, t]);
  const b = x.useMemo(() => {
    const E = u.map(() => {
    });
    return D.forEach(({ i: y }, L) => {
      const B = w == null ? void 0 : w.lineas[L];
      B && (E[y] = { precio: B.precioUnitario, importe: B.importe, costeUnitarioEntrada: B.costeUnitarioEntrada, conceptos: B.conceptos });
    }), E;
  }, [w, u, D]), P = x.useMemo(() => {
    const E = {};
    return u.forEach((y, L) => {
      var B;
      return E[y.clave] = (((B = b[L]) == null ? void 0 : B.conceptos) ?? []).filter((W) => !W.repartido).map((W) => ({ conceptoId: W.conceptoId, valor: W.valor }));
    }), E;
  }, [u, b]);
  function Q(E, y) {
    const L = y.precioCompraPorUnidadCompra ?? y.precioCompra;
    m((B) => B.map((W) => W.clave === E ? { ...W, productoId: y.id, referencia: y.referencia ?? y.nombre, descripcion: y.nombre, precio: L, conceptos: void 0, unidad: y.unidadCompra || y.unidad, stock: y.stock, controlarStock: y.controlarStock } : W));
  }
  async function ce() {
    k(!0);
    try {
      const E = e.id ? await t.put(`/compras/pedidos/${e.id}`, C) : await t.post("/compras/pedidos", C);
      n.aviso("Pedido de compra guardado.", "ok"), e.alGuardar(E.id);
    } catch (E) {
      n.aviso(E.message, "err");
    } finally {
      k(!1);
    }
  }
  const Le = ((w == null ? void 0 : w.lineas) ?? []).reduce((E, y) => E + y.costeConceptos, 0);
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsxs("div", { className: "panel-head", children: [
        /* @__PURE__ */ l.jsx("h2", { children: e.id ? `Editar pedido ${((ge = e.semilla) == null ? void 0 : ge.numeroCompleto) ?? ""}` : "Nuevo pedido de compra" }),
        /* @__PURE__ */ l.jsxs("div", { style: { display: "flex", gap: 8 }, children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: e.alCancelar, children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", disabled: !w || v, onClick: ce, children: "Guardar" })
        ] })
      ] }),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-cabecera", children: [
        /* @__PURE__ */ l.jsxs("div", { className: "dx-cab-campos", children: [
          /* @__PURE__ */ l.jsx(Vo, { terceros: r, valor: s, alCambiar: c, etiqueta: "Proveedor", deshabilitado: !!e.id }),
          /* @__PURE__ */ l.jsx("div", { className: "dx-fila", children: /* @__PURE__ */ l.jsxs("div", { children: [
            /* @__PURE__ */ l.jsx("label", { children: "Fecha del pedido" }),
            /* @__PURE__ */ l.jsx("input", { type: "date", value: d, onChange: (E) => j(E.target.value) })
          ] }) })
        ] }),
        /* @__PURE__ */ l.jsx("div", { className: "dx-ficha", children: F ? /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("strong", { children: F.nombre }),
          /* @__PURE__ */ l.jsx("div", { className: "muted", children: [F.nifFiscal, F.poblacion, F.pais].filter(Boolean).join(" · ") })
        ] }) : /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Elige un proveedor: se aplican sus conceptos (pronto pago, portes…)." }) })
      ] })
    ] }),
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsx(Pd, { modo: "compra", lineas: u, alCambiar: m, calculos: b, ivas: [], catalogo: i, sugeridos: P, alElegirArticulo: Q }),
      i.length > 0 && /* @__PURE__ */ l.jsx("div", { style: { marginTop: 10 }, children: /* @__PURE__ */ l.jsx(Bo, { catalogo: i, lista: h, alCambiar: (E) => g(E ?? []), documento: !0 }) })
    ] }),
    /* @__PURE__ */ l.jsxs("div", { className: "dx-pie", children: [
      /* @__PURE__ */ l.jsx("div", { className: "dx-estado", children: p && /* @__PURE__ */ l.jsx("span", { className: "dx-rojo", children: p }) }),
      /* @__PURE__ */ l.jsxs("div", { className: "panel dx-totales", children: [
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ l.jsx("span", { children: "Total del pedido (sin impuestos)" }),
          /* @__PURE__ */ l.jsx("span", { children: _(w == null ? void 0 : w.total) })
        ] }),
        Le !== 0 && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Costes añadidos al almacén" }),
          /* @__PURE__ */ l.jsx("span", { children: _(Le) })
        ] }),
        /* @__PURE__ */ l.jsx("div", { className: "muted", style: { fontSize: 12, marginTop: 6 }, children: "El impuesto se aplica al facturar el pedido." })
      ] })
    ] })
  ] });
}
function gl(e) {
  return /* @__PURE__ */ l.jsxs("div", { className: "panel-head", children: [
    /* @__PURE__ */ l.jsxs("h2", { style: { display: "flex", alignItems: "center", gap: 10 }, children: [
      /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: e.volver, title: "Volver a la lista", children: "←" }),
      e.titulo,
      e.estado && /* @__PURE__ */ l.jsx("span", { className: Uo(e.estado), children: e.estado }),
      e.extra
    ] }),
    /* @__PURE__ */ l.jsx("div", { className: "dx-acciones", children: e.acciones })
  ] });
}
function be(e) {
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-dato", children: [
    /* @__PURE__ */ l.jsx("small", { children: e.etiqueta }),
    /* @__PURE__ */ l.jsx("div", { children: e.children })
  ] });
}
function Yn(e) {
  return /* @__PURE__ */ l.jsx("button", { type: "button", className: "dx-enlace", onClick: e.alPulsar, children: e.children });
}
function yl(e) {
  const [t, n] = x.useState(null), [r, a] = x.useState(""), i = x.useCallback(() => {
    e().then(n).catch((o) => a(o.message));
  }, []);
  return x.useEffect(i, [i]), { dato: t, error: r, recargar: i };
}
async function ft(e, t, n) {
  try {
    return await e(), n && t(n, "ok"), !0;
  } catch (r) {
    return t(r.message, "err"), !1;
  }
}
function Tm(e) {
  const { api: t, anfitrion: n, navegar: r } = Dt(), { dato: a, error: i, recargar: o } = yl(() => t.get(`/facturas/${e.id}`)), [s, c] = x.useState(null), [d, j] = x.useState(!1), [u, m] = x.useState("");
  x.useEffect(() => void t.get(`/facturas/${e.id}/saldo`).then(c).catch(() => c(null)), [t, e.id, a]);
  const [h, g] = x.useState([]), [w, $] = x.useState([]), [p, f] = x.useState(null);
  x.useEffect(() => {
    !(a != null && a.clienteId) || a.estado !== "Emitida" || t.get(`/anticipos?clienteId=${a.clienteId}`).then((C) => {
      g(Ho(C)), $(Rd(C));
    }).catch(() => g([]));
  }, [t, a]);
  const v = h.reduce((C, M) => C + M.disponible, 0);
  if (i) return /* @__PURE__ */ l.jsx("div", { className: "panel", children: /* @__PURE__ */ l.jsx("p", { className: "dx-rojo", children: i }) });
  if (!a) return /* @__PURE__ */ l.jsx("div", { className: "muted", children: "Cargando…" });
  const k = { clienteId: a.clienteId ?? void 0, lineas: a.lineas }, T = a.lineas.reduce((C, M) => C + (M.base - M.margen), 0), F = a.estado === "Emitida", D = a.lineas.some((C) => C.cuentaContable === "438" && !C.anticipoId);
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsx(
        gl,
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
            /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => Gr(n, `/facturas/${a.id}/pdf`).catch((C) => n.aviso(C.message, "err")), children: "PDF" }),
            a.tipo !== "Simplificada" && a.clienteNif && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => Gr(n, `/facturas/${a.id}/facturae.xml`).catch((C) => n.aviso(C.message, "err")), children: "Facturae" }),
            a.estado !== "Borrador" && a.tipo !== "Simplificada" && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", title: "Factura EDIFACT INVOIC (EANCOM) para clientes con EDI", onClick: () => hm(n, `/integraciones/edi/facturas/${a.id}/invoic`).catch((C) => n.aviso(C.message, "err")), children: "EDI" }),
            /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "factura", pantalla: "editor", semilla: k }), children: "Duplicar" }),
            F && a.tipo === "Ordinaria" && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "factura", pantalla: "editor", semilla: { ...k, rectificaId: a.id, rectificaNumero: a.numeroCompleto } }), children: "Rectificar" }),
            F && /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", onClick: () => j(!0), children: "Anular" })
          ] })
        }
      ),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ l.jsxs(be, { etiqueta: "Cliente", children: [
          /* @__PURE__ */ l.jsx("strong", { children: a.clienteNombre }),
          a.clienteNif && /* @__PURE__ */ l.jsx("div", { className: "muted mono", children: a.clienteNif }),
          /* @__PURE__ */ l.jsx("div", { className: "muted", children: [a.clienteCalle, a.clienteCodigoPostal, a.clientePoblacion].filter(Boolean).join(" ") })
        ] }),
        /* @__PURE__ */ l.jsxs(be, { etiqueta: "Emisión", children: [
          ke(a.fechaEmision),
          a.fechaOperacion !== a.fechaEmision && /* @__PURE__ */ l.jsxs("div", { className: "muted", children: [
            "Operación ",
            ke(a.fechaOperacion)
          ] })
        ] }),
        /* @__PURE__ */ l.jsx(be, { etiqueta: "Vencimiento", children: ke(a.fechaVencimiento) }),
        /* @__PURE__ */ l.jsxs(be, { etiqueta: "Cobro", children: [
          s ? s.pendiente <= 0 ? /* @__PURE__ */ l.jsx("span", { className: "pill ok", children: "Cobrada" }) : /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            "Pendiente ",
            /* @__PURE__ */ l.jsx("strong", { children: _(s.pendiente) }),
            s.liquidado > 0 && /* @__PURE__ */ l.jsxs("div", { className: "muted", children: [
              "Cobrado ",
              _(s.liquidado)
            ] })
          ] }) : "—",
          s && s.pendiente > 0 && F && n.irA && /* @__PURE__ */ l.jsx("div", { children: /* @__PURE__ */ l.jsx(Yn, { alPulsar: () => n.irA("cobros"), children: "Registrar cobro" }) }),
          s && s.pendiente > 0 && F && w.length > 0 && !D && /* @__PURE__ */ l.jsxs("div", { className: "dx-anticipo", children: [
            "🧾 Anticipos facturados sin descontar: ",
            /* @__PURE__ */ l.jsx("strong", { children: _(w.reduce((C, M) => C + (M.disponibleBase ?? 0), 0)) }),
            " de base. Se descuentan al hacer la siguiente factura (o rectifica esta para incluirlos)."
          ] }),
          s && s.pendiente > 0 && F && v > 0 && /* @__PURE__ */ l.jsxs("div", { className: "dx-anticipo", children: [
            "💶 Anticipos del cliente sin aplicar: ",
            /* @__PURE__ */ l.jsx("strong", { children: _(v) }),
            /* @__PURE__ */ l.jsx("div", { children: /* @__PURE__ */ l.jsx(Yn, { alPulsar: () => f(_d(h, s.pendiente)), children: "Aplicar a esta factura" }) })
          ] })
        ] })
      ] }),
      a.motivoRectificacion && /* @__PURE__ */ l.jsxs("p", { className: "muted", children: [
        "Rectifica: ",
        a.motivoRectificacion,
        a.rectificaFacturaId && /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          " · ",
          /* @__PURE__ */ l.jsx(Yn, { alPulsar: () => r({ tipo: "factura", pantalla: "vista", id: a.rectificaFacturaId }), children: "ver la factura original" })
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
        /* @__PURE__ */ l.jsx("tbody", { children: a.lineas.map((C, M) => /* @__PURE__ */ l.jsxs("tr", { children: [
          /* @__PURE__ */ l.jsxs("td", { children: [
            C.descripcion,
            /* @__PURE__ */ l.jsx(xl, { conceptos: C.conceptos })
          ] }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: We(C.cantidad) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: _(C.precioUnitario) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: C.porcentajeDescuento ? `${xe(C.porcentajeDescuento)} %` : "" }),
          /* @__PURE__ */ l.jsxs("td", { children: [
            C.codigoIva,
            " · ",
            xe(C.porcentajeIva),
            " %"
          ] }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: _(C.base) }) }),
          /* @__PURE__ */ l.jsx("td", { className: "num muted", children: C.costeUnitario || C.costeConceptos ? _(C.margen) : "" })
        ] }, M)) })
      ] }),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-totales-vista", children: [
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Base imponible" }),
          /* @__PURE__ */ l.jsx("span", { children: _(a.baseImponible) })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: a.impuesto === "Igic" ? "IGIC" : "IVA" }),
          /* @__PURE__ */ l.jsx("span", { children: _(a.cuotaIva) })
        ] }),
        !!a.recargoTotal && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Recargo de equivalencia" }),
          /* @__PURE__ */ l.jsx("span", { children: _(a.recargoTotal) })
        ] }),
        !!a.retencionIrpf && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
            "Retención IRPF (",
            xe(a.porcentajeIrpf),
            " %)"
          ] }),
          /* @__PURE__ */ l.jsxs("span", { children: [
            "−",
            _(a.retencionIrpf)
          ] })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ l.jsx("span", { children: "Total" }),
          /* @__PURE__ */ l.jsx("span", { children: _(a.total) })
        ] }),
        T > 0 && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Margen" }),
          /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
            _(a.baseImponible - T),
            " (",
            xe(a.baseImponible ? (a.baseImponible - T) / a.baseImponible * 100 : 0),
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
      un,
      {
        titulo: `Aplicar anticipos a ${a.numeroCompleto}`,
        alCerrar: () => f(null),
        acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => f(null), children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", disabled: !p.some((C) => C.importe > 0), onClick: async () => {
            for (const C of p.filter((M) => M.importe > 0))
              if (!await ft(() => t.post(`/anticipos/${C.id}/aplicar`, { facturaId: a.id, importe: C.importe }), n.aviso, "Anticipo aplicado.")) return;
            f(null), o();
          }, children: "Aplicar" })
        ] }),
        children: [
          /* @__PURE__ */ l.jsxs("p", { className: "muted", style: { marginTop: 0 }, children: [
            "Se registra el cobro de la factura con el anticipo y su asiento de cancelación: 438 Anticipos de clientes al debe, 430 Clientes al haber. Pendiente de la factura: ",
            /* @__PURE__ */ l.jsx("strong", { children: _(s == null ? void 0 : s.pendiente) }),
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
              const M = p.find((b) => b.id === C.id);
              return /* @__PURE__ */ l.jsxs("tr", { children: [
                /* @__PURE__ */ l.jsx("td", { children: ke(C.fecha) }),
                /* @__PURE__ */ l.jsx("td", { className: "muted", children: C.concepto }),
                /* @__PURE__ */ l.jsx("td", { className: "num", children: _(C.disponible) }),
                /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx(
                  "input",
                  {
                    type: "number",
                    step: "0.01",
                    min: 0,
                    max: C.disponible,
                    style: { width: 110, textAlign: "right" },
                    value: (M == null ? void 0 : M.importe) ?? 0,
                    onChange: (b) => {
                      const P = Math.max(0, Math.min(C.disponible, Number(b.target.value) || 0));
                      f([...p.filter((Q) => Q.id !== C.id), { id: C.id, importe: P }]);
                    }
                  }
                ) })
              ] }, C.id);
            }) })
          ] })
        ]
      }
    ),
    d && /* @__PURE__ */ l.jsxs(
      un,
      {
        titulo: `Anular ${a.numeroCompleto}`,
        alCerrar: () => j(!1),
        acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => j(!1), children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", disabled: !u.trim(), onClick: async () => await ft(() => t.post(`/facturas/${a.id}/anular`, { motivo: u }), n.aviso, "Factura anulada.") && (j(!1), o()), children: "Anular" })
        ] }),
        children: [
          /* @__PURE__ */ l.jsx("p", { className: "muted", style: { marginTop: 0 }, children: "La factura no se borra: queda anulada con un registro de anulación VeriFactu. Si hay que corregir importes, rectifícala en lugar de anularla." }),
          /* @__PURE__ */ l.jsx("label", { children: "Motivo" }),
          /* @__PURE__ */ l.jsx("input", { value: u, onChange: (C) => m(C.target.value), autoFocus: !0 })
        ]
      }
    )
  ] });
}
function Dm(e) {
  const { api: t, anfitrion: n, navegar: r } = Dt(), { dato: a, error: i, recargar: o } = yl(() => t.get(`/presupuestos/${e.id}`));
  if (i) return /* @__PURE__ */ l.jsx("div", { className: "panel", children: /* @__PURE__ */ l.jsx("p", { className: "dx-rojo", children: i }) });
  if (!a) return /* @__PURE__ */ l.jsx("div", { className: "muted", children: "Cargando…" });
  const s = a.estado === "Borrador", c = { clienteId: a.clienteId, lineas: a.lineas };
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsx(
        gl,
        {
          titulo: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            "Presupuesto ",
            /* @__PURE__ */ l.jsx("span", { className: "mono", children: a.numeroCompleto })
          ] }),
          estado: a.estado,
          volver: () => r({ tipo: "presupuesto", pantalla: "lista" }),
          acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => Gr(n, `/presupuestos/${a.id}/pdf`).catch((d) => n.aviso(d.message, "err")), children: "PDF" }),
            /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "presupuesto", pantalla: "editor", semilla: c }), children: "Duplicar" }),
            s && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "presupuesto", pantalla: "editor", id: a.id, semilla: c }), children: "Editar" }),
            s && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: async () => {
              try {
                const d = await t.post("/pedidos-venta/desde-presupuesto", { presupuestoId: a.id });
                n.aviso("Pedido creado.", "ok"), r({ tipo: "pedido", pantalla: "vista", id: d.id });
              } catch (d) {
                n.aviso(d.message, "err");
              }
            }, children: "Pasar a pedido" }),
            s && /* @__PURE__ */ l.jsx("button", { className: "btn small", onClick: async () => {
              try {
                const d = await t.post(`/presupuestos/${a.id}/aceptar`, {});
                n.aviso("Factura emitida.", "ok"), r({ tipo: "factura", pantalla: "vista", id: d.id });
              } catch (d) {
                n.aviso(d.message, "err");
              }
            }, children: "Aceptar y facturar" }),
            s && /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", onClick: async () => await ft(() => t.post(`/presupuestos/${a.id}/rechazar`, {}), n.aviso, "Presupuesto rechazado.") && o(), children: "Rechazar" })
          ] })
        }
      ),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ l.jsx(be, { etiqueta: "Cliente", children: /* @__PURE__ */ l.jsx("strong", { children: a.clienteNombre }) }),
        /* @__PURE__ */ l.jsx(be, { etiqueta: "Fecha", children: ke(a.fecha) }),
        /* @__PURE__ */ l.jsx(be, { etiqueta: "Válido hasta", children: ke(a.validez) }),
        /* @__PURE__ */ l.jsx(be, { etiqueta: "Factura", children: a.facturaId ? /* @__PURE__ */ l.jsx(Yn, { alPulsar: () => r({ tipo: "factura", pantalla: "vista", id: a.facturaId }), children: "ver la factura" }) : "—" })
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
        /* @__PURE__ */ l.jsx("tbody", { children: a.lineas.map((d, j) => /* @__PURE__ */ l.jsxs("tr", { children: [
          /* @__PURE__ */ l.jsxs("td", { children: [
            d.descripcion,
            /* @__PURE__ */ l.jsx(xl, { conceptos: d.conceptos })
          ] }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: We(d.cantidad) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: _(d.precioUnitario) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: d.porcentajeDescuento ? `${xe(d.porcentajeDescuento)} %` : "" }),
          /* @__PURE__ */ l.jsx("td", { children: d.codigoIva }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: _(d.base) }) })
        ] }, j)) })
      ] }),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-totales-vista", children: [
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Base imponible" }),
          /* @__PURE__ */ l.jsx("span", { children: _(a.baseImponible) })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Impuestos" }),
          /* @__PURE__ */ l.jsx("span", { children: _(a.cuotaIva) })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ l.jsx("span", { children: "Total" }),
          /* @__PURE__ */ l.jsx("span", { children: _(a.total) })
        ] })
      ] })
    ] })
  ] });
}
function zm(e) {
  const { api: t, anfitrion: n, navegar: r } = Dt(), { dato: a, error: i, recargar: o } = yl(() => t.get(`/pedidos-venta/${e.id}`)), [s, c] = x.useState([]), [d, j] = x.useState([]), [u, m] = x.useState(null), [h, g] = x.useState(""), [w, $] = x.useState(Ct()), [p, f] = x.useState(!1), [v, k] = x.useState(Ct()), [T, F] = x.useState("");
  if (x.useEffect(() => void t.get(`/pedidos-venta/${e.id}/albaranes`).then(c).catch(() => c([])), [t, e.id, a]), x.useEffect(() => void t.get("/formas-pago").then((P) => j(P.filter((Q) => Q.activo))).catch(() => j([])), [t]), i) return /* @__PURE__ */ l.jsx("div", { className: "panel", children: /* @__PURE__ */ l.jsx("p", { className: "dx-rojo", children: i }) });
  if (!a) return /* @__PURE__ */ l.jsx("div", { className: "muted", children: "Cargando…" });
  const D = (a.estado === "Borrador" || a.estado === "Confirmado") && a.lineas.every((P) => P.cantidadServida === 0), C = a.lineas.some((P) => P.pendienteServir > 0), M = a.estado !== "Cancelado" && a.estado !== "Facturado", b = { clienteId: a.clienteId, fecha: a.fecha, lineas: a.lineas };
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsx(
        gl,
        {
          titulo: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            "Pedido de venta ",
            /* @__PURE__ */ l.jsx("span", { className: "mono", children: a.numeroCompleto })
          ] }),
          estado: a.estado,
          volver: () => r({ tipo: "pedido", pantalla: "lista" }),
          acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => Gr(n, `/pedidos-venta/${a.id}/pdf`).catch((P) => n.aviso(P.message, "err")), children: "PDF" }),
            /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "pedido", pantalla: "editor", semilla: { ...b, fecha: void 0 } }), children: "Duplicar" }),
            D && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "pedido", pantalla: "editor", id: a.id, semilla: b }), children: "Editar" }),
            a.estado === "Borrador" && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: async () => await ft(() => t.post(`/pedidos-venta/${a.id}/confirmar`), n.aviso, "Pedido confirmado.") && o(), children: "Confirmar" }),
            M && a.estado !== "Borrador" && C && n.reservarPales && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", title: "Apartar palés cerrados para este pedido", onClick: () => n.reservarPales(a.id), children: "Reservar palés" }),
            M && a.estado !== "Borrador" && C && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => m(Object.fromEntries(a.lineas.map((P) => [P.id, P.pendienteServir]))), children: "Entregar (albarán)" }),
            M && a.estado !== "Borrador" && /* @__PURE__ */ l.jsx("button", { className: "btn small", onClick: () => f(!0), children: "Facturar" }),
            M && /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", onClick: async () => window.confirm("¿Cancelar el pedido?") && await ft(() => t.post(`/pedidos-venta/${a.id}/cancelar`), n.aviso, "Pedido cancelado.") && o(), children: "Cancelar" })
          ] })
        }
      ),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ l.jsx(be, { etiqueta: "Cliente", children: /* @__PURE__ */ l.jsx("strong", { children: a.clienteNombre }) }),
        /* @__PURE__ */ l.jsx(be, { etiqueta: "Fecha", children: ke(a.fecha) }),
        /* @__PURE__ */ l.jsx(be, { etiqueta: "Viene de", children: a.presupuestoOrigenId ? /* @__PURE__ */ l.jsx(Yn, { alPulsar: () => r({ tipo: "presupuesto", pantalla: "vista", id: a.presupuestoOrigenId }), children: "presupuesto" }) : "—" }),
        /* @__PURE__ */ l.jsx(be, { etiqueta: "Factura", children: a.facturaId ? /* @__PURE__ */ l.jsx(Yn, { alPulsar: () => r({ tipo: "factura", pantalla: "vista", id: a.facturaId }), children: "ver la factura" }) : "—" })
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
            /* @__PURE__ */ l.jsx(xl, { conceptos: P.conceptos })
          ] }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: We(P.cantidad) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: We(P.cantidadServida) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: P.pendienteServir > 0 ? /* @__PURE__ */ l.jsx("strong", { children: We(P.pendienteServir) }) : "—" }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: _(P.precioUnitario) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: P.porcentajeDescuento ? `${xe(P.porcentajeDescuento)} %` : "" }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: _(P.base) }) })
        ] }, P.id)) })
      ] }),
      /* @__PURE__ */ l.jsx("div", { className: "dx-totales-vista", children: /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-grande", children: [
        /* @__PURE__ */ l.jsx("span", { children: "Total (sin impuestos)" }),
        /* @__PURE__ */ l.jsx("span", { children: _(a.total) })
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
          /* @__PURE__ */ l.jsx("td", { children: ke(P.fecha) }),
          /* @__PURE__ */ l.jsx("td", { className: "muted", children: P.referencia }),
          /* @__PURE__ */ l.jsx("td", { className: "muted", children: P.lineas.map((Q) => `${We(Q.cantidad)} × ${Q.descripcion}`).join(" · ") }),
          /* @__PURE__ */ l.jsx("td", { className: "right", children: !P.anulado && a.estado !== "Facturado" && /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", onClick: async () => {
            const Q = window.prompt("Motivo de la anulación del albarán:");
            Q !== null && await ft(() => t.post(`/pedidos-venta/${a.id}/albaranes/${P.id}/anular`, { motivo: Q || null }), n.aviso, "Albarán anulado.") && o();
          }, children: "Anular" }) })
        ] }, P.id)) })
      ] }) : /* @__PURE__ */ l.jsx("p", { className: "muted", style: { margin: 0 }, children: "Sin entregas todavía." })
    ] }),
    u && /* @__PURE__ */ l.jsxs(
      un,
      {
        titulo: "Entrega (albarán de venta)",
        alCerrar: () => m(null),
        ancho: 640,
        acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => m(null), children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", onClick: async () => await ft(() => t.post(`/pedidos-venta/${a.id}/entregar`, { fecha: w, referencia: h || null, lineas: Object.entries(u).filter(([, P]) => P > 0).map(([P, Q]) => ({ lineaPedidoId: P, cantidad: Q })) }), n.aviso, "Albarán creado.") && (m(null), o()), children: "Crear albarán" })
        ] }),
        children: [
          /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Fecha" }),
              /* @__PURE__ */ l.jsx("input", { type: "date", value: w, onChange: (P) => $(P.target.value) })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Referencia" }),
              /* @__PURE__ */ l.jsx("input", { value: h, onChange: (P) => g(P.target.value) })
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
              /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx("input", { className: "num", type: "number", step: "0.001", value: u[P.id] ?? 0, onChange: (Q) => m({ ...u, [P.id]: Number(Q.target.value) }) }) })
            ] }, P.id)) })
          ] })
        ]
      }
    ),
    p && /* @__PURE__ */ l.jsxs(
      un,
      {
        titulo: `Facturar el pedido ${a.numeroCompleto}`,
        alCerrar: () => f(!1),
        acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => f(!1), children: "Cancelar" }),
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
              /* @__PURE__ */ l.jsx("input", { type: "date", value: v, onChange: (P) => k(P.target.value) })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Forma de pago" }),
              /* @__PURE__ */ l.jsxs("select", { value: T, onChange: (P) => F(P.target.value), children: [
                /* @__PURE__ */ l.jsx("option", { value: "", children: "La del cliente" }),
                d.map((P) => /* @__PURE__ */ l.jsx("option", { value: P.id, children: P.nombre }, P.id))
              ] })
            ] })
          ] })
        ]
      }
    )
  ] });
}
function Lm(e) {
  const { api: t, anfitrion: n, navegar: r } = Dt(), { dato: a, error: i, recargar: o } = yl(() => t.get(`/compras/pedidos/${e.id}`)), [s, c] = x.useState([]), [d, j] = x.useState([]), [u, m] = x.useState([]), [h, g] = x.useState(null), [w, $] = x.useState(""), [p, f] = x.useState(""), [v, k] = x.useState(Ct()), [T, F] = x.useState(!1), [D, C] = x.useState("IVA21"), [M, b] = x.useState(0), [P, Q] = x.useState(""), [ce, Le] = x.useState(Ct());
  if (x.useEffect(() => void t.get(`/compras/pedidos/${e.id}/albaranes`).then(c).catch(() => c([])), [t, e.id, a]), x.useEffect(() => {
    t.get("/inventario/almacenes").then((y) => (j(y), y[0] && $(y[0].id))).catch(() => j([])), t.get("/tipos-iva").then((y) => m(y.filter((L) => L.activo))).catch(() => m([]));
  }, [t]), i) return /* @__PURE__ */ l.jsx("div", { className: "panel", children: /* @__PURE__ */ l.jsx("p", { className: "dx-rojo", children: i }) });
  if (!a) return /* @__PURE__ */ l.jsx("div", { className: "muted", children: "Cargando…" });
  const Ve = (a.estado === "Borrador" || a.estado === "Confirmado") && a.lineas.every((y) => y.cantidadRecibida === 0 && y.cantidadFacturada === 0) && !a.empresaOrigenId, pe = a.estado !== "Cancelado" && a.estado !== "Facturado", ge = a.lineas.some((y) => y.pendienteRecibir > 0), E = a.lineas.reduce((y, L) => y + L.costeConceptos, 0);
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsx(
        gl,
        {
          titulo: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            "Pedido de compra ",
            /* @__PURE__ */ l.jsx("span", { className: "mono", children: a.numeroCompleto })
          ] }),
          estado: a.estado,
          volver: () => r({ tipo: "compra", pantalla: "lista" }),
          acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => Gr(n, `/compras/pedidos/${a.id}/pdf`).catch((y) => n.aviso(y.message, "err")), children: "PDF" }),
            !a.empresaOrigenId && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "compra", pantalla: "editor", semilla: { ...a, fecha: Ct() } }), children: "Duplicar" }),
            Ve && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "compra", pantalla: "editor", id: a.id, semilla: a }), children: "Editar" }),
            a.estado === "Borrador" && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: async () => await ft(() => t.post(`/compras/pedidos/${a.id}/confirmar`), n.aviso, "Pedido confirmado.") && o(), children: "Confirmar" }),
            pe && a.estado !== "Borrador" && ge && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => g(Object.fromEntries(a.lineas.map((y) => [y.id, { cantidad: y.pendienteRecibir, lote: "" }]))), children: "Recibir mercancía" }),
            pe && a.estado !== "Borrador" && /* @__PURE__ */ l.jsx("button", { className: "btn small", onClick: () => F(!0), children: "Facturar" }),
            pe && !a.empresaOrigenId && /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", onClick: async () => window.confirm("¿Cancelar el pedido?") && await ft(() => t.post(`/compras/pedidos/${a.id}/cancelar`), n.aviso, "Pedido cancelado.") && o(), children: "Cancelar" })
          ] })
        }
      ),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ l.jsx(be, { etiqueta: "Proveedor", children: /* @__PURE__ */ l.jsx("strong", { children: a.proveedorTexto }) }),
        /* @__PURE__ */ l.jsx(be, { etiqueta: "Fecha", children: ke(a.fecha) }),
        /* @__PURE__ */ l.jsx(be, { etiqueta: "Total", children: _(a.total) }),
        /* @__PURE__ */ l.jsx(be, { etiqueta: "Costes añadidos", children: E ? _(E) : "—" })
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
      /* @__PURE__ */ l.jsx("tbody", { children: a.lineas.map((y) => /* @__PURE__ */ l.jsxs("tr", { children: [
        /* @__PURE__ */ l.jsxs("td", { children: [
          y.descripcion,
          /* @__PURE__ */ l.jsx(xl, { conceptos: y.conceptos })
        ] }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: We(y.cantidad) }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: We(y.cantidadRecibida) }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: y.pendienteRecibir > 0 ? /* @__PURE__ */ l.jsx("strong", { children: We(y.pendienteRecibir) }) : "—" }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: _(y.precioUnitario) }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: _(y.importe) }) }),
        /* @__PURE__ */ l.jsxs("td", { className: "num muted", children: [
          _(y.costeUnitarioEntrada),
          "/ud"
        ] })
      ] }, y.id)) })
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
        /* @__PURE__ */ l.jsx("tbody", { children: s.map((y) => {
          var L;
          return /* @__PURE__ */ l.jsxs("tr", { children: [
            /* @__PURE__ */ l.jsxs("td", { className: "mono", children: [
              /* @__PURE__ */ l.jsx("strong", { children: y.numeroCompleto }),
              " ",
              y.anulado && /* @__PURE__ */ l.jsx("span", { className: "pill neg", title: y.motivoAnulacion ?? "", children: "Anulado" })
            ] }),
            /* @__PURE__ */ l.jsx("td", { children: ke(y.fecha) }),
            /* @__PURE__ */ l.jsx("td", { className: "muted", children: y.referencia }),
            /* @__PURE__ */ l.jsx("td", { className: "muted", children: ((L = d.find((B) => B.id === y.almacenId)) == null ? void 0 : L.nombre) ?? "—" }),
            /* @__PURE__ */ l.jsx("td", { className: "muted", children: y.lineas.map((B) => `${We(B.cantidad)} × ${B.descripcion}`).join(" · ") }),
            /* @__PURE__ */ l.jsx("td", { className: "right", children: !y.anulado && a.estado !== "Facturado" && /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", onClick: async () => {
              const B = window.prompt("Motivo de la anulación del albarán:");
              B !== null && await ft(() => t.post(`/compras/pedidos/${a.id}/albaranes/${y.id}/anular`, { motivo: B || null }), n.aviso, "Albarán anulado.") && o();
            }, children: "Anular" }) })
          ] }, y.id);
        }) })
      ] }) : /* @__PURE__ */ l.jsx("p", { className: "muted", style: { margin: 0 }, children: "Sin recepciones todavía." })
    ] }),
    h && /* @__PURE__ */ l.jsxs(
      un,
      {
        titulo: "Recepción de mercancía",
        alCerrar: () => g(null),
        ancho: 680,
        acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => g(null), children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", onClick: async () => await ft(() => t.post(`/compras/pedidos/${a.id}/recibir`, { fecha: v, referencia: p || null, almacenId: w || null, lineas: Object.entries(h).filter(([, y]) => y.cantidad > 0).map(([y, L]) => ({ lineaPedidoId: y, cantidad: L.cantidad, lote: L.lote || null })) }), n.aviso, "Recepción registrada.") && (g(null), o()), children: "Registrar recepción" })
        ] }),
        children: [
          /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Fecha" }),
              /* @__PURE__ */ l.jsx("input", { type: "date", value: v, onChange: (y) => k(y.target.value) })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Albarán del proveedor" }),
              /* @__PURE__ */ l.jsx("input", { value: p, onChange: (y) => f(y.target.value) })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Almacén" }),
              /* @__PURE__ */ l.jsxs("select", { value: w, onChange: (y) => $(y.target.value), children: [
                /* @__PURE__ */ l.jsx("option", { value: "", children: "Sin entrada en almacén" }),
                d.map((y) => /* @__PURE__ */ l.jsx("option", { value: y.id, children: y.nombre }, y.id))
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
            /* @__PURE__ */ l.jsx("tbody", { children: a.lineas.filter((y) => y.pendienteRecibir > 0).map((y) => {
              var L, B;
              return /* @__PURE__ */ l.jsxs("tr", { children: [
                /* @__PURE__ */ l.jsx("td", { children: y.descripcion }),
                /* @__PURE__ */ l.jsx("td", { className: "num", children: We(y.pendienteRecibir) }),
                /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx("input", { className: "num", type: "number", step: "0.001", value: ((L = h[y.id]) == null ? void 0 : L.cantidad) ?? 0, onChange: (W) => g({ ...h, [y.id]: { ...h[y.id], cantidad: Number(W.target.value) } }) }) }),
                /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx("input", { value: ((B = h[y.id]) == null ? void 0 : B.lote) ?? "", onChange: (W) => g({ ...h, [y.id]: { ...h[y.id], lote: W.target.value } }) }) })
              ] }, y.id);
            }) })
          ] })
        ]
      }
    ),
    T && /* @__PURE__ */ l.jsxs(
      un,
      {
        titulo: `Facturar el pedido ${a.numeroCompleto}`,
        alCerrar: () => F(!1),
        acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => F(!1), children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", onClick: async () => await ft(() => t.post(`/compras/pedidos/${a.id}/facturar`, { codigoIva: D, porcentajeIrpf: M, numeroFactura: P || null, fechaFactura: ce }), n.aviso, "Factura del proveedor registrada como gasto.") && (F(!1), o()), children: "Registrar factura" })
        ] }),
        children: [
          /* @__PURE__ */ l.jsxs("p", { className: "muted", style: { marginTop: 0 }, children: [
            "Registra la factura del proveedor por el total del pedido (",
            _(a.total),
            ") como gasto, con su asiento si la contabilidad es automática."
          ] }),
          /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Nº de factura del proveedor" }),
              /* @__PURE__ */ l.jsx("input", { value: P, onChange: (y) => Q(y.target.value), autoFocus: !0 })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Fecha de la factura" }),
              /* @__PURE__ */ l.jsx("input", { type: "date", value: ce, onChange: (y) => Le(y.target.value) })
            ] })
          ] }),
          /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Impuesto" }),
              /* @__PURE__ */ l.jsx("select", { value: D, onChange: (y) => C(y.target.value), children: u.map((y) => /* @__PURE__ */ l.jsx("option", { value: y.codigo, children: y.nombre }, y.codigo)) })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Retención IRPF %" }),
              /* @__PURE__ */ l.jsx("input", { type: "number", step: "0.01", value: M, onChange: (y) => b(Number(y.target.value)) })
            ] })
          ] })
        ]
      }
    )
  ] });
}
const Zl = (e = "") => ({ clave: Jr(), descripcion: "", cuentaGasto: "", base: 0, codigoIva: e, porcentajeIva: null, porcentajeDeducible: 100 }), Am = (e) => e === "InversionSujetoPasivo" || e === "Intracomunitario";
function Mm(e) {
  const { api: t, anfitrion: n } = Dt(), r = e.semilla, [a, i] = x.useState([]), [o, s] = x.useState([]), [c, d] = x.useState([]), [j, u] = x.useState([]), [m, h] = x.useState((r == null ? void 0 : r.proveedorId) ?? ""), [g, w] = x.useState(e.id ? (r == null ? void 0 : r.numeroFactura) ?? "" : ""), [$, p] = x.useState((r == null ? void 0 : r.fechaFactura) ?? Ct()), [f, v] = x.useState(e.id ? (r == null ? void 0 : r.fecha) ?? Ct() : Ct()), [k, T] = x.useState(r != null && r.concepto && !r.concepto.startsWith("Factura ") ? r.concepto : ""), [F, D] = x.useState((r == null ? void 0 : r.porcentajeIrpf) ?? 0), [C, M] = x.useState(""), [b, P] = x.useState(((r == null ? void 0 : r.recargoTotal) ?? 0) > 0), Q = !!(r != null && r.esRectificativa), [ce, Le] = x.useState((r == null ? void 0 : r.numeroRectificado) ?? ""), [Ve, pe] = x.useState((r == null ? void 0 : r.fechaRectificada) ?? ""), [ge, E] = x.useState((r == null ? void 0 : r.motivoRectificacion) ?? ""), [y, L] = x.useState(!1), [B, W] = x.useState((r == null ? void 0 : r.afectacion) ?? "Comun"), [q, Se] = x.useState((r == null ? void 0 : r.moneda) ?? ""), [je, me] = x.useState((r == null ? void 0 : r.tasaCambio) ?? null), Re = (N) => r != null && r.moneda && r.tasaCambio ? Oe(N / r.tasaCambio) : N, [ot, Be] = x.useState(
    () => {
      var N;
      return (N = r == null ? void 0 : r.lineas) != null && N.length ? r.lineas.map((H) => ({ clave: Jr(), descripcion: H.descripcion ?? "", cuentaGasto: H.cuentaGasto ?? "", base: Re(H.base), codigoIva: H.codigoIva, porcentajeIva: H.autoliquidada ? H.porcentajeIva : null, porcentajeDeducible: H.porcentajeDeducible })) : [Zl()];
    }
  ), [z, st] = x.useState(e.id && (r != null && r.vencimientos) && r.vencimientos.length > 1 ? r.vencimientos : null), [U, Et] = x.useState(null), [et, _e] = x.useState(""), [It, zt] = x.useState(!1), mn = ea();
  x.useEffect(() => {
    t.get("/proveedores").then(i).catch(() => i([])), t.get("/tipos-iva").then((N) => s(N.filter((H) => H.activo))).catch(() => s([])), t.get("/formas-pago").then((N) => d(N.filter((H) => H.activo))).catch(() => d([])), t.get("/empresas/actual").then((N) => {
      N.regimenIva === "RecargoEquivalencia" && (L(!0), r || P(!0));
    }).catch(() => {
    }), t.get("/contabilidad/cuentas").then((N) => u(N.filter((H) => H.codigo.startsWith("6") || H.codigo.startsWith("2")))).catch(() => u([]));
  }, [t]);
  const he = a.find((N) => N.id === m);
  x.useEffect(() => {
    he != null && he.formaPagoDefectoId && !C && M(he.formaPagoDefectoId);
  }, [he]);
  const Ie = x.useMemo(
    () => ({
      proveedorId: m || null,
      proveedorTexto: (he == null ? void 0 : he.nombre) ?? null,
      numeroFactura: g.trim() || null,
      fechaFactura: $ || null,
      fecha: f,
      concepto: k.trim() || null,
      porcentajeIrpf: F,
      formaPagoId: C || null,
      recargoEquivalencia: b,
      afectacion: B,
      baseImponible: 0,
      lineas: ot.filter((N) => N.base !== 0).map((N) => ({
        base: N.base,
        codigoIva: N.codigoIva || null,
        descripcion: N.descripcion.trim() || null,
        porcentajeIva: N.porcentajeIva,
        porcentajeDeducible: N.porcentajeDeducible,
        cuentaGasto: N.cuentaGasto.trim() || null
      })),
      vencimientos: z,
      rectificaGastoId: Q ? (r == null ? void 0 : r.rectificaGastoId) ?? null : null,
      numeroRectificado: Q && ce.trim() || null,
      fechaRectificada: Q && Ve || null,
      motivoRectificacion: Q && ge.trim() || null,
      moneda: q || null,
      tasaCambio: q ? je : null
    }),
    [m, he, g, $, f, k, F, C, b, B, ot, z, Q, r, ce, Ve, ge, q, je]
  ), Lt = Sn(Ie, 350);
  x.useEffect(() => {
    if (!Lt.lineas.length) {
      Et(null), _e("Añade al menos una línea con base.");
      return;
    }
    const N = mn();
    t.post("/gastos/simular", Lt).then((H) => N() && (Et(H), _e(""))).catch((H) => N() && (Et(null), _e(H.message)));
  }, [Lt, t]);
  const ct = (N, H) => Be((we) => we.map((K) => K.clave === N ? { ...K, ...H } : K)), R = (N) => o.find((H) => H.codigo === N), G = (N) => {
    var H;
    return (H = U == null ? void 0 : U.lineas) == null ? void 0 : H[ot.filter((we) => we.base !== 0).indexOf(N)];
  };
  function A(N) {
    if (!U) return;
    const H = /* @__PURE__ */ new Date(($ || f) + "T00:00:00"), we = Oe(U.total / N);
    st(Array.from({ length: N }, (K, te) => {
      const xt = new Date(H);
      return xt.setMonth(xt.getMonth() + te + 1), { fecha: xt.toISOString().slice(0, 10), importe: te === N - 1 ? Oe(U.total - we * (N - 1)) : we };
    }));
  }
  async function Y() {
    zt(!0);
    try {
      const N = e.id ? await t.put(`/gastos/${e.id}`, Ie) : await t.post("/gastos", Ie);
      n.aviso(e.id ? "Factura corregida." : "Factura registrada.", "ok"), N.avisoRiesgo && n.aviso(N.avisoRiesgo, "err"), e.alGuardar(N.id);
    } catch (N) {
      n.aviso(N.message, "err");
    } finally {
      zt(!1);
    }
  }
  const le = Oe((z ?? []).reduce((N, H) => N + (Number(H.importe) || 0), 0));
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsxs("div", { className: "panel-head", children: [
        /* @__PURE__ */ l.jsx("h2", { children: e.id ? `Corregir factura ${(r == null ? void 0 : r.numeroFactura) ?? ""}` : Q ? "Rectificativa / abono del proveedor" : "Nueva factura de proveedor" }),
        /* @__PURE__ */ l.jsxs("div", { style: { display: "flex", gap: 8 }, children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: e.alCancelar, children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", disabled: !U || It, onClick: Y, children: e.id ? "Guardar corrección" : Q ? "Registrar abono" : "Registrar factura" })
        ] })
      ] }),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-cabecera", children: [
        /* @__PURE__ */ l.jsxs("div", { className: "dx-cab-campos", children: [
          /* @__PURE__ */ l.jsx(Vo, { terceros: a, valor: m, alCambiar: h, etiqueta: "Proveedor" }),
          /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Nº de factura del proveedor" }),
              /* @__PURE__ */ l.jsx("input", { value: g, onChange: (N) => w(N.target.value), placeholder: "F-2026/0117" })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Fecha de la factura" }),
              /* @__PURE__ */ l.jsx("input", { type: "date", value: $, onChange: (N) => p(N.target.value) })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Fecha de registro" }),
              /* @__PURE__ */ l.jsx("input", { type: "date", value: f, onChange: (N) => v(N.target.value), title: "La del asiento y la del periodo de IVA en que se deduce" })
            ] })
          ] }),
          /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Forma de pago" }),
              /* @__PURE__ */ l.jsxs("select", { value: C, onChange: (N) => M(N.target.value), children: [
                /* @__PURE__ */ l.jsx("option", { value: "", children: "Contado (vence en la fecha de factura)" }),
                c.map((N) => /* @__PURE__ */ l.jsx("option", { value: N.id, children: N.nombre }, N.id))
              ] })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Retención IRPF %" }),
              /* @__PURE__ */ l.jsx("input", { type: "number", step: "0.01", value: F, onChange: (N) => D(Number(N.target.value)) })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Moneda de la factura" }),
              /* @__PURE__ */ l.jsxs("select", { value: q, onChange: (N) => Se(N.target.value), children: [
                /* @__PURE__ */ l.jsx("option", { value: "", children: "EUR · euros" }),
                Fd.map((N) => /* @__PURE__ */ l.jsx("option", { value: N, children: N }, N))
              ] })
            ] }),
            q && /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsxs("label", { children: [
                "Tipo de cambio (€ por 1 ",
                q,
                ")"
              ] }),
              /* @__PURE__ */ l.jsx("input", { type: "number", step: "0.000001", min: "0", value: je ?? "", placeholder: "El del día de la factura", onChange: (N) => me(N.target.value === "" ? null : Number(N.target.value)) })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Prorrata especial" }),
              /* @__PURE__ */ l.jsxs("select", { value: B, onChange: (N) => W(N.target.value), children: [
                /* @__PURE__ */ l.jsx("option", { value: "Comun", children: "Uso común" }),
                /* @__PURE__ */ l.jsx("option", { value: "ConDerecho", children: "Solo operaciones con derecho" }),
                /* @__PURE__ */ l.jsx("option", { value: "SinDerecho", children: "Solo operaciones exentas" })
              ] })
            ] })
          ] }),
          /* @__PURE__ */ l.jsx("div", { className: "dx-fila", children: /* @__PURE__ */ l.jsxs("div", { style: { gridColumn: "1 / -1" }, children: [
            /* @__PURE__ */ l.jsx("label", { children: "Concepto (vacío: «Factura nº»)" }),
            /* @__PURE__ */ l.jsx("input", { value: k, onChange: (N) => T(N.target.value) })
          ] }) }),
          Q && /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Factura que rectifica" }),
              /* @__PURE__ */ l.jsx("input", { value: ce, disabled: !!(r != null && r.rectificaGastoId), onChange: (N) => Le(N.target.value) })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Fecha de la rectificada" }),
              /* @__PURE__ */ l.jsx("input", { type: "date", value: Ve, disabled: !!(r != null && r.rectificaGastoId), onChange: (N) => pe(N.target.value) })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Motivo" }),
              /* @__PURE__ */ l.jsx("input", { value: ge, onChange: (N) => E(N.target.value), placeholder: "Devolución, descuento posterior, error de precio…" })
            ] })
          ] }),
          Q && /* @__PURE__ */ l.jsx("div", { className: "muted", children: "Las bases del abono van en negativo; el asiento y el SII (R1, por diferencias) salen con el signo." }),
          /* @__PURE__ */ l.jsxs("label", { className: "dx-check", children: [
            /* @__PURE__ */ l.jsx("input", { type: "checkbox", checked: b, onChange: (N) => P(N.target.checked) }),
            " El proveedor me cobra recargo de equivalencia"
          ] }),
          y && /* @__PURE__ */ l.jsx("div", { className: "muted", children: "La empresa está en recargo de equivalencia: el IVA y el recargo soportados son coste (no se deducen)." })
        ] }),
        /* @__PURE__ */ l.jsx("div", { className: "dx-ficha", children: he ? /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("strong", { children: he.nombre }),
          /* @__PURE__ */ l.jsx("div", { className: "muted", children: [he.nifFiscal, he.poblacion, he.pais].filter(Boolean).join(" · ") }),
          !he.nifFiscal && /* @__PURE__ */ l.jsx("div", { className: "dx-aviso", children: "⚠ Sin NIF en la ficha: hace falta para el libro de IVA, el 347 y el SII." }),
          (U == null ? void 0 : U.avisoRiesgo) && /* @__PURE__ */ l.jsxs("div", { className: "dx-aviso", children: [
            "⚠ ",
            U.avisoRiesgo
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
        /* @__PURE__ */ l.jsx("tbody", { children: ot.map((N, H) => {
          const we = R(N.codigoIva), K = G(N);
          return /* @__PURE__ */ l.jsxs("tr", { className: H % 2 ? "dx-par" : "", children: [
            /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx("input", { value: N.descripcion, onChange: (te) => ct(N.clave, { descripcion: te.target.value }), placeholder: "Qué se compra" }) }),
            /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx("input", { list: "dx-cuentas-gasto", value: N.cuentaGasto, onChange: (te) => ct(N.clave, { cuentaGasto: te.target.value }), placeholder: "la de la regla" }) }),
            /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx("input", { className: "num", type: "number", step: "0.01", value: N.base || "", onChange: (te) => ct(N.clave, { base: Number(te.target.value) }) }) }),
            /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsxs("select", { value: N.codigoIva, onChange: (te) => ct(N.clave, { codigoIva: te.target.value, porcentajeIva: null }), children: [
              /* @__PURE__ */ l.jsx("option", { value: "", children: "General" }),
              o.map((te) => /* @__PURE__ */ l.jsx("option", { value: te.codigo, children: te.nombre }, te.codigo))
            ] }) }),
            /* @__PURE__ */ l.jsx("td", { children: Am(we == null ? void 0 : we.clase) ? /* @__PURE__ */ l.jsx("input", { className: "num", type: "number", step: "0.01", value: N.porcentajeIva ?? "", placeholder: "21", title: "Tipo que autoliquidas", onChange: (te) => ct(N.clave, { porcentajeIva: te.target.value === "" ? null : Number(te.target.value) }) }) : /* @__PURE__ */ l.jsx("span", { className: "muted", children: K ? `${xe(K.porcentajeIva)} %` : "" }) }),
            /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx("input", { className: "num", type: "number", step: "1", min: 0, max: 100, value: N.porcentajeDeducible, onChange: (te) => ct(N.clave, { porcentajeDeducible: Number(te.target.value) }) }) }),
            /* @__PURE__ */ l.jsx("td", { className: "num", children: K ? /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
              /* @__PURE__ */ l.jsx("strong", { children: _(K.cuota) }),
              K.autoliquidada && /* @__PURE__ */ l.jsx("div", { className: "dx-sub", children: "autoliquidada" }),
              K.cuotaRecargo !== 0 && /* @__PURE__ */ l.jsxs("div", { className: "dx-sub", children: [
                "+ recargo ",
                _(K.cuotaRecargo)
              ] })
            ] }) : "—" }),
            /* @__PURE__ */ l.jsx("td", { className: "right", children: /* @__PURE__ */ l.jsx("button", { className: "dx-icono", title: "Quitar", onClick: () => Be((te) => te.length > 1 ? te.filter((xt) => xt.clave !== N.clave) : [Zl()]), children: "✕" }) })
          ] }, N.clave);
        }) })
      ] }),
      /* @__PURE__ */ l.jsx("datalist", { id: "dx-cuentas-gasto", children: j.map((N) => /* @__PURE__ */ l.jsx("option", { value: N.codigo, children: N.nombre }, N.codigo)) }),
      /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", style: { marginTop: 8 }, onClick: () => Be((N) => {
        var H;
        return [...N, Zl(((H = N[N.length - 1]) == null ? void 0 : H.codigoIva) ?? "")];
      }), children: "+ Añadir línea" }),
      /* @__PURE__ */ l.jsx("span", { className: "muted dx-ayuda", children: "Una línea por cada base e impuesto de la factura. En inversión del sujeto pasivo e intracomunitarias el IVA lo autoliquidas tú (no lo cobra el proveedor)." })
    ] }),
    /* @__PURE__ */ l.jsxs("div", { className: "dx-pie", children: [
      /* @__PURE__ */ l.jsxs("div", { className: "panel", style: { margin: 0 }, children: [
        /* @__PURE__ */ l.jsxs("div", { className: "panel-head", children: [
          /* @__PURE__ */ l.jsx("h2", { children: "Vencimientos" }),
          /* @__PURE__ */ l.jsxs("div", { className: "dx-acciones", children: [
            [2, 3, 4].map((N) => /* @__PURE__ */ l.jsxs("button", { className: "btn small secondary", disabled: !U, onClick: () => A(N), children: [
              N,
              " plazos"
            ] }, N)),
            z && /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", onClick: () => st(null), children: "Según forma de pago" })
          ] })
        ] }),
        z ? /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          z.map((N, H) => /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", style: { marginBottom: 6 }, children: [
            /* @__PURE__ */ l.jsx("input", { type: "date", value: N.fecha, onChange: (we) => st(z.map((K, te) => te === H ? { ...K, fecha: we.target.value } : K)) }),
            /* @__PURE__ */ l.jsx("input", { className: "num", type: "number", step: "0.01", value: N.importe, onChange: (we) => st(z.map((K, te) => te === H ? { ...K, importe: Number(we.target.value) } : K)) })
          ] }, H)),
          U && le !== U.total && /* @__PURE__ */ l.jsxs("p", { className: "dx-rojo", style: { margin: 0 }, children: [
            "Los plazos suman ",
            _(le),
            "; la factura, ",
            _(U.total),
            "."
          ] })
        ] }) : /* @__PURE__ */ l.jsx("p", { className: "muted", style: { margin: 0 }, children: ((U == null ? void 0 : U.vencimientos) ?? []).map((N) => `${ke(N.fecha)}: ${_(N.importe)}`).join(" · ") || "—" }),
        et && /* @__PURE__ */ l.jsx("p", { className: "dx-rojo", style: { marginBottom: 0 }, children: et })
      ] }),
      /* @__PURE__ */ l.jsxs("div", { className: "panel dx-totales", children: [
        ((U == null ? void 0 : U.desglose) ?? []).map((N, H) => {
          var we;
          return /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
            /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
              ((we = R(N.codigoIva)) == null ? void 0 : we.nombre) ?? N.codigoIva,
              " ",
              N.autoliquidada ? `(${xe(N.porcentajeIva)} %, autoliquidado)` : "",
              " · base ",
              xe(N.base)
            ] }),
            /* @__PURE__ */ l.jsx("span", { children: _(N.cuota) })
          ] }, H);
        }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Base imponible" }),
          /* @__PURE__ */ l.jsx("span", { children: _(U == null ? void 0 : U.baseImponible) })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Impuestos" }),
          /* @__PURE__ */ l.jsx("span", { children: _(U == null ? void 0 : U.cuotaIva) })
        ] }),
        !!(U != null && U.recargoTotal) && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Recargo de equivalencia" }),
          /* @__PURE__ */ l.jsx("span", { children: _(U.recargoTotal) })
        ] }),
        !!(U != null && U.retencionIrpf) && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Retención IRPF" }),
          /* @__PURE__ */ l.jsxs("span", { children: [
            "−",
            _(U.retencionIrpf)
          ] })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ l.jsx("span", { children: ((U == null ? void 0 : U.total) ?? 0) < 0 ? "A favor (abono del proveedor)" : "Total a pagar" }),
          /* @__PURE__ */ l.jsx("span", { children: _(U == null ? void 0 : U.total) })
        ] }),
        (U == null ? void 0 : U.moneda) && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ l.jsxs("span", { children: [
            "Total en ",
            U.moneda,
            " (1 ",
            U.moneda,
            " = ",
            String(U.tasaCambio ?? 0).replace(".", ","),
            " €)"
          ] }),
          /* @__PURE__ */ l.jsxs("span", { children: [
            xe(U.totalDivisa ?? 0),
            " ",
            U.moneda
          ] })
        ] }),
        U && (U.desglose ?? []).some((N) => N.cuotaDeducible !== N.cuota) && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "IVA deducible (antes de prorrata)" }),
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: _((U.desglose ?? []).reduce((N, H) => N + H.cuotaDeducible, 0)) })
        ] })
      ] })
    ] })
  ] });
}
function $m(e) {
  const { api: t, anfitrion: n, navegar: r } = Dt(), [a, i] = x.useState(null), [o, s] = x.useState(null), [c, d] = x.useState(""), [j, u] = x.useState(!1), m = () => {
    t.get(`/gastos/${e.id}`).then(i).catch((w) => d(w.message)), t.get(`/gastos/${e.id}/saldo`).then(s).catch(() => s(null));
  };
  if (x.useEffect(m, [e.id]), c) return /* @__PURE__ */ l.jsx("div", { className: "panel", children: /* @__PURE__ */ l.jsx("p", { className: "dx-rojo", children: c }) });
  if (!a) return /* @__PURE__ */ l.jsx("div", { className: "muted", children: "Cargando…" });
  const h = a.estado === "Registrado", g = !o || o.liquidado === 0;
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsxs("div", { className: "panel-head", children: [
        /* @__PURE__ */ l.jsxs("h2", { style: { display: "flex", alignItems: "center", gap: 10 }, children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "gasto", pantalla: "lista" }), children: "←" }),
          "Factura ",
          /* @__PURE__ */ l.jsx("span", { className: "mono", children: a.numeroFactura ?? "(sin número)" }),
          " ",
          /* @__PURE__ */ l.jsx("span", { className: Uo(a.estado === "Anulado" ? "Anulada" : "Emitida"), children: a.estado }),
          a.esRectificativa && /* @__PURE__ */ l.jsxs("span", { className: "pill", children: [
            "Rectifica ",
            a.numeroRectificado
          ] })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-acciones", children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "gasto", pantalla: "editor", semilla: { ...a, numeroFactura: null } }), children: "Duplicar" }),
          h && g && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "gasto", pantalla: "editor", id: a.id, semilla: a }), children: "Corregir" }),
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
              lineas: (w = a.lineas) == null ? void 0 : w.map(($) => ({ ...$, base: -$.base }))
            } });
          }, children: "Rectificativa / abono" }),
          h && g && /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", onClick: () => u(!0), children: "Anular" })
        ] })
      ] }),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ l.jsxs("div", { className: "dx-dato", children: [
          /* @__PURE__ */ l.jsx("small", { children: "Proveedor" }),
          /* @__PURE__ */ l.jsx("div", { children: /* @__PURE__ */ l.jsx("strong", { children: a.proveedorTexto }) })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-dato", children: [
          /* @__PURE__ */ l.jsx("small", { children: "Fecha de factura" }),
          /* @__PURE__ */ l.jsx("div", { children: ke(a.fechaFactura ?? a.fecha) })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-dato", children: [
          /* @__PURE__ */ l.jsx("small", { children: "Registro" }),
          /* @__PURE__ */ l.jsx("div", { children: ke(a.fecha) })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-dato", children: [
          /* @__PURE__ */ l.jsx("small", { children: "Pago" }),
          /* @__PURE__ */ l.jsx("div", { children: o ? o.pendiente <= 0 ? /* @__PURE__ */ l.jsx("span", { className: "pill ok", children: "Pagada" }) : /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            "Pendiente ",
            /* @__PURE__ */ l.jsx("strong", { children: _(o.pendiente) })
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
        /* @__PURE__ */ l.jsx("tbody", { children: (a.lineas ?? []).map((w, $) => /* @__PURE__ */ l.jsxs("tr", { children: [
          /* @__PURE__ */ l.jsx("td", { children: w.descripcion ?? "" }),
          /* @__PURE__ */ l.jsx("td", { className: "mono muted", children: w.cuentaGasto ?? "regla" }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: _(w.base) }),
          /* @__PURE__ */ l.jsxs("td", { children: [
            w.codigoIva,
            " · ",
            xe(w.porcentajeIva),
            " %",
            w.autoliquidada ? " · autoliquidada" : "",
            w.cuotaRecargo ? ` · recargo ${_(w.cuotaRecargo)}` : ""
          ] }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: _(w.cuota) }),
          /* @__PURE__ */ l.jsxs("td", { className: "num muted", children: [
            w.porcentajeDeducible !== 100 ? `${xe(w.porcentajeDeducible)} % · ` : "",
            _(w.cuotaDeducible)
          ] })
        ] }, $)) })
      ] }),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-totales-vista", children: [
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Base imponible" }),
          /* @__PURE__ */ l.jsx("span", { children: _(a.baseImponible) })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Impuestos" }),
          /* @__PURE__ */ l.jsx("span", { children: _(a.cuotaIva) })
        ] }),
        !!a.recargoTotal && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Recargo de equivalencia" }),
          /* @__PURE__ */ l.jsx("span", { children: _(a.recargoTotal) })
        ] }),
        !!a.retencionIrpf && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
            "Retención IRPF (",
            xe(a.porcentajeIrpf),
            " %)"
          ] }),
          /* @__PURE__ */ l.jsxs("span", { children: [
            "−",
            _(a.retencionIrpf)
          ] })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ l.jsx("span", { children: a.total < 0 ? "A favor (abono del proveedor)" : "Total a pagar" }),
          /* @__PURE__ */ l.jsx("span", { children: _(a.total) })
        ] })
      ] }),
      /* @__PURE__ */ l.jsxs("p", { className: "muted", style: { fontSize: 12.5 }, children: [
        "Vencimientos: ",
        (a.vencimientos ?? []).map((w) => `${ke(w.fecha)} ${_(w.importe)}`).join(" · ")
      ] })
    ] }),
    j && /* @__PURE__ */ l.jsx(
      un,
      {
        titulo: "Anular la factura",
        alCerrar: () => u(!1),
        acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => u(!1), children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", onClick: async () => {
            try {
              await t.post(`/gastos/${a.id}/anular`), n.aviso("Factura anulada.", "ok"), u(!1), m();
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
function Om(e) {
  const [t, n] = x.useState(e.inicial), r = x.useRef(0), [a, i] = x.useState(0), o = x.useMemo(() => mm(e.anfitrion), [e.anfitrion]), s = (u) => {
    n(u), i(++r.current), window.scrollTo({ top: 0 });
  }, c = { api: o, anfitrion: e.anfitrion, ruta: t, navegar: s }, d = `${t.tipo}-${t.pantalla}-${t.id ?? ""}-${a}`;
  let j;
  if (t.pantalla === "lista") j = /* @__PURE__ */ l.jsx(km, { tipo: t.tipo });
  else if (t.pantalla === "vista" && t.id)
    j = t.tipo === "factura" ? /* @__PURE__ */ l.jsx(Tm, { id: t.id }) : t.tipo === "presupuesto" ? /* @__PURE__ */ l.jsx(Dm, { id: t.id }) : t.tipo === "pedido" ? /* @__PURE__ */ l.jsx(zm, { id: t.id }) : t.tipo === "gasto" ? /* @__PURE__ */ l.jsx($m, { id: t.id }) : /* @__PURE__ */ l.jsx(Lm, { id: t.id });
  else if (t.tipo === "gasto")
    j = /* @__PURE__ */ l.jsx(
      Mm,
      {
        id: t.id,
        semilla: t.semilla,
        alGuardar: (u) => s({ tipo: "gasto", pantalla: "vista", id: u }),
        alCancelar: () => s(t.id ? { tipo: "gasto", pantalla: "vista", id: t.id } : { tipo: "gasto", pantalla: "lista" })
      }
    );
  else if (t.tipo === "compra")
    j = /* @__PURE__ */ l.jsx(
      _m,
      {
        id: t.id,
        semilla: t.semilla,
        alGuardar: (u) => s({ tipo: "compra", pantalla: "vista", id: u }),
        alCancelar: () => s(t.id ? { tipo: "compra", pantalla: "vista", id: t.id } : { tipo: "compra", pantalla: "lista" })
      }
    );
  else {
    const u = t.tipo;
    j = /* @__PURE__ */ l.jsx(
      Rm,
      {
        tipo: u,
        id: t.id,
        semilla: t.semilla,
        alGuardar: (m) => s({ tipo: u, pantalla: "vista", id: m }),
        alCancelar: () => s(t.id ? { tipo: u, pantalla: "vista", id: t.id } : { tipo: u, pantalla: "lista" })
      }
    );
  }
  return /* @__PURE__ */ l.jsx(Cd.Provider, { value: c, children: /* @__PURE__ */ l.jsx("div", { className: "dx-raiz", children: j }, d) });
}
const bm = `
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
function Um() {
  if (document.getElementById("dx-estilos")) return;
  const e = document.createElement("style");
  e.id = "dx-estilos", e.textContent = bm, document.head.appendChild(e);
}
function Vm(e, t, n) {
  Um();
  const r = wd(e);
  return r.render(/* @__PURE__ */ l.jsx(Om, { anfitrion: t, inicial: n })), () => r.unmount();
}
export {
  Vm as montar
};
