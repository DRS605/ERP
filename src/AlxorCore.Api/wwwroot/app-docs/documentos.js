var Gs = { exports: {} }, Gl = {}, Ks = { exports: {} }, G = {};
/**
 * @license React
 * react.production.min.js
 *
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */
var Vr = Symbol.for("react.element"), fd = Symbol.for("react.portal"), pd = Symbol.for("react.fragment"), md = Symbol.for("react.strict_mode"), hd = Symbol.for("react.profiler"), vd = Symbol.for("react.provider"), gd = Symbol.for("react.context"), xd = Symbol.for("react.forward_ref"), yd = Symbol.for("react.suspense"), jd = Symbol.for("react.memo"), Nd = Symbol.for("react.lazy"), Do = Symbol.iterator;
function wd(e) {
  return e === null || typeof e != "object" ? null : (e = Do && e[Do] || e["@@iterator"], typeof e == "function" ? e : null);
}
var qs = { isMounted: function() {
  return !1;
}, enqueueForceUpdate: function() {
}, enqueueReplaceState: function() {
}, enqueueSetState: function() {
} }, Ys = Object.assign, Xs = {};
function Xn(e, t, n) {
  this.props = e, this.context = t, this.refs = Xs, this.updater = n || qs;
}
Xn.prototype.isReactComponent = {};
Xn.prototype.setState = function(e, t) {
  if (typeof e != "object" && typeof e != "function" && e != null) throw Error("setState(...): takes an object of state variables to update or a function which returns an object of state variables.");
  this.updater.enqueueSetState(this, e, t, "setState");
};
Xn.prototype.forceUpdate = function(e) {
  this.updater.enqueueForceUpdate(this, e, "forceUpdate");
};
function Zs() {
}
Zs.prototype = Xn.prototype;
function Mi(e, t, n) {
  this.props = e, this.context = t, this.refs = Xs, this.updater = n || qs;
}
var $i = Mi.prototype = new Zs();
$i.constructor = Mi;
Ys($i, Xn.prototype);
$i.isPureReactComponent = !0;
var Mo = Array.isArray, Js = Object.prototype.hasOwnProperty, Oi = { current: null }, eu = { key: !0, ref: !0, __self: !0, __source: !0 };
function tu(e, t, n) {
  var r, l = {}, i = null, o = null;
  if (t != null) for (r in t.ref !== void 0 && (o = t.ref), t.key !== void 0 && (i = "" + t.key), t) Js.call(t, r) && !eu.hasOwnProperty(r) && (l[r] = t[r]);
  var s = arguments.length - 2;
  if (s === 1) l.children = n;
  else if (1 < s) {
    for (var u = Array(s), d = 0; d < s; d++) u[d] = arguments[d + 2];
    l.children = u;
  }
  if (e && e.defaultProps) for (r in s = e.defaultProps, s) l[r] === void 0 && (l[r] = s[r]);
  return { $$typeof: Vr, type: e, key: i, ref: o, props: l, _owner: Oi.current };
}
function Sd(e, t) {
  return { $$typeof: Vr, type: e.type, key: t, ref: e.ref, props: e.props, _owner: e._owner };
}
function Ai(e) {
  return typeof e == "object" && e !== null && e.$$typeof === Vr;
}
function kd(e) {
  var t = { "=": "=0", ":": "=2" };
  return "$" + e.replace(/[=:]/g, function(n) {
    return t[n];
  });
}
var $o = /\/+/g;
function pa(e, t) {
  return typeof e == "object" && e !== null && e.key != null ? kd("" + e.key) : t.toString(36);
}
function dl(e, t, n, r, l) {
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
        case Vr:
        case fd:
          o = !0;
      }
  }
  if (o) return o = e, l = l(o), e = r === "" ? "." + pa(o, 0) : r, Mo(l) ? (n = "", e != null && (n = e.replace($o, "$&/") + "/"), dl(l, t, n, "", function(d) {
    return d;
  })) : l != null && (Ai(l) && (l = Sd(l, n + (!l.key || o && o.key === l.key ? "" : ("" + l.key).replace($o, "$&/") + "/") + e)), t.push(l)), 1;
  if (o = 0, r = r === "" ? "." : r + ":", Mo(e)) for (var s = 0; s < e.length; s++) {
    i = e[s];
    var u = r + pa(i, s);
    o += dl(i, t, n, u, l);
  }
  else if (u = wd(e), typeof u == "function") for (e = u.call(e), s = 0; !(i = e.next()).done; ) i = i.value, u = r + pa(i, s++), o += dl(i, t, n, u, l);
  else if (i === "object") throw t = String(e), Error("Objects are not valid as a React child (found: " + (t === "[object Object]" ? "object with keys {" + Object.keys(e).join(", ") + "}" : t) + "). If you meant to render a collection of children, use an array instead.");
  return o;
}
function Kr(e, t, n) {
  if (e == null) return e;
  var r = [], l = 0;
  return dl(e, r, "", "", function(i) {
    return t.call(n, i, l++);
  }), r;
}
function Cd(e) {
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
var Ae = { current: null }, fl = { transition: null }, Ed = { ReactCurrentDispatcher: Ae, ReactCurrentBatchConfig: fl, ReactCurrentOwner: Oi };
function nu() {
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
  if (!Ai(e)) throw Error("React.Children.only expected to receive a single React element child.");
  return e;
} };
G.Component = Xn;
G.Fragment = pd;
G.Profiler = hd;
G.PureComponent = Mi;
G.StrictMode = md;
G.Suspense = yd;
G.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED = Ed;
G.act = nu;
G.cloneElement = function(e, t, n) {
  if (e == null) throw Error("React.cloneElement(...): The argument must be a React element, but you passed " + e + ".");
  var r = Ys({}, e.props), l = e.key, i = e.ref, o = e._owner;
  if (t != null) {
    if (t.ref !== void 0 && (i = t.ref, o = Oi.current), t.key !== void 0 && (l = "" + t.key), e.type && e.type.defaultProps) var s = e.type.defaultProps;
    for (u in t) Js.call(t, u) && !eu.hasOwnProperty(u) && (r[u] = t[u] === void 0 && s !== void 0 ? s[u] : t[u]);
  }
  var u = arguments.length - 2;
  if (u === 1) r.children = n;
  else if (1 < u) {
    s = Array(u);
    for (var d = 0; d < u; d++) s[d] = arguments[d + 2];
    r.children = s;
  }
  return { $$typeof: Vr, type: e.type, key: l, ref: i, props: r, _owner: o };
};
G.createContext = function(e) {
  return e = { $$typeof: gd, _currentValue: e, _currentValue2: e, _threadCount: 0, Provider: null, Consumer: null, _defaultValue: null, _globalName: null }, e.Provider = { $$typeof: vd, _context: e }, e.Consumer = e;
};
G.createElement = tu;
G.createFactory = function(e) {
  var t = tu.bind(null, e);
  return t.type = e, t;
};
G.createRef = function() {
  return { current: null };
};
G.forwardRef = function(e) {
  return { $$typeof: xd, render: e };
};
G.isValidElement = Ai;
G.lazy = function(e) {
  return { $$typeof: Nd, _payload: { _status: -1, _result: e }, _init: Cd };
};
G.memo = function(e, t) {
  return { $$typeof: jd, type: e, compare: t === void 0 ? null : t };
};
G.startTransition = function(e) {
  var t = fl.transition;
  fl.transition = {};
  try {
    e();
  } finally {
    fl.transition = t;
  }
};
G.unstable_act = nu;
G.useCallback = function(e, t) {
  return Ae.current.useCallback(e, t);
};
G.useContext = function(e) {
  return Ae.current.useContext(e);
};
G.useDebugValue = function() {
};
G.useDeferredValue = function(e) {
  return Ae.current.useDeferredValue(e);
};
G.useEffect = function(e, t) {
  return Ae.current.useEffect(e, t);
};
G.useId = function() {
  return Ae.current.useId();
};
G.useImperativeHandle = function(e, t, n) {
  return Ae.current.useImperativeHandle(e, t, n);
};
G.useInsertionEffect = function(e, t) {
  return Ae.current.useInsertionEffect(e, t);
};
G.useLayoutEffect = function(e, t) {
  return Ae.current.useLayoutEffect(e, t);
};
G.useMemo = function(e, t) {
  return Ae.current.useMemo(e, t);
};
G.useReducer = function(e, t, n) {
  return Ae.current.useReducer(e, t, n);
};
G.useRef = function(e) {
  return Ae.current.useRef(e);
};
G.useState = function(e) {
  return Ae.current.useState(e);
};
G.useSyncExternalStore = function(e, t, n) {
  return Ae.current.useSyncExternalStore(e, t, n);
};
G.useTransition = function() {
  return Ae.current.useTransition();
};
G.version = "18.3.1";
Ks.exports = G;
var x = Ks.exports;
/**
 * @license React
 * react-jsx-runtime.production.min.js
 *
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */
var Id = x, Pd = Symbol.for("react.element"), Fd = Symbol.for("react.fragment"), _d = Object.prototype.hasOwnProperty, Rd = Id.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED.ReactCurrentOwner, Td = { key: !0, ref: !0, __self: !0, __source: !0 };
function ru(e, t, n) {
  var r, l = {}, i = null, o = null;
  n !== void 0 && (i = "" + n), t.key !== void 0 && (i = "" + t.key), t.ref !== void 0 && (o = t.ref);
  for (r in t) _d.call(t, r) && !Td.hasOwnProperty(r) && (l[r] = t[r]);
  if (e && e.defaultProps) for (r in t = e.defaultProps, t) l[r] === void 0 && (l[r] = t[r]);
  return { $$typeof: Pd, type: e, key: i, ref: o, props: l, _owner: Rd.current };
}
Gl.Fragment = Fd;
Gl.jsx = ru;
Gl.jsxs = ru;
Gs.exports = Gl;
var a = Gs.exports, lu = { exports: {} }, Je = {}, au = { exports: {} }, iu = {};
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
  function t(C, j) {
    var S = C.length;
    C.push(j);
    e: for (; 0 < S; ) {
      var V = S - 1 >>> 1, W = C[V];
      if (0 < l(W, j)) C[V] = j, C[S] = W, S = V;
      else break e;
    }
  }
  function n(C) {
    return C.length === 0 ? null : C[0];
  }
  function r(C) {
    if (C.length === 0) return null;
    var j = C[0], S = C.pop();
    if (S !== j) {
      C[0] = S;
      e: for (var V = 0, W = C.length, K = W >>> 1; V < K; ) {
        var fe = 2 * (V + 1) - 1, pe = C[fe], J = fe + 1, O = C[J];
        if (0 > l(pe, S)) J < W && 0 > l(O, pe) ? (C[V] = O, C[J] = S, V = J) : (C[V] = pe, C[fe] = S, V = fe);
        else if (J < W && 0 > l(O, S)) C[V] = O, C[J] = S, V = J;
        else break e;
      }
    }
    return j;
  }
  function l(C, j) {
    var S = C.sortIndex - j.sortIndex;
    return S !== 0 ? S : C.id - j.id;
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
  var u = [], d = [], N = 1, c = null, h = 3, v = !1, y = !1, w = !1, _ = typeof setTimeout == "function" ? setTimeout : null, f = typeof clearTimeout == "function" ? clearTimeout : null, p = typeof setImmediate < "u" ? setImmediate : null;
  typeof navigator < "u" && navigator.scheduling !== void 0 && navigator.scheduling.isInputPending !== void 0 && navigator.scheduling.isInputPending.bind(navigator.scheduling);
  function g(C) {
    for (var j = n(d); j !== null; ) {
      if (j.callback === null) r(d);
      else if (j.startTime <= C) r(d), j.sortIndex = j.expirationTime, t(u, j);
      else break;
      j = n(d);
    }
  }
  function k(C) {
    if (w = !1, g(C), !y) if (n(u) !== null) y = !0, de(T);
    else {
      var j = n(d);
      j !== null && ge(k, j.startTime - C);
    }
  }
  function T(C, j) {
    y = !1, w && (w = !1, f(M), M = -1), v = !0;
    var S = h;
    try {
      for (g(j), c = n(u); c !== null && (!(c.expirationTime > j) || C && !F()); ) {
        var V = c.callback;
        if (typeof V == "function") {
          c.callback = null, h = c.priorityLevel;
          var W = V(c.expirationTime <= j);
          j = e.unstable_now(), typeof W == "function" ? c.callback = W : c === n(u) && r(u), g(j);
        } else r(u);
        c = n(u);
      }
      if (c !== null) var K = !0;
      else {
        var fe = n(d);
        fe !== null && ge(k, fe.startTime - j), K = !1;
      }
      return K;
    } finally {
      c = null, h = S, v = !1;
    }
  }
  var R = !1, z = null, M = -1, A = 5, U = -1;
  function F() {
    return !(e.unstable_now() - U < A);
  }
  function Q() {
    if (z !== null) {
      var C = e.unstable_now();
      U = C;
      var j = !0;
      try {
        j = z(!0, C);
      } finally {
        j ? oe() : (R = !1, z = null);
      }
    } else R = !1;
  }
  var oe;
  if (typeof p == "function") oe = function() {
    p(Q);
  };
  else if (typeof MessageChannel < "u") {
    var Fe = new MessageChannel(), De = Fe.port2;
    Fe.port1.onmessage = Q, oe = function() {
      De.postMessage(null);
    };
  } else oe = function() {
    _(Q, 0);
  };
  function de(C) {
    z = C, R || (R = !0, oe());
  }
  function ge(C, j) {
    M = _(function() {
      C(e.unstable_now());
    }, j);
  }
  e.unstable_IdlePriority = 5, e.unstable_ImmediatePriority = 1, e.unstable_LowPriority = 4, e.unstable_NormalPriority = 3, e.unstable_Profiling = null, e.unstable_UserBlockingPriority = 2, e.unstable_cancelCallback = function(C) {
    C.callback = null;
  }, e.unstable_continueExecution = function() {
    y || v || (y = !0, de(T));
  }, e.unstable_forceFrameRate = function(C) {
    0 > C || 125 < C ? console.error("forceFrameRate takes a positive int between 0 and 125, forcing frame rates higher than 125 fps is not supported") : A = 0 < C ? Math.floor(1e3 / C) : 5;
  }, e.unstable_getCurrentPriorityLevel = function() {
    return h;
  }, e.unstable_getFirstCallbackNode = function() {
    return n(u);
  }, e.unstable_next = function(C) {
    switch (h) {
      case 1:
      case 2:
      case 3:
        var j = 3;
        break;
      default:
        j = h;
    }
    var S = h;
    h = j;
    try {
      return C();
    } finally {
      h = S;
    }
  }, e.unstable_pauseExecution = function() {
  }, e.unstable_requestPaint = function() {
  }, e.unstable_runWithPriority = function(C, j) {
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
    var S = h;
    h = C;
    try {
      return j();
    } finally {
      h = S;
    }
  }, e.unstable_scheduleCallback = function(C, j, S) {
    var V = e.unstable_now();
    switch (typeof S == "object" && S !== null ? (S = S.delay, S = typeof S == "number" && 0 < S ? V + S : V) : S = V, C) {
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
    return W = S + W, C = { id: N++, callback: j, priorityLevel: C, startTime: S, expirationTime: W, sortIndex: -1 }, S > V ? (C.sortIndex = S, t(d, C), n(u) === null && C === n(d) && (w ? (f(M), M = -1) : w = !0, ge(k, S - V))) : (C.sortIndex = W, t(u, C), y || v || (y = !0, de(T))), C;
  }, e.unstable_shouldYield = F, e.unstable_wrapCallback = function(C) {
    var j = h;
    return function() {
      var S = h;
      h = j;
      try {
        return C.apply(this, arguments);
      } finally {
        h = S;
      }
    };
  };
})(iu);
au.exports = iu;
var zd = au.exports;
/**
 * @license React
 * react-dom.production.min.js
 *
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */
var Ld = x, Ze = zd;
function I(e) {
  for (var t = "https://reactjs.org/docs/error-decoder.html?invariant=" + e, n = 1; n < arguments.length; n++) t += "&args[]=" + encodeURIComponent(arguments[n]);
  return "Minified React error #" + e + "; visit " + t + " for the full message or use the non-minified dev environment for full errors and additional helpful warnings.";
}
var ou = /* @__PURE__ */ new Set(), Sr = {};
function wn(e, t) {
  Hn(e, t), Hn(e + "Capture", t);
}
function Hn(e, t) {
  for (Sr[e] = t, e = 0; e < t.length; e++) ou.add(t[e]);
}
var Rt = !(typeof window > "u" || typeof window.document > "u" || typeof window.document.createElement > "u"), Ba = Object.prototype.hasOwnProperty, Dd = /^[:A-Z_a-z\u00C0-\u00D6\u00D8-\u00F6\u00F8-\u02FF\u0370-\u037D\u037F-\u1FFF\u200C-\u200D\u2070-\u218F\u2C00-\u2FEF\u3001-\uD7FF\uF900-\uFDCF\uFDF0-\uFFFD][:A-Z_a-z\u00C0-\u00D6\u00D8-\u00F6\u00F8-\u02FF\u0370-\u037D\u037F-\u1FFF\u200C-\u200D\u2070-\u218F\u2C00-\u2FEF\u3001-\uD7FF\uF900-\uFDCF\uFDF0-\uFFFD\-.0-9\u00B7\u0300-\u036F\u203F-\u2040]*$/, Oo = {}, Ao = {};
function Md(e) {
  return Ba.call(Ao, e) ? !0 : Ba.call(Oo, e) ? !1 : Dd.test(e) ? Ao[e] = !0 : (Oo[e] = !0, !1);
}
function $d(e, t, n, r) {
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
function Od(e, t, n, r) {
  if (t === null || typeof t > "u" || $d(e, t, n, r)) return !0;
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
function Ue(e, t, n, r, l, i, o) {
  this.acceptsBooleans = t === 2 || t === 3 || t === 4, this.attributeName = r, this.attributeNamespace = l, this.mustUseProperty = n, this.propertyName = e, this.type = t, this.sanitizeURL = i, this.removeEmptyString = o;
}
var Pe = {};
"children dangerouslySetInnerHTML defaultValue defaultChecked innerHTML suppressContentEditableWarning suppressHydrationWarning style".split(" ").forEach(function(e) {
  Pe[e] = new Ue(e, 0, !1, e, null, !1, !1);
});
[["acceptCharset", "accept-charset"], ["className", "class"], ["htmlFor", "for"], ["httpEquiv", "http-equiv"]].forEach(function(e) {
  var t = e[0];
  Pe[t] = new Ue(t, 1, !1, e[1], null, !1, !1);
});
["contentEditable", "draggable", "spellCheck", "value"].forEach(function(e) {
  Pe[e] = new Ue(e, 2, !1, e.toLowerCase(), null, !1, !1);
});
["autoReverse", "externalResourcesRequired", "focusable", "preserveAlpha"].forEach(function(e) {
  Pe[e] = new Ue(e, 2, !1, e, null, !1, !1);
});
"allowFullScreen async autoFocus autoPlay controls default defer disabled disablePictureInPicture disableRemotePlayback formNoValidate hidden loop noModule noValidate open playsInline readOnly required reversed scoped seamless itemScope".split(" ").forEach(function(e) {
  Pe[e] = new Ue(e, 3, !1, e.toLowerCase(), null, !1, !1);
});
["checked", "multiple", "muted", "selected"].forEach(function(e) {
  Pe[e] = new Ue(e, 3, !0, e, null, !1, !1);
});
["capture", "download"].forEach(function(e) {
  Pe[e] = new Ue(e, 4, !1, e, null, !1, !1);
});
["cols", "rows", "size", "span"].forEach(function(e) {
  Pe[e] = new Ue(e, 6, !1, e, null, !1, !1);
});
["rowSpan", "start"].forEach(function(e) {
  Pe[e] = new Ue(e, 5, !1, e.toLowerCase(), null, !1, !1);
});
var Ui = /[\-:]([a-z])/g;
function Vi(e) {
  return e[1].toUpperCase();
}
"accent-height alignment-baseline arabic-form baseline-shift cap-height clip-path clip-rule color-interpolation color-interpolation-filters color-profile color-rendering dominant-baseline enable-background fill-opacity fill-rule flood-color flood-opacity font-family font-size font-size-adjust font-stretch font-style font-variant font-weight glyph-name glyph-orientation-horizontal glyph-orientation-vertical horiz-adv-x horiz-origin-x image-rendering letter-spacing lighting-color marker-end marker-mid marker-start overline-position overline-thickness paint-order panose-1 pointer-events rendering-intent shape-rendering stop-color stop-opacity strikethrough-position strikethrough-thickness stroke-dasharray stroke-dashoffset stroke-linecap stroke-linejoin stroke-miterlimit stroke-opacity stroke-width text-anchor text-decoration text-rendering underline-position underline-thickness unicode-bidi unicode-range units-per-em v-alphabetic v-hanging v-ideographic v-mathematical vector-effect vert-adv-y vert-origin-x vert-origin-y word-spacing writing-mode xmlns:xlink x-height".split(" ").forEach(function(e) {
  var t = e.replace(
    Ui,
    Vi
  );
  Pe[t] = new Ue(t, 1, !1, e, null, !1, !1);
});
"xlink:actuate xlink:arcrole xlink:role xlink:show xlink:title xlink:type".split(" ").forEach(function(e) {
  var t = e.replace(Ui, Vi);
  Pe[t] = new Ue(t, 1, !1, e, "http://www.w3.org/1999/xlink", !1, !1);
});
["xml:base", "xml:lang", "xml:space"].forEach(function(e) {
  var t = e.replace(Ui, Vi);
  Pe[t] = new Ue(t, 1, !1, e, "http://www.w3.org/XML/1998/namespace", !1, !1);
});
["tabIndex", "crossOrigin"].forEach(function(e) {
  Pe[e] = new Ue(e, 1, !1, e.toLowerCase(), null, !1, !1);
});
Pe.xlinkHref = new Ue("xlinkHref", 1, !1, "xlink:href", "http://www.w3.org/1999/xlink", !0, !1);
["src", "href", "action", "formAction"].forEach(function(e) {
  Pe[e] = new Ue(e, 1, !1, e.toLowerCase(), null, !0, !0);
});
function Bi(e, t, n, r) {
  var l = Pe.hasOwnProperty(t) ? Pe[t] : null;
  (l !== null ? l.type !== 0 : r || !(2 < t.length) || t[0] !== "o" && t[0] !== "O" || t[1] !== "n" && t[1] !== "N") && (Od(t, n, l, r) && (n = null), r || l === null ? Md(t) && (n === null ? e.removeAttribute(t) : e.setAttribute(t, "" + n)) : l.mustUseProperty ? e[l.propertyName] = n === null ? l.type === 3 ? !1 : "" : n : (t = l.attributeName, r = l.attributeNamespace, n === null ? e.removeAttribute(t) : (l = l.type, n = l === 3 || l === 4 && n === !0 ? "" : "" + n, r ? e.setAttributeNS(r, t, n) : e.setAttribute(t, n))));
}
var Dt = Ld.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED, qr = Symbol.for("react.element"), En = Symbol.for("react.portal"), In = Symbol.for("react.fragment"), bi = Symbol.for("react.strict_mode"), ba = Symbol.for("react.profiler"), su = Symbol.for("react.provider"), uu = Symbol.for("react.context"), Hi = Symbol.for("react.forward_ref"), Ha = Symbol.for("react.suspense"), Wa = Symbol.for("react.suspense_list"), Wi = Symbol.for("react.memo"), At = Symbol.for("react.lazy"), cu = Symbol.for("react.offscreen"), Uo = Symbol.iterator;
function tr(e) {
  return e === null || typeof e != "object" ? null : (e = Uo && e[Uo] || e["@@iterator"], typeof e == "function" ? e : null);
}
var ce = Object.assign, ma;
function ur(e) {
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
  return (e = e ? e.displayName || e.name : "") ? ur(e) : "";
}
function Ad(e) {
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
      return e = va(e.type, !1), e;
    case 11:
      return e = va(e.type.render, !1), e;
    case 1:
      return e = va(e.type, !0), e;
    default:
      return "";
  }
}
function Qa(e) {
  if (e == null) return null;
  if (typeof e == "function") return e.displayName || e.name || null;
  if (typeof e == "string") return e;
  switch (e) {
    case In:
      return "Fragment";
    case En:
      return "Portal";
    case ba:
      return "Profiler";
    case bi:
      return "StrictMode";
    case Ha:
      return "Suspense";
    case Wa:
      return "SuspenseList";
  }
  if (typeof e == "object") switch (e.$$typeof) {
    case uu:
      return (e.displayName || "Context") + ".Consumer";
    case su:
      return (e._context.displayName || "Context") + ".Provider";
    case Hi:
      var t = e.render;
      return e = e.displayName, e || (e = t.displayName || t.name || "", e = e !== "" ? "ForwardRef(" + e + ")" : "ForwardRef"), e;
    case Wi:
      return t = e.displayName || null, t !== null ? t : Qa(e.type) || "Memo";
    case At:
      t = e._payload, e = e._init;
      try {
        return Qa(e(t));
      } catch {
      }
  }
  return null;
}
function Ud(e) {
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
      return Qa(t);
    case 8:
      return t === bi ? "StrictMode" : "Mode";
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
function Jt(e) {
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
function du(e) {
  var t = e.type;
  return (e = e.nodeName) && e.toLowerCase() === "input" && (t === "checkbox" || t === "radio");
}
function Vd(e) {
  var t = du(e) ? "checked" : "value", n = Object.getOwnPropertyDescriptor(e.constructor.prototype, t), r = "" + e[t];
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
function Yr(e) {
  e._valueTracker || (e._valueTracker = Vd(e));
}
function fu(e) {
  if (!e) return !1;
  var t = e._valueTracker;
  if (!t) return !0;
  var n = t.getValue(), r = "";
  return e && (r = du(e) ? e.checked ? "true" : "false" : e.value), e = r, e !== n ? (t.setValue(e), !0) : !1;
}
function Sl(e) {
  if (e = e || (typeof document < "u" ? document : void 0), typeof e > "u") return null;
  try {
    return e.activeElement || e.body;
  } catch {
    return e.body;
  }
}
function Ga(e, t) {
  var n = t.checked;
  return ce({}, t, { defaultChecked: void 0, defaultValue: void 0, value: void 0, checked: n ?? e._wrapperState.initialChecked });
}
function Vo(e, t) {
  var n = t.defaultValue == null ? "" : t.defaultValue, r = t.checked != null ? t.checked : t.defaultChecked;
  n = Jt(t.value != null ? t.value : n), e._wrapperState = { initialChecked: r, initialValue: n, controlled: t.type === "checkbox" || t.type === "radio" ? t.checked != null : t.value != null };
}
function pu(e, t) {
  t = t.checked, t != null && Bi(e, "checked", t, !1);
}
function Ka(e, t) {
  pu(e, t);
  var n = Jt(t.value), r = t.type;
  if (n != null) r === "number" ? (n === 0 && e.value === "" || e.value != n) && (e.value = "" + n) : e.value !== "" + n && (e.value = "" + n);
  else if (r === "submit" || r === "reset") {
    e.removeAttribute("value");
    return;
  }
  t.hasOwnProperty("value") ? qa(e, t.type, n) : t.hasOwnProperty("defaultValue") && qa(e, t.type, Jt(t.defaultValue)), t.checked == null && t.defaultChecked != null && (e.defaultChecked = !!t.defaultChecked);
}
function Bo(e, t, n) {
  if (t.hasOwnProperty("value") || t.hasOwnProperty("defaultValue")) {
    var r = t.type;
    if (!(r !== "submit" && r !== "reset" || t.value !== void 0 && t.value !== null)) return;
    t = "" + e._wrapperState.initialValue, n || t === e.value || (e.value = t), e.defaultValue = t;
  }
  n = e.name, n !== "" && (e.name = ""), e.defaultChecked = !!e._wrapperState.initialChecked, n !== "" && (e.name = n);
}
function qa(e, t, n) {
  (t !== "number" || Sl(e.ownerDocument) !== e) && (n == null ? e.defaultValue = "" + e._wrapperState.initialValue : e.defaultValue !== "" + n && (e.defaultValue = "" + n));
}
var cr = Array.isArray;
function On(e, t, n, r) {
  if (e = e.options, t) {
    t = {};
    for (var l = 0; l < n.length; l++) t["$" + n[l]] = !0;
    for (n = 0; n < e.length; n++) l = t.hasOwnProperty("$" + e[n].value), e[n].selected !== l && (e[n].selected = l), l && r && (e[n].defaultSelected = !0);
  } else {
    for (n = "" + Jt(n), t = null, l = 0; l < e.length; l++) {
      if (e[l].value === n) {
        e[l].selected = !0, r && (e[l].defaultSelected = !0);
        return;
      }
      t !== null || e[l].disabled || (t = e[l]);
    }
    t !== null && (t.selected = !0);
  }
}
function Ya(e, t) {
  if (t.dangerouslySetInnerHTML != null) throw Error(I(91));
  return ce({}, t, { value: void 0, defaultValue: void 0, children: "" + e._wrapperState.initialValue });
}
function bo(e, t) {
  var n = t.value;
  if (n == null) {
    if (n = t.children, t = t.defaultValue, n != null) {
      if (t != null) throw Error(I(92));
      if (cr(n)) {
        if (1 < n.length) throw Error(I(93));
        n = n[0];
      }
      t = n;
    }
    t == null && (t = ""), n = t;
  }
  e._wrapperState = { initialValue: Jt(n) };
}
function mu(e, t) {
  var n = Jt(t.value), r = Jt(t.defaultValue);
  n != null && (n = "" + n, n !== e.value && (e.value = n), t.defaultValue == null && e.defaultValue !== n && (e.defaultValue = n)), r != null && (e.defaultValue = "" + r);
}
function Ho(e) {
  var t = e.textContent;
  t === e._wrapperState.initialValue && t !== "" && t !== null && (e.value = t);
}
function hu(e) {
  switch (e) {
    case "svg":
      return "http://www.w3.org/2000/svg";
    case "math":
      return "http://www.w3.org/1998/Math/MathML";
    default:
      return "http://www.w3.org/1999/xhtml";
  }
}
function Xa(e, t) {
  return e == null || e === "http://www.w3.org/1999/xhtml" ? hu(t) : e === "http://www.w3.org/2000/svg" && t === "foreignObject" ? "http://www.w3.org/1999/xhtml" : e;
}
var Xr, vu = function(e) {
  return typeof MSApp < "u" && MSApp.execUnsafeLocalFunction ? function(t, n, r, l) {
    MSApp.execUnsafeLocalFunction(function() {
      return e(t, n, r, l);
    });
  } : e;
}(function(e, t) {
  if (e.namespaceURI !== "http://www.w3.org/2000/svg" || "innerHTML" in e) e.innerHTML = t;
  else {
    for (Xr = Xr || document.createElement("div"), Xr.innerHTML = "<svg>" + t.valueOf().toString() + "</svg>", t = Xr.firstChild; e.firstChild; ) e.removeChild(e.firstChild);
    for (; t.firstChild; ) e.appendChild(t.firstChild);
  }
});
function kr(e, t) {
  if (t) {
    var n = e.firstChild;
    if (n && n === e.lastChild && n.nodeType === 3) {
      n.nodeValue = t;
      return;
    }
  }
  e.textContent = t;
}
var pr = {
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
}, Bd = ["Webkit", "ms", "Moz", "O"];
Object.keys(pr).forEach(function(e) {
  Bd.forEach(function(t) {
    t = t + e.charAt(0).toUpperCase() + e.substring(1), pr[t] = pr[e];
  });
});
function gu(e, t, n) {
  return t == null || typeof t == "boolean" || t === "" ? "" : n || typeof t != "number" || t === 0 || pr.hasOwnProperty(e) && pr[e] ? ("" + t).trim() : t + "px";
}
function xu(e, t) {
  e = e.style;
  for (var n in t) if (t.hasOwnProperty(n)) {
    var r = n.indexOf("--") === 0, l = gu(n, t[n], r);
    n === "float" && (n = "cssFloat"), r ? e.setProperty(n, l) : e[n] = l;
  }
}
var bd = ce({ menuitem: !0 }, { area: !0, base: !0, br: !0, col: !0, embed: !0, hr: !0, img: !0, input: !0, keygen: !0, link: !0, meta: !0, param: !0, source: !0, track: !0, wbr: !0 });
function Za(e, t) {
  if (t) {
    if (bd[e] && (t.children != null || t.dangerouslySetInnerHTML != null)) throw Error(I(137, e));
    if (t.dangerouslySetInnerHTML != null) {
      if (t.children != null) throw Error(I(60));
      if (typeof t.dangerouslySetInnerHTML != "object" || !("__html" in t.dangerouslySetInnerHTML)) throw Error(I(61));
    }
    if (t.style != null && typeof t.style != "object") throw Error(I(62));
  }
}
function Ja(e, t) {
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
var ei = null;
function Qi(e) {
  return e = e.target || e.srcElement || window, e.correspondingUseElement && (e = e.correspondingUseElement), e.nodeType === 3 ? e.parentNode : e;
}
var ti = null, An = null, Un = null;
function Wo(e) {
  if (e = Hr(e)) {
    if (typeof ti != "function") throw Error(I(280));
    var t = e.stateNode;
    t && (t = Zl(t), ti(e.stateNode, e.type, t));
  }
}
function yu(e) {
  An ? Un ? Un.push(e) : Un = [e] : An = e;
}
function ju() {
  if (An) {
    var e = An, t = Un;
    if (Un = An = null, Wo(e), t) for (e = 0; e < t.length; e++) Wo(t[e]);
  }
}
function Nu(e, t) {
  return e(t);
}
function wu() {
}
var ga = !1;
function Su(e, t, n) {
  if (ga) return e(t, n);
  ga = !0;
  try {
    return Nu(e, t, n);
  } finally {
    ga = !1, (An !== null || Un !== null) && (wu(), ju());
  }
}
function Cr(e, t) {
  var n = e.stateNode;
  if (n === null) return null;
  var r = Zl(n);
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
var ni = !1;
if (Rt) try {
  var nr = {};
  Object.defineProperty(nr, "passive", { get: function() {
    ni = !0;
  } }), window.addEventListener("test", nr, nr), window.removeEventListener("test", nr, nr);
} catch {
  ni = !1;
}
function Hd(e, t, n, r, l, i, o, s, u) {
  var d = Array.prototype.slice.call(arguments, 3);
  try {
    t.apply(n, d);
  } catch (N) {
    this.onError(N);
  }
}
var mr = !1, kl = null, Cl = !1, ri = null, Wd = { onError: function(e) {
  mr = !0, kl = e;
} };
function Qd(e, t, n, r, l, i, o, s, u) {
  mr = !1, kl = null, Hd.apply(Wd, arguments);
}
function Gd(e, t, n, r, l, i, o, s, u) {
  if (Qd.apply(this, arguments), mr) {
    if (mr) {
      var d = kl;
      mr = !1, kl = null;
    } else throw Error(I(198));
    Cl || (Cl = !0, ri = d);
  }
}
function Sn(e) {
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
function ku(e) {
  if (e.tag === 13) {
    var t = e.memoizedState;
    if (t === null && (e = e.alternate, e !== null && (t = e.memoizedState)), t !== null) return t.dehydrated;
  }
  return null;
}
function Qo(e) {
  if (Sn(e) !== e) throw Error(I(188));
}
function Kd(e) {
  var t = e.alternate;
  if (!t) {
    if (t = Sn(e), t === null) throw Error(I(188));
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
        if (i === n) return Qo(l), e;
        if (i === r) return Qo(l), t;
        i = i.sibling;
      }
      throw Error(I(188));
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
        if (!o) throw Error(I(189));
      }
    }
    if (n.alternate !== r) throw Error(I(190));
  }
  if (n.tag !== 3) throw Error(I(188));
  return n.stateNode.current === n ? e : t;
}
function Cu(e) {
  return e = Kd(e), e !== null ? Eu(e) : null;
}
function Eu(e) {
  if (e.tag === 5 || e.tag === 6) return e;
  for (e = e.child; e !== null; ) {
    var t = Eu(e);
    if (t !== null) return t;
    e = e.sibling;
  }
  return null;
}
var Iu = Ze.unstable_scheduleCallback, Go = Ze.unstable_cancelCallback, qd = Ze.unstable_shouldYield, Yd = Ze.unstable_requestPaint, ve = Ze.unstable_now, Xd = Ze.unstable_getCurrentPriorityLevel, Gi = Ze.unstable_ImmediatePriority, Pu = Ze.unstable_UserBlockingPriority, El = Ze.unstable_NormalPriority, Zd = Ze.unstable_LowPriority, Fu = Ze.unstable_IdlePriority, Kl = null, St = null;
function Jd(e) {
  if (St && typeof St.onCommitFiberRoot == "function") try {
    St.onCommitFiberRoot(Kl, e, void 0, (e.current.flags & 128) === 128);
  } catch {
  }
}
var vt = Math.clz32 ? Math.clz32 : nf, ef = Math.log, tf = Math.LN2;
function nf(e) {
  return e >>>= 0, e === 0 ? 32 : 31 - (ef(e) / tf | 0) | 0;
}
var Zr = 64, Jr = 4194304;
function dr(e) {
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
function Il(e, t) {
  var n = e.pendingLanes;
  if (n === 0) return 0;
  var r = 0, l = e.suspendedLanes, i = e.pingedLanes, o = n & 268435455;
  if (o !== 0) {
    var s = o & ~l;
    s !== 0 ? r = dr(s) : (i &= o, i !== 0 && (r = dr(i)));
  } else o = n & ~l, o !== 0 ? r = dr(o) : i !== 0 && (r = dr(i));
  if (r === 0) return 0;
  if (t !== 0 && t !== r && !(t & l) && (l = r & -r, i = t & -t, l >= i || l === 16 && (i & 4194240) !== 0)) return t;
  if (r & 4 && (r |= n & 16), t = e.entangledLanes, t !== 0) for (e = e.entanglements, t &= r; 0 < t; ) n = 31 - vt(t), l = 1 << n, r |= e[n], t &= ~l;
  return r;
}
function rf(e, t) {
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
function lf(e, t) {
  for (var n = e.suspendedLanes, r = e.pingedLanes, l = e.expirationTimes, i = e.pendingLanes; 0 < i; ) {
    var o = 31 - vt(i), s = 1 << o, u = l[o];
    u === -1 ? (!(s & n) || s & r) && (l[o] = rf(s, t)) : u <= t && (e.expiredLanes |= s), i &= ~s;
  }
}
function li(e) {
  return e = e.pendingLanes & -1073741825, e !== 0 ? e : e & 1073741824 ? 1073741824 : 0;
}
function _u() {
  var e = Zr;
  return Zr <<= 1, !(Zr & 4194240) && (Zr = 64), e;
}
function xa(e) {
  for (var t = [], n = 0; 31 > n; n++) t.push(e);
  return t;
}
function Br(e, t, n) {
  e.pendingLanes |= t, t !== 536870912 && (e.suspendedLanes = 0, e.pingedLanes = 0), e = e.eventTimes, t = 31 - vt(t), e[t] = n;
}
function af(e, t) {
  var n = e.pendingLanes & ~t;
  e.pendingLanes = t, e.suspendedLanes = 0, e.pingedLanes = 0, e.expiredLanes &= t, e.mutableReadLanes &= t, e.entangledLanes &= t, t = e.entanglements;
  var r = e.eventTimes;
  for (e = e.expirationTimes; 0 < n; ) {
    var l = 31 - vt(n), i = 1 << l;
    t[l] = 0, r[l] = -1, e[l] = -1, n &= ~i;
  }
}
function Ki(e, t) {
  var n = e.entangledLanes |= t;
  for (e = e.entanglements; n; ) {
    var r = 31 - vt(n), l = 1 << r;
    l & t | e[r] & t && (e[r] |= t), n &= ~l;
  }
}
var Y = 0;
function Ru(e) {
  return e &= -e, 1 < e ? 4 < e ? e & 268435455 ? 16 : 536870912 : 4 : 1;
}
var Tu, qi, zu, Lu, Du, ai = !1, el = [], Wt = null, Qt = null, Gt = null, Er = /* @__PURE__ */ new Map(), Ir = /* @__PURE__ */ new Map(), Vt = [], of = "mousedown mouseup touchcancel touchend touchstart auxclick dblclick pointercancel pointerdown pointerup dragend dragstart drop compositionend compositionstart keydown keypress keyup input textInput copy cut paste click change contextmenu reset submit".split(" ");
function Ko(e, t) {
  switch (e) {
    case "focusin":
    case "focusout":
      Wt = null;
      break;
    case "dragenter":
    case "dragleave":
      Qt = null;
      break;
    case "mouseover":
    case "mouseout":
      Gt = null;
      break;
    case "pointerover":
    case "pointerout":
      Er.delete(t.pointerId);
      break;
    case "gotpointercapture":
    case "lostpointercapture":
      Ir.delete(t.pointerId);
  }
}
function rr(e, t, n, r, l, i) {
  return e === null || e.nativeEvent !== i ? (e = { blockedOn: t, domEventName: n, eventSystemFlags: r, nativeEvent: i, targetContainers: [l] }, t !== null && (t = Hr(t), t !== null && qi(t)), e) : (e.eventSystemFlags |= r, t = e.targetContainers, l !== null && t.indexOf(l) === -1 && t.push(l), e);
}
function sf(e, t, n, r, l) {
  switch (t) {
    case "focusin":
      return Wt = rr(Wt, e, t, n, r, l), !0;
    case "dragenter":
      return Qt = rr(Qt, e, t, n, r, l), !0;
    case "mouseover":
      return Gt = rr(Gt, e, t, n, r, l), !0;
    case "pointerover":
      var i = l.pointerId;
      return Er.set(i, rr(Er.get(i) || null, e, t, n, r, l)), !0;
    case "gotpointercapture":
      return i = l.pointerId, Ir.set(i, rr(Ir.get(i) || null, e, t, n, r, l)), !0;
  }
  return !1;
}
function Mu(e) {
  var t = cn(e.target);
  if (t !== null) {
    var n = Sn(t);
    if (n !== null) {
      if (t = n.tag, t === 13) {
        if (t = ku(n), t !== null) {
          e.blockedOn = t, Du(e.priority, function() {
            zu(n);
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
function pl(e) {
  if (e.blockedOn !== null) return !1;
  for (var t = e.targetContainers; 0 < t.length; ) {
    var n = ii(e.domEventName, e.eventSystemFlags, t[0], e.nativeEvent);
    if (n === null) {
      n = e.nativeEvent;
      var r = new n.constructor(n.type, n);
      ei = r, n.target.dispatchEvent(r), ei = null;
    } else return t = Hr(n), t !== null && qi(t), e.blockedOn = n, !1;
    t.shift();
  }
  return !0;
}
function qo(e, t, n) {
  pl(e) && n.delete(t);
}
function uf() {
  ai = !1, Wt !== null && pl(Wt) && (Wt = null), Qt !== null && pl(Qt) && (Qt = null), Gt !== null && pl(Gt) && (Gt = null), Er.forEach(qo), Ir.forEach(qo);
}
function lr(e, t) {
  e.blockedOn === t && (e.blockedOn = null, ai || (ai = !0, Ze.unstable_scheduleCallback(Ze.unstable_NormalPriority, uf)));
}
function Pr(e) {
  function t(l) {
    return lr(l, e);
  }
  if (0 < el.length) {
    lr(el[0], e);
    for (var n = 1; n < el.length; n++) {
      var r = el[n];
      r.blockedOn === e && (r.blockedOn = null);
    }
  }
  for (Wt !== null && lr(Wt, e), Qt !== null && lr(Qt, e), Gt !== null && lr(Gt, e), Er.forEach(t), Ir.forEach(t), n = 0; n < Vt.length; n++) r = Vt[n], r.blockedOn === e && (r.blockedOn = null);
  for (; 0 < Vt.length && (n = Vt[0], n.blockedOn === null); ) Mu(n), n.blockedOn === null && Vt.shift();
}
var Vn = Dt.ReactCurrentBatchConfig, Pl = !0;
function cf(e, t, n, r) {
  var l = Y, i = Vn.transition;
  Vn.transition = null;
  try {
    Y = 1, Yi(e, t, n, r);
  } finally {
    Y = l, Vn.transition = i;
  }
}
function df(e, t, n, r) {
  var l = Y, i = Vn.transition;
  Vn.transition = null;
  try {
    Y = 4, Yi(e, t, n, r);
  } finally {
    Y = l, Vn.transition = i;
  }
}
function Yi(e, t, n, r) {
  if (Pl) {
    var l = ii(e, t, n, r);
    if (l === null) Pa(e, t, r, Fl, n), Ko(e, r);
    else if (sf(l, e, t, n, r)) r.stopPropagation();
    else if (Ko(e, r), t & 4 && -1 < of.indexOf(e)) {
      for (; l !== null; ) {
        var i = Hr(l);
        if (i !== null && Tu(i), i = ii(e, t, n, r), i === null && Pa(e, t, r, Fl, n), i === l) break;
        l = i;
      }
      l !== null && r.stopPropagation();
    } else Pa(e, t, r, null, n);
  }
}
var Fl = null;
function ii(e, t, n, r) {
  if (Fl = null, e = Qi(r), e = cn(e), e !== null) if (t = Sn(e), t === null) e = null;
  else if (n = t.tag, n === 13) {
    if (e = ku(t), e !== null) return e;
    e = null;
  } else if (n === 3) {
    if (t.stateNode.current.memoizedState.isDehydrated) return t.tag === 3 ? t.stateNode.containerInfo : null;
    e = null;
  } else t !== e && (e = null);
  return Fl = e, null;
}
function $u(e) {
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
      switch (Xd()) {
        case Gi:
          return 1;
        case Pu:
          return 4;
        case El:
        case Zd:
          return 16;
        case Fu:
          return 536870912;
        default:
          return 16;
      }
    default:
      return 16;
  }
}
var bt = null, Xi = null, ml = null;
function Ou() {
  if (ml) return ml;
  var e, t = Xi, n = t.length, r, l = "value" in bt ? bt.value : bt.textContent, i = l.length;
  for (e = 0; e < n && t[e] === l[e]; e++) ;
  var o = n - e;
  for (r = 1; r <= o && t[n - r] === l[i - r]; r++) ;
  return ml = l.slice(e, 1 < r ? 1 - r : void 0);
}
function hl(e) {
  var t = e.keyCode;
  return "charCode" in e ? (e = e.charCode, e === 0 && t === 13 && (e = 13)) : e = t, e === 10 && (e = 13), 32 <= e || e === 13 ? e : 0;
}
function tl() {
  return !0;
}
function Yo() {
  return !1;
}
function et(e) {
  function t(n, r, l, i, o) {
    this._reactName = n, this._targetInst = l, this.type = r, this.nativeEvent = i, this.target = o, this.currentTarget = null;
    for (var s in e) e.hasOwnProperty(s) && (n = e[s], this[s] = n ? n(i) : i[s]);
    return this.isDefaultPrevented = (i.defaultPrevented != null ? i.defaultPrevented : i.returnValue === !1) ? tl : Yo, this.isPropagationStopped = Yo, this;
  }
  return ce(t.prototype, { preventDefault: function() {
    this.defaultPrevented = !0;
    var n = this.nativeEvent;
    n && (n.preventDefault ? n.preventDefault() : typeof n.returnValue != "unknown" && (n.returnValue = !1), this.isDefaultPrevented = tl);
  }, stopPropagation: function() {
    var n = this.nativeEvent;
    n && (n.stopPropagation ? n.stopPropagation() : typeof n.cancelBubble != "unknown" && (n.cancelBubble = !0), this.isPropagationStopped = tl);
  }, persist: function() {
  }, isPersistent: tl }), t;
}
var Zn = { eventPhase: 0, bubbles: 0, cancelable: 0, timeStamp: function(e) {
  return e.timeStamp || Date.now();
}, defaultPrevented: 0, isTrusted: 0 }, Zi = et(Zn), br = ce({}, Zn, { view: 0, detail: 0 }), ff = et(br), ya, ja, ar, ql = ce({}, br, { screenX: 0, screenY: 0, clientX: 0, clientY: 0, pageX: 0, pageY: 0, ctrlKey: 0, shiftKey: 0, altKey: 0, metaKey: 0, getModifierState: Ji, button: 0, buttons: 0, relatedTarget: function(e) {
  return e.relatedTarget === void 0 ? e.fromElement === e.srcElement ? e.toElement : e.fromElement : e.relatedTarget;
}, movementX: function(e) {
  return "movementX" in e ? e.movementX : (e !== ar && (ar && e.type === "mousemove" ? (ya = e.screenX - ar.screenX, ja = e.screenY - ar.screenY) : ja = ya = 0, ar = e), ya);
}, movementY: function(e) {
  return "movementY" in e ? e.movementY : ja;
} }), Xo = et(ql), pf = ce({}, ql, { dataTransfer: 0 }), mf = et(pf), hf = ce({}, br, { relatedTarget: 0 }), Na = et(hf), vf = ce({}, Zn, { animationName: 0, elapsedTime: 0, pseudoElement: 0 }), gf = et(vf), xf = ce({}, Zn, { clipboardData: function(e) {
  return "clipboardData" in e ? e.clipboardData : window.clipboardData;
} }), yf = et(xf), jf = ce({}, Zn, { data: 0 }), Zo = et(jf), Nf = {
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
}, wf = {
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
}, Sf = { Alt: "altKey", Control: "ctrlKey", Meta: "metaKey", Shift: "shiftKey" };
function kf(e) {
  var t = this.nativeEvent;
  return t.getModifierState ? t.getModifierState(e) : (e = Sf[e]) ? !!t[e] : !1;
}
function Ji() {
  return kf;
}
var Cf = ce({}, br, { key: function(e) {
  if (e.key) {
    var t = Nf[e.key] || e.key;
    if (t !== "Unidentified") return t;
  }
  return e.type === "keypress" ? (e = hl(e), e === 13 ? "Enter" : String.fromCharCode(e)) : e.type === "keydown" || e.type === "keyup" ? wf[e.keyCode] || "Unidentified" : "";
}, code: 0, location: 0, ctrlKey: 0, shiftKey: 0, altKey: 0, metaKey: 0, repeat: 0, locale: 0, getModifierState: Ji, charCode: function(e) {
  return e.type === "keypress" ? hl(e) : 0;
}, keyCode: function(e) {
  return e.type === "keydown" || e.type === "keyup" ? e.keyCode : 0;
}, which: function(e) {
  return e.type === "keypress" ? hl(e) : e.type === "keydown" || e.type === "keyup" ? e.keyCode : 0;
} }), Ef = et(Cf), If = ce({}, ql, { pointerId: 0, width: 0, height: 0, pressure: 0, tangentialPressure: 0, tiltX: 0, tiltY: 0, twist: 0, pointerType: 0, isPrimary: 0 }), Jo = et(If), Pf = ce({}, br, { touches: 0, targetTouches: 0, changedTouches: 0, altKey: 0, metaKey: 0, ctrlKey: 0, shiftKey: 0, getModifierState: Ji }), Ff = et(Pf), _f = ce({}, Zn, { propertyName: 0, elapsedTime: 0, pseudoElement: 0 }), Rf = et(_f), Tf = ce({}, ql, {
  deltaX: function(e) {
    return "deltaX" in e ? e.deltaX : "wheelDeltaX" in e ? -e.wheelDeltaX : 0;
  },
  deltaY: function(e) {
    return "deltaY" in e ? e.deltaY : "wheelDeltaY" in e ? -e.wheelDeltaY : "wheelDelta" in e ? -e.wheelDelta : 0;
  },
  deltaZ: 0,
  deltaMode: 0
}), zf = et(Tf), Lf = [9, 13, 27, 32], eo = Rt && "CompositionEvent" in window, hr = null;
Rt && "documentMode" in document && (hr = document.documentMode);
var Df = Rt && "TextEvent" in window && !hr, Au = Rt && (!eo || hr && 8 < hr && 11 >= hr), es = " ", ts = !1;
function Uu(e, t) {
  switch (e) {
    case "keyup":
      return Lf.indexOf(t.keyCode) !== -1;
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
function Vu(e) {
  return e = e.detail, typeof e == "object" && "data" in e ? e.data : null;
}
var Pn = !1;
function Mf(e, t) {
  switch (e) {
    case "compositionend":
      return Vu(t);
    case "keypress":
      return t.which !== 32 ? null : (ts = !0, es);
    case "textInput":
      return e = t.data, e === es && ts ? null : e;
    default:
      return null;
  }
}
function $f(e, t) {
  if (Pn) return e === "compositionend" || !eo && Uu(e, t) ? (e = Ou(), ml = Xi = bt = null, Pn = !1, e) : null;
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
      return Au && t.locale !== "ko" ? null : t.data;
    default:
      return null;
  }
}
var Of = { color: !0, date: !0, datetime: !0, "datetime-local": !0, email: !0, month: !0, number: !0, password: !0, range: !0, search: !0, tel: !0, text: !0, time: !0, url: !0, week: !0 };
function ns(e) {
  var t = e && e.nodeName && e.nodeName.toLowerCase();
  return t === "input" ? !!Of[e.type] : t === "textarea";
}
function Bu(e, t, n, r) {
  yu(r), t = _l(t, "onChange"), 0 < t.length && (n = new Zi("onChange", "change", null, n, r), e.push({ event: n, listeners: t }));
}
var vr = null, Fr = null;
function Af(e) {
  Ju(e, 0);
}
function Yl(e) {
  var t = Rn(e);
  if (fu(t)) return e;
}
function Uf(e, t) {
  if (e === "change") return t;
}
var bu = !1;
if (Rt) {
  var wa;
  if (Rt) {
    var Sa = "oninput" in document;
    if (!Sa) {
      var rs = document.createElement("div");
      rs.setAttribute("oninput", "return;"), Sa = typeof rs.oninput == "function";
    }
    wa = Sa;
  } else wa = !1;
  bu = wa && (!document.documentMode || 9 < document.documentMode);
}
function ls() {
  vr && (vr.detachEvent("onpropertychange", Hu), Fr = vr = null);
}
function Hu(e) {
  if (e.propertyName === "value" && Yl(Fr)) {
    var t = [];
    Bu(t, Fr, e, Qi(e)), Su(Af, t);
  }
}
function Vf(e, t, n) {
  e === "focusin" ? (ls(), vr = t, Fr = n, vr.attachEvent("onpropertychange", Hu)) : e === "focusout" && ls();
}
function Bf(e) {
  if (e === "selectionchange" || e === "keyup" || e === "keydown") return Yl(Fr);
}
function bf(e, t) {
  if (e === "click") return Yl(t);
}
function Hf(e, t) {
  if (e === "input" || e === "change") return Yl(t);
}
function Wf(e, t) {
  return e === t && (e !== 0 || 1 / e === 1 / t) || e !== e && t !== t;
}
var yt = typeof Object.is == "function" ? Object.is : Wf;
function _r(e, t) {
  if (yt(e, t)) return !0;
  if (typeof e != "object" || e === null || typeof t != "object" || t === null) return !1;
  var n = Object.keys(e), r = Object.keys(t);
  if (n.length !== r.length) return !1;
  for (r = 0; r < n.length; r++) {
    var l = n[r];
    if (!Ba.call(t, l) || !yt(e[l], t[l])) return !1;
  }
  return !0;
}
function as(e) {
  for (; e && e.firstChild; ) e = e.firstChild;
  return e;
}
function is(e, t) {
  var n = as(e);
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
    n = as(n);
  }
}
function Wu(e, t) {
  return e && t ? e === t ? !0 : e && e.nodeType === 3 ? !1 : t && t.nodeType === 3 ? Wu(e, t.parentNode) : "contains" in e ? e.contains(t) : e.compareDocumentPosition ? !!(e.compareDocumentPosition(t) & 16) : !1 : !1;
}
function Qu() {
  for (var e = window, t = Sl(); t instanceof e.HTMLIFrameElement; ) {
    try {
      var n = typeof t.contentWindow.location.href == "string";
    } catch {
      n = !1;
    }
    if (n) e = t.contentWindow;
    else break;
    t = Sl(e.document);
  }
  return t;
}
function to(e) {
  var t = e && e.nodeName && e.nodeName.toLowerCase();
  return t && (t === "input" && (e.type === "text" || e.type === "search" || e.type === "tel" || e.type === "url" || e.type === "password") || t === "textarea" || e.contentEditable === "true");
}
function Qf(e) {
  var t = Qu(), n = e.focusedElem, r = e.selectionRange;
  if (t !== n && n && n.ownerDocument && Wu(n.ownerDocument.documentElement, n)) {
    if (r !== null && to(n)) {
      if (t = r.start, e = r.end, e === void 0 && (e = t), "selectionStart" in n) n.selectionStart = t, n.selectionEnd = Math.min(e, n.value.length);
      else if (e = (t = n.ownerDocument || document) && t.defaultView || window, e.getSelection) {
        e = e.getSelection();
        var l = n.textContent.length, i = Math.min(r.start, l);
        r = r.end === void 0 ? i : Math.min(r.end, l), !e.extend && i > r && (l = r, r = i, i = l), l = is(n, i);
        var o = is(
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
var Gf = Rt && "documentMode" in document && 11 >= document.documentMode, Fn = null, oi = null, gr = null, si = !1;
function os(e, t, n) {
  var r = n.window === n ? n.document : n.nodeType === 9 ? n : n.ownerDocument;
  si || Fn == null || Fn !== Sl(r) || (r = Fn, "selectionStart" in r && to(r) ? r = { start: r.selectionStart, end: r.selectionEnd } : (r = (r.ownerDocument && r.ownerDocument.defaultView || window).getSelection(), r = { anchorNode: r.anchorNode, anchorOffset: r.anchorOffset, focusNode: r.focusNode, focusOffset: r.focusOffset }), gr && _r(gr, r) || (gr = r, r = _l(oi, "onSelect"), 0 < r.length && (t = new Zi("onSelect", "select", null, t, n), e.push({ event: t, listeners: r }), t.target = Fn)));
}
function nl(e, t) {
  var n = {};
  return n[e.toLowerCase()] = t.toLowerCase(), n["Webkit" + e] = "webkit" + t, n["Moz" + e] = "moz" + t, n;
}
var _n = { animationend: nl("Animation", "AnimationEnd"), animationiteration: nl("Animation", "AnimationIteration"), animationstart: nl("Animation", "AnimationStart"), transitionend: nl("Transition", "TransitionEnd") }, ka = {}, Gu = {};
Rt && (Gu = document.createElement("div").style, "AnimationEvent" in window || (delete _n.animationend.animation, delete _n.animationiteration.animation, delete _n.animationstart.animation), "TransitionEvent" in window || delete _n.transitionend.transition);
function Xl(e) {
  if (ka[e]) return ka[e];
  if (!_n[e]) return e;
  var t = _n[e], n;
  for (n in t) if (t.hasOwnProperty(n) && n in Gu) return ka[e] = t[n];
  return e;
}
var Ku = Xl("animationend"), qu = Xl("animationiteration"), Yu = Xl("animationstart"), Xu = Xl("transitionend"), Zu = /* @__PURE__ */ new Map(), ss = "abort auxClick cancel canPlay canPlayThrough click close contextMenu copy cut drag dragEnd dragEnter dragExit dragLeave dragOver dragStart drop durationChange emptied encrypted ended error gotPointerCapture input invalid keyDown keyPress keyUp load loadedData loadedMetadata loadStart lostPointerCapture mouseDown mouseMove mouseOut mouseOver mouseUp paste pause play playing pointerCancel pointerDown pointerMove pointerOut pointerOver pointerUp progress rateChange reset resize seeked seeking stalled submit suspend timeUpdate touchCancel touchEnd touchStart volumeChange scroll toggle touchMove waiting wheel".split(" ");
function tn(e, t) {
  Zu.set(e, t), wn(t, [e]);
}
for (var Ca = 0; Ca < ss.length; Ca++) {
  var Ea = ss[Ca], Kf = Ea.toLowerCase(), qf = Ea[0].toUpperCase() + Ea.slice(1);
  tn(Kf, "on" + qf);
}
tn(Ku, "onAnimationEnd");
tn(qu, "onAnimationIteration");
tn(Yu, "onAnimationStart");
tn("dblclick", "onDoubleClick");
tn("focusin", "onFocus");
tn("focusout", "onBlur");
tn(Xu, "onTransitionEnd");
Hn("onMouseEnter", ["mouseout", "mouseover"]);
Hn("onMouseLeave", ["mouseout", "mouseover"]);
Hn("onPointerEnter", ["pointerout", "pointerover"]);
Hn("onPointerLeave", ["pointerout", "pointerover"]);
wn("onChange", "change click focusin focusout input keydown keyup selectionchange".split(" "));
wn("onSelect", "focusout contextmenu dragend focusin keydown keyup mousedown mouseup selectionchange".split(" "));
wn("onBeforeInput", ["compositionend", "keypress", "textInput", "paste"]);
wn("onCompositionEnd", "compositionend focusout keydown keypress keyup mousedown".split(" "));
wn("onCompositionStart", "compositionstart focusout keydown keypress keyup mousedown".split(" "));
wn("onCompositionUpdate", "compositionupdate focusout keydown keypress keyup mousedown".split(" "));
var fr = "abort canplay canplaythrough durationchange emptied encrypted ended error loadeddata loadedmetadata loadstart pause play playing progress ratechange resize seeked seeking stalled suspend timeupdate volumechange waiting".split(" "), Yf = new Set("cancel close invalid load scroll toggle".split(" ").concat(fr));
function us(e, t, n) {
  var r = e.type || "unknown-event";
  e.currentTarget = n, Gd(r, t, void 0, e), e.currentTarget = null;
}
function Ju(e, t) {
  t = (t & 4) !== 0;
  for (var n = 0; n < e.length; n++) {
    var r = e[n], l = r.event;
    r = r.listeners;
    e: {
      var i = void 0;
      if (t) for (var o = r.length - 1; 0 <= o; o--) {
        var s = r[o], u = s.instance, d = s.currentTarget;
        if (s = s.listener, u !== i && l.isPropagationStopped()) break e;
        us(l, s, d), i = u;
      }
      else for (o = 0; o < r.length; o++) {
        if (s = r[o], u = s.instance, d = s.currentTarget, s = s.listener, u !== i && l.isPropagationStopped()) break e;
        us(l, s, d), i = u;
      }
    }
  }
  if (Cl) throw e = ri, Cl = !1, ri = null, e;
}
function re(e, t) {
  var n = t[pi];
  n === void 0 && (n = t[pi] = /* @__PURE__ */ new Set());
  var r = e + "__bubble";
  n.has(r) || (ec(t, e, 2, !1), n.add(r));
}
function Ia(e, t, n) {
  var r = 0;
  t && (r |= 4), ec(n, e, r, t);
}
var rl = "_reactListening" + Math.random().toString(36).slice(2);
function Rr(e) {
  if (!e[rl]) {
    e[rl] = !0, ou.forEach(function(n) {
      n !== "selectionchange" && (Yf.has(n) || Ia(n, !1, e), Ia(n, !0, e));
    });
    var t = e.nodeType === 9 ? e : e.ownerDocument;
    t === null || t[rl] || (t[rl] = !0, Ia("selectionchange", !1, t));
  }
}
function ec(e, t, n, r) {
  switch ($u(t)) {
    case 1:
      var l = cf;
      break;
    case 4:
      l = df;
      break;
    default:
      l = Yi;
  }
  n = l.bind(null, t, n, e), l = void 0, !ni || t !== "touchstart" && t !== "touchmove" && t !== "wheel" || (l = !0), r ? l !== void 0 ? e.addEventListener(t, n, { capture: !0, passive: l }) : e.addEventListener(t, n, !0) : l !== void 0 ? e.addEventListener(t, n, { passive: l }) : e.addEventListener(t, n, !1);
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
        if (o = cn(s), o === null) return;
        if (u = o.tag, u === 5 || u === 6) {
          r = i = o;
          continue e;
        }
        s = s.parentNode;
      }
    }
    r = r.return;
  }
  Su(function() {
    var d = i, N = Qi(n), c = [];
    e: {
      var h = Zu.get(e);
      if (h !== void 0) {
        var v = Zi, y = e;
        switch (e) {
          case "keypress":
            if (hl(n) === 0) break e;
          case "keydown":
          case "keyup":
            v = Ef;
            break;
          case "focusin":
            y = "focus", v = Na;
            break;
          case "focusout":
            y = "blur", v = Na;
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
            v = Xo;
            break;
          case "drag":
          case "dragend":
          case "dragenter":
          case "dragexit":
          case "dragleave":
          case "dragover":
          case "dragstart":
          case "drop":
            v = mf;
            break;
          case "touchcancel":
          case "touchend":
          case "touchmove":
          case "touchstart":
            v = Ff;
            break;
          case Ku:
          case qu:
          case Yu:
            v = gf;
            break;
          case Xu:
            v = Rf;
            break;
          case "scroll":
            v = ff;
            break;
          case "wheel":
            v = zf;
            break;
          case "copy":
          case "cut":
          case "paste":
            v = yf;
            break;
          case "gotpointercapture":
          case "lostpointercapture":
          case "pointercancel":
          case "pointerdown":
          case "pointermove":
          case "pointerout":
          case "pointerover":
          case "pointerup":
            v = Jo;
        }
        var w = (t & 4) !== 0, _ = !w && e === "scroll", f = w ? h !== null ? h + "Capture" : null : h;
        w = [];
        for (var p = d, g; p !== null; ) {
          g = p;
          var k = g.stateNode;
          if (g.tag === 5 && k !== null && (g = k, f !== null && (k = Cr(p, f), k != null && w.push(Tr(p, k, g)))), _) break;
          p = p.return;
        }
        0 < w.length && (h = new v(h, y, null, n, N), c.push({ event: h, listeners: w }));
      }
    }
    if (!(t & 7)) {
      e: {
        if (h = e === "mouseover" || e === "pointerover", v = e === "mouseout" || e === "pointerout", h && n !== ei && (y = n.relatedTarget || n.fromElement) && (cn(y) || y[Tt])) break e;
        if ((v || h) && (h = N.window === N ? N : (h = N.ownerDocument) ? h.defaultView || h.parentWindow : window, v ? (y = n.relatedTarget || n.toElement, v = d, y = y ? cn(y) : null, y !== null && (_ = Sn(y), y !== _ || y.tag !== 5 && y.tag !== 6) && (y = null)) : (v = null, y = d), v !== y)) {
          if (w = Xo, k = "onMouseLeave", f = "onMouseEnter", p = "mouse", (e === "pointerout" || e === "pointerover") && (w = Jo, k = "onPointerLeave", f = "onPointerEnter", p = "pointer"), _ = v == null ? h : Rn(v), g = y == null ? h : Rn(y), h = new w(k, p + "leave", v, n, N), h.target = _, h.relatedTarget = g, k = null, cn(N) === d && (w = new w(f, p + "enter", y, n, N), w.target = g, w.relatedTarget = _, k = w), _ = k, v && y) t: {
            for (w = v, f = y, p = 0, g = w; g; g = Cn(g)) p++;
            for (g = 0, k = f; k; k = Cn(k)) g++;
            for (; 0 < p - g; ) w = Cn(w), p--;
            for (; 0 < g - p; ) f = Cn(f), g--;
            for (; p--; ) {
              if (w === f || f !== null && w === f.alternate) break t;
              w = Cn(w), f = Cn(f);
            }
            w = null;
          }
          else w = null;
          v !== null && cs(c, h, v, w, !1), y !== null && _ !== null && cs(c, _, y, w, !0);
        }
      }
      e: {
        if (h = d ? Rn(d) : window, v = h.nodeName && h.nodeName.toLowerCase(), v === "select" || v === "input" && h.type === "file") var T = Uf;
        else if (ns(h)) if (bu) T = Hf;
        else {
          T = Bf;
          var R = Vf;
        }
        else (v = h.nodeName) && v.toLowerCase() === "input" && (h.type === "checkbox" || h.type === "radio") && (T = bf);
        if (T && (T = T(e, d))) {
          Bu(c, T, n, N);
          break e;
        }
        R && R(e, h, d), e === "focusout" && (R = h._wrapperState) && R.controlled && h.type === "number" && qa(h, "number", h.value);
      }
      switch (R = d ? Rn(d) : window, e) {
        case "focusin":
          (ns(R) || R.contentEditable === "true") && (Fn = R, oi = d, gr = null);
          break;
        case "focusout":
          gr = oi = Fn = null;
          break;
        case "mousedown":
          si = !0;
          break;
        case "contextmenu":
        case "mouseup":
        case "dragend":
          si = !1, os(c, n, N);
          break;
        case "selectionchange":
          if (Gf) break;
        case "keydown":
        case "keyup":
          os(c, n, N);
      }
      var z;
      if (eo) e: {
        switch (e) {
          case "compositionstart":
            var M = "onCompositionStart";
            break e;
          case "compositionend":
            M = "onCompositionEnd";
            break e;
          case "compositionupdate":
            M = "onCompositionUpdate";
            break e;
        }
        M = void 0;
      }
      else Pn ? Uu(e, n) && (M = "onCompositionEnd") : e === "keydown" && n.keyCode === 229 && (M = "onCompositionStart");
      M && (Au && n.locale !== "ko" && (Pn || M !== "onCompositionStart" ? M === "onCompositionEnd" && Pn && (z = Ou()) : (bt = N, Xi = "value" in bt ? bt.value : bt.textContent, Pn = !0)), R = _l(d, M), 0 < R.length && (M = new Zo(M, e, null, n, N), c.push({ event: M, listeners: R }), z ? M.data = z : (z = Vu(n), z !== null && (M.data = z)))), (z = Df ? Mf(e, n) : $f(e, n)) && (d = _l(d, "onBeforeInput"), 0 < d.length && (N = new Zo("onBeforeInput", "beforeinput", null, n, N), c.push({ event: N, listeners: d }), N.data = z));
    }
    Ju(c, t);
  });
}
function Tr(e, t, n) {
  return { instance: e, listener: t, currentTarget: n };
}
function _l(e, t) {
  for (var n = t + "Capture", r = []; e !== null; ) {
    var l = e, i = l.stateNode;
    l.tag === 5 && i !== null && (l = i, i = Cr(e, n), i != null && r.unshift(Tr(e, i, l)), i = Cr(e, t), i != null && r.push(Tr(e, i, l))), e = e.return;
  }
  return r;
}
function Cn(e) {
  if (e === null) return null;
  do
    e = e.return;
  while (e && e.tag !== 5);
  return e || null;
}
function cs(e, t, n, r, l) {
  for (var i = t._reactName, o = []; n !== null && n !== r; ) {
    var s = n, u = s.alternate, d = s.stateNode;
    if (u !== null && u === r) break;
    s.tag === 5 && d !== null && (s = d, l ? (u = Cr(n, i), u != null && o.unshift(Tr(n, u, s))) : l || (u = Cr(n, i), u != null && o.push(Tr(n, u, s)))), n = n.return;
  }
  o.length !== 0 && e.push({ event: t, listeners: o });
}
var Xf = /\r\n?/g, Zf = /\u0000|\uFFFD/g;
function ds(e) {
  return (typeof e == "string" ? e : "" + e).replace(Xf, `
`).replace(Zf, "");
}
function ll(e, t, n) {
  if (t = ds(t), ds(e) !== t && n) throw Error(I(425));
}
function Rl() {
}
var ui = null, ci = null;
function di(e, t) {
  return e === "textarea" || e === "noscript" || typeof t.children == "string" || typeof t.children == "number" || typeof t.dangerouslySetInnerHTML == "object" && t.dangerouslySetInnerHTML !== null && t.dangerouslySetInnerHTML.__html != null;
}
var fi = typeof setTimeout == "function" ? setTimeout : void 0, Jf = typeof clearTimeout == "function" ? clearTimeout : void 0, fs = typeof Promise == "function" ? Promise : void 0, ep = typeof queueMicrotask == "function" ? queueMicrotask : typeof fs < "u" ? function(e) {
  return fs.resolve(null).then(e).catch(tp);
} : fi;
function tp(e) {
  setTimeout(function() {
    throw e;
  });
}
function Fa(e, t) {
  var n = t, r = 0;
  do {
    var l = n.nextSibling;
    if (e.removeChild(n), l && l.nodeType === 8) if (n = l.data, n === "/$") {
      if (r === 0) {
        e.removeChild(l), Pr(t);
        return;
      }
      r--;
    } else n !== "$" && n !== "$?" && n !== "$!" || r++;
    n = l;
  } while (n);
  Pr(t);
}
function Kt(e) {
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
function ps(e) {
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
var Jn = Math.random().toString(36).slice(2), wt = "__reactFiber$" + Jn, zr = "__reactProps$" + Jn, Tt = "__reactContainer$" + Jn, pi = "__reactEvents$" + Jn, np = "__reactListeners$" + Jn, rp = "__reactHandles$" + Jn;
function cn(e) {
  var t = e[wt];
  if (t) return t;
  for (var n = e.parentNode; n; ) {
    if (t = n[Tt] || n[wt]) {
      if (n = t.alternate, t.child !== null || n !== null && n.child !== null) for (e = ps(e); e !== null; ) {
        if (n = e[wt]) return n;
        e = ps(e);
      }
      return t;
    }
    e = n, n = e.parentNode;
  }
  return null;
}
function Hr(e) {
  return e = e[wt] || e[Tt], !e || e.tag !== 5 && e.tag !== 6 && e.tag !== 13 && e.tag !== 3 ? null : e;
}
function Rn(e) {
  if (e.tag === 5 || e.tag === 6) return e.stateNode;
  throw Error(I(33));
}
function Zl(e) {
  return e[zr] || null;
}
var mi = [], Tn = -1;
function nn(e) {
  return { current: e };
}
function le(e) {
  0 > Tn || (e.current = mi[Tn], mi[Tn] = null, Tn--);
}
function ne(e, t) {
  Tn++, mi[Tn] = e.current, e.current = t;
}
var en = {}, Le = nn(en), We = nn(!1), vn = en;
function Wn(e, t) {
  var n = e.type.contextTypes;
  if (!n) return en;
  var r = e.stateNode;
  if (r && r.__reactInternalMemoizedUnmaskedChildContext === t) return r.__reactInternalMemoizedMaskedChildContext;
  var l = {}, i;
  for (i in n) l[i] = t[i];
  return r && (e = e.stateNode, e.__reactInternalMemoizedUnmaskedChildContext = t, e.__reactInternalMemoizedMaskedChildContext = l), l;
}
function Qe(e) {
  return e = e.childContextTypes, e != null;
}
function Tl() {
  le(We), le(Le);
}
function ms(e, t, n) {
  if (Le.current !== en) throw Error(I(168));
  ne(Le, t), ne(We, n);
}
function tc(e, t, n) {
  var r = e.stateNode;
  if (t = t.childContextTypes, typeof r.getChildContext != "function") return n;
  r = r.getChildContext();
  for (var l in r) if (!(l in t)) throw Error(I(108, Ud(e) || "Unknown", l));
  return ce({}, n, r);
}
function zl(e) {
  return e = (e = e.stateNode) && e.__reactInternalMemoizedMergedChildContext || en, vn = Le.current, ne(Le, e), ne(We, We.current), !0;
}
function hs(e, t, n) {
  var r = e.stateNode;
  if (!r) throw Error(I(169));
  n ? (e = tc(e, t, vn), r.__reactInternalMemoizedMergedChildContext = e, le(We), le(Le), ne(Le, e)) : le(We), ne(We, n);
}
var It = null, Jl = !1, _a = !1;
function nc(e) {
  It === null ? It = [e] : It.push(e);
}
function lp(e) {
  Jl = !0, nc(e);
}
function rn() {
  if (!_a && It !== null) {
    _a = !0;
    var e = 0, t = Y;
    try {
      var n = It;
      for (Y = 1; e < n.length; e++) {
        var r = n[e];
        do
          r = r(!0);
        while (r !== null);
      }
      It = null, Jl = !1;
    } catch (l) {
      throw It !== null && (It = It.slice(e + 1)), Iu(Gi, rn), l;
    } finally {
      Y = t, _a = !1;
    }
  }
  return null;
}
var zn = [], Ln = 0, Ll = null, Dl = 0, nt = [], rt = 0, gn = null, Pt = 1, Ft = "";
function sn(e, t) {
  zn[Ln++] = Dl, zn[Ln++] = Ll, Ll = e, Dl = t;
}
function rc(e, t, n) {
  nt[rt++] = Pt, nt[rt++] = Ft, nt[rt++] = gn, gn = e;
  var r = Pt;
  e = Ft;
  var l = 32 - vt(r) - 1;
  r &= ~(1 << l), n += 1;
  var i = 32 - vt(t) + l;
  if (30 < i) {
    var o = l - l % 5;
    i = (r & (1 << o) - 1).toString(32), r >>= o, l -= o, Pt = 1 << 32 - vt(t) + l | n << l | r, Ft = i + e;
  } else Pt = 1 << i | n << l | r, Ft = e;
}
function no(e) {
  e.return !== null && (sn(e, 1), rc(e, 1, 0));
}
function ro(e) {
  for (; e === Ll; ) Ll = zn[--Ln], zn[Ln] = null, Dl = zn[--Ln], zn[Ln] = null;
  for (; e === gn; ) gn = nt[--rt], nt[rt] = null, Ft = nt[--rt], nt[rt] = null, Pt = nt[--rt], nt[rt] = null;
}
var Xe = null, Ye = null, ie = !1, mt = null;
function lc(e, t) {
  var n = lt(5, null, null, 0);
  n.elementType = "DELETED", n.stateNode = t, n.return = e, t = e.deletions, t === null ? (e.deletions = [n], e.flags |= 16) : t.push(n);
}
function vs(e, t) {
  switch (e.tag) {
    case 5:
      var n = e.type;
      return t = t.nodeType !== 1 || n.toLowerCase() !== t.nodeName.toLowerCase() ? null : t, t !== null ? (e.stateNode = t, Xe = e, Ye = Kt(t.firstChild), !0) : !1;
    case 6:
      return t = e.pendingProps === "" || t.nodeType !== 3 ? null : t, t !== null ? (e.stateNode = t, Xe = e, Ye = null, !0) : !1;
    case 13:
      return t = t.nodeType !== 8 ? null : t, t !== null ? (n = gn !== null ? { id: Pt, overflow: Ft } : null, e.memoizedState = { dehydrated: t, treeContext: n, retryLane: 1073741824 }, n = lt(18, null, null, 0), n.stateNode = t, n.return = e, e.child = n, Xe = e, Ye = null, !0) : !1;
    default:
      return !1;
  }
}
function hi(e) {
  return (e.mode & 1) !== 0 && (e.flags & 128) === 0;
}
function vi(e) {
  if (ie) {
    var t = Ye;
    if (t) {
      var n = t;
      if (!vs(e, t)) {
        if (hi(e)) throw Error(I(418));
        t = Kt(n.nextSibling);
        var r = Xe;
        t && vs(e, t) ? lc(r, n) : (e.flags = e.flags & -4097 | 2, ie = !1, Xe = e);
      }
    } else {
      if (hi(e)) throw Error(I(418));
      e.flags = e.flags & -4097 | 2, ie = !1, Xe = e;
    }
  }
}
function gs(e) {
  for (e = e.return; e !== null && e.tag !== 5 && e.tag !== 3 && e.tag !== 13; ) e = e.return;
  Xe = e;
}
function al(e) {
  if (e !== Xe) return !1;
  if (!ie) return gs(e), ie = !0, !1;
  var t;
  if ((t = e.tag !== 3) && !(t = e.tag !== 5) && (t = e.type, t = t !== "head" && t !== "body" && !di(e.type, e.memoizedProps)), t && (t = Ye)) {
    if (hi(e)) throw ac(), Error(I(418));
    for (; t; ) lc(e, t), t = Kt(t.nextSibling);
  }
  if (gs(e), e.tag === 13) {
    if (e = e.memoizedState, e = e !== null ? e.dehydrated : null, !e) throw Error(I(317));
    e: {
      for (e = e.nextSibling, t = 0; e; ) {
        if (e.nodeType === 8) {
          var n = e.data;
          if (n === "/$") {
            if (t === 0) {
              Ye = Kt(e.nextSibling);
              break e;
            }
            t--;
          } else n !== "$" && n !== "$!" && n !== "$?" || t++;
        }
        e = e.nextSibling;
      }
      Ye = null;
    }
  } else Ye = Xe ? Kt(e.stateNode.nextSibling) : null;
  return !0;
}
function ac() {
  for (var e = Ye; e; ) e = Kt(e.nextSibling);
}
function Qn() {
  Ye = Xe = null, ie = !1;
}
function lo(e) {
  mt === null ? mt = [e] : mt.push(e);
}
var ap = Dt.ReactCurrentBatchConfig;
function ir(e, t, n) {
  if (e = n.ref, e !== null && typeof e != "function" && typeof e != "object") {
    if (n._owner) {
      if (n = n._owner, n) {
        if (n.tag !== 1) throw Error(I(309));
        var r = n.stateNode;
      }
      if (!r) throw Error(I(147, e));
      var l = r, i = "" + e;
      return t !== null && t.ref !== null && typeof t.ref == "function" && t.ref._stringRef === i ? t.ref : (t = function(o) {
        var s = l.refs;
        o === null ? delete s[i] : s[i] = o;
      }, t._stringRef = i, t);
    }
    if (typeof e != "string") throw Error(I(284));
    if (!n._owner) throw Error(I(290, e));
  }
  return e;
}
function il(e, t) {
  throw e = Object.prototype.toString.call(t), Error(I(31, e === "[object Object]" ? "object with keys {" + Object.keys(t).join(", ") + "}" : e));
}
function xs(e) {
  var t = e._init;
  return t(e._payload);
}
function ic(e) {
  function t(f, p) {
    if (e) {
      var g = f.deletions;
      g === null ? (f.deletions = [p], f.flags |= 16) : g.push(p);
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
    return f = Zt(f, p), f.index = 0, f.sibling = null, f;
  }
  function i(f, p, g) {
    return f.index = g, e ? (g = f.alternate, g !== null ? (g = g.index, g < p ? (f.flags |= 2, p) : g) : (f.flags |= 2, p)) : (f.flags |= 1048576, p);
  }
  function o(f) {
    return e && f.alternate === null && (f.flags |= 2), f;
  }
  function s(f, p, g, k) {
    return p === null || p.tag !== 6 ? (p = $a(g, f.mode, k), p.return = f, p) : (p = l(p, g), p.return = f, p);
  }
  function u(f, p, g, k) {
    var T = g.type;
    return T === In ? N(f, p, g.props.children, k, g.key) : p !== null && (p.elementType === T || typeof T == "object" && T !== null && T.$$typeof === At && xs(T) === p.type) ? (k = l(p, g.props), k.ref = ir(f, p, g), k.return = f, k) : (k = wl(g.type, g.key, g.props, null, f.mode, k), k.ref = ir(f, p, g), k.return = f, k);
  }
  function d(f, p, g, k) {
    return p === null || p.tag !== 4 || p.stateNode.containerInfo !== g.containerInfo || p.stateNode.implementation !== g.implementation ? (p = Oa(g, f.mode, k), p.return = f, p) : (p = l(p, g.children || []), p.return = f, p);
  }
  function N(f, p, g, k, T) {
    return p === null || p.tag !== 7 ? (p = mn(g, f.mode, k, T), p.return = f, p) : (p = l(p, g), p.return = f, p);
  }
  function c(f, p, g) {
    if (typeof p == "string" && p !== "" || typeof p == "number") return p = $a("" + p, f.mode, g), p.return = f, p;
    if (typeof p == "object" && p !== null) {
      switch (p.$$typeof) {
        case qr:
          return g = wl(p.type, p.key, p.props, null, f.mode, g), g.ref = ir(f, null, p), g.return = f, g;
        case En:
          return p = Oa(p, f.mode, g), p.return = f, p;
        case At:
          var k = p._init;
          return c(f, k(p._payload), g);
      }
      if (cr(p) || tr(p)) return p = mn(p, f.mode, g, null), p.return = f, p;
      il(f, p);
    }
    return null;
  }
  function h(f, p, g, k) {
    var T = p !== null ? p.key : null;
    if (typeof g == "string" && g !== "" || typeof g == "number") return T !== null ? null : s(f, p, "" + g, k);
    if (typeof g == "object" && g !== null) {
      switch (g.$$typeof) {
        case qr:
          return g.key === T ? u(f, p, g, k) : null;
        case En:
          return g.key === T ? d(f, p, g, k) : null;
        case At:
          return T = g._init, h(
            f,
            p,
            T(g._payload),
            k
          );
      }
      if (cr(g) || tr(g)) return T !== null ? null : N(f, p, g, k, null);
      il(f, g);
    }
    return null;
  }
  function v(f, p, g, k, T) {
    if (typeof k == "string" && k !== "" || typeof k == "number") return f = f.get(g) || null, s(p, f, "" + k, T);
    if (typeof k == "object" && k !== null) {
      switch (k.$$typeof) {
        case qr:
          return f = f.get(k.key === null ? g : k.key) || null, u(p, f, k, T);
        case En:
          return f = f.get(k.key === null ? g : k.key) || null, d(p, f, k, T);
        case At:
          var R = k._init;
          return v(f, p, g, R(k._payload), T);
      }
      if (cr(k) || tr(k)) return f = f.get(g) || null, N(p, f, k, T, null);
      il(p, k);
    }
    return null;
  }
  function y(f, p, g, k) {
    for (var T = null, R = null, z = p, M = p = 0, A = null; z !== null && M < g.length; M++) {
      z.index > M ? (A = z, z = null) : A = z.sibling;
      var U = h(f, z, g[M], k);
      if (U === null) {
        z === null && (z = A);
        break;
      }
      e && z && U.alternate === null && t(f, z), p = i(U, p, M), R === null ? T = U : R.sibling = U, R = U, z = A;
    }
    if (M === g.length) return n(f, z), ie && sn(f, M), T;
    if (z === null) {
      for (; M < g.length; M++) z = c(f, g[M], k), z !== null && (p = i(z, p, M), R === null ? T = z : R.sibling = z, R = z);
      return ie && sn(f, M), T;
    }
    for (z = r(f, z); M < g.length; M++) A = v(z, f, M, g[M], k), A !== null && (e && A.alternate !== null && z.delete(A.key === null ? M : A.key), p = i(A, p, M), R === null ? T = A : R.sibling = A, R = A);
    return e && z.forEach(function(F) {
      return t(f, F);
    }), ie && sn(f, M), T;
  }
  function w(f, p, g, k) {
    var T = tr(g);
    if (typeof T != "function") throw Error(I(150));
    if (g = T.call(g), g == null) throw Error(I(151));
    for (var R = T = null, z = p, M = p = 0, A = null, U = g.next(); z !== null && !U.done; M++, U = g.next()) {
      z.index > M ? (A = z, z = null) : A = z.sibling;
      var F = h(f, z, U.value, k);
      if (F === null) {
        z === null && (z = A);
        break;
      }
      e && z && F.alternate === null && t(f, z), p = i(F, p, M), R === null ? T = F : R.sibling = F, R = F, z = A;
    }
    if (U.done) return n(
      f,
      z
    ), ie && sn(f, M), T;
    if (z === null) {
      for (; !U.done; M++, U = g.next()) U = c(f, U.value, k), U !== null && (p = i(U, p, M), R === null ? T = U : R.sibling = U, R = U);
      return ie && sn(f, M), T;
    }
    for (z = r(f, z); !U.done; M++, U = g.next()) U = v(z, f, M, U.value, k), U !== null && (e && U.alternate !== null && z.delete(U.key === null ? M : U.key), p = i(U, p, M), R === null ? T = U : R.sibling = U, R = U);
    return e && z.forEach(function(Q) {
      return t(f, Q);
    }), ie && sn(f, M), T;
  }
  function _(f, p, g, k) {
    if (typeof g == "object" && g !== null && g.type === In && g.key === null && (g = g.props.children), typeof g == "object" && g !== null) {
      switch (g.$$typeof) {
        case qr:
          e: {
            for (var T = g.key, R = p; R !== null; ) {
              if (R.key === T) {
                if (T = g.type, T === In) {
                  if (R.tag === 7) {
                    n(f, R.sibling), p = l(R, g.props.children), p.return = f, f = p;
                    break e;
                  }
                } else if (R.elementType === T || typeof T == "object" && T !== null && T.$$typeof === At && xs(T) === R.type) {
                  n(f, R.sibling), p = l(R, g.props), p.ref = ir(f, R, g), p.return = f, f = p;
                  break e;
                }
                n(f, R);
                break;
              } else t(f, R);
              R = R.sibling;
            }
            g.type === In ? (p = mn(g.props.children, f.mode, k, g.key), p.return = f, f = p) : (k = wl(g.type, g.key, g.props, null, f.mode, k), k.ref = ir(f, p, g), k.return = f, f = k);
          }
          return o(f);
        case En:
          e: {
            for (R = g.key; p !== null; ) {
              if (p.key === R) if (p.tag === 4 && p.stateNode.containerInfo === g.containerInfo && p.stateNode.implementation === g.implementation) {
                n(f, p.sibling), p = l(p, g.children || []), p.return = f, f = p;
                break e;
              } else {
                n(f, p);
                break;
              }
              else t(f, p);
              p = p.sibling;
            }
            p = Oa(g, f.mode, k), p.return = f, f = p;
          }
          return o(f);
        case At:
          return R = g._init, _(f, p, R(g._payload), k);
      }
      if (cr(g)) return y(f, p, g, k);
      if (tr(g)) return w(f, p, g, k);
      il(f, g);
    }
    return typeof g == "string" && g !== "" || typeof g == "number" ? (g = "" + g, p !== null && p.tag === 6 ? (n(f, p.sibling), p = l(p, g), p.return = f, f = p) : (n(f, p), p = $a(g, f.mode, k), p.return = f, f = p), o(f)) : n(f, p);
  }
  return _;
}
var Gn = ic(!0), oc = ic(!1), Ml = nn(null), $l = null, Dn = null, ao = null;
function io() {
  ao = Dn = $l = null;
}
function oo(e) {
  var t = Ml.current;
  le(Ml), e._currentValue = t;
}
function gi(e, t, n) {
  for (; e !== null; ) {
    var r = e.alternate;
    if ((e.childLanes & t) !== t ? (e.childLanes |= t, r !== null && (r.childLanes |= t)) : r !== null && (r.childLanes & t) !== t && (r.childLanes |= t), e === n) break;
    e = e.return;
  }
}
function Bn(e, t) {
  $l = e, ao = Dn = null, e = e.dependencies, e !== null && e.firstContext !== null && (e.lanes & t && (He = !0), e.firstContext = null);
}
function it(e) {
  var t = e._currentValue;
  if (ao !== e) if (e = { context: e, memoizedValue: t, next: null }, Dn === null) {
    if ($l === null) throw Error(I(308));
    Dn = e, $l.dependencies = { lanes: 0, firstContext: e };
  } else Dn = Dn.next = e;
  return t;
}
var dn = null;
function so(e) {
  dn === null ? dn = [e] : dn.push(e);
}
function sc(e, t, n, r) {
  var l = t.interleaved;
  return l === null ? (n.next = n, so(t)) : (n.next = l.next, l.next = n), t.interleaved = n, zt(e, r);
}
function zt(e, t) {
  e.lanes |= t;
  var n = e.alternate;
  for (n !== null && (n.lanes |= t), n = e, e = e.return; e !== null; ) e.childLanes |= t, n = e.alternate, n !== null && (n.childLanes |= t), n = e, e = e.return;
  return n.tag === 3 ? n.stateNode : null;
}
var Ut = !1;
function uo(e) {
  e.updateQueue = { baseState: e.memoizedState, firstBaseUpdate: null, lastBaseUpdate: null, shared: { pending: null, interleaved: null, lanes: 0 }, effects: null };
}
function uc(e, t) {
  e = e.updateQueue, t.updateQueue === e && (t.updateQueue = { baseState: e.baseState, firstBaseUpdate: e.firstBaseUpdate, lastBaseUpdate: e.lastBaseUpdate, shared: e.shared, effects: e.effects });
}
function _t(e, t) {
  return { eventTime: e, lane: t, tag: 0, payload: null, callback: null, next: null };
}
function qt(e, t, n) {
  var r = e.updateQueue;
  if (r === null) return null;
  if (r = r.shared, q & 2) {
    var l = r.pending;
    return l === null ? t.next = t : (t.next = l.next, l.next = t), r.pending = t, zt(e, n);
  }
  return l = r.interleaved, l === null ? (t.next = t, so(r)) : (t.next = l.next, l.next = t), r.interleaved = t, zt(e, n);
}
function vl(e, t, n) {
  if (t = t.updateQueue, t !== null && (t = t.shared, (n & 4194240) !== 0)) {
    var r = t.lanes;
    r &= e.pendingLanes, n |= r, t.lanes = n, Ki(e, n);
  }
}
function ys(e, t) {
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
function Ol(e, t, n, r) {
  var l = e.updateQueue;
  Ut = !1;
  var i = l.firstBaseUpdate, o = l.lastBaseUpdate, s = l.shared.pending;
  if (s !== null) {
    l.shared.pending = null;
    var u = s, d = u.next;
    u.next = null, o === null ? i = d : o.next = d, o = u;
    var N = e.alternate;
    N !== null && (N = N.updateQueue, s = N.lastBaseUpdate, s !== o && (s === null ? N.firstBaseUpdate = d : s.next = d, N.lastBaseUpdate = u));
  }
  if (i !== null) {
    var c = l.baseState;
    o = 0, N = d = u = null, s = i;
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
          var y = e, w = s;
          switch (h = t, v = n, w.tag) {
            case 1:
              if (y = w.payload, typeof y == "function") {
                c = y.call(v, c, h);
                break e;
              }
              c = y;
              break e;
            case 3:
              y.flags = y.flags & -65537 | 128;
            case 0:
              if (y = w.payload, h = typeof y == "function" ? y.call(v, c, h) : y, h == null) break e;
              c = ce({}, c, h);
              break e;
            case 2:
              Ut = !0;
          }
        }
        s.callback !== null && s.lane !== 0 && (e.flags |= 64, h = l.effects, h === null ? l.effects = [s] : h.push(s));
      } else v = { eventTime: v, lane: h, tag: s.tag, payload: s.payload, callback: s.callback, next: null }, N === null ? (d = N = v, u = c) : N = N.next = v, o |= h;
      if (s = s.next, s === null) {
        if (s = l.shared.pending, s === null) break;
        h = s, s = h.next, h.next = null, l.lastBaseUpdate = h, l.shared.pending = null;
      }
    } while (!0);
    if (N === null && (u = c), l.baseState = u, l.firstBaseUpdate = d, l.lastBaseUpdate = N, t = l.shared.interleaved, t !== null) {
      l = t;
      do
        o |= l.lane, l = l.next;
      while (l !== t);
    } else i === null && (l.shared.lanes = 0);
    yn |= o, e.lanes = o, e.memoizedState = c;
  }
}
function js(e, t, n) {
  if (e = t.effects, t.effects = null, e !== null) for (t = 0; t < e.length; t++) {
    var r = e[t], l = r.callback;
    if (l !== null) {
      if (r.callback = null, r = n, typeof l != "function") throw Error(I(191, l));
      l.call(r);
    }
  }
}
var Wr = {}, kt = nn(Wr), Lr = nn(Wr), Dr = nn(Wr);
function fn(e) {
  if (e === Wr) throw Error(I(174));
  return e;
}
function co(e, t) {
  switch (ne(Dr, t), ne(Lr, e), ne(kt, Wr), e = t.nodeType, e) {
    case 9:
    case 11:
      t = (t = t.documentElement) ? t.namespaceURI : Xa(null, "");
      break;
    default:
      e = e === 8 ? t.parentNode : t, t = e.namespaceURI || null, e = e.tagName, t = Xa(t, e);
  }
  le(kt), ne(kt, t);
}
function Kn() {
  le(kt), le(Lr), le(Dr);
}
function cc(e) {
  fn(Dr.current);
  var t = fn(kt.current), n = Xa(t, e.type);
  t !== n && (ne(Lr, e), ne(kt, n));
}
function fo(e) {
  Lr.current === e && (le(kt), le(Lr));
}
var se = nn(0);
function Al(e) {
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
function po() {
  for (var e = 0; e < Ra.length; e++) Ra[e]._workInProgressVersionPrimary = null;
  Ra.length = 0;
}
var gl = Dt.ReactCurrentDispatcher, Ta = Dt.ReactCurrentBatchConfig, xn = 0, ue = null, ye = null, we = null, Ul = !1, xr = !1, Mr = 0, ip = 0;
function _e() {
  throw Error(I(321));
}
function mo(e, t) {
  if (t === null) return !1;
  for (var n = 0; n < t.length && n < e.length; n++) if (!yt(e[n], t[n])) return !1;
  return !0;
}
function ho(e, t, n, r, l, i) {
  if (xn = i, ue = t, t.memoizedState = null, t.updateQueue = null, t.lanes = 0, gl.current = e === null || e.memoizedState === null ? cp : dp, e = n(r, l), xr) {
    i = 0;
    do {
      if (xr = !1, Mr = 0, 25 <= i) throw Error(I(301));
      i += 1, we = ye = null, t.updateQueue = null, gl.current = fp, e = n(r, l);
    } while (xr);
  }
  if (gl.current = Vl, t = ye !== null && ye.next !== null, xn = 0, we = ye = ue = null, Ul = !1, t) throw Error(I(300));
  return e;
}
function vo() {
  var e = Mr !== 0;
  return Mr = 0, e;
}
function Nt() {
  var e = { memoizedState: null, baseState: null, baseQueue: null, queue: null, next: null };
  return we === null ? ue.memoizedState = we = e : we = we.next = e, we;
}
function ot() {
  if (ye === null) {
    var e = ue.alternate;
    e = e !== null ? e.memoizedState : null;
  } else e = ye.next;
  var t = we === null ? ue.memoizedState : we.next;
  if (t !== null) we = t, ye = e;
  else {
    if (e === null) throw Error(I(310));
    ye = e, e = { memoizedState: ye.memoizedState, baseState: ye.baseState, baseQueue: ye.baseQueue, queue: ye.queue, next: null }, we === null ? ue.memoizedState = we = e : we = we.next = e;
  }
  return we;
}
function $r(e, t) {
  return typeof t == "function" ? t(e) : t;
}
function za(e) {
  var t = ot(), n = t.queue;
  if (n === null) throw Error(I(311));
  n.lastRenderedReducer = e;
  var r = ye, l = r.baseQueue, i = n.pending;
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
      var N = d.lane;
      if ((xn & N) === N) u !== null && (u = u.next = { lane: 0, action: d.action, hasEagerState: d.hasEagerState, eagerState: d.eagerState, next: null }), r = d.hasEagerState ? d.eagerState : e(r, d.action);
      else {
        var c = {
          lane: N,
          action: d.action,
          hasEagerState: d.hasEagerState,
          eagerState: d.eagerState,
          next: null
        };
        u === null ? (s = u = c, o = r) : u = u.next = c, ue.lanes |= N, yn |= N;
      }
      d = d.next;
    } while (d !== null && d !== i);
    u === null ? o = r : u.next = s, yt(r, t.memoizedState) || (He = !0), t.memoizedState = r, t.baseState = o, t.baseQueue = u, n.lastRenderedState = r;
  }
  if (e = n.interleaved, e !== null) {
    l = e;
    do
      i = l.lane, ue.lanes |= i, yn |= i, l = l.next;
    while (l !== e);
  } else l === null && (n.lanes = 0);
  return [t.memoizedState, n.dispatch];
}
function La(e) {
  var t = ot(), n = t.queue;
  if (n === null) throw Error(I(311));
  n.lastRenderedReducer = e;
  var r = n.dispatch, l = n.pending, i = t.memoizedState;
  if (l !== null) {
    n.pending = null;
    var o = l = l.next;
    do
      i = e(i, o.action), o = o.next;
    while (o !== l);
    yt(i, t.memoizedState) || (He = !0), t.memoizedState = i, t.baseQueue === null && (t.baseState = i), n.lastRenderedState = i;
  }
  return [i, r];
}
function dc() {
}
function fc(e, t) {
  var n = ue, r = ot(), l = t(), i = !yt(r.memoizedState, l);
  if (i && (r.memoizedState = l, He = !0), r = r.queue, go(hc.bind(null, n, r, e), [e]), r.getSnapshot !== t || i || we !== null && we.memoizedState.tag & 1) {
    if (n.flags |= 2048, Or(9, mc.bind(null, n, r, l, t), void 0, null), Ce === null) throw Error(I(349));
    xn & 30 || pc(n, t, l);
  }
  return l;
}
function pc(e, t, n) {
  e.flags |= 16384, e = { getSnapshot: t, value: n }, t = ue.updateQueue, t === null ? (t = { lastEffect: null, stores: null }, ue.updateQueue = t, t.stores = [e]) : (n = t.stores, n === null ? t.stores = [e] : n.push(e));
}
function mc(e, t, n, r) {
  t.value = n, t.getSnapshot = r, vc(t) && gc(e);
}
function hc(e, t, n) {
  return n(function() {
    vc(t) && gc(e);
  });
}
function vc(e) {
  var t = e.getSnapshot;
  e = e.value;
  try {
    var n = t();
    return !yt(e, n);
  } catch {
    return !0;
  }
}
function gc(e) {
  var t = zt(e, 1);
  t !== null && gt(t, e, 1, -1);
}
function Ns(e) {
  var t = Nt();
  return typeof e == "function" && (e = e()), t.memoizedState = t.baseState = e, e = { pending: null, interleaved: null, lanes: 0, dispatch: null, lastRenderedReducer: $r, lastRenderedState: e }, t.queue = e, e = e.dispatch = up.bind(null, ue, e), [t.memoizedState, e];
}
function Or(e, t, n, r) {
  return e = { tag: e, create: t, destroy: n, deps: r, next: null }, t = ue.updateQueue, t === null ? (t = { lastEffect: null, stores: null }, ue.updateQueue = t, t.lastEffect = e.next = e) : (n = t.lastEffect, n === null ? t.lastEffect = e.next = e : (r = n.next, n.next = e, e.next = r, t.lastEffect = e)), e;
}
function xc() {
  return ot().memoizedState;
}
function xl(e, t, n, r) {
  var l = Nt();
  ue.flags |= e, l.memoizedState = Or(1 | t, n, void 0, r === void 0 ? null : r);
}
function ea(e, t, n, r) {
  var l = ot();
  r = r === void 0 ? null : r;
  var i = void 0;
  if (ye !== null) {
    var o = ye.memoizedState;
    if (i = o.destroy, r !== null && mo(r, o.deps)) {
      l.memoizedState = Or(t, n, i, r);
      return;
    }
  }
  ue.flags |= e, l.memoizedState = Or(1 | t, n, i, r);
}
function ws(e, t) {
  return xl(8390656, 8, e, t);
}
function go(e, t) {
  return ea(2048, 8, e, t);
}
function yc(e, t) {
  return ea(4, 2, e, t);
}
function jc(e, t) {
  return ea(4, 4, e, t);
}
function Nc(e, t) {
  if (typeof t == "function") return e = e(), t(e), function() {
    t(null);
  };
  if (t != null) return e = e(), t.current = e, function() {
    t.current = null;
  };
}
function wc(e, t, n) {
  return n = n != null ? n.concat([e]) : null, ea(4, 4, Nc.bind(null, t, e), n);
}
function xo() {
}
function Sc(e, t) {
  var n = ot();
  t = t === void 0 ? null : t;
  var r = n.memoizedState;
  return r !== null && t !== null && mo(t, r[1]) ? r[0] : (n.memoizedState = [e, t], e);
}
function kc(e, t) {
  var n = ot();
  t = t === void 0 ? null : t;
  var r = n.memoizedState;
  return r !== null && t !== null && mo(t, r[1]) ? r[0] : (e = e(), n.memoizedState = [e, t], e);
}
function Cc(e, t, n) {
  return xn & 21 ? (yt(n, t) || (n = _u(), ue.lanes |= n, yn |= n, e.baseState = !0), t) : (e.baseState && (e.baseState = !1, He = !0), e.memoizedState = n);
}
function op(e, t) {
  var n = Y;
  Y = n !== 0 && 4 > n ? n : 4, e(!0);
  var r = Ta.transition;
  Ta.transition = {};
  try {
    e(!1), t();
  } finally {
    Y = n, Ta.transition = r;
  }
}
function Ec() {
  return ot().memoizedState;
}
function sp(e, t, n) {
  var r = Xt(e);
  if (n = { lane: r, action: n, hasEagerState: !1, eagerState: null, next: null }, Ic(e)) Pc(t, n);
  else if (n = sc(e, t, n, r), n !== null) {
    var l = Oe();
    gt(n, e, r, l), Fc(n, t, r);
  }
}
function up(e, t, n) {
  var r = Xt(e), l = { lane: r, action: n, hasEagerState: !1, eagerState: null, next: null };
  if (Ic(e)) Pc(t, l);
  else {
    var i = e.alternate;
    if (e.lanes === 0 && (i === null || i.lanes === 0) && (i = t.lastRenderedReducer, i !== null)) try {
      var o = t.lastRenderedState, s = i(o, n);
      if (l.hasEagerState = !0, l.eagerState = s, yt(s, o)) {
        var u = t.interleaved;
        u === null ? (l.next = l, so(t)) : (l.next = u.next, u.next = l), t.interleaved = l;
        return;
      }
    } catch {
    } finally {
    }
    n = sc(e, t, l, r), n !== null && (l = Oe(), gt(n, e, r, l), Fc(n, t, r));
  }
}
function Ic(e) {
  var t = e.alternate;
  return e === ue || t !== null && t === ue;
}
function Pc(e, t) {
  xr = Ul = !0;
  var n = e.pending;
  n === null ? t.next = t : (t.next = n.next, n.next = t), e.pending = t;
}
function Fc(e, t, n) {
  if (n & 4194240) {
    var r = t.lanes;
    r &= e.pendingLanes, n |= r, t.lanes = n, Ki(e, n);
  }
}
var Vl = { readContext: it, useCallback: _e, useContext: _e, useEffect: _e, useImperativeHandle: _e, useInsertionEffect: _e, useLayoutEffect: _e, useMemo: _e, useReducer: _e, useRef: _e, useState: _e, useDebugValue: _e, useDeferredValue: _e, useTransition: _e, useMutableSource: _e, useSyncExternalStore: _e, useId: _e, unstable_isNewReconciler: !1 }, cp = { readContext: it, useCallback: function(e, t) {
  return Nt().memoizedState = [e, t === void 0 ? null : t], e;
}, useContext: it, useEffect: ws, useImperativeHandle: function(e, t, n) {
  return n = n != null ? n.concat([e]) : null, xl(
    4194308,
    4,
    Nc.bind(null, t, e),
    n
  );
}, useLayoutEffect: function(e, t) {
  return xl(4194308, 4, e, t);
}, useInsertionEffect: function(e, t) {
  return xl(4, 2, e, t);
}, useMemo: function(e, t) {
  var n = Nt();
  return t = t === void 0 ? null : t, e = e(), n.memoizedState = [e, t], e;
}, useReducer: function(e, t, n) {
  var r = Nt();
  return t = n !== void 0 ? n(t) : t, r.memoizedState = r.baseState = t, e = { pending: null, interleaved: null, lanes: 0, dispatch: null, lastRenderedReducer: e, lastRenderedState: t }, r.queue = e, e = e.dispatch = sp.bind(null, ue, e), [r.memoizedState, e];
}, useRef: function(e) {
  var t = Nt();
  return e = { current: e }, t.memoizedState = e;
}, useState: Ns, useDebugValue: xo, useDeferredValue: function(e) {
  return Nt().memoizedState = e;
}, useTransition: function() {
  var e = Ns(!1), t = e[0];
  return e = op.bind(null, e[1]), Nt().memoizedState = e, [t, e];
}, useMutableSource: function() {
}, useSyncExternalStore: function(e, t, n) {
  var r = ue, l = Nt();
  if (ie) {
    if (n === void 0) throw Error(I(407));
    n = n();
  } else {
    if (n = t(), Ce === null) throw Error(I(349));
    xn & 30 || pc(r, t, n);
  }
  l.memoizedState = n;
  var i = { value: n, getSnapshot: t };
  return l.queue = i, ws(hc.bind(
    null,
    r,
    i,
    e
  ), [e]), r.flags |= 2048, Or(9, mc.bind(null, r, i, n, t), void 0, null), n;
}, useId: function() {
  var e = Nt(), t = Ce.identifierPrefix;
  if (ie) {
    var n = Ft, r = Pt;
    n = (r & ~(1 << 32 - vt(r) - 1)).toString(32) + n, t = ":" + t + "R" + n, n = Mr++, 0 < n && (t += "H" + n.toString(32)), t += ":";
  } else n = ip++, t = ":" + t + "r" + n.toString(32) + ":";
  return e.memoizedState = t;
}, unstable_isNewReconciler: !1 }, dp = {
  readContext: it,
  useCallback: Sc,
  useContext: it,
  useEffect: go,
  useImperativeHandle: wc,
  useInsertionEffect: yc,
  useLayoutEffect: jc,
  useMemo: kc,
  useReducer: za,
  useRef: xc,
  useState: function() {
    return za($r);
  },
  useDebugValue: xo,
  useDeferredValue: function(e) {
    var t = ot();
    return Cc(t, ye.memoizedState, e);
  },
  useTransition: function() {
    var e = za($r)[0], t = ot().memoizedState;
    return [e, t];
  },
  useMutableSource: dc,
  useSyncExternalStore: fc,
  useId: Ec,
  unstable_isNewReconciler: !1
}, fp = { readContext: it, useCallback: Sc, useContext: it, useEffect: go, useImperativeHandle: wc, useInsertionEffect: yc, useLayoutEffect: jc, useMemo: kc, useReducer: La, useRef: xc, useState: function() {
  return La($r);
}, useDebugValue: xo, useDeferredValue: function(e) {
  var t = ot();
  return ye === null ? t.memoizedState = e : Cc(t, ye.memoizedState, e);
}, useTransition: function() {
  var e = La($r)[0], t = ot().memoizedState;
  return [e, t];
}, useMutableSource: dc, useSyncExternalStore: fc, useId: Ec, unstable_isNewReconciler: !1 };
function ft(e, t) {
  if (e && e.defaultProps) {
    t = ce({}, t), e = e.defaultProps;
    for (var n in e) t[n] === void 0 && (t[n] = e[n]);
    return t;
  }
  return t;
}
function xi(e, t, n, r) {
  t = e.memoizedState, n = n(r, t), n = n == null ? t : ce({}, t, n), e.memoizedState = n, e.lanes === 0 && (e.updateQueue.baseState = n);
}
var ta = { isMounted: function(e) {
  return (e = e._reactInternals) ? Sn(e) === e : !1;
}, enqueueSetState: function(e, t, n) {
  e = e._reactInternals;
  var r = Oe(), l = Xt(e), i = _t(r, l);
  i.payload = t, n != null && (i.callback = n), t = qt(e, i, l), t !== null && (gt(t, e, l, r), vl(t, e, l));
}, enqueueReplaceState: function(e, t, n) {
  e = e._reactInternals;
  var r = Oe(), l = Xt(e), i = _t(r, l);
  i.tag = 1, i.payload = t, n != null && (i.callback = n), t = qt(e, i, l), t !== null && (gt(t, e, l, r), vl(t, e, l));
}, enqueueForceUpdate: function(e, t) {
  e = e._reactInternals;
  var n = Oe(), r = Xt(e), l = _t(n, r);
  l.tag = 2, t != null && (l.callback = t), t = qt(e, l, r), t !== null && (gt(t, e, r, n), vl(t, e, r));
} };
function Ss(e, t, n, r, l, i, o) {
  return e = e.stateNode, typeof e.shouldComponentUpdate == "function" ? e.shouldComponentUpdate(r, i, o) : t.prototype && t.prototype.isPureReactComponent ? !_r(n, r) || !_r(l, i) : !0;
}
function _c(e, t, n) {
  var r = !1, l = en, i = t.contextType;
  return typeof i == "object" && i !== null ? i = it(i) : (l = Qe(t) ? vn : Le.current, r = t.contextTypes, i = (r = r != null) ? Wn(e, l) : en), t = new t(n, i), e.memoizedState = t.state !== null && t.state !== void 0 ? t.state : null, t.updater = ta, e.stateNode = t, t._reactInternals = e, r && (e = e.stateNode, e.__reactInternalMemoizedUnmaskedChildContext = l, e.__reactInternalMemoizedMaskedChildContext = i), t;
}
function ks(e, t, n, r) {
  e = t.state, typeof t.componentWillReceiveProps == "function" && t.componentWillReceiveProps(n, r), typeof t.UNSAFE_componentWillReceiveProps == "function" && t.UNSAFE_componentWillReceiveProps(n, r), t.state !== e && ta.enqueueReplaceState(t, t.state, null);
}
function yi(e, t, n, r) {
  var l = e.stateNode;
  l.props = n, l.state = e.memoizedState, l.refs = {}, uo(e);
  var i = t.contextType;
  typeof i == "object" && i !== null ? l.context = it(i) : (i = Qe(t) ? vn : Le.current, l.context = Wn(e, i)), l.state = e.memoizedState, i = t.getDerivedStateFromProps, typeof i == "function" && (xi(e, t, i, n), l.state = e.memoizedState), typeof t.getDerivedStateFromProps == "function" || typeof l.getSnapshotBeforeUpdate == "function" || typeof l.UNSAFE_componentWillMount != "function" && typeof l.componentWillMount != "function" || (t = l.state, typeof l.componentWillMount == "function" && l.componentWillMount(), typeof l.UNSAFE_componentWillMount == "function" && l.UNSAFE_componentWillMount(), t !== l.state && ta.enqueueReplaceState(l, l.state, null), Ol(e, n, l, r), l.state = e.memoizedState), typeof l.componentDidMount == "function" && (e.flags |= 4194308);
}
function qn(e, t) {
  try {
    var n = "", r = t;
    do
      n += Ad(r), r = r.return;
    while (r);
    var l = n;
  } catch (i) {
    l = `
Error generating stack: ` + i.message + `
` + i.stack;
  }
  return { value: e, source: t, stack: l, digest: null };
}
function Da(e, t, n) {
  return { value: e, source: null, stack: n ?? null, digest: t ?? null };
}
function ji(e, t) {
  try {
    console.error(t.value);
  } catch (n) {
    setTimeout(function() {
      throw n;
    });
  }
}
var pp = typeof WeakMap == "function" ? WeakMap : Map;
function Rc(e, t, n) {
  n = _t(-1, n), n.tag = 3, n.payload = { element: null };
  var r = t.value;
  return n.callback = function() {
    bl || (bl = !0, _i = r), ji(e, t);
  }, n;
}
function Tc(e, t, n) {
  n = _t(-1, n), n.tag = 3;
  var r = e.type.getDerivedStateFromError;
  if (typeof r == "function") {
    var l = t.value;
    n.payload = function() {
      return r(l);
    }, n.callback = function() {
      ji(e, t);
    };
  }
  var i = e.stateNode;
  return i !== null && typeof i.componentDidCatch == "function" && (n.callback = function() {
    ji(e, t), typeof r != "function" && (Yt === null ? Yt = /* @__PURE__ */ new Set([this]) : Yt.add(this));
    var o = t.stack;
    this.componentDidCatch(t.value, { componentStack: o !== null ? o : "" });
  }), n;
}
function Cs(e, t, n) {
  var r = e.pingCache;
  if (r === null) {
    r = e.pingCache = new pp();
    var l = /* @__PURE__ */ new Set();
    r.set(t, l);
  } else l = r.get(t), l === void 0 && (l = /* @__PURE__ */ new Set(), r.set(t, l));
  l.has(n) || (l.add(n), e = Ip.bind(null, e, t, n), t.then(e, e));
}
function Es(e) {
  do {
    var t;
    if ((t = e.tag === 13) && (t = e.memoizedState, t = t !== null ? t.dehydrated !== null : !0), t) return e;
    e = e.return;
  } while (e !== null);
  return null;
}
function Is(e, t, n, r, l) {
  return e.mode & 1 ? (e.flags |= 65536, e.lanes = l, e) : (e === t ? e.flags |= 65536 : (e.flags |= 128, n.flags |= 131072, n.flags &= -52805, n.tag === 1 && (n.alternate === null ? n.tag = 17 : (t = _t(-1, 1), t.tag = 2, qt(n, t, 1))), n.lanes |= 1), e);
}
var mp = Dt.ReactCurrentOwner, He = !1;
function Me(e, t, n, r) {
  t.child = e === null ? oc(t, null, n, r) : Gn(t, e.child, n, r);
}
function Ps(e, t, n, r, l) {
  n = n.render;
  var i = t.ref;
  return Bn(t, l), r = ho(e, t, n, r, i, l), n = vo(), e !== null && !He ? (t.updateQueue = e.updateQueue, t.flags &= -2053, e.lanes &= ~l, Lt(e, t, l)) : (ie && n && no(t), t.flags |= 1, Me(e, t, r, l), t.child);
}
function Fs(e, t, n, r, l) {
  if (e === null) {
    var i = n.type;
    return typeof i == "function" && !Eo(i) && i.defaultProps === void 0 && n.compare === null && n.defaultProps === void 0 ? (t.tag = 15, t.type = i, zc(e, t, i, r, l)) : (e = wl(n.type, null, r, t, t.mode, l), e.ref = t.ref, e.return = t, t.child = e);
  }
  if (i = e.child, !(e.lanes & l)) {
    var o = i.memoizedProps;
    if (n = n.compare, n = n !== null ? n : _r, n(o, r) && e.ref === t.ref) return Lt(e, t, l);
  }
  return t.flags |= 1, e = Zt(i, r), e.ref = t.ref, e.return = t, t.child = e;
}
function zc(e, t, n, r, l) {
  if (e !== null) {
    var i = e.memoizedProps;
    if (_r(i, r) && e.ref === t.ref) if (He = !1, t.pendingProps = r = i, (e.lanes & l) !== 0) e.flags & 131072 && (He = !0);
    else return t.lanes = e.lanes, Lt(e, t, l);
  }
  return Ni(e, t, n, r, l);
}
function Lc(e, t, n) {
  var r = t.pendingProps, l = r.children, i = e !== null ? e.memoizedState : null;
  if (r.mode === "hidden") if (!(t.mode & 1)) t.memoizedState = { baseLanes: 0, cachePool: null, transitions: null }, ne($n, qe), qe |= n;
  else {
    if (!(n & 1073741824)) return e = i !== null ? i.baseLanes | n : n, t.lanes = t.childLanes = 1073741824, t.memoizedState = { baseLanes: e, cachePool: null, transitions: null }, t.updateQueue = null, ne($n, qe), qe |= e, null;
    t.memoizedState = { baseLanes: 0, cachePool: null, transitions: null }, r = i !== null ? i.baseLanes : n, ne($n, qe), qe |= r;
  }
  else i !== null ? (r = i.baseLanes | n, t.memoizedState = null) : r = n, ne($n, qe), qe |= r;
  return Me(e, t, l, n), t.child;
}
function Dc(e, t) {
  var n = t.ref;
  (e === null && n !== null || e !== null && e.ref !== n) && (t.flags |= 512, t.flags |= 2097152);
}
function Ni(e, t, n, r, l) {
  var i = Qe(n) ? vn : Le.current;
  return i = Wn(t, i), Bn(t, l), n = ho(e, t, n, r, i, l), r = vo(), e !== null && !He ? (t.updateQueue = e.updateQueue, t.flags &= -2053, e.lanes &= ~l, Lt(e, t, l)) : (ie && r && no(t), t.flags |= 1, Me(e, t, n, l), t.child);
}
function _s(e, t, n, r, l) {
  if (Qe(n)) {
    var i = !0;
    zl(t);
  } else i = !1;
  if (Bn(t, l), t.stateNode === null) yl(e, t), _c(t, n, r), yi(t, n, r, l), r = !0;
  else if (e === null) {
    var o = t.stateNode, s = t.memoizedProps;
    o.props = s;
    var u = o.context, d = n.contextType;
    typeof d == "object" && d !== null ? d = it(d) : (d = Qe(n) ? vn : Le.current, d = Wn(t, d));
    var N = n.getDerivedStateFromProps, c = typeof N == "function" || typeof o.getSnapshotBeforeUpdate == "function";
    c || typeof o.UNSAFE_componentWillReceiveProps != "function" && typeof o.componentWillReceiveProps != "function" || (s !== r || u !== d) && ks(t, o, r, d), Ut = !1;
    var h = t.memoizedState;
    o.state = h, Ol(t, r, o, l), u = t.memoizedState, s !== r || h !== u || We.current || Ut ? (typeof N == "function" && (xi(t, n, N, r), u = t.memoizedState), (s = Ut || Ss(t, n, s, r, h, u, d)) ? (c || typeof o.UNSAFE_componentWillMount != "function" && typeof o.componentWillMount != "function" || (typeof o.componentWillMount == "function" && o.componentWillMount(), typeof o.UNSAFE_componentWillMount == "function" && o.UNSAFE_componentWillMount()), typeof o.componentDidMount == "function" && (t.flags |= 4194308)) : (typeof o.componentDidMount == "function" && (t.flags |= 4194308), t.memoizedProps = r, t.memoizedState = u), o.props = r, o.state = u, o.context = d, r = s) : (typeof o.componentDidMount == "function" && (t.flags |= 4194308), r = !1);
  } else {
    o = t.stateNode, uc(e, t), s = t.memoizedProps, d = t.type === t.elementType ? s : ft(t.type, s), o.props = d, c = t.pendingProps, h = o.context, u = n.contextType, typeof u == "object" && u !== null ? u = it(u) : (u = Qe(n) ? vn : Le.current, u = Wn(t, u));
    var v = n.getDerivedStateFromProps;
    (N = typeof v == "function" || typeof o.getSnapshotBeforeUpdate == "function") || typeof o.UNSAFE_componentWillReceiveProps != "function" && typeof o.componentWillReceiveProps != "function" || (s !== c || h !== u) && ks(t, o, r, u), Ut = !1, h = t.memoizedState, o.state = h, Ol(t, r, o, l);
    var y = t.memoizedState;
    s !== c || h !== y || We.current || Ut ? (typeof v == "function" && (xi(t, n, v, r), y = t.memoizedState), (d = Ut || Ss(t, n, d, r, h, y, u) || !1) ? (N || typeof o.UNSAFE_componentWillUpdate != "function" && typeof o.componentWillUpdate != "function" || (typeof o.componentWillUpdate == "function" && o.componentWillUpdate(r, y, u), typeof o.UNSAFE_componentWillUpdate == "function" && o.UNSAFE_componentWillUpdate(r, y, u)), typeof o.componentDidUpdate == "function" && (t.flags |= 4), typeof o.getSnapshotBeforeUpdate == "function" && (t.flags |= 1024)) : (typeof o.componentDidUpdate != "function" || s === e.memoizedProps && h === e.memoizedState || (t.flags |= 4), typeof o.getSnapshotBeforeUpdate != "function" || s === e.memoizedProps && h === e.memoizedState || (t.flags |= 1024), t.memoizedProps = r, t.memoizedState = y), o.props = r, o.state = y, o.context = u, r = d) : (typeof o.componentDidUpdate != "function" || s === e.memoizedProps && h === e.memoizedState || (t.flags |= 4), typeof o.getSnapshotBeforeUpdate != "function" || s === e.memoizedProps && h === e.memoizedState || (t.flags |= 1024), r = !1);
  }
  return wi(e, t, n, r, i, l);
}
function wi(e, t, n, r, l, i) {
  Dc(e, t);
  var o = (t.flags & 128) !== 0;
  if (!r && !o) return l && hs(t, n, !1), Lt(e, t, i);
  r = t.stateNode, mp.current = t;
  var s = o && typeof n.getDerivedStateFromError != "function" ? null : r.render();
  return t.flags |= 1, e !== null && o ? (t.child = Gn(t, e.child, null, i), t.child = Gn(t, null, s, i)) : Me(e, t, s, i), t.memoizedState = r.state, l && hs(t, n, !0), t.child;
}
function Mc(e) {
  var t = e.stateNode;
  t.pendingContext ? ms(e, t.pendingContext, t.pendingContext !== t.context) : t.context && ms(e, t.context, !1), co(e, t.containerInfo);
}
function Rs(e, t, n, r, l) {
  return Qn(), lo(l), t.flags |= 256, Me(e, t, n, r), t.child;
}
var Si = { dehydrated: null, treeContext: null, retryLane: 0 };
function ki(e) {
  return { baseLanes: e, cachePool: null, transitions: null };
}
function $c(e, t, n) {
  var r = t.pendingProps, l = se.current, i = !1, o = (t.flags & 128) !== 0, s;
  if ((s = o) || (s = e !== null && e.memoizedState === null ? !1 : (l & 2) !== 0), s ? (i = !0, t.flags &= -129) : (e === null || e.memoizedState !== null) && (l |= 1), ne(se, l & 1), e === null)
    return vi(t), e = t.memoizedState, e !== null && (e = e.dehydrated, e !== null) ? (t.mode & 1 ? e.data === "$!" ? t.lanes = 8 : t.lanes = 1073741824 : t.lanes = 1, null) : (o = r.children, e = r.fallback, i ? (r = t.mode, i = t.child, o = { mode: "hidden", children: o }, !(r & 1) && i !== null ? (i.childLanes = 0, i.pendingProps = o) : i = la(o, r, 0, null), e = mn(e, r, n, null), i.return = t, e.return = t, i.sibling = e, t.child = i, t.child.memoizedState = ki(n), t.memoizedState = Si, e) : yo(t, o));
  if (l = e.memoizedState, l !== null && (s = l.dehydrated, s !== null)) return hp(e, t, o, r, s, l, n);
  if (i) {
    i = r.fallback, o = t.mode, l = e.child, s = l.sibling;
    var u = { mode: "hidden", children: r.children };
    return !(o & 1) && t.child !== l ? (r = t.child, r.childLanes = 0, r.pendingProps = u, t.deletions = null) : (r = Zt(l, u), r.subtreeFlags = l.subtreeFlags & 14680064), s !== null ? i = Zt(s, i) : (i = mn(i, o, n, null), i.flags |= 2), i.return = t, r.return = t, r.sibling = i, t.child = r, r = i, i = t.child, o = e.child.memoizedState, o = o === null ? ki(n) : { baseLanes: o.baseLanes | n, cachePool: null, transitions: o.transitions }, i.memoizedState = o, i.childLanes = e.childLanes & ~n, t.memoizedState = Si, r;
  }
  return i = e.child, e = i.sibling, r = Zt(i, { mode: "visible", children: r.children }), !(t.mode & 1) && (r.lanes = n), r.return = t, r.sibling = null, e !== null && (n = t.deletions, n === null ? (t.deletions = [e], t.flags |= 16) : n.push(e)), t.child = r, t.memoizedState = null, r;
}
function yo(e, t) {
  return t = la({ mode: "visible", children: t }, e.mode, 0, null), t.return = e, e.child = t;
}
function ol(e, t, n, r) {
  return r !== null && lo(r), Gn(t, e.child, null, n), e = yo(t, t.pendingProps.children), e.flags |= 2, t.memoizedState = null, e;
}
function hp(e, t, n, r, l, i, o) {
  if (n)
    return t.flags & 256 ? (t.flags &= -257, r = Da(Error(I(422))), ol(e, t, o, r)) : t.memoizedState !== null ? (t.child = e.child, t.flags |= 128, null) : (i = r.fallback, l = t.mode, r = la({ mode: "visible", children: r.children }, l, 0, null), i = mn(i, l, o, null), i.flags |= 2, r.return = t, i.return = t, r.sibling = i, t.child = r, t.mode & 1 && Gn(t, e.child, null, o), t.child.memoizedState = ki(o), t.memoizedState = Si, i);
  if (!(t.mode & 1)) return ol(e, t, o, null);
  if (l.data === "$!") {
    if (r = l.nextSibling && l.nextSibling.dataset, r) var s = r.dgst;
    return r = s, i = Error(I(419)), r = Da(i, r, void 0), ol(e, t, o, r);
  }
  if (s = (o & e.childLanes) !== 0, He || s) {
    if (r = Ce, r !== null) {
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
      l = l & (r.suspendedLanes | o) ? 0 : l, l !== 0 && l !== i.retryLane && (i.retryLane = l, zt(e, l), gt(r, e, l, -1));
    }
    return Co(), r = Da(Error(I(421))), ol(e, t, o, r);
  }
  return l.data === "$?" ? (t.flags |= 128, t.child = e.child, t = Pp.bind(null, e), l._reactRetry = t, null) : (e = i.treeContext, Ye = Kt(l.nextSibling), Xe = t, ie = !0, mt = null, e !== null && (nt[rt++] = Pt, nt[rt++] = Ft, nt[rt++] = gn, Pt = e.id, Ft = e.overflow, gn = t), t = yo(t, r.children), t.flags |= 4096, t);
}
function Ts(e, t, n) {
  e.lanes |= t;
  var r = e.alternate;
  r !== null && (r.lanes |= t), gi(e.return, t, n);
}
function Ma(e, t, n, r, l) {
  var i = e.memoizedState;
  i === null ? e.memoizedState = { isBackwards: t, rendering: null, renderingStartTime: 0, last: r, tail: n, tailMode: l } : (i.isBackwards = t, i.rendering = null, i.renderingStartTime = 0, i.last = r, i.tail = n, i.tailMode = l);
}
function Oc(e, t, n) {
  var r = t.pendingProps, l = r.revealOrder, i = r.tail;
  if (Me(e, t, r.children, n), r = se.current, r & 2) r = r & 1 | 2, t.flags |= 128;
  else {
    if (e !== null && e.flags & 128) e: for (e = t.child; e !== null; ) {
      if (e.tag === 13) e.memoizedState !== null && Ts(e, n, t);
      else if (e.tag === 19) Ts(e, n, t);
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
  if (ne(se, r), !(t.mode & 1)) t.memoizedState = null;
  else switch (l) {
    case "forwards":
      for (n = t.child, l = null; n !== null; ) e = n.alternate, e !== null && Al(e) === null && (l = n), n = n.sibling;
      n = l, n === null ? (l = t.child, t.child = null) : (l = n.sibling, n.sibling = null), Ma(t, !1, l, n, i);
      break;
    case "backwards":
      for (n = null, l = t.child, t.child = null; l !== null; ) {
        if (e = l.alternate, e !== null && Al(e) === null) {
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
function yl(e, t) {
  !(t.mode & 1) && e !== null && (e.alternate = null, t.alternate = null, t.flags |= 2);
}
function Lt(e, t, n) {
  if (e !== null && (t.dependencies = e.dependencies), yn |= t.lanes, !(n & t.childLanes)) return null;
  if (e !== null && t.child !== e.child) throw Error(I(153));
  if (t.child !== null) {
    for (e = t.child, n = Zt(e, e.pendingProps), t.child = n, n.return = t; e.sibling !== null; ) e = e.sibling, n = n.sibling = Zt(e, e.pendingProps), n.return = t;
    n.sibling = null;
  }
  return t.child;
}
function vp(e, t, n) {
  switch (t.tag) {
    case 3:
      Mc(t), Qn();
      break;
    case 5:
      cc(t);
      break;
    case 1:
      Qe(t.type) && zl(t);
      break;
    case 4:
      co(t, t.stateNode.containerInfo);
      break;
    case 10:
      var r = t.type._context, l = t.memoizedProps.value;
      ne(Ml, r._currentValue), r._currentValue = l;
      break;
    case 13:
      if (r = t.memoizedState, r !== null)
        return r.dehydrated !== null ? (ne(se, se.current & 1), t.flags |= 128, null) : n & t.child.childLanes ? $c(e, t, n) : (ne(se, se.current & 1), e = Lt(e, t, n), e !== null ? e.sibling : null);
      ne(se, se.current & 1);
      break;
    case 19:
      if (r = (n & t.childLanes) !== 0, e.flags & 128) {
        if (r) return Oc(e, t, n);
        t.flags |= 128;
      }
      if (l = t.memoizedState, l !== null && (l.rendering = null, l.tail = null, l.lastEffect = null), ne(se, se.current), r) break;
      return null;
    case 22:
    case 23:
      return t.lanes = 0, Lc(e, t, n);
  }
  return Lt(e, t, n);
}
var Ac, Ci, Uc, Vc;
Ac = function(e, t) {
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
Ci = function() {
};
Uc = function(e, t, n, r) {
  var l = e.memoizedProps;
  if (l !== r) {
    e = t.stateNode, fn(kt.current);
    var i = null;
    switch (n) {
      case "input":
        l = Ga(e, l), r = Ga(e, r), i = [];
        break;
      case "select":
        l = ce({}, l, { value: void 0 }), r = ce({}, r, { value: void 0 }), i = [];
        break;
      case "textarea":
        l = Ya(e, l), r = Ya(e, r), i = [];
        break;
      default:
        typeof l.onClick != "function" && typeof r.onClick == "function" && (e.onclick = Rl);
    }
    Za(n, r);
    var o;
    n = null;
    for (d in l) if (!r.hasOwnProperty(d) && l.hasOwnProperty(d) && l[d] != null) if (d === "style") {
      var s = l[d];
      for (o in s) s.hasOwnProperty(o) && (n || (n = {}), n[o] = "");
    } else d !== "dangerouslySetInnerHTML" && d !== "children" && d !== "suppressContentEditableWarning" && d !== "suppressHydrationWarning" && d !== "autoFocus" && (Sr.hasOwnProperty(d) ? i || (i = []) : (i = i || []).push(d, null));
    for (d in r) {
      var u = r[d];
      if (s = l != null ? l[d] : void 0, r.hasOwnProperty(d) && u !== s && (u != null || s != null)) if (d === "style") if (s) {
        for (o in s) !s.hasOwnProperty(o) || u && u.hasOwnProperty(o) || (n || (n = {}), n[o] = "");
        for (o in u) u.hasOwnProperty(o) && s[o] !== u[o] && (n || (n = {}), n[o] = u[o]);
      } else n || (i || (i = []), i.push(
        d,
        n
      )), n = u;
      else d === "dangerouslySetInnerHTML" ? (u = u ? u.__html : void 0, s = s ? s.__html : void 0, u != null && s !== u && (i = i || []).push(d, u)) : d === "children" ? typeof u != "string" && typeof u != "number" || (i = i || []).push(d, "" + u) : d !== "suppressContentEditableWarning" && d !== "suppressHydrationWarning" && (Sr.hasOwnProperty(d) ? (u != null && d === "onScroll" && re("scroll", e), i || s === u || (i = [])) : (i = i || []).push(d, u));
    }
    n && (i = i || []).push("style", n);
    var d = i;
    (t.updateQueue = d) && (t.flags |= 4);
  }
};
Vc = function(e, t, n, r) {
  n !== r && (t.flags |= 4);
};
function or(e, t) {
  if (!ie) switch (e.tailMode) {
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
  if (t) for (var l = e.child; l !== null; ) n |= l.lanes | l.childLanes, r |= l.subtreeFlags & 14680064, r |= l.flags & 14680064, l.return = e, l = l.sibling;
  else for (l = e.child; l !== null; ) n |= l.lanes | l.childLanes, r |= l.subtreeFlags, r |= l.flags, l.return = e, l = l.sibling;
  return e.subtreeFlags |= r, e.childLanes = n, t;
}
function gp(e, t, n) {
  var r = t.pendingProps;
  switch (ro(t), t.tag) {
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
      return Qe(t.type) && Tl(), Re(t), null;
    case 3:
      return r = t.stateNode, Kn(), le(We), le(Le), po(), r.pendingContext && (r.context = r.pendingContext, r.pendingContext = null), (e === null || e.child === null) && (al(t) ? t.flags |= 4 : e === null || e.memoizedState.isDehydrated && !(t.flags & 256) || (t.flags |= 1024, mt !== null && (zi(mt), mt = null))), Ci(e, t), Re(t), null;
    case 5:
      fo(t);
      var l = fn(Dr.current);
      if (n = t.type, e !== null && t.stateNode != null) Uc(e, t, n, r, l), e.ref !== t.ref && (t.flags |= 512, t.flags |= 2097152);
      else {
        if (!r) {
          if (t.stateNode === null) throw Error(I(166));
          return Re(t), null;
        }
        if (e = fn(kt.current), al(t)) {
          r = t.stateNode, n = t.type;
          var i = t.memoizedProps;
          switch (r[wt] = t, r[zr] = i, e = (t.mode & 1) !== 0, n) {
            case "dialog":
              re("cancel", r), re("close", r);
              break;
            case "iframe":
            case "object":
            case "embed":
              re("load", r);
              break;
            case "video":
            case "audio":
              for (l = 0; l < fr.length; l++) re(fr[l], r);
              break;
            case "source":
              re("error", r);
              break;
            case "img":
            case "image":
            case "link":
              re(
                "error",
                r
              ), re("load", r);
              break;
            case "details":
              re("toggle", r);
              break;
            case "input":
              Vo(r, i), re("invalid", r);
              break;
            case "select":
              r._wrapperState = { wasMultiple: !!i.multiple }, re("invalid", r);
              break;
            case "textarea":
              bo(r, i), re("invalid", r);
          }
          Za(n, i), l = null;
          for (var o in i) if (i.hasOwnProperty(o)) {
            var s = i[o];
            o === "children" ? typeof s == "string" ? r.textContent !== s && (i.suppressHydrationWarning !== !0 && ll(r.textContent, s, e), l = ["children", s]) : typeof s == "number" && r.textContent !== "" + s && (i.suppressHydrationWarning !== !0 && ll(
              r.textContent,
              s,
              e
            ), l = ["children", "" + s]) : Sr.hasOwnProperty(o) && s != null && o === "onScroll" && re("scroll", r);
          }
          switch (n) {
            case "input":
              Yr(r), Bo(r, i, !0);
              break;
            case "textarea":
              Yr(r), Ho(r);
              break;
            case "select":
            case "option":
              break;
            default:
              typeof i.onClick == "function" && (r.onclick = Rl);
          }
          r = l, t.updateQueue = r, r !== null && (t.flags |= 4);
        } else {
          o = l.nodeType === 9 ? l : l.ownerDocument, e === "http://www.w3.org/1999/xhtml" && (e = hu(n)), e === "http://www.w3.org/1999/xhtml" ? n === "script" ? (e = o.createElement("div"), e.innerHTML = "<script><\/script>", e = e.removeChild(e.firstChild)) : typeof r.is == "string" ? e = o.createElement(n, { is: r.is }) : (e = o.createElement(n), n === "select" && (o = e, r.multiple ? o.multiple = !0 : r.size && (o.size = r.size))) : e = o.createElementNS(e, n), e[wt] = t, e[zr] = r, Ac(e, t, !1, !1), t.stateNode = e;
          e: {
            switch (o = Ja(n, r), n) {
              case "dialog":
                re("cancel", e), re("close", e), l = r;
                break;
              case "iframe":
              case "object":
              case "embed":
                re("load", e), l = r;
                break;
              case "video":
              case "audio":
                for (l = 0; l < fr.length; l++) re(fr[l], e);
                l = r;
                break;
              case "source":
                re("error", e), l = r;
                break;
              case "img":
              case "image":
              case "link":
                re(
                  "error",
                  e
                ), re("load", e), l = r;
                break;
              case "details":
                re("toggle", e), l = r;
                break;
              case "input":
                Vo(e, r), l = Ga(e, r), re("invalid", e);
                break;
              case "option":
                l = r;
                break;
              case "select":
                e._wrapperState = { wasMultiple: !!r.multiple }, l = ce({}, r, { value: void 0 }), re("invalid", e);
                break;
              case "textarea":
                bo(e, r), l = Ya(e, r), re("invalid", e);
                break;
              default:
                l = r;
            }
            Za(n, l), s = l;
            for (i in s) if (s.hasOwnProperty(i)) {
              var u = s[i];
              i === "style" ? xu(e, u) : i === "dangerouslySetInnerHTML" ? (u = u ? u.__html : void 0, u != null && vu(e, u)) : i === "children" ? typeof u == "string" ? (n !== "textarea" || u !== "") && kr(e, u) : typeof u == "number" && kr(e, "" + u) : i !== "suppressContentEditableWarning" && i !== "suppressHydrationWarning" && i !== "autoFocus" && (Sr.hasOwnProperty(i) ? u != null && i === "onScroll" && re("scroll", e) : u != null && Bi(e, i, u, o));
            }
            switch (n) {
              case "input":
                Yr(e), Bo(e, r, !1);
                break;
              case "textarea":
                Yr(e), Ho(e);
                break;
              case "option":
                r.value != null && e.setAttribute("value", "" + Jt(r.value));
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
                typeof l.onClick == "function" && (e.onclick = Rl);
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
      if (e && t.stateNode != null) Vc(e, t, e.memoizedProps, r);
      else {
        if (typeof r != "string" && t.stateNode === null) throw Error(I(166));
        if (n = fn(Dr.current), fn(kt.current), al(t)) {
          if (r = t.stateNode, n = t.memoizedProps, r[wt] = t, (i = r.nodeValue !== n) && (e = Xe, e !== null)) switch (e.tag) {
            case 3:
              ll(r.nodeValue, n, (e.mode & 1) !== 0);
              break;
            case 5:
              e.memoizedProps.suppressHydrationWarning !== !0 && ll(r.nodeValue, n, (e.mode & 1) !== 0);
          }
          i && (t.flags |= 4);
        } else r = (n.nodeType === 9 ? n : n.ownerDocument).createTextNode(r), r[wt] = t, t.stateNode = r;
      }
      return Re(t), null;
    case 13:
      if (le(se), r = t.memoizedState, e === null || e.memoizedState !== null && e.memoizedState.dehydrated !== null) {
        if (ie && Ye !== null && t.mode & 1 && !(t.flags & 128)) ac(), Qn(), t.flags |= 98560, i = !1;
        else if (i = al(t), r !== null && r.dehydrated !== null) {
          if (e === null) {
            if (!i) throw Error(I(318));
            if (i = t.memoizedState, i = i !== null ? i.dehydrated : null, !i) throw Error(I(317));
            i[wt] = t;
          } else Qn(), !(t.flags & 128) && (t.memoizedState = null), t.flags |= 4;
          Re(t), i = !1;
        } else mt !== null && (zi(mt), mt = null), i = !0;
        if (!i) return t.flags & 65536 ? t : null;
      }
      return t.flags & 128 ? (t.lanes = n, t) : (r = r !== null, r !== (e !== null && e.memoizedState !== null) && r && (t.child.flags |= 8192, t.mode & 1 && (e === null || se.current & 1 ? je === 0 && (je = 3) : Co())), t.updateQueue !== null && (t.flags |= 4), Re(t), null);
    case 4:
      return Kn(), Ci(e, t), e === null && Rr(t.stateNode.containerInfo), Re(t), null;
    case 10:
      return oo(t.type._context), Re(t), null;
    case 17:
      return Qe(t.type) && Tl(), Re(t), null;
    case 19:
      if (le(se), i = t.memoizedState, i === null) return Re(t), null;
      if (r = (t.flags & 128) !== 0, o = i.rendering, o === null) if (r) or(i, !1);
      else {
        if (je !== 0 || e !== null && e.flags & 128) for (e = t.child; e !== null; ) {
          if (o = Al(e), o !== null) {
            for (t.flags |= 128, or(i, !1), r = o.updateQueue, r !== null && (t.updateQueue = r, t.flags |= 4), t.subtreeFlags = 0, r = n, n = t.child; n !== null; ) i = n, e = r, i.flags &= 14680066, o = i.alternate, o === null ? (i.childLanes = 0, i.lanes = e, i.child = null, i.subtreeFlags = 0, i.memoizedProps = null, i.memoizedState = null, i.updateQueue = null, i.dependencies = null, i.stateNode = null) : (i.childLanes = o.childLanes, i.lanes = o.lanes, i.child = o.child, i.subtreeFlags = 0, i.deletions = null, i.memoizedProps = o.memoizedProps, i.memoizedState = o.memoizedState, i.updateQueue = o.updateQueue, i.type = o.type, e = o.dependencies, i.dependencies = e === null ? null : { lanes: e.lanes, firstContext: e.firstContext }), n = n.sibling;
            return ne(se, se.current & 1 | 2), t.child;
          }
          e = e.sibling;
        }
        i.tail !== null && ve() > Yn && (t.flags |= 128, r = !0, or(i, !1), t.lanes = 4194304);
      }
      else {
        if (!r) if (e = Al(o), e !== null) {
          if (t.flags |= 128, r = !0, n = e.updateQueue, n !== null && (t.updateQueue = n, t.flags |= 4), or(i, !0), i.tail === null && i.tailMode === "hidden" && !o.alternate && !ie) return Re(t), null;
        } else 2 * ve() - i.renderingStartTime > Yn && n !== 1073741824 && (t.flags |= 128, r = !0, or(i, !1), t.lanes = 4194304);
        i.isBackwards ? (o.sibling = t.child, t.child = o) : (n = i.last, n !== null ? n.sibling = o : t.child = o, i.last = o);
      }
      return i.tail !== null ? (t = i.tail, i.rendering = t, i.tail = t.sibling, i.renderingStartTime = ve(), t.sibling = null, n = se.current, ne(se, r ? n & 1 | 2 : n & 1), t) : (Re(t), null);
    case 22:
    case 23:
      return ko(), r = t.memoizedState !== null, e !== null && e.memoizedState !== null !== r && (t.flags |= 8192), r && t.mode & 1 ? qe & 1073741824 && (Re(t), t.subtreeFlags & 6 && (t.flags |= 8192)) : Re(t), null;
    case 24:
      return null;
    case 25:
      return null;
  }
  throw Error(I(156, t.tag));
}
function xp(e, t) {
  switch (ro(t), t.tag) {
    case 1:
      return Qe(t.type) && Tl(), e = t.flags, e & 65536 ? (t.flags = e & -65537 | 128, t) : null;
    case 3:
      return Kn(), le(We), le(Le), po(), e = t.flags, e & 65536 && !(e & 128) ? (t.flags = e & -65537 | 128, t) : null;
    case 5:
      return fo(t), null;
    case 13:
      if (le(se), e = t.memoizedState, e !== null && e.dehydrated !== null) {
        if (t.alternate === null) throw Error(I(340));
        Qn();
      }
      return e = t.flags, e & 65536 ? (t.flags = e & -65537 | 128, t) : null;
    case 19:
      return le(se), null;
    case 4:
      return Kn(), null;
    case 10:
      return oo(t.type._context), null;
    case 22:
    case 23:
      return ko(), null;
    case 24:
      return null;
    default:
      return null;
  }
}
var sl = !1, Te = !1, yp = typeof WeakSet == "function" ? WeakSet : Set, $ = null;
function Mn(e, t) {
  var n = e.ref;
  if (n !== null) if (typeof n == "function") try {
    n(null);
  } catch (r) {
    he(e, t, r);
  }
  else n.current = null;
}
function Ei(e, t, n) {
  try {
    n();
  } catch (r) {
    he(e, t, r);
  }
}
var zs = !1;
function jp(e, t) {
  if (ui = Pl, e = Qu(), to(e)) {
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
        var o = 0, s = -1, u = -1, d = 0, N = 0, c = e, h = null;
        t: for (; ; ) {
          for (var v; c !== n || l !== 0 && c.nodeType !== 3 || (s = o + l), c !== i || r !== 0 && c.nodeType !== 3 || (u = o + r), c.nodeType === 3 && (o += c.nodeValue.length), (v = c.firstChild) !== null; )
            h = c, c = v;
          for (; ; ) {
            if (c === e) break t;
            if (h === n && ++d === l && (s = o), h === i && ++N === r && (u = o), (v = c.nextSibling) !== null) break;
            c = h, h = c.parentNode;
          }
          c = v;
        }
        n = s === -1 || u === -1 ? null : { start: s, end: u };
      } else n = null;
    }
    n = n || { start: 0, end: 0 };
  } else n = null;
  for (ci = { focusedElem: e, selectionRange: n }, Pl = !1, $ = t; $ !== null; ) if (t = $, e = t.child, (t.subtreeFlags & 1028) !== 0 && e !== null) e.return = t, $ = e;
  else for (; $ !== null; ) {
    t = $;
    try {
      var y = t.alternate;
      if (t.flags & 1024) switch (t.tag) {
        case 0:
        case 11:
        case 15:
          break;
        case 1:
          if (y !== null) {
            var w = y.memoizedProps, _ = y.memoizedState, f = t.stateNode, p = f.getSnapshotBeforeUpdate(t.elementType === t.type ? w : ft(t.type, w), _);
            f.__reactInternalSnapshotBeforeUpdate = p;
          }
          break;
        case 3:
          var g = t.stateNode.containerInfo;
          g.nodeType === 1 ? g.textContent = "" : g.nodeType === 9 && g.documentElement && g.removeChild(g.documentElement);
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
      he(t, t.return, k);
    }
    if (e = t.sibling, e !== null) {
      e.return = t.return, $ = e;
      break;
    }
    $ = t.return;
  }
  return y = zs, zs = !1, y;
}
function yr(e, t, n) {
  var r = t.updateQueue;
  if (r = r !== null ? r.lastEffect : null, r !== null) {
    var l = r = r.next;
    do {
      if ((l.tag & e) === e) {
        var i = l.destroy;
        l.destroy = void 0, i !== void 0 && Ei(t, n, i);
      }
      l = l.next;
    } while (l !== r);
  }
}
function na(e, t) {
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
function Ii(e) {
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
function Bc(e) {
  var t = e.alternate;
  t !== null && (e.alternate = null, Bc(t)), e.child = null, e.deletions = null, e.sibling = null, e.tag === 5 && (t = e.stateNode, t !== null && (delete t[wt], delete t[zr], delete t[pi], delete t[np], delete t[rp])), e.stateNode = null, e.return = null, e.dependencies = null, e.memoizedProps = null, e.memoizedState = null, e.pendingProps = null, e.stateNode = null, e.updateQueue = null;
}
function bc(e) {
  return e.tag === 5 || e.tag === 3 || e.tag === 4;
}
function Ls(e) {
  e: for (; ; ) {
    for (; e.sibling === null; ) {
      if (e.return === null || bc(e.return)) return null;
      e = e.return;
    }
    for (e.sibling.return = e.return, e = e.sibling; e.tag !== 5 && e.tag !== 6 && e.tag !== 18; ) {
      if (e.flags & 2 || e.child === null || e.tag === 4) continue e;
      e.child.return = e, e = e.child;
    }
    if (!(e.flags & 2)) return e.stateNode;
  }
}
function Pi(e, t, n) {
  var r = e.tag;
  if (r === 5 || r === 6) e = e.stateNode, t ? n.nodeType === 8 ? n.parentNode.insertBefore(e, t) : n.insertBefore(e, t) : (n.nodeType === 8 ? (t = n.parentNode, t.insertBefore(e, n)) : (t = n, t.appendChild(e)), n = n._reactRootContainer, n != null || t.onclick !== null || (t.onclick = Rl));
  else if (r !== 4 && (e = e.child, e !== null)) for (Pi(e, t, n), e = e.sibling; e !== null; ) Pi(e, t, n), e = e.sibling;
}
function Fi(e, t, n) {
  var r = e.tag;
  if (r === 5 || r === 6) e = e.stateNode, t ? n.insertBefore(e, t) : n.appendChild(e);
  else if (r !== 4 && (e = e.child, e !== null)) for (Fi(e, t, n), e = e.sibling; e !== null; ) Fi(e, t, n), e = e.sibling;
}
var Ee = null, pt = !1;
function Ot(e, t, n) {
  for (n = n.child; n !== null; ) Hc(e, t, n), n = n.sibling;
}
function Hc(e, t, n) {
  if (St && typeof St.onCommitFiberUnmount == "function") try {
    St.onCommitFiberUnmount(Kl, n);
  } catch {
  }
  switch (n.tag) {
    case 5:
      Te || Mn(n, t);
    case 6:
      var r = Ee, l = pt;
      Ee = null, Ot(e, t, n), Ee = r, pt = l, Ee !== null && (pt ? (e = Ee, n = n.stateNode, e.nodeType === 8 ? e.parentNode.removeChild(n) : e.removeChild(n)) : Ee.removeChild(n.stateNode));
      break;
    case 18:
      Ee !== null && (pt ? (e = Ee, n = n.stateNode, e.nodeType === 8 ? Fa(e.parentNode, n) : e.nodeType === 1 && Fa(e, n), Pr(e)) : Fa(Ee, n.stateNode));
      break;
    case 4:
      r = Ee, l = pt, Ee = n.stateNode.containerInfo, pt = !0, Ot(e, t, n), Ee = r, pt = l;
      break;
    case 0:
    case 11:
    case 14:
    case 15:
      if (!Te && (r = n.updateQueue, r !== null && (r = r.lastEffect, r !== null))) {
        l = r = r.next;
        do {
          var i = l, o = i.destroy;
          i = i.tag, o !== void 0 && (i & 2 || i & 4) && Ei(n, t, o), l = l.next;
        } while (l !== r);
      }
      Ot(e, t, n);
      break;
    case 1:
      if (!Te && (Mn(n, t), r = n.stateNode, typeof r.componentWillUnmount == "function")) try {
        r.props = n.memoizedProps, r.state = n.memoizedState, r.componentWillUnmount();
      } catch (s) {
        he(n, t, s);
      }
      Ot(e, t, n);
      break;
    case 21:
      Ot(e, t, n);
      break;
    case 22:
      n.mode & 1 ? (Te = (r = Te) || n.memoizedState !== null, Ot(e, t, n), Te = r) : Ot(e, t, n);
      break;
    default:
      Ot(e, t, n);
  }
}
function Ds(e) {
  var t = e.updateQueue;
  if (t !== null) {
    e.updateQueue = null;
    var n = e.stateNode;
    n === null && (n = e.stateNode = new yp()), t.forEach(function(r) {
      var l = Fp.bind(null, e, r);
      n.has(r) || (n.add(r), r.then(l, l));
    });
  }
}
function dt(e, t) {
  var n = t.deletions;
  if (n !== null) for (var r = 0; r < n.length; r++) {
    var l = n[r];
    try {
      var i = e, o = t, s = o;
      e: for (; s !== null; ) {
        switch (s.tag) {
          case 5:
            Ee = s.stateNode, pt = !1;
            break e;
          case 3:
            Ee = s.stateNode.containerInfo, pt = !0;
            break e;
          case 4:
            Ee = s.stateNode.containerInfo, pt = !0;
            break e;
        }
        s = s.return;
      }
      if (Ee === null) throw Error(I(160));
      Hc(i, o, l), Ee = null, pt = !1;
      var u = l.alternate;
      u !== null && (u.return = null), l.return = null;
    } catch (d) {
      he(l, t, d);
    }
  }
  if (t.subtreeFlags & 12854) for (t = t.child; t !== null; ) Wc(t, e), t = t.sibling;
}
function Wc(e, t) {
  var n = e.alternate, r = e.flags;
  switch (e.tag) {
    case 0:
    case 11:
    case 14:
    case 15:
      if (dt(t, e), jt(e), r & 4) {
        try {
          yr(3, e, e.return), na(3, e);
        } catch (w) {
          he(e, e.return, w);
        }
        try {
          yr(5, e, e.return);
        } catch (w) {
          he(e, e.return, w);
        }
      }
      break;
    case 1:
      dt(t, e), jt(e), r & 512 && n !== null && Mn(n, n.return);
      break;
    case 5:
      if (dt(t, e), jt(e), r & 512 && n !== null && Mn(n, n.return), e.flags & 32) {
        var l = e.stateNode;
        try {
          kr(l, "");
        } catch (w) {
          he(e, e.return, w);
        }
      }
      if (r & 4 && (l = e.stateNode, l != null)) {
        var i = e.memoizedProps, o = n !== null ? n.memoizedProps : i, s = e.type, u = e.updateQueue;
        if (e.updateQueue = null, u !== null) try {
          s === "input" && i.type === "radio" && i.name != null && pu(l, i), Ja(s, o);
          var d = Ja(s, i);
          for (o = 0; o < u.length; o += 2) {
            var N = u[o], c = u[o + 1];
            N === "style" ? xu(l, c) : N === "dangerouslySetInnerHTML" ? vu(l, c) : N === "children" ? kr(l, c) : Bi(l, N, c, d);
          }
          switch (s) {
            case "input":
              Ka(l, i);
              break;
            case "textarea":
              mu(l, i);
              break;
            case "select":
              var h = l._wrapperState.wasMultiple;
              l._wrapperState.wasMultiple = !!i.multiple;
              var v = i.value;
              v != null ? On(l, !!i.multiple, v, !1) : h !== !!i.multiple && (i.defaultValue != null ? On(
                l,
                !!i.multiple,
                i.defaultValue,
                !0
              ) : On(l, !!i.multiple, i.multiple ? [] : "", !1));
          }
          l[zr] = i;
        } catch (w) {
          he(e, e.return, w);
        }
      }
      break;
    case 6:
      if (dt(t, e), jt(e), r & 4) {
        if (e.stateNode === null) throw Error(I(162));
        l = e.stateNode, i = e.memoizedProps;
        try {
          l.nodeValue = i;
        } catch (w) {
          he(e, e.return, w);
        }
      }
      break;
    case 3:
      if (dt(t, e), jt(e), r & 4 && n !== null && n.memoizedState.isDehydrated) try {
        Pr(t.containerInfo);
      } catch (w) {
        he(e, e.return, w);
      }
      break;
    case 4:
      dt(t, e), jt(e);
      break;
    case 13:
      dt(t, e), jt(e), l = e.child, l.flags & 8192 && (i = l.memoizedState !== null, l.stateNode.isHidden = i, !i || l.alternate !== null && l.alternate.memoizedState !== null || (wo = ve())), r & 4 && Ds(e);
      break;
    case 22:
      if (N = n !== null && n.memoizedState !== null, e.mode & 1 ? (Te = (d = Te) || N, dt(t, e), Te = d) : dt(t, e), jt(e), r & 8192) {
        if (d = e.memoizedState !== null, (e.stateNode.isHidden = d) && !N && e.mode & 1) for ($ = e, N = e.child; N !== null; ) {
          for (c = $ = N; $ !== null; ) {
            switch (h = $, v = h.child, h.tag) {
              case 0:
              case 11:
              case 14:
              case 15:
                yr(4, h, h.return);
                break;
              case 1:
                Mn(h, h.return);
                var y = h.stateNode;
                if (typeof y.componentWillUnmount == "function") {
                  r = h, n = h.return;
                  try {
                    t = r, y.props = t.memoizedProps, y.state = t.memoizedState, y.componentWillUnmount();
                  } catch (w) {
                    he(r, n, w);
                  }
                }
                break;
              case 5:
                Mn(h, h.return);
                break;
              case 22:
                if (h.memoizedState !== null) {
                  $s(c);
                  continue;
                }
            }
            v !== null ? (v.return = h, $ = v) : $s(c);
          }
          N = N.sibling;
        }
        e: for (N = null, c = e; ; ) {
          if (c.tag === 5) {
            if (N === null) {
              N = c;
              try {
                l = c.stateNode, d ? (i = l.style, typeof i.setProperty == "function" ? i.setProperty("display", "none", "important") : i.display = "none") : (s = c.stateNode, u = c.memoizedProps.style, o = u != null && u.hasOwnProperty("display") ? u.display : null, s.style.display = gu("display", o));
              } catch (w) {
                he(e, e.return, w);
              }
            }
          } else if (c.tag === 6) {
            if (N === null) try {
              c.stateNode.nodeValue = d ? "" : c.memoizedProps;
            } catch (w) {
              he(e, e.return, w);
            }
          } else if ((c.tag !== 22 && c.tag !== 23 || c.memoizedState === null || c === e) && c.child !== null) {
            c.child.return = c, c = c.child;
            continue;
          }
          if (c === e) break e;
          for (; c.sibling === null; ) {
            if (c.return === null || c.return === e) break e;
            N === c && (N = null), c = c.return;
          }
          N === c && (N = null), c.sibling.return = c.return, c = c.sibling;
        }
      }
      break;
    case 19:
      dt(t, e), jt(e), r & 4 && Ds(e);
      break;
    case 21:
      break;
    default:
      dt(
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
          if (bc(n)) {
            var r = n;
            break e;
          }
          n = n.return;
        }
        throw Error(I(160));
      }
      switch (r.tag) {
        case 5:
          var l = r.stateNode;
          r.flags & 32 && (kr(l, ""), r.flags &= -33);
          var i = Ls(e);
          Fi(e, i, l);
          break;
        case 3:
        case 4:
          var o = r.stateNode.containerInfo, s = Ls(e);
          Pi(e, s, o);
          break;
        default:
          throw Error(I(161));
      }
    } catch (u) {
      he(e, e.return, u);
    }
    e.flags &= -3;
  }
  t & 4096 && (e.flags &= -4097);
}
function Np(e, t, n) {
  $ = e, Qc(e);
}
function Qc(e, t, n) {
  for (var r = (e.mode & 1) !== 0; $ !== null; ) {
    var l = $, i = l.child;
    if (l.tag === 22 && r) {
      var o = l.memoizedState !== null || sl;
      if (!o) {
        var s = l.alternate, u = s !== null && s.memoizedState !== null || Te;
        s = sl;
        var d = Te;
        if (sl = o, (Te = u) && !d) for ($ = l; $ !== null; ) o = $, u = o.child, o.tag === 22 && o.memoizedState !== null ? Os(l) : u !== null ? (u.return = o, $ = u) : Os(l);
        for (; i !== null; ) $ = i, Qc(i), i = i.sibling;
        $ = l, sl = s, Te = d;
      }
      Ms(e);
    } else l.subtreeFlags & 8772 && i !== null ? (i.return = l, $ = i) : Ms(e);
  }
}
function Ms(e) {
  for (; $ !== null; ) {
    var t = $;
    if (t.flags & 8772) {
      var n = t.alternate;
      try {
        if (t.flags & 8772) switch (t.tag) {
          case 0:
          case 11:
          case 15:
            Te || na(5, t);
            break;
          case 1:
            var r = t.stateNode;
            if (t.flags & 4 && !Te) if (n === null) r.componentDidMount();
            else {
              var l = t.elementType === t.type ? n.memoizedProps : ft(t.type, n.memoizedProps);
              r.componentDidUpdate(l, n.memoizedState, r.__reactInternalSnapshotBeforeUpdate);
            }
            var i = t.updateQueue;
            i !== null && js(t, i, r);
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
              js(t, o, n);
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
                var N = d.memoizedState;
                if (N !== null) {
                  var c = N.dehydrated;
                  c !== null && Pr(c);
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
        Te || t.flags & 512 && Ii(t);
      } catch (h) {
        he(t, t.return, h);
      }
    }
    if (t === e) {
      $ = null;
      break;
    }
    if (n = t.sibling, n !== null) {
      n.return = t.return, $ = n;
      break;
    }
    $ = t.return;
  }
}
function $s(e) {
  for (; $ !== null; ) {
    var t = $;
    if (t === e) {
      $ = null;
      break;
    }
    var n = t.sibling;
    if (n !== null) {
      n.return = t.return, $ = n;
      break;
    }
    $ = t.return;
  }
}
function Os(e) {
  for (; $ !== null; ) {
    var t = $;
    try {
      switch (t.tag) {
        case 0:
        case 11:
        case 15:
          var n = t.return;
          try {
            na(4, t);
          } catch (u) {
            he(t, n, u);
          }
          break;
        case 1:
          var r = t.stateNode;
          if (typeof r.componentDidMount == "function") {
            var l = t.return;
            try {
              r.componentDidMount();
            } catch (u) {
              he(t, l, u);
            }
          }
          var i = t.return;
          try {
            Ii(t);
          } catch (u) {
            he(t, i, u);
          }
          break;
        case 5:
          var o = t.return;
          try {
            Ii(t);
          } catch (u) {
            he(t, o, u);
          }
      }
    } catch (u) {
      he(t, t.return, u);
    }
    if (t === e) {
      $ = null;
      break;
    }
    var s = t.sibling;
    if (s !== null) {
      s.return = t.return, $ = s;
      break;
    }
    $ = t.return;
  }
}
var wp = Math.ceil, Bl = Dt.ReactCurrentDispatcher, jo = Dt.ReactCurrentOwner, at = Dt.ReactCurrentBatchConfig, q = 0, Ce = null, xe = null, Ie = 0, qe = 0, $n = nn(0), je = 0, Ar = null, yn = 0, ra = 0, No = 0, jr = null, Be = null, wo = 0, Yn = 1 / 0, Et = null, bl = !1, _i = null, Yt = null, ul = !1, Ht = null, Hl = 0, Nr = 0, Ri = null, jl = -1, Nl = 0;
function Oe() {
  return q & 6 ? ve() : jl !== -1 ? jl : jl = ve();
}
function Xt(e) {
  return e.mode & 1 ? q & 2 && Ie !== 0 ? Ie & -Ie : ap.transition !== null ? (Nl === 0 && (Nl = _u()), Nl) : (e = Y, e !== 0 || (e = window.event, e = e === void 0 ? 16 : $u(e.type)), e) : 1;
}
function gt(e, t, n, r) {
  if (50 < Nr) throw Nr = 0, Ri = null, Error(I(185));
  Br(e, n, r), (!(q & 2) || e !== Ce) && (e === Ce && (!(q & 2) && (ra |= n), je === 4 && Bt(e, Ie)), Ge(e, r), n === 1 && q === 0 && !(t.mode & 1) && (Yn = ve() + 500, Jl && rn()));
}
function Ge(e, t) {
  var n = e.callbackNode;
  lf(e, t);
  var r = Il(e, e === Ce ? Ie : 0);
  if (r === 0) n !== null && Go(n), e.callbackNode = null, e.callbackPriority = 0;
  else if (t = r & -r, e.callbackPriority !== t) {
    if (n != null && Go(n), t === 1) e.tag === 0 ? lp(As.bind(null, e)) : nc(As.bind(null, e)), ep(function() {
      !(q & 6) && rn();
    }), n = null;
    else {
      switch (Ru(r)) {
        case 1:
          n = Gi;
          break;
        case 4:
          n = Pu;
          break;
        case 16:
          n = El;
          break;
        case 536870912:
          n = Fu;
          break;
        default:
          n = El;
      }
      n = ed(n, Gc.bind(null, e));
    }
    e.callbackPriority = t, e.callbackNode = n;
  }
}
function Gc(e, t) {
  if (jl = -1, Nl = 0, q & 6) throw Error(I(327));
  var n = e.callbackNode;
  if (bn() && e.callbackNode !== n) return null;
  var r = Il(e, e === Ce ? Ie : 0);
  if (r === 0) return null;
  if (r & 30 || r & e.expiredLanes || t) t = Wl(e, r);
  else {
    t = r;
    var l = q;
    q |= 2;
    var i = qc();
    (Ce !== e || Ie !== t) && (Et = null, Yn = ve() + 500, pn(e, t));
    do
      try {
        Cp();
        break;
      } catch (s) {
        Kc(e, s);
      }
    while (!0);
    io(), Bl.current = i, q = l, xe !== null ? t = 0 : (Ce = null, Ie = 0, t = je);
  }
  if (t !== 0) {
    if (t === 2 && (l = li(e), l !== 0 && (r = l, t = Ti(e, l))), t === 1) throw n = Ar, pn(e, 0), Bt(e, r), Ge(e, ve()), n;
    if (t === 6) Bt(e, r);
    else {
      if (l = e.current.alternate, !(r & 30) && !Sp(l) && (t = Wl(e, r), t === 2 && (i = li(e), i !== 0 && (r = i, t = Ti(e, i))), t === 1)) throw n = Ar, pn(e, 0), Bt(e, r), Ge(e, ve()), n;
      switch (e.finishedWork = l, e.finishedLanes = r, t) {
        case 0:
        case 1:
          throw Error(I(345));
        case 2:
          un(e, Be, Et);
          break;
        case 3:
          if (Bt(e, r), (r & 130023424) === r && (t = wo + 500 - ve(), 10 < t)) {
            if (Il(e, 0) !== 0) break;
            if (l = e.suspendedLanes, (l & r) !== r) {
              Oe(), e.pingedLanes |= e.suspendedLanes & l;
              break;
            }
            e.timeoutHandle = fi(un.bind(null, e, Be, Et), t);
            break;
          }
          un(e, Be, Et);
          break;
        case 4:
          if (Bt(e, r), (r & 4194240) === r) break;
          for (t = e.eventTimes, l = -1; 0 < r; ) {
            var o = 31 - vt(r);
            i = 1 << o, o = t[o], o > l && (l = o), r &= ~i;
          }
          if (r = l, r = ve() - r, r = (120 > r ? 120 : 480 > r ? 480 : 1080 > r ? 1080 : 1920 > r ? 1920 : 3e3 > r ? 3e3 : 4320 > r ? 4320 : 1960 * wp(r / 1960)) - r, 10 < r) {
            e.timeoutHandle = fi(un.bind(null, e, Be, Et), r);
            break;
          }
          un(e, Be, Et);
          break;
        case 5:
          un(e, Be, Et);
          break;
        default:
          throw Error(I(329));
      }
    }
  }
  return Ge(e, ve()), e.callbackNode === n ? Gc.bind(null, e) : null;
}
function Ti(e, t) {
  var n = jr;
  return e.current.memoizedState.isDehydrated && (pn(e, t).flags |= 256), e = Wl(e, t), e !== 2 && (t = Be, Be = n, t !== null && zi(t)), e;
}
function zi(e) {
  Be === null ? Be = e : Be.push.apply(Be, e);
}
function Sp(e) {
  for (var t = e; ; ) {
    if (t.flags & 16384) {
      var n = t.updateQueue;
      if (n !== null && (n = n.stores, n !== null)) for (var r = 0; r < n.length; r++) {
        var l = n[r], i = l.getSnapshot;
        l = l.value;
        try {
          if (!yt(i(), l)) return !1;
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
  for (t &= ~No, t &= ~ra, e.suspendedLanes |= t, e.pingedLanes &= ~t, e = e.expirationTimes; 0 < t; ) {
    var n = 31 - vt(t), r = 1 << n;
    e[n] = -1, t &= ~r;
  }
}
function As(e) {
  if (q & 6) throw Error(I(327));
  bn();
  var t = Il(e, 0);
  if (!(t & 1)) return Ge(e, ve()), null;
  var n = Wl(e, t);
  if (e.tag !== 0 && n === 2) {
    var r = li(e);
    r !== 0 && (t = r, n = Ti(e, r));
  }
  if (n === 1) throw n = Ar, pn(e, 0), Bt(e, t), Ge(e, ve()), n;
  if (n === 6) throw Error(I(345));
  return e.finishedWork = e.current.alternate, e.finishedLanes = t, un(e, Be, Et), Ge(e, ve()), null;
}
function So(e, t) {
  var n = q;
  q |= 1;
  try {
    return e(t);
  } finally {
    q = n, q === 0 && (Yn = ve() + 500, Jl && rn());
  }
}
function jn(e) {
  Ht !== null && Ht.tag === 0 && !(q & 6) && bn();
  var t = q;
  q |= 1;
  var n = at.transition, r = Y;
  try {
    if (at.transition = null, Y = 1, e) return e();
  } finally {
    Y = r, at.transition = n, q = t, !(q & 6) && rn();
  }
}
function ko() {
  qe = $n.current, le($n);
}
function pn(e, t) {
  e.finishedWork = null, e.finishedLanes = 0;
  var n = e.timeoutHandle;
  if (n !== -1 && (e.timeoutHandle = -1, Jf(n)), xe !== null) for (n = xe.return; n !== null; ) {
    var r = n;
    switch (ro(r), r.tag) {
      case 1:
        r = r.type.childContextTypes, r != null && Tl();
        break;
      case 3:
        Kn(), le(We), le(Le), po();
        break;
      case 5:
        fo(r);
        break;
      case 4:
        Kn();
        break;
      case 13:
        le(se);
        break;
      case 19:
        le(se);
        break;
      case 10:
        oo(r.type._context);
        break;
      case 22:
      case 23:
        ko();
    }
    n = n.return;
  }
  if (Ce = e, xe = e = Zt(e.current, null), Ie = qe = t, je = 0, Ar = null, No = ra = yn = 0, Be = jr = null, dn !== null) {
    for (t = 0; t < dn.length; t++) if (n = dn[t], r = n.interleaved, r !== null) {
      n.interleaved = null;
      var l = r.next, i = n.pending;
      if (i !== null) {
        var o = i.next;
        i.next = l, r.next = o;
      }
      n.pending = r;
    }
    dn = null;
  }
  return e;
}
function Kc(e, t) {
  do {
    var n = xe;
    try {
      if (io(), gl.current = Vl, Ul) {
        for (var r = ue.memoizedState; r !== null; ) {
          var l = r.queue;
          l !== null && (l.pending = null), r = r.next;
        }
        Ul = !1;
      }
      if (xn = 0, we = ye = ue = null, xr = !1, Mr = 0, jo.current = null, n === null || n.return === null) {
        je = 1, Ar = t, xe = null;
        break;
      }
      e: {
        var i = e, o = n.return, s = n, u = t;
        if (t = Ie, s.flags |= 32768, u !== null && typeof u == "object" && typeof u.then == "function") {
          var d = u, N = s, c = N.tag;
          if (!(N.mode & 1) && (c === 0 || c === 11 || c === 15)) {
            var h = N.alternate;
            h ? (N.updateQueue = h.updateQueue, N.memoizedState = h.memoizedState, N.lanes = h.lanes) : (N.updateQueue = null, N.memoizedState = null);
          }
          var v = Es(o);
          if (v !== null) {
            v.flags &= -257, Is(v, o, s, i, t), v.mode & 1 && Cs(i, d, t), t = v, u = d;
            var y = t.updateQueue;
            if (y === null) {
              var w = /* @__PURE__ */ new Set();
              w.add(u), t.updateQueue = w;
            } else y.add(u);
            break e;
          } else {
            if (!(t & 1)) {
              Cs(i, d, t), Co();
              break e;
            }
            u = Error(I(426));
          }
        } else if (ie && s.mode & 1) {
          var _ = Es(o);
          if (_ !== null) {
            !(_.flags & 65536) && (_.flags |= 256), Is(_, o, s, i, t), lo(qn(u, s));
            break e;
          }
        }
        i = u = qn(u, s), je !== 4 && (je = 2), jr === null ? jr = [i] : jr.push(i), i = o;
        do {
          switch (i.tag) {
            case 3:
              i.flags |= 65536, t &= -t, i.lanes |= t;
              var f = Rc(i, u, t);
              ys(i, f);
              break e;
            case 1:
              s = u;
              var p = i.type, g = i.stateNode;
              if (!(i.flags & 128) && (typeof p.getDerivedStateFromError == "function" || g !== null && typeof g.componentDidCatch == "function" && (Yt === null || !Yt.has(g)))) {
                i.flags |= 65536, t &= -t, i.lanes |= t;
                var k = Tc(i, s, t);
                ys(i, k);
                break e;
              }
          }
          i = i.return;
        } while (i !== null);
      }
      Xc(n);
    } catch (T) {
      t = T, xe === n && n !== null && (xe = n = n.return);
      continue;
    }
    break;
  } while (!0);
}
function qc() {
  var e = Bl.current;
  return Bl.current = Vl, e === null ? Vl : e;
}
function Co() {
  (je === 0 || je === 3 || je === 2) && (je = 4), Ce === null || !(yn & 268435455) && !(ra & 268435455) || Bt(Ce, Ie);
}
function Wl(e, t) {
  var n = q;
  q |= 2;
  var r = qc();
  (Ce !== e || Ie !== t) && (Et = null, pn(e, t));
  do
    try {
      kp();
      break;
    } catch (l) {
      Kc(e, l);
    }
  while (!0);
  if (io(), q = n, Bl.current = r, xe !== null) throw Error(I(261));
  return Ce = null, Ie = 0, je;
}
function kp() {
  for (; xe !== null; ) Yc(xe);
}
function Cp() {
  for (; xe !== null && !qd(); ) Yc(xe);
}
function Yc(e) {
  var t = Jc(e.alternate, e, qe);
  e.memoizedProps = e.pendingProps, t === null ? Xc(e) : xe = t, jo.current = null;
}
function Xc(e) {
  var t = e;
  do {
    var n = t.alternate;
    if (e = t.return, t.flags & 32768) {
      if (n = xp(n, t), n !== null) {
        n.flags &= 32767, xe = n;
        return;
      }
      if (e !== null) e.flags |= 32768, e.subtreeFlags = 0, e.deletions = null;
      else {
        je = 6, xe = null;
        return;
      }
    } else if (n = gp(n, t, qe), n !== null) {
      xe = n;
      return;
    }
    if (t = t.sibling, t !== null) {
      xe = t;
      return;
    }
    xe = t = e;
  } while (t !== null);
  je === 0 && (je = 5);
}
function un(e, t, n) {
  var r = Y, l = at.transition;
  try {
    at.transition = null, Y = 1, Ep(e, t, n, r);
  } finally {
    at.transition = l, Y = r;
  }
  return null;
}
function Ep(e, t, n, r) {
  do
    bn();
  while (Ht !== null);
  if (q & 6) throw Error(I(327));
  n = e.finishedWork;
  var l = e.finishedLanes;
  if (n === null) return null;
  if (e.finishedWork = null, e.finishedLanes = 0, n === e.current) throw Error(I(177));
  e.callbackNode = null, e.callbackPriority = 0;
  var i = n.lanes | n.childLanes;
  if (af(e, i), e === Ce && (xe = Ce = null, Ie = 0), !(n.subtreeFlags & 2064) && !(n.flags & 2064) || ul || (ul = !0, ed(El, function() {
    return bn(), null;
  })), i = (n.flags & 15990) !== 0, n.subtreeFlags & 15990 || i) {
    i = at.transition, at.transition = null;
    var o = Y;
    Y = 1;
    var s = q;
    q |= 4, jo.current = null, jp(e, n), Wc(n, e), Qf(ci), Pl = !!ui, ci = ui = null, e.current = n, Np(n), Yd(), q = s, Y = o, at.transition = i;
  } else e.current = n;
  if (ul && (ul = !1, Ht = e, Hl = l), i = e.pendingLanes, i === 0 && (Yt = null), Jd(n.stateNode), Ge(e, ve()), t !== null) for (r = e.onRecoverableError, n = 0; n < t.length; n++) l = t[n], r(l.value, { componentStack: l.stack, digest: l.digest });
  if (bl) throw bl = !1, e = _i, _i = null, e;
  return Hl & 1 && e.tag !== 0 && bn(), i = e.pendingLanes, i & 1 ? e === Ri ? Nr++ : (Nr = 0, Ri = e) : Nr = 0, rn(), null;
}
function bn() {
  if (Ht !== null) {
    var e = Ru(Hl), t = at.transition, n = Y;
    try {
      if (at.transition = null, Y = 16 > e ? 16 : e, Ht === null) var r = !1;
      else {
        if (e = Ht, Ht = null, Hl = 0, q & 6) throw Error(I(331));
        var l = q;
        for (q |= 4, $ = e.current; $ !== null; ) {
          var i = $, o = i.child;
          if ($.flags & 16) {
            var s = i.deletions;
            if (s !== null) {
              for (var u = 0; u < s.length; u++) {
                var d = s[u];
                for ($ = d; $ !== null; ) {
                  var N = $;
                  switch (N.tag) {
                    case 0:
                    case 11:
                    case 15:
                      yr(8, N, i);
                  }
                  var c = N.child;
                  if (c !== null) c.return = N, $ = c;
                  else for (; $ !== null; ) {
                    N = $;
                    var h = N.sibling, v = N.return;
                    if (Bc(N), N === d) {
                      $ = null;
                      break;
                    }
                    if (h !== null) {
                      h.return = v, $ = h;
                      break;
                    }
                    $ = v;
                  }
                }
              }
              var y = i.alternate;
              if (y !== null) {
                var w = y.child;
                if (w !== null) {
                  y.child = null;
                  do {
                    var _ = w.sibling;
                    w.sibling = null, w = _;
                  } while (w !== null);
                }
              }
              $ = i;
            }
          }
          if (i.subtreeFlags & 2064 && o !== null) o.return = i, $ = o;
          else e: for (; $ !== null; ) {
            if (i = $, i.flags & 2048) switch (i.tag) {
              case 0:
              case 11:
              case 15:
                yr(9, i, i.return);
            }
            var f = i.sibling;
            if (f !== null) {
              f.return = i.return, $ = f;
              break e;
            }
            $ = i.return;
          }
        }
        var p = e.current;
        for ($ = p; $ !== null; ) {
          o = $;
          var g = o.child;
          if (o.subtreeFlags & 2064 && g !== null) g.return = o, $ = g;
          else e: for (o = p; $ !== null; ) {
            if (s = $, s.flags & 2048) try {
              switch (s.tag) {
                case 0:
                case 11:
                case 15:
                  na(9, s);
              }
            } catch (T) {
              he(s, s.return, T);
            }
            if (s === o) {
              $ = null;
              break e;
            }
            var k = s.sibling;
            if (k !== null) {
              k.return = s.return, $ = k;
              break e;
            }
            $ = s.return;
          }
        }
        if (q = l, rn(), St && typeof St.onPostCommitFiberRoot == "function") try {
          St.onPostCommitFiberRoot(Kl, e);
        } catch {
        }
        r = !0;
      }
      return r;
    } finally {
      Y = n, at.transition = t;
    }
  }
  return !1;
}
function Us(e, t, n) {
  t = qn(n, t), t = Rc(e, t, 1), e = qt(e, t, 1), t = Oe(), e !== null && (Br(e, 1, t), Ge(e, t));
}
function he(e, t, n) {
  if (e.tag === 3) Us(e, e, n);
  else for (; t !== null; ) {
    if (t.tag === 3) {
      Us(t, e, n);
      break;
    } else if (t.tag === 1) {
      var r = t.stateNode;
      if (typeof t.type.getDerivedStateFromError == "function" || typeof r.componentDidCatch == "function" && (Yt === null || !Yt.has(r))) {
        e = qn(n, e), e = Tc(t, e, 1), t = qt(t, e, 1), e = Oe(), t !== null && (Br(t, 1, e), Ge(t, e));
        break;
      }
    }
    t = t.return;
  }
}
function Ip(e, t, n) {
  var r = e.pingCache;
  r !== null && r.delete(t), t = Oe(), e.pingedLanes |= e.suspendedLanes & n, Ce === e && (Ie & n) === n && (je === 4 || je === 3 && (Ie & 130023424) === Ie && 500 > ve() - wo ? pn(e, 0) : No |= n), Ge(e, t);
}
function Zc(e, t) {
  t === 0 && (e.mode & 1 ? (t = Jr, Jr <<= 1, !(Jr & 130023424) && (Jr = 4194304)) : t = 1);
  var n = Oe();
  e = zt(e, t), e !== null && (Br(e, t, n), Ge(e, n));
}
function Pp(e) {
  var t = e.memoizedState, n = 0;
  t !== null && (n = t.retryLane), Zc(e, n);
}
function Fp(e, t) {
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
      throw Error(I(314));
  }
  r !== null && r.delete(t), Zc(e, n);
}
var Jc;
Jc = function(e, t, n) {
  if (e !== null) if (e.memoizedProps !== t.pendingProps || We.current) He = !0;
  else {
    if (!(e.lanes & n) && !(t.flags & 128)) return He = !1, vp(e, t, n);
    He = !!(e.flags & 131072);
  }
  else He = !1, ie && t.flags & 1048576 && rc(t, Dl, t.index);
  switch (t.lanes = 0, t.tag) {
    case 2:
      var r = t.type;
      yl(e, t), e = t.pendingProps;
      var l = Wn(t, Le.current);
      Bn(t, n), l = ho(null, t, r, e, l, n);
      var i = vo();
      return t.flags |= 1, typeof l == "object" && l !== null && typeof l.render == "function" && l.$$typeof === void 0 ? (t.tag = 1, t.memoizedState = null, t.updateQueue = null, Qe(r) ? (i = !0, zl(t)) : i = !1, t.memoizedState = l.state !== null && l.state !== void 0 ? l.state : null, uo(t), l.updater = ta, t.stateNode = l, l._reactInternals = t, yi(t, r, e, n), t = wi(null, t, r, !0, i, n)) : (t.tag = 0, ie && i && no(t), Me(null, t, l, n), t = t.child), t;
    case 16:
      r = t.elementType;
      e: {
        switch (yl(e, t), e = t.pendingProps, l = r._init, r = l(r._payload), t.type = r, l = t.tag = Rp(r), e = ft(r, e), l) {
          case 0:
            t = Ni(null, t, r, e, n);
            break e;
          case 1:
            t = _s(null, t, r, e, n);
            break e;
          case 11:
            t = Ps(null, t, r, e, n);
            break e;
          case 14:
            t = Fs(null, t, r, ft(r.type, e), n);
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
      return r = t.type, l = t.pendingProps, l = t.elementType === r ? l : ft(r, l), Ni(e, t, r, l, n);
    case 1:
      return r = t.type, l = t.pendingProps, l = t.elementType === r ? l : ft(r, l), _s(e, t, r, l, n);
    case 3:
      e: {
        if (Mc(t), e === null) throw Error(I(387));
        r = t.pendingProps, i = t.memoizedState, l = i.element, uc(e, t), Ol(t, r, null, n);
        var o = t.memoizedState;
        if (r = o.element, i.isDehydrated) if (i = { element: r, isDehydrated: !1, cache: o.cache, pendingSuspenseBoundaries: o.pendingSuspenseBoundaries, transitions: o.transitions }, t.updateQueue.baseState = i, t.memoizedState = i, t.flags & 256) {
          l = qn(Error(I(423)), t), t = Rs(e, t, r, n, l);
          break e;
        } else if (r !== l) {
          l = qn(Error(I(424)), t), t = Rs(e, t, r, n, l);
          break e;
        } else for (Ye = Kt(t.stateNode.containerInfo.firstChild), Xe = t, ie = !0, mt = null, n = oc(t, null, r, n), t.child = n; n; ) n.flags = n.flags & -3 | 4096, n = n.sibling;
        else {
          if (Qn(), r === l) {
            t = Lt(e, t, n);
            break e;
          }
          Me(e, t, r, n);
        }
        t = t.child;
      }
      return t;
    case 5:
      return cc(t), e === null && vi(t), r = t.type, l = t.pendingProps, i = e !== null ? e.memoizedProps : null, o = l.children, di(r, l) ? o = null : i !== null && di(r, i) && (t.flags |= 32), Dc(e, t), Me(e, t, o, n), t.child;
    case 6:
      return e === null && vi(t), null;
    case 13:
      return $c(e, t, n);
    case 4:
      return co(t, t.stateNode.containerInfo), r = t.pendingProps, e === null ? t.child = Gn(t, null, r, n) : Me(e, t, r, n), t.child;
    case 11:
      return r = t.type, l = t.pendingProps, l = t.elementType === r ? l : ft(r, l), Ps(e, t, r, l, n);
    case 7:
      return Me(e, t, t.pendingProps, n), t.child;
    case 8:
      return Me(e, t, t.pendingProps.children, n), t.child;
    case 12:
      return Me(e, t, t.pendingProps.children, n), t.child;
    case 10:
      e: {
        if (r = t.type._context, l = t.pendingProps, i = t.memoizedProps, o = l.value, ne(Ml, r._currentValue), r._currentValue = o, i !== null) if (yt(i.value, o)) {
          if (i.children === l.children && !We.current) {
            t = Lt(e, t, n);
            break e;
          }
        } else for (i = t.child, i !== null && (i.return = t); i !== null; ) {
          var s = i.dependencies;
          if (s !== null) {
            o = i.child;
            for (var u = s.firstContext; u !== null; ) {
              if (u.context === r) {
                if (i.tag === 1) {
                  u = _t(-1, n & -n), u.tag = 2;
                  var d = i.updateQueue;
                  if (d !== null) {
                    d = d.shared;
                    var N = d.pending;
                    N === null ? u.next = u : (u.next = N.next, N.next = u), d.pending = u;
                  }
                }
                i.lanes |= n, u = i.alternate, u !== null && (u.lanes |= n), gi(
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
            if (o = i.return, o === null) throw Error(I(341));
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
        Me(e, t, l.children, n), t = t.child;
      }
      return t;
    case 9:
      return l = t.type, r = t.pendingProps.children, Bn(t, n), l = it(l), r = r(l), t.flags |= 1, Me(e, t, r, n), t.child;
    case 14:
      return r = t.type, l = ft(r, t.pendingProps), l = ft(r.type, l), Fs(e, t, r, l, n);
    case 15:
      return zc(e, t, t.type, t.pendingProps, n);
    case 17:
      return r = t.type, l = t.pendingProps, l = t.elementType === r ? l : ft(r, l), yl(e, t), t.tag = 1, Qe(r) ? (e = !0, zl(t)) : e = !1, Bn(t, n), _c(t, r, l), yi(t, r, l, n), wi(null, t, r, !0, e, n);
    case 19:
      return Oc(e, t, n);
    case 22:
      return Lc(e, t, n);
  }
  throw Error(I(156, t.tag));
};
function ed(e, t) {
  return Iu(e, t);
}
function _p(e, t, n, r) {
  this.tag = e, this.key = n, this.sibling = this.child = this.return = this.stateNode = this.type = this.elementType = null, this.index = 0, this.ref = null, this.pendingProps = t, this.dependencies = this.memoizedState = this.updateQueue = this.memoizedProps = null, this.mode = r, this.subtreeFlags = this.flags = 0, this.deletions = null, this.childLanes = this.lanes = 0, this.alternate = null;
}
function lt(e, t, n, r) {
  return new _p(e, t, n, r);
}
function Eo(e) {
  return e = e.prototype, !(!e || !e.isReactComponent);
}
function Rp(e) {
  if (typeof e == "function") return Eo(e) ? 1 : 0;
  if (e != null) {
    if (e = e.$$typeof, e === Hi) return 11;
    if (e === Wi) return 14;
  }
  return 2;
}
function Zt(e, t) {
  var n = e.alternate;
  return n === null ? (n = lt(e.tag, t, e.key, e.mode), n.elementType = e.elementType, n.type = e.type, n.stateNode = e.stateNode, n.alternate = e, e.alternate = n) : (n.pendingProps = t, n.type = e.type, n.flags = 0, n.subtreeFlags = 0, n.deletions = null), n.flags = e.flags & 14680064, n.childLanes = e.childLanes, n.lanes = e.lanes, n.child = e.child, n.memoizedProps = e.memoizedProps, n.memoizedState = e.memoizedState, n.updateQueue = e.updateQueue, t = e.dependencies, n.dependencies = t === null ? null : { lanes: t.lanes, firstContext: t.firstContext }, n.sibling = e.sibling, n.index = e.index, n.ref = e.ref, n;
}
function wl(e, t, n, r, l, i) {
  var o = 2;
  if (r = e, typeof e == "function") Eo(e) && (o = 1);
  else if (typeof e == "string") o = 5;
  else e: switch (e) {
    case In:
      return mn(n.children, l, i, t);
    case bi:
      o = 8, l |= 8;
      break;
    case ba:
      return e = lt(12, n, t, l | 2), e.elementType = ba, e.lanes = i, e;
    case Ha:
      return e = lt(13, n, t, l), e.elementType = Ha, e.lanes = i, e;
    case Wa:
      return e = lt(19, n, t, l), e.elementType = Wa, e.lanes = i, e;
    case cu:
      return la(n, l, i, t);
    default:
      if (typeof e == "object" && e !== null) switch (e.$$typeof) {
        case su:
          o = 10;
          break e;
        case uu:
          o = 9;
          break e;
        case Hi:
          o = 11;
          break e;
        case Wi:
          o = 14;
          break e;
        case At:
          o = 16, r = null;
          break e;
      }
      throw Error(I(130, e == null ? e : typeof e, ""));
  }
  return t = lt(o, n, t, l), t.elementType = e, t.type = r, t.lanes = i, t;
}
function mn(e, t, n, r) {
  return e = lt(7, e, r, t), e.lanes = n, e;
}
function la(e, t, n, r) {
  return e = lt(22, e, r, t), e.elementType = cu, e.lanes = n, e.stateNode = { isHidden: !1 }, e;
}
function $a(e, t, n) {
  return e = lt(6, e, null, t), e.lanes = n, e;
}
function Oa(e, t, n) {
  return t = lt(4, e.children !== null ? e.children : [], e.key, t), t.lanes = n, t.stateNode = { containerInfo: e.containerInfo, pendingChildren: null, implementation: e.implementation }, t;
}
function Tp(e, t, n, r, l) {
  this.tag = t, this.containerInfo = e, this.finishedWork = this.pingCache = this.current = this.pendingChildren = null, this.timeoutHandle = -1, this.callbackNode = this.pendingContext = this.context = null, this.callbackPriority = 0, this.eventTimes = xa(0), this.expirationTimes = xa(-1), this.entangledLanes = this.finishedLanes = this.mutableReadLanes = this.expiredLanes = this.pingedLanes = this.suspendedLanes = this.pendingLanes = 0, this.entanglements = xa(0), this.identifierPrefix = r, this.onRecoverableError = l, this.mutableSourceEagerHydrationData = null;
}
function Io(e, t, n, r, l, i, o, s, u) {
  return e = new Tp(e, t, n, s, u), t === 1 ? (t = 1, i === !0 && (t |= 8)) : t = 0, i = lt(3, null, null, t), e.current = i, i.stateNode = e, i.memoizedState = { element: r, isDehydrated: n, cache: null, transitions: null, pendingSuspenseBoundaries: null }, uo(i), e;
}
function zp(e, t, n) {
  var r = 3 < arguments.length && arguments[3] !== void 0 ? arguments[3] : null;
  return { $$typeof: En, key: r == null ? null : "" + r, children: e, containerInfo: t, implementation: n };
}
function td(e) {
  if (!e) return en;
  e = e._reactInternals;
  e: {
    if (Sn(e) !== e || e.tag !== 1) throw Error(I(170));
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
    throw Error(I(171));
  }
  if (e.tag === 1) {
    var n = e.type;
    if (Qe(n)) return tc(e, n, t);
  }
  return t;
}
function nd(e, t, n, r, l, i, o, s, u) {
  return e = Io(n, r, !0, e, l, i, o, s, u), e.context = td(null), n = e.current, r = Oe(), l = Xt(n), i = _t(r, l), i.callback = t ?? null, qt(n, i, l), e.current.lanes = l, Br(e, l, r), Ge(e, r), e;
}
function aa(e, t, n, r) {
  var l = t.current, i = Oe(), o = Xt(l);
  return n = td(n), t.context === null ? t.context = n : t.pendingContext = n, t = _t(i, o), t.payload = { element: e }, r = r === void 0 ? null : r, r !== null && (t.callback = r), e = qt(l, t, o), e !== null && (gt(e, l, o, i), vl(e, l, o)), o;
}
function Ql(e) {
  if (e = e.current, !e.child) return null;
  switch (e.child.tag) {
    case 5:
      return e.child.stateNode;
    default:
      return e.child.stateNode;
  }
}
function Vs(e, t) {
  if (e = e.memoizedState, e !== null && e.dehydrated !== null) {
    var n = e.retryLane;
    e.retryLane = n !== 0 && n < t ? n : t;
  }
}
function Po(e, t) {
  Vs(e, t), (e = e.alternate) && Vs(e, t);
}
function Lp() {
  return null;
}
var rd = typeof reportError == "function" ? reportError : function(e) {
  console.error(e);
};
function Fo(e) {
  this._internalRoot = e;
}
ia.prototype.render = Fo.prototype.render = function(e) {
  var t = this._internalRoot;
  if (t === null) throw Error(I(409));
  aa(e, t, null, null);
};
ia.prototype.unmount = Fo.prototype.unmount = function() {
  var e = this._internalRoot;
  if (e !== null) {
    this._internalRoot = null;
    var t = e.containerInfo;
    jn(function() {
      aa(null, e, null, null);
    }), t[Tt] = null;
  }
};
function ia(e) {
  this._internalRoot = e;
}
ia.prototype.unstable_scheduleHydration = function(e) {
  if (e) {
    var t = Lu();
    e = { blockedOn: null, target: e, priority: t };
    for (var n = 0; n < Vt.length && t !== 0 && t < Vt[n].priority; n++) ;
    Vt.splice(n, 0, e), n === 0 && Mu(e);
  }
};
function _o(e) {
  return !(!e || e.nodeType !== 1 && e.nodeType !== 9 && e.nodeType !== 11);
}
function oa(e) {
  return !(!e || e.nodeType !== 1 && e.nodeType !== 9 && e.nodeType !== 11 && (e.nodeType !== 8 || e.nodeValue !== " react-mount-point-unstable "));
}
function Bs() {
}
function Dp(e, t, n, r, l) {
  if (l) {
    if (typeof r == "function") {
      var i = r;
      r = function() {
        var d = Ql(o);
        i.call(d);
      };
    }
    var o = nd(t, r, e, 0, null, !1, !1, "", Bs);
    return e._reactRootContainer = o, e[Tt] = o.current, Rr(e.nodeType === 8 ? e.parentNode : e), jn(), o;
  }
  for (; l = e.lastChild; ) e.removeChild(l);
  if (typeof r == "function") {
    var s = r;
    r = function() {
      var d = Ql(u);
      s.call(d);
    };
  }
  var u = Io(e, 0, !1, null, null, !1, !1, "", Bs);
  return e._reactRootContainer = u, e[Tt] = u.current, Rr(e.nodeType === 8 ? e.parentNode : e), jn(function() {
    aa(t, u, n, r);
  }), u;
}
function sa(e, t, n, r, l) {
  var i = n._reactRootContainer;
  if (i) {
    var o = i;
    if (typeof l == "function") {
      var s = l;
      l = function() {
        var u = Ql(o);
        s.call(u);
      };
    }
    aa(t, o, e, l);
  } else o = Dp(n, t, e, l, r);
  return Ql(o);
}
Tu = function(e) {
  switch (e.tag) {
    case 3:
      var t = e.stateNode;
      if (t.current.memoizedState.isDehydrated) {
        var n = dr(t.pendingLanes);
        n !== 0 && (Ki(t, n | 1), Ge(t, ve()), !(q & 6) && (Yn = ve() + 500, rn()));
      }
      break;
    case 13:
      jn(function() {
        var r = zt(e, 1);
        if (r !== null) {
          var l = Oe();
          gt(r, e, 1, l);
        }
      }), Po(e, 1);
  }
};
qi = function(e) {
  if (e.tag === 13) {
    var t = zt(e, 134217728);
    if (t !== null) {
      var n = Oe();
      gt(t, e, 134217728, n);
    }
    Po(e, 134217728);
  }
};
zu = function(e) {
  if (e.tag === 13) {
    var t = Xt(e), n = zt(e, t);
    if (n !== null) {
      var r = Oe();
      gt(n, e, t, r);
    }
    Po(e, t);
  }
};
Lu = function() {
  return Y;
};
Du = function(e, t) {
  var n = Y;
  try {
    return Y = e, t();
  } finally {
    Y = n;
  }
};
ti = function(e, t, n) {
  switch (t) {
    case "input":
      if (Ka(e, n), t = n.name, n.type === "radio" && t != null) {
        for (n = e; n.parentNode; ) n = n.parentNode;
        for (n = n.querySelectorAll("input[name=" + JSON.stringify("" + t) + '][type="radio"]'), t = 0; t < n.length; t++) {
          var r = n[t];
          if (r !== e && r.form === e.form) {
            var l = Zl(r);
            if (!l) throw Error(I(90));
            fu(r), Ka(r, l);
          }
        }
      }
      break;
    case "textarea":
      mu(e, n);
      break;
    case "select":
      t = n.value, t != null && On(e, !!n.multiple, t, !1);
  }
};
Nu = So;
wu = jn;
var Mp = { usingClientEntryPoint: !1, Events: [Hr, Rn, Zl, yu, ju, So] }, sr = { findFiberByHostInstance: cn, bundleType: 0, version: "18.3.1", rendererPackageName: "react-dom" }, $p = { bundleType: sr.bundleType, version: sr.version, rendererPackageName: sr.rendererPackageName, rendererConfig: sr.rendererConfig, overrideHookState: null, overrideHookStateDeletePath: null, overrideHookStateRenamePath: null, overrideProps: null, overridePropsDeletePath: null, overridePropsRenamePath: null, setErrorHandler: null, setSuspenseHandler: null, scheduleUpdate: null, currentDispatcherRef: Dt.ReactCurrentDispatcher, findHostInstanceByFiber: function(e) {
  return e = Cu(e), e === null ? null : e.stateNode;
}, findFiberByHostInstance: sr.findFiberByHostInstance || Lp, findHostInstancesForRefresh: null, scheduleRefresh: null, scheduleRoot: null, setRefreshHandler: null, getCurrentFiber: null, reconcilerVersion: "18.3.1-next-f1338f8080-20240426" };
if (typeof __REACT_DEVTOOLS_GLOBAL_HOOK__ < "u") {
  var cl = __REACT_DEVTOOLS_GLOBAL_HOOK__;
  if (!cl.isDisabled && cl.supportsFiber) try {
    Kl = cl.inject($p), St = cl;
  } catch {
  }
}
Je.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED = Mp;
Je.createPortal = function(e, t) {
  var n = 2 < arguments.length && arguments[2] !== void 0 ? arguments[2] : null;
  if (!_o(t)) throw Error(I(200));
  return zp(e, t, null, n);
};
Je.createRoot = function(e, t) {
  if (!_o(e)) throw Error(I(299));
  var n = !1, r = "", l = rd;
  return t != null && (t.unstable_strictMode === !0 && (n = !0), t.identifierPrefix !== void 0 && (r = t.identifierPrefix), t.onRecoverableError !== void 0 && (l = t.onRecoverableError)), t = Io(e, 1, !1, null, null, n, !1, r, l), e[Tt] = t.current, Rr(e.nodeType === 8 ? e.parentNode : e), new Fo(t);
};
Je.findDOMNode = function(e) {
  if (e == null) return null;
  if (e.nodeType === 1) return e;
  var t = e._reactInternals;
  if (t === void 0)
    throw typeof e.render == "function" ? Error(I(188)) : (e = Object.keys(e).join(","), Error(I(268, e)));
  return e = Cu(t), e = e === null ? null : e.stateNode, e;
};
Je.flushSync = function(e) {
  return jn(e);
};
Je.hydrate = function(e, t, n) {
  if (!oa(t)) throw Error(I(200));
  return sa(null, e, t, !0, n);
};
Je.hydrateRoot = function(e, t, n) {
  if (!_o(e)) throw Error(I(405));
  var r = n != null && n.hydratedSources || null, l = !1, i = "", o = rd;
  if (n != null && (n.unstable_strictMode === !0 && (l = !0), n.identifierPrefix !== void 0 && (i = n.identifierPrefix), n.onRecoverableError !== void 0 && (o = n.onRecoverableError)), t = nd(t, null, e, 1, n ?? null, l, !1, i, o), e[Tt] = t.current, Rr(e), r) for (e = 0; e < r.length; e++) n = r[e], l = n._getVersion, l = l(n._source), t.mutableSourceEagerHydrationData == null ? t.mutableSourceEagerHydrationData = [n, l] : t.mutableSourceEagerHydrationData.push(
    n,
    l
  );
  return new ia(t);
};
Je.render = function(e, t, n) {
  if (!oa(t)) throw Error(I(200));
  return sa(null, e, t, !1, n);
};
Je.unmountComponentAtNode = function(e) {
  if (!oa(e)) throw Error(I(40));
  return e._reactRootContainer ? (jn(function() {
    sa(null, null, e, !1, function() {
      e._reactRootContainer = null, e[Tt] = null;
    });
  }), !0) : !1;
};
Je.unstable_batchedUpdates = So;
Je.unstable_renderSubtreeIntoContainer = function(e, t, n, r) {
  if (!oa(n)) throw Error(I(200));
  if (e == null || e._reactInternals === void 0) throw Error(I(38));
  return sa(e, t, n, !1, r);
};
Je.version = "18.3.1-next-f1338f8080-20240426";
function ld() {
  if (!(typeof __REACT_DEVTOOLS_GLOBAL_HOOK__ > "u" || typeof __REACT_DEVTOOLS_GLOBAL_HOOK__.checkDCE != "function"))
    try {
      __REACT_DEVTOOLS_GLOBAL_HOOK__.checkDCE(ld);
    } catch (e) {
      console.error(e);
    }
}
ld(), lu.exports = Je;
var Op = lu.exports, ad, bs = Op;
ad = bs.createRoot, bs.hydrateRoot;
class Ap extends Error {
  constructor(t, n) {
    super(t), this.status = n, this.name = "ApiError";
  }
}
function Up(e, t) {
  async function n(r, l = {}) {
    const i = { ...l.headers ?? {} }, o = t();
    o && (i.Authorization = "Bearer " + o);
    let s;
    l.body !== void 0 && (i["Content-Type"] = "application/json", s = JSON.stringify(l.body));
    const u = await e(r, { method: l.method ?? "GET", headers: i, body: s });
    if (!u.ok) {
      let N = `HTTP ${u.status}`;
      try {
        const c = await u.json();
        N = c.detail || c.title || N;
      } catch {
      }
      throw new Ap(N, u.status);
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
const id = x.createContext(null);
function Ct() {
  const e = x.useContext(id);
  if (!e) throw new Error("Fuera del módulo de documentos");
  return e;
}
function Vp(e) {
  return Up((t, n) => fetch(t, n), e.token);
}
async function Li(e, t) {
  const n = e.token(), r = await fetch(t, { headers: n ? { Authorization: "Bearer " + n } : {} });
  if (!r.ok) throw new Error("No se pudo generar el documento");
  const l = URL.createObjectURL(await r.blob());
  window.open(l, "_blank"), setTimeout(() => URL.revokeObjectURL(l), 6e4);
}
function od(e, t) {
  return `${e.toLowerCase().normalize("NFD").replace(/[̀-ͯ]/g, "").replace(/[^a-z0-9]+/g, "-").replace(/^-|-$/g, "").slice(0, 60) || "listado"}-${(/* @__PURE__ */ new Date()).toISOString().slice(0, 10)}.${t}`;
}
function sd(e, t) {
  const n = URL.createObjectURL(e), r = document.createElement("a");
  r.href = n, r.download = t, document.body.appendChild(r), r.click(), r.remove(), setTimeout(() => URL.revokeObjectURL(n), 6e4);
}
async function Bp(e, t) {
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
  sd(await n.blob(), od(t.titulo, "xlsx"));
}
function bp(e) {
  const t = (r, l) => {
    if (r == null) return "";
    const i = typeof r == "number" ? l === "texto" ? String(r) : r.toFixed(2).replace(".", ",") : r;
    return /[";\n]/.test(i) ? `"${i.replace(/"/g, '""')}"` : i;
  }, n = [e.columnas.map((r) => t(r.titulo, "texto")).join(";")];
  for (const r of e.filas) n.push(r.map((l, i) => {
    var o;
    return t(l, ((o = e.columnas[i]) == null ? void 0 : o.tipo) ?? "texto");
  }).join(";"));
  return n.join(`\r
`);
}
function Hp(e) {
  sd(new Blob(["\uFEFF" + bp(e)], { type: "text/csv;charset=utf-8" }), od(e.titulo, "csv"));
}
const ud = new Intl.NumberFormat("es-ES", { minimumFractionDigits: 2, maximumFractionDigits: 2 }), Wp = new Intl.NumberFormat("es-ES", { maximumFractionDigits: 3 }), D = (e) => `${ud.format(Number(e) || 0)} €`, ke = (e) => ud.format(Number(e) || 0), $e = (e) => Wp.format(Number(e) || 0), Se = (e) => {
  if (!e) return "";
  const t = String(e).slice(0, 10).split("-");
  return t.length === 3 ? `${t[2]}/${t[1]}/${t[0]}` : String(e);
}, xt = () => (/* @__PURE__ */ new Date()).toISOString().slice(0, 10), be = (e) => Math.round((e + Number.EPSILON) * 100) / 100;
let Qp = 0;
const Qr = () => `l${Date.now().toString(36)}${(++Qp).toString(36)}`;
function hn(e, t) {
  const [n, r] = x.useState(e);
  return x.useEffect(() => {
    const l = setTimeout(() => r(e), t);
    return () => clearTimeout(l);
  }, [e, t]), n;
}
function Gr() {
  const e = x.useRef(0);
  return () => {
    const t = ++e.current;
    return () => t === e.current;
  };
}
function Ro(e) {
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
const Hs = { presupuesto: "Presupuestos", pedido: "Pedidos de venta", factura: "Facturas emitidas", compra: "Pedidos de compra", gasto: "Facturas de proveedor y gastos" }, Gp = { presupuesto: "+ Nuevo presupuesto", pedido: "+ Nuevo pedido", factura: "+ Nueva factura", compra: "+ Nuevo pedido", gasto: "+ Registrar factura" }, Kp = {
  presupuesto: ["Borrador", "Aceptado", "Rechazado"],
  pedido: ["Borrador", "Confirmado", "ServidoParcial", "Servido", "Facturado", "Cancelado"],
  factura: ["Emitida", "Rectificada", "Anulada"],
  compra: ["Borrador", "Confirmado", "RecibidoParcial", "Recibido", "Facturado", "Cancelado"],
  gasto: ["Registrado", "Anulado"]
}, Aa = { numero: "numero", fecha: "fecha", tercero: "tercero", base: "base", impuestos: "impuestos", total: "total" }, Ws = 50, Di = (e) => e === "Anulada" || e === "Anulado" || e === "Cancelado";
function Qs(e, t) {
  const n = { documentos: 0, baseImponible: 0, impuestos: 0, retenciones: 0, total: 0, pendiente: 0, vencido: 0, documentosVencidos: 0 };
  for (const r of e) {
    if (Di(r.estado)) continue;
    n.documentos++, n.baseImponible += r.base, n.impuestos += r.impuestos, n.total += r.total;
    const l = r.pendiente ?? 0;
    l > 0 && (n.pendiente += l, r.vencimiento && r.vencimiento < t && (n.vencido += l, n.documentosVencidos++));
  }
  return n.baseImponible = be(n.baseImponible), n.impuestos = be(n.impuestos), n.total = be(n.total), n.pendiente = be(n.pendiente), n.vencido = be(n.vencido), n;
}
function Ua(e, t, n) {
  const r = (l) => t === "numero" || t === "tercero" || t === "estado" ? l[t].toLowerCase() : t === "fecha" ? l.fecha : l[t] ?? 0;
  return [...e].sort((l, i) => {
    const o = r(l), s = r(i), u = typeof o == "number" && typeof s == "number" ? o - s : String(o).localeCompare(String(s), "es", { numeric: !0 });
    return n ? -u : u;
  });
}
const qp = (e, t) => ({
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
}), Yp = (e, t) => {
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
function Xp(e) {
  const { api: t, navegar: n, anfitrion: r } = Ct(), l = e.tipo, i = l === "factura" || l === "gasto", o = l === "compra" || l === "gasto", [s, u] = x.useState(""), [d, N] = x.useState(""), [c, h] = x.useState(""), [v, y] = x.useState(""), [w, _] = x.useState(""), [f, p] = x.useState(""), [g, k] = x.useState(""), [T, R] = x.useState(""), [z, M] = x.useState(""), [A, U] = x.useState({ campo: "fecha", desc: !0 }), [F, Q] = x.useState(1), [oe, Fe] = x.useState(null), [De, de] = x.useState(0), [ge, C] = x.useState(null), [j, S] = x.useState([]), [V, W] = x.useState([]), [K, fe] = x.useState(""), [pe, J] = x.useState(!1), O = hn(s, 250), st = hn(g, 350), Ke = hn(T, 350), ln = Gr(), ee = xt();
  x.useEffect(() => {
    t.get(o ? "/proveedores" : "/clientes").then((m) => S([...m].sort((L, P) => L.nombre.localeCompare(P.nombre, "es")))).catch(() => S([])), l === "factura" && t.get("/series").then((m) => W([...new Set(m.filter((L) => L.tipoDocumento === "Factura" || L.tipoDocumento === 0).map((L) => L.prefijo))].sort())).catch(() => W([]));
  }, [t, l, o]), x.useEffect(() => Q(1), [O, d, c, v, w, f, st, Ke, z, A, l]);
  const ut = (m, L) => {
    const P = new URLSearchParams({ pagina: String(m), tamanoPagina: String(L) });
    O.trim() && P.set("texto", O.trim()), d && P.set("estado", d === "Anulada" && l === "gasto" ? "Anulado" : d), c && P.set("desde", c), v && P.set("hasta", v), w && P.set(l === "gasto" ? "proveedorId" : "clienteId", w), f && l === "factura" && P.set("serie", f);
    const B = parseFloat(st.replace(/\./g, "").replace(",", ".")), b = parseFloat(Ke.replace(/\./g, "").replace(",", "."));
    isNaN(B) || P.set("importeMin", String(B)), isNaN(b) || P.set("importeMax", String(b)), z && P.set("cobro", z);
    const Z = Aa[A.campo];
    return Z && (P.set("orden", Z === "tercero" ? l === "gasto" ? "proveedor" : "cliente" : Z), P.set("desc", String(A.desc))), P;
  }, an = async (m, L) => {
    if (l === "factura") {
      const B = await t.get(`/facturas/buscar?${ut(m, L)}`);
      return { r: B, filas: B.elementos.map((b) => qp(b, B.pendientes)) };
    }
    const P = await t.get(`/gastos/buscar?${ut(m, L)}`);
    return { r: P, filas: P.elementos.map((B) => Yp(B, P.pendientes)) };
  };
  x.useEffect(() => {
    fe("");
    const m = ln();
    (async () => {
      if (i) {
        const { r: P, filas: B } = await an(F, Ws);
        return m() && (de(P.total), C(P.totales ?? null)), B;
      }
      switch (l) {
        case "presupuesto":
          return (await t.get("/presupuestos")).map((P) => ({ id: P.id, numero: P.numeroCompleto, fecha: P.fecha, tercero: P.clienteNombre, terceroId: P.clienteId, base: P.baseImponible ?? P.total, impuestos: P.cuotaIva ?? 0, total: P.total, estado: P.estado }));
        case "pedido":
          return (await t.get("/pedidos-venta")).map((P) => {
            const B = be(P.lineas.reduce((b, Z) => b + Z.base, 0));
            return { id: P.id, numero: P.numeroCompleto, fecha: P.fecha, tercero: P.clienteNombre, terceroId: P.clienteId, base: B, impuestos: be(P.total - B), total: P.total, estado: P.estado };
          });
        default:
          return (await t.get("/compras/pedidos")).map((P) => {
            const B = be(P.lineas.reduce((b, Z) => b + Z.importe, 0));
            return { id: P.id, numero: P.numeroCompleto, fecha: P.fecha, tercero: P.proveedorTexto, terceroId: P.proveedorId, base: B, impuestos: be(P.total - B), total: P.total, estado: P.estado, extra: P.empresaOrigenId ? "Intragrupo" : void 0 };
          });
      }
    })().then((P) => m() && Fe(P)).catch((P) => m() && (fe(P.message), Fe([])));
  }, [t, l, F, O, d, c, v, w, f, st, Ke, z, A.campo, A.desc]);
  const X = x.useMemo(() => {
    if (!oe) return [];
    if (i) return Aa[A.campo] ? oe : Ua(oe, A.campo, A.desc);
    const m = O.trim().toLowerCase(), L = parseFloat(st.replace(/\./g, "").replace(",", ".")), P = parseFloat(Ke.replace(/\./g, "").replace(",", ".")), B = oe.filter((b) => (!m || b.numero.toLowerCase().includes(m) || b.tercero.toLowerCase().includes(m)) && (!d || b.estado === d) && (!c || b.fecha >= c) && (!v || b.fecha <= v) && (!w || b.terceroId === w) && (isNaN(L) || b.total >= L) && (isNaN(P) || b.total <= P));
    return Ua(B, A.campo, A.desc);
  }, [oe, O, d, c, v, w, st, Ke, A, i]), Ne = i ? ge : Qs(X, ee), ct = i ? Math.max(1, Math.ceil(De / Ws)) : 1, tt = [d, c, v, w, f, g, T, z].filter(Boolean).length, Mt = o ? "Proveedor" : "Cliente";
  function on() {
    u(""), N(""), h(""), y(""), _(""), p(""), k(""), R(""), M("");
  }
  function Ve(m, L, P = !1) {
    const B = A.campo === m;
    return /* @__PURE__ */ a.jsxs("th", { className: (P ? "num " : "") + "dx-ordenable" + (B ? " activo" : ""), onClick: () => U({ campo: m, desc: B ? !A.desc : m === "fecha" || P }), title: `Ordenar por ${L.toLowerCase()}`, children: [
      L,
      /* @__PURE__ */ a.jsx("span", { className: "dx-flecha", children: B ? A.desc ? "▼" : "▲" : "" })
    ] });
  }
  async function er() {
    if (!i) return X;
    const m = [];
    for (let L = 1; L <= 500; L++) {
      const { r: P, filas: B } = await an(L, 200);
      if (m.push(...B), m.length >= P.total || B.length === 0) break;
    }
    return Aa[A.campo] ? m : Ua(m, A.campo, A.desc);
  }
  async function $t(m) {
    J(!0);
    try {
      const L = await er(), P = i, B = i ? ge : Qs(L, ee), b = {
        titulo: Hs[l],
        columnas: [
          { titulo: "Número", tipo: "texto" },
          { titulo: "Fecha", tipo: "fecha" },
          { titulo: Mt, tipo: "texto" },
          { titulo: "Estado", tipo: "texto" },
          { titulo: "Base", tipo: "moneda", total: "no" },
          { titulo: "Impuestos", tipo: "moneda", total: "no" },
          { titulo: "Total", tipo: "moneda", total: "no" },
          ...P ? [{ titulo: "Pendiente", tipo: "moneda", total: "no" }, { titulo: "Vencimiento", tipo: "fecha" }] : []
        ],
        filas: L.map((Z) => [Z.numero + (Z.extra ? ` (${Z.extra})` : ""), Se(Z.fecha), Z.tercero, Z.estado, Z.base, Z.impuestos, Z.total, ...P ? [Z.pendiente ?? 0, Se(Z.vencimiento)] : []]),
        totales: B ? [`Total · ${B.documentos} (sin anulados)`, null, null, null, B.baseImponible, B.impuestos, B.total, ...P ? [B.pendiente, null] : []] : void 0
      };
      m === "xlsx" ? await Bp(r.token(), b) : Hp(b), r.aviso(`Exportados ${L.length} documento(s).`, "ok");
    } catch (L) {
      r.aviso("No se pudo exportar: " + L.message, "err");
    } finally {
      J(!1);
    }
  }
  return /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
    /* @__PURE__ */ a.jsxs("div", { className: "panel-head", children: [
      /* @__PURE__ */ a.jsx("h2", { children: Hs[l] }),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-acciones", children: [
        /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", disabled: pe || !X.length, onClick: () => $t("xlsx"), title: "Exportar a Excel todo lo filtrado", children: pe ? "Exportando…" : "↓ Excel" }),
        /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", disabled: pe || !X.length, onClick: () => $t("csv"), title: "Exportar a CSV todo lo filtrado", children: "↓ CSV" }),
        /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: () => n({ tipo: l, pantalla: "editor" }), children: Gp[l] })
      ] })
    ] }),
    /* @__PURE__ */ a.jsxs("div", { className: "dx-filtros", children: [
      /* @__PURE__ */ a.jsx("input", { placeholder: o ? "Número, proveedor o concepto…" : "Número, cliente o NIF…", value: s, onChange: (m) => u(m.target.value), autoFocus: !0 }),
      /* @__PURE__ */ a.jsxs("select", { value: d, onChange: (m) => N(m.target.value), "aria-label": "Estado", children: [
        /* @__PURE__ */ a.jsx("option", { value: "", children: "Todos los estados" }),
        Kp[l].map((m) => /* @__PURE__ */ a.jsx("option", { value: m, children: m }, m))
      ] }),
      /* @__PURE__ */ a.jsx("input", { type: "date", value: c, onChange: (m) => h(m.target.value), title: "Desde", "aria-label": "Desde" }),
      /* @__PURE__ */ a.jsx("input", { type: "date", value: v, onChange: (m) => y(m.target.value), title: "Hasta", "aria-label": "Hasta" })
    ] }),
    /* @__PURE__ */ a.jsxs("div", { className: "dx-filtros dx-filtros2", children: [
      /* @__PURE__ */ a.jsxs("select", { value: w, onChange: (m) => _(m.target.value), "aria-label": Mt, children: [
        /* @__PURE__ */ a.jsx("option", { value: "", children: o ? "Todos los proveedores" : "Todos los clientes" }),
        j.map((m) => /* @__PURE__ */ a.jsxs("option", { value: m.id, children: [
          m.nombre,
          m.nifFiscal ? ` · ${m.nifFiscal}` : ""
        ] }, m.id))
      ] }),
      l === "factura" && /* @__PURE__ */ a.jsxs("select", { value: f, onChange: (m) => p(m.target.value), "aria-label": "Serie", children: [
        /* @__PURE__ */ a.jsx("option", { value: "", children: "Todas las series" }),
        V.map((m) => /* @__PURE__ */ a.jsxs("option", { value: m, children: [
          "Serie ",
          m
        ] }, m))
      ] }),
      /* @__PURE__ */ a.jsx("input", { inputMode: "decimal", placeholder: "Importe mín.", value: g, onChange: (m) => k(m.target.value), "aria-label": "Importe mínimo" }),
      /* @__PURE__ */ a.jsx("input", { inputMode: "decimal", placeholder: "Importe máx.", value: T, onChange: (m) => R(m.target.value), "aria-label": "Importe máximo" }),
      i && /* @__PURE__ */ a.jsxs("select", { value: z, onChange: (m) => M(m.target.value), "aria-label": l === "gasto" ? "Estado de pago" : "Estado de cobro", children: [
        /* @__PURE__ */ a.jsx("option", { value: "", children: l === "gasto" ? "Pagadas y pendientes" : "Cobradas y pendientes" }),
        /* @__PURE__ */ a.jsx("option", { value: "pendiente", children: "Pendientes" }),
        /* @__PURE__ */ a.jsx("option", { value: "vencida", children: "Vencidas" }),
        /* @__PURE__ */ a.jsx("option", { value: l === "gasto" ? "pagada" : "cobrada", children: l === "gasto" ? "Pagadas" : "Cobradas" })
      ] }),
      (tt > 0 || s) && /* @__PURE__ */ a.jsxs("button", { className: "btn small secondary", onClick: on, children: [
        "Limpiar",
        tt ? ` (${tt})` : ""
      ] })
    ] }),
    K && /* @__PURE__ */ a.jsx("p", { className: "dx-rojo", children: K }),
    oe === null ? /* @__PURE__ */ a.jsx("p", { className: "muted", children: "Cargando…" }) : X.length === 0 ? /* @__PURE__ */ a.jsx("p", { className: "muted", children: "No hay documentos con estos filtros." }) : /* @__PURE__ */ a.jsxs("table", { className: "dx-lista-docs", children: [
      /* @__PURE__ */ a.jsx("thead", { children: /* @__PURE__ */ a.jsxs("tr", { children: [
        Ve("numero", "Número"),
        Ve("fecha", "Fecha"),
        Ve("tercero", Mt),
        Ve("estado", "Estado"),
        Ve("base", "Base", !0),
        Ve("impuestos", "Impuestos", !0),
        Ve("total", "Total", !0),
        i && Ve("pendiente", "Pendiente", !0)
      ] }) }),
      /* @__PURE__ */ a.jsx("tbody", { children: X.map((m) => {
        const L = (m.pendiente ?? 0) > 0 && !!m.vencimiento && m.vencimiento < ee;
        return /* @__PURE__ */ a.jsxs("tr", { onClick: () => n({ tipo: l, pantalla: "vista", id: m.id }), tabIndex: 0, onKeyDown: (P) => P.key === "Enter" && n({ tipo: l, pantalla: "vista", id: m.id }), className: Di(m.estado) ? "dx-anulado" : void 0, children: [
          /* @__PURE__ */ a.jsxs("td", { className: "mono", children: [
            /* @__PURE__ */ a.jsx("strong", { children: m.numero }),
            m.extra && /* @__PURE__ */ a.jsx("span", { className: "pill part", style: { marginLeft: 6 }, children: m.extra })
          ] }),
          /* @__PURE__ */ a.jsx("td", { children: Se(m.fecha) }),
          /* @__PURE__ */ a.jsx("td", { children: m.tercero }),
          /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx("span", { className: Ro(m.estado), children: m.estado }) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: D(m.base) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: D(m.impuestos) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: /* @__PURE__ */ a.jsx("strong", { children: D(m.total) }) }),
          i && /* @__PURE__ */ a.jsx("td", { className: "num", children: (m.pendiente ?? 0) > 0 ? /* @__PURE__ */ a.jsx("strong", { className: L ? "dx-rojo" : void 0, title: L ? `Vencida el ${Se(m.vencimiento)}` : `Vence el ${Se(m.vencimiento)}`, children: D(m.pendiente) }) : /* @__PURE__ */ a.jsx("span", { className: "muted", children: Di(m.estado) || m.estado === "Rectificada" ? "—" : l === "gasto" ? "Pagada" : "Cobrada" }) })
        ] }, m.id);
      }) }),
      Ne && /* @__PURE__ */ a.jsx("tfoot", { children: /* @__PURE__ */ a.jsxs("tr", { className: "dx-total", children: [
        /* @__PURE__ */ a.jsxs("td", { colSpan: 4, children: [
          /* @__PURE__ */ a.jsxs("strong", { children: [
            "Total · ",
            Ne.documentos
          ] }),
          " ",
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: i ? `documento${Ne.documentos === 1 ? "" : "s"} de todo el filtro (${ct} página${ct === 1 ? "" : "s"}) · sin anulados` : "sin anulados ni cancelados" }),
          i && Ne.vencido > 0 && /* @__PURE__ */ a.jsxs("span", { className: "dx-rojo", style: { marginLeft: 8 }, children: [
            "· vencido ",
            D(Ne.vencido),
            " (",
            Ne.documentosVencidos,
            ")"
          ] })
        ] }),
        /* @__PURE__ */ a.jsx("td", { className: "num", children: /* @__PURE__ */ a.jsx("strong", { children: D(Ne.baseImponible) }) }),
        /* @__PURE__ */ a.jsx("td", { className: "num", children: /* @__PURE__ */ a.jsx("strong", { children: D(Ne.impuestos) }) }),
        /* @__PURE__ */ a.jsx("td", { className: "num", children: /* @__PURE__ */ a.jsx("strong", { children: D(Ne.total) }) }),
        i && /* @__PURE__ */ a.jsx("td", { className: "num", children: /* @__PURE__ */ a.jsx("strong", { children: D(Ne.pendiente) }) })
      ] }) })
    ] }),
    ct > 1 && /* @__PURE__ */ a.jsxs("div", { className: "dx-paginas", children: [
      /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", disabled: F <= 1, onClick: () => Q(F - 1), children: "←" }),
      /* @__PURE__ */ a.jsxs("span", { className: "muted", children: [
        "Página ",
        F,
        " de ",
        ct,
        " · ",
        De,
        " documentos"
      ] }),
      /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", disabled: F >= ct, onClick: () => Q(F + 1), children: "→" })
    ] })
  ] });
}
function Nn(e) {
  return x.useEffect(() => {
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
function Zp(e) {
  const { api: t } = Ct(), [n, r] = x.useState(!1), [l, i] = x.useState([]), [o, s] = x.useState(null), [u, d] = x.useState(0), N = hn(e.texto, 180), c = o === e.texto.trim() ? l : [], h = Gr();
  x.useEffect(() => {
    if (!n) return;
    const y = h(), w = encodeURIComponent(N.trim());
    t.get(`/productos/buscar?texto=${w}&tamanoPagina=12`).then((_) => y() && (i(_.elementos ?? []), s(N.trim()), d(0))).catch(() => y() && (i([]), s(N.trim())));
  }, [N, n]);
  function v(y) {
    var w;
    if (n && y.key === "Enter" && e.texto.trim() && !c.length) {
      y.preventDefault(), y.stopPropagation();
      return;
    }
    if (n && c.length) {
      if (y.key === "ArrowDown") return y.preventDefault(), d((_) => Math.min(_ + 1, c.length - 1));
      if (y.key === "ArrowUp") return y.preventDefault(), d((_) => Math.max(_ - 1, 0));
      if (y.key === "Enter") {
        y.preventDefault(), y.stopPropagation(), e.alElegir(c[u]), r(!1);
        return;
      }
    }
    if (y.key === "Escape") return r(!1);
    if (y.key === "F2") return y.preventDefault(), r(!0);
    (w = e.alTeclaFuera) == null || w.call(e, y);
  }
  return /* @__PURE__ */ a.jsxs("div", { className: "dx-buscador", children: [
    /* @__PURE__ */ a.jsx(
      "input",
      {
        value: e.texto,
        placeholder: "Buscar artículo…",
        autoFocus: e.autoFocus,
        onChange: (y) => (e.alCambiarTexto(y.target.value), r(!0)),
        onFocus: (y) => y.target.select(),
        onBlur: () => setTimeout(() => r(!1), 150),
        onKeyDown: v,
        ...Object.fromEntries(Object.entries(e.datos ?? {}).map(([y, w]) => [`data-${y}`, w]))
      }
    ),
    n && c.length > 0 && /* @__PURE__ */ a.jsx("div", { className: "dx-lista", children: c.map((y, w) => /* @__PURE__ */ a.jsxs(
      "div",
      {
        className: "dx-opcion" + (w === u ? " activa" : ""),
        onMouseDown: (_) => (_.preventDefault(), e.alElegir(y), r(!1)),
        onMouseEnter: () => d(w),
        children: [
          /* @__PURE__ */ a.jsxs("span", { children: [
            y.referencia && /* @__PURE__ */ a.jsxs("span", { className: "mono muted", children: [
              y.referencia,
              " · "
            ] }),
            /* @__PURE__ */ a.jsx("strong", { children: y.nombre }),
            y.familia && /* @__PURE__ */ a.jsxs("span", { className: "muted", children: [
              " · ",
              y.familia
            ] })
          ] }),
          /* @__PURE__ */ a.jsxs("span", { className: "muted", style: { whiteSpace: "nowrap" }, children: [
            D(e.precioDe ? e.precioDe(y) : y.precioUnitario),
            "/",
            y.unidad,
            y.controlarStock && /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
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
function To(e) {
  const [t, n] = x.useState(""), [r, l] = x.useState(!1), [i, o] = x.useState(0), s = e.terceros.find((c) => c.id === e.valor), u = x.useMemo(() => {
    const c = t.trim().toLowerCase();
    return e.terceros.filter((h) => h.activo !== !1 && (!c || h.nombre.toLowerCase().includes(c) || (h.nifFiscal ?? "").toLowerCase().includes(c))).slice(0, 30);
  }, [t, e.terceros]), d = x.useRef(null);
  function N(c) {
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
            if (c.key === "ArrowDown") return c.preventDefault(), o((h) => Math.min(h + 1, u.length - 1));
            if (c.key === "ArrowUp") return c.preventDefault(), o((h) => Math.max(h - 1, 0));
            if (c.key === "Enter" && u[i]) return c.preventDefault(), N(u[i]);
            if (c.key === "Escape") return l(!1);
          }
        }
      ),
      r && u.length > 0 && /* @__PURE__ */ a.jsx("div", { className: "dx-lista", children: u.map((c, h) => /* @__PURE__ */ a.jsxs("div", { className: "dx-opcion" + (h === i ? " activa" : ""), onMouseDown: (v) => (v.preventDefault(), N(c)), onMouseEnter: () => o(h), children: [
        /* @__PURE__ */ a.jsx("strong", { children: c.nombre }),
        /* @__PURE__ */ a.jsx("span", { className: "muted", children: [c.nifFiscal, c.poblacion].filter(Boolean).join(" · ") })
      ] }, c.id)) })
    ] })
  ] });
}
const Jp = { Porcentaje: "%", PorUnidad: "€/ud", PorKilo: "€/kg", Importe: "€" };
function zo(e) {
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
          Jp[s.calculo],
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
function ua(e) {
  var t;
  return (t = e.conceptos) != null && t.length ? /* @__PURE__ */ a.jsx("div", { className: "dx-aplicados", children: e.conceptos.map((n, r) => /* @__PURE__ */ a.jsxs("div", { children: [
    "· ",
    n.nombre,
    n.calculo === "Porcentaje" ? ` (${$e(n.valor)} %)` : "",
    n.repartido ? " · del documento" : "",
    n.efecto === "Coste" ? " · coste" : "",
    ": ",
    /* @__PURE__ */ a.jsx("strong", { children: D(n.importe) })
  ] }, r)) }) : null;
}
const wr = () => ({ clave: Qr(), productoId: null, descripcion: "", cantidad: 1, precio: null, dto: 0, iva: null });
function cd(e) {
  const t = x.useRef(null), [n, r] = x.useState(/* @__PURE__ */ new Set()), l = e.modo === "venta", i = l ? 7 : 5, o = (c, h) => e.alCambiar(e.lineas.map((v) => v.clave === c ? { ...v, ...h } : v)), s = (c) => {
    const h = e.lineas.filter((v) => v.clave !== c);
    e.alCambiar(h.length ? h : [wr()]);
  };
  function u(c, h) {
    var y;
    const v = (y = t.current) == null ? void 0 : y.querySelector(`[data-f="${c}"][data-c="${h}"]`);
    v == null || v.focus(), v instanceof HTMLInputElement && v.select();
  }
  function d(c) {
    const h = c.target, v = Number(h.dataset.f), y = Number(h.dataset.c);
    if (!(Number.isNaN(v) || Number.isNaN(y)))
      if (c.key === "Enter") {
        if (c.preventDefault(), y < i - 1) return u(v, y + 1);
        v === e.lineas.length - 1 ? (e.alCambiar([...e.lineas, wr()]), setTimeout(() => u(v + 1, 0), 30)) : u(v + 1, 0);
      } else c.key === "ArrowDown" && h.tagName !== "SELECT" ? (c.preventDefault(), u(Math.min(v + 1, e.lineas.length - 1), y)) : c.key === "ArrowUp" && h.tagName !== "SELECT" && (c.preventDefault(), u(Math.max(v - 1, 0), y));
  }
  const N = (c) => r((h) => {
    const v = new Set(h);
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
      /* @__PURE__ */ a.jsx("tbody", { children: e.lineas.map((c, h) => {
        const v = e.calculos[h], y = (v == null ? void 0 : v.conceptos) ?? [], w = l && c.controlarStock && c.stock != null && c.cantidad > c.stock, _ = v && v.margen != null && v.importe ? v.margen / v.importe * 100 : null;
        return [
          /* @__PURE__ */ a.jsxs("tr", { className: h % 2 ? "dx-par" : "", children: [
            /* @__PURE__ */ a.jsxs("td", { children: [
              e.soloLectura ? /* @__PURE__ */ a.jsx("span", { className: "mono", children: c.referencia ?? "" }) : /* @__PURE__ */ a.jsx(
                Zp,
                {
                  texto: c.referencia ?? (c.productoId ? c.descripcion : ""),
                  alCambiarTexto: (f) => o(c.clave, { referencia: f, ...f === "" ? { productoId: null } : {} }),
                  alElegir: (f) => (e.alElegirArticulo(c.clave, f), u(h, 2)),
                  precioDe: l ? void 0 : (f) => f.precioCompraPorUnidadCompra ?? f.precioCompra,
                  datos: { f: h, c: 0 }
                }
              ),
              c.productoId && /* @__PURE__ */ a.jsxs("div", { className: "dx-sub", children: [
                c.unidad && /* @__PURE__ */ a.jsx("span", { children: c.unidad }),
                c.controlarStock && /* @__PURE__ */ a.jsxs("span", { className: w ? "dx-rojo" : "", children: [
                  " · stock ",
                  $e(c.stock)
                ] })
              ] })
            ] }),
            /* @__PURE__ */ a.jsxs("td", { children: [
              /* @__PURE__ */ a.jsx(
                "input",
                {
                  "data-f": h,
                  "data-c": 1,
                  value: c.descripcion,
                  placeholder: c.productoId ? "" : "Descripción (línea libre)",
                  disabled: e.soloLectura,
                  onChange: (f) => o(c.clave, { descripcion: f.target.value })
                }
              ),
              y.length > 0 && !n.has(c.clave) && /* @__PURE__ */ a.jsx("button", { type: "button", className: "dx-resumen-conc", onClick: () => N(c.clave), title: "Ver y cambiar los conceptos", children: y.map((f) => `${f.importe < 0 ? "−" : "+"} ${f.codigo.toLowerCase()} ${ke(Math.abs(f.importe))}${f.efecto === "Coste" ? " (coste)" : ""}`).join(" · ") })
            ] }),
            /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx(
              "input",
              {
                "data-f": h,
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
                "data-f": h,
                "data-c": 3,
                className: "num" + (c.precio == null ? " dx-auto" : ""),
                type: "number",
                step: "0.0001",
                disabled: e.soloLectura,
                value: c.precio ?? "",
                placeholder: v ? ke(v.precio) : "",
                title: c.precio == null ? "Precio de la tarifa del cliente o del artículo (escribe uno para fijarlo)" : "",
                onChange: (f) => o(c.clave, { precio: f.target.value === "" ? null : Number(f.target.value) })
              }
            ) }),
            l && /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx(
              "input",
              {
                "data-f": h,
                "data-c": 4,
                className: "num",
                type: "number",
                step: "0.01",
                value: c.dto || (c.precio == null && (v != null && v.dto) ? v.dto : 0),
                disabled: e.soloLectura,
                onChange: (f) => o(c.clave, { dto: Number(f.target.value), precio: c.precio ?? (v == null ? void 0 : v.precio) ?? null })
              }
            ) }),
            l && /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsxs("select", { "data-f": h, "data-c": 5, value: c.iva ?? (v == null ? void 0 : v.iva) ?? "", disabled: e.soloLectura, onChange: (f) => o(c.clave, { iva: f.target.value || null }), children: [
              !c.iva && !(v != null && v.iva) && /* @__PURE__ */ a.jsx("option", { value: "", children: "Del artículo" }),
              e.ivas.map((f) => /* @__PURE__ */ a.jsx("option", { value: f.codigo, children: f.nombre }, f.codigo))
            ] }) }),
            /* @__PURE__ */ a.jsx("td", { className: "num", children: /* @__PURE__ */ a.jsx("strong", { children: v ? D(v.importe) : "—" }) }),
            /* @__PURE__ */ a.jsx("td", { className: "num", children: l ? (v == null ? void 0 : v.margen) != null && /* @__PURE__ */ a.jsxs("span", { className: v.margen < 0 ? "dx-rojo" : "muted", children: [
              D(v.margen),
              _ != null && /* @__PURE__ */ a.jsxs("div", { className: "dx-sub", children: [
                ke(_),
                " %"
              ] })
            ] }) : (v == null ? void 0 : v.costeUnitarioEntrada) != null && /* @__PURE__ */ a.jsxs("span", { className: "muted", children: [
              D(v.costeUnitarioEntrada),
              "/ud"
            ] }) }),
            /* @__PURE__ */ a.jsxs("td", { className: "right", style: { whiteSpace: "nowrap" }, children: [
              !e.soloLectura && e.catalogo.length > 0 && /* @__PURE__ */ a.jsx("button", { type: "button", className: "dx-icono" + (n.has(c.clave) ? " activo" : ""), title: "Conceptos de la línea", onClick: () => N(c.clave), "data-f": h, "data-c": l ? 6 : 4, children: "±" }),
              !e.soloLectura && /* @__PURE__ */ a.jsx("button", { type: "button", className: "dx-icono", title: "Quitar la línea", onClick: () => s(c.clave), children: "✕" })
            ] })
          ] }, c.clave),
          n.has(c.clave) && /* @__PURE__ */ a.jsx("tr", { className: "dx-fila-conc", children: /* @__PURE__ */ a.jsx("td", { colSpan: l ? 9 : 7, children: /* @__PURE__ */ a.jsx(zo, { catalogo: e.catalogo, lista: c.conceptos, sugeridos: e.sugeridos[c.clave], alCambiar: (f) => o(c.clave, { conceptos: f }) }) }) }, c.clave + "c")
        ];
      }) })
    ] }),
    !e.soloLectura && /* @__PURE__ */ a.jsx("button", { type: "button", className: "btn small ghost", style: { marginTop: 8 }, onClick: () => (e.alCambiar([...e.lineas, wr()]), setTimeout(() => u(e.lineas.length, 0), 30)), children: "+ Añadir línea" }),
    !e.soloLectura && /* @__PURE__ */ a.jsx("span", { className: "muted dx-ayuda", children: "Intro: siguiente casilla · ↑↓: cambiar de línea · F2: buscar artículo · el precio en gris es el de tarifa" })
  ] });
}
const em = { presupuesto: "presupuesto", pedido: "pedido de venta", factura: "factura" };
function dd(e) {
  const t = /* @__PURE__ */ new Map();
  for (const n of e)
    for (const r of n.conceptos ?? [])
      if (r.repartido) {
        const l = t.get(r.conceptoId);
        t.set(r.conceptoId, { conceptoId: r.conceptoId, valor: r.calculo === "Importe" ? be(((l == null ? void 0 : l.valor) ?? 0) + r.valor) : r.valor });
      }
  return {
    porLinea: e.map((n) => (n.conceptos ?? []).filter((r) => !r.repartido).map((r) => ({ conceptoId: r.conceptoId, valor: r.valor }))),
    documento: [...t.values()]
  };
}
function tm(e) {
  var P, B, b, Z;
  const { api: t, anfitrion: n } = Ct(), r = !!((P = e.semilla) != null && P.rectificaId), [l, i] = x.useState([]), [o, s] = x.useState([]), [u, d] = x.useState([]), [N, c] = x.useState([]), [h, v] = x.useState([]), [y, w] = x.useState(((B = e.semilla) == null ? void 0 : B.clienteId) ?? ""), [_, f] = x.useState(e.tipo === "pedido" && ((b = e.semilla) != null && b.fecha) ? e.semilla.fecha : xt()), [p, g] = x.useState(""), [k, T] = x.useState(""), [R, z] = x.useState(0), [M, A] = x.useState(!1), [U, F] = x.useState(null), [Q, oe] = x.useState(30), [Fe, De] = x.useState(""), [de, ge] = x.useState([wr()]), [C, j] = x.useState([]), [S, V] = x.useState(null), [W, K] = x.useState(""), [fe, pe] = x.useState(!1), [J, O] = x.useState(!1), [st, Ke] = x.useState(!1), ln = Gr();
  x.useEffect(() => {
    t.get("/clientes").then(i).catch(() => i([])), t.get("/tipos-iva").then((E) => s(E.filter((H) => H.activo))).catch(() => s([])), t.get("/formas-pago").then((E) => d(E.filter((H) => H.activo))).catch(() => d([])), t.get("/series").then((E) => c([...new Set(E.filter((H) => H.tipoDocumento === "Factura").map((H) => H.prefijo))])).catch(() => c([])), t.get("/conceptos-linea?ambito=Ventas&activos=true").then(v).catch(() => v([]));
  }, [t]), x.useEffect(() => {
    const E = e.semilla;
    if (!E || !E.lineas.length) return;
    const { porLinea: H, documento: ae } = dd(E.lineas), me = E.lineas.map((te, fa) => ({
      clave: Qr(),
      productoId: te.productoId ?? null,
      descripcion: te.descripcion,
      cantidad: te.cantidad,
      precio: te.precioUnitario,
      dto: te.porcentajeDescuento,
      iva: te.codigoIva,
      conceptos: r ? [] : H[fa]
    }));
    ge(me), j(r ? [] : ae), Promise.all(me.map((te) => te.productoId ? t.get(`/productos/${te.productoId}`).catch(() => null) : Promise.resolve(null))).then(
      (te) => ge((fa) => fa.map((Lo, kn) => te[kn] ? { ...Lo, referencia: te[kn].referencia ?? te[kn].nombre, unidad: te[kn].unidad, stock: te[kn].stock, controlarStock: te[kn].controlarStock } : Lo))
    );
  }, [e.semilla, t, r]);
  const ee = l.find((E) => E.id === y);
  x.useEffect(() => {
    ee && (A(!!ee.recargoEquivalencia), ee.formaPagoDefectoId && T(ee.formaPagoDefectoId));
  }, [ee]);
  const ut = x.useMemo(() => de.map((E, H) => ({ l: E, i: H })).filter(({ l: E }) => (E.productoId || E.descripcion.trim()) && E.cantidad > 0), [de]), an = x.useMemo(
    () => ({
      clienteId: y,
      fechaEmision: e.tipo === "factura" ? _ : null,
      serie: p || null,
      diasVencimiento: R,
      formaPagoId: k || null,
      recargoEquivalencia: M,
      porcentajeIrpf: U,
      conceptosDocumento: C,
      lineas: ut.map(({ l: E }) => ({
        cantidad: E.cantidad,
        descripcion: E.descripcion.trim() || null,
        precioUnitario: E.precio,
        codigoIva: E.iva,
        porcentajeDescuento: E.dto,
        productoId: E.productoId,
        ...r ? { conceptos: [] } : E.conceptos === void 0 ? {} : { conceptos: E.conceptos }
      }))
    }),
    [y, _, p, R, k, M, U, C, ut, e.tipo, r]
  ), X = hn(an, 350);
  x.useEffect(() => {
    if (!X.clienteId || X.lineas.length === 0) {
      V(null), K(X.clienteId ? "" : "Elige el cliente para calcular precios e importes.");
      return;
    }
    const E = ln();
    pe(!0), t.post("/facturas/simular", X).then((H) => E() && (V(H), K(""))).catch((H) => E() && (V(null), K(H.message))).finally(() => E() && pe(!1));
  }, [X, t]);
  const Ne = x.useMemo(() => {
    const E = de.map(() => {
    });
    return S && ut.forEach(({ i: H }, ae) => {
      const me = S.lineas[ae];
      me && (E[H] = { precio: me.precioUnitario, dto: me.porcentajeDescuento, iva: me.codigoIva, importe: me.base, margen: me.productoId || me.costeUnitario || me.costeConceptos ? me.margen : void 0, conceptos: me.conceptos });
    }), E;
  }, [S, de, ut]), ct = x.useMemo(() => {
    const E = {};
    return de.forEach((H, ae) => {
      var me;
      return E[H.clave] = (((me = Ne[ae]) == null ? void 0 : me.conceptos) ?? []).filter((te) => !te.repartido).map((te) => ({ conceptoId: te.conceptoId, valor: te.valor }));
    }), E;
  }, [de, Ne]);
  function tt(E, H) {
    ge(
      (ae) => ae.map(
        (me) => me.clave === E ? { ...me, productoId: H.id, referencia: H.referencia ?? H.nombre, descripcion: H.nombre, precio: null, dto: 0, iva: null, conceptos: void 0, unidad: H.unidad, stock: H.stock, controlarStock: H.controlarStock } : me
      )
    );
  }
  const Mt = x.useMemo(() => {
    const E = /* @__PURE__ */ new Map();
    for (const H of (S == null ? void 0 : S.lineas) ?? []) {
      const ae = E.get(H.codigoIva) ?? { base: 0, cuota: 0, pct: H.porcentajeIva };
      ae.base += H.base, ae.cuota += H.cuotaIva, E.set(H.codigoIva, ae);
    }
    return [...E.entries()];
  }, [S]), on = ((S == null ? void 0 : S.lineas) ?? []).reduce((E, H) => E + (H.base - H.margen), 0), Ve = S ? S.baseImponible - on : 0, er = (E) => {
    var H;
    return ((H = o.find((ae) => ae.codigo === E)) == null ? void 0 : H.nombre) ?? E;
  };
  async function $t() {
    if (S) {
      O(!0);
      try {
        const E = ut.map(({ l: ae }, me) => {
          const te = S.lineas[me];
          return {
            cantidad: ae.cantidad,
            descripcion: te.descripcion,
            precioUnitario: te.precioUnitario,
            codigoIva: te.codigoIva,
            porcentajeDescuento: te.porcentajeDescuento,
            productoId: ae.productoId,
            ...r ? {} : ae.conceptos === void 0 ? {} : { conceptos: ae.conceptos }
          };
        });
        let H;
        if (r)
          H = (await t.post(`/facturas/${e.semilla.rectificaId}/rectificar`, { motivo: Fe, lineas: E, fechaEmision: _, porcentajeIrpf: U, serie: p || null })).id;
        else if (e.tipo === "factura")
          H = (await t.post("/facturas", { ...an, lineas: E })).id;
        else if (e.tipo === "presupuesto") {
          const ae = { clienteId: y, diasValidez: Q, lineas: E, conceptosDocumento: C };
          H = e.id ? (await t.put(`/presupuestos/${e.id}`, ae)).id : (await t.post("/presupuestos", ae)).id;
        } else {
          const ae = { clienteId: y, fecha: _, lineas: E, conceptosDocumento: C };
          H = e.id ? (await t.put(`/pedidos-venta/${e.id}`, ae)).id : (await t.post("/pedidos-venta", ae)).id;
        }
        n.aviso(e.tipo === "factura" ? "Factura emitida." : "Documento guardado.", "ok"), e.alGuardar(H);
      } catch (E) {
        n.aviso(E.message, "err");
      } finally {
        O(!1), Ke(!1);
      }
    }
  }
  const m = r ? `Rectificativa de la factura ${((Z = e.semilla) == null ? void 0 : Z.rectificaNumero) ?? ""}` : `${e.id ? "Editar" : "Nuevo"} ${em[e.tipo]}`.replace("Nuevo factura", "Nueva factura"), L = !!S && !fe && (!r || Fe.trim().length > 0);
  return /* @__PURE__ */ a.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsxs("div", { className: "panel-head", children: [
        /* @__PURE__ */ a.jsx("h2", { children: m }),
        /* @__PURE__ */ a.jsxs("div", { style: { display: "flex", gap: 8 }, children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: e.alCancelar, children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", disabled: !L || J, onClick: () => e.tipo === "factura" ? Ke(!0) : $t(), children: e.tipo === "factura" ? "Emitir factura" : "Guardar" })
        ] })
      ] }),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-cabecera", children: [
        /* @__PURE__ */ a.jsxs("div", { className: "dx-cab-campos", children: [
          /* @__PURE__ */ a.jsx(To, { terceros: l, valor: y, alCambiar: w, etiqueta: "Cliente", deshabilitado: r || e.tipo === "pedido" && !!e.id && !1 }),
          /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", children: [
            e.tipo !== "presupuesto" && /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: e.tipo === "factura" ? "Fecha de emisión" : "Fecha del pedido" }),
              /* @__PURE__ */ a.jsx("input", { type: "date", value: _, onChange: (E) => f(E.target.value) })
            ] }),
            e.tipo === "presupuesto" && /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Validez" }),
              /* @__PURE__ */ a.jsx("select", { value: Q, onChange: (E) => oe(Number(E.target.value)), children: [15, 30, 60, 90].map((E) => /* @__PURE__ */ a.jsxs("option", { value: E, children: [
                E,
                " días"
              ] }, E)) })
            ] }),
            e.tipo === "factura" && N.length > 0 && /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Serie" }),
              /* @__PURE__ */ a.jsxs("select", { value: p, onChange: (E) => g(E.target.value), children: [
                /* @__PURE__ */ a.jsx("option", { value: "", children: "La del cliente" }),
                N.map((E) => /* @__PURE__ */ a.jsx("option", { value: E, children: E }, E))
              ] })
            ] }),
            e.tipo === "factura" && !r && /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
              /* @__PURE__ */ a.jsxs("div", { children: [
                /* @__PURE__ */ a.jsx("label", { children: "Forma de pago" }),
                /* @__PURE__ */ a.jsxs("select", { value: k, onChange: (E) => T(E.target.value), children: [
                  /* @__PURE__ */ a.jsx("option", { value: "", children: "— Vencimiento a mano —" }),
                  u.map((E) => /* @__PURE__ */ a.jsx("option", { value: E.id, children: E.nombre }, E.id))
                ] })
              ] }),
              !k && /* @__PURE__ */ a.jsxs("div", { children: [
                /* @__PURE__ */ a.jsx("label", { children: "Vencimiento" }),
                /* @__PURE__ */ a.jsx("select", { value: R, onChange: (E) => z(Number(E.target.value)), children: [0, 15, 30, 45, 60, 90].map((E) => /* @__PURE__ */ a.jsx("option", { value: E, children: E ? `${E} días` : "Contado" }, E)) })
              ] })
            ] }),
            e.tipo === "factura" && /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Retención IRPF %" }),
              /* @__PURE__ */ a.jsx("input", { type: "number", step: "0.01", value: U ?? "", placeholder: String((ee == null ? void 0 : ee.porcentajeIrpfDefecto) ?? 0), onChange: (E) => F(E.target.value === "" ? null : Number(E.target.value)) })
            ] })
          ] }),
          e.tipo === "factura" && !r && /* @__PURE__ */ a.jsxs("label", { className: "dx-check", children: [
            /* @__PURE__ */ a.jsx("input", { type: "checkbox", checked: M, onChange: (E) => A(E.target.checked) }),
            " Recargo de equivalencia"
          ] }),
          r && /* @__PURE__ */ a.jsxs("div", { children: [
            /* @__PURE__ */ a.jsx("label", { children: "Motivo de la rectificación (obligatorio)" }),
            /* @__PURE__ */ a.jsx("input", { value: Fe, onChange: (E) => De(E.target.value), placeholder: "Devolución, error en precio…" })
          ] })
        ] }),
        /* @__PURE__ */ a.jsx("div", { className: "dx-ficha", children: ee ? /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("strong", { children: ee.nombre }),
          /* @__PURE__ */ a.jsx("div", { className: "muted", children: [ee.nifFiscal, ee.poblacion, ee.provincia].filter(Boolean).join(" · ") }),
          ee.limiteRiesgo != null && /* @__PURE__ */ a.jsxs("div", { className: "muted", children: [
            "Límite de riesgo ",
            D(ee.limiteRiesgo)
          ] }),
          ee.tarifaId && /* @__PURE__ */ a.jsx("div", { className: "muted", children: "Con tarifa de precios propia" }),
          ee.recargoEquivalencia && /* @__PURE__ */ a.jsx("div", { className: "muted", children: "En recargo de equivalencia" }),
          (S == null ? void 0 : S.avisoRiesgo) && /* @__PURE__ */ a.jsxs("div", { className: "dx-aviso", children: [
            "⚠ ",
            S.avisoRiesgo
          ] })
        ] }) : /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Elige un cliente: se aplican su tarifa, su forma de pago, su recargo y sus conceptos." }) })
      ] })
    ] }),
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsx(cd, { modo: "venta", lineas: de, alCambiar: ge, calculos: Ne, ivas: o, catalogo: r ? [] : h, sugeridos: ct, alElegirArticulo: tt }),
      !r && h.length > 0 && /* @__PURE__ */ a.jsx("div", { style: { marginTop: 10 }, children: /* @__PURE__ */ a.jsx(zo, { catalogo: h, lista: C, alCambiar: (E) => j(E ?? []), documento: !0 }) })
    ] }),
    /* @__PURE__ */ a.jsxs("div", { className: "dx-pie", children: [
      /* @__PURE__ */ a.jsxs("div", { className: "dx-estado", children: [
        fe && /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Calculando…" }),
        !fe && W && /* @__PURE__ */ a.jsx("span", { className: "dx-rojo", children: W }),
        (S == null ? void 0 : S.mencionFiscal) && /* @__PURE__ */ a.jsx("div", { className: "muted", style: { fontSize: 12 }, children: S.mencionFiscal })
      ] }),
      /* @__PURE__ */ a.jsxs("div", { className: "panel dx-totales", children: [
        Mt.map(([E, H]) => /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsxs("span", { className: "muted", children: [
            er(E),
            " · base ",
            ke(H.base)
          ] }),
          /* @__PURE__ */ a.jsx("span", { children: D(H.cuota) })
        ] }, E)),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Base imponible" }),
          /* @__PURE__ */ a.jsx("span", { children: D(S == null ? void 0 : S.baseImponible) })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Impuestos" }),
          /* @__PURE__ */ a.jsx("span", { children: D(S == null ? void 0 : S.cuotaIva) })
        ] }),
        !!(S != null && S.recargoTotal) && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Recargo de equivalencia" }),
          /* @__PURE__ */ a.jsx("span", { children: D(S.recargoTotal) })
        ] }),
        !!(S != null && S.retencionIrpf) && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsxs("span", { className: "muted", children: [
            "Retención IRPF (",
            ke(S.porcentajeIrpf),
            " %)"
          ] }),
          /* @__PURE__ */ a.jsxs("span", { children: [
            "−",
            D(S.retencionIrpf)
          ] })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ a.jsx("span", { children: "Total" }),
          /* @__PURE__ */ a.jsx("span", { children: D(S == null ? void 0 : S.total) })
        ] }),
        S && on > 0 && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Coste · margen" }),
          /* @__PURE__ */ a.jsxs("span", { className: Ve < 0 ? "dx-rojo" : "muted", children: [
            D(on),
            " · ",
            D(Ve),
            " (",
            ke(S.baseImponible ? Ve / S.baseImponible * 100 : 0),
            " %)"
          ] })
        ] })
      ] })
    ] }),
    st && S && /* @__PURE__ */ a.jsx(
      Nn,
      {
        titulo: r ? "Emitir la rectificativa" : "Emitir la factura",
        alCerrar: () => Ke(!1),
        acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => Ke(!1), children: "Revisar" }),
          /* @__PURE__ */ a.jsxs("button", { className: "btn small", disabled: J, onClick: $t, children: [
            "Emitir ",
            D(S.total)
          ] })
        ] }),
        children: /* @__PURE__ */ a.jsxs("p", { style: { margin: 0 }, children: [
          "Se emitirá una factura ",
          r ? "rectificativa " : "",
          "de ",
          /* @__PURE__ */ a.jsx("strong", { children: D(S.total) }),
          " a ",
          /* @__PURE__ */ a.jsx("strong", { children: ee == null ? void 0 : ee.nombre }),
          " con fecha ",
          _.split("-").reverse().join("/"),
          ". Una factura emitida no se modifica: queda numerada y encadenada en VeriFactu; para corregirla hay que rectificarla o anularla."
        ] })
      }
    )
  ] });
}
function nm(e) {
  var De, de, ge;
  const { api: t, anfitrion: n } = Ct(), [r, l] = x.useState([]), [i, o] = x.useState([]), [s, u] = x.useState(((De = e.semilla) == null ? void 0 : De.proveedorId) ?? ""), [d, N] = x.useState(((de = e.semilla) == null ? void 0 : de.fecha) ?? xt()), [c, h] = x.useState([wr()]), [v, y] = x.useState([]), [w, _] = x.useState(null), [f, p] = x.useState(""), [g, k] = x.useState(!1), T = Gr();
  x.useEffect(() => {
    t.get("/proveedores").then(l).catch(() => l([])), t.get("/conceptos-linea?ambito=Compras&activos=true").then(o).catch(() => o([]));
  }, [t]), x.useEffect(() => {
    const C = e.semilla;
    if (!C) return;
    const j = C.lineas.map((K) => ({ ...K, porcentajeDescuento: 0, codigoIva: "" })), { porLinea: S, documento: V } = dd(j), W = C.lineas.map((K, fe) => ({ clave: Qr(), productoId: K.productoId ?? null, descripcion: K.descripcion, cantidad: K.cantidad, precio: K.precioUnitario, dto: 0, iva: null, conceptos: S[fe] }));
    h(W), y(V), Promise.all(W.map((K) => K.productoId ? t.get(`/productos/${K.productoId}`).catch(() => null) : Promise.resolve(null))).then(
      (K) => h((fe) => fe.map((pe, J) => K[J] ? { ...pe, referencia: K[J].referencia ?? K[J].nombre, unidad: K[J].unidadCompra || K[J].unidad, stock: K[J].stock, controlarStock: K[J].controlarStock } : pe))
    );
  }, [e.semilla, t]);
  const R = r.find((C) => C.id === s), z = x.useMemo(() => c.map((C, j) => ({ l: C, i: j })).filter(({ l: C }) => C.descripcion.trim() && C.cantidad > 0), [c]), M = x.useMemo(
    () => {
      var C, j;
      return {
        proveedorId: s || null,
        proveedorTexto: (R == null ? void 0 : R.nombre) ?? (((C = e.semilla) == null ? void 0 : C.proveedorTexto) || null),
        solicitudOrigenId: e.id ? null : ((j = e.semilla) == null ? void 0 : j.solicitudOrigenId) ?? null,
        fecha: d,
        conceptosDocumento: v,
        lineas: z.map(({ l: S }) => ({ descripcion: S.descripcion.trim(), cantidad: S.cantidad, precioUnitario: S.precio ?? 0, productoId: S.productoId, ...S.conceptos === void 0 ? {} : { conceptos: S.conceptos } }))
      };
    },
    [s, R, d, v, z, e.id, e.semilla]
  ), A = hn(M, 350);
  x.useEffect(() => {
    if (!A.proveedorId || A.lineas.length === 0) {
      _(null), p(A.proveedorId ? "" : "Elige el proveedor.");
      return;
    }
    const C = T();
    t.post("/compras/pedidos/simular", A).then((j) => C() && (_(j), p(""))).catch((j) => C() && (_(null), p(j.message)));
  }, [A, t]);
  const U = x.useMemo(() => {
    const C = c.map(() => {
    });
    return z.forEach(({ i: j }, S) => {
      const V = w == null ? void 0 : w.lineas[S];
      V && (C[j] = { precio: V.precioUnitario, importe: V.importe, costeUnitarioEntrada: V.costeUnitarioEntrada, conceptos: V.conceptos });
    }), C;
  }, [w, c, z]), F = x.useMemo(() => {
    const C = {};
    return c.forEach((j, S) => {
      var V;
      return C[j.clave] = (((V = U[S]) == null ? void 0 : V.conceptos) ?? []).filter((W) => !W.repartido).map((W) => ({ conceptoId: W.conceptoId, valor: W.valor }));
    }), C;
  }, [c, U]);
  function Q(C, j) {
    const S = j.precioCompraPorUnidadCompra ?? j.precioCompra;
    h((V) => V.map((W) => W.clave === C ? { ...W, productoId: j.id, referencia: j.referencia ?? j.nombre, descripcion: j.nombre, precio: S, conceptos: void 0, unidad: j.unidadCompra || j.unidad, stock: j.stock, controlarStock: j.controlarStock } : W));
  }
  async function oe() {
    k(!0);
    try {
      const C = e.id ? await t.put(`/compras/pedidos/${e.id}`, M) : await t.post("/compras/pedidos", M);
      n.aviso("Pedido de compra guardado.", "ok"), e.alGuardar(C.id);
    } catch (C) {
      n.aviso(C.message, "err");
    } finally {
      k(!1);
    }
  }
  const Fe = ((w == null ? void 0 : w.lineas) ?? []).reduce((C, j) => C + j.costeConceptos, 0);
  return /* @__PURE__ */ a.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsxs("div", { className: "panel-head", children: [
        /* @__PURE__ */ a.jsx("h2", { children: e.id ? `Editar pedido ${((ge = e.semilla) == null ? void 0 : ge.numeroCompleto) ?? ""}` : "Nuevo pedido de compra" }),
        /* @__PURE__ */ a.jsxs("div", { style: { display: "flex", gap: 8 }, children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: e.alCancelar, children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", disabled: !w || g, onClick: oe, children: "Guardar" })
        ] })
      ] }),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-cabecera", children: [
        /* @__PURE__ */ a.jsxs("div", { className: "dx-cab-campos", children: [
          /* @__PURE__ */ a.jsx(To, { terceros: r, valor: s, alCambiar: u, etiqueta: "Proveedor", deshabilitado: !!e.id }),
          /* @__PURE__ */ a.jsx("div", { className: "dx-fila", children: /* @__PURE__ */ a.jsxs("div", { children: [
            /* @__PURE__ */ a.jsx("label", { children: "Fecha del pedido" }),
            /* @__PURE__ */ a.jsx("input", { type: "date", value: d, onChange: (C) => N(C.target.value) })
          ] }) })
        ] }),
        /* @__PURE__ */ a.jsx("div", { className: "dx-ficha", children: R ? /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("strong", { children: R.nombre }),
          /* @__PURE__ */ a.jsx("div", { className: "muted", children: [R.nifFiscal, R.poblacion, R.pais].filter(Boolean).join(" · ") })
        ] }) : /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Elige un proveedor: se aplican sus conceptos (pronto pago, portes…)." }) })
      ] })
    ] }),
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsx(cd, { modo: "compra", lineas: c, alCambiar: h, calculos: U, ivas: [], catalogo: i, sugeridos: F, alElegirArticulo: Q }),
      i.length > 0 && /* @__PURE__ */ a.jsx("div", { style: { marginTop: 10 }, children: /* @__PURE__ */ a.jsx(zo, { catalogo: i, lista: v, alCambiar: (C) => y(C ?? []), documento: !0 }) })
    ] }),
    /* @__PURE__ */ a.jsxs("div", { className: "dx-pie", children: [
      /* @__PURE__ */ a.jsx("div", { className: "dx-estado", children: f && /* @__PURE__ */ a.jsx("span", { className: "dx-rojo", children: f }) }),
      /* @__PURE__ */ a.jsxs("div", { className: "panel dx-totales", children: [
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ a.jsx("span", { children: "Total del pedido (sin impuestos)" }),
          /* @__PURE__ */ a.jsx("span", { children: D(w == null ? void 0 : w.total) })
        ] }),
        Fe !== 0 && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Costes añadidos al almacén" }),
          /* @__PURE__ */ a.jsx("span", { children: D(Fe) })
        ] }),
        /* @__PURE__ */ a.jsx("div", { className: "muted", style: { fontSize: 12, marginTop: 6 }, children: "El impuesto se aplica al facturar el pedido." })
      ] })
    ] })
  ] });
}
function ca(e) {
  return /* @__PURE__ */ a.jsxs("div", { className: "panel-head", children: [
    /* @__PURE__ */ a.jsxs("h2", { style: { display: "flex", alignItems: "center", gap: 10 }, children: [
      /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: e.volver, title: "Volver a la lista", children: "←" }),
      e.titulo,
      e.estado && /* @__PURE__ */ a.jsx("span", { className: Ro(e.estado), children: e.estado })
    ] }),
    /* @__PURE__ */ a.jsx("div", { className: "dx-acciones", children: e.acciones })
  ] });
}
function ze(e) {
  return /* @__PURE__ */ a.jsxs("div", { className: "dx-dato", children: [
    /* @__PURE__ */ a.jsx("small", { children: e.etiqueta }),
    /* @__PURE__ */ a.jsx("div", { children: e.children })
  ] });
}
function Ur(e) {
  return /* @__PURE__ */ a.jsx("button", { type: "button", className: "dx-enlace", onClick: e.alPulsar, children: e.children });
}
function da(e) {
  const [t, n] = x.useState(null), [r, l] = x.useState(""), i = x.useCallback(() => {
    e().then(n).catch((o) => l(o.message));
  }, []);
  return x.useEffect(i, [i]), { dato: t, error: r, recargar: i };
}
async function ht(e, t, n) {
  try {
    return await e(), n && t(n, "ok"), !0;
  } catch (r) {
    return t(r.message, "err"), !1;
  }
}
function rm(e) {
  const { api: t, anfitrion: n, navegar: r } = Ct(), { dato: l, error: i, recargar: o } = da(() => t.get(`/facturas/${e.id}`)), [s, u] = x.useState(null), [d, N] = x.useState(!1), [c, h] = x.useState("");
  if (x.useEffect(() => void t.get(`/facturas/${e.id}/saldo`).then(u).catch(() => u(null)), [t, e.id, l]), i) return /* @__PURE__ */ a.jsx("div", { className: "panel", children: /* @__PURE__ */ a.jsx("p", { className: "dx-rojo", children: i }) });
  if (!l) return /* @__PURE__ */ a.jsx("div", { className: "muted", children: "Cargando…" });
  const v = { clienteId: l.clienteId ?? void 0, lineas: l.lineas }, y = l.lineas.reduce((_, f) => _ + (f.base - f.margen), 0), w = l.estado === "Emitida";
  return /* @__PURE__ */ a.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsx(
        ca,
        {
          titulo: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            l.tipo === "Rectificativa" ? "Rectificativa" : l.tipo === "Simplificada" ? "Ticket" : "Factura",
            " ",
            /* @__PURE__ */ a.jsx("span", { className: "mono", children: l.numeroCompleto })
          ] }),
          estado: l.estado,
          volver: () => r({ tipo: "factura", pantalla: "lista" }),
          acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => Li(n, `/facturas/${l.id}/pdf`).catch((_) => n.aviso(_.message, "err")), children: "PDF" }),
            l.tipo !== "Simplificada" && l.clienteNif && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => Li(n, `/facturas/${l.id}/facturae.xml`).catch((_) => n.aviso(_.message, "err")), children: "Facturae" }),
            /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "factura", pantalla: "editor", semilla: v }), children: "Duplicar" }),
            w && l.tipo === "Ordinaria" && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "factura", pantalla: "editor", semilla: { ...v, rectificaId: l.id, rectificaNumero: l.numeroCompleto } }), children: "Rectificar" }),
            w && /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", onClick: () => N(!0), children: "Anular" })
          ] })
        }
      ),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ a.jsxs(ze, { etiqueta: "Cliente", children: [
          /* @__PURE__ */ a.jsx("strong", { children: l.clienteNombre }),
          l.clienteNif && /* @__PURE__ */ a.jsx("div", { className: "muted mono", children: l.clienteNif }),
          /* @__PURE__ */ a.jsx("div", { className: "muted", children: [l.clienteCalle, l.clienteCodigoPostal, l.clientePoblacion].filter(Boolean).join(" ") })
        ] }),
        /* @__PURE__ */ a.jsxs(ze, { etiqueta: "Emisión", children: [
          Se(l.fechaEmision),
          l.fechaOperacion !== l.fechaEmision && /* @__PURE__ */ a.jsxs("div", { className: "muted", children: [
            "Operación ",
            Se(l.fechaOperacion)
          ] })
        ] }),
        /* @__PURE__ */ a.jsx(ze, { etiqueta: "Vencimiento", children: Se(l.fechaVencimiento) }),
        /* @__PURE__ */ a.jsxs(ze, { etiqueta: "Cobro", children: [
          s ? s.pendiente <= 0 ? /* @__PURE__ */ a.jsx("span", { className: "pill ok", children: "Cobrada" }) : /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            "Pendiente ",
            /* @__PURE__ */ a.jsx("strong", { children: D(s.pendiente) }),
            s.liquidado > 0 && /* @__PURE__ */ a.jsxs("div", { className: "muted", children: [
              "Cobrado ",
              D(s.liquidado)
            ] })
          ] }) : "—",
          s && s.pendiente > 0 && w && n.irA && /* @__PURE__ */ a.jsx("div", { children: /* @__PURE__ */ a.jsx(Ur, { alPulsar: () => n.irA("cobros"), children: "Registrar cobro" }) })
        ] })
      ] }),
      l.motivoRectificacion && /* @__PURE__ */ a.jsxs("p", { className: "muted", children: [
        "Rectifica: ",
        l.motivoRectificacion,
        l.rectificaFacturaId && /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          " · ",
          /* @__PURE__ */ a.jsx(Ur, { alPulsar: () => r({ tipo: "factura", pantalla: "vista", id: l.rectificaFacturaId }), children: "ver la factura original" })
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
        /* @__PURE__ */ a.jsx("tbody", { children: l.lineas.map((_, f) => /* @__PURE__ */ a.jsxs("tr", { children: [
          /* @__PURE__ */ a.jsxs("td", { children: [
            _.descripcion,
            /* @__PURE__ */ a.jsx(ua, { conceptos: _.conceptos })
          ] }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: $e(_.cantidad) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: D(_.precioUnitario) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: _.porcentajeDescuento ? `${ke(_.porcentajeDescuento)} %` : "" }),
          /* @__PURE__ */ a.jsxs("td", { children: [
            _.codigoIva,
            " · ",
            ke(_.porcentajeIva),
            " %"
          ] }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: /* @__PURE__ */ a.jsx("strong", { children: D(_.base) }) }),
          /* @__PURE__ */ a.jsx("td", { className: "num muted", children: _.costeUnitario || _.costeConceptos ? D(_.margen) : "" })
        ] }, f)) })
      ] }),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-totales-vista", children: [
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Base imponible" }),
          /* @__PURE__ */ a.jsx("span", { children: D(l.baseImponible) })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: l.impuesto === "Igic" ? "IGIC" : "IVA" }),
          /* @__PURE__ */ a.jsx("span", { children: D(l.cuotaIva) })
        ] }),
        !!l.recargoTotal && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Recargo de equivalencia" }),
          /* @__PURE__ */ a.jsx("span", { children: D(l.recargoTotal) })
        ] }),
        !!l.retencionIrpf && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsxs("span", { className: "muted", children: [
            "Retención IRPF (",
            ke(l.porcentajeIrpf),
            " %)"
          ] }),
          /* @__PURE__ */ a.jsxs("span", { children: [
            "−",
            D(l.retencionIrpf)
          ] })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ a.jsx("span", { children: "Total" }),
          /* @__PURE__ */ a.jsx("span", { children: D(l.total) })
        ] }),
        y > 0 && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Margen" }),
          /* @__PURE__ */ a.jsxs("span", { className: "muted", children: [
            D(l.baseImponible - y),
            " (",
            ke(l.baseImponible ? (l.baseImponible - y) / l.baseImponible * 100 : 0),
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
      Nn,
      {
        titulo: `Anular ${l.numeroCompleto}`,
        alCerrar: () => N(!1),
        acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => N(!1), children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", disabled: !c.trim(), onClick: async () => await ht(() => t.post(`/facturas/${l.id}/anular`, { motivo: c }), n.aviso, "Factura anulada.") && (N(!1), o()), children: "Anular" })
        ] }),
        children: [
          /* @__PURE__ */ a.jsx("p", { className: "muted", style: { marginTop: 0 }, children: "La factura no se borra: queda anulada con un registro de anulación VeriFactu. Si hay que corregir importes, rectifícala en lugar de anularla." }),
          /* @__PURE__ */ a.jsx("label", { children: "Motivo" }),
          /* @__PURE__ */ a.jsx("input", { value: c, onChange: (_) => h(_.target.value), autoFocus: !0 })
        ]
      }
    )
  ] });
}
function lm(e) {
  const { api: t, anfitrion: n, navegar: r } = Ct(), { dato: l, error: i, recargar: o } = da(() => t.get(`/presupuestos/${e.id}`));
  if (i) return /* @__PURE__ */ a.jsx("div", { className: "panel", children: /* @__PURE__ */ a.jsx("p", { className: "dx-rojo", children: i }) });
  if (!l) return /* @__PURE__ */ a.jsx("div", { className: "muted", children: "Cargando…" });
  const s = l.estado === "Borrador", u = { clienteId: l.clienteId, lineas: l.lineas };
  return /* @__PURE__ */ a.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsx(
        ca,
        {
          titulo: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            "Presupuesto ",
            /* @__PURE__ */ a.jsx("span", { className: "mono", children: l.numeroCompleto })
          ] }),
          estado: l.estado,
          volver: () => r({ tipo: "presupuesto", pantalla: "lista" }),
          acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => Li(n, `/presupuestos/${l.id}/pdf`).catch((d) => n.aviso(d.message, "err")), children: "PDF" }),
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
            s && /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", onClick: async () => await ht(() => t.post(`/presupuestos/${l.id}/rechazar`, {}), n.aviso, "Presupuesto rechazado.") && o(), children: "Rechazar" })
          ] })
        }
      ),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ a.jsx(ze, { etiqueta: "Cliente", children: /* @__PURE__ */ a.jsx("strong", { children: l.clienteNombre }) }),
        /* @__PURE__ */ a.jsx(ze, { etiqueta: "Fecha", children: Se(l.fecha) }),
        /* @__PURE__ */ a.jsx(ze, { etiqueta: "Válido hasta", children: Se(l.validez) }),
        /* @__PURE__ */ a.jsx(ze, { etiqueta: "Factura", children: l.facturaId ? /* @__PURE__ */ a.jsx(Ur, { alPulsar: () => r({ tipo: "factura", pantalla: "vista", id: l.facturaId }), children: "ver la factura" }) : "—" })
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
        /* @__PURE__ */ a.jsx("tbody", { children: l.lineas.map((d, N) => /* @__PURE__ */ a.jsxs("tr", { children: [
          /* @__PURE__ */ a.jsxs("td", { children: [
            d.descripcion,
            /* @__PURE__ */ a.jsx(ua, { conceptos: d.conceptos })
          ] }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: $e(d.cantidad) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: D(d.precioUnitario) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: d.porcentajeDescuento ? `${ke(d.porcentajeDescuento)} %` : "" }),
          /* @__PURE__ */ a.jsx("td", { children: d.codigoIva }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: /* @__PURE__ */ a.jsx("strong", { children: D(d.base) }) })
        ] }, N)) })
      ] }),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-totales-vista", children: [
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Base imponible" }),
          /* @__PURE__ */ a.jsx("span", { children: D(l.baseImponible) })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Impuestos" }),
          /* @__PURE__ */ a.jsx("span", { children: D(l.cuotaIva) })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ a.jsx("span", { children: "Total" }),
          /* @__PURE__ */ a.jsx("span", { children: D(l.total) })
        ] })
      ] })
    ] })
  ] });
}
function am(e) {
  const { api: t, anfitrion: n, navegar: r } = Ct(), { dato: l, error: i, recargar: o } = da(() => t.get(`/pedidos-venta/${e.id}`)), [s, u] = x.useState([]), [d, N] = x.useState([]), [c, h] = x.useState(null), [v, y] = x.useState(""), [w, _] = x.useState(xt()), [f, p] = x.useState(!1), [g, k] = x.useState(xt()), [T, R] = x.useState("");
  if (x.useEffect(() => void t.get(`/pedidos-venta/${e.id}/albaranes`).then(u).catch(() => u([])), [t, e.id, l]), x.useEffect(() => void t.get("/formas-pago").then((F) => N(F.filter((Q) => Q.activo))).catch(() => N([])), [t]), i) return /* @__PURE__ */ a.jsx("div", { className: "panel", children: /* @__PURE__ */ a.jsx("p", { className: "dx-rojo", children: i }) });
  if (!l) return /* @__PURE__ */ a.jsx("div", { className: "muted", children: "Cargando…" });
  const z = (l.estado === "Borrador" || l.estado === "Confirmado") && l.lineas.every((F) => F.cantidadServida === 0), M = l.lineas.some((F) => F.pendienteServir > 0), A = l.estado !== "Cancelado" && l.estado !== "Facturado", U = { clienteId: l.clienteId, fecha: l.fecha, lineas: l.lineas };
  return /* @__PURE__ */ a.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsx(
        ca,
        {
          titulo: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            "Pedido de venta ",
            /* @__PURE__ */ a.jsx("span", { className: "mono", children: l.numeroCompleto })
          ] }),
          estado: l.estado,
          volver: () => r({ tipo: "pedido", pantalla: "lista" }),
          acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "pedido", pantalla: "editor", semilla: { ...U, fecha: void 0 } }), children: "Duplicar" }),
            z && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "pedido", pantalla: "editor", id: l.id, semilla: U }), children: "Editar" }),
            l.estado === "Borrador" && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: async () => await ht(() => t.post(`/pedidos-venta/${l.id}/confirmar`), n.aviso, "Pedido confirmado.") && o(), children: "Confirmar" }),
            A && l.estado !== "Borrador" && M && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => h(Object.fromEntries(l.lineas.map((F) => [F.id, F.pendienteServir]))), children: "Entregar (albarán)" }),
            A && l.estado !== "Borrador" && /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: () => p(!0), children: "Facturar" }),
            A && /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", onClick: async () => window.confirm("¿Cancelar el pedido?") && await ht(() => t.post(`/pedidos-venta/${l.id}/cancelar`), n.aviso, "Pedido cancelado.") && o(), children: "Cancelar" })
          ] })
        }
      ),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ a.jsx(ze, { etiqueta: "Cliente", children: /* @__PURE__ */ a.jsx("strong", { children: l.clienteNombre }) }),
        /* @__PURE__ */ a.jsx(ze, { etiqueta: "Fecha", children: Se(l.fecha) }),
        /* @__PURE__ */ a.jsx(ze, { etiqueta: "Viene de", children: l.presupuestoOrigenId ? /* @__PURE__ */ a.jsx(Ur, { alPulsar: () => r({ tipo: "presupuesto", pantalla: "vista", id: l.presupuestoOrigenId }), children: "presupuesto" }) : "—" }),
        /* @__PURE__ */ a.jsx(ze, { etiqueta: "Factura", children: l.facturaId ? /* @__PURE__ */ a.jsx(Ur, { alPulsar: () => r({ tipo: "factura", pantalla: "vista", id: l.facturaId }), children: "ver la factura" }) : "—" })
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
        /* @__PURE__ */ a.jsx("tbody", { children: l.lineas.map((F) => /* @__PURE__ */ a.jsxs("tr", { children: [
          /* @__PURE__ */ a.jsxs("td", { children: [
            F.descripcion,
            /* @__PURE__ */ a.jsx(ua, { conceptos: F.conceptos })
          ] }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: $e(F.cantidad) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: $e(F.cantidadServida) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: F.pendienteServir > 0 ? /* @__PURE__ */ a.jsx("strong", { children: $e(F.pendienteServir) }) : "—" }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: D(F.precioUnitario) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: F.porcentajeDescuento ? `${ke(F.porcentajeDescuento)} %` : "" }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: /* @__PURE__ */ a.jsx("strong", { children: D(F.base) }) })
        ] }, F.id)) })
      ] }),
      /* @__PURE__ */ a.jsx("div", { className: "dx-totales-vista", children: /* @__PURE__ */ a.jsxs("div", { className: "dx-tot dx-grande", children: [
        /* @__PURE__ */ a.jsx("span", { children: "Total (sin impuestos)" }),
        /* @__PURE__ */ a.jsx("span", { children: D(l.total) })
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
        /* @__PURE__ */ a.jsx("tbody", { children: s.map((F) => /* @__PURE__ */ a.jsxs("tr", { children: [
          /* @__PURE__ */ a.jsxs("td", { className: "mono", children: [
            /* @__PURE__ */ a.jsx("strong", { children: F.numeroCompleto }),
            " ",
            F.anulado && /* @__PURE__ */ a.jsx("span", { className: "pill neg", title: F.motivoAnulacion ?? "", children: "Anulado" })
          ] }),
          /* @__PURE__ */ a.jsx("td", { children: Se(F.fecha) }),
          /* @__PURE__ */ a.jsx("td", { className: "muted", children: F.referencia }),
          /* @__PURE__ */ a.jsx("td", { className: "muted", children: F.lineas.map((Q) => `${$e(Q.cantidad)} × ${Q.descripcion}`).join(" · ") }),
          /* @__PURE__ */ a.jsx("td", { className: "right", children: !F.anulado && l.estado !== "Facturado" && /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", onClick: async () => {
            const Q = window.prompt("Motivo de la anulación del albarán:");
            Q !== null && await ht(() => t.post(`/pedidos-venta/${l.id}/albaranes/${F.id}/anular`, { motivo: Q || null }), n.aviso, "Albarán anulado.") && o();
          }, children: "Anular" }) })
        ] }, F.id)) })
      ] }) : /* @__PURE__ */ a.jsx("p", { className: "muted", style: { margin: 0 }, children: "Sin entregas todavía." })
    ] }),
    c && /* @__PURE__ */ a.jsxs(
      Nn,
      {
        titulo: "Entrega (albarán de venta)",
        alCerrar: () => h(null),
        ancho: 640,
        acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => h(null), children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: async () => await ht(() => t.post(`/pedidos-venta/${l.id}/entregar`, { fecha: w, referencia: v || null, lineas: Object.entries(c).filter(([, F]) => F > 0).map(([F, Q]) => ({ lineaPedidoId: F, cantidad: Q })) }), n.aviso, "Albarán creado.") && (h(null), o()), children: "Crear albarán" })
        ] }),
        children: [
          /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Fecha" }),
              /* @__PURE__ */ a.jsx("input", { type: "date", value: w, onChange: (F) => _(F.target.value) })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Referencia" }),
              /* @__PURE__ */ a.jsx("input", { value: v, onChange: (F) => y(F.target.value) })
            ] })
          ] }),
          /* @__PURE__ */ a.jsxs("table", { style: { marginTop: 10 }, children: [
            /* @__PURE__ */ a.jsx("thead", { children: /* @__PURE__ */ a.jsxs("tr", { children: [
              /* @__PURE__ */ a.jsx("th", { children: "Línea" }),
              /* @__PURE__ */ a.jsx("th", { className: "num", children: "Pendiente" }),
              /* @__PURE__ */ a.jsx("th", { className: "num", style: { width: 120 }, children: "Entregar" })
            ] }) }),
            /* @__PURE__ */ a.jsx("tbody", { children: l.lineas.filter((F) => F.pendienteServir > 0).map((F) => /* @__PURE__ */ a.jsxs("tr", { children: [
              /* @__PURE__ */ a.jsx("td", { children: F.descripcion }),
              /* @__PURE__ */ a.jsx("td", { className: "num", children: $e(F.pendienteServir) }),
              /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx("input", { className: "num", type: "number", step: "0.001", value: c[F.id] ?? 0, onChange: (Q) => h({ ...c, [F.id]: Number(Q.target.value) }) }) })
            ] }, F.id)) })
          ] })
        ]
      }
    ),
    f && /* @__PURE__ */ a.jsxs(
      Nn,
      {
        titulo: `Facturar el pedido ${l.numeroCompleto}`,
        alCerrar: () => p(!1),
        acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => p(!1), children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: async () => {
            try {
              const F = await t.post(`/pedidos-venta/${l.id}/facturar`, { fechaEmision: g, formaPagoId: T || null });
              n.aviso("Factura emitida.", "ok"), r({ tipo: "factura", pantalla: "vista", id: F.id });
            } catch (F) {
              n.aviso(F.message, "err");
            }
          }, children: "Emitir factura" })
        ] }),
        children: [
          /* @__PURE__ */ a.jsx("p", { className: "muted", style: { marginTop: 0 }, children: "Se factura el pedido completo con sus precios y conceptos. La factura queda numerada y encadenada en VeriFactu." }),
          /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Fecha de emisión" }),
              /* @__PURE__ */ a.jsx("input", { type: "date", value: g, onChange: (F) => k(F.target.value) })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Forma de pago" }),
              /* @__PURE__ */ a.jsxs("select", { value: T, onChange: (F) => R(F.target.value), children: [
                /* @__PURE__ */ a.jsx("option", { value: "", children: "La del cliente" }),
                d.map((F) => /* @__PURE__ */ a.jsx("option", { value: F.id, children: F.nombre }, F.id))
              ] })
            ] })
          ] })
        ]
      }
    )
  ] });
}
function im(e) {
  const { api: t, anfitrion: n, navegar: r } = Ct(), { dato: l, error: i, recargar: o } = da(() => t.get(`/compras/pedidos/${e.id}`)), [s, u] = x.useState([]), [d, N] = x.useState([]), [c, h] = x.useState([]), [v, y] = x.useState(null), [w, _] = x.useState(""), [f, p] = x.useState(""), [g, k] = x.useState(xt()), [T, R] = x.useState(!1), [z, M] = x.useState("IVA21"), [A, U] = x.useState(0), [F, Q] = x.useState(""), [oe, Fe] = x.useState(xt());
  if (x.useEffect(() => void t.get(`/compras/pedidos/${e.id}/albaranes`).then(u).catch(() => u([])), [t, e.id, l]), x.useEffect(() => {
    t.get("/inventario/almacenes").then((j) => (N(j), j[0] && _(j[0].id))).catch(() => N([])), t.get("/tipos-iva").then((j) => h(j.filter((S) => S.activo))).catch(() => h([]));
  }, [t]), i) return /* @__PURE__ */ a.jsx("div", { className: "panel", children: /* @__PURE__ */ a.jsx("p", { className: "dx-rojo", children: i }) });
  if (!l) return /* @__PURE__ */ a.jsx("div", { className: "muted", children: "Cargando…" });
  const De = (l.estado === "Borrador" || l.estado === "Confirmado") && l.lineas.every((j) => j.cantidadRecibida === 0 && j.cantidadFacturada === 0) && !l.empresaOrigenId, de = l.estado !== "Cancelado" && l.estado !== "Facturado", ge = l.lineas.some((j) => j.pendienteRecibir > 0), C = l.lineas.reduce((j, S) => j + S.costeConceptos, 0);
  return /* @__PURE__ */ a.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsx(
        ca,
        {
          titulo: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            "Pedido de compra ",
            /* @__PURE__ */ a.jsx("span", { className: "mono", children: l.numeroCompleto })
          ] }),
          estado: l.estado,
          volver: () => r({ tipo: "compra", pantalla: "lista" }),
          acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            !l.empresaOrigenId && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "compra", pantalla: "editor", semilla: { ...l, fecha: xt() } }), children: "Duplicar" }),
            De && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "compra", pantalla: "editor", id: l.id, semilla: l }), children: "Editar" }),
            l.estado === "Borrador" && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: async () => await ht(() => t.post(`/compras/pedidos/${l.id}/confirmar`), n.aviso, "Pedido confirmado.") && o(), children: "Confirmar" }),
            de && l.estado !== "Borrador" && ge && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => y(Object.fromEntries(l.lineas.map((j) => [j.id, { cantidad: j.pendienteRecibir, lote: "" }]))), children: "Recibir mercancía" }),
            de && l.estado !== "Borrador" && /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: () => R(!0), children: "Facturar" }),
            de && !l.empresaOrigenId && /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", onClick: async () => window.confirm("¿Cancelar el pedido?") && await ht(() => t.post(`/compras/pedidos/${l.id}/cancelar`), n.aviso, "Pedido cancelado.") && o(), children: "Cancelar" })
          ] })
        }
      ),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ a.jsx(ze, { etiqueta: "Proveedor", children: /* @__PURE__ */ a.jsx("strong", { children: l.proveedorTexto }) }),
        /* @__PURE__ */ a.jsx(ze, { etiqueta: "Fecha", children: Se(l.fecha) }),
        /* @__PURE__ */ a.jsx(ze, { etiqueta: "Total", children: D(l.total) }),
        /* @__PURE__ */ a.jsx(ze, { etiqueta: "Costes añadidos", children: C ? D(C) : "—" })
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
      /* @__PURE__ */ a.jsx("tbody", { children: l.lineas.map((j) => /* @__PURE__ */ a.jsxs("tr", { children: [
        /* @__PURE__ */ a.jsxs("td", { children: [
          j.descripcion,
          /* @__PURE__ */ a.jsx(ua, { conceptos: j.conceptos })
        ] }),
        /* @__PURE__ */ a.jsx("td", { className: "num", children: $e(j.cantidad) }),
        /* @__PURE__ */ a.jsx("td", { className: "num", children: $e(j.cantidadRecibida) }),
        /* @__PURE__ */ a.jsx("td", { className: "num", children: j.pendienteRecibir > 0 ? /* @__PURE__ */ a.jsx("strong", { children: $e(j.pendienteRecibir) }) : "—" }),
        /* @__PURE__ */ a.jsx("td", { className: "num", children: D(j.precioUnitario) }),
        /* @__PURE__ */ a.jsx("td", { className: "num", children: /* @__PURE__ */ a.jsx("strong", { children: D(j.importe) }) }),
        /* @__PURE__ */ a.jsxs("td", { className: "num muted", children: [
          D(j.costeUnitarioEntrada),
          "/ud"
        ] })
      ] }, j.id)) })
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
        /* @__PURE__ */ a.jsx("tbody", { children: s.map((j) => {
          var S;
          return /* @__PURE__ */ a.jsxs("tr", { children: [
            /* @__PURE__ */ a.jsxs("td", { className: "mono", children: [
              /* @__PURE__ */ a.jsx("strong", { children: j.numeroCompleto }),
              " ",
              j.anulado && /* @__PURE__ */ a.jsx("span", { className: "pill neg", title: j.motivoAnulacion ?? "", children: "Anulado" })
            ] }),
            /* @__PURE__ */ a.jsx("td", { children: Se(j.fecha) }),
            /* @__PURE__ */ a.jsx("td", { className: "muted", children: j.referencia }),
            /* @__PURE__ */ a.jsx("td", { className: "muted", children: ((S = d.find((V) => V.id === j.almacenId)) == null ? void 0 : S.nombre) ?? "—" }),
            /* @__PURE__ */ a.jsx("td", { className: "muted", children: j.lineas.map((V) => `${$e(V.cantidad)} × ${V.descripcion}`).join(" · ") }),
            /* @__PURE__ */ a.jsx("td", { className: "right", children: !j.anulado && l.estado !== "Facturado" && /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", onClick: async () => {
              const V = window.prompt("Motivo de la anulación del albarán:");
              V !== null && await ht(() => t.post(`/compras/pedidos/${l.id}/albaranes/${j.id}/anular`, { motivo: V || null }), n.aviso, "Albarán anulado.") && o();
            }, children: "Anular" }) })
          ] }, j.id);
        }) })
      ] }) : /* @__PURE__ */ a.jsx("p", { className: "muted", style: { margin: 0 }, children: "Sin recepciones todavía." })
    ] }),
    v && /* @__PURE__ */ a.jsxs(
      Nn,
      {
        titulo: "Recepción de mercancía",
        alCerrar: () => y(null),
        ancho: 680,
        acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => y(null), children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: async () => await ht(() => t.post(`/compras/pedidos/${l.id}/recibir`, { fecha: g, referencia: f || null, almacenId: w || null, lineas: Object.entries(v).filter(([, j]) => j.cantidad > 0).map(([j, S]) => ({ lineaPedidoId: j, cantidad: S.cantidad, lote: S.lote || null })) }), n.aviso, "Recepción registrada.") && (y(null), o()), children: "Registrar recepción" })
        ] }),
        children: [
          /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Fecha" }),
              /* @__PURE__ */ a.jsx("input", { type: "date", value: g, onChange: (j) => k(j.target.value) })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Albarán del proveedor" }),
              /* @__PURE__ */ a.jsx("input", { value: f, onChange: (j) => p(j.target.value) })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Almacén" }),
              /* @__PURE__ */ a.jsxs("select", { value: w, onChange: (j) => _(j.target.value), children: [
                /* @__PURE__ */ a.jsx("option", { value: "", children: "Sin entrada en almacén" }),
                d.map((j) => /* @__PURE__ */ a.jsx("option", { value: j.id, children: j.nombre }, j.id))
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
            /* @__PURE__ */ a.jsx("tbody", { children: l.lineas.filter((j) => j.pendienteRecibir > 0).map((j) => {
              var S, V;
              return /* @__PURE__ */ a.jsxs("tr", { children: [
                /* @__PURE__ */ a.jsx("td", { children: j.descripcion }),
                /* @__PURE__ */ a.jsx("td", { className: "num", children: $e(j.pendienteRecibir) }),
                /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx("input", { className: "num", type: "number", step: "0.001", value: ((S = v[j.id]) == null ? void 0 : S.cantidad) ?? 0, onChange: (W) => y({ ...v, [j.id]: { ...v[j.id], cantidad: Number(W.target.value) } }) }) }),
                /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx("input", { value: ((V = v[j.id]) == null ? void 0 : V.lote) ?? "", onChange: (W) => y({ ...v, [j.id]: { ...v[j.id], lote: W.target.value } }) }) })
              ] }, j.id);
            }) })
          ] })
        ]
      }
    ),
    T && /* @__PURE__ */ a.jsxs(
      Nn,
      {
        titulo: `Facturar el pedido ${l.numeroCompleto}`,
        alCerrar: () => R(!1),
        acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => R(!1), children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: async () => await ht(() => t.post(`/compras/pedidos/${l.id}/facturar`, { codigoIva: z, porcentajeIrpf: A, numeroFactura: F || null, fechaFactura: oe }), n.aviso, "Factura del proveedor registrada como gasto.") && (R(!1), o()), children: "Registrar factura" })
        ] }),
        children: [
          /* @__PURE__ */ a.jsxs("p", { className: "muted", style: { marginTop: 0 }, children: [
            "Registra la factura del proveedor por el total del pedido (",
            D(l.total),
            ") como gasto, con su asiento si la contabilidad es automática."
          ] }),
          /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Nº de factura del proveedor" }),
              /* @__PURE__ */ a.jsx("input", { value: F, onChange: (j) => Q(j.target.value), autoFocus: !0 })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Fecha de la factura" }),
              /* @__PURE__ */ a.jsx("input", { type: "date", value: oe, onChange: (j) => Fe(j.target.value) })
            ] })
          ] }),
          /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Impuesto" }),
              /* @__PURE__ */ a.jsx("select", { value: z, onChange: (j) => M(j.target.value), children: c.map((j) => /* @__PURE__ */ a.jsx("option", { value: j.codigo, children: j.nombre }, j.codigo)) })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Retención IRPF %" }),
              /* @__PURE__ */ a.jsx("input", { type: "number", step: "0.01", value: A, onChange: (j) => U(Number(j.target.value)) })
            ] })
          ] })
        ]
      }
    )
  ] });
}
const Va = (e = "") => ({ clave: Qr(), descripcion: "", cuentaGasto: "", base: 0, codigoIva: e, porcentajeIva: null, porcentajeDeducible: 100 }), om = (e) => e === "InversionSujetoPasivo" || e === "Intracomunitario";
function sm(e) {
  const { api: t, anfitrion: n } = Ct(), r = e.semilla, [l, i] = x.useState([]), [o, s] = x.useState([]), [u, d] = x.useState([]), [N, c] = x.useState([]), [h, v] = x.useState((r == null ? void 0 : r.proveedorId) ?? ""), [y, w] = x.useState(e.id ? (r == null ? void 0 : r.numeroFactura) ?? "" : ""), [_, f] = x.useState((r == null ? void 0 : r.fechaFactura) ?? xt()), [p, g] = x.useState(e.id ? (r == null ? void 0 : r.fecha) ?? xt() : xt()), [k, T] = x.useState(r != null && r.concepto && !r.concepto.startsWith("Factura ") ? r.concepto : ""), [R, z] = x.useState((r == null ? void 0 : r.porcentajeIrpf) ?? 0), [M, A] = x.useState(""), [U, F] = x.useState(((r == null ? void 0 : r.recargoTotal) ?? 0) > 0), Q = !!(r != null && r.esRectificativa), [oe, Fe] = x.useState((r == null ? void 0 : r.numeroRectificado) ?? ""), [De, de] = x.useState((r == null ? void 0 : r.fechaRectificada) ?? ""), [ge, C] = x.useState((r == null ? void 0 : r.motivoRectificacion) ?? ""), [j, S] = x.useState(!1), [V, W] = x.useState((r == null ? void 0 : r.afectacion) ?? "Comun"), [K, fe] = x.useState(
    () => {
      var m;
      return (m = r == null ? void 0 : r.lineas) != null && m.length ? r.lineas.map((L) => ({ clave: Qr(), descripcion: L.descripcion ?? "", cuentaGasto: L.cuentaGasto ?? "", base: L.base, codigoIva: L.codigoIva, porcentajeIva: L.autoliquidada ? L.porcentajeIva : null, porcentajeDeducible: L.porcentajeDeducible })) : [Va()];
    }
  ), [pe, J] = x.useState(e.id && (r != null && r.vencimientos) && r.vencimientos.length > 1 ? r.vencimientos : null), [O, st] = x.useState(null), [Ke, ln] = x.useState(""), [ee, ut] = x.useState(!1), an = Gr();
  x.useEffect(() => {
    t.get("/proveedores").then(i).catch(() => i([])), t.get("/tipos-iva").then((m) => s(m.filter((L) => L.activo))).catch(() => s([])), t.get("/formas-pago").then((m) => d(m.filter((L) => L.activo))).catch(() => d([])), t.get("/empresas/actual").then((m) => {
      m.regimenIva === "RecargoEquivalencia" && (S(!0), r || F(!0));
    }).catch(() => {
    }), t.get("/contabilidad/cuentas").then((m) => c(m.filter((L) => L.codigo.startsWith("6") || L.codigo.startsWith("2")))).catch(() => c([]));
  }, [t]);
  const X = l.find((m) => m.id === h);
  x.useEffect(() => {
    X != null && X.formaPagoDefectoId && !M && A(X.formaPagoDefectoId);
  }, [X]);
  const Ne = x.useMemo(
    () => ({
      proveedorId: h || null,
      proveedorTexto: (X == null ? void 0 : X.nombre) ?? null,
      numeroFactura: y.trim() || null,
      fechaFactura: _ || null,
      fecha: p,
      concepto: k.trim() || null,
      porcentajeIrpf: R,
      formaPagoId: M || null,
      recargoEquivalencia: U,
      afectacion: V,
      baseImponible: 0,
      lineas: K.filter((m) => m.base !== 0).map((m) => ({
        base: m.base,
        codigoIva: m.codigoIva || null,
        descripcion: m.descripcion.trim() || null,
        porcentajeIva: m.porcentajeIva,
        porcentajeDeducible: m.porcentajeDeducible,
        cuentaGasto: m.cuentaGasto.trim() || null
      })),
      vencimientos: pe,
      rectificaGastoId: Q ? (r == null ? void 0 : r.rectificaGastoId) ?? null : null,
      numeroRectificado: Q && oe.trim() || null,
      fechaRectificada: Q && De || null,
      motivoRectificacion: Q && ge.trim() || null
    }),
    [h, X, y, _, p, k, R, M, U, V, K, pe, Q, r, oe, De, ge]
  ), ct = hn(Ne, 350);
  x.useEffect(() => {
    if (!ct.lineas.length) {
      st(null), ln("Añade al menos una línea con base.");
      return;
    }
    const m = an();
    t.post("/gastos/simular", ct).then((L) => m() && (st(L), ln(""))).catch((L) => m() && (st(null), ln(L.message)));
  }, [ct, t]);
  const tt = (m, L) => fe((P) => P.map((B) => B.clave === m ? { ...B, ...L } : B)), Mt = (m) => o.find((L) => L.codigo === m), on = (m) => {
    var L;
    return (L = O == null ? void 0 : O.lineas) == null ? void 0 : L[K.filter((P) => P.base !== 0).indexOf(m)];
  };
  function Ve(m) {
    if (!O) return;
    const L = /* @__PURE__ */ new Date((_ || p) + "T00:00:00"), P = be(O.total / m);
    J(Array.from({ length: m }, (B, b) => {
      const Z = new Date(L);
      return Z.setMonth(Z.getMonth() + b + 1), { fecha: Z.toISOString().slice(0, 10), importe: b === m - 1 ? be(O.total - P * (m - 1)) : P };
    }));
  }
  async function er() {
    ut(!0);
    try {
      const m = e.id ? await t.put(`/gastos/${e.id}`, Ne) : await t.post("/gastos", Ne);
      n.aviso(e.id ? "Factura corregida." : "Factura registrada.", "ok"), m.avisoRiesgo && n.aviso(m.avisoRiesgo, "err"), e.alGuardar(m.id);
    } catch (m) {
      n.aviso(m.message, "err");
    } finally {
      ut(!1);
    }
  }
  const $t = be((pe ?? []).reduce((m, L) => m + (Number(L.importe) || 0), 0));
  return /* @__PURE__ */ a.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsxs("div", { className: "panel-head", children: [
        /* @__PURE__ */ a.jsx("h2", { children: e.id ? `Corregir factura ${(r == null ? void 0 : r.numeroFactura) ?? ""}` : Q ? "Rectificativa / abono del proveedor" : "Nueva factura de proveedor" }),
        /* @__PURE__ */ a.jsxs("div", { style: { display: "flex", gap: 8 }, children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: e.alCancelar, children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", disabled: !O || ee, onClick: er, children: e.id ? "Guardar corrección" : Q ? "Registrar abono" : "Registrar factura" })
        ] })
      ] }),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-cabecera", children: [
        /* @__PURE__ */ a.jsxs("div", { className: "dx-cab-campos", children: [
          /* @__PURE__ */ a.jsx(To, { terceros: l, valor: h, alCambiar: v, etiqueta: "Proveedor" }),
          /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Nº de factura del proveedor" }),
              /* @__PURE__ */ a.jsx("input", { value: y, onChange: (m) => w(m.target.value), placeholder: "F-2026/0117" })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Fecha de la factura" }),
              /* @__PURE__ */ a.jsx("input", { type: "date", value: _, onChange: (m) => f(m.target.value) })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Fecha de registro" }),
              /* @__PURE__ */ a.jsx("input", { type: "date", value: p, onChange: (m) => g(m.target.value), title: "La del asiento y la del periodo de IVA en que se deduce" })
            ] })
          ] }),
          /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Forma de pago" }),
              /* @__PURE__ */ a.jsxs("select", { value: M, onChange: (m) => A(m.target.value), children: [
                /* @__PURE__ */ a.jsx("option", { value: "", children: "Contado (vence en la fecha de factura)" }),
                u.map((m) => /* @__PURE__ */ a.jsx("option", { value: m.id, children: m.nombre }, m.id))
              ] })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Retención IRPF %" }),
              /* @__PURE__ */ a.jsx("input", { type: "number", step: "0.01", value: R, onChange: (m) => z(Number(m.target.value)) })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Prorrata especial" }),
              /* @__PURE__ */ a.jsxs("select", { value: V, onChange: (m) => W(m.target.value), children: [
                /* @__PURE__ */ a.jsx("option", { value: "Comun", children: "Uso común" }),
                /* @__PURE__ */ a.jsx("option", { value: "ConDerecho", children: "Solo operaciones con derecho" }),
                /* @__PURE__ */ a.jsx("option", { value: "SinDerecho", children: "Solo operaciones exentas" })
              ] })
            ] })
          ] }),
          /* @__PURE__ */ a.jsx("div", { className: "dx-fila", children: /* @__PURE__ */ a.jsxs("div", { style: { gridColumn: "1 / -1" }, children: [
            /* @__PURE__ */ a.jsx("label", { children: "Concepto (vacío: «Factura nº»)" }),
            /* @__PURE__ */ a.jsx("input", { value: k, onChange: (m) => T(m.target.value) })
          ] }) }),
          Q && /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Factura que rectifica" }),
              /* @__PURE__ */ a.jsx("input", { value: oe, disabled: !!(r != null && r.rectificaGastoId), onChange: (m) => Fe(m.target.value) })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Fecha de la rectificada" }),
              /* @__PURE__ */ a.jsx("input", { type: "date", value: De, disabled: !!(r != null && r.rectificaGastoId), onChange: (m) => de(m.target.value) })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Motivo" }),
              /* @__PURE__ */ a.jsx("input", { value: ge, onChange: (m) => C(m.target.value), placeholder: "Devolución, descuento posterior, error de precio…" })
            ] })
          ] }),
          Q && /* @__PURE__ */ a.jsx("div", { className: "muted", children: "Las bases del abono van en negativo; el asiento y el SII (R1, por diferencias) salen con el signo." }),
          /* @__PURE__ */ a.jsxs("label", { className: "dx-check", children: [
            /* @__PURE__ */ a.jsx("input", { type: "checkbox", checked: U, onChange: (m) => F(m.target.checked) }),
            " El proveedor me cobra recargo de equivalencia"
          ] }),
          j && /* @__PURE__ */ a.jsx("div", { className: "muted", children: "La empresa está en recargo de equivalencia: el IVA y el recargo soportados son coste (no se deducen)." })
        ] }),
        /* @__PURE__ */ a.jsx("div", { className: "dx-ficha", children: X ? /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("strong", { children: X.nombre }),
          /* @__PURE__ */ a.jsx("div", { className: "muted", children: [X.nifFiscal, X.poblacion, X.pais].filter(Boolean).join(" · ") }),
          !X.nifFiscal && /* @__PURE__ */ a.jsx("div", { className: "dx-aviso", children: "⚠ Sin NIF en la ficha: hace falta para el libro de IVA, el 347 y el SII." }),
          (O == null ? void 0 : O.avisoRiesgo) && /* @__PURE__ */ a.jsxs("div", { className: "dx-aviso", children: [
            "⚠ ",
            O.avisoRiesgo
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
        /* @__PURE__ */ a.jsx("tbody", { children: K.map((m, L) => {
          const P = Mt(m.codigoIva), B = on(m);
          return /* @__PURE__ */ a.jsxs("tr", { className: L % 2 ? "dx-par" : "", children: [
            /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx("input", { value: m.descripcion, onChange: (b) => tt(m.clave, { descripcion: b.target.value }), placeholder: "Qué se compra" }) }),
            /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx("input", { list: "dx-cuentas-gasto", value: m.cuentaGasto, onChange: (b) => tt(m.clave, { cuentaGasto: b.target.value }), placeholder: "la de la regla" }) }),
            /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx("input", { className: "num", type: "number", step: "0.01", value: m.base || "", onChange: (b) => tt(m.clave, { base: Number(b.target.value) }) }) }),
            /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsxs("select", { value: m.codigoIva, onChange: (b) => tt(m.clave, { codigoIva: b.target.value, porcentajeIva: null }), children: [
              /* @__PURE__ */ a.jsx("option", { value: "", children: "General" }),
              o.map((b) => /* @__PURE__ */ a.jsx("option", { value: b.codigo, children: b.nombre }, b.codigo))
            ] }) }),
            /* @__PURE__ */ a.jsx("td", { children: om(P == null ? void 0 : P.clase) ? /* @__PURE__ */ a.jsx("input", { className: "num", type: "number", step: "0.01", value: m.porcentajeIva ?? "", placeholder: "21", title: "Tipo que autoliquidas", onChange: (b) => tt(m.clave, { porcentajeIva: b.target.value === "" ? null : Number(b.target.value) }) }) : /* @__PURE__ */ a.jsx("span", { className: "muted", children: B ? `${ke(B.porcentajeIva)} %` : "" }) }),
            /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx("input", { className: "num", type: "number", step: "1", min: 0, max: 100, value: m.porcentajeDeducible, onChange: (b) => tt(m.clave, { porcentajeDeducible: Number(b.target.value) }) }) }),
            /* @__PURE__ */ a.jsx("td", { className: "num", children: B ? /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
              /* @__PURE__ */ a.jsx("strong", { children: D(B.cuota) }),
              B.autoliquidada && /* @__PURE__ */ a.jsx("div", { className: "dx-sub", children: "autoliquidada" }),
              B.cuotaRecargo !== 0 && /* @__PURE__ */ a.jsxs("div", { className: "dx-sub", children: [
                "+ recargo ",
                D(B.cuotaRecargo)
              ] })
            ] }) : "—" }),
            /* @__PURE__ */ a.jsx("td", { className: "right", children: /* @__PURE__ */ a.jsx("button", { className: "dx-icono", title: "Quitar", onClick: () => fe((b) => b.length > 1 ? b.filter((Z) => Z.clave !== m.clave) : [Va()]), children: "✕" }) })
          ] }, m.clave);
        }) })
      ] }),
      /* @__PURE__ */ a.jsx("datalist", { id: "dx-cuentas-gasto", children: N.map((m) => /* @__PURE__ */ a.jsx("option", { value: m.codigo, children: m.nombre }, m.codigo)) }),
      /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", style: { marginTop: 8 }, onClick: () => fe((m) => {
        var L;
        return [...m, Va(((L = m[m.length - 1]) == null ? void 0 : L.codigoIva) ?? "")];
      }), children: "+ Añadir línea" }),
      /* @__PURE__ */ a.jsx("span", { className: "muted dx-ayuda", children: "Una línea por cada base e impuesto de la factura. En inversión del sujeto pasivo e intracomunitarias el IVA lo autoliquidas tú (no lo cobra el proveedor)." })
    ] }),
    /* @__PURE__ */ a.jsxs("div", { className: "dx-pie", children: [
      /* @__PURE__ */ a.jsxs("div", { className: "panel", style: { margin: 0 }, children: [
        /* @__PURE__ */ a.jsxs("div", { className: "panel-head", children: [
          /* @__PURE__ */ a.jsx("h2", { children: "Vencimientos" }),
          /* @__PURE__ */ a.jsxs("div", { className: "dx-acciones", children: [
            [2, 3, 4].map((m) => /* @__PURE__ */ a.jsxs("button", { className: "btn small secondary", disabled: !O, onClick: () => Ve(m), children: [
              m,
              " plazos"
            ] }, m)),
            pe && /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", onClick: () => J(null), children: "Según forma de pago" })
          ] })
        ] }),
        pe ? /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          pe.map((m, L) => /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", style: { marginBottom: 6 }, children: [
            /* @__PURE__ */ a.jsx("input", { type: "date", value: m.fecha, onChange: (P) => J(pe.map((B, b) => b === L ? { ...B, fecha: P.target.value } : B)) }),
            /* @__PURE__ */ a.jsx("input", { className: "num", type: "number", step: "0.01", value: m.importe, onChange: (P) => J(pe.map((B, b) => b === L ? { ...B, importe: Number(P.target.value) } : B)) })
          ] }, L)),
          O && $t !== O.total && /* @__PURE__ */ a.jsxs("p", { className: "dx-rojo", style: { margin: 0 }, children: [
            "Los plazos suman ",
            D($t),
            "; la factura, ",
            D(O.total),
            "."
          ] })
        ] }) : /* @__PURE__ */ a.jsx("p", { className: "muted", style: { margin: 0 }, children: ((O == null ? void 0 : O.vencimientos) ?? []).map((m) => `${Se(m.fecha)}: ${D(m.importe)}`).join(" · ") || "—" }),
        Ke && /* @__PURE__ */ a.jsx("p", { className: "dx-rojo", style: { marginBottom: 0 }, children: Ke })
      ] }),
      /* @__PURE__ */ a.jsxs("div", { className: "panel dx-totales", children: [
        ((O == null ? void 0 : O.desglose) ?? []).map((m, L) => {
          var P;
          return /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
            /* @__PURE__ */ a.jsxs("span", { className: "muted", children: [
              ((P = Mt(m.codigoIva)) == null ? void 0 : P.nombre) ?? m.codigoIva,
              " ",
              m.autoliquidada ? `(${ke(m.porcentajeIva)} %, autoliquidado)` : "",
              " · base ",
              ke(m.base)
            ] }),
            /* @__PURE__ */ a.jsx("span", { children: D(m.cuota) })
          ] }, L);
        }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Base imponible" }),
          /* @__PURE__ */ a.jsx("span", { children: D(O == null ? void 0 : O.baseImponible) })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Impuestos" }),
          /* @__PURE__ */ a.jsx("span", { children: D(O == null ? void 0 : O.cuotaIva) })
        ] }),
        !!(O != null && O.recargoTotal) && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Recargo de equivalencia" }),
          /* @__PURE__ */ a.jsx("span", { children: D(O.recargoTotal) })
        ] }),
        !!(O != null && O.retencionIrpf) && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Retención IRPF" }),
          /* @__PURE__ */ a.jsxs("span", { children: [
            "−",
            D(O.retencionIrpf)
          ] })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ a.jsx("span", { children: ((O == null ? void 0 : O.total) ?? 0) < 0 ? "A favor (abono del proveedor)" : "Total a pagar" }),
          /* @__PURE__ */ a.jsx("span", { children: D(O == null ? void 0 : O.total) })
        ] }),
        O && (O.desglose ?? []).some((m) => m.cuotaDeducible !== m.cuota) && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "IVA deducible (antes de prorrata)" }),
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: D((O.desglose ?? []).reduce((m, L) => m + L.cuotaDeducible, 0)) })
        ] })
      ] })
    ] })
  ] });
}
function um(e) {
  const { api: t, anfitrion: n, navegar: r } = Ct(), [l, i] = x.useState(null), [o, s] = x.useState(null), [u, d] = x.useState(""), [N, c] = x.useState(!1), h = () => {
    t.get(`/gastos/${e.id}`).then(i).catch((w) => d(w.message)), t.get(`/gastos/${e.id}/saldo`).then(s).catch(() => s(null));
  };
  if (x.useEffect(h, [e.id]), u) return /* @__PURE__ */ a.jsx("div", { className: "panel", children: /* @__PURE__ */ a.jsx("p", { className: "dx-rojo", children: u }) });
  if (!l) return /* @__PURE__ */ a.jsx("div", { className: "muted", children: "Cargando…" });
  const v = l.estado === "Registrado", y = !o || o.liquidado === 0;
  return /* @__PURE__ */ a.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsxs("div", { className: "panel-head", children: [
        /* @__PURE__ */ a.jsxs("h2", { style: { display: "flex", alignItems: "center", gap: 10 }, children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "gasto", pantalla: "lista" }), children: "←" }),
          "Factura ",
          /* @__PURE__ */ a.jsx("span", { className: "mono", children: l.numeroFactura ?? "(sin número)" }),
          " ",
          /* @__PURE__ */ a.jsx("span", { className: Ro(l.estado === "Anulado" ? "Anulada" : "Emitida"), children: l.estado }),
          l.esRectificativa && /* @__PURE__ */ a.jsxs("span", { className: "pill", children: [
            "Rectifica ",
            l.numeroRectificado
          ] })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-acciones", children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "gasto", pantalla: "editor", semilla: { ...l, numeroFactura: null } }), children: "Duplicar" }),
          v && y && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "gasto", pantalla: "editor", id: l.id, semilla: l }), children: "Corregir" }),
          v && o && o.pendiente > 0 && n.irA && /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: () => n.irA("pagos"), children: "Pagar" }),
          v && !l.esRectificativa && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => {
            var w;
            return r({ tipo: "gasto", pantalla: "editor", semilla: {
              ...l,
              numeroFactura: null,
              esRectificativa: !0,
              rectificaGastoId: l.id,
              numeroRectificado: l.numeroFactura ?? l.concepto,
              fechaRectificada: l.fechaFactura ?? l.fecha,
              motivoRectificacion: "",
              vencimientos: null,
              lineas: (w = l.lineas) == null ? void 0 : w.map((_) => ({ ..._, base: -_.base }))
            } });
          }, children: "Rectificativa / abono" }),
          v && y && /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", onClick: () => c(!0), children: "Anular" })
        ] })
      ] }),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ a.jsxs("div", { className: "dx-dato", children: [
          /* @__PURE__ */ a.jsx("small", { children: "Proveedor" }),
          /* @__PURE__ */ a.jsx("div", { children: /* @__PURE__ */ a.jsx("strong", { children: l.proveedorTexto }) })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-dato", children: [
          /* @__PURE__ */ a.jsx("small", { children: "Fecha de factura" }),
          /* @__PURE__ */ a.jsx("div", { children: Se(l.fechaFactura ?? l.fecha) })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-dato", children: [
          /* @__PURE__ */ a.jsx("small", { children: "Registro" }),
          /* @__PURE__ */ a.jsx("div", { children: Se(l.fecha) })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-dato", children: [
          /* @__PURE__ */ a.jsx("small", { children: "Pago" }),
          /* @__PURE__ */ a.jsx("div", { children: o ? o.pendiente <= 0 ? /* @__PURE__ */ a.jsx("span", { className: "pill ok", children: "Pagada" }) : /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            "Pendiente ",
            /* @__PURE__ */ a.jsx("strong", { children: D(o.pendiente) })
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
        /* @__PURE__ */ a.jsx("tbody", { children: (l.lineas ?? []).map((w, _) => /* @__PURE__ */ a.jsxs("tr", { children: [
          /* @__PURE__ */ a.jsx("td", { children: w.descripcion ?? "" }),
          /* @__PURE__ */ a.jsx("td", { className: "mono muted", children: w.cuentaGasto ?? "regla" }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: D(w.base) }),
          /* @__PURE__ */ a.jsxs("td", { children: [
            w.codigoIva,
            " · ",
            ke(w.porcentajeIva),
            " %",
            w.autoliquidada ? " · autoliquidada" : "",
            w.cuotaRecargo ? ` · recargo ${D(w.cuotaRecargo)}` : ""
          ] }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: D(w.cuota) }),
          /* @__PURE__ */ a.jsxs("td", { className: "num muted", children: [
            w.porcentajeDeducible !== 100 ? `${ke(w.porcentajeDeducible)} % · ` : "",
            D(w.cuotaDeducible)
          ] })
        ] }, _)) })
      ] }),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-totales-vista", children: [
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Base imponible" }),
          /* @__PURE__ */ a.jsx("span", { children: D(l.baseImponible) })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Impuestos" }),
          /* @__PURE__ */ a.jsx("span", { children: D(l.cuotaIva) })
        ] }),
        !!l.recargoTotal && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Recargo de equivalencia" }),
          /* @__PURE__ */ a.jsx("span", { children: D(l.recargoTotal) })
        ] }),
        !!l.retencionIrpf && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsxs("span", { className: "muted", children: [
            "Retención IRPF (",
            ke(l.porcentajeIrpf),
            " %)"
          ] }),
          /* @__PURE__ */ a.jsxs("span", { children: [
            "−",
            D(l.retencionIrpf)
          ] })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ a.jsx("span", { children: l.total < 0 ? "A favor (abono del proveedor)" : "Total a pagar" }),
          /* @__PURE__ */ a.jsx("span", { children: D(l.total) })
        ] })
      ] }),
      /* @__PURE__ */ a.jsxs("p", { className: "muted", style: { fontSize: 12.5 }, children: [
        "Vencimientos: ",
        (l.vencimientos ?? []).map((w) => `${Se(w.fecha)} ${D(w.importe)}`).join(" · ")
      ] })
    ] }),
    N && /* @__PURE__ */ a.jsx(
      Nn,
      {
        titulo: "Anular la factura",
        alCerrar: () => c(!1),
        acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => c(!1), children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: async () => {
            try {
              await t.post(`/gastos/${l.id}/anular`), n.aviso("Factura anulada.", "ok"), c(!1), h();
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
function cm(e) {
  const [t, n] = x.useState(e.inicial), r = x.useRef(0), [l, i] = x.useState(0), o = x.useMemo(() => Vp(e.anfitrion), [e.anfitrion]), s = (c) => {
    n(c), i(++r.current), window.scrollTo({ top: 0 });
  }, u = { api: o, anfitrion: e.anfitrion, ruta: t, navegar: s }, d = `${t.tipo}-${t.pantalla}-${t.id ?? ""}-${l}`;
  let N;
  if (t.pantalla === "lista") N = /* @__PURE__ */ a.jsx(Xp, { tipo: t.tipo });
  else if (t.pantalla === "vista" && t.id)
    N = t.tipo === "factura" ? /* @__PURE__ */ a.jsx(rm, { id: t.id }) : t.tipo === "presupuesto" ? /* @__PURE__ */ a.jsx(lm, { id: t.id }) : t.tipo === "pedido" ? /* @__PURE__ */ a.jsx(am, { id: t.id }) : t.tipo === "gasto" ? /* @__PURE__ */ a.jsx(um, { id: t.id }) : /* @__PURE__ */ a.jsx(im, { id: t.id });
  else if (t.tipo === "gasto")
    N = /* @__PURE__ */ a.jsx(
      sm,
      {
        id: t.id,
        semilla: t.semilla,
        alGuardar: (c) => s({ tipo: "gasto", pantalla: "vista", id: c }),
        alCancelar: () => s(t.id ? { tipo: "gasto", pantalla: "vista", id: t.id } : { tipo: "gasto", pantalla: "lista" })
      }
    );
  else if (t.tipo === "compra")
    N = /* @__PURE__ */ a.jsx(
      nm,
      {
        id: t.id,
        semilla: t.semilla,
        alGuardar: (c) => s({ tipo: "compra", pantalla: "vista", id: c }),
        alCancelar: () => s(t.id ? { tipo: "compra", pantalla: "vista", id: t.id } : { tipo: "compra", pantalla: "lista" })
      }
    );
  else {
    const c = t.tipo;
    N = /* @__PURE__ */ a.jsx(
      tm,
      {
        tipo: c,
        id: t.id,
        semilla: t.semilla,
        alGuardar: (h) => s({ tipo: c, pantalla: "vista", id: h }),
        alCancelar: () => s(t.id ? { tipo: c, pantalla: "vista", id: t.id } : { tipo: c, pantalla: "lista" })
      }
    );
  }
  return /* @__PURE__ */ a.jsx(id.Provider, { value: u, children: /* @__PURE__ */ a.jsx("div", { className: "dx-raiz", children: N }, d) });
}
const dm = `
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
function fm() {
  if (document.getElementById("dx-estilos")) return;
  const e = document.createElement("style");
  e.id = "dx-estilos", e.textContent = dm, document.head.appendChild(e);
}
function pm(e, t, n) {
  fm();
  const r = ad(e);
  return r.render(/* @__PURE__ */ a.jsx(cm, { anfitrion: t, inicial: n })), () => r.unmount();
}
export {
  pm as montar
};
