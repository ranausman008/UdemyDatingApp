import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
})
export class RegisterComponent implements OnInit {
  @Output() cancelRegister = new EventEmitter();

  model: any = {};

  constructor(
    private accountService: AccountService,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    throw new Error('Method not implemented.');
  }

  Register() {
    this.accountService.Register(this.model).subscribe({
      next: (response) => {
        this.Cancel();
      },
      error: (error) => {
        this.toastr.error(error.error);
      },
    });
  }

  Cancel() {
    this.cancelRegister.emit(false);
  }
}
