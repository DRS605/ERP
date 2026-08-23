import { createContext, useContext, useMemo, useState, type ReactNode } from "react";
import { api } from "./cliente";
import { sesion } from "./sesion";
import type { Empresa, LoginRespuesta, SeleccionRespuesta, Usuario } from "./tipos";

/** Resultado de un intento de login: o pide 2FA, o entrega la lista de empresas del usuario. */
export interface ResultadoEntrar {
  requiere2fa: boolean;
  empresas: Empresa[];
}

interface EstadoAuth {
  usuario: Usuario | null;
  empresa: Empresa | null;
  autenticado: boolean;
  entrar: (email: string, contrasena: string, codigo?: string) => Promise<ResultadoEntrar>;
  seleccionarEmpresa: (empresa: Empresa) => Promise<void>;
  salir: () => void;
}

const ContextoAuth = createContext<EstadoAuth | null>(null);

export function ProveedorAuth({ children }: { children: ReactNode }) {
  const [usuario, setUsuario] = useState<Usuario | null>(sesion.usuario);
  const [empresa, setEmpresa] = useState<Empresa | null>(sesion.empresa);

  const valor = useMemo<EstadoAuth>(() => {
    async function entrar(email: string, contrasena: string, codigo?: string): Promise<ResultadoEntrar> {
      const login = await api.post<LoginRespuesta>("/auth/login", { email, contrasena, codigo });
      if (login.requiere2fa) {
        return { requiere2fa: true, empresas: [] };
      }

      // Token provisional (sin empresa) para poder listar las empresas del usuario.
      sesion.guardar({ token: login.token, usuario: login.usuario, empresa: { id: "", nif: "", razonSocial: "" } });
      setUsuario(login.usuario);
      return { requiere2fa: false, empresas: await api.get<Empresa[]>("/empresas") };
    }

    async function seleccionarEmpresa(emp: Empresa): Promise<void> {
      const sel = await api.post<SeleccionRespuesta>(`/empresas/${emp.id}/seleccionar`);
      const u = sesion.usuario ?? usuario;
      if (!u) throw new Error("Sesión no iniciada.");
      sesion.guardar({ token: sel.token, usuario: u, empresa: emp });
      setUsuario(u);
      setEmpresa(emp);
    }

    function salir() {
      sesion.limpiar();
      setUsuario(null);
      setEmpresa(null);
    }

    return {
      usuario,
      empresa,
      autenticado: Boolean(usuario && empresa && empresa.id),
      entrar,
      seleccionarEmpresa,
      salir,
    };
  }, [usuario, empresa]);

  return <ContextoAuth.Provider value={valor}>{children}</ContextoAuth.Provider>;
}

export function useAuth(): EstadoAuth {
  const ctx = useContext(ContextoAuth);
  if (!ctx) throw new Error("useAuth debe usarse dentro de ProveedorAuth.");
  return ctx;
}
