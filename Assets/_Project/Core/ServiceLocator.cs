using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Core
{
    /// <summary>
    /// Service Locator leggero per la risoluzione di dipendenze tramite interfacce.
    /// Ogni sistema si registra all'avvio e si deregistra alla distruzione.
    /// Uso: ServiceLocator.Register&lt;IMyService&gt;(this);
    ///      ServiceLocator.Get&lt;IMyService&gt;().DoSomething();
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        // ── Registrazione ────────────────────────────────────────────────────

        /// <summary>Registra un servizio. Se esiste già, lo sovrascrive con un warning.</summary>
        public static void Register<T>(T service) where T : class
        {
            var type = typeof(T);
            if (_services.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] Servizio '{type.Name}' già registrato, verrà sovrascritto.");
            }
            _services[type] = service;
        }

        /// <summary>Rimuove la registrazione del servizio dell'interfaccia specificata.</summary>
        public static void Unregister<T>() where T : class
        {
            var type = typeof(T);
            if (!_services.Remove(type))
            {
                Debug.LogWarning($"[ServiceLocator] Tentativo di deregistrare '{type.Name}', ma non era registrato.");
            }
        }

        // ── Risoluzione ──────────────────────────────────────────────────────

        /// <summary>
        /// Restituisce il servizio registrato. Lancia InvalidOperationException se non trovato.
        /// Usare in contesti dove il servizio è garantito.
        /// </summary>
        public static T Get<T>() where T : class
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var service))
            {
                return (T)service;
            }
            throw new InvalidOperationException(
                $"[ServiceLocator] Servizio '{type.Name}' non registrato. " +
                $"Assicurati che il manager sia presente nella scena Bootstrap e si sia inizializzato prima.");
        }

        /// <summary>
        /// Versione sicura: restituisce true e popola 'service' se trovato, false altrimenti.
        /// Usare in contesti opzionali o durante il boot dove l'ordine non è garantito.
        /// </summary>
        public static bool TryGet<T>(out T service) where T : class
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var obj))
            {
                service = (T)obj;
                return true;
            }
            service = null;
            return false;
        }

        /// <summary>Controlla se un servizio è registrato senza risolverlo.</summary>
        public static bool IsRegistered<T>() where T : class
        {
            return _services.ContainsKey(typeof(T));
        }

        // ── Utility ──────────────────────────────────────────────────────────

        /// <summary>
        /// Rimuove tutti i servizi registrati.
        /// Usare solo in fase di test o reset totale dell'applicazione.
        /// </summary>
        public static void Clear()
        {
            _services.Clear();
        }
    }
}
