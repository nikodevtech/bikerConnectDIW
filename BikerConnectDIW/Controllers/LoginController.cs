﻿using BikerConnectDIW.DTO;
using BikerConnectDIW.Servicios;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BikerConnectDIW.Controllers
{
    public class LoginController : Controller
    {

        private readonly IUsuarioServicio _usuarioServicio;

        public LoginController(IUsuarioServicio usuarioServicio)
        {
            _usuarioServicio = usuarioServicio;
        }

        [HttpGet]
        [Route("/auth/login")]
        public IActionResult Login()
        {
            try
            {
                UsuarioDTO usuarioDTO = new UsuarioDTO();
                return View("~/Views/Home/login.cshtml", usuarioDTO);

            }
            catch (Exception e)
            {
                ViewData["error"] = "Error al procesar la solicitud. Por favor, inténtelo de nuevo.";
                return View("~/Views/Home/login.cshtml");
            }
        }

        [HttpPost]
        [Route("/auth/iniciar-sesion")]
        public IActionResult ProcesarInicioSesion(UsuarioDTO usuarioDTO)
        {
            try
            {
                bool credencialesValidas = _usuarioServicio.verificarCredenciales(usuarioDTO.EmailUsuario, usuarioDTO.ClaveUsuario);

                if (credencialesValidas)
                {
                    UsuarioDTO u = _usuarioServicio.obtenerUsuarioPorEmail(usuarioDTO.EmailUsuario);

                    // crea una identidad de reclamaciones (claims identity) con información del usuario y su rol registrado
                    // de esta manera controlamos que solo los admin puedan acceder a la administracion de usuarios
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, usuarioDTO.EmailUsuario),
                    };
                    if (!string.IsNullOrEmpty(u.Rol))
                    {
                        claims.Add(new Claim(ClaimTypes.Role, u.Rol));
                    }

                    var identidadDeReclamaciones = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    // establece una cookie en el navegador del usuario para mantener la sesión.
                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identidadDeReclamaciones));

                    return RedirectToAction("Dashboard", "Login");
                }
                else
                {
                    ViewData["MensajeErrorInicioSesion"] = "Credenciales inválidas o cuenta no confirmada. Inténtelo de nuevo.";
                    return View("~/Views/Home/login.cshtml");
                }
            }
            catch (Exception e)
            {
                ViewData["error"] = "Error al procesar la solicitud. Por favor, inténtelo de nuevo.";
                return View("~/Views/Home/login.cshtml");
            }
        }



        [HttpGet]
        [Route("/auth/confirmar-cuenta")]
        public IActionResult ConfirmarCuenta([FromQuery] string token)
        {
            try
            {
                bool confirmacionExitosa = _usuarioServicio.confirmarCuenta(token);

                if (confirmacionExitosa)
                {
                    ViewData["CuentaVerificada"] = "Su dirección de correo ha sido confirmada correctamente";
                }
                else
                {
                    ViewData["yaEstabaVerificada"] = "Ya estaba registrado y verificado";
                }

                return View("~/Views/Home/login.cshtml");
            }
            catch (Exception e)
            {
                ViewData["error"] = "Error al procesar la solicitud. Por favor, inténtelo de nuevo.";
                return View("~/Views/Home/login.cshtml");
            }
        }

        [Authorize]
        [HttpGet]
        [Route("/privada/dashboard")]
        public IActionResult Dashboard()
        {
            return View("~/Views/Home/dashboard.cshtml");
        }

        [HttpPost]
        public IActionResult CerrarSesion()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

    }
}
