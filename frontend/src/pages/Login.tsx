import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../lib/auth";
import type { Empresa } from "../lib/tipos";

export function Login() {
  const { entrar, seleccionarEmpresa } = useAuth();
  const navegar = useNavigate();
  const [email, setEmail] = useState("");
  const [contrasena, setContrasena] = useState("");
  const [codigo, setCodigo] = useState("");
  const [pide2fa, setPide2fa] = useState(false);
  const [error, setError] = useState("");
  const [cargando, setCargando] = useState(false);
  const [empresas, setEmpresas] = useState<Empresa[] | null>(null);

  async function enviar(e: React.FormEvent) {
    e.preventDefault();
    setError("");
    setCargando(true);
    try {
      const r = await entrar(email.trim(), contrasena, pide2fa ? codigo.trim() : undefined);
      if (r.requiere2fa) {
        setPide2fa(true);
        setError(pide2fa ? "Código incorrecto. Inténtalo de nuevo." : "Introduce el código de tu app de autenticación.");
      } else if (r.empresas.length === 0) {
        setError("Tu usuario aún no tiene ninguna empresa. Créala en la interfaz clásica.");
      } else if (r.empresas.length === 1) {
        await elegir(r.empresas[0]);
      } else {
        setEmpresas(r.empresas);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "No se pudo iniciar sesión.");
    } finally {
      setCargando(false);
    }
  }

  async function elegir(emp: Empresa) {
    await seleccionarEmpresa(emp);
    navegar("/inicio");
  }

  return (
    <div style={{ minHeight: "100vh", display: "grid", placeItems: "center", padding: 20 }}>
      <div style={{ width: "min(400px, 100%)", background: "var(--surface)", border: "1px solid var(--line)", borderRadius: "var(--radius)", padding: 28, boxShadow: "var(--shadow)" }}>
        <div style={{ display: "flex", alignItems: "center", gap: 12, marginBottom: 8 }}>
          <div style={{ width: 40, height: 40, borderRadius: 12, background: "linear-gradient(120deg, var(--brand1), var(--brand2))", display: "grid", placeItems: "center", color: "#fff", fontWeight: 800 }}>A</div>
          <h1 style={{ margin: 0, fontSize: 22 }}>ALXOR Core</h1>
        </div>

        {empresas ? (
          <>
            <p className="muted" style={{ margin: "8px 0 16px" }}>Elige la empresa con la que quieres trabajar.</p>
            {empresas.map((e) => (
              <button key={e.id} className="btn secondary" style={{ width: "100%", textAlign: "left", marginBottom: 8 }} onClick={() => void elegir(e)}>
                <strong>{e.razonSocial}</strong> <span className="muted mono">· {e.nif}</span>
              </button>
            ))}
          </>
        ) : (
          <form onSubmit={enviar}>
            <p className="muted" style={{ margin: "8px 0 16px" }}>Entra para gestionar tu empresa.</p>
            <label htmlFor="email">Correo electrónico</label>
            <input id="email" type="email" autoComplete="username" value={email} onChange={(e) => setEmail(e.target.value)} required />
            <label htmlFor="pass">Contraseña</label>
            <input id="pass" type="password" autoComplete="current-password" value={contrasena} onChange={(e) => setContrasena(e.target.value)} required />
            {pide2fa && (
              <>
                <label htmlFor="cod">Código de verificación (2FA)</label>
                <input id="cod" inputMode="numeric" autoComplete="one-time-code" placeholder="123456 o código de recuperación" value={codigo} onChange={(e) => setCodigo(e.target.value)} autoFocus />
              </>
            )}
            <button className="btn" style={{ width: "100%", marginTop: 18 }} disabled={cargando}>{cargando ? "Entrando…" : "Entrar"}</button>
            {error && <p style={{ color: "var(--neg)", fontSize: 12.5, marginTop: 12 }}>{error}</p>}
          </form>
        )}
      </div>
    </div>
  );
}
