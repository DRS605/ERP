var Is = { exports: {} }, El = {}, zs = { exports: {} }, A = {};
/**
 * @license React
 * react.production.min.js
 *
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */
var wr = Symbol.for("react.element"), nd = Symbol.for("react.portal"), rd = Symbol.for("react.fragment"), ld = Symbol.for("react.strict_mode"), id = Symbol.for("react.profiler"), ad = Symbol.for("react.provider"), od = Symbol.for("react.context"), sd = Symbol.for("react.forward_ref"), ud = Symbol.for("react.suspense"), cd = Symbol.for("react.memo"), dd = Symbol.for("react.lazy"), jo = Symbol.iterator;
function fd(e) {
  return e === null || typeof e != "object" ? null : (e = jo && e[jo] || e["@@iterator"], typeof e == "function" ? e : null);
}
var Ts = { isMounted: function() {
  return !1;
}, enqueueForceUpdate: function() {
}, enqueueReplaceState: function() {
}, enqueueSetState: function() {
} }, Fs = Object.assign, Ls = {};
function zn(e, t, n) {
  this.props = e, this.context = t, this.refs = Ls, this.updater = n || Ts;
}
zn.prototype.isReactComponent = {};
zn.prototype.setState = function(e, t) {
  if (typeof e != "object" && typeof e != "function" && e != null) throw Error("setState(...): takes an object of state variables to update or a function which returns an object of state variables.");
  this.updater.enqueueSetState(this, e, t, "setState");
};
zn.prototype.forceUpdate = function(e) {
  this.updater.enqueueForceUpdate(this, e, "forceUpdate");
};
function Rs() {
}
Rs.prototype = zn.prototype;
function ha(e, t, n) {
  this.props = e, this.context = t, this.refs = Ls, this.updater = n || Ts;
}
var va = ha.prototype = new Rs();
va.constructor = ha;
Fs(va, zn.prototype);
va.isPureReactComponent = !0;
var wo = Array.isArray, Ds = Object.prototype.hasOwnProperty, ga = { current: null }, Ms = { key: !0, ref: !0, __self: !0, __source: !0 };
function Os(e, t, n) {
  var r, l = {}, i = null, o = null;
  if (t != null) for (r in t.ref !== void 0 && (o = t.ref), t.key !== void 0 && (i = "" + t.key), t) Ds.call(t, r) && !Ms.hasOwnProperty(r) && (l[r] = t[r]);
  var s = arguments.length - 2;
  if (s === 1) l.children = n;
  else if (1 < s) {
    for (var u = Array(s), d = 0; d < s; d++) u[d] = arguments[d + 2];
    l.children = u;
  }
  if (e && e.defaultProps) for (r in s = e.defaultProps, s) l[r] === void 0 && (l[r] = s[r]);
  return { $$typeof: wr, type: e, key: i, ref: o, props: l, _owner: ga.current };
}
function pd(e, t) {
  return { $$typeof: wr, type: e.type, key: t, ref: e.ref, props: e.props, _owner: e._owner };
}
function xa(e) {
  return typeof e == "object" && e !== null && e.$$typeof === wr;
}
function md(e) {
  var t = { "=": "=0", ":": "=2" };
  return "$" + e.replace(/[=:]/g, function(n) {
    return t[n];
  });
}
var ko = /\/+/g;
function ql(e, t) {
  return typeof e == "object" && e !== null && e.key != null ? md("" + e.key) : t.toString(36);
}
function Wr(e, t, n, r, l) {
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
        case wr:
        case nd:
          o = !0;
      }
  }
  if (o) return o = e, l = l(o), e = r === "" ? "." + ql(o, 0) : r, wo(l) ? (n = "", e != null && (n = e.replace(ko, "$&/") + "/"), Wr(l, t, n, "", function(d) {
    return d;
  })) : l != null && (xa(l) && (l = pd(l, n + (!l.key || o && o.key === l.key ? "" : ("" + l.key).replace(ko, "$&/") + "/") + e)), t.push(l)), 1;
  if (o = 0, r = r === "" ? "." : r + ":", wo(e)) for (var s = 0; s < e.length; s++) {
    i = e[s];
    var u = r + ql(i, s);
    o += Wr(i, t, n, u, l);
  }
  else if (u = fd(e), typeof u == "function") for (e = u.call(e), s = 0; !(i = e.next()).done; ) i = i.value, u = r + ql(i, s++), o += Wr(i, t, n, u, l);
  else if (i === "object") throw t = String(e), Error("Objects are not valid as a React child (found: " + (t === "[object Object]" ? "object with keys {" + Object.keys(e).join(", ") + "}" : t) + "). If you meant to render a collection of children, use an array instead.");
  return o;
}
function Pr(e, t, n) {
  if (e == null) return e;
  var r = [], l = 0;
  return Wr(e, r, "", "", function(i) {
    return t.call(n, i, l++);
  }), r;
}
function hd(e) {
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
var Ce = { current: null }, Qr = { transition: null }, vd = { ReactCurrentDispatcher: Ce, ReactCurrentBatchConfig: Qr, ReactCurrentOwner: ga };
function $s() {
  throw Error("act(...) is not supported in production builds of React.");
}
A.Children = { map: Pr, forEach: function(e, t, n) {
  Pr(e, function() {
    t.apply(this, arguments);
  }, n);
}, count: function(e) {
  var t = 0;
  return Pr(e, function() {
    t++;
  }), t;
}, toArray: function(e) {
  return Pr(e, function(t) {
    return t;
  }) || [];
}, only: function(e) {
  if (!xa(e)) throw Error("React.Children.only expected to receive a single React element child.");
  return e;
} };
A.Component = zn;
A.Fragment = rd;
A.Profiler = id;
A.PureComponent = ha;
A.StrictMode = ld;
A.Suspense = ud;
A.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED = vd;
A.act = $s;
A.cloneElement = function(e, t, n) {
  if (e == null) throw Error("React.cloneElement(...): The argument must be a React element, but you passed " + e + ".");
  var r = Fs({}, e.props), l = e.key, i = e.ref, o = e._owner;
  if (t != null) {
    if (t.ref !== void 0 && (i = t.ref, o = ga.current), t.key !== void 0 && (l = "" + t.key), e.type && e.type.defaultProps) var s = e.type.defaultProps;
    for (u in t) Ds.call(t, u) && !Ms.hasOwnProperty(u) && (r[u] = t[u] === void 0 && s !== void 0 ? s[u] : t[u]);
  }
  var u = arguments.length - 2;
  if (u === 1) r.children = n;
  else if (1 < u) {
    s = Array(u);
    for (var d = 0; d < u; d++) s[d] = arguments[d + 2];
    r.children = s;
  }
  return { $$typeof: wr, type: e.type, key: l, ref: i, props: r, _owner: o };
};
A.createContext = function(e) {
  return e = { $$typeof: od, _currentValue: e, _currentValue2: e, _threadCount: 0, Provider: null, Consumer: null, _defaultValue: null, _globalName: null }, e.Provider = { $$typeof: ad, _context: e }, e.Consumer = e;
};
A.createElement = Os;
A.createFactory = function(e) {
  var t = Os.bind(null, e);
  return t.type = e, t;
};
A.createRef = function() {
  return { current: null };
};
A.forwardRef = function(e) {
  return { $$typeof: sd, render: e };
};
A.isValidElement = xa;
A.lazy = function(e) {
  return { $$typeof: dd, _payload: { _status: -1, _result: e }, _init: hd };
};
A.memo = function(e, t) {
  return { $$typeof: cd, type: e, compare: t === void 0 ? null : t };
};
A.startTransition = function(e) {
  var t = Qr.transition;
  Qr.transition = {};
  try {
    e();
  } finally {
    Qr.transition = t;
  }
};
A.unstable_act = $s;
A.useCallback = function(e, t) {
  return Ce.current.useCallback(e, t);
};
A.useContext = function(e) {
  return Ce.current.useContext(e);
};
A.useDebugValue = function() {
};
A.useDeferredValue = function(e) {
  return Ce.current.useDeferredValue(e);
};
A.useEffect = function(e, t) {
  return Ce.current.useEffect(e, t);
};
A.useId = function() {
  return Ce.current.useId();
};
A.useImperativeHandle = function(e, t, n) {
  return Ce.current.useImperativeHandle(e, t, n);
};
A.useInsertionEffect = function(e, t) {
  return Ce.current.useInsertionEffect(e, t);
};
A.useLayoutEffect = function(e, t) {
  return Ce.current.useLayoutEffect(e, t);
};
A.useMemo = function(e, t) {
  return Ce.current.useMemo(e, t);
};
A.useReducer = function(e, t, n) {
  return Ce.current.useReducer(e, t, n);
};
A.useRef = function(e) {
  return Ce.current.useRef(e);
};
A.useState = function(e) {
  return Ce.current.useState(e);
};
A.useSyncExternalStore = function(e, t, n) {
  return Ce.current.useSyncExternalStore(e, t, n);
};
A.useTransition = function() {
  return Ce.current.useTransition();
};
A.version = "18.3.1";
zs.exports = A;
var y = zs.exports;
/**
 * @license React
 * react-jsx-runtime.production.min.js
 *
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */
var gd = y, xd = Symbol.for("react.element"), yd = Symbol.for("react.fragment"), jd = Object.prototype.hasOwnProperty, wd = gd.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED.ReactCurrentOwner, kd = { key: !0, ref: !0, __self: !0, __source: !0 };
function Us(e, t, n) {
  var r, l = {}, i = null, o = null;
  n !== void 0 && (i = "" + n), t.key !== void 0 && (i = "" + t.key), t.ref !== void 0 && (o = t.ref);
  for (r in t) jd.call(t, r) && !kd.hasOwnProperty(r) && (l[r] = t[r]);
  if (e && e.defaultProps) for (r in t = e.defaultProps, t) l[r] === void 0 && (l[r] = t[r]);
  return { $$typeof: xd, type: e, key: i, ref: o, props: l, _owner: wd.current };
}
El.Fragment = yd;
El.jsx = Us;
El.jsxs = Us;
Is.exports = El;
var a = Is.exports, As = { exports: {} }, Me = {}, Vs = { exports: {} }, Bs = {};
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
  function t(j, R) {
    var S = j.length;
    j.push(R);
    e: for (; 0 < S; ) {
      var U = S - 1 >>> 1, V = j[U];
      if (0 < l(V, R)) j[U] = R, j[S] = V, S = U;
      else break e;
    }
  }
  function n(j) {
    return j.length === 0 ? null : j[0];
  }
  function r(j) {
    if (j.length === 0) return null;
    var R = j[0], S = j.pop();
    if (S !== R) {
      j[0] = S;
      e: for (var U = 0, V = j.length, K = V >>> 1; U < K; ) {
        var we = 2 * (U + 1) - 1, ot = j[we], ce = we + 1, Ut = j[ce];
        if (0 > l(ot, S)) ce < V && 0 > l(Ut, ot) ? (j[U] = Ut, j[ce] = S, U = ce) : (j[U] = ot, j[we] = S, U = we);
        else if (ce < V && 0 > l(Ut, S)) j[U] = Ut, j[ce] = S, U = ce;
        else break e;
      }
    }
    return R;
  }
  function l(j, R) {
    var S = j.sortIndex - R.sortIndex;
    return S !== 0 ? S : j.id - R.id;
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
  var u = [], d = [], x = 1, c = null, m = 3, v = !1, g = !1, k = !1, F = typeof setTimeout == "function" ? setTimeout : null, f = typeof clearTimeout == "function" ? clearTimeout : null, p = typeof setImmediate < "u" ? setImmediate : null;
  typeof navigator < "u" && navigator.scheduling !== void 0 && navigator.scheduling.isInputPending !== void 0 && navigator.scheduling.isInputPending.bind(navigator.scheduling);
  function h(j) {
    for (var R = n(d); R !== null; ) {
      if (R.callback === null) r(d);
      else if (R.startTime <= j) r(d), R.sortIndex = R.expirationTime, t(u, R);
      else break;
      R = n(d);
    }
  }
  function w(j) {
    if (k = !1, h(j), !g) if (n(u) !== null) g = !0, W(I);
    else {
      var R = n(d);
      R !== null && ne(w, R.startTime - j);
    }
  }
  function I(j, R) {
    g = !1, k && (k = !1, f(N), N = -1), v = !0;
    var S = m;
    try {
      for (h(R), c = n(u); c !== null && (!(c.expirationTime > R) || j && !P()); ) {
        var U = c.callback;
        if (typeof U == "function") {
          c.callback = null, m = c.priorityLevel;
          var V = U(c.expirationTime <= R);
          R = e.unstable_now(), typeof V == "function" ? c.callback = V : c === n(u) && r(u), h(R);
        } else r(u);
        c = n(u);
      }
      if (c !== null) var K = !0;
      else {
        var we = n(d);
        we !== null && ne(w, we.startTime - R), K = !1;
      }
      return K;
    } finally {
      c = null, m = S, v = !1;
    }
  }
  var _ = !1, L = null, N = -1, z = 5, M = -1;
  function P() {
    return !(e.unstable_now() - M < z);
  }
  function Q() {
    if (L !== null) {
      var j = e.unstable_now();
      M = j;
      var R = !0;
      try {
        R = L(!0, j);
      } finally {
        R ? Ke() : (_ = !1, L = null);
      }
    } else _ = !1;
  }
  var Ke;
  if (typeof p == "function") Ke = function() {
    p(Q);
  };
  else if (typeof MessageChannel < "u") {
    var $e = new MessageChannel(), T = $e.port2;
    $e.port1.onmessage = Q, Ke = function() {
      T.postMessage(null);
    };
  } else Ke = function() {
    F(Q, 0);
  };
  function W(j) {
    L = j, _ || (_ = !0, Ke());
  }
  function ne(j, R) {
    N = F(function() {
      j(e.unstable_now());
    }, R);
  }
  e.unstable_IdlePriority = 5, e.unstable_ImmediatePriority = 1, e.unstable_LowPriority = 4, e.unstable_NormalPriority = 3, e.unstable_Profiling = null, e.unstable_UserBlockingPriority = 2, e.unstable_cancelCallback = function(j) {
    j.callback = null;
  }, e.unstable_continueExecution = function() {
    g || v || (g = !0, W(I));
  }, e.unstable_forceFrameRate = function(j) {
    0 > j || 125 < j ? console.error("forceFrameRate takes a positive int between 0 and 125, forcing frame rates higher than 125 fps is not supported") : z = 0 < j ? Math.floor(1e3 / j) : 5;
  }, e.unstable_getCurrentPriorityLevel = function() {
    return m;
  }, e.unstable_getFirstCallbackNode = function() {
    return n(u);
  }, e.unstable_next = function(j) {
    switch (m) {
      case 1:
      case 2:
      case 3:
        var R = 3;
        break;
      default:
        R = m;
    }
    var S = m;
    m = R;
    try {
      return j();
    } finally {
      m = S;
    }
  }, e.unstable_pauseExecution = function() {
  }, e.unstable_requestPaint = function() {
  }, e.unstable_runWithPriority = function(j, R) {
    switch (j) {
      case 1:
      case 2:
      case 3:
      case 4:
      case 5:
        break;
      default:
        j = 3;
    }
    var S = m;
    m = j;
    try {
      return R();
    } finally {
      m = S;
    }
  }, e.unstable_scheduleCallback = function(j, R, S) {
    var U = e.unstable_now();
    switch (typeof S == "object" && S !== null ? (S = S.delay, S = typeof S == "number" && 0 < S ? U + S : U) : S = U, j) {
      case 1:
        var V = -1;
        break;
      case 2:
        V = 250;
        break;
      case 5:
        V = 1073741823;
        break;
      case 4:
        V = 1e4;
        break;
      default:
        V = 5e3;
    }
    return V = S + V, j = { id: x++, callback: R, priorityLevel: j, startTime: S, expirationTime: V, sortIndex: -1 }, S > U ? (j.sortIndex = S, t(d, j), n(u) === null && j === n(d) && (k ? (f(N), N = -1) : k = !0, ne(w, S - U))) : (j.sortIndex = V, t(u, j), g || v || (g = !0, W(I))), j;
  }, e.unstable_shouldYield = P, e.unstable_wrapCallback = function(j) {
    var R = m;
    return function() {
      var S = m;
      m = R;
      try {
        return j.apply(this, arguments);
      } finally {
        m = S;
      }
    };
  };
})(Bs);
Vs.exports = Bs;
var Sd = Vs.exports;
/**
 * @license React
 * react-dom.production.min.js
 *
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */
var Nd = y, De = Sd;
function E(e) {
  for (var t = "https://reactjs.org/docs/error-decoder.html?invariant=" + e, n = 1; n < arguments.length; n++) t += "&args[]=" + encodeURIComponent(arguments[n]);
  return "Minified React error #" + e + "; visit " + t + " for the full message or use the non-minified dev environment for full errors and additional helpful warnings.";
}
var Hs = /* @__PURE__ */ new Set(), rr = {};
function Jt(e, t) {
  wn(e, t), wn(e + "Capture", t);
}
function wn(e, t) {
  for (rr[e] = t, e = 0; e < t.length; e++) Hs.add(t[e]);
}
var pt = !(typeof window > "u" || typeof window.document > "u" || typeof window.document.createElement > "u"), ki = Object.prototype.hasOwnProperty, Cd = /^[:A-Z_a-z\u00C0-\u00D6\u00D8-\u00F6\u00F8-\u02FF\u0370-\u037D\u037F-\u1FFF\u200C-\u200D\u2070-\u218F\u2C00-\u2FEF\u3001-\uD7FF\uF900-\uFDCF\uFDF0-\uFFFD][:A-Z_a-z\u00C0-\u00D6\u00D8-\u00F6\u00F8-\u02FF\u0370-\u037D\u037F-\u1FFF\u200C-\u200D\u2070-\u218F\u2C00-\u2FEF\u3001-\uD7FF\uF900-\uFDCF\uFDF0-\uFFFD\-.0-9\u00B7\u0300-\u036F\u203F-\u2040]*$/, So = {}, No = {};
function Ed(e) {
  return ki.call(No, e) ? !0 : ki.call(So, e) ? !1 : Cd.test(e) ? No[e] = !0 : (So[e] = !0, !1);
}
function Pd(e, t, n, r) {
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
function _d(e, t, n, r) {
  if (t === null || typeof t > "u" || Pd(e, t, n, r)) return !0;
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
function Ee(e, t, n, r, l, i, o) {
  this.acceptsBooleans = t === 2 || t === 3 || t === 4, this.attributeName = r, this.attributeNamespace = l, this.mustUseProperty = n, this.propertyName = e, this.type = t, this.sanitizeURL = i, this.removeEmptyString = o;
}
var he = {};
"children dangerouslySetInnerHTML defaultValue defaultChecked innerHTML suppressContentEditableWarning suppressHydrationWarning style".split(" ").forEach(function(e) {
  he[e] = new Ee(e, 0, !1, e, null, !1, !1);
});
[["acceptCharset", "accept-charset"], ["className", "class"], ["htmlFor", "for"], ["httpEquiv", "http-equiv"]].forEach(function(e) {
  var t = e[0];
  he[t] = new Ee(t, 1, !1, e[1], null, !1, !1);
});
["contentEditable", "draggable", "spellCheck", "value"].forEach(function(e) {
  he[e] = new Ee(e, 2, !1, e.toLowerCase(), null, !1, !1);
});
["autoReverse", "externalResourcesRequired", "focusable", "preserveAlpha"].forEach(function(e) {
  he[e] = new Ee(e, 2, !1, e, null, !1, !1);
});
"allowFullScreen async autoFocus autoPlay controls default defer disabled disablePictureInPicture disableRemotePlayback formNoValidate hidden loop noModule noValidate open playsInline readOnly required reversed scoped seamless itemScope".split(" ").forEach(function(e) {
  he[e] = new Ee(e, 3, !1, e.toLowerCase(), null, !1, !1);
});
["checked", "multiple", "muted", "selected"].forEach(function(e) {
  he[e] = new Ee(e, 3, !0, e, null, !1, !1);
});
["capture", "download"].forEach(function(e) {
  he[e] = new Ee(e, 4, !1, e, null, !1, !1);
});
["cols", "rows", "size", "span"].forEach(function(e) {
  he[e] = new Ee(e, 6, !1, e, null, !1, !1);
});
["rowSpan", "start"].forEach(function(e) {
  he[e] = new Ee(e, 5, !1, e.toLowerCase(), null, !1, !1);
});
var ya = /[\-:]([a-z])/g;
function ja(e) {
  return e[1].toUpperCase();
}
"accent-height alignment-baseline arabic-form baseline-shift cap-height clip-path clip-rule color-interpolation color-interpolation-filters color-profile color-rendering dominant-baseline enable-background fill-opacity fill-rule flood-color flood-opacity font-family font-size font-size-adjust font-stretch font-style font-variant font-weight glyph-name glyph-orientation-horizontal glyph-orientation-vertical horiz-adv-x horiz-origin-x image-rendering letter-spacing lighting-color marker-end marker-mid marker-start overline-position overline-thickness paint-order panose-1 pointer-events rendering-intent shape-rendering stop-color stop-opacity strikethrough-position strikethrough-thickness stroke-dasharray stroke-dashoffset stroke-linecap stroke-linejoin stroke-miterlimit stroke-opacity stroke-width text-anchor text-decoration text-rendering underline-position underline-thickness unicode-bidi unicode-range units-per-em v-alphabetic v-hanging v-ideographic v-mathematical vector-effect vert-adv-y vert-origin-x vert-origin-y word-spacing writing-mode xmlns:xlink x-height".split(" ").forEach(function(e) {
  var t = e.replace(
    ya,
    ja
  );
  he[t] = new Ee(t, 1, !1, e, null, !1, !1);
});
"xlink:actuate xlink:arcrole xlink:role xlink:show xlink:title xlink:type".split(" ").forEach(function(e) {
  var t = e.replace(ya, ja);
  he[t] = new Ee(t, 1, !1, e, "http://www.w3.org/1999/xlink", !1, !1);
});
["xml:base", "xml:lang", "xml:space"].forEach(function(e) {
  var t = e.replace(ya, ja);
  he[t] = new Ee(t, 1, !1, e, "http://www.w3.org/XML/1998/namespace", !1, !1);
});
["tabIndex", "crossOrigin"].forEach(function(e) {
  he[e] = new Ee(e, 1, !1, e.toLowerCase(), null, !1, !1);
});
he.xlinkHref = new Ee("xlinkHref", 1, !1, "xlink:href", "http://www.w3.org/1999/xlink", !0, !1);
["src", "href", "action", "formAction"].forEach(function(e) {
  he[e] = new Ee(e, 1, !1, e.toLowerCase(), null, !0, !0);
});
function wa(e, t, n, r) {
  var l = he.hasOwnProperty(t) ? he[t] : null;
  (l !== null ? l.type !== 0 : r || !(2 < t.length) || t[0] !== "o" && t[0] !== "O" || t[1] !== "n" && t[1] !== "N") && (_d(t, n, l, r) && (n = null), r || l === null ? Ed(t) && (n === null ? e.removeAttribute(t) : e.setAttribute(t, "" + n)) : l.mustUseProperty ? e[l.propertyName] = n === null ? l.type === 3 ? !1 : "" : n : (t = l.attributeName, r = l.attributeNamespace, n === null ? e.removeAttribute(t) : (l = l.type, n = l === 3 || l === 4 && n === !0 ? "" : "" + n, r ? e.setAttributeNS(r, t, n) : e.setAttribute(t, n))));
}
var gt = Nd.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED, _r = Symbol.for("react.element"), nn = Symbol.for("react.portal"), rn = Symbol.for("react.fragment"), ka = Symbol.for("react.strict_mode"), Si = Symbol.for("react.profiler"), Ws = Symbol.for("react.provider"), Qs = Symbol.for("react.context"), Sa = Symbol.for("react.forward_ref"), Ni = Symbol.for("react.suspense"), Ci = Symbol.for("react.suspense_list"), Na = Symbol.for("react.memo"), yt = Symbol.for("react.lazy"), Ks = Symbol.for("react.offscreen"), Co = Symbol.iterator;
function Dn(e) {
  return e === null || typeof e != "object" ? null : (e = Co && e[Co] || e["@@iterator"], typeof e == "function" ? e : null);
}
var te = Object.assign, Zl;
function Hn(e) {
  if (Zl === void 0) try {
    throw Error();
  } catch (n) {
    var t = n.stack.trim().match(/\n( *(at )?)/);
    Zl = t && t[1] || "";
  }
  return `
` + Zl + e;
}
var Jl = !1;
function bl(e, t) {
  if (!e || Jl) return "";
  Jl = !0;
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
    Jl = !1, Error.prepareStackTrace = n;
  }
  return (e = e ? e.displayName || e.name : "") ? Hn(e) : "";
}
function Id(e) {
  switch (e.tag) {
    case 5:
      return Hn(e.type);
    case 16:
      return Hn("Lazy");
    case 13:
      return Hn("Suspense");
    case 19:
      return Hn("SuspenseList");
    case 0:
    case 2:
    case 15:
      return e = bl(e.type, !1), e;
    case 11:
      return e = bl(e.type.render, !1), e;
    case 1:
      return e = bl(e.type, !0), e;
    default:
      return "";
  }
}
function Ei(e) {
  if (e == null) return null;
  if (typeof e == "function") return e.displayName || e.name || null;
  if (typeof e == "string") return e;
  switch (e) {
    case rn:
      return "Fragment";
    case nn:
      return "Portal";
    case Si:
      return "Profiler";
    case ka:
      return "StrictMode";
    case Ni:
      return "Suspense";
    case Ci:
      return "SuspenseList";
  }
  if (typeof e == "object") switch (e.$$typeof) {
    case Qs:
      return (e.displayName || "Context") + ".Consumer";
    case Ws:
      return (e._context.displayName || "Context") + ".Provider";
    case Sa:
      var t = e.render;
      return e = e.displayName, e || (e = t.displayName || t.name || "", e = e !== "" ? "ForwardRef(" + e + ")" : "ForwardRef"), e;
    case Na:
      return t = e.displayName || null, t !== null ? t : Ei(e.type) || "Memo";
    case yt:
      t = e._payload, e = e._init;
      try {
        return Ei(e(t));
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
      return Ei(t);
    case 8:
      return t === ka ? "StrictMode" : "Mode";
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
function Lt(e) {
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
function Gs(e) {
  var t = e.type;
  return (e = e.nodeName) && e.toLowerCase() === "input" && (t === "checkbox" || t === "radio");
}
function Td(e) {
  var t = Gs(e) ? "checked" : "value", n = Object.getOwnPropertyDescriptor(e.constructor.prototype, t), r = "" + e[t];
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
function Ir(e) {
  e._valueTracker || (e._valueTracker = Td(e));
}
function Ys(e) {
  if (!e) return !1;
  var t = e._valueTracker;
  if (!t) return !0;
  var n = t.getValue(), r = "";
  return e && (r = Gs(e) ? e.checked ? "true" : "false" : e.value), e = r, e !== n ? (t.setValue(e), !0) : !1;
}
function nl(e) {
  if (e = e || (typeof document < "u" ? document : void 0), typeof e > "u") return null;
  try {
    return e.activeElement || e.body;
  } catch {
    return e.body;
  }
}
function Pi(e, t) {
  var n = t.checked;
  return te({}, t, { defaultChecked: void 0, defaultValue: void 0, value: void 0, checked: n ?? e._wrapperState.initialChecked });
}
function Eo(e, t) {
  var n = t.defaultValue == null ? "" : t.defaultValue, r = t.checked != null ? t.checked : t.defaultChecked;
  n = Lt(t.value != null ? t.value : n), e._wrapperState = { initialChecked: r, initialValue: n, controlled: t.type === "checkbox" || t.type === "radio" ? t.checked != null : t.value != null };
}
function Xs(e, t) {
  t = t.checked, t != null && wa(e, "checked", t, !1);
}
function _i(e, t) {
  Xs(e, t);
  var n = Lt(t.value), r = t.type;
  if (n != null) r === "number" ? (n === 0 && e.value === "" || e.value != n) && (e.value = "" + n) : e.value !== "" + n && (e.value = "" + n);
  else if (r === "submit" || r === "reset") {
    e.removeAttribute("value");
    return;
  }
  t.hasOwnProperty("value") ? Ii(e, t.type, n) : t.hasOwnProperty("defaultValue") && Ii(e, t.type, Lt(t.defaultValue)), t.checked == null && t.defaultChecked != null && (e.defaultChecked = !!t.defaultChecked);
}
function Po(e, t, n) {
  if (t.hasOwnProperty("value") || t.hasOwnProperty("defaultValue")) {
    var r = t.type;
    if (!(r !== "submit" && r !== "reset" || t.value !== void 0 && t.value !== null)) return;
    t = "" + e._wrapperState.initialValue, n || t === e.value || (e.value = t), e.defaultValue = t;
  }
  n = e.name, n !== "" && (e.name = ""), e.defaultChecked = !!e._wrapperState.initialChecked, n !== "" && (e.name = n);
}
function Ii(e, t, n) {
  (t !== "number" || nl(e.ownerDocument) !== e) && (n == null ? e.defaultValue = "" + e._wrapperState.initialValue : e.defaultValue !== "" + n && (e.defaultValue = "" + n));
}
var Wn = Array.isArray;
function hn(e, t, n, r) {
  if (e = e.options, t) {
    t = {};
    for (var l = 0; l < n.length; l++) t["$" + n[l]] = !0;
    for (n = 0; n < e.length; n++) l = t.hasOwnProperty("$" + e[n].value), e[n].selected !== l && (e[n].selected = l), l && r && (e[n].defaultSelected = !0);
  } else {
    for (n = "" + Lt(n), t = null, l = 0; l < e.length; l++) {
      if (e[l].value === n) {
        e[l].selected = !0, r && (e[l].defaultSelected = !0);
        return;
      }
      t !== null || e[l].disabled || (t = e[l]);
    }
    t !== null && (t.selected = !0);
  }
}
function zi(e, t) {
  if (t.dangerouslySetInnerHTML != null) throw Error(E(91));
  return te({}, t, { value: void 0, defaultValue: void 0, children: "" + e._wrapperState.initialValue });
}
function _o(e, t) {
  var n = t.value;
  if (n == null) {
    if (n = t.children, t = t.defaultValue, n != null) {
      if (t != null) throw Error(E(92));
      if (Wn(n)) {
        if (1 < n.length) throw Error(E(93));
        n = n[0];
      }
      t = n;
    }
    t == null && (t = ""), n = t;
  }
  e._wrapperState = { initialValue: Lt(n) };
}
function qs(e, t) {
  var n = Lt(t.value), r = Lt(t.defaultValue);
  n != null && (n = "" + n, n !== e.value && (e.value = n), t.defaultValue == null && e.defaultValue !== n && (e.defaultValue = n)), r != null && (e.defaultValue = "" + r);
}
function Io(e) {
  var t = e.textContent;
  t === e._wrapperState.initialValue && t !== "" && t !== null && (e.value = t);
}
function Zs(e) {
  switch (e) {
    case "svg":
      return "http://www.w3.org/2000/svg";
    case "math":
      return "http://www.w3.org/1998/Math/MathML";
    default:
      return "http://www.w3.org/1999/xhtml";
  }
}
function Ti(e, t) {
  return e == null || e === "http://www.w3.org/1999/xhtml" ? Zs(t) : e === "http://www.w3.org/2000/svg" && t === "foreignObject" ? "http://www.w3.org/1999/xhtml" : e;
}
var zr, Js = function(e) {
  return typeof MSApp < "u" && MSApp.execUnsafeLocalFunction ? function(t, n, r, l) {
    MSApp.execUnsafeLocalFunction(function() {
      return e(t, n, r, l);
    });
  } : e;
}(function(e, t) {
  if (e.namespaceURI !== "http://www.w3.org/2000/svg" || "innerHTML" in e) e.innerHTML = t;
  else {
    for (zr = zr || document.createElement("div"), zr.innerHTML = "<svg>" + t.valueOf().toString() + "</svg>", t = zr.firstChild; e.firstChild; ) e.removeChild(e.firstChild);
    for (; t.firstChild; ) e.appendChild(t.firstChild);
  }
});
function lr(e, t) {
  if (t) {
    var n = e.firstChild;
    if (n && n === e.lastChild && n.nodeType === 3) {
      n.nodeValue = t;
      return;
    }
  }
  e.textContent = t;
}
var Gn = {
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
}, Fd = ["Webkit", "ms", "Moz", "O"];
Object.keys(Gn).forEach(function(e) {
  Fd.forEach(function(t) {
    t = t + e.charAt(0).toUpperCase() + e.substring(1), Gn[t] = Gn[e];
  });
});
function bs(e, t, n) {
  return t == null || typeof t == "boolean" || t === "" ? "" : n || typeof t != "number" || t === 0 || Gn.hasOwnProperty(e) && Gn[e] ? ("" + t).trim() : t + "px";
}
function eu(e, t) {
  e = e.style;
  for (var n in t) if (t.hasOwnProperty(n)) {
    var r = n.indexOf("--") === 0, l = bs(n, t[n], r);
    n === "float" && (n = "cssFloat"), r ? e.setProperty(n, l) : e[n] = l;
  }
}
var Ld = te({ menuitem: !0 }, { area: !0, base: !0, br: !0, col: !0, embed: !0, hr: !0, img: !0, input: !0, keygen: !0, link: !0, meta: !0, param: !0, source: !0, track: !0, wbr: !0 });
function Fi(e, t) {
  if (t) {
    if (Ld[e] && (t.children != null || t.dangerouslySetInnerHTML != null)) throw Error(E(137, e));
    if (t.dangerouslySetInnerHTML != null) {
      if (t.children != null) throw Error(E(60));
      if (typeof t.dangerouslySetInnerHTML != "object" || !("__html" in t.dangerouslySetInnerHTML)) throw Error(E(61));
    }
    if (t.style != null && typeof t.style != "object") throw Error(E(62));
  }
}
function Li(e, t) {
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
var Ri = null;
function Ca(e) {
  return e = e.target || e.srcElement || window, e.correspondingUseElement && (e = e.correspondingUseElement), e.nodeType === 3 ? e.parentNode : e;
}
var Di = null, vn = null, gn = null;
function zo(e) {
  if (e = Nr(e)) {
    if (typeof Di != "function") throw Error(E(280));
    var t = e.stateNode;
    t && (t = Tl(t), Di(e.stateNode, e.type, t));
  }
}
function tu(e) {
  vn ? gn ? gn.push(e) : gn = [e] : vn = e;
}
function nu() {
  if (vn) {
    var e = vn, t = gn;
    if (gn = vn = null, zo(e), t) for (e = 0; e < t.length; e++) zo(t[e]);
  }
}
function ru(e, t) {
  return e(t);
}
function lu() {
}
var ei = !1;
function iu(e, t, n) {
  if (ei) return e(t, n);
  ei = !0;
  try {
    return ru(e, t, n);
  } finally {
    ei = !1, (vn !== null || gn !== null) && (lu(), nu());
  }
}
function ir(e, t) {
  var n = e.stateNode;
  if (n === null) return null;
  var r = Tl(n);
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
  if (n && typeof n != "function") throw Error(E(231, t, typeof n));
  return n;
}
var Mi = !1;
if (pt) try {
  var Mn = {};
  Object.defineProperty(Mn, "passive", { get: function() {
    Mi = !0;
  } }), window.addEventListener("test", Mn, Mn), window.removeEventListener("test", Mn, Mn);
} catch {
  Mi = !1;
}
function Rd(e, t, n, r, l, i, o, s, u) {
  var d = Array.prototype.slice.call(arguments, 3);
  try {
    t.apply(n, d);
  } catch (x) {
    this.onError(x);
  }
}
var Yn = !1, rl = null, ll = !1, Oi = null, Dd = { onError: function(e) {
  Yn = !0, rl = e;
} };
function Md(e, t, n, r, l, i, o, s, u) {
  Yn = !1, rl = null, Rd.apply(Dd, arguments);
}
function Od(e, t, n, r, l, i, o, s, u) {
  if (Md.apply(this, arguments), Yn) {
    if (Yn) {
      var d = rl;
      Yn = !1, rl = null;
    } else throw Error(E(198));
    ll || (ll = !0, Oi = d);
  }
}
function bt(e) {
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
function au(e) {
  if (e.tag === 13) {
    var t = e.memoizedState;
    if (t === null && (e = e.alternate, e !== null && (t = e.memoizedState)), t !== null) return t.dehydrated;
  }
  return null;
}
function To(e) {
  if (bt(e) !== e) throw Error(E(188));
}
function $d(e) {
  var t = e.alternate;
  if (!t) {
    if (t = bt(e), t === null) throw Error(E(188));
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
        if (i === n) return To(l), e;
        if (i === r) return To(l), t;
        i = i.sibling;
      }
      throw Error(E(188));
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
        if (!o) throw Error(E(189));
      }
    }
    if (n.alternate !== r) throw Error(E(190));
  }
  if (n.tag !== 3) throw Error(E(188));
  return n.stateNode.current === n ? e : t;
}
function ou(e) {
  return e = $d(e), e !== null ? su(e) : null;
}
function su(e) {
  if (e.tag === 5 || e.tag === 6) return e;
  for (e = e.child; e !== null; ) {
    var t = su(e);
    if (t !== null) return t;
    e = e.sibling;
  }
  return null;
}
var uu = De.unstable_scheduleCallback, Fo = De.unstable_cancelCallback, Ud = De.unstable_shouldYield, Ad = De.unstable_requestPaint, ie = De.unstable_now, Vd = De.unstable_getCurrentPriorityLevel, Ea = De.unstable_ImmediatePriority, cu = De.unstable_UserBlockingPriority, il = De.unstable_NormalPriority, Bd = De.unstable_LowPriority, du = De.unstable_IdlePriority, Pl = null, lt = null;
function Hd(e) {
  if (lt && typeof lt.onCommitFiberRoot == "function") try {
    lt.onCommitFiberRoot(Pl, e, void 0, (e.current.flags & 128) === 128);
  } catch {
  }
}
var Je = Math.clz32 ? Math.clz32 : Kd, Wd = Math.log, Qd = Math.LN2;
function Kd(e) {
  return e >>>= 0, e === 0 ? 32 : 31 - (Wd(e) / Qd | 0) | 0;
}
var Tr = 64, Fr = 4194304;
function Qn(e) {
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
function al(e, t) {
  var n = e.pendingLanes;
  if (n === 0) return 0;
  var r = 0, l = e.suspendedLanes, i = e.pingedLanes, o = n & 268435455;
  if (o !== 0) {
    var s = o & ~l;
    s !== 0 ? r = Qn(s) : (i &= o, i !== 0 && (r = Qn(i)));
  } else o = n & ~l, o !== 0 ? r = Qn(o) : i !== 0 && (r = Qn(i));
  if (r === 0) return 0;
  if (t !== 0 && t !== r && !(t & l) && (l = r & -r, i = t & -t, l >= i || l === 16 && (i & 4194240) !== 0)) return t;
  if (r & 4 && (r |= n & 16), t = e.entangledLanes, t !== 0) for (e = e.entanglements, t &= r; 0 < t; ) n = 31 - Je(t), l = 1 << n, r |= e[n], t &= ~l;
  return r;
}
function Gd(e, t) {
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
    var o = 31 - Je(i), s = 1 << o, u = l[o];
    u === -1 ? (!(s & n) || s & r) && (l[o] = Gd(s, t)) : u <= t && (e.expiredLanes |= s), i &= ~s;
  }
}
function $i(e) {
  return e = e.pendingLanes & -1073741825, e !== 0 ? e : e & 1073741824 ? 1073741824 : 0;
}
function fu() {
  var e = Tr;
  return Tr <<= 1, !(Tr & 4194240) && (Tr = 64), e;
}
function ti(e) {
  for (var t = [], n = 0; 31 > n; n++) t.push(e);
  return t;
}
function kr(e, t, n) {
  e.pendingLanes |= t, t !== 536870912 && (e.suspendedLanes = 0, e.pingedLanes = 0), e = e.eventTimes, t = 31 - Je(t), e[t] = n;
}
function Xd(e, t) {
  var n = e.pendingLanes & ~t;
  e.pendingLanes = t, e.suspendedLanes = 0, e.pingedLanes = 0, e.expiredLanes &= t, e.mutableReadLanes &= t, e.entangledLanes &= t, t = e.entanglements;
  var r = e.eventTimes;
  for (e = e.expirationTimes; 0 < n; ) {
    var l = 31 - Je(n), i = 1 << l;
    t[l] = 0, r[l] = -1, e[l] = -1, n &= ~i;
  }
}
function Pa(e, t) {
  var n = e.entangledLanes |= t;
  for (e = e.entanglements; n; ) {
    var r = 31 - Je(n), l = 1 << r;
    l & t | e[r] & t && (e[r] |= t), n &= ~l;
  }
}
var H = 0;
function pu(e) {
  return e &= -e, 1 < e ? 4 < e ? e & 268435455 ? 16 : 536870912 : 4 : 1;
}
var mu, _a, hu, vu, gu, Ui = !1, Lr = [], Ct = null, Et = null, Pt = null, ar = /* @__PURE__ */ new Map(), or = /* @__PURE__ */ new Map(), wt = [], qd = "mousedown mouseup touchcancel touchend touchstart auxclick dblclick pointercancel pointerdown pointerup dragend dragstart drop compositionend compositionstart keydown keypress keyup input textInput copy cut paste click change contextmenu reset submit".split(" ");
function Lo(e, t) {
  switch (e) {
    case "focusin":
    case "focusout":
      Ct = null;
      break;
    case "dragenter":
    case "dragleave":
      Et = null;
      break;
    case "mouseover":
    case "mouseout":
      Pt = null;
      break;
    case "pointerover":
    case "pointerout":
      ar.delete(t.pointerId);
      break;
    case "gotpointercapture":
    case "lostpointercapture":
      or.delete(t.pointerId);
  }
}
function On(e, t, n, r, l, i) {
  return e === null || e.nativeEvent !== i ? (e = { blockedOn: t, domEventName: n, eventSystemFlags: r, nativeEvent: i, targetContainers: [l] }, t !== null && (t = Nr(t), t !== null && _a(t)), e) : (e.eventSystemFlags |= r, t = e.targetContainers, l !== null && t.indexOf(l) === -1 && t.push(l), e);
}
function Zd(e, t, n, r, l) {
  switch (t) {
    case "focusin":
      return Ct = On(Ct, e, t, n, r, l), !0;
    case "dragenter":
      return Et = On(Et, e, t, n, r, l), !0;
    case "mouseover":
      return Pt = On(Pt, e, t, n, r, l), !0;
    case "pointerover":
      var i = l.pointerId;
      return ar.set(i, On(ar.get(i) || null, e, t, n, r, l)), !0;
    case "gotpointercapture":
      return i = l.pointerId, or.set(i, On(or.get(i) || null, e, t, n, r, l)), !0;
  }
  return !1;
}
function xu(e) {
  var t = Bt(e.target);
  if (t !== null) {
    var n = bt(t);
    if (n !== null) {
      if (t = n.tag, t === 13) {
        if (t = au(n), t !== null) {
          e.blockedOn = t, gu(e.priority, function() {
            hu(n);
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
function Kr(e) {
  if (e.blockedOn !== null) return !1;
  for (var t = e.targetContainers; 0 < t.length; ) {
    var n = Ai(e.domEventName, e.eventSystemFlags, t[0], e.nativeEvent);
    if (n === null) {
      n = e.nativeEvent;
      var r = new n.constructor(n.type, n);
      Ri = r, n.target.dispatchEvent(r), Ri = null;
    } else return t = Nr(n), t !== null && _a(t), e.blockedOn = n, !1;
    t.shift();
  }
  return !0;
}
function Ro(e, t, n) {
  Kr(e) && n.delete(t);
}
function Jd() {
  Ui = !1, Ct !== null && Kr(Ct) && (Ct = null), Et !== null && Kr(Et) && (Et = null), Pt !== null && Kr(Pt) && (Pt = null), ar.forEach(Ro), or.forEach(Ro);
}
function $n(e, t) {
  e.blockedOn === t && (e.blockedOn = null, Ui || (Ui = !0, De.unstable_scheduleCallback(De.unstable_NormalPriority, Jd)));
}
function sr(e) {
  function t(l) {
    return $n(l, e);
  }
  if (0 < Lr.length) {
    $n(Lr[0], e);
    for (var n = 1; n < Lr.length; n++) {
      var r = Lr[n];
      r.blockedOn === e && (r.blockedOn = null);
    }
  }
  for (Ct !== null && $n(Ct, e), Et !== null && $n(Et, e), Pt !== null && $n(Pt, e), ar.forEach(t), or.forEach(t), n = 0; n < wt.length; n++) r = wt[n], r.blockedOn === e && (r.blockedOn = null);
  for (; 0 < wt.length && (n = wt[0], n.blockedOn === null); ) xu(n), n.blockedOn === null && wt.shift();
}
var xn = gt.ReactCurrentBatchConfig, ol = !0;
function bd(e, t, n, r) {
  var l = H, i = xn.transition;
  xn.transition = null;
  try {
    H = 1, Ia(e, t, n, r);
  } finally {
    H = l, xn.transition = i;
  }
}
function ef(e, t, n, r) {
  var l = H, i = xn.transition;
  xn.transition = null;
  try {
    H = 4, Ia(e, t, n, r);
  } finally {
    H = l, xn.transition = i;
  }
}
function Ia(e, t, n, r) {
  if (ol) {
    var l = Ai(e, t, n, r);
    if (l === null) di(e, t, r, sl, n), Lo(e, r);
    else if (Zd(l, e, t, n, r)) r.stopPropagation();
    else if (Lo(e, r), t & 4 && -1 < qd.indexOf(e)) {
      for (; l !== null; ) {
        var i = Nr(l);
        if (i !== null && mu(i), i = Ai(e, t, n, r), i === null && di(e, t, r, sl, n), i === l) break;
        l = i;
      }
      l !== null && r.stopPropagation();
    } else di(e, t, r, null, n);
  }
}
var sl = null;
function Ai(e, t, n, r) {
  if (sl = null, e = Ca(r), e = Bt(e), e !== null) if (t = bt(e), t === null) e = null;
  else if (n = t.tag, n === 13) {
    if (e = au(t), e !== null) return e;
    e = null;
  } else if (n === 3) {
    if (t.stateNode.current.memoizedState.isDehydrated) return t.tag === 3 ? t.stateNode.containerInfo : null;
    e = null;
  } else t !== e && (e = null);
  return sl = e, null;
}
function yu(e) {
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
      switch (Vd()) {
        case Ea:
          return 1;
        case cu:
          return 4;
        case il:
        case Bd:
          return 16;
        case du:
          return 536870912;
        default:
          return 16;
      }
    default:
      return 16;
  }
}
var St = null, za = null, Gr = null;
function ju() {
  if (Gr) return Gr;
  var e, t = za, n = t.length, r, l = "value" in St ? St.value : St.textContent, i = l.length;
  for (e = 0; e < n && t[e] === l[e]; e++) ;
  var o = n - e;
  for (r = 1; r <= o && t[n - r] === l[i - r]; r++) ;
  return Gr = l.slice(e, 1 < r ? 1 - r : void 0);
}
function Yr(e) {
  var t = e.keyCode;
  return "charCode" in e ? (e = e.charCode, e === 0 && t === 13 && (e = 13)) : e = t, e === 10 && (e = 13), 32 <= e || e === 13 ? e : 0;
}
function Rr() {
  return !0;
}
function Do() {
  return !1;
}
function Oe(e) {
  function t(n, r, l, i, o) {
    this._reactName = n, this._targetInst = l, this.type = r, this.nativeEvent = i, this.target = o, this.currentTarget = null;
    for (var s in e) e.hasOwnProperty(s) && (n = e[s], this[s] = n ? n(i) : i[s]);
    return this.isDefaultPrevented = (i.defaultPrevented != null ? i.defaultPrevented : i.returnValue === !1) ? Rr : Do, this.isPropagationStopped = Do, this;
  }
  return te(t.prototype, { preventDefault: function() {
    this.defaultPrevented = !0;
    var n = this.nativeEvent;
    n && (n.preventDefault ? n.preventDefault() : typeof n.returnValue != "unknown" && (n.returnValue = !1), this.isDefaultPrevented = Rr);
  }, stopPropagation: function() {
    var n = this.nativeEvent;
    n && (n.stopPropagation ? n.stopPropagation() : typeof n.cancelBubble != "unknown" && (n.cancelBubble = !0), this.isPropagationStopped = Rr);
  }, persist: function() {
  }, isPersistent: Rr }), t;
}
var Tn = { eventPhase: 0, bubbles: 0, cancelable: 0, timeStamp: function(e) {
  return e.timeStamp || Date.now();
}, defaultPrevented: 0, isTrusted: 0 }, Ta = Oe(Tn), Sr = te({}, Tn, { view: 0, detail: 0 }), tf = Oe(Sr), ni, ri, Un, _l = te({}, Sr, { screenX: 0, screenY: 0, clientX: 0, clientY: 0, pageX: 0, pageY: 0, ctrlKey: 0, shiftKey: 0, altKey: 0, metaKey: 0, getModifierState: Fa, button: 0, buttons: 0, relatedTarget: function(e) {
  return e.relatedTarget === void 0 ? e.fromElement === e.srcElement ? e.toElement : e.fromElement : e.relatedTarget;
}, movementX: function(e) {
  return "movementX" in e ? e.movementX : (e !== Un && (Un && e.type === "mousemove" ? (ni = e.screenX - Un.screenX, ri = e.screenY - Un.screenY) : ri = ni = 0, Un = e), ni);
}, movementY: function(e) {
  return "movementY" in e ? e.movementY : ri;
} }), Mo = Oe(_l), nf = te({}, _l, { dataTransfer: 0 }), rf = Oe(nf), lf = te({}, Sr, { relatedTarget: 0 }), li = Oe(lf), af = te({}, Tn, { animationName: 0, elapsedTime: 0, pseudoElement: 0 }), of = Oe(af), sf = te({}, Tn, { clipboardData: function(e) {
  return "clipboardData" in e ? e.clipboardData : window.clipboardData;
} }), uf = Oe(sf), cf = te({}, Tn, { data: 0 }), Oo = Oe(cf), df = {
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
}, ff = {
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
}, pf = { Alt: "altKey", Control: "ctrlKey", Meta: "metaKey", Shift: "shiftKey" };
function mf(e) {
  var t = this.nativeEvent;
  return t.getModifierState ? t.getModifierState(e) : (e = pf[e]) ? !!t[e] : !1;
}
function Fa() {
  return mf;
}
var hf = te({}, Sr, { key: function(e) {
  if (e.key) {
    var t = df[e.key] || e.key;
    if (t !== "Unidentified") return t;
  }
  return e.type === "keypress" ? (e = Yr(e), e === 13 ? "Enter" : String.fromCharCode(e)) : e.type === "keydown" || e.type === "keyup" ? ff[e.keyCode] || "Unidentified" : "";
}, code: 0, location: 0, ctrlKey: 0, shiftKey: 0, altKey: 0, metaKey: 0, repeat: 0, locale: 0, getModifierState: Fa, charCode: function(e) {
  return e.type === "keypress" ? Yr(e) : 0;
}, keyCode: function(e) {
  return e.type === "keydown" || e.type === "keyup" ? e.keyCode : 0;
}, which: function(e) {
  return e.type === "keypress" ? Yr(e) : e.type === "keydown" || e.type === "keyup" ? e.keyCode : 0;
} }), vf = Oe(hf), gf = te({}, _l, { pointerId: 0, width: 0, height: 0, pressure: 0, tangentialPressure: 0, tiltX: 0, tiltY: 0, twist: 0, pointerType: 0, isPrimary: 0 }), $o = Oe(gf), xf = te({}, Sr, { touches: 0, targetTouches: 0, changedTouches: 0, altKey: 0, metaKey: 0, ctrlKey: 0, shiftKey: 0, getModifierState: Fa }), yf = Oe(xf), jf = te({}, Tn, { propertyName: 0, elapsedTime: 0, pseudoElement: 0 }), wf = Oe(jf), kf = te({}, _l, {
  deltaX: function(e) {
    return "deltaX" in e ? e.deltaX : "wheelDeltaX" in e ? -e.wheelDeltaX : 0;
  },
  deltaY: function(e) {
    return "deltaY" in e ? e.deltaY : "wheelDeltaY" in e ? -e.wheelDeltaY : "wheelDelta" in e ? -e.wheelDelta : 0;
  },
  deltaZ: 0,
  deltaMode: 0
}), Sf = Oe(kf), Nf = [9, 13, 27, 32], La = pt && "CompositionEvent" in window, Xn = null;
pt && "documentMode" in document && (Xn = document.documentMode);
var Cf = pt && "TextEvent" in window && !Xn, wu = pt && (!La || Xn && 8 < Xn && 11 >= Xn), Uo = " ", Ao = !1;
function ku(e, t) {
  switch (e) {
    case "keyup":
      return Nf.indexOf(t.keyCode) !== -1;
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
function Su(e) {
  return e = e.detail, typeof e == "object" && "data" in e ? e.data : null;
}
var ln = !1;
function Ef(e, t) {
  switch (e) {
    case "compositionend":
      return Su(t);
    case "keypress":
      return t.which !== 32 ? null : (Ao = !0, Uo);
    case "textInput":
      return e = t.data, e === Uo && Ao ? null : e;
    default:
      return null;
  }
}
function Pf(e, t) {
  if (ln) return e === "compositionend" || !La && ku(e, t) ? (e = ju(), Gr = za = St = null, ln = !1, e) : null;
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
      return wu && t.locale !== "ko" ? null : t.data;
    default:
      return null;
  }
}
var _f = { color: !0, date: !0, datetime: !0, "datetime-local": !0, email: !0, month: !0, number: !0, password: !0, range: !0, search: !0, tel: !0, text: !0, time: !0, url: !0, week: !0 };
function Vo(e) {
  var t = e && e.nodeName && e.nodeName.toLowerCase();
  return t === "input" ? !!_f[e.type] : t === "textarea";
}
function Nu(e, t, n, r) {
  tu(r), t = ul(t, "onChange"), 0 < t.length && (n = new Ta("onChange", "change", null, n, r), e.push({ event: n, listeners: t }));
}
var qn = null, ur = null;
function If(e) {
  Du(e, 0);
}
function Il(e) {
  var t = sn(e);
  if (Ys(t)) return e;
}
function zf(e, t) {
  if (e === "change") return t;
}
var Cu = !1;
if (pt) {
  var ii;
  if (pt) {
    var ai = "oninput" in document;
    if (!ai) {
      var Bo = document.createElement("div");
      Bo.setAttribute("oninput", "return;"), ai = typeof Bo.oninput == "function";
    }
    ii = ai;
  } else ii = !1;
  Cu = ii && (!document.documentMode || 9 < document.documentMode);
}
function Ho() {
  qn && (qn.detachEvent("onpropertychange", Eu), ur = qn = null);
}
function Eu(e) {
  if (e.propertyName === "value" && Il(ur)) {
    var t = [];
    Nu(t, ur, e, Ca(e)), iu(If, t);
  }
}
function Tf(e, t, n) {
  e === "focusin" ? (Ho(), qn = t, ur = n, qn.attachEvent("onpropertychange", Eu)) : e === "focusout" && Ho();
}
function Ff(e) {
  if (e === "selectionchange" || e === "keyup" || e === "keydown") return Il(ur);
}
function Lf(e, t) {
  if (e === "click") return Il(t);
}
function Rf(e, t) {
  if (e === "input" || e === "change") return Il(t);
}
function Df(e, t) {
  return e === t && (e !== 0 || 1 / e === 1 / t) || e !== e && t !== t;
}
var et = typeof Object.is == "function" ? Object.is : Df;
function cr(e, t) {
  if (et(e, t)) return !0;
  if (typeof e != "object" || e === null || typeof t != "object" || t === null) return !1;
  var n = Object.keys(e), r = Object.keys(t);
  if (n.length !== r.length) return !1;
  for (r = 0; r < n.length; r++) {
    var l = n[r];
    if (!ki.call(t, l) || !et(e[l], t[l])) return !1;
  }
  return !0;
}
function Wo(e) {
  for (; e && e.firstChild; ) e = e.firstChild;
  return e;
}
function Qo(e, t) {
  var n = Wo(e);
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
    n = Wo(n);
  }
}
function Pu(e, t) {
  return e && t ? e === t ? !0 : e && e.nodeType === 3 ? !1 : t && t.nodeType === 3 ? Pu(e, t.parentNode) : "contains" in e ? e.contains(t) : e.compareDocumentPosition ? !!(e.compareDocumentPosition(t) & 16) : !1 : !1;
}
function _u() {
  for (var e = window, t = nl(); t instanceof e.HTMLIFrameElement; ) {
    try {
      var n = typeof t.contentWindow.location.href == "string";
    } catch {
      n = !1;
    }
    if (n) e = t.contentWindow;
    else break;
    t = nl(e.document);
  }
  return t;
}
function Ra(e) {
  var t = e && e.nodeName && e.nodeName.toLowerCase();
  return t && (t === "input" && (e.type === "text" || e.type === "search" || e.type === "tel" || e.type === "url" || e.type === "password") || t === "textarea" || e.contentEditable === "true");
}
function Mf(e) {
  var t = _u(), n = e.focusedElem, r = e.selectionRange;
  if (t !== n && n && n.ownerDocument && Pu(n.ownerDocument.documentElement, n)) {
    if (r !== null && Ra(n)) {
      if (t = r.start, e = r.end, e === void 0 && (e = t), "selectionStart" in n) n.selectionStart = t, n.selectionEnd = Math.min(e, n.value.length);
      else if (e = (t = n.ownerDocument || document) && t.defaultView || window, e.getSelection) {
        e = e.getSelection();
        var l = n.textContent.length, i = Math.min(r.start, l);
        r = r.end === void 0 ? i : Math.min(r.end, l), !e.extend && i > r && (l = r, r = i, i = l), l = Qo(n, i);
        var o = Qo(
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
var Of = pt && "documentMode" in document && 11 >= document.documentMode, an = null, Vi = null, Zn = null, Bi = !1;
function Ko(e, t, n) {
  var r = n.window === n ? n.document : n.nodeType === 9 ? n : n.ownerDocument;
  Bi || an == null || an !== nl(r) || (r = an, "selectionStart" in r && Ra(r) ? r = { start: r.selectionStart, end: r.selectionEnd } : (r = (r.ownerDocument && r.ownerDocument.defaultView || window).getSelection(), r = { anchorNode: r.anchorNode, anchorOffset: r.anchorOffset, focusNode: r.focusNode, focusOffset: r.focusOffset }), Zn && cr(Zn, r) || (Zn = r, r = ul(Vi, "onSelect"), 0 < r.length && (t = new Ta("onSelect", "select", null, t, n), e.push({ event: t, listeners: r }), t.target = an)));
}
function Dr(e, t) {
  var n = {};
  return n[e.toLowerCase()] = t.toLowerCase(), n["Webkit" + e] = "webkit" + t, n["Moz" + e] = "moz" + t, n;
}
var on = { animationend: Dr("Animation", "AnimationEnd"), animationiteration: Dr("Animation", "AnimationIteration"), animationstart: Dr("Animation", "AnimationStart"), transitionend: Dr("Transition", "TransitionEnd") }, oi = {}, Iu = {};
pt && (Iu = document.createElement("div").style, "AnimationEvent" in window || (delete on.animationend.animation, delete on.animationiteration.animation, delete on.animationstart.animation), "TransitionEvent" in window || delete on.transitionend.transition);
function zl(e) {
  if (oi[e]) return oi[e];
  if (!on[e]) return e;
  var t = on[e], n;
  for (n in t) if (t.hasOwnProperty(n) && n in Iu) return oi[e] = t[n];
  return e;
}
var zu = zl("animationend"), Tu = zl("animationiteration"), Fu = zl("animationstart"), Lu = zl("transitionend"), Ru = /* @__PURE__ */ new Map(), Go = "abort auxClick cancel canPlay canPlayThrough click close contextMenu copy cut drag dragEnd dragEnter dragExit dragLeave dragOver dragStart drop durationChange emptied encrypted ended error gotPointerCapture input invalid keyDown keyPress keyUp load loadedData loadedMetadata loadStart lostPointerCapture mouseDown mouseMove mouseOut mouseOver mouseUp paste pause play playing pointerCancel pointerDown pointerMove pointerOut pointerOver pointerUp progress rateChange reset resize seeked seeking stalled submit suspend timeUpdate touchCancel touchEnd touchStart volumeChange scroll toggle touchMove waiting wheel".split(" ");
function Dt(e, t) {
  Ru.set(e, t), Jt(t, [e]);
}
for (var si = 0; si < Go.length; si++) {
  var ui = Go[si], $f = ui.toLowerCase(), Uf = ui[0].toUpperCase() + ui.slice(1);
  Dt($f, "on" + Uf);
}
Dt(zu, "onAnimationEnd");
Dt(Tu, "onAnimationIteration");
Dt(Fu, "onAnimationStart");
Dt("dblclick", "onDoubleClick");
Dt("focusin", "onFocus");
Dt("focusout", "onBlur");
Dt(Lu, "onTransitionEnd");
wn("onMouseEnter", ["mouseout", "mouseover"]);
wn("onMouseLeave", ["mouseout", "mouseover"]);
wn("onPointerEnter", ["pointerout", "pointerover"]);
wn("onPointerLeave", ["pointerout", "pointerover"]);
Jt("onChange", "change click focusin focusout input keydown keyup selectionchange".split(" "));
Jt("onSelect", "focusout contextmenu dragend focusin keydown keyup mousedown mouseup selectionchange".split(" "));
Jt("onBeforeInput", ["compositionend", "keypress", "textInput", "paste"]);
Jt("onCompositionEnd", "compositionend focusout keydown keypress keyup mousedown".split(" "));
Jt("onCompositionStart", "compositionstart focusout keydown keypress keyup mousedown".split(" "));
Jt("onCompositionUpdate", "compositionupdate focusout keydown keypress keyup mousedown".split(" "));
var Kn = "abort canplay canplaythrough durationchange emptied encrypted ended error loadeddata loadedmetadata loadstart pause play playing progress ratechange resize seeked seeking stalled suspend timeupdate volumechange waiting".split(" "), Af = new Set("cancel close invalid load scroll toggle".split(" ").concat(Kn));
function Yo(e, t, n) {
  var r = e.type || "unknown-event";
  e.currentTarget = n, Od(r, t, void 0, e), e.currentTarget = null;
}
function Du(e, t) {
  t = (t & 4) !== 0;
  for (var n = 0; n < e.length; n++) {
    var r = e[n], l = r.event;
    r = r.listeners;
    e: {
      var i = void 0;
      if (t) for (var o = r.length - 1; 0 <= o; o--) {
        var s = r[o], u = s.instance, d = s.currentTarget;
        if (s = s.listener, u !== i && l.isPropagationStopped()) break e;
        Yo(l, s, d), i = u;
      }
      else for (o = 0; o < r.length; o++) {
        if (s = r[o], u = s.instance, d = s.currentTarget, s = s.listener, u !== i && l.isPropagationStopped()) break e;
        Yo(l, s, d), i = u;
      }
    }
  }
  if (ll) throw e = Oi, ll = !1, Oi = null, e;
}
function X(e, t) {
  var n = t[Gi];
  n === void 0 && (n = t[Gi] = /* @__PURE__ */ new Set());
  var r = e + "__bubble";
  n.has(r) || (Mu(t, e, 2, !1), n.add(r));
}
function ci(e, t, n) {
  var r = 0;
  t && (r |= 4), Mu(n, e, r, t);
}
var Mr = "_reactListening" + Math.random().toString(36).slice(2);
function dr(e) {
  if (!e[Mr]) {
    e[Mr] = !0, Hs.forEach(function(n) {
      n !== "selectionchange" && (Af.has(n) || ci(n, !1, e), ci(n, !0, e));
    });
    var t = e.nodeType === 9 ? e : e.ownerDocument;
    t === null || t[Mr] || (t[Mr] = !0, ci("selectionchange", !1, t));
  }
}
function Mu(e, t, n, r) {
  switch (yu(t)) {
    case 1:
      var l = bd;
      break;
    case 4:
      l = ef;
      break;
    default:
      l = Ia;
  }
  n = l.bind(null, t, n, e), l = void 0, !Mi || t !== "touchstart" && t !== "touchmove" && t !== "wheel" || (l = !0), r ? l !== void 0 ? e.addEventListener(t, n, { capture: !0, passive: l }) : e.addEventListener(t, n, !0) : l !== void 0 ? e.addEventListener(t, n, { passive: l }) : e.addEventListener(t, n, !1);
}
function di(e, t, n, r, l) {
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
        if (o = Bt(s), o === null) return;
        if (u = o.tag, u === 5 || u === 6) {
          r = i = o;
          continue e;
        }
        s = s.parentNode;
      }
    }
    r = r.return;
  }
  iu(function() {
    var d = i, x = Ca(n), c = [];
    e: {
      var m = Ru.get(e);
      if (m !== void 0) {
        var v = Ta, g = e;
        switch (e) {
          case "keypress":
            if (Yr(n) === 0) break e;
          case "keydown":
          case "keyup":
            v = vf;
            break;
          case "focusin":
            g = "focus", v = li;
            break;
          case "focusout":
            g = "blur", v = li;
            break;
          case "beforeblur":
          case "afterblur":
            v = li;
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
            v = Mo;
            break;
          case "drag":
          case "dragend":
          case "dragenter":
          case "dragexit":
          case "dragleave":
          case "dragover":
          case "dragstart":
          case "drop":
            v = rf;
            break;
          case "touchcancel":
          case "touchend":
          case "touchmove":
          case "touchstart":
            v = yf;
            break;
          case zu:
          case Tu:
          case Fu:
            v = of;
            break;
          case Lu:
            v = wf;
            break;
          case "scroll":
            v = tf;
            break;
          case "wheel":
            v = Sf;
            break;
          case "copy":
          case "cut":
          case "paste":
            v = uf;
            break;
          case "gotpointercapture":
          case "lostpointercapture":
          case "pointercancel":
          case "pointerdown":
          case "pointermove":
          case "pointerout":
          case "pointerover":
          case "pointerup":
            v = $o;
        }
        var k = (t & 4) !== 0, F = !k && e === "scroll", f = k ? m !== null ? m + "Capture" : null : m;
        k = [];
        for (var p = d, h; p !== null; ) {
          h = p;
          var w = h.stateNode;
          if (h.tag === 5 && w !== null && (h = w, f !== null && (w = ir(p, f), w != null && k.push(fr(p, w, h)))), F) break;
          p = p.return;
        }
        0 < k.length && (m = new v(m, g, null, n, x), c.push({ event: m, listeners: k }));
      }
    }
    if (!(t & 7)) {
      e: {
        if (m = e === "mouseover" || e === "pointerover", v = e === "mouseout" || e === "pointerout", m && n !== Ri && (g = n.relatedTarget || n.fromElement) && (Bt(g) || g[mt])) break e;
        if ((v || m) && (m = x.window === x ? x : (m = x.ownerDocument) ? m.defaultView || m.parentWindow : window, v ? (g = n.relatedTarget || n.toElement, v = d, g = g ? Bt(g) : null, g !== null && (F = bt(g), g !== F || g.tag !== 5 && g.tag !== 6) && (g = null)) : (v = null, g = d), v !== g)) {
          if (k = Mo, w = "onMouseLeave", f = "onMouseEnter", p = "mouse", (e === "pointerout" || e === "pointerover") && (k = $o, w = "onPointerLeave", f = "onPointerEnter", p = "pointer"), F = v == null ? m : sn(v), h = g == null ? m : sn(g), m = new k(w, p + "leave", v, n, x), m.target = F, m.relatedTarget = h, w = null, Bt(x) === d && (k = new k(f, p + "enter", g, n, x), k.target = h, k.relatedTarget = F, w = k), F = w, v && g) t: {
            for (k = v, f = g, p = 0, h = k; h; h = tn(h)) p++;
            for (h = 0, w = f; w; w = tn(w)) h++;
            for (; 0 < p - h; ) k = tn(k), p--;
            for (; 0 < h - p; ) f = tn(f), h--;
            for (; p--; ) {
              if (k === f || f !== null && k === f.alternate) break t;
              k = tn(k), f = tn(f);
            }
            k = null;
          }
          else k = null;
          v !== null && Xo(c, m, v, k, !1), g !== null && F !== null && Xo(c, F, g, k, !0);
        }
      }
      e: {
        if (m = d ? sn(d) : window, v = m.nodeName && m.nodeName.toLowerCase(), v === "select" || v === "input" && m.type === "file") var I = zf;
        else if (Vo(m)) if (Cu) I = Rf;
        else {
          I = Ff;
          var _ = Tf;
        }
        else (v = m.nodeName) && v.toLowerCase() === "input" && (m.type === "checkbox" || m.type === "radio") && (I = Lf);
        if (I && (I = I(e, d))) {
          Nu(c, I, n, x);
          break e;
        }
        _ && _(e, m, d), e === "focusout" && (_ = m._wrapperState) && _.controlled && m.type === "number" && Ii(m, "number", m.value);
      }
      switch (_ = d ? sn(d) : window, e) {
        case "focusin":
          (Vo(_) || _.contentEditable === "true") && (an = _, Vi = d, Zn = null);
          break;
        case "focusout":
          Zn = Vi = an = null;
          break;
        case "mousedown":
          Bi = !0;
          break;
        case "contextmenu":
        case "mouseup":
        case "dragend":
          Bi = !1, Ko(c, n, x);
          break;
        case "selectionchange":
          if (Of) break;
        case "keydown":
        case "keyup":
          Ko(c, n, x);
      }
      var L;
      if (La) e: {
        switch (e) {
          case "compositionstart":
            var N = "onCompositionStart";
            break e;
          case "compositionend":
            N = "onCompositionEnd";
            break e;
          case "compositionupdate":
            N = "onCompositionUpdate";
            break e;
        }
        N = void 0;
      }
      else ln ? ku(e, n) && (N = "onCompositionEnd") : e === "keydown" && n.keyCode === 229 && (N = "onCompositionStart");
      N && (wu && n.locale !== "ko" && (ln || N !== "onCompositionStart" ? N === "onCompositionEnd" && ln && (L = ju()) : (St = x, za = "value" in St ? St.value : St.textContent, ln = !0)), _ = ul(d, N), 0 < _.length && (N = new Oo(N, e, null, n, x), c.push({ event: N, listeners: _ }), L ? N.data = L : (L = Su(n), L !== null && (N.data = L)))), (L = Cf ? Ef(e, n) : Pf(e, n)) && (d = ul(d, "onBeforeInput"), 0 < d.length && (x = new Oo("onBeforeInput", "beforeinput", null, n, x), c.push({ event: x, listeners: d }), x.data = L));
    }
    Du(c, t);
  });
}
function fr(e, t, n) {
  return { instance: e, listener: t, currentTarget: n };
}
function ul(e, t) {
  for (var n = t + "Capture", r = []; e !== null; ) {
    var l = e, i = l.stateNode;
    l.tag === 5 && i !== null && (l = i, i = ir(e, n), i != null && r.unshift(fr(e, i, l)), i = ir(e, t), i != null && r.push(fr(e, i, l))), e = e.return;
  }
  return r;
}
function tn(e) {
  if (e === null) return null;
  do
    e = e.return;
  while (e && e.tag !== 5);
  return e || null;
}
function Xo(e, t, n, r, l) {
  for (var i = t._reactName, o = []; n !== null && n !== r; ) {
    var s = n, u = s.alternate, d = s.stateNode;
    if (u !== null && u === r) break;
    s.tag === 5 && d !== null && (s = d, l ? (u = ir(n, i), u != null && o.unshift(fr(n, u, s))) : l || (u = ir(n, i), u != null && o.push(fr(n, u, s)))), n = n.return;
  }
  o.length !== 0 && e.push({ event: t, listeners: o });
}
var Vf = /\r\n?/g, Bf = /\u0000|\uFFFD/g;
function qo(e) {
  return (typeof e == "string" ? e : "" + e).replace(Vf, `
`).replace(Bf, "");
}
function Or(e, t, n) {
  if (t = qo(t), qo(e) !== t && n) throw Error(E(425));
}
function cl() {
}
var Hi = null, Wi = null;
function Qi(e, t) {
  return e === "textarea" || e === "noscript" || typeof t.children == "string" || typeof t.children == "number" || typeof t.dangerouslySetInnerHTML == "object" && t.dangerouslySetInnerHTML !== null && t.dangerouslySetInnerHTML.__html != null;
}
var Ki = typeof setTimeout == "function" ? setTimeout : void 0, Hf = typeof clearTimeout == "function" ? clearTimeout : void 0, Zo = typeof Promise == "function" ? Promise : void 0, Wf = typeof queueMicrotask == "function" ? queueMicrotask : typeof Zo < "u" ? function(e) {
  return Zo.resolve(null).then(e).catch(Qf);
} : Ki;
function Qf(e) {
  setTimeout(function() {
    throw e;
  });
}
function fi(e, t) {
  var n = t, r = 0;
  do {
    var l = n.nextSibling;
    if (e.removeChild(n), l && l.nodeType === 8) if (n = l.data, n === "/$") {
      if (r === 0) {
        e.removeChild(l), sr(t);
        return;
      }
      r--;
    } else n !== "$" && n !== "$?" && n !== "$!" || r++;
    n = l;
  } while (n);
  sr(t);
}
function _t(e) {
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
function Jo(e) {
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
var Fn = Math.random().toString(36).slice(2), rt = "__reactFiber$" + Fn, pr = "__reactProps$" + Fn, mt = "__reactContainer$" + Fn, Gi = "__reactEvents$" + Fn, Kf = "__reactListeners$" + Fn, Gf = "__reactHandles$" + Fn;
function Bt(e) {
  var t = e[rt];
  if (t) return t;
  for (var n = e.parentNode; n; ) {
    if (t = n[mt] || n[rt]) {
      if (n = t.alternate, t.child !== null || n !== null && n.child !== null) for (e = Jo(e); e !== null; ) {
        if (n = e[rt]) return n;
        e = Jo(e);
      }
      return t;
    }
    e = n, n = e.parentNode;
  }
  return null;
}
function Nr(e) {
  return e = e[rt] || e[mt], !e || e.tag !== 5 && e.tag !== 6 && e.tag !== 13 && e.tag !== 3 ? null : e;
}
function sn(e) {
  if (e.tag === 5 || e.tag === 6) return e.stateNode;
  throw Error(E(33));
}
function Tl(e) {
  return e[pr] || null;
}
var Yi = [], un = -1;
function Mt(e) {
  return { current: e };
}
function q(e) {
  0 > un || (e.current = Yi[un], Yi[un] = null, un--);
}
function Y(e, t) {
  un++, Yi[un] = e.current, e.current = t;
}
var Rt = {}, je = Mt(Rt), Ie = Mt(!1), Gt = Rt;
function kn(e, t) {
  var n = e.type.contextTypes;
  if (!n) return Rt;
  var r = e.stateNode;
  if (r && r.__reactInternalMemoizedUnmaskedChildContext === t) return r.__reactInternalMemoizedMaskedChildContext;
  var l = {}, i;
  for (i in n) l[i] = t[i];
  return r && (e = e.stateNode, e.__reactInternalMemoizedUnmaskedChildContext = t, e.__reactInternalMemoizedMaskedChildContext = l), l;
}
function ze(e) {
  return e = e.childContextTypes, e != null;
}
function dl() {
  q(Ie), q(je);
}
function bo(e, t, n) {
  if (je.current !== Rt) throw Error(E(168));
  Y(je, t), Y(Ie, n);
}
function Ou(e, t, n) {
  var r = e.stateNode;
  if (t = t.childContextTypes, typeof r.getChildContext != "function") return n;
  r = r.getChildContext();
  for (var l in r) if (!(l in t)) throw Error(E(108, zd(e) || "Unknown", l));
  return te({}, n, r);
}
function fl(e) {
  return e = (e = e.stateNode) && e.__reactInternalMemoizedMergedChildContext || Rt, Gt = je.current, Y(je, e), Y(Ie, Ie.current), !0;
}
function es(e, t, n) {
  var r = e.stateNode;
  if (!r) throw Error(E(169));
  n ? (e = Ou(e, t, Gt), r.__reactInternalMemoizedMergedChildContext = e, q(Ie), q(je), Y(je, e)) : q(Ie), Y(Ie, n);
}
var ut = null, Fl = !1, pi = !1;
function $u(e) {
  ut === null ? ut = [e] : ut.push(e);
}
function Yf(e) {
  Fl = !0, $u(e);
}
function Ot() {
  if (!pi && ut !== null) {
    pi = !0;
    var e = 0, t = H;
    try {
      var n = ut;
      for (H = 1; e < n.length; e++) {
        var r = n[e];
        do
          r = r(!0);
        while (r !== null);
      }
      ut = null, Fl = !1;
    } catch (l) {
      throw ut !== null && (ut = ut.slice(e + 1)), uu(Ea, Ot), l;
    } finally {
      H = t, pi = !1;
    }
  }
  return null;
}
var cn = [], dn = 0, pl = null, ml = 0, Ue = [], Ae = 0, Yt = null, ct = 1, dt = "";
function At(e, t) {
  cn[dn++] = ml, cn[dn++] = pl, pl = e, ml = t;
}
function Uu(e, t, n) {
  Ue[Ae++] = ct, Ue[Ae++] = dt, Ue[Ae++] = Yt, Yt = e;
  var r = ct;
  e = dt;
  var l = 32 - Je(r) - 1;
  r &= ~(1 << l), n += 1;
  var i = 32 - Je(t) + l;
  if (30 < i) {
    var o = l - l % 5;
    i = (r & (1 << o) - 1).toString(32), r >>= o, l -= o, ct = 1 << 32 - Je(t) + l | n << l | r, dt = i + e;
  } else ct = 1 << i | n << l | r, dt = e;
}
function Da(e) {
  e.return !== null && (At(e, 1), Uu(e, 1, 0));
}
function Ma(e) {
  for (; e === pl; ) pl = cn[--dn], cn[dn] = null, ml = cn[--dn], cn[dn] = null;
  for (; e === Yt; ) Yt = Ue[--Ae], Ue[Ae] = null, dt = Ue[--Ae], Ue[Ae] = null, ct = Ue[--Ae], Ue[Ae] = null;
}
var Re = null, Le = null, J = !1, qe = null;
function Au(e, t) {
  var n = Ve(5, null, null, 0);
  n.elementType = "DELETED", n.stateNode = t, n.return = e, t = e.deletions, t === null ? (e.deletions = [n], e.flags |= 16) : t.push(n);
}
function ts(e, t) {
  switch (e.tag) {
    case 5:
      var n = e.type;
      return t = t.nodeType !== 1 || n.toLowerCase() !== t.nodeName.toLowerCase() ? null : t, t !== null ? (e.stateNode = t, Re = e, Le = _t(t.firstChild), !0) : !1;
    case 6:
      return t = e.pendingProps === "" || t.nodeType !== 3 ? null : t, t !== null ? (e.stateNode = t, Re = e, Le = null, !0) : !1;
    case 13:
      return t = t.nodeType !== 8 ? null : t, t !== null ? (n = Yt !== null ? { id: ct, overflow: dt } : null, e.memoizedState = { dehydrated: t, treeContext: n, retryLane: 1073741824 }, n = Ve(18, null, null, 0), n.stateNode = t, n.return = e, e.child = n, Re = e, Le = null, !0) : !1;
    default:
      return !1;
  }
}
function Xi(e) {
  return (e.mode & 1) !== 0 && (e.flags & 128) === 0;
}
function qi(e) {
  if (J) {
    var t = Le;
    if (t) {
      var n = t;
      if (!ts(e, t)) {
        if (Xi(e)) throw Error(E(418));
        t = _t(n.nextSibling);
        var r = Re;
        t && ts(e, t) ? Au(r, n) : (e.flags = e.flags & -4097 | 2, J = !1, Re = e);
      }
    } else {
      if (Xi(e)) throw Error(E(418));
      e.flags = e.flags & -4097 | 2, J = !1, Re = e;
    }
  }
}
function ns(e) {
  for (e = e.return; e !== null && e.tag !== 5 && e.tag !== 3 && e.tag !== 13; ) e = e.return;
  Re = e;
}
function $r(e) {
  if (e !== Re) return !1;
  if (!J) return ns(e), J = !0, !1;
  var t;
  if ((t = e.tag !== 3) && !(t = e.tag !== 5) && (t = e.type, t = t !== "head" && t !== "body" && !Qi(e.type, e.memoizedProps)), t && (t = Le)) {
    if (Xi(e)) throw Vu(), Error(E(418));
    for (; t; ) Au(e, t), t = _t(t.nextSibling);
  }
  if (ns(e), e.tag === 13) {
    if (e = e.memoizedState, e = e !== null ? e.dehydrated : null, !e) throw Error(E(317));
    e: {
      for (e = e.nextSibling, t = 0; e; ) {
        if (e.nodeType === 8) {
          var n = e.data;
          if (n === "/$") {
            if (t === 0) {
              Le = _t(e.nextSibling);
              break e;
            }
            t--;
          } else n !== "$" && n !== "$!" && n !== "$?" || t++;
        }
        e = e.nextSibling;
      }
      Le = null;
    }
  } else Le = Re ? _t(e.stateNode.nextSibling) : null;
  return !0;
}
function Vu() {
  for (var e = Le; e; ) e = _t(e.nextSibling);
}
function Sn() {
  Le = Re = null, J = !1;
}
function Oa(e) {
  qe === null ? qe = [e] : qe.push(e);
}
var Xf = gt.ReactCurrentBatchConfig;
function An(e, t, n) {
  if (e = n.ref, e !== null && typeof e != "function" && typeof e != "object") {
    if (n._owner) {
      if (n = n._owner, n) {
        if (n.tag !== 1) throw Error(E(309));
        var r = n.stateNode;
      }
      if (!r) throw Error(E(147, e));
      var l = r, i = "" + e;
      return t !== null && t.ref !== null && typeof t.ref == "function" && t.ref._stringRef === i ? t.ref : (t = function(o) {
        var s = l.refs;
        o === null ? delete s[i] : s[i] = o;
      }, t._stringRef = i, t);
    }
    if (typeof e != "string") throw Error(E(284));
    if (!n._owner) throw Error(E(290, e));
  }
  return e;
}
function Ur(e, t) {
  throw e = Object.prototype.toString.call(t), Error(E(31, e === "[object Object]" ? "object with keys {" + Object.keys(t).join(", ") + "}" : e));
}
function rs(e) {
  var t = e._init;
  return t(e._payload);
}
function Bu(e) {
  function t(f, p) {
    if (e) {
      var h = f.deletions;
      h === null ? (f.deletions = [p], f.flags |= 16) : h.push(p);
    }
  }
  function n(f, p) {
    if (!e) return null;
    for (; p !== null; ) t(f, p), p = p.sibling;
    return null;
  }
  function r(f, p) {
    for (f = /* @__PURE__ */ new Map(); p !== null; ) p.key !== null ? f.set(p.key, p) : f.set(p.index, p), p = p.sibling;
    return f;
  }
  function l(f, p) {
    return f = Ft(f, p), f.index = 0, f.sibling = null, f;
  }
  function i(f, p, h) {
    return f.index = h, e ? (h = f.alternate, h !== null ? (h = h.index, h < p ? (f.flags |= 2, p) : h) : (f.flags |= 2, p)) : (f.flags |= 1048576, p);
  }
  function o(f) {
    return e && f.alternate === null && (f.flags |= 2), f;
  }
  function s(f, p, h, w) {
    return p === null || p.tag !== 6 ? (p = ji(h, f.mode, w), p.return = f, p) : (p = l(p, h), p.return = f, p);
  }
  function u(f, p, h, w) {
    var I = h.type;
    return I === rn ? x(f, p, h.props.children, w, h.key) : p !== null && (p.elementType === I || typeof I == "object" && I !== null && I.$$typeof === yt && rs(I) === p.type) ? (w = l(p, h.props), w.ref = An(f, p, h), w.return = f, w) : (w = tl(h.type, h.key, h.props, null, f.mode, w), w.ref = An(f, p, h), w.return = f, w);
  }
  function d(f, p, h, w) {
    return p === null || p.tag !== 4 || p.stateNode.containerInfo !== h.containerInfo || p.stateNode.implementation !== h.implementation ? (p = wi(h, f.mode, w), p.return = f, p) : (p = l(p, h.children || []), p.return = f, p);
  }
  function x(f, p, h, w, I) {
    return p === null || p.tag !== 7 ? (p = Kt(h, f.mode, w, I), p.return = f, p) : (p = l(p, h), p.return = f, p);
  }
  function c(f, p, h) {
    if (typeof p == "string" && p !== "" || typeof p == "number") return p = ji("" + p, f.mode, h), p.return = f, p;
    if (typeof p == "object" && p !== null) {
      switch (p.$$typeof) {
        case _r:
          return h = tl(p.type, p.key, p.props, null, f.mode, h), h.ref = An(f, null, p), h.return = f, h;
        case nn:
          return p = wi(p, f.mode, h), p.return = f, p;
        case yt:
          var w = p._init;
          return c(f, w(p._payload), h);
      }
      if (Wn(p) || Dn(p)) return p = Kt(p, f.mode, h, null), p.return = f, p;
      Ur(f, p);
    }
    return null;
  }
  function m(f, p, h, w) {
    var I = p !== null ? p.key : null;
    if (typeof h == "string" && h !== "" || typeof h == "number") return I !== null ? null : s(f, p, "" + h, w);
    if (typeof h == "object" && h !== null) {
      switch (h.$$typeof) {
        case _r:
          return h.key === I ? u(f, p, h, w) : null;
        case nn:
          return h.key === I ? d(f, p, h, w) : null;
        case yt:
          return I = h._init, m(
            f,
            p,
            I(h._payload),
            w
          );
      }
      if (Wn(h) || Dn(h)) return I !== null ? null : x(f, p, h, w, null);
      Ur(f, h);
    }
    return null;
  }
  function v(f, p, h, w, I) {
    if (typeof w == "string" && w !== "" || typeof w == "number") return f = f.get(h) || null, s(p, f, "" + w, I);
    if (typeof w == "object" && w !== null) {
      switch (w.$$typeof) {
        case _r:
          return f = f.get(w.key === null ? h : w.key) || null, u(p, f, w, I);
        case nn:
          return f = f.get(w.key === null ? h : w.key) || null, d(p, f, w, I);
        case yt:
          var _ = w._init;
          return v(f, p, h, _(w._payload), I);
      }
      if (Wn(w) || Dn(w)) return f = f.get(h) || null, x(p, f, w, I, null);
      Ur(p, w);
    }
    return null;
  }
  function g(f, p, h, w) {
    for (var I = null, _ = null, L = p, N = p = 0, z = null; L !== null && N < h.length; N++) {
      L.index > N ? (z = L, L = null) : z = L.sibling;
      var M = m(f, L, h[N], w);
      if (M === null) {
        L === null && (L = z);
        break;
      }
      e && L && M.alternate === null && t(f, L), p = i(M, p, N), _ === null ? I = M : _.sibling = M, _ = M, L = z;
    }
    if (N === h.length) return n(f, L), J && At(f, N), I;
    if (L === null) {
      for (; N < h.length; N++) L = c(f, h[N], w), L !== null && (p = i(L, p, N), _ === null ? I = L : _.sibling = L, _ = L);
      return J && At(f, N), I;
    }
    for (L = r(f, L); N < h.length; N++) z = v(L, f, N, h[N], w), z !== null && (e && z.alternate !== null && L.delete(z.key === null ? N : z.key), p = i(z, p, N), _ === null ? I = z : _.sibling = z, _ = z);
    return e && L.forEach(function(P) {
      return t(f, P);
    }), J && At(f, N), I;
  }
  function k(f, p, h, w) {
    var I = Dn(h);
    if (typeof I != "function") throw Error(E(150));
    if (h = I.call(h), h == null) throw Error(E(151));
    for (var _ = I = null, L = p, N = p = 0, z = null, M = h.next(); L !== null && !M.done; N++, M = h.next()) {
      L.index > N ? (z = L, L = null) : z = L.sibling;
      var P = m(f, L, M.value, w);
      if (P === null) {
        L === null && (L = z);
        break;
      }
      e && L && P.alternate === null && t(f, L), p = i(P, p, N), _ === null ? I = P : _.sibling = P, _ = P, L = z;
    }
    if (M.done) return n(
      f,
      L
    ), J && At(f, N), I;
    if (L === null) {
      for (; !M.done; N++, M = h.next()) M = c(f, M.value, w), M !== null && (p = i(M, p, N), _ === null ? I = M : _.sibling = M, _ = M);
      return J && At(f, N), I;
    }
    for (L = r(f, L); !M.done; N++, M = h.next()) M = v(L, f, N, M.value, w), M !== null && (e && M.alternate !== null && L.delete(M.key === null ? N : M.key), p = i(M, p, N), _ === null ? I = M : _.sibling = M, _ = M);
    return e && L.forEach(function(Q) {
      return t(f, Q);
    }), J && At(f, N), I;
  }
  function F(f, p, h, w) {
    if (typeof h == "object" && h !== null && h.type === rn && h.key === null && (h = h.props.children), typeof h == "object" && h !== null) {
      switch (h.$$typeof) {
        case _r:
          e: {
            for (var I = h.key, _ = p; _ !== null; ) {
              if (_.key === I) {
                if (I = h.type, I === rn) {
                  if (_.tag === 7) {
                    n(f, _.sibling), p = l(_, h.props.children), p.return = f, f = p;
                    break e;
                  }
                } else if (_.elementType === I || typeof I == "object" && I !== null && I.$$typeof === yt && rs(I) === _.type) {
                  n(f, _.sibling), p = l(_, h.props), p.ref = An(f, _, h), p.return = f, f = p;
                  break e;
                }
                n(f, _);
                break;
              } else t(f, _);
              _ = _.sibling;
            }
            h.type === rn ? (p = Kt(h.props.children, f.mode, w, h.key), p.return = f, f = p) : (w = tl(h.type, h.key, h.props, null, f.mode, w), w.ref = An(f, p, h), w.return = f, f = w);
          }
          return o(f);
        case nn:
          e: {
            for (_ = h.key; p !== null; ) {
              if (p.key === _) if (p.tag === 4 && p.stateNode.containerInfo === h.containerInfo && p.stateNode.implementation === h.implementation) {
                n(f, p.sibling), p = l(p, h.children || []), p.return = f, f = p;
                break e;
              } else {
                n(f, p);
                break;
              }
              else t(f, p);
              p = p.sibling;
            }
            p = wi(h, f.mode, w), p.return = f, f = p;
          }
          return o(f);
        case yt:
          return _ = h._init, F(f, p, _(h._payload), w);
      }
      if (Wn(h)) return g(f, p, h, w);
      if (Dn(h)) return k(f, p, h, w);
      Ur(f, h);
    }
    return typeof h == "string" && h !== "" || typeof h == "number" ? (h = "" + h, p !== null && p.tag === 6 ? (n(f, p.sibling), p = l(p, h), p.return = f, f = p) : (n(f, p), p = ji(h, f.mode, w), p.return = f, f = p), o(f)) : n(f, p);
  }
  return F;
}
var Nn = Bu(!0), Hu = Bu(!1), hl = Mt(null), vl = null, fn = null, $a = null;
function Ua() {
  $a = fn = vl = null;
}
function Aa(e) {
  var t = hl.current;
  q(hl), e._currentValue = t;
}
function Zi(e, t, n) {
  for (; e !== null; ) {
    var r = e.alternate;
    if ((e.childLanes & t) !== t ? (e.childLanes |= t, r !== null && (r.childLanes |= t)) : r !== null && (r.childLanes & t) !== t && (r.childLanes |= t), e === n) break;
    e = e.return;
  }
}
function yn(e, t) {
  vl = e, $a = fn = null, e = e.dependencies, e !== null && e.firstContext !== null && (e.lanes & t && (_e = !0), e.firstContext = null);
}
function We(e) {
  var t = e._currentValue;
  if ($a !== e) if (e = { context: e, memoizedValue: t, next: null }, fn === null) {
    if (vl === null) throw Error(E(308));
    fn = e, vl.dependencies = { lanes: 0, firstContext: e };
  } else fn = fn.next = e;
  return t;
}
var Ht = null;
function Va(e) {
  Ht === null ? Ht = [e] : Ht.push(e);
}
function Wu(e, t, n, r) {
  var l = t.interleaved;
  return l === null ? (n.next = n, Va(t)) : (n.next = l.next, l.next = n), t.interleaved = n, ht(e, r);
}
function ht(e, t) {
  e.lanes |= t;
  var n = e.alternate;
  for (n !== null && (n.lanes |= t), n = e, e = e.return; e !== null; ) e.childLanes |= t, n = e.alternate, n !== null && (n.childLanes |= t), n = e, e = e.return;
  return n.tag === 3 ? n.stateNode : null;
}
var jt = !1;
function Ba(e) {
  e.updateQueue = { baseState: e.memoizedState, firstBaseUpdate: null, lastBaseUpdate: null, shared: { pending: null, interleaved: null, lanes: 0 }, effects: null };
}
function Qu(e, t) {
  e = e.updateQueue, t.updateQueue === e && (t.updateQueue = { baseState: e.baseState, firstBaseUpdate: e.firstBaseUpdate, lastBaseUpdate: e.lastBaseUpdate, shared: e.shared, effects: e.effects });
}
function ft(e, t) {
  return { eventTime: e, lane: t, tag: 0, payload: null, callback: null, next: null };
}
function It(e, t, n) {
  var r = e.updateQueue;
  if (r === null) return null;
  if (r = r.shared, B & 2) {
    var l = r.pending;
    return l === null ? t.next = t : (t.next = l.next, l.next = t), r.pending = t, ht(e, n);
  }
  return l = r.interleaved, l === null ? (t.next = t, Va(r)) : (t.next = l.next, l.next = t), r.interleaved = t, ht(e, n);
}
function Xr(e, t, n) {
  if (t = t.updateQueue, t !== null && (t = t.shared, (n & 4194240) !== 0)) {
    var r = t.lanes;
    r &= e.pendingLanes, n |= r, t.lanes = n, Pa(e, n);
  }
}
function ls(e, t) {
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
function gl(e, t, n, r) {
  var l = e.updateQueue;
  jt = !1;
  var i = l.firstBaseUpdate, o = l.lastBaseUpdate, s = l.shared.pending;
  if (s !== null) {
    l.shared.pending = null;
    var u = s, d = u.next;
    u.next = null, o === null ? i = d : o.next = d, o = u;
    var x = e.alternate;
    x !== null && (x = x.updateQueue, s = x.lastBaseUpdate, s !== o && (s === null ? x.firstBaseUpdate = d : s.next = d, x.lastBaseUpdate = u));
  }
  if (i !== null) {
    var c = l.baseState;
    o = 0, x = d = u = null, s = i;
    do {
      var m = s.lane, v = s.eventTime;
      if ((r & m) === m) {
        x !== null && (x = x.next = {
          eventTime: v,
          lane: 0,
          tag: s.tag,
          payload: s.payload,
          callback: s.callback,
          next: null
        });
        e: {
          var g = e, k = s;
          switch (m = t, v = n, k.tag) {
            case 1:
              if (g = k.payload, typeof g == "function") {
                c = g.call(v, c, m);
                break e;
              }
              c = g;
              break e;
            case 3:
              g.flags = g.flags & -65537 | 128;
            case 0:
              if (g = k.payload, m = typeof g == "function" ? g.call(v, c, m) : g, m == null) break e;
              c = te({}, c, m);
              break e;
            case 2:
              jt = !0;
          }
        }
        s.callback !== null && s.lane !== 0 && (e.flags |= 64, m = l.effects, m === null ? l.effects = [s] : m.push(s));
      } else v = { eventTime: v, lane: m, tag: s.tag, payload: s.payload, callback: s.callback, next: null }, x === null ? (d = x = v, u = c) : x = x.next = v, o |= m;
      if (s = s.next, s === null) {
        if (s = l.shared.pending, s === null) break;
        m = s, s = m.next, m.next = null, l.lastBaseUpdate = m, l.shared.pending = null;
      }
    } while (!0);
    if (x === null && (u = c), l.baseState = u, l.firstBaseUpdate = d, l.lastBaseUpdate = x, t = l.shared.interleaved, t !== null) {
      l = t;
      do
        o |= l.lane, l = l.next;
      while (l !== t);
    } else i === null && (l.shared.lanes = 0);
    qt |= o, e.lanes = o, e.memoizedState = c;
  }
}
function is(e, t, n) {
  if (e = t.effects, t.effects = null, e !== null) for (t = 0; t < e.length; t++) {
    var r = e[t], l = r.callback;
    if (l !== null) {
      if (r.callback = null, r = n, typeof l != "function") throw Error(E(191, l));
      l.call(r);
    }
  }
}
var Cr = {}, it = Mt(Cr), mr = Mt(Cr), hr = Mt(Cr);
function Wt(e) {
  if (e === Cr) throw Error(E(174));
  return e;
}
function Ha(e, t) {
  switch (Y(hr, t), Y(mr, e), Y(it, Cr), e = t.nodeType, e) {
    case 9:
    case 11:
      t = (t = t.documentElement) ? t.namespaceURI : Ti(null, "");
      break;
    default:
      e = e === 8 ? t.parentNode : t, t = e.namespaceURI || null, e = e.tagName, t = Ti(t, e);
  }
  q(it), Y(it, t);
}
function Cn() {
  q(it), q(mr), q(hr);
}
function Ku(e) {
  Wt(hr.current);
  var t = Wt(it.current), n = Ti(t, e.type);
  t !== n && (Y(mr, e), Y(it, n));
}
function Wa(e) {
  mr.current === e && (q(it), q(mr));
}
var b = Mt(0);
function xl(e) {
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
var mi = [];
function Qa() {
  for (var e = 0; e < mi.length; e++) mi[e]._workInProgressVersionPrimary = null;
  mi.length = 0;
}
var qr = gt.ReactCurrentDispatcher, hi = gt.ReactCurrentBatchConfig, Xt = 0, ee = null, se = null, de = null, yl = !1, Jn = !1, vr = 0, qf = 0;
function ve() {
  throw Error(E(321));
}
function Ka(e, t) {
  if (t === null) return !1;
  for (var n = 0; n < t.length && n < e.length; n++) if (!et(e[n], t[n])) return !1;
  return !0;
}
function Ga(e, t, n, r, l, i) {
  if (Xt = i, ee = t, t.memoizedState = null, t.updateQueue = null, t.lanes = 0, qr.current = e === null || e.memoizedState === null ? ep : tp, e = n(r, l), Jn) {
    i = 0;
    do {
      if (Jn = !1, vr = 0, 25 <= i) throw Error(E(301));
      i += 1, de = se = null, t.updateQueue = null, qr.current = np, e = n(r, l);
    } while (Jn);
  }
  if (qr.current = jl, t = se !== null && se.next !== null, Xt = 0, de = se = ee = null, yl = !1, t) throw Error(E(300));
  return e;
}
function Ya() {
  var e = vr !== 0;
  return vr = 0, e;
}
function nt() {
  var e = { memoizedState: null, baseState: null, baseQueue: null, queue: null, next: null };
  return de === null ? ee.memoizedState = de = e : de = de.next = e, de;
}
function Qe() {
  if (se === null) {
    var e = ee.alternate;
    e = e !== null ? e.memoizedState : null;
  } else e = se.next;
  var t = de === null ? ee.memoizedState : de.next;
  if (t !== null) de = t, se = e;
  else {
    if (e === null) throw Error(E(310));
    se = e, e = { memoizedState: se.memoizedState, baseState: se.baseState, baseQueue: se.baseQueue, queue: se.queue, next: null }, de === null ? ee.memoizedState = de = e : de = de.next = e;
  }
  return de;
}
function gr(e, t) {
  return typeof t == "function" ? t(e) : t;
}
function vi(e) {
  var t = Qe(), n = t.queue;
  if (n === null) throw Error(E(311));
  n.lastRenderedReducer = e;
  var r = se, l = r.baseQueue, i = n.pending;
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
      var x = d.lane;
      if ((Xt & x) === x) u !== null && (u = u.next = { lane: 0, action: d.action, hasEagerState: d.hasEagerState, eagerState: d.eagerState, next: null }), r = d.hasEagerState ? d.eagerState : e(r, d.action);
      else {
        var c = {
          lane: x,
          action: d.action,
          hasEagerState: d.hasEagerState,
          eagerState: d.eagerState,
          next: null
        };
        u === null ? (s = u = c, o = r) : u = u.next = c, ee.lanes |= x, qt |= x;
      }
      d = d.next;
    } while (d !== null && d !== i);
    u === null ? o = r : u.next = s, et(r, t.memoizedState) || (_e = !0), t.memoizedState = r, t.baseState = o, t.baseQueue = u, n.lastRenderedState = r;
  }
  if (e = n.interleaved, e !== null) {
    l = e;
    do
      i = l.lane, ee.lanes |= i, qt |= i, l = l.next;
    while (l !== e);
  } else l === null && (n.lanes = 0);
  return [t.memoizedState, n.dispatch];
}
function gi(e) {
  var t = Qe(), n = t.queue;
  if (n === null) throw Error(E(311));
  n.lastRenderedReducer = e;
  var r = n.dispatch, l = n.pending, i = t.memoizedState;
  if (l !== null) {
    n.pending = null;
    var o = l = l.next;
    do
      i = e(i, o.action), o = o.next;
    while (o !== l);
    et(i, t.memoizedState) || (_e = !0), t.memoizedState = i, t.baseQueue === null && (t.baseState = i), n.lastRenderedState = i;
  }
  return [i, r];
}
function Gu() {
}
function Yu(e, t) {
  var n = ee, r = Qe(), l = t(), i = !et(r.memoizedState, l);
  if (i && (r.memoizedState = l, _e = !0), r = r.queue, Xa(Zu.bind(null, n, r, e), [e]), r.getSnapshot !== t || i || de !== null && de.memoizedState.tag & 1) {
    if (n.flags |= 2048, xr(9, qu.bind(null, n, r, l, t), void 0, null), fe === null) throw Error(E(349));
    Xt & 30 || Xu(n, t, l);
  }
  return l;
}
function Xu(e, t, n) {
  e.flags |= 16384, e = { getSnapshot: t, value: n }, t = ee.updateQueue, t === null ? (t = { lastEffect: null, stores: null }, ee.updateQueue = t, t.stores = [e]) : (n = t.stores, n === null ? t.stores = [e] : n.push(e));
}
function qu(e, t, n, r) {
  t.value = n, t.getSnapshot = r, Ju(t) && bu(e);
}
function Zu(e, t, n) {
  return n(function() {
    Ju(t) && bu(e);
  });
}
function Ju(e) {
  var t = e.getSnapshot;
  e = e.value;
  try {
    var n = t();
    return !et(e, n);
  } catch {
    return !0;
  }
}
function bu(e) {
  var t = ht(e, 1);
  t !== null && be(t, e, 1, -1);
}
function as(e) {
  var t = nt();
  return typeof e == "function" && (e = e()), t.memoizedState = t.baseState = e, e = { pending: null, interleaved: null, lanes: 0, dispatch: null, lastRenderedReducer: gr, lastRenderedState: e }, t.queue = e, e = e.dispatch = bf.bind(null, ee, e), [t.memoizedState, e];
}
function xr(e, t, n, r) {
  return e = { tag: e, create: t, destroy: n, deps: r, next: null }, t = ee.updateQueue, t === null ? (t = { lastEffect: null, stores: null }, ee.updateQueue = t, t.lastEffect = e.next = e) : (n = t.lastEffect, n === null ? t.lastEffect = e.next = e : (r = n.next, n.next = e, e.next = r, t.lastEffect = e)), e;
}
function ec() {
  return Qe().memoizedState;
}
function Zr(e, t, n, r) {
  var l = nt();
  ee.flags |= e, l.memoizedState = xr(1 | t, n, void 0, r === void 0 ? null : r);
}
function Ll(e, t, n, r) {
  var l = Qe();
  r = r === void 0 ? null : r;
  var i = void 0;
  if (se !== null) {
    var o = se.memoizedState;
    if (i = o.destroy, r !== null && Ka(r, o.deps)) {
      l.memoizedState = xr(t, n, i, r);
      return;
    }
  }
  ee.flags |= e, l.memoizedState = xr(1 | t, n, i, r);
}
function os(e, t) {
  return Zr(8390656, 8, e, t);
}
function Xa(e, t) {
  return Ll(2048, 8, e, t);
}
function tc(e, t) {
  return Ll(4, 2, e, t);
}
function nc(e, t) {
  return Ll(4, 4, e, t);
}
function rc(e, t) {
  if (typeof t == "function") return e = e(), t(e), function() {
    t(null);
  };
  if (t != null) return e = e(), t.current = e, function() {
    t.current = null;
  };
}
function lc(e, t, n) {
  return n = n != null ? n.concat([e]) : null, Ll(4, 4, rc.bind(null, t, e), n);
}
function qa() {
}
function ic(e, t) {
  var n = Qe();
  t = t === void 0 ? null : t;
  var r = n.memoizedState;
  return r !== null && t !== null && Ka(t, r[1]) ? r[0] : (n.memoizedState = [e, t], e);
}
function ac(e, t) {
  var n = Qe();
  t = t === void 0 ? null : t;
  var r = n.memoizedState;
  return r !== null && t !== null && Ka(t, r[1]) ? r[0] : (e = e(), n.memoizedState = [e, t], e);
}
function oc(e, t, n) {
  return Xt & 21 ? (et(n, t) || (n = fu(), ee.lanes |= n, qt |= n, e.baseState = !0), t) : (e.baseState && (e.baseState = !1, _e = !0), e.memoizedState = n);
}
function Zf(e, t) {
  var n = H;
  H = n !== 0 && 4 > n ? n : 4, e(!0);
  var r = hi.transition;
  hi.transition = {};
  try {
    e(!1), t();
  } finally {
    H = n, hi.transition = r;
  }
}
function sc() {
  return Qe().memoizedState;
}
function Jf(e, t, n) {
  var r = Tt(e);
  if (n = { lane: r, action: n, hasEagerState: !1, eagerState: null, next: null }, uc(e)) cc(t, n);
  else if (n = Wu(e, t, n, r), n !== null) {
    var l = Ne();
    be(n, e, r, l), dc(n, t, r);
  }
}
function bf(e, t, n) {
  var r = Tt(e), l = { lane: r, action: n, hasEagerState: !1, eagerState: null, next: null };
  if (uc(e)) cc(t, l);
  else {
    var i = e.alternate;
    if (e.lanes === 0 && (i === null || i.lanes === 0) && (i = t.lastRenderedReducer, i !== null)) try {
      var o = t.lastRenderedState, s = i(o, n);
      if (l.hasEagerState = !0, l.eagerState = s, et(s, o)) {
        var u = t.interleaved;
        u === null ? (l.next = l, Va(t)) : (l.next = u.next, u.next = l), t.interleaved = l;
        return;
      }
    } catch {
    } finally {
    }
    n = Wu(e, t, l, r), n !== null && (l = Ne(), be(n, e, r, l), dc(n, t, r));
  }
}
function uc(e) {
  var t = e.alternate;
  return e === ee || t !== null && t === ee;
}
function cc(e, t) {
  Jn = yl = !0;
  var n = e.pending;
  n === null ? t.next = t : (t.next = n.next, n.next = t), e.pending = t;
}
function dc(e, t, n) {
  if (n & 4194240) {
    var r = t.lanes;
    r &= e.pendingLanes, n |= r, t.lanes = n, Pa(e, n);
  }
}
var jl = { readContext: We, useCallback: ve, useContext: ve, useEffect: ve, useImperativeHandle: ve, useInsertionEffect: ve, useLayoutEffect: ve, useMemo: ve, useReducer: ve, useRef: ve, useState: ve, useDebugValue: ve, useDeferredValue: ve, useTransition: ve, useMutableSource: ve, useSyncExternalStore: ve, useId: ve, unstable_isNewReconciler: !1 }, ep = { readContext: We, useCallback: function(e, t) {
  return nt().memoizedState = [e, t === void 0 ? null : t], e;
}, useContext: We, useEffect: os, useImperativeHandle: function(e, t, n) {
  return n = n != null ? n.concat([e]) : null, Zr(
    4194308,
    4,
    rc.bind(null, t, e),
    n
  );
}, useLayoutEffect: function(e, t) {
  return Zr(4194308, 4, e, t);
}, useInsertionEffect: function(e, t) {
  return Zr(4, 2, e, t);
}, useMemo: function(e, t) {
  var n = nt();
  return t = t === void 0 ? null : t, e = e(), n.memoizedState = [e, t], e;
}, useReducer: function(e, t, n) {
  var r = nt();
  return t = n !== void 0 ? n(t) : t, r.memoizedState = r.baseState = t, e = { pending: null, interleaved: null, lanes: 0, dispatch: null, lastRenderedReducer: e, lastRenderedState: t }, r.queue = e, e = e.dispatch = Jf.bind(null, ee, e), [r.memoizedState, e];
}, useRef: function(e) {
  var t = nt();
  return e = { current: e }, t.memoizedState = e;
}, useState: as, useDebugValue: qa, useDeferredValue: function(e) {
  return nt().memoizedState = e;
}, useTransition: function() {
  var e = as(!1), t = e[0];
  return e = Zf.bind(null, e[1]), nt().memoizedState = e, [t, e];
}, useMutableSource: function() {
}, useSyncExternalStore: function(e, t, n) {
  var r = ee, l = nt();
  if (J) {
    if (n === void 0) throw Error(E(407));
    n = n();
  } else {
    if (n = t(), fe === null) throw Error(E(349));
    Xt & 30 || Xu(r, t, n);
  }
  l.memoizedState = n;
  var i = { value: n, getSnapshot: t };
  return l.queue = i, os(Zu.bind(
    null,
    r,
    i,
    e
  ), [e]), r.flags |= 2048, xr(9, qu.bind(null, r, i, n, t), void 0, null), n;
}, useId: function() {
  var e = nt(), t = fe.identifierPrefix;
  if (J) {
    var n = dt, r = ct;
    n = (r & ~(1 << 32 - Je(r) - 1)).toString(32) + n, t = ":" + t + "R" + n, n = vr++, 0 < n && (t += "H" + n.toString(32)), t += ":";
  } else n = qf++, t = ":" + t + "r" + n.toString(32) + ":";
  return e.memoizedState = t;
}, unstable_isNewReconciler: !1 }, tp = {
  readContext: We,
  useCallback: ic,
  useContext: We,
  useEffect: Xa,
  useImperativeHandle: lc,
  useInsertionEffect: tc,
  useLayoutEffect: nc,
  useMemo: ac,
  useReducer: vi,
  useRef: ec,
  useState: function() {
    return vi(gr);
  },
  useDebugValue: qa,
  useDeferredValue: function(e) {
    var t = Qe();
    return oc(t, se.memoizedState, e);
  },
  useTransition: function() {
    var e = vi(gr)[0], t = Qe().memoizedState;
    return [e, t];
  },
  useMutableSource: Gu,
  useSyncExternalStore: Yu,
  useId: sc,
  unstable_isNewReconciler: !1
}, np = { readContext: We, useCallback: ic, useContext: We, useEffect: Xa, useImperativeHandle: lc, useInsertionEffect: tc, useLayoutEffect: nc, useMemo: ac, useReducer: gi, useRef: ec, useState: function() {
  return gi(gr);
}, useDebugValue: qa, useDeferredValue: function(e) {
  var t = Qe();
  return se === null ? t.memoizedState = e : oc(t, se.memoizedState, e);
}, useTransition: function() {
  var e = gi(gr)[0], t = Qe().memoizedState;
  return [e, t];
}, useMutableSource: Gu, useSyncExternalStore: Yu, useId: sc, unstable_isNewReconciler: !1 };
function Ye(e, t) {
  if (e && e.defaultProps) {
    t = te({}, t), e = e.defaultProps;
    for (var n in e) t[n] === void 0 && (t[n] = e[n]);
    return t;
  }
  return t;
}
function Ji(e, t, n, r) {
  t = e.memoizedState, n = n(r, t), n = n == null ? t : te({}, t, n), e.memoizedState = n, e.lanes === 0 && (e.updateQueue.baseState = n);
}
var Rl = { isMounted: function(e) {
  return (e = e._reactInternals) ? bt(e) === e : !1;
}, enqueueSetState: function(e, t, n) {
  e = e._reactInternals;
  var r = Ne(), l = Tt(e), i = ft(r, l);
  i.payload = t, n != null && (i.callback = n), t = It(e, i, l), t !== null && (be(t, e, l, r), Xr(t, e, l));
}, enqueueReplaceState: function(e, t, n) {
  e = e._reactInternals;
  var r = Ne(), l = Tt(e), i = ft(r, l);
  i.tag = 1, i.payload = t, n != null && (i.callback = n), t = It(e, i, l), t !== null && (be(t, e, l, r), Xr(t, e, l));
}, enqueueForceUpdate: function(e, t) {
  e = e._reactInternals;
  var n = Ne(), r = Tt(e), l = ft(n, r);
  l.tag = 2, t != null && (l.callback = t), t = It(e, l, r), t !== null && (be(t, e, r, n), Xr(t, e, r));
} };
function ss(e, t, n, r, l, i, o) {
  return e = e.stateNode, typeof e.shouldComponentUpdate == "function" ? e.shouldComponentUpdate(r, i, o) : t.prototype && t.prototype.isPureReactComponent ? !cr(n, r) || !cr(l, i) : !0;
}
function fc(e, t, n) {
  var r = !1, l = Rt, i = t.contextType;
  return typeof i == "object" && i !== null ? i = We(i) : (l = ze(t) ? Gt : je.current, r = t.contextTypes, i = (r = r != null) ? kn(e, l) : Rt), t = new t(n, i), e.memoizedState = t.state !== null && t.state !== void 0 ? t.state : null, t.updater = Rl, e.stateNode = t, t._reactInternals = e, r && (e = e.stateNode, e.__reactInternalMemoizedUnmaskedChildContext = l, e.__reactInternalMemoizedMaskedChildContext = i), t;
}
function us(e, t, n, r) {
  e = t.state, typeof t.componentWillReceiveProps == "function" && t.componentWillReceiveProps(n, r), typeof t.UNSAFE_componentWillReceiveProps == "function" && t.UNSAFE_componentWillReceiveProps(n, r), t.state !== e && Rl.enqueueReplaceState(t, t.state, null);
}
function bi(e, t, n, r) {
  var l = e.stateNode;
  l.props = n, l.state = e.memoizedState, l.refs = {}, Ba(e);
  var i = t.contextType;
  typeof i == "object" && i !== null ? l.context = We(i) : (i = ze(t) ? Gt : je.current, l.context = kn(e, i)), l.state = e.memoizedState, i = t.getDerivedStateFromProps, typeof i == "function" && (Ji(e, t, i, n), l.state = e.memoizedState), typeof t.getDerivedStateFromProps == "function" || typeof l.getSnapshotBeforeUpdate == "function" || typeof l.UNSAFE_componentWillMount != "function" && typeof l.componentWillMount != "function" || (t = l.state, typeof l.componentWillMount == "function" && l.componentWillMount(), typeof l.UNSAFE_componentWillMount == "function" && l.UNSAFE_componentWillMount(), t !== l.state && Rl.enqueueReplaceState(l, l.state, null), gl(e, n, l, r), l.state = e.memoizedState), typeof l.componentDidMount == "function" && (e.flags |= 4194308);
}
function En(e, t) {
  try {
    var n = "", r = t;
    do
      n += Id(r), r = r.return;
    while (r);
    var l = n;
  } catch (i) {
    l = `
Error generating stack: ` + i.message + `
` + i.stack;
  }
  return { value: e, source: t, stack: l, digest: null };
}
function xi(e, t, n) {
  return { value: e, source: null, stack: n ?? null, digest: t ?? null };
}
function ea(e, t) {
  try {
    console.error(t.value);
  } catch (n) {
    setTimeout(function() {
      throw n;
    });
  }
}
var rp = typeof WeakMap == "function" ? WeakMap : Map;
function pc(e, t, n) {
  n = ft(-1, n), n.tag = 3, n.payload = { element: null };
  var r = t.value;
  return n.callback = function() {
    kl || (kl = !0, ca = r), ea(e, t);
  }, n;
}
function mc(e, t, n) {
  n = ft(-1, n), n.tag = 3;
  var r = e.type.getDerivedStateFromError;
  if (typeof r == "function") {
    var l = t.value;
    n.payload = function() {
      return r(l);
    }, n.callback = function() {
      ea(e, t);
    };
  }
  var i = e.stateNode;
  return i !== null && typeof i.componentDidCatch == "function" && (n.callback = function() {
    ea(e, t), typeof r != "function" && (zt === null ? zt = /* @__PURE__ */ new Set([this]) : zt.add(this));
    var o = t.stack;
    this.componentDidCatch(t.value, { componentStack: o !== null ? o : "" });
  }), n;
}
function cs(e, t, n) {
  var r = e.pingCache;
  if (r === null) {
    r = e.pingCache = new rp();
    var l = /* @__PURE__ */ new Set();
    r.set(t, l);
  } else l = r.get(t), l === void 0 && (l = /* @__PURE__ */ new Set(), r.set(t, l));
  l.has(n) || (l.add(n), e = gp.bind(null, e, t, n), t.then(e, e));
}
function ds(e) {
  do {
    var t;
    if ((t = e.tag === 13) && (t = e.memoizedState, t = t !== null ? t.dehydrated !== null : !0), t) return e;
    e = e.return;
  } while (e !== null);
  return null;
}
function fs(e, t, n, r, l) {
  return e.mode & 1 ? (e.flags |= 65536, e.lanes = l, e) : (e === t ? e.flags |= 65536 : (e.flags |= 128, n.flags |= 131072, n.flags &= -52805, n.tag === 1 && (n.alternate === null ? n.tag = 17 : (t = ft(-1, 1), t.tag = 2, It(n, t, 1))), n.lanes |= 1), e);
}
var lp = gt.ReactCurrentOwner, _e = !1;
function ke(e, t, n, r) {
  t.child = e === null ? Hu(t, null, n, r) : Nn(t, e.child, n, r);
}
function ps(e, t, n, r, l) {
  n = n.render;
  var i = t.ref;
  return yn(t, l), r = Ga(e, t, n, r, i, l), n = Ya(), e !== null && !_e ? (t.updateQueue = e.updateQueue, t.flags &= -2053, e.lanes &= ~l, vt(e, t, l)) : (J && n && Da(t), t.flags |= 1, ke(e, t, r, l), t.child);
}
function ms(e, t, n, r, l) {
  if (e === null) {
    var i = n.type;
    return typeof i == "function" && !lo(i) && i.defaultProps === void 0 && n.compare === null && n.defaultProps === void 0 ? (t.tag = 15, t.type = i, hc(e, t, i, r, l)) : (e = tl(n.type, null, r, t, t.mode, l), e.ref = t.ref, e.return = t, t.child = e);
  }
  if (i = e.child, !(e.lanes & l)) {
    var o = i.memoizedProps;
    if (n = n.compare, n = n !== null ? n : cr, n(o, r) && e.ref === t.ref) return vt(e, t, l);
  }
  return t.flags |= 1, e = Ft(i, r), e.ref = t.ref, e.return = t, t.child = e;
}
function hc(e, t, n, r, l) {
  if (e !== null) {
    var i = e.memoizedProps;
    if (cr(i, r) && e.ref === t.ref) if (_e = !1, t.pendingProps = r = i, (e.lanes & l) !== 0) e.flags & 131072 && (_e = !0);
    else return t.lanes = e.lanes, vt(e, t, l);
  }
  return ta(e, t, n, r, l);
}
function vc(e, t, n) {
  var r = t.pendingProps, l = r.children, i = e !== null ? e.memoizedState : null;
  if (r.mode === "hidden") if (!(t.mode & 1)) t.memoizedState = { baseLanes: 0, cachePool: null, transitions: null }, Y(mn, Fe), Fe |= n;
  else {
    if (!(n & 1073741824)) return e = i !== null ? i.baseLanes | n : n, t.lanes = t.childLanes = 1073741824, t.memoizedState = { baseLanes: e, cachePool: null, transitions: null }, t.updateQueue = null, Y(mn, Fe), Fe |= e, null;
    t.memoizedState = { baseLanes: 0, cachePool: null, transitions: null }, r = i !== null ? i.baseLanes : n, Y(mn, Fe), Fe |= r;
  }
  else i !== null ? (r = i.baseLanes | n, t.memoizedState = null) : r = n, Y(mn, Fe), Fe |= r;
  return ke(e, t, l, n), t.child;
}
function gc(e, t) {
  var n = t.ref;
  (e === null && n !== null || e !== null && e.ref !== n) && (t.flags |= 512, t.flags |= 2097152);
}
function ta(e, t, n, r, l) {
  var i = ze(n) ? Gt : je.current;
  return i = kn(t, i), yn(t, l), n = Ga(e, t, n, r, i, l), r = Ya(), e !== null && !_e ? (t.updateQueue = e.updateQueue, t.flags &= -2053, e.lanes &= ~l, vt(e, t, l)) : (J && r && Da(t), t.flags |= 1, ke(e, t, n, l), t.child);
}
function hs(e, t, n, r, l) {
  if (ze(n)) {
    var i = !0;
    fl(t);
  } else i = !1;
  if (yn(t, l), t.stateNode === null) Jr(e, t), fc(t, n, r), bi(t, n, r, l), r = !0;
  else if (e === null) {
    var o = t.stateNode, s = t.memoizedProps;
    o.props = s;
    var u = o.context, d = n.contextType;
    typeof d == "object" && d !== null ? d = We(d) : (d = ze(n) ? Gt : je.current, d = kn(t, d));
    var x = n.getDerivedStateFromProps, c = typeof x == "function" || typeof o.getSnapshotBeforeUpdate == "function";
    c || typeof o.UNSAFE_componentWillReceiveProps != "function" && typeof o.componentWillReceiveProps != "function" || (s !== r || u !== d) && us(t, o, r, d), jt = !1;
    var m = t.memoizedState;
    o.state = m, gl(t, r, o, l), u = t.memoizedState, s !== r || m !== u || Ie.current || jt ? (typeof x == "function" && (Ji(t, n, x, r), u = t.memoizedState), (s = jt || ss(t, n, s, r, m, u, d)) ? (c || typeof o.UNSAFE_componentWillMount != "function" && typeof o.componentWillMount != "function" || (typeof o.componentWillMount == "function" && o.componentWillMount(), typeof o.UNSAFE_componentWillMount == "function" && o.UNSAFE_componentWillMount()), typeof o.componentDidMount == "function" && (t.flags |= 4194308)) : (typeof o.componentDidMount == "function" && (t.flags |= 4194308), t.memoizedProps = r, t.memoizedState = u), o.props = r, o.state = u, o.context = d, r = s) : (typeof o.componentDidMount == "function" && (t.flags |= 4194308), r = !1);
  } else {
    o = t.stateNode, Qu(e, t), s = t.memoizedProps, d = t.type === t.elementType ? s : Ye(t.type, s), o.props = d, c = t.pendingProps, m = o.context, u = n.contextType, typeof u == "object" && u !== null ? u = We(u) : (u = ze(n) ? Gt : je.current, u = kn(t, u));
    var v = n.getDerivedStateFromProps;
    (x = typeof v == "function" || typeof o.getSnapshotBeforeUpdate == "function") || typeof o.UNSAFE_componentWillReceiveProps != "function" && typeof o.componentWillReceiveProps != "function" || (s !== c || m !== u) && us(t, o, r, u), jt = !1, m = t.memoizedState, o.state = m, gl(t, r, o, l);
    var g = t.memoizedState;
    s !== c || m !== g || Ie.current || jt ? (typeof v == "function" && (Ji(t, n, v, r), g = t.memoizedState), (d = jt || ss(t, n, d, r, m, g, u) || !1) ? (x || typeof o.UNSAFE_componentWillUpdate != "function" && typeof o.componentWillUpdate != "function" || (typeof o.componentWillUpdate == "function" && o.componentWillUpdate(r, g, u), typeof o.UNSAFE_componentWillUpdate == "function" && o.UNSAFE_componentWillUpdate(r, g, u)), typeof o.componentDidUpdate == "function" && (t.flags |= 4), typeof o.getSnapshotBeforeUpdate == "function" && (t.flags |= 1024)) : (typeof o.componentDidUpdate != "function" || s === e.memoizedProps && m === e.memoizedState || (t.flags |= 4), typeof o.getSnapshotBeforeUpdate != "function" || s === e.memoizedProps && m === e.memoizedState || (t.flags |= 1024), t.memoizedProps = r, t.memoizedState = g), o.props = r, o.state = g, o.context = u, r = d) : (typeof o.componentDidUpdate != "function" || s === e.memoizedProps && m === e.memoizedState || (t.flags |= 4), typeof o.getSnapshotBeforeUpdate != "function" || s === e.memoizedProps && m === e.memoizedState || (t.flags |= 1024), r = !1);
  }
  return na(e, t, n, r, i, l);
}
function na(e, t, n, r, l, i) {
  gc(e, t);
  var o = (t.flags & 128) !== 0;
  if (!r && !o) return l && es(t, n, !1), vt(e, t, i);
  r = t.stateNode, lp.current = t;
  var s = o && typeof n.getDerivedStateFromError != "function" ? null : r.render();
  return t.flags |= 1, e !== null && o ? (t.child = Nn(t, e.child, null, i), t.child = Nn(t, null, s, i)) : ke(e, t, s, i), t.memoizedState = r.state, l && es(t, n, !0), t.child;
}
function xc(e) {
  var t = e.stateNode;
  t.pendingContext ? bo(e, t.pendingContext, t.pendingContext !== t.context) : t.context && bo(e, t.context, !1), Ha(e, t.containerInfo);
}
function vs(e, t, n, r, l) {
  return Sn(), Oa(l), t.flags |= 256, ke(e, t, n, r), t.child;
}
var ra = { dehydrated: null, treeContext: null, retryLane: 0 };
function la(e) {
  return { baseLanes: e, cachePool: null, transitions: null };
}
function yc(e, t, n) {
  var r = t.pendingProps, l = b.current, i = !1, o = (t.flags & 128) !== 0, s;
  if ((s = o) || (s = e !== null && e.memoizedState === null ? !1 : (l & 2) !== 0), s ? (i = !0, t.flags &= -129) : (e === null || e.memoizedState !== null) && (l |= 1), Y(b, l & 1), e === null)
    return qi(t), e = t.memoizedState, e !== null && (e = e.dehydrated, e !== null) ? (t.mode & 1 ? e.data === "$!" ? t.lanes = 8 : t.lanes = 1073741824 : t.lanes = 1, null) : (o = r.children, e = r.fallback, i ? (r = t.mode, i = t.child, o = { mode: "hidden", children: o }, !(r & 1) && i !== null ? (i.childLanes = 0, i.pendingProps = o) : i = Ol(o, r, 0, null), e = Kt(e, r, n, null), i.return = t, e.return = t, i.sibling = e, t.child = i, t.child.memoizedState = la(n), t.memoizedState = ra, e) : Za(t, o));
  if (l = e.memoizedState, l !== null && (s = l.dehydrated, s !== null)) return ip(e, t, o, r, s, l, n);
  if (i) {
    i = r.fallback, o = t.mode, l = e.child, s = l.sibling;
    var u = { mode: "hidden", children: r.children };
    return !(o & 1) && t.child !== l ? (r = t.child, r.childLanes = 0, r.pendingProps = u, t.deletions = null) : (r = Ft(l, u), r.subtreeFlags = l.subtreeFlags & 14680064), s !== null ? i = Ft(s, i) : (i = Kt(i, o, n, null), i.flags |= 2), i.return = t, r.return = t, r.sibling = i, t.child = r, r = i, i = t.child, o = e.child.memoizedState, o = o === null ? la(n) : { baseLanes: o.baseLanes | n, cachePool: null, transitions: o.transitions }, i.memoizedState = o, i.childLanes = e.childLanes & ~n, t.memoizedState = ra, r;
  }
  return i = e.child, e = i.sibling, r = Ft(i, { mode: "visible", children: r.children }), !(t.mode & 1) && (r.lanes = n), r.return = t, r.sibling = null, e !== null && (n = t.deletions, n === null ? (t.deletions = [e], t.flags |= 16) : n.push(e)), t.child = r, t.memoizedState = null, r;
}
function Za(e, t) {
  return t = Ol({ mode: "visible", children: t }, e.mode, 0, null), t.return = e, e.child = t;
}
function Ar(e, t, n, r) {
  return r !== null && Oa(r), Nn(t, e.child, null, n), e = Za(t, t.pendingProps.children), e.flags |= 2, t.memoizedState = null, e;
}
function ip(e, t, n, r, l, i, o) {
  if (n)
    return t.flags & 256 ? (t.flags &= -257, r = xi(Error(E(422))), Ar(e, t, o, r)) : t.memoizedState !== null ? (t.child = e.child, t.flags |= 128, null) : (i = r.fallback, l = t.mode, r = Ol({ mode: "visible", children: r.children }, l, 0, null), i = Kt(i, l, o, null), i.flags |= 2, r.return = t, i.return = t, r.sibling = i, t.child = r, t.mode & 1 && Nn(t, e.child, null, o), t.child.memoizedState = la(o), t.memoizedState = ra, i);
  if (!(t.mode & 1)) return Ar(e, t, o, null);
  if (l.data === "$!") {
    if (r = l.nextSibling && l.nextSibling.dataset, r) var s = r.dgst;
    return r = s, i = Error(E(419)), r = xi(i, r, void 0), Ar(e, t, o, r);
  }
  if (s = (o & e.childLanes) !== 0, _e || s) {
    if (r = fe, r !== null) {
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
      l = l & (r.suspendedLanes | o) ? 0 : l, l !== 0 && l !== i.retryLane && (i.retryLane = l, ht(e, l), be(r, e, l, -1));
    }
    return ro(), r = xi(Error(E(421))), Ar(e, t, o, r);
  }
  return l.data === "$?" ? (t.flags |= 128, t.child = e.child, t = xp.bind(null, e), l._reactRetry = t, null) : (e = i.treeContext, Le = _t(l.nextSibling), Re = t, J = !0, qe = null, e !== null && (Ue[Ae++] = ct, Ue[Ae++] = dt, Ue[Ae++] = Yt, ct = e.id, dt = e.overflow, Yt = t), t = Za(t, r.children), t.flags |= 4096, t);
}
function gs(e, t, n) {
  e.lanes |= t;
  var r = e.alternate;
  r !== null && (r.lanes |= t), Zi(e.return, t, n);
}
function yi(e, t, n, r, l) {
  var i = e.memoizedState;
  i === null ? e.memoizedState = { isBackwards: t, rendering: null, renderingStartTime: 0, last: r, tail: n, tailMode: l } : (i.isBackwards = t, i.rendering = null, i.renderingStartTime = 0, i.last = r, i.tail = n, i.tailMode = l);
}
function jc(e, t, n) {
  var r = t.pendingProps, l = r.revealOrder, i = r.tail;
  if (ke(e, t, r.children, n), r = b.current, r & 2) r = r & 1 | 2, t.flags |= 128;
  else {
    if (e !== null && e.flags & 128) e: for (e = t.child; e !== null; ) {
      if (e.tag === 13) e.memoizedState !== null && gs(e, n, t);
      else if (e.tag === 19) gs(e, n, t);
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
  if (Y(b, r), !(t.mode & 1)) t.memoizedState = null;
  else switch (l) {
    case "forwards":
      for (n = t.child, l = null; n !== null; ) e = n.alternate, e !== null && xl(e) === null && (l = n), n = n.sibling;
      n = l, n === null ? (l = t.child, t.child = null) : (l = n.sibling, n.sibling = null), yi(t, !1, l, n, i);
      break;
    case "backwards":
      for (n = null, l = t.child, t.child = null; l !== null; ) {
        if (e = l.alternate, e !== null && xl(e) === null) {
          t.child = l;
          break;
        }
        e = l.sibling, l.sibling = n, n = l, l = e;
      }
      yi(t, !0, n, null, i);
      break;
    case "together":
      yi(t, !1, null, null, void 0);
      break;
    default:
      t.memoizedState = null;
  }
  return t.child;
}
function Jr(e, t) {
  !(t.mode & 1) && e !== null && (e.alternate = null, t.alternate = null, t.flags |= 2);
}
function vt(e, t, n) {
  if (e !== null && (t.dependencies = e.dependencies), qt |= t.lanes, !(n & t.childLanes)) return null;
  if (e !== null && t.child !== e.child) throw Error(E(153));
  if (t.child !== null) {
    for (e = t.child, n = Ft(e, e.pendingProps), t.child = n, n.return = t; e.sibling !== null; ) e = e.sibling, n = n.sibling = Ft(e, e.pendingProps), n.return = t;
    n.sibling = null;
  }
  return t.child;
}
function ap(e, t, n) {
  switch (t.tag) {
    case 3:
      xc(t), Sn();
      break;
    case 5:
      Ku(t);
      break;
    case 1:
      ze(t.type) && fl(t);
      break;
    case 4:
      Ha(t, t.stateNode.containerInfo);
      break;
    case 10:
      var r = t.type._context, l = t.memoizedProps.value;
      Y(hl, r._currentValue), r._currentValue = l;
      break;
    case 13:
      if (r = t.memoizedState, r !== null)
        return r.dehydrated !== null ? (Y(b, b.current & 1), t.flags |= 128, null) : n & t.child.childLanes ? yc(e, t, n) : (Y(b, b.current & 1), e = vt(e, t, n), e !== null ? e.sibling : null);
      Y(b, b.current & 1);
      break;
    case 19:
      if (r = (n & t.childLanes) !== 0, e.flags & 128) {
        if (r) return jc(e, t, n);
        t.flags |= 128;
      }
      if (l = t.memoizedState, l !== null && (l.rendering = null, l.tail = null, l.lastEffect = null), Y(b, b.current), r) break;
      return null;
    case 22:
    case 23:
      return t.lanes = 0, vc(e, t, n);
  }
  return vt(e, t, n);
}
var wc, ia, kc, Sc;
wc = function(e, t) {
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
ia = function() {
};
kc = function(e, t, n, r) {
  var l = e.memoizedProps;
  if (l !== r) {
    e = t.stateNode, Wt(it.current);
    var i = null;
    switch (n) {
      case "input":
        l = Pi(e, l), r = Pi(e, r), i = [];
        break;
      case "select":
        l = te({}, l, { value: void 0 }), r = te({}, r, { value: void 0 }), i = [];
        break;
      case "textarea":
        l = zi(e, l), r = zi(e, r), i = [];
        break;
      default:
        typeof l.onClick != "function" && typeof r.onClick == "function" && (e.onclick = cl);
    }
    Fi(n, r);
    var o;
    n = null;
    for (d in l) if (!r.hasOwnProperty(d) && l.hasOwnProperty(d) && l[d] != null) if (d === "style") {
      var s = l[d];
      for (o in s) s.hasOwnProperty(o) && (n || (n = {}), n[o] = "");
    } else d !== "dangerouslySetInnerHTML" && d !== "children" && d !== "suppressContentEditableWarning" && d !== "suppressHydrationWarning" && d !== "autoFocus" && (rr.hasOwnProperty(d) ? i || (i = []) : (i = i || []).push(d, null));
    for (d in r) {
      var u = r[d];
      if (s = l != null ? l[d] : void 0, r.hasOwnProperty(d) && u !== s && (u != null || s != null)) if (d === "style") if (s) {
        for (o in s) !s.hasOwnProperty(o) || u && u.hasOwnProperty(o) || (n || (n = {}), n[o] = "");
        for (o in u) u.hasOwnProperty(o) && s[o] !== u[o] && (n || (n = {}), n[o] = u[o]);
      } else n || (i || (i = []), i.push(
        d,
        n
      )), n = u;
      else d === "dangerouslySetInnerHTML" ? (u = u ? u.__html : void 0, s = s ? s.__html : void 0, u != null && s !== u && (i = i || []).push(d, u)) : d === "children" ? typeof u != "string" && typeof u != "number" || (i = i || []).push(d, "" + u) : d !== "suppressContentEditableWarning" && d !== "suppressHydrationWarning" && (rr.hasOwnProperty(d) ? (u != null && d === "onScroll" && X("scroll", e), i || s === u || (i = [])) : (i = i || []).push(d, u));
    }
    n && (i = i || []).push("style", n);
    var d = i;
    (t.updateQueue = d) && (t.flags |= 4);
  }
};
Sc = function(e, t, n, r) {
  n !== r && (t.flags |= 4);
};
function Vn(e, t) {
  if (!J) switch (e.tailMode) {
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
function ge(e) {
  var t = e.alternate !== null && e.alternate.child === e.child, n = 0, r = 0;
  if (t) for (var l = e.child; l !== null; ) n |= l.lanes | l.childLanes, r |= l.subtreeFlags & 14680064, r |= l.flags & 14680064, l.return = e, l = l.sibling;
  else for (l = e.child; l !== null; ) n |= l.lanes | l.childLanes, r |= l.subtreeFlags, r |= l.flags, l.return = e, l = l.sibling;
  return e.subtreeFlags |= r, e.childLanes = n, t;
}
function op(e, t, n) {
  var r = t.pendingProps;
  switch (Ma(t), t.tag) {
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
      return ge(t), null;
    case 1:
      return ze(t.type) && dl(), ge(t), null;
    case 3:
      return r = t.stateNode, Cn(), q(Ie), q(je), Qa(), r.pendingContext && (r.context = r.pendingContext, r.pendingContext = null), (e === null || e.child === null) && ($r(t) ? t.flags |= 4 : e === null || e.memoizedState.isDehydrated && !(t.flags & 256) || (t.flags |= 1024, qe !== null && (pa(qe), qe = null))), ia(e, t), ge(t), null;
    case 5:
      Wa(t);
      var l = Wt(hr.current);
      if (n = t.type, e !== null && t.stateNode != null) kc(e, t, n, r, l), e.ref !== t.ref && (t.flags |= 512, t.flags |= 2097152);
      else {
        if (!r) {
          if (t.stateNode === null) throw Error(E(166));
          return ge(t), null;
        }
        if (e = Wt(it.current), $r(t)) {
          r = t.stateNode, n = t.type;
          var i = t.memoizedProps;
          switch (r[rt] = t, r[pr] = i, e = (t.mode & 1) !== 0, n) {
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
              for (l = 0; l < Kn.length; l++) X(Kn[l], r);
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
              Eo(r, i), X("invalid", r);
              break;
            case "select":
              r._wrapperState = { wasMultiple: !!i.multiple }, X("invalid", r);
              break;
            case "textarea":
              _o(r, i), X("invalid", r);
          }
          Fi(n, i), l = null;
          for (var o in i) if (i.hasOwnProperty(o)) {
            var s = i[o];
            o === "children" ? typeof s == "string" ? r.textContent !== s && (i.suppressHydrationWarning !== !0 && Or(r.textContent, s, e), l = ["children", s]) : typeof s == "number" && r.textContent !== "" + s && (i.suppressHydrationWarning !== !0 && Or(
              r.textContent,
              s,
              e
            ), l = ["children", "" + s]) : rr.hasOwnProperty(o) && s != null && o === "onScroll" && X("scroll", r);
          }
          switch (n) {
            case "input":
              Ir(r), Po(r, i, !0);
              break;
            case "textarea":
              Ir(r), Io(r);
              break;
            case "select":
            case "option":
              break;
            default:
              typeof i.onClick == "function" && (r.onclick = cl);
          }
          r = l, t.updateQueue = r, r !== null && (t.flags |= 4);
        } else {
          o = l.nodeType === 9 ? l : l.ownerDocument, e === "http://www.w3.org/1999/xhtml" && (e = Zs(n)), e === "http://www.w3.org/1999/xhtml" ? n === "script" ? (e = o.createElement("div"), e.innerHTML = "<script><\/script>", e = e.removeChild(e.firstChild)) : typeof r.is == "string" ? e = o.createElement(n, { is: r.is }) : (e = o.createElement(n), n === "select" && (o = e, r.multiple ? o.multiple = !0 : r.size && (o.size = r.size))) : e = o.createElementNS(e, n), e[rt] = t, e[pr] = r, wc(e, t, !1, !1), t.stateNode = e;
          e: {
            switch (o = Li(n, r), n) {
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
                for (l = 0; l < Kn.length; l++) X(Kn[l], e);
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
                Eo(e, r), l = Pi(e, r), X("invalid", e);
                break;
              case "option":
                l = r;
                break;
              case "select":
                e._wrapperState = { wasMultiple: !!r.multiple }, l = te({}, r, { value: void 0 }), X("invalid", e);
                break;
              case "textarea":
                _o(e, r), l = zi(e, r), X("invalid", e);
                break;
              default:
                l = r;
            }
            Fi(n, l), s = l;
            for (i in s) if (s.hasOwnProperty(i)) {
              var u = s[i];
              i === "style" ? eu(e, u) : i === "dangerouslySetInnerHTML" ? (u = u ? u.__html : void 0, u != null && Js(e, u)) : i === "children" ? typeof u == "string" ? (n !== "textarea" || u !== "") && lr(e, u) : typeof u == "number" && lr(e, "" + u) : i !== "suppressContentEditableWarning" && i !== "suppressHydrationWarning" && i !== "autoFocus" && (rr.hasOwnProperty(i) ? u != null && i === "onScroll" && X("scroll", e) : u != null && wa(e, i, u, o));
            }
            switch (n) {
              case "input":
                Ir(e), Po(e, r, !1);
                break;
              case "textarea":
                Ir(e), Io(e);
                break;
              case "option":
                r.value != null && e.setAttribute("value", "" + Lt(r.value));
                break;
              case "select":
                e.multiple = !!r.multiple, i = r.value, i != null ? hn(e, !!r.multiple, i, !1) : r.defaultValue != null && hn(
                  e,
                  !!r.multiple,
                  r.defaultValue,
                  !0
                );
                break;
              default:
                typeof l.onClick == "function" && (e.onclick = cl);
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
      return ge(t), null;
    case 6:
      if (e && t.stateNode != null) Sc(e, t, e.memoizedProps, r);
      else {
        if (typeof r != "string" && t.stateNode === null) throw Error(E(166));
        if (n = Wt(hr.current), Wt(it.current), $r(t)) {
          if (r = t.stateNode, n = t.memoizedProps, r[rt] = t, (i = r.nodeValue !== n) && (e = Re, e !== null)) switch (e.tag) {
            case 3:
              Or(r.nodeValue, n, (e.mode & 1) !== 0);
              break;
            case 5:
              e.memoizedProps.suppressHydrationWarning !== !0 && Or(r.nodeValue, n, (e.mode & 1) !== 0);
          }
          i && (t.flags |= 4);
        } else r = (n.nodeType === 9 ? n : n.ownerDocument).createTextNode(r), r[rt] = t, t.stateNode = r;
      }
      return ge(t), null;
    case 13:
      if (q(b), r = t.memoizedState, e === null || e.memoizedState !== null && e.memoizedState.dehydrated !== null) {
        if (J && Le !== null && t.mode & 1 && !(t.flags & 128)) Vu(), Sn(), t.flags |= 98560, i = !1;
        else if (i = $r(t), r !== null && r.dehydrated !== null) {
          if (e === null) {
            if (!i) throw Error(E(318));
            if (i = t.memoizedState, i = i !== null ? i.dehydrated : null, !i) throw Error(E(317));
            i[rt] = t;
          } else Sn(), !(t.flags & 128) && (t.memoizedState = null), t.flags |= 4;
          ge(t), i = !1;
        } else qe !== null && (pa(qe), qe = null), i = !0;
        if (!i) return t.flags & 65536 ? t : null;
      }
      return t.flags & 128 ? (t.lanes = n, t) : (r = r !== null, r !== (e !== null && e.memoizedState !== null) && r && (t.child.flags |= 8192, t.mode & 1 && (e === null || b.current & 1 ? ue === 0 && (ue = 3) : ro())), t.updateQueue !== null && (t.flags |= 4), ge(t), null);
    case 4:
      return Cn(), ia(e, t), e === null && dr(t.stateNode.containerInfo), ge(t), null;
    case 10:
      return Aa(t.type._context), ge(t), null;
    case 17:
      return ze(t.type) && dl(), ge(t), null;
    case 19:
      if (q(b), i = t.memoizedState, i === null) return ge(t), null;
      if (r = (t.flags & 128) !== 0, o = i.rendering, o === null) if (r) Vn(i, !1);
      else {
        if (ue !== 0 || e !== null && e.flags & 128) for (e = t.child; e !== null; ) {
          if (o = xl(e), o !== null) {
            for (t.flags |= 128, Vn(i, !1), r = o.updateQueue, r !== null && (t.updateQueue = r, t.flags |= 4), t.subtreeFlags = 0, r = n, n = t.child; n !== null; ) i = n, e = r, i.flags &= 14680066, o = i.alternate, o === null ? (i.childLanes = 0, i.lanes = e, i.child = null, i.subtreeFlags = 0, i.memoizedProps = null, i.memoizedState = null, i.updateQueue = null, i.dependencies = null, i.stateNode = null) : (i.childLanes = o.childLanes, i.lanes = o.lanes, i.child = o.child, i.subtreeFlags = 0, i.deletions = null, i.memoizedProps = o.memoizedProps, i.memoizedState = o.memoizedState, i.updateQueue = o.updateQueue, i.type = o.type, e = o.dependencies, i.dependencies = e === null ? null : { lanes: e.lanes, firstContext: e.firstContext }), n = n.sibling;
            return Y(b, b.current & 1 | 2), t.child;
          }
          e = e.sibling;
        }
        i.tail !== null && ie() > Pn && (t.flags |= 128, r = !0, Vn(i, !1), t.lanes = 4194304);
      }
      else {
        if (!r) if (e = xl(o), e !== null) {
          if (t.flags |= 128, r = !0, n = e.updateQueue, n !== null && (t.updateQueue = n, t.flags |= 4), Vn(i, !0), i.tail === null && i.tailMode === "hidden" && !o.alternate && !J) return ge(t), null;
        } else 2 * ie() - i.renderingStartTime > Pn && n !== 1073741824 && (t.flags |= 128, r = !0, Vn(i, !1), t.lanes = 4194304);
        i.isBackwards ? (o.sibling = t.child, t.child = o) : (n = i.last, n !== null ? n.sibling = o : t.child = o, i.last = o);
      }
      return i.tail !== null ? (t = i.tail, i.rendering = t, i.tail = t.sibling, i.renderingStartTime = ie(), t.sibling = null, n = b.current, Y(b, r ? n & 1 | 2 : n & 1), t) : (ge(t), null);
    case 22:
    case 23:
      return no(), r = t.memoizedState !== null, e !== null && e.memoizedState !== null !== r && (t.flags |= 8192), r && t.mode & 1 ? Fe & 1073741824 && (ge(t), t.subtreeFlags & 6 && (t.flags |= 8192)) : ge(t), null;
    case 24:
      return null;
    case 25:
      return null;
  }
  throw Error(E(156, t.tag));
}
function sp(e, t) {
  switch (Ma(t), t.tag) {
    case 1:
      return ze(t.type) && dl(), e = t.flags, e & 65536 ? (t.flags = e & -65537 | 128, t) : null;
    case 3:
      return Cn(), q(Ie), q(je), Qa(), e = t.flags, e & 65536 && !(e & 128) ? (t.flags = e & -65537 | 128, t) : null;
    case 5:
      return Wa(t), null;
    case 13:
      if (q(b), e = t.memoizedState, e !== null && e.dehydrated !== null) {
        if (t.alternate === null) throw Error(E(340));
        Sn();
      }
      return e = t.flags, e & 65536 ? (t.flags = e & -65537 | 128, t) : null;
    case 19:
      return q(b), null;
    case 4:
      return Cn(), null;
    case 10:
      return Aa(t.type._context), null;
    case 22:
    case 23:
      return no(), null;
    case 24:
      return null;
    default:
      return null;
  }
}
var Vr = !1, xe = !1, up = typeof WeakSet == "function" ? WeakSet : Set, D = null;
function pn(e, t) {
  var n = e.ref;
  if (n !== null) if (typeof n == "function") try {
    n(null);
  } catch (r) {
    le(e, t, r);
  }
  else n.current = null;
}
function aa(e, t, n) {
  try {
    n();
  } catch (r) {
    le(e, t, r);
  }
}
var xs = !1;
function cp(e, t) {
  if (Hi = ol, e = _u(), Ra(e)) {
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
        var o = 0, s = -1, u = -1, d = 0, x = 0, c = e, m = null;
        t: for (; ; ) {
          for (var v; c !== n || l !== 0 && c.nodeType !== 3 || (s = o + l), c !== i || r !== 0 && c.nodeType !== 3 || (u = o + r), c.nodeType === 3 && (o += c.nodeValue.length), (v = c.firstChild) !== null; )
            m = c, c = v;
          for (; ; ) {
            if (c === e) break t;
            if (m === n && ++d === l && (s = o), m === i && ++x === r && (u = o), (v = c.nextSibling) !== null) break;
            c = m, m = c.parentNode;
          }
          c = v;
        }
        n = s === -1 || u === -1 ? null : { start: s, end: u };
      } else n = null;
    }
    n = n || { start: 0, end: 0 };
  } else n = null;
  for (Wi = { focusedElem: e, selectionRange: n }, ol = !1, D = t; D !== null; ) if (t = D, e = t.child, (t.subtreeFlags & 1028) !== 0 && e !== null) e.return = t, D = e;
  else for (; D !== null; ) {
    t = D;
    try {
      var g = t.alternate;
      if (t.flags & 1024) switch (t.tag) {
        case 0:
        case 11:
        case 15:
          break;
        case 1:
          if (g !== null) {
            var k = g.memoizedProps, F = g.memoizedState, f = t.stateNode, p = f.getSnapshotBeforeUpdate(t.elementType === t.type ? k : Ye(t.type, k), F);
            f.__reactInternalSnapshotBeforeUpdate = p;
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
          throw Error(E(163));
      }
    } catch (w) {
      le(t, t.return, w);
    }
    if (e = t.sibling, e !== null) {
      e.return = t.return, D = e;
      break;
    }
    D = t.return;
  }
  return g = xs, xs = !1, g;
}
function bn(e, t, n) {
  var r = t.updateQueue;
  if (r = r !== null ? r.lastEffect : null, r !== null) {
    var l = r = r.next;
    do {
      if ((l.tag & e) === e) {
        var i = l.destroy;
        l.destroy = void 0, i !== void 0 && aa(t, n, i);
      }
      l = l.next;
    } while (l !== r);
  }
}
function Dl(e, t) {
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
function oa(e) {
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
function Nc(e) {
  var t = e.alternate;
  t !== null && (e.alternate = null, Nc(t)), e.child = null, e.deletions = null, e.sibling = null, e.tag === 5 && (t = e.stateNode, t !== null && (delete t[rt], delete t[pr], delete t[Gi], delete t[Kf], delete t[Gf])), e.stateNode = null, e.return = null, e.dependencies = null, e.memoizedProps = null, e.memoizedState = null, e.pendingProps = null, e.stateNode = null, e.updateQueue = null;
}
function Cc(e) {
  return e.tag === 5 || e.tag === 3 || e.tag === 4;
}
function ys(e) {
  e: for (; ; ) {
    for (; e.sibling === null; ) {
      if (e.return === null || Cc(e.return)) return null;
      e = e.return;
    }
    for (e.sibling.return = e.return, e = e.sibling; e.tag !== 5 && e.tag !== 6 && e.tag !== 18; ) {
      if (e.flags & 2 || e.child === null || e.tag === 4) continue e;
      e.child.return = e, e = e.child;
    }
    if (!(e.flags & 2)) return e.stateNode;
  }
}
function sa(e, t, n) {
  var r = e.tag;
  if (r === 5 || r === 6) e = e.stateNode, t ? n.nodeType === 8 ? n.parentNode.insertBefore(e, t) : n.insertBefore(e, t) : (n.nodeType === 8 ? (t = n.parentNode, t.insertBefore(e, n)) : (t = n, t.appendChild(e)), n = n._reactRootContainer, n != null || t.onclick !== null || (t.onclick = cl));
  else if (r !== 4 && (e = e.child, e !== null)) for (sa(e, t, n), e = e.sibling; e !== null; ) sa(e, t, n), e = e.sibling;
}
function ua(e, t, n) {
  var r = e.tag;
  if (r === 5 || r === 6) e = e.stateNode, t ? n.insertBefore(e, t) : n.appendChild(e);
  else if (r !== 4 && (e = e.child, e !== null)) for (ua(e, t, n), e = e.sibling; e !== null; ) ua(e, t, n), e = e.sibling;
}
var pe = null, Xe = !1;
function xt(e, t, n) {
  for (n = n.child; n !== null; ) Ec(e, t, n), n = n.sibling;
}
function Ec(e, t, n) {
  if (lt && typeof lt.onCommitFiberUnmount == "function") try {
    lt.onCommitFiberUnmount(Pl, n);
  } catch {
  }
  switch (n.tag) {
    case 5:
      xe || pn(n, t);
    case 6:
      var r = pe, l = Xe;
      pe = null, xt(e, t, n), pe = r, Xe = l, pe !== null && (Xe ? (e = pe, n = n.stateNode, e.nodeType === 8 ? e.parentNode.removeChild(n) : e.removeChild(n)) : pe.removeChild(n.stateNode));
      break;
    case 18:
      pe !== null && (Xe ? (e = pe, n = n.stateNode, e.nodeType === 8 ? fi(e.parentNode, n) : e.nodeType === 1 && fi(e, n), sr(e)) : fi(pe, n.stateNode));
      break;
    case 4:
      r = pe, l = Xe, pe = n.stateNode.containerInfo, Xe = !0, xt(e, t, n), pe = r, Xe = l;
      break;
    case 0:
    case 11:
    case 14:
    case 15:
      if (!xe && (r = n.updateQueue, r !== null && (r = r.lastEffect, r !== null))) {
        l = r = r.next;
        do {
          var i = l, o = i.destroy;
          i = i.tag, o !== void 0 && (i & 2 || i & 4) && aa(n, t, o), l = l.next;
        } while (l !== r);
      }
      xt(e, t, n);
      break;
    case 1:
      if (!xe && (pn(n, t), r = n.stateNode, typeof r.componentWillUnmount == "function")) try {
        r.props = n.memoizedProps, r.state = n.memoizedState, r.componentWillUnmount();
      } catch (s) {
        le(n, t, s);
      }
      xt(e, t, n);
      break;
    case 21:
      xt(e, t, n);
      break;
    case 22:
      n.mode & 1 ? (xe = (r = xe) || n.memoizedState !== null, xt(e, t, n), xe = r) : xt(e, t, n);
      break;
    default:
      xt(e, t, n);
  }
}
function js(e) {
  var t = e.updateQueue;
  if (t !== null) {
    e.updateQueue = null;
    var n = e.stateNode;
    n === null && (n = e.stateNode = new up()), t.forEach(function(r) {
      var l = yp.bind(null, e, r);
      n.has(r) || (n.add(r), r.then(l, l));
    });
  }
}
function Ge(e, t) {
  var n = t.deletions;
  if (n !== null) for (var r = 0; r < n.length; r++) {
    var l = n[r];
    try {
      var i = e, o = t, s = o;
      e: for (; s !== null; ) {
        switch (s.tag) {
          case 5:
            pe = s.stateNode, Xe = !1;
            break e;
          case 3:
            pe = s.stateNode.containerInfo, Xe = !0;
            break e;
          case 4:
            pe = s.stateNode.containerInfo, Xe = !0;
            break e;
        }
        s = s.return;
      }
      if (pe === null) throw Error(E(160));
      Ec(i, o, l), pe = null, Xe = !1;
      var u = l.alternate;
      u !== null && (u.return = null), l.return = null;
    } catch (d) {
      le(l, t, d);
    }
  }
  if (t.subtreeFlags & 12854) for (t = t.child; t !== null; ) Pc(t, e), t = t.sibling;
}
function Pc(e, t) {
  var n = e.alternate, r = e.flags;
  switch (e.tag) {
    case 0:
    case 11:
    case 14:
    case 15:
      if (Ge(t, e), tt(e), r & 4) {
        try {
          bn(3, e, e.return), Dl(3, e);
        } catch (k) {
          le(e, e.return, k);
        }
        try {
          bn(5, e, e.return);
        } catch (k) {
          le(e, e.return, k);
        }
      }
      break;
    case 1:
      Ge(t, e), tt(e), r & 512 && n !== null && pn(n, n.return);
      break;
    case 5:
      if (Ge(t, e), tt(e), r & 512 && n !== null && pn(n, n.return), e.flags & 32) {
        var l = e.stateNode;
        try {
          lr(l, "");
        } catch (k) {
          le(e, e.return, k);
        }
      }
      if (r & 4 && (l = e.stateNode, l != null)) {
        var i = e.memoizedProps, o = n !== null ? n.memoizedProps : i, s = e.type, u = e.updateQueue;
        if (e.updateQueue = null, u !== null) try {
          s === "input" && i.type === "radio" && i.name != null && Xs(l, i), Li(s, o);
          var d = Li(s, i);
          for (o = 0; o < u.length; o += 2) {
            var x = u[o], c = u[o + 1];
            x === "style" ? eu(l, c) : x === "dangerouslySetInnerHTML" ? Js(l, c) : x === "children" ? lr(l, c) : wa(l, x, c, d);
          }
          switch (s) {
            case "input":
              _i(l, i);
              break;
            case "textarea":
              qs(l, i);
              break;
            case "select":
              var m = l._wrapperState.wasMultiple;
              l._wrapperState.wasMultiple = !!i.multiple;
              var v = i.value;
              v != null ? hn(l, !!i.multiple, v, !1) : m !== !!i.multiple && (i.defaultValue != null ? hn(
                l,
                !!i.multiple,
                i.defaultValue,
                !0
              ) : hn(l, !!i.multiple, i.multiple ? [] : "", !1));
          }
          l[pr] = i;
        } catch (k) {
          le(e, e.return, k);
        }
      }
      break;
    case 6:
      if (Ge(t, e), tt(e), r & 4) {
        if (e.stateNode === null) throw Error(E(162));
        l = e.stateNode, i = e.memoizedProps;
        try {
          l.nodeValue = i;
        } catch (k) {
          le(e, e.return, k);
        }
      }
      break;
    case 3:
      if (Ge(t, e), tt(e), r & 4 && n !== null && n.memoizedState.isDehydrated) try {
        sr(t.containerInfo);
      } catch (k) {
        le(e, e.return, k);
      }
      break;
    case 4:
      Ge(t, e), tt(e);
      break;
    case 13:
      Ge(t, e), tt(e), l = e.child, l.flags & 8192 && (i = l.memoizedState !== null, l.stateNode.isHidden = i, !i || l.alternate !== null && l.alternate.memoizedState !== null || (eo = ie())), r & 4 && js(e);
      break;
    case 22:
      if (x = n !== null && n.memoizedState !== null, e.mode & 1 ? (xe = (d = xe) || x, Ge(t, e), xe = d) : Ge(t, e), tt(e), r & 8192) {
        if (d = e.memoizedState !== null, (e.stateNode.isHidden = d) && !x && e.mode & 1) for (D = e, x = e.child; x !== null; ) {
          for (c = D = x; D !== null; ) {
            switch (m = D, v = m.child, m.tag) {
              case 0:
              case 11:
              case 14:
              case 15:
                bn(4, m, m.return);
                break;
              case 1:
                pn(m, m.return);
                var g = m.stateNode;
                if (typeof g.componentWillUnmount == "function") {
                  r = m, n = m.return;
                  try {
                    t = r, g.props = t.memoizedProps, g.state = t.memoizedState, g.componentWillUnmount();
                  } catch (k) {
                    le(r, n, k);
                  }
                }
                break;
              case 5:
                pn(m, m.return);
                break;
              case 22:
                if (m.memoizedState !== null) {
                  ks(c);
                  continue;
                }
            }
            v !== null ? (v.return = m, D = v) : ks(c);
          }
          x = x.sibling;
        }
        e: for (x = null, c = e; ; ) {
          if (c.tag === 5) {
            if (x === null) {
              x = c;
              try {
                l = c.stateNode, d ? (i = l.style, typeof i.setProperty == "function" ? i.setProperty("display", "none", "important") : i.display = "none") : (s = c.stateNode, u = c.memoizedProps.style, o = u != null && u.hasOwnProperty("display") ? u.display : null, s.style.display = bs("display", o));
              } catch (k) {
                le(e, e.return, k);
              }
            }
          } else if (c.tag === 6) {
            if (x === null) try {
              c.stateNode.nodeValue = d ? "" : c.memoizedProps;
            } catch (k) {
              le(e, e.return, k);
            }
          } else if ((c.tag !== 22 && c.tag !== 23 || c.memoizedState === null || c === e) && c.child !== null) {
            c.child.return = c, c = c.child;
            continue;
          }
          if (c === e) break e;
          for (; c.sibling === null; ) {
            if (c.return === null || c.return === e) break e;
            x === c && (x = null), c = c.return;
          }
          x === c && (x = null), c.sibling.return = c.return, c = c.sibling;
        }
      }
      break;
    case 19:
      Ge(t, e), tt(e), r & 4 && js(e);
      break;
    case 21:
      break;
    default:
      Ge(
        t,
        e
      ), tt(e);
  }
}
function tt(e) {
  var t = e.flags;
  if (t & 2) {
    try {
      e: {
        for (var n = e.return; n !== null; ) {
          if (Cc(n)) {
            var r = n;
            break e;
          }
          n = n.return;
        }
        throw Error(E(160));
      }
      switch (r.tag) {
        case 5:
          var l = r.stateNode;
          r.flags & 32 && (lr(l, ""), r.flags &= -33);
          var i = ys(e);
          ua(e, i, l);
          break;
        case 3:
        case 4:
          var o = r.stateNode.containerInfo, s = ys(e);
          sa(e, s, o);
          break;
        default:
          throw Error(E(161));
      }
    } catch (u) {
      le(e, e.return, u);
    }
    e.flags &= -3;
  }
  t & 4096 && (e.flags &= -4097);
}
function dp(e, t, n) {
  D = e, _c(e);
}
function _c(e, t, n) {
  for (var r = (e.mode & 1) !== 0; D !== null; ) {
    var l = D, i = l.child;
    if (l.tag === 22 && r) {
      var o = l.memoizedState !== null || Vr;
      if (!o) {
        var s = l.alternate, u = s !== null && s.memoizedState !== null || xe;
        s = Vr;
        var d = xe;
        if (Vr = o, (xe = u) && !d) for (D = l; D !== null; ) o = D, u = o.child, o.tag === 22 && o.memoizedState !== null ? Ss(l) : u !== null ? (u.return = o, D = u) : Ss(l);
        for (; i !== null; ) D = i, _c(i), i = i.sibling;
        D = l, Vr = s, xe = d;
      }
      ws(e);
    } else l.subtreeFlags & 8772 && i !== null ? (i.return = l, D = i) : ws(e);
  }
}
function ws(e) {
  for (; D !== null; ) {
    var t = D;
    if (t.flags & 8772) {
      var n = t.alternate;
      try {
        if (t.flags & 8772) switch (t.tag) {
          case 0:
          case 11:
          case 15:
            xe || Dl(5, t);
            break;
          case 1:
            var r = t.stateNode;
            if (t.flags & 4 && !xe) if (n === null) r.componentDidMount();
            else {
              var l = t.elementType === t.type ? n.memoizedProps : Ye(t.type, n.memoizedProps);
              r.componentDidUpdate(l, n.memoizedState, r.__reactInternalSnapshotBeforeUpdate);
            }
            var i = t.updateQueue;
            i !== null && is(t, i, r);
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
              is(t, o, n);
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
                var x = d.memoizedState;
                if (x !== null) {
                  var c = x.dehydrated;
                  c !== null && sr(c);
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
            throw Error(E(163));
        }
        xe || t.flags & 512 && oa(t);
      } catch (m) {
        le(t, t.return, m);
      }
    }
    if (t === e) {
      D = null;
      break;
    }
    if (n = t.sibling, n !== null) {
      n.return = t.return, D = n;
      break;
    }
    D = t.return;
  }
}
function ks(e) {
  for (; D !== null; ) {
    var t = D;
    if (t === e) {
      D = null;
      break;
    }
    var n = t.sibling;
    if (n !== null) {
      n.return = t.return, D = n;
      break;
    }
    D = t.return;
  }
}
function Ss(e) {
  for (; D !== null; ) {
    var t = D;
    try {
      switch (t.tag) {
        case 0:
        case 11:
        case 15:
          var n = t.return;
          try {
            Dl(4, t);
          } catch (u) {
            le(t, n, u);
          }
          break;
        case 1:
          var r = t.stateNode;
          if (typeof r.componentDidMount == "function") {
            var l = t.return;
            try {
              r.componentDidMount();
            } catch (u) {
              le(t, l, u);
            }
          }
          var i = t.return;
          try {
            oa(t);
          } catch (u) {
            le(t, i, u);
          }
          break;
        case 5:
          var o = t.return;
          try {
            oa(t);
          } catch (u) {
            le(t, o, u);
          }
      }
    } catch (u) {
      le(t, t.return, u);
    }
    if (t === e) {
      D = null;
      break;
    }
    var s = t.sibling;
    if (s !== null) {
      s.return = t.return, D = s;
      break;
    }
    D = t.return;
  }
}
var fp = Math.ceil, wl = gt.ReactCurrentDispatcher, Ja = gt.ReactCurrentOwner, He = gt.ReactCurrentBatchConfig, B = 0, fe = null, oe = null, me = 0, Fe = 0, mn = Mt(0), ue = 0, yr = null, qt = 0, Ml = 0, ba = 0, er = null, Pe = null, eo = 0, Pn = 1 / 0, st = null, kl = !1, ca = null, zt = null, Br = !1, Nt = null, Sl = 0, tr = 0, da = null, br = -1, el = 0;
function Ne() {
  return B & 6 ? ie() : br !== -1 ? br : br = ie();
}
function Tt(e) {
  return e.mode & 1 ? B & 2 && me !== 0 ? me & -me : Xf.transition !== null ? (el === 0 && (el = fu()), el) : (e = H, e !== 0 || (e = window.event, e = e === void 0 ? 16 : yu(e.type)), e) : 1;
}
function be(e, t, n, r) {
  if (50 < tr) throw tr = 0, da = null, Error(E(185));
  kr(e, n, r), (!(B & 2) || e !== fe) && (e === fe && (!(B & 2) && (Ml |= n), ue === 4 && kt(e, me)), Te(e, r), n === 1 && B === 0 && !(t.mode & 1) && (Pn = ie() + 500, Fl && Ot()));
}
function Te(e, t) {
  var n = e.callbackNode;
  Yd(e, t);
  var r = al(e, e === fe ? me : 0);
  if (r === 0) n !== null && Fo(n), e.callbackNode = null, e.callbackPriority = 0;
  else if (t = r & -r, e.callbackPriority !== t) {
    if (n != null && Fo(n), t === 1) e.tag === 0 ? Yf(Ns.bind(null, e)) : $u(Ns.bind(null, e)), Wf(function() {
      !(B & 6) && Ot();
    }), n = null;
    else {
      switch (pu(r)) {
        case 1:
          n = Ea;
          break;
        case 4:
          n = cu;
          break;
        case 16:
          n = il;
          break;
        case 536870912:
          n = du;
          break;
        default:
          n = il;
      }
      n = Mc(n, Ic.bind(null, e));
    }
    e.callbackPriority = t, e.callbackNode = n;
  }
}
function Ic(e, t) {
  if (br = -1, el = 0, B & 6) throw Error(E(327));
  var n = e.callbackNode;
  if (jn() && e.callbackNode !== n) return null;
  var r = al(e, e === fe ? me : 0);
  if (r === 0) return null;
  if (r & 30 || r & e.expiredLanes || t) t = Nl(e, r);
  else {
    t = r;
    var l = B;
    B |= 2;
    var i = Tc();
    (fe !== e || me !== t) && (st = null, Pn = ie() + 500, Qt(e, t));
    do
      try {
        hp();
        break;
      } catch (s) {
        zc(e, s);
      }
    while (!0);
    Ua(), wl.current = i, B = l, oe !== null ? t = 0 : (fe = null, me = 0, t = ue);
  }
  if (t !== 0) {
    if (t === 2 && (l = $i(e), l !== 0 && (r = l, t = fa(e, l))), t === 1) throw n = yr, Qt(e, 0), kt(e, r), Te(e, ie()), n;
    if (t === 6) kt(e, r);
    else {
      if (l = e.current.alternate, !(r & 30) && !pp(l) && (t = Nl(e, r), t === 2 && (i = $i(e), i !== 0 && (r = i, t = fa(e, i))), t === 1)) throw n = yr, Qt(e, 0), kt(e, r), Te(e, ie()), n;
      switch (e.finishedWork = l, e.finishedLanes = r, t) {
        case 0:
        case 1:
          throw Error(E(345));
        case 2:
          Vt(e, Pe, st);
          break;
        case 3:
          if (kt(e, r), (r & 130023424) === r && (t = eo + 500 - ie(), 10 < t)) {
            if (al(e, 0) !== 0) break;
            if (l = e.suspendedLanes, (l & r) !== r) {
              Ne(), e.pingedLanes |= e.suspendedLanes & l;
              break;
            }
            e.timeoutHandle = Ki(Vt.bind(null, e, Pe, st), t);
            break;
          }
          Vt(e, Pe, st);
          break;
        case 4:
          if (kt(e, r), (r & 4194240) === r) break;
          for (t = e.eventTimes, l = -1; 0 < r; ) {
            var o = 31 - Je(r);
            i = 1 << o, o = t[o], o > l && (l = o), r &= ~i;
          }
          if (r = l, r = ie() - r, r = (120 > r ? 120 : 480 > r ? 480 : 1080 > r ? 1080 : 1920 > r ? 1920 : 3e3 > r ? 3e3 : 4320 > r ? 4320 : 1960 * fp(r / 1960)) - r, 10 < r) {
            e.timeoutHandle = Ki(Vt.bind(null, e, Pe, st), r);
            break;
          }
          Vt(e, Pe, st);
          break;
        case 5:
          Vt(e, Pe, st);
          break;
        default:
          throw Error(E(329));
      }
    }
  }
  return Te(e, ie()), e.callbackNode === n ? Ic.bind(null, e) : null;
}
function fa(e, t) {
  var n = er;
  return e.current.memoizedState.isDehydrated && (Qt(e, t).flags |= 256), e = Nl(e, t), e !== 2 && (t = Pe, Pe = n, t !== null && pa(t)), e;
}
function pa(e) {
  Pe === null ? Pe = e : Pe.push.apply(Pe, e);
}
function pp(e) {
  for (var t = e; ; ) {
    if (t.flags & 16384) {
      var n = t.updateQueue;
      if (n !== null && (n = n.stores, n !== null)) for (var r = 0; r < n.length; r++) {
        var l = n[r], i = l.getSnapshot;
        l = l.value;
        try {
          if (!et(i(), l)) return !1;
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
function kt(e, t) {
  for (t &= ~ba, t &= ~Ml, e.suspendedLanes |= t, e.pingedLanes &= ~t, e = e.expirationTimes; 0 < t; ) {
    var n = 31 - Je(t), r = 1 << n;
    e[n] = -1, t &= ~r;
  }
}
function Ns(e) {
  if (B & 6) throw Error(E(327));
  jn();
  var t = al(e, 0);
  if (!(t & 1)) return Te(e, ie()), null;
  var n = Nl(e, t);
  if (e.tag !== 0 && n === 2) {
    var r = $i(e);
    r !== 0 && (t = r, n = fa(e, r));
  }
  if (n === 1) throw n = yr, Qt(e, 0), kt(e, t), Te(e, ie()), n;
  if (n === 6) throw Error(E(345));
  return e.finishedWork = e.current.alternate, e.finishedLanes = t, Vt(e, Pe, st), Te(e, ie()), null;
}
function to(e, t) {
  var n = B;
  B |= 1;
  try {
    return e(t);
  } finally {
    B = n, B === 0 && (Pn = ie() + 500, Fl && Ot());
  }
}
function Zt(e) {
  Nt !== null && Nt.tag === 0 && !(B & 6) && jn();
  var t = B;
  B |= 1;
  var n = He.transition, r = H;
  try {
    if (He.transition = null, H = 1, e) return e();
  } finally {
    H = r, He.transition = n, B = t, !(B & 6) && Ot();
  }
}
function no() {
  Fe = mn.current, q(mn);
}
function Qt(e, t) {
  e.finishedWork = null, e.finishedLanes = 0;
  var n = e.timeoutHandle;
  if (n !== -1 && (e.timeoutHandle = -1, Hf(n)), oe !== null) for (n = oe.return; n !== null; ) {
    var r = n;
    switch (Ma(r), r.tag) {
      case 1:
        r = r.type.childContextTypes, r != null && dl();
        break;
      case 3:
        Cn(), q(Ie), q(je), Qa();
        break;
      case 5:
        Wa(r);
        break;
      case 4:
        Cn();
        break;
      case 13:
        q(b);
        break;
      case 19:
        q(b);
        break;
      case 10:
        Aa(r.type._context);
        break;
      case 22:
      case 23:
        no();
    }
    n = n.return;
  }
  if (fe = e, oe = e = Ft(e.current, null), me = Fe = t, ue = 0, yr = null, ba = Ml = qt = 0, Pe = er = null, Ht !== null) {
    for (t = 0; t < Ht.length; t++) if (n = Ht[t], r = n.interleaved, r !== null) {
      n.interleaved = null;
      var l = r.next, i = n.pending;
      if (i !== null) {
        var o = i.next;
        i.next = l, r.next = o;
      }
      n.pending = r;
    }
    Ht = null;
  }
  return e;
}
function zc(e, t) {
  do {
    var n = oe;
    try {
      if (Ua(), qr.current = jl, yl) {
        for (var r = ee.memoizedState; r !== null; ) {
          var l = r.queue;
          l !== null && (l.pending = null), r = r.next;
        }
        yl = !1;
      }
      if (Xt = 0, de = se = ee = null, Jn = !1, vr = 0, Ja.current = null, n === null || n.return === null) {
        ue = 1, yr = t, oe = null;
        break;
      }
      e: {
        var i = e, o = n.return, s = n, u = t;
        if (t = me, s.flags |= 32768, u !== null && typeof u == "object" && typeof u.then == "function") {
          var d = u, x = s, c = x.tag;
          if (!(x.mode & 1) && (c === 0 || c === 11 || c === 15)) {
            var m = x.alternate;
            m ? (x.updateQueue = m.updateQueue, x.memoizedState = m.memoizedState, x.lanes = m.lanes) : (x.updateQueue = null, x.memoizedState = null);
          }
          var v = ds(o);
          if (v !== null) {
            v.flags &= -257, fs(v, o, s, i, t), v.mode & 1 && cs(i, d, t), t = v, u = d;
            var g = t.updateQueue;
            if (g === null) {
              var k = /* @__PURE__ */ new Set();
              k.add(u), t.updateQueue = k;
            } else g.add(u);
            break e;
          } else {
            if (!(t & 1)) {
              cs(i, d, t), ro();
              break e;
            }
            u = Error(E(426));
          }
        } else if (J && s.mode & 1) {
          var F = ds(o);
          if (F !== null) {
            !(F.flags & 65536) && (F.flags |= 256), fs(F, o, s, i, t), Oa(En(u, s));
            break e;
          }
        }
        i = u = En(u, s), ue !== 4 && (ue = 2), er === null ? er = [i] : er.push(i), i = o;
        do {
          switch (i.tag) {
            case 3:
              i.flags |= 65536, t &= -t, i.lanes |= t;
              var f = pc(i, u, t);
              ls(i, f);
              break e;
            case 1:
              s = u;
              var p = i.type, h = i.stateNode;
              if (!(i.flags & 128) && (typeof p.getDerivedStateFromError == "function" || h !== null && typeof h.componentDidCatch == "function" && (zt === null || !zt.has(h)))) {
                i.flags |= 65536, t &= -t, i.lanes |= t;
                var w = mc(i, s, t);
                ls(i, w);
                break e;
              }
          }
          i = i.return;
        } while (i !== null);
      }
      Lc(n);
    } catch (I) {
      t = I, oe === n && n !== null && (oe = n = n.return);
      continue;
    }
    break;
  } while (!0);
}
function Tc() {
  var e = wl.current;
  return wl.current = jl, e === null ? jl : e;
}
function ro() {
  (ue === 0 || ue === 3 || ue === 2) && (ue = 4), fe === null || !(qt & 268435455) && !(Ml & 268435455) || kt(fe, me);
}
function Nl(e, t) {
  var n = B;
  B |= 2;
  var r = Tc();
  (fe !== e || me !== t) && (st = null, Qt(e, t));
  do
    try {
      mp();
      break;
    } catch (l) {
      zc(e, l);
    }
  while (!0);
  if (Ua(), B = n, wl.current = r, oe !== null) throw Error(E(261));
  return fe = null, me = 0, ue;
}
function mp() {
  for (; oe !== null; ) Fc(oe);
}
function hp() {
  for (; oe !== null && !Ud(); ) Fc(oe);
}
function Fc(e) {
  var t = Dc(e.alternate, e, Fe);
  e.memoizedProps = e.pendingProps, t === null ? Lc(e) : oe = t, Ja.current = null;
}
function Lc(e) {
  var t = e;
  do {
    var n = t.alternate;
    if (e = t.return, t.flags & 32768) {
      if (n = sp(n, t), n !== null) {
        n.flags &= 32767, oe = n;
        return;
      }
      if (e !== null) e.flags |= 32768, e.subtreeFlags = 0, e.deletions = null;
      else {
        ue = 6, oe = null;
        return;
      }
    } else if (n = op(n, t, Fe), n !== null) {
      oe = n;
      return;
    }
    if (t = t.sibling, t !== null) {
      oe = t;
      return;
    }
    oe = t = e;
  } while (t !== null);
  ue === 0 && (ue = 5);
}
function Vt(e, t, n) {
  var r = H, l = He.transition;
  try {
    He.transition = null, H = 1, vp(e, t, n, r);
  } finally {
    He.transition = l, H = r;
  }
  return null;
}
function vp(e, t, n, r) {
  do
    jn();
  while (Nt !== null);
  if (B & 6) throw Error(E(327));
  n = e.finishedWork;
  var l = e.finishedLanes;
  if (n === null) return null;
  if (e.finishedWork = null, e.finishedLanes = 0, n === e.current) throw Error(E(177));
  e.callbackNode = null, e.callbackPriority = 0;
  var i = n.lanes | n.childLanes;
  if (Xd(e, i), e === fe && (oe = fe = null, me = 0), !(n.subtreeFlags & 2064) && !(n.flags & 2064) || Br || (Br = !0, Mc(il, function() {
    return jn(), null;
  })), i = (n.flags & 15990) !== 0, n.subtreeFlags & 15990 || i) {
    i = He.transition, He.transition = null;
    var o = H;
    H = 1;
    var s = B;
    B |= 4, Ja.current = null, cp(e, n), Pc(n, e), Mf(Wi), ol = !!Hi, Wi = Hi = null, e.current = n, dp(n), Ad(), B = s, H = o, He.transition = i;
  } else e.current = n;
  if (Br && (Br = !1, Nt = e, Sl = l), i = e.pendingLanes, i === 0 && (zt = null), Hd(n.stateNode), Te(e, ie()), t !== null) for (r = e.onRecoverableError, n = 0; n < t.length; n++) l = t[n], r(l.value, { componentStack: l.stack, digest: l.digest });
  if (kl) throw kl = !1, e = ca, ca = null, e;
  return Sl & 1 && e.tag !== 0 && jn(), i = e.pendingLanes, i & 1 ? e === da ? tr++ : (tr = 0, da = e) : tr = 0, Ot(), null;
}
function jn() {
  if (Nt !== null) {
    var e = pu(Sl), t = He.transition, n = H;
    try {
      if (He.transition = null, H = 16 > e ? 16 : e, Nt === null) var r = !1;
      else {
        if (e = Nt, Nt = null, Sl = 0, B & 6) throw Error(E(331));
        var l = B;
        for (B |= 4, D = e.current; D !== null; ) {
          var i = D, o = i.child;
          if (D.flags & 16) {
            var s = i.deletions;
            if (s !== null) {
              for (var u = 0; u < s.length; u++) {
                var d = s[u];
                for (D = d; D !== null; ) {
                  var x = D;
                  switch (x.tag) {
                    case 0:
                    case 11:
                    case 15:
                      bn(8, x, i);
                  }
                  var c = x.child;
                  if (c !== null) c.return = x, D = c;
                  else for (; D !== null; ) {
                    x = D;
                    var m = x.sibling, v = x.return;
                    if (Nc(x), x === d) {
                      D = null;
                      break;
                    }
                    if (m !== null) {
                      m.return = v, D = m;
                      break;
                    }
                    D = v;
                  }
                }
              }
              var g = i.alternate;
              if (g !== null) {
                var k = g.child;
                if (k !== null) {
                  g.child = null;
                  do {
                    var F = k.sibling;
                    k.sibling = null, k = F;
                  } while (k !== null);
                }
              }
              D = i;
            }
          }
          if (i.subtreeFlags & 2064 && o !== null) o.return = i, D = o;
          else e: for (; D !== null; ) {
            if (i = D, i.flags & 2048) switch (i.tag) {
              case 0:
              case 11:
              case 15:
                bn(9, i, i.return);
            }
            var f = i.sibling;
            if (f !== null) {
              f.return = i.return, D = f;
              break e;
            }
            D = i.return;
          }
        }
        var p = e.current;
        for (D = p; D !== null; ) {
          o = D;
          var h = o.child;
          if (o.subtreeFlags & 2064 && h !== null) h.return = o, D = h;
          else e: for (o = p; D !== null; ) {
            if (s = D, s.flags & 2048) try {
              switch (s.tag) {
                case 0:
                case 11:
                case 15:
                  Dl(9, s);
              }
            } catch (I) {
              le(s, s.return, I);
            }
            if (s === o) {
              D = null;
              break e;
            }
            var w = s.sibling;
            if (w !== null) {
              w.return = s.return, D = w;
              break e;
            }
            D = s.return;
          }
        }
        if (B = l, Ot(), lt && typeof lt.onPostCommitFiberRoot == "function") try {
          lt.onPostCommitFiberRoot(Pl, e);
        } catch {
        }
        r = !0;
      }
      return r;
    } finally {
      H = n, He.transition = t;
    }
  }
  return !1;
}
function Cs(e, t, n) {
  t = En(n, t), t = pc(e, t, 1), e = It(e, t, 1), t = Ne(), e !== null && (kr(e, 1, t), Te(e, t));
}
function le(e, t, n) {
  if (e.tag === 3) Cs(e, e, n);
  else for (; t !== null; ) {
    if (t.tag === 3) {
      Cs(t, e, n);
      break;
    } else if (t.tag === 1) {
      var r = t.stateNode;
      if (typeof t.type.getDerivedStateFromError == "function" || typeof r.componentDidCatch == "function" && (zt === null || !zt.has(r))) {
        e = En(n, e), e = mc(t, e, 1), t = It(t, e, 1), e = Ne(), t !== null && (kr(t, 1, e), Te(t, e));
        break;
      }
    }
    t = t.return;
  }
}
function gp(e, t, n) {
  var r = e.pingCache;
  r !== null && r.delete(t), t = Ne(), e.pingedLanes |= e.suspendedLanes & n, fe === e && (me & n) === n && (ue === 4 || ue === 3 && (me & 130023424) === me && 500 > ie() - eo ? Qt(e, 0) : ba |= n), Te(e, t);
}
function Rc(e, t) {
  t === 0 && (e.mode & 1 ? (t = Fr, Fr <<= 1, !(Fr & 130023424) && (Fr = 4194304)) : t = 1);
  var n = Ne();
  e = ht(e, t), e !== null && (kr(e, t, n), Te(e, n));
}
function xp(e) {
  var t = e.memoizedState, n = 0;
  t !== null && (n = t.retryLane), Rc(e, n);
}
function yp(e, t) {
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
      throw Error(E(314));
  }
  r !== null && r.delete(t), Rc(e, n);
}
var Dc;
Dc = function(e, t, n) {
  if (e !== null) if (e.memoizedProps !== t.pendingProps || Ie.current) _e = !0;
  else {
    if (!(e.lanes & n) && !(t.flags & 128)) return _e = !1, ap(e, t, n);
    _e = !!(e.flags & 131072);
  }
  else _e = !1, J && t.flags & 1048576 && Uu(t, ml, t.index);
  switch (t.lanes = 0, t.tag) {
    case 2:
      var r = t.type;
      Jr(e, t), e = t.pendingProps;
      var l = kn(t, je.current);
      yn(t, n), l = Ga(null, t, r, e, l, n);
      var i = Ya();
      return t.flags |= 1, typeof l == "object" && l !== null && typeof l.render == "function" && l.$$typeof === void 0 ? (t.tag = 1, t.memoizedState = null, t.updateQueue = null, ze(r) ? (i = !0, fl(t)) : i = !1, t.memoizedState = l.state !== null && l.state !== void 0 ? l.state : null, Ba(t), l.updater = Rl, t.stateNode = l, l._reactInternals = t, bi(t, r, e, n), t = na(null, t, r, !0, i, n)) : (t.tag = 0, J && i && Da(t), ke(null, t, l, n), t = t.child), t;
    case 16:
      r = t.elementType;
      e: {
        switch (Jr(e, t), e = t.pendingProps, l = r._init, r = l(r._payload), t.type = r, l = t.tag = wp(r), e = Ye(r, e), l) {
          case 0:
            t = ta(null, t, r, e, n);
            break e;
          case 1:
            t = hs(null, t, r, e, n);
            break e;
          case 11:
            t = ps(null, t, r, e, n);
            break e;
          case 14:
            t = ms(null, t, r, Ye(r.type, e), n);
            break e;
        }
        throw Error(E(
          306,
          r,
          ""
        ));
      }
      return t;
    case 0:
      return r = t.type, l = t.pendingProps, l = t.elementType === r ? l : Ye(r, l), ta(e, t, r, l, n);
    case 1:
      return r = t.type, l = t.pendingProps, l = t.elementType === r ? l : Ye(r, l), hs(e, t, r, l, n);
    case 3:
      e: {
        if (xc(t), e === null) throw Error(E(387));
        r = t.pendingProps, i = t.memoizedState, l = i.element, Qu(e, t), gl(t, r, null, n);
        var o = t.memoizedState;
        if (r = o.element, i.isDehydrated) if (i = { element: r, isDehydrated: !1, cache: o.cache, pendingSuspenseBoundaries: o.pendingSuspenseBoundaries, transitions: o.transitions }, t.updateQueue.baseState = i, t.memoizedState = i, t.flags & 256) {
          l = En(Error(E(423)), t), t = vs(e, t, r, n, l);
          break e;
        } else if (r !== l) {
          l = En(Error(E(424)), t), t = vs(e, t, r, n, l);
          break e;
        } else for (Le = _t(t.stateNode.containerInfo.firstChild), Re = t, J = !0, qe = null, n = Hu(t, null, r, n), t.child = n; n; ) n.flags = n.flags & -3 | 4096, n = n.sibling;
        else {
          if (Sn(), r === l) {
            t = vt(e, t, n);
            break e;
          }
          ke(e, t, r, n);
        }
        t = t.child;
      }
      return t;
    case 5:
      return Ku(t), e === null && qi(t), r = t.type, l = t.pendingProps, i = e !== null ? e.memoizedProps : null, o = l.children, Qi(r, l) ? o = null : i !== null && Qi(r, i) && (t.flags |= 32), gc(e, t), ke(e, t, o, n), t.child;
    case 6:
      return e === null && qi(t), null;
    case 13:
      return yc(e, t, n);
    case 4:
      return Ha(t, t.stateNode.containerInfo), r = t.pendingProps, e === null ? t.child = Nn(t, null, r, n) : ke(e, t, r, n), t.child;
    case 11:
      return r = t.type, l = t.pendingProps, l = t.elementType === r ? l : Ye(r, l), ps(e, t, r, l, n);
    case 7:
      return ke(e, t, t.pendingProps, n), t.child;
    case 8:
      return ke(e, t, t.pendingProps.children, n), t.child;
    case 12:
      return ke(e, t, t.pendingProps.children, n), t.child;
    case 10:
      e: {
        if (r = t.type._context, l = t.pendingProps, i = t.memoizedProps, o = l.value, Y(hl, r._currentValue), r._currentValue = o, i !== null) if (et(i.value, o)) {
          if (i.children === l.children && !Ie.current) {
            t = vt(e, t, n);
            break e;
          }
        } else for (i = t.child, i !== null && (i.return = t); i !== null; ) {
          var s = i.dependencies;
          if (s !== null) {
            o = i.child;
            for (var u = s.firstContext; u !== null; ) {
              if (u.context === r) {
                if (i.tag === 1) {
                  u = ft(-1, n & -n), u.tag = 2;
                  var d = i.updateQueue;
                  if (d !== null) {
                    d = d.shared;
                    var x = d.pending;
                    x === null ? u.next = u : (u.next = x.next, x.next = u), d.pending = u;
                  }
                }
                i.lanes |= n, u = i.alternate, u !== null && (u.lanes |= n), Zi(
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
            if (o = i.return, o === null) throw Error(E(341));
            o.lanes |= n, s = o.alternate, s !== null && (s.lanes |= n), Zi(o, n, t), o = i.sibling;
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
        ke(e, t, l.children, n), t = t.child;
      }
      return t;
    case 9:
      return l = t.type, r = t.pendingProps.children, yn(t, n), l = We(l), r = r(l), t.flags |= 1, ke(e, t, r, n), t.child;
    case 14:
      return r = t.type, l = Ye(r, t.pendingProps), l = Ye(r.type, l), ms(e, t, r, l, n);
    case 15:
      return hc(e, t, t.type, t.pendingProps, n);
    case 17:
      return r = t.type, l = t.pendingProps, l = t.elementType === r ? l : Ye(r, l), Jr(e, t), t.tag = 1, ze(r) ? (e = !0, fl(t)) : e = !1, yn(t, n), fc(t, r, l), bi(t, r, l, n), na(null, t, r, !0, e, n);
    case 19:
      return jc(e, t, n);
    case 22:
      return vc(e, t, n);
  }
  throw Error(E(156, t.tag));
};
function Mc(e, t) {
  return uu(e, t);
}
function jp(e, t, n, r) {
  this.tag = e, this.key = n, this.sibling = this.child = this.return = this.stateNode = this.type = this.elementType = null, this.index = 0, this.ref = null, this.pendingProps = t, this.dependencies = this.memoizedState = this.updateQueue = this.memoizedProps = null, this.mode = r, this.subtreeFlags = this.flags = 0, this.deletions = null, this.childLanes = this.lanes = 0, this.alternate = null;
}
function Ve(e, t, n, r) {
  return new jp(e, t, n, r);
}
function lo(e) {
  return e = e.prototype, !(!e || !e.isReactComponent);
}
function wp(e) {
  if (typeof e == "function") return lo(e) ? 1 : 0;
  if (e != null) {
    if (e = e.$$typeof, e === Sa) return 11;
    if (e === Na) return 14;
  }
  return 2;
}
function Ft(e, t) {
  var n = e.alternate;
  return n === null ? (n = Ve(e.tag, t, e.key, e.mode), n.elementType = e.elementType, n.type = e.type, n.stateNode = e.stateNode, n.alternate = e, e.alternate = n) : (n.pendingProps = t, n.type = e.type, n.flags = 0, n.subtreeFlags = 0, n.deletions = null), n.flags = e.flags & 14680064, n.childLanes = e.childLanes, n.lanes = e.lanes, n.child = e.child, n.memoizedProps = e.memoizedProps, n.memoizedState = e.memoizedState, n.updateQueue = e.updateQueue, t = e.dependencies, n.dependencies = t === null ? null : { lanes: t.lanes, firstContext: t.firstContext }, n.sibling = e.sibling, n.index = e.index, n.ref = e.ref, n;
}
function tl(e, t, n, r, l, i) {
  var o = 2;
  if (r = e, typeof e == "function") lo(e) && (o = 1);
  else if (typeof e == "string") o = 5;
  else e: switch (e) {
    case rn:
      return Kt(n.children, l, i, t);
    case ka:
      o = 8, l |= 8;
      break;
    case Si:
      return e = Ve(12, n, t, l | 2), e.elementType = Si, e.lanes = i, e;
    case Ni:
      return e = Ve(13, n, t, l), e.elementType = Ni, e.lanes = i, e;
    case Ci:
      return e = Ve(19, n, t, l), e.elementType = Ci, e.lanes = i, e;
    case Ks:
      return Ol(n, l, i, t);
    default:
      if (typeof e == "object" && e !== null) switch (e.$$typeof) {
        case Ws:
          o = 10;
          break e;
        case Qs:
          o = 9;
          break e;
        case Sa:
          o = 11;
          break e;
        case Na:
          o = 14;
          break e;
        case yt:
          o = 16, r = null;
          break e;
      }
      throw Error(E(130, e == null ? e : typeof e, ""));
  }
  return t = Ve(o, n, t, l), t.elementType = e, t.type = r, t.lanes = i, t;
}
function Kt(e, t, n, r) {
  return e = Ve(7, e, r, t), e.lanes = n, e;
}
function Ol(e, t, n, r) {
  return e = Ve(22, e, r, t), e.elementType = Ks, e.lanes = n, e.stateNode = { isHidden: !1 }, e;
}
function ji(e, t, n) {
  return e = Ve(6, e, null, t), e.lanes = n, e;
}
function wi(e, t, n) {
  return t = Ve(4, e.children !== null ? e.children : [], e.key, t), t.lanes = n, t.stateNode = { containerInfo: e.containerInfo, pendingChildren: null, implementation: e.implementation }, t;
}
function kp(e, t, n, r, l) {
  this.tag = t, this.containerInfo = e, this.finishedWork = this.pingCache = this.current = this.pendingChildren = null, this.timeoutHandle = -1, this.callbackNode = this.pendingContext = this.context = null, this.callbackPriority = 0, this.eventTimes = ti(0), this.expirationTimes = ti(-1), this.entangledLanes = this.finishedLanes = this.mutableReadLanes = this.expiredLanes = this.pingedLanes = this.suspendedLanes = this.pendingLanes = 0, this.entanglements = ti(0), this.identifierPrefix = r, this.onRecoverableError = l, this.mutableSourceEagerHydrationData = null;
}
function io(e, t, n, r, l, i, o, s, u) {
  return e = new kp(e, t, n, s, u), t === 1 ? (t = 1, i === !0 && (t |= 8)) : t = 0, i = Ve(3, null, null, t), e.current = i, i.stateNode = e, i.memoizedState = { element: r, isDehydrated: n, cache: null, transitions: null, pendingSuspenseBoundaries: null }, Ba(i), e;
}
function Sp(e, t, n) {
  var r = 3 < arguments.length && arguments[3] !== void 0 ? arguments[3] : null;
  return { $$typeof: nn, key: r == null ? null : "" + r, children: e, containerInfo: t, implementation: n };
}
function Oc(e) {
  if (!e) return Rt;
  e = e._reactInternals;
  e: {
    if (bt(e) !== e || e.tag !== 1) throw Error(E(170));
    var t = e;
    do {
      switch (t.tag) {
        case 3:
          t = t.stateNode.context;
          break e;
        case 1:
          if (ze(t.type)) {
            t = t.stateNode.__reactInternalMemoizedMergedChildContext;
            break e;
          }
      }
      t = t.return;
    } while (t !== null);
    throw Error(E(171));
  }
  if (e.tag === 1) {
    var n = e.type;
    if (ze(n)) return Ou(e, n, t);
  }
  return t;
}
function $c(e, t, n, r, l, i, o, s, u) {
  return e = io(n, r, !0, e, l, i, o, s, u), e.context = Oc(null), n = e.current, r = Ne(), l = Tt(n), i = ft(r, l), i.callback = t ?? null, It(n, i, l), e.current.lanes = l, kr(e, l, r), Te(e, r), e;
}
function $l(e, t, n, r) {
  var l = t.current, i = Ne(), o = Tt(l);
  return n = Oc(n), t.context === null ? t.context = n : t.pendingContext = n, t = ft(i, o), t.payload = { element: e }, r = r === void 0 ? null : r, r !== null && (t.callback = r), e = It(l, t, o), e !== null && (be(e, l, o, i), Xr(e, l, o)), o;
}
function Cl(e) {
  if (e = e.current, !e.child) return null;
  switch (e.child.tag) {
    case 5:
      return e.child.stateNode;
    default:
      return e.child.stateNode;
  }
}
function Es(e, t) {
  if (e = e.memoizedState, e !== null && e.dehydrated !== null) {
    var n = e.retryLane;
    e.retryLane = n !== 0 && n < t ? n : t;
  }
}
function ao(e, t) {
  Es(e, t), (e = e.alternate) && Es(e, t);
}
function Np() {
  return null;
}
var Uc = typeof reportError == "function" ? reportError : function(e) {
  console.error(e);
};
function oo(e) {
  this._internalRoot = e;
}
Ul.prototype.render = oo.prototype.render = function(e) {
  var t = this._internalRoot;
  if (t === null) throw Error(E(409));
  $l(e, t, null, null);
};
Ul.prototype.unmount = oo.prototype.unmount = function() {
  var e = this._internalRoot;
  if (e !== null) {
    this._internalRoot = null;
    var t = e.containerInfo;
    Zt(function() {
      $l(null, e, null, null);
    }), t[mt] = null;
  }
};
function Ul(e) {
  this._internalRoot = e;
}
Ul.prototype.unstable_scheduleHydration = function(e) {
  if (e) {
    var t = vu();
    e = { blockedOn: null, target: e, priority: t };
    for (var n = 0; n < wt.length && t !== 0 && t < wt[n].priority; n++) ;
    wt.splice(n, 0, e), n === 0 && xu(e);
  }
};
function so(e) {
  return !(!e || e.nodeType !== 1 && e.nodeType !== 9 && e.nodeType !== 11);
}
function Al(e) {
  return !(!e || e.nodeType !== 1 && e.nodeType !== 9 && e.nodeType !== 11 && (e.nodeType !== 8 || e.nodeValue !== " react-mount-point-unstable "));
}
function Ps() {
}
function Cp(e, t, n, r, l) {
  if (l) {
    if (typeof r == "function") {
      var i = r;
      r = function() {
        var d = Cl(o);
        i.call(d);
      };
    }
    var o = $c(t, r, e, 0, null, !1, !1, "", Ps);
    return e._reactRootContainer = o, e[mt] = o.current, dr(e.nodeType === 8 ? e.parentNode : e), Zt(), o;
  }
  for (; l = e.lastChild; ) e.removeChild(l);
  if (typeof r == "function") {
    var s = r;
    r = function() {
      var d = Cl(u);
      s.call(d);
    };
  }
  var u = io(e, 0, !1, null, null, !1, !1, "", Ps);
  return e._reactRootContainer = u, e[mt] = u.current, dr(e.nodeType === 8 ? e.parentNode : e), Zt(function() {
    $l(t, u, n, r);
  }), u;
}
function Vl(e, t, n, r, l) {
  var i = n._reactRootContainer;
  if (i) {
    var o = i;
    if (typeof l == "function") {
      var s = l;
      l = function() {
        var u = Cl(o);
        s.call(u);
      };
    }
    $l(t, o, e, l);
  } else o = Cp(n, t, e, l, r);
  return Cl(o);
}
mu = function(e) {
  switch (e.tag) {
    case 3:
      var t = e.stateNode;
      if (t.current.memoizedState.isDehydrated) {
        var n = Qn(t.pendingLanes);
        n !== 0 && (Pa(t, n | 1), Te(t, ie()), !(B & 6) && (Pn = ie() + 500, Ot()));
      }
      break;
    case 13:
      Zt(function() {
        var r = ht(e, 1);
        if (r !== null) {
          var l = Ne();
          be(r, e, 1, l);
        }
      }), ao(e, 1);
  }
};
_a = function(e) {
  if (e.tag === 13) {
    var t = ht(e, 134217728);
    if (t !== null) {
      var n = Ne();
      be(t, e, 134217728, n);
    }
    ao(e, 134217728);
  }
};
hu = function(e) {
  if (e.tag === 13) {
    var t = Tt(e), n = ht(e, t);
    if (n !== null) {
      var r = Ne();
      be(n, e, t, r);
    }
    ao(e, t);
  }
};
vu = function() {
  return H;
};
gu = function(e, t) {
  var n = H;
  try {
    return H = e, t();
  } finally {
    H = n;
  }
};
Di = function(e, t, n) {
  switch (t) {
    case "input":
      if (_i(e, n), t = n.name, n.type === "radio" && t != null) {
        for (n = e; n.parentNode; ) n = n.parentNode;
        for (n = n.querySelectorAll("input[name=" + JSON.stringify("" + t) + '][type="radio"]'), t = 0; t < n.length; t++) {
          var r = n[t];
          if (r !== e && r.form === e.form) {
            var l = Tl(r);
            if (!l) throw Error(E(90));
            Ys(r), _i(r, l);
          }
        }
      }
      break;
    case "textarea":
      qs(e, n);
      break;
    case "select":
      t = n.value, t != null && hn(e, !!n.multiple, t, !1);
  }
};
ru = to;
lu = Zt;
var Ep = { usingClientEntryPoint: !1, Events: [Nr, sn, Tl, tu, nu, to] }, Bn = { findFiberByHostInstance: Bt, bundleType: 0, version: "18.3.1", rendererPackageName: "react-dom" }, Pp = { bundleType: Bn.bundleType, version: Bn.version, rendererPackageName: Bn.rendererPackageName, rendererConfig: Bn.rendererConfig, overrideHookState: null, overrideHookStateDeletePath: null, overrideHookStateRenamePath: null, overrideProps: null, overridePropsDeletePath: null, overridePropsRenamePath: null, setErrorHandler: null, setSuspenseHandler: null, scheduleUpdate: null, currentDispatcherRef: gt.ReactCurrentDispatcher, findHostInstanceByFiber: function(e) {
  return e = ou(e), e === null ? null : e.stateNode;
}, findFiberByHostInstance: Bn.findFiberByHostInstance || Np, findHostInstancesForRefresh: null, scheduleRefresh: null, scheduleRoot: null, setRefreshHandler: null, getCurrentFiber: null, reconcilerVersion: "18.3.1-next-f1338f8080-20240426" };
if (typeof __REACT_DEVTOOLS_GLOBAL_HOOK__ < "u") {
  var Hr = __REACT_DEVTOOLS_GLOBAL_HOOK__;
  if (!Hr.isDisabled && Hr.supportsFiber) try {
    Pl = Hr.inject(Pp), lt = Hr;
  } catch {
  }
}
Me.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED = Ep;
Me.createPortal = function(e, t) {
  var n = 2 < arguments.length && arguments[2] !== void 0 ? arguments[2] : null;
  if (!so(t)) throw Error(E(200));
  return Sp(e, t, null, n);
};
Me.createRoot = function(e, t) {
  if (!so(e)) throw Error(E(299));
  var n = !1, r = "", l = Uc;
  return t != null && (t.unstable_strictMode === !0 && (n = !0), t.identifierPrefix !== void 0 && (r = t.identifierPrefix), t.onRecoverableError !== void 0 && (l = t.onRecoverableError)), t = io(e, 1, !1, null, null, n, !1, r, l), e[mt] = t.current, dr(e.nodeType === 8 ? e.parentNode : e), new oo(t);
};
Me.findDOMNode = function(e) {
  if (e == null) return null;
  if (e.nodeType === 1) return e;
  var t = e._reactInternals;
  if (t === void 0)
    throw typeof e.render == "function" ? Error(E(188)) : (e = Object.keys(e).join(","), Error(E(268, e)));
  return e = ou(t), e = e === null ? null : e.stateNode, e;
};
Me.flushSync = function(e) {
  return Zt(e);
};
Me.hydrate = function(e, t, n) {
  if (!Al(t)) throw Error(E(200));
  return Vl(null, e, t, !0, n);
};
Me.hydrateRoot = function(e, t, n) {
  if (!so(e)) throw Error(E(405));
  var r = n != null && n.hydratedSources || null, l = !1, i = "", o = Uc;
  if (n != null && (n.unstable_strictMode === !0 && (l = !0), n.identifierPrefix !== void 0 && (i = n.identifierPrefix), n.onRecoverableError !== void 0 && (o = n.onRecoverableError)), t = $c(t, null, e, 1, n ?? null, l, !1, i, o), e[mt] = t.current, dr(e), r) for (e = 0; e < r.length; e++) n = r[e], l = n._getVersion, l = l(n._source), t.mutableSourceEagerHydrationData == null ? t.mutableSourceEagerHydrationData = [n, l] : t.mutableSourceEagerHydrationData.push(
    n,
    l
  );
  return new Ul(t);
};
Me.render = function(e, t, n) {
  if (!Al(t)) throw Error(E(200));
  return Vl(null, e, t, !1, n);
};
Me.unmountComponentAtNode = function(e) {
  if (!Al(e)) throw Error(E(40));
  return e._reactRootContainer ? (Zt(function() {
    Vl(null, null, e, !1, function() {
      e._reactRootContainer = null, e[mt] = null;
    });
  }), !0) : !1;
};
Me.unstable_batchedUpdates = to;
Me.unstable_renderSubtreeIntoContainer = function(e, t, n, r) {
  if (!Al(n)) throw Error(E(200));
  if (e == null || e._reactInternals === void 0) throw Error(E(38));
  return Vl(e, t, n, !1, r);
};
Me.version = "18.3.1-next-f1338f8080-20240426";
function Ac() {
  if (!(typeof __REACT_DEVTOOLS_GLOBAL_HOOK__ > "u" || typeof __REACT_DEVTOOLS_GLOBAL_HOOK__.checkDCE != "function"))
    try {
      __REACT_DEVTOOLS_GLOBAL_HOOK__.checkDCE(Ac);
    } catch (e) {
      console.error(e);
    }
}
Ac(), As.exports = Me;
var _p = As.exports, Vc, _s = _p;
Vc = _s.createRoot, _s.hydrateRoot;
class Ip extends Error {
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
      let x = `HTTP ${u.status}`;
      try {
        const c = await u.json();
        x = c.detail || c.title || x;
      } catch {
      }
      throw new Ip(x, u.status);
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
const Bc = y.createContext(null);
function $t() {
  const e = y.useContext(Bc);
  if (!e) throw new Error("Fuera del módulo de documentos");
  return e;
}
function Tp(e) {
  return zp((t, n) => fetch(t, n), e.token);
}
async function ma(e, t) {
  const n = e.token(), r = await fetch(t, { headers: n ? { Authorization: "Bearer " + n } : {} });
  if (!r.ok) throw new Error("No se pudo generar el documento");
  const l = URL.createObjectURL(await r.blob());
  window.open(l, "_blank"), setTimeout(() => URL.revokeObjectURL(l), 6e4);
}
const Hc = new Intl.NumberFormat("es-ES", { minimumFractionDigits: 2, maximumFractionDigits: 2 }), Fp = new Intl.NumberFormat("es-ES", { maximumFractionDigits: 3 }), $ = (e) => `${Hc.format(Number(e) || 0)} €`, Be = (e) => Hc.format(Number(e) || 0), Se = (e) => Fp.format(Number(e) || 0), at = (e) => {
  if (!e) return "";
  const t = String(e).slice(0, 10).split("-");
  return t.length === 3 ? `${t[2]}/${t[1]}/${t[0]}` : String(e);
}, _n = () => (/* @__PURE__ */ new Date()).toISOString().slice(0, 10), Lp = (e) => Math.round((e + Number.EPSILON) * 100) / 100;
let Rp = 0;
const uo = () => `l${Date.now().toString(36)}${(++Rp).toString(36)}`;
function Bl(e, t) {
  const [n, r] = y.useState(e);
  return y.useEffect(() => {
    const l = setTimeout(() => r(e), t);
    return () => clearTimeout(l);
  }, [e, t]), n;
}
function co() {
  const e = y.useRef(0);
  return () => {
    const t = ++e.current;
    return () => t === e.current;
  };
}
function Wc(e) {
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
const Dp = { presupuesto: "Presupuestos", pedido: "Pedidos de venta", factura: "Facturas emitidas", compra: "Pedidos de compra" }, Mp = { presupuesto: "+ Nuevo presupuesto", pedido: "+ Nuevo pedido", factura: "+ Nueva factura", compra: "+ Nuevo pedido" }, Op = {
  presupuesto: ["Borrador", "Aceptado", "Rechazado"],
  pedido: ["Borrador", "Confirmado", "ServidoParcial", "Servido", "Facturado", "Cancelado"],
  factura: ["Emitida", "Rectificada", "Anulada"],
  compra: ["Borrador", "Confirmado", "RecibidoParcial", "Recibido", "Facturado", "Cancelado"]
};
function $p(e) {
  const { api: t, navegar: n } = $t(), [r, l] = y.useState(""), [i, o] = y.useState(""), [s, u] = y.useState(""), [d, x] = y.useState(""), [c, m] = y.useState(1), [v, g] = y.useState(null), [k, F] = y.useState(0), [f, p] = y.useState(""), h = Bl(r, 250), w = 50;
  y.useEffect(() => m(1), [h, i, s, d, e.tipo]), y.useEffect(() => {
    p(""), (async () => {
      switch (e.tipo) {
        case "factura": {
          const z = new URLSearchParams({ pagina: String(c), tamanoPagina: String(w) });
          h.trim() && z.set("texto", h.trim()), i && z.set("estado", i), s && z.set("desde", s), d && z.set("hasta", d);
          const M = await t.get(`/facturas/buscar?${z}`);
          return F(M.total), M.elementos.map((P) => ({ id: P.id, numero: P.numeroCompleto, fecha: P.fechaEmision, tercero: P.clienteNombre + (P.clienteNif ? ` · ${P.clienteNif}` : ""), total: P.total, estado: P.estado, extra: P.tipo !== "Ordinaria" ? P.tipo : void 0 }));
        }
        case "presupuesto":
          return (await t.get("/presupuestos")).map((z) => ({ id: z.id, numero: z.numeroCompleto, fecha: z.fecha, tercero: z.clienteNombre, total: z.total, estado: z.estado }));
        case "pedido":
          return (await t.get("/pedidos-venta")).map((z) => ({ id: z.id, numero: z.numeroCompleto, fecha: z.fecha, tercero: z.clienteNombre, total: z.total, estado: z.estado }));
        case "compra":
          return (await t.get("/compras/pedidos")).map((z) => ({ id: z.id, numero: z.numeroCompleto, fecha: z.fecha, tercero: z.proveedorTexto, total: z.total, estado: z.estado, extra: z.empresaOrigenId ? "Intragrupo" : void 0 }));
      }
    })().then(g).catch((z) => (p(z.message), g([])));
  }, [t, e.tipo, c, h, i, s, d]);
  const I = y.useMemo(() => {
    if (!v || e.tipo === "factura") return v ?? [];
    const N = h.trim().toLowerCase();
    return v.filter((z) => (!N || z.numero.toLowerCase().includes(N) || z.tercero.toLowerCase().includes(N)) && (!i || z.estado === i) && (!s || z.fecha >= s) && (!d || z.fecha <= d));
  }, [v, h, i, s, d, e.tipo]), _ = I.reduce((N, z) => N + (z.estado === "Anulada" || z.estado === "Cancelado" ? 0 : z.total), 0), L = e.tipo === "factura" ? Math.max(1, Math.ceil(k / w)) : 1;
  return /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
    /* @__PURE__ */ a.jsxs("div", { className: "panel-head", children: [
      /* @__PURE__ */ a.jsx("h2", { children: Dp[e.tipo] }),
      /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: () => n({ tipo: e.tipo, pantalla: "editor" }), children: Mp[e.tipo] })
    ] }),
    /* @__PURE__ */ a.jsxs("div", { className: "dx-filtros", children: [
      /* @__PURE__ */ a.jsx("input", { placeholder: e.tipo === "compra" ? "Número o proveedor…" : "Número, cliente o NIF…", value: r, onChange: (N) => l(N.target.value), autoFocus: !0 }),
      /* @__PURE__ */ a.jsxs("select", { value: i, onChange: (N) => o(N.target.value), children: [
        /* @__PURE__ */ a.jsx("option", { value: "", children: "Todos los estados" }),
        Op[e.tipo].map((N) => /* @__PURE__ */ a.jsx("option", { value: N, children: N }, N))
      ] }),
      /* @__PURE__ */ a.jsx("input", { type: "date", value: s, onChange: (N) => u(N.target.value), title: "Desde" }),
      /* @__PURE__ */ a.jsx("input", { type: "date", value: d, onChange: (N) => x(N.target.value), title: "Hasta" })
    ] }),
    f && /* @__PURE__ */ a.jsx("p", { className: "dx-rojo", children: f }),
    v === null ? /* @__PURE__ */ a.jsx("p", { className: "muted", children: "Cargando…" }) : I.length === 0 ? /* @__PURE__ */ a.jsx("p", { className: "muted", children: "No hay documentos con estos filtros." }) : /* @__PURE__ */ a.jsxs("table", { className: "dx-lista-docs", children: [
      /* @__PURE__ */ a.jsx("thead", { children: /* @__PURE__ */ a.jsxs("tr", { children: [
        /* @__PURE__ */ a.jsx("th", { children: "Número" }),
        /* @__PURE__ */ a.jsx("th", { children: "Fecha" }),
        /* @__PURE__ */ a.jsx("th", { children: e.tipo === "compra" ? "Proveedor" : "Cliente" }),
        /* @__PURE__ */ a.jsx("th", { children: "Estado" }),
        /* @__PURE__ */ a.jsx("th", { className: "num", children: "Total" })
      ] }) }),
      /* @__PURE__ */ a.jsx("tbody", { children: I.map((N) => /* @__PURE__ */ a.jsxs("tr", { onClick: () => n({ tipo: e.tipo, pantalla: "vista", id: N.id }), tabIndex: 0, onKeyDown: (z) => z.key === "Enter" && n({ tipo: e.tipo, pantalla: "vista", id: N.id }), children: [
        /* @__PURE__ */ a.jsxs("td", { className: "mono", children: [
          /* @__PURE__ */ a.jsx("strong", { children: N.numero }),
          N.extra && /* @__PURE__ */ a.jsx("span", { className: "pill part", style: { marginLeft: 6 }, children: N.extra })
        ] }),
        /* @__PURE__ */ a.jsx("td", { children: at(N.fecha) }),
        /* @__PURE__ */ a.jsx("td", { children: N.tercero }),
        /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx("span", { className: Wc(N.estado), children: N.estado }) }),
        /* @__PURE__ */ a.jsx("td", { className: "num", children: /* @__PURE__ */ a.jsx("strong", { children: $(N.total) }) })
      ] }, N.id)) }),
      /* @__PURE__ */ a.jsx("tfoot", { children: /* @__PURE__ */ a.jsxs("tr", { children: [
        /* @__PURE__ */ a.jsxs("td", { colSpan: 4, className: "muted", children: [
          e.tipo === "factura" ? `${k} documentos` : `${I.length} documentos`,
          " · suma de la página sin anulados"
        ] }),
        /* @__PURE__ */ a.jsx("td", { className: "num", children: /* @__PURE__ */ a.jsx("strong", { children: $(_) }) })
      ] }) })
    ] }),
    L > 1 && /* @__PURE__ */ a.jsxs("div", { className: "dx-paginas", children: [
      /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", disabled: c <= 1, onClick: () => m(c - 1), children: "←" }),
      /* @__PURE__ */ a.jsxs("span", { className: "muted", children: [
        "Página ",
        c,
        " de ",
        L
      ] }),
      /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", disabled: c >= L, onClick: () => m(c + 1), children: "→" })
    ] })
  ] });
}
function In(e) {
  return y.useEffect(() => {
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
  const { api: t } = $t(), [n, r] = y.useState(!1), [l, i] = y.useState([]), [o, s] = y.useState(null), [u, d] = y.useState(0), x = Bl(e.texto, 180), c = o === e.texto.trim() ? l : [], m = co();
  y.useEffect(() => {
    if (!n) return;
    const g = m(), k = encodeURIComponent(x.trim());
    t.get(`/productos/buscar?texto=${k}&tamanoPagina=12`).then((F) => g() && (i(F.elementos ?? []), s(x.trim()), d(0))).catch(() => g() && (i([]), s(x.trim())));
  }, [x, n]);
  function v(g) {
    var k;
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
    (k = e.alTeclaFuera) == null || k.call(e, g);
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
        ...Object.fromEntries(Object.entries(e.datos ?? {}).map(([g, k]) => [`data-${g}`, k]))
      }
    ),
    n && c.length > 0 && /* @__PURE__ */ a.jsx("div", { className: "dx-lista", children: c.map((g, k) => /* @__PURE__ */ a.jsxs(
      "div",
      {
        className: "dx-opcion" + (k === u ? " activa" : ""),
        onMouseDown: (F) => (F.preventDefault(), e.alElegir(g), r(!1)),
        onMouseEnter: () => d(k),
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
            $(e.precioDe ? e.precioDe(g) : g.precioUnitario),
            "/",
            g.unidad,
            g.controlarStock && /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
              " · stock ",
              Se(g.stock)
            ] })
          ] })
        ]
      },
      g.id
    )) })
  ] });
}
function Qc(e) {
  const [t, n] = y.useState(""), [r, l] = y.useState(!1), [i, o] = y.useState(0), s = e.terceros.find((c) => c.id === e.valor), u = y.useMemo(() => {
    const c = t.trim().toLowerCase();
    return e.terceros.filter((m) => m.activo !== !1 && (!c || m.nombre.toLowerCase().includes(c) || (m.nifFiscal ?? "").toLowerCase().includes(c))).slice(0, 30);
  }, [t, e.terceros]), d = y.useRef(null);
  function x(c) {
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
            if (c.key === "Enter" && u[i]) return c.preventDefault(), x(u[i]);
            if (c.key === "Escape") return l(!1);
          }
        }
      ),
      r && u.length > 0 && /* @__PURE__ */ a.jsx("div", { className: "dx-lista", children: u.map((c, m) => /* @__PURE__ */ a.jsxs("div", { className: "dx-opcion" + (m === i ? " activa" : ""), onMouseDown: (v) => (v.preventDefault(), x(c)), onMouseEnter: () => o(m), children: [
        /* @__PURE__ */ a.jsx("strong", { children: c.nombre }),
        /* @__PURE__ */ a.jsx("span", { className: "muted", children: [c.nifFiscal, c.poblacion].filter(Boolean).join(" · ") })
      ] }, c.id)) })
    ] })
  ] });
}
const Ap = { Porcentaje: "%", PorUnidad: "€/ud", PorKilo: "€/kg", Importe: "€" };
function fo(e) {
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
          Ap[s.calculo],
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
function Hl(e) {
  var t;
  return (t = e.conceptos) != null && t.length ? /* @__PURE__ */ a.jsx("div", { className: "dx-aplicados", children: e.conceptos.map((n, r) => /* @__PURE__ */ a.jsxs("div", { children: [
    "· ",
    n.nombre,
    n.calculo === "Porcentaje" ? ` (${Se(n.valor)} %)` : "",
    n.repartido ? " · del documento" : "",
    n.efecto === "Coste" ? " · coste" : "",
    ": ",
    /* @__PURE__ */ a.jsx("strong", { children: $(n.importe) })
  ] }, r)) }) : null;
}
const nr = () => ({ clave: uo(), productoId: null, descripcion: "", cantidad: 1, precio: null, dto: 0, iva: null });
function Kc(e) {
  const t = y.useRef(null), [n, r] = y.useState(/* @__PURE__ */ new Set()), l = e.modo === "venta", i = l ? 7 : 5, o = (c, m) => e.alCambiar(e.lineas.map((v) => v.clave === c ? { ...v, ...m } : v)), s = (c) => {
    const m = e.lineas.filter((v) => v.clave !== c);
    e.alCambiar(m.length ? m : [nr()]);
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
        v === e.lineas.length - 1 ? (e.alCambiar([...e.lineas, nr()]), setTimeout(() => u(v + 1, 0), 30)) : u(v + 1, 0);
      } else c.key === "ArrowDown" && m.tagName !== "SELECT" ? (c.preventDefault(), u(Math.min(v + 1, e.lineas.length - 1), g)) : c.key === "ArrowUp" && m.tagName !== "SELECT" && (c.preventDefault(), u(Math.max(v - 1, 0), g));
  }
  const x = (c) => r((m) => {
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
        const v = e.calculos[m], g = (v == null ? void 0 : v.conceptos) ?? [], k = l && c.controlarStock && c.stock != null && c.cantidad > c.stock, F = v && v.margen != null && v.importe ? v.margen / v.importe * 100 : null;
        return [
          /* @__PURE__ */ a.jsxs("tr", { className: m % 2 ? "dx-par" : "", children: [
            /* @__PURE__ */ a.jsxs("td", { children: [
              e.soloLectura ? /* @__PURE__ */ a.jsx("span", { className: "mono", children: c.referencia ?? "" }) : /* @__PURE__ */ a.jsx(
                Up,
                {
                  texto: c.referencia ?? (c.productoId ? c.descripcion : ""),
                  alCambiarTexto: (f) => o(c.clave, { referencia: f, ...f === "" ? { productoId: null } : {} }),
                  alElegir: (f) => (e.alElegirArticulo(c.clave, f), u(m, 2)),
                  precioDe: l ? void 0 : (f) => f.precioCompraPorUnidadCompra ?? f.precioCompra,
                  datos: { f: m, c: 0 }
                }
              ),
              c.productoId && /* @__PURE__ */ a.jsxs("div", { className: "dx-sub", children: [
                c.unidad && /* @__PURE__ */ a.jsx("span", { children: c.unidad }),
                c.controlarStock && /* @__PURE__ */ a.jsxs("span", { className: k ? "dx-rojo" : "", children: [
                  " · stock ",
                  Se(c.stock)
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
                  onChange: (f) => o(c.clave, { descripcion: f.target.value })
                }
              ),
              g.length > 0 && !n.has(c.clave) && /* @__PURE__ */ a.jsx("button", { type: "button", className: "dx-resumen-conc", onClick: () => x(c.clave), title: "Ver y cambiar los conceptos", children: g.map((f) => `${f.importe < 0 ? "−" : "+"} ${f.codigo.toLowerCase()} ${Be(Math.abs(f.importe))}${f.efecto === "Coste" ? " (coste)" : ""}`).join(" · ") })
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
                onChange: (f) => o(c.clave, { cantidad: Number(f.target.value) })
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
                placeholder: v ? Be(v.precio) : "",
                title: c.precio == null ? "Precio de la tarifa del cliente o del artículo (escribe uno para fijarlo)" : "",
                onChange: (f) => o(c.clave, { precio: f.target.value === "" ? null : Number(f.target.value) })
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
                onChange: (f) => o(c.clave, { dto: Number(f.target.value), precio: c.precio ?? (v == null ? void 0 : v.precio) ?? null })
              }
            ) }),
            l && /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsxs("select", { "data-f": m, "data-c": 5, value: c.iva ?? (v == null ? void 0 : v.iva) ?? "", disabled: e.soloLectura, onChange: (f) => o(c.clave, { iva: f.target.value || null }), children: [
              !c.iva && !(v != null && v.iva) && /* @__PURE__ */ a.jsx("option", { value: "", children: "Del artículo" }),
              e.ivas.map((f) => /* @__PURE__ */ a.jsx("option", { value: f.codigo, children: f.nombre }, f.codigo))
            ] }) }),
            /* @__PURE__ */ a.jsx("td", { className: "num", children: /* @__PURE__ */ a.jsx("strong", { children: v ? $(v.importe) : "—" }) }),
            /* @__PURE__ */ a.jsx("td", { className: "num", children: l ? (v == null ? void 0 : v.margen) != null && /* @__PURE__ */ a.jsxs("span", { className: v.margen < 0 ? "dx-rojo" : "muted", children: [
              $(v.margen),
              F != null && /* @__PURE__ */ a.jsxs("div", { className: "dx-sub", children: [
                Be(F),
                " %"
              ] })
            ] }) : (v == null ? void 0 : v.costeUnitarioEntrada) != null && /* @__PURE__ */ a.jsxs("span", { className: "muted", children: [
              $(v.costeUnitarioEntrada),
              "/ud"
            ] }) }),
            /* @__PURE__ */ a.jsxs("td", { className: "right", style: { whiteSpace: "nowrap" }, children: [
              !e.soloLectura && e.catalogo.length > 0 && /* @__PURE__ */ a.jsx("button", { type: "button", className: "dx-icono" + (n.has(c.clave) ? " activo" : ""), title: "Conceptos de la línea", onClick: () => x(c.clave), "data-f": m, "data-c": l ? 6 : 4, children: "±" }),
              !e.soloLectura && /* @__PURE__ */ a.jsx("button", { type: "button", className: "dx-icono", title: "Quitar la línea", onClick: () => s(c.clave), children: "✕" })
            ] })
          ] }, c.clave),
          n.has(c.clave) && /* @__PURE__ */ a.jsx("tr", { className: "dx-fila-conc", children: /* @__PURE__ */ a.jsx("td", { colSpan: l ? 9 : 7, children: /* @__PURE__ */ a.jsx(fo, { catalogo: e.catalogo, lista: c.conceptos, sugeridos: e.sugeridos[c.clave], alCambiar: (f) => o(c.clave, { conceptos: f }) }) }) }, c.clave + "c")
        ];
      }) })
    ] }),
    !e.soloLectura && /* @__PURE__ */ a.jsx("button", { type: "button", className: "btn small ghost", style: { marginTop: 8 }, onClick: () => (e.alCambiar([...e.lineas, nr()]), setTimeout(() => u(e.lineas.length, 0), 30)), children: "+ Añadir línea" }),
    !e.soloLectura && /* @__PURE__ */ a.jsx("span", { className: "muted dx-ayuda", children: "Intro: siguiente casilla · ↑↓: cambiar de línea · F2: buscar artículo · el precio en gris es el de tarifa" })
  ] });
}
const Vp = { presupuesto: "presupuesto", pedido: "pedido de venta", factura: "factura" };
function Gc(e) {
  const t = /* @__PURE__ */ new Map();
  for (const n of e)
    for (const r of n.conceptos ?? [])
      if (r.repartido) {
        const l = t.get(r.conceptoId);
        t.set(r.conceptoId, { conceptoId: r.conceptoId, valor: r.calculo === "Importe" ? Lp(((l == null ? void 0 : l.valor) ?? 0) + r.valor) : r.valor });
      }
  return {
    porLinea: e.map((n) => (n.conceptos ?? []).filter((r) => !r.repartido).map((r) => ({ conceptoId: r.conceptoId, valor: r.valor }))),
    documento: [...t.values()]
  };
}
function Bp(e) {
  var ho, vo, go, xo;
  const { api: t, anfitrion: n } = $t(), r = !!((ho = e.semilla) != null && ho.rectificaId), [l, i] = y.useState([]), [o, s] = y.useState([]), [u, d] = y.useState([]), [x, c] = y.useState([]), [m, v] = y.useState([]), [g, k] = y.useState(((vo = e.semilla) == null ? void 0 : vo.clienteId) ?? ""), [F, f] = y.useState(e.tipo === "pedido" && ((go = e.semilla) != null && go.fecha) ? e.semilla.fecha : _n()), [p, h] = y.useState(""), [w, I] = y.useState(""), [_, L] = y.useState(0), [N, z] = y.useState(!1), [M, P] = y.useState(null), [Q, Ke] = y.useState(30), [$e, T] = y.useState(""), [W, ne] = y.useState([nr()]), [j, R] = y.useState([]), [S, U] = y.useState(null), [V, K] = y.useState(""), [we, ot] = y.useState(!1), [ce, Ut] = y.useState(!1), [Yc, Er] = y.useState(!1), Xc = co();
  y.useEffect(() => {
    t.get("/clientes").then(i).catch(() => i([])), t.get("/tipos-iva").then((C) => s(C.filter((O) => O.activo))).catch(() => s([])), t.get("/formas-pago").then((C) => d(C.filter((O) => O.activo))).catch(() => d([])), t.get("/series").then((C) => c([...new Set(C.filter((O) => O.tipoDocumento === "Factura").map((O) => O.prefijo))])).catch(() => c([])), t.get("/conceptos-linea?ambito=Ventas&activos=true").then(v).catch(() => v([]));
  }, [t]), y.useEffect(() => {
    const C = e.semilla;
    if (!C || !C.lineas.length) return;
    const { porLinea: O, documento: Z } = Gc(C.lineas), re = C.lineas.map((G, Xl) => ({
      clave: uo(),
      productoId: G.productoId ?? null,
      descripcion: G.descripcion,
      cantidad: G.cantidad,
      precio: G.precioUnitario,
      dto: G.porcentajeDescuento,
      iva: G.codigoIva,
      conceptos: r ? [] : O[Xl]
    }));
    ne(re), R(r ? [] : Z), Promise.all(re.map((G) => G.productoId ? t.get(`/productos/${G.productoId}`).catch(() => null) : Promise.resolve(null))).then(
      (G) => ne((Xl) => Xl.map((yo, en) => G[en] ? { ...yo, referencia: G[en].referencia ?? G[en].nombre, unidad: G[en].unidad, stock: G[en].stock, controlarStock: G[en].controlarStock } : yo))
    );
  }, [e.semilla, t, r]);
  const ae = l.find((C) => C.id === g);
  y.useEffect(() => {
    ae && (z(!!ae.recargoEquivalencia), ae.formaPagoDefectoId && I(ae.formaPagoDefectoId));
  }, [ae]);
  const Ln = y.useMemo(() => W.map((C, O) => ({ l: C, i: O })).filter(({ l: C }) => (C.productoId || C.descripcion.trim()) && C.cantidad > 0), [W]), po = y.useMemo(
    () => ({
      clienteId: g,
      fechaEmision: e.tipo === "factura" ? F : null,
      serie: p || null,
      diasVencimiento: _,
      formaPagoId: w || null,
      recargoEquivalencia: N,
      porcentajeIrpf: M,
      conceptosDocumento: j,
      lineas: Ln.map(({ l: C }) => ({
        cantidad: C.cantidad,
        descripcion: C.descripcion.trim() || null,
        precioUnitario: C.precio,
        codigoIva: C.iva,
        porcentajeDescuento: C.dto,
        productoId: C.productoId,
        ...r ? { conceptos: [] } : C.conceptos === void 0 ? {} : { conceptos: C.conceptos }
      }))
    }),
    [g, F, p, _, w, N, M, j, Ln, e.tipo, r]
  ), Rn = Bl(po, 350);
  y.useEffect(() => {
    if (!Rn.clienteId || Rn.lineas.length === 0) {
      U(null), K(Rn.clienteId ? "" : "Elige el cliente para calcular precios e importes.");
      return;
    }
    const C = Xc();
    ot(!0), t.post("/facturas/simular", Rn).then((O) => C() && (U(O), K(""))).catch((O) => C() && (U(null), K(O.message))).finally(() => C() && ot(!1));
  }, [Rn, t]);
  const Kl = y.useMemo(() => {
    const C = W.map(() => {
    });
    return S && Ln.forEach(({ i: O }, Z) => {
      const re = S.lineas[Z];
      re && (C[O] = { precio: re.precioUnitario, dto: re.porcentajeDescuento, iva: re.codigoIva, importe: re.base, margen: re.productoId || re.costeUnitario || re.costeConceptos ? re.margen : void 0, conceptos: re.conceptos });
    }), C;
  }, [S, W, Ln]), qc = y.useMemo(() => {
    const C = {};
    return W.forEach((O, Z) => {
      var re;
      return C[O.clave] = (((re = Kl[Z]) == null ? void 0 : re.conceptos) ?? []).filter((G) => !G.repartido).map((G) => ({ conceptoId: G.conceptoId, valor: G.valor }));
    }), C;
  }, [W, Kl]);
  function Zc(C, O) {
    ne(
      (Z) => Z.map(
        (re) => re.clave === C ? { ...re, productoId: O.id, referencia: O.referencia ?? O.nombre, descripcion: O.nombre, precio: null, dto: 0, iva: null, conceptos: void 0, unidad: O.unidad, stock: O.stock, controlarStock: O.controlarStock } : re
      )
    );
  }
  const Jc = y.useMemo(() => {
    const C = /* @__PURE__ */ new Map();
    for (const O of (S == null ? void 0 : S.lineas) ?? []) {
      const Z = C.get(O.codigoIva) ?? { base: 0, cuota: 0, pct: O.porcentajeIva };
      Z.base += O.base, Z.cuota += O.cuotaIva, C.set(O.codigoIva, Z);
    }
    return [...C.entries()];
  }, [S]), Gl = ((S == null ? void 0 : S.lineas) ?? []).reduce((C, O) => C + (O.base - O.margen), 0), Yl = S ? S.baseImponible - Gl : 0, bc = (C) => {
    var O;
    return ((O = o.find((Z) => Z.codigo === C)) == null ? void 0 : O.nombre) ?? C;
  };
  async function mo() {
    if (S) {
      Ut(!0);
      try {
        const C = Ln.map(({ l: Z }, re) => {
          const G = S.lineas[re];
          return {
            cantidad: Z.cantidad,
            descripcion: G.descripcion,
            precioUnitario: G.precioUnitario,
            codigoIva: G.codigoIva,
            porcentajeDescuento: G.porcentajeDescuento,
            productoId: Z.productoId,
            ...r ? {} : Z.conceptos === void 0 ? {} : { conceptos: Z.conceptos }
          };
        });
        let O;
        if (r)
          O = (await t.post(`/facturas/${e.semilla.rectificaId}/rectificar`, { motivo: $e, lineas: C, fechaEmision: F, porcentajeIrpf: M, serie: p || null })).id;
        else if (e.tipo === "factura")
          O = (await t.post("/facturas", { ...po, lineas: C })).id;
        else if (e.tipo === "presupuesto") {
          const Z = { clienteId: g, diasValidez: Q, lineas: C, conceptosDocumento: j };
          O = e.id ? (await t.put(`/presupuestos/${e.id}`, Z)).id : (await t.post("/presupuestos", Z)).id;
        } else {
          const Z = { clienteId: g, fecha: F, lineas: C, conceptosDocumento: j };
          O = e.id ? (await t.put(`/pedidos-venta/${e.id}`, Z)).id : (await t.post("/pedidos-venta", Z)).id;
        }
        n.aviso(e.tipo === "factura" ? "Factura emitida." : "Documento guardado.", "ok"), e.alGuardar(O);
      } catch (C) {
        n.aviso(C.message, "err");
      } finally {
        Ut(!1), Er(!1);
      }
    }
  }
  const ed = r ? `Rectificativa de la factura ${((xo = e.semilla) == null ? void 0 : xo.rectificaNumero) ?? ""}` : `${e.id ? "Editar" : "Nuevo"} ${Vp[e.tipo]}`.replace("Nuevo factura", "Nueva factura"), td = !!S && !we && (!r || $e.trim().length > 0);
  return /* @__PURE__ */ a.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsxs("div", { className: "panel-head", children: [
        /* @__PURE__ */ a.jsx("h2", { children: ed }),
        /* @__PURE__ */ a.jsxs("div", { style: { display: "flex", gap: 8 }, children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: e.alCancelar, children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", disabled: !td || ce, onClick: () => e.tipo === "factura" ? Er(!0) : mo(), children: e.tipo === "factura" ? "Emitir factura" : "Guardar" })
        ] })
      ] }),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-cabecera", children: [
        /* @__PURE__ */ a.jsxs("div", { className: "dx-cab-campos", children: [
          /* @__PURE__ */ a.jsx(Qc, { terceros: l, valor: g, alCambiar: k, etiqueta: "Cliente", deshabilitado: r || e.tipo === "pedido" && !!e.id && !1 }),
          /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", children: [
            e.tipo !== "presupuesto" && /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: e.tipo === "factura" ? "Fecha de emisión" : "Fecha del pedido" }),
              /* @__PURE__ */ a.jsx("input", { type: "date", value: F, onChange: (C) => f(C.target.value) })
            ] }),
            e.tipo === "presupuesto" && /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Validez" }),
              /* @__PURE__ */ a.jsx("select", { value: Q, onChange: (C) => Ke(Number(C.target.value)), children: [15, 30, 60, 90].map((C) => /* @__PURE__ */ a.jsxs("option", { value: C, children: [
                C,
                " días"
              ] }, C)) })
            ] }),
            e.tipo === "factura" && x.length > 0 && /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Serie" }),
              /* @__PURE__ */ a.jsxs("select", { value: p, onChange: (C) => h(C.target.value), children: [
                /* @__PURE__ */ a.jsx("option", { value: "", children: "La del cliente" }),
                x.map((C) => /* @__PURE__ */ a.jsx("option", { value: C, children: C }, C))
              ] })
            ] }),
            e.tipo === "factura" && !r && /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
              /* @__PURE__ */ a.jsxs("div", { children: [
                /* @__PURE__ */ a.jsx("label", { children: "Forma de pago" }),
                /* @__PURE__ */ a.jsxs("select", { value: w, onChange: (C) => I(C.target.value), children: [
                  /* @__PURE__ */ a.jsx("option", { value: "", children: "— Vencimiento a mano —" }),
                  u.map((C) => /* @__PURE__ */ a.jsx("option", { value: C.id, children: C.nombre }, C.id))
                ] })
              ] }),
              !w && /* @__PURE__ */ a.jsxs("div", { children: [
                /* @__PURE__ */ a.jsx("label", { children: "Vencimiento" }),
                /* @__PURE__ */ a.jsx("select", { value: _, onChange: (C) => L(Number(C.target.value)), children: [0, 15, 30, 45, 60, 90].map((C) => /* @__PURE__ */ a.jsx("option", { value: C, children: C ? `${C} días` : "Contado" }, C)) })
              ] })
            ] }),
            e.tipo === "factura" && /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Retención IRPF %" }),
              /* @__PURE__ */ a.jsx("input", { type: "number", step: "0.01", value: M ?? "", placeholder: String((ae == null ? void 0 : ae.porcentajeIrpfDefecto) ?? 0), onChange: (C) => P(C.target.value === "" ? null : Number(C.target.value)) })
            ] })
          ] }),
          e.tipo === "factura" && !r && /* @__PURE__ */ a.jsxs("label", { className: "dx-check", children: [
            /* @__PURE__ */ a.jsx("input", { type: "checkbox", checked: N, onChange: (C) => z(C.target.checked) }),
            " Recargo de equivalencia"
          ] }),
          r && /* @__PURE__ */ a.jsxs("div", { children: [
            /* @__PURE__ */ a.jsx("label", { children: "Motivo de la rectificación (obligatorio)" }),
            /* @__PURE__ */ a.jsx("input", { value: $e, onChange: (C) => T(C.target.value), placeholder: "Devolución, error en precio…" })
          ] })
        ] }),
        /* @__PURE__ */ a.jsx("div", { className: "dx-ficha", children: ae ? /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("strong", { children: ae.nombre }),
          /* @__PURE__ */ a.jsx("div", { className: "muted", children: [ae.nifFiscal, ae.poblacion, ae.provincia].filter(Boolean).join(" · ") }),
          ae.limiteRiesgo != null && /* @__PURE__ */ a.jsxs("div", { className: "muted", children: [
            "Límite de riesgo ",
            $(ae.limiteRiesgo)
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
      /* @__PURE__ */ a.jsx(Kc, { modo: "venta", lineas: W, alCambiar: ne, calculos: Kl, ivas: o, catalogo: r ? [] : m, sugeridos: qc, alElegirArticulo: Zc }),
      !r && m.length > 0 && /* @__PURE__ */ a.jsx("div", { style: { marginTop: 10 }, children: /* @__PURE__ */ a.jsx(fo, { catalogo: m, lista: j, alCambiar: (C) => R(C ?? []), documento: !0 }) })
    ] }),
    /* @__PURE__ */ a.jsxs("div", { className: "dx-pie", children: [
      /* @__PURE__ */ a.jsxs("div", { className: "dx-estado", children: [
        we && /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Calculando…" }),
        !we && V && /* @__PURE__ */ a.jsx("span", { className: "dx-rojo", children: V }),
        (S == null ? void 0 : S.mencionFiscal) && /* @__PURE__ */ a.jsx("div", { className: "muted", style: { fontSize: 12 }, children: S.mencionFiscal })
      ] }),
      /* @__PURE__ */ a.jsxs("div", { className: "panel dx-totales", children: [
        Jc.map(([C, O]) => /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsxs("span", { className: "muted", children: [
            bc(C),
            " · base ",
            Be(O.base)
          ] }),
          /* @__PURE__ */ a.jsx("span", { children: $(O.cuota) })
        ] }, C)),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Base imponible" }),
          /* @__PURE__ */ a.jsx("span", { children: $(S == null ? void 0 : S.baseImponible) })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Impuestos" }),
          /* @__PURE__ */ a.jsx("span", { children: $(S == null ? void 0 : S.cuotaIva) })
        ] }),
        !!(S != null && S.recargoTotal) && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Recargo de equivalencia" }),
          /* @__PURE__ */ a.jsx("span", { children: $(S.recargoTotal) })
        ] }),
        !!(S != null && S.retencionIrpf) && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsxs("span", { className: "muted", children: [
            "Retención IRPF (",
            Be(S.porcentajeIrpf),
            " %)"
          ] }),
          /* @__PURE__ */ a.jsxs("span", { children: [
            "−",
            $(S.retencionIrpf)
          ] })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ a.jsx("span", { children: "Total" }),
          /* @__PURE__ */ a.jsx("span", { children: $(S == null ? void 0 : S.total) })
        ] }),
        S && Gl > 0 && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Coste · margen" }),
          /* @__PURE__ */ a.jsxs("span", { className: Yl < 0 ? "dx-rojo" : "muted", children: [
            $(Gl),
            " · ",
            $(Yl),
            " (",
            Be(S.baseImponible ? Yl / S.baseImponible * 100 : 0),
            " %)"
          ] })
        ] })
      ] })
    ] }),
    Yc && S && /* @__PURE__ */ a.jsx(
      In,
      {
        titulo: r ? "Emitir la rectificativa" : "Emitir la factura",
        alCerrar: () => Er(!1),
        acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => Er(!1), children: "Revisar" }),
          /* @__PURE__ */ a.jsxs("button", { className: "btn small", disabled: ce, onClick: mo, children: [
            "Emitir ",
            $(S.total)
          ] })
        ] }),
        children: /* @__PURE__ */ a.jsxs("p", { style: { margin: 0 }, children: [
          "Se emitirá una factura ",
          r ? "rectificativa " : "",
          "de ",
          /* @__PURE__ */ a.jsx("strong", { children: $(S.total) }),
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
function Hp(e) {
  var T, W, ne;
  const { api: t, anfitrion: n } = $t(), [r, l] = y.useState([]), [i, o] = y.useState([]), [s, u] = y.useState(((T = e.semilla) == null ? void 0 : T.proveedorId) ?? ""), [d, x] = y.useState(((W = e.semilla) == null ? void 0 : W.fecha) ?? _n()), [c, m] = y.useState([nr()]), [v, g] = y.useState([]), [k, F] = y.useState(null), [f, p] = y.useState(""), [h, w] = y.useState(!1), I = co();
  y.useEffect(() => {
    t.get("/proveedores").then(l).catch(() => l([])), t.get("/conceptos-linea?ambito=Compras&activos=true").then(o).catch(() => o([]));
  }, [t]), y.useEffect(() => {
    const j = e.semilla;
    if (!j) return;
    const R = j.lineas.map((K) => ({ ...K, porcentajeDescuento: 0, codigoIva: "" })), { porLinea: S, documento: U } = Gc(R), V = j.lineas.map((K, we) => ({ clave: uo(), productoId: K.productoId ?? null, descripcion: K.descripcion, cantidad: K.cantidad, precio: K.precioUnitario, dto: 0, iva: null, conceptos: S[we] }));
    m(V), g(U), Promise.all(V.map((K) => K.productoId ? t.get(`/productos/${K.productoId}`).catch(() => null) : Promise.resolve(null))).then(
      (K) => m((we) => we.map((ot, ce) => K[ce] ? { ...ot, referencia: K[ce].referencia ?? K[ce].nombre, unidad: K[ce].unidadCompra || K[ce].unidad, stock: K[ce].stock, controlarStock: K[ce].controlarStock } : ot))
    );
  }, [e.semilla, t]);
  const _ = r.find((j) => j.id === s), L = y.useMemo(() => c.map((j, R) => ({ l: j, i: R })).filter(({ l: j }) => j.descripcion.trim() && j.cantidad > 0), [c]), N = y.useMemo(
    () => {
      var j, R;
      return {
        proveedorId: s || null,
        proveedorTexto: (_ == null ? void 0 : _.nombre) ?? (((j = e.semilla) == null ? void 0 : j.proveedorTexto) || null),
        solicitudOrigenId: e.id ? null : ((R = e.semilla) == null ? void 0 : R.solicitudOrigenId) ?? null,
        fecha: d,
        conceptosDocumento: v,
        lineas: L.map(({ l: S }) => ({ descripcion: S.descripcion.trim(), cantidad: S.cantidad, precioUnitario: S.precio ?? 0, productoId: S.productoId, ...S.conceptos === void 0 ? {} : { conceptos: S.conceptos } }))
      };
    },
    [s, _, d, v, L, e.id, e.semilla]
  ), z = Bl(N, 350);
  y.useEffect(() => {
    if (!z.proveedorId || z.lineas.length === 0) {
      F(null), p(z.proveedorId ? "" : "Elige el proveedor.");
      return;
    }
    const j = I();
    t.post("/compras/pedidos/simular", z).then((R) => j() && (F(R), p(""))).catch((R) => j() && (F(null), p(R.message)));
  }, [z, t]);
  const M = y.useMemo(() => {
    const j = c.map(() => {
    });
    return L.forEach(({ i: R }, S) => {
      const U = k == null ? void 0 : k.lineas[S];
      U && (j[R] = { precio: U.precioUnitario, importe: U.importe, costeUnitarioEntrada: U.costeUnitarioEntrada, conceptos: U.conceptos });
    }), j;
  }, [k, c, L]), P = y.useMemo(() => {
    const j = {};
    return c.forEach((R, S) => {
      var U;
      return j[R.clave] = (((U = M[S]) == null ? void 0 : U.conceptos) ?? []).filter((V) => !V.repartido).map((V) => ({ conceptoId: V.conceptoId, valor: V.valor }));
    }), j;
  }, [c, M]);
  function Q(j, R) {
    const S = R.precioCompraPorUnidadCompra ?? R.precioCompra;
    m((U) => U.map((V) => V.clave === j ? { ...V, productoId: R.id, referencia: R.referencia ?? R.nombre, descripcion: R.nombre, precio: S, conceptos: void 0, unidad: R.unidadCompra || R.unidad, stock: R.stock, controlarStock: R.controlarStock } : V));
  }
  async function Ke() {
    w(!0);
    try {
      const j = e.id ? await t.put(`/compras/pedidos/${e.id}`, N) : await t.post("/compras/pedidos", N);
      n.aviso("Pedido de compra guardado.", "ok"), e.alGuardar(j.id);
    } catch (j) {
      n.aviso(j.message, "err");
    } finally {
      w(!1);
    }
  }
  const $e = ((k == null ? void 0 : k.lineas) ?? []).reduce((j, R) => j + R.costeConceptos, 0);
  return /* @__PURE__ */ a.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsxs("div", { className: "panel-head", children: [
        /* @__PURE__ */ a.jsx("h2", { children: e.id ? `Editar pedido ${((ne = e.semilla) == null ? void 0 : ne.numeroCompleto) ?? ""}` : "Nuevo pedido de compra" }),
        /* @__PURE__ */ a.jsxs("div", { style: { display: "flex", gap: 8 }, children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: e.alCancelar, children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", disabled: !k || h, onClick: Ke, children: "Guardar" })
        ] })
      ] }),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-cabecera", children: [
        /* @__PURE__ */ a.jsxs("div", { className: "dx-cab-campos", children: [
          /* @__PURE__ */ a.jsx(Qc, { terceros: r, valor: s, alCambiar: u, etiqueta: "Proveedor", deshabilitado: !!e.id }),
          /* @__PURE__ */ a.jsx("div", { className: "dx-fila", children: /* @__PURE__ */ a.jsxs("div", { children: [
            /* @__PURE__ */ a.jsx("label", { children: "Fecha del pedido" }),
            /* @__PURE__ */ a.jsx("input", { type: "date", value: d, onChange: (j) => x(j.target.value) })
          ] }) })
        ] }),
        /* @__PURE__ */ a.jsx("div", { className: "dx-ficha", children: _ ? /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("strong", { children: _.nombre }),
          /* @__PURE__ */ a.jsx("div", { className: "muted", children: [_.nifFiscal, _.poblacion, _.pais].filter(Boolean).join(" · ") })
        ] }) : /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Elige un proveedor: se aplican sus conceptos (pronto pago, portes…)." }) })
      ] })
    ] }),
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsx(Kc, { modo: "compra", lineas: c, alCambiar: m, calculos: M, ivas: [], catalogo: i, sugeridos: P, alElegirArticulo: Q }),
      i.length > 0 && /* @__PURE__ */ a.jsx("div", { style: { marginTop: 10 }, children: /* @__PURE__ */ a.jsx(fo, { catalogo: i, lista: v, alCambiar: (j) => g(j ?? []), documento: !0 }) })
    ] }),
    /* @__PURE__ */ a.jsxs("div", { className: "dx-pie", children: [
      /* @__PURE__ */ a.jsx("div", { className: "dx-estado", children: f && /* @__PURE__ */ a.jsx("span", { className: "dx-rojo", children: f }) }),
      /* @__PURE__ */ a.jsxs("div", { className: "panel dx-totales", children: [
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ a.jsx("span", { children: "Total del pedido (sin impuestos)" }),
          /* @__PURE__ */ a.jsx("span", { children: $(k == null ? void 0 : k.total) })
        ] }),
        $e !== 0 && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Costes añadidos al almacén" }),
          /* @__PURE__ */ a.jsx("span", { children: $($e) })
        ] }),
        /* @__PURE__ */ a.jsx("div", { className: "muted", style: { fontSize: 12, marginTop: 6 }, children: "El impuesto se aplica al facturar el pedido." })
      ] })
    ] })
  ] });
}
function Wl(e) {
  return /* @__PURE__ */ a.jsxs("div", { className: "panel-head", children: [
    /* @__PURE__ */ a.jsxs("h2", { style: { display: "flex", alignItems: "center", gap: 10 }, children: [
      /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: e.volver, title: "Volver a la lista", children: "←" }),
      e.titulo,
      e.estado && /* @__PURE__ */ a.jsx("span", { className: Wc(e.estado), children: e.estado })
    ] }),
    /* @__PURE__ */ a.jsx("div", { className: "dx-acciones", children: e.acciones })
  ] });
}
function ye(e) {
  return /* @__PURE__ */ a.jsxs("div", { className: "dx-dato", children: [
    /* @__PURE__ */ a.jsx("small", { children: e.etiqueta }),
    /* @__PURE__ */ a.jsx("div", { children: e.children })
  ] });
}
function jr(e) {
  return /* @__PURE__ */ a.jsx("button", { type: "button", className: "dx-enlace", onClick: e.alPulsar, children: e.children });
}
function Ql(e) {
  const [t, n] = y.useState(null), [r, l] = y.useState(""), i = y.useCallback(() => {
    e().then(n).catch((o) => l(o.message));
  }, []);
  return y.useEffect(i, [i]), { dato: t, error: r, recargar: i };
}
async function Ze(e, t, n) {
  try {
    return await e(), n && t(n, "ok"), !0;
  } catch (r) {
    return t(r.message, "err"), !1;
  }
}
function Wp(e) {
  const { api: t, anfitrion: n, navegar: r } = $t(), { dato: l, error: i, recargar: o } = Ql(() => t.get(`/facturas/${e.id}`)), [s, u] = y.useState(null), [d, x] = y.useState(!1), [c, m] = y.useState("");
  if (y.useEffect(() => void t.get(`/facturas/${e.id}/saldo`).then(u).catch(() => u(null)), [t, e.id, l]), i) return /* @__PURE__ */ a.jsx("div", { className: "panel", children: /* @__PURE__ */ a.jsx("p", { className: "dx-rojo", children: i }) });
  if (!l) return /* @__PURE__ */ a.jsx("div", { className: "muted", children: "Cargando…" });
  const v = { clienteId: l.clienteId ?? void 0, lineas: l.lineas }, g = l.lineas.reduce((F, f) => F + (f.base - f.margen), 0), k = l.estado === "Emitida";
  return /* @__PURE__ */ a.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsx(
        Wl,
        {
          titulo: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            l.tipo === "Rectificativa" ? "Rectificativa" : l.tipo === "Simplificada" ? "Ticket" : "Factura",
            " ",
            /* @__PURE__ */ a.jsx("span", { className: "mono", children: l.numeroCompleto })
          ] }),
          estado: l.estado,
          volver: () => r({ tipo: "factura", pantalla: "lista" }),
          acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => ma(n, `/facturas/${l.id}/pdf`).catch((F) => n.aviso(F.message, "err")), children: "PDF" }),
            l.tipo !== "Simplificada" && l.clienteNif && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => ma(n, `/facturas/${l.id}/facturae.xml`).catch((F) => n.aviso(F.message, "err")), children: "Facturae" }),
            /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "factura", pantalla: "editor", semilla: v }), children: "Duplicar" }),
            k && l.tipo === "Ordinaria" && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "factura", pantalla: "editor", semilla: { ...v, rectificaId: l.id, rectificaNumero: l.numeroCompleto } }), children: "Rectificar" }),
            k && /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", onClick: () => x(!0), children: "Anular" })
          ] })
        }
      ),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ a.jsxs(ye, { etiqueta: "Cliente", children: [
          /* @__PURE__ */ a.jsx("strong", { children: l.clienteNombre }),
          l.clienteNif && /* @__PURE__ */ a.jsx("div", { className: "muted mono", children: l.clienteNif }),
          /* @__PURE__ */ a.jsx("div", { className: "muted", children: [l.clienteCalle, l.clienteCodigoPostal, l.clientePoblacion].filter(Boolean).join(" ") })
        ] }),
        /* @__PURE__ */ a.jsxs(ye, { etiqueta: "Emisión", children: [
          at(l.fechaEmision),
          l.fechaOperacion !== l.fechaEmision && /* @__PURE__ */ a.jsxs("div", { className: "muted", children: [
            "Operación ",
            at(l.fechaOperacion)
          ] })
        ] }),
        /* @__PURE__ */ a.jsx(ye, { etiqueta: "Vencimiento", children: at(l.fechaVencimiento) }),
        /* @__PURE__ */ a.jsxs(ye, { etiqueta: "Cobro", children: [
          s ? s.pendiente <= 0 ? /* @__PURE__ */ a.jsx("span", { className: "pill ok", children: "Cobrada" }) : /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            "Pendiente ",
            /* @__PURE__ */ a.jsx("strong", { children: $(s.pendiente) }),
            s.liquidado > 0 && /* @__PURE__ */ a.jsxs("div", { className: "muted", children: [
              "Cobrado ",
              $(s.liquidado)
            ] })
          ] }) : "—",
          s && s.pendiente > 0 && k && n.irA && /* @__PURE__ */ a.jsx("div", { children: /* @__PURE__ */ a.jsx(jr, { alPulsar: () => n.irA("cobros"), children: "Registrar cobro" }) })
        ] })
      ] }),
      l.motivoRectificacion && /* @__PURE__ */ a.jsxs("p", { className: "muted", children: [
        "Rectifica: ",
        l.motivoRectificacion,
        l.rectificaFacturaId && /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          " · ",
          /* @__PURE__ */ a.jsx(jr, { alPulsar: () => r({ tipo: "factura", pantalla: "vista", id: l.rectificaFacturaId }), children: "ver la factura original" })
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
        /* @__PURE__ */ a.jsx("tbody", { children: l.lineas.map((F, f) => /* @__PURE__ */ a.jsxs("tr", { children: [
          /* @__PURE__ */ a.jsxs("td", { children: [
            F.descripcion,
            /* @__PURE__ */ a.jsx(Hl, { conceptos: F.conceptos })
          ] }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: Se(F.cantidad) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: $(F.precioUnitario) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: F.porcentajeDescuento ? `${Be(F.porcentajeDescuento)} %` : "" }),
          /* @__PURE__ */ a.jsxs("td", { children: [
            F.codigoIva,
            " · ",
            Be(F.porcentajeIva),
            " %"
          ] }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: /* @__PURE__ */ a.jsx("strong", { children: $(F.base) }) }),
          /* @__PURE__ */ a.jsx("td", { className: "num muted", children: F.costeUnitario || F.costeConceptos ? $(F.margen) : "" })
        ] }, f)) })
      ] }),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-totales-vista", children: [
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Base imponible" }),
          /* @__PURE__ */ a.jsx("span", { children: $(l.baseImponible) })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: l.impuesto === "Igic" ? "IGIC" : "IVA" }),
          /* @__PURE__ */ a.jsx("span", { children: $(l.cuotaIva) })
        ] }),
        !!l.recargoTotal && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Recargo de equivalencia" }),
          /* @__PURE__ */ a.jsx("span", { children: $(l.recargoTotal) })
        ] }),
        !!l.retencionIrpf && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsxs("span", { className: "muted", children: [
            "Retención IRPF (",
            Be(l.porcentajeIrpf),
            " %)"
          ] }),
          /* @__PURE__ */ a.jsxs("span", { children: [
            "−",
            $(l.retencionIrpf)
          ] })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ a.jsx("span", { children: "Total" }),
          /* @__PURE__ */ a.jsx("span", { children: $(l.total) })
        ] }),
        g > 0 && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Margen" }),
          /* @__PURE__ */ a.jsxs("span", { className: "muted", children: [
            $(l.baseImponible - g),
            " (",
            Be(l.baseImponible ? (l.baseImponible - g) / l.baseImponible * 100 : 0),
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
      In,
      {
        titulo: `Anular ${l.numeroCompleto}`,
        alCerrar: () => x(!1),
        acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => x(!1), children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", disabled: !c.trim(), onClick: async () => await Ze(() => t.post(`/facturas/${l.id}/anular`, { motivo: c }), n.aviso, "Factura anulada.") && (x(!1), o()), children: "Anular" })
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
function Qp(e) {
  const { api: t, anfitrion: n, navegar: r } = $t(), { dato: l, error: i, recargar: o } = Ql(() => t.get(`/presupuestos/${e.id}`));
  if (i) return /* @__PURE__ */ a.jsx("div", { className: "panel", children: /* @__PURE__ */ a.jsx("p", { className: "dx-rojo", children: i }) });
  if (!l) return /* @__PURE__ */ a.jsx("div", { className: "muted", children: "Cargando…" });
  const s = l.estado === "Borrador", u = { clienteId: l.clienteId, lineas: l.lineas };
  return /* @__PURE__ */ a.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsx(
        Wl,
        {
          titulo: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            "Presupuesto ",
            /* @__PURE__ */ a.jsx("span", { className: "mono", children: l.numeroCompleto })
          ] }),
          estado: l.estado,
          volver: () => r({ tipo: "presupuesto", pantalla: "lista" }),
          acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => ma(n, `/presupuestos/${l.id}/pdf`).catch((d) => n.aviso(d.message, "err")), children: "PDF" }),
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
            s && /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", onClick: async () => await Ze(() => t.post(`/presupuestos/${l.id}/rechazar`, {}), n.aviso, "Presupuesto rechazado.") && o(), children: "Rechazar" })
          ] })
        }
      ),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ a.jsx(ye, { etiqueta: "Cliente", children: /* @__PURE__ */ a.jsx("strong", { children: l.clienteNombre }) }),
        /* @__PURE__ */ a.jsx(ye, { etiqueta: "Fecha", children: at(l.fecha) }),
        /* @__PURE__ */ a.jsx(ye, { etiqueta: "Válido hasta", children: at(l.validez) }),
        /* @__PURE__ */ a.jsx(ye, { etiqueta: "Factura", children: l.facturaId ? /* @__PURE__ */ a.jsx(jr, { alPulsar: () => r({ tipo: "factura", pantalla: "vista", id: l.facturaId }), children: "ver la factura" }) : "—" })
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
        /* @__PURE__ */ a.jsx("tbody", { children: l.lineas.map((d, x) => /* @__PURE__ */ a.jsxs("tr", { children: [
          /* @__PURE__ */ a.jsxs("td", { children: [
            d.descripcion,
            /* @__PURE__ */ a.jsx(Hl, { conceptos: d.conceptos })
          ] }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: Se(d.cantidad) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: $(d.precioUnitario) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: d.porcentajeDescuento ? `${Be(d.porcentajeDescuento)} %` : "" }),
          /* @__PURE__ */ a.jsx("td", { children: d.codigoIva }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: /* @__PURE__ */ a.jsx("strong", { children: $(d.base) }) })
        ] }, x)) })
      ] }),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-totales-vista", children: [
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Base imponible" }),
          /* @__PURE__ */ a.jsx("span", { children: $(l.baseImponible) })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Impuestos" }),
          /* @__PURE__ */ a.jsx("span", { children: $(l.cuotaIva) })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ a.jsx("span", { children: "Total" }),
          /* @__PURE__ */ a.jsx("span", { children: $(l.total) })
        ] })
      ] })
    ] })
  ] });
}
function Kp(e) {
  const { api: t, anfitrion: n, navegar: r } = $t(), { dato: l, error: i, recargar: o } = Ql(() => t.get(`/pedidos-venta/${e.id}`)), [s, u] = y.useState([]), [d, x] = y.useState([]), [c, m] = y.useState(null), [v, g] = y.useState(""), [k, F] = y.useState(_n()), [f, p] = y.useState(!1), [h, w] = y.useState(_n()), [I, _] = y.useState("");
  if (y.useEffect(() => void t.get(`/pedidos-venta/${e.id}/albaranes`).then(u).catch(() => u([])), [t, e.id, l]), y.useEffect(() => void t.get("/formas-pago").then((P) => x(P.filter((Q) => Q.activo))).catch(() => x([])), [t]), i) return /* @__PURE__ */ a.jsx("div", { className: "panel", children: /* @__PURE__ */ a.jsx("p", { className: "dx-rojo", children: i }) });
  if (!l) return /* @__PURE__ */ a.jsx("div", { className: "muted", children: "Cargando…" });
  const L = (l.estado === "Borrador" || l.estado === "Confirmado") && l.lineas.every((P) => P.cantidadServida === 0), N = l.lineas.some((P) => P.pendienteServir > 0), z = l.estado !== "Cancelado" && l.estado !== "Facturado", M = { clienteId: l.clienteId, fecha: l.fecha, lineas: l.lineas };
  return /* @__PURE__ */ a.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsx(
        Wl,
        {
          titulo: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            "Pedido de venta ",
            /* @__PURE__ */ a.jsx("span", { className: "mono", children: l.numeroCompleto })
          ] }),
          estado: l.estado,
          volver: () => r({ tipo: "pedido", pantalla: "lista" }),
          acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "pedido", pantalla: "editor", semilla: { ...M, fecha: void 0 } }), children: "Duplicar" }),
            L && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "pedido", pantalla: "editor", id: l.id, semilla: M }), children: "Editar" }),
            l.estado === "Borrador" && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: async () => await Ze(() => t.post(`/pedidos-venta/${l.id}/confirmar`), n.aviso, "Pedido confirmado.") && o(), children: "Confirmar" }),
            z && l.estado !== "Borrador" && N && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => m(Object.fromEntries(l.lineas.map((P) => [P.id, P.pendienteServir]))), children: "Entregar (albarán)" }),
            z && l.estado !== "Borrador" && /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: () => p(!0), children: "Facturar" }),
            z && /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", onClick: async () => window.confirm("¿Cancelar el pedido?") && await Ze(() => t.post(`/pedidos-venta/${l.id}/cancelar`), n.aviso, "Pedido cancelado.") && o(), children: "Cancelar" })
          ] })
        }
      ),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ a.jsx(ye, { etiqueta: "Cliente", children: /* @__PURE__ */ a.jsx("strong", { children: l.clienteNombre }) }),
        /* @__PURE__ */ a.jsx(ye, { etiqueta: "Fecha", children: at(l.fecha) }),
        /* @__PURE__ */ a.jsx(ye, { etiqueta: "Viene de", children: l.presupuestoOrigenId ? /* @__PURE__ */ a.jsx(jr, { alPulsar: () => r({ tipo: "presupuesto", pantalla: "vista", id: l.presupuestoOrigenId }), children: "presupuesto" }) : "—" }),
        /* @__PURE__ */ a.jsx(ye, { etiqueta: "Factura", children: l.facturaId ? /* @__PURE__ */ a.jsx(jr, { alPulsar: () => r({ tipo: "factura", pantalla: "vista", id: l.facturaId }), children: "ver la factura" }) : "—" })
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
        /* @__PURE__ */ a.jsx("tbody", { children: l.lineas.map((P) => /* @__PURE__ */ a.jsxs("tr", { children: [
          /* @__PURE__ */ a.jsxs("td", { children: [
            P.descripcion,
            /* @__PURE__ */ a.jsx(Hl, { conceptos: P.conceptos })
          ] }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: Se(P.cantidad) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: Se(P.cantidadServida) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: P.pendienteServir > 0 ? /* @__PURE__ */ a.jsx("strong", { children: Se(P.pendienteServir) }) : "—" }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: $(P.precioUnitario) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: P.porcentajeDescuento ? `${Be(P.porcentajeDescuento)} %` : "" }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: /* @__PURE__ */ a.jsx("strong", { children: $(P.base) }) })
        ] }, P.id)) })
      ] }),
      /* @__PURE__ */ a.jsx("div", { className: "dx-totales-vista", children: /* @__PURE__ */ a.jsxs("div", { className: "dx-tot dx-grande", children: [
        /* @__PURE__ */ a.jsx("span", { children: "Total (sin impuestos)" }),
        /* @__PURE__ */ a.jsx("span", { children: $(l.total) })
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
        /* @__PURE__ */ a.jsx("tbody", { children: s.map((P) => /* @__PURE__ */ a.jsxs("tr", { children: [
          /* @__PURE__ */ a.jsxs("td", { className: "mono", children: [
            /* @__PURE__ */ a.jsx("strong", { children: P.numeroCompleto }),
            " ",
            P.anulado && /* @__PURE__ */ a.jsx("span", { className: "pill neg", title: P.motivoAnulacion ?? "", children: "Anulado" })
          ] }),
          /* @__PURE__ */ a.jsx("td", { children: at(P.fecha) }),
          /* @__PURE__ */ a.jsx("td", { className: "muted", children: P.referencia }),
          /* @__PURE__ */ a.jsx("td", { className: "muted", children: P.lineas.map((Q) => `${Se(Q.cantidad)} × ${Q.descripcion}`).join(" · ") }),
          /* @__PURE__ */ a.jsx("td", { className: "right", children: !P.anulado && l.estado !== "Facturado" && /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", onClick: async () => {
            const Q = window.prompt("Motivo de la anulación del albarán:");
            Q !== null && await Ze(() => t.post(`/pedidos-venta/${l.id}/albaranes/${P.id}/anular`, { motivo: Q || null }), n.aviso, "Albarán anulado.") && o();
          }, children: "Anular" }) })
        ] }, P.id)) })
      ] }) : /* @__PURE__ */ a.jsx("p", { className: "muted", style: { margin: 0 }, children: "Sin entregas todavía." })
    ] }),
    c && /* @__PURE__ */ a.jsxs(
      In,
      {
        titulo: "Entrega (albarán de venta)",
        alCerrar: () => m(null),
        ancho: 640,
        acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => m(null), children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: async () => await Ze(() => t.post(`/pedidos-venta/${l.id}/entregar`, { fecha: k, referencia: v || null, lineas: Object.entries(c).filter(([, P]) => P > 0).map(([P, Q]) => ({ lineaPedidoId: P, cantidad: Q })) }), n.aviso, "Albarán creado.") && (m(null), o()), children: "Crear albarán" })
        ] }),
        children: [
          /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Fecha" }),
              /* @__PURE__ */ a.jsx("input", { type: "date", value: k, onChange: (P) => F(P.target.value) })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Referencia" }),
              /* @__PURE__ */ a.jsx("input", { value: v, onChange: (P) => g(P.target.value) })
            ] })
          ] }),
          /* @__PURE__ */ a.jsxs("table", { style: { marginTop: 10 }, children: [
            /* @__PURE__ */ a.jsx("thead", { children: /* @__PURE__ */ a.jsxs("tr", { children: [
              /* @__PURE__ */ a.jsx("th", { children: "Línea" }),
              /* @__PURE__ */ a.jsx("th", { className: "num", children: "Pendiente" }),
              /* @__PURE__ */ a.jsx("th", { className: "num", style: { width: 120 }, children: "Entregar" })
            ] }) }),
            /* @__PURE__ */ a.jsx("tbody", { children: l.lineas.filter((P) => P.pendienteServir > 0).map((P) => /* @__PURE__ */ a.jsxs("tr", { children: [
              /* @__PURE__ */ a.jsx("td", { children: P.descripcion }),
              /* @__PURE__ */ a.jsx("td", { className: "num", children: Se(P.pendienteServir) }),
              /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx("input", { className: "num", type: "number", step: "0.001", value: c[P.id] ?? 0, onChange: (Q) => m({ ...c, [P.id]: Number(Q.target.value) }) }) })
            ] }, P.id)) })
          ] })
        ]
      }
    ),
    f && /* @__PURE__ */ a.jsxs(
      In,
      {
        titulo: `Facturar el pedido ${l.numeroCompleto}`,
        alCerrar: () => p(!1),
        acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => p(!1), children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: async () => {
            try {
              const P = await t.post(`/pedidos-venta/${l.id}/facturar`, { fechaEmision: h, formaPagoId: I || null });
              n.aviso("Factura emitida.", "ok"), r({ tipo: "factura", pantalla: "vista", id: P.id });
            } catch (P) {
              n.aviso(P.message, "err");
            }
          }, children: "Emitir factura" })
        ] }),
        children: [
          /* @__PURE__ */ a.jsx("p", { className: "muted", style: { marginTop: 0 }, children: "Se factura el pedido completo con sus precios y conceptos. La factura queda numerada y encadenada en VeriFactu." }),
          /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Fecha de emisión" }),
              /* @__PURE__ */ a.jsx("input", { type: "date", value: h, onChange: (P) => w(P.target.value) })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Forma de pago" }),
              /* @__PURE__ */ a.jsxs("select", { value: I, onChange: (P) => _(P.target.value), children: [
                /* @__PURE__ */ a.jsx("option", { value: "", children: "La del cliente" }),
                d.map((P) => /* @__PURE__ */ a.jsx("option", { value: P.id, children: P.nombre }, P.id))
              ] })
            ] })
          ] })
        ]
      }
    )
  ] });
}
function Gp(e) {
  const { api: t, anfitrion: n, navegar: r } = $t(), { dato: l, error: i, recargar: o } = Ql(() => t.get(`/compras/pedidos/${e.id}`)), [s, u] = y.useState([]), [d, x] = y.useState([]), [c, m] = y.useState([]), [v, g] = y.useState(null), [k, F] = y.useState(""), [f, p] = y.useState(""), [h, w] = y.useState(_n()), [I, _] = y.useState(!1), [L, N] = y.useState("IVA21"), [z, M] = y.useState(0);
  if (y.useEffect(() => void t.get(`/compras/pedidos/${e.id}/albaranes`).then(u).catch(() => u([])), [t, e.id, l]), y.useEffect(() => {
    t.get("/inventario/almacenes").then((T) => (x(T), T[0] && F(T[0].id))).catch(() => x([])), t.get("/tipos-iva").then((T) => m(T.filter((W) => W.activo))).catch(() => m([]));
  }, [t]), i) return /* @__PURE__ */ a.jsx("div", { className: "panel", children: /* @__PURE__ */ a.jsx("p", { className: "dx-rojo", children: i }) });
  if (!l) return /* @__PURE__ */ a.jsx("div", { className: "muted", children: "Cargando…" });
  const P = (l.estado === "Borrador" || l.estado === "Confirmado") && l.lineas.every((T) => T.cantidadRecibida === 0 && T.cantidadFacturada === 0) && !l.empresaOrigenId, Q = l.estado !== "Cancelado" && l.estado !== "Facturado", Ke = l.lineas.some((T) => T.pendienteRecibir > 0), $e = l.lineas.reduce((T, W) => T + W.costeConceptos, 0);
  return /* @__PURE__ */ a.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsx(
        Wl,
        {
          titulo: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            "Pedido de compra ",
            /* @__PURE__ */ a.jsx("span", { className: "mono", children: l.numeroCompleto })
          ] }),
          estado: l.estado,
          volver: () => r({ tipo: "compra", pantalla: "lista" }),
          acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            !l.empresaOrigenId && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "compra", pantalla: "editor", semilla: { ...l, fecha: _n() } }), children: "Duplicar" }),
            P && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "compra", pantalla: "editor", id: l.id, semilla: l }), children: "Editar" }),
            l.estado === "Borrador" && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: async () => await Ze(() => t.post(`/compras/pedidos/${l.id}/confirmar`), n.aviso, "Pedido confirmado.") && o(), children: "Confirmar" }),
            Q && l.estado !== "Borrador" && Ke && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => g(Object.fromEntries(l.lineas.map((T) => [T.id, { cantidad: T.pendienteRecibir, lote: "" }]))), children: "Recibir mercancía" }),
            Q && l.estado !== "Borrador" && /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: () => _(!0), children: "Facturar" }),
            Q && !l.empresaOrigenId && /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", onClick: async () => window.confirm("¿Cancelar el pedido?") && await Ze(() => t.post(`/compras/pedidos/${l.id}/cancelar`), n.aviso, "Pedido cancelado.") && o(), children: "Cancelar" })
          ] })
        }
      ),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ a.jsx(ye, { etiqueta: "Proveedor", children: /* @__PURE__ */ a.jsx("strong", { children: l.proveedorTexto }) }),
        /* @__PURE__ */ a.jsx(ye, { etiqueta: "Fecha", children: at(l.fecha) }),
        /* @__PURE__ */ a.jsx(ye, { etiqueta: "Total", children: $(l.total) }),
        /* @__PURE__ */ a.jsx(ye, { etiqueta: "Costes añadidos", children: $e ? $($e) : "—" })
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
      /* @__PURE__ */ a.jsx("tbody", { children: l.lineas.map((T) => /* @__PURE__ */ a.jsxs("tr", { children: [
        /* @__PURE__ */ a.jsxs("td", { children: [
          T.descripcion,
          /* @__PURE__ */ a.jsx(Hl, { conceptos: T.conceptos })
        ] }),
        /* @__PURE__ */ a.jsx("td", { className: "num", children: Se(T.cantidad) }),
        /* @__PURE__ */ a.jsx("td", { className: "num", children: Se(T.cantidadRecibida) }),
        /* @__PURE__ */ a.jsx("td", { className: "num", children: T.pendienteRecibir > 0 ? /* @__PURE__ */ a.jsx("strong", { children: Se(T.pendienteRecibir) }) : "—" }),
        /* @__PURE__ */ a.jsx("td", { className: "num", children: $(T.precioUnitario) }),
        /* @__PURE__ */ a.jsx("td", { className: "num", children: /* @__PURE__ */ a.jsx("strong", { children: $(T.importe) }) }),
        /* @__PURE__ */ a.jsxs("td", { className: "num muted", children: [
          $(T.costeUnitarioEntrada),
          "/ud"
        ] })
      ] }, T.id)) })
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
        /* @__PURE__ */ a.jsx("tbody", { children: s.map((T) => {
          var W;
          return /* @__PURE__ */ a.jsxs("tr", { children: [
            /* @__PURE__ */ a.jsxs("td", { className: "mono", children: [
              /* @__PURE__ */ a.jsx("strong", { children: T.numeroCompleto }),
              " ",
              T.anulado && /* @__PURE__ */ a.jsx("span", { className: "pill neg", title: T.motivoAnulacion ?? "", children: "Anulado" })
            ] }),
            /* @__PURE__ */ a.jsx("td", { children: at(T.fecha) }),
            /* @__PURE__ */ a.jsx("td", { className: "muted", children: T.referencia }),
            /* @__PURE__ */ a.jsx("td", { className: "muted", children: ((W = d.find((ne) => ne.id === T.almacenId)) == null ? void 0 : W.nombre) ?? "—" }),
            /* @__PURE__ */ a.jsx("td", { className: "muted", children: T.lineas.map((ne) => `${Se(ne.cantidad)} × ${ne.descripcion}`).join(" · ") }),
            /* @__PURE__ */ a.jsx("td", { className: "right", children: !T.anulado && l.estado !== "Facturado" && /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", onClick: async () => {
              const ne = window.prompt("Motivo de la anulación del albarán:");
              ne !== null && await Ze(() => t.post(`/compras/pedidos/${l.id}/albaranes/${T.id}/anular`, { motivo: ne || null }), n.aviso, "Albarán anulado.") && o();
            }, children: "Anular" }) })
          ] }, T.id);
        }) })
      ] }) : /* @__PURE__ */ a.jsx("p", { className: "muted", style: { margin: 0 }, children: "Sin recepciones todavía." })
    ] }),
    v && /* @__PURE__ */ a.jsxs(
      In,
      {
        titulo: "Recepción de mercancía",
        alCerrar: () => g(null),
        ancho: 680,
        acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => g(null), children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: async () => await Ze(() => t.post(`/compras/pedidos/${l.id}/recibir`, { fecha: h, referencia: f || null, almacenId: k || null, lineas: Object.entries(v).filter(([, T]) => T.cantidad > 0).map(([T, W]) => ({ lineaPedidoId: T, cantidad: W.cantidad, lote: W.lote || null })) }), n.aviso, "Recepción registrada.") && (g(null), o()), children: "Registrar recepción" })
        ] }),
        children: [
          /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Fecha" }),
              /* @__PURE__ */ a.jsx("input", { type: "date", value: h, onChange: (T) => w(T.target.value) })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Albarán del proveedor" }),
              /* @__PURE__ */ a.jsx("input", { value: f, onChange: (T) => p(T.target.value) })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Almacén" }),
              /* @__PURE__ */ a.jsxs("select", { value: k, onChange: (T) => F(T.target.value), children: [
                /* @__PURE__ */ a.jsx("option", { value: "", children: "Sin entrada en almacén" }),
                d.map((T) => /* @__PURE__ */ a.jsx("option", { value: T.id, children: T.nombre }, T.id))
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
            /* @__PURE__ */ a.jsx("tbody", { children: l.lineas.filter((T) => T.pendienteRecibir > 0).map((T) => {
              var W, ne;
              return /* @__PURE__ */ a.jsxs("tr", { children: [
                /* @__PURE__ */ a.jsx("td", { children: T.descripcion }),
                /* @__PURE__ */ a.jsx("td", { className: "num", children: Se(T.pendienteRecibir) }),
                /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx("input", { className: "num", type: "number", step: "0.001", value: ((W = v[T.id]) == null ? void 0 : W.cantidad) ?? 0, onChange: (j) => g({ ...v, [T.id]: { ...v[T.id], cantidad: Number(j.target.value) } }) }) }),
                /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx("input", { value: ((ne = v[T.id]) == null ? void 0 : ne.lote) ?? "", onChange: (j) => g({ ...v, [T.id]: { ...v[T.id], lote: j.target.value } }) }) })
              ] }, T.id);
            }) })
          ] })
        ]
      }
    ),
    I && /* @__PURE__ */ a.jsxs(
      In,
      {
        titulo: `Facturar el pedido ${l.numeroCompleto}`,
        alCerrar: () => _(!1),
        acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => _(!1), children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: async () => await Ze(() => t.post(`/compras/pedidos/${l.id}/facturar`, { codigoIva: L, porcentajeIrpf: z }), n.aviso, "Factura del proveedor registrada como gasto.") && (_(!1), o()), children: "Registrar factura" })
        ] }),
        children: [
          /* @__PURE__ */ a.jsxs("p", { className: "muted", style: { marginTop: 0 }, children: [
            "Registra la factura del proveedor por el total del pedido (",
            $(l.total),
            ") como gasto, con su asiento si la contabilidad es automática."
          ] }),
          /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Impuesto" }),
              /* @__PURE__ */ a.jsx("select", { value: L, onChange: (T) => N(T.target.value), children: c.map((T) => /* @__PURE__ */ a.jsx("option", { value: T.codigo, children: T.nombre }, T.codigo)) })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Retención IRPF %" }),
              /* @__PURE__ */ a.jsx("input", { type: "number", step: "0.01", value: z, onChange: (T) => M(Number(T.target.value)) })
            ] })
          ] })
        ]
      }
    )
  ] });
}
function Yp(e) {
  const [t, n] = y.useState(e.inicial), r = y.useRef(0), [l, i] = y.useState(0), o = y.useMemo(() => Tp(e.anfitrion), [e.anfitrion]), s = (c) => {
    n(c), i(++r.current), window.scrollTo({ top: 0 });
  }, u = { api: o, anfitrion: e.anfitrion, ruta: t, navegar: s }, d = `${t.tipo}-${t.pantalla}-${t.id ?? ""}-${l}`;
  let x;
  if (t.pantalla === "lista") x = /* @__PURE__ */ a.jsx($p, { tipo: t.tipo });
  else if (t.pantalla === "vista" && t.id)
    x = t.tipo === "factura" ? /* @__PURE__ */ a.jsx(Wp, { id: t.id }) : t.tipo === "presupuesto" ? /* @__PURE__ */ a.jsx(Qp, { id: t.id }) : t.tipo === "pedido" ? /* @__PURE__ */ a.jsx(Kp, { id: t.id }) : /* @__PURE__ */ a.jsx(Gp, { id: t.id });
  else if (t.tipo === "compra")
    x = /* @__PURE__ */ a.jsx(
      Hp,
      {
        id: t.id,
        semilla: t.semilla,
        alGuardar: (c) => s({ tipo: "compra", pantalla: "vista", id: c }),
        alCancelar: () => s(t.id ? { tipo: "compra", pantalla: "vista", id: t.id } : { tipo: "compra", pantalla: "lista" })
      }
    );
  else {
    const c = t.tipo;
    x = /* @__PURE__ */ a.jsx(
      Bp,
      {
        tipo: c,
        id: t.id,
        semilla: t.semilla,
        alGuardar: (m) => s({ tipo: c, pantalla: "vista", id: m }),
        alCancelar: () => s(t.id ? { tipo: c, pantalla: "vista", id: t.id } : { tipo: c, pantalla: "lista" })
      }
    );
  }
  return /* @__PURE__ */ a.jsx(Bc.Provider, { value: u, children: /* @__PURE__ */ a.jsx("div", { className: "dx-raiz", children: x }, d) });
}
const Xp = `
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
function qp() {
  if (document.getElementById("dx-estilos")) return;
  const e = document.createElement("style");
  e.id = "dx-estilos", e.textContent = Xp, document.head.appendChild(e);
}
function Zp(e, t, n) {
  qp();
  const r = Vc(e);
  return r.render(/* @__PURE__ */ a.jsx(Yp, { anfitrion: t, inicial: n })), () => r.unmount();
}
export {
  Zp as montar
};
