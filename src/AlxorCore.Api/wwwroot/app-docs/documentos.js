var Zs = { exports: {} }, Gl = {}, Js = { exports: {} }, G = {};
/**
 * @license React
 * react.production.min.js
 *
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */
var br = Symbol.for("react.element"), yd = Symbol.for("react.portal"), jd = Symbol.for("react.fragment"), Nd = Symbol.for("react.strict_mode"), Sd = Symbol.for("react.profiler"), wd = Symbol.for("react.provider"), kd = Symbol.for("react.context"), Cd = Symbol.for("react.forward_ref"), Ed = Symbol.for("react.suspense"), Id = Symbol.for("react.memo"), Pd = Symbol.for("react.lazy"), Uo = Symbol.iterator;
function Fd(e) {
  return e === null || typeof e != "object" ? null : (e = Uo && e[Uo] || e["@@iterator"], typeof e == "function" ? e : null);
}
var eu = { isMounted: function() {
  return !1;
}, enqueueForceUpdate: function() {
}, enqueueReplaceState: function() {
}, enqueueSetState: function() {
} }, tu = Object.assign, nu = {};
function Zn(e, t, n) {
  this.props = e, this.context = t, this.refs = nu, this.updater = n || eu;
}
Zn.prototype.isReactComponent = {};
Zn.prototype.setState = function(e, t) {
  if (typeof e != "object" && typeof e != "function" && e != null) throw Error("setState(...): takes an object of state variables to update or a function which returns an object of state variables.");
  this.updater.enqueueSetState(this, e, t, "setState");
};
Zn.prototype.forceUpdate = function(e) {
  this.updater.enqueueForceUpdate(this, e, "forceUpdate");
};
function ru() {
}
ru.prototype = Zn.prototype;
function Mi(e, t, n) {
  this.props = e, this.context = t, this.refs = nu, this.updater = n || eu;
}
var $i = Mi.prototype = new ru();
$i.constructor = Mi;
tu($i, Zn.prototype);
$i.isPureReactComponent = !0;
var bo = Array.isArray, lu = Object.prototype.hasOwnProperty, Oi = { current: null }, au = { key: !0, ref: !0, __self: !0, __source: !0 };
function iu(e, t, n) {
  var r, l = {}, i = null, o = null;
  if (t != null) for (r in t.ref !== void 0 && (o = t.ref), t.key !== void 0 && (i = "" + t.key), t) lu.call(t, r) && !au.hasOwnProperty(r) && (l[r] = t[r]);
  var s = arguments.length - 2;
  if (s === 1) l.children = n;
  else if (1 < s) {
    for (var u = Array(s), d = 0; d < s; d++) u[d] = arguments[d + 2];
    l.children = u;
  }
  if (e && e.defaultProps) for (r in s = e.defaultProps, s) l[r] === void 0 && (l[r] = s[r]);
  return { $$typeof: br, type: e, key: i, ref: o, props: l, _owner: Oi.current };
}
function _d(e, t) {
  return { $$typeof: br, type: e.type, key: t, ref: e.ref, props: e.props, _owner: e._owner };
}
function Ai(e) {
  return typeof e == "object" && e !== null && e.$$typeof === br;
}
function Rd(e) {
  var t = { "=": "=0", ":": "=2" };
  return "$" + e.replace(/[=:]/g, function(n) {
    return t[n];
  });
}
var Vo = /\/+/g;
function pa(e, t) {
  return typeof e == "object" && e !== null && e.key != null ? Rd("" + e.key) : t.toString(36);
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
        case br:
        case yd:
          o = !0;
      }
  }
  if (o) return o = e, l = l(o), e = r === "" ? "." + pa(o, 0) : r, bo(l) ? (n = "", e != null && (n = e.replace(Vo, "$&/") + "/"), dl(l, t, n, "", function(d) {
    return d;
  })) : l != null && (Ai(l) && (l = _d(l, n + (!l.key || o && o.key === l.key ? "" : ("" + l.key).replace(Vo, "$&/") + "/") + e)), t.push(l)), 1;
  if (o = 0, r = r === "" ? "." : r + ":", bo(e)) for (var s = 0; s < e.length; s++) {
    i = e[s];
    var u = r + pa(i, s);
    o += dl(i, t, n, u, l);
  }
  else if (u = Fd(e), typeof u == "function") for (e = u.call(e), s = 0; !(i = e.next()).done; ) i = i.value, u = r + pa(i, s++), o += dl(i, t, n, u, l);
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
function Td(e) {
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
var Ue = { current: null }, fl = { transition: null }, zd = { ReactCurrentDispatcher: Ue, ReactCurrentBatchConfig: fl, ReactCurrentOwner: Oi };
function ou() {
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
G.Component = Zn;
G.Fragment = jd;
G.Profiler = Sd;
G.PureComponent = Mi;
G.StrictMode = Nd;
G.Suspense = Ed;
G.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED = zd;
G.act = ou;
G.cloneElement = function(e, t, n) {
  if (e == null) throw Error("React.cloneElement(...): The argument must be a React element, but you passed " + e + ".");
  var r = tu({}, e.props), l = e.key, i = e.ref, o = e._owner;
  if (t != null) {
    if (t.ref !== void 0 && (i = t.ref, o = Oi.current), t.key !== void 0 && (l = "" + t.key), e.type && e.type.defaultProps) var s = e.type.defaultProps;
    for (u in t) lu.call(t, u) && !au.hasOwnProperty(u) && (r[u] = t[u] === void 0 && s !== void 0 ? s[u] : t[u]);
  }
  var u = arguments.length - 2;
  if (u === 1) r.children = n;
  else if (1 < u) {
    s = Array(u);
    for (var d = 0; d < u; d++) s[d] = arguments[d + 2];
    r.children = s;
  }
  return { $$typeof: br, type: e.type, key: l, ref: i, props: r, _owner: o };
};
G.createContext = function(e) {
  return e = { $$typeof: kd, _currentValue: e, _currentValue2: e, _threadCount: 0, Provider: null, Consumer: null, _defaultValue: null, _globalName: null }, e.Provider = { $$typeof: wd, _context: e }, e.Consumer = e;
};
G.createElement = iu;
G.createFactory = function(e) {
  var t = iu.bind(null, e);
  return t.type = e, t;
};
G.createRef = function() {
  return { current: null };
};
G.forwardRef = function(e) {
  return { $$typeof: Cd, render: e };
};
G.isValidElement = Ai;
G.lazy = function(e) {
  return { $$typeof: Pd, _payload: { _status: -1, _result: e }, _init: Td };
};
G.memo = function(e, t) {
  return { $$typeof: Id, type: e, compare: t === void 0 ? null : t };
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
G.unstable_act = ou;
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
Js.exports = G;
var g = Js.exports;
/**
 * @license React
 * react-jsx-runtime.production.min.js
 *
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */
var Ld = g, Dd = Symbol.for("react.element"), Md = Symbol.for("react.fragment"), $d = Object.prototype.hasOwnProperty, Od = Ld.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED.ReactCurrentOwner, Ad = { key: !0, ref: !0, __self: !0, __source: !0 };
function su(e, t, n) {
  var r, l = {}, i = null, o = null;
  n !== void 0 && (i = "" + n), t.key !== void 0 && (i = "" + t.key), t.ref !== void 0 && (o = t.ref);
  for (r in t) $d.call(t, r) && !Ad.hasOwnProperty(r) && (l[r] = t[r]);
  if (e && e.defaultProps) for (r in t = e.defaultProps, t) l[r] === void 0 && (l[r] = t[r]);
  return { $$typeof: Dd, type: e, key: i, ref: o, props: l, _owner: Od.current };
}
Gl.Fragment = Md;
Gl.jsx = su;
Gl.jsxs = su;
Zs.exports = Gl;
var a = Zs.exports, uu = { exports: {} }, et = {}, cu = { exports: {} }, du = {};
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
    var w = E.length;
    E.push(j);
    e: for (; 0 < w; ) {
      var b = w - 1 >>> 1, W = E[b];
      if (0 < l(W, j)) E[b] = j, E[w] = W, w = b;
      else break e;
    }
  }
  function n(E) {
    return E.length === 0 ? null : E[0];
  }
  function r(E) {
    if (E.length === 0) return null;
    var j = E[0], w = E.pop();
    if (w !== j) {
      E[0] = w;
      e: for (var b = 0, W = E.length, q = W >>> 1; b < q; ) {
        var pe = 2 * (b + 1) - 1, me = E[pe], te = pe + 1, A = E[te];
        if (0 > l(me, w)) te < W && 0 > l(A, me) ? (E[b] = A, E[te] = w, b = te) : (E[b] = me, E[pe] = w, b = pe);
        else if (te < W && 0 > l(A, w)) E[b] = A, E[te] = w, b = te;
        else break e;
      }
    }
    return j;
  }
  function l(E, j) {
    var w = E.sortIndex - j.sortIndex;
    return w !== 0 ? w : E.id - j.id;
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
  var u = [], d = [], N = 1, c = null, h = 3, x = !1, y = !1, S = !1, M = typeof setTimeout == "function" ? setTimeout : null, p = typeof clearTimeout == "function" ? clearTimeout : null, f = typeof setImmediate < "u" ? setImmediate : null;
  typeof navigator < "u" && navigator.scheduling !== void 0 && navigator.scheduling.isInputPending !== void 0 && navigator.scheduling.isInputPending.bind(navigator.scheduling);
  function v(E) {
    for (var j = n(d); j !== null; ) {
      if (j.callback === null) r(d);
      else if (j.startTime <= E) r(d), j.sortIndex = j.expirationTime, t(u, j);
      else break;
      j = n(d);
    }
  }
  function C(E) {
    if (S = !1, v(E), !y) if (n(u) !== null) y = !0, fe(k);
    else {
      var j = n(d);
      j !== null && xe(C, j.startTime - E);
    }
  }
  function k(E, j) {
    y = !1, S && (S = !1, p(D), D = -1), x = !0;
    var w = h;
    try {
      for (v(j), c = n(u); c !== null && (!(c.expirationTime > j) || E && !R()); ) {
        var b = c.callback;
        if (typeof b == "function") {
          c.callback = null, h = c.priorityLevel;
          var W = b(c.expirationTime <= j);
          j = e.unstable_now(), typeof W == "function" ? c.callback = W : c === n(u) && r(u), v(j);
        } else r(u);
        c = n(u);
      }
      if (c !== null) var q = !0;
      else {
        var pe = n(d);
        pe !== null && xe(C, pe.startTime - j), q = !1;
      }
      return q;
    } finally {
      c = null, h = w, x = !1;
    }
  }
  var P = !1, T = null, D = -1, O = 5, U = -1;
  function R() {
    return !(e.unstable_now() - U < O);
  }
  function Q() {
    if (T !== null) {
      var E = e.unstable_now();
      U = E;
      var j = !0;
      try {
        j = T(!0, E);
      } finally {
        j ? se() : (P = !1, T = null);
      }
    } else P = !1;
  }
  var se;
  if (typeof f == "function") se = function() {
    f(Q);
  };
  else if (typeof MessageChannel < "u") {
    var Pe = new MessageChannel(), De = Pe.port2;
    Pe.port1.onmessage = Q, se = function() {
      De.postMessage(null);
    };
  } else se = function() {
    M(Q, 0);
  };
  function fe(E) {
    T = E, P || (P = !0, se());
  }
  function xe(E, j) {
    D = M(function() {
      E(e.unstable_now());
    }, j);
  }
  e.unstable_IdlePriority = 5, e.unstable_ImmediatePriority = 1, e.unstable_LowPriority = 4, e.unstable_NormalPriority = 3, e.unstable_Profiling = null, e.unstable_UserBlockingPriority = 2, e.unstable_cancelCallback = function(E) {
    E.callback = null;
  }, e.unstable_continueExecution = function() {
    y || x || (y = !0, fe(k));
  }, e.unstable_forceFrameRate = function(E) {
    0 > E || 125 < E ? console.error("forceFrameRate takes a positive int between 0 and 125, forcing frame rates higher than 125 fps is not supported") : O = 0 < E ? Math.floor(1e3 / E) : 5;
  }, e.unstable_getCurrentPriorityLevel = function() {
    return h;
  }, e.unstable_getFirstCallbackNode = function() {
    return n(u);
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
    var w = h;
    h = j;
    try {
      return E();
    } finally {
      h = w;
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
    var w = h;
    h = E;
    try {
      return j();
    } finally {
      h = w;
    }
  }, e.unstable_scheduleCallback = function(E, j, w) {
    var b = e.unstable_now();
    switch (typeof w == "object" && w !== null ? (w = w.delay, w = typeof w == "number" && 0 < w ? b + w : b) : w = b, E) {
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
    return W = w + W, E = { id: N++, callback: j, priorityLevel: E, startTime: w, expirationTime: W, sortIndex: -1 }, w > b ? (E.sortIndex = w, t(d, E), n(u) === null && E === n(d) && (S ? (p(D), D = -1) : S = !0, xe(C, w - b))) : (E.sortIndex = W, t(u, E), y || x || (y = !0, fe(k))), E;
  }, e.unstable_shouldYield = R, e.unstable_wrapCallback = function(E) {
    var j = h;
    return function() {
      var w = h;
      h = j;
      try {
        return E.apply(this, arguments);
      } finally {
        h = w;
      }
    };
  };
})(du);
cu.exports = du;
var Ud = cu.exports;
/**
 * @license React
 * react-dom.production.min.js
 *
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */
var bd = g, Je = Ud;
function _(e) {
  for (var t = "https://reactjs.org/docs/error-decoder.html?invariant=" + e, n = 1; n < arguments.length; n++) t += "&args[]=" + encodeURIComponent(arguments[n]);
  return "Minified React error #" + e + "; visit " + t + " for the full message or use the non-minified dev environment for full errors and additional helpful warnings.";
}
var fu = /* @__PURE__ */ new Set(), kr = {};
function Nn(e, t) {
  Wn(e, t), Wn(e + "Capture", t);
}
function Wn(e, t) {
  for (kr[e] = t, e = 0; e < t.length; e++) fu.add(t[e]);
}
var zt = !(typeof window > "u" || typeof window.document > "u" || typeof window.document.createElement > "u"), Va = Object.prototype.hasOwnProperty, Vd = /^[:A-Z_a-z\u00C0-\u00D6\u00D8-\u00F6\u00F8-\u02FF\u0370-\u037D\u037F-\u1FFF\u200C-\u200D\u2070-\u218F\u2C00-\u2FEF\u3001-\uD7FF\uF900-\uFDCF\uFDF0-\uFFFD][:A-Z_a-z\u00C0-\u00D6\u00D8-\u00F6\u00F8-\u02FF\u0370-\u037D\u037F-\u1FFF\u200C-\u200D\u2070-\u218F\u2C00-\u2FEF\u3001-\uD7FF\uF900-\uFDCF\uFDF0-\uFFFD\-.0-9\u00B7\u0300-\u036F\u203F-\u2040]*$/, Bo = {}, Ho = {};
function Bd(e) {
  return Va.call(Ho, e) ? !0 : Va.call(Bo, e) ? !1 : Vd.test(e) ? Ho[e] = !0 : (Bo[e] = !0, !1);
}
function Hd(e, t, n, r) {
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
function Wd(e, t, n, r) {
  if (t === null || typeof t > "u" || Hd(e, t, n, r)) return !0;
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
function be(e, t, n, r, l, i, o) {
  this.acceptsBooleans = t === 2 || t === 3 || t === 4, this.attributeName = r, this.attributeNamespace = l, this.mustUseProperty = n, this.propertyName = e, this.type = t, this.sanitizeURL = i, this.removeEmptyString = o;
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
var Ui = /[\-:]([a-z])/g;
function bi(e) {
  return e[1].toUpperCase();
}
"accent-height alignment-baseline arabic-form baseline-shift cap-height clip-path clip-rule color-interpolation color-interpolation-filters color-profile color-rendering dominant-baseline enable-background fill-opacity fill-rule flood-color flood-opacity font-family font-size font-size-adjust font-stretch font-style font-variant font-weight glyph-name glyph-orientation-horizontal glyph-orientation-vertical horiz-adv-x horiz-origin-x image-rendering letter-spacing lighting-color marker-end marker-mid marker-start overline-position overline-thickness paint-order panose-1 pointer-events rendering-intent shape-rendering stop-color stop-opacity strikethrough-position strikethrough-thickness stroke-dasharray stroke-dashoffset stroke-linecap stroke-linejoin stroke-miterlimit stroke-opacity stroke-width text-anchor text-decoration text-rendering underline-position underline-thickness unicode-bidi unicode-range units-per-em v-alphabetic v-hanging v-ideographic v-mathematical vector-effect vert-adv-y vert-origin-x vert-origin-y word-spacing writing-mode xmlns:xlink x-height".split(" ").forEach(function(e) {
  var t = e.replace(
    Ui,
    bi
  );
  Ie[t] = new be(t, 1, !1, e, null, !1, !1);
});
"xlink:actuate xlink:arcrole xlink:role xlink:show xlink:title xlink:type".split(" ").forEach(function(e) {
  var t = e.replace(Ui, bi);
  Ie[t] = new be(t, 1, !1, e, "http://www.w3.org/1999/xlink", !1, !1);
});
["xml:base", "xml:lang", "xml:space"].forEach(function(e) {
  var t = e.replace(Ui, bi);
  Ie[t] = new be(t, 1, !1, e, "http://www.w3.org/XML/1998/namespace", !1, !1);
});
["tabIndex", "crossOrigin"].forEach(function(e) {
  Ie[e] = new be(e, 1, !1, e.toLowerCase(), null, !1, !1);
});
Ie.xlinkHref = new be("xlinkHref", 1, !1, "xlink:href", "http://www.w3.org/1999/xlink", !0, !1);
["src", "href", "action", "formAction"].forEach(function(e) {
  Ie[e] = new be(e, 1, !1, e.toLowerCase(), null, !0, !0);
});
function Vi(e, t, n, r) {
  var l = Ie.hasOwnProperty(t) ? Ie[t] : null;
  (l !== null ? l.type !== 0 : r || !(2 < t.length) || t[0] !== "o" && t[0] !== "O" || t[1] !== "n" && t[1] !== "N") && (Wd(t, n, l, r) && (n = null), r || l === null ? Bd(t) && (n === null ? e.removeAttribute(t) : e.setAttribute(t, "" + n)) : l.mustUseProperty ? e[l.propertyName] = n === null ? l.type === 3 ? !1 : "" : n : (t = l.attributeName, r = l.attributeNamespace, n === null ? e.removeAttribute(t) : (l = l.type, n = l === 3 || l === 4 && n === !0 ? "" : "" + n, r ? e.setAttributeNS(r, t, n) : e.setAttribute(t, n))));
}
var $t = bd.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED, qr = Symbol.for("react.element"), En = Symbol.for("react.portal"), In = Symbol.for("react.fragment"), Bi = Symbol.for("react.strict_mode"), Ba = Symbol.for("react.profiler"), pu = Symbol.for("react.provider"), mu = Symbol.for("react.context"), Hi = Symbol.for("react.forward_ref"), Ha = Symbol.for("react.suspense"), Wa = Symbol.for("react.suspense_list"), Wi = Symbol.for("react.memo"), Ut = Symbol.for("react.lazy"), hu = Symbol.for("react.offscreen"), Wo = Symbol.iterator;
function nr(e) {
  return e === null || typeof e != "object" ? null : (e = Wo && e[Wo] || e["@@iterator"], typeof e == "function" ? e : null);
}
var de = Object.assign, ma;
function cr(e) {
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
  return (e = e ? e.displayName || e.name : "") ? cr(e) : "";
}
function Qd(e) {
  switch (e.tag) {
    case 5:
      return cr(e.type);
    case 16:
      return cr("Lazy");
    case 13:
      return cr("Suspense");
    case 19:
      return cr("SuspenseList");
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
    case Ba:
      return "Profiler";
    case Bi:
      return "StrictMode";
    case Ha:
      return "Suspense";
    case Wa:
      return "SuspenseList";
  }
  if (typeof e == "object") switch (e.$$typeof) {
    case mu:
      return (e.displayName || "Context") + ".Consumer";
    case pu:
      return (e._context.displayName || "Context") + ".Provider";
    case Hi:
      var t = e.render;
      return e = e.displayName, e || (e = t.displayName || t.name || "", e = e !== "" ? "ForwardRef(" + e + ")" : "ForwardRef"), e;
    case Wi:
      return t = e.displayName || null, t !== null ? t : Qa(e.type) || "Memo";
    case Ut:
      t = e._payload, e = e._init;
      try {
        return Qa(e(t));
      } catch {
      }
  }
  return null;
}
function Gd(e) {
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
      return t === Bi ? "StrictMode" : "Mode";
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
function vu(e) {
  var t = e.type;
  return (e = e.nodeName) && e.toLowerCase() === "input" && (t === "checkbox" || t === "radio");
}
function Kd(e) {
  var t = vu(e) ? "checked" : "value", n = Object.getOwnPropertyDescriptor(e.constructor.prototype, t), r = "" + e[t];
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
  e._valueTracker || (e._valueTracker = Kd(e));
}
function xu(e) {
  if (!e) return !1;
  var t = e._valueTracker;
  if (!t) return !0;
  var n = t.getValue(), r = "";
  return e && (r = vu(e) ? e.checked ? "true" : "false" : e.value), e = r, e !== n ? (t.setValue(e), !0) : !1;
}
function wl(e) {
  if (e = e || (typeof document < "u" ? document : void 0), typeof e > "u") return null;
  try {
    return e.activeElement || e.body;
  } catch {
    return e.body;
  }
}
function Ga(e, t) {
  var n = t.checked;
  return de({}, t, { defaultChecked: void 0, defaultValue: void 0, value: void 0, checked: n ?? e._wrapperState.initialChecked });
}
function Qo(e, t) {
  var n = t.defaultValue == null ? "" : t.defaultValue, r = t.checked != null ? t.checked : t.defaultChecked;
  n = en(t.value != null ? t.value : n), e._wrapperState = { initialChecked: r, initialValue: n, controlled: t.type === "checkbox" || t.type === "radio" ? t.checked != null : t.value != null };
}
function gu(e, t) {
  t = t.checked, t != null && Vi(e, "checked", t, !1);
}
function Ka(e, t) {
  gu(e, t);
  var n = en(t.value), r = t.type;
  if (n != null) r === "number" ? (n === 0 && e.value === "" || e.value != n) && (e.value = "" + n) : e.value !== "" + n && (e.value = "" + n);
  else if (r === "submit" || r === "reset") {
    e.removeAttribute("value");
    return;
  }
  t.hasOwnProperty("value") ? qa(e, t.type, n) : t.hasOwnProperty("defaultValue") && qa(e, t.type, en(t.defaultValue)), t.checked == null && t.defaultChecked != null && (e.defaultChecked = !!t.defaultChecked);
}
function Go(e, t, n) {
  if (t.hasOwnProperty("value") || t.hasOwnProperty("defaultValue")) {
    var r = t.type;
    if (!(r !== "submit" && r !== "reset" || t.value !== void 0 && t.value !== null)) return;
    t = "" + e._wrapperState.initialValue, n || t === e.value || (e.value = t), e.defaultValue = t;
  }
  n = e.name, n !== "" && (e.name = ""), e.defaultChecked = !!e._wrapperState.initialChecked, n !== "" && (e.name = n);
}
function qa(e, t, n) {
  (t !== "number" || wl(e.ownerDocument) !== e) && (n == null ? e.defaultValue = "" + e._wrapperState.initialValue : e.defaultValue !== "" + n && (e.defaultValue = "" + n));
}
var dr = Array.isArray;
function On(e, t, n, r) {
  if (e = e.options, t) {
    t = {};
    for (var l = 0; l < n.length; l++) t["$" + n[l]] = !0;
    for (n = 0; n < e.length; n++) l = t.hasOwnProperty("$" + e[n].value), e[n].selected !== l && (e[n].selected = l), l && r && (e[n].defaultSelected = !0);
  } else {
    for (n = "" + en(n), t = null, l = 0; l < e.length; l++) {
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
  if (t.dangerouslySetInnerHTML != null) throw Error(_(91));
  return de({}, t, { value: void 0, defaultValue: void 0, children: "" + e._wrapperState.initialValue });
}
function Ko(e, t) {
  var n = t.value;
  if (n == null) {
    if (n = t.children, t = t.defaultValue, n != null) {
      if (t != null) throw Error(_(92));
      if (dr(n)) {
        if (1 < n.length) throw Error(_(93));
        n = n[0];
      }
      t = n;
    }
    t == null && (t = ""), n = t;
  }
  e._wrapperState = { initialValue: en(n) };
}
function yu(e, t) {
  var n = en(t.value), r = en(t.defaultValue);
  n != null && (n = "" + n, n !== e.value && (e.value = n), t.defaultValue == null && e.defaultValue !== n && (e.defaultValue = n)), r != null && (e.defaultValue = "" + r);
}
function qo(e) {
  var t = e.textContent;
  t === e._wrapperState.initialValue && t !== "" && t !== null && (e.value = t);
}
function ju(e) {
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
  return e == null || e === "http://www.w3.org/1999/xhtml" ? ju(t) : e === "http://www.w3.org/2000/svg" && t === "foreignObject" ? "http://www.w3.org/1999/xhtml" : e;
}
var Xr, Nu = function(e) {
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
}, qd = ["Webkit", "ms", "Moz", "O"];
Object.keys(mr).forEach(function(e) {
  qd.forEach(function(t) {
    t = t + e.charAt(0).toUpperCase() + e.substring(1), mr[t] = mr[e];
  });
});
function Su(e, t, n) {
  return t == null || typeof t == "boolean" || t === "" ? "" : n || typeof t != "number" || t === 0 || mr.hasOwnProperty(e) && mr[e] ? ("" + t).trim() : t + "px";
}
function wu(e, t) {
  e = e.style;
  for (var n in t) if (t.hasOwnProperty(n)) {
    var r = n.indexOf("--") === 0, l = Su(n, t[n], r);
    n === "float" && (n = "cssFloat"), r ? e.setProperty(n, l) : e[n] = l;
  }
}
var Yd = de({ menuitem: !0 }, { area: !0, base: !0, br: !0, col: !0, embed: !0, hr: !0, img: !0, input: !0, keygen: !0, link: !0, meta: !0, param: !0, source: !0, track: !0, wbr: !0 });
function Za(e, t) {
  if (t) {
    if (Yd[e] && (t.children != null || t.dangerouslySetInnerHTML != null)) throw Error(_(137, e));
    if (t.dangerouslySetInnerHTML != null) {
      if (t.children != null) throw Error(_(60));
      if (typeof t.dangerouslySetInnerHTML != "object" || !("__html" in t.dangerouslySetInnerHTML)) throw Error(_(61));
    }
    if (t.style != null && typeof t.style != "object") throw Error(_(62));
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
function Yo(e) {
  if (e = Hr(e)) {
    if (typeof ti != "function") throw Error(_(280));
    var t = e.stateNode;
    t && (t = Zl(t), ti(e.stateNode, e.type, t));
  }
}
function ku(e) {
  An ? Un ? Un.push(e) : Un = [e] : An = e;
}
function Cu() {
  if (An) {
    var e = An, t = Un;
    if (Un = An = null, Yo(e), t) for (e = 0; e < t.length; e++) Yo(t[e]);
  }
}
function Eu(e, t) {
  return e(t);
}
function Iu() {
}
var xa = !1;
function Pu(e, t, n) {
  if (xa) return e(t, n);
  xa = !0;
  try {
    return Eu(e, t, n);
  } finally {
    xa = !1, (An !== null || Un !== null) && (Iu(), Cu());
  }
}
function Er(e, t) {
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
  if (n && typeof n != "function") throw Error(_(231, t, typeof n));
  return n;
}
var ni = !1;
if (zt) try {
  var rr = {};
  Object.defineProperty(rr, "passive", { get: function() {
    ni = !0;
  } }), window.addEventListener("test", rr, rr), window.removeEventListener("test", rr, rr);
} catch {
  ni = !1;
}
function Xd(e, t, n, r, l, i, o, s, u) {
  var d = Array.prototype.slice.call(arguments, 3);
  try {
    t.apply(n, d);
  } catch (N) {
    this.onError(N);
  }
}
var hr = !1, kl = null, Cl = !1, ri = null, Zd = { onError: function(e) {
  hr = !0, kl = e;
} };
function Jd(e, t, n, r, l, i, o, s, u) {
  hr = !1, kl = null, Xd.apply(Zd, arguments);
}
function ef(e, t, n, r, l, i, o, s, u) {
  if (Jd.apply(this, arguments), hr) {
    if (hr) {
      var d = kl;
      hr = !1, kl = null;
    } else throw Error(_(198));
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
function Fu(e) {
  if (e.tag === 13) {
    var t = e.memoizedState;
    if (t === null && (e = e.alternate, e !== null && (t = e.memoizedState)), t !== null) return t.dehydrated;
  }
  return null;
}
function Xo(e) {
  if (Sn(e) !== e) throw Error(_(188));
}
function tf(e) {
  var t = e.alternate;
  if (!t) {
    if (t = Sn(e), t === null) throw Error(_(188));
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
        if (i === n) return Xo(l), e;
        if (i === r) return Xo(l), t;
        i = i.sibling;
      }
      throw Error(_(188));
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
        if (!o) throw Error(_(189));
      }
    }
    if (n.alternate !== r) throw Error(_(190));
  }
  if (n.tag !== 3) throw Error(_(188));
  return n.stateNode.current === n ? e : t;
}
function _u(e) {
  return e = tf(e), e !== null ? Ru(e) : null;
}
function Ru(e) {
  if (e.tag === 5 || e.tag === 6) return e;
  for (e = e.child; e !== null; ) {
    var t = Ru(e);
    if (t !== null) return t;
    e = e.sibling;
  }
  return null;
}
var Tu = Je.unstable_scheduleCallback, Zo = Je.unstable_cancelCallback, nf = Je.unstable_shouldYield, rf = Je.unstable_requestPaint, ve = Je.unstable_now, lf = Je.unstable_getCurrentPriorityLevel, Gi = Je.unstable_ImmediatePriority, zu = Je.unstable_UserBlockingPriority, El = Je.unstable_NormalPriority, af = Je.unstable_LowPriority, Lu = Je.unstable_IdlePriority, Kl = null, St = null;
function of(e) {
  if (St && typeof St.onCommitFiberRoot == "function") try {
    St.onCommitFiberRoot(Kl, e, void 0, (e.current.flags & 128) === 128);
  } catch {
  }
}
var ht = Math.clz32 ? Math.clz32 : cf, sf = Math.log, uf = Math.LN2;
function cf(e) {
  return e >>>= 0, e === 0 ? 32 : 31 - (sf(e) / uf | 0) | 0;
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
function Il(e, t) {
  var n = e.pendingLanes;
  if (n === 0) return 0;
  var r = 0, l = e.suspendedLanes, i = e.pingedLanes, o = n & 268435455;
  if (o !== 0) {
    var s = o & ~l;
    s !== 0 ? r = fr(s) : (i &= o, i !== 0 && (r = fr(i)));
  } else o = n & ~l, o !== 0 ? r = fr(o) : i !== 0 && (r = fr(i));
  if (r === 0) return 0;
  if (t !== 0 && t !== r && !(t & l) && (l = r & -r, i = t & -t, l >= i || l === 16 && (i & 4194240) !== 0)) return t;
  if (r & 4 && (r |= n & 16), t = e.entangledLanes, t !== 0) for (e = e.entanglements, t &= r; 0 < t; ) n = 31 - ht(t), l = 1 << n, r |= e[n], t &= ~l;
  return r;
}
function df(e, t) {
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
function ff(e, t) {
  for (var n = e.suspendedLanes, r = e.pingedLanes, l = e.expirationTimes, i = e.pendingLanes; 0 < i; ) {
    var o = 31 - ht(i), s = 1 << o, u = l[o];
    u === -1 ? (!(s & n) || s & r) && (l[o] = df(s, t)) : u <= t && (e.expiredLanes |= s), i &= ~s;
  }
}
function li(e) {
  return e = e.pendingLanes & -1073741825, e !== 0 ? e : e & 1073741824 ? 1073741824 : 0;
}
function Du() {
  var e = Zr;
  return Zr <<= 1, !(Zr & 4194240) && (Zr = 64), e;
}
function ga(e) {
  for (var t = [], n = 0; 31 > n; n++) t.push(e);
  return t;
}
function Vr(e, t, n) {
  e.pendingLanes |= t, t !== 536870912 && (e.suspendedLanes = 0, e.pingedLanes = 0), e = e.eventTimes, t = 31 - ht(t), e[t] = n;
}
function pf(e, t) {
  var n = e.pendingLanes & ~t;
  e.pendingLanes = t, e.suspendedLanes = 0, e.pingedLanes = 0, e.expiredLanes &= t, e.mutableReadLanes &= t, e.entangledLanes &= t, t = e.entanglements;
  var r = e.eventTimes;
  for (e = e.expirationTimes; 0 < n; ) {
    var l = 31 - ht(n), i = 1 << l;
    t[l] = 0, r[l] = -1, e[l] = -1, n &= ~i;
  }
}
function Ki(e, t) {
  var n = e.entangledLanes |= t;
  for (e = e.entanglements; n; ) {
    var r = 31 - ht(n), l = 1 << r;
    l & t | e[r] & t && (e[r] |= t), n &= ~l;
  }
}
var Z = 0;
function Mu(e) {
  return e &= -e, 1 < e ? 4 < e ? e & 268435455 ? 16 : 536870912 : 4 : 1;
}
var $u, qi, Ou, Au, Uu, ai = !1, el = [], Qt = null, Gt = null, Kt = null, Ir = /* @__PURE__ */ new Map(), Pr = /* @__PURE__ */ new Map(), Vt = [], mf = "mousedown mouseup touchcancel touchend touchstart auxclick dblclick pointercancel pointerdown pointerup dragend dragstart drop compositionend compositionstart keydown keypress keyup input textInput copy cut paste click change contextmenu reset submit".split(" ");
function Jo(e, t) {
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
      Ir.delete(t.pointerId);
      break;
    case "gotpointercapture":
    case "lostpointercapture":
      Pr.delete(t.pointerId);
  }
}
function lr(e, t, n, r, l, i) {
  return e === null || e.nativeEvent !== i ? (e = { blockedOn: t, domEventName: n, eventSystemFlags: r, nativeEvent: i, targetContainers: [l] }, t !== null && (t = Hr(t), t !== null && qi(t)), e) : (e.eventSystemFlags |= r, t = e.targetContainers, l !== null && t.indexOf(l) === -1 && t.push(l), e);
}
function hf(e, t, n, r, l) {
  switch (t) {
    case "focusin":
      return Qt = lr(Qt, e, t, n, r, l), !0;
    case "dragenter":
      return Gt = lr(Gt, e, t, n, r, l), !0;
    case "mouseover":
      return Kt = lr(Kt, e, t, n, r, l), !0;
    case "pointerover":
      var i = l.pointerId;
      return Ir.set(i, lr(Ir.get(i) || null, e, t, n, r, l)), !0;
    case "gotpointercapture":
      return i = l.pointerId, Pr.set(i, lr(Pr.get(i) || null, e, t, n, r, l)), !0;
  }
  return !1;
}
function bu(e) {
  var t = cn(e.target);
  if (t !== null) {
    var n = Sn(t);
    if (n !== null) {
      if (t = n.tag, t === 13) {
        if (t = Fu(n), t !== null) {
          e.blockedOn = t, Uu(e.priority, function() {
            Ou(n);
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
function es(e, t, n) {
  pl(e) && n.delete(t);
}
function vf() {
  ai = !1, Qt !== null && pl(Qt) && (Qt = null), Gt !== null && pl(Gt) && (Gt = null), Kt !== null && pl(Kt) && (Kt = null), Ir.forEach(es), Pr.forEach(es);
}
function ar(e, t) {
  e.blockedOn === t && (e.blockedOn = null, ai || (ai = !0, Je.unstable_scheduleCallback(Je.unstable_NormalPriority, vf)));
}
function Fr(e) {
  function t(l) {
    return ar(l, e);
  }
  if (0 < el.length) {
    ar(el[0], e);
    for (var n = 1; n < el.length; n++) {
      var r = el[n];
      r.blockedOn === e && (r.blockedOn = null);
    }
  }
  for (Qt !== null && ar(Qt, e), Gt !== null && ar(Gt, e), Kt !== null && ar(Kt, e), Ir.forEach(t), Pr.forEach(t), n = 0; n < Vt.length; n++) r = Vt[n], r.blockedOn === e && (r.blockedOn = null);
  for (; 0 < Vt.length && (n = Vt[0], n.blockedOn === null); ) bu(n), n.blockedOn === null && Vt.shift();
}
var bn = $t.ReactCurrentBatchConfig, Pl = !0;
function xf(e, t, n, r) {
  var l = Z, i = bn.transition;
  bn.transition = null;
  try {
    Z = 1, Yi(e, t, n, r);
  } finally {
    Z = l, bn.transition = i;
  }
}
function gf(e, t, n, r) {
  var l = Z, i = bn.transition;
  bn.transition = null;
  try {
    Z = 4, Yi(e, t, n, r);
  } finally {
    Z = l, bn.transition = i;
  }
}
function Yi(e, t, n, r) {
  if (Pl) {
    var l = ii(e, t, n, r);
    if (l === null) Pa(e, t, r, Fl, n), Jo(e, r);
    else if (hf(l, e, t, n, r)) r.stopPropagation();
    else if (Jo(e, r), t & 4 && -1 < mf.indexOf(e)) {
      for (; l !== null; ) {
        var i = Hr(l);
        if (i !== null && $u(i), i = ii(e, t, n, r), i === null && Pa(e, t, r, Fl, n), i === l) break;
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
    if (e = Fu(t), e !== null) return e;
    e = null;
  } else if (n === 3) {
    if (t.stateNode.current.memoizedState.isDehydrated) return t.tag === 3 ? t.stateNode.containerInfo : null;
    e = null;
  } else t !== e && (e = null);
  return Fl = e, null;
}
function Vu(e) {
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
      switch (lf()) {
        case Gi:
          return 1;
        case zu:
          return 4;
        case El:
        case af:
          return 16;
        case Lu:
          return 536870912;
        default:
          return 16;
      }
    default:
      return 16;
  }
}
var Ht = null, Xi = null, ml = null;
function Bu() {
  if (ml) return ml;
  var e, t = Xi, n = t.length, r, l = "value" in Ht ? Ht.value : Ht.textContent, i = l.length;
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
function ts() {
  return !1;
}
function tt(e) {
  function t(n, r, l, i, o) {
    this._reactName = n, this._targetInst = l, this.type = r, this.nativeEvent = i, this.target = o, this.currentTarget = null;
    for (var s in e) e.hasOwnProperty(s) && (n = e[s], this[s] = n ? n(i) : i[s]);
    return this.isDefaultPrevented = (i.defaultPrevented != null ? i.defaultPrevented : i.returnValue === !1) ? tl : ts, this.isPropagationStopped = ts, this;
  }
  return de(t.prototype, { preventDefault: function() {
    this.defaultPrevented = !0;
    var n = this.nativeEvent;
    n && (n.preventDefault ? n.preventDefault() : typeof n.returnValue != "unknown" && (n.returnValue = !1), this.isDefaultPrevented = tl);
  }, stopPropagation: function() {
    var n = this.nativeEvent;
    n && (n.stopPropagation ? n.stopPropagation() : typeof n.cancelBubble != "unknown" && (n.cancelBubble = !0), this.isPropagationStopped = tl);
  }, persist: function() {
  }, isPersistent: tl }), t;
}
var Jn = { eventPhase: 0, bubbles: 0, cancelable: 0, timeStamp: function(e) {
  return e.timeStamp || Date.now();
}, defaultPrevented: 0, isTrusted: 0 }, Zi = tt(Jn), Br = de({}, Jn, { view: 0, detail: 0 }), yf = tt(Br), ya, ja, ir, ql = de({}, Br, { screenX: 0, screenY: 0, clientX: 0, clientY: 0, pageX: 0, pageY: 0, ctrlKey: 0, shiftKey: 0, altKey: 0, metaKey: 0, getModifierState: Ji, button: 0, buttons: 0, relatedTarget: function(e) {
  return e.relatedTarget === void 0 ? e.fromElement === e.srcElement ? e.toElement : e.fromElement : e.relatedTarget;
}, movementX: function(e) {
  return "movementX" in e ? e.movementX : (e !== ir && (ir && e.type === "mousemove" ? (ya = e.screenX - ir.screenX, ja = e.screenY - ir.screenY) : ja = ya = 0, ir = e), ya);
}, movementY: function(e) {
  return "movementY" in e ? e.movementY : ja;
} }), ns = tt(ql), jf = de({}, ql, { dataTransfer: 0 }), Nf = tt(jf), Sf = de({}, Br, { relatedTarget: 0 }), Na = tt(Sf), wf = de({}, Jn, { animationName: 0, elapsedTime: 0, pseudoElement: 0 }), kf = tt(wf), Cf = de({}, Jn, { clipboardData: function(e) {
  return "clipboardData" in e ? e.clipboardData : window.clipboardData;
} }), Ef = tt(Cf), If = de({}, Jn, { data: 0 }), rs = tt(If), Pf = {
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
}, Ff = {
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
}, _f = { Alt: "altKey", Control: "ctrlKey", Meta: "metaKey", Shift: "shiftKey" };
function Rf(e) {
  var t = this.nativeEvent;
  return t.getModifierState ? t.getModifierState(e) : (e = _f[e]) ? !!t[e] : !1;
}
function Ji() {
  return Rf;
}
var Tf = de({}, Br, { key: function(e) {
  if (e.key) {
    var t = Pf[e.key] || e.key;
    if (t !== "Unidentified") return t;
  }
  return e.type === "keypress" ? (e = hl(e), e === 13 ? "Enter" : String.fromCharCode(e)) : e.type === "keydown" || e.type === "keyup" ? Ff[e.keyCode] || "Unidentified" : "";
}, code: 0, location: 0, ctrlKey: 0, shiftKey: 0, altKey: 0, metaKey: 0, repeat: 0, locale: 0, getModifierState: Ji, charCode: function(e) {
  return e.type === "keypress" ? hl(e) : 0;
}, keyCode: function(e) {
  return e.type === "keydown" || e.type === "keyup" ? e.keyCode : 0;
}, which: function(e) {
  return e.type === "keypress" ? hl(e) : e.type === "keydown" || e.type === "keyup" ? e.keyCode : 0;
} }), zf = tt(Tf), Lf = de({}, ql, { pointerId: 0, width: 0, height: 0, pressure: 0, tangentialPressure: 0, tiltX: 0, tiltY: 0, twist: 0, pointerType: 0, isPrimary: 0 }), ls = tt(Lf), Df = de({}, Br, { touches: 0, targetTouches: 0, changedTouches: 0, altKey: 0, metaKey: 0, ctrlKey: 0, shiftKey: 0, getModifierState: Ji }), Mf = tt(Df), $f = de({}, Jn, { propertyName: 0, elapsedTime: 0, pseudoElement: 0 }), Of = tt($f), Af = de({}, ql, {
  deltaX: function(e) {
    return "deltaX" in e ? e.deltaX : "wheelDeltaX" in e ? -e.wheelDeltaX : 0;
  },
  deltaY: function(e) {
    return "deltaY" in e ? e.deltaY : "wheelDeltaY" in e ? -e.wheelDeltaY : "wheelDelta" in e ? -e.wheelDelta : 0;
  },
  deltaZ: 0,
  deltaMode: 0
}), Uf = tt(Af), bf = [9, 13, 27, 32], eo = zt && "CompositionEvent" in window, vr = null;
zt && "documentMode" in document && (vr = document.documentMode);
var Vf = zt && "TextEvent" in window && !vr, Hu = zt && (!eo || vr && 8 < vr && 11 >= vr), as = " ", is = !1;
function Wu(e, t) {
  switch (e) {
    case "keyup":
      return bf.indexOf(t.keyCode) !== -1;
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
function Qu(e) {
  return e = e.detail, typeof e == "object" && "data" in e ? e.data : null;
}
var Pn = !1;
function Bf(e, t) {
  switch (e) {
    case "compositionend":
      return Qu(t);
    case "keypress":
      return t.which !== 32 ? null : (is = !0, as);
    case "textInput":
      return e = t.data, e === as && is ? null : e;
    default:
      return null;
  }
}
function Hf(e, t) {
  if (Pn) return e === "compositionend" || !eo && Wu(e, t) ? (e = Bu(), ml = Xi = Ht = null, Pn = !1, e) : null;
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
      return Hu && t.locale !== "ko" ? null : t.data;
    default:
      return null;
  }
}
var Wf = { color: !0, date: !0, datetime: !0, "datetime-local": !0, email: !0, month: !0, number: !0, password: !0, range: !0, search: !0, tel: !0, text: !0, time: !0, url: !0, week: !0 };
function os(e) {
  var t = e && e.nodeName && e.nodeName.toLowerCase();
  return t === "input" ? !!Wf[e.type] : t === "textarea";
}
function Gu(e, t, n, r) {
  ku(r), t = _l(t, "onChange"), 0 < t.length && (n = new Zi("onChange", "change", null, n, r), e.push({ event: n, listeners: t }));
}
var xr = null, _r = null;
function Qf(e) {
  lc(e, 0);
}
function Yl(e) {
  var t = Rn(e);
  if (xu(t)) return e;
}
function Gf(e, t) {
  if (e === "change") return t;
}
var Ku = !1;
if (zt) {
  var Sa;
  if (zt) {
    var wa = "oninput" in document;
    if (!wa) {
      var ss = document.createElement("div");
      ss.setAttribute("oninput", "return;"), wa = typeof ss.oninput == "function";
    }
    Sa = wa;
  } else Sa = !1;
  Ku = Sa && (!document.documentMode || 9 < document.documentMode);
}
function us() {
  xr && (xr.detachEvent("onpropertychange", qu), _r = xr = null);
}
function qu(e) {
  if (e.propertyName === "value" && Yl(_r)) {
    var t = [];
    Gu(t, _r, e, Qi(e)), Pu(Qf, t);
  }
}
function Kf(e, t, n) {
  e === "focusin" ? (us(), xr = t, _r = n, xr.attachEvent("onpropertychange", qu)) : e === "focusout" && us();
}
function qf(e) {
  if (e === "selectionchange" || e === "keyup" || e === "keydown") return Yl(_r);
}
function Yf(e, t) {
  if (e === "click") return Yl(t);
}
function Xf(e, t) {
  if (e === "input" || e === "change") return Yl(t);
}
function Zf(e, t) {
  return e === t && (e !== 0 || 1 / e === 1 / t) || e !== e && t !== t;
}
var gt = typeof Object.is == "function" ? Object.is : Zf;
function Rr(e, t) {
  if (gt(e, t)) return !0;
  if (typeof e != "object" || e === null || typeof t != "object" || t === null) return !1;
  var n = Object.keys(e), r = Object.keys(t);
  if (n.length !== r.length) return !1;
  for (r = 0; r < n.length; r++) {
    var l = n[r];
    if (!Va.call(t, l) || !gt(e[l], t[l])) return !1;
  }
  return !0;
}
function cs(e) {
  for (; e && e.firstChild; ) e = e.firstChild;
  return e;
}
function ds(e, t) {
  var n = cs(e);
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
    n = cs(n);
  }
}
function Yu(e, t) {
  return e && t ? e === t ? !0 : e && e.nodeType === 3 ? !1 : t && t.nodeType === 3 ? Yu(e, t.parentNode) : "contains" in e ? e.contains(t) : e.compareDocumentPosition ? !!(e.compareDocumentPosition(t) & 16) : !1 : !1;
}
function Xu() {
  for (var e = window, t = wl(); t instanceof e.HTMLIFrameElement; ) {
    try {
      var n = typeof t.contentWindow.location.href == "string";
    } catch {
      n = !1;
    }
    if (n) e = t.contentWindow;
    else break;
    t = wl(e.document);
  }
  return t;
}
function to(e) {
  var t = e && e.nodeName && e.nodeName.toLowerCase();
  return t && (t === "input" && (e.type === "text" || e.type === "search" || e.type === "tel" || e.type === "url" || e.type === "password") || t === "textarea" || e.contentEditable === "true");
}
function Jf(e) {
  var t = Xu(), n = e.focusedElem, r = e.selectionRange;
  if (t !== n && n && n.ownerDocument && Yu(n.ownerDocument.documentElement, n)) {
    if (r !== null && to(n)) {
      if (t = r.start, e = r.end, e === void 0 && (e = t), "selectionStart" in n) n.selectionStart = t, n.selectionEnd = Math.min(e, n.value.length);
      else if (e = (t = n.ownerDocument || document) && t.defaultView || window, e.getSelection) {
        e = e.getSelection();
        var l = n.textContent.length, i = Math.min(r.start, l);
        r = r.end === void 0 ? i : Math.min(r.end, l), !e.extend && i > r && (l = r, r = i, i = l), l = ds(n, i);
        var o = ds(
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
var ep = zt && "documentMode" in document && 11 >= document.documentMode, Fn = null, oi = null, gr = null, si = !1;
function fs(e, t, n) {
  var r = n.window === n ? n.document : n.nodeType === 9 ? n : n.ownerDocument;
  si || Fn == null || Fn !== wl(r) || (r = Fn, "selectionStart" in r && to(r) ? r = { start: r.selectionStart, end: r.selectionEnd } : (r = (r.ownerDocument && r.ownerDocument.defaultView || window).getSelection(), r = { anchorNode: r.anchorNode, anchorOffset: r.anchorOffset, focusNode: r.focusNode, focusOffset: r.focusOffset }), gr && Rr(gr, r) || (gr = r, r = _l(oi, "onSelect"), 0 < r.length && (t = new Zi("onSelect", "select", null, t, n), e.push({ event: t, listeners: r }), t.target = Fn)));
}
function nl(e, t) {
  var n = {};
  return n[e.toLowerCase()] = t.toLowerCase(), n["Webkit" + e] = "webkit" + t, n["Moz" + e] = "moz" + t, n;
}
var _n = { animationend: nl("Animation", "AnimationEnd"), animationiteration: nl("Animation", "AnimationIteration"), animationstart: nl("Animation", "AnimationStart"), transitionend: nl("Transition", "TransitionEnd") }, ka = {}, Zu = {};
zt && (Zu = document.createElement("div").style, "AnimationEvent" in window || (delete _n.animationend.animation, delete _n.animationiteration.animation, delete _n.animationstart.animation), "TransitionEvent" in window || delete _n.transitionend.transition);
function Xl(e) {
  if (ka[e]) return ka[e];
  if (!_n[e]) return e;
  var t = _n[e], n;
  for (n in t) if (t.hasOwnProperty(n) && n in Zu) return ka[e] = t[n];
  return e;
}
var Ju = Xl("animationend"), ec = Xl("animationiteration"), tc = Xl("animationstart"), nc = Xl("transitionend"), rc = /* @__PURE__ */ new Map(), ps = "abort auxClick cancel canPlay canPlayThrough click close contextMenu copy cut drag dragEnd dragEnter dragExit dragLeave dragOver dragStart drop durationChange emptied encrypted ended error gotPointerCapture input invalid keyDown keyPress keyUp load loadedData loadedMetadata loadStart lostPointerCapture mouseDown mouseMove mouseOut mouseOver mouseUp paste pause play playing pointerCancel pointerDown pointerMove pointerOut pointerOver pointerUp progress rateChange reset resize seeked seeking stalled submit suspend timeUpdate touchCancel touchEnd touchStart volumeChange scroll toggle touchMove waiting wheel".split(" ");
function rn(e, t) {
  rc.set(e, t), Nn(t, [e]);
}
for (var Ca = 0; Ca < ps.length; Ca++) {
  var Ea = ps[Ca], tp = Ea.toLowerCase(), np = Ea[0].toUpperCase() + Ea.slice(1);
  rn(tp, "on" + np);
}
rn(Ju, "onAnimationEnd");
rn(ec, "onAnimationIteration");
rn(tc, "onAnimationStart");
rn("dblclick", "onDoubleClick");
rn("focusin", "onFocus");
rn("focusout", "onBlur");
rn(nc, "onTransitionEnd");
Wn("onMouseEnter", ["mouseout", "mouseover"]);
Wn("onMouseLeave", ["mouseout", "mouseover"]);
Wn("onPointerEnter", ["pointerout", "pointerover"]);
Wn("onPointerLeave", ["pointerout", "pointerover"]);
Nn("onChange", "change click focusin focusout input keydown keyup selectionchange".split(" "));
Nn("onSelect", "focusout contextmenu dragend focusin keydown keyup mousedown mouseup selectionchange".split(" "));
Nn("onBeforeInput", ["compositionend", "keypress", "textInput", "paste"]);
Nn("onCompositionEnd", "compositionend focusout keydown keypress keyup mousedown".split(" "));
Nn("onCompositionStart", "compositionstart focusout keydown keypress keyup mousedown".split(" "));
Nn("onCompositionUpdate", "compositionupdate focusout keydown keypress keyup mousedown".split(" "));
var pr = "abort canplay canplaythrough durationchange emptied encrypted ended error loadeddata loadedmetadata loadstart pause play playing progress ratechange resize seeked seeking stalled suspend timeupdate volumechange waiting".split(" "), rp = new Set("cancel close invalid load scroll toggle".split(" ").concat(pr));
function ms(e, t, n) {
  var r = e.type || "unknown-event";
  e.currentTarget = n, ef(r, t, void 0, e), e.currentTarget = null;
}
function lc(e, t) {
  t = (t & 4) !== 0;
  for (var n = 0; n < e.length; n++) {
    var r = e[n], l = r.event;
    r = r.listeners;
    e: {
      var i = void 0;
      if (t) for (var o = r.length - 1; 0 <= o; o--) {
        var s = r[o], u = s.instance, d = s.currentTarget;
        if (s = s.listener, u !== i && l.isPropagationStopped()) break e;
        ms(l, s, d), i = u;
      }
      else for (o = 0; o < r.length; o++) {
        if (s = r[o], u = s.instance, d = s.currentTarget, s = s.listener, u !== i && l.isPropagationStopped()) break e;
        ms(l, s, d), i = u;
      }
    }
  }
  if (Cl) throw e = ri, Cl = !1, ri = null, e;
}
function le(e, t) {
  var n = t[pi];
  n === void 0 && (n = t[pi] = /* @__PURE__ */ new Set());
  var r = e + "__bubble";
  n.has(r) || (ac(t, e, 2, !1), n.add(r));
}
function Ia(e, t, n) {
  var r = 0;
  t && (r |= 4), ac(n, e, r, t);
}
var rl = "_reactListening" + Math.random().toString(36).slice(2);
function Tr(e) {
  if (!e[rl]) {
    e[rl] = !0, fu.forEach(function(n) {
      n !== "selectionchange" && (rp.has(n) || Ia(n, !1, e), Ia(n, !0, e));
    });
    var t = e.nodeType === 9 ? e : e.ownerDocument;
    t === null || t[rl] || (t[rl] = !0, Ia("selectionchange", !1, t));
  }
}
function ac(e, t, n, r) {
  switch (Vu(t)) {
    case 1:
      var l = xf;
      break;
    case 4:
      l = gf;
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
  Pu(function() {
    var d = i, N = Qi(n), c = [];
    e: {
      var h = rc.get(e);
      if (h !== void 0) {
        var x = Zi, y = e;
        switch (e) {
          case "keypress":
            if (hl(n) === 0) break e;
          case "keydown":
          case "keyup":
            x = zf;
            break;
          case "focusin":
            y = "focus", x = Na;
            break;
          case "focusout":
            y = "blur", x = Na;
            break;
          case "beforeblur":
          case "afterblur":
            x = Na;
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
            x = ns;
            break;
          case "drag":
          case "dragend":
          case "dragenter":
          case "dragexit":
          case "dragleave":
          case "dragover":
          case "dragstart":
          case "drop":
            x = Nf;
            break;
          case "touchcancel":
          case "touchend":
          case "touchmove":
          case "touchstart":
            x = Mf;
            break;
          case Ju:
          case ec:
          case tc:
            x = kf;
            break;
          case nc:
            x = Of;
            break;
          case "scroll":
            x = yf;
            break;
          case "wheel":
            x = Uf;
            break;
          case "copy":
          case "cut":
          case "paste":
            x = Ef;
            break;
          case "gotpointercapture":
          case "lostpointercapture":
          case "pointercancel":
          case "pointerdown":
          case "pointermove":
          case "pointerout":
          case "pointerover":
          case "pointerup":
            x = ls;
        }
        var S = (t & 4) !== 0, M = !S && e === "scroll", p = S ? h !== null ? h + "Capture" : null : h;
        S = [];
        for (var f = d, v; f !== null; ) {
          v = f;
          var C = v.stateNode;
          if (v.tag === 5 && C !== null && (v = C, p !== null && (C = Er(f, p), C != null && S.push(zr(f, C, v)))), M) break;
          f = f.return;
        }
        0 < S.length && (h = new x(h, y, null, n, N), c.push({ event: h, listeners: S }));
      }
    }
    if (!(t & 7)) {
      e: {
        if (h = e === "mouseover" || e === "pointerover", x = e === "mouseout" || e === "pointerout", h && n !== ei && (y = n.relatedTarget || n.fromElement) && (cn(y) || y[Lt])) break e;
        if ((x || h) && (h = N.window === N ? N : (h = N.ownerDocument) ? h.defaultView || h.parentWindow : window, x ? (y = n.relatedTarget || n.toElement, x = d, y = y ? cn(y) : null, y !== null && (M = Sn(y), y !== M || y.tag !== 5 && y.tag !== 6) && (y = null)) : (x = null, y = d), x !== y)) {
          if (S = ns, C = "onMouseLeave", p = "onMouseEnter", f = "mouse", (e === "pointerout" || e === "pointerover") && (S = ls, C = "onPointerLeave", p = "onPointerEnter", f = "pointer"), M = x == null ? h : Rn(x), v = y == null ? h : Rn(y), h = new S(C, f + "leave", x, n, N), h.target = M, h.relatedTarget = v, C = null, cn(N) === d && (S = new S(p, f + "enter", y, n, N), S.target = v, S.relatedTarget = M, C = S), M = C, x && y) t: {
            for (S = x, p = y, f = 0, v = S; v; v = Cn(v)) f++;
            for (v = 0, C = p; C; C = Cn(C)) v++;
            for (; 0 < f - v; ) S = Cn(S), f--;
            for (; 0 < v - f; ) p = Cn(p), v--;
            for (; f--; ) {
              if (S === p || p !== null && S === p.alternate) break t;
              S = Cn(S), p = Cn(p);
            }
            S = null;
          }
          else S = null;
          x !== null && hs(c, h, x, S, !1), y !== null && M !== null && hs(c, M, y, S, !0);
        }
      }
      e: {
        if (h = d ? Rn(d) : window, x = h.nodeName && h.nodeName.toLowerCase(), x === "select" || x === "input" && h.type === "file") var k = Gf;
        else if (os(h)) if (Ku) k = Xf;
        else {
          k = qf;
          var P = Kf;
        }
        else (x = h.nodeName) && x.toLowerCase() === "input" && (h.type === "checkbox" || h.type === "radio") && (k = Yf);
        if (k && (k = k(e, d))) {
          Gu(c, k, n, N);
          break e;
        }
        P && P(e, h, d), e === "focusout" && (P = h._wrapperState) && P.controlled && h.type === "number" && qa(h, "number", h.value);
      }
      switch (P = d ? Rn(d) : window, e) {
        case "focusin":
          (os(P) || P.contentEditable === "true") && (Fn = P, oi = d, gr = null);
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
          si = !1, fs(c, n, N);
          break;
        case "selectionchange":
          if (ep) break;
        case "keydown":
        case "keyup":
          fs(c, n, N);
      }
      var T;
      if (eo) e: {
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
      else Pn ? Wu(e, n) && (D = "onCompositionEnd") : e === "keydown" && n.keyCode === 229 && (D = "onCompositionStart");
      D && (Hu && n.locale !== "ko" && (Pn || D !== "onCompositionStart" ? D === "onCompositionEnd" && Pn && (T = Bu()) : (Ht = N, Xi = "value" in Ht ? Ht.value : Ht.textContent, Pn = !0)), P = _l(d, D), 0 < P.length && (D = new rs(D, e, null, n, N), c.push({ event: D, listeners: P }), T ? D.data = T : (T = Qu(n), T !== null && (D.data = T)))), (T = Vf ? Bf(e, n) : Hf(e, n)) && (d = _l(d, "onBeforeInput"), 0 < d.length && (N = new rs("onBeforeInput", "beforeinput", null, n, N), c.push({ event: N, listeners: d }), N.data = T));
    }
    lc(c, t);
  });
}
function zr(e, t, n) {
  return { instance: e, listener: t, currentTarget: n };
}
function _l(e, t) {
  for (var n = t + "Capture", r = []; e !== null; ) {
    var l = e, i = l.stateNode;
    l.tag === 5 && i !== null && (l = i, i = Er(e, n), i != null && r.unshift(zr(e, i, l)), i = Er(e, t), i != null && r.push(zr(e, i, l))), e = e.return;
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
function hs(e, t, n, r, l) {
  for (var i = t._reactName, o = []; n !== null && n !== r; ) {
    var s = n, u = s.alternate, d = s.stateNode;
    if (u !== null && u === r) break;
    s.tag === 5 && d !== null && (s = d, l ? (u = Er(n, i), u != null && o.unshift(zr(n, u, s))) : l || (u = Er(n, i), u != null && o.push(zr(n, u, s)))), n = n.return;
  }
  o.length !== 0 && e.push({ event: t, listeners: o });
}
var lp = /\r\n?/g, ap = /\u0000|\uFFFD/g;
function vs(e) {
  return (typeof e == "string" ? e : "" + e).replace(lp, `
`).replace(ap, "");
}
function ll(e, t, n) {
  if (t = vs(t), vs(e) !== t && n) throw Error(_(425));
}
function Rl() {
}
var ui = null, ci = null;
function di(e, t) {
  return e === "textarea" || e === "noscript" || typeof t.children == "string" || typeof t.children == "number" || typeof t.dangerouslySetInnerHTML == "object" && t.dangerouslySetInnerHTML !== null && t.dangerouslySetInnerHTML.__html != null;
}
var fi = typeof setTimeout == "function" ? setTimeout : void 0, ip = typeof clearTimeout == "function" ? clearTimeout : void 0, xs = typeof Promise == "function" ? Promise : void 0, op = typeof queueMicrotask == "function" ? queueMicrotask : typeof xs < "u" ? function(e) {
  return xs.resolve(null).then(e).catch(sp);
} : fi;
function sp(e) {
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
        e.removeChild(l), Fr(t);
        return;
      }
      r--;
    } else n !== "$" && n !== "$?" && n !== "$!" || r++;
    n = l;
  } while (n);
  Fr(t);
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
function gs(e) {
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
var er = Math.random().toString(36).slice(2), Nt = "__reactFiber$" + er, Lr = "__reactProps$" + er, Lt = "__reactContainer$" + er, pi = "__reactEvents$" + er, up = "__reactListeners$" + er, cp = "__reactHandles$" + er;
function cn(e) {
  var t = e[Nt];
  if (t) return t;
  for (var n = e.parentNode; n; ) {
    if (t = n[Lt] || n[Nt]) {
      if (n = t.alternate, t.child !== null || n !== null && n.child !== null) for (e = gs(e); e !== null; ) {
        if (n = e[Nt]) return n;
        e = gs(e);
      }
      return t;
    }
    e = n, n = e.parentNode;
  }
  return null;
}
function Hr(e) {
  return e = e[Nt] || e[Lt], !e || e.tag !== 5 && e.tag !== 6 && e.tag !== 13 && e.tag !== 3 ? null : e;
}
function Rn(e) {
  if (e.tag === 5 || e.tag === 6) return e.stateNode;
  throw Error(_(33));
}
function Zl(e) {
  return e[Lr] || null;
}
var mi = [], Tn = -1;
function ln(e) {
  return { current: e };
}
function ae(e) {
  0 > Tn || (e.current = mi[Tn], mi[Tn] = null, Tn--);
}
function ne(e, t) {
  Tn++, mi[Tn] = e.current, e.current = t;
}
var tn = {}, Le = ln(tn), We = ln(!1), vn = tn;
function Qn(e, t) {
  var n = e.type.contextTypes;
  if (!n) return tn;
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
  ae(We), ae(Le);
}
function ys(e, t, n) {
  if (Le.current !== tn) throw Error(_(168));
  ne(Le, t), ne(We, n);
}
function ic(e, t, n) {
  var r = e.stateNode;
  if (t = t.childContextTypes, typeof r.getChildContext != "function") return n;
  r = r.getChildContext();
  for (var l in r) if (!(l in t)) throw Error(_(108, Gd(e) || "Unknown", l));
  return de({}, n, r);
}
function zl(e) {
  return e = (e = e.stateNode) && e.__reactInternalMemoizedMergedChildContext || tn, vn = Le.current, ne(Le, e), ne(We, We.current), !0;
}
function js(e, t, n) {
  var r = e.stateNode;
  if (!r) throw Error(_(169));
  n ? (e = ic(e, t, vn), r.__reactInternalMemoizedMergedChildContext = e, ae(We), ae(Le), ne(Le, e)) : ae(We), ne(We, n);
}
var Ft = null, Jl = !1, _a = !1;
function oc(e) {
  Ft === null ? Ft = [e] : Ft.push(e);
}
function dp(e) {
  Jl = !0, oc(e);
}
function an() {
  if (!_a && Ft !== null) {
    _a = !0;
    var e = 0, t = Z;
    try {
      var n = Ft;
      for (Z = 1; e < n.length; e++) {
        var r = n[e];
        do
          r = r(!0);
        while (r !== null);
      }
      Ft = null, Jl = !1;
    } catch (l) {
      throw Ft !== null && (Ft = Ft.slice(e + 1)), Tu(Gi, an), l;
    } finally {
      Z = t, _a = !1;
    }
  }
  return null;
}
var zn = [], Ln = 0, Ll = null, Dl = 0, nt = [], rt = 0, xn = null, _t = 1, Rt = "";
function sn(e, t) {
  zn[Ln++] = Dl, zn[Ln++] = Ll, Ll = e, Dl = t;
}
function sc(e, t, n) {
  nt[rt++] = _t, nt[rt++] = Rt, nt[rt++] = xn, xn = e;
  var r = _t;
  e = Rt;
  var l = 32 - ht(r) - 1;
  r &= ~(1 << l), n += 1;
  var i = 32 - ht(t) + l;
  if (30 < i) {
    var o = l - l % 5;
    i = (r & (1 << o) - 1).toString(32), r >>= o, l -= o, _t = 1 << 32 - ht(t) + l | n << l | r, Rt = i + e;
  } else _t = 1 << i | n << l | r, Rt = e;
}
function no(e) {
  e.return !== null && (sn(e, 1), sc(e, 1, 0));
}
function ro(e) {
  for (; e === Ll; ) Ll = zn[--Ln], zn[Ln] = null, Dl = zn[--Ln], zn[Ln] = null;
  for (; e === xn; ) xn = nt[--rt], nt[rt] = null, Rt = nt[--rt], nt[rt] = null, _t = nt[--rt], nt[rt] = null;
}
var Ze = null, Xe = null, oe = !1, mt = null;
function uc(e, t) {
  var n = at(5, null, null, 0);
  n.elementType = "DELETED", n.stateNode = t, n.return = e, t = e.deletions, t === null ? (e.deletions = [n], e.flags |= 16) : t.push(n);
}
function Ns(e, t) {
  switch (e.tag) {
    case 5:
      var n = e.type;
      return t = t.nodeType !== 1 || n.toLowerCase() !== t.nodeName.toLowerCase() ? null : t, t !== null ? (e.stateNode = t, Ze = e, Xe = qt(t.firstChild), !0) : !1;
    case 6:
      return t = e.pendingProps === "" || t.nodeType !== 3 ? null : t, t !== null ? (e.stateNode = t, Ze = e, Xe = null, !0) : !1;
    case 13:
      return t = t.nodeType !== 8 ? null : t, t !== null ? (n = xn !== null ? { id: _t, overflow: Rt } : null, e.memoizedState = { dehydrated: t, treeContext: n, retryLane: 1073741824 }, n = at(18, null, null, 0), n.stateNode = t, n.return = e, e.child = n, Ze = e, Xe = null, !0) : !1;
    default:
      return !1;
  }
}
function hi(e) {
  return (e.mode & 1) !== 0 && (e.flags & 128) === 0;
}
function vi(e) {
  if (oe) {
    var t = Xe;
    if (t) {
      var n = t;
      if (!Ns(e, t)) {
        if (hi(e)) throw Error(_(418));
        t = qt(n.nextSibling);
        var r = Ze;
        t && Ns(e, t) ? uc(r, n) : (e.flags = e.flags & -4097 | 2, oe = !1, Ze = e);
      }
    } else {
      if (hi(e)) throw Error(_(418));
      e.flags = e.flags & -4097 | 2, oe = !1, Ze = e;
    }
  }
}
function Ss(e) {
  for (e = e.return; e !== null && e.tag !== 5 && e.tag !== 3 && e.tag !== 13; ) e = e.return;
  Ze = e;
}
function al(e) {
  if (e !== Ze) return !1;
  if (!oe) return Ss(e), oe = !0, !1;
  var t;
  if ((t = e.tag !== 3) && !(t = e.tag !== 5) && (t = e.type, t = t !== "head" && t !== "body" && !di(e.type, e.memoizedProps)), t && (t = Xe)) {
    if (hi(e)) throw cc(), Error(_(418));
    for (; t; ) uc(e, t), t = qt(t.nextSibling);
  }
  if (Ss(e), e.tag === 13) {
    if (e = e.memoizedState, e = e !== null ? e.dehydrated : null, !e) throw Error(_(317));
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
function cc() {
  for (var e = Xe; e; ) e = qt(e.nextSibling);
}
function Gn() {
  Xe = Ze = null, oe = !1;
}
function lo(e) {
  mt === null ? mt = [e] : mt.push(e);
}
var fp = $t.ReactCurrentBatchConfig;
function or(e, t, n) {
  if (e = n.ref, e !== null && typeof e != "function" && typeof e != "object") {
    if (n._owner) {
      if (n = n._owner, n) {
        if (n.tag !== 1) throw Error(_(309));
        var r = n.stateNode;
      }
      if (!r) throw Error(_(147, e));
      var l = r, i = "" + e;
      return t !== null && t.ref !== null && typeof t.ref == "function" && t.ref._stringRef === i ? t.ref : (t = function(o) {
        var s = l.refs;
        o === null ? delete s[i] : s[i] = o;
      }, t._stringRef = i, t);
    }
    if (typeof e != "string") throw Error(_(284));
    if (!n._owner) throw Error(_(290, e));
  }
  return e;
}
function il(e, t) {
  throw e = Object.prototype.toString.call(t), Error(_(31, e === "[object Object]" ? "object with keys {" + Object.keys(t).join(", ") + "}" : e));
}
function ws(e) {
  var t = e._init;
  return t(e._payload);
}
function dc(e) {
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
  function l(p, f) {
    return p = Jt(p, f), p.index = 0, p.sibling = null, p;
  }
  function i(p, f, v) {
    return p.index = v, e ? (v = p.alternate, v !== null ? (v = v.index, v < f ? (p.flags |= 2, f) : v) : (p.flags |= 2, f)) : (p.flags |= 1048576, f);
  }
  function o(p) {
    return e && p.alternate === null && (p.flags |= 2), p;
  }
  function s(p, f, v, C) {
    return f === null || f.tag !== 6 ? (f = $a(v, p.mode, C), f.return = p, f) : (f = l(f, v), f.return = p, f);
  }
  function u(p, f, v, C) {
    var k = v.type;
    return k === In ? N(p, f, v.props.children, C, v.key) : f !== null && (f.elementType === k || typeof k == "object" && k !== null && k.$$typeof === Ut && ws(k) === f.type) ? (C = l(f, v.props), C.ref = or(p, f, v), C.return = p, C) : (C = Sl(v.type, v.key, v.props, null, p.mode, C), C.ref = or(p, f, v), C.return = p, C);
  }
  function d(p, f, v, C) {
    return f === null || f.tag !== 4 || f.stateNode.containerInfo !== v.containerInfo || f.stateNode.implementation !== v.implementation ? (f = Oa(v, p.mode, C), f.return = p, f) : (f = l(f, v.children || []), f.return = p, f);
  }
  function N(p, f, v, C, k) {
    return f === null || f.tag !== 7 ? (f = mn(v, p.mode, C, k), f.return = p, f) : (f = l(f, v), f.return = p, f);
  }
  function c(p, f, v) {
    if (typeof f == "string" && f !== "" || typeof f == "number") return f = $a("" + f, p.mode, v), f.return = p, f;
    if (typeof f == "object" && f !== null) {
      switch (f.$$typeof) {
        case qr:
          return v = Sl(f.type, f.key, f.props, null, p.mode, v), v.ref = or(p, null, f), v.return = p, v;
        case En:
          return f = Oa(f, p.mode, v), f.return = p, f;
        case Ut:
          var C = f._init;
          return c(p, C(f._payload), v);
      }
      if (dr(f) || nr(f)) return f = mn(f, p.mode, v, null), f.return = p, f;
      il(p, f);
    }
    return null;
  }
  function h(p, f, v, C) {
    var k = f !== null ? f.key : null;
    if (typeof v == "string" && v !== "" || typeof v == "number") return k !== null ? null : s(p, f, "" + v, C);
    if (typeof v == "object" && v !== null) {
      switch (v.$$typeof) {
        case qr:
          return v.key === k ? u(p, f, v, C) : null;
        case En:
          return v.key === k ? d(p, f, v, C) : null;
        case Ut:
          return k = v._init, h(
            p,
            f,
            k(v._payload),
            C
          );
      }
      if (dr(v) || nr(v)) return k !== null ? null : N(p, f, v, C, null);
      il(p, v);
    }
    return null;
  }
  function x(p, f, v, C, k) {
    if (typeof C == "string" && C !== "" || typeof C == "number") return p = p.get(v) || null, s(f, p, "" + C, k);
    if (typeof C == "object" && C !== null) {
      switch (C.$$typeof) {
        case qr:
          return p = p.get(C.key === null ? v : C.key) || null, u(f, p, C, k);
        case En:
          return p = p.get(C.key === null ? v : C.key) || null, d(f, p, C, k);
        case Ut:
          var P = C._init;
          return x(p, f, v, P(C._payload), k);
      }
      if (dr(C) || nr(C)) return p = p.get(v) || null, N(f, p, C, k, null);
      il(f, C);
    }
    return null;
  }
  function y(p, f, v, C) {
    for (var k = null, P = null, T = f, D = f = 0, O = null; T !== null && D < v.length; D++) {
      T.index > D ? (O = T, T = null) : O = T.sibling;
      var U = h(p, T, v[D], C);
      if (U === null) {
        T === null && (T = O);
        break;
      }
      e && T && U.alternate === null && t(p, T), f = i(U, f, D), P === null ? k = U : P.sibling = U, P = U, T = O;
    }
    if (D === v.length) return n(p, T), oe && sn(p, D), k;
    if (T === null) {
      for (; D < v.length; D++) T = c(p, v[D], C), T !== null && (f = i(T, f, D), P === null ? k = T : P.sibling = T, P = T);
      return oe && sn(p, D), k;
    }
    for (T = r(p, T); D < v.length; D++) O = x(T, p, D, v[D], C), O !== null && (e && O.alternate !== null && T.delete(O.key === null ? D : O.key), f = i(O, f, D), P === null ? k = O : P.sibling = O, P = O);
    return e && T.forEach(function(R) {
      return t(p, R);
    }), oe && sn(p, D), k;
  }
  function S(p, f, v, C) {
    var k = nr(v);
    if (typeof k != "function") throw Error(_(150));
    if (v = k.call(v), v == null) throw Error(_(151));
    for (var P = k = null, T = f, D = f = 0, O = null, U = v.next(); T !== null && !U.done; D++, U = v.next()) {
      T.index > D ? (O = T, T = null) : O = T.sibling;
      var R = h(p, T, U.value, C);
      if (R === null) {
        T === null && (T = O);
        break;
      }
      e && T && R.alternate === null && t(p, T), f = i(R, f, D), P === null ? k = R : P.sibling = R, P = R, T = O;
    }
    if (U.done) return n(
      p,
      T
    ), oe && sn(p, D), k;
    if (T === null) {
      for (; !U.done; D++, U = v.next()) U = c(p, U.value, C), U !== null && (f = i(U, f, D), P === null ? k = U : P.sibling = U, P = U);
      return oe && sn(p, D), k;
    }
    for (T = r(p, T); !U.done; D++, U = v.next()) U = x(T, p, D, U.value, C), U !== null && (e && U.alternate !== null && T.delete(U.key === null ? D : U.key), f = i(U, f, D), P === null ? k = U : P.sibling = U, P = U);
    return e && T.forEach(function(Q) {
      return t(p, Q);
    }), oe && sn(p, D), k;
  }
  function M(p, f, v, C) {
    if (typeof v == "object" && v !== null && v.type === In && v.key === null && (v = v.props.children), typeof v == "object" && v !== null) {
      switch (v.$$typeof) {
        case qr:
          e: {
            for (var k = v.key, P = f; P !== null; ) {
              if (P.key === k) {
                if (k = v.type, k === In) {
                  if (P.tag === 7) {
                    n(p, P.sibling), f = l(P, v.props.children), f.return = p, p = f;
                    break e;
                  }
                } else if (P.elementType === k || typeof k == "object" && k !== null && k.$$typeof === Ut && ws(k) === P.type) {
                  n(p, P.sibling), f = l(P, v.props), f.ref = or(p, P, v), f.return = p, p = f;
                  break e;
                }
                n(p, P);
                break;
              } else t(p, P);
              P = P.sibling;
            }
            v.type === In ? (f = mn(v.props.children, p.mode, C, v.key), f.return = p, p = f) : (C = Sl(v.type, v.key, v.props, null, p.mode, C), C.ref = or(p, f, v), C.return = p, p = C);
          }
          return o(p);
        case En:
          e: {
            for (P = v.key; f !== null; ) {
              if (f.key === P) if (f.tag === 4 && f.stateNode.containerInfo === v.containerInfo && f.stateNode.implementation === v.implementation) {
                n(p, f.sibling), f = l(f, v.children || []), f.return = p, p = f;
                break e;
              } else {
                n(p, f);
                break;
              }
              else t(p, f);
              f = f.sibling;
            }
            f = Oa(v, p.mode, C), f.return = p, p = f;
          }
          return o(p);
        case Ut:
          return P = v._init, M(p, f, P(v._payload), C);
      }
      if (dr(v)) return y(p, f, v, C);
      if (nr(v)) return S(p, f, v, C);
      il(p, v);
    }
    return typeof v == "string" && v !== "" || typeof v == "number" ? (v = "" + v, f !== null && f.tag === 6 ? (n(p, f.sibling), f = l(f, v), f.return = p, p = f) : (n(p, f), f = $a(v, p.mode, C), f.return = p, p = f), o(p)) : n(p, f);
  }
  return M;
}
var Kn = dc(!0), fc = dc(!1), Ml = ln(null), $l = null, Dn = null, ao = null;
function io() {
  ao = Dn = $l = null;
}
function oo(e) {
  var t = Ml.current;
  ae(Ml), e._currentValue = t;
}
function xi(e, t, n) {
  for (; e !== null; ) {
    var r = e.alternate;
    if ((e.childLanes & t) !== t ? (e.childLanes |= t, r !== null && (r.childLanes |= t)) : r !== null && (r.childLanes & t) !== t && (r.childLanes |= t), e === n) break;
    e = e.return;
  }
}
function Vn(e, t) {
  $l = e, ao = Dn = null, e = e.dependencies, e !== null && e.firstContext !== null && (e.lanes & t && (He = !0), e.firstContext = null);
}
function ot(e) {
  var t = e._currentValue;
  if (ao !== e) if (e = { context: e, memoizedValue: t, next: null }, Dn === null) {
    if ($l === null) throw Error(_(308));
    Dn = e, $l.dependencies = { lanes: 0, firstContext: e };
  } else Dn = Dn.next = e;
  return t;
}
var dn = null;
function so(e) {
  dn === null ? dn = [e] : dn.push(e);
}
function pc(e, t, n, r) {
  var l = t.interleaved;
  return l === null ? (n.next = n, so(t)) : (n.next = l.next, l.next = n), t.interleaved = n, Dt(e, r);
}
function Dt(e, t) {
  e.lanes |= t;
  var n = e.alternate;
  for (n !== null && (n.lanes |= t), n = e, e = e.return; e !== null; ) e.childLanes |= t, n = e.alternate, n !== null && (n.childLanes |= t), n = e, e = e.return;
  return n.tag === 3 ? n.stateNode : null;
}
var bt = !1;
function uo(e) {
  e.updateQueue = { baseState: e.memoizedState, firstBaseUpdate: null, lastBaseUpdate: null, shared: { pending: null, interleaved: null, lanes: 0 }, effects: null };
}
function mc(e, t) {
  e = e.updateQueue, t.updateQueue === e && (t.updateQueue = { baseState: e.baseState, firstBaseUpdate: e.firstBaseUpdate, lastBaseUpdate: e.lastBaseUpdate, shared: e.shared, effects: e.effects });
}
function Tt(e, t) {
  return { eventTime: e, lane: t, tag: 0, payload: null, callback: null, next: null };
}
function Yt(e, t, n) {
  var r = e.updateQueue;
  if (r === null) return null;
  if (r = r.shared, Y & 2) {
    var l = r.pending;
    return l === null ? t.next = t : (t.next = l.next, l.next = t), r.pending = t, Dt(e, n);
  }
  return l = r.interleaved, l === null ? (t.next = t, so(r)) : (t.next = l.next, l.next = t), r.interleaved = t, Dt(e, n);
}
function vl(e, t, n) {
  if (t = t.updateQueue, t !== null && (t = t.shared, (n & 4194240) !== 0)) {
    var r = t.lanes;
    r &= e.pendingLanes, n |= r, t.lanes = n, Ki(e, n);
  }
}
function ks(e, t) {
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
  bt = !1;
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
      var h = s.lane, x = s.eventTime;
      if ((r & h) === h) {
        N !== null && (N = N.next = {
          eventTime: x,
          lane: 0,
          tag: s.tag,
          payload: s.payload,
          callback: s.callback,
          next: null
        });
        e: {
          var y = e, S = s;
          switch (h = t, x = n, S.tag) {
            case 1:
              if (y = S.payload, typeof y == "function") {
                c = y.call(x, c, h);
                break e;
              }
              c = y;
              break e;
            case 3:
              y.flags = y.flags & -65537 | 128;
            case 0:
              if (y = S.payload, h = typeof y == "function" ? y.call(x, c, h) : y, h == null) break e;
              c = de({}, c, h);
              break e;
            case 2:
              bt = !0;
          }
        }
        s.callback !== null && s.lane !== 0 && (e.flags |= 64, h = l.effects, h === null ? l.effects = [s] : h.push(s));
      } else x = { eventTime: x, lane: h, tag: s.tag, payload: s.payload, callback: s.callback, next: null }, N === null ? (d = N = x, u = c) : N = N.next = x, o |= h;
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
function Cs(e, t, n) {
  if (e = t.effects, t.effects = null, e !== null) for (t = 0; t < e.length; t++) {
    var r = e[t], l = r.callback;
    if (l !== null) {
      if (r.callback = null, r = n, typeof l != "function") throw Error(_(191, l));
      l.call(r);
    }
  }
}
var Wr = {}, wt = ln(Wr), Dr = ln(Wr), Mr = ln(Wr);
function fn(e) {
  if (e === Wr) throw Error(_(174));
  return e;
}
function co(e, t) {
  switch (ne(Mr, t), ne(Dr, e), ne(wt, Wr), e = t.nodeType, e) {
    case 9:
    case 11:
      t = (t = t.documentElement) ? t.namespaceURI : Xa(null, "");
      break;
    default:
      e = e === 8 ? t.parentNode : t, t = e.namespaceURI || null, e = e.tagName, t = Xa(t, e);
  }
  ae(wt), ne(wt, t);
}
function qn() {
  ae(wt), ae(Dr), ae(Mr);
}
function hc(e) {
  fn(Mr.current);
  var t = fn(wt.current), n = Xa(t, e.type);
  t !== n && (ne(Dr, e), ne(wt, n));
}
function fo(e) {
  Dr.current === e && (ae(wt), ae(Dr));
}
var ue = ln(0);
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
var xl = $t.ReactCurrentDispatcher, Ta = $t.ReactCurrentBatchConfig, gn = 0, ce = null, ye = null, Se = null, Ul = !1, yr = !1, $r = 0, pp = 0;
function _e() {
  throw Error(_(321));
}
function mo(e, t) {
  if (t === null) return !1;
  for (var n = 0; n < t.length && n < e.length; n++) if (!gt(e[n], t[n])) return !1;
  return !0;
}
function ho(e, t, n, r, l, i) {
  if (gn = i, ce = t, t.memoizedState = null, t.updateQueue = null, t.lanes = 0, xl.current = e === null || e.memoizedState === null ? xp : gp, e = n(r, l), yr) {
    i = 0;
    do {
      if (yr = !1, $r = 0, 25 <= i) throw Error(_(301));
      i += 1, Se = ye = null, t.updateQueue = null, xl.current = yp, e = n(r, l);
    } while (yr);
  }
  if (xl.current = bl, t = ye !== null && ye.next !== null, gn = 0, Se = ye = ce = null, Ul = !1, t) throw Error(_(300));
  return e;
}
function vo() {
  var e = $r !== 0;
  return $r = 0, e;
}
function jt() {
  var e = { memoizedState: null, baseState: null, baseQueue: null, queue: null, next: null };
  return Se === null ? ce.memoizedState = Se = e : Se = Se.next = e, Se;
}
function st() {
  if (ye === null) {
    var e = ce.alternate;
    e = e !== null ? e.memoizedState : null;
  } else e = ye.next;
  var t = Se === null ? ce.memoizedState : Se.next;
  if (t !== null) Se = t, ye = e;
  else {
    if (e === null) throw Error(_(310));
    ye = e, e = { memoizedState: ye.memoizedState, baseState: ye.baseState, baseQueue: ye.baseQueue, queue: ye.queue, next: null }, Se === null ? ce.memoizedState = Se = e : Se = Se.next = e;
  }
  return Se;
}
function Or(e, t) {
  return typeof t == "function" ? t(e) : t;
}
function za(e) {
  var t = st(), n = t.queue;
  if (n === null) throw Error(_(311));
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
      if ((gn & N) === N) u !== null && (u = u.next = { lane: 0, action: d.action, hasEagerState: d.hasEagerState, eagerState: d.eagerState, next: null }), r = d.hasEagerState ? d.eagerState : e(r, d.action);
      else {
        var c = {
          lane: N,
          action: d.action,
          hasEagerState: d.hasEagerState,
          eagerState: d.eagerState,
          next: null
        };
        u === null ? (s = u = c, o = r) : u = u.next = c, ce.lanes |= N, yn |= N;
      }
      d = d.next;
    } while (d !== null && d !== i);
    u === null ? o = r : u.next = s, gt(r, t.memoizedState) || (He = !0), t.memoizedState = r, t.baseState = o, t.baseQueue = u, n.lastRenderedState = r;
  }
  if (e = n.interleaved, e !== null) {
    l = e;
    do
      i = l.lane, ce.lanes |= i, yn |= i, l = l.next;
    while (l !== e);
  } else l === null && (n.lanes = 0);
  return [t.memoizedState, n.dispatch];
}
function La(e) {
  var t = st(), n = t.queue;
  if (n === null) throw Error(_(311));
  n.lastRenderedReducer = e;
  var r = n.dispatch, l = n.pending, i = t.memoizedState;
  if (l !== null) {
    n.pending = null;
    var o = l = l.next;
    do
      i = e(i, o.action), o = o.next;
    while (o !== l);
    gt(i, t.memoizedState) || (He = !0), t.memoizedState = i, t.baseQueue === null && (t.baseState = i), n.lastRenderedState = i;
  }
  return [i, r];
}
function vc() {
}
function xc(e, t) {
  var n = ce, r = st(), l = t(), i = !gt(r.memoizedState, l);
  if (i && (r.memoizedState = l, He = !0), r = r.queue, xo(jc.bind(null, n, r, e), [e]), r.getSnapshot !== t || i || Se !== null && Se.memoizedState.tag & 1) {
    if (n.flags |= 2048, Ar(9, yc.bind(null, n, r, l, t), void 0, null), ke === null) throw Error(_(349));
    gn & 30 || gc(n, t, l);
  }
  return l;
}
function gc(e, t, n) {
  e.flags |= 16384, e = { getSnapshot: t, value: n }, t = ce.updateQueue, t === null ? (t = { lastEffect: null, stores: null }, ce.updateQueue = t, t.stores = [e]) : (n = t.stores, n === null ? t.stores = [e] : n.push(e));
}
function yc(e, t, n, r) {
  t.value = n, t.getSnapshot = r, Nc(t) && Sc(e);
}
function jc(e, t, n) {
  return n(function() {
    Nc(t) && Sc(e);
  });
}
function Nc(e) {
  var t = e.getSnapshot;
  e = e.value;
  try {
    var n = t();
    return !gt(e, n);
  } catch {
    return !0;
  }
}
function Sc(e) {
  var t = Dt(e, 1);
  t !== null && vt(t, e, 1, -1);
}
function Es(e) {
  var t = jt();
  return typeof e == "function" && (e = e()), t.memoizedState = t.baseState = e, e = { pending: null, interleaved: null, lanes: 0, dispatch: null, lastRenderedReducer: Or, lastRenderedState: e }, t.queue = e, e = e.dispatch = vp.bind(null, ce, e), [t.memoizedState, e];
}
function Ar(e, t, n, r) {
  return e = { tag: e, create: t, destroy: n, deps: r, next: null }, t = ce.updateQueue, t === null ? (t = { lastEffect: null, stores: null }, ce.updateQueue = t, t.lastEffect = e.next = e) : (n = t.lastEffect, n === null ? t.lastEffect = e.next = e : (r = n.next, n.next = e, e.next = r, t.lastEffect = e)), e;
}
function wc() {
  return st().memoizedState;
}
function gl(e, t, n, r) {
  var l = jt();
  ce.flags |= e, l.memoizedState = Ar(1 | t, n, void 0, r === void 0 ? null : r);
}
function ea(e, t, n, r) {
  var l = st();
  r = r === void 0 ? null : r;
  var i = void 0;
  if (ye !== null) {
    var o = ye.memoizedState;
    if (i = o.destroy, r !== null && mo(r, o.deps)) {
      l.memoizedState = Ar(t, n, i, r);
      return;
    }
  }
  ce.flags |= e, l.memoizedState = Ar(1 | t, n, i, r);
}
function Is(e, t) {
  return gl(8390656, 8, e, t);
}
function xo(e, t) {
  return ea(2048, 8, e, t);
}
function kc(e, t) {
  return ea(4, 2, e, t);
}
function Cc(e, t) {
  return ea(4, 4, e, t);
}
function Ec(e, t) {
  if (typeof t == "function") return e = e(), t(e), function() {
    t(null);
  };
  if (t != null) return e = e(), t.current = e, function() {
    t.current = null;
  };
}
function Ic(e, t, n) {
  return n = n != null ? n.concat([e]) : null, ea(4, 4, Ec.bind(null, t, e), n);
}
function go() {
}
function Pc(e, t) {
  var n = st();
  t = t === void 0 ? null : t;
  var r = n.memoizedState;
  return r !== null && t !== null && mo(t, r[1]) ? r[0] : (n.memoizedState = [e, t], e);
}
function Fc(e, t) {
  var n = st();
  t = t === void 0 ? null : t;
  var r = n.memoizedState;
  return r !== null && t !== null && mo(t, r[1]) ? r[0] : (e = e(), n.memoizedState = [e, t], e);
}
function _c(e, t, n) {
  return gn & 21 ? (gt(n, t) || (n = Du(), ce.lanes |= n, yn |= n, e.baseState = !0), t) : (e.baseState && (e.baseState = !1, He = !0), e.memoizedState = n);
}
function mp(e, t) {
  var n = Z;
  Z = n !== 0 && 4 > n ? n : 4, e(!0);
  var r = Ta.transition;
  Ta.transition = {};
  try {
    e(!1), t();
  } finally {
    Z = n, Ta.transition = r;
  }
}
function Rc() {
  return st().memoizedState;
}
function hp(e, t, n) {
  var r = Zt(e);
  if (n = { lane: r, action: n, hasEagerState: !1, eagerState: null, next: null }, Tc(e)) zc(t, n);
  else if (n = pc(e, t, n, r), n !== null) {
    var l = Ae();
    vt(n, e, r, l), Lc(n, t, r);
  }
}
function vp(e, t, n) {
  var r = Zt(e), l = { lane: r, action: n, hasEagerState: !1, eagerState: null, next: null };
  if (Tc(e)) zc(t, l);
  else {
    var i = e.alternate;
    if (e.lanes === 0 && (i === null || i.lanes === 0) && (i = t.lastRenderedReducer, i !== null)) try {
      var o = t.lastRenderedState, s = i(o, n);
      if (l.hasEagerState = !0, l.eagerState = s, gt(s, o)) {
        var u = t.interleaved;
        u === null ? (l.next = l, so(t)) : (l.next = u.next, u.next = l), t.interleaved = l;
        return;
      }
    } catch {
    } finally {
    }
    n = pc(e, t, l, r), n !== null && (l = Ae(), vt(n, e, r, l), Lc(n, t, r));
  }
}
function Tc(e) {
  var t = e.alternate;
  return e === ce || t !== null && t === ce;
}
function zc(e, t) {
  yr = Ul = !0;
  var n = e.pending;
  n === null ? t.next = t : (t.next = n.next, n.next = t), e.pending = t;
}
function Lc(e, t, n) {
  if (n & 4194240) {
    var r = t.lanes;
    r &= e.pendingLanes, n |= r, t.lanes = n, Ki(e, n);
  }
}
var bl = { readContext: ot, useCallback: _e, useContext: _e, useEffect: _e, useImperativeHandle: _e, useInsertionEffect: _e, useLayoutEffect: _e, useMemo: _e, useReducer: _e, useRef: _e, useState: _e, useDebugValue: _e, useDeferredValue: _e, useTransition: _e, useMutableSource: _e, useSyncExternalStore: _e, useId: _e, unstable_isNewReconciler: !1 }, xp = { readContext: ot, useCallback: function(e, t) {
  return jt().memoizedState = [e, t === void 0 ? null : t], e;
}, useContext: ot, useEffect: Is, useImperativeHandle: function(e, t, n) {
  return n = n != null ? n.concat([e]) : null, gl(
    4194308,
    4,
    Ec.bind(null, t, e),
    n
  );
}, useLayoutEffect: function(e, t) {
  return gl(4194308, 4, e, t);
}, useInsertionEffect: function(e, t) {
  return gl(4, 2, e, t);
}, useMemo: function(e, t) {
  var n = jt();
  return t = t === void 0 ? null : t, e = e(), n.memoizedState = [e, t], e;
}, useReducer: function(e, t, n) {
  var r = jt();
  return t = n !== void 0 ? n(t) : t, r.memoizedState = r.baseState = t, e = { pending: null, interleaved: null, lanes: 0, dispatch: null, lastRenderedReducer: e, lastRenderedState: t }, r.queue = e, e = e.dispatch = hp.bind(null, ce, e), [r.memoizedState, e];
}, useRef: function(e) {
  var t = jt();
  return e = { current: e }, t.memoizedState = e;
}, useState: Es, useDebugValue: go, useDeferredValue: function(e) {
  return jt().memoizedState = e;
}, useTransition: function() {
  var e = Es(!1), t = e[0];
  return e = mp.bind(null, e[1]), jt().memoizedState = e, [t, e];
}, useMutableSource: function() {
}, useSyncExternalStore: function(e, t, n) {
  var r = ce, l = jt();
  if (oe) {
    if (n === void 0) throw Error(_(407));
    n = n();
  } else {
    if (n = t(), ke === null) throw Error(_(349));
    gn & 30 || gc(r, t, n);
  }
  l.memoizedState = n;
  var i = { value: n, getSnapshot: t };
  return l.queue = i, Is(jc.bind(
    null,
    r,
    i,
    e
  ), [e]), r.flags |= 2048, Ar(9, yc.bind(null, r, i, n, t), void 0, null), n;
}, useId: function() {
  var e = jt(), t = ke.identifierPrefix;
  if (oe) {
    var n = Rt, r = _t;
    n = (r & ~(1 << 32 - ht(r) - 1)).toString(32) + n, t = ":" + t + "R" + n, n = $r++, 0 < n && (t += "H" + n.toString(32)), t += ":";
  } else n = pp++, t = ":" + t + "r" + n.toString(32) + ":";
  return e.memoizedState = t;
}, unstable_isNewReconciler: !1 }, gp = {
  readContext: ot,
  useCallback: Pc,
  useContext: ot,
  useEffect: xo,
  useImperativeHandle: Ic,
  useInsertionEffect: kc,
  useLayoutEffect: Cc,
  useMemo: Fc,
  useReducer: za,
  useRef: wc,
  useState: function() {
    return za(Or);
  },
  useDebugValue: go,
  useDeferredValue: function(e) {
    var t = st();
    return _c(t, ye.memoizedState, e);
  },
  useTransition: function() {
    var e = za(Or)[0], t = st().memoizedState;
    return [e, t];
  },
  useMutableSource: vc,
  useSyncExternalStore: xc,
  useId: Rc,
  unstable_isNewReconciler: !1
}, yp = { readContext: ot, useCallback: Pc, useContext: ot, useEffect: xo, useImperativeHandle: Ic, useInsertionEffect: kc, useLayoutEffect: Cc, useMemo: Fc, useReducer: La, useRef: wc, useState: function() {
  return La(Or);
}, useDebugValue: go, useDeferredValue: function(e) {
  var t = st();
  return ye === null ? t.memoizedState = e : _c(t, ye.memoizedState, e);
}, useTransition: function() {
  var e = La(Or)[0], t = st().memoizedState;
  return [e, t];
}, useMutableSource: vc, useSyncExternalStore: xc, useId: Rc, unstable_isNewReconciler: !1 };
function ft(e, t) {
  if (e && e.defaultProps) {
    t = de({}, t), e = e.defaultProps;
    for (var n in e) t[n] === void 0 && (t[n] = e[n]);
    return t;
  }
  return t;
}
function gi(e, t, n, r) {
  t = e.memoizedState, n = n(r, t), n = n == null ? t : de({}, t, n), e.memoizedState = n, e.lanes === 0 && (e.updateQueue.baseState = n);
}
var ta = { isMounted: function(e) {
  return (e = e._reactInternals) ? Sn(e) === e : !1;
}, enqueueSetState: function(e, t, n) {
  e = e._reactInternals;
  var r = Ae(), l = Zt(e), i = Tt(r, l);
  i.payload = t, n != null && (i.callback = n), t = Yt(e, i, l), t !== null && (vt(t, e, l, r), vl(t, e, l));
}, enqueueReplaceState: function(e, t, n) {
  e = e._reactInternals;
  var r = Ae(), l = Zt(e), i = Tt(r, l);
  i.tag = 1, i.payload = t, n != null && (i.callback = n), t = Yt(e, i, l), t !== null && (vt(t, e, l, r), vl(t, e, l));
}, enqueueForceUpdate: function(e, t) {
  e = e._reactInternals;
  var n = Ae(), r = Zt(e), l = Tt(n, r);
  l.tag = 2, t != null && (l.callback = t), t = Yt(e, l, r), t !== null && (vt(t, e, r, n), vl(t, e, r));
} };
function Ps(e, t, n, r, l, i, o) {
  return e = e.stateNode, typeof e.shouldComponentUpdate == "function" ? e.shouldComponentUpdate(r, i, o) : t.prototype && t.prototype.isPureReactComponent ? !Rr(n, r) || !Rr(l, i) : !0;
}
function Dc(e, t, n) {
  var r = !1, l = tn, i = t.contextType;
  return typeof i == "object" && i !== null ? i = ot(i) : (l = Qe(t) ? vn : Le.current, r = t.contextTypes, i = (r = r != null) ? Qn(e, l) : tn), t = new t(n, i), e.memoizedState = t.state !== null && t.state !== void 0 ? t.state : null, t.updater = ta, e.stateNode = t, t._reactInternals = e, r && (e = e.stateNode, e.__reactInternalMemoizedUnmaskedChildContext = l, e.__reactInternalMemoizedMaskedChildContext = i), t;
}
function Fs(e, t, n, r) {
  e = t.state, typeof t.componentWillReceiveProps == "function" && t.componentWillReceiveProps(n, r), typeof t.UNSAFE_componentWillReceiveProps == "function" && t.UNSAFE_componentWillReceiveProps(n, r), t.state !== e && ta.enqueueReplaceState(t, t.state, null);
}
function yi(e, t, n, r) {
  var l = e.stateNode;
  l.props = n, l.state = e.memoizedState, l.refs = {}, uo(e);
  var i = t.contextType;
  typeof i == "object" && i !== null ? l.context = ot(i) : (i = Qe(t) ? vn : Le.current, l.context = Qn(e, i)), l.state = e.memoizedState, i = t.getDerivedStateFromProps, typeof i == "function" && (gi(e, t, i, n), l.state = e.memoizedState), typeof t.getDerivedStateFromProps == "function" || typeof l.getSnapshotBeforeUpdate == "function" || typeof l.UNSAFE_componentWillMount != "function" && typeof l.componentWillMount != "function" || (t = l.state, typeof l.componentWillMount == "function" && l.componentWillMount(), typeof l.UNSAFE_componentWillMount == "function" && l.UNSAFE_componentWillMount(), t !== l.state && ta.enqueueReplaceState(l, l.state, null), Ol(e, n, l, r), l.state = e.memoizedState), typeof l.componentDidMount == "function" && (e.flags |= 4194308);
}
function Yn(e, t) {
  try {
    var n = "", r = t;
    do
      n += Qd(r), r = r.return;
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
var jp = typeof WeakMap == "function" ? WeakMap : Map;
function Mc(e, t, n) {
  n = Tt(-1, n), n.tag = 3, n.payload = { element: null };
  var r = t.value;
  return n.callback = function() {
    Bl || (Bl = !0, _i = r), ji(e, t);
  }, n;
}
function $c(e, t, n) {
  n = Tt(-1, n), n.tag = 3;
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
    ji(e, t), typeof r != "function" && (Xt === null ? Xt = /* @__PURE__ */ new Set([this]) : Xt.add(this));
    var o = t.stack;
    this.componentDidCatch(t.value, { componentStack: o !== null ? o : "" });
  }), n;
}
function _s(e, t, n) {
  var r = e.pingCache;
  if (r === null) {
    r = e.pingCache = new jp();
    var l = /* @__PURE__ */ new Set();
    r.set(t, l);
  } else l = r.get(t), l === void 0 && (l = /* @__PURE__ */ new Set(), r.set(t, l));
  l.has(n) || (l.add(n), e = Lp.bind(null, e, t, n), t.then(e, e));
}
function Rs(e) {
  do {
    var t;
    if ((t = e.tag === 13) && (t = e.memoizedState, t = t !== null ? t.dehydrated !== null : !0), t) return e;
    e = e.return;
  } while (e !== null);
  return null;
}
function Ts(e, t, n, r, l) {
  return e.mode & 1 ? (e.flags |= 65536, e.lanes = l, e) : (e === t ? e.flags |= 65536 : (e.flags |= 128, n.flags |= 131072, n.flags &= -52805, n.tag === 1 && (n.alternate === null ? n.tag = 17 : (t = Tt(-1, 1), t.tag = 2, Yt(n, t, 1))), n.lanes |= 1), e);
}
var Np = $t.ReactCurrentOwner, He = !1;
function Me(e, t, n, r) {
  t.child = e === null ? fc(t, null, n, r) : Kn(t, e.child, n, r);
}
function zs(e, t, n, r, l) {
  n = n.render;
  var i = t.ref;
  return Vn(t, l), r = ho(e, t, n, r, i, l), n = vo(), e !== null && !He ? (t.updateQueue = e.updateQueue, t.flags &= -2053, e.lanes &= ~l, Mt(e, t, l)) : (oe && n && no(t), t.flags |= 1, Me(e, t, r, l), t.child);
}
function Ls(e, t, n, r, l) {
  if (e === null) {
    var i = n.type;
    return typeof i == "function" && !Eo(i) && i.defaultProps === void 0 && n.compare === null && n.defaultProps === void 0 ? (t.tag = 15, t.type = i, Oc(e, t, i, r, l)) : (e = Sl(n.type, null, r, t, t.mode, l), e.ref = t.ref, e.return = t, t.child = e);
  }
  if (i = e.child, !(e.lanes & l)) {
    var o = i.memoizedProps;
    if (n = n.compare, n = n !== null ? n : Rr, n(o, r) && e.ref === t.ref) return Mt(e, t, l);
  }
  return t.flags |= 1, e = Jt(i, r), e.ref = t.ref, e.return = t, t.child = e;
}
function Oc(e, t, n, r, l) {
  if (e !== null) {
    var i = e.memoizedProps;
    if (Rr(i, r) && e.ref === t.ref) if (He = !1, t.pendingProps = r = i, (e.lanes & l) !== 0) e.flags & 131072 && (He = !0);
    else return t.lanes = e.lanes, Mt(e, t, l);
  }
  return Ni(e, t, n, r, l);
}
function Ac(e, t, n) {
  var r = t.pendingProps, l = r.children, i = e !== null ? e.memoizedState : null;
  if (r.mode === "hidden") if (!(t.mode & 1)) t.memoizedState = { baseLanes: 0, cachePool: null, transitions: null }, ne($n, Ye), Ye |= n;
  else {
    if (!(n & 1073741824)) return e = i !== null ? i.baseLanes | n : n, t.lanes = t.childLanes = 1073741824, t.memoizedState = { baseLanes: e, cachePool: null, transitions: null }, t.updateQueue = null, ne($n, Ye), Ye |= e, null;
    t.memoizedState = { baseLanes: 0, cachePool: null, transitions: null }, r = i !== null ? i.baseLanes : n, ne($n, Ye), Ye |= r;
  }
  else i !== null ? (r = i.baseLanes | n, t.memoizedState = null) : r = n, ne($n, Ye), Ye |= r;
  return Me(e, t, l, n), t.child;
}
function Uc(e, t) {
  var n = t.ref;
  (e === null && n !== null || e !== null && e.ref !== n) && (t.flags |= 512, t.flags |= 2097152);
}
function Ni(e, t, n, r, l) {
  var i = Qe(n) ? vn : Le.current;
  return i = Qn(t, i), Vn(t, l), n = ho(e, t, n, r, i, l), r = vo(), e !== null && !He ? (t.updateQueue = e.updateQueue, t.flags &= -2053, e.lanes &= ~l, Mt(e, t, l)) : (oe && r && no(t), t.flags |= 1, Me(e, t, n, l), t.child);
}
function Ds(e, t, n, r, l) {
  if (Qe(n)) {
    var i = !0;
    zl(t);
  } else i = !1;
  if (Vn(t, l), t.stateNode === null) yl(e, t), Dc(t, n, r), yi(t, n, r, l), r = !0;
  else if (e === null) {
    var o = t.stateNode, s = t.memoizedProps;
    o.props = s;
    var u = o.context, d = n.contextType;
    typeof d == "object" && d !== null ? d = ot(d) : (d = Qe(n) ? vn : Le.current, d = Qn(t, d));
    var N = n.getDerivedStateFromProps, c = typeof N == "function" || typeof o.getSnapshotBeforeUpdate == "function";
    c || typeof o.UNSAFE_componentWillReceiveProps != "function" && typeof o.componentWillReceiveProps != "function" || (s !== r || u !== d) && Fs(t, o, r, d), bt = !1;
    var h = t.memoizedState;
    o.state = h, Ol(t, r, o, l), u = t.memoizedState, s !== r || h !== u || We.current || bt ? (typeof N == "function" && (gi(t, n, N, r), u = t.memoizedState), (s = bt || Ps(t, n, s, r, h, u, d)) ? (c || typeof o.UNSAFE_componentWillMount != "function" && typeof o.componentWillMount != "function" || (typeof o.componentWillMount == "function" && o.componentWillMount(), typeof o.UNSAFE_componentWillMount == "function" && o.UNSAFE_componentWillMount()), typeof o.componentDidMount == "function" && (t.flags |= 4194308)) : (typeof o.componentDidMount == "function" && (t.flags |= 4194308), t.memoizedProps = r, t.memoizedState = u), o.props = r, o.state = u, o.context = d, r = s) : (typeof o.componentDidMount == "function" && (t.flags |= 4194308), r = !1);
  } else {
    o = t.stateNode, mc(e, t), s = t.memoizedProps, d = t.type === t.elementType ? s : ft(t.type, s), o.props = d, c = t.pendingProps, h = o.context, u = n.contextType, typeof u == "object" && u !== null ? u = ot(u) : (u = Qe(n) ? vn : Le.current, u = Qn(t, u));
    var x = n.getDerivedStateFromProps;
    (N = typeof x == "function" || typeof o.getSnapshotBeforeUpdate == "function") || typeof o.UNSAFE_componentWillReceiveProps != "function" && typeof o.componentWillReceiveProps != "function" || (s !== c || h !== u) && Fs(t, o, r, u), bt = !1, h = t.memoizedState, o.state = h, Ol(t, r, o, l);
    var y = t.memoizedState;
    s !== c || h !== y || We.current || bt ? (typeof x == "function" && (gi(t, n, x, r), y = t.memoizedState), (d = bt || Ps(t, n, d, r, h, y, u) || !1) ? (N || typeof o.UNSAFE_componentWillUpdate != "function" && typeof o.componentWillUpdate != "function" || (typeof o.componentWillUpdate == "function" && o.componentWillUpdate(r, y, u), typeof o.UNSAFE_componentWillUpdate == "function" && o.UNSAFE_componentWillUpdate(r, y, u)), typeof o.componentDidUpdate == "function" && (t.flags |= 4), typeof o.getSnapshotBeforeUpdate == "function" && (t.flags |= 1024)) : (typeof o.componentDidUpdate != "function" || s === e.memoizedProps && h === e.memoizedState || (t.flags |= 4), typeof o.getSnapshotBeforeUpdate != "function" || s === e.memoizedProps && h === e.memoizedState || (t.flags |= 1024), t.memoizedProps = r, t.memoizedState = y), o.props = r, o.state = y, o.context = u, r = d) : (typeof o.componentDidUpdate != "function" || s === e.memoizedProps && h === e.memoizedState || (t.flags |= 4), typeof o.getSnapshotBeforeUpdate != "function" || s === e.memoizedProps && h === e.memoizedState || (t.flags |= 1024), r = !1);
  }
  return Si(e, t, n, r, i, l);
}
function Si(e, t, n, r, l, i) {
  Uc(e, t);
  var o = (t.flags & 128) !== 0;
  if (!r && !o) return l && js(t, n, !1), Mt(e, t, i);
  r = t.stateNode, Np.current = t;
  var s = o && typeof n.getDerivedStateFromError != "function" ? null : r.render();
  return t.flags |= 1, e !== null && o ? (t.child = Kn(t, e.child, null, i), t.child = Kn(t, null, s, i)) : Me(e, t, s, i), t.memoizedState = r.state, l && js(t, n, !0), t.child;
}
function bc(e) {
  var t = e.stateNode;
  t.pendingContext ? ys(e, t.pendingContext, t.pendingContext !== t.context) : t.context && ys(e, t.context, !1), co(e, t.containerInfo);
}
function Ms(e, t, n, r, l) {
  return Gn(), lo(l), t.flags |= 256, Me(e, t, n, r), t.child;
}
var wi = { dehydrated: null, treeContext: null, retryLane: 0 };
function ki(e) {
  return { baseLanes: e, cachePool: null, transitions: null };
}
function Vc(e, t, n) {
  var r = t.pendingProps, l = ue.current, i = !1, o = (t.flags & 128) !== 0, s;
  if ((s = o) || (s = e !== null && e.memoizedState === null ? !1 : (l & 2) !== 0), s ? (i = !0, t.flags &= -129) : (e === null || e.memoizedState !== null) && (l |= 1), ne(ue, l & 1), e === null)
    return vi(t), e = t.memoizedState, e !== null && (e = e.dehydrated, e !== null) ? (t.mode & 1 ? e.data === "$!" ? t.lanes = 8 : t.lanes = 1073741824 : t.lanes = 1, null) : (o = r.children, e = r.fallback, i ? (r = t.mode, i = t.child, o = { mode: "hidden", children: o }, !(r & 1) && i !== null ? (i.childLanes = 0, i.pendingProps = o) : i = la(o, r, 0, null), e = mn(e, r, n, null), i.return = t, e.return = t, i.sibling = e, t.child = i, t.child.memoizedState = ki(n), t.memoizedState = wi, e) : yo(t, o));
  if (l = e.memoizedState, l !== null && (s = l.dehydrated, s !== null)) return Sp(e, t, o, r, s, l, n);
  if (i) {
    i = r.fallback, o = t.mode, l = e.child, s = l.sibling;
    var u = { mode: "hidden", children: r.children };
    return !(o & 1) && t.child !== l ? (r = t.child, r.childLanes = 0, r.pendingProps = u, t.deletions = null) : (r = Jt(l, u), r.subtreeFlags = l.subtreeFlags & 14680064), s !== null ? i = Jt(s, i) : (i = mn(i, o, n, null), i.flags |= 2), i.return = t, r.return = t, r.sibling = i, t.child = r, r = i, i = t.child, o = e.child.memoizedState, o = o === null ? ki(n) : { baseLanes: o.baseLanes | n, cachePool: null, transitions: o.transitions }, i.memoizedState = o, i.childLanes = e.childLanes & ~n, t.memoizedState = wi, r;
  }
  return i = e.child, e = i.sibling, r = Jt(i, { mode: "visible", children: r.children }), !(t.mode & 1) && (r.lanes = n), r.return = t, r.sibling = null, e !== null && (n = t.deletions, n === null ? (t.deletions = [e], t.flags |= 16) : n.push(e)), t.child = r, t.memoizedState = null, r;
}
function yo(e, t) {
  return t = la({ mode: "visible", children: t }, e.mode, 0, null), t.return = e, e.child = t;
}
function ol(e, t, n, r) {
  return r !== null && lo(r), Kn(t, e.child, null, n), e = yo(t, t.pendingProps.children), e.flags |= 2, t.memoizedState = null, e;
}
function Sp(e, t, n, r, l, i, o) {
  if (n)
    return t.flags & 256 ? (t.flags &= -257, r = Da(Error(_(422))), ol(e, t, o, r)) : t.memoizedState !== null ? (t.child = e.child, t.flags |= 128, null) : (i = r.fallback, l = t.mode, r = la({ mode: "visible", children: r.children }, l, 0, null), i = mn(i, l, o, null), i.flags |= 2, r.return = t, i.return = t, r.sibling = i, t.child = r, t.mode & 1 && Kn(t, e.child, null, o), t.child.memoizedState = ki(o), t.memoizedState = wi, i);
  if (!(t.mode & 1)) return ol(e, t, o, null);
  if (l.data === "$!") {
    if (r = l.nextSibling && l.nextSibling.dataset, r) var s = r.dgst;
    return r = s, i = Error(_(419)), r = Da(i, r, void 0), ol(e, t, o, r);
  }
  if (s = (o & e.childLanes) !== 0, He || s) {
    if (r = ke, r !== null) {
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
      l = l & (r.suspendedLanes | o) ? 0 : l, l !== 0 && l !== i.retryLane && (i.retryLane = l, Dt(e, l), vt(r, e, l, -1));
    }
    return Co(), r = Da(Error(_(421))), ol(e, t, o, r);
  }
  return l.data === "$?" ? (t.flags |= 128, t.child = e.child, t = Dp.bind(null, e), l._reactRetry = t, null) : (e = i.treeContext, Xe = qt(l.nextSibling), Ze = t, oe = !0, mt = null, e !== null && (nt[rt++] = _t, nt[rt++] = Rt, nt[rt++] = xn, _t = e.id, Rt = e.overflow, xn = t), t = yo(t, r.children), t.flags |= 4096, t);
}
function $s(e, t, n) {
  e.lanes |= t;
  var r = e.alternate;
  r !== null && (r.lanes |= t), xi(e.return, t, n);
}
function Ma(e, t, n, r, l) {
  var i = e.memoizedState;
  i === null ? e.memoizedState = { isBackwards: t, rendering: null, renderingStartTime: 0, last: r, tail: n, tailMode: l } : (i.isBackwards = t, i.rendering = null, i.renderingStartTime = 0, i.last = r, i.tail = n, i.tailMode = l);
}
function Bc(e, t, n) {
  var r = t.pendingProps, l = r.revealOrder, i = r.tail;
  if (Me(e, t, r.children, n), r = ue.current, r & 2) r = r & 1 | 2, t.flags |= 128;
  else {
    if (e !== null && e.flags & 128) e: for (e = t.child; e !== null; ) {
      if (e.tag === 13) e.memoizedState !== null && $s(e, n, t);
      else if (e.tag === 19) $s(e, n, t);
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
  if (ne(ue, r), !(t.mode & 1)) t.memoizedState = null;
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
function Mt(e, t, n) {
  if (e !== null && (t.dependencies = e.dependencies), yn |= t.lanes, !(n & t.childLanes)) return null;
  if (e !== null && t.child !== e.child) throw Error(_(153));
  if (t.child !== null) {
    for (e = t.child, n = Jt(e, e.pendingProps), t.child = n, n.return = t; e.sibling !== null; ) e = e.sibling, n = n.sibling = Jt(e, e.pendingProps), n.return = t;
    n.sibling = null;
  }
  return t.child;
}
function wp(e, t, n) {
  switch (t.tag) {
    case 3:
      bc(t), Gn();
      break;
    case 5:
      hc(t);
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
        return r.dehydrated !== null ? (ne(ue, ue.current & 1), t.flags |= 128, null) : n & t.child.childLanes ? Vc(e, t, n) : (ne(ue, ue.current & 1), e = Mt(e, t, n), e !== null ? e.sibling : null);
      ne(ue, ue.current & 1);
      break;
    case 19:
      if (r = (n & t.childLanes) !== 0, e.flags & 128) {
        if (r) return Bc(e, t, n);
        t.flags |= 128;
      }
      if (l = t.memoizedState, l !== null && (l.rendering = null, l.tail = null, l.lastEffect = null), ne(ue, ue.current), r) break;
      return null;
    case 22:
    case 23:
      return t.lanes = 0, Ac(e, t, n);
  }
  return Mt(e, t, n);
}
var Hc, Ci, Wc, Qc;
Hc = function(e, t) {
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
Wc = function(e, t, n, r) {
  var l = e.memoizedProps;
  if (l !== r) {
    e = t.stateNode, fn(wt.current);
    var i = null;
    switch (n) {
      case "input":
        l = Ga(e, l), r = Ga(e, r), i = [];
        break;
      case "select":
        l = de({}, l, { value: void 0 }), r = de({}, r, { value: void 0 }), i = [];
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
    } else d !== "dangerouslySetInnerHTML" && d !== "children" && d !== "suppressContentEditableWarning" && d !== "suppressHydrationWarning" && d !== "autoFocus" && (kr.hasOwnProperty(d) ? i || (i = []) : (i = i || []).push(d, null));
    for (d in r) {
      var u = r[d];
      if (s = l != null ? l[d] : void 0, r.hasOwnProperty(d) && u !== s && (u != null || s != null)) if (d === "style") if (s) {
        for (o in s) !s.hasOwnProperty(o) || u && u.hasOwnProperty(o) || (n || (n = {}), n[o] = "");
        for (o in u) u.hasOwnProperty(o) && s[o] !== u[o] && (n || (n = {}), n[o] = u[o]);
      } else n || (i || (i = []), i.push(
        d,
        n
      )), n = u;
      else d === "dangerouslySetInnerHTML" ? (u = u ? u.__html : void 0, s = s ? s.__html : void 0, u != null && s !== u && (i = i || []).push(d, u)) : d === "children" ? typeof u != "string" && typeof u != "number" || (i = i || []).push(d, "" + u) : d !== "suppressContentEditableWarning" && d !== "suppressHydrationWarning" && (kr.hasOwnProperty(d) ? (u != null && d === "onScroll" && le("scroll", e), i || s === u || (i = [])) : (i = i || []).push(d, u));
    }
    n && (i = i || []).push("style", n);
    var d = i;
    (t.updateQueue = d) && (t.flags |= 4);
  }
};
Qc = function(e, t, n, r) {
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
  if (t) for (var l = e.child; l !== null; ) n |= l.lanes | l.childLanes, r |= l.subtreeFlags & 14680064, r |= l.flags & 14680064, l.return = e, l = l.sibling;
  else for (l = e.child; l !== null; ) n |= l.lanes | l.childLanes, r |= l.subtreeFlags, r |= l.flags, l.return = e, l = l.sibling;
  return e.subtreeFlags |= r, e.childLanes = n, t;
}
function kp(e, t, n) {
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
      return r = t.stateNode, qn(), ae(We), ae(Le), po(), r.pendingContext && (r.context = r.pendingContext, r.pendingContext = null), (e === null || e.child === null) && (al(t) ? t.flags |= 4 : e === null || e.memoizedState.isDehydrated && !(t.flags & 256) || (t.flags |= 1024, mt !== null && (zi(mt), mt = null))), Ci(e, t), Re(t), null;
    case 5:
      fo(t);
      var l = fn(Mr.current);
      if (n = t.type, e !== null && t.stateNode != null) Wc(e, t, n, r, l), e.ref !== t.ref && (t.flags |= 512, t.flags |= 2097152);
      else {
        if (!r) {
          if (t.stateNode === null) throw Error(_(166));
          return Re(t), null;
        }
        if (e = fn(wt.current), al(t)) {
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
              for (l = 0; l < pr.length; l++) le(pr[l], r);
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
              Qo(r, i), le("invalid", r);
              break;
            case "select":
              r._wrapperState = { wasMultiple: !!i.multiple }, le("invalid", r);
              break;
            case "textarea":
              Ko(r, i), le("invalid", r);
          }
          Za(n, i), l = null;
          for (var o in i) if (i.hasOwnProperty(o)) {
            var s = i[o];
            o === "children" ? typeof s == "string" ? r.textContent !== s && (i.suppressHydrationWarning !== !0 && ll(r.textContent, s, e), l = ["children", s]) : typeof s == "number" && r.textContent !== "" + s && (i.suppressHydrationWarning !== !0 && ll(
              r.textContent,
              s,
              e
            ), l = ["children", "" + s]) : kr.hasOwnProperty(o) && s != null && o === "onScroll" && le("scroll", r);
          }
          switch (n) {
            case "input":
              Yr(r), Go(r, i, !0);
              break;
            case "textarea":
              Yr(r), qo(r);
              break;
            case "select":
            case "option":
              break;
            default:
              typeof i.onClick == "function" && (r.onclick = Rl);
          }
          r = l, t.updateQueue = r, r !== null && (t.flags |= 4);
        } else {
          o = l.nodeType === 9 ? l : l.ownerDocument, e === "http://www.w3.org/1999/xhtml" && (e = ju(n)), e === "http://www.w3.org/1999/xhtml" ? n === "script" ? (e = o.createElement("div"), e.innerHTML = "<script><\/script>", e = e.removeChild(e.firstChild)) : typeof r.is == "string" ? e = o.createElement(n, { is: r.is }) : (e = o.createElement(n), n === "select" && (o = e, r.multiple ? o.multiple = !0 : r.size && (o.size = r.size))) : e = o.createElementNS(e, n), e[Nt] = t, e[Lr] = r, Hc(e, t, !1, !1), t.stateNode = e;
          e: {
            switch (o = Ja(n, r), n) {
              case "dialog":
                le("cancel", e), le("close", e), l = r;
                break;
              case "iframe":
              case "object":
              case "embed":
                le("load", e), l = r;
                break;
              case "video":
              case "audio":
                for (l = 0; l < pr.length; l++) le(pr[l], e);
                l = r;
                break;
              case "source":
                le("error", e), l = r;
                break;
              case "img":
              case "image":
              case "link":
                le(
                  "error",
                  e
                ), le("load", e), l = r;
                break;
              case "details":
                le("toggle", e), l = r;
                break;
              case "input":
                Qo(e, r), l = Ga(e, r), le("invalid", e);
                break;
              case "option":
                l = r;
                break;
              case "select":
                e._wrapperState = { wasMultiple: !!r.multiple }, l = de({}, r, { value: void 0 }), le("invalid", e);
                break;
              case "textarea":
                Ko(e, r), l = Ya(e, r), le("invalid", e);
                break;
              default:
                l = r;
            }
            Za(n, l), s = l;
            for (i in s) if (s.hasOwnProperty(i)) {
              var u = s[i];
              i === "style" ? wu(e, u) : i === "dangerouslySetInnerHTML" ? (u = u ? u.__html : void 0, u != null && Nu(e, u)) : i === "children" ? typeof u == "string" ? (n !== "textarea" || u !== "") && Cr(e, u) : typeof u == "number" && Cr(e, "" + u) : i !== "suppressContentEditableWarning" && i !== "suppressHydrationWarning" && i !== "autoFocus" && (kr.hasOwnProperty(i) ? u != null && i === "onScroll" && le("scroll", e) : u != null && Vi(e, i, u, o));
            }
            switch (n) {
              case "input":
                Yr(e), Go(e, r, !1);
                break;
              case "textarea":
                Yr(e), qo(e);
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
      if (e && t.stateNode != null) Qc(e, t, e.memoizedProps, r);
      else {
        if (typeof r != "string" && t.stateNode === null) throw Error(_(166));
        if (n = fn(Mr.current), fn(wt.current), al(t)) {
          if (r = t.stateNode, n = t.memoizedProps, r[Nt] = t, (i = r.nodeValue !== n) && (e = Ze, e !== null)) switch (e.tag) {
            case 3:
              ll(r.nodeValue, n, (e.mode & 1) !== 0);
              break;
            case 5:
              e.memoizedProps.suppressHydrationWarning !== !0 && ll(r.nodeValue, n, (e.mode & 1) !== 0);
          }
          i && (t.flags |= 4);
        } else r = (n.nodeType === 9 ? n : n.ownerDocument).createTextNode(r), r[Nt] = t, t.stateNode = r;
      }
      return Re(t), null;
    case 13:
      if (ae(ue), r = t.memoizedState, e === null || e.memoizedState !== null && e.memoizedState.dehydrated !== null) {
        if (oe && Xe !== null && t.mode & 1 && !(t.flags & 128)) cc(), Gn(), t.flags |= 98560, i = !1;
        else if (i = al(t), r !== null && r.dehydrated !== null) {
          if (e === null) {
            if (!i) throw Error(_(318));
            if (i = t.memoizedState, i = i !== null ? i.dehydrated : null, !i) throw Error(_(317));
            i[Nt] = t;
          } else Gn(), !(t.flags & 128) && (t.memoizedState = null), t.flags |= 4;
          Re(t), i = !1;
        } else mt !== null && (zi(mt), mt = null), i = !0;
        if (!i) return t.flags & 65536 ? t : null;
      }
      return t.flags & 128 ? (t.lanes = n, t) : (r = r !== null, r !== (e !== null && e.memoizedState !== null) && r && (t.child.flags |= 8192, t.mode & 1 && (e === null || ue.current & 1 ? Ne === 0 && (Ne = 3) : Co())), t.updateQueue !== null && (t.flags |= 4), Re(t), null);
    case 4:
      return qn(), Ci(e, t), e === null && Tr(t.stateNode.containerInfo), Re(t), null;
    case 10:
      return oo(t.type._context), Re(t), null;
    case 17:
      return Qe(t.type) && Tl(), Re(t), null;
    case 19:
      if (ae(ue), i = t.memoizedState, i === null) return Re(t), null;
      if (r = (t.flags & 128) !== 0, o = i.rendering, o === null) if (r) sr(i, !1);
      else {
        if (Ne !== 0 || e !== null && e.flags & 128) for (e = t.child; e !== null; ) {
          if (o = Al(e), o !== null) {
            for (t.flags |= 128, sr(i, !1), r = o.updateQueue, r !== null && (t.updateQueue = r, t.flags |= 4), t.subtreeFlags = 0, r = n, n = t.child; n !== null; ) i = n, e = r, i.flags &= 14680066, o = i.alternate, o === null ? (i.childLanes = 0, i.lanes = e, i.child = null, i.subtreeFlags = 0, i.memoizedProps = null, i.memoizedState = null, i.updateQueue = null, i.dependencies = null, i.stateNode = null) : (i.childLanes = o.childLanes, i.lanes = o.lanes, i.child = o.child, i.subtreeFlags = 0, i.deletions = null, i.memoizedProps = o.memoizedProps, i.memoizedState = o.memoizedState, i.updateQueue = o.updateQueue, i.type = o.type, e = o.dependencies, i.dependencies = e === null ? null : { lanes: e.lanes, firstContext: e.firstContext }), n = n.sibling;
            return ne(ue, ue.current & 1 | 2), t.child;
          }
          e = e.sibling;
        }
        i.tail !== null && ve() > Xn && (t.flags |= 128, r = !0, sr(i, !1), t.lanes = 4194304);
      }
      else {
        if (!r) if (e = Al(o), e !== null) {
          if (t.flags |= 128, r = !0, n = e.updateQueue, n !== null && (t.updateQueue = n, t.flags |= 4), sr(i, !0), i.tail === null && i.tailMode === "hidden" && !o.alternate && !oe) return Re(t), null;
        } else 2 * ve() - i.renderingStartTime > Xn && n !== 1073741824 && (t.flags |= 128, r = !0, sr(i, !1), t.lanes = 4194304);
        i.isBackwards ? (o.sibling = t.child, t.child = o) : (n = i.last, n !== null ? n.sibling = o : t.child = o, i.last = o);
      }
      return i.tail !== null ? (t = i.tail, i.rendering = t, i.tail = t.sibling, i.renderingStartTime = ve(), t.sibling = null, n = ue.current, ne(ue, r ? n & 1 | 2 : n & 1), t) : (Re(t), null);
    case 22:
    case 23:
      return ko(), r = t.memoizedState !== null, e !== null && e.memoizedState !== null !== r && (t.flags |= 8192), r && t.mode & 1 ? Ye & 1073741824 && (Re(t), t.subtreeFlags & 6 && (t.flags |= 8192)) : Re(t), null;
    case 24:
      return null;
    case 25:
      return null;
  }
  throw Error(_(156, t.tag));
}
function Cp(e, t) {
  switch (ro(t), t.tag) {
    case 1:
      return Qe(t.type) && Tl(), e = t.flags, e & 65536 ? (t.flags = e & -65537 | 128, t) : null;
    case 3:
      return qn(), ae(We), ae(Le), po(), e = t.flags, e & 65536 && !(e & 128) ? (t.flags = e & -65537 | 128, t) : null;
    case 5:
      return fo(t), null;
    case 13:
      if (ae(ue), e = t.memoizedState, e !== null && e.dehydrated !== null) {
        if (t.alternate === null) throw Error(_(340));
        Gn();
      }
      return e = t.flags, e & 65536 ? (t.flags = e & -65537 | 128, t) : null;
    case 19:
      return ae(ue), null;
    case 4:
      return qn(), null;
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
var sl = !1, Te = !1, Ep = typeof WeakSet == "function" ? WeakSet : Set, $ = null;
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
var Os = !1;
function Ip(e, t) {
  if (ui = Pl, e = Xu(), to(e)) {
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
          for (var x; c !== n || l !== 0 && c.nodeType !== 3 || (s = o + l), c !== i || r !== 0 && c.nodeType !== 3 || (u = o + r), c.nodeType === 3 && (o += c.nodeValue.length), (x = c.firstChild) !== null; )
            h = c, c = x;
          for (; ; ) {
            if (c === e) break t;
            if (h === n && ++d === l && (s = o), h === i && ++N === r && (u = o), (x = c.nextSibling) !== null) break;
            c = h, h = c.parentNode;
          }
          c = x;
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
            var S = y.memoizedProps, M = y.memoizedState, p = t.stateNode, f = p.getSnapshotBeforeUpdate(t.elementType === t.type ? S : ft(t.type, S), M);
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
          throw Error(_(163));
      }
    } catch (C) {
      he(t, t.return, C);
    }
    if (e = t.sibling, e !== null) {
      e.return = t.return, $ = e;
      break;
    }
    $ = t.return;
  }
  return y = Os, Os = !1, y;
}
function jr(e, t, n) {
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
function Gc(e) {
  var t = e.alternate;
  t !== null && (e.alternate = null, Gc(t)), e.child = null, e.deletions = null, e.sibling = null, e.tag === 5 && (t = e.stateNode, t !== null && (delete t[Nt], delete t[Lr], delete t[pi], delete t[up], delete t[cp])), e.stateNode = null, e.return = null, e.dependencies = null, e.memoizedProps = null, e.memoizedState = null, e.pendingProps = null, e.stateNode = null, e.updateQueue = null;
}
function Kc(e) {
  return e.tag === 5 || e.tag === 3 || e.tag === 4;
}
function As(e) {
  e: for (; ; ) {
    for (; e.sibling === null; ) {
      if (e.return === null || Kc(e.return)) return null;
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
var Ce = null, pt = !1;
function At(e, t, n) {
  for (n = n.child; n !== null; ) qc(e, t, n), n = n.sibling;
}
function qc(e, t, n) {
  if (St && typeof St.onCommitFiberUnmount == "function") try {
    St.onCommitFiberUnmount(Kl, n);
  } catch {
  }
  switch (n.tag) {
    case 5:
      Te || Mn(n, t);
    case 6:
      var r = Ce, l = pt;
      Ce = null, At(e, t, n), Ce = r, pt = l, Ce !== null && (pt ? (e = Ce, n = n.stateNode, e.nodeType === 8 ? e.parentNode.removeChild(n) : e.removeChild(n)) : Ce.removeChild(n.stateNode));
      break;
    case 18:
      Ce !== null && (pt ? (e = Ce, n = n.stateNode, e.nodeType === 8 ? Fa(e.parentNode, n) : e.nodeType === 1 && Fa(e, n), Fr(e)) : Fa(Ce, n.stateNode));
      break;
    case 4:
      r = Ce, l = pt, Ce = n.stateNode.containerInfo, pt = !0, At(e, t, n), Ce = r, pt = l;
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
      At(e, t, n);
      break;
    case 1:
      if (!Te && (Mn(n, t), r = n.stateNode, typeof r.componentWillUnmount == "function")) try {
        r.props = n.memoizedProps, r.state = n.memoizedState, r.componentWillUnmount();
      } catch (s) {
        he(n, t, s);
      }
      At(e, t, n);
      break;
    case 21:
      At(e, t, n);
      break;
    case 22:
      n.mode & 1 ? (Te = (r = Te) || n.memoizedState !== null, At(e, t, n), Te = r) : At(e, t, n);
      break;
    default:
      At(e, t, n);
  }
}
function Us(e) {
  var t = e.updateQueue;
  if (t !== null) {
    e.updateQueue = null;
    var n = e.stateNode;
    n === null && (n = e.stateNode = new Ep()), t.forEach(function(r) {
      var l = Mp.bind(null, e, r);
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
            Ce = s.stateNode, pt = !1;
            break e;
          case 3:
            Ce = s.stateNode.containerInfo, pt = !0;
            break e;
          case 4:
            Ce = s.stateNode.containerInfo, pt = !0;
            break e;
        }
        s = s.return;
      }
      if (Ce === null) throw Error(_(160));
      qc(i, o, l), Ce = null, pt = !1;
      var u = l.alternate;
      u !== null && (u.return = null), l.return = null;
    } catch (d) {
      he(l, t, d);
    }
  }
  if (t.subtreeFlags & 12854) for (t = t.child; t !== null; ) Yc(t, e), t = t.sibling;
}
function Yc(e, t) {
  var n = e.alternate, r = e.flags;
  switch (e.tag) {
    case 0:
    case 11:
    case 14:
    case 15:
      if (dt(t, e), yt(e), r & 4) {
        try {
          jr(3, e, e.return), na(3, e);
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
      dt(t, e), yt(e), r & 512 && n !== null && Mn(n, n.return);
      break;
    case 5:
      if (dt(t, e), yt(e), r & 512 && n !== null && Mn(n, n.return), e.flags & 32) {
        var l = e.stateNode;
        try {
          Cr(l, "");
        } catch (S) {
          he(e, e.return, S);
        }
      }
      if (r & 4 && (l = e.stateNode, l != null)) {
        var i = e.memoizedProps, o = n !== null ? n.memoizedProps : i, s = e.type, u = e.updateQueue;
        if (e.updateQueue = null, u !== null) try {
          s === "input" && i.type === "radio" && i.name != null && gu(l, i), Ja(s, o);
          var d = Ja(s, i);
          for (o = 0; o < u.length; o += 2) {
            var N = u[o], c = u[o + 1];
            N === "style" ? wu(l, c) : N === "dangerouslySetInnerHTML" ? Nu(l, c) : N === "children" ? Cr(l, c) : Vi(l, N, c, d);
          }
          switch (s) {
            case "input":
              Ka(l, i);
              break;
            case "textarea":
              yu(l, i);
              break;
            case "select":
              var h = l._wrapperState.wasMultiple;
              l._wrapperState.wasMultiple = !!i.multiple;
              var x = i.value;
              x != null ? On(l, !!i.multiple, x, !1) : h !== !!i.multiple && (i.defaultValue != null ? On(
                l,
                !!i.multiple,
                i.defaultValue,
                !0
              ) : On(l, !!i.multiple, i.multiple ? [] : "", !1));
          }
          l[Lr] = i;
        } catch (S) {
          he(e, e.return, S);
        }
      }
      break;
    case 6:
      if (dt(t, e), yt(e), r & 4) {
        if (e.stateNode === null) throw Error(_(162));
        l = e.stateNode, i = e.memoizedProps;
        try {
          l.nodeValue = i;
        } catch (S) {
          he(e, e.return, S);
        }
      }
      break;
    case 3:
      if (dt(t, e), yt(e), r & 4 && n !== null && n.memoizedState.isDehydrated) try {
        Fr(t.containerInfo);
      } catch (S) {
        he(e, e.return, S);
      }
      break;
    case 4:
      dt(t, e), yt(e);
      break;
    case 13:
      dt(t, e), yt(e), l = e.child, l.flags & 8192 && (i = l.memoizedState !== null, l.stateNode.isHidden = i, !i || l.alternate !== null && l.alternate.memoizedState !== null || (So = ve())), r & 4 && Us(e);
      break;
    case 22:
      if (N = n !== null && n.memoizedState !== null, e.mode & 1 ? (Te = (d = Te) || N, dt(t, e), Te = d) : dt(t, e), yt(e), r & 8192) {
        if (d = e.memoizedState !== null, (e.stateNode.isHidden = d) && !N && e.mode & 1) for ($ = e, N = e.child; N !== null; ) {
          for (c = $ = N; $ !== null; ) {
            switch (h = $, x = h.child, h.tag) {
              case 0:
              case 11:
              case 14:
              case 15:
                jr(4, h, h.return);
                break;
              case 1:
                Mn(h, h.return);
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
                Mn(h, h.return);
                break;
              case 22:
                if (h.memoizedState !== null) {
                  Vs(c);
                  continue;
                }
            }
            x !== null ? (x.return = h, $ = x) : Vs(c);
          }
          N = N.sibling;
        }
        e: for (N = null, c = e; ; ) {
          if (c.tag === 5) {
            if (N === null) {
              N = c;
              try {
                l = c.stateNode, d ? (i = l.style, typeof i.setProperty == "function" ? i.setProperty("display", "none", "important") : i.display = "none") : (s = c.stateNode, u = c.memoizedProps.style, o = u != null && u.hasOwnProperty("display") ? u.display : null, s.style.display = Su("display", o));
              } catch (S) {
                he(e, e.return, S);
              }
            }
          } else if (c.tag === 6) {
            if (N === null) try {
              c.stateNode.nodeValue = d ? "" : c.memoizedProps;
            } catch (S) {
              he(e, e.return, S);
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
      dt(t, e), yt(e), r & 4 && Us(e);
      break;
    case 21:
      break;
    default:
      dt(
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
          if (Kc(n)) {
            var r = n;
            break e;
          }
          n = n.return;
        }
        throw Error(_(160));
      }
      switch (r.tag) {
        case 5:
          var l = r.stateNode;
          r.flags & 32 && (Cr(l, ""), r.flags &= -33);
          var i = As(e);
          Fi(e, i, l);
          break;
        case 3:
        case 4:
          var o = r.stateNode.containerInfo, s = As(e);
          Pi(e, s, o);
          break;
        default:
          throw Error(_(161));
      }
    } catch (u) {
      he(e, e.return, u);
    }
    e.flags &= -3;
  }
  t & 4096 && (e.flags &= -4097);
}
function Pp(e, t, n) {
  $ = e, Xc(e);
}
function Xc(e, t, n) {
  for (var r = (e.mode & 1) !== 0; $ !== null; ) {
    var l = $, i = l.child;
    if (l.tag === 22 && r) {
      var o = l.memoizedState !== null || sl;
      if (!o) {
        var s = l.alternate, u = s !== null && s.memoizedState !== null || Te;
        s = sl;
        var d = Te;
        if (sl = o, (Te = u) && !d) for ($ = l; $ !== null; ) o = $, u = o.child, o.tag === 22 && o.memoizedState !== null ? Bs(l) : u !== null ? (u.return = o, $ = u) : Bs(l);
        for (; i !== null; ) $ = i, Xc(i), i = i.sibling;
        $ = l, sl = s, Te = d;
      }
      bs(e);
    } else l.subtreeFlags & 8772 && i !== null ? (i.return = l, $ = i) : bs(e);
  }
}
function bs(e) {
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
            i !== null && Cs(t, i, r);
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
              Cs(t, o, n);
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
                  c !== null && Fr(c);
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
            throw Error(_(163));
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
function Vs(e) {
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
function Bs(e) {
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
var Fp = Math.ceil, Vl = $t.ReactCurrentDispatcher, jo = $t.ReactCurrentOwner, it = $t.ReactCurrentBatchConfig, Y = 0, ke = null, ge = null, Ee = 0, Ye = 0, $n = ln(0), Ne = 0, Ur = null, yn = 0, ra = 0, No = 0, Nr = null, Be = null, So = 0, Xn = 1 / 0, Pt = null, Bl = !1, _i = null, Xt = null, ul = !1, Wt = null, Hl = 0, Sr = 0, Ri = null, jl = -1, Nl = 0;
function Ae() {
  return Y & 6 ? ve() : jl !== -1 ? jl : jl = ve();
}
function Zt(e) {
  return e.mode & 1 ? Y & 2 && Ee !== 0 ? Ee & -Ee : fp.transition !== null ? (Nl === 0 && (Nl = Du()), Nl) : (e = Z, e !== 0 || (e = window.event, e = e === void 0 ? 16 : Vu(e.type)), e) : 1;
}
function vt(e, t, n, r) {
  if (50 < Sr) throw Sr = 0, Ri = null, Error(_(185));
  Vr(e, n, r), (!(Y & 2) || e !== ke) && (e === ke && (!(Y & 2) && (ra |= n), Ne === 4 && Bt(e, Ee)), Ge(e, r), n === 1 && Y === 0 && !(t.mode & 1) && (Xn = ve() + 500, Jl && an()));
}
function Ge(e, t) {
  var n = e.callbackNode;
  ff(e, t);
  var r = Il(e, e === ke ? Ee : 0);
  if (r === 0) n !== null && Zo(n), e.callbackNode = null, e.callbackPriority = 0;
  else if (t = r & -r, e.callbackPriority !== t) {
    if (n != null && Zo(n), t === 1) e.tag === 0 ? dp(Hs.bind(null, e)) : oc(Hs.bind(null, e)), op(function() {
      !(Y & 6) && an();
    }), n = null;
    else {
      switch (Mu(r)) {
        case 1:
          n = Gi;
          break;
        case 4:
          n = zu;
          break;
        case 16:
          n = El;
          break;
        case 536870912:
          n = Lu;
          break;
        default:
          n = El;
      }
      n = ad(n, Zc.bind(null, e));
    }
    e.callbackPriority = t, e.callbackNode = n;
  }
}
function Zc(e, t) {
  if (jl = -1, Nl = 0, Y & 6) throw Error(_(327));
  var n = e.callbackNode;
  if (Bn() && e.callbackNode !== n) return null;
  var r = Il(e, e === ke ? Ee : 0);
  if (r === 0) return null;
  if (r & 30 || r & e.expiredLanes || t) t = Wl(e, r);
  else {
    t = r;
    var l = Y;
    Y |= 2;
    var i = ed();
    (ke !== e || Ee !== t) && (Pt = null, Xn = ve() + 500, pn(e, t));
    do
      try {
        Tp();
        break;
      } catch (s) {
        Jc(e, s);
      }
    while (!0);
    io(), Vl.current = i, Y = l, ge !== null ? t = 0 : (ke = null, Ee = 0, t = Ne);
  }
  if (t !== 0) {
    if (t === 2 && (l = li(e), l !== 0 && (r = l, t = Ti(e, l))), t === 1) throw n = Ur, pn(e, 0), Bt(e, r), Ge(e, ve()), n;
    if (t === 6) Bt(e, r);
    else {
      if (l = e.current.alternate, !(r & 30) && !_p(l) && (t = Wl(e, r), t === 2 && (i = li(e), i !== 0 && (r = i, t = Ti(e, i))), t === 1)) throw n = Ur, pn(e, 0), Bt(e, r), Ge(e, ve()), n;
      switch (e.finishedWork = l, e.finishedLanes = r, t) {
        case 0:
        case 1:
          throw Error(_(345));
        case 2:
          un(e, Be, Pt);
          break;
        case 3:
          if (Bt(e, r), (r & 130023424) === r && (t = So + 500 - ve(), 10 < t)) {
            if (Il(e, 0) !== 0) break;
            if (l = e.suspendedLanes, (l & r) !== r) {
              Ae(), e.pingedLanes |= e.suspendedLanes & l;
              break;
            }
            e.timeoutHandle = fi(un.bind(null, e, Be, Pt), t);
            break;
          }
          un(e, Be, Pt);
          break;
        case 4:
          if (Bt(e, r), (r & 4194240) === r) break;
          for (t = e.eventTimes, l = -1; 0 < r; ) {
            var o = 31 - ht(r);
            i = 1 << o, o = t[o], o > l && (l = o), r &= ~i;
          }
          if (r = l, r = ve() - r, r = (120 > r ? 120 : 480 > r ? 480 : 1080 > r ? 1080 : 1920 > r ? 1920 : 3e3 > r ? 3e3 : 4320 > r ? 4320 : 1960 * Fp(r / 1960)) - r, 10 < r) {
            e.timeoutHandle = fi(un.bind(null, e, Be, Pt), r);
            break;
          }
          un(e, Be, Pt);
          break;
        case 5:
          un(e, Be, Pt);
          break;
        default:
          throw Error(_(329));
      }
    }
  }
  return Ge(e, ve()), e.callbackNode === n ? Zc.bind(null, e) : null;
}
function Ti(e, t) {
  var n = Nr;
  return e.current.memoizedState.isDehydrated && (pn(e, t).flags |= 256), e = Wl(e, t), e !== 2 && (t = Be, Be = n, t !== null && zi(t)), e;
}
function zi(e) {
  Be === null ? Be = e : Be.push.apply(Be, e);
}
function _p(e) {
  for (var t = e; ; ) {
    if (t.flags & 16384) {
      var n = t.updateQueue;
      if (n !== null && (n = n.stores, n !== null)) for (var r = 0; r < n.length; r++) {
        var l = n[r], i = l.getSnapshot;
        l = l.value;
        try {
          if (!gt(i(), l)) return !1;
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
    var n = 31 - ht(t), r = 1 << n;
    e[n] = -1, t &= ~r;
  }
}
function Hs(e) {
  if (Y & 6) throw Error(_(327));
  Bn();
  var t = Il(e, 0);
  if (!(t & 1)) return Ge(e, ve()), null;
  var n = Wl(e, t);
  if (e.tag !== 0 && n === 2) {
    var r = li(e);
    r !== 0 && (t = r, n = Ti(e, r));
  }
  if (n === 1) throw n = Ur, pn(e, 0), Bt(e, t), Ge(e, ve()), n;
  if (n === 6) throw Error(_(345));
  return e.finishedWork = e.current.alternate, e.finishedLanes = t, un(e, Be, Pt), Ge(e, ve()), null;
}
function wo(e, t) {
  var n = Y;
  Y |= 1;
  try {
    return e(t);
  } finally {
    Y = n, Y === 0 && (Xn = ve() + 500, Jl && an());
  }
}
function jn(e) {
  Wt !== null && Wt.tag === 0 && !(Y & 6) && Bn();
  var t = Y;
  Y |= 1;
  var n = it.transition, r = Z;
  try {
    if (it.transition = null, Z = 1, e) return e();
  } finally {
    Z = r, it.transition = n, Y = t, !(Y & 6) && an();
  }
}
function ko() {
  Ye = $n.current, ae($n);
}
function pn(e, t) {
  e.finishedWork = null, e.finishedLanes = 0;
  var n = e.timeoutHandle;
  if (n !== -1 && (e.timeoutHandle = -1, ip(n)), ge !== null) for (n = ge.return; n !== null; ) {
    var r = n;
    switch (ro(r), r.tag) {
      case 1:
        r = r.type.childContextTypes, r != null && Tl();
        break;
      case 3:
        qn(), ae(We), ae(Le), po();
        break;
      case 5:
        fo(r);
        break;
      case 4:
        qn();
        break;
      case 13:
        ae(ue);
        break;
      case 19:
        ae(ue);
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
  if (ke = e, ge = e = Jt(e.current, null), Ee = Ye = t, Ne = 0, Ur = null, No = ra = yn = 0, Be = Nr = null, dn !== null) {
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
function Jc(e, t) {
  do {
    var n = ge;
    try {
      if (io(), xl.current = bl, Ul) {
        for (var r = ce.memoizedState; r !== null; ) {
          var l = r.queue;
          l !== null && (l.pending = null), r = r.next;
        }
        Ul = !1;
      }
      if (gn = 0, Se = ye = ce = null, yr = !1, $r = 0, jo.current = null, n === null || n.return === null) {
        Ne = 1, Ur = t, ge = null;
        break;
      }
      e: {
        var i = e, o = n.return, s = n, u = t;
        if (t = Ee, s.flags |= 32768, u !== null && typeof u == "object" && typeof u.then == "function") {
          var d = u, N = s, c = N.tag;
          if (!(N.mode & 1) && (c === 0 || c === 11 || c === 15)) {
            var h = N.alternate;
            h ? (N.updateQueue = h.updateQueue, N.memoizedState = h.memoizedState, N.lanes = h.lanes) : (N.updateQueue = null, N.memoizedState = null);
          }
          var x = Rs(o);
          if (x !== null) {
            x.flags &= -257, Ts(x, o, s, i, t), x.mode & 1 && _s(i, d, t), t = x, u = d;
            var y = t.updateQueue;
            if (y === null) {
              var S = /* @__PURE__ */ new Set();
              S.add(u), t.updateQueue = S;
            } else y.add(u);
            break e;
          } else {
            if (!(t & 1)) {
              _s(i, d, t), Co();
              break e;
            }
            u = Error(_(426));
          }
        } else if (oe && s.mode & 1) {
          var M = Rs(o);
          if (M !== null) {
            !(M.flags & 65536) && (M.flags |= 256), Ts(M, o, s, i, t), lo(Yn(u, s));
            break e;
          }
        }
        i = u = Yn(u, s), Ne !== 4 && (Ne = 2), Nr === null ? Nr = [i] : Nr.push(i), i = o;
        do {
          switch (i.tag) {
            case 3:
              i.flags |= 65536, t &= -t, i.lanes |= t;
              var p = Mc(i, u, t);
              ks(i, p);
              break e;
            case 1:
              s = u;
              var f = i.type, v = i.stateNode;
              if (!(i.flags & 128) && (typeof f.getDerivedStateFromError == "function" || v !== null && typeof v.componentDidCatch == "function" && (Xt === null || !Xt.has(v)))) {
                i.flags |= 65536, t &= -t, i.lanes |= t;
                var C = $c(i, s, t);
                ks(i, C);
                break e;
              }
          }
          i = i.return;
        } while (i !== null);
      }
      nd(n);
    } catch (k) {
      t = k, ge === n && n !== null && (ge = n = n.return);
      continue;
    }
    break;
  } while (!0);
}
function ed() {
  var e = Vl.current;
  return Vl.current = bl, e === null ? bl : e;
}
function Co() {
  (Ne === 0 || Ne === 3 || Ne === 2) && (Ne = 4), ke === null || !(yn & 268435455) && !(ra & 268435455) || Bt(ke, Ee);
}
function Wl(e, t) {
  var n = Y;
  Y |= 2;
  var r = ed();
  (ke !== e || Ee !== t) && (Pt = null, pn(e, t));
  do
    try {
      Rp();
      break;
    } catch (l) {
      Jc(e, l);
    }
  while (!0);
  if (io(), Y = n, Vl.current = r, ge !== null) throw Error(_(261));
  return ke = null, Ee = 0, Ne;
}
function Rp() {
  for (; ge !== null; ) td(ge);
}
function Tp() {
  for (; ge !== null && !nf(); ) td(ge);
}
function td(e) {
  var t = ld(e.alternate, e, Ye);
  e.memoizedProps = e.pendingProps, t === null ? nd(e) : ge = t, jo.current = null;
}
function nd(e) {
  var t = e;
  do {
    var n = t.alternate;
    if (e = t.return, t.flags & 32768) {
      if (n = Cp(n, t), n !== null) {
        n.flags &= 32767, ge = n;
        return;
      }
      if (e !== null) e.flags |= 32768, e.subtreeFlags = 0, e.deletions = null;
      else {
        Ne = 6, ge = null;
        return;
      }
    } else if (n = kp(n, t, Ye), n !== null) {
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
  var r = Z, l = it.transition;
  try {
    it.transition = null, Z = 1, zp(e, t, n, r);
  } finally {
    it.transition = l, Z = r;
  }
  return null;
}
function zp(e, t, n, r) {
  do
    Bn();
  while (Wt !== null);
  if (Y & 6) throw Error(_(327));
  n = e.finishedWork;
  var l = e.finishedLanes;
  if (n === null) return null;
  if (e.finishedWork = null, e.finishedLanes = 0, n === e.current) throw Error(_(177));
  e.callbackNode = null, e.callbackPriority = 0;
  var i = n.lanes | n.childLanes;
  if (pf(e, i), e === ke && (ge = ke = null, Ee = 0), !(n.subtreeFlags & 2064) && !(n.flags & 2064) || ul || (ul = !0, ad(El, function() {
    return Bn(), null;
  })), i = (n.flags & 15990) !== 0, n.subtreeFlags & 15990 || i) {
    i = it.transition, it.transition = null;
    var o = Z;
    Z = 1;
    var s = Y;
    Y |= 4, jo.current = null, Ip(e, n), Yc(n, e), Jf(ci), Pl = !!ui, ci = ui = null, e.current = n, Pp(n), rf(), Y = s, Z = o, it.transition = i;
  } else e.current = n;
  if (ul && (ul = !1, Wt = e, Hl = l), i = e.pendingLanes, i === 0 && (Xt = null), of(n.stateNode), Ge(e, ve()), t !== null) for (r = e.onRecoverableError, n = 0; n < t.length; n++) l = t[n], r(l.value, { componentStack: l.stack, digest: l.digest });
  if (Bl) throw Bl = !1, e = _i, _i = null, e;
  return Hl & 1 && e.tag !== 0 && Bn(), i = e.pendingLanes, i & 1 ? e === Ri ? Sr++ : (Sr = 0, Ri = e) : Sr = 0, an(), null;
}
function Bn() {
  if (Wt !== null) {
    var e = Mu(Hl), t = it.transition, n = Z;
    try {
      if (it.transition = null, Z = 16 > e ? 16 : e, Wt === null) var r = !1;
      else {
        if (e = Wt, Wt = null, Hl = 0, Y & 6) throw Error(_(331));
        var l = Y;
        for (Y |= 4, $ = e.current; $ !== null; ) {
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
                      jr(8, N, i);
                  }
                  var c = N.child;
                  if (c !== null) c.return = N, $ = c;
                  else for (; $ !== null; ) {
                    N = $;
                    var h = N.sibling, x = N.return;
                    if (Gc(N), N === d) {
                      $ = null;
                      break;
                    }
                    if (h !== null) {
                      h.return = x, $ = h;
                      break;
                    }
                    $ = x;
                  }
                }
              }
              var y = i.alternate;
              if (y !== null) {
                var S = y.child;
                if (S !== null) {
                  y.child = null;
                  do {
                    var M = S.sibling;
                    S.sibling = null, S = M;
                  } while (S !== null);
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
                jr(9, i, i.return);
            }
            var p = i.sibling;
            if (p !== null) {
              p.return = i.return, $ = p;
              break e;
            }
            $ = i.return;
          }
        }
        var f = e.current;
        for ($ = f; $ !== null; ) {
          o = $;
          var v = o.child;
          if (o.subtreeFlags & 2064 && v !== null) v.return = o, $ = v;
          else e: for (o = f; $ !== null; ) {
            if (s = $, s.flags & 2048) try {
              switch (s.tag) {
                case 0:
                case 11:
                case 15:
                  na(9, s);
              }
            } catch (k) {
              he(s, s.return, k);
            }
            if (s === o) {
              $ = null;
              break e;
            }
            var C = s.sibling;
            if (C !== null) {
              C.return = s.return, $ = C;
              break e;
            }
            $ = s.return;
          }
        }
        if (Y = l, an(), St && typeof St.onPostCommitFiberRoot == "function") try {
          St.onPostCommitFiberRoot(Kl, e);
        } catch {
        }
        r = !0;
      }
      return r;
    } finally {
      Z = n, it.transition = t;
    }
  }
  return !1;
}
function Ws(e, t, n) {
  t = Yn(n, t), t = Mc(e, t, 1), e = Yt(e, t, 1), t = Ae(), e !== null && (Vr(e, 1, t), Ge(e, t));
}
function he(e, t, n) {
  if (e.tag === 3) Ws(e, e, n);
  else for (; t !== null; ) {
    if (t.tag === 3) {
      Ws(t, e, n);
      break;
    } else if (t.tag === 1) {
      var r = t.stateNode;
      if (typeof t.type.getDerivedStateFromError == "function" || typeof r.componentDidCatch == "function" && (Xt === null || !Xt.has(r))) {
        e = Yn(n, e), e = $c(t, e, 1), t = Yt(t, e, 1), e = Ae(), t !== null && (Vr(t, 1, e), Ge(t, e));
        break;
      }
    }
    t = t.return;
  }
}
function Lp(e, t, n) {
  var r = e.pingCache;
  r !== null && r.delete(t), t = Ae(), e.pingedLanes |= e.suspendedLanes & n, ke === e && (Ee & n) === n && (Ne === 4 || Ne === 3 && (Ee & 130023424) === Ee && 500 > ve() - So ? pn(e, 0) : No |= n), Ge(e, t);
}
function rd(e, t) {
  t === 0 && (e.mode & 1 ? (t = Jr, Jr <<= 1, !(Jr & 130023424) && (Jr = 4194304)) : t = 1);
  var n = Ae();
  e = Dt(e, t), e !== null && (Vr(e, t, n), Ge(e, n));
}
function Dp(e) {
  var t = e.memoizedState, n = 0;
  t !== null && (n = t.retryLane), rd(e, n);
}
function Mp(e, t) {
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
      throw Error(_(314));
  }
  r !== null && r.delete(t), rd(e, n);
}
var ld;
ld = function(e, t, n) {
  if (e !== null) if (e.memoizedProps !== t.pendingProps || We.current) He = !0;
  else {
    if (!(e.lanes & n) && !(t.flags & 128)) return He = !1, wp(e, t, n);
    He = !!(e.flags & 131072);
  }
  else He = !1, oe && t.flags & 1048576 && sc(t, Dl, t.index);
  switch (t.lanes = 0, t.tag) {
    case 2:
      var r = t.type;
      yl(e, t), e = t.pendingProps;
      var l = Qn(t, Le.current);
      Vn(t, n), l = ho(null, t, r, e, l, n);
      var i = vo();
      return t.flags |= 1, typeof l == "object" && l !== null && typeof l.render == "function" && l.$$typeof === void 0 ? (t.tag = 1, t.memoizedState = null, t.updateQueue = null, Qe(r) ? (i = !0, zl(t)) : i = !1, t.memoizedState = l.state !== null && l.state !== void 0 ? l.state : null, uo(t), l.updater = ta, t.stateNode = l, l._reactInternals = t, yi(t, r, e, n), t = Si(null, t, r, !0, i, n)) : (t.tag = 0, oe && i && no(t), Me(null, t, l, n), t = t.child), t;
    case 16:
      r = t.elementType;
      e: {
        switch (yl(e, t), e = t.pendingProps, l = r._init, r = l(r._payload), t.type = r, l = t.tag = Op(r), e = ft(r, e), l) {
          case 0:
            t = Ni(null, t, r, e, n);
            break e;
          case 1:
            t = Ds(null, t, r, e, n);
            break e;
          case 11:
            t = zs(null, t, r, e, n);
            break e;
          case 14:
            t = Ls(null, t, r, ft(r.type, e), n);
            break e;
        }
        throw Error(_(
          306,
          r,
          ""
        ));
      }
      return t;
    case 0:
      return r = t.type, l = t.pendingProps, l = t.elementType === r ? l : ft(r, l), Ni(e, t, r, l, n);
    case 1:
      return r = t.type, l = t.pendingProps, l = t.elementType === r ? l : ft(r, l), Ds(e, t, r, l, n);
    case 3:
      e: {
        if (bc(t), e === null) throw Error(_(387));
        r = t.pendingProps, i = t.memoizedState, l = i.element, mc(e, t), Ol(t, r, null, n);
        var o = t.memoizedState;
        if (r = o.element, i.isDehydrated) if (i = { element: r, isDehydrated: !1, cache: o.cache, pendingSuspenseBoundaries: o.pendingSuspenseBoundaries, transitions: o.transitions }, t.updateQueue.baseState = i, t.memoizedState = i, t.flags & 256) {
          l = Yn(Error(_(423)), t), t = Ms(e, t, r, n, l);
          break e;
        } else if (r !== l) {
          l = Yn(Error(_(424)), t), t = Ms(e, t, r, n, l);
          break e;
        } else for (Xe = qt(t.stateNode.containerInfo.firstChild), Ze = t, oe = !0, mt = null, n = fc(t, null, r, n), t.child = n; n; ) n.flags = n.flags & -3 | 4096, n = n.sibling;
        else {
          if (Gn(), r === l) {
            t = Mt(e, t, n);
            break e;
          }
          Me(e, t, r, n);
        }
        t = t.child;
      }
      return t;
    case 5:
      return hc(t), e === null && vi(t), r = t.type, l = t.pendingProps, i = e !== null ? e.memoizedProps : null, o = l.children, di(r, l) ? o = null : i !== null && di(r, i) && (t.flags |= 32), Uc(e, t), Me(e, t, o, n), t.child;
    case 6:
      return e === null && vi(t), null;
    case 13:
      return Vc(e, t, n);
    case 4:
      return co(t, t.stateNode.containerInfo), r = t.pendingProps, e === null ? t.child = Kn(t, null, r, n) : Me(e, t, r, n), t.child;
    case 11:
      return r = t.type, l = t.pendingProps, l = t.elementType === r ? l : ft(r, l), zs(e, t, r, l, n);
    case 7:
      return Me(e, t, t.pendingProps, n), t.child;
    case 8:
      return Me(e, t, t.pendingProps.children, n), t.child;
    case 12:
      return Me(e, t, t.pendingProps.children, n), t.child;
    case 10:
      e: {
        if (r = t.type._context, l = t.pendingProps, i = t.memoizedProps, o = l.value, ne(Ml, r._currentValue), r._currentValue = o, i !== null) if (gt(i.value, o)) {
          if (i.children === l.children && !We.current) {
            t = Mt(e, t, n);
            break e;
          }
        } else for (i = t.child, i !== null && (i.return = t); i !== null; ) {
          var s = i.dependencies;
          if (s !== null) {
            o = i.child;
            for (var u = s.firstContext; u !== null; ) {
              if (u.context === r) {
                if (i.tag === 1) {
                  u = Tt(-1, n & -n), u.tag = 2;
                  var d = i.updateQueue;
                  if (d !== null) {
                    d = d.shared;
                    var N = d.pending;
                    N === null ? u.next = u : (u.next = N.next, N.next = u), d.pending = u;
                  }
                }
                i.lanes |= n, u = i.alternate, u !== null && (u.lanes |= n), xi(
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
            if (o = i.return, o === null) throw Error(_(341));
            o.lanes |= n, s = o.alternate, s !== null && (s.lanes |= n), xi(o, n, t), o = i.sibling;
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
      return l = t.type, r = t.pendingProps.children, Vn(t, n), l = ot(l), r = r(l), t.flags |= 1, Me(e, t, r, n), t.child;
    case 14:
      return r = t.type, l = ft(r, t.pendingProps), l = ft(r.type, l), Ls(e, t, r, l, n);
    case 15:
      return Oc(e, t, t.type, t.pendingProps, n);
    case 17:
      return r = t.type, l = t.pendingProps, l = t.elementType === r ? l : ft(r, l), yl(e, t), t.tag = 1, Qe(r) ? (e = !0, zl(t)) : e = !1, Vn(t, n), Dc(t, r, l), yi(t, r, l, n), Si(null, t, r, !0, e, n);
    case 19:
      return Bc(e, t, n);
    case 22:
      return Ac(e, t, n);
  }
  throw Error(_(156, t.tag));
};
function ad(e, t) {
  return Tu(e, t);
}
function $p(e, t, n, r) {
  this.tag = e, this.key = n, this.sibling = this.child = this.return = this.stateNode = this.type = this.elementType = null, this.index = 0, this.ref = null, this.pendingProps = t, this.dependencies = this.memoizedState = this.updateQueue = this.memoizedProps = null, this.mode = r, this.subtreeFlags = this.flags = 0, this.deletions = null, this.childLanes = this.lanes = 0, this.alternate = null;
}
function at(e, t, n, r) {
  return new $p(e, t, n, r);
}
function Eo(e) {
  return e = e.prototype, !(!e || !e.isReactComponent);
}
function Op(e) {
  if (typeof e == "function") return Eo(e) ? 1 : 0;
  if (e != null) {
    if (e = e.$$typeof, e === Hi) return 11;
    if (e === Wi) return 14;
  }
  return 2;
}
function Jt(e, t) {
  var n = e.alternate;
  return n === null ? (n = at(e.tag, t, e.key, e.mode), n.elementType = e.elementType, n.type = e.type, n.stateNode = e.stateNode, n.alternate = e, e.alternate = n) : (n.pendingProps = t, n.type = e.type, n.flags = 0, n.subtreeFlags = 0, n.deletions = null), n.flags = e.flags & 14680064, n.childLanes = e.childLanes, n.lanes = e.lanes, n.child = e.child, n.memoizedProps = e.memoizedProps, n.memoizedState = e.memoizedState, n.updateQueue = e.updateQueue, t = e.dependencies, n.dependencies = t === null ? null : { lanes: t.lanes, firstContext: t.firstContext }, n.sibling = e.sibling, n.index = e.index, n.ref = e.ref, n;
}
function Sl(e, t, n, r, l, i) {
  var o = 2;
  if (r = e, typeof e == "function") Eo(e) && (o = 1);
  else if (typeof e == "string") o = 5;
  else e: switch (e) {
    case In:
      return mn(n.children, l, i, t);
    case Bi:
      o = 8, l |= 8;
      break;
    case Ba:
      return e = at(12, n, t, l | 2), e.elementType = Ba, e.lanes = i, e;
    case Ha:
      return e = at(13, n, t, l), e.elementType = Ha, e.lanes = i, e;
    case Wa:
      return e = at(19, n, t, l), e.elementType = Wa, e.lanes = i, e;
    case hu:
      return la(n, l, i, t);
    default:
      if (typeof e == "object" && e !== null) switch (e.$$typeof) {
        case pu:
          o = 10;
          break e;
        case mu:
          o = 9;
          break e;
        case Hi:
          o = 11;
          break e;
        case Wi:
          o = 14;
          break e;
        case Ut:
          o = 16, r = null;
          break e;
      }
      throw Error(_(130, e == null ? e : typeof e, ""));
  }
  return t = at(o, n, t, l), t.elementType = e, t.type = r, t.lanes = i, t;
}
function mn(e, t, n, r) {
  return e = at(7, e, r, t), e.lanes = n, e;
}
function la(e, t, n, r) {
  return e = at(22, e, r, t), e.elementType = hu, e.lanes = n, e.stateNode = { isHidden: !1 }, e;
}
function $a(e, t, n) {
  return e = at(6, e, null, t), e.lanes = n, e;
}
function Oa(e, t, n) {
  return t = at(4, e.children !== null ? e.children : [], e.key, t), t.lanes = n, t.stateNode = { containerInfo: e.containerInfo, pendingChildren: null, implementation: e.implementation }, t;
}
function Ap(e, t, n, r, l) {
  this.tag = t, this.containerInfo = e, this.finishedWork = this.pingCache = this.current = this.pendingChildren = null, this.timeoutHandle = -1, this.callbackNode = this.pendingContext = this.context = null, this.callbackPriority = 0, this.eventTimes = ga(0), this.expirationTimes = ga(-1), this.entangledLanes = this.finishedLanes = this.mutableReadLanes = this.expiredLanes = this.pingedLanes = this.suspendedLanes = this.pendingLanes = 0, this.entanglements = ga(0), this.identifierPrefix = r, this.onRecoverableError = l, this.mutableSourceEagerHydrationData = null;
}
function Io(e, t, n, r, l, i, o, s, u) {
  return e = new Ap(e, t, n, s, u), t === 1 ? (t = 1, i === !0 && (t |= 8)) : t = 0, i = at(3, null, null, t), e.current = i, i.stateNode = e, i.memoizedState = { element: r, isDehydrated: n, cache: null, transitions: null, pendingSuspenseBoundaries: null }, uo(i), e;
}
function Up(e, t, n) {
  var r = 3 < arguments.length && arguments[3] !== void 0 ? arguments[3] : null;
  return { $$typeof: En, key: r == null ? null : "" + r, children: e, containerInfo: t, implementation: n };
}
function id(e) {
  if (!e) return tn;
  e = e._reactInternals;
  e: {
    if (Sn(e) !== e || e.tag !== 1) throw Error(_(170));
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
    throw Error(_(171));
  }
  if (e.tag === 1) {
    var n = e.type;
    if (Qe(n)) return ic(e, n, t);
  }
  return t;
}
function od(e, t, n, r, l, i, o, s, u) {
  return e = Io(n, r, !0, e, l, i, o, s, u), e.context = id(null), n = e.current, r = Ae(), l = Zt(n), i = Tt(r, l), i.callback = t ?? null, Yt(n, i, l), e.current.lanes = l, Vr(e, l, r), Ge(e, r), e;
}
function aa(e, t, n, r) {
  var l = t.current, i = Ae(), o = Zt(l);
  return n = id(n), t.context === null ? t.context = n : t.pendingContext = n, t = Tt(i, o), t.payload = { element: e }, r = r === void 0 ? null : r, r !== null && (t.callback = r), e = Yt(l, t, o), e !== null && (vt(e, l, o, i), vl(e, l, o)), o;
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
function Qs(e, t) {
  if (e = e.memoizedState, e !== null && e.dehydrated !== null) {
    var n = e.retryLane;
    e.retryLane = n !== 0 && n < t ? n : t;
  }
}
function Po(e, t) {
  Qs(e, t), (e = e.alternate) && Qs(e, t);
}
function bp() {
  return null;
}
var sd = typeof reportError == "function" ? reportError : function(e) {
  console.error(e);
};
function Fo(e) {
  this._internalRoot = e;
}
ia.prototype.render = Fo.prototype.render = function(e) {
  var t = this._internalRoot;
  if (t === null) throw Error(_(409));
  aa(e, t, null, null);
};
ia.prototype.unmount = Fo.prototype.unmount = function() {
  var e = this._internalRoot;
  if (e !== null) {
    this._internalRoot = null;
    var t = e.containerInfo;
    jn(function() {
      aa(null, e, null, null);
    }), t[Lt] = null;
  }
};
function ia(e) {
  this._internalRoot = e;
}
ia.prototype.unstable_scheduleHydration = function(e) {
  if (e) {
    var t = Au();
    e = { blockedOn: null, target: e, priority: t };
    for (var n = 0; n < Vt.length && t !== 0 && t < Vt[n].priority; n++) ;
    Vt.splice(n, 0, e), n === 0 && bu(e);
  }
};
function _o(e) {
  return !(!e || e.nodeType !== 1 && e.nodeType !== 9 && e.nodeType !== 11);
}
function oa(e) {
  return !(!e || e.nodeType !== 1 && e.nodeType !== 9 && e.nodeType !== 11 && (e.nodeType !== 8 || e.nodeValue !== " react-mount-point-unstable "));
}
function Gs() {
}
function Vp(e, t, n, r, l) {
  if (l) {
    if (typeof r == "function") {
      var i = r;
      r = function() {
        var d = Ql(o);
        i.call(d);
      };
    }
    var o = od(t, r, e, 0, null, !1, !1, "", Gs);
    return e._reactRootContainer = o, e[Lt] = o.current, Tr(e.nodeType === 8 ? e.parentNode : e), jn(), o;
  }
  for (; l = e.lastChild; ) e.removeChild(l);
  if (typeof r == "function") {
    var s = r;
    r = function() {
      var d = Ql(u);
      s.call(d);
    };
  }
  var u = Io(e, 0, !1, null, null, !1, !1, "", Gs);
  return e._reactRootContainer = u, e[Lt] = u.current, Tr(e.nodeType === 8 ? e.parentNode : e), jn(function() {
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
  } else o = Vp(n, t, e, l, r);
  return Ql(o);
}
$u = function(e) {
  switch (e.tag) {
    case 3:
      var t = e.stateNode;
      if (t.current.memoizedState.isDehydrated) {
        var n = fr(t.pendingLanes);
        n !== 0 && (Ki(t, n | 1), Ge(t, ve()), !(Y & 6) && (Xn = ve() + 500, an()));
      }
      break;
    case 13:
      jn(function() {
        var r = Dt(e, 1);
        if (r !== null) {
          var l = Ae();
          vt(r, e, 1, l);
        }
      }), Po(e, 1);
  }
};
qi = function(e) {
  if (e.tag === 13) {
    var t = Dt(e, 134217728);
    if (t !== null) {
      var n = Ae();
      vt(t, e, 134217728, n);
    }
    Po(e, 134217728);
  }
};
Ou = function(e) {
  if (e.tag === 13) {
    var t = Zt(e), n = Dt(e, t);
    if (n !== null) {
      var r = Ae();
      vt(n, e, t, r);
    }
    Po(e, t);
  }
};
Au = function() {
  return Z;
};
Uu = function(e, t) {
  var n = Z;
  try {
    return Z = e, t();
  } finally {
    Z = n;
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
            if (!l) throw Error(_(90));
            xu(r), Ka(r, l);
          }
        }
      }
      break;
    case "textarea":
      yu(e, n);
      break;
    case "select":
      t = n.value, t != null && On(e, !!n.multiple, t, !1);
  }
};
Eu = wo;
Iu = jn;
var Bp = { usingClientEntryPoint: !1, Events: [Hr, Rn, Zl, ku, Cu, wo] }, ur = { findFiberByHostInstance: cn, bundleType: 0, version: "18.3.1", rendererPackageName: "react-dom" }, Hp = { bundleType: ur.bundleType, version: ur.version, rendererPackageName: ur.rendererPackageName, rendererConfig: ur.rendererConfig, overrideHookState: null, overrideHookStateDeletePath: null, overrideHookStateRenamePath: null, overrideProps: null, overridePropsDeletePath: null, overridePropsRenamePath: null, setErrorHandler: null, setSuspenseHandler: null, scheduleUpdate: null, currentDispatcherRef: $t.ReactCurrentDispatcher, findHostInstanceByFiber: function(e) {
  return e = _u(e), e === null ? null : e.stateNode;
}, findFiberByHostInstance: ur.findFiberByHostInstance || bp, findHostInstancesForRefresh: null, scheduleRefresh: null, scheduleRoot: null, setRefreshHandler: null, getCurrentFiber: null, reconcilerVersion: "18.3.1-next-f1338f8080-20240426" };
if (typeof __REACT_DEVTOOLS_GLOBAL_HOOK__ < "u") {
  var cl = __REACT_DEVTOOLS_GLOBAL_HOOK__;
  if (!cl.isDisabled && cl.supportsFiber) try {
    Kl = cl.inject(Hp), St = cl;
  } catch {
  }
}
et.__SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED = Bp;
et.createPortal = function(e, t) {
  var n = 2 < arguments.length && arguments[2] !== void 0 ? arguments[2] : null;
  if (!_o(t)) throw Error(_(200));
  return Up(e, t, null, n);
};
et.createRoot = function(e, t) {
  if (!_o(e)) throw Error(_(299));
  var n = !1, r = "", l = sd;
  return t != null && (t.unstable_strictMode === !0 && (n = !0), t.identifierPrefix !== void 0 && (r = t.identifierPrefix), t.onRecoverableError !== void 0 && (l = t.onRecoverableError)), t = Io(e, 1, !1, null, null, n, !1, r, l), e[Lt] = t.current, Tr(e.nodeType === 8 ? e.parentNode : e), new Fo(t);
};
et.findDOMNode = function(e) {
  if (e == null) return null;
  if (e.nodeType === 1) return e;
  var t = e._reactInternals;
  if (t === void 0)
    throw typeof e.render == "function" ? Error(_(188)) : (e = Object.keys(e).join(","), Error(_(268, e)));
  return e = _u(t), e = e === null ? null : e.stateNode, e;
};
et.flushSync = function(e) {
  return jn(e);
};
et.hydrate = function(e, t, n) {
  if (!oa(t)) throw Error(_(200));
  return sa(null, e, t, !0, n);
};
et.hydrateRoot = function(e, t, n) {
  if (!_o(e)) throw Error(_(405));
  var r = n != null && n.hydratedSources || null, l = !1, i = "", o = sd;
  if (n != null && (n.unstable_strictMode === !0 && (l = !0), n.identifierPrefix !== void 0 && (i = n.identifierPrefix), n.onRecoverableError !== void 0 && (o = n.onRecoverableError)), t = od(t, null, e, 1, n ?? null, l, !1, i, o), e[Lt] = t.current, Tr(e), r) for (e = 0; e < r.length; e++) n = r[e], l = n._getVersion, l = l(n._source), t.mutableSourceEagerHydrationData == null ? t.mutableSourceEagerHydrationData = [n, l] : t.mutableSourceEagerHydrationData.push(
    n,
    l
  );
  return new ia(t);
};
et.render = function(e, t, n) {
  if (!oa(t)) throw Error(_(200));
  return sa(null, e, t, !1, n);
};
et.unmountComponentAtNode = function(e) {
  if (!oa(e)) throw Error(_(40));
  return e._reactRootContainer ? (jn(function() {
    sa(null, null, e, !1, function() {
      e._reactRootContainer = null, e[Lt] = null;
    });
  }), !0) : !1;
};
et.unstable_batchedUpdates = wo;
et.unstable_renderSubtreeIntoContainer = function(e, t, n, r) {
  if (!oa(n)) throw Error(_(200));
  if (e == null || e._reactInternals === void 0) throw Error(_(38));
  return sa(e, t, n, !1, r);
};
et.version = "18.3.1-next-f1338f8080-20240426";
function ud() {
  if (!(typeof __REACT_DEVTOOLS_GLOBAL_HOOK__ > "u" || typeof __REACT_DEVTOOLS_GLOBAL_HOOK__.checkDCE != "function"))
    try {
      __REACT_DEVTOOLS_GLOBAL_HOOK__.checkDCE(ud);
    } catch (e) {
      console.error(e);
    }
}
ud(), uu.exports = et;
var Wp = uu.exports, cd, Ks = Wp;
cd = Ks.createRoot, Ks.hydrateRoot;
class Qp extends Error {
  constructor(t, n) {
    super(t), this.status = n, this.name = "ApiError";
  }
}
function Gp(e, t) {
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
      throw new Qp(N, u.status);
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
const dd = g.createContext(null);
function kt() {
  const e = g.useContext(dd);
  if (!e) throw new Error("Fuera del módulo de documentos");
  return e;
}
function Kp(e) {
  return Gp((t, n) => fetch(t, n), e.token);
}
async function Li(e, t) {
  const n = e.token(), r = await fetch(t, { headers: n ? { Authorization: "Bearer " + n } : {} });
  if (!r.ok) throw new Error("No se pudo generar el documento");
  const l = URL.createObjectURL(await r.blob());
  window.open(l, "_blank"), setTimeout(() => URL.revokeObjectURL(l), 6e4);
}
function fd(e, t) {
  return `${e.toLowerCase().normalize("NFD").replace(/[̀-ͯ]/g, "").replace(/[^a-z0-9]+/g, "-").replace(/^-|-$/g, "").slice(0, 60) || "listado"}-${(/* @__PURE__ */ new Date()).toISOString().slice(0, 10)}.${t}`;
}
function pd(e, t) {
  const n = URL.createObjectURL(e), r = document.createElement("a");
  r.href = n, r.download = t, document.body.appendChild(r), r.click(), r.remove(), setTimeout(() => URL.revokeObjectURL(n), 6e4);
}
async function qp(e, t) {
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
  pd(await n.blob(), fd(t.titulo, "xlsx"));
}
function Yp(e) {
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
function Xp(e) {
  pd(new Blob(["\uFEFF" + Yp(e)], { type: "text/csv;charset=utf-8" }), fd(e.titulo, "csv"));
}
const md = new Intl.NumberFormat("es-ES", { minimumFractionDigits: 2, maximumFractionDigits: 2 }), Zp = new Intl.NumberFormat("es-ES", { maximumFractionDigits: 3 }), z = (e) => `${md.format(Number(e) || 0)} €`, we = (e) => md.format(Number(e) || 0), $e = (e) => Zp.format(Number(e) || 0), je = (e) => {
  if (!e) return "";
  const t = String(e).slice(0, 10).split("-");
  return t.length === 3 ? `${t[2]}/${t[1]}/${t[0]}` : String(e);
}, xt = () => (/* @__PURE__ */ new Date()).toISOString().slice(0, 10), Oe = (e) => Math.round((e + Number.EPSILON) * 100) / 100;
let Jp = 0;
const Qr = () => `l${Date.now().toString(36)}${(++Jp).toString(36)}`;
function hn(e, t) {
  const [n, r] = g.useState(e);
  return g.useEffect(() => {
    const l = setTimeout(() => r(e), t);
    return () => clearTimeout(l);
  }, [e, t]), n;
}
function Gr() {
  const e = g.useRef(0);
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
const qs = { presupuesto: "Presupuestos", pedido: "Pedidos de venta", factura: "Facturas emitidas", compra: "Pedidos de compra", gasto: "Facturas de proveedor y gastos" }, em = { presupuesto: "+ Nuevo presupuesto", pedido: "+ Nuevo pedido", factura: "+ Nueva factura", compra: "+ Nuevo pedido", gasto: "+ Registrar factura" }, tm = {
  presupuesto: ["Borrador", "Aceptado", "Rechazado"],
  pedido: ["Borrador", "Confirmado", "ServidoParcial", "Servido", "Facturado", "Cancelado"],
  factura: ["Emitida", "Rectificada", "Anulada"],
  compra: ["Borrador", "Confirmado", "RecibidoParcial", "Recibido", "Facturado", "Cancelado"],
  gasto: ["Registrado", "Anulado"]
}, Aa = { numero: "numero", fecha: "fecha", tercero: "tercero", base: "base", impuestos: "impuestos", total: "total" }, Ys = 50, Di = (e) => e === "Anulada" || e === "Anulado" || e === "Cancelado";
function Xs(e, t) {
  const n = { documentos: 0, baseImponible: 0, impuestos: 0, retenciones: 0, total: 0, pendiente: 0, vencido: 0, documentosVencidos: 0 };
  for (const r of e) {
    if (Di(r.estado)) continue;
    n.documentos++, n.baseImponible += r.base, n.impuestos += r.impuestos, n.total += r.total;
    const l = r.pendiente ?? 0;
    l > 0 && (n.pendiente += l, r.vencimiento && r.vencimiento < t && (n.vencido += l, n.documentosVencidos++));
  }
  return n.baseImponible = Oe(n.baseImponible), n.impuestos = Oe(n.impuestos), n.total = Oe(n.total), n.pendiente = Oe(n.pendiente), n.vencido = Oe(n.vencido), n;
}
function Ua(e, t, n) {
  const r = (l) => t === "numero" || t === "tercero" || t === "estado" ? l[t].toLowerCase() : t === "fecha" ? l.fecha : l[t] ?? 0;
  return [...e].sort((l, i) => {
    const o = r(l), s = r(i), u = typeof o == "number" && typeof s == "number" ? o - s : String(o).localeCompare(String(s), "es", { numeric: !0 });
    return n ? -u : u;
  });
}
const nm = (e, t) => ({
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
}), rm = (e, t) => {
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
function lm(e) {
  const { api: t, navegar: n, anfitrion: r } = kt(), l = e.tipo, i = l === "factura" || l === "gasto", o = l === "compra" || l === "gasto", [s, u] = g.useState(""), [d, N] = g.useState(""), [c, h] = g.useState(""), [x, y] = g.useState(""), [S, M] = g.useState(""), [p, f] = g.useState(""), [v, C] = g.useState(""), [k, P] = g.useState(""), [T, D] = g.useState(""), [O, U] = g.useState({ campo: "fecha", desc: !0 }), [R, Q] = g.useState(1), [se, Pe] = g.useState(null), [De, fe] = g.useState(0), [xe, E] = g.useState(null), [j, w] = g.useState([]), [b, W] = g.useState([]), [q, pe] = g.useState(""), [me, te] = g.useState(!1), A = hn(s, 250), ut = hn(v, 350), Ke = hn(k, 350), ct = Gr(), Ct = xt();
  g.useEffect(() => {
    t.get(o ? "/proveedores" : "/clientes").then((m) => w([...m].sort((L, F) => L.nombre.localeCompare(F.nombre, "es")))).catch(() => w([])), l === "factura" && t.get("/series").then((m) => W([...new Set(m.filter((L) => L.tipoDocumento === "Factura" || L.tipoDocumento === 0).map((L) => L.prefijo))].sort())).catch(() => W([]));
  }, [t, l, o]), g.useEffect(() => Q(1), [A, d, c, x, S, p, ut, Ke, T, O, l]);
  const Ot = (m, L) => {
    const F = new URLSearchParams({ pagina: String(m), tamanoPagina: String(L) });
    A.trim() && F.set("texto", A.trim()), d && F.set("estado", d === "Anulada" && l === "gasto" ? "Anulado" : d), c && F.set("desde", c), x && F.set("hasta", x), S && F.set(l === "gasto" ? "proveedorId" : "clienteId", S), p && l === "factura" && F.set("serie", p);
    const V = parseFloat(ut.replace(/\./g, "").replace(",", ".")), H = parseFloat(Ke.replace(/\./g, "").replace(",", "."));
    isNaN(V) || F.set("importeMin", String(V)), isNaN(H) || F.set("importeMax", String(H)), T && F.set("cobro", T);
    const J = Aa[O.campo];
    return J && (F.set("orden", J === "tercero" ? l === "gasto" ? "proveedor" : "cliente" : J), F.set("desc", String(O.desc))), F;
  }, wn = async (m, L) => {
    if (l === "factura") {
      const V = await t.get(`/facturas/buscar?${Ot(m, L)}`);
      return { r: V, filas: V.elementos.map((H) => nm(H, V.pendientes)) };
    }
    const F = await t.get(`/gastos/buscar?${Ot(m, L)}`);
    return { r: F, filas: F.elementos.map((V) => rm(V, F.pendientes)) };
  };
  g.useEffect(() => {
    pe("");
    const m = ct();
    (async () => {
      if (i) {
        const { r: F, filas: V } = await wn(R, Ys);
        return m() && (fe(F.total), E(F.totales ?? null)), V;
      }
      switch (l) {
        case "presupuesto":
          return (await t.get("/presupuestos")).map((F) => ({ id: F.id, numero: F.numeroCompleto, fecha: F.fecha, tercero: F.clienteNombre, terceroId: F.clienteId, base: F.baseImponible ?? F.total, impuestos: F.cuotaIva ?? 0, total: F.total, estado: F.estado }));
        case "pedido":
          return (await t.get("/pedidos-venta")).map((F) => {
            const V = Oe(F.lineas.reduce((H, J) => H + J.base, 0));
            return { id: F.id, numero: F.numeroCompleto, fecha: F.fecha, tercero: F.clienteNombre, terceroId: F.clienteId, base: V, impuestos: Oe(F.total - V), total: F.total, estado: F.estado };
          });
        default:
          return (await t.get("/compras/pedidos")).map((F) => {
            const V = Oe(F.lineas.reduce((H, J) => H + J.importe, 0));
            return { id: F.id, numero: F.numeroCompleto, fecha: F.fecha, tercero: F.proveedorTexto, terceroId: F.proveedorId, base: V, impuestos: Oe(F.total - V), total: F.total, estado: F.estado, extra: F.empresaOrigenId ? "Intragrupo" : void 0 };
          });
      }
    })().then((F) => m() && Pe(F)).catch((F) => m() && (pe(F.message), Pe([])));
  }, [t, l, R, A, d, c, x, S, p, ut, Ke, T, O.campo, O.desc]);
  const ie = g.useMemo(() => {
    if (!se) return [];
    if (i) return Aa[O.campo] ? se : Ua(se, O.campo, O.desc);
    const m = A.trim().toLowerCase(), L = parseFloat(ut.replace(/\./g, "").replace(",", ".")), F = parseFloat(Ke.replace(/\./g, "").replace(",", ".")), V = se.filter((H) => (!m || H.numero.toLowerCase().includes(m) || H.tercero.toLowerCase().includes(m)) && (!d || H.estado === d) && (!c || H.fecha >= c) && (!x || H.fecha <= x) && (!S || H.terceroId === S) && (isNaN(L) || H.total >= L) && (isNaN(F) || H.total <= F));
    return Ua(V, O.campo, O.desc);
  }, [se, A, d, c, x, S, ut, Ke, O, i]), K = i ? xe : Xs(ie, Ct), qe = i ? Math.max(1, Math.ceil(De / Ys)) : 1, Fe = [d, c, x, S, p, v, k, T].filter(Boolean).length, Et = o ? "Proveedor" : "Cliente";
  function It() {
    u(""), N(""), h(""), y(""), M(""), f(""), C(""), P(""), D("");
  }
  function Ve(m, L, F = !1) {
    const V = O.campo === m;
    return /* @__PURE__ */ a.jsxs("th", { className: (F ? "num " : "") + "dx-ordenable" + (V ? " activo" : ""), onClick: () => U({ campo: m, desc: V ? !O.desc : m === "fecha" || F }), title: `Ordenar por ${L.toLowerCase()}`, children: [
      L,
      /* @__PURE__ */ a.jsx("span", { className: "dx-flecha", children: V ? O.desc ? "▼" : "▲" : "" })
    ] });
  }
  async function tr() {
    if (!i) return ie;
    const m = [];
    for (let L = 1; L <= 500; L++) {
      const { r: F, filas: V } = await wn(L, 200);
      if (m.push(...V), m.length >= F.total || V.length === 0) break;
    }
    return Aa[O.campo] ? m : Ua(m, O.campo, O.desc);
  }
  async function on(m) {
    te(!0);
    try {
      const L = await tr(), F = i, V = i ? xe : Xs(L, Ct), H = {
        titulo: qs[l],
        columnas: [
          { titulo: "Número", tipo: "texto" },
          { titulo: "Fecha", tipo: "fecha" },
          { titulo: Et, tipo: "texto" },
          { titulo: "Estado", tipo: "texto" },
          { titulo: "Base", tipo: "moneda", total: "no" },
          { titulo: "Impuestos", tipo: "moneda", total: "no" },
          { titulo: "Total", tipo: "moneda", total: "no" },
          ...F ? [{ titulo: "Pendiente", tipo: "moneda", total: "no" }, { titulo: "Vencimiento", tipo: "fecha" }] : []
        ],
        filas: L.map((J) => [J.numero + (J.extra ? ` (${J.extra})` : ""), je(J.fecha), J.tercero, J.estado, J.base, J.impuestos, J.total, ...F ? [J.pendiente ?? 0, je(J.vencimiento)] : []]),
        totales: V ? [`Total · ${V.documentos} (sin anulados)`, null, null, null, V.baseImponible, V.impuestos, V.total, ...F ? [V.pendiente, null] : []] : void 0
      };
      m === "xlsx" ? await qp(r.token(), H) : Xp(H), r.aviso(`Exportados ${L.length} documento(s).`, "ok");
    } catch (L) {
      r.aviso("No se pudo exportar: " + L.message, "err");
    } finally {
      te(!1);
    }
  }
  return /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
    /* @__PURE__ */ a.jsxs("div", { className: "panel-head", children: [
      /* @__PURE__ */ a.jsx("h2", { children: qs[l] }),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-acciones", children: [
        /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", disabled: me || !ie.length, onClick: () => on("xlsx"), title: "Exportar a Excel todo lo filtrado", children: me ? "Exportando…" : "↓ Excel" }),
        /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", disabled: me || !ie.length, onClick: () => on("csv"), title: "Exportar a CSV todo lo filtrado", children: "↓ CSV" }),
        /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: () => n({ tipo: l, pantalla: "editor" }), children: em[l] })
      ] })
    ] }),
    /* @__PURE__ */ a.jsxs("div", { className: "dx-filtros", children: [
      /* @__PURE__ */ a.jsx("input", { placeholder: o ? "Número, proveedor o concepto…" : "Número, cliente o NIF…", value: s, onChange: (m) => u(m.target.value), autoFocus: !0 }),
      /* @__PURE__ */ a.jsxs("select", { value: d, onChange: (m) => N(m.target.value), "aria-label": "Estado", children: [
        /* @__PURE__ */ a.jsx("option", { value: "", children: "Todos los estados" }),
        tm[l].map((m) => /* @__PURE__ */ a.jsx("option", { value: m, children: m }, m))
      ] }),
      /* @__PURE__ */ a.jsx("input", { type: "date", value: c, onChange: (m) => h(m.target.value), title: "Desde", "aria-label": "Desde" }),
      /* @__PURE__ */ a.jsx("input", { type: "date", value: x, onChange: (m) => y(m.target.value), title: "Hasta", "aria-label": "Hasta" })
    ] }),
    /* @__PURE__ */ a.jsxs("div", { className: "dx-filtros dx-filtros2", children: [
      /* @__PURE__ */ a.jsxs("select", { value: S, onChange: (m) => M(m.target.value), "aria-label": Et, children: [
        /* @__PURE__ */ a.jsx("option", { value: "", children: o ? "Todos los proveedores" : "Todos los clientes" }),
        j.map((m) => /* @__PURE__ */ a.jsxs("option", { value: m.id, children: [
          m.nombre,
          m.nifFiscal ? ` · ${m.nifFiscal}` : ""
        ] }, m.id))
      ] }),
      l === "factura" && /* @__PURE__ */ a.jsxs("select", { value: p, onChange: (m) => f(m.target.value), "aria-label": "Serie", children: [
        /* @__PURE__ */ a.jsx("option", { value: "", children: "Todas las series" }),
        b.map((m) => /* @__PURE__ */ a.jsxs("option", { value: m, children: [
          "Serie ",
          m
        ] }, m))
      ] }),
      /* @__PURE__ */ a.jsx("input", { inputMode: "decimal", placeholder: "Importe mín.", value: v, onChange: (m) => C(m.target.value), "aria-label": "Importe mínimo" }),
      /* @__PURE__ */ a.jsx("input", { inputMode: "decimal", placeholder: "Importe máx.", value: k, onChange: (m) => P(m.target.value), "aria-label": "Importe máximo" }),
      i && /* @__PURE__ */ a.jsxs("select", { value: T, onChange: (m) => D(m.target.value), "aria-label": l === "gasto" ? "Estado de pago" : "Estado de cobro", children: [
        /* @__PURE__ */ a.jsx("option", { value: "", children: l === "gasto" ? "Pagadas y pendientes" : "Cobradas y pendientes" }),
        /* @__PURE__ */ a.jsx("option", { value: "pendiente", children: "Pendientes" }),
        /* @__PURE__ */ a.jsx("option", { value: "vencida", children: "Vencidas" }),
        /* @__PURE__ */ a.jsx("option", { value: l === "gasto" ? "pagada" : "cobrada", children: l === "gasto" ? "Pagadas" : "Cobradas" })
      ] }),
      (Fe > 0 || s) && /* @__PURE__ */ a.jsxs("button", { className: "btn small secondary", onClick: It, children: [
        "Limpiar",
        Fe ? ` (${Fe})` : ""
      ] })
    ] }),
    q && /* @__PURE__ */ a.jsx("p", { className: "dx-rojo", children: q }),
    se === null ? /* @__PURE__ */ a.jsx("p", { className: "muted", children: "Cargando…" }) : ie.length === 0 ? /* @__PURE__ */ a.jsx("p", { className: "muted", children: "No hay documentos con estos filtros." }) : /* @__PURE__ */ a.jsxs("table", { className: "dx-lista-docs", children: [
      /* @__PURE__ */ a.jsx("thead", { children: /* @__PURE__ */ a.jsxs("tr", { children: [
        Ve("numero", "Número"),
        Ve("fecha", "Fecha"),
        Ve("tercero", Et),
        Ve("estado", "Estado"),
        Ve("base", "Base", !0),
        Ve("impuestos", "Impuestos", !0),
        Ve("total", "Total", !0),
        i && Ve("pendiente", "Pendiente", !0)
      ] }) }),
      /* @__PURE__ */ a.jsx("tbody", { children: ie.map((m) => {
        const L = (m.pendiente ?? 0) > 0 && !!m.vencimiento && m.vencimiento < Ct;
        return /* @__PURE__ */ a.jsxs("tr", { onClick: () => n({ tipo: l, pantalla: "vista", id: m.id }), tabIndex: 0, onKeyDown: (F) => F.key === "Enter" && n({ tipo: l, pantalla: "vista", id: m.id }), className: Di(m.estado) ? "dx-anulado" : void 0, children: [
          /* @__PURE__ */ a.jsxs("td", { className: "mono", children: [
            /* @__PURE__ */ a.jsx("strong", { children: m.numero }),
            m.extra && /* @__PURE__ */ a.jsx("span", { className: "pill part", style: { marginLeft: 6 }, children: m.extra })
          ] }),
          /* @__PURE__ */ a.jsx("td", { children: je(m.fecha) }),
          /* @__PURE__ */ a.jsx("td", { children: m.tercero }),
          /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx("span", { className: Ro(m.estado), children: m.estado }) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: z(m.base) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: z(m.impuestos) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: /* @__PURE__ */ a.jsx("strong", { children: z(m.total) }) }),
          i && /* @__PURE__ */ a.jsx("td", { className: "num", children: (m.pendiente ?? 0) > 0 ? /* @__PURE__ */ a.jsx("strong", { className: L ? "dx-rojo" : void 0, title: L ? `Vencida el ${je(m.vencimiento)}` : `Vence el ${je(m.vencimiento)}`, children: z(m.pendiente) }) : /* @__PURE__ */ a.jsx("span", { className: "muted", children: Di(m.estado) || m.estado === "Rectificada" ? "—" : l === "gasto" ? "Pagada" : "Cobrada" }) })
        ] }, m.id);
      }) }),
      K && /* @__PURE__ */ a.jsx("tfoot", { children: /* @__PURE__ */ a.jsxs("tr", { className: "dx-total", children: [
        /* @__PURE__ */ a.jsxs("td", { colSpan: 4, children: [
          /* @__PURE__ */ a.jsxs("strong", { children: [
            "Total · ",
            K.documentos
          ] }),
          " ",
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: i ? `documento${K.documentos === 1 ? "" : "s"} de todo el filtro (${qe} página${qe === 1 ? "" : "s"}) · sin anulados` : "sin anulados ni cancelados" }),
          i && K.vencido > 0 && /* @__PURE__ */ a.jsxs("span", { className: "dx-rojo", style: { marginLeft: 8 }, children: [
            "· vencido ",
            z(K.vencido),
            " (",
            K.documentosVencidos,
            ")"
          ] })
        ] }),
        /* @__PURE__ */ a.jsx("td", { className: "num", children: /* @__PURE__ */ a.jsx("strong", { children: z(K.baseImponible) }) }),
        /* @__PURE__ */ a.jsx("td", { className: "num", children: /* @__PURE__ */ a.jsx("strong", { children: z(K.impuestos) }) }),
        /* @__PURE__ */ a.jsx("td", { className: "num", children: /* @__PURE__ */ a.jsx("strong", { children: z(K.total) }) }),
        i && /* @__PURE__ */ a.jsx("td", { className: "num", children: /* @__PURE__ */ a.jsx("strong", { children: z(K.pendiente) }) })
      ] }) })
    ] }),
    qe > 1 && /* @__PURE__ */ a.jsxs("div", { className: "dx-paginas", children: [
      /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", disabled: R <= 1, onClick: () => Q(R - 1), children: "←" }),
      /* @__PURE__ */ a.jsxs("span", { className: "muted", children: [
        "Página ",
        R,
        " de ",
        qe,
        " · ",
        De,
        " documentos"
      ] }),
      /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", disabled: R >= qe, onClick: () => Q(R + 1), children: "→" })
    ] })
  ] });
}
function nn(e) {
  return g.useEffect(() => {
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
function am(e) {
  const { api: t } = kt(), [n, r] = g.useState(!1), [l, i] = g.useState([]), [o, s] = g.useState(null), [u, d] = g.useState(0), N = hn(e.texto, 180), c = o === e.texto.trim() ? l : [], h = Gr();
  g.useEffect(() => {
    if (!n) return;
    const y = h(), S = encodeURIComponent(N.trim());
    t.get(`/productos/buscar?texto=${S}&tamanoPagina=12`).then((M) => y() && (i(M.elementos ?? []), s(N.trim()), d(0))).catch(() => y() && (i([]), s(N.trim())));
  }, [N, n]);
  function x(y) {
    var S;
    if (n && y.key === "Enter" && e.texto.trim() && !c.length) {
      y.preventDefault(), y.stopPropagation();
      return;
    }
    if (n && c.length) {
      if (y.key === "ArrowDown") return y.preventDefault(), d((M) => Math.min(M + 1, c.length - 1));
      if (y.key === "ArrowUp") return y.preventDefault(), d((M) => Math.max(M - 1, 0));
      if (y.key === "Enter") {
        y.preventDefault(), y.stopPropagation(), e.alElegir(c[u]), r(!1);
        return;
      }
    }
    if (y.key === "Escape") return r(!1);
    if (y.key === "F2") return y.preventDefault(), r(!0);
    (S = e.alTeclaFuera) == null || S.call(e, y);
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
        onKeyDown: x,
        ...Object.fromEntries(Object.entries(e.datos ?? {}).map(([y, S]) => [`data-${y}`, S]))
      }
    ),
    n && c.length > 0 && /* @__PURE__ */ a.jsx("div", { className: "dx-lista", children: c.map((y, S) => /* @__PURE__ */ a.jsxs(
      "div",
      {
        className: "dx-opcion" + (S === u ? " activa" : ""),
        onMouseDown: (M) => (M.preventDefault(), e.alElegir(y), r(!1)),
        onMouseEnter: () => d(S),
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
            z(e.precioDe ? e.precioDe(y) : y.precioUnitario),
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
  const [t, n] = g.useState(""), [r, l] = g.useState(!1), [i, o] = g.useState(0), s = e.terceros.find((c) => c.id === e.valor), u = g.useMemo(() => {
    const c = t.trim().toLowerCase();
    return e.terceros.filter((h) => h.activo !== !1 && (!c || h.nombre.toLowerCase().includes(c) || (h.nifFiscal ?? "").toLowerCase().includes(c))).slice(0, 30);
  }, [t, e.terceros]), d = g.useRef(null);
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
      r && u.length > 0 && /* @__PURE__ */ a.jsx("div", { className: "dx-lista", children: u.map((c, h) => /* @__PURE__ */ a.jsxs("div", { className: "dx-opcion" + (h === i ? " activa" : ""), onMouseDown: (x) => (x.preventDefault(), N(c)), onMouseEnter: () => o(h), children: [
        /* @__PURE__ */ a.jsx("strong", { children: c.nombre }),
        /* @__PURE__ */ a.jsx("span", { className: "muted", children: [c.nifFiscal, c.poblacion].filter(Boolean).join(" · ") })
      ] }, c.id)) })
    ] })
  ] });
}
const im = { Porcentaje: "%", PorUnidad: "€/ud", PorKilo: "€/kg", Importe: "€" };
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
          im[s.calculo],
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
    /* @__PURE__ */ a.jsx("strong", { children: z(n.importe) })
  ] }, r)) }) : null;
}
const wr = () => ({ clave: Qr(), productoId: null, descripcion: "", cantidad: 1, precio: null, dto: 0, iva: null });
function hd(e) {
  const t = g.useRef(null), [n, r] = g.useState(/* @__PURE__ */ new Set()), l = e.modo === "venta", i = l ? 7 : 5, o = (c, h) => e.alCambiar(e.lineas.map((x) => x.clave === c ? { ...x, ...h } : x)), s = (c) => {
    const h = e.lineas.filter((x) => x.clave !== c);
    e.alCambiar(h.length ? h : [wr()]);
  };
  function u(c, h) {
    var y;
    const x = (y = t.current) == null ? void 0 : y.querySelector(`[data-f="${c}"][data-c="${h}"]`);
    x == null || x.focus(), x instanceof HTMLInputElement && x.select();
  }
  function d(c) {
    const h = c.target, x = Number(h.dataset.f), y = Number(h.dataset.c);
    if (!(Number.isNaN(x) || Number.isNaN(y)))
      if (c.key === "Enter") {
        if (c.preventDefault(), y < i - 1) return u(x, y + 1);
        x === e.lineas.length - 1 ? (e.alCambiar([...e.lineas, wr()]), setTimeout(() => u(x + 1, 0), 30)) : u(x + 1, 0);
      } else c.key === "ArrowDown" && h.tagName !== "SELECT" ? (c.preventDefault(), u(Math.min(x + 1, e.lineas.length - 1), y)) : c.key === "ArrowUp" && h.tagName !== "SELECT" && (c.preventDefault(), u(Math.max(x - 1, 0), y));
  }
  const N = (c) => r((h) => {
    const x = new Set(h);
    return x.has(c) ? x.delete(c) : x.add(c), x;
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
        const x = e.calculos[h], y = (x == null ? void 0 : x.conceptos) ?? [], S = l && c.controlarStock && c.stock != null && c.cantidad > c.stock, M = x && x.margen != null && x.importe ? x.margen / x.importe * 100 : null;
        return [
          /* @__PURE__ */ a.jsxs("tr", { className: h % 2 ? "dx-par" : "", children: [
            /* @__PURE__ */ a.jsxs("td", { children: [
              e.soloLectura ? /* @__PURE__ */ a.jsx("span", { className: "mono", children: c.referencia ?? "" }) : /* @__PURE__ */ a.jsx(
                am,
                {
                  texto: c.referencia ?? (c.productoId ? c.descripcion : ""),
                  alCambiarTexto: (p) => o(c.clave, { referencia: p, ...p === "" ? { productoId: null } : {} }),
                  alElegir: (p) => (e.alElegirArticulo(c.clave, p), u(h, 2)),
                  precioDe: l ? void 0 : (p) => p.precioCompraPorUnidadCompra ?? p.precioCompra,
                  datos: { f: h, c: 0 }
                }
              ),
              c.productoId && /* @__PURE__ */ a.jsxs("div", { className: "dx-sub", children: [
                c.unidad && /* @__PURE__ */ a.jsx("span", { children: c.unidad }),
                c.controlarStock && /* @__PURE__ */ a.jsxs("span", { className: S ? "dx-rojo" : "", children: [
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
                  onChange: (p) => o(c.clave, { descripcion: p.target.value })
                }
              ),
              y.length > 0 && !n.has(c.clave) && /* @__PURE__ */ a.jsx("button", { type: "button", className: "dx-resumen-conc", onClick: () => N(c.clave), title: "Ver y cambiar los conceptos", children: y.map((p) => `${p.importe < 0 ? "−" : "+"} ${p.codigo.toLowerCase()} ${we(Math.abs(p.importe))}${p.efecto === "Coste" ? " (coste)" : ""}`).join(" · ") })
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
                onChange: (p) => o(c.clave, { cantidad: Number(p.target.value) })
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
                placeholder: x ? we(x.precio) : "",
                title: c.precio == null ? "Precio de la tarifa del cliente o del artículo (escribe uno para fijarlo)" : "",
                onChange: (p) => o(c.clave, { precio: p.target.value === "" ? null : Number(p.target.value) })
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
                value: c.dto || (c.precio == null && (x != null && x.dto) ? x.dto : 0),
                disabled: e.soloLectura,
                onChange: (p) => o(c.clave, { dto: Number(p.target.value), precio: c.precio ?? (x == null ? void 0 : x.precio) ?? null })
              }
            ) }),
            l && /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsxs("select", { "data-f": h, "data-c": 5, value: c.iva ?? (x == null ? void 0 : x.iva) ?? "", disabled: e.soloLectura, onChange: (p) => o(c.clave, { iva: p.target.value || null }), children: [
              !c.iva && !(x != null && x.iva) && /* @__PURE__ */ a.jsx("option", { value: "", children: "Del artículo" }),
              e.ivas.map((p) => /* @__PURE__ */ a.jsx("option", { value: p.codigo, children: p.nombre }, p.codigo))
            ] }) }),
            /* @__PURE__ */ a.jsx("td", { className: "num", children: /* @__PURE__ */ a.jsx("strong", { children: x ? z(x.importe) : "—" }) }),
            /* @__PURE__ */ a.jsx("td", { className: "num", children: l ? (x == null ? void 0 : x.margen) != null && /* @__PURE__ */ a.jsxs("span", { className: x.margen < 0 ? "dx-rojo" : "muted", children: [
              z(x.margen),
              M != null && /* @__PURE__ */ a.jsxs("div", { className: "dx-sub", children: [
                we(M),
                " %"
              ] })
            ] }) : (x == null ? void 0 : x.costeUnitarioEntrada) != null && /* @__PURE__ */ a.jsxs("span", { className: "muted", children: [
              z(x.costeUnitarioEntrada),
              "/ud"
            ] }) }),
            /* @__PURE__ */ a.jsxs("td", { className: "right", style: { whiteSpace: "nowrap" }, children: [
              !e.soloLectura && e.catalogo.length > 0 && /* @__PURE__ */ a.jsx("button", { type: "button", className: "dx-icono" + (n.has(c.clave) ? " activo" : ""), title: "Conceptos de la línea", onClick: () => N(c.clave), "data-f": h, "data-c": l ? 6 : 4, children: "±" }),
              !e.soloLectura && /* @__PURE__ */ a.jsx("button", { type: "button", className: "dx-icono", title: "Quitar la línea", onClick: () => s(c.clave), children: "✕" })
            ] })
          ] }, c.clave),
          n.has(c.clave) && /* @__PURE__ */ a.jsx("tr", { className: "dx-fila-conc", children: /* @__PURE__ */ a.jsx("td", { colSpan: l ? 9 : 7, children: /* @__PURE__ */ a.jsx(zo, { catalogo: e.catalogo, lista: c.conceptos, sugeridos: e.sugeridos[c.clave], alCambiar: (p) => o(c.clave, { conceptos: p }) }) }) }, c.clave + "c")
        ];
      }) })
    ] }),
    !e.soloLectura && /* @__PURE__ */ a.jsx("button", { type: "button", className: "btn small ghost", style: { marginTop: 8 }, onClick: () => (e.alCambiar([...e.lineas, wr()]), setTimeout(() => u(e.lineas.length, 0), 30)), children: "+ Añadir línea" }),
    !e.soloLectura && /* @__PURE__ */ a.jsx("span", { className: "muted dx-ayuda", children: "Intro: siguiente casilla · ↑↓: cambiar de línea · F2: buscar artículo · el precio en gris es el de tarifa" })
  ] });
}
const Lo = (e) => e.filter((t) => t.disponible > 0 && t.estado !== "Anulado").sort((t, n) => t.fecha.localeCompare(n.fecha));
function vd(e, t) {
  let n = Math.round(t * 100);
  const r = [];
  for (const l of Lo(e)) {
    if (n <= 0) break;
    const i = Math.min(n, Math.round(l.disponible * 100));
    i > 0 && r.push({ id: l.id, importe: i / 100 }), n -= i;
  }
  return r;
}
const om = { presupuesto: "presupuesto", pedido: "pedido de venta", factura: "factura" };
function xd(e) {
  const t = /* @__PURE__ */ new Map();
  for (const n of e)
    for (const r of n.conceptos ?? [])
      if (r.repartido) {
        const l = t.get(r.conceptoId);
        t.set(r.conceptoId, { conceptoId: r.conceptoId, valor: r.calculo === "Importe" ? Oe(((l == null ? void 0 : l.valor) ?? 0) + r.valor) : r.valor });
      }
  return {
    porLinea: e.map((n) => (n.conceptos ?? []).filter((r) => !r.repartido).map((r) => ({ conceptoId: r.conceptoId, valor: r.valor }))),
    documento: [...t.values()]
  };
}
function sm(e) {
  var Do, Mo, $o, Oo;
  const { api: t, anfitrion: n } = kt(), r = !!((Do = e.semilla) != null && Do.rectificaId), [l, i] = g.useState([]), [o, s] = g.useState([]), [u, d] = g.useState([]), [N, c] = g.useState([]), [h, x] = g.useState([]), [y, S] = g.useState(((Mo = e.semilla) == null ? void 0 : Mo.clienteId) ?? ""), [M, p] = g.useState(e.tipo === "pedido" && (($o = e.semilla) != null && $o.fecha) ? e.semilla.fecha : xt()), [f, v] = g.useState(""), [C, k] = g.useState(""), [P, T] = g.useState(0), [D, O] = g.useState(!1), [U, R] = g.useState(null), [Q, se] = g.useState(30), [Pe, De] = g.useState(""), [fe, xe] = g.useState([wr()]), [E, j] = g.useState([]), [w, b] = g.useState(null), [W, q] = g.useState(""), [pe, me] = g.useState(!1), [te, A] = g.useState(!1), [ut, Ke] = g.useState(!1), [ct, Ct] = g.useState([]), [Ot, wn] = g.useState(!0), ie = Gr();
  g.useEffect(() => {
    t.get("/clientes").then(i).catch(() => i([])), t.get("/tipos-iva").then((I) => s(I.filter((B) => B.activo))).catch(() => s([])), t.get("/formas-pago").then((I) => d(I.filter((B) => B.activo))).catch(() => d([])), t.get("/series").then((I) => c([...new Set(I.filter((B) => B.tipoDocumento === "Factura").map((B) => B.prefijo))])).catch(() => c([])), t.get("/conceptos-linea?ambito=Ventas&activos=true").then(x).catch(() => x([]));
  }, [t]), g.useEffect(() => {
    const I = e.semilla;
    if (!I || !I.lineas.length) return;
    const { porLinea: B, documento: ee } = xd(I.lineas), re = I.lineas.map((X, fa) => ({
      clave: Qr(),
      productoId: X.productoId ?? null,
      descripcion: X.descripcion,
      cantidad: X.cantidad,
      precio: X.precioUnitario,
      dto: X.porcentajeDescuento,
      iva: X.codigoIva,
      conceptos: r ? [] : B[fa]
    }));
    xe(re), j(r ? [] : ee), Promise.all(re.map((X) => X.productoId ? t.get(`/productos/${X.productoId}`).catch(() => null) : Promise.resolve(null))).then(
      (X) => xe((fa) => fa.map((Ao, kn) => X[kn] ? { ...Ao, referencia: X[kn].referencia ?? X[kn].nombre, unidad: X[kn].unidad, stock: X[kn].stock, controlarStock: X[kn].controlarStock } : Ao))
    );
  }, [e.semilla, t, r]);
  const K = l.find((I) => I.id === y);
  g.useEffect(() => {
    if (e.tipo !== "factura" || r || !y) {
      Ct([]);
      return;
    }
    t.get(`/anticipos?clienteId=${y}`).then((I) => Ct(Lo(I))).catch(() => Ct([]));
  }, [t, y, e.tipo, r]);
  const qe = Oe(ct.reduce((I, B) => I + B.disponible, 0));
  g.useEffect(() => {
    K && (O(!!K.recargoEquivalencia), K.formaPagoDefectoId && k(K.formaPagoDefectoId));
  }, [K]);
  const Fe = g.useMemo(() => fe.map((I, B) => ({ l: I, i: B })).filter(({ l: I }) => (I.productoId || I.descripcion.trim()) && I.cantidad > 0), [fe]), Et = g.useMemo(
    () => ({
      clienteId: y,
      fechaEmision: e.tipo === "factura" ? M : null,
      serie: f || null,
      diasVencimiento: P,
      formaPagoId: C || null,
      recargoEquivalencia: D,
      porcentajeIrpf: U,
      conceptosDocumento: E,
      lineas: Fe.map(({ l: I }) => ({
        cantidad: I.cantidad,
        descripcion: I.descripcion.trim() || null,
        precioUnitario: I.precio,
        codigoIva: I.iva,
        porcentajeDescuento: I.dto,
        productoId: I.productoId,
        ...r ? { conceptos: [] } : I.conceptos === void 0 ? {} : { conceptos: I.conceptos }
      }))
    }),
    [y, M, f, P, C, D, U, E, Fe, e.tipo, r]
  ), It = hn(Et, 350);
  g.useEffect(() => {
    if (!It.clienteId || It.lineas.length === 0) {
      b(null), q(It.clienteId ? "" : "Elige el cliente para calcular precios e importes.");
      return;
    }
    const I = ie();
    me(!0), t.post("/facturas/simular", It).then((B) => I() && (b(B), q(""))).catch((B) => I() && (b(null), q(B.message))).finally(() => I() && me(!1));
  }, [It, t]);
  const Ve = g.useMemo(() => {
    const I = fe.map(() => {
    });
    return w && Fe.forEach(({ i: B }, ee) => {
      const re = w.lineas[ee];
      re && (I[B] = { precio: re.precioUnitario, dto: re.porcentajeDescuento, iva: re.codigoIva, importe: re.base, margen: re.productoId || re.costeUnitario || re.costeConceptos ? re.margen : void 0, conceptos: re.conceptos });
    }), I;
  }, [w, fe, Fe]), tr = g.useMemo(() => {
    const I = {};
    return fe.forEach((B, ee) => {
      var re;
      return I[B.clave] = (((re = Ve[ee]) == null ? void 0 : re.conceptos) ?? []).filter((X) => !X.repartido).map((X) => ({ conceptoId: X.conceptoId, valor: X.valor }));
    }), I;
  }, [fe, Ve]);
  function on(I, B) {
    xe(
      (ee) => ee.map(
        (re) => re.clave === I ? { ...re, productoId: B.id, referencia: B.referencia ?? B.nombre, descripcion: B.nombre, precio: null, dto: 0, iva: null, conceptos: void 0, unidad: B.unidad, stock: B.stock, controlarStock: B.controlarStock } : re
      )
    );
  }
  const m = g.useMemo(() => {
    const I = /* @__PURE__ */ new Map();
    for (const B of (w == null ? void 0 : w.lineas) ?? []) {
      const ee = I.get(B.codigoIva) ?? { base: 0, cuota: 0, pct: B.porcentajeIva };
      ee.base += B.base, ee.cuota += B.cuotaIva, I.set(B.codigoIva, ee);
    }
    return [...I.entries()];
  }, [w]), L = ((w == null ? void 0 : w.lineas) ?? []).reduce((I, B) => I + (B.base - B.margen), 0), F = w ? w.baseImponible - L : 0, V = (I) => {
    var B;
    return ((B = o.find((ee) => ee.codigo === I)) == null ? void 0 : B.nombre) ?? I;
  };
  async function H() {
    if (w) {
      A(!0);
      try {
        const I = Fe.map(({ l: ee }, re) => {
          const X = w.lineas[re];
          return {
            cantidad: ee.cantidad,
            descripcion: X.descripcion,
            precioUnitario: X.precioUnitario,
            codigoIva: X.codigoIva,
            porcentajeDescuento: X.porcentajeDescuento,
            productoId: ee.productoId,
            ...r ? {} : ee.conceptos === void 0 ? {} : { conceptos: ee.conceptos }
          };
        });
        let B;
        if (r)
          B = (await t.post(`/facturas/${e.semilla.rectificaId}/rectificar`, { motivo: Pe, lineas: I, fechaEmision: M, porcentajeIrpf: U, serie: f || null })).id;
        else if (e.tipo === "factura") {
          const ee = await t.post("/facturas", { ...Et, lineas: I });
          if (B = ee.id, Ot && ct.length) {
            let re = 0;
            try {
              for (const X of vd(ct, ee.total))
                await t.post(`/anticipos/${X.id}/aplicar`, { facturaId: B, importe: X.importe }), re += X.importe;
              n.aviso(`Factura emitida. Aplicados ${z(re)} de anticipos (asiento 438 a 430).`, "ok");
            } catch (X) {
              n.aviso(`Factura emitida, pero no se pudo aplicar el anticipo: ${X.message}`, "err");
            }
            e.alGuardar(B);
            return;
          }
        } else if (e.tipo === "presupuesto") {
          const ee = { clienteId: y, diasValidez: Q, lineas: I, conceptosDocumento: E };
          B = e.id ? (await t.put(`/presupuestos/${e.id}`, ee)).id : (await t.post("/presupuestos", ee)).id;
        } else {
          const ee = { clienteId: y, fecha: M, lineas: I, conceptosDocumento: E };
          B = e.id ? (await t.put(`/pedidos-venta/${e.id}`, ee)).id : (await t.post("/pedidos-venta", ee)).id;
        }
        n.aviso(e.tipo === "factura" ? "Factura emitida." : "Documento guardado.", "ok"), e.alGuardar(B);
      } catch (I) {
        n.aviso(I.message, "err");
      } finally {
        A(!1), Ke(!1);
      }
    }
  }
  const J = r ? `Rectificativa de la factura ${((Oo = e.semilla) == null ? void 0 : Oo.rectificaNumero) ?? ""}` : `${e.id ? "Editar" : "Nuevo"} ${om[e.tipo]}`.replace("Nuevo factura", "Nueva factura"), gd = !!w && !pe && (!r || Pe.trim().length > 0);
  return /* @__PURE__ */ a.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsxs("div", { className: "panel-head", children: [
        /* @__PURE__ */ a.jsx("h2", { children: J }),
        /* @__PURE__ */ a.jsxs("div", { style: { display: "flex", gap: 8 }, children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: e.alCancelar, children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", disabled: !gd || te, onClick: () => e.tipo === "factura" ? Ke(!0) : H(), children: e.tipo === "factura" ? "Emitir factura" : "Guardar" })
        ] })
      ] }),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-cabecera", children: [
        /* @__PURE__ */ a.jsxs("div", { className: "dx-cab-campos", children: [
          /* @__PURE__ */ a.jsx(To, { terceros: l, valor: y, alCambiar: S, etiqueta: "Cliente", deshabilitado: r || e.tipo === "pedido" && !!e.id && !1 }),
          /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", children: [
            e.tipo !== "presupuesto" && /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: e.tipo === "factura" ? "Fecha de emisión" : "Fecha del pedido" }),
              /* @__PURE__ */ a.jsx("input", { type: "date", value: M, onChange: (I) => p(I.target.value) })
            ] }),
            e.tipo === "presupuesto" && /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Validez" }),
              /* @__PURE__ */ a.jsx("select", { value: Q, onChange: (I) => se(Number(I.target.value)), children: [15, 30, 60, 90].map((I) => /* @__PURE__ */ a.jsxs("option", { value: I, children: [
                I,
                " días"
              ] }, I)) })
            ] }),
            e.tipo === "factura" && N.length > 0 && /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Serie" }),
              /* @__PURE__ */ a.jsxs("select", { value: f, onChange: (I) => v(I.target.value), children: [
                /* @__PURE__ */ a.jsx("option", { value: "", children: "La del cliente" }),
                N.map((I) => /* @__PURE__ */ a.jsx("option", { value: I, children: I }, I))
              ] })
            ] }),
            e.tipo === "factura" && !r && /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
              /* @__PURE__ */ a.jsxs("div", { children: [
                /* @__PURE__ */ a.jsx("label", { children: "Forma de pago" }),
                /* @__PURE__ */ a.jsxs("select", { value: C, onChange: (I) => k(I.target.value), children: [
                  /* @__PURE__ */ a.jsx("option", { value: "", children: "— Vencimiento a mano —" }),
                  u.map((I) => /* @__PURE__ */ a.jsx("option", { value: I.id, children: I.nombre }, I.id))
                ] })
              ] }),
              !C && /* @__PURE__ */ a.jsxs("div", { children: [
                /* @__PURE__ */ a.jsx("label", { children: "Vencimiento" }),
                /* @__PURE__ */ a.jsx("select", { value: P, onChange: (I) => T(Number(I.target.value)), children: [0, 15, 30, 45, 60, 90].map((I) => /* @__PURE__ */ a.jsx("option", { value: I, children: I ? `${I} días` : "Contado" }, I)) })
              ] })
            ] }),
            e.tipo === "factura" && /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Retención IRPF %" }),
              /* @__PURE__ */ a.jsx("input", { type: "number", step: "0.01", value: U ?? "", placeholder: String((K == null ? void 0 : K.porcentajeIrpfDefecto) ?? 0), onChange: (I) => R(I.target.value === "" ? null : Number(I.target.value)) })
            ] })
          ] }),
          e.tipo === "factura" && !r && /* @__PURE__ */ a.jsxs("label", { className: "dx-check", children: [
            /* @__PURE__ */ a.jsx("input", { type: "checkbox", checked: D, onChange: (I) => O(I.target.checked) }),
            " Recargo de equivalencia"
          ] }),
          r && /* @__PURE__ */ a.jsxs("div", { children: [
            /* @__PURE__ */ a.jsx("label", { children: "Motivo de la rectificación (obligatorio)" }),
            /* @__PURE__ */ a.jsx("input", { value: Pe, onChange: (I) => De(I.target.value), placeholder: "Devolución, error en precio…" })
          ] })
        ] }),
        /* @__PURE__ */ a.jsx("div", { className: "dx-ficha", children: K ? /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("strong", { children: K.nombre }),
          /* @__PURE__ */ a.jsx("div", { className: "muted", children: [K.nifFiscal, K.poblacion, K.provincia].filter(Boolean).join(" · ") }),
          K.limiteRiesgo != null && /* @__PURE__ */ a.jsxs("div", { className: "muted", children: [
            "Límite de riesgo ",
            z(K.limiteRiesgo)
          ] }),
          K.tarifaId && /* @__PURE__ */ a.jsx("div", { className: "muted", children: "Con tarifa de precios propia" }),
          K.recargoEquivalencia && /* @__PURE__ */ a.jsx("div", { className: "muted", children: "En recargo de equivalencia" }),
          (w == null ? void 0 : w.avisoRiesgo) && /* @__PURE__ */ a.jsxs("div", { className: "dx-aviso", children: [
            "⚠ ",
            w.avisoRiesgo
          ] }),
          qe > 0 && /* @__PURE__ */ a.jsxs("div", { className: "dx-anticipo", children: [
            /* @__PURE__ */ a.jsxs("div", { children: [
              "💶 Tiene ",
              /* @__PURE__ */ a.jsx("strong", { children: z(qe) }),
              " en ",
              ct.length === 1 ? "un anticipo pendiente" : `${ct.length} anticipos pendientes`,
              " de aplicar."
            ] }),
            /* @__PURE__ */ a.jsxs("label", { className: "dx-check", children: [
              /* @__PURE__ */ a.jsx("input", { type: "checkbox", checked: Ot, onChange: (I) => wn(I.target.checked) }),
              "Aplicarlo al emitir",
              w ? ` (${z(Math.min(qe, w.total))})` : ""
            ] })
          ] })
        ] }) : /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Elige un cliente: se aplican su tarifa, su forma de pago, su recargo y sus conceptos." }) })
      ] })
    ] }),
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsx(hd, { modo: "venta", lineas: fe, alCambiar: xe, calculos: Ve, ivas: o, catalogo: r ? [] : h, sugeridos: tr, alElegirArticulo: on }),
      !r && h.length > 0 && /* @__PURE__ */ a.jsx("div", { style: { marginTop: 10 }, children: /* @__PURE__ */ a.jsx(zo, { catalogo: h, lista: E, alCambiar: (I) => j(I ?? []), documento: !0 }) })
    ] }),
    /* @__PURE__ */ a.jsxs("div", { className: "dx-pie", children: [
      /* @__PURE__ */ a.jsxs("div", { className: "dx-estado", children: [
        pe && /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Calculando…" }),
        !pe && W && /* @__PURE__ */ a.jsx("span", { className: "dx-rojo", children: W }),
        (w == null ? void 0 : w.mencionFiscal) && /* @__PURE__ */ a.jsx("div", { className: "muted", style: { fontSize: 12 }, children: w.mencionFiscal })
      ] }),
      /* @__PURE__ */ a.jsxs("div", { className: "panel dx-totales", children: [
        m.map(([I, B]) => /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsxs("span", { className: "muted", children: [
            V(I),
            " · base ",
            we(B.base)
          ] }),
          /* @__PURE__ */ a.jsx("span", { children: z(B.cuota) })
        ] }, I)),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Base imponible" }),
          /* @__PURE__ */ a.jsx("span", { children: z(w == null ? void 0 : w.baseImponible) })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Impuestos" }),
          /* @__PURE__ */ a.jsx("span", { children: z(w == null ? void 0 : w.cuotaIva) })
        ] }),
        !!(w != null && w.recargoTotal) && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Recargo de equivalencia" }),
          /* @__PURE__ */ a.jsx("span", { children: z(w.recargoTotal) })
        ] }),
        !!(w != null && w.retencionIrpf) && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsxs("span", { className: "muted", children: [
            "Retención IRPF (",
            we(w.porcentajeIrpf),
            " %)"
          ] }),
          /* @__PURE__ */ a.jsxs("span", { children: [
            "−",
            z(w.retencionIrpf)
          ] })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ a.jsx("span", { children: "Total" }),
          /* @__PURE__ */ a.jsx("span", { children: z(w == null ? void 0 : w.total) })
        ] }),
        w && L > 0 && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Coste · margen" }),
          /* @__PURE__ */ a.jsxs("span", { className: F < 0 ? "dx-rojo" : "muted", children: [
            z(L),
            " · ",
            z(F),
            " (",
            we(w.baseImponible ? F / w.baseImponible * 100 : 0),
            " %)"
          ] })
        ] })
      ] })
    ] }),
    ut && w && /* @__PURE__ */ a.jsx(
      nn,
      {
        titulo: r ? "Emitir la rectificativa" : "Emitir la factura",
        alCerrar: () => Ke(!1),
        acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => Ke(!1), children: "Revisar" }),
          /* @__PURE__ */ a.jsxs("button", { className: "btn small", disabled: te, onClick: H, children: [
            "Emitir ",
            z(w.total)
          ] })
        ] }),
        children: /* @__PURE__ */ a.jsxs("p", { style: { margin: 0 }, children: [
          "Se emitirá una factura ",
          r ? "rectificativa " : "",
          "de ",
          /* @__PURE__ */ a.jsx("strong", { children: z(w.total) }),
          " a ",
          /* @__PURE__ */ a.jsx("strong", { children: K == null ? void 0 : K.nombre }),
          " con fecha ",
          M.split("-").reverse().join("/"),
          ". Una factura emitida no se modifica: queda numerada y encadenada en VeriFactu; para corregirla hay que rectificarla o anularla."
        ] })
      }
    )
  ] });
}
function um(e) {
  var De, fe, xe;
  const { api: t, anfitrion: n } = kt(), [r, l] = g.useState([]), [i, o] = g.useState([]), [s, u] = g.useState(((De = e.semilla) == null ? void 0 : De.proveedorId) ?? ""), [d, N] = g.useState(((fe = e.semilla) == null ? void 0 : fe.fecha) ?? xt()), [c, h] = g.useState([wr()]), [x, y] = g.useState([]), [S, M] = g.useState(null), [p, f] = g.useState(""), [v, C] = g.useState(!1), k = Gr();
  g.useEffect(() => {
    t.get("/proveedores").then(l).catch(() => l([])), t.get("/conceptos-linea?ambito=Compras&activos=true").then(o).catch(() => o([]));
  }, [t]), g.useEffect(() => {
    const E = e.semilla;
    if (!E) return;
    const j = E.lineas.map((q) => ({ ...q, porcentajeDescuento: 0, codigoIva: "" })), { porLinea: w, documento: b } = xd(j), W = E.lineas.map((q, pe) => ({ clave: Qr(), productoId: q.productoId ?? null, descripcion: q.descripcion, cantidad: q.cantidad, precio: q.precioUnitario, dto: 0, iva: null, conceptos: w[pe] }));
    h(W), y(b), Promise.all(W.map((q) => q.productoId ? t.get(`/productos/${q.productoId}`).catch(() => null) : Promise.resolve(null))).then(
      (q) => h((pe) => pe.map((me, te) => q[te] ? { ...me, referencia: q[te].referencia ?? q[te].nombre, unidad: q[te].unidadCompra || q[te].unidad, stock: q[te].stock, controlarStock: q[te].controlarStock } : me))
    );
  }, [e.semilla, t]);
  const P = r.find((E) => E.id === s), T = g.useMemo(() => c.map((E, j) => ({ l: E, i: j })).filter(({ l: E }) => E.descripcion.trim() && E.cantidad > 0), [c]), D = g.useMemo(
    () => {
      var E, j;
      return {
        proveedorId: s || null,
        proveedorTexto: (P == null ? void 0 : P.nombre) ?? (((E = e.semilla) == null ? void 0 : E.proveedorTexto) || null),
        solicitudOrigenId: e.id ? null : ((j = e.semilla) == null ? void 0 : j.solicitudOrigenId) ?? null,
        fecha: d,
        conceptosDocumento: x,
        lineas: T.map(({ l: w }) => ({ descripcion: w.descripcion.trim(), cantidad: w.cantidad, precioUnitario: w.precio ?? 0, productoId: w.productoId, ...w.conceptos === void 0 ? {} : { conceptos: w.conceptos } }))
      };
    },
    [s, P, d, x, T, e.id, e.semilla]
  ), O = hn(D, 350);
  g.useEffect(() => {
    if (!O.proveedorId || O.lineas.length === 0) {
      M(null), f(O.proveedorId ? "" : "Elige el proveedor.");
      return;
    }
    const E = k();
    t.post("/compras/pedidos/simular", O).then((j) => E() && (M(j), f(""))).catch((j) => E() && (M(null), f(j.message)));
  }, [O, t]);
  const U = g.useMemo(() => {
    const E = c.map(() => {
    });
    return T.forEach(({ i: j }, w) => {
      const b = S == null ? void 0 : S.lineas[w];
      b && (E[j] = { precio: b.precioUnitario, importe: b.importe, costeUnitarioEntrada: b.costeUnitarioEntrada, conceptos: b.conceptos });
    }), E;
  }, [S, c, T]), R = g.useMemo(() => {
    const E = {};
    return c.forEach((j, w) => {
      var b;
      return E[j.clave] = (((b = U[w]) == null ? void 0 : b.conceptos) ?? []).filter((W) => !W.repartido).map((W) => ({ conceptoId: W.conceptoId, valor: W.valor }));
    }), E;
  }, [c, U]);
  function Q(E, j) {
    const w = j.precioCompraPorUnidadCompra ?? j.precioCompra;
    h((b) => b.map((W) => W.clave === E ? { ...W, productoId: j.id, referencia: j.referencia ?? j.nombre, descripcion: j.nombre, precio: w, conceptos: void 0, unidad: j.unidadCompra || j.unidad, stock: j.stock, controlarStock: j.controlarStock } : W));
  }
  async function se() {
    C(!0);
    try {
      const E = e.id ? await t.put(`/compras/pedidos/${e.id}`, D) : await t.post("/compras/pedidos", D);
      n.aviso("Pedido de compra guardado.", "ok"), e.alGuardar(E.id);
    } catch (E) {
      n.aviso(E.message, "err");
    } finally {
      C(!1);
    }
  }
  const Pe = ((S == null ? void 0 : S.lineas) ?? []).reduce((E, j) => E + j.costeConceptos, 0);
  return /* @__PURE__ */ a.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsxs("div", { className: "panel-head", children: [
        /* @__PURE__ */ a.jsx("h2", { children: e.id ? `Editar pedido ${((xe = e.semilla) == null ? void 0 : xe.numeroCompleto) ?? ""}` : "Nuevo pedido de compra" }),
        /* @__PURE__ */ a.jsxs("div", { style: { display: "flex", gap: 8 }, children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: e.alCancelar, children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", disabled: !S || v, onClick: se, children: "Guardar" })
        ] })
      ] }),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-cabecera", children: [
        /* @__PURE__ */ a.jsxs("div", { className: "dx-cab-campos", children: [
          /* @__PURE__ */ a.jsx(To, { terceros: r, valor: s, alCambiar: u, etiqueta: "Proveedor", deshabilitado: !!e.id }),
          /* @__PURE__ */ a.jsx("div", { className: "dx-fila", children: /* @__PURE__ */ a.jsxs("div", { children: [
            /* @__PURE__ */ a.jsx("label", { children: "Fecha del pedido" }),
            /* @__PURE__ */ a.jsx("input", { type: "date", value: d, onChange: (E) => N(E.target.value) })
          ] }) })
        ] }),
        /* @__PURE__ */ a.jsx("div", { className: "dx-ficha", children: P ? /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("strong", { children: P.nombre }),
          /* @__PURE__ */ a.jsx("div", { className: "muted", children: [P.nifFiscal, P.poblacion, P.pais].filter(Boolean).join(" · ") })
        ] }) : /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Elige un proveedor: se aplican sus conceptos (pronto pago, portes…)." }) })
      ] })
    ] }),
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsx(hd, { modo: "compra", lineas: c, alCambiar: h, calculos: U, ivas: [], catalogo: i, sugeridos: R, alElegirArticulo: Q }),
      i.length > 0 && /* @__PURE__ */ a.jsx("div", { style: { marginTop: 10 }, children: /* @__PURE__ */ a.jsx(zo, { catalogo: i, lista: x, alCambiar: (E) => y(E ?? []), documento: !0 }) })
    ] }),
    /* @__PURE__ */ a.jsxs("div", { className: "dx-pie", children: [
      /* @__PURE__ */ a.jsx("div", { className: "dx-estado", children: p && /* @__PURE__ */ a.jsx("span", { className: "dx-rojo", children: p }) }),
      /* @__PURE__ */ a.jsxs("div", { className: "panel dx-totales", children: [
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ a.jsx("span", { children: "Total del pedido (sin impuestos)" }),
          /* @__PURE__ */ a.jsx("span", { children: z(S == null ? void 0 : S.total) })
        ] }),
        Pe !== 0 && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Costes añadidos al almacén" }),
          /* @__PURE__ */ a.jsx("span", { children: z(Pe) })
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
function Hn(e) {
  return /* @__PURE__ */ a.jsx("button", { type: "button", className: "dx-enlace", onClick: e.alPulsar, children: e.children });
}
function da(e) {
  const [t, n] = g.useState(null), [r, l] = g.useState(""), i = g.useCallback(() => {
    e().then(n).catch((o) => l(o.message));
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
function cm(e) {
  const { api: t, anfitrion: n, navegar: r } = kt(), { dato: l, error: i, recargar: o } = da(() => t.get(`/facturas/${e.id}`)), [s, u] = g.useState(null), [d, N] = g.useState(!1), [c, h] = g.useState("");
  g.useEffect(() => void t.get(`/facturas/${e.id}/saldo`).then(u).catch(() => u(null)), [t, e.id, l]);
  const [x, y] = g.useState([]), [S, M] = g.useState(null);
  g.useEffect(() => {
    !(l != null && l.clienteId) || l.estado !== "Emitida" || t.get(`/anticipos?clienteId=${l.clienteId}`).then((k) => y(Lo(k))).catch(() => y([]));
  }, [t, l]);
  const p = x.reduce((k, P) => k + P.disponible, 0);
  if (i) return /* @__PURE__ */ a.jsx("div", { className: "panel", children: /* @__PURE__ */ a.jsx("p", { className: "dx-rojo", children: i }) });
  if (!l) return /* @__PURE__ */ a.jsx("div", { className: "muted", children: "Cargando…" });
  const f = { clienteId: l.clienteId ?? void 0, lineas: l.lineas }, v = l.lineas.reduce((k, P) => k + (P.base - P.margen), 0), C = l.estado === "Emitida";
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
            /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => Li(n, `/facturas/${l.id}/pdf`).catch((k) => n.aviso(k.message, "err")), children: "PDF" }),
            l.tipo !== "Simplificada" && l.clienteNif && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => Li(n, `/facturas/${l.id}/facturae.xml`).catch((k) => n.aviso(k.message, "err")), children: "Facturae" }),
            /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "factura", pantalla: "editor", semilla: f }), children: "Duplicar" }),
            C && l.tipo === "Ordinaria" && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "factura", pantalla: "editor", semilla: { ...f, rectificaId: l.id, rectificaNumero: l.numeroCompleto } }), children: "Rectificar" }),
            C && /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", onClick: () => N(!0), children: "Anular" })
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
          je(l.fechaEmision),
          l.fechaOperacion !== l.fechaEmision && /* @__PURE__ */ a.jsxs("div", { className: "muted", children: [
            "Operación ",
            je(l.fechaOperacion)
          ] })
        ] }),
        /* @__PURE__ */ a.jsx(ze, { etiqueta: "Vencimiento", children: je(l.fechaVencimiento) }),
        /* @__PURE__ */ a.jsxs(ze, { etiqueta: "Cobro", children: [
          s ? s.pendiente <= 0 ? /* @__PURE__ */ a.jsx("span", { className: "pill ok", children: "Cobrada" }) : /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            "Pendiente ",
            /* @__PURE__ */ a.jsx("strong", { children: z(s.pendiente) }),
            s.liquidado > 0 && /* @__PURE__ */ a.jsxs("div", { className: "muted", children: [
              "Cobrado ",
              z(s.liquidado)
            ] })
          ] }) : "—",
          s && s.pendiente > 0 && C && n.irA && /* @__PURE__ */ a.jsx("div", { children: /* @__PURE__ */ a.jsx(Hn, { alPulsar: () => n.irA("cobros"), children: "Registrar cobro" }) }),
          s && s.pendiente > 0 && C && p > 0 && /* @__PURE__ */ a.jsxs("div", { className: "dx-anticipo", children: [
            "💶 Anticipos del cliente sin aplicar: ",
            /* @__PURE__ */ a.jsx("strong", { children: z(p) }),
            /* @__PURE__ */ a.jsx("div", { children: /* @__PURE__ */ a.jsx(Hn, { alPulsar: () => M(vd(x, s.pendiente)), children: "Aplicar a esta factura" }) })
          ] })
        ] })
      ] }),
      l.motivoRectificacion && /* @__PURE__ */ a.jsxs("p", { className: "muted", children: [
        "Rectifica: ",
        l.motivoRectificacion,
        l.rectificaFacturaId && /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          " · ",
          /* @__PURE__ */ a.jsx(Hn, { alPulsar: () => r({ tipo: "factura", pantalla: "vista", id: l.rectificaFacturaId }), children: "ver la factura original" })
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
        /* @__PURE__ */ a.jsx("tbody", { children: l.lineas.map((k, P) => /* @__PURE__ */ a.jsxs("tr", { children: [
          /* @__PURE__ */ a.jsxs("td", { children: [
            k.descripcion,
            /* @__PURE__ */ a.jsx(ua, { conceptos: k.conceptos })
          ] }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: $e(k.cantidad) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: z(k.precioUnitario) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: k.porcentajeDescuento ? `${we(k.porcentajeDescuento)} %` : "" }),
          /* @__PURE__ */ a.jsxs("td", { children: [
            k.codigoIva,
            " · ",
            we(k.porcentajeIva),
            " %"
          ] }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: /* @__PURE__ */ a.jsx("strong", { children: z(k.base) }) }),
          /* @__PURE__ */ a.jsx("td", { className: "num muted", children: k.costeUnitario || k.costeConceptos ? z(k.margen) : "" })
        ] }, P)) })
      ] }),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-totales-vista", children: [
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Base imponible" }),
          /* @__PURE__ */ a.jsx("span", { children: z(l.baseImponible) })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: l.impuesto === "Igic" ? "IGIC" : "IVA" }),
          /* @__PURE__ */ a.jsx("span", { children: z(l.cuotaIva) })
        ] }),
        !!l.recargoTotal && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Recargo de equivalencia" }),
          /* @__PURE__ */ a.jsx("span", { children: z(l.recargoTotal) })
        ] }),
        !!l.retencionIrpf && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsxs("span", { className: "muted", children: [
            "Retención IRPF (",
            we(l.porcentajeIrpf),
            " %)"
          ] }),
          /* @__PURE__ */ a.jsxs("span", { children: [
            "−",
            z(l.retencionIrpf)
          ] })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ a.jsx("span", { children: "Total" }),
          /* @__PURE__ */ a.jsx("span", { children: z(l.total) })
        ] }),
        v > 0 && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Margen" }),
          /* @__PURE__ */ a.jsxs("span", { className: "muted", children: [
            z(l.baseImponible - v),
            " (",
            we(l.baseImponible ? (l.baseImponible - v) / l.baseImponible * 100 : 0),
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
    S && /* @__PURE__ */ a.jsxs(
      nn,
      {
        titulo: `Aplicar anticipos a ${l.numeroCompleto}`,
        alCerrar: () => M(null),
        acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => M(null), children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", disabled: !S.some((k) => k.importe > 0), onClick: async () => {
            for (const k of S.filter((P) => P.importe > 0))
              if (!await lt(() => t.post(`/anticipos/${k.id}/aplicar`, { facturaId: l.id, importe: k.importe }), n.aviso, "Anticipo aplicado.")) return;
            M(null), o();
          }, children: "Aplicar" })
        ] }),
        children: [
          /* @__PURE__ */ a.jsxs("p", { className: "muted", style: { marginTop: 0 }, children: [
            "Se registra el cobro de la factura con el anticipo y su asiento de cancelación: 438 Anticipos de clientes al debe, 430 Clientes al haber. Pendiente de la factura: ",
            /* @__PURE__ */ a.jsx("strong", { children: z(s == null ? void 0 : s.pendiente) }),
            "."
          ] }),
          /* @__PURE__ */ a.jsxs("table", { children: [
            /* @__PURE__ */ a.jsx("thead", { children: /* @__PURE__ */ a.jsxs("tr", { children: [
              /* @__PURE__ */ a.jsx("th", { children: "Anticipo" }),
              /* @__PURE__ */ a.jsx("th", { children: "Concepto" }),
              /* @__PURE__ */ a.jsx("th", { className: "num", children: "Disponible" }),
              /* @__PURE__ */ a.jsx("th", { className: "num", children: "Aplicar" })
            ] }) }),
            /* @__PURE__ */ a.jsx("tbody", { children: x.map((k) => {
              const P = S.find((T) => T.id === k.id);
              return /* @__PURE__ */ a.jsxs("tr", { children: [
                /* @__PURE__ */ a.jsx("td", { children: je(k.fecha) }),
                /* @__PURE__ */ a.jsx("td", { className: "muted", children: k.concepto }),
                /* @__PURE__ */ a.jsx("td", { className: "num", children: z(k.disponible) }),
                /* @__PURE__ */ a.jsx("td", { className: "num", children: /* @__PURE__ */ a.jsx(
                  "input",
                  {
                    type: "number",
                    step: "0.01",
                    min: 0,
                    max: k.disponible,
                    style: { width: 110, textAlign: "right" },
                    value: (P == null ? void 0 : P.importe) ?? 0,
                    onChange: (T) => {
                      const D = Math.max(0, Math.min(k.disponible, Number(T.target.value) || 0));
                      M([...S.filter((O) => O.id !== k.id), { id: k.id, importe: D }]);
                    }
                  }
                ) })
              ] }, k.id);
            }) })
          ] })
        ]
      }
    ),
    d && /* @__PURE__ */ a.jsxs(
      nn,
      {
        titulo: `Anular ${l.numeroCompleto}`,
        alCerrar: () => N(!1),
        acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => N(!1), children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", disabled: !c.trim(), onClick: async () => await lt(() => t.post(`/facturas/${l.id}/anular`, { motivo: c }), n.aviso, "Factura anulada.") && (N(!1), o()), children: "Anular" })
        ] }),
        children: [
          /* @__PURE__ */ a.jsx("p", { className: "muted", style: { marginTop: 0 }, children: "La factura no se borra: queda anulada con un registro de anulación VeriFactu. Si hay que corregir importes, rectifícala en lugar de anularla." }),
          /* @__PURE__ */ a.jsx("label", { children: "Motivo" }),
          /* @__PURE__ */ a.jsx("input", { value: c, onChange: (k) => h(k.target.value), autoFocus: !0 })
        ]
      }
    )
  ] });
}
function dm(e) {
  const { api: t, anfitrion: n, navegar: r } = kt(), { dato: l, error: i, recargar: o } = da(() => t.get(`/presupuestos/${e.id}`));
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
            s && /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", onClick: async () => await lt(() => t.post(`/presupuestos/${l.id}/rechazar`, {}), n.aviso, "Presupuesto rechazado.") && o(), children: "Rechazar" })
          ] })
        }
      ),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ a.jsx(ze, { etiqueta: "Cliente", children: /* @__PURE__ */ a.jsx("strong", { children: l.clienteNombre }) }),
        /* @__PURE__ */ a.jsx(ze, { etiqueta: "Fecha", children: je(l.fecha) }),
        /* @__PURE__ */ a.jsx(ze, { etiqueta: "Válido hasta", children: je(l.validez) }),
        /* @__PURE__ */ a.jsx(ze, { etiqueta: "Factura", children: l.facturaId ? /* @__PURE__ */ a.jsx(Hn, { alPulsar: () => r({ tipo: "factura", pantalla: "vista", id: l.facturaId }), children: "ver la factura" }) : "—" })
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
          /* @__PURE__ */ a.jsx("td", { className: "num", children: z(d.precioUnitario) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: d.porcentajeDescuento ? `${we(d.porcentajeDescuento)} %` : "" }),
          /* @__PURE__ */ a.jsx("td", { children: d.codigoIva }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: /* @__PURE__ */ a.jsx("strong", { children: z(d.base) }) })
        ] }, N)) })
      ] }),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-totales-vista", children: [
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Base imponible" }),
          /* @__PURE__ */ a.jsx("span", { children: z(l.baseImponible) })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Impuestos" }),
          /* @__PURE__ */ a.jsx("span", { children: z(l.cuotaIva) })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ a.jsx("span", { children: "Total" }),
          /* @__PURE__ */ a.jsx("span", { children: z(l.total) })
        ] })
      ] })
    ] })
  ] });
}
function fm(e) {
  const { api: t, anfitrion: n, navegar: r } = kt(), { dato: l, error: i, recargar: o } = da(() => t.get(`/pedidos-venta/${e.id}`)), [s, u] = g.useState([]), [d, N] = g.useState([]), [c, h] = g.useState(null), [x, y] = g.useState(""), [S, M] = g.useState(xt()), [p, f] = g.useState(!1), [v, C] = g.useState(xt()), [k, P] = g.useState("");
  if (g.useEffect(() => void t.get(`/pedidos-venta/${e.id}/albaranes`).then(u).catch(() => u([])), [t, e.id, l]), g.useEffect(() => void t.get("/formas-pago").then((R) => N(R.filter((Q) => Q.activo))).catch(() => N([])), [t]), i) return /* @__PURE__ */ a.jsx("div", { className: "panel", children: /* @__PURE__ */ a.jsx("p", { className: "dx-rojo", children: i }) });
  if (!l) return /* @__PURE__ */ a.jsx("div", { className: "muted", children: "Cargando…" });
  const T = (l.estado === "Borrador" || l.estado === "Confirmado") && l.lineas.every((R) => R.cantidadServida === 0), D = l.lineas.some((R) => R.pendienteServir > 0), O = l.estado !== "Cancelado" && l.estado !== "Facturado", U = { clienteId: l.clienteId, fecha: l.fecha, lineas: l.lineas };
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
            T && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "pedido", pantalla: "editor", id: l.id, semilla: U }), children: "Editar" }),
            l.estado === "Borrador" && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: async () => await lt(() => t.post(`/pedidos-venta/${l.id}/confirmar`), n.aviso, "Pedido confirmado.") && o(), children: "Confirmar" }),
            O && l.estado !== "Borrador" && D && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => h(Object.fromEntries(l.lineas.map((R) => [R.id, R.pendienteServir]))), children: "Entregar (albarán)" }),
            O && l.estado !== "Borrador" && /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: () => f(!0), children: "Facturar" }),
            O && /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", onClick: async () => window.confirm("¿Cancelar el pedido?") && await lt(() => t.post(`/pedidos-venta/${l.id}/cancelar`), n.aviso, "Pedido cancelado.") && o(), children: "Cancelar" })
          ] })
        }
      ),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ a.jsx(ze, { etiqueta: "Cliente", children: /* @__PURE__ */ a.jsx("strong", { children: l.clienteNombre }) }),
        /* @__PURE__ */ a.jsx(ze, { etiqueta: "Fecha", children: je(l.fecha) }),
        /* @__PURE__ */ a.jsx(ze, { etiqueta: "Viene de", children: l.presupuestoOrigenId ? /* @__PURE__ */ a.jsx(Hn, { alPulsar: () => r({ tipo: "presupuesto", pantalla: "vista", id: l.presupuestoOrigenId }), children: "presupuesto" }) : "—" }),
        /* @__PURE__ */ a.jsx(ze, { etiqueta: "Factura", children: l.facturaId ? /* @__PURE__ */ a.jsx(Hn, { alPulsar: () => r({ tipo: "factura", pantalla: "vista", id: l.facturaId }), children: "ver la factura" }) : "—" })
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
        /* @__PURE__ */ a.jsx("tbody", { children: l.lineas.map((R) => /* @__PURE__ */ a.jsxs("tr", { children: [
          /* @__PURE__ */ a.jsxs("td", { children: [
            R.descripcion,
            /* @__PURE__ */ a.jsx(ua, { conceptos: R.conceptos })
          ] }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: $e(R.cantidad) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: $e(R.cantidadServida) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: R.pendienteServir > 0 ? /* @__PURE__ */ a.jsx("strong", { children: $e(R.pendienteServir) }) : "—" }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: z(R.precioUnitario) }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: R.porcentajeDescuento ? `${we(R.porcentajeDescuento)} %` : "" }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: /* @__PURE__ */ a.jsx("strong", { children: z(R.base) }) })
        ] }, R.id)) })
      ] }),
      /* @__PURE__ */ a.jsx("div", { className: "dx-totales-vista", children: /* @__PURE__ */ a.jsxs("div", { className: "dx-tot dx-grande", children: [
        /* @__PURE__ */ a.jsx("span", { children: "Total (sin impuestos)" }),
        /* @__PURE__ */ a.jsx("span", { children: z(l.total) })
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
        /* @__PURE__ */ a.jsx("tbody", { children: s.map((R) => /* @__PURE__ */ a.jsxs("tr", { children: [
          /* @__PURE__ */ a.jsxs("td", { className: "mono", children: [
            /* @__PURE__ */ a.jsx("strong", { children: R.numeroCompleto }),
            " ",
            R.anulado && /* @__PURE__ */ a.jsx("span", { className: "pill neg", title: R.motivoAnulacion ?? "", children: "Anulado" })
          ] }),
          /* @__PURE__ */ a.jsx("td", { children: je(R.fecha) }),
          /* @__PURE__ */ a.jsx("td", { className: "muted", children: R.referencia }),
          /* @__PURE__ */ a.jsx("td", { className: "muted", children: R.lineas.map((Q) => `${$e(Q.cantidad)} × ${Q.descripcion}`).join(" · ") }),
          /* @__PURE__ */ a.jsx("td", { className: "right", children: !R.anulado && l.estado !== "Facturado" && /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", onClick: async () => {
            const Q = window.prompt("Motivo de la anulación del albarán:");
            Q !== null && await lt(() => t.post(`/pedidos-venta/${l.id}/albaranes/${R.id}/anular`, { motivo: Q || null }), n.aviso, "Albarán anulado.") && o();
          }, children: "Anular" }) })
        ] }, R.id)) })
      ] }) : /* @__PURE__ */ a.jsx("p", { className: "muted", style: { margin: 0 }, children: "Sin entregas todavía." })
    ] }),
    c && /* @__PURE__ */ a.jsxs(
      nn,
      {
        titulo: "Entrega (albarán de venta)",
        alCerrar: () => h(null),
        ancho: 640,
        acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => h(null), children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: async () => await lt(() => t.post(`/pedidos-venta/${l.id}/entregar`, { fecha: S, referencia: x || null, lineas: Object.entries(c).filter(([, R]) => R > 0).map(([R, Q]) => ({ lineaPedidoId: R, cantidad: Q })) }), n.aviso, "Albarán creado.") && (h(null), o()), children: "Crear albarán" })
        ] }),
        children: [
          /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Fecha" }),
              /* @__PURE__ */ a.jsx("input", { type: "date", value: S, onChange: (R) => M(R.target.value) })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Referencia" }),
              /* @__PURE__ */ a.jsx("input", { value: x, onChange: (R) => y(R.target.value) })
            ] })
          ] }),
          /* @__PURE__ */ a.jsxs("table", { style: { marginTop: 10 }, children: [
            /* @__PURE__ */ a.jsx("thead", { children: /* @__PURE__ */ a.jsxs("tr", { children: [
              /* @__PURE__ */ a.jsx("th", { children: "Línea" }),
              /* @__PURE__ */ a.jsx("th", { className: "num", children: "Pendiente" }),
              /* @__PURE__ */ a.jsx("th", { className: "num", style: { width: 120 }, children: "Entregar" })
            ] }) }),
            /* @__PURE__ */ a.jsx("tbody", { children: l.lineas.filter((R) => R.pendienteServir > 0).map((R) => /* @__PURE__ */ a.jsxs("tr", { children: [
              /* @__PURE__ */ a.jsx("td", { children: R.descripcion }),
              /* @__PURE__ */ a.jsx("td", { className: "num", children: $e(R.pendienteServir) }),
              /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx("input", { className: "num", type: "number", step: "0.001", value: c[R.id] ?? 0, onChange: (Q) => h({ ...c, [R.id]: Number(Q.target.value) }) }) })
            ] }, R.id)) })
          ] })
        ]
      }
    ),
    p && /* @__PURE__ */ a.jsxs(
      nn,
      {
        titulo: `Facturar el pedido ${l.numeroCompleto}`,
        alCerrar: () => f(!1),
        acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => f(!1), children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: async () => {
            try {
              const R = await t.post(`/pedidos-venta/${l.id}/facturar`, { fechaEmision: v, formaPagoId: k || null });
              n.aviso("Factura emitida.", "ok"), r({ tipo: "factura", pantalla: "vista", id: R.id });
            } catch (R) {
              n.aviso(R.message, "err");
            }
          }, children: "Emitir factura" })
        ] }),
        children: [
          /* @__PURE__ */ a.jsx("p", { className: "muted", style: { marginTop: 0 }, children: "Se factura el pedido completo con sus precios y conceptos. La factura queda numerada y encadenada en VeriFactu." }),
          /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Fecha de emisión" }),
              /* @__PURE__ */ a.jsx("input", { type: "date", value: v, onChange: (R) => C(R.target.value) })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Forma de pago" }),
              /* @__PURE__ */ a.jsxs("select", { value: k, onChange: (R) => P(R.target.value), children: [
                /* @__PURE__ */ a.jsx("option", { value: "", children: "La del cliente" }),
                d.map((R) => /* @__PURE__ */ a.jsx("option", { value: R.id, children: R.nombre }, R.id))
              ] })
            ] })
          ] })
        ]
      }
    )
  ] });
}
function pm(e) {
  const { api: t, anfitrion: n, navegar: r } = kt(), { dato: l, error: i, recargar: o } = da(() => t.get(`/compras/pedidos/${e.id}`)), [s, u] = g.useState([]), [d, N] = g.useState([]), [c, h] = g.useState([]), [x, y] = g.useState(null), [S, M] = g.useState(""), [p, f] = g.useState(""), [v, C] = g.useState(xt()), [k, P] = g.useState(!1), [T, D] = g.useState("IVA21"), [O, U] = g.useState(0), [R, Q] = g.useState(""), [se, Pe] = g.useState(xt());
  if (g.useEffect(() => void t.get(`/compras/pedidos/${e.id}/albaranes`).then(u).catch(() => u([])), [t, e.id, l]), g.useEffect(() => {
    t.get("/inventario/almacenes").then((j) => (N(j), j[0] && M(j[0].id))).catch(() => N([])), t.get("/tipos-iva").then((j) => h(j.filter((w) => w.activo))).catch(() => h([]));
  }, [t]), i) return /* @__PURE__ */ a.jsx("div", { className: "panel", children: /* @__PURE__ */ a.jsx("p", { className: "dx-rojo", children: i }) });
  if (!l) return /* @__PURE__ */ a.jsx("div", { className: "muted", children: "Cargando…" });
  const De = (l.estado === "Borrador" || l.estado === "Confirmado") && l.lineas.every((j) => j.cantidadRecibida === 0 && j.cantidadFacturada === 0) && !l.empresaOrigenId, fe = l.estado !== "Cancelado" && l.estado !== "Facturado", xe = l.lineas.some((j) => j.pendienteRecibir > 0), E = l.lineas.reduce((j, w) => j + w.costeConceptos, 0);
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
            l.estado === "Borrador" && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: async () => await lt(() => t.post(`/compras/pedidos/${l.id}/confirmar`), n.aviso, "Pedido confirmado.") && o(), children: "Confirmar" }),
            fe && l.estado !== "Borrador" && xe && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => y(Object.fromEntries(l.lineas.map((j) => [j.id, { cantidad: j.pendienteRecibir, lote: "" }]))), children: "Recibir mercancía" }),
            fe && l.estado !== "Borrador" && /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: () => P(!0), children: "Facturar" }),
            fe && !l.empresaOrigenId && /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", onClick: async () => window.confirm("¿Cancelar el pedido?") && await lt(() => t.post(`/compras/pedidos/${l.id}/cancelar`), n.aviso, "Pedido cancelado.") && o(), children: "Cancelar" })
          ] })
        }
      ),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ a.jsx(ze, { etiqueta: "Proveedor", children: /* @__PURE__ */ a.jsx("strong", { children: l.proveedorTexto }) }),
        /* @__PURE__ */ a.jsx(ze, { etiqueta: "Fecha", children: je(l.fecha) }),
        /* @__PURE__ */ a.jsx(ze, { etiqueta: "Total", children: z(l.total) }),
        /* @__PURE__ */ a.jsx(ze, { etiqueta: "Costes añadidos", children: E ? z(E) : "—" })
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
        /* @__PURE__ */ a.jsx("td", { className: "num", children: z(j.precioUnitario) }),
        /* @__PURE__ */ a.jsx("td", { className: "num", children: /* @__PURE__ */ a.jsx("strong", { children: z(j.importe) }) }),
        /* @__PURE__ */ a.jsxs("td", { className: "num muted", children: [
          z(j.costeUnitarioEntrada),
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
          var w;
          return /* @__PURE__ */ a.jsxs("tr", { children: [
            /* @__PURE__ */ a.jsxs("td", { className: "mono", children: [
              /* @__PURE__ */ a.jsx("strong", { children: j.numeroCompleto }),
              " ",
              j.anulado && /* @__PURE__ */ a.jsx("span", { className: "pill neg", title: j.motivoAnulacion ?? "", children: "Anulado" })
            ] }),
            /* @__PURE__ */ a.jsx("td", { children: je(j.fecha) }),
            /* @__PURE__ */ a.jsx("td", { className: "muted", children: j.referencia }),
            /* @__PURE__ */ a.jsx("td", { className: "muted", children: ((w = d.find((b) => b.id === j.almacenId)) == null ? void 0 : w.nombre) ?? "—" }),
            /* @__PURE__ */ a.jsx("td", { className: "muted", children: j.lineas.map((b) => `${$e(b.cantidad)} × ${b.descripcion}`).join(" · ") }),
            /* @__PURE__ */ a.jsx("td", { className: "right", children: !j.anulado && l.estado !== "Facturado" && /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", onClick: async () => {
              const b = window.prompt("Motivo de la anulación del albarán:");
              b !== null && await lt(() => t.post(`/compras/pedidos/${l.id}/albaranes/${j.id}/anular`, { motivo: b || null }), n.aviso, "Albarán anulado.") && o();
            }, children: "Anular" }) })
          ] }, j.id);
        }) })
      ] }) : /* @__PURE__ */ a.jsx("p", { className: "muted", style: { margin: 0 }, children: "Sin recepciones todavía." })
    ] }),
    x && /* @__PURE__ */ a.jsxs(
      nn,
      {
        titulo: "Recepción de mercancía",
        alCerrar: () => y(null),
        ancho: 680,
        acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => y(null), children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: async () => await lt(() => t.post(`/compras/pedidos/${l.id}/recibir`, { fecha: v, referencia: p || null, almacenId: S || null, lineas: Object.entries(x).filter(([, j]) => j.cantidad > 0).map(([j, w]) => ({ lineaPedidoId: j, cantidad: w.cantidad, lote: w.lote || null })) }), n.aviso, "Recepción registrada.") && (y(null), o()), children: "Registrar recepción" })
        ] }),
        children: [
          /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Fecha" }),
              /* @__PURE__ */ a.jsx("input", { type: "date", value: v, onChange: (j) => C(j.target.value) })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Albarán del proveedor" }),
              /* @__PURE__ */ a.jsx("input", { value: p, onChange: (j) => f(j.target.value) })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Almacén" }),
              /* @__PURE__ */ a.jsxs("select", { value: S, onChange: (j) => M(j.target.value), children: [
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
              var w, b;
              return /* @__PURE__ */ a.jsxs("tr", { children: [
                /* @__PURE__ */ a.jsx("td", { children: j.descripcion }),
                /* @__PURE__ */ a.jsx("td", { className: "num", children: $e(j.pendienteRecibir) }),
                /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx("input", { className: "num", type: "number", step: "0.001", value: ((w = x[j.id]) == null ? void 0 : w.cantidad) ?? 0, onChange: (W) => y({ ...x, [j.id]: { ...x[j.id], cantidad: Number(W.target.value) } }) }) }),
                /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx("input", { value: ((b = x[j.id]) == null ? void 0 : b.lote) ?? "", onChange: (W) => y({ ...x, [j.id]: { ...x[j.id], lote: W.target.value } }) }) })
              ] }, j.id);
            }) })
          ] })
        ]
      }
    ),
    k && /* @__PURE__ */ a.jsxs(
      nn,
      {
        titulo: `Facturar el pedido ${l.numeroCompleto}`,
        alCerrar: () => P(!1),
        acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => P(!1), children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: async () => await lt(() => t.post(`/compras/pedidos/${l.id}/facturar`, { codigoIva: T, porcentajeIrpf: O, numeroFactura: R || null, fechaFactura: se }), n.aviso, "Factura del proveedor registrada como gasto.") && (P(!1), o()), children: "Registrar factura" })
        ] }),
        children: [
          /* @__PURE__ */ a.jsxs("p", { className: "muted", style: { marginTop: 0 }, children: [
            "Registra la factura del proveedor por el total del pedido (",
            z(l.total),
            ") como gasto, con su asiento si la contabilidad es automática."
          ] }),
          /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Nº de factura del proveedor" }),
              /* @__PURE__ */ a.jsx("input", { value: R, onChange: (j) => Q(j.target.value), autoFocus: !0 })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Fecha de la factura" }),
              /* @__PURE__ */ a.jsx("input", { type: "date", value: se, onChange: (j) => Pe(j.target.value) })
            ] })
          ] }),
          /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Impuesto" }),
              /* @__PURE__ */ a.jsx("select", { value: T, onChange: (j) => D(j.target.value), children: c.map((j) => /* @__PURE__ */ a.jsx("option", { value: j.codigo, children: j.nombre }, j.codigo)) })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Retención IRPF %" }),
              /* @__PURE__ */ a.jsx("input", { type: "number", step: "0.01", value: O, onChange: (j) => U(Number(j.target.value)) })
            ] })
          ] })
        ]
      }
    )
  ] });
}
const ba = (e = "") => ({ clave: Qr(), descripcion: "", cuentaGasto: "", base: 0, codigoIva: e, porcentajeIva: null, porcentajeDeducible: 100 }), mm = (e) => e === "InversionSujetoPasivo" || e === "Intracomunitario";
function hm(e) {
  const { api: t, anfitrion: n } = kt(), r = e.semilla, [l, i] = g.useState([]), [o, s] = g.useState([]), [u, d] = g.useState([]), [N, c] = g.useState([]), [h, x] = g.useState((r == null ? void 0 : r.proveedorId) ?? ""), [y, S] = g.useState(e.id ? (r == null ? void 0 : r.numeroFactura) ?? "" : ""), [M, p] = g.useState((r == null ? void 0 : r.fechaFactura) ?? xt()), [f, v] = g.useState(e.id ? (r == null ? void 0 : r.fecha) ?? xt() : xt()), [C, k] = g.useState(r != null && r.concepto && !r.concepto.startsWith("Factura ") ? r.concepto : ""), [P, T] = g.useState((r == null ? void 0 : r.porcentajeIrpf) ?? 0), [D, O] = g.useState(""), [U, R] = g.useState(((r == null ? void 0 : r.recargoTotal) ?? 0) > 0), Q = !!(r != null && r.esRectificativa), [se, Pe] = g.useState((r == null ? void 0 : r.numeroRectificado) ?? ""), [De, fe] = g.useState((r == null ? void 0 : r.fechaRectificada) ?? ""), [xe, E] = g.useState((r == null ? void 0 : r.motivoRectificacion) ?? ""), [j, w] = g.useState(!1), [b, W] = g.useState((r == null ? void 0 : r.afectacion) ?? "Comun"), [q, pe] = g.useState(
    () => {
      var m;
      return (m = r == null ? void 0 : r.lineas) != null && m.length ? r.lineas.map((L) => ({ clave: Qr(), descripcion: L.descripcion ?? "", cuentaGasto: L.cuentaGasto ?? "", base: L.base, codigoIva: L.codigoIva, porcentajeIva: L.autoliquidada ? L.porcentajeIva : null, porcentajeDeducible: L.porcentajeDeducible })) : [ba()];
    }
  ), [me, te] = g.useState(e.id && (r != null && r.vencimientos) && r.vencimientos.length > 1 ? r.vencimientos : null), [A, ut] = g.useState(null), [Ke, ct] = g.useState(""), [Ct, Ot] = g.useState(!1), wn = Gr();
  g.useEffect(() => {
    t.get("/proveedores").then(i).catch(() => i([])), t.get("/tipos-iva").then((m) => s(m.filter((L) => L.activo))).catch(() => s([])), t.get("/formas-pago").then((m) => d(m.filter((L) => L.activo))).catch(() => d([])), t.get("/empresas/actual").then((m) => {
      m.regimenIva === "RecargoEquivalencia" && (w(!0), r || R(!0));
    }).catch(() => {
    }), t.get("/contabilidad/cuentas").then((m) => c(m.filter((L) => L.codigo.startsWith("6") || L.codigo.startsWith("2")))).catch(() => c([]));
  }, [t]);
  const ie = l.find((m) => m.id === h);
  g.useEffect(() => {
    ie != null && ie.formaPagoDefectoId && !D && O(ie.formaPagoDefectoId);
  }, [ie]);
  const K = g.useMemo(
    () => ({
      proveedorId: h || null,
      proveedorTexto: (ie == null ? void 0 : ie.nombre) ?? null,
      numeroFactura: y.trim() || null,
      fechaFactura: M || null,
      fecha: f,
      concepto: C.trim() || null,
      porcentajeIrpf: P,
      formaPagoId: D || null,
      recargoEquivalencia: U,
      afectacion: b,
      baseImponible: 0,
      lineas: q.filter((m) => m.base !== 0).map((m) => ({
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
      fechaRectificada: Q && De || null,
      motivoRectificacion: Q && xe.trim() || null
    }),
    [h, ie, y, M, f, C, P, D, U, b, q, me, Q, r, se, De, xe]
  ), qe = hn(K, 350);
  g.useEffect(() => {
    if (!qe.lineas.length) {
      ut(null), ct("Añade al menos una línea con base.");
      return;
    }
    const m = wn();
    t.post("/gastos/simular", qe).then((L) => m() && (ut(L), ct(""))).catch((L) => m() && (ut(null), ct(L.message)));
  }, [qe, t]);
  const Fe = (m, L) => pe((F) => F.map((V) => V.clave === m ? { ...V, ...L } : V)), Et = (m) => o.find((L) => L.codigo === m), It = (m) => {
    var L;
    return (L = A == null ? void 0 : A.lineas) == null ? void 0 : L[q.filter((F) => F.base !== 0).indexOf(m)];
  };
  function Ve(m) {
    if (!A) return;
    const L = /* @__PURE__ */ new Date((M || f) + "T00:00:00"), F = Oe(A.total / m);
    te(Array.from({ length: m }, (V, H) => {
      const J = new Date(L);
      return J.setMonth(J.getMonth() + H + 1), { fecha: J.toISOString().slice(0, 10), importe: H === m - 1 ? Oe(A.total - F * (m - 1)) : F };
    }));
  }
  async function tr() {
    Ot(!0);
    try {
      const m = e.id ? await t.put(`/gastos/${e.id}`, K) : await t.post("/gastos", K);
      n.aviso(e.id ? "Factura corregida." : "Factura registrada.", "ok"), m.avisoRiesgo && n.aviso(m.avisoRiesgo, "err"), e.alGuardar(m.id);
    } catch (m) {
      n.aviso(m.message, "err");
    } finally {
      Ot(!1);
    }
  }
  const on = Oe((me ?? []).reduce((m, L) => m + (Number(L.importe) || 0), 0));
  return /* @__PURE__ */ a.jsxs("div", { className: "dx-editor", children: [
    /* @__PURE__ */ a.jsxs("div", { className: "panel", children: [
      /* @__PURE__ */ a.jsxs("div", { className: "panel-head", children: [
        /* @__PURE__ */ a.jsx("h2", { children: e.id ? `Corregir factura ${(r == null ? void 0 : r.numeroFactura) ?? ""}` : Q ? "Rectificativa / abono del proveedor" : "Nueva factura de proveedor" }),
        /* @__PURE__ */ a.jsxs("div", { style: { display: "flex", gap: 8 }, children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: e.alCancelar, children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", disabled: !A || Ct, onClick: tr, children: e.id ? "Guardar corrección" : Q ? "Registrar abono" : "Registrar factura" })
        ] })
      ] }),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-cabecera", children: [
        /* @__PURE__ */ a.jsxs("div", { className: "dx-cab-campos", children: [
          /* @__PURE__ */ a.jsx(To, { terceros: l, valor: h, alCambiar: x, etiqueta: "Proveedor" }),
          /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Nº de factura del proveedor" }),
              /* @__PURE__ */ a.jsx("input", { value: y, onChange: (m) => S(m.target.value), placeholder: "F-2026/0117" })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Fecha de la factura" }),
              /* @__PURE__ */ a.jsx("input", { type: "date", value: M, onChange: (m) => p(m.target.value) })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Fecha de registro" }),
              /* @__PURE__ */ a.jsx("input", { type: "date", value: f, onChange: (m) => v(m.target.value), title: "La del asiento y la del periodo de IVA en que se deduce" })
            ] })
          ] }),
          /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Forma de pago" }),
              /* @__PURE__ */ a.jsxs("select", { value: D, onChange: (m) => O(m.target.value), children: [
                /* @__PURE__ */ a.jsx("option", { value: "", children: "Contado (vence en la fecha de factura)" }),
                u.map((m) => /* @__PURE__ */ a.jsx("option", { value: m.id, children: m.nombre }, m.id))
              ] })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Retención IRPF %" }),
              /* @__PURE__ */ a.jsx("input", { type: "number", step: "0.01", value: P, onChange: (m) => T(Number(m.target.value)) })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Prorrata especial" }),
              /* @__PURE__ */ a.jsxs("select", { value: b, onChange: (m) => W(m.target.value), children: [
                /* @__PURE__ */ a.jsx("option", { value: "Comun", children: "Uso común" }),
                /* @__PURE__ */ a.jsx("option", { value: "ConDerecho", children: "Solo operaciones con derecho" }),
                /* @__PURE__ */ a.jsx("option", { value: "SinDerecho", children: "Solo operaciones exentas" })
              ] })
            ] })
          ] }),
          /* @__PURE__ */ a.jsx("div", { className: "dx-fila", children: /* @__PURE__ */ a.jsxs("div", { style: { gridColumn: "1 / -1" }, children: [
            /* @__PURE__ */ a.jsx("label", { children: "Concepto (vacío: «Factura nº»)" }),
            /* @__PURE__ */ a.jsx("input", { value: C, onChange: (m) => k(m.target.value) })
          ] }) }),
          Q && /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", children: [
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Factura que rectifica" }),
              /* @__PURE__ */ a.jsx("input", { value: se, disabled: !!(r != null && r.rectificaGastoId), onChange: (m) => Pe(m.target.value) })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Fecha de la rectificada" }),
              /* @__PURE__ */ a.jsx("input", { type: "date", value: De, disabled: !!(r != null && r.rectificaGastoId), onChange: (m) => fe(m.target.value) })
            ] }),
            /* @__PURE__ */ a.jsxs("div", { children: [
              /* @__PURE__ */ a.jsx("label", { children: "Motivo" }),
              /* @__PURE__ */ a.jsx("input", { value: xe, onChange: (m) => E(m.target.value), placeholder: "Devolución, descuento posterior, error de precio…" })
            ] })
          ] }),
          Q && /* @__PURE__ */ a.jsx("div", { className: "muted", children: "Las bases del abono van en negativo; el asiento y el SII (R1, por diferencias) salen con el signo." }),
          /* @__PURE__ */ a.jsxs("label", { className: "dx-check", children: [
            /* @__PURE__ */ a.jsx("input", { type: "checkbox", checked: U, onChange: (m) => R(m.target.checked) }),
            " El proveedor me cobra recargo de equivalencia"
          ] }),
          j && /* @__PURE__ */ a.jsx("div", { className: "muted", children: "La empresa está en recargo de equivalencia: el IVA y el recargo soportados son coste (no se deducen)." })
        ] }),
        /* @__PURE__ */ a.jsx("div", { className: "dx-ficha", children: ie ? /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("strong", { children: ie.nombre }),
          /* @__PURE__ */ a.jsx("div", { className: "muted", children: [ie.nifFiscal, ie.poblacion, ie.pais].filter(Boolean).join(" · ") }),
          !ie.nifFiscal && /* @__PURE__ */ a.jsx("div", { className: "dx-aviso", children: "⚠ Sin NIF en la ficha: hace falta para el libro de IVA, el 347 y el SII." }),
          (A == null ? void 0 : A.avisoRiesgo) && /* @__PURE__ */ a.jsxs("div", { className: "dx-aviso", children: [
            "⚠ ",
            A.avisoRiesgo
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
        /* @__PURE__ */ a.jsx("tbody", { children: q.map((m, L) => {
          const F = Et(m.codigoIva), V = It(m);
          return /* @__PURE__ */ a.jsxs("tr", { className: L % 2 ? "dx-par" : "", children: [
            /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx("input", { value: m.descripcion, onChange: (H) => Fe(m.clave, { descripcion: H.target.value }), placeholder: "Qué se compra" }) }),
            /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx("input", { list: "dx-cuentas-gasto", value: m.cuentaGasto, onChange: (H) => Fe(m.clave, { cuentaGasto: H.target.value }), placeholder: "la de la regla" }) }),
            /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx("input", { className: "num", type: "number", step: "0.01", value: m.base || "", onChange: (H) => Fe(m.clave, { base: Number(H.target.value) }) }) }),
            /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsxs("select", { value: m.codigoIva, onChange: (H) => Fe(m.clave, { codigoIva: H.target.value, porcentajeIva: null }), children: [
              /* @__PURE__ */ a.jsx("option", { value: "", children: "General" }),
              o.map((H) => /* @__PURE__ */ a.jsx("option", { value: H.codigo, children: H.nombre }, H.codigo))
            ] }) }),
            /* @__PURE__ */ a.jsx("td", { children: mm(F == null ? void 0 : F.clase) ? /* @__PURE__ */ a.jsx("input", { className: "num", type: "number", step: "0.01", value: m.porcentajeIva ?? "", placeholder: "21", title: "Tipo que autoliquidas", onChange: (H) => Fe(m.clave, { porcentajeIva: H.target.value === "" ? null : Number(H.target.value) }) }) : /* @__PURE__ */ a.jsx("span", { className: "muted", children: V ? `${we(V.porcentajeIva)} %` : "" }) }),
            /* @__PURE__ */ a.jsx("td", { children: /* @__PURE__ */ a.jsx("input", { className: "num", type: "number", step: "1", min: 0, max: 100, value: m.porcentajeDeducible, onChange: (H) => Fe(m.clave, { porcentajeDeducible: Number(H.target.value) }) }) }),
            /* @__PURE__ */ a.jsx("td", { className: "num", children: V ? /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
              /* @__PURE__ */ a.jsx("strong", { children: z(V.cuota) }),
              V.autoliquidada && /* @__PURE__ */ a.jsx("div", { className: "dx-sub", children: "autoliquidada" }),
              V.cuotaRecargo !== 0 && /* @__PURE__ */ a.jsxs("div", { className: "dx-sub", children: [
                "+ recargo ",
                z(V.cuotaRecargo)
              ] })
            ] }) : "—" }),
            /* @__PURE__ */ a.jsx("td", { className: "right", children: /* @__PURE__ */ a.jsx("button", { className: "dx-icono", title: "Quitar", onClick: () => pe((H) => H.length > 1 ? H.filter((J) => J.clave !== m.clave) : [ba()]), children: "✕" }) })
          ] }, m.clave);
        }) })
      ] }),
      /* @__PURE__ */ a.jsx("datalist", { id: "dx-cuentas-gasto", children: N.map((m) => /* @__PURE__ */ a.jsx("option", { value: m.codigo, children: m.nombre }, m.codigo)) }),
      /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", style: { marginTop: 8 }, onClick: () => pe((m) => {
        var L;
        return [...m, ba(((L = m[m.length - 1]) == null ? void 0 : L.codigoIva) ?? "")];
      }), children: "+ Añadir línea" }),
      /* @__PURE__ */ a.jsx("span", { className: "muted dx-ayuda", children: "Una línea por cada base e impuesto de la factura. En inversión del sujeto pasivo e intracomunitarias el IVA lo autoliquidas tú (no lo cobra el proveedor)." })
    ] }),
    /* @__PURE__ */ a.jsxs("div", { className: "dx-pie", children: [
      /* @__PURE__ */ a.jsxs("div", { className: "panel", style: { margin: 0 }, children: [
        /* @__PURE__ */ a.jsxs("div", { className: "panel-head", children: [
          /* @__PURE__ */ a.jsx("h2", { children: "Vencimientos" }),
          /* @__PURE__ */ a.jsxs("div", { className: "dx-acciones", children: [
            [2, 3, 4].map((m) => /* @__PURE__ */ a.jsxs("button", { className: "btn small secondary", disabled: !A, onClick: () => Ve(m), children: [
              m,
              " plazos"
            ] }, m)),
            me && /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", onClick: () => te(null), children: "Según forma de pago" })
          ] })
        ] }),
        me ? /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          me.map((m, L) => /* @__PURE__ */ a.jsxs("div", { className: "dx-fila", style: { marginBottom: 6 }, children: [
            /* @__PURE__ */ a.jsx("input", { type: "date", value: m.fecha, onChange: (F) => te(me.map((V, H) => H === L ? { ...V, fecha: F.target.value } : V)) }),
            /* @__PURE__ */ a.jsx("input", { className: "num", type: "number", step: "0.01", value: m.importe, onChange: (F) => te(me.map((V, H) => H === L ? { ...V, importe: Number(F.target.value) } : V)) })
          ] }, L)),
          A && on !== A.total && /* @__PURE__ */ a.jsxs("p", { className: "dx-rojo", style: { margin: 0 }, children: [
            "Los plazos suman ",
            z(on),
            "; la factura, ",
            z(A.total),
            "."
          ] })
        ] }) : /* @__PURE__ */ a.jsx("p", { className: "muted", style: { margin: 0 }, children: ((A == null ? void 0 : A.vencimientos) ?? []).map((m) => `${je(m.fecha)}: ${z(m.importe)}`).join(" · ") || "—" }),
        Ke && /* @__PURE__ */ a.jsx("p", { className: "dx-rojo", style: { marginBottom: 0 }, children: Ke })
      ] }),
      /* @__PURE__ */ a.jsxs("div", { className: "panel dx-totales", children: [
        ((A == null ? void 0 : A.desglose) ?? []).map((m, L) => {
          var F;
          return /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
            /* @__PURE__ */ a.jsxs("span", { className: "muted", children: [
              ((F = Et(m.codigoIva)) == null ? void 0 : F.nombre) ?? m.codigoIva,
              " ",
              m.autoliquidada ? `(${we(m.porcentajeIva)} %, autoliquidado)` : "",
              " · base ",
              we(m.base)
            ] }),
            /* @__PURE__ */ a.jsx("span", { children: z(m.cuota) })
          ] }, L);
        }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Base imponible" }),
          /* @__PURE__ */ a.jsx("span", { children: z(A == null ? void 0 : A.baseImponible) })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Impuestos" }),
          /* @__PURE__ */ a.jsx("span", { children: z(A == null ? void 0 : A.cuotaIva) })
        ] }),
        !!(A != null && A.recargoTotal) && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Recargo de equivalencia" }),
          /* @__PURE__ */ a.jsx("span", { children: z(A.recargoTotal) })
        ] }),
        !!(A != null && A.retencionIrpf) && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Retención IRPF" }),
          /* @__PURE__ */ a.jsxs("span", { children: [
            "−",
            z(A.retencionIrpf)
          ] })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ a.jsx("span", { children: ((A == null ? void 0 : A.total) ?? 0) < 0 ? "A favor (abono del proveedor)" : "Total a pagar" }),
          /* @__PURE__ */ a.jsx("span", { children: z(A == null ? void 0 : A.total) })
        ] }),
        A && (A.desglose ?? []).some((m) => m.cuotaDeducible !== m.cuota) && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "IVA deducible (antes de prorrata)" }),
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: z((A.desglose ?? []).reduce((m, L) => m + L.cuotaDeducible, 0)) })
        ] })
      ] })
    ] })
  ] });
}
function vm(e) {
  const { api: t, anfitrion: n, navegar: r } = kt(), [l, i] = g.useState(null), [o, s] = g.useState(null), [u, d] = g.useState(""), [N, c] = g.useState(!1), h = () => {
    t.get(`/gastos/${e.id}`).then(i).catch((S) => d(S.message)), t.get(`/gastos/${e.id}/saldo`).then(s).catch(() => s(null));
  };
  if (g.useEffect(h, [e.id]), u) return /* @__PURE__ */ a.jsx("div", { className: "panel", children: /* @__PURE__ */ a.jsx("p", { className: "dx-rojo", children: u }) });
  if (!l) return /* @__PURE__ */ a.jsx("div", { className: "muted", children: "Cargando…" });
  const x = l.estado === "Registrado", y = !o || o.liquidado === 0;
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
          x && y && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => r({ tipo: "gasto", pantalla: "editor", id: l.id, semilla: l }), children: "Corregir" }),
          x && o && o.pendiente > 0 && n.irA && /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: () => n.irA("pagos"), children: "Pagar" }),
          x && !l.esRectificativa && /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => {
            var S;
            return r({ tipo: "gasto", pantalla: "editor", semilla: {
              ...l,
              numeroFactura: null,
              esRectificativa: !0,
              rectificaGastoId: l.id,
              numeroRectificado: l.numeroFactura ?? l.concepto,
              fechaRectificada: l.fechaFactura ?? l.fecha,
              motivoRectificacion: "",
              vencimientos: null,
              lineas: (S = l.lineas) == null ? void 0 : S.map((M) => ({ ...M, base: -M.base }))
            } });
          }, children: "Rectificativa / abono" }),
          x && y && /* @__PURE__ */ a.jsx("button", { className: "btn small ghost", onClick: () => c(!0), children: "Anular" })
        ] })
      ] }),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-datos", children: [
        /* @__PURE__ */ a.jsxs("div", { className: "dx-dato", children: [
          /* @__PURE__ */ a.jsx("small", { children: "Proveedor" }),
          /* @__PURE__ */ a.jsx("div", { children: /* @__PURE__ */ a.jsx("strong", { children: l.proveedorTexto }) })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-dato", children: [
          /* @__PURE__ */ a.jsx("small", { children: "Fecha de factura" }),
          /* @__PURE__ */ a.jsx("div", { children: je(l.fechaFactura ?? l.fecha) })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-dato", children: [
          /* @__PURE__ */ a.jsx("small", { children: "Registro" }),
          /* @__PURE__ */ a.jsx("div", { children: je(l.fecha) })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-dato", children: [
          /* @__PURE__ */ a.jsx("small", { children: "Pago" }),
          /* @__PURE__ */ a.jsx("div", { children: o ? o.pendiente <= 0 ? /* @__PURE__ */ a.jsx("span", { className: "pill ok", children: "Pagada" }) : /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
            "Pendiente ",
            /* @__PURE__ */ a.jsx("strong", { children: z(o.pendiente) })
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
        /* @__PURE__ */ a.jsx("tbody", { children: (l.lineas ?? []).map((S, M) => /* @__PURE__ */ a.jsxs("tr", { children: [
          /* @__PURE__ */ a.jsx("td", { children: S.descripcion ?? "" }),
          /* @__PURE__ */ a.jsx("td", { className: "mono muted", children: S.cuentaGasto ?? "regla" }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: z(S.base) }),
          /* @__PURE__ */ a.jsxs("td", { children: [
            S.codigoIva,
            " · ",
            we(S.porcentajeIva),
            " %",
            S.autoliquidada ? " · autoliquidada" : "",
            S.cuotaRecargo ? ` · recargo ${z(S.cuotaRecargo)}` : ""
          ] }),
          /* @__PURE__ */ a.jsx("td", { className: "num", children: z(S.cuota) }),
          /* @__PURE__ */ a.jsxs("td", { className: "num muted", children: [
            S.porcentajeDeducible !== 100 ? `${we(S.porcentajeDeducible)} % · ` : "",
            z(S.cuotaDeducible)
          ] })
        ] }, M)) })
      ] }),
      /* @__PURE__ */ a.jsxs("div", { className: "dx-totales-vista", children: [
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Base imponible" }),
          /* @__PURE__ */ a.jsx("span", { children: z(l.baseImponible) })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Impuestos" }),
          /* @__PURE__ */ a.jsx("span", { children: z(l.cuotaIva) })
        ] }),
        !!l.recargoTotal && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsx("span", { className: "muted", children: "Recargo de equivalencia" }),
          /* @__PURE__ */ a.jsx("span", { children: z(l.recargoTotal) })
        ] }),
        !!l.retencionIrpf && /* @__PURE__ */ a.jsxs("div", { className: "dx-tot", children: [
          /* @__PURE__ */ a.jsxs("span", { className: "muted", children: [
            "Retención IRPF (",
            we(l.porcentajeIrpf),
            " %)"
          ] }),
          /* @__PURE__ */ a.jsxs("span", { children: [
            "−",
            z(l.retencionIrpf)
          ] })
        ] }),
        /* @__PURE__ */ a.jsxs("div", { className: "dx-tot dx-grande", children: [
          /* @__PURE__ */ a.jsx("span", { children: l.total < 0 ? "A favor (abono del proveedor)" : "Total a pagar" }),
          /* @__PURE__ */ a.jsx("span", { children: z(l.total) })
        ] })
      ] }),
      /* @__PURE__ */ a.jsxs("p", { className: "muted", style: { fontSize: 12.5 }, children: [
        "Vencimientos: ",
        (l.vencimientos ?? []).map((S) => `${je(S.fecha)} ${z(S.importe)}`).join(" · ")
      ] })
    ] }),
    N && /* @__PURE__ */ a.jsx(
      nn,
      {
        titulo: "Anular la factura",
        alCerrar: () => c(!1),
        acciones: /* @__PURE__ */ a.jsxs(a.Fragment, { children: [
          /* @__PURE__ */ a.jsx("button", { className: "btn small secondary", onClick: () => c(!1), children: "Cancelar" }),
          /* @__PURE__ */ a.jsx("button", { className: "btn small", onClick: async () => {
            try {
              await t.post(`/gastos/${l.id}/anular`), n.aviso("Factura anulada.", "ok"), c(!1), h();
            } catch (S) {
              n.aviso(S.message, "err");
            }
          }, children: "Anular" })
        ] }),
        children: /* @__PURE__ */ a.jsx("p", { style: { margin: 0 }, children: "Sale de los libros de IVA y de las declaraciones, y se contabiliza su contraasiento. Si solo hay que cambiar algo, mejor «Corregir»." })
      }
    )
  ] });
}
function xm(e) {
  const [t, n] = g.useState(e.inicial), r = g.useRef(0), [l, i] = g.useState(0), o = g.useMemo(() => Kp(e.anfitrion), [e.anfitrion]), s = (c) => {
    n(c), i(++r.current), window.scrollTo({ top: 0 });
  }, u = { api: o, anfitrion: e.anfitrion, ruta: t, navegar: s }, d = `${t.tipo}-${t.pantalla}-${t.id ?? ""}-${l}`;
  let N;
  if (t.pantalla === "lista") N = /* @__PURE__ */ a.jsx(lm, { tipo: t.tipo });
  else if (t.pantalla === "vista" && t.id)
    N = t.tipo === "factura" ? /* @__PURE__ */ a.jsx(cm, { id: t.id }) : t.tipo === "presupuesto" ? /* @__PURE__ */ a.jsx(dm, { id: t.id }) : t.tipo === "pedido" ? /* @__PURE__ */ a.jsx(fm, { id: t.id }) : t.tipo === "gasto" ? /* @__PURE__ */ a.jsx(vm, { id: t.id }) : /* @__PURE__ */ a.jsx(pm, { id: t.id });
  else if (t.tipo === "gasto")
    N = /* @__PURE__ */ a.jsx(
      hm,
      {
        id: t.id,
        semilla: t.semilla,
        alGuardar: (c) => s({ tipo: "gasto", pantalla: "vista", id: c }),
        alCancelar: () => s(t.id ? { tipo: "gasto", pantalla: "vista", id: t.id } : { tipo: "gasto", pantalla: "lista" })
      }
    );
  else if (t.tipo === "compra")
    N = /* @__PURE__ */ a.jsx(
      um,
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
      sm,
      {
        tipo: c,
        id: t.id,
        semilla: t.semilla,
        alGuardar: (h) => s({ tipo: c, pantalla: "vista", id: h }),
        alCancelar: () => s(t.id ? { tipo: c, pantalla: "vista", id: t.id } : { tipo: c, pantalla: "lista" })
      }
    );
  }
  return /* @__PURE__ */ a.jsx(dd.Provider, { value: u, children: /* @__PURE__ */ a.jsx("div", { className: "dx-raiz", children: N }, d) });
}
const gm = `
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
function ym() {
  if (document.getElementById("dx-estilos")) return;
  const e = document.createElement("style");
  e.id = "dx-estilos", e.textContent = gm, document.head.appendChild(e);
}
function jm(e, t, n) {
  ym();
  const r = cd(e);
  return r.render(/* @__PURE__ */ a.jsx(xm, { anfitrion: t, inicial: n })), () => r.unmount();
}
export {
  jm as montar
};
