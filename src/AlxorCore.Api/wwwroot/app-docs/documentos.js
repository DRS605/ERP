var mc = { exports: {} }, nl = {}, hc = { exports: {} }, X = {};
/**
 * @license React
 * react.production.min.js
 *
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */
var Yr = Symbol.for("react.element"), Vd = Symbol.for("react.portal"), Bd = Symbol.for("react.fragment"), Hd = Symbol.for("react.strict_mode"), Wd = Symbol.for("react.profiler"), Qd = Symbol.for("react.provider"), Gd = Symbol.for("react.context"), Kd = Symbol.for("react.forward_ref"), qd = Symbol.for("react.suspense"), Yd = Symbol.for("react.memo"), Xd = Symbol.for("react.lazy"), ns = Symbol.iterator;
function Zd(e) {
  return e === null || typeof e != "object" ? null : (e = ns && e[ns] || e["@@iterator"], typeof e == "function" ? e : null);
}
var vc = { isMounted: function() {
  return !1;
}, enqueueForceUpdate: function() {
}, enqueueReplaceState: function() {
}, enqueueSetState: function() {
} }, xc = Object.assign, gc = {};
function ir(e, t, n) {
  this.props = e, this.context = t, this.refs = gc, this.updater = n || vc;
}
ir.prototype.isReactComponent = {};
ir.prototype.setState = function(e, t) {
  if (typeof e != "object" && typeof e != "function" && e != null) throw Error("setState(...): takes an object of state variables to update or a function which returns an object of state variables.");
  this.updater.enqueueSetState(this, e, t, "setState");
};
ir.prototype.forceUpdate = function(e) {
  this.updater.enqueueForceUpdate(this, e, "forceUpdate");
};
function yc() {
}
yc.prototype = ir.prototype;
function Gi(e, t, n) {
  this.props = e, this.context = t, this.refs = gc, this.updater = n || vc;
}
var Ki = Gi.prototype = new yc();
Ki.constructor = Gi;
xc(Ki, ir.prototype);
Ki.isPureReactComponent = !0;
var rs = Array.isArray, jc = Object.prototype.hasOwnProperty, qi = { current: null }, Nc = { key: !0, ref: !0, __self: !0, __source: !0 };
function Sc(e, t, n) {
  var r, a = {}, i = null, o = null;
  if (t != null) for (r in t.ref !== void 0 && (o = t.ref), t.key !== void 0 && (i = "" + t.key), t) jc.call(t, r) && !Nc.hasOwnProperty(r) && (a[r] = t[r]);
  var s = arguments.length - 2;
  if (s === 1) a.children = n;
  else if (1 < s) {
    for (var c = Array(s), d = 0; d < s; d++) c[d] = arguments[d + 2];
    a.children = c;
  }
  if (e && e.defaultProps) for (r in s = e.defaultProps, s) a[r] === void 0 && (a[r] = s[r]);
  return { $$typeof: Yr, type: e, key: i, ref: o, props: a, _owner: qi.current };
}
function Jd(e, t) {
  return { $$typeof: Yr, type: e.type, key: t, ref: e.ref, props: e.props, _owner: e._owner };
}
function Yi(e) {
  return typeof e == "object" && e !== null && e.$$typeof === Yr;
}
function ef(e) {
  var t = { "=": "=0", ":": "=2" };
  return "$" + e.replace(/[=:]/g, function(n) {
    return t[n];
  });
}
var as = /\/+/g;
function kl(e, t) {
  return typeof e == "object" && e !== null && e.key != null ? ef("" + e.key) : t.toString(36);
}
function ja(e, t, n, r, a) {
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
        case Yr:
        case Vd:
          o = !0;
      }
  }
  if (o) return o = e, a = a(o), e = r === "" ? "." + kl(o, 0) : r, rs(a) ? (n = "", e != null && (n = e.replace(as, "$&/") + "/"), ja(a, t, n, "", function(d) {
    return d;
  })) : a != null && (Yi(a) && (a = Jd(a, n + (!a.key || o && o.key === a.key ? "" : ("" + a.key).replace(as, "$&/") + "/") + e)), t.push(a)), 1;
  if (o = 0, r = r === "" ? "." : r + ":", rs(e)) for (var s = 0; s < e.length; s++) {
    i = e[s];
    var c = r + kl(i, s);
    o += ja(i, t, n, c, a);
  }
  else if (c = Zd(e), typeof c == "function") for (e = c.call(e), s = 0; !(i = e.next()).done; ) i = i.value, c = r + kl(i, s++), o += ja(i, t, n, c, a);
  else if (i === "object") throw t = String(e), Error("Objects are not valid as a React child (found: " + (t === "[object Object]" ? "object with keys {" + Object.keys(e).join(", ") + "}" : t) + "). If you meant to render a collection of children, use an array instead.");
  return o;
}
function ra(e, t, n) {
  if (e == null) return e;
  var r = [], a = 0;
  return ja(e, r, "", "", function(i) {
    return t.call(n, i, a++);
  }), r;
}
function tf(e) {
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
var Ge = { current: null }, Na = { transition: null }, nf = { ReactCurrentDispatcher: Ge, ReactCurrentBatchConfig: Na, ReactCurrentOwner: qi };
function wc() {
  throw Error("act(...) is not supported in production builds of React.");
}
X.Children = { map: ra, forEach: function(e, t, n) {
  ra(e, function() {
    t.apply(this, arguments);
  }, n);
}, count: function(e) {
  var t = 0;
  return ra(e, function() {
    t++;
  }), t;
}, toArray: function(e) {
  return ra(e, function(t) {
    return t;
  }) || [];
}, only: function(e) {
  if (!Yi(e)) throw Error("React.Children.only expected to receive a single React element child.");
  return e;
} };
X.Component = ir;
X.Fragment = Bd;
X.Profiler = Wd;
X.PureComponent = Gi;
X.StrictMode = Hd;
X.Suspense = qd;
X.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED = nf;
X.act = wc;
X.cloneElement = function(e, t, n) {
  if (e == null) throw Error("React.cloneElement(...): The argument must be a React element, but you passed " + e + ".");
  var r = xc({}, e.props), a = e.key, i = e.ref, o = e._owner;
  if (t != null) {
    if (t.ref !== void 0 && (i = t.ref, o = qi.current), t.key !== void 0 && (a = "" + t.key), e.type && e.type.defaultProps) var s = e.type.defaultProps;
    for (c in t) jc.call(t, c) && !Nc.hasOwnProperty(c) && (r[c] = t[c] === void 0 && s !== void 0 ? s[c] : t[c]);
  }
  var c = arguments.length - 2;
  if (c === 1) r.children = n;
  else if (1 < c) {
    s = Array(c);
    for (var d = 0; d < c; d++) s[d] = arguments[d + 2];
    r.children = s;
  }
  return { $$typeof: Yr, type: e.type, key: a, ref: i, props: r, _owner: o };
};
X.createContext = function(e) {
  return e = { $$typeof: Gd, _currentValue: e, _currentValue2: e, _threadCount: 0, Provider: null, Consumer: null, _defaultValue: null, _globalName: null }, e.Provider = { $$typeof: Qd, _context: e }, e.Consumer = e;
};
X.createElement = Sc;
X.createFactory = function(e) {
  var t = Sc.bind(null, e);
  return t.type = e, t;
};
X.createRef = function() {
  return { current: null };
};
X.forwardRef = function(e) {
  return { $$typeof: Kd, render: e };
};
X.isValidElement = Yi;
X.lazy = function(e) {
  return { $$typeof: Xd, _payload: { _status: -1, _result: e }, _init: tf };
};
X.memo = function(e, t) {
  return { $$typeof: Yd, type: e, compare: t === void 0 ? null : t };
};
X.startTransition = function(e) {
  var t = Na.transition;
  Na.transition = {};
  try {
    e();
  } finally {
    Na.transition = t;
  }
};
X.unstable_act = wc;
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
hc.exports = X;
var x = hc.exports;
/**
 * @license React
 * react-jsx-runtime.production.min.js
 *
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */
var rf = x, af = Symbol.for("react.element"), lf = Symbol.for("react.fragment"), of = Object.prototype.hasOwnProperty, sf = rf.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED.ReactCurrentOwner, cf = { key: !0, ref: !0, __self: !0, __source: !0 };
function Cc(e, t, n) {
  var r, a = {}, i = null, o = null;
  n !== void 0 && (i = "" + n), t.key !== void 0 && (i = "" + t.key), t.ref !== void 0 && (o = t.ref);
  for (r in t) of.call(t, r) && !cf.hasOwnProperty(r) && (a[r] = t[r]);
  if (e && e.defaultProps) for (r in t = e.defaultProps, t) a[r] === void 0 && (a[r] = t[r]);
  return { $$typeof: af, type: e, key: i, ref: o, props: a, _owner: sf.current };
}
nl.Fragment = lf;
nl.jsx = Cc;
nl.jsxs = Cc;
mc.exports = nl;
var l = mc.exports, kc = { exports: {} }, lt = {}, Ec = { exports: {} }, Ic = {};
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
        var Se = 2 * (B + 1) - 1, je = E[Se], he = Se + 1, _e = E[he];
        if (0 > a(je, L)) he < W && 0 > a(_e, je) ? (E[B] = _e, E[he] = L, B = he) : (E[B] = je, E[Se] = L, B = Se);
        else if (he < W && 0 > a(_e, L)) E[B] = _e, E[he] = L, B = he;
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
    if (w = !1, v(E), !g) if (n(c) !== null) g = !0, me(_);
    else {
      var y = n(d);
      y !== null && ge(k, y.startTime - E);
    }
  }
  function _(E, y) {
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
  var F = !1, T = null, C = -1, M = 5, b = -1;
  function P() {
    return !(e.unstable_now() - b < M);
  }
  function Q() {
    if (T !== null) {
      var E = e.unstable_now();
      b = E;
      var y = !0;
      try {
        y = T(!0, E);
      } finally {
        y ? ce() : (F = !1, T = null);
      }
    } else F = !1;
  }
  var ce;
  if (typeof f == "function") ce = function() {
    f(Q);
  };
  else if (typeof MessageChannel < "u") {
    var Ae = new MessageChannel(), Be = Ae.port2;
    Ae.port1.onmessage = Q, ce = function() {
      Be.postMessage(null);
    };
  } else ce = function() {
    $(Q, 0);
  };
  function me(E) {
    T = E, F || (F = !0, ce());
  }
  function ge(E, y) {
    C = $(function() {
      E(e.unstable_now());
    }, y);
  }
  e.unstable_IdlePriority = 5, e.unstable_ImmediatePriority = 1, e.unstable_LowPriority = 4, e.unstable_NormalPriority = 3, e.unstable_Profiling = null, e.unstable_UserBlockingPriority = 2, e.unstable_cancelCallback = function(E) {
    E.callback = null;
  }, e.unstable_continueExecution = function() {
    g || h || (g = !0, me(_));
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
    return W = L + W, E = { id: j++, callback: y, priorityLevel: E, startTime: L, expirationTime: W, sortIndex: -1 }, L > B ? (E.sortIndex = L, t(d, E), n(c) === null && E === n(d) && (w ? (p(C), C = -1) : w = !0, ge(k, L - B))) : (E.sortIndex = W, t(c, E), g || h || (g = !0, me(_))), E;
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
})(Ic);
Ec.exports = Ic;
var uf = Ec.exports;
/**
 * @license React
 * react-dom.production.min.js
 *
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */
var df = x, at = uf;
function I(e) {
  for (var t = "https://reactjs.org/docs/error-decoder.html?invariant=" + e, n = 1; n < arguments.length; n++) t += "&args[]=" + encodeURIComponent(arguments[n]);
  return "Minified React error #" + e + "; visit " + t + " for the full message or use the non-minified dev environment for full errors and additional helpful warnings.";
}
var Pc = /* @__PURE__ */ new Set(), Tr = {};
function Rn(e, t) {
  Jn(e, t), Jn(e + "Capture", t);
}
function Jn(e, t) {
  for (Tr[e] = t, e = 0; e < t.length; e++) Pc.add(t[e]);
}
var Ut = !(typeof window > "u" || typeof window.document > "u" || typeof window.document.createElement > "u"), ei = Object.prototype.hasOwnProperty, ff = /^[:A-Z_a-z\u00C0-\u00D6\u00D8-\u00F6\u00F8-\u02FF\u0370-\u037D\u037F-\u1FFF\u200C-\u200D\u2070-\u218F\u2C00-\u2FEF\u3001-\uD7FF\uF900-\uFDCF\uFDF0-\uFFFD][:A-Z_a-z\u00C0-\u00D6\u00D8-\u00F6\u00F8-\u02FF\u0370-\u037D\u037F-\u1FFF\u200C-\u200D\u2070-\u218F\u2C00-\u2FEF\u3001-\uD7FF\uF900-\uFDCF\uFDF0-\uFFFD\-.0-9\u00B7\u0300-\u036F\u203F-\u2040]*$/, ls = {}, is = {};
function pf(e) {
  return ei.call(is, e) ? !0 : ei.call(ls, e) ? !1 : ff.test(e) ? is[e] = !0 : (ls[e] = !0, !1);
}
function mf(e, t, n, r) {
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
function hf(e, t, n, r) {
  if (t === null || typeof t > "u" || mf(e, t, n, r)) return !0;
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
var Le = {};
"children dangerouslySetInnerHTML defaultValue defaultChecked innerHTML suppressContentEditableWarning suppressHydrationWarning style".split(" ").forEach(function(e) {
  Le[e] = new Ke(e, 0, !1, e, null, !1, !1);
});
[["acceptCharset", "accept-charset"], ["className", "class"], ["htmlFor", "for"], ["httpEquiv", "http-equiv"]].forEach(function(e) {
  var t = e[0];
  Le[t] = new Ke(t, 1, !1, e[1], null, !1, !1);
});
["contentEditable", "draggable", "spellCheck", "value"].forEach(function(e) {
  Le[e] = new Ke(e, 2, !1, e.toLowerCase(), null, !1, !1);
});
["autoReverse", "externalResourcesRequired", "focusable", "preserveAlpha"].forEach(function(e) {
  Le[e] = new Ke(e, 2, !1, e, null, !1, !1);
});
"allowFullScreen async autoFocus autoPlay controls default defer disabled disablePictureInPicture disableRemotePlayback formNoValidate hidden loop noModule noValidate open playsInline readOnly required reversed scoped seamless itemScope".split(" ").forEach(function(e) {
  Le[e] = new Ke(e, 3, !1, e.toLowerCase(), null, !1, !1);
});
["checked", "multiple", "muted", "selected"].forEach(function(e) {
  Le[e] = new Ke(e, 3, !0, e, null, !1, !1);
});
["capture", "download"].forEach(function(e) {
  Le[e] = new Ke(e, 4, !1, e, null, !1, !1);
});
["cols", "rows", "size", "span"].forEach(function(e) {
  Le[e] = new Ke(e, 6, !1, e, null, !1, !1);
});
["rowSpan", "start"].forEach(function(e) {
  Le[e] = new Ke(e, 5, !1, e.toLowerCase(), null, !1, !1);
});
var Xi = /[\-:]([a-z])/g;
function Zi(e) {
  return e[1].toUpperCase();
}
"accent-height alignment-baseline arabic-form baseline-shift cap-height clip-path clip-rule color-interpolation color-interpolation-filters color-profile color-rendering dominant-baseline enable-background fill-opacity fill-rule flood-color flood-opacity font-family font-size font-size-adjust font-stretch font-style font-variant font-weight glyph-name glyph-orientation-horizontal glyph-orientation-vertical horiz-adv-x horiz-origin-x image-rendering letter-spacing lighting-color marker-end marker-mid marker-start overline-position overline-thickness paint-order panose-1 pointer-events rendering-intent shape-rendering stop-color stop-opacity strikethrough-position strikethrough-thickness stroke-dasharray stroke-dashoffset stroke-linecap stroke-linejoin stroke-miterlimit stroke-opacity stroke-width text-anchor text-decoration text-rendering underline-position underline-thickness unicode-bidi unicode-range units-per-em v-alphabetic v-hanging v-ideographic v-mathematical vector-effect vert-adv-y vert-origin-x vert-origin-y word-spacing writing-mode xmlns:xlink x-height".split(" ").forEach(function(e) {
  var t = e.replace(
    Xi,
    Zi
  );
  Le[t] = new Ke(t, 1, !1, e, null, !1, !1);
});
"xlink:actuate xlink:arcrole xlink:role xlink:show xlink:title xlink:type".split(" ").forEach(function(e) {
  var t = e.replace(Xi, Zi);
  Le[t] = new Ke(t, 1, !1, e, "http://www.w3.org/1999/xlink", !1, !1);
});
["xml:base", "xml:lang", "xml:space"].forEach(function(e) {
  var t = e.replace(Xi, Zi);
  Le[t] = new Ke(t, 1, !1, e, "http://www.w3.org/XML/1998/namespace", !1, !1);
});
["tabIndex", "crossOrigin"].forEach(function(e) {
  Le[e] = new Ke(e, 1, !1, e.toLowerCase(), null, !1, !1);
});
Le.xlinkHref = new Ke("xlinkHref", 1, !1, "xlink:href", "http://www.w3.org/1999/xlink", !0, !1);
["src", "href", "action", "formAction"].forEach(function(e) {
  Le[e] = new Ke(e, 1, !1, e.toLowerCase(), null, !0, !0);
});
function Ji(e, t, n, r) {
  var a = Le.hasOwnProperty(t) ? Le[t] : null;
  (a !== null ? a.type !== 0 : r || !(2 < t.length) || t[0] !== "o" && t[0] !== "O" || t[1] !== "n" && t[1] !== "N") && (hf(t, n, a, r) && (n = null), r || a === null ? pf(t) && (n === null ? e.removeAttribute(t) : e.setAttribute(t, "" + n)) : a.mustUseProperty ? e[a.propertyName] = n === null ? a.type === 3 ? !1 : "" : n : (t = a.attributeName, r = a.attributeNamespace, n === null ? e.removeAttribute(t) : (a = a.type, n = a === 3 || a === 4 && n === !0 ? "" : "" + n, r ? e.setAttributeNS(r, t, n) : e.setAttribute(t, n))));
}
var Wt = df.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED, aa = Symbol.for("react.element"), zn = Symbol.for("react.portal"), Ln = Symbol.for("react.fragment"), eo = Symbol.for("react.strict_mode"), ti = Symbol.for("react.profiler"), Fc = Symbol.for("react.provider"), Rc = Symbol.for("react.context"), to = Symbol.for("react.forward_ref"), ni = Symbol.for("react.suspense"), ri = Symbol.for("react.suspense_list"), no = Symbol.for("react.memo"), Kt = Symbol.for("react.lazy"), _c = Symbol.for("react.offscreen"), os = Symbol.iterator;
function ur(e) {
  return e === null || typeof e != "object" ? null : (e = os && e[os] || e["@@iterator"], typeof e == "function" ? e : null);
}
var pe = Object.assign, El;
function gr(e) {
  if (El === void 0) try {
    throw Error();
  } catch (n) {
    var t = n.stack.trim().match(/\n( *(at )?)/);
    El = t && t[1] || "";
  }
  return `
` + El + e;
}
var Il = !1;
function Pl(e, t) {
  if (!e || Il) return "";
  Il = !0;
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
    Il = !1, Error.prepareStackTrace = n;
  }
  return (e = e ? e.displayName || e.name : "") ? gr(e) : "";
}
function vf(e) {
  switch (e.tag) {
    case 5:
      return gr(e.type);
    case 16:
      return gr("Lazy");
    case 13:
      return gr("Suspense");
    case 19:
      return gr("SuspenseList");
    case 0:
    case 2:
    case 15:
      return e = Pl(e.type, !1), e;
    case 11:
      return e = Pl(e.type.render, !1), e;
    case 1:
      return e = Pl(e.type, !0), e;
    default:
      return "";
  }
}
function ai(e) {
  if (e == null) return null;
  if (typeof e == "function") return e.displayName || e.name || null;
  if (typeof e == "string") return e;
  switch (e) {
    case Ln:
      return "Fragment";
    case zn:
      return "Portal";
    case ti:
      return "Profiler";
    case eo:
      return "StrictMode";
    case ni:
      return "Suspense";
    case ri:
      return "SuspenseList";
  }
  if (typeof e == "object") switch (e.$$typeof) {
    case Rc:
      return (e.displayName || "Context") + ".Consumer";
    case Fc:
      return (e._context.displayName || "Context") + ".Provider";
    case to:
      var t = e.render;
      return e = e.displayName, e || (e = t.displayName || t.name || "", e = e !== "" ? "ForwardRef(" + e + ")" : "ForwardRef"), e;
    case no:
      return t = e.displayName || null, t !== null ? t : ai(e.type) || "Memo";
    case Kt:
      t = e._payload, e = e._init;
      try {
        return ai(e(t));
      } catch {
      }
  }
  return null;
}
function xf(e) {
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
      return ai(t);
    case 8:
      return t === eo ? "StrictMode" : "Mode";
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
function un(e) {
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
function Tc(e) {
  var t = e.type;
  return (e = e.nodeName) && e.toLowerCase() === "input" && (t === "checkbox" || t === "radio");
}
function gf(e) {
  var t = Tc(e) ? "checked" : "value", n = Object.getOwnPropertyDescriptor(e.constructor.prototype, t), r = "" + e[t];
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
function la(e) {
  e._valueTracker || (e._valueTracker = gf(e));
}
function Dc(e) {
  if (!e) return !1;
  var t = e._valueTracker;
  if (!t) return !0;
  var n = t.getValue(), r = "";
  return e && (r = Tc(e) ? e.checked ? "true" : "false" : e.value), e = r, e !== n ? (t.setValue(e), !0) : !1;
}
function Ta(e) {
  if (e = e || (typeof document < "u" ? document : void 0), typeof e > "u") return null;
  try {
    return e.activeElement || e.body;
  } catch {
    return e.body;
  }
}
function li(e, t) {
  var n = t.checked;
  return pe({}, t, { defaultChecked: void 0, defaultValue: void 0, value: void 0, checked: n ?? e._wrapperState.initialChecked });
}
function ss(e, t) {
  var n = t.defaultValue == null ? "" : t.defaultValue, r = t.checked != null ? t.checked : t.defaultChecked;
  n = un(t.value != null ? t.value : n), e._wrapperState = { initialChecked: r, initialValue: n, controlled: t.type === "checkbox" || t.type === "radio" ? t.checked != null : t.value != null };
}
function zc(e, t) {
  t = t.checked, t != null && Ji(e, "checked", t, !1);
}
function ii(e, t) {
  zc(e, t);
  var n = un(t.value), r = t.type;
  if (n != null) r === "number" ? (n === 0 && e.value === "" || e.value != n) && (e.value = "" + n) : e.value !== "" + n && (e.value = "" + n);
  else if (r === "submit" || r === "reset") {
    e.removeAttribute("value");
    return;
  }
  t.hasOwnProperty("value") ? oi(e, t.type, n) : t.hasOwnProperty("defaultValue") && oi(e, t.type, un(t.defaultValue)), t.checked == null && t.defaultChecked != null && (e.defaultChecked = !!t.defaultChecked);
}
function cs(e, t, n) {
  if (t.hasOwnProperty("value") || t.hasOwnProperty("defaultValue")) {
    var r = t.type;
    if (!(r !== "submit" && r !== "reset" || t.value !== void 0 && t.value !== null)) return;
    t = "" + e._wrapperState.initialValue, n || t === e.value || (e.value = t), e.defaultValue = t;
  }
  n = e.name, n !== "" && (e.name = ""), e.defaultChecked = !!e._wrapperState.initialChecked, n !== "" && (e.name = n);
}
function oi(e, t, n) {
  (t !== "number" || Ta(e.ownerDocument) !== e) && (n == null ? e.defaultValue = "" + e._wrapperState.initialValue : e.defaultValue !== "" + n && (e.defaultValue = "" + n));
}
var yr = Array.isArray;
function Qn(e, t, n, r) {
  if (e = e.options, t) {
    t = {};
    for (var a = 0; a < n.length; a++) t["$" + n[a]] = !0;
    for (n = 0; n < e.length; n++) a = t.hasOwnProperty("$" + e[n].value), e[n].selected !== a && (e[n].selected = a), a && r && (e[n].defaultSelected = !0);
  } else {
    for (n = "" + un(n), t = null, a = 0; a < e.length; a++) {
      if (e[a].value === n) {
        e[a].selected = !0, r && (e[a].defaultSelected = !0);
        return;
      }
      t !== null || e[a].disabled || (t = e[a]);
    }
    t !== null && (t.selected = !0);
  }
}
function si(e, t) {
  if (t.dangerouslySetInnerHTML != null) throw Error(I(91));
  return pe({}, t, { value: void 0, defaultValue: void 0, children: "" + e._wrapperState.initialValue });
}
function us(e, t) {
  var n = t.value;
  if (n == null) {
    if (n = t.children, t = t.defaultValue, n != null) {
      if (t != null) throw Error(I(92));
      if (yr(n)) {
        if (1 < n.length) throw Error(I(93));
        n = n[0];
      }
      t = n;
    }
    t == null && (t = ""), n = t;
  }
  e._wrapperState = { initialValue: un(n) };
}
function Lc(e, t) {
  var n = un(t.value), r = un(t.defaultValue);
  n != null && (n = "" + n, n !== e.value && (e.value = n), t.defaultValue == null && e.defaultValue !== n && (e.defaultValue = n)), r != null && (e.defaultValue = "" + r);
}
function ds(e) {
  var t = e.textContent;
  t === e._wrapperState.initialValue && t !== "" && t !== null && (e.value = t);
}
function Ac(e) {
  switch (e) {
    case "svg":
      return "http://www.w3.org/2000/svg";
    case "math":
      return "http://www.w3.org/1998/Math/MathML";
    default:
      return "http://www.w3.org/1999/xhtml";
  }
}
function ci(e, t) {
  return e == null || e === "http://www.w3.org/1999/xhtml" ? Ac(t) : e === "http://www.w3.org/2000/svg" && t === "foreignObject" ? "http://www.w3.org/1999/xhtml" : e;
}
var ia, Mc = function(e) {
  return typeof MSApp < "u" && MSApp.execUnsafeLocalFunction ? function(t, n, r, a) {
    MSApp.execUnsafeLocalFunction(function() {
      return e(t, n, r, a);
    });
  } : e;
}(function(e, t) {
  if (e.namespaceURI !== "http://www.w3.org/2000/svg" || "innerHTML" in e) e.innerHTML = t;
  else {
    for (ia = ia || document.createElement("div"), ia.innerHTML = "<svg>" + t.valueOf().toString() + "</svg>", t = ia.firstChild; e.firstChild; ) e.removeChild(e.firstChild);
    for (; t.firstChild; ) e.appendChild(t.firstChild);
  }
});
function Dr(e, t) {
  if (t) {
    var n = e.firstChild;
    if (n && n === e.lastChild && n.nodeType === 3) {
      n.nodeValue = t;
      return;
    }
  }
  e.textContent = t;
}
var Sr = {
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
}, yf = ["Webkit", "ms", "Moz", "O"];
Object.keys(Sr).forEach(function(e) {
  yf.forEach(function(t) {
    t = t + e.charAt(0).toUpperCase() + e.substring(1), Sr[t] = Sr[e];
  });
});
function $c(e, t, n) {
  return t == null || typeof t == "boolean" || t === "" ? "" : n || typeof t != "number" || t === 0 || Sr.hasOwnProperty(e) && Sr[e] ? ("" + t).trim() : t + "px";
}
function Oc(e, t) {
  e = e.style;
  for (var n in t) if (t.hasOwnProperty(n)) {
    var r = n.indexOf("--") === 0, a = $c(n, t[n], r);
    n === "float" && (n = "cssFloat"), r ? e.setProperty(n, a) : e[n] = a;
  }
}
var jf = pe({ menuitem: !0 }, { area: !0, base: !0, br: !0, col: !0, embed: !0, hr: !0, img: !0, input: !0, keygen: !0, link: !0, meta: !0, param: !0, source: !0, track: !0, wbr: !0 });
function ui(e, t) {
  if (t) {
    if (jf[e] && (t.children != null || t.dangerouslySetInnerHTML != null)) throw Error(I(137, e));
    if (t.dangerouslySetInnerHTML != null) {
      if (t.children != null) throw Error(I(60));
      if (typeof t.dangerouslySetInnerHTML != "object" || !("__html" in t.dangerouslySetInnerHTML)) throw Error(I(61));
    }
    if (t.style != null && typeof t.style != "object") throw Error(I(62));
  }
}
function di(e, t) {
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
var fi = null;
function ro(e) {
  return e = e.target || e.srcElement || window, e.correspondingUseElement && (e = e.correspondingUseElement), e.nodeType === 3 ? e.parentNode : e;
}
var pi = null, Gn = null, Kn = null;
function fs(e) {
  if (e = Jr(e)) {
    if (typeof pi != "function") throw Error(I(280));
    var t = e.stateNode;
    t && (t = ol(t), pi(e.stateNode, e.type, t));
  }
}
function bc(e) {
  Gn ? Kn ? Kn.push(e) : Kn = [e] : Gn = e;
}
function Uc() {
  if (Gn) {
    var e = Gn, t = Kn;
    if (Kn = Gn = null, fs(e), t) for (e = 0; e < t.length; e++) fs(t[e]);
  }
}
function Vc(e, t) {
  return e(t);
}
function Bc() {
}
var Fl = !1;
function Hc(e, t, n) {
  if (Fl) return e(t, n);
  Fl = !0;
  try {
    return Vc(e, t, n);
  } finally {
    Fl = !1, (Gn !== null || Kn !== null) && (Bc(), Uc());
  }
}
function zr(e, t) {
  var n = e.stateNode;
  if (n === null) return null;
  var r = ol(n);
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
var mi = !1;
if (Ut) try {
  var dr = {};
  Object.defineProperty(dr, "passive", { get: function() {
    mi = !0;
  } }), window.addEventListener("test", dr, dr), window.removeEventListener("test", dr, dr);
} catch {
  mi = !1;
}
function Nf(e, t, n, r, a, i, o, s, c) {
  var d = Array.prototype.slice.call(arguments, 3);
  try {
    t.apply(n, d);
  } catch (j) {
    this.onError(j);
  }
}
var wr = !1, Da = null, za = !1, hi = null, Sf = { onError: function(e) {
  wr = !0, Da = e;
} };
function wf(e, t, n, r, a, i, o, s, c) {
  wr = !1, Da = null, Nf.apply(Sf, arguments);
}
function Cf(e, t, n, r, a, i, o, s, c) {
  if (wf.apply(this, arguments), wr) {
    if (wr) {
      var d = Da;
      wr = !1, Da = null;
    } else throw Error(I(198));
    za || (za = !0, hi = d);
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
function Wc(e) {
  if (e.tag === 13) {
    var t = e.memoizedState;
    if (t === null && (e = e.alternate, e !== null && (t = e.memoizedState)), t !== null) return t.dehydrated;
  }
  return null;
}
function ps(e) {
  if (_n(e) !== e) throw Error(I(188));
}
function kf(e) {
  var t = e.alternate;
  if (!t) {
    if (t = _n(e), t === null) throw Error(I(188));
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
        if (i === n) return ps(a), e;
        if (i === r) return ps(a), t;
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
function Qc(e) {
  return e = kf(e), e !== null ? Gc(e) : null;
}
function Gc(e) {
  if (e.tag === 5 || e.tag === 6) return e;
  for (e = e.child; e !== null; ) {
    var t = Gc(e);
    if (t !== null) return t;
    e = e.sibling;
  }
  return null;
}
var Kc = at.unstable_scheduleCallback, ms = at.unstable_cancelCallback, Ef = at.unstable_shouldYield, If = at.unstable_requestPaint, ye = at.unstable_now, Pf = at.unstable_getCurrentPriorityLevel, ao = at.unstable_ImmediatePriority, qc = at.unstable_UserBlockingPriority, La = at.unstable_NormalPriority, Ff = at.unstable_LowPriority, Yc = at.unstable_IdlePriority, rl = null, _t = null;
function Rf(e) {
  if (_t && typeof _t.onCommitFiberRoot == "function") try {
    _t.onCommitFiberRoot(rl, e, void 0, (e.current.flags & 128) === 128);
  } catch {
  }
}
var St = Math.clz32 ? Math.clz32 : Df, _f = Math.log, Tf = Math.LN2;
function Df(e) {
  return e >>>= 0, e === 0 ? 32 : 31 - (_f(e) / Tf | 0) | 0;
}
var oa = 64, sa = 4194304;
function jr(e) {
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
function Aa(e, t) {
  var n = e.pendingLanes;
  if (n === 0) return 0;
  var r = 0, a = e.suspendedLanes, i = e.pingedLanes, o = n & 268435455;
  if (o !== 0) {
    var s = o & ~a;
    s !== 0 ? r = jr(s) : (i &= o, i !== 0 && (r = jr(i)));
  } else o = n & ~a, o !== 0 ? r = jr(o) : i !== 0 && (r = jr(i));
  if (r === 0) return 0;
  if (t !== 0 && t !== r && !(t & a) && (a = r & -r, i = t & -t, a >= i || a === 16 && (i & 4194240) !== 0)) return t;
  if (r & 4 && (r |= n & 16), t = e.entangledLanes, t !== 0) for (e = e.entanglements, t &= r; 0 < t; ) n = 31 - St(t), a = 1 << n, r |= e[n], t &= ~a;
  return r;
}
function zf(e, t) {
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
function Lf(e, t) {
  for (var n = e.suspendedLanes, r = e.pingedLanes, a = e.expirationTimes, i = e.pendingLanes; 0 < i; ) {
    var o = 31 - St(i), s = 1 << o, c = a[o];
    c === -1 ? (!(s & n) || s & r) && (a[o] = zf(s, t)) : c <= t && (e.expiredLanes |= s), i &= ~s;
  }
}
function vi(e) {
  return e = e.pendingLanes & -1073741825, e !== 0 ? e : e & 1073741824 ? 1073741824 : 0;
}
function Xc() {
  var e = oa;
  return oa <<= 1, !(oa & 4194240) && (oa = 64), e;
}
function Rl(e) {
  for (var t = [], n = 0; 31 > n; n++) t.push(e);
  return t;
}
function Xr(e, t, n) {
  e.pendingLanes |= t, t !== 536870912 && (e.suspendedLanes = 0, e.pingedLanes = 0), e = e.eventTimes, t = 31 - St(t), e[t] = n;
}
function Af(e, t) {
  var n = e.pendingLanes & ~t;
  e.pendingLanes = t, e.suspendedLanes = 0, e.pingedLanes = 0, e.expiredLanes &= t, e.mutableReadLanes &= t, e.entangledLanes &= t, t = e.entanglements;
  var r = e.eventTimes;
  for (e = e.expirationTimes; 0 < n; ) {
    var a = 31 - St(n), i = 1 << a;
    t[a] = 0, r[a] = -1, e[a] = -1, n &= ~i;
  }
}
function lo(e, t) {
  var n = e.entangledLanes |= t;
  for (e = e.entanglements; n; ) {
    var r = 31 - St(n), a = 1 << r;
    a & t | e[r] & t && (e[r] |= t), n &= ~a;
  }
}
var ne = 0;
function Zc(e) {
  return e &= -e, 1 < e ? 4 < e ? e & 268435455 ? 16 : 536870912 : 4 : 1;
}
var Jc, io, eu, tu, nu, xi = !1, ca = [], tn = null, nn = null, rn = null, Lr = /* @__PURE__ */ new Map(), Ar = /* @__PURE__ */ new Map(), Yt = [], Mf = "mousedown mouseup touchcancel touchend touchstart auxclick dblclick pointercancel pointerdown pointerup dragend dragstart drop compositionend compositionstart keydown keypress keyup input textInput copy cut paste click change contextmenu reset submit".split(" ");
function hs(e, t) {
  switch (e) {
    case "focusin":
    case "focusout":
      tn = null;
      break;
    case "dragenter":
    case "dragleave":
      nn = null;
      break;
    case "mouseover":
    case "mouseout":
      rn = null;
      break;
    case "pointerover":
    case "pointerout":
      Lr.delete(t.pointerId);
      break;
    case "gotpointercapture":
    case "lostpointercapture":
      Ar.delete(t.pointerId);
  }
}
function fr(e, t, n, r, a, i) {
  return e === null || e.nativeEvent !== i ? (e = { blockedOn: t, domEventName: n, eventSystemFlags: r, nativeEvent: i, targetContainers: [a] }, t !== null && (t = Jr(t), t !== null && io(t)), e) : (e.eventSystemFlags |= r, t = e.targetContainers, a !== null && t.indexOf(a) === -1 && t.push(a), e);
}
function $f(e, t, n, r, a) {
  switch (t) {
    case "focusin":
      return tn = fr(tn, e, t, n, r, a), !0;
    case "dragenter":
      return nn = fr(nn, e, t, n, r, a), !0;
    case "mouseover":
      return rn = fr(rn, e, t, n, r, a), !0;
    case "pointerover":
      var i = a.pointerId;
      return Lr.set(i, fr(Lr.get(i) || null, e, t, n, r, a)), !0;
    case "gotpointercapture":
      return i = a.pointerId, Ar.set(i, fr(Ar.get(i) || null, e, t, n, r, a)), !0;
  }
  return !1;
}
function ru(e) {
  var t = yn(e.target);
  if (t !== null) {
    var n = _n(t);
    if (n !== null) {
      if (t = n.tag, t === 13) {
        if (t = Wc(n), t !== null) {
          e.blockedOn = t, nu(e.priority, function() {
            eu(n);
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
function Sa(e) {
  if (e.blockedOn !== null) return !1;
  for (var t = e.targetContainers; 0 < t.length; ) {
    var n = gi(e.domEventName, e.eventSystemFlags, t[0], e.nativeEvent);
    if (n === null) {
      n = e.nativeEvent;
      var r = new n.constructor(n.type, n);
      fi = r, n.target.dispatchEvent(r), fi = null;
    } else return t = Jr(n), t !== null && io(t), e.blockedOn = n, !1;
    t.shift();
  }
  return !0;
}
function vs(e, t, n) {
  Sa(e) && n.delete(t);
}
function Of() {
  xi = !1, tn !== null && Sa(tn) && (tn = null), nn !== null && Sa(nn) && (nn = null), rn !== null && Sa(rn) && (rn = null), Lr.forEach(vs), Ar.forEach(vs);
}
function pr(e, t) {
  e.blockedOn === t && (e.blockedOn = null, xi || (xi = !0, at.unstable_scheduleCallback(at.unstable_NormalPriority, Of)));
}
function Mr(e) {
  function t(a) {
    return pr(a, e);
  }
  if (0 < ca.length) {
    pr(ca[0], e);
    for (var n = 1; n < ca.length; n++) {
      var r = ca[n];
      r.blockedOn === e && (r.blockedOn = null);
    }
  }
  for (tn !== null && pr(tn, e), nn !== null && pr(nn, e), rn !== null && pr(rn, e), Lr.forEach(t), Ar.forEach(t), n = 0; n < Yt.length; n++) r = Yt[n], r.blockedOn === e && (r.blockedOn = null);
  for (; 0 < Yt.length && (n = Yt[0], n.blockedOn === null); ) ru(n), n.blockedOn === null && Yt.shift();
}
var qn = Wt.ReactCurrentBatchConfig, Ma = !0;
function bf(e, t, n, r) {
  var a = ne, i = qn.transition;
  qn.transition = null;
  try {
    ne = 1, oo(e, t, n, r);
  } finally {
    ne = a, qn.transition = i;
  }
}
function Uf(e, t, n, r) {
  var a = ne, i = qn.transition;
  qn.transition = null;
  try {
    ne = 4, oo(e, t, n, r);
  } finally {
    ne = a, qn.transition = i;
  }
}
function oo(e, t, n, r) {
  if (Ma) {
    var a = gi(e, t, n, r);
    if (a === null) bl(e, t, r, $a, n), hs(e, r);
    else if ($f(a, e, t, n, r)) r.stopPropagation();
    else if (hs(e, r), t & 4 && -1 < Mf.indexOf(e)) {
      for (; a !== null; ) {
        var i = Jr(a);
        if (i !== null && Jc(i), i = gi(e, t, n, r), i === null && bl(e, t, r, $a, n), i === a) break;
        a = i;
      }
      a !== null && r.stopPropagation();
    } else bl(e, t, r, null, n);
  }
}
var $a = null;
function gi(e, t, n, r) {
  if ($a = null, e = ro(r), e = yn(e), e !== null) if (t = _n(e), t === null) e = null;
  else if (n = t.tag, n === 13) {
    if (e = Wc(t), e !== null) return e;
    e = null;
  } else if (n === 3) {
    if (t.stateNode.current.memoizedState.isDehydrated) return t.tag === 3 ? t.stateNode.containerInfo : null;
    e = null;
  } else t !== e && (e = null);
  return $a = e, null;
}
function au(e) {
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
      switch (Pf()) {
        case ao:
          return 1;
        case qc:
          return 4;
        case La:
        case Ff:
          return 16;
        case Yc:
          return 536870912;
        default:
          return 16;
      }
    default:
      return 16;
  }
}
var Jt = null, so = null, wa = null;
function lu() {
  if (wa) return wa;
  var e, t = so, n = t.length, r, a = "value" in Jt ? Jt.value : Jt.textContent, i = a.length;
  for (e = 0; e < n && t[e] === a[e]; e++) ;
  var o = n - e;
  for (r = 1; r <= o && t[n - r] === a[i - r]; r++) ;
  return wa = a.slice(e, 1 < r ? 1 - r : void 0);
}
function Ca(e) {
  var t = e.keyCode;
  return "charCode" in e ? (e = e.charCode, e === 0 && t === 13 && (e = 13)) : e = t, e === 10 && (e = 13), 32 <= e || e === 13 ? e : 0;
}
function ua() {
  return !0;
}
function xs() {
  return !1;
}
function it(e) {
  function t(n, r, a, i, o) {
    this._reactName = n, this._targetInst = a, this.type = r, this.nativeEvent = i, this.target = o, this.currentTarget = null;
    for (var s in e) e.hasOwnProperty(s) && (n = e[s], this[s] = n ? n(i) : i[s]);
    return this.isDefaultPrevented = (i.defaultPrevented != null ? i.defaultPrevented : i.returnValue === !1) ? ua : xs, this.isPropagationStopped = xs, this;
  }
  return pe(t.prototype, { preventDefault: function() {
    this.defaultPrevented = !0;
    var n = this.nativeEvent;
    n && (n.preventDefault ? n.preventDefault() : typeof n.returnValue != "unknown" && (n.returnValue = !1), this.isDefaultPrevented = ua);
  }, stopPropagation: function() {
    var n = this.nativeEvent;
    n && (n.stopPropagation ? n.stopPropagation() : typeof n.cancelBubble != "unknown" && (n.cancelBubble = !0), this.isPropagationStopped = ua);
  }, persist: function() {
  }, isPersistent: ua }), t;
}
var or = { eventPhase: 0, bubbles: 0, cancelable: 0, timeStamp: function(e) {
  return e.timeStamp || Date.now();
}, defaultPrevented: 0, isTrusted: 0 }, co = it(or), Zr = pe({}, or, { view: 0, detail: 0 }), Vf = it(Zr), _l, Tl, mr, al = pe({}, Zr, { screenX: 0, screenY: 0, clientX: 0, clientY: 0, pageX: 0, pageY: 0, ctrlKey: 0, shiftKey: 0, altKey: 0, metaKey: 0, getModifierState: uo, button: 0, buttons: 0, relatedTarget: function(e) {
  return e.relatedTarget === void 0 ? e.fromElement === e.srcElement ? e.toElement : e.fromElement : e.relatedTarget;
}, movementX: function(e) {
  return "movementX" in e ? e.movementX : (e !== mr && (mr && e.type === "mousemove" ? (_l = e.screenX - mr.screenX, Tl = e.screenY - mr.screenY) : Tl = _l = 0, mr = e), _l);
}, movementY: function(e) {
  return "movementY" in e ? e.movementY : Tl;
} }), gs = it(al), Bf = pe({}, al, { dataTransfer: 0 }), Hf = it(Bf), Wf = pe({}, Zr, { relatedTarget: 0 }), Dl = it(Wf), Qf = pe({}, or, { animationName: 0, elapsedTime: 0, pseudoElement: 0 }), Gf = it(Qf), Kf = pe({}, or, { clipboardData: function(e) {
  return "clipboardData" in e ? e.clipboardData : window.clipboardData;
} }), qf = it(Kf), Yf = pe({}, or, { data: 0 }), ys = it(Yf), Xf = {
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
}, Zf = {
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
}, Jf = { Alt: "altKey", Control: "ctrlKey", Meta: "metaKey", Shift: "shiftKey" };
function ep(e) {
  var t = this.nativeEvent;
  return t.getModifierState ? t.getModifierState(e) : (e = Jf[e]) ? !!t[e] : !1;
}
function uo() {
  return ep;
}
var tp = pe({}, Zr, { key: function(e) {
  if (e.key) {
    var t = Xf[e.key] || e.key;
    if (t !== "Unidentified") return t;
  }
  return e.type === "keypress" ? (e = Ca(e), e === 13 ? "Enter" : String.fromCharCode(e)) : e.type === "keydown" || e.type === "keyup" ? Zf[e.keyCode] || "Unidentified" : "";
}, code: 0, location: 0, ctrlKey: 0, shiftKey: 0, altKey: 0, metaKey: 0, repeat: 0, locale: 0, getModifierState: uo, charCode: function(e) {
  return e.type === "keypress" ? Ca(e) : 0;
}, keyCode: function(e) {
  return e.type === "keydown" || e.type === "keyup" ? e.keyCode : 0;
}, which: function(e) {
  return e.type === "keypress" ? Ca(e) : e.type === "keydown" || e.type === "keyup" ? e.keyCode : 0;
} }), np = it(tp), rp = pe({}, al, { pointerId: 0, width: 0, height: 0, pressure: 0, tangentialPressure: 0, tiltX: 0, tiltY: 0, twist: 0, pointerType: 0, isPrimary: 0 }), js = it(rp), ap = pe({}, Zr, { touches: 0, targetTouches: 0, changedTouches: 0, altKey: 0, metaKey: 0, ctrlKey: 0, shiftKey: 0, getModifierState: uo }), lp = it(ap), ip = pe({}, or, { propertyName: 0, elapsedTime: 0, pseudoElement: 0 }), op = it(ip), sp = pe({}, al, {
  deltaX: function(e) {
    return "deltaX" in e ? e.deltaX : "wheelDeltaX" in e ? -e.wheelDeltaX : 0;
  },
  deltaY: function(e) {
    return "deltaY" in e ? e.deltaY : "wheelDeltaY" in e ? -e.wheelDeltaY : "wheelDelta" in e ? -e.wheelDelta : 0;
  },
  deltaZ: 0,
  deltaMode: 0
}), cp = it(sp), up = [9, 13, 27, 32], fo = Ut && "CompositionEvent" in window, Cr = null;
Ut && "documentMode" in document && (Cr = document.documentMode);
var dp = Ut && "TextEvent" in window && !Cr, iu = Ut && (!fo || Cr && 8 < Cr && 11 >= Cr), Ns = " ", Ss = !1;
function ou(e, t) {
  switch (e) {
    case "keyup":
      return up.indexOf(t.keyCode) !== -1;
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
function su(e) {
  return e = e.detail, typeof e == "object" && "data" in e ? e.data : null;
}
var An = !1;
function fp(e, t) {
  switch (e) {
    case "compositionend":
      return su(t);
    case "keypress":
      return t.which !== 32 ? null : (Ss = !0, Ns);
    case "textInput":
      return e = t.data, e === Ns && Ss ? null : e;
    default:
      return null;
  }
}
function pp(e, t) {
  if (An) return e === "compositionend" || !fo && ou(e, t) ? (e = lu(), wa = so = Jt = null, An = !1, e) : null;
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
      return iu && t.locale !== "ko" ? null : t.data;
    default:
      return null;
  }
}
var mp = { color: !0, date: !0, datetime: !0, "datetime-local": !0, email: !0, month: !0, number: !0, password: !0, range: !0, search: !0, tel: !0, text: !0, time: !0, url: !0, week: !0 };
function ws(e) {
  var t = e && e.nodeName && e.nodeName.toLowerCase();
  return t === "input" ? !!mp[e.type] : t === "textarea";
}
function cu(e, t, n, r) {
  bc(r), t = Oa(t, "onChange"), 0 < t.length && (n = new co("onChange", "change", null, n, r), e.push({ event: n, listeners: t }));
}
var kr = null, $r = null;
function hp(e) {
  ju(e, 0);
}
function ll(e) {
  var t = On(e);
  if (Dc(t)) return e;
}
function vp(e, t) {
  if (e === "change") return t;
}
var uu = !1;
if (Ut) {
  var zl;
  if (Ut) {
    var Ll = "oninput" in document;
    if (!Ll) {
      var Cs = document.createElement("div");
      Cs.setAttribute("oninput", "return;"), Ll = typeof Cs.oninput == "function";
    }
    zl = Ll;
  } else zl = !1;
  uu = zl && (!document.documentMode || 9 < document.documentMode);
}
function ks() {
  kr && (kr.detachEvent("onpropertychange", du), $r = kr = null);
}
function du(e) {
  if (e.propertyName === "value" && ll($r)) {
    var t = [];
    cu(t, $r, e, ro(e)), Hc(hp, t);
  }
}
function xp(e, t, n) {
  e === "focusin" ? (ks(), kr = t, $r = n, kr.attachEvent("onpropertychange", du)) : e === "focusout" && ks();
}
function gp(e) {
  if (e === "selectionchange" || e === "keyup" || e === "keydown") return ll($r);
}
function yp(e, t) {
  if (e === "click") return ll(t);
}
function jp(e, t) {
  if (e === "input" || e === "change") return ll(t);
}
function Np(e, t) {
  return e === t && (e !== 0 || 1 / e === 1 / t) || e !== e && t !== t;
}
var kt = typeof Object.is == "function" ? Object.is : Np;
function Or(e, t) {
  if (kt(e, t)) return !0;
  if (typeof e != "object" || e === null || typeof t != "object" || t === null) return !1;
  var n = Object.keys(e), r = Object.keys(t);
  if (n.length !== r.length) return !1;
  for (r = 0; r < n.length; r++) {
    var a = n[r];
    if (!ei.call(t, a) || !kt(e[a], t[a])) return !1;
  }
  return !0;
}
function Es(e) {
  for (; e && e.firstChild; ) e = e.firstChild;
  return e;
}
function Is(e, t) {
  var n = Es(e);
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
    n = Es(n);
  }
}
function fu(e, t) {
  return e && t ? e === t ? !0 : e && e.nodeType === 3 ? !1 : t && t.nodeType === 3 ? fu(e, t.parentNode) : "contains" in e ? e.contains(t) : e.compareDocumentPosition ? !!(e.compareDocumentPosition(t) & 16) : !1 : !1;
}
function pu() {
  for (var e = window, t = Ta(); t instanceof e.HTMLIFrameElement; ) {
    try {
      var n = typeof t.contentWindow.location.href == "string";
    } catch {
      n = !1;
    }
    if (n) e = t.contentWindow;
    else break;
    t = Ta(e.document);
  }
  return t;
}
function po(e) {
  var t = e && e.nodeName && e.nodeName.toLowerCase();
  return t && (t === "input" && (e.type === "text" || e.type === "search" || e.type === "tel" || e.type === "url" || e.type === "password") || t === "textarea" || e.contentEditable === "true");
}
function Sp(e) {
  var t = pu(), n = e.focusedElem, r = e.selectionRange;
  if (t !== n && n && n.ownerDocument && fu(n.ownerDocument.documentElement, n)) {
    if (r !== null && po(n)) {
      if (t = r.start, e = r.end, e === void 0 && (e = t), "selectionStart" in n) n.selectionStart = t, n.selectionEnd = Math.min(e, n.value.length);
      else if (e = (t = n.ownerDocument || document) && t.defaultView || window, e.getSelection) {
        e = e.getSelection();
        var a = n.textContent.length, i = Math.min(r.start, a);
        r = r.end === void 0 ? i : Math.min(r.end, a), !e.extend && i > r && (a = r, r = i, i = a), a = Is(n, i);
        var o = Is(
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
var wp = Ut && "documentMode" in document && 11 >= document.documentMode, Mn = null, yi = null, Er = null, ji = !1;
function Ps(e, t, n) {
  var r = n.window === n ? n.document : n.nodeType === 9 ? n : n.ownerDocument;
  ji || Mn == null || Mn !== Ta(r) || (r = Mn, "selectionStart" in r && po(r) ? r = { start: r.selectionStart, end: r.selectionEnd } : (r = (r.ownerDocument && r.ownerDocument.defaultView || window).getSelection(), r = { anchorNode: r.anchorNode, anchorOffset: r.anchorOffset, focusNode: r.focusNode, focusOffset: r.focusOffset }), Er && Or(Er, r) || (Er = r, r = Oa(yi, "onSelect"), 0 < r.length && (t = new co("onSelect", "select", null, t, n), e.push({ event: t, listeners: r }), t.target = Mn)));
}
function da(e, t) {
  var n = {};
  return n[e.toLowerCase()] = t.toLowerCase(), n["Webkit" + e] = "webkit" + t, n["Moz" + e] = "moz" + t, n;
}
var $n = { animationend: da("Animation", "AnimationEnd"), animationiteration: da("Animation", "AnimationIteration"), animationstart: da("Animation", "AnimationStart"), transitionend: da("Transition", "TransitionEnd") }, Al = {}, mu = {};
Ut && (mu = document.createElement("div").style, "AnimationEvent" in window || (delete $n.animationend.animation, delete $n.animationiteration.animation, delete $n.animationstart.animation), "TransitionEvent" in window || delete $n.transitionend.transition);
function il(e) {
  if (Al[e]) return Al[e];
  if (!$n[e]) return e;
  var t = $n[e], n;
  for (n in t) if (t.hasOwnProperty(n) && n in mu) return Al[e] = t[n];
  return e;
}
var hu = il("animationend"), vu = il("animationiteration"), xu = il("animationstart"), gu = il("transitionend"), yu = /* @__PURE__ */ new Map(), Fs = "abort auxClick cancel canPlay canPlayThrough click close contextMenu copy cut drag dragEnd dragEnter dragExit dragLeave dragOver dragStart drop durationChange emptied encrypted ended error gotPointerCapture input invalid keyDown keyPress keyUp load loadedData loadedMetadata loadStart lostPointerCapture mouseDown mouseMove mouseOut mouseOver mouseUp paste pause play playing pointerCancel pointerDown pointerMove pointerOut pointerOver pointerUp progress rateChange reset resize seeked seeking stalled submit suspend timeUpdate touchCancel touchEnd touchStart volumeChange scroll toggle touchMove waiting wheel".split(" ");
function pn(e, t) {
  yu.set(e, t), Rn(t, [e]);
}
for (var Ml = 0; Ml < Fs.length; Ml++) {
  var $l = Fs[Ml], Cp = $l.toLowerCase(), kp = $l[0].toUpperCase() + $l.slice(1);
  pn(Cp, "on" + kp);
}
pn(hu, "onAnimationEnd");
pn(vu, "onAnimationIteration");
pn(xu, "onAnimationStart");
pn("dblclick", "onDoubleClick");
pn("focusin", "onFocus");
pn("focusout", "onBlur");
pn(gu, "onTransitionEnd");
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
var Nr = "abort canplay canplaythrough durationchange emptied encrypted ended error loadeddata loadedmetadata loadstart pause play playing progress ratechange resize seeked seeking stalled suspend timeupdate volumechange waiting".split(" "), Ep = new Set("cancel close invalid load scroll toggle".split(" ").concat(Nr));
function Rs(e, t, n) {
  var r = e.type || "unknown-event";
  e.currentTarget = n, Cf(r, t, void 0, e), e.currentTarget = null;
}
function ju(e, t) {
  t = (t & 4) !== 0;
  for (var n = 0; n < e.length; n++) {
    var r = e[n], a = r.event;
    r = r.listeners;
    e: {
      var i = void 0;
      if (t) for (var o = r.length - 1; 0 <= o; o--) {
        var s = r[o], c = s.instance, d = s.currentTarget;
        if (s = s.listener, c !== i && a.isPropagationStopped()) break e;
        Rs(a, s, d), i = c;
      }
      else for (o = 0; o < r.length; o++) {
        if (s = r[o], c = s.instance, d = s.currentTarget, s = s.listener, c !== i && a.isPropagationStopped()) break e;
        Rs(a, s, d), i = c;
      }
    }
  }
  if (za) throw e = hi, za = !1, hi = null, e;
}
function ie(e, t) {
  var n = t[ki];
  n === void 0 && (n = t[ki] = /* @__PURE__ */ new Set());
  var r = e + "__bubble";
  n.has(r) || (Nu(t, e, 2, !1), n.add(r));
}
function Ol(e, t, n) {
  var r = 0;
  t && (r |= 4), Nu(n, e, r, t);
}
var fa = "_reactListening" + Math.random().toString(36).slice(2);
function br(e) {
  if (!e[fa]) {
    e[fa] = !0, Pc.forEach(function(n) {
      n !== "selectionchange" && (Ep.has(n) || Ol(n, !1, e), Ol(n, !0, e));
    });
    var t = e.nodeType === 9 ? e : e.ownerDocument;
    t === null || t[fa] || (t[fa] = !0, Ol("selectionchange", !1, t));
  }
}
function Nu(e, t, n, r) {
  switch (au(t)) {
    case 1:
      var a = bf;
      break;
    case 4:
      a = Uf;
      break;
    default:
      a = oo;
  }
  n = a.bind(null, t, n, e), a = void 0, !mi || t !== "touchstart" && t !== "touchmove" && t !== "wheel" || (a = !0), r ? a !== void 0 ? e.addEventListener(t, n, { capture: !0, passive: a }) : e.addEventListener(t, n, !0) : a !== void 0 ? e.addEventListener(t, n, { passive: a }) : e.addEventListener(t, n, !1);
}
function bl(e, t, n, r, a) {
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
  Hc(function() {
    var d = i, j = ro(n), u = [];
    e: {
      var m = yu.get(e);
      if (m !== void 0) {
        var h = co, g = e;
        switch (e) {
          case "keypress":
            if (Ca(n) === 0) break e;
          case "keydown":
          case "keyup":
            h = np;
            break;
          case "focusin":
            g = "focus", h = Dl;
            break;
          case "focusout":
            g = "blur", h = Dl;
            break;
          case "beforeblur":
          case "afterblur":
            h = Dl;
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
            h = gs;
            break;
          case "drag":
          case "dragend":
          case "dragenter":
          case "dragexit":
          case "dragleave":
          case "dragover":
          case "dragstart":
          case "drop":
            h = Hf;
            break;
          case "touchcancel":
          case "touchend":
          case "touchmove":
          case "touchstart":
            h = lp;
            break;
          case hu:
          case vu:
          case xu:
            h = Gf;
            break;
          case gu:
            h = op;
            break;
          case "scroll":
            h = Vf;
            break;
          case "wheel":
            h = cp;
            break;
          case "copy":
          case "cut":
          case "paste":
            h = qf;
            break;
          case "gotpointercapture":
          case "lostpointercapture":
          case "pointercancel":
          case "pointerdown":
          case "pointermove":
          case "pointerout":
          case "pointerover":
          case "pointerup":
            h = js;
        }
        var w = (t & 4) !== 0, $ = !w && e === "scroll", p = w ? m !== null ? m + "Capture" : null : m;
        w = [];
        for (var f = d, v; f !== null; ) {
          v = f;
          var k = v.stateNode;
          if (v.tag === 5 && k !== null && (v = k, p !== null && (k = zr(f, p), k != null && w.push(Ur(f, k, v)))), $) break;
          f = f.return;
        }
        0 < w.length && (m = new h(m, g, null, n, j), u.push({ event: m, listeners: w }));
      }
    }
    if (!(t & 7)) {
      e: {
        if (m = e === "mouseover" || e === "pointerover", h = e === "mouseout" || e === "pointerout", m && n !== fi && (g = n.relatedTarget || n.fromElement) && (yn(g) || g[Vt])) break e;
        if ((h || m) && (m = j.window === j ? j : (m = j.ownerDocument) ? m.defaultView || m.parentWindow : window, h ? (g = n.relatedTarget || n.toElement, h = d, g = g ? yn(g) : null, g !== null && ($ = _n(g), g !== $ || g.tag !== 5 && g.tag !== 6) && (g = null)) : (h = null, g = d), h !== g)) {
          if (w = gs, k = "onMouseLeave", p = "onMouseEnter", f = "mouse", (e === "pointerout" || e === "pointerover") && (w = js, k = "onPointerLeave", p = "onPointerEnter", f = "pointer"), $ = h == null ? m : On(h), v = g == null ? m : On(g), m = new w(k, f + "leave", h, n, j), m.target = $, m.relatedTarget = v, k = null, yn(j) === d && (w = new w(p, f + "enter", g, n, j), w.target = v, w.relatedTarget = $, k = w), $ = k, h && g) t: {
            for (w = h, p = g, f = 0, v = w; v; v = Dn(v)) f++;
            for (v = 0, k = p; k; k = Dn(k)) v++;
            for (; 0 < f - v; ) w = Dn(w), f--;
            for (; 0 < v - f; ) p = Dn(p), v--;
            for (; f--; ) {
              if (w === p || p !== null && w === p.alternate) break t;
              w = Dn(w), p = Dn(p);
            }
            w = null;
          }
          else w = null;
          h !== null && _s(u, m, h, w, !1), g !== null && $ !== null && _s(u, $, g, w, !0);
        }
      }
      e: {
        if (m = d ? On(d) : window, h = m.nodeName && m.nodeName.toLowerCase(), h === "select" || h === "input" && m.type === "file") var _ = vp;
        else if (ws(m)) if (uu) _ = jp;
        else {
          _ = gp;
          var F = xp;
        }
        else (h = m.nodeName) && h.toLowerCase() === "input" && (m.type === "checkbox" || m.type === "radio") && (_ = yp);
        if (_ && (_ = _(e, d))) {
          cu(u, _, n, j);
          break e;
        }
        F && F(e, m, d), e === "focusout" && (F = m._wrapperState) && F.controlled && m.type === "number" && oi(m, "number", m.value);
      }
      switch (F = d ? On(d) : window, e) {
        case "focusin":
          (ws(F) || F.contentEditable === "true") && (Mn = F, yi = d, Er = null);
          break;
        case "focusout":
          Er = yi = Mn = null;
          break;
        case "mousedown":
          ji = !0;
          break;
        case "contextmenu":
        case "mouseup":
        case "dragend":
          ji = !1, Ps(u, n, j);
          break;
        case "selectionchange":
          if (wp) break;
        case "keydown":
        case "keyup":
          Ps(u, n, j);
      }
      var T;
      if (fo) e: {
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
      else An ? ou(e, n) && (C = "onCompositionEnd") : e === "keydown" && n.keyCode === 229 && (C = "onCompositionStart");
      C && (iu && n.locale !== "ko" && (An || C !== "onCompositionStart" ? C === "onCompositionEnd" && An && (T = lu()) : (Jt = j, so = "value" in Jt ? Jt.value : Jt.textContent, An = !0)), F = Oa(d, C), 0 < F.length && (C = new ys(C, e, null, n, j), u.push({ event: C, listeners: F }), T ? C.data = T : (T = su(n), T !== null && (C.data = T)))), (T = dp ? fp(e, n) : pp(e, n)) && (d = Oa(d, "onBeforeInput"), 0 < d.length && (j = new ys("onBeforeInput", "beforeinput", null, n, j), u.push({ event: j, listeners: d }), j.data = T));
    }
    ju(u, t);
  });
}
function Ur(e, t, n) {
  return { instance: e, listener: t, currentTarget: n };
}
function Oa(e, t) {
  for (var n = t + "Capture", r = []; e !== null; ) {
    var a = e, i = a.stateNode;
    a.tag === 5 && i !== null && (a = i, i = zr(e, n), i != null && r.unshift(Ur(e, i, a)), i = zr(e, t), i != null && r.push(Ur(e, i, a))), e = e.return;
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
function _s(e, t, n, r, a) {
  for (var i = t._reactName, o = []; n !== null && n !== r; ) {
    var s = n, c = s.alternate, d = s.stateNode;
    if (c !== null && c === r) break;
    s.tag === 5 && d !== null && (s = d, a ? (c = zr(n, i), c != null && o.unshift(Ur(n, c, s))) : a || (c = zr(n, i), c != null && o.push(Ur(n, c, s)))), n = n.return;
  }
  o.length !== 0 && e.push({ event: t, listeners: o });
}
var Ip = /\r\n?/g, Pp = /\u0000|\uFFFD/g;
function Ts(e) {
  return (typeof e == "string" ? e : "" + e).replace(Ip, `
`).replace(Pp, "");
}
function pa(e, t, n) {
  if (t = Ts(t), Ts(e) !== t && n) throw Error(I(425));
}
function ba() {
}
var Ni = null, Si = null;
function wi(e, t) {
  return e === "textarea" || e === "noscript" || typeof t.children == "string" || typeof t.children == "number" || typeof t.dangerouslySetInnerHTML == "object" && t.dangerouslySetInnerHTML !== null && t.dangerouslySetInnerHTML.__html != null;
}
var Ci = typeof setTimeout == "function" ? setTimeout : void 0, Fp = typeof clearTimeout == "function" ? clearTimeout : void 0, Ds = typeof Promise == "function" ? Promise : void 0, Rp = typeof queueMicrotask == "function" ? queueMicrotask : typeof Ds < "u" ? function(e) {
  return Ds.resolve(null).then(e).catch(_p);
} : Ci;
function _p(e) {
  setTimeout(function() {
    throw e;
  });
}
function Ul(e, t) {
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
function an(e) {
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
function zs(e) {
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
var sr = Math.random().toString(36).slice(2), Rt = "__reactFiber$" + sr, Vr = "__reactProps$" + sr, Vt = "__reactContainer$" + sr, ki = "__reactEvents$" + sr, Tp = "__reactListeners$" + sr, Dp = "__reactHandles$" + sr;
function yn(e) {
  var t = e[Rt];
  if (t) return t;
  for (var n = e.parentNode; n; ) {
    if (t = n[Vt] || n[Rt]) {
      if (n = t.alternate, t.child !== null || n !== null && n.child !== null) for (e = zs(e); e !== null; ) {
        if (n = e[Rt]) return n;
        e = zs(e);
      }
      return t;
    }
    e = n, n = e.parentNode;
  }
  return null;
}
function Jr(e) {
  return e = e[Rt] || e[Vt], !e || e.tag !== 5 && e.tag !== 6 && e.tag !== 13 && e.tag !== 3 ? null : e;
}
function On(e) {
  if (e.tag === 5 || e.tag === 6) return e.stateNode;
  throw Error(I(33));
}
function ol(e) {
  return e[Vr] || null;
}
var Ei = [], bn = -1;
function mn(e) {
  return { current: e };
}
function oe(e) {
  0 > bn || (e.current = Ei[bn], Ei[bn] = null, bn--);
}
function ae(e, t) {
  bn++, Ei[bn] = e.current, e.current = t;
}
var dn = {}, Ve = mn(dn), Xe = mn(!1), kn = dn;
function er(e, t) {
  var n = e.type.contextTypes;
  if (!n) return dn;
  var r = e.stateNode;
  if (r && r.__reactInternalMemoizedUnmaskedChildContext === t) return r.__reactInternalMemoizedMaskedChildContext;
  var a = {}, i;
  for (i in n) a[i] = t[i];
  return r && (e = e.stateNode, e.__reactInternalMemoizedUnmaskedChildContext = t, e.__reactInternalMemoizedMaskedChildContext = a), a;
}
function Ze(e) {
  return e = e.childContextTypes, e != null;
}
function Ua() {
  oe(Xe), oe(Ve);
}
function Ls(e, t, n) {
  if (Ve.current !== dn) throw Error(I(168));
  ae(Ve, t), ae(Xe, n);
}
function Su(e, t, n) {
  var r = e.stateNode;
  if (t = t.childContextTypes, typeof r.getChildContext != "function") return n;
  r = r.getChildContext();
  for (var a in r) if (!(a in t)) throw Error(I(108, xf(e) || "Unknown", a));
  return pe({}, n, r);
}
function Va(e) {
  return e = (e = e.stateNode) && e.__reactInternalMemoizedMergedChildContext || dn, kn = Ve.current, ae(Ve, e), ae(Xe, Xe.current), !0;
}
function As(e, t, n) {
  var r = e.stateNode;
  if (!r) throw Error(I(169));
  n ? (e = Su(e, t, kn), r.__reactInternalMemoizedMergedChildContext = e, oe(Xe), oe(Ve), ae(Ve, e)) : oe(Xe), ae(Xe, n);
}
var Mt = null, sl = !1, Vl = !1;
function wu(e) {
  Mt === null ? Mt = [e] : Mt.push(e);
}
function zp(e) {
  sl = !0, wu(e);
}
function hn() {
  if (!Vl && Mt !== null) {
    Vl = !0;
    var e = 0, t = ne;
    try {
      var n = Mt;
      for (ne = 1; e < n.length; e++) {
        var r = n[e];
        do
          r = r(!0);
        while (r !== null);
      }
      Mt = null, sl = !1;
    } catch (a) {
      throw Mt !== null && (Mt = Mt.slice(e + 1)), Kc(ao, hn), a;
    } finally {
      ne = t, Vl = !1;
    }
  }
  return null;
}
var Un = [], Vn = 0, Ba = null, Ha = 0, ut = [], dt = 0, En = null, $t = 1, Ot = "";
function xn(e, t) {
  Un[Vn++] = Ha, Un[Vn++] = Ba, Ba = e, Ha = t;
}
function Cu(e, t, n) {
  ut[dt++] = $t, ut[dt++] = Ot, ut[dt++] = En, En = e;
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
function mo(e) {
  e.return !== null && (xn(e, 1), Cu(e, 1, 0));
}
function ho(e) {
  for (; e === Ba; ) Ba = Un[--Vn], Un[Vn] = null, Ha = Un[--Vn], Un[Vn] = null;
  for (; e === En; ) En = ut[--dt], ut[dt] = null, Ot = ut[--dt], ut[dt] = null, $t = ut[--dt], ut[dt] = null;
}
var rt = null, nt = null, se = !1, Nt = null;
function ku(e, t) {
  var n = pt(5, null, null, 0);
  n.elementType = "DELETED", n.stateNode = t, n.return = e, t = e.deletions, t === null ? (e.deletions = [n], e.flags |= 16) : t.push(n);
}
function Ms(e, t) {
  switch (e.tag) {
    case 5:
      var n = e.type;
      return t = t.nodeType !== 1 || n.toLowerCase() !== t.nodeName.toLowerCase() ? null : t, t !== null ? (e.stateNode = t, rt = e, nt = an(t.firstChild), !0) : !1;
    case 6:
      return t = e.pendingProps === "" || t.nodeType !== 3 ? null : t, t !== null ? (e.stateNode = t, rt = e, nt = null, !0) : !1;
    case 13:
      return t = t.nodeType !== 8 ? null : t, t !== null ? (n = En !== null ? { id: $t, overflow: Ot } : null, e.memoizedState = { dehydrated: t, treeContext: n, retryLane: 1073741824 }, n = pt(18, null, null, 0), n.stateNode = t, n.return = e, e.child = n, rt = e, nt = null, !0) : !1;
    default:
      return !1;
  }
}
function Ii(e) {
  return (e.mode & 1) !== 0 && (e.flags & 128) === 0;
}
function Pi(e) {
  if (se) {
    var t = nt;
    if (t) {
      var n = t;
      if (!Ms(e, t)) {
        if (Ii(e)) throw Error(I(418));
        t = an(n.nextSibling);
        var r = rt;
        t && Ms(e, t) ? ku(r, n) : (e.flags = e.flags & -4097 | 2, se = !1, rt = e);
      }
    } else {
      if (Ii(e)) throw Error(I(418));
      e.flags = e.flags & -4097 | 2, se = !1, rt = e;
    }
  }
}
function $s(e) {
  for (e = e.return; e !== null && e.tag !== 5 && e.tag !== 3 && e.tag !== 13; ) e = e.return;
  rt = e;
}
function ma(e) {
  if (e !== rt) return !1;
  if (!se) return $s(e), se = !0, !1;
  var t;
  if ((t = e.tag !== 3) && !(t = e.tag !== 5) && (t = e.type, t = t !== "head" && t !== "body" && !wi(e.type, e.memoizedProps)), t && (t = nt)) {
    if (Ii(e)) throw Eu(), Error(I(418));
    for (; t; ) ku(e, t), t = an(t.nextSibling);
  }
  if ($s(e), e.tag === 13) {
    if (e = e.memoizedState, e = e !== null ? e.dehydrated : null, !e) throw Error(I(317));
    e: {
      for (e = e.nextSibling, t = 0; e; ) {
        if (e.nodeType === 8) {
          var n = e.data;
          if (n === "/$") {
            if (t === 0) {
              nt = an(e.nextSibling);
              break e;
            }
            t--;
          } else n !== "$" && n !== "$!" && n !== "$?" || t++;
        }
        e = e.nextSibling;
      }
      nt = null;
    }
  } else nt = rt ? an(e.stateNode.nextSibling) : null;
  return !0;
}
function Eu() {
  for (var e = nt; e; ) e = an(e.nextSibling);
}
function tr() {
  nt = rt = null, se = !1;
}
function vo(e) {
  Nt === null ? Nt = [e] : Nt.push(e);
}
var Lp = Wt.ReactCurrentBatchConfig;
function hr(e, t, n) {
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
function ha(e, t) {
  throw e = Object.prototype.toString.call(t), Error(I(31, e === "[object Object]" ? "object with keys {" + Object.keys(t).join(", ") + "}" : e));
}
function Os(e) {
  var t = e._init;
  return t(e._payload);
}
function Iu(e) {
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
    return p = cn(p, f), p.index = 0, p.sibling = null, p;
  }
  function i(p, f, v) {
    return p.index = v, e ? (v = p.alternate, v !== null ? (v = v.index, v < f ? (p.flags |= 2, f) : v) : (p.flags |= 2, f)) : (p.flags |= 1048576, f);
  }
  function o(p) {
    return e && p.alternate === null && (p.flags |= 2), p;
  }
  function s(p, f, v, k) {
    return f === null || f.tag !== 6 ? (f = ql(v, p.mode, k), f.return = p, f) : (f = a(f, v), f.return = p, f);
  }
  function c(p, f, v, k) {
    var _ = v.type;
    return _ === Ln ? j(p, f, v.props.children, k, v.key) : f !== null && (f.elementType === _ || typeof _ == "object" && _ !== null && _.$$typeof === Kt && Os(_) === f.type) ? (k = a(f, v.props), k.ref = hr(p, f, v), k.return = p, k) : (k = _a(v.type, v.key, v.props, null, p.mode, k), k.ref = hr(p, f, v), k.return = p, k);
  }
  function d(p, f, v, k) {
    return f === null || f.tag !== 4 || f.stateNode.containerInfo !== v.containerInfo || f.stateNode.implementation !== v.implementation ? (f = Yl(v, p.mode, k), f.return = p, f) : (f = a(f, v.children || []), f.return = p, f);
  }
  function j(p, f, v, k, _) {
    return f === null || f.tag !== 7 ? (f = wn(v, p.mode, k, _), f.return = p, f) : (f = a(f, v), f.return = p, f);
  }
  function u(p, f, v) {
    if (typeof f == "string" && f !== "" || typeof f == "number") return f = ql("" + f, p.mode, v), f.return = p, f;
    if (typeof f == "object" && f !== null) {
      switch (f.$$typeof) {
        case aa:
          return v = _a(f.type, f.key, f.props, null, p.mode, v), v.ref = hr(p, null, f), v.return = p, v;
        case zn:
          return f = Yl(f, p.mode, v), f.return = p, f;
        case Kt:
          var k = f._init;
          return u(p, k(f._payload), v);
      }
      if (yr(f) || ur(f)) return f = wn(f, p.mode, v, null), f.return = p, f;
      ha(p, f);
    }
    return null;
  }
  function m(p, f, v, k) {
    var _ = f !== null ? f.key : null;
    if (typeof v == "string" && v !== "" || typeof v == "number") return _ !== null ? null : s(p, f, "" + v, k);
    if (typeof v == "object" && v !== null) {
      switch (v.$$typeof) {
        case aa:
          return v.key === _ ? c(p, f, v, k) : null;
        case zn:
          return v.key === _ ? d(p, f, v, k) : null;
        case Kt:
          return _ = v._init, m(
            p,
            f,
            _(v._payload),
            k
          );
      }
      if (yr(v) || ur(v)) return _ !== null ? null : j(p, f, v, k, null);
      ha(p, v);
    }
    return null;
  }
  function h(p, f, v, k, _) {
    if (typeof k == "string" && k !== "" || typeof k == "number") return p = p.get(v) || null, s(f, p, "" + k, _);
    if (typeof k == "object" && k !== null) {
      switch (k.$$typeof) {
        case aa:
          return p = p.get(k.key === null ? v : k.key) || null, c(f, p, k, _);
        case zn:
          return p = p.get(k.key === null ? v : k.key) || null, d(f, p, k, _);
        case Kt:
          var F = k._init;
          return h(p, f, v, F(k._payload), _);
      }
      if (yr(k) || ur(k)) return p = p.get(v) || null, j(f, p, k, _, null);
      ha(f, k);
    }
    return null;
  }
  function g(p, f, v, k) {
    for (var _ = null, F = null, T = f, C = f = 0, M = null; T !== null && C < v.length; C++) {
      T.index > C ? (M = T, T = null) : M = T.sibling;
      var b = m(p, T, v[C], k);
      if (b === null) {
        T === null && (T = M);
        break;
      }
      e && T && b.alternate === null && t(p, T), f = i(b, f, C), F === null ? _ = b : F.sibling = b, F = b, T = M;
    }
    if (C === v.length) return n(p, T), se && xn(p, C), _;
    if (T === null) {
      for (; C < v.length; C++) T = u(p, v[C], k), T !== null && (f = i(T, f, C), F === null ? _ = T : F.sibling = T, F = T);
      return se && xn(p, C), _;
    }
    for (T = r(p, T); C < v.length; C++) M = h(T, p, C, v[C], k), M !== null && (e && M.alternate !== null && T.delete(M.key === null ? C : M.key), f = i(M, f, C), F === null ? _ = M : F.sibling = M, F = M);
    return e && T.forEach(function(P) {
      return t(p, P);
    }), se && xn(p, C), _;
  }
  function w(p, f, v, k) {
    var _ = ur(v);
    if (typeof _ != "function") throw Error(I(150));
    if (v = _.call(v), v == null) throw Error(I(151));
    for (var F = _ = null, T = f, C = f = 0, M = null, b = v.next(); T !== null && !b.done; C++, b = v.next()) {
      T.index > C ? (M = T, T = null) : M = T.sibling;
      var P = m(p, T, b.value, k);
      if (P === null) {
        T === null && (T = M);
        break;
      }
      e && T && P.alternate === null && t(p, T), f = i(P, f, C), F === null ? _ = P : F.sibling = P, F = P, T = M;
    }
    if (b.done) return n(
      p,
      T
    ), se && xn(p, C), _;
    if (T === null) {
      for (; !b.done; C++, b = v.next()) b = u(p, b.value, k), b !== null && (f = i(b, f, C), F === null ? _ = b : F.sibling = b, F = b);
      return se && xn(p, C), _;
    }
    for (T = r(p, T); !b.done; C++, b = v.next()) b = h(T, p, C, b.value, k), b !== null && (e && b.alternate !== null && T.delete(b.key === null ? C : b.key), f = i(b, f, C), F === null ? _ = b : F.sibling = b, F = b);
    return e && T.forEach(function(Q) {
      return t(p, Q);
    }), se && xn(p, C), _;
  }
  function $(p, f, v, k) {
    if (typeof v == "object" && v !== null && v.type === Ln && v.key === null && (v = v.props.children), typeof v == "object" && v !== null) {
      switch (v.$$typeof) {
        case aa:
          e: {
            for (var _ = v.key, F = f; F !== null; ) {
              if (F.key === _) {
                if (_ = v.type, _ === Ln) {
                  if (F.tag === 7) {
                    n(p, F.sibling), f = a(F, v.props.children), f.return = p, p = f;
                    break e;
                  }
                } else if (F.elementType === _ || typeof _ == "object" && _ !== null && _.$$typeof === Kt && Os(_) === F.type) {
                  n(p, F.sibling), f = a(F, v.props), f.ref = hr(p, F, v), f.return = p, p = f;
                  break e;
                }
                n(p, F);
                break;
              } else t(p, F);
              F = F.sibling;
            }
            v.type === Ln ? (f = wn(v.props.children, p.mode, k, v.key), f.return = p, p = f) : (k = _a(v.type, v.key, v.props, null, p.mode, k), k.ref = hr(p, f, v), k.return = p, p = k);
          }
          return o(p);
        case zn:
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
            f = Yl(v, p.mode, k), f.return = p, p = f;
          }
          return o(p);
        case Kt:
          return F = v._init, $(p, f, F(v._payload), k);
      }
      if (yr(v)) return g(p, f, v, k);
      if (ur(v)) return w(p, f, v, k);
      ha(p, v);
    }
    return typeof v == "string" && v !== "" || typeof v == "number" ? (v = "" + v, f !== null && f.tag === 6 ? (n(p, f.sibling), f = a(f, v), f.return = p, p = f) : (n(p, f), f = ql(v, p.mode, k), f.return = p, p = f), o(p)) : n(p, f);
  }
  return $;
}
var nr = Iu(!0), Pu = Iu(!1), Wa = mn(null), Qa = null, Bn = null, xo = null;
function go() {
  xo = Bn = Qa = null;
}
function yo(e) {
  var t = Wa.current;
  oe(Wa), e._currentValue = t;
}
function Fi(e, t, n) {
  for (; e !== null; ) {
    var r = e.alternate;
    if ((e.childLanes & t) !== t ? (e.childLanes |= t, r !== null && (r.childLanes |= t)) : r !== null && (r.childLanes & t) !== t && (r.childLanes |= t), e === n) break;
    e = e.return;
  }
}
function Yn(e, t) {
  Qa = e, xo = Bn = null, e = e.dependencies, e !== null && e.firstContext !== null && (e.lanes & t && (Ye = !0), e.firstContext = null);
}
function ht(e) {
  var t = e._currentValue;
  if (xo !== e) if (e = { context: e, memoizedValue: t, next: null }, Bn === null) {
    if (Qa === null) throw Error(I(308));
    Bn = e, Qa.dependencies = { lanes: 0, firstContext: e };
  } else Bn = Bn.next = e;
  return t;
}
var jn = null;
function jo(e) {
  jn === null ? jn = [e] : jn.push(e);
}
function Fu(e, t, n, r) {
  var a = t.interleaved;
  return a === null ? (n.next = n, jo(t)) : (n.next = a.next, a.next = n), t.interleaved = n, Bt(e, r);
}
function Bt(e, t) {
  e.lanes |= t;
  var n = e.alternate;
  for (n !== null && (n.lanes |= t), n = e, e = e.return; e !== null; ) e.childLanes |= t, n = e.alternate, n !== null && (n.childLanes |= t), n = e, e = e.return;
  return n.tag === 3 ? n.stateNode : null;
}
var qt = !1;
function No(e) {
  e.updateQueue = { baseState: e.memoizedState, firstBaseUpdate: null, lastBaseUpdate: null, shared: { pending: null, interleaved: null, lanes: 0 }, effects: null };
}
function Ru(e, t) {
  e = e.updateQueue, t.updateQueue === e && (t.updateQueue = { baseState: e.baseState, firstBaseUpdate: e.firstBaseUpdate, lastBaseUpdate: e.lastBaseUpdate, shared: e.shared, effects: e.effects });
}
function bt(e, t) {
  return { eventTime: e, lane: t, tag: 0, payload: null, callback: null, next: null };
}
function ln(e, t, n) {
  var r = e.updateQueue;
  if (r === null) return null;
  if (r = r.shared, J & 2) {
    var a = r.pending;
    return a === null ? t.next = t : (t.next = a.next, a.next = t), r.pending = t, Bt(e, n);
  }
  return a = r.interleaved, a === null ? (t.next = t, jo(r)) : (t.next = a.next, a.next = t), r.interleaved = t, Bt(e, n);
}
function ka(e, t, n) {
  if (t = t.updateQueue, t !== null && (t = t.shared, (n & 4194240) !== 0)) {
    var r = t.lanes;
    r &= e.pendingLanes, n |= r, t.lanes = n, lo(e, n);
  }
}
function bs(e, t) {
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
function Ga(e, t, n, r) {
  var a = e.updateQueue;
  qt = !1;
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
              u = pe({}, u, m);
              break e;
            case 2:
              qt = !0;
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
    Pn |= o, e.lanes = o, e.memoizedState = u;
  }
}
function Us(e, t, n) {
  if (e = t.effects, t.effects = null, e !== null) for (t = 0; t < e.length; t++) {
    var r = e[t], a = r.callback;
    if (a !== null) {
      if (r.callback = null, r = n, typeof a != "function") throw Error(I(191, a));
      a.call(r);
    }
  }
}
var ea = {}, Tt = mn(ea), Br = mn(ea), Hr = mn(ea);
function Nn(e) {
  if (e === ea) throw Error(I(174));
  return e;
}
function So(e, t) {
  switch (ae(Hr, t), ae(Br, e), ae(Tt, ea), e = t.nodeType, e) {
    case 9:
    case 11:
      t = (t = t.documentElement) ? t.namespaceURI : ci(null, "");
      break;
    default:
      e = e === 8 ? t.parentNode : t, t = e.namespaceURI || null, e = e.tagName, t = ci(t, e);
  }
  oe(Tt), ae(Tt, t);
}
function rr() {
  oe(Tt), oe(Br), oe(Hr);
}
function _u(e) {
  Nn(Hr.current);
  var t = Nn(Tt.current), n = ci(t, e.type);
  t !== n && (ae(Br, e), ae(Tt, n));
}
function wo(e) {
  Br.current === e && (oe(Tt), oe(Br));
}
var ue = mn(0);
function Ka(e) {
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
var Bl = [];
function Co() {
  for (var e = 0; e < Bl.length; e++) Bl[e]._workInProgressVersionPrimary = null;
  Bl.length = 0;
}
var Ea = Wt.ReactCurrentDispatcher, Hl = Wt.ReactCurrentBatchConfig, In = 0, fe = null, ke = null, Fe = null, qa = !1, Ir = !1, Wr = 0, Ap = 0;
function Me() {
  throw Error(I(321));
}
function ko(e, t) {
  if (t === null) return !1;
  for (var n = 0; n < t.length && n < e.length; n++) if (!kt(e[n], t[n])) return !1;
  return !0;
}
function Eo(e, t, n, r, a, i) {
  if (In = i, fe = t, t.memoizedState = null, t.updateQueue = null, t.lanes = 0, Ea.current = e === null || e.memoizedState === null ? bp : Up, e = n(r, a), Ir) {
    i = 0;
    do {
      if (Ir = !1, Wr = 0, 25 <= i) throw Error(I(301));
      i += 1, Fe = ke = null, t.updateQueue = null, Ea.current = Vp, e = n(r, a);
    } while (Ir);
  }
  if (Ea.current = Ya, t = ke !== null && ke.next !== null, In = 0, Fe = ke = fe = null, qa = !1, t) throw Error(I(300));
  return e;
}
function Io() {
  var e = Wr !== 0;
  return Wr = 0, e;
}
function Ft() {
  var e = { memoizedState: null, baseState: null, baseQueue: null, queue: null, next: null };
  return Fe === null ? fe.memoizedState = Fe = e : Fe = Fe.next = e, Fe;
}
function vt() {
  if (ke === null) {
    var e = fe.alternate;
    e = e !== null ? e.memoizedState : null;
  } else e = ke.next;
  var t = Fe === null ? fe.memoizedState : Fe.next;
  if (t !== null) Fe = t, ke = e;
  else {
    if (e === null) throw Error(I(310));
    ke = e, e = { memoizedState: ke.memoizedState, baseState: ke.baseState, baseQueue: ke.baseQueue, queue: ke.queue, next: null }, Fe === null ? fe.memoizedState = Fe = e : Fe = Fe.next = e;
  }
  return Fe;
}
function Qr(e, t) {
  return typeof t == "function" ? t(e) : t;
}
function Wl(e) {
  var t = vt(), n = t.queue;
  if (n === null) throw Error(I(311));
  n.lastRenderedReducer = e;
  var r = ke, a = r.baseQueue, i = n.pending;
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
      if ((In & j) === j) c !== null && (c = c.next = { lane: 0, action: d.action, hasEagerState: d.hasEagerState, eagerState: d.eagerState, next: null }), r = d.hasEagerState ? d.eagerState : e(r, d.action);
      else {
        var u = {
          lane: j,
          action: d.action,
          hasEagerState: d.hasEagerState,
          eagerState: d.eagerState,
          next: null
        };
        c === null ? (s = c = u, o = r) : c = c.next = u, fe.lanes |= j, Pn |= j;
      }
      d = d.next;
    } while (d !== null && d !== i);
    c === null ? o = r : c.next = s, kt(r, t.memoizedState) || (Ye = !0), t.memoizedState = r, t.baseState = o, t.baseQueue = c, n.lastRenderedState = r;
  }
  if (e = n.interleaved, e !== null) {
    a = e;
    do
      i = a.lane, fe.lanes |= i, Pn |= i, a = a.next;
    while (a !== e);
  } else a === null && (n.lanes = 0);
  return [t.memoizedState, n.dispatch];
}
function Ql(e) {
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
function Tu() {
}
function Du(e, t) {
  var n = fe, r = vt(), a = t(), i = !kt(r.memoizedState, a);
  if (i && (r.memoizedState = a, Ye = !0), r = r.queue, Po(Au.bind(null, n, r, e), [e]), r.getSnapshot !== t || i || Fe !== null && Fe.memoizedState.tag & 1) {
    if (n.flags |= 2048, Gr(9, Lu.bind(null, n, r, a, t), void 0, null), Re === null) throw Error(I(349));
    In & 30 || zu(n, t, a);
  }
  return a;
}
function zu(e, t, n) {
  e.flags |= 16384, e = { getSnapshot: t, value: n }, t = fe.updateQueue, t === null ? (t = { lastEffect: null, stores: null }, fe.updateQueue = t, t.stores = [e]) : (n = t.stores, n === null ? t.stores = [e] : n.push(e));
}
function Lu(e, t, n, r) {
  t.value = n, t.getSnapshot = r, Mu(t) && $u(e);
}
function Au(e, t, n) {
  return n(function() {
    Mu(t) && $u(e);
  });
}
function Mu(e) {
  var t = e.getSnapshot;
  e = e.value;
  try {
    var n = t();
    return !kt(e, n);
  } catch {
    return !0;
  }
}
function $u(e) {
  var t = Bt(e, 1);
  t !== null && wt(t, e, 1, -1);
}
function Vs(e) {
  var t = Ft();
  return typeof e == "function" && (e = e()), t.memoizedState = t.baseState = e, e = { pending: null, interleaved: null, lanes: 0, dispatch: null, lastRenderedReducer: Qr, lastRenderedState: e }, t.queue = e, e = e.dispatch = Op.bind(null, fe, e), [t.memoizedState, e];
}
function Gr(e, t, n, r) {
  return e = { tag: e, create: t, destroy: n, deps: r, next: null }, t = fe.updateQueue, t === null ? (t = { lastEffect: null, stores: null }, fe.updateQueue = t, t.lastEffect = e.next = e) : (n = t.lastEffect, n === null ? t.lastEffect = e.next = e : (r = n.next, n.next = e, e.next = r, t.lastEffect = e)), e;
}
function Ou() {
  return vt().memoizedState;
}
function Ia(e, t, n, r) {
  var a = Ft();
  fe.flags |= e, a.memoizedState = Gr(1 | t, n, void 0, r === void 0 ? null : r);
}
function cl(e, t, n, r) {
  var a = vt();
  r = r === void 0 ? null : r;
  var i = void 0;
  if (ke !== null) {
    var o = ke.memoizedState;
    if (i = o.destroy, r !== null && ko(r, o.deps)) {
      a.memoizedState = Gr(t, n, i, r);
      return;
    }
  }
  fe.flags |= e, a.memoizedState = Gr(1 | t, n, i, r);
}
function Bs(e, t) {
  return Ia(8390656, 8, e, t);
}
function Po(e, t) {
  return cl(2048, 8, e, t);
}
function bu(e, t) {
  return cl(4, 2, e, t);
}
function Uu(e, t) {
  return cl(4, 4, e, t);
}
function Vu(e, t) {
  if (typeof t == "function") return e = e(), t(e), function() {
    t(null);
  };
  if (t != null) return e = e(), t.current = e, function() {
    t.current = null;
  };
}
function Bu(e, t, n) {
  return n = n != null ? n.concat([e]) : null, cl(4, 4, Vu.bind(null, t, e), n);
}
function Fo() {
}
function Hu(e, t) {
  var n = vt();
  t = t === void 0 ? null : t;
  var r = n.memoizedState;
  return r !== null && t !== null && ko(t, r[1]) ? r[0] : (n.memoizedState = [e, t], e);
}
function Wu(e, t) {
  var n = vt();
  t = t === void 0 ? null : t;
  var r = n.memoizedState;
  return r !== null && t !== null && ko(t, r[1]) ? r[0] : (e = e(), n.memoizedState = [e, t], e);
}
function Qu(e, t, n) {
  return In & 21 ? (kt(n, t) || (n = Xc(), fe.lanes |= n, Pn |= n, e.baseState = !0), t) : (e.baseState && (e.baseState = !1, Ye = !0), e.memoizedState = n);
}
function Mp(e, t) {
  var n = ne;
  ne = n !== 0 && 4 > n ? n : 4, e(!0);
  var r = Hl.transition;
  Hl.transition = {};
  try {
    e(!1), t();
  } finally {
    ne = n, Hl.transition = r;
  }
}
function Gu() {
  return vt().memoizedState;
}
function $p(e, t, n) {
  var r = sn(e);
  if (n = { lane: r, action: n, hasEagerState: !1, eagerState: null, next: null }, Ku(e)) qu(t, n);
  else if (n = Fu(e, t, n, r), n !== null) {
    var a = Qe();
    wt(n, e, r, a), Yu(n, t, r);
  }
}
function Op(e, t, n) {
  var r = sn(e), a = { lane: r, action: n, hasEagerState: !1, eagerState: null, next: null };
  if (Ku(e)) qu(t, a);
  else {
    var i = e.alternate;
    if (e.lanes === 0 && (i === null || i.lanes === 0) && (i = t.lastRenderedReducer, i !== null)) try {
      var o = t.lastRenderedState, s = i(o, n);
      if (a.hasEagerState = !0, a.eagerState = s, kt(s, o)) {
        var c = t.interleaved;
        c === null ? (a.next = a, jo(t)) : (a.next = c.next, c.next = a), t.interleaved = a;
        return;
      }
    } catch {
    } finally {
    }
    n = Fu(e, t, a, r), n !== null && (a = Qe(), wt(n, e, r, a), Yu(n, t, r));
  }
}
function Ku(e) {
  var t = e.alternate;
  return e === fe || t !== null && t === fe;
}
function qu(e, t) {
  Ir = qa = !0;
  var n = e.pending;
  n === null ? t.next = t : (t.next = n.next, n.next = t), e.pending = t;
}
function Yu(e, t, n) {
  if (n & 4194240) {
    var r = t.lanes;
    r &= e.pendingLanes, n |= r, t.lanes = n, lo(e, n);
  }
}
var Ya = { readContext: ht, useCallback: Me, useContext: Me, useEffect: Me, useImperativeHandle: Me, useInsertionEffect: Me, useLayoutEffect: Me, useMemo: Me, useReducer: Me, useRef: Me, useState: Me, useDebugValue: Me, useDeferredValue: Me, useTransition: Me, useMutableSource: Me, useSyncExternalStore: Me, useId: Me, unstable_isNewReconciler: !1 }, bp = { readContext: ht, useCallback: function(e, t) {
  return Ft().memoizedState = [e, t === void 0 ? null : t], e;
}, useContext: ht, useEffect: Bs, useImperativeHandle: function(e, t, n) {
  return n = n != null ? n.concat([e]) : null, Ia(
    4194308,
    4,
    Vu.bind(null, t, e),
    n
  );
}, useLayoutEffect: function(e, t) {
  return Ia(4194308, 4, e, t);
}, useInsertionEffect: function(e, t) {
  return Ia(4, 2, e, t);
}, useMemo: function(e, t) {
  var n = Ft();
  return t = t === void 0 ? null : t, e = e(), n.memoizedState = [e, t], e;
}, useReducer: function(e, t, n) {
  var r = Ft();
  return t = n !== void 0 ? n(t) : t, r.memoizedState = r.baseState = t, e = { pending: null, interleaved: null, lanes: 0, dispatch: null, lastRenderedReducer: e, lastRenderedState: t }, r.queue = e, e = e.dispatch = $p.bind(null, fe, e), [r.memoizedState, e];
}, useRef: function(e) {
  var t = Ft();
  return e = { current: e }, t.memoizedState = e;
}, useState: Vs, useDebugValue: Fo, useDeferredValue: function(e) {
  return Ft().memoizedState = e;
}, useTransition: function() {
  var e = Vs(!1), t = e[0];
  return e = Mp.bind(null, e[1]), Ft().memoizedState = e, [t, e];
}, useMutableSource: function() {
}, useSyncExternalStore: function(e, t, n) {
  var r = fe, a = Ft();
  if (se) {
    if (n === void 0) throw Error(I(407));
    n = n();
  } else {
    if (n = t(), Re === null) throw Error(I(349));
    In & 30 || zu(r, t, n);
  }
  a.memoizedState = n;
  var i = { value: n, getSnapshot: t };
  return a.queue = i, Bs(Au.bind(
    null,
    r,
    i,
    e
  ), [e]), r.flags |= 2048, Gr(9, Lu.bind(null, r, i, n, t), void 0, null), n;
}, useId: function() {
  var e = Ft(), t = Re.identifierPrefix;
  if (se) {
    var n = Ot, r = $t;
    n = (r & ~(1 << 32 - St(r) - 1)).toString(32) + n, t = ":" + t + "R" + n, n = Wr++, 0 < n && (t += "H" + n.toString(32)), t += ":";
  } else n = Ap++, t = ":" + t + "r" + n.toString(32) + ":";
  return e.memoizedState = t;
}, unstable_isNewReconciler: !1 }, Up = {
  readContext: ht,
  useCallback: Hu,
  useContext: ht,
  useEffect: Po,
  useImperativeHandle: Bu,
  useInsertionEffect: bu,
  useLayoutEffect: Uu,
  useMemo: Wu,
  useReducer: Wl,
  useRef: Ou,
  useState: function() {
    return Wl(Qr);
  },
  useDebugValue: Fo,
  useDeferredValue: function(e) {
    var t = vt();
    return Qu(t, ke.memoizedState, e);
  },
  useTransition: function() {
    var e = Wl(Qr)[0], t = vt().memoizedState;
    return [e, t];
  },
  useMutableSource: Tu,
  useSyncExternalStore: Du,
  useId: Gu,
  unstable_isNewReconciler: !1
}, Vp = { readContext: ht, useCallback: Hu, useContext: ht, useEffect: Po, useImperativeHandle: Bu, useInsertionEffect: bu, useLayoutEffect: Uu, useMemo: Wu, useReducer: Ql, useRef: Ou, useState: function() {
  return Ql(Qr);
}, useDebugValue: Fo, useDeferredValue: function(e) {
  var t = vt();
  return ke === null ? t.memoizedState = e : Qu(t, ke.memoizedState, e);
}, useTransition: function() {
  var e = Ql(Qr)[0], t = vt().memoizedState;
  return [e, t];
}, useMutableSource: Tu, useSyncExternalStore: Du, useId: Gu, unstable_isNewReconciler: !1 };
function yt(e, t) {
  if (e && e.defaultProps) {
    t = pe({}, t), e = e.defaultProps;
    for (var n in e) t[n] === void 0 && (t[n] = e[n]);
    return t;
  }
  return t;
}
function Ri(e, t, n, r) {
  t = e.memoizedState, n = n(r, t), n = n == null ? t : pe({}, t, n), e.memoizedState = n, e.lanes === 0 && (e.updateQueue.baseState = n);
}
var ul = { isMounted: function(e) {
  return (e = e._reactInternals) ? _n(e) === e : !1;
}, enqueueSetState: function(e, t, n) {
  e = e._reactInternals;
  var r = Qe(), a = sn(e), i = bt(r, a);
  i.payload = t, n != null && (i.callback = n), t = ln(e, i, a), t !== null && (wt(t, e, a, r), ka(t, e, a));
}, enqueueReplaceState: function(e, t, n) {
  e = e._reactInternals;
  var r = Qe(), a = sn(e), i = bt(r, a);
  i.tag = 1, i.payload = t, n != null && (i.callback = n), t = ln(e, i, a), t !== null && (wt(t, e, a, r), ka(t, e, a));
}, enqueueForceUpdate: function(e, t) {
  e = e._reactInternals;
  var n = Qe(), r = sn(e), a = bt(n, r);
  a.tag = 2, t != null && (a.callback = t), t = ln(e, a, r), t !== null && (wt(t, e, r, n), ka(t, e, r));
} };
function Hs(e, t, n, r, a, i, o) {
  return e = e.stateNode, typeof e.shouldComponentUpdate == "function" ? e.shouldComponentUpdate(r, i, o) : t.prototype && t.prototype.isPureReactComponent ? !Or(n, r) || !Or(a, i) : !0;
}
function Xu(e, t, n) {
  var r = !1, a = dn, i = t.contextType;
  return typeof i == "object" && i !== null ? i = ht(i) : (a = Ze(t) ? kn : Ve.current, r = t.contextTypes, i = (r = r != null) ? er(e, a) : dn), t = new t(n, i), e.memoizedState = t.state !== null && t.state !== void 0 ? t.state : null, t.updater = ul, e.stateNode = t, t._reactInternals = e, r && (e = e.stateNode, e.__reactInternalMemoizedUnmaskedChildContext = a, e.__reactInternalMemoizedMaskedChildContext = i), t;
}
function Ws(e, t, n, r) {
  e = t.state, typeof t.componentWillReceiveProps == "function" && t.componentWillReceiveProps(n, r), typeof t.UNSAFE_componentWillReceiveProps == "function" && t.UNSAFE_componentWillReceiveProps(n, r), t.state !== e && ul.enqueueReplaceState(t, t.state, null);
}
function _i(e, t, n, r) {
  var a = e.stateNode;
  a.props = n, a.state = e.memoizedState, a.refs = {}, No(e);
  var i = t.contextType;
  typeof i == "object" && i !== null ? a.context = ht(i) : (i = Ze(t) ? kn : Ve.current, a.context = er(e, i)), a.state = e.memoizedState, i = t.getDerivedStateFromProps, typeof i == "function" && (Ri(e, t, i, n), a.state = e.memoizedState), typeof t.getDerivedStateFromProps == "function" || typeof a.getSnapshotBeforeUpdate == "function" || typeof a.UNSAFE_componentWillMount != "function" && typeof a.componentWillMount != "function" || (t = a.state, typeof a.componentWillMount == "function" && a.componentWillMount(), typeof a.UNSAFE_componentWillMount == "function" && a.UNSAFE_componentWillMount(), t !== a.state && ul.enqueueReplaceState(a, a.state, null), Ga(e, n, a, r), a.state = e.memoizedState), typeof a.componentDidMount == "function" && (e.flags |= 4194308);
}
function ar(e, t) {
  try {
    var n = "", r = t;
    do
      n += vf(r), r = r.return;
    while (r);
    var a = n;
  } catch (i) {
    a = `
Error generating stack: ` + i.message + `
` + i.stack;
  }
  return { value: e, source: t, stack: a, digest: null };
}
function Gl(e, t, n) {
  return { value: e, source: null, stack: n ?? null, digest: t ?? null };
}
function Ti(e, t) {
  try {
    console.error(t.value);
  } catch (n) {
    setTimeout(function() {
      throw n;
    });
  }
}
var Bp = typeof WeakMap == "function" ? WeakMap : Map;
function Zu(e, t, n) {
  n = bt(-1, n), n.tag = 3, n.payload = { element: null };
  var r = t.value;
  return n.callback = function() {
    Za || (Za = !0, Vi = r), Ti(e, t);
  }, n;
}
function Ju(e, t, n) {
  n = bt(-1, n), n.tag = 3;
  var r = e.type.getDerivedStateFromError;
  if (typeof r == "function") {
    var a = t.value;
    n.payload = function() {
      return r(a);
    }, n.callback = function() {
      Ti(e, t);
    };
  }
  var i = e.stateNode;
  return i !== null && typeof i.componentDidCatch == "function" && (n.callback = function() {
    Ti(e, t), typeof r != "function" && (on === null ? on = /* @__PURE__ */ new Set([this]) : on.add(this));
    var o = t.stack;
    this.componentDidCatch(t.value, { componentStack: o !== null ? o : "" });
  }), n;
}
function Qs(e, t, n) {
  var r = e.pingCache;
  if (r === null) {
    r = e.pingCache = new Bp();
    var a = /* @__PURE__ */ new Set();
    r.set(t, a);
  } else a = r.get(t), a === void 0 && (a = /* @__PURE__ */ new Set(), r.set(t, a));
  a.has(n) || (a.add(n), e = rm.bind(null, e, t, n), t.then(e, e));
}
function Gs(e) {
  do {
    var t;
    if ((t = e.tag === 13) && (t = e.memoizedState, t = t !== null ? t.dehydrated !== null : !0), t) return e;
    e = e.return;
  } while (e !== null);
  return null;
}
function Ks(e, t, n, r, a) {
  return e.mode & 1 ? (e.flags |= 65536, e.lanes = a, e) : (e === t ? e.flags |= 65536 : (e.flags |= 128, n.flags |= 131072, n.flags &= -52805, n.tag === 1 && (n.alternate === null ? n.tag = 17 : (t = bt(-1, 1), t.tag = 2, ln(n, t, 1))), n.lanes |= 1), e);
}
var Hp = Wt.ReactCurrentOwner, Ye = !1;
function He(e, t, n, r) {
  t.child = e === null ? Pu(t, null, n, r) : nr(t, e.child, n, r);
}
function qs(e, t, n, r, a) {
  n = n.render;
  var i = t.ref;
  return Yn(t, a), r = Eo(e, t, n, r, i, a), n = Io(), e !== null && !Ye ? (t.updateQueue = e.updateQueue, t.flags &= -2053, e.lanes &= ~a, Ht(e, t, a)) : (se && n && mo(t), t.flags |= 1, He(e, t, r, a), t.child);
}
function Ys(e, t, n, r, a) {
  if (e === null) {
    var i = n.type;
    return typeof i == "function" && !Mo(i) && i.defaultProps === void 0 && n.compare === null && n.defaultProps === void 0 ? (t.tag = 15, t.type = i, ed(e, t, i, r, a)) : (e = _a(n.type, null, r, t, t.mode, a), e.ref = t.ref, e.return = t, t.child = e);
  }
  if (i = e.child, !(e.lanes & a)) {
    var o = i.memoizedProps;
    if (n = n.compare, n = n !== null ? n : Or, n(o, r) && e.ref === t.ref) return Ht(e, t, a);
  }
  return t.flags |= 1, e = cn(i, r), e.ref = t.ref, e.return = t, t.child = e;
}
function ed(e, t, n, r, a) {
  if (e !== null) {
    var i = e.memoizedProps;
    if (Or(i, r) && e.ref === t.ref) if (Ye = !1, t.pendingProps = r = i, (e.lanes & a) !== 0) e.flags & 131072 && (Ye = !0);
    else return t.lanes = e.lanes, Ht(e, t, a);
  }
  return Di(e, t, n, r, a);
}
function td(e, t, n) {
  var r = t.pendingProps, a = r.children, i = e !== null ? e.memoizedState : null;
  if (r.mode === "hidden") if (!(t.mode & 1)) t.memoizedState = { baseLanes: 0, cachePool: null, transitions: null }, ae(Wn, tt), tt |= n;
  else {
    if (!(n & 1073741824)) return e = i !== null ? i.baseLanes | n : n, t.lanes = t.childLanes = 1073741824, t.memoizedState = { baseLanes: e, cachePool: null, transitions: null }, t.updateQueue = null, ae(Wn, tt), tt |= e, null;
    t.memoizedState = { baseLanes: 0, cachePool: null, transitions: null }, r = i !== null ? i.baseLanes : n, ae(Wn, tt), tt |= r;
  }
  else i !== null ? (r = i.baseLanes | n, t.memoizedState = null) : r = n, ae(Wn, tt), tt |= r;
  return He(e, t, a, n), t.child;
}
function nd(e, t) {
  var n = t.ref;
  (e === null && n !== null || e !== null && e.ref !== n) && (t.flags |= 512, t.flags |= 2097152);
}
function Di(e, t, n, r, a) {
  var i = Ze(n) ? kn : Ve.current;
  return i = er(t, i), Yn(t, a), n = Eo(e, t, n, r, i, a), r = Io(), e !== null && !Ye ? (t.updateQueue = e.updateQueue, t.flags &= -2053, e.lanes &= ~a, Ht(e, t, a)) : (se && r && mo(t), t.flags |= 1, He(e, t, n, a), t.child);
}
function Xs(e, t, n, r, a) {
  if (Ze(n)) {
    var i = !0;
    Va(t);
  } else i = !1;
  if (Yn(t, a), t.stateNode === null) Pa(e, t), Xu(t, n, r), _i(t, n, r, a), r = !0;
  else if (e === null) {
    var o = t.stateNode, s = t.memoizedProps;
    o.props = s;
    var c = o.context, d = n.contextType;
    typeof d == "object" && d !== null ? d = ht(d) : (d = Ze(n) ? kn : Ve.current, d = er(t, d));
    var j = n.getDerivedStateFromProps, u = typeof j == "function" || typeof o.getSnapshotBeforeUpdate == "function";
    u || typeof o.UNSAFE_componentWillReceiveProps != "function" && typeof o.componentWillReceiveProps != "function" || (s !== r || c !== d) && Ws(t, o, r, d), qt = !1;
    var m = t.memoizedState;
    o.state = m, Ga(t, r, o, a), c = t.memoizedState, s !== r || m !== c || Xe.current || qt ? (typeof j == "function" && (Ri(t, n, j, r), c = t.memoizedState), (s = qt || Hs(t, n, s, r, m, c, d)) ? (u || typeof o.UNSAFE_componentWillMount != "function" && typeof o.componentWillMount != "function" || (typeof o.componentWillMount == "function" && o.componentWillMount(), typeof o.UNSAFE_componentWillMount == "function" && o.UNSAFE_componentWillMount()), typeof o.componentDidMount == "function" && (t.flags |= 4194308)) : (typeof o.componentDidMount == "function" && (t.flags |= 4194308), t.memoizedProps = r, t.memoizedState = c), o.props = r, o.state = c, o.context = d, r = s) : (typeof o.componentDidMount == "function" && (t.flags |= 4194308), r = !1);
  } else {
    o = t.stateNode, Ru(e, t), s = t.memoizedProps, d = t.type === t.elementType ? s : yt(t.type, s), o.props = d, u = t.pendingProps, m = o.context, c = n.contextType, typeof c == "object" && c !== null ? c = ht(c) : (c = Ze(n) ? kn : Ve.current, c = er(t, c));
    var h = n.getDerivedStateFromProps;
    (j = typeof h == "function" || typeof o.getSnapshotBeforeUpdate == "function") || typeof o.UNSAFE_componentWillReceiveProps != "function" && typeof o.componentWillReceiveProps != "function" || (s !== u || m !== c) && Ws(t, o, r, c), qt = !1, m = t.memoizedState, o.state = m, Ga(t, r, o, a);
    var g = t.memoizedState;
    s !== u || m !== g || Xe.current || qt ? (typeof h == "function" && (Ri(t, n, h, r), g = t.memoizedState), (d = qt || Hs(t, n, d, r, m, g, c) || !1) ? (j || typeof o.UNSAFE_componentWillUpdate != "function" && typeof o.componentWillUpdate != "function" || (typeof o.componentWillUpdate == "function" && o.componentWillUpdate(r, g, c), typeof o.UNSAFE_componentWillUpdate == "function" && o.UNSAFE_componentWillUpdate(r, g, c)), typeof o.componentDidUpdate == "function" && (t.flags |= 4), typeof o.getSnapshotBeforeUpdate == "function" && (t.flags |= 1024)) : (typeof o.componentDidUpdate != "function" || s === e.memoizedProps && m === e.memoizedState || (t.flags |= 4), typeof o.getSnapshotBeforeUpdate != "function" || s === e.memoizedProps && m === e.memoizedState || (t.flags |= 1024), t.memoizedProps = r, t.memoizedState = g), o.props = r, o.state = g, o.context = c, r = d) : (typeof o.componentDidUpdate != "function" || s === e.memoizedProps && m === e.memoizedState || (t.flags |= 4), typeof o.getSnapshotBeforeUpdate != "function" || s === e.memoizedProps && m === e.memoizedState || (t.flags |= 1024), r = !1);
  }
  return zi(e, t, n, r, i, a);
}
function zi(e, t, n, r, a, i) {
  nd(e, t);
  var o = (t.flags & 128) !== 0;
  if (!r && !o) return a && As(t, n, !1), Ht(e, t, i);
  r = t.stateNode, Hp.current = t;
  var s = o && typeof n.getDerivedStateFromError != "function" ? null : r.render();
  return t.flags |= 1, e !== null && o ? (t.child = nr(t, e.child, null, i), t.child = nr(t, null, s, i)) : He(e, t, s, i), t.memoizedState = r.state, a && As(t, n, !0), t.child;
}
function rd(e) {
  var t = e.stateNode;
  t.pendingContext ? Ls(e, t.pendingContext, t.pendingContext !== t.context) : t.context && Ls(e, t.context, !1), So(e, t.containerInfo);
}
function Zs(e, t, n, r, a) {
  return tr(), vo(a), t.flags |= 256, He(e, t, n, r), t.child;
}
var Li = { dehydrated: null, treeContext: null, retryLane: 0 };
function Ai(e) {
  return { baseLanes: e, cachePool: null, transitions: null };
}
function ad(e, t, n) {
  var r = t.pendingProps, a = ue.current, i = !1, o = (t.flags & 128) !== 0, s;
  if ((s = o) || (s = e !== null && e.memoizedState === null ? !1 : (a & 2) !== 0), s ? (i = !0, t.flags &= -129) : (e === null || e.memoizedState !== null) && (a |= 1), ae(ue, a & 1), e === null)
    return Pi(t), e = t.memoizedState, e !== null && (e = e.dehydrated, e !== null) ? (t.mode & 1 ? e.data === "$!" ? t.lanes = 8 : t.lanes = 1073741824 : t.lanes = 1, null) : (o = r.children, e = r.fallback, i ? (r = t.mode, i = t.child, o = { mode: "hidden", children: o }, !(r & 1) && i !== null ? (i.childLanes = 0, i.pendingProps = o) : i = pl(o, r, 0, null), e = wn(e, r, n, null), i.return = t, e.return = t, i.sibling = e, t.child = i, t.child.memoizedState = Ai(n), t.memoizedState = Li, e) : Ro(t, o));
  if (a = e.memoizedState, a !== null && (s = a.dehydrated, s !== null)) return Wp(e, t, o, r, s, a, n);
  if (i) {
    i = r.fallback, o = t.mode, a = e.child, s = a.sibling;
    var c = { mode: "hidden", children: r.children };
    return !(o & 1) && t.child !== a ? (r = t.child, r.childLanes = 0, r.pendingProps = c, t.deletions = null) : (r = cn(a, c), r.subtreeFlags = a.subtreeFlags & 14680064), s !== null ? i = cn(s, i) : (i = wn(i, o, n, null), i.flags |= 2), i.return = t, r.return = t, r.sibling = i, t.child = r, r = i, i = t.child, o = e.child.memoizedState, o = o === null ? Ai(n) : { baseLanes: o.baseLanes | n, cachePool: null, transitions: o.transitions }, i.memoizedState = o, i.childLanes = e.childLanes & ~n, t.memoizedState = Li, r;
  }
  return i = e.child, e = i.sibling, r = cn(i, { mode: "visible", children: r.children }), !(t.mode & 1) && (r.lanes = n), r.return = t, r.sibling = null, e !== null && (n = t.deletions, n === null ? (t.deletions = [e], t.flags |= 16) : n.push(e)), t.child = r, t.memoizedState = null, r;
}
function Ro(e, t) {
  return t = pl({ mode: "visible", children: t }, e.mode, 0, null), t.return = e, e.child = t;
}
function va(e, t, n, r) {
  return r !== null && vo(r), nr(t, e.child, null, n), e = Ro(t, t.pendingProps.children), e.flags |= 2, t.memoizedState = null, e;
}
function Wp(e, t, n, r, a, i, o) {
  if (n)
    return t.flags & 256 ? (t.flags &= -257, r = Gl(Error(I(422))), va(e, t, o, r)) : t.memoizedState !== null ? (t.child = e.child, t.flags |= 128, null) : (i = r.fallback, a = t.mode, r = pl({ mode: "visible", children: r.children }, a, 0, null), i = wn(i, a, o, null), i.flags |= 2, r.return = t, i.return = t, r.sibling = i, t.child = r, t.mode & 1 && nr(t, e.child, null, o), t.child.memoizedState = Ai(o), t.memoizedState = Li, i);
  if (!(t.mode & 1)) return va(e, t, o, null);
  if (a.data === "$!") {
    if (r = a.nextSibling && a.nextSibling.dataset, r) var s = r.dgst;
    return r = s, i = Error(I(419)), r = Gl(i, r, void 0), va(e, t, o, r);
  }
  if (s = (o & e.childLanes) !== 0, Ye || s) {
    if (r = Re, r !== null) {
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
    return Ao(), r = Gl(Error(I(421))), va(e, t, o, r);
  }
  return a.data === "$?" ? (t.flags |= 128, t.child = e.child, t = am.bind(null, e), a._reactRetry = t, null) : (e = i.treeContext, nt = an(a.nextSibling), rt = t, se = !0, Nt = null, e !== null && (ut[dt++] = $t, ut[dt++] = Ot, ut[dt++] = En, $t = e.id, Ot = e.overflow, En = t), t = Ro(t, r.children), t.flags |= 4096, t);
}
function Js(e, t, n) {
  e.lanes |= t;
  var r = e.alternate;
  r !== null && (r.lanes |= t), Fi(e.return, t, n);
}
function Kl(e, t, n, r, a) {
  var i = e.memoizedState;
  i === null ? e.memoizedState = { isBackwards: t, rendering: null, renderingStartTime: 0, last: r, tail: n, tailMode: a } : (i.isBackwards = t, i.rendering = null, i.renderingStartTime = 0, i.last = r, i.tail = n, i.tailMode = a);
}
function ld(e, t, n) {
  var r = t.pendingProps, a = r.revealOrder, i = r.tail;
  if (He(e, t, r.children, n), r = ue.current, r & 2) r = r & 1 | 2, t.flags |= 128;
  else {
    if (e !== null && e.flags & 128) e: for (e = t.child; e !== null; ) {
      if (e.tag === 13) e.memoizedState !== null && Js(e, n, t);
      else if (e.tag === 19) Js(e, n, t);
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
      for (n = t.child, a = null; n !== null; ) e = n.alternate, e !== null && Ka(e) === null && (a = n), n = n.sibling;
      n = a, n === null ? (a = t.child, t.child = null) : (a = n.sibling, n.sibling = null), Kl(t, !1, a, n, i);
      break;
    case "backwards":
      for (n = null, a = t.child, t.child = null; a !== null; ) {
        if (e = a.alternate, e !== null && Ka(e) === null) {
          t.child = a;
          break;
        }
        e = a.sibling, a.sibling = n, n = a, a = e;
      }
      Kl(t, !0, n, null, i);
      break;
    case "together":
      Kl(t, !1, null, null, void 0);
      break;
    default:
      t.memoizedState = null;
  }
  return t.child;
}
function Pa(e, t) {
  !(t.mode & 1) && e !== null && (e.alternate = null, t.alternate = null, t.flags |= 2);
}
function Ht(e, t, n) {
  if (e !== null && (t.dependencies = e.dependencies), Pn |= t.lanes, !(n & t.childLanes)) return null;
  if (e !== null && t.child !== e.child) throw Error(I(153));
  if (t.child !== null) {
    for (e = t.child, n = cn(e, e.pendingProps), t.child = n, n.return = t; e.sibling !== null; ) e = e.sibling, n = n.sibling = cn(e, e.pendingProps), n.return = t;
    n.sibling = null;
  }
  return t.child;
}
function Qp(e, t, n) {
  switch (t.tag) {
    case 3:
      rd(t), tr();
      break;
    case 5:
      _u(t);
      break;
    case 1:
      Ze(t.type) && Va(t);
      break;
    case 4:
      So(t, t.stateNode.containerInfo);
      break;
    case 10:
      var r = t.type._context, a = t.memoizedProps.value;
      ae(Wa, r._currentValue), r._currentValue = a;
      break;
    case 13:
      if (r = t.memoizedState, r !== null)
        return r.dehydrated !== null ? (ae(ue, ue.current & 1), t.flags |= 128, null) : n & t.child.childLanes ? ad(e, t, n) : (ae(ue, ue.current & 1), e = Ht(e, t, n), e !== null ? e.sibling : null);
      ae(ue, ue.current & 1);
      break;
    case 19:
      if (r = (n & t.childLanes) !== 0, e.flags & 128) {
        if (r) return ld(e, t, n);
        t.flags |= 128;
      }
      if (a = t.memoizedState, a !== null && (a.rendering = null, a.tail = null, a.lastEffect = null), ae(ue, ue.current), r) break;
      return null;
    case 22:
    case 23:
      return t.lanes = 0, td(e, t, n);
  }
  return Ht(e, t, n);
}
var id, Mi, od, sd;
id = function(e, t) {
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
Mi = function() {
};
od = function(e, t, n, r) {
  var a = e.memoizedProps;
  if (a !== r) {
    e = t.stateNode, Nn(Tt.current);
    var i = null;
    switch (n) {
      case "input":
        a = li(e, a), r = li(e, r), i = [];
        break;
      case "select":
        a = pe({}, a, { value: void 0 }), r = pe({}, r, { value: void 0 }), i = [];
        break;
      case "textarea":
        a = si(e, a), r = si(e, r), i = [];
        break;
      default:
        typeof a.onClick != "function" && typeof r.onClick == "function" && (e.onclick = ba);
    }
    ui(n, r);
    var o;
    n = null;
    for (d in a) if (!r.hasOwnProperty(d) && a.hasOwnProperty(d) && a[d] != null) if (d === "style") {
      var s = a[d];
      for (o in s) s.hasOwnProperty(o) && (n || (n = {}), n[o] = "");
    } else d !== "dangerouslySetInnerHTML" && d !== "children" && d !== "suppressContentEditableWarning" && d !== "suppressHydrationWarning" && d !== "autoFocus" && (Tr.hasOwnProperty(d) ? i || (i = []) : (i = i || []).push(d, null));
    for (d in r) {
      var c = r[d];
      if (s = a != null ? a[d] : void 0, r.hasOwnProperty(d) && c !== s && (c != null || s != null)) if (d === "style") if (s) {
        for (o in s) !s.hasOwnProperty(o) || c && c.hasOwnProperty(o) || (n || (n = {}), n[o] = "");
        for (o in c) c.hasOwnProperty(o) && s[o] !== c[o] && (n || (n = {}), n[o] = c[o]);
      } else n || (i || (i = []), i.push(
        d,
        n
      )), n = c;
      else d === "dangerouslySetInnerHTML" ? (c = c ? c.__html : void 0, s = s ? s.__html : void 0, c != null && s !== c && (i = i || []).push(d, c)) : d === "children" ? typeof c != "string" && typeof c != "number" || (i = i || []).push(d, "" + c) : d !== "suppressContentEditableWarning" && d !== "suppressHydrationWarning" && (Tr.hasOwnProperty(d) ? (c != null && d === "onScroll" && ie("scroll", e), i || s === c || (i = [])) : (i = i || []).push(d, c));
    }
    n && (i = i || []).push("style", n);
    var d = i;
    (t.updateQueue = d) && (t.flags |= 4);
  }
};
sd = function(e, t, n, r) {
  n !== r && (t.flags |= 4);
};
function vr(e, t) {
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
function $e(e) {
  var t = e.alternate !== null && e.alternate.child === e.child, n = 0, r = 0;
  if (t) for (var a = e.child; a !== null; ) n |= a.lanes | a.childLanes, r |= a.subtreeFlags & 14680064, r |= a.flags & 14680064, a.return = e, a = a.sibling;
  else for (a = e.child; a !== null; ) n |= a.lanes | a.childLanes, r |= a.subtreeFlags, r |= a.flags, a.return = e, a = a.sibling;
  return e.subtreeFlags |= r, e.childLanes = n, t;
}
function Gp(e, t, n) {
  var r = t.pendingProps;
  switch (ho(t), t.tag) {
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
      return $e(t), null;
    case 1:
      return Ze(t.type) && Ua(), $e(t), null;
    case 3:
      return r = t.stateNode, rr(), oe(Xe), oe(Ve), Co(), r.pendingContext && (r.context = r.pendingContext, r.pendingContext = null), (e === null || e.child === null) && (ma(t) ? t.flags |= 4 : e === null || e.memoizedState.isDehydrated && !(t.flags & 256) || (t.flags |= 1024, Nt !== null && (Wi(Nt), Nt = null))), Mi(e, t), $e(t), null;
    case 5:
      wo(t);
      var a = Nn(Hr.current);
      if (n = t.type, e !== null && t.stateNode != null) od(e, t, n, r, a), e.ref !== t.ref && (t.flags |= 512, t.flags |= 2097152);
      else {
        if (!r) {
          if (t.stateNode === null) throw Error(I(166));
          return $e(t), null;
        }
        if (e = Nn(Tt.current), ma(t)) {
          r = t.stateNode, n = t.type;
          var i = t.memoizedProps;
          switch (r[Rt] = t, r[Vr] = i, e = (t.mode & 1) !== 0, n) {
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
              for (a = 0; a < Nr.length; a++) ie(Nr[a], r);
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
              ss(r, i), ie("invalid", r);
              break;
            case "select":
              r._wrapperState = { wasMultiple: !!i.multiple }, ie("invalid", r);
              break;
            case "textarea":
              us(r, i), ie("invalid", r);
          }
          ui(n, i), a = null;
          for (var o in i) if (i.hasOwnProperty(o)) {
            var s = i[o];
            o === "children" ? typeof s == "string" ? r.textContent !== s && (i.suppressHydrationWarning !== !0 && pa(r.textContent, s, e), a = ["children", s]) : typeof s == "number" && r.textContent !== "" + s && (i.suppressHydrationWarning !== !0 && pa(
              r.textContent,
              s,
              e
            ), a = ["children", "" + s]) : Tr.hasOwnProperty(o) && s != null && o === "onScroll" && ie("scroll", r);
          }
          switch (n) {
            case "input":
              la(r), cs(r, i, !0);
              break;
            case "textarea":
              la(r), ds(r);
              break;
            case "select":
            case "option":
              break;
            default:
              typeof i.onClick == "function" && (r.onclick = ba);
          }
          r = a, t.updateQueue = r, r !== null && (t.flags |= 4);
        } else {
          o = a.nodeType === 9 ? a : a.ownerDocument, e === "http://www.w3.org/1999/xhtml" && (e = Ac(n)), e === "http://www.w3.org/1999/xhtml" ? n === "script" ? (e = o.createElement("div"), e.innerHTML = "<script><\/script>", e = e.removeChild(e.firstChild)) : typeof r.is == "string" ? e = o.createElement(n, { is: r.is }) : (e = o.createElement(n), n === "select" && (o = e, r.multiple ? o.multiple = !0 : r.size && (o.size = r.size))) : e = o.createElementNS(e, n), e[Rt] = t, e[Vr] = r, id(e, t, !1, !1), t.stateNode = e;
          e: {
            switch (o = di(n, r), n) {
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
                for (a = 0; a < Nr.length; a++) ie(Nr[a], e);
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
                ss(e, r), a = li(e, r), ie("invalid", e);
                break;
              case "option":
                a = r;
                break;
              case "select":
                e._wrapperState = { wasMultiple: !!r.multiple }, a = pe({}, r, { value: void 0 }), ie("invalid", e);
                break;
              case "textarea":
                us(e, r), a = si(e, r), ie("invalid", e);
                break;
              default:
                a = r;
            }
            ui(n, a), s = a;
            for (i in s) if (s.hasOwnProperty(i)) {
              var c = s[i];
              i === "style" ? Oc(e, c) : i === "dangerouslySetInnerHTML" ? (c = c ? c.__html : void 0, c != null && Mc(e, c)) : i === "children" ? typeof c == "string" ? (n !== "textarea" || c !== "") && Dr(e, c) : typeof c == "number" && Dr(e, "" + c) : i !== "suppressContentEditableWarning" && i !== "suppressHydrationWarning" && i !== "autoFocus" && (Tr.hasOwnProperty(i) ? c != null && i === "onScroll" && ie("scroll", e) : c != null && Ji(e, i, c, o));
            }
            switch (n) {
              case "input":
                la(e), cs(e, r, !1);
                break;
              case "textarea":
                la(e), ds(e);
                break;
              case "option":
                r.value != null && e.setAttribute("value", "" + un(r.value));
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
                typeof a.onClick == "function" && (e.onclick = ba);
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
      return $e(t), null;
    case 6:
      if (e && t.stateNode != null) sd(e, t, e.memoizedProps, r);
      else {
        if (typeof r != "string" && t.stateNode === null) throw Error(I(166));
        if (n = Nn(Hr.current), Nn(Tt.current), ma(t)) {
          if (r = t.stateNode, n = t.memoizedProps, r[Rt] = t, (i = r.nodeValue !== n) && (e = rt, e !== null)) switch (e.tag) {
            case 3:
              pa(r.nodeValue, n, (e.mode & 1) !== 0);
              break;
            case 5:
              e.memoizedProps.suppressHydrationWarning !== !0 && pa(r.nodeValue, n, (e.mode & 1) !== 0);
          }
          i && (t.flags |= 4);
        } else r = (n.nodeType === 9 ? n : n.ownerDocument).createTextNode(r), r[Rt] = t, t.stateNode = r;
      }
      return $e(t), null;
    case 13:
      if (oe(ue), r = t.memoizedState, e === null || e.memoizedState !== null && e.memoizedState.dehydrated !== null) {
        if (se && nt !== null && t.mode & 1 && !(t.flags & 128)) Eu(), tr(), t.flags |= 98560, i = !1;
        else if (i = ma(t), r !== null && r.dehydrated !== null) {
          if (e === null) {
            if (!i) throw Error(I(318));
            if (i = t.memoizedState, i = i !== null ? i.dehydrated : null, !i) throw Error(I(317));
            i[Rt] = t;
          } else tr(), !(t.flags & 128) && (t.memoizedState = null), t.flags |= 4;
          $e(t), i = !1;
        } else Nt !== null && (Wi(Nt), Nt = null), i = !0;
        if (!i) return t.flags & 65536 ? t : null;
      }
      return t.flags & 128 ? (t.lanes = n, t) : (r = r !== null, r !== (e !== null && e.memoizedState !== null) && r && (t.child.flags |= 8192, t.mode & 1 && (e === null || ue.current & 1 ? Ie === 0 && (Ie = 3) : Ao())), t.updateQueue !== null && (t.flags |= 4), $e(t), null);
    case 4:
      return rr(), Mi(e, t), e === null && br(t.stateNode.containerInfo), $e(t), null;
    case 10:
      return yo(t.type._context), $e(t), null;
    case 17:
      return Ze(t.type) && Ua(), $e(t), null;
    case 19:
      if (oe(ue), i = t.memoizedState, i === null) return $e(t), null;
      if (r = (t.flags & 128) !== 0, o = i.rendering, o === null) if (r) vr(i, !1);
      else {
        if (Ie !== 0 || e !== null && e.flags & 128) for (e = t.child; e !== null; ) {
          if (o = Ka(e), o !== null) {
            for (t.flags |= 128, vr(i, !1), r = o.updateQueue, r !== null && (t.updateQueue = r, t.flags |= 4), t.subtreeFlags = 0, r = n, n = t.child; n !== null; ) i = n, e = r, i.flags &= 14680066, o = i.alternate, o === null ? (i.childLanes = 0, i.lanes = e, i.child = null, i.subtreeFlags = 0, i.memoizedProps = null, i.memoizedState = null, i.updateQueue = null, i.dependencies = null, i.stateNode = null) : (i.childLanes = o.childLanes, i.lanes = o.lanes, i.child = o.child, i.subtreeFlags = 0, i.deletions = null, i.memoizedProps = o.memoizedProps, i.memoizedState = o.memoizedState, i.updateQueue = o.updateQueue, i.type = o.type, e = o.dependencies, i.dependencies = e === null ? null : { lanes: e.lanes, firstContext: e.firstContext }), n = n.sibling;
            return ae(ue, ue.current & 1 | 2), t.child;
          }
          e = e.sibling;
        }
        i.tail !== null && ye() > lr && (t.flags |= 128, r = !0, vr(i, !1), t.lanes = 4194304);
      }
      else {
        if (!r) if (e = Ka(o), e !== null) {
          if (t.flags |= 128, r = !0, n = e.updateQueue, n !== null && (t.updateQueue = n, t.flags |= 4), vr(i, !0), i.tail === null && i.tailMode === "hidden" && !o.alternate && !se) return $e(t), null;
        } else 2 * ye() - i.renderingStartTime > lr && n !== 1073741824 && (t.flags |= 128, r = !0, vr(i, !1), t.lanes = 4194304);
        i.isBackwards ? (o.sibling = t.child, t.child = o) : (n = i.last, n !== null ? n.sibling = o : t.child = o, i.last = o);
      }
      return i.tail !== null ? (t = i.tail, i.rendering = t, i.tail = t.sibling, i.renderingStartTime = ye(), t.sibling = null, n = ue.current, ae(ue, r ? n & 1 | 2 : n & 1), t) : ($e(t), null);
    case 22:
    case 23:
      return Lo(), r = t.memoizedState !== null, e !== null && e.memoizedState !== null !== r && (t.flags |= 8192), r && t.mode & 1 ? tt & 1073741824 && ($e(t), t.subtreeFlags & 6 && (t.flags |= 8192)) : $e(t), null;
    case 24:
      return null;
    case 25:
      return null;
  }
  throw Error(I(156, t.tag));
}
function Kp(e, t) {
  switch (ho(t), t.tag) {
    case 1:
      return Ze(t.type) && Ua(), e = t.flags, e & 65536 ? (t.flags = e & -65537 | 128, t) : null;
    case 3:
      return rr(), oe(Xe), oe(Ve), Co(), e = t.flags, e & 65536 && !(e & 128) ? (t.flags = e & -65537 | 128, t) : null;
    case 5:
      return wo(t), null;
    case 13:
      if (oe(ue), e = t.memoizedState, e !== null && e.dehydrated !== null) {
        if (t.alternate === null) throw Error(I(340));
        tr();
      }
      return e = t.flags, e & 65536 ? (t.flags = e & -65537 | 128, t) : null;
    case 19:
      return oe(ue), null;
    case 4:
      return rr(), null;
    case 10:
      return yo(t.type._context), null;
    case 22:
    case 23:
      return Lo(), null;
    case 24:
      return null;
    default:
      return null;
  }
}
var xa = !1, Oe = !1, qp = typeof WeakSet == "function" ? WeakSet : Set, O = null;
function Hn(e, t) {
  var n = e.ref;
  if (n !== null) if (typeof n == "function") try {
    n(null);
  } catch (r) {
    xe(e, t, r);
  }
  else n.current = null;
}
function $i(e, t, n) {
  try {
    n();
  } catch (r) {
    xe(e, t, r);
  }
}
var ec = !1;
function Yp(e, t) {
  if (Ni = Ma, e = pu(), po(e)) {
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
  for (Si = { focusedElem: e, selectionRange: n }, Ma = !1, O = t; O !== null; ) if (t = O, e = t.child, (t.subtreeFlags & 1028) !== 0 && e !== null) e.return = t, O = e;
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
      xe(t, t.return, k);
    }
    if (e = t.sibling, e !== null) {
      e.return = t.return, O = e;
      break;
    }
    O = t.return;
  }
  return g = ec, ec = !1, g;
}
function Pr(e, t, n) {
  var r = t.updateQueue;
  if (r = r !== null ? r.lastEffect : null, r !== null) {
    var a = r = r.next;
    do {
      if ((a.tag & e) === e) {
        var i = a.destroy;
        a.destroy = void 0, i !== void 0 && $i(t, n, i);
      }
      a = a.next;
    } while (a !== r);
  }
}
function dl(e, t) {
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
function Oi(e) {
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
function cd(e) {
  var t = e.alternate;
  t !== null && (e.alternate = null, cd(t)), e.child = null, e.deletions = null, e.sibling = null, e.tag === 5 && (t = e.stateNode, t !== null && (delete t[Rt], delete t[Vr], delete t[ki], delete t[Tp], delete t[Dp])), e.stateNode = null, e.return = null, e.dependencies = null, e.memoizedProps = null, e.memoizedState = null, e.pendingProps = null, e.stateNode = null, e.updateQueue = null;
}
function ud(e) {
  return e.tag === 5 || e.tag === 3 || e.tag === 4;
}
function tc(e) {
  e: for (; ; ) {
    for (; e.sibling === null; ) {
      if (e.return === null || ud(e.return)) return null;
      e = e.return;
    }
    for (e.sibling.return = e.return, e = e.sibling; e.tag !== 5 && e.tag !== 6 && e.tag !== 18; ) {
      if (e.flags & 2 || e.child === null || e.tag === 4) continue e;
      e.child.return = e, e = e.child;
    }
    if (!(e.flags & 2)) return e.stateNode;
  }
}
function bi(e, t, n) {
  var r = e.tag;
  if (r === 5 || r === 6) e = e.stateNode, t ? n.nodeType === 8 ? n.parentNode.insertBefore(e, t) : n.insertBefore(e, t) : (n.nodeType === 8 ? (t = n.parentNode, t.insertBefore(e, n)) : (t = n, t.appendChild(e)), n = n._reactRootContainer, n != null || t.onclick !== null || (t.onclick = ba));
  else if (r !== 4 && (e = e.child, e !== null)) for (bi(e, t, n), e = e.sibling; e !== null; ) bi(e, t, n), e = e.sibling;
}
function Ui(e, t, n) {
  var r = e.tag;
  if (r === 5 || r === 6) e = e.stateNode, t ? n.insertBefore(e, t) : n.appendChild(e);
  else if (r !== 4 && (e = e.child, e !== null)) for (Ui(e, t, n), e = e.sibling; e !== null; ) Ui(e, t, n), e = e.sibling;
}
var De = null, jt = !1;
function Gt(e, t, n) {
  for (n = n.child; n !== null; ) dd(e, t, n), n = n.sibling;
}
function dd(e, t, n) {
  if (_t && typeof _t.onCommitFiberUnmount == "function") try {
    _t.onCommitFiberUnmount(rl, n);
  } catch {
  }
  switch (n.tag) {
    case 5:
      Oe || Hn(n, t);
    case 6:
      var r = De, a = jt;
      De = null, Gt(e, t, n), De = r, jt = a, De !== null && (jt ? (e = De, n = n.stateNode, e.nodeType === 8 ? e.parentNode.removeChild(n) : e.removeChild(n)) : De.removeChild(n.stateNode));
      break;
    case 18:
      De !== null && (jt ? (e = De, n = n.stateNode, e.nodeType === 8 ? Ul(e.parentNode, n) : e.nodeType === 1 && Ul(e, n), Mr(e)) : Ul(De, n.stateNode));
      break;
    case 4:
      r = De, a = jt, De = n.stateNode.containerInfo, jt = !0, Gt(e, t, n), De = r, jt = a;
      break;
    case 0:
    case 11:
    case 14:
    case 15:
      if (!Oe && (r = n.updateQueue, r !== null && (r = r.lastEffect, r !== null))) {
        a = r = r.next;
        do {
          var i = a, o = i.destroy;
          i = i.tag, o !== void 0 && (i & 2 || i & 4) && $i(n, t, o), a = a.next;
        } while (a !== r);
      }
      Gt(e, t, n);
      break;
    case 1:
      if (!Oe && (Hn(n, t), r = n.stateNode, typeof r.componentWillUnmount == "function")) try {
        r.props = n.memoizedProps, r.state = n.memoizedState, r.componentWillUnmount();
      } catch (s) {
        xe(n, t, s);
      }
      Gt(e, t, n);
      break;
    case 21:
      Gt(e, t, n);
      break;
    case 22:
      n.mode & 1 ? (Oe = (r = Oe) || n.memoizedState !== null, Gt(e, t, n), Oe = r) : Gt(e, t, n);
      break;
    default:
      Gt(e, t, n);
  }
}
function nc(e) {
  var t = e.updateQueue;
  if (t !== null) {
    e.updateQueue = null;
    var n = e.stateNode;
    n === null && (n = e.stateNode = new qp()), t.forEach(function(r) {
      var a = lm.bind(null, e, r);
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
            De = s.stateNode, jt = !1;
            break e;
          case 3:
            De = s.stateNode.containerInfo, jt = !0;
            break e;
          case 4:
            De = s.stateNode.containerInfo, jt = !0;
            break e;
        }
        s = s.return;
      }
      if (De === null) throw Error(I(160));
      dd(i, o, a), De = null, jt = !1;
      var c = a.alternate;
      c !== null && (c.return = null), a.return = null;
    } catch (d) {
      xe(a, t, d);
    }
  }
  if (t.subtreeFlags & 12854) for (t = t.child; t !== null; ) fd(t, e), t = t.sibling;
}
function fd(e, t) {
  var n = e.alternate, r = e.flags;
  switch (e.tag) {
    case 0:
    case 11:
    case 14:
    case 15:
      if (gt(t, e), Pt(e), r & 4) {
        try {
          Pr(3, e, e.return), dl(3, e);
        } catch (w) {
          xe(e, e.return, w);
        }
        try {
          Pr(5, e, e.return);
        } catch (w) {
          xe(e, e.return, w);
        }
      }
      break;
    case 1:
      gt(t, e), Pt(e), r & 512 && n !== null && Hn(n, n.return);
      break;
    case 5:
      if (gt(t, e), Pt(e), r & 512 && n !== null && Hn(n, n.return), e.flags & 32) {
        var a = e.stateNode;
        try {
          Dr(a, "");
        } catch (w) {
          xe(e, e.return, w);
        }
      }
      if (r & 4 && (a = e.stateNode, a != null)) {
        var i = e.memoizedProps, o = n !== null ? n.memoizedProps : i, s = e.type, c = e.updateQueue;
        if (e.updateQueue = null, c !== null) try {
          s === "input" && i.type === "radio" && i.name != null && zc(a, i), di(s, o);
          var d = di(s, i);
          for (o = 0; o < c.length; o += 2) {
            var j = c[o], u = c[o + 1];
            j === "style" ? Oc(a, u) : j === "dangerouslySetInnerHTML" ? Mc(a, u) : j === "children" ? Dr(a, u) : Ji(a, j, u, d);
          }
          switch (s) {
            case "input":
              ii(a, i);
              break;
            case "textarea":
              Lc(a, i);
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
          a[Vr] = i;
        } catch (w) {
          xe(e, e.return, w);
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
          xe(e, e.return, w);
        }
      }
      break;
    case 3:
      if (gt(t, e), Pt(e), r & 4 && n !== null && n.memoizedState.isDehydrated) try {
        Mr(t.containerInfo);
      } catch (w) {
        xe(e, e.return, w);
      }
      break;
    case 4:
      gt(t, e), Pt(e);
      break;
    case 13:
      gt(t, e), Pt(e), a = e.child, a.flags & 8192 && (i = a.memoizedState !== null, a.stateNode.isHidden = i, !i || a.alternate !== null && a.alternate.memoizedState !== null || (Do = ye())), r & 4 && nc(e);
      break;
    case 22:
      if (j = n !== null && n.memoizedState !== null, e.mode & 1 ? (Oe = (d = Oe) || j, gt(t, e), Oe = d) : gt(t, e), Pt(e), r & 8192) {
        if (d = e.memoizedState !== null, (e.stateNode.isHidden = d) && !j && e.mode & 1) for (O = e, j = e.child; j !== null; ) {
          for (u = O = j; O !== null; ) {
            switch (m = O, h = m.child, m.tag) {
              case 0:
              case 11:
              case 14:
              case 15:
                Pr(4, m, m.return);
                break;
              case 1:
                Hn(m, m.return);
                var g = m.stateNode;
                if (typeof g.componentWillUnmount == "function") {
                  r = m, n = m.return;
                  try {
                    t = r, g.props = t.memoizedProps, g.state = t.memoizedState, g.componentWillUnmount();
                  } catch (w) {
                    xe(r, n, w);
                  }
                }
                break;
              case 5:
                Hn(m, m.return);
                break;
              case 22:
                if (m.memoizedState !== null) {
                  ac(u);
                  continue;
                }
            }
            h !== null ? (h.return = m, O = h) : ac(u);
          }
          j = j.sibling;
        }
        e: for (j = null, u = e; ; ) {
          if (u.tag === 5) {
            if (j === null) {
              j = u;
              try {
                a = u.stateNode, d ? (i = a.style, typeof i.setProperty == "function" ? i.setProperty("display", "none", "important") : i.display = "none") : (s = u.stateNode, c = u.memoizedProps.style, o = c != null && c.hasOwnProperty("display") ? c.display : null, s.style.display = $c("display", o));
              } catch (w) {
                xe(e, e.return, w);
              }
            }
          } else if (u.tag === 6) {
            if (j === null) try {
              u.stateNode.nodeValue = d ? "" : u.memoizedProps;
            } catch (w) {
              xe(e, e.return, w);
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
      gt(t, e), Pt(e), r & 4 && nc(e);
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
          if (ud(n)) {
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
          r.flags & 32 && (Dr(a, ""), r.flags &= -33);
          var i = tc(e);
          Ui(e, i, a);
          break;
        case 3:
        case 4:
          var o = r.stateNode.containerInfo, s = tc(e);
          bi(e, s, o);
          break;
        default:
          throw Error(I(161));
      }
    } catch (c) {
      xe(e, e.return, c);
    }
    e.flags &= -3;
  }
  t & 4096 && (e.flags &= -4097);
}
function Xp(e, t, n) {
  O = e, pd(e);
}
function pd(e, t, n) {
  for (var r = (e.mode & 1) !== 0; O !== null; ) {
    var a = O, i = a.child;
    if (a.tag === 22 && r) {
      var o = a.memoizedState !== null || xa;
      if (!o) {
        var s = a.alternate, c = s !== null && s.memoizedState !== null || Oe;
        s = xa;
        var d = Oe;
        if (xa = o, (Oe = c) && !d) for (O = a; O !== null; ) o = O, c = o.child, o.tag === 22 && o.memoizedState !== null ? lc(a) : c !== null ? (c.return = o, O = c) : lc(a);
        for (; i !== null; ) O = i, pd(i), i = i.sibling;
        O = a, xa = s, Oe = d;
      }
      rc(e);
    } else a.subtreeFlags & 8772 && i !== null ? (i.return = a, O = i) : rc(e);
  }
}
function rc(e) {
  for (; O !== null; ) {
    var t = O;
    if (t.flags & 8772) {
      var n = t.alternate;
      try {
        if (t.flags & 8772) switch (t.tag) {
          case 0:
          case 11:
          case 15:
            Oe || dl(5, t);
            break;
          case 1:
            var r = t.stateNode;
            if (t.flags & 4 && !Oe) if (n === null) r.componentDidMount();
            else {
              var a = t.elementType === t.type ? n.memoizedProps : yt(t.type, n.memoizedProps);
              r.componentDidUpdate(a, n.memoizedState, r.__reactInternalSnapshotBeforeUpdate);
            }
            var i = t.updateQueue;
            i !== null && Us(t, i, r);
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
              Us(t, o, n);
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
                  u !== null && Mr(u);
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
        Oe || t.flags & 512 && Oi(t);
      } catch (m) {
        xe(t, t.return, m);
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
function ac(e) {
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
function lc(e) {
  for (; O !== null; ) {
    var t = O;
    try {
      switch (t.tag) {
        case 0:
        case 11:
        case 15:
          var n = t.return;
          try {
            dl(4, t);
          } catch (c) {
            xe(t, n, c);
          }
          break;
        case 1:
          var r = t.stateNode;
          if (typeof r.componentDidMount == "function") {
            var a = t.return;
            try {
              r.componentDidMount();
            } catch (c) {
              xe(t, a, c);
            }
          }
          var i = t.return;
          try {
            Oi(t);
          } catch (c) {
            xe(t, i, c);
          }
          break;
        case 5:
          var o = t.return;
          try {
            Oi(t);
          } catch (c) {
            xe(t, o, c);
          }
      }
    } catch (c) {
      xe(t, t.return, c);
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
var Zp = Math.ceil, Xa = Wt.ReactCurrentDispatcher, _o = Wt.ReactCurrentOwner, mt = Wt.ReactCurrentBatchConfig, J = 0, Re = null, Ne = null, ze = 0, tt = 0, Wn = mn(0), Ie = 0, Kr = null, Pn = 0, fl = 0, To = 0, Fr = null, qe = null, Do = 0, lr = 1 / 0, At = null, Za = !1, Vi = null, on = null, ga = !1, en = null, Ja = 0, Rr = 0, Bi = null, Fa = -1, Ra = 0;
function Qe() {
  return J & 6 ? ye() : Fa !== -1 ? Fa : Fa = ye();
}
function sn(e) {
  return e.mode & 1 ? J & 2 && ze !== 0 ? ze & -ze : Lp.transition !== null ? (Ra === 0 && (Ra = Xc()), Ra) : (e = ne, e !== 0 || (e = window.event, e = e === void 0 ? 16 : au(e.type)), e) : 1;
}
function wt(e, t, n, r) {
  if (50 < Rr) throw Rr = 0, Bi = null, Error(I(185));
  Xr(e, n, r), (!(J & 2) || e !== Re) && (e === Re && (!(J & 2) && (fl |= n), Ie === 4 && Xt(e, ze)), Je(e, r), n === 1 && J === 0 && !(t.mode & 1) && (lr = ye() + 500, sl && hn()));
}
function Je(e, t) {
  var n = e.callbackNode;
  Lf(e, t);
  var r = Aa(e, e === Re ? ze : 0);
  if (r === 0) n !== null && ms(n), e.callbackNode = null, e.callbackPriority = 0;
  else if (t = r & -r, e.callbackPriority !== t) {
    if (n != null && ms(n), t === 1) e.tag === 0 ? zp(ic.bind(null, e)) : wu(ic.bind(null, e)), Rp(function() {
      !(J & 6) && hn();
    }), n = null;
    else {
      switch (Zc(r)) {
        case 1:
          n = ao;
          break;
        case 4:
          n = qc;
          break;
        case 16:
          n = La;
          break;
        case 536870912:
          n = Yc;
          break;
        default:
          n = La;
      }
      n = Nd(n, md.bind(null, e));
    }
    e.callbackPriority = t, e.callbackNode = n;
  }
}
function md(e, t) {
  if (Fa = -1, Ra = 0, J & 6) throw Error(I(327));
  var n = e.callbackNode;
  if (Xn() && e.callbackNode !== n) return null;
  var r = Aa(e, e === Re ? ze : 0);
  if (r === 0) return null;
  if (r & 30 || r & e.expiredLanes || t) t = el(e, r);
  else {
    t = r;
    var a = J;
    J |= 2;
    var i = vd();
    (Re !== e || ze !== t) && (At = null, lr = ye() + 500, Sn(e, t));
    do
      try {
        tm();
        break;
      } catch (s) {
        hd(e, s);
      }
    while (!0);
    go(), Xa.current = i, J = a, Ne !== null ? t = 0 : (Re = null, ze = 0, t = Ie);
  }
  if (t !== 0) {
    if (t === 2 && (a = vi(e), a !== 0 && (r = a, t = Hi(e, a))), t === 1) throw n = Kr, Sn(e, 0), Xt(e, r), Je(e, ye()), n;
    if (t === 6) Xt(e, r);
    else {
      if (a = e.current.alternate, !(r & 30) && !Jp(a) && (t = el(e, r), t === 2 && (i = vi(e), i !== 0 && (r = i, t = Hi(e, i))), t === 1)) throw n = Kr, Sn(e, 0), Xt(e, r), Je(e, ye()), n;
      switch (e.finishedWork = a, e.finishedLanes = r, t) {
        case 0:
        case 1:
          throw Error(I(345));
        case 2:
          gn(e, qe, At);
          break;
        case 3:
          if (Xt(e, r), (r & 130023424) === r && (t = Do + 500 - ye(), 10 < t)) {
            if (Aa(e, 0) !== 0) break;
            if (a = e.suspendedLanes, (a & r) !== r) {
              Qe(), e.pingedLanes |= e.suspendedLanes & a;
              break;
            }
            e.timeoutHandle = Ci(gn.bind(null, e, qe, At), t);
            break;
          }
          gn(e, qe, At);
          break;
        case 4:
          if (Xt(e, r), (r & 4194240) === r) break;
          for (t = e.eventTimes, a = -1; 0 < r; ) {
            var o = 31 - St(r);
            i = 1 << o, o = t[o], o > a && (a = o), r &= ~i;
          }
          if (r = a, r = ye() - r, r = (120 > r ? 120 : 480 > r ? 480 : 1080 > r ? 1080 : 1920 > r ? 1920 : 3e3 > r ? 3e3 : 4320 > r ? 4320 : 1960 * Zp(r / 1960)) - r, 10 < r) {
            e.timeoutHandle = Ci(gn.bind(null, e, qe, At), r);
            break;
          }
          gn(e, qe, At);
          break;
        case 5:
          gn(e, qe, At);
          break;
        default:
          throw Error(I(329));
      }
    }
  }
  return Je(e, ye()), e.callbackNode === n ? md.bind(null, e) : null;
}
function Hi(e, t) {
  var n = Fr;
  return e.current.memoizedState.isDehydrated && (Sn(e, t).flags |= 256), e = el(e, t), e !== 2 && (t = qe, qe = n, t !== null && Wi(t)), e;
}
function Wi(e) {
  qe === null ? qe = e : qe.push.apply(qe, e);
}
function Jp(e) {
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
function Xt(e, t) {
  for (t &= ~To, t &= ~fl, e.suspendedLanes |= t, e.pingedLanes &= ~t, e = e.expirationTimes; 0 < t; ) {
    var n = 31 - St(t), r = 1 << n;
    e[n] = -1, t &= ~r;
  }
}
function ic(e) {
  if (J & 6) throw Error(I(327));
  Xn();
  var t = Aa(e, 0);
  if (!(t & 1)) return Je(e, ye()), null;
  var n = el(e, t);
  if (e.tag !== 0 && n === 2) {
    var r = vi(e);
    r !== 0 && (t = r, n = Hi(e, r));
  }
  if (n === 1) throw n = Kr, Sn(e, 0), Xt(e, t), Je(e, ye()), n;
  if (n === 6) throw Error(I(345));
  return e.finishedWork = e.current.alternate, e.finishedLanes = t, gn(e, qe, At), Je(e, ye()), null;
}
function zo(e, t) {
  var n = J;
  J |= 1;
  try {
    return e(t);
  } finally {
    J = n, J === 0 && (lr = ye() + 500, sl && hn());
  }
}
function Fn(e) {
  en !== null && en.tag === 0 && !(J & 6) && Xn();
  var t = J;
  J |= 1;
  var n = mt.transition, r = ne;
  try {
    if (mt.transition = null, ne = 1, e) return e();
  } finally {
    ne = r, mt.transition = n, J = t, !(J & 6) && hn();
  }
}
function Lo() {
  tt = Wn.current, oe(Wn);
}
function Sn(e, t) {
  e.finishedWork = null, e.finishedLanes = 0;
  var n = e.timeoutHandle;
  if (n !== -1 && (e.timeoutHandle = -1, Fp(n)), Ne !== null) for (n = Ne.return; n !== null; ) {
    var r = n;
    switch (ho(r), r.tag) {
      case 1:
        r = r.type.childContextTypes, r != null && Ua();
        break;
      case 3:
        rr(), oe(Xe), oe(Ve), Co();
        break;
      case 5:
        wo(r);
        break;
      case 4:
        rr();
        break;
      case 13:
        oe(ue);
        break;
      case 19:
        oe(ue);
        break;
      case 10:
        yo(r.type._context);
        break;
      case 22:
      case 23:
        Lo();
    }
    n = n.return;
  }
  if (Re = e, Ne = e = cn(e.current, null), ze = tt = t, Ie = 0, Kr = null, To = fl = Pn = 0, qe = Fr = null, jn !== null) {
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
function hd(e, t) {
  do {
    var n = Ne;
    try {
      if (go(), Ea.current = Ya, qa) {
        for (var r = fe.memoizedState; r !== null; ) {
          var a = r.queue;
          a !== null && (a.pending = null), r = r.next;
        }
        qa = !1;
      }
      if (In = 0, Fe = ke = fe = null, Ir = !1, Wr = 0, _o.current = null, n === null || n.return === null) {
        Ie = 1, Kr = t, Ne = null;
        break;
      }
      e: {
        var i = e, o = n.return, s = n, c = t;
        if (t = ze, s.flags |= 32768, c !== null && typeof c == "object" && typeof c.then == "function") {
          var d = c, j = s, u = j.tag;
          if (!(j.mode & 1) && (u === 0 || u === 11 || u === 15)) {
            var m = j.alternate;
            m ? (j.updateQueue = m.updateQueue, j.memoizedState = m.memoizedState, j.lanes = m.lanes) : (j.updateQueue = null, j.memoizedState = null);
          }
          var h = Gs(o);
          if (h !== null) {
            h.flags &= -257, Ks(h, o, s, i, t), h.mode & 1 && Qs(i, d, t), t = h, c = d;
            var g = t.updateQueue;
            if (g === null) {
              var w = /* @__PURE__ */ new Set();
              w.add(c), t.updateQueue = w;
            } else g.add(c);
            break e;
          } else {
            if (!(t & 1)) {
              Qs(i, d, t), Ao();
              break e;
            }
            c = Error(I(426));
          }
        } else if (se && s.mode & 1) {
          var $ = Gs(o);
          if ($ !== null) {
            !($.flags & 65536) && ($.flags |= 256), Ks($, o, s, i, t), vo(ar(c, s));
            break e;
          }
        }
        i = c = ar(c, s), Ie !== 4 && (Ie = 2), Fr === null ? Fr = [i] : Fr.push(i), i = o;
        do {
          switch (i.tag) {
            case 3:
              i.flags |= 65536, t &= -t, i.lanes |= t;
              var p = Zu(i, c, t);
              bs(i, p);
              break e;
            case 1:
              s = c;
              var f = i.type, v = i.stateNode;
              if (!(i.flags & 128) && (typeof f.getDerivedStateFromError == "function" || v !== null && typeof v.componentDidCatch == "function" && (on === null || !on.has(v)))) {
                i.flags |= 65536, t &= -t, i.lanes |= t;
                var k = Ju(i, s, t);
                bs(i, k);
                break e;
              }
          }
          i = i.return;
        } while (i !== null);
      }
      gd(n);
    } catch (_) {
      t = _, Ne === n && n !== null && (Ne = n = n.return);
      continue;
    }
    break;
  } while (!0);
}
function vd() {
  var e = Xa.current;
  return Xa.current = Ya, e === null ? Ya : e;
}
function Ao() {
  (Ie === 0 || Ie === 3 || Ie === 2) && (Ie = 4), Re === null || !(Pn & 268435455) && !(fl & 268435455) || Xt(Re, ze);
}
function el(e, t) {
  var n = J;
  J |= 2;
  var r = vd();
  (Re !== e || ze !== t) && (At = null, Sn(e, t));
  do
    try {
      em();
      break;
    } catch (a) {
      hd(e, a);
    }
  while (!0);
  if (go(), J = n, Xa.current = r, Ne !== null) throw Error(I(261));
  return Re = null, ze = 0, Ie;
}
function em() {
  for (; Ne !== null; ) xd(Ne);
}
function tm() {
  for (; Ne !== null && !Ef(); ) xd(Ne);
}
function xd(e) {
  var t = jd(e.alternate, e, tt);
  e.memoizedProps = e.pendingProps, t === null ? gd(e) : Ne = t, _o.current = null;
}
function gd(e) {
  var t = e;
  do {
    var n = t.alternate;
    if (e = t.return, t.flags & 32768) {
      if (n = Kp(n, t), n !== null) {
        n.flags &= 32767, Ne = n;
        return;
      }
      if (e !== null) e.flags |= 32768, e.subtreeFlags = 0, e.deletions = null;
      else {
        Ie = 6, Ne = null;
        return;
      }
    } else if (n = Gp(n, t, tt), n !== null) {
      Ne = n;
      return;
    }
    if (t = t.sibling, t !== null) {
      Ne = t;
      return;
    }
    Ne = t = e;
  } while (t !== null);
  Ie === 0 && (Ie = 5);
}
function gn(e, t, n) {
  var r = ne, a = mt.transition;
  try {
    mt.transition = null, ne = 1, nm(e, t, n, r);
  } finally {
    mt.transition = a, ne = r;
  }
  return null;
}
function nm(e, t, n, r) {
  do
    Xn();
  while (en !== null);
  if (J & 6) throw Error(I(327));
  n = e.finishedWork;
  var a = e.finishedLanes;
  if (n === null) return null;
  if (e.finishedWork = null, e.finishedLanes = 0, n === e.current) throw Error(I(177));
  e.callbackNode = null, e.callbackPriority = 0;
  var i = n.lanes | n.childLanes;
  if (Af(e, i), e === Re && (Ne = Re = null, ze = 0), !(n.subtreeFlags & 2064) && !(n.flags & 2064) || ga || (ga = !0, Nd(La, function() {
    return Xn(), null;
  })), i = (n.flags & 15990) !== 0, n.subtreeFlags & 15990 || i) {
    i = mt.transition, mt.transition = null;
    var o = ne;
    ne = 1;
    var s = J;
    J |= 4, _o.current = null, Yp(e, n), fd(n, e), Sp(Si), Ma = !!Ni, Si = Ni = null, e.current = n, Xp(n), If(), J = s, ne = o, mt.transition = i;
  } else e.current = n;
  if (ga && (ga = !1, en = e, Ja = a), i = e.pendingLanes, i === 0 && (on = null), Rf(n.stateNode), Je(e, ye()), t !== null) for (r = e.onRecoverableError, n = 0; n < t.length; n++) a = t[n], r(a.value, { componentStack: a.stack, digest: a.digest });
  if (Za) throw Za = !1, e = Vi, Vi = null, e;
  return Ja & 1 && e.tag !== 0 && Xn(), i = e.pendingLanes, i & 1 ? e === Bi ? Rr++ : (Rr = 0, Bi = e) : Rr = 0, hn(), null;
}
function Xn() {
  if (en !== null) {
    var e = Zc(Ja), t = mt.transition, n = ne;
    try {
      if (mt.transition = null, ne = 16 > e ? 16 : e, en === null) var r = !1;
      else {
        if (e = en, en = null, Ja = 0, J & 6) throw Error(I(331));
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
                      Pr(8, j, i);
                  }
                  var u = j.child;
                  if (u !== null) u.return = j, O = u;
                  else for (; O !== null; ) {
                    j = O;
                    var m = j.sibling, h = j.return;
                    if (cd(j), j === d) {
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
                Pr(9, i, i.return);
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
                  dl(9, s);
              }
            } catch (_) {
              xe(s, s.return, _);
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
        if (J = a, hn(), _t && typeof _t.onPostCommitFiberRoot == "function") try {
          _t.onPostCommitFiberRoot(rl, e);
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
function oc(e, t, n) {
  t = ar(n, t), t = Zu(e, t, 1), e = ln(e, t, 1), t = Qe(), e !== null && (Xr(e, 1, t), Je(e, t));
}
function xe(e, t, n) {
  if (e.tag === 3) oc(e, e, n);
  else for (; t !== null; ) {
    if (t.tag === 3) {
      oc(t, e, n);
      break;
    } else if (t.tag === 1) {
      var r = t.stateNode;
      if (typeof t.type.getDerivedStateFromError == "function" || typeof r.componentDidCatch == "function" && (on === null || !on.has(r))) {
        e = ar(n, e), e = Ju(t, e, 1), t = ln(t, e, 1), e = Qe(), t !== null && (Xr(t, 1, e), Je(t, e));
        break;
      }
    }
    t = t.return;
  }
}
function rm(e, t, n) {
  var r = e.pingCache;
  r !== null && r.delete(t), t = Qe(), e.pingedLanes |= e.suspendedLanes & n, Re === e && (ze & n) === n && (Ie === 4 || Ie === 3 && (ze & 130023424) === ze && 500 > ye() - Do ? Sn(e, 0) : To |= n), Je(e, t);
}
function yd(e, t) {
  t === 0 && (e.mode & 1 ? (t = sa, sa <<= 1, !(sa & 130023424) && (sa = 4194304)) : t = 1);
  var n = Qe();
  e = Bt(e, t), e !== null && (Xr(e, t, n), Je(e, n));
}
function am(e) {
  var t = e.memoizedState, n = 0;
  t !== null && (n = t.retryLane), yd(e, n);
}
function lm(e, t) {
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
  r !== null && r.delete(t), yd(e, n);
}
var jd;
jd = function(e, t, n) {
  if (e !== null) if (e.memoizedProps !== t.pendingProps || Xe.current) Ye = !0;
  else {
    if (!(e.lanes & n) && !(t.flags & 128)) return Ye = !1, Qp(e, t, n);
    Ye = !!(e.flags & 131072);
  }
  else Ye = !1, se && t.flags & 1048576 && Cu(t, Ha, t.index);
  switch (t.lanes = 0, t.tag) {
    case 2:
      var r = t.type;
      Pa(e, t), e = t.pendingProps;
      var a = er(t, Ve.current);
      Yn(t, n), a = Eo(null, t, r, e, a, n);
      var i = Io();
      return t.flags |= 1, typeof a == "object" && a !== null && typeof a.render == "function" && a.$$typeof === void 0 ? (t.tag = 1, t.memoizedState = null, t.updateQueue = null, Ze(r) ? (i = !0, Va(t)) : i = !1, t.memoizedState = a.state !== null && a.state !== void 0 ? a.state : null, No(t), a.updater = ul, t.stateNode = a, a._reactInternals = t, _i(t, r, e, n), t = zi(null, t, r, !0, i, n)) : (t.tag = 0, se && i && mo(t), He(null, t, a, n), t = t.child), t;
    case 16:
      r = t.elementType;
      e: {
        switch (Pa(e, t), e = t.pendingProps, a = r._init, r = a(r._payload), t.type = r, a = t.tag = om(r), e = yt(r, e), a) {
          case 0:
            t = Di(null, t, r, e, n);
            break e;
          case 1:
            t = Xs(null, t, r, e, n);
            break e;
          case 11:
            t = qs(null, t, r, e, n);
            break e;
          case 14:
            t = Ys(null, t, r, yt(r.type, e), n);
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
      return r = t.type, a = t.pendingProps, a = t.elementType === r ? a : yt(r, a), Di(e, t, r, a, n);
    case 1:
      return r = t.type, a = t.pendingProps, a = t.elementType === r ? a : yt(r, a), Xs(e, t, r, a, n);
    case 3:
      e: {
        if (rd(t), e === null) throw Error(I(387));
        r = t.pendingProps, i = t.memoizedState, a = i.element, Ru(e, t), Ga(t, r, null, n);
        var o = t.memoizedState;
        if (r = o.element, i.isDehydrated) if (i = { element: r, isDehydrated: !1, cache: o.cache, pendingSuspenseBoundaries: o.pendingSuspenseBoundaries, transitions: o.transitions }, t.updateQueue.baseState = i, t.memoizedState = i, t.flags & 256) {
          a = ar(Error(I(423)), t), t = Zs(e, t, r, n, a);
          break e;
        } else if (r !== a) {
          a = ar(Error(I(424)), t), t = Zs(e, t, r, n, a);
          break e;
        } else for (nt = an(t.stateNode.containerInfo.firstChild), rt = t, se = !0, Nt = null, n = Pu(t, null, r, n), t.child = n; n; ) n.flags = n.flags & -3 | 4096, n = n.sibling;
        else {
          if (tr(), r === a) {
            t = Ht(e, t, n);
            break e;
          }
          He(e, t, r, n);
        }
        t = t.child;
      }
      return t;
    case 5:
      return _u(t), e === null && Pi(t), r = t.type, a = t.pendingProps, i = e !== null ? e.memoizedProps : null, o = a.children, wi(r, a) ? o = null : i !== null && wi(r, i) && (t.flags |= 32), nd(e, t), He(e, t, o, n), t.child;
    case 6:
      return e === null && Pi(t), null;
    case 13:
      return ad(e, t, n);
    case 4:
      return So(t, t.stateNode.containerInfo), r = t.pendingProps, e === null ? t.child = nr(t, null, r, n) : He(e, t, r, n), t.child;
    case 11:
      return r = t.type, a = t.pendingProps, a = t.elementType === r ? a : yt(r, a), qs(e, t, r, a, n);
    case 7:
      return He(e, t, t.pendingProps, n), t.child;
    case 8:
      return He(e, t, t.pendingProps.children, n), t.child;
    case 12:
      return He(e, t, t.pendingProps.children, n), t.child;
    case 10:
      e: {
        if (r = t.type._context, a = t.pendingProps, i = t.memoizedProps, o = a.value, ae(Wa, r._currentValue), r._currentValue = o, i !== null) if (kt(i.value, o)) {
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
                i.lanes |= n, c = i.alternate, c !== null && (c.lanes |= n), Fi(
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
            o.lanes |= n, s = o.alternate, s !== null && (s.lanes |= n), Fi(o, n, t), o = i.sibling;
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
      return a = t.type, r = t.pendingProps.children, Yn(t, n), a = ht(a), r = r(a), t.flags |= 1, He(e, t, r, n), t.child;
    case 14:
      return r = t.type, a = yt(r, t.pendingProps), a = yt(r.type, a), Ys(e, t, r, a, n);
    case 15:
      return ed(e, t, t.type, t.pendingProps, n);
    case 17:
      return r = t.type, a = t.pendingProps, a = t.elementType === r ? a : yt(r, a), Pa(e, t), t.tag = 1, Ze(r) ? (e = !0, Va(t)) : e = !1, Yn(t, n), Xu(t, r, a), _i(t, r, a, n), zi(null, t, r, !0, e, n);
    case 19:
      return ld(e, t, n);
    case 22:
      return td(e, t, n);
  }
  throw Error(I(156, t.tag));
};
function Nd(e, t) {
  return Kc(e, t);
}
function im(e, t, n, r) {
  this.tag = e, this.key = n, this.sibling = this.child = this.return = this.stateNode = this.type = this.elementType = null, this.index = 0, this.ref = null, this.pendingProps = t, this.dependencies = this.memoizedState = this.updateQueue = this.memoizedProps = null, this.mode = r, this.subtreeFlags = this.flags = 0, this.deletions = null, this.childLanes = this.lanes = 0, this.alternate = null;
}
function pt(e, t, n, r) {
  return new im(e, t, n, r);
}
function Mo(e) {
  return e = e.prototype, !(!e || !e.isReactComponent);
}
function om(e) {
  if (typeof e == "function") return Mo(e) ? 1 : 0;
  if (e != null) {
    if (e = e.$$typeof, e === to) return 11;
    if (e === no) return 14;
  }
  return 2;
}
function cn(e, t) {
  var n = e.alternate;
  return n === null ? (n = pt(e.tag, t, e.key, e.mode), n.elementType = e.elementType, n.type = e.type, n.stateNode = e.stateNode, n.alternate = e, e.alternate = n) : (n.pendingProps = t, n.type = e.type, n.flags = 0, n.subtreeFlags = 0, n.deletions = null), n.flags = e.flags & 14680064, n.childLanes = e.childLanes, n.lanes = e.lanes, n.child = e.child, n.memoizedProps = e.memoizedProps, n.memoizedState = e.memoizedState, n.updateQueue = e.updateQueue, t = e.dependencies, n.dependencies = t === null ? null : { lanes: t.lanes, firstContext: t.firstContext }, n.sibling = e.sibling, n.index = e.index, n.ref = e.ref, n;
}
function _a(e, t, n, r, a, i) {
  var o = 2;
  if (r = e, typeof e == "function") Mo(e) && (o = 1);
  else if (typeof e == "string") o = 5;
  else e: switch (e) {
    case Ln:
      return wn(n.children, a, i, t);
    case eo:
      o = 8, a |= 8;
      break;
    case ti:
      return e = pt(12, n, t, a | 2), e.elementType = ti, e.lanes = i, e;
    case ni:
      return e = pt(13, n, t, a), e.elementType = ni, e.lanes = i, e;
    case ri:
      return e = pt(19, n, t, a), e.elementType = ri, e.lanes = i, e;
    case _c:
      return pl(n, a, i, t);
    default:
      if (typeof e == "object" && e !== null) switch (e.$$typeof) {
        case Fc:
          o = 10;
          break e;
        case Rc:
          o = 9;
          break e;
        case to:
          o = 11;
          break e;
        case no:
          o = 14;
          break e;
        case Kt:
          o = 16, r = null;
          break e;
      }
      throw Error(I(130, e == null ? e : typeof e, ""));
  }
  return t = pt(o, n, t, a), t.elementType = e, t.type = r, t.lanes = i, t;
}
function wn(e, t, n, r) {
  return e = pt(7, e, r, t), e.lanes = n, e;
}
function pl(e, t, n, r) {
  return e = pt(22, e, r, t), e.elementType = _c, e.lanes = n, e.stateNode = { isHidden: !1 }, e;
}
function ql(e, t, n) {
  return e = pt(6, e, null, t), e.lanes = n, e;
}
function Yl(e, t, n) {
  return t = pt(4, e.children !== null ? e.children : [], e.key, t), t.lanes = n, t.stateNode = { containerInfo: e.containerInfo, pendingChildren: null, implementation: e.implementation }, t;
}
function sm(e, t, n, r, a) {
  this.tag = t, this.containerInfo = e, this.finishedWork = this.pingCache = this.current = this.pendingChildren = null, this.timeoutHandle = -1, this.callbackNode = this.pendingContext = this.context = null, this.callbackPriority = 0, this.eventTimes = Rl(0), this.expirationTimes = Rl(-1), this.entangledLanes = this.finishedLanes = this.mutableReadLanes = this.expiredLanes = this.pingedLanes = this.suspendedLanes = this.pendingLanes = 0, this.entanglements = Rl(0), this.identifierPrefix = r, this.onRecoverableError = a, this.mutableSourceEagerHydrationData = null;
}
function $o(e, t, n, r, a, i, o, s, c) {
  return e = new sm(e, t, n, s, c), t === 1 ? (t = 1, i === !0 && (t |= 8)) : t = 0, i = pt(3, null, null, t), e.current = i, i.stateNode = e, i.memoizedState = { element: r, isDehydrated: n, cache: null, transitions: null, pendingSuspenseBoundaries: null }, No(i), e;
}
function cm(e, t, n) {
  var r = 3 < arguments.length && arguments[3] !== void 0 ? arguments[3] : null;
  return { $$typeof: zn, key: r == null ? null : "" + r, children: e, containerInfo: t, implementation: n };
}
function Sd(e) {
  if (!e) return dn;
  e = e._reactInternals;
  e: {
    if (_n(e) !== e || e.tag !== 1) throw Error(I(170));
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
    if (Ze(n)) return Su(e, n, t);
  }
  return t;
}
function wd(e, t, n, r, a, i, o, s, c) {
  return e = $o(n, r, !0, e, a, i, o, s, c), e.context = Sd(null), n = e.current, r = Qe(), a = sn(n), i = bt(r, a), i.callback = t ?? null, ln(n, i, a), e.current.lanes = a, Xr(e, a, r), Je(e, r), e;
}
function ml(e, t, n, r) {
  var a = t.current, i = Qe(), o = sn(a);
  return n = Sd(n), t.context === null ? t.context = n : t.pendingContext = n, t = bt(i, o), t.payload = { element: e }, r = r === void 0 ? null : r, r !== null && (t.callback = r), e = ln(a, t, o), e !== null && (wt(e, a, o, i), ka(e, a, o)), o;
}
function tl(e) {
  if (e = e.current, !e.child) return null;
  switch (e.child.tag) {
    case 5:
      return e.child.stateNode;
    default:
      return e.child.stateNode;
  }
}
function sc(e, t) {
  if (e = e.memoizedState, e !== null && e.dehydrated !== null) {
    var n = e.retryLane;
    e.retryLane = n !== 0 && n < t ? n : t;
  }
}
function Oo(e, t) {
  sc(e, t), (e = e.alternate) && sc(e, t);
}
function um() {
  return null;
}
var Cd = typeof reportError == "function" ? reportError : function(e) {
  console.error(e);
};
function bo(e) {
  this._internalRoot = e;
}
hl.prototype.render = bo.prototype.render = function(e) {
  var t = this._internalRoot;
  if (t === null) throw Error(I(409));
  ml(e, t, null, null);
};
hl.prototype.unmount = bo.prototype.unmount = function() {
  var e = this._internalRoot;
  if (e !== null) {
    this._internalRoot = null;
    var t = e.containerInfo;
    Fn(function() {
      ml(null, e, null, null);
    }), t[Vt] = null;
  }
};
function hl(e) {
  this._internalRoot = e;
}
hl.prototype.unstable_scheduleHydration = function(e) {
  if (e) {
    var t = tu();
    e = { blockedOn: null, target: e, priority: t };
    for (var n = 0; n < Yt.length && t !== 0 && t < Yt[n].priority; n++) ;
    Yt.splice(n, 0, e), n === 0 && ru(e);
  }
};
function Uo(e) {
  return !(!e || e.nodeType !== 1 && e.nodeType !== 9 && e.nodeType !== 11);
}
function vl(e) {
  return !(!e || e.nodeType !== 1 && e.nodeType !== 9 && e.nodeType !== 11 && (e.nodeType !== 8 || e.nodeValue !== " react-mount-point-unstable "));
}
function cc() {
}
function dm(e, t, n, r, a) {
  if (a) {
    if (typeof r == "function") {
      var i = r;
      r = function() {
        var d = tl(o);
        i.call(d);
      };
    }
    var o = wd(t, r, e, 0, null, !1, !1, "", cc);
    return e._reactRootContainer = o, e[Vt] = o.current, br(e.nodeType === 8 ? e.parentNode : e), Fn(), o;
  }
  for (; a = e.lastChild; ) e.removeChild(a);
  if (typeof r == "function") {
    var s = r;
    r = function() {
      var d = tl(c);
      s.call(d);
    };
  }
  var c = $o(e, 0, !1, null, null, !1, !1, "", cc);
  return e._reactRootContainer = c, e[Vt] = c.current, br(e.nodeType === 8 ? e.parentNode : e), Fn(function() {
    ml(t, c, n, r);
  }), c;
}
function xl(e, t, n, r, a) {
  var i = n._reactRootContainer;
  if (i) {
    var o = i;
    if (typeof a == "function") {
      var s = a;
      a = function() {
        var c = tl(o);
        s.call(c);
      };
    }
    ml(t, o, e, a);
  } else o = dm(n, t, e, a, r);
  return tl(o);
}
Jc = function(e) {
  switch (e.tag) {
    case 3:
      var t = e.stateNode;
      if (t.current.memoizedState.isDehydrated) {
        var n = jr(t.pendingLanes);
        n !== 0 && (lo(t, n | 1), Je(t, ye()), !(J & 6) && (lr = ye() + 500, hn()));
      }
      break;
    case 13:
      Fn(function() {
        var r = Bt(e, 1);
        if (r !== null) {
          var a = Qe();
          wt(r, e, 1, a);
        }
      }), Oo(e, 1);
  }
};
io = function(e) {
  if (e.tag === 13) {
    var t = Bt(e, 134217728);
    if (t !== null) {
      var n = Qe();
      wt(t, e, 134217728, n);
    }
    Oo(e, 134217728);
  }
};
eu = function(e) {
  if (e.tag === 13) {
    var t = sn(e), n = Bt(e, t);
    if (n !== null) {
      var r = Qe();
      wt(n, e, t, r);
    }
    Oo(e, t);
  }
};
tu = function() {
  return ne;
};
nu = function(e, t) {
  var n = ne;
  try {
    return ne = e, t();
  } finally {
    ne = n;
  }
};
pi = function(e, t, n) {
  switch (t) {
    case "input":
      if (ii(e, n), t = n.name, n.type === "radio" && t != null) {
        for (n = e; n.parentNode; ) n = n.parentNode;
        for (n = n.querySelectorAll("input[name=" + JSON.stringify("" + t) + '][type="radio"]'), t = 0; t < n.length; t++) {
          var r = n[t];
          if (r !== e && r.form === e.form) {
            var a = ol(r);
            if (!a) throw Error(I(90));
            Dc(r), ii(r, a);
          }
        }
      }
      break;
    case "textarea":
      Lc(e, n);
      break;
    case "select":
      t = n.value, t != null && Qn(e, !!n.multiple, t, !1);
  }
};
Vc = zo;
Bc = Fn;
var fm = { usingClientEntryPoint: !1, Events: [Jr, On, ol, bc, Uc, zo] }, xr = { findFiberByHostInstance: yn, bundleType: 0, version: "18.3.1", rendererPackageName: "react-dom" }, pm = { bundleType: xr.bundleType, version: xr.version, rendererPackageName: xr.rendererPackageName, rendererConfig: xr.rendererConfig, overrideHookState: null, overrideHookStateDeletePath: null, overrideHookStateRenamePath: null, overrideProps: null, overridePropsDeletePath: null, overridePropsRenamePath: null, setErrorHandler: null, setSuspenseHandler: null, scheduleUpdate: null, currentDispatcherRef: Wt.ReactCurrentDispatcher, findHostInstanceByFiber: function(e) {
  return e = Qc(e), e === null ? null : e.stateNode;
}, findFiberByHostInstance: xr.findFiberByHostInstance || um, findHostInstancesForRefresh: null, scheduleRefresh: null, scheduleRoot: null, setRefreshHandler: null, getCurrentFiber: null, reconcilerVersion: "18.3.1-next-f1338f8080-20240426" };
if (typeof __REACT_DEVTOOLS_GLOBAL_HOOK__ < "u") {
  var ya = __REACT_DEVTOOLS_GLOBAL_HOOK__;
  if (!ya.isDisabled && ya.supportsFiber) try {
    rl = ya.inject(pm), _t = ya;
  } catch {
  }
}
lt.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED = fm;
lt.createPortal = function(e, t) {
  var n = 2 < arguments.length && arguments[2] !== void 0 ? arguments[2] : null;
  if (!Uo(t)) throw Error(I(200));
  return cm(e, t, null, n);
};
lt.createRoot = function(e, t) {
  if (!Uo(e)) throw Error(I(299));
  var n = !1, r = "", a = Cd;
  return t != null && (t.unstable_strictMode === !0 && (n = !0), t.identifierPrefix !== void 0 && (r = t.identifierPrefix), t.onRecoverableError !== void 0 && (a = t.onRecoverableError)), t = $o(e, 1, !1, null, null, n, !1, r, a), e[Vt] = t.current, br(e.nodeType === 8 ? e.parentNode : e), new bo(t);
};
lt.findDOMNode = function(e) {
  if (e == null) return null;
  if (e.nodeType === 1) return e;
  var t = e._reactInternals;
  if (t === void 0)
    throw typeof e.render == "function" ? Error(I(188)) : (e = Object.keys(e).join(","), Error(I(268, e)));
  return e = Qc(t), e = e === null ? null : e.stateNode, e;
};
lt.flushSync = function(e) {
  return Fn(e);
};
lt.hydrate = function(e, t, n) {
  if (!vl(t)) throw Error(I(200));
  return xl(null, e, t, !0, n);
};
lt.hydrateRoot = function(e, t, n) {
  if (!Uo(e)) throw Error(I(405));
  var r = n != null && n.hydratedSources || null, a = !1, i = "", o = Cd;
  if (n != null && (n.unstable_strictMode === !0 && (a = !0), n.identifierPrefix !== void 0 && (i = n.identifierPrefix), n.onRecoverableError !== void 0 && (o = n.onRecoverableError)), t = wd(t, null, e, 1, n ?? null, a, !1, i, o), e[Vt] = t.current, br(e), r) for (e = 0; e < r.length; e++) n = r[e], a = n._getVersion, a = a(n._source), t.mutableSourceEagerHydrationData == null ? t.mutableSourceEagerHydrationData = [n, a] : t.mutableSourceEagerHydrationData.push(
    n,
    a
  );
  return new hl(t);
};
lt.render = function(e, t, n) {
  if (!vl(t)) throw Error(I(200));
  return xl(null, e, t, !1, n);
};
lt.unmountComponentAtNode = function(e) {
  if (!vl(e)) throw Error(I(40));
  return e._reactRootContainer ? (Fn(function() {
    xl(null, null, e, !1, function() {
      e._reactRootContainer = null, e[Vt] = null;
    });
  }), !0) : !1;
};
lt.unstable_batchedUpdates = zo;
lt.unstable_renderSubtreeIntoContainer = function(e, t, n, r) {
  if (!vl(n)) throw Error(I(200));
  if (e == null || e._reactInternals === void 0) throw Error(I(38));
  return xl(e, t, n, !1, r);
};
lt.version = "18.3.1-next-f1338f8080-20240426";
function kd() {
  if (!(typeof __REACT_DEVTOOLS_GLOBAL_HOOK__ > "u" || typeof __REACT_DEVTOOLS_GLOBAL_HOOK__.checkDCE != "function"))
    try {
      __REACT_DEVTOOLS_GLOBAL_HOOK__.checkDCE(kd);
    } catch (e) {
      console.error(e);
    }
}
kd(), kc.exports = lt;
var mm = kc.exports, Ed, uc = mm;
Ed = uc.createRoot, uc.hydrateRoot;
class hm extends Error {
  constructor(t, n) {
    super(t), this.status = n, this.name = "ApiError";
  }
}
function vm(e, t) {
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
      throw new hm(j, c.status);
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
const Id = x.createContext(null);
function Dt() {
  const e = x.useContext(Id);
  if (!e) throw new Error("Fuera del módulo de documentos");
  return e;
}
function xm(e) {
  return vm((t, n) => fetch(t, n), e.token);
}
async function qr(e, t) {
  const n = e.token(), r = await fetch(t, { headers: n ? { Authorization: "Bearer " + n } : {} });
  if (!r.ok) throw new Error("No se pudo generar el documento");
  const a = URL.createObjectURL(await r.blob());
  window.open(a, "_blank"), setTimeout(() => URL.revokeObjectURL(a), 6e4);
}
async function gm(e, t) {
  var s;
  const n = e.token(), r = await fetch(t, { headers: n ? { Authorization: "Bearer " + n } : {} });
  if (!r.ok) throw new Error((await r.json().catch(() => ({}))).title || "No se pudo generar el fichero");
  const a = ((s = /filename="?([^";]+)"?/.exec(r.headers.get("Content-Disposition") ?? "")) == null ? void 0 : s[1]) ?? "fichero", i = URL.createObjectURL(await r.blob()), o = document.createElement("a");
  o.href = i, o.download = a, o.click(), setTimeout(() => URL.revokeObjectURL(i), 6e4);
}
function Pd(e, t) {
  return `${e.toLowerCase().normalize("NFD").replace(/[̀-ͯ]/g, "").replace(/[^a-z0-9]+/g, "-").replace(/^-|-$/g, "").slice(0, 60) || "listado"}-${(/* @__PURE__ */ new Date()).toISOString().slice(0, 10)}.${t}`;
}
function Fd(e, t) {
  const n = URL.createObjectURL(e), r = document.createElement("a");
  r.href = n, r.download = t, document.body.appendChild(r), r.click(), r.remove(), setTimeout(() => URL.revokeObjectURL(n), 6e4);
}
async function ym(e, t) {
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
  Fd(await n.blob(), Pd(t.titulo, "xlsx"));
}
function jm(e) {
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
function Nm(e) {
  Fd(new Blob(["\uFEFF" + jm(e)], { type: "text/csv;charset=utf-8" }), Pd(e.titulo, "csv"));
}
const Rd = new Intl.NumberFormat("es-ES", { minimumFractionDigits: 2, maximumFractionDigits: 2 }), Sm = new Intl.NumberFormat("es-ES", { maximumFractionDigits: 3 }), z = (e) => `${Rd.format(Number(e) || 0)} €`, de = (e) => Rd.format(Number(e) || 0), Zt = (e, t) => t ? `${de(e)} ${t}` : z(e), We = (e) => Sm.format(Number(e) || 0), Ee = (e) => {
  if (!e) return "";
  const t = String(e).slice(0, 10).split("-");
  return t.length === 3 ? `${t[2]}/${t[1]}/${t[0]}` : String(e);
}, Ct = () => (/* @__PURE__ */ new Date()).toISOString().slice(0, 10), be = (e) => Math.round((e + Number.EPSILON) * 100) / 100;
let wm = 0;
const ta = () => `l${Date.now().toString(36)}${(++wm).toString(36)}`;
function Cn(e, t) {
  const [n, r] = x.useState(e);
  return x.useEffect(() => {
    const a = setTimeout(() => r(e), t);
    return () => clearTimeout(a);
  }, [e, t]), n;
}
function na() {
  const e = x.useRef(0);
  return () => {
    const t = ++e.current;
    return () => t === e.current;
  };
}
function Vo(e) {
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
const dc = { presupuesto: "Presupuestos", pedido: "Pedidos de venta", factura: "Facturas emitidas", compra: "Pedidos de compra", gasto: "Facturas de proveedor y gastos" }, Cm = { presupuesto: "+ Nuevo presupuesto", pedido: "+ Nuevo pedido", factura: "+ Nueva factura", compra: "+ Nuevo pedido", gasto: "+ Registrar factura" }, km = {
  presupuesto: ["Borrador", "Aceptado", "Rechazado"],
  pedido: ["Borrador", "Confirmado", "ServidoParcial", "Servido", "Facturado", "Cancelado"],
  factura: ["Emitida", "Rectificada", "Anulada"],
  compra: ["Borrador", "Confirmado", "RecibidoParcial", "Recibido", "Facturado", "Cancelado"],
  gasto: ["Registrado", "Anulado"]
}, Xl = { numero: "numero", fecha: "fecha", tercero: "tercero", base: "base", impuestos: "impuestos", total: "total" }, fc = 50, Qi = (e) => e === "Anulada" || e === "Anulado" || e === "Cancelado";
function pc(e, t) {
  const n = { documentos: 0, baseImponible: 0, impuestos: 0, retenciones: 0, total: 0, pendiente: 0, vencido: 0, documentosVencidos: 0 };
  for (const r of e) {
    if (Qi(r.estado)) continue;
    n.documentos++, n.baseImponible += r.base, n.impuestos += r.impuestos, n.total += r.total;
    const a = r.pendiente ?? 0;
    a > 0 && (n.pendiente += a, r.vencimiento && r.vencimiento < t && (n.vencido += a, n.documentosVencidos++));
  }
  return n.baseImponible = be(n.baseImponible), n.impuestos = be(n.impuestos), n.total = be(n.total), n.pendiente = be(n.pendiente), n.vencido = be(n.vencido), n;
}
function Zl(e, t, n) {
  const r = (a) => t === "numero" || t === "tercero" || t === "estado" ? a[t].toLowerCase() : t === "fecha" ? a.fecha : a[t] ?? 0;
  return [...e].sort((a, i) => {
    const o = r(a), s = r(i), c = typeof o == "number" && typeof s == "number" ? o - s : String(o).localeCompare(String(s), "es", { numeric: !0 });
    return n ? -c : c;
  });
}
const Em = (e, t) => ({
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
}), Im = (e, t) => {
  var n, r;
  return {
    id: e.id,
    numero: e.numeroFactura ?? "—",
    fecha: e.fecha,
    tercero: `${e.proveedorTexto ?? ""}${e.numeroFactura ? "" : ` · ${e.concepto}`}`,
    terceroId: e.proveedorId,
    base: e.baseImponible,
    impuestos: be(e.cuotaIva + (e.recargoTotal || 0)),
    total: e.total,
    estado: e.estado === "Anulado" ? "Anulada" : e.estado,
    extra: e.esRectificativa ? "Rectificativa" : void 0,
    pendiente: t ? t[e.id] ?? 0 : null,
    vencimiento: ((r = (n = e.vencimientos) == null ? void 0 : n[0]) == null ? void 0 : r.fecha) ?? e.fecha
  };
};
function Pm(e) {
  const { api: t, navegar: n, anfitrion: r } = Dt(), a = e.tipo, i = a === "factura" || a === "gasto", o = a === "compra" || a === "gasto", [s, c] = x.useState(""), [d, j] = x.useState(""), [u, m] = x.useState(""), [h, g] = x.useState(""), [w, $] = x.useState(""), [p, f] = x.useState(""), [v, k] = x.useState(""), [_, F] = x.useState(""), [T, C] = x.useState(""), [M, b] = x.useState({ campo: "fecha", desc: !0 }), [P, Q] = x.useState(1), [ce, Ae] = x.useState(null), [Be, me] = x.useState(0), [ge, E] = x.useState(null), [y, L] = x.useState([]), [B, W] = x.useState([]), [q, Se] = x.useState(""), [je, he] = x.useState(!1), _e = Cn(s, 250), ot = Cn(v, 350), we = Cn(_, 350), D = na(), st = Ct();
  x.useEffect(() => {
    t.get(o ? "/proveedores" : "/clientes").then((R) => L([...R].sort((G, A) => G.nombre.localeCompare(A.nombre, "es")))).catch(() => L([])), a === "factura" && t.get("/series").then((R) => W([...new Set(R.filter((G) => G.tipoDocumento === "Factura" || G.tipoDocumento === 0).map((G) => G.prefijo))].sort())).catch(() => W([]));
  }, [t, a, o]), x.useEffect(() => Q(1), [_e, d, u, h, w, p, ot, we, T, M, a]);
  const U = (R, G) => {
    const A = new URLSearchParams({ pagina: String(R), tamanoPagina: String(G) });
    _e.trim() && A.set("texto", _e.trim()), d && A.set("estado", d === "Anulada" && a === "gasto" ? "Anulado" : d), u && A.set("desde", u), h && A.set("hasta", h), w && A.set(a === "gasto" ? "proveedorId" : "clienteId", w), p && a === "factura" && A.set("serie", p);
    const Y = parseFloat(ot.replace(/\./g, "").replace(",", ".")), le = parseFloat(we.replace(/\./g, "").replace(",", "."));
    isNaN(Y) || A.set("importeMin", String(Y)), isNaN(le) || A.set("importeMax", String(le)), T && A.set("cobro", T);
    const N = Xl[M.campo];
    return N && (A.set("orden", N === "tercero" ? a === "gasto" ? "proveedor" : "cliente" : N), A.set("desc", String(M.desc))), A;
  }, Et = async (R, G) => {
    if (a === "factura") {
      const Y = await t.get(`/facturas/buscar?${U(R, G)}`);
      return { r: Y, filas: Y.elementos.map((le) => Em(le, Y.pendientes)) };
    }
    const A = await t.get(`/gastos/buscar?${U(R, G)}`);
    return { r: A, filas: A.elementos.map((Y) => Im(Y, A.pendientes)) };
  };
  x.useEffect(() => {
    Se("");
    const R = D();
    (async () => {
      if (i) {
        const { r: A, filas: Y } = await Et(P, fc);
        return R() && (me(A.total), E(A.totales ?? null)), Y;
      }
      switch (a) {
        case "presupuesto":
          return (await t.get("/presupuestos")).map((A) => ({ id: A.id, numero: A.numeroCompleto, fecha: A.fecha, tercero: A.clienteNombre, terceroId: A.clienteId, base: A.baseImponible ?? A.total, impuestos: A.cuotaIva ?? 0, total: A.total, estado: A.estado }));
        case "pedido":
          return (await t.get("/pedidos-venta")).map((A) => {
            const Y = be(A.lineas.reduce((le, N) => le + N.base, 0));
            return { id: A.id, numero: A.numeroCompleto, fecha: A.fecha, tercero: A.clienteNombre, terceroId: A.clienteId, base: Y, impuestos: be(A.total - Y), total: A.total, estado: A.estado };
          });
        default:
          return (await t.get("/compras/pedidos")).map((A) => {
            const Y = be(A.lineas.reduce((le, N) => le + N.importe, 0));
            return { id: A.id, numero: A.numeroCompleto, fecha: A.fecha, tercero: A.proveedorTexto, terceroId: A.proveedorId, base: Y, impuestos: be(A.total - Y), total: A.total, estado: A.estado, extra: A.empresaOrigenId ? "Intragrupo" : void 0 };
          });
      }
    })().then((A) => R() && Ae(A)).catch((A) => R() && (Se(A.message), Ae([])));
  }, [t, a, P, _e, d, u, h, w, p, ot, we, T, M.campo, M.desc]);
  const et = x.useMemo(() => {
    if (!ce) return [];
    if (i) return Xl[M.campo] ? ce : Zl(ce, M.campo, M.desc);
    const R = _e.trim().toLowerCase(), G = parseFloat(ot.replace(/\./g, "").replace(",", ".")), A = parseFloat(we.replace(/\./g, "").replace(",", ".")), Y = ce.filter((le) => (!R || le.numero.toLowerCase().includes(R) || le.tercero.toLowerCase().includes(R)) && (!d || le.estado === d) && (!u || le.fecha >= u) && (!h || le.fecha <= h) && (!w || le.terceroId === w) && (isNaN(G) || le.total >= G) && (isNaN(A) || le.total <= A));
    return Zl(Y, M.campo, M.desc);
  }, [ce, _e, d, u, h, w, ot, we, M, i]), Te = i ? ge : pc(et, st), It = i ? Math.max(1, Math.ceil(Be / fc)) : 1, zt = [d, u, h, w, p, v, _, T].filter(Boolean).length, vn = o ? "Proveedor" : "Cliente";
  function ve() {
    c(""), j(""), m(""), g(""), $(""), f(""), k(""), F(""), C("");
  }
  function Pe(R, G, A = !1) {
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
    return Xl[M.campo] ? R : Zl(R, M.campo, M.desc);
  }
  async function ct(R) {
    he(!0);
    try {
      const G = await Lt(), A = i, Y = i ? ge : pc(G, st), le = {
        titulo: dc[a],
        columnas: [
          { titulo: "Número", tipo: "texto" },
          { titulo: "Fecha", tipo: "fecha" },
          { titulo: vn, tipo: "texto" },
          { titulo: "Estado", tipo: "texto" },
          { titulo: "Base", tipo: "moneda", total: "no" },
          { titulo: "Impuestos", tipo: "moneda", total: "no" },
          { titulo: "Total", tipo: "moneda", total: "no" },
          ...A ? [{ titulo: "Pendiente", tipo: "moneda", total: "no" }, { titulo: "Vencimiento", tipo: "fecha" }] : []
        ],
        filas: G.map((N) => [N.numero + (N.extra ? ` (${N.extra})` : ""), Ee(N.fecha), N.tercero, N.estado, N.base, N.impuestos, N.total, ...A ? [N.pendiente ?? 0, Ee(N.vencimiento)] : []]),
        totales: Y ? [`Total · ${Y.documentos} (sin anulados)`, null, null, null, Y.baseImponible, Y.impuestos, Y.total, ...A ? [Y.pendiente, null] : []] : void 0
      };
      R === "xlsx" ? await ym(r.token(), le) : Nm(le), r.aviso(`Exportados ${G.length} documento(s).`, "ok");
    } catch (G) {
      r.aviso("No se pudo exportar: " + G.message, "err");
    } finally {
      he(!1);
    }
  }
  return /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel-head", children: [
      /* @__PURE__ */ l.jsx("h2", { children: dc[a] }),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-acciones", children: [
        /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", disabled: je || !et.length, onClick: () => ct("xlsx"), title: "Exportar a Excel todo lo filtrado", children: je ? "Exportando…" : "↓ Excel" }),
        /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", disabled: je || !et.length, onClick: () => ct("csv"), title: "Exportar a CSV todo lo filtrado", children: "↓ CSV" }),
        /* @__PURE__ */ l.jsx("button", { className: "btn small", onClick: () => n({ tipo: a, pantalla: "editor" }), children: Cm[a] })
      ] })
    ] }),
    /* @__PURE__ */ l.jsxs("div", { className: "dx-filtros", children: [
      /* @__PURE__ */ l.jsx("input", { placeholder: o ? "Número, proveedor o concepto…" : "Número, cliente o NIF…", value: s, onChange: (R) => c(R.target.value), autoFocus: !0 }),
      /* @__PURE__ */ l.jsxs("select", { value: d, onChange: (R) => j(R.target.value), "aria-label": "Estado", children: [
        /* @__PURE__ */ l.jsx("option", { value: "", children: "Todos los estados" }),
        km[a].map((R) => /* @__PURE__ */ l.jsx("option", { value: R, children: R }, R))
      ] }),
      /* @__PURE__ */ l.jsx("input", { type: "date", value: u, onChange: (R) => m(R.target.value), title: "Desde", "aria-label": "Desde" }),
      /* @__PURE__ */ l.jsx("input", { type: "date", value: h, onChange: (R) => g(R.target.value), title: "Hasta", "aria-label": "Hasta" })
    ] }),
    /* @__PURE__ */ l.jsxs("div", { className: "dx-filtros dx-filtros2", children: [
      /* @__PURE__ */ l.jsxs("select", { value: w, onChange: (R) => $(R.target.value), "aria-label": vn, children: [
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
      /* @__PURE__ */ l.jsx("input", { inputMode: "decimal", placeholder: "Importe máx.", value: _, onChange: (R) => F(R.target.value), "aria-label": "Importe máximo" }),
      i && /* @__PURE__ */ l.jsxs("select", { value: T, onChange: (R) => C(R.target.value), "aria-label": a === "gasto" ? "Estado de pago" : "Estado de cobro", children: [
        /* @__PURE__ */ l.jsx("option", { value: "", children: a === "gasto" ? "Pagadas y pendientes" : "Cobradas y pendientes" }),
        /* @__PURE__ */ l.jsx("option", { value: "pendiente", children: "Pendientes" }),
        /* @__PURE__ */ l.jsx("option", { value: "vencida", children: "Vencidas" }),
        /* @__PURE__ */ l.jsx("option", { value: a === "gasto" ? "pagada" : "cobrada", children: a === "gasto" ? "Pagadas" : "Cobradas" })
      ] }),
      (zt > 0 || s) && /* @__PURE__ */ l.jsxs("button", { className: "btn small secondary", onClick: ve, children: [
        "Limpiar",
        zt ? ` (${zt})` : ""
      ] })
    ] }),
    q && /* @__PURE__ */ l.jsx("p", { className: "dx-rojo", children: q }),
    ce === null ? /* @__PURE__ */ l.jsx("p", { className: "muted", children: "Cargando…" }) : et.length === 0 ? /* @__PURE__ */ l.jsx("p", { className: "muted", children: "No hay documentos con estos filtros." }) : /* @__PURE__ */ l.jsxs("table", { className: "dx-lista-docs", children: [
      /* @__PURE__ */ l.jsx("thead", { children: /* @__PURE__ */ l.jsxs("tr", { children: [
        Pe("numero", "Número"),
        Pe("fecha", "Fecha"),
        Pe("tercero", vn),
        Pe("estado", "Estado"),
        Pe("base", "Base", !0),
        Pe("impuestos", "Impuestos", !0),
        Pe("total", "Total", !0),
        i && Pe("pendiente", "Pendiente", !0)
      ] }) }),
      /* @__PURE__ */ l.jsx("tbody", { children: et.map((R) => {
        const G = (R.pendiente ?? 0) > 0 && !!R.vencimiento && R.vencimiento < st;
        return /* @__PURE__ */ l.jsxs("tr", { onClick: () => n({ tipo: a, pantalla: "vista", id: R.id }), tabIndex: 0, onKeyDown: (A) => A.key === "Enter" && n({ tipo: a, pantalla: "vista", id: R.id }), className: Qi(R.estado) ? "dx-anulado" : void 0, children: [
          /* @__PURE__ */ l.jsxs("td", { className: "mono", children: [
            /* @__PURE__ */ l.jsx("strong", { children: R.numero }),
            R.extra && /* @__PURE__ */ l.jsx("span", { className: "pill part", style: { marginLeft: 6 }, children: R.extra })
          ] }),
          /* @__PURE__ */ l.jsx("td", { children: Ee(R.fecha) }),
          /* @__PURE__ */ l.jsx("td", { children: R.tercero }),
          /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx("span", { className: Vo(R.estado), children: R.estado }) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: z(R.base) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: z(R.impuestos) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: z(R.total) }) }),
          i && /* @__PURE__ */ l.jsx("td", { className: "num", children: (R.pendiente ?? 0) > 0 ? /* @__PURE__ */ l.jsx("strong", { className: G ? "dx-rojo" : void 0, title: G ? `Vencida el ${Ee(R.vencimiento)}` : `Vence el ${Ee(R.vencimiento)}`, children: z(R.pendiente) }) : /* @__PURE__ */ l.jsx("span", { className: "muted", children: Qi(R.estado) || R.estado === "Rectificada" ? "—" : a === "gasto" ? "Pagada" : "Cobrada" }) })
        ] }, R.id);
      }) }),
      Te && /* @__PURE__ */ l.jsx("tfoot", { children: /* @__PURE__ */ l.jsxs("tr", { className: "dx-total", children: [
        /* @__PURE__ */ l.jsxs("td", { colSpan: 4, children: [
          /* @__PURE__ */ l.jsxs("strong", { children: [
            "Total · ",
            Te.documentos
          ] }),
          " ",
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: i ? `documento${Te.documentos === 1 ? "" : "s"} de todo el filtro (${It} página${It === 1 ? "" : "s"}) · sin anulados` : "sin anulados ni cancelados" }),
          i && Te.vencido > 0 && /* @__PURE__ */ l.jsxs("span", { className: "dx-rojo", style: { marginLeft: 8 }, children: [
            "· vencido ",
            z(Te.vencido),
            " (",
            Te.documentosVencidos,
            ")"
          ] })
        ] }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: z(Te.baseImponible) }) }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: z(Te.impuestos) }) }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: z(Te.total) }) }),
        i && /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: z(Te.pendiente) }) })
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
        Be,
        " documentos"
      ] }),
      /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", disabled: P >= It, onClick: () => Q(P + 1), children: "→" })
    ] })
  ] });
}
function fn(e) {
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
function Fm(e) {
  const { api: t } = Dt(), [n, r] = x.useState(!1), [a, i] = x.useState([]), [o, s] = x.useState(null), [c, d] = x.useState(0), j = Cn(e.texto, 180), u = o === e.texto.trim() ? a : [], m = na();
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
            z(e.precioDe ? e.precioDe(g) : g.precioUnitario),
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
function Bo(e) {
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
const Rm = { Porcentaje: "%", PorUnidad: "€/ud", PorKilo: "€/kg", Importe: "€" };
function Ho(e) {
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
          Rm[s.calculo],
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
function gl(e) {
  var t;
  return (t = e.conceptos) != null && t.length ? /* @__PURE__ */ l.jsx("div", { className: "dx-aplicados", children: e.conceptos.map((n, r) => /* @__PURE__ */ l.jsxs("div", { children: [
    "· ",
    n.nombre,
    n.calculo === "Porcentaje" ? ` (${We(n.valor)} %)` : "",
    n.repartido ? " · del documento" : "",
    n.efecto === "Coste" ? " · coste" : "",
    ": ",
    /* @__PURE__ */ l.jsx("strong", { children: z(n.importe) })
  ] }, r)) }) : null;
}
const _r = () => ({ clave: ta(), productoId: null, descripcion: "", cantidad: 1, precio: null, dto: 0, iva: null });
function _d(e) {
  const t = x.useRef(null), [n, r] = x.useState(/* @__PURE__ */ new Set()), a = e.modo === "venta", i = a ? 7 : 5, o = (u, m) => e.alCambiar(e.lineas.map((h) => h.clave === u ? { ...h, ...m } : h)), s = (u) => {
    const m = e.lineas.filter((h) => h.clave !== u);
    e.alCambiar(m.length ? m : [_r()]);
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
        h === e.lineas.length - 1 ? (e.alCambiar([...e.lineas, _r()]), setTimeout(() => c(h + 1, 0), 30)) : c(h + 1, 0);
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
                Fm,
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
              g.length > 0 && !n.has(u.clave) && /* @__PURE__ */ l.jsx("button", { type: "button", className: "dx-resumen-conc", onClick: () => j(u.clave), title: "Ver y cambiar los conceptos", children: g.map((p) => `${p.importe < 0 ? "−" : "+"} ${p.codigo.toLowerCase()} ${de(Math.abs(p.importe))}${p.efecto === "Coste" ? " (coste)" : ""}`).join(" · ") })
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
                placeholder: h ? de(h.precio) : "",
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
            /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: h ? z(h.importe) : "—" }) }),
            /* @__PURE__ */ l.jsx("td", { className: "num", children: a ? (h == null ? void 0 : h.margen) != null && /* @__PURE__ */ l.jsxs("span", { className: h.margen < 0 ? "dx-rojo" : "muted", children: [
              z(h.margen),
              $ != null && /* @__PURE__ */ l.jsxs("div", { className: "dx-sub", children: [
                de($),
                " %"
              ] })
            ] }) : (h == null ? void 0 : h.costeUnitarioEntrada) != null && /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
              z(h.costeUnitarioEntrada),
              "/ud"
            ] }) }),
            /* @__PURE__ */ l.jsxs("td", { className: "right", style: { whiteSpace: "nowrap" }, children: [
              !e.soloLectura && e.catalogo.length > 0 && /* @__PURE__ */ l.jsx("button", { type: "button", className: "dx-icono" + (n.has(u.clave) ? " activo" : ""), title: "Conceptos de la línea", onClick: () => j(u.clave), "data-f": m, "data-c": a ? 6 : 4, children: "±" }),
              !e.soloLectura && /* @__PURE__ */ l.jsx("button", { type: "button", className: "dx-icono", title: "Quitar la línea", onClick: () => s(u.clave), children: "✕" })
            ] })
          ] }, u.clave),
          n.has(u.clave) && /* @__PURE__ */ l.jsx("tr", { className: "dx-fila-conc", children: /* @__PURE__ */ l.jsx("td", { colSpan: a ? 9 : 7, children: /* @__PURE__ */ l.jsx(Ho, { catalogo: e.catalogo, lista: u.conceptos, sugeridos: e.sugeridos[u.clave], alCambiar: (p) => o(u.clave, { conceptos: p }) }) }) }, u.clave + "c")
        ];
      }) })
    ] }),
    !e.soloLectura && /* @__PURE__ */ l.jsx("button", { type: "button", className: "btn small ghost", style: { marginTop: 8 }, onClick: () => (e.alCambiar([...e.lineas, _r()]), setTimeout(() => c(e.lineas.length, 0), 30)), children: "+ Añadir línea" }),
    !e.soloLectura && /* @__PURE__ */ l.jsx("span", { className: "muted dx-ayuda", children: "Intro: siguiente casilla · ↑↓: cambiar de línea · F2: buscar artículo · el precio en gris es el de tarifa" })
  ] });
}
function _m(e, t) {
  if (!e) return e;
  const n = e.toUpperCase();
  return (t === "Igic" ? { IVA21: "IGIC7", IVA10: "IGIC3", IVA4: "IGIC0", IVA0: "IGIC0", REAGP12: "REAGPIGIC", REAGP105: "REAGPIGIC" }[n] : { IGIC7: "IVA21", IGIC3: "IVA10", IGIC0: "IVA0", REAGPIGIC: "REAGP12" }[n]) ?? e;
}
const Td = ["USD", "GBP", "CHF", "JPY", "CNY", "CAD", "MXN", "BRL", "SEK", "NOK", "DKK", "PLN", "MAD"], Wo = (e) => e.filter((t) => t.disponible > 0 && t.estado !== "Anulado" && !t.facturaId).sort((t, n) => t.fecha.localeCompare(n.fecha)), Dd = (e) => e.filter((t) => !!t.facturaId && (t.disponibleBase ?? 0) > 0 && t.estado !== "Anulado").sort((t, n) => t.fecha.localeCompare(n.fecha));
function zd(e, t) {
  let n = Math.round(t * 100);
  const r = [];
  for (const a of Wo(e)) {
    if (n <= 0) break;
    const i = Math.min(n, Math.round(a.disponible * 100));
    i > 0 && r.push({ id: a.id, importe: i / 100 }), n -= i;
  }
  return r;
}
const Tm = { presupuesto: "presupuesto", pedido: "pedido de venta", factura: "factura" };
function Ld(e) {
  const t = /* @__PURE__ */ new Map();
  for (const n of e)
    for (const r of n.conceptos ?? [])
      if (r.repartido) {
        const a = t.get(r.conceptoId);
        t.set(r.conceptoId, { conceptoId: r.conceptoId, valor: r.calculo === "Importe" ? be(((a == null ? void 0 : a.valor) ?? 0) + r.valor) : r.valor });
      }
  return {
    porLinea: e.map((n) => (n.conceptos ?? []).filter((r) => !r.repartido).map((r) => ({ conceptoId: r.conceptoId, valor: r.valor }))),
    documento: [...t.values()]
  };
}
function Dm(e) {
  var Go, Ko, qo, Yo, Xo, Zo, Jo, es;
  const { api: t, anfitrion: n } = Dt(), r = !!((Go = e.semilla) != null && Go.rectificaId), [a, i] = x.useState([]), [o, s] = x.useState([]), [c, d] = x.useState([]), [j, u] = x.useState([]), [m, h] = x.useState([]), [g, w] = x.useState(((Ko = e.semilla) == null ? void 0 : Ko.clienteId) ?? ""), [$, p] = x.useState(e.tipo === "pedido" && ((qo = e.semilla) != null && qo.fecha) ? e.semilla.fecha : Ct()), [f, v] = x.useState(""), [k, _] = x.useState(""), [F, T] = x.useState(0), [C, M] = x.useState(!1), [b, P] = x.useState(null), [Q, ce] = x.useState(30), [Ae, Be] = x.useState(""), [me, ge] = x.useState([_r()]), [E, y] = x.useState([]), [L, B] = x.useState(!1), W = (Yo = e.semilla) != null && Yo.lineas.some((S) => /^(IGIC|REAGPIGIC)/i.test(S.codigoIva ?? "")) ? "Igic" : (Xo = e.semilla) != null && Xo.lineas.length ? "Iva" : null, [q, Se] = x.useState(W ?? "Iva"), [je, he] = x.useState(((Zo = e.semilla) == null ? void 0 : Zo.moneda) ?? ""), [_e, ot] = x.useState(r ? ((Jo = e.semilla) == null ? void 0 : Jo.tasaCambio) ?? null : null), we = !!je, [D, st] = x.useState(null), [U, Et] = x.useState(""), [et, Te] = x.useState(!1), [It, zt] = x.useState(!1), [vn, ve] = x.useState(!1), [Pe, Lt] = x.useState([]), [ct, R] = x.useState(!0), [G, A] = x.useState([]), [Y, le] = x.useState(!0), N = na();
  x.useEffect(() => {
    t.get("/clientes").then(i).catch(() => i([])), t.get("/tipos-iva").then((S) => s(S.filter((V) => V.activo))).catch(() => s([])), t.get("/formas-pago").then((S) => d(S.filter((V) => V.activo))).catch(() => d([])), t.get("/series").then((S) => u([...new Set(S.filter((V) => V.tipoDocumento === "Factura").map((V) => V.prefijo))])).catch(() => u([])), t.get("/conceptos-linea?ambito=Ventas&activos=true").then(h).catch(() => h([])), t.get("/empresas/actual").then((S) => {
      B(!!S.operaEnAmbosTerritorios), Se(W ?? (S.territorioFiscal === "Canarias" ? "Igic" : "Iva"));
    }).catch(() => B(!1));
  }, [t]);
  function H(S) {
    Se(S), ge((V) => V.map((ee) => ({ ...ee, iva: ee.productoId ? null : _m(ee.iva, S) })));
  }
  const Ce = x.useMemo(() => L ? o.filter((S) => (S.impuesto ?? "Iva") === q) : o, [L, o, q]);
  x.useEffect(() => {
    const S = e.semilla;
    if (!S || !S.lineas.length) return;
    const { porLinea: V, documento: ee } = Ld(S.lineas), re = S.lineas.map((Z, Cl) => ({
      clave: ta(),
      productoId: Z.productoId ?? null,
      descripcion: Z.descripcion,
      cantidad: Z.cantidad,
      precio: Z.precioUnitario,
      dto: Z.porcentajeDescuento,
      iva: Z.codigoIva,
      conceptos: r ? [] : V[Cl]
    }));
    ge(re), y(r ? [] : ee), Promise.all(re.map((Z) => Z.productoId ? t.get(`/productos/${Z.productoId}`).catch(() => null) : Promise.resolve(null))).then(
      (Z) => ge((Cl) => Cl.map((ts, Tn) => Z[Tn] ? { ...ts, referencia: Z[Tn].referencia ?? Z[Tn].nombre, unidad: Z[Tn].unidad, stock: Z[Tn].stock, controlarStock: Z[Tn].controlarStock } : ts))
    );
  }, [e.semilla, t, r]);
  const K = a.find((S) => S.id === g);
  x.useEffect(() => {
    if (e.tipo !== "factura" || r || !g) {
      Lt([]), A([]);
      return;
    }
    t.get(`/anticipos?clienteId=${g}`).then((S) => {
      Lt(Wo(S)), A(Dd(S));
    }).catch(() => {
      Lt([]), A([]);
    });
  }, [t, g, e.tipo, r]);
  const te = be(Pe.reduce((S, V) => S + V.disponible, 0));
  x.useEffect(() => {
    K && (M(!!K.recargoEquivalencia), K.formaPagoDefectoId && _(K.formaPagoDefectoId));
  }, [K]);
  const xt = x.useMemo(() => me.map((S, V) => ({ l: S, i: V })).filter(({ l: S }) => (S.productoId || S.descripcion.trim()) && S.cantidad > 0), [me]), Qt = x.useMemo(
    () => ({
      clienteId: g,
      fechaEmision: e.tipo === "factura" ? $ : null,
      serie: f || null,
      diasVencimiento: F,
      formaPagoId: k || null,
      recargoEquivalencia: C,
      porcentajeIrpf: b,
      conceptosDocumento: we ? [] : E,
      impuesto: L && !r ? q : null,
      descontarAnticipos: Y && G.length && !we ? G.map((S) => ({ anticipoId: S.id })) : null,
      moneda: we ? je : null,
      tasaCambio: we ? _e : null,
      lineas: xt.map(({ l: S }) => ({
        cantidad: S.cantidad,
        descripcion: S.descripcion.trim() || null,
        precioUnitario: S.precio,
        codigoIva: S.iva,
        porcentajeDescuento: S.dto,
        productoId: S.productoId,
        ...r || we ? { conceptos: [] } : S.conceptos === void 0 ? {} : { conceptos: S.conceptos }
      }))
    }),
    [g, $, f, F, k, C, b, E, xt, e.tipo, r, Y, G, L, q, we, je, _e]
  ), cr = Cn(Qt, 350);
  x.useEffect(() => {
    if (!cr.clienteId || cr.lineas.length === 0) {
      st(null), Et(cr.clienteId ? "" : "Elige el cliente para calcular precios e importes.");
      return;
    }
    const S = N();
    Te(!0), t.post("/facturas/simular", cr).then((V) => S() && (st(V), Et(""))).catch((V) => S() && (st(null), Et(V.message))).finally(() => S() && Te(!1));
  }, [cr, t]);
  const Nl = x.useMemo(() => {
    const S = me.map(() => {
    });
    return D && xt.forEach(({ i: V }, ee) => {
      const re = D.lineas[ee];
      re && (S[V] = { precio: re.precioDivisa ?? re.precioUnitario, dto: re.porcentajeDescuento, iva: re.codigoIva, importe: re.baseDivisa ?? re.base, margen: re.productoId || re.costeUnitario || re.costeConceptos ? re.margen : void 0, conceptos: re.conceptos });
    }), S;
  }, [D, me, xt]), Ad = x.useMemo(() => {
    const S = {};
    return me.forEach((V, ee) => {
      var re;
      return S[V.clave] = (((re = Nl[ee]) == null ? void 0 : re.conceptos) ?? []).filter((Z) => !Z.repartido).map((Z) => ({ conceptoId: Z.conceptoId, valor: Z.valor }));
    }), S;
  }, [me, Nl]);
  function Md(S, V) {
    ge(
      (ee) => ee.map(
        (re) => re.clave === S ? { ...re, productoId: V.id, referencia: V.referencia ?? V.nombre, descripcion: V.nombre, precio: null, dto: 0, iva: null, conceptos: void 0, unidad: V.unidad, stock: V.stock, controlarStock: V.controlarStock } : re
      )
    );
  }
  const $d = x.useMemo(() => {
    const S = /* @__PURE__ */ new Map();
    for (const V of (D == null ? void 0 : D.lineas) ?? []) {
      const ee = S.get(V.codigoIva) ?? { base: 0, cuota: 0, pct: V.porcentajeIva };
      ee.base += V.base, ee.cuota += V.cuotaIva, S.set(V.codigoIva, ee);
    }
    return [...S.entries()];
  }, [D]), Sl = ((D == null ? void 0 : D.lineas) ?? []).reduce((S, V) => S + (V.base - V.margen), 0), wl = D ? D.baseImponible - Sl : 0, Od = (S) => {
    var V;
    return ((V = o.find((ee) => ee.codigo === S)) == null ? void 0 : V.nombre) ?? S;
  };
  async function Qo() {
    if (D) {
      zt(!0);
      try {
        const S = xt.map(({ l: ee }, re) => {
          const Z = D.lineas[re];
          return {
            cantidad: ee.cantidad,
            descripcion: Z.descripcion,
            // En divisa, el precio que se fija es el de la divisa (el de euros es su contravalor).
            precioUnitario: Z.precioDivisa ?? Z.precioUnitario,
            codigoIva: Z.codigoIva,
            porcentajeDescuento: Z.porcentajeDescuento,
            productoId: ee.productoId,
            ...r || we ? {} : ee.conceptos === void 0 ? {} : { conceptos: ee.conceptos }
          };
        });
        let V;
        if (r)
          V = (await t.post(`/facturas/${e.semilla.rectificaId}/rectificar`, { motivo: Ae, lineas: S, fechaEmision: $, porcentajeIrpf: b, serie: f || null })).id;
        else if (e.tipo === "factura") {
          const ee = await t.post("/facturas", { ...Qt, lineas: S });
          if (V = ee.id, ct && Pe.length) {
            let re = 0;
            try {
              for (const Z of zd(Pe, ee.total))
                await t.post(`/anticipos/${Z.id}/aplicar`, { facturaId: V, importe: Z.importe }), re += Z.importe;
              n.aviso(`Factura emitida. Aplicados ${z(re)} de anticipos (asiento 438 a 430).`, "ok");
            } catch (Z) {
              n.aviso(`Factura emitida, pero no se pudo aplicar el anticipo: ${Z.message}`, "err");
            }
            e.alGuardar(V);
            return;
          }
        } else if (e.tipo === "presupuesto") {
          const ee = { clienteId: g, diasValidez: Q, lineas: S, conceptosDocumento: Qt.conceptosDocumento, impuesto: Qt.impuesto, moneda: Qt.moneda };
          V = e.id ? (await t.put(`/presupuestos/${e.id}`, ee)).id : (await t.post("/presupuestos", ee)).id;
        } else {
          const ee = { clienteId: g, fecha: $, lineas: S, conceptosDocumento: Qt.conceptosDocumento, impuesto: Qt.impuesto, moneda: Qt.moneda };
          V = e.id ? (await t.put(`/pedidos-venta/${e.id}`, ee)).id : (await t.post("/pedidos-venta", ee)).id;
        }
        n.aviso(e.tipo === "factura" ? "Factura emitida." : "Documento guardado.", "ok"), e.alGuardar(V);
      } catch (S) {
        n.aviso(S.message, "err");
      } finally {
        zt(!1), ve(!1);
      }
    }
  }
  const bd = r ? `Rectificativa de la factura ${((es = e.semilla) == null ? void 0 : es.rectificaNumero) ?? ""}` : `${e.id ? "Editar" : "Nuevo"} ${Tm[e.tipo]}`.replace("Nuevo factura", "Nueva factura"), Ud = !!D && !et && (!r || Ae.trim().length > 0);
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsxs("div", { className: "panel-head", children: [
        /* @__PURE__ */ l.jsx("h2", { children: bd }),
        /* @__PURE__ */ l.jsxs("div", { style: { display: "flex", gap: 8 }, children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: e.alCancelar, children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", disabled: !Ud || It, onClick: () => e.tipo === "factura" ? ve(!0) : Qo(), children: e.tipo === "factura" ? "Emitir factura" : "Guardar" })
        ] })
      ] }),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-cabecera", children: [
        /* @__PURE__ */ l.jsxs("div", { className: "dx-cab-campos", children: [
          /* @__PURE__ */ l.jsx(Bo, { terceros: a, valor: g, alCambiar: w, etiqueta: "Cliente", deshabilitado: r || e.tipo === "pedido" && !!e.id && !1 }),
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
                /* @__PURE__ */ l.jsxs("select", { value: k, onChange: (S) => _(S.target.value), children: [
                  /* @__PURE__ */ l.jsx("option", { value: "", children: "— Vencimiento a mano —" }),
                  c.map((S) => /* @__PURE__ */ l.jsx("option", { value: S.id, children: S.nombre }, S.id))
                ] })
              ] }),
              !k && /* @__PURE__ */ l.jsxs("div", { children: [
                /* @__PURE__ */ l.jsx("label", { children: "Vencimiento" }),
                /* @__PURE__ */ l.jsx("select", { value: F, onChange: (S) => T(Number(S.target.value)), children: [0, 15, 30, 45, 60, 90].map((S) => /* @__PURE__ */ l.jsx("option", { value: S, children: S ? `${S} días` : "Contado" }, S)) })
              ] })
            ] }),
            (!r || we) && /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
              /* @__PURE__ */ l.jsxs("div", { children: [
                /* @__PURE__ */ l.jsx("label", { children: "Moneda" }),
                /* @__PURE__ */ l.jsxs("select", { value: je, disabled: r, title: r ? "La rectificativa va en la divisa de la original" : void 0, onChange: (S) => he(S.target.value), children: [
                  /* @__PURE__ */ l.jsx("option", { value: "", children: "EUR · euros" }),
                  Td.map((S) => /* @__PURE__ */ l.jsx("option", { value: S, children: S }, S))
                ] })
              ] }),
              we && e.tipo === "factura" && /* @__PURE__ */ l.jsxs("div", { children: [
                /* @__PURE__ */ l.jsxs("label", { children: [
                  "Tipo de cambio (€ por 1 ",
                  je,
                  ")"
                ] }),
                /* @__PURE__ */ l.jsx("input", { type: "number", step: "0.000001", min: "0", value: _e ?? "", placeholder: "El del día", onChange: (S) => ot(S.target.value === "" ? null : Number(S.target.value)) })
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
            /* @__PURE__ */ l.jsx("input", { value: Ae, onChange: (S) => Be(S.target.value), placeholder: "Devolución, error en precio…" })
          ] })
        ] }),
        /* @__PURE__ */ l.jsx("div", { className: "dx-ficha", children: K ? /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("strong", { children: K.nombre }),
          /* @__PURE__ */ l.jsx("div", { className: "muted", children: [K.nifFiscal, K.poblacion, K.provincia].filter(Boolean).join(" · ") }),
          K.limiteRiesgo != null && /* @__PURE__ */ l.jsxs("div", { className: "muted", children: [
            "Límite de riesgo ",
            z(K.limiteRiesgo)
          ] }),
          K.tarifaId && /* @__PURE__ */ l.jsx("div", { className: "muted", children: "Con tarifa de precios propia" }),
          K.recargoEquivalencia && /* @__PURE__ */ l.jsx("div", { className: "muted", children: "En recargo de equivalencia" }),
          (D == null ? void 0 : D.avisoRiesgo) && /* @__PURE__ */ l.jsxs("div", { className: "dx-aviso", children: [
            "⚠ ",
            D.avisoRiesgo
          ] }),
          G.length > 0 && /* @__PURE__ */ l.jsxs("div", { className: "dx-anticipo", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              "🧾 Anticipos facturados pendientes de descontar: ",
              /* @__PURE__ */ l.jsx("strong", { children: z(G.reduce((S, V) => S + (V.disponibleBase ?? 0), 0)) }),
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
              /* @__PURE__ */ l.jsx("strong", { children: z(te) }),
              " en ",
              Pe.length === 1 ? "un anticipo pendiente" : `${Pe.length} anticipos pendientes`,
              " de aplicar."
            ] }),
            /* @__PURE__ */ l.jsxs("label", { className: "dx-check", children: [
              /* @__PURE__ */ l.jsx("input", { type: "checkbox", checked: ct, onChange: (S) => R(S.target.checked) }),
              "Aplicarlo al emitir",
              D ? ` (${z(Math.min(te, D.total))})` : ""
            ] })
          ] })
        ] }) : /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Elige un cliente: se aplican su tarifa, su forma de pago, su recargo y sus conceptos." }) })
      ] })
    ] }),
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsx(_d, { modo: "venta", lineas: me, alCambiar: ge, calculos: Nl, ivas: Ce, catalogo: r ? [] : m, sugeridos: Ad, alElegirArticulo: Md }),
      !r && m.length > 0 && /* @__PURE__ */ l.jsx("div", { style: { marginTop: 10 }, children: /* @__PURE__ */ l.jsx(Ho, { catalogo: m, lista: E, alCambiar: (S) => y(S ?? []), documento: !0 }) })
    ] }),
    /* @__PURE__ */ l.jsxs("div", { className: "dx-pie", children: [
      /* @__PURE__ */ l.jsxs("div", { className: "dx-estado", children: [
        et && /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Calculando…" }),
        !et && U && /* @__PURE__ */ l.jsx("span", { className: "dx-rojo", children: U }),
        (D == null ? void 0 : D.mencionFiscal) && /* @__PURE__ */ l.jsx("div", { className: "muted", style: { fontSize: 12 }, children: D.mencionFiscal })
      ] }),
      /* @__PURE__ */ l.jsxs("div", { className: "panel dx-totales", children: [
        $d.map(([S, V]) => /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
            Od(S),
            " · base ",
            de(V.base)
          ] }),
          /* @__PURE__ */ l.jsx("span", { children: z(V.cuota) })
        ] }, S)),
        D == null ? void 0 : D.lineas.filter((S) => S.anticipoId).map((S) => /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-tot-anticipo", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: S.descripcion }),
          /* @__PURE__ */ l.jsx("span", { children: z(S.base + S.cuotaIva + S.cuotaRecargo) })
        ] }, S.anticipoId)),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Base imponible" }),
          /* @__PURE__ */ l.jsx("span", { children: z(D == null ? void 0 : D.baseImponible) })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Impuestos" }),
          /* @__PURE__ */ l.jsx("span", { children: z(D == null ? void 0 : D.cuotaIva) })
        ] }),
        !!(D != null && D.recargoTotal) && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Recargo de equivalencia" }),
          /* @__PURE__ */ l.jsx("span", { children: z(D.recargoTotal) })
        ] }),
        !!(D != null && D.retencionIrpf) && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
            "Retención IRPF (",
            de(D.porcentajeIrpf),
            " %)"
          ] }),
          /* @__PURE__ */ l.jsxs("span", { children: [
            "−",
            z(D.retencionIrpf)
          ] })
        ] }),
        D != null && D.moneda ? /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
            /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
              "Contravalor en euros (1 ",
              D.moneda,
              " = ",
              String(D.tasaCambio ?? 0).replace(".", ","),
              " €)"
            ] }),
            /* @__PURE__ */ l.jsx("span", { children: z(D.total) })
          ] }),
          /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-grande", children: [
            /* @__PURE__ */ l.jsxs("span", { children: [
              "Total ",
              D.moneda
            ] }),
            /* @__PURE__ */ l.jsxs("span", { children: [
              de(D.totalDivisa ?? 0),
              " ",
              D.moneda
            ] })
          ] })
        ] }) : /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ l.jsx("span", { children: "Total" }),
          /* @__PURE__ */ l.jsx("span", { children: z(D == null ? void 0 : D.total) })
        ] }),
        D && Sl > 0 && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Coste · margen" }),
          /* @__PURE__ */ l.jsxs("span", { className: wl < 0 ? "dx-rojo" : "muted", children: [
            z(Sl),
            " · ",
            z(wl),
            " (",
            de(D.baseImponible ? wl / D.baseImponible * 100 : 0),
            " %)"
          ] })
        ] })
      ] })
    ] }),
    vn && D && /* @__PURE__ */ l.jsx(
      fn,
      {
        titulo: r ? "Emitir la rectificativa" : "Emitir la factura",
        alCerrar: () => ve(!1),
        acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => ve(!1), children: "Revisar" }),
          /* @__PURE__ */ l.jsxs("button", { className: "btn small", disabled: It, onClick: Qo, children: [
            "Emitir ",
            D.moneda ? `${de(D.totalDivisa ?? 0)} ${D.moneda}` : z(D.total)
          ] })
        ] }),
        children: /* @__PURE__ */ l.jsxs("p", { style: { margin: 0 }, children: [
          "Se emitirá una factura ",
          r ? "rectificativa " : "",
          "de ",
          /* @__PURE__ */ l.jsx("strong", { children: D.moneda ? `${de(D.totalDivisa ?? 0)} ${D.moneda} (${z(D.total)})` : z(D.total) }),
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
function zm(e) {
  var Be, me, ge;
  const { api: t, anfitrion: n } = Dt(), [r, a] = x.useState([]), [i, o] = x.useState([]), [s, c] = x.useState(((Be = e.semilla) == null ? void 0 : Be.proveedorId) ?? ""), [d, j] = x.useState(((me = e.semilla) == null ? void 0 : me.fecha) ?? Ct()), [u, m] = x.useState([_r()]), [h, g] = x.useState([]), [w, $] = x.useState(null), [p, f] = x.useState(""), [v, k] = x.useState(!1), _ = na();
  x.useEffect(() => {
    t.get("/proveedores").then(a).catch(() => a([])), t.get("/conceptos-linea?ambito=Compras&activos=true").then(o).catch(() => o([]));
  }, [t]), x.useEffect(() => {
    const E = e.semilla;
    if (!E) return;
    const y = E.lineas.map((q) => ({ ...q, porcentajeDescuento: 0, codigoIva: "" })), { porLinea: L, documento: B } = Ld(y), W = E.lineas.map((q, Se) => ({ clave: ta(), productoId: q.productoId ?? null, descripcion: q.descripcion, cantidad: q.cantidad, precio: q.precioUnitario, dto: 0, iva: null, conceptos: L[Se] }));
    m(W), g(B), Promise.all(W.map((q) => q.productoId ? t.get(`/productos/${q.productoId}`).catch(() => null) : Promise.resolve(null))).then(
      (q) => m((Se) => Se.map((je, he) => q[he] ? { ...je, referencia: q[he].referencia ?? q[he].nombre, unidad: q[he].unidadCompra || q[he].unidad, stock: q[he].stock, controlarStock: q[he].controlarStock } : je))
    );
  }, [e.semilla, t]);
  const F = r.find((E) => E.id === s), T = x.useMemo(() => u.map((E, y) => ({ l: E, i: y })).filter(({ l: E }) => E.descripcion.trim() && E.cantidad > 0), [u]), C = x.useMemo(
    () => {
      var E, y;
      return {
        proveedorId: s || null,
        proveedorTexto: (F == null ? void 0 : F.nombre) ?? (((E = e.semilla) == null ? void 0 : E.proveedorTexto) || null),
        solicitudOrigenId: e.id ? null : ((y = e.semilla) == null ? void 0 : y.solicitudOrigenId) ?? null,
        fecha: d,
        conceptosDocumento: h,
        lineas: T.map(({ l: L }) => ({ descripcion: L.descripcion.trim(), cantidad: L.cantidad, precioUnitario: L.precio ?? 0, productoId: L.productoId, ...L.conceptos === void 0 ? {} : { conceptos: L.conceptos } }))
      };
    },
    [s, F, d, h, T, e.id, e.semilla]
  ), M = Cn(C, 350);
  x.useEffect(() => {
    if (!M.proveedorId || M.lineas.length === 0) {
      $(null), f(M.proveedorId ? "" : "Elige el proveedor.");
      return;
    }
    const E = _();
    t.post("/compras/pedidos/simular", M).then((y) => E() && ($(y), f(""))).catch((y) => E() && ($(null), f(y.message)));
  }, [M, t]);
  const b = x.useMemo(() => {
    const E = u.map(() => {
    });
    return T.forEach(({ i: y }, L) => {
      const B = w == null ? void 0 : w.lineas[L];
      B && (E[y] = { precio: B.precioUnitario, importe: B.importe, costeUnitarioEntrada: B.costeUnitarioEntrada, conceptos: B.conceptos });
    }), E;
  }, [w, u, T]), P = x.useMemo(() => {
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
  const Ae = ((w == null ? void 0 : w.lineas) ?? []).reduce((E, y) => E + y.costeConceptos, 0);
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
          /* @__PURE__ */ l.jsx(Bo, { terceros: r, valor: s, alCambiar: c, etiqueta: "Proveedor", deshabilitado: !!e.id }),
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
      /* @__PURE__ */ l.jsx(_d, { modo: "compra", lineas: u, alCambiar: m, calculos: b, ivas: [], catalogo: i, sugeridos: P, alElegirArticulo: Q }),
      i.length > 0 && /* @__PURE__ */ l.jsx("div", { style: { marginTop: 10 }, children: /* @__PURE__ */ l.jsx(Ho, { catalogo: i, lista: h, alCambiar: (E) => g(E ?? []), documento: !0 }) })
    ] }),
    /* @__PURE__ */ l.jsxs("div", { className: "dx-pie", children: [
      /* @__PURE__ */ l.jsx("div", { className: "dx-estado", children: p && /* @__PURE__ */ l.jsx("span", { className: "dx-rojo", children: p }) }),
      /* @__PURE__ */ l.jsxs("div", { className: "panel dx-totales", children: [
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ l.jsx("span", { children: "Total del pedido (sin impuestos)" }),
          /* @__PURE__ */ l.jsx("span", { children: z(w == null ? void 0 : w.total) })
        ] }),
        Ae !== 0 && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Costes añadidos al almacén" }),
          /* @__PURE__ */ l.jsx("span", { children: z(Ae) })
        ] }),
        /* @__PURE__ */ l.jsx("div", { className: "muted", style: { fontSize: 12, marginTop: 6 }, children: "El impuesto se aplica al facturar el pedido." })
      ] })
    ] })
  ] });
}
function yl(e) {
  return /* @__PURE__ */ l.jsxs("div", { className: "panel-head", children: [
    /* @__PURE__ */ l.jsxs("h2", { style: { display: "flex", alignItems: "center", gap: 10 }, children: [
      /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: e.volver, title: "Volver a la lista", children: "←" }),
      e.titulo,
      e.estado && /* @__PURE__ */ l.jsx("span", { className: Vo(e.estado), children: e.estado }),
      e.extra
    ] }),
    /* @__PURE__ */ l.jsx("div", { className: "dx-acciones", children: e.acciones })
  ] });
}
function Ue(e) {
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-dato", children: [
    /* @__PURE__ */ l.jsx("small", { children: e.etiqueta }),
    /* @__PURE__ */ l.jsx("div", { children: e.children })
  ] });
}
function Zn(e) {
  return /* @__PURE__ */ l.jsx("button", { type: "button", className: "dx-enlace", onClick: e.alPulsar, children: e.children });
}
function jl(e) {
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
function Lm(e) {
  const { api: t, anfitrion: n, navegar: r } = Dt(), { dato: a, error: i, recargar: o } = jl(() => t.get(`/facturas/${e.id}`)), [s, c] = x.useState(null), [d, j] = x.useState(!1), [u, m] = x.useState("");
  x.useEffect(() => void t.get(`/facturas/${e.id}/saldo`).then(c).catch(() => c(null)), [t, e.id, a]);
  const [h, g] = x.useState([]), [w, $] = x.useState([]), [p, f] = x.useState(null);
  x.useEffect(() => {
    !(a != null && a.clienteId) || a.estado !== "Emitida" || t.get(`/anticipos?clienteId=${a.clienteId}`).then((C) => {
      g(Wo(C)), $(Dd(C));
    }).catch(() => g([]));
  }, [t, a]);
  const v = h.reduce((C, M) => C + M.disponible, 0);
  if (i) return /* @__PURE__ */ l.jsx("div", { className: "panel", children: /* @__PURE__ */ l.jsx("p", { className: "dx-rojo", children: i }) });
  if (!a) return /* @__PURE__ */ l.jsx("div", { className: "muted", children: "Cargando…" });
  const k = { clienteId: a.clienteId ?? void 0, lineas: a.lineas.map((C) => ({ ...C, precioUnitario: C.precioDivisa ?? C.precioUnitario })), moneda: a.moneda, tasaCambio: a.tasaCambio }, _ = a.lineas.reduce((C, M) => C + (M.base - M.margen), 0), F = a.estado === "Emitida", T = a.lineas.some((C) => C.cuentaContable === "438" && !C.anticipoId);
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsx(
        yl,
        {
          titulo: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            a.tipo === "Rectificativa" ? "Rectificativa" : a.tipo === "Simplificada" ? "Ticket" : "Factura",
            " ",
            /* @__PURE__ */ l.jsx("span", { className: "mono", children: a.numeroCompleto })
          ] }),
          estado: a.estado,
          extra: T ? /* @__PURE__ */ l.jsx("span", { className: "pill part", children: "Factura de anticipo" }) : a.lineas.some((C) => C.anticipoId) ? /* @__PURE__ */ l.jsx("span", { className: "pill", children: "Descuenta anticipos" }) : null,
          volver: () => r({ tipo: "factura", pantalla: "lista" }),
          acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => qr(n, `/facturas/${a.id}/pdf`).catch((C) => n.aviso(C.message, "err")), children: "PDF" }),
            a.tipo !== "Simplificada" && a.clienteNif && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => qr(n, `/facturas/${a.id}/facturae.xml`).catch((C) => n.aviso(C.message, "err")), children: "Facturae" }),
            a.estado !== "Borrador" && a.tipo !== "Simplificada" && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", title: "Factura EDIFACT INVOIC (EANCOM) para clientes con EDI", onClick: () => gm(n, `/integraciones/edi/facturas/${a.id}/invoic`).catch((C) => n.aviso(C.message, "err")), children: "EDI" }),
            /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "factura", pantalla: "editor", semilla: k }), children: "Duplicar" }),
            F && a.tipo === "Ordinaria" && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "factura", pantalla: "editor", semilla: { ...k, rectificaId: a.id, rectificaNumero: a.numeroCompleto } }), children: "Rectificar" }),
            F && /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", onClick: () => j(!0), children: "Anular" })
          ] })
        }
      ),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ l.jsxs(Ue, { etiqueta: "Cliente", children: [
          /* @__PURE__ */ l.jsx("strong", { children: a.clienteNombre }),
          a.clienteNif && /* @__PURE__ */ l.jsx("div", { className: "muted mono", children: a.clienteNif }),
          /* @__PURE__ */ l.jsx("div", { className: "muted", children: [a.clienteCalle, a.clienteCodigoPostal, a.clientePoblacion].filter(Boolean).join(" ") })
        ] }),
        /* @__PURE__ */ l.jsxs(Ue, { etiqueta: "Emisión", children: [
          Ee(a.fechaEmision),
          a.fechaOperacion !== a.fechaEmision && /* @__PURE__ */ l.jsxs("div", { className: "muted", children: [
            "Operación ",
            Ee(a.fechaOperacion)
          ] })
        ] }),
        /* @__PURE__ */ l.jsx(Ue, { etiqueta: "Vencimiento", children: Ee(a.fechaVencimiento) }),
        /* @__PURE__ */ l.jsxs(Ue, { etiqueta: "Cobro", children: [
          s ? s.pendiente <= 0 ? /* @__PURE__ */ l.jsx("span", { className: "pill ok", children: "Cobrada" }) : /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            "Pendiente ",
            /* @__PURE__ */ l.jsx("strong", { children: z(s.pendiente) }),
            s.liquidado > 0 && /* @__PURE__ */ l.jsxs("div", { className: "muted", children: [
              "Cobrado ",
              z(s.liquidado)
            ] })
          ] }) : "—",
          s && s.pendiente > 0 && F && n.irA && /* @__PURE__ */ l.jsx("div", { children: /* @__PURE__ */ l.jsx(Zn, { alPulsar: () => n.irA("cobros"), children: "Registrar cobro" }) }),
          s && s.pendiente > 0 && F && w.length > 0 && !T && /* @__PURE__ */ l.jsxs("div", { className: "dx-anticipo", children: [
            "🧾 Anticipos facturados sin descontar: ",
            /* @__PURE__ */ l.jsx("strong", { children: z(w.reduce((C, M) => C + (M.disponibleBase ?? 0), 0)) }),
            " de base. Se descuentan al hacer la siguiente factura (o rectifica esta para incluirlos)."
          ] }),
          s && s.pendiente > 0 && F && v > 0 && /* @__PURE__ */ l.jsxs("div", { className: "dx-anticipo", children: [
            "💶 Anticipos del cliente sin aplicar: ",
            /* @__PURE__ */ l.jsx("strong", { children: z(v) }),
            /* @__PURE__ */ l.jsx("div", { children: /* @__PURE__ */ l.jsx(Zn, { alPulsar: () => f(zd(h, s.pendiente)), children: "Aplicar a esta factura" }) })
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
        /* @__PURE__ */ l.jsx("tbody", { children: a.lineas.map((C, M) => /* @__PURE__ */ l.jsxs("tr", { children: [
          /* @__PURE__ */ l.jsxs("td", { children: [
            C.descripcion,
            /* @__PURE__ */ l.jsx(gl, { conceptos: C.conceptos })
          ] }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: We(C.cantidad) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: z(C.precioUnitario) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: C.porcentajeDescuento ? `${de(C.porcentajeDescuento)} %` : "" }),
          /* @__PURE__ */ l.jsxs("td", { children: [
            C.codigoIva,
            " · ",
            de(C.porcentajeIva),
            " %"
          ] }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: z(C.base) }) }),
          /* @__PURE__ */ l.jsx("td", { className: "num muted", children: C.costeUnitario || C.costeConceptos ? z(C.margen) : "" })
        ] }, M)) })
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
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ l.jsx("span", { children: "Total" }),
          /* @__PURE__ */ l.jsx("span", { children: z(a.total) })
        ] }),
        _ > 0 && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Margen" }),
          /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
            z(a.baseImponible - _),
            " (",
            de(a.baseImponible ? (a.baseImponible - _) / a.baseImponible * 100 : 0),
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
      fn,
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
              const M = p.find((b) => b.id === C.id);
              return /* @__PURE__ */ l.jsxs("tr", { children: [
                /* @__PURE__ */ l.jsx("td", { children: Ee(C.fecha) }),
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
      fn,
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
function Am(e) {
  const { api: t, anfitrion: n, navegar: r } = Dt(), { dato: a, error: i, recargar: o } = jl(() => t.get(`/presupuestos/${e.id}`));
  if (i) return /* @__PURE__ */ l.jsx("div", { className: "panel", children: /* @__PURE__ */ l.jsx("p", { className: "dx-rojo", children: i }) });
  if (!a) return /* @__PURE__ */ l.jsx("div", { className: "muted", children: "Cargando…" });
  const s = a.estado === "Borrador", c = { clienteId: a.clienteId, lineas: a.lineas, moneda: a.moneda };
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsx(
        yl,
        {
          titulo: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            "Presupuesto ",
            /* @__PURE__ */ l.jsx("span", { className: "mono", children: a.numeroCompleto })
          ] }),
          estado: a.estado,
          volver: () => r({ tipo: "presupuesto", pantalla: "lista" }),
          acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => qr(n, `/presupuestos/${a.id}/pdf`).catch((d) => n.aviso(d.message, "err")), children: "PDF" }),
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
        /* @__PURE__ */ l.jsx(Ue, { etiqueta: "Cliente", children: /* @__PURE__ */ l.jsx("strong", { children: a.clienteNombre }) }),
        /* @__PURE__ */ l.jsx(Ue, { etiqueta: "Fecha", children: Ee(a.fecha) }),
        /* @__PURE__ */ l.jsx(Ue, { etiqueta: "Válido hasta", children: Ee(a.validez) }),
        /* @__PURE__ */ l.jsx(Ue, { etiqueta: "Factura", children: a.facturaId ? /* @__PURE__ */ l.jsx(Zn, { alPulsar: () => r({ tipo: "factura", pantalla: "vista", id: a.facturaId }), children: "ver la factura" }) : "—" })
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
            /* @__PURE__ */ l.jsx(gl, { conceptos: d.conceptos })
          ] }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: We(d.cantidad) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: Zt(d.precioUnitario, a.moneda) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: d.porcentajeDescuento ? `${de(d.porcentajeDescuento)} %` : "" }),
          /* @__PURE__ */ l.jsx("td", { children: d.codigoIva }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: Zt(d.base, a.moneda) }) })
        ] }, j)) })
      ] }),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-totales-vista", children: [
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Base imponible" }),
          /* @__PURE__ */ l.jsx("span", { children: Zt(a.baseImponible, a.moneda) })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Impuestos" }),
          /* @__PURE__ */ l.jsx("span", { children: Zt(a.cuotaIva, a.moneda) })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ l.jsx("span", { children: "Total" }),
          /* @__PURE__ */ l.jsx("span", { children: Zt(a.total, a.moneda) })
        ] })
      ] })
    ] })
  ] });
}
function Mm(e) {
  const { api: t, anfitrion: n, navegar: r } = Dt(), { dato: a, error: i, recargar: o } = jl(() => t.get(`/pedidos-venta/${e.id}`)), [s, c] = x.useState([]), [d, j] = x.useState([]), [u, m] = x.useState(null), [h, g] = x.useState(""), [w, $] = x.useState(Ct()), [p, f] = x.useState(!1), [v, k] = x.useState(Ct()), [_, F] = x.useState("");
  if (x.useEffect(() => void t.get(`/pedidos-venta/${e.id}/albaranes`).then(c).catch(() => c([])), [t, e.id, a]), x.useEffect(() => void t.get("/formas-pago").then((P) => j(P.filter((Q) => Q.activo))).catch(() => j([])), [t]), i) return /* @__PURE__ */ l.jsx("div", { className: "panel", children: /* @__PURE__ */ l.jsx("p", { className: "dx-rojo", children: i }) });
  if (!a) return /* @__PURE__ */ l.jsx("div", { className: "muted", children: "Cargando…" });
  const T = (a.estado === "Borrador" || a.estado === "Confirmado") && a.lineas.every((P) => P.cantidadServida === 0), C = a.lineas.some((P) => P.pendienteServir > 0), M = a.estado !== "Cancelado" && a.estado !== "Facturado", b = { clienteId: a.clienteId, fecha: a.fecha, lineas: a.lineas, moneda: a.moneda };
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsx(
        yl,
        {
          titulo: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            "Pedido de venta ",
            /* @__PURE__ */ l.jsx("span", { className: "mono", children: a.numeroCompleto })
          ] }),
          estado: a.estado,
          volver: () => r({ tipo: "pedido", pantalla: "lista" }),
          acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => qr(n, `/pedidos-venta/${a.id}/pdf`).catch((P) => n.aviso(P.message, "err")), children: "PDF" }),
            /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "pedido", pantalla: "editor", semilla: { ...b, fecha: void 0 } }), children: "Duplicar" }),
            T && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "pedido", pantalla: "editor", id: a.id, semilla: b }), children: "Editar" }),
            a.estado === "Borrador" && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: async () => await ft(() => t.post(`/pedidos-venta/${a.id}/confirmar`), n.aviso, "Pedido confirmado.") && o(), children: "Confirmar" }),
            M && a.estado !== "Borrador" && C && n.reservarPales && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", title: "Apartar palés cerrados para este pedido", onClick: () => n.reservarPales(a.id), children: "Reservar palés" }),
            M && a.estado !== "Borrador" && C && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => m(Object.fromEntries(a.lineas.map((P) => [P.id, P.pendienteServir]))), children: "Entregar (albarán)" }),
            M && a.estado !== "Borrador" && /* @__PURE__ */ l.jsx("button", { className: "btn small", onClick: () => f(!0), children: "Facturar" }),
            M && /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", onClick: async () => window.confirm("¿Cancelar el pedido?") && await ft(() => t.post(`/pedidos-venta/${a.id}/cancelar`), n.aviso, "Pedido cancelado.") && o(), children: "Cancelar" })
          ] })
        }
      ),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ l.jsx(Ue, { etiqueta: "Cliente", children: /* @__PURE__ */ l.jsx("strong", { children: a.clienteNombre }) }),
        /* @__PURE__ */ l.jsx(Ue, { etiqueta: "Fecha", children: Ee(a.fecha) }),
        /* @__PURE__ */ l.jsx(Ue, { etiqueta: "Viene de", children: a.presupuestoOrigenId ? /* @__PURE__ */ l.jsx(Zn, { alPulsar: () => r({ tipo: "presupuesto", pantalla: "vista", id: a.presupuestoOrigenId }), children: "presupuesto" }) : "—" }),
        /* @__PURE__ */ l.jsx(Ue, { etiqueta: "Factura", children: a.facturaId ? /* @__PURE__ */ l.jsx(Zn, { alPulsar: () => r({ tipo: "factura", pantalla: "vista", id: a.facturaId }), children: "ver la factura" }) : "—" })
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
            /* @__PURE__ */ l.jsx(gl, { conceptos: P.conceptos })
          ] }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: We(P.cantidad) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: We(P.cantidadServida) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: P.pendienteServir > 0 ? /* @__PURE__ */ l.jsx("strong", { children: We(P.pendienteServir) }) : "—" }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: Zt(P.precioUnitario, a.moneda) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: P.porcentajeDescuento ? `${de(P.porcentajeDescuento)} %` : "" }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: Zt(P.base, a.moneda) }) })
        ] }, P.id)) })
      ] }),
      /* @__PURE__ */ l.jsx("div", { className: "dx-totales-vista", children: /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-grande", children: [
        /* @__PURE__ */ l.jsx("span", { children: "Total (sin impuestos)" }),
        /* @__PURE__ */ l.jsx("span", { children: Zt(a.total, a.moneda) })
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
          /* @__PURE__ */ l.jsx("td", { children: Ee(P.fecha) }),
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
      fn,
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
      fn,
      {
        titulo: `Facturar el pedido ${a.numeroCompleto}`,
        alCerrar: () => f(!1),
        acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => f(!1), children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", onClick: async () => {
            try {
              const P = await t.post(`/pedidos-venta/${a.id}/facturar`, { fechaEmision: v, formaPagoId: _ || null });
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
              /* @__PURE__ */ l.jsxs("select", { value: _, onChange: (P) => F(P.target.value), children: [
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
function $m(e) {
  const { api: t, anfitrion: n, navegar: r } = Dt(), { dato: a, error: i, recargar: o } = jl(() => t.get(`/compras/pedidos/${e.id}`)), [s, c] = x.useState([]), [d, j] = x.useState([]), [u, m] = x.useState([]), [h, g] = x.useState(null), [w, $] = x.useState(""), [p, f] = x.useState(""), [v, k] = x.useState(Ct()), [_, F] = x.useState(!1), [T, C] = x.useState("IVA21"), [M, b] = x.useState(0), [P, Q] = x.useState(""), [ce, Ae] = x.useState(Ct());
  if (x.useEffect(() => void t.get(`/compras/pedidos/${e.id}/albaranes`).then(c).catch(() => c([])), [t, e.id, a]), x.useEffect(() => {
    t.get("/inventario/almacenes").then((y) => (j(y), y[0] && $(y[0].id))).catch(() => j([])), t.get("/tipos-iva").then((y) => m(y.filter((L) => L.activo))).catch(() => m([]));
  }, [t]), i) return /* @__PURE__ */ l.jsx("div", { className: "panel", children: /* @__PURE__ */ l.jsx("p", { className: "dx-rojo", children: i }) });
  if (!a) return /* @__PURE__ */ l.jsx("div", { className: "muted", children: "Cargando…" });
  const Be = (a.estado === "Borrador" || a.estado === "Confirmado") && a.lineas.every((y) => y.cantidadRecibida === 0 && y.cantidadFacturada === 0) && !a.empresaOrigenId, me = a.estado !== "Cancelado" && a.estado !== "Facturado", ge = a.lineas.some((y) => y.pendienteRecibir > 0), E = a.lineas.reduce((y, L) => y + L.costeConceptos, 0);
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsx(
        yl,
        {
          titulo: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            "Pedido de compra ",
            /* @__PURE__ */ l.jsx("span", { className: "mono", children: a.numeroCompleto })
          ] }),
          estado: a.estado,
          volver: () => r({ tipo: "compra", pantalla: "lista" }),
          acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => qr(n, `/compras/pedidos/${a.id}/pdf`).catch((y) => n.aviso(y.message, "err")), children: "PDF" }),
            !a.empresaOrigenId && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "compra", pantalla: "editor", semilla: { ...a, fecha: Ct() } }), children: "Duplicar" }),
            Be && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "compra", pantalla: "editor", id: a.id, semilla: a }), children: "Editar" }),
            a.estado === "Borrador" && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: async () => await ft(() => t.post(`/compras/pedidos/${a.id}/confirmar`), n.aviso, "Pedido confirmado.") && o(), children: "Confirmar" }),
            me && a.estado !== "Borrador" && ge && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => g(Object.fromEntries(a.lineas.map((y) => [y.id, { cantidad: y.pendienteRecibir, lote: "" }]))), children: "Recibir mercancía" }),
            me && a.estado !== "Borrador" && /* @__PURE__ */ l.jsx("button", { className: "btn small", onClick: () => F(!0), children: "Facturar" }),
            me && !a.empresaOrigenId && /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", onClick: async () => window.confirm("¿Cancelar el pedido?") && await ft(() => t.post(`/compras/pedidos/${a.id}/cancelar`), n.aviso, "Pedido cancelado.") && o(), children: "Cancelar" })
          ] })
        }
      ),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ l.jsx(Ue, { etiqueta: "Proveedor", children: /* @__PURE__ */ l.jsx("strong", { children: a.proveedorTexto }) }),
        /* @__PURE__ */ l.jsx(Ue, { etiqueta: "Fecha", children: Ee(a.fecha) }),
        /* @__PURE__ */ l.jsx(Ue, { etiqueta: "Total", children: z(a.total) }),
        /* @__PURE__ */ l.jsx(Ue, { etiqueta: "Costes añadidos", children: E ? z(E) : "—" })
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
          /* @__PURE__ */ l.jsx(gl, { conceptos: y.conceptos })
        ] }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: We(y.cantidad) }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: We(y.cantidadRecibida) }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: y.pendienteRecibir > 0 ? /* @__PURE__ */ l.jsx("strong", { children: We(y.pendienteRecibir) }) : "—" }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: z(y.precioUnitario) }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: z(y.importe) }) }),
        /* @__PURE__ */ l.jsxs("td", { className: "num muted", children: [
          z(y.costeUnitarioEntrada),
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
            /* @__PURE__ */ l.jsx("td", { children: Ee(y.fecha) }),
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
      fn,
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
    _ && /* @__PURE__ */ l.jsxs(
      fn,
      {
        titulo: `Facturar el pedido ${a.numeroCompleto}`,
        alCerrar: () => F(!1),
        acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => F(!1), children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", onClick: async () => await ft(() => t.post(`/compras/pedidos/${a.id}/facturar`, { codigoIva: T, porcentajeIrpf: M, numeroFactura: P || null, fechaFactura: ce }), n.aviso, "Factura del proveedor registrada como gasto.") && (F(!1), o()), children: "Registrar factura" })
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
              /* @__PURE__ */ l.jsx("input", { value: P, onChange: (y) => Q(y.target.value), autoFocus: !0 })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Fecha de la factura" }),
              /* @__PURE__ */ l.jsx("input", { type: "date", value: ce, onChange: (y) => Ae(y.target.value) })
            ] })
          ] }),
          /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Impuesto" }),
              /* @__PURE__ */ l.jsx("select", { value: T, onChange: (y) => C(y.target.value), children: u.map((y) => /* @__PURE__ */ l.jsx("option", { value: y.codigo, children: y.nombre }, y.codigo)) })
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
const Jl = (e = "") => ({ clave: ta(), descripcion: "", cuentaGasto: "", base: 0, codigoIva: e, porcentajeIva: null, porcentajeDeducible: 100 }), Om = (e) => e === "InversionSujetoPasivo" || e === "Intracomunitario";
function bm(e) {
  const { api: t, anfitrion: n } = Dt(), r = e.semilla, [a, i] = x.useState([]), [o, s] = x.useState([]), [c, d] = x.useState([]), [j, u] = x.useState([]), [m, h] = x.useState((r == null ? void 0 : r.proveedorId) ?? ""), [g, w] = x.useState(e.id ? (r == null ? void 0 : r.numeroFactura) ?? "" : ""), [$, p] = x.useState((r == null ? void 0 : r.fechaFactura) ?? Ct()), [f, v] = x.useState(e.id ? (r == null ? void 0 : r.fecha) ?? Ct() : Ct()), [k, _] = x.useState(r != null && r.concepto && !r.concepto.startsWith("Factura ") ? r.concepto : ""), [F, T] = x.useState((r == null ? void 0 : r.porcentajeIrpf) ?? 0), [C, M] = x.useState(""), [b, P] = x.useState(((r == null ? void 0 : r.recargoTotal) ?? 0) > 0), Q = !!(r != null && r.esRectificativa), [ce, Ae] = x.useState((r == null ? void 0 : r.numeroRectificado) ?? ""), [Be, me] = x.useState((r == null ? void 0 : r.fechaRectificada) ?? ""), [ge, E] = x.useState((r == null ? void 0 : r.motivoRectificacion) ?? ""), [y, L] = x.useState(!1), [B, W] = x.useState((r == null ? void 0 : r.afectacion) ?? "Comun"), [q, Se] = x.useState((r == null ? void 0 : r.moneda) ?? ""), [je, he] = x.useState((r == null ? void 0 : r.tasaCambio) ?? null), _e = (N) => r != null && r.moneda && r.tasaCambio ? be(N / r.tasaCambio) : N, [ot, we] = x.useState(
    () => {
      var N;
      return (N = r == null ? void 0 : r.lineas) != null && N.length ? r.lineas.map((H) => ({ clave: ta(), descripcion: H.descripcion ?? "", cuentaGasto: H.cuentaGasto ?? "", base: _e(H.base), codigoIva: H.codigoIva, porcentajeIva: H.autoliquidada ? H.porcentajeIva : null, porcentajeDeducible: H.porcentajeDeducible })) : [Jl()];
    }
  ), [D, st] = x.useState(e.id && (r != null && r.vencimientos) && r.vencimientos.length > 1 ? r.vencimientos : null), [U, Et] = x.useState(null), [et, Te] = x.useState(""), [It, zt] = x.useState(!1), vn = na();
  x.useEffect(() => {
    t.get("/proveedores").then(i).catch(() => i([])), t.get("/tipos-iva").then((N) => s(N.filter((H) => H.activo))).catch(() => s([])), t.get("/formas-pago").then((N) => d(N.filter((H) => H.activo))).catch(() => d([])), t.get("/empresas/actual").then((N) => {
      N.regimenIva === "RecargoEquivalencia" && (L(!0), r || P(!0));
    }).catch(() => {
    }), t.get("/contabilidad/cuentas").then((N) => u(N.filter((H) => H.codigo.startsWith("6") || H.codigo.startsWith("2")))).catch(() => u([]));
  }, [t]);
  const ve = a.find((N) => N.id === m);
  x.useEffect(() => {
    ve != null && ve.formaPagoDefectoId && !C && M(ve.formaPagoDefectoId);
  }, [ve]);
  const Pe = x.useMemo(
    () => ({
      proveedorId: m || null,
      proveedorTexto: (ve == null ? void 0 : ve.nombre) ?? null,
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
      vencimientos: D,
      rectificaGastoId: Q ? (r == null ? void 0 : r.rectificaGastoId) ?? null : null,
      numeroRectificado: Q && ce.trim() || null,
      fechaRectificada: Q && Be || null,
      motivoRectificacion: Q && ge.trim() || null,
      moneda: q || null,
      tasaCambio: q ? je : null
    }),
    [m, ve, g, $, f, k, F, C, b, B, ot, D, Q, r, ce, Be, ge, q, je]
  ), Lt = Cn(Pe, 350);
  x.useEffect(() => {
    if (!Lt.lineas.length) {
      Et(null), Te("Añade al menos una línea con base.");
      return;
    }
    const N = vn();
    t.post("/gastos/simular", Lt).then((H) => N() && (Et(H), Te(""))).catch((H) => N() && (Et(null), Te(H.message)));
  }, [Lt, t]);
  const ct = (N, H) => we((Ce) => Ce.map((K) => K.clave === N ? { ...K, ...H } : K)), R = (N) => o.find((H) => H.codigo === N), G = (N) => {
    var H;
    return (H = U == null ? void 0 : U.lineas) == null ? void 0 : H[ot.filter((Ce) => Ce.base !== 0).indexOf(N)];
  };
  function A(N) {
    if (!U) return;
    const H = /* @__PURE__ */ new Date(($ || f) + "T00:00:00"), Ce = be(U.total / N);
    st(Array.from({ length: N }, (K, te) => {
      const xt = new Date(H);
      return xt.setMonth(xt.getMonth() + te + 1), { fecha: xt.toISOString().slice(0, 10), importe: te === N - 1 ? be(U.total - Ce * (N - 1)) : Ce };
    }));
  }
  async function Y() {
    zt(!0);
    try {
      const N = e.id ? await t.put(`/gastos/${e.id}`, Pe) : await t.post("/gastos", Pe);
      n.aviso(e.id ? "Factura corregida." : "Factura registrada.", "ok"), N.avisoRiesgo && n.aviso(N.avisoRiesgo, "err"), e.alGuardar(N.id);
    } catch (N) {
      n.aviso(N.message, "err");
    } finally {
      zt(!1);
    }
  }
  const le = be((D ?? []).reduce((N, H) => N + (Number(H.importe) || 0), 0));
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
          /* @__PURE__ */ l.jsx(Bo, { terceros: a, valor: m, alCambiar: h, etiqueta: "Proveedor" }),
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
              /* @__PURE__ */ l.jsx("input", { type: "number", step: "0.01", value: F, onChange: (N) => T(Number(N.target.value)) })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Moneda de la factura" }),
              /* @__PURE__ */ l.jsxs("select", { value: q, onChange: (N) => Se(N.target.value), children: [
                /* @__PURE__ */ l.jsx("option", { value: "", children: "EUR · euros" }),
                Td.map((N) => /* @__PURE__ */ l.jsx("option", { value: N, children: N }, N))
              ] })
            ] }),
            q && /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsxs("label", { children: [
                "Tipo de cambio (€ por 1 ",
                q,
                ")"
              ] }),
              /* @__PURE__ */ l.jsx("input", { type: "number", step: "0.000001", min: "0", value: je ?? "", placeholder: "El del día de la factura", onChange: (N) => he(N.target.value === "" ? null : Number(N.target.value)) })
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
            /* @__PURE__ */ l.jsx("input", { value: k, onChange: (N) => _(N.target.value) })
          ] }) }),
          Q && /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Factura que rectifica" }),
              /* @__PURE__ */ l.jsx("input", { value: ce, disabled: !!(r != null && r.rectificaGastoId), onChange: (N) => Ae(N.target.value) })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Fecha de la rectificada" }),
              /* @__PURE__ */ l.jsx("input", { type: "date", value: Be, disabled: !!(r != null && r.rectificaGastoId), onChange: (N) => me(N.target.value) })
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
        /* @__PURE__ */ l.jsx("div", { className: "dx-ficha", children: ve ? /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("strong", { children: ve.nombre }),
          /* @__PURE__ */ l.jsx("div", { className: "muted", children: [ve.nifFiscal, ve.poblacion, ve.pais].filter(Boolean).join(" · ") }),
          !ve.nifFiscal && /* @__PURE__ */ l.jsx("div", { className: "dx-aviso", children: "⚠ Sin NIF en la ficha: hace falta para el libro de IVA, el 347 y el SII." }),
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
          const Ce = R(N.codigoIva), K = G(N);
          return /* @__PURE__ */ l.jsxs("tr", { className: H % 2 ? "dx-par" : "", children: [
            /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx("input", { value: N.descripcion, onChange: (te) => ct(N.clave, { descripcion: te.target.value }), placeholder: "Qué se compra" }) }),
            /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx("input", { list: "dx-cuentas-gasto", value: N.cuentaGasto, onChange: (te) => ct(N.clave, { cuentaGasto: te.target.value }), placeholder: "la de la regla" }) }),
            /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx("input", { className: "num", type: "number", step: "0.01", value: N.base || "", onChange: (te) => ct(N.clave, { base: Number(te.target.value) }) }) }),
            /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsxs("select", { value: N.codigoIva, onChange: (te) => ct(N.clave, { codigoIva: te.target.value, porcentajeIva: null }), children: [
              /* @__PURE__ */ l.jsx("option", { value: "", children: "General" }),
              o.map((te) => /* @__PURE__ */ l.jsx("option", { value: te.codigo, children: te.nombre }, te.codigo))
            ] }) }),
            /* @__PURE__ */ l.jsx("td", { children: Om(Ce == null ? void 0 : Ce.clase) ? /* @__PURE__ */ l.jsx("input", { className: "num", type: "number", step: "0.01", value: N.porcentajeIva ?? "", placeholder: "21", title: "Tipo que autoliquidas", onChange: (te) => ct(N.clave, { porcentajeIva: te.target.value === "" ? null : Number(te.target.value) }) }) : /* @__PURE__ */ l.jsx("span", { className: "muted", children: K ? `${de(K.porcentajeIva)} %` : "" }) }),
            /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx("input", { className: "num", type: "number", step: "1", min: 0, max: 100, value: N.porcentajeDeducible, onChange: (te) => ct(N.clave, { porcentajeDeducible: Number(te.target.value) }) }) }),
            /* @__PURE__ */ l.jsx("td", { className: "num", children: K ? /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
              /* @__PURE__ */ l.jsx("strong", { children: z(K.cuota) }),
              K.autoliquidada && /* @__PURE__ */ l.jsx("div", { className: "dx-sub", children: "autoliquidada" }),
              K.cuotaRecargo !== 0 && /* @__PURE__ */ l.jsxs("div", { className: "dx-sub", children: [
                "+ recargo ",
                z(K.cuotaRecargo)
              ] })
            ] }) : "—" }),
            /* @__PURE__ */ l.jsx("td", { className: "right", children: /* @__PURE__ */ l.jsx("button", { className: "dx-icono", title: "Quitar", onClick: () => we((te) => te.length > 1 ? te.filter((xt) => xt.clave !== N.clave) : [Jl()]), children: "✕" }) })
          ] }, N.clave);
        }) })
      ] }),
      /* @__PURE__ */ l.jsx("datalist", { id: "dx-cuentas-gasto", children: j.map((N) => /* @__PURE__ */ l.jsx("option", { value: N.codigo, children: N.nombre }, N.codigo)) }),
      /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", style: { marginTop: 8 }, onClick: () => we((N) => {
        var H;
        return [...N, Jl(((H = N[N.length - 1]) == null ? void 0 : H.codigoIva) ?? "")];
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
            D && /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", onClick: () => st(null), children: "Según forma de pago" })
          ] })
        ] }),
        D ? /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          D.map((N, H) => /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", style: { marginBottom: 6 }, children: [
            /* @__PURE__ */ l.jsx("input", { type: "date", value: N.fecha, onChange: (Ce) => st(D.map((K, te) => te === H ? { ...K, fecha: Ce.target.value } : K)) }),
            /* @__PURE__ */ l.jsx("input", { className: "num", type: "number", step: "0.01", value: N.importe, onChange: (Ce) => st(D.map((K, te) => te === H ? { ...K, importe: Number(Ce.target.value) } : K)) })
          ] }, H)),
          U && le !== U.total && /* @__PURE__ */ l.jsxs("p", { className: "dx-rojo", style: { margin: 0 }, children: [
            "Los plazos suman ",
            z(le),
            "; la factura, ",
            z(U.total),
            "."
          ] })
        ] }) : /* @__PURE__ */ l.jsx("p", { className: "muted", style: { margin: 0 }, children: ((U == null ? void 0 : U.vencimientos) ?? []).map((N) => `${Ee(N.fecha)}: ${z(N.importe)}`).join(" · ") || "—" }),
        et && /* @__PURE__ */ l.jsx("p", { className: "dx-rojo", style: { marginBottom: 0 }, children: et })
      ] }),
      /* @__PURE__ */ l.jsxs("div", { className: "panel dx-totales", children: [
        ((U == null ? void 0 : U.desglose) ?? []).map((N, H) => {
          var Ce;
          return /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
            /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
              ((Ce = R(N.codigoIva)) == null ? void 0 : Ce.nombre) ?? N.codigoIva,
              " ",
              N.autoliquidada ? `(${de(N.porcentajeIva)} %, autoliquidado)` : "",
              " · base ",
              de(N.base)
            ] }),
            /* @__PURE__ */ l.jsx("span", { children: z(N.cuota) })
          ] }, H);
        }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Base imponible" }),
          /* @__PURE__ */ l.jsx("span", { children: z(U == null ? void 0 : U.baseImponible) })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Impuestos" }),
          /* @__PURE__ */ l.jsx("span", { children: z(U == null ? void 0 : U.cuotaIva) })
        ] }),
        !!(U != null && U.recargoTotal) && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Recargo de equivalencia" }),
          /* @__PURE__ */ l.jsx("span", { children: z(U.recargoTotal) })
        ] }),
        !!(U != null && U.retencionIrpf) && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Retención IRPF" }),
          /* @__PURE__ */ l.jsxs("span", { children: [
            "−",
            z(U.retencionIrpf)
          ] })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ l.jsx("span", { children: ((U == null ? void 0 : U.total) ?? 0) < 0 ? "A favor (abono del proveedor)" : "Total a pagar" }),
          /* @__PURE__ */ l.jsx("span", { children: z(U == null ? void 0 : U.total) })
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
            de(U.totalDivisa ?? 0),
            " ",
            U.moneda
          ] })
        ] }),
        U && (U.desglose ?? []).some((N) => N.cuotaDeducible !== N.cuota) && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "IVA deducible (antes de prorrata)" }),
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: z((U.desglose ?? []).reduce((N, H) => N + H.cuotaDeducible, 0)) })
        ] })
      ] })
    ] })
  ] });
}
function Um(e) {
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
          /* @__PURE__ */ l.jsx("span", { className: Vo(a.estado === "Anulado" ? "Anulada" : "Emitida"), children: a.estado }),
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
          /* @__PURE__ */ l.jsx("div", { children: Ee(a.fechaFactura ?? a.fecha) })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-dato", children: [
          /* @__PURE__ */ l.jsx("small", { children: "Registro" }),
          /* @__PURE__ */ l.jsx("div", { children: Ee(a.fecha) })
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
        /* @__PURE__ */ l.jsx("tbody", { children: (a.lineas ?? []).map((w, $) => /* @__PURE__ */ l.jsxs("tr", { children: [
          /* @__PURE__ */ l.jsx("td", { children: w.descripcion ?? "" }),
          /* @__PURE__ */ l.jsx("td", { className: "mono muted", children: w.cuentaGasto ?? "regla" }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: z(w.base) }),
          /* @__PURE__ */ l.jsxs("td", { children: [
            w.codigoIva,
            " · ",
            de(w.porcentajeIva),
            " %",
            w.autoliquidada ? " · autoliquidada" : "",
            w.cuotaRecargo ? ` · recargo ${z(w.cuotaRecargo)}` : ""
          ] }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: z(w.cuota) }),
          /* @__PURE__ */ l.jsxs("td", { className: "num muted", children: [
            w.porcentajeDeducible !== 100 ? `${de(w.porcentajeDeducible)} % · ` : "",
            z(w.cuotaDeducible)
          ] })
        ] }, $)) })
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
        (a.vencimientos ?? []).map((w) => `${Ee(w.fecha)} ${z(w.importe)}`).join(" · ")
      ] })
    ] }),
    j && /* @__PURE__ */ l.jsx(
      fn,
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
function Vm(e) {
  const [t, n] = x.useState(e.inicial), r = x.useRef(0), [a, i] = x.useState(0), o = x.useMemo(() => xm(e.anfitrion), [e.anfitrion]), s = (u) => {
    n(u), i(++r.current), window.scrollTo({ top: 0 });
  }, c = { api: o, anfitrion: e.anfitrion, ruta: t, navegar: s }, d = `${t.tipo}-${t.pantalla}-${t.id ?? ""}-${a}`;
  let j;
  if (t.pantalla === "lista") j = /* @__PURE__ */ l.jsx(Pm, { tipo: t.tipo });
  else if (t.pantalla === "vista" && t.id)
    j = t.tipo === "factura" ? /* @__PURE__ */ l.jsx(Lm, { id: t.id }) : t.tipo === "presupuesto" ? /* @__PURE__ */ l.jsx(Am, { id: t.id }) : t.tipo === "pedido" ? /* @__PURE__ */ l.jsx(Mm, { id: t.id }) : t.tipo === "gasto" ? /* @__PURE__ */ l.jsx(Um, { id: t.id }) : /* @__PURE__ */ l.jsx($m, { id: t.id });
  else if (t.tipo === "gasto")
    j = /* @__PURE__ */ l.jsx(
      bm,
      {
        id: t.id,
        semilla: t.semilla,
        alGuardar: (u) => s({ tipo: "gasto", pantalla: "vista", id: u }),
        alCancelar: () => s(t.id ? { tipo: "gasto", pantalla: "vista", id: t.id } : { tipo: "gasto", pantalla: "lista" })
      }
    );
  else if (t.tipo === "compra")
    j = /* @__PURE__ */ l.jsx(
      zm,
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
      Dm,
      {
        tipo: u,
        id: t.id,
        semilla: t.semilla,
        alGuardar: (m) => s({ tipo: u, pantalla: "vista", id: m }),
        alCancelar: () => s(t.id ? { tipo: u, pantalla: "vista", id: t.id } : { tipo: u, pantalla: "lista" })
      }
    );
  }
  return /* @__PURE__ */ l.jsx(Id.Provider, { value: c, children: /* @__PURE__ */ l.jsx("div", { className: "dx-raiz", children: j }, d) });
}
const Bm = `
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
function Hm() {
  if (document.getElementById("dx-estilos")) return;
  const e = document.createElement("style");
  e.id = "dx-estilos", e.textContent = Bm, document.head.appendChild(e);
}
function Wm(e, t, n) {
  Hm();
  const r = Ed(e);
  return r.render(/* @__PURE__ */ l.jsx(Vm, { anfitrion: t, inicial: n })), () => r.unmount();
}
export {
  Wm as montar
};
