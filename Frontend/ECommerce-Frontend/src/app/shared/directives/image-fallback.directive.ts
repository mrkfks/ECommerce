import { Directive, ElementRef, HostListener, Input } from '@angular/core';

@Directive({
  selector: 'img[appImageFallback]',
  standalone: true
})
export class ImageFallbackDirective {
  @Input() appImageFallback: string = 'assets/images/no-image.svg';

  constructor(private el: ElementRef<HTMLImageElement>) { }

  @HostListener('error')
  onError() {
    this.el.nativeElement.src = this.appImageFallback;
  }
}
