var $s = { exports: {} }, Ml = {}, As = { exports: {} }, H = {};
/**
 * @license React
 * react.production.min.js
 *
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */
var _r = Symbol.for("react.element"), ld = Symbol.for("react.portal"), ad = Symbol.for("react.fragment"), id = Symbol.for("react.strict_mode"), od = Symbol.for("react.profiler"), sd = Symbol.for("react.provider"), ud = Symbol.for("react.context"), cd = Symbol.for("react.forward_ref"), dd = Symbol.for("react.suspense"), fd = Symbol.for("react.memo"), pd = Symbol.for("react.lazy"), _o = Symbol.iterator;
function md(e) {
  return e === null || typeof e != "object" ? null : (e = _o && e[_o] || e["@@iterator"], typeof e == "function" ? e : null);
}
var Us = { isMounted: function() {
  return !1;
}, enqueueForceUpdate: function() {
}, enqueueReplaceState: function() {
}, enqueueSetState: function() {
} }, Vs = Object.assign, Bs = {};
function An(e, t, n) {
  this.props = e, this.context = t, this.refs = Bs, this.updater = n || Us;
}
An.prototype.isReactComponent = {};
An.prototype.setState = function(e, t) {
  if (typeof e != "object" && typeof e != "function" && e != null) throw Error("setState(...): takes an object of state variables to update or a function which returns an object of state variables.");
  this.updater.enqueueSetState(this, e, t, "setState");
};
An.prototype.forceUpdate = function(e) {
  this.updater.enqueueForceUpdate(this, e, "forceUpdate");
};
function Hs() {
}
Hs.prototype = An.prototype;
function Ci(e, t, n) {
  this.props = e, this.context = t, this.refs = Bs, this.updater = n || Us;
}
var Ei = Ci.prototype = new Hs();
Ei.constructor = Ci;
Vs(Ei, An.prototype);
Ei.isPureReactComponent = !0;
var Fo = Array.isArray, Ws = Object.prototype.hasOwnProperty, Ii = { current: null }, Qs = { key: !0, ref: !0, __self: !0, __source: !0 };
function Gs(e, t, n) {
  var r, l = {}, i = null, o = null;
  if (t != null) for (r in t.ref !== void 0 && (o = t.ref), t.key !== void 0 && (i = "" + t.key), t) Ws.call(t, r) && !Qs.hasOwnProperty(r) && (l[r] = t[r]);
  var s = arguments.length - 2;
  if (s === 1) l.children = n;
  else if (1 < s) {
    for (var u = Array(s), d = 0; d < s; d++) u[d] = arguments[d + 2];
    l.children = u;
  }
  if (e && e.defaultProps) for (r in s = e.defaultProps, s) l[r] === void 0 && (l[r] = s[r]);
  return { $$typeof: _r, type: e, key: i, ref: o, props: l, _owner: Ii.current };
}
function hd(e, t) {
  return { $$typeof: _r, type: e.type, key: t, ref: e.ref, props: e.props, _owner: e._owner };
}
function Pi(e) {
  return typeof e == "object" && e !== null && e.$$typeof === _r;
}
function vd(e) {
  var t = { "=": "=0", ":": "=2" };
  return "$" + e.replace(/[=:]/g, function(n) {
    return t[n];
  });
}
var zo = /\/+/g;
function aa(e, t) {
  return typeof e == "object" && e !== null && e.key != null ? vd("" + e.key) : t.toString(36);
}
function Jr(e, t, n, r, l) {
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
        case _r:
        case ld:
          o = !0;
      }
  }
  if (o) return o = e, l = l(o), e = r === "" ? "." + aa(o, 0) : r, Fo(l) ? (n = "", e != null && (n = e.replace(zo, "$&/") + "/"), Jr(l, t, n, "", function(d) {
    return d;
  })) : l != null && (Pi(l) && (l = hd(l, n + (!l.key || o && o.key === l.key ? "" : ("" + l.key).replace(zo, "$&/") + "/") + e)), t.push(l)), 1;
  if (o = 0, r = r === "" ? "." : r + ":", Fo(e)) for (var s = 0; s < e.length; s++) {
    i = e[s];
    var u = r + aa(i, s);
    o += Jr(i, t, n, u, l);
  }
  else if (u = md(e), typeof u == "function") for (e = u.call(e), s = 0; !(i = e.next()).done; ) i = i.value, u = r + aa(i, s++), o += Jr(i, t, n, u, l);
  else if (i === "object") throw t = String(e), Error("Objects are not valid as a React child (found: " + (t === "[object Object]" ? "object with keys {" + Object.keys(e).join(", ") + "}" : t) + "). If you meant to render a collection of children, use an array instead.");
  return o;
}
function Mr(e, t, n) {
  if (e == null) return e;
  var r = [], l = 0;
  return Jr(e, r, "", "", function(i) {
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
var Te = { current: null }, el = { transition: null }, xd = { ReactCurrentDispatcher: Te, ReactCurrentBatchConfig: el, ReactCurrentOwner: Ii };
function Ks() {
  throw Error("act(...) is not supported in production builds of React.");
}
H.Children = { map: Mr, forEach: function(e, t, n) {
  Mr(e, function() {
    t.apply(this, arguments);
  }, n);
}, count: function(e) {
  var t = 0;
  return Mr(e, function() {
    t++;
  }), t;
}, toArray: function(e) {
  return Mr(e, function(t) {
    return t;
  }) || [];
}, only: function(e) {
  if (!Pi(e)) throw Error("React.Children.only expected to receive a single React element child.");
  return e;
} };
H.Component = An;
H.Fragment = ad;
H.Profiler = od;
H.PureComponent = Ci;
H.StrictMode = id;
H.Suspense = dd;
H.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED = xd;
H.act = Ks;
H.cloneElement = function(e, t, n) {
  if (e == null) throw Error("React.cloneElement(...): The argument must be a React element, but you passed " + e + ".");
  var r = Vs({}, e.props), l = e.key, i = e.ref, o = e._owner;
  if (t != null) {
    if (t.ref !== void 0 && (i = t.ref, o = Ii.current), t.key !== void 0 && (l = "" + t.key), e.type && e.type.defaultProps) var s = e.type.defaultProps;
    for (u in t) Ws.call(t, u) && !Qs.hasOwnProperty(u) && (r[u] = t[u] === void 0 && s !== void 0 ? s[u] : t[u]);
  }
  var u = arguments.length - 2;
  if (u === 1) r.children = n;
  else if (1 < u) {
    s = Array(u);
    for (var d = 0; d < u; d++) s[d] = arguments[d + 2];
    r.children = s;
  }
  return { $$typeof: _r, type: e.type, key: l, ref: i, props: r, _owner: o };
};
H.createContext = function(e) {
  return e = { $$typeof: ud, _currentValue: e, _currentValue2: e, _threadCount: 0, Provider: null, Consumer: null, _defaultValue: null, _globalName: null }, e.Provider = { $$typeof: sd, _context: e }, e.Consumer = e;
};
H.createElement = Gs;
H.createFactory = function(e) {
  var t = Gs.bind(null, e);
  return t.type = e, t;
};
H.createRef = function() {
  return { current: null };
};
H.forwardRef = function(e) {
  return { $$typeof: cd, render: e };
};
H.isValidElement = Pi;
H.lazy = function(e) {
  return { $$typeof: pd, _payload: { _status: -1, _result: e }, _init: gd };
};
H.memo = function(e, t) {
  return { $$typeof: fd, type: e, compare: t === void 0 ? null : t };
};
H.startTransition = function(e) {
  var t = el.transition;
  el.transition = {};
  try {
    e();
  } finally {
    el.transition = t;
  }
};
H.unstable_act = Ks;
H.useCallback = function(e, t) {
  return Te.current.useCallback(e, t);
};
H.useContext = function(e) {
  return Te.current.useContext(e);
};
H.useDebugValue = function() {
};
H.useDeferredValue = function(e) {
  return Te.current.useDeferredValue(e);
};
H.useEffect = function(e, t) {
  return Te.current.useEffect(e, t);
};
H.useId = function() {
  return Te.current.useId();
};
H.useImperativeHandle = function(e, t, n) {
  return Te.current.useImperativeHandle(e, t, n);
};
H.useInsertionEffect = function(e, t) {
  return Te.current.useInsertionEffect(e, t);
};
H.useLayoutEffect = function(e, t) {
  return Te.current.useLayoutEffect(e, t);
};
H.useMemo = function(e, t) {
  return Te.current.useMemo(e, t);
};
H.useReducer = function(e, t, n) {
  return Te.current.useReducer(e, t, n);
};
H.useRef = function(e) {
  return Te.current.useRef(e);
};
H.useState = function(e) {
  return Te.current.useState(e);
};
H.useSyncExternalStore = function(e, t, n) {
  return Te.current.useSyncExternalStore(e, t, n);
};
H.useTransition = function() {
  return Te.current.useTransition();
};
H.version = "18.3.1";
As.exports = H;
var N = As.exports;
/**
 * @license React
 * react-jsx-runtime.production.min.js
 *
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */
var yd = N, jd = Symbol.for("react.element"), Nd = Symbol.for("react.fragment"), wd = Object.prototype.hasOwnProperty, Sd = yd.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED.ReactCurrentOwner, kd = { key: !0, ref: !0, __self: !0, __source: !0 };
function qs(e, t, n) {
  var r, l = {}, i = null, o = null;
  n !== void 0 && (i = "" + n), t.key !== void 0 && (i = "" + t.key), t.ref !== void 0 && (o = t.ref);
  for (r in t) wd.call(t, r) && !kd.hasOwnProperty(r) && (l[r] = t[r]);
  if (e && e.defaultProps) for (r in t = e.defaultProps, t) l[r] === void 0 && (l[r] = t[r]);
  return { $$typeof: jd, type: e, key: i, ref: o, props: l, _owner: Sd.current };
}
Ml.Fragment = Nd;
Ml.jsx = qs;
Ml.jsxs = qs;
$s.exports = Ml;
var a = $s.exports, Ys = { exports: {} }, Qe = {}, Xs = { exports: {} }, bs = {};
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
  function t(y, x) {
    var S = y.length;
    y.push(x);
    e: for (; 0 < S; ) {
      var $ = S - 1 >>> 1, B = y[$];
      if (0 < l(B, x)) y[$] = x, y[S] = B, S = $;
      else break e;
    }
  }
  function n(y) {
    return y.length === 0 ? null : y[0];
  }
  function r(y) {
    if (y.length === 0) return null;
    var x = y[0], S = y.pop();
    if (S !== x) {
      y[0] = S;
      e: for (var $ = 0, B = y.length, Q = B >>> 1; $ < Q; ) {
        var ye = 2 * ($ + 1) - 1, b = y[ye], ae = ye + 1, et = y[ae];
        if (0 > l(b, S)) ae < B && 0 > l(et, b) ? (y[$] = et, y[ae] = S, $ = ae) : (y[$] = b, y[ye] = S, $ = ye);
        else if (ae < B && 0 > l(et, S)) y[$] = et, y[ae] = S, $ = ae;
        else break e;
      }
    }
    return x;
  }
  function l(y, x) {
    var S = y.sortIndex - x.sortIndex;
    return S !== 0 ? S : y.id - x.id;
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
  var u = [], d = [], j = 1, c = null, m = 3, v = !1, g = !1, w = !1, z = typeof setTimeout == "function" ? setTimeout : null, p = typeof clearTimeout == "function" ? clearTimeout : null, f = typeof setImmediate < "u" ? setImmediate : null;
  typeof navigator < "u" && navigator.scheduling !== void 0 && navigator.scheduling.isInputPending !== void 0 && navigator.scheduling.isInputPending.bind(navigator.scheduling);
  function h(y) {
    for (var x = n(d); x !== null; ) {
      if (x.callback === null) r(d);
      else if (x.startTime <= y) r(d), x.sortIndex = x.expirationTime, t(u, x);
      else break;
      x = n(d);
    }
  }
  function C(y) {
    if (w = !1, h(y), !g) if (n(u) !== null) g = !0, q(T);
    else {
      var x = n(d);
      x !== null && he(C, x.startTime - y);
    }
  }
  function T(y, x) {
    g = !1, w && (w = !1, p(D), D = -1), v = !0;
    var S = m;
    try {
      for (h(x), c = n(u); c !== null && (!(c.expirationTime > x) || y && !_()); ) {
        var $ = c.callback;
        if (typeof $ == "function") {
          c.callback = null, m = c.priorityLevel;
          var B = $(c.expirationTime <= x);
          x = e.unstable_now(), typeof B == "function" ? c.callback = B : c === n(u) && r(u), h(x);
        } else r(u);
        c = n(u);
      }
      if (c !== null) var Q = !0;
      else {
        var ye = n(d);
        ye !== null && he(C, ye.startTime - x), Q = !1;
      }
      return Q;
    } finally {
      c = null, m = S, v = !1;
    }
  }
  var F = !1, R = null, D = -1, L = 5, E = -1;
  function _() {
    return !(e.unstable_now() - E < L);
  }
  function A() {
    if (R !== null) {
      var y = e.unstable_now();
      E = y;
      var x = !0;
      try {
        x = R(!0, y);
      } finally {
        x ? De() : (F = !1, R = null);
      }
    } else F = !1;
  }
  var De;
  if (typeof f == "function") De = function() {
    f(A);
  };
  else if (typeof MessageChannel < "u") {
    var Se = new MessageChannel(), Ke = Se.port2;
    Se.port1.onmessage = A, De = function() {
      Ke.postMessage(null);
    };
  } else De = function() {
    z(A, 0);
  };
  function q(y) {
    R = y, F || (F = !0, De());
  }
  function he(y, x) {
    D = z(function() {
      y(e.unstable_now());
    }, x);
  }
  e.unstable_IdlePriority = 5, e.unstable_ImmediatePriority = 1, e.unstable_LowPriority = 4, e.unstable_NormalPriority = 3, e.unstable_Profiling = null, e.unstable_UserBlockingPriority = 2, e.unstable_cancelCallback = function(y) {
    y.callback = null;
  }, e.unstable_continueExecution = function() {
    g || v || (g = !0, q(T));
  }, e.unstable_forceFrameRate = function(y) {
    0 > y || 125 < y ? console.error("forceFrameRate takes a positive int between 0 and 125, forcing frame rates higher than 125 fps is not supported") : L = 0 < y ? Math.floor(1e3 / y) : 5;
  }, e.unstable_getCurrentPriorityLevel = function() {
    return m;
  }, e.unstable_getFirstCallbackNode = function() {
    return n(u);
  }, e.unstable_next = function(y) {
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
      return y();
    } finally {
      m = S;
    }
  }, e.unstable_pauseExecution = function() {
  }, e.unstable_requestPaint = function() {
  }, e.unstable_runWithPriority = function(y, x) {
    switch (y) {
      case 1:
      case 2:
      case 3:
      case 4:
      case 5:
        break;
      default:
        y = 3;
    }
    var S = m;
    m = y;
    try {
      return x();
    } finally {
      m = S;
    }
  }, e.unstable_scheduleCallback = function(y, x, S) {
    var $ = e.unstable_now();
    switch (typeof S == "object" && S !== null ? (S = S.delay, S = typeof S == "number" && 0 < S ? $ + S : $) : S = $, y) {
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
    return B = S + B, y = { id: j++, callback: x, priorityLevel: y, startTime: S, expirationTime: B, sortIndex: -1 }, S > $ ? (y.sortIndex = S, t(d, y), n(u) === null && y === n(d) && (w ? (p(D), D = -1) : w = !0, he(C, S - $))) : (y.sortIndex = B, t(u, y), g || v || (g = !0, q(T))), y;
  }, e.unstable_shouldYield = _, e.unstable_wrapCallback = function(y) {
    var x = m;
    return function() {
      var S = m;
      m = x;
      try {
        return y.apply(this, arguments);
      } finally {
        m = S;
      }
    };
  };
})(bs);
Xs.exports = bs;
var Cd = Xs.exports;
/**
 * @license React
 * react-dom.production.min.js
 *
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */
var Ed = N, We = Cd;
function P(e) {
  for (var t = "https://reactjs.org/docs/error-decoder.html?invariant=" + e, n = 1; n < arguments.length; n++) t += "&args[]=" + encodeURIComponent(arguments[n]);
  return "Minified React error #" + e + "; visit " + t + " for the full message or use the non-minified dev environment for full errors and additional helpful warnings.";
}
var Zs = /* @__PURE__ */ new Set(), dr = {};
function cn(e, t) {
  Tn(e, t), Tn(e + "Capture", t);
}
function Tn(e, t) {
  for (dr[e] = t, e = 0; e < t.length; e++) Zs.add(t[e]);
}
var wt = !(typeof window > "u" || typeof window.document > "u" || typeof window.document.createElement > "u"), Ta = Object.prototype.hasOwnProperty, Id = /^[:A-Z_a-z\u00C0-\u00D6\u00D8-\u00F6\u00F8-\u02FF\u0370-\u037D\u037F-\u1FFF\u200C-\u200D\u2070-\u218F\u2C00-\u2FEF\u3001-\uD7FF\uF900-\uFDCF\uFDF0-\uFFFD][:A-Z_a-z\u00C0-\u00D6\u00D8-\u00F6\u00F8-\u02FF\u0370-\u037D\u037F-\u1FFF\u200C-\u200D\u2070-\u218F\u2C00-\u2FEF\u3001-\uD7FF\uF900-\uFDCF\uFDF0-\uFFFD\-.0-9\u00B7\u0300-\u036F\u203F-\u2040]*$/, To = {}, Ro = {};
function Pd(e) {
  return Ta.call(Ro, e) ? !0 : Ta.call(To, e) ? !1 : Id.test(e) ? Ro[e] = !0 : (To[e] = !0, !1);
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
function Re(e, t, n, r, l, i, o) {
  this.acceptsBooleans = t === 2 || t === 3 || t === 4, this.attributeName = r, this.attributeNamespace = l, this.mustUseProperty = n, this.propertyName = e, this.type = t, this.sanitizeURL = i, this.removeEmptyString = o;
}
var we = {};
"children dangerouslySetInnerHTML defaultValue defaultChecked innerHTML suppressContentEditableWarning suppressHydrationWarning style".split(" ").forEach(function(e) {
  we[e] = new Re(e, 0, !1, e, null, !1, !1);
});
[["acceptCharset", "accept-charset"], ["className", "class"], ["htmlFor", "for"], ["httpEquiv", "http-equiv"]].forEach(function(e) {
  var t = e[0];
  we[t] = new Re(t, 1, !1, e[1], null, !1, !1);
});
["contentEditable", "draggable", "spellCheck", "value"].forEach(function(e) {
  we[e] = new Re(e, 2, !1, e.toLowerCase(), null, !1, !1);
});
["autoReverse", "externalResourcesRequired", "focusable", "preserveAlpha"].forEach(function(e) {
  we[e] = new Re(e, 2, !1, e, null, !1, !1);
});
"allowFullScreen async autoFocus autoPlay controls default defer disabled disablePictureInPicture disableRemotePlayback formNoValidate hidden loop noModule noValidate open playsInline readOnly required reversed scoped seamless itemScope".split(" ").forEach(function(e) {
  we[e] = new Re(e, 3, !1, e.toLowerCase(), null, !1, !1);
});
["checked", "multiple", "muted", "selected"].forEach(function(e) {
  we[e] = new Re(e, 3, !0, e, null, !1, !1);
});
["capture", "download"].forEach(function(e) {
  we[e] = new Re(e, 4, !1, e, null, !1, !1);
});
["cols", "rows", "size", "span"].forEach(function(e) {
  we[e] = new Re(e, 6, !1, e, null, !1, !1);
});
["rowSpan", "start"].forEach(function(e) {
  we[e] = new Re(e, 5, !1, e.toLowerCase(), null, !1, !1);
});
var _i = /[\-:]([a-z])/g;
function Fi(e) {
  return e[1].toUpperCase();
}
"accent-height alignment-baseline arabic-form baseline-shift cap-height clip-path clip-rule color-interpolation color-interpolation-filters color-profile color-rendering dominant-baseline enable-background fill-opacity fill-rule flood-color flood-opacity font-family font-size font-size-adjust font-stretch font-style font-variant font-weight glyph-name glyph-orientation-horizontal glyph-orientation-vertical horiz-adv-x horiz-origin-x image-rendering letter-spacing lighting-color marker-end marker-mid marker-start overline-position overline-thickness paint-order panose-1 pointer-events rendering-intent shape-rendering stop-color stop-opacity strikethrough-position strikethrough-thickness stroke-dasharray stroke-dashoffset stroke-linecap stroke-linejoin stroke-miterlimit stroke-opacity stroke-width text-anchor text-decoration text-rendering underline-position underline-thickness unicode-bidi unicode-range units-per-em v-alphabetic v-hanging v-ideographic v-mathematical vector-effect vert-adv-y vert-origin-x vert-origin-y word-spacing writing-mode xmlns:xlink x-height".split(" ").forEach(function(e) {
  var t = e.replace(
    _i,
    Fi
  );
  we[t] = new Re(t, 1, !1, e, null, !1, !1);
});
"xlink:actuate xlink:arcrole xlink:role xlink:show xlink:title xlink:type".split(" ").forEach(function(e) {
  var t = e.replace(_i, Fi);
  we[t] = new Re(t, 1, !1, e, "http://www.w3.org/1999/xlink", !1, !1);
});
["xml:base", "xml:lang", "xml:space"].forEach(function(e) {
  var t = e.replace(_i, Fi);
  we[t] = new Re(t, 1, !1, e, "http://www.w3.org/XML/1998/namespace", !1, !1);
});
["tabIndex", "crossOrigin"].forEach(function(e) {
  we[e] = new Re(e, 1, !1, e.toLowerCase(), null, !1, !1);
});
we.xlinkHref = new Re("xlinkHref", 1, !1, "xlink:href", "http://www.w3.org/1999/xlink", !0, !1);
["src", "href", "action", "formAction"].forEach(function(e) {
  we[e] = new Re(e, 1, !1, e.toLowerCase(), null, !0, !0);
});
function zi(e, t, n, r) {
  var l = we.hasOwnProperty(t) ? we[t] : null;
  (l !== null ? l.type !== 0 : r || !(2 < t.length) || t[0] !== "o" && t[0] !== "O" || t[1] !== "n" && t[1] !== "N") && (Fd(t, n, l, r) && (n = null), r || l === null ? Pd(t) && (n === null ? e.removeAttribute(t) : e.setAttribute(t, "" + n)) : l.mustUseProperty ? e[l.propertyName] = n === null ? l.type === 3 ? !1 : "" : n : (t = l.attributeName, r = l.attributeNamespace, n === null ? e.removeAttribute(t) : (l = l.type, n = l === 3 || l === 4 && n === !0 ? "" : "" + n, r ? e.setAttributeNS(r, t, n) : e.setAttribute(t, n))));
}
var Et = Ed.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED, Or = Symbol.for("react.element"), mn = Symbol.for("react.portal"), hn = Symbol.for("react.fragment"), Ti = Symbol.for("react.strict_mode"), Ra = Symbol.for("react.profiler"), Js = Symbol.for("react.provider"), eu = Symbol.for("react.context"), Ri = Symbol.for("react.forward_ref"), Da = Symbol.for("react.suspense"), La = Symbol.for("react.suspense_list"), Di = Symbol.for("react.memo"), _t = Symbol.for("react.lazy"), tu = Symbol.for("react.offscreen"), Do = Symbol.iterator;
function Hn(e) {
  return e === null || typeof e != "object" ? null : (e = Do && e[Do] || e["@@iterator"], typeof e == "function" ? e : null);
}
var le = Object.assign, ia;
function bn(e) {
  if (ia === void 0) try {
    throw Error();
  } catch (n) {
    var t = n.stack.trim().match(/\n( *(at )?)/);
    ia = t && t[1] || "";
  }
  return `
` + ia + e;
}
var oa = !1;
function sa(e, t) {
  if (!e || oa) return "";
  oa = !0;
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
    oa = !1, Error.prepareStackTrace = n;
  }
  return (e = e ? e.displayName || e.name : "") ? bn(e) : "";
}
function zd(e) {
  switch (e.tag) {
    case 5:
      return bn(e.type);
    case 16:
      return bn("Lazy");
    case 13:
      return bn("Suspense");
    case 19:
      return bn("SuspenseList");
    case 0:
    case 2:
    case 15:
      return e = sa(e.type, !1), e;
    case 11:
      return e = sa(e.type.render, !1), e;
    case 1:
      return e = sa(e.type, !0), e;
    default:
      return "";
  }
}
function Ma(e) {
  if (e == null) return null;
  if (typeof e == "function") return e.displayName || e.name || null;
  if (typeof e == "string") return e;
  switch (e) {
    case hn:
      return "Fragment";
    case mn:
      return "Portal";
    case Ra:
      return "Profiler";
    case Ti:
      return "StrictMode";
    case Da:
      return "Suspense";
    case La:
      return "SuspenseList";
  }
  if (typeof e == "object") switch (e.$$typeof) {
    case eu:
      return (e.displayName || "Context") + ".Consumer";
    case Js:
      return (e._context.displayName || "Context") + ".Provider";
    case Ri:
      var t = e.render;
      return e = e.displayName, e || (e = t.displayName || t.name || "", e = e !== "" ? "ForwardRef(" + e + ")" : "ForwardRef"), e;
    case Di:
      return t = e.displayName || null, t !== null ? t : Ma(e.type) || "Memo";
    case _t:
      t = e._payload, e = e._init;
      try {
        return Ma(e(t));
      } catch {
      }
  }
  return null;
}
function Td(e) {
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
      return Ma(t);
    case 8:
      return t === Ti ? "StrictMode" : "Mode";
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
function Ht(e) {
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
function nu(e) {
  var t = e.type;
  return (e = e.nodeName) && e.toLowerCase() === "input" && (t === "checkbox" || t === "radio");
}
function Rd(e) {
  var t = nu(e) ? "checked" : "value", n = Object.getOwnPropertyDescriptor(e.constructor.prototype, t), r = "" + e[t];
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
function $r(e) {
  e._valueTracker || (e._valueTracker = Rd(e));
}
function ru(e) {
  if (!e) return !1;
  var t = e._valueTracker;
  if (!t) return !0;
  var n = t.getValue(), r = "";
  return e && (r = nu(e) ? e.checked ? "true" : "false" : e.value), e = r, e !== n ? (t.setValue(e), !0) : !1;
}
function fl(e) {
  if (e = e || (typeof document < "u" ? document : void 0), typeof e > "u") return null;
  try {
    return e.activeElement || e.body;
  } catch {
    return e.body;
  }
}
function Oa(e, t) {
  var n = t.checked;
  return le({}, t, { defaultChecked: void 0, defaultValue: void 0, value: void 0, checked: n ?? e._wrapperState.initialChecked });
}
function Lo(e, t) {
  var n = t.defaultValue == null ? "" : t.defaultValue, r = t.checked != null ? t.checked : t.defaultChecked;
  n = Ht(t.value != null ? t.value : n), e._wrapperState = { initialChecked: r, initialValue: n, controlled: t.type === "checkbox" || t.type === "radio" ? t.checked != null : t.value != null };
}
function lu(e, t) {
  t = t.checked, t != null && zi(e, "checked", t, !1);
}
function $a(e, t) {
  lu(e, t);
  var n = Ht(t.value), r = t.type;
  if (n != null) r === "number" ? (n === 0 && e.value === "" || e.value != n) && (e.value = "" + n) : e.value !== "" + n && (e.value = "" + n);
  else if (r === "submit" || r === "reset") {
    e.removeAttribute("value");
    return;
  }
  t.hasOwnProperty("value") ? Aa(e, t.type, n) : t.hasOwnProperty("defaultValue") && Aa(e, t.type, Ht(t.defaultValue)), t.checked == null && t.defaultChecked != null && (e.defaultChecked = !!t.defaultChecked);
}
function Mo(e, t, n) {
  if (t.hasOwnProperty("value") || t.hasOwnProperty("defaultValue")) {
    var r = t.type;
    if (!(r !== "submit" && r !== "reset" || t.value !== void 0 && t.value !== null)) return;
    t = "" + e._wrapperState.initialValue, n || t === e.value || (e.value = t), e.defaultValue = t;
  }
  n = e.name, n !== "" && (e.name = ""), e.defaultChecked = !!e._wrapperState.initialChecked, n !== "" && (e.name = n);
}
function Aa(e, t, n) {
  (t !== "number" || fl(e.ownerDocument) !== e) && (n == null ? e.defaultValue = "" + e._wrapperState.initialValue : e.defaultValue !== "" + n && (e.defaultValue = "" + n));
}
var Zn = Array.isArray;
function En(e, t, n, r) {
  if (e = e.options, t) {
    t = {};
    for (var l = 0; l < n.length; l++) t["$" + n[l]] = !0;
    for (n = 0; n < e.length; n++) l = t.hasOwnProperty("$" + e[n].value), e[n].selected !== l && (e[n].selected = l), l && r && (e[n].defaultSelected = !0);
  } else {
    for (n = "" + Ht(n), t = null, l = 0; l < e.length; l++) {
      if (e[l].value === n) {
        e[l].selected = !0, r && (e[l].defaultSelected = !0);
        return;
      }
      t !== null || e[l].disabled || (t = e[l]);
    }
    t !== null && (t.selected = !0);
  }
}
function Ua(e, t) {
  if (t.dangerouslySetInnerHTML != null) throw Error(P(91));
  return le({}, t, { value: void 0, defaultValue: void 0, children: "" + e._wrapperState.initialValue });
}
function Oo(e, t) {
  var n = t.value;
  if (n == null) {
    if (n = t.children, t = t.defaultValue, n != null) {
      if (t != null) throw Error(P(92));
      if (Zn(n)) {
        if (1 < n.length) throw Error(P(93));
        n = n[0];
      }
      t = n;
    }
    t == null && (t = ""), n = t;
  }
  e._wrapperState = { initialValue: Ht(n) };
}
function au(e, t) {
  var n = Ht(t.value), r = Ht(t.defaultValue);
  n != null && (n = "" + n, n !== e.value && (e.value = n), t.defaultValue == null && e.defaultValue !== n && (e.defaultValue = n)), r != null && (e.defaultValue = "" + r);
}
function $o(e) {
  var t = e.textContent;
  t === e._wrapperState.initialValue && t !== "" && t !== null && (e.value = t);
}
function iu(e) {
  switch (e) {
    case "svg":
      return "http://www.w3.org/2000/svg";
    case "math":
      return "http://www.w3.org/1998/Math/MathML";
    default:
      return "http://www.w3.org/1999/xhtml";
  }
}
function Va(e, t) {
  return e == null || e === "http://www.w3.org/1999/xhtml" ? iu(t) : e === "http://www.w3.org/2000/svg" && t === "foreignObject" ? "http://www.w3.org/1999/xhtml" : e;
}
var Ar, ou = function(e) {
  return typeof MSApp < "u" && MSApp.execUnsafeLocalFunction ? function(t, n, r, l) {
    MSApp.execUnsafeLocalFunction(function() {
      return e(t, n, r, l);
    });
  } : e;
}(function(e, t) {
  if (e.namespaceURI !== "http://www.w3.org/2000/svg" || "innerHTML" in e) e.innerHTML = t;
  else {
    for (Ar = Ar || document.createElement("div"), Ar.innerHTML = "<svg>" + t.valueOf().toString() + "</svg>", t = Ar.firstChild; e.firstChild; ) e.removeChild(e.firstChild);
    for (; t.firstChild; ) e.appendChild(t.firstChild);
  }
});
function fr(e, t) {
  if (t) {
    var n = e.firstChild;
    if (n && n === e.lastChild && n.nodeType === 3) {
      n.nodeValue = t;
      return;
    }
  }
  e.textContent = t;
}
var tr = {
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
Object.keys(tr).forEach(function(e) {
  Dd.forEach(function(t) {
    t = t + e.charAt(0).toUpperCase() + e.substring(1), tr[t] = tr[e];
  });
});
function su(e, t, n) {
  return t == null || typeof t == "boolean" || t === "" ? "" : n || typeof t != "number" || t === 0 || tr.hasOwnProperty(e) && tr[e] ? ("" + t).trim() : t + "px";
}
function uu(e, t) {
  e = e.style;
  for (var n in t) if (t.hasOwnProperty(n)) {
    var r = n.indexOf("--") === 0, l = su(n, t[n], r);
    n === "float" && (n = "cssFloat"), r ? e.setProperty(n, l) : e[n] = l;
  }
}
var Ld = le({ menuitem: !0 }, { area: !0, base: !0, br: !0, col: !0, embed: !0, hr: !0, img: !0, input: !0, keygen: !0, link: !0, meta: !0, param: !0, source: !0, track: !0, wbr: !0 });
function Ba(e, t) {
  if (t) {
    if (Ld[e] && (t.children != null || t.dangerouslySetInnerHTML != null)) throw Error(P(137, e));
    if (t.dangerouslySetInnerHTML != null) {
      if (t.children != null) throw Error(P(60));
      if (typeof t.dangerouslySetInnerHTML != "object" || !("__html" in t.dangerouslySetInnerHTML)) throw Error(P(61));
    }
    if (t.style != null && typeof t.style != "object") throw Error(P(62));
  }
}
function Ha(e, t) {
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
var Wa = null;
function Li(e) {
  return e = e.target || e.srcElement || window, e.correspondingUseElement && (e = e.correspondingUseElement), e.nodeType === 3 ? e.parentNode : e;
}
var Qa = null, In = null, Pn = null;
function Ao(e) {
  if (e = Tr(e)) {
    if (typeof Qa != "function") throw Error(P(280));
    var t = e.stateNode;
    t && (t = Vl(t), Qa(e.stateNode, e.type, t));
  }
}
function cu(e) {
  In ? Pn ? Pn.push(e) : Pn = [e] : In = e;
}
function du() {
  if (In) {
    var e = In, t = Pn;
    if (Pn = In = null, Ao(e), t) for (e = 0; e < t.length; e++) Ao(t[e]);
  }
}
function fu(e, t) {
  return e(t);
}
function pu() {
}
var ua = !1;
function mu(e, t, n) {
  if (ua) return e(t, n);
  ua = !0;
  try {
    return fu(e, t, n);
  } finally {
    ua = !1, (In !== null || Pn !== null) && (pu(), du());
  }
}
function pr(e, t) {
  var n = e.stateNode;
  if (n === null) return null;
  var r = Vl(n);
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
var Ga = !1;
if (wt) try {
  var Wn = {};
  Object.defineProperty(Wn, "passive", { get: function() {
    Ga = !0;
  } }), window.addEventListener("test", Wn, Wn), window.removeEventListener("test", Wn, Wn);
} catch {
  Ga = !1;
}
function Md(e, t, n, r, l, i, o, s, u) {
  var d = Array.prototype.slice.call(arguments, 3);
  try {
    t.apply(n, d);
  } catch (j) {
    this.onError(j);
  }
}
var nr = !1, pl = null, ml = !1, Ka = null, Od = { onError: function(e) {
  nr = !0, pl = e;
} };
function $d(e, t, n, r, l, i, o, s, u) {
  nr = !1, pl = null, Md.apply(Od, arguments);
}
function Ad(e, t, n, r, l, i, o, s, u) {
  if ($d.apply(this, arguments), nr) {
    if (nr) {
      var d = pl;
      nr = !1, pl = null;
    } else throw Error(P(198));
    ml || (ml = !0, Ka = d);
  }
}
function dn(e) {
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
function hu(e) {
  if (e.tag === 13) {
    var t = e.memoizedState;
    if (t === null && (e = e.alternate, e !== null && (t = e.memoizedState)), t !== null) return t.dehydrated;
  }
  return null;
}
function Uo(e) {
  if (dn(e) !== e) throw Error(P(188));
}
function Ud(e) {
  var t = e.alternate;
  if (!t) {
    if (t = dn(e), t === null) throw Error(P(188));
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
        if (i === n) return Uo(l), e;
        if (i === r) return Uo(l), t;
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
function vu(e) {
  return e = Ud(e), e !== null ? gu(e) : null;
}
function gu(e) {
  if (e.tag === 5 || e.tag === 6) return e;
  for (e = e.child; e !== null; ) {
    var t = gu(e);
    if (t !== null) return t;
    e = e.sibling;
  }
  return null;
}
var xu = We.unstable_scheduleCallback, Vo = We.unstable_cancelCallback, Vd = We.unstable_shouldYield, Bd = We.unstable_requestPaint, ue = We.unstable_now, Hd = We.unstable_getCurrentPriorityLevel, Mi = We.unstable_ImmediatePriority, yu = We.unstable_UserBlockingPriority, hl = We.unstable_NormalPriority, Wd = We.unstable_LowPriority, ju = We.unstable_IdlePriority, Ol = null, ft = null;
function Qd(e) {
  if (ft && typeof ft.onCommitFiberRoot == "function") try {
    ft.onCommitFiberRoot(Ol, e, void 0, (e.current.flags & 128) === 128);
  } catch {
  }
}
var it = Math.clz32 ? Math.clz32 : qd, Gd = Math.log, Kd = Math.LN2;
function qd(e) {
  return e >>>= 0, e === 0 ? 32 : 31 - (Gd(e) / Kd | 0) | 0;
}
var Ur = 64, Vr = 4194304;
function Jn(e) {
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
function vl(e, t) {
  var n = e.pendingLanes;
  if (n === 0) return 0;
  var r = 0, l = e.suspendedLanes, i = e.pingedLanes, o = n & 268435455;
  if (o !== 0) {
    var s = o & ~l;
    s !== 0 ? r = Jn(s) : (i &= o, i !== 0 && (r = Jn(i)));
  } else o = n & ~l, o !== 0 ? r = Jn(o) : i !== 0 && (r = Jn(i));
  if (r === 0) return 0;
  if (t !== 0 && t !== r && !(t & l) && (l = r & -r, i = t & -t, l >= i || l === 16 && (i & 4194240) !== 0)) return t;
  if (r & 4 && (r |= n & 16), t = e.entangledLanes, t !== 0) for (e = e.entanglements, t &= r; 0 < t; ) n = 31 - it(t), l = 1 << n, r |= e[n], t &= ~l;
  return r;
}
function Yd(e, t) {
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
function Xd(e, t) {
  for (var n = e.suspendedLanes, r = e.pingedLanes, l = e.expirationTimes, i = e.pendingLanes; 0 < i; ) {
    var o = 31 - it(i), s = 1 << o, u = l[o];
    u === -1 ? (!(s & n) || s & r) && (l[o] = Yd(s, t)) : u <= t && (e.expiredLanes |= s), i &= ~s;
  }
}
function qa(e) {
  return e = e.pendingLanes & -1073741825, e !== 0 ? e : e & 1073741824 ? 1073741824 : 0;
}
function Nu() {
  var e = Ur;
  return Ur <<= 1, !(Ur & 4194240) && (Ur = 64), e;
}
function ca(e) {
  for (var t = [], n = 0; 31 > n; n++) t.push(e);
  return t;
}
function Fr(e, t, n) {
  e.pendingLanes |= t, t !== 536870912 && (e.suspendedLanes = 0, e.pingedLanes = 0), e = e.eventTimes, t = 31 - it(t), e[t] = n;
}
function bd(e, t) {
  var n = e.pendingLanes & ~t;
  e.pendingLanes = t, e.suspendedLanes = 0, e.pingedLanes = 0, e.expiredLanes &= t, e.mutableReadLanes &= t, e.entangledLanes &= t, t = e.entanglements;
  var r = e.eventTimes;
  for (e = e.expirationTimes; 0 < n; ) {
    var l = 31 - it(n), i = 1 << l;
    t[l] = 0, r[l] = -1, e[l] = -1, n &= ~i;
  }
}
function Oi(e, t) {
  var n = e.entangledLanes |= t;
  for (e = e.entanglements; n; ) {
    var r = 31 - it(n), l = 1 << r;
    l & t | e[r] & t && (e[r] |= t), n &= ~l;
  }
}
var G = 0;
function wu(e) {
  return e &= -e, 1 < e ? 4 < e ? e & 268435455 ? 16 : 536870912 : 4 : 1;
}
var Su, $i, ku, Cu, Eu, Ya = !1, Br = [], Lt = null, Mt = null, Ot = null, mr = /* @__PURE__ */ new Map(), hr = /* @__PURE__ */ new Map(), zt = [], Zd = "mousedown mouseup touchcancel touchend touchstart auxclick dblclick pointercancel pointerdown pointerup dragend dragstart drop compositionend compositionstart keydown keypress keyup input textInput copy cut paste click change contextmenu reset submit".split(" ");
function Bo(e, t) {
  switch (e) {
    case "focusin":
    case "focusout":
      Lt = null;
      break;
    case "dragenter":
    case "dragleave":
      Mt = null;
      break;
    case "mouseover":
    case "mouseout":
      Ot = null;
      break;
    case "pointerover":
    case "pointerout":
      mr.delete(t.pointerId);
      break;
    case "gotpointercapture":
    case "lostpointercapture":
      hr.delete(t.pointerId);
  }
}
function Qn(e, t, n, r, l, i) {
  return e === null || e.nativeEvent !== i ? (e = { blockedOn: t, domEventName: n, eventSystemFlags: r, nativeEvent: i, targetContainers: [l] }, t !== null && (t = Tr(t), t !== null && $i(t)), e) : (e.eventSystemFlags |= r, t = e.targetContainers, l !== null && t.indexOf(l) === -1 && t.push(l), e);
}
function Jd(e, t, n, r, l) {
  switch (t) {
    case "focusin":
      return Lt = Qn(Lt, e, t, n, r, l), !0;
    case "dragenter":
      return Mt = Qn(Mt, e, t, n, r, l), !0;
    case "mouseover":
      return Ot = Qn(Ot, e, t, n, r, l), !0;
    case "pointerover":
      var i = l.pointerId;
      return mr.set(i, Qn(mr.get(i) || null, e, t, n, r, l)), !0;
    case "gotpointercapture":
      return i = l.pointerId, hr.set(i, Qn(hr.get(i) || null, e, t, n, r, l)), !0;
  }
  return !1;
}
function Iu(e) {
  var t = Zt(e.target);
  if (t !== null) {
    var n = dn(t);
    if (n !== null) {
      if (t = n.tag, t === 13) {
        if (t = hu(n), t !== null) {
          e.blockedOn = t, Eu(e.priority, function() {
            ku(n);
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
function tl(e) {
  if (e.blockedOn !== null) return !1;
  for (var t = e.targetContainers; 0 < t.length; ) {
    var n = Xa(e.domEventName, e.eventSystemFlags, t[0], e.nativeEvent);
    if (n === null) {
      n = e.nativeEvent;
      var r = new n.constructor(n.type, n);
      Wa = r, n.target.dispatchEvent(r), Wa = null;
    } else return t = Tr(n), t !== null && $i(t), e.blockedOn = n, !1;
    t.shift();
  }
  return !0;
}
function Ho(e, t, n) {
  tl(e) && n.delete(t);
}
function ef() {
  Ya = !1, Lt !== null && tl(Lt) && (Lt = null), Mt !== null && tl(Mt) && (Mt = null), Ot !== null && tl(Ot) && (Ot = null), mr.forEach(Ho), hr.forEach(Ho);
}
function Gn(e, t) {
  e.blockedOn === t && (e.blockedOn = null, Ya || (Ya = !0, We.unstable_scheduleCallback(We.unstable_NormalPriority, ef)));
}
function vr(e) {
  function t(l) {
    return Gn(l, e);
  }
  if (0 < Br.length) {
    Gn(Br[0], e);
    for (var n = 1; n < Br.length; n++) {
      var r = Br[n];
      r.blockedOn === e && (r.blockedOn = null);
    }
  }
  for (Lt !== null && Gn(Lt, e), Mt !== null && Gn(Mt, e), Ot !== null && Gn(Ot, e), mr.forEach(t), hr.forEach(t), n = 0; n < zt.length; n++) r = zt[n], r.blockedOn === e && (r.blockedOn = null);
  for (; 0 < zt.length && (n = zt[0], n.blockedOn === null); ) Iu(n), n.blockedOn === null && zt.shift();
}
var _n = Et.ReactCurrentBatchConfig, gl = !0;
function tf(e, t, n, r) {
  var l = G, i = _n.transition;
  _n.transition = null;
  try {
    G = 1, Ai(e, t, n, r);
  } finally {
    G = l, _n.transition = i;
  }
}
function nf(e, t, n, r) {
  var l = G, i = _n.transition;
  _n.transition = null;
  try {
    G = 4, Ai(e, t, n, r);
  } finally {
    G = l, _n.transition = i;
  }
}
function Ai(e, t, n, r) {
  if (gl) {
    var l = Xa(e, t, n, r);
    if (l === null) ja(e, t, r, xl, n), Bo(e, r);
    else if (Jd(l, e, t, n, r)) r.stopPropagation();
    else if (Bo(e, r), t & 4 && -1 < Zd.indexOf(e)) {
      for (; l !== null; ) {
        var i = Tr(l);
        if (i !== null && Su(i), i = Xa(e, t, n, r), i === null && ja(e, t, r, xl, n), i === l) break;
        l = i;
      }
      l !== null && r.stopPropagation();
    } else ja(e, t, r, null, n);
  }
}
var xl = null;
function Xa(e, t, n, r) {
  if (xl = null, e = Li(r), e = Zt(e), e !== null) if (t = dn(e), t === null) e = null;
  else if (n = t.tag, n === 13) {
    if (e = hu(t), e !== null) return e;
    e = null;
  } else if (n === 3) {
    if (t.stateNode.current.memoizedState.isDehydrated) return t.tag === 3 ? t.stateNode.containerInfo : null;
    e = null;
  } else t !== e && (e = null);
  return xl = e, null;
}
function Pu(e) {
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
        case Mi:
          return 1;
        case yu:
          return 4;
        case hl:
        case Wd:
          return 16;
        case ju:
          return 536870912;
        default:
          return 16;
      }
    default:
      return 16;
  }
}
var Rt = null, Ui = null, nl = null;
function _u() {
  if (nl) return nl;
  var e, t = Ui, n = t.length, r, l = "value" in Rt ? Rt.value : Rt.textContent, i = l.length;
  for (e = 0; e < n && t[e] === l[e]; e++) ;
  var o = n - e;
  for (r = 1; r <= o && t[n - r] === l[i - r]; r++) ;
  return nl = l.slice(e, 1 < r ? 1 - r : void 0);
}
function rl(e) {
  var t = e.keyCode;
  return "charCode" in e ? (e = e.charCode, e === 0 && t === 13 && (e = 13)) : e = t, e === 10 && (e = 13), 32 <= e || e === 13 ? e : 0;
}
function Hr() {
  return !0;
}
function Wo() {
  return !1;
}
function Ge(e) {
  function t(n, r, l, i, o) {
    this._reactName = n, this._targetInst = l, this.type = r, this.nativeEvent = i, this.target = o, this.currentTarget = null;
    for (var s in e) e.hasOwnProperty(s) && (n = e[s], this[s] = n ? n(i) : i[s]);
    return this.isDefaultPrevented = (i.defaultPrevented != null ? i.defaultPrevented : i.returnValue === !1) ? Hr : Wo, this.isPropagationStopped = Wo, this;
  }
  return le(t.prototype, { preventDefault: function() {
    this.defaultPrevented = !0;
    var n = this.nativeEvent;
    n && (n.preventDefault ? n.preventDefault() : typeof n.returnValue != "unknown" && (n.returnValue = !1), this.isDefaultPrevented = Hr);
  }, stopPropagation: function() {
    var n = this.nativeEvent;
    n && (n.stopPropagation ? n.stopPropagation() : typeof n.cancelBubble != "unknown" && (n.cancelBubble = !0), this.isPropagationStopped = Hr);
  }, persist: function() {
  }, isPersistent: Hr }), t;
}
var Un = { eventPhase: 0, bubbles: 0, cancelable: 0, timeStamp: function(e) {
  return e.timeStamp || Date.now();
}, defaultPrevented: 0, isTrusted: 0 }, Vi = Ge(Un), zr = le({}, Un, { view: 0, detail: 0 }), rf = Ge(zr), da, fa, Kn, $l = le({}, zr, { screenX: 0, screenY: 0, clientX: 0, clientY: 0, pageX: 0, pageY: 0, ctrlKey: 0, shiftKey: 0, altKey: 0, metaKey: 0, getModifierState: Bi, button: 0, buttons: 0, relatedTarget: function(e) {
  return e.relatedTarget === void 0 ? e.fromElement === e.srcElement ? e.toElement : e.fromElement : e.relatedTarget;
}, movementX: function(e) {
  return "movementX" in e ? e.movementX : (e !== Kn && (Kn && e.type === "mousemove" ? (da = e.screenX - Kn.screenX, fa = e.screenY - Kn.screenY) : fa = da = 0, Kn = e), da);
}, movementY: function(e) {
  return "movementY" in e ? e.movementY : fa;
} }), Qo = Ge($l), lf = le({}, $l, { dataTransfer: 0 }), af = Ge(lf), of = le({}, zr, { relatedTarget: 0 }), pa = Ge(of), sf = le({}, Un, { animationName: 0, elapsedTime: 0, pseudoElement: 0 }), uf = Ge(sf), cf = le({}, Un, { clipboardData: function(e) {
  return "clipboardData" in e ? e.clipboardData : window.clipboardData;
} }), df = Ge(cf), ff = le({}, Un, { data: 0 }), Go = Ge(ff), pf = {
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
function Bi() {
  return vf;
}
var gf = le({}, zr, { key: function(e) {
  if (e.key) {
    var t = pf[e.key] || e.key;
    if (t !== "Unidentified") return t;
  }
  return e.type === "keypress" ? (e = rl(e), e === 13 ? "Enter" : String.fromCharCode(e)) : e.type === "keydown" || e.type === "keyup" ? mf[e.keyCode] || "Unidentified" : "";
}, code: 0, location: 0, ctrlKey: 0, shiftKey: 0, altKey: 0, metaKey: 0, repeat: 0, locale: 0, getModifierState: Bi, charCode: function(e) {
  return e.type === "keypress" ? rl(e) : 0;
}, keyCode: function(e) {
  return e.type === "keydown" || e.type === "keyup" ? e.keyCode : 0;
}, which: function(e) {
  return e.type === "keypress" ? rl(e) : e.type === "keydown" || e.type === "keyup" ? e.keyCode : 0;
} }), xf = Ge(gf), yf = le({}, $l, { pointerId: 0, width: 0, height: 0, pressure: 0, tangentialPressure: 0, tiltX: 0, tiltY: 0, twist: 0, pointerType: 0, isPrimary: 0 }), Ko = Ge(yf), jf = le({}, zr, { touches: 0, targetTouches: 0, changedTouches: 0, altKey: 0, metaKey: 0, ctrlKey: 0, shiftKey: 0, getModifierState: Bi }), Nf = Ge(jf), wf = le({}, Un, { propertyName: 0, elapsedTime: 0, pseudoElement: 0 }), Sf = Ge(wf), kf = le({}, $l, {
  deltaX: function(e) {
    return "deltaX" in e ? e.deltaX : "wheelDeltaX" in e ? -e.wheelDeltaX : 0;
  },
  deltaY: function(e) {
    return "deltaY" in e ? e.deltaY : "wheelDeltaY" in e ? -e.wheelDeltaY : "wheelDelta" in e ? -e.wheelDelta : 0;
  },
  deltaZ: 0,
  deltaMode: 0
}), Cf = Ge(kf), Ef = [9, 13, 27, 32], Hi = wt && "CompositionEvent" in window, rr = null;
wt && "documentMode" in document && (rr = document.documentMode);
var If = wt && "TextEvent" in window && !rr, Fu = wt && (!Hi || rr && 8 < rr && 11 >= rr), qo = " ", Yo = !1;
function zu(e, t) {
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
function Tu(e) {
  return e = e.detail, typeof e == "object" && "data" in e ? e.data : null;
}
var vn = !1;
function Pf(e, t) {
  switch (e) {
    case "compositionend":
      return Tu(t);
    case "keypress":
      return t.which !== 32 ? null : (Yo = !0, qo);
    case "textInput":
      return e = t.data, e === qo && Yo ? null : e;
    default:
      return null;
  }
}
function _f(e, t) {
  if (vn) return e === "compositionend" || !Hi && zu(e, t) ? (e = _u(), nl = Ui = Rt = null, vn = !1, e) : null;
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
      return Fu && t.locale !== "ko" ? null : t.data;
    default:
      return null;
  }
}
var Ff = { color: !0, date: !0, datetime: !0, "datetime-local": !0, email: !0, month: !0, number: !0, password: !0, range: !0, search: !0, tel: !0, text: !0, time: !0, url: !0, week: !0 };
function Xo(e) {
  var t = e && e.nodeName && e.nodeName.toLowerCase();
  return t === "input" ? !!Ff[e.type] : t === "textarea";
}
function Ru(e, t, n, r) {
  cu(r), t = yl(t, "onChange"), 0 < t.length && (n = new Vi("onChange", "change", null, n, r), e.push({ event: n, listeners: t }));
}
var lr = null, gr = null;
function zf(e) {
  Wu(e, 0);
}
function Al(e) {
  var t = yn(e);
  if (ru(t)) return e;
}
function Tf(e, t) {
  if (e === "change") return t;
}
var Du = !1;
if (wt) {
  var ma;
  if (wt) {
    var ha = "oninput" in document;
    if (!ha) {
      var bo = document.createElement("div");
      bo.setAttribute("oninput", "return;"), ha = typeof bo.oninput == "function";
    }
    ma = ha;
  } else ma = !1;
  Du = ma && (!document.documentMode || 9 < document.documentMode);
}
function Zo() {
  lr && (lr.detachEvent("onpropertychange", Lu), gr = lr = null);
}
function Lu(e) {
  if (e.propertyName === "value" && Al(gr)) {
    var t = [];
    Ru(t, gr, e, Li(e)), mu(zf, t);
  }
}
function Rf(e, t, n) {
  e === "focusin" ? (Zo(), lr = t, gr = n, lr.attachEvent("onpropertychange", Lu)) : e === "focusout" && Zo();
}
function Df(e) {
  if (e === "selectionchange" || e === "keyup" || e === "keydown") return Al(gr);
}
function Lf(e, t) {
  if (e === "click") return Al(t);
}
function Mf(e, t) {
  if (e === "input" || e === "change") return Al(t);
}
function Of(e, t) {
  return e === t && (e !== 0 || 1 / e === 1 / t) || e !== e && t !== t;
}
var st = typeof Object.is == "function" ? Object.is : Of;
function xr(e, t) {
  if (st(e, t)) return !0;
  if (typeof e != "object" || e === null || typeof t != "object" || t === null) return !1;
  var n = Object.keys(e), r = Object.keys(t);
  if (n.length !== r.length) return !1;
  for (r = 0; r < n.length; r++) {
    var l = n[r];
    if (!Ta.call(t, l) || !st(e[l], t[l])) return !1;
  }
  return !0;
}
function Jo(e) {
  for (; e && e.firstChild; ) e = e.firstChild;
  return e;
}
function es(e, t) {
  var n = Jo(e);
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
    n = Jo(n);
  }
}
function Mu(e, t) {
  return e && t ? e === t ? !0 : e && e.nodeType === 3 ? !1 : t && t.nodeType === 3 ? Mu(e, t.parentNode) : "contains" in e ? e.contains(t) : e.compareDocumentPosition ? !!(e.compareDocumentPosition(t) & 16) : !1 : !1;
}
function Ou() {
  for (var e = window, t = fl(); t instanceof e.HTMLIFrameElement; ) {
    try {
      var n = typeof t.contentWindow.location.href == "string";
    } catch {
      n = !1;
    }
    if (n) e = t.contentWindow;
    else break;
    t = fl(e.document);
  }
  return t;
}
function Wi(e) {
  var t = e && e.nodeName && e.nodeName.toLowerCase();
  return t && (t === "input" && (e.type === "text" || e.type === "search" || e.type === "tel" || e.type === "url" || e.type === "password") || t === "textarea" || e.contentEditable === "true");
}
function $f(e) {
  var t = Ou(), n = e.focusedElem, r = e.selectionRange;
  if (t !== n && n && n.ownerDocument && Mu(n.ownerDocument.documentElement, n)) {
    if (r !== null && Wi(n)) {
      if (t = r.start, e = r.end, e === void 0 && (e = t), "selectionStart" in n) n.selectionStart = t, n.selectionEnd = Math.min(e, n.value.length);
      else if (e = (t = n.ownerDocument || document) && t.defaultView || window, e.getSelection) {
        e = e.getSelection();
        var l = n.textContent.length, i = Math.min(r.start, l);
        r = r.end === void 0 ? i : Math.min(r.end, l), !e.extend && i > r && (l = r, r = i, i = l), l = es(n, i);
        var o = es(
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
var Af = wt && "documentMode" in document && 11 >= document.documentMode, gn = null, ba = null, ar = null, Za = !1;
function ts(e, t, n) {
  var r = n.window === n ? n.document : n.nodeType === 9 ? n : n.ownerDocument;
  Za || gn == null || gn !== fl(r) || (r = gn, "selectionStart" in r && Wi(r) ? r = { start: r.selectionStart, end: r.selectionEnd } : (r = (r.ownerDocument && r.ownerDocument.defaultView || window).getSelection(), r = { anchorNode: r.anchorNode, anchorOffset: r.anchorOffset, focusNode: r.focusNode, focusOffset: r.focusOffset }), ar && xr(ar, r) || (ar = r, r = yl(ba, "onSelect"), 0 < r.length && (t = new Vi("onSelect", "select", null, t, n), e.push({ event: t, listeners: r }), t.target = gn)));
}
function Wr(e, t) {
  var n = {};
  return n[e.toLowerCase()] = t.toLowerCase(), n["Webkit" + e] = "webkit" + t, n["Moz" + e] = "moz" + t, n;
}
var xn = { animationend: Wr("Animation", "AnimationEnd"), animationiteration: Wr("Animation", "AnimationIteration"), animationstart: Wr("Animation", "AnimationStart"), transitionend: Wr("Transition", "TransitionEnd") }, va = {}, $u = {};
wt && ($u = document.createElement("div").style, "AnimationEvent" in window || (delete xn.animationend.animation, delete xn.animationiteration.animation, delete xn.animationstart.animation), "TransitionEvent" in window || delete xn.transitionend.transition);
function Ul(e) {
  if (va[e]) return va[e];
  if (!xn[e]) return e;
  var t = xn[e], n;
  for (n in t) if (t.hasOwnProperty(n) && n in $u) return va[e] = t[n];
  return e;
}
var Au = Ul("animationend"), Uu = Ul("animationiteration"), Vu = Ul("animationstart"), Bu = Ul("transitionend"), Hu = /* @__PURE__ */ new Map(), ns = "abort auxClick cancel canPlay canPlayThrough click close contextMenu copy cut drag dragEnd dragEnter dragExit dragLeave dragOver dragStart drop durationChange emptied encrypted ended error gotPointerCapture input invalid keyDown keyPress keyUp load loadedData loadedMetadata loadStart lostPointerCapture mouseDown mouseMove mouseOut mouseOver mouseUp paste pause play playing pointerCancel pointerDown pointerMove pointerOut pointerOver pointerUp progress rateChange reset resize seeked seeking stalled submit suspend timeUpdate touchCancel touchEnd touchStart volumeChange scroll toggle touchMove waiting wheel".split(" ");
function Qt(e, t) {
  Hu.set(e, t), cn(t, [e]);
}
for (var ga = 0; ga < ns.length; ga++) {
  var xa = ns[ga], Uf = xa.toLowerCase(), Vf = xa[0].toUpperCase() + xa.slice(1);
  Qt(Uf, "on" + Vf);
}
Qt(Au, "onAnimationEnd");
Qt(Uu, "onAnimationIteration");
Qt(Vu, "onAnimationStart");
Qt("dblclick", "onDoubleClick");
Qt("focusin", "onFocus");
Qt("focusout", "onBlur");
Qt(Bu, "onTransitionEnd");
Tn("onMouseEnter", ["mouseout", "mouseover"]);
Tn("onMouseLeave", ["mouseout", "mouseover"]);
Tn("onPointerEnter", ["pointerout", "pointerover"]);
Tn("onPointerLeave", ["pointerout", "pointerover"]);
cn("onChange", "change click focusin focusout input keydown keyup selectionchange".split(" "));
cn("onSelect", "focusout contextmenu dragend focusin keydown keyup mousedown mouseup selectionchange".split(" "));
cn("onBeforeInput", ["compositionend", "keypress", "textInput", "paste"]);
cn("onCompositionEnd", "compositionend focusout keydown keypress keyup mousedown".split(" "));
cn("onCompositionStart", "compositionstart focusout keydown keypress keyup mousedown".split(" "));
cn("onCompositionUpdate", "compositionupdate focusout keydown keypress keyup mousedown".split(" "));
var er = "abort canplay canplaythrough durationchange emptied encrypted ended error loadeddata loadedmetadata loadstart pause play playing progress ratechange resize seeked seeking stalled suspend timeupdate volumechange waiting".split(" "), Bf = new Set("cancel close invalid load scroll toggle".split(" ").concat(er));
function rs(e, t, n) {
  var r = e.type || "unknown-event";
  e.currentTarget = n, Ad(r, t, void 0, e), e.currentTarget = null;
}
function Wu(e, t) {
  t = (t & 4) !== 0;
  for (var n = 0; n < e.length; n++) {
    var r = e[n], l = r.event;
    r = r.listeners;
    e: {
      var i = void 0;
      if (t) for (var o = r.length - 1; 0 <= o; o--) {
        var s = r[o], u = s.instance, d = s.currentTarget;
        if (s = s.listener, u !== i && l.isPropagationStopped()) break e;
        rs(l, s, d), i = u;
      }
      else for (o = 0; o < r.length; o++) {
        if (s = r[o], u = s.instance, d = s.currentTarget, s = s.listener, u !== i && l.isPropagationStopped()) break e;
        rs(l, s, d), i = u;
      }
    }
  }
  if (ml) throw e = Ka, ml = !1, Ka = null, e;
}
function Z(e, t) {
  var n = t[ri];
  n === void 0 && (n = t[ri] = /* @__PURE__ */ new Set());
  var r = e + "__bubble";
  n.has(r) || (Qu(t, e, 2, !1), n.add(r));
}
function ya(e, t, n) {
  var r = 0;
  t && (r |= 4), Qu(n, e, r, t);
}
var Qr = "_reactListening" + Math.random().toString(36).slice(2);
function yr(e) {
  if (!e[Qr]) {
    e[Qr] = !0, Zs.forEach(function(n) {
      n !== "selectionchange" && (Bf.has(n) || ya(n, !1, e), ya(n, !0, e));
    });
    var t = e.nodeType === 9 ? e : e.ownerDocument;
    t === null || t[Qr] || (t[Qr] = !0, ya("selectionchange", !1, t));
  }
}
function Qu(e, t, n, r) {
  switch (Pu(t)) {
    case 1:
      var l = tf;
      break;
    case 4:
      l = nf;
      break;
    default:
      l = Ai;
  }
  n = l.bind(null, t, n, e), l = void 0, !Ga || t !== "touchstart" && t !== "touchmove" && t !== "wheel" || (l = !0), r ? l !== void 0 ? e.addEventListener(t, n, { capture: !0, passive: l }) : e.addEventListener(t, n, !0) : l !== void 0 ? e.addEventListener(t, n, { passive: l }) : e.addEventListener(t, n, !1);
}
function ja(e, t, n, r, l) {
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
        if (o = Zt(s), o === null) return;
        if (u = o.tag, u === 5 || u === 6) {
          r = i = o;
          continue e;
        }
        s = s.parentNode;
      }
    }
    r = r.return;
  }
  mu(function() {
    var d = i, j = Li(n), c = [];
    e: {
      var m = Hu.get(e);
      if (m !== void 0) {
        var v = Vi, g = e;
        switch (e) {
          case "keypress":
            if (rl(n) === 0) break e;
          case "keydown":
          case "keyup":
            v = xf;
            break;
          case "focusin":
            g = "focus", v = pa;
            break;
          case "focusout":
            g = "blur", v = pa;
            break;
          case "beforeblur":
          case "afterblur":
            v = pa;
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
            v = Qo;
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
          case Au:
          case Uu:
          case Vu:
            v = uf;
            break;
          case Bu:
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
            v = Ko;
        }
        var w = (t & 4) !== 0, z = !w && e === "scroll", p = w ? m !== null ? m + "Capture" : null : m;
        w = [];
        for (var f = d, h; f !== null; ) {
          h = f;
          var C = h.stateNode;
          if (h.tag === 5 && C !== null && (h = C, p !== null && (C = pr(f, p), C != null && w.push(jr(f, C, h)))), z) break;
          f = f.return;
        }
        0 < w.length && (m = new v(m, g, null, n, j), c.push({ event: m, listeners: w }));
      }
    }
    if (!(t & 7)) {
      e: {
        if (m = e === "mouseover" || e === "pointerover", v = e === "mouseout" || e === "pointerout", m && n !== Wa && (g = n.relatedTarget || n.fromElement) && (Zt(g) || g[St])) break e;
        if ((v || m) && (m = j.window === j ? j : (m = j.ownerDocument) ? m.defaultView || m.parentWindow : window, v ? (g = n.relatedTarget || n.toElement, v = d, g = g ? Zt(g) : null, g !== null && (z = dn(g), g !== z || g.tag !== 5 && g.tag !== 6) && (g = null)) : (v = null, g = d), v !== g)) {
          if (w = Qo, C = "onMouseLeave", p = "onMouseEnter", f = "mouse", (e === "pointerout" || e === "pointerover") && (w = Ko, C = "onPointerLeave", p = "onPointerEnter", f = "pointer"), z = v == null ? m : yn(v), h = g == null ? m : yn(g), m = new w(C, f + "leave", v, n, j), m.target = z, m.relatedTarget = h, C = null, Zt(j) === d && (w = new w(p, f + "enter", g, n, j), w.target = h, w.relatedTarget = z, C = w), z = C, v && g) t: {
            for (w = v, p = g, f = 0, h = w; h; h = pn(h)) f++;
            for (h = 0, C = p; C; C = pn(C)) h++;
            for (; 0 < f - h; ) w = pn(w), f--;
            for (; 0 < h - f; ) p = pn(p), h--;
            for (; f--; ) {
              if (w === p || p !== null && w === p.alternate) break t;
              w = pn(w), p = pn(p);
            }
            w = null;
          }
          else w = null;
          v !== null && ls(c, m, v, w, !1), g !== null && z !== null && ls(c, z, g, w, !0);
        }
      }
      e: {
        if (m = d ? yn(d) : window, v = m.nodeName && m.nodeName.toLowerCase(), v === "select" || v === "input" && m.type === "file") var T = Tf;
        else if (Xo(m)) if (Du) T = Mf;
        else {
          T = Df;
          var F = Rf;
        }
        else (v = m.nodeName) && v.toLowerCase() === "input" && (m.type === "checkbox" || m.type === "radio") && (T = Lf);
        if (T && (T = T(e, d))) {
          Ru(c, T, n, j);
          break e;
        }
        F && F(e, m, d), e === "focusout" && (F = m._wrapperState) && F.controlled && m.type === "number" && Aa(m, "number", m.value);
      }
      switch (F = d ? yn(d) : window, e) {
        case "focusin":
          (Xo(F) || F.contentEditable === "true") && (gn = F, ba = d, ar = null);
          break;
        case "focusout":
          ar = ba = gn = null;
          break;
        case "mousedown":
          Za = !0;
          break;
        case "contextmenu":
        case "mouseup":
        case "dragend":
          Za = !1, ts(c, n, j);
          break;
        case "selectionchange":
          if (Af) break;
        case "keydown":
        case "keyup":
          ts(c, n, j);
      }
      var R;
      if (Hi) e: {
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
      else vn ? zu(e, n) && (D = "onCompositionEnd") : e === "keydown" && n.keyCode === 229 && (D = "onCompositionStart");
      D && (Fu && n.locale !== "ko" && (vn || D !== "onCompositionStart" ? D === "onCompositionEnd" && vn && (R = _u()) : (Rt = j, Ui = "value" in Rt ? Rt.value : Rt.textContent, vn = !0)), F = yl(d, D), 0 < F.length && (D = new Go(D, e, null, n, j), c.push({ event: D, listeners: F }), R ? D.data = R : (R = Tu(n), R !== null && (D.data = R)))), (R = If ? Pf(e, n) : _f(e, n)) && (d = yl(d, "onBeforeInput"), 0 < d.length && (j = new Go("onBeforeInput", "beforeinput", null, n, j), c.push({ event: j, listeners: d }), j.data = R));
    }
    Wu(c, t);
  });
}
function jr(e, t, n) {
  return { instance: e, listener: t, currentTarget: n };
}
function yl(e, t) {
  for (var n = t + "Capture", r = []; e !== null; ) {
    var l = e, i = l.stateNode;
    l.tag === 5 && i !== null && (l = i, i = pr(e, n), i != null && r.unshift(jr(e, i, l)), i = pr(e, t), i != null && r.push(jr(e, i, l))), e = e.return;
  }
  return r;
}
function pn(e) {
  if (e === null) return null;
  do
    e = e.return;
  while (e && e.tag !== 5);
  return e || null;
}
function ls(e, t, n, r, l) {
  for (var i = t._reactName, o = []; n !== null && n !== r; ) {
    var s = n, u = s.alternate, d = s.stateNode;
    if (u !== null && u === r) break;
    s.tag === 5 && d !== null && (s = d, l ? (u = pr(n, i), u != null && o.unshift(jr(n, u, s))) : l || (u = pr(n, i), u != null && o.push(jr(n, u, s)))), n = n.return;
  }
  o.length !== 0 && e.push({ event: t, listeners: o });
}
var Hf = /\r\n?/g, Wf = /\u0000|\uFFFD/g;
function as(e) {
  return (typeof e == "string" ? e : "" + e).replace(Hf, `
`).replace(Wf, "");
}
function Gr(e, t, n) {
  if (t = as(t), as(e) !== t && n) throw Error(P(425));
}
function jl() {
}
var Ja = null, ei = null;
function ti(e, t) {
  return e === "textarea" || e === "noscript" || typeof t.children == "string" || typeof t.children == "number" || typeof t.dangerouslySetInnerHTML == "object" && t.dangerouslySetInnerHTML !== null && t.dangerouslySetInnerHTML.__html != null;
}
var ni = typeof setTimeout == "function" ? setTimeout : void 0, Qf = typeof clearTimeout == "function" ? clearTimeout : void 0, is = typeof Promise == "function" ? Promise : void 0, Gf = typeof queueMicrotask == "function" ? queueMicrotask : typeof is < "u" ? function(e) {
  return is.resolve(null).then(e).catch(Kf);
} : ni;
function Kf(e) {
  setTimeout(function() {
    throw e;
  });
}
function Na(e, t) {
  var n = t, r = 0;
  do {
    var l = n.nextSibling;
    if (e.removeChild(n), l && l.nodeType === 8) if (n = l.data, n === "/$") {
      if (r === 0) {
        e.removeChild(l), vr(t);
        return;
      }
      r--;
    } else n !== "$" && n !== "$?" && n !== "$!" || r++;
    n = l;
  } while (n);
  vr(t);
}
function $t(e) {
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
function os(e) {
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
var Vn = Math.random().toString(36).slice(2), dt = "__reactFiber$" + Vn, Nr = "__reactProps$" + Vn, St = "__reactContainer$" + Vn, ri = "__reactEvents$" + Vn, qf = "__reactListeners$" + Vn, Yf = "__reactHandles$" + Vn;
function Zt(e) {
  var t = e[dt];
  if (t) return t;
  for (var n = e.parentNode; n; ) {
    if (t = n[St] || n[dt]) {
      if (n = t.alternate, t.child !== null || n !== null && n.child !== null) for (e = os(e); e !== null; ) {
        if (n = e[dt]) return n;
        e = os(e);
      }
      return t;
    }
    e = n, n = e.parentNode;
  }
  return null;
}
function Tr(e) {
  return e = e[dt] || e[St], !e || e.tag !== 5 && e.tag !== 6 && e.tag !== 13 && e.tag !== 3 ? null : e;
}
function yn(e) {
  if (e.tag === 5 || e.tag === 6) return e.stateNode;
  throw Error(P(33));
}
function Vl(e) {
  return e[Nr] || null;
}
var li = [], jn = -1;
function Gt(e) {
  return { current: e };
}
function J(e) {
  0 > jn || (e.current = li[jn], li[jn] = null, jn--);
}
function X(e, t) {
  jn++, li[jn] = e.current, e.current = t;
}
var Wt = {}, Pe = Gt(Wt), Oe = Gt(!1), rn = Wt;
function Rn(e, t) {
  var n = e.type.contextTypes;
  if (!n) return Wt;
  var r = e.stateNode;
  if (r && r.__reactInternalMemoizedUnmaskedChildContext === t) return r.__reactInternalMemoizedMaskedChildContext;
  var l = {}, i;
  for (i in n) l[i] = t[i];
  return r && (e = e.stateNode, e.__reactInternalMemoizedUnmaskedChildContext = t, e.__reactInternalMemoizedMaskedChildContext = l), l;
}
function $e(e) {
  return e = e.childContextTypes, e != null;
}
function Nl() {
  J(Oe), J(Pe);
}
function ss(e, t, n) {
  if (Pe.current !== Wt) throw Error(P(168));
  X(Pe, t), X(Oe, n);
}
function Gu(e, t, n) {
  var r = e.stateNode;
  if (t = t.childContextTypes, typeof r.getChildContext != "function") return n;
  r = r.getChildContext();
  for (var l in r) if (!(l in t)) throw Error(P(108, Td(e) || "Unknown", l));
  return le({}, n, r);
}
function wl(e) {
  return e = (e = e.stateNode) && e.__reactInternalMemoizedMergedChildContext || Wt, rn = Pe.current, X(Pe, e), X(Oe, Oe.current), !0;
}
function us(e, t, n) {
  var r = e.stateNode;
  if (!r) throw Error(P(169));
  n ? (e = Gu(e, t, rn), r.__reactInternalMemoizedMergedChildContext = e, J(Oe), J(Pe), X(Pe, e)) : J(Oe), X(Oe, n);
}
var xt = null, Bl = !1, wa = !1;
function Ku(e) {
  xt === null ? xt = [e] : xt.push(e);
}
function Xf(e) {
  Bl = !0, Ku(e);
}
function Kt() {
  if (!wa && xt !== null) {
    wa = !0;
    var e = 0, t = G;
    try {
      var n = xt;
      for (G = 1; e < n.length; e++) {
        var r = n[e];
        do
          r = r(!0);
        while (r !== null);
      }
      xt = null, Bl = !1;
    } catch (l) {
      throw xt !== null && (xt = xt.slice(e + 1)), xu(Mi, Kt), l;
    } finally {
      G = t, wa = !1;
    }
  }
  return null;
}
var Nn = [], wn = 0, Sl = null, kl = 0, qe = [], Ye = 0, ln = null, yt = 1, jt = "";
function Xt(e, t) {
  Nn[wn++] = kl, Nn[wn++] = Sl, Sl = e, kl = t;
}
function qu(e, t, n) {
  qe[Ye++] = yt, qe[Ye++] = jt, qe[Ye++] = ln, ln = e;
  var r = yt;
  e = jt;
  var l = 32 - it(r) - 1;
  r &= ~(1 << l), n += 1;
  var i = 32 - it(t) + l;
  if (30 < i) {
    var o = l - l % 5;
    i = (r & (1 << o) - 1).toString(32), r >>= o, l -= o, yt = 1 << 32 - it(t) + l | n << l | r, jt = i + e;
  } else yt = 1 << i | n << l | r, jt = e;
}
function Qi(e) {
  e.return !== null && (Xt(e, 1), qu(e, 1, 0));
}
function Gi(e) {
  for (; e === Sl; ) Sl = Nn[--wn], Nn[wn] = null, kl = Nn[--wn], Nn[wn] = null;
  for (; e === ln; ) ln = qe[--Ye], qe[Ye] = null, jt = qe[--Ye], qe[Ye] = null, yt = qe[--Ye], qe[Ye] = null;
}
var He = null, Be = null, te = !1, lt = null;
function Yu(e, t) {
  var n = Xe(5, null, null, 0);
  n.elementType = "DELETED", n.stateNode = t, n.return = e, t = e.deletions, t === null ? (e.deletions = [n], e.flags |= 16) : t.push(n);
}
function cs(e, t) {
  switch (e.tag) {
    case 5:
      var n = e.type;
      return t = t.nodeType !== 1 || n.toLowerCase() !== t.nodeName.toLowerCase() ? null : t, t !== null ? (e.stateNode = t, He = e, Be = $t(t.firstChild), !0) : !1;
    case 6:
      return t = e.pendingProps === "" || t.nodeType !== 3 ? null : t, t !== null ? (e.stateNode = t, He = e, Be = null, !0) : !1;
    case 13:
      return t = t.nodeType !== 8 ? null : t, t !== null ? (n = ln !== null ? { id: yt, overflow: jt } : null, e.memoizedState = { dehydrated: t, treeContext: n, retryLane: 1073741824 }, n = Xe(18, null, null, 0), n.stateNode = t, n.return = e, e.child = n, He = e, Be = null, !0) : !1;
    default:
      return !1;
  }
}
function ai(e) {
  return (e.mode & 1) !== 0 && (e.flags & 128) === 0;
}
function ii(e) {
  if (te) {
    var t = Be;
    if (t) {
      var n = t;
      if (!cs(e, t)) {
        if (ai(e)) throw Error(P(418));
        t = $t(n.nextSibling);
        var r = He;
        t && cs(e, t) ? Yu(r, n) : (e.flags = e.flags & -4097 | 2, te = !1, He = e);
      }
    } else {
      if (ai(e)) throw Error(P(418));
      e.flags = e.flags & -4097 | 2, te = !1, He = e;
    }
  }
}
function ds(e) {
  for (e = e.return; e !== null && e.tag !== 5 && e.tag !== 3 && e.tag !== 13; ) e = e.return;
  He = e;
}
function Kr(e) {
  if (e !== He) return !1;
  if (!te) return ds(e), te = !0, !1;
  var t;
  if ((t = e.tag !== 3) && !(t = e.tag !== 5) && (t = e.type, t = t !== "head" && t !== "body" && !ti(e.type, e.memoizedProps)), t && (t = Be)) {
    if (ai(e)) throw Xu(), Error(P(418));
    for (; t; ) Yu(e, t), t = $t(t.nextSibling);
  }
  if (ds(e), e.tag === 13) {
    if (e = e.memoizedState, e = e !== null ? e.dehydrated : null, !e) throw Error(P(317));
    e: {
      for (e = e.nextSibling, t = 0; e; ) {
        if (e.nodeType === 8) {
          var n = e.data;
          if (n === "/$") {
            if (t === 0) {
              Be = $t(e.nextSibling);
              break e;
            }
            t--;
          } else n !== "$" && n !== "$!" && n !== "$?" || t++;
        }
        e = e.nextSibling;
      }
      Be = null;
    }
  } else Be = He ? $t(e.stateNode.nextSibling) : null;
  return !0;
}
function Xu() {
  for (var e = Be; e; ) e = $t(e.nextSibling);
}
function Dn() {
  Be = He = null, te = !1;
}
function Ki(e) {
  lt === null ? lt = [e] : lt.push(e);
}
var bf = Et.ReactCurrentBatchConfig;
function qn(e, t, n) {
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
function qr(e, t) {
  throw e = Object.prototype.toString.call(t), Error(P(31, e === "[object Object]" ? "object with keys {" + Object.keys(t).join(", ") + "}" : e));
}
function fs(e) {
  var t = e._init;
  return t(e._payload);
}
function bu(e) {
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
    return p = Bt(p, f), p.index = 0, p.sibling = null, p;
  }
  function i(p, f, h) {
    return p.index = h, e ? (h = p.alternate, h !== null ? (h = h.index, h < f ? (p.flags |= 2, f) : h) : (p.flags |= 2, f)) : (p.flags |= 1048576, f);
  }
  function o(p) {
    return e && p.alternate === null && (p.flags |= 2), p;
  }
  function s(p, f, h, C) {
    return f === null || f.tag !== 6 ? (f = _a(h, p.mode, C), f.return = p, f) : (f = l(f, h), f.return = p, f);
  }
  function u(p, f, h, C) {
    var T = h.type;
    return T === hn ? j(p, f, h.props.children, C, h.key) : f !== null && (f.elementType === T || typeof T == "object" && T !== null && T.$$typeof === _t && fs(T) === f.type) ? (C = l(f, h.props), C.ref = qn(p, f, h), C.return = p, C) : (C = cl(h.type, h.key, h.props, null, p.mode, C), C.ref = qn(p, f, h), C.return = p, C);
  }
  function d(p, f, h, C) {
    return f === null || f.tag !== 4 || f.stateNode.containerInfo !== h.containerInfo || f.stateNode.implementation !== h.implementation ? (f = Fa(h, p.mode, C), f.return = p, f) : (f = l(f, h.children || []), f.return = p, f);
  }
  function j(p, f, h, C, T) {
    return f === null || f.tag !== 7 ? (f = nn(h, p.mode, C, T), f.return = p, f) : (f = l(f, h), f.return = p, f);
  }
  function c(p, f, h) {
    if (typeof f == "string" && f !== "" || typeof f == "number") return f = _a("" + f, p.mode, h), f.return = p, f;
    if (typeof f == "object" && f !== null) {
      switch (f.$$typeof) {
        case Or:
          return h = cl(f.type, f.key, f.props, null, p.mode, h), h.ref = qn(p, null, f), h.return = p, h;
        case mn:
          return f = Fa(f, p.mode, h), f.return = p, f;
        case _t:
          var C = f._init;
          return c(p, C(f._payload), h);
      }
      if (Zn(f) || Hn(f)) return f = nn(f, p.mode, h, null), f.return = p, f;
      qr(p, f);
    }
    return null;
  }
  function m(p, f, h, C) {
    var T = f !== null ? f.key : null;
    if (typeof h == "string" && h !== "" || typeof h == "number") return T !== null ? null : s(p, f, "" + h, C);
    if (typeof h == "object" && h !== null) {
      switch (h.$$typeof) {
        case Or:
          return h.key === T ? u(p, f, h, C) : null;
        case mn:
          return h.key === T ? d(p, f, h, C) : null;
        case _t:
          return T = h._init, m(
            p,
            f,
            T(h._payload),
            C
          );
      }
      if (Zn(h) || Hn(h)) return T !== null ? null : j(p, f, h, C, null);
      qr(p, h);
    }
    return null;
  }
  function v(p, f, h, C, T) {
    if (typeof C == "string" && C !== "" || typeof C == "number") return p = p.get(h) || null, s(f, p, "" + C, T);
    if (typeof C == "object" && C !== null) {
      switch (C.$$typeof) {
        case Or:
          return p = p.get(C.key === null ? h : C.key) || null, u(f, p, C, T);
        case mn:
          return p = p.get(C.key === null ? h : C.key) || null, d(f, p, C, T);
        case _t:
          var F = C._init;
          return v(p, f, h, F(C._payload), T);
      }
      if (Zn(C) || Hn(C)) return p = p.get(h) || null, j(f, p, C, T, null);
      qr(f, C);
    }
    return null;
  }
  function g(p, f, h, C) {
    for (var T = null, F = null, R = f, D = f = 0, L = null; R !== null && D < h.length; D++) {
      R.index > D ? (L = R, R = null) : L = R.sibling;
      var E = m(p, R, h[D], C);
      if (E === null) {
        R === null && (R = L);
        break;
      }
      e && R && E.alternate === null && t(p, R), f = i(E, f, D), F === null ? T = E : F.sibling = E, F = E, R = L;
    }
    if (D === h.length) return n(p, R), te && Xt(p, D), T;
    if (R === null) {
      for (; D < h.length; D++) R = c(p, h[D], C), R !== null && (f = i(R, f, D), F === null ? T = R : F.sibling = R, F = R);
      return te && Xt(p, D), T;
    }
    for (R = r(p, R); D < h.length; D++) L = v(R, p, D, h[D], C), L !== null && (e && L.alternate !== null && R.delete(L.key === null ? D : L.key), f = i(L, f, D), F === null ? T = L : F.sibling = L, F = L);
    return e && R.forEach(function(_) {
      return t(p, _);
    }), te && Xt(p, D), T;
  }
  function w(p, f, h, C) {
    var T = Hn(h);
    if (typeof T != "function") throw Error(P(150));
    if (h = T.call(h), h == null) throw Error(P(151));
    for (var F = T = null, R = f, D = f = 0, L = null, E = h.next(); R !== null && !E.done; D++, E = h.next()) {
      R.index > D ? (L = R, R = null) : L = R.sibling;
      var _ = m(p, R, E.value, C);
      if (_ === null) {
        R === null && (R = L);
        break;
      }
      e && R && _.alternate === null && t(p, R), f = i(_, f, D), F === null ? T = _ : F.sibling = _, F = _, R = L;
    }
    if (E.done) return n(
      p,
      R
    ), te && Xt(p, D), T;
    if (R === null) {
      for (; !E.done; D++, E = h.next()) E = c(p, E.value, C), E !== null && (f = i(E, f, D), F === null ? T = E : F.sibling = E, F = E);
      return te && Xt(p, D), T;
    }
    for (R = r(p, R); !E.done; D++, E = h.next()) E = v(R, p, D, E.value, C), E !== null && (e && E.alternate !== null && R.delete(E.key === null ? D : E.key), f = i(E, f, D), F === null ? T = E : F.sibling = E, F = E);
    return e && R.forEach(function(A) {
      return t(p, A);
    }), te && Xt(p, D), T;
  }
  function z(p, f, h, C) {
    if (typeof h == "object" && h !== null && h.type === hn && h.key === null && (h = h.props.children), typeof h == "object" && h !== null) {
      switch (h.$$typeof) {
        case Or:
          e: {
            for (var T = h.key, F = f; F !== null; ) {
              if (F.key === T) {
                if (T = h.type, T === hn) {
                  if (F.tag === 7) {
                    n(p, F.sibling), f = l(F, h.props.children), f.return = p, p = f;
                    break e;
                  }
                } else if (F.elementType === T || typeof T == "object" && T !== null && T.$$typeof === _t && fs(T) === F.type) {
                  n(p, F.sibling), f = l(F, h.props), f.ref = qn(p, F, h), f.return = p, p = f;
                  break e;
                }
                n(p, F);
                break;
              } else t(p, F);
              F = F.sibling;
            }
            h.type === hn ? (f = nn(h.props.children, p.mode, C, h.key), f.return = p, p = f) : (C = cl(h.type, h.key, h.props, null, p.mode, C), C.ref = qn(p, f, h), C.return = p, p = C);
          }
          return o(p);
        case mn:
          e: {
            for (F = h.key; f !== null; ) {
              if (f.key === F) if (f.tag === 4 && f.stateNode.containerInfo === h.containerInfo && f.stateNode.implementation === h.implementation) {
                n(p, f.sibling), f = l(f, h.children || []), f.return = p, p = f;
                break e;
              } else {
                n(p, f);
                break;
              }
              else t(p, f);
              f = f.sibling;
            }
            f = Fa(h, p.mode, C), f.return = p, p = f;
          }
          return o(p);
        case _t:
          return F = h._init, z(p, f, F(h._payload), C);
      }
      if (Zn(h)) return g(p, f, h, C);
      if (Hn(h)) return w(p, f, h, C);
      qr(p, h);
    }
    return typeof h == "string" && h !== "" || typeof h == "number" ? (h = "" + h, f !== null && f.tag === 6 ? (n(p, f.sibling), f = l(f, h), f.return = p, p = f) : (n(p, f), f = _a(h, p.mode, C), f.return = p, p = f), o(p)) : n(p, f);
  }
  return z;
}
var Ln = bu(!0), Zu = bu(!1), Cl = Gt(null), El = null, Sn = null, qi = null;
function Yi() {
  qi = Sn = El = null;
}
function Xi(e) {
  var t = Cl.current;
  J(Cl), e._currentValue = t;
}
function oi(e, t, n) {
  for (; e !== null; ) {
    var r = e.alternate;
    if ((e.childLanes & t) !== t ? (e.childLanes |= t, r !== null && (r.childLanes |= t)) : r !== null && (r.childLanes & t) !== t && (r.childLanes |= t), e === n) break;
    e = e.return;
  }
}
function Fn(e, t) {
  El = e, qi = Sn = null, e = e.dependencies, e !== null && e.firstContext !== null && (e.lanes & t && (Me = !0), e.firstContext = null);
}
function Ze(e) {
  var t = e._currentValue;
  if (qi !== e) if (e = { context: e, memoizedValue: t, next: null }, Sn === null) {
    if (El === null) throw Error(P(308));
    Sn = e, El.dependencies = { lanes: 0, firstContext: e };
  } else Sn = Sn.next = e;
  return t;
}
var Jt = null;
function bi(e) {
  Jt === null ? Jt = [e] : Jt.push(e);
}
function Ju(e, t, n, r) {
  var l = t.interleaved;
  return l === null ? (n.next = n, bi(t)) : (n.next = l.next, l.next = n), t.interleaved = n, kt(e, r);
}
function kt(e, t) {
  e.lanes |= t;
  var n = e.alternate;
  for (n !== null && (n.lanes |= t), n = e, e = e.return; e !== null; ) e.childLanes |= t, n = e.alternate, n !== null && (n.childLanes |= t), n = e, e = e.return;
  return n.tag === 3 ? n.stateNode : null;
}
var Ft = !1;
function Zi(e) {
  e.updateQueue = { baseState: e.memoizedState, firstBaseUpdate: null, lastBaseUpdate: null, shared: { pending: null, interleaved: null, lanes: 0 }, effects: null };
}
function ec(e, t) {
  e = e.updateQueue, t.updateQueue === e && (t.updateQueue = { baseState: e.baseState, firstBaseUpdate: e.firstBaseUpdate, lastBaseUpdate: e.lastBaseUpdate, shared: e.shared, effects: e.effects });
}
function Nt(e, t) {
  return { eventTime: e, lane: t, tag: 0, payload: null, callback: null, next: null };
}
function At(e, t, n) {
  var r = e.updateQueue;
  if (r === null) return null;
  if (r = r.shared, W & 2) {
    var l = r.pending;
    return l === null ? t.next = t : (t.next = l.next, l.next = t), r.pending = t, kt(e, n);
  }
  return l = r.interleaved, l === null ? (t.next = t, bi(r)) : (t.next = l.next, l.next = t), r.interleaved = t, kt(e, n);
}
function ll(e, t, n) {
  if (t = t.updateQueue, t !== null && (t = t.shared, (n & 4194240) !== 0)) {
    var r = t.lanes;
    r &= e.pendingLanes, n |= r, t.lanes = n, Oi(e, n);
  }
}
function ps(e, t) {
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
function Il(e, t, n, r) {
  var l = e.updateQueue;
  Ft = !1;
  var i = l.firstBaseUpdate, o = l.lastBaseUpdate, s = l.shared.pending;
  if (s !== null) {
    l.shared.pending = null;
    var u = s, d = u.next;
    u.next = null, o === null ? i = d : o.next = d, o = u;
    var j = e.alternate;
    j !== null && (j = j.updateQueue, s = j.lastBaseUpdate, s !== o && (s === null ? j.firstBaseUpdate = d : s.next = d, j.lastBaseUpdate = u));
  }
  if (i !== null) {
    var c = l.baseState;
    o = 0, j = d = u = null, s = i;
    do {
      var m = s.lane, v = s.eventTime;
      if ((r & m) === m) {
        j !== null && (j = j.next = {
          eventTime: v,
          lane: 0,
          tag: s.tag,
          payload: s.payload,
          callback: s.callback,
          next: null
        });
        e: {
          var g = e, w = s;
          switch (m = t, v = n, w.tag) {
            case 1:
              if (g = w.payload, typeof g == "function") {
                c = g.call(v, c, m);
                break e;
              }
              c = g;
              break e;
            case 3:
              g.flags = g.flags & -65537 | 128;
            case 0:
              if (g = w.payload, m = typeof g == "function" ? g.call(v, c, m) : g, m == null) break e;
              c = le({}, c, m);
              break e;
            case 2:
              Ft = !0;
          }
        }
        s.callback !== null && s.lane !== 0 && (e.flags |= 64, m = l.effects, m === null ? l.effects = [s] : m.push(s));
      } else v = { eventTime: v, lane: m, tag: s.tag, payload: s.payload, callback: s.callback, next: null }, j === null ? (d = j = v, u = c) : j = j.next = v, o |= m;
      if (s = s.next, s === null) {
        if (s = l.shared.pending, s === null) break;
        m = s, s = m.next, m.next = null, l.lastBaseUpdate = m, l.shared.pending = null;
      }
    } while (!0);
    if (j === null && (u = c), l.baseState = u, l.firstBaseUpdate = d, l.lastBaseUpdate = j, t = l.shared.interleaved, t !== null) {
      l = t;
      do
        o |= l.lane, l = l.next;
      while (l !== t);
    } else i === null && (l.shared.lanes = 0);
    on |= o, e.lanes = o, e.memoizedState = c;
  }
}
function ms(e, t, n) {
  if (e = t.effects, t.effects = null, e !== null) for (t = 0; t < e.length; t++) {
    var r = e[t], l = r.callback;
    if (l !== null) {
      if (r.callback = null, r = n, typeof l != "function") throw Error(P(191, l));
      l.call(r);
    }
  }
}
var Rr = {}, pt = Gt(Rr), wr = Gt(Rr), Sr = Gt(Rr);
function en(e) {
  if (e === Rr) throw Error(P(174));
  return e;
}
function Ji(e, t) {
  switch (X(Sr, t), X(wr, e), X(pt, Rr), e = t.nodeType, e) {
    case 9:
    case 11:
      t = (t = t.documentElement) ? t.namespaceURI : Va(null, "");
      break;
    default:
      e = e === 8 ? t.parentNode : t, t = e.namespaceURI || null, e = e.tagName, t = Va(t, e);
  }
  J(pt), X(pt, t);
}
function Mn() {
  J(pt), J(wr), J(Sr);
}
function tc(e) {
  en(Sr.current);
  var t = en(pt.current), n = Va(t, e.type);
  t !== n && (X(wr, e), X(pt, n));
}
function eo(e) {
  wr.current === e && (J(pt), J(wr));
}
var ne = Gt(0);
function Pl(e) {
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
var Sa = [];
function to() {
  for (var e = 0; e < Sa.length; e++) Sa[e]._workInProgressVersionPrimary = null;
  Sa.length = 0;
}
var al = Et.ReactCurrentDispatcher, ka = Et.ReactCurrentBatchConfig, an = 0, re = null, pe = null, ve = null, _l = !1, ir = !1, kr = 0, Zf = 0;
function ke() {
  throw Error(P(321));
}
function no(e, t) {
  if (t === null) return !1;
  for (var n = 0; n < t.length && n < e.length; n++) if (!st(e[n], t[n])) return !1;
  return !0;
}
function ro(e, t, n, r, l, i) {
  if (an = i, re = t, t.memoizedState = null, t.updateQueue = null, t.lanes = 0, al.current = e === null || e.memoizedState === null ? np : rp, e = n(r, l), ir) {
    i = 0;
    do {
      if (ir = !1, kr = 0, 25 <= i) throw Error(P(301));
      i += 1, ve = pe = null, t.updateQueue = null, al.current = lp, e = n(r, l);
    } while (ir);
  }
  if (al.current = Fl, t = pe !== null && pe.next !== null, an = 0, ve = pe = re = null, _l = !1, t) throw Error(P(300));
  return e;
}
function lo() {
  var e = kr !== 0;
  return kr = 0, e;
}
function ct() {
  var e = { memoizedState: null, baseState: null, baseQueue: null, queue: null, next: null };
  return ve === null ? re.memoizedState = ve = e : ve = ve.next = e, ve;
}
function Je() {
  if (pe === null) {
    var e = re.alternate;
    e = e !== null ? e.memoizedState : null;
  } else e = pe.next;
  var t = ve === null ? re.memoizedState : ve.next;
  if (t !== null) ve = t, pe = e;
  else {
    if (e === null) throw Error(P(310));
    pe = e, e = { memoizedState: pe.memoizedState, baseState: pe.baseState, baseQueue: pe.baseQueue, queue: pe.queue, next: null }, ve === null ? re.memoizedState = ve = e : ve = ve.next = e;
  }
  return ve;
}
function Cr(e, t) {
  return typeof t == "function" ? t(e) : t;
}
function Ca(e) {
  var t = Je(), n = t.queue;
  if (n === null) throw Error(P(311));
  n.lastRenderedReducer = e;
  var r = pe, l = r.baseQueue, i = n.pending;
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
      var j = d.lane;
      if ((an & j) === j) u !== null && (u = u.next = { lane: 0, action: d.action, hasEagerState: d.hasEagerState, eagerState: d.eagerState, next: null }), r = d.hasEagerState ? d.eagerState : e(r, d.action);
      else {
        var c = {
          lane: j,
          action: d.action,
          hasEagerState: d.hasEagerState,
          eagerState: d.eagerState,
          next: null
        };
        u === null ? (s = u = c, o = r) : u = u.next = c, re.lanes |= j, on |= j;
      }
      d = d.next;
    } while (d !== null && d !== i);
    u === null ? o = r : u.next = s, st(r, t.memoizedState) || (Me = !0), t.memoizedState = r, t.baseState = o, t.baseQueue = u, n.lastRenderedState = r;
  }
  if (e = n.interleaved, e !== null) {
    l = e;
    do
      i = l.lane, re.lanes |= i, on |= i, l = l.next;
    while (l !== e);
  } else l === null && (n.lanes = 0);
  return [t.memoizedState, n.dispatch];
}
function Ea(e) {
  var t = Je(), n = t.queue;
  if (n === null) throw Error(P(311));
  n.lastRenderedReducer = e;
  var r = n.dispatch, l = n.pending, i = t.memoizedState;
  if (l !== null) {
    n.pending = null;
    var o = l = l.next;
    do
      i = e(i, o.action), o = o.next;
    while (o !== l);
    st(i, t.memoizedState) || (Me = !0), t.memoizedState = i, t.baseQueue === null && (t.baseState = i), n.lastRenderedState = i;
  }
  return [i, r];
}
function nc() {
}
function rc(e, t) {
  var n = re, r = Je(), l = t(), i = !st(r.memoizedState, l);
  if (i && (r.memoizedState = l, Me = !0), r = r.queue, ao(ic.bind(null, n, r, e), [e]), r.getSnapshot !== t || i || ve !== null && ve.memoizedState.tag & 1) {
    if (n.flags |= 2048, Er(9, ac.bind(null, n, r, l, t), void 0, null), xe === null) throw Error(P(349));
    an & 30 || lc(n, t, l);
  }
  return l;
}
function lc(e, t, n) {
  e.flags |= 16384, e = { getSnapshot: t, value: n }, t = re.updateQueue, t === null ? (t = { lastEffect: null, stores: null }, re.updateQueue = t, t.stores = [e]) : (n = t.stores, n === null ? t.stores = [e] : n.push(e));
}
function ac(e, t, n, r) {
  t.value = n, t.getSnapshot = r, oc(t) && sc(e);
}
function ic(e, t, n) {
  return n(function() {
    oc(t) && sc(e);
  });
}
function oc(e) {
  var t = e.getSnapshot;
  e = e.value;
  try {
    var n = t();
    return !st(e, n);
  } catch {
    return !0;
  }
}
function sc(e) {
  var t = kt(e, 1);
  t !== null && ot(t, e, 1, -1);
}
function hs(e) {
  var t = ct();
  return typeof e == "function" && (e = e()), t.memoizedState = t.baseState = e, e = { pending: null, interleaved: null, lanes: 0, dispatch: null, lastRenderedReducer: Cr, lastRenderedState: e }, t.queue = e, e = e.dispatch = tp.bind(null, re, e), [t.memoizedState, e];
}
function Er(e, t, n, r) {
  return e = { tag: e, create: t, destroy: n, deps: r, next: null }, t = re.updateQueue, t === null ? (t = { lastEffect: null, stores: null }, re.updateQueue = t, t.lastEffect = e.next = e) : (n = t.lastEffect, n === null ? t.lastEffect = e.next = e : (r = n.next, n.next = e, e.next = r, t.lastEffect = e)), e;
}
function uc() {
  return Je().memoizedState;
}
function il(e, t, n, r) {
  var l = ct();
  re.flags |= e, l.memoizedState = Er(1 | t, n, void 0, r === void 0 ? null : r);
}
function Hl(e, t, n, r) {
  var l = Je();
  r = r === void 0 ? null : r;
  var i = void 0;
  if (pe !== null) {
    var o = pe.memoizedState;
    if (i = o.destroy, r !== null && no(r, o.deps)) {
      l.memoizedState = Er(t, n, i, r);
      return;
    }
  }
  re.flags |= e, l.memoizedState = Er(1 | t, n, i, r);
}
function vs(e, t) {
  return il(8390656, 8, e, t);
}
function ao(e, t) {
  return Hl(2048, 8, e, t);
}
function cc(e, t) {
  return Hl(4, 2, e, t);
}
function dc(e, t) {
  return Hl(4, 4, e, t);
}
function fc(e, t) {
  if (typeof t == "function") return e = e(), t(e), function() {
    t(null);
  };
  if (t != null) return e = e(), t.current = e, function() {
    t.current = null;
  };
}
function pc(e, t, n) {
  return n = n != null ? n.concat([e]) : null, Hl(4, 4, fc.bind(null, t, e), n);
}
function io() {
}
function mc(e, t) {
  var n = Je();
  t = t === void 0 ? null : t;
  var r = n.memoizedState;
  return r !== null && t !== null && no(t, r[1]) ? r[0] : (n.memoizedState = [e, t], e);
}
function hc(e, t) {
  var n = Je();
  t = t === void 0 ? null : t;
  var r = n.memoizedState;
  return r !== null && t !== null && no(t, r[1]) ? r[0] : (e = e(), n.memoizedState = [e, t], e);
}
function vc(e, t, n) {
  return an & 21 ? (st(n, t) || (n = Nu(), re.lanes |= n, on |= n, e.baseState = !0), t) : (e.baseState && (e.baseState = !1, Me = !0), e.memoizedState = n);
}
function Jf(e, t) {
  var n = G;
  G = n !== 0 && 4 > n ? n : 4, e(!0);
  var r = ka.transition;
  ka.transition = {};
  try {
    e(!1), t();
  } finally {
    G = n, ka.transition = r;
  }
}
function gc() {
  return Je().memoizedState;
}
function ep(e, t, n) {
  var r = Vt(e);
  if (n = { lane: r, action: n, hasEagerState: !1, eagerState: null, next: null }, xc(e)) yc(t, n);
  else if (n = Ju(e, t, n, r), n !== null) {
    var l = ze();
    ot(n, e, r, l), jc(n, t, r);
  }
}
function tp(e, t, n) {
  var r = Vt(e), l = { lane: r, action: n, hasEagerState: !1, eagerState: null, next: null };
  if (xc(e)) yc(t, l);
  else {
    var i = e.alternate;
    if (e.lanes === 0 && (i === null || i.lanes === 0) && (i = t.lastRenderedReducer, i !== null)) try {
      var o = t.lastRenderedState, s = i(o, n);
      if (l.hasEagerState = !0, l.eagerState = s, st(s, o)) {
        var u = t.interleaved;
        u === null ? (l.next = l, bi(t)) : (l.next = u.next, u.next = l), t.interleaved = l;
        return;
      }
    } catch {
    } finally {
    }
    n = Ju(e, t, l, r), n !== null && (l = ze(), ot(n, e, r, l), jc(n, t, r));
  }
}
function xc(e) {
  var t = e.alternate;
  return e === re || t !== null && t === re;
}
function yc(e, t) {
  ir = _l = !0;
  var n = e.pending;
  n === null ? t.next = t : (t.next = n.next, n.next = t), e.pending = t;
}
function jc(e, t, n) {
  if (n & 4194240) {
    var r = t.lanes;
    r &= e.pendingLanes, n |= r, t.lanes = n, Oi(e, n);
  }
}
var Fl = { readContext: Ze, useCallback: ke, useContext: ke, useEffect: ke, useImperativeHandle: ke, useInsertionEffect: ke, useLayoutEffect: ke, useMemo: ke, useReducer: ke, useRef: ke, useState: ke, useDebugValue: ke, useDeferredValue: ke, useTransition: ke, useMutableSource: ke, useSyncExternalStore: ke, useId: ke, unstable_isNewReconciler: !1 }, np = { readContext: Ze, useCallback: function(e, t) {
  return ct().memoizedState = [e, t === void 0 ? null : t], e;
}, useContext: Ze, useEffect: vs, useImperativeHandle: function(e, t, n) {
  return n = n != null ? n.concat([e]) : null, il(
    4194308,
    4,
    fc.bind(null, t, e),
    n
  );
}, useLayoutEffect: function(e, t) {
  return il(4194308, 4, e, t);
}, useInsertionEffect: function(e, t) {
  return il(4, 2, e, t);
}, useMemo: function(e, t) {
  var n = ct();
  return t = t === void 0 ? null : t, e = e(), n.memoizedState = [e, t], e;
}, useReducer: function(e, t, n) {
  var r = ct();
  return t = n !== void 0 ? n(t) : t, r.memoizedState = r.baseState = t, e = { pending: null, interleaved: null, lanes: 0, dispatch: null, lastRenderedReducer: e, lastRenderedState: t }, r.queue = e, e = e.dispatch = ep.bind(null, re, e), [r.memoizedState, e];
}, useRef: function(e) {
  var t = ct();
  return e = { current: e }, t.memoizedState = e;
}, useState: hs, useDebugValue: io, useDeferredValue: function(e) {
  return ct().memoizedState = e;
}, useTransition: function() {
  var e = hs(!1), t = e[0];
  return e = Jf.bind(null, e[1]), ct().memoizedState = e, [t, e];
}, useMutableSource: function() {
}, useSyncExternalStore: function(e, t, n) {
  var r = re, l = ct();
  if (te) {
    if (n === void 0) throw Error(P(407));
    n = n();
  } else {
    if (n = t(), xe === null) throw Error(P(349));
    an & 30 || lc(r, t, n);
  }
  l.memoizedState = n;
  var i = { value: n, getSnapshot: t };
  return l.queue = i, vs(ic.bind(
    null,
    r,
    i,
    e
  ), [e]), r.flags |= 2048, Er(9, ac.bind(null, r, i, n, t), void 0, null), n;
}, useId: function() {
  var e = ct(), t = xe.identifierPrefix;
  if (te) {
    var n = jt, r = yt;
    n = (r & ~(1 << 32 - it(r) - 1)).toString(32) + n, t = ":" + t + "R" + n, n = kr++, 0 < n && (t += "H" + n.toString(32)), t += ":";
  } else n = Zf++, t = ":" + t + "r" + n.toString(32) + ":";
  return e.memoizedState = t;
}, unstable_isNewReconciler: !1 }, rp = {
  readContext: Ze,
  useCallback: mc,
  useContext: Ze,
  useEffect: ao,
  useImperativeHandle: pc,
  useInsertionEffect: cc,
  useLayoutEffect: dc,
  useMemo: hc,
  useReducer: Ca,
  useRef: uc,
  useState: function() {
    return Ca(Cr);
  },
  useDebugValue: io,
  useDeferredValue: function(e) {
    var t = Je();
    return vc(t, pe.memoizedState, e);
  },
  useTransition: function() {
    var e = Ca(Cr)[0], t = Je().memoizedState;
    return [e, t];
  },
  useMutableSource: nc,
  useSyncExternalStore: rc,
  useId: gc,
  unstable_isNewReconciler: !1
}, lp = { readContext: Ze, useCallback: mc, useContext: Ze, useEffect: ao, useImperativeHandle: pc, useInsertionEffect: cc, useLayoutEffect: dc, useMemo: hc, useReducer: Ea, useRef: uc, useState: function() {
  return Ea(Cr);
}, useDebugValue: io, useDeferredValue: function(e) {
  var t = Je();
  return pe === null ? t.memoizedState = e : vc(t, pe.memoizedState, e);
}, useTransition: function() {
  var e = Ea(Cr)[0], t = Je().memoizedState;
  return [e, t];
}, useMutableSource: nc, useSyncExternalStore: rc, useId: gc, unstable_isNewReconciler: !1 };
function nt(e, t) {
  if (e && e.defaultProps) {
    t = le({}, t), e = e.defaultProps;
    for (var n in e) t[n] === void 0 && (t[n] = e[n]);
    return t;
  }
  return t;
}
function si(e, t, n, r) {
  t = e.memoizedState, n = n(r, t), n = n == null ? t : le({}, t, n), e.memoizedState = n, e.lanes === 0 && (e.updateQueue.baseState = n);
}
var Wl = { isMounted: function(e) {
  return (e = e._reactInternals) ? dn(e) === e : !1;
}, enqueueSetState: function(e, t, n) {
  e = e._reactInternals;
  var r = ze(), l = Vt(e), i = Nt(r, l);
  i.payload = t, n != null && (i.callback = n), t = At(e, i, l), t !== null && (ot(t, e, l, r), ll(t, e, l));
}, enqueueReplaceState: function(e, t, n) {
  e = e._reactInternals;
  var r = ze(), l = Vt(e), i = Nt(r, l);
  i.tag = 1, i.payload = t, n != null && (i.callback = n), t = At(e, i, l), t !== null && (ot(t, e, l, r), ll(t, e, l));
}, enqueueForceUpdate: function(e, t) {
  e = e._reactInternals;
  var n = ze(), r = Vt(e), l = Nt(n, r);
  l.tag = 2, t != null && (l.callback = t), t = At(e, l, r), t !== null && (ot(t, e, r, n), ll(t, e, r));
} };
function gs(e, t, n, r, l, i, o) {
  return e = e.stateNode, typeof e.shouldComponentUpdate == "function" ? e.shouldComponentUpdate(r, i, o) : t.prototype && t.prototype.isPureReactComponent ? !xr(n, r) || !xr(l, i) : !0;
}
function Nc(e, t, n) {
  var r = !1, l = Wt, i = t.contextType;
  return typeof i == "object" && i !== null ? i = Ze(i) : (l = $e(t) ? rn : Pe.current, r = t.contextTypes, i = (r = r != null) ? Rn(e, l) : Wt), t = new t(n, i), e.memoizedState = t.state !== null && t.state !== void 0 ? t.state : null, t.updater = Wl, e.stateNode = t, t._reactInternals = e, r && (e = e.stateNode, e.__reactInternalMemoizedUnmaskedChildContext = l, e.__reactInternalMemoizedMaskedChildContext = i), t;
}
function xs(e, t, n, r) {
  e = t.state, typeof t.componentWillReceiveProps == "function" && t.componentWillReceiveProps(n, r), typeof t.UNSAFE_componentWillReceiveProps == "function" && t.UNSAFE_componentWillReceiveProps(n, r), t.state !== e && Wl.enqueueReplaceState(t, t.state, null);
}
function ui(e, t, n, r) {
  var l = e.stateNode;
  l.props = n, l.state = e.memoizedState, l.refs = {}, Zi(e);
  var i = t.contextType;
  typeof i == "object" && i !== null ? l.context = Ze(i) : (i = $e(t) ? rn : Pe.current, l.context = Rn(e, i)), l.state = e.memoizedState, i = t.getDerivedStateFromProps, typeof i == "function" && (si(e, t, i, n), l.state = e.memoizedState), typeof t.getDerivedStateFromProps == "function" || typeof l.getSnapshotBeforeUpdate == "function" || typeof l.UNSAFE_componentWillMount != "function" && typeof l.componentWillMount != "function" || (t = l.state, typeof l.componentWillMount == "function" && l.componentWillMount(), typeof l.UNSAFE_componentWillMount == "function" && l.UNSAFE_componentWillMount(), t !== l.state && Wl.enqueueReplaceState(l, l.state, null), Il(e, n, l, r), l.state = e.memoizedState), typeof l.componentDidMount == "function" && (e.flags |= 4194308);
}
function On(e, t) {
  try {
    var n = "", r = t;
    do
      n += zd(r), r = r.return;
    while (r);
    var l = n;
  } catch (i) {
    l = `
Error generating stack: ` + i.message + `
` + i.stack;
  }
  return { value: e, source: t, stack: l, digest: null };
}
function Ia(e, t, n) {
  return { value: e, source: null, stack: n ?? null, digest: t ?? null };
}
function ci(e, t) {
  try {
    console.error(t.value);
  } catch (n) {
    setTimeout(function() {
      throw n;
    });
  }
}
var ap = typeof WeakMap == "function" ? WeakMap : Map;
function wc(e, t, n) {
  n = Nt(-1, n), n.tag = 3, n.payload = { element: null };
  var r = t.value;
  return n.callback = function() {
    Tl || (Tl = !0, ji = r), ci(e, t);
  }, n;
}
function Sc(e, t, n) {
  n = Nt(-1, n), n.tag = 3;
  var r = e.type.getDerivedStateFromError;
  if (typeof r == "function") {
    var l = t.value;
    n.payload = function() {
      return r(l);
    }, n.callback = function() {
      ci(e, t);
    };
  }
  var i = e.stateNode;
  return i !== null && typeof i.componentDidCatch == "function" && (n.callback = function() {
    ci(e, t), typeof r != "function" && (Ut === null ? Ut = /* @__PURE__ */ new Set([this]) : Ut.add(this));
    var o = t.stack;
    this.componentDidCatch(t.value, { componentStack: o !== null ? o : "" });
  }), n;
}
function ys(e, t, n) {
  var r = e.pingCache;
  if (r === null) {
    r = e.pingCache = new ap();
    var l = /* @__PURE__ */ new Set();
    r.set(t, l);
  } else l = r.get(t), l === void 0 && (l = /* @__PURE__ */ new Set(), r.set(t, l));
  l.has(n) || (l.add(n), e = yp.bind(null, e, t, n), t.then(e, e));
}
function js(e) {
  do {
    var t;
    if ((t = e.tag === 13) && (t = e.memoizedState, t = t !== null ? t.dehydrated !== null : !0), t) return e;
    e = e.return;
  } while (e !== null);
  return null;
}
function Ns(e, t, n, r, l) {
  return e.mode & 1 ? (e.flags |= 65536, e.lanes = l, e) : (e === t ? e.flags |= 65536 : (e.flags |= 128, n.flags |= 131072, n.flags &= -52805, n.tag === 1 && (n.alternate === null ? n.tag = 17 : (t = Nt(-1, 1), t.tag = 2, At(n, t, 1))), n.lanes |= 1), e);
}
var ip = Et.ReactCurrentOwner, Me = !1;
function _e(e, t, n, r) {
  t.child = e === null ? Zu(t, null, n, r) : Ln(t, e.child, n, r);
}
function ws(e, t, n, r, l) {
  n = n.render;
  var i = t.ref;
  return Fn(t, l), r = ro(e, t, n, r, i, l), n = lo(), e !== null && !Me ? (t.updateQueue = e.updateQueue, t.flags &= -2053, e.lanes &= ~l, Ct(e, t, l)) : (te && n && Qi(t), t.flags |= 1, _e(e, t, r, l), t.child);
}
function Ss(e, t, n, r, l) {
  if (e === null) {
    var i = n.type;
    return typeof i == "function" && !ho(i) && i.defaultProps === void 0 && n.compare === null && n.defaultProps === void 0 ? (t.tag = 15, t.type = i, kc(e, t, i, r, l)) : (e = cl(n.type, null, r, t, t.mode, l), e.ref = t.ref, e.return = t, t.child = e);
  }
  if (i = e.child, !(e.lanes & l)) {
    var o = i.memoizedProps;
    if (n = n.compare, n = n !== null ? n : xr, n(o, r) && e.ref === t.ref) return Ct(e, t, l);
  }
  return t.flags |= 1, e = Bt(i, r), e.ref = t.ref, e.return = t, t.child = e;
}
function kc(e, t, n, r, l) {
  if (e !== null) {
    var i = e.memoizedProps;
    if (xr(i, r) && e.ref === t.ref) if (Me = !1, t.pendingProps = r = i, (e.lanes & l) !== 0) e.flags & 131072 && (Me = !0);
    else return t.lanes = e.lanes, Ct(e, t, l);
  }
  return di(e, t, n, r, l);
}
function Cc(e, t, n) {
  var r = t.pendingProps, l = r.children, i = e !== null ? e.memoizedState : null;
  if (r.mode === "hidden") if (!(t.mode & 1)) t.memoizedState = { baseLanes: 0, cachePool: null, transitions: null }, X(Cn, Ve), Ve |= n;
  else {
    if (!(n & 1073741824)) return e = i !== null ? i.baseLanes | n : n, t.lanes = t.childLanes = 1073741824, t.memoizedState = { baseLanes: e, cachePool: null, transitions: null }, t.updateQueue = null, X(Cn, Ve), Ve |= e, null;
    t.memoizedState = { baseLanes: 0, cachePool: null, transitions: null }, r = i !== null ? i.baseLanes : n, X(Cn, Ve), Ve |= r;
  }
  else i !== null ? (r = i.baseLanes | n, t.memoizedState = null) : r = n, X(Cn, Ve), Ve |= r;
  return _e(e, t, l, n), t.child;
}
function Ec(e, t) {
  var n = t.ref;
  (e === null && n !== null || e !== null && e.ref !== n) && (t.flags |= 512, t.flags |= 2097152);
}
function di(e, t, n, r, l) {
  var i = $e(n) ? rn : Pe.current;
  return i = Rn(t, i), Fn(t, l), n = ro(e, t, n, r, i, l), r = lo(), e !== null && !Me ? (t.updateQueue = e.updateQueue, t.flags &= -2053, e.lanes &= ~l, Ct(e, t, l)) : (te && r && Qi(t), t.flags |= 1, _e(e, t, n, l), t.child);
}
function ks(e, t, n, r, l) {
  if ($e(n)) {
    var i = !0;
    wl(t);
  } else i = !1;
  if (Fn(t, l), t.stateNode === null) ol(e, t), Nc(t, n, r), ui(t, n, r, l), r = !0;
  else if (e === null) {
    var o = t.stateNode, s = t.memoizedProps;
    o.props = s;
    var u = o.context, d = n.contextType;
    typeof d == "object" && d !== null ? d = Ze(d) : (d = $e(n) ? rn : Pe.current, d = Rn(t, d));
    var j = n.getDerivedStateFromProps, c = typeof j == "function" || typeof o.getSnapshotBeforeUpdate == "function";
    c || typeof o.UNSAFE_componentWillReceiveProps != "function" && typeof o.componentWillReceiveProps != "function" || (s !== r || u !== d) && xs(t, o, r, d), Ft = !1;
    var m = t.memoizedState;
    o.state = m, Il(t, r, o, l), u = t.memoizedState, s !== r || m !== u || Oe.current || Ft ? (typeof j == "function" && (si(t, n, j, r), u = t.memoizedState), (s = Ft || gs(t, n, s, r, m, u, d)) ? (c || typeof o.UNSAFE_componentWillMount != "function" && typeof o.componentWillMount != "function" || (typeof o.componentWillMount == "function" && o.componentWillMount(), typeof o.UNSAFE_componentWillMount == "function" && o.UNSAFE_componentWillMount()), typeof o.componentDidMount == "function" && (t.flags |= 4194308)) : (typeof o.componentDidMount == "function" && (t.flags |= 4194308), t.memoizedProps = r, t.memoizedState = u), o.props = r, o.state = u, o.context = d, r = s) : (typeof o.componentDidMount == "function" && (t.flags |= 4194308), r = !1);
  } else {
    o = t.stateNode, ec(e, t), s = t.memoizedProps, d = t.type === t.elementType ? s : nt(t.type, s), o.props = d, c = t.pendingProps, m = o.context, u = n.contextType, typeof u == "object" && u !== null ? u = Ze(u) : (u = $e(n) ? rn : Pe.current, u = Rn(t, u));
    var v = n.getDerivedStateFromProps;
    (j = typeof v == "function" || typeof o.getSnapshotBeforeUpdate == "function") || typeof o.UNSAFE_componentWillReceiveProps != "function" && typeof o.componentWillReceiveProps != "function" || (s !== c || m !== u) && xs(t, o, r, u), Ft = !1, m = t.memoizedState, o.state = m, Il(t, r, o, l);
    var g = t.memoizedState;
    s !== c || m !== g || Oe.current || Ft ? (typeof v == "function" && (si(t, n, v, r), g = t.memoizedState), (d = Ft || gs(t, n, d, r, m, g, u) || !1) ? (j || typeof o.UNSAFE_componentWillUpdate != "function" && typeof o.componentWillUpdate != "function" || (typeof o.componentWillUpdate == "function" && o.componentWillUpdate(r, g, u), typeof o.UNSAFE_componentWillUpdate == "function" && o.UNSAFE_componentWillUpdate(r, g, u)), typeof o.componentDidUpdate == "function" && (t.flags |= 4), typeof o.getSnapshotBeforeUpdate == "function" && (t.flags |= 1024)) : (typeof o.componentDidUpdate != "function" || s === e.memoizedProps && m === e.memoizedState || (t.flags |= 4), typeof o.getSnapshotBeforeUpdate != "function" || s === e.memoizedProps && m === e.memoizedState || (t.flags |= 1024), t.memoizedProps = r, t.memoizedState = g), o.props = r, o.state = g, o.context = u, r = d) : (typeof o.componentDidUpdate != "function" || s === e.memoizedProps && m === e.memoizedState || (t.flags |= 4), typeof o.getSnapshotBeforeUpdate != "function" || s === e.memoizedProps && m === e.memoizedState || (t.flags |= 1024), r = !1);
  }
  return fi(e, t, n, r, i, l);
}
function fi(e, t, n, r, l, i) {
  Ec(e, t);
  var o = (t.flags & 128) !== 0;
  if (!r && !o) return l && us(t, n, !1), Ct(e, t, i);
  r = t.stateNode, ip.current = t;
  var s = o && typeof n.getDerivedStateFromError != "function" ? null : r.render();
  return t.flags |= 1, e !== null && o ? (t.child = Ln(t, e.child, null, i), t.child = Ln(t, null, s, i)) : _e(e, t, s, i), t.memoizedState = r.state, l && us(t, n, !0), t.child;
}
function Ic(e) {
  var t = e.stateNode;
  t.pendingContext ? ss(e, t.pendingContext, t.pendingContext !== t.context) : t.context && ss(e, t.context, !1), Ji(e, t.containerInfo);
}
function Cs(e, t, n, r, l) {
  return Dn(), Ki(l), t.flags |= 256, _e(e, t, n, r), t.child;
}
var pi = { dehydrated: null, treeContext: null, retryLane: 0 };
function mi(e) {
  return { baseLanes: e, cachePool: null, transitions: null };
}
function Pc(e, t, n) {
  var r = t.pendingProps, l = ne.current, i = !1, o = (t.flags & 128) !== 0, s;
  if ((s = o) || (s = e !== null && e.memoizedState === null ? !1 : (l & 2) !== 0), s ? (i = !0, t.flags &= -129) : (e === null || e.memoizedState !== null) && (l |= 1), X(ne, l & 1), e === null)
    return ii(t), e = t.memoizedState, e !== null && (e = e.dehydrated, e !== null) ? (t.mode & 1 ? e.data === "$!" ? t.lanes = 8 : t.lanes = 1073741824 : t.lanes = 1, null) : (o = r.children, e = r.fallback, i ? (r = t.mode, i = t.child, o = { mode: "hidden", children: o }, !(r & 1) && i !== null ? (i.childLanes = 0, i.pendingProps = o) : i = Kl(o, r, 0, null), e = nn(e, r, n, null), i.return = t, e.return = t, i.sibling = e, t.child = i, t.child.memoizedState = mi(n), t.memoizedState = pi, e) : oo(t, o));
  if (l = e.memoizedState, l !== null && (s = l.dehydrated, s !== null)) return op(e, t, o, r, s, l, n);
  if (i) {
    i = r.fallback, o = t.mode, l = e.child, s = l.sibling;
    var u = { mode: "hidden", children: r.children };
    return !(o & 1) && t.child !== l ? (r = t.child, r.childLanes = 0, r.pendingProps = u, t.deletions = null) : (r = Bt(l, u), r.subtreeFlags = l.subtreeFlags & 14680064), s !== null ? i = Bt(s, i) : (i = nn(i, o, n, null), i.flags |= 2), i.return = t, r.return = t, r.sibling = i, t.child = r, r = i, i = t.child, o = e.child.memoizedState, o = o === null ? mi(n) : { baseLanes: o.baseLanes | n, cachePool: null, transitions: o.transitions }, i.memoizedState = o, i.childLanes = e.childLanes & ~n, t.memoizedState = pi, r;
  }
  return i = e.child, e = i.sibling, r = Bt(i, { mode: "visible", children: r.children }), !(t.mode & 1) && (r.lanes = n), r.return = t, r.sibling = null, e !== null && (n = t.deletions, n === null ? (t.deletions = [e], t.flags |= 16) : n.push(e)), t.child = r, t.memoizedState = null, r;
}
function oo(e, t) {
  return t = Kl({ mode: "visible", children: t }, e.mode, 0, null), t.return = e, e.child = t;
}
function Yr(e, t, n, r) {
  return r !== null && Ki(r), Ln(t, e.child, null, n), e = oo(t, t.pendingProps.children), e.flags |= 2, t.memoizedState = null, e;
}
function op(e, t, n, r, l, i, o) {
  if (n)
    return t.flags & 256 ? (t.flags &= -257, r = Ia(Error(P(422))), Yr(e, t, o, r)) : t.memoizedState !== null ? (t.child = e.child, t.flags |= 128, null) : (i = r.fallback, l = t.mode, r = Kl({ mode: "visible", children: r.children }, l, 0, null), i = nn(i, l, o, null), i.flags |= 2, r.return = t, i.return = t, r.sibling = i, t.child = r, t.mode & 1 && Ln(t, e.child, null, o), t.child.memoizedState = mi(o), t.memoizedState = pi, i);
  if (!(t.mode & 1)) return Yr(e, t, o, null);
  if (l.data === "$!") {
    if (r = l.nextSibling && l.nextSibling.dataset, r) var s = r.dgst;
    return r = s, i = Error(P(419)), r = Ia(i, r, void 0), Yr(e, t, o, r);
  }
  if (s = (o & e.childLanes) !== 0, Me || s) {
    if (r = xe, r !== null) {
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
      l = l & (r.suspendedLanes | o) ? 0 : l, l !== 0 && l !== i.retryLane && (i.retryLane = l, kt(e, l), ot(r, e, l, -1));
    }
    return mo(), r = Ia(Error(P(421))), Yr(e, t, o, r);
  }
  return l.data === "$?" ? (t.flags |= 128, t.child = e.child, t = jp.bind(null, e), l._reactRetry = t, null) : (e = i.treeContext, Be = $t(l.nextSibling), He = t, te = !0, lt = null, e !== null && (qe[Ye++] = yt, qe[Ye++] = jt, qe[Ye++] = ln, yt = e.id, jt = e.overflow, ln = t), t = oo(t, r.children), t.flags |= 4096, t);
}
function Es(e, t, n) {
  e.lanes |= t;
  var r = e.alternate;
  r !== null && (r.lanes |= t), oi(e.return, t, n);
}
function Pa(e, t, n, r, l) {
  var i = e.memoizedState;
  i === null ? e.memoizedState = { isBackwards: t, rendering: null, renderingStartTime: 0, last: r, tail: n, tailMode: l } : (i.isBackwards = t, i.rendering = null, i.renderingStartTime = 0, i.last = r, i.tail = n, i.tailMode = l);
}
function _c(e, t, n) {
  var r = t.pendingProps, l = r.revealOrder, i = r.tail;
  if (_e(e, t, r.children, n), r = ne.current, r & 2) r = r & 1 | 2, t.flags |= 128;
  else {
    if (e !== null && e.flags & 128) e: for (e = t.child; e !== null; ) {
      if (e.tag === 13) e.memoizedState !== null && Es(e, n, t);
      else if (e.tag === 19) Es(e, n, t);
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
  if (X(ne, r), !(t.mode & 1)) t.memoizedState = null;
  else switch (l) {
    case "forwards":
      for (n = t.child, l = null; n !== null; ) e = n.alternate, e !== null && Pl(e) === null && (l = n), n = n.sibling;
      n = l, n === null ? (l = t.child, t.child = null) : (l = n.sibling, n.sibling = null), Pa(t, !1, l, n, i);
      break;
    case "backwards":
      for (n = null, l = t.child, t.child = null; l !== null; ) {
        if (e = l.alternate, e !== null && Pl(e) === null) {
          t.child = l;
          break;
        }
        e = l.sibling, l.sibling = n, n = l, l = e;
      }
      Pa(t, !0, n, null, i);
      break;
    case "together":
      Pa(t, !1, null, null, void 0);
      break;
    default:
      t.memoizedState = null;
  }
  return t.child;
}
function ol(e, t) {
  !(t.mode & 1) && e !== null && (e.alternate = null, t.alternate = null, t.flags |= 2);
}
function Ct(e, t, n) {
  if (e !== null && (t.dependencies = e.dependencies), on |= t.lanes, !(n & t.childLanes)) return null;
  if (e !== null && t.child !== e.child) throw Error(P(153));
  if (t.child !== null) {
    for (e = t.child, n = Bt(e, e.pendingProps), t.child = n, n.return = t; e.sibling !== null; ) e = e.sibling, n = n.sibling = Bt(e, e.pendingProps), n.return = t;
    n.sibling = null;
  }
  return t.child;
}
function sp(e, t, n) {
  switch (t.tag) {
    case 3:
      Ic(t), Dn();
      break;
    case 5:
      tc(t);
      break;
    case 1:
      $e(t.type) && wl(t);
      break;
    case 4:
      Ji(t, t.stateNode.containerInfo);
      break;
    case 10:
      var r = t.type._context, l = t.memoizedProps.value;
      X(Cl, r._currentValue), r._currentValue = l;
      break;
    case 13:
      if (r = t.memoizedState, r !== null)
        return r.dehydrated !== null ? (X(ne, ne.current & 1), t.flags |= 128, null) : n & t.child.childLanes ? Pc(e, t, n) : (X(ne, ne.current & 1), e = Ct(e, t, n), e !== null ? e.sibling : null);
      X(ne, ne.current & 1);
      break;
    case 19:
      if (r = (n & t.childLanes) !== 0, e.flags & 128) {
        if (r) return _c(e, t, n);
        t.flags |= 128;
      }
      if (l = t.memoizedState, l !== null && (l.rendering = null, l.tail = null, l.lastEffect = null), X(ne, ne.current), r) break;
      return null;
    case 22:
    case 23:
      return t.lanes = 0, Cc(e, t, n);
  }
  return Ct(e, t, n);
}
var Fc, hi, zc, Tc;
Fc = function(e, t) {
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
hi = function() {
};
zc = function(e, t, n, r) {
  var l = e.memoizedProps;
  if (l !== r) {
    e = t.stateNode, en(pt.current);
    var i = null;
    switch (n) {
      case "input":
        l = Oa(e, l), r = Oa(e, r), i = [];
        break;
      case "select":
        l = le({}, l, { value: void 0 }), r = le({}, r, { value: void 0 }), i = [];
        break;
      case "textarea":
        l = Ua(e, l), r = Ua(e, r), i = [];
        break;
      default:
        typeof l.onClick != "function" && typeof r.onClick == "function" && (e.onclick = jl);
    }
    Ba(n, r);
    var o;
    n = null;
    for (d in l) if (!r.hasOwnProperty(d) && l.hasOwnProperty(d) && l[d] != null) if (d === "style") {
      var s = l[d];
      for (o in s) s.hasOwnProperty(o) && (n || (n = {}), n[o] = "");
    } else d !== "dangerouslySetInnerHTML" && d !== "children" && d !== "suppressContentEditableWarning" && d !== "suppressHydrationWarning" && d !== "autoFocus" && (dr.hasOwnProperty(d) ? i || (i = []) : (i = i || []).push(d, null));
    for (d in r) {
      var u = r[d];
      if (s = l != null ? l[d] : void 0, r.hasOwnProperty(d) && u !== s && (u != null || s != null)) if (d === "style") if (s) {
        for (o in s) !s.hasOwnProperty(o) || u && u.hasOwnProperty(o) || (n || (n = {}), n[o] = "");
        for (o in u) u.hasOwnProperty(o) && s[o] !== u[o] && (n || (n = {}), n[o] = u[o]);
      } else n || (i || (i = []), i.push(
        d,
        n
      )), n = u;
      else d === "dangerouslySetInnerHTML" ? (u = u ? u.__html : void 0, s = s ? s.__html : void 0, u != null && s !== u && (i = i || []).push(d, u)) : d === "children" ? typeof u != "string" && typeof u != "number" || (i = i || []).push(d, "" + u) : d !== "suppressContentEditableWarning" && d !== "suppressHydrationWarning" && (dr.hasOwnProperty(d) ? (u != null && d === "onScroll" && Z("scroll", e), i || s === u || (i = [])) : (i = i || []).push(d, u));
    }
    n && (i = i || []).push("style", n);
    var d = i;
    (t.updateQueue = d) && (t.flags |= 4);
  }
};
Tc = function(e, t, n, r) {
  n !== r && (t.flags |= 4);
};
function Yn(e, t) {
  if (!te) switch (e.tailMode) {
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
function Ce(e) {
  var t = e.alternate !== null && e.alternate.child === e.child, n = 0, r = 0;
  if (t) for (var l = e.child; l !== null; ) n |= l.lanes | l.childLanes, r |= l.subtreeFlags & 14680064, r |= l.flags & 14680064, l.return = e, l = l.sibling;
  else for (l = e.child; l !== null; ) n |= l.lanes | l.childLanes, r |= l.subtreeFlags, r |= l.flags, l.return = e, l = l.sibling;
  return e.subtreeFlags |= r, e.childLanes = n, t;
}
function up(e, t, n) {
  var r = t.pendingProps;
  switch (Gi(t), t.tag) {
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
      return Ce(t), null;
    case 1:
      return $e(t.type) && Nl(), Ce(t), null;
    case 3:
      return r = t.stateNode, Mn(), J(Oe), J(Pe), to(), r.pendingContext && (r.context = r.pendingContext, r.pendingContext = null), (e === null || e.child === null) && (Kr(t) ? t.flags |= 4 : e === null || e.memoizedState.isDehydrated && !(t.flags & 256) || (t.flags |= 1024, lt !== null && (Si(lt), lt = null))), hi(e, t), Ce(t), null;
    case 5:
      eo(t);
      var l = en(Sr.current);
      if (n = t.type, e !== null && t.stateNode != null) zc(e, t, n, r, l), e.ref !== t.ref && (t.flags |= 512, t.flags |= 2097152);
      else {
        if (!r) {
          if (t.stateNode === null) throw Error(P(166));
          return Ce(t), null;
        }
        if (e = en(pt.current), Kr(t)) {
          r = t.stateNode, n = t.type;
          var i = t.memoizedProps;
          switch (r[dt] = t, r[Nr] = i, e = (t.mode & 1) !== 0, n) {
            case "dialog":
              Z("cancel", r), Z("close", r);
              break;
            case "iframe":
            case "object":
            case "embed":
              Z("load", r);
              break;
            case "video":
            case "audio":
              for (l = 0; l < er.length; l++) Z(er[l], r);
              break;
            case "source":
              Z("error", r);
              break;
            case "img":
            case "image":
            case "link":
              Z(
                "error",
                r
              ), Z("load", r);
              break;
            case "details":
              Z("toggle", r);
              break;
            case "input":
              Lo(r, i), Z("invalid", r);
              break;
            case "select":
              r._wrapperState = { wasMultiple: !!i.multiple }, Z("invalid", r);
              break;
            case "textarea":
              Oo(r, i), Z("invalid", r);
          }
          Ba(n, i), l = null;
          for (var o in i) if (i.hasOwnProperty(o)) {
            var s = i[o];
            o === "children" ? typeof s == "string" ? r.textContent !== s && (i.suppressHydrationWarning !== !0 && Gr(r.textContent, s, e), l = ["children", s]) : typeof s == "number" && r.textContent !== "" + s && (i.suppressHydrationWarning !== !0 && Gr(
              r.textContent,
              s,
              e
            ), l = ["children", "" + s]) : dr.hasOwnProperty(o) && s != null && o === "onScroll" && Z("scroll", r);
          }
          switch (n) {
            case "input":
              $r(r), Mo(r, i, !0);
              break;
            case "textarea":
              $r(r), $o(r);
              break;
            case "select":
            case "option":
              break;
            default:
              typeof i.onClick == "function" && (r.onclick = jl);
          }
          r = l, t.updateQueue = r, r !== null && (t.flags |= 4);
        } else {
          o = l.nodeType === 9 ? l : l.ownerDocument, e === "http://www.w3.org/1999/xhtml" && (e = iu(n)), e === "http://www.w3.org/1999/xhtml" ? n === "script" ? (e = o.createElement("div"), e.innerHTML = "<script><\/script>", e = e.removeChild(e.firstChild)) : typeof r.is == "string" ? e = o.createElement(n, { is: r.is }) : (e = o.createElement(n), n === "select" && (o = e, r.multiple ? o.multiple = !0 : r.size && (o.size = r.size))) : e = o.createElementNS(e, n), e[dt] = t, e[Nr] = r, Fc(e, t, !1, !1), t.stateNode = e;
          e: {
            switch (o = Ha(n, r), n) {
              case "dialog":
                Z("cancel", e), Z("close", e), l = r;
                break;
              case "iframe":
              case "object":
              case "embed":
                Z("load", e), l = r;
                break;
              case "video":
              case "audio":
                for (l = 0; l < er.length; l++) Z(er[l], e);
                l = r;
                break;
              case "source":
                Z("error", e), l = r;
                break;
              case "img":
              case "image":
              case "link":
                Z(
                  "error",
                  e
                ), Z("load", e), l = r;
                break;
              case "details":
                Z("toggle", e), l = r;
                break;
              case "input":
                Lo(e, r), l = Oa(e, r), Z("invalid", e);
                break;
              case "option":
                l = r;
                break;
              case "select":
                e._wrapperState = { wasMultiple: !!r.multiple }, l = le({}, r, { value: void 0 }), Z("invalid", e);
                break;
              case "textarea":
                Oo(e, r), l = Ua(e, r), Z("invalid", e);
                break;
              default:
                l = r;
            }
            Ba(n, l), s = l;
            for (i in s) if (s.hasOwnProperty(i)) {
              var u = s[i];
              i === "style" ? uu(e, u) : i === "dangerouslySetInnerHTML" ? (u = u ? u.__html : void 0, u != null && ou(e, u)) : i === "children" ? typeof u == "string" ? (n !== "textarea" || u !== "") && fr(e, u) : typeof u == "number" && fr(e, "" + u) : i !== "suppressContentEditableWarning" && i !== "suppressHydrationWarning" && i !== "autoFocus" && (dr.hasOwnProperty(i) ? u != null && i === "onScroll" && Z("scroll", e) : u != null && zi(e, i, u, o));
            }
            switch (n) {
              case "input":
                $r(e), Mo(e, r, !1);
                break;
              case "textarea":
                $r(e), $o(e);
                break;
              case "option":
                r.value != null && e.setAttribute("value", "" + Ht(r.value));
                break;
              case "select":
                e.multiple = !!r.multiple, i = r.value, i != null ? En(e, !!r.multiple, i, !1) : r.defaultValue != null && En(
                  e,
                  !!r.multiple,
                  r.defaultValue,
                  !0
                );
                break;
              default:
                typeof l.onClick == "function" && (e.onclick = jl);
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
      return Ce(t), null;
    case 6:
      if (e && t.stateNode != null) Tc(e, t, e.memoizedProps, r);
      else {
        if (typeof r != "string" && t.stateNode === null) throw Error(P(166));
        if (n = en(Sr.current), en(pt.current), Kr(t)) {
          if (r = t.stateNode, n = t.memoizedProps, r[dt] = t, (i = r.nodeValue !== n) && (e = He, e !== null)) switch (e.tag) {
            case 3:
              Gr(r.nodeValue, n, (e.mode & 1) !== 0);
              break;
            case 5:
              e.memoizedProps.suppressHydrationWarning !== !0 && Gr(r.nodeValue, n, (e.mode & 1) !== 0);
          }
          i && (t.flags |= 4);
        } else r = (n.nodeType === 9 ? n : n.ownerDocument).createTextNode(r), r[dt] = t, t.stateNode = r;
      }
      return Ce(t), null;
    case 13:
      if (J(ne), r = t.memoizedState, e === null || e.memoizedState !== null && e.memoizedState.dehydrated !== null) {
        if (te && Be !== null && t.mode & 1 && !(t.flags & 128)) Xu(), Dn(), t.flags |= 98560, i = !1;
        else if (i = Kr(t), r !== null && r.dehydrated !== null) {
          if (e === null) {
            if (!i) throw Error(P(318));
            if (i = t.memoizedState, i = i !== null ? i.dehydrated : null, !i) throw Error(P(317));
            i[dt] = t;
          } else Dn(), !(t.flags & 128) && (t.memoizedState = null), t.flags |= 4;
          Ce(t), i = !1;
        } else lt !== null && (Si(lt), lt = null), i = !0;
        if (!i) return t.flags & 65536 ? t : null;
      }
      return t.flags & 128 ? (t.lanes = n, t) : (r = r !== null, r !== (e !== null && e.memoizedState !== null) && r && (t.child.flags |= 8192, t.mode & 1 && (e === null || ne.current & 1 ? me === 0 && (me = 3) : mo())), t.updateQueue !== null && (t.flags |= 4), Ce(t), null);
    case 4:
      return Mn(), hi(e, t), e === null && yr(t.stateNode.containerInfo), Ce(t), null;
    case 10:
      return Xi(t.type._context), Ce(t), null;
    case 17:
      return $e(t.type) && Nl(), Ce(t), null;
    case 19:
      if (J(ne), i = t.memoizedState, i === null) return Ce(t), null;
      if (r = (t.flags & 128) !== 0, o = i.rendering, o === null) if (r) Yn(i, !1);
      else {
        if (me !== 0 || e !== null && e.flags & 128) for (e = t.child; e !== null; ) {
          if (o = Pl(e), o !== null) {
            for (t.flags |= 128, Yn(i, !1), r = o.updateQueue, r !== null && (t.updateQueue = r, t.flags |= 4), t.subtreeFlags = 0, r = n, n = t.child; n !== null; ) i = n, e = r, i.flags &= 14680066, o = i.alternate, o === null ? (i.childLanes = 0, i.lanes = e, i.child = null, i.subtreeFlags = 0, i.memoizedProps = null, i.memoizedState = null, i.updateQueue = null, i.dependencies = null, i.stateNode = null) : (i.childLanes = o.childLanes, i.lanes = o.lanes, i.child = o.child, i.subtreeFlags = 0, i.deletions = null, i.memoizedProps = o.memoizedProps, i.memoizedState = o.memoizedState, i.updateQueue = o.updateQueue, i.type = o.type, e = o.dependencies, i.dependencies = e === null ? null : { lanes: e.lanes, firstContext: e.firstContext }), n = n.sibling;
            return X(ne, ne.current & 1 | 2), t.child;
          }
          e = e.sibling;
        }
        i.tail !== null && ue() > $n && (t.flags |= 128, r = !0, Yn(i, !1), t.lanes = 4194304);
      }
      else {
        if (!r) if (e = Pl(o), e !== null) {
          if (t.flags |= 128, r = !0, n = e.updateQueue, n !== null && (t.updateQueue = n, t.flags |= 4), Yn(i, !0), i.tail === null && i.tailMode === "hidden" && !o.alternate && !te) return Ce(t), null;
        } else 2 * ue() - i.renderingStartTime > $n && n !== 1073741824 && (t.flags |= 128, r = !0, Yn(i, !1), t.lanes = 4194304);
        i.isBackwards ? (o.sibling = t.child, t.child = o) : (n = i.last, n !== null ? n.sibling = o : t.child = o, i.last = o);
      }
      return i.tail !== null ? (t = i.tail, i.rendering = t, i.tail = t.sibling, i.renderingStartTime = ue(), t.sibling = null, n = ne.current, X(ne, r ? n & 1 | 2 : n & 1), t) : (Ce(t), null);
    case 22:
    case 23:
      return po(), r = t.memoizedState !== null, e !== null && e.memoizedState !== null !== r && (t.flags |= 8192), r && t.mode & 1 ? Ve & 1073741824 && (Ce(t), t.subtreeFlags & 6 && (t.flags |= 8192)) : Ce(t), null;
    case 24:
      return null;
    case 25:
      return null;
  }
  throw Error(P(156, t.tag));
}
function cp(e, t) {
  switch (Gi(t), t.tag) {
    case 1:
      return $e(t.type) && Nl(), e = t.flags, e & 65536 ? (t.flags = e & -65537 | 128, t) : null;
    case 3:
      return Mn(), J(Oe), J(Pe), to(), e = t.flags, e & 65536 && !(e & 128) ? (t.flags = e & -65537 | 128, t) : null;
    case 5:
      return eo(t), null;
    case 13:
      if (J(ne), e = t.memoizedState, e !== null && e.dehydrated !== null) {
        if (t.alternate === null) throw Error(P(340));
        Dn();
      }
      return e = t.flags, e & 65536 ? (t.flags = e & -65537 | 128, t) : null;
    case 19:
      return J(ne), null;
    case 4:
      return Mn(), null;
    case 10:
      return Xi(t.type._context), null;
    case 22:
    case 23:
      return po(), null;
    case 24:
      return null;
    default:
      return null;
  }
}
var Xr = !1, Ee = !1, dp = typeof WeakSet == "function" ? WeakSet : Set, O = null;
function kn(e, t) {
  var n = e.ref;
  if (n !== null) if (typeof n == "function") try {
    n(null);
  } catch (r) {
    se(e, t, r);
  }
  else n.current = null;
}
function vi(e, t, n) {
  try {
    n();
  } catch (r) {
    se(e, t, r);
  }
}
var Is = !1;
function fp(e, t) {
  if (Ja = gl, e = Ou(), Wi(e)) {
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
        var o = 0, s = -1, u = -1, d = 0, j = 0, c = e, m = null;
        t: for (; ; ) {
          for (var v; c !== n || l !== 0 && c.nodeType !== 3 || (s = o + l), c !== i || r !== 0 && c.nodeType !== 3 || (u = o + r), c.nodeType === 3 && (o += c.nodeValue.length), (v = c.firstChild) !== null; )
            m = c, c = v;
          for (; ; ) {
            if (c === e) break t;
            if (m === n && ++d === l && (s = o), m === i && ++j === r && (u = o), (v = c.nextSibling) !== null) break;
            c = m, m = c.parentNode;
          }
          c = v;
        }
        n = s === -1 || u === -1 ? null : { start: s, end: u };
      } else n = null;
    }
    n = n || { start: 0, end: 0 };
  } else n = null;
  for (ei = { focusedElem: e, selectionRange: n }, gl = !1, O = t; O !== null; ) if (t = O, e = t.child, (t.subtreeFlags & 1028) !== 0 && e !== null) e.return = t, O = e;
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
            var w = g.memoizedProps, z = g.memoizedState, p = t.stateNode, f = p.getSnapshotBeforeUpdate(t.elementType === t.type ? w : nt(t.type, w), z);
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
    } catch (C) {
      se(t, t.return, C);
    }
    if (e = t.sibling, e !== null) {
      e.return = t.return, O = e;
      break;
    }
    O = t.return;
  }
  return g = Is, Is = !1, g;
}
function or(e, t, n) {
  var r = t.updateQueue;
  if (r = r !== null ? r.lastEffect : null, r !== null) {
    var l = r = r.next;
    do {
      if ((l.tag & e) === e) {
        var i = l.destroy;
        l.destroy = void 0, i !== void 0 && vi(t, n, i);
      }
      l = l.next;
    } while (l !== r);
  }
}
function Ql(e, t) {
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
function gi(e) {
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
function Rc(e) {
  var t = e.alternate;
  t !== null && (e.alternate = null, Rc(t)), e.child = null, e.deletions = null, e.sibling = null, e.tag === 5 && (t = e.stateNode, t !== null && (delete t[dt], delete t[Nr], delete t[ri], delete t[qf], delete t[Yf])), e.stateNode = null, e.return = null, e.dependencies = null, e.memoizedProps = null, e.memoizedState = null, e.pendingProps = null, e.stateNode = null, e.updateQueue = null;
}
function Dc(e) {
  return e.tag === 5 || e.tag === 3 || e.tag === 4;
}
function Ps(e) {
  e: for (; ; ) {
    for (; e.sibling === null; ) {
      if (e.return === null || Dc(e.return)) return null;
      e = e.return;
    }
    for (e.sibling.return = e.return, e = e.sibling; e.tag !== 5 && e.tag !== 6 && e.tag !== 18; ) {
      if (e.flags & 2 || e.child === null || e.tag === 4) continue e;
      e.child.return = e, e = e.child;
    }
    if (!(e.flags & 2)) return e.stateNode;
  }
}
function xi(e, t, n) {
  var r = e.tag;
  if (r === 5 || r === 6) e = e.stateNode, t ? n.nodeType === 8 ? n.parentNode.insertBefore(e, t) : n.insertBefore(e, t) : (n.nodeType === 8 ? (t = n.parentNode, t.insertBefore(e, n)) : (t = n, t.appendChild(e)), n = n._reactRootContainer, n != null || t.onclick !== null || (t.onclick = jl));
  else if (r !== 4 && (e = e.child, e !== null)) for (xi(e, t, n), e = e.sibling; e !== null; ) xi(e, t, n), e = e.sibling;
}
function yi(e, t, n) {
  var r = e.tag;
  if (r === 5 || r === 6) e = e.stateNode, t ? n.insertBefore(e, t) : n.appendChild(e);
  else if (r !== 4 && (e = e.child, e !== null)) for (yi(e, t, n), e = e.sibling; e !== null; ) yi(e, t, n), e = e.sibling;
}
var je = null, rt = !1;
function Pt(e, t, n) {
  for (n = n.child; n !== null; ) Lc(e, t, n), n = n.sibling;
}
function Lc(e, t, n) {
  if (ft && typeof ft.onCommitFiberUnmount == "function") try {
    ft.onCommitFiberUnmount(Ol, n);
  } catch {
  }
  switch (n.tag) {
    case 5:
      Ee || kn(n, t);
    case 6:
      var r = je, l = rt;
      je = null, Pt(e, t, n), je = r, rt = l, je !== null && (rt ? (e = je, n = n.stateNode, e.nodeType === 8 ? e.parentNode.removeChild(n) : e.removeChild(n)) : je.removeChild(n.stateNode));
      break;
    case 18:
      je !== null && (rt ? (e = je, n = n.stateNode, e.nodeType === 8 ? Na(e.parentNode, n) : e.nodeType === 1 && Na(e, n), vr(e)) : Na(je, n.stateNode));
      break;
    case 4:
      r = je, l = rt, je = n.stateNode.containerInfo, rt = !0, Pt(e, t, n), je = r, rt = l;
      break;
    case 0:
    case 11:
    case 14:
    case 15:
      if (!Ee && (r = n.updateQueue, r !== null && (r = r.lastEffect, r !== null))) {
        l = r = r.next;
        do {
          var i = l, o = i.destroy;
          i = i.tag, o !== void 0 && (i & 2 || i & 4) && vi(n, t, o), l = l.next;
        } while (l !== r);
      }
      Pt(e, t, n);
      break;
    case 1:
      if (!Ee && (kn(n, t), r = n.stateNode, typeof r.componentWillUnmount == "function")) try {
        r.props = n.memoizedProps, r.state = n.memoizedState, r.componentWillUnmount();
      } catch (s) {
        se(n, t, s);
      }
      Pt(e, t, n);
      break;
    case 21:
      Pt(e, t, n);
      break;
    case 22:
      n.mode & 1 ? (Ee = (r = Ee) || n.memoizedState !== null, Pt(e, t, n), Ee = r) : Pt(e, t, n);
      break;
    default:
      Pt(e, t, n);
  }
}
function _s(e) {
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
function tt(e, t) {
  var n = t.deletions;
  if (n !== null) for (var r = 0; r < n.length; r++) {
    var l = n[r];
    try {
      var i = e, o = t, s = o;
      e: for (; s !== null; ) {
        switch (s.tag) {
          case 5:
            je = s.stateNode, rt = !1;
            break e;
          case 3:
            je = s.stateNode.containerInfo, rt = !0;
            break e;
          case 4:
            je = s.stateNode.containerInfo, rt = !0;
            break e;
        }
        s = s.return;
      }
      if (je === null) throw Error(P(160));
      Lc(i, o, l), je = null, rt = !1;
      var u = l.alternate;
      u !== null && (u.return = null), l.return = null;
    } catch (d) {
      se(l, t, d);
    }
  }
  if (t.subtreeFlags & 12854) for (t = t.child; t !== null; ) Mc(t, e), t = t.sibling;
}
function Mc(e, t) {
  var n = e.alternate, r = e.flags;
  switch (e.tag) {
    case 0:
    case 11:
    case 14:
    case 15:
      if (tt(t, e), ut(e), r & 4) {
        try {
          or(3, e, e.return), Ql(3, e);
        } catch (w) {
          se(e, e.return, w);
        }
        try {
          or(5, e, e.return);
        } catch (w) {
          se(e, e.return, w);
        }
      }
      break;
    case 1:
      tt(t, e), ut(e), r & 512 && n !== null && kn(n, n.return);
      break;
    case 5:
      if (tt(t, e), ut(e), r & 512 && n !== null && kn(n, n.return), e.flags & 32) {
        var l = e.stateNode;
        try {
          fr(l, "");
        } catch (w) {
          se(e, e.return, w);
        }
      }
      if (r & 4 && (l = e.stateNode, l != null)) {
        var i = e.memoizedProps, o = n !== null ? n.memoizedProps : i, s = e.type, u = e.updateQueue;
        if (e.updateQueue = null, u !== null) try {
          s === "input" && i.type === "radio" && i.name != null && lu(l, i), Ha(s, o);
          var d = Ha(s, i);
          for (o = 0; o < u.length; o += 2) {
            var j = u[o], c = u[o + 1];
            j === "style" ? uu(l, c) : j === "dangerouslySetInnerHTML" ? ou(l, c) : j === "children" ? fr(l, c) : zi(l, j, c, d);
          }
          switch (s) {
            case "input":
              $a(l, i);
              break;
            case "textarea":
              au(l, i);
              break;
            case "select":
              var m = l._wrapperState.wasMultiple;
              l._wrapperState.wasMultiple = !!i.multiple;
              var v = i.value;
              v != null ? En(l, !!i.multiple, v, !1) : m !== !!i.multiple && (i.defaultValue != null ? En(
                l,
                !!i.multiple,
                i.defaultValue,
                !0
              ) : En(l, !!i.multiple, i.multiple ? [] : "", !1));
          }
          l[Nr] = i;
        } catch (w) {
          se(e, e.return, w);
        }
      }
      break;
    case 6:
      if (tt(t, e), ut(e), r & 4) {
        if (e.stateNode === null) throw Error(P(162));
        l = e.stateNode, i = e.memoizedProps;
        try {
          l.nodeValue = i;
        } catch (w) {
          se(e, e.return, w);
        }
      }
      break;
    case 3:
      if (tt(t, e), ut(e), r & 4 && n !== null && n.memoizedState.isDehydrated) try {
        vr(t.containerInfo);
      } catch (w) {
        se(e, e.return, w);
      }
      break;
    case 4:
      tt(t, e), ut(e);
      break;
    case 13:
      tt(t, e), ut(e), l = e.child, l.flags & 8192 && (i = l.memoizedState !== null, l.stateNode.isHidden = i, !i || l.alternate !== null && l.alternate.memoizedState !== null || (co = ue())), r & 4 && _s(e);
      break;
    case 22:
      if (j = n !== null && n.memoizedState !== null, e.mode & 1 ? (Ee = (d = Ee) || j, tt(t, e), Ee = d) : tt(t, e), ut(e), r & 8192) {
        if (d = e.memoizedState !== null, (e.stateNode.isHidden = d) && !j && e.mode & 1) for (O = e, j = e.child; j !== null; ) {
          for (c = O = j; O !== null; ) {
            switch (m = O, v = m.child, m.tag) {
              case 0:
              case 11:
              case 14:
              case 15:
                or(4, m, m.return);
                break;
              case 1:
                kn(m, m.return);
                var g = m.stateNode;
                if (typeof g.componentWillUnmount == "function") {
                  r = m, n = m.return;
                  try {
                    t = r, g.props = t.memoizedProps, g.state = t.memoizedState, g.componentWillUnmount();
                  } catch (w) {
                    se(r, n, w);
                  }
                }
                break;
              case 5:
                kn(m, m.return);
                break;
              case 22:
                if (m.memoizedState !== null) {
                  zs(c);
                  continue;
                }
            }
            v !== null ? (v.return = m, O = v) : zs(c);
          }
          j = j.sibling;
        }
        e: for (j = null, c = e; ; ) {
          if (c.tag === 5) {
            if (j === null) {
              j = c;
              try {
                l = c.stateNode, d ? (i = l.style, typeof i.setProperty == "function" ? i.setProperty("display", "none", "important") : i.display = "none") : (s = c.stateNode, u = c.memoizedProps.style, o = u != null && u.hasOwnProperty("display") ? u.display : null, s.style.display = su("display", o));
              } catch (w) {
                se(e, e.return, w);
              }
            }
          } else if (c.tag === 6) {
            if (j === null) try {
              c.stateNode.nodeValue = d ? "" : c.memoizedProps;
            } catch (w) {
              se(e, e.return, w);
            }
          } else if ((c.tag !== 22 && c.tag !== 23 || c.memoizedState === null || c === e) && c.child !== null) {
            c.child.return = c, c = c.child;
            continue;
          }
          if (c === e) break e;
          for (; c.sibling === null; ) {
            if (c.return === null || c.return === e) break e;
            j === c && (j = null), c = c.return;
          }
          j === c && (j = null), c.sibling.return = c.return, c = c.sibling;
        }
      }
      break;
    case 19:
      tt(t, e), ut(e), r & 4 && _s(e);
      break;
    case 21:
      break;
    default:
      tt(
        t,
        e
      ), ut(e);
  }
}
function ut(e) {
  var t = e.flags;
  if (t & 2) {
    try {
      e: {
        for (var n = e.return; n !== null; ) {
          if (Dc(n)) {
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
          r.flags & 32 && (fr(l, ""), r.flags &= -33);
          var i = Ps(e);
          yi(e, i, l);
          break;
        case 3:
        case 4:
          var o = r.stateNode.containerInfo, s = Ps(e);
          xi(e, s, o);
          break;
        default:
          throw Error(P(161));
      }
    } catch (u) {
      se(e, e.return, u);
    }
    e.flags &= -3;
  }
  t & 4096 && (e.flags &= -4097);
}
function pp(e, t, n) {
  O = e, Oc(e);
}
function Oc(e, t, n) {
  for (var r = (e.mode & 1) !== 0; O !== null; ) {
    var l = O, i = l.child;
    if (l.tag === 22 && r) {
      var o = l.memoizedState !== null || Xr;
      if (!o) {
        var s = l.alternate, u = s !== null && s.memoizedState !== null || Ee;
        s = Xr;
        var d = Ee;
        if (Xr = o, (Ee = u) && !d) for (O = l; O !== null; ) o = O, u = o.child, o.tag === 22 && o.memoizedState !== null ? Ts(l) : u !== null ? (u.return = o, O = u) : Ts(l);
        for (; i !== null; ) O = i, Oc(i), i = i.sibling;
        O = l, Xr = s, Ee = d;
      }
      Fs(e);
    } else l.subtreeFlags & 8772 && i !== null ? (i.return = l, O = i) : Fs(e);
  }
}
function Fs(e) {
  for (; O !== null; ) {
    var t = O;
    if (t.flags & 8772) {
      var n = t.alternate;
      try {
        if (t.flags & 8772) switch (t.tag) {
          case 0:
          case 11:
          case 15:
            Ee || Ql(5, t);
            break;
          case 1:
            var r = t.stateNode;
            if (t.flags & 4 && !Ee) if (n === null) r.componentDidMount();
            else {
              var l = t.elementType === t.type ? n.memoizedProps : nt(t.type, n.memoizedProps);
              r.componentDidUpdate(l, n.memoizedState, r.__reactInternalSnapshotBeforeUpdate);
            }
            var i = t.updateQueue;
            i !== null && ms(t, i, r);
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
              ms(t, o, n);
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
                var j = d.memoizedState;
                if (j !== null) {
                  var c = j.dehydrated;
                  c !== null && vr(c);
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
        Ee || t.flags & 512 && gi(t);
      } catch (m) {
        se(t, t.return, m);
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
function zs(e) {
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
function Ts(e) {
  for (; O !== null; ) {
    var t = O;
    try {
      switch (t.tag) {
        case 0:
        case 11:
        case 15:
          var n = t.return;
          try {
            Ql(4, t);
          } catch (u) {
            se(t, n, u);
          }
          break;
        case 1:
          var r = t.stateNode;
          if (typeof r.componentDidMount == "function") {
            var l = t.return;
            try {
              r.componentDidMount();
            } catch (u) {
              se(t, l, u);
            }
          }
          var i = t.return;
          try {
            gi(t);
          } catch (u) {
            se(t, i, u);
          }
          break;
        case 5:
          var o = t.return;
          try {
            gi(t);
          } catch (u) {
            se(t, o, u);
          }
      }
    } catch (u) {
      se(t, t.return, u);
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
var mp = Math.ceil, zl = Et.ReactCurrentDispatcher, so = Et.ReactCurrentOwner, be = Et.ReactCurrentBatchConfig, W = 0, xe = null, de = null, Ne = 0, Ve = 0, Cn = Gt(0), me = 0, Ir = null, on = 0, Gl = 0, uo = 0, sr = null, Le = null, co = 0, $n = 1 / 0, gt = null, Tl = !1, ji = null, Ut = null, br = !1, Dt = null, Rl = 0, ur = 0, Ni = null, sl = -1, ul = 0;
function ze() {
  return W & 6 ? ue() : sl !== -1 ? sl : sl = ue();
}
function Vt(e) {
  return e.mode & 1 ? W & 2 && Ne !== 0 ? Ne & -Ne : bf.transition !== null ? (ul === 0 && (ul = Nu()), ul) : (e = G, e !== 0 || (e = window.event, e = e === void 0 ? 16 : Pu(e.type)), e) : 1;
}
function ot(e, t, n, r) {
  if (50 < ur) throw ur = 0, Ni = null, Error(P(185));
  Fr(e, n, r), (!(W & 2) || e !== xe) && (e === xe && (!(W & 2) && (Gl |= n), me === 4 && Tt(e, Ne)), Ae(e, r), n === 1 && W === 0 && !(t.mode & 1) && ($n = ue() + 500, Bl && Kt()));
}
function Ae(e, t) {
  var n = e.callbackNode;
  Xd(e, t);
  var r = vl(e, e === xe ? Ne : 0);
  if (r === 0) n !== null && Vo(n), e.callbackNode = null, e.callbackPriority = 0;
  else if (t = r & -r, e.callbackPriority !== t) {
    if (n != null && Vo(n), t === 1) e.tag === 0 ? Xf(Rs.bind(null, e)) : Ku(Rs.bind(null, e)), Gf(function() {
      !(W & 6) && Kt();
    }), n = null;
    else {
      switch (wu(r)) {
        case 1:
          n = Mi;
          break;
        case 4:
          n = yu;
          break;
        case 16:
          n = hl;
          break;
        case 536870912:
          n = ju;
          break;
        default:
          n = hl;
      }
      n = Qc(n, $c.bind(null, e));
    }
    e.callbackPriority = t, e.callbackNode = n;
  }
}
function $c(e, t) {
  if (sl = -1, ul = 0, W & 6) throw Error(P(327));
  var n = e.callbackNode;
  if (zn() && e.callbackNode !== n) return null;
  var r = vl(e, e === xe ? Ne : 0);
  if (r === 0) return null;
  if (r & 30 || r & e.expiredLanes || t) t = Dl(e, r);
  else {
    t = r;
    var l = W;
    W |= 2;
    var i = Uc();
    (xe !== e || Ne !== t) && (gt = null, $n = ue() + 500, tn(e, t));
    do
      try {
        gp();
        break;
      } catch (s) {
        Ac(e, s);
      }
    while (!0);
    Yi(), zl.current = i, W = l, de !== null ? t = 0 : (xe = null, Ne = 0, t = me);
  }
  if (t !== 0) {
    if (t === 2 && (l = qa(e), l !== 0 && (r = l, t = wi(e, l))), t === 1) throw n = Ir, tn(e, 0), Tt(e, r), Ae(e, ue()), n;
    if (t === 6) Tt(e, r);
    else {
      if (l = e.current.alternate, !(r & 30) && !hp(l) && (t = Dl(e, r), t === 2 && (i = qa(e), i !== 0 && (r = i, t = wi(e, i))), t === 1)) throw n = Ir, tn(e, 0), Tt(e, r), Ae(e, ue()), n;
      switch (e.finishedWork = l, e.finishedLanes = r, t) {
        case 0:
        case 1:
          throw Error(P(345));
        case 2:
          bt(e, Le, gt);
          break;
        case 3:
          if (Tt(e, r), (r & 130023424) === r && (t = co + 500 - ue(), 10 < t)) {
            if (vl(e, 0) !== 0) break;
            if (l = e.suspendedLanes, (l & r) !== r) {
              ze(), e.pingedLanes |= e.suspendedLanes & l;
              break;
            }
            e.timeoutHandle = ni(bt.bind(null, e, Le, gt), t);
            break;
          }
          bt(e, Le, gt);
          break;
        case 4:
          if (Tt(e, r), (r & 4194240) === r) break;
          for (t = e.eventTimes, l = -1; 0 < r; ) {
            var o = 31 - it(r);
            i = 1 << o, o = t[o], o > l && (l = o), r &= ~i;
          }
          if (r = l, r = ue() - r, r = (120 > r ? 120 : 480 > r ? 480 : 1080 > r ? 1080 : 1920 > r ? 1920 : 3e3 > r ? 3e3 : 4320 > r ? 4320 : 1960 * mp(r / 1960)) - r, 10 < r) {
            e.timeoutHandle = ni(bt.bind(null, e, Le, gt), r);
            break;
          }
          bt(e, Le, gt);
          break;
        case 5:
          bt(e, Le, gt);
          break;
        default:
          throw Error(P(329));
      }
    }
  }
  return Ae(e, ue()), e.callbackNode === n ? $c.bind(null, e) : null;
}
function wi(e, t) {
  var n = sr;
  return e.current.memoizedState.isDehydrated && (tn(e, t).flags |= 256), e = Dl(e, t), e !== 2 && (t = Le, Le = n, t !== null && Si(t)), e;
}
function Si(e) {
  Le === null ? Le = e : Le.push.apply(Le, e);
}
function hp(e) {
  for (var t = e; ; ) {
    if (t.flags & 16384) {
      var n = t.updateQueue;
      if (n !== null && (n = n.stores, n !== null)) for (var r = 0; r < n.length; r++) {
        var l = n[r], i = l.getSnapshot;
        l = l.value;
        try {
          if (!st(i(), l)) return !1;
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
function Tt(e, t) {
  for (t &= ~uo, t &= ~Gl, e.suspendedLanes |= t, e.pingedLanes &= ~t, e = e.expirationTimes; 0 < t; ) {
    var n = 31 - it(t), r = 1 << n;
    e[n] = -1, t &= ~r;
  }
}
function Rs(e) {
  if (W & 6) throw Error(P(327));
  zn();
  var t = vl(e, 0);
  if (!(t & 1)) return Ae(e, ue()), null;
  var n = Dl(e, t);
  if (e.tag !== 0 && n === 2) {
    var r = qa(e);
    r !== 0 && (t = r, n = wi(e, r));
  }
  if (n === 1) throw n = Ir, tn(e, 0), Tt(e, t), Ae(e, ue()), n;
  if (n === 6) throw Error(P(345));
  return e.finishedWork = e.current.alternate, e.finishedLanes = t, bt(e, Le, gt), Ae(e, ue()), null;
}
function fo(e, t) {
  var n = W;
  W |= 1;
  try {
    return e(t);
  } finally {
    W = n, W === 0 && ($n = ue() + 500, Bl && Kt());
  }
}
function sn(e) {
  Dt !== null && Dt.tag === 0 && !(W & 6) && zn();
  var t = W;
  W |= 1;
  var n = be.transition, r = G;
  try {
    if (be.transition = null, G = 1, e) return e();
  } finally {
    G = r, be.transition = n, W = t, !(W & 6) && Kt();
  }
}
function po() {
  Ve = Cn.current, J(Cn);
}
function tn(e, t) {
  e.finishedWork = null, e.finishedLanes = 0;
  var n = e.timeoutHandle;
  if (n !== -1 && (e.timeoutHandle = -1, Qf(n)), de !== null) for (n = de.return; n !== null; ) {
    var r = n;
    switch (Gi(r), r.tag) {
      case 1:
        r = r.type.childContextTypes, r != null && Nl();
        break;
      case 3:
        Mn(), J(Oe), J(Pe), to();
        break;
      case 5:
        eo(r);
        break;
      case 4:
        Mn();
        break;
      case 13:
        J(ne);
        break;
      case 19:
        J(ne);
        break;
      case 10:
        Xi(r.type._context);
        break;
      case 22:
      case 23:
        po();
    }
    n = n.return;
  }
  if (xe = e, de = e = Bt(e.current, null), Ne = Ve = t, me = 0, Ir = null, uo = Gl = on = 0, Le = sr = null, Jt !== null) {
    for (t = 0; t < Jt.length; t++) if (n = Jt[t], r = n.interleaved, r !== null) {
      n.interleaved = null;
      var l = r.next, i = n.pending;
      if (i !== null) {
        var o = i.next;
        i.next = l, r.next = o;
      }
      n.pending = r;
    }
    Jt = null;
  }
  return e;
}
function Ac(e, t) {
  do {
    var n = de;
    try {
      if (Yi(), al.current = Fl, _l) {
        for (var r = re.memoizedState; r !== null; ) {
          var l = r.queue;
          l !== null && (l.pending = null), r = r.next;
        }
        _l = !1;
      }
      if (an = 0, ve = pe = re = null, ir = !1, kr = 0, so.current = null, n === null || n.return === null) {
        me = 1, Ir = t, de = null;
        break;
      }
      e: {
        var i = e, o = n.return, s = n, u = t;
        if (t = Ne, s.flags |= 32768, u !== null && typeof u == "object" && typeof u.then == "function") {
          var d = u, j = s, c = j.tag;
          if (!(j.mode & 1) && (c === 0 || c === 11 || c === 15)) {
            var m = j.alternate;
            m ? (j.updateQueue = m.updateQueue, j.memoizedState = m.memoizedState, j.lanes = m.lanes) : (j.updateQueue = null, j.memoizedState = null);
          }
          var v = js(o);
          if (v !== null) {
            v.flags &= -257, Ns(v, o, s, i, t), v.mode & 1 && ys(i, d, t), t = v, u = d;
            var g = t.updateQueue;
            if (g === null) {
              var w = /* @__PURE__ */ new Set();
              w.add(u), t.updateQueue = w;
            } else g.add(u);
            break e;
          } else {
            if (!(t & 1)) {
              ys(i, d, t), mo();
              break e;
            }
            u = Error(P(426));
          }
        } else if (te && s.mode & 1) {
          var z = js(o);
          if (z !== null) {
            !(z.flags & 65536) && (z.flags |= 256), Ns(z, o, s, i, t), Ki(On(u, s));
            break e;
          }
        }
        i = u = On(u, s), me !== 4 && (me = 2), sr === null ? sr = [i] : sr.push(i), i = o;
        do {
          switch (i.tag) {
            case 3:
              i.flags |= 65536, t &= -t, i.lanes |= t;
              var p = wc(i, u, t);
              ps(i, p);
              break e;
            case 1:
              s = u;
              var f = i.type, h = i.stateNode;
              if (!(i.flags & 128) && (typeof f.getDerivedStateFromError == "function" || h !== null && typeof h.componentDidCatch == "function" && (Ut === null || !Ut.has(h)))) {
                i.flags |= 65536, t &= -t, i.lanes |= t;
                var C = Sc(i, s, t);
                ps(i, C);
                break e;
              }
          }
          i = i.return;
        } while (i !== null);
      }
      Bc(n);
    } catch (T) {
      t = T, de === n && n !== null && (de = n = n.return);
      continue;
    }
    break;
  } while (!0);
}
function Uc() {
  var e = zl.current;
  return zl.current = Fl, e === null ? Fl : e;
}
function mo() {
  (me === 0 || me === 3 || me === 2) && (me = 4), xe === null || !(on & 268435455) && !(Gl & 268435455) || Tt(xe, Ne);
}
function Dl(e, t) {
  var n = W;
  W |= 2;
  var r = Uc();
  (xe !== e || Ne !== t) && (gt = null, tn(e, t));
  do
    try {
      vp();
      break;
    } catch (l) {
      Ac(e, l);
    }
  while (!0);
  if (Yi(), W = n, zl.current = r, de !== null) throw Error(P(261));
  return xe = null, Ne = 0, me;
}
function vp() {
  for (; de !== null; ) Vc(de);
}
function gp() {
  for (; de !== null && !Vd(); ) Vc(de);
}
function Vc(e) {
  var t = Wc(e.alternate, e, Ve);
  e.memoizedProps = e.pendingProps, t === null ? Bc(e) : de = t, so.current = null;
}
function Bc(e) {
  var t = e;
  do {
    var n = t.alternate;
    if (e = t.return, t.flags & 32768) {
      if (n = cp(n, t), n !== null) {
        n.flags &= 32767, de = n;
        return;
      }
      if (e !== null) e.flags |= 32768, e.subtreeFlags = 0, e.deletions = null;
      else {
        me = 6, de = null;
        return;
      }
    } else if (n = up(n, t, Ve), n !== null) {
      de = n;
      return;
    }
    if (t = t.sibling, t !== null) {
      de = t;
      return;
    }
    de = t = e;
  } while (t !== null);
  me === 0 && (me = 5);
}
function bt(e, t, n) {
  var r = G, l = be.transition;
  try {
    be.transition = null, G = 1, xp(e, t, n, r);
  } finally {
    be.transition = l, G = r;
  }
  return null;
}
function xp(e, t, n, r) {
  do
    zn();
  while (Dt !== null);
  if (W & 6) throw Error(P(327));
  n = e.finishedWork;
  var l = e.finishedLanes;
  if (n === null) return null;
  if (e.finishedWork = null, e.finishedLanes = 0, n === e.current) throw Error(P(177));
  e.callbackNode = null, e.callbackPriority = 0;
  var i = n.lanes | n.childLanes;
  if (bd(e, i), e === xe && (de = xe = null, Ne = 0), !(n.subtreeFlags & 2064) && !(n.flags & 2064) || br || (br = !0, Qc(hl, function() {
    return zn(), null;
  })), i = (n.flags & 15990) !== 0, n.subtreeFlags & 15990 || i) {
    i = be.transition, be.transition = null;
    var o = G;
    G = 1;
    var s = W;
    W |= 4, so.current = null, fp(e, n), Mc(n, e), $f(ei), gl = !!Ja, ei = Ja = null, e.current = n, pp(n), Bd(), W = s, G = o, be.transition = i;
  } else e.current = n;
  if (br && (br = !1, Dt = e, Rl = l), i = e.pendingLanes, i === 0 && (Ut = null), Qd(n.stateNode), Ae(e, ue()), t !== null) for (r = e.onRecoverableError, n = 0; n < t.length; n++) l = t[n], r(l.value, { componentStack: l.stack, digest: l.digest });
  if (Tl) throw Tl = !1, e = ji, ji = null, e;
  return Rl & 1 && e.tag !== 0 && zn(), i = e.pendingLanes, i & 1 ? e === Ni ? ur++ : (ur = 0, Ni = e) : ur = 0, Kt(), null;
}
function zn() {
  if (Dt !== null) {
    var e = wu(Rl), t = be.transition, n = G;
    try {
      if (be.transition = null, G = 16 > e ? 16 : e, Dt === null) var r = !1;
      else {
        if (e = Dt, Dt = null, Rl = 0, W & 6) throw Error(P(331));
        var l = W;
        for (W |= 4, O = e.current; O !== null; ) {
          var i = O, o = i.child;
          if (O.flags & 16) {
            var s = i.deletions;
            if (s !== null) {
              for (var u = 0; u < s.length; u++) {
                var d = s[u];
                for (O = d; O !== null; ) {
                  var j = O;
                  switch (j.tag) {
                    case 0:
                    case 11:
                    case 15:
                      or(8, j, i);
                  }
                  var c = j.child;
                  if (c !== null) c.return = j, O = c;
                  else for (; O !== null; ) {
                    j = O;
                    var m = j.sibling, v = j.return;
                    if (Rc(j), j === d) {
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
                var w = g.child;
                if (w !== null) {
                  g.child = null;
                  do {
                    var z = w.sibling;
                    w.sibling = null, w = z;
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
                or(9, i, i.return);
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
                  Ql(9, s);
              }
            } catch (T) {
              se(s, s.return, T);
            }
            if (s === o) {
              O = null;
              break e;
            }
            var C = s.sibling;
            if (C !== null) {
              C.return = s.return, O = C;
              break e;
            }
            O = s.return;
          }
        }
        if (W = l, Kt(), ft && typeof ft.onPostCommitFiberRoot == "function") try {
          ft.onPostCommitFiberRoot(Ol, e);
        } catch {
        }
        r = !0;
      }
      return r;
    } finally {
      G = n, be.transition = t;
    }
  }
  return !1;
}
function Ds(e, t, n) {
  t = On(n, t), t = wc(e, t, 1), e = At(e, t, 1), t = ze(), e !== null && (Fr(e, 1, t), Ae(e, t));
}
function se(e, t, n) {
  if (e.tag === 3) Ds(e, e, n);
  else for (; t !== null; ) {
    if (t.tag === 3) {
      Ds(t, e, n);
      break;
    } else if (t.tag === 1) {
      var r = t.stateNode;
      if (typeof t.type.getDerivedStateFromError == "function" || typeof r.componentDidCatch == "function" && (Ut === null || !Ut.has(r))) {
        e = On(n, e), e = Sc(t, e, 1), t = At(t, e, 1), e = ze(), t !== null && (Fr(t, 1, e), Ae(t, e));
        break;
      }
    }
    t = t.return;
  }
}
function yp(e, t, n) {
  var r = e.pingCache;
  r !== null && r.delete(t), t = ze(), e.pingedLanes |= e.suspendedLanes & n, xe === e && (Ne & n) === n && (me === 4 || me === 3 && (Ne & 130023424) === Ne && 500 > ue() - co ? tn(e, 0) : uo |= n), Ae(e, t);
}
function Hc(e, t) {
  t === 0 && (e.mode & 1 ? (t = Vr, Vr <<= 1, !(Vr & 130023424) && (Vr = 4194304)) : t = 1);
  var n = ze();
  e = kt(e, t), e !== null && (Fr(e, t, n), Ae(e, n));
}
function jp(e) {
  var t = e.memoizedState, n = 0;
  t !== null && (n = t.retryLane), Hc(e, n);
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
  r !== null && r.delete(t), Hc(e, n);
}
var Wc;
Wc = function(e, t, n) {
  if (e !== null) if (e.memoizedProps !== t.pendingProps || Oe.current) Me = !0;
  else {
    if (!(e.lanes & n) && !(t.flags & 128)) return Me = !1, sp(e, t, n);
    Me = !!(e.flags & 131072);
  }
  else Me = !1, te && t.flags & 1048576 && qu(t, kl, t.index);
  switch (t.lanes = 0, t.tag) {
    case 2:
      var r = t.type;
      ol(e, t), e = t.pendingProps;
      var l = Rn(t, Pe.current);
      Fn(t, n), l = ro(null, t, r, e, l, n);
      var i = lo();
      return t.flags |= 1, typeof l == "object" && l !== null && typeof l.render == "function" && l.$$typeof === void 0 ? (t.tag = 1, t.memoizedState = null, t.updateQueue = null, $e(r) ? (i = !0, wl(t)) : i = !1, t.memoizedState = l.state !== null && l.state !== void 0 ? l.state : null, Zi(t), l.updater = Wl, t.stateNode = l, l._reactInternals = t, ui(t, r, e, n), t = fi(null, t, r, !0, i, n)) : (t.tag = 0, te && i && Qi(t), _e(null, t, l, n), t = t.child), t;
    case 16:
      r = t.elementType;
      e: {
        switch (ol(e, t), e = t.pendingProps, l = r._init, r = l(r._payload), t.type = r, l = t.tag = Sp(r), e = nt(r, e), l) {
          case 0:
            t = di(null, t, r, e, n);
            break e;
          case 1:
            t = ks(null, t, r, e, n);
            break e;
          case 11:
            t = ws(null, t, r, e, n);
            break e;
          case 14:
            t = Ss(null, t, r, nt(r.type, e), n);
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
      return r = t.type, l = t.pendingProps, l = t.elementType === r ? l : nt(r, l), di(e, t, r, l, n);
    case 1:
      return r = t.type, l = t.pendingProps, l = t.elementType === r ? l : nt(r, l), ks(e, t, r, l, n);
    case 3:
      e: {
        if (Ic(t), e === null) throw Error(P(387));
        r = t.pendingProps, i = t.memoizedState, l = i.element, ec(e, t), Il(t, r, null, n);
        var o = t.memoizedState;
        if (r = o.element, i.isDehydrated) if (i = { element: r, isDehydrated: !1, cache: o.cache, pendingSuspenseBoundaries: o.pendingSuspenseBoundaries, transitions: o.transitions }, t.updateQueue.baseState = i, t.memoizedState = i, t.flags & 256) {
          l = On(Error(P(423)), t), t = Cs(e, t, r, n, l);
          break e;
        } else if (r !== l) {
          l = On(Error(P(424)), t), t = Cs(e, t, r, n, l);
          break e;
        } else for (Be = $t(t.stateNode.containerInfo.firstChild), He = t, te = !0, lt = null, n = Zu(t, null, r, n), t.child = n; n; ) n.flags = n.flags & -3 | 4096, n = n.sibling;
        else {
          if (Dn(), r === l) {
            t = Ct(e, t, n);
            break e;
          }
          _e(e, t, r, n);
        }
        t = t.child;
      }
      return t;
    case 5:
      return tc(t), e === null && ii(t), r = t.type, l = t.pendingProps, i = e !== null ? e.memoizedProps : null, o = l.children, ti(r, l) ? o = null : i !== null && ti(r, i) && (t.flags |= 32), Ec(e, t), _e(e, t, o, n), t.child;
    case 6:
      return e === null && ii(t), null;
    case 13:
      return Pc(e, t, n);
    case 4:
      return Ji(t, t.stateNode.containerInfo), r = t.pendingProps, e === null ? t.child = Ln(t, null, r, n) : _e(e, t, r, n), t.child;
    case 11:
      return r = t.type, l = t.pendingProps, l = t.elementType === r ? l : nt(r, l), ws(e, t, r, l, n);
    case 7:
      return _e(e, t, t.pendingProps, n), t.child;
    case 8:
      return _e(e, t, t.pendingProps.children, n), t.child;
    case 12:
      return _e(e, t, t.pendingProps.children, n), t.child;
    case 10:
      e: {
        if (r = t.type._context, l = t.pendingProps, i = t.memoizedProps, o = l.value, X(Cl, r._currentValue), r._currentValue = o, i !== null) if (st(i.value, o)) {
          if (i.children === l.children && !Oe.current) {
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
                    var j = d.pending;
                    j === null ? u.next = u : (u.next = j.next, j.next = u), d.pending = u;
                  }
                }
                i.lanes |= n, u = i.alternate, u !== null && (u.lanes |= n), oi(
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
            o.lanes |= n, s = o.alternate, s !== null && (s.lanes |= n), oi(o, n, t), o = i.sibling;
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
        _e(e, t, l.children, n), t = t.child;
      }
      return t;
    case 9:
      return l = t.type, r = t.pendingProps.children, Fn(t, n), l = Ze(l), r = r(l), t.flags |= 1, _e(e, t, r, n), t.child;
    case 14:
      return r = t.type, l = nt(r, t.pendingProps), l = nt(r.type, l), Ss(e, t, r, l, n);
    case 15:
      return kc(e, t, t.type, t.pendingProps, n);
    case 17:
      return r = t.type, l = t.pendingProps, l = t.elementType === r ? l : nt(r, l), ol(e, t), t.tag = 1, $e(r) ? (e = !0, wl(t)) : e = !1, Fn(t, n), Nc(t, r, l), ui(t, r, l, n), fi(null, t, r, !0, e, n);
    case 19:
      return _c(e, t, n);
    case 22:
      return Cc(e, t, n);
  }
  throw Error(P(156, t.tag));
};
function Qc(e, t) {
  return xu(e, t);
}
function wp(e, t, n, r) {
  this.tag = e, this.key = n, this.sibling = this.child = this.return = this.stateNode = this.type = this.elementType = null, this.index = 0, this.ref = null, this.pendingProps = t, this.dependencies = this.memoizedState = this.updateQueue = this.memoizedProps = null, this.mode = r, this.subtreeFlags = this.flags = 0, this.deletions = null, this.childLanes = this.lanes = 0, this.alternate = null;
}
function Xe(e, t, n, r) {
  return new wp(e, t, n, r);
}
function ho(e) {
  return e = e.prototype, !(!e || !e.isReactComponent);
}
function Sp(e) {
  if (typeof e == "function") return ho(e) ? 1 : 0;
  if (e != null) {
    if (e = e.$$typeof, e === Ri) return 11;
    if (e === Di) return 14;
  }
  return 2;
}
function Bt(e, t) {
  var n = e.alternate;
  return n === null ? (n = Xe(e.tag, t, e.key, e.mode), n.elementType = e.elementType, n.type = e.type, n.stateNode = e.stateNode, n.alternate = e, e.alternate = n) : (n.pendingProps = t, n.type = e.type, n.flags = 0, n.subtreeFlags = 0, n.deletions = null), n.flags = e.flags & 14680064, n.childLanes = e.childLanes, n.lanes = e.lanes, n.child = e.child, n.memoizedProps = e.memoizedProps, n.memoizedState = e.memoizedState, n.updateQueue = e.updateQueue, t = e.dependencies, n.dependencies = t === null ? null : { lanes: t.lanes, firstContext: t.firstContext }, n.sibling = e.sibling, n.index = e.index, n.ref = e.ref, n;
}
function cl(e, t, n, r, l, i) {
  var o = 2;
  if (r = e, typeof e == "function") ho(e) && (o = 1);
  else if (typeof e == "string") o = 5;
  else e: switch (e) {
    case hn:
      return nn(n.children, l, i, t);
    case Ti:
      o = 8, l |= 8;
      break;
    case Ra:
      return e = Xe(12, n, t, l | 2), e.elementType = Ra, e.lanes = i, e;
    case Da:
      return e = Xe(13, n, t, l), e.elementType = Da, e.lanes = i, e;
    case La:
      return e = Xe(19, n, t, l), e.elementType = La, e.lanes = i, e;
    case tu:
      return Kl(n, l, i, t);
    default:
      if (typeof e == "object" && e !== null) switch (e.$$typeof) {
        case Js:
          o = 10;
          break e;
        case eu:
          o = 9;
          break e;
        case Ri:
          o = 11;
          break e;
        case Di:
          o = 14;
          break e;
        case _t:
          o = 16, r = null;
          break e;
      }
      throw Error(P(130, e == null ? e : typeof e, ""));
  }
  return t = Xe(o, n, t, l), t.elementType = e, t.type = r, t.lanes = i, t;
}
function nn(e, t, n, r) {
  return e = Xe(7, e, r, t), e.lanes = n, e;
}
function Kl(e, t, n, r) {
  return e = Xe(22, e, r, t), e.elementType = tu, e.lanes = n, e.stateNode = { isHidden: !1 }, e;
}
function _a(e, t, n) {
  return e = Xe(6, e, null, t), e.lanes = n, e;
}
function Fa(e, t, n) {
  return t = Xe(4, e.children !== null ? e.children : [], e.key, t), t.lanes = n, t.stateNode = { containerInfo: e.containerInfo, pendingChildren: null, implementation: e.implementation }, t;
}
function kp(e, t, n, r, l) {
  this.tag = t, this.containerInfo = e, this.finishedWork = this.pingCache = this.current = this.pendingChildren = null, this.timeoutHandle = -1, this.callbackNode = this.pendingContext = this.context = null, this.callbackPriority = 0, this.eventTimes = ca(0), this.expirationTimes = ca(-1), this.entangledLanes = this.finishedLanes = this.mutableReadLanes = this.expiredLanes = this.pingedLanes = this.suspendedLanes = this.pendingLanes = 0, this.entanglements = ca(0), this.identifierPrefix = r, this.onRecoverableError = l, this.mutableSourceEagerHydrationData = null;
}
function vo(e, t, n, r, l, i, o, s, u) {
  return e = new kp(e, t, n, s, u), t === 1 ? (t = 1, i === !0 && (t |= 8)) : t = 0, i = Xe(3, null, null, t), e.current = i, i.stateNode = e, i.memoizedState = { element: r, isDehydrated: n, cache: null, transitions: null, pendingSuspenseBoundaries: null }, Zi(i), e;
}
function Cp(e, t, n) {
  var r = 3 < arguments.length && arguments[3] !== void 0 ? arguments[3] : null;
  return { $$typeof: mn, key: r == null ? null : "" + r, children: e, containerInfo: t, implementation: n };
}
function Gc(e) {
  if (!e) return Wt;
  e = e._reactInternals;
  e: {
    if (dn(e) !== e || e.tag !== 1) throw Error(P(170));
    var t = e;
    do {
      switch (t.tag) {
        case 3:
          t = t.stateNode.context;
          break e;
        case 1:
          if ($e(t.type)) {
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
    if ($e(n)) return Gu(e, n, t);
  }
  return t;
}
function Kc(e, t, n, r, l, i, o, s, u) {
  return e = vo(n, r, !0, e, l, i, o, s, u), e.context = Gc(null), n = e.current, r = ze(), l = Vt(n), i = Nt(r, l), i.callback = t ?? null, At(n, i, l), e.current.lanes = l, Fr(e, l, r), Ae(e, r), e;
}
function ql(e, t, n, r) {
  var l = t.current, i = ze(), o = Vt(l);
  return n = Gc(n), t.context === null ? t.context = n : t.pendingContext = n, t = Nt(i, o), t.payload = { element: e }, r = r === void 0 ? null : r, r !== null && (t.callback = r), e = At(l, t, o), e !== null && (ot(e, l, o, i), ll(e, l, o)), o;
}
function Ll(e) {
  if (e = e.current, !e.child) return null;
  switch (e.child.tag) {
    case 5:
      return e.child.stateNode;
    default:
      return e.child.stateNode;
  }
}
function Ls(e, t) {
  if (e = e.memoizedState, e !== null && e.dehydrated !== null) {
    var n = e.retryLane;
    e.retryLane = n !== 0 && n < t ? n : t;
  }
}
function go(e, t) {
  Ls(e, t), (e = e.alternate) && Ls(e, t);
}
function Ep() {
  return null;
}
var qc = typeof reportError == "function" ? reportError : function(e) {
  console.error(e);
};
function xo(e) {
  this._internalRoot = e;
}
Yl.prototype.render = xo.prototype.render = function(e) {
  var t = this._internalRoot;
  if (t === null) throw Error(P(409));
  ql(e, t, null, null);
};
Yl.prototype.unmount = xo.prototype.unmount = function() {
  var e = this._internalRoot;
  if (e !== null) {
    this._internalRoot = null;
    var t = e.containerInfo;
    sn(function() {
      ql(null, e, null, null);
    }), t[St] = null;
  }
};
function Yl(e) {
  this._internalRoot = e;
}
Yl.prototype.unstable_scheduleHydration = function(e) {
  if (e) {
    var t = Cu();
    e = { blockedOn: null, target: e, priority: t };
    for (var n = 0; n < zt.length && t !== 0 && t < zt[n].priority; n++) ;
    zt.splice(n, 0, e), n === 0 && Iu(e);
  }
};
function yo(e) {
  return !(!e || e.nodeType !== 1 && e.nodeType !== 9 && e.nodeType !== 11);
}
function Xl(e) {
  return !(!e || e.nodeType !== 1 && e.nodeType !== 9 && e.nodeType !== 11 && (e.nodeType !== 8 || e.nodeValue !== " react-mount-point-unstable "));
}
function Ms() {
}
function Ip(e, t, n, r, l) {
  if (l) {
    if (typeof r == "function") {
      var i = r;
      r = function() {
        var d = Ll(o);
        i.call(d);
      };
    }
    var o = Kc(t, r, e, 0, null, !1, !1, "", Ms);
    return e._reactRootContainer = o, e[St] = o.current, yr(e.nodeType === 8 ? e.parentNode : e), sn(), o;
  }
  for (; l = e.lastChild; ) e.removeChild(l);
  if (typeof r == "function") {
    var s = r;
    r = function() {
      var d = Ll(u);
      s.call(d);
    };
  }
  var u = vo(e, 0, !1, null, null, !1, !1, "", Ms);
  return e._reactRootContainer = u, e[St] = u.current, yr(e.nodeType === 8 ? e.parentNode : e), sn(function() {
    ql(t, u, n, r);
  }), u;
}
function bl(e, t, n, r, l) {
  var i = n._reactRootContainer;
  if (i) {
    var o = i;
    if (typeof l == "function") {
      var s = l;
      l = function() {
        var u = Ll(o);
        s.call(u);
      };
    }
    ql(t, o, e, l);
  } else o = Ip(n, t, e, l, r);
  return Ll(o);
}
Su = function(e) {
  switch (e.tag) {
    case 3:
      var t = e.stateNode;
      if (t.current.memoizedState.isDehydrated) {
        var n = Jn(t.pendingLanes);
        n !== 0 && (Oi(t, n | 1), Ae(t, ue()), !(W & 6) && ($n = ue() + 500, Kt()));
      }
      break;
    case 13:
      sn(function() {
        var r = kt(e, 1);
        if (r !== null) {
          var l = ze();
          ot(r, e, 1, l);
        }
      }), go(e, 1);
  }
};
$i = function(e) {
  if (e.tag === 13) {
    var t = kt(e, 134217728);
    if (t !== null) {
      var n = ze();
      ot(t, e, 134217728, n);
    }
    go(e, 134217728);
  }
};
ku = function(e) {
  if (e.tag === 13) {
    var t = Vt(e), n = kt(e, t);
    if (n !== null) {
      var r = ze();
      ot(n, e, t, r);
    }
    go(e, t);
  }
};
Cu = function() {
  return G;
};
Eu = function(e, t) {
  var n = G;
  try {
    return G = e, t();
  } finally {
    G = n;
  }
};
Qa = function(e, t, n) {
  switch (t) {
    case "input":
      if ($a(e, n), t = n.name, n.type === "radio" && t != null) {
        for (n = e; n.parentNode; ) n = n.parentNode;
        for (n = n.querySelectorAll("input[name=" + JSON.stringify("" + t) + '][type="radio"]'), t = 0; t < n.length; t++) {
          var r = n[t];
          if (r !== e && r.form === e.form) {
            var l = Vl(r);
            if (!l) throw Error(P(90));
            ru(r), $a(r, l);
          }
        }
      }
      break;
    case "textarea":
      au(e, n);
      break;
    case "select":
      t = n.value, t != null && En(e, !!n.multiple, t, !1);
  }
};
fu = fo;
pu = sn;
var Pp = { usingClientEntryPoint: !1, Events: [Tr, yn, Vl, cu, du, fo] }, Xn = { findFiberByHostInstance: Zt, bundleType: 0, version: "18.3.1", rendererPackageName: "react-dom" }, _p = { bundleType: Xn.bundleType, version: Xn.version, rendererPackageName: Xn.rendererPackageName, rendererConfig: Xn.rendererConfig, overrideHookState: null, overrideHookStateDeletePath: null, overrideHookStateRenamePath: null, overrideProps: null, overridePropsDeletePath: null, overridePropsRenamePath: null, setErrorHandler: null, setSuspenseHandler: null, scheduleUpdate: null, currentDispatcherRef: Et.ReactCurrentDispatcher, findHostInstanceByFiber: function(e) {
  return e = vu(e), e === null ? null : e.stateNode;
}, findFiberByHostInstance: Xn.findFiberByHostInstance || Ep, findHostInstancesForRefresh: null, scheduleRefresh: null, scheduleRoot: null, setRefreshHandler: null, getCurrentFiber: null, reconcilerVersion: "18.3.1-next-f1338f8080-20240426" };
if (typeof __REACT_DEVTOOLS_GLOBAL_HOOK__ < "u") {
  var Zr = __REACT_DEVTOOLS_GLOBAL_HOOK__;
  if (!Zr.isDisabled && Zr.supportsFiber) try {
    Ol = Zr.inject(_p), ft = Zr;
  } catch {
  }
}
Qe.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED = Pp;
Qe.createPortal = function(e, t) {
  var n = 2 < arguments.length && arguments[2] !== void 0 ? arguments[2] : null;
  if (!yo(t)) throw Error(P(200));
  return Cp(e, t, null, n);
};
Qe.createRoot = function(e, t) {
  if (!yo(e)) throw Error(P(299));
  var n = !1, r = "", l = qc;
  return t != null && (t.unstable_strictMode === !0 && (n = !0), t.identifierPrefix !== void 0 && (r = t.identifierPrefix), t.onRecoverableError !== void 0 && (l = t.onRecoverableError)), t = vo(e, 1, !1, null, null, n, !1, r, l), e[St] = t.current, yr(e.nodeType === 8 ? e.parentNode : e), new xo(t);
};
Qe.findDOMNode = function(e) {
  if (e == null) return null;
  if (e.nodeType === 1) return e;
  var t = e._reactInternals;
  if (t === void 0)
    throw typeof e.render == "function" ? Error(P(188)) : (e = Object.keys(e).join(","), Error(P(268, e)));
  return e = vu(t), e = e === null ? null : e.stateNode, e;
};
Qe.flushSync = function(e) {
  return sn(e);
};
Qe.hydrate = function(e, t, n) {
  if (!Xl(t)) throw Error(P(200));
  return bl(null, e, t, !0, n);
};
Qe.hydrateRoot = function(e, t, n) {
  if (!yo(e)) throw Error(P(405));
  var r = n != null && n.hydratedSources || null, l = !1, i = "", o = qc;
  if (n != null && (n.unstable_strictMode === !0 && (l = !0), n.identifierPrefix !== void 0 && (i = n.identifierPrefix), n.onRecoverableError !== void 0 && (o = n.onRecoverableError)), t = Kc(t, null, e, 1, n ?? null, l, !1, i, o), e[St] = t.current, yr(e), r) for (e = 0; e < r.length; e++) n = r[e], l = n._getVersion, l = l(n._source), t.mutableSourceEagerHydrationData == null ? t.mutableSourceEagerHydrationData = [n, l] : t.mutableSourceEagerHydrationData.push(
    n,
    l
  );
  return new Yl(t);
};
Qe.render = function(e, t, n) {
  if (!Xl(t)) throw Error(P(200));
  return bl(null, e, t, !1, n);
};
Qe.unmountComponentAtNode = function(e) {
  if (!Xl(e)) throw Error(P(40));
  return e._reactRootContainer ? (sn(function() {
    bl(null, null, e, !1, function() {
      e._reactRootContainer = null, e[St] = null;
    });
  }), !0) : !1;
};
Qe.unstable_batchedUpdates = fo;
Qe.unstable_renderSubtreeIntoContainer = function(e, t, n, r) {
  if (!Xl(n)) throw Error(P(200));
  if (e == null || e._reactInternals === void 0) throw Error(P(38));
  return bl(e, t, n, !1, r);
};
Qe.version = "18.3.1-next-f1338f8080-20240426";
function Yc() {
  if (!(typeof __REACT_DEVTOOLS_GLOBAL_HOOK__ > "u" || typeof __REACT_DEVTOOLS_GLOBAL_HOOK__.checkDCE != "function"))
    try {
      __REACT_DEVTOOLS_GLOBAL_HOOK__.checkDCE(Yc);
    } catch (e) {
      console.error(e);
    }
}
Yc(), Ys.exports = Qe;
var Fp = Ys.exports, Xc, Os = Fp;
Xc = Os.createRoot, Os.hydrateRoot;
class zp extends Error {
  constructor(t, n) {
    super(t), this.status = n, this.name = "ApiError";
  }
}
function Tp(e, t) {
  async function n(r, l = {}) {
    const i = { ...l.headers ?? {} }, o = t();
    o && (i.Authorization = "Bearer " + o);
    let s;
    l.body !== void 0 && (i["Content-Type"] = "application/json", s = JSON.stringify(l.body));
    const u = await e(r, { method: l.method ?? "GET", headers: i, body: s });
    if (!u.ok) {
      let j = `HTTP ${u.status}`;
      try {
        const c = await u.json();
        j = c.detail || c.title || j;
      } catch {
      }
      throw new zp(j, u.status);
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
const bc = N.createContext(null);
function ht() {
  const e = N.useContext(bc);
  if (!e) throw new Error("Fuera del módulo de documentos");
  return e;
}
function Rp(e) {
  return Tp((t, n) => fetch(t, n), e.token);
}
async function ki(e, t) {
  const n = e.token(), r = await fetch(t, { headers: n ? { Authorization: "Bearer " + n } : {} });
  if (!r.ok) throw new Error("No se pudo generar el documento");
  const l = URL.createObjectURL(await r.blob());
  window.open(l, "_blank"), setTimeout(() => URL.revokeObjectURL(l), 6e4);
}
const Zc = new Intl.NumberFormat("es-ES", { minimumFractionDigits: 2, maximumFractionDigits: 2 }), Dp = new Intl.NumberFormat("es-ES", { maximumFractionDigits: 3 }), M = (e) => `${Zc.format(Number(e) || 0)} €`, ge = (e) => Zc.format(Number(e) || 0), Fe = (e) => Dp.format(Number(e) || 0), Ue = (e) => {
  if (!e) return "";
  const t = String(e).slice(0, 10).split("-");
  return t.length === 3 ? `${t[2]}/${t[1]}/${t[0]}` : String(e);
}, mt = () => (/* @__PURE__ */ new Date()).toISOString().slice(0, 10), dl = (e) => Math.round((e + Number.EPSILON) * 100) / 100;
let Lp = 0;
const Dr = () => `l${Date.now().toString(36)}${(++Lp).toString(36)}`;
function Lr(e, t) {
  const [n, r] = N.useState(e);
  return N.useEffect(() => {
    const l = setTimeout(() => r(e), t);
    return () => clearTimeout(l);
  }, [e, t]), n;
}
function Zl() {
  const e = N.useRef(0);
  return () => {
    const t = ++e.current;
    return () => t === e.current;
  };
}
function jo(e) {
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
  const { api: t, navegar: n } = ht(), [r, l] = N.useState(""), [i, o] = N.useState(""), [s, u] = N.useState(""), [d, j] = N.useState(""), [c, m] = N.useState(1), [v, g] = N.useState(null), [w, z] = N.useState(0), [p, f] = N.useState(""), h = Lr(r, 250), C = 50;
  N.useEffect(() => m(1), [h, i, s, d, e.tipo]), N.useEffect(() => {
    f(""), (async () => {
      switch (e.tipo) {
        case "factura": {
          const E = new URLSearchParams({ pagina: String(c), tamanoPagina: String(C) });
          h.trim() && E.set("texto", h.trim()), i && E.set("estado", i), s && E.set("desde", s), d && E.set("hasta", d);
          const _ = await t.get(`/facturas/buscar?${E}`);
          return z(_.total), _.elementos.map((A) => ({ id: A.id, numero: A.numeroCompleto, fecha: A.fechaEmision, tercero: A.clienteNombre + (A.clienteNif ? ` · ${A.clienteNif}` : ""), total: A.total, estado: A.estado, extra: A.tipo !== "Ordinaria" ? A.tipo : void 0 }));
        }
        case "gasto": {
          const E = new URLSearchParams({ pagina: String(c), tamanoPagina: String(C) });
          h.trim() && E.set("texto", h.trim()), i && E.set("estado", i), s && E.set("desde", s), d && E.set("hasta", d);
          const _ = await t.get(`/gastos/buscar?${E}`);
          return z(_.total), _.elementos.map((A) => ({ id: A.id, numero: A.numeroFactura ?? "—", fecha: A.fecha, tercero: `${A.proveedorTexto ?? ""}${A.numeroFactura ? "" : ` · ${A.concepto}`}`, total: A.total, estado: A.estado === "Anulado" ? "Anulada" : A.estado }));
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
  const T = N.useMemo(() => {
    if (!v || e.tipo === "factura" || e.tipo === "gasto") return v ?? [];
    const L = h.trim().toLowerCase();
    return v.filter((E) => (!L || E.numero.toLowerCase().includes(L) || E.tercero.toLowerCase().includes(L)) && (!i || E.estado === i) && (!s || E.fecha >= s) && (!d || E.fecha <= d));
  }, [v, h, i, s, d, e.tipo]), F = T.reduce((L, E) => L + (E.estado === "Anulada" || E.estado === "Cancelado" ? 0 : E.total), 0), R = e.tipo === "factura" || e.tipo === "gasto", D = R ? Math.max(1, Math.ceil(w / C)) : 1;
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
      /* @__PURE__ */ a.jsx("input", { type: "date", value: d, onChange: (L) => j(L.target.value), title: "Hasta" })
    ] }),
    p && /* @__PURE__ */ a.jsx("p", { className: "dx-rojo", children: p }),
    v === null ? /* @__PURE__ */ a.jsx("p", { className: "muted", children: "Cargando…" }) : T.length === 0 ? /* @__PURE__ */ a.jsx("p", { className: "muted", children: "No hay documentos con estos filtros." }) : /* @__PURE__ */ a.jsxs("table", { className: "dx-lista-docs", children: [
      /* @__PURE__ */ a.jsx("thead", { children: /* @__PURE__ */ a.jsxs("tr", { children: [
        /* @__PURE__ */ a.jsx("th", { children: "Número" }),
        /* @__PURE__ */ a.jsx("th", { children: "Fecha" }),
        /* @__PURE__ */ a.jsx("th", { children: e.tipo === "compra" || e.tipo === "gasto" ? "Proveedor" : "Cliente" }),
        /* @__PURE__ */ a.jsx("th", { children: "Estado" }),
        /* @__PURE__ */ a.jsx("th", { className: "num", children: "Total" })
      ] }) }),
      /* @__PURE__ */ a.jsx("tbody", { children: T.map((L) => /* @__PURE__ */ a.jsxs("tr", { onClick: () => n({ tipo: e.tipo, pantalla: "vista", id: L.id }), tabIndex: 0, onKeyDown: (E) => E.key === "Enter" && n({ tipo: e.tipo, pantalla: "vista", id: L.id }), children: [
        /* @__PURE__ */ a.jsxs("td", { className: "mono", children: [
          /* @__PURE__ */ a.jsx("strong", { children: L.numero }),
          L.extra && /* @__PURE__ */ a.jsx("span", { className: "pill part", style: { marginLeft: 6 }, children: L.extra })
        ] }),
        /* @__PURE__ */ a.jsx("td", { children: Ue(L.fecha) }),
        /* @__PURE__ */ a.jsx("td", { children: L.tercero }),
        /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx("span", { className: jo(L.estado), children: L.estado }) }),
        /* @__PURE__ */ a.jsx("td", { className: "num", children: /* @__PURE__ */ a.jsx("strong", { children: M(L.total) }) })
      ] }, L.id)) }),
      /* @__PURE__ */ a.jsx("tfoot", { children: /* @__PURE__ */ a.jsxs("tr", { children: [
        /* @__PURE__ */ a.jsxs("td", { colSpan: 4, className: "muted", children: [
          R ? `${w} documentos` : `${T.length} documentos`,
          " · suma de la página sin anulados"
        ] }),
        /* @__PURE__ */ a.jsx("td", { className: "num", children: /* @__PURE__ */ a.jsx("strong", { children: M(F) }) })
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
function un(e) {
  return N.useEffect(() => {
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
  const { api: t } = ht(), [n, r] = N.useState(!1), [l, i] = N.useState([]), [o, s] = N.useState(null), [u, d] = N.useState(0), j = Lr(e.texto, 180), c = o === e.texto.trim() ? l : [], m = Zl();
  N.useEffect(() => {
    if (!n) return;
    const g = m(), w = encodeURIComponent(j.trim());
    t.get(`/productos/buscar?texto=${w}&tamanoPagina=12`).then((z) => g() && (i(z.elementos ?? []), s(j.trim()), d(0))).catch(() => g() && (i([]), s(j.trim())));
  }, [j, n]);
  function v(g) {
    var w;
    if (n && g.key === "Enter" && e.texto.trim() && !c.length) {
      g.preventDefault(), g.stopPropagation();
      return;
    }
    if (n && c.length) {
      if (g.key === "ArrowDown") return g.preventDefault(), d((z) => Math.min(z + 1, c.length - 1));
      if (g.key === "ArrowUp") return g.preventDefault(), d((z) => Math.max(z - 1, 0));
      if (g.key === "Enter") {
        g.preventDefault(), g.stopPropagation(), e.alElegir(c[u]), r(!1);
        return;
      }
    }
    if (g.key === "Escape") return r(!1);
    if (g.key === "F2") return g.preventDefault(), r(!0);
    (w = e.alTeclaFuera) == null || w.call(e, g);
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
        ...Object.fromEntries(Object.entries(e.datos ?? {}).map(([g, w]) => [`data-${g}`, w]))
      }
    ),
    n && c.length > 0 && /* @__PURE__ */ a.jsx("div", { className: "dx-lista", children: c.map((g, w) => /* @__PURE__ */ a.jsxs(
      "div",
      {
        className: "dx-opcion" + (w === u ? " activa" : ""),
        onMouseDown: (z) => (z.preventDefault(), e.alElegir(g), r(!1)),
        onMouseEnter: () => d(w),
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
              Fe(g.stock)
            ] })
          ] })
        ]
      },
      g.id
    )) })
  ] });
}
function No(e) {
  const [t, n] = N.useState(""), [r, l] = N.useState(!1), [i, o] = N.useState(0), s = e.terceros.find((c) => c.id === e.valor), u = N.useMemo(() => {
    const c = t.trim().toLowerCase();
    return e.terceros.filter((m) => m.activo !== !1 && (!c || m.nombre.toLowerCase().includes(c) || (m.nifFiscal ?? "").toLowerCase().includes(c))).slice(0, 30);
  }, [t, e.terceros]), d = N.useRef(null);
  function j(c) {
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
            if (c.key === "Enter" && u[i]) return c.preventDefault(), j(u[i]);
            if (c.key === "Escape") return l(!1);
          }
        }
      ),
      r && u.length > 0 && /* @__PURE__ */ a.jsx("div", { className: "dx-lista", children: u.map((c, m) => /* @__PURE__ */ a.jsxs("div", { className: "dx-opcion" + (m === i ? " activa" : ""), onMouseDown: (v) => (v.preventDefault(), j(c)), onMouseEnter: () => o(m), children: [
        /* @__PURE__ */ a.jsx("strong", { children: c.nombre }),
        /* @__PURE__ */ a.jsx("span", { className: "muted", children: [c.nifFiscal, c.poblacion].filter(Boolean).join(" · ") })
      ] }, c.id)) })
    ] })
  ] });
}
const Vp = { Porcentaje: "%", PorUnidad: "€/ud", PorKilo: "€/kg", Importe: "€" };
function wo(e) {
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
function Jl(e) {
  var t;
  return (t = e.conceptos) != null && t.length ? /* @__PURE__ */ a.jsx("div", { className: "dx-aplicados", children: e.conceptos.map((n, r) => /* @__PURE__ */ a.jsxs("div", { children: [
    "· ",
    n.nombre,
    n.calculo === "Porcentaje" ? ` (${Fe(n.valor)} %)` : "",
    n.repartido ? " · del documento" : "",
    n.efecto === "Coste" ? " · coste" : "",
    ": ",
    /* @__PURE__ */ a.jsx("strong", { children: M(n.importe) })
  ] }, r)) }) : null;
}
const cr = () => ({ clave: Dr(), productoId: null, descripcion: "", cantidad: 1, precio: null, dto: 0, iva: null });
function Jc(e) {
  const t = N.useRef(null), [n, r] = N.useState(/* @__PURE__ */ new Set()), l = e.modo === "venta", i = l ? 7 : 5, o = (c, m) => e.alCambiar(e.lineas.map((v) => v.clave === c ? { ...v, ...m } : v)), s = (c) => {
    const m = e.lineas.filter((v) => v.clave !== c);
    e.alCambiar(m.length ? m : [cr()]);
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
        v === e.lineas.length - 1 ? (e.alCambiar([...e.lineas, cr()]), setTimeout(() => u(v + 1, 0), 30)) : u(v + 1, 0);
      } else c.key === "ArrowDown" && m.tagName !== "SELECT" ? (c.preventDefault(), u(Math.min(v + 1, e.lineas.length - 1), g)) : c.key === "ArrowUp" && m.tagName !== "SELECT" && (c.preventDefault(), u(Math.max(v - 1, 0), g));
  }
  const j = (c) => r((m) => {
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
        const v = e.calculos[m], g = (v == null ? void 0 : v.conceptos) ?? [], w = l && c.controlarStock && c.stock != null && c.cantidad > c.stock, z = v && v.margen != null && v.importe ? v.margen / v.importe * 100 : null;
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
                c.controlarStock && /* @__PURE__ */ a.jsxs("span", { className: w ? "dx-rojo" : "", children: [
                  " · stock ",
                  Fe(c.stock)
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
              g.length > 0 && !n.has(c.clave) && /* @__PURE__ */ a.jsx("button", { type: "button", className: "dx-resumen-conc", onClick: () => j(c.clave), title: "Ver y cambiar los conceptos", children: g.map((p) => `${p.importe < 0 ? "−" : "+"} ${p.codigo.toLowerCase()} ${ge(Math.abs(p.importe))}${p.efecto === "Coste" ? " (coste)" : ""}`).join(" · ") })
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
                placeholder: v ? ge(v.precio) : "",
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
              z != null && /* @__PURE__ */ a.jsxs("div", { className: "dx-sub", children: [
                ge(z),
                " %"
              ] })
            ] }) : (v == null ? void 0 : v.costeUnitarioEntrada) != null && /* @__PURE__ */ a.jsxs("span", { className: "muted", children: [
              M(v.costeUnitarioEntrada),
              "/ud"
            ] }) }),
            /* @__PURE__ */ a.jsxs("td", { className: "right", style: { whiteSpace: "nowrap" }, children: [
              !e.soloLectura && e.catalogo.length > 0 && /* @__PURE__ */ a.jsx("button", { type: "button", className: "dx-icono" + (n.has(c.clave) ? " activo" : ""), title: "Conceptos de la línea", onClick: () => j(c.clave), "data-f": m, "data-c": l ? 6 : 4, children: "±" }),
              !e.soloLectura && /* @__PURE__ */ a.jsx("button", { type: "button", className: "dx-icono", title: "Quitar la línea", onClick: () => s(c.clave), children: "✕" })
            ] })
          ] }, c.clave),
          n.has(c.clave) && /* @__PURE__ */ a.jsx("tr", { className: "dx-fila-conc", children: /* @__PURE__ */ a.jsx("td", { colSpan: l ? 9 : 7, children: /* @__PURE__ */ a.jsx(wo, { catalogo: e.catalogo, lista: c.conceptos, sugeridos: e.sugeridos[c.clave], alCambiar: (p) => o(c.clave, { conceptos: p }) }) }) }, c.clave + "c")
        ];
      }) })
    ] }),
    !e.soloLectura && /* @__PURE__ */ a.jsx("button", { type: "button", className: "btn small ghost", style: { marginTop: 8 }, onClick: () => (e.alCambiar([...e.lineas, cr()]), setTimeout(() => u(e.lineas.length, 0), 30)), children: "+ Añadir línea" }),
    !e.soloLectura && /* @__PURE__ */ a.jsx("span", { className: "muted dx-ayuda", children: "Intro: siguiente casilla · ↑↓: cambiar de línea · F2: buscar artículo · el precio en gris es el de tarifa" })
  ] });
}
const Bp = { presupuesto: "presupuesto", pedido: "pedido de venta", factura: "factura" };
function ed(e) {
  const t = /* @__PURE__ */ new Map();
  for (const n of e)
    for (const r of n.conceptos ?? [])
      if (r.repartido) {
        const l = t.get(r.conceptoId);
        t.set(r.conceptoId, { conceptoId: r.conceptoId, valor: r.calculo === "Importe" ? dl(((l == null ? void 0 : l.valor) ?? 0) + r.valor) : r.valor });
      }
  return {
    porLinea: e.map((n) => (n.conceptos ?? []).filter((r) => !r.repartido).map((r) => ({ conceptoId: r.conceptoId, valor: r.valor }))),
    documento: [...t.values()]
  };
}
function Hp(e) {
  var ko, Co, Eo, Io;
  const { api: t, anfitrion: n } = ht(), r = !!((ko = e.semilla) != null && ko.rectificaId), [l, i] = N.useState([]), [o, s] = N.useState([]), [u, d] = N.useState([]), [j, c] = N.useState([]), [m, v] = N.useState([]), [g, w] = N.useState(((Co = e.semilla) == null ? void 0 : Co.clienteId) ?? ""), [z, p] = N.useState(e.tipo === "pedido" && ((Eo = e.semilla) != null && Eo.fecha) ? e.semilla.fecha : mt()), [f, h] = N.useState(""), [C, T] = N.useState(""), [F, R] = N.useState(0), [D, L] = N.useState(!1), [E, _] = N.useState(null), [A, De] = N.useState(30), [Se, Ke] = N.useState(""), [q, he] = N.useState([cr()]), [y, x] = N.useState([]), [S, $] = N.useState(null), [B, Q] = N.useState(""), [ye, b] = N.useState(!1), [ae, et] = N.useState(!1), [It, qt] = N.useState(!1), na = Zl();
  N.useEffect(() => {
    t.get("/clientes").then(i).catch(() => i([])), t.get("/tipos-iva").then((I) => s(I.filter((U) => U.activo))).catch(() => s([])), t.get("/formas-pago").then((I) => d(I.filter((U) => U.activo))).catch(() => d([])), t.get("/series").then((I) => c([...new Set(I.filter((U) => U.tipoDocumento === "Factura").map((U) => U.prefijo))])).catch(() => c([])), t.get("/conceptos-linea?ambito=Ventas&activos=true").then(v).catch(() => v([]));
  }, [t]), N.useEffect(() => {
    const I = e.semilla;
    if (!I || !I.lineas.length) return;
    const { porLinea: U, documento: ee } = ed(I.lineas), oe = I.lineas.map((Y, la) => ({
      clave: Dr(),
      productoId: Y.productoId ?? null,
      descripcion: Y.descripcion,
      cantidad: Y.cantidad,
      precio: Y.precioUnitario,
      dto: Y.porcentajeDescuento,
      iva: Y.codigoIva,
      conceptos: r ? [] : U[la]
    }));
    he(oe), x(r ? [] : ee), Promise.all(oe.map((Y) => Y.productoId ? t.get(`/productos/${Y.productoId}`).catch(() => null) : Promise.resolve(null))).then(
      (Y) => he((la) => la.map((Po, fn) => Y[fn] ? { ...Po, referencia: Y[fn].referencia ?? Y[fn].nombre, unidad: Y[fn].unidad, stock: Y[fn].stock, controlarStock: Y[fn].controlarStock } : Po))
    );
  }, [e.semilla, t, r]);
  const ie = l.find((I) => I.id === g);
  N.useEffect(() => {
    ie && (L(!!ie.recargoEquivalencia), ie.formaPagoDefectoId && T(ie.formaPagoDefectoId));
  }, [ie]);
  const Yt = N.useMemo(() => q.map((I, U) => ({ l: I, i: U })).filter(({ l: I }) => (I.productoId || I.descripcion.trim()) && I.cantidad > 0), [q]), Bn = N.useMemo(
    () => ({
      clienteId: g,
      fechaEmision: e.tipo === "factura" ? z : null,
      serie: f || null,
      diasVencimiento: F,
      formaPagoId: C || null,
      recargoEquivalencia: D,
      porcentajeIrpf: E,
      conceptosDocumento: y,
      lineas: Yt.map(({ l: I }) => ({
        cantidad: I.cantidad,
        descripcion: I.descripcion.trim() || null,
        precioUnitario: I.precio,
        codigoIva: I.iva,
        porcentajeDescuento: I.dto,
        productoId: I.productoId,
        ...r ? { conceptos: [] } : I.conceptos === void 0 ? {} : { conceptos: I.conceptos }
      }))
    }),
    [g, z, f, F, C, D, E, y, Yt, e.tipo, r]
  ), k = Lr(Bn, 350);
  N.useEffect(() => {
    if (!k.clienteId || k.lineas.length === 0) {
      $(null), Q(k.clienteId ? "" : "Elige el cliente para calcular precios e importes.");
      return;
    }
    const I = na();
    b(!0), t.post("/facturas/simular", k).then((U) => I() && ($(U), Q(""))).catch((U) => I() && ($(null), Q(U.message))).finally(() => I() && b(!1));
  }, [k, t]);
  const V = N.useMemo(() => {
    const I = q.map(() => {
    });
    return S && Yt.forEach(({ i: U }, ee) => {
      const oe = S.lineas[ee];
      oe && (I[U] = { precio: oe.precioUnitario, dto: oe.porcentajeDescuento, iva: oe.codigoIva, importe: oe.base, margen: oe.productoId || oe.costeUnitario || oe.costeConceptos ? oe.margen : void 0, conceptos: oe.conceptos });
    }), I;
  }, [S, q, Yt]), fe = N.useMemo(() => {
    const I = {};
    return q.forEach((U, ee) => {
      var oe;
      return I[U.clave] = (((oe = V[ee]) == null ? void 0 : oe.conceptos) ?? []).filter((Y) => !Y.repartido).map((Y) => ({ conceptoId: Y.conceptoId, valor: Y.valor }));
    }), I;
  }, [q, V]);
  function ce(I, U) {
    he(
      (ee) => ee.map(
        (oe) => oe.clave === I ? { ...oe, productoId: U.id, referencia: U.referencia ?? U.nombre, descripcion: U.nombre, precio: null, dto: 0, iva: null, conceptos: void 0, unidad: U.unidad, stock: U.stock, controlarStock: U.controlarStock } : oe
      )
    );
  }
  const K = N.useMemo(() => {
    const I = /* @__PURE__ */ new Map();
    for (const U of (S == null ? void 0 : S.lineas) ?? []) {
      const ee = I.get(U.codigoIva) ?? { base: 0, cuota: 0, pct: U.porcentajeIva };
      ee.base += U.base, ee.cuota += U.cuotaIva, I.set(U.codigoIva, ee);
    }
    return [...I.entries()];
  }, [S]), vt = ((S == null ? void 0 : S.lineas) ?? []).reduce((I, U) => I + (U.base - U.margen), 0), ra = S ? S.baseImponible - vt : 0, td = (I) => {
    var U;
    return ((U = o.find((ee) => ee.codigo === I)) == null ? void 0 : U.nombre) ?? I;
  };
  async function So() {
    if (S) {
      et(!0);
      try {
        const I = Yt.map(({ l: ee }, oe) => {
          const Y = S.lineas[oe];
          return {
            cantidad: ee.cantidad,
            descripcion: Y.descripcion,
            precioUnitario: Y.precioUnitario,
            codigoIva: Y.codigoIva,
            porcentajeDescuento: Y.porcentajeDescuento,
            productoId: ee.productoId,
            ...r ? {} : ee.conceptos === void 0 ? {} : { conceptos: ee.conceptos }
          };
        });
        let U;
        if (r)
          U = (await t.post(`/facturas/${e.semilla.rectificaId}/rectificar`, { motivo: Se, lineas: I, fechaEmision: z, porcentajeIrpf: E, serie: f || null })).id;
        else if (e.tipo === "factura")
          U = (await t.post("/facturas", { ...Bn, lineas: I })).id;
        else if (e.tipo === "presupuesto") {
          const ee = { clienteId: g, diasValidez: A, lineas: I, conceptosDocumento: y };
          U = e.id ? (await t.put(`/presupuestos/${e.id}`, ee)).id : (await t.post("/presupuestos", ee)).id;
        } else {
          const ee = { clienteId: g, fecha: z, lineas: I, conceptosDocumento: y };
          U = e.id ? (await t.put(`/pedidos-venta/${e.id}`, ee)).id : (await t.post("/pedidos-venta", ee)).id;
        }
        n.aviso(e.tipo === "factura" ? "Factura emitida." : "Documento guardado.", "ok"), e.alGuardar(U);
      } catch (I) {
        n.aviso(I.message, "err");
      } finally {
        et(!1), qt(!1);
      }
    }
  }
  const nd = r ? `Rectificativa de la factura ${((Io = e.semilla) == null ? void 0 : Io.rectificaNumero) ?? ""}` : `${e.id ? "Editar" : "Nuevo"} ${Bp[e.tipo]}`.replace("Nuevo factura", "Nueva factura"), rd = !!S && !ye && (!r || Se.trim().length > 0);
  return /* @__PURE__ */ a.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsxs("div", { className: "panel-head", children: [
        /* @__PURE__ */ a.jsx("h2", { children: nd }),
        /* @__PURE__ */ a.jsxs("div", { style: { display: "flex", gap: 8 }, children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: e.alCancelar, children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", disabled: !rd || ae, onClick: () => e.tipo === "factura" ? qt(!0) : So(), children: e.tipo === "factura" ? "Emitir factura" : "Guardar" })
        ] })
      ] }),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-cabecera", children: [
        /* @__PURE__ */ a.jsxs("div", { className: "dx-cab-campos", children: [
          /* @__PURE__ */ a.jsx(No, { terceros: l, valor: g, alCambiar: w, etiqueta: "Cliente", deshabilitado: r || e.tipo === "pedido" && !!e.id && !1 }),
          /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", children: [
            e.tipo !== "presupuesto" && /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: e.tipo === "factura" ? "Fecha de emisión" : "Fecha del pedido" }),
              /* @__PURE__ */ a.jsx("input", { type: "date", value: z, onChange: (I) => p(I.target.value) })
            ] }),
            e.tipo === "presupuesto" && /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Validez" }),
              /* @__PURE__ */ a.jsx("select", { value: A, onChange: (I) => De(Number(I.target.value)), children: [15, 30, 60, 90].map((I) => /* @__PURE__ */ a.jsxs("option", { value: I, children: [
                I,
                " días"
              ] }, I)) })
            ] }),
            e.tipo === "factura" && j.length > 0 && /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Serie" }),
              /* @__PURE__ */ a.jsxs("select", { value: f, onChange: (I) => h(I.target.value), children: [
                /* @__PURE__ */ a.jsx("option", { value: "", children: "La del cliente" }),
                j.map((I) => /* @__PURE__ */ a.jsx("option", { value: I, children: I }, I))
              ] })
            ] }),
            e.tipo === "factura" && !r && /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
              /* @__PURE__ */ a.jsxs("div", { children: [
                /* @__PURE__ */ a.jsx("label", { children: "Forma de pago" }),
                /* @__PURE__ */ a.jsxs("select", { value: C, onChange: (I) => T(I.target.value), children: [
                  /* @__PURE__ */ a.jsx("option", { value: "", children: "— Vencimiento a mano —" }),
                  u.map((I) => /* @__PURE__ */ a.jsx("option", { value: I.id, children: I.nombre }, I.id))
                ] })
              ] }),
              !C && /* @__PURE__ */ a.jsxs("div", { children: [
                /* @__PURE__ */ a.jsx("label", { children: "Vencimiento" }),
                /* @__PURE__ */ a.jsx("select", { value: F, onChange: (I) => R(Number(I.target.value)), children: [0, 15, 30, 45, 60, 90].map((I) => /* @__PURE__ */ a.jsx("option", { value: I, children: I ? `${I} días` : "Contado" }, I)) })
              ] })
            ] }),
            e.tipo === "factura" && /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Retención IRPF %" }),
              /* @__PURE__ */ a.jsx("input", { type: "number", step: "0.01", value: E ?? "", placeholder: String((ie == null ? void 0 : ie.porcentajeIrpfDefecto) ?? 0), onChange: (I) => _(I.target.value === "" ? null : Number(I.target.value)) })
            ] })
          ] }),
          e.tipo === "factura" && !r && /* @__PURE__ */ a.jsxs("label", { className: "dx-check", children: [
            /* @__PURE__ */ a.jsx("input", { type: "checkbox", checked: D, onChange: (I) => L(I.target.checked) }),
            " Recargo de equivalencia"
          ] }),
          r && /* @__PURE__ */ a.jsxs("div", { children: [
            /* @__PURE__ */ a.jsx("label", { children: "Motivo de la rectificación (obligatorio)" }),
            /* @__PURE__ */ a.jsx("input", { value: Se, onChange: (I) => Ke(I.target.value), placeholder: "Devolución, error en precio…" })
          ] })
        ] }),
        /* @__PURE__ */ a.jsx("div", { className: "dx-ficha", children: ie ? /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("strong", { children: ie.nombre }),
          /* @__PURE__ */ a.jsx("div", { className: "muted", children: [ie.nifFiscal, ie.poblacion, ie.provincia].filter(Boolean).join(" · ") }),
          ie.limiteRiesgo != null && /* @__PURE__ */ a.jsxs("div", { className: "muted", children: [
            "Límite de riesgo ",
            M(ie.limiteRiesgo)
          ] }),
          ie.tarifaId && /* @__PURE__ */ a.jsx("div", { className: "muted", children: "Con tarifa de precios propia" }),
          ie.recargoEquivalencia && /* @__PURE__ */ a.jsx("div", { className: "muted", children: "En recargo de equivalencia" }),
          (S == null ? void 0 : S.avisoRiesgo) && /* @__PURE__ */ a.jsxs("div", { className: "dx-aviso", children: [
            "⚠ ",
            S.avisoRiesgo
          ] })
        ] }) : /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Elige un cliente: se aplican su tarifa, su forma de pago, su recargo y sus conceptos." }) })
      ] })
    ] }),
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsx(Jc, { modo: "venta", lineas: q, alCambiar: he, calculos: V, ivas: o, catalogo: r ? [] : m, sugeridos: fe, alElegirArticulo: ce }),
      !r && m.length > 0 && /* @__PURE__ */ a.jsx("div", { style: { marginTop: 10 }, children: /* @__PURE__ */ a.jsx(wo, { catalogo: m, lista: y, alCambiar: (I) => x(I ?? []), documento: !0 }) })
    ] }),
    /* @__PURE__ */ a.jsxs("div", { className: "dx-pie", children: [
      /* @__PURE__ */ a.jsxs("div", { className: "dx-estado", children: [
        ye && /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Calculando…" }),
        !ye && B && /* @__PURE__ */ a.jsx("span", { className: "dx-rojo", children: B }),
        (S == null ? void 0 : S.mencionFiscal) && /* @__PURE__ */ a.jsx("div", { className: "muted", style: { fontSize: 12 }, children: S.mencionFiscal })
      ] }),
      /* @__PURE__ */ a.jsxs("div", { className: "panel dx-totales", children: [
        K.map(([I, U]) => /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsxs("span", { className: "muted", children: [
            td(I),
            " · base ",
            ge(U.base)
          ] }),
          /* @__PURE__ */ a.jsx("span", { children: M(U.cuota) })
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
            ge(S.porcentajeIrpf),
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
        S && vt > 0 && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Coste · margen" }),
          /* @__PURE__ */ a.jsxs("span", { className: ra < 0 ? "dx-rojo" : "muted", children: [
            M(vt),
            " · ",
            M(ra),
            " (",
            ge(S.baseImponible ? ra / S.baseImponible * 100 : 0),
            " %)"
          ] })
        ] })
      ] })
    ] }),
    It && S && /* @__PURE__ */ a.jsx(
      un,
      {
        titulo: r ? "Emitir la rectificativa" : "Emitir la factura",
        alCerrar: () => qt(!1),
        acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => qt(!1), children: "Revisar" }),
          /* @__PURE__ */ a.jsxs("button", { className: "btn small", disabled: ae, onClick: So, children: [
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
          /* @__PURE__ */ a.jsx("strong", { children: ie == null ? void 0 : ie.nombre }),
          " con fecha ",
          z.split("-").reverse().join("/"),
          ". Una factura emitida no se modifica: queda numerada y encadenada en VeriFactu; para corregirla hay que rectificarla o anularla."
        ] })
      }
    )
  ] });
}
function Wp(e) {
  var Ke, q, he;
  const { api: t, anfitrion: n } = ht(), [r, l] = N.useState([]), [i, o] = N.useState([]), [s, u] = N.useState(((Ke = e.semilla) == null ? void 0 : Ke.proveedorId) ?? ""), [d, j] = N.useState(((q = e.semilla) == null ? void 0 : q.fecha) ?? mt()), [c, m] = N.useState([cr()]), [v, g] = N.useState([]), [w, z] = N.useState(null), [p, f] = N.useState(""), [h, C] = N.useState(!1), T = Zl();
  N.useEffect(() => {
    t.get("/proveedores").then(l).catch(() => l([])), t.get("/conceptos-linea?ambito=Compras&activos=true").then(o).catch(() => o([]));
  }, [t]), N.useEffect(() => {
    const y = e.semilla;
    if (!y) return;
    const x = y.lineas.map((Q) => ({ ...Q, porcentajeDescuento: 0, codigoIva: "" })), { porLinea: S, documento: $ } = ed(x), B = y.lineas.map((Q, ye) => ({ clave: Dr(), productoId: Q.productoId ?? null, descripcion: Q.descripcion, cantidad: Q.cantidad, precio: Q.precioUnitario, dto: 0, iva: null, conceptos: S[ye] }));
    m(B), g($), Promise.all(B.map((Q) => Q.productoId ? t.get(`/productos/${Q.productoId}`).catch(() => null) : Promise.resolve(null))).then(
      (Q) => m((ye) => ye.map((b, ae) => Q[ae] ? { ...b, referencia: Q[ae].referencia ?? Q[ae].nombre, unidad: Q[ae].unidadCompra || Q[ae].unidad, stock: Q[ae].stock, controlarStock: Q[ae].controlarStock } : b))
    );
  }, [e.semilla, t]);
  const F = r.find((y) => y.id === s), R = N.useMemo(() => c.map((y, x) => ({ l: y, i: x })).filter(({ l: y }) => y.descripcion.trim() && y.cantidad > 0), [c]), D = N.useMemo(
    () => {
      var y, x;
      return {
        proveedorId: s || null,
        proveedorTexto: (F == null ? void 0 : F.nombre) ?? (((y = e.semilla) == null ? void 0 : y.proveedorTexto) || null),
        solicitudOrigenId: e.id ? null : ((x = e.semilla) == null ? void 0 : x.solicitudOrigenId) ?? null,
        fecha: d,
        conceptosDocumento: v,
        lineas: R.map(({ l: S }) => ({ descripcion: S.descripcion.trim(), cantidad: S.cantidad, precioUnitario: S.precio ?? 0, productoId: S.productoId, ...S.conceptos === void 0 ? {} : { conceptos: S.conceptos } }))
      };
    },
    [s, F, d, v, R, e.id, e.semilla]
  ), L = Lr(D, 350);
  N.useEffect(() => {
    if (!L.proveedorId || L.lineas.length === 0) {
      z(null), f(L.proveedorId ? "" : "Elige el proveedor.");
      return;
    }
    const y = T();
    t.post("/compras/pedidos/simular", L).then((x) => y() && (z(x), f(""))).catch((x) => y() && (z(null), f(x.message)));
  }, [L, t]);
  const E = N.useMemo(() => {
    const y = c.map(() => {
    });
    return R.forEach(({ i: x }, S) => {
      const $ = w == null ? void 0 : w.lineas[S];
      $ && (y[x] = { precio: $.precioUnitario, importe: $.importe, costeUnitarioEntrada: $.costeUnitarioEntrada, conceptos: $.conceptos });
    }), y;
  }, [w, c, R]), _ = N.useMemo(() => {
    const y = {};
    return c.forEach((x, S) => {
      var $;
      return y[x.clave] = ((($ = E[S]) == null ? void 0 : $.conceptos) ?? []).filter((B) => !B.repartido).map((B) => ({ conceptoId: B.conceptoId, valor: B.valor }));
    }), y;
  }, [c, E]);
  function A(y, x) {
    const S = x.precioCompraPorUnidadCompra ?? x.precioCompra;
    m(($) => $.map((B) => B.clave === y ? { ...B, productoId: x.id, referencia: x.referencia ?? x.nombre, descripcion: x.nombre, precio: S, conceptos: void 0, unidad: x.unidadCompra || x.unidad, stock: x.stock, controlarStock: x.controlarStock } : B));
  }
  async function De() {
    C(!0);
    try {
      const y = e.id ? await t.put(`/compras/pedidos/${e.id}`, D) : await t.post("/compras/pedidos", D);
      n.aviso("Pedido de compra guardado.", "ok"), e.alGuardar(y.id);
    } catch (y) {
      n.aviso(y.message, "err");
    } finally {
      C(!1);
    }
  }
  const Se = ((w == null ? void 0 : w.lineas) ?? []).reduce((y, x) => y + x.costeConceptos, 0);
  return /* @__PURE__ */ a.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsxs("div", { className: "panel-head", children: [
        /* @__PURE__ */ a.jsx("h2", { children: e.id ? `Editar pedido ${((he = e.semilla) == null ? void 0 : he.numeroCompleto) ?? ""}` : "Nuevo pedido de compra" }),
        /* @__PURE__ */ a.jsxs("div", { style: { display: "flex", gap: 8 }, children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: e.alCancelar, children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", disabled: !w || h, onClick: De, children: "Guardar" })
        ] })
      ] }),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-cabecera", children: [
        /* @__PURE__ */ a.jsxs("div", { className: "dx-cab-campos", children: [
          /* @__PURE__ */ a.jsx(No, { terceros: r, valor: s, alCambiar: u, etiqueta: "Proveedor", deshabilitado: !!e.id }),
          /* @__PURE__ */ a.jsx("div", { className: "dx-fila", children: /* @__PURE__ */ a.jsxs("div", { children: [
            /* @__PURE__ */ a.jsx("label", { children: "Fecha del pedido" }),
            /* @__PURE__ */ a.jsx("input", { type: "date", value: d, onChange: (y) => j(y.target.value) })
          ] }) })
        ] }),
        /* @__PURE__ */ a.jsx("div", { className: "dx-ficha", children: F ? /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("strong", { children: F.nombre }),
          /* @__PURE__ */ a.jsx("div", { className: "muted", children: [F.nifFiscal, F.poblacion, F.pais].filter(Boolean).join(" · ") })
        ] }) : /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Elige un proveedor: se aplican sus conceptos (pronto pago, portes…)." }) })
      ] })
    ] }),
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsx(Jc, { modo: "compra", lineas: c, alCambiar: m, calculos: E, ivas: [], catalogo: i, sugeridos: _, alElegirArticulo: A }),
      i.length > 0 && /* @__PURE__ */ a.jsx("div", { style: { marginTop: 10 }, children: /* @__PURE__ */ a.jsx(wo, { catalogo: i, lista: v, alCambiar: (y) => g(y ?? []), documento: !0 }) })
    ] }),
    /* @__PURE__ */ a.jsxs("div", { className: "dx-pie", children: [
      /* @__PURE__ */ a.jsx("div", { className: "dx-estado", children: p && /* @__PURE__ */ a.jsx("span", { className: "dx-rojo", children: p }) }),
      /* @__PURE__ */ a.jsxs("div", { className: "panel dx-totales", children: [
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ a.jsx("span", { children: "Total del pedido (sin impuestos)" }),
          /* @__PURE__ */ a.jsx("span", { children: M(w == null ? void 0 : w.total) })
        ] }),
        Se !== 0 && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Costes añadidos al almacén" }),
          /* @__PURE__ */ a.jsx("span", { children: M(Se) })
        ] }),
        /* @__PURE__ */ a.jsx("div", { className: "muted", style: { fontSize: 12, marginTop: 6 }, children: "El impuesto se aplica al facturar el pedido." })
      ] })
    ] })
  ] });
}
function ea(e) {
  return /* @__PURE__ */ a.jsxs("div", { className: "panel-head", children: [
    /* @__PURE__ */ a.jsxs("h2", { style: { display: "flex", alignItems: "center", gap: 10 }, children: [
      /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: e.volver, title: "Volver a la lista", children: "←" }),
      e.titulo,
      e.estado && /* @__PURE__ */ a.jsx("span", { className: jo(e.estado), children: e.estado })
    ] }),
    /* @__PURE__ */ a.jsx("div", { className: "dx-acciones", children: e.acciones })
  ] });
}
function Ie(e) {
  return /* @__PURE__ */ a.jsxs("div", { className: "dx-dato", children: [
    /* @__PURE__ */ a.jsx("small", { children: e.etiqueta }),
    /* @__PURE__ */ a.jsx("div", { children: e.children })
  ] });
}
function Pr(e) {
  return /* @__PURE__ */ a.jsx("button", { type: "button", className: "dx-enlace", onClick: e.alPulsar, children: e.children });
}
function ta(e) {
  const [t, n] = N.useState(null), [r, l] = N.useState(""), i = N.useCallback(() => {
    e().then(n).catch((o) => l(o.message));
  }, []);
  return N.useEffect(i, [i]), { dato: t, error: r, recargar: i };
}
async function at(e, t, n) {
  try {
    return await e(), n && t(n, "ok"), !0;
  } catch (r) {
    return t(r.message, "err"), !1;
  }
}
function Qp(e) {
  const { api: t, anfitrion: n, navegar: r } = ht(), { dato: l, error: i, recargar: o } = ta(() => t.get(`/facturas/${e.id}`)), [s, u] = N.useState(null), [d, j] = N.useState(!1), [c, m] = N.useState("");
  if (N.useEffect(() => void t.get(`/facturas/${e.id}/saldo`).then(u).catch(() => u(null)), [t, e.id, l]), i) return /* @__PURE__ */ a.jsx("div", { className: "panel", children: /* @__PURE__ */ a.jsx("p", { className: "dx-rojo", children: i }) });
  if (!l) return /* @__PURE__ */ a.jsx("div", { className: "muted", children: "Cargando…" });
  const v = { clienteId: l.clienteId ?? void 0, lineas: l.lineas }, g = l.lineas.reduce((z, p) => z + (p.base - p.margen), 0), w = l.estado === "Emitida";
  return /* @__PURE__ */ a.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsx(
        ea,
        {
          titulo: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            l.tipo === "Rectificativa" ? "Rectificativa" : l.tipo === "Simplificada" ? "Ticket" : "Factura",
            " ",
            /* @__PURE__ */ a.jsx("span", { className: "mono", children: l.numeroCompleto })
          ] }),
          estado: l.estado,
          volver: () => r({ tipo: "factura", pantalla: "lista" }),
          acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => ki(n, `/facturas/${l.id}/pdf`).catch((z) => n.aviso(z.message, "err")), children: "PDF" }),
            l.tipo !== "Simplificada" && l.clienteNif && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => ki(n, `/facturas/${l.id}/facturae.xml`).catch((z) => n.aviso(z.message, "err")), children: "Facturae" }),
            /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "factura", pantalla: "editor", semilla: v }), children: "Duplicar" }),
            w && l.tipo === "Ordinaria" && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "factura", pantalla: "editor", semilla: { ...v, rectificaId: l.id, rectificaNumero: l.numeroCompleto } }), children: "Rectificar" }),
            w && /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", onClick: () => j(!0), children: "Anular" })
          ] })
        }
      ),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ a.jsxs(Ie, { etiqueta: "Cliente", children: [
          /* @__PURE__ */ a.jsx("strong", { children: l.clienteNombre }),
          l.clienteNif && /* @__PURE__ */ a.jsx("div", { className: "muted mono", children: l.clienteNif }),
          /* @__PURE__ */ a.jsx("div", { className: "muted", children: [l.clienteCalle, l.clienteCodigoPostal, l.clientePoblacion].filter(Boolean).join(" ") })
        ] }),
        /* @__PURE__ */ a.jsxs(Ie, { etiqueta: "Emisión", children: [
          Ue(l.fechaEmision),
          l.fechaOperacion !== l.fechaEmision && /* @__PURE__ */ a.jsxs("div", { className: "muted", children: [
            "Operación ",
            Ue(l.fechaOperacion)
          ] })
        ] }),
        /* @__PURE__ */ a.jsx(Ie, { etiqueta: "Vencimiento", children: Ue(l.fechaVencimiento) }),
        /* @__PURE__ */ a.jsxs(Ie, { etiqueta: "Cobro", children: [
          s ? s.pendiente <= 0 ? /* @__PURE__ */ a.jsx("span", { className: "pill ok", children: "Cobrada" }) : /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            "Pendiente ",
            /* @__PURE__ */ a.jsx("strong", { children: M(s.pendiente) }),
            s.liquidado > 0 && /* @__PURE__ */ a.jsxs("div", { className: "muted", children: [
              "Cobrado ",
              M(s.liquidado)
            ] })
          ] }) : "—",
          s && s.pendiente > 0 && w && n.irA && /* @__PURE__ */ a.jsx("div", { children: /* @__PURE__ */ a.jsx(Pr, { alPulsar: () => n.irA("cobros"), children: "Registrar cobro" }) })
        ] })
      ] }),
      l.motivoRectificacion && /* @__PURE__ */ a.jsxs("p", { className: "muted", children: [
        "Rectifica: ",
        l.motivoRectificacion,
        l.rectificaFacturaId && /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          " · ",
          /* @__PURE__ */ a.jsx(Pr, { alPulsar: () => r({ tipo: "factura", pantalla: "vista", id: l.rectificaFacturaId }), children: "ver la factura original" })
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
        /* @__PURE__ */ a.jsx("tbody", { children: l.lineas.map((z, p) => /* @__PURE__ */ a.jsxs("tr", { children: [
          /* @__PURE__ */ a.jsxs("td", { children: [
            z.descripcion,
            /* @__PURE__ */ a.jsx(Jl, { conceptos: z.conceptos })
          ] }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: Fe(z.cantidad) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: M(z.precioUnitario) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: z.porcentajeDescuento ? `${ge(z.porcentajeDescuento)} %` : "" }),
          /* @__PURE__ */ a.jsxs("td", { children: [
            z.codigoIva,
            " · ",
            ge(z.porcentajeIva),
            " %"
          ] }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: /* @__PURE__ */ a.jsx("strong", { children: M(z.base) }) }),
          /* @__PURE__ */ a.jsx("td", { className: "num muted", children: z.costeUnitario || z.costeConceptos ? M(z.margen) : "" })
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
            ge(l.porcentajeIrpf),
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
            ge(l.baseImponible ? (l.baseImponible - g) / l.baseImponible * 100 : 0),
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
      un,
      {
        titulo: `Anular ${l.numeroCompleto}`,
        alCerrar: () => j(!1),
        acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => j(!1), children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", disabled: !c.trim(), onClick: async () => await at(() => t.post(`/facturas/${l.id}/anular`, { motivo: c }), n.aviso, "Factura anulada.") && (j(!1), o()), children: "Anular" })
        ] }),
        children: [
          /* @__PURE__ */ a.jsx("p", { className: "muted", style: { marginTop: 0 }, children: "La factura no se borra: queda anulada con un registro de anulación VeriFactu. Si hay que corregir importes, rectifícala en lugar de anularla." }),
          /* @__PURE__ */ a.jsx("label", { children: "Motivo" }),
          /* @__PURE__ */ a.jsx("input", { value: c, onChange: (z) => m(z.target.value), autoFocus: !0 })
        ]
      }
    )
  ] });
}
function Gp(e) {
  const { api: t, anfitrion: n, navegar: r } = ht(), { dato: l, error: i, recargar: o } = ta(() => t.get(`/presupuestos/${e.id}`));
  if (i) return /* @__PURE__ */ a.jsx("div", { className: "panel", children: /* @__PURE__ */ a.jsx("p", { className: "dx-rojo", children: i }) });
  if (!l) return /* @__PURE__ */ a.jsx("div", { className: "muted", children: "Cargando…" });
  const s = l.estado === "Borrador", u = { clienteId: l.clienteId, lineas: l.lineas };
  return /* @__PURE__ */ a.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsx(
        ea,
        {
          titulo: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            "Presupuesto ",
            /* @__PURE__ */ a.jsx("span", { className: "mono", children: l.numeroCompleto })
          ] }),
          estado: l.estado,
          volver: () => r({ tipo: "presupuesto", pantalla: "lista" }),
          acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => ki(n, `/presupuestos/${l.id}/pdf`).catch((d) => n.aviso(d.message, "err")), children: "PDF" }),
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
            s && /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", onClick: async () => await at(() => t.post(`/presupuestos/${l.id}/rechazar`, {}), n.aviso, "Presupuesto rechazado.") && o(), children: "Rechazar" })
          ] })
        }
      ),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ a.jsx(Ie, { etiqueta: "Cliente", children: /* @__PURE__ */ a.jsx("strong", { children: l.clienteNombre }) }),
        /* @__PURE__ */ a.jsx(Ie, { etiqueta: "Fecha", children: Ue(l.fecha) }),
        /* @__PURE__ */ a.jsx(Ie, { etiqueta: "Válido hasta", children: Ue(l.validez) }),
        /* @__PURE__ */ a.jsx(Ie, { etiqueta: "Factura", children: l.facturaId ? /* @__PURE__ */ a.jsx(Pr, { alPulsar: () => r({ tipo: "factura", pantalla: "vista", id: l.facturaId }), children: "ver la factura" }) : "—" })
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
        /* @__PURE__ */ a.jsx("tbody", { children: l.lineas.map((d, j) => /* @__PURE__ */ a.jsxs("tr", { children: [
          /* @__PURE__ */ a.jsxs("td", { children: [
            d.descripcion,
            /* @__PURE__ */ a.jsx(Jl, { conceptos: d.conceptos })
          ] }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: Fe(d.cantidad) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: M(d.precioUnitario) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: d.porcentajeDescuento ? `${ge(d.porcentajeDescuento)} %` : "" }),
          /* @__PURE__ */ a.jsx("td", { children: d.codigoIva }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: /* @__PURE__ */ a.jsx("strong", { children: M(d.base) }) })
        ] }, j)) })
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
  const { api: t, anfitrion: n, navegar: r } = ht(), { dato: l, error: i, recargar: o } = ta(() => t.get(`/pedidos-venta/${e.id}`)), [s, u] = N.useState([]), [d, j] = N.useState([]), [c, m] = N.useState(null), [v, g] = N.useState(""), [w, z] = N.useState(mt()), [p, f] = N.useState(!1), [h, C] = N.useState(mt()), [T, F] = N.useState("");
  if (N.useEffect(() => void t.get(`/pedidos-venta/${e.id}/albaranes`).then(u).catch(() => u([])), [t, e.id, l]), N.useEffect(() => void t.get("/formas-pago").then((_) => j(_.filter((A) => A.activo))).catch(() => j([])), [t]), i) return /* @__PURE__ */ a.jsx("div", { className: "panel", children: /* @__PURE__ */ a.jsx("p", { className: "dx-rojo", children: i }) });
  if (!l) return /* @__PURE__ */ a.jsx("div", { className: "muted", children: "Cargando…" });
  const R = (l.estado === "Borrador" || l.estado === "Confirmado") && l.lineas.every((_) => _.cantidadServida === 0), D = l.lineas.some((_) => _.pendienteServir > 0), L = l.estado !== "Cancelado" && l.estado !== "Facturado", E = { clienteId: l.clienteId, fecha: l.fecha, lineas: l.lineas };
  return /* @__PURE__ */ a.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsx(
        ea,
        {
          titulo: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            "Pedido de venta ",
            /* @__PURE__ */ a.jsx("span", { className: "mono", children: l.numeroCompleto })
          ] }),
          estado: l.estado,
          volver: () => r({ tipo: "pedido", pantalla: "lista" }),
          acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "pedido", pantalla: "editor", semilla: { ...E, fecha: void 0 } }), children: "Duplicar" }),
            R && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "pedido", pantalla: "editor", id: l.id, semilla: E }), children: "Editar" }),
            l.estado === "Borrador" && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: async () => await at(() => t.post(`/pedidos-venta/${l.id}/confirmar`), n.aviso, "Pedido confirmado.") && o(), children: "Confirmar" }),
            L && l.estado !== "Borrador" && D && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => m(Object.fromEntries(l.lineas.map((_) => [_.id, _.pendienteServir]))), children: "Entregar (albarán)" }),
            L && l.estado !== "Borrador" && /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: () => f(!0), children: "Facturar" }),
            L && /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", onClick: async () => window.confirm("¿Cancelar el pedido?") && await at(() => t.post(`/pedidos-venta/${l.id}/cancelar`), n.aviso, "Pedido cancelado.") && o(), children: "Cancelar" })
          ] })
        }
      ),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ a.jsx(Ie, { etiqueta: "Cliente", children: /* @__PURE__ */ a.jsx("strong", { children: l.clienteNombre }) }),
        /* @__PURE__ */ a.jsx(Ie, { etiqueta: "Fecha", children: Ue(l.fecha) }),
        /* @__PURE__ */ a.jsx(Ie, { etiqueta: "Viene de", children: l.presupuestoOrigenId ? /* @__PURE__ */ a.jsx(Pr, { alPulsar: () => r({ tipo: "presupuesto", pantalla: "vista", id: l.presupuestoOrigenId }), children: "presupuesto" }) : "—" }),
        /* @__PURE__ */ a.jsx(Ie, { etiqueta: "Factura", children: l.facturaId ? /* @__PURE__ */ a.jsx(Pr, { alPulsar: () => r({ tipo: "factura", pantalla: "vista", id: l.facturaId }), children: "ver la factura" }) : "—" })
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
            /* @__PURE__ */ a.jsx(Jl, { conceptos: _.conceptos })
          ] }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: Fe(_.cantidad) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: Fe(_.cantidadServida) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: _.pendienteServir > 0 ? /* @__PURE__ */ a.jsx("strong", { children: Fe(_.pendienteServir) }) : "—" }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: M(_.precioUnitario) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: _.porcentajeDescuento ? `${ge(_.porcentajeDescuento)} %` : "" }),
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
          /* @__PURE__ */ a.jsx("td", { children: Ue(_.fecha) }),
          /* @__PURE__ */ a.jsx("td", { className: "muted", children: _.referencia }),
          /* @__PURE__ */ a.jsx("td", { className: "muted", children: _.lineas.map((A) => `${Fe(A.cantidad)} × ${A.descripcion}`).join(" · ") }),
          /* @__PURE__ */ a.jsx("td", { className: "right", children: !_.anulado && l.estado !== "Facturado" && /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", onClick: async () => {
            const A = window.prompt("Motivo de la anulación del albarán:");
            A !== null && await at(() => t.post(`/pedidos-venta/${l.id}/albaranes/${_.id}/anular`, { motivo: A || null }), n.aviso, "Albarán anulado.") && o();
          }, children: "Anular" }) })
        ] }, _.id)) })
      ] }) : /* @__PURE__ */ a.jsx("p", { className: "muted", style: { margin: 0 }, children: "Sin entregas todavía." })
    ] }),
    c && /* @__PURE__ */ a.jsxs(
      un,
      {
        titulo: "Entrega (albarán de venta)",
        alCerrar: () => m(null),
        ancho: 640,
        acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => m(null), children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: async () => await at(() => t.post(`/pedidos-venta/${l.id}/entregar`, { fecha: w, referencia: v || null, lineas: Object.entries(c).filter(([, _]) => _ > 0).map(([_, A]) => ({ lineaPedidoId: _, cantidad: A })) }), n.aviso, "Albarán creado.") && (m(null), o()), children: "Crear albarán" })
        ] }),
        children: [
          /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Fecha" }),
              /* @__PURE__ */ a.jsx("input", { type: "date", value: w, onChange: (_) => z(_.target.value) })
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
              /* @__PURE__ */ a.jsx("td", { className: "num", children: Fe(_.pendienteServir) }),
              /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx("input", { className: "num", type: "number", step: "0.001", value: c[_.id] ?? 0, onChange: (A) => m({ ...c, [_.id]: Number(A.target.value) }) }) })
            ] }, _.id)) })
          ] })
        ]
      }
    ),
    p && /* @__PURE__ */ a.jsxs(
      un,
      {
        titulo: `Facturar el pedido ${l.numeroCompleto}`,
        alCerrar: () => f(!1),
        acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => f(!1), children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: async () => {
            try {
              const _ = await t.post(`/pedidos-venta/${l.id}/facturar`, { fechaEmision: h, formaPagoId: T || null });
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
              /* @__PURE__ */ a.jsx("input", { type: "date", value: h, onChange: (_) => C(_.target.value) })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Forma de pago" }),
              /* @__PURE__ */ a.jsxs("select", { value: T, onChange: (_) => F(_.target.value), children: [
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
  const { api: t, anfitrion: n, navegar: r } = ht(), { dato: l, error: i, recargar: o } = ta(() => t.get(`/compras/pedidos/${e.id}`)), [s, u] = N.useState([]), [d, j] = N.useState([]), [c, m] = N.useState([]), [v, g] = N.useState(null), [w, z] = N.useState(""), [p, f] = N.useState(""), [h, C] = N.useState(mt()), [T, F] = N.useState(!1), [R, D] = N.useState("IVA21"), [L, E] = N.useState(0), [_, A] = N.useState(""), [De, Se] = N.useState(mt());
  if (N.useEffect(() => void t.get(`/compras/pedidos/${e.id}/albaranes`).then(u).catch(() => u([])), [t, e.id, l]), N.useEffect(() => {
    t.get("/inventario/almacenes").then((x) => (j(x), x[0] && z(x[0].id))).catch(() => j([])), t.get("/tipos-iva").then((x) => m(x.filter((S) => S.activo))).catch(() => m([]));
  }, [t]), i) return /* @__PURE__ */ a.jsx("div", { className: "panel", children: /* @__PURE__ */ a.jsx("p", { className: "dx-rojo", children: i }) });
  if (!l) return /* @__PURE__ */ a.jsx("div", { className: "muted", children: "Cargando…" });
  const Ke = (l.estado === "Borrador" || l.estado === "Confirmado") && l.lineas.every((x) => x.cantidadRecibida === 0 && x.cantidadFacturada === 0) && !l.empresaOrigenId, q = l.estado !== "Cancelado" && l.estado !== "Facturado", he = l.lineas.some((x) => x.pendienteRecibir > 0), y = l.lineas.reduce((x, S) => x + S.costeConceptos, 0);
  return /* @__PURE__ */ a.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsx(
        ea,
        {
          titulo: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            "Pedido de compra ",
            /* @__PURE__ */ a.jsx("span", { className: "mono", children: l.numeroCompleto })
          ] }),
          estado: l.estado,
          volver: () => r({ tipo: "compra", pantalla: "lista" }),
          acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            !l.empresaOrigenId && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "compra", pantalla: "editor", semilla: { ...l, fecha: mt() } }), children: "Duplicar" }),
            Ke && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "compra", pantalla: "editor", id: l.id, semilla: l }), children: "Editar" }),
            l.estado === "Borrador" && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: async () => await at(() => t.post(`/compras/pedidos/${l.id}/confirmar`), n.aviso, "Pedido confirmado.") && o(), children: "Confirmar" }),
            q && l.estado !== "Borrador" && he && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => g(Object.fromEntries(l.lineas.map((x) => [x.id, { cantidad: x.pendienteRecibir, lote: "" }]))), children: "Recibir mercancía" }),
            q && l.estado !== "Borrador" && /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: () => F(!0), children: "Facturar" }),
            q && !l.empresaOrigenId && /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", onClick: async () => window.confirm("¿Cancelar el pedido?") && await at(() => t.post(`/compras/pedidos/${l.id}/cancelar`), n.aviso, "Pedido cancelado.") && o(), children: "Cancelar" })
          ] })
        }
      ),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ a.jsx(Ie, { etiqueta: "Proveedor", children: /* @__PURE__ */ a.jsx("strong", { children: l.proveedorTexto }) }),
        /* @__PURE__ */ a.jsx(Ie, { etiqueta: "Fecha", children: Ue(l.fecha) }),
        /* @__PURE__ */ a.jsx(Ie, { etiqueta: "Total", children: M(l.total) }),
        /* @__PURE__ */ a.jsx(Ie, { etiqueta: "Costes añadidos", children: y ? M(y) : "—" })
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
          /* @__PURE__ */ a.jsx(Jl, { conceptos: x.conceptos })
        ] }),
        /* @__PURE__ */ a.jsx("td", { className: "num", children: Fe(x.cantidad) }),
        /* @__PURE__ */ a.jsx("td", { className: "num", children: Fe(x.cantidadRecibida) }),
        /* @__PURE__ */ a.jsx("td", { className: "num", children: x.pendienteRecibir > 0 ? /* @__PURE__ */ a.jsx("strong", { children: Fe(x.pendienteRecibir) }) : "—" }),
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
            /* @__PURE__ */ a.jsx("td", { children: Ue(x.fecha) }),
            /* @__PURE__ */ a.jsx("td", { className: "muted", children: x.referencia }),
            /* @__PURE__ */ a.jsx("td", { className: "muted", children: ((S = d.find(($) => $.id === x.almacenId)) == null ? void 0 : S.nombre) ?? "—" }),
            /* @__PURE__ */ a.jsx("td", { className: "muted", children: x.lineas.map(($) => `${Fe($.cantidad)} × ${$.descripcion}`).join(" · ") }),
            /* @__PURE__ */ a.jsx("td", { className: "right", children: !x.anulado && l.estado !== "Facturado" && /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", onClick: async () => {
              const $ = window.prompt("Motivo de la anulación del albarán:");
              $ !== null && await at(() => t.post(`/compras/pedidos/${l.id}/albaranes/${x.id}/anular`, { motivo: $ || null }), n.aviso, "Albarán anulado.") && o();
            }, children: "Anular" }) })
          ] }, x.id);
        }) })
      ] }) : /* @__PURE__ */ a.jsx("p", { className: "muted", style: { margin: 0 }, children: "Sin recepciones todavía." })
    ] }),
    v && /* @__PURE__ */ a.jsxs(
      un,
      {
        titulo: "Recepción de mercancía",
        alCerrar: () => g(null),
        ancho: 680,
        acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => g(null), children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: async () => await at(() => t.post(`/compras/pedidos/${l.id}/recibir`, { fecha: h, referencia: p || null, almacenId: w || null, lineas: Object.entries(v).filter(([, x]) => x.cantidad > 0).map(([x, S]) => ({ lineaPedidoId: x, cantidad: S.cantidad, lote: S.lote || null })) }), n.aviso, "Recepción registrada.") && (g(null), o()), children: "Registrar recepción" })
        ] }),
        children: [
          /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Fecha" }),
              /* @__PURE__ */ a.jsx("input", { type: "date", value: h, onChange: (x) => C(x.target.value) })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Albarán del proveedor" }),
              /* @__PURE__ */ a.jsx("input", { value: p, onChange: (x) => f(x.target.value) })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Almacén" }),
              /* @__PURE__ */ a.jsxs("select", { value: w, onChange: (x) => z(x.target.value), children: [
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
              var S, $;
              return /* @__PURE__ */ a.jsxs("tr", { children: [
                /* @__PURE__ */ a.jsx("td", { children: x.descripcion }),
                /* @__PURE__ */ a.jsx("td", { className: "num", children: Fe(x.pendienteRecibir) }),
                /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx("input", { className: "num", type: "number", step: "0.001", value: ((S = v[x.id]) == null ? void 0 : S.cantidad) ?? 0, onChange: (B) => g({ ...v, [x.id]: { ...v[x.id], cantidad: Number(B.target.value) } }) }) }),
                /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx("input", { value: (($ = v[x.id]) == null ? void 0 : $.lote) ?? "", onChange: (B) => g({ ...v, [x.id]: { ...v[x.id], lote: B.target.value } }) }) })
              ] }, x.id);
            }) })
          ] })
        ]
      }
    ),
    T && /* @__PURE__ */ a.jsxs(
      un,
      {
        titulo: `Facturar el pedido ${l.numeroCompleto}`,
        alCerrar: () => F(!1),
        acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => F(!1), children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: async () => await at(() => t.post(`/compras/pedidos/${l.id}/facturar`, { codigoIva: R, porcentajeIrpf: L, numeroFactura: _ || null, fechaFactura: De }), n.aviso, "Factura del proveedor registrada como gasto.") && (F(!1), o()), children: "Registrar factura" })
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
              /* @__PURE__ */ a.jsx("input", { value: _, onChange: (x) => A(x.target.value), autoFocus: !0 })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Fecha de la factura" }),
              /* @__PURE__ */ a.jsx("input", { type: "date", value: De, onChange: (x) => Se(x.target.value) })
            ] })
          ] }),
          /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Impuesto" }),
              /* @__PURE__ */ a.jsx("select", { value: R, onChange: (x) => D(x.target.value), children: c.map((x) => /* @__PURE__ */ a.jsx("option", { value: x.codigo, children: x.nombre }, x.codigo)) })
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
const za = (e = "") => ({ clave: Dr(), descripcion: "", cuentaGasto: "", base: 0, codigoIva: e, porcentajeIva: null, porcentajeDeducible: 100 }), Yp = (e) => e === "InversionSujetoPasivo" || e === "Intracomunitario";
function Xp(e) {
  const { api: t, anfitrion: n } = ht(), r = e.semilla, [l, i] = N.useState([]), [o, s] = N.useState([]), [u, d] = N.useState([]), [j, c] = N.useState([]), [m, v] = N.useState((r == null ? void 0 : r.proveedorId) ?? ""), [g, w] = N.useState(e.id ? (r == null ? void 0 : r.numeroFactura) ?? "" : ""), [z, p] = N.useState((r == null ? void 0 : r.fechaFactura) ?? mt()), [f, h] = N.useState(e.id ? (r == null ? void 0 : r.fecha) ?? mt() : mt()), [C, T] = N.useState(r != null && r.concepto && !r.concepto.startsWith("Factura ") ? r.concepto : ""), [F, R] = N.useState((r == null ? void 0 : r.porcentajeIrpf) ?? 0), [D, L] = N.useState(""), [E, _] = N.useState(((r == null ? void 0 : r.recargoTotal) ?? 0) > 0), [A, De] = N.useState((r == null ? void 0 : r.afectacion) ?? "Comun"), [Se, Ke] = N.useState(
    () => {
      var k;
      return (k = r == null ? void 0 : r.lineas) != null && k.length ? r.lineas.map((V) => ({ clave: Dr(), descripcion: V.descripcion ?? "", cuentaGasto: V.cuentaGasto ?? "", base: V.base, codigoIva: V.codigoIva, porcentajeIva: V.autoliquidada ? V.porcentajeIva : null, porcentajeDeducible: V.porcentajeDeducible })) : [za()];
    }
  ), [q, he] = N.useState(e.id && (r != null && r.vencimientos) && r.vencimientos.length > 1 ? r.vencimientos : null), [y, x] = N.useState(null), [S, $] = N.useState(""), [B, Q] = N.useState(!1), ye = Zl();
  N.useEffect(() => {
    t.get("/proveedores").then(i).catch(() => i([])), t.get("/tipos-iva").then((k) => s(k.filter((V) => V.activo))).catch(() => s([])), t.get("/formas-pago").then((k) => d(k.filter((V) => V.activo))).catch(() => d([])), t.get("/contabilidad/cuentas").then((k) => c(k.filter((V) => V.codigo.startsWith("6") || V.codigo.startsWith("2")))).catch(() => c([]));
  }, [t]);
  const b = l.find((k) => k.id === m);
  N.useEffect(() => {
    b != null && b.formaPagoDefectoId && !D && L(b.formaPagoDefectoId);
  }, [b]);
  const ae = N.useMemo(
    () => ({
      proveedorId: m || null,
      proveedorTexto: (b == null ? void 0 : b.nombre) ?? null,
      numeroFactura: g.trim() || null,
      fechaFactura: z || null,
      fecha: f,
      concepto: C.trim() || null,
      porcentajeIrpf: F,
      formaPagoId: D || null,
      recargoEquivalencia: E,
      afectacion: A,
      baseImponible: 0,
      lineas: Se.filter((k) => k.base !== 0).map((k) => ({
        base: k.base,
        codigoIva: k.codigoIva || null,
        descripcion: k.descripcion.trim() || null,
        porcentajeIva: k.porcentajeIva,
        porcentajeDeducible: k.porcentajeDeducible,
        cuentaGasto: k.cuentaGasto.trim() || null
      })),
      vencimientos: q
    }),
    [m, b, g, z, f, C, F, D, E, A, Se, q]
  ), et = Lr(ae, 350);
  N.useEffect(() => {
    if (!et.lineas.length) {
      x(null), $("Añade al menos una línea con base.");
      return;
    }
    const k = ye();
    t.post("/gastos/simular", et).then((V) => k() && (x(V), $(""))).catch((V) => k() && (x(null), $(V.message)));
  }, [et, t]);
  const It = (k, V) => Ke((fe) => fe.map((ce) => ce.clave === k ? { ...ce, ...V } : ce)), qt = (k) => o.find((V) => V.codigo === k), na = (k) => {
    var V;
    return (V = y == null ? void 0 : y.lineas) == null ? void 0 : V[Se.filter((fe) => fe.base !== 0).indexOf(k)];
  };
  function ie(k) {
    if (!y) return;
    const V = /* @__PURE__ */ new Date((z || f) + "T00:00:00"), fe = dl(y.total / k);
    he(Array.from({ length: k }, (ce, K) => {
      const vt = new Date(V);
      return vt.setMonth(vt.getMonth() + K + 1), { fecha: vt.toISOString().slice(0, 10), importe: K === k - 1 ? dl(y.total - fe * (k - 1)) : fe };
    }));
  }
  async function Yt() {
    Q(!0);
    try {
      const k = e.id ? await t.put(`/gastos/${e.id}`, ae) : await t.post("/gastos", ae);
      n.aviso(e.id ? "Factura corregida." : "Factura registrada.", "ok"), k.avisoRiesgo && n.aviso(k.avisoRiesgo, "err"), e.alGuardar(k.id);
    } catch (k) {
      n.aviso(k.message, "err");
    } finally {
      Q(!1);
    }
  }
  const Bn = dl((q ?? []).reduce((k, V) => k + (Number(V.importe) || 0), 0));
  return /* @__PURE__ */ a.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsxs("div", { className: "panel-head", children: [
        /* @__PURE__ */ a.jsx("h2", { children: e.id ? `Corregir factura ${(r == null ? void 0 : r.numeroFactura) ?? ""}` : "Nueva factura de proveedor" }),
        /* @__PURE__ */ a.jsxs("div", { style: { display: "flex", gap: 8 }, children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: e.alCancelar, children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", disabled: !y || B, onClick: Yt, children: e.id ? "Guardar corrección" : "Registrar factura" })
        ] })
      ] }),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-cabecera", children: [
        /* @__PURE__ */ a.jsxs("div", { className: "dx-cab-campos", children: [
          /* @__PURE__ */ a.jsx(No, { terceros: l, valor: m, alCambiar: v, etiqueta: "Proveedor" }),
          /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Nº de factura del proveedor" }),
              /* @__PURE__ */ a.jsx("input", { value: g, onChange: (k) => w(k.target.value), placeholder: "F-2026/0117" })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Fecha de la factura" }),
              /* @__PURE__ */ a.jsx("input", { type: "date", value: z, onChange: (k) => p(k.target.value) })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Fecha de registro" }),
              /* @__PURE__ */ a.jsx("input", { type: "date", value: f, onChange: (k) => h(k.target.value), title: "La del asiento y la del periodo de IVA en que se deduce" })
            ] })
          ] }),
          /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Forma de pago" }),
              /* @__PURE__ */ a.jsxs("select", { value: D, onChange: (k) => L(k.target.value), children: [
                /* @__PURE__ */ a.jsx("option", { value: "", children: "Contado (vence en la fecha de factura)" }),
                u.map((k) => /* @__PURE__ */ a.jsx("option", { value: k.id, children: k.nombre }, k.id))
              ] })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Retención IRPF %" }),
              /* @__PURE__ */ a.jsx("input", { type: "number", step: "0.01", value: F, onChange: (k) => R(Number(k.target.value)) })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Prorrata especial" }),
              /* @__PURE__ */ a.jsxs("select", { value: A, onChange: (k) => De(k.target.value), children: [
                /* @__PURE__ */ a.jsx("option", { value: "Comun", children: "Uso común" }),
                /* @__PURE__ */ a.jsx("option", { value: "ConDerecho", children: "Solo operaciones con derecho" }),
                /* @__PURE__ */ a.jsx("option", { value: "SinDerecho", children: "Solo operaciones exentas" })
              ] })
            ] })
          ] }),
          /* @__PURE__ */ a.jsx("div", { className: "dx-fila", children: /* @__PURE__ */ a.jsxs("div", { style: { gridColumn: "1 / -1" }, children: [
            /* @__PURE__ */ a.jsx("label", { children: "Concepto (vacío: «Factura nº»)" }),
            /* @__PURE__ */ a.jsx("input", { value: C, onChange: (k) => T(k.target.value) })
          ] }) }),
          /* @__PURE__ */ a.jsxs("label", { className: "dx-check", children: [
            /* @__PURE__ */ a.jsx("input", { type: "checkbox", checked: E, onChange: (k) => _(k.target.checked) }),
            " El proveedor me cobra recargo de equivalencia"
          ] })
        ] }),
        /* @__PURE__ */ a.jsx("div", { className: "dx-ficha", children: b ? /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("strong", { children: b.nombre }),
          /* @__PURE__ */ a.jsx("div", { className: "muted", children: [b.nifFiscal, b.poblacion, b.pais].filter(Boolean).join(" · ") }),
          !b.nifFiscal && /* @__PURE__ */ a.jsx("div", { className: "dx-aviso", children: "⚠ Sin NIF en la ficha: hace falta para el libro de IVA, el 347 y el SII." }),
          (y == null ? void 0 : y.avisoRiesgo) && /* @__PURE__ */ a.jsxs("div", { className: "dx-aviso", children: [
            "⚠ ",
            y.avisoRiesgo
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
        /* @__PURE__ */ a.jsx("tbody", { children: Se.map((k, V) => {
          const fe = qt(k.codigoIva), ce = na(k);
          return /* @__PURE__ */ a.jsxs("tr", { className: V % 2 ? "dx-par" : "", children: [
            /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx("input", { value: k.descripcion, onChange: (K) => It(k.clave, { descripcion: K.target.value }), placeholder: "Qué se compra" }) }),
            /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx("input", { list: "dx-cuentas-gasto", value: k.cuentaGasto, onChange: (K) => It(k.clave, { cuentaGasto: K.target.value }), placeholder: "la de la regla" }) }),
            /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx("input", { className: "num", type: "number", step: "0.01", value: k.base || "", onChange: (K) => It(k.clave, { base: Number(K.target.value) }) }) }),
            /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsxs("select", { value: k.codigoIva, onChange: (K) => It(k.clave, { codigoIva: K.target.value, porcentajeIva: null }), children: [
              /* @__PURE__ */ a.jsx("option", { value: "", children: "General" }),
              o.map((K) => /* @__PURE__ */ a.jsx("option", { value: K.codigo, children: K.nombre }, K.codigo))
            ] }) }),
            /* @__PURE__ */ a.jsx("td", { children: Yp(fe == null ? void 0 : fe.clase) ? /* @__PURE__ */ a.jsx("input", { className: "num", type: "number", step: "0.01", value: k.porcentajeIva ?? "", placeholder: "21", title: "Tipo que autoliquidas", onChange: (K) => It(k.clave, { porcentajeIva: K.target.value === "" ? null : Number(K.target.value) }) }) : /* @__PURE__ */ a.jsx("span", { className: "muted", children: ce ? `${ge(ce.porcentajeIva)} %` : "" }) }),
            /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx("input", { className: "num", type: "number", step: "1", min: 0, max: 100, value: k.porcentajeDeducible, onChange: (K) => It(k.clave, { porcentajeDeducible: Number(K.target.value) }) }) }),
            /* @__PURE__ */ a.jsx("td", { className: "num", children: ce ? /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
              /* @__PURE__ */ a.jsx("strong", { children: M(ce.cuota) }),
              ce.autoliquidada && /* @__PURE__ */ a.jsx("div", { className: "dx-sub", children: "autoliquidada" }),
              ce.cuotaRecargo !== 0 && /* @__PURE__ */ a.jsxs("div", { className: "dx-sub", children: [
                "+ recargo ",
                M(ce.cuotaRecargo)
              ] })
            ] }) : "—" }),
            /* @__PURE__ */ a.jsx("td", { className: "right", children: /* @__PURE__ */ a.jsx("button", { className: "dx-icono", title: "Quitar", onClick: () => Ke((K) => K.length > 1 ? K.filter((vt) => vt.clave !== k.clave) : [za()]), children: "✕" }) })
          ] }, k.clave);
        }) })
      ] }),
      /* @__PURE__ */ a.jsx("datalist", { id: "dx-cuentas-gasto", children: j.map((k) => /* @__PURE__ */ a.jsx("option", { value: k.codigo, children: k.nombre }, k.codigo)) }),
      /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", style: { marginTop: 8 }, onClick: () => Ke((k) => {
        var V;
        return [...k, za(((V = k[k.length - 1]) == null ? void 0 : V.codigoIva) ?? "")];
      }), children: "+ Añadir línea" }),
      /* @__PURE__ */ a.jsx("span", { className: "muted dx-ayuda", children: "Una línea por cada base e impuesto de la factura. En inversión del sujeto pasivo e intracomunitarias el IVA lo autoliquidas tú (no lo cobra el proveedor)." })
    ] }),
    /* @__PURE__ */ a.jsxs("div", { className: "dx-pie", children: [
      /* @__PURE__ */ a.jsxs("div", { className: "panel", style: { margin: 0 }, children: [
        /* @__PURE__ */ a.jsxs("div", { className: "panel-head", children: [
          /* @__PURE__ */ a.jsx("h2", { children: "Vencimientos" }),
          /* @__PURE__ */ a.jsxs("div", { className: "dx-acciones", children: [
            [2, 3, 4].map((k) => /* @__PURE__ */ a.jsxs("button", { className: "btn small secondary", disabled: !y, onClick: () => ie(k), children: [
              k,
              " plazos"
            ] }, k)),
            q && /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", onClick: () => he(null), children: "Según forma de pago" })
          ] })
        ] }),
        q ? /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          q.map((k, V) => /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", style: { marginBottom: 6 }, children: [
            /* @__PURE__ */ a.jsx("input", { type: "date", value: k.fecha, onChange: (fe) => he(q.map((ce, K) => K === V ? { ...ce, fecha: fe.target.value } : ce)) }),
            /* @__PURE__ */ a.jsx("input", { className: "num", type: "number", step: "0.01", value: k.importe, onChange: (fe) => he(q.map((ce, K) => K === V ? { ...ce, importe: Number(fe.target.value) } : ce)) })
          ] }, V)),
          y && Bn !== y.total && /* @__PURE__ */ a.jsxs("p", { className: "dx-rojo", style: { margin: 0 }, children: [
            "Los plazos suman ",
            M(Bn),
            "; la factura, ",
            M(y.total),
            "."
          ] })
        ] }) : /* @__PURE__ */ a.jsx("p", { className: "muted", style: { margin: 0 }, children: ((y == null ? void 0 : y.vencimientos) ?? []).map((k) => `${Ue(k.fecha)}: ${M(k.importe)}`).join(" · ") || "—" }),
        S && /* @__PURE__ */ a.jsx("p", { className: "dx-rojo", style: { marginBottom: 0 }, children: S })
      ] }),
      /* @__PURE__ */ a.jsxs("div", { className: "panel dx-totales", children: [
        ((y == null ? void 0 : y.desglose) ?? []).map((k, V) => {
          var fe;
          return /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
            /* @__PURE__ */ a.jsxs("span", { className: "muted", children: [
              ((fe = qt(k.codigoIva)) == null ? void 0 : fe.nombre) ?? k.codigoIva,
              " ",
              k.autoliquidada ? `(${ge(k.porcentajeIva)} %, autoliquidado)` : "",
              " · base ",
              ge(k.base)
            ] }),
            /* @__PURE__ */ a.jsx("span", { children: M(k.cuota) })
          ] }, V);
        }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Base imponible" }),
          /* @__PURE__ */ a.jsx("span", { children: M(y == null ? void 0 : y.baseImponible) })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Impuestos" }),
          /* @__PURE__ */ a.jsx("span", { children: M(y == null ? void 0 : y.cuotaIva) })
        ] }),
        !!(y != null && y.recargoTotal) && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Recargo de equivalencia" }),
          /* @__PURE__ */ a.jsx("span", { children: M(y.recargoTotal) })
        ] }),
        !!(y != null && y.retencionIrpf) && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Retención IRPF" }),
          /* @__PURE__ */ a.jsxs("span", { children: [
            "−",
            M(y.retencionIrpf)
          ] })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ a.jsx("span", { children: "Total a pagar" }),
          /* @__PURE__ */ a.jsx("span", { children: M(y == null ? void 0 : y.total) })
        ] }),
        y && (y.desglose ?? []).some((k) => k.cuotaDeducible !== k.cuota) && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "IVA deducible (antes de prorrata)" }),
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: M((y.desglose ?? []).reduce((k, V) => k + V.cuotaDeducible, 0)) })
        ] })
      ] })
    ] })
  ] });
}
function bp(e) {
  const { api: t, anfitrion: n, navegar: r } = ht(), [l, i] = N.useState(null), [o, s] = N.useState(null), [u, d] = N.useState(""), [j, c] = N.useState(!1), m = () => {
    t.get(`/gastos/${e.id}`).then(i).catch((w) => d(w.message)), t.get(`/gastos/${e.id}/saldo`).then(s).catch(() => s(null));
  };
  if (N.useEffect(m, [e.id]), u) return /* @__PURE__ */ a.jsx("div", { className: "panel", children: /* @__PURE__ */ a.jsx("p", { className: "dx-rojo", children: u }) });
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
          /* @__PURE__ */ a.jsx("span", { className: jo(l.estado === "Anulado" ? "Anulada" : "Emitida"), children: l.estado })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-acciones", children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "gasto", pantalla: "editor", semilla: { ...l, numeroFactura: null } }), children: "Duplicar" }),
          v && g && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "gasto", pantalla: "editor", id: l.id, semilla: l }), children: "Corregir" }),
          v && o && o.pendiente > 0 && n.irA && /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: () => n.irA("pagos"), children: "Pagar" }),
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
          /* @__PURE__ */ a.jsx("div", { children: Ue(l.fechaFactura ?? l.fecha) })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-dato", children: [
          /* @__PURE__ */ a.jsx("small", { children: "Registro" }),
          /* @__PURE__ */ a.jsx("div", { children: Ue(l.fecha) })
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
        /* @__PURE__ */ a.jsx("tbody", { children: (l.lineas ?? []).map((w, z) => /* @__PURE__ */ a.jsxs("tr", { children: [
          /* @__PURE__ */ a.jsx("td", { children: w.descripcion ?? "" }),
          /* @__PURE__ */ a.jsx("td", { className: "mono muted", children: w.cuentaGasto ?? "regla" }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: M(w.base) }),
          /* @__PURE__ */ a.jsxs("td", { children: [
            w.codigoIva,
            " · ",
            ge(w.porcentajeIva),
            " %",
            w.autoliquidada ? " · autoliquidada" : "",
            w.cuotaRecargo ? ` · recargo ${M(w.cuotaRecargo)}` : ""
          ] }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: M(w.cuota) }),
          /* @__PURE__ */ a.jsxs("td", { className: "num muted", children: [
            w.porcentajeDeducible !== 100 ? `${ge(w.porcentajeDeducible)} % · ` : "",
            M(w.cuotaDeducible)
          ] })
        ] }, z)) })
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
            ge(l.porcentajeIrpf),
            " %)"
          ] }),
          /* @__PURE__ */ a.jsxs("span", { children: [
            "−",
            M(l.retencionIrpf)
          ] })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ a.jsx("span", { children: "Total a pagar" }),
          /* @__PURE__ */ a.jsx("span", { children: M(l.total) })
        ] })
      ] }),
      /* @__PURE__ */ a.jsxs("p", { className: "muted", style: { fontSize: 12.5 }, children: [
        "Vencimientos: ",
        (l.vencimientos ?? []).map((w) => `${Ue(w.fecha)} ${M(w.importe)}`).join(" · ")
      ] })
    ] }),
    j && /* @__PURE__ */ a.jsx(
      un,
      {
        titulo: "Anular la factura",
        alCerrar: () => c(!1),
        acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => c(!1), children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: async () => {
            try {
              await t.post(`/gastos/${l.id}/anular`), n.aviso("Factura anulada.", "ok"), c(!1), m();
            } catch (w) {
              n.aviso(w.message, "err");
            }
          }, children: "Anular" })
        ] }),
        children: /* @__PURE__ */ a.jsx("p", { style: { margin: 0 }, children: "Sale de los libros de IVA y de las declaraciones, y se contabiliza su contraasiento. Si solo hay que cambiar algo, mejor «Corregir»." })
      }
    )
  ] });
}
function Zp(e) {
  const [t, n] = N.useState(e.inicial), r = N.useRef(0), [l, i] = N.useState(0), o = N.useMemo(() => Rp(e.anfitrion), [e.anfitrion]), s = (c) => {
    n(c), i(++r.current), window.scrollTo({ top: 0 });
  }, u = { api: o, anfitrion: e.anfitrion, ruta: t, navegar: s }, d = `${t.tipo}-${t.pantalla}-${t.id ?? ""}-${l}`;
  let j;
  if (t.pantalla === "lista") j = /* @__PURE__ */ a.jsx(Ap, { tipo: t.tipo });
  else if (t.pantalla === "vista" && t.id)
    j = t.tipo === "factura" ? /* @__PURE__ */ a.jsx(Qp, { id: t.id }) : t.tipo === "presupuesto" ? /* @__PURE__ */ a.jsx(Gp, { id: t.id }) : t.tipo === "pedido" ? /* @__PURE__ */ a.jsx(Kp, { id: t.id }) : t.tipo === "gasto" ? /* @__PURE__ */ a.jsx(bp, { id: t.id }) : /* @__PURE__ */ a.jsx(qp, { id: t.id });
  else if (t.tipo === "gasto")
    j = /* @__PURE__ */ a.jsx(
      Xp,
      {
        id: t.id,
        semilla: t.semilla,
        alGuardar: (c) => s({ tipo: "gasto", pantalla: "vista", id: c }),
        alCancelar: () => s(t.id ? { tipo: "gasto", pantalla: "vista", id: t.id } : { tipo: "gasto", pantalla: "lista" })
      }
    );
  else if (t.tipo === "compra")
    j = /* @__PURE__ */ a.jsx(
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
    j = /* @__PURE__ */ a.jsx(
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
  return /* @__PURE__ */ a.jsx(bc.Provider, { value: u, children: /* @__PURE__ */ a.jsx("div", { className: "dx-raiz", children: j }, d) });
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
  const r = Xc(e);
  return r.render(/* @__PURE__ */ a.jsx(Zp, { anfitrion: t, inicial: n })), () => r.unmount();
}
export {
  tm as montar
};
