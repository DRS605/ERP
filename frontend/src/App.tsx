import { Navigate, Route, Routes } from "react-router-dom";
import { useAuth } from "./lib/auth";
import { Shell } from "./components/Shell";
import { Login } from "./pages/Login";
import { Inicio } from "./pages/Inicio";
import { Clientes } from "./pages/Clientes";
import { Actividades } from "./pages/Actividades";
import { Facturas } from "./pages/Facturas";

export function App() {
  const { autenticado } = useAuth();

  return (
    <Routes>
      <Route path="/login" element={autenticado ? <Navigate to="/inicio" replace /> : <Login />} />
      <Route element={autenticado ? <Shell /> : <Navigate to="/login" replace />}>
        <Route path="/inicio" element={<Inicio />} />
        <Route path="/clientes" element={<Clientes />} />
        <Route path="/actividades" element={<Actividades />} />
        <Route path="/facturas" element={<Facturas />} />
      </Route>
      <Route path="*" element={<Navigate to={autenticado ? "/inicio" : "/login"} replace />} />
    </Routes>
  );
}
