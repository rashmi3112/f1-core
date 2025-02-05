import { Route } from "@angular/router";
import { LoginComponent } from "./login/login.component";
import { RegisterComponent } from "./register/register.component";

export const accountRoutes: Route[] = [
    { path: 'register', component: RegisterComponent },
]