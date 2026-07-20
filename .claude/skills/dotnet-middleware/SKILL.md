---
name: dotnet-middleware
description: Use when creating, testing, or debugging ASP.NET Core middleware — custom middleware classes, request pipeline ordering (app.Use), conditional branching (app.Map/app.MapWhen), or converting inline middleware to reusable classes.
---

# ASP.NET Core Middleware

## Core pattern
Middleware is a class with:
- A constructor taking `RequestDelegate next`
- An `InvokeAsync(HttpContext context)` method that does work, then calls `await _next(context)`

## Registration order matters
Middleware runs in the order registered in `Program.cs`. Authentication before authorization, 
exception handling first, etc.

## Conventions to follow
- Prefer extension methods (`app.UseMyMiddleware()`) over raw `app.UseMiddleware<T>()` calls
- Short-circuit the pipeline only when necessary (don't call `_next` if you're terminating the request)