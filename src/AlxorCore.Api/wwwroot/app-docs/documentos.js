var ic = { exports: {} }, Ya = {}, oc = { exports: {} }, K = {};
/**
 * @license React
 * react.production.min.js
 *
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */
var Br = Symbol.for("react.element"), zd = Symbol.for("react.portal"), Dd = Symbol.for("react.fragment"), Ld = Symbol.for("react.strict_mode"), Ad = Symbol.for("react.profiler"), Md = Symbol.for("react.provider"), $d = Symbol.for("react.context"), Od = Symbol.for("react.forward_ref"), bd = Symbol.for("react.suspense"), Ud = Symbol.for("react.memo"), Vd = Symbol.for("react.lazy"), Ko = Symbol.iterator;
function Bd(e) {
  return e === null || typeof e != "object" ? null : (e = Ko && e[Ko] || e["@@iterator"], typeof e == "function" ? e : null);
}
var sc = { isMounted: function() {
  return !1;
}, enqueueForceUpdate: function() {
}, enqueueReplaceState: function() {
}, enqueueSetState: function() {
} }, cc = Object.assign, uc = {};
function Jn(e, t, n) {
  this.props = e, this.context = t, this.refs = uc, this.updater = n || sc;
}
Jn.prototype.isReactComponent = {};
Jn.prototype.setState = function(e, t) {
  if (typeof e != "object" && typeof e != "function" && e != null) throw Error("setState(...): takes an object of state variables to update or a function which returns an object of state variables.");
  this.updater.enqueueSetState(this, e, t, "setState");
};
Jn.prototype.forceUpdate = function(e) {
  this.updater.enqueueForceUpdate(this, e, "forceUpdate");
};
function dc() {
}
dc.prototype = Jn.prototype;
function Ui(e, t, n) {
  this.props = e, this.context = t, this.refs = uc, this.updater = n || sc;
}
var Vi = Ui.prototype = new dc();
Vi.constructor = Ui;
cc(Vi, Jn.prototype);
Vi.isPureReactComponent = !0;
var qo = Array.isArray, fc = Object.prototype.hasOwnProperty, Bi = { current: null }, pc = { key: !0, ref: !0, __self: !0, __source: !0 };
function mc(e, t, n) {
  var r, a = {}, i = null, o = null;
  if (t != null) for (r in t.ref !== void 0 && (o = t.ref), t.key !== void 0 && (i = "" + t.key), t) fc.call(t, r) && !pc.hasOwnProperty(r) && (a[r] = t[r]);
  var s = arguments.length - 2;
  if (s === 1) a.children = n;
  else if (1 < s) {
    for (var c = Array(s), d = 0; d < s; d++) c[d] = arguments[d + 2];
    a.children = c;
  }
  if (e && e.defaultProps) for (r in s = e.defaultProps, s) a[r] === void 0 && (a[r] = s[r]);
  return { $$typeof: Br, type: e, key: i, ref: o, props: a, _owner: Bi.current };
}
function Hd(e, t) {
  return { $$typeof: Br, type: e.type, key: t, ref: e.ref, props: e.props, _owner: e._owner };
}
function Hi(e) {
  return typeof e == "object" && e !== null && e.$$typeof === Br;
}
function Wd(e) {
  var t = { "=": "=0", ":": "=2" };
  return "$" + e.replace(/[=:]/g, function(n) {
    return t[n];
  });
}
var Yo = /\/+/g;
function yl(e, t) {
  return typeof e == "object" && e !== null && e.key != null ? Wd("" + e.key) : t.toString(36);
}
function ma(e, t, n, r, a) {
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
        case Br:
        case zd:
          o = !0;
      }
  }
  if (o) return o = e, a = a(o), e = r === "" ? "." + yl(o, 0) : r, qo(a) ? (n = "", e != null && (n = e.replace(Yo, "$&/") + "/"), ma(a, t, n, "", function(d) {
    return d;
  })) : a != null && (Hi(a) && (a = Hd(a, n + (!a.key || o && o.key === a.key ? "" : ("" + a.key).replace(Yo, "$&/") + "/") + e)), t.push(a)), 1;
  if (o = 0, r = r === "" ? "." : r + ":", qo(e)) for (var s = 0; s < e.length; s++) {
    i = e[s];
    var c = r + yl(i, s);
    o += ma(i, t, n, c, a);
  }
  else if (c = Bd(e), typeof c == "function") for (e = c.call(e), s = 0; !(i = e.next()).done; ) i = i.value, c = r + yl(i, s++), o += ma(i, t, n, c, a);
  else if (i === "object") throw t = String(e), Error("Objects are not valid as a React child (found: " + (t === "[object Object]" ? "object with keys {" + Object.keys(e).join(", ") + "}" : t) + "). If you meant to render a collection of children, use an array instead.");
  return o;
}
function Xr(e, t, n) {
  if (e == null) return e;
  var r = [], a = 0;
  return ma(e, r, "", "", function(i) {
    return t.call(n, i, a++);
  }), r;
}
function Qd(e) {
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
var Oe = { current: null }, ha = { transition: null }, Gd = { ReactCurrentDispatcher: Oe, ReactCurrentBatchConfig: ha, ReactCurrentOwner: Bi };
function hc() {
  throw Error("act(...) is not supported in production builds of React.");
}
K.Children = { map: Xr, forEach: function(e, t, n) {
  Xr(e, function() {
    t.apply(this, arguments);
  }, n);
}, count: function(e) {
  var t = 0;
  return Xr(e, function() {
    t++;
  }), t;
}, toArray: function(e) {
  return Xr(e, function(t) {
    return t;
  }) || [];
}, only: function(e) {
  if (!Hi(e)) throw Error("React.Children.only expected to receive a single React element child.");
  return e;
} };
K.Component = Jn;
K.Fragment = Dd;
K.Profiler = Ad;
K.PureComponent = Ui;
K.StrictMode = Ld;
K.Suspense = bd;
K.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED = Gd;
K.act = hc;
K.cloneElement = function(e, t, n) {
  if (e == null) throw Error("React.cloneElement(...): The argument must be a React element, but you passed " + e + ".");
  var r = cc({}, e.props), a = e.key, i = e.ref, o = e._owner;
  if (t != null) {
    if (t.ref !== void 0 && (i = t.ref, o = Bi.current), t.key !== void 0 && (a = "" + t.key), e.type && e.type.defaultProps) var s = e.type.defaultProps;
    for (c in t) fc.call(t, c) && !pc.hasOwnProperty(c) && (r[c] = t[c] === void 0 && s !== void 0 ? s[c] : t[c]);
  }
  var c = arguments.length - 2;
  if (c === 1) r.children = n;
  else if (1 < c) {
    s = Array(c);
    for (var d = 0; d < c; d++) s[d] = arguments[d + 2];
    r.children = s;
  }
  return { $$typeof: Br, type: e.type, key: a, ref: i, props: r, _owner: o };
};
K.createContext = function(e) {
  return e = { $$typeof: $d, _currentValue: e, _currentValue2: e, _threadCount: 0, Provider: null, Consumer: null, _defaultValue: null, _globalName: null }, e.Provider = { $$typeof: Md, _context: e }, e.Consumer = e;
};
K.createElement = mc;
K.createFactory = function(e) {
  var t = mc.bind(null, e);
  return t.type = e, t;
};
K.createRef = function() {
  return { current: null };
};
K.forwardRef = function(e) {
  return { $$typeof: Od, render: e };
};
K.isValidElement = Hi;
K.lazy = function(e) {
  return { $$typeof: Vd, _payload: { _status: -1, _result: e }, _init: Qd };
};
K.memo = function(e, t) {
  return { $$typeof: Ud, type: e, compare: t === void 0 ? null : t };
};
K.startTransition = function(e) {
  var t = ha.transition;
  ha.transition = {};
  try {
    e();
  } finally {
    ha.transition = t;
  }
};
K.unstable_act = hc;
K.useCallback = function(e, t) {
  return Oe.current.useCallback(e, t);
};
K.useContext = function(e) {
  return Oe.current.useContext(e);
};
K.useDebugValue = function() {
};
K.useDeferredValue = function(e) {
  return Oe.current.useDeferredValue(e);
};
K.useEffect = function(e, t) {
  return Oe.current.useEffect(e, t);
};
K.useId = function() {
  return Oe.current.useId();
};
K.useImperativeHandle = function(e, t, n) {
  return Oe.current.useImperativeHandle(e, t, n);
};
K.useInsertionEffect = function(e, t) {
  return Oe.current.useInsertionEffect(e, t);
};
K.useLayoutEffect = function(e, t) {
  return Oe.current.useLayoutEffect(e, t);
};
K.useMemo = function(e, t) {
  return Oe.current.useMemo(e, t);
};
K.useReducer = function(e, t, n) {
  return Oe.current.useReducer(e, t, n);
};
K.useRef = function(e) {
  return Oe.current.useRef(e);
};
K.useState = function(e) {
  return Oe.current.useState(e);
};
K.useSyncExternalStore = function(e, t, n) {
  return Oe.current.useSyncExternalStore(e, t, n);
};
K.useTransition = function() {
  return Oe.current.useTransition();
};
K.version = "18.3.1";
oc.exports = K;
var g = oc.exports;
/**
 * @license React
 * react-jsx-runtime.production.min.js
 *
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */
var Kd = g, qd = Symbol.for("react.element"), Yd = Symbol.for("react.fragment"), Xd = Object.prototype.hasOwnProperty, Zd = Kd.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED.ReactCurrentOwner, Jd = { key: !0, ref: !0, __self: !0, __source: !0 };
function vc(e, t, n) {
  var r, a = {}, i = null, o = null;
  n !== void 0 && (i = "" + n), t.key !== void 0 && (i = "" + t.key), t.ref !== void 0 && (o = t.ref);
  for (r in t) Xd.call(t, r) && !Jd.hasOwnProperty(r) && (a[r] = t[r]);
  if (e && e.defaultProps) for (r in t = e.defaultProps, t) a[r] === void 0 && (a[r] = t[r]);
  return { $$typeof: qd, type: e, key: i, ref: o, props: a, _owner: Zd.current };
}
Ya.Fragment = Yd;
Ya.jsx = vc;
Ya.jsxs = vc;
ic.exports = Ya;
var l = ic.exports, xc = { exports: {} }, et = {}, gc = { exports: {} }, yc = {};
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
  function t(E, j) {
    var D = E.length;
    E.push(j);
    e: for (; 0 < D; ) {
      var H = D - 1 >>> 1, Q = E[H];
      if (0 < a(Q, j)) E[H] = j, E[D] = Q, D = H;
      else break e;
    }
  }
  function n(E) {
    return E.length === 0 ? null : E[0];
  }
  function r(E) {
    if (E.length === 0) return null;
    var j = E[0], D = E.pop();
    if (D !== j) {
      E[0] = D;
      e: for (var H = 0, Q = E.length, q = Q >>> 1; H < q; ) {
        var he = 2 * (H + 1) - 1, M = E[he], te = he + 1, V = E[te];
        if (0 > a(M, D)) te < Q && 0 > a(V, M) ? (E[H] = V, E[te] = D, H = te) : (E[H] = M, E[he] = D, H = he);
        else if (te < Q && 0 > a(V, D)) E[H] = V, E[te] = D, H = te;
        else break e;
      }
    }
    return j;
  }
  function a(E, j) {
    var D = E.sortIndex - j.sortIndex;
    return D !== 0 ? D : E.id - j.id;
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
  var c = [], d = [], N = 1, u = null, h = 3, v = !1, y = !1, S = !1, O = typeof setTimeout == "function" ? setTimeout : null, p = typeof clearTimeout == "function" ? clearTimeout : null, f = typeof setImmediate < "u" ? setImmediate : null;
  typeof navigator < "u" && navigator.scheduling !== void 0 && navigator.scheduling.isInputPending !== void 0 && navigator.scheduling.isInputPending.bind(navigator.scheduling);
  function x(E) {
    for (var j = n(d); j !== null; ) {
      if (j.callback === null) r(d);
      else if (j.startTime <= E) r(d), j.sortIndex = j.expirationTime, t(c, j);
      else break;
      j = n(d);
    }
  }
  function C(E) {
    if (S = !1, x(E), !y) if (n(c) !== null) y = !0, fe(_);
    else {
      var j = n(d);
      j !== null && me(C, j.startTime - E);
    }
  }
  function _(E, j) {
    y = !1, S && (S = !1, p(k), k = -1), v = !0;
    var D = h;
    try {
      for (x(j), u = n(c); u !== null && (!(u.expirationTime > j) || E && !P()); ) {
        var H = u.callback;
        if (typeof H == "function") {
          u.callback = null, h = u.priorityLevel;
          var Q = H(u.expirationTime <= j);
          j = e.unstable_now(), typeof Q == "function" ? u.callback = Q : u === n(c) && r(c), x(j);
        } else r(c);
        u = n(c);
      }
      if (u !== null) var q = !0;
      else {
        var he = n(d);
        he !== null && me(C, he.startTime - j), q = !1;
      }
      return q;
    } finally {
      u = null, h = D, v = !1;
    }
  }
  var F = !1, z = null, k = -1, A = 5, U = -1;
  function P() {
    return !(e.unstable_now() - U < A);
  }
  function G() {
    if (z !== null) {
      var E = e.unstable_now();
      U = E;
      var j = !0;
      try {
        j = z(!0, E);
      } finally {
        j ? se() : (F = !1, z = null);
      }
    } else F = !1;
  }
  var se;
  if (typeof f == "function") se = function() {
    f(G);
  };
  else if (typeof MessageChannel < "u") {
    var Pe = new MessageChannel(), De = Pe.port2;
    Pe.port1.onmessage = G, se = function() {
      De.postMessage(null);
    };
  } else se = function() {
    O(G, 0);
  };
  function fe(E) {
    z = E, F || (F = !0, se());
  }
  function me(E, j) {
    k = O(function() {
      E(e.unstable_now());
    }, j);
  }
  e.unstable_IdlePriority = 5, e.unstable_ImmediatePriority = 1, e.unstable_LowPriority = 4, e.unstable_NormalPriority = 3, e.unstable_Profiling = null, e.unstable_UserBlockingPriority = 2, e.unstable_cancelCallback = function(E) {
    E.callback = null;
  }, e.unstable_continueExecution = function() {
    y || v || (y = !0, fe(_));
  }, e.unstable_forceFrameRate = function(E) {
    0 > E || 125 < E ? console.error("forceFrameRate takes a positive int between 0 and 125, forcing frame rates higher than 125 fps is not supported") : A = 0 < E ? Math.floor(1e3 / E) : 5;
  }, e.unstable_getCurrentPriorityLevel = function() {
    return h;
  }, e.unstable_getFirstCallbackNode = function() {
    return n(c);
  }, e.unstable_next = function(E) {
    switch (h) {
      case 1:
      case 2:
      case 3:
        var j = 3;
        break;
      default:
        j = h;
    }
    var D = h;
    h = j;
    try {
      return E();
    } finally {
      h = D;
    }
  }, e.unstable_pauseExecution = function() {
  }, e.unstable_requestPaint = function() {
  }, e.unstable_runWithPriority = function(E, j) {
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
    var D = h;
    h = E;
    try {
      return j();
    } finally {
      h = D;
    }
  }, e.unstable_scheduleCallback = function(E, j, D) {
    var H = e.unstable_now();
    switch (typeof D == "object" && D !== null ? (D = D.delay, D = typeof D == "number" && 0 < D ? H + D : H) : D = H, E) {
      case 1:
        var Q = -1;
        break;
      case 2:
        Q = 250;
        break;
      case 5:
        Q = 1073741823;
        break;
      case 4:
        Q = 1e4;
        break;
      default:
        Q = 5e3;
    }
    return Q = D + Q, E = { id: N++, callback: j, priorityLevel: E, startTime: D, expirationTime: Q, sortIndex: -1 }, D > H ? (E.sortIndex = D, t(d, E), n(c) === null && E === n(d) && (S ? (p(k), k = -1) : S = !0, me(C, D - H))) : (E.sortIndex = Q, t(c, E), y || v || (y = !0, fe(_))), E;
  }, e.unstable_shouldYield = P, e.unstable_wrapCallback = function(E) {
    var j = h;
    return function() {
      var D = h;
      h = j;
      try {
        return E.apply(this, arguments);
      } finally {
        h = D;
      }
    };
  };
})(yc);
gc.exports = yc;
var ef = gc.exports;
/**
 * @license React
 * react-dom.production.min.js
 *
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */
var tf = g, Je = ef;
function I(e) {
  for (var t = "https://reactjs.org/docs/error-decoder.html?invariant=" + e, n = 1; n < arguments.length; n++) t += "&args[]=" + encodeURIComponent(arguments[n]);
  return "Minified React error #" + e + "; visit " + t + " for the full message or use the non-minified dev environment for full errors and additional helpful warnings.";
}
var jc = /* @__PURE__ */ new Set(), Cr = {};
function Sn(e, t) {
  Qn(e, t), Qn(e + "Capture", t);
}
function Qn(e, t) {
  for (Cr[e] = t, e = 0; e < t.length; e++) jc.add(t[e]);
}
var Rt = !(typeof window > "u" || typeof window.document > "u" || typeof window.document.createElement > "u"), Kl = Object.prototype.hasOwnProperty, nf = /^[:A-Z_a-z\u00C0-\u00D6\u00D8-\u00F6\u00F8-\u02FF\u0370-\u037D\u037F-\u1FFF\u200C-\u200D\u2070-\u218F\u2C00-\u2FEF\u3001-\uD7FF\uF900-\uFDCF\uFDF0-\uFFFD][:A-Z_a-z\u00C0-\u00D6\u00D8-\u00F6\u00F8-\u02FF\u0370-\u037D\u037F-\u1FFF\u200C-\u200D\u2070-\u218F\u2C00-\u2FEF\u3001-\uD7FF\uF900-\uFDCF\uFDF0-\uFFFD\-.0-9\u00B7\u0300-\u036F\u203F-\u2040]*$/, Xo = {}, Zo = {};
function rf(e) {
  return Kl.call(Zo, e) ? !0 : Kl.call(Xo, e) ? !1 : nf.test(e) ? Zo[e] = !0 : (Xo[e] = !0, !1);
}
function af(e, t, n, r) {
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
function lf(e, t, n, r) {
  if (t === null || typeof t > "u" || af(e, t, n, r)) return !0;
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
var Ie = {};
"children dangerouslySetInnerHTML defaultValue defaultChecked innerHTML suppressContentEditableWarning suppressHydrationWarning style".split(" ").forEach(function(e) {
  Ie[e] = new be(e, 0, !1, e, null, !1, !1);
});
[["acceptCharset", "accept-charset"], ["className", "class"], ["htmlFor", "for"], ["httpEquiv", "http-equiv"]].forEach(function(e) {
  var t = e[0];
  Ie[t] = new be(t, 1, !1, e[1], null, !1, !1);
});
["contentEditable", "draggable", "spellCheck", "value"].forEach(function(e) {
  Ie[e] = new be(e, 2, !1, e.toLowerCase(), null, !1, !1);
});
["autoReverse", "externalResourcesRequired", "focusable", "preserveAlpha"].forEach(function(e) {
  Ie[e] = new be(e, 2, !1, e, null, !1, !1);
});
"allowFullScreen async autoFocus autoPlay controls default defer disabled disablePictureInPicture disableRemotePlayback formNoValidate hidden loop noModule noValidate open playsInline readOnly required reversed scoped seamless itemScope".split(" ").forEach(function(e) {
  Ie[e] = new be(e, 3, !1, e.toLowerCase(), null, !1, !1);
});
["checked", "multiple", "muted", "selected"].forEach(function(e) {
  Ie[e] = new be(e, 3, !0, e, null, !1, !1);
});
["capture", "download"].forEach(function(e) {
  Ie[e] = new be(e, 4, !1, e, null, !1, !1);
});
["cols", "rows", "size", "span"].forEach(function(e) {
  Ie[e] = new be(e, 6, !1, e, null, !1, !1);
});
["rowSpan", "start"].forEach(function(e) {
  Ie[e] = new be(e, 5, !1, e.toLowerCase(), null, !1, !1);
});
var Wi = /[\-:]([a-z])/g;
function Qi(e) {
  return e[1].toUpperCase();
}
"accent-height alignment-baseline arabic-form baseline-shift cap-height clip-path clip-rule color-interpolation color-interpolation-filters color-profile color-rendering dominant-baseline enable-background fill-opacity fill-rule flood-color flood-opacity font-family font-size font-size-adjust font-stretch font-style font-variant font-weight glyph-name glyph-orientation-horizontal glyph-orientation-vertical horiz-adv-x horiz-origin-x image-rendering letter-spacing lighting-color marker-end marker-mid marker-start overline-position overline-thickness paint-order panose-1 pointer-events rendering-intent shape-rendering stop-color stop-opacity strikethrough-position strikethrough-thickness stroke-dasharray stroke-dashoffset stroke-linecap stroke-linejoin stroke-miterlimit stroke-opacity stroke-width text-anchor text-decoration text-rendering underline-position underline-thickness unicode-bidi unicode-range units-per-em v-alphabetic v-hanging v-ideographic v-mathematical vector-effect vert-adv-y vert-origin-x vert-origin-y word-spacing writing-mode xmlns:xlink x-height".split(" ").forEach(function(e) {
  var t = e.replace(
    Wi,
    Qi
  );
  Ie[t] = new be(t, 1, !1, e, null, !1, !1);
});
"xlink:actuate xlink:arcrole xlink:role xlink:show xlink:title xlink:type".split(" ").forEach(function(e) {
  var t = e.replace(Wi, Qi);
  Ie[t] = new be(t, 1, !1, e, "http://www.w3.org/1999/xlink", !1, !1);
});
["xml:base", "xml:lang", "xml:space"].forEach(function(e) {
  var t = e.replace(Wi, Qi);
  Ie[t] = new be(t, 1, !1, e, "http://www.w3.org/XML/1998/namespace", !1, !1);
});
["tabIndex", "crossOrigin"].forEach(function(e) {
  Ie[e] = new be(e, 1, !1, e.toLowerCase(), null, !1, !1);
});
Ie.xlinkHref = new be("xlinkHref", 1, !1, "xlink:href", "http://www.w3.org/1999/xlink", !0, !1);
["src", "href", "action", "formAction"].forEach(function(e) {
  Ie[e] = new be(e, 1, !1, e.toLowerCase(), null, !0, !0);
});
function Gi(e, t, n, r) {
  var a = Ie.hasOwnProperty(t) ? Ie[t] : null;
  (a !== null ? a.type !== 0 : r || !(2 < t.length) || t[0] !== "o" && t[0] !== "O" || t[1] !== "n" && t[1] !== "N") && (lf(t, n, a, r) && (n = null), r || a === null ? rf(t) && (n === null ? e.removeAttribute(t) : e.setAttribute(t, "" + n)) : a.mustUseProperty ? e[a.propertyName] = n === null ? a.type === 3 ? !1 : "" : n : (t = a.attributeName, r = a.attributeNamespace, n === null ? e.removeAttribute(t) : (a = a.type, n = a === 3 || a === 4 && n === !0 ? "" : "" + n, r ? e.setAttributeNS(r, t, n) : e.setAttribute(t, n))));
}
var Dt = tf.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED, Zr = Symbol.for("react.element"), In = Symbol.for("react.portal"), Pn = Symbol.for("react.fragment"), Ki = Symbol.for("react.strict_mode"), ql = Symbol.for("react.profiler"), Nc = Symbol.for("react.provider"), Sc = Symbol.for("react.context"), qi = Symbol.for("react.forward_ref"), Yl = Symbol.for("react.suspense"), Xl = Symbol.for("react.suspense_list"), Yi = Symbol.for("react.memo"), bt = Symbol.for("react.lazy"), wc = Symbol.for("react.offscreen"), Jo = Symbol.iterator;
function rr(e) {
  return e === null || typeof e != "object" ? null : (e = Jo && e[Jo] || e["@@iterator"], typeof e == "function" ? e : null);
}
var de = Object.assign, jl;
function dr(e) {
  if (jl === void 0) try {
    throw Error();
  } catch (n) {
    var t = n.stack.trim().match(/\n( *(at )?)/);
    jl = t && t[1] || "";
  }
  return `
` + jl + e;
}
var Nl = !1;
function Sl(e, t) {
  if (!e || Nl) return "";
  Nl = !0;
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
    Nl = !1, Error.prepareStackTrace = n;
  }
  return (e = e ? e.displayName || e.name : "") ? dr(e) : "";
}
function of(e) {
  switch (e.tag) {
    case 5:
      return dr(e.type);
    case 16:
      return dr("Lazy");
    case 13:
      return dr("Suspense");
    case 19:
      return dr("SuspenseList");
    case 0:
    case 2:
    case 15:
      return e = Sl(e.type, !1), e;
    case 11:
      return e = Sl(e.type.render, !1), e;
    case 1:
      return e = Sl(e.type, !0), e;
    default:
      return "";
  }
}
function Zl(e) {
  if (e == null) return null;
  if (typeof e == "function") return e.displayName || e.name || null;
  if (typeof e == "string") return e;
  switch (e) {
    case Pn:
      return "Fragment";
    case In:
      return "Portal";
    case ql:
      return "Profiler";
    case Ki:
      return "StrictMode";
    case Yl:
      return "Suspense";
    case Xl:
      return "SuspenseList";
  }
  if (typeof e == "object") switch (e.$$typeof) {
    case Sc:
      return (e.displayName || "Context") + ".Consumer";
    case Nc:
      return (e._context.displayName || "Context") + ".Provider";
    case qi:
      var t = e.render;
      return e = e.displayName, e || (e = t.displayName || t.name || "", e = e !== "" ? "ForwardRef(" + e + ")" : "ForwardRef"), e;
    case Yi:
      return t = e.displayName || null, t !== null ? t : Zl(e.type) || "Memo";
    case bt:
      t = e._payload, e = e._init;
      try {
        return Zl(e(t));
      } catch {
      }
  }
  return null;
}
function sf(e) {
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
      return Zl(t);
    case 8:
      return t === Ki ? "StrictMode" : "Mode";
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
function en(e) {
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
function kc(e) {
  var t = e.type;
  return (e = e.nodeName) && e.toLowerCase() === "input" && (t === "checkbox" || t === "radio");
}
function cf(e) {
  var t = kc(e) ? "checked" : "value", n = Object.getOwnPropertyDescriptor(e.constructor.prototype, t), r = "" + e[t];
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
function Jr(e) {
  e._valueTracker || (e._valueTracker = cf(e));
}
function Cc(e) {
  if (!e) return !1;
  var t = e._valueTracker;
  if (!t) return !0;
  var n = t.getValue(), r = "";
  return e && (r = kc(e) ? e.checked ? "true" : "false" : e.value), e = r, e !== n ? (t.setValue(e), !0) : !1;
}
function Ea(e) {
  if (e = e || (typeof document < "u" ? document : void 0), typeof e > "u") return null;
  try {
    return e.activeElement || e.body;
  } catch {
    return e.body;
  }
}
function Jl(e, t) {
  var n = t.checked;
  return de({}, t, { defaultChecked: void 0, defaultValue: void 0, value: void 0, checked: n ?? e._wrapperState.initialChecked });
}
function es(e, t) {
  var n = t.defaultValue == null ? "" : t.defaultValue, r = t.checked != null ? t.checked : t.defaultChecked;
  n = en(t.value != null ? t.value : n), e._wrapperState = { initialChecked: r, initialValue: n, controlled: t.type === "checkbox" || t.type === "radio" ? t.checked != null : t.value != null };
}
function Ec(e, t) {
  t = t.checked, t != null && Gi(e, "checked", t, !1);
}
function ei(e, t) {
  Ec(e, t);
  var n = en(t.value), r = t.type;
  if (n != null) r === "number" ? (n === 0 && e.value === "" || e.value != n) && (e.value = "" + n) : e.value !== "" + n && (e.value = "" + n);
  else if (r === "submit" || r === "reset") {
    e.removeAttribute("value");
    return;
  }
  t.hasOwnProperty("value") ? ti(e, t.type, n) : t.hasOwnProperty("defaultValue") && ti(e, t.type, en(t.defaultValue)), t.checked == null && t.defaultChecked != null && (e.defaultChecked = !!t.defaultChecked);
}
function ts(e, t, n) {
  if (t.hasOwnProperty("value") || t.hasOwnProperty("defaultValue")) {
    var r = t.type;
    if (!(r !== "submit" && r !== "reset" || t.value !== void 0 && t.value !== null)) return;
    t = "" + e._wrapperState.initialValue, n || t === e.value || (e.value = t), e.defaultValue = t;
  }
  n = e.name, n !== "" && (e.name = ""), e.defaultChecked = !!e._wrapperState.initialChecked, n !== "" && (e.name = n);
}
function ti(e, t, n) {
  (t !== "number" || Ea(e.ownerDocument) !== e) && (n == null ? e.defaultValue = "" + e._wrapperState.initialValue : e.defaultValue !== "" + n && (e.defaultValue = "" + n));
}
var fr = Array.isArray;
function On(e, t, n, r) {
  if (e = e.options, t) {
    t = {};
    for (var a = 0; a < n.length; a++) t["$" + n[a]] = !0;
    for (n = 0; n < e.length; n++) a = t.hasOwnProperty("$" + e[n].value), e[n].selected !== a && (e[n].selected = a), a && r && (e[n].defaultSelected = !0);
  } else {
    for (n = "" + en(n), t = null, a = 0; a < e.length; a++) {
      if (e[a].value === n) {
        e[a].selected = !0, r && (e[a].defaultSelected = !0);
        return;
      }
      t !== null || e[a].disabled || (t = e[a]);
    }
    t !== null && (t.selected = !0);
  }
}
function ni(e, t) {
  if (t.dangerouslySetInnerHTML != null) throw Error(I(91));
  return de({}, t, { value: void 0, defaultValue: void 0, children: "" + e._wrapperState.initialValue });
}
function ns(e, t) {
  var n = t.value;
  if (n == null) {
    if (n = t.children, t = t.defaultValue, n != null) {
      if (t != null) throw Error(I(92));
      if (fr(n)) {
        if (1 < n.length) throw Error(I(93));
        n = n[0];
      }
      t = n;
    }
    t == null && (t = ""), n = t;
  }
  e._wrapperState = { initialValue: en(n) };
}
function Ic(e, t) {
  var n = en(t.value), r = en(t.defaultValue);
  n != null && (n = "" + n, n !== e.value && (e.value = n), t.defaultValue == null && e.defaultValue !== n && (e.defaultValue = n)), r != null && (e.defaultValue = "" + r);
}
function rs(e) {
  var t = e.textContent;
  t === e._wrapperState.initialValue && t !== "" && t !== null && (e.value = t);
}
function Pc(e) {
  switch (e) {
    case "svg":
      return "http://www.w3.org/2000/svg";
    case "math":
      return "http://www.w3.org/1998/Math/MathML";
    default:
      return "http://www.w3.org/1999/xhtml";
  }
}
function ri(e, t) {
  return e == null || e === "http://www.w3.org/1999/xhtml" ? Pc(t) : e === "http://www.w3.org/2000/svg" && t === "foreignObject" ? "http://www.w3.org/1999/xhtml" : e;
}
var ea, Fc = function(e) {
  return typeof MSApp < "u" && MSApp.execUnsafeLocalFunction ? function(t, n, r, a) {
    MSApp.execUnsafeLocalFunction(function() {
      return e(t, n, r, a);
    });
  } : e;
}(function(e, t) {
  if (e.namespaceURI !== "http://www.w3.org/2000/svg" || "innerHTML" in e) e.innerHTML = t;
  else {
    for (ea = ea || document.createElement("div"), ea.innerHTML = "<svg>" + t.valueOf().toString() + "</svg>", t = ea.firstChild; e.firstChild; ) e.removeChild(e.firstChild);
    for (; t.firstChild; ) e.appendChild(t.firstChild);
  }
});
function Er(e, t) {
  if (t) {
    var n = e.firstChild;
    if (n && n === e.lastChild && n.nodeType === 3) {
      n.nodeValue = t;
      return;
    }
  }
  e.textContent = t;
}
var hr = {
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
}, uf = ["Webkit", "ms", "Moz", "O"];
Object.keys(hr).forEach(function(e) {
  uf.forEach(function(t) {
    t = t + e.charAt(0).toUpperCase() + e.substring(1), hr[t] = hr[e];
  });
});
function Rc(e, t, n) {
  return t == null || typeof t == "boolean" || t === "" ? "" : n || typeof t != "number" || t === 0 || hr.hasOwnProperty(e) && hr[e] ? ("" + t).trim() : t + "px";
}
function _c(e, t) {
  e = e.style;
  for (var n in t) if (t.hasOwnProperty(n)) {
    var r = n.indexOf("--") === 0, a = Rc(n, t[n], r);
    n === "float" && (n = "cssFloat"), r ? e.setProperty(n, a) : e[n] = a;
  }
}
var df = de({ menuitem: !0 }, { area: !0, base: !0, br: !0, col: !0, embed: !0, hr: !0, img: !0, input: !0, keygen: !0, link: !0, meta: !0, param: !0, source: !0, track: !0, wbr: !0 });
function ai(e, t) {
  if (t) {
    if (df[e] && (t.children != null || t.dangerouslySetInnerHTML != null)) throw Error(I(137, e));
    if (t.dangerouslySetInnerHTML != null) {
      if (t.children != null) throw Error(I(60));
      if (typeof t.dangerouslySetInnerHTML != "object" || !("__html" in t.dangerouslySetInnerHTML)) throw Error(I(61));
    }
    if (t.style != null && typeof t.style != "object") throw Error(I(62));
  }
}
function li(e, t) {
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
var ii = null;
function Xi(e) {
  return e = e.target || e.srcElement || window, e.correspondingUseElement && (e = e.correspondingUseElement), e.nodeType === 3 ? e.parentNode : e;
}
var oi = null, bn = null, Un = null;
function as(e) {
  if (e = Qr(e)) {
    if (typeof oi != "function") throw Error(I(280));
    var t = e.stateNode;
    t && (t = tl(t), oi(e.stateNode, e.type, t));
  }
}
function Tc(e) {
  bn ? Un ? Un.push(e) : Un = [e] : bn = e;
}
function zc() {
  if (bn) {
    var e = bn, t = Un;
    if (Un = bn = null, as(e), t) for (e = 0; e < t.length; e++) as(t[e]);
  }
}
function Dc(e, t) {
  return e(t);
}
function Lc() {
}
var wl = !1;
function Ac(e, t, n) {
  if (wl) return e(t, n);
  wl = !0;
  try {
    return Dc(e, t, n);
  } finally {
    wl = !1, (bn !== null || Un !== null) && (Lc(), zc());
  }
}
function Ir(e, t) {
  var n = e.stateNode;
  if (n === null) return null;
  var r = tl(n);
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
var si = !1;
if (Rt) try {
  var ar = {};
  Object.defineProperty(ar, "passive", { get: function() {
    si = !0;
  } }), window.addEventListener("test", ar, ar), window.removeEventListener("test", ar, ar);
} catch {
  si = !1;
}
function ff(e, t, n, r, a, i, o, s, c) {
  var d = Array.prototype.slice.call(arguments, 3);
  try {
    t.apply(n, d);
  } catch (N) {
    this.onError(N);
  }
}
var vr = !1, Ia = null, Pa = !1, ci = null, pf = { onError: function(e) {
  vr = !0, Ia = e;
} };
function mf(e, t, n, r, a, i, o, s, c) {
  vr = !1, Ia = null, ff.apply(pf, arguments);
}
function hf(e, t, n, r, a, i, o, s, c) {
  if (mf.apply(this, arguments), vr) {
    if (vr) {
      var d = Ia;
      vr = !1, Ia = null;
    } else throw Error(I(198));
    Pa || (Pa = !0, ci = d);
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
function Mc(e) {
  if (e.tag === 13) {
    var t = e.memoizedState;
    if (t === null && (e = e.alternate, e !== null && (t = e.memoizedState)), t !== null) return t.dehydrated;
  }
  return null;
}
function ls(e) {
  if (wn(e) !== e) throw Error(I(188));
}
function vf(e) {
  var t = e.alternate;
  if (!t) {
    if (t = wn(e), t === null) throw Error(I(188));
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
        if (i === n) return ls(a), e;
        if (i === r) return ls(a), t;
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
function $c(e) {
  return e = vf(e), e !== null ? Oc(e) : null;
}
function Oc(e) {
  if (e.tag === 5 || e.tag === 6) return e;
  for (e = e.child; e !== null; ) {
    var t = Oc(e);
    if (t !== null) return t;
    e = e.sibling;
  }
  return null;
}
var bc = Je.unstable_scheduleCallback, is = Je.unstable_cancelCallback, xf = Je.unstable_shouldYield, gf = Je.unstable_requestPaint, ve = Je.unstable_now, yf = Je.unstable_getCurrentPriorityLevel, Zi = Je.unstable_ImmediatePriority, Uc = Je.unstable_UserBlockingPriority, Fa = Je.unstable_NormalPriority, jf = Je.unstable_LowPriority, Vc = Je.unstable_IdlePriority, Xa = null, St = null;
function Nf(e) {
  if (St && typeof St.onCommitFiberRoot == "function") try {
    St.onCommitFiberRoot(Xa, e, void 0, (e.current.flags & 128) === 128);
  } catch {
  }
}
var mt = Math.clz32 ? Math.clz32 : kf, Sf = Math.log, wf = Math.LN2;
function kf(e) {
  return e >>>= 0, e === 0 ? 32 : 31 - (Sf(e) / wf | 0) | 0;
}
var ta = 64, na = 4194304;
function pr(e) {
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
function Ra(e, t) {
  var n = e.pendingLanes;
  if (n === 0) return 0;
  var r = 0, a = e.suspendedLanes, i = e.pingedLanes, o = n & 268435455;
  if (o !== 0) {
    var s = o & ~a;
    s !== 0 ? r = pr(s) : (i &= o, i !== 0 && (r = pr(i)));
  } else o = n & ~a, o !== 0 ? r = pr(o) : i !== 0 && (r = pr(i));
  if (r === 0) return 0;
  if (t !== 0 && t !== r && !(t & a) && (a = r & -r, i = t & -t, a >= i || a === 16 && (i & 4194240) !== 0)) return t;
  if (r & 4 && (r |= n & 16), t = e.entangledLanes, t !== 0) for (e = e.entanglements, t &= r; 0 < t; ) n = 31 - mt(t), a = 1 << n, r |= e[n], t &= ~a;
  return r;
}
function Cf(e, t) {
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
function Ef(e, t) {
  for (var n = e.suspendedLanes, r = e.pingedLanes, a = e.expirationTimes, i = e.pendingLanes; 0 < i; ) {
    var o = 31 - mt(i), s = 1 << o, c = a[o];
    c === -1 ? (!(s & n) || s & r) && (a[o] = Cf(s, t)) : c <= t && (e.expiredLanes |= s), i &= ~s;
  }
}
function ui(e) {
  return e = e.pendingLanes & -1073741825, e !== 0 ? e : e & 1073741824 ? 1073741824 : 0;
}
function Bc() {
  var e = ta;
  return ta <<= 1, !(ta & 4194240) && (ta = 64), e;
}
function kl(e) {
  for (var t = [], n = 0; 31 > n; n++) t.push(e);
  return t;
}
function Hr(e, t, n) {
  e.pendingLanes |= t, t !== 536870912 && (e.suspendedLanes = 0, e.pingedLanes = 0), e = e.eventTimes, t = 31 - mt(t), e[t] = n;
}
function If(e, t) {
  var n = e.pendingLanes & ~t;
  e.pendingLanes = t, e.suspendedLanes = 0, e.pingedLanes = 0, e.expiredLanes &= t, e.mutableReadLanes &= t, e.entangledLanes &= t, t = e.entanglements;
  var r = e.eventTimes;
  for (e = e.expirationTimes; 0 < n; ) {
    var a = 31 - mt(n), i = 1 << a;
    t[a] = 0, r[a] = -1, e[a] = -1, n &= ~i;
  }
}
function Ji(e, t) {
  var n = e.entangledLanes |= t;
  for (e = e.entanglements; n; ) {
    var r = 31 - mt(n), a = 1 << r;
    a & t | e[r] & t && (e[r] |= t), n &= ~a;
  }
}
var ee = 0;
function Hc(e) {
  return e &= -e, 1 < e ? 4 < e ? e & 268435455 ? 16 : 536870912 : 4 : 1;
}
var Wc, eo, Qc, Gc, Kc, di = !1, ra = [], Qt = null, Gt = null, Kt = null, Pr = /* @__PURE__ */ new Map(), Fr = /* @__PURE__ */ new Map(), Vt = [], Pf = "mousedown mouseup touchcancel touchend touchstart auxclick dblclick pointercancel pointerdown pointerup dragend dragstart drop compositionend compositionstart keydown keypress keyup input textInput copy cut paste click change contextmenu reset submit".split(" ");
function os(e, t) {
  switch (e) {
    case "focusin":
    case "focusout":
      Qt = null;
      break;
    case "dragenter":
    case "dragleave":
      Gt = null;
      break;
    case "mouseover":
    case "mouseout":
      Kt = null;
      break;
    case "pointerover":
    case "pointerout":
      Pr.delete(t.pointerId);
      break;
    case "gotpointercapture":
    case "lostpointercapture":
      Fr.delete(t.pointerId);
  }
}
function lr(e, t, n, r, a, i) {
  return e === null || e.nativeEvent !== i ? (e = { blockedOn: t, domEventName: n, eventSystemFlags: r, nativeEvent: i, targetContainers: [a] }, t !== null && (t = Qr(t), t !== null && eo(t)), e) : (e.eventSystemFlags |= r, t = e.targetContainers, a !== null && t.indexOf(a) === -1 && t.push(a), e);
}
function Ff(e, t, n, r, a) {
  switch (t) {
    case "focusin":
      return Qt = lr(Qt, e, t, n, r, a), !0;
    case "dragenter":
      return Gt = lr(Gt, e, t, n, r, a), !0;
    case "mouseover":
      return Kt = lr(Kt, e, t, n, r, a), !0;
    case "pointerover":
      var i = a.pointerId;
      return Pr.set(i, lr(Pr.get(i) || null, e, t, n, r, a)), !0;
    case "gotpointercapture":
      return i = a.pointerId, Fr.set(i, lr(Fr.get(i) || null, e, t, n, r, a)), !0;
  }
  return !1;
}
function qc(e) {
  var t = dn(e.target);
  if (t !== null) {
    var n = wn(t);
    if (n !== null) {
      if (t = n.tag, t === 13) {
        if (t = Mc(n), t !== null) {
          e.blockedOn = t, Kc(e.priority, function() {
            Qc(n);
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
function va(e) {
  if (e.blockedOn !== null) return !1;
  for (var t = e.targetContainers; 0 < t.length; ) {
    var n = fi(e.domEventName, e.eventSystemFlags, t[0], e.nativeEvent);
    if (n === null) {
      n = e.nativeEvent;
      var r = new n.constructor(n.type, n);
      ii = r, n.target.dispatchEvent(r), ii = null;
    } else return t = Qr(n), t !== null && eo(t), e.blockedOn = n, !1;
    t.shift();
  }
  return !0;
}
function ss(e, t, n) {
  va(e) && n.delete(t);
}
function Rf() {
  di = !1, Qt !== null && va(Qt) && (Qt = null), Gt !== null && va(Gt) && (Gt = null), Kt !== null && va(Kt) && (Kt = null), Pr.forEach(ss), Fr.forEach(ss);
}
function ir(e, t) {
  e.blockedOn === t && (e.blockedOn = null, di || (di = !0, Je.unstable_scheduleCallback(Je.unstable_NormalPriority, Rf)));
}
function Rr(e) {
  function t(a) {
    return ir(a, e);
  }
  if (0 < ra.length) {
    ir(ra[0], e);
    for (var n = 1; n < ra.length; n++) {
      var r = ra[n];
      r.blockedOn === e && (r.blockedOn = null);
    }
  }
  for (Qt !== null && ir(Qt, e), Gt !== null && ir(Gt, e), Kt !== null && ir(Kt, e), Pr.forEach(t), Fr.forEach(t), n = 0; n < Vt.length; n++) r = Vt[n], r.blockedOn === e && (r.blockedOn = null);
  for (; 0 < Vt.length && (n = Vt[0], n.blockedOn === null); ) qc(n), n.blockedOn === null && Vt.shift();
}
var Vn = Dt.ReactCurrentBatchConfig, _a = !0;
function _f(e, t, n, r) {
  var a = ee, i = Vn.transition;
  Vn.transition = null;
  try {
    ee = 1, to(e, t, n, r);
  } finally {
    ee = a, Vn.transition = i;
  }
}
function Tf(e, t, n, r) {
  var a = ee, i = Vn.transition;
  Vn.transition = null;
  try {
    ee = 4, to(e, t, n, r);
  } finally {
    ee = a, Vn.transition = i;
  }
}
function to(e, t, n, r) {
  if (_a) {
    var a = fi(e, t, n, r);
    if (a === null) Dl(e, t, r, Ta, n), os(e, r);
    else if (Ff(a, e, t, n, r)) r.stopPropagation();
    else if (os(e, r), t & 4 && -1 < Pf.indexOf(e)) {
      for (; a !== null; ) {
        var i = Qr(a);
        if (i !== null && Wc(i), i = fi(e, t, n, r), i === null && Dl(e, t, r, Ta, n), i === a) break;
        a = i;
      }
      a !== null && r.stopPropagation();
    } else Dl(e, t, r, null, n);
  }
}
var Ta = null;
function fi(e, t, n, r) {
  if (Ta = null, e = Xi(r), e = dn(e), e !== null) if (t = wn(e), t === null) e = null;
  else if (n = t.tag, n === 13) {
    if (e = Mc(t), e !== null) return e;
    e = null;
  } else if (n === 3) {
    if (t.stateNode.current.memoizedState.isDehydrated) return t.tag === 3 ? t.stateNode.containerInfo : null;
    e = null;
  } else t !== e && (e = null);
  return Ta = e, null;
}
function Yc(e) {
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
      switch (yf()) {
        case Zi:
          return 1;
        case Uc:
          return 4;
        case Fa:
        case jf:
          return 16;
        case Vc:
          return 536870912;
        default:
          return 16;
      }
    default:
      return 16;
  }
}
var Ht = null, no = null, xa = null;
function Xc() {
  if (xa) return xa;
  var e, t = no, n = t.length, r, a = "value" in Ht ? Ht.value : Ht.textContent, i = a.length;
  for (e = 0; e < n && t[e] === a[e]; e++) ;
  var o = n - e;
  for (r = 1; r <= o && t[n - r] === a[i - r]; r++) ;
  return xa = a.slice(e, 1 < r ? 1 - r : void 0);
}
function ga(e) {
  var t = e.keyCode;
  return "charCode" in e ? (e = e.charCode, e === 0 && t === 13 && (e = 13)) : e = t, e === 10 && (e = 13), 32 <= e || e === 13 ? e : 0;
}
function aa() {
  return !0;
}
function cs() {
  return !1;
}
function tt(e) {
  function t(n, r, a, i, o) {
    this._reactName = n, this._targetInst = a, this.type = r, this.nativeEvent = i, this.target = o, this.currentTarget = null;
    for (var s in e) e.hasOwnProperty(s) && (n = e[s], this[s] = n ? n(i) : i[s]);
    return this.isDefaultPrevented = (i.defaultPrevented != null ? i.defaultPrevented : i.returnValue === !1) ? aa : cs, this.isPropagationStopped = cs, this;
  }
  return de(t.prototype, { preventDefault: function() {
    this.defaultPrevented = !0;
    var n = this.nativeEvent;
    n && (n.preventDefault ? n.preventDefault() : typeof n.returnValue != "unknown" && (n.returnValue = !1), this.isDefaultPrevented = aa);
  }, stopPropagation: function() {
    var n = this.nativeEvent;
    n && (n.stopPropagation ? n.stopPropagation() : typeof n.cancelBubble != "unknown" && (n.cancelBubble = !0), this.isPropagationStopped = aa);
  }, persist: function() {
  }, isPersistent: aa }), t;
}
var er = { eventPhase: 0, bubbles: 0, cancelable: 0, timeStamp: function(e) {
  return e.timeStamp || Date.now();
}, defaultPrevented: 0, isTrusted: 0 }, ro = tt(er), Wr = de({}, er, { view: 0, detail: 0 }), zf = tt(Wr), Cl, El, or, Za = de({}, Wr, { screenX: 0, screenY: 0, clientX: 0, clientY: 0, pageX: 0, pageY: 0, ctrlKey: 0, shiftKey: 0, altKey: 0, metaKey: 0, getModifierState: ao, button: 0, buttons: 0, relatedTarget: function(e) {
  return e.relatedTarget === void 0 ? e.fromElement === e.srcElement ? e.toElement : e.fromElement : e.relatedTarget;
}, movementX: function(e) {
  return "movementX" in e ? e.movementX : (e !== or && (or && e.type === "mousemove" ? (Cl = e.screenX - or.screenX, El = e.screenY - or.screenY) : El = Cl = 0, or = e), Cl);
}, movementY: function(e) {
  return "movementY" in e ? e.movementY : El;
} }), us = tt(Za), Df = de({}, Za, { dataTransfer: 0 }), Lf = tt(Df), Af = de({}, Wr, { relatedTarget: 0 }), Il = tt(Af), Mf = de({}, er, { animationName: 0, elapsedTime: 0, pseudoElement: 0 }), $f = tt(Mf), Of = de({}, er, { clipboardData: function(e) {
  return "clipboardData" in e ? e.clipboardData : window.clipboardData;
} }), bf = tt(Of), Uf = de({}, er, { data: 0 }), ds = tt(Uf), Vf = {
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
}, Bf = {
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
}, Hf = { Alt: "altKey", Control: "ctrlKey", Meta: "metaKey", Shift: "shiftKey" };
function Wf(e) {
  var t = this.nativeEvent;
  return t.getModifierState ? t.getModifierState(e) : (e = Hf[e]) ? !!t[e] : !1;
}
function ao() {
  return Wf;
}
var Qf = de({}, Wr, { key: function(e) {
  if (e.key) {
    var t = Vf[e.key] || e.key;
    if (t !== "Unidentified") return t;
  }
  return e.type === "keypress" ? (e = ga(e), e === 13 ? "Enter" : String.fromCharCode(e)) : e.type === "keydown" || e.type === "keyup" ? Bf[e.keyCode] || "Unidentified" : "";
}, code: 0, location: 0, ctrlKey: 0, shiftKey: 0, altKey: 0, metaKey: 0, repeat: 0, locale: 0, getModifierState: ao, charCode: function(e) {
  return e.type === "keypress" ? ga(e) : 0;
}, keyCode: function(e) {
  return e.type === "keydown" || e.type === "keyup" ? e.keyCode : 0;
}, which: function(e) {
  return e.type === "keypress" ? ga(e) : e.type === "keydown" || e.type === "keyup" ? e.keyCode : 0;
} }), Gf = tt(Qf), Kf = de({}, Za, { pointerId: 0, width: 0, height: 0, pressure: 0, tangentialPressure: 0, tiltX: 0, tiltY: 0, twist: 0, pointerType: 0, isPrimary: 0 }), fs = tt(Kf), qf = de({}, Wr, { touches: 0, targetTouches: 0, changedTouches: 0, altKey: 0, metaKey: 0, ctrlKey: 0, shiftKey: 0, getModifierState: ao }), Yf = tt(qf), Xf = de({}, er, { propertyName: 0, elapsedTime: 0, pseudoElement: 0 }), Zf = tt(Xf), Jf = de({}, Za, {
  deltaX: function(e) {
    return "deltaX" in e ? e.deltaX : "wheelDeltaX" in e ? -e.wheelDeltaX : 0;
  },
  deltaY: function(e) {
    return "deltaY" in e ? e.deltaY : "wheelDeltaY" in e ? -e.wheelDeltaY : "wheelDelta" in e ? -e.wheelDelta : 0;
  },
  deltaZ: 0,
  deltaMode: 0
}), ep = tt(Jf), tp = [9, 13, 27, 32], lo = Rt && "CompositionEvent" in window, xr = null;
Rt && "documentMode" in document && (xr = document.documentMode);
var np = Rt && "TextEvent" in window && !xr, Zc = Rt && (!lo || xr && 8 < xr && 11 >= xr), ps = " ", ms = !1;
function Jc(e, t) {
  switch (e) {
    case "keyup":
      return tp.indexOf(t.keyCode) !== -1;
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
function eu(e) {
  return e = e.detail, typeof e == "object" && "data" in e ? e.data : null;
}
var Fn = !1;
function rp(e, t) {
  switch (e) {
    case "compositionend":
      return eu(t);
    case "keypress":
      return t.which !== 32 ? null : (ms = !0, ps);
    case "textInput":
      return e = t.data, e === ps && ms ? null : e;
    default:
      return null;
  }
}
function ap(e, t) {
  if (Fn) return e === "compositionend" || !lo && Jc(e, t) ? (e = Xc(), xa = no = Ht = null, Fn = !1, e) : null;
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
      return Zc && t.locale !== "ko" ? null : t.data;
    default:
      return null;
  }
}
var lp = { color: !0, date: !0, datetime: !0, "datetime-local": !0, email: !0, month: !0, number: !0, password: !0, range: !0, search: !0, tel: !0, text: !0, time: !0, url: !0, week: !0 };
function hs(e) {
  var t = e && e.nodeName && e.nodeName.toLowerCase();
  return t === "input" ? !!lp[e.type] : t === "textarea";
}
function tu(e, t, n, r) {
  Tc(r), t = za(t, "onChange"), 0 < t.length && (n = new ro("onChange", "change", null, n, r), e.push({ event: n, listeners: t }));
}
var gr = null, _r = null;
function ip(e) {
  fu(e, 0);
}
function Ja(e) {
  var t = Tn(e);
  if (Cc(t)) return e;
}
function op(e, t) {
  if (e === "change") return t;
}
var nu = !1;
if (Rt) {
  var Pl;
  if (Rt) {
    var Fl = "oninput" in document;
    if (!Fl) {
      var vs = document.createElement("div");
      vs.setAttribute("oninput", "return;"), Fl = typeof vs.oninput == "function";
    }
    Pl = Fl;
  } else Pl = !1;
  nu = Pl && (!document.documentMode || 9 < document.documentMode);
}
function xs() {
  gr && (gr.detachEvent("onpropertychange", ru), _r = gr = null);
}
function ru(e) {
  if (e.propertyName === "value" && Ja(_r)) {
    var t = [];
    tu(t, _r, e, Xi(e)), Ac(ip, t);
  }
}
function sp(e, t, n) {
  e === "focusin" ? (xs(), gr = t, _r = n, gr.attachEvent("onpropertychange", ru)) : e === "focusout" && xs();
}
function cp(e) {
  if (e === "selectionchange" || e === "keyup" || e === "keydown") return Ja(_r);
}
function up(e, t) {
  if (e === "click") return Ja(t);
}
function dp(e, t) {
  if (e === "input" || e === "change") return Ja(t);
}
function fp(e, t) {
  return e === t && (e !== 0 || 1 / e === 1 / t) || e !== e && t !== t;
}
var xt = typeof Object.is == "function" ? Object.is : fp;
function Tr(e, t) {
  if (xt(e, t)) return !0;
  if (typeof e != "object" || e === null || typeof t != "object" || t === null) return !1;
  var n = Object.keys(e), r = Object.keys(t);
  if (n.length !== r.length) return !1;
  for (r = 0; r < n.length; r++) {
    var a = n[r];
    if (!Kl.call(t, a) || !xt(e[a], t[a])) return !1;
  }
  return !0;
}
function gs(e) {
  for (; e && e.firstChild; ) e = e.firstChild;
  return e;
}
function ys(e, t) {
  var n = gs(e);
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
    n = gs(n);
  }
}
function au(e, t) {
  return e && t ? e === t ? !0 : e && e.nodeType === 3 ? !1 : t && t.nodeType === 3 ? au(e, t.parentNode) : "contains" in e ? e.contains(t) : e.compareDocumentPosition ? !!(e.compareDocumentPosition(t) & 16) : !1 : !1;
}
function lu() {
  for (var e = window, t = Ea(); t instanceof e.HTMLIFrameElement; ) {
    try {
      var n = typeof t.contentWindow.location.href == "string";
    } catch {
      n = !1;
    }
    if (n) e = t.contentWindow;
    else break;
    t = Ea(e.document);
  }
  return t;
}
function io(e) {
  var t = e && e.nodeName && e.nodeName.toLowerCase();
  return t && (t === "input" && (e.type === "text" || e.type === "search" || e.type === "tel" || e.type === "url" || e.type === "password") || t === "textarea" || e.contentEditable === "true");
}
function pp(e) {
  var t = lu(), n = e.focusedElem, r = e.selectionRange;
  if (t !== n && n && n.ownerDocument && au(n.ownerDocument.documentElement, n)) {
    if (r !== null && io(n)) {
      if (t = r.start, e = r.end, e === void 0 && (e = t), "selectionStart" in n) n.selectionStart = t, n.selectionEnd = Math.min(e, n.value.length);
      else if (e = (t = n.ownerDocument || document) && t.defaultView || window, e.getSelection) {
        e = e.getSelection();
        var a = n.textContent.length, i = Math.min(r.start, a);
        r = r.end === void 0 ? i : Math.min(r.end, a), !e.extend && i > r && (a = r, r = i, i = a), a = ys(n, i);
        var o = ys(
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
var mp = Rt && "documentMode" in document && 11 >= document.documentMode, Rn = null, pi = null, yr = null, mi = !1;
function js(e, t, n) {
  var r = n.window === n ? n.document : n.nodeType === 9 ? n : n.ownerDocument;
  mi || Rn == null || Rn !== Ea(r) || (r = Rn, "selectionStart" in r && io(r) ? r = { start: r.selectionStart, end: r.selectionEnd } : (r = (r.ownerDocument && r.ownerDocument.defaultView || window).getSelection(), r = { anchorNode: r.anchorNode, anchorOffset: r.anchorOffset, focusNode: r.focusNode, focusOffset: r.focusOffset }), yr && Tr(yr, r) || (yr = r, r = za(pi, "onSelect"), 0 < r.length && (t = new ro("onSelect", "select", null, t, n), e.push({ event: t, listeners: r }), t.target = Rn)));
}
function la(e, t) {
  var n = {};
  return n[e.toLowerCase()] = t.toLowerCase(), n["Webkit" + e] = "webkit" + t, n["Moz" + e] = "moz" + t, n;
}
var _n = { animationend: la("Animation", "AnimationEnd"), animationiteration: la("Animation", "AnimationIteration"), animationstart: la("Animation", "AnimationStart"), transitionend: la("Transition", "TransitionEnd") }, Rl = {}, iu = {};
Rt && (iu = document.createElement("div").style, "AnimationEvent" in window || (delete _n.animationend.animation, delete _n.animationiteration.animation, delete _n.animationstart.animation), "TransitionEvent" in window || delete _n.transitionend.transition);
function el(e) {
  if (Rl[e]) return Rl[e];
  if (!_n[e]) return e;
  var t = _n[e], n;
  for (n in t) if (t.hasOwnProperty(n) && n in iu) return Rl[e] = t[n];
  return e;
}
var ou = el("animationend"), su = el("animationiteration"), cu = el("animationstart"), uu = el("transitionend"), du = /* @__PURE__ */ new Map(), Ns = "abort auxClick cancel canPlay canPlayThrough click close contextMenu copy cut drag dragEnd dragEnter dragExit dragLeave dragOver dragStart drop durationChange emptied encrypted ended error gotPointerCapture input invalid keyDown keyPress keyUp load loadedData loadedMetadata loadStart lostPointerCapture mouseDown mouseMove mouseOut mouseOver mouseUp paste pause play playing pointerCancel pointerDown pointerMove pointerOut pointerOver pointerUp progress rateChange reset resize seeked seeking stalled submit suspend timeUpdate touchCancel touchEnd touchStart volumeChange scroll toggle touchMove waiting wheel".split(" ");
function rn(e, t) {
  du.set(e, t), Sn(t, [e]);
}
for (var _l = 0; _l < Ns.length; _l++) {
  var Tl = Ns[_l], hp = Tl.toLowerCase(), vp = Tl[0].toUpperCase() + Tl.slice(1);
  rn(hp, "on" + vp);
}
rn(ou, "onAnimationEnd");
rn(su, "onAnimationIteration");
rn(cu, "onAnimationStart");
rn("dblclick", "onDoubleClick");
rn("focusin", "onFocus");
rn("focusout", "onBlur");
rn(uu, "onTransitionEnd");
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
var mr = "abort canplay canplaythrough durationchange emptied encrypted ended error loadeddata loadedmetadata loadstart pause play playing progress ratechange resize seeked seeking stalled suspend timeupdate volumechange waiting".split(" "), xp = new Set("cancel close invalid load scroll toggle".split(" ").concat(mr));
function Ss(e, t, n) {
  var r = e.type || "unknown-event";
  e.currentTarget = n, hf(r, t, void 0, e), e.currentTarget = null;
}
function fu(e, t) {
  t = (t & 4) !== 0;
  for (var n = 0; n < e.length; n++) {
    var r = e[n], a = r.event;
    r = r.listeners;
    e: {
      var i = void 0;
      if (t) for (var o = r.length - 1; 0 <= o; o--) {
        var s = r[o], c = s.instance, d = s.currentTarget;
        if (s = s.listener, c !== i && a.isPropagationStopped()) break e;
        Ss(a, s, d), i = c;
      }
      else for (o = 0; o < r.length; o++) {
        if (s = r[o], c = s.instance, d = s.currentTarget, s = s.listener, c !== i && a.isPropagationStopped()) break e;
        Ss(a, s, d), i = c;
      }
    }
  }
  if (Pa) throw e = ci, Pa = !1, ci = null, e;
}
function le(e, t) {
  var n = t[yi];
  n === void 0 && (n = t[yi] = /* @__PURE__ */ new Set());
  var r = e + "__bubble";
  n.has(r) || (pu(t, e, 2, !1), n.add(r));
}
function zl(e, t, n) {
  var r = 0;
  t && (r |= 4), pu(n, e, r, t);
}
var ia = "_reactListening" + Math.random().toString(36).slice(2);
function zr(e) {
  if (!e[ia]) {
    e[ia] = !0, jc.forEach(function(n) {
      n !== "selectionchange" && (xp.has(n) || zl(n, !1, e), zl(n, !0, e));
    });
    var t = e.nodeType === 9 ? e : e.ownerDocument;
    t === null || t[ia] || (t[ia] = !0, zl("selectionchange", !1, t));
  }
}
function pu(e, t, n, r) {
  switch (Yc(t)) {
    case 1:
      var a = _f;
      break;
    case 4:
      a = Tf;
      break;
    default:
      a = to;
  }
  n = a.bind(null, t, n, e), a = void 0, !si || t !== "touchstart" && t !== "touchmove" && t !== "wheel" || (a = !0), r ? a !== void 0 ? e.addEventListener(t, n, { capture: !0, passive: a }) : e.addEventListener(t, n, !0) : a !== void 0 ? e.addEventListener(t, n, { passive: a }) : e.addEventListener(t, n, !1);
}
function Dl(e, t, n, r, a) {
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
  Ac(function() {
    var d = i, N = Xi(n), u = [];
    e: {
      var h = du.get(e);
      if (h !== void 0) {
        var v = ro, y = e;
        switch (e) {
          case "keypress":
            if (ga(n) === 0) break e;
          case "keydown":
          case "keyup":
            v = Gf;
            break;
          case "focusin":
            y = "focus", v = Il;
            break;
          case "focusout":
            y = "blur", v = Il;
            break;
          case "beforeblur":
          case "afterblur":
            v = Il;
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
            v = us;
            break;
          case "drag":
          case "dragend":
          case "dragenter":
          case "dragexit":
          case "dragleave":
          case "dragover":
          case "dragstart":
          case "drop":
            v = Lf;
            break;
          case "touchcancel":
          case "touchend":
          case "touchmove":
          case "touchstart":
            v = Yf;
            break;
          case ou:
          case su:
          case cu:
            v = $f;
            break;
          case uu:
            v = Zf;
            break;
          case "scroll":
            v = zf;
            break;
          case "wheel":
            v = ep;
            break;
          case "copy":
          case "cut":
          case "paste":
            v = bf;
            break;
          case "gotpointercapture":
          case "lostpointercapture":
          case "pointercancel":
          case "pointerdown":
          case "pointermove":
          case "pointerout":
          case "pointerover":
          case "pointerup":
            v = fs;
        }
        var S = (t & 4) !== 0, O = !S && e === "scroll", p = S ? h !== null ? h + "Capture" : null : h;
        S = [];
        for (var f = d, x; f !== null; ) {
          x = f;
          var C = x.stateNode;
          if (x.tag === 5 && C !== null && (x = C, p !== null && (C = Ir(f, p), C != null && S.push(Dr(f, C, x)))), O) break;
          f = f.return;
        }
        0 < S.length && (h = new v(h, y, null, n, N), u.push({ event: h, listeners: S }));
      }
    }
    if (!(t & 7)) {
      e: {
        if (h = e === "mouseover" || e === "pointerover", v = e === "mouseout" || e === "pointerout", h && n !== ii && (y = n.relatedTarget || n.fromElement) && (dn(y) || y[_t])) break e;
        if ((v || h) && (h = N.window === N ? N : (h = N.ownerDocument) ? h.defaultView || h.parentWindow : window, v ? (y = n.relatedTarget || n.toElement, v = d, y = y ? dn(y) : null, y !== null && (O = wn(y), y !== O || y.tag !== 5 && y.tag !== 6) && (y = null)) : (v = null, y = d), v !== y)) {
          if (S = us, C = "onMouseLeave", p = "onMouseEnter", f = "mouse", (e === "pointerout" || e === "pointerover") && (S = fs, C = "onPointerLeave", p = "onPointerEnter", f = "pointer"), O = v == null ? h : Tn(v), x = y == null ? h : Tn(y), h = new S(C, f + "leave", v, n, N), h.target = O, h.relatedTarget = x, C = null, dn(N) === d && (S = new S(p, f + "enter", y, n, N), S.target = x, S.relatedTarget = O, C = S), O = C, v && y) t: {
            for (S = v, p = y, f = 0, x = S; x; x = En(x)) f++;
            for (x = 0, C = p; C; C = En(C)) x++;
            for (; 0 < f - x; ) S = En(S), f--;
            for (; 0 < x - f; ) p = En(p), x--;
            for (; f--; ) {
              if (S === p || p !== null && S === p.alternate) break t;
              S = En(S), p = En(p);
            }
            S = null;
          }
          else S = null;
          v !== null && ws(u, h, v, S, !1), y !== null && O !== null && ws(u, O, y, S, !0);
        }
      }
      e: {
        if (h = d ? Tn(d) : window, v = h.nodeName && h.nodeName.toLowerCase(), v === "select" || v === "input" && h.type === "file") var _ = op;
        else if (hs(h)) if (nu) _ = dp;
        else {
          _ = cp;
          var F = sp;
        }
        else (v = h.nodeName) && v.toLowerCase() === "input" && (h.type === "checkbox" || h.type === "radio") && (_ = up);
        if (_ && (_ = _(e, d))) {
          tu(u, _, n, N);
          break e;
        }
        F && F(e, h, d), e === "focusout" && (F = h._wrapperState) && F.controlled && h.type === "number" && ti(h, "number", h.value);
      }
      switch (F = d ? Tn(d) : window, e) {
        case "focusin":
          (hs(F) || F.contentEditable === "true") && (Rn = F, pi = d, yr = null);
          break;
        case "focusout":
          yr = pi = Rn = null;
          break;
        case "mousedown":
          mi = !0;
          break;
        case "contextmenu":
        case "mouseup":
        case "dragend":
          mi = !1, js(u, n, N);
          break;
        case "selectionchange":
          if (mp) break;
        case "keydown":
        case "keyup":
          js(u, n, N);
      }
      var z;
      if (lo) e: {
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
      else Fn ? Jc(e, n) && (k = "onCompositionEnd") : e === "keydown" && n.keyCode === 229 && (k = "onCompositionStart");
      k && (Zc && n.locale !== "ko" && (Fn || k !== "onCompositionStart" ? k === "onCompositionEnd" && Fn && (z = Xc()) : (Ht = N, no = "value" in Ht ? Ht.value : Ht.textContent, Fn = !0)), F = za(d, k), 0 < F.length && (k = new ds(k, e, null, n, N), u.push({ event: k, listeners: F }), z ? k.data = z : (z = eu(n), z !== null && (k.data = z)))), (z = np ? rp(e, n) : ap(e, n)) && (d = za(d, "onBeforeInput"), 0 < d.length && (N = new ds("onBeforeInput", "beforeinput", null, n, N), u.push({ event: N, listeners: d }), N.data = z));
    }
    fu(u, t);
  });
}
function Dr(e, t, n) {
  return { instance: e, listener: t, currentTarget: n };
}
function za(e, t) {
  for (var n = t + "Capture", r = []; e !== null; ) {
    var a = e, i = a.stateNode;
    a.tag === 5 && i !== null && (a = i, i = Ir(e, n), i != null && r.unshift(Dr(e, i, a)), i = Ir(e, t), i != null && r.push(Dr(e, i, a))), e = e.return;
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
function ws(e, t, n, r, a) {
  for (var i = t._reactName, o = []; n !== null && n !== r; ) {
    var s = n, c = s.alternate, d = s.stateNode;
    if (c !== null && c === r) break;
    s.tag === 5 && d !== null && (s = d, a ? (c = Ir(n, i), c != null && o.unshift(Dr(n, c, s))) : a || (c = Ir(n, i), c != null && o.push(Dr(n, c, s)))), n = n.return;
  }
  o.length !== 0 && e.push({ event: t, listeners: o });
}
var gp = /\r\n?/g, yp = /\u0000|\uFFFD/g;
function ks(e) {
  return (typeof e == "string" ? e : "" + e).replace(gp, `
`).replace(yp, "");
}
function oa(e, t, n) {
  if (t = ks(t), ks(e) !== t && n) throw Error(I(425));
}
function Da() {
}
var hi = null, vi = null;
function xi(e, t) {
  return e === "textarea" || e === "noscript" || typeof t.children == "string" || typeof t.children == "number" || typeof t.dangerouslySetInnerHTML == "object" && t.dangerouslySetInnerHTML !== null && t.dangerouslySetInnerHTML.__html != null;
}
var gi = typeof setTimeout == "function" ? setTimeout : void 0, jp = typeof clearTimeout == "function" ? clearTimeout : void 0, Cs = typeof Promise == "function" ? Promise : void 0, Np = typeof queueMicrotask == "function" ? queueMicrotask : typeof Cs < "u" ? function(e) {
  return Cs.resolve(null).then(e).catch(Sp);
} : gi;
function Sp(e) {
  setTimeout(function() {
    throw e;
  });
}
function Ll(e, t) {
  var n = t, r = 0;
  do {
    var a = n.nextSibling;
    if (e.removeChild(n), a && a.nodeType === 8) if (n = a.data, n === "/$") {
      if (r === 0) {
        e.removeChild(a), Rr(t);
        return;
      }
      r--;
    } else n !== "$" && n !== "$?" && n !== "$!" || r++;
    n = a;
  } while (n);
  Rr(t);
}
function qt(e) {
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
function Es(e) {
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
var tr = Math.random().toString(36).slice(2), Nt = "__reactFiber$" + tr, Lr = "__reactProps$" + tr, _t = "__reactContainer$" + tr, yi = "__reactEvents$" + tr, wp = "__reactListeners$" + tr, kp = "__reactHandles$" + tr;
function dn(e) {
  var t = e[Nt];
  if (t) return t;
  for (var n = e.parentNode; n; ) {
    if (t = n[_t] || n[Nt]) {
      if (n = t.alternate, t.child !== null || n !== null && n.child !== null) for (e = Es(e); e !== null; ) {
        if (n = e[Nt]) return n;
        e = Es(e);
      }
      return t;
    }
    e = n, n = e.parentNode;
  }
  return null;
}
function Qr(e) {
  return e = e[Nt] || e[_t], !e || e.tag !== 5 && e.tag !== 6 && e.tag !== 13 && e.tag !== 3 ? null : e;
}
function Tn(e) {
  if (e.tag === 5 || e.tag === 6) return e.stateNode;
  throw Error(I(33));
}
function tl(e) {
  return e[Lr] || null;
}
var ji = [], zn = -1;
function an(e) {
  return { current: e };
}
function ie(e) {
  0 > zn || (e.current = ji[zn], ji[zn] = null, zn--);
}
function re(e, t) {
  zn++, ji[zn] = e.current, e.current = t;
}
var tn = {}, ze = an(tn), He = an(!1), xn = tn;
function Gn(e, t) {
  var n = e.type.contextTypes;
  if (!n) return tn;
  var r = e.stateNode;
  if (r && r.__reactInternalMemoizedUnmaskedChildContext === t) return r.__reactInternalMemoizedMaskedChildContext;
  var a = {}, i;
  for (i in n) a[i] = t[i];
  return r && (e = e.stateNode, e.__reactInternalMemoizedUnmaskedChildContext = t, e.__reactInternalMemoizedMaskedChildContext = a), a;
}
function We(e) {
  return e = e.childContextTypes, e != null;
}
function La() {
  ie(He), ie(ze);
}
function Is(e, t, n) {
  if (ze.current !== tn) throw Error(I(168));
  re(ze, t), re(He, n);
}
function mu(e, t, n) {
  var r = e.stateNode;
  if (t = t.childContextTypes, typeof r.getChildContext != "function") return n;
  r = r.getChildContext();
  for (var a in r) if (!(a in t)) throw Error(I(108, sf(e) || "Unknown", a));
  return de({}, n, r);
}
function Aa(e) {
  return e = (e = e.stateNode) && e.__reactInternalMemoizedMergedChildContext || tn, xn = ze.current, re(ze, e), re(He, He.current), !0;
}
function Ps(e, t, n) {
  var r = e.stateNode;
  if (!r) throw Error(I(169));
  n ? (e = mu(e, t, xn), r.__reactInternalMemoizedMergedChildContext = e, ie(He), ie(ze), re(ze, e)) : ie(He), re(He, n);
}
var Et = null, nl = !1, Al = !1;
function hu(e) {
  Et === null ? Et = [e] : Et.push(e);
}
function Cp(e) {
  nl = !0, hu(e);
}
function ln() {
  if (!Al && Et !== null) {
    Al = !0;
    var e = 0, t = ee;
    try {
      var n = Et;
      for (ee = 1; e < n.length; e++) {
        var r = n[e];
        do
          r = r(!0);
        while (r !== null);
      }
      Et = null, nl = !1;
    } catch (a) {
      throw Et !== null && (Et = Et.slice(e + 1)), bc(Zi, ln), a;
    } finally {
      ee = t, Al = !1;
    }
  }
  return null;
}
var Dn = [], Ln = 0, Ma = null, $a = 0, rt = [], at = 0, gn = null, It = 1, Pt = "";
function cn(e, t) {
  Dn[Ln++] = $a, Dn[Ln++] = Ma, Ma = e, $a = t;
}
function vu(e, t, n) {
  rt[at++] = It, rt[at++] = Pt, rt[at++] = gn, gn = e;
  var r = It;
  e = Pt;
  var a = 32 - mt(r) - 1;
  r &= ~(1 << a), n += 1;
  var i = 32 - mt(t) + a;
  if (30 < i) {
    var o = a - a % 5;
    i = (r & (1 << o) - 1).toString(32), r >>= o, a -= o, It = 1 << 32 - mt(t) + a | n << a | r, Pt = i + e;
  } else It = 1 << i | n << a | r, Pt = e;
}
function oo(e) {
  e.return !== null && (cn(e, 1), vu(e, 1, 0));
}
function so(e) {
  for (; e === Ma; ) Ma = Dn[--Ln], Dn[Ln] = null, $a = Dn[--Ln], Dn[Ln] = null;
  for (; e === gn; ) gn = rt[--at], rt[at] = null, Pt = rt[--at], rt[at] = null, It = rt[--at], rt[at] = null;
}
var Ze = null, Xe = null, oe = !1, pt = null;
function xu(e, t) {
  var n = it(5, null, null, 0);
  n.elementType = "DELETED", n.stateNode = t, n.return = e, t = e.deletions, t === null ? (e.deletions = [n], e.flags |= 16) : t.push(n);
}
function Fs(e, t) {
  switch (e.tag) {
    case 5:
      var n = e.type;
      return t = t.nodeType !== 1 || n.toLowerCase() !== t.nodeName.toLowerCase() ? null : t, t !== null ? (e.stateNode = t, Ze = e, Xe = qt(t.firstChild), !0) : !1;
    case 6:
      return t = e.pendingProps === "" || t.nodeType !== 3 ? null : t, t !== null ? (e.stateNode = t, Ze = e, Xe = null, !0) : !1;
    case 13:
      return t = t.nodeType !== 8 ? null : t, t !== null ? (n = gn !== null ? { id: It, overflow: Pt } : null, e.memoizedState = { dehydrated: t, treeContext: n, retryLane: 1073741824 }, n = it(18, null, null, 0), n.stateNode = t, n.return = e, e.child = n, Ze = e, Xe = null, !0) : !1;
    default:
      return !1;
  }
}
function Ni(e) {
  return (e.mode & 1) !== 0 && (e.flags & 128) === 0;
}
function Si(e) {
  if (oe) {
    var t = Xe;
    if (t) {
      var n = t;
      if (!Fs(e, t)) {
        if (Ni(e)) throw Error(I(418));
        t = qt(n.nextSibling);
        var r = Ze;
        t && Fs(e, t) ? xu(r, n) : (e.flags = e.flags & -4097 | 2, oe = !1, Ze = e);
      }
    } else {
      if (Ni(e)) throw Error(I(418));
      e.flags = e.flags & -4097 | 2, oe = !1, Ze = e;
    }
  }
}
function Rs(e) {
  for (e = e.return; e !== null && e.tag !== 5 && e.tag !== 3 && e.tag !== 13; ) e = e.return;
  Ze = e;
}
function sa(e) {
  if (e !== Ze) return !1;
  if (!oe) return Rs(e), oe = !0, !1;
  var t;
  if ((t = e.tag !== 3) && !(t = e.tag !== 5) && (t = e.type, t = t !== "head" && t !== "body" && !xi(e.type, e.memoizedProps)), t && (t = Xe)) {
    if (Ni(e)) throw gu(), Error(I(418));
    for (; t; ) xu(e, t), t = qt(t.nextSibling);
  }
  if (Rs(e), e.tag === 13) {
    if (e = e.memoizedState, e = e !== null ? e.dehydrated : null, !e) throw Error(I(317));
    e: {
      for (e = e.nextSibling, t = 0; e; ) {
        if (e.nodeType === 8) {
          var n = e.data;
          if (n === "/$") {
            if (t === 0) {
              Xe = qt(e.nextSibling);
              break e;
            }
            t--;
          } else n !== "$" && n !== "$!" && n !== "$?" || t++;
        }
        e = e.nextSibling;
      }
      Xe = null;
    }
  } else Xe = Ze ? qt(e.stateNode.nextSibling) : null;
  return !0;
}
function gu() {
  for (var e = Xe; e; ) e = qt(e.nextSibling);
}
function Kn() {
  Xe = Ze = null, oe = !1;
}
function co(e) {
  pt === null ? pt = [e] : pt.push(e);
}
var Ep = Dt.ReactCurrentBatchConfig;
function sr(e, t, n) {
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
function ca(e, t) {
  throw e = Object.prototype.toString.call(t), Error(I(31, e === "[object Object]" ? "object with keys {" + Object.keys(t).join(", ") + "}" : e));
}
function _s(e) {
  var t = e._init;
  return t(e._payload);
}
function yu(e) {
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
    return p = Jt(p, f), p.index = 0, p.sibling = null, p;
  }
  function i(p, f, x) {
    return p.index = x, e ? (x = p.alternate, x !== null ? (x = x.index, x < f ? (p.flags |= 2, f) : x) : (p.flags |= 2, f)) : (p.flags |= 1048576, f);
  }
  function o(p) {
    return e && p.alternate === null && (p.flags |= 2), p;
  }
  function s(p, f, x, C) {
    return f === null || f.tag !== 6 ? (f = Bl(x, p.mode, C), f.return = p, f) : (f = a(f, x), f.return = p, f);
  }
  function c(p, f, x, C) {
    var _ = x.type;
    return _ === Pn ? N(p, f, x.props.children, C, x.key) : f !== null && (f.elementType === _ || typeof _ == "object" && _ !== null && _.$$typeof === bt && _s(_) === f.type) ? (C = a(f, x.props), C.ref = sr(p, f, x), C.return = p, C) : (C = Ca(x.type, x.key, x.props, null, p.mode, C), C.ref = sr(p, f, x), C.return = p, C);
  }
  function d(p, f, x, C) {
    return f === null || f.tag !== 4 || f.stateNode.containerInfo !== x.containerInfo || f.stateNode.implementation !== x.implementation ? (f = Hl(x, p.mode, C), f.return = p, f) : (f = a(f, x.children || []), f.return = p, f);
  }
  function N(p, f, x, C, _) {
    return f === null || f.tag !== 7 ? (f = hn(x, p.mode, C, _), f.return = p, f) : (f = a(f, x), f.return = p, f);
  }
  function u(p, f, x) {
    if (typeof f == "string" && f !== "" || typeof f == "number") return f = Bl("" + f, p.mode, x), f.return = p, f;
    if (typeof f == "object" && f !== null) {
      switch (f.$$typeof) {
        case Zr:
          return x = Ca(f.type, f.key, f.props, null, p.mode, x), x.ref = sr(p, null, f), x.return = p, x;
        case In:
          return f = Hl(f, p.mode, x), f.return = p, f;
        case bt:
          var C = f._init;
          return u(p, C(f._payload), x);
      }
      if (fr(f) || rr(f)) return f = hn(f, p.mode, x, null), f.return = p, f;
      ca(p, f);
    }
    return null;
  }
  function h(p, f, x, C) {
    var _ = f !== null ? f.key : null;
    if (typeof x == "string" && x !== "" || typeof x == "number") return _ !== null ? null : s(p, f, "" + x, C);
    if (typeof x == "object" && x !== null) {
      switch (x.$$typeof) {
        case Zr:
          return x.key === _ ? c(p, f, x, C) : null;
        case In:
          return x.key === _ ? d(p, f, x, C) : null;
        case bt:
          return _ = x._init, h(
            p,
            f,
            _(x._payload),
            C
          );
      }
      if (fr(x) || rr(x)) return _ !== null ? null : N(p, f, x, C, null);
      ca(p, x);
    }
    return null;
  }
  function v(p, f, x, C, _) {
    if (typeof C == "string" && C !== "" || typeof C == "number") return p = p.get(x) || null, s(f, p, "" + C, _);
    if (typeof C == "object" && C !== null) {
      switch (C.$$typeof) {
        case Zr:
          return p = p.get(C.key === null ? x : C.key) || null, c(f, p, C, _);
        case In:
          return p = p.get(C.key === null ? x : C.key) || null, d(f, p, C, _);
        case bt:
          var F = C._init;
          return v(p, f, x, F(C._payload), _);
      }
      if (fr(C) || rr(C)) return p = p.get(x) || null, N(f, p, C, _, null);
      ca(f, C);
    }
    return null;
  }
  function y(p, f, x, C) {
    for (var _ = null, F = null, z = f, k = f = 0, A = null; z !== null && k < x.length; k++) {
      z.index > k ? (A = z, z = null) : A = z.sibling;
      var U = h(p, z, x[k], C);
      if (U === null) {
        z === null && (z = A);
        break;
      }
      e && z && U.alternate === null && t(p, z), f = i(U, f, k), F === null ? _ = U : F.sibling = U, F = U, z = A;
    }
    if (k === x.length) return n(p, z), oe && cn(p, k), _;
    if (z === null) {
      for (; k < x.length; k++) z = u(p, x[k], C), z !== null && (f = i(z, f, k), F === null ? _ = z : F.sibling = z, F = z);
      return oe && cn(p, k), _;
    }
    for (z = r(p, z); k < x.length; k++) A = v(z, p, k, x[k], C), A !== null && (e && A.alternate !== null && z.delete(A.key === null ? k : A.key), f = i(A, f, k), F === null ? _ = A : F.sibling = A, F = A);
    return e && z.forEach(function(P) {
      return t(p, P);
    }), oe && cn(p, k), _;
  }
  function S(p, f, x, C) {
    var _ = rr(x);
    if (typeof _ != "function") throw Error(I(150));
    if (x = _.call(x), x == null) throw Error(I(151));
    for (var F = _ = null, z = f, k = f = 0, A = null, U = x.next(); z !== null && !U.done; k++, U = x.next()) {
      z.index > k ? (A = z, z = null) : A = z.sibling;
      var P = h(p, z, U.value, C);
      if (P === null) {
        z === null && (z = A);
        break;
      }
      e && z && P.alternate === null && t(p, z), f = i(P, f, k), F === null ? _ = P : F.sibling = P, F = P, z = A;
    }
    if (U.done) return n(
      p,
      z
    ), oe && cn(p, k), _;
    if (z === null) {
      for (; !U.done; k++, U = x.next()) U = u(p, U.value, C), U !== null && (f = i(U, f, k), F === null ? _ = U : F.sibling = U, F = U);
      return oe && cn(p, k), _;
    }
    for (z = r(p, z); !U.done; k++, U = x.next()) U = v(z, p, k, U.value, C), U !== null && (e && U.alternate !== null && z.delete(U.key === null ? k : U.key), f = i(U, f, k), F === null ? _ = U : F.sibling = U, F = U);
    return e && z.forEach(function(G) {
      return t(p, G);
    }), oe && cn(p, k), _;
  }
  function O(p, f, x, C) {
    if (typeof x == "object" && x !== null && x.type === Pn && x.key === null && (x = x.props.children), typeof x == "object" && x !== null) {
      switch (x.$$typeof) {
        case Zr:
          e: {
            for (var _ = x.key, F = f; F !== null; ) {
              if (F.key === _) {
                if (_ = x.type, _ === Pn) {
                  if (F.tag === 7) {
                    n(p, F.sibling), f = a(F, x.props.children), f.return = p, p = f;
                    break e;
                  }
                } else if (F.elementType === _ || typeof _ == "object" && _ !== null && _.$$typeof === bt && _s(_) === F.type) {
                  n(p, F.sibling), f = a(F, x.props), f.ref = sr(p, F, x), f.return = p, p = f;
                  break e;
                }
                n(p, F);
                break;
              } else t(p, F);
              F = F.sibling;
            }
            x.type === Pn ? (f = hn(x.props.children, p.mode, C, x.key), f.return = p, p = f) : (C = Ca(x.type, x.key, x.props, null, p.mode, C), C.ref = sr(p, f, x), C.return = p, p = C);
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
            f = Hl(x, p.mode, C), f.return = p, p = f;
          }
          return o(p);
        case bt:
          return F = x._init, O(p, f, F(x._payload), C);
      }
      if (fr(x)) return y(p, f, x, C);
      if (rr(x)) return S(p, f, x, C);
      ca(p, x);
    }
    return typeof x == "string" && x !== "" || typeof x == "number" ? (x = "" + x, f !== null && f.tag === 6 ? (n(p, f.sibling), f = a(f, x), f.return = p, p = f) : (n(p, f), f = Bl(x, p.mode, C), f.return = p, p = f), o(p)) : n(p, f);
  }
  return O;
}
var qn = yu(!0), ju = yu(!1), Oa = an(null), ba = null, An = null, uo = null;
function fo() {
  uo = An = ba = null;
}
function po(e) {
  var t = Oa.current;
  ie(Oa), e._currentValue = t;
}
function wi(e, t, n) {
  for (; e !== null; ) {
    var r = e.alternate;
    if ((e.childLanes & t) !== t ? (e.childLanes |= t, r !== null && (r.childLanes |= t)) : r !== null && (r.childLanes & t) !== t && (r.childLanes |= t), e === n) break;
    e = e.return;
  }
}
function Bn(e, t) {
  ba = e, uo = An = null, e = e.dependencies, e !== null && e.firstContext !== null && (e.lanes & t && (Be = !0), e.firstContext = null);
}
function st(e) {
  var t = e._currentValue;
  if (uo !== e) if (e = { context: e, memoizedValue: t, next: null }, An === null) {
    if (ba === null) throw Error(I(308));
    An = e, ba.dependencies = { lanes: 0, firstContext: e };
  } else An = An.next = e;
  return t;
}
var fn = null;
function mo(e) {
  fn === null ? fn = [e] : fn.push(e);
}
function Nu(e, t, n, r) {
  var a = t.interleaved;
  return a === null ? (n.next = n, mo(t)) : (n.next = a.next, a.next = n), t.interleaved = n, Tt(e, r);
}
function Tt(e, t) {
  e.lanes |= t;
  var n = e.alternate;
  for (n !== null && (n.lanes |= t), n = e, e = e.return; e !== null; ) e.childLanes |= t, n = e.alternate, n !== null && (n.childLanes |= t), n = e, e = e.return;
  return n.tag === 3 ? n.stateNode : null;
}
var Ut = !1;
function ho(e) {
  e.updateQueue = { baseState: e.memoizedState, firstBaseUpdate: null, lastBaseUpdate: null, shared: { pending: null, interleaved: null, lanes: 0 }, effects: null };
}
function Su(e, t) {
  e = e.updateQueue, t.updateQueue === e && (t.updateQueue = { baseState: e.baseState, firstBaseUpdate: e.firstBaseUpdate, lastBaseUpdate: e.lastBaseUpdate, shared: e.shared, effects: e.effects });
}
function Ft(e, t) {
  return { eventTime: e, lane: t, tag: 0, payload: null, callback: null, next: null };
}
function Yt(e, t, n) {
  var r = e.updateQueue;
  if (r === null) return null;
  if (r = r.shared, Y & 2) {
    var a = r.pending;
    return a === null ? t.next = t : (t.next = a.next, a.next = t), r.pending = t, Tt(e, n);
  }
  return a = r.interleaved, a === null ? (t.next = t, mo(r)) : (t.next = a.next, a.next = t), r.interleaved = t, Tt(e, n);
}
function ya(e, t, n) {
  if (t = t.updateQueue, t !== null && (t = t.shared, (n & 4194240) !== 0)) {
    var r = t.lanes;
    r &= e.pendingLanes, n |= r, t.lanes = n, Ji(e, n);
  }
}
function Ts(e, t) {
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
function Ua(e, t, n, r) {
  var a = e.updateQueue;
  Ut = !1;
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
              Ut = !0;
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
function zs(e, t, n) {
  if (e = t.effects, t.effects = null, e !== null) for (t = 0; t < e.length; t++) {
    var r = e[t], a = r.callback;
    if (a !== null) {
      if (r.callback = null, r = n, typeof a != "function") throw Error(I(191, a));
      a.call(r);
    }
  }
}
var Gr = {}, wt = an(Gr), Ar = an(Gr), Mr = an(Gr);
function pn(e) {
  if (e === Gr) throw Error(I(174));
  return e;
}
function vo(e, t) {
  switch (re(Mr, t), re(Ar, e), re(wt, Gr), e = t.nodeType, e) {
    case 9:
    case 11:
      t = (t = t.documentElement) ? t.namespaceURI : ri(null, "");
      break;
    default:
      e = e === 8 ? t.parentNode : t, t = e.namespaceURI || null, e = e.tagName, t = ri(t, e);
  }
  ie(wt), re(wt, t);
}
function Yn() {
  ie(wt), ie(Ar), ie(Mr);
}
function wu(e) {
  pn(Mr.current);
  var t = pn(wt.current), n = ri(t, e.type);
  t !== n && (re(Ar, e), re(wt, n));
}
function xo(e) {
  Ar.current === e && (ie(wt), ie(Ar));
}
var ce = an(0);
function Va(e) {
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
var Ml = [];
function go() {
  for (var e = 0; e < Ml.length; e++) Ml[e]._workInProgressVersionPrimary = null;
  Ml.length = 0;
}
var ja = Dt.ReactCurrentDispatcher, $l = Dt.ReactCurrentBatchConfig, yn = 0, ue = null, ye = null, Se = null, Ba = !1, jr = !1, $r = 0, Ip = 0;
function Fe() {
  throw Error(I(321));
}
function yo(e, t) {
  if (t === null) return !1;
  for (var n = 0; n < t.length && n < e.length; n++) if (!xt(e[n], t[n])) return !1;
  return !0;
}
function jo(e, t, n, r, a, i) {
  if (yn = i, ue = t, t.memoizedState = null, t.updateQueue = null, t.lanes = 0, ja.current = e === null || e.memoizedState === null ? _p : Tp, e = n(r, a), jr) {
    i = 0;
    do {
      if (jr = !1, $r = 0, 25 <= i) throw Error(I(301));
      i += 1, Se = ye = null, t.updateQueue = null, ja.current = zp, e = n(r, a);
    } while (jr);
  }
  if (ja.current = Ha, t = ye !== null && ye.next !== null, yn = 0, Se = ye = ue = null, Ba = !1, t) throw Error(I(300));
  return e;
}
function No() {
  var e = $r !== 0;
  return $r = 0, e;
}
function jt() {
  var e = { memoizedState: null, baseState: null, baseQueue: null, queue: null, next: null };
  return Se === null ? ue.memoizedState = Se = e : Se = Se.next = e, Se;
}
function ct() {
  if (ye === null) {
    var e = ue.alternate;
    e = e !== null ? e.memoizedState : null;
  } else e = ye.next;
  var t = Se === null ? ue.memoizedState : Se.next;
  if (t !== null) Se = t, ye = e;
  else {
    if (e === null) throw Error(I(310));
    ye = e, e = { memoizedState: ye.memoizedState, baseState: ye.baseState, baseQueue: ye.baseQueue, queue: ye.queue, next: null }, Se === null ? ue.memoizedState = Se = e : Se = Se.next = e;
  }
  return Se;
}
function Or(e, t) {
  return typeof t == "function" ? t(e) : t;
}
function Ol(e) {
  var t = ct(), n = t.queue;
  if (n === null) throw Error(I(311));
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
    c === null ? o = r : c.next = s, xt(r, t.memoizedState) || (Be = !0), t.memoizedState = r, t.baseState = o, t.baseQueue = c, n.lastRenderedState = r;
  }
  if (e = n.interleaved, e !== null) {
    a = e;
    do
      i = a.lane, ue.lanes |= i, jn |= i, a = a.next;
    while (a !== e);
  } else a === null && (n.lanes = 0);
  return [t.memoizedState, n.dispatch];
}
function bl(e) {
  var t = ct(), n = t.queue;
  if (n === null) throw Error(I(311));
  n.lastRenderedReducer = e;
  var r = n.dispatch, a = n.pending, i = t.memoizedState;
  if (a !== null) {
    n.pending = null;
    var o = a = a.next;
    do
      i = e(i, o.action), o = o.next;
    while (o !== a);
    xt(i, t.memoizedState) || (Be = !0), t.memoizedState = i, t.baseQueue === null && (t.baseState = i), n.lastRenderedState = i;
  }
  return [i, r];
}
function ku() {
}
function Cu(e, t) {
  var n = ue, r = ct(), a = t(), i = !xt(r.memoizedState, a);
  if (i && (r.memoizedState = a, Be = !0), r = r.queue, So(Pu.bind(null, n, r, e), [e]), r.getSnapshot !== t || i || Se !== null && Se.memoizedState.tag & 1) {
    if (n.flags |= 2048, br(9, Iu.bind(null, n, r, a, t), void 0, null), ke === null) throw Error(I(349));
    yn & 30 || Eu(n, t, a);
  }
  return a;
}
function Eu(e, t, n) {
  e.flags |= 16384, e = { getSnapshot: t, value: n }, t = ue.updateQueue, t === null ? (t = { lastEffect: null, stores: null }, ue.updateQueue = t, t.stores = [e]) : (n = t.stores, n === null ? t.stores = [e] : n.push(e));
}
function Iu(e, t, n, r) {
  t.value = n, t.getSnapshot = r, Fu(t) && Ru(e);
}
function Pu(e, t, n) {
  return n(function() {
    Fu(t) && Ru(e);
  });
}
function Fu(e) {
  var t = e.getSnapshot;
  e = e.value;
  try {
    var n = t();
    return !xt(e, n);
  } catch {
    return !0;
  }
}
function Ru(e) {
  var t = Tt(e, 1);
  t !== null && ht(t, e, 1, -1);
}
function Ds(e) {
  var t = jt();
  return typeof e == "function" && (e = e()), t.memoizedState = t.baseState = e, e = { pending: null, interleaved: null, lanes: 0, dispatch: null, lastRenderedReducer: Or, lastRenderedState: e }, t.queue = e, e = e.dispatch = Rp.bind(null, ue, e), [t.memoizedState, e];
}
function br(e, t, n, r) {
  return e = { tag: e, create: t, destroy: n, deps: r, next: null }, t = ue.updateQueue, t === null ? (t = { lastEffect: null, stores: null }, ue.updateQueue = t, t.lastEffect = e.next = e) : (n = t.lastEffect, n === null ? t.lastEffect = e.next = e : (r = n.next, n.next = e, e.next = r, t.lastEffect = e)), e;
}
function _u() {
  return ct().memoizedState;
}
function Na(e, t, n, r) {
  var a = jt();
  ue.flags |= e, a.memoizedState = br(1 | t, n, void 0, r === void 0 ? null : r);
}
function rl(e, t, n, r) {
  var a = ct();
  r = r === void 0 ? null : r;
  var i = void 0;
  if (ye !== null) {
    var o = ye.memoizedState;
    if (i = o.destroy, r !== null && yo(r, o.deps)) {
      a.memoizedState = br(t, n, i, r);
      return;
    }
  }
  ue.flags |= e, a.memoizedState = br(1 | t, n, i, r);
}
function Ls(e, t) {
  return Na(8390656, 8, e, t);
}
function So(e, t) {
  return rl(2048, 8, e, t);
}
function Tu(e, t) {
  return rl(4, 2, e, t);
}
function zu(e, t) {
  return rl(4, 4, e, t);
}
function Du(e, t) {
  if (typeof t == "function") return e = e(), t(e), function() {
    t(null);
  };
  if (t != null) return e = e(), t.current = e, function() {
    t.current = null;
  };
}
function Lu(e, t, n) {
  return n = n != null ? n.concat([e]) : null, rl(4, 4, Du.bind(null, t, e), n);
}
function wo() {
}
function Au(e, t) {
  var n = ct();
  t = t === void 0 ? null : t;
  var r = n.memoizedState;
  return r !== null && t !== null && yo(t, r[1]) ? r[0] : (n.memoizedState = [e, t], e);
}
function Mu(e, t) {
  var n = ct();
  t = t === void 0 ? null : t;
  var r = n.memoizedState;
  return r !== null && t !== null && yo(t, r[1]) ? r[0] : (e = e(), n.memoizedState = [e, t], e);
}
function $u(e, t, n) {
  return yn & 21 ? (xt(n, t) || (n = Bc(), ue.lanes |= n, jn |= n, e.baseState = !0), t) : (e.baseState && (e.baseState = !1, Be = !0), e.memoizedState = n);
}
function Pp(e, t) {
  var n = ee;
  ee = n !== 0 && 4 > n ? n : 4, e(!0);
  var r = $l.transition;
  $l.transition = {};
  try {
    e(!1), t();
  } finally {
    ee = n, $l.transition = r;
  }
}
function Ou() {
  return ct().memoizedState;
}
function Fp(e, t, n) {
  var r = Zt(e);
  if (n = { lane: r, action: n, hasEagerState: !1, eagerState: null, next: null }, bu(e)) Uu(t, n);
  else if (n = Nu(e, t, n, r), n !== null) {
    var a = $e();
    ht(n, e, r, a), Vu(n, t, r);
  }
}
function Rp(e, t, n) {
  var r = Zt(e), a = { lane: r, action: n, hasEagerState: !1, eagerState: null, next: null };
  if (bu(e)) Uu(t, a);
  else {
    var i = e.alternate;
    if (e.lanes === 0 && (i === null || i.lanes === 0) && (i = t.lastRenderedReducer, i !== null)) try {
      var o = t.lastRenderedState, s = i(o, n);
      if (a.hasEagerState = !0, a.eagerState = s, xt(s, o)) {
        var c = t.interleaved;
        c === null ? (a.next = a, mo(t)) : (a.next = c.next, c.next = a), t.interleaved = a;
        return;
      }
    } catch {
    } finally {
    }
    n = Nu(e, t, a, r), n !== null && (a = $e(), ht(n, e, r, a), Vu(n, t, r));
  }
}
function bu(e) {
  var t = e.alternate;
  return e === ue || t !== null && t === ue;
}
function Uu(e, t) {
  jr = Ba = !0;
  var n = e.pending;
  n === null ? t.next = t : (t.next = n.next, n.next = t), e.pending = t;
}
function Vu(e, t, n) {
  if (n & 4194240) {
    var r = t.lanes;
    r &= e.pendingLanes, n |= r, t.lanes = n, Ji(e, n);
  }
}
var Ha = { readContext: st, useCallback: Fe, useContext: Fe, useEffect: Fe, useImperativeHandle: Fe, useInsertionEffect: Fe, useLayoutEffect: Fe, useMemo: Fe, useReducer: Fe, useRef: Fe, useState: Fe, useDebugValue: Fe, useDeferredValue: Fe, useTransition: Fe, useMutableSource: Fe, useSyncExternalStore: Fe, useId: Fe, unstable_isNewReconciler: !1 }, _p = { readContext: st, useCallback: function(e, t) {
  return jt().memoizedState = [e, t === void 0 ? null : t], e;
}, useContext: st, useEffect: Ls, useImperativeHandle: function(e, t, n) {
  return n = n != null ? n.concat([e]) : null, Na(
    4194308,
    4,
    Du.bind(null, t, e),
    n
  );
}, useLayoutEffect: function(e, t) {
  return Na(4194308, 4, e, t);
}, useInsertionEffect: function(e, t) {
  return Na(4, 2, e, t);
}, useMemo: function(e, t) {
  var n = jt();
  return t = t === void 0 ? null : t, e = e(), n.memoizedState = [e, t], e;
}, useReducer: function(e, t, n) {
  var r = jt();
  return t = n !== void 0 ? n(t) : t, r.memoizedState = r.baseState = t, e = { pending: null, interleaved: null, lanes: 0, dispatch: null, lastRenderedReducer: e, lastRenderedState: t }, r.queue = e, e = e.dispatch = Fp.bind(null, ue, e), [r.memoizedState, e];
}, useRef: function(e) {
  var t = jt();
  return e = { current: e }, t.memoizedState = e;
}, useState: Ds, useDebugValue: wo, useDeferredValue: function(e) {
  return jt().memoizedState = e;
}, useTransition: function() {
  var e = Ds(!1), t = e[0];
  return e = Pp.bind(null, e[1]), jt().memoizedState = e, [t, e];
}, useMutableSource: function() {
}, useSyncExternalStore: function(e, t, n) {
  var r = ue, a = jt();
  if (oe) {
    if (n === void 0) throw Error(I(407));
    n = n();
  } else {
    if (n = t(), ke === null) throw Error(I(349));
    yn & 30 || Eu(r, t, n);
  }
  a.memoizedState = n;
  var i = { value: n, getSnapshot: t };
  return a.queue = i, Ls(Pu.bind(
    null,
    r,
    i,
    e
  ), [e]), r.flags |= 2048, br(9, Iu.bind(null, r, i, n, t), void 0, null), n;
}, useId: function() {
  var e = jt(), t = ke.identifierPrefix;
  if (oe) {
    var n = Pt, r = It;
    n = (r & ~(1 << 32 - mt(r) - 1)).toString(32) + n, t = ":" + t + "R" + n, n = $r++, 0 < n && (t += "H" + n.toString(32)), t += ":";
  } else n = Ip++, t = ":" + t + "r" + n.toString(32) + ":";
  return e.memoizedState = t;
}, unstable_isNewReconciler: !1 }, Tp = {
  readContext: st,
  useCallback: Au,
  useContext: st,
  useEffect: So,
  useImperativeHandle: Lu,
  useInsertionEffect: Tu,
  useLayoutEffect: zu,
  useMemo: Mu,
  useReducer: Ol,
  useRef: _u,
  useState: function() {
    return Ol(Or);
  },
  useDebugValue: wo,
  useDeferredValue: function(e) {
    var t = ct();
    return $u(t, ye.memoizedState, e);
  },
  useTransition: function() {
    var e = Ol(Or)[0], t = ct().memoizedState;
    return [e, t];
  },
  useMutableSource: ku,
  useSyncExternalStore: Cu,
  useId: Ou,
  unstable_isNewReconciler: !1
}, zp = { readContext: st, useCallback: Au, useContext: st, useEffect: So, useImperativeHandle: Lu, useInsertionEffect: Tu, useLayoutEffect: zu, useMemo: Mu, useReducer: bl, useRef: _u, useState: function() {
  return bl(Or);
}, useDebugValue: wo, useDeferredValue: function(e) {
  var t = ct();
  return ye === null ? t.memoizedState = e : $u(t, ye.memoizedState, e);
}, useTransition: function() {
  var e = bl(Or)[0], t = ct().memoizedState;
  return [e, t];
}, useMutableSource: ku, useSyncExternalStore: Cu, useId: Ou, unstable_isNewReconciler: !1 };
function dt(e, t) {
  if (e && e.defaultProps) {
    t = de({}, t), e = e.defaultProps;
    for (var n in e) t[n] === void 0 && (t[n] = e[n]);
    return t;
  }
  return t;
}
function ki(e, t, n, r) {
  t = e.memoizedState, n = n(r, t), n = n == null ? t : de({}, t, n), e.memoizedState = n, e.lanes === 0 && (e.updateQueue.baseState = n);
}
var al = { isMounted: function(e) {
  return (e = e._reactInternals) ? wn(e) === e : !1;
}, enqueueSetState: function(e, t, n) {
  e = e._reactInternals;
  var r = $e(), a = Zt(e), i = Ft(r, a);
  i.payload = t, n != null && (i.callback = n), t = Yt(e, i, a), t !== null && (ht(t, e, a, r), ya(t, e, a));
}, enqueueReplaceState: function(e, t, n) {
  e = e._reactInternals;
  var r = $e(), a = Zt(e), i = Ft(r, a);
  i.tag = 1, i.payload = t, n != null && (i.callback = n), t = Yt(e, i, a), t !== null && (ht(t, e, a, r), ya(t, e, a));
}, enqueueForceUpdate: function(e, t) {
  e = e._reactInternals;
  var n = $e(), r = Zt(e), a = Ft(n, r);
  a.tag = 2, t != null && (a.callback = t), t = Yt(e, a, r), t !== null && (ht(t, e, r, n), ya(t, e, r));
} };
function As(e, t, n, r, a, i, o) {
  return e = e.stateNode, typeof e.shouldComponentUpdate == "function" ? e.shouldComponentUpdate(r, i, o) : t.prototype && t.prototype.isPureReactComponent ? !Tr(n, r) || !Tr(a, i) : !0;
}
function Bu(e, t, n) {
  var r = !1, a = tn, i = t.contextType;
  return typeof i == "object" && i !== null ? i = st(i) : (a = We(t) ? xn : ze.current, r = t.contextTypes, i = (r = r != null) ? Gn(e, a) : tn), t = new t(n, i), e.memoizedState = t.state !== null && t.state !== void 0 ? t.state : null, t.updater = al, e.stateNode = t, t._reactInternals = e, r && (e = e.stateNode, e.__reactInternalMemoizedUnmaskedChildContext = a, e.__reactInternalMemoizedMaskedChildContext = i), t;
}
function Ms(e, t, n, r) {
  e = t.state, typeof t.componentWillReceiveProps == "function" && t.componentWillReceiveProps(n, r), typeof t.UNSAFE_componentWillReceiveProps == "function" && t.UNSAFE_componentWillReceiveProps(n, r), t.state !== e && al.enqueueReplaceState(t, t.state, null);
}
function Ci(e, t, n, r) {
  var a = e.stateNode;
  a.props = n, a.state = e.memoizedState, a.refs = {}, ho(e);
  var i = t.contextType;
  typeof i == "object" && i !== null ? a.context = st(i) : (i = We(t) ? xn : ze.current, a.context = Gn(e, i)), a.state = e.memoizedState, i = t.getDerivedStateFromProps, typeof i == "function" && (ki(e, t, i, n), a.state = e.memoizedState), typeof t.getDerivedStateFromProps == "function" || typeof a.getSnapshotBeforeUpdate == "function" || typeof a.UNSAFE_componentWillMount != "function" && typeof a.componentWillMount != "function" || (t = a.state, typeof a.componentWillMount == "function" && a.componentWillMount(), typeof a.UNSAFE_componentWillMount == "function" && a.UNSAFE_componentWillMount(), t !== a.state && al.enqueueReplaceState(a, a.state, null), Ua(e, n, a, r), a.state = e.memoizedState), typeof a.componentDidMount == "function" && (e.flags |= 4194308);
}
function Xn(e, t) {
  try {
    var n = "", r = t;
    do
      n += of(r), r = r.return;
    while (r);
    var a = n;
  } catch (i) {
    a = `
Error generating stack: ` + i.message + `
` + i.stack;
  }
  return { value: e, source: t, stack: a, digest: null };
}
function Ul(e, t, n) {
  return { value: e, source: null, stack: n ?? null, digest: t ?? null };
}
function Ei(e, t) {
  try {
    console.error(t.value);
  } catch (n) {
    setTimeout(function() {
      throw n;
    });
  }
}
var Dp = typeof WeakMap == "function" ? WeakMap : Map;
function Hu(e, t, n) {
  n = Ft(-1, n), n.tag = 3, n.payload = { element: null };
  var r = t.value;
  return n.callback = function() {
    Qa || (Qa = !0, Ai = r), Ei(e, t);
  }, n;
}
function Wu(e, t, n) {
  n = Ft(-1, n), n.tag = 3;
  var r = e.type.getDerivedStateFromError;
  if (typeof r == "function") {
    var a = t.value;
    n.payload = function() {
      return r(a);
    }, n.callback = function() {
      Ei(e, t);
    };
  }
  var i = e.stateNode;
  return i !== null && typeof i.componentDidCatch == "function" && (n.callback = function() {
    Ei(e, t), typeof r != "function" && (Xt === null ? Xt = /* @__PURE__ */ new Set([this]) : Xt.add(this));
    var o = t.stack;
    this.componentDidCatch(t.value, { componentStack: o !== null ? o : "" });
  }), n;
}
function $s(e, t, n) {
  var r = e.pingCache;
  if (r === null) {
    r = e.pingCache = new Dp();
    var a = /* @__PURE__ */ new Set();
    r.set(t, a);
  } else a = r.get(t), a === void 0 && (a = /* @__PURE__ */ new Set(), r.set(t, a));
  a.has(n) || (a.add(n), e = Kp.bind(null, e, t, n), t.then(e, e));
}
function Os(e) {
  do {
    var t;
    if ((t = e.tag === 13) && (t = e.memoizedState, t = t !== null ? t.dehydrated !== null : !0), t) return e;
    e = e.return;
  } while (e !== null);
  return null;
}
function bs(e, t, n, r, a) {
  return e.mode & 1 ? (e.flags |= 65536, e.lanes = a, e) : (e === t ? e.flags |= 65536 : (e.flags |= 128, n.flags |= 131072, n.flags &= -52805, n.tag === 1 && (n.alternate === null ? n.tag = 17 : (t = Ft(-1, 1), t.tag = 2, Yt(n, t, 1))), n.lanes |= 1), e);
}
var Lp = Dt.ReactCurrentOwner, Be = !1;
function Le(e, t, n, r) {
  t.child = e === null ? ju(t, null, n, r) : qn(t, e.child, n, r);
}
function Us(e, t, n, r, a) {
  n = n.render;
  var i = t.ref;
  return Bn(t, a), r = jo(e, t, n, r, i, a), n = No(), e !== null && !Be ? (t.updateQueue = e.updateQueue, t.flags &= -2053, e.lanes &= ~a, zt(e, t, a)) : (oe && n && oo(t), t.flags |= 1, Le(e, t, r, a), t.child);
}
function Vs(e, t, n, r, a) {
  if (e === null) {
    var i = n.type;
    return typeof i == "function" && !_o(i) && i.defaultProps === void 0 && n.compare === null && n.defaultProps === void 0 ? (t.tag = 15, t.type = i, Qu(e, t, i, r, a)) : (e = Ca(n.type, null, r, t, t.mode, a), e.ref = t.ref, e.return = t, t.child = e);
  }
  if (i = e.child, !(e.lanes & a)) {
    var o = i.memoizedProps;
    if (n = n.compare, n = n !== null ? n : Tr, n(o, r) && e.ref === t.ref) return zt(e, t, a);
  }
  return t.flags |= 1, e = Jt(i, r), e.ref = t.ref, e.return = t, t.child = e;
}
function Qu(e, t, n, r, a) {
  if (e !== null) {
    var i = e.memoizedProps;
    if (Tr(i, r) && e.ref === t.ref) if (Be = !1, t.pendingProps = r = i, (e.lanes & a) !== 0) e.flags & 131072 && (Be = !0);
    else return t.lanes = e.lanes, zt(e, t, a);
  }
  return Ii(e, t, n, r, a);
}
function Gu(e, t, n) {
  var r = t.pendingProps, a = r.children, i = e !== null ? e.memoizedState : null;
  if (r.mode === "hidden") if (!(t.mode & 1)) t.memoizedState = { baseLanes: 0, cachePool: null, transitions: null }, re($n, Ye), Ye |= n;
  else {
    if (!(n & 1073741824)) return e = i !== null ? i.baseLanes | n : n, t.lanes = t.childLanes = 1073741824, t.memoizedState = { baseLanes: e, cachePool: null, transitions: null }, t.updateQueue = null, re($n, Ye), Ye |= e, null;
    t.memoizedState = { baseLanes: 0, cachePool: null, transitions: null }, r = i !== null ? i.baseLanes : n, re($n, Ye), Ye |= r;
  }
  else i !== null ? (r = i.baseLanes | n, t.memoizedState = null) : r = n, re($n, Ye), Ye |= r;
  return Le(e, t, a, n), t.child;
}
function Ku(e, t) {
  var n = t.ref;
  (e === null && n !== null || e !== null && e.ref !== n) && (t.flags |= 512, t.flags |= 2097152);
}
function Ii(e, t, n, r, a) {
  var i = We(n) ? xn : ze.current;
  return i = Gn(t, i), Bn(t, a), n = jo(e, t, n, r, i, a), r = No(), e !== null && !Be ? (t.updateQueue = e.updateQueue, t.flags &= -2053, e.lanes &= ~a, zt(e, t, a)) : (oe && r && oo(t), t.flags |= 1, Le(e, t, n, a), t.child);
}
function Bs(e, t, n, r, a) {
  if (We(n)) {
    var i = !0;
    Aa(t);
  } else i = !1;
  if (Bn(t, a), t.stateNode === null) Sa(e, t), Bu(t, n, r), Ci(t, n, r, a), r = !0;
  else if (e === null) {
    var o = t.stateNode, s = t.memoizedProps;
    o.props = s;
    var c = o.context, d = n.contextType;
    typeof d == "object" && d !== null ? d = st(d) : (d = We(n) ? xn : ze.current, d = Gn(t, d));
    var N = n.getDerivedStateFromProps, u = typeof N == "function" || typeof o.getSnapshotBeforeUpdate == "function";
    u || typeof o.UNSAFE_componentWillReceiveProps != "function" && typeof o.componentWillReceiveProps != "function" || (s !== r || c !== d) && Ms(t, o, r, d), Ut = !1;
    var h = t.memoizedState;
    o.state = h, Ua(t, r, o, a), c = t.memoizedState, s !== r || h !== c || He.current || Ut ? (typeof N == "function" && (ki(t, n, N, r), c = t.memoizedState), (s = Ut || As(t, n, s, r, h, c, d)) ? (u || typeof o.UNSAFE_componentWillMount != "function" && typeof o.componentWillMount != "function" || (typeof o.componentWillMount == "function" && o.componentWillMount(), typeof o.UNSAFE_componentWillMount == "function" && o.UNSAFE_componentWillMount()), typeof o.componentDidMount == "function" && (t.flags |= 4194308)) : (typeof o.componentDidMount == "function" && (t.flags |= 4194308), t.memoizedProps = r, t.memoizedState = c), o.props = r, o.state = c, o.context = d, r = s) : (typeof o.componentDidMount == "function" && (t.flags |= 4194308), r = !1);
  } else {
    o = t.stateNode, Su(e, t), s = t.memoizedProps, d = t.type === t.elementType ? s : dt(t.type, s), o.props = d, u = t.pendingProps, h = o.context, c = n.contextType, typeof c == "object" && c !== null ? c = st(c) : (c = We(n) ? xn : ze.current, c = Gn(t, c));
    var v = n.getDerivedStateFromProps;
    (N = typeof v == "function" || typeof o.getSnapshotBeforeUpdate == "function") || typeof o.UNSAFE_componentWillReceiveProps != "function" && typeof o.componentWillReceiveProps != "function" || (s !== u || h !== c) && Ms(t, o, r, c), Ut = !1, h = t.memoizedState, o.state = h, Ua(t, r, o, a);
    var y = t.memoizedState;
    s !== u || h !== y || He.current || Ut ? (typeof v == "function" && (ki(t, n, v, r), y = t.memoizedState), (d = Ut || As(t, n, d, r, h, y, c) || !1) ? (N || typeof o.UNSAFE_componentWillUpdate != "function" && typeof o.componentWillUpdate != "function" || (typeof o.componentWillUpdate == "function" && o.componentWillUpdate(r, y, c), typeof o.UNSAFE_componentWillUpdate == "function" && o.UNSAFE_componentWillUpdate(r, y, c)), typeof o.componentDidUpdate == "function" && (t.flags |= 4), typeof o.getSnapshotBeforeUpdate == "function" && (t.flags |= 1024)) : (typeof o.componentDidUpdate != "function" || s === e.memoizedProps && h === e.memoizedState || (t.flags |= 4), typeof o.getSnapshotBeforeUpdate != "function" || s === e.memoizedProps && h === e.memoizedState || (t.flags |= 1024), t.memoizedProps = r, t.memoizedState = y), o.props = r, o.state = y, o.context = c, r = d) : (typeof o.componentDidUpdate != "function" || s === e.memoizedProps && h === e.memoizedState || (t.flags |= 4), typeof o.getSnapshotBeforeUpdate != "function" || s === e.memoizedProps && h === e.memoizedState || (t.flags |= 1024), r = !1);
  }
  return Pi(e, t, n, r, i, a);
}
function Pi(e, t, n, r, a, i) {
  Ku(e, t);
  var o = (t.flags & 128) !== 0;
  if (!r && !o) return a && Ps(t, n, !1), zt(e, t, i);
  r = t.stateNode, Lp.current = t;
  var s = o && typeof n.getDerivedStateFromError != "function" ? null : r.render();
  return t.flags |= 1, e !== null && o ? (t.child = qn(t, e.child, null, i), t.child = qn(t, null, s, i)) : Le(e, t, s, i), t.memoizedState = r.state, a && Ps(t, n, !0), t.child;
}
function qu(e) {
  var t = e.stateNode;
  t.pendingContext ? Is(e, t.pendingContext, t.pendingContext !== t.context) : t.context && Is(e, t.context, !1), vo(e, t.containerInfo);
}
function Hs(e, t, n, r, a) {
  return Kn(), co(a), t.flags |= 256, Le(e, t, n, r), t.child;
}
var Fi = { dehydrated: null, treeContext: null, retryLane: 0 };
function Ri(e) {
  return { baseLanes: e, cachePool: null, transitions: null };
}
function Yu(e, t, n) {
  var r = t.pendingProps, a = ce.current, i = !1, o = (t.flags & 128) !== 0, s;
  if ((s = o) || (s = e !== null && e.memoizedState === null ? !1 : (a & 2) !== 0), s ? (i = !0, t.flags &= -129) : (e === null || e.memoizedState !== null) && (a |= 1), re(ce, a & 1), e === null)
    return Si(t), e = t.memoizedState, e !== null && (e = e.dehydrated, e !== null) ? (t.mode & 1 ? e.data === "$!" ? t.lanes = 8 : t.lanes = 1073741824 : t.lanes = 1, null) : (o = r.children, e = r.fallback, i ? (r = t.mode, i = t.child, o = { mode: "hidden", children: o }, !(r & 1) && i !== null ? (i.childLanes = 0, i.pendingProps = o) : i = ol(o, r, 0, null), e = hn(e, r, n, null), i.return = t, e.return = t, i.sibling = e, t.child = i, t.child.memoizedState = Ri(n), t.memoizedState = Fi, e) : ko(t, o));
  if (a = e.memoizedState, a !== null && (s = a.dehydrated, s !== null)) return Ap(e, t, o, r, s, a, n);
  if (i) {
    i = r.fallback, o = t.mode, a = e.child, s = a.sibling;
    var c = { mode: "hidden", children: r.children };
    return !(o & 1) && t.child !== a ? (r = t.child, r.childLanes = 0, r.pendingProps = c, t.deletions = null) : (r = Jt(a, c), r.subtreeFlags = a.subtreeFlags & 14680064), s !== null ? i = Jt(s, i) : (i = hn(i, o, n, null), i.flags |= 2), i.return = t, r.return = t, r.sibling = i, t.child = r, r = i, i = t.child, o = e.child.memoizedState, o = o === null ? Ri(n) : { baseLanes: o.baseLanes | n, cachePool: null, transitions: o.transitions }, i.memoizedState = o, i.childLanes = e.childLanes & ~n, t.memoizedState = Fi, r;
  }
  return i = e.child, e = i.sibling, r = Jt(i, { mode: "visible", children: r.children }), !(t.mode & 1) && (r.lanes = n), r.return = t, r.sibling = null, e !== null && (n = t.deletions, n === null ? (t.deletions = [e], t.flags |= 16) : n.push(e)), t.child = r, t.memoizedState = null, r;
}
function ko(e, t) {
  return t = ol({ mode: "visible", children: t }, e.mode, 0, null), t.return = e, e.child = t;
}
function ua(e, t, n, r) {
  return r !== null && co(r), qn(t, e.child, null, n), e = ko(t, t.pendingProps.children), e.flags |= 2, t.memoizedState = null, e;
}
function Ap(e, t, n, r, a, i, o) {
  if (n)
    return t.flags & 256 ? (t.flags &= -257, r = Ul(Error(I(422))), ua(e, t, o, r)) : t.memoizedState !== null ? (t.child = e.child, t.flags |= 128, null) : (i = r.fallback, a = t.mode, r = ol({ mode: "visible", children: r.children }, a, 0, null), i = hn(i, a, o, null), i.flags |= 2, r.return = t, i.return = t, r.sibling = i, t.child = r, t.mode & 1 && qn(t, e.child, null, o), t.child.memoizedState = Ri(o), t.memoizedState = Fi, i);
  if (!(t.mode & 1)) return ua(e, t, o, null);
  if (a.data === "$!") {
    if (r = a.nextSibling && a.nextSibling.dataset, r) var s = r.dgst;
    return r = s, i = Error(I(419)), r = Ul(i, r, void 0), ua(e, t, o, r);
  }
  if (s = (o & e.childLanes) !== 0, Be || s) {
    if (r = ke, r !== null) {
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
      a = a & (r.suspendedLanes | o) ? 0 : a, a !== 0 && a !== i.retryLane && (i.retryLane = a, Tt(e, a), ht(r, e, a, -1));
    }
    return Ro(), r = Ul(Error(I(421))), ua(e, t, o, r);
  }
  return a.data === "$?" ? (t.flags |= 128, t.child = e.child, t = qp.bind(null, e), a._reactRetry = t, null) : (e = i.treeContext, Xe = qt(a.nextSibling), Ze = t, oe = !0, pt = null, e !== null && (rt[at++] = It, rt[at++] = Pt, rt[at++] = gn, It = e.id, Pt = e.overflow, gn = t), t = ko(t, r.children), t.flags |= 4096, t);
}
function Ws(e, t, n) {
  e.lanes |= t;
  var r = e.alternate;
  r !== null && (r.lanes |= t), wi(e.return, t, n);
}
function Vl(e, t, n, r, a) {
  var i = e.memoizedState;
  i === null ? e.memoizedState = { isBackwards: t, rendering: null, renderingStartTime: 0, last: r, tail: n, tailMode: a } : (i.isBackwards = t, i.rendering = null, i.renderingStartTime = 0, i.last = r, i.tail = n, i.tailMode = a);
}
function Xu(e, t, n) {
  var r = t.pendingProps, a = r.revealOrder, i = r.tail;
  if (Le(e, t, r.children, n), r = ce.current, r & 2) r = r & 1 | 2, t.flags |= 128;
  else {
    if (e !== null && e.flags & 128) e: for (e = t.child; e !== null; ) {
      if (e.tag === 13) e.memoizedState !== null && Ws(e, n, t);
      else if (e.tag === 19) Ws(e, n, t);
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
  if (re(ce, r), !(t.mode & 1)) t.memoizedState = null;
  else switch (a) {
    case "forwards":
      for (n = t.child, a = null; n !== null; ) e = n.alternate, e !== null && Va(e) === null && (a = n), n = n.sibling;
      n = a, n === null ? (a = t.child, t.child = null) : (a = n.sibling, n.sibling = null), Vl(t, !1, a, n, i);
      break;
    case "backwards":
      for (n = null, a = t.child, t.child = null; a !== null; ) {
        if (e = a.alternate, e !== null && Va(e) === null) {
          t.child = a;
          break;
        }
        e = a.sibling, a.sibling = n, n = a, a = e;
      }
      Vl(t, !0, n, null, i);
      break;
    case "together":
      Vl(t, !1, null, null, void 0);
      break;
    default:
      t.memoizedState = null;
  }
  return t.child;
}
function Sa(e, t) {
  !(t.mode & 1) && e !== null && (e.alternate = null, t.alternate = null, t.flags |= 2);
}
function zt(e, t, n) {
  if (e !== null && (t.dependencies = e.dependencies), jn |= t.lanes, !(n & t.childLanes)) return null;
  if (e !== null && t.child !== e.child) throw Error(I(153));
  if (t.child !== null) {
    for (e = t.child, n = Jt(e, e.pendingProps), t.child = n, n.return = t; e.sibling !== null; ) e = e.sibling, n = n.sibling = Jt(e, e.pendingProps), n.return = t;
    n.sibling = null;
  }
  return t.child;
}
function Mp(e, t, n) {
  switch (t.tag) {
    case 3:
      qu(t), Kn();
      break;
    case 5:
      wu(t);
      break;
    case 1:
      We(t.type) && Aa(t);
      break;
    case 4:
      vo(t, t.stateNode.containerInfo);
      break;
    case 10:
      var r = t.type._context, a = t.memoizedProps.value;
      re(Oa, r._currentValue), r._currentValue = a;
      break;
    case 13:
      if (r = t.memoizedState, r !== null)
        return r.dehydrated !== null ? (re(ce, ce.current & 1), t.flags |= 128, null) : n & t.child.childLanes ? Yu(e, t, n) : (re(ce, ce.current & 1), e = zt(e, t, n), e !== null ? e.sibling : null);
      re(ce, ce.current & 1);
      break;
    case 19:
      if (r = (n & t.childLanes) !== 0, e.flags & 128) {
        if (r) return Xu(e, t, n);
        t.flags |= 128;
      }
      if (a = t.memoizedState, a !== null && (a.rendering = null, a.tail = null, a.lastEffect = null), re(ce, ce.current), r) break;
      return null;
    case 22:
    case 23:
      return t.lanes = 0, Gu(e, t, n);
  }
  return zt(e, t, n);
}
var Zu, _i, Ju, ed;
Zu = function(e, t) {
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
_i = function() {
};
Ju = function(e, t, n, r) {
  var a = e.memoizedProps;
  if (a !== r) {
    e = t.stateNode, pn(wt.current);
    var i = null;
    switch (n) {
      case "input":
        a = Jl(e, a), r = Jl(e, r), i = [];
        break;
      case "select":
        a = de({}, a, { value: void 0 }), r = de({}, r, { value: void 0 }), i = [];
        break;
      case "textarea":
        a = ni(e, a), r = ni(e, r), i = [];
        break;
      default:
        typeof a.onClick != "function" && typeof r.onClick == "function" && (e.onclick = Da);
    }
    ai(n, r);
    var o;
    n = null;
    for (d in a) if (!r.hasOwnProperty(d) && a.hasOwnProperty(d) && a[d] != null) if (d === "style") {
      var s = a[d];
      for (o in s) s.hasOwnProperty(o) && (n || (n = {}), n[o] = "");
    } else d !== "dangerouslySetInnerHTML" && d !== "children" && d !== "suppressContentEditableWarning" && d !== "suppressHydrationWarning" && d !== "autoFocus" && (Cr.hasOwnProperty(d) ? i || (i = []) : (i = i || []).push(d, null));
    for (d in r) {
      var c = r[d];
      if (s = a != null ? a[d] : void 0, r.hasOwnProperty(d) && c !== s && (c != null || s != null)) if (d === "style") if (s) {
        for (o in s) !s.hasOwnProperty(o) || c && c.hasOwnProperty(o) || (n || (n = {}), n[o] = "");
        for (o in c) c.hasOwnProperty(o) && s[o] !== c[o] && (n || (n = {}), n[o] = c[o]);
      } else n || (i || (i = []), i.push(
        d,
        n
      )), n = c;
      else d === "dangerouslySetInnerHTML" ? (c = c ? c.__html : void 0, s = s ? s.__html : void 0, c != null && s !== c && (i = i || []).push(d, c)) : d === "children" ? typeof c != "string" && typeof c != "number" || (i = i || []).push(d, "" + c) : d !== "suppressContentEditableWarning" && d !== "suppressHydrationWarning" && (Cr.hasOwnProperty(d) ? (c != null && d === "onScroll" && le("scroll", e), i || s === c || (i = [])) : (i = i || []).push(d, c));
    }
    n && (i = i || []).push("style", n);
    var d = i;
    (t.updateQueue = d) && (t.flags |= 4);
  }
};
ed = function(e, t, n, r) {
  n !== r && (t.flags |= 4);
};
function cr(e, t) {
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
function $p(e, t, n) {
  var r = t.pendingProps;
  switch (so(t), t.tag) {
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
      return We(t.type) && La(), Re(t), null;
    case 3:
      return r = t.stateNode, Yn(), ie(He), ie(ze), go(), r.pendingContext && (r.context = r.pendingContext, r.pendingContext = null), (e === null || e.child === null) && (sa(t) ? t.flags |= 4 : e === null || e.memoizedState.isDehydrated && !(t.flags & 256) || (t.flags |= 1024, pt !== null && (Oi(pt), pt = null))), _i(e, t), Re(t), null;
    case 5:
      xo(t);
      var a = pn(Mr.current);
      if (n = t.type, e !== null && t.stateNode != null) Ju(e, t, n, r, a), e.ref !== t.ref && (t.flags |= 512, t.flags |= 2097152);
      else {
        if (!r) {
          if (t.stateNode === null) throw Error(I(166));
          return Re(t), null;
        }
        if (e = pn(wt.current), sa(t)) {
          r = t.stateNode, n = t.type;
          var i = t.memoizedProps;
          switch (r[Nt] = t, r[Lr] = i, e = (t.mode & 1) !== 0, n) {
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
              for (a = 0; a < mr.length; a++) le(mr[a], r);
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
              es(r, i), le("invalid", r);
              break;
            case "select":
              r._wrapperState = { wasMultiple: !!i.multiple }, le("invalid", r);
              break;
            case "textarea":
              ns(r, i), le("invalid", r);
          }
          ai(n, i), a = null;
          for (var o in i) if (i.hasOwnProperty(o)) {
            var s = i[o];
            o === "children" ? typeof s == "string" ? r.textContent !== s && (i.suppressHydrationWarning !== !0 && oa(r.textContent, s, e), a = ["children", s]) : typeof s == "number" && r.textContent !== "" + s && (i.suppressHydrationWarning !== !0 && oa(
              r.textContent,
              s,
              e
            ), a = ["children", "" + s]) : Cr.hasOwnProperty(o) && s != null && o === "onScroll" && le("scroll", r);
          }
          switch (n) {
            case "input":
              Jr(r), ts(r, i, !0);
              break;
            case "textarea":
              Jr(r), rs(r);
              break;
            case "select":
            case "option":
              break;
            default:
              typeof i.onClick == "function" && (r.onclick = Da);
          }
          r = a, t.updateQueue = r, r !== null && (t.flags |= 4);
        } else {
          o = a.nodeType === 9 ? a : a.ownerDocument, e === "http://www.w3.org/1999/xhtml" && (e = Pc(n)), e === "http://www.w3.org/1999/xhtml" ? n === "script" ? (e = o.createElement("div"), e.innerHTML = "<script><\/script>", e = e.removeChild(e.firstChild)) : typeof r.is == "string" ? e = o.createElement(n, { is: r.is }) : (e = o.createElement(n), n === "select" && (o = e, r.multiple ? o.multiple = !0 : r.size && (o.size = r.size))) : e = o.createElementNS(e, n), e[Nt] = t, e[Lr] = r, Zu(e, t, !1, !1), t.stateNode = e;
          e: {
            switch (o = li(n, r), n) {
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
                for (a = 0; a < mr.length; a++) le(mr[a], e);
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
                es(e, r), a = Jl(e, r), le("invalid", e);
                break;
              case "option":
                a = r;
                break;
              case "select":
                e._wrapperState = { wasMultiple: !!r.multiple }, a = de({}, r, { value: void 0 }), le("invalid", e);
                break;
              case "textarea":
                ns(e, r), a = ni(e, r), le("invalid", e);
                break;
              default:
                a = r;
            }
            ai(n, a), s = a;
            for (i in s) if (s.hasOwnProperty(i)) {
              var c = s[i];
              i === "style" ? _c(e, c) : i === "dangerouslySetInnerHTML" ? (c = c ? c.__html : void 0, c != null && Fc(e, c)) : i === "children" ? typeof c == "string" ? (n !== "textarea" || c !== "") && Er(e, c) : typeof c == "number" && Er(e, "" + c) : i !== "suppressContentEditableWarning" && i !== "suppressHydrationWarning" && i !== "autoFocus" && (Cr.hasOwnProperty(i) ? c != null && i === "onScroll" && le("scroll", e) : c != null && Gi(e, i, c, o));
            }
            switch (n) {
              case "input":
                Jr(e), ts(e, r, !1);
                break;
              case "textarea":
                Jr(e), rs(e);
                break;
              case "option":
                r.value != null && e.setAttribute("value", "" + en(r.value));
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
                typeof a.onClick == "function" && (e.onclick = Da);
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
      if (e && t.stateNode != null) ed(e, t, e.memoizedProps, r);
      else {
        if (typeof r != "string" && t.stateNode === null) throw Error(I(166));
        if (n = pn(Mr.current), pn(wt.current), sa(t)) {
          if (r = t.stateNode, n = t.memoizedProps, r[Nt] = t, (i = r.nodeValue !== n) && (e = Ze, e !== null)) switch (e.tag) {
            case 3:
              oa(r.nodeValue, n, (e.mode & 1) !== 0);
              break;
            case 5:
              e.memoizedProps.suppressHydrationWarning !== !0 && oa(r.nodeValue, n, (e.mode & 1) !== 0);
          }
          i && (t.flags |= 4);
        } else r = (n.nodeType === 9 ? n : n.ownerDocument).createTextNode(r), r[Nt] = t, t.stateNode = r;
      }
      return Re(t), null;
    case 13:
      if (ie(ce), r = t.memoizedState, e === null || e.memoizedState !== null && e.memoizedState.dehydrated !== null) {
        if (oe && Xe !== null && t.mode & 1 && !(t.flags & 128)) gu(), Kn(), t.flags |= 98560, i = !1;
        else if (i = sa(t), r !== null && r.dehydrated !== null) {
          if (e === null) {
            if (!i) throw Error(I(318));
            if (i = t.memoizedState, i = i !== null ? i.dehydrated : null, !i) throw Error(I(317));
            i[Nt] = t;
          } else Kn(), !(t.flags & 128) && (t.memoizedState = null), t.flags |= 4;
          Re(t), i = !1;
        } else pt !== null && (Oi(pt), pt = null), i = !0;
        if (!i) return t.flags & 65536 ? t : null;
      }
      return t.flags & 128 ? (t.lanes = n, t) : (r = r !== null, r !== (e !== null && e.memoizedState !== null) && r && (t.child.flags |= 8192, t.mode & 1 && (e === null || ce.current & 1 ? Ne === 0 && (Ne = 3) : Ro())), t.updateQueue !== null && (t.flags |= 4), Re(t), null);
    case 4:
      return Yn(), _i(e, t), e === null && zr(t.stateNode.containerInfo), Re(t), null;
    case 10:
      return po(t.type._context), Re(t), null;
    case 17:
      return We(t.type) && La(), Re(t), null;
    case 19:
      if (ie(ce), i = t.memoizedState, i === null) return Re(t), null;
      if (r = (t.flags & 128) !== 0, o = i.rendering, o === null) if (r) cr(i, !1);
      else {
        if (Ne !== 0 || e !== null && e.flags & 128) for (e = t.child; e !== null; ) {
          if (o = Va(e), o !== null) {
            for (t.flags |= 128, cr(i, !1), r = o.updateQueue, r !== null && (t.updateQueue = r, t.flags |= 4), t.subtreeFlags = 0, r = n, n = t.child; n !== null; ) i = n, e = r, i.flags &= 14680066, o = i.alternate, o === null ? (i.childLanes = 0, i.lanes = e, i.child = null, i.subtreeFlags = 0, i.memoizedProps = null, i.memoizedState = null, i.updateQueue = null, i.dependencies = null, i.stateNode = null) : (i.childLanes = o.childLanes, i.lanes = o.lanes, i.child = o.child, i.subtreeFlags = 0, i.deletions = null, i.memoizedProps = o.memoizedProps, i.memoizedState = o.memoizedState, i.updateQueue = o.updateQueue, i.type = o.type, e = o.dependencies, i.dependencies = e === null ? null : { lanes: e.lanes, firstContext: e.firstContext }), n = n.sibling;
            return re(ce, ce.current & 1 | 2), t.child;
          }
          e = e.sibling;
        }
        i.tail !== null && ve() > Zn && (t.flags |= 128, r = !0, cr(i, !1), t.lanes = 4194304);
      }
      else {
        if (!r) if (e = Va(o), e !== null) {
          if (t.flags |= 128, r = !0, n = e.updateQueue, n !== null && (t.updateQueue = n, t.flags |= 4), cr(i, !0), i.tail === null && i.tailMode === "hidden" && !o.alternate && !oe) return Re(t), null;
        } else 2 * ve() - i.renderingStartTime > Zn && n !== 1073741824 && (t.flags |= 128, r = !0, cr(i, !1), t.lanes = 4194304);
        i.isBackwards ? (o.sibling = t.child, t.child = o) : (n = i.last, n !== null ? n.sibling = o : t.child = o, i.last = o);
      }
      return i.tail !== null ? (t = i.tail, i.rendering = t, i.tail = t.sibling, i.renderingStartTime = ve(), t.sibling = null, n = ce.current, re(ce, r ? n & 1 | 2 : n & 1), t) : (Re(t), null);
    case 22:
    case 23:
      return Fo(), r = t.memoizedState !== null, e !== null && e.memoizedState !== null !== r && (t.flags |= 8192), r && t.mode & 1 ? Ye & 1073741824 && (Re(t), t.subtreeFlags & 6 && (t.flags |= 8192)) : Re(t), null;
    case 24:
      return null;
    case 25:
      return null;
  }
  throw Error(I(156, t.tag));
}
function Op(e, t) {
  switch (so(t), t.tag) {
    case 1:
      return We(t.type) && La(), e = t.flags, e & 65536 ? (t.flags = e & -65537 | 128, t) : null;
    case 3:
      return Yn(), ie(He), ie(ze), go(), e = t.flags, e & 65536 && !(e & 128) ? (t.flags = e & -65537 | 128, t) : null;
    case 5:
      return xo(t), null;
    case 13:
      if (ie(ce), e = t.memoizedState, e !== null && e.dehydrated !== null) {
        if (t.alternate === null) throw Error(I(340));
        Kn();
      }
      return e = t.flags, e & 65536 ? (t.flags = e & -65537 | 128, t) : null;
    case 19:
      return ie(ce), null;
    case 4:
      return Yn(), null;
    case 10:
      return po(t.type._context), null;
    case 22:
    case 23:
      return Fo(), null;
    case 24:
      return null;
    default:
      return null;
  }
}
var da = !1, _e = !1, bp = typeof WeakSet == "function" ? WeakSet : Set, b = null;
function Mn(e, t) {
  var n = e.ref;
  if (n !== null) if (typeof n == "function") try {
    n(null);
  } catch (r) {
    pe(e, t, r);
  }
  else n.current = null;
}
function Ti(e, t, n) {
  try {
    n();
  } catch (r) {
    pe(e, t, r);
  }
}
var Qs = !1;
function Up(e, t) {
  if (hi = _a, e = lu(), io(e)) {
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
  for (vi = { focusedElem: e, selectionRange: n }, _a = !1, b = t; b !== null; ) if (t = b, e = t.child, (t.subtreeFlags & 1028) !== 0 && e !== null) e.return = t, b = e;
  else for (; b !== null; ) {
    t = b;
    try {
      var y = t.alternate;
      if (t.flags & 1024) switch (t.tag) {
        case 0:
        case 11:
        case 15:
          break;
        case 1:
          if (y !== null) {
            var S = y.memoizedProps, O = y.memoizedState, p = t.stateNode, f = p.getSnapshotBeforeUpdate(t.elementType === t.type ? S : dt(t.type, S), O);
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
          throw Error(I(163));
      }
    } catch (C) {
      pe(t, t.return, C);
    }
    if (e = t.sibling, e !== null) {
      e.return = t.return, b = e;
      break;
    }
    b = t.return;
  }
  return y = Qs, Qs = !1, y;
}
function Nr(e, t, n) {
  var r = t.updateQueue;
  if (r = r !== null ? r.lastEffect : null, r !== null) {
    var a = r = r.next;
    do {
      if ((a.tag & e) === e) {
        var i = a.destroy;
        a.destroy = void 0, i !== void 0 && Ti(t, n, i);
      }
      a = a.next;
    } while (a !== r);
  }
}
function ll(e, t) {
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
function zi(e) {
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
function td(e) {
  var t = e.alternate;
  t !== null && (e.alternate = null, td(t)), e.child = null, e.deletions = null, e.sibling = null, e.tag === 5 && (t = e.stateNode, t !== null && (delete t[Nt], delete t[Lr], delete t[yi], delete t[wp], delete t[kp])), e.stateNode = null, e.return = null, e.dependencies = null, e.memoizedProps = null, e.memoizedState = null, e.pendingProps = null, e.stateNode = null, e.updateQueue = null;
}
function nd(e) {
  return e.tag === 5 || e.tag === 3 || e.tag === 4;
}
function Gs(e) {
  e: for (; ; ) {
    for (; e.sibling === null; ) {
      if (e.return === null || nd(e.return)) return null;
      e = e.return;
    }
    for (e.sibling.return = e.return, e = e.sibling; e.tag !== 5 && e.tag !== 6 && e.tag !== 18; ) {
      if (e.flags & 2 || e.child === null || e.tag === 4) continue e;
      e.child.return = e, e = e.child;
    }
    if (!(e.flags & 2)) return e.stateNode;
  }
}
function Di(e, t, n) {
  var r = e.tag;
  if (r === 5 || r === 6) e = e.stateNode, t ? n.nodeType === 8 ? n.parentNode.insertBefore(e, t) : n.insertBefore(e, t) : (n.nodeType === 8 ? (t = n.parentNode, t.insertBefore(e, n)) : (t = n, t.appendChild(e)), n = n._reactRootContainer, n != null || t.onclick !== null || (t.onclick = Da));
  else if (r !== 4 && (e = e.child, e !== null)) for (Di(e, t, n), e = e.sibling; e !== null; ) Di(e, t, n), e = e.sibling;
}
function Li(e, t, n) {
  var r = e.tag;
  if (r === 5 || r === 6) e = e.stateNode, t ? n.insertBefore(e, t) : n.appendChild(e);
  else if (r !== 4 && (e = e.child, e !== null)) for (Li(e, t, n), e = e.sibling; e !== null; ) Li(e, t, n), e = e.sibling;
}
var Ce = null, ft = !1;
function Ot(e, t, n) {
  for (n = n.child; n !== null; ) rd(e, t, n), n = n.sibling;
}
function rd(e, t, n) {
  if (St && typeof St.onCommitFiberUnmount == "function") try {
    St.onCommitFiberUnmount(Xa, n);
  } catch {
  }
  switch (n.tag) {
    case 5:
      _e || Mn(n, t);
    case 6:
      var r = Ce, a = ft;
      Ce = null, Ot(e, t, n), Ce = r, ft = a, Ce !== null && (ft ? (e = Ce, n = n.stateNode, e.nodeType === 8 ? e.parentNode.removeChild(n) : e.removeChild(n)) : Ce.removeChild(n.stateNode));
      break;
    case 18:
      Ce !== null && (ft ? (e = Ce, n = n.stateNode, e.nodeType === 8 ? Ll(e.parentNode, n) : e.nodeType === 1 && Ll(e, n), Rr(e)) : Ll(Ce, n.stateNode));
      break;
    case 4:
      r = Ce, a = ft, Ce = n.stateNode.containerInfo, ft = !0, Ot(e, t, n), Ce = r, ft = a;
      break;
    case 0:
    case 11:
    case 14:
    case 15:
      if (!_e && (r = n.updateQueue, r !== null && (r = r.lastEffect, r !== null))) {
        a = r = r.next;
        do {
          var i = a, o = i.destroy;
          i = i.tag, o !== void 0 && (i & 2 || i & 4) && Ti(n, t, o), a = a.next;
        } while (a !== r);
      }
      Ot(e, t, n);
      break;
    case 1:
      if (!_e && (Mn(n, t), r = n.stateNode, typeof r.componentWillUnmount == "function")) try {
        r.props = n.memoizedProps, r.state = n.memoizedState, r.componentWillUnmount();
      } catch (s) {
        pe(n, t, s);
      }
      Ot(e, t, n);
      break;
    case 21:
      Ot(e, t, n);
      break;
    case 22:
      n.mode & 1 ? (_e = (r = _e) || n.memoizedState !== null, Ot(e, t, n), _e = r) : Ot(e, t, n);
      break;
    default:
      Ot(e, t, n);
  }
}
function Ks(e) {
  var t = e.updateQueue;
  if (t !== null) {
    e.updateQueue = null;
    var n = e.stateNode;
    n === null && (n = e.stateNode = new bp()), t.forEach(function(r) {
      var a = Yp.bind(null, e, r);
      n.has(r) || (n.add(r), r.then(a, a));
    });
  }
}
function ut(e, t) {
  var n = t.deletions;
  if (n !== null) for (var r = 0; r < n.length; r++) {
    var a = n[r];
    try {
      var i = e, o = t, s = o;
      e: for (; s !== null; ) {
        switch (s.tag) {
          case 5:
            Ce = s.stateNode, ft = !1;
            break e;
          case 3:
            Ce = s.stateNode.containerInfo, ft = !0;
            break e;
          case 4:
            Ce = s.stateNode.containerInfo, ft = !0;
            break e;
        }
        s = s.return;
      }
      if (Ce === null) throw Error(I(160));
      rd(i, o, a), Ce = null, ft = !1;
      var c = a.alternate;
      c !== null && (c.return = null), a.return = null;
    } catch (d) {
      pe(a, t, d);
    }
  }
  if (t.subtreeFlags & 12854) for (t = t.child; t !== null; ) ad(t, e), t = t.sibling;
}
function ad(e, t) {
  var n = e.alternate, r = e.flags;
  switch (e.tag) {
    case 0:
    case 11:
    case 14:
    case 15:
      if (ut(t, e), yt(e), r & 4) {
        try {
          Nr(3, e, e.return), ll(3, e);
        } catch (S) {
          pe(e, e.return, S);
        }
        try {
          Nr(5, e, e.return);
        } catch (S) {
          pe(e, e.return, S);
        }
      }
      break;
    case 1:
      ut(t, e), yt(e), r & 512 && n !== null && Mn(n, n.return);
      break;
    case 5:
      if (ut(t, e), yt(e), r & 512 && n !== null && Mn(n, n.return), e.flags & 32) {
        var a = e.stateNode;
        try {
          Er(a, "");
        } catch (S) {
          pe(e, e.return, S);
        }
      }
      if (r & 4 && (a = e.stateNode, a != null)) {
        var i = e.memoizedProps, o = n !== null ? n.memoizedProps : i, s = e.type, c = e.updateQueue;
        if (e.updateQueue = null, c !== null) try {
          s === "input" && i.type === "radio" && i.name != null && Ec(a, i), li(s, o);
          var d = li(s, i);
          for (o = 0; o < c.length; o += 2) {
            var N = c[o], u = c[o + 1];
            N === "style" ? _c(a, u) : N === "dangerouslySetInnerHTML" ? Fc(a, u) : N === "children" ? Er(a, u) : Gi(a, N, u, d);
          }
          switch (s) {
            case "input":
              ei(a, i);
              break;
            case "textarea":
              Ic(a, i);
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
          a[Lr] = i;
        } catch (S) {
          pe(e, e.return, S);
        }
      }
      break;
    case 6:
      if (ut(t, e), yt(e), r & 4) {
        if (e.stateNode === null) throw Error(I(162));
        a = e.stateNode, i = e.memoizedProps;
        try {
          a.nodeValue = i;
        } catch (S) {
          pe(e, e.return, S);
        }
      }
      break;
    case 3:
      if (ut(t, e), yt(e), r & 4 && n !== null && n.memoizedState.isDehydrated) try {
        Rr(t.containerInfo);
      } catch (S) {
        pe(e, e.return, S);
      }
      break;
    case 4:
      ut(t, e), yt(e);
      break;
    case 13:
      ut(t, e), yt(e), a = e.child, a.flags & 8192 && (i = a.memoizedState !== null, a.stateNode.isHidden = i, !i || a.alternate !== null && a.alternate.memoizedState !== null || (Io = ve())), r & 4 && Ks(e);
      break;
    case 22:
      if (N = n !== null && n.memoizedState !== null, e.mode & 1 ? (_e = (d = _e) || N, ut(t, e), _e = d) : ut(t, e), yt(e), r & 8192) {
        if (d = e.memoizedState !== null, (e.stateNode.isHidden = d) && !N && e.mode & 1) for (b = e, N = e.child; N !== null; ) {
          for (u = b = N; b !== null; ) {
            switch (h = b, v = h.child, h.tag) {
              case 0:
              case 11:
              case 14:
              case 15:
                Nr(4, h, h.return);
                break;
              case 1:
                Mn(h, h.return);
                var y = h.stateNode;
                if (typeof y.componentWillUnmount == "function") {
                  r = h, n = h.return;
                  try {
                    t = r, y.props = t.memoizedProps, y.state = t.memoizedState, y.componentWillUnmount();
                  } catch (S) {
                    pe(r, n, S);
                  }
                }
                break;
              case 5:
                Mn(h, h.return);
                break;
              case 22:
                if (h.memoizedState !== null) {
                  Ys(u);
                  continue;
                }
            }
            v !== null ? (v.return = h, b = v) : Ys(u);
          }
          N = N.sibling;
        }
        e: for (N = null, u = e; ; ) {
          if (u.tag === 5) {
            if (N === null) {
              N = u;
              try {
                a = u.stateNode, d ? (i = a.style, typeof i.setProperty == "function" ? i.setProperty("display", "none", "important") : i.display = "none") : (s = u.stateNode, c = u.memoizedProps.style, o = c != null && c.hasOwnProperty("display") ? c.display : null, s.style.display = Rc("display", o));
              } catch (S) {
                pe(e, e.return, S);
              }
            }
          } else if (u.tag === 6) {
            if (N === null) try {
              u.stateNode.nodeValue = d ? "" : u.memoizedProps;
            } catch (S) {
              pe(e, e.return, S);
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
      ut(t, e), yt(e), r & 4 && Ks(e);
      break;
    case 21:
      break;
    default:
      ut(
        t,
        e
      ), yt(e);
  }
}
function yt(e) {
  var t = e.flags;
  if (t & 2) {
    try {
      e: {
        for (var n = e.return; n !== null; ) {
          if (nd(n)) {
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
          r.flags & 32 && (Er(a, ""), r.flags &= -33);
          var i = Gs(e);
          Li(e, i, a);
          break;
        case 3:
        case 4:
          var o = r.stateNode.containerInfo, s = Gs(e);
          Di(e, s, o);
          break;
        default:
          throw Error(I(161));
      }
    } catch (c) {
      pe(e, e.return, c);
    }
    e.flags &= -3;
  }
  t & 4096 && (e.flags &= -4097);
}
function Vp(e, t, n) {
  b = e, ld(e);
}
function ld(e, t, n) {
  for (var r = (e.mode & 1) !== 0; b !== null; ) {
    var a = b, i = a.child;
    if (a.tag === 22 && r) {
      var o = a.memoizedState !== null || da;
      if (!o) {
        var s = a.alternate, c = s !== null && s.memoizedState !== null || _e;
        s = da;
        var d = _e;
        if (da = o, (_e = c) && !d) for (b = a; b !== null; ) o = b, c = o.child, o.tag === 22 && o.memoizedState !== null ? Xs(a) : c !== null ? (c.return = o, b = c) : Xs(a);
        for (; i !== null; ) b = i, ld(i), i = i.sibling;
        b = a, da = s, _e = d;
      }
      qs(e);
    } else a.subtreeFlags & 8772 && i !== null ? (i.return = a, b = i) : qs(e);
  }
}
function qs(e) {
  for (; b !== null; ) {
    var t = b;
    if (t.flags & 8772) {
      var n = t.alternate;
      try {
        if (t.flags & 8772) switch (t.tag) {
          case 0:
          case 11:
          case 15:
            _e || ll(5, t);
            break;
          case 1:
            var r = t.stateNode;
            if (t.flags & 4 && !_e) if (n === null) r.componentDidMount();
            else {
              var a = t.elementType === t.type ? n.memoizedProps : dt(t.type, n.memoizedProps);
              r.componentDidUpdate(a, n.memoizedState, r.__reactInternalSnapshotBeforeUpdate);
            }
            var i = t.updateQueue;
            i !== null && zs(t, i, r);
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
              zs(t, o, n);
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
                  u !== null && Rr(u);
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
        _e || t.flags & 512 && zi(t);
      } catch (h) {
        pe(t, t.return, h);
      }
    }
    if (t === e) {
      b = null;
      break;
    }
    if (n = t.sibling, n !== null) {
      n.return = t.return, b = n;
      break;
    }
    b = t.return;
  }
}
function Ys(e) {
  for (; b !== null; ) {
    var t = b;
    if (t === e) {
      b = null;
      break;
    }
    var n = t.sibling;
    if (n !== null) {
      n.return = t.return, b = n;
      break;
    }
    b = t.return;
  }
}
function Xs(e) {
  for (; b !== null; ) {
    var t = b;
    try {
      switch (t.tag) {
        case 0:
        case 11:
        case 15:
          var n = t.return;
          try {
            ll(4, t);
          } catch (c) {
            pe(t, n, c);
          }
          break;
        case 1:
          var r = t.stateNode;
          if (typeof r.componentDidMount == "function") {
            var a = t.return;
            try {
              r.componentDidMount();
            } catch (c) {
              pe(t, a, c);
            }
          }
          var i = t.return;
          try {
            zi(t);
          } catch (c) {
            pe(t, i, c);
          }
          break;
        case 5:
          var o = t.return;
          try {
            zi(t);
          } catch (c) {
            pe(t, o, c);
          }
      }
    } catch (c) {
      pe(t, t.return, c);
    }
    if (t === e) {
      b = null;
      break;
    }
    var s = t.sibling;
    if (s !== null) {
      s.return = t.return, b = s;
      break;
    }
    b = t.return;
  }
}
var Bp = Math.ceil, Wa = Dt.ReactCurrentDispatcher, Co = Dt.ReactCurrentOwner, ot = Dt.ReactCurrentBatchConfig, Y = 0, ke = null, ge = null, Ee = 0, Ye = 0, $n = an(0), Ne = 0, Ur = null, jn = 0, il = 0, Eo = 0, Sr = null, Ve = null, Io = 0, Zn = 1 / 0, Ct = null, Qa = !1, Ai = null, Xt = null, fa = !1, Wt = null, Ga = 0, wr = 0, Mi = null, wa = -1, ka = 0;
function $e() {
  return Y & 6 ? ve() : wa !== -1 ? wa : wa = ve();
}
function Zt(e) {
  return e.mode & 1 ? Y & 2 && Ee !== 0 ? Ee & -Ee : Ep.transition !== null ? (ka === 0 && (ka = Bc()), ka) : (e = ee, e !== 0 || (e = window.event, e = e === void 0 ? 16 : Yc(e.type)), e) : 1;
}
function ht(e, t, n, r) {
  if (50 < wr) throw wr = 0, Mi = null, Error(I(185));
  Hr(e, n, r), (!(Y & 2) || e !== ke) && (e === ke && (!(Y & 2) && (il |= n), Ne === 4 && Bt(e, Ee)), Qe(e, r), n === 1 && Y === 0 && !(t.mode & 1) && (Zn = ve() + 500, nl && ln()));
}
function Qe(e, t) {
  var n = e.callbackNode;
  Ef(e, t);
  var r = Ra(e, e === ke ? Ee : 0);
  if (r === 0) n !== null && is(n), e.callbackNode = null, e.callbackPriority = 0;
  else if (t = r & -r, e.callbackPriority !== t) {
    if (n != null && is(n), t === 1) e.tag === 0 ? Cp(Zs.bind(null, e)) : hu(Zs.bind(null, e)), Np(function() {
      !(Y & 6) && ln();
    }), n = null;
    else {
      switch (Hc(r)) {
        case 1:
          n = Zi;
          break;
        case 4:
          n = Uc;
          break;
        case 16:
          n = Fa;
          break;
        case 536870912:
          n = Vc;
          break;
        default:
          n = Fa;
      }
      n = pd(n, id.bind(null, e));
    }
    e.callbackPriority = t, e.callbackNode = n;
  }
}
function id(e, t) {
  if (wa = -1, ka = 0, Y & 6) throw Error(I(327));
  var n = e.callbackNode;
  if (Hn() && e.callbackNode !== n) return null;
  var r = Ra(e, e === ke ? Ee : 0);
  if (r === 0) return null;
  if (r & 30 || r & e.expiredLanes || t) t = Ka(e, r);
  else {
    t = r;
    var a = Y;
    Y |= 2;
    var i = sd();
    (ke !== e || Ee !== t) && (Ct = null, Zn = ve() + 500, mn(e, t));
    do
      try {
        Qp();
        break;
      } catch (s) {
        od(e, s);
      }
    while (!0);
    fo(), Wa.current = i, Y = a, ge !== null ? t = 0 : (ke = null, Ee = 0, t = Ne);
  }
  if (t !== 0) {
    if (t === 2 && (a = ui(e), a !== 0 && (r = a, t = $i(e, a))), t === 1) throw n = Ur, mn(e, 0), Bt(e, r), Qe(e, ve()), n;
    if (t === 6) Bt(e, r);
    else {
      if (a = e.current.alternate, !(r & 30) && !Hp(a) && (t = Ka(e, r), t === 2 && (i = ui(e), i !== 0 && (r = i, t = $i(e, i))), t === 1)) throw n = Ur, mn(e, 0), Bt(e, r), Qe(e, ve()), n;
      switch (e.finishedWork = a, e.finishedLanes = r, t) {
        case 0:
        case 1:
          throw Error(I(345));
        case 2:
          un(e, Ve, Ct);
          break;
        case 3:
          if (Bt(e, r), (r & 130023424) === r && (t = Io + 500 - ve(), 10 < t)) {
            if (Ra(e, 0) !== 0) break;
            if (a = e.suspendedLanes, (a & r) !== r) {
              $e(), e.pingedLanes |= e.suspendedLanes & a;
              break;
            }
            e.timeoutHandle = gi(un.bind(null, e, Ve, Ct), t);
            break;
          }
          un(e, Ve, Ct);
          break;
        case 4:
          if (Bt(e, r), (r & 4194240) === r) break;
          for (t = e.eventTimes, a = -1; 0 < r; ) {
            var o = 31 - mt(r);
            i = 1 << o, o = t[o], o > a && (a = o), r &= ~i;
          }
          if (r = a, r = ve() - r, r = (120 > r ? 120 : 480 > r ? 480 : 1080 > r ? 1080 : 1920 > r ? 1920 : 3e3 > r ? 3e3 : 4320 > r ? 4320 : 1960 * Bp(r / 1960)) - r, 10 < r) {
            e.timeoutHandle = gi(un.bind(null, e, Ve, Ct), r);
            break;
          }
          un(e, Ve, Ct);
          break;
        case 5:
          un(e, Ve, Ct);
          break;
        default:
          throw Error(I(329));
      }
    }
  }
  return Qe(e, ve()), e.callbackNode === n ? id.bind(null, e) : null;
}
function $i(e, t) {
  var n = Sr;
  return e.current.memoizedState.isDehydrated && (mn(e, t).flags |= 256), e = Ka(e, t), e !== 2 && (t = Ve, Ve = n, t !== null && Oi(t)), e;
}
function Oi(e) {
  Ve === null ? Ve = e : Ve.push.apply(Ve, e);
}
function Hp(e) {
  for (var t = e; ; ) {
    if (t.flags & 16384) {
      var n = t.updateQueue;
      if (n !== null && (n = n.stores, n !== null)) for (var r = 0; r < n.length; r++) {
        var a = n[r], i = a.getSnapshot;
        a = a.value;
        try {
          if (!xt(i(), a)) return !1;
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
function Bt(e, t) {
  for (t &= ~Eo, t &= ~il, e.suspendedLanes |= t, e.pingedLanes &= ~t, e = e.expirationTimes; 0 < t; ) {
    var n = 31 - mt(t), r = 1 << n;
    e[n] = -1, t &= ~r;
  }
}
function Zs(e) {
  if (Y & 6) throw Error(I(327));
  Hn();
  var t = Ra(e, 0);
  if (!(t & 1)) return Qe(e, ve()), null;
  var n = Ka(e, t);
  if (e.tag !== 0 && n === 2) {
    var r = ui(e);
    r !== 0 && (t = r, n = $i(e, r));
  }
  if (n === 1) throw n = Ur, mn(e, 0), Bt(e, t), Qe(e, ve()), n;
  if (n === 6) throw Error(I(345));
  return e.finishedWork = e.current.alternate, e.finishedLanes = t, un(e, Ve, Ct), Qe(e, ve()), null;
}
function Po(e, t) {
  var n = Y;
  Y |= 1;
  try {
    return e(t);
  } finally {
    Y = n, Y === 0 && (Zn = ve() + 500, nl && ln());
  }
}
function Nn(e) {
  Wt !== null && Wt.tag === 0 && !(Y & 6) && Hn();
  var t = Y;
  Y |= 1;
  var n = ot.transition, r = ee;
  try {
    if (ot.transition = null, ee = 1, e) return e();
  } finally {
    ee = r, ot.transition = n, Y = t, !(Y & 6) && ln();
  }
}
function Fo() {
  Ye = $n.current, ie($n);
}
function mn(e, t) {
  e.finishedWork = null, e.finishedLanes = 0;
  var n = e.timeoutHandle;
  if (n !== -1 && (e.timeoutHandle = -1, jp(n)), ge !== null) for (n = ge.return; n !== null; ) {
    var r = n;
    switch (so(r), r.tag) {
      case 1:
        r = r.type.childContextTypes, r != null && La();
        break;
      case 3:
        Yn(), ie(He), ie(ze), go();
        break;
      case 5:
        xo(r);
        break;
      case 4:
        Yn();
        break;
      case 13:
        ie(ce);
        break;
      case 19:
        ie(ce);
        break;
      case 10:
        po(r.type._context);
        break;
      case 22:
      case 23:
        Fo();
    }
    n = n.return;
  }
  if (ke = e, ge = e = Jt(e.current, null), Ee = Ye = t, Ne = 0, Ur = null, Eo = il = jn = 0, Ve = Sr = null, fn !== null) {
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
function od(e, t) {
  do {
    var n = ge;
    try {
      if (fo(), ja.current = Ha, Ba) {
        for (var r = ue.memoizedState; r !== null; ) {
          var a = r.queue;
          a !== null && (a.pending = null), r = r.next;
        }
        Ba = !1;
      }
      if (yn = 0, Se = ye = ue = null, jr = !1, $r = 0, Co.current = null, n === null || n.return === null) {
        Ne = 1, Ur = t, ge = null;
        break;
      }
      e: {
        var i = e, o = n.return, s = n, c = t;
        if (t = Ee, s.flags |= 32768, c !== null && typeof c == "object" && typeof c.then == "function") {
          var d = c, N = s, u = N.tag;
          if (!(N.mode & 1) && (u === 0 || u === 11 || u === 15)) {
            var h = N.alternate;
            h ? (N.updateQueue = h.updateQueue, N.memoizedState = h.memoizedState, N.lanes = h.lanes) : (N.updateQueue = null, N.memoizedState = null);
          }
          var v = Os(o);
          if (v !== null) {
            v.flags &= -257, bs(v, o, s, i, t), v.mode & 1 && $s(i, d, t), t = v, c = d;
            var y = t.updateQueue;
            if (y === null) {
              var S = /* @__PURE__ */ new Set();
              S.add(c), t.updateQueue = S;
            } else y.add(c);
            break e;
          } else {
            if (!(t & 1)) {
              $s(i, d, t), Ro();
              break e;
            }
            c = Error(I(426));
          }
        } else if (oe && s.mode & 1) {
          var O = Os(o);
          if (O !== null) {
            !(O.flags & 65536) && (O.flags |= 256), bs(O, o, s, i, t), co(Xn(c, s));
            break e;
          }
        }
        i = c = Xn(c, s), Ne !== 4 && (Ne = 2), Sr === null ? Sr = [i] : Sr.push(i), i = o;
        do {
          switch (i.tag) {
            case 3:
              i.flags |= 65536, t &= -t, i.lanes |= t;
              var p = Hu(i, c, t);
              Ts(i, p);
              break e;
            case 1:
              s = c;
              var f = i.type, x = i.stateNode;
              if (!(i.flags & 128) && (typeof f.getDerivedStateFromError == "function" || x !== null && typeof x.componentDidCatch == "function" && (Xt === null || !Xt.has(x)))) {
                i.flags |= 65536, t &= -t, i.lanes |= t;
                var C = Wu(i, s, t);
                Ts(i, C);
                break e;
              }
          }
          i = i.return;
        } while (i !== null);
      }
      ud(n);
    } catch (_) {
      t = _, ge === n && n !== null && (ge = n = n.return);
      continue;
    }
    break;
  } while (!0);
}
function sd() {
  var e = Wa.current;
  return Wa.current = Ha, e === null ? Ha : e;
}
function Ro() {
  (Ne === 0 || Ne === 3 || Ne === 2) && (Ne = 4), ke === null || !(jn & 268435455) && !(il & 268435455) || Bt(ke, Ee);
}
function Ka(e, t) {
  var n = Y;
  Y |= 2;
  var r = sd();
  (ke !== e || Ee !== t) && (Ct = null, mn(e, t));
  do
    try {
      Wp();
      break;
    } catch (a) {
      od(e, a);
    }
  while (!0);
  if (fo(), Y = n, Wa.current = r, ge !== null) throw Error(I(261));
  return ke = null, Ee = 0, Ne;
}
function Wp() {
  for (; ge !== null; ) cd(ge);
}
function Qp() {
  for (; ge !== null && !xf(); ) cd(ge);
}
function cd(e) {
  var t = fd(e.alternate, e, Ye);
  e.memoizedProps = e.pendingProps, t === null ? ud(e) : ge = t, Co.current = null;
}
function ud(e) {
  var t = e;
  do {
    var n = t.alternate;
    if (e = t.return, t.flags & 32768) {
      if (n = Op(n, t), n !== null) {
        n.flags &= 32767, ge = n;
        return;
      }
      if (e !== null) e.flags |= 32768, e.subtreeFlags = 0, e.deletions = null;
      else {
        Ne = 6, ge = null;
        return;
      }
    } else if (n = $p(n, t, Ye), n !== null) {
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
  var r = ee, a = ot.transition;
  try {
    ot.transition = null, ee = 1, Gp(e, t, n, r);
  } finally {
    ot.transition = a, ee = r;
  }
  return null;
}
function Gp(e, t, n, r) {
  do
    Hn();
  while (Wt !== null);
  if (Y & 6) throw Error(I(327));
  n = e.finishedWork;
  var a = e.finishedLanes;
  if (n === null) return null;
  if (e.finishedWork = null, e.finishedLanes = 0, n === e.current) throw Error(I(177));
  e.callbackNode = null, e.callbackPriority = 0;
  var i = n.lanes | n.childLanes;
  if (If(e, i), e === ke && (ge = ke = null, Ee = 0), !(n.subtreeFlags & 2064) && !(n.flags & 2064) || fa || (fa = !0, pd(Fa, function() {
    return Hn(), null;
  })), i = (n.flags & 15990) !== 0, n.subtreeFlags & 15990 || i) {
    i = ot.transition, ot.transition = null;
    var o = ee;
    ee = 1;
    var s = Y;
    Y |= 4, Co.current = null, Up(e, n), ad(n, e), pp(vi), _a = !!hi, vi = hi = null, e.current = n, Vp(n), gf(), Y = s, ee = o, ot.transition = i;
  } else e.current = n;
  if (fa && (fa = !1, Wt = e, Ga = a), i = e.pendingLanes, i === 0 && (Xt = null), Nf(n.stateNode), Qe(e, ve()), t !== null) for (r = e.onRecoverableError, n = 0; n < t.length; n++) a = t[n], r(a.value, { componentStack: a.stack, digest: a.digest });
  if (Qa) throw Qa = !1, e = Ai, Ai = null, e;
  return Ga & 1 && e.tag !== 0 && Hn(), i = e.pendingLanes, i & 1 ? e === Mi ? wr++ : (wr = 0, Mi = e) : wr = 0, ln(), null;
}
function Hn() {
  if (Wt !== null) {
    var e = Hc(Ga), t = ot.transition, n = ee;
    try {
      if (ot.transition = null, ee = 16 > e ? 16 : e, Wt === null) var r = !1;
      else {
        if (e = Wt, Wt = null, Ga = 0, Y & 6) throw Error(I(331));
        var a = Y;
        for (Y |= 4, b = e.current; b !== null; ) {
          var i = b, o = i.child;
          if (b.flags & 16) {
            var s = i.deletions;
            if (s !== null) {
              for (var c = 0; c < s.length; c++) {
                var d = s[c];
                for (b = d; b !== null; ) {
                  var N = b;
                  switch (N.tag) {
                    case 0:
                    case 11:
                    case 15:
                      Nr(8, N, i);
                  }
                  var u = N.child;
                  if (u !== null) u.return = N, b = u;
                  else for (; b !== null; ) {
                    N = b;
                    var h = N.sibling, v = N.return;
                    if (td(N), N === d) {
                      b = null;
                      break;
                    }
                    if (h !== null) {
                      h.return = v, b = h;
                      break;
                    }
                    b = v;
                  }
                }
              }
              var y = i.alternate;
              if (y !== null) {
                var S = y.child;
                if (S !== null) {
                  y.child = null;
                  do {
                    var O = S.sibling;
                    S.sibling = null, S = O;
                  } while (S !== null);
                }
              }
              b = i;
            }
          }
          if (i.subtreeFlags & 2064 && o !== null) o.return = i, b = o;
          else e: for (; b !== null; ) {
            if (i = b, i.flags & 2048) switch (i.tag) {
              case 0:
              case 11:
              case 15:
                Nr(9, i, i.return);
            }
            var p = i.sibling;
            if (p !== null) {
              p.return = i.return, b = p;
              break e;
            }
            b = i.return;
          }
        }
        var f = e.current;
        for (b = f; b !== null; ) {
          o = b;
          var x = o.child;
          if (o.subtreeFlags & 2064 && x !== null) x.return = o, b = x;
          else e: for (o = f; b !== null; ) {
            if (s = b, s.flags & 2048) try {
              switch (s.tag) {
                case 0:
                case 11:
                case 15:
                  ll(9, s);
              }
            } catch (_) {
              pe(s, s.return, _);
            }
            if (s === o) {
              b = null;
              break e;
            }
            var C = s.sibling;
            if (C !== null) {
              C.return = s.return, b = C;
              break e;
            }
            b = s.return;
          }
        }
        if (Y = a, ln(), St && typeof St.onPostCommitFiberRoot == "function") try {
          St.onPostCommitFiberRoot(Xa, e);
        } catch {
        }
        r = !0;
      }
      return r;
    } finally {
      ee = n, ot.transition = t;
    }
  }
  return !1;
}
function Js(e, t, n) {
  t = Xn(n, t), t = Hu(e, t, 1), e = Yt(e, t, 1), t = $e(), e !== null && (Hr(e, 1, t), Qe(e, t));
}
function pe(e, t, n) {
  if (e.tag === 3) Js(e, e, n);
  else for (; t !== null; ) {
    if (t.tag === 3) {
      Js(t, e, n);
      break;
    } else if (t.tag === 1) {
      var r = t.stateNode;
      if (typeof t.type.getDerivedStateFromError == "function" || typeof r.componentDidCatch == "function" && (Xt === null || !Xt.has(r))) {
        e = Xn(n, e), e = Wu(t, e, 1), t = Yt(t, e, 1), e = $e(), t !== null && (Hr(t, 1, e), Qe(t, e));
        break;
      }
    }
    t = t.return;
  }
}
function Kp(e, t, n) {
  var r = e.pingCache;
  r !== null && r.delete(t), t = $e(), e.pingedLanes |= e.suspendedLanes & n, ke === e && (Ee & n) === n && (Ne === 4 || Ne === 3 && (Ee & 130023424) === Ee && 500 > ve() - Io ? mn(e, 0) : Eo |= n), Qe(e, t);
}
function dd(e, t) {
  t === 0 && (e.mode & 1 ? (t = na, na <<= 1, !(na & 130023424) && (na = 4194304)) : t = 1);
  var n = $e();
  e = Tt(e, t), e !== null && (Hr(e, t, n), Qe(e, n));
}
function qp(e) {
  var t = e.memoizedState, n = 0;
  t !== null && (n = t.retryLane), dd(e, n);
}
function Yp(e, t) {
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
  r !== null && r.delete(t), dd(e, n);
}
var fd;
fd = function(e, t, n) {
  if (e !== null) if (e.memoizedProps !== t.pendingProps || He.current) Be = !0;
  else {
    if (!(e.lanes & n) && !(t.flags & 128)) return Be = !1, Mp(e, t, n);
    Be = !!(e.flags & 131072);
  }
  else Be = !1, oe && t.flags & 1048576 && vu(t, $a, t.index);
  switch (t.lanes = 0, t.tag) {
    case 2:
      var r = t.type;
      Sa(e, t), e = t.pendingProps;
      var a = Gn(t, ze.current);
      Bn(t, n), a = jo(null, t, r, e, a, n);
      var i = No();
      return t.flags |= 1, typeof a == "object" && a !== null && typeof a.render == "function" && a.$$typeof === void 0 ? (t.tag = 1, t.memoizedState = null, t.updateQueue = null, We(r) ? (i = !0, Aa(t)) : i = !1, t.memoizedState = a.state !== null && a.state !== void 0 ? a.state : null, ho(t), a.updater = al, t.stateNode = a, a._reactInternals = t, Ci(t, r, e, n), t = Pi(null, t, r, !0, i, n)) : (t.tag = 0, oe && i && oo(t), Le(null, t, a, n), t = t.child), t;
    case 16:
      r = t.elementType;
      e: {
        switch (Sa(e, t), e = t.pendingProps, a = r._init, r = a(r._payload), t.type = r, a = t.tag = Zp(r), e = dt(r, e), a) {
          case 0:
            t = Ii(null, t, r, e, n);
            break e;
          case 1:
            t = Bs(null, t, r, e, n);
            break e;
          case 11:
            t = Us(null, t, r, e, n);
            break e;
          case 14:
            t = Vs(null, t, r, dt(r.type, e), n);
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
      return r = t.type, a = t.pendingProps, a = t.elementType === r ? a : dt(r, a), Ii(e, t, r, a, n);
    case 1:
      return r = t.type, a = t.pendingProps, a = t.elementType === r ? a : dt(r, a), Bs(e, t, r, a, n);
    case 3:
      e: {
        if (qu(t), e === null) throw Error(I(387));
        r = t.pendingProps, i = t.memoizedState, a = i.element, Su(e, t), Ua(t, r, null, n);
        var o = t.memoizedState;
        if (r = o.element, i.isDehydrated) if (i = { element: r, isDehydrated: !1, cache: o.cache, pendingSuspenseBoundaries: o.pendingSuspenseBoundaries, transitions: o.transitions }, t.updateQueue.baseState = i, t.memoizedState = i, t.flags & 256) {
          a = Xn(Error(I(423)), t), t = Hs(e, t, r, n, a);
          break e;
        } else if (r !== a) {
          a = Xn(Error(I(424)), t), t = Hs(e, t, r, n, a);
          break e;
        } else for (Xe = qt(t.stateNode.containerInfo.firstChild), Ze = t, oe = !0, pt = null, n = ju(t, null, r, n), t.child = n; n; ) n.flags = n.flags & -3 | 4096, n = n.sibling;
        else {
          if (Kn(), r === a) {
            t = zt(e, t, n);
            break e;
          }
          Le(e, t, r, n);
        }
        t = t.child;
      }
      return t;
    case 5:
      return wu(t), e === null && Si(t), r = t.type, a = t.pendingProps, i = e !== null ? e.memoizedProps : null, o = a.children, xi(r, a) ? o = null : i !== null && xi(r, i) && (t.flags |= 32), Ku(e, t), Le(e, t, o, n), t.child;
    case 6:
      return e === null && Si(t), null;
    case 13:
      return Yu(e, t, n);
    case 4:
      return vo(t, t.stateNode.containerInfo), r = t.pendingProps, e === null ? t.child = qn(t, null, r, n) : Le(e, t, r, n), t.child;
    case 11:
      return r = t.type, a = t.pendingProps, a = t.elementType === r ? a : dt(r, a), Us(e, t, r, a, n);
    case 7:
      return Le(e, t, t.pendingProps, n), t.child;
    case 8:
      return Le(e, t, t.pendingProps.children, n), t.child;
    case 12:
      return Le(e, t, t.pendingProps.children, n), t.child;
    case 10:
      e: {
        if (r = t.type._context, a = t.pendingProps, i = t.memoizedProps, o = a.value, re(Oa, r._currentValue), r._currentValue = o, i !== null) if (xt(i.value, o)) {
          if (i.children === a.children && !He.current) {
            t = zt(e, t, n);
            break e;
          }
        } else for (i = t.child, i !== null && (i.return = t); i !== null; ) {
          var s = i.dependencies;
          if (s !== null) {
            o = i.child;
            for (var c = s.firstContext; c !== null; ) {
              if (c.context === r) {
                if (i.tag === 1) {
                  c = Ft(-1, n & -n), c.tag = 2;
                  var d = i.updateQueue;
                  if (d !== null) {
                    d = d.shared;
                    var N = d.pending;
                    N === null ? c.next = c : (c.next = N.next, N.next = c), d.pending = c;
                  }
                }
                i.lanes |= n, c = i.alternate, c !== null && (c.lanes |= n), wi(
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
            o.lanes |= n, s = o.alternate, s !== null && (s.lanes |= n), wi(o, n, t), o = i.sibling;
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
        Le(e, t, a.children, n), t = t.child;
      }
      return t;
    case 9:
      return a = t.type, r = t.pendingProps.children, Bn(t, n), a = st(a), r = r(a), t.flags |= 1, Le(e, t, r, n), t.child;
    case 14:
      return r = t.type, a = dt(r, t.pendingProps), a = dt(r.type, a), Vs(e, t, r, a, n);
    case 15:
      return Qu(e, t, t.type, t.pendingProps, n);
    case 17:
      return r = t.type, a = t.pendingProps, a = t.elementType === r ? a : dt(r, a), Sa(e, t), t.tag = 1, We(r) ? (e = !0, Aa(t)) : e = !1, Bn(t, n), Bu(t, r, a), Ci(t, r, a, n), Pi(null, t, r, !0, e, n);
    case 19:
      return Xu(e, t, n);
    case 22:
      return Gu(e, t, n);
  }
  throw Error(I(156, t.tag));
};
function pd(e, t) {
  return bc(e, t);
}
function Xp(e, t, n, r) {
  this.tag = e, this.key = n, this.sibling = this.child = this.return = this.stateNode = this.type = this.elementType = null, this.index = 0, this.ref = null, this.pendingProps = t, this.dependencies = this.memoizedState = this.updateQueue = this.memoizedProps = null, this.mode = r, this.subtreeFlags = this.flags = 0, this.deletions = null, this.childLanes = this.lanes = 0, this.alternate = null;
}
function it(e, t, n, r) {
  return new Xp(e, t, n, r);
}
function _o(e) {
  return e = e.prototype, !(!e || !e.isReactComponent);
}
function Zp(e) {
  if (typeof e == "function") return _o(e) ? 1 : 0;
  if (e != null) {
    if (e = e.$$typeof, e === qi) return 11;
    if (e === Yi) return 14;
  }
  return 2;
}
function Jt(e, t) {
  var n = e.alternate;
  return n === null ? (n = it(e.tag, t, e.key, e.mode), n.elementType = e.elementType, n.type = e.type, n.stateNode = e.stateNode, n.alternate = e, e.alternate = n) : (n.pendingProps = t, n.type = e.type, n.flags = 0, n.subtreeFlags = 0, n.deletions = null), n.flags = e.flags & 14680064, n.childLanes = e.childLanes, n.lanes = e.lanes, n.child = e.child, n.memoizedProps = e.memoizedProps, n.memoizedState = e.memoizedState, n.updateQueue = e.updateQueue, t = e.dependencies, n.dependencies = t === null ? null : { lanes: t.lanes, firstContext: t.firstContext }, n.sibling = e.sibling, n.index = e.index, n.ref = e.ref, n;
}
function Ca(e, t, n, r, a, i) {
  var o = 2;
  if (r = e, typeof e == "function") _o(e) && (o = 1);
  else if (typeof e == "string") o = 5;
  else e: switch (e) {
    case Pn:
      return hn(n.children, a, i, t);
    case Ki:
      o = 8, a |= 8;
      break;
    case ql:
      return e = it(12, n, t, a | 2), e.elementType = ql, e.lanes = i, e;
    case Yl:
      return e = it(13, n, t, a), e.elementType = Yl, e.lanes = i, e;
    case Xl:
      return e = it(19, n, t, a), e.elementType = Xl, e.lanes = i, e;
    case wc:
      return ol(n, a, i, t);
    default:
      if (typeof e == "object" && e !== null) switch (e.$$typeof) {
        case Nc:
          o = 10;
          break e;
        case Sc:
          o = 9;
          break e;
        case qi:
          o = 11;
          break e;
        case Yi:
          o = 14;
          break e;
        case bt:
          o = 16, r = null;
          break e;
      }
      throw Error(I(130, e == null ? e : typeof e, ""));
  }
  return t = it(o, n, t, a), t.elementType = e, t.type = r, t.lanes = i, t;
}
function hn(e, t, n, r) {
  return e = it(7, e, r, t), e.lanes = n, e;
}
function ol(e, t, n, r) {
  return e = it(22, e, r, t), e.elementType = wc, e.lanes = n, e.stateNode = { isHidden: !1 }, e;
}
function Bl(e, t, n) {
  return e = it(6, e, null, t), e.lanes = n, e;
}
function Hl(e, t, n) {
  return t = it(4, e.children !== null ? e.children : [], e.key, t), t.lanes = n, t.stateNode = { containerInfo: e.containerInfo, pendingChildren: null, implementation: e.implementation }, t;
}
function Jp(e, t, n, r, a) {
  this.tag = t, this.containerInfo = e, this.finishedWork = this.pingCache = this.current = this.pendingChildren = null, this.timeoutHandle = -1, this.callbackNode = this.pendingContext = this.context = null, this.callbackPriority = 0, this.eventTimes = kl(0), this.expirationTimes = kl(-1), this.entangledLanes = this.finishedLanes = this.mutableReadLanes = this.expiredLanes = this.pingedLanes = this.suspendedLanes = this.pendingLanes = 0, this.entanglements = kl(0), this.identifierPrefix = r, this.onRecoverableError = a, this.mutableSourceEagerHydrationData = null;
}
function To(e, t, n, r, a, i, o, s, c) {
  return e = new Jp(e, t, n, s, c), t === 1 ? (t = 1, i === !0 && (t |= 8)) : t = 0, i = it(3, null, null, t), e.current = i, i.stateNode = e, i.memoizedState = { element: r, isDehydrated: n, cache: null, transitions: null, pendingSuspenseBoundaries: null }, ho(i), e;
}
function em(e, t, n) {
  var r = 3 < arguments.length && arguments[3] !== void 0 ? arguments[3] : null;
  return { $$typeof: In, key: r == null ? null : "" + r, children: e, containerInfo: t, implementation: n };
}
function md(e) {
  if (!e) return tn;
  e = e._reactInternals;
  e: {
    if (wn(e) !== e || e.tag !== 1) throw Error(I(170));
    var t = e;
    do {
      switch (t.tag) {
        case 3:
          t = t.stateNode.context;
          break e;
        case 1:
          if (We(t.type)) {
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
    if (We(n)) return mu(e, n, t);
  }
  return t;
}
function hd(e, t, n, r, a, i, o, s, c) {
  return e = To(n, r, !0, e, a, i, o, s, c), e.context = md(null), n = e.current, r = $e(), a = Zt(n), i = Ft(r, a), i.callback = t ?? null, Yt(n, i, a), e.current.lanes = a, Hr(e, a, r), Qe(e, r), e;
}
function sl(e, t, n, r) {
  var a = t.current, i = $e(), o = Zt(a);
  return n = md(n), t.context === null ? t.context = n : t.pendingContext = n, t = Ft(i, o), t.payload = { element: e }, r = r === void 0 ? null : r, r !== null && (t.callback = r), e = Yt(a, t, o), e !== null && (ht(e, a, o, i), ya(e, a, o)), o;
}
function qa(e) {
  if (e = e.current, !e.child) return null;
  switch (e.child.tag) {
    case 5:
      return e.child.stateNode;
    default:
      return e.child.stateNode;
  }
}
function ec(e, t) {
  if (e = e.memoizedState, e !== null && e.dehydrated !== null) {
    var n = e.retryLane;
    e.retryLane = n !== 0 && n < t ? n : t;
  }
}
function zo(e, t) {
  ec(e, t), (e = e.alternate) && ec(e, t);
}
function tm() {
  return null;
}
var vd = typeof reportError == "function" ? reportError : function(e) {
  console.error(e);
};
function Do(e) {
  this._internalRoot = e;
}
cl.prototype.render = Do.prototype.render = function(e) {
  var t = this._internalRoot;
  if (t === null) throw Error(I(409));
  sl(e, t, null, null);
};
cl.prototype.unmount = Do.prototype.unmount = function() {
  var e = this._internalRoot;
  if (e !== null) {
    this._internalRoot = null;
    var t = e.containerInfo;
    Nn(function() {
      sl(null, e, null, null);
    }), t[_t] = null;
  }
};
function cl(e) {
  this._internalRoot = e;
}
cl.prototype.unstable_scheduleHydration = function(e) {
  if (e) {
    var t = Gc();
    e = { blockedOn: null, target: e, priority: t };
    for (var n = 0; n < Vt.length && t !== 0 && t < Vt[n].priority; n++) ;
    Vt.splice(n, 0, e), n === 0 && qc(e);
  }
};
function Lo(e) {
  return !(!e || e.nodeType !== 1 && e.nodeType !== 9 && e.nodeType !== 11);
}
function ul(e) {
  return !(!e || e.nodeType !== 1 && e.nodeType !== 9 && e.nodeType !== 11 && (e.nodeType !== 8 || e.nodeValue !== " react-mount-point-unstable "));
}
function tc() {
}
function nm(e, t, n, r, a) {
  if (a) {
    if (typeof r == "function") {
      var i = r;
      r = function() {
        var d = qa(o);
        i.call(d);
      };
    }
    var o = hd(t, r, e, 0, null, !1, !1, "", tc);
    return e._reactRootContainer = o, e[_t] = o.current, zr(e.nodeType === 8 ? e.parentNode : e), Nn(), o;
  }
  for (; a = e.lastChild; ) e.removeChild(a);
  if (typeof r == "function") {
    var s = r;
    r = function() {
      var d = qa(c);
      s.call(d);
    };
  }
  var c = To(e, 0, !1, null, null, !1, !1, "", tc);
  return e._reactRootContainer = c, e[_t] = c.current, zr(e.nodeType === 8 ? e.parentNode : e), Nn(function() {
    sl(t, c, n, r);
  }), c;
}
function dl(e, t, n, r, a) {
  var i = n._reactRootContainer;
  if (i) {
    var o = i;
    if (typeof a == "function") {
      var s = a;
      a = function() {
        var c = qa(o);
        s.call(c);
      };
    }
    sl(t, o, e, a);
  } else o = nm(n, t, e, a, r);
  return qa(o);
}
Wc = function(e) {
  switch (e.tag) {
    case 3:
      var t = e.stateNode;
      if (t.current.memoizedState.isDehydrated) {
        var n = pr(t.pendingLanes);
        n !== 0 && (Ji(t, n | 1), Qe(t, ve()), !(Y & 6) && (Zn = ve() + 500, ln()));
      }
      break;
    case 13:
      Nn(function() {
        var r = Tt(e, 1);
        if (r !== null) {
          var a = $e();
          ht(r, e, 1, a);
        }
      }), zo(e, 1);
  }
};
eo = function(e) {
  if (e.tag === 13) {
    var t = Tt(e, 134217728);
    if (t !== null) {
      var n = $e();
      ht(t, e, 134217728, n);
    }
    zo(e, 134217728);
  }
};
Qc = function(e) {
  if (e.tag === 13) {
    var t = Zt(e), n = Tt(e, t);
    if (n !== null) {
      var r = $e();
      ht(n, e, t, r);
    }
    zo(e, t);
  }
};
Gc = function() {
  return ee;
};
Kc = function(e, t) {
  var n = ee;
  try {
    return ee = e, t();
  } finally {
    ee = n;
  }
};
oi = function(e, t, n) {
  switch (t) {
    case "input":
      if (ei(e, n), t = n.name, n.type === "radio" && t != null) {
        for (n = e; n.parentNode; ) n = n.parentNode;
        for (n = n.querySelectorAll("input[name=" + JSON.stringify("" + t) + '][type="radio"]'), t = 0; t < n.length; t++) {
          var r = n[t];
          if (r !== e && r.form === e.form) {
            var a = tl(r);
            if (!a) throw Error(I(90));
            Cc(r), ei(r, a);
          }
        }
      }
      break;
    case "textarea":
      Ic(e, n);
      break;
    case "select":
      t = n.value, t != null && On(e, !!n.multiple, t, !1);
  }
};
Dc = Po;
Lc = Nn;
var rm = { usingClientEntryPoint: !1, Events: [Qr, Tn, tl, Tc, zc, Po] }, ur = { findFiberByHostInstance: dn, bundleType: 0, version: "18.3.1", rendererPackageName: "react-dom" }, am = { bundleType: ur.bundleType, version: ur.version, rendererPackageName: ur.rendererPackageName, rendererConfig: ur.rendererConfig, overrideHookState: null, overrideHookStateDeletePath: null, overrideHookStateRenamePath: null, overrideProps: null, overridePropsDeletePath: null, overridePropsRenamePath: null, setErrorHandler: null, setSuspenseHandler: null, scheduleUpdate: null, currentDispatcherRef: Dt.ReactCurrentDispatcher, findHostInstanceByFiber: function(e) {
  return e = $c(e), e === null ? null : e.stateNode;
}, findFiberByHostInstance: ur.findFiberByHostInstance || tm, findHostInstancesForRefresh: null, scheduleRefresh: null, scheduleRoot: null, setRefreshHandler: null, getCurrentFiber: null, reconcilerVersion: "18.3.1-next-f1338f8080-20240426" };
if (typeof __REACT_DEVTOOLS_GLOBAL_HOOK__ < "u") {
  var pa = __REACT_DEVTOOLS_GLOBAL_HOOK__;
  if (!pa.isDisabled && pa.supportsFiber) try {
    Xa = pa.inject(am), St = pa;
  } catch {
  }
}
et.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED = rm;
et.createPortal = function(e, t) {
  var n = 2 < arguments.length && arguments[2] !== void 0 ? arguments[2] : null;
  if (!Lo(t)) throw Error(I(200));
  return em(e, t, null, n);
};
et.createRoot = function(e, t) {
  if (!Lo(e)) throw Error(I(299));
  var n = !1, r = "", a = vd;
  return t != null && (t.unstable_strictMode === !0 && (n = !0), t.identifierPrefix !== void 0 && (r = t.identifierPrefix), t.onRecoverableError !== void 0 && (a = t.onRecoverableError)), t = To(e, 1, !1, null, null, n, !1, r, a), e[_t] = t.current, zr(e.nodeType === 8 ? e.parentNode : e), new Do(t);
};
et.findDOMNode = function(e) {
  if (e == null) return null;
  if (e.nodeType === 1) return e;
  var t = e._reactInternals;
  if (t === void 0)
    throw typeof e.render == "function" ? Error(I(188)) : (e = Object.keys(e).join(","), Error(I(268, e)));
  return e = $c(t), e = e === null ? null : e.stateNode, e;
};
et.flushSync = function(e) {
  return Nn(e);
};
et.hydrate = function(e, t, n) {
  if (!ul(t)) throw Error(I(200));
  return dl(null, e, t, !0, n);
};
et.hydrateRoot = function(e, t, n) {
  if (!Lo(e)) throw Error(I(405));
  var r = n != null && n.hydratedSources || null, a = !1, i = "", o = vd;
  if (n != null && (n.unstable_strictMode === !0 && (a = !0), n.identifierPrefix !== void 0 && (i = n.identifierPrefix), n.onRecoverableError !== void 0 && (o = n.onRecoverableError)), t = hd(t, null, e, 1, n ?? null, a, !1, i, o), e[_t] = t.current, zr(e), r) for (e = 0; e < r.length; e++) n = r[e], a = n._getVersion, a = a(n._source), t.mutableSourceEagerHydrationData == null ? t.mutableSourceEagerHydrationData = [n, a] : t.mutableSourceEagerHydrationData.push(
    n,
    a
  );
  return new cl(t);
};
et.render = function(e, t, n) {
  if (!ul(t)) throw Error(I(200));
  return dl(null, e, t, !1, n);
};
et.unmountComponentAtNode = function(e) {
  if (!ul(e)) throw Error(I(40));
  return e._reactRootContainer ? (Nn(function() {
    dl(null, null, e, !1, function() {
      e._reactRootContainer = null, e[_t] = null;
    });
  }), !0) : !1;
};
et.unstable_batchedUpdates = Po;
et.unstable_renderSubtreeIntoContainer = function(e, t, n, r) {
  if (!ul(n)) throw Error(I(200));
  if (e == null || e._reactInternals === void 0) throw Error(I(38));
  return dl(e, t, n, !1, r);
};
et.version = "18.3.1-next-f1338f8080-20240426";
function xd() {
  if (!(typeof __REACT_DEVTOOLS_GLOBAL_HOOK__ > "u" || typeof __REACT_DEVTOOLS_GLOBAL_HOOK__.checkDCE != "function"))
    try {
      __REACT_DEVTOOLS_GLOBAL_HOOK__.checkDCE(xd);
    } catch (e) {
      console.error(e);
    }
}
xd(), xc.exports = et;
var lm = xc.exports, gd, nc = lm;
gd = nc.createRoot, nc.hydrateRoot;
class im extends Error {
  constructor(t, n) {
    super(t), this.status = n, this.name = "ApiError";
  }
}
function om(e, t) {
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
      throw new im(N, c.status);
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
const yd = g.createContext(null);
function kt() {
  const e = g.useContext(yd);
  if (!e) throw new Error("Fuera del módulo de documentos");
  return e;
}
function sm(e) {
  return om((t, n) => fetch(t, n), e.token);
}
async function Vr(e, t) {
  const n = e.token(), r = await fetch(t, { headers: n ? { Authorization: "Bearer " + n } : {} });
  if (!r.ok) throw new Error("No se pudo generar el documento");
  const a = URL.createObjectURL(await r.blob());
  window.open(a, "_blank"), setTimeout(() => URL.revokeObjectURL(a), 6e4);
}
async function cm(e, t) {
  var s;
  const n = e.token(), r = await fetch(t, { headers: n ? { Authorization: "Bearer " + n } : {} });
  if (!r.ok) throw new Error((await r.json().catch(() => ({}))).title || "No se pudo generar el fichero");
  const a = ((s = /filename="?([^";]+)"?/.exec(r.headers.get("Content-Disposition") ?? "")) == null ? void 0 : s[1]) ?? "fichero", i = URL.createObjectURL(await r.blob()), o = document.createElement("a");
  o.href = i, o.download = a, o.click(), setTimeout(() => URL.revokeObjectURL(i), 6e4);
}
function jd(e, t) {
  return `${e.toLowerCase().normalize("NFD").replace(/[̀-ͯ]/g, "").replace(/[^a-z0-9]+/g, "-").replace(/^-|-$/g, "").slice(0, 60) || "listado"}-${(/* @__PURE__ */ new Date()).toISOString().slice(0, 10)}.${t}`;
}
function Nd(e, t) {
  const n = URL.createObjectURL(e), r = document.createElement("a");
  r.href = n, r.download = t, document.body.appendChild(r), r.click(), r.remove(), setTimeout(() => URL.revokeObjectURL(n), 6e4);
}
async function um(e, t) {
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
  Nd(await n.blob(), jd(t.titulo, "xlsx"));
}
function dm(e) {
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
function fm(e) {
  Nd(new Blob(["\uFEFF" + dm(e)], { type: "text/csv;charset=utf-8" }), jd(e.titulo, "csv"));
}
const Sd = new Intl.NumberFormat("es-ES", { minimumFractionDigits: 2, maximumFractionDigits: 2 }), pm = new Intl.NumberFormat("es-ES", { maximumFractionDigits: 3 }), T = (e) => `${Sd.format(Number(e) || 0)} €`, we = (e) => Sd.format(Number(e) || 0), Ae = (e) => pm.format(Number(e) || 0), je = (e) => {
  if (!e) return "";
  const t = String(e).slice(0, 10).split("-");
  return t.length === 3 ? `${t[2]}/${t[1]}/${t[0]}` : String(e);
}, vt = () => (/* @__PURE__ */ new Date()).toISOString().slice(0, 10), Me = (e) => Math.round((e + Number.EPSILON) * 100) / 100;
let mm = 0;
const Kr = () => `l${Date.now().toString(36)}${(++mm).toString(36)}`;
function vn(e, t) {
  const [n, r] = g.useState(e);
  return g.useEffect(() => {
    const a = setTimeout(() => r(e), t);
    return () => clearTimeout(a);
  }, [e, t]), n;
}
function qr() {
  const e = g.useRef(0);
  return () => {
    const t = ++e.current;
    return () => t === e.current;
  };
}
function Ao(e) {
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
const rc = { presupuesto: "Presupuestos", pedido: "Pedidos de venta", factura: "Facturas emitidas", compra: "Pedidos de compra", gasto: "Facturas de proveedor y gastos" }, hm = { presupuesto: "+ Nuevo presupuesto", pedido: "+ Nuevo pedido", factura: "+ Nueva factura", compra: "+ Nuevo pedido", gasto: "+ Registrar factura" }, vm = {
  presupuesto: ["Borrador", "Aceptado", "Rechazado"],
  pedido: ["Borrador", "Confirmado", "ServidoParcial", "Servido", "Facturado", "Cancelado"],
  factura: ["Emitida", "Rectificada", "Anulada"],
  compra: ["Borrador", "Confirmado", "RecibidoParcial", "Recibido", "Facturado", "Cancelado"],
  gasto: ["Registrado", "Anulado"]
}, Wl = { numero: "numero", fecha: "fecha", tercero: "tercero", base: "base", impuestos: "impuestos", total: "total" }, ac = 50, bi = (e) => e === "Anulada" || e === "Anulado" || e === "Cancelado";
function lc(e, t) {
  const n = { documentos: 0, baseImponible: 0, impuestos: 0, retenciones: 0, total: 0, pendiente: 0, vencido: 0, documentosVencidos: 0 };
  for (const r of e) {
    if (bi(r.estado)) continue;
    n.documentos++, n.baseImponible += r.base, n.impuestos += r.impuestos, n.total += r.total;
    const a = r.pendiente ?? 0;
    a > 0 && (n.pendiente += a, r.vencimiento && r.vencimiento < t && (n.vencido += a, n.documentosVencidos++));
  }
  return n.baseImponible = Me(n.baseImponible), n.impuestos = Me(n.impuestos), n.total = Me(n.total), n.pendiente = Me(n.pendiente), n.vencido = Me(n.vencido), n;
}
function Ql(e, t, n) {
  const r = (a) => t === "numero" || t === "tercero" || t === "estado" ? a[t].toLowerCase() : t === "fecha" ? a.fecha : a[t] ?? 0;
  return [...e].sort((a, i) => {
    const o = r(a), s = r(i), c = typeof o == "number" && typeof s == "number" ? o - s : String(o).localeCompare(String(s), "es", { numeric: !0 });
    return n ? -c : c;
  });
}
const xm = (e, t) => ({
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
}), gm = (e, t) => {
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
function ym(e) {
  const { api: t, navegar: n, anfitrion: r } = kt(), a = e.tipo, i = a === "factura" || a === "gasto", o = a === "compra" || a === "gasto", [s, c] = g.useState(""), [d, N] = g.useState(""), [u, h] = g.useState(""), [v, y] = g.useState(""), [S, O] = g.useState(""), [p, f] = g.useState(""), [x, C] = g.useState(""), [_, F] = g.useState(""), [z, k] = g.useState(""), [A, U] = g.useState({ campo: "fecha", desc: !0 }), [P, G] = g.useState(1), [se, Pe] = g.useState(null), [De, fe] = g.useState(0), [me, E] = g.useState(null), [j, D] = g.useState([]), [H, Q] = g.useState([]), [q, he] = g.useState(""), [M, te] = g.useState(!1), V = vn(s, 250), Ge = vn(x, 350), nt = vn(_, 350), Lt = qr(), At = vt();
  g.useEffect(() => {
    t.get(o ? "/proveedores" : "/clientes").then((m) => D([...m].sort((L, R) => L.nombre.localeCompare(R.nombre, "es")))).catch(() => D([])), a === "factura" && t.get("/series").then((m) => Q([...new Set(m.filter((L) => L.tipoDocumento === "Factura" || L.tipoDocumento === 0).map((L) => L.prefijo))].sort())).catch(() => Q([]));
  }, [t, a, o]), g.useEffect(() => G(1), [V, d, u, v, S, p, Ge, nt, z, A, a]);
  const Mt = (m, L) => {
    const R = new URLSearchParams({ pagina: String(m), tamanoPagina: String(L) });
    V.trim() && R.set("texto", V.trim()), d && R.set("estado", d === "Anulada" && a === "gasto" ? "Anulado" : d), u && R.set("desde", u), v && R.set("hasta", v), S && R.set(a === "gasto" ? "proveedorId" : "clienteId", S), p && a === "factura" && R.set("serie", p);
    const $ = parseFloat(Ge.replace(/\./g, "").replace(",", ".")), W = parseFloat(nt.replace(/\./g, "").replace(",", "."));
    isNaN($) || R.set("importeMin", String($)), isNaN(W) || R.set("importeMax", String(W)), z && R.set("cobro", z);
    const Z = Wl[A.campo];
    return Z && (R.set("orden", Z === "tercero" ? a === "gasto" ? "proveedor" : "cliente" : Z), R.set("desc", String(A.desc))), R;
  }, kn = async (m, L) => {
    if (a === "factura") {
      const $ = await t.get(`/facturas/buscar?${Mt(m, L)}`);
      return { r: $, filas: $.elementos.map((W) => xm(W, $.pendientes)) };
    }
    const R = await t.get(`/gastos/buscar?${Mt(m, L)}`);
    return { r: R, filas: R.elementos.map(($) => gm($, R.pendientes)) };
  };
  g.useEffect(() => {
    he("");
    const m = Lt();
    (async () => {
      if (i) {
        const { r: R, filas: $ } = await kn(P, ac);
        return m() && (fe(R.total), E(R.totales ?? null)), $;
      }
      switch (a) {
        case "presupuesto":
          return (await t.get("/presupuestos")).map((R) => ({ id: R.id, numero: R.numeroCompleto, fecha: R.fecha, tercero: R.clienteNombre, terceroId: R.clienteId, base: R.baseImponible ?? R.total, impuestos: R.cuotaIva ?? 0, total: R.total, estado: R.estado }));
        case "pedido":
          return (await t.get("/pedidos-venta")).map((R) => {
            const $ = Me(R.lineas.reduce((W, Z) => W + Z.base, 0));
            return { id: R.id, numero: R.numeroCompleto, fecha: R.fecha, tercero: R.clienteNombre, terceroId: R.clienteId, base: $, impuestos: Me(R.total - $), total: R.total, estado: R.estado };
          });
        default:
          return (await t.get("/compras/pedidos")).map((R) => {
            const $ = Me(R.lineas.reduce((W, Z) => W + Z.importe, 0));
            return { id: R.id, numero: R.numeroCompleto, fecha: R.fecha, tercero: R.proveedorTexto, terceroId: R.proveedorId, base: $, impuestos: Me(R.total - $), total: R.total, estado: R.estado, extra: R.empresaOrigenId ? "Intragrupo" : void 0 };
          });
      }
    })().then((R) => m() && Pe(R)).catch((R) => m() && (he(R.message), Pe([])));
  }, [t, a, P, V, d, u, v, S, p, Ge, nt, z, A.campo, A.desc]);
  const ne = g.useMemo(() => {
    if (!se) return [];
    if (i) return Wl[A.campo] ? se : Ql(se, A.campo, A.desc);
    const m = V.trim().toLowerCase(), L = parseFloat(Ge.replace(/\./g, "").replace(",", ".")), R = parseFloat(nt.replace(/\./g, "").replace(",", ".")), $ = se.filter((W) => (!m || W.numero.toLowerCase().includes(m) || W.tercero.toLowerCase().includes(m)) && (!d || W.estado === d) && (!u || W.fecha >= u) && (!v || W.fecha <= v) && (!S || W.terceroId === S) && (isNaN(L) || W.total >= L) && (isNaN(R) || W.total <= R));
    return Ql($, A.campo, A.desc);
  }, [se, V, d, u, v, S, Ge, nt, A, i]), xe = i ? me : lc(ne, At), Ke = i ? Math.max(1, Math.ceil(De / ac)) : 1, qe = [d, u, v, S, p, x, _, z].filter(Boolean).length, $t = o ? "Proveedor" : "Cliente";
  function gt() {
    c(""), N(""), h(""), y(""), O(""), f(""), C(""), F(""), k("");
  }
  function Ue(m, L, R = !1) {
    const $ = A.campo === m;
    return /* @__PURE__ */ l.jsxs("th", { className: (R ? "num " : "") + "dx-ordenable" + ($ ? " activo" : ""), onClick: () => U({ campo: m, desc: $ ? !A.desc : m === "fecha" || R }), title: `Ordenar por ${L.toLowerCase()}`, children: [
      L,
      /* @__PURE__ */ l.jsx("span", { className: "dx-flecha", children: $ ? A.desc ? "▼" : "▲" : "" })
    ] });
  }
  async function on() {
    if (!i) return ne;
    const m = [];
    for (let L = 1; L <= 500; L++) {
      const { r: R, filas: $ } = await kn(L, 200);
      if (m.push(...$), m.length >= R.total || $.length === 0) break;
    }
    return Wl[A.campo] ? m : Ql(m, A.campo, A.desc);
  }
  async function sn(m) {
    te(!0);
    try {
      const L = await on(), R = i, $ = i ? me : lc(L, At), W = {
        titulo: rc[a],
        columnas: [
          { titulo: "Número", tipo: "texto" },
          { titulo: "Fecha", tipo: "fecha" },
          { titulo: $t, tipo: "texto" },
          { titulo: "Estado", tipo: "texto" },
          { titulo: "Base", tipo: "moneda", total: "no" },
          { titulo: "Impuestos", tipo: "moneda", total: "no" },
          { titulo: "Total", tipo: "moneda", total: "no" },
          ...R ? [{ titulo: "Pendiente", tipo: "moneda", total: "no" }, { titulo: "Vencimiento", tipo: "fecha" }] : []
        ],
        filas: L.map((Z) => [Z.numero + (Z.extra ? ` (${Z.extra})` : ""), je(Z.fecha), Z.tercero, Z.estado, Z.base, Z.impuestos, Z.total, ...R ? [Z.pendiente ?? 0, je(Z.vencimiento)] : []]),
        totales: $ ? [`Total · ${$.documentos} (sin anulados)`, null, null, null, $.baseImponible, $.impuestos, $.total, ...R ? [$.pendiente, null] : []] : void 0
      };
      m === "xlsx" ? await um(r.token(), W) : fm(W), r.aviso(`Exportados ${L.length} documento(s).`, "ok");
    } catch (L) {
      r.aviso("No se pudo exportar: " + L.message, "err");
    } finally {
      te(!1);
    }
  }
  return /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel-head", children: [
      /* @__PURE__ */ l.jsx("h2", { children: rc[a] }),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-acciones", children: [
        /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", disabled: M || !ne.length, onClick: () => sn("xlsx"), title: "Exportar a Excel todo lo filtrado", children: M ? "Exportando…" : "↓ Excel" }),
        /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", disabled: M || !ne.length, onClick: () => sn("csv"), title: "Exportar a CSV todo lo filtrado", children: "↓ CSV" }),
        /* @__PURE__ */ l.jsx("button", { className: "btn small", onClick: () => n({ tipo: a, pantalla: "editor" }), children: hm[a] })
      ] })
    ] }),
    /* @__PURE__ */ l.jsxs("div", { className: "dx-filtros", children: [
      /* @__PURE__ */ l.jsx("input", { placeholder: o ? "Número, proveedor o concepto…" : "Número, cliente o NIF…", value: s, onChange: (m) => c(m.target.value), autoFocus: !0 }),
      /* @__PURE__ */ l.jsxs("select", { value: d, onChange: (m) => N(m.target.value), "aria-label": "Estado", children: [
        /* @__PURE__ */ l.jsx("option", { value: "", children: "Todos los estados" }),
        vm[a].map((m) => /* @__PURE__ */ l.jsx("option", { value: m, children: m }, m))
      ] }),
      /* @__PURE__ */ l.jsx("input", { type: "date", value: u, onChange: (m) => h(m.target.value), title: "Desde", "aria-label": "Desde" }),
      /* @__PURE__ */ l.jsx("input", { type: "date", value: v, onChange: (m) => y(m.target.value), title: "Hasta", "aria-label": "Hasta" })
    ] }),
    /* @__PURE__ */ l.jsxs("div", { className: "dx-filtros dx-filtros2", children: [
      /* @__PURE__ */ l.jsxs("select", { value: S, onChange: (m) => O(m.target.value), "aria-label": $t, children: [
        /* @__PURE__ */ l.jsx("option", { value: "", children: o ? "Todos los proveedores" : "Todos los clientes" }),
        j.map((m) => /* @__PURE__ */ l.jsxs("option", { value: m.id, children: [
          m.nombre,
          m.nifFiscal ? ` · ${m.nifFiscal}` : ""
        ] }, m.id))
      ] }),
      a === "factura" && /* @__PURE__ */ l.jsxs("select", { value: p, onChange: (m) => f(m.target.value), "aria-label": "Serie", children: [
        /* @__PURE__ */ l.jsx("option", { value: "", children: "Todas las series" }),
        H.map((m) => /* @__PURE__ */ l.jsxs("option", { value: m, children: [
          "Serie ",
          m
        ] }, m))
      ] }),
      /* @__PURE__ */ l.jsx("input", { inputMode: "decimal", placeholder: "Importe mín.", value: x, onChange: (m) => C(m.target.value), "aria-label": "Importe mínimo" }),
      /* @__PURE__ */ l.jsx("input", { inputMode: "decimal", placeholder: "Importe máx.", value: _, onChange: (m) => F(m.target.value), "aria-label": "Importe máximo" }),
      i && /* @__PURE__ */ l.jsxs("select", { value: z, onChange: (m) => k(m.target.value), "aria-label": a === "gasto" ? "Estado de pago" : "Estado de cobro", children: [
        /* @__PURE__ */ l.jsx("option", { value: "", children: a === "gasto" ? "Pagadas y pendientes" : "Cobradas y pendientes" }),
        /* @__PURE__ */ l.jsx("option", { value: "pendiente", children: "Pendientes" }),
        /* @__PURE__ */ l.jsx("option", { value: "vencida", children: "Vencidas" }),
        /* @__PURE__ */ l.jsx("option", { value: a === "gasto" ? "pagada" : "cobrada", children: a === "gasto" ? "Pagadas" : "Cobradas" })
      ] }),
      (qe > 0 || s) && /* @__PURE__ */ l.jsxs("button", { className: "btn small secondary", onClick: gt, children: [
        "Limpiar",
        qe ? ` (${qe})` : ""
      ] })
    ] }),
    q && /* @__PURE__ */ l.jsx("p", { className: "dx-rojo", children: q }),
    se === null ? /* @__PURE__ */ l.jsx("p", { className: "muted", children: "Cargando…" }) : ne.length === 0 ? /* @__PURE__ */ l.jsx("p", { className: "muted", children: "No hay documentos con estos filtros." }) : /* @__PURE__ */ l.jsxs("table", { className: "dx-lista-docs", children: [
      /* @__PURE__ */ l.jsx("thead", { children: /* @__PURE__ */ l.jsxs("tr", { children: [
        Ue("numero", "Número"),
        Ue("fecha", "Fecha"),
        Ue("tercero", $t),
        Ue("estado", "Estado"),
        Ue("base", "Base", !0),
        Ue("impuestos", "Impuestos", !0),
        Ue("total", "Total", !0),
        i && Ue("pendiente", "Pendiente", !0)
      ] }) }),
      /* @__PURE__ */ l.jsx("tbody", { children: ne.map((m) => {
        const L = (m.pendiente ?? 0) > 0 && !!m.vencimiento && m.vencimiento < At;
        return /* @__PURE__ */ l.jsxs("tr", { onClick: () => n({ tipo: a, pantalla: "vista", id: m.id }), tabIndex: 0, onKeyDown: (R) => R.key === "Enter" && n({ tipo: a, pantalla: "vista", id: m.id }), className: bi(m.estado) ? "dx-anulado" : void 0, children: [
          /* @__PURE__ */ l.jsxs("td", { className: "mono", children: [
            /* @__PURE__ */ l.jsx("strong", { children: m.numero }),
            m.extra && /* @__PURE__ */ l.jsx("span", { className: "pill part", style: { marginLeft: 6 }, children: m.extra })
          ] }),
          /* @__PURE__ */ l.jsx("td", { children: je(m.fecha) }),
          /* @__PURE__ */ l.jsx("td", { children: m.tercero }),
          /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx("span", { className: Ao(m.estado), children: m.estado }) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: T(m.base) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: T(m.impuestos) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: T(m.total) }) }),
          i && /* @__PURE__ */ l.jsx("td", { className: "num", children: (m.pendiente ?? 0) > 0 ? /* @__PURE__ */ l.jsx("strong", { className: L ? "dx-rojo" : void 0, title: L ? `Vencida el ${je(m.vencimiento)}` : `Vence el ${je(m.vencimiento)}`, children: T(m.pendiente) }) : /* @__PURE__ */ l.jsx("span", { className: "muted", children: bi(m.estado) || m.estado === "Rectificada" ? "—" : a === "gasto" ? "Pagada" : "Cobrada" }) })
        ] }, m.id);
      }) }),
      xe && /* @__PURE__ */ l.jsx("tfoot", { children: /* @__PURE__ */ l.jsxs("tr", { className: "dx-total", children: [
        /* @__PURE__ */ l.jsxs("td", { colSpan: 4, children: [
          /* @__PURE__ */ l.jsxs("strong", { children: [
            "Total · ",
            xe.documentos
          ] }),
          " ",
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: i ? `documento${xe.documentos === 1 ? "" : "s"} de todo el filtro (${Ke} página${Ke === 1 ? "" : "s"}) · sin anulados` : "sin anulados ni cancelados" }),
          i && xe.vencido > 0 && /* @__PURE__ */ l.jsxs("span", { className: "dx-rojo", style: { marginLeft: 8 }, children: [
            "· vencido ",
            T(xe.vencido),
            " (",
            xe.documentosVencidos,
            ")"
          ] })
        ] }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: T(xe.baseImponible) }) }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: T(xe.impuestos) }) }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: T(xe.total) }) }),
        i && /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: T(xe.pendiente) }) })
      ] }) })
    ] }),
    Ke > 1 && /* @__PURE__ */ l.jsxs("div", { className: "dx-paginas", children: [
      /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", disabled: P <= 1, onClick: () => G(P - 1), children: "←" }),
      /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
        "Página ",
        P,
        " de ",
        Ke,
        " · ",
        De,
        " documentos"
      ] }),
      /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", disabled: P >= Ke, onClick: () => G(P + 1), children: "→" })
    ] })
  ] });
}
function nn(e) {
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
function jm(e) {
  const { api: t } = kt(), [n, r] = g.useState(!1), [a, i] = g.useState([]), [o, s] = g.useState(null), [c, d] = g.useState(0), N = vn(e.texto, 180), u = o === e.texto.trim() ? a : [], h = qr();
  g.useEffect(() => {
    if (!n) return;
    const y = h(), S = encodeURIComponent(N.trim());
    t.get(`/productos/buscar?texto=${S}&tamanoPagina=12`).then((O) => y() && (i(O.elementos ?? []), s(N.trim()), d(0))).catch(() => y() && (i([]), s(N.trim())));
  }, [N, n]);
  function v(y) {
    var S;
    if (n && y.key === "Enter" && e.texto.trim() && !u.length) {
      y.preventDefault(), y.stopPropagation();
      return;
    }
    if (n && u.length) {
      if (y.key === "ArrowDown") return y.preventDefault(), d((O) => Math.min(O + 1, u.length - 1));
      if (y.key === "ArrowUp") return y.preventDefault(), d((O) => Math.max(O - 1, 0));
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
        onMouseDown: (O) => (O.preventDefault(), e.alElegir(y), r(!1)),
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
            T(e.precioDe ? e.precioDe(y) : y.precioUnitario),
            "/",
            y.unidad,
            y.controlarStock && /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
              " · stock ",
              Ae(y.stock)
            ] })
          ] })
        ]
      },
      y.id
    )) })
  ] });
}
function Mo(e) {
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
const Nm = { Porcentaje: "%", PorUnidad: "€/ud", PorKilo: "€/kg", Importe: "€" };
function $o(e) {
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
          Nm[s.calculo],
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
function fl(e) {
  var t;
  return (t = e.conceptos) != null && t.length ? /* @__PURE__ */ l.jsx("div", { className: "dx-aplicados", children: e.conceptos.map((n, r) => /* @__PURE__ */ l.jsxs("div", { children: [
    "· ",
    n.nombre,
    n.calculo === "Porcentaje" ? ` (${Ae(n.valor)} %)` : "",
    n.repartido ? " · del documento" : "",
    n.efecto === "Coste" ? " · coste" : "",
    ": ",
    /* @__PURE__ */ l.jsx("strong", { children: T(n.importe) })
  ] }, r)) }) : null;
}
const kr = () => ({ clave: Kr(), productoId: null, descripcion: "", cantidad: 1, precio: null, dto: 0, iva: null });
function wd(e) {
  const t = g.useRef(null), [n, r] = g.useState(/* @__PURE__ */ new Set()), a = e.modo === "venta", i = a ? 7 : 5, o = (u, h) => e.alCambiar(e.lineas.map((v) => v.clave === u ? { ...v, ...h } : v)), s = (u) => {
    const h = e.lineas.filter((v) => v.clave !== u);
    e.alCambiar(h.length ? h : [kr()]);
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
        v === e.lineas.length - 1 ? (e.alCambiar([...e.lineas, kr()]), setTimeout(() => c(v + 1, 0), 30)) : c(v + 1, 0);
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
        const v = e.calculos[h], y = (v == null ? void 0 : v.conceptos) ?? [], S = a && u.controlarStock && u.stock != null && u.cantidad > u.stock, O = v && v.margen != null && v.importe ? v.margen / v.importe * 100 : null;
        return [
          /* @__PURE__ */ l.jsxs("tr", { className: h % 2 ? "dx-par" : "", children: [
            /* @__PURE__ */ l.jsxs("td", { children: [
              e.soloLectura ? /* @__PURE__ */ l.jsx("span", { className: "mono", children: u.referencia ?? "" }) : /* @__PURE__ */ l.jsx(
                jm,
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
                  Ae(u.stock)
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
              y.length > 0 && !n.has(u.clave) && /* @__PURE__ */ l.jsx("button", { type: "button", className: "dx-resumen-conc", onClick: () => N(u.clave), title: "Ver y cambiar los conceptos", children: y.map((p) => `${p.importe < 0 ? "−" : "+"} ${p.codigo.toLowerCase()} ${we(Math.abs(p.importe))}${p.efecto === "Coste" ? " (coste)" : ""}`).join(" · ") })
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
                placeholder: v ? we(v.precio) : "",
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
            /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: v ? T(v.importe) : "—" }) }),
            /* @__PURE__ */ l.jsx("td", { className: "num", children: a ? (v == null ? void 0 : v.margen) != null && /* @__PURE__ */ l.jsxs("span", { className: v.margen < 0 ? "dx-rojo" : "muted", children: [
              T(v.margen),
              O != null && /* @__PURE__ */ l.jsxs("div", { className: "dx-sub", children: [
                we(O),
                " %"
              ] })
            ] }) : (v == null ? void 0 : v.costeUnitarioEntrada) != null && /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
              T(v.costeUnitarioEntrada),
              "/ud"
            ] }) }),
            /* @__PURE__ */ l.jsxs("td", { className: "right", style: { whiteSpace: "nowrap" }, children: [
              !e.soloLectura && e.catalogo.length > 0 && /* @__PURE__ */ l.jsx("button", { type: "button", className: "dx-icono" + (n.has(u.clave) ? " activo" : ""), title: "Conceptos de la línea", onClick: () => N(u.clave), "data-f": h, "data-c": a ? 6 : 4, children: "±" }),
              !e.soloLectura && /* @__PURE__ */ l.jsx("button", { type: "button", className: "dx-icono", title: "Quitar la línea", onClick: () => s(u.clave), children: "✕" })
            ] })
          ] }, u.clave),
          n.has(u.clave) && /* @__PURE__ */ l.jsx("tr", { className: "dx-fila-conc", children: /* @__PURE__ */ l.jsx("td", { colSpan: a ? 9 : 7, children: /* @__PURE__ */ l.jsx($o, { catalogo: e.catalogo, lista: u.conceptos, sugeridos: e.sugeridos[u.clave], alCambiar: (p) => o(u.clave, { conceptos: p }) }) }) }, u.clave + "c")
        ];
      }) })
    ] }),
    !e.soloLectura && /* @__PURE__ */ l.jsx("button", { type: "button", className: "btn small ghost", style: { marginTop: 8 }, onClick: () => (e.alCambiar([...e.lineas, kr()]), setTimeout(() => c(e.lineas.length, 0), 30)), children: "+ Añadir línea" }),
    !e.soloLectura && /* @__PURE__ */ l.jsx("span", { className: "muted dx-ayuda", children: "Intro: siguiente casilla · ↑↓: cambiar de línea · F2: buscar artículo · el precio en gris es el de tarifa" })
  ] });
}
function Sm(e, t) {
  if (!e) return e;
  const n = e.toUpperCase();
  return (t === "Igic" ? { IVA21: "IGIC7", IVA10: "IGIC3", IVA4: "IGIC0", IVA0: "IGIC0", REAGP12: "REAGPIGIC", REAGP105: "REAGPIGIC" }[n] : { IGIC7: "IVA21", IGIC3: "IVA10", IGIC0: "IVA0", REAGPIGIC: "REAGP12" }[n]) ?? e;
}
const Oo = (e) => e.filter((t) => t.disponible > 0 && t.estado !== "Anulado" && !t.facturaId).sort((t, n) => t.fecha.localeCompare(n.fecha)), kd = (e) => e.filter((t) => !!t.facturaId && (t.disponibleBase ?? 0) > 0 && t.estado !== "Anulado").sort((t, n) => t.fecha.localeCompare(n.fecha));
function Cd(e, t) {
  let n = Math.round(t * 100);
  const r = [];
  for (const a of Oo(e)) {
    if (n <= 0) break;
    const i = Math.min(n, Math.round(a.disponible * 100));
    i > 0 && r.push({ id: a.id, importe: i / 100 }), n -= i;
  }
  return r;
}
const wm = { presupuesto: "presupuesto", pedido: "pedido de venta", factura: "factura" };
function Ed(e) {
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
function km(e) {
  var Uo, Vo, Bo, Ho, Wo, Qo;
  const { api: t, anfitrion: n } = kt(), r = !!((Uo = e.semilla) != null && Uo.rectificaId), [a, i] = g.useState([]), [o, s] = g.useState([]), [c, d] = g.useState([]), [N, u] = g.useState([]), [h, v] = g.useState([]), [y, S] = g.useState(((Vo = e.semilla) == null ? void 0 : Vo.clienteId) ?? ""), [O, p] = g.useState(e.tipo === "pedido" && ((Bo = e.semilla) != null && Bo.fecha) ? e.semilla.fecha : vt()), [f, x] = g.useState(""), [C, _] = g.useState(""), [F, z] = g.useState(0), [k, A] = g.useState(!1), [U, P] = g.useState(null), [G, se] = g.useState(30), [Pe, De] = g.useState(""), [fe, me] = g.useState([kr()]), [E, j] = g.useState([]), [D, H] = g.useState(!1), Q = (Ho = e.semilla) != null && Ho.lineas.some((w) => /^(IGIC|REAGPIGIC)/i.test(w.codigoIva ?? "")) ? "Igic" : (Wo = e.semilla) != null && Wo.lineas.length ? "Iva" : null, [q, he] = g.useState(Q ?? "Iva"), [M, te] = g.useState(null), [V, Ge] = g.useState(""), [nt, Lt] = g.useState(!1), [At, Mt] = g.useState(!1), [kn, ne] = g.useState(!1), [xe, Ke] = g.useState([]), [qe, $t] = g.useState(!0), [gt, Ue] = g.useState([]), [on, sn] = g.useState(!0), m = qr();
  g.useEffect(() => {
    t.get("/clientes").then(i).catch(() => i([])), t.get("/tipos-iva").then((w) => s(w.filter((B) => B.activo))).catch(() => s([])), t.get("/formas-pago").then((w) => d(w.filter((B) => B.activo))).catch(() => d([])), t.get("/series").then((w) => u([...new Set(w.filter((B) => B.tipoDocumento === "Factura").map((B) => B.prefijo))])).catch(() => u([])), t.get("/conceptos-linea?ambito=Ventas&activos=true").then(v).catch(() => v([])), t.get("/empresas/actual").then((w) => {
      H(!!w.operaEnAmbosTerritorios), he(Q ?? (w.territorioFiscal === "Canarias" ? "Igic" : "Iva"));
    }).catch(() => H(!1));
  }, [t]);
  function L(w) {
    he(w), me((B) => B.map((J) => ({ ...J, iva: J.productoId ? null : Sm(J.iva, w) })));
  }
  const R = g.useMemo(() => D ? o.filter((w) => (w.impuesto ?? "Iva") === q) : o, [D, o, q]);
  g.useEffect(() => {
    const w = e.semilla;
    if (!w || !w.lineas.length) return;
    const { porLinea: B, documento: J } = Ed(w.lineas), ae = w.lineas.map((X, gl) => ({
      clave: Kr(),
      productoId: X.productoId ?? null,
      descripcion: X.descripcion,
      cantidad: X.cantidad,
      precio: X.precioUnitario,
      dto: X.porcentajeDescuento,
      iva: X.codigoIva,
      conceptos: r ? [] : B[gl]
    }));
    me(ae), j(r ? [] : J), Promise.all(ae.map((X) => X.productoId ? t.get(`/productos/${X.productoId}`).catch(() => null) : Promise.resolve(null))).then(
      (X) => me((gl) => gl.map((Go, Cn) => X[Cn] ? { ...Go, referencia: X[Cn].referencia ?? X[Cn].nombre, unidad: X[Cn].unidad, stock: X[Cn].stock, controlarStock: X[Cn].controlarStock } : Go))
    );
  }, [e.semilla, t, r]);
  const $ = a.find((w) => w.id === y);
  g.useEffect(() => {
    if (e.tipo !== "factura" || r || !y) {
      Ke([]), Ue([]);
      return;
    }
    t.get(`/anticipos?clienteId=${y}`).then((w) => {
      Ke(Oo(w)), Ue(kd(w));
    }).catch(() => {
      Ke([]), Ue([]);
    });
  }, [t, y, e.tipo, r]);
  const W = Me(xe.reduce((w, B) => w + B.disponible, 0));
  g.useEffect(() => {
    $ && (A(!!$.recargoEquivalencia), $.formaPagoDefectoId && _($.formaPagoDefectoId));
  }, [$]);
  const Z = g.useMemo(() => fe.map((w, B) => ({ l: w, i: B })).filter(({ l: w }) => (w.productoId || w.descripcion.trim()) && w.cantidad > 0), [fe]), Yr = g.useMemo(
    () => ({
      clienteId: y,
      fechaEmision: e.tipo === "factura" ? O : null,
      serie: f || null,
      diasVencimiento: F,
      formaPagoId: C || null,
      recargoEquivalencia: k,
      porcentajeIrpf: U,
      conceptosDocumento: E,
      impuesto: D && !r ? q : null,
      descontarAnticipos: on && gt.length ? gt.map((w) => ({ anticipoId: w.id })) : null,
      lineas: Z.map(({ l: w }) => ({
        cantidad: w.cantidad,
        descripcion: w.descripcion.trim() || null,
        precioUnitario: w.precio,
        codigoIva: w.iva,
        porcentajeDescuento: w.dto,
        productoId: w.productoId,
        ...r ? { conceptos: [] } : w.conceptos === void 0 ? {} : { conceptos: w.conceptos }
      }))
    }),
    [y, O, f, F, C, k, U, E, Z, e.tipo, r, on, gt, D, q]
  ), nr = vn(Yr, 350);
  g.useEffect(() => {
    if (!nr.clienteId || nr.lineas.length === 0) {
      te(null), Ge(nr.clienteId ? "" : "Elige el cliente para calcular precios e importes.");
      return;
    }
    const w = m();
    Lt(!0), t.post("/facturas/simular", nr).then((B) => w() && (te(B), Ge(""))).catch((B) => w() && (te(null), Ge(B.message))).finally(() => w() && Lt(!1));
  }, [nr, t]);
  const hl = g.useMemo(() => {
    const w = fe.map(() => {
    });
    return M && Z.forEach(({ i: B }, J) => {
      const ae = M.lineas[J];
      ae && (w[B] = { precio: ae.precioUnitario, dto: ae.porcentajeDescuento, iva: ae.codigoIva, importe: ae.base, margen: ae.productoId || ae.costeUnitario || ae.costeConceptos ? ae.margen : void 0, conceptos: ae.conceptos });
    }), w;
  }, [M, fe, Z]), Id = g.useMemo(() => {
    const w = {};
    return fe.forEach((B, J) => {
      var ae;
      return w[B.clave] = (((ae = hl[J]) == null ? void 0 : ae.conceptos) ?? []).filter((X) => !X.repartido).map((X) => ({ conceptoId: X.conceptoId, valor: X.valor }));
    }), w;
  }, [fe, hl]);
  function Pd(w, B) {
    me(
      (J) => J.map(
        (ae) => ae.clave === w ? { ...ae, productoId: B.id, referencia: B.referencia ?? B.nombre, descripcion: B.nombre, precio: null, dto: 0, iva: null, conceptos: void 0, unidad: B.unidad, stock: B.stock, controlarStock: B.controlarStock } : ae
      )
    );
  }
  const Fd = g.useMemo(() => {
    const w = /* @__PURE__ */ new Map();
    for (const B of (M == null ? void 0 : M.lineas) ?? []) {
      const J = w.get(B.codigoIva) ?? { base: 0, cuota: 0, pct: B.porcentajeIva };
      J.base += B.base, J.cuota += B.cuotaIva, w.set(B.codigoIva, J);
    }
    return [...w.entries()];
  }, [M]), vl = ((M == null ? void 0 : M.lineas) ?? []).reduce((w, B) => w + (B.base - B.margen), 0), xl = M ? M.baseImponible - vl : 0, Rd = (w) => {
    var B;
    return ((B = o.find((J) => J.codigo === w)) == null ? void 0 : B.nombre) ?? w;
  };
  async function bo() {
    if (M) {
      Mt(!0);
      try {
        const w = Z.map(({ l: J }, ae) => {
          const X = M.lineas[ae];
          return {
            cantidad: J.cantidad,
            descripcion: X.descripcion,
            precioUnitario: X.precioUnitario,
            codigoIva: X.codigoIva,
            porcentajeDescuento: X.porcentajeDescuento,
            productoId: J.productoId,
            ...r ? {} : J.conceptos === void 0 ? {} : { conceptos: J.conceptos }
          };
        });
        let B;
        if (r)
          B = (await t.post(`/facturas/${e.semilla.rectificaId}/rectificar`, { motivo: Pe, lineas: w, fechaEmision: O, porcentajeIrpf: U, serie: f || null })).id;
        else if (e.tipo === "factura") {
          const J = await t.post("/facturas", { ...Yr, lineas: w });
          if (B = J.id, qe && xe.length) {
            let ae = 0;
            try {
              for (const X of Cd(xe, J.total))
                await t.post(`/anticipos/${X.id}/aplicar`, { facturaId: B, importe: X.importe }), ae += X.importe;
              n.aviso(`Factura emitida. Aplicados ${T(ae)} de anticipos (asiento 438 a 430).`, "ok");
            } catch (X) {
              n.aviso(`Factura emitida, pero no se pudo aplicar el anticipo: ${X.message}`, "err");
            }
            e.alGuardar(B);
            return;
          }
        } else if (e.tipo === "presupuesto") {
          const J = { clienteId: y, diasValidez: G, lineas: w, conceptosDocumento: E, impuesto: Yr.impuesto };
          B = e.id ? (await t.put(`/presupuestos/${e.id}`, J)).id : (await t.post("/presupuestos", J)).id;
        } else {
          const J = { clienteId: y, fecha: O, lineas: w, conceptosDocumento: E, impuesto: Yr.impuesto };
          B = e.id ? (await t.put(`/pedidos-venta/${e.id}`, J)).id : (await t.post("/pedidos-venta", J)).id;
        }
        n.aviso(e.tipo === "factura" ? "Factura emitida." : "Documento guardado.", "ok"), e.alGuardar(B);
      } catch (w) {
        n.aviso(w.message, "err");
      } finally {
        Mt(!1), ne(!1);
      }
    }
  }
  const _d = r ? `Rectificativa de la factura ${((Qo = e.semilla) == null ? void 0 : Qo.rectificaNumero) ?? ""}` : `${e.id ? "Editar" : "Nuevo"} ${wm[e.tipo]}`.replace("Nuevo factura", "Nueva factura"), Td = !!M && !nt && (!r || Pe.trim().length > 0);
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsxs("div", { className: "panel-head", children: [
        /* @__PURE__ */ l.jsx("h2", { children: _d }),
        /* @__PURE__ */ l.jsxs("div", { style: { display: "flex", gap: 8 }, children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: e.alCancelar, children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", disabled: !Td || At, onClick: () => e.tipo === "factura" ? ne(!0) : bo(), children: e.tipo === "factura" ? "Emitir factura" : "Guardar" })
        ] })
      ] }),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-cabecera", children: [
        /* @__PURE__ */ l.jsxs("div", { className: "dx-cab-campos", children: [
          /* @__PURE__ */ l.jsx(Mo, { terceros: a, valor: y, alCambiar: S, etiqueta: "Cliente", deshabilitado: r || e.tipo === "pedido" && !!e.id && !1 }),
          /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", children: [
            e.tipo !== "presupuesto" && /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: e.tipo === "factura" ? "Fecha de emisión" : "Fecha del pedido" }),
              /* @__PURE__ */ l.jsx("input", { type: "date", value: O, onChange: (w) => p(w.target.value) })
            ] }),
            e.tipo === "presupuesto" && /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Validez" }),
              /* @__PURE__ */ l.jsx("select", { value: G, onChange: (w) => se(Number(w.target.value)), children: [15, 30, 60, 90].map((w) => /* @__PURE__ */ l.jsxs("option", { value: w, children: [
                w,
                " días"
              ] }, w)) })
            ] }),
            D && !r && /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Territorio de la operación" }),
              /* @__PURE__ */ l.jsxs("select", { value: q, onChange: (w) => L(w.target.value), children: [
                /* @__PURE__ */ l.jsx("option", { value: "Iva", children: "Península y Baleares · IVA" }),
                /* @__PURE__ */ l.jsx("option", { value: "Igic", children: "Canarias · IGIC" })
              ] })
            ] }),
            e.tipo === "factura" && N.length > 0 && /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Serie" }),
              /* @__PURE__ */ l.jsxs("select", { value: f, onChange: (w) => x(w.target.value), children: [
                /* @__PURE__ */ l.jsx("option", { value: "", children: "La del cliente" }),
                N.map((w) => /* @__PURE__ */ l.jsx("option", { value: w, children: w }, w))
              ] })
            ] }),
            e.tipo === "factura" && !r && /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
              /* @__PURE__ */ l.jsxs("div", { children: [
                /* @__PURE__ */ l.jsx("label", { children: "Forma de pago" }),
                /* @__PURE__ */ l.jsxs("select", { value: C, onChange: (w) => _(w.target.value), children: [
                  /* @__PURE__ */ l.jsx("option", { value: "", children: "— Vencimiento a mano —" }),
                  c.map((w) => /* @__PURE__ */ l.jsx("option", { value: w.id, children: w.nombre }, w.id))
                ] })
              ] }),
              !C && /* @__PURE__ */ l.jsxs("div", { children: [
                /* @__PURE__ */ l.jsx("label", { children: "Vencimiento" }),
                /* @__PURE__ */ l.jsx("select", { value: F, onChange: (w) => z(Number(w.target.value)), children: [0, 15, 30, 45, 60, 90].map((w) => /* @__PURE__ */ l.jsx("option", { value: w, children: w ? `${w} días` : "Contado" }, w)) })
              ] })
            ] }),
            e.tipo === "factura" && /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Retención IRPF %" }),
              /* @__PURE__ */ l.jsx("input", { type: "number", step: "0.01", value: U ?? "", placeholder: String(($ == null ? void 0 : $.porcentajeIrpfDefecto) ?? 0), onChange: (w) => P(w.target.value === "" ? null : Number(w.target.value)) })
            ] })
          ] }),
          e.tipo === "factura" && !r && /* @__PURE__ */ l.jsxs("label", { className: "dx-check", children: [
            /* @__PURE__ */ l.jsx("input", { type: "checkbox", checked: k, onChange: (w) => A(w.target.checked) }),
            " Recargo de equivalencia"
          ] }),
          r && /* @__PURE__ */ l.jsxs("div", { children: [
            /* @__PURE__ */ l.jsx("label", { children: "Motivo de la rectificación (obligatorio)" }),
            /* @__PURE__ */ l.jsx("input", { value: Pe, onChange: (w) => De(w.target.value), placeholder: "Devolución, error en precio…" })
          ] })
        ] }),
        /* @__PURE__ */ l.jsx("div", { className: "dx-ficha", children: $ ? /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("strong", { children: $.nombre }),
          /* @__PURE__ */ l.jsx("div", { className: "muted", children: [$.nifFiscal, $.poblacion, $.provincia].filter(Boolean).join(" · ") }),
          $.limiteRiesgo != null && /* @__PURE__ */ l.jsxs("div", { className: "muted", children: [
            "Límite de riesgo ",
            T($.limiteRiesgo)
          ] }),
          $.tarifaId && /* @__PURE__ */ l.jsx("div", { className: "muted", children: "Con tarifa de precios propia" }),
          $.recargoEquivalencia && /* @__PURE__ */ l.jsx("div", { className: "muted", children: "En recargo de equivalencia" }),
          (M == null ? void 0 : M.avisoRiesgo) && /* @__PURE__ */ l.jsxs("div", { className: "dx-aviso", children: [
            "⚠ ",
            M.avisoRiesgo
          ] }),
          gt.length > 0 && /* @__PURE__ */ l.jsxs("div", { className: "dx-anticipo", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              "🧾 Anticipos facturados pendientes de descontar: ",
              /* @__PURE__ */ l.jsx("strong", { children: T(gt.reduce((w, B) => w + (B.disponibleBase ?? 0), 0)) }),
              " de base (",
              gt.map((w) => w.facturaNumero).join(", "),
              ")."
            ] }),
            /* @__PURE__ */ l.jsxs("label", { className: "dx-check", children: [
              /* @__PURE__ */ l.jsx("input", { type: "checkbox", checked: on, onChange: (w) => sn(w.target.checked) }),
              "Descontar en esta factura (línea negativa con su base e IVA; hasta la base de la factura)"
            ] })
          ] }),
          W > 0 && /* @__PURE__ */ l.jsxs("div", { className: "dx-anticipo", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              "💶 Tiene ",
              /* @__PURE__ */ l.jsx("strong", { children: T(W) }),
              " en ",
              xe.length === 1 ? "un anticipo pendiente" : `${xe.length} anticipos pendientes`,
              " de aplicar."
            ] }),
            /* @__PURE__ */ l.jsxs("label", { className: "dx-check", children: [
              /* @__PURE__ */ l.jsx("input", { type: "checkbox", checked: qe, onChange: (w) => $t(w.target.checked) }),
              "Aplicarlo al emitir",
              M ? ` (${T(Math.min(W, M.total))})` : ""
            ] })
          ] })
        ] }) : /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Elige un cliente: se aplican su tarifa, su forma de pago, su recargo y sus conceptos." }) })
      ] })
    ] }),
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsx(wd, { modo: "venta", lineas: fe, alCambiar: me, calculos: hl, ivas: R, catalogo: r ? [] : h, sugeridos: Id, alElegirArticulo: Pd }),
      !r && h.length > 0 && /* @__PURE__ */ l.jsx("div", { style: { marginTop: 10 }, children: /* @__PURE__ */ l.jsx($o, { catalogo: h, lista: E, alCambiar: (w) => j(w ?? []), documento: !0 }) })
    ] }),
    /* @__PURE__ */ l.jsxs("div", { className: "dx-pie", children: [
      /* @__PURE__ */ l.jsxs("div", { className: "dx-estado", children: [
        nt && /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Calculando…" }),
        !nt && V && /* @__PURE__ */ l.jsx("span", { className: "dx-rojo", children: V }),
        (M == null ? void 0 : M.mencionFiscal) && /* @__PURE__ */ l.jsx("div", { className: "muted", style: { fontSize: 12 }, children: M.mencionFiscal })
      ] }),
      /* @__PURE__ */ l.jsxs("div", { className: "panel dx-totales", children: [
        Fd.map(([w, B]) => /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
            Rd(w),
            " · base ",
            we(B.base)
          ] }),
          /* @__PURE__ */ l.jsx("span", { children: T(B.cuota) })
        ] }, w)),
        M == null ? void 0 : M.lineas.filter((w) => w.anticipoId).map((w) => /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-tot-anticipo", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: w.descripcion }),
          /* @__PURE__ */ l.jsx("span", { children: T(w.base + w.cuotaIva + w.cuotaRecargo) })
        ] }, w.anticipoId)),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Base imponible" }),
          /* @__PURE__ */ l.jsx("span", { children: T(M == null ? void 0 : M.baseImponible) })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Impuestos" }),
          /* @__PURE__ */ l.jsx("span", { children: T(M == null ? void 0 : M.cuotaIva) })
        ] }),
        !!(M != null && M.recargoTotal) && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Recargo de equivalencia" }),
          /* @__PURE__ */ l.jsx("span", { children: T(M.recargoTotal) })
        ] }),
        !!(M != null && M.retencionIrpf) && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
            "Retención IRPF (",
            we(M.porcentajeIrpf),
            " %)"
          ] }),
          /* @__PURE__ */ l.jsxs("span", { children: [
            "−",
            T(M.retencionIrpf)
          ] })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ l.jsx("span", { children: "Total" }),
          /* @__PURE__ */ l.jsx("span", { children: T(M == null ? void 0 : M.total) })
        ] }),
        M && vl > 0 && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Coste · margen" }),
          /* @__PURE__ */ l.jsxs("span", { className: xl < 0 ? "dx-rojo" : "muted", children: [
            T(vl),
            " · ",
            T(xl),
            " (",
            we(M.baseImponible ? xl / M.baseImponible * 100 : 0),
            " %)"
          ] })
        ] })
      ] })
    ] }),
    kn && M && /* @__PURE__ */ l.jsx(
      nn,
      {
        titulo: r ? "Emitir la rectificativa" : "Emitir la factura",
        alCerrar: () => ne(!1),
        acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => ne(!1), children: "Revisar" }),
          /* @__PURE__ */ l.jsxs("button", { className: "btn small", disabled: At, onClick: bo, children: [
            "Emitir ",
            T(M.total)
          ] })
        ] }),
        children: /* @__PURE__ */ l.jsxs("p", { style: { margin: 0 }, children: [
          "Se emitirá una factura ",
          r ? "rectificativa " : "",
          "de ",
          /* @__PURE__ */ l.jsx("strong", { children: T(M.total) }),
          " a ",
          /* @__PURE__ */ l.jsx("strong", { children: $ == null ? void 0 : $.nombre }),
          " con fecha ",
          O.split("-").reverse().join("/"),
          ". Una factura emitida no se modifica: queda numerada y encadenada en VeriFactu; para corregirla hay que rectificarla o anularla."
        ] })
      }
    )
  ] });
}
function Cm(e) {
  var De, fe, me;
  const { api: t, anfitrion: n } = kt(), [r, a] = g.useState([]), [i, o] = g.useState([]), [s, c] = g.useState(((De = e.semilla) == null ? void 0 : De.proveedorId) ?? ""), [d, N] = g.useState(((fe = e.semilla) == null ? void 0 : fe.fecha) ?? vt()), [u, h] = g.useState([kr()]), [v, y] = g.useState([]), [S, O] = g.useState(null), [p, f] = g.useState(""), [x, C] = g.useState(!1), _ = qr();
  g.useEffect(() => {
    t.get("/proveedores").then(a).catch(() => a([])), t.get("/conceptos-linea?ambito=Compras&activos=true").then(o).catch(() => o([]));
  }, [t]), g.useEffect(() => {
    const E = e.semilla;
    if (!E) return;
    const j = E.lineas.map((q) => ({ ...q, porcentajeDescuento: 0, codigoIva: "" })), { porLinea: D, documento: H } = Ed(j), Q = E.lineas.map((q, he) => ({ clave: Kr(), productoId: q.productoId ?? null, descripcion: q.descripcion, cantidad: q.cantidad, precio: q.precioUnitario, dto: 0, iva: null, conceptos: D[he] }));
    h(Q), y(H), Promise.all(Q.map((q) => q.productoId ? t.get(`/productos/${q.productoId}`).catch(() => null) : Promise.resolve(null))).then(
      (q) => h((he) => he.map((M, te) => q[te] ? { ...M, referencia: q[te].referencia ?? q[te].nombre, unidad: q[te].unidadCompra || q[te].unidad, stock: q[te].stock, controlarStock: q[te].controlarStock } : M))
    );
  }, [e.semilla, t]);
  const F = r.find((E) => E.id === s), z = g.useMemo(() => u.map((E, j) => ({ l: E, i: j })).filter(({ l: E }) => E.descripcion.trim() && E.cantidad > 0), [u]), k = g.useMemo(
    () => {
      var E, j;
      return {
        proveedorId: s || null,
        proveedorTexto: (F == null ? void 0 : F.nombre) ?? (((E = e.semilla) == null ? void 0 : E.proveedorTexto) || null),
        solicitudOrigenId: e.id ? null : ((j = e.semilla) == null ? void 0 : j.solicitudOrigenId) ?? null,
        fecha: d,
        conceptosDocumento: v,
        lineas: z.map(({ l: D }) => ({ descripcion: D.descripcion.trim(), cantidad: D.cantidad, precioUnitario: D.precio ?? 0, productoId: D.productoId, ...D.conceptos === void 0 ? {} : { conceptos: D.conceptos } }))
      };
    },
    [s, F, d, v, z, e.id, e.semilla]
  ), A = vn(k, 350);
  g.useEffect(() => {
    if (!A.proveedorId || A.lineas.length === 0) {
      O(null), f(A.proveedorId ? "" : "Elige el proveedor.");
      return;
    }
    const E = _();
    t.post("/compras/pedidos/simular", A).then((j) => E() && (O(j), f(""))).catch((j) => E() && (O(null), f(j.message)));
  }, [A, t]);
  const U = g.useMemo(() => {
    const E = u.map(() => {
    });
    return z.forEach(({ i: j }, D) => {
      const H = S == null ? void 0 : S.lineas[D];
      H && (E[j] = { precio: H.precioUnitario, importe: H.importe, costeUnitarioEntrada: H.costeUnitarioEntrada, conceptos: H.conceptos });
    }), E;
  }, [S, u, z]), P = g.useMemo(() => {
    const E = {};
    return u.forEach((j, D) => {
      var H;
      return E[j.clave] = (((H = U[D]) == null ? void 0 : H.conceptos) ?? []).filter((Q) => !Q.repartido).map((Q) => ({ conceptoId: Q.conceptoId, valor: Q.valor }));
    }), E;
  }, [u, U]);
  function G(E, j) {
    const D = j.precioCompraPorUnidadCompra ?? j.precioCompra;
    h((H) => H.map((Q) => Q.clave === E ? { ...Q, productoId: j.id, referencia: j.referencia ?? j.nombre, descripcion: j.nombre, precio: D, conceptos: void 0, unidad: j.unidadCompra || j.unidad, stock: j.stock, controlarStock: j.controlarStock } : Q));
  }
  async function se() {
    C(!0);
    try {
      const E = e.id ? await t.put(`/compras/pedidos/${e.id}`, k) : await t.post("/compras/pedidos", k);
      n.aviso("Pedido de compra guardado.", "ok"), e.alGuardar(E.id);
    } catch (E) {
      n.aviso(E.message, "err");
    } finally {
      C(!1);
    }
  }
  const Pe = ((S == null ? void 0 : S.lineas) ?? []).reduce((E, j) => E + j.costeConceptos, 0);
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsxs("div", { className: "panel-head", children: [
        /* @__PURE__ */ l.jsx("h2", { children: e.id ? `Editar pedido ${((me = e.semilla) == null ? void 0 : me.numeroCompleto) ?? ""}` : "Nuevo pedido de compra" }),
        /* @__PURE__ */ l.jsxs("div", { style: { display: "flex", gap: 8 }, children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: e.alCancelar, children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", disabled: !S || x, onClick: se, children: "Guardar" })
        ] })
      ] }),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-cabecera", children: [
        /* @__PURE__ */ l.jsxs("div", { className: "dx-cab-campos", children: [
          /* @__PURE__ */ l.jsx(Mo, { terceros: r, valor: s, alCambiar: c, etiqueta: "Proveedor", deshabilitado: !!e.id }),
          /* @__PURE__ */ l.jsx("div", { className: "dx-fila", children: /* @__PURE__ */ l.jsxs("div", { children: [
            /* @__PURE__ */ l.jsx("label", { children: "Fecha del pedido" }),
            /* @__PURE__ */ l.jsx("input", { type: "date", value: d, onChange: (E) => N(E.target.value) })
          ] }) })
        ] }),
        /* @__PURE__ */ l.jsx("div", { className: "dx-ficha", children: F ? /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("strong", { children: F.nombre }),
          /* @__PURE__ */ l.jsx("div", { className: "muted", children: [F.nifFiscal, F.poblacion, F.pais].filter(Boolean).join(" · ") })
        ] }) : /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Elige un proveedor: se aplican sus conceptos (pronto pago, portes…)." }) })
      ] })
    ] }),
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsx(wd, { modo: "compra", lineas: u, alCambiar: h, calculos: U, ivas: [], catalogo: i, sugeridos: P, alElegirArticulo: G }),
      i.length > 0 && /* @__PURE__ */ l.jsx("div", { style: { marginTop: 10 }, children: /* @__PURE__ */ l.jsx($o, { catalogo: i, lista: v, alCambiar: (E) => y(E ?? []), documento: !0 }) })
    ] }),
    /* @__PURE__ */ l.jsxs("div", { className: "dx-pie", children: [
      /* @__PURE__ */ l.jsx("div", { className: "dx-estado", children: p && /* @__PURE__ */ l.jsx("span", { className: "dx-rojo", children: p }) }),
      /* @__PURE__ */ l.jsxs("div", { className: "panel dx-totales", children: [
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ l.jsx("span", { children: "Total del pedido (sin impuestos)" }),
          /* @__PURE__ */ l.jsx("span", { children: T(S == null ? void 0 : S.total) })
        ] }),
        Pe !== 0 && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Costes añadidos al almacén" }),
          /* @__PURE__ */ l.jsx("span", { children: T(Pe) })
        ] }),
        /* @__PURE__ */ l.jsx("div", { className: "muted", style: { fontSize: 12, marginTop: 6 }, children: "El impuesto se aplica al facturar el pedido." })
      ] })
    ] })
  ] });
}
function pl(e) {
  return /* @__PURE__ */ l.jsxs("div", { className: "panel-head", children: [
    /* @__PURE__ */ l.jsxs("h2", { style: { display: "flex", alignItems: "center", gap: 10 }, children: [
      /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: e.volver, title: "Volver a la lista", children: "←" }),
      e.titulo,
      e.estado && /* @__PURE__ */ l.jsx("span", { className: Ao(e.estado), children: e.estado }),
      e.extra
    ] }),
    /* @__PURE__ */ l.jsx("div", { className: "dx-acciones", children: e.acciones })
  ] });
}
function Te(e) {
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-dato", children: [
    /* @__PURE__ */ l.jsx("small", { children: e.etiqueta }),
    /* @__PURE__ */ l.jsx("div", { children: e.children })
  ] });
}
function Wn(e) {
  return /* @__PURE__ */ l.jsx("button", { type: "button", className: "dx-enlace", onClick: e.alPulsar, children: e.children });
}
function ml(e) {
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
function Em(e) {
  const { api: t, anfitrion: n, navegar: r } = kt(), { dato: a, error: i, recargar: o } = ml(() => t.get(`/facturas/${e.id}`)), [s, c] = g.useState(null), [d, N] = g.useState(!1), [u, h] = g.useState("");
  g.useEffect(() => void t.get(`/facturas/${e.id}/saldo`).then(c).catch(() => c(null)), [t, e.id, a]);
  const [v, y] = g.useState([]), [S, O] = g.useState([]), [p, f] = g.useState(null);
  g.useEffect(() => {
    !(a != null && a.clienteId) || a.estado !== "Emitida" || t.get(`/anticipos?clienteId=${a.clienteId}`).then((k) => {
      y(Oo(k)), O(kd(k));
    }).catch(() => y([]));
  }, [t, a]);
  const x = v.reduce((k, A) => k + A.disponible, 0);
  if (i) return /* @__PURE__ */ l.jsx("div", { className: "panel", children: /* @__PURE__ */ l.jsx("p", { className: "dx-rojo", children: i }) });
  if (!a) return /* @__PURE__ */ l.jsx("div", { className: "muted", children: "Cargando…" });
  const C = { clienteId: a.clienteId ?? void 0, lineas: a.lineas }, _ = a.lineas.reduce((k, A) => k + (A.base - A.margen), 0), F = a.estado === "Emitida", z = a.lineas.some((k) => k.cuentaContable === "438" && !k.anticipoId);
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsx(
        pl,
        {
          titulo: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            a.tipo === "Rectificativa" ? "Rectificativa" : a.tipo === "Simplificada" ? "Ticket" : "Factura",
            " ",
            /* @__PURE__ */ l.jsx("span", { className: "mono", children: a.numeroCompleto })
          ] }),
          estado: a.estado,
          extra: z ? /* @__PURE__ */ l.jsx("span", { className: "pill part", children: "Factura de anticipo" }) : a.lineas.some((k) => k.anticipoId) ? /* @__PURE__ */ l.jsx("span", { className: "pill", children: "Descuenta anticipos" }) : null,
          volver: () => r({ tipo: "factura", pantalla: "lista" }),
          acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => Vr(n, `/facturas/${a.id}/pdf`).catch((k) => n.aviso(k.message, "err")), children: "PDF" }),
            a.tipo !== "Simplificada" && a.clienteNif && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => Vr(n, `/facturas/${a.id}/facturae.xml`).catch((k) => n.aviso(k.message, "err")), children: "Facturae" }),
            a.estado !== "Borrador" && a.tipo !== "Simplificada" && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", title: "Factura EDIFACT INVOIC (EANCOM) para clientes con EDI", onClick: () => cm(n, `/integraciones/edi/facturas/${a.id}/invoic`).catch((k) => n.aviso(k.message, "err")), children: "EDI" }),
            /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "factura", pantalla: "editor", semilla: C }), children: "Duplicar" }),
            F && a.tipo === "Ordinaria" && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "factura", pantalla: "editor", semilla: { ...C, rectificaId: a.id, rectificaNumero: a.numeroCompleto } }), children: "Rectificar" }),
            F && /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", onClick: () => N(!0), children: "Anular" })
          ] })
        }
      ),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ l.jsxs(Te, { etiqueta: "Cliente", children: [
          /* @__PURE__ */ l.jsx("strong", { children: a.clienteNombre }),
          a.clienteNif && /* @__PURE__ */ l.jsx("div", { className: "muted mono", children: a.clienteNif }),
          /* @__PURE__ */ l.jsx("div", { className: "muted", children: [a.clienteCalle, a.clienteCodigoPostal, a.clientePoblacion].filter(Boolean).join(" ") })
        ] }),
        /* @__PURE__ */ l.jsxs(Te, { etiqueta: "Emisión", children: [
          je(a.fechaEmision),
          a.fechaOperacion !== a.fechaEmision && /* @__PURE__ */ l.jsxs("div", { className: "muted", children: [
            "Operación ",
            je(a.fechaOperacion)
          ] })
        ] }),
        /* @__PURE__ */ l.jsx(Te, { etiqueta: "Vencimiento", children: je(a.fechaVencimiento) }),
        /* @__PURE__ */ l.jsxs(Te, { etiqueta: "Cobro", children: [
          s ? s.pendiente <= 0 ? /* @__PURE__ */ l.jsx("span", { className: "pill ok", children: "Cobrada" }) : /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            "Pendiente ",
            /* @__PURE__ */ l.jsx("strong", { children: T(s.pendiente) }),
            s.liquidado > 0 && /* @__PURE__ */ l.jsxs("div", { className: "muted", children: [
              "Cobrado ",
              T(s.liquidado)
            ] })
          ] }) : "—",
          s && s.pendiente > 0 && F && n.irA && /* @__PURE__ */ l.jsx("div", { children: /* @__PURE__ */ l.jsx(Wn, { alPulsar: () => n.irA("cobros"), children: "Registrar cobro" }) }),
          s && s.pendiente > 0 && F && S.length > 0 && !z && /* @__PURE__ */ l.jsxs("div", { className: "dx-anticipo", children: [
            "🧾 Anticipos facturados sin descontar: ",
            /* @__PURE__ */ l.jsx("strong", { children: T(S.reduce((k, A) => k + (A.disponibleBase ?? 0), 0)) }),
            " de base. Se descuentan al hacer la siguiente factura (o rectifica esta para incluirlos)."
          ] }),
          s && s.pendiente > 0 && F && x > 0 && /* @__PURE__ */ l.jsxs("div", { className: "dx-anticipo", children: [
            "💶 Anticipos del cliente sin aplicar: ",
            /* @__PURE__ */ l.jsx("strong", { children: T(x) }),
            /* @__PURE__ */ l.jsx("div", { children: /* @__PURE__ */ l.jsx(Wn, { alPulsar: () => f(Cd(v, s.pendiente)), children: "Aplicar a esta factura" }) })
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
        /* @__PURE__ */ l.jsx("tbody", { children: a.lineas.map((k, A) => /* @__PURE__ */ l.jsxs("tr", { children: [
          /* @__PURE__ */ l.jsxs("td", { children: [
            k.descripcion,
            /* @__PURE__ */ l.jsx(fl, { conceptos: k.conceptos })
          ] }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: Ae(k.cantidad) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: T(k.precioUnitario) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: k.porcentajeDescuento ? `${we(k.porcentajeDescuento)} %` : "" }),
          /* @__PURE__ */ l.jsxs("td", { children: [
            k.codigoIva,
            " · ",
            we(k.porcentajeIva),
            " %"
          ] }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: T(k.base) }) }),
          /* @__PURE__ */ l.jsx("td", { className: "num muted", children: k.costeUnitario || k.costeConceptos ? T(k.margen) : "" })
        ] }, A)) })
      ] }),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-totales-vista", children: [
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Base imponible" }),
          /* @__PURE__ */ l.jsx("span", { children: T(a.baseImponible) })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: a.impuesto === "Igic" ? "IGIC" : "IVA" }),
          /* @__PURE__ */ l.jsx("span", { children: T(a.cuotaIva) })
        ] }),
        !!a.recargoTotal && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Recargo de equivalencia" }),
          /* @__PURE__ */ l.jsx("span", { children: T(a.recargoTotal) })
        ] }),
        !!a.retencionIrpf && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
            "Retención IRPF (",
            we(a.porcentajeIrpf),
            " %)"
          ] }),
          /* @__PURE__ */ l.jsxs("span", { children: [
            "−",
            T(a.retencionIrpf)
          ] })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ l.jsx("span", { children: "Total" }),
          /* @__PURE__ */ l.jsx("span", { children: T(a.total) })
        ] }),
        _ > 0 && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Margen" }),
          /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
            T(a.baseImponible - _),
            " (",
            we(a.baseImponible ? (a.baseImponible - _) / a.baseImponible * 100 : 0),
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
      nn,
      {
        titulo: `Aplicar anticipos a ${a.numeroCompleto}`,
        alCerrar: () => f(null),
        acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => f(null), children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", disabled: !p.some((k) => k.importe > 0), onClick: async () => {
            for (const k of p.filter((A) => A.importe > 0))
              if (!await lt(() => t.post(`/anticipos/${k.id}/aplicar`, { facturaId: a.id, importe: k.importe }), n.aviso, "Anticipo aplicado.")) return;
            f(null), o();
          }, children: "Aplicar" })
        ] }),
        children: [
          /* @__PURE__ */ l.jsxs("p", { className: "muted", style: { marginTop: 0 }, children: [
            "Se registra el cobro de la factura con el anticipo y su asiento de cancelación: 438 Anticipos de clientes al debe, 430 Clientes al haber. Pendiente de la factura: ",
            /* @__PURE__ */ l.jsx("strong", { children: T(s == null ? void 0 : s.pendiente) }),
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
              const A = p.find((U) => U.id === k.id);
              return /* @__PURE__ */ l.jsxs("tr", { children: [
                /* @__PURE__ */ l.jsx("td", { children: je(k.fecha) }),
                /* @__PURE__ */ l.jsx("td", { className: "muted", children: k.concepto }),
                /* @__PURE__ */ l.jsx("td", { className: "num", children: T(k.disponible) }),
                /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx(
                  "input",
                  {
                    type: "number",
                    step: "0.01",
                    min: 0,
                    max: k.disponible,
                    style: { width: 110, textAlign: "right" },
                    value: (A == null ? void 0 : A.importe) ?? 0,
                    onChange: (U) => {
                      const P = Math.max(0, Math.min(k.disponible, Number(U.target.value) || 0));
                      f([...p.filter((G) => G.id !== k.id), { id: k.id, importe: P }]);
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
      nn,
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
function Im(e) {
  const { api: t, anfitrion: n, navegar: r } = kt(), { dato: a, error: i, recargar: o } = ml(() => t.get(`/presupuestos/${e.id}`));
  if (i) return /* @__PURE__ */ l.jsx("div", { className: "panel", children: /* @__PURE__ */ l.jsx("p", { className: "dx-rojo", children: i }) });
  if (!a) return /* @__PURE__ */ l.jsx("div", { className: "muted", children: "Cargando…" });
  const s = a.estado === "Borrador", c = { clienteId: a.clienteId, lineas: a.lineas };
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsx(
        pl,
        {
          titulo: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            "Presupuesto ",
            /* @__PURE__ */ l.jsx("span", { className: "mono", children: a.numeroCompleto })
          ] }),
          estado: a.estado,
          volver: () => r({ tipo: "presupuesto", pantalla: "lista" }),
          acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => Vr(n, `/presupuestos/${a.id}/pdf`).catch((d) => n.aviso(d.message, "err")), children: "PDF" }),
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
        /* @__PURE__ */ l.jsx(Te, { etiqueta: "Cliente", children: /* @__PURE__ */ l.jsx("strong", { children: a.clienteNombre }) }),
        /* @__PURE__ */ l.jsx(Te, { etiqueta: "Fecha", children: je(a.fecha) }),
        /* @__PURE__ */ l.jsx(Te, { etiqueta: "Válido hasta", children: je(a.validez) }),
        /* @__PURE__ */ l.jsx(Te, { etiqueta: "Factura", children: a.facturaId ? /* @__PURE__ */ l.jsx(Wn, { alPulsar: () => r({ tipo: "factura", pantalla: "vista", id: a.facturaId }), children: "ver la factura" }) : "—" })
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
            /* @__PURE__ */ l.jsx(fl, { conceptos: d.conceptos })
          ] }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: Ae(d.cantidad) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: T(d.precioUnitario) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: d.porcentajeDescuento ? `${we(d.porcentajeDescuento)} %` : "" }),
          /* @__PURE__ */ l.jsx("td", { children: d.codigoIva }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: T(d.base) }) })
        ] }, N)) })
      ] }),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-totales-vista", children: [
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Base imponible" }),
          /* @__PURE__ */ l.jsx("span", { children: T(a.baseImponible) })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Impuestos" }),
          /* @__PURE__ */ l.jsx("span", { children: T(a.cuotaIva) })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ l.jsx("span", { children: "Total" }),
          /* @__PURE__ */ l.jsx("span", { children: T(a.total) })
        ] })
      ] })
    ] })
  ] });
}
function Pm(e) {
  const { api: t, anfitrion: n, navegar: r } = kt(), { dato: a, error: i, recargar: o } = ml(() => t.get(`/pedidos-venta/${e.id}`)), [s, c] = g.useState([]), [d, N] = g.useState([]), [u, h] = g.useState(null), [v, y] = g.useState(""), [S, O] = g.useState(vt()), [p, f] = g.useState(!1), [x, C] = g.useState(vt()), [_, F] = g.useState("");
  if (g.useEffect(() => void t.get(`/pedidos-venta/${e.id}/albaranes`).then(c).catch(() => c([])), [t, e.id, a]), g.useEffect(() => void t.get("/formas-pago").then((P) => N(P.filter((G) => G.activo))).catch(() => N([])), [t]), i) return /* @__PURE__ */ l.jsx("div", { className: "panel", children: /* @__PURE__ */ l.jsx("p", { className: "dx-rojo", children: i }) });
  if (!a) return /* @__PURE__ */ l.jsx("div", { className: "muted", children: "Cargando…" });
  const z = (a.estado === "Borrador" || a.estado === "Confirmado") && a.lineas.every((P) => P.cantidadServida === 0), k = a.lineas.some((P) => P.pendienteServir > 0), A = a.estado !== "Cancelado" && a.estado !== "Facturado", U = { clienteId: a.clienteId, fecha: a.fecha, lineas: a.lineas };
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsx(
        pl,
        {
          titulo: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            "Pedido de venta ",
            /* @__PURE__ */ l.jsx("span", { className: "mono", children: a.numeroCompleto })
          ] }),
          estado: a.estado,
          volver: () => r({ tipo: "pedido", pantalla: "lista" }),
          acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => Vr(n, `/pedidos-venta/${a.id}/pdf`).catch((P) => n.aviso(P.message, "err")), children: "PDF" }),
            /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "pedido", pantalla: "editor", semilla: { ...U, fecha: void 0 } }), children: "Duplicar" }),
            z && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "pedido", pantalla: "editor", id: a.id, semilla: U }), children: "Editar" }),
            a.estado === "Borrador" && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: async () => await lt(() => t.post(`/pedidos-venta/${a.id}/confirmar`), n.aviso, "Pedido confirmado.") && o(), children: "Confirmar" }),
            A && a.estado !== "Borrador" && k && n.reservarPales && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", title: "Apartar palés cerrados para este pedido", onClick: () => n.reservarPales(a.id), children: "Reservar palés" }),
            A && a.estado !== "Borrador" && k && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => h(Object.fromEntries(a.lineas.map((P) => [P.id, P.pendienteServir]))), children: "Entregar (albarán)" }),
            A && a.estado !== "Borrador" && /* @__PURE__ */ l.jsx("button", { className: "btn small", onClick: () => f(!0), children: "Facturar" }),
            A && /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", onClick: async () => window.confirm("¿Cancelar el pedido?") && await lt(() => t.post(`/pedidos-venta/${a.id}/cancelar`), n.aviso, "Pedido cancelado.") && o(), children: "Cancelar" })
          ] })
        }
      ),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ l.jsx(Te, { etiqueta: "Cliente", children: /* @__PURE__ */ l.jsx("strong", { children: a.clienteNombre }) }),
        /* @__PURE__ */ l.jsx(Te, { etiqueta: "Fecha", children: je(a.fecha) }),
        /* @__PURE__ */ l.jsx(Te, { etiqueta: "Viene de", children: a.presupuestoOrigenId ? /* @__PURE__ */ l.jsx(Wn, { alPulsar: () => r({ tipo: "presupuesto", pantalla: "vista", id: a.presupuestoOrigenId }), children: "presupuesto" }) : "—" }),
        /* @__PURE__ */ l.jsx(Te, { etiqueta: "Factura", children: a.facturaId ? /* @__PURE__ */ l.jsx(Wn, { alPulsar: () => r({ tipo: "factura", pantalla: "vista", id: a.facturaId }), children: "ver la factura" }) : "—" })
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
            /* @__PURE__ */ l.jsx(fl, { conceptos: P.conceptos })
          ] }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: Ae(P.cantidad) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: Ae(P.cantidadServida) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: P.pendienteServir > 0 ? /* @__PURE__ */ l.jsx("strong", { children: Ae(P.pendienteServir) }) : "—" }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: T(P.precioUnitario) }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: P.porcentajeDescuento ? `${we(P.porcentajeDescuento)} %` : "" }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: T(P.base) }) })
        ] }, P.id)) })
      ] }),
      /* @__PURE__ */ l.jsx("div", { className: "dx-totales-vista", children: /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-grande", children: [
        /* @__PURE__ */ l.jsx("span", { children: "Total (sin impuestos)" }),
        /* @__PURE__ */ l.jsx("span", { children: T(a.total) })
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
          /* @__PURE__ */ l.jsx("td", { children: je(P.fecha) }),
          /* @__PURE__ */ l.jsx("td", { className: "muted", children: P.referencia }),
          /* @__PURE__ */ l.jsx("td", { className: "muted", children: P.lineas.map((G) => `${Ae(G.cantidad)} × ${G.descripcion}`).join(" · ") }),
          /* @__PURE__ */ l.jsx("td", { className: "right", children: !P.anulado && a.estado !== "Facturado" && /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", onClick: async () => {
            const G = window.prompt("Motivo de la anulación del albarán:");
            G !== null && await lt(() => t.post(`/pedidos-venta/${a.id}/albaranes/${P.id}/anular`, { motivo: G || null }), n.aviso, "Albarán anulado.") && o();
          }, children: "Anular" }) })
        ] }, P.id)) })
      ] }) : /* @__PURE__ */ l.jsx("p", { className: "muted", style: { margin: 0 }, children: "Sin entregas todavía." })
    ] }),
    u && /* @__PURE__ */ l.jsxs(
      nn,
      {
        titulo: "Entrega (albarán de venta)",
        alCerrar: () => h(null),
        ancho: 640,
        acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => h(null), children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", onClick: async () => await lt(() => t.post(`/pedidos-venta/${a.id}/entregar`, { fecha: S, referencia: v || null, lineas: Object.entries(u).filter(([, P]) => P > 0).map(([P, G]) => ({ lineaPedidoId: P, cantidad: G })) }), n.aviso, "Albarán creado.") && (h(null), o()), children: "Crear albarán" })
        ] }),
        children: [
          /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Fecha" }),
              /* @__PURE__ */ l.jsx("input", { type: "date", value: S, onChange: (P) => O(P.target.value) })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Referencia" }),
              /* @__PURE__ */ l.jsx("input", { value: v, onChange: (P) => y(P.target.value) })
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
              /* @__PURE__ */ l.jsx("td", { className: "num", children: Ae(P.pendienteServir) }),
              /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx("input", { className: "num", type: "number", step: "0.001", value: u[P.id] ?? 0, onChange: (G) => h({ ...u, [P.id]: Number(G.target.value) }) }) })
            ] }, P.id)) })
          ] })
        ]
      }
    ),
    p && /* @__PURE__ */ l.jsxs(
      nn,
      {
        titulo: `Facturar el pedido ${a.numeroCompleto}`,
        alCerrar: () => f(!1),
        acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => f(!1), children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", onClick: async () => {
            try {
              const P = await t.post(`/pedidos-venta/${a.id}/facturar`, { fechaEmision: x, formaPagoId: _ || null });
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
              /* @__PURE__ */ l.jsx("input", { type: "date", value: x, onChange: (P) => C(P.target.value) })
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
function Fm(e) {
  const { api: t, anfitrion: n, navegar: r } = kt(), { dato: a, error: i, recargar: o } = ml(() => t.get(`/compras/pedidos/${e.id}`)), [s, c] = g.useState([]), [d, N] = g.useState([]), [u, h] = g.useState([]), [v, y] = g.useState(null), [S, O] = g.useState(""), [p, f] = g.useState(""), [x, C] = g.useState(vt()), [_, F] = g.useState(!1), [z, k] = g.useState("IVA21"), [A, U] = g.useState(0), [P, G] = g.useState(""), [se, Pe] = g.useState(vt());
  if (g.useEffect(() => void t.get(`/compras/pedidos/${e.id}/albaranes`).then(c).catch(() => c([])), [t, e.id, a]), g.useEffect(() => {
    t.get("/inventario/almacenes").then((j) => (N(j), j[0] && O(j[0].id))).catch(() => N([])), t.get("/tipos-iva").then((j) => h(j.filter((D) => D.activo))).catch(() => h([]));
  }, [t]), i) return /* @__PURE__ */ l.jsx("div", { className: "panel", children: /* @__PURE__ */ l.jsx("p", { className: "dx-rojo", children: i }) });
  if (!a) return /* @__PURE__ */ l.jsx("div", { className: "muted", children: "Cargando…" });
  const De = (a.estado === "Borrador" || a.estado === "Confirmado") && a.lineas.every((j) => j.cantidadRecibida === 0 && j.cantidadFacturada === 0) && !a.empresaOrigenId, fe = a.estado !== "Cancelado" && a.estado !== "Facturado", me = a.lineas.some((j) => j.pendienteRecibir > 0), E = a.lineas.reduce((j, D) => j + D.costeConceptos, 0);
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsx(
        pl,
        {
          titulo: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            "Pedido de compra ",
            /* @__PURE__ */ l.jsx("span", { className: "mono", children: a.numeroCompleto })
          ] }),
          estado: a.estado,
          volver: () => r({ tipo: "compra", pantalla: "lista" }),
          acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
            /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => Vr(n, `/compras/pedidos/${a.id}/pdf`).catch((j) => n.aviso(j.message, "err")), children: "PDF" }),
            !a.empresaOrigenId && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "compra", pantalla: "editor", semilla: { ...a, fecha: vt() } }), children: "Duplicar" }),
            De && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "compra", pantalla: "editor", id: a.id, semilla: a }), children: "Editar" }),
            a.estado === "Borrador" && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: async () => await lt(() => t.post(`/compras/pedidos/${a.id}/confirmar`), n.aviso, "Pedido confirmado.") && o(), children: "Confirmar" }),
            fe && a.estado !== "Borrador" && me && /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => y(Object.fromEntries(a.lineas.map((j) => [j.id, { cantidad: j.pendienteRecibir, lote: "" }]))), children: "Recibir mercancía" }),
            fe && a.estado !== "Borrador" && /* @__PURE__ */ l.jsx("button", { className: "btn small", onClick: () => F(!0), children: "Facturar" }),
            fe && !a.empresaOrigenId && /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", onClick: async () => window.confirm("¿Cancelar el pedido?") && await lt(() => t.post(`/compras/pedidos/${a.id}/cancelar`), n.aviso, "Pedido cancelado.") && o(), children: "Cancelar" })
          ] })
        }
      ),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ l.jsx(Te, { etiqueta: "Proveedor", children: /* @__PURE__ */ l.jsx("strong", { children: a.proveedorTexto }) }),
        /* @__PURE__ */ l.jsx(Te, { etiqueta: "Fecha", children: je(a.fecha) }),
        /* @__PURE__ */ l.jsx(Te, { etiqueta: "Total", children: T(a.total) }),
        /* @__PURE__ */ l.jsx(Te, { etiqueta: "Costes añadidos", children: E ? T(E) : "—" })
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
          /* @__PURE__ */ l.jsx(fl, { conceptos: j.conceptos })
        ] }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: Ae(j.cantidad) }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: Ae(j.cantidadRecibida) }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: j.pendienteRecibir > 0 ? /* @__PURE__ */ l.jsx("strong", { children: Ae(j.pendienteRecibir) }) : "—" }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: T(j.precioUnitario) }),
        /* @__PURE__ */ l.jsx("td", { className: "num", children: /* @__PURE__ */ l.jsx("strong", { children: T(j.importe) }) }),
        /* @__PURE__ */ l.jsxs("td", { className: "num muted", children: [
          T(j.costeUnitarioEntrada),
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
          var D;
          return /* @__PURE__ */ l.jsxs("tr", { children: [
            /* @__PURE__ */ l.jsxs("td", { className: "mono", children: [
              /* @__PURE__ */ l.jsx("strong", { children: j.numeroCompleto }),
              " ",
              j.anulado && /* @__PURE__ */ l.jsx("span", { className: "pill neg", title: j.motivoAnulacion ?? "", children: "Anulado" })
            ] }),
            /* @__PURE__ */ l.jsx("td", { children: je(j.fecha) }),
            /* @__PURE__ */ l.jsx("td", { className: "muted", children: j.referencia }),
            /* @__PURE__ */ l.jsx("td", { className: "muted", children: ((D = d.find((H) => H.id === j.almacenId)) == null ? void 0 : D.nombre) ?? "—" }),
            /* @__PURE__ */ l.jsx("td", { className: "muted", children: j.lineas.map((H) => `${Ae(H.cantidad)} × ${H.descripcion}`).join(" · ") }),
            /* @__PURE__ */ l.jsx("td", { className: "right", children: !j.anulado && a.estado !== "Facturado" && /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", onClick: async () => {
              const H = window.prompt("Motivo de la anulación del albarán:");
              H !== null && await lt(() => t.post(`/compras/pedidos/${a.id}/albaranes/${j.id}/anular`, { motivo: H || null }), n.aviso, "Albarán anulado.") && o();
            }, children: "Anular" }) })
          ] }, j.id);
        }) })
      ] }) : /* @__PURE__ */ l.jsx("p", { className: "muted", style: { margin: 0 }, children: "Sin recepciones todavía." })
    ] }),
    v && /* @__PURE__ */ l.jsxs(
      nn,
      {
        titulo: "Recepción de mercancía",
        alCerrar: () => y(null),
        ancho: 680,
        acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => y(null), children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", onClick: async () => await lt(() => t.post(`/compras/pedidos/${a.id}/recibir`, { fecha: x, referencia: p || null, almacenId: S || null, lineas: Object.entries(v).filter(([, j]) => j.cantidad > 0).map(([j, D]) => ({ lineaPedidoId: j, cantidad: D.cantidad, lote: D.lote || null })) }), n.aviso, "Recepción registrada.") && (y(null), o()), children: "Registrar recepción" })
        ] }),
        children: [
          /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Fecha" }),
              /* @__PURE__ */ l.jsx("input", { type: "date", value: x, onChange: (j) => C(j.target.value) })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Albarán del proveedor" }),
              /* @__PURE__ */ l.jsx("input", { value: p, onChange: (j) => f(j.target.value) })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Almacén" }),
              /* @__PURE__ */ l.jsxs("select", { value: S, onChange: (j) => O(j.target.value), children: [
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
              var D, H;
              return /* @__PURE__ */ l.jsxs("tr", { children: [
                /* @__PURE__ */ l.jsx("td", { children: j.descripcion }),
                /* @__PURE__ */ l.jsx("td", { className: "num", children: Ae(j.pendienteRecibir) }),
                /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx("input", { className: "num", type: "number", step: "0.001", value: ((D = v[j.id]) == null ? void 0 : D.cantidad) ?? 0, onChange: (Q) => y({ ...v, [j.id]: { ...v[j.id], cantidad: Number(Q.target.value) } }) }) }),
                /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx("input", { value: ((H = v[j.id]) == null ? void 0 : H.lote) ?? "", onChange: (Q) => y({ ...v, [j.id]: { ...v[j.id], lote: Q.target.value } }) }) })
              ] }, j.id);
            }) })
          ] })
        ]
      }
    ),
    _ && /* @__PURE__ */ l.jsxs(
      nn,
      {
        titulo: `Facturar el pedido ${a.numeroCompleto}`,
        alCerrar: () => F(!1),
        acciones: /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: () => F(!1), children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", onClick: async () => await lt(() => t.post(`/compras/pedidos/${a.id}/facturar`, { codigoIva: z, porcentajeIrpf: A, numeroFactura: P || null, fechaFactura: se }), n.aviso, "Factura del proveedor registrada como gasto.") && (F(!1), o()), children: "Registrar factura" })
        ] }),
        children: [
          /* @__PURE__ */ l.jsxs("p", { className: "muted", style: { marginTop: 0 }, children: [
            "Registra la factura del proveedor por el total del pedido (",
            T(a.total),
            ") como gasto, con su asiento si la contabilidad es automática."
          ] }),
          /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Nº de factura del proveedor" }),
              /* @__PURE__ */ l.jsx("input", { value: P, onChange: (j) => G(j.target.value), autoFocus: !0 })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Fecha de la factura" }),
              /* @__PURE__ */ l.jsx("input", { type: "date", value: se, onChange: (j) => Pe(j.target.value) })
            ] })
          ] }),
          /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Impuesto" }),
              /* @__PURE__ */ l.jsx("select", { value: z, onChange: (j) => k(j.target.value), children: u.map((j) => /* @__PURE__ */ l.jsx("option", { value: j.codigo, children: j.nombre }, j.codigo)) })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Retención IRPF %" }),
              /* @__PURE__ */ l.jsx("input", { type: "number", step: "0.01", value: A, onChange: (j) => U(Number(j.target.value)) })
            ] })
          ] })
        ]
      }
    )
  ] });
}
const Gl = (e = "") => ({ clave: Kr(), descripcion: "", cuentaGasto: "", base: 0, codigoIva: e, porcentajeIva: null, porcentajeDeducible: 100 }), Rm = (e) => e === "InversionSujetoPasivo" || e === "Intracomunitario";
function _m(e) {
  const { api: t, anfitrion: n } = kt(), r = e.semilla, [a, i] = g.useState([]), [o, s] = g.useState([]), [c, d] = g.useState([]), [N, u] = g.useState([]), [h, v] = g.useState((r == null ? void 0 : r.proveedorId) ?? ""), [y, S] = g.useState(e.id ? (r == null ? void 0 : r.numeroFactura) ?? "" : ""), [O, p] = g.useState((r == null ? void 0 : r.fechaFactura) ?? vt()), [f, x] = g.useState(e.id ? (r == null ? void 0 : r.fecha) ?? vt() : vt()), [C, _] = g.useState(r != null && r.concepto && !r.concepto.startsWith("Factura ") ? r.concepto : ""), [F, z] = g.useState((r == null ? void 0 : r.porcentajeIrpf) ?? 0), [k, A] = g.useState(""), [U, P] = g.useState(((r == null ? void 0 : r.recargoTotal) ?? 0) > 0), G = !!(r != null && r.esRectificativa), [se, Pe] = g.useState((r == null ? void 0 : r.numeroRectificado) ?? ""), [De, fe] = g.useState((r == null ? void 0 : r.fechaRectificada) ?? ""), [me, E] = g.useState((r == null ? void 0 : r.motivoRectificacion) ?? ""), [j, D] = g.useState(!1), [H, Q] = g.useState((r == null ? void 0 : r.afectacion) ?? "Comun"), [q, he] = g.useState(
    () => {
      var m;
      return (m = r == null ? void 0 : r.lineas) != null && m.length ? r.lineas.map((L) => ({ clave: Kr(), descripcion: L.descripcion ?? "", cuentaGasto: L.cuentaGasto ?? "", base: L.base, codigoIva: L.codigoIva, porcentajeIva: L.autoliquidada ? L.porcentajeIva : null, porcentajeDeducible: L.porcentajeDeducible })) : [Gl()];
    }
  ), [M, te] = g.useState(e.id && (r != null && r.vencimientos) && r.vencimientos.length > 1 ? r.vencimientos : null), [V, Ge] = g.useState(null), [nt, Lt] = g.useState(""), [At, Mt] = g.useState(!1), kn = qr();
  g.useEffect(() => {
    t.get("/proveedores").then(i).catch(() => i([])), t.get("/tipos-iva").then((m) => s(m.filter((L) => L.activo))).catch(() => s([])), t.get("/formas-pago").then((m) => d(m.filter((L) => L.activo))).catch(() => d([])), t.get("/empresas/actual").then((m) => {
      m.regimenIva === "RecargoEquivalencia" && (D(!0), r || P(!0));
    }).catch(() => {
    }), t.get("/contabilidad/cuentas").then((m) => u(m.filter((L) => L.codigo.startsWith("6") || L.codigo.startsWith("2")))).catch(() => u([]));
  }, [t]);
  const ne = a.find((m) => m.id === h);
  g.useEffect(() => {
    ne != null && ne.formaPagoDefectoId && !k && A(ne.formaPagoDefectoId);
  }, [ne]);
  const xe = g.useMemo(
    () => ({
      proveedorId: h || null,
      proveedorTexto: (ne == null ? void 0 : ne.nombre) ?? null,
      numeroFactura: y.trim() || null,
      fechaFactura: O || null,
      fecha: f,
      concepto: C.trim() || null,
      porcentajeIrpf: F,
      formaPagoId: k || null,
      recargoEquivalencia: U,
      afectacion: H,
      baseImponible: 0,
      lineas: q.filter((m) => m.base !== 0).map((m) => ({
        base: m.base,
        codigoIva: m.codigoIva || null,
        descripcion: m.descripcion.trim() || null,
        porcentajeIva: m.porcentajeIva,
        porcentajeDeducible: m.porcentajeDeducible,
        cuentaGasto: m.cuentaGasto.trim() || null
      })),
      vencimientos: M,
      rectificaGastoId: G ? (r == null ? void 0 : r.rectificaGastoId) ?? null : null,
      numeroRectificado: G && se.trim() || null,
      fechaRectificada: G && De || null,
      motivoRectificacion: G && me.trim() || null
    }),
    [h, ne, y, O, f, C, F, k, U, H, q, M, G, r, se, De, me]
  ), Ke = vn(xe, 350);
  g.useEffect(() => {
    if (!Ke.lineas.length) {
      Ge(null), Lt("Añade al menos una línea con base.");
      return;
    }
    const m = kn();
    t.post("/gastos/simular", Ke).then((L) => m() && (Ge(L), Lt(""))).catch((L) => m() && (Ge(null), Lt(L.message)));
  }, [Ke, t]);
  const qe = (m, L) => he((R) => R.map(($) => $.clave === m ? { ...$, ...L } : $)), $t = (m) => o.find((L) => L.codigo === m), gt = (m) => {
    var L;
    return (L = V == null ? void 0 : V.lineas) == null ? void 0 : L[q.filter((R) => R.base !== 0).indexOf(m)];
  };
  function Ue(m) {
    if (!V) return;
    const L = /* @__PURE__ */ new Date((O || f) + "T00:00:00"), R = Me(V.total / m);
    te(Array.from({ length: m }, ($, W) => {
      const Z = new Date(L);
      return Z.setMonth(Z.getMonth() + W + 1), { fecha: Z.toISOString().slice(0, 10), importe: W === m - 1 ? Me(V.total - R * (m - 1)) : R };
    }));
  }
  async function on() {
    Mt(!0);
    try {
      const m = e.id ? await t.put(`/gastos/${e.id}`, xe) : await t.post("/gastos", xe);
      n.aviso(e.id ? "Factura corregida." : "Factura registrada.", "ok"), m.avisoRiesgo && n.aviso(m.avisoRiesgo, "err"), e.alGuardar(m.id);
    } catch (m) {
      n.aviso(m.message, "err");
    } finally {
      Mt(!1);
    }
  }
  const sn = Me((M ?? []).reduce((m, L) => m + (Number(L.importe) || 0), 0));
  return /* @__PURE__ */ l.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ l.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ l.jsxs("div", { className: "panel-head", children: [
        /* @__PURE__ */ l.jsx("h2", { children: e.id ? `Corregir factura ${(r == null ? void 0 : r.numeroFactura) ?? ""}` : G ? "Rectificativa / abono del proveedor" : "Nueva factura de proveedor" }),
        /* @__PURE__ */ l.jsxs("div", { style: { display: "flex", gap: 8 }, children: [
          /* @__PURE__ */ l.jsx("button", { className: "btn small secondary", onClick: e.alCancelar, children: "Cancelar" }),
          /* @__PURE__ */ l.jsx("button", { className: "btn small", disabled: !V || At, onClick: on, children: e.id ? "Guardar corrección" : G ? "Registrar abono" : "Registrar factura" })
        ] })
      ] }),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-cabecera", children: [
        /* @__PURE__ */ l.jsxs("div", { className: "dx-cab-campos", children: [
          /* @__PURE__ */ l.jsx(Mo, { terceros: a, valor: h, alCambiar: v, etiqueta: "Proveedor" }),
          /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Nº de factura del proveedor" }),
              /* @__PURE__ */ l.jsx("input", { value: y, onChange: (m) => S(m.target.value), placeholder: "F-2026/0117" })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Fecha de la factura" }),
              /* @__PURE__ */ l.jsx("input", { type: "date", value: O, onChange: (m) => p(m.target.value) })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Fecha de registro" }),
              /* @__PURE__ */ l.jsx("input", { type: "date", value: f, onChange: (m) => x(m.target.value), title: "La del asiento y la del periodo de IVA en que se deduce" })
            ] })
          ] }),
          /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Forma de pago" }),
              /* @__PURE__ */ l.jsxs("select", { value: k, onChange: (m) => A(m.target.value), children: [
                /* @__PURE__ */ l.jsx("option", { value: "", children: "Contado (vence en la fecha de factura)" }),
                c.map((m) => /* @__PURE__ */ l.jsx("option", { value: m.id, children: m.nombre }, m.id))
              ] })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Retención IRPF %" }),
              /* @__PURE__ */ l.jsx("input", { type: "number", step: "0.01", value: F, onChange: (m) => z(Number(m.target.value)) })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Prorrata especial" }),
              /* @__PURE__ */ l.jsxs("select", { value: H, onChange: (m) => Q(m.target.value), children: [
                /* @__PURE__ */ l.jsx("option", { value: "Comun", children: "Uso común" }),
                /* @__PURE__ */ l.jsx("option", { value: "ConDerecho", children: "Solo operaciones con derecho" }),
                /* @__PURE__ */ l.jsx("option", { value: "SinDerecho", children: "Solo operaciones exentas" })
              ] })
            ] })
          ] }),
          /* @__PURE__ */ l.jsx("div", { className: "dx-fila", children: /* @__PURE__ */ l.jsxs("div", { style: { gridColumn: "1 / -1" }, children: [
            /* @__PURE__ */ l.jsx("label", { children: "Concepto (vacío: «Factura nº»)" }),
            /* @__PURE__ */ l.jsx("input", { value: C, onChange: (m) => _(m.target.value) })
          ] }) }),
          G && /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Factura que rectifica" }),
              /* @__PURE__ */ l.jsx("input", { value: se, disabled: !!(r != null && r.rectificaGastoId), onChange: (m) => Pe(m.target.value) })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Fecha de la rectificada" }),
              /* @__PURE__ */ l.jsx("input", { type: "date", value: De, disabled: !!(r != null && r.rectificaGastoId), onChange: (m) => fe(m.target.value) })
            ] }),
            /* @__PURE__ */ l.jsxs("div", { children: [
              /* @__PURE__ */ l.jsx("label", { children: "Motivo" }),
              /* @__PURE__ */ l.jsx("input", { value: me, onChange: (m) => E(m.target.value), placeholder: "Devolución, descuento posterior, error de precio…" })
            ] })
          ] }),
          G && /* @__PURE__ */ l.jsx("div", { className: "muted", children: "Las bases del abono van en negativo; el asiento y el SII (R1, por diferencias) salen con el signo." }),
          /* @__PURE__ */ l.jsxs("label", { className: "dx-check", children: [
            /* @__PURE__ */ l.jsx("input", { type: "checkbox", checked: U, onChange: (m) => P(m.target.checked) }),
            " El proveedor me cobra recargo de equivalencia"
          ] }),
          j && /* @__PURE__ */ l.jsx("div", { className: "muted", children: "La empresa está en recargo de equivalencia: el IVA y el recargo soportados son coste (no se deducen)." })
        ] }),
        /* @__PURE__ */ l.jsx("div", { className: "dx-ficha", children: ne ? /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          /* @__PURE__ */ l.jsx("strong", { children: ne.nombre }),
          /* @__PURE__ */ l.jsx("div", { className: "muted", children: [ne.nifFiscal, ne.poblacion, ne.pais].filter(Boolean).join(" · ") }),
          !ne.nifFiscal && /* @__PURE__ */ l.jsx("div", { className: "dx-aviso", children: "⚠ Sin NIF en la ficha: hace falta para el libro de IVA, el 347 y el SII." }),
          (V == null ? void 0 : V.avisoRiesgo) && /* @__PURE__ */ l.jsxs("div", { className: "dx-aviso", children: [
            "⚠ ",
            V.avisoRiesgo
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
        /* @__PURE__ */ l.jsx("tbody", { children: q.map((m, L) => {
          const R = $t(m.codigoIva), $ = gt(m);
          return /* @__PURE__ */ l.jsxs("tr", { className: L % 2 ? "dx-par" : "", children: [
            /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx("input", { value: m.descripcion, onChange: (W) => qe(m.clave, { descripcion: W.target.value }), placeholder: "Qué se compra" }) }),
            /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx("input", { list: "dx-cuentas-gasto", value: m.cuentaGasto, onChange: (W) => qe(m.clave, { cuentaGasto: W.target.value }), placeholder: "la de la regla" }) }),
            /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx("input", { className: "num", type: "number", step: "0.01", value: m.base || "", onChange: (W) => qe(m.clave, { base: Number(W.target.value) }) }) }),
            /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsxs("select", { value: m.codigoIva, onChange: (W) => qe(m.clave, { codigoIva: W.target.value, porcentajeIva: null }), children: [
              /* @__PURE__ */ l.jsx("option", { value: "", children: "General" }),
              o.map((W) => /* @__PURE__ */ l.jsx("option", { value: W.codigo, children: W.nombre }, W.codigo))
            ] }) }),
            /* @__PURE__ */ l.jsx("td", { children: Rm(R == null ? void 0 : R.clase) ? /* @__PURE__ */ l.jsx("input", { className: "num", type: "number", step: "0.01", value: m.porcentajeIva ?? "", placeholder: "21", title: "Tipo que autoliquidas", onChange: (W) => qe(m.clave, { porcentajeIva: W.target.value === "" ? null : Number(W.target.value) }) }) : /* @__PURE__ */ l.jsx("span", { className: "muted", children: $ ? `${we($.porcentajeIva)} %` : "" }) }),
            /* @__PURE__ */ l.jsx("td", { children: /* @__PURE__ */ l.jsx("input", { className: "num", type: "number", step: "1", min: 0, max: 100, value: m.porcentajeDeducible, onChange: (W) => qe(m.clave, { porcentajeDeducible: Number(W.target.value) }) }) }),
            /* @__PURE__ */ l.jsx("td", { className: "num", children: $ ? /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
              /* @__PURE__ */ l.jsx("strong", { children: T($.cuota) }),
              $.autoliquidada && /* @__PURE__ */ l.jsx("div", { className: "dx-sub", children: "autoliquidada" }),
              $.cuotaRecargo !== 0 && /* @__PURE__ */ l.jsxs("div", { className: "dx-sub", children: [
                "+ recargo ",
                T($.cuotaRecargo)
              ] })
            ] }) : "—" }),
            /* @__PURE__ */ l.jsx("td", { className: "right", children: /* @__PURE__ */ l.jsx("button", { className: "dx-icono", title: "Quitar", onClick: () => he((W) => W.length > 1 ? W.filter((Z) => Z.clave !== m.clave) : [Gl()]), children: "✕" }) })
          ] }, m.clave);
        }) })
      ] }),
      /* @__PURE__ */ l.jsx("datalist", { id: "dx-cuentas-gasto", children: N.map((m) => /* @__PURE__ */ l.jsx("option", { value: m.codigo, children: m.nombre }, m.codigo)) }),
      /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", style: { marginTop: 8 }, onClick: () => he((m) => {
        var L;
        return [...m, Gl(((L = m[m.length - 1]) == null ? void 0 : L.codigoIva) ?? "")];
      }), children: "+ Añadir línea" }),
      /* @__PURE__ */ l.jsx("span", { className: "muted dx-ayuda", children: "Una línea por cada base e impuesto de la factura. En inversión del sujeto pasivo e intracomunitarias el IVA lo autoliquidas tú (no lo cobra el proveedor)." })
    ] }),
    /* @__PURE__ */ l.jsxs("div", { className: "dx-pie", children: [
      /* @__PURE__ */ l.jsxs("div", { className: "panel", style: { margin: 0 }, children: [
        /* @__PURE__ */ l.jsxs("div", { className: "panel-head", children: [
          /* @__PURE__ */ l.jsx("h2", { children: "Vencimientos" }),
          /* @__PURE__ */ l.jsxs("div", { className: "dx-acciones", children: [
            [2, 3, 4].map((m) => /* @__PURE__ */ l.jsxs("button", { className: "btn small secondary", disabled: !V, onClick: () => Ue(m), children: [
              m,
              " plazos"
            ] }, m)),
            M && /* @__PURE__ */ l.jsx("button", { className: "btn small ghost", onClick: () => te(null), children: "Según forma de pago" })
          ] })
        ] }),
        M ? /* @__PURE__ */ l.jsxs(l.Fragment, { children: [
          M.map((m, L) => /* @__PURE__ */ l.jsxs("div", { className: "dx-fila", style: { marginBottom: 6 }, children: [
            /* @__PURE__ */ l.jsx("input", { type: "date", value: m.fecha, onChange: (R) => te(M.map(($, W) => W === L ? { ...$, fecha: R.target.value } : $)) }),
            /* @__PURE__ */ l.jsx("input", { className: "num", type: "number", step: "0.01", value: m.importe, onChange: (R) => te(M.map(($, W) => W === L ? { ...$, importe: Number(R.target.value) } : $)) })
          ] }, L)),
          V && sn !== V.total && /* @__PURE__ */ l.jsxs("p", { className: "dx-rojo", style: { margin: 0 }, children: [
            "Los plazos suman ",
            T(sn),
            "; la factura, ",
            T(V.total),
            "."
          ] })
        ] }) : /* @__PURE__ */ l.jsx("p", { className: "muted", style: { margin: 0 }, children: ((V == null ? void 0 : V.vencimientos) ?? []).map((m) => `${je(m.fecha)}: ${T(m.importe)}`).join(" · ") || "—" }),
        nt && /* @__PURE__ */ l.jsx("p", { className: "dx-rojo", style: { marginBottom: 0 }, children: nt })
      ] }),
      /* @__PURE__ */ l.jsxs("div", { className: "panel dx-totales", children: [
        ((V == null ? void 0 : V.desglose) ?? []).map((m, L) => {
          var R;
          return /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
            /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
              ((R = $t(m.codigoIva)) == null ? void 0 : R.nombre) ?? m.codigoIva,
              " ",
              m.autoliquidada ? `(${we(m.porcentajeIva)} %, autoliquidado)` : "",
              " · base ",
              we(m.base)
            ] }),
            /* @__PURE__ */ l.jsx("span", { children: T(m.cuota) })
          ] }, L);
        }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Base imponible" }),
          /* @__PURE__ */ l.jsx("span", { children: T(V == null ? void 0 : V.baseImponible) })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Impuestos" }),
          /* @__PURE__ */ l.jsx("span", { children: T(V == null ? void 0 : V.cuotaIva) })
        ] }),
        !!(V != null && V.recargoTotal) && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Recargo de equivalencia" }),
          /* @__PURE__ */ l.jsx("span", { children: T(V.recargoTotal) })
        ] }),
        !!(V != null && V.retencionIrpf) && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Retención IRPF" }),
          /* @__PURE__ */ l.jsxs("span", { children: [
            "−",
            T(V.retencionIrpf)
          ] })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ l.jsx("span", { children: ((V == null ? void 0 : V.total) ?? 0) < 0 ? "A favor (abono del proveedor)" : "Total a pagar" }),
          /* @__PURE__ */ l.jsx("span", { children: T(V == null ? void 0 : V.total) })
        ] }),
        V && (V.desglose ?? []).some((m) => m.cuotaDeducible !== m.cuota) && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "IVA deducible (antes de prorrata)" }),
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: T((V.desglose ?? []).reduce((m, L) => m + L.cuotaDeducible, 0)) })
        ] })
      ] })
    ] })
  ] });
}
function Tm(e) {
  const { api: t, anfitrion: n, navegar: r } = kt(), [a, i] = g.useState(null), [o, s] = g.useState(null), [c, d] = g.useState(""), [N, u] = g.useState(!1), h = () => {
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
          /* @__PURE__ */ l.jsx("span", { className: Ao(a.estado === "Anulado" ? "Anulada" : "Emitida"), children: a.estado }),
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
              lineas: (S = a.lineas) == null ? void 0 : S.map((O) => ({ ...O, base: -O.base }))
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
            /* @__PURE__ */ l.jsx("strong", { children: T(o.pendiente) })
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
        /* @__PURE__ */ l.jsx("tbody", { children: (a.lineas ?? []).map((S, O) => /* @__PURE__ */ l.jsxs("tr", { children: [
          /* @__PURE__ */ l.jsx("td", { children: S.descripcion ?? "" }),
          /* @__PURE__ */ l.jsx("td", { className: "mono muted", children: S.cuentaGasto ?? "regla" }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: T(S.base) }),
          /* @__PURE__ */ l.jsxs("td", { children: [
            S.codigoIva,
            " · ",
            we(S.porcentajeIva),
            " %",
            S.autoliquidada ? " · autoliquidada" : "",
            S.cuotaRecargo ? ` · recargo ${T(S.cuotaRecargo)}` : ""
          ] }),
          /* @__PURE__ */ l.jsx("td", { className: "num", children: T(S.cuota) }),
          /* @__PURE__ */ l.jsxs("td", { className: "num muted", children: [
            S.porcentajeDeducible !== 100 ? `${we(S.porcentajeDeducible)} % · ` : "",
            T(S.cuotaDeducible)
          ] })
        ] }, O)) })
      ] }),
      /* @__PURE__ */ l.jsxs("div", { className: "dx-totales-vista", children: [
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Base imponible" }),
          /* @__PURE__ */ l.jsx("span", { children: T(a.baseImponible) })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Impuestos" }),
          /* @__PURE__ */ l.jsx("span", { children: T(a.cuotaIva) })
        ] }),
        !!a.recargoTotal && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsx("span", { className: "muted", children: "Recargo de equivalencia" }),
          /* @__PURE__ */ l.jsx("span", { children: T(a.recargoTotal) })
        ] }),
        !!a.retencionIrpf && /* @__PURE__ */ l.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ l.jsxs("span", { className: "muted", children: [
            "Retención IRPF (",
            we(a.porcentajeIrpf),
            " %)"
          ] }),
          /* @__PURE__ */ l.jsxs("span", { children: [
            "−",
            T(a.retencionIrpf)
          ] })
        ] }),
        /* @__PURE__ */ l.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ l.jsx("span", { children: a.total < 0 ? "A favor (abono del proveedor)" : "Total a pagar" }),
          /* @__PURE__ */ l.jsx("span", { children: T(a.total) })
        ] })
      ] }),
      /* @__PURE__ */ l.jsxs("p", { className: "muted", style: { fontSize: 12.5 }, children: [
        "Vencimientos: ",
        (a.vencimientos ?? []).map((S) => `${je(S.fecha)} ${T(S.importe)}`).join(" · ")
      ] })
    ] }),
    N && /* @__PURE__ */ l.jsx(
      nn,
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
function zm(e) {
  const [t, n] = g.useState(e.inicial), r = g.useRef(0), [a, i] = g.useState(0), o = g.useMemo(() => sm(e.anfitrion), [e.anfitrion]), s = (u) => {
    n(u), i(++r.current), window.scrollTo({ top: 0 });
  }, c = { api: o, anfitrion: e.anfitrion, ruta: t, navegar: s }, d = `${t.tipo}-${t.pantalla}-${t.id ?? ""}-${a}`;
  let N;
  if (t.pantalla === "lista") N = /* @__PURE__ */ l.jsx(ym, { tipo: t.tipo });
  else if (t.pantalla === "vista" && t.id)
    N = t.tipo === "factura" ? /* @__PURE__ */ l.jsx(Em, { id: t.id }) : t.tipo === "presupuesto" ? /* @__PURE__ */ l.jsx(Im, { id: t.id }) : t.tipo === "pedido" ? /* @__PURE__ */ l.jsx(Pm, { id: t.id }) : t.tipo === "gasto" ? /* @__PURE__ */ l.jsx(Tm, { id: t.id }) : /* @__PURE__ */ l.jsx(Fm, { id: t.id });
  else if (t.tipo === "gasto")
    N = /* @__PURE__ */ l.jsx(
      _m,
      {
        id: t.id,
        semilla: t.semilla,
        alGuardar: (u) => s({ tipo: "gasto", pantalla: "vista", id: u }),
        alCancelar: () => s(t.id ? { tipo: "gasto", pantalla: "vista", id: t.id } : { tipo: "gasto", pantalla: "lista" })
      }
    );
  else if (t.tipo === "compra")
    N = /* @__PURE__ */ l.jsx(
      Cm,
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
      km,
      {
        tipo: u,
        id: t.id,
        semilla: t.semilla,
        alGuardar: (h) => s({ tipo: u, pantalla: "vista", id: h }),
        alCancelar: () => s(t.id ? { tipo: u, pantalla: "vista", id: t.id } : { tipo: u, pantalla: "lista" })
      }
    );
  }
  return /* @__PURE__ */ l.jsx(yd.Provider, { value: c, children: /* @__PURE__ */ l.jsx("div", { className: "dx-raiz", children: N }, d) });
}
const Dm = `
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
function Lm() {
  if (document.getElementById("dx-estilos")) return;
  const e = document.createElement("style");
  e.id = "dx-estilos", e.textContent = Dm, document.head.appendChild(e);
}
function Am(e, t, n) {
  Lm();
  const r = gd(e);
  return r.render(/* @__PURE__ */ l.jsx(zm, { anfitrion: t, inicial: n })), () => r.unmount();
}
export {
  Am as montar
};
