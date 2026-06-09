import {
  Component, Output, EventEmitter, HostListener, ElementRef, inject, Input,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

interface EmojiCategory {
  id: string;
  label: string;
  title: string;
  emojis: string[];
}

const CATEGORIES: EmojiCategory[] = [
  {
    id: 'smileys', label: '😊', title: 'Smileys',
    emojis: ['😀','😃','😄','😁','😆','😅','🤣','😂','🙂','🙃','😉','😊','😇','🥰','😍','🤩','😘','🥲','😋','😛','😜','🤪','😝','🤑','🤗','🤭','🤫','🤔','😐','😑','😶','😏','😒','🙄','😬','🤥','😌','😔','😪','🤤','😴','🤒','🤕','🥵','🥶','😵','🤯','🤠','🥳','😎','🤓','🧐','😕','🙁','☹️','😮','😲','🥺','😦','😨','😰','😥','😢','😭','😱','😤','😡','😠','🤬'],
  },
  {
    id: 'gestures', label: '👋', title: 'Gestures',
    emojis: ['👋','🤚','🖐','✋','🖖','👌','🤌','🤏','✌️','🤞','🤟','🤘','🤙','👈','👉','👆','👇','☝️','👍','👎','✊','👊','🤛','🤜','👏','🙌','🫶','👐','🤲','🤝','🙏','💪','💅','🫵','🫡','🫳','🫴'],
  },
  {
    id: 'hearts', label: '❤️', title: 'Hearts',
    emojis: ['❤️','🧡','💛','💚','💙','💜','🖤','🤍','🤎','💔','❣️','💕','💞','💓','💗','💖','💘','💝','💟','💯','✅','🎯','⭐','🌟','💫','✨','🔥','🌈','⚡','💥','💢','🎉','🎊','🎈','🏆','🎖','🥇'],
  },
  {
    id: 'people', label: '🧑', title: 'People',
    emojis: ['👶','🧒','👦','👧','🧑','👱','👩','👨','🧔','👴','👵','🧓','👮','👷','💂','🕵','🧑‍⚕️','🧑‍🏫','🧑‍🍳','🧑‍🎨','🧑‍🚀','🧑‍🎤','🧑‍💼','🦸','🦹','🧙','🧝','🧛','🧟','🧞','🧜'],
  },
  {
    id: 'animals', label: '🐶', title: 'Animals',
    emojis: ['🐶','🐱','🐭','🐹','🐰','🦊','🐻','🐼','🐨','🐯','🦁','🐮','🐷','🐸','🐵','🙈','🙉','🙊','🐔','🐧','🐦','🦆','🦅','🦉','🦇','🐺','🐗','🐴','🦄','🐝','🐛','🦋','🐌','🐞','🐜','🐢','🐍','🦎','🦖','🦕','🐙','🦑','🦐','🦞','🦀','🐡','🐟','🐬','🐳','🐋','🦈','🐊'],
  },
  {
    id: 'food', label: '🍕', title: 'Food & Drink',
    emojis: ['🍕','🍔','🌮','🌯','🥗','🍣','🍜','🍝','🍛','🍱','🥘','🍲','🥙','🧆','🥚','🍳','🌭','🍟','🍞','🥐','🧀','🍰','🎂','🧁','🍩','🍪','🍫','🍬','🍭','🍡','🍧','🍨','🍦','🥧','🧃','☕','🍵','🧋','🥤','🍺','🍻','🥂','🍷','🍸','🍹','🧉','🍾'],
  },
  {
    id: 'nature', label: '🌱', title: 'Nature',
    emojis: ['🌱','🌿','☘️','🍀','🌾','🌵','🌲','🌳','🌴','🌸','🌺','🌻','🌹','🌷','🌼','💐','🍁','🍂','🍃','🍄','🌊','💧','🔥','❄️','⛄','🌈','☀️','🌤','⛅','☁️','🌧','⛈','🌩','🌨','🌪','🌀','🌐','🗺️'],
  },
  {
    id: 'activities', label: '⚽', title: 'Activities',
    emojis: ['⚽','🏀','🏈','⚾','🎾','🏐','🏉','🎱','🏓','🏸','🥊','🥋','🏹','🎯','🎣','⛳','🎿','⛷','🏂','🏋','🤸','⛹','🤺','🏇','🏊','🚴','🤾','🏌','🧗','🤼','🤹','🎪','🎨','🎭','🎬','🎤','🎵','🎶','🎸','🎹','🎺','🎻','🥁','🎷','🎮','🕹','🎲','🎯','🎳'],
  },
  {
    id: 'travel', label: '🚀', title: 'Travel',
    emojis: ['🚗','🚕','🚙','🚌','🏎','🚓','🚑','🚒','🚚','🚛','🚜','🏍','🛵','🚲','🛴','✈️','🛸','🚀','🛶','⛵','🛥','🚢','🏠','🏡','🏢','🏥','🏦','🏨','🏪','🏫','🏬','🏭','🏯','🏰','🗼','🗽','⛩️','🧭'],
  },
];

@Component({
  selector: 'app-emoji-picker',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './emoji-picker.component.html',
  styleUrl: './emoji-picker.component.scss',
})
export class EmojiPickerComponent {
  @Input() placement: 'above' | 'below' = 'above';
  @Output() emojiSelected = new EventEmitter<string>();
  @Output() closed = new EventEmitter<void>();

  private readonly elRef = inject(ElementRef);

  readonly categories = CATEGORIES;
  activeCategory = CATEGORIES[0];
  searchTerm = '';

  get filteredEmojis(): string[] {
    if (!this.searchTerm.trim()) return this.activeCategory.emojis;
    const term = this.searchTerm.toLowerCase();
    return CATEGORIES.flatMap(c => c.emojis).filter(e => e.includes(term));
  }

  selectCategory(cat: EmojiCategory): void {
    this.activeCategory = cat;
    this.searchTerm = '';
  }

  select(emoji: string): void {
    this.emojiSelected.emit(emoji);
  }

  @HostListener('document:keydown.escape')
  onEscape(): void { this.closed.emit(); }

  @HostListener('document:click', ['$event'])
  onDocumentClick(e: MouseEvent): void {
    if (!this.elRef.nativeElement.contains(e.target)) {
      this.closed.emit();
    }
  }
}
