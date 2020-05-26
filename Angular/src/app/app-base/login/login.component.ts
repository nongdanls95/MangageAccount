import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

import { AuthService } from '@app-base/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {

  credential: { USERNAME: string, PASSWORD: string } = { USERNAME: '005230', PASSWORD: '1234' };

  constructor(private authService: AuthService, private router: Router) { }

  ngOnInit() {
  }

  login() {
    if (this.credential.USERNAME === 'daibuu002' && this.credential.PASSWORD === 'hocbai123') {
      const redirect = this.authService.redirectUrl ? this.authService.redirectUrl : '/';
      console.log(redirect);
      // Redirect the user
      this.router.navigate([redirect]);
    }
    // this.authService.login(this.credential).subscribe(response => {
    //   if (response.success) {
    //     // Get the redirect URL from our auth service
    //     // If no redirect has been set, use the default
    //     const redirect = this.authService.redirectUrl ? this.authService.redirectUrl : '/';

    //     // Redirect the user
    //     this.router.navigate([redirect]);
    //   }
    // });
  }

}
