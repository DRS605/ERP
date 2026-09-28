var ec = { exports: {} }, Ga = {}, tc = { exports: {} }, G = {};
/**
 * @license React
 * react.production.min.js
 *
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */
var br = Symbol.for("react.element"), kd = Symbol.for("react.portal"), Cd = Symbol.for("react.fragment"), Ed = Symbol.for("react.strict_mode"), Id = Symbol.for("react.profiler"), Pd = Symbol.for("react.provider"), Fd = Symbol.for("react.context"), _d = Symbol.for("react.forward_ref"), Rd = Symbol.for("react.suspense"), Td = Symbol.for("react.memo"), zd = Symbol.for("react.lazy"), Vo = Symbol.iterator;
function Dd(e) {
  return e === null || typeof e != "object" ? null : (e = Vo && e[Vo] || e["@@iterator"], typeof e == "function" ? e : null);
}
var nc = { isMounted: function() {
  return !1;
}, enqueueForceUpdate: function() {
}, enqueueReplaceState: function() {
}, enqueueSetState: function() {
} }, rc = Object.assign, ac = {};
function Jn(e, t, n) {
  this.props = e, this.context = t, this.refs = ac, this.updater = n || nc;
}
Jn.prototype.isReactComponent = {};
Jn.prototype.setState = function(e, t) {
  if (typeof e != "object" && typeof e != "function" && e != null) throw Error("setState(...): takes an object of state variables to update or a function which returns an object of state variables.");
  this.updater.enqueueSetState(this, e, t, "setState");
};
Jn.prototype.forceUpdate = function(e) {
  this.updater.enqueueForceUpdate(this, e, "forceUpdate");
};
function lc() {
}
lc.prototype = Jn.prototype;
function $i(e, t, n) {
  this.props = e, this.context = t, this.refs = ac, this.updater = n || nc;
}
var Ai = $i.prototype = new lc();
Ai.constructor = $i;
rc(Ai, Jn.prototype);
Ai.isPureReactComponent = !0;
var Bo = Array.isArray, ic = Object.prototype.hasOwnProperty, Oi = { current: null }, oc = { key: !0, ref: !0, __self: !0, __source: !0 };
function sc(e, t, n) {
  var r, a = {}, i = null, o = null;
  if (t != null) for (r in t.ref !== void 0 && (o = t.ref), t.key !== void 0 && (i = "" + t.key), t) ic.call(t, r) && !oc.hasOwnProperty(r) && (a[r] = t[r]);
  var s = arguments.length - 2;
  if (s === 1) a.children = n;
  else if (1 < s) {
    for (var c = Array(s), d = 0; d < s; d++) c[d] = arguments[d + 2];
    a.children = c;
  }
  if (e && e.defaultProps) for (r in s = e.defaultProps, s) a[r] === void 0 && (a[r] = s[r]);
  return { $$typeof: br, type: e, key: i, ref: o, props: a, _owner: Oi.current };
}
function Ld(e, t) {
  return { $$typeof: br, type: e.type, key: t, ref: e.ref, props: e.props, _owner: e._owner };
}
function Ui(e) {
  return typeof e == "object" && e !== null && e.$$typeof === br;
}
function Md(e) {
  var t = { "=": "=0", ":": "=2" };
  return "$" + e.replace(/[=:]/g, function(n) {
    return t[n];
  });
}
var Ho = /\/+/g;
function ml(e, t) {
  return typeof e == "object" && e !== null && e.key != null ? Md("" + e.key) : t.toString(36);
}
function da(e, t, n, r, a) {
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
        case br:
        case kd:
          o = !0;
      }
  }
  if (o) return o = e, a = a(o), e = r === "" ? "." + ml(o, 0) : r, Bo(a) ? (n = "", e != null && (n = e.replace(Ho, "$&/") + "/"), da(a, t, n, "", function(d) {
    return d;
  })) : a != null && (Ui(a) && (a = Ld(a, n + (!a.key || o && o.key === a.key ? "" : ("" + a.key).replace(Ho, "$&/") + "/") + e)), t.push(a)), 1;
  if (o = 0, r = r === "" ? "." : r + ":", Bo(e)) for (var s = 0; s < e.length; s++) {
    i = e[s];
    var c = r + ml(i, s);
    o += da(i, t, n, c, a);
  }
  else if (c = Dd(e), typeof c == "function") for (e = c.call(e), s = 0; !(i = e.next()).done; ) i = i.value, c = r + ml(i, s++), o += da(i, t, n, c, a);
  else if (i === "object") throw t = String(e), Error("Objects are not valid as a React child (found: " + (t === "[object Object]" ? "object with keys {" + Object.keys(e).join(", ") + "}" : t) + "). If you meant to render a collection of children, use an array instead.");
  return o;
}
function Kr(e, t, n) {
  if (e == null) return e;
  var r = [], a = 0;
  return da(e, r, "", "", function(i) {
    return t.call(n, i, a++);
  }), r;
}
function $d(e) {
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
var Ue = { current: null }, fa = { transition: null }, Ad = { ReactCurrentDispatcher: Ue, ReactCurrentBatchConfig: fa, ReactCurrentOwner: Oi };
function cc() {
  throw Error("act(...) is not supported in production builds of React.");
}
G.Children = { map: Kr, forEach: function(e, t, n) {
  Kr(e, function() {
    t.apply(this, arguments);
  }, n);
}, count: function(e) {
  var t = 0;
  return Kr(e, function() {
    t++;
  }), t;
}, toArray: function(e) {
  return Kr(e, function(t) {
    return t;
  }) || [];
}, only: function(e) {
  if (!Ui(e)) throw Error("React.Children.only expected to receive a single React element child.");
  return e;
} };
G.Component = Jn;
G.Fragment = Cd;
G.Profiler = Id;
G.PureComponent = $i;
G.StrictMode = Ed;
G.Suspense = Rd;
G.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED = Ad;
G.act = cc;
G.cloneElement = function(e, t, n) {
  if (e == null) throw Error("React.cloneElement(...): The argument must be a React element, but you passed " + e + ".");
  var r = rc({}, e.props), a = e.key, i = e.ref, o = e._owner;
  if (t != null) {
    if (t.ref !== void 0 && (i = t.ref, o = Oi.current), t.key !== void 0 && (a = "" + t.key), e.type && e.type.defaultProps) var s = e.type.defaultProps;
    for (c in t) ic.call(t, c) && !oc.hasOwnProperty(c) && (r[c] = t[c] === void 0 && s !== void 0 ? s[c] : t[c]);
  }
  var c = arguments.length - 2;
  if (c === 1) r.children = n;
  else if (1 < c) {
    s = Array(c);
    for (var d = 0; d < c; d++) s[d] = arguments[d + 2];
    r.children = s;
  }
  return { $$typeof: br, type: e.type, key: a, ref: i, props: r, _owner: o };
};
G.createContext = function(e) {
  return e = { $$typeof: Fd, _currentValue: e, _currentValue2: e, _threadCount: 0, Provider: null, Consumer: null, _defaultValue: null, _globalName: null }, e.Provider = { $$typeof: Pd, _context: e }, e.Consumer = e;
};
G.createElement = sc;
G.createFactory = function(e) {
  var t = sc.bind(null, e);
  return t.type = e, t;
};
G.createRef = function() {
  return { current: null };
};
G.forwardRef = function(e) {
  return { $$typeof: _d, render: e };
};
G.isValidElement = Ui;
G.lazy = function(e) {
  return { $$typeof: zd, _payload: { _status: -1, _result: e }, _init: $d };
};
G.memo = function(e, t) {
  return { $$typeof: Td, type: e, compare: t === void 0 ? null : t };
};
G.startTransition = function(e) {
  var t = fa.transition;
  fa.transition = {};
  try {
    e();
  } finally {
    fa.transition = t;
  }
};
G.unstable_act = cc;
G.useCallback = function(e, t) {
  return Ue.current.useCallback(e, t);
};
G.useContext = function(e) {
  return Ue.current.useContext(e);
};
G.useDebugValue = function() {
};
G.useDeferredValue = function(e) {
  return Ue.current.useDeferredValue(e);
};
G.useEffect = function(e, t) {
  return Ue.current.useEffect(e, t);
};
G.useId = function() {
  return Ue.current.useId();
};
G.useImperativeHandle = function(e, t, n) {
  return Ue.current.useImperativeHandle(e, t, n);
};
G.useInsertionEffect = function(e, t) {
  return Ue.current.useInsertionEffect(e, t);
};
G.useLayoutEffect = function(e, t) {
  return Ue.current.useLayoutEffect(e, t);
};
G.useMemo = function(e, t) {
  return Ue.current.useMemo(e, t);
};
G.useReducer = function(e, t, n) {
  return Ue.current.useReducer(e, t, n);
};
G.useRef = function(e) {
  return Ue.current.useRef(e);
};
G.useState = function(e) {
  return Ue.current.useState(e);
};
G.useSyncExternalStore = function(e, t, n) {
  return Ue.current.useSyncExternalStore(e, t, n);
};
G.useTransition = function() {
  return Ue.current.useTransition();
};
G.version = "18.3.1";
tc.exports = G;
var g = tc.exports;
/**
 * @license React
 * react-jsx-runtime.production.min.js
 *
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */
var Od = g, Ud = Symbol.for("react.element"), bd = Symbol.for("react.fragment"), Vd = Object.prototype.hasOwnProperty, Bd = Od.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED.ReactCurrentOwner, Hd = { key: !0, ref: !0, __self: !0, __source: !0 };
function uc(e, t, n) {
  var r, a = {}, i = null, o = null;
  n !== void 0 && (i = "" + n), t.key !== void 0 && (i = "" + t.key), t.ref !== void 0 && (o = t.ref);
  for (r in t) Vd.call(t, r) && !Hd.hasOwnProperty(r) && (a[r] = t[r]);
  if (e && e.defaultProps) for (r in t = e.defaultProps, t) a[r] === void 0 && (a[r] = t[r]);
  return { $$typeof: Ud, type: e, key: i, ref: o, props: a, _owner: Bd.current };
}
Ga.Fragment = bd;
Ga.jsx = uc;
Ga.jsxs = uc;
ec.exports = Ga;
var l = ec.exports, dc = { exports: {} }, et = {}, fc = { exports: {} }, pc = {};
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
  function t(I, j) {
    var w = I.length;
    I.push(j);
    e: for (; 0 < w; ) {
      var b = w - 1 >>> 1, W = I[b];
      if (0 < a(W, j)) I[b] = j, I[w] = W, w = b;
      else break e;
    }
  }
  function n(I) {
    return I.length === 0 ? null : I[0];
  }
  function r(I) {
    if (I.length === 0) return null;
    var j = I[0], w = I.pop();
    if (w !== j) {
      I[0] = w;
      e: for (var b = 0, W = I.length, K = W >>> 1; b < K; ) {
        var pe = 2 * (b + 1) - 1, me = I[pe], te = pe + 1, U = I[te];
        if (0 > a(me, w)) te < W && 0 > a(U, me) ? (I[b] = U, I[te] = w, b = te) : (I[b] = me, I[pe] = w, b = pe);
        else if (te < W && 0 > a(U, w)) I[b] = U, I[te] = w, b = te;
        else break e;
      }
    }
    return j;
  }
  function a(I, j) {
    var w = I.sortIndex - j.sortIndex;
    return w !== 0 ? w : I.id - j.id;
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
  var c = [], d = [], N = 1, u = null, h = 3, v = !1, y = !1, S = !1, $ = typeof setTimeout == "function" ? setTimeout : null, p = typeof clearTimeout == "function" ? clearTimeout : null, f = typeof setImmediate < "u" ? setImmediate : null;
  typeof navigator < "u" && navigator.scheduling !== void 0 && navigator.scheduling.isInputPending !== void 0 && navigator.scheduling.isInputPending.bind(navigator.scheduling);
  function x(I) {
    for (var j = n(d); j !== null; ) {
      if (j.callback === null) r(d);
      else if (j.startTime <= I) r(d), j.sortIndex = j.expirationTime, t(c, j);
      else break;
      j = n(d);
    }
  }
  function E(I) {
    if (S = !1, x(I), !y) if (n(c) !== null) y = !0, fe(T);
    else {
      var j = n(d);
      j !== null && xe(E, j.startTime - I);
    }
  }
  function T(I, j) {
    y = !1, S && (S = !1, p(k), k = -1), v = !0;
    var w = h;
    try {
      for (x(j), u = n(c); u !== null && (!(u.expirationTime > j) || I && !R()); ) {
        var b = u.callback;
        if (typeof b == "function") {
          u.callback = null, h = u.priorityLevel;
          var W = b(u.expirationTime <= j);
          j = e.unstable_now(), typeof W == "function" ? u.callback = W : u === n(c) && r(c), x(j);
        } else r(c);
        u = n(c);
      }
      if (u !== null) var K = !0;
      else {
        var pe = n(d);
        pe !== null && xe(E, pe.startTime - j), K = !1;
      }
      return K;
    } finally {
      u = null, h = w, v = !1;
    }
  }
  var F = !1, D = null, k = -1, M = 5, O = -1;
  function R() {
    return !(e.unstable_now() - O < M);
  }
  function Q() {
    if (D !== null) {
      var I = e.unstable_now();
      O = I;
      var j = !0;
      try {
        j = D(!0, I);
      } finally {
        j ? se() : (F = !1, D = null);
      }
    } else F = !1;
  }
  var se;
  if (typeof f == "function") se = function() {
    f(Q);
  };
  else if (typeof MessageChannel < "u") {
    var Fe = new MessageChannel(), Le = Fe.port2;
    Fe.port1.onmessage = Q, se = function() {
      Le.postMessage(null);
    };
  } else se = function() {
    $(Q, 0);
  };
  function fe(I) {
    D = I, F || (F = !0, se());
  }
  function xe(I, j) {
    k = $(function() {
      I(e.unstable_now());
    }, j);
  }
  e.unstable_IdlePriority = 5, e.unstable_ImmediatePriority = 1, e.unstable_LowPriority = 4, e.unstable_NormalPriority = 3, e.unstable_Profiling = null, e.unstable_UserBlockingPriority = 2, e.unstable_cancelCallback = function(I) {
    I.callback = null;
  }, e.unstable_continueExecution = function() {
    y || v || (y = !0, fe(T));
  }, e.unstable_forceFrameRate = function(I) {
    0 > I || 125 < I ? console.error("forceFrameRate takes a positive int between 0 and 125, forcing frame rates higher than 125 fps is not supported") : M = 0 < I ? Math.floor(1e3 / I) : 5;
  }, e.unstable_getCurrentPriorityLevel = function() {
    return h;
  }, e.unstable_getFirstCallbackNode = function() {
    return n(c);
  }, e.unstable_next = function(I) {
    switch (h) {
      case 1:
      case 2:
      case 3:
        var j = 3;
        break;
      default:
        j = h;
    }
    var w = h;
    h = j;
    try {
      return I();
    } finally {
      h = w;
    }
  }, e.unstable_pauseExecution = function() {
  }, e.unstable_requestPaint = function() {
  }, e.unstable_runWithPriority = function(I, j) {
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
    var w = h;
    h = I;
    try {
      return j();
    } finally {
      h = w;
    }
  }, e.unstable_scheduleCallback = function(I, j, w) {
    var b = e.unstable_now();
    switch (typeof w == "object" && w !== null ? (w = w.delay, w = typeof w == "number" && 0 < w ? b + w : b) : w = b, I) {
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
    return W = w + W, I = { id: N++, callback: j, priorityLevel: I, startTime: w, expirationTime: W, sortIndex: -1 }, w > b ? (I.sortIndex = w, t(d, I), n(c) === null && I === n(d) && (S ? (p(k), k = -1) : S = !0, xe(E, w - b))) : (I.sortIndex = W, t(c, I), y || v || (y = !0, fe(T))), I;
  }, e.unstable_shouldYield = R, e.unstable_wrapCallback = function(I) {
    var j = h;
    return function() {
      var w = h;
      h = j;
      try {
        return I.apply(this, arguments);
      } finally {
        h = w;
      }
    };
  };
})(pc);
fc.exports = pc;
var Wd = fc.exports;
/**
 * @license React
 * react-dom.production.min.js
 *
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */
var Qd = g, Je = Wd;
function P(e) {
  for (var t = "https://reactjs.org/docs/error-decoder.html?invariant=" + e, n = 1; n < arguments.length; n++) t += "&args[]=" + encodeURIComponent(arguments[n]);
  return "Minified React error #" + e + "; visit " + t + " for the full message or use the non-minified dev environment for full errors and additional helpful warnings.";
}
var mc = /* @__PURE__ */ new Set(), kr = {};
function Sn(e, t) {
  Qn(e, t), Qn(e + "Capture", t);
}
function Qn(e, t) {
  for (kr[e] = t, e = 0; e < t.length; e++) mc.add(t[e]);
}
var zt = !(typeof window > "u" || typeof window.document > "u" || typeof window.document.createElement > "u"), Bl = Object.prototype.hasOwnProperty, Gd = /^[:A-Z_a-z\u00C0-\u00D6\u00D8-\u00F6\u00F8-\u02FF\u0370-\u037D\u037F-\u1FFF\u200C-\u200D\u2070-\u218F\u2C00-\u2FEF\u3001-\uD7FF\uF900-\uFDCF\uFDF0-\uFFFD][:A-Z_a-z\u00C0-\u00D6\u00D8-\u00F6\u00F8-\u02FF\u0370-\u037D\u037F-\u1FFF\u200C-\u200D\u2070-\u218F\u2C00-\u2FEF\u3001-\uD7FF\uF900-\uFDCF\uFDF0-\uFFFD\-.0-9\u00B7\u0300-\u036F\u203F-\u2040]*$/, Wo = {}, Qo = {};
function Kd(e) {
  return Bl.call(Qo, e) ? !0 : Bl.call(Wo, e) ? !1 : Gd.test(e) ? Qo[e] = !0 : (Wo[e] = !0, !1);
}
function qd(e, t, n, r) {
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
function Yd(e, t, n, r) {
  if (t === null || typeof t > "u" || qd(e, t, n, r)) return !0;
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
function be(e, t, n, r, a, i, o) {
  this.acceptsBooleans = t === 2 || t === 3 || t === 4, this.attributeName = r, this.attributeNamespace = a, this.mustUseProperty = n, this.propertyName = e, this.type = t, this.sanitizeURL = i, this.removeEmptyString = o;
}
var Pe = {};
"children dangerouslySetInnerHTML defaultValue defaultChecked innerHTML suppressContentEditableWarning suppressHydrationWarning style".split(" ").forEach(function(e) {
  Pe[e] = new be(e, 0, !1, e, null, !1, !1);
});
[["acceptCharset", "accept-charset"], ["className", "class"], ["htmlFor", "for"], ["httpEquiv", "http-equiv"]].forEach(function(e) {
  var t = e[0];
  Pe[t] = new be(t, 1, !1, e[1], null, !1, !1);
});
["contentEditable", "draggable", "spellCheck", "value"].forEach(function(e) {
  Pe[e] = new be(e, 2, !1, e.toLowerCase(), null, !1, !1);
});
["autoReverse", "externalResourcesRequired", "focusable", "preserveAlpha"].forEach(function(e) {
  Pe[e] = new be(e, 2, !1, e, null, !1, !1);
});
"allowFullScreen async autoFocus autoPlay controls default defer disabled disablePictureInPicture disableRemotePlayback formNoValidate hidden loop noModule noValidate open playsInline readOnly required reversed scoped seamless itemScope".split(" ").forEach(function(e) {
  Pe[e] = new be(e, 3, !1, e.toLowerCase(), null, !1, !1);
});
["checked", "multiple", "muted", "selected"].forEach(function(e) {
  Pe[e] = new be(e, 3, !0, e, null, !1, !1);
});
["capture", "download"].forEach(function(e) {
  Pe[e] = new be(e, 4, !1, e, null, !1, !1);
});
["cols", "rows", "size", "span"].forEach(function(e) {
  Pe[e] = new be(e, 6, !1, e, null, !1, !1);
});
["rowSpan", "start"].forEach(function(e) {
  Pe[e] = new be(e, 5, !1, e.toLowerCase(), null, !1, !1);
});
var bi = /[\-:]([a-z])/g;
function Vi(e) {
  return e[1].toUpperCase();
}
"accent-height alignment-baseline arabic-form baseline-shift cap-height clip-path clip-rule color-interpolation color-interpolation-filters color-profile color-rendering dominant-baseline enable-background fill-opacity fill-rule flood-color flood-opacity font-family font-size font-size-adjust font-stretch font-style font-variant font-weight glyph-name glyph-orientation-horizontal glyph-orientation-vertical horiz-adv-x horiz-origin-x image-rendering letter-spacing lighting-color marker-end marker-mid marker-start overline-position overline-thickness paint-order panose-1 pointer-events rendering-intent shape-rendering stop-color stop-opacity strikethrough-position strikethrough-thickness stroke-dasharray stroke-dashoffset stroke-linecap stroke-linejoin stroke-miterlimit stroke-opacity stroke-width text-anchor text-decoration text-rendering underline-position underline-thickness unicode-bidi unicode-range units-per-em v-alphabetic v-hanging v-ideographic v-mathematical vector-effect vert-adv-y vert-origin-x vert-origin-y word-spacing writing-mode xmlns:xlink x-height".split(" ").forEach(function(e) {
  var t = e.replace(
    bi,
    Vi
  );
  Pe[t] = new be(t, 1, !1, e, null, !1, !1);
});
"xlink:actuate xlink:arcrole xlink:role xlink:show xlink:title xlink:type".split(" ").forEach(function(e) {
  var t = e.replace(bi, Vi);
  Pe[t] = new be(t, 1, !1, e, "http://www.w3.org/1999/xlink", !1, !1);
});
["xml:base", "xml:lang", "xml:space"].forEach(function(e) {
  var t = e.replace(bi, Vi);
  Pe[t] = new be(t, 1, !1, e, "http://www.w3.org/XML/1998/namespace", !1, !1);
});
["tabIndex", "crossOrigin"].forEach(function(e) {
  Pe[e] = new be(e, 1, !1, e.toLowerCase(), null, !1, !1);
});
Pe.xlinkHref = new be("xlinkHref", 1, !1, "xlink:href", "http://www.w3.org/1999/xlink", !0, !1);
["src", "href", "action", "formAction"].forEach(function(e) {
  Pe[e] = new be(e, 1, !1, e.toLowerCase(), null, !0, !0);
});
function Bi(e, t, n, r) {
  var a = Pe.hasOwnProperty(t) ? Pe[t] : null;
  (a !== null ? a.type !== 0 : r || !(2 < t.length) || t[0] !== "o" && t[0] !== "O" || t[1] !== "n" && t[1] !== "N") && (Yd(t, n, a, r) && (n = null), r || a === null ? Kd(t) && (n === null ? e.removeAttribute(t) : e.setAttribute(t, "" + n)) : a.mustUseProperty ? e[a.propertyName] = n === null ? a.type === 3 ? !1 : "" : n : (t = a.attributeName, r = a.attributeNamespace, n === null ? e.removeAttribute(t) : (a = a.type, n = a === 3 || a === 4 && n === !0 ? "" : "" + n, r ? e.setAttributeNS(r, t, n) : e.setAttribute(t, n))));
}
var $t = Qd.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED, qr = Symbol.for("react.element"), In = Symbol.for("react.portal"), Pn = Symbol.for("react.fragment"), Hi = Symbol.for("react.strict_mode"), Hl = Symbol.for("react.profiler"), hc = Symbol.for("react.provider"), vc = Symbol.for("react.context"), Wi = Symbol.for("react.forward_ref"), Wl = Symbol.for("react.suspense"), Ql = Symbol.for("react.suspense_list"), Qi = Symbol.for("react.memo"), Vt = Symbol.for("react.lazy"), xc = Symbol.for("react.offscreen"), Go = Symbol.iterator;
function nr(e) {
  return e === null || typeof e != "object" ? null : (e = Go && e[Go] || e["@@iterator"], typeof e == "function" ? e : null);
}
var de = Object.assign, hl;
function ur(e) {
  if (hl === void 0) try {
    throw Error();
  } catch (n) {
    var t = n.stack.trim().match(/\n( *(at )?)/);
    hl = t && t[1] || "";
  }
  return `
` + hl + e;
}
var vl = !1;
function xl(e, t) {
  if (!e || vl) return "";
  vl = !0;
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
    vl = !1, Error.prepareStackTrace = n;
  }
  return (e = e ? e.displayName || e.name : "") ? ur(e) : "";
}
function Xd(e) {
  switch (e.tag) {
    case 5:
      return ur(e.type);
    case 16:
      return ur("Lazy");
    case 13:
      return ur("Suspense");
    case 19:
      return ur("SuspenseList");
    case 0:
    case 2:
    case 15:
      return e = xl(e.type, !1), e;
    case 11:
      return e = xl(e.type.render, !1), e;
    case 1:
      return e = xl(e.type, !0), e;
    default:
      return "";
  }
}
function Gl(e) {
  if (e == null) return null;
  if (typeof e == "function") return e.displayName || e.name || null;
  if (typeof e == "string") return e;
  switch (e) {
    case Pn:
      return "Fragment";
    case In:
      return "Portal";
    case Hl:
      return "Profiler";
    case Hi:
      return "StrictMode";
    case Wl:
      return "Suspense";
    case Ql:
      return "SuspenseList";
  }
  if (typeof e == "object") switch (e.$$typeof) {
    case vc:
      return (e.displayName || "Context") + ".Consumer";
    case hc:
      return (e._context.displayName || "Context") + ".Provider";
    case Wi:
      var t = e.render;
      return e = e.displayName, e || (e = t.displayName || t.name || "", e = e !== "" ? "ForwardRef(" + e + ")" : "ForwardRef"), e;
    case Qi:
      return t = e.displayName || null, t !== null ? t : Gl(e.type) || "Memo";
    case Vt:
      t = e._payload, e = e._init;
      try {
        return Gl(e(t));
      } catch {
      }
  }
  return null;
}
function Zd(e) {
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
      return Gl(t);
    case 8:
      return t === Hi ? "StrictMode" : "Mode";
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
function nn(e) {
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
function gc(e) {
  var t = e.type;
  return (e = e.nodeName) && e.toLowerCase() === "input" && (t === "checkbox" || t === "radio");
}
function Jd(e) {
  var t = gc(e) ? "checked" : "value", n = Object.getOwnPropertyDescriptor(e.constructor.prototype, t), r = "" + e[t];
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
function Yr(e) {
  e._valueTracker || (e._valueTracker = Jd(e));
}
function yc(e) {
  if (!e) return !1;
  var t = e._valueTracker;
  if (!t) return !0;
  var n = t.getValue(), r = "";
  return e && (r = gc(e) ? e.checked ? "true" : "false" : e.value), e = r, e !== n ? (t.setValue(e), !0) : !1;
}
function wa(e) {
  if (e = e || (typeof document < "u" ? document : void 0), typeof e > "u") return null;
  try {
    return e.activeElement || e.body;
  } catch {
    return e.body;
  }
}
function Kl(e, t) {
  var n = t.checked;
  return de({}, t, { defaultChecked: void 0, defaultValue: void 0, value: void 0, checked: n ?? e._wrapperState.initialChecked });
}
function Ko(e, t) {
  var n = t.defaultValue == null ? "" : t.defaultValue, r = t.checked != null ? t.checked : t.defaultChecked;
  n = nn(t.value != null ? t.value : n), e._wrapperState = { initialChecked: r, initialValue: n, controlled: t.type === "checkbox" || t.type === "radio" ? t.checked != null : t.value != null };
}
function jc(e, t) {
  t = t.checked, t != null && Bi(e, "checked", t, !1);
}
function ql(e, t) {
  jc(e, t);
  var n = nn(t.value), r = t.type;
  if (n != null) r === "number" ? (n === 0 && e.value === "" || e.value != n) && (e.value = "" + n) : e.value !== "" + n && (e.value = "" + n);
  else if (r === "submit" || r === "reset") {
    e.removeAttribute("value");
    return;
  }
  t.hasOwnProperty("value") ? Yl(e, t.type, n) : t.hasOwnProperty("defaultValue") && Yl(e, t.type, nn(t.defaultValue)), t.checked == null && t.defaultChecked != null && (e.defaultChecked = !!t.defaultChecked);
}
function qo(e, t, n) {
  if (t.hasOwnProperty("value") || t.hasOwnProperty("defaultValue")) {
    var r = t.type;
    if (!(r !== "submit" && r !== "reset" || t.value !== void 0 && t.value !== null)) return;
    t = "" + e._wrapperState.initialValue, n || t === e.value || (e.value = t), e.defaultValue = t;
  }
  n = e.name, n !== "" && (e.name = ""), e.defaultChecked = !!e._wrapperState.initialChecked, n !== "" && (e.name = n);
}
function Yl(e, t, n) {
  (t !== "number" || wa(e.ownerDocument) !== e) && (n == null ? e.defaultValue = "" + e._wrapperState.initialValue : e.defaultValue !== "" + n && (e.defaultValue = "" + n));
}
var dr = Array.isArray;
function On(e, t, n, r) {
  if (e = e.options, t) {
    t = {};
    for (var a = 0; a < n.length; a++) t["$" + n[a]] = !0;
    for (n = 0; n < e.length; n++) a = t.hasOwnProperty("$" + e[n].value), e[n].selected !== a && (e[n].selected = a), a && r && (e[n].defaultSelected = !0);
  } else {
    for (n = "" + nn(n), t = null, a = 0; a < e.length; a++) {
      if (e[a].value === n) {
        e[a].selected = !0, r && (e[a].defaultSelected = !0);
        return;
      }
      t !== null || e[a].disabled || (t = e[a]);
    }
    t !== null && (t.selected = !0);
  }
}
function Xl(e, t) {
  if (t.dangerouslySetInnerHTML != null) throw Error(P(91));
  return de({}, t, { value: void 0, defaultValue: void 0, children: "" + e._wrapperState.initialValue });
}
function Yo(e, t) {
  var n = t.value;
  if (n == null) {
    if (n = t.children, t = t.defaultValue, n != null) {
      if (t != null) throw Error(P(92));
      if (dr(n)) {
        if (1 < n.length) throw Error(P(93));
        n = n[0];
      }
      t = n;
    }
    t == null && (t = ""), n = t;
  }
  e._wrapperState = { initialValue: nn(n) };
}
function Nc(e, t) {
  var n = nn(t.value), r = nn(t.defaultValue);
  n != null && (n = "" + n, n !== e.value && (e.value = n), t.defaultValue == null && e.defaultValue !== n && (e.defaultValue = n)), r != null && (e.defaultValue = "" + r);
}
function Xo(e) {
  var t = e.textContent;
  t === e._wrapperState.initialValue && t !== "" && t !== null && (e.value = t);
}
function Sc(e) {
  switch (e) {
    case "svg":
      return "http://www.w3.org/2000/svg";
    case "math":
      return "http://www.w3.org/1998/Math/MathML";
    default:
      return "http://www.w3.org/1999/xhtml";
  }
}
function Zl(e, t) {
  return e == null || e === "http://www.w3.org/1999/xhtml" ? Sc(t) : e === "http://www.w3.org/2000/svg" && t === "foreignObject" ? "http://www.w3.org/1999/xhtml" : e;
}
var Xr, wc = function(e) {
  return typeof MSApp < "u" && MSApp.execUnsafeLocalFunction ? function(t, n, r, a) {
    MSApp.execUnsafeLocalFunction(function() {
      return e(t, n, r, a);
    });
  } : e;
}(function(e, t) {
  if (e.namespaceURI !== "http://www.w3.org/2000/svg" || "innerHTML" in e) e.innerHTML = t;
  else {
    for (Xr = Xr || document.createElement("div"), Xr.innerHTML = "<svg>" + t.valueOf().toString() + "</svg>", t = Xr.firstChild; e.firstChild; ) e.removeChild(e.firstChild);
    for (; t.firstChild; ) e.appendChild(t.firstChild);
  }
});
function Cr(e, t) {
  if (t) {
    var n = e.firstChild;
    if (n && n === e.lastChild && n.nodeType === 3) {
      n.nodeValue = t;
      return;
    }
  }
  e.textContent = t;
}
var mr = {
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
}, ef = ["Webkit", "ms", "Moz", "O"];
Object.keys(mr).forEach(function(e) {
  ef.forEach(function(t) {
    t = t + e.charAt(0).toUpperCase() + e.substring(1), mr[t] = mr[e];
  });
});
function kc(e, t, n) {
  return t == null || typeof t == "boolean" || t === "" ? "" : n || typeof t != "number" || t === 0 || mr.hasOwnProperty(e) && mr[e] ? ("" + t).trim() : t + "px";
}
function Cc(e, t) {
  e = e.style;
  for (var n in t) if (t.hasOwnProperty(n)) {
    var r = n.indexOf("--") === 0, a = kc(n, t[n], r);
    n === "float" && (n = "cssFloat"), r ? e.setProperty(n, a) : e[n] = a;
  }
}
var tf = de({ menuitem: !0 }, { area: !0, base: !0, br: !0, col: !0, embed: !0, hr: !0, img: !0, input: !0, keygen: !0, link: !0, meta: !0, param: !0, source: !0, track: !0, wbr: !0 });
function Jl(e, t) {
  if (t) {
    if (tf[e] && (t.children != null || t.dangerouslySetInnerHTML != null)) throw Error(P(137, e));
    if (t.dangerouslySetInnerHTML != null) {
      if (t.children != null) throw Error(P(60));
      if (typeof t.dangerouslySetInnerHTML != "object" || !("__html" in t.dangerouslySetInnerHTML)) throw Error(P(61));
    }
    if (t.style != null && typeof t.style != "object") throw Error(P(62));
  }
}
function ei(e, t) {
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
var ti = null;
function Gi(e) {
  return e = e.target || e.srcElement || window, e.correspondingUseElement && (e = e.correspondingUseElement), e.nodeType === 3 ? e.parentNode : e;
}
var ni = null, Un = null, bn = null;
function Zo(e) {
  if (e = Hr(e)) {
    if (typeof ni != "function") throw Error(P(280));
    var t = e.stateNode;
    t && (t = Za(t), ni(e.stateNode, e.type, t));
  }
}
function Ec(e) {
  Un ? bn ? bn.push(e) : bn = [e] : Un = e;
}
function Ic() {
  if (Un) {
    var e = Un, t = bn;
    if (bn = Un = null, Zo(e), t) for (e = 0; e < t.length; e++) Zo(t[e]);
  }
}
function Pc(e, t) {
  return e(t);
}
function Fc() {
}
var gl = !1;
function _c(e, t, n) {
  if (gl) return e(t, n);
  gl = !0;
  try {
    return Pc(e, t, n);
  } finally {
    gl = !1, (Un !== null || bn !== null) && (Fc(), Ic());
  }
}
function Er(e, t) {
  var n = e.stateNode;
  if (n === null) return null;
  var r = Za(n);
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
var ri = !1;
if (zt) try {
  var rr = {};
  Object.defineProperty(rr, "passive", { get: function() {
    ri = !0;
  } }), window.addEventListener("test", rr, rr), window.removeEventListener("test", rr, rr);
} catch {
  ri = !1;
}
function nf(e, t, n, r, a, i, o, s, c) {
  var d = Array.prototype.slice.call(arguments, 3);
  try {
    t.apply(n, d);
  } catch (N) {
    this.onError(N);
  }
}
var hr = !1, ka = null, Ca = !1, ai = null, rf = { onError: function(e) {
  hr = !0, ka = e;
} };
function af(e, t, n, r, a, i, o, s, c) {
  hr = !1, ka = null, nf.apply(rf, arguments);
}
function lf(e, t, n, r, a, i, o, s, c) {
  if (af.apply(this, arguments), hr) {
    if (hr) {
      var d = ka;
      hr = !1, ka = null;
    } else throw Error(P(198));
    Ca || (Ca = !0, ai = d);
  }
}
function wn(e) {
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
function Rc(e) {
  if (e.tag === 13) {
    var t = e.memoizedState;
    if (t === null && (e = e.alternate, e !== null && (t = e.memoizedState)), t !== null) return t.dehydrated;
  }
  return null;
}
function Jo(e) {
  if (wn(e) !== e) throw Error(P(188));
}
function of(e) {
  var t = e.alternate;
  if (!t) {
    if (t = wn(e), t === null) throw Error(P(188));
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
        if (i === n) return Jo(a), e;
        if (i === r) return Jo(a), t;
        i = i.sibling;
      }
      throw Error(P(188));
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
        if (!o) throw Error(P(189));
      }
    }
    if (n.alternate !== r) throw Error(P(190));
  }
  if (n.tag !== 3) throw Error(P(188));
  return n.stateNode.current === n ? e : t;
}
function Tc(e) {
  return e = of(e), e !== null ? zc(e) : null;
}
function zc(e) {
  if (e.tag === 5 || e.tag === 6) return e;
  for (e = e.child; e !== null; ) {
    var t = zc(e);
    if (t !== null) return t;
    e = e.sibling;
  }
  return null;
}
var Dc = Je.unstable_scheduleCallback, es = Je.unstable_cancelCallback, sf = Je.unstable_shouldYield, cf = Je.unstable_requestPaint, ve = Je.unstable_now, uf = Je.unstable_getCurrentPriorityLevel, Ki = Je.unstable_ImmediatePriority, Lc = Je.unstable_UserBlockingPriority, Ea = Je.unstable_NormalPriority, df = Je.unstable_LowPriority, Mc = Je.unstable_IdlePriority, Ka = null, wt = null;
function ff(e) {
  if (wt && typeof wt.onCommitFiberRoot == "function") try {
    wt.onCommitFiberRoot(Ka, e, void 0, (e.current.flags & 128) === 128);
  } catch {
  }
}
var vt = Math.clz32 ? Math.clz32 : hf, pf = Math.log, mf = Math.LN2;
function hf(e) {
  return e >>>= 0, e === 0 ? 32 : 31 - (pf(e) / mf | 0) | 0;
}
var Zr = 64, Jr = 4194304;
function fr(e) {
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
function Ia(e, t) {
  var n = e.pendingLanes;
  if (n === 0) return 0;
  var r = 0, a = e.suspendedLanes, i = e.pingedLanes, o = n & 268435455;
  if (o !== 0) {
    var s = o & ~a;
    s !== 0 ? r = fr(s) : (i &= o, i !== 0 && (r = fr(i)));
  } else o = n & ~a, o !== 0 ? r = fr(o) : i !== 0 && (r = fr(i));
  if (r === 0) return 0;
  if (t !== 0 && t !== r && !(t & a) && (a = r & -r, i = t & -t, a >= i || a === 16 && (i & 4194240) !== 0)) return t;
  if (r & 4 && (r |= n & 16), t = e.entangledLanes, t !== 0) for (e = e.entanglements, t &= r; 0 < t; ) n = 31 - vt(t), a = 1 << n, r |= e[n], t &= ~a;
  return r;
}
function vf(e, t) {
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
function xf(e, t) {
  for (var n = e.suspendedLanes, r = e.pingedLanes, a = e.expirationTimes, i = e.pendingLanes; 0 < i; ) {
    var o = 31 - vt(i), s = 1 << o, c = a[o];
    c === -1 ? (!(s & n) || s & r) && (a[o] = vf(s, t)) : c <= t && (e.expiredLanes |= s), i &= ~s;
  }
}
function li(e) {
  return e = e.pendingLanes & -1073741825, e !== 0 ? e : e & 1073741824 ? 1073741824 : 0;
}
function $c() {
  var e = Zr;
  return Zr <<= 1, !(Zr & 4194240) && (Zr = 64), e;
}
function yl(e) {
  for (var t = [], n = 0; 31 > n; n++) t.push(e);
  return t;
}
function Vr(e, t, n) {
  e.pendingLanes |= t, t !== 536870912 && (e.suspendedLanes = 0, e.pingedLanes = 0), e = e.eventTimes, t = 31 - vt(t), e[t] = n;
}
function gf(e, t) {
  var n = e.pendingLanes & ~t;
  e.pendingLanes = t, e.suspendedLanes = 0, e.pingedLanes = 0, e.expiredLanes &= t, e.mutableReadLanes &= t, e.entangledLanes &= t, t = e.entanglements;
  var r = e.eventTimes;
  for (e = e.expirationTimes; 0 < n; ) {
    var a = 31 - vt(n), i = 1 << a;
    t[a] = 0, r[a] = -1, e[a] = -1, n &= ~i;
  }
}
function qi(e, t) {
  var n = e.entangledLanes |= t;
  for (e = e.entanglements; n; ) {
    var r = 31 - vt(n), a = 1 << r;
    a & t | e[r] & t && (e[r] |= t), n &= ~a;
  }
}
var J = 0;
function Ac(e) {
  return e &= -e, 1 < e ? 4 < e ? e & 268435455 ? 16 : 536870912 : 4 : 1;
}
var Oc, Yi, Uc, bc, Vc, ii = !1, ea = [], Kt = null, qt = null, Yt = null, Ir = /* @__PURE__ */ new Map(), Pr = /* @__PURE__ */ new Map(), Ht = [], yf = "mousedown mouseup touchcancel touchend touchstart auxclick dblclick pointercancel pointerdown pointerup dragend dragstart drop compositionend compositionstart keydown keypress keyup input textInput copy cut paste click change contextmenu reset submit".split(" ");
function ts(e, t) {
  switch (e) {
    case "focusin":
    case "focusout":
      Kt = null;
      break;
    case "dragenter":
    case "dragleave":
      qt = null;
      break;
    case "mouseover":
    case "mouseout":
      Yt = null;
      break;
    case "pointerover":
    case "pointerout":
      Ir.delete(t.pointerId);
      break;
    case "gotpointercapture":
    case "lostpointercapture":
      Pr.delete(t.pointerId);
  }
}
function ar(e, t, n, r, a, i) {
  return e === null || e.nativeEvent !== i ? (e = { blockedOn: t, domEventName: n, eventSystemFlags: r, nativeEvent: i, targetContainers: [a] }, t !== null && (t = Hr(t), t !== null && Yi(t)), e) : (e.eventSystemFlags |= r, t = e.targetContainers, a !== null && t.indexOf(a) === -1 && t.push(a), e);
}
function jf(e, t, n, r, a) {
  switch (t) {
    case "focusin":
      return Kt = ar(Kt, e, t, n, r, a), !0;
    case "dragenter":
      return qt = ar(qt, e, t, n, r, a), !0;
    case "mouseover":
      return Yt = ar(Yt, e, t, n, r, a), !0;
    case "pointerover":
      var i = a.pointerId;
      return Ir.set(i, ar(Ir.get(i) || null, e, t, n, r, a)), !0;
    case "gotpointercapture":
      return i = a.pointerId, Pr.set(i, ar(Pr.get(i) || null, e, t, n, r, a)), !0;
  }
  return !1;
}
function Bc(e) {
  var t = dn(e.target);
  if (t !== null) {
    var n = wn(t);
    if (n !== null) {
      if (t = n.tag, t === 13) {
        if (t = Rc(n), t !== null) {
          e.blockedOn = t, Vc(e.priority, function() {
            Uc(n);
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
function pa(e) {
  if (e.blockedOn !== null) return !1;
  for (var t = e.targetContainers; 0 < t.length; ) {
    var n = oi(e.domEventName, e.eventSystemFlags, t[0], e.nativeEvent);
    if (n === null) {
      n = e.nativeEvent;
      var r = new n.constructor(n.type, n);
      ti = r, n.target.dispatchEvent(r), ti = null;
    } else return t = Hr(n), t !== null && Yi(t), e.blockedOn = n, !1;
    t.shift();
  }
  return !0;
}
function ns(e, t, n) {
  pa(e) && n.delete(t);
}
function Nf() {
  ii = !1, Kt !== null && pa(Kt) && (Kt = null), qt !== null && pa(qt) && (qt = null), Yt !== null && pa(Yt) && (Yt = null), Ir.forEach(ns), Pr.forEach(ns);
}
function lr(e, t) {
  e.blockedOn === t && (e.blockedOn = null, ii || (ii = !0, Je.unstable_scheduleCallback(Je.unstable_NormalPriority, Nf)));
}
function Fr(e) {
  function t(a) {
    return lr(a, e);
  }
  if (0 < ea.length) {
    lr(ea[0], e);
    for (var n = 1; n < ea.length; n++) {
      var r = ea[n];
      r.blockedOn === e && (r.blockedOn = null);
    }
  }
  for (Kt !== null && lr(Kt, e), qt !== null && lr(qt, e), Yt !== null && lr(Yt, e), Ir.forEach(t), Pr.forEach(t), n = 0; n < Ht.length; n++) r = Ht[n], r.blockedOn === e && (r.blockedOn = null);
  for (; 0 < Ht.length && (n = Ht[0], n.blockedOn === null); ) Bc(n), n.blockedOn === null && Ht.shift();
}
var Vn = $t.ReactCurrentBatchConfig, Pa = !0;
function Sf(e, t, n, r) {
  var a = J, i = Vn.transition;
  Vn.transition = null;
  try {
    J = 1, Xi(e, t, n, r);
  } finally {
    J = a, Vn.transition = i;
  }
}
function wf(e, t, n, r) {
  var a = J, i = Vn.transition;
  Vn.transition = null;
  try {
    J = 4, Xi(e, t, n, r);
  } finally {
    J = a, Vn.transition = i;
  }
}
function Xi(e, t, n, r) {
  if (Pa) {
    var a = oi(e, t, n, r);
    if (a === null) Fl(e, t, r, Fa, n), ts(e, r);
    else if (jf(a, e, t, n, r)) r.stopPropagation();
    else if (ts(e, r), t & 4 && -1 < yf.indexOf(e)) {
      for (; a !== null; ) {
        var i = Hr(a);
        if (i !== null && Oc(i), i = oi(e, t, n, r), i === null && Fl(e, t, r, Fa, n), i === a) break;
        a = i;
      }
      a !== null && r.stopPropagation();
    } else Fl(e, t, r, null, n);
  }
}
var Fa = null;
function oi(e, t, n, r) {
  if (Fa = null, e = Gi(r), e = dn(e), e !== null) if (t = wn(e), t === null) e = null;
  else if (n = t.tag, n === 13) {
    if (e = Rc(t), e !== null) return e;
    e = null;
  } else if (n === 3) {
    if (t.stateNode.current.memoizedState.isDehydrated) return t.tag === 3 ? t.stateNode.containerInfo : null;
    e = null;
  } else t !== e && (e = null);
  return Fa = e, null;
}
function Hc(e) {
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
      switch (uf()) {
        case Ki:
          return 1;
        case Lc:
          return 4;
        case Ea:
        case df:
          return 16;
        case Mc:
          return 536870912;
        default:
          return 16;
      }
    default:
      return 16;
  }
}
var Qt = null, Zi = null, ma = null;
function Wc() {
  if (ma) return ma;
  var e, t = Zi, n = t.length, r, a = "value" in Qt ? Qt.value : Qt.textContent, i = a.length;
  for (e = 0; e < n && t[e] === a[e]; e++) ;
  var o = n - e;
  for (r = 1; r <= o && t[n - r] === a[i - r]; r++) ;
  return ma = a.slice(e, 1 < r ? 1 - r : void 0);
}
function ha(e) {
  var t = e.keyCode;
  return "charCode" in e ? (e = e.charCode, e === 0 && t === 13 && (e = 13)) : e = t, e === 10 && (e = 13), 32 <= e || e === 13 ? e : 0;
}
function ta() {
  return !0;
}
function rs() {
  return !1;
}
function tt(e) {
  function t(n, r, a, i, o) {
    this._reactName = n, this._targetInst = a, this.type = r, this.nativeEvent = i, this.target = o, this.currentTarget = null;
    for (var s in e) e.hasOwnProperty(s) && (n = e[s], this[s] = n ? n(i) : i[s]);
    return this.isDefaultPrevented = (i.defaultPrevented != null ? i.defaultPrevented : i.returnValue === !1) ? ta : rs, this.isPropagationStopped = rs, this;
  }
  return de(t.prototype, { preventDefault: function() {
    this.defaultPrevented = !0;
    var n = this.nativeEvent;
    n && (n.preventDefault ? n.preventDefault() : typeof n.returnValue != "unknown" && (n.returnValue = !1), this.isDefaultPrevented = ta);
  }, stopPropagation: function() {
    var n = this.nativeEvent;
    n && (n.stopPropagation ? n.stopPropagation() : typeof n.cancelBubble != "unknown" && (n.cancelBubble = !0), this.isPropagationStopped = ta);
  }, persist: function() {
  }, isPersistent: ta }), t;
}
var er = { eventPhase: 0, bubbles: 0, cancelable: 0, timeStamp: function(e) {
  return e.timeStamp || Date.now();
}, defaultPrevented: 0, isTrusted: 0 }, Ji = tt(er), Br = de({}, er, { view: 0, detail: 0 }), kf = tt(Br), jl, Nl, ir, qa = de({}, Br, { screenX: 0, screenY: 0, clientX: 0, clientY: 0, pageX: 0, pageY: 0, ctrlKey: 0, shiftKey: 0, altKey: 0, metaKey: 0, getModifierState: eo, button: 0, buttons: 0, relatedTarget: function(e) {
  return e.relatedTarget === void 0 ? e.fromElement === e.srcElement ? e.toElement : e.fromElement : e.relatedTarget;
}, movementX: function(e) {
  return "movementX" in e ? e.movementX : (e !== ir && (ir && e.type === "mousemove" ? (jl = e.screenX - ir.screenX, Nl = e.screenY - ir.screenY) : Nl = jl = 0, ir = e), jl);
}, movementY: function(e) {
  return "movementY" in e ? e.movementY : Nl;
} }), as = tt(qa), Cf = de({}, qa, { dataTransfer: 0 }), Ef = tt(Cf), If = de({}, Br, { relatedTarget: 0 }), Sl = tt(If), Pf = de({}, er, { animationName: 0, elapsedTime: 0, pseudoElement: 0 }), Ff = tt(Pf), _f = de({}, er, { clipboardData: function(e) {
  return "clipboardData" in e ? e.clipboardData : window.clipboardData;
} }), Rf = tt(_f), Tf = de({}, er, { data: 0 }), ls = tt(Tf), zf = {
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
}, Df = {
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
}, Lf = { Alt: "altKey", Control: "ctrlKey", Meta: "metaKey", Shift: "shiftKey" };
function Mf(e) {
  var t = this.nativeEvent;
  return t.getModifierState ? t.getModifierState(e) : (e = Lf[e]) ? !!t[e] : !1;
}
function eo() {
  return Mf;
}
var $f = de({}, Br, { key: function(e) {
  if (e.key) {
    var t = zf[e.key] || e.key;
    if (t !== "Unidentified") return t;
  }
  return e.type === "keypress" ? (e = ha(e), e === 13 ? "Enter" : String.fromCharCode(e)) : e.type === "keydown" || e.type === "keyup" ? Df[e.keyCode] || "Unidentified" : "";
}, code: 0, location: 0, ctrlKey: 0, shiftKey: 0, altKey: 0, metaKey: 0, repeat: 0, locale: 0, getModifierState: eo, charCode: function(e) {
  return e.type === "keypress" ? ha(e) : 0;
}, keyCode: function(e) {
  return e.type === "keydown" || e.type === "keyup" ? e.keyCode : 0;
}, which: function(e) {
  return e.type === "keypress" ? ha(e) : e.type === "keydown" || e.type === "keyup" ? e.keyCode : 0;
} }), Af = tt($f), Of = de({}, qa, { pointerId: 0, width: 0, height: 0, pressure: 0, tangentialPressure: 0, tiltX: 0, tiltY: 0, twist: 0, pointerType: 0, isPrimary: 0 }), is = tt(Of), Uf = de({}, Br, { touches: 0, targetTouches: 0, changedTouches: 0, altKey: 0, metaKey: 0, ctrlKey: 0, shiftKey: 0, getModifierState: eo }), bf = tt(Uf), Vf = de({}, er, { propertyName: 0, elapsedTime: 0, pseudoElement: 0 }), Bf = tt(Vf), Hf = de({}, qa, {
  deltaX: function(e) {
    return "deltaX" in e ? e.deltaX : "wheelDeltaX" in e ? -e.wheelDeltaX : 0;
  },
  deltaY: function(e) {
    return "deltaY" in e ? e.deltaY : "wheelDeltaY" in e ? -e.wheelDeltaY : "wheelDelta" in e ? -e.wheelDelta : 0;
  },
  deltaZ: 0,
  deltaMode: 0
}), Wf = tt(Hf), Qf = [9, 13, 27, 32], to = zt && "CompositionEvent" in window, vr = null;
zt && "documentMode" in document && (vr = document.documentMode);
var Gf = zt && "TextEvent" in window && !vr, Qc = zt && (!to || vr && 8 < vr && 11 >= vr), os = " ", ss = !1;
function Gc(e, t) {
  switch (e) {
    case "keyup":
      return Qf.indexOf(t.keyCode) !== -1;
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
function Kc(e) {
  return e = e.detail, typeof e == "object" && "data" in e ? e.data : null;
}
var Fn = !1;
function Kf(e, t) {
  switch (e) {
    case "compositionend":
      return Kc(t);
    case "keypress":
      return t.which !== 32 ? null : (ss = !0, os);
    case "textInput":
      return e = t.data, e === os && ss ? null : e;
    default:
      return null;
  }
}
function qf(e, t) {
  if (Fn) return e === "compositionend" || !to && Gc(e, t) ? (e = Wc(), ma = Zi = Qt = null, Fn = !1, e) : null;
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
      return Qc && t.locale !== "ko" ? null : t.data;
    default:
      return null;
  }
}
var Yf = { color: !0, date: !0, datetime: !0, "datetime-local": !0, email: !0, month: !0, number: !0, password: !0, range: !0, search: !0, tel: !0, text: !0, time: !0, url: !0, week: !0 };
function cs(e) {
  var t = e && e.nodeName && e.nodeName.toLowerCase();
  return t === "input" ? !!Yf[e.type] : t === "textarea";
}
function qc(e, t, n, r) {
  Ec(r), t = _a(t, "onChange"), 0 < t.length && (n = new Ji("onChange", "change", null, n, r), e.push({ event: n, listeners: t }));
}
var xr = null, _r = null;
function Xf(e) {
  iu(e, 0);
}
function Ya(e) {
  var t = Tn(e);
  if (yc(t)) return e;
}
function Zf(e, t) {
  if (e === "change") return t;
}
var Yc = !1;
if (zt) {
  var wl;
  if (zt) {
    var kl = "oninput" in document;
    if (!kl) {
      var us = document.createElement("div");
      us.setAttribute("oninput", "return;"), kl = typeof us.oninput == "function";
    }
    wl = kl;
  } else wl = !1;
  Yc = wl && (!document.documentMode || 9 < document.documentMode);
}
function ds() {
  xr && (xr.detachEvent("onpropertychange", Xc), _r = xr = null);
}
function Xc(e) {
  if (e.propertyName === "value" && Ya(_r)) {
    var t = [];
    qc(t, _r, e, Gi(e)), _c(Xf, t);
  }
}
function Jf(e, t, n) {
  e === "focusin" ? (ds(), xr = t, _r = n, xr.attachEvent("onpropertychange", Xc)) : e === "focusout" && ds();
}
function ep(e) {
  if (e === "selectionchange" || e === "keyup" || e === "keydown") return Ya(_r);
}
function tp(e, t) {
  if (e === "click") return Ya(t);
}
function np(e, t) {
  if (e === "input" || e === "change") return Ya(t);
}
function rp(e, t) {
  return e === t && (e !== 0 || 1 / e === 1 / t) || e !== e && t !== t;
}
var yt = typeof Object.is == "function" ? Object.is : rp;
function Rr(e, t) {
  if (yt(e, t)) return !0;
  if (typeof e != "object" || e === null || typeof t != "object" || t === null) return !1;
  var n = Object.keys(e), r = Object.keys(t);
  if (n.length !== r.length) return !1;
  for (r = 0; r < n.length; r++) {
    var a = n[r];
    if (!Bl.call(t, a) || !yt(e[a], t[a])) return !1;
  }
  return !0;
}
function fs(e) {
  for (; e && e.firstChild; ) e = e.firstChild;
  return e;
}
function ps(e, t) {
  var n = fs(e);
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
    n = fs(n);
  }
}
function Zc(e, t) {
  return e && t ? e === t ? !0 : e && e.nodeType === 3 ? !1 : t && t.nodeType === 3 ? Zc(e, t.parentNode) : "contains" in e ? e.contains(t) : e.compareDocumentPosition ? !!(e.compareDocumentPosition(t) & 16) : !1 : !1;
}
function Jc() {
  for (var e = window, t = wa(); t instanceof e.HTMLIFrameElement; ) {
    try {
      var n = typeof t.contentWindow.location.href == "string";
    } catch {
      n = !1;
    }
    if (n) e = t.contentWindow;
    else break;
    t = wa(e.document);
  }
  return t;
}
function no(e) {
  var t = e && e.nodeName && e.nodeName.toLowerCase();
  return t && (t === "input" && (e.type === "text" || e.type === "search" || e.type === "tel" || e.type === "url" || e.type === "password") || t === "textarea" || e.contentEditable === "true");
}
function ap(e) {
  var t = Jc(), n = e.focusedElem, r = e.selectionRange;
  if (t !== n && n && n.ownerDocument && Zc(n.ownerDocument.documentElement, n)) {
    if (r !== null && no(n)) {
      if (t = r.start, e = r.end, e === void 0 && (e = t), "selectionStart" in n) n.selectionStart = t, n.selectionEnd = Math.min(e, n.value.length);
      else if (e = (t = n.ownerDocument || document) && t.defaultView || window, e.getSelection) {
        e = e.getSelection();
        var a = n.textContent.length, i = Math.min(r.start, a);
        r = r.end === void 0 ? i : Math.min(r.end, a), !e.extend && i > r && (a = r, r = i, i = a), a = ps(n, i);
        var o = ps(
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
var lp = zt && "documentMode" in document && 11 >= document.documentMode, _n = null, si = null, gr = null, ci = !1;
function ms(e, t, n) {
  var r = n.window === n ? n.document : n.nodeType === 9 ? n : n.ownerDocument;
  ci || _n == null || _n !== wa(r) || (r = _n, "selectionStart" in r && no(r) ? r = { start: r.selectionStart, end: r.selectionEnd } : (r = (r.ownerDocument && r.ownerDocument.defaultView || window).getSelection(), r = { anchorNode: r.anchorNode, anchorOffset: r.anchorOffset, focusNode: r.focusNode, focusOffset: r.focusOffset }), gr && Rr(gr, r) || (gr = r, r = _a(si, "onSelect"), 0 < r.length && (t = new Ji("onSelect", "select", null, t, n), e.push({ event: t, listeners: r }), t.target = _n)));
}
function na(e, t) {
  var n = {};
  return n[e.toLowerCase()] = t.toLowerCase(), n["Webkit" + e] = "webkit" + t, n["Moz" + e] = "moz" + t, n;
}
var Rn = { animationend: na("Animation", "AnimationEnd"), animationiteration: na("Animation", "AnimationIteration"), animationstart: na("Animation", "AnimationStart"), transitionend: na("Transition", "TransitionEnd") }, Cl = {}, eu = {};
zt && (eu = document.createElement("div").style, "AnimationEvent" in window || (delete Rn.animationend.animation, delete Rn.animationiteration.animation, delete Rn.animationstart.animation), "TransitionEvent" in window || delete Rn.transitionend.transition);
function Xa(e) {
  if (Cl[e]) return Cl[e];
  if (!Rn[e]) return e;
  var t = Rn[e], n;
  for (n in t) if (t.hasOwnProperty(n) && n in eu) return Cl[e] = t[n];
  return e;
}
var tu = Xa("animationend"), nu = Xa("animationiteration"), ru = Xa("animationstart"), au = Xa("transitionend"), lu = /* @__PURE__ */ new Map(), hs = "abort auxClick cancel canPlay canPlayThrough click close contextMenu copy cut drag dragEnd dragEnter dragExit dragLeave dragOver dragStart drop durationChange emptied encrypted ended error gotPointerCapture input invalid keyDown keyPress keyUp load loadedData loadedMetadata loadStart lostPointerCapture mouseDown mouseMove mouseOut mouseOver mouseUp paste pause play playing pointerCancel pointerDown pointerMove pointerOut pointerOver pointerUp progress rateChange reset resize seeked seeking stalled submit suspend timeUpdate touchCancel touchEnd touchStart volumeChange scroll toggle touchMove waiting wheel".split(" ");
function ln(e, t) {
  lu.set(e, t), Sn(t, [e]);
}
for (var El = 0; El < hs.length; El++) {
  var Il = hs[El], ip = Il.toLowerCase(), op = Il[0].toUpperCase() + Il.slice(1);
  ln(ip, "on" + op);
}
ln(tu, "onAnimationEnd");
ln(nu, "onAnimationIteration");
ln(ru, "onAnimationStart");
ln("dblclick", "onDoubleClick");
ln("focusin", "onFocus");
ln("focusout", "onBlur");
ln(au, "onTransitionEnd");
Qn("onMouseEnter", ["mouseout", "mouseover"]);
Qn("onMouseLeave", ["mouseout", "mouseover"]);
Qn("onPointerEnter", ["pointerout", "pointerover"]);
Qn("onPointerLeave", ["pointerout", "pointerover"]);
Sn("onChange", "change click focusin focusout input keydown keyup selectionchange".split(" "));
Sn("onSelect", "focusout contextmenu dragend focusin keydown keyup mousedown mouseup selectionchange".split(" "));
Sn("onBeforeInput", ["compositionend", "keypress", "textInput", "paste"]);
Sn("onCompositionEnd", "compositionend focusout keydown keypress keyup mousedown".split(" "));
Sn("onCompositionStart", "compositionstart focusout keydown keypress keyup mousedown".split(" "));
Sn("onCompositionUpdate", "compositionupdate focusout keydown keypress keyup mousedown".split(" "));
var pr = "abort canplay canplaythrough durationchange emptied encrypted ended error loadeddata loadedmetadata loadstart pause play playing progress ratechange resize seeked seeking stalled suspend timeupdate volumechange waiting".split(" "), sp = new Set("cancel close invalid load scroll toggle".split(" ").concat(pr));
function vs(e, t, n) {
  var r = e.type || "unknown-event";
  e.currentTarget = n, lf(r, t, void 0, e), e.currentTarget = null;
}
function iu(e, t) {
  t = (t & 4) !== 0;
  for (var n = 0; n < e.length; n++) {
    var r = e[n], a = r.event;
    r = r.listeners;
    e: {
      var i = void 0;
      if (t) for (var o = r.length - 1; 0 <= o; o--) {
        var s = r[o], c = s.instance, d = s.currentTarget;
        if (s = s.listener, c !== i && a.isPropagationStopped()) break e;
        vs(a, s, d), i = c;
      }
      else for (o = 0; o < r.length; o++) {
        if (s = r[o], c = s.instance, d = s.currentTarget, s = s.listener, c !== i && a.isPropagationStopped()) break e;
        vs(a, s, d), i = c;
      }
    }
  }
  if (Ca) throw e = ai, Ca = !1, ai = null, e;
}
function ae(e, t) {
  var n = t[mi];
  n === void 0 && (n = t[mi] = /* @__PURE__ */ new Set());
  var r = e + "__bubble";
  n.has(r) || (ou(t, e, 2, !1), n.add(r));
}
function Pl(e, t, n) {
  var r = 0;
  t && (r |= 4), ou(n, e, r, t);
}
var ra = "_reactListening" + Math.random().toString(36).slice(2);
function Tr(e) {
  if (!e[ra]) {
    e[ra] = !0, mc.forEach(function(n) {
      n !== "selectionchange" && (sp.has(n) || Pl(n, !1, e), Pl(n, !0, e));
    });
    var t = e.nodeType === 9 ? e : e.ownerDocument;
    t === null || t[ra] || (t[ra] = !0, Pl("selectionchange", !1, t));
  }
}
function ou(e, t, n, r) {
  switch (Hc(t)) {
    case 1:
      var a = Sf;
      break;
    case 4:
      a = wf;
      break;
    default:
      a = Xi;
  }
  n = a.bind(null, t, n, e), a = void 0, !ri || t !== "touchstart" && t !== "touchmove" && t !== "wheel" || (a = !0), r ? a !== void 0 ? e.addEventListener(t, n, { capture: !0, passive: a }) : e.addEventListener(t, n, !0) : a !== void 0 ? e.addEventListener(t, n, { passive: a }) : e.addEventListener(t, n, !1);
}
function Fl(e, t, n, r, a) {
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
        if (o = dn(s), o === null) return;
        if (c = o.tag, c === 5 || c === 6) {
          r = i = o;
          continue e;
        }
        s = s.parentNode;
      }
    }
    r = r.return;
  }
  _c(function() {
    var d = i, N = Gi(n), u = [];
    e: {
      var h = lu.get(e);
      if (h !== void 0) {
        var v = Ji, y = e;
        switch (e) {
          case "keypress":
            if (ha(n) === 0) break e;
          case "keydown":
          case "keyup":
            v = Af;
            break;
          case "focusin":
            y = "focus", v = Sl;
            break;
          case "focusout":
            y = "blur", v = Sl;
            break;
          case "beforeblur":
          case "afterblur":
            v = Sl;
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
            v = as;
            break;
          case "drag":
          case "dragend":
          case "dragenter":
          case "dragexit":
          case "dragleave":
          case "dragover":
          case "dragstart":
          case "drop":
            v = Ef;
            break;
          case "touchcancel":
          case "touchend":
          case "touchmove":
          case "touchstart":
            v = bf;
            break;
          case tu:
          case nu:
          case ru:
            v = Ff;
            break;
          case au:
            v = Bf;
            break;
          case "scroll":
            v = kf;
            break;
          case "wheel":
            v = Wf;
            break;
          case "copy":
          case "cut":
          case "paste":
            v = Rf;
            break;
          case "gotpointercapture":
          case "lostpointercapture":
          case "pointercancel":
          case "pointerdown":
          case "pointermove":
          case "pointerout":
          case "pointerover":
          case "pointerup":
            v = is;
        }
        var S = (t & 4) !== 0, $ = !S && e === "scroll", p = S ? h !== null ? h + "Capture" : null : h;
        S = [];
        for (var f = d, x; f !== null; ) {
          x = f;
          var E = x.stateNode;
          if (x.tag === 5 && E !== null && (x = E, p !== null && (E = Er(f, p), E != null && S.push(zr(f, E, x)))), $) break;
          f = f.return;
        }
        0 < S.length && (h = new v(h, y, null, n, N), u.push({ event: h, listeners: S }));
      }
    }
    if (!(t & 7)) {
      e: {
        if (h = e === "mouseover" || e === "pointerover", v = e === "mouseout" || e === "pointerout", h && n !== ti && (y = n.relatedTarget || n.fromElement) && (dn(y) || y[Dt])) break e;
        if ((v || h) && (h = N.window === N ? N : (h = N.ownerDocument) ? h.defaultView || h.parentWindow : window, v ? (y = n.relatedTarget || n.toElement, v = d, y = y ? dn(y) : null, y !== null && ($ = wn(y), y !== $ || y.tag !== 5 && y.tag !== 6) && (y = null)) : (v = null, y = d), v !== y)) {
          if (S = as, E = "onMouseLeave", p = "onMouseEnter", f = "mouse", (e === "pointerout" || e === "pointerover") && (S = is, E = "onPointerLeave", p = "onPointerEnter", f = "pointer"), $ = v == null ? h : Tn(v), x = y == null ? h : Tn(y), h = new S(E, f + "leave", v, n, N), h.target = $, h.relatedTarget = x, E = null, dn(N) === d && (S = new S(p, f + "enter", y, n, N), S.target = x, S.relatedTarget = $, E = S), $ = E, v && y) t: {
            for (S = v, p = y, f = 0, x = S; x; x = En(x)) f++;
            for (x = 0, E = p; E; E = En(E)) x++;
            for (; 0 < f - x; ) S = En(S), f--;
            for (; 0 < x - f; ) p = En(p), x--;
            for (; f--; ) {
              if (S === p || p !== null && S === p.alternate) break t;
              S = En(S), p = En(p);
            }
            S = null;
          }
          else S = null;
          v !== null && xs(u, h, v, S, !1), y !== null && $ !== null && xs(u, $, y, S, !0);
        }
      }
      e: {
        if (h = d ? Tn(d) : window, v = h.nodeName && h.nodeName.toLowerCase(), v === "select" || v === "input" && h.type === "file") var T = Zf;
        else if (cs(h)) if (Yc) T = np;
        else {
          T = ep;
          var F = Jf;
        }
        else (v = h.nodeName) && v.toLowerCase() === "input" && (h.type === "checkbox" || h.type === "radio") && (T = tp);
        if (T && (T = T(e, d))) {
          qc(u, T, n, N);
          break e;
        }
        F && F(e, h, d), e === "focusout" && (F = h._wrapperState) && F.controlled && h.type === "number" && Yl(h, "number", h.value);
      }
      switch (F = d ? Tn(d) : window, e) {
        case "focusin":
          (cs(F) || F.contentEditable === "true") && (_n = F, si = d, gr = null);
          break;
        case "focusout":
          gr = si = _n = null;
          break;
        case "mousedown":
          ci = !0;
          break;
        case "contextmenu":
        case "mouseup":
        case "dragend":
          ci = !1, ms(u, n, N);
          break;
        case "selectionchange":
          if (lp) break;
        case "keydown":
        case "keyup":
          ms(u, n, N);
      }
      var D;
      if (to) e: {
        switch (e) {
          case "compositionstart":
            var k = "onCompositionStart";
            break e;
          case "compositionend":
            k = "onCompositionEnd";
            break e;
          case "compositionupdate":
            k = "onCompositionUpdate";
            break e;
        }
        k = void 0;
      }
      else Fn ? Gc(e, n) && (k = "onCompositionEnd") : e === "keydown" && n.keyCode === 229 && (k = "onCompositionStart");
      k && (Qc && n.locale !== "ko" && (Fn || k !== "onCompositionStart" ? k === "onCompositionEnd" && Fn && (D = Wc()) : (Qt = N, Zi = "value" in Qt ? Qt.value : Qt.textContent, Fn = !0)), F = _a(d, k), 0 < F.length && (k = new ls(k, e, null, n, N), u.push({ event: k, listeners: F }), D ? k.data = D : (D = Kc(n), D !== null && (k.data = D)))), (D = Gf ? Kf(e, n) : qf(e, n)) && (d = _a(d, "onBeforeInput"), 0 < d.length && (N = new ls("onBeforeInput", "beforeinput", null, n, N), u.push({ event: N, listeners: d }), N.data = D));
    }
    iu(u, t);
  });
}
function zr(e, t, n) {
  return { instance: e, listener: t, currentTarget: n };
}
function _a(e, t) {
  for (var n = t + "Capture", r = []; e !== null; ) {
    var a = e, i = a.stateNode;
    a.tag === 5 && i !== null && (a = i, i = Er(e, n), i != null && r.unshift(zr(e, i, a)), i = Er(e, t), i != null && r.push(zr(e, i, a))), e = e.return;
  }
  return r;
}
function En(e) {
  if (e === null) return null;
  do
    e = e.return;
  while (e && e.tag !== 5);
  return e || null;
}
function xs(e, t, n, r, a) {
  for (var i = t._reactName, o = []; n !== null && n !== r; ) {
    var s = n, c = s.alternate, d = s.stateNode;
    if (c !== null && c === r) break;
    s.tag === 5 && d !== null && (s = d, a ? (c = Er(n, i), c != null && o.unshift(zr(n, c, s))) : a || (c = Er(n, i), c != null && o.push(zr(n, c, s)))), n = n.return;
  }
  o.length !== 0 && e.push({ event: t, listeners: o });
}
var cp = /\r\n?/g, up = /\u0000|\uFFFD/g;
function gs(e) {
  return (typeof e == "string" ? e : "" + e).replace(cp, `
`).replace(up, "");
}
function aa(e, t, n) {
  if (t = gs(t), gs(e) !== t && n) throw Error(P(425));
}
function Ra() {
}
var ui = null, di = null;
function fi(e, t) {
  return e === "textarea" || e === "noscript" || typeof t.children == "string" || typeof t.children == "number" || typeof t.dangerouslySetInnerHTML == "object" && t.dangerouslySetInnerHTML !== null && t.dangerouslySetInnerHTML.__html != null;
}
var pi = typeof setTimeout == "function" ? setTimeout : void 0, dp = typeof clearTimeout == "function" ? clearTimeout : void 0, ys = typeof Promise == "function" ? Promise : void 0, fp = typeof queueMicrotask == "function" ? queueMicrotask : typeof ys < "u" ? function(e) {
  return ys.resolve(null).then(e).catch(pp);
} : pi;
function pp(e) {
  setTimeout(function() {
    throw e;
  });
}
function _l(e, t) {
  var n = t, r = 0;
  do {
    var a = n.nextSibling;
    if (e.removeChild(n), a && a.nodeType === 8) if (n = a.data, n === "/$") {
      if (r === 0) {
        e.removeChild(a), Fr(t);
        return;
      }
      r--;
    } else n !== "$" && n !== "$?" && n !== "$!" || r++;
    n = a;
  } while (n);
  Fr(t);
}
function Xt(e) {
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
function js(e) {
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
var tr = Math.random().toString(36).slice(2), St = "__reactFiber$" + tr, Dr = "__reactProps$" + tr, Dt = "__reactContainer$" + tr, mi = "__reactEvents$" + tr, mp = "__reactListeners$" + tr, hp = "__reactHandles$" + tr;
function dn(e) {
  var t = e[St];
  if (t) return t;
  for (var n = e.parentNode; n; ) {
    if (t = n[Dt] || n[St]) {
      if (n = t.alternate, t.child !== null || n !== null && n.child !== null) for (e = js(e); e !== null; ) {
        if (n = e[St]) return n;
        e = js(e);
      }
      return t;
    }
    e = n, n = e.parentNode;
  }
  return null;
}
function Hr(e) {
  return e = e[St] || e[Dt], !e || e.tag !== 5 && e.tag !== 6 && e.tag !== 13 && e.tag !== 3 ? null : e;
}
function Tn(e) {
  if (e.tag === 5 || e.tag === 6) return e.stateNode;
  throw Error(P(33));
}
function Za(e) {
  return e[Dr] || null;
}
var hi = [], zn = -1;
function on(e) {
  return { current: e };
}
function le(e) {
  0 > zn || (e.current = hi[zn], hi[zn] = null, zn--);
}
function ne(e, t) {
  zn++, hi[zn] = e.current, e.current = t;
}
var rn = {}, De = on(rn), We = on(!1), xn = rn;
function Gn(e, t) {
  var n = e.type.contextTypes;
  if (!n) return rn;
  var r = e.stateNode;
  if (r && r.__reactInternalMemoizedUnmaskedChildContext === t) return r.__reactInternalMemoizedMaskedChildContext;
  var a = {}, i;
  for (i in n) a[i] = t[i];
  return r && (e = e.stateNode, e.__reactInternalMemoizedUnmaskedChildContext = t, e.__reactInternalMemoizedMaskedChildContext = a), a;
}
function Qe(e) {
  return e = e.childContextTypes, e != null;
}
function Ta() {
  le(We), le(De);
}
function Ns(e, t, n) {
  if (De.current !== rn) throw Error(P(168));
  ne(De, t), ne(We, n);
}
function su(e, t, n) {
  var r = e.stateNode;
  if (t = t.childContextTypes, typeof r.getChildContext != "function") return n;
  r = r.getChildContext();
  for (var a in r) if (!(a in t)) throw Error(P(108, Zd(e) || "Unknown", a));
  return de({}, n, r);
}
function za(e) {
  return e = (e = e.stateNode) && e.__reactInternalMemoizedMergedChildContext || rn, xn = De.current, ne(De, e), ne(We, We.current), !0;
}
function Ss(e, t, n) {
  var r = e.stateNode;
  if (!r) throw Error(P(169));
  n ? (e = su(e, t, xn), r.__reactInternalMemoizedMergedChildContext = e, le(We), le(De), ne(De, e)) : le(We), ne(We, n);
}
var Ft = null, Ja = !1, Rl = !1;
function cu(e) {
  Ft === null ? Ft = [e] : Ft.push(e);
}
function vp(e) {
  Ja = !0, cu(e);
}
function sn() {
  if (!Rl && Ft !== null) {
    Rl = !0;
    var e = 0, t = J;
    try {
      var n = Ft;
      for (J = 1; e < n.length; e++) {
        var r = n[e];
        do
          r = r(!0);
        while (r !== null);
      }
      Ft = null, Ja = !1;
    } catch (a) {
      throw Ft !== null && (Ft = Ft.slice(e + 1)), Dc(Ki, sn), a;
    } finally {
      J = t, Rl = !1;
    }
  }
  return null;
}
var Dn = [], Ln = 0, Da = null, La = 0, rt = [], at = 0, gn = null, _t = 1, Rt = "";
function cn(e, t) {
  Dn[Ln++] = La, Dn[Ln++] = Da, Da = e, La = t;
}
function uu(e, t, n) {
  rt[at++] = _t, rt[at++] = Rt, rt[at++] = gn, gn = e;
  var r = _t;
  e = Rt;
  var a = 32 - vt(r) - 1;
  r &= ~(1 << a), n += 1;
  var i = 32 - vt(t) + a;
  if (30 < i) {
    var o = a - a % 5;
    i = (r & (1 << o) - 1).toString(32), r >>= o, a -= o, _t = 1 << 32 - vt(t) + a | n << a | r, Rt = i + e;
  } else _t = 1 << i | n << a | r, Rt = e;
}
function ro(e) {
  e.return !== null && (cn(e, 1), uu(e, 1, 0));
}
function ao(e) {
  for (; e === Da; ) Da = Dn[--Ln], Dn[Ln] = null, La = Dn[--Ln], Dn[Ln] = null;
  for (; e === gn; ) gn = rt[--at], rt[at] = null, Rt = rt[--at], rt[at] = null, _t = rt[--at], rt[at] = null;
}
var Ze = null, Xe = null, oe = !1, ht = null;
function du(e, t) {
  var n = it(5, null, null, 0);
  n.elementType = "DELETED", n.stateNode = t, n.return = e, t = e.deletions, t === null ? (e.deletions = [n], e.flags |= 16) : t.push(n);
}
function ws(e, t) {
  switch (e.tag) {
    case 5:
      var n = e.type;
      return t = t.nodeType !== 1 || n.toLowerCase() !== t.nodeName.toLowerCase() ? null : t, t !== null ? (e.stateNode = t, Ze = e, Xe = Xt(t.firstChild), !0) : !1;
    case 6:
      return t = e.pendingProps === "" || t.nodeType !== 3 ? null : t, t !== null ? (e.stateNode = t, Ze = e, Xe = null, !0) : !1;
    case 13:
      return t = t.nodeType !== 8 ? null : t, t !== null ? (n = gn !== null ? { id: _t, overflow: Rt } : null, e.memoizedState = { dehydrated: t, treeContext: n, retryLane: 1073741824 }, n = it(18, null, null, 0), n.stateNode = t, n.return = e, e.child = n, Ze = e, Xe = null, !0) : !1;
    default:
      return !1;
  }
}
function vi(e) {
  return (e.mode & 1) !== 0 && (e.flags & 128) === 0;
}
function xi(e) {
  if (oe) {
    var t = Xe;
    if (t) {
      var n = t;
      if (!ws(e, t)) {
        if (vi(e)) throw Error(P(418));
        t = Xt(n.nextSibling);
        var r = Ze;
        t && ws(e, t) ? du(r, n) : (e.flags = e.flags & -4097 | 2, oe = !1, Ze = e);
      }
    } else {
      if (vi(e)) throw Error(P(418));
      e.flags = e.flags & -4097 | 2, oe = !1, Ze = e;
    }
  }
}
function ks(e) {
  for (e = e.return; e !== null && e.tag !== 5 && e.tag !== 3 && e.tag !== 13; ) e = e.return;
  Ze = e;
}
function la(e) {
  if (e !== Ze) return !1;
  if (!oe) return ks(e), oe = !0, !1;
  var t;
  if ((t = e.tag !== 3) && !(t = e.tag !== 5) && (t = e.type, t = t !== "head" && t !== "body" && !fi(e.type, e.memoizedProps)), t && (t = Xe)) {
    if (vi(e)) throw fu(), Error(P(418));
    for (; t; ) du(e, t), t = Xt(t.nextSibling);
  }
  if (ks(e), e.tag === 13) {
    if (e = e.memoizedState, e = e !== null ? e.dehydrated : null, !e) throw Error(P(317));
    e: {
      for (e = e.nextSibling, t = 0; e; ) {
        if (e.nodeType === 8) {
          var n = e.data;
          if (n === "/$") {
            if (t === 0) {
              Xe = Xt(e.nextSibling);
              break e;
            }
            t--;
          } else n !== "$" && n !== "$!" && n !== "$?" || t++;
        }
        e = e.nextSibling;
      }
      Xe = null;
    }
  } else Xe = Ze ? Xt(e.stateNode.nextSibling) : null;
  return !0;
}
function fu() {
  for (var e = Xe; e; ) e = Xt(e.nextSibling);
}
function Kn() {
  Xe = Ze = null, oe = !1;
}
function lo(e) {
  ht === null ? ht = [e] : ht.push(e);
}
var xp = $t.ReactCurrentBatchConfig;
function or(e, t, n) {
  if (e = n.ref, e !== null && typeof e != "function" && typeof e != "object") {
    if (n._owner) {
      if (n = n._owner, n) {
        if (n.tag !== 1) throw Error(P(309));
        var r = n.stateNode;
      }
      if (!r) throw Error(P(147, e));
      var a = r, i = "" + e;
      return t !== null && t.ref !== null && typeof t.ref == "function" && t.ref._stringRef === i ? t.ref : (t = function(o) {
        var s = a.refs;
        o === null ? delete s[i] : s[i] = o;
      }, t._stringRef = i, t);
    }
    if (typeof e != "string") throw Error(P(284));
    if (!n._owner) throw Error(P(290, e));
  }
  return e;
}
function ia(e, t) {
  throw e = Object.prototype.toString.call(t), Error(P(31, e === "[object Object]" ? "object with keys {" + Object.keys(t).join(", ") + "}" : e));
}
function Cs(e) {
  var t = e._init;
  return t(e._payload);
}
function pu(e) {
  function t(p, f) {
    if (e) {
      var x = p.deletions;
      x === null ? (p.deletions = [f], p.flags |= 16) : x.push(f);
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
    return p = tn(p, f), p.index = 0, p.sibling = null, p;
  }
  function i(p, f, x) {
    return p.index = x, e ? (x = p.alternate, x !== null ? (x = x.index, x < f ? (p.flags |= 2, f) : x) : (p.flags |= 2, f)) : (p.flags |= 1048576, f);
  }
  function o(p) {
    return e && p.alternate === null && (p.flags |= 2), p;
  }
  function s(p, f, x, E) {
    return f === null || f.tag !== 6 ? (f = Al(x, p.mode, E), f.return = p, f) : (f = a(f, x), f.return = p, f);
  }
  function c(p, f, x, E) {
    var T = x.type;
    return T === Pn ? N(p, f, x.props.children, E, x.key) : f !== null && (f.elementType === T || typeof T == "object" && T !== null && T.$$typeof === Vt && Cs(T) === f.type) ? (E = a(f, x.props), E.ref = or(p, f, x), E.return = p, E) : (E = Sa(x.type, x.key, x.props, null, p.mode, E), E.ref = or(p, f, x), E.return = p, E);
  }
  function d(p, f, x, E) {
    return f === null || f.tag !== 4 || f.stateNode.containerInfo !== x.containerInfo || f.stateNode.implementation !== x.implementation ? (f = Ol(x, p.mode, E), f.return = p, f) : (f = a(f, x.children || []), f.return = p, f);
  }
  function N(p, f, x, E, T) {
    return f === null || f.tag !== 7 ? (f = hn(x, p.mode, E, T), f.return = p, f) : (f = a(f, x), f.return = p, f);
  }
  function u(p, f, x) {
    if (typeof f == "string" && f !== "" || typeof f == "number") return f = Al("" + f, p.mode, x), f.return = p, f;
    if (typeof f == "object" && f !== null) {
      switch (f.$$typeof) {
        case qr:
          return x = Sa(f.type, f.key, f.props, null, p.mode, x), x.ref = or(p, null, f), x.return = p, x;
        case In:
          return f = Ol(f, p.mode, x), f.return = p, f;
        case Vt:
          var E = f._init;
          return u(p, E(f._payload), x);
      }
      if (dr(f) || nr(f)) return f = hn(f, p.mode, x, null), f.return = p, f;
      ia(p, f);
    }
    return null;
  }
  function h(p, f, x, E) {
    var T = f !== null ? f.key : null;
    if (typeof x == "string" && x !== "" || typeof x == "number") return T !== null ? null : s(p, f, "" + x, E);
    if (typeof x == "object" && x !== null) {
      switch (x.$$typeof) {
        case qr:
          return x.key === T ? c(p, f, x, E) : null;
        case In:
          return x.key === T ? d(p, f, x, E) : null;
        case Vt:
          return T = x._init, h(
            p,
            f,
            T(x._payload),
            E
          );
      }
      if (dr(x) || nr(x)) return T !== null ? null : N(p, f, x, E, null);
      ia(p, x);
    }
    return null;
  }
  function v(p, f, x, E, T) {
    if (typeof E == "string" && E !== "" || typeof E == "number") return p = p.get(x) || null, s(f, p, "" + E, T);
    if (typeof E == "object" && E !== null) {
      switch (E.$$typeof) {
        case qr:
          return p = p.get(E.key === null ? x : E.key) || null, c(f, p, E, T);
        case In:
          return p = p.get(E.key === null ? x : E.key) || null, d(f, p, E, T);
        case Vt:
          var F = E._init;
          return v(p, f, x, F(E._payload), T);
      }
      if (dr(E) || nr(E)) return p = p.get(x) || null, N(f, p, E, T, null);
      ia(f, E);
    }
    return null;
  }
  function y(p, f, x, E) {
    for (var T = null, F = null, D = f, k = f = 0, M = null; D !== null && k < x.length; k++) {
      D.index > k ? (M = D, D = null) : M = D.sibling;
      var O = h(p, D, x[k], E);
      if (O === null) {
        D === null && (D = M);
        break;
      }
      e && D && O.alternate === null && t(p, D), f = i(O, f, k), F === null ? T = O : F.sibling = O, F = O, D = M;
    }
    if (k === x.length) return n(p, D), oe && cn(p, k), T;
    if (D === null) {
      for (; k < x.length; k++) D = u(p, x[k], E), D !== null && (f = i(D, f, k), F === null ? T = D : F.sibling = D, F = D);
      return oe && cn(p, k), T;
    }
    for (D = r(p, D); k < x.length; k++) M = v(D, p, k, x[k], E), M !== null && (e && M.alternate !== null && D.delete(M.key === null ? k : M.key), f = i(M, f, k), F === null ? T = M : F.sibling = M, F = M);
    return e && D.forEach(function(R) {
      return t(p, R);
    }), oe && cn(p, k), T;
  }
  function S(p, f, x, E) {
    var T = nr(x);
    if (typeof T != "function") throw Error(P(150));
    if (x = T.call(x), x == null) throw Error(P(151));
    for (var F = T = null, D = f, k = f = 0, M = null, O = x.next(); D !== null && !O.done; k++, O = x.next()) {
      D.index > k ? (M = D, D = null) : M = D.sibling;
      var R = h(p, D, O.value, E);
      if (R === null) {
        D === null && (D = M);
        break;
      }
      e && D && R.alternate === null && t(p, D), f = i(R, f, k), F === null ? T = R : F.sibling = R, F = R, D = M;
    }
    if (O.done) return n(
      p,
      D
    ), oe && cn(p, k), T;
    if (D === null) {
      for (; !O.done; k++, O = x.next()) O = u(p, O.value, E), O !== null && (f = i(O, f, k), F === null ? T = O : F.sibling = O, F = O);
      return oe && cn(p, k), T;
    }
    for (D = r(p, D); !O.done; k++, O = x.next()) O = v(D, p, k, O.value, E), O !== null && (e && O.alternate !== null && D.delete(O.key === null ? k : O.key), f = i(O, f, k), F === null ? T = O : F.sibling = O, F = O);
    return e && D.forEach(function(Q) {
      return t(p, Q);
    }), oe && cn(p, k), T;
  }
  function $(p, f, x, E) {
    if (typeof x == "object" && x !== null && x.type === Pn && x.key === null && (x = x.props.children), typeof x == "object" && x !== null) {
      switch (x.$$typeof) {
        case qr:
          e: {
            for (var T = x.key, F = f; F !== null; ) {
              if (F.key === T) {
                if (T = x.type, T === Pn) {
                  if (F.tag === 7) {
                    n(p, F.sibling), f = a(F, x.props.children), f.return = p, p = f;
                    break e;
                  }
                } else if (F.elementType === T || typeof T == "object" && T !== null && T.$$typeof === Vt && Cs(T) === F.type) {
                  n(p, F.sibling), f = a(F, x.props), f.ref = or(p, F, x), f.return = p, p = f;
                  break e;
                }
                n(p, F);
                break;
              } else t(p, F);
              F = F.sibling;
            }
            x.type === Pn ? (f = hn(x.props.children, p.mode, E, x.key), f.return = p, p = f) : (E = Sa(x.type, x.key, x.props, null, p.mode, E), E.ref = or(p, f, x), E.return = p, p = E);
          }
          return o(p);
        case In:
          e: {
            for (F = x.key; f !== null; ) {
              if (f.key === F) if (f.tag === 4 && f.stateNode.containerInfo === x.containerInfo && f.stateNode.implementation === x.implementation) {
                n(p, f.sibling), f = a(f, x.children || []), f.return = p, p = f;
                break e;
              } else {
                n(p, f);
                break;
              }
              else t(p, f);
              f = f.sibling;
            }
            f = Ol(x, p.mode, E), f.return = p, p = f;
          }
          return o(p);
        case Vt:
          return F = x._init, $(p, f, F(x._payload), E);
      }
      if (dr(x)) return y(p, f, x, E);
      if (nr(x)) return S(p, f, x, E);
      ia(p, x);
    }
    return typeof x == "string" && x !== "" || typeof x == "number" ? (x = "" + x, f !== null && f.tag === 6 ? (n(p, f.sibling), f = a(f, x), f.return = p, p = f) : (n(p, f), f = Al(x, p.mode, E), f.return = p, p = f), o(p)) : n(p, f);
  }
  return $;
}
var qn = pu(!0), mu = pu(!1), Ma = on(null), $a = null, Mn = null, io = null;
function oo() {
  io = Mn = $a = null;
}
function so(e) {
  var t = Ma.current;
  le(Ma), e._currentValue = t;
}
function gi(e, t, n) {
  for (; e !== null; ) {
    var r = e.alternate;
    if ((e.childLanes & t) !== t ? (e.childLanes |= t, r !== null && (r.childLanes |= t)) : r !== null && (r.childLanes & t) !== t && (r.childLanes |= t), e === n) break;
    e = e.return;
  }
}
function Bn(e, t) {
  $a = e, io = Mn = null, e = e.dependencies, e !== null && e.firstContext !== null && (e.lanes & t && (He = !0), e.firstContext = null);
}
function st(e) {
  var t = e._currentValue;
  if (io !== e) if (e = { context: e, memoizedValue: t, next: null }, Mn === null) {
    if ($a === null) throw Error(P(308));
    Mn = e, $a.dependencies = { lanes: 0, firstContext: e };
  } else Mn = Mn.next = e;
  return t;
}
var fn = null;
function co(e) {
  fn === null ? fn = [e] : fn.push(e);
}
function hu(e, t, n, r) {
  var a = t.interleaved;
  return a === null ? (n.next = n, co(t)) : (n.next = a.next, a.next = n), t.interleaved = n, Lt(e, r);
}
function Lt(e, t) {
  e.lanes |= t;
  var n = e.alternate;
  for (n !== null && (n.lanes |= t), n = e, e = e.return; e !== null; ) e.childLanes |= t, n = e.alternate, n !== null && (n.childLanes |= t), n = e, e = e.return;
  return n.tag === 3 ? n.stateNode : null;
}
var Bt = !1;
function uo(e) {
  e.updateQueue = { baseState: e.memoizedState, firstBaseUpdate: null, lastBaseUpdate: null, shared: { pending: null, interleaved: null, lanes: 0 }, effects: null };
}
function vu(e, t) {
  e = e.updateQueue, t.updateQueue === e && (t.updateQueue = { baseState: e.baseState, firstBaseUpdate: e.firstBaseUpdate, lastBaseUpdate: e.lastBaseUpdate, shared: e.shared, effects: e.effects });
}
function Tt(e, t) {
  return { eventTime: e, lane: t, tag: 0, payload: null, callback: null, next: null };
}
function Zt(e, t, n) {
  var r = e.updateQueue;
  if (r === null) return null;
  if (r = r.shared, q & 2) {
    var a = r.pending;
    return a === null ? t.next = t : (t.next = a.next, a.next = t), r.pending = t, Lt(e, n);
  }
  return a = r.interleaved, a === null ? (t.next = t, co(r)) : (t.next = a.next, a.next = t), r.interleaved = t, Lt(e, n);
}
function va(e, t, n) {
  if (t = t.updateQueue, t !== null && (t = t.shared, (n & 4194240) !== 0)) {
    var r = t.lanes;
    r &= e.pendingLanes, n |= r, t.lanes = n, qi(e, n);
  }
}
function Es(e, t) {
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
function Aa(e, t, n, r) {
  var a = e.updateQueue;
  Bt = !1;
  var i = a.firstBaseUpdate, o = a.lastBaseUpdate, s = a.shared.pending;
  if (s !== null) {
    a.shared.pending = null;
    var c = s, d = c.next;
    c.next = null, o === null ? i = d : o.next = d, o = c;
    var N = e.alternate;
    N !== null && (N = N.updateQueue, s = N.lastBaseUpdate, s !== o && (s === null ? N.firstBaseUpdate = d : s.next = d, N.lastBaseUpdate = c));
  }
  if (i !== null) {
    var u = a.baseState;
    o = 0, N = d = c = null, s = i;
    do {
      var h = s.lane, v = s.eventTime;
      if ((r & h) === h) {
        N !== null && (N = N.next = {
          eventTime: v,
          lane: 0,
          tag: s.tag,
          payload: s.payload,
          callback: s.callback,
          next: null
        });
        e: {
          var y = e, S = s;
          switch (h = t, v = n, S.tag) {
            case 1:
              if (y = S.payload, typeof y == "function") {
                u = y.call(v, u, h);
                break e;
              }
              u = y;
              break e;
            case 3:
              y.flags = y.flags & -65537 | 128;
            case 0:
              if (y = S.payload, h = typeof y == "function" ? y.call(v, u, h) : y, h == null) break e;
              u = de({}, u, h);
              break e;
            case 2:
              Bt = !0;
          }
        }
        s.callback !== null && s.lane !== 0 && (e.flags |= 64, h = a.effects, h === null ? a.effects = [s] : h.push(s));
      } else v = { eventTime: v, lane: h, tag: s.tag, payload: s.payload, callback: s.callback, next: null }, N === null ? (d = N = v, c = u) : N = N.next = v, o |= h;
      if (s = s.next, s === null) {
        if (s = a.shared.pending, s === null) break;
        h = s, s = h.next, h.next = null, a.lastBaseUpdate = h, a.shared.pending = null;
      }
    } while (!0);
    if (N === null && (c = u), a.baseState = c, a.firstBaseUpdate = d, a.lastBaseUpdate = N, t = a.shared.interleaved, t !== null) {
      a = t;
      do
        o |= a.lane, a = a.next;
      while (a !== t);
    } else i === null && (a.shared.lanes = 0);
    jn |= o, e.lanes = o, e.memoizedState = u;
  }
}
function Is(e, t, n) {
  if (e = t.effects, t.effects = null, e !== null) for (t = 0; t < e.length; t++) {
    var r = e[t], a = r.callback;
    if (a !== null) {
      if (r.callback = null, r = n, typeof a != "function") throw Error(P(191, a));
      a.call(r);
    }
  }
}
var Wr = {}, kt = on(Wr), Lr = on(Wr), Mr = on(Wr);
function pn(e) {
  if (e === Wr) throw Error(P(174));
  return e;
}
function fo(e, t) {
  switch (ne(Mr, t), ne(Lr, e), ne(kt, Wr), e = t.nodeType, e) {
    case 9:
    case 11:
      t = (t = t.documentElement) ? t.namespaceURI : Zl(null, "");
      break;
    default:
      e = e === 8 ? t.parentNode : t, t = e.namespaceURI || null, e = e.tagName, t = Zl(t, e);
  }
  le(kt), ne(kt, t);
}
function Yn() {
  le(kt), le(Lr), le(Mr);
}
function xu(e) {
  pn(Mr.current);
  var t = pn(kt.current), n = Zl(t, e.type);
  t !== n && (ne(Lr, e), ne(kt, n));
}
function po(e) {
  Lr.current === e && (le(kt), le(Lr));
}
var ce = on(0);
function Oa(e) {
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
var Tl = [];
function mo() {
  for (var e = 0; e < Tl.length; e++) Tl[e]._workInProgressVersionPrimary = null;
  Tl.length = 0;
}
var xa = $t.ReactCurrentDispatcher, zl = $t.ReactCurrentBatchConfig, yn = 0, ue = null, ye = null, we = null, Ua = !1, yr = !1, $r = 0, gp = 0;
function _e() {
  throw Error(P(321));
}
function ho(e, t) {
  if (t === null) return !1;
  for (var n = 0; n < t.length && n < e.length; n++) if (!yt(e[n], t[n])) return !1;
  return !0;
}
function vo(e, t, n, r, a, i) {
  if (yn = i, ue = t, t.memoizedState = null, t.updateQueue = null, t.lanes = 0, xa.current = e === null || e.memoizedState === null ? Sp : wp, e = n(r, a), yr) {
    i = 0;
    do {
      if (yr = !1, $r = 0, 25 <= i) throw Error(P(301));
      i += 1, we = ye = null, t.updateQueue = null, xa.current = kp, e = n(r, a);
    } while (yr);
  }
  if (xa.current = ba, t = ye !== null && ye.next !== null, yn = 0, we = ye = ue = null, Ua = !1, t) throw Error(P(300));
  return e;
}
function xo() {
  var e = $r !== 0;
  return $r = 0, e;
}
function Nt() {
  var e = { memoizedState: null, baseState: null, baseQueue: null, queue: null, next: null };
  return we === null ? ue.memoizedState = we = e : we = we.next = e, we;
}
function ct() {
  if (ye === null) {
    var e = ue.alternate;
    e = e !== null ? e.memoizedState : null;
  } else e = ye.next;
  var t = we === null ? ue.memoizedState : we.next;
  if (t !== null) we = t, ye = e;
  else {
    if (e === null) throw Error(P(310));
    ye = e, e = { memoizedState: ye.memoizedState, baseState: ye.baseState, baseQueue: ye.baseQueue, queue: ye.queue, next: null }, we === null ? ue.memoizedState = we = e : we = we.next = e;
  }
  return we;
}
function Ar(e, t) {
  return typeof t == "function" ? t(e) : t;
}
function Dl(e) {
  var t = ct(), n = t.queue;
  if (n === null) throw Error(P(311));
  n.lastRenderedReducer = e;
  var r = ye, a = r.baseQueue, i = n.pending;
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
      var N = d.lane;
      if ((yn & N) === N) c !== null && (c = c.next = { lane: 0, action: d.action, hasEagerState: d.hasEagerState, eagerState: d.eagerState, next: null }), r = d.hasEagerState ? d.eagerState : e(r, d.action);
      else {
        var u = {
          lane: N,
          action: d.action,
          hasEagerState: d.hasEagerState,
          eagerState: d.eagerState,
          next: null
        };
        c === null ? (s = c = u, o = r) : c = c.next = u, ue.lanes |= N, jn |= N;
      }
      d = d.next;
    } while (d !== null && d !== i);
    c === null ? o = r : c.next = s, yt(r, t.memoizedState) || (He = !0), t.memoizedState = r, t.baseState = o, t.baseQueue = c, n.lastRenderedState = r;
  }
  if (e = n.interleaved, e !== null) {
    a = e;
    do
      i = a.lane, ue.lanes |= i, jn |= i, a = a.next;
    while (a !== e);
  } else a === null && (n.lanes = 0);
  return [t.memoizedState, n.dispatch];
}
function Ll(e) {
  var t = ct(), n = t.queue;
  if (n === null) throw Error(P(311));
  n.lastRenderedReducer = e;
  var r = n.dispatch, a = n.pending, i = t.memoizedState;
  if (a !== null) {
    n.pending = null;
    var o = a = a.next;
    do
      i = e(i, o.action), o = o.next;
    while (o !== a);
    yt(i, t.memoizedState) || (He = !0), t.memoizedState = i, t.baseQueue === null && (t.baseState = i), n.lastRenderedState = i;
  }
  return [i, r];
}
function gu() {
}
function yu(e, t) {
  var n = ue, r = ct(), a = t(), i = !yt(r.memoizedState, a);
  if (i && (r.memoizedState = a, He = !0), r = r.queue, go(Su.bind(null, n, r, e), [e]), r.getSnapshot !== t || i || we !== null && we.memoizedState.tag & 1) {
    if (n.flags |= 2048, Or(9, Nu.bind(null, n, r, a, t), void 0, null), Ce === null) throw Error(P(349));
    yn & 30 || ju(n, t, a);
  }
  return a;
}
function ju(e, t, n) {
  e.flags |= 16384, e = { getSnapshot: t, value: n }, t = ue.updateQueue, t === null ? (t = { lastEffect: null, stores: null }, ue.updateQueue = t, t.stores = [e]) : (n = t.stores, n === null ? t.stores = [e] : n.push(e));
}
function Nu(e, t, n, r) {
  t.value = n, t.getSnapshot = r, wu(t) && ku(e);
}
function Su(e, t, n) {
  return n(function() {
    wu(t) && ku(e);
  });
}
function wu(e) {
  var t = e.getSnapshot;
  e = e.value;
  try {
    var n = t();
    return !yt(e, n);
  } catch {
    return !0;
  }
}
function ku(e) {
  var t = Lt(e, 1);
  t !== null && xt(t, e, 1, -1);
}
function Ps(e) {
  var t = Nt();
  return typeof e == "function" && (e = e()), t.memoizedState = t.baseState = e, e = { pending: null, interleaved: null, lanes: 0, dispatch: null, lastRenderedReducer: Ar, lastRenderedState: e }, t.queue = e, e = e.dispatch = Np.bind(null, ue, e), [t.memoizedState, e];
}
function Or(e, t, n, r) {
  return e = { tag: e, create: t, destroy: n, deps: r, next: null }, t = ue.updateQueue, t === null ? (t = { lastEffect: null, stores: null }, ue.updateQueue = t, t.lastEffect = e.next = e) : (n = t.lastEffect, n === null ? t.lastEffect = e.next = e : (r = n.next, n.next = e, e.next = r, t.lastEffect = e)), e;
}
function Cu() {
  return ct().memoizedState;
}
function ga(e, t, n, r) {
  var a = Nt();
  ue.flags |= e, a.memoizedState = Or(1 | t, n, void 0, r === void 0 ? null : r);
}
function el(e, t, n, r) {
  var a = ct();
  r = r === void 0 ? null : r;
  var i = void 0;
  if (ye !== null) {
    var o = ye.memoizedState;
    if (i = o.destroy, r !== null && ho(r, o.deps)) {
      a.memoizedState = Or(t, n, i, r);
      return;
    }
  }
  ue.flags |= e, a.memoizedState = Or(1 | t, n, i, r);
}
function Fs(e, t) {
  return ga(8390656, 8, e, t);
}
function go(e, t) {
  return el(2048, 8, e, t);
}
function Eu(e, t) {
  return el(4, 2, e, t);
}
function Iu(e, t) {
  return el(4, 4, e, t);
}
function Pu(e, t) {
  if (typeof t == "function") return e = e(), t(e), function() {
    t(null);
  };
  if (t != null) return e = e(), t.current = e, function() {
    t.current = null;
  };
}
function Fu(e, t, n) {
  return n = n != null ? n.concat([e]) : null, el(4, 4, Pu.bind(null, t, e), n);
}
function yo() {
}
function _u(e, t) {
  var n = ct();
  t = t === void 0 ? null : t;
  var r = n.memoizedState;
  return r !== null && t !== null && ho(t, r[1]) ? r[0] : (n.memoizedState = [e, t], e);
}
function Ru(e, t) {
  var n = ct();
  t = t === void 0 ? null : t;
  var r = n.memoizedState;
  return r !== null && t !== null && ho(t, r[1]) ? r[0] : (e = e(), n.memoizedState = [e, t], e);
}
function Tu(e, t, n) {
  return yn & 21 ? (yt(n, t) || (n = $c(), ue.lanes |= n, jn |= n, e.baseState = !0), t) : (e.baseState && (e.baseState = !1, He = !0), e.memoizedState = n);
}
function yp(e, t) {
  var n = J;
  J = n !== 0 && 4 > n ? n : 4, e(!0);
  var r = zl.transition;
  zl.transition = {};
  try {
    e(!1), t();
  } finally {
    J = n, zl.transition = r;
  }
}
function zu() {
  return ct().memoizedState;
}
function jp(e, t, n) {
  var r = en(e);
  if (n = { lane: r, action: n, hasEagerState: !1, eagerState: null, next: null }, Du(e)) Lu(t, n);
  else if (n = hu(e, t, n, r), n !== null) {
    var a = Oe();
    xt(n, e, r, a), Mu(n, t, r);
  }
}
function Np(e, t, n) {
  var r = en(e), a = { lane: r, action: n, hasEagerState: !1, eagerState: null, next: null };
  if (Du(e)) Lu(t, a);
  else {
    var i = e.alternate;
    if (e.lanes === 0 && (i === null || i.lanes === 0) && (i = t.lastRenderedReducer, i !== null)) try {
      var o = t.lastRenderedState, s = i(o, n);
      if (a.hasEagerState = !0, a.eagerState = s, yt(s, o)) {
        var c = t.interleaved;
        c === null ? (a.next = a, co(t)) : (a.next = c.next, c.next = a), t.interleaved = a;
        return;
      }
    } catch {
    } finally {
    }
    n = hu(e, t, a, r), n !== null && (a = Oe(), xt(n, e, r, a), Mu(n, t, r));
  }
}
function Du(e) {
  var t = e.alternate;
  return e === ue || t !== null && t === ue;
}
function Lu(e, t) {
  yr = Ua = !0;
  var n = e.pending;
  n === null ? t.next = t : (t.next = n.next, n.next = t), e.pending = t;
}
function Mu(e, t, n) {
  if (n & 4194240) {
    var r = t.lanes;
    r &= e.pendingLanes, n |= r, t.lanes = n, qi(e, n);
  }
}
var ba = { readContext: st, useCallback: _e, useContext: _e, useEffect: _e, useImperativeHandle: _e, useInsertionEffect: _e, useLayoutEffect: _e, useMemo: _e, useReducer: _e, useRef: _e, useState: _e, useDebugValue: _e, useDeferredValue: _e, useTransition: _e, useMutableSource: _e, useSyncExternalStore: _e, useId: _e, unstable_isNewReconciler: !1 }, Sp = { readContext: st, useCallback: function(e, t) {
  return Nt().memoizedState = [e, t === void 0 ? null : t], e;
}, useContext: st, useEffect: Fs, useImperativeHandle: function(e, t, n) {
  return n = n != null ? n.concat([e]) : null, ga(
    4194308,
    4,
    Pu.bind(null, t, e),
    n
  );
}, useLayoutEffect: function(e, t) {
  return ga(4194308, 4, e, t);
}, useInsertionEffect: function(e, t) {
  return ga(4, 2, e, t);
}, useMemo: function(e, t) {
  var n = Nt();
  return t = t === void 0 ? null : t, e = e(), n.memoizedState = [e, t], e;
}, useReducer: function(e, t, n) {
  var r = Nt();
  return t = n !== void 0 ? n(t) : t, r.memoizedState = r.baseState = t, e = { pending: null, interleaved: null, lanes: 0, dispatch: null, lastRenderedReducer: e, lastRenderedState: t }, r.queue = e, e = e.dispatch = jp.bind(null, ue, e), [r.memoizedState, e];
}, useRef: function(e) {
  var t = Nt();
  return e = { current: e }, t.memoizedState = e;
}, useState: Ps, useDebugValue: yo, useDeferredValue: function(e) {
  return Nt().memoizedState = e;
}, useTransition: function() {
  var e = Ps(!1), t = e[0];
  return e = yp.bind(null, e[1]), Nt().memoizedState = e, [t, e];
}, useMutableSource: function() {
}, useSyncExternalStore: function(e, t, n) {
  var r = ue, a = Nt();
  if (oe) {
    if (n === void 0) throw Error(P(407));
    n = n();
  } else {
    if (n = t(), Ce === null) throw Error(P(349));
    yn & 30 || ju(r, t, n);
  }
  a.memoizedState = n;
  var i = { value: n, getSnapshot: t };
  return a.queue = i, Fs(Su.bind(
    null,
    r,
    i,
    e
  ), [e]), r.flags |= 2048, Or(9, Nu.bind(null, r, i, n, t), void 0, null), n;
}, useId: function() {
  var e = Nt(), t = Ce.identifierPrefix;
  if (oe) {
    var n = Rt, r = _t;
    n = (r & ~(1 << 32 - vt(r) - 1)).toString(32) + n, t = ":" + t + "R" + n, n = $r++, 0 < n && (t += "H" + n.toString(32)), t += ":";
  } else n = gp++, t = ":" + t + "r" + n.toString(32) + ":";
  return e.memoizedState = t;
}, unstable_isNewReconciler: !1 }, wp = {
  readContext: st,
  useCallback: _u,
  useContext: st,
  useEffect: go,
  useImperativeHandle: Fu,
  useInsertionEffect: Eu,
  useLayoutEffect: Iu,
  useMemo: Ru,
  useReducer: Dl,
  useRef: Cu,
  useState: function() {
    return Dl(Ar);
  },
  useDebugValue: yo,
  useDeferredValue: function(e) {
    var t = ct();
    return Tu(t, ye.memoizedState, e);
  },
  useTransition: function() {
    var e = Dl(Ar)[0], t = ct().memoizedState;
    return [e, t];
  },
  useMutableSource: gu,
  useSyncExternalStore: yu,
  useId: zu,
  unstable_isNewReconciler: !1
}, kp = { readContext: st, useCallback: _u, useContext: st, useEffect: go, useImperativeHandle: Fu, useInsertionEffect: Eu, useLayoutEffect: Iu, useMemo: Ru, useReducer: Ll, useRef: Cu, useState: function() {
  return Ll(Ar);
}, useDebugValue: yo, useDeferredValue: function(e) {
  var t = ct();
  return ye === null ? t.memoizedState = e : Tu(t, ye.memoizedState, e);
}, useTransition: function() {
  var e = Ll(Ar)[0], t = ct().memoizedState;
  return [e, t];
}, useMutableSource: gu, useSyncExternalStore: yu, useId: zu, unstable_isNewReconciler: !1 };
function pt(e, t) {
  if (e && e.defaultProps) {
    t = de({}, t), e = e.defaultProps;
    for (var n in e) t[n] === void 0 && (t[n] = e[n]);
    return t;
  }
  return t;
}
function yi(e, t, n, r) {
  t = e.memoizedState, n = n(r, t), n = n == null ? t : de({}, t, n), e.memoizedState = n, e.lanes === 0 && (e.updateQueue.baseState = n);
}
var tl = { isMounted: function(e) {
  return (e = e._reactInternals) ? wn(e) === e : !1;
}, enqueueSetState: function(e, t, n) {
  e = e._reactInternals;
  var r = Oe(), a = en(e), i = Tt(r, a);
  i.payload = t, n != null && (i.callback = n), t = Zt(e, i, a), t !== null && (xt(t, e, a, r), va(t, e, a));
}, enqueueReplaceState: function(e, t, n) {
  e = e._reactInternals;
  var r = Oe(), a = en(e), i = Tt(r, a);
  i.tag = 1, i.payload = t, n != null && (i.callback = n), t = Zt(e, i, a), t !== null && (xt(t, e, a, r), va(t, e, a));
}, enqueueForceUpdate: function(e, t) {
  e = e._reactInternals;
  var n = Oe(), r = en(e), a = Tt(n, r);
  a.tag = 2, t != null && (a.callback = t), t = Zt(e, a, r), t !== null && (xt(t, e, r, n), va(t, e, r));
} };
function _s(e, t, n, r, a, i, o) {
  return e = e.stateNode, typeof e.shouldComponentUpdate == "function" ? e.shouldComponentUpdate(r, i, o) : t.prototype && t.prototype.isPureReactComponent ? !Rr(n, r) || !Rr(a, i) : !0;
}
function $u(e, t, n) {
  var r = !1, a = rn, i = t.contextType;
  return typeof i == "object" && i !== null ? i = st(i) : (a = Qe(t) ? xn : De.current, r = t.contextTypes, i = (r = r != null) ? Gn(e, a) : rn), t = new t(n, i), e.memoizedState = t.state !== null && t.state !== void 0 ? t.state : null, t.updater = tl, e.stateNode = t, t._reactInternals = e, r && (e = e.stateNode, e.__reactInternalMemoizedUnmaskedChildContext = a, e.__reactInternalMemoizedMaskedChildContext = i), t;
}
function Rs(e, t, n, r) {
  e = t.state, typeof t.componentWillReceiveProps == "function" && t.componentWillReceiveProps(n, r), typeof t.UNSAFE_componentWillReceiveProps == "function" && t.UNSAFE_componentWillReceiveProps(n, r), t.state !== e && tl.enqueueReplaceState(t, t.state, null);
}
function ji(e, t, n, r) {
  var a = e.stateNode;
  a.props = n, a.state = e.memoizedState, a.refs = {}, uo(e);
  var i = t.contextType;
  typeof i == "object" && i !== null ? a.context = st(i) : (i = Qe(t) ? xn : De.current, a.context = Gn(e, i)), a.state = e.memoizedState, i = t.getDerivedStateFromProps, typeof i == "function" && (yi(e, t, i, n), a.state = e.memoizedState), typeof t.getDerivedStateFromProps == "function" || typeof a.getSnapshotBeforeUpdate == "function" || typeof a.UNSAFE_componentWillMount != "function" && typeof a.componentWillMount != "function" || (t = a.state, typeof a.componentWillMount == "function" && a.componentWillMount(), typeof a.UNSAFE_componentWillMount == "function" && a.UNSAFE_componentWillMount(), t !== a.state && tl.enqueueReplaceState(a, a.state, null), Aa(e, n, a, r), a.state = e.memoizedState), typeof a.componentDidMount == "function" && (e.flags |= 4194308);
}
function Xn(e, t) {
  try {
    var n = "", r = t;
    do
      n += Xd(r), r = r.return;
    while (r);
    var a = n;
  } catch (i) {
    a = `
Error generating stack: ` + i.message + `
` + i.stack;
  }
  return { value: e, source: t, stack: a, digest: null };
}
function Ml(e, t, n) {
  return { value: e, source: null, stack: n ?? null, digest: t ?? null };
}
function Ni(e, t) {
  try {
    console.error(t.value);
  } catch (n) {
    setTimeout(function() {
      throw n;
    });
  }
}
var Cp = typeof WeakMap == "function" ? WeakMap : Map;
function Au(e, t, n) {
  n = Tt(-1, n), n.tag = 3, n.payload = { element: null };
  var r = t.value;
  return n.callback = function() {
    Ba || (Ba = !0, Ri = r), Ni(e, t);
  }, n;
}
function Ou(e, t, n) {
  n = Tt(-1, n), n.tag = 3;
  var r = e.type.getDerivedStateFromError;
  if (typeof r == "function") {
    var a = t.value;
    n.payload = function() {
      return r(a);
    }, n.callback = function() {
      Ni(e, t);
    };
  }
  var i = e.stateNode;
  return i !== null && typeof i.componentDidCatch == "function" && (n.callback = function() {
    Ni(e, t), typeof r != "function" && (Jt === null ? Jt = /* @__PURE__ */ new Set([this]) : Jt.add(this));
    var o = t.stack;
    this.componentDidCatch(t.value, { componentStack: o !== null ? o : "" });
  }), n;
}
function Ts(e, t, n) {
  var r = e.pingCache;
  if (r === null) {
    r = e.pingCache = new Cp();
    var a = /* @__PURE__ */ new Set();
    r.set(t, a);
  } else a = r.get(t), a === void 0 && (a = /* @__PURE__ */ new Set(), r.set(t, a));
  a.has(n) || (a.add(n), e = Op.bind(null, e, t, n), t.then(e, e));
}
function zs(e) {
  do {
    var t;
    if ((t = e.tag === 13) && (t = e.memoizedState, t = t !== null ? t.dehydrated !== null : !0), t) return e;
    e = e.return;
  } while (e !== null);
  return null;
}
function Ds(e, t, n, r, a) {
  return e.mode & 1 ? (e.flags |= 65536, e.lanes = a, e) : (e === t ? e.flags |= 65536 : (e.flags |= 128, n.flags |= 131072, n.flags &= -52805, n.tag === 1 && (n.alternate === null ? n.tag = 17 : (t = Tt(-1, 1), t.tag = 2, Zt(n, t, 1))), n.lanes |= 1), e);
}
var Ep = $t.ReactCurrentOwner, He = !1;
function Me(e, t, n, r) {
  t.child = e === null ? mu(t, null, n, r) : qn(t, e.child, n, r);
}
function Ls(e, t, n, r, a) {
  n = n.render;
  var i = t.ref;
  return Bn(t, a), r = vo(e, t, n, r, i, a), n = xo(), e !== null && !He ? (t.updateQueue = e.updateQueue, t.flags &= -2053, e.lanes &= ~a, Mt(e, t, a)) : (oe && n && ro(t), t.flags |= 1, Me(e, t, r, a), t.child);
}
function Ms(e, t, n, r, a) {
  if (e === null) {
    var i = n.type;
    return typeof i == "function" && !Io(i) && i.defaultProps === void 0 && n.compare === null && n.defaultProps === void 0 ? (t.tag = 15, t.type = i, Uu(e, t, i, r, a)) : (e = Sa(n.type, null, r, t, t.mode, a), e.ref = t.ref, e.return = t, t.child = e);
  }
  if (i = e.child, !(e.lanes & a)) {
    var o = i.memoizedProps;
    if (n = n.compare, n = n !== null ? n : Rr, n(o, r) && e.ref === t.ref) return Mt(e, t, a);
  }
  return t.flags |= 1, e = tn(i, r), e.ref = t.ref, e.return = t, t.child = e;
}
function Uu(e, t, n, r, a) {
  if (e !== null) {
    var i = e.memoizedProps;
    if (Rr(i, r) && e.ref === t.ref) if (He = !1, t.pendingProps = r = i, (e.lanes & a) !== 0) e.flags & 131072 && (He = !0);
    else return t.lanes = e.lanes, Mt(e, t, a);
  }
  return Si(e, t, n, r, a);
}
function bu(e, t, n) {
  var r = t.pendingProps, a = r.children, i = e !== null ? e.memoizedState : null;
  if (r.mode === "hidden") if (!(t.mode & 1)) t.memoizedState = { baseLanes: 0, cachePool: null, transitions: null }, ne(An, Ye), Ye |= n;
  else {
    if (!(n & 1073741824)) return e = i !== null ? i.baseLanes | n : n, t.lanes = t.childLanes = 1073741824, t.memoizedState = { baseLanes: e, cachePool: null, transitions: null }, t.updateQueue = null, ne(An, Ye), Ye |= e, null;
    t.memoizedState = { baseLanes: 0, cachePool: null, transitions: null }, r = i !== null ? i.baseLanes : n, ne(An, Ye), Ye |= r;
  }
  else i !== null ? (r = i.baseLanes | n, t.memoizedState = null) : r = n, ne(An, Ye), Ye |= r;
  return Me(e, t, a, n), t.child;
}
function Vu(e, t) {
  var n = t.ref;
  (e === null && n !== null || e !== null && e.ref !== n) && (t.flags |= 512, t.flags |= 2097152);
}
function Si(e, t, n, r, a) {
  var i = Qe(n) ? xn : De.current;
  return i = Gn(t, i), Bn(t, a), n = vo(e, t, n, r, i, a), r = xo(), e !== null && !He ? (t.updateQueue = e.updateQueue, t.flags &= -2053, e.lanes &= ~a, Mt(e, t, a)) : (oe && r && ro(t), t.flags |= 1, Me(e, t, n, a), t.child);
}
function $s(e, t, n, r, a) {
  if (Qe(n)) {
    var i = !0;
    za(t);
  } else i = !1;
  if (Bn(t, a), t.stateNode === null) ya(e, t), $u(t, n, r), ji(t, n, r, a), r = !0;
  else if (e === null) {
    var o = t.stateNode, s = t.memoizedProps;
    o.props = s;
    var c = o.context, d = n.contextType;
    typeof d == "object" && d !== null ? d = st(d) : (d = Qe(n) ? xn : De.current, d = Gn(t, d));
    var N = n.getDerivedStateFromProps, u = typeof N == "function" || typeof o.getSnapshotBeforeUpdate == "function";
    u || typeof o.UNSAFE_componentWillReceiveProps != "function" && typeof o.componentWillReceiveProps != "function" || (s !== r || c !== d) && Rs(t, o, r, d), Bt = !1;
    var h = t.memoizedState;
    o.state = h, Aa(t, r, o, a), c = t.memoizedState, s !== r || h !== c || We.current || Bt ? (typeof N == "function" && (yi(t, n, N, r), c = t.memoizedState), (s = Bt || _s(t, n, s, r, h, c, d)) ? (u || typeof o.UNSAFE_componentWillMount != "function" && typeof o.componentWillMount != "function" || (typeof o.componentWillMount == "function" && o.componentWillMount(), typeof o.UNSAFE_componentWillMount == "function" && o.UNSAFE_componentWillMount()), typeof o.componentDidMount == "function" && (t.flags |= 4194308)) : (typeof o.componentDidMount == "function" && (t.flags |= 4194308), t.memoizedProps = r, t.memoizedState = c), o.props = r, o.state = c, o.context = d, r = s) : (typeof o.componentDidMount == "function" && (t.flags |= 4194308), r = !1);
  } else {
    o = t.stateNode, vu(e, t), s = t.memoizedProps, d = t.type === t.elementType ? s : pt(t.type, s), o.props = d, u = t.pendingProps, h = o.context, c = n.contextType, typeof c == "object" && c !== null ? c = st(c) : (c = Qe(n) ? xn : De.current, c = Gn(t, c));
    var v = n.getDerivedStateFromProps;
    (N = typeof v == "function" || typeof o.getSnapshotBeforeUpdate == "function") || typeof o.UNSAFE_componentWillReceiveProps != "function" && typeof o.componentWillReceiveProps != "function" || (s !== u || h !== c) && Rs(t, o, r, c), Bt = !1, h = t.memoizedState, o.state = h, Aa(t, r, o, a);
    var y = t.memoizedState;
    s !== u || h !== y || We.current || Bt ? (typeof v == "function" && (yi(t, n, v, r), y = t.memoizedState), (d = Bt || _s(t, n, d, r, h, y, c) || !1) ? (N || typeof o.UNSAFE_componentWillUpdate != "function" && typeof o.componentWillUpdate != "function" || (typeof o.componentWillUpdate == "function" && o.componentWillUpdate(r, y, c), typeof o.UNSAFE_componentWillUpdate == "function" && o.UNSAFE_componentWillUpdate(r, y, c)), typeof o.componentDidUpdate == "function" && (t.flags |= 4), typeof o.getSnapshotBeforeUpdate == "function" && (t.flags |= 1024)) : (typeof o.componentDidUpdate != "function" || s === e.memoizedProps && h === e.memoizedState || (t.flags |= 4), typeof o.getSnapshotBeforeUpdate != "function" || s === e.memoizedProps && h === e.memoizedState || (t.flags |= 1024), t.memoizedProps = r, t.memoizedState = y), o.props = r, o.state = y, o.context = c, r = d) : (typeof o.componentDidUpdate != "function" || s === e.memoizedProps && h === e.memoizedState || (t.flags |= 4), typeof o.getSnapshotBeforeUpdate != "function" || s === e.memoizedProps && h === e.memoizedState || (t.flags |= 1024), r = !1);
  }
  return wi(e, t, n, r, i, a);
}
function wi(e, t, n, r, a, i) {
  Vu(e, t);
  var o = (t.flags & 128) !== 0;
  if (!r && !o) return a && Ss(t, n, !1), Mt(e, t, i);
  r = t.stateNode, Ep.current = t;
  var s = o && typeof n.getDerivedStateFromError != "function" ? null : r.render();
  return t.flags |= 1, e !== null && o ? (t.child = qn(t, e.child, null, i), t.child = qn(t, null, s, i)) : Me(e, t, s, i), t.memoizedState = r.state, a && Ss(t, n, !0), t.child;
}
function Bu(e) {
  var t = e.stateNode;
  t.pendingContext ? Ns(e, t.pendingContext, t.pendingContext !== t.context) : t.context && Ns(e, t.context, !1), fo(e, t.containerInfo);
}
function As(e, t, n, r, a) {
  return Kn(), lo(a), t.flags |= 256, Me(e, t, n, r), t.child;
}
var ki = { dehydrated: null, treeContext: null, retryLane: 0 };
function Ci(e) {
  return { baseLanes: e, cachePool: null, transitions: null };
}
function Hu(e, t, n) {
  var r = t.pendingProps, a = ce.current, i = !1, o = (t.flags & 128) !== 0, s;
  if ((s = o) || (s = e !== null && e.memoizedState === null ? !1 : (a & 2) !== 0), s ? (i = !0, t.flags &= -129) : (e === null || e.memoizedState !== null) && (a |= 1), ne(ce, a & 1), e === null)
    return xi(t), e = t.memoizedState, e !== null && (e = e.dehydrated, e !== null) ? (t.mode & 1 ? e.data === "$!" ? t.lanes = 8 : t.lanes = 1073741824 : t.lanes = 1, null) : (o = r.children, e = r.fallback, i ? (r = t.mode, i = t.child, o = { mode: "hidden", children: o }, !(r & 1) && i !== null ? (i.childLanes = 0, i.pendingProps = o) : i = al(o, r, 0, null), e = hn(e, r, n, null), i.return = t, e.return = t, i.sibling = e, t.child = i, t.child.memoizedState = Ci(n), t.memoizedState = ki, e) : jo(t, o));
  if (a = e.memoizedState, a !== null && (s = a.dehydrated, s !== null)) return Ip(e, t, o, r, s, a, n);
  if (i) {
    i = r.fallback, o = t.mode, a = e.child, s = a.sibling;
    var c = { mode: "hidden", children: r.children };
    return !(o & 1) && t.child !== a ? (r = t.child, r.childLanes = 0, r.pendingProps = c, t.deletions = null) : (r = tn(a, c), r.subtreeFlags = a.subtreeFlags & 14680064), s !== null ? i = tn(s, i) : (i = hn(i, o, n, null), i.flags |= 2), i.return = t, r.return = t, r.sibling = i, t.child = r, r = i, i = t.child, o = e.child.memoizedState, o = o === null ? Ci(n) : { baseLanes: o.baseLanes | n, cachePool: null, transitions: o.transitions }, i.memoizedState = o, i.childLanes = e.childLanes & ~n, t.memoizedState = ki, r;
  }
  return i = e.child, e = i.sibling, r = tn(i, { mode: "visible", children: r.children }), !(t.mode & 1) && (r.lanes = n), r.return = t, r.sibling = null, e !== null && (n = t.deletions, n === null ? (t.deletions = [e], t.flags |= 16) : n.push(e)), t.child = r, t.memoizedState = null, r;
}
function jo(e, t) {
  return t = al({ mode: "visible", children: t }, e.mode, 0, null), t.return = e, e.child = t;
}
function oa(e, t, n, r) {
  return r !== null && lo(r), qn(t, e.child, null, n), e = jo(t, t.pendingProps.children), e.flags |= 2, t.memoizedState = null, e;
}
function Ip(e, t, n, r, a, i, o) {
  if (n)
    return t.flags & 256 ? (t.flags &= -257, r = Ml(Error(P(422))), oa(e, t, o, r)) : t.memoizedState !== null ? (t.child = e.child, t.flags |= 128, null) : (i = r.fallback, a = t.mode, r = al({ mode: "visible", children: r.children }, a, 0, null), i = hn(i, a, o, null), i.flags |= 2, r.return = t, i.return = t, r.sibling = i, t.child = r, t.mode & 1 && qn(t, e.child, null, o), t.child.memoizedState = Ci(o), t.memoizedState = ki, i);
  if (!(t.mode & 1)) return oa(e, t, o, null);
  if (a.data === "$!") {
    if (r = a.nextSibling && a.nextSibling.dataset, r) var s = r.dgst;
    return r = s, i = Error(P(419)), r = Ml(i, r, void 0), oa(e, t, o, r);
  }
  if (s = (o & e.childLanes) !== 0, He || s) {
    if (r = Ce, r !== null) {
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
      a = a & (r.suspendedLanes | o) ? 0 : a, a !== 0 && a !== i.retryLane && (i.retryLane = a, Lt(e, a), xt(r, e, a, -1));
    }
    return Eo(), r = Ml(Error(P(421))), oa(e, t, o, r);
  }
  return a.data === "$?" ? (t.flags |= 128, t.child = e.child, t = Up.bind(null, e), a._reactRetry = t, null) : (e = i.treeContext, Xe = Xt(a.nextSibling), Ze = t, oe = !0, ht = null, e !== null && (rt[at++] = _t, rt[at++] = Rt, rt[at++] = gn, _t = e.id, Rt = e.overflow, gn = t), t = jo(t, r.children), t.flags |= 4096, t);
}
function Os(e, t, n) {
  e.lanes |= t;
  var r = e.alternate;
  r !== null && (r.lanes |= t), gi(e.return, t, n);
}
function $l(e, t, n, r, a) {
  var i = e.memoizedState;
  i === null ? e.memoizedState = { isBackwards: t, rendering: null, renderingStartTime: 0, last: r, tail: n, tailMode: a } : (i.isBackwards = t, i.rendering = null, i.renderingStartTime = 0, i.last = r, i.tail = n, i.tailMode = a);
}
function Wu(e, t, n) {
  var r = t.pendingProps, a = r.revealOrder, i = r.tail;
  if (Me(e, t, r.children, n), r = ce.current, r & 2) r = r & 1 | 2, t.flags |= 128;
  else {
    if (e !== null && e.flags & 128) e: for (e = t.child; e !== null; ) {
      if (e.tag === 13) e.memoizedState !== null && Os(e, n, t);
      else if (e.tag === 19) Os(e, n, t);
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
  if (ne(ce, r), !(t.mode & 1)) t.memoizedState = null;
  else switch (a) {
    case "forwards":
      for (n = t.child, a = null; n !== null; ) e = n.alternate, e !== null && Oa(e) === null && (a = n), n = n.sibling;
      n = a, n === null ? (a = t.child, t.child = null) : (a = n.sibling, n.sibling = null), $l(t, !1, a, n, i);
      break;
    case "backwards":
      for (n = null, a = t.child, t.child = null; a !== null; ) {
        if (e = a.alternate, e !== null && Oa(e) === null) {
          t.child = a;
          break;
        }
        e = a.sibling, a.sibling = n, n = a, a = e;
      }
      $l(t, !0, n, null, i);
      break;
    case "together":
      $l(t, !1, null, null, void 0);
      break;
    default:
      t.memoizedState = null;
  }
  return t.child;
}
function ya(e, t) {
  !(t.mode & 1) && e !== null && (e.alternate = null, t.alternate = null, t.flags |= 2);
}
function Mt(e, t, n) {
  if (e !== null && (t.dependencies = e.dependencies), jn |= t.lanes, !(n & t.childLanes)) return null;
  if (e !== null && t.child !== e.child) throw Error(P(153));
  if (t.child !== null) {
    for (e = t.child, n = tn(e, e.pendingProps), t.child = n, n.return = t; e.sibling !== null; ) e = e.sibling, n = n.sibling = tn(e, e.pendingProps), n.return = t;
    n.sibling = null;
  }
  return t.child;
}
function Pp(e, t, n) {
  switch (t.tag) {
    case 3:
      Bu(t), Kn();
      break;
    case 5:
      xu(t);
      break;
    case 1:
      Qe(t.type) && za(t);
      break;
    case 4:
      fo(t, t.stateNode.containerInfo);
      break;
    case 10:
      var r = t.type._context, a = t.memoizedProps.value;
      ne(Ma, r._currentValue), r._currentValue = a;
      break;
    case 13:
      if (r = t.memoizedState, r !== null)
        return r.dehydrated !== null ? (ne(ce, ce.current & 1), t.flags |= 128, null) : n & t.child.childLanes ? Hu(e, t, n) : (ne(ce, ce.current & 1), e = Mt(e, t, n), e !== null ? e.sibling : null);
      ne(ce, ce.current & 1);
      break;
    case 19:
      if (r = (n & t.childLanes) !== 0, e.flags & 128) {
        if (r) return Wu(e, t, n);
        t.flags |= 128;
      }
      if (a = t.memoizedState, a !== null && (a.rendering = null, a.tail = null, a.lastEffect = null), ne(ce, ce.current), r) break;
      return null;
    case 22:
    case 23:
      return t.lanes = 0, bu(e, t, n);
  }
  return Mt(e, t, n);
}
var Qu, Ei, Gu, Ku;
Qu = function(e, t) {
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
Ei = function() {
};
Gu = function(e, t, n, r) {
  var a = e.memoizedProps;
  if (a !== r) {
    e = t.stateNode, pn(kt.current);
    var i = null;
    switch (n) {
      case "input":
        a = Kl(e, a), r = Kl(e, r), i = [];
        break;
      case "select":
        a = de({}, a, { value: void 0 }), r = de({}, r, { value: void 0 }), i = [];
        break;
      case "textarea":
        a = Xl(e, a), r = Xl(e, r), i = [];
        break;
      default:
        typeof a.onClick != "function" && typeof r.onClick == "function" && (e.onclick = Ra);
    }
    Jl(n, r);
    var o;
    n = null;
    for (d in a) if (!r.hasOwnProperty(d) && a.hasOwnProperty(d) && a[d] != null) if (d === "style") {
      var s = a[d];
      for (o in s) s.hasOwnProperty(o) && (n || (n = {}), n[o] = "");
    } else d !== "dangerouslySetInnerHTML" && d !== "children" && d !== "suppressContentEditableWarning" && d !== "suppressHydrationWarning" && d !== "autoFocus" && (kr.hasOwnProperty(d) ? i || (i = []) : (i = i || []).push(d, null));
    for (d in r) {
      var c = r[d];
      if (s = a != null ? a[d] : void 0, r.hasOwnProperty(d) && c !== s && (c != null || s != null)) if (d === "style") if (s) {
        for (o in s) !s.hasOwnProperty(o) || c && c.hasOwnProperty(o) || (n || (n = {}), n[o] = "");
        for (o in c) c.hasOwnProperty(o) && s[o] !== c[o] && (n || (n = {}), n[o] = c[o]);
      } else n || (i || (i = []), i.push(
        d,
        n
      )), n = c;
      else d === "dangerouslySetInnerHTML" ? (c = c ? c.__html : void 0, s = s ? s.__html : void 0, c != null && s !== c && (i = i || []).push(d, c)) : d === "children" ? typeof c != "string" && typeof c != "number" || (i = i || []).push(d, "" + c) : d !== "suppressContentEditableWarning" && d !== "suppressHydrationWarning" && (kr.hasOwnProperty(d) ? (c != null && d === "onScroll" && ae("scroll", e), i || s === c || (i = [])) : (i = i || []).push(d, c));
    }
    n && (i = i || []).push("style", n);
    var d = i;
    (t.updateQueue = d) && (t.flags |= 4);
  }
};
Ku = function(e, t, n, r) {
  n !== r && (t.flags |= 4);
};
function sr(e, t) {
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
function Re(e) {
  var t = e.alternate !== null && e.alternate.child === e.child, n = 0, r = 0;
  if (t) for (var a = e.child; a !== null; ) n |= a.lanes | a.childLanes, r |= a.subtreeFlags & 14680064, r |= a.flags & 14680064, a.return = e, a = a.sibling;
  else for (a = e.child; a !== null; ) n |= a.lanes | a.childLanes, r |= a.subtreeFlags, r |= a.flags, a.return = e, a = a.sibling;
  return e.subtreeFlags |= r, e.childLanes = n, t;
}
function Fp(e, t, n) {
  var r = t.pendingProps;
  switch (ao(t), t.tag) {
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
      return Re(t), null;
    case 1:
      return Qe(t.type) && Ta(), Re(t), null;
    case 3:
      return r = t.stateNode, Yn(), le(We), le(De), mo(), r.pendingContext && (r.context = r.pendingContext, r.pendingContext = null), (e === null || e.child === null) && (la(t) ? t.flags |= 4 : e === null || e.memoizedState.isDehydrated && !(t.flags & 256) || (t.flags |= 1024, ht !== null && (Di(ht), ht = null))), Ei(e, t), Re(t), null;
    case 5:
      po(t);
      var a = pn(Mr.current);
      if (n = t.type, e !== null && t.stateNode != null) Gu(e, t, n, r, a), e.ref !== t.ref && (t.flags |= 512, t.flags |= 2097152);
      else {
        if (!r) {
          if (t.stateNode === null) throw Error(P(166));
          return Re(t), null;
        }
        if (e = pn(kt.current), la(t)) {
          r = t.stateNode, n = t.type;
          var i = t.memoizedProps;
          switch (r[St] = t, r[Dr] = i, e = (t.mode & 1) !== 0, n) {
            case "dialog":
              ae("cancel", r), ae("close", r);
              break;
            case "iframe":
            case "object":
            case "embed":
              ae("load", r);
              break;
            case "video":
            case "audio":
              for (a = 0; a < pr.length; a++) ae(pr[a], r);
              break;
            case "source":
              ae("error", r);
              break;
            case "img":
            case "image":
            case "link":
              ae(
                "error",
                r
              ), ae("load", r);
              break;
            case "details":
              ae("toggle", r);
              break;
            case "input":
              Ko(r, i), ae("invalid", r);
              break;
            case "select":
              r._wrapperState = { wasMultiple: !!i.multiple }, ae("invalid", r);
              break;
            case "textarea":
              Yo(r, i), ae("invalid", r);
          }
          Jl(n, i), a = null;
          for (var o in i) if (i.hasOwnProperty(o)) {
            var s = i[o];
            o === "children" ? typeof s == "string" ? r.textContent !== s && (i.suppressHydrationWarning !== !0 && aa(r.textContent, s, e), a = ["children", s]) : typeof s == "number" && r.textContent !== "" + s && (i.suppressHydrationWarning !== !0 && aa(
              r.textContent,
              s,
              e
            ), a = ["children", "" + s]) : kr.hasOwnProperty(o) && s != null && o === "onScroll" && ae("scroll", r);
          }
          switch (n) {
            case "input":
              Yr(r), qo(r, i, !0);
              break;
            case "textarea":
              Yr(r), Xo(r);
              break;
            case "select":
            case "option":
              break;
            default:
              typeof i.onClick == "function" && (r.onclick = Ra);
          }
          r = a, t.updateQueue = r, r !== null && (t.flags |= 4);
        } else {
          o = a.nodeType === 9 ? a : a.ownerDocument, e === "http://www.w3.org/1999/xhtml" && (e = Sc(n)), e === "http://www.w3.org/1999/xhtml" ? n === "script" ? (e = o.createElement("div"), e.innerHTML = "<script><\/script>", e = e.removeChild(e.firstChild)) : typeof r.is == "string" ? e = o.createElement(n, { is: r.is }) : (e = o.createElement(n), n === "select" && (o = e, r.multiple ? o.multiple = !0 : r.size && (o.size = r.size))) : e = o.createElementNS(e, n), e[St] = t, e[Dr] = r, Qu(e, t, !1, !1), t.stateNode = e;
          e: {
            switch (o = ei(n, r), n) {
              case "dialog":
                ae("cancel", e), ae("close", e), a = r;
                break;
              case "iframe":
              case "object":
              case "embed":
                ae("load", e), a = r;
                break;
              case "video":
              case "audio":
                for (a = 0; a < pr.length; a++) ae(pr[a], e);
                a = r;
                break;
              case "source":
                ae("error", e), a = r;
                break;
              case "img":
              case "image":
              case "link":
                ae(
                  "error",
                  e
                ), ae("load", e), a = r;
                break;
              case "details":
                ae("toggle", e), a = r;
                break;
              case "input":
                Ko(e, r), a = Kl(e, r), ae("invalid", e);
                break;
              case "option":
                a = r;
                break;
              case "select":
                e._wrapperState = { wasMultiple: !!r.multiple }, a = de({}, r, { value: void 0 }), ae("invalid", e);
                break;
              case "textarea":
                Yo(e, r), a = Xl(e, r), ae("invalid", e);
                break;
              default:
                a = r;
            }
            Jl(n, a), s = a;
            for (i in s) if (s.hasOwnProperty(i)) {
              var c = s[i];
              i === "style" ? Cc(e, c) : i === "dangerouslySetInnerHTML" ? (c = c ? c.__html : void 0, c != null && wc(e, c)) : i === "children" ? typeof c == "string" ? (n !== "textarea" || c !== "") && Cr(e, c) : typeof c == "number" && Cr(e, "" + c) : i !== "suppressContentEditableWarning" && i !== "suppressHydrationWarning" && i !== "autoFocus" && (kr.hasOwnProperty(i) ? c != null && i === "onScroll" && ae("scroll", e) : c != null && Bi(e, i, c, o));
            }
            switch (n) {
              case "input":
                Yr(e), qo(e, r, !1);
                break;
              case "textarea":
                Yr(e), Xo(e);
                break;
              case "option":
                r.value != null && e.setAttribute("value", "" + nn(r.value));
                break;
              case "select":
                e.multiple = !!r.multiple, i = r.value, i != null ? On(e, !!r.multiple, i, !1) : r.defaultValue != null && On(
                  e,
                  !!r.multiple,
                  r.defaultValue,
                  !0
                );
                break;
              default:
                typeof a.onClick == "function" && (e.onclick = Ra);
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
      return Re(t), null;
    case 6:
      if (e && t.stateNode != null) Ku(e, t, e.memoizedProps, r);
      else {
        if (typeof r != "string" && t.stateNode === null) throw Error(P(166));
        if (n = pn(Mr.current), pn(kt.current), la(t)) {
          if (r = t.stateNode, n = t.memoizedProps, r[St] = t, (i = r.nodeValue !== n) && (e = Ze, e !== null)) switch (e.tag) {
            case 3:
              aa(r.nodeValue, n, (e.mode & 1) !== 0);
              break;
            case 5:
              e.memoizedProps.suppressHydrationWarning !== !0 && aa(r.nodeValue, n, (e.mode & 1) !== 0);
          }
          i && (t.flags |= 4);
        } else r = (n.nodeType === 9 ? n : n.ownerDocument).createTextNode(r), r[St] = t, t.stateNode = r;
      }
      return Re(t), null;
    case 13:
      if (le(ce), r = t.memoizedState, e === null || e.memoizedState !== null && e.memoizedState.dehydrated !== null) {
        if (oe && Xe !== null && t.mode & 1 && !(t.flags & 128)) fu(), Kn(), t.flags |= 98560, i = !1;
        else if (i = la(t), r !== null && r.dehydrated !== null) {
          if (e === null) {
            if (!i) throw Error(P(318));
            if (i = t.memoizedState, i = i !== null ? i.dehydrated : null, !i) throw Error(P(317));
            i[St] = t;
          } else Kn(), !(t.flags & 128) && (t.memoizedState = null), t.flags |= 4;
          Re(t), i = !1;
        } else ht !== null && (Di(ht), ht = null), i = !0;
        if (!i) return t.flags & 65536 ? t : null;
      }
      return t.flags & 128 ? (t.lanes = n, t) : (r = r !== null, r !== (e !== null && e.memoizedState !== null) && r && (t.child.flags |= 8192, t.mode & 1 && (e === null || ce.current & 1 ? Ne === 0 && (Ne = 3) : Eo())), t.updateQueue !== null && (t.flags |= 4), Re(t), null);
    case 4:
      return Yn(), Ei(e, t), e === null && Tr(t.stateNode.containerInfo), Re(t), null;
    case 10:
      return so(t.type._context), Re(t), null;
    case 17:
      return Qe(t.type) && Ta(), Re(t), null;
    case 19:
      if (le(ce), i = t.memoizedState, i === null) return Re(t), null;
      if (r = (t.flags & 128) !== 0, o = i.rendering, o === null) if (r) sr(i, !1);
      else {
        if (Ne !== 0 || e !== null && e.flags & 128) for (e = t.child; e !== null; ) {
          if (o = Oa(e), o !== null) {
            for (t.flags |= 128, sr(i, !1), r = o.updateQueue, r !== null && (t.updateQueue = r, t.flags |= 4), t.subtreeFlags = 0, r = n, n = t.child; n !== null; ) i = n, e = r, i.flags &= 14680066, o = i.alternate, o === null ? (i.childLanes = 0, i.lanes = e, i.child = null, i.subtreeFlags = 0, i.memoizedProps = null, i.memoizedState = null, i.updateQueue = null, i.dependencies = null, i.stateNode = null) : (i.childLanes = o.childLanes, i.lanes = o.lanes, i.child = o.child, i.subtreeFlags = 0, i.deletions = null, i.memoizedProps = o.memoizedProps, i.memoizedState = o.memoizedState, i.updateQueue = o.updateQueue, i.type = o.type, e = o.dependencies, i.dependencies = e === null ? null : { lanes: e.lanes, firstContext: e.firstContext }), n = n.sibling;
            return ne(ce, ce.current & 1 | 2), t.child;
          }
          e = e.sibling;
        }
        i.tail !== null && ve() > Zn && (t.flags |= 128, r = !0, sr(i, !1), t.lanes = 4194304);
      }
      else {
        if (!r) if (e = Oa(o), e !== null) {
          if (t.flags |= 128, r = !0, n = e.updateQueue, n !== null && (t.updateQueue = n, t.flags |= 4), sr(i, !0), i.tail === null && i.tailMode === "hidden" && !o.alternate && !oe) return Re(t), null;
        } else 2 * ve() - i.renderingStartTime > Zn && n !== 1073741824 && (t.flags |= 128, r = !0, sr(i, !1), t.lanes = 4194304);
        i.isBackwards ? (o.sibling = t.child, t.child = o) : (n = i.last, n !== null ? n.sibling = o : t.child = o, i.last = o);
      }
      return i.tail !== null ? (t = i.tail, i.rendering = t, i.tail = t.sibling, i.renderingStartTime = ve(), t.sibling = null, n = ce.current, ne(ce, r ? n & 1 | 2 : n & 1), t) : (Re(t), null);
    case 22:
    case 23:
      return Co(), r = t.memoizedState !== null, e !== null && e.memoizedState !== null !== r && (t.flags |= 8192), r && t.mode & 1 ? Ye & 1073741824 && (Re(t), t.subtreeFlags & 6 && (t.flags |= 8192)) : Re(t), null;
    case 24:
      return null;
    case 25:
      return null;
  }
  throw Error(P(156, t.tag));
}
function _p(e, t) {
  switch (ao(t), t.tag) {
    case 1:
      return Qe(t.type) && Ta(), e = t.flags, e & 65536 ? (t.flags = e & -65537 | 128, t) : null;
    case 3:
      return Yn(), le(We), le(De), mo(), e = t.flags, e & 65536 && !(e & 128) ? (t.flags = e & -65537 | 128, t) : null;
    case 5:
      return po(t), null;
    case 13:
      if (le(ce), e = t.memoizedState, e !== null && e.dehydrated !== null) {
        if (t.alternate === null) throw Error(P(340));
        Kn();
      }
      return e = t.flags, e & 65536 ? (t.flags = e & -65537 | 128, t) : null;
    case 19:
      return le(ce), null;
    case 4:
      return Yn(), null;
    case 10:
      return so(t.type._context), null;
    case 22:
    case 23:
      return Co(), null;
    case 24:
      return null;
    default:
      return null;
  }
}
var sa = !1, Te = !1, Rp = typeof WeakSet == "function" ? WeakSet : Set, A = null;
function $n(e, t) {
  var n = e.ref;
  if (n !== null) if (typeof n == "function") try {
    n(null);
  } catch (r) {
    he(e, t, r);
  }
  else n.current = null;
}
function Ii(e, t, n) {
  try {
    n();
  } catch (r) {
    he(e, t, r);
  }
}
var Us = !1;
function Tp(e, t) {
  if (ui = Pa, e = Jc(), no(e)) {
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
        var o = 0, s = -1, c = -1, d = 0, N = 0, u = e, h = null;
        t: for (; ; ) {
          for (var v; u !== n || a !== 0 && u.nodeType !== 3 || (s = o + a), u !== i || r !== 0 && u.nodeType !== 3 || (c = o + r), u.nodeType === 3 && (o += u.nodeValue.length), (v = u.firstChild) !== null; )
            h = u, u = v;
          for (; ; ) {
            if (u === e) break t;
            if (h === n && ++d === a && (s = o), h === i && ++N === r && (c = o), (v = u.nextSibling) !== null) break;
            u = h, h = u.parentNode;
          }
          u = v;
        }
        n = s === -1 || c === -1 ? null : { start: s, end: c };
      } else n = null;
    }
    n = n || { start: 0, end: 0 };
  } else n = null;
  for (di = { focusedElem: e, selectionRange: n }, Pa = !1, A = t; A !== null; ) if (t = A, e = t.child, (t.subtreeFlags & 1028) !== 0 && e !== null) e.return = t, A = e;
  else for (; A !== null; ) {
    t = A;
    try {
      var y = t.alternate;
      if (t.flags & 1024) switch (t.tag) {
        case 0:
        case 11:
        case 15:
          break;
        case 1:
          if (y !== null) {
            var S = y.memoizedProps, $ = y.memoizedState, p = t.stateNode, f = p.getSnapshotBeforeUpdate(t.elementType === t.type ? S : pt(t.type, S), $);
            p.__reactInternalSnapshotBeforeUpdate = f;
          }
          break;
        case 3:
          var x = t.stateNode.containerInfo;
          x.nodeType === 1 ? x.textContent = "" : x.nodeType === 9 && x.documentElement && x.removeChild(x.documentElement);
          break;
        case 5:
        case 6:
        case 4:
        case 17:
          break;
        default:
          throw Error(P(163));
      }
    } catch (E) {
      he(t, t.return, E);
    }
    if (e = t.sibling, e !== null) {
      e.return = t.return, A = e;
      break;
    }
    A = t.return;
  }
  return y = Us, Us = !1, y;
}
function jr(e, t, n) {
  var r = t.updateQueue;
  if (r = r !== null ? r.lastEffect : null, r !== null) {
    var a = r = r.next;
    do {
      if ((a.tag & e) === e) {
        var i = a.destroy;
        a.destroy = void 0, i !== void 0 && Ii(t, n, i);
      }
      a = a.next;
    } while (a !== r);
  }
}
function nl(e, t) {
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
function Pi(e) {
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
function qu(e) {
  var t = e.alternate;
  t !== null && (e.alternate = null, qu(t)), e.child = null, e.deletions = null, e.sibling = null, e.tag === 5 && (t = e.stateNode, t !== null && (delete t[St], delete t[Dr], delete t[mi], delete t[mp], delete t[hp])), e.stateNode = null, e.return = null, e.dependencies = null, e.memoizedProps = null, e.memoizedState = null, e.pendingProps = null, e.stateNode = null, e.updateQueue = null;
}
function Yu(e) {
  return e.tag === 5 || e.tag === 3 || e.tag === 4;
}
function bs(e) {
  e: for (; ; ) {
    for (; e.sibling === null; ) {
      if (e.return === null || Yu(e.return)) return null;
      e = e.return;
    }
    for (e.sibling.return = e.return, e = e.sibling; e.tag !== 5 && e.tag !== 6 && e.tag !== 18; ) {
      if (e.flags & 2 || e.child === null || e.tag === 4) continue e;
      e.child.return = e, e = e.child;
    }
    if (!(e.flags & 2)) return e.stateNode;
  }
}
function Fi(e, t, n) {
  var r = e.tag;
  if (r === 5 || r === 6) e = e.stateNode, t ? n.nodeType === 8 ? n.parentNode.insertBefore(e, t) : n.insertBefore(e, t) : (n.nodeType === 8 ? (t = n.parentNode, t.insertBefore(e, n)) : (t = n, t.appendChild(e)), n = n._reactRootContainer, n != null || t.onclick !== null || (t.onclick = Ra));
  else if (r !== 4 && (e = e.child, e !== null)) for (Fi(e, t, n), e = e.sibling; e !== null; ) Fi(e, t, n), e = e.sibling;
}
function _i(e, t, n) {
  var r = e.tag;
  if (r === 5 || r === 6) e = e.stateNode, t ? n.insertBefore(e, t) : n.appendChild(e);
  else if (r !== 4 && (e = e.child, e !== null)) for (_i(e, t, n), e = e.sibling; e !== null; ) _i(e, t, n), e = e.sibling;
}
var Ee = null, mt = !1;
function bt(e, t, n) {
  for (n = n.child; n !== null; ) Xu(e, t, n), n = n.sibling;
}
function Xu(e, t, n) {
  if (wt && typeof wt.onCommitFiberUnmount == "function") try {
    wt.onCommitFiberUnmount(Ka, n);
  } catch {
  }
  switch (n.tag) {
    case 5:
      Te || $n(n, t);
    case 6:
      var r = Ee, a = mt;
      Ee = null, bt(e, t, n), Ee = r, mt = a, Ee !== null && (mt ? (e = Ee, n = n.stateNode, e.nodeType === 8 ? e.parentNode.removeChild(n) : e.removeChild(n)) : Ee.removeChild(n.stateNode));
      break;
    case 18:
      Ee !== null && (mt ? (e = Ee, n = n.stateNode, e.nodeType === 8 ? _l(e.parentNode, n) : e.nodeType === 1 && _l(e, n), Fr(e)) : _l(Ee, n.stateNode));
      break;
    case 4:
      r = Ee, a = mt, Ee = n.stateNode.containerInfo, mt = !0, bt(e, t, n), Ee = r, mt = a;
      break;
    case 0:
    case 11:
    case 14:
    case 15:
      if (!Te && (r = n.updateQueue, r !== null && (r = r.lastEffect, r !== null))) {
        a = r = r.next;
        do {
          var i = a, o = i.destroy;
          i = i.tag, o !== void 0 && (i & 2 || i & 4) && Ii(n, t, o), a = a.next;
        } while (a !== r);
      }
      bt(e, t, n);
      break;
    case 1:
      if (!Te && ($n(n, t), r = n.stateNode, typeof r.componentWillUnmount == "function")) try {
        r.props = n.memoizedProps, r.state = n.memoizedState, r.componentWillUnmount();
      } catch (s) {
        he(n, t, s);
      }
      bt(e, t, n);
      break;
    case 21:
      bt(e, t, n);
      break;
    case 22:
      n.mode & 1 ? (Te = (r = Te) || n.memoizedState !== null, bt(e, t, n), Te = r) : bt(e, t, n);
      break;
    default:
      bt(e, t, n);
  }
}
function Vs(e) {
  var t = e.updateQueue;
  if (t !== null) {
    e.updateQueue = null;
    var n = e.stateNode;
    n === null && (n = e.stateNode = new Rp()), t.forEach(function(r) {
      var a = bp.bind(null, e, r);
      n.has(r) || (n.add(r), r.then(a, a));
    });
  }
}
function ft(e, t) {
  var n = t.deletions;
  if (n !== null) for (var r = 0; r < n.length; r++) {
    var a = n[r];
    try {
      var i = e, o = t, s = o;
      e: for (; s !== null; ) {
        switch (s.tag) {
          case 5:
            Ee = s.stateNode, mt = !1;
            break e;
          case 3:
            Ee = s.stateNode.containerInfo, mt = !0;
            break e;
          case 4:
            Ee = s.stateNode.containerInfo, mt = !0;
            break e;
        }
        s = s.return;
      }
      if (Ee === null) throw Error(P(160));
      Xu(i, o, a), Ee = null, mt = !1;
      var c = a.alternate;
      c !== null && (c.return = null), a.return = null;
    } catch (d) {
      he(a, t, d);
    }
  }
  if (t.subtreeFlags & 12854) for (t = t.child; t !== null; ) Zu(t, e), t = t.sibling;
}
function Zu(e, t) {
  var n = e.alternate, r = e.flags;
  switch (e.tag) {
    case 0:
    case 11:
    case 14:
    case 15:
      if (ft(t, e), jt(e), r & 4) {
        try {
          jr(3, e, e.return), nl(3, e);
        } catch (S) {
          he(e, e.return, S);
        }
        try {
          jr(5, e, e.return);
        } catch (S) {
          he(e, e.return, S);
        }
      }
      break;
    case 1:
      ft(t, e), jt(e), r & 512 && n !== null && $n(n, n.return);
      break;
    case 5:
      if (ft(t, e), jt(e), r & 512 && n !== null && $n(n, n.return), e.flags & 32) {
        var a = e.stateNode;
        try {
          Cr(a, "");
        } catch (S) {
          he(e, e.return, S);
        }
      }
      if (r & 4 && (a = e.stateNode, a != null)) {
        var i = e.memoizedProps, o = n !== null ? n.memoizedProps : i, s = e.type, c = e.updateQueue;
        if (e.updateQueue = null, c !== null) try {
          s === "input" && i.type === "radio" && i.name != null && jc(a, i), ei(s, o);
          var d = ei(s, i);
          for (o = 0; o < c.length; o += 2) {
            var N = c[o], u = c[o + 1];
            N === "style" ? Cc(a, u) : N === "dangerouslySetInnerHTML" ? wc(a, u) : N === "children" ? Cr(a, u) : Bi(a, N, u, d);
          }
          switch (s) {
            case "input":
              ql(a, i);
              break;
            case "textarea":
              Nc(a, i);
              break;
            case "select":
              var h = a._wrapperState.wasMultiple;
              a._wrapperState.wasMultiple = !!i.multiple;
              var v = i.value;
              v != null ? On(a, !!i.multiple, v, !1) : h !== !!i.multiple && (i.defaultValue != null ? On(
                a,
                !!i.multiple,
                i.defaultValue,
                !0
              ) : On(a, !!i.multiple, i.multiple ? [] : "", !1));
          }
          a[Dr] = i;
        } catch (S) {
          he(e, e.return, S);
        }
      }
      break;
    case 6:
      if (ft(t, e), jt(e), r & 4) {
        if (e.stateNode === null) throw Error(P(162));
        a = e.stateNode, i = e.memoizedProps;
        try {
          a.nodeValue = i;
        } catch (S) {
          he(e, e.return, S);
        }
      }
      break;
    case 3:
      if (ft(t, e), jt(e), r & 4 && n !== null && n.memoizedState.isDehydrated) try {
        Fr(t.containerInfo);
      } catch (S) {
        he(e, e.return, S);
      }
      break;
    case 4:
      ft(t, e), jt(e);
      break;
    case 13:
      ft(t, e), jt(e), a = e.child, a.flags & 8192 && (i = a.memoizedState !== null, a.stateNode.isHidden = i, !i || a.alternate !== null && a.alternate.memoizedState !== null || (wo = ve())), r & 4 && Vs(e);
      break;
    case 22:
      if (N = n !== null && n.memoizedState !== null, e.mode & 1 ? (Te = (d = Te) || N, ft(t, e), Te = d) : ft(t, e), jt(e), r & 8192) {
        if (d = e.memoizedState !== null, (e.stateNode.isHidden = d) && !N && e.mode & 1) for (A = e, N = e.child; N !== null; ) {
          for (u = A = N; A !== null; ) {
            switch (h = A, v = h.child, h.tag) {
              case 0:
              case 11:
              case 14:
              case 15:
                jr(4, h, h.return);
                break;
              case 1:
                $n(h, h.return);
                var y = h.stateNode;
                if (typeof y.componentWillUnmount == "function") {
                  r = h, n = h.return;
                  try {
                    t = r, y.props = t.memoizedProps, y.state = t.memoizedState, y.componentWillUnmount();
                  } catch (S) {
                    he(r, n, S);
                  }
                }
                break;
              case 5:
                $n(h, h.return);
                break;
              case 22:
                if (h.memoizedState !== null) {
                  Hs(u);
                  continue;
                }
            }
            v !== null ? (v.return = h, A = v) : Hs(u);
          }
          N = N.sibling;
        }
        e: for (N = null, u = e; ; ) {
          if (u.tag === 5) {
            if (N === null) {
              N = u;
              try {
                a = u.stateNode, d ? (i = a.style, typeof i.setProperty == "function" ? i.setProperty("display", "none", "important") : i.display = "none") : (s = u.stateNode, c = u.memoizedProps.style, o = c != null && c.hasOwnProperty("display") ? c.display : null, s.style.display = kc("display", o));
              } catch (S) {
                he(e, e.return, S);
              }
            }
          } else if (u.tag === 6) {
            if (N === null) try {
              u.stateNode.nodeValue = d ? "" : u.memoizedProps;
            } catch (S) {
              he(e, e.return, S);
            }
          } else if ((u.tag !== 22 && u.tag !== 23 || u.memoizedState === null || u === e) && u.child !== null) {
            u.child.return = u, u = u.child;
            continue;
          }
          if (u === e) break e;
          for (; u.sibling === null; ) {
            if (u.return === null || u.return === e) break e;
            N === u && (N = null), u = u.return;
          }
          N === u && (N = null), u.sibling.return = u.return, u = u.sibling;
        }
      }
      break;
    case 19:
      ft(t, e), jt(e), r & 4 && Vs(e);
      break;
    case 21:
      break;
    default:
      ft(
        t,
        e
      ), jt(e);
  }
}
function jt(e) {
  var t = e.flags;
  if (t & 2) {
    try {
      e: {
        for (var n = e.return; n !== null; ) {
          if (Yu(n)) {
            var r = n;
            break e;
          }
          n = n.return;
        }
        throw Error(P(160));
      }
      switch (r.tag) {
        case 5:
          var a = r.stateNode;
          r.flags & 32 && (Cr(a, ""), r.flags &= -33);
          var i = bs(e);
          _i(e, i, a);
          break;
        case 3:
        case 4:
          var o = r.stateNode.containerInfo, s = bs(e);
          Fi(e, s, o);
          break;
        default:
          throw Error(P(161));
      }
    } catch (c) {
      he(e, e.return, c);
    }
    e.flags &= -3;
  }
  t & 4096 && (e.flags &= -4097);
}
function zp(e, t, n) {
  A = e, Ju(e);
}
function Ju(e, t, n) {
  for (var r = (e.mode & 1) !== 0; A !== null; ) {
    var a = A, i = a.child;
    if (a.tag === 22 && r) {
      var o = a.memoizedState !== null || sa;
      if (!o) {
        var s = a.alternate, c = s !== null && s.memoizedState !== null || Te;
        s = sa;
        var d = Te;
        if (sa = o, (Te = c) && !d) for (A = a; A !== null; ) o = A, c = o.child, o.tag === 22 && o.memoizedState !== null ? Ws(a) : c !== null ? (c.return = o, A = c) : Ws(a);
        for (; i !== null; ) A = i, Ju(i), i = i.sibling;
        A = a, sa = s, Te = d;
      }
      Bs(e);
    } else a.subtreeFlags & 8772 && i !== null ? (i.return = a, A = i) : Bs(e);
  }
}
function Bs(e) {
  for (; A !== null; ) {
    var t = A;
    if (t.flags & 8772) {
      var n = t.alternate;
      try {
        if (t.flags & 8772) switch (t.tag) {
          case 0:
          case 11:
          case 15:
            Te || nl(5, t);
            break;
          case 1:
            var r = t.stateNode;
            if (t.flags & 4 && !Te) if (n === null) r.componentDidMount();
            else {
              var a = t.elementType === t.type ? n.memoizedProps : pt(t.type, n.memoizedProps);
              r.componentDidUpdate(a, n.memoizedState, r.__reactInternalSnapshotBeforeUpdate);
            }
            var i = t.updateQueue;
            i !== null && Is(t, i, r);
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
              Is(t, o, n);
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
                var N = d.memoizedState;
                if (N !== null) {
                  var u = N.dehydrated;
                  u !== null && Fr(u);
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
        Te || t.flags & 512 && Pi(t);
      } catch (h) {
        he(t, t.return, h);
      }
    }
    if (t === e) {
      A = null;
      break;
    }
    if (n = t.sibling, n !== null) {
      n.return = t.return, A = n;
      break;
    }
    A = t.return;
  }
}
function Hs(e) {
  for (; A !== null; ) {
    var t = A;
    if (t === e) {
      A = null;
      break;
    }
    var n = t.sibling;
    if (n !== null) {
      n.return = t.return, A = n;
      break;
    }
    A = t.return;
  }
}
function Ws(e) {
  for (; A !== null; ) {
    var t = A;
    try {
      switch (t.tag) {
        case 0:
        case 11:
        case 15:
          var n = t.return;
          try {
            nl(4, t);
          } catch (c) {
            he(t, n, c);
          }
          break;
        case 1:
          var r = t.stateNode;
          if (typeof r.componentDidMount == "function") {
            var a = t.return;
            try {
              r.componentDidMount();
            } catch (c) {
              he(t, a, c);
            }
          }
          var i = t.return;
          try {
            Pi(t);
          } catch (c) {
            he(t, i, c);
          }
          break;
        case 5:
          var o = t.return;
          try {
            Pi(t);
          } catch (c) {
            he(t, o, c);
          }
      }
    } catch (c) {
      he(t, t.return, c);
    }
    if (t === e) {
      A = null;
      break;
    }
    var s = t.sibling;
    if (s !== null) {
      s.return = t.return, A = s;
      break;
    }
    A = t.return;
  }
}
var Dp = Math.ceil, Va = $t.ReactCurrentDispatcher, No = $t.ReactCurrentOwner, ot = $t.ReactCurrentBatchConfig, q = 0, Ce = null, ge = null, Ie = 0, Ye = 0, An = on(0), Ne = 0, Ur = null, jn = 0, rl = 0, So = 0, Nr = null, Be = null, wo = 0, Zn = 1 / 0, Pt = null, Ba = !1, Ri = null, Jt = null, ca = !1, Gt = null, Ha = 0, Sr = 0, Ti = null, ja = -1, Na = 0;
function Oe() {
  return q & 6 ? ve() : ja !== -1 ? ja : ja = ve();
}
function en(e) {
  return e.mode & 1 ? q & 2 && Ie !== 0 ? Ie & -Ie : xp.transition !== null ? (Na === 0 && (Na = $c()), Na) : (e = J, e !== 0 || (e = window.event, e = e === void 0 ? 16 : Hc(e.type)), e) : 1;
}
function xt(e, t, n, r) {
  if (50 < Sr) throw Sr = 0, Ti = null, Error(P(185));
  Vr(e, n, r), (!(q & 2) || e !== Ce) && (e === Ce && (!(q & 2) && (rl |= n), Ne === 4 && Wt(e, Ie)), Ge(e, r), n === 1 && q === 0 && !(t.mode & 1) && (Zn = ve() + 500, Ja && sn()));
}
function Ge(e, t) {
  var n = e.callbackNode;
  xf(e, t);
  var r = Ia(e, e === Ce ? Ie : 0);
  if (r === 0) n !== null && es(n), e.callbackNode = null, e.callbackPriority = 0;
  else if (t = r & -r, e.callbackPriority !== t) {
    if (n != null && es(n), t === 1) e.tag === 0 ? vp(Qs.bind(null, e)) : cu(Qs.bind(null, e)), fp(function() {
      !(q & 6) && sn();
    }), n = null;
    else {
      switch (Ac(r)) {
        case 1:
          n = Ki;
          break;
        case 4:
          n = Lc;
          break;
        case 16:
          n = Ea;
          break;
        case 536870912:
          n = Mc;
          break;
        default:
          n = Ea;
      }
      n = od(n, ed.bind(null, e));
    }
    e.callbackPriority = t, e.callbackNode = n;
  }
}
function ed(e, t) {
  if (ja = -1, Na = 0, q & 6) throw Error(P(327));
  var n = e.callbackNode;
  if (Hn() && e.callbackNode !== n) return null;
  var r = Ia(e, e === Ce ? Ie : 0);
  if (r === 0) return null;
  if (r & 30 || r & e.expiredLanes || t) t = Wa(e, r);
  else {
    t = r;
    var a = q;
    q |= 2;
    var i = nd();
    (Ce !== e || Ie !== t) && (Pt = null, Zn = ve() + 500, mn(e, t));
    do
      try {
        $p();
        break;
      } catch (s) {
        td(e, s);
      }
    while (!0);
    oo(), Va.current = i, q = a, ge !== null ? t = 0 : (Ce = null, Ie = 0, t = Ne);
  }
  if (t !== 0) {
    if (t === 2 && (a = li(e), a !== 0 && (r = a, t = zi(e, a))), t === 1) throw n = Ur, mn(e, 0), Wt(e, r), Ge(e, ve()), n;
    if (t === 6) Wt(e, r);
    else {
      if (a = e.current.alternate, !(r & 30) && !Lp(a) && (t = Wa(e, r), t === 2 && (i = li(e), i !== 0 && (r = i, t = zi(e, i))), t === 1)) throw n = Ur, mn(e, 0), Wt(e, r), Ge(e, ve()), n;
      switch (e.finishedWork = a, e.finishedLanes = r, t) {
        case 0:
        case 1:
          throw Error(P(345));
        case 2:
          un(e, Be, Pt);
          break;
        case 3:
          if (Wt(e, r), (r & 130023424) === r && (t = wo + 500 - ve(), 10 < t)) {
            if (Ia(e, 0) !== 0) break;
            if (a = e.suspendedLanes, (a & r) !== r) {
              Oe(), e.pingedLanes |= e.suspendedLanes & a;
              break;
            }
            e.timeoutHandle = pi(un.bind(null, e, Be, Pt), t);
            break;
          }
          un(e, Be, Pt);
          break;
        case 4:
          if (Wt(e, r), (r & 4194240) === r) break;
          for (t = e.eventTimes, a = -1; 0 < r; ) {
            var o = 31 - vt(r);
            i = 1 << o, o = t[o], o > a && (a = o), r &= ~i;
          }
          if (r = a, r = ve() - r, r = (120 > r ? 120 : 480 > r ? 480 : 1080 > r ? 1080 : 1920 > r ? 1920 : 3e3 > r ? 3e3 : 4320 > r ? 4320 : 1960 * Dp(r / 1960)) - r, 10 < r) {
            e.timeoutHandle = pi(un.bind(null, e, Be, Pt), r);
            break;
          }
          un(e, Be, Pt);
          break;
        case 5:
          un(e, Be, Pt);
          break;
        default:
          throw Error(P(329));
      }
    }
  }
  return Ge(e, ve()), e.callbackNode === n ? ed.bind(null, e) : null;
}
function zi(e, t) {
  var n = Nr;
  return e.current.memoizedState.isDehydrated && (mn(e, t).flags |= 256), e = Wa(e, t), e !== 2 && (t = Be, Be = n, t !== null && Di(t)), e;
}
function Di(e) {
  Be === null ? Be = e : Be.push.apply(Be, e);
}
function Lp(e) {
  for (var t = e; ; ) {
    if (t.flags & 16384) {
      var n = t.updateQueue;
      if (n !== null && (n = n.stores, n !== null)) for (var r = 0; r < n.length; r++) {
        var a = n[r], i = a.getSnapshot;
        a = a.value;
        try {
          if (!yt(i(), a)) return !1;
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
function Wt(e, t) {
  for (t &= ~So, t &= ~rl, e.suspendedLanes |= t, e.pingedLanes &= ~t, e = e.expirationTimes; 0 < t; ) {
    var n = 31 - vt(t), r = 1 << n;
    e[n] = -1, t &= ~r;
  }
}
function Qs(e) {
  if (q & 6) throw Error(P(327));
  Hn();
  var t = Ia(e, 0);
  if (!(t & 1)) return Ge(e, ve()), null;
  var n = Wa(e, t);
  if (e.tag !== 0 && n === 2) {
    var r = li(e);
    r !== 0 && (t = r, n = zi(e, r));
  }
  if (n === 1) throw n = Ur, mn(e, 0), Wt(e, t), Ge(e, ve()), n;
  if (n === 6) throw Error(P(345));
  return e.finishedWork = e.current.alternate, e.finishedLanes = t, un(e, Be, Pt), Ge(e, ve()), null;
}
function ko(e, t) {
  var n = q;
  q |= 1;
  try {
    return e(t);
  } finally {
    q = n, q === 0 && (Zn = ve() + 500, Ja && sn());
  }
}
function Nn(e) {
  Gt !== null && Gt.tag === 0 && !(q & 6) && Hn();
  var t = q;
  q |= 1;
  var n = ot.transition, r = J;
  try {
    if (ot.transition = null, J = 1, e) return e();
  } finally {
    J = r, ot.transition = n, q = t, !(q & 6) && sn();
  }
}
function Co() {
  Ye = An.current, le(An);
}
function mn(e, t) {
  e.finishedWork = null, e.finishedLanes = 0;
  var n = e.timeoutHandle;
  if (n !== -1 && (e.timeoutHandle = -1, dp(n)), ge !== null) for (n = ge.return; n !== null; ) {
    var r = n;
    switch (ao(r), r.tag) {
      case 1:
        r = r.type.childContextTypes, r != null && Ta();
        break;
      case 3:
        Yn(), le(We), le(De), mo();
        break;
      case 5:
        po(r);
        break;
      case 4:
        Yn();
        break;
      case 13:
        le(ce);
        break;
      case 19:
        le(ce);
        break;
      case 10:
        so(r.type._context);
        break;
      case 22:
      case 23:
        Co();
    }
    n = n.return;
  }
  if (Ce = e, ge = e = tn(e.current, null), Ie = Ye = t, Ne = 0, Ur = null, So = rl = jn = 0, Be = Nr = null, fn !== null) {
    for (t = 0; t < fn.length; t++) if (n = fn[t], r = n.interleaved, r !== null) {
      n.interleaved = null;
      var a = r.next, i = n.pending;
      if (i !== null) {
        var o = i.next;
        i.next = a, r.next = o;
      }
      n.pending = r;
    }
    fn = null;
  }
  return e;
}
function td(e, t) {
  do {
    var n = ge;
    try {
      if (oo(), xa.current = ba, Ua) {
        for (var r = ue.memoizedState; r !== null; ) {
          var a = r.queue;
          a !== null && (a.pending = null), r = r.next;
        }
        Ua = !1;
      }
      if (yn = 0, we = ye = ue = null, yr = !1, $r = 0, No.current = null, n === null || n.return === null) {
        Ne = 1, Ur = t, ge = null;
        break;
      }
      e: {
        var i = e, o = n.return, s = n, c = t;
        if (t = Ie, s.flags |= 32768, c !== null && typeof c == "object" && typeof c.then == "function") {
          var d = c, N = s, u = N.tag;
          if (!(N.mode & 1) && (u === 0 || u === 11 || u === 15)) {
            var h = N.alternate;
            h ? (N.updateQueue = h.updateQueue, N.memoizedState = h.memoizedState, N.lanes = h.lanes) : (N.updateQueue = null, N.memoizedState = null);
          }
          var v = zs(o);
          if (v !== null) {
            v.flags &= -257, Ds(v, o, s, i, t), v.mode & 1 && Ts(i, d, t), t = v, c = d;
            var y = t.updateQueue;
            if (y === null) {
              var S = /* @__PURE__ */ new Set();
              S.add(c), t.updateQueue = S;
            } else y.add(c);
            break e;
          } else {
            if (!(t & 1)) {
              Ts(i, d, t), Eo();
              break e;
            }
            c = Error(P(426));
          }
        } else if (oe && s.mode & 1) {
          var $ = zs(o);
          if ($ !== null) {
            !($.flags & 65536) && ($.flags |= 256), Ds($, o, s, i, t), lo(Xn(c, s));
            break e;
          }
        }
        i = c = Xn(c, s), Ne !== 4 && (Ne = 2), Nr === null ? Nr = [i] : Nr.push(i), i = o;
        do {
          switch (i.tag) {
            case 3:
              i.flags |= 65536, t &= -t, i.lanes |= t;
              var p = Au(i, c, t);
              Es(i, p);
              break e;
            case 1:
              s = c;
              var f = i.type, x = i.stateNode;
              if (!(i.flags & 128) && (typeof f.getDerivedStateFromError == "function" || x !== null && typeof x.componentDidCatch == "function" && (Jt === null || !Jt.has(x)))) {
                i.flags |= 65536, t &= -t, i.lanes |= t;
                var E = Ou(i, s, t);
                Es(i, E);
                break e;
              }
          }
          i = i.return;
        } while (i !== null);
      }
      ad(n);
    } catch (T) {
      t = T, ge === n && n !== null && (ge = n = n.return);
      continue;
    }
    break;
  } while (!0);
}
function nd() {
  var e = Va.current;
  return Va.current = ba, e === null ? ba : e;
}
function Eo() {
  (Ne === 0 || Ne === 3 || Ne === 2) && (Ne = 4), Ce === null || !(jn & 268435455) && !(rl & 268435455) || Wt(Ce, Ie);
}
function Wa(e, t) {
  var n = q;
  q |= 2;
  var r = nd();
  (Ce !== e || Ie !== t) && (Pt = null, mn(e, t));
  do
    try {
      Mp();
      break;
    } catch (a) {
      td(e, a);
    }
  while (!0);
  if (oo(), q = n, Va.current = r, ge !== null) throw Error(P(261));
  return Ce = null, Ie = 0, Ne;
}
function Mp() {
  for (; ge !== null; ) rd(ge);
}
function $p() {
  for (; ge !== null && !sf(); ) rd(ge);
}
function rd(e) {
  var t = id(e.alternate, e, Ye);
  e.memoizedProps = e.pendingProps, t === null ? ad(e) : ge = t, No.current = null;
}
function ad(e) {
  var t = e;
  do {
    var n = t.alternate;
    if (e = t.return, t.flags & 32768) {
      if (n = _p(n, t), n !== null) {
        n.flags &= 32767, ge = n;
        return;
      }
      if (e !== null) e.flags |= 32768, e.subtreeFlags = 0, e.deletions = null;
      else {
        Ne = 6, ge = null;
        return;
      }
    } else if (n = Fp(n, t, Ye), n !== null) {
      ge = n;
      return;
    }
    if (t = t.sibling, t !== null) {
      ge = t;
      return;
    }
    ge = t = e;
  } while (t !== null);
  Ne === 0 && (Ne = 5);
}
function un(e, t, n) {
  var r = J, a = ot.transition;
  try {
    ot.transition = null, J = 1, Ap(e, t, n, r);
  } finally {
    ot.transition = a, J = r;
  }
  return null;
}
function Ap(e, t, n, r) {
  do
    Hn();
  while (Gt !== null);
  if (q & 6) throw Error(P(327));
  n = e.finishedWork;
  var a = e.finishedLanes;
  if (n === null) return null;
  if (e.finishedWork = null, e.finishedLanes = 0, n === e.current) throw Error(P(177));
  e.callbackNode = null, e.callbackPriority = 0;
  var i = n.lanes | n.childLanes;
  if (gf(e, i), e === Ce && (ge = Ce = null, Ie = 0), !(n.subtreeFlags & 2064) && !(n.flags & 2064) || ca || (ca = !0, od(Ea, function() {
    return Hn(), null;
  })), i = (n.flags & 15990) !== 0, n.subtreeFlags & 15990 || i) {
    i = ot.transition, ot.transition = null;
    var o = J;
    J = 1;
    var s = q;
    q |= 4, No.current = null, Tp(e, n), Zu(n, e), ap(di), Pa = !!ui, di = ui = null, e.current = n, zp(n), cf(), q = s, J = o, ot.transition = i;
  } else e.current = n;
  if (ca && (ca = !1, Gt = e, Ha = a), i = e.pendingLanes, i === 0 && (Jt = null), ff(n.stateNode), Ge(e, ve()), t !== null) for (r = e.onRecoverableError, n = 0; n < t.length; n++) a = t[n], r(a.value, { componentStack: a.stack, digest: a.digest });
  if (Ba) throw Ba = !1, e = Ri, Ri = null, e;
  return Ha & 1 && e.tag !== 0 && Hn(), i = e.pendingLanes, i & 1 ? e === Ti ? Sr++ : (Sr = 0, Ti = e) : Sr = 0, sn(), null;
}
function Hn() {
  if (Gt !== null) {
    var e = Ac(Ha), t = ot.transition, n = J;
    try {
      if (ot.transition = null, J = 16 > e ? 16 : e, Gt === null) var r = !1;
      else {
        if (e = Gt, Gt = null, Ha = 0, q & 6) throw Error(P(331));
        var a = q;
        for (q |= 4, A = e.current; A !== null; ) {
          var i = A, o = i.child;
          if (A.flags & 16) {
            var s = i.deletions;
            if (s !== null) {
              for (var c = 0; c < s.length; c++) {
                var d = s[c];
                for (A = d; A !== null; ) {
                  var N = A;
                  switch (N.tag) {
                    case 0:
                    case 11:
                    case 15:
                      jr(8, N, i);
                  }
                  var u = N.child;
                  if (u !== null) u.return = N, A = u;
                  else for (; A !== null; ) {
                    N = A;
                    var h = N.sibling, v = N.return;
                    if (qu(N), N === d) {
                      A = null;
                      break;
                    }
                    if (h !== null) {
                      h.return = v, A = h;
                      break;
                    }
                    A = v;
                  }
                }
              }
              var y = i.alternate;
              if (y !== null) {
                var S = y.child;
                if (S !== null) {
                  y.child = null;
                  do {
                    var $ = S.sibling;
                    S.sibling = null, S = $;
                  } while (S !== null);
                }
              }
              A = i;
            }
          }
          if (i.subtreeFlags & 2064 && o !== null) o.return = i, A = o;
          else e: for (; A !== null; ) {
            if (i = A, i.flags & 2048) switch (i.tag) {
              case 0:
              case 11:
              case 15:
                jr(9, i, i.return);
            }
            var p = i.sibling;
            if (p !== null) {
              p.return = i.return, A = p;
              break e;
            }
            A = i.return;
          }
        }
        var f = e.current;
        for (A = f; A !== null; ) {
          o = A;
          var x = o.child;
          if (o.subtreeFlags & 2064 && x !== null) x.return = o, A = x;
          else e: for (o = f; A !== null; ) {
            if (s = A, s.flags & 2048) try {
              switch (s.tag) {
                case 0:
                case 11:
                case 15:
                  nl(9, s);
              }
            } catch (T) {
              he(s, s.return, T);
            }
            if (s === o) {
              A = null;
              break e;
            }
            var E = s.sibling;
            if (E !== null) {
              E.return = s.return, A = E;
              break e;
            }
            A = s.return;
          }
        }
        if (q = a, sn(), wt && typeof wt.onPostCommitFiberRoot == "function") try {
          wt.onPostCommitFiberRoot(Ka, e);
        } catch {
        }
        r = !0;
      }
      return r;
    } finally {
      J = n, ot.transition = t;
    }
  }
  return !1;
}
function Gs(e, t, n) {
  t = Xn(n, t), t = Au(e, t, 1), e = Zt(e, t, 1), t = Oe(), e !== null && (Vr(e, 1, t), Ge(e, t));
}
function he(e, t, n) {
  if (e.tag === 3) Gs(e, e, n);
  else for (; t !== null; ) {
    if (t.tag === 3) {
      Gs(t, e, n);
      break;
    } else if (t.tag === 1) {
      var r = t.stateNode;
      if (typeof t.type.getDerivedStateFromError == "function" || typeof r.componentDidCatch == "function" && (Jt === null || !Jt.has(r))) {
        e = Xn(n, e), e = Ou(t, e, 1), t = Zt(t, e, 1), e = Oe(), t !== null && (Vr(t, 1, e), Ge(t, e));
        break;
      }
    }
    t = t.return;
  }
}
function Op(e, t, n) {
  var r = e.pingCache;
  r !== null && r.delete(t), t = Oe(), e.pingedLanes |= e.suspendedLanes & n, Ce === e && (Ie & n) === n && (Ne === 4 || Ne === 3 && (Ie & 130023424) === Ie && 500 > ve() - wo ? mn(e, 0) : So |= n), Ge(e, t);
}
function ld(e, t) {
  t === 0 && (e.mode & 1 ? (t = Jr, Jr <<= 1, !(Jr & 130023424) && (Jr = 4194304)) : t = 1);
  var n = Oe();
  e = Lt(e, t), e !== null && (Vr(e, t, n), Ge(e, n));
}
function Up(e) {
  var t = e.memoizedState, n = 0;
  t !== null && (n = t.retryLane), ld(e, n);
}
function bp(e, t) {
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
      throw Error(P(314));
  }
  r !== null && r.delete(t), ld(e, n);
}
var id;
id = function(e, t, n) {
  if (e !== null) if (e.memoizedProps !== t.pendingProps || We.current) He = !0;
  else {
    if (!(e.lanes & n) && !(t.flags & 128)) return He = !1, Pp(e, t, n);
    He = !!(e.flags & 131072);
  }
  else He = !1, oe && t.flags & 1048576 && uu(t, La, t.index);
  switch (t.lanes = 0, t.tag) {
    case 2:
      var r = t.type;
      ya(e, t), e = t.pendingProps;
      var a = Gn(t, De.current);
      Bn(t, n), a = vo(null, t, r, e, a, n);
      var i = xo();
      return t.flags |= 1, typeof a == "object" && a !== null && typeof a.render == "function" && a.$$typeof === void 0 ? (t.tag = 1, t.memoizedState = null, t.updateQueue = null, Qe(r) ? (i = !0, za(t)) : i = !1, t.memoizedState = a.state !== null && a.state !== void 0 ? a.state : null, uo(t), a.updater = tl, t.stateNode = a, a._reactInternals = t, ji(t, r, e, n), t = wi(null, t, r, !0, i, n)) : (t.tag = 0, oe && i && ro(t), Me(null, t, a, n), t = t.child), t;
    case 16:
      r = t.elementType;
      e: {
        switch (ya(e, t), e = t.pendingProps, a = r._init, r = a(r._payload), t.type = r, a = t.tag = Bp(r), e = pt(r, e), a) {
          case 0:
            t = Si(null, t, r, e, n);
            break e;
          case 1:
            t = $s(null, t, r, e, n);
            break e;
          case 11:
            t = Ls(null, t, r, e, n);
            break e;
          case 14:
            t = Ms(null, t, r, pt(r.type, e), n);
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
      return r = t.type, a = t.pendingProps, a = t.elementType === r ? a : pt(r, a), Si(e, t, r, a, n);
    case 1:
      return r = t.type, a = t.pendingProps, a = t.elementType === r ? a : pt(r, a), $s(e, t, r, a, n);
    case 3:
      e: {
        if (Bu(t), e === null) throw Error(P(387));
        r = t.pendingProps, i = t.memoizedState, a = i.element, vu(e, t), Aa(t, r, null, n);
        var o = t.memoizedState;
        if (r = o.element, i.isDehydrated) if (i = { element: r, isDehydrated: !1, cache: o.cache, pendingSuspenseBoundaries: o.pendingSuspenseBoundaries, transitions: o.transitions }, t.updateQueue.baseState = i, t.memoizedState = i, t.flags & 256) {
          a = Xn(Error(P(423)), t), t = As(e, t, r, n, a);
          break e;
        } else if (r !== a) {
          a = Xn(Error(P(424)), t), t = As(e, t, r, n, a);
          break e;
        } else for (Xe = Xt(t.stateNode.containerInfo.firstChild), Ze = t, oe = !0, ht = null, n = mu(t, null, r, n), t.child = n; n; ) n.flags = n.flags & -3 | 4096, n = n.sibling;
        else {
          if (Kn(), r === a) {
            t = Mt(e, t, n);
            break e;
          }
          Me(e, t, r, n);
        }
        t = t.child;
      }
      return t;
    case 5:
      return xu(t), e === null && xi(t), r = t.type, a = t.pendingProps, i = e !== null ? e.memoizedProps : null, o = a.children, fi(r, a) ? o = null : i !== null && fi(r, i) && (t.flags |= 32), Vu(e, t), Me(e, t, o, n), t.child;
    case 6:
      return e === null && xi(t), null;
    case 13:
      return Hu(e, t, n);
    case 4:
      return fo(t, t.stateNode.containerInfo), r = t.pendingProps, e === null ? t.child = qn(t, null, r, n) : Me(e, t, r, n), t.child;
    case 11:
      return r = t.type, a = t.pendingProps, a = t.elementType === r ? a : pt(r, a), Ls(e, t, r, a, n);
    case 7:
      return Me(e, t, t.pendingProps, n), t.child;
    case 8:
      return Me(e, t, t.pendingProps.children, n), t.child;
    case 12:
      return Me(e, t, t.pendingProps.children, n), t.child;
    case 10:
      e: {
        if (r = t.type._context, a = t.pendingProps, i = t.memoizedProps, o = a.value, ne(Ma, r._currentValue), r._currentValue = o, i !== null) if (yt(i.value, o)) {
          if (i.children === a.children && !We.current) {
            t = Mt(e, t, n);
            break e;
          }
        } else for (i = t.child, i !== null && (i.return = t); i !== null; ) {
          var s = i.dependencies;
          if (s !== null) {
            o = i.child;
            for (var c = s.firstContext; c !== null; ) {
              if (c.context === r) {
                if (i.tag === 1) {
                  c = Tt(-1, n & -n), c.tag = 2;
                  var d = i.updateQueue;
                  if (d !== null) {
                    d = d.shared;
                    var N = d.pending;
                    N === null ? c.next = c : (c.next = N.next, N.next = c), d.pending = c;
                  }
                }
                i.lanes |= n, c = i.alternate, c !== null && (c.lanes |= n), gi(
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
            if (o = i.return, o === null) throw Error(P(341));
            o.lanes |= n, s = o.alternate, s !== null && (s.lanes |= n), gi(o, n, t), o = i.sibling;
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
        Me(e, t, a.children, n), t = t.child;
      }
      return t;
    case 9:
      return a = t.type, r = t.pendingProps.children, Bn(t, n), a = st(a), r = r(a), t.flags |= 1, Me(e, t, r, n), t.child;
    case 14:
      return r = t.type, a = pt(r, t.pendingProps), a = pt(r.type, a), Ms(e, t, r, a, n);
    case 15:
      return Uu(e, t, t.type, t.pendingProps, n);
    case 17:
      return r = t.type, a = t.pendingProps, a = t.elementType === r ? a : pt(r, a), ya(e, t), t.tag = 1, Qe(r) ? (e = !0, za(t)) : e = !1, Bn(t, n), $u(t, r, a), ji(t, r, a, n), wi(null, t, r, !0, e, n);
    case 19:
      return Wu(e, t, n);
    case 22:
      return bu(e, t, n);
  }
  throw Error(P(156, t.tag));
};
function od(e, t) {
  return Dc(e, t);
}
function Vp(e, t, n, r) {
  this.tag = e, this.key = n, this.sibling = this.child = this.return = this.stateNode = this.type = this.elementType = null, this.index = 0, this.ref = null, this.pendingProps = t, this.dependencies = this.memoizedState = this.updateQueue = this.memoizedProps = null, this.mode = r, this.subtreeFlags = this.flags = 0, this.deletions = null, this.childLanes = this.lanes = 0, this.alternate = null;
}
function it(e, t, n, r) {
  return new Vp(e, t, n, r);
}
function Io(e) {
  return e = e.prototype, !(!e || !e.isReactComponent);
}
function Bp(e) {
  if (typeof e == "function") return Io(e) ? 1 : 0;
  if (e != null) {
    if (e = e.$$typeof, e === Wi) return 11;
    if (e === Qi) return 14;
  }
  return 2;
}
function tn(e, t) {
  var n = e.alternate;
  return n === null ? (n = it(e.tag, t, e.key, e.mode), n.elementType = e.elementType, n.type = e.type, n.stateNode = e.stateNode, n.alternate = e, e.alternate = n) : (n.pendingProps = t, n.type = e.type, n.flags = 0, n.subtreeFlags = 0, n.deletions = null), n.flags = e.flags & 14680064, n.childLanes = e.childLanes, n.lanes = e.lanes, n.child = e.child, n.memoizedProps = e.memoizedProps, n.memoizedState = e.memoizedState, n.updateQueue = e.updateQueue, t = e.dependencies, n.dependencies = t === null ? null : { lanes: t.lanes, firstContext: t.firstContext }, n.sibling = e.sibling, n.index = e.index, n.ref = e.ref, n;
}
function Sa(e, t, n, r, a, i) {
  var o = 2;
  if (r = e, typeof e == "function") Io(e) && (o = 1);
  else if (typeof e == "string") o = 5;
  else e: switch (e) {
    case Pn:
      return hn(n.children, a, i, t);
    case Hi:
      o = 8, a |= 8;
      break;
    case Hl:
      return e = it(12, n, t, a | 2), e.elementType = Hl, e.lanes = i, e;
    case Wl:
      return e = it(13, n, t, a), e.elementType = Wl, e.lanes = i, e;
    case Ql:
      return e = it(19, n, t, a), e.elementType = Ql, e.lanes = i, e;
    case xc:
      return al(n, a, i, t);
    default:
      if (typeof e == "object" && e !== null) switch (e.$$typeof) {
        case hc:
          o = 10;
          break e;
        case vc:
          o = 9;
          break e;
        case Wi:
          o = 11;
          break e;
        case Qi:
          o = 14;
          break e;
        case Vt:
          o = 16, r = null;
          break e;
      }
      throw Error(P(130, e == null ? e : typeof e, ""));
  }
  return t = it(o, n, t, a), t.elementType = e, t.type = r, t.lanes = i, t;
}
function hn(e, t, n, r) {
  return e = it(7, e, r, t), e.lanes = n, e;
}
function al(e, t, n, r) {
  return e = it(22, e, r, t), e.elementType = xc, e.lanes = n, e.stateNode = { isHidden: !1 }, e;
}
function Al(e, t, n) {
  return e = it(6, e, null, t), e.lanes = n, e;
}
function Ol(e, t, n) {
  return t = it(4, e.children !== null ? e.children : [], e.key, t), t.lanes = n, t.stateNode = { containerInfo: e.containerInfo, pendingChildren: null, implementation: e.implementation }, t;
}
function Hp(e, t, n, r, a) {
  this.tag = t, this.containerInfo = e, this.finishedWork = this.pingCache = this.current = this.pendingChildren = null, this.timeoutHandle = -1, this.callbackNode = this.pendingContext = this.context = null, this.callbackPriority = 0, this.eventTimes = yl(0), this.expirationTimes = yl(-1), this.entangledLanes = this.finishedLanes = this.mutableReadLanes = this.expiredLanes = this.pingedLanes = this.suspendedLanes = this.pendingLanes = 0, this.entanglements = yl(0), this.identifierPrefix = r, this.onRecoverableError = a, this.mutableSourceEagerHydrationData = null;
}
function Po(e, t, n, r, a, i, o, s, c) {
  return e = new Hp(e, t, n, s, c), t === 1 ? (t = 1, i === !0 && (t |= 8)) : t = 0, i = it(3, null, null, t), e.current = i, i.stateNode = e, i.memoizedState = { element: r, isDehydrated: n, cache: null, transitions: null, pendingSuspenseBoundaries: null }, uo(i), e;
}
function Wp(e, t, n) {
  var r = 3 < arguments.length && arguments[3] !== void 0 ? arguments[3] : null;
  return { $$typeof: In, key: r == null ? null : "" + r, children: e, containerInfo: t, implementation: n };
}
function sd(e) {
  if (!e) return rn;
  e = e._reactInternals;
  e: {
    if (wn(e) !== e || e.tag !== 1) throw Error(P(170));
    var t = e;
    do {
      switch (t.tag) {
        case 3:
          t = t.stateNode.context;
          break e;
        case 1:
          if (Qe(t.type)) {
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
    if (Qe(n)) return su(e, n, t);
  }
  return t;
}
function cd(e, t, n, r, a, i, o, s, c) {
  return e = Po(n, r, !0, e, a, i, o, s, c), e.context = sd(null), n = e.current, r = Oe(), a = en(n), i = Tt(r, a), i.callback = t ?? null, Zt(n, i, a), e.current.lanes = a, Vr(e, a, r), Ge(e, r), e;
}
function ll(e, t, n, r) {
  var a = t.current, i = Oe(), o = en(a);
  return n = sd(n), t.context === null ? t.context = n : t.pendingContext = n, t = Tt(i, o), t.payload = { element: e }, r = r === void 0 ? null : r, r !== null && (t.callback = r), e = Zt(a, t, o), e !== null && (xt(e, a, o, i), va(e, a, o)), o;
}
function Qa(e) {
  if (e = e.current, !e.child) return null;
  switch (e.child.tag) {
    case 5:
      return e.child.stateNode;
    default:
      return e.child.stateNode;
  }
}
function Ks(e, t) {
  if (e = e.memoizedState, e !== null && e.dehydrated !== null) {
    var n = e.retryLane;
    e.retryLane = n !== 0 && n < t ? n : t;
  }
}
function Fo(e, t) {
  Ks(e, t), (e = e.alternate) && Ks(e, t);
}
function Qp() {
  return null;
}
var ud = typeof reportError == "function" ? reportError : function(e) {
  console.error(e);
};
function _o(e) {
  this._internalRoot = e;
}
il.prototype.render = _o.prototype.render = function(e) {
  var t = this._internalRoot;
  if (t === null) throw Error(P(409));
  ll(e, t, null, null);
};
il.prototype.unmount = _o.prototype.unmount = function() {
  var e = this._internalRoot;
  if (e !== null) {
    this._internalRoot = null;
    var t = e.containerInfo;
    Nn(function() {
      ll(null, e, null, null);
    }), t[Dt] = null;
  }
};
function il(e) {
  this._internalRoot = e;
}
il.prototype.unstable_scheduleHydration = function(e) {
  if (e) {
    var t = bc();
    e = { blockedOn: null, target: e, priority: t };
    for (var n = 0; n < Ht.length && t !== 0 && t < Ht[n].priority; n++) ;
    Ht.splice(n, 0, e), n === 0 && Bc(e);
  }
};
function Ro(e) {
  return !(!e || e.nodeType !== 1 && e.nodeType !== 9 && e.nodeType !== 11);
}
function ol(e) {
  return !(!e || e.nodeType !== 1 && e.nodeType !== 9 && e.nodeType !== 11 && (e.nodeType !== 8 || e.nodeValue !== " react-mount-point-unstable "));
}
function qs() {
}
function Gp(e, t, n, r, a) {
  if (a) {
    if (typeof r == "function") {
      var i = r;
      r = function() {
        var d = Qa(o);
        i.call(d);
      };
    }
    var o = cd(t, r, e, 0, null, !1, !1, "", qs);
    return e._reactRootContainer = o, e[Dt] = o.current, Tr(e.nodeType === 8 ? e.parentNode : e), Nn(), o;
  }
  for (; a = e.lastChild; ) e.removeChild(a);
  if (typeof r == "function") {
    var s = r;
    r = function() {
      var d = Qa(c);
      s.call(d);
    };
  }
  var c = Po(e, 0, !1, null, null, !1, !1, "", qs);
  return e._reactRootContainer = c, e[Dt] = c.current, Tr(e.nodeType === 8 ? e.parentNode : e), Nn(function() {
    ll(t, c, n, r);
  }), c;
}
function sl(e, t, n, r, a) {
  var i = n._reactRootContainer;
  if (i) {
    var o = i;
    if (typeof a == "function") {
      var s = a;
      a = function() {
        var c = Qa(o);
        s.call(c);
      };
    }
    ll(t, o, e, a);
  } else o = Gp(n, t, e, a, r);
  return Qa(o);
}
Oc = function(e) {
  switch (e.tag) {
    case 3:
      var t = e.stateNode;
      if (t.current.memoizedState.isDehydrated) {
        var n = fr(t.pendingLanes);
        n !== 0 && (qi(t, n | 1), Ge(t, ve()), !(q & 6) && (Zn = ve() + 500, sn()));
      }
      break;
    case 13:
      Nn(function() {
        var r = Lt(e, 1);
        if (r !== null) {
          var a = Oe();
          xt(r, e, 1, a);
        }
      }), Fo(e, 1);
  }
};
Yi = function(e) {
  if (e.tag === 13) {
    var t = Lt(e, 134217728);
    if (t !== null) {
      var n = Oe();
      xt(t, e, 134217728, n);
    }
    Fo(e, 134217728);
  }
};
Uc = function(e) {
  if (e.tag === 13) {
    var t = en(e), n = Lt(e, t);
    if (n !== null) {
      var r = Oe();
      xt(n, e, t, r);
    }
    Fo(e, t);
  }
};
bc = function() {
  return J;
};
Vc = function(e, t) {
  var n = J;
  try {
    return J = e, t();
  } finally {
    J = n;
  }
};
ni = function(e, t, n) {
  switch (t) {
    case "input":
      if (ql(e, n), t = n.name, n.type === "radio" && t != null) {
        for (n = e; n.parentNode; ) n = n.parentNode;
        for (n = n.querySelectorAll("input[name=" + JSON.stringify("" + t) + '][type="radio"]'), t = 0; t < n.length; t++) {
          var r = n[t];
          if (r !== e && r.form === e.form) {
            var a = Za(r);
            if (!a) throw Error(P(90));
            yc(r), ql(r, a);
          }
        }
      }
      break;
    case "textarea":
      Nc(e, n);
      break;
    case "select":
      t = n.value, t != null && On(e, !!n.multiple, t, !1);
  }
};
Pc = ko;
Fc = Nn;
var Kp = { usingClientEntryPoint: !1, Events: [Hr, Tn, Za, Ec, Ic, ko] }, cr = { findFiberByHostInstance: dn, bundleType: 0, version: "18.3.1", rendererPackageName: "react-dom" }, qp = { bundleType: cr.bundleType, version: cr.version, rendererPackageName: cr.rendererPackageName, rendererConfig: cr.rendererConfig, overrideHookState: null, overrideHookStateDeletePath: null, overrideHookStateRenamePath: null, overrideProps: null, overridePropsDeletePath: null, overridePropsRenamePath: null, setErrorHandler: null, setSuspenseHandler: null, scheduleUpdate: null, currentDispatcherRef: $t.ReactCurrentDispatcher, findHostInstanceByFiber: function(e) {
  return e = Tc(e), e === null ? null : e.stateNode;
}, findFiberByHostInstance: cr.findFiberByHostInstance || Qp, findHostInstancesForRefresh: null, scheduleRefresh: null, scheduleRoot: null, setRefreshHandler: null, getCurrentFiber: null, reconcilerVersion: "18.3.1-next-f1338f8080-20240426" };
if (typeof __REACT_DEVTOOLS_GLOBAL_HOOK__ < "u") {
  var ua = __REACT_DEVTOOLS_GLOBAL_HOOK__;
  if (!ua.isDisabled && ua.supportsFiber) try {
    Ka = ua.inject(qp), wt = ua;
  } catch {
  }
}
et.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED = Kp;
et.createPortal = function(e, t) {
  var n = 2 < arguments.length && arguments[2] !== void 0 ? arguments[2] : null;
  if (!Ro(t)) throw Error(P(200));
  return Wp(e, t, null, n);
};
et.createRoot = function(e, t) {
  if (!Ro(e)) throw Error(P(299));
  var n = !1, r = "", a = ud;
  return t != null && (t.unstable_strictMode === !0 && (n = !0), t.identifierPrefix !== void 0 && (r = t.identifierPrefix), t.onRecoverableError !== void 0 && (a = t.onRecoverableError)), t = Po(e, 1, !1, null, null, n, !1, r, a), e[Dt] = t.current, Tr(e.nodeType === 8 ? e.parentNode : e), new _o(t);
};
et.findDOMNode = function(e) {
  if (e == null) return null;
  if (e.nodeType === 1) return e;
  var t = e._reactInternals;
  if (t === void 0)
    throw typeof e.render == "function" ? Error(P(188)) : (e = Object.keys(e).join(","), Error(P(268, e)));
  return e = Tc(t), e = e === null ? null : e.stateNode, e;
};
et.flushSync = function(e) {
  return Nn(e);
};
et.hydrate = function(e, t, n) {
  if (!ol(t)) throw Error(P(200));
  return sl(null, e, t, !0, n);
};
et.hydrateRoot = function(e, t, n) {
  if (!Ro(e)) throw Error(P(405));
  var r = n != null && n.hydratedSources || null, a = !1, i = "", o = ud;
  if (n != null && (n.unstable_strictMode === !0 && (a = !0), n.identifierPrefix !== void 0 && (i = n.identifierPrefix), n.onRecoverableError !== void 0 && (o = n.onRecoverableError)), t = cd(t, null, e, 1, n ?? null, a, !1, i, o), e[Dt] = t.current, Tr(e), r) for (e = 0; e < r.length; e++) n = r[e], a = n._getVersion, a = a(n._source), t.mutableSourceEagerHydrationData == null ? t.mutableSourceEagerHydrationData = [n, a] : t.mutableSourceEagerHydrationData.push(
    n,
    a
  );
  return new il(t);
};
et.render = function(e, t, n) {
  if (!ol(t)) throw Error(P(200));
  return sl(null, e, t, !1, n);
};
et.unmountComponentAtNode = function(e) {
  if (!ol(e)) throw Error(P(40));
  return e._reactRootContainer ? (Nn(function() {
    sl(null, null, e, !1, function() {
      e._reactRootContainer = null, e[Dt] = null;
    });
  }), !0) : !1;
};
et.unstable_batchedUpdates = ko;
et.unstable_renderSubtreeIntoContainer = function(e, t, n, r) {
  if (!ol(n)) throw Error(P(200));
  if (e == null || e._reactInternals === void 0) throw Error(P(38));
  return sl(e, t, n, !1, r);
};
et.version = "18.3.1-next-f1338f8080-20240426";
function dd() {
  if (!(typeof __REACT_DEVTOOLS_GLOBAL_HOOK__ > "u" || typeof __REACT_DEVTOOLS_GLOBAL_HOOK__.checkDCE != "function"))
    try {
      __REACT_DEVTOOLS_GLOBAL_HOOK__.checkDCE(dd);
    } catch (e) {
      console.error(e);
    }
}
dd(), dc.exports = et;
var Yp = dc.exports, fd, Ys = Yp;
fd = Ys.createRoot, Ys.hydrateRoot;
class Xp extends Error {
  constructor(t, n) {
    super(t), this.status = n, this.name = "ApiError";
  }
}
function Zp(e, t) {
  async function n(r, a = {}) {
    const i = { ...a.headers ?? {} }, o = t();
    o && (i.Authorization = "Bearer " + o);
    let s;
    a.body !== void 0 && (i["Content-Type"] = "application/json", s = JSON.stringify(a.body));
    const c = await e(r, { method: a.method ?? "GET", headers: i, body: s });
    if (!c.ok) {
      let N = `HTTP ${c.status}`;
      try {
        const u = await c.json();
        N = u.detail || u.title || N;
      } catch {
      }
      throw new Xp(N, c.status);
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
const pd = g.createContext(null);
function Ct() {
  const e = g.useContext(pd);
  if (!e) throw new Error("Fuera del módulo de documentos");
  return e;
}
function Jp(e) {
  return Zp((t, n) => fetch(t, n), e.token);
}
async function Li(e, t) {
  const n = e.token(), r = await fetch(t, { headers: n ? { Authorization: "Bearer " + n } : {} });
  if (!r.ok) throw new Error("No se pudo generar el documento");
  const a = URL.createObjectURL(await r.blob());
  window.open(a, "_blank"), setTimeout(() => URL.revokeObjectURL(a), 6e4);
}
function md(e, t) {
  return `${e.toLowerCase().normalize("NFD").replace(/[̀-ͯ]/g, "").replace(/[^a-z0-9]+/g, "-").replace(/^-|-$/g, "").slice(0, 60) || "listado"}-${(/* @__PURE__ */ new Date()).toISOString().slice(0, 10)}.${t}`;
}
function hd(e, t) {
  const n = URL.createObjectURL(e), r = document.createElement("a");
  r.href = n, r.download = t, document.body.appendChild(r), r.click(), r.remove(), setTimeout(() => URL.revokeObjectURL(n), 6e4);
}
async function em(e, t) {
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
  hd(await n.blob(), md(t.titulo, "xlsx"));
}
function tm(e) {
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
function nm(e) {
  hd(new Blob(["\uFEFF" + tm(e)], { type: "text/csv;charset=utf-8" }), md(e.titulo, "csv"));
}
const vd = new Intl.NumberFormat("es-ES", { minimumFractionDigits: 2, maximumFractionDigits: 2 }), rm = new Intl.NumberFormat("es-ES", { maximumFractionDigits: 3 }), z = (e) => `${vd.format(Number(e) || 0)} €`, ke = (e) => vd.format(Number(e) || 0), $e = (e) => rm.format(Number(e) || 0), je = (e) => {
  if (!e) return "";
  const t = String(e).slice(0, 10).split("-");
  return t.length === 3 ? `${t[2]}/${t[1]}/${t[0]}` : String(e);
}, gt = () => (/* @__PURE__ */ new Date()).toISOString().slice(0, 10), Ae = (e) => Math.round((e + Number.EPSILON) * 100) / 100;
let am = 0;
const Qr = () => `l${Date.now().toString(36)}${(++am).toString(36)}`;
function vn(e, t) {
  const [n, r] = g.useState(e);
  return g.useEffect(() => {
    const a = setTimeout(() => r(e), t);
    return () => clearTimeout(a);
  }, [e, t]), n;
}
function Gr() {
  const e = g.useRef(0);
  return () => {
    const t = ++e.current;
    return () => t === e.current;
  };
}
function To(e) {
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
const Xs = { presupuesto: "Presupuestos", pedido: "Pedidos de venta", factura: "Facturas emitidas", compra: "Pedidos de compra", gasto: "Facturas de proveedor y gastos" }, lm = { presupuesto: "+ Nuevo presupuesto", pedido: "+ Nuevo pedido", factura: "+ Nueva factura", compra: "+ Nuevo pedido", gasto: "+ Registrar factura" }, im = {
  presupuesto: ["Borrador", "Aceptado", "Rechazado"],
  pedido: ["Borrador", "Confirmado", "ServidoParcial", "Servido", "Facturado", "Cancelado"],
  factura: ["Emitida", "Rectificada", "Anulada"],
  compra: ["Borrador", "Confirmado", "RecibidoParcial", "Recibido", "Facturado", "Cancelado"],
  gasto: ["Registrado", "Anulado"]
}, Ul = { numero: "numero", fecha: "fecha", tercero: "tercero", base: "base", impuestos: "impuestos", total: "total" }, Zs = 50, Mi = (e) => e === "Anulada" || e === "Anulado" || e === "Cancelado";
function Js(e, t) {
  const n = { documentos: 0, baseImponible: 0, impuestos: 0, retenciones: 0, total: 0, pendiente: 0, vencido: 0, documentosVencidos: 0 };
  for (const r of e) {
    if (Mi(r.estado)) continue;
    n.documentos++, n.baseImponible += r.base, n.impuestos += r.impuestos, n.total += r.total;
    const a = r.pendiente ?? 0;
    a > 0 && (n.pendiente += a, r.vencimiento && r.vencimiento < t && (n.vencido += a, n.documentosVencidos++));
  }
  return n.baseImponible = Ae(n.baseImponible), n.impuestos = Ae(n.impuestos), n.total = Ae(n.total), n.pendiente = Ae(n.pendiente), n.vencido = Ae(n.vencido), n;
}
function bl(e, t, n) {
  const r = (a) => t === "numero" || t === "tercero" || t === "estado" ? a[t].toLowerCase() : t === "fecha" ? a.fecha : a[t] ?? 0;
  return [...e].sort((a, i) => {
    const o = r(a), s = r(i), c = typeof o == "number" && typeof s == "number" ? o - s : String(o).localeCompare(String(s), "es", { numeric: !0 });
    return n ? -c : c;
  });
}
const om = (e, t) => ({
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
}), sm = (e, t) => {
  var n, r;
  return {
    id: e.id,
    numero: e.numeroFactura ?? "—",
    fecha: e.fecha,
    tercero: `${e.proveedorTexto ?? ""}${e.numeroFactura ? "" : ` · ${e.concepto}`}`,
    terceroId: e.proveedorId,
    base: e.baseImponible,
    impuestos: Ae(e.cuotaIva + (e.recargoTotal || 0)),
    total: e.total,
    estado: e.estado === "Anulado" ? "Anulada" : e.estado,
    extra: e.esRectificativa ? "Rectificativa" : void 0,
    pendiente: t ? t[e.id] ?? 0 : null,
    vencimiento: ((r = (n = e.vencimientos) == null ? void 0 : n[0]) == null ? void 0 : r.fecha) ?? e.fecha
  };
};
function cm(e) {
  const { api: t, navegar: n, anfitrion: r } = Ct(), a = e.tipo, i = a === "factura" || a === "gasto", o = a === "compra" || a === "gasto", [s, c] = g.useState(""), [d, N] = g.useState(""), [u, h] = g.useState(""), [v, y] = g.useState(""), [S, $] = g.useState(""), [p, f] = g.useState(""), [x, E] = g.useState(""), [T, F] = g.useState(""), [D, k] = g.useState(""), [M, O] = g.useState({ campo: "fecha", desc: !0 }), [R, Q] = g.useState(1), [se, Fe] = g.useState(null), [Le, fe] = g.useState(0), [xe, I] = g.useState(null), [j, w] = g.useState([]), [b, W] = g.useState([]), [K, pe] = g.useState(""), [me, te] = g.useState(!1), U = vn(s, 250), ut = vn(x, 350), Ke = vn(T, 350), dt = Gr(), Et = gt();
  g.useEffect(() => {
    t.get(o ? "/proveedores" : "/clientes").then((m) => w([...m].sort((L, _) => L.nombre.localeCompare(_.nombre, "es")))).catch(() => w([])), a === "factura" && t.get("/series").then((m) => W([...new Set(m.filter((L) => L.tipoDocumento === "Factura" || L.tipoDocumento === 0).map((L) => L.prefijo))].sort())).catch(() => W([]));
  }, [t, a, o]), g.useEffect(() => Q(1), [U, d, u, v, S, p, ut, Ke, D, M, a]);
  const At = (m, L) => {
    const _ = new URLSearchParams({ pagina: String(m), tamanoPagina: String(L) });
    U.trim() && _.set("texto", U.trim()), d && _.set("estado", d === "Anulada" && a === "gasto" ? "Anulado" : d), u && _.set("desde", u), v && _.set("hasta", v), S && _.set(a === "gasto" ? "proveedorId" : "clienteId", S), p && a === "factura" && _.set("serie", p);
    const B = parseFloat(ut.replace(/\./g, "").replace(",", ".")), H = parseFloat(Ke.replace(/\./g, "").replace(",", "."));
    isNaN(B) || _.set("importeMin", String(B)), isNaN(H) || _.set("importeMax", String(H)), D && _.set("cobro", D);
    const Z = Ul[M.campo];
    return Z && (_.set("orden", Z === "tercero" ? a === "gasto" ? "proveedor" : "cliente" : Z), _.set("desc", String(M.desc))), _;
  }, kn = async (m, L) => {
    if (a === "factura") {
      const B = await t.get(`/facturas/buscar?${At(m, L)}`);
      return { r: B, filas: B.elementos.map((H) => om(H, B.pendientes)) };
    }
    const _ = await t.get(`/gastos/buscar?${At(m, L)}`);
    return { r: _, filas: _.elementos.map((B) => sm(B, _.pendientes)) };
  };
  g.useEffect(() => {
    pe("");
    const m = dt();
    (async () => {
      if (i) {
        const { r: _, filas: B } = await kn(R, Zs);
        return m() && (fe(_.total), I(_.totales ?? null)), B;
      }
      switch (a) {
        case "presupuesto":
          return (await t.get("/presupuestos")).map((_) => ({ id: _.id, numero: _.numeroCompleto, fecha: _.fecha, tercero: _.clienteNombre, terceroId: _.clienteId, base: _.baseImponible ?? _.total, impuestos: _.cuotaIva ?? 0, total: _.total, estado: _.estado }));
        case "pedido":
          return (await t.get("/pedidos-venta")).map((_) => {
            const B = Ae(_.lineas.reduce((H, Z) => H + Z.base, 0));
            return { id: _.id, numero: _.numeroCompleto, fecha: _.fecha, tercero: _.clienteNombre, terceroId: _.clienteId, base: B, impuestos: Ae(_.total - B), total: _.total, estado: _.estado };
          });
        default:
          return (await t.get("/compras/pedidos")).map((_) => {
            const B = Ae(_.lineas.reduce((H, Z) => H + Z.importe, 0));
            return { id: _.id, numero: _.numeroCompleto, fecha: _.fecha, tercero: _.proveedorTexto, terceroId: _.proveedorId, base: B, impuestos: Ae(_.total - B), total: _.total, estado: _.estado, extra: _.empresaOrigenId ? "Intragrupo" : void 0 };
          });
      }
    })().then((_) => m() && Fe(_)).catch((_) => m() && (pe(_.message), Fe([])));
  }, [t, a, R, U, d, u, v, S, p, ut, Ke, D, M.campo, M.desc]);
  const X = g.useMemo(() => {
    if (!se) return [];
    if (i) return Ul[M.campo] ? se : bl(se, M.campo, M.desc);
    const m = U.trim().toLowerCase(), L = parseFloat(ut.replace(/\./g, "").replace(",", ".")), _ = parseFloat(Ke.replace(/\./g, "").replace(",", ".")), B = se.filter((H) => (!m || H.numero.toLowerCase().includes(m) || H.tercero.toLowerCase().includes(m)) && (!d || H.estado === d) && (!u || H.fecha >= u) && (!v || H.fecha <= v) && (!S || H.terceroId === S) && (isNaN(L) || H.total >= L) && (isNaN(_) || H.total <= _));
    return bl(B, M.campo, M.desc);
  }, [se, U, d, u, v, S, ut, Ke, M, i]), Se = i ? xe : Js(X, Et), qe = i ? Math.max(1, Math.ceil(Le / Zs)) : 1, nt = [d, u, v, S, p, x, T, D].filter(Boolean).length, Ot = o ? "Proveedor" : "Cliente";
  function ie() {
    c(""), N(""), h(""), y(""), $(""), f(""), E(""), F(""), k("");
  }
  function Ve(m, L, _ = !1) {
    const B = M.campo === m;
    return /* @__PURE__ */ l.jsxs("th", { className: (_ ? "num " : "") + "dx-ordenable" + (B ? " activo" : ""), onClick: () => O({ campo: m, desc: B ? !M.desc : m === "fecha" || _ }), title: `Ordenar por ${L.toLowerCase()}`, children: [
      L,
      /* @__PURE__ */ l.jsx("span", { className: "dx-flecha", children: B ? M.desc ? "▼" : "▲" : "" })
    ] });
  }
  async function It() {
    if (!i) return X;
    const m = [];
    for (let L = 1; L <= 500; L++) {
      const { r: _, filas: B } = await kn(L, 200);
      if (m.push(...B), m.length >= _.total || B.length === 0) break;
    }
    return Ul[M.campo] ? m : bl(m, M.campo, M.desc);
  }
  async function Ut(m) {
    te(!0);
    try {
      const L = await It(), _ = i, B = i ? xe : Js(L, Et), H = {
        titulo: Xs[a],
        columnas: [
          { titulo: "Número", tipo: "texto" },
          { titulo: "Fecha", tipo: "fecha" },
          { titulo: Ot, tipo: "texto" },
          { titulo: "Estado", tipo: "texto" },
          { titulo: "Base", tipo: "moneda", total: "no" },
          { titulo: "Impuestos", tipo: "moneda", total: "no" },
          { titulo: "Total", tipo: "moneda", total: "no" },
          ..._ ? [{ titulo: "Pendiente", tipo: "moneda", total: "no" }, { titulo: "Vencimiento", tipo: "fecha" }] : []
        ],
        filas: L.map((Z) => [Z.numero + (Z.extra ? ` (${Z.extra})` : ""), je(Z.fecha), Z.tercero, Z.estado, Z.base, Z.impuestos, Z.total, ..._ ? [Z.pendiente ?? 0, je(Z.vencimiento)] : []]),
        totales: B ? [`Total · ${B.documentos} (sin anulados)`, null, null, null, B.baseImponible, B.impuestos, B.total, ..._ ? [B.pendiente, null] : []] : void 0
      };
      m === "xlsx" ? await em(r.token(), H) : nm(H), r.aviso(`Exportados ${L.length} documento(s).`, "ok");
    } catch (L) {
      r.aviso("No se pudo exportar: " + L.message, "err");
    } finally {
      te(!1);
    }
  }
  return /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel-head", children: [
      /* @__PURE__ */ l.jsx("h2", { children: Xs[a] }),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-acciones", children: [
        /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", disabled: me || !X.length, onClick: () => Ut("xlsx"), title: "Exportar a Excel todo lo filtrado", children: me ? "Exportando…" : "↓ Excel" }),
        /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", disabled: me || !X.length, onClick: () => Ut("csv"), title: "Exportar a CSV todo lo filtrado", children: "↓ CSV" }),
        /* @__PURE__ */ l.jsx("button", { className: "btn small", onClick: () => n({ tipo: a, pantalla: "editor" }), children: lm[a] })
      ] })
    ] }),
    /* @__PURE__ */ l.jsxs("div", { className: "dx-filtros", children: [
      /* @__PURE__ */ l.jsx("input", { placeholder: o ? "Número, proveedor o concepto…" : "Número, cliente o NIF…", value: s, onChange: (m) => c(m.target.value), autoFocus: !0 }),
      /* @__PURE__ */ l.jsxs("select", { value: d, onChange: (m) => N(m.target.value), "aria-label": "Estado", children: [
        /* @__PURE__ */ l.jsx("option", { value: "", children: "Todos los estados" }),
        im[a].map((m) => /* @__PURE__ */ l.jsx("option", { value: m, children: m }, m))
      ] }),
      /* @__PURE__ */ l.jsx("input", { type: "date", value: u, onChange: (m) => h(m.target.value), title: "Desde", "aria-label": "Desde" }),
      /* @__PURE__ */ l.jsx("input", { type: "date", value: v, onChange: (m) => y(m.target.value), title: "Hasta", "aria-label": "Hasta" })
    ] }),
    /* @__PURE__ */ l.jsxs("div", { className: "dx-filtros dx-filtros2", children: [
      /* @__PURE__ */ l.jsxs("select", { value: S, onChange: (m) => $(m.target.value), "aria-label": Ot, children: [
        /* @__PURE__ */ l.jsx("option", { value: "", children: o ? "Todos los proveedores" : "Todos los clientes" }),
        j.map((m) => /* @__PURE__ */ l.jsxs("option", { value: m.id, children: [
          m.nombre,
          m.nifFiscal ? ` · ${m.nifFiscal}` : ""
        ] }, m.id))
      ] }),
      a === "factura" && /* @__PURE__ */ l.jsxs("select", { value: p, onChange: (m) => f(m.target.value), "aria-label": "Serie", children: [
        /* @__PURE__ */ l.jsx("option", { value: "", children: "Todas las series" }),
        b.map((m) => /* @__PURE__ */ l.jsxs("option", { value: m, children: [
          "Serie ",
          m
        ] }, m))
      ] }),
      /* @__PURE__ */ l.jsx("input", { inputMode: "decimal", placeholder: "Importe mín.", value: x, onChange: (m) => E(m.target.value), "aria-label": "Importe mínimo" }),
      /* @__PURE__ */ l.jsx("input", { inputMode: "decimal", placeholder: "Importe máx.", value: T, onChange: (m) => F(m.target.value), "aria-label": "Importe máximo" }),
      i && /* @__PURE__ */ l.jsxs("select", { value: D, onChange: (m) => k(m.target.value), "aria-label": a === "gasto" ? "Estado de pago" : "Estado de cobro", children: [
        /* @__PURE__ */ l.jsx("option", { value: "", children: a === "gasto" ? "Pagadas y pendientes" : "Cobradas y pendientes" }),
        /* @__PURE__ */ l.jsx("option", { value: "pendiente", children: "Pendientes" }),
        /* @__PURE__ */ l.jsx("option", { value: "vencida", children: "Vencidas" }),
        /* @__PURE__ */ l.jsx("option", { value: a === "gasto" ? "pagada" : "cobrada", children: a === "gasto" ? "Pagadas" : "Cobradas" })
      ] }),
      (nt > 0 || s) && /* @__PURE__ */ l.jsxs("button", { className: "btn small secondary", onClick: ie, children: [
        "Limpiar",
        nt ? ` (${nt})` : ""
      ] })
    ] }),
    K && /* @__PURE__ */ l.jsx("p", { className: "dx-rojo", children: K }),
    se === null ? /* @__PURE__ */ l.jsx("p", { className: "muted", children: "Cargando…" }) : X.length === 0 ? /* @__PURE__ */ l.jsx("p", { className: "muted", children: "No hay documentos con estos filtros." }) : /* @__PURE__ */ l.jsxs("table", { className: "dx-lista-docs", children: [
      /* @__PURE__ */ l.jsx("thead", { children: /* @__PURE__ */ l.jsxs("tr", { children: [
        Ve("numero", "Número"),
        Ve("fecha", "Fecha"),
        Ve("tercero", Ot),
        Ve("estado", "Estado"),
        Ve("base", "Base", !0),
        Ve("impuestos", "Impuestos", !0),
        Ve("total", "Total", !0),
        i && Ve("pendiente", "Pendiente", !0)
      ] }) }),
      /* @__PURE__ */ l.jsx("tbody", { children: X.map((m) => {
        const L = (m.pendiente ?? 0) > 0 && !!m.vencimiento && m.vencimiento < Et;
        return /* @__PURE__ */ l.jsxs("tr", { onClick: () => n({ tipo: a, pantalla: "vista", id: m.id }), tabIndex: 0, onKeyDown: (_) => _.key === "Enter" && n({ tipo: a, pantalla: "vista", id: m.id }), className: Mi(m.estado) ? "dx-anulado" : void 0, children: [
          /* @__PURE__ */ l.jsxs("td", { className: "mono", children: [
            /* @__PURE__ */ l.jsx("strong", { children: m.numero }),
            m.extra && /* @__PURE__ */ l.jsx("span", { className: "pill part", style: { marginLeft: 6 }, children: m.extra })
          ] }),
          /* @__PURE__ */ l.jsx("td", { children: je(m.fecha) }),
          /* @__PURE__ */ l.jsx("td", { children: m.tercero }),
          /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx("span", { className: To(m.estado), children: m.estado }) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: z(m.base) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: z(m.impuestos) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: z(m.total) }) }),
          i && /* @__PURE__ */ l.jsx("td", { className: "num", children: (m.pendiente ?? 0) > 0 ? /* @__PURE__ */ l.jsx("strong", { className: L ? "dx-rojo" : void 0, title: L ? `Vencida el ${je(m.vencimiento)}` : `Vence el ${je(m.vencimiento)}`, children: z(m.pendiente) }) : /* @__PURE__ */ l.jsx("span", { className: "muted", children: Mi(m.estado) || m.estado === "Rectificada" ? "—" : a === "gasto" ? "Pagada" : "Cobrada" }) })
        ] }, m.id);
      }) }),
      Se && /* @__PURE__ */ l.jsx("tfoot", { children: /* @__PURE__ */ l.jsxs("tr", { className: "dx-total", children: [
        /* @__PURE__ */ l.jsxs("td", { colSpan: 4, children: [
          /* @__PURE__ */ l.jsxs("strong", { children: [
            "Total · ",
            Se.documentos
          ] }),
          " ",
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: i ? `documento${Se.documentos === 1 ? "" : "s"} de todo el filtro (${qe} página${qe === 1 ? "" : "s"}) · sin anulados` : "sin anulados ni cancelados" }),
          i && Se.vencido > 0 && /* @__PURE__ */ l.jsxs("span", { className: "dx-rojo", style: { marginLeft: 8 }, children: [
            "· vencido ",
            z(Se.vencido),
            " (",
            Se.documentosVencidos,
            ")"
          ] })
        ] }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: z(Se.baseImponible) }) }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: z(Se.impuestos) }) }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: z(Se.total) }) }),
        i && /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: z(Se.pendiente) }) })
      ] }) })
    ] }),
    qe > 1 && /* @__PURE__ */ l.jsxs("div", { className: "dx-paginas", children: [
      /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", disabled: R <= 1, onClick: () => Q(R - 1), children: "←" }),
      /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
        "Página ",
        R,
        " de ",
        qe,
        " · ",
        Le,
        " documentos"
      ] }),
      /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", disabled: R >= qe, onClick: () => Q(R + 1), children: "→" })
    ] })
  ] });
}
function an(e) {
  return g.useEffect(() => {
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
function um(e) {
  const { api: t } = Ct(), [n, r] = g.useState(!1), [a, i] = g.useState([]), [o, s] = g.useState(null), [c, d] = g.useState(0), N = vn(e.texto, 180), u = o === e.texto.trim() ? a : [], h = Gr();
  g.useEffect(() => {
    if (!n) return;
    const y = h(), S = encodeURIComponent(N.trim());
    t.get(`/productos/buscar?texto=${S}&tamanoPagina=12`).then(($) => y() && (i($.elementos ?? []), s(N.trim()), d(0))).catch(() => y() && (i([]), s(N.trim())));
  }, [N, n]);
  function v(y) {
    var S;
    if (n && y.key === "Enter" && e.texto.trim() && !u.length) {
      y.preventDefault(), y.stopPropagation();
      return;
    }
    if (n && u.length) {
      if (y.key === "ArrowDown") return y.preventDefault(), d(($) => Math.min($ + 1, u.length - 1));
      if (y.key === "ArrowUp") return y.preventDefault(), d(($) => Math.max($ - 1, 0));
      if (y.key === "Enter") {
        y.preventDefault(), y.stopPropagation(), e.alElegir(u[c]), r(!1);
        return;
      }
    }
    if (y.key === "Escape") return r(!1);
    if (y.key === "F2") return y.preventDefault(), r(!0);
    (S = e.alTeclaFuera) == null || S.call(e, y);
  }
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-buscador", children: [
    /* @__PURE__ */ l.jsx(
      "input",
      {
        value: e.texto,
        placeholder: "Buscar artículo…",
        autoFocus: e.autoFocus,
        onChange: (y) => (e.alCambiarTexto(y.target.value), r(!0)),
        onFocus: (y) => y.target.select(),
        onBlur: () => setTimeout(() => r(!1), 150),
        onKeyDown: v,
        ...Object.fromEntries(Object.entries(e.datos ?? {}).map(([y, S]) => [`data-${y}`, S]))
      }
    ),
    n && u.length > 0 && /* @__PURE__ */ l.jsx("div", { className: "dx-lista", children: u.map((y, S) => /* @__PURE__ */ l.jsxs(
      "div",
      {
        className: "dx-opcion" + (S === c ? " activa" : ""),
        onMouseDown: ($) => ($.preventDefault(), e.alElegir(y), r(!1)),
        onMouseEnter: () => d(S),
        children: [
          /* @__PURE__ */ l.jsxs("span", { children: [
            y.referencia && /* @__PURE__ */ l.jsxs("span", { className: "mono muted", children: [
              y.referencia,
              " · "
            ] }),
            /* @__PURE__ */ l.jsx("strong", { children: y.nombre }),
            y.familia && /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
              " · ",
              y.familia
            ] })
          ] }),
          /* @__PURE__ */ l.jsxs("span", { className: "muted", style: { whiteSpace: "nowrap" }, children: [
            z(e.precioDe ? e.precioDe(y) : y.precioUnitario),
            "/",
            y.unidad,
            y.controlarStock && /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
              " · stock ",
              $e(y.stock)
            ] })
          ] })
        ]
      },
      y.id
    )) })
  ] });
}
function zo(e) {
  const [t, n] = g.useState(""), [r, a] = g.useState(!1), [i, o] = g.useState(0), s = e.terceros.find((u) => u.id === e.valor), c = g.useMemo(() => {
    const u = t.trim().toLowerCase();
    return e.terceros.filter((h) => h.activo !== !1 && (!u || h.nombre.toLowerCase().includes(u) || (h.nifFiscal ?? "").toLowerCase().includes(u))).slice(0, 30);
  }, [t, e.terceros]), d = g.useRef(null);
  function N(u) {
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
            if (u.key === "ArrowDown") return u.preventDefault(), o((h) => Math.min(h + 1, c.length - 1));
            if (u.key === "ArrowUp") return u.preventDefault(), o((h) => Math.max(h - 1, 0));
            if (u.key === "Enter" && c[i]) return u.preventDefault(), N(c[i]);
            if (u.key === "Escape") return a(!1);
          }
        }
      ),
      r && c.length > 0 && /* @__PURE__ */ l.jsx("div", { className: "dx-lista", children: c.map((u, h) => /* @__PURE__ */ l.jsxs("div", { className: "dx-opcion" + (h === i ? " activa" : ""), onMouseDown: (v) => (v.preventDefault(), N(u)), onMouseEnter: () => o(h), children: [
        /* @__PURE__ */ l.jsx("strong", { children: u.nombre }),
        /* @__PURE__ */ l.jsx("span", { className: "muted", children: [u.nifFiscal, u.poblacion].filter(Boolean).join(" · ") })
      ] }, u.id)) })
    ] })
  ] });
}
const dm = { Porcentaje: "%", PorUnidad: "€/ud", PorKilo: "€/kg", Importe: "€" };
function Do(e) {
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
          dm[s.calculo],
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
function cl(e) {
  var t;
  return (t = e.conceptos) != null && t.length ? /* @__PURE__ */ l.jsx("div", { className: "dx-aplicados", children: e.conceptos.map((n, r) => /* @__PURE__ */ l.jsxs("div", { children: [
    "· ",
    n.nombre,
    n.calculo === "Porcentaje" ? ` (${$e(n.valor)} %)` : "",
    n.repartido ? " · del documento" : "",
    n.efecto === "Coste" ? " · coste" : "",
    ": ",
    /* @__PURE__ */ l.jsx("strong", { children: z(n.importe) })
  ] }, r)) }) : null;
}
const wr = () => ({ clave: Qr(), productoId: null, descripcion: "", cantidad: 1, precio: null, dto: 0, iva: null });
function xd(e) {
  const t = g.useRef(null), [n, r] = g.useState(/* @__PURE__ */ new Set()), a = e.modo === "venta", i = a ? 7 : 5, o = (u, h) => e.alCambiar(e.lineas.map((v) => v.clave === u ? { ...v, ...h } : v)), s = (u) => {
    const h = e.lineas.filter((v) => v.clave !== u);
    e.alCambiar(h.length ? h : [wr()]);
  };
  function c(u, h) {
    var y;
    const v = (y = t.current) == null ? void 0 : y.querySelector(`[data-f="${u}"][data-c="${h}"]`);
    v == null || v.focus(), v instanceof HTMLInputElement && v.select();
  }
  function d(u) {
    const h = u.target, v = Number(h.dataset.f), y = Number(h.dataset.c);
    if (!(Number.isNaN(v) || Number.isNaN(y)))
      if (u.key === "Enter") {
        if (u.preventDefault(), y < i - 1) return c(v, y + 1);
        v === e.lineas.length - 1 ? (e.alCambiar([...e.lineas, wr()]), setTimeout(() => c(v + 1, 0), 30)) : c(v + 1, 0);
      } else u.key === "ArrowDown" && h.tagName !== "SELECT" ? (u.preventDefault(), c(Math.min(v + 1, e.lineas.length - 1), y)) : u.key === "ArrowUp" && h.tagName !== "SELECT" && (u.preventDefault(), c(Math.max(v - 1, 0), y));
  }
  const N = (u) => r((h) => {
    const v = new Set(h);
    return v.has(u) ? v.delete(u) : v.add(u), v;
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
      /* @__PURE__ */ l.jsx("tbody", { children: e.lineas.map((u, h) => {
        const v = e.calculos[h], y = (v == null ? void 0 : v.conceptos) ?? [], S = a && u.controlarStock && u.stock != null && u.cantidad > u.stock, $ = v && v.margen != null && v.importe ? v.margen / v.importe * 100 : null;
        return [
          /* @__PURE__ */ l.jsxs("tr", { className: h % 2 ? "dx-par" : "", children: [
            /* @__PURE__ */ l.jsxs("td", { children: [
              e.soloLectura ? /* @__PURE__ */ l.jsx("span", { className: "mono", children: u.referencia ?? "" }) : /* @__PURE__ */ l.jsx(
                um,
                {
                  texto: u.referencia ?? (u.productoId ? u.descripcion : ""),
                  alCambiarTexto: (p) => o(u.clave, { referencia: p, ...p === "" ? { productoId: null } : {} }),
                  alElegir: (p) => (e.alElegirArticulo(u.clave, p), c(h, 2)),
                  precioDe: a ? void 0 : (p) => p.precioCompraPorUnidadCompra ?? p.precioCompra,
                  datos: { f: h, c: 0 }
                }
              ),
              u.productoId && /* @__PURE__ */ l.jsxs("div", { className: "dx-sub", children: [
                u.unidad && /* @__PURE__ */ l.jsx("span", { children: u.unidad }),
                u.controlarStock && /* @__PURE__ */ l.jsxs("span", { className: S ? "dx-rojo" : "", children: [
                  " · stock ",
                  $e(u.stock)
                ] })
              ] })
            ] }),
            /* @__PURE__ */ l.jsxs("td", { children: [
              /* @__PURE__ */ l.jsx(
                "input",
                {
                  "data-f": h,
                  "data-c": 1,
                  value: u.descripcion,
                  placeholder: u.productoId ? "" : "Descripción (línea libre)",
                  disabled: e.soloLectura,
                  onChange: (p) => o(u.clave, { descripcion: p.target.value })
                }
              ),
              y.length > 0 && !n.has(u.clave) && /* @__PURE__ */ l.jsx("button", { type: "button", className: "dx-resumen-conc", onClick: () => N(u.clave), title: "Ver y cambiar los conceptos", children: y.map((p) => `${p.importe < 0 ? "−" : "+"} ${p.codigo.toLowerCase()} ${ke(Math.abs(p.importe))}${p.efecto === "Coste" ? " (coste)" : ""}`).join(" · ") })
            ] }),
            /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx(
              "input",
              {
                "data-f": h,
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
                "data-f": h,
                "data-c": 3,
                className: "num" + (u.precio == null ? " dx-auto" : ""),
                type: "number",
                step: "0.0001",
                disabled: e.soloLectura,
                value: u.precio ?? "",
                placeholder: v ? ke(v.precio) : "",
                title: u.precio == null ? "Precio de la tarifa del cliente o del artículo (escribe uno para fijarlo)" : "",
                onChange: (p) => o(u.clave, { precio: p.target.value === "" ? null : Number(p.target.value) })
              }
            ) }),
            a && /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx(
              "input",
              {
                "data-f": h,
                "data-c": 4,
                className: "num",
                type: "number",
                step: "0.01",
                value: u.dto || (u.precio == null && (v != null && v.dto) ? v.dto : 0),
                disabled: e.soloLectura,
                onChange: (p) => o(u.clave, { dto: Number(p.target.value), precio: u.precio ?? (v == null ? void 0 : v.precio) ?? null })
              }
            ) }),
            a && /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsxs("select", { "data-f": h, "data-c": 5, value: u.iva ?? (v == null ? void 0 : v.iva) ?? "", disabled: e.soloLectura, onChange: (p) => o(u.clave, { iva: p.target.value || null }), children: [
              !u.iva && !(v != null && v.iva) && /* @__PURE__ */ l.jsx("option", { value: "", children: "Del artículo" }),
              e.ivas.map((p) => /* @__PURE__ */ l.jsx("option", { value: p.codigo, children: p.nombre }, p.codigo))
            ] }) }),
            /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: v ? z(v.importe) : "—" }) }),
            /* @__PURE__ */ l.jsx("td", { className: "num", children: a ? (v == null ? void 0 : v.margen) != null && /* @__PURE__ */ l.jsxs("span", { className: v.margen < 0 ? "dx-rojo" : "muted", children: [
              z(v.margen),
              $ != null && /* @__PURE__ */ l.jsxs("div", { className: "dx-sub", children: [
                ke($),
                " %"
              ] })
            ] }) : (v == null ? void 0 : v.costeUnitarioEntrada) != null && /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
              z(v.costeUnitarioEntrada),
              "/ud"
            ] }) }),
            /* @__PURE__ */ l.jsxs("td", { className: "right", style: { whiteSpace: "nowrap" }, children: [
              !e.soloLectura && e.catalogo.length > 0 && /* @__PURE__ */ l.jsx("button", { type: "button", className: "dx-icono" + (n.has(u.clave) ? " activo" : ""), title: "Conceptos de la línea", onClick: () => N(u.clave), "data-f": h, "data-c": a ? 6 : 4, children: "±" }),
              !e.soloLectura && /* @__PURE__ */ l.jsx("button", { type: "button", className: "dx-icono", title: "Quitar la línea", onClick: () => s(u.clave), children: "✕" })
            ] })
          ] }, u.clave),
          n.has(u.clave) && /* @__PURE__ */ l.jsx("tr", { className: "dx-fila-conc", children: /* @__PURE__ */ l.jsx("td", { colSpan: a ? 9 : 7, children: /* @__PURE__ */ l.jsx(Do, { catalogo: e.catalogo, lista: u.conceptos, sugeridos: e.sugeridos[u.clave], alCambiar: (p) => o(u.clave, { conceptos: p }) }) }) }, u.clave + "c")
        ];
      }) })
    ] }),
    !e.soloLectura && /* @__PURE__ */ l.jsx("button", { type: "button", className: "btn small ghost", style: { marginTop: 8 }, onClick: () => (e.alCambiar([...e.lineas, wr()]), setTimeout(() => c(e.lineas.length, 0), 30)), children: "+ Añadir línea" }),
    !e.soloLectura && /* @__PURE__ */ l.jsx("span", { className: "muted dx-ayuda", children: "Intro: siguiente casilla · ↑↓: cambiar de línea · F2: buscar artículo · el precio en gris es el de tarifa" })
  ] });
}
const Lo = (e) => e.filter((t) => t.disponible > 0 && t.estado !== "Anulado" && !t.facturaId).sort((t, n) => t.fecha.localeCompare(n.fecha)), gd = (e) => e.filter((t) => !!t.facturaId && (t.disponibleBase ?? 0) > 0 && t.estado !== "Anulado").sort((t, n) => t.fecha.localeCompare(n.fecha));
function yd(e, t) {
  let n = Math.round(t * 100);
  const r = [];
  for (const a of Lo(e)) {
    if (n <= 0) break;
    const i = Math.min(n, Math.round(a.disponible * 100));
    i > 0 && r.push({ id: a.id, importe: i / 100 }), n -= i;
  }
  return r;
}
const fm = { presupuesto: "presupuesto", pedido: "pedido de venta", factura: "factura" };
function jd(e) {
  const t = /* @__PURE__ */ new Map();
  for (const n of e)
    for (const r of n.conceptos ?? [])
      if (r.repartido) {
        const a = t.get(r.conceptoId);
        t.set(r.conceptoId, { conceptoId: r.conceptoId, valor: r.calculo === "Importe" ? Ae(((a == null ? void 0 : a.valor) ?? 0) + r.valor) : r.valor });
      }
  return {
    porLinea: e.map((n) => (n.conceptos ?? []).filter((r) => !r.repartido).map((r) => ({ conceptoId: r.conceptoId, valor: r.valor }))),
    documento: [...t.values()]
  };
}
function pm(e) {
  var $o, Ao, Oo, Uo;
  const { api: t, anfitrion: n } = Ct(), r = !!(($o = e.semilla) != null && $o.rectificaId), [a, i] = g.useState([]), [o, s] = g.useState([]), [c, d] = g.useState([]), [N, u] = g.useState([]), [h, v] = g.useState([]), [y, S] = g.useState(((Ao = e.semilla) == null ? void 0 : Ao.clienteId) ?? ""), [$, p] = g.useState(e.tipo === "pedido" && ((Oo = e.semilla) != null && Oo.fecha) ? e.semilla.fecha : gt()), [f, x] = g.useState(""), [E, T] = g.useState(""), [F, D] = g.useState(0), [k, M] = g.useState(!1), [O, R] = g.useState(null), [Q, se] = g.useState(30), [Fe, Le] = g.useState(""), [fe, xe] = g.useState([wr()]), [I, j] = g.useState([]), [w, b] = g.useState(null), [W, K] = g.useState(""), [pe, me] = g.useState(!1), [te, U] = g.useState(!1), [ut, Ke] = g.useState(!1), [dt, Et] = g.useState([]), [At, kn] = g.useState(!0), [X, Se] = g.useState([]), [qe, nt] = g.useState(!0), Ot = Gr();
  g.useEffect(() => {
    t.get("/clientes").then(i).catch(() => i([])), t.get("/tipos-iva").then((C) => s(C.filter((V) => V.activo))).catch(() => s([])), t.get("/formas-pago").then((C) => d(C.filter((V) => V.activo))).catch(() => d([])), t.get("/series").then((C) => u([...new Set(C.filter((V) => V.tipoDocumento === "Factura").map((V) => V.prefijo))])).catch(() => u([])), t.get("/conceptos-linea?ambito=Ventas&activos=true").then(v).catch(() => v([]));
  }, [t]), g.useEffect(() => {
    const C = e.semilla;
    if (!C || !C.lineas.length) return;
    const { porLinea: V, documento: ee } = jd(C.lineas), re = C.lineas.map((Y, pl) => ({
      clave: Qr(),
      productoId: Y.productoId ?? null,
      descripcion: Y.descripcion,
      cantidad: Y.cantidad,
      precio: Y.precioUnitario,
      dto: Y.porcentajeDescuento,
      iva: Y.codigoIva,
      conceptos: r ? [] : V[pl]
    }));
    xe(re), j(r ? [] : ee), Promise.all(re.map((Y) => Y.productoId ? t.get(`/productos/${Y.productoId}`).catch(() => null) : Promise.resolve(null))).then(
      (Y) => xe((pl) => pl.map((bo, Cn) => Y[Cn] ? { ...bo, referencia: Y[Cn].referencia ?? Y[Cn].nombre, unidad: Y[Cn].unidad, stock: Y[Cn].stock, controlarStock: Y[Cn].controlarStock } : bo))
    );
  }, [e.semilla, t, r]);
  const ie = a.find((C) => C.id === y);
  g.useEffect(() => {
    if (e.tipo !== "factura" || r || !y) {
      Et([]), Se([]);
      return;
    }
    t.get(`/anticipos?clienteId=${y}`).then((C) => {
      Et(Lo(C)), Se(gd(C));
    }).catch(() => {
      Et([]), Se([]);
    });
  }, [t, y, e.tipo, r]);
  const Ve = Ae(dt.reduce((C, V) => C + V.disponible, 0));
  g.useEffect(() => {
    ie && (M(!!ie.recargoEquivalencia), ie.formaPagoDefectoId && T(ie.formaPagoDefectoId));
  }, [ie]);
  const It = g.useMemo(() => fe.map((C, V) => ({ l: C, i: V })).filter(({ l: C }) => (C.productoId || C.descripcion.trim()) && C.cantidad > 0), [fe]), Ut = g.useMemo(
    () => ({
      clienteId: y,
      fechaEmision: e.tipo === "factura" ? $ : null,
      serie: f || null,
      diasVencimiento: F,
      formaPagoId: E || null,
      recargoEquivalencia: k,
      porcentajeIrpf: O,
      conceptosDocumento: I,
      descontarAnticipos: qe && X.length ? X.map((C) => ({ anticipoId: C.id })) : null,
      lineas: It.map(({ l: C }) => ({
        cantidad: C.cantidad,
        descripcion: C.descripcion.trim() || null,
        precioUnitario: C.precio,
        codigoIva: C.iva,
        porcentajeDescuento: C.dto,
        productoId: C.productoId,
        ...r ? { conceptos: [] } : C.conceptos === void 0 ? {} : { conceptos: C.conceptos }
      }))
    }),
    [y, $, f, F, E, k, O, I, It, e.tipo, r, qe, X]
  ), m = vn(Ut, 350);
  g.useEffect(() => {
    if (!m.clienteId || m.lineas.length === 0) {
      b(null), K(m.clienteId ? "" : "Elige el cliente para calcular precios e importes.");
      return;
    }
    const C = Ot();
    me(!0), t.post("/facturas/simular", m).then((V) => C() && (b(V), K(""))).catch((V) => C() && (b(null), K(V.message))).finally(() => C() && me(!1));
  }, [m, t]);
  const L = g.useMemo(() => {
    const C = fe.map(() => {
    });
    return w && It.forEach(({ i: V }, ee) => {
      const re = w.lineas[ee];
      re && (C[V] = { precio: re.precioUnitario, dto: re.porcentajeDescuento, iva: re.codigoIva, importe: re.base, margen: re.productoId || re.costeUnitario || re.costeConceptos ? re.margen : void 0, conceptos: re.conceptos });
    }), C;
  }, [w, fe, It]), _ = g.useMemo(() => {
    const C = {};
    return fe.forEach((V, ee) => {
      var re;
      return C[V.clave] = (((re = L[ee]) == null ? void 0 : re.conceptos) ?? []).filter((Y) => !Y.repartido).map((Y) => ({ conceptoId: Y.conceptoId, valor: Y.valor }));
    }), C;
  }, [fe, L]);
  function B(C, V) {
    xe(
      (ee) => ee.map(
        (re) => re.clave === C ? { ...re, productoId: V.id, referencia: V.referencia ?? V.nombre, descripcion: V.nombre, precio: null, dto: 0, iva: null, conceptos: void 0, unidad: V.unidad, stock: V.stock, controlarStock: V.controlarStock } : re
      )
    );
  }
  const H = g.useMemo(() => {
    const C = /* @__PURE__ */ new Map();
    for (const V of (w == null ? void 0 : w.lineas) ?? []) {
      const ee = C.get(V.codigoIva) ?? { base: 0, cuota: 0, pct: V.porcentajeIva };
      ee.base += V.base, ee.cuota += V.cuotaIva, C.set(V.codigoIva, ee);
    }
    return [...C.entries()];
  }, [w]), Z = ((w == null ? void 0 : w.lineas) ?? []).reduce((C, V) => C + (V.base - V.margen), 0), fl = w ? w.baseImponible - Z : 0, Nd = (C) => {
    var V;
    return ((V = o.find((ee) => ee.codigo === C)) == null ? void 0 : V.nombre) ?? C;
  };
  async function Mo() {
    if (w) {
      U(!0);
      try {
        const C = It.map(({ l: ee }, re) => {
          const Y = w.lineas[re];
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
        let V;
        if (r)
          V = (await t.post(`/facturas/${e.semilla.rectificaId}/rectificar`, { motivo: Fe, lineas: C, fechaEmision: $, porcentajeIrpf: O, serie: f || null })).id;
        else if (e.tipo === "factura") {
          const ee = await t.post("/facturas", { ...Ut, lineas: C });
          if (V = ee.id, At && dt.length) {
            let re = 0;
            try {
              for (const Y of yd(dt, ee.total))
                await t.post(`/anticipos/${Y.id}/aplicar`, { facturaId: V, importe: Y.importe }), re += Y.importe;
              n.aviso(`Factura emitida. Aplicados ${z(re)} de anticipos (asiento 438 a 430).`, "ok");
            } catch (Y) {
              n.aviso(`Factura emitida, pero no se pudo aplicar el anticipo: ${Y.message}`, "err");
            }
            e.alGuardar(V);
            return;
          }
        } else if (e.tipo === "presupuesto") {
          const ee = { clienteId: y, diasValidez: Q, lineas: C, conceptosDocumento: I };
          V = e.id ? (await t.put(`/presupuestos/${e.id}`, ee)).id : (await t.post("/presupuestos", ee)).id;
        } else {
          const ee = { clienteId: y, fecha: $, lineas: C, conceptosDocumento: I };
          V = e.id ? (await t.put(`/pedidos-venta/${e.id}`, ee)).id : (await t.post("/pedidos-venta", ee)).id;
        }
        n.aviso(e.tipo === "factura" ? "Factura emitida." : "Documento guardado.", "ok"), e.alGuardar(V);
      } catch (C) {
        n.aviso(C.message, "err");
      } finally {
        U(!1), Ke(!1);
      }
    }
  }
  const Sd = r ? `Rectificativa de la factura ${((Uo = e.semilla) == null ? void 0 : Uo.rectificaNumero) ?? ""}` : `${e.id ? "Editar" : "Nuevo"} ${fm[e.tipo]}`.replace("Nuevo factura", "Nueva factura"), wd = !!w && !pe && (!r || Fe.trim().length > 0);
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsxs("div", { className: "panel-head", children: [
        /* @__PURE__ */ l.jsx("h2", { children: Sd }),
        /* @__PURE__ */ l.jsxs("div", { style: { display: "flex", gap: 8 }, children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: e.alCancelar, children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", disabled: !wd || te, onClick: () => e.tipo === "factura" ? Ke(!0) : Mo(), children: e.tipo === "factura" ? "Emitir factura" : "Guardar" })
        ] })
      ] }),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-cabecera", children: [
        /* @__PURE__ */ l.jsxs("div", { className: "dx-cab-campos", children: [
          /* @__PURE__ */ l.jsx(zo, { terceros: a, valor: y, alCambiar: S, etiqueta: "Cliente", deshabilitado: r || e.tipo === "pedido" && !!e.id && !1 }),
          /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", children: [
            e.tipo !== "presupuesto" && /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: e.tipo === "factura" ? "Fecha de emisión" : "Fecha del pedido" }),
              /* @__PURE__ */ l.jsx("input", { type: "date", value: $, onChange: (C) => p(C.target.value) })
            ] }),
            e.tipo === "presupuesto" && /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Validez" }),
              /* @__PURE__ */ l.jsx("select", { value: Q, onChange: (C) => se(Number(C.target.value)), children: [15, 30, 60, 90].map((C) => /* @__PURE__ */ l.jsxs("option", { value: C, children: [
                C,
                " días"
              ] }, C)) })
            ] }),
            e.tipo === "factura" && N.length > 0 && /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Serie" }),
              /* @__PURE__ */ l.jsxs("select", { value: f, onChange: (C) => x(C.target.value), children: [
                /* @__PURE__ */ l.jsx("option", { value: "", children: "La del cliente" }),
                N.map((C) => /* @__PURE__ */ l.jsx("option", { value: C, children: C }, C))
              ] })
            ] }),
            e.tipo === "factura" && !r && /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
              /* @__PURE__ */ l.jsxs("div", { children: [
                /* @__PURE__ */ l.jsx("label", { children: "Forma de pago" }),
                /* @__PURE__ */ l.jsxs("select", { value: E, onChange: (C) => T(C.target.value), children: [
                  /* @__PURE__ */ l.jsx("option", { value: "", children: "— Vencimiento a mano —" }),
                  c.map((C) => /* @__PURE__ */ l.jsx("option", { value: C.id, children: C.nombre }, C.id))
                ] })
              ] }),
              !E && /* @__PURE__ */ l.jsxs("div", { children: [
                /* @__PURE__ */ l.jsx("label", { children: "Vencimiento" }),
                /* @__PURE__ */ l.jsx("select", { value: F, onChange: (C) => D(Number(C.target.value)), children: [0, 15, 30, 45, 60, 90].map((C) => /* @__PURE__ */ l.jsx("option", { value: C, children: C ? `${C} días` : "Contado" }, C)) })
              ] })
            ] }),
            e.tipo === "factura" && /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Retención IRPF %" }),
              /* @__PURE__ */ l.jsx("input", { type: "number", step: "0.01", value: O ?? "", placeholder: String((ie == null ? void 0 : ie.porcentajeIrpfDefecto) ?? 0), onChange: (C) => R(C.target.value === "" ? null : Number(C.target.value)) })
            ] })
          ] }),
          e.tipo === "factura" && !r && /* @__PURE__ */ l.jsxs("label", { className: "dx-check", children: [
            /* @__PURE__ */ l.jsx("input", { type: "checkbox", checked: k, onChange: (C) => M(C.target.checked) }),
            " Recargo de equivalencia"
          ] }),
          r && /* @__PURE__ */ l.jsxs("div", { children: [
            /* @__PURE__ */ l.jsx("label", { children: "Motivo de la rectificación (obligatorio)" }),
            /* @__PURE__ */ l.jsx("input", { value: Fe, onChange: (C) => Le(C.target.value), placeholder: "Devolución, error en precio…" })
          ] })
        ] }),
        /* @__PURE__ */ l.jsx("div", { className: "dx-ficha", children: ie ? /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("strong", { children: ie.nombre }),
          /* @__PURE__ */ l.jsx("div", { className: "muted", children: [ie.nifFiscal, ie.poblacion, ie.provincia].filter(Boolean).join(" · ") }),
          ie.limiteRiesgo != null && /* @__PURE__ */ l.jsxs("div", { className: "muted", children: [
            "Límite de riesgo ",
            z(ie.limiteRiesgo)
          ] }),
          ie.tarifaId && /* @__PURE__ */ l.jsx("div", { className: "muted", children: "Con tarifa de precios propia" }),
          ie.recargoEquivalencia && /* @__PURE__ */ l.jsx("div", { className: "muted", children: "En recargo de equivalencia" }),
          (w == null ? void 0 : w.avisoRiesgo) && /* @__PURE__ */ l.jsxs("div", { className: "dx-aviso", children: [
            "⚠ ",
            w.avisoRiesgo
          ] }),
          X.length > 0 && /* @__PURE__ */ l.jsxs("div", { className: "dx-anticipo", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              "🧾 Anticipos facturados pendientes de descontar: ",
              /* @__PURE__ */ l.jsx("strong", { children: z(X.reduce((C, V) => C + (V.disponibleBase ?? 0), 0)) }),
              " de base (",
              X.map((C) => C.facturaNumero).join(", "),
              ")."
            ] }),
            /* @__PURE__ */ l.jsxs("label", { className: "dx-check", children: [
              /* @__PURE__ */ l.jsx("input", { type: "checkbox", checked: qe, onChange: (C) => nt(C.target.checked) }),
              "Descontar en esta factura (línea negativa con su base e IVA; hasta la base de la factura)"
            ] })
          ] }),
          Ve > 0 && /* @__PURE__ */ l.jsxs("div", { className: "dx-anticipo", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              "💶 Tiene ",
              /* @__PURE__ */ l.jsx("strong", { children: z(Ve) }),
              " en ",
              dt.length === 1 ? "un anticipo pendiente" : `${dt.length} anticipos pendientes`,
              " de aplicar."
            ] }),
            /* @__PURE__ */ l.jsxs("label", { className: "dx-check", children: [
              /* @__PURE__ */ l.jsx("input", { type: "checkbox", checked: At, onChange: (C) => kn(C.target.checked) }),
              "Aplicarlo al emitir",
              w ? ` (${z(Math.min(Ve, w.total))})` : ""
            ] })
          ] })
        ] }) : /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Elige un cliente: se aplican su tarifa, su forma de pago, su recargo y sus conceptos." }) })
      ] })
    ] }),
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsx(xd, { modo: "venta", lineas: fe, alCambiar: xe, calculos: L, ivas: o, catalogo: r ? [] : h, sugeridos: _, alElegirArticulo: B }),
      !r && h.length > 0 && /* @__PURE__ */ l.jsx("div", { style: { marginTop: 10 }, children: /* @__PURE__ */ l.jsx(Do, { catalogo: h, lista: I, alCambiar: (C) => j(C ?? []), documento: !0 }) })
    ] }),
    /* @__PURE__ */ l.jsxs("div", { className: "dx-pie", children: [
      /* @__PURE__ */ l.jsxs("div", { className: "dx-estado", children: [
        pe && /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Calculando…" }),
        !pe && W && /* @__PURE__ */ l.jsx("span", { className: "dx-rojo", children: W }),
        (w == null ? void 0 : w.mencionFiscal) && /* @__PURE__ */ l.jsx("div", { className: "muted", style: { fontSize: 12 }, children: w.mencionFiscal })
      ] }),
      /* @__PURE__ */ l.jsxs("div", { className: "panel dx-totales", children: [
        H.map(([C, V]) => /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
            Nd(C),
            " · base ",
            ke(V.base)
          ] }),
          /* @__PURE__ */ l.jsx("span", { children: z(V.cuota) })
        ] }, C)),
        w == null ? void 0 : w.lineas.filter((C) => C.anticipoId).map((C) => /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-tot-anticipo", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: C.descripcion }),
          /* @__PURE__ */ l.jsx("span", { children: z(C.base + C.cuotaIva + C.cuotaRecargo) })
        ] }, C.anticipoId)),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Base imponible" }),
          /* @__PURE__ */ l.jsx("span", { children: z(w == null ? void 0 : w.baseImponible) })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Impuestos" }),
          /* @__PURE__ */ l.jsx("span", { children: z(w == null ? void 0 : w.cuotaIva) })
        ] }),
        !!(w != null && w.recargoTotal) && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Recargo de equivalencia" }),
          /* @__PURE__ */ l.jsx("span", { children: z(w.recargoTotal) })
        ] }),
        !!(w != null && w.retencionIrpf) && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
            "Retención IRPF (",
            ke(w.porcentajeIrpf),
            " %)"
          ] }),
          /* @__PURE__ */ l.jsxs("span", { children: [
            "−",
            z(w.retencionIrpf)
          ] })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ l.jsx("span", { children: "Total" }),
          /* @__PURE__ */ l.jsx("span", { children: z(w == null ? void 0 : w.total) })
        ] }),
        w && Z > 0 && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Coste · margen" }),
          /* @__PURE__ */ l.jsxs("span", { className: fl < 0 ? "dx-rojo" : "muted", children: [
            z(Z),
            " · ",
            z(fl),
            " (",
            ke(w.baseImponible ? fl / w.baseImponible * 100 : 0),
            " %)"
          ] })
        ] })
      ] })
    ] }),
    ut && w && /* @__PURE__ */ l.jsx(
      an,
      {
        titulo: r ? "Emitir la rectificativa" : "Emitir la factura",
        alCerrar: () => Ke(!1),
        acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => Ke(!1), children: "Revisar" }),
          /* @__PURE__ */ l.jsxs("button", { className: "btn small", disabled: te, onClick: Mo, children: [
            "Emitir ",
            z(w.total)
          ] })
        ] }),
        children: /* @__PURE__ */ l.jsxs("p", { style: { margin: 0 }, children: [
          "Se emitirá una factura ",
          r ? "rectificativa " : "",
          "de ",
          /* @__PURE__ */ l.jsx("strong", { children: z(w.total) }),
          " a ",
          /* @__PURE__ */ l.jsx("strong", { children: ie == null ? void 0 : ie.nombre }),
          " con fecha ",
          $.split("-").reverse().join("/"),
          ". Una factura emitida no se modifica: queda numerada y encadenada en VeriFactu; para corregirla hay que rectificarla o anularla."
        ] })
      }
    )
  ] });
}
function mm(e) {
  var Le, fe, xe;
  const { api: t, anfitrion: n } = Ct(), [r, a] = g.useState([]), [i, o] = g.useState([]), [s, c] = g.useState(((Le = e.semilla) == null ? void 0 : Le.proveedorId) ?? ""), [d, N] = g.useState(((fe = e.semilla) == null ? void 0 : fe.fecha) ?? gt()), [u, h] = g.useState([wr()]), [v, y] = g.useState([]), [S, $] = g.useState(null), [p, f] = g.useState(""), [x, E] = g.useState(!1), T = Gr();
  g.useEffect(() => {
    t.get("/proveedores").then(a).catch(() => a([])), t.get("/conceptos-linea?ambito=Compras&activos=true").then(o).catch(() => o([]));
  }, [t]), g.useEffect(() => {
    const I = e.semilla;
    if (!I) return;
    const j = I.lineas.map((K) => ({ ...K, porcentajeDescuento: 0, codigoIva: "" })), { porLinea: w, documento: b } = jd(j), W = I.lineas.map((K, pe) => ({ clave: Qr(), productoId: K.productoId ?? null, descripcion: K.descripcion, cantidad: K.cantidad, precio: K.precioUnitario, dto: 0, iva: null, conceptos: w[pe] }));
    h(W), y(b), Promise.all(W.map((K) => K.productoId ? t.get(`/productos/${K.productoId}`).catch(() => null) : Promise.resolve(null))).then(
      (K) => h((pe) => pe.map((me, te) => K[te] ? { ...me, referencia: K[te].referencia ?? K[te].nombre, unidad: K[te].unidadCompra || K[te].unidad, stock: K[te].stock, controlarStock: K[te].controlarStock } : me))
    );
  }, [e.semilla, t]);
  const F = r.find((I) => I.id === s), D = g.useMemo(() => u.map((I, j) => ({ l: I, i: j })).filter(({ l: I }) => I.descripcion.trim() && I.cantidad > 0), [u]), k = g.useMemo(
    () => {
      var I, j;
      return {
        proveedorId: s || null,
        proveedorTexto: (F == null ? void 0 : F.nombre) ?? (((I = e.semilla) == null ? void 0 : I.proveedorTexto) || null),
        solicitudOrigenId: e.id ? null : ((j = e.semilla) == null ? void 0 : j.solicitudOrigenId) ?? null,
        fecha: d,
        conceptosDocumento: v,
        lineas: D.map(({ l: w }) => ({ descripcion: w.descripcion.trim(), cantidad: w.cantidad, precioUnitario: w.precio ?? 0, productoId: w.productoId, ...w.conceptos === void 0 ? {} : { conceptos: w.conceptos } }))
      };
    },
    [s, F, d, v, D, e.id, e.semilla]
  ), M = vn(k, 350);
  g.useEffect(() => {
    if (!M.proveedorId || M.lineas.length === 0) {
      $(null), f(M.proveedorId ? "" : "Elige el proveedor.");
      return;
    }
    const I = T();
    t.post("/compras/pedidos/simular", M).then((j) => I() && ($(j), f(""))).catch((j) => I() && ($(null), f(j.message)));
  }, [M, t]);
  const O = g.useMemo(() => {
    const I = u.map(() => {
    });
    return D.forEach(({ i: j }, w) => {
      const b = S == null ? void 0 : S.lineas[w];
      b && (I[j] = { precio: b.precioUnitario, importe: b.importe, costeUnitarioEntrada: b.costeUnitarioEntrada, conceptos: b.conceptos });
    }), I;
  }, [S, u, D]), R = g.useMemo(() => {
    const I = {};
    return u.forEach((j, w) => {
      var b;
      return I[j.clave] = (((b = O[w]) == null ? void 0 : b.conceptos) ?? []).filter((W) => !W.repartido).map((W) => ({ conceptoId: W.conceptoId, valor: W.valor }));
    }), I;
  }, [u, O]);
  function Q(I, j) {
    const w = j.precioCompraPorUnidadCompra ?? j.precioCompra;
    h((b) => b.map((W) => W.clave === I ? { ...W, productoId: j.id, referencia: j.referencia ?? j.nombre, descripcion: j.nombre, precio: w, conceptos: void 0, unidad: j.unidadCompra || j.unidad, stock: j.stock, controlarStock: j.controlarStock } : W));
  }
  async function se() {
    E(!0);
    try {
      const I = e.id ? await t.put(`/compras/pedidos/${e.id}`, k) : await t.post("/compras/pedidos", k);
      n.aviso("Pedido de compra guardado.", "ok"), e.alGuardar(I.id);
    } catch (I) {
      n.aviso(I.message, "err");
    } finally {
      E(!1);
    }
  }
  const Fe = ((S == null ? void 0 : S.lineas) ?? []).reduce((I, j) => I + j.costeConceptos, 0);
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsxs("div", { className: "panel-head", children: [
        /* @__PURE__ */ l.jsx("h2", { children: e.id ? `Editar pedido ${((xe = e.semilla) == null ? void 0 : xe.numeroCompleto) ?? ""}` : "Nuevo pedido de compra" }),
        /* @__PURE__ */ l.jsxs("div", { style: { display: "flex", gap: 8 }, children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: e.alCancelar, children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", disabled: !S || x, onClick: se, children: "Guardar" })
        ] })
      ] }),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-cabecera", children: [
        /* @__PURE__ */ l.jsxs("div", { className: "dx-cab-campos", children: [
          /* @__PURE__ */ l.jsx(zo, { terceros: r, valor: s, alCambiar: c, etiqueta: "Proveedor", deshabilitado: !!e.id }),
          /* @__PURE__ */ l.jsx("div", { className: "dx-fila", children: /* @__PURE__ */ l.jsxs("div", { children: [
            /* @__PURE__ */ l.jsx("label", { children: "Fecha del pedido" }),
            /* @__PURE__ */ l.jsx("input", { type: "date", value: d, onChange: (I) => N(I.target.value) })
          ] }) })
        ] }),
        /* @__PURE__ */ l.jsx("div", { className: "dx-ficha", children: F ? /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("strong", { children: F.nombre }),
          /* @__PURE__ */ l.jsx("div", { className: "muted", children: [F.nifFiscal, F.poblacion, F.pais].filter(Boolean).join(" · ") })
        ] }) : /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Elige un proveedor: se aplican sus conceptos (pronto pago, portes…)." }) })
      ] })
    ] }),
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsx(xd, { modo: "compra", lineas: u, alCambiar: h, calculos: O, ivas: [], catalogo: i, sugeridos: R, alElegirArticulo: Q }),
      i.length > 0 && /* @__PURE__ */ l.jsx("div", { style: { marginTop: 10 }, children: /* @__PURE__ */ l.jsx(Do, { catalogo: i, lista: v, alCambiar: (I) => y(I ?? []), documento: !0 }) })
    ] }),
    /* @__PURE__ */ l.jsxs("div", { className: "dx-pie", children: [
      /* @__PURE__ */ l.jsx("div", { className: "dx-estado", children: p && /* @__PURE__ */ l.jsx("span", { className: "dx-rojo", children: p }) }),
      /* @__PURE__ */ l.jsxs("div", { className: "panel dx-totales", children: [
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ l.jsx("span", { children: "Total del pedido (sin impuestos)" }),
          /* @__PURE__ */ l.jsx("span", { children: z(S == null ? void 0 : S.total) })
        ] }),
        Fe !== 0 && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Costes añadidos al almacén" }),
          /* @__PURE__ */ l.jsx("span", { children: z(Fe) })
        ] }),
        /* @__PURE__ */ l.jsx("div", { className: "muted", style: { fontSize: 12, marginTop: 6 }, children: "El impuesto se aplica al facturar el pedido." })
      ] })
    ] })
  ] });
}
function ul(e) {
  return /* @__PURE__ */ l.jsxs("div", { className: "panel-head", children: [
    /* @__PURE__ */ l.jsxs("h2", { style: { display: "flex", alignItems: "center", gap: 10 }, children: [
      /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: e.volver, title: "Volver a la lista", children: "←" }),
      e.titulo,
      e.estado && /* @__PURE__ */ l.jsx("span", { className: To(e.estado), children: e.estado }),
      e.extra
    ] }),
    /* @__PURE__ */ l.jsx("div", { className: "dx-acciones", children: e.acciones })
  ] });
}
function ze(e) {
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-dato", children: [
    /* @__PURE__ */ l.jsx("small", { children: e.etiqueta }),
    /* @__PURE__ */ l.jsx("div", { children: e.children })
  ] });
}
function Wn(e) {
  return /* @__PURE__ */ l.jsx("button", { type: "button", className: "dx-enlace", onClick: e.alPulsar, children: e.children });
}
function dl(e) {
  const [t, n] = g.useState(null), [r, a] = g.useState(""), i = g.useCallback(() => {
    e().then(n).catch((o) => a(o.message));
  }, []);
  return g.useEffect(i, [i]), { dato: t, error: r, recargar: i };
}
async function lt(e, t, n) {
  try {
    return await e(), n && t(n, "ok"), !0;
  } catch (r) {
    return t(r.message, "err"), !1;
  }
}
function hm(e) {
  const { api: t, anfitrion: n, navegar: r } = Ct(), { dato: a, error: i, recargar: o } = dl(() => t.get(`/facturas/${e.id}`)), [s, c] = g.useState(null), [d, N] = g.useState(!1), [u, h] = g.useState("");
  g.useEffect(() => void t.get(`/facturas/${e.id}/saldo`).then(c).catch(() => c(null)), [t, e.id, a]);
  const [v, y] = g.useState([]), [S, $] = g.useState([]), [p, f] = g.useState(null);
  g.useEffect(() => {
    !(a != null && a.clienteId) || a.estado !== "Emitida" || t.get(`/anticipos?clienteId=${a.clienteId}`).then((k) => {
      y(Lo(k)), $(gd(k));
    }).catch(() => y([]));
  }, [t, a]);
  const x = v.reduce((k, M) => k + M.disponible, 0);
  if (i) return /* @__PURE__ */ l.jsx("div", { className: "panel", children: /* @__PURE__ */ l.jsx("p", { className: "dx-rojo", children: i }) });
  if (!a) return /* @__PURE__ */ l.jsx("div", { className: "muted", children: "Cargando…" });
  const E = { clienteId: a.clienteId ?? void 0, lineas: a.lineas }, T = a.lineas.reduce((k, M) => k + (M.base - M.margen), 0), F = a.estado === "Emitida", D = a.lineas.some((k) => k.cuentaContable === "438" && !k.anticipoId);
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsx(
        ul,
        {
          titulo: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            a.tipo === "Rectificativa" ? "Rectificativa" : a.tipo === "Simplificada" ? "Ticket" : "Factura",
            " ",
            /* @__PURE__ */ l.jsx("span", { className: "mono", children: a.numeroCompleto })
          ] }),
          estado: a.estado,
          extra: D ? /* @__PURE__ */ l.jsx("span", { className: "pill part", children: "Factura de anticipo" }) : a.lineas.some((k) => k.anticipoId) ? /* @__PURE__ */ l.jsx("span", { className: "pill", children: "Descuenta anticipos" }) : null,
          volver: () => r({ tipo: "factura", pantalla: "lista" }),
          acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => Li(n, `/facturas/${a.id}/pdf`).catch((k) => n.aviso(k.message, "err")), children: "PDF" }),
            a.tipo !== "Simplificada" && a.clienteNif && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => Li(n, `/facturas/${a.id}/facturae.xml`).catch((k) => n.aviso(k.message, "err")), children: "Facturae" }),
            /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "factura", pantalla: "editor", semilla: E }), children: "Duplicar" }),
            F && a.tipo === "Ordinaria" && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "factura", pantalla: "editor", semilla: { ...E, rectificaId: a.id, rectificaNumero: a.numeroCompleto } }), children: "Rectificar" }),
            F && /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", onClick: () => N(!0), children: "Anular" })
          ] })
        }
      ),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ l.jsxs(ze, { etiqueta: "Cliente", children: [
          /* @__PURE__ */ l.jsx("strong", { children: a.clienteNombre }),
          a.clienteNif && /* @__PURE__ */ l.jsx("div", { className: "muted mono", children: a.clienteNif }),
          /* @__PURE__ */ l.jsx("div", { className: "muted", children: [a.clienteCalle, a.clienteCodigoPostal, a.clientePoblacion].filter(Boolean).join(" ") })
        ] }),
        /* @__PURE__ */ l.jsxs(ze, { etiqueta: "Emisión", children: [
          je(a.fechaEmision),
          a.fechaOperacion !== a.fechaEmision && /* @__PURE__ */ l.jsxs("div", { className: "muted", children: [
            "Operación ",
            je(a.fechaOperacion)
          ] })
        ] }),
        /* @__PURE__ */ l.jsx(ze, { etiqueta: "Vencimiento", children: je(a.fechaVencimiento) }),
        /* @__PURE__ */ l.jsxs(ze, { etiqueta: "Cobro", children: [
          s ? s.pendiente <= 0 ? /* @__PURE__ */ l.jsx("span", { className: "pill ok", children: "Cobrada" }) : /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            "Pendiente ",
            /* @__PURE__ */ l.jsx("strong", { children: z(s.pendiente) }),
            s.liquidado > 0 && /* @__PURE__ */ l.jsxs("div", { className: "muted", children: [
              "Cobrado ",
              z(s.liquidado)
            ] })
          ] }) : "—",
          s && s.pendiente > 0 && F && n.irA && /* @__PURE__ */ l.jsx("div", { children: /* @__PURE__ */ l.jsx(Wn, { alPulsar: () => n.irA("cobros"), children: "Registrar cobro" }) }),
          s && s.pendiente > 0 && F && S.length > 0 && !D && /* @__PURE__ */ l.jsxs("div", { className: "dx-anticipo", children: [
            "🧾 Anticipos facturados sin descontar: ",
            /* @__PURE__ */ l.jsx("strong", { children: z(S.reduce((k, M) => k + (M.disponibleBase ?? 0), 0)) }),
            " de base. Se descuentan al hacer la siguiente factura (o rectifica esta para incluirlos)."
          ] }),
          s && s.pendiente > 0 && F && x > 0 && /* @__PURE__ */ l.jsxs("div", { className: "dx-anticipo", children: [
            "💶 Anticipos del cliente sin aplicar: ",
            /* @__PURE__ */ l.jsx("strong", { children: z(x) }),
            /* @__PURE__ */ l.jsx("div", { children: /* @__PURE__ */ l.jsx(Wn, { alPulsar: () => f(yd(v, s.pendiente)), children: "Aplicar a esta factura" }) })
          ] })
        ] })
      ] }),
      a.motivoRectificacion && /* @__PURE__ */ l.jsxs("p", { className: "muted", children: [
        "Rectifica: ",
        a.motivoRectificacion,
        a.rectificaFacturaId && /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          " · ",
          /* @__PURE__ */ l.jsx(Wn, { alPulsar: () => r({ tipo: "factura", pantalla: "vista", id: a.rectificaFacturaId }), children: "ver la factura original" })
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
        /* @__PURE__ */ l.jsx("tbody", { children: a.lineas.map((k, M) => /* @__PURE__ */ l.jsxs("tr", { children: [
          /* @__PURE__ */ l.jsxs("td", { children: [
            k.descripcion,
            /* @__PURE__ */ l.jsx(cl, { conceptos: k.conceptos })
          ] }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: $e(k.cantidad) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: z(k.precioUnitario) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: k.porcentajeDescuento ? `${ke(k.porcentajeDescuento)} %` : "" }),
          /* @__PURE__ */ l.jsxs("td", { children: [
            k.codigoIva,
            " · ",
            ke(k.porcentajeIva),
            " %"
          ] }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: z(k.base) }) }),
          /* @__PURE__ */ l.jsx("td", { className: "num muted", children: k.costeUnitario || k.costeConceptos ? z(k.margen) : "" })
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
            ke(a.porcentajeIrpf),
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
        T > 0 && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Margen" }),
          /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
            z(a.baseImponible - T),
            " (",
            ke(a.baseImponible ? (a.baseImponible - T) / a.baseImponible * 100 : 0),
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
      an,
      {
        titulo: `Aplicar anticipos a ${a.numeroCompleto}`,
        alCerrar: () => f(null),
        acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => f(null), children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", disabled: !p.some((k) => k.importe > 0), onClick: async () => {
            for (const k of p.filter((M) => M.importe > 0))
              if (!await lt(() => t.post(`/anticipos/${k.id}/aplicar`, { facturaId: a.id, importe: k.importe }), n.aviso, "Anticipo aplicado.")) return;
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
            /* @__PURE__ */ l.jsx("tbody", { children: v.map((k) => {
              const M = p.find((O) => O.id === k.id);
              return /* @__PURE__ */ l.jsxs("tr", { children: [
                /* @__PURE__ */ l.jsx("td", { children: je(k.fecha) }),
                /* @__PURE__ */ l.jsx("td", { className: "muted", children: k.concepto }),
                /* @__PURE__ */ l.jsx("td", { className: "num", children: z(k.disponible) }),
                /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx(
                  "input",
                  {
                    type: "number",
                    step: "0.01",
                    min: 0,
                    max: k.disponible,
                    style: { width: 110, textAlign: "right" },
                    value: (M == null ? void 0 : M.importe) ?? 0,
                    onChange: (O) => {
                      const R = Math.max(0, Math.min(k.disponible, Number(O.target.value) || 0));
                      f([...p.filter((Q) => Q.id !== k.id), { id: k.id, importe: R }]);
                    }
                  }
                ) })
              ] }, k.id);
            }) })
          ] })
        ]
      }
    ),
    d && /* @__PURE__ */ l.jsxs(
      an,
      {
        titulo: `Anular ${a.numeroCompleto}`,
        alCerrar: () => N(!1),
        acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => N(!1), children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", disabled: !u.trim(), onClick: async () => await lt(() => t.post(`/facturas/${a.id}/anular`, { motivo: u }), n.aviso, "Factura anulada.") && (N(!1), o()), children: "Anular" })
        ] }),
        children: [
          /* @__PURE__ */ l.jsx("p", { className: "muted", style: { marginTop: 0 }, children: "La factura no se borra: queda anulada con un registro de anulación VeriFactu. Si hay que corregir importes, rectifícala en lugar de anularla." }),
          /* @__PURE__ */ l.jsx("label", { children: "Motivo" }),
          /* @__PURE__ */ l.jsx("input", { value: u, onChange: (k) => h(k.target.value), autoFocus: !0 })
        ]
      }
    )
  ] });
}
function vm(e) {
  const { api: t, anfitrion: n, navegar: r } = Ct(), { dato: a, error: i, recargar: o } = dl(() => t.get(`/presupuestos/${e.id}`));
  if (i) return /* @__PURE__ */ l.jsx("div", { className: "panel", children: /* @__PURE__ */ l.jsx("p", { className: "dx-rojo", children: i }) });
  if (!a) return /* @__PURE__ */ l.jsx("div", { className: "muted", children: "Cargando…" });
  const s = a.estado === "Borrador", c = { clienteId: a.clienteId, lineas: a.lineas };
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsx(
        ul,
        {
          titulo: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            "Presupuesto ",
            /* @__PURE__ */ l.jsx("span", { className: "mono", children: a.numeroCompleto })
          ] }),
          estado: a.estado,
          volver: () => r({ tipo: "presupuesto", pantalla: "lista" }),
          acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => Li(n, `/presupuestos/${a.id}/pdf`).catch((d) => n.aviso(d.message, "err")), children: "PDF" }),
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
            s && /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", onClick: async () => await lt(() => t.post(`/presupuestos/${a.id}/rechazar`, {}), n.aviso, "Presupuesto rechazado.") && o(), children: "Rechazar" })
          ] })
        }
      ),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ l.jsx(ze, { etiqueta: "Cliente", children: /* @__PURE__ */ l.jsx("strong", { children: a.clienteNombre }) }),
        /* @__PURE__ */ l.jsx(ze, { etiqueta: "Fecha", children: je(a.fecha) }),
        /* @__PURE__ */ l.jsx(ze, { etiqueta: "Válido hasta", children: je(a.validez) }),
        /* @__PURE__ */ l.jsx(ze, { etiqueta: "Factura", children: a.facturaId ? /* @__PURE__ */ l.jsx(Wn, { alPulsar: () => r({ tipo: "factura", pantalla: "vista", id: a.facturaId }), children: "ver la factura" }) : "—" })
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
        /* @__PURE__ */ l.jsx("tbody", { children: a.lineas.map((d, N) => /* @__PURE__ */ l.jsxs("tr", { children: [
          /* @__PURE__ */ l.jsxs("td", { children: [
            d.descripcion,
            /* @__PURE__ */ l.jsx(cl, { conceptos: d.conceptos })
          ] }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: $e(d.cantidad) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: z(d.precioUnitario) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: d.porcentajeDescuento ? `${ke(d.porcentajeDescuento)} %` : "" }),
          /* @__PURE__ */ l.jsx("td", { children: d.codigoIva }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: z(d.base) }) })
        ] }, N)) })
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
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ l.jsx("span", { children: "Total" }),
          /* @__PURE__ */ l.jsx("span", { children: z(a.total) })
        ] })
      ] })
    ] })
  ] });
}
function xm(e) {
  const { api: t, anfitrion: n, navegar: r } = Ct(), { dato: a, error: i, recargar: o } = dl(() => t.get(`/pedidos-venta/${e.id}`)), [s, c] = g.useState([]), [d, N] = g.useState([]), [u, h] = g.useState(null), [v, y] = g.useState(""), [S, $] = g.useState(gt()), [p, f] = g.useState(!1), [x, E] = g.useState(gt()), [T, F] = g.useState("");
  if (g.useEffect(() => void t.get(`/pedidos-venta/${e.id}/albaranes`).then(c).catch(() => c([])), [t, e.id, a]), g.useEffect(() => void t.get("/formas-pago").then((R) => N(R.filter((Q) => Q.activo))).catch(() => N([])), [t]), i) return /* @__PURE__ */ l.jsx("div", { className: "panel", children: /* @__PURE__ */ l.jsx("p", { className: "dx-rojo", children: i }) });
  if (!a) return /* @__PURE__ */ l.jsx("div", { className: "muted", children: "Cargando…" });
  const D = (a.estado === "Borrador" || a.estado === "Confirmado") && a.lineas.every((R) => R.cantidadServida === 0), k = a.lineas.some((R) => R.pendienteServir > 0), M = a.estado !== "Cancelado" && a.estado !== "Facturado", O = { clienteId: a.clienteId, fecha: a.fecha, lineas: a.lineas };
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsx(
        ul,
        {
          titulo: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            "Pedido de venta ",
            /* @__PURE__ */ l.jsx("span", { className: "mono", children: a.numeroCompleto })
          ] }),
          estado: a.estado,
          volver: () => r({ tipo: "pedido", pantalla: "lista" }),
          acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "pedido", pantalla: "editor", semilla: { ...O, fecha: void 0 } }), children: "Duplicar" }),
            D && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "pedido", pantalla: "editor", id: a.id, semilla: O }), children: "Editar" }),
            a.estado === "Borrador" && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: async () => await lt(() => t.post(`/pedidos-venta/${a.id}/confirmar`), n.aviso, "Pedido confirmado.") && o(), children: "Confirmar" }),
            M && a.estado !== "Borrador" && k && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => h(Object.fromEntries(a.lineas.map((R) => [R.id, R.pendienteServir]))), children: "Entregar (albarán)" }),
            M && a.estado !== "Borrador" && /* @__PURE__ */ l.jsx("button", { className: "btn small", onClick: () => f(!0), children: "Facturar" }),
            M && /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", onClick: async () => window.confirm("¿Cancelar el pedido?") && await lt(() => t.post(`/pedidos-venta/${a.id}/cancelar`), n.aviso, "Pedido cancelado.") && o(), children: "Cancelar" })
          ] })
        }
      ),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ l.jsx(ze, { etiqueta: "Cliente", children: /* @__PURE__ */ l.jsx("strong", { children: a.clienteNombre }) }),
        /* @__PURE__ */ l.jsx(ze, { etiqueta: "Fecha", children: je(a.fecha) }),
        /* @__PURE__ */ l.jsx(ze, { etiqueta: "Viene de", children: a.presupuestoOrigenId ? /* @__PURE__ */ l.jsx(Wn, { alPulsar: () => r({ tipo: "presupuesto", pantalla: "vista", id: a.presupuestoOrigenId }), children: "presupuesto" }) : "—" }),
        /* @__PURE__ */ l.jsx(ze, { etiqueta: "Factura", children: a.facturaId ? /* @__PURE__ */ l.jsx(Wn, { alPulsar: () => r({ tipo: "factura", pantalla: "vista", id: a.facturaId }), children: "ver la factura" }) : "—" })
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
        /* @__PURE__ */ l.jsx("tbody", { children: a.lineas.map((R) => /* @__PURE__ */ l.jsxs("tr", { children: [
          /* @__PURE__ */ l.jsxs("td", { children: [
            R.descripcion,
            /* @__PURE__ */ l.jsx(cl, { conceptos: R.conceptos })
          ] }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: $e(R.cantidad) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: $e(R.cantidadServida) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: R.pendienteServir > 0 ? /* @__PURE__ */ l.jsx("strong", { children: $e(R.pendienteServir) }) : "—" }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: z(R.precioUnitario) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: R.porcentajeDescuento ? `${ke(R.porcentajeDescuento)} %` : "" }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: z(R.base) }) })
        ] }, R.id)) })
      ] }),
      /* @__PURE__ */ l.jsx("div", { className: "dx-totales-vista", children: /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-grande", children: [
        /* @__PURE__ */ l.jsx("span", { children: "Total (sin impuestos)" }),
        /* @__PURE__ */ l.jsx("span", { children: z(a.total) })
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
        /* @__PURE__ */ l.jsx("tbody", { children: s.map((R) => /* @__PURE__ */ l.jsxs("tr", { children: [
          /* @__PURE__ */ l.jsxs("td", { className: "mono", children: [
            /* @__PURE__ */ l.jsx("strong", { children: R.numeroCompleto }),
            " ",
            R.anulado && /* @__PURE__ */ l.jsx("span", { className: "pill neg", title: R.motivoAnulacion ?? "", children: "Anulado" })
          ] }),
          /* @__PURE__ */ l.jsx("td", { children: je(R.fecha) }),
          /* @__PURE__ */ l.jsx("td", { className: "muted", children: R.referencia }),
          /* @__PURE__ */ l.jsx("td", { className: "muted", children: R.lineas.map((Q) => `${$e(Q.cantidad)} × ${Q.descripcion}`).join(" · ") }),
          /* @__PURE__ */ l.jsx("td", { className: "right", children: !R.anulado && a.estado !== "Facturado" && /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", onClick: async () => {
            const Q = window.prompt("Motivo de la anulación del albarán:");
            Q !== null && await lt(() => t.post(`/pedidos-venta/${a.id}/albaranes/${R.id}/anular`, { motivo: Q || null }), n.aviso, "Albarán anulado.") && o();
          }, children: "Anular" }) })
        ] }, R.id)) })
      ] }) : /* @__PURE__ */ l.jsx("p", { className: "muted", style: { margin: 0 }, children: "Sin entregas todavía." })
    ] }),
    u && /* @__PURE__ */ l.jsxs(
      an,
      {
        titulo: "Entrega (albarán de venta)",
        alCerrar: () => h(null),
        ancho: 640,
        acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => h(null), children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", onClick: async () => await lt(() => t.post(`/pedidos-venta/${a.id}/entregar`, { fecha: S, referencia: v || null, lineas: Object.entries(u).filter(([, R]) => R > 0).map(([R, Q]) => ({ lineaPedidoId: R, cantidad: Q })) }), n.aviso, "Albarán creado.") && (h(null), o()), children: "Crear albarán" })
        ] }),
        children: [
          /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Fecha" }),
              /* @__PURE__ */ l.jsx("input", { type: "date", value: S, onChange: (R) => $(R.target.value) })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Referencia" }),
              /* @__PURE__ */ l.jsx("input", { value: v, onChange: (R) => y(R.target.value) })
            ] })
          ] }),
          /* @__PURE__ */ l.jsxs("table", { style: { marginTop: 10 }, children: [
            /* @__PURE__ */ l.jsx("thead", { children: /* @__PURE__ */ l.jsxs("tr", { children: [
              /* @__PURE__ */ l.jsx("th", { children: "Línea" }),
              /* @__PURE__ */ l.jsx("th", { className: "num", children: "Pendiente" }),
              /* @__PURE__ */ l.jsx("th", { className: "num", style: { width: 120 }, children: "Entregar" })
            ] }) }),
            /* @__PURE__ */ l.jsx("tbody", { children: a.lineas.filter((R) => R.pendienteServir > 0).map((R) => /* @__PURE__ */ l.jsxs("tr", { children: [
              /* @__PURE__ */ l.jsx("td", { children: R.descripcion }),
              /* @__PURE__ */ l.jsx("td", { className: "num", children: $e(R.pendienteServir) }),
              /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx("input", { className: "num", type: "number", step: "0.001", value: u[R.id] ?? 0, onChange: (Q) => h({ ...u, [R.id]: Number(Q.target.value) }) }) })
            ] }, R.id)) })
          ] })
        ]
      }
    ),
    p && /* @__PURE__ */ l.jsxs(
      an,
      {
        titulo: `Facturar el pedido ${a.numeroCompleto}`,
        alCerrar: () => f(!1),
        acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => f(!1), children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", onClick: async () => {
            try {
              const R = await t.post(`/pedidos-venta/${a.id}/facturar`, { fechaEmision: x, formaPagoId: T || null });
              n.aviso("Factura emitida.", "ok"), r({ tipo: "factura", pantalla: "vista", id: R.id });
            } catch (R) {
              n.aviso(R.message, "err");
            }
          }, children: "Emitir factura" })
        ] }),
        children: [
          /* @__PURE__ */ l.jsx("p", { className: "muted", style: { marginTop: 0 }, children: "Se factura el pedido completo con sus precios y conceptos. La factura queda numerada y encadenada en VeriFactu." }),
          /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Fecha de emisión" }),
              /* @__PURE__ */ l.jsx("input", { type: "date", value: x, onChange: (R) => E(R.target.value) })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Forma de pago" }),
              /* @__PURE__ */ l.jsxs("select", { value: T, onChange: (R) => F(R.target.value), children: [
                /* @__PURE__ */ l.jsx("option", { value: "", children: "La del cliente" }),
                d.map((R) => /* @__PURE__ */ l.jsx("option", { value: R.id, children: R.nombre }, R.id))
              ] })
            ] })
          ] })
        ]
      }
    )
  ] });
}
function gm(e) {
  const { api: t, anfitrion: n, navegar: r } = Ct(), { dato: a, error: i, recargar: o } = dl(() => t.get(`/compras/pedidos/${e.id}`)), [s, c] = g.useState([]), [d, N] = g.useState([]), [u, h] = g.useState([]), [v, y] = g.useState(null), [S, $] = g.useState(""), [p, f] = g.useState(""), [x, E] = g.useState(gt()), [T, F] = g.useState(!1), [D, k] = g.useState("IVA21"), [M, O] = g.useState(0), [R, Q] = g.useState(""), [se, Fe] = g.useState(gt());
  if (g.useEffect(() => void t.get(`/compras/pedidos/${e.id}/albaranes`).then(c).catch(() => c([])), [t, e.id, a]), g.useEffect(() => {
    t.get("/inventario/almacenes").then((j) => (N(j), j[0] && $(j[0].id))).catch(() => N([])), t.get("/tipos-iva").then((j) => h(j.filter((w) => w.activo))).catch(() => h([]));
  }, [t]), i) return /* @__PURE__ */ l.jsx("div", { className: "panel", children: /* @__PURE__ */ l.jsx("p", { className: "dx-rojo", children: i }) });
  if (!a) return /* @__PURE__ */ l.jsx("div", { className: "muted", children: "Cargando…" });
  const Le = (a.estado === "Borrador" || a.estado === "Confirmado") && a.lineas.every((j) => j.cantidadRecibida === 0 && j.cantidadFacturada === 0) && !a.empresaOrigenId, fe = a.estado !== "Cancelado" && a.estado !== "Facturado", xe = a.lineas.some((j) => j.pendienteRecibir > 0), I = a.lineas.reduce((j, w) => j + w.costeConceptos, 0);
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsx(
        ul,
        {
          titulo: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            "Pedido de compra ",
            /* @__PURE__ */ l.jsx("span", { className: "mono", children: a.numeroCompleto })
          ] }),
          estado: a.estado,
          volver: () => r({ tipo: "compra", pantalla: "lista" }),
          acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            !a.empresaOrigenId && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "compra", pantalla: "editor", semilla: { ...a, fecha: gt() } }), children: "Duplicar" }),
            Le && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "compra", pantalla: "editor", id: a.id, semilla: a }), children: "Editar" }),
            a.estado === "Borrador" && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: async () => await lt(() => t.post(`/compras/pedidos/${a.id}/confirmar`), n.aviso, "Pedido confirmado.") && o(), children: "Confirmar" }),
            fe && a.estado !== "Borrador" && xe && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => y(Object.fromEntries(a.lineas.map((j) => [j.id, { cantidad: j.pendienteRecibir, lote: "" }]))), children: "Recibir mercancía" }),
            fe && a.estado !== "Borrador" && /* @__PURE__ */ l.jsx("button", { className: "btn small", onClick: () => F(!0), children: "Facturar" }),
            fe && !a.empresaOrigenId && /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", onClick: async () => window.confirm("¿Cancelar el pedido?") && await lt(() => t.post(`/compras/pedidos/${a.id}/cancelar`), n.aviso, "Pedido cancelado.") && o(), children: "Cancelar" })
          ] })
        }
      ),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ l.jsx(ze, { etiqueta: "Proveedor", children: /* @__PURE__ */ l.jsx("strong", { children: a.proveedorTexto }) }),
        /* @__PURE__ */ l.jsx(ze, { etiqueta: "Fecha", children: je(a.fecha) }),
        /* @__PURE__ */ l.jsx(ze, { etiqueta: "Total", children: z(a.total) }),
        /* @__PURE__ */ l.jsx(ze, { etiqueta: "Costes añadidos", children: I ? z(I) : "—" })
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
      /* @__PURE__ */ l.jsx("tbody", { children: a.lineas.map((j) => /* @__PURE__ */ l.jsxs("tr", { children: [
        /* @__PURE__ */ l.jsxs("td", { children: [
          j.descripcion,
          /* @__PURE__ */ l.jsx(cl, { conceptos: j.conceptos })
        ] }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: $e(j.cantidad) }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: $e(j.cantidadRecibida) }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: j.pendienteRecibir > 0 ? /* @__PURE__ */ l.jsx("strong", { children: $e(j.pendienteRecibir) }) : "—" }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: z(j.precioUnitario) }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: z(j.importe) }) }),
        /* @__PURE__ */ l.jsxs("td", { className: "num muted", children: [
          z(j.costeUnitarioEntrada),
          "/ud"
        ] })
      ] }, j.id)) })
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
        /* @__PURE__ */ l.jsx("tbody", { children: s.map((j) => {
          var w;
          return /* @__PURE__ */ l.jsxs("tr", { children: [
            /* @__PURE__ */ l.jsxs("td", { className: "mono", children: [
              /* @__PURE__ */ l.jsx("strong", { children: j.numeroCompleto }),
              " ",
              j.anulado && /* @__PURE__ */ l.jsx("span", { className: "pill neg", title: j.motivoAnulacion ?? "", children: "Anulado" })
            ] }),
            /* @__PURE__ */ l.jsx("td", { children: je(j.fecha) }),
            /* @__PURE__ */ l.jsx("td", { className: "muted", children: j.referencia }),
            /* @__PURE__ */ l.jsx("td", { className: "muted", children: ((w = d.find((b) => b.id === j.almacenId)) == null ? void 0 : w.nombre) ?? "—" }),
            /* @__PURE__ */ l.jsx("td", { className: "muted", children: j.lineas.map((b) => `${$e(b.cantidad)} × ${b.descripcion}`).join(" · ") }),
            /* @__PURE__ */ l.jsx("td", { className: "right", children: !j.anulado && a.estado !== "Facturado" && /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", onClick: async () => {
              const b = window.prompt("Motivo de la anulación del albarán:");
              b !== null && await lt(() => t.post(`/compras/pedidos/${a.id}/albaranes/${j.id}/anular`, { motivo: b || null }), n.aviso, "Albarán anulado.") && o();
            }, children: "Anular" }) })
          ] }, j.id);
        }) })
      ] }) : /* @__PURE__ */ l.jsx("p", { className: "muted", style: { margin: 0 }, children: "Sin recepciones todavía." })
    ] }),
    v && /* @__PURE__ */ l.jsxs(
      an,
      {
        titulo: "Recepción de mercancía",
        alCerrar: () => y(null),
        ancho: 680,
        acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => y(null), children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", onClick: async () => await lt(() => t.post(`/compras/pedidos/${a.id}/recibir`, { fecha: x, referencia: p || null, almacenId: S || null, lineas: Object.entries(v).filter(([, j]) => j.cantidad > 0).map(([j, w]) => ({ lineaPedidoId: j, cantidad: w.cantidad, lote: w.lote || null })) }), n.aviso, "Recepción registrada.") && (y(null), o()), children: "Registrar recepción" })
        ] }),
        children: [
          /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Fecha" }),
              /* @__PURE__ */ l.jsx("input", { type: "date", value: x, onChange: (j) => E(j.target.value) })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Albarán del proveedor" }),
              /* @__PURE__ */ l.jsx("input", { value: p, onChange: (j) => f(j.target.value) })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Almacén" }),
              /* @__PURE__ */ l.jsxs("select", { value: S, onChange: (j) => $(j.target.value), children: [
                /* @__PURE__ */ l.jsx("option", { value: "", children: "Sin entrada en almacén" }),
                d.map((j) => /* @__PURE__ */ l.jsx("option", { value: j.id, children: j.nombre }, j.id))
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
            /* @__PURE__ */ l.jsx("tbody", { children: a.lineas.filter((j) => j.pendienteRecibir > 0).map((j) => {
              var w, b;
              return /* @__PURE__ */ l.jsxs("tr", { children: [
                /* @__PURE__ */ l.jsx("td", { children: j.descripcion }),
                /* @__PURE__ */ l.jsx("td", { className: "num", children: $e(j.pendienteRecibir) }),
                /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx("input", { className: "num", type: "number", step: "0.001", value: ((w = v[j.id]) == null ? void 0 : w.cantidad) ?? 0, onChange: (W) => y({ ...v, [j.id]: { ...v[j.id], cantidad: Number(W.target.value) } }) }) }),
                /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx("input", { value: ((b = v[j.id]) == null ? void 0 : b.lote) ?? "", onChange: (W) => y({ ...v, [j.id]: { ...v[j.id], lote: W.target.value } }) }) })
              ] }, j.id);
            }) })
          ] })
        ]
      }
    ),
    T && /* @__PURE__ */ l.jsxs(
      an,
      {
        titulo: `Facturar el pedido ${a.numeroCompleto}`,
        alCerrar: () => F(!1),
        acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => F(!1), children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", onClick: async () => await lt(() => t.post(`/compras/pedidos/${a.id}/facturar`, { codigoIva: D, porcentajeIrpf: M, numeroFactura: R || null, fechaFactura: se }), n.aviso, "Factura del proveedor registrada como gasto.") && (F(!1), o()), children: "Registrar factura" })
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
              /* @__PURE__ */ l.jsx("input", { value: R, onChange: (j) => Q(j.target.value), autoFocus: !0 })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Fecha de la factura" }),
              /* @__PURE__ */ l.jsx("input", { type: "date", value: se, onChange: (j) => Fe(j.target.value) })
            ] })
          ] }),
          /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Impuesto" }),
              /* @__PURE__ */ l.jsx("select", { value: D, onChange: (j) => k(j.target.value), children: u.map((j) => /* @__PURE__ */ l.jsx("option", { value: j.codigo, children: j.nombre }, j.codigo)) })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Retención IRPF %" }),
              /* @__PURE__ */ l.jsx("input", { type: "number", step: "0.01", value: M, onChange: (j) => O(Number(j.target.value)) })
            ] })
          ] })
        ]
      }
    )
  ] });
}
const Vl = (e = "") => ({ clave: Qr(), descripcion: "", cuentaGasto: "", base: 0, codigoIva: e, porcentajeIva: null, porcentajeDeducible: 100 }), ym = (e) => e === "InversionSujetoPasivo" || e === "Intracomunitario";
function jm(e) {
  const { api: t, anfitrion: n } = Ct(), r = e.semilla, [a, i] = g.useState([]), [o, s] = g.useState([]), [c, d] = g.useState([]), [N, u] = g.useState([]), [h, v] = g.useState((r == null ? void 0 : r.proveedorId) ?? ""), [y, S] = g.useState(e.id ? (r == null ? void 0 : r.numeroFactura) ?? "" : ""), [$, p] = g.useState((r == null ? void 0 : r.fechaFactura) ?? gt()), [f, x] = g.useState(e.id ? (r == null ? void 0 : r.fecha) ?? gt() : gt()), [E, T] = g.useState(r != null && r.concepto && !r.concepto.startsWith("Factura ") ? r.concepto : ""), [F, D] = g.useState((r == null ? void 0 : r.porcentajeIrpf) ?? 0), [k, M] = g.useState(""), [O, R] = g.useState(((r == null ? void 0 : r.recargoTotal) ?? 0) > 0), Q = !!(r != null && r.esRectificativa), [se, Fe] = g.useState((r == null ? void 0 : r.numeroRectificado) ?? ""), [Le, fe] = g.useState((r == null ? void 0 : r.fechaRectificada) ?? ""), [xe, I] = g.useState((r == null ? void 0 : r.motivoRectificacion) ?? ""), [j, w] = g.useState(!1), [b, W] = g.useState((r == null ? void 0 : r.afectacion) ?? "Comun"), [K, pe] = g.useState(
    () => {
      var m;
      return (m = r == null ? void 0 : r.lineas) != null && m.length ? r.lineas.map((L) => ({ clave: Qr(), descripcion: L.descripcion ?? "", cuentaGasto: L.cuentaGasto ?? "", base: L.base, codigoIva: L.codigoIva, porcentajeIva: L.autoliquidada ? L.porcentajeIva : null, porcentajeDeducible: L.porcentajeDeducible })) : [Vl()];
    }
  ), [me, te] = g.useState(e.id && (r != null && r.vencimientos) && r.vencimientos.length > 1 ? r.vencimientos : null), [U, ut] = g.useState(null), [Ke, dt] = g.useState(""), [Et, At] = g.useState(!1), kn = Gr();
  g.useEffect(() => {
    t.get("/proveedores").then(i).catch(() => i([])), t.get("/tipos-iva").then((m) => s(m.filter((L) => L.activo))).catch(() => s([])), t.get("/formas-pago").then((m) => d(m.filter((L) => L.activo))).catch(() => d([])), t.get("/empresas/actual").then((m) => {
      m.regimenIva === "RecargoEquivalencia" && (w(!0), r || R(!0));
    }).catch(() => {
    }), t.get("/contabilidad/cuentas").then((m) => u(m.filter((L) => L.codigo.startsWith("6") || L.codigo.startsWith("2")))).catch(() => u([]));
  }, [t]);
  const X = a.find((m) => m.id === h);
  g.useEffect(() => {
    X != null && X.formaPagoDefectoId && !k && M(X.formaPagoDefectoId);
  }, [X]);
  const Se = g.useMemo(
    () => ({
      proveedorId: h || null,
      proveedorTexto: (X == null ? void 0 : X.nombre) ?? null,
      numeroFactura: y.trim() || null,
      fechaFactura: $ || null,
      fecha: f,
      concepto: E.trim() || null,
      porcentajeIrpf: F,
      formaPagoId: k || null,
      recargoEquivalencia: O,
      afectacion: b,
      baseImponible: 0,
      lineas: K.filter((m) => m.base !== 0).map((m) => ({
        base: m.base,
        codigoIva: m.codigoIva || null,
        descripcion: m.descripcion.trim() || null,
        porcentajeIva: m.porcentajeIva,
        porcentajeDeducible: m.porcentajeDeducible,
        cuentaGasto: m.cuentaGasto.trim() || null
      })),
      vencimientos: me,
      rectificaGastoId: Q ? (r == null ? void 0 : r.rectificaGastoId) ?? null : null,
      numeroRectificado: Q && se.trim() || null,
      fechaRectificada: Q && Le || null,
      motivoRectificacion: Q && xe.trim() || null
    }),
    [h, X, y, $, f, E, F, k, O, b, K, me, Q, r, se, Le, xe]
  ), qe = vn(Se, 350);
  g.useEffect(() => {
    if (!qe.lineas.length) {
      ut(null), dt("Añade al menos una línea con base.");
      return;
    }
    const m = kn();
    t.post("/gastos/simular", qe).then((L) => m() && (ut(L), dt(""))).catch((L) => m() && (ut(null), dt(L.message)));
  }, [qe, t]);
  const nt = (m, L) => pe((_) => _.map((B) => B.clave === m ? { ...B, ...L } : B)), Ot = (m) => o.find((L) => L.codigo === m), ie = (m) => {
    var L;
    return (L = U == null ? void 0 : U.lineas) == null ? void 0 : L[K.filter((_) => _.base !== 0).indexOf(m)];
  };
  function Ve(m) {
    if (!U) return;
    const L = /* @__PURE__ */ new Date(($ || f) + "T00:00:00"), _ = Ae(U.total / m);
    te(Array.from({ length: m }, (B, H) => {
      const Z = new Date(L);
      return Z.setMonth(Z.getMonth() + H + 1), { fecha: Z.toISOString().slice(0, 10), importe: H === m - 1 ? Ae(U.total - _ * (m - 1)) : _ };
    }));
  }
  async function It() {
    At(!0);
    try {
      const m = e.id ? await t.put(`/gastos/${e.id}`, Se) : await t.post("/gastos", Se);
      n.aviso(e.id ? "Factura corregida." : "Factura registrada.", "ok"), m.avisoRiesgo && n.aviso(m.avisoRiesgo, "err"), e.alGuardar(m.id);
    } catch (m) {
      n.aviso(m.message, "err");
    } finally {
      At(!1);
    }
  }
  const Ut = Ae((me ?? []).reduce((m, L) => m + (Number(L.importe) || 0), 0));
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsxs("div", { className: "panel-head", children: [
        /* @__PURE__ */ l.jsx("h2", { children: e.id ? `Corregir factura ${(r == null ? void 0 : r.numeroFactura) ?? ""}` : Q ? "Rectificativa / abono del proveedor" : "Nueva factura de proveedor" }),
        /* @__PURE__ */ l.jsxs("div", { style: { display: "flex", gap: 8 }, children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: e.alCancelar, children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", disabled: !U || Et, onClick: It, children: e.id ? "Guardar corrección" : Q ? "Registrar abono" : "Registrar factura" })
        ] })
      ] }),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-cabecera", children: [
        /* @__PURE__ */ l.jsxs("div", { className: "dx-cab-campos", children: [
          /* @__PURE__ */ l.jsx(zo, { terceros: a, valor: h, alCambiar: v, etiqueta: "Proveedor" }),
          /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Nº de factura del proveedor" }),
              /* @__PURE__ */ l.jsx("input", { value: y, onChange: (m) => S(m.target.value), placeholder: "F-2026/0117" })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Fecha de la factura" }),
              /* @__PURE__ */ l.jsx("input", { type: "date", value: $, onChange: (m) => p(m.target.value) })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Fecha de registro" }),
              /* @__PURE__ */ l.jsx("input", { type: "date", value: f, onChange: (m) => x(m.target.value), title: "La del asiento y la del periodo de IVA en que se deduce" })
            ] })
          ] }),
          /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Forma de pago" }),
              /* @__PURE__ */ l.jsxs("select", { value: k, onChange: (m) => M(m.target.value), children: [
                /* @__PURE__ */ l.jsx("option", { value: "", children: "Contado (vence en la fecha de factura)" }),
                c.map((m) => /* @__PURE__ */ l.jsx("option", { value: m.id, children: m.nombre }, m.id))
              ] })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Retención IRPF %" }),
              /* @__PURE__ */ l.jsx("input", { type: "number", step: "0.01", value: F, onChange: (m) => D(Number(m.target.value)) })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Prorrata especial" }),
              /* @__PURE__ */ l.jsxs("select", { value: b, onChange: (m) => W(m.target.value), children: [
                /* @__PURE__ */ l.jsx("option", { value: "Comun", children: "Uso común" }),
                /* @__PURE__ */ l.jsx("option", { value: "ConDerecho", children: "Solo operaciones con derecho" }),
                /* @__PURE__ */ l.jsx("option", { value: "SinDerecho", children: "Solo operaciones exentas" })
              ] })
            ] })
          ] }),
          /* @__PURE__ */ l.jsx("div", { className: "dx-fila", children: /* @__PURE__ */ l.jsxs("div", { style: { gridColumn: "1 / -1" }, children: [
            /* @__PURE__ */ l.jsx("label", { children: "Concepto (vacío: «Factura nº»)" }),
            /* @__PURE__ */ l.jsx("input", { value: E, onChange: (m) => T(m.target.value) })
          ] }) }),
          Q && /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Factura que rectifica" }),
              /* @__PURE__ */ l.jsx("input", { value: se, disabled: !!(r != null && r.rectificaGastoId), onChange: (m) => Fe(m.target.value) })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Fecha de la rectificada" }),
              /* @__PURE__ */ l.jsx("input", { type: "date", value: Le, disabled: !!(r != null && r.rectificaGastoId), onChange: (m) => fe(m.target.value) })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Motivo" }),
              /* @__PURE__ */ l.jsx("input", { value: xe, onChange: (m) => I(m.target.value), placeholder: "Devolución, descuento posterior, error de precio…" })
            ] })
          ] }),
          Q && /* @__PURE__ */ l.jsx("div", { className: "muted", children: "Las bases del abono van en negativo; el asiento y el SII (R1, por diferencias) salen con el signo." }),
          /* @__PURE__ */ l.jsxs("label", { className: "dx-check", children: [
            /* @__PURE__ */ l.jsx("input", { type: "checkbox", checked: O, onChange: (m) => R(m.target.checked) }),
            " El proveedor me cobra recargo de equivalencia"
          ] }),
          j && /* @__PURE__ */ l.jsx("div", { className: "muted", children: "La empresa está en recargo de equivalencia: el IVA y el recargo soportados son coste (no se deducen)." })
        ] }),
        /* @__PURE__ */ l.jsx("div", { className: "dx-ficha", children: X ? /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("strong", { children: X.nombre }),
          /* @__PURE__ */ l.jsx("div", { className: "muted", children: [X.nifFiscal, X.poblacion, X.pais].filter(Boolean).join(" · ") }),
          !X.nifFiscal && /* @__PURE__ */ l.jsx("div", { className: "dx-aviso", children: "⚠ Sin NIF en la ficha: hace falta para el libro de IVA, el 347 y el SII." }),
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
        /* @__PURE__ */ l.jsx("tbody", { children: K.map((m, L) => {
          const _ = Ot(m.codigoIva), B = ie(m);
          return /* @__PURE__ */ l.jsxs("tr", { className: L % 2 ? "dx-par" : "", children: [
            /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx("input", { value: m.descripcion, onChange: (H) => nt(m.clave, { descripcion: H.target.value }), placeholder: "Qué se compra" }) }),
            /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx("input", { list: "dx-cuentas-gasto", value: m.cuentaGasto, onChange: (H) => nt(m.clave, { cuentaGasto: H.target.value }), placeholder: "la de la regla" }) }),
            /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx("input", { className: "num", type: "number", step: "0.01", value: m.base || "", onChange: (H) => nt(m.clave, { base: Number(H.target.value) }) }) }),
            /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsxs("select", { value: m.codigoIva, onChange: (H) => nt(m.clave, { codigoIva: H.target.value, porcentajeIva: null }), children: [
              /* @__PURE__ */ l.jsx("option", { value: "", children: "General" }),
              o.map((H) => /* @__PURE__ */ l.jsx("option", { value: H.codigo, children: H.nombre }, H.codigo))
            ] }) }),
            /* @__PURE__ */ l.jsx("td", { children: ym(_ == null ? void 0 : _.clase) ? /* @__PURE__ */ l.jsx("input", { className: "num", type: "number", step: "0.01", value: m.porcentajeIva ?? "", placeholder: "21", title: "Tipo que autoliquidas", onChange: (H) => nt(m.clave, { porcentajeIva: H.target.value === "" ? null : Number(H.target.value) }) }) : /* @__PURE__ */ l.jsx("span", { className: "muted", children: B ? `${ke(B.porcentajeIva)} %` : "" }) }),
            /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx("input", { className: "num", type: "number", step: "1", min: 0, max: 100, value: m.porcentajeDeducible, onChange: (H) => nt(m.clave, { porcentajeDeducible: Number(H.target.value) }) }) }),
            /* @__PURE__ */ l.jsx("td", { className: "num", children: B ? /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
              /* @__PURE__ */ l.jsx("strong", { children: z(B.cuota) }),
              B.autoliquidada && /* @__PURE__ */ l.jsx("div", { className: "dx-sub", children: "autoliquidada" }),
              B.cuotaRecargo !== 0 && /* @__PURE__ */ l.jsxs("div", { className: "dx-sub", children: [
                "+ recargo ",
                z(B.cuotaRecargo)
              ] })
            ] }) : "—" }),
            /* @__PURE__ */ l.jsx("td", { className: "right", children: /* @__PURE__ */ l.jsx("button", { className: "dx-icono", title: "Quitar", onClick: () => pe((H) => H.length > 1 ? H.filter((Z) => Z.clave !== m.clave) : [Vl()]), children: "✕" }) })
          ] }, m.clave);
        }) })
      ] }),
      /* @__PURE__ */ l.jsx("datalist", { id: "dx-cuentas-gasto", children: N.map((m) => /* @__PURE__ */ l.jsx("option", { value: m.codigo, children: m.nombre }, m.codigo)) }),
      /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", style: { marginTop: 8 }, onClick: () => pe((m) => {
        var L;
        return [...m, Vl(((L = m[m.length - 1]) == null ? void 0 : L.codigoIva) ?? "")];
      }), children: "+ Añadir línea" }),
      /* @__PURE__ */ l.jsx("span", { className: "muted dx-ayuda", children: "Una línea por cada base e impuesto de la factura. En inversión del sujeto pasivo e intracomunitarias el IVA lo autoliquidas tú (no lo cobra el proveedor)." })
    ] }),
    /* @__PURE__ */ l.jsxs("div", { className: "dx-pie", children: [
      /* @__PURE__ */ l.jsxs("div", { className: "panel", style: { margin: 0 }, children: [
        /* @__PURE__ */ l.jsxs("div", { className: "panel-head", children: [
          /* @__PURE__ */ l.jsx("h2", { children: "Vencimientos" }),
          /* @__PURE__ */ l.jsxs("div", { className: "dx-acciones", children: [
            [2, 3, 4].map((m) => /* @__PURE__ */ l.jsxs("button", { className: "btn small secondary", disabled: !U, onClick: () => Ve(m), children: [
              m,
              " plazos"
            ] }, m)),
            me && /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", onClick: () => te(null), children: "Según forma de pago" })
          ] })
        ] }),
        me ? /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          me.map((m, L) => /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", style: { marginBottom: 6 }, children: [
            /* @__PURE__ */ l.jsx("input", { type: "date", value: m.fecha, onChange: (_) => te(me.map((B, H) => H === L ? { ...B, fecha: _.target.value } : B)) }),
            /* @__PURE__ */ l.jsx("input", { className: "num", type: "number", step: "0.01", value: m.importe, onChange: (_) => te(me.map((B, H) => H === L ? { ...B, importe: Number(_.target.value) } : B)) })
          ] }, L)),
          U && Ut !== U.total && /* @__PURE__ */ l.jsxs("p", { className: "dx-rojo", style: { margin: 0 }, children: [
            "Los plazos suman ",
            z(Ut),
            "; la factura, ",
            z(U.total),
            "."
          ] })
        ] }) : /* @__PURE__ */ l.jsx("p", { className: "muted", style: { margin: 0 }, children: ((U == null ? void 0 : U.vencimientos) ?? []).map((m) => `${je(m.fecha)}: ${z(m.importe)}`).join(" · ") || "—" }),
        Ke && /* @__PURE__ */ l.jsx("p", { className: "dx-rojo", style: { marginBottom: 0 }, children: Ke })
      ] }),
      /* @__PURE__ */ l.jsxs("div", { className: "panel dx-totales", children: [
        ((U == null ? void 0 : U.desglose) ?? []).map((m, L) => {
          var _;
          return /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
            /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
              ((_ = Ot(m.codigoIva)) == null ? void 0 : _.nombre) ?? m.codigoIva,
              " ",
              m.autoliquidada ? `(${ke(m.porcentajeIva)} %, autoliquidado)` : "",
              " · base ",
              ke(m.base)
            ] }),
            /* @__PURE__ */ l.jsx("span", { children: z(m.cuota) })
          ] }, L);
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
        U && (U.desglose ?? []).some((m) => m.cuotaDeducible !== m.cuota) && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "IVA deducible (antes de prorrata)" }),
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: z((U.desglose ?? []).reduce((m, L) => m + L.cuotaDeducible, 0)) })
        ] })
      ] })
    ] })
  ] });
}
function Nm(e) {
  const { api: t, anfitrion: n, navegar: r } = Ct(), [a, i] = g.useState(null), [o, s] = g.useState(null), [c, d] = g.useState(""), [N, u] = g.useState(!1), h = () => {
    t.get(`/gastos/${e.id}`).then(i).catch((S) => d(S.message)), t.get(`/gastos/${e.id}/saldo`).then(s).catch(() => s(null));
  };
  if (g.useEffect(h, [e.id]), c) return /* @__PURE__ */ l.jsx("div", { className: "panel", children: /* @__PURE__ */ l.jsx("p", { className: "dx-rojo", children: c }) });
  if (!a) return /* @__PURE__ */ l.jsx("div", { className: "muted", children: "Cargando…" });
  const v = a.estado === "Registrado", y = !o || o.liquidado === 0;
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsxs("div", { className: "panel-head", children: [
        /* @__PURE__ */ l.jsxs("h2", { style: { display: "flex", alignItems: "center", gap: 10 }, children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "gasto", pantalla: "lista" }), children: "←" }),
          "Factura ",
          /* @__PURE__ */ l.jsx("span", { className: "mono", children: a.numeroFactura ?? "(sin número)" }),
          " ",
          /* @__PURE__ */ l.jsx("span", { className: To(a.estado === "Anulado" ? "Anulada" : "Emitida"), children: a.estado }),
          a.esRectificativa && /* @__PURE__ */ l.jsxs("span", { className: "pill", children: [
            "Rectifica ",
            a.numeroRectificado
          ] })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-acciones", children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "gasto", pantalla: "editor", semilla: { ...a, numeroFactura: null } }), children: "Duplicar" }),
          v && y && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "gasto", pantalla: "editor", id: a.id, semilla: a }), children: "Corregir" }),
          v && o && o.pendiente > 0 && n.irA && /* @__PURE__ */ l.jsx("button", { className: "btn small", onClick: () => n.irA("pagos"), children: "Pagar" }),
          v && !a.esRectificativa && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => {
            var S;
            return r({ tipo: "gasto", pantalla: "editor", semilla: {
              ...a,
              numeroFactura: null,
              esRectificativa: !0,
              rectificaGastoId: a.id,
              numeroRectificado: a.numeroFactura ?? a.concepto,
              fechaRectificada: a.fechaFactura ?? a.fecha,
              motivoRectificacion: "",
              vencimientos: null,
              lineas: (S = a.lineas) == null ? void 0 : S.map(($) => ({ ...$, base: -$.base }))
            } });
          }, children: "Rectificativa / abono" }),
          v && y && /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", onClick: () => u(!0), children: "Anular" })
        ] })
      ] }),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ l.jsxs("div", { className: "dx-dato", children: [
          /* @__PURE__ */ l.jsx("small", { children: "Proveedor" }),
          /* @__PURE__ */ l.jsx("div", { children: /* @__PURE__ */ l.jsx("strong", { children: a.proveedorTexto }) })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-dato", children: [
          /* @__PURE__ */ l.jsx("small", { children: "Fecha de factura" }),
          /* @__PURE__ */ l.jsx("div", { children: je(a.fechaFactura ?? a.fecha) })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-dato", children: [
          /* @__PURE__ */ l.jsx("small", { children: "Registro" }),
          /* @__PURE__ */ l.jsx("div", { children: je(a.fecha) })
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
        /* @__PURE__ */ l.jsx("tbody", { children: (a.lineas ?? []).map((S, $) => /* @__PURE__ */ l.jsxs("tr", { children: [
          /* @__PURE__ */ l.jsx("td", { children: S.descripcion ?? "" }),
          /* @__PURE__ */ l.jsx("td", { className: "mono muted", children: S.cuentaGasto ?? "regla" }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: z(S.base) }),
          /* @__PURE__ */ l.jsxs("td", { children: [
            S.codigoIva,
            " · ",
            ke(S.porcentajeIva),
            " %",
            S.autoliquidada ? " · autoliquidada" : "",
            S.cuotaRecargo ? ` · recargo ${z(S.cuotaRecargo)}` : ""
          ] }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: z(S.cuota) }),
          /* @__PURE__ */ l.jsxs("td", { className: "num muted", children: [
            S.porcentajeDeducible !== 100 ? `${ke(S.porcentajeDeducible)} % · ` : "",
            z(S.cuotaDeducible)
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
            ke(a.porcentajeIrpf),
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
        (a.vencimientos ?? []).map((S) => `${je(S.fecha)} ${z(S.importe)}`).join(" · ")
      ] })
    ] }),
    N && /* @__PURE__ */ l.jsx(
      an,
      {
        titulo: "Anular la factura",
        alCerrar: () => u(!1),
        acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => u(!1), children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", onClick: async () => {
            try {
              await t.post(`/gastos/${a.id}/anular`), n.aviso("Factura anulada.", "ok"), u(!1), h();
            } catch (S) {
              n.aviso(S.message, "err");
            }
          }, children: "Anular" })
        ] }),
        children: /* @__PURE__ */ l.jsx("p", { style: { margin: 0 }, children: "Sale de los libros de IVA y de las declaraciones, y se contabiliza su contraasiento. Si solo hay que cambiar algo, mejor «Corregir»." })
      }
    )
  ] });
}
function Sm(e) {
  const [t, n] = g.useState(e.inicial), r = g.useRef(0), [a, i] = g.useState(0), o = g.useMemo(() => Jp(e.anfitrion), [e.anfitrion]), s = (u) => {
    n(u), i(++r.current), window.scrollTo({ top: 0 });
  }, c = { api: o, anfitrion: e.anfitrion, ruta: t, navegar: s }, d = `${t.tipo}-${t.pantalla}-${t.id ?? ""}-${a}`;
  let N;
  if (t.pantalla === "lista") N = /* @__PURE__ */ l.jsx(cm, { tipo: t.tipo });
  else if (t.pantalla === "vista" && t.id)
    N = t.tipo === "factura" ? /* @__PURE__ */ l.jsx(hm, { id: t.id }) : t.tipo === "presupuesto" ? /* @__PURE__ */ l.jsx(vm, { id: t.id }) : t.tipo === "pedido" ? /* @__PURE__ */ l.jsx(xm, { id: t.id }) : t.tipo === "gasto" ? /* @__PURE__ */ l.jsx(Nm, { id: t.id }) : /* @__PURE__ */ l.jsx(gm, { id: t.id });
  else if (t.tipo === "gasto")
    N = /* @__PURE__ */ l.jsx(
      jm,
      {
        id: t.id,
        semilla: t.semilla,
        alGuardar: (u) => s({ tipo: "gasto", pantalla: "vista", id: u }),
        alCancelar: () => s(t.id ? { tipo: "gasto", pantalla: "vista", id: t.id } : { tipo: "gasto", pantalla: "lista" })
      }
    );
  else if (t.tipo === "compra")
    N = /* @__PURE__ */ l.jsx(
      mm,
      {
        id: t.id,
        semilla: t.semilla,
        alGuardar: (u) => s({ tipo: "compra", pantalla: "vista", id: u }),
        alCancelar: () => s(t.id ? { tipo: "compra", pantalla: "vista", id: t.id } : { tipo: "compra", pantalla: "lista" })
      }
    );
  else {
    const u = t.tipo;
    N = /* @__PURE__ */ l.jsx(
      pm,
      {
        tipo: u,
        id: t.id,
        semilla: t.semilla,
        alGuardar: (h) => s({ tipo: u, pantalla: "vista", id: h }),
        alCancelar: () => s(t.id ? { tipo: u, pantalla: "vista", id: t.id } : { tipo: u, pantalla: "lista" })
      }
    );
  }
  return /* @__PURE__ */ l.jsx(pd.Provider, { value: c, children: /* @__PURE__ */ l.jsx("div", { className: "dx-raiz", children: N }, d) });
}
const wm = `
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
function km() {
  if (document.getElementById("dx-estilos")) return;
  const e = document.createElement("style");
  e.id = "dx-estilos", e.textContent = wm, document.head.appendChild(e);
}
function Cm(e, t, n) {
  km();
  const r = fd(e);
  return r.render(/* @__PURE__ */ l.jsx(Sm, { anfitrion: t, inicial: n })), () => r.unmount();
}
export {
  Cm as montar
};
