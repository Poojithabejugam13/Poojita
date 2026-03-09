import { Component, signal } from '@angular/core';
import { RouterOutlet, Router } from '@angular/router';
import { Navbar } from './core/navbar/navbar';
import { Footer } from './core/footer/footer';
import { SidebarComponent } from './core/sidebar/sidebar';
import { CommonModule } from '@angular/common';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, Navbar, Footer, SidebarComponent, CommonModule],
  templateUrl: './app.html',
  styleUrls: ['./app.css']
})
export class AppComponent {
  title = 'hartford-insurance-web';

  isErrorPage = signal(false);

  constructor(
    public router: Router,
    public authService: AuthService
  ) {
    // Reactive detection of route changes to toggle shell visibility
    this.router.events.subscribe(() => {
      this.isErrorPage.set(this.router.url.includes('/error'));
    });
  }
}
