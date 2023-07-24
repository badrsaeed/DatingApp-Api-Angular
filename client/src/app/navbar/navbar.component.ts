import { Component, OnInit } from '@angular/core';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.css'],
})
export class NavbarComponent implements OnInit {
  model: any = {};
  loggedIn: boolean = false;
  constructor(private accountService: AccountService) {}
  ngOnInit(): void {
    this.getCurrentUser();
  }
  login() {
    this.accountService.login(this.model).subscribe(
      (response) => {
        console.log(response);
        this.loggedIn = true;
      },
      (error) => {
        console.log(error);
      }
    );
  }
  logout() {
    this.accountService.logout();
    this.loggedIn = false;
  }
  get isLogin() {
    return this.loggedIn;
  }

  getCurrentUser() {
    this.accountService.currentUser$.subscribe(
      (user) => {
        this.loggedIn = !!user;
      },
      (error) => {
        console.log(error);
      }
    );
  }
}
