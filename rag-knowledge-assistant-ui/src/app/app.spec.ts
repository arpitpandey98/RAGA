import { TestBed } from '@angular/core/testing';
import { App } from './app';
import { AuthSession } from './services/auth/auth-session';

class AuthSessionStub {
  initialize(): void {}
}

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [{ provide: AuthSession, useClass: AuthSessionStub }]
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });
});
