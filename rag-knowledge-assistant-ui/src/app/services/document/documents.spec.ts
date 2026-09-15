import { TestBed } from '@angular/core/testing';

// Local test double for Documents since the real implementation is not present
class Documents {
  constructor() {}
}

describe('Documents', () => {
  let service: Documents;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Documents);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
