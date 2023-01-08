import { Component, OnInit } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { Observable } from 'rxjs';
import { User } from '../_models/user';
import { of } from 'rxjs';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css'],
})
export class NavComponent implements OnInit {
  model: any = {};

  constructor(public accountService: AccountService) {}

  ngOnInit(): void {}

  Login() {
    this.accountService.Login(this.model).subscribe({
      next: (response) => {
        console.log(response);
      },

      error: (error) => console.log(error),
    });
  }

  Logout() {
    this.accountService.Logout();
  }
}
