import { createContext, useContext, useMemo, useState, type ReactNode } from "react";
import { api } from "./cliente";
import { sesion } from "./sesion";
import type { Empresa, LoginRespuesta, SeleccionRespuesta, Usuario } from "./tipos";

interface EstadoAuth {
  usuario: Usuario | null;
  empresa: Empresa | null;
  autenticado: boolean;
  entrar: (email: string, contrasena: string) => Promise<Empresa[]>;
  seleccionarEmpresa: (empresa: Empresa) => Promise<void>;
  salir: () => void;
}

const ContextoAuth = createContext<EstadoAuth | null>(null);

export function ProveedorAuth({ children }: { children: ReactNode }) {
  const [usuario, setUsuario] = useState<Usuario | null>(sesion.usuario);
  const [empresa, setEmpresa] = useState<Empresa | null>(sesion.empresa);

  const valor = useMemo<EstadoAuth>(() => {
    async function entrar(email: string, contrasena: string): Promise<Empresa[]> {
      const login = await api.post<LoginRespuesta>("/auth/login", { email, contrasena });
      // Token provisional (sin empresa) para poder listar las empresas del usuario.
      sesion.guardar({ token: login.token, usuario: login.usuario, empresa: { id: "", nif: "", razonSocial: "" } });
      setUsuario(login.usuario);
      return api.get<Empresa[]>("/empresas");
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
